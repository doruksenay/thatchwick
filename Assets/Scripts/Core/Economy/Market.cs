using System;
using Thatchwick.Core.Characters;
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
        /// <summary>Talep günde bu kadar toparlanır (en çok 1). Belge 47: bir güne iki kat iş sığdığı için 0,15 → 0,25.</summary>
        public double DemandRecoveryPerDay = 0.25;
        public int OffersPerDay = 3;
        /// <summary>Kış talebi: yakıt ve yiyecek daha değerli.</summary>
        public double WinterFuelMultiplier = 1.5, WinterFoodMultiplier = 1.3;
        /// <summary>Köy işi (teslimat) ödülü, malın taban değerinin bu katı: pazarda satmaktan kârlı.</summary>
        public double ContractRewardRatio = 0.9;
        /// <summary>Takas teklifinde tüccarın verdiği değer, aldığının bu katı.</summary>
        public double BarterBonus = 1.15;
        /// <summary>Ham madde doygunluğu (23. belge karar 7): aynı ham maddeden haftada bu kadardan fazla satılınca fiyat
        /// birim başına RawDropPerUnit düşer, en çok RawFloor'a kadar; hafta dönünce sıfırlanır. İşlenmiş üründe düşmez.</summary>
        public int RawWeeklyFree = 30;
        public double RawDropPerUnit = 0.01, RawFloor = 0.8;
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
        /// <summary>Özel sipariş adı ("Usta siparişi: Demircilik"); sıradan teklifte null.</summary>
        public string Label { get; internal set; }
        /// <summary>Teslimde kazanılan itibar (usta siparişi).</summary>
        public int Reputation { get; internal set; }
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

        /// <summary>Satış fiyatına eklenen oran (köy projesi, Tüccar dalı); yoksa 0.</summary>
        public Func<double> SellBonus { get; set; }
        /// <summary>Günlük ek teklif sayısı (pazar günü, şenlik); yoksa 0.</summary>
        public Func<int> ExtraOffers { get; set; }

        /// <summary>Teklif kabul edildiğinde (hesap XP'si, rozet sayaçları için).</summary>
        public event Action<TradeOffer> OfferAccepted;
        /// <summary>Mal satıldı (eşya, satılan adet): çeyiz gibi hanenin hatırlı mallarını izleyenler için.</summary>
        public event Action<string, int> Sold;
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

        /// <summary>Kayıt için talep değerleri (yalnızca 1'den düşük olanlar).</summary>
        public List<KeyValuePair<string, double>> ExportDemand()
        {
            var list = new List<KeyValuePair<string, double>>();
            foreach (var kv in _demand) if (kv.Value < 1.0) list.Add(kv);
            return list;
        }

        public List<int> ExportTakenOffers()
        {
            var list = new List<int>();
            foreach (var o in _offers) if (o.Taken) list.Add(o.Id);
            return list;
        }

        /// <summary>Kayıttan yükleme: talep ve o günün alınmış teklifleri (teklifler gün ve tohumdan yeniden üretilir).</summary>
        public void RestoreState(IEnumerable<KeyValuePair<string, double>> demand, double totalDays, Season season, IEnumerable<int> takenOffers)
        {
            _demand.Clear();
            if (demand != null) foreach (var kv in demand) _demand[kv.Key] = Math.Max(_cfg.MinDemand, Math.Min(1.0, kv.Value));
            _offerDay = -1;
            Tick(0, totalDays, season);
            if (takenOffers == null) return;
            var taken = new HashSet<int>(takenOffers);
            foreach (var o in _offers) if (taken.Contains(o.Id)) o.Taken = true;
        }

        public double Demand(string itemId) => _demand.TryGetValue(itemId, out var d) ? d : 1.0;

        private readonly Dictionary<string, int> _rawSold = new Dictionary<string, int>();
        private long _rawWeek = -1;

        /// <summary>Bu hafta satılan ham madde (doygunluk) çarpanı: 1 ya da RawFloor'a kadar düşük.</summary>
        public double RawSaturation(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var d) || !d.HasTag("raw")) return 1.0;
            int sold = _rawSold.TryGetValue(itemId, out var n) ? n : 0;
            // Toptan satış (Ticaret 6): haftalık serbest miktar 1,5 kat.
            int free = HouseTrade() >= WholesaleLevel ? (int)Math.Round(_cfg.RawWeeklyFree * WholesaleFreeFactor) : _cfg.RawWeeklyFree;
            if (sold <= free) return 1.0;
            return Math.Max(_cfg.RawFloor, 1.0 - _cfg.RawDropPerUnit * (sold - free));
        }

        /// <summary>Ham madde doygunluk sayacı kayda girer (belge 58d M0.5); yoksa yüklemeden sonra hafta sıfırdan başlardı.</summary>
        public List<KeyValuePair<string, int>> ExportRawSold(out long week)
        {
            week = _rawWeek;
            var list = new List<KeyValuePair<string, int>>();
            foreach (var kv in _rawSold) list.Add(kv);
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        public void RestoreRawSold(long week, IEnumerable<KeyValuePair<string, int>> sold)
        {
            _rawSold.Clear();
            _rawWeek = week;
            if (sold != null) foreach (var kv in sold) if (kv.Value > 0) _rawSold[kv.Key] = kv.Value;
        }

        public int RawSoldThisWeek(string itemId) => _rawSold.TryGetValue(itemId, out var n) ? n : 0;

        public double SeasonMultiplier(ItemDef def)
        {
            if (_season != Season.Winter) return 1.0;
            if (def.HasTag("fuel")) return _cfg.WinterFuelMultiplier;
            if (def.HasTag("food")) return _cfg.WinterFoodMultiplier;
            return 1.0;
        }

        /// <summary>Pazarlık eden karakter (komut alan); ticaret becerisi fiyatı iyileştirir ve XP kazanır. null ise etki yok.</summary>
        public Func<Character> Trader { get; set; }

        private int TradeLevel() => Math.Max(1, Trader?.Invoke()?.Skills.GetLevel(SkillType.Trade) ?? 1);

        /// <summary>Ticaret: satış seviye başına %2 pahalı (10. seviyede %18), alış %1 ucuz.</summary>
        public double TradeSellFactor => 1.0 + 0.02 * (TradeLevel() - 1);
        public double TradeBuyFactor => 1.0 - 0.01 * (TradeLevel() - 1);

        private void TradeXp(double xp) { if (xp > 0) Trader?.Invoke()?.GainSkillXp(SkillType.Trade, xp); }

        /// <summary>
        /// Ticaret XP'si (belge 45, 5.3): işlem başına sabit + değerin karekökü. Eski kural (satışta kazancın %30'u)
        /// geç oyunda Ticaret'i en hızlı beceri yapıyordu. 10 akçe ≈ 6,7 XP, 100 ≈ 17, 1000 ≈ 49.
        /// </summary>
        public static double SaleXp(int earned) => earned > 0 ? 2.0 + 1.5 * Math.Sqrt(earned) : 0.0;
        public static double PurchaseXp(int cost) => cost > 0 ? 1.0 + 0.5 * Math.Sqrt(cost) : 0.0;

        /// <summary>Takvim çarpanı (perhiz günlerinde balık pahalı, et ucuz; belge 48 §9.4). null = 1.</summary>
        public Func<ItemDef, double> CalendarFactor { get; set; }
        private double Calendar(ItemDef def) => CalendarFactor != null ? Math.Max(0.1, CalendarFactor(def)) : 1.0;

        public int BuyPrice(string itemId)
        {
            var def = _content.Items.Get(itemId);
            return Math.Max(1, (int)Math.Ceiling(def.Price * SeasonMultiplier(def) * Calendar(def) * TradeBuyFactor * (HaggledToday ? HaggleDiscount : 1.0)));
        }

        // ---------- Ticaret basamakları (belge 45) ----------
        public const int HaggleLevel = 2, YesterdayLevel = 3, StallLevel = 5, WholesaleLevel = 6, BarterLevel = 7, FairLevel = 8;
        public const double HaggleDiscount = 0.9, StallBonus = 0.10, FairBonus = 0.15, WholesaleFreeFactor = 1.5;

        /// <summary>Hanenin en iyi Ticaret seviyesi (tezgâh, toptan, takas); null ise komut alanınki.</summary>
        public Func<int> HouseTradeLevel { get; set; }
        /// <summary>Bugün şenlik (panayır) mı; null ise değil.</summary>
        public Func<bool> IsFairDay { get; set; }
        private int HouseTrade() => Math.Max(TradeLevel(), HouseTradeLevel != null ? HouseTradeLevel() : 1);

        private long _haggleDay = -1, _stallDay = -1;
        private bool _haggleWon;
        public bool HaggledToday => _haggleDay == _offerDay && _haggleWon;
        public bool StallToday => _stallDay == _offerDay;

        /// <summary>Pazarlık et (Ticaret 2): günde bir; tutarsa bugünkü alışlar %10 ucuz. Sonuç metni.</summary>
        public string Haggle()
        {
            int lv = TradeLevel();
            if (lv < HaggleLevel) return $"Pazarlık için Ticaret {HaggleLevel} gerekli.";
            if (_haggleDay == _offerDay) return HaggledToday ? "Bugünkü pazarlık tuttu (alışlar %10 ucuz)." : "Bugün pazarlık edildi; satıcı yumuşamadı.";
            _haggleDay = _offerDay;
            var rng = new Random(unchecked(_seed * 31 + (int)_offerDay * 977));
            _haggleWon = rng.NextDouble() < Math.Min(0.9, 0.35 + 0.05 * lv);
            TradeXp(4);
            return _haggleWon ? "Pazarlık tuttu: bugün alışlar %10 ucuz." : "Satıcı inat etti; fiyat aynı kaldı.";
        }

        /// <summary>Tezgâh kur (Ticaret 5): bugün satışlar %10 pahalı; panayır günü Ticaret 8 ile %15 daha.</summary>
        public string SetUpStall()
        {
            if (HouseTrade() < StallLevel) return $"Tezgâh için Ticaret {StallLevel} gerekli.";
            if (StallToday) return "Tezgâh bugün kurulu.";
            _stallDay = _offerDay;
            TradeXp(6);
            bool fair = IsFairDay != null && IsFairDay() && HouseTrade() >= FairLevel;
            return fair ? "Panayırda tezgâh kuruldu: bugün satışlar %25 pahalı." : "Tezgâh kuruldu: bugün satışlar %10 pahalı.";
        }

        private double StallFactor() => !StallToday ? 1.0 : 1.0 + StallBonus + (IsFairDay != null && IsFairDay() && HouseTrade() >= FairLevel ? FairBonus : 0.0);

        // Dünkü fiyatlar (Ticaret 3): gün değişince bugünkü satış fiyatları dünkü olur.
        private Dictionary<string, int> _yesterday = new Dictionary<string, int>(), _todayPrices = new Dictionary<string, int>();
        /// <summary>Dünkü satış fiyatı (Ticaret 3 yoksa ya da bilinmiyorsa null).</summary>
        public int? YesterdaySellPrice(string itemId) =>
            HouseTrade() >= YesterdayLevel && _yesterday.TryGetValue(itemId, out var p) ? p : (int?)null;

        private void SnapshotPrices()
        {
            _yesterday = _todayPrices;
            _todayPrices = new Dictionary<string, int>();
            foreach (var g in Goods()) _todayPrices[g.Id] = SellPrice(g.Id);
        }

        public int SellPrice(string itemId) => SellPrice(itemId, ItemQuality.Common);

        /// <summary>Kalite başına satış fiyatı (53 §4.5): ×1 / 1,25 / 1,5 / 3 / 4. Talep kimlik başına ortaktır.</summary>
        public int SellPrice(string itemId, int quality)
        {
            var def = _content.Items.Get(itemId);
            double bonus = 1.0 + (SellBonus != null ? Math.Max(0.0, Math.Min(0.75, SellBonus())) : 0.0);
            double q = def.HasQuality ? ItemQuality.PriceFactor(quality) : 1.0;
            return Math.Max(1, (int)Math.Round(def.Price * _cfg.SellRatio * Demand(itemId) * SeasonMultiplier(def) * Calendar(def) * bonus * TradeSellFactor * RawSaturation(itemId) * StallFactor() * q, MidpointRounding.AwayFromZero));
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
            if (day != _offerDay) { RefreshOffers(day); SnapshotPrices(); }
            long week = day / 7;
            if (week != _rawWeek) { _rawWeek = week; _rawSold.Clear(); }
        }

        public PlacementCheck Buy(string itemId, int count = 1)
        {
            if (!IsGood(itemId)) return PlacementCheck.Fail("Pazarda bu yok.");
            if (!IsBuyable(itemId)) return PlacementCheck.Fail("Bu pazarda satılmaz, yalnızca yapılır (sat ama alınmaz).");
            if (count <= 0) return PlacementCheck.Fail("Adet geçersiz.");
            if (_storage != null && !_storage.HasRoomFor(itemId, count)) return PlacementCheck.Fail("Depo dolu.");
            int cost = BuyPrice(itemId) * count;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            _house.AddItem(itemId, count);
            _house.MarkBought(itemId, count);   // pazar malı: "Kendi emeğimiz" eki almaz (27 §9)
            TradeXp(PurchaseXp(cost));
            Trader?.Invoke()?.GainSkillXp(SkillType.Carting, 0.5 * count);    // malı taşımak arabacılık
            return PlacementCheck.Success;
        }

        /// <summary>Malları birer birer satar; her birim talebi düşürdüğü için fiyat azalarak gider. Satılan toplam akçe.</summary>
        public int Sell(string itemId, int count = 1) => Sell(itemId, count, null);

        /// <summary>
        /// Satış (53 §4.7): kalite verilmezse en düşükten başlar (toplu satış); verilirse yalnızca o kalitedekiler satılır
        /// (oyuncunun seçimi). Her adet kendi kalitesinin fiyatını alır.
        /// </summary>
        public int Sell(string itemId, int count, int? quality)
        {
            if (!IsGood(itemId)) return 0;
            int earned = 0, sold = 0;
            for (int i = 0; i < count; i++)
            {
                int q;
                if (quality.HasValue) { if (!_house.TryTakeQuality(itemId, quality.Value)) break; q = quality.Value; }
                else { if (!_house.TryTake(itemId, TakePolicy.Lowest, out var key)) break; q = key.Quality; }
                earned += SellPrice(itemId, q);
                sold++;
                _demand[itemId] = Math.Max(_cfg.MinDemand, Demand(itemId) * (1.0 - _cfg.DemandDropPerUnit));
                if (_content.Items.TryGet(itemId, out var sd) && sd.HasTag("raw")) _rawSold[itemId] = RawSoldThisWeek(itemId) + 1;
            }
            if (earned > 0) _house.AddAkce(earned);
            TradeXp(SaleXp(earned));
            if (sold > 0) Sold?.Invoke(itemId, sold);
            if (earned > 0) Trader?.Invoke()?.GainSkillXp(SkillType.Carting, 0.5 * count);
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
            if (offer.IsContract) _house.AddAkce((int)Math.Round(offer.RewardAkce * (ContractFactor != null ? ContractFactor() : 1.0)));
            else { _house.AddItem(offer.GetItem, offer.GetCount); _house.MarkBought(offer.GetItem, offer.GetCount); }
            offer.Taken = true;
            TradeXp(10 + 1.5 * Math.Sqrt(Math.Max(0, offer.RewardAkce)));
            OfferAccepted?.Invoke(offer);
            return PlacementCheck.Success;
        }

        public string Describe(TradeOffer o) => o.IsContract
            ? $"{o.Label ?? "Köy işi"}: {o.GiveCount} {Name(o.GiveItem)} teslim et → {o.RewardAkce} akçe" + (o.Reputation > 0 ? $", itibar +{o.Reputation}" : "")
            : $"Tüccar: {o.GiveCount} {Name(o.GiveItem)} ver → {o.GetCount} {Name(o.GetItem)} al";

        /// <summary>Pazardan alınabilir mi? "craftonly" etiketli mal (işlenmiş eyer) yalnızca yapılır, satılabilir ama alınmaz.</summary>
        public bool IsBuyable(string id) => IsGood(id) && _content.Items.TryGet(id, out var d) && !d.HasTag("craftonly") && !d.HasTag("merchantonly") && !d.HasTag("wild");   // yaban malı (cam bahçe türleri, zemin) doğadan toplanır   // tüccar malı pazarda alınmaz

        public bool IsGood(string id) => _content.Items.TryGet(id, out var d) && d.Placement == ItemPlacement.Resource && d.Price > 0;
        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        // Gün ve tohuma bağlı, tekrarlanabilir teklifler: yarısı köy işi, yarısı takas.
        private void RefreshOffers(long day)
        {
            _offerDay = day;
            _offers.Clear();
            var goods = Goods();
            if (goods.Count == 0) return;
            var rng = new Random(unchecked(_seed * 7919 + (int)day * 104729));
            int offerCount = _cfg.OffersPerDay + Math.Max(0, ExtraOffers != null ? ExtraOffers() : 0);
            int barterExtra = HouseTrade() >= BarterLevel ? 1 : 0;   // takas anlaşması (Ticaret 7): bir takas fazla
            for (int i = 0; i < offerCount + barterExtra; i++)
            {
                var give = goods[rng.Next(goods.Count)];
                int giveCount = rng.Next(3, 9);
                var offer = new TradeOffer { Id = (int)(day * 100 + i), GiveItem = give.Id, GiveCount = giveCount };
                bool contract = (i % 2 == 0 && i < offerCount) || goods.Count < 2;
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
            // Pazar günü usta siparişleri (belge 45 ortak basamak Sv 10).
            if (SpecialContracts != null)
            {
                int k = 0;
                foreach (var sc in SpecialContracts(day))
                    _offers.Add(new TradeOffer
                    {
                        Id = (int)(day * 100 + 50 + k++), GiveItem = sc.item, GiveCount = sc.count, RewardAkce = sc.reward,
                        Label = sc.label, Reputation = sc.reputation
                    });
            }
        }

        /// <summary>Günün özel siparişleri (usta siparişi); null ise yok.</summary>
        public Func<long, IEnumerable<(string item, int count, int reward, string label, int reputation)>> SpecialContracts { get; set; }

        /// <summary>Köy işi ödül çarpanı (Okuryazarlık 7 "Sözleşme yaz"); null ise 1.</summary>
        public Func<double> ContractFactor { get; set; }
    }
}
