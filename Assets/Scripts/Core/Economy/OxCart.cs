using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Economy
{
    /// <summary>
    /// Öküz arabası (22. belge): haftada bir (yedinci gün sabahı) çeki hayvanı varken envanterdeki fazlayı pazara götürüp
    /// satar. Her maldan Keep kadarı evde kalır; tohum, yakıt, yem, alet malzemesi ("keep" etiketi) taşınmaz. Araba
    /// kapasitesi Arabacılık seviyesiyle artar; en değerli mal önce yüklenir. Pazarın talep düşüşü burada da geçerlidir.
    /// </summary>
    public sealed class OxCart
    {
        public const string CartItem = "item.ox_cart";
        public const int Keep = 20, BaseCapacity = 40, CapacityPerLevel = 4, TripEvery = 7;
        public static readonly string[] NeverSell = { "seed", "fuel", "feed", "keep", "sheaf" };

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly Market _market;
        private readonly ContentDatabase _content;

        /// <summary>Çalışabilir çeki hayvanı sayısı; null ise araba gitmez.</summary>
        public Func<int> DraftAnimals { get; set; }
        /// <summary>Hanenin en iyi arabacısı; null ise kapasite tabandır, XP verilmez.</summary>
        public Func<Character> Carter { get; set; }

        public OxCart(WorldState world, Household house, Market market, ContentDatabase content)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _market = market ?? throw new ArgumentNullException(nameof(market));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public bool HasCart { get { foreach (var it in _world.Items) if (!it.IsDecor && it.Def.Id == CartItem) return true; return false; } }

        public int Capacity
        {
            get
            {
                var c = Carter?.Invoke();
                int lvl = c != null ? c.Skills.GetLevel(SkillType.Carting) : 1;
                return BaseCapacity + CapacityPerLevel * Math.Max(0, lvl - 1);
            }
        }

        /// <summary>Bugün araba gider mi (neden gitmediği, gidecekse null).</summary>
        public string Blocker(long dayIndex)
        {
            if (!HasCart) return "Öküz arabası yok.";
            if ((dayIndex + 1) % TripEvery != 0) return $"Araba {TripEvery - (int)((dayIndex + 1) % TripEvery)} gün sonra pazara iner.";
            if ((DraftAnimals?.Invoke() ?? 0) <= 0) return "Arabayı çekecek hayvan yok (at ya da öküz, tok ve sağlıklı).";
            return null;
        }

        /// <summary>Arabaya yüklenecekler: (mal, adet), en değerli önce, kapasiteye kadar.</summary>
        public List<KeyValuePair<string, int>> Plan()
        {
            var goods = new List<KeyValuePair<string, int>>();
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= Keep || !_content.Items.TryGet(kv.Key, out var d)) continue;
                if (d.Placement != ItemPlacement.Resource || d.Price <= 0) continue;
                bool skip = false;
                foreach (var t in NeverSell) if (d.HasTag(t)) { skip = true; break; }
                if (skip || d.PlantsAs != null) continue;
                goods.Add(new KeyValuePair<string, int>(kv.Key, kv.Value - Keep));
            }
            goods.Sort((a, b) =>
            {
                int pa = _market.SellPrice(a.Key), pb = _market.SellPrice(b.Key);
                return pa != pb ? pb.CompareTo(pa) : string.CompareOrdinal(a.Key, b.Key);
            });
            var plan = new List<KeyValuePair<string, int>>();
            int room = Capacity;
            foreach (var g in goods)
            {
                if (room <= 0) break;
                int n = Math.Min(room, g.Value);
                plan.Add(new KeyValuePair<string, int>(g.Key, n));
                room -= n;
            }
            return plan;
        }

        /// <summary>Gün başı: sırası geldiyse yükler ve satar. Kazanılan akçe ve satılanlar.</summary>
        public int OnNewDay(long dayIndex, out List<KeyValuePair<string, int>> sold)
        {
            sold = new List<KeyValuePair<string, int>>();
            if (Blocker(dayIndex) != null) return 0;
            int earned = 0, units = 0;
            foreach (var kv in Plan())
            {
                int before = _house.Count(kv.Key);
                earned += _market.Sell(kv.Key, kv.Value);
                int n = before - _house.Count(kv.Key);
                if (n > 0) { sold.Add(new KeyValuePair<string, int>(kv.Key, n)); units += n; }
            }
            Carter?.Invoke()?.GainSkillXp(SkillType.Carting, 0.5 * units);
            return earned;
        }
    }
}
