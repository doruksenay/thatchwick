using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Bir harita bölgesi (Stardew gibi ayrı harita): dünya ızgarasında kendi dikdörtgeni.</summary>
    public sealed class Region
    {
        public string Id; public string Name; public int X, Y, W, H;
        public bool Contains(GridPos p) => p.X >= X && p.Y >= Y && p.X < X + W && p.Y < Y + H;
    }

    /// <summary>Tamsayı dikdörtgen (Unity'den bağımsız çekirdek için).</summary>
    public readonly struct RectI
    {
        public readonly int X, Y, W, H;
        public RectI(int x, int y, int w, int h) { X = x; Y = y; W = w; H = h; }
        public bool Contains(GridPos p) => p.X >= X && p.Y >= Y && p.X < X + W && p.Y < Y + H;
    }

    /// <summary>Başlangıçta haritaya konacak eşya (dünya koordinatı). WallSide &gt;= 0: duvara asılır (0 kuzey, 3 batı);
    /// Slot != null: (X,Y)'deki yüzeyin yuvasına konur; Grown: olgun bitki/ağaç (yol ve taş zemine konmaz).</summary>
    public struct RegionItem
    {
        public string Id; public int X, Y; public bool Grown; public int WallSide; public string Slot;
        public RegionItem(string id, int x, int y, bool grown = false, int wallSide = -1, string slot = null)
        { Id = id; X = x; Y = y; Grown = grown; WallSide = wallSide; Slot = slot; }
    }

    /// <summary>Bölgenin yerel koordinatla çizimi (ox, oy: bölgenin dünya köşesi).</summary>
    internal sealed class Painter
    {
        private readonly GridMap _m; private readonly int _ox, _oy;
        public readonly List<RegionItem> Items = new List<RegionItem>();
        public Painter(GridMap m, int ox, int oy) { _m = m; _ox = ox; _oy = oy; }
        public GridPos P(int x, int y) => new GridPos(_ox + x, _oy + y);

        public void Floor(int x0, int y0, int x1, int y1, FloorType f)
        {
            if (_m == null) return;
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) _m.SetFloor(P(x, y), f);
        }

        public void RoadTile(int x, int y)
        {
            if (_m == null) return;
            var p = P(x, y);
            var home = Regions.At(P(0, 0));
            if (home == null || !home.Contains(p)) return;   // yol bölge dışına taşmaz
            var f = _m.GetFloor(p);
            if (f == FloorType.Grass || f == FloorType.Dirt) _m.SetRoad(p, RoadType.Path);
        }

        /// <summary>Kıvrılan yol: eksene paralel noktalar arasından, 'width' karo genişliğinde.</summary>
        public void Path(int width, params (int x, int y)[] pts)
        {
            for (int i = 0; i + 1 < pts.Length; i++)
            {
                var (ax, ay) = pts[i]; var (bx, by) = pts[i + 1];
                int x0 = Math.Min(ax, bx), x1 = Math.Max(ax, bx), y0 = Math.Min(ay, by), y1 = Math.Max(ay, by);
                // L biçimli: önce yatay, sonra dikey.
                for (int x = x0; x <= x1; x++) for (int w = 0; w < width; w++) RoadTile(x, ay + w);
                for (int y = y0; y <= y1; y++) for (int w = 0; w < width; w++) RoadTile(bx + w, y);
            }
        }

        /// <summary>
        /// Bina: iç (x0..x0+w-1, y0..y0+h-1). door: 'N','S','E','W' ve kapının kenar boyunca konumu (dx ya da dy, dünya değil yerel).
        /// Ön cephede (kapının olduğu yüz) her üç karoda bir pencere; yan duvarlarda bir pencere. Kapı önünden 'to' noktasına patika.
        /// </summary>
        public void Building(int x0, int y0, int w, int h, char door, int at, FloorType floor, params (int x, int y)[] to)
        {
            int x1 = x0 + w - 1, y1 = y0 + h - 1;
            if (_m != null)
            {
                for (int x = x0; x <= x1; x++)
                {
                    bool frontN = door == 'N', frontS = door == 'S';
                    bool winN = frontN ? (x - x0) % 3 == 1 : x == x0 + w / 2 && w >= 6;
                    bool winS = frontS ? (x - x0) % 3 == 1 : false;
                    _m.SetEdge(new Edge(_ox + x, _oy + y0, EdgeAxis.Horizontal), door == 'N' && x == at ? EdgeKind.Door : winN ? EdgeKind.Window : EdgeKind.Wall);
                    _m.SetEdge(new Edge(_ox + x, _oy + y1 + 1, EdgeAxis.Horizontal), door == 'S' && x == at ? EdgeKind.Door : winS ? EdgeKind.Window : EdgeKind.Wall);
                }
                for (int y = y0; y <= y1; y++)
                {
                    bool winE = door == 'E' ? (y - y0) % 3 == 1 : y == y0 + h / 2 && door != 'W';
                    bool winW = door == 'W' && (y - y0) % 3 == 1;
                    _m.SetEdge(new Edge(_ox + x0, _oy + y, EdgeAxis.Vertical), door == 'W' && y == at ? EdgeKind.Door : winW ? EdgeKind.Window : EdgeKind.Wall);
                    _m.SetEdge(new Edge(_ox + x1 + 1, _oy + y, EdgeAxis.Vertical), door == 'E' && y == at ? EdgeKind.Door : winE ? EdgeKind.Window : EdgeKind.Wall);
                }
                Floor(x0, y0, x1, y1, floor);
            }
            // Kapının dış karosu
            (int x, int y) outside = door == 'N' ? (at, y0 - 1) : door == 'S' ? (at, y1 + 1) : door == 'W' ? (x0 - 1, at) : (x1 + 1, at);
            if (to != null && to.Length > 0)
            {
                var pts = new (int, int)[to.Length + 1];
                pts[0] = outside; Array.Copy(to, 0, pts, 1, to.Length);
                Path(1, pts);
            }
        }

        /// <summary>Çitli alan (ağıl, bahçe). gate: 'N','S','E','W' ve konumu. pasture: otlak bölgesi işaretlenir.</summary>
        public void Fence(int x0, int y0, int x1, int y1, char gate, int at, bool pasture)
        {
            if (_m == null) return;
            for (int x = x0; x <= x1; x++)
            {
                _m.SetEdge(new Edge(_ox + x, _oy + y0, EdgeAxis.Horizontal), gate == 'N' && x == at ? EdgeKind.Gate : EdgeKind.Fence);
                _m.SetEdge(new Edge(_ox + x, _oy + y1 + 1, EdgeAxis.Horizontal), gate == 'S' && x == at ? EdgeKind.Gate : EdgeKind.Fence);
            }
            for (int y = y0; y <= y1; y++)
            {
                _m.SetEdge(new Edge(_ox + x0, _oy + y, EdgeAxis.Vertical), gate == 'W' && y == at ? EdgeKind.Gate : EdgeKind.Fence);
                _m.SetEdge(new Edge(_ox + x1 + 1, _oy + y, EdgeAxis.Vertical), gate == 'E' && y == at ? EdgeKind.Gate : EdgeKind.Fence);
            }
            if (pasture) for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) _m.SetZone(P(x, y), ZoneType.Pasture);
        }

        public void Water(int x, int y) { if (_m != null) _m.SetFloor(P(x, y), FloorType.Water); }

        public void PondAt(int x, int y, params KeyValuePair<string, int>[] fish)
        {
            if (_m == null) return;
            var a = P(x, y);
            if (_m.GetFloor(a) != FloorType.Water) return;
            _m.SetSpring(a);
            _m.RestorePond(a, 1f, spring: true, insects: true, filledDays: 30, fish);
        }

        public void I(string id, int x, int y) => Items.Add(new RegionItem(id, _ox + x, _oy + y));
        public void G(string id, int x, int y) => Items.Add(new RegionItem(id, _ox + x, _oy + y, grown: true));
        /// <summary>Duvara as: side 0 kuzey duvar, 3 batı duvar (izometrikte görünen arka duvarlar).</summary>
        public void Wall(string id, int x, int y, int side) => Items.Add(new RegionItem(id, _ox + x, _oy + y, wallSide: side));
        /// <summary>(x,y)'deki masanın/rafın yuvasına koy.</summary>
        public void On(string id, int x, int y, string slot) => Items.Add(new RegionItem(id, _ox + x, _oy + y, slot: slot));
    }

    /// <summary>
    /// Bölge düzeni (belge 38-40). Haritalar tek dünya ızgarasında, aralarında 4 karoluk kapalı boşluk.
    /// Geçitler komşu bölgelerin kenar karolarını bağlar; yol bulma ve yürüme bunları kullanır, böylece ekran dışındaki
    /// bölgede hayat sürer. Görünüm yalnızca etkin bölgeyi çizer, geçişte kısa kararma olur.
    /// Çiftlik — Köy yolu — Şehir. Orman (kuzey), ırmak ve göl (güney), tepe (doğu) çıkışları şehirde hazır.
    /// </summary>
    public static class Regions
    {
        public const int WorldWidth = 208, WorldHeight = 72;

        public static readonly Region Farm = new Region { Id = "farm", Name = "Çiftlik", X = 0, Y = 0, W = 96, H = 72 };
        public static readonly Region Road = new Region { Id = "road", Name = "Köy yolu", X = 100, Y = 20, W = 40, H = 24 };
        public static readonly Region Town = new Region { Id = "town", Name = "Şehir", X = 144, Y = 10, W = 64, H = 52 };
        public static readonly Region[] All = { Farm, Road, Town };

        // Geçitler (a, b): çiftliğin doğu yolu -> köy yolunun batı ucu; köy yolunun doğu ucu -> şehrin batı girişi.
        private static readonly int[][] PortalPairs =
        {
            new[] { 95, 40, 100, 40 }, new[] { 95, 41, 100, 41 },
            new[] { 139, 28, 144, 28 }, new[] { 139, 29, 144, 29 },
        };

        public static bool IsRegional(GridMap map) => map != null && map.Width == WorldWidth && map.Height == WorldHeight;

        public static Region At(GridPos p) { foreach (var r in All) if (r.Contains(p)) return r; return null; }

        /// <summary>Bölge dışı boşlukları kapatır ve geçitleri kurar (yeni oyunda).</summary>
        public static void Apply(GridMap map)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (At(p) == null) { map.SetBlocked(p, true); map.SetOwned(p, false); }
                }
            ApplyPortals(map);
        }

        /// <summary>Geçitler kayda yazılmaz: yeni oyunda ve her yüklemede bölge düzeninden kurulur.</summary>
        public static void ApplyPortals(GridMap map)
        {
            map.ClearPortals();
            foreach (var q in PortalPairs) map.AddPortal(new GridPos(q[0], q[1]), new GridPos(q[2], q[3]));
        }

        private static KeyValuePair<string, int> F(string id, int n) => new KeyValuePair<string, int>(id, n);

        /// <summary>Köy yolu ve şehrin arazisi (zemin, su, yol, binalar). Çiftlik GameRoot'ta kurulur.</summary>
        public static void BuildTerrain(GridMap m)
        {
            RoadMap(new Painter(m, Road.X, Road.Y));
            TownMap(new Painter(m, Town.X, Town.Y));
        }

        /// <summary>
        /// Yolları doğallaştırır (yeni oyunda, eşyalardan önce): kenar boyunca yer yer bir karoluk şişkinlik, böylece yol
        /// cetvelle çizilmiş gibi durmaz. Yalnızca aynı bölgedeki açık çime taşar; sabit desen (tohumdan bağımsız).
        /// </summary>
        public static void Naturalize(GridMap m)
        {
            var add = new List<GridPos>();
            var dirs = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (int y = 0; y < m.Height; y++)
                for (int x = 0; x < m.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (m.GetRoad(p) != RoadType.Path) continue;
                    var home = At(p);
                    foreach (var d in dirs)
                    {
                        var q = new GridPos(x + d.X, y + d.Y);
                        if (!m.InBounds(q) || m.GetRoad(q) != RoadType.None || m.GetFloor(q) != FloorType.Grass || m.IsBlocked(q)) continue;
                        if (home != null && !home.Contains(q)) continue;
                        if (!m.EdgeFree(p, q)) continue;
                        if (Hash(q.X, q.Y) < 0.09f) add.Add(q);
                    }
                }
            foreach (var q in add) m.SetRoad(q, RoadType.Path);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 73856093 ^ y * 19349663);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        /// <summary>Köy yolu ve şehrin başlangıç eşyaları.</summary>
        public static List<RegionItem> Items()
        {
            var a = new Painter(null, Road.X, Road.Y); RoadMap(a);
            var b = new Painter(null, Town.X, Town.Y); TownMap(b);
            a.Items.AddRange(b.Items);
            return a.Items;
        }

        // ================= Köy yolu (40x24) =================
        // Çiftlikten şehre kıvrılarak çıkan toprak yol; ortasından kuzey-güney bir dere geçer, üstünde ahşap köprü.
        // Gezgin tüccar durağı (bank, fıçı), yol kenarında toplama düğümleri, dere boyunca söğütler.
        private static int StreamX(int y) => 20 + (int)Math.Round(1.5 * Math.Sin(y / 4.0));

        private static void RoadMap(Painter p)
        {
            for (int y = 0; y < 24; y++) { int c = StreamX(y); p.Water(c, y); p.Water(c + 1, y); }
            // Köprü: yolun dereyi geçtiği yer (y12-13)
            for (int x = StreamX(12) - 1; x <= StreamX(13) + 2; x++) p.Floor(x, 12, x, 13, FloorType.Wood);
            p.PondAt(StreamX(0), 0, F("item.fish_trout", 5), F("item.fish_crayfish", 5));
            p.PondAt(StreamX(23), 23, F("item.fish_trout", 3), F("item.fish_crayfish", 4));
            p.Path(2, (0, 20), (7, 20), (7, 16), (14, 16), (14, 12), (27, 12), (27, 8), (39, 8));

            // Gezgin tüccar durağı ve yol kenarı
            p.I("item.garden_bench", 30, 6); p.I("item.barrel", 33, 6); p.I("item.seed_crate", 34, 6);
            p.I("item.forage_berry_bush", 5, 18); p.I("item.forage_strawberry", 11, 18); p.I("item.forage_dry_branch", 17, 9);
            p.I("item.forage_elderflower", 24, 15); p.I("item.forage_mushroom", 35, 14); p.I("item.forage_wild_garlic", 3, 10);
            p.I("item.forage_nettle", 31, 18);
            foreach (var (x, y) in new[] { (17, 3), (24, 5), (16, 19), (24, 21), (23, 1) }) p.G("item.tree_willow", x, y);
            var rng = new Random(40);
            for (int i = 0; i < 120; i++)
            {
                int x = rng.Next(0, 40), y = rng.Next(0, 24);
                if (Math.Abs(x - StreamX(y)) <= 2) continue;
                p.G(rng.Next(3) == 0 ? "item.tree_birch" : "item.tree_oak", x, y);
            }
        }

        // ================= Şehir (64x52) =================
        // Organik plan (belge 40): yollar kıvrılır, binalar farklı boyutta ve geri çekilmelerle dizilir, temel dükkânlar
        // sekizgen taş meydanın çevresinde toplanır (Pelican Town gibi), içinden kıvrılan bir ırmak geçer (iki köprü).
        // Yerleşim mantığı: demirci girişe yakın (yangın), değirmen ve çömlekçi ırmak kıyısında, tabakhaneli saraç akış
        // aşağıda ve kenarda (koku), hayvan pazarı ırmağın ötesinde, otacının bahçesi sessiz kuzeybatıda.
        // Çıkışlar: batı (köy yolu), kuzey (orman), güney (ırmak ve göl), doğu (tepe).
        private static int RiverX(int y) => 50 + (int)Math.Round(2.0 * Math.Sin(y / 8.0));

        private static bool InPlaza(int x, int y) { int dx = Math.Abs(x - 30), dy = Math.Abs(y - 26); return dx <= 7 && dy <= 5 && dx + dy <= 10; }

        public const int TownEntryY = 18;   // batı girişi (yerel y18-19)

        private static void TownMap(Painter p)
        {
            // Irmak (3 karo) ve iki köprü (doğu yolu y26-27, güney patikası y44-45)
            for (int y = 0; y < 52; y++) { int c = RiverX(y); for (int x = c - 1; x <= c + 1; x++) p.Water(x, y); }
            for (int y = 26; y <= 27; y++) for (int x = RiverX(y) - 1; x <= RiverX(y) + 1; x++) p.Floor(x, y, x, y, FloorType.Wood);
            for (int y = 44; y <= 45; y++) for (int x = RiverX(y) - 1; x <= RiverX(y) + 1; x++) p.Floor(x, y, x, y, FloorType.Wood);
            p.PondAt(RiverX(0), 0, F("item.fish_trout", 6), F("item.fish_pike", 2), F("item.fish_carp", 3));
            p.PondAt(RiverX(35), 35, F("item.fish_trout", 4), F("item.fish_carp", 4), F("item.fish_eel", 2));
            p.PondAt(RiverX(51), 51, F("item.fish_trout", 2), F("item.fish_crayfish", 4));

            // Meydan: sekizgen taş
            for (int y = 21; y <= 31; y++) for (int x = 23; x <= 37; x++) if (InPlaza(x, y)) p.Floor(x, y, x, y, FloorType.Stone);

            // Yollar (2 karo): batı girişi -> meydan; kuzey (orman), güney (göl), doğu (köprü, tepe); batı ara sokağı
            p.Path(2, (0, TownEntryY), (9, TownEntryY), (9, 21), (16, 21), (16, 25), (24, 25));
            p.Path(2, (30, 22), (30, 13), (28, 13), (28, 0));
            p.Path(2, (30, 30), (30, 38), (33, 38), (33, 51));
            p.Path(2, (36, 26), (63, 26));
            p.Path(2, (12, 21), (12, 44));
            p.Path(1, (34, 44), (47, 44), (47, 45), (56, 45));     // güney köprüsüne patika

            // ---- Meydanın çevresi ----
            // Meyhane / han (meydanın kuzeybatısında, en büyük yapı)
            p.Building(20, 12, 10, 7, 'S', 25, FloorType.Wood, (25, 21));
            p.I("item.hearth_stone", 20, 12); p.I("item.copper_pots", 21, 12); p.I("item.prep_counter", 22, 12);
            p.I("item.brew_barrel", 27, 12); p.I("item.brew_barrel", 28, 12); p.I("item.brew_barrel", 29, 12);
            p.I("item.barrel", 29, 13); p.I("item.barrel", 29, 14);
            p.Wall("item.tankard_rack", 23, 12, 0); p.Wall("item.plate_rack", 20, 14, 3);
            p.I("item.trestle_table", 21, 15); p.I("item.bench_long", 21, 14); p.I("item.bench_long", 21, 16);
            p.I("item.trestle_table", 25, 15); p.I("item.bench_long", 25, 14); p.I("item.bench_long", 25, 16);
            p.On("item.tankard", 21, 15, "a"); p.On("item.tankard", 25, 15, "a"); p.On("item.bread_basket", 21, 15, "b");
            p.I("item.singer_stand", 29, 17); p.I("item.candlestick", 20, 18); p.I("item.firewood_basket", 20, 13);

            // Pazar / genel dükkân (Kerem): meydanın kuzeydoğusu
            p.Building(33, 14, 8, 6, 'S', 36, FloorType.Wood, (36, 22));
            p.I("item.pantry_shelf", 33, 14); p.I("item.pantry_shelf", 38, 14); p.I("item.storage_jar", 35, 14); p.I("item.storage_jar", 36, 14);
            p.I("item.seed_crate", 40, 14); p.I("item.barrel", 40, 16); p.I("item.barrel", 40, 17); p.I("item.firewood_basket", 33, 18);
            p.I("item.prep_counter", 35, 17);
            p.On("item.fruit_basket", 35, 17, "a"); p.On("item.bread_basket", 35, 17, "b");
            p.On("item.honey_pot", 33, 14, "a"); p.On("item.storage_pot_small", 38, 14, "a");

            // Kâtip ve ulak: meydanın batısı, kapı doğuya
            p.Building(14, 28, 6, 5, 'E', 30, FloorType.Wood, (24, 30));
            p.I("item.shelf_wood", 14, 28); p.I("item.shelf_wood", 16, 28);
            p.On("item.book", 14, 28, "a"); p.On("item.book", 16, 28, "b"); p.On("item.book", 14, 28, "b");
            p.I("item.rug_runner", 14, 30); p.I("item.table_painted", 16, 30); p.I("item.chair_carved", 16, 31);
            p.On("item.hourglass", 16, 30, "a"); p.On("item.candle", 16, 30, "c");
            p.I("item.chest_wood", 14, 32); p.I("item.candlestick", 19, 28);

            // Terzi ve dokumacı: meydanın güneydoğusu
            p.Building(36, 32, 7, 6, 'N', 38, FloorType.Wood, (36, 31));
            p.I("item.loom", 36, 34); p.I("item.spinning_wheel", 36, 36); p.I("item.dye_vat", 42, 36);
            p.I("item.clothes_press", 40, 37); p.I("item.table_wood", 40, 33); p.I("item.chest_wood", 42, 33);
            p.On("item.sewing_basket", 40, 33, "a"); p.On("item.yarn_spindle", 40, 33, "c");
            p.I("item.stool_wood", 38, 36); p.I("item.felt_rug", 38, 34);

            // ---- Batı (giriş) ----
            // Otacı ve ebe: sessiz kuzeybatı, önünde çitli ot bahçesi
            p.Building(6, 6, 7, 6, 'S', 9, FloorType.Wood, (9, TownEntryY));
            p.I("item.potion_shelf", 6, 6); p.I("item.potion_shelf", 7, 6); p.I("item.potion_shelf", 8, 6);
            p.I("item.drying_rack", 10, 6); p.I("item.table_wood", 6, 9); p.I("item.bed_straw", 11, 9);
            p.On("item.mortar_pestle", 6, 9, "a"); p.On("item.herb_pot_thyme", 6, 9, "c");
            p.Wall("item.herb_bundle_wall", 6, 8, 3); p.I("item.ceiling_herbs", 9, 8); p.I("item.basin_ewer", 12, 11);
            p.Fence(11, 13, 15, 16, 'W', 14, false);
            p.Path(1, (10, 14), (10, 14));
            foreach (var (x, y) in new[] { (12, 14), (14, 14), (12, 16), (14, 16) }) p.I("item.forage_herbs", x, y);
            p.I("item.beehive", 15, 13);

            // Demirci: girişin hemen güneyi (yangın riski: meydandan uzak, açık alanda)
            p.Building(3, 22, 7, 6, 'N', 6, FloorType.Stone, (6, 20));
            p.I("item.forge", 3, 22); p.I("item.trip_hammer", 3, 26); p.I("item.whetstone", 7, 26);
            p.I("item.water_barrel", 9, 26); p.I("item.firewood_basket", 9, 22); p.I("item.bloomery", 8, 24);
            p.Wall("item.tool_rack_wall", 3, 25, 3);

            // Marangoz / mobilyacı: demircinin güneyi, batı ara sokağına bakar; dışarıda kereste avlusu
            p.Building(3, 31, 8, 6, 'E', 33, FloorType.Wood, (12, 33));
            p.I("item.workbench", 3, 31); p.I("item.workbench", 6, 31); p.Wall("item.tool_rack_wall", 5, 31, 0);
            p.I("item.chair_carved", 9, 35); p.I("item.chair_painted", 8, 35); p.I("item.table_painted", 5, 35);
            p.I("item.chest_bench", 3, 35); p.I("item.firewood_basket", 8, 31);
            p.I("item.sawmill", 4, 38); p.I("item.firewood_basket", 7, 38);

            // ---- Irmak kıyısı ----
            // Değirmen ve fırın: ırmağın kuzeyinde, dışında su çarkı
            p.Building(40, 4, 7, 6, 'S', 43, FloorType.Wood, (43, 11), (31, 11));
            p.I("item.millstone", 40, 4); p.I("item.quern", 44, 4); p.I("item.seed_crate", 46, 4); p.I("item.stone_oven", 45, 7);
            p.I("item.table_wood", 41, 8); p.On("item.bread_basket", 41, 8, "a"); p.I("item.barrel", 46, 5);
            p.I("item.water_wheel", 47, 5);

            // Çömlekçi: güneyde ırmak kıyısı (kil)
            p.Building(39, 40, 6, 6, 'N', 41, FloorType.Dirt, (41, 39), (32, 39));
            p.I("item.pottery_wheel", 39, 42); p.I("item.kiln", 43, 44); p.I("item.storage_jar", 39, 45); p.I("item.storage_jar", 40, 45);
            p.I("item.shelf_wood", 42, 40); p.On("item.clay_cup", 42, 40, "a"); p.On("item.clay_cup", 42, 40, "b");
            p.I("item.barrel_tub", 39, 40);

            // ---- Irmağın ötesi (doğu) ----
            // Balıkçı: doğu kıyısında, kapı ırmağa (batı)
            p.Building(54, 18, 6, 5, 'W', 20, FloorType.Wood, (53, 26));
            p.I("item.fish_trap", 55, 22); p.I("item.barrel", 59, 18); p.I("item.salting_barrel", 59, 19);
            p.I("item.shelf_wood", 56, 18); p.On("item.fish_basket_decor", 56, 18, "a");
            p.Wall("item.oar_rack", 54, 18, 3); p.Wall("item.dried_fish_string", 55, 18, 0); p.Wall("item.net_curtain", 58, 18, 0);
            p.I("item.table_wood", 57, 21);

            // Saraç ve tabakhane: akış aşağıda, kasabanın kenarında (koku)
            p.Building(54, 34, 7, 6, 'N', 57, FloorType.Dirt, (57, 28));
            p.I("item.saddler_bench", 54, 34); p.I("item.saddle_rack", 59, 34); p.I("item.saddle_rack", 60, 34);
            p.Wall("item.saddle_peg", 54, 37, 3); p.I("item.table_wood", 58, 38); p.I("item.barrel", 60, 39);
            p.I("item.tanning_rack", 55, 41); p.I("item.tanning_rack", 58, 41); p.I("item.barrel_tub", 61, 41);

            // Hayvan pazarı (Murat): ırmağın ötesinde, kuzeydoğuda çitli ağıl
            p.Fence(55, 4, 62, 12, 'S', 58, true);
            p.Path(1, (58, 13), (58, 26));
            p.I("item.feed_trough", 56, 5); p.I("item.water_trough", 60, 5); p.I("item.hay_bale", 56, 10); p.I("item.hay_bale", 57, 10);
            p.I("item.milking_stool", 61, 10); p.I("item.saddle_rack", 62, 8); p.I("item.barn_lantern", 55, 12);

            // ---- Güney ----
            // Taşçı: güney çıkışına yakın (taş ocağı yolu), dışarıda kireç ocağı
            p.Building(20, 42, 7, 6, 'E', 44, FloorType.Stone, (33, 44));
            p.I("item.stonecutter", 20, 42); p.I("item.workbench", 23, 42); p.I("item.table_wood", 20, 46); p.I("item.barrel", 26, 47);
            p.I("item.lime_kiln", 16, 47);

            // ---- Köylü evleri: dağınık, kendi bahçeleriyle ----
            p.Building(14, 4, 4, 3, 'S', 15, FloorType.Wood, (15, 9), (29, 9));                // ev 1
            p.Building(34, 4, 4, 3, 'S', 35, FloorType.Wood, (35, 8), (31, 8));                // ev 2
            p.Building(14, 36, 4, 3, 'N', 15, FloorType.Wood, (13, 35));                       // ev 3
            p.Building(22, 35, 4, 3, 'N', 23, FloorType.Wood, (23, 33), (30, 33));             // ev 4
            p.Building(56, 44, 4, 3, 'N', 57, FloorType.Wood, (62, 43), (62, 28));             // ev 5
            p.Building(4, 44, 4, 3, 'N', 5, FloorType.Wood, (5, 42), (12, 42));                // ev 6
            foreach (var (hx, hy) in Homes) { p.I("item.bed_straw", hx + 2, hy + 2); p.I("item.chest_wood", hx, hy + 2); p.I("item.stool_wood", hx, hy); }
            p.I("item.garden_bench", 18, 8); p.G("item.tree_apple", 13, 8); p.G("item.tree_pear", 38, 6); p.G("item.tree_plum", 19, 38);
            p.G("item.tree_cherry", 27, 37); p.G("item.tree_apple", 61, 47); p.I("item.forage_berry_bush", 8, 46); p.I("item.rain_barrel", 8, 44);

            // Meydan: Kerem'in tezgâhı, ortada kuyu, banklar, bekçi ateşi, köşelerde ağaç
            p.I("item.market_stall", 25, 22); p.I("item.well", 30, 26);
            p.I("item.garden_bench", 26, 29); p.I("item.garden_bench", 33, 22); p.I("item.bonfire", 34, 29);
            p.G("item.tree_oak", 23, 24); p.G("item.tree_oak", 37, 28);

            // Ağaç kümeleri: ırmak kıyısında söğüt, köşelerde meşe ve huş (yol ve binaya konmayanlar atlanır)
            foreach (var (x, y) in new[] { (47, 14), (53, 10), (46, 30), (53, 31), (52, 49), (46, 48), (49, 21) }) p.G("item.tree_willow", x, y);
            var rng = new Random(52);
            for (int i = 0; i < 90; i++)
            {
                int x = rng.Next(0, 64), y = rng.Next(0, 52);
                bool corner = (x < 18 && y < 5) || (x > 44 && y > 47) || (x < 10 && y > 47) || (y < 3) || (x > 61) || (x < 2 && y > 22);
                if (!corner || Math.Abs(x - RiverX(y)) <= 2) continue;
                p.G(rng.Next(3) == 0 ? "item.tree_birch" : "item.tree_oak", x, y);
            }
        }

        private static readonly (int x, int y)[] Homes = { (14, 4), (34, 4), (14, 36), (22, 35), (56, 44), (4, 44) };

        private static GridPos T(int x, int y) => new GridPos(Town.X + x, Town.Y + y);

        /// <summary>Köylülerin evleri ve iş yerleri şehirde (dünya koordinatı).</summary>
        public static Dictionary<string, GridPos> Places() => new Dictionary<string, GridPos>
        {
            { "home:1", T(15, 5) }, { "home:2", T(35, 5) }, { "home:3", T(15, 37) },
            { "home:4", T(23, 36) }, { "home:5", T(57, 45) }, { "home:6", T(5, 45) },
            { "work:smithy", T(6, 24) }, { "work:mill", T(42, 6) }, { "work:herb", T(9, 8) },
            { "work:pasture", T(58, 8) }, { "work:market", T(37, 18) }, { "work:pottery", T(41, 42) },
            { "square", T(30, 28) },
            // Yeni dükkân sahipleri (belge 41): dükkânın içinde çalışır ve yaşar (ortaçağda usta dükkânının arkasında otururdu).
            { "work:carpenter", T(7, 33) }, { "home:carpenter", T(9, 34) },
            { "work:tavern", T(24, 13) }, { "home:tavern", T(28, 17) },
            { "work:scribe", T(18, 31) }, { "home:scribe", T(15, 31) },
            { "work:tailor", T(39, 35) }, { "home:tailor", T(41, 35) },
            { "work:saddler", T(57, 36) }, { "home:saddler", T(56, 38) },
            { "work:fisher", T(56, 20) }, { "home:fisher", T(58, 19) },
            { "work:mason", T(23, 44) }, { "home:mason", T(24, 46) }
        };

        /// <summary>Dükkânların iç alanı (dünya koordinatı); dükkân kimliği Core.Economy.Shops ile aynı.</summary>
        public static Dictionary<string, RectI> ShopAreas() => new Dictionary<string, RectI>
        {
            { "general", R(33, 14, 8, 6) }, { "smithy", R(3, 22, 7, 6) }, { "carpenter", R(3, 31, 8, 6) },
            { "tavern", R(20, 12, 10, 7) }, { "mill", R(40, 4, 7, 6) }, { "scribe", R(14, 28, 6, 5) },
            { "herb", R(6, 6, 7, 6) }, { "potter", R(39, 40, 6, 6) }, { "tailor", R(36, 32, 7, 6) },
            { "saddler", R(54, 34, 7, 6) }, { "fisher", R(54, 18, 6, 5) }, { "mason", R(20, 42, 7, 6) },
            { "animals", R(55, 4, 8, 9) },
        };

        private static RectI R(int x, int y, int w, int h) => new RectI(Town.X + x, Town.Y + y, w, h);
    }
}
