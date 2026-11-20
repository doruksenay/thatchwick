using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Economy
{
    public enum MerchantOfferKind { Goods = 0, Breed = 1 }

    /// <summary>Gezgin tüccarın bir malı: uzak diyar malı (pazarda alınmaz) ya da damızlık (soy hayvanı).</summary>
    public sealed class MerchantOffer
    {
        public MerchantOfferKind Kind;
        public string Id;          // eşya ya da hayvan türü
        public int Count;          // kalan adet
        public int Price;          // birim fiyat
    }

    /// <summary>Tüccarın siparişi: bir sonraki gelişinde istenen el ürünü.</summary>
    public sealed class MerchantOrder
    {
        public string ItemId;
        public int Count;
        public int RewardAkce;
        public int RewardReputation;
        public string BonusItemId;
        public long PlacedDay;
    }

    /// <summary>
    /// Gezgin tüccar ve panayır (belge 34-D5, onay 19.11.2026). Pazar günü köye gelir; pazarda bulunmayan mallar
    /// (yalnızca yapılan eşyalar) ve damızlık soy hayvanı (3. kuşak, ürün ve kalite bonuslu) satar, bir sonraki gelişi için
    /// el ürünü siparişi bırakır. Mevsimin ilk pazar günü panayırdır: daha çok mal, iki damızlık, siparişte itibar iki kat.
    /// </summary>
    public sealed class TravelingMerchant
    {
        public const double GoodsMarkup = 1.4, BreedMarkup = 2.0, OrderRewardRatio = 1.6;
        public const int GoodsPerVisit = 3, GoodsPerFair = 5, BreedsPerFair = 2;
        public const int OrderReputation = 5;

        /// <summary>Siparişte istenebilecek el ürünleri (hane yapabilir).</summary>
        public static readonly string[] OrderPool =
        {
            "item.cloth", "item.linen", "item.yarn", "item.felt", "item.cheese", "item.butter", "item.ale", "item.cider",
            "item.candle", "item.tallow_candle", "item.bread", "item.leather", "item.brick", "item.roof_tiles",
            "item.jam_berry", "item.smoked_meat", "item.salted_fish", "item.soap", "item.parchment", "item.dye_red", "item.dye_yellow"
        };

        /// <summary>Damızlık satılabilecek türler.</summary>
        public static readonly string[] BreedPool = { "animal.chicken", "animal.duck", "animal.goat", "animal.sheep", "animal.pig", "animal.cow", "animal.horse" };

        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly Random _rng;
        private readonly List<MerchantOffer> _offers = new List<MerchantOffer>();

        public TravelingMerchant(ContentDatabase content, Household house, int seed = 23)
        {
            _content = content;
            _house = house;
            _rng = new Random(seed);
        }

        public AnimalSystem Animals { get; set; }
        /// <summary>İtibar ekleme (Upbringing.Reputation.Add).</summary>
        public Action<int, string> AddReputation { get; set; }
        /// <summary>Satın alma fiyat çarpanı (ticaret becerisi; Market.TradeBuyFactor).</summary>
        public Func<double> BuyFactor { get; set; }

        public bool Here { get; private set; }
        public bool IsFair { get; private set; }
        public IReadOnlyList<MerchantOffer> Offers => _offers;
        public MerchantOrder Order { get; private set; }
        public int OrdersDelivered { get; private set; }

        /// <summary>Uzak diyar malları: pazarda alınmayan hammadde/ürünler (yalnızca yapılır ya da yalnızca tüccarda: çivit, safran, karabiber). Risaleler Kervan Odası kataloğunda.</summary>
        public List<ItemDef> ExoticGoods()
        {
            var list = new List<ItemDef>();
            foreach (var d in _content.Items.All)
                if (d.Placement == ItemPlacement.Resource && d.Price > 0 && (d.HasTag("craftonly") || d.HasTag("merchantonly"))) list.Add(d);
            return list;
        }

        /// <summary>Tüccar geldi: mallar yeniden dizilir, sipariş yoksa yenisi bırakılır.</summary>
        public void Arrive(long day, bool fair)
        {
            Here = true;
            IsFair = fair;
            _offers.Clear();
            var goods = ExoticGoods();
            int n = Math.Min(goods.Count, fair ? GoodsPerFair : GoodsPerVisit);
            for (int i = 0; i < n; i++)
            {
                int k = _rng.Next(goods.Count);
                var d = goods[k];
                goods.RemoveAt(k);
                _offers.Add(new MerchantOffer { Kind = MerchantOfferKind.Goods, Id = d.Id, Count = 1 + _rng.Next(3), Price = (int)Math.Ceiling(d.Price * GoodsMarkup) });
            }
            int breeds = fair ? BreedsPerFair : (_rng.NextDouble() < 0.3 ? 1 : 0);
            var pool = new List<string>();
            foreach (var id in BreedPool) if (_content.Animals.TryGet(id, out _)) pool.Add(id);
            for (int i = 0; i < breeds && pool.Count > 0; i++)
            {
                int k = _rng.Next(pool.Count);
                var sp = _content.Animals.Get(pool[k]);
                pool.RemoveAt(k);
                _offers.Add(new MerchantOffer { Kind = MerchantOfferKind.Breed, Id = sp.Id, Count = 1, Price = (int)Math.Ceiling(sp.Price * BreedMarkup) });
            }
            if (Order == null) PlaceOrder(day);
        }

        public void Leave()
        {
            Here = false;
            IsFair = false;
            _offers.Clear();
        }

        public MerchantOrder PlaceOrder(long day)
        {
            var pool = new List<string>();
            foreach (var id in OrderPool) if (!ContentPacks.IsHiddenAlcohol(id) && _content.Items.TryGet(id, out var d) && d.Price > 0) pool.Add(id);
            if (pool.Count == 0) return null;
            var item = _content.Items.Get(pool[_rng.Next(pool.Count)]);
            int count = Math.Max(3, Math.Min(12, 120 / Math.Max(1, item.Price)));
            var exotic = ExoticGoods();
            Order = new MerchantOrder
            {
                ItemId = item.Id, Count = count, PlacedDay = day,
                RewardAkce = (int)Math.Ceiling(item.Price * count * OrderRewardRatio),
                RewardReputation = OrderReputation,
                BonusItemId = exotic.Count > 0 ? exotic[_rng.Next(exotic.Count)].Id : null
            };
            return Order;
        }

        public int UnitPrice(MerchantOffer o) => Math.Max(1, (int)Math.Ceiling(o.Price * (BuyFactor != null ? BuyFactor() : 1.0)));

        /// <summary>Bir birim alır. Damızlık hayvan barınakta yer ister; 3. kuşak (soy hayvanı) olarak gelir.</summary>
        public PlacementCheck Buy(MerchantOffer o)
        {
            if (!Here) return PlacementCheck.Fail("Tüccar köyde değil.");
            if (o == null || o.Count <= 0 || !_offers.Contains(o)) return PlacementCheck.Fail("Bu mal kalmadı.");
            int price = UnitPrice(o);
            if (_house.Akce < price) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (o.Kind == MerchantOfferKind.Breed)
            {
                if (Animals == null) return PlacementCheck.Fail("Hayvan alınamıyor.");
                int before = _house.Akce;
                var r = Animals.Buy(o.Id, _house, out var animal);
                if (!r.Ok) return r;
                int paid = before - _house.Akce;   // pazar fiyatı ödendi; farkı tüccara
                _house.TrySpend(Math.Max(0, price - paid));   // başta tam fiyat kontrol edildi, fark ödenebilir
                animal.Generation = Math.Max(animal.Generation, AnimalSpeciesDef.HeritageGeneration);
                // Soy kademesi (belge 29): tüccarın damızlığı İyi soy, panayırdaki Soylu. Var olan daha yüksek soy düşmez.
                var lineage = IsFair ? AnimalLineage.Noble : AnimalLineage.Good;
                if (animal.Lineage < lineage) Animals.SetLineage(animal, lineage);
            }
            else
            {
                if (!_house.TrySpend(price)) return PlacementCheck.Fail("Akçe yetmiyor.");
                _house.AddItem(o.Id, 1);
                _house.MarkBought(o.Id, 1);
            }
            o.Count--;
            if (o.Count <= 0) _offers.Remove(o);
            return PlacementCheck.Success;
        }

        public string DeliverBlocker()
        {
            if (!Here) return Localization.CoreText.T("ui.village.merchant_away");
            if (Order == null) return "Sipariş yok.";
            if (_house.Count(Order.ItemId) < Order.Count) return $"{Name(Order.ItemId)} {_house.Count(Order.ItemId)}/{Order.Count}.";
            return null;
        }

        /// <summary>Siparişi teslim eder: akçe, itibar (panayırda iki kat) ve bir uzak diyar malı. Yeni sipariş bırakır.</summary>
        public string Deliver(long day)
        {
            string why = DeliverBlocker();
            if (why != null) return why;
            var o = Order;
            for (int i = 0; i < o.Count; i++) _house.TryTakeItem(o.ItemId);
            _house.AddAkce(o.RewardAkce);
            int rep = o.RewardReputation * (IsFair ? 2 : 1);
            AddReputation?.Invoke(rep, "tüccar siparişi");
            if (o.BonusItemId != null) _house.AddItem(o.BonusItemId, 1);
            OrdersDelivered++;
            Order = null;
            PlaceOrder(day);
            return null;
        }

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        public MerchantSave Export()
        {
            var offers = new List<MerchantOfferSave>();
            foreach (var o in _offers) offers.Add(new MerchantOfferSave { kind = (int)o.Kind, id = o.Id, count = o.Count, price = o.Price });
            return new MerchantSave
            {
                here = Here, fair = IsFair, offers = offers.ToArray(), delivered = OrdersDelivered,
                order = Order == null ? null : new MerchantOrderSave { item = Order.ItemId, count = Order.Count, akce = Order.RewardAkce, rep = Order.RewardReputation, bonus = Order.BonusItemId, day = Order.PlacedDay }
            };
        }

        public void Import(MerchantSave s)
        {
            _offers.Clear();
            Here = s?.here ?? false;
            IsFair = s?.fair ?? false;
            OrdersDelivered = s?.delivered ?? 0;
            if (s?.offers != null)
                foreach (var o in s.offers) if (o != null) _offers.Add(new MerchantOffer { Kind = (MerchantOfferKind)o.kind, Id = o.id, Count = o.count, Price = o.price });
            Order = s?.order == null || string.IsNullOrEmpty(s.order.item) ? null : new MerchantOrder
            {
                ItemId = s.order.item, Count = s.order.count, RewardAkce = s.order.akce, RewardReputation = s.order.rep, BonusItemId = s.order.bonus, PlacedDay = s.order.day
            };
        }
    }

    [Serializable] public sealed class MerchantOfferSave { public int kind, count, price; public string id; }
    [Serializable] public sealed class MerchantOrderSave { public string item, bonus; public int count, akce, rep; public long day; }
    [Serializable] public sealed class MerchantSave { public bool here, fair; public int delivered; public MerchantOfferSave[] offers; public MerchantOrderSave order; }
}
