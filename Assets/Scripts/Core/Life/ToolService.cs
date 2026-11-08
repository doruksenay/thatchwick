using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Save;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Aletler (Adım 39): balta, çapa, testere kademe kademe geliştirilir. Kademe 0 temel alettir (hız 1);
    /// her yükseltme akçe + odun ister, hesap seviyesi ve (içerikte bir demirci varsa) yaşayan bir demirci gerektirir.
    /// İş etkileşimi `tool` alanıyla hangi aletin hızını kullanacağını söyler.
    /// </summary>
    public sealed class ToolService
    {
        public const string FuelItem = "item.firewood";

        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly Dictionary<string, int> _tier = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Hesap seviyesi sağlayıcı; null ise sınır yok.</summary>
        public Func<int> AccountLevel { get; set; }
        /// <summary>Demirci var mı; null ise gerekmez.</summary>
        public Func<bool> SmithAvailable { get; set; }

        public event Action<ToolDef, ToolTierDef> Upgraded;

        public ToolService(ContentDatabase content, Household house)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
        }

        public int Tier(string toolId) => toolId != null && _tier.TryGetValue(toolId, out var t) ? t : 0;

        public int MaxTier(string toolId) => _content.Tools.TryGet(toolId, out var d) ? d.Tiers.Count : 0;

        /// <summary>Aletin şu anki hız çarpanı (temel alet 1).</summary>
        public float SpeedFor(string toolId)
        {
            if (string.IsNullOrEmpty(toolId) || !_content.Tools.TryGet(toolId, out var def)) return 1f;
            int t = Math.Min(Tier(toolId), def.Tiers.Count);
            return t <= 0 ? 1f : def.Tiers[t - 1].Speed;
        }

        public string NameOf(string toolId)
        {
            if (!_content.Tools.TryGet(toolId, out var def)) return toolId;
            int t = Math.Min(Tier(toolId), def.Tiers.Count);
            return t <= 0 ? def.DisplayName : def.Tiers[t - 1].Name;
        }

        public ToolTierDef Next(string toolId)
        {
            if (!_content.Tools.TryGet(toolId, out var def)) return null;
            int t = Tier(toolId);
            return t < def.Tiers.Count ? def.Tiers[t] : null;
        }

        public string CanUpgrade(string toolId)
        {
            if (!_content.Tools.TryGet(toolId, out _)) return "Böyle bir alet yok.";
            var next = Next(toolId);
            if (next == null) return "Alet son kademede.";
            if (AccountLevel != null && AccountLevel() < next.Level) return $"Hesap seviyesi {next.Level} gerekir.";
            if (SmithAvailable != null && !SmithAvailable()) return "Köyde demirci yok.";
            if (_house.Akce < next.Akce) return $"Akçe yetmiyor ({next.Akce} gerekir).";
            if (_house.Count(FuelItem) < next.Firewood) return $"Odun yetmiyor ({next.Firewood} gerekir).";
            return null;
        }

        public bool Upgrade(string toolId, out string reason)
        {
            reason = CanUpgrade(toolId);
            if (reason != null) return false;
            var next = Next(toolId);
            _house.TrySpend(next.Akce);
            for (int i = 0; i < next.Firewood; i++) _house.TryTakeItem(FuelItem);
            _tier[toolId] = Tier(toolId) + 1;
            Upgraded?.Invoke(_content.Tools.Get(toolId), next);
            return true;
        }

        public List<ToolTierSave> Export()
        {
            var list = new List<ToolTierSave>();
            foreach (var kv in _tier) if (kv.Value > 0) list.Add(new ToolTierSave { tool = kv.Key, tier = kv.Value });
            list.Sort((a, b) => string.CompareOrdinal(a.tool, b.tool));
            return list;
        }

        public void Restore(IEnumerable<ToolTierSave> saved)
        {
            _tier.Clear();
            if (saved == null) return;
            foreach (var s in saved)
                if (s != null && _content.Tools.Contains(s.tool)) _tier[s.tool] = Math.Max(0, Math.Min(MaxTier(s.tool), s.tier));
        }
    }
}
