using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Mining
{
    /// <summary>Bir damar türü: hangi hammadde, kaç damar, karo sertliği, gereken Madencilik seviyesi.</summary>
    public sealed class VeinDef
    {
        public string Item;
        public int Count;
        public RockKind Rock = RockKind.Stone;
        public int MinLevel = 1;
        /// <summary>Damar boyu aralığı (karo); cep türleri (kil) kısa ve yuvarlak.</summary>
        public int MinLength = 4, MaxLength = 9;
        public bool Pocket;

        public VeinDef(string item, int count, RockKind rock, int minLevel, bool pocket = false)
        {
            Item = item; Count = count; Rock = rock; MinLevel = minLevel; Pocket = pocket;
        }
    }

    /// <summary>Bir kat ya da arsa kazısının üretim tablosu (23. belge 3.2, 3.4, 4.2).</summary>
    public sealed class MineTable
    {
        public string Id;
        /// <summary>Ana kaya ağırlıkları: toprak, yumuşak taş, taş, sert taş.</summary>
        public float Soil, Soft, Stone, Hard;
        public bool BedrockBorder;
        public List<VeinDef> Veins = new List<VeinDef>();
        /// <summary>Damar karolarının sulu olma oranı ve damar dışı karolarınki.</summary>
        public double WetOre, WetOther;
        /// <summary>Taş karolarında kalıntı şansı ve kalıntı listesi.</summary>
        public double RelicChance;
        public string[] Relics = new string[0];
        /// <summary>Destek mesafesi ve gevşek karonun günlük çatırdama şansı.</summary>
        public int SupportDistance = 3;
        public double CreakChance = 0.10;
        /// <summary>Kazılabilmesi için gereken Madencilik seviyesi.</summary>
        public int MinLevel = 1;

        public static readonly string[] PlotRelics = { "item.relic_foundation_stone", "item.relic_buried_jar", "item.relic_horseshoe" };
        public static readonly string[] MineRelics = { "item.relic_fossil", "item.relic_miner_lamp", "item.relic_pick_head", "item.relic_coin", "item.relic_bone_comb", "item.relic_crystal" };

        /// <summary>Arsa kazısı (kat -2): toprak ve yumuşak taş ağırlıklı; kil, kireçtaşı, kömür, seyrek demir; derin hammadde yok.</summary>
        public static MineTable Plot() => new MineTable
        {
            Id = "plot", Soil = 0.45f, Soft = 0.33f, Stone = 0.19f, Hard = 0.03f,
            Veins =
            {
                new VeinDef("item.clay", 4, RockKind.Soil, 1, pocket: true),
                new VeinDef("item.limestone", 2, RockKind.Stone, 1),
                new VeinDef("item.coal", 2, RockKind.Soft, 2),
                new VeinDef("item.iron_ore", 1, RockKind.Hard, 2),
            },
            WetOre = 0.08, WetOther = 0.04, RelicChance = 0.005, Relics = PlotRelics,
            SupportDistance = 2, CreakChance = 0.10, MinLevel = 2
        };

        /// <summary>Aile ocağının galeri katı (-1): taş, kireçtaşı, demir, kömür, arduvaz, alçıtaşı, aşı boyası.</summary>
        public static MineTable Gallery() => new MineTable
        {
            Id = "mine-1", Soil = 0.05f, Soft = 0.25f, Stone = 0.5f, Hard = 0.2f, BedrockBorder = true,
            Veins =
            {
                new VeinDef("item.limestone", 4, RockKind.Stone, 1),
                new VeinDef("item.flint", 2, RockKind.Stone, 1),
                new VeinDef("item.clay", 3, RockKind.Soil, 1, pocket: true),
                new VeinDef("item.ochre", 2, RockKind.Soft, 2),
                new VeinDef("item.coal", 3, RockKind.Soft, 2),
                new VeinDef("item.iron_ore", 4, RockKind.Hard, 2),
                new VeinDef("item.gypsum", 2, RockKind.Soft, 3),
                new VeinDef("item.slate_stone", 2, RockKind.Hard, 3),
            },
            WetOre = 0, WetOther = 0, RelicChance = 0.005, Relics = MineRelics,
            SupportDistance = 3, CreakChance = 0.10, MinLevel = 1
        };
    }

    /// <summary>
    /// Tohumlu kazı yeri üretimi: aynı tohum aynı katı verir (kayıtta yalnızca kazılan şekil ve değişen karolar tutulur).
    /// Sürüm 2 (belge 64 §6): kaya türleri karo başına zarla değil, katın eğim yönünde uzayan düşük frekanslı gürültüden
    /// okunur; eşikler tablonun oranlarını birebir verir (bantlar ve cepler oluşur, denge değişmez). Damarlar katın eğim
    /// yönüne ±30° sadık uzanır. Sulu karolar nem alanından öbek hâlinde gelir. Madende (arsada değil) 2-3 doğal kovuk ve
    /// elle tasarlanmış hazır mekânlar (eski galeri, yer altı gölcüğü, çökmüş geçit, kemerli oda) açılır; girişin çevresine
    /// taşmazlar. Açılmış maden kayıttan gelir; yeni üretim yalnızca yeni açılan katta çalışır.
    /// </summary>
    public static class MineGen
    {
        public const int Version = 2;

        /// <summary>Katın eğim yönü (radyan, 0-π): damarlar ve kaya bantları bu yönde uzanır.</summary>
        public static double DirectionOf(int seed) => new Random(seed).NextDouble() * Math.PI;

        public static void Generate(MineMap map, MineTable table)
        {
            var rng = new Random(map.Seed);
            // Katın eğim yönü (damar ve bant yönü): 0-180°.
            double theta = rng.NextDouble() * Math.PI;
            double ct = Math.Cos(theta), st = Math.Sin(theta);
            int salt = rng.Next();
            var tiles = new List<(int x, int y, MineTile t)>();
            foreach (var e in map.All())
            {
                if (table.BedrockBorder && (e.x == 0 || e.y == 0 || e.x == map.Width - 1 || e.y == map.Height - 1)) { e.t.Rock = RockKind.Bedrock; continue; }
                tiles.Add(e);
            }
            // Kaya alanı: eğim yönünde uzun (bant), dik yönde dar; iki oktav.
            var rock = new double[tiles.Count];
            var wet = new double[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                var (x, y, _) = tiles[i];
                double u = x * ct + y * st, v = -x * st + y * ct;
                rock[i] = Noise.Value(u / 9.0, v / 3.2, salt) * 0.7 + Noise.Value(u / 3.5, v / 2.0, salt + 17) * 0.3;
                wet[i] = Noise.Value(x / 4.5, y / 4.5, salt + 41) * 0.8 + Noise.Value(x / 1.7, y / 1.7, salt + 43) * 0.2;
            }
            float total = table.Soil + table.Soft + table.Stone + table.Hard;
            double q1 = Quantile(rock, table.Soil / total), q2 = Quantile(rock, (table.Soil + table.Soft) / total),
                   q3 = Quantile(rock, (table.Soil + table.Soft + table.Stone) / total);
            double wOther = Quantile(wet, 1.0 - table.WetOther), wOre = Quantile(wet, 1.0 - table.WetOre);
            for (int i = 0; i < tiles.Count; i++)
            {
                var t = tiles[i].t; double r = rock[i];
                t.Rock = r < q1 ? RockKind.Soil : r < q2 ? RockKind.Soft : r < q3 ? RockKind.Stone : RockKind.Hard;
                t.Wet = table.WetOther > 0 && wet[i] > wOther;
            }
            var wetAt = new Dictionary<(int, int), double>();
            for (int i = 0; i < tiles.Count; i++) wetAt[(tiles[i].x, tiles[i].y)] = wet[i];

            // Damarlar: eğim yönüne ±30° sadık; cepler kısa ve toplu.
            foreach (var v in table.Veins)
                for (int n = 0; n < v.Count; n++)
                {
                    double x = 1 + rng.Next(Math.Max(1, map.Width - 2)), y = 1 + rng.Next(Math.Max(1, map.Height - 2));
                    double baseAng = theta + (rng.Next(2) == 0 ? 0 : Math.PI);
                    double ang = baseAng + (rng.NextDouble() - 0.5) * (Math.PI / 3);
                    int len = v.Pocket ? 2 + rng.Next(3) : v.MinLength + rng.Next(v.MaxLength - v.MinLength + 1);
                    bool wide = v.Pocket || rng.NextDouble() < 0.4;
                    for (int i = 0; i < len; i++)
                    {
                        int ix = (int)Math.Round(x), iy = (int)Math.Round(y);
                        if (!map.InBounds(ix, iy)) break;
                        Paint(map, ix, iy, v, table, wetAt, wOre);
                        if (wide)
                        {
                            // Genişlik: yöne dik komşu.
                            int px = ix + (int)Math.Round(-Math.Sin(ang)), py = iy + (int)Math.Round(Math.Cos(ang));
                            Paint(map, px, py, v, table, wetAt, wOre);
                        }
                        if (v.Pocket) { ang += (rng.NextDouble() - 0.5) * 2.0; }
                        else
                        {
                            ang += (rng.NextDouble() - 0.5) * (Math.PI / 9);
                            double d = Wrap(ang - baseAng);
                            if (d > Math.PI / 6) ang = baseAng + Math.PI / 6; else if (d < -Math.PI / 6) ang = baseAng - Math.PI / 6;
                        }
                        x += Math.Cos(ang); y += Math.Sin(ang);
                    }
                }

            if (map.Kind == MineSiteKind.Mine) Spaces(map, rng);
        }

        private static double Wrap(double a) { while (a > Math.PI) a -= 2 * Math.PI; while (a < -Math.PI) a += 2 * Math.PI; return a; }

        private static double Quantile(double[] values, double p)
        {
            if (values.Length == 0) return 0;
            if (p <= 0) return double.NegativeInfinity;
            if (p >= 1) return double.PositiveInfinity;
            var s = (double[])values.Clone();
            Array.Sort(s);
            return s[Math.Min(s.Length - 1, (int)Math.Floor(p * s.Length))];
        }

        private static void Paint(MineMap map, int x, int y, VeinDef v, MineTable table, Dictionary<(int, int), double> wet, double wOre)
        {
            var t = map.Get(x, y);
            if (t == null || t.Rock == RockKind.Bedrock || t.IsOpen) return;
            t.Ore = v.Item;
            t.Rock = v.Rock;
            t.Wet = table.WetOre > 0 && wet.TryGetValue((x, y), out var w) && w > wOre;
        }

        // ---------- doğal kovuklar ve hazır mekânlar ----------

        /// <summary>
        /// Hazır mekân: '#' dokunulmaz, '.' açık, 'w' açık ve sulu, 'r' moloz, 'c' çürümüş ahşap çatkılı açık karo,
        /// 'a' taş kemerli açık karo (doğal kemer: çökmez).
        /// </summary>
        public sealed class Prefab
        {
            public string Id; public string[] Rows;
            public int W => Rows[0].Length; public int H => Rows.Length;
        }

        public static readonly Prefab[] Prefabs =
        {
            new Prefab { Id = "old_gallery", Rows = new[] { "#######", "c.c.c.c", "#######" } },
            new Prefab { Id = "old_gallery_bend", Rows = new[] { "c.c##", "###.#", "###c#", "###.#" } },
            new Prefab { Id = "pool", Rows = new[] { "#a..#", "a.ww.", ".www.", ".ww.a", "#..a#" } },
            new Prefab { Id = "collapsed_passage", Rows = new[] { "######", "c.rr.c", "######" } },
            new Prefab { Id = "arched_room", Rows = new[] { "#a.a#", "a...a", ".....", "a...a", "#a.a#" } },
            new Prefab { Id = "seep", Rows = new[] { "#ww#", "w..w", "#ww#" } },
        };

        private static void Spaces(MineMap map, Random rng)
        {
            var (ex, ey) = MineSystem.MineEntrance;
            bool NearEntrance(int x, int y) => Math.Abs(x - ex) <= 3 && Math.Abs(y - ey) <= 3;
            bool Free(int x0, int y0, int w, int h)
            {
                for (int y = y0 - 1; y <= y0 + h; y++)
                    for (int x = x0 - 1; x <= x0 + w; x++)
                    {
                        var t = map.Get(x, y);
                        if (t == null || t.Rock == RockKind.Bedrock || t.IsOpen || NearEntrance(x, y)) return false;
                    }
                return true;
            }
            // Doğal kovuklar: 2-3, hücresel otomatla yumuşatılmış; tavanı doğal kemer (çökmez).
            int caves = 2 + rng.Next(2);
            for (int k = 0, tries = 0; k < caves && tries < 40; tries++)
            {
                int w = 5 + rng.Next(3), h = 4 + rng.Next(2);
                int x0 = 2 + rng.Next(Math.Max(1, map.Width - w - 4)), y0 = 2 + rng.Next(Math.Max(1, map.Height - h - 4));
                if (!Free(x0, y0, w, h)) continue;
                var cell = new bool[w, h];
                for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                {
                    double dx = (x - (w - 1) / 2.0) / (w / 2.0), dy = (y - (h - 1) / 2.0) / (h / 2.0);
                    cell[x, y] = dx * dx + dy * dy < 0.9 && rng.NextDouble() < 0.75;
                }
                for (int it = 0; it < 2; it++)
                {
                    var next = new bool[w, h];
                    for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                    {
                        int n = 0;
                        for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                        { int nx = x + dx, ny = y + dy; if (nx >= 0 && ny >= 0 && nx < w && ny < h && cell[nx, ny]) n++; }
                        next[x, y] = n >= 5;
                    }
                    cell = next;
                }
                int opened = 0;
                for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                    if (cell[x, y]) { Open(map.Get(x0 + x, y0 + y), SupportKind.StoneArch); opened++; }
                if (opened > 0)
                {
                    k++;
                    // Kovuğun duvar dibinde kristal öbeği (ender: kovukların yarısında)
                    if (rng.NextDouble() < 0.5)
                        for (int y = 0, done = 0; y < h && done == 0; y++)
                            for (int x = 0; x < w && done == 0; x++)
                                if (cell[x, y] && (x == 0 || !cell[x - 1, y])) { map.Props.Add((x0 + x, y0 + y, rng.NextDouble() < 0.4 ? "item.mine_crystal_cluster" : "item.mine_crystal_small")); done = 1; }
                }
            }
            // Hazır mekânlar: 2-3 tanesi tohumla seçilir.
            int count = 2 + rng.Next(2);
            var order = new List<Prefab>(Prefabs);
            for (int i = order.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); var tmp = order[i]; order[i] = order[j]; order[j] = tmp; }
            int placed = 0;
            foreach (var pf in order)
            {
                if (placed >= count) break;
                for (int tries = 0; tries < 30; tries++)
                {
                    int x0 = 2 + rng.Next(Math.Max(1, map.Width - pf.W - 4)), y0 = 2 + rng.Next(Math.Max(1, map.Height - pf.H - 4));
                    if (!Free(x0, y0, pf.W, pf.H)) continue;
                    for (int y = 0; y < pf.H; y++)
                        for (int x = 0; x < pf.W; x++)
                        {
                            var t = map.Get(x0 + x, y0 + y);
                            switch (pf.Rows[y][x])
                            {
                                case '.': Open(t, SupportKind.None); break;
                                case 'w': Open(t, SupportKind.None); t.Wet = true; break;
                                case 'r': t.Rock = RockKind.Rubble; t.Ore = null; break;
                                case 'c': Open(t, SupportKind.Wood); t.SupportAge = 40; break;
                                case 'a': Open(t, SupportKind.StoneArch); break;
                            }
                        }
                    PrefabProps(map, pf, x0, y0, rng);
                    placed++;
                    break;
                }
            }
        }

        /// <summary>Hazır mekânın süsleri: eski galeride çürük direk, göletin kıyısında ıslak taş, kemerli odada unutulmuş sandık
        /// ve kandil oyuğu, çökmüş geçitte devrik el arabası.</summary>
        private static void PrefabProps(MineMap map, Prefab pf, int x0, int y0, Random rng)
        {
            void Add(char want, string id, int nth)
            {
                int seen = 0;
                for (int y = 0; y < pf.H; y++)
                    for (int x = 0; x < pf.W; x++)
                        if (pf.Rows[y][x] == want && seen++ == nth) { map.Props.Add((x0 + x, y0 + y, id)); return; }
            }
            switch (pf.Id)
            {
                case "old_gallery": Add('c', "item.mine_rotten_prop", 1); Add('.', "item.mine_old_lamp_niche", rng.Next(3)); break;
                case "old_gallery_bend": Add('c', "item.mine_rotten_prop", 0); break;
                case "pool": Add('a', "item.mine_pool_stones", 0); Add('a', "item.mine_pool_stones", 3); break;
                case "collapsed_passage": Add('.', "item.mine_broken_barrow", 0); break;
                case "arched_room": Add('.', "item.mine_old_chest", 4); Add('a', "item.mine_old_lamp_niche", 1); break;
            }
        }

        private static void Open(MineTile t, SupportKind support)
        {
            if (t == null) return;
            t.Rock = RockKind.Open; t.Ore = null; t.Support = support;
        }
    }

    /// <summary>Konumdan belirleyici, düzgün değer gürültüsü (0..1). Unity'siz çekirdek ve önizleme aracı aynı sonucu verir.</summary>
    public static class Noise
    {
        private static double Lattice(int x, int y, int salt)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + salt * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }

        private static double Fade(double t) => t * t * (3 - 2 * t);

        public static double Value(double x, double y, int salt)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            double fx = Fade(x - x0), fy = Fade(y - y0);
            double a = Lattice(x0, y0, salt), b = Lattice(x0 + 1, y0, salt), c = Lattice(x0, y0 + 1, salt), d = Lattice(x0 + 1, y0 + 1, salt);
            return (a + (b - a) * fx) * (1 - fy) + (c + (d - c) * fx) * fy;
        }
    }
}
