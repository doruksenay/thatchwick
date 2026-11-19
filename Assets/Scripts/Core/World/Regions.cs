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
                // Bina içinden yol geçmez (eski planda kuzey yolu meyhanenin içinden geçiyordu, belge 44 D).
                for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) if (_m.GetRoad(P(x, y)) != RoadType.None) _m.SetRoad(P(x, y), RoadType.None);
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
    public static partial class Regions
    {
        public const int WorldWidth = 428, WorldHeight = 72;
        /// <summary>Yalnızca çiftlik, köy yolu ve şehri olan eski bölgeli kayıtların genişliği (yeni haritalar onlarda yok).</summary>
        public const int LegacyWorldWidth = 208;

        public static readonly Region Farm = new Region { Id = "farm", Name = "Çiftlik", X = 0, Y = 0, W = 96, H = 72 };
        public static readonly Region Road = new Region { Id = "road", Name = "Köy yolu", X = 100, Y = 20, W = 40, H = 24 };
        public static readonly Region Town = new Region { Id = "town", Name = "Şehir", X = 144, Y = 10, W = 64, H = 52 };
        public static readonly Region Forest = new Region { Id = "forest", Name = "Orman", X = 212, Y = 0, W = 48, H = 48 };
        public static readonly Region Lake = new Region { Id = "lake", Name = "Irmak ve göl", X = 264, Y = 0, W = 48, H = 32 };
        public static readonly Region Marsh = new Region { Id = "marsh", Name = "Sazlık", X = 316, Y = 0, W = 32, H = 24 };
        public static readonly Region Hill = new Region { Id = "hill", Name = "Tepe ve taş ocağı", X = 352, Y = 0, W = 40, H = 40 };
        public static readonly Region Slopes = new Region { Id = "slopes", Name = "Güney Yamaçları", X = 396, Y = 0, W = 32, H = 24 };

        public static readonly Region[] All = { Farm, Road, Town, Forest, Lake, Marsh, Hill, Slopes };

        // Geçitler (a, b): çiftliğin doğu yolu -> köy yolunun batı ucu; köy yolunun doğu ucu -> şehrin batı girişi.
        private static readonly int[][] PortalPairs =
        {
            new[] { 95, 40, 100, 40 }, new[] { 95, 41, 100, 41 },
            new[] { 139, 28, 144, 28 }, new[] { 139, 29, 144, 29 },
            // Şehrin kuzey çıkışı -> orman; güney çıkışı -> göl; doğu köprüsü -> tepe (belge 38 bağlantı şeması)
            new[] { 172, 10, 236, 47 }, new[] { 173, 10, 237, 47 },
            new[] { 177, 61, 288, 0 }, new[] { 178, 61, 289, 0 },
            new[] { 207, 36, 352, 20 }, new[] { 207, 37, 352, 21 },
            // Gölün doğu kıyısı -> sazlık; köy yolunun güney patikası -> Güney Yamaçları
            new[] { 311, 16, 316, 12 }, new[] { 311, 17, 316, 13 },
            new[] { 107, 20, 412, 23 },
        };

        public static bool IsRegional(GridMap map) => map != null && map.Height == WorldHeight && (map.Width == WorldWidth || map.Width == LegacyWorldWidth);

        public static Region At(GridPos p) { foreach (var r in All) if (r.Contains(p)) return r; return null; }

        /// <summary>Bölge bu haritada var mı (eski 208 genişlikli kayıtta yeni haritalar yok).</summary>
        public static bool Exists(GridMap map, Region r) => map != null && r.X + r.W <= map.Width && r.Y + r.H <= map.Height;

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
            foreach (var q in PortalPairs)
            {
                var a = new GridPos(q[0], q[1]); var b = new GridPos(q[2], q[3]);
                if (map.InBounds(a) && map.InBounds(b)) map.AddPortal(a, b);   // eski kayıtta yeni haritaların geçidi yok
            }
        }

        private static KeyValuePair<string, int> F(string id, int n) => new KeyValuePair<string, int>(id, n);

        /// <summary>Köy yolu ve şehrin arazisi (zemin, su, yol, binalar). Çiftlik GameRoot'ta kurulur.</summary>
        public static void BuildTerrain(GridMap m)
        {
            RoadMap(new Painter(m, Road.X, Road.Y));
            TownMap(new Painter(m, Town.X, Town.Y));
            WildTerrain(m);
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
            a.Items.AddRange(WildItems());
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
            p.Path(1, (7, 16), (7, 0));   // güneye ayrılan patika: Güney Yamaçları

            // Donatı (gezgin tüccar durağı, toplama düğümleri, ağaçlar) kurallarla: Core.Layout.OutdoorBuilder (belge 44 D).
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
            p.Path(2, (30, 22), (30, 10), (28, 10), (28, 0));   // meyhanenin doğusundan ve kuzeyinden dolanır (eskiden içinden geçiyordu)
            p.Path(2, (30, 30), (30, 38), (33, 38), (33, 51));
            p.Path(2, (36, 26), (63, 26));
            p.Path(2, (12, 21), (12, 44));
            p.Path(1, (34, 44), (47, 44), (47, 45), (56, 45));     // güney köprüsüne patika

            // ---- Binalar (plan: belge 40; iç döşeme şablonlardan, Core.Layout.TownBuilder, belge 44 C) ----
            foreach (var b in TownBuildings) p.Building(b.X, b.Y, b.W, b.H, b.Door, b.DoorAt, b.Floor, b.PathTo);

            // ---- Hayvan pazarı (Murat): ırmağın ötesinde, kuzeydoğuda çitli ağıl (plan); içi kurallarla ----
            p.Fence(PenX0, PenY0, PenX1, PenY1, 'S', 58, true);
            p.Path(1, (58, 13), (58, 26));
            // Dış donatı (meydan, tabelalar, avlular, bahçeler, ağaçlar) kurallarla: Core.Layout.OutdoorBuilder (belge 44 D).
        }

        /// <summary>Hayvan pazarının ağılı (yerel).</summary>
        public const int PenX0 = 55, PenY0 = 4, PenX1 = 62, PenY1 = 12;
        /// <summary>Meydanın ortası (yerel) ve sekizgen alanı.</summary>
        public const int PlazaCX = 30, PlazaCY = 26;
        public static bool InPlazaLocal(int x, int y) => InPlaza(x, y);
        /// <summary>Meydanı kapsayan dikdörtgen (dünya).</summary>
        public static RectI PlazaRect => new RectI(Town.X + PlazaCX - 7, Town.Y + PlazaCY - 5, 15, 11);
        public static RectI PenRect => new RectI(Town.X + PenX0, Town.Y + PenY0, PenX1 - PenX0 + 1, PenY1 - PenY0 + 1);
        public static int RiverAt(int localY) => RiverX(localY);
        /// <summary>Köy yolundaki derenin orta sütunu (yerel).</summary>
        public static int StreamAt(int localY) => StreamX(localY);

        /// <summary>
        /// Şehrin binaları (yerel koordinat, plan belge 40). Template: layouts.json bina şablonu (meslek ya da "cottage"); katlar ve
        /// iç döşeme oradan. Owner: sahibinin adlandırılmış evi (üst kat varsa orada yaşar).
        /// </summary>
        public static readonly TownBuilding[] TownBuildings =
        {
            // Meydanın çevresi
            new TownBuilding("tavern", "Meyhane / han", "inn", 20, 12, 10, 7, 'S', 25, FloorType.Wood, "home:tavern", (25, 21)),
            new TownBuilding("general", "Pazar (Kerem)", "shop", 33, 14, 8, 6, 'S', 36, FloorType.Wood, "home:general", (36, 22)),
            new TownBuilding("scribe", "Kâtip ve ulak", "shop", 14, 28, 6, 5, 'E', 30, FloorType.Wood, "home:scribe", (24, 30)),
            new TownBuilding("tailor", "Terzi ve dokumacı", "shop", 36, 32, 7, 6, 'N', 38, FloorType.Wood, "home:tailor", (36, 31)),
            // Batı (giriş)
            new TownBuilding("herb", "Otacı ve ebe", "shop", 6, 6, 7, 6, 'S', 9, FloorType.Wood, "home:herb", (9, TownEntryY)),
            new TownBuilding("smithy", "Demirci (Orhan)", "shop", 3, 22, 7, 6, 'N', 6, FloorType.Stone, null, (6, 20)),
            new TownBuilding("carpenter", "Marangoz / mobilyacı", "shop", 3, 31, 8, 6, 'E', 33, FloorType.Wood, "home:carpenter", (12, 33)),
            // Irmak kıyısı
            new TownBuilding("mill", "Değirmen ve fırın (Hatice)", "shop", 40, 4, 7, 6, 'S', 43, FloorType.Wood, "home:mill", (43, 11), (31, 11)),
            new TownBuilding("potter", "Çömlekçi (Ayşe)", "shop", 39, 40, 6, 6, 'N', 41, FloorType.Dirt, "home:potter", (41, 39), (32, 39)),
            // Irmağın ötesi
            new TownBuilding("fisher", "Balıkçı", "shop", 55, 18, 6, 5, 'W', 20, FloorType.Wood, "home:fisher", (53, 20), (53, 26)),   // bir karo doğuya: kapı önü 2 karo kuru (belge 44 D)
            new TownBuilding("saddler", "Saraç ve tabakhane", "shop", 54, 34, 7, 6, 'N', 57, FloorType.Dirt, "home:saddler", (57, 28)),
            // Güney
            new TownBuilding("mason", "Taşçı", "shop", 20, 42, 7, 6, 'E', 44, FloorType.Stone, "home:mason", (33, 44)),
            // Köylü evleri: dağınık, kendi bahçeleriyle
            new TownBuilding("home:1", "Köylü evi 1", "house", 14, 4, 4, 3, 'S', 15, FloorType.Wood, "home:1", (15, 9), (29, 9)) { Template = "cottage" },
            new TownBuilding("home:2", "Köylü evi 2", "house", 34, 4, 4, 3, 'S', 35, FloorType.Wood, "home:2", (35, 8), (31, 8), (31, 10)) { Template = "cottage" },   // patika yola bağlanır
            new TownBuilding("home:3", "Köylü evi 3", "house", 14, 36, 4, 3, 'N', 15, FloorType.Wood, "home:3", (13, 35)) { Template = "cottage" },
            new TownBuilding("home:4", "Köylü evi 4", "house", 22, 35, 4, 3, 'N', 23, FloorType.Wood, "home:4", (23, 33), (30, 33)) { Template = "cottage" },
            new TownBuilding("home:5", "Köylü evi 5", "house", 56, 44, 4, 3, 'N', 57, FloorType.Wood, "home:5", (62, 43), (62, 28)) { Template = "cottage" },
            new TownBuilding("home:6", "Köylü evi 6", "house", 4, 44, 4, 3, 'N', 5, FloorType.Wood, "home:6", (5, 42), (12, 42)) { Template = "cottage" },
        };

        public static TownBuilding TownBuildingById(string id)
        {
            foreach (var b in TownBuildings) if (b.Id == id) return b;
            return null;
        }

        private static GridPos T(int x, int y) => new GridPos(Town.X + x, Town.Y + y);

        /// <summary>
        /// Köylülerin evleri ve iş yerleri; map verilirse dükkân sahiplerinin evi üst kattadır (belge 44 C, Ek tablosu). Üst katı
        /// olmayan eski haritada (eski kayıt) eski yerine düşer.
        /// </summary>
        public static Dictionary<string, GridPos> Places(GridMap map)
        {
            var places = Places();
            if (map == null) return places;
            foreach (var kv in UpperHomes)
            {
                var b = TownBuildingById(kv.Key);
                if (b == null) continue;
                var p = Layout.TownBuilder.HomeSpot(map, b);
                if (p.HasValue) places[b.Owner] = p.Value;
                else if (kv.Value != null && places.TryGetValue(kv.Value, out var legacy)) places[b.Owner] = legacy;
            }
            return places;
        }

        /// <summary>Üst katta yaşayan dükkân sahipleri: bina → eski kayıtta düşeceği yer (önceden oturduğu ev).</summary>
        private static readonly Dictionary<string, string> UpperHomes = new Dictionary<string, string>
        {
            { "tavern", "home:tavern" }, { "general", "home:5" }, { "scribe", "home:scribe" }, { "tailor", "home:tailor" },
            { "herb", "home:3" }, { "carpenter", "home:carpenter" }, { "mill", "home:2" }, { "potter", "home:6" },
            { "fisher", "home:fisher" }, { "saddler", "home:saddler" },
        };

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
            { "work:fisher", T(57, 20) }, { "home:fisher", T(59, 19) },
            { "work:mason", T(23, 44) }, { "home:mason", T(24, 46) },
            // Yaban haritalarının buluşma yerleri (köylü programları ve gezinti için)
            { "forest", W(Forest, 24, 24) }, { "lake", W(Lake, 24, 8) }, { "marsh", W(Marsh, 6, 12) },
            { "hill", W(Hill, 8, 20) }, { "mine", W(Hill, MineMouthX, MineMouthY - 1) }, { "slopes", W(Slopes, 16, 20) },
        };

        /// <summary>Dükkânların iç alanı (dünya koordinatı); dükkân kimliği Core.Economy.Shops ile aynı.</summary>
        public static Dictionary<string, RectI> ShopAreas() => new Dictionary<string, RectI>
        {
            { "general", R(33, 14, 8, 6) }, { "smithy", R(3, 22, 7, 6) }, { "carpenter", R(3, 31, 8, 6) },
            { "tavern", R(20, 12, 10, 7) }, { "mill", R(40, 4, 7, 6) }, { "scribe", R(14, 28, 6, 5) },
            { "herb", R(6, 6, 7, 6) }, { "potter", R(39, 40, 6, 6) }, { "tailor", R(36, 32, 7, 6) },
            { "saddler", R(54, 34, 7, 6) }, { "fisher", R(55, 18, 6, 5) }, { "mason", R(20, 42, 7, 6) },
            { "animals", R(55, 4, 8, 9) },
        };

        private static RectI R(int x, int y, int w, int h) => new RectI(Town.X + x, Town.Y + y, w, h);
    }
}
