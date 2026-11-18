using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Mining
{
    /// <summary>
    /// Kazı motoru (23. belge): arsa kazısı (evin altında kat -2) ve aile ocağı (tepe, kat -1). Karo karo kazılır; sertlik
    /// aletle ve Madencilik seviyesiyle aşılır. Dövüş yok: tehlike göçük, su ve karanlıktan gelir, her biri bir gün önceden
    /// görünür (çatırdama, yarının su derinliği). Göçük yaralar, öldürmez. Günde bir kez (sabah) NewDay çalışır.
    /// </summary>
    public sealed class MineSystem
    {
        public const string PlotId = "plot";
        public const string MineId = "mine-1";

        /// <summary>Bir vuruşun süresi (oyun günü): 4 oyun dakikası.</summary>
        public const double HitDays = 4.0 / 1440.0;
        /// <summary>Madencilik seviyesi başına vuruş hızı ve ek ürün şansı.</summary>
        public const double SkillSpeedPerLevel = 0.04, ExtraYieldPerLevel = 0.04;
        /// <summary>Sulu karonun günlük suyu; kovanın bir seferde boşalttığı.</summary>
        public const double WetFlowPerDay = 2, BucketWater = 2;
        /// <summary>Dren çukuru (arsa) ve su galerisi (ocak) günlük tahliyesi.</summary>
        public const double DrainPitPerDay = 6, AditPerDay = 30;
        /// <summary>Ahşap çatkı ömrü (gün; 1 yıl 40 gün) ve çürüdükten sonra bakım beklenen süre.</summary>
        public const double WoodSupportLife = 80, RotGraceDays = 10;
        /// <summary>Destek malzemesi.</summary>
        public const int WoodSupportPlanks = 2, StoneArchStones = 3;
        /// <summary>Ateşle kırma: odun ve duman süresi.</summary>
        public const int FireWood = 2;
        public const double SmokeDuration = 2.0 / 24.0;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly int _seed;
        private readonly Random _rng;
        private readonly Dictionary<string, MineMap> _sites = new Dictionary<string, MineMap>();
        private readonly Dictionary<string, MineTable> _tables = new Dictionary<string, MineTable>();
        private readonly List<(string site, int x, int y)> _fires = new List<(string, int, int)>();

        public MineSystem(WorldState world, Household house, int seed = 53)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _seed = seed;
            _rng = new Random(seed ^ 0x5A17);
        }

        // ---------- bağlantılar ----------

        /// <summary>Kazmanın kademesi (0 temel kazma: sertlik 3'e kadar; 1 demir: 5; 2+ çelik: hepsi). Null ise 0.</summary>
        public Func<Character, int> PickTier { get; set; }
        /// <summary>Kazmanın hız çarpanı (alet kademesi); null ise 1.</summary>
        public Func<Character, double> PickSpeed { get; set; }
        /// <summary>Ürünü depoya koyar (sığan adet döner); null ise hane envanterine.</summary>
        public Func<string, int, int> Store { get; set; }
        /// <summary>Bir karoda duran karakterler (arsa kazısında göçük yarası için); null ise kimse yok sayılır.</summary>
        public Func<GridPos, IEnumerable<Character>> CharactersAt { get; set; }
        /// <summary>Karakterin taşıdığı ışık yarıçapı (boynuz fener); null ise 0.</summary>
        public Func<Character, int> CarriedLight { get; set; }

        /// <summary>Kazıldı (karakter, yer, x, y, çıkanlar).</summary>
        public event Action<Character, MineMap, int, int, List<KeyValuePair<string, int>>> Dug;
        /// <summary>Kalıntı bulundu.</summary>
        public event Action<Character, string> RelicFound;
        /// <summary>Gün başında yeni çatırdayan karolar (yer, adet).</summary>
        public event Action<MineMap, int> Creaked;
        /// <summary>Göçük (yer, çöken karolar).</summary>
        public event Action<MineMap, List<(int x, int y)>> CaveIn;
        /// <summary>Göçük altında kalan karakter (yara: oyun katmanı yara sistemine verir; ölüm yok).</summary>
        public event Action<Character, MineMap> CaughtInCaveIn;
        /// <summary>Ahşap destek çürüdü (bakım ister) ya da bakımsız kalıp kalktı.</summary>
        public event Action<MineMap, int, int, bool> SupportRotted;

        public IEnumerable<MineMap> Sites => _sites.Values;
        public MineMap Get(string id) => id != null && _sites.TryGetValue(id, out var m) ? m : null;
        public MineTable TableOf(MineMap m) => m != null && _tables.TryGetValue(m.Id, out var t) ? t : null;

        // ---------- kazı yerleri ----------

        /// <summary>Arsa kazısı (harita boyunda, kat -2); yoksa üretir.</summary>
        public MineMap EnsurePlot()
        {
            var m = Get(PlotId);
            if (m != null) return m;
            m = new MineMap(PlotId, MineSiteKind.Plot, -2, _world.Map.Width, _world.Map.Height, Seeds.Derive(_seed, 101, 0));
            var table = MineTable.Plot();
            MineGen.Generate(m, table);
            // Önceden açılmış kat -2 karoları (kayıttan önce yapı) açık sayılır.
            foreach (var (x, y, t) in m.All()) if (_world.Map.HasTile(new GridPos(x, y, -2))) { t.Rock = RockKind.Open; t.Ore = null; }
            _sites[PlotId] = m;
            _tables[PlotId] = table;
            return m;
        }

        public const int MineWidth = 24, MineHeight = 24;

        /// <summary>Aile ocağının galeri katı (-1); yoksa üretir. Giriş ağzı güney kenarın ortasındadır.</summary>
        public MineMap EnsureMine()
        {
            var m = Get(MineId);
            if (m != null) return m;
            m = new MineMap(MineId, MineSiteKind.Mine, -1, MineWidth, MineHeight, Seeds.Derive(_seed, 102, 0));
            var table = MineTable.Gallery();
            MineGen.Generate(m, table);
            var (ex, ey) = MineEntrance;
            for (int dy = 0; dy <= 1; dy++) { var t = m.Get(ex, ey - dy); t.Rock = RockKind.Open; t.Ore = null; t.Wet = false; }
            m.Get(ex, ey).Support = SupportKind.Wood;        // giriş ağzının çatkısı
            _sites[MineId] = m;
            _tables[MineId] = table;
            return m;
        }

        public static (int x, int y) MineEntrance => (MineWidth / 2, MineHeight - 2);

        /// <summary>Arsada kuyu inişi: kat -2'de merdivenin indiği karo açılır (yapı modu merdivenle çağırır).</summary>
        public void OpenShaft(GridPos landing)
        {
            var m = EnsurePlot();
            var t = m.Get(landing.X, landing.Y);
            if (t == null) return;
            t.Rock = RockKind.Open;
            t.Ore = null;
            t.Wet = false;
        }

        /// <summary>Kat -2'ye inen bir kuyu açılabilir mi: Madencilik 2 (hanenin en iyisi) gerekir.</summary>
        public string ShaftBlocker(int miningLevel) =>
            miningLevel < MineTable.Plot().MinLevel ? $"Bodrumun altını kazmak için Madencilik {MineTable.Plot().MinLevel} gerekir." : null;

        // ---------- kurallar ----------

        public static int Hardness(RockKind r)
        {
            switch (r)
            {
                case RockKind.Soil: return 1;
                case RockKind.Soft: return 2;
                case RockKind.Stone: return 3;
                case RockKind.Hard: return 5;
                case RockKind.Bedrock: return 8;
                case RockKind.Rubble: return 1;
                default: return 0;
            }
        }

        /// <summary>Kazma kademesinin kazabildiği en sert karo.</summary>
        public static int MaxHardness(int tier) => tier <= 0 ? 3 : tier == 1 ? 5 : 8;

        public static string RockName(RockKind r)
        {
            switch (r)
            {
                case RockKind.Soil: return "toprak";
                case RockKind.Soft: return "yumuşak taş";
                case RockKind.Stone: return "taş";
                case RockKind.Hard: return "sert taş";
                case RockKind.Bedrock: return "ana kaya";
                case RockKind.Rubble: return "moloz";
                default: return "açık";
            }
        }

        private static GridPos WorldPos(MineMap m, int x, int y) => new GridPos(x, y, m.Level);

        /// <summary>Arsa kazısında üstündeki bodrum karosunda duvar, merdiven ya da büyük eşya var mı (destek ister)?</summary>
        public bool HouseAbove(int x, int y)
        {
            var above = new GridPos(x, y, -1);
            var map = _world.Map;
            if (!map.InBounds(above) || !map.HasTile(above)) return false;
            if (map.HasLinkAt(above)) return true;
            foreach (var (dx, dy) in MineMap.Four)
                if (map.GetEdge(GridMap.EdgeBetween(above, new GridPos(x + dx, y + dy, -1))) != EdgeKind.None) return true;
            var it = _world.GetItemAt(above);
            return it != null && (it.Def.IsStairs || it.Def.Width * it.Def.Height > 4);
        }

        /// <summary>Kazılamazsa nedeni; kazılabilirse null.</summary>
        public string CanDig(MineMap m, int x, int y, Character c)
        {
            var t = m?.Get(x, y);
            if (t == null) return "Kazı alanı dışında.";
            if (t.IsOpen) return "Orası zaten açık.";
            if (m.Kind == MineSiteKind.Plot && !_world.Map.IsOwned(new GridPos(x, y, 0))) return "Arsa sınırının dışı kazılmaz.";
            if (!m.TouchesOpen(x, y)) return "Önce yanına kadar kaz.";
            var table = TableOf(m);
            int level = c != null ? c.Skills.GetLevel(SkillType.Mining) : 1;
            if (table != null && level < table.MinLevel) return $"Madencilik {table.MinLevel} gerekir.";
            if (c != null && (c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen)) return "Çocuk ve ergen kazıya inmez.";
            if (t.Ore != null && table != null)
                foreach (var v in table.Veins) if (v.Item == t.Ore && level < v.MinLevel) return $"Bu damar için Madencilik {v.MinLevel} gerekir.";
            if (m.WaterDepth >= 2) return "Su basmış: önce suyu boşalt.";
            if (m.SmokeDays > 0) return "Galeri dumanlı: duman çekilene kadar bekle.";
            int hard = t.Cracked ? 1 : Hardness(t.Rock);
            int tier = c != null && PickTier != null ? PickTier(c) : 0;
            if (hard > MaxHardness(tier)) return t.Rock == RockKind.Bedrock ? "Ana kaya: çelik kazma ya da ateşle kırma gerekir." : "Kazman bu kadar sert taşa yetmiyor (kazmayı demircide yükselt ya da ateşle kır).";
            if (m.Kind == MineSiteKind.Plot && HouseAbove(x, y) && !m.SupportWithin(x, y, 2)) return "Üstünde evin duvarı ya da büyük eşya var: önce 2 karo yakına destek kur.";
            return null;
        }

        /// <summary>Kazma süresi (oyun günü): vuruş x 4 dk / alet / beceri; karanlıkta 2 kat, ıslakta %20 yavaş.</summary>
        public double DigDays(MineMap m, int x, int y, Character c)
        {
            var t = m.Get(x, y);
            int hits = t == null ? 1 : t.Cracked ? 1 : Math.Max(1, Hardness(t.Rock));
            double speed = c != null && PickSpeed != null ? Math.Max(0.25, PickSpeed(c)) : 1.0;
            int level = c != null ? c.Skills.GetLevel(SkillType.Mining) : 1;
            double days = hits * HitDays / speed / (1.0 + SkillSpeedPerLevel * Math.Max(0, level - 1));
            if (!Lit(m, x, y, c)) days *= 2;
            if (m.WaterDepth == 1) days *= 1.25;
            return days;
        }

        private bool Lit(MineMap m, int x, int y, Character c)
        {
            int carried = c != null && CarriedLight != null ? CarriedLight(c) : 0;
            // Arsa kazısında karakterin karosu dünyada; ocakta karakter karonun yanında sayılır.
            return m.IsLit(x, y, x, y, carried);
        }

        /// <summary>Karoyu kazar: çıkanları depoya koyar, XP verir. Çıkanların listesi döner (kazılamazsa boş).</summary>
        public List<KeyValuePair<string, int>> Dig(MineMap m, int x, int y, Character c)
        {
            var got = new List<KeyValuePair<string, int>>();
            if (CanDig(m, x, y, c) != null) return got;
            var t = m.Get(x, y);
            var table = TableOf(m);
            int level = c != null ? c.Skills.GetLevel(SkillType.Mining) : 1;
            int hard = Hardness(t.Rock);
            float xp = hard;
            void Give(string id, int n)
            {
                if (n <= 0) return;
                int added = Store != null ? Store(id, n) : AddAll(id, n);
                if (added > 0) got.Add(new KeyValuePair<string, int>(id, added));
            }
            if (t.Ore != null)
            {
                int n = 1 + (_rng.NextDouble() < ExtraYieldPerLevel * Math.Max(0, level - 1) ? 1 : 0);
                if (t.Rock == RockKind.Rubble) n = _rng.NextDouble() < 0.5 ? 1 : 0;        // göçük molozunda yarısı
                Give(t.Ore, n);
                xp *= 2;
            }
            else if (t.Rock == RockKind.Soil) { if (_rng.NextDouble() < 0.2) Give("item.clay", 1); }
            else if (t.Rock == RockKind.Soft || t.Rock == RockKind.Rubble) Give("item.rubble", 1);
            else Give("item.stone", 1);
            if (t.Rock >= RockKind.Stone && t.Rock <= RockKind.Bedrock && table != null && table.Relics.Length > 0 && _rng.NextDouble() < table.RelicChance)
            {
                var relic = table.Relics[_rng.Next(table.Relics.Length)];
                Give(relic, 1);
                xp += 10;
                if (c != null) RelicFound?.Invoke(c, relic);
            }
            bool wet = t.Wet;
            t.Rock = RockKind.Open;
            t.Ore = null;
            t.Cracked = false;
            t.Marked = false;
            t.Wet = wet;
            if (m.Kind == MineSiteKind.Plot)
            {
                var p = WorldPos(m, x, y);
                _world.Map.SetTile(p, true);
                _world.Map.SetFloor(p, FloorType.Dirt);
            }
            c?.GainSkillXp(SkillType.Mining, xp);
            Dug?.Invoke(c, m, x, y, got);
            return got;
        }

        private int AddAll(string id, int n) { _house.AddItem(id, n); return n; }

        public void Mark(MineMap m, int x, int y, bool on)
        {
            var t = m?.Get(x, y);
            if (t != null && !t.IsOpen) t.Marked = on;
        }

        // ---------- destek, ışık, su, ateş ----------

        public string CanSupport(MineMap m, int x, int y, SupportKind kind, Character c = null)
        {
            var t = m?.Get(x, y);
            if (t == null || !t.IsOpen) return "Destek açık bir karoya kurulur.";
            if (t.Support != SupportKind.None && !(t.Support == SupportKind.Wood && t.SupportAge >= WoodSupportLife)) return "Burada zaten destek var.";
            if (kind == SupportKind.Wood && _house.Count("item.plank") < WoodSupportPlanks) return $"{WoodSupportPlanks} tahta gerekir.";
            if (kind == SupportKind.StoneArch)
            {
                if (_house.Count("item.cut_stone") < StoneArchStones) return $"{StoneArchStones} yontma taş gerekir.";
                if (c != null && c.Skills.GetLevel(SkillType.Masonry) < 4) return "Taş kemer için Taş ustalığı 4 gerekir.";
            }
            return null;
        }

        /// <summary>Destek kurar (çürüyen çatkıyı yeniler); çatırdayan karo susar.</summary>
        public bool PlaceSupport(MineMap m, int x, int y, SupportKind kind, Character c = null)
        {
            if (kind == SupportKind.None || CanSupport(m, x, y, kind, c) != null) return false;
            if (kind == SupportKind.Wood) for (int i = 0; i < WoodSupportPlanks; i++) _house.TryTakeItem("item.plank");
            else for (int i = 0; i < StoneArchStones; i++) _house.TryTakeItem("item.cut_stone");
            var t = m.Get(x, y);
            t.Support = kind;
            t.SupportAge = 0;
            int d = TableOf(m)?.SupportDistance ?? 2;
            for (int ty = y - d; ty <= y + d; ty++) for (int tx = x - d; tx <= x + d; tx++) { var n = m.Get(tx, ty); if (n != null) n.Creaking = false; }
            c?.GainSkillXp(SkillType.Mining, 3);
            return true;
        }

        /// <summary>Işıklar: iç yağı mumu (2, 6 saat), balmumu (2, 10 saat), kandil (3, 1 gün), meşale (4, 4 saat).</summary>
        public static bool LightOf(string itemId, out int radius, out double days)
        {
            switch (itemId)
            {
                case "item.tallow_candle": case "item.candle": radius = 2; days = 0.25; return true;
                case "item.beeswax_candle": radius = 2; days = 10.0 / 24; return true;
                case "item.clay_lamp": radius = 3; days = 1.0; return true;
                case "item.torch": radius = 4; days = 4.0 / 24; return true;
                default: radius = 0; days = 0; return false;
            }
        }

        public string CanLight(MineMap m, int x, int y, string itemId)
        {
            var t = m?.Get(x, y);
            if (t == null || !t.IsOpen) return "Işık açık bir karoya konur.";
            if (!LightOf(itemId, out _, out _)) return "Bu ışık olmaz.";
            if (_house.Count(itemId) <= 0) return "Envanterde yok.";
            return null;
        }

        public bool PlaceLight(MineMap m, int x, int y, string itemId)
        {
            if (CanLight(m, x, y, itemId) != null) return false;
            LightOf(itemId, out var r, out var d);
            _house.TryTakeItem(itemId);
            var t = m.Get(x, y);
            t.LightRadius = r;
            t.LightDays = d;
            return true;
        }

        /// <summary>Kovayla su boşaltır (bir sefer).</summary>
        public bool Bail(MineMap m, Character c = null)
        {
            if (m == null || m.Water <= 0) return false;
            m.Water = Math.Max(0, m.Water - BucketWater);
            c?.GainSkillXp(SkillType.Mining, 0.5f);
            return true;
        }

        /// <summary>Ateşle kırma (yalnızca ocakta): sert taşa ya da ana kayaya karşı odun yakılır, ertesi sabah karo çatlar.</summary>
        public string CanFireSet(MineMap m, int x, int y)
        {
            var t = m?.Get(x, y);
            if (t == null || t.IsOpen) return "Ateş açık olmayan bir karoya karşı yakılır.";
            if (m.Kind == MineSiteKind.Plot) return "Evin altında ateş yakılmaz: duman eve dolar.";
            if (t.Rock != RockKind.Hard && t.Rock != RockKind.Bedrock) return "Ateşle kırma yalnızca sert taşta ve ana kayada işe yarar.";
            if (!m.TouchesOpen(x, y)) return "Önce yanına kadar kaz.";
            if (_house.Count("item.firewood") < FireWood) return $"{FireWood} odun gerekir.";
            return null;
        }

        public bool FireSet(MineMap m, int x, int y)
        {
            if (CanFireSet(m, x, y) != null) return false;
            for (int i = 0; i < FireWood; i++) _house.TryTakeItem("item.firewood");
            _fires.Add((m.Id, x, y));
            m.SmokeDays = Math.Max(m.SmokeDays, SmokeDuration);
            return true;
        }

        /// <summary>Su galerisi: ocağın batı (yamaç) kenarına ulaşan açık karo varsa su kendiliğinden akar.</summary>
        public static bool HasAdit(MineMap m)
        {
            if (m == null || m.Kind != MineSiteKind.Mine) return false;
            for (int y = 1; y < m.Height - 1; y++) if (m.IsOpen(1, y)) return true;
            return false;
        }

        /// <summary>Arsadaki dren çukurları (kat -2'deki "drain" etiketli eşyalar).</summary>
        public int DrainPits()
        {
            int n = 0;
            foreach (var it in _world.Items) if (!it.IsDecor && it.Position.Z == -2 && it.Def.HasTag("drain")) n++;
            return n;
        }

        /// <summary>Bir sonraki sabah suyun ne olacağı (arayüz: "yarın: ıslak").</summary>
        public int TomorrowDepth(MineMap m)
        {
            double w = Math.Max(0, m.Water + Inflow(m) - Drain(m));
            int open = m.OpenCount;
            return open == 0 ? 0 : (int)Math.Min(3, Math.Floor(w / open));
        }

        private static double Inflow(MineMap m)
        {
            double n = 0;
            foreach (var (_, _, t) in m.All()) if (t.IsOpen && t.Wet) n += WetFlowPerDay;
            return n;
        }

        private double Drain(MineMap m) => m.Kind == MineSiteKind.Plot ? DrainPits() * DrainPitPerDay : HasAdit(m) ? AditPerDay : 0;

        // ---------- günlük döngü ----------

        /// <summary>Işıklar yanar biter, duman çekilir.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            foreach (var m in _sites.Values)
            {
                if (m.SmokeDays > 0) m.SmokeDays = Math.Max(0, m.SmokeDays - gameDays);
                foreach (var (_, _, t) in m.All())
                    if (t.LightDays > 0) { t.LightDays = Math.Max(0, t.LightDays - gameDays); if (t.LightDays <= 0) t.LightRadius = 0; }
            }
        }

        /// <summary>Gün başı: ateşler çatlatır, destekler yaşlanır, su gelir ve tahliye olur; çatırdayan karo desteklenmediyse çöker, yeni çatırdamalar başlar.</summary>
        public void NewDay(double days = 1.0)
        {
            foreach (var (site, x, y) in _fires)
            {
                var t = Get(site)?.Get(x, y);
                if (t != null && !t.IsOpen) t.Cracked = true;
            }
            _fires.Clear();
            var ids = new List<string>(_sites.Keys);
            ids.Sort(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                var m = _sites[id];
                var table = TableOf(m);
                int dist = table?.SupportDistance ?? 2;
                foreach (var (x, y, t) in m.All())
                {
                    if (t.Support != SupportKind.Wood) continue;
                    double before = t.SupportAge;
                    t.SupportAge += days;
                    if (before < WoodSupportLife && t.SupportAge >= WoodSupportLife) SupportRotted?.Invoke(m, x, y, false);
                    if (t.SupportAge >= WoodSupportLife + RotGraceDays) { t.Support = SupportKind.None; t.SupportAge = 0; SupportRotted?.Invoke(m, x, y, true); }
                }
                m.Water = Math.Max(0, m.Water + (Inflow(m) - Drain(m)) * days);
                // Yer altı sarnıcı (arsa kazısı): kazının suyunu toplar, donmaz.
                if (m.Kind == MineSiteKind.Plot && m.Water > 0)
                    foreach (var it in _world.Items)
                    {
                        if (it.IsDecor || it.Position.Z != m.Level || !it.Def.IsCistern) continue;
                        int take = (int)Math.Min(Math.Floor(m.Water), it.Def.CisternCapacity - it.Fill);
                        if (take <= 0) continue;
                        it.Fill += take;
                        m.Water -= take;
                    }

                // Dün çatırdayan ve hâlâ gevşek karo çöker (kendisi ve çevresindeki açık karolar molozla dolar).
                var collapsed = new List<(int x, int y)>();
                foreach (var (x, y, t) in m.All())
                {
                    if (!t.Creaking) continue;
                    t.Creaking = false;
                    if (!m.IsLoose(x, y, dist)) continue;
                    Collapse(m, x, y, collapsed);
                }
                if (collapsed.Count > 0) CaveIn?.Invoke(m, collapsed);

                int creaks = 0;
                foreach (var (x, y, t) in m.All())
                    if (m.IsLoose(x, y, dist) && _rng.NextDouble() < (table?.CreakChance ?? 0.1)) { t.Creaking = true; creaks++; }
                if (creaks > 0) Creaked?.Invoke(m, creaks);
            }
        }

        private void Collapse(MineMap m, int cx, int cy, List<(int x, int y)> collapsed)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    var t = m.Get(x, y);
                    if (t == null || !t.IsOpen) continue;
                    // Destekli karo ve kuyu inişi (merdiven) çökmez.
                    if (t.Support != SupportKind.None) continue;
                    if (m.Kind == MineSiteKind.Plot && _world.Map.HasLinkAt(WorldPos(m, x, y))) continue;
                    if (m.Kind == MineSiteKind.Mine && (x, y) == MineEntrance) continue;
                    t.Rock = RockKind.Rubble;
                    t.LightRadius = 0; t.LightDays = 0;
                    collapsed.Add((x, y));
                    if (m.Kind != MineSiteKind.Plot) continue;
                    var p = WorldPos(m, x, y);
                    if (CharactersAt != null) foreach (var c in CharactersAt(p)) CaughtInCaveIn?.Invoke(c, m);
                    // Göçen karodaki eşya hane envanterine döner (kaybolmaz), karo haritadan kalkar.
                    var it = _world.GetItemAt(p);
                    if (it != null && !it.Def.IsStairs) { _world.RemoveItem(it.Id); _house.AddItem(it.Def.Id); }
                    if (_world.GetItemAt(p) == null) _world.Map.SetTile(p, false);
                    else t.Rock = RockKind.Open;
                }
        }

        /// <summary>
        /// Hazine odası (karar 12): kat -2'deki, içinde hazine sandığı olan kapalı oda. Buradaki yadigârlar yıkımla ve göçükle
        /// kaybolmaz (oyun katmanı Legacy.HeirloomOf ile birleştirir).
        /// </summary>
        public static bool InTreasureRoom(WorldState world, ItemInstance item)
        {
            if (world == null || item == null || item.Position.Z > -2) return false;
            var room = world.Map.Rooms.RoomAt(item.Position);
            if (room == null || !room.IsIndoor) return false;
            var tiles = new HashSet<GridPos>(room.Tiles);
            foreach (var it in world.Items)
                if (!it.IsDecor && it.Def.HasTag("treasure_chest") && tiles.Contains(it.Position)) return true;
            return false;
        }

        // ---------- ekran dışı çalışma (ocak) ----------

        /// <summary>
        /// Ocakta görevli/oyuncu vardiyası (ekran dışı): bu kadar oyun günü içinde işaretli karoları sırayla kazar; su varsa
        /// önce boşaltır, çatırdayan karoya tahta varsa destek kurar. Kazılan karo sayısı döner.
        /// </summary>
        public int Shift(MineMap m, Character c, double days)
        {
            if (m == null || c == null) return 0;
            double left = days;
            int dug = 0;
            int dist = TableOf(m)?.SupportDistance ?? 2;
            for (int guard = 0; guard < 400 && left > 1e-9; guard++)
            {
                // Çatırdayan karo varsa önce destek (ocakbaşı işi).
                bool did = false;
                foreach (var (x, y, t) in m.All())
                {
                    if (!t.Creaking) continue;
                    if (PlaceSupport(m, x, y, SupportKind.Wood, c)) { left -= 0.03; did = true; break; }
                }
                if (did) continue;
                if (m.WaterDepth >= 1 && m.Water > 0) { Bail(m, c); left -= 0.02; continue; }
                (int x, int y) best = (-1, -1);
                double bestD = double.MaxValue;
                foreach (var (x, y, t) in m.All())
                {
                    if (!t.Marked || CanDig(m, x, y, c) != null) continue;
                    double d = Math.Abs(x - MineEntrance.x) + Math.Abs(y - MineEntrance.y);
                    if (d < bestD) { bestD = d; best = (x, y); }
                }
                if (best.x < 0) break;
                double need = DigDays(m, best.x, best.y, c);
                if (need > left) break;
                Dig(m, best.x, best.y, c);
                left -= need;
                dug++;
                _ = dist;
            }
            return dug;
        }

        // ---------- kayıt ----------

        public List<MineSave> Export()
        {
            var list = new List<MineSave>();
            var ids = new List<string>(_sites.Keys);
            ids.Sort(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                var m = _sites[id];
                var s = new MineSave { id = m.Id, kind = (int)m.Kind, level = m.Level, w = m.Width, h = m.Height, seed = m.Seed, water = m.Water, smoke = m.SmokeDays };
                var rock = new int[m.Width * m.Height];
                var flags = new int[rock.Length];
                var ore = new string[rock.Length];
                var age = new double[rock.Length];
                var light = new double[rock.Length];
                int i = 0;
                foreach (var (_, _, t) in m.All())
                {
                    rock[i] = (int)t.Rock;
                    flags[i] = (t.Wet ? 1 : 0) | (t.Cracked ? 2 : 0) | (t.Marked ? 4 : 0) | (t.Creaking ? 8 : 0) | ((int)t.Support << 4) | (t.LightRadius << 8);
                    ore[i] = t.Ore;
                    age[i] = t.SupportAge;
                    light[i] = t.LightDays;
                    i++;
                }
                s.rock = rock; s.flags = flags; s.ore = ore; s.supportAge = age; s.light = light;
                list.Add(s);
            }
            return list;
        }

        public void Restore(IEnumerable<MineSave> saves)
        {
            if (saves == null) return;
            foreach (var s in saves)
            {
                if (s == null || s.w <= 0 || s.h <= 0 || s.rock == null || s.rock.Length != s.w * s.h) continue;
                var m = new MineMap(s.id, (MineSiteKind)s.kind, s.level, s.w, s.h, s.seed) { Water = Math.Max(0, s.water), SmokeDays = Math.Max(0, s.smoke) };
                int i = 0;
                foreach (var (_, _, t) in m.All())
                {
                    t.Rock = (RockKind)s.rock[i];
                    int f = s.flags != null && i < s.flags.Length ? s.flags[i] : 0;
                    t.Wet = (f & 1) != 0; t.Cracked = (f & 2) != 0; t.Marked = (f & 4) != 0; t.Creaking = (f & 8) != 0;
                    t.Support = (SupportKind)((f >> 4) & 0xF);
                    t.LightRadius = (f >> 8) & 0xF;
                    t.Ore = s.ore != null && i < s.ore.Length && !string.IsNullOrEmpty(s.ore[i]) ? s.ore[i] : null;
                    t.SupportAge = s.supportAge != null && i < s.supportAge.Length ? s.supportAge[i] : 0;
                    t.LightDays = s.light != null && i < s.light.Length ? s.light[i] : 0;
                    i++;
                }
                _sites[m.Id] = m;
                _tables[m.Id] = m.Kind == MineSiteKind.Plot ? MineTable.Plot() : MineTable.Gallery();
            }
        }
    }

    /// <summary>Bir kazı yerinin kaydı (karo başına diziler).</summary>
    [Serializable]
    public sealed class MineSave
    {
        public string id;
        public int kind, level, w, h, seed;
        public double water, smoke;
        public int[] rock;
        public int[] flags;          // bit 0 sulu, 1 çatlak, 2 işaretli, 3 çatırdıyor, 4-7 destek, 8-11 ışık yarıçapı
        public string[] ore;
        public double[] supportAge;
        public double[] light;
    }
}
