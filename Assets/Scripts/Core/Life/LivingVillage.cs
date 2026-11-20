using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Köylünün ruh hali basamağı (replik, ikram, pazarlık): keyifli / nötr / kasvetli.</summary>
    public enum VillageMood { Gloomy = 0, Neutral = 1, Cheerful = 2 }

    /// <summary>Keyifli ustanın alışverişteki ikramı (belge 66f §B.8): ya küçük bir eşya ya da indirim, her ikisinde balon.</summary>
    public sealed class PurchaseTreat
    {
        /// <summary>İkram edilen eşya; indirimse null.</summary>
        public string ItemId;
        /// <summary>İndirim payı (0,05); eşya ikramında 0.</summary>
        public double Discount;
        /// <summary>Ustanın başının üstündeki balon.</summary>
        public LocText Line;
    }

    /// <summary>Kapıda yazan dükkân kapalılık nedeni.</summary>
    public enum ShopClosure { None, RestDay, Breakdown, Sick, Mourning }

    /// <summary>
    /// Yaşayan köy, parti 1 (Project belgeleri 66d Y1-Y2-Y5, 66e, 66f): köylülerin gerçek ihtiyacı, hane kileri, öğünler,
    /// dükkândan alım (stok düşer), Pazar dinlenme günü, akşam meyhanesi, bunalım ve teselli, ustanın ikramı, iş döngüsü
    /// pozları ve oyuncunun bulunmadığı bölgelerde soyut hesap.
    ///
    /// Program önce gelir: <see cref="PlaceFor"/> köylünün saat saat nerede olacağını söyler (VillagerService/VillageHomes
    /// programı üstüne Pazar, akşam sofrası, akşam meyhanesi, bunalım ve çağrı). Açlık ve sosyal ihtiyaç burada, o yerde
    /// karşılanır (öğün kilerden düşer); enerji, temizlik, tuvalet, ısınma ve eğlence oyuncunun bölgesinde JobRunner'ın
    /// özgür iradesiyle (yalnızca izinli yerlerdeki eşyalar, <see cref="VillagerFreeWill"/>), başka bölgelerde saatlik
    /// ortalamayla karşılanır. Köylü açlıktan ve soğuktan ölmez.
    /// </summary>
    public sealed class LivingVillage
    {
        /// <summary>Simülasyon adımı (oyun günü, ≈7 dakika): öğün, alım ve ortalama ihtiyaç bu sıklıkta hesaplanır.</summary>
        public const double StepDays = 0.005;
        /// <summary>Soyut hesap adımı (oyun günü, 1 saat): oyuncunun bulunmadığı bölgedeki köylüler.</summary>
        public const double AbstractStepDays = 1.0 / 24.0;

        // Gün akışı (gün kesri). Program sabitleri VillagerService'te; burada öğün ve akşam pencereleri.
        public const double BreakfastFrom = 0.15, BreakfastTo = 0.26;
        public const double LunchFrom = 0.47, LunchTo = 0.55;
        public const double DinnerFrom = 0.70, DinnerTo = 0.76;
        public const double EveningFrom = 0.76, EveningTo = 0.88;
        /// <summary>Pazar geç kalkılır: bu saate dek evde.</summary>
        public const double RestDayWake = 0.32;
        /// <summary>Alım (dükkânlar açıldıktan sonra) ve kendi ürününü kilere koyma saati.</summary>
        public const double ShoppingFrom = 0.34, ProduceFrom = 0.22;
        /// <summary>Çağrılan usta bu kadar gün (≈2 saat) dükkânında ya da yardımda kalır.</summary>
        public const double CallDays = 0.09;
        /// <summary>
        /// Uzun süre aç ya da üşüyen köylü (gün) yardım ricası çıkarır. Seyrek (belge 66g karar 4): aynı köylü iki haftada en çok
        /// bir kez, bütün köyde de üç günde en çok bir rica.
        /// </summary>
        public const double HardshipDays = 0.5, HardshipRepeatDays = 14, HardshipVillageGapDays = 3;
        private double _lastHardshipAny = double.MinValue;

        /// <summary>Çalgıcı (belge 66g karar 1): akşam meyhanede çalar, Pazar öğlenden.</summary>
        public const string RoleMusician = "musician";
        /// <summary>Ebe doğuma çağrıldıysa bu kadar gün (doğuma dek) evin önünde bekler.</summary>
        public const double MidwifeCallDays = 1.0;
        /// <summary>Ebe yanındayken doğum: annenin doğum sonrası toparlanmasının bu payı gider (belge 66g karar 2).</summary>
        public const double MidwifeRecoveryShare = 0.5;
        public const string MidwifeBabyMood = "mood.village.midwife_hands", MidwifeMotherMood = "mood.village.midwife_help";
        /// <summary>Ebenin yardıma geldiği yer (oyuncunun evinin önü).</summary>
        public const string FarmhouseKey = "farmhouse";
        /// <summary>Kilerde bir malın tutulabileceği en çok adet (kendi ürünü birikmesin).</summary>
        public const int PantryCap = 12;

        /// <summary>Replik havuzları (ui.village): basamak başına satır sayısı.</summary>
        public static readonly int[] LineCounts = { 6, 5, 6 };   // kasvetli, nötr, keyifli
        public const int TreatRoleLines = 3, TreatAnyLines = 6, SharedLossLines = 3;

        public const string ConsoledMood = "mood.village.consoled";
        public const string SharedLossMood = "mood.village.shared_loss";
        /// <summary>Köylü hastalığı (belge 66g): günde temel olasılık, aç ya da üşümüşse ek; 1-2 gün evde yatar, dükkân kapalı.</summary>
        public const double SickChance = 0.006, SickHardshipChance = 0.08, SickMinDays = 1.0, SickMaxDays = 2.0;
        public const string SickMood = "mood.village.sick";
        /// <summary>Köydeki doğumda ebenin lohusa ziyareti (gün).</summary>
        public const double MidwifeVisitDays = 0.12;
        /// <summary>Evde pişen öğün sahnesi (ocak başında pişirme, sonra sofrada yeme).</summary>
        public const double CookedMealSceneDays = 0.05;

        private sealed class DayState
        {
            public long Day = -1;
            public int Meals;            // 1 kahvaltı, 2 öğle, 4 akşam
            public double HardshipDays;
            public double LastHardshipRequest = double.MinValue;
            public double CalledUntil = -1;
            public string CalledTo;
            public string[] Scene;       // oynayan öğün sahnesinin pozları
            public double SceneFrom, SceneUntil;
        }

        private readonly ContentDatabase _content;
        private readonly Population _population;
        private readonly VillagerService _villagers;
        private readonly VillageHomes _homes;
        private readonly GameClock _clock;
        private readonly Random _rng;
        private readonly Dictionary<string, Dictionary<string, int>> _pantries = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        private readonly Dictionary<string, long> _shoppedDay = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, long> _producedDay = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<int, DayState> _state = new Dictionary<int, DayState>();
        private readonly HashSet<int> _watched = new HashSet<int>();
        private readonly List<Character> _scratch = new List<Character>();
        private double _acc, _abstractAcc;

        public ShopStock Stock { get; }
        public VillageLifeConfig Config => _content.VillageLife;

        /// <summary>Hava ve şenlik (program için); null ise açık hava, şenlik yok.</summary>
        public Func<WeatherType> Weather { get; set; }
        public Func<bool> FestivalToday { get; set; }
        /// <summary>Ağaç ev, aşevi ve sofra yeri (Treehouses.VillagerPlace): programın önüne geçer. Null dönerse yok.</summary>
        public Func<Character, string> TreePlace { get; set; }
        /// <summary>Adlandırılmış yer bu dünyada var mı (ırmak kıyısı, ulu ağaç); null ise yalnızca meydan ve ev.</summary>
        public Func<string, bool> PlaceAvailable { get; set; }
        /// <summary>Köylü oyuncunun bulunduğu bölgede mi (tam simülasyon); null ise herkes tam simülasyonda.</summary>
        public Func<Character, bool> FullSimulation { get; set; }
        /// <summary>Köylü programdaki yerine vardı mı (oyun katmanı); null ise varmış sayılır.</summary>
        public Func<Character, string, bool> AtPlace { get; set; }
        /// <summary>Aşevi (yoksul hane, kiler boş): hayır sofrası.</summary>
        public CharityKitchen Charity { get; set; }
        /// <summary>Hane hafızası ve rica (teselli, yardım ricası).</summary>
        public Thatchwick.Core.Life.Requests.RequestService Requests { get; set; }
        /// <summary>Yakınlık (teselli, ortak kayıp).</summary>
        public FamilyService Family { get; set; }
        /// <summary>Son ölümler (kimlik, ad, ölüm günü): "yakın zamanda kayıp" seçeneği için.</summary>
        public Func<IEnumerable<(int id, string name, double day)>> RecentDeaths { get; set; }
        /// <summary>Metni oyuncunun diline çevirir (iç ses adları); null ise anahtar.</summary>
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";

        /// <summary>Köylünün başının üstünde balon (ikram, teselli, kapı).</summary>
        public event Action<Character, LocText> Bubble;
        /// <summary>Bildirim (dükkân kapandı, açıldı, çağrı).</summary>
        public event Action<LocText, int> Notice;
        /// <summary>Ortak kayıp (oyuncu karakteri, köylü): ilişki sistemi ikisine "Yası ortak" hissini verir (LifeSystems.Village).</summary>
        public event Action<Character, Character> SharedGrief;
        /// <summary>Köylü bir öğün yedi (kişi, öğün): oyun katmanı sahneyi oynatır.</summary>
        public event Action<Character, VillageMeal> MealEaten;

        public LivingVillage(ContentDatabase content, Population population, VillagerService villagers, VillageHomes homes, GameClock clock, ShopStock stock, int seed = 66)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _villagers = villagers ?? throw new ArgumentNullException(nameof(villagers));
            _homes = homes ?? throw new ArgumentNullException(nameof(homes));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Stock = stock ?? new ShopStock(content);
            _rng = new Random(seed);
        }

        // ---- köylü kuralları ----

        /// <summary>
        /// Köylüye gerçek ihtiyaç, ruh hali ve özgür irade (belge 66e karar 1): ihtiyaçlar azalır, ruh hali işler, JobRunner
        /// yalnızca ihtiyaç giderir. Köylü açlıktan ve soğuktan ölmez. Haneye katılan köylü hane gibi yaşar.
        /// </summary>
        public static void ApplyRules(Character c, Jobs.JobRunner runner)
        {
            if (c == null) return;
            c.CanStarve = !c.IsVillager;
            c.CanFreeze = !c.IsVillager;
            c.MoodEnabled = true;
            c.Needs.SetDecayPerDay(NeedType.Hunger, NeedSet.DefaultDecayPerDay[(int)NeedType.Hunger]);
            c.Needs.SetDecayPerDay(NeedType.Energy, NeedSet.DefaultDecayPerDay[(int)NeedType.Energy]);
            if (runner == null) return;
            runner.Autonomous = true;          // komut her zaman önce gelir; kuyruk boşken ihtiyacını kendisi giderir
            runner.NeedsOnly = c.IsVillager;   // köylü: yalnızca ihtiyaç, hanenin envanterine dokunmadan
            if (!c.IsVillager) runner.PlaceFilter = null;
        }

        public static VillageMood MoodTier(Character c)
        {
            if (c == null || !c.MoodEnabled) return VillageMood.Neutral;
            var s = c.Mood.Status;
            return s >= MoodStatus.Good ? VillageMood.Cheerful : s <= MoodStatus.Unhappy ? VillageMood.Gloomy : VillageMood.Neutral;
        }

        /// <summary>Sohbet satırı (belge 66f §B.7): ruh hali basamağının havuzundan.</summary>
        public LocText ChatLine(Character c)
        {
            var tier = MoodTier(c);
            string name = tier == VillageMood.Cheerful ? "cheerful" : tier == VillageMood.Gloomy ? "gloomy" : "neutral";
            int n = 1 + _rng.Next(LineCounts[(int)tier]);
            return LocText.Of("village.line." + name + "." + n);
        }

        private DayState S(Character c)
        {
            if (!_state.TryGetValue(c.Id, out var s)) { s = new DayState(); _state[c.Id] = s; }
            if (s.Day != _clock.DayIndex) { s.Day = _clock.DayIndex; s.Meals = 0; }
            return s;
        }

        private void Watch(Character c)
        {
            if (!_watched.Add(c.Id)) return;
            c.BreakdownEnded += (ch, kind, interrupted) =>
            {
                if (ch.IsDead || !ch.IsVillager) return;
                var shop = ShopOf(ch);
                if (shop != null) Notice?.Invoke(LocText.Of("village.n.reopened", ("name", ch.Name)), ch.Id);
            };
        }

        // ---- program ----

        public VillagerDef DefFor(Character c)
        {
            var def = _villagers.DefOf(c);
            if (def != null) return def;
            // Komşu haneye evlenen ya da orada doğan köylü: eşinin, yoksa annesinin programı (GameRoot ile aynı kural).
            Character anchor = c.SpouseId.HasValue ? _population.Find(c.SpouseId.Value) : null;
            if ((anchor == null || anchor.IsDead || _villagers.DefOf(anchor) == null) && c.BirthMotherId.HasValue)
                anchor = _population.Find(c.BirthMotherId.Value);
            def = anchor != null && !anchor.IsDead ? _villagers.DefOf(anchor) : null;
            if (def == null && anchor != null && !anchor.IsDead && anchor.SpouseId.HasValue)
                def = _villagers.DefOf(_population.Find(anchor.SpouseId.Value));
            return def;
        }

        public string HomeOf(Character c)
        {
            var named = _villagers.DefOf(c);
            if (named != null && !string.IsNullOrEmpty(named.Home)) return named.Home;
            return _homes.HomeOf(c) ?? VillageHomes.InnCommon;
        }

        /// <summary>Köylünün kendi işyeri (adlı köylü); işi yoksa null.</summary>
        public string WorkOf(Character c)
        {
            var named = _villagers.DefOf(c);
            return named != null && !string.IsNullOrEmpty(named.Work) ? named.Work : null;
        }

        private bool Available(string key) => key == "square" || (PlaceAvailable != null && PlaceAvailable(key));

        /// <summary>Bugün dinlenme günü (Pazar) mü.</summary>
        public bool RestDay => ShopDef.IsRestDay(_clock.DayIndex);

        /// <summary>Köylünün şu anki yeri (oyun katmanı yürütür, köy hayatı orada öğün ve ihtiyaç hesaplar).</summary>
        public string PlaceFor(Character c) => PlaceFor(c, _clock.DayFraction, Weather != null ? Weather() : WeatherType.Clear, FestivalToday != null && FestivalToday());

        public string PlaceFor(Character c, double frac, WeatherType weather, bool festival)
        {
            if (c == null || c.IsDead || !c.IsVillager) return null;
            string home = HomeOf(c);
            if (c.InBreakdown || InMourning(c) || IsSick(c)) return home;   // bunalım, hastalık ve yas günü: evde, dükkân kapalı
            var st = S(c);
            if (st.CalledTo != null && _clock.TotalDays < st.CalledUntil) return st.CalledTo;
            var tree = TreePlace?.Invoke(c);
            if (tree != null) return tree;
            var def = DefFor(c);
            string baseLoc = def != null ? _villagers.LocationFor(def, frac, weather, festival) : _homes.LocationFor(c, frac, weather, festival);
            if (festival || weather == WeatherType.Storm) return baseLoc;
            bool wet = weather == WeatherType.Rain || weather == WeatherType.Snow;
            string work = WorkOf(c);
            bool innkeeper = def != null && def.Role == VillagerService.RoleInnkeeper && work != null && work == def.Work && _villagers.DefOf(c) != null;
            bool adult = c.Stage >= AgeStage.YoungAdult;
            int salt = Hash(c.Id, _clock.DayIndex);

            // Pazar (belge 66e §3): dükkânlar kapalı, köylüler geç kalkar; meydan, ırmak kıyısı, ulu ağaç ya da ev.
            if (RestDay)
            {
                if (innkeeper) return baseLoc;                       // meyhane öğlenden açık
                if (def != null && def.Role == RoleMusician && frac >= VillagerService.InnOpens && frac < 0.95 && TavernOpen()) return VillagerFreeWill.Tavern;
                if (frac < RestDayWake || frac >= VillagerService.SquareUntil) return home;
                if (frac >= DinnerFrom && frac < DinnerTo) return home;
                if (wet) return adult && frac >= VillagerService.InnOpens && salt % 3 == 0 && TavernOpen() ? VillagerFreeWill.Tavern : home;
                int pick = (salt / 7 + (frac < 0.55 ? 0 : 1)) % 5;
                if (pick == 1 && Available(VillagerFreeWill.Riverside)) return VillagerFreeWill.Riverside;
                if (pick == 2 && Available(VillagerFreeWill.GreatTree)) return VillagerFreeWill.GreatTree;
                if (pick == 3) return home;
                if (pick == 4 && adult && frac >= VillagerService.InnOpens && TavernOpen()) return VillagerFreeWill.Tavern;
                return "square";
            }

            if (innkeeper) return baseLoc;
            // Çalgıcı: gündüz evinde ya da meydanda, akşam meyhanede çalar.
            if (def != null && def.Role == RoleMusician && _villagers.DefOf(c) != null && frac >= DinnerFrom && frac < 0.95 && TavernOpen()) return VillagerFreeWill.Tavern;
            // Öğle: işyerindeki usta bazı günler meyhanede yer; ihtiyacı kritikse öğle arasını öne alır (belge 66f §A.2).
            if (work != null && baseLoc == work && adult)
            {
                bool lunchedToday = (st.Meals & 2) != 0;
                bool critical = Critical(c);
                bool lunchTime = frac >= LunchFrom && frac < LunchTo;
                if (!lunchedToday && (lunchTime || critical) && salt % 3 == 0 && TavernOpen()) return VillagerFreeWill.Tavern;
                return baseLoc;
            }
            // Akşam sofrası evde, hane birlikte.
            if (frac >= DinnerFrom && frac < DinnerTo) return home;
            // Akşam: yetişkinlerin bir kısmı (huy ve Eğlence ihtiyacına göre) meyhaneye gider.
            if (adult && frac >= EveningFrom && frac < EveningTo && TavernOpen() && GoesToTavern(c, salt)) return VillagerFreeWill.Tavern;
            return baseLoc;
        }

        private bool Critical(Character c)
        {
            double t = Config.CriticalNeed;
            return c.Needs.Get(NeedType.Hunger) <= t || c.Needs.Get(NeedType.Energy) <= t || c.Needs.Get(NeedType.Bladder) <= t;
        }

        /// <summary>Akşam meyhanesi: Eğlence ihtiyacı düştükçe, keyifli ve sosyal huylarda daha olası.</summary>
        public bool GoesToTavern(Character c, int salt)
        {
            double share = Config.EveningTavernShare;
            float fun = c.Needs.Get(NeedType.Fun);
            if (fun < 50f) share += 0.2;
            if (c.HasTrait("trait.cheerful") || c.HasTrait("trait.sociable")) share += 0.15;
            if (c.HasTrait("trait.introvert") || c.HasTrait("trait.gloomy")) share -= 0.2;
            return (salt % 100) / 100.0 < share;
        }

        /// <summary>Meyhane açık mı (hancı varsa ve saati uygunsa).</summary>
        public bool TavernOpen()
        {
            var tavern = Shops.Get("tavern");
            if (tavern == null || !_villagers.HasRole(VillagerService.RoleInnkeeper)) return false;
            var keeper = OwnerOf(tavern);
            if (keeper != null && ClosureOf(tavern, keeper) != ShopClosure.None) return false;   // hancı bunalımda, hasta ya da yasta
            return tavern.IsOpenAt(_clock.DayFraction * 24.0, _clock.DayIndex);
        }

        private static int Hash(int id, long day) => (int)((((uint)id * 2654435761u) ^ (uint)(day * 40503)) % 100000u);

        // ---- dükkân ----

        /// <summary>Köylünün dükkânı (adlı köylünün işyeri); yoksa null.</summary>
        public ShopDef ShopOf(Character c)
        {
            var w = WorkOf(c);
            if (w == null) return null;
            foreach (var s in Shops.All) if (s.WorkKey == w) return s;
            return null;
        }

        /// <summary>Dükkânın sahibi: işyeri dükkânın anahtarı olan yaşayan adlı köylü; yoksa null.</summary>
        public Character OwnerOf(ShopDef shop)
        {
            if (shop == null) return null;
            foreach (var st in _villagers.States)
            {
                if (!_content.Villagers.TryGet(st.DefId, out var d) || d.Work != shop.WorkKey) continue;
                var c = _population.Find(st.CharacterId);
                if (c != null && !c.IsDead) return c;
            }
            return null;
        }

        /// <summary>Dükkânın kapalılık nedeni (kapıda yazar). Saat dışı kapalılık burada sayılmaz (ShopDef.IsOpenAt).</summary>
        public ShopClosure ClosureOf(ShopDef shop, Character owner)
        {
            if (shop == null) return ShopClosure.None;
            if (owner != null && !owner.IsDead)
            {
                if (owner.InBreakdown) return ShopClosure.Breakdown;
                if (owner.Injury != null || IsSick(owner)) return ShopClosure.Sick;
                if (_mourningUntil.TryGetValue(owner.Id, out var until) && _clock.TotalDays < until) return ShopClosure.Mourning;
            }
            if (ShopDef.IsRestDay(_clock.DayIndex) && !shop.OpenOnRestDay && !IsCalled(owner)) return ShopClosure.RestDay;
            return ShopClosure.None;
        }

        /// <summary>Yas günü (belge 66f §A.5): hanesinden ya da yakın akrabasından biri ölen usta ertesi günün sonuna dek dükkânı açmaz.</summary>
        private readonly Dictionary<int, double> _mourningUntil = new Dictionary<int, double>();

        /// <summary>Köylü öldü: aynı evdeki ve yakın akrabası olan köylüler yas günü tutar.</summary>
        public void OnDeath(Character dead)
        {
            if (dead == null) return;
            string home = dead.IsVillager ? HomeOf(dead) : null;
            double until = Math.Floor(_clock.TotalDays) + 2.0;
            foreach (var c in _population.Members)
            {
                if (c == dead || c.IsDead || !c.IsVillager) continue;
                if ((home != null && HomeOf(c) == home) || c.SpouseId == dead.Id || dead.SpouseId == c.Id || FamilyService.AreCloseRelatives(c, dead)) _mourningUntil[c.Id] = until;
            }
        }

        public bool InMourning(Character c) => c != null && _mourningUntil.TryGetValue(c.Id, out var u) && _clock.TotalDays < u;

        public static LocText ClosureText(ShopClosure c)
        {
            switch (c)
            {
                case ShopClosure.Breakdown: return LocText.Of("village.closed.breakdown");
                case ShopClosure.Sick: return LocText.Of("village.closed.sick");
                case ShopClosure.Mourning: return LocText.Of("village.closed.mourning");
                case ShopClosure.RestDay: return LocText.Of("village.closed.rest_day");
                default: return LocText.Of("village.closed.none");
            }
        }

        /// <summary>Dükkân şu an açık mı: saat, Pazar (çağrılmadıysa), ustanın bunalımı, hastalığı ya da yası.</summary>
        public bool IsShopOpen(ShopDef shop, Character owner)
        {
            if (shop == null) return false;
            var closure = ClosureOf(shop, owner);
            if (closure != ShopClosure.None) return false;
            if (IsCalled(owner)) return true;   // çağrılan usta kapalı saatte de açar
            return shop.IsOpenAt(_clock.DayFraction * 24.0, _clock.DayIndex);
        }

        // ---- çağırma (belge 66e karar 9) ----

        public bool IsCalled(Character c) => c != null && _state.TryGetValue(c.Id, out var s) && s.CalledTo != null && _clock.TotalDays < s.CalledUntil;

        /// <summary>
        /// Pazar ya da kapalı saatte oyuncu ustanın kapısını çalar. Usta evdeyse gelir: dükkânı oyuncu için açar ya da
        /// doğrudan yardıma gelir (<paramref name="helpAt"/>: ebe doğuma). Evde değilse "Evde yok" ve nerede olabileceği.
        /// </summary>
        public bool Call(Character caller, Character master, bool masterAtHome, out LocText message, string helpAt = null)
        {
            message = default;
            if (caller == null || master == null || master.IsDead || !master.IsVillager) { message = LocText.Of("village.call.nobody"); return false; }
            if (master.InBreakdown) { message = LocText.Of("village.call.breakdown", ("name", master.Name)); return false; }
            if (!masterAtHome)
            {
                string where = PlaceFor(master);
                message = LocText.Of("village.call.not_home", ("name", master.Name), ("where", LocText.Of(PlaceTextKey(where))));
                return false;
            }
            var st = S(master);
            st.CalledTo = helpAt ?? WorkOf(master) ?? HomeOf(master);
            st.CalledUntil = _clock.TotalDays + (helpAt == FarmhouseKey ? MidwifeCallDays : CallDays);
            message = LocText.Of(helpAt != null ? "village.call.coming_help" : "village.call.coming", ("name", master.Name));
            Bubble?.Invoke(master, LocText.Of("village.call.bubble"));
            return true;
        }

        /// <summary>Yer anahtarının oyuncuya söylenen adı ("meydanda", "meyhanede").</summary>
        public static string PlaceTextKey(string place)
        {
            if (string.IsNullOrEmpty(place)) return "village.place.unknown";
            if (place == "square") return "village.place.square";
            if (place == VillagerFreeWill.Tavern) return "village.place.tavern";
            if (place == VillagerFreeWill.Riverside) return "village.place.riverside";
            if (place == VillagerFreeWill.GreatTree || place == VillagerFreeWill.Charity) return "village.place.great_tree";
            if (place.StartsWith("work:", StringComparison.Ordinal)) return "village.place.work";
            if (place.StartsWith("home:", StringComparison.Ordinal) || VillageHomes.IsInn(place)) return "village.place.home";
            return "village.place.out";
        }

        // ---- ikram ve pazarlık (belge 66f §B.8) ----

        /// <summary>Oyuncu bu ustadan alışveriş yaptı: keyifliyse düşük olasılıkla ikram (eşya ya da indirim) ve balon.</summary>
        public PurchaseTreat OnPurchase(Character owner, double? roll = null)
        {
            if (owner == null || owner.IsDead || MoodTier(owner) != VillageMood.Cheerful) return null;
            double r = roll ?? _rng.NextDouble();
            if (r >= Config.GiftChance) return null;
            var def = _villagers.DefOf(owner);
            string role = def?.Role;
            string item = role != null && Config.Treats.TryGetValue(role, out var t) ? t : null;
            bool asItem = item != null && _content.Items.Contains(item) && _rng.Next(2) == 0;
            var treat = new PurchaseTreat { ItemId = asItem ? item : null, Discount = asItem ? 0.0 : Config.Discount, Line = TreatLine(role) };
            Bubble?.Invoke(owner, treat.Line);
            return treat;
        }

        private LocText TreatLine(string role)
        {
            if (role != null && _rng.Next(2) == 0) return LocText.Of("village.treat." + role + "." + (1 + _rng.Next(TreatRoleLines)));
            return LocText.Of("village.treat.any." + (1 + _rng.Next(TreatAnyLines)));
        }

        /// <summary>Kasvetli usta pazarlığa kapalı (fiyat cezası yok); açıksa null.</summary>
        public LocText? HaggleRefusal(Character owner) =>
            owner != null && !owner.IsDead && owner.IsVillager && MoodTier(owner) == VillageMood.Gloomy
                ? LocText.Of("village.haggle.gloomy", ("name", owner.Name)) : (LocText?)null;

        // ---- bunalım, teselli, ortak kayıp (belge 66f §B.9-10) ----

        /// <summary>Köylü bunalıma girdi: dükkânı erken kapatır, eve gider; kapıda not.</summary>
        public void OnBreakdownStarted(Character c)
        {
            if (c == null || !c.IsVillager) return;
            Watch(c);
            if (ShopOf(c) != null)
            {
                Notice?.Invoke(LocText.Of("village.n.closed_breakdown", ("name", c.Name)), c.Id);
                Bubble?.Invoke(c, LocText.Of("village.closed.breakdown"));
            }
        }

        /// <summary>Teselli: kalan bunalım süresinin bir kısmı gider (Gönül kazanma ile artar); yakınlık ve hane hafızası artar.</summary>
        public double ConsoleShareFor(Character consoler)
        {
            int charm = consoler != null ? consoler.Skills.GetLevel(SkillType.Charm) : 0;
            return Math.Min(0.9, Config.ConsoleShare + 0.03 * Math.Max(0, charm - 1));
        }

        public LocText? CanConsole(Character consoler, Character villager, bool atVillagerHome)
        {
            if (consoler == null || villager == null || villager.IsDead) return LocText.Of("village.call.nobody");
            if (!villager.InBreakdown) return LocText.Of("village.console.not_needed", ("name", villager.Name));
            if (!atVillagerHome) return LocText.Of("village.console.go_home", ("name", villager.Name));
            if (consoler.Stage < AgeStage.Child) return LocText.Of("village.console.too_young");
            return null;
        }

        public bool Console(Character consoler, Character villager, bool atVillagerHome, out LocText message)
        {
            var block = CanConsole(consoler, villager, atVillagerHome);
            if (block.HasValue) { message = block.Value; return false; }
            double share = ConsoleShareFor(consoler);
            villager.ShortenBreakdown(share);
            if (Family != null) Family.SetAffection(consoler.Id, villager.Id, Math.Min(100, Family.Affection(consoler.Id, villager.Id) + 8));
            Requests?.Memory.Add(villager, Thatchwick.Core.Life.Requests.HouseMemory.Favor, "village.memory.console");
            consoler.GainSkillXp(SkillType.Charm, 3);
            villager.AddMoodlet(ConsoledMood, Text(LocText.Of("village.mood.consoled")), 6, 1.0);
            Bubble?.Invoke(villager, LocText.Of("village.console.thanks"));
            message = villager.InBreakdown
                ? LocText.Of("village.console.done", ("name", villager.Name), ("percent", (int)Math.Round(share * 100)))
                : LocText.Of("village.console.ended", ("name", villager.Name));
            return true;
        }

        /// <summary>Ortak kayıp penceresi (gün): ayar yoksa bir mevsim.</summary>
        public double SharedLossWindow => Config.SharedLossDays > 0 ? Config.SharedLossDays : _clock.Config.DaysPerSeason;

        /// <summary>
        /// Oyuncu karakteri son pencere içinde bir yakınını kaybettiyse ya da yas iç sesi sürüyorsa kaybettiği kişinin adı;
        /// yoksa null. Yakın: eş, ebeveyn, çocuk, kardeş ya da yakınlığı 60 ve üstü dost.
        /// </summary>
        public string RecentLoss(Character player)
        {
            if (player == null || player.IsDead) return null;
            string best = null; double bestDay = double.MinValue;
            if (RecentDeaths != null)
                foreach (var (id, name, day) in RecentDeaths())
                {
                    if (id == player.Id || _clock.TotalDays - day > SharedLossWindow || day < bestDay) continue;
                    var dead = _population.Find(id);
                    bool close = dead != null && (FamilyService.AreCloseRelatives(player, dead) || player.SpouseId == id
                        || (Family != null && Family.Affection(player.Id, id) >= 60));
                    if (!close && !(player.MoodEnabled && player.Mood.Has(MoodCatalog.Grief))) continue;
                    best = name; bestDay = day;
                }
            return best;
        }

        public bool CanShareLoss(Character player, Character villager, bool atVillagerHome) =>
            CanConsole(player, villager, atVillagerHome) == null && RecentLoss(player) != null;

        /// <summary>"Ben de yakın zamanda {ad}'ı kaybettim": bunalım anında biter, ikisine keyif iç sesi; kısa karşılıklı konuşma.</summary>
        public bool ShareLoss(Character player, Character villager, bool atVillagerHome, out LocText message, out List<(Character speaker, LocText line)> talk)
        {
            talk = new List<(Character, LocText)>();
            var block = CanConsole(player, villager, atVillagerHome);
            string lost = RecentLoss(player);
            if (block.HasValue || lost == null) { message = block ?? LocText.Of("village.console.no_loss"); return false; }
            villager.EndBreakdown();
            foreach (var c in new[] { player, villager })
                c.AddMoodlet(SharedLossMood, Text(LocText.Of("village.mood.shared_loss")), 8, 2.0);
            if (Family != null) Family.SetAffection(player.Id, villager.Id, Math.Min(100, Family.Affection(player.Id, villager.Id) + 15));
            Requests?.Memory.Add(villager, Thatchwick.Core.Life.Requests.HouseMemory.Story, "village.memory.shared_loss");
            SharedGrief?.Invoke(player, villager);   // 67a "Yası ortak" hissi (RelationService.AddFeeling)
            talk.Add((player, LocText.Of("village.loss.1", ("name", lost))));
            talk.Add((villager, LocText.Of("village.loss.2")));
            talk.Add((player, LocText.Of("village.loss.3")));
            message = LocText.Of("village.console.shared", ("name", villager.Name));
            return true;
        }

        // ---- iş döngüsü (belge 66f §C.11) ----

        /// <summary>Köylünün işyerinde şu an yaptığı iş (iş döngüsü sırayla, kişiye göre evre kaydırılmış); işi yoksa null.</summary>
        public VillagerWorkStep WorkStep(Character c, double totalDays)
        {
            var def = _villagers.DefOf(c);
            var steps = Config.WorkFor(def?.Role);
            if (steps == null || steps.Count == 0) return null;
            double total = 0;
            foreach (var s in steps) total += s.Minutes;
            if (total <= 0) return steps[0];
            double minute = totalDays * 1440.0 + (c.Id * 37 % 97);
            double t = minute % total;
            foreach (var s in steps)
            {
                if (t < s.Minutes) return s;
                t -= s.Minutes;
            }
            return steps[steps.Count - 1];
        }

        /// <summary>Oynayan öğün sahnesinin pozu (zincir adımları süreye bölünür); sahne yoksa null.</summary>
        public string ScenePose(Character c, double totalDays)
        {
            if (c == null || !_state.TryGetValue(c.Id, out var s) || s.Scene == null || s.Scene.Length == 0) return null;
            if (totalDays < s.SceneFrom || totalDays >= s.SceneUntil) return null;
            double f = (totalDays - s.SceneFrom) / Math.Max(1e-6, s.SceneUntil - s.SceneFrom);
            return s.Scene[Math.Min(s.Scene.Length - 1, (int)(f * s.Scene.Length))];
        }

        /// <summary>
        /// Öğün sahnesini yeniden başlatır: köylü ocağın başına yürüyüp varınca pişirme oradan başlar (oyun katmanı çağırır).
        /// </summary>
        public void RestartScene(Character c)
        {
            if (c == null || !_state.TryGetValue(c.Id, out var s) || s.Scene == null) return;
            double len = s.SceneUntil - s.SceneFrom;
            s.SceneFrom = _clock.TotalDays;
            s.SceneUntil = _clock.TotalDays + len;
        }

        /// <summary>Öğün sahnesi sürüyor mu.</summary>
        public bool InScene(Character c) => c != null && _state.TryGetValue(c.Id, out var s) && s.Scene != null && _clock.TotalDays < s.SceneUntil;

        // ---- hastalık (belge 66g) ----

        private readonly Dictionary<int, double> _sickUntil = new Dictionary<int, double>();
        private long _sickRollDay = -1;

        public bool IsSick(Character c) => c != null && _sickUntil.TryGetValue(c.Id, out var u) && _clock.TotalDays < u;

        /// <summary>Köylüyü hasta yapar (testler ve olaylar): evde yatar, dükkân kapanır, iç ses, rica.</summary>
        public void MakeSick(Character c, double days)
        {
            if (c == null || c.IsDead || !c.IsVillager || days <= 0) return;
            _sickUntil[c.Id] = _clock.TotalDays + days;
            c.AddMoodlet(SickMood, Text(LocText.Of("village.mood.sick")), -10, days);
            if (ShopOf(c) != null)
            {
                Notice?.Invoke(LocText.Of("village.n.closed_sick", ("name", c.Name)), c.Id);
                Bubble?.Invoke(c, LocText.Of("village.closed.sick"));
            }
            if (Requests != null && _content.Items.Contains("item.tea_linden") && _clock.TotalDays - _lastHardshipAny >= HardshipVillageGapDays)
            {
                _lastHardshipAny = _clock.TotalDays;
                Requests.PostSystemicFavor(c, "village.sick", LocText.Of("village.req.sick.title"), LocText.Of("village.req.sick.text", ("name", c.Name)), "item.tea_linden", 1, 2);
            }
        }

        /// <summary>Hastaya şifa veren hediye (çay, şurup, iksir, merhem): hastalık hemen geçer.</summary>
        public static bool Heals(Content.ItemDef item) =>
            item != null && (item.HasTag("potion") || item.Id.StartsWith("item.tea_", StringComparison.Ordinal) || item.Id.Contains("syrup") || item.Id.Contains("remedy"));

        /// <summary>Köylüye hediye verildi: hastaysa ve hediye şifalıysa iyileşir.</summary>
        public bool OnGift(Character villager, Content.ItemDef item)
        {
            if (!IsSick(villager) || !Heals(item)) return false;
            _sickUntil.Remove(villager.Id);
            villager.Mood.Remove(SickMood);
            Bubble?.Invoke(villager, LocText.Of("village.sick.thanks"));
            if (ShopOf(villager) != null) Notice?.Invoke(LocText.Of("village.n.reopened", ("name", villager.Name)), villager.Id);
            return true;
        }

        /// <summary>Günde bir zar: aç ya da üşümüş köylü daha kolay hastalanır.</summary>
        private void RollSickness()
        {
            long day = _clock.DayIndex;
            if (_sickRollDay == day) return;
            _sickRollDay = day;
            var done = new List<int>();
            foreach (var kv in _sickUntil) if (_clock.TotalDays >= kv.Value) done.Add(kv.Key);
            foreach (var id in done)
            {
                _sickUntil.Remove(id);
                var c = _population.Find(id);
                if (c != null && !c.IsDead && ShopOf(c) != null) Notice?.Invoke(LocText.Of("village.n.reopened", ("name", c.Name)), c.Id);
            }
            foreach (var c in _scratch)
            {
                if (c.Stage < AgeStage.Child || IsSick(c)) continue;
                double p = SickChance;
                if (_state.TryGetValue(c.Id, out var st) && st.HardshipDays > 0.1) p += SickHardshipChance;
                if (c.Needs.Get(NeedType.Warmth) < 30f || c.Needs.Get(NeedType.Hunger) < 20f) p += 0.03;
                if (_rng.NextDouble() < p) MakeSick(c, SickMinDays + _rng.NextDouble() * (SickMaxDays - SickMinDays));
            }
        }

        /// <summary>Öğün sahnesinin süresi (oyun günü, ≈40 dakika).</summary>
        public const double MealSceneDays = 0.028;

        private void StartScene(Character c, VillageMeal meal)
        {
            var st = S(c);
            string[] poses = null;
            if (meal.Chain != null && _content.Chains.TryGet(meal.Chain, out var chain))
            {
                var list = new List<string>();
                foreach (var step in chain.Steps) if (!string.IsNullOrEmpty(step.Pose)) list.Add(step.Pose);
                poses = list.ToArray();
            }
            bool cooked = meal.Where == "home" && meal.Chain != "eat";
            if (cooked && _content.Chains.TryGet("eat_serving", out var serve) && poses != null)
            {
                // Ocakta pişirdikten sonra sofrada yer (belge 66g: gerçek ocakta pişirme).
                var list = new List<string>(poses);
                foreach (var step in serve.Steps) if (!string.IsNullOrEmpty(step.Pose)) list.Add(step.Pose);
                poses = list.ToArray();
            }
            st.Scene = poses;
            st.SceneFrom = _clock.TotalDays;
            st.SceneUntil = _clock.TotalDays + (cooked ? CookedMealSceneDays : MealSceneDays);
        }

        // ---- kiler ----

        public int PantryCount(string home, string item) =>
            home != null && item != null && _pantries.TryGetValue(home, out var p) && p.TryGetValue(item, out var n) ? n : 0;

        public void AddToPantry(string home, string item, int count)
        {
            if (home == null || item == null || count == 0) return;
            if (!_pantries.TryGetValue(home, out var p)) { p = new Dictionary<string, int>(StringComparer.Ordinal); _pantries[home] = p; }
            int n = Math.Max(0, Math.Min(PantryCap, (p.TryGetValue(item, out var v) ? v : 0) + count));
            if (n == 0) p.Remove(item); else p[item] = n;
        }

        public IEnumerable<string> PantryHomes => _pantries.Keys;

        /// <summary>Yeni oyun: her hanenin kileri hedefle dolu başlar.</summary>
        public void FillPantries()
        {
            foreach (var c in Villagers())
            {
                string home = HomeOf(c);
                if (_pantries.ContainsKey(home)) continue;
                foreach (var st in Config.Pantry) AddToPantry(home, st.Item, st.Target);
            }
        }

        private bool CanEat(string home, VillageMeal m)
        {
            foreach (var kv in m.Consume) if (PantryCount(home, kv.Key) < kv.Value) return false;
            return true;
        }

        private VillageMeal PickMeal(string home, string where, int salt)
        {
            VillageMeal pick = null; int n = 0;
            foreach (var m in Config.Meals)
            {
                if (m.Where != where || !CanEat(home, m)) continue;
                n++;
                if (pick == null || (salt + n) % n == 0) pick = m;   // eşit olasılıklı seçim (öğün çeşidi)
            }
            return pick;
        }

        private bool Eat(Character c, string home, VillageMeal m)
        {
            foreach (var kv in m.Consume) AddToPantry(home, kv.Key, -kv.Value);
            c.Needs.Restore(NeedType.Hunger, m.Hunger);
            StartScene(c, m);
            MealEaten?.Invoke(c, m);
            return true;
        }

        /// <summary>Meyhanede öğle ya da akşam yemeği: meyhanenin pişmiş yemek stoğundan düşer.</summary>
        private bool EatAtTavern(Character c)
        {
            string dish = TavernDish();
            if (dish == null || Stock.TakeUpTo(dish, 1) == 0) return false;
            c.Needs.Restore(NeedType.Hunger, 70f);
            c.Needs.Restore(NeedType.Fun, 10f);
            return true;
        }

        private string _tavernDish;
        private string TavernDish()
        {
            if (_tavernDish != null) return _tavernDish.Length == 0 ? null : _tavernDish;
            _tavernDish = "";
            var tavern = Shops.Get("tavern");
            if (tavern?.Goods == null) return null;
            foreach (var id in new[] { "item.stew_plain", "item.soup_pea", "item.bread" })
                if (_content.Items.TryGet(id, out var d) && tavern.Goods(d)) { _tavernDish = id; break; }
            if (_tavernDish.Length == 0)
                foreach (var d in _content.Items.All)
                    if (d.HasTag("cooked") && d.Price > 0 && tavern.Goods(d)) { _tavernDish = d.Id; break; }
            return _tavernDish.Length == 0 ? null : _tavernDish;
        }

        // ---- günlük akış ----

        private IEnumerable<Character> Villagers()
        {
            _scratch.Clear();
            foreach (var c in _population.Members) if (!c.IsDead && c.IsVillager) _scratch.Add(c);
            return _scratch;
        }

        /// <summary>
        /// Her karede çağrılır; ~7 oyun dakikasında bir öğün, alım, kendi ürünü ve ortalama ihtiyaç hesaplanır. Oyuncunun
        /// bulunmadığı bölgedeki köylülerde enerji, temizlik, tuvalet, ısınma ve eğlence saatlik ortalamayla tutulur.
        /// </summary>
        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            Stock.Restock(_clock.DayIndex);
            _acc += gameDays;
            _abstractAcc += gameDays;
            if (_acc < StepDays) return;
            double dt = _acc;
            _acc = 0;
            bool hourly = _abstractAcc >= AbstractStepDays;
            double hours = _abstractAcc;
            if (hourly) _abstractAcc = 0;
            double frac = _clock.DayFraction;
            var weather = Weather != null ? Weather() : WeatherType.Clear;
            bool festival = FestivalToday != null && FestivalToday();
            _musicAtTavern = false;
            foreach (var c in Villagers())
                if (c.Stage >= AgeStage.Teen && c.Skills.GetLevel(SkillType.Music) >= 1 && PlaceFor(c, frac, weather, festival) == VillagerFreeWill.Tavern
                    && (AtPlace == null || AtPlace(c, VillagerFreeWill.Tavern))) { _musicAtTavern = true; break; }
            foreach (var c in Villagers())
            {
                Watch(c);
                string place = PlaceFor(c, frac, weather, festival);
                string home = HomeOf(c);
                if (!_pantries.ContainsKey(home)) foreach (var st in Config.Pantry) AddToPantry(home, st.Item, st.Target);   // yeni hane dolu başlar
                bool full = FullSimulation == null || FullSimulation(c);
                bool there = !full || AtPlace == null || AtPlace(c, place);
                if (there) StepNeeds(c, place, home, frac, dt, full);
                if (!full && hourly) AbstractNeeds(c, place, home, frac, hours);
                Hardship(c, home, dt);
            }
            HouseholdDay(frac);
            RollSickness();
        }

        /// <summary>Öğünler, meydan ve meyhanede sosyal ve eğlence (her iki benzetimde aynı).</summary>
        private void StepNeeds(Character c, string place, string home, double frac, double dt, bool full)
        {
            var st = S(c);
            int salt = Hash(c.Id, _clock.DayIndex);
            bool atHome = place == home;
            bool atTavern = place == VillagerFreeWill.Tavern;
            bool critical = c.Needs.Get(NeedType.Hunger) <= Config.CriticalNeed;
            if (c.Stage >= AgeStage.Child)
            {
                // Kahvaltı ve akşam sofrası evde (hane birlikte); hancı akşamını meyhanede yer.
                if ((st.Meals & 1) == 0 && atHome && frac >= BreakfastFrom && frac < Math.Max(BreakfastTo, RestDay ? RestDayWake + 0.02 : 0))
                    if (EatHome(c, home, salt)) st.Meals |= 1;
                if ((st.Meals & 4) == 0 && frac >= DinnerFrom && frac < DinnerTo + 0.1 && (atHome || IsInnkeeperAtWork(c, place)))
                    if (EatHome(c, home, salt)) st.Meals |= 4;
                if ((st.Meals & 4) == 0 && frac >= DinnerFrom && frac < DinnerTo + 0.1 && atTavern && IsMusician(c))
                    if (EatAtTavern(c) || EatHome(c, home, salt)) st.Meals |= 4;   // çalgıcı akşamını meyhanede yer
                // Öğle: işte azık ya da meyhanede yemek; yoksul aşevinde; kritik açlıkta öne alınır.
                if ((st.Meals & 2) == 0 && ((frac >= LunchFrom && frac < LunchTo + 0.05) || (critical && frac >= BreakfastTo && frac < DinnerFrom)))
                {
                    bool ate = false;
                    if (atTavern) ate = EatAtTavern(c);
                    else if (place == VillagerFreeWill.Charity && Charity != null) { Charity.ServeCharity(c.Id, _clock.DayIndex); c.Needs.Restore(NeedType.Hunger, 60f); ate = true; }
                    else if (!atHome) { var m = PickMeal(home, "work", salt); if (m != null) ate = Eat(c, home, m); }
                    else ate = EatHome(c, home, salt);
                    if (!ate && critical && Charity != null && CharityOpen(frac)) { Charity.ServeCharity(c.Id, _clock.DayIndex); c.Needs.Restore(NeedType.Hunger, 50f); ate = true; }
                    if (ate) st.Meals |= 2;
                }
                // Evde kritik açlık: kilerde ne varsa.
                if (critical && atHome && c.Needs.Get(NeedType.Hunger) <= Config.CriticalNeed) EatHome(c, home, salt);
            }
            else if (c.Stage == AgeStage.Baby) c.Needs.Set(NeedType.Hunger, Math.Max(c.Needs.Get(NeedType.Hunger), 60f));   // köy bebeğini anası doyurur
            // Meydan ve meyhane: sohbet ve eğlence (köylüler arası sohbet parti 2'de görünür olur).
            float perDay = (float)(dt / 1.0);
            if (place == "square" || atTavern || place == VillagerFreeWill.Riverside || place == VillagerFreeWill.GreatTree || place == "feast")
            {
                c.Needs.Restore(NeedType.Social, 160f * perDay);
                c.Needs.Restore(NeedType.Fun, (atTavern ? 120f : 70f) * perDay);
                if (atTavern && _musicAtTavern) c.Needs.Restore(NeedType.Fun, 60f * perDay);   // çalgı çalınıyor: herkes daha çok eğlenir
            }
            else if (atHome && HouseMates(c, home)) c.Needs.Restore(NeedType.Social, 50f * perDay);   // evde hane sohbeti
            else if (place != null && place.StartsWith("work:", StringComparison.Ordinal)) c.Needs.Restore(NeedType.Social, 40f * perDay);   // müşteri ile laflamak
            // Uyku: oyuncunun bölgesinde yatağa JobRunner gider; yatak bulunamazsa evde gece kestirir (güvenlik ağı).
            if (full && atHome && Night(frac) && c.Needs.Get(NeedType.Energy) <= NeedSet.CriticalThreshold) c.Needs.Restore(NeedType.Energy, 200f * perDay);
        }

        private static bool Night(double frac) => frac < VillagerService.HomeUntil || frac >= VillagerService.SquareUntil;

        private bool CharityOpen(double frac) => frac >= BreakfastTo && frac < DinnerTo;

        private bool _musicAtTavern;

        /// <summary>Meyhanede şu an çalgı çalınıyor mu (son adımda).</summary>
        public bool MusicAtTavern => _musicAtTavern;

        public bool IsMusician(Character c) => _villagers.DefOf(c)?.Role == RoleMusician;

        /// <summary>
        /// Doğum (belge 66g karar 2): çağrılan ebe evin önündeyse annenin doğum sonrası toparlanması yarıya iner, bebeğe ve anneye
        /// iç ses gelir; ebe işini bitirip döner.
        /// </summary>
        public bool OnBirth(Character baby, Character mother)
        {
            if (baby == null || mother == null) return false;
            if (mother.IsVillager) return VillageBirth(baby, mother);
            foreach (var c in _population.Members)
            {
                if (c.IsDead || !c.IsVillager || !_state.TryGetValue(c.Id, out var st)) continue;
                if (st.CalledTo != FarmhouseKey || _clock.TotalDays >= st.CalledUntil) continue;
                if (AtPlace != null && !AtPlace(c, FarmhouseKey)) continue;
                mother.ShortenPostpartum(MidwifeRecoveryShare);
                baby.AddMoodlet(MidwifeBabyMood, Text(LocText.Of("village.mood.midwife_hands")), 5, 3.0);
                mother.AddMoodlet(MidwifeMotherMood, Text(LocText.Of("village.mood.midwife_help")), 6, 2.0);
                Bubble?.Invoke(c, LocText.Of("village.midwife.bubble"));
                Notice?.Invoke(LocText.Of("village.n.midwife", ("name", c.Name), ("baby", baby.Name)), mother.Id);
                st.CalledUntil = _clock.TotalDays;   // işi bitti, programına döner
                return true;
            }
            return false;
        }

        /// <summary>
        /// Köydeki doğum (belge 66g): köyün ebesi (aktar) yaşıyor, hasta ya da bunalımda değilse lohusayı ziyaret eder; annenin
        /// toparlanması yarıya iner, bebeğe ve anneye iç ses gelir.
        /// </summary>
        private bool VillageBirth(Character baby, Character mother)
        {
            Character midwife = null;
            foreach (var st in _villagers.States)
            {
                if (!_content.Villagers.TryGet(st.DefId, out var d) || d.Role != "herbalist") continue;
                var c = _population.Find(st.CharacterId);
                if (c != null && !c.IsDead && c.IsVillager && !c.InBreakdown && !IsSick(c) && c != mother) { midwife = c; break; }
            }
            if (midwife == null) return false;
            mother.ShortenPostpartum(MidwifeRecoveryShare);
            baby.AddMoodlet(MidwifeBabyMood, Text(LocText.Of("village.mood.midwife_hands")), 5, 3.0);
            mother.AddMoodlet(MidwifeMotherMood, Text(LocText.Of("village.mood.midwife_help")), 6, 2.0);
            var ms = S(midwife);
            ms.CalledTo = HomeOf(mother);
            ms.CalledUntil = _clock.TotalDays + MidwifeVisitDays;   // lohusayı ziyaret eder
            Bubble?.Invoke(midwife, LocText.Of("village.midwife.village_bubble"));
            return true;
        }

        private bool IsInnkeeperAtWork(Character c, string place)
        {
            var d = _villagers.DefOf(c);
            return d != null && d.Role == VillagerService.RoleInnkeeper && place == d.Work;
        }

        private bool HouseMates(Character c, string home)
        {
            foreach (var o in _scratch) if (o != c && HomeOf(o) == home) return true;
            return false;
        }

        private bool EatHome(Character c, string home, int salt)
        {
            var m = PickMeal(home, "home", salt) ?? PickMeal(home, "work", salt);
            return m != null && Eat(c, home, m);
        }

        /// <summary>
        /// Soyut hesap (oyuncunun bulunmadığı bölge): yürüyüş ve eşya kullanımı yok. Uyku gece evde, tuvalet ve yıkanma evde
        /// ya da işte, ısınma kapalı yerde; değerler oyuncunun bölgesindeki ortalama gün akışına yakın tutulur.
        /// </summary>
        private void AbstractNeeds(Character c, string place, string home, double frac, double days)
        {
            bool atHome = place == home;
            bool indoor = atHome || (place != null && (place.StartsWith("work:", StringComparison.Ordinal) || VillageHomes.IsInn(place)));
            float h = (float)(days * 24.0);
            if (atHome && Night(frac)) { c.Needs.Restore(NeedType.Energy, 14f * h); c.Asleep = true; }
            else c.Asleep = false;
            if (indoor && c.Needs.Get(NeedType.Bladder) < 45f) c.Needs.Set(NeedType.Bladder, 95f);
            if (atHome && c.Needs.Get(NeedType.Hygiene) < 45f) c.Needs.Set(NeedType.Hygiene, 90f);
            if (indoor) c.Needs.Restore(NeedType.Warmth, 25f * h);
            if (c.Needs.Get(NeedType.Fun) < 40f) c.Needs.Restore(NeedType.Fun, 4f * h);   // evde küçük uğraşlar
        }

        /// <summary>Aç ya da üşüyen köylü (uzun süre): yardım ricası (belge 66e §2.4). Köylü ölmez.</summary>
        private void Hardship(Character c, string home, double dt)
        {
            if (c.Stage < AgeStage.YoungAdult) return;
            var st = S(c);
            bool hard = c.Needs.Get(NeedType.Hunger) <= NeedSet.CriticalThreshold || c.Needs.Get(NeedType.Warmth) <= NeedSet.CriticalThreshold;
            st.HardshipDays = hard ? st.HardshipDays + dt : 0;
            if (st.HardshipDays < HardshipDays || Requests == null || _clock.TotalDays - st.LastHardshipRequest < HardshipRepeatDays) return;
            if (_clock.TotalDays - _lastHardshipAny < HardshipVillageGapDays) return;
            st.LastHardshipRequest = _clock.TotalDays;
            _lastHardshipAny = _clock.TotalDays;
            st.HardshipDays = 0;
            bool cold = c.Needs.Get(NeedType.Warmth) <= NeedSet.CriticalThreshold;
            string item = cold ? "item.firewood" : "item.bread";
            if (!_content.Items.Contains(item)) return;
            Requests.PostSystemicFavor(c, cold ? "village.hardship.cold" : "village.hardship.hunger",
                LocText.Of(cold ? "village.req.cold.title" : "village.req.hunger.title"),
                LocText.Of(cold ? "village.req.cold.text" : "village.req.hunger.text", ("name", c.Name)), item, cold ? 6 : 3);
        }

        /// <summary>Hane günü: sabah kendi ürünü kilere, dükkânlar açılınca eksik kiler için alım (stoktan düşer).</summary>
        private void HouseholdDay(double frac)
        {
            long day = _clock.DayIndex;
            if (frac < ProduceFrom) return;
            foreach (var c in _scratch)
            {
                string home = HomeOf(c);
                if (!_producedDay.TryGetValue(home, out var pd) || pd != day)
                {
                    _producedDay[home] = day;
                    foreach (var mate in _scratch)
                    {
                        if (HomeOf(mate) != home) continue;
                        var role = _villagers.DefOf(mate)?.Role;
                        if (role == null) continue;
                        foreach (var p in Config.Produce) if (p.Role == role) AddToPantry(home, p.Item, p.Count);
                    }
                }
                if (frac < ShoppingFrom || RestDay) continue;
                if (_shoppedDay.TryGetValue(home, out var sd) && sd == day) continue;
                if (c.Stage < AgeStage.YoungAdult || c.InBreakdown) continue;   // alımı hanenin bir yetişkini yapar
                _shoppedDay[home] = day;
                Shop(home);
            }
        }

        /// <summary>Kilerde eşiğin altına inen mallar dükkân stoğundan tamamlanır (bu partide görünür yolculuk yok).</summary>
        public int Shop(string home)
        {
            int bought = 0;
            foreach (var st in Config.Pantry)
            {
                int have = PantryCount(home, st.Item);
                if (have >= st.Min && have > 0) continue;
                var shopId = Stock.ShopOf(st.Item);
                var shop = shopId != null ? Shops.Get(shopId) : null;
                if (shop == null || !ShopTradesToday(shop)) continue;
                int got = Stock.TakeUpTo(st.Item, st.Target - have);
                if (got > 0) { AddToPantry(home, st.Item, got); bought += got; }
            }
            return bought;
        }

        /// <summary>Dükkân bugün köylüye satar mı: Pazar değil, ustası bunalımda, hasta ya da yasta değil.</summary>
        private bool ShopTradesToday(ShopDef shop)
        {
            if (ShopDef.IsRestDay(_clock.DayIndex) && !shop.OpenOnRestDay) return false;
            foreach (var c in _scratch)
                if (WorkOf(c) == shop.WorkKey && _villagers.DefOf(c) != null)
                    return ClosureOf(shop, c) == ShopClosure.None;
            return true;   // sahibi olmayan dükkân (eski kayıt): stok yine satılır
        }

        // ---- kayıt ----

        public LivingVillageSave Export()
        {
            var save = new LivingVillageSave { stock = Stock.Export() };
            var homes = new List<string>(_pantries.Keys);
            homes.Sort(StringComparer.Ordinal);
            var list = new List<PantrySave>();
            foreach (var h in homes)
            {
                var items = new List<string>(_pantries[h].Keys);
                items.Sort(StringComparer.Ordinal);
                var counts = new int[items.Count];
                for (int i = 0; i < items.Count; i++) counts[i] = _pantries[h][items[i]];
                list.Add(new PantrySave { home = h, items = items.ToArray(), counts = counts });
            }
            save.pantries = list.ToArray();
            var meals = new List<int>();
            foreach (var kv in _state) if (kv.Value.Day == _clock.DayIndex && kv.Value.Meals != 0) { meals.Add(kv.Key); meals.Add(kv.Value.Meals); }
            save.mealsDay = _clock.DayIndex;
            save.meals = meals.ToArray();
            var sick = new List<double>();
            foreach (var kv in _sickUntil) if (_clock.TotalDays < kv.Value) { sick.Add(kv.Key); sick.Add(kv.Value); }
            save.sick = sick.ToArray();
            return save;
        }

        /// <summary>Eski kayıt (null): kilerler dolu başlar, stok bugünden.</summary>
        public void Import(LivingVillageSave save)
        {
            _pantries.Clear(); _state.Clear(); _shoppedDay.Clear(); _producedDay.Clear(); _sickUntil.Clear();
            Stock.Import(save?.stock);
            if (save == null) { FillPantries(); return; }
            if (save.pantries != null)
                foreach (var p in save.pantries)
                {
                    if (p?.home == null || p.items == null || p.counts == null) continue;
                    for (int i = 0; i < p.items.Length && i < p.counts.Length; i++) AddToPantry(p.home, p.items[i], p.counts[i]);
                    if (!_pantries.ContainsKey(p.home)) _pantries[p.home] = new Dictionary<string, int>(StringComparer.Ordinal);
                }
            if (save.meals != null && save.mealsDay == _clock.DayIndex)
                for (int i = 0; i + 1 < save.meals.Length; i += 2)
                {
                    var s = new DayState { Day = save.mealsDay, Meals = save.meals[i + 1] };
                    _state[save.meals[i]] = s;
                }
            if (save.sick != null)
                for (int i = 0; i + 1 < save.sick.Length; i += 2) _sickUntil[(int)save.sick[i]] = save.sick[i + 1];
            _sickRollDay = _clock.DayIndex;   // yüklenen gün yeniden zar atılmaz
            FillPantries();   // kaydı olmayan yeni hane (halef, evlenen)
        }
    }

    [Serializable]
    public sealed class PantrySave
    {
        public string home;
        public string[] items;
        public int[] counts;
    }

    /// <summary>Yaşayan köy kaydı (eklemeli alan; eski kayıtta null).</summary>
    [Serializable]
    public sealed class LivingVillageSave
    {
        public PantrySave[] pantries;
        public ShopStockSave stock;
        public long mealsDay = -1;
        public int[] meals;
        /// <summary>Hasta köylüler: (kimlik, iyileşme günü) çiftleri. Eski kayıtta null.</summary>
        public double[] sick;
    }
}
