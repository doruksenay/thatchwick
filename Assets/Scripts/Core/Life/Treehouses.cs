using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Ağaç ev türleri (belge 52 §4). Sıra Marangozluk kilidiyle artar.</summary>
    public enum TreehouseKind { None = 0, Nest = 1, Lookout = 2, Fort = 3, Hut = 4, Kiosk = 5 }

    /// <summary>Ulu ağaç işlerinin kodları (InteractionDef.TreeAction).</summary>
    public static class TreeActions
    {
        public const string Found = "found", Build = "build", Repair = "repair", Play = "play", Lookout = "lookout",
            Siege = "siege", Sleep = "sleep", Stash = "stash", Dig = "dig", Carve = "carve", Gather = "gather", Shade = "shade",
            Dine = "dine", Charity = "charity", Donate = "donate", InnToggle = "innToggle", DonateFood = "donateFood", CallDown = "callDown";
    }

    /// <summary>Bir ağaç evde saklanan hazine (Saz Damlı Kulübe, belge 52 §4).</summary>
    public sealed class TreeStash
    {
        public string ItemId;
        public int OwnerId;
        public string OwnerName;
        public double Day;
    }

    /// <summary>Bir ulu ağacın üstündeki ağaç evin durumu.</summary>
    public sealed class TreehouseSite
    {
        public int TreeId;
        public TreehouseKind Kind;
        public double Progress;
        public double Needed;
        public double Durability = 100;
        /// <summary>Kuran: 0 oyuncunun hanesi, &gt;0 komşu hane kimliği, -1 köy (kasaba aşevi).</summary>
        public int Builder;
        public string BuilderName;
        public double BuiltDay = -1;
        public double RuinedSince = -1;
        public bool Warned;
        public bool InnOpen;
        public long LastGatherDay = -1;
        public readonly List<int> Helpers = new List<int>();
        public bool HadChild;
        public readonly List<string> CarvedNames = new List<string>();
        public readonly List<int> CarvedIds = new List<int>();
        public TreeStash Stash;
        /// <summary>Köşkün oda köşesi (z=1) ve boyu; köşk değilse N=0.</summary>
        public int KioskX, KioskY, KioskN;
        /// <summary>Bugün uğrayan köy çocukları (kaydedilmez).</summary>
        public readonly List<int> Visitors = new List<int>();

        public bool HasHouse => Kind != TreehouseKind.None;
        public bool IsBuilt => Kind != TreehouseKind.None && Progress >= Needed - 1e-9;
        public bool IsRuined => IsBuilt && Durability <= 0;
        public bool IsUsable => IsBuilt && !IsRuined;
        public bool IsVillageInn => Builder < 0;
    }

    /// <summary>
    /// Ulu ağaçlar ve ağaç evler (Project belgesi 52). Beş tür ağaç ev Marangozluk'la kurulur: marangoz tezgâhında iskelet
    /// yapılır, ağacın dibinde birkaç oturumda biter; çocuk ve ergen yardım eder. 1-4. türler tek eşyadır, içi görünmez
    /// (karakter kaybolur); köşk ağaca bağlı küçük bir kattır, eşya konur. Arsa dışındaki ağaç evler ortaktır: satılmaz,
    /// yıkılmaz; harabe olursa her hane onarabilir. Bütün çocuklar (köy çocukları dahil) her ağaç evde takılır.
    /// Kasaba çınarındaki aşevi köyündür: ücretli yemek, hayır sofrası, bağış ve mevsim sonu Bereket Sofrası
    /// (CharityKitchen). Sayılar yer tutucudur (denge testi).
    /// </summary>
    public sealed class TreehouseSystem
    {
        // ---- sabitler ----
        public const double DecayPerDay = 0.6, WinterDecay = 0.6, StormDecay = 6.0, WarnAt = 30.0;
        public const double RepairBelow = 90.0;
        public const int MealPrice = 8;   // aşevinde yahni (inn_eat)
        public const double InnOpens = 0.30, InnCloses = 0.92;
        public const double FallChance = 0.02;
        public const double RefugeRange = 40;
        public const double RivalBuildChance = 0.25;
        public const double FeastHour = 0.75;
        public const int InnCustomersBase = 3;
        public const double NestFeatherChance = 0.15;
        /// <summary>Düşenlerin bu kadarı gerçek yara alır (yara izi kuralına uyar).</summary>
        public const double FallInjuryShare = 0.10;
        /// <summary>Ağaç evde oynayan çocuğun en sevdiği oyuncağa alışkanlık çarpanı (belge 52 §7).</summary>
        public const double ToyBoostFactor = 1.25;
        /// <summary>"Yükseklik sevdalısı" izi için çocuklukta ağaç evde oynama sayısı.</summary>
        public const int HeightsPlays = 15;
        public const string HeightsMark = "mark.heights";
        /// <summary>Köşk taşıma sınırı (4x4 / 5x5) ve kestane çarpanı (belge 52 §5.1).</summary>
        public const int KioskCapacity4 = 40, KioskCapacity5 = 60;
        public const double ChestnutCapacity = 0.9, LoadWarnAt = 0.9;
        /// <summary>Ağaç köşkündeki konforlu eşyaya manzara katkısı.</summary>
        public const int ViewComfort = 2;
        public const string ViewMood = "mood.tree_view";
        /// <summary>Yiyecek bağışında bir seferde en çok bu kadar birim.</summary>
        public const int FoodDonationUnits = 5;
        /// <summary>İstek sistemi: ağaç ev bitince sayılan iş (belge 46).</summary>
        public const string BuiltActId = "treehouse.built";
        public const double RivalRepairChance = 0.3, RequestChance = 0.35;
        /// <summary>Köylülerin yürüdüğü saatler (gün kesri): aşevinde öğle, çocuklar ağaç evde ikindi, mevsim sonu sofra.</summary>
        public const double LunchFrom = 0.45, LunchTo = 0.55, VisitFrom = 0.55, VisitTo = 0.68, FeastGatherFrom = 0.70, FeastTo = 0.86;

        /// <summary>Çınar Aşevi'nin öğünleri (belge 52 §5.4): etkileşim kimliği → fiyat, moodlet metni, değer.</summary>
        public static readonly Dictionary<string, (int price, string mood, int value)> Meals = new Dictionary<string, (int, string, int)>(StringComparer.Ordinal)
        {
            ["inn_soup"] = (5, "Sıcak çorba", 3),
            ["inn_eat"] = (8, "Manzaralı sofra", 5),
            ["inn_roast"] = (14, "Çınar altında ziyafet", 10),
        };
        public static int PriceOf(string interactionId) => interactionId != null && Meals.TryGetValue(interactionId, out var m) ? m.price : MealPrice;
        public const string NestFeather = "item.down_feather";
        public const string PlayMood = "mood.treehouse_play", OwnFortMood = "mood.own_treehouse", BuiltTogetherMood = "mood.built_together",
            FortWinnerMood = "mood.fort_holder", KneeMood = "mood.scraped_knee", ShadeMood = "mood.great_tree_shade",
            ViewMealMood = "mood.view_meal", CharityGoodMood = "mood.charity_stew", GoodDeedMood = "mood.good_deed",
            FeastMood = "mood.same_table", BannedMood = "mood.treehouse_banned";

        /// <summary>Türün bitmesi için gereken iş (oturum birimi: yetişkin 1, ergen 0,75, çocuk 0,5).</summary>
        public static double WorkNeeded(TreehouseKind k) => k switch
        {
            TreehouseKind.Nest => 2, TreehouseKind.Lookout => 3, TreehouseKind.Fort => 3, TreehouseKind.Hut => 4, TreehouseKind.Kiosk => 5, _ => 0
        };

        public static string KindName(TreehouseKind k) => k switch
        {
            TreehouseKind.Nest => "Dal Yuvası", TreehouseKind.Lookout => "Gözcü Kulesi", TreehouseKind.Fort => "Tahta Kale",
            TreehouseKind.Hut => "Saz Damlı Kulübe", TreehouseKind.Kiosk => "Ağaç Köşkü", _ => ""
        };

        public static string KindKey(TreehouseKind k) => k switch
        {
            TreehouseKind.Nest => "nest", TreehouseKind.Lookout => "lookout", TreehouseKind.Fort => "fort",
            TreehouseKind.Hut => "hut", TreehouseKind.Kiosk => "kiosk", _ => null
        };

        public static TreehouseKind KindFromKey(string key)
        {
            switch (key)
            {
                case "nest": return TreehouseKind.Nest;
                case "lookout": return TreehouseKind.Lookout;
                case "fort": return TreehouseKind.Fort;
                case "hut": return TreehouseKind.Hut;
                case "kiosk": return TreehouseKind.Kiosk;
                default: return TreehouseKind.None;
            }
        }

        /// <summary>Büyük ağaçta (5x5) köşk ek malzemesi (belge 52 §4: +%30).</summary>
        public static readonly (string item, int count)[] BigKioskExtra = { ("item.plank", 8), ("item.nails", 4) };

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly Population _population;
        private readonly GameClock _clock;
        private readonly Random _rng;
        private readonly Dictionary<int, TreehouseSite> _sites = new Dictionary<int, TreehouseSite>();
        private readonly HashSet<GridPos> _shell = new HashSet<GridPos>();
        private readonly HashSet<GridPos> _ownKiosk = new HashSet<GridPos>();
        private readonly HashSet<int> _banned = new HashSet<int>();
        private readonly Dictionary<int, long> _fedVillagers = new Dictionary<int, long>();
        private long _feastAnnounced = -1;
        private readonly Dictionary<int, int> _sulking = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _plays = new Dictionary<int, int>();
        private readonly HashSet<int> _poorToday = new HashSet<int>();
        private readonly HashSet<int> _loadWarned = new HashSet<int>();
        private readonly Dictionary<int, string> _chosenStash = new Dictionary<int, string>();
        private long _wishSeason = -1, _requestSeason = -1;

        public CharityKitchen Kitchen { get; } = new CharityKitchen();

        /// <summary>Bereket Sofrası kuruldu (oyun katmanı: sofra ezgisi).</summary>
        public event Action<FeastResult> FeastHeld;
        /// <summary>Hanenin ağaç evi gıcırdıyor ya da harap oldu (oyun katmanı: gıcırtı sesi).</summary>
        public event Action Creaked;

        // ---- bağlar (LifeSystems kurar) ----
        public Action<string, int> Notify { get; set; }
        public Action<string, int> Warn { get; set; }
        public Action<string> Chronicle { get; set; }
        public Action<int, string> AddReputation { get; set; }
        /// <summary>İki kişi arasında yakınlık (a, b, fark).</summary>
        public Action<int, int, int> AddAffection { get; set; }
        /// <summary>Köylünün hanesine hane hafızası (köylü, fark, neden).</summary>
        public Action<Character, int, string> AddMemory { get; set; }
        public Func<WeatherType> WeatherToday { get; set; }
        public Func<string> Forecast { get; set; }
        /// <summary>Kurt sürüsü yaklaşıyor mu (gözcü kulesi erken uyarısı).</summary>
        public Func<bool> WolvesComing { get; set; }
        public Func<string> HouseholdName { get; set; }
        /// <summary>Komşu haneler (kimlik, ad) — rakip kuruluş ve bağış.</summary>
        public Func<IEnumerable<(int id, string name)>> Neighbors { get; set; }
        /// <summary>Hatıra verir (eşya, etiket, karakter).</summary>
        public Action<string, string, int> GrantKeepsake { get; set; }
        /// <summary>Gerçek düşme yarası (InjurySystem "fall"; yara izi kuralı orada). Yara olduysa true.</summary>
        public Func<Character, bool> Injure { get; set; }
        /// <summary>Çocuğun en sevdiği oyuncağa alışkanlık (çarpan) — ToyHabitSystem.PlayFavorite.</summary>
        public Action<Character, double> ToyBoost { get; set; }
        /// <summary>Köy çocuğunun ailesinden "Bize ağaç ev yapar mısın?" ricası (veren, çocuk). Asıldıysa true.</summary>
        public Func<Character, Character, bool> PostRequest { get; set; }
        /// <summary>Hanenin ağaç evi bitti (son çivi çakan, yer): istek sayımı.</summary>
        public event Action<Character, TreehouseSite> Built;

        public TreehouseSystem(WorldState world, ContentDatabase content, Household house, Population population, GameClock clock, int seed = 52)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _rng = new Random(seed);
        }

        private double Now => _clock.TotalDays;
        private long Today => _clock.DayIndex;
        private void Say(string text, int id = 0) => Notify?.Invoke(text, id);
        private void Alarm(string text, int id = 0) { if (Warn != null) Warn(text, id); else Say(text, id); }
        private string HouseName => HouseholdName?.Invoke() ?? "Hane";

        public IEnumerable<TreehouseSite> Sites => _sites.Values;
        public TreehouseSite SiteOf(int treeItemId) => _sites.TryGetValue(treeItemId, out var s) ? s : null;
        public TreehouseSite SiteOf(ItemInstance tree) => tree == null ? null : SiteOf(tree.Id);

        public static bool IsMinor(Character c) => c != null && (c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen);
        public static bool IsAdult(Character c) => c != null && c.Stage >= AgeStage.YoungAdult;

        /// <summary>Ağaç arsada mı (hanenin malı).</summary>
        public bool OnPlot(ItemInstance tree) => tree != null && _world.Map.IsOwned(tree.Position);

        // ---- kuruluş ve kayıttan sonra ----

        /// <summary>
        /// Dünya hazırken bir kez: kasaba çınarının hazır aşevi kaydı açılır, köşklerin ip merdiveni ve kabuğu kurulur,
        /// silinmiş ağaçların kaydı düşer.
        /// </summary>
        public void Attach()
        {
            foreach (var id in new List<int>(_sites.Keys))
                if (_world.GetItem(id) == null || !GreatTrees.IsGreatTree(_world.GetItem(id).Def)) _sites.Remove(id);
            foreach (var it in _world.Items)
            {
                if (!GreatTrees.IsInn(it.Def) || _sites.ContainsKey(it.Id)) continue;
                _sites[it.Id] = new TreehouseSite
                {
                    TreeId = it.Id, Kind = TreehouseKind.Kiosk, Progress = WorkNeeded(TreehouseKind.Kiosk), Needed = WorkNeeded(TreehouseKind.Kiosk),
                    Builder = -1, BuilderName = "Köy", BuiltDay = 0, InnOpen = true
                };
            }
            RebuildShell();
        }

        private void RebuildShell()
        {
            _shell.Clear(); _ownKiosk.Clear();
            foreach (var s in _sites.Values)
            {
                if (s.KioskN <= 0) continue;
                var tree = _world.GetItem(s.TreeId);
                if (tree == null) continue;
                foreach (var p in KioskTiles(s)) { _shell.Add(p); if (s.Builder == 0) _ownKiosk.Add(p); }
                var ladder = GreatTrees.LadderTile(tree);
                _world.AddFixedLink(ladder, new GridPos(ladder.X, ladder.Y, 1));
            }
        }

        /// <summary>Köşkün z=1 karoları: oda ve balkon.</summary>
        public static IEnumerable<GridPos> KioskTiles(TreehouseSite s)
        {
            for (int y = s.KioskY - 1; y < s.KioskY + s.KioskN; y++)
                for (int x = s.KioskX; x < s.KioskX + s.KioskN; x++)
                    yield return new GridPos(x, y, 1);
        }

        private static IEnumerable<GridPos> KioskRoom(TreehouseSite s)
        {
            for (int y = s.KioskY; y < s.KioskY + s.KioskN; y++)
                for (int x = s.KioskX; x < s.KioskX + s.KioskN; x++)
                    yield return new GridPos(x, y, 1);
        }

        /// <summary>Köşk kabuğu (duvar ve döşeme aracı çalışmaz). BuildService.Shell bağlar.</summary>
        public bool IsShell(GridPos p) => _shell.Contains(p);
        /// <summary>Hanenin kurduğu köşkün içi: eşya konur (arsa dışında da). BuildService.ExtraOwned bağlar.</summary>
        public bool ExtraOwned(GridPos p) => _ownKiosk.Contains(p);

        /// <summary>Köşkü kurar: oda (4x4 ya da 5x5), balkon, duvar, kapı, pencereler, korkuluk ve ip merdiveni (belge 52 §5.1).</summary>
        public void BuildKiosk(ItemInstance tree, TreehouseSite s)
        {
            var map = _world.Map;
            int n = GreatTrees.KioskSize(tree.Def);
            s.KioskN = n; s.KioskX = tree.Position.X - 1; s.KioskY = tree.Position.Y - 1;
            foreach (var p in KioskTiles(s))
            {
                if (!map.InBounds(p)) continue;
                if (!map.HasTile(p)) map.SetTile(p, true);
                map.SetFloor(p, FloorType.Wood);
            }
            int x0 = s.KioskX, y0 = s.KioskY, yb = y0 - 1, z = 1;
            int door = tree.Position.X, mid = x0 + n / 2;
            for (int x = x0; x < x0 + n; x++)
            {
                SetEdge(new Edge(x, y0, EdgeAxis.Horizontal, z), x == door ? EdgeKind.Door : EdgeKind.Wall);
                SetEdge(new Edge(x, y0 + n, EdgeAxis.Horizontal, z), x == mid ? EdgeKind.Window : EdgeKind.Wall);
                SetEdge(new Edge(x, yb, EdgeAxis.Horizontal, z), EdgeKind.Railing);
            }
            for (int y = y0; y < y0 + n; y++)
            {
                SetEdge(new Edge(x0, y, EdgeAxis.Vertical, z), y == y0 + n / 2 ? EdgeKind.Window : EdgeKind.Wall);
                SetEdge(new Edge(x0 + n, y, EdgeAxis.Vertical, z), y == y0 + n / 2 ? EdgeKind.Window : EdgeKind.Wall);
            }
            SetEdge(new Edge(x0, yb, EdgeAxis.Vertical, z), EdgeKind.Railing);
            SetEdge(new Edge(x0 + n, yb, EdgeAxis.Vertical, z), EdgeKind.Railing);
            RebuildShell();
            _world.NotifyChanged();
        }

        private void SetEdge(Edge e, EdgeKind kind)
        {
            if (_world.Map.EdgeInBounds(e)) _world.Map.SetEdge(e, kind);
        }

        // ---- menü ve engeller ----

        /// <summary>Ağaç işinde karakter görünmez: ağaç evin içinde (Sims 3 yöntemi). Oyun katmanı çizmez.</summary>
        public static bool Hides(InteractionDef i)
        {
            switch (i?.TreeAction)
            {
                case TreeActions.Play: case TreeActions.Lookout: case TreeActions.Siege: case TreeActions.Sleep:
                case TreeActions.Dine: case TreeActions.Charity: return true;
                default: return false;
            }
        }

        private static TreehouseKind FoundKind(InteractionDef i)
        {
            int u = i.Id.IndexOf('_');
            return u < 0 ? TreehouseKind.None : KindFromKey(i.Id.Substring(u + 1));
        }

        private static int DonateAmount(InteractionDef i)
        {
            int u = i.Id.LastIndexOf('_');
            return u >= 0 && int.TryParse(i.Id.Substring(u + 1), out int n) ? n : 0;
        }

        /// <summary>Menüde görünsün mü (duruma göre; engeli olan ama durumuna uygun iş görünür, gri kalır).</summary>
        public bool Shows(ItemInstance item, InteractionDef i)
        {
            if (item == null || i?.TreeAction == null || !GreatTrees.IsGreatTree(item.Def)) return true;
            var s = SiteOf(item);
            bool inn = GreatTrees.IsInn(item.Def);
            switch (i.TreeAction)
            {
                case TreeActions.Carve: case TreeActions.Gather: case TreeActions.Shade: return true;
                case TreeActions.Dine: case TreeActions.Charity: case TreeActions.Donate: case TreeActions.DonateFood: return inn;
                case TreeActions.CallDown: return !inn && s != null && SulkingAt(item.Id).Count > 0;
            }
            if (inn) return false;
            switch (i.TreeAction)
            {
                case TreeActions.Found: return s == null || !s.HasHouse;
                case TreeActions.Build: return s != null && s.HasHouse && !s.IsBuilt;
                case TreeActions.Repair: return s != null && s.IsBuilt && s.Durability < RepairBelow;
                case TreeActions.Play: return s != null && s.IsBuilt;
                case TreeActions.Lookout: return s != null && s.IsBuilt && s.Kind == TreehouseKind.Lookout;
                case TreeActions.Siege: return s != null && s.IsBuilt && s.Kind == TreehouseKind.Fort;
                case TreeActions.Sleep: case TreeActions.Stash: case TreeActions.Dig: return s != null && s.IsBuilt && s.Kind == TreehouseKind.Hut;
                case TreeActions.InnToggle: return s != null && s.IsBuilt && s.Kind == TreehouseKind.Kiosk && s.Builder == 0;
            }
            return true;
        }

        private bool Snowy => WeatherToday != null && WeatherToday() == WeatherType.Snow;
        private static bool Covered(TreehouseKind k) => k == TreehouseKind.Hut || k == TreehouseKind.Kiosk;

        /// <summary>Ağaç işinin başlama engeli (InteractionEffects.ExtraBlocker); yoksa null.</summary>
        public string CanStart(Character c, ItemInstance item, InteractionDef i)
        {
            if (i?.TreeAction == null || item == null || c == null) return null;
            if (!GreatTrees.IsGreatTree(item.Def)) return "Bu bir ulu ağaç değil.";
            var s = SiteOf(item);
            bool inn = GreatTrees.IsInn(item.Def);
            switch (i.TreeAction)
            {
                case TreeActions.Found:
                {
                    if (inn) return "Çınarın köşkü köyün aşevidir.";
                    if (s != null && s.HasHouse) return "Bu ağaçta zaten bir ağaç ev var.";
                    if (!IsAdult(c)) return "Ağaç evi bir yetişkin kurar; çocuklar sonra yardıma gelir.";
                    var k = FoundKind(i);
                    if (k == TreehouseKind.Kiosk && GreatTrees.KioskSize(item.Def) == 5)
                        foreach (var (id, n) in BigKioskExtra)
                            if (_house.Count(id) < n) return $"Büyük ağaçta köşk için ayrıca {n} {ItemName(id).ToLowerInvariant()} gerekir.";
                    return null;
                }
                case TreeActions.Build:
                    if (s == null || !s.HasHouse || s.IsBuilt) return "Sürmekte olan bir yapım yok.";
                    if (c.Stage < AgeStage.Child) return "Bebek yardım edemez.";
                    return null;
                case TreeActions.Repair:
                    if (s == null || !s.IsBuilt) return "Onarılacak bir ağaç ev yok.";
                    if (s.IsVillageInn) return "Aşevine meyhaneci bakar.";
                    if (!IsAdult(c) && c.Stage != AgeStage.Teen) return "Onarımı büyükler yapar.";
                    return s.Durability >= RepairBelow ? "Ağaç ev sağlam." : null;
                case TreeActions.Play: case TreeActions.Lookout: case TreeActions.Siege:
                {
                    if (s == null || !s.IsBuilt) return "Henüz ağaç ev yok.";
                    if (s.IsRuined) return "Ağaç ev harap; önce onarılmalı.";
                    if (!IsMinor(c) && !(s.Kind == TreehouseKind.Kiosk && i.TreeAction == TreeActions.Play)) return "Burası çocukların yeri.";
                    if (_banned.Contains(c.Id)) return "Ağaç eve çıkması yasak.";
                    if (Snowy && !Covered(s.Kind)) return "Kar var; açık ağaç eve çıkılmaz.";
                    if (i.TreeAction == TreeActions.Lookout && s.Kind != TreehouseKind.Lookout) return "Bu bir gözcü kulesi değil.";
                    if (i.TreeAction == TreeActions.Siege)
                    {
                        if (s.Kind != TreehouseKind.Fort) return "Bu bir kale değil.";
                        if (Playmates(c, s).Count == 0) return "Kuşatma için en az bir oyun arkadaşı gerekir.";
                    }
                    return null;
                }
                case TreeActions.Sleep:
                    if (s == null || !s.IsUsable || s.Kind != TreehouseKind.Hut) return "Uyunacak bir kulübe yok.";
                    if (!IsMinor(c)) return "Kulübe çocuklar için.";
                    if (_banned.Contains(c.Id)) return "Ağaç eve çıkması yasak.";
                    if (_clock.Season == Season.Winter) return "Kışın kulübede uyunmaz.";
                    return null;
                case TreeActions.Stash:
                    if (s == null || !s.IsUsable || s.Kind != TreehouseKind.Hut) return "Saklanacak bir kulübe yok.";
                    if (!IsMinor(c)) return "Hazineyi çocuklar saklar.";
                    if (s.Stash != null) return "Kulübenin sandığında zaten bir hazine var.";
                    return StashCandidate() == null ? "Saklanacak küçük bir eşya yok (oyuncak, süs)." : null;
                case TreeActions.Dig:
                    if (s == null || !s.IsUsable || s.Kind != TreehouseKind.Hut) return "Burada hazine sandığı yok.";
                    if (!IsMinor(c)) return "Hazineyi çocuklar arar.";
                    if (s.Stash == null) return "Sandık boş.";
                    if (s.Stash.OwnerId == c.Id) return "Kendi sakladığını bulamazsın; yıllar geçmeli, başkası bulmalı.";
                    if (Now - s.Stash.Day < StashWaitDays) return "Sandık sıkı kapalı; daha zaman var.";
                    return null;
                case TreeActions.Carve:
                    if (c.Stage < AgeStage.Teen) return "Adını kazımak için ergen olmalı.";
                    foreach (var other in _sites.Values) if (other.CarvedIds.Contains(c.Id)) return "Adını zaten bir ulu ağaca kazıdı.";
                    return null;
                case TreeActions.Gather:
                    return s != null && s.LastGatherDay == Today ? "Bugün toplandı; yarın yine gel." : null;
                case TreeActions.Shade:
                    return null;
                case TreeActions.Dine:
                {
                    if (!inn) return "Burada aşevi yok.";
                    if (!InnOpenNow) return "Aşevi şu an kapalı.";
                    int price = PriceOf(i.Id);
                    return _house.Akce < price ? $"{price} akçe gerekir." : null;
                }
                case TreeActions.Charity:
                    if (!inn) return "Burada aşevi yok.";
                    if (!InnOpenNow) return "Aşevi şu an kapalı.";
                    if (_house.Akce >= CharityKitchen.CheapestMeal) return "Hayır sofrası parası olmayanlar için.";
                    return Kitchen.AteCharityToday(c.Id, Today) ? "Bugün hayır sofrasına oturdu; yarın yine gelebilir." : null;
                case TreeActions.Donate:
                {
                    if (!inn) return "Burada aşevi yok.";
                    if (c.Stage < AgeStage.Teen) return "Bağışı büyükler yapar.";
                    int amount = DonateAmount(i);
                    return _house.Akce < amount ? $"{amount} akçe yok." : null;
                }
                case TreeActions.DonateFood:
                    if (!inn) return "Burada aşevi yok.";
                    if (c.Stage < AgeStage.Teen) return "Bağışı büyükler yapar.";
                    return FoodForDonation().Count == 0 ? "Bağışlanacak yiyecek yok (ekmek, tahıl, sebze, pişmiş yemek)." : null;
                case TreeActions.CallDown:
                    if (!IsAdult(c)) return "Küsen çocuğu bir büyük çağırır.";
                    return SulkingAt(item.Id).Count == 0 ? "Ağaç evde küsen kimse yok." : null;
                case TreeActions.InnToggle:
                    if (s == null || !s.IsUsable || s.Kind != TreehouseKind.Kiosk || s.Builder != 0) return "Hanenin köşkü yok.";
                    if (!IsAdult(c)) return "Aşevini büyükler açar.";
                    return s.InnOpen ? null : InnRequirement(s);
            }
            return null;
        }

        public bool InnOpenNow => _clock.DayFraction >= InnOpens && _clock.DayFraction < InnCloses;

        /// <summary>Hazine saklama ile bulma arasında en az geçecek süre: iki takvim yılı (bir nesil).</summary>
        public double StashWaitDays => _clock.Config.DaysPerYear * 2.0;

        private string ItemName(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        /// <summary>Hazine adayı: oyuncak, sonra küçük süs (en ucuzu değil, en değerlisi).</summary>
        public string StashCandidate()
        {
            string best = null; int bestPrice = -1; bool bestToy = false;
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var d)) continue;
                bool toy = d.Category == "kids";
                bool trinket = d.Category == "decor" && d.Width <= 1 && d.Height <= 1;
                if (!toy && !trinket) continue;
                if (toy && !bestToy || toy == bestToy && d.Price > bestPrice) { best = d.Id; bestPrice = d.Price; bestToy = toy; }
            }
            return best;
        }

        /// <summary>Köşk aşevinin şartı (2 masa, 4 oturak, 1 tezgâh); sağlanıyorsa null.</summary>
        public string InnRequirement(TreehouseSite s)
        {
            var (tables, seats, counters) = CountFurniture(s);
            if (tables >= 2 && seats >= 4 && counters >= 1) return null;
            return $"Aşevi için köşkte 2 masa, 4 oturak ve 1 tezgâh gerekir (şimdi {tables} masa, {seats} oturak, {counters} tezgâh).";
        }

        public (int tables, int seats, int counters) CountFurniture(TreehouseSite s)
        {
            int tables = 0, seats = 0, counters = 0;
            if (s == null || s.KioskN <= 0) return (0, 0, 0);
            var room = new HashSet<GridPos>(KioskTiles(s));
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || it.IsWallMounted || !room.Contains(it.Position)) continue;
                string id = it.Def.Id;
                if (id.Contains("counter")) counters++;
                else if (it.Def.Category == "surfaces" && id.Contains("table")) tables++;
                else if (it.Def.Category == "seating") seats++;
            }
            return (tables, seats, counters);
        }

        // ---- etkiler ----

        /// <summary>Ağaç işinin etkisi (InteractionEffects.AppliedOn).</summary>
        public void OnApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (i?.TreeAction == null || item == null || c == null || !GreatTrees.IsGreatTree(item.Def)) return;
            var s = SiteOf(item);
            switch (i.TreeAction)
            {
                case TreeActions.Found: Found(c, item, FoundKind(i)); break;
                case TreeActions.Build: if (s != null) Work(c, item, s); break;
                case TreeActions.Repair: if (s != null) Repair(c, item, s); break;
                case TreeActions.Play: if (s != null) Play(c, item, s); break;
                case TreeActions.Lookout: if (s != null) Lookout(c, s); break;
                case TreeActions.Siege: if (s != null) Siege(c, s); break;
                case TreeActions.Sleep: Climbed(c); break;
                case TreeActions.Stash: if (s != null) StashItem(c, s); break;
                case TreeActions.Dig: if (s != null) Dig(c, item, s); break;
                case TreeActions.Carve: Carve(c, item); break;
                case TreeActions.Gather: Ensure(item).LastGatherDay = Today; break;
                case TreeActions.Shade:
                    if (_clock.Season == Season.Summer) c.AddMoodlet(ShadeMood, "Ulu ağacın gölgesi", 5, 0.5);
                    break;
                case TreeActions.Dine:
                {
                    int price = PriceOf(i.Id);
                    var meal = Meals.TryGetValue(i.Id, out var m) ? m : (MealPrice, "Manzaralı sofra", 5);
                    if (_house.TrySpend(price)) c.AddMoodlet(ViewMealMood, meal.Item2, meal.Item3, 0.5);
                    break;
                }
                case TreeActions.DonateFood:
                {
                    int value = 0, units = 0;
                    foreach (var (id, price) in FoodForDonation())
                    {
                        while (units < FoodDonationUnits && _house.Count(id) > 0 && _house.TryTakeItem(id)) { value += price; units++; }
                        if (units >= FoodDonationUnits) break;
                    }
                    if (units == 0) break;
                    int rep = Kitchen.Donate(value, Today, CharityKitchen.PlayerKey, HouseName);
                    if (rep > 0) AddReputation?.Invoke(rep, "aşevine bağış");
                    c.AddMoodlet(GoodDeedMood, "Hayırlı iş", 10, 1.0);
                    Say($"{c.Name} aşevine {units} parça yiyecek bağışladı ({value} akçelik). Kasada {Kitchen.Fund} akçe var.", c.Id);
                    break;
                }
                case TreeActions.CallDown: CallDown(c, item); break;
                case TreeActions.Charity:
                    if (Kitchen.ServeCharity(c.Id, Today))
                    {
                        c.Needs.Restore(NeedType.Hunger, 25f);
                        c.Needs.Restore(NeedType.Fun, 10f);
                        c.AddMoodlet(CharityGoodMood, "Hayır sofrasında yahni", 5, 0.5);
                    }
                    break;
                case TreeActions.Donate:
                {
                    int amount = DonateAmount(i);
                    if (!_house.TrySpend(amount)) break;
                    int rep = Kitchen.Donate(amount, Today, CharityKitchen.PlayerKey, HouseName);
                    if (rep > 0) AddReputation?.Invoke(rep, "aşevine bağış");
                    c.AddMoodlet(GoodDeedMood, "Hayırlı iş", 10, 1.0);
                    Say($"{c.Name} aşevine {amount} akçe bağışladı. Kasada {Kitchen.Fund} akçe var.", c.Id);
                    if (amount >= 100) Chronicle?.Invoke($"{c.Name}, Çınar Aşevi'ne {amount} akçe bağışladı.");
                    break;
                }
                case TreeActions.InnToggle:
                    if (s == null) break;
                    s.InnOpen = !s.InnOpen;
                    Say(s.InnOpen ? $"{HouseName} köşkünde aşevi açtı: köylüler yemeğe gelecek." : "Köşkteki aşevi kapandı.", c.Id);
                    break;
            }
        }

        private TreehouseSite Ensure(ItemInstance tree)
        {
            if (!_sites.TryGetValue(tree.Id, out var s)) { s = new TreehouseSite { TreeId = tree.Id }; _sites[tree.Id] = s; }
            return s;
        }

        private void Found(Character c, ItemInstance tree, TreehouseKind kind)
        {
            if (kind == TreehouseKind.None || _sites.ContainsKey(tree.Id) && _sites[tree.Id].Kind != TreehouseKind.None) return;
            if (kind == TreehouseKind.Kiosk && GreatTrees.KioskSize(tree.Def) == 5)
                foreach (var (id, n) in BigKioskExtra) for (int k = 0; k < n; k++) _house.TryTakeItem(id);
            var s = Ensure(tree);
            s.Kind = kind; s.Needed = WorkNeeded(kind); s.Progress = 0; s.Durability = 100;
            s.Builder = 0; s.BuilderName = HouseName;
            Say($"{c.Name}, {tree.Def.DisplayName.ToLowerInvariant()} dibine {KindName(kind)} iskeletini kurdu. Çocuklar da yardıma gelebilir.", c.Id);
            Work(c, tree, s);
        }

        /// <summary>Bir yapım oturumu: yetişkin 1, ergen 0,75, çocuk 0,5 iş.</summary>
        public static double WorkShare(Character c) => c.Stage == AgeStage.Child ? 0.5 : c.Stage == AgeStage.Teen ? 0.75 : 1.0;

        private void Work(Character c, ItemInstance tree, TreehouseSite s)
        {
            if (s.IsBuilt) return;
            s.Progress = Math.Min(s.Needed, s.Progress + WorkShare(c));
            LastWorker = c;
            if (!s.Helpers.Contains(c.Id)) s.Helpers.Add(c.Id);
            if (IsMinor(c)) { s.HadChild = true; Climbed(c); }
            if (s.IsBuilt) Complete(tree, s);
        }

        private void Complete(ItemInstance tree, TreehouseSite s)
        {
            s.BuiltDay = Now; s.Durability = 100; s.Warned = false; s.RuinedSince = -1;
            if (s.Kind == TreehouseKind.Kiosk) BuildKiosk(tree, s);
            bool together = false, adult = false;
            foreach (var id in s.Helpers) { var h = _population.Find(id); if (h == null) continue; if (IsMinor(h)) together = true; else adult = true; }
            together &= adult;
            if (together)
                foreach (var id in s.Helpers) _population.Find(id)?.AddMoodlet(BuiltTogetherMood, "Birlikte yaptık", 15, 2.0);
            if (s.Builder == 0 && !OnPlot(tree)) AddReputation?.Invoke(4, "köye ağaç ev");
            string where = tree.Def.DisplayName.ToLowerInvariant();
            Say($"{KindName(s.Kind)} bitti ({where}).{(together ? " Çocuklar da çivi çaktı." : "")}", 0);
            Chronicle?.Invoke($"{HouseName}, {where} üstüne {KindName(s.Kind)} kurdu.");
            if (s.Builder == 0 && LastWorker != null) Built?.Invoke(LastWorker, s);
        }

        private Character LastWorker;

        private void Repair(Character c, ItemInstance tree, TreehouseSite s)
        {
            bool wasRuined = s.IsRuined;
            s.Durability = 100; s.RuinedSince = -1; s.Warned = false;
            if (s.Builder != 0 && !OnPlot(tree)) AddReputation?.Invoke(3, "ortak ağaç evi onardı");
            if (wasRuined) Say($"{c.Name} harap {KindName(s.Kind).ToLowerInvariant()} evini onardı; çocuklar yine çıkabilir.", c.Id);
        }

        /// <summary>Tırmanan çocuk: küçük düşme şansı (Atletizm azaltır). Ölümcül değil.</summary>
        private void Climbed(Character c)
        {
            if (!IsMinor(c)) return;
            double p = FallChance * Math.Max(0.3, 1.0 - 0.1 * c.Skills.GetLevel(SkillType.Athletics));
            if (_rng.NextDouble() >= p) return;
            c.AddMoodlet(KneeMood, "Dizi sıyrıldı", -5, 0.5);
            if (Injure != null && _rng.NextDouble() < FallInjuryShare && Injure(c)) return;   // nadiren gerçek yara (iz kalabilir)
            Say($"{c.Name} merdivenden kaydı, dizi sıyrıldı.", c.Id);
        }

        /// <summary>Bugün bu ağaç evde olan oyun arkadaşları: hanedeki öbür çocuklar ve uğrayan köy çocukları.</summary>
        public List<Character> Playmates(Character c, TreehouseSite s)
        {
            var list = new List<Character>();
            foreach (var m in _population.Members)
                if (m != c && !m.IsDead && !m.IsVillager && !m.Away && IsMinor(m)) list.Add(m);
            foreach (var id in s.Visitors) { var v = _population.Find(id); if (v != null && !v.IsDead && v != c && !list.Contains(v)) list.Add(v); }
            return list;
        }

        private void Play(Character c, ItemInstance tree, TreehouseSite s)
        {
            Climbed(c);
            bool own = s.Builder == 0 && s.Helpers.Contains(c.Id);
            if (own) c.AddMoodlet(OwnFortMood, "Kendi kalemiz", 15, 1.0);
            else c.AddMoodlet(PlayMood, "Ağaç evde oynadı", 10, 1.0);
            if (c.Stage == AgeStage.Child) c.GainSkillXp(SkillType.Literacy, 3);   // hayal gücü (belge 52 §6)
            if (IsMinor(c)) { _plays[c.Id] = PlayCount(c.Id) + 1; ToyBoost?.Invoke(c, ToyBoostFactor); }
            if (s.Kind == TreehouseKind.Nest && _rng.NextDouble() < NestFeatherChance)
            {
                _house.AddItem(NestFeather, 1);   // kuş yuvasından tüy: yazı tüyü ve mürekkep zinciri (34-B5)
                Say($"{c.Name} dalların arasındaki eski bir yuvada tüy buldu.", c.Id);
            }
            foreach (var id in s.Visitors)
            {
                var v = _population.Find(id);
                if (v == null || v.IsDead) continue;
                AddAffection?.Invoke(c.Id, v.Id, 3);
            }
            if (s.Visitors.Count > 0 && _rng.NextDouble() < 0.3)
            {
                var v = _population.Find(s.Visitors[0]);
                if (v != null) Say($"{c.Name} ve {v.Name} {KindName(s.Kind).ToLowerInvariant()} evinde oynadı.", c.Id);
            }
        }

        private void Lookout(Character c, TreehouseSite s)
        {
            Climbed(c);
            c.AddMoodlet(PlayMood, "Ağaç evde oynadı", 10, 1.0);
            if (WolvesComing != null && WolvesComing())
                Alarm($"{c.Name} gözcü kulesinden ormanın kıyısında kurt izleri gördü: sürüyü içeri al!", c.Id);
            else if (Forecast != null && _rng.NextDouble() < 0.6)
                Say($"{c.Name} kuleden ufka baktı: {Forecast()}", c.Id);
            else if (_rng.NextDouble() < 0.4)
                Say($"{c.Name} kuleden uzakta otlayan bir karaca gördü.", c.Id);
        }

        private void Siege(Character c, TreehouseSite s)
        {
            Climbed(c);
            var mates = Playmates(c, s);
            var all = new List<Character>(mates) { c };
            var winner = all[_rng.Next(all.Count)];
            foreach (var m in mates) AddAffection?.Invoke(c.Id, m.Id, 3);
            foreach (var m in all)
            {
                if (m.IsVillager) continue;
                if (m == winner) m.AddMoodlet(FortWinnerMood, "Kalenin sahibi", 12, 1.0);
                else m.AddMoodlet(PlayMood, "Ağaç evde oynadı", 10, 1.0);
            }
            Say($"Kale kuşatması: {winner.Name} kaleyi tuttu ({all.Count} çocuk).", winner.Id);
        }

        /// <summary>Oyuncunun seçtiği hazine (bir sonraki "Hazine sakla" için); geçersizse kendiliğinden seçilir.</summary>
        public void ChooseStash(int characterId, string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) _chosenStash.Remove(characterId); else _chosenStash[characterId] = itemId;
        }

        /// <summary>Saklanabilecek eşyalar: oyuncaklar ve küçük süsler (değere göre, en çok max).</summary>
        public List<string> StashCandidates(int max = 6)
        {
            var list = new List<(string id, bool toy, int price)>();
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var d)) continue;
                bool toy = d.Category == "kids";
                if (!toy && !(d.Category == "decor" && d.Width <= 1 && d.Height <= 1)) continue;
                list.Add((d.Id, toy, d.Price));
            }
            list.Sort((a, b) => a.toy != b.toy ? (a.toy ? -1 : 1) : b.price.CompareTo(a.price));
            var ids = new List<string>();
            foreach (var x in list) { if (ids.Count >= max) break; ids.Add(x.id); }
            return ids;
        }

        private void StashItem(Character c, TreehouseSite s)
        {
            string id = _chosenStash.TryGetValue(c.Id, out var chosen) && _house.Count(chosen) > 0 ? chosen : StashCandidate();
            _chosenStash.Remove(c.Id);
            if (id == null || !_house.TryTakeItem(id)) return;
            s.Stash = new TreeStash { ItemId = id, OwnerId = c.Id, OwnerName = c.Name, Day = Now };
            Say($"{c.Name}, {ItemName(id).ToLowerInvariant()} eşyasını kulübenin sandığına sakladı. Yıllar sonra biri bulabilir.", c.Id);
        }

        /// <summary>a, b'nin atası mı (anne, baba, büyükanne, büyükbaba, …; 4 kuşağa kadar).</summary>
        public bool IsAncestor(int ancestorId, Character of, int depth = 4)
        {
            if (of == null || depth <= 0) return false;
            foreach (var pid in new[] { of.BirthMotherId, of.BirthFatherId })
            {
                if (!pid.HasValue) continue;
                if (pid.Value == ancestorId) return true;
                if (IsAncestor(ancestorId, _population.Find(pid.Value), depth - 1)) return true;
            }
            return false;
        }

        private void Dig(Character c, ItemInstance tree, TreehouseSite s)
        {
            var st = s.Stash;
            if (st == null) return;
            s.Stash = null;
            _house.AddItem(st.ItemId, 1);
            bool family = IsAncestor(st.OwnerId, c);
            string what = ItemName(st.ItemId).ToLowerInvariant();
            if (family)
            {
                string label = $"{st.OwnerName}'in hazinesi";
                GrantKeepsake?.Invoke(st.ItemId, label, c.Id);
                c.AddMoodlet(PlayMood, "Atasının hazinesini buldu", 20, 2.0);
                Say($"{c.Name} kulübenin sandığında {st.OwnerName}'in yıllar önce sakladığı {what} eşyasını buldu!", c.Id);
                Chronicle?.Invoke($"{c.Name}, {st.OwnerName}'in ağaç evde sakladığı hazineyi buldu.");
            }
            else
            {
                c.AddMoodlet(PlayMood, "Hazine buldu", 12, 1.0);
                Say($"{c.Name} kulübenin sandığında {st.OwnerName} adlı birinin sakladığı {what} eşyasını buldu.", c.Id);
            }
        }

        private void Carve(Character c, ItemInstance tree)
        {
            var s = Ensure(tree);
            if (s.CarvedIds.Contains(c.Id)) return;
            s.CarvedIds.Add(c.Id); s.CarvedNames.Add(c.Name);
            Say($"{c.Name} adını {tree.Def.DisplayName.ToLowerInvariant()} gövdesine kazıdı.", c.Id);
            c.AddChronicle($"Adını {tree.Def.DisplayName.ToLowerInvariant()} gövdesine kazıdı.");
            // Torun, atasının adını görür.
            foreach (var other in s.CarvedIds)
                if (other != c.Id && IsAncestor(other, c))
                {
                    var anc = _population.Find(other);
                    string name = anc?.Name ?? s.CarvedNames[s.CarvedIds.IndexOf(other)];
                    c.AddMoodlet(PlayMood, "Atasının adını buldu", 10, 1.0);
                    Say($"{c.Name}, kabukta {name} adını gördü: \"Bu bizimkilerden!\"", c.Id);
                    break;
                }
        }

        /// <summary>Ebeveyn yasağı: çocuk ağaç evlere çıkamaz (true) ya da yeniden çıkabilir.</summary>
        public void SetBanned(Character child, bool banned)
        {
            if (child == null) return;
            if (banned && _banned.Add(child.Id)) child.AddMoodlet(BannedMood, "Ağaç ev yasaklandı", -8, 1.0);
            else if (!banned) _banned.Remove(child.Id);
        }
        public bool IsBanned(Character c) => c != null && _banned.Contains(c.Id);

        /// <summary>Bunalımdaki çocuğun küseceği en yakın kullanılabilir ağaç ev (ip merdiveni); yoksa null.</summary>
        public GridPos? RefugeFor(Character c)
        {
            if (c == null || !IsMinor(c) || _banned.Contains(c.Id)) return null;
            GridPos? best = null; double bestD = RefugeRange; int bestTree = 0;
            foreach (var s in _sites.Values)
            {
                if (!s.IsUsable || s.IsVillageInn) continue;
                var tree = _world.GetItem(s.TreeId);
                if (tree == null) continue;
                var p = GreatTrees.LadderTile(tree);
                double d = Math.Abs(p.X - c.Position.X) + Math.Abs(p.Y - c.Position.Y);
                if (d < bestD) { bestD = d; best = p; bestTree = s.TreeId; }
            }
            if (best.HasValue) _sulking[c.Id] = bestTree;
            return best;
        }

        // ---- günlük ----

        /// <summary>Yeni gün: aşınma ve harabe, köy çocuklarının ziyareti, rakip kuruluş, köşk aşevi geliri, aşevinde yoksullar.</summary>
        public void OnNewDay()
        {
            var weather = WeatherToday != null ? WeatherToday() : WeatherType.Clear;
            var villageKids = new List<Character>();
            foreach (var m in _population.Members) if (m.IsVillager && !m.IsDead && IsMinor(m)) villageKids.Add(m);

            foreach (var s in new List<TreehouseSite>(_sites.Values))
            {
                var tree = _world.GetItem(s.TreeId);
                if (tree == null) { _sites.Remove(s.TreeId); continue; }
                s.Visitors.Clear();
                if (!s.IsBuilt || s.IsVillageInn) continue;
                Decay(tree, s, weather);
                if (_sites.ContainsKey(s.TreeId) && s.IsUsable && villageKids.Count > 0)
                {
                    int n = Math.Min(villageKids.Count, _rng.Next(0, 3));
                    for (int k = 0; k < n; k++)
                    {
                        var kid = villageKids[_rng.Next(villageKids.Count)];
                        if (!s.Visitors.Contains(kid.Id)) s.Visitors.Add(kid.Id);
                    }
                }
                if (s.Kind == TreehouseKind.Kiosk && s.InnOpen && s.Builder == 0 && s.IsUsable) RunPlayerInn(s);
            }
            if (_clock.DayOfSeason == 0) RivalBuilds();
            RivalRepairs();
            foreach (var st in _sites.Values) if (st.KioskN > 0) CheckLoad(st);
            Wishes(villageKids);
            FeedPoor(villageKids.Count > 0 || _population.Members.Count > 0);
            NeighborDonations();
            if (CharityKitchen.IsFeastDay(_clock.DayOfSeason, _clock.Config.DaysPerSeason) && _feastAnnounced != Today)
            {
                _feastAnnounced = Today;
                Say($"Bu akşam çınarın altında Bereket Sofrası kurulacak. Aşevi kasasında {Kitchen.Fund} akçe var.", 0);
            }
        }

        private void Decay(ItemInstance tree, TreehouseSite s, WeatherType weather)
        {
            if (s.IsRuined)
            {
                if (s.Kind != TreehouseKind.Kiosk && Now - s.RuinedSince >= _clock.Config.DaysPerYear)
                {
                    var was = s.Kind;
                    s.Kind = TreehouseKind.None; s.Progress = 0; s.Needed = 0; s.Durability = 100; s.RuinedSince = -1; s.Warned = false;
                    s.Helpers.Clear(); s.HadChild = false; s.Stash = null; s.Builder = 0; s.BuilderName = null; s.InnOpen = false;
                    if (was != TreehouseKind.None) Say($"{tree.Def.DisplayName} üstündeki harap {KindName(was).ToLowerInvariant()} çöktü; ağaç yeniden boş.", 0);
                }
                return;
            }
            double d = DecayPerDay + (_clock.Season == Season.Winter ? WinterDecay : 0) + (weather == WeatherType.Storm ? StormDecay : 0);
            s.Durability = Math.Max(0, s.Durability - d);
            bool ours = s.Builder == 0;
            if (s.Durability <= 0)
            {
                s.RuinedSince = Now;
                if (ours) Creaked?.Invoke();
                if (ours) Alarm($"{tree.Def.DisplayName} üstündeki {KindName(s.Kind).ToLowerInvariant()} harap oldu; onarılana dek kimse çıkamaz.", 0);
            }
            else if (ours && s.Durability < WarnAt && !s.Warned)
            {
                s.Warned = true;
                Creaked?.Invoke();
                Alarm($"{tree.Def.DisplayName} üstündeki {KindName(s.Kind).ToLowerInvariant()} gıcırdıyor: tahtalar çürüyor, onarılmalı.", 0);
            }
        }

        private void RivalBuilds()
        {
            if (Neighbors == null) return;
            var houses = new List<(int id, string name)>(Neighbors());
            if (houses.Count == 0) return;
            foreach (var it in _world.Items)
            {
                if (!GreatTrees.IsGreatTree(it.Def) || GreatTrees.IsInn(it.Def) || OnPlot(it)) continue;
                if (_sites.TryGetValue(it.Id, out var had) && had.HasHouse) continue;
                if (_rng.NextDouble() >= RivalBuildChance) continue;
                var h = houses[_rng.Next(houses.Count)];
                var kind = RivalKind();
                var s = Ensure(it);
                s.Kind = kind; s.Needed = WorkNeeded(kind); s.Progress = s.Needed; s.Durability = 100;
                s.Builder = h.id; s.BuilderName = h.name; s.BuiltDay = Now;
                if (kind == TreehouseKind.Kiosk) BuildKiosk(it, s);
                Say($"{h.name}, {it.Def.DisplayName.ToLowerInvariant()} üstüne {KindName(kind)} kurdu. Bütün çocuklar çıkabilir.", 0);
            }
        }

        /// <summary>Hanenin köşk aşevi: günün müşterileri hanenin pişmiş yemeğini alır (belge 52 §5.3).</summary>
        private void RunPlayerInn(TreehouseSite s)
        {
            if (InnRequirement(s) != null) { s.InnOpen = false; Say("Köşk aşevi kapandı: masa, oturak ya da tezgâh eksik.", 0); return; }
            int customers = InnCustomersBase + (_clock.Season == Season.Summer ? 2 : 0) - (_clock.Season == Season.Winter ? 2 : 0) + _rng.Next(0, 2);
            int sold = 0, earned = 0;
            for (int k = 0; k < customers; k++)
            {
                string meal = null; int price = 0;
                foreach (var kv in _house.InventoryList())
                {
                    if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var d) || !d.HasTag("cooked")) continue;
                    int p = Math.Max(3, (int)Math.Round(d.Price * 1.2));
                    if (p > price) { price = p; meal = d.Id; }
                }
                if (meal == null || !_house.TryTakeItem(meal)) break;
                _house.AddAkce(price); earned += price; sold++;
            }
            if (sold > 0) Say($"Köşk aşevi: {sold} müşteri, {earned} akçe (manzaralı sofra).", 0);
            else if (customers > 0) Say("Köşk aşevine müşteri geldi ama pişmiş yemek yoktu.", 0);
        }

        private void FeedPoor(bool any)
        {
            if (!any) return;
            var poor = new List<Character>();
            foreach (var m in _population.Members)
                if (m.IsVillager && !m.IsDead && m.Stage >= AgeStage.Child) poor.Add(m);
            if (poor.Count == 0) return;
            int want = 1 + _rng.Next(Math.Min(3, Math.Max(1, poor.Count / 6 + 1)));
            _poorToday.Clear();
            int fed = Kitchen.FeedPoor(want);
            if (fed == 0) return;
            bool playerGave = Kitchen.PlayerSeasonShare > 0;
            int season = (int)(_clock.DayIndex / _clock.Config.DaysPerSeason);
            for (int k = 0; k < fed; k++)
            {
                var v = poor[_rng.Next(poor.Count)];
                _poorToday.Add(v.Id);
                if (!playerGave || AddMemory == null) continue;
                if (_fedVillagers.TryGetValue(v.Id, out long last) && last == season) continue;   // köylü başına mevsimde bir kez
                _fedVillagers[v.Id] = season;
                AddMemory(v, 2, "aşevinde doydu");
            }
        }

        private void NeighborDonations()
        {
            if (Neighbors == null) return;
            foreach (var (id, name) in Neighbors())
                if (_rng.NextDouble() < 0.12) Kitchen.Donate(5 + 5 * _rng.Next(3), Today, id, name);
        }

        /// <summary>Gün içi: mevsimin son akşamı Bereket Sofrası (belge 52 §5.5). Kurulduysa sonucu döner.</summary>
        public FeastResult Tick()
        {
            if (!CharityKitchen.IsFeastDay(_clock.DayOfSeason, _clock.Config.DaysPerSeason)) return null;
            if (_clock.DayFraction < FeastHour || Kitchen.FeastHeldDay == Today) return null;
            if (!HasInn()) return null;
            var r = Kitchen.HoldFeast(Today);
            var guests = new List<Character>();
            foreach (var m in _population.Members)
                if (!m.IsDead && !m.IsVillager && !m.Away && m.Stage != AgeStage.Baby) guests.Add(m);
            var villagers = new List<Character>();
            foreach (var m in _population.Members) if (m.IsVillager && !m.IsDead && m.Stage >= AgeStage.Child) villagers.Add(m);
            foreach (var g in guests)
            {
                g.Needs.Restore(NeedType.Hunger, r.Hunger);
                g.Needs.Restore(NeedType.Social, 30f);
                g.AddMoodlet(FeastMood, "Aynı sofrada", r.MoodValue, 1.0);
                foreach (var o in guests) if (o.Id < g.Id) AddAffection?.Invoke(g.Id, o.Id, 2);
                for (int k = 0; k < Math.Min(6, villagers.Count); k++)
                {
                    var v = villagers[_rng.Next(villagers.Count)];
                    AddAffection?.Invoke(g.Id, v.Id, 3);
                }
            }
            string menu = r.Tier switch { 0 => "kuru ekmek", 1 => "ekmek ve çorba", 2 => "ekmek ve yahni", _ => "yahni ve pekmezli tatlı" };
            string host = r.HostKey == CharityKitchen.PlayerKey ? HouseName : r.HostName;
            if (r.HostKey == CharityKitchen.PlayerKey) AddReputation?.Invoke(CharityKitchen.HostReputation, "Bereket Sofrası sahibi");
            Say($"Bereket Sofrası kuruldu: bütün köy çınarın altında {menu} yedi.{(host != null ? $" Sofra sahibi: {host}." : "")}", 0);
            if (r.HostKey == CharityKitchen.PlayerKey) Chronicle?.Invoke($"{HouseName}, Bereket Sofrası'nın sahibi oldu ({menu}).");
            FeastHeld?.Invoke(r);
            return r;
        }

        private bool HasInn()
        {
            foreach (var s in _sites.Values) if (s.IsVillageInn) return true;
            return false;
        }

        // ---- ebeveyn, oyuncak, iz (belge 52 §3, §7) ----

        /// <summary>Bu ağaç evde küsen (bunalımdaki) hane çocukları.</summary>
        public List<Character> SulkingAt(int treeId)
        {
            var list = new List<Character>();
            foreach (var kv in _sulking)
            {
                if (kv.Value != treeId) continue;
                var ch = _population.Find(kv.Key);
                if (ch != null && !ch.IsDead && ch.InBreakdown) list.Add(ch);
            }
            return list;
        }

        /// <summary>"Çocuğu aşağı çağır": büyük ağacın dibinde konuşur; çocuğun bunalımı biter, ikisi yakınlaşır.</summary>
        private void CallDown(Character parent, ItemInstance tree)
        {
            foreach (var child in SulkingAt(tree.Id))
            {
                child.EndBreakdown(interrupted: true);
                _sulking.Remove(child.Id);
                child.AddMoodlet(CalledDownMood, "Ağaç evden indirildi, konuştular", 8, 1.0);
                parent.AddMoodlet(CalledDownMood, "Çocuğuyla konuştu", 5, 1.0);
                parent.GainSkillXp(SkillType.Parenting, 10);
                AddAffection?.Invoke(parent.Id, child.Id, 5);
                Say($"{parent.Name}, ağaç evde küsen {child.Name}'i aşağı çağırdı; biraz konuştular.", child.Id);
            }
        }
        public const string CalledDownMood = "mood.called_down";

        public int PlayCount(int characterId) => _plays.TryGetValue(characterId, out int n) ? n : 0;

        /// <summary>Genç yetişkinliğe geçerken çocuklukta yeterince ağaç evde oynadı mı ("Yükseklik sevdalısı").</summary>
        public bool EarnedHeightsMark(Character c) => c != null && PlayCount(c.Id) >= HeightsPlays;

        // ---- yiyecek bağışı ----

        /// <summary>Bağışlanabilecek yiyecekler (ucuzdan pahalıya; çiğ et ve bozulmuş hariç): kimlik, birim değer.</summary>
        public List<(string id, int price)> FoodForDonation()
        {
            var list = new List<(string, int)>();
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var d) || d.Price <= 0 || d.HasTag("raw")) continue;
                if (d.HasTag("food") || d.HasTag("cooked") || d.HasTag("grain") || d.HasTag("veg")) list.Add((d.Id, d.Price));
            }
            list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            return list;
        }

        // ---- köşk: taşıma sınırı, oda türü, manzara ----

        /// <summary>Köşkün taşıma sınırı: 4x4'te 40, 5x5'te 60; kestanede %10 az.</summary>
        public int KioskCapacity(TreehouseSite s)
        {
            if (s == null || s.KioskN <= 0) return 0;
            double cap = s.KioskN >= 5 ? KioskCapacity5 : KioskCapacity4;
            var tree = _world.GetItem(s.TreeId);
            if (tree != null && tree.Def.Id == GreatTrees.Chestnut) cap *= ChestnutCapacity;
            return (int)Math.Round(cap);
        }

        /// <summary>Eşyanın köşke yükü: tanımdaki ağırlık (varsayılan 1) ya da ayak izi; halı 1.</summary>
        public static int LoadOf(ItemInstance it)
        {
            if (it.IsRug) return 1;
            int w = Math.Max(1, it.Def.Weight);
            if (!it.IsDecor && !it.IsWallMounted && !it.IsCeiling) w = Math.Max(w, 2 * it.Width * it.Height);
            return w;
        }

        public int KioskLoad(TreehouseSite s)
        {
            if (s == null || s.KioskN <= 0) return 0;
            var tiles = new HashSet<GridPos>(KioskTiles(s));
            int load = 0;
            foreach (var it in _world.Items)
            {
                var at = it.IsDecor && it.ParentId.HasValue ? (_world.GetItem(it.ParentId.Value)?.Position ?? it.Position) : it.Position;
                if (tiles.Contains(at)) load += LoadOf(it);
            }
            return load;
        }

        /// <summary>
        /// Taşıma denetimi (eşya konunca ve her gün): %90'da "Dallar gıcırdıyor"; sınır aşılırsa en ağır eşya düşer,
        /// hasarlı olarak hane envanterine (raf çökmesi kuralı, belge 27 §16) ya da arsa dışındaysa yine haneye döner.
        /// </summary>
        public void CheckLoad(TreehouseSite s)
        {
            if (s == null || s.KioskN <= 0 || s.IsVillageInn) return;
            int cap = KioskCapacity(s);
            int load = KioskLoad(s);
            var tree = _world.GetItem(s.TreeId);
            string where = tree?.Def.DisplayName.ToLowerInvariant() ?? "ağaç";
            if (load <= cap)
            {
                if (load >= cap * LoadWarnAt)
                {
                    if (_loadWarned.Add(s.TreeId)) { Creaked?.Invoke(); Alarm($"{where} köşkünde dallar gıcırdıyor: köşk ağır ({load}/{cap}).", 0); }
                }
                else _loadWarned.Remove(s.TreeId);
                return;
            }
            var tiles = new HashSet<GridPos>(KioskTiles(s));
            while (load > cap)
            {
                ItemInstance heaviest = null;
                foreach (var it in _world.Items)
                    if (!it.IsDecor && tiles.Contains(it.Position) && (heaviest == null || LoadOf(it) > LoadOf(heaviest))) heaviest = it;
                if (heaviest == null) break;
                int before = load;
                var fallen = _world.RemoveItemInstances(heaviest.Id);
                foreach (var f in fallen) _house.AddItem(f.Def.Id, 1, ItemCondition.Damaged);
                load = KioskLoad(s);
                Creaked?.Invoke();
                Alarm($"{where} köşkü fazla yüklendi: {heaviest.Def.DisplayName.ToLowerInvariant()} dallardan aşağı düştü ve hasar gördü (envanterde).", 0);
                if (load >= before) break;
            }
            _world.NotifyChanged();
        }

        /// <summary>Eşya konunca (BuildService.Furnished): köşkteyse yük denetimi.</summary>
        public void OnFurnished(ItemInstance it)
        {
            if (it == null || it.Position.Z != 1) return;
            var at = it.IsDecor && it.ParentId.HasValue ? (_world.GetItem(it.ParentId.Value)?.Position ?? it.Position) : it.Position;
            var s = KioskAt(at);
            if (s != null) CheckLoad(s);
        }

        /// <summary>Karonun köşkü (z=1); yoksa null.</summary>
        public TreehouseSite KioskAt(GridPos p)
        {
            if (!_shell.Contains(p)) return null;
            foreach (var s in _sites.Values)
                if (s.KioskN > 0 && p.X >= s.KioskX && p.X < s.KioskX + s.KioskN && p.Y >= s.KioskY - 1 && p.Y < s.KioskY + s.KioskN) return s;
            return null;
        }

        public bool InKiosk(GridPos p) => _shell.Contains(p);

        /// <summary>Oda bir ağaç köşkü mü (HomeLife oda türü "Ağaç köşkü").</summary>
        public bool IsKioskRoom(Room room)
        {
            if (room == null || room.Tiles.Count == 0) return false;
            foreach (var t in room.Tiles) if (_shell.Contains(t)) return true;
            return false;
        }

        /// <summary>Köşkteki konforlu eşyaya manzara katkısı (MoodService.ExtraComfort).</summary>
        public int ComfortBonus(ItemInstance it) => it != null && !it.IsDecor && it.Def.Comfort > 0 && _shell.Contains(it.Position) ? ViewComfort : 0;

        private TreehouseKind RivalKind()
        {
            int r = _rng.Next(100);
            return r < 30 ? TreehouseKind.Nest : r < 55 ? TreehouseKind.Lookout : r < 75 ? TreehouseKind.Fort : r < 90 ? TreehouseKind.Hut : TreehouseKind.Kiosk;
        }

        /// <summary>Rakip hane kendi kurduğu ağaç evi onarır (hane yaşıyorsa); soyu biten hanenin evi harabeye döner.</summary>
        private void RivalRepairs()
        {
            if (Neighbors == null) return;
            var alive = new HashSet<int>();
            foreach (var (id, _) in Neighbors()) alive.Add(id);
            foreach (var s in _sites.Values)
            {
                if (s.Builder <= 0 || !s.IsBuilt || s.Durability >= 40 || !alive.Contains(s.Builder)) continue;
                if (_rng.NextDouble() >= RivalRepairChance) continue;
                bool wasRuined = s.IsRuined;
                s.Durability = 100; s.RuinedSince = -1; s.Warned = false;
                var tree = _world.GetItem(s.TreeId);
                if (wasRuined && tree != null) Say($"{s.BuilderName}, {tree.Def.DisplayName.ToLowerInvariant()} üstündeki {KindName(s.Kind).ToLowerInvariant()} evini onardı.", 0);
            }
        }

        // ---- istek: "Bize ağaç ev yapar mısın?" (belge 46) ----

        private void Wishes(List<Character> villageKids)
        {
            long season = _clock.DayIndex / Math.Max(1, _clock.Config.DaysPerSeason);
            if (_clock.DayOfSeason != 2) return;
            // Hanenin çocukları: hanenin kurduğu kullanılabilir ağaç ev yoksa mevsimde bir kez dile getirir.
            if (_wishSeason != season)
            {
                bool own = false;
                foreach (var st in _sites.Values) if (st.Builder == 0 && st.IsUsable) { own = true; break; }
                Character kid = null;
                foreach (var m in _population.Members)
                    if (!m.IsDead && !m.IsVillager && !m.Away && IsMinor(m)) { kid = m; break; }
                if (!own && kid != null)
                {
                    _wishSeason = season;
                    kid.AddMoodlet(WishMood, "Ağaç ev hayali", 0, 3.0);
                    Say($"{kid.Name}: \"Ulu ağaca bir ağaç ev yapar mısın? Ben de çivi tutarım!\"", kid.Id);
                }
            }
            // Köy çocuğunun ailesi: boş ulu ağaç varsa rica asar.
            if (_requestSeason == season || PostRequest == null || villageKids.Count == 0 || _rng.NextDouble() >= RequestChance) return;
            bool empty = false;
            foreach (var it in _world.Items)
                if (GreatTrees.IsGreatTree(it.Def) && !GreatTrees.IsInn(it.Def) && !OnPlot(it) && !(SiteOf(it)?.HasHouse ?? false)) { empty = true; break; }
            if (!empty) return;
            var child = villageKids[_rng.Next(villageKids.Count)];
            var giver = child.BirthMotherId.HasValue ? _population.Find(child.BirthMotherId.Value) : null;
            if (giver == null || giver.IsDead) giver = child.BirthFatherId.HasValue ? _population.Find(child.BirthFatherId.Value) : null;
            if (giver == null || giver.IsDead) giver = child;
            if (PostRequest(giver, child)) _requestSeason = season;
        }
        public const string WishMood = "mood.treehouse_wish";

        // ---- köylülerin yürüyüşü (oyun katmanı programa ekler) ----

        /// <summary>
        /// Köylünün şu anki ağaç ev yeri: mevsim sonu sofrası ("feast"), aşevinde öğle ("inn", bugün doyan yoksullar),
        /// ağaç eve uğrayan köy çocuğu ("tree:&lt;kimlik&gt;"); yoksa null (köylü programına uyar).
        /// </summary>
        public string VillagerPlace(Character c, double dayFraction, WeatherType weather)
        {
            if (c == null || c.IsDead || !c.IsVillager) return null;
            bool feastDay = CharityKitchen.IsFeastDay(_clock.DayOfSeason, _clock.Config.DaysPerSeason);
            if (feastDay && HasInn() && c.Stage >= AgeStage.Child && dayFraction >= FeastGatherFrom && dayFraction < FeastTo) return "feast";
            if (weather == WeatherType.Storm) return null;
            if (_poorToday.Contains(c.Id) && HasInn() && dayFraction >= LunchFrom && dayFraction < LunchTo) return "inn";
            if (weather == WeatherType.Rain || weather == WeatherType.Snow || !IsMinor(c)) return null;
            if (dayFraction < VisitFrom || dayFraction >= VisitTo) return null;
            foreach (var s in _sites.Values)
                if (s.IsUsable && !s.IsVillageInn && s.Visitors.Contains(c.Id)) return "tree:" + s.TreeId;
            return null;
        }

        /// <summary>VillagerPlace anahtarının karosu (merdiven başı, aşevi önü, sofra yeri); yoksa null.</summary>
        public GridPos? PlaceTile(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (key == "inn" || key == "feast")
            {
                foreach (var s in _sites.Values)
                {
                    if (!s.IsVillageInn) continue;
                    var tree = _world.GetItem(s.TreeId);
                    if (tree == null) return null;
                    var l = GreatTrees.LadderTile(tree);
                    return key == "inn" ? l : new GridPos(l.X + 2, l.Y - 1, 0);
                }
                return null;
            }
            if (key.StartsWith("tree:", StringComparison.Ordinal) && int.TryParse(key.Substring(5), out int id))
            {
                var tree = _world.GetItem(id);
                return tree != null ? GreatTrees.LadderTile(tree) : (GridPos?)null;
            }
            return null;
        }

        /// <summary>Köylü ağaç evin ya da aşevinin içinde mi (merdiven başına vardı): oyun katmanı çizmez.</summary>
        public bool VillagerInside(Character c, double dayFraction, WeatherType weather)
        {
            var key = VillagerPlace(c, dayFraction, weather);
            if (key == null || key == "feast") return false;
            var t = PlaceTile(key);
            return t.HasValue && t.Value.Z == c.Position.Z && Math.Abs(t.Value.X - c.Position.X) <= 1 && Math.Abs(t.Value.Y - c.Position.Y) <= 1;
        }

        /// <summary>Hane üyelerinin sofraya yürüme vakti (mevsimin son günü, sofradan hemen önce).</summary>
        public bool FeastGatherNow => HasInn() && CharityKitchen.IsFeastDay(_clock.DayOfSeason, _clock.Config.DaysPerSeason)
            && _clock.DayFraction >= FeastGatherFrom && _clock.DayFraction < FeastHour && Kitchen.FeastHeldDay != Today;

        // ---- kayıt ----

        public TreehouseSave Export()
        {
            var list = new List<TreehouseSiteSave>();
            foreach (var s in _sites.Values)
                list.Add(new TreehouseSiteSave
                {
                    treeId = s.TreeId, kind = (int)s.Kind, progress = s.Progress, needed = s.Needed, durability = s.Durability,
                    builder = s.Builder, builderName = s.BuilderName, builtDay = s.BuiltDay, ruinedSince = s.RuinedSince, warned = s.Warned,
                    innOpen = s.InnOpen, lastGatherDay = s.LastGatherDay, helpers = s.Helpers.ToArray(), hadChild = s.HadChild,
                    carvedNames = s.CarvedNames.ToArray(), carvedIds = s.CarvedIds.ToArray(),
                    stashItem = s.Stash?.ItemId, stashOwner = s.Stash?.OwnerId ?? 0, stashOwnerName = s.Stash?.OwnerName, stashDay = s.Stash?.Day ?? 0,
                    kioskX = s.KioskX, kioskY = s.KioskY, kioskN = s.KioskN
                });
            var pk = new List<int>(); var pv = new List<int>();
            foreach (var kv in _plays) { pk.Add(kv.Key); pv.Add(kv.Value); }
            return new TreehouseSave
            {
                sites = list.ToArray(), kitchen = Kitchen.Export(), banned = new List<int>(_banned).ToArray(), feastAnnounced = _feastAnnounced,
                playIds = pk.ToArray(), playCounts = pv.ToArray(), wishSeason = _wishSeason, requestSeason = _requestSeason
            };
        }

        public void Import(TreehouseSave data)
        {
            _sites.Clear(); _banned.Clear(); _fedVillagers.Clear(); _plays.Clear(); _sulking.Clear(); _poorToday.Clear(); _loadWarned.Clear(); _chosenStash.Clear();
            _wishSeason = data?.wishSeason ?? -1; _requestSeason = data?.requestSeason ?? -1;
            if (data?.playIds != null)
                for (int k = 0; k < data.playIds.Length && data.playCounts != null && k < data.playCounts.Length; k++) _plays[data.playIds[k]] = data.playCounts[k];
            Kitchen.Import(data?.kitchen);
            _feastAnnounced = data?.feastAnnounced ?? -1;
            if (data?.banned != null) foreach (var b in data.banned) _banned.Add(b);
            if (data?.sites != null)
                foreach (var d in data.sites)
                {
                    if (d == null) continue;
                    var s = new TreehouseSite
                    {
                        TreeId = d.treeId, Kind = Enum.IsDefined(typeof(TreehouseKind), d.kind) ? (TreehouseKind)d.kind : TreehouseKind.None,
                        Progress = d.progress, Needed = d.needed, Durability = d.durability, Builder = d.builder, BuilderName = d.builderName,
                        BuiltDay = d.builtDay, RuinedSince = d.ruinedSince, Warned = d.warned, InnOpen = d.innOpen, LastGatherDay = d.lastGatherDay,
                        HadChild = d.hadChild, KioskX = d.kioskX, KioskY = d.kioskY, KioskN = d.kioskN
                    };
                    if (d.helpers != null) s.Helpers.AddRange(d.helpers);
                    if (d.carvedNames != null) s.CarvedNames.AddRange(d.carvedNames);
                    if (d.carvedIds != null) s.CarvedIds.AddRange(d.carvedIds);
                    if (!string.IsNullOrEmpty(d.stashItem)) s.Stash = new TreeStash { ItemId = d.stashItem, OwnerId = d.stashOwner, OwnerName = d.stashOwnerName, Day = d.stashDay };
                    _sites[s.TreeId] = s;
                }
            Attach();
        }
    }

    [Serializable]
    public sealed class TreehouseSave
    {
        public TreehouseSiteSave[] sites;
        public CharityKitchenSave kitchen;
        public int[] banned;
        public long feastAnnounced = -1;
        public int[] playIds;
        public int[] playCounts;
        public long wishSeason = -1, requestSeason = -1;
    }

    [Serializable]
    public sealed class TreehouseSiteSave
    {
        public int treeId;
        public int kind;
        public double progress, needed, durability;
        public int builder;
        public string builderName;
        public double builtDay = -1, ruinedSince = -1;
        public bool warned, innOpen, hadChild;
        public long lastGatherDay = -1;
        public int[] helpers;
        public string[] carvedNames;
        public int[] carvedIds;
        public string stashItem;
        public int stashOwner;
        public string stashOwnerName;
        public double stashDay;
        public int kioskX, kioskY, kioskN;
    }
}
