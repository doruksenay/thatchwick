using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>Karın ağrısının kaynağı (Z2, Z3).</summary>
    public enum AcheCause { None = 0, DirtyHands = 1, Flies = 2, RawFilth = 3, MurkyWater = 4 }

    /// <summary>
    /// Pisliğin sağlığa ve kilere etkisi (belge 57b §5): kirli el, sinekli sofra, pislikli ham kompostla büyümüş çiğ ürün,
    /// bulanık kuyu ve fare. Karın ağrısı öldürmez; ruh hali ve tuvalet ihtiyacı olarak yaşanır.
    /// </summary>
    public sealed class YardHealth
    {
        /// <summary>Farenin yediği kiler malları (sırayla denenir).</summary>
        public static readonly string[] MouseFood = { "item.wheat", "item.barley", "item.oats", "item.rye", "item.flour", "item.feed", "item.bread" };

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly FilthService _filth;
        private readonly HashSet<int> _dirtyHands = new HashSet<int>();
        private readonly Random _rng;
        private int _tainted, _mice, _cleanDays, _reekDays, _openFoodDays;
        /// <summary>Açıkta yiyecek kalan üst üste gün (fare çağırır).</summary>
        public int OpenFoodDays => _openFoodDays;

        public YardConfig Config => _filth.Config;

        public YardHealth(WorldState world, Household house, FilthService filth, int seed = 573)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _filth = filth ?? throw new ArgumentNullException(nameof(filth));
            _rng = new Random(seed);
        }

        // ---------- kirli el ----------
        public void MarkDirty(int characterId) => _dirtyHands.Add(characterId);
        public void Wash(int characterId) => _dirtyHands.Remove(characterId);
        public bool HasDirtyHands(int characterId) => _dirtyHands.Contains(characterId);

        // ---------- çiğ ürün ----------
        public int Tainted => _tainted;
        public void AddTainted(int count) { if (count > 0) _tainted += count; }

        /// <summary>Yemek yendi: karın ağrısı çıkar mı. Sıra: kirli el, pislikli çiğ ürün, sinekli sofra.</summary>
        public AcheCause OnAte(int characterId, GridPos at)
        {
            if (_dirtyHands.Contains(characterId) && _rng.NextDouble() < Config.DirtyHandsAche) return AcheCause.DirtyHands;
            if (_tainted > 0)
            {
                _tainted--;
                if (_rng.NextDouble() < Config.RawFilthAche) return AcheCause.RawFilth;
            }
            if (_filth.Nearest(at, Config.AppetiteRadius, 1, fliesOnly: true).HasValue && _rng.NextDouble() < Config.FliesAche) return AcheCause.Flies;
            return AcheCause.None;
        }

        // ---------- su (Z3) ----------
        /// <summary>Kuyu bulanık mı: yakınında yeterince pislik ya da dolu hela var.</summary>
        public bool WaterFoul(ItemInstance source)
        {
            if (source == null || !source.Def.WaterSource) return false;
            int n = 0;
            foreach (var s in _filth.Sources)
            {
                if (s.ItemId == source.Id || s.Position.Z != source.Position.Z) continue;
                if (s.Kind != FilthService.PileSourceKind && s.Kind != FilthService.PrivyKind && s.Kind != "barn") continue;
                if (Math.Max(Math.Abs(s.Position.X - source.Position.X), Math.Abs(s.Position.Y - source.Position.Y)) > Config.WaterFoulRadius) continue;
                n += s.Tier >= 2 ? 2 : 1;
            }
            return n >= Config.WaterFoulPiles;
        }

        public bool AnyFoulWater()
        {
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.WaterSource && WaterFoul(it)) return true;
            return false;
        }

        public AcheCause OnDrewWater(ItemInstance source) =>
            WaterFoul(source) && _rng.NextDouble() < Config.MurkyWaterAche ? AcheCause.MurkyWater : AcheCause.None;

        // ---------- fare (Z7) ve avlu temizliği ----------
        public int Mice => _mice;
        /// <summary>Üst üste temiz ve kokulu günler (itibar ve "Tertemiz avlu").</summary>
        public int CleanDays => _cleanDays;
        public int ReekDays => _reekDays;

        /// <summary>
        /// Yeni gün: kokan yığın ya da bayat süprüntü fareyi çağırır; kedi başına bir fare eksilir. Kalan her fare kilerden
        /// bir mal yer. Dönen liste yenen mallardır.
        /// </summary>
        public List<string> OnNewDay(int cats, out int caught, bool openFood = false)
        {
            caught = 0;
            bool refuse = false;
            _openFoodDays = openFood ? _openFoodDays + 1 : 0;
            foreach (var p in _filth.Piles) if (p.Kind == PileKind.Refuse && p.Age >= Config.MiceAttractAge) { refuse = true; break; }
            int reeking = _filth.ReekingCount;
            if (reeking > 0) { _reekDays++; _cleanDays = 0; }
            else { _reekDays = 0; _cleanDays = _filth.Sources.Count == 0 ? _cleanDays + 1 : 0; }

            if (refuse || _reekDays >= 2 || _openFoodDays >= Config.OpenFoodMiceDays) _mice = Math.Min(Config.MiceMax, _mice + 1);
            else if (_mice > 0) _mice--;
            caught = Math.Min(_mice, Math.Max(0, cats));
            _mice -= caught;

            var eaten = new List<string>();
            for (int i = 0; i < _mice; i++)
                foreach (var id in MouseFood)
                    if (_house.TryTakeItem(id)) { eaten.Add(id); break; }
            return eaten;
        }

        // ---------- kayıt ----------
        public YardHealthSave Export()
        {
            var hands = new List<int>(_dirtyHands);
            hands.Sort();
            return new YardHealthSave { dirtyHands = hands.ToArray(), tainted = _tainted, mice = _mice, cleanDays = _cleanDays, reekDays = _reekDays, openFoodDays = _openFoodDays };
        }

        public void Import(YardHealthSave data)
        {
            _dirtyHands.Clear(); _tainted = 0; _mice = 0; _cleanDays = 0; _reekDays = 0; _openFoodDays = 0;
            if (data == null) return;
            _openFoodDays = Math.Max(0, data.openFoodDays);
            if (data.dirtyHands != null) foreach (var id in data.dirtyHands) _dirtyHands.Add(id);
            _tainted = Math.Max(0, data.tainted); _mice = Math.Max(0, data.mice);
            _cleanDays = Math.Max(0, data.cleanDays); _reekDays = Math.Max(0, data.reekDays);
        }
    }
}
