using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Economy
{
    /// <summary>
    /// Dükkânların günlük stoğu (Project belgesi 66e karar 3, 66f §A.4): her mal bir dükkâna aittir (Shops süzgeçleri),
    /// her sabah temel stokla yenilenir. Köylünün alımı da oyuncunun alımı da aynı stoktan düşer: komşu son ekmeği aldıysa
    /// oyuncu o gün bulamaz. Saf mantık; gün dışarıdan verilir. Kayıtta yalnızca bugün satılanlar tutulur (eklemeli alan).
    /// </summary>
    public sealed class ShopStock
    {
        private readonly ContentDatabase _content;
        private readonly Life.VillageLifeConfig _cfg;
        private readonly Dictionary<string, int> _sold = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _shopOf = new Dictionary<string, string>(StringComparer.Ordinal);
        private long _day = -1;

        public ShopStock(ContentDatabase content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _cfg = content.VillageLife;
        }

        /// <summary>Stoğun ait olduğu gün; yeni günde <see cref="Restock"/> satılanları sıfırlar.</summary>
        public long Day => _day;

        /// <summary>Malın satıldığı dükkân (Shops süzgeçleriyle; uzman dükkânlar önce, kalan genel dükkân). Mal değilse null.</summary>
        public string ShopOf(string itemId)
        {
            if (itemId == null) return null;
            if (_shopOf.TryGetValue(itemId, out var known)) return known;
            string shop = null;
            if (_content.Items.TryGet(itemId, out var def))
            {
                foreach (var s in Shops.All)
                {
                    if (s.Id == "general" || s.Goods == null || !s.Goods(def)) continue;
                    shop = s.Id; break;
                }
                if (shop == null)
                {
                    var g = Shops.Get("general");
                    if (g?.Goods != null && g.Goods(def)) shop = g.Id;
                }
            }
            _shopOf[itemId] = shop;
            return shop;
        }

        /// <summary>Malın sabah stoğu: pişmiş yemek, yiyecek, ham madde, tohum ve diğer (life.json villageLife).</summary>
        public int BaseStock(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var d) || d.Price <= 0) return 0;
            if (_cfg.IsStaple(d.Id)) return _cfg.StockStaple;
            if (d.Id.StartsWith("item.seed_", StringComparison.Ordinal) || d.Id.StartsWith("item.sapling_", StringComparison.Ordinal)) return _cfg.StockSeed;
            if (d.HasTag("cooked") || d.HasTag("drink")) return _cfg.StockCooked;
            if (d.HasTag("food")) return _cfg.StockFood;
            if (d.HasTag("raw") || d.HasTag("grain") || d.HasTag("fuel") || d.HasTag("stone") || d.HasTag("fiber")) return _cfg.StockRaw;
            return _cfg.StockOther;
        }

        /// <summary>Yeni gün: stok yenilenir (satılanlar sıfırlanır). Aynı gün ikinci çağrı bir şey yapmaz.</summary>
        public void Restock(long day)
        {
            if (day == _day) return;
            _day = day;
            _sold.Clear();
        }

        public int Sold(string itemId) => itemId != null && _sold.TryGetValue(itemId, out var n) ? n : 0;

        /// <summary>Bugün dükkânda kalan adet.</summary>
        public int Available(string itemId) => Math.Max(0, BaseStock(itemId) - Sold(itemId));

        /// <summary>Stoktan düşer; yetmiyorsa hiçbir şey düşmez ve false döner.</summary>
        public bool TryTake(string itemId, int count)
        {
            if (count <= 0) return true;
            if (Available(itemId) < count) return false;
            _sold[itemId] = Sold(itemId) + count;
            return true;
        }

        /// <summary>Stoktan alabildiği kadarını düşer (köylü alımı); alınan adet.</summary>
        public int TakeUpTo(string itemId, int count)
        {
            int n = Math.Min(Math.Max(0, count), Available(itemId));
            if (n > 0) _sold[itemId] = Sold(itemId) + n;
            return n;
        }

        // ---- kayıt ----

        public ShopStockSave Export()
        {
            var keys = new List<string>(_sold.Keys);
            keys.Sort(StringComparer.Ordinal);
            var counts = new int[keys.Count];
            for (int i = 0; i < keys.Count; i++) counts[i] = _sold[keys[i]];
            return new ShopStockSave { day = _day, items = keys.ToArray(), sold = counts };
        }

        public void Import(ShopStockSave save)
        {
            _sold.Clear();
            _day = -1;
            if (save == null) return;
            _day = save.day;
            if (save.items == null || save.sold == null) return;
            for (int i = 0; i < save.items.Length && i < save.sold.Length; i++)
                if (!string.IsNullOrEmpty(save.items[i]) && save.sold[i] > 0) _sold[save.items[i]] = save.sold[i];
        }
    }

    [Serializable]
    public sealed class ShopStockSave
    {
        public long day = -1;
        public string[] items;
        public int[] sold;
    }
}
