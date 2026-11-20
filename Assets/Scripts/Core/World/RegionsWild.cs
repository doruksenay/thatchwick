using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Yaban haritaları (belge 38, bağlantı şeması): orman (şehrin kuzeyi), ırmak ve göl (güneyi), sazlık (gölün doğusu),
    /// tepe ve taş ocağı (doğu köprüsü), Güney Yamaçları (köy yolunun güney patikası). Ev yok; toplama düğümleri, ağaçlar,
    /// balık gölleri ve maden ağzı. Hepsi sabit desenle (tohumdan bağımsız) çizilir: arazi ve eşyalar aynı fonksiyonla iki
    /// geçişte (önce harita, sonra eşyalar) üretildiği için her iki geçişte aynı sonucu verir.
    /// </summary>
    public static partial class Regions
    {
        // Bölge tanımları Regions.cs içinde (All dizisinden önce): partial dosyalar arasında statik alanların ilk değer
        // sırası tanımsızdır, ayrı dosyada kalsalar All boş bölge (null) içerebilirdi.

        /// <summary>Maden ağzı (tepe, yerel). Ocak paneli ileride buradan açılır.</summary>
        public const int MineMouthX = 28, MineMouthY = 34;

        private static GridPos W(Region r, int x, int y) => new GridPos(r.X + x, r.Y + y);

        private static void WildTerrain(GridMap m)
        {
            if (!Exists(m, Slopes)) return;
            ForestMap(new Painter(m, Forest.X, Forest.Y));
            LakeMap(new Painter(m, Lake.X, Lake.Y));
            MarshMap(new Painter(m, Marsh.X, Marsh.Y));
            HillMap(new Painter(m, Hill.X, Hill.Y));
            SlopesMap(new Painter(m, Slopes.X, Slopes.Y));
            CoastMap(new Painter(m, Coast.X, Coast.Y));
        }

        private static List<RegionItem> WildItems()
        {
            var all = new List<RegionItem>();
            var maps = new (Region r, Action<Painter> draw)[]
            {
                (Forest, new Action<Painter>(ForestMap)), (Lake, new Action<Painter>(LakeMap)), (Marsh, new Action<Painter>(MarshMap)),
                (Hill, new Action<Painter>(HillMap)), (Slopes, new Action<Painter>(SlopesMap)), (Coast, new Action<Painter>(CoastMap)),
            };
            foreach (var (r, draw) in maps) { var p = new Painter(null, r.X, r.Y); draw(p); all.AddRange(p.Items); }
            return all;
        }

        // ---------- ortak yardımcılar ----------

        /// <summary>Yolun karolarını (ve bir karoluk kenarını) ağaç dikilmeyecek yer olarak işaretler; yolu da çizer.</summary>
        private static void Trail(Painter p, HashSet<(int, int)> keep, int width, params (int x, int y)[] pts)
        {
            p.Path(width, pts);
            for (int i = 0; i + 1 < pts.Length; i++)
            {
                var (ax, ay) = pts[i]; var (bx, by) = pts[i + 1];
                for (int x = Math.Min(ax, bx) - 1; x <= Math.Max(ax, bx) + width; x++)
                    for (int w = -1; w <= width; w++) keep.Add((x, ay + w));
                for (int y = Math.Min(ay, by) - 1; y <= Math.Max(ay, by) + width; y++)
                    for (int w = -1; w <= width; w++) keep.Add((bx + w, y));
            }
        }

        private static void Keep(HashSet<(int, int)> keep, int x0, int y0, int x1, int y1)
        {
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) keep.Add((x, y));
        }

        /// <summary>Boş yerlere rastgele ama sabit desenli ağaç serper (kendi tohumuyla).</summary>
        private static void Scatter(Painter p, HashSet<(int, int)> keep, int w, int h, int count, int seed, Func<Random, string> pick)
        {
            var rng = new Random(seed);
            var used = new HashSet<(int, int)>();
            for (int i = 0; i < count; i++)
            {
                int x = rng.Next(0, w), y = rng.Next(0, h);
                string id = pick(rng);
                if (keep.Contains((x, y)) || !used.Add((x, y))) continue;
                p.G(id, x, y);
            }
        }

        /// <summary>Toplama düğümü koyar ve yerini ağaçlara kapatır.</summary>
        private static void Node(Painter p, HashSet<(int, int)> keep, string id, int x, int y) { p.I(id, x, y); keep.Add((x, y)); }

        // ================= Orman (48x48) =================
        // Şehrin kuzey çıkışından girilir (yerel 24-25, 47). Yol ormanın içine kıvrılarak iner: önce bir açıklık (avcı
        // dinlenme yeri), sonra küçük orman gölü, en derinde (kuzeybatı) kestane ve fındık korusu. Ağaçlar sık: meşe ve
        // huş yolun yakınında, çam derinde. Yaban kovanı, mantar, sarımsak ve ısırgan gölgede.
        private static bool ForestPond(int x, int y) { double dx = (x - 38) / 5.0, dy = (y - 12) / 3.5; return dx * dx + dy * dy <= 1; }

        private static void ForestMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            Trail(p, keep, 2, (24, 47), (24, 38), (30, 38), (30, 28), (20, 28), (20, 16), (12, 16), (12, 6));
            Trail(p, keep, 1, (30, 28), (34, 28), (34, 17));   // göle inen patika

            // Gölet: turna ve kerevit
            for (int y = 0; y < 48; y++) for (int x = 0; x < 48; x++) if (ForestPond(x, y)) { p.Water(x, y); keep.Add((x, y)); }
            Keep(keep, 32, 7, 44, 17);
            p.PondAt(38, 12, F("item.fish_pike", 3), F("item.fish_crayfish", 5), F("item.fish_trout", 2), F("item.fish_eel", 2), F("item.mussel", 3));

            // Açıklık (avcı dinlenme yeri): kütük sehpa ve yanında kuru dal yığını (tomruk "item.log" bir kaynaktır, yere
            // eşya olarak konmaz). Bank yok: dış mekân bankı yola bakmalı ve
            // sırtı duvara ya da çite dayanmalı (belge 44 D); açıklıkta ikisi de yok. Yol (x30-31) açıklığın doğusundan geçer.
            Keep(keep, 24, 30, 33, 36);
            p.I("item.stump_table", 27, 33); Node(p, keep, "item.forage_dry_branch", 25, 32);

            // Derin koru: kestane, fındık, kozalak
            Keep(keep, 3, 2, 11, 9);
            foreach (var (x, y) in new[] { (4, 3), (7, 2), (10, 4), (5, 7), (9, 8) }) Node(p, keep, "item.forage_chestnut", x, y);
            foreach (var (x, y) in new[] { (15, 4), (16, 8), (3, 11) }) Node(p, keep, "item.forage_hazel", x, y);

            Node(p, keep, "item.forage_hive", 17, 22);
            Node(p, keep, "item.forage_hive", 41, 30);
            foreach (var (x, y) in new[] { (22, 20), (27, 25), (9, 14), (36, 40), (44, 22) }) Node(p, keep, "item.forage_mushroom", x, y);
            foreach (var (x, y) in new[] { (18, 34), (32, 22) }) Node(p, keep, "item.forage_wild_garlic", x, y);   // gölgede, göl patikasının batısı
            foreach (var (x, y) in new[] { (22, 42), (14, 26) }) Node(p, keep, "item.forage_nettle", x, y);
            foreach (var (x, y) in new[] { (28, 44), (16, 38), (40, 36) }) Node(p, keep, "item.forage_berry_bush", x, y);
            foreach (var (x, y) in new[] { (6, 20), (2, 30), (44, 4) }) Node(p, keep, "item.forage_pinecone", x, y);
            foreach (var (x, y) in new[] { (26, 8), (8, 40) }) Node(p, keep, "item.forage_dry_branch", x, y);
            Node(p, keep, "item.forage_herbs", 37, 24);

            // Sık ağaçlar: kuzeye (derine, düşük y) gittikçe çam artar.
            Scatter(p, keep, 48, 48, 520, 212, rng =>
            {
                int r = rng.Next(10);
                return r < 4 ? "item.tree_oak" : r < 7 ? "item.tree_birch" : "item.tree_pine";
            });
        }

        // ================= Irmak ve göl (48x32) =================
        // Şehrin güney çıkışından girilir (yerel 24-25, 0). Büyük göl, güney kıyısında kum plajı (cam zincirinin kumu),
        // plajdan göle uzanan ahşap iskele, kıyıda söğütler ve su teresi. Doğu kıyısından sazlığa patika çıkar.
        private static bool LakeWater(int x, int y) { double dx = (x - 26) / 15.0, dy = (y - 20) / 9.0; return dx * dx + dy * dy <= 1 + 0.06 * Math.Sin(x * 0.9 + y * 0.4); }

        private static void LakeMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 48; x++)
                {
                    if (LakeWater(x, y)) { p.Water(x, y); keep.Add((x, y)); continue; }
                    // Güney kıyısındaki plaj: göle yakın ve gölün altında kalan şerit toprak (kum)
                    bool near = LakeWater(x, y + 1) || LakeWater(x, y + 2) || LakeWater(x - 1, y + 1) || LakeWater(x + 1, y + 1);
                    if (near && y <= 14) { p.Floor(x, y, x, y, FloorType.Dirt); keep.Add((x, y)); }
                }

            Trail(p, keep, 2, (24, 0), (24, 7), (44, 7), (44, 16), (47, 16));
            Trail(p, keep, 1, LakeToCoast);   // batı kıyısından denize inen patika (belge 47 ek)
            // İskele: göle yürüyen ahşap
            for (int y = 9; y <= 15; y++) for (int x = 24; x <= 25; x++) { p.Floor(x, y, x, y, FloorType.Wood); keep.Add((x, y)); }
            p.I("item.stump_table", 24, 15); p.On("item.fish_basket_decor", 24, 15, "a"); p.I("item.barrel", 25, 9);   // iskele ucunda sehpa, üstünde balık sepeti
            p.PondAt(28, 20, F("item.fish_carp", 8), F("item.fish_pike", 4), F("item.fish_eel", 3), F("item.fish_trout", 2), F("item.fish_tench", 4), F("item.fish_bream", 4), F("item.fish_rudd", 4), F("item.fish_catfish", 1));

            foreach (var (x, y) in new[] { (17, 10), (21, 9), (30, 9), (34, 10), (13, 12) }) Node(p, keep, "item.forage_sand", x, y);
            foreach (var (x, y) in new[] { (9, 18), (42, 22), (40, 14) }) Node(p, keep, "item.forage_cress", x, y);
            Node(p, keep, "item.forage_elderflower", 6, 8);
            Node(p, keep, "item.forage_chamomile", 36, 4);
            Node(p, keep, "item.forage_mint", 8, 24);
            foreach (var (x, y) in new[] { (9, 13), (43, 19), (11, 26), (41, 27), (19, 30), (33, 30), (7, 21) }) { p.G("item.tree_willow", x, y); keep.Add((x, y)); }

            Scatter(p, keep, 48, 32, 110, 264, rng => rng.Next(3) == 0 ? "item.tree_birch" : "item.tree_oak");
        }

        /// <summary>Gölden deniz kıyısına inen patika (yerel).</summary>
        private static readonly (int x, int y)[] LakeToCoast = { (24, 7), (5, 7), (5, 31) };

        // ================= Deniz kıyısı (48x32) =================
        // Gölün güneybatı patikasından girilir (yerel 5, 0). Kuzeyde çayır, ortada kumsal (toprak zemin), güneyde deniz.
        // Doğuda kayalık burun: gelgit çukurları (deniz yosunu, karides, keşiş yengeci; kayada tuz kabuğu). Kumsalda
        // denizin bıraktığı kıyı çizgisi (dere odunu, kabuk, ender mercan) ve deniz kumu. Ahşap iskeleden ringa ve uskumru.
        private static int SeaEdge(int x) => 19 + (int)Math.Round(1.5 * Math.Sin(x / 3.2) + 0.8 * Math.Sin(x / 7.0));

        /// <summary>Yumuşak gürültü (3x3 ortalama): kaya kümesi dağınık benek değil, bütün bir burun olsun.</summary>
        private static double Smooth(int x, int y)
        {
            double sum = 0;
            for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) sum += Hash(x + dx + 264, y + dy + 36);
            return sum / 9.0;
        }

        // Kayalık burun: doğuya doğru genişler, kumsalın arkasından denize uzanır; kenarı gürültüyle girintili.
        private static bool CoastRock(int x, int y)
        {
            if (x < 34 || y < 10 || y > SeaEdge(x) + 6) return false;
            double reach = (x - 34) / 13.0;                      // 0 batı ucu, 1 doğu kenarı
            double core = reach * 1.1 - Math.Abs(y - (SeaEdge(x) - 1)) / 9.0;
            return core + (Smooth(x, y) - 0.5) * 0.6 > 0.25;
        }

        private static void CoastMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 48; x++)
                {
                    if (CoastRock(x, y)) { p.Floor(x, y, x, y, FloorType.Stone); keep.Add((x, y)); continue; }
                    if (y >= SeaEdge(x)) { p.Water(x, y); keep.Add((x, y)); continue; }
                    // Kumsal: denizden geriye 6-9 karo; çayıra geçişi gürültüyle girintili.
                    int depth = 6 + (int)Math.Round(Smooth(x, y / 2) * 3);
                    if (y >= SeaEdge(x) - depth) { p.Floor(x, y, x, y, FloorType.Dirt); keep.Add((x, y)); }
                }
            // Patika: gölden iner, kumsalda iskeleye kadar sürer.
            int pierTop = SeaEdge(20) - 1;
            Trail(p, keep, 1, (5, 0), (5, 6), (20, 6), (20, pierTop - 1));
            // İskele: kumsaldan denize
            for (int y = pierTop; y <= SeaEdge(20) + 5; y++) for (int x = 20; x <= 21; x++) { p.Floor(x, y, x, y, FloorType.Wood); keep.Add((x, y)); }
            // Balıkçı iniş yeri (iskelenin batısında, kumsalda): sepet, fıçı, halat, ateş yeri. Ağ askısı balıkçı dükkânına aittir (şehir).
            int land = pierTop - 2;
            var used = new HashSet<(int, int)>();
            foreach (var (id, x, y) in new[] { ("item.fish_basket", 18, land), ("item.barrel", 22, pierTop - 1), ("item.rope_coil", 19, land + 1), ("item.bonfire", 13, land + 1) })
            { p.I(id, x, y); used.Add((x, y)); }
            p.PondAt(14, 27, F("item.fish_herring", 12), F("item.fish_mackerel", 8));
            p.PondAt(30, 28, F("item.fish_herring", 8), F("item.fish_mackerel", 6));

            // Gelgit çukurları: denize değen kaya karolarında, aralarında en az 3 karo, gürültüyle seçilir.
            var pools = new List<(int x, int y)>();
            for (int y = 10; y < 32 && pools.Count < 4; y++)
                for (int x = 34; x < 48 && pools.Count < 4; x++)
                {
                    if (!CoastRock(x, y)) continue;
                    bool touchesSea = false;
                    foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && nx < 48 && ny < 32 && !CoastRock(nx, ny) && ny >= SeaEdge(nx)) touchesSea = true;
                    }
                    if (!touchesSea || Hash(x + 7, y + 11) < 0.45) continue;
                    bool far = true;
                    foreach (var q in pools) if (Math.Abs(q.x - x) + Math.Abs(q.y - y) < 5) far = false;
                    if (!far) continue;
                    pools.Add((x, y));
                    p.I("item.forage_tidepool", x, y);
                    used.Add((x, y));
                }
            // Kıyı çizgisi suyun hemen gerisinde, deniz kumsalı biraz daha içeride (kumsal karosu, iskele ve iniş yeri dışında).
            foreach (var x in new[] { 6, 27, 31 }) { int y = SeaEdge(x) - 1; if (!used.Contains((x, y)) && !CoastRock(x, y) && x != 20 && x != 21) { p.I("item.forage_driftline", x, y); used.Add((x, y)); } }
            foreach (var x in new[] { 9, 25, 29 }) { int y = SeaEdge(x) - 4; if (!used.Contains((x, y)) && !CoastRock(x, y)) { p.I("item.forage_sea_sand", x, y); used.Add((x, y)); } }
            Node(p, keep, "item.forage_chamomile", 30, 3);
            Node(p, keep, "item.forage_sage", 40, 4);
            // Rüzgâra açık kıyı: seyrek çam, birkaç meşe; kumsala ağaç girmez (keep).
            Scatter(p, keep, 48, 32, 45, 300, rng => rng.Next(4) == 0 ? "item.tree_oak" : "item.tree_pine");
        }

        /// <summary>
        /// Eski kayıt göçü: deniz kıyısı sonradan eklendi (20.11.2026). Bölge hâlâ kapalıysa açılır ve çizilir, gölden inen
        /// patika döşenir. Eşyaları ve patikadaki ağaçların kaldırılmasını oyun katmanı yapar (StarterWorld.PlaceIn).
        /// </summary>
        public static bool EnsureCoast(GridMap m)
        {
            if (m == null || m.Width != WorldWidth || m.Height != WorldHeight) return false;
            var probe = W(Coast, 24, 4);
            if (!m.IsBlocked(probe)) return false;
            for (int y = Coast.Y; y < Coast.Y + Coast.H; y++)
                for (int x = Coast.X; x < Coast.X + Coast.W; x++) m.SetBlocked(new GridPos(x, y), false);
            CoastMap(new Painter(m, Coast.X, Coast.Y));
            new Painter(m, Lake.X, Lake.Y).Path(1, LakeToCoast);
            ApplyPortals(m);
            return true;
        }

        /// <summary>Deniz suyu mu (kıyı bölgesi; tuzludur, donmaz).</summary>
        public static bool IsSea(GridMap m, GridPos p) => m != null && m.Width == WorldWidth && Coast.Contains(p);

        /// <summary>Bir dünya karosu gölden kıyıya inen yeni patikanın üstünde mi (göçte oradaki ağaçlar kaldırılır).</summary>
        public static bool OnLakeToCoastTrail(GridMap m, GridPos p) => Lake.Contains(p) && m.GetRoad(p) != RoadType.None && (p.X - Lake.X == 5 || p.Y - Lake.Y == 7);

        // ================= Sazlık (32x24) =================
        // Gölün doğu kıyısından girilir (yerel 0, 12-13). Dağınık su gözleri arasında ahşap yürüme yolu; kerevit ve yılan
        // balığı gözleri, söğütler. Saz kümesi eşyası henüz yok (çatı sazı için iş dosyası gerekiyor).
        private static bool MarshWater(int x, int y)
        {
            if (y == 12 || y == 13) return false;   // yürüme yolu
            double v = Math.Sin(x / 2.6) + Math.Cos(y / 2.2) + 0.9 * Hash(x + 316, y);
            return v > 1.25;
        }

        private static void MarshMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 32; x++)
                    if (x >= 3 && MarshWater(x, y)) { p.Water(x, y); keep.Add((x, y)); }
            Trail(p, keep, 2, (0, 12), (3, 12));
            for (int x = 3; x <= 28; x++) for (int y = 12; y <= 13; y++) { p.Floor(x, y, x, y, FloorType.Wood); keep.Add((x, y)); }
            p.I("item.fish_trap", 28, 14); keep.Add((28, 14));

            // Balık gözleri: her biri kendi tablosuyla (kerevit ve yılan balığı ağırlıklı)
            int n = 0;
            for (int y = 0; y < 24 && n < 4; y += 5)
                for (int x = 4; x < 32 && n < 4; x += 7)
                    if (MarshWater(x, y)) { p.PondAt(x, y, F("item.fish_crayfish", 6), F("item.fish_eel", 4)); n++; }

            foreach (var (x, y) in new[] { (5, 16), (18, 8), (26, 18) }) if (!keep.Contains((x, y))) Node(p, keep, "item.forage_cress", x, y);
            foreach (var (x, y) in new[] { (10, 4), (22, 20) }) if (!keep.Contains((x, y))) Node(p, keep, "item.forage_nettle", x, y);
            Scatter(p, keep, 32, 24, 40, 316, rng => rng.Next(4) == 0 ? "item.tree_birch" : "item.tree_willow");
        }

        // ================= Tepe ve taş ocağı (40x40) =================
        // Şehrin doğu köprüsünden girilir (yerel 0, 20-21). Yol çamlık yamaçtan taşlık düzlüğe çıkar: taş ocağı, kaya
        // zemin ve en üstte maden ağzı (aile ocağı, kat -1). Kozalak, adaçayı ve kuşburnu.
        private static bool HillRock(int x, int y) => x >= 20 && y >= 24 && Hash(x + 352, y) + (x - 20) / 30.0 + (y - 24) / 24.0 > 0.75;

        private static void HillMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            for (int y = 0; y < 40; y++)
                for (int x = 0; x < 40; x++)
                    if (HillRock(x, y)) { p.Floor(x, y, x, y, FloorType.Stone); keep.Add((x, y)); }
            // Maden ağzı: taş meydancık ve fener. Ağzın kendisi (kaya yüzünde açıklık) görsel iş dosyası bekliyor; ocak
            // şimdilik M paneliyle açılır. Merdiven eşyası konmaz: kat -1'e bağlantı kurup bölgesiz bir bodrum açardı.
            p.Floor(MineMouthX - 2, MineMouthY - 1, MineMouthX + 2, MineMouthY + 2, FloorType.Stone);
            Keep(keep, MineMouthX - 3, MineMouthY - 2, MineMouthX + 3, MineMouthY + 3);
            p.I("item.barn_lantern", MineMouthX + 2, MineMouthY);
            p.I("item.barrel", MineMouthX - 2, MineMouthY + 1);
            Trail(p, keep, 2, (0, 20), (12, 20), (12, 28), (24, 28), (24, 33), (MineMouthX - 3, 33));

            // Taş ocağı ve taşçı tezgâhı
            Keep(keep, 28, 22, 34, 28);
            p.I("item.quarry", 30, 24); p.I("item.stonecutter", 33, 23);

            foreach (var (x, y) in new[] { (4, 10), (8, 30), (16, 6), (6, 36) }) Node(p, keep, "item.forage_pinecone", x, y);
            foreach (var (x, y) in new[] { (14, 14), (18, 33) }) Node(p, keep, "item.forage_sage", x, y);
            foreach (var (x, y) in new[] { (5, 24), (16, 24), (36, 8) }) Node(p, keep, "item.forage_rosehip", x, y);
            Node(p, keep, "item.forage_dry_branch", 9, 16);
            Node(p, keep, "item.forage_hive", 3, 4);

            Scatter(p, keep, 40, 40, 260, 352, rng => rng.Next(5) == 0 ? "item.tree_birch" : "item.tree_pine");
        }

        // ================= Güney Yamaçları (32x24) =================
        // Köy yolunun güney patikasından girilir (yerel 16, 23). Güneye bakan ılık yamaç: taş setli teraslarda zeytin,
        // asma ve kahve çalısı; ev kurulmaz, yalnızca küçük bir bekçi barınağı. Safran soğanı eşyası gelince setlere eklenir.
        private static void SlopesMap(Painter p)
        {
            var keep = new HashSet<(int, int)>();
            Trail(p, keep, 1, (16, 23), (16, 14), (9, 14), (9, 6));
            // Taş setler: her terasın alt kenarı (yol geçen yerde açık)
            foreach (int ty in new[] { 17, 11, 5 })
                for (int x = 1; x <= 30; x++)
                {
                    if (Math.Abs(x - 16) <= 1 || Math.Abs(x - 9) <= 1) continue;   // yol geçitleri
                    p.Floor(x, ty, x, ty, FloorType.Stone); keep.Add((x, ty));
                }

            // Barınak (4x3), kapısı güneyde, yola bağlı
            p.Building(22, 1, 4, 3, 'S', 23, FloorType.Wood, (16, 14));
            Keep(keep, 21, 0, 27, 5);
            p.I("item.bed_straw", 22, 1); p.I("item.barrel", 25, 1);

            // Teraslar: alt sette zeytin, ortada asma, üstte kahve çalısı
            for (int x = 3; x <= 29; x += 3) if (!keep.Contains((x, 20))) { p.G("item.tree_olive", x, 20); keep.Add((x, 20)); }
            for (int x = 2; x <= 30; x += 2)
                foreach (int y in new[] { 13, 15 })
                    if (!keep.Contains((x, y))) { p.G("item.vine_grape", x, y); keep.Add((x, y)); }
            for (int x = 3; x <= 19; x += 2)
                foreach (int y in new[] { 7, 9 })
                    if (!keep.Contains((x, y))) { p.G("item.bush_coffee", x, y); keep.Add((x, y)); }

            Node(p, keep, "item.forage_sage", 28, 8);
            Node(p, keep, "item.forage_rosehip", 2, 21);
            Node(p, keep, "item.forage_linden", 30, 19);
        }
    }
}
