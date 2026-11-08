using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Economy
{
    /// <summary>Pazar ayarları (yer tutucu değerler; Adım 28'de dengelenir).</summary>
    public sealed class MarketConfig
    {
        /// <summary>Satış fiyatı = taban fiyat × bu oran × talep × mevsim çarpanı.</summary>
        public double SellRatio = 0.6;
        /// <summary>Satılan her birim o kaynağın talebini bu oranda düşürür.</summary>
        public double DemandDropPerUnit = 0.05;
        public double MinDemand = 0.3;
        /// <summary>Talep günde bu kadar toparlanır (en çok 1).</summary>
        public double DemandRecoveryPerDay = 0.15;
        public int OffersPerDay = 3;
        /// <summary>Kış talebi: yakıt ve yiyecek daha değerli.</summary>
        public double WinterFuelMultiplier = 1.5, WinterFoodMultiplier = 1.3;
        /// <summary>Köy işi (teslimat) ödülü, malın taban değerinin bu katı: pazarda satmaktan kârlı.</summary>
        public double ContractRewardRatio = 0.9;
        /// <summary>Takas teklifinde tüccarın verdiği değer, aldığının bu katı.</summary>
        public double BarterBonus = 1.15;
    }

    /// <summary>Günlük teklif: köy işi (mal ver, akçe al) ya da tüccar takası (mal ver, mal al).</summary>
    public sealed class TradeOffer
    {
        public int Id { get; internal set; }
        public string GiveItem { get; internal set; }
        public int GiveCount { get; internal set; }
        /// <summary>Takasta alınan mal; köy işinde null.</summary>
        public string GetItem { get; internal set; }
        public int GetCount { get; internal set; }
        public int RewardAkce { get; internal set; }
        public bool Taken { get; internal set; }
        public bool IsContract => GetItem == null;
    }

    /// <summary>
    /// Köy pazarı (T5, T6): kaynak alım satımı, talebe ve mevsime göre değişen fiyat, her gün yenilenen
    /// köy işleri (teslimat karşılığı akçe) ve tüccar takasları. Akçe akışının amacı: günlük işten
    /// gelen akçe yapı moduyla eve dönüşür. Oyuncular arası takas Adım 21'de.
    /// </summary>
    public sealed class Market
    {
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly StorageService _storage;
        private readonly MarketConfig _cfg;
        private readonly int _seed;
        private readonly Dictionary<string, double> _demand = new Dictionary<string, double>();
        private readonly List<TradeOffer> _offers = new List<TradeOffer>();
        private long _offerDay = -1;
        private Season _season;

        public MarketConfig Config => _cfg;
        public IReadOnlyList<TradeOffer> Offers => _offers;
        public Season Season => _season;

        public Market(ContentDatabase content, Household house, StorageService storage = null, MarketConfig config = null, int seed = 1)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _storage = storage;
            _cfg = config ?? new MarketConfig();
            _seed = seed;
        }

        /// <summary>Pazarda işlem gören kaynaklar (fiyatı olan, yerleştirilmeyen mallar).</summary>
        public List<ItemDef> Goods()
        {
            var list = new List<ItemDef>();
            foreach (var d in _content.Items.All)
                if (d.Placement == ItemPlacement.Resource && d.Price > 0) list.Add(d);
            return list;
        }

        public double Demand(string itemId) => _demand.TryGetValue(itemId, out var d) ? d : 1.0;

        public double SeasonMultiplier(ItemDef def)
        {
            if (_season != Season.Winter) return 1.0;
            if (def.HasTag("fuel")) return _cfg.WinterFuelMultiplier;
            if (def.HasTag("food")) return _cfg.WinterFoodMultiplier;
            return 1.0;
        }

        public int BuyPrice(string itemId)
        {
            var def = _content.Items.Get(itemId);
            return Math.Max(1, (int)Math.Ceiling(def.Price * SeasonMultiplier(def)));
        }

        public int SellPrice(string itemId)
        {
            var def = _content.Items.Get(itemId);
            return Math.Max(1, (int)Math.Round(def.Price * _cfg.SellRatio * Demand(itemId) * SeasonMultiplier(def), MidpointRounding.AwayFromZero));
        }

        /// <summary>Gün ilerledikçe talep toparlanır; yeni günde teklifler yenilenir.</summary>
        public void Tick(double gameDays, double totalDays, Season season)
        {
            _season = season;
            if (gameDays > 0)
            {
                var keys = new List<string>(_demand.Keys);
                foreach (var k in keys)
                    _demand[k] = Math.Min(1.0, _demand[k] + _cfg.DemandRecoveryPerDay * gameDays);
            }
            long day = (long)Math.Floor(totalDays);
            if (day != _offerDay) RefreshOffers(day);
        }

        public PlacementCheck Buy(string itemId, int count = 1)
        {
            if (!IsGood(itemId)) return PlacementCheck.Fail("Pazarda bu yok.");
            if (count <= 0) return PlacementCheck.Fail("Adet geçersiz.");
            if (_storage != null && !_storage.HasRoomFor(itemId, count)) return PlacementCheck.Fail("Depo dolu.");
            if (!_house.TrySpend(BuyPrice(itemId) * count)) return PlacementCheck.Fail("Akçe yetmiyor.");
            _house.AddItem(itemId, count);
            return PlacementCheck.Success;
        }

        /// <summary>Malları birer birer satar; her birim talebi düşürdüğü için fiyat azalarak gider. Satılan toplam akçe.</summary>
        public int Sell(string itemId, int count = 1)
        {
            if (!IsGood(itemId)) return 0;
            int earned = 0;
            for (int i = 0; i < count; i++)
            {
                if (!_house.TryTakeItem(itemId)) break;
                earned += SellPrice(itemId);
                _demand[itemId] = Math.Max(_cfg.MinDemand, Demand(itemId) * (1.0 - _cfg.DemandDropPerUnit));
            }
            if (earned > 0) _house.AddAkce(earned);
            return earned;
        }

        public PlacementCheck Accept(int offerId)
        {
            TradeOffer offer = null;
            foreach (var o in _offers) if (o.Id == offerId) offer = o;
            if (offer == null) return PlacementCheck.Fail("Teklif bulunamadı.");
            if (offer.Taken) return PlacementCheck.Fail("Bu teklif bugün alındı.");
            if (_house.Count(offer.GiveItem) < offer.GiveCount) return PlacementCheck.Fail($"{Name(offer.GiveItem)} yetmiyor ({offer.GiveCount} gerekli).");
            if (!offer.IsContract && _storage != null && _storage.Free + offer.GiveCount < offer.GetCount)
                return PlacementCheck.Fail("Depo dolu.");

            for (int i = 0; i < offer.GiveCount; i++) _house.TryTakeItem(offer.GiveItem);
            if (offer.IsContract) _house.AddAkce(offer.RewardAkce);
            else _house.AddItem(offer.GetItem, offer.GetCount);
            offer.Taken = true;
            return PlacementCheck.Success;
        }

        public string Describe(TradeOffer o) => o.IsContract
            ? $"Köy işi: {o.GiveCount} {Name(o.GiveItem)} teslim et → {o.RewardAkce} akçe"
            : $"Tüccar: {o.GiveCount} {Name(o.GiveItem)} ver → {o.GetCount} {Name(o.GetItem)} al";

        private bool IsGood(string id) => _content.Items.TryGet(id, out var d) && d.Placement == ItemPlacement.Resource && d.Price > 0;
        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        // Gün ve tohuma bağlı, tekrarlanabilir teklifler: yarısı köy işi, yarısı takas.
        private void RefreshOffers(long day)
        {
            _offerDay = day;
            _offers.Clear();
            var goods = Goods();
            if (goods.Count == 0) return;
            var rng = new Random(unchecked(_seed * 7919 + (int)day * 104729));
            for (int i = 0; i < _cfg.OffersPerDay; i++)
            {
                var give = goods[rng.Next(goods.Count)];
                int giveCount = rng.Next(3, 9);
                var offer = new TradeOffer { Id = (int)(day * 100 + i), GiveItem = give.Id, GiveCount = giveCount };
                bool contract = i % 2 == 0 || goods.Count < 2;
                if (contract)
                {
                    offer.RewardAkce = Math.Max(1, (int)Math.Ceiling(give.Price * giveCount * _cfg.ContractRewardRatio * SeasonMultiplier(give)));
                }
                else
                {
                    ItemDef get;
                    do get = goods[rng.Next(goods.Count)]; while (get.Id == give.Id);
                    offer.GetItem = get.Id;
                    offer.GetCount = Math.Max(1, (int)Math.Round(give.Price * giveCount * _cfg.BarterBonus / get.Price));
                }
                _offers.Add(offer);
            }
        }
    }
}
