using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

// Harita önizlemesi (belge 64 §3, Katman 0). Yeni oyunun dünyasını kurar ve her bölge için çizim listesi yazar:
// zemin, yol, kenarlar (tam boy ve alçak görsel adıyla), eşyalar, bitkiler, çatılar. Görsel seçimi oyunun kurallarını
// izler (Game/IsoArt, WorldView, WallView, ItemViews); görseli olmayan eşya "missing" olarak işaretlenir.
// Kullanım: dotnet run --project tools/map-preview -- out=<klasör> [seed=1] [regions=road,town]
internal static class Program
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    private static HashSet<string> _art;
    private static ContentDatabase _db;

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets", "Resources", "Content"))) dir = dir.Parent;
        if (dir == null) throw new DirectoryNotFoundException("Depo kökü bulunamadı.");
        return dir.FullName;
    }

    private sealed class Draw
    {
        public string s;          // görsel adı (dosya adı, uzantısız); null ise yer tutucu
        public string low;        // duvar: alçak görsel (kesit kipinde ön duvar)
        public string k;          // tür: ground, path, rug, wall, item, plant, decor, roof, scenery
        public float gx, gy;      // ızgara konumu (kat yüksekliği hariç)
        public float lift;        // dünya birimi (yukarı)
        public float px = -1, py; // pivot (piksel, soldan ve alttan); -1 = görselin ortası
        public int order;
        public int z;
        public bool flip;
        public float scale = 1f;
        public float[] tint;      // çarpan (r, g, b)
        public int[] color;       // yer tutucu rengi (r, g, b, a)
        public string label;      // eksik görselde yazılacak ad
        public bool front;        // duvar: ön duvar mı (kesitte alçalır)
        public bool missing;      // görseli olmayan eşya
        public int w = 1, h = 1;  // ayak izi (yer tutucu için)
    }

    private static string Has(string name) => name != null && _art.Contains(name) ? name : null;

    private static int Hash(int x, int y) { int h = (x * 73856093) ^ (y * 19349663); return h < 0 ? -h : h; }

    private static int SortOrder(float worldY, int bias, int level) => -(int)Math.Round((worldY - level * 3f) * 20f, MidpointRounding.ToEven) + bias + level * 14000;
    private static float WorldY(float gx, float gy, int level = 0) => (gx + gy) * 0.5f + level * 3f;

    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        string outDir = null; int seed = 1; string[] only = null;
        foreach (var a in args)
        {
            if (a.StartsWith("out=")) outDir = a.Substring(4);
            else if (a.StartsWith("seed=")) seed = int.Parse(a.Substring(5));
            else if (a.StartsWith("regions=")) only = a.Substring(8).Split(',');
        }
        var root = RepoRoot();
        if (outDir == null) outDir = Path.Combine(root, "art", "preview", "_dump");
        Directory.CreateDirectory(outDir);
        var content = Path.Combine(root, "Assets", "Resources", "Content");
        MapFiles.Provider = MapFiles.FromFolder(Path.Combine(content, "maps"), t => JsonSerializer.Deserialize<MapMarkersDto>(t, Json));
        var file = JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "items.json")), Json);
        file.Merge(JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "life.json")), Json));
        _db = new ContentDatabase();
        foreach (var e in ContentMapper.Apply(file, _db)) Console.Error.WriteLine("İÇERİK: " + e);
        var layErr = new List<string>();
        var layouts = LayoutCatalog.Map(JsonSerializer.Deserialize<LayoutFileDto>(File.ReadAllText(Path.Combine(content, "layouts.json")), Json), _db, layErr);
        foreach (var e in layErr) Console.Error.WriteLine("ŞABLON: " + e);

        _art = new HashSet<string>(StringComparer.Ordinal);
        var artRoot = Path.Combine(root, "Assets", "Art", "Iso");
        var index = new Dictionary<string, string>();
        foreach (var f in Directory.EnumerateFiles(artRoot, "*.png", SearchOption.AllDirectories))
        {
            var rel = f.Replace('\\', '/');
            if (rel.Contains("/_ham/")) continue;
            if (rel.Contains("/anim/") && !rel.Contains("_r84_") && !rel.Contains("_r64_")) continue;
            var name = Path.GetFileNameWithoutExtension(f);
            _art.Add(name);
            // Kökteki dosya adaydakinden önce gelir (oyunun kütüphanesi adı tek tutar).
            if (!index.ContainsKey(name) || !rel.Contains("/_aday/")) index[name] = Path.GetRelativePath(root, f).Replace('\\', '/');
        }
        File.WriteAllText(Path.Combine(outDir, "art-index.json"), JsonSerializer.Serialize(index));

        if (args.Contains("export"))
        {
            // Taslak tabanı (tools/maps/draw.py): taslaksız, kod çizimiyle kurulan arazi bölge bölge karakter ızgarası olarak.
            MapFiles.Provider = null; MapFiles.Reset();
            var plain = StarterWorld.BuildMap(layouts);
            foreach (var r in Regions.All)
            {
                var rows = new List<string>();
                for (int y = r.Y; y < r.Y + r.H; y++)
                {
                    var sb = new System.Text.StringBuilder();
                    for (int x = r.X; x < r.X + r.W; x++)
                    {
                        var p = new GridPos(x, y);
                        var f = plain.GetFloor(p);
                        char c = plain.GetRoad(p) != RoadType.None ? '=' : f == FloorType.Water ? '~' : f == FloorType.Wood ? 'b' : f == FloorType.Stone ? 'S'
                            : f == FloorType.Dirt ? 'd' : f == FloorType.Sand ? 's' : plain.IsBlocked(p) ? '#' : '.';
                        sb.Append(c);
                    }
                    rows.Add(sb.ToString());
                }
                File.WriteAllText(Path.Combine(outDir, "base-" + r.Id + ".json"), JsonSerializer.Serialize(new { id = r.Id, x = r.X, y = r.Y, w = r.W, h = r.H, rows }));
            }
            Console.WriteLine("taban yazıldı: " + outDir);
            return 0;
        }

        var map = StarterWorld.BuildMap(layouts);
        var world = new WorldState(map);
        var log = new List<string>();
        var res = StarterWorld.Place(world, _db, layouts, seed, log.Add);
        File.WriteAllLines(Path.Combine(outDir, "log.txt"), log);

        var occupied = new HashSet<GridPos>();
        foreach (var it in world.Items) if (!it.IsDecor && !it.IsWallMounted && !it.IsCeiling) foreach (var t in it.OccupiedTiles()) occupied.Add(t);
        PreviewExtras.TerrainBlocked = p => map.IsBlocked(p) && !occupied.Contains(p);
        var roofs = RoofBuilder.Compute(map);
        if (args.Contains("conn"))
        {
            var comp = OutdoorRules.RoadComponents(map, out int main);
            foreach (var q in new[] { new GridPos(107, 16), new GridPos(107, 17), new GridPos(107, 25), new GridPos(100, 40), new GridPos(139, 28), new GridPos(412, 23), new GridPos(419, 5) })
                Console.WriteLine($"{q}: comp {(comp.TryGetValue(q, out var c) ? c : -1)} main {main} road {map.GetRoad(q)} floor {map.GetFloor(q)}");
        }
        foreach (var r in Regions.All)
        {
            if (only != null && !only.Contains(r.Id)) continue;
            var draws = new List<Draw>();
            Ground(map, r, draws);
            Paths(map, r, draws);
            Walls(map, r, draws);
            Roofs(map, r, roofs, draws);
            Items(world, r, draws);
            Extra(world, r, draws);
            var sites = new List<object>();
            foreach (var s in res.Plan.Sites)
                if (s.Building.W > 0 && r.Contains(new GridPos(s.Building.X, s.Building.Y)))
                    sites.Add(new { id = s.Id, name = s.Name, x = s.Building.X, y = s.Building.Y, w = s.Building.W, h = s.Building.H, minLevel = s.MinLevel, maxLevel = s.MaxLevel });
            int missing = draws.Count(d => d.missing);
            var doc = new { id = r.Id, name = r.Name, x = r.X, y = r.Y, w = r.W, h = r.H, seed, sites, draws };
            File.WriteAllText(Path.Combine(outDir, r.Id + ".json"), JsonSerializer.Serialize(doc, Json));
            Console.WriteLine($"{r.Id}: {draws.Count} çizim, görseli olmayan {missing}");
        }
        return 0;
    }

    // ---------------- zemin (WorldView.PaintTile) ----------------

    private static void Ground(GridMap map, Region r, List<Draw> draws)
    {
        for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
            for (int y = r.Y; y < r.Y + r.H; y++)
                for (int x = r.X; x < r.X + r.W; x++)
                {
                    var p = new GridPos(x, y, z);
                    if (z != 0 && !map.HasTile(p)) continue;
                    var d = new Draw { k = "ground", gx = x + 0.5f, gy = y + 0.5f, z = z, order = -10000 + z * 14000 };
                    GroundLookup.Paint(map, p, _db, Has, out d.s, out d.flip, out d.tint, out d.color);
                    draws.Add(d);
                }
    }

    private static void Paths(GridMap map, Region r, List<Draw> draws)
    {
        if (Has("iso_path_dirt_wang_03_a") == null && Has("iso_path_dirt_wang_03_b") == null && Has("iso_path_dirt_wang_03_c") == null) return;
        bool In(int x, int y) => x >= r.X && y >= r.Y && x < r.X + r.W && y < r.Y + r.H && map.InBounds(new GridPos(x, y));
        bool Dirt(int x, int y)
        {
            if (!In(x, y)) return false;
            var q = new GridPos(x, y);
            if (map.GetRoad(q) != RoadType.Path) return false;
            return !(map.IsNaturalRoad(q) && map.GetRoadCondition(q) < 0.5f);
        }
        bool Join(int x, int y)
        {
            if (!In(x, y)) return false;
            var f = map.GetFloor(new GridPos(x, y));
            return f == FloorType.Stone || f == FloorType.Wood;
        }
        string[] letters = { "a", "b", "c" };
        for (int vy = r.Y - 1; vy < r.Y + r.H; vy++)
            for (int vx = r.X - 1; vx < r.X + r.W; vx++)
            {
                bool top = Dirt(vx + 1, vy + 1), right = Dirt(vx + 1, vy), bottom = Dirt(vx, vy), left = Dirt(vx, vy + 1);
                if (!top && !right && !bottom && !left) continue;
                top |= Join(vx + 1, vy + 1); right |= Join(vx + 1, vy); bottom |= Join(vx, vy); left |= Join(vx, vy + 1);
                int mask = (top ? 1 : 0) | (right ? 2 : 0) | (bottom ? 4 : 0) | (left ? 8 : 0);
                int variant = (int)(Noise(vx, vy, 404) * 3) % 3;
                string s = null;
                for (int i = 0; i < 3 && s == null; i++)
                {
                    string v = letters[(variant + i) % 3];
                    s = mask >= 15 ? Has($"iso_path_dirt_fill_v2_{v}") : Has($"iso_path_dirt_wang_{mask:00}_{v}");
                }
                if (s == null && mask >= 15) s = Has("iso_path_dirt_fill_a") ?? Has("iso_ground_dirt_a");
                if (s == null) continue;
                draws.Add(new Draw { k = "path", s = s, gx = vx + 1f, gy = vy + 1f, order = -9999 });
            }
    }

    private static float Noise(int x, int y, int salt)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + salt * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFF) / 65535f;
        }
    }

    // ---------------- kenarlar (WallView.Rebuild) ----------------

    private static string WallPart(string partId, EdgeAxis axis, string variant = null)
    {
        if (string.IsNullOrEmpty(partId)) return null;
        string a = axis == EdgeAxis.Horizontal ? "sw_ne" : "se_nw";
        string v = string.IsNullOrEmpty(variant) ? "" : "_" + variant;
        return Has($"iso_wall_{partId}{v}_{a}");
    }

    private static bool IsFrontWall(GridMap map, Edge e)
    {
        var rooms = map.Rooms;
        if (rooms != null)
        {
            var back = map.InBounds(e.SideB) ? rooms.RoomAt(e.SideB) : null;
            var front = map.InBounds(e.SideA) ? rooms.RoomAt(e.SideA) : null;
            if (back != null && back.IsIndoor) return front == null || !front.IsIndoor;
        }
        bool behindFloor = map.InBounds(e.SideB) && map.GetFloor(e.SideB) != FloorType.Grass;
        bool frontFloor = map.InBounds(e.SideA) && map.GetFloor(e.SideA) != FloorType.Grass;
        return behindFloor && !frontFloor;
    }

    private static string LowPartFor(GridMap map, Edge e, EdgeKind kind, string style)
    {
        if (kind == EdgeKind.Wall || kind == EdgeKind.HalfWall) return BuildParts.Resolve(_db, BuildPartKind.WallMaterial, style)?.Id ?? "wall";
        foreach (int d in new[] { -1, 1, -2, 2 })
        {
            var n = e.Axis == EdgeAxis.Horizontal ? new Edge(e.X + d, e.Y, e.Axis, e.Z) : new Edge(e.X, e.Y + d, e.Axis, e.Z);
            if (!map.EdgeInBounds(n) || map.GetEdge(n) != EdgeKind.Wall) continue;
            return BuildParts.Resolve(_db, BuildPartKind.WallMaterial, map.GetLook(n)?.Style)?.Id ?? "wall";
        }
        return "wall";
    }

    private static void Walls(GridMap map, Region r, List<Draw> draws)
    {
        foreach (var kv in map.Edges)
        {
            var e = kv.Key; var kind = kv.Value;
            if (e.X < r.X || e.Y < r.Y || e.X > r.X + r.W || e.Y > r.Y + r.H) continue;
            bool fullHeight = EdgeKinds.IsRoomBoundary(kind) || kind == EdgeKind.HalfWall;
            var look = map.GetLook(e);
            string finish = EdgeKinds.IsRoomBoundary(kind) ? look?.FinishA : null;
            bool plain = look?.Style == null && finish == null;
            var pk = BuildService.PartKindFor(kind);
            var part = pk.HasValue ? BuildParts.Resolve(_db, pk.Value, look?.Style) : null;
            string Pick(bool low)
            {
                string variant = kind == EdgeKind.HalfWall ? (low ? "low" : "half") : low ? "low" : null;
                var finishArt = finish != null ? WallPart(finish, e.Axis, variant) : null;
                var real = finishArt ?? WallPart(part?.Id, e.Axis, variant);
                if (low && real == null) real = WallPart(LowPartFor(map, e, kind, look?.Style), e.Axis, "low") ?? WallPart("wall", e.Axis, "low");
                if (!low && real == null && look?.Style == null && (kind == EdgeKind.Wall || kind == EdgeKind.Door || kind == EdgeKind.Window))
                    real = Has($"iso_wall_{(kind == EdgeKind.Door ? "door" : "wall")}_{(e.Axis == EdgeAxis.Horizontal ? "sw_ne" : "se_nw")}");
                return real;
            }
            float mx = e.X + (e.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), my = e.Y + (e.Axis == EdgeAxis.Vertical ? 0.5f : 0f);
            var d = new Draw
            {
                k = "wall", s = Pick(false), low = fullHeight ? Pick(true) : null, gx = e.X, gy = e.Y, z = e.Z,
                px = e.Axis == EdgeAxis.Horizontal ? 32f : 70f, py = 19f, order = SortOrder(WorldY(mx, my, e.Z), 0, e.Z),
                front = fullHeight && IsFrontWall(map, e), label = kind.ToString() + (look?.Style != null ? ":" + look.Style : ""),
                flip = e.Axis == EdgeAxis.Vertical && false,
            };
            if (kind == EdgeKind.Window && plain) d.tint = new[] { 0.75f, 0.85f, 1f };
            if (d.s == null) { d.missing = kind != EdgeKind.Window; d.color = new[] { 199, 173, 133, 255 }; d.label = "kenar " + d.label + (e.Axis == EdgeAxis.Horizontal ? " /" : " \\"); }
            draws.Add(d);
        }
    }

    // ---------------- çatılar (WallView.DrawRoofs) ----------------

    private static void Roofs(GridMap map, Region r, List<RoofRegion> roofs, List<Draw> draws)
    {
        foreach (var region in roofs)
        {
            var part = BuildParts.Resolve(_db, BuildPartKind.Roof, region.Style);
            foreach (var t in region.Tiles)
            {
                if (!r.Contains(new GridPos(t.Pos.X, t.Pos.Y))) continue;
                if (map.GetRoofStyle(t.Pos) == GridMap.NoRoof) continue;
                string n = part?.Id != null && part.Id.StartsWith("roof_") ? part.Id.Substring(5) : part?.Id;
                string s = n == null ? null : (t.IsRidge ? Has($"iso_roof_{n}_ridge") : null) ?? Has($"iso_roof_{n}_a");
                float shade = t.IsRidge ? 1.08f : t.Down.Y < 0 ? 1.0f : t.Down.X < 0 ? 0.9f : t.Down.Y > 0 ? 0.72f : 0.8f;
                var d = new Draw
                {
                    k = "roof", s = s, gx = t.Pos.X + 0.5f, gy = t.Pos.Y + 0.5f, z = t.Pos.Z, lift = 40f / 32f + (t.Height - 0.5f) * 0.45f,
                    order = SortOrder(WorldY(t.Pos.X + 0.5f, t.Pos.Y + 0.5f, t.Pos.Z), 60 + t.Height, t.Pos.Z), tint = new[] { shade, shade, shade },
                };
                if (s == null) d.color = new[] { (int)(184 * shade), (int)(153 * shade), (int)(87 * shade), 255 };
                draws.Add(d);
            }
        }
    }

    // ---------------- eşyalar (ItemViews) ----------------

    private static readonly Dictionary<string, string> Trees = new Dictionary<string, string>
    {
        ["item.tree_oak"] = "iso_tree_oak", ["item.tree_apple"] = "iso_tree_apple", ["item.tree_pear"] = "iso_tree_pear", ["item.tree_cherry"] = "iso_tree_cherry",
        ["item.tree_plum"] = "iso_tree_plum", ["item.tree_birch"] = "iso_tree_birch", ["item.tree_pine"] = "iso_tree_pine", ["item.tree_willow"] = "iso_tree_willow", ["item.tree_olive"] = "iso_plant_olive", ["item.vine_grape"] = "iso_plant_grape", ["item.bush_coffee"] = "iso_plant_coffee",
    };

    private static (float x, float y) PivotOf(ItemDef def)
    {
        var a = def.Art;
        if (a != null && (a.PivotX > 0f || a.PivotY > 0f)) return (a.PivotX, a.PivotY);
        if (def.Placement == ItemPlacement.Surface) return (16f, 7f);
        int big = Math.Max(def.Width, def.Height);
        return big <= 1 ? (40f, 24f) : def.Width * def.Height == 2 ? (56f, 32f) : (72f, 40f);
    }

    private static string ItemSprite(ItemInstance item, out bool flip)
    {
        flip = false;
        if (!ItemFacing.TryView(item.Def, item.Rotation, item.IsLit, out var name, out var f)) return null;
        if (Has(name) == null) return null;
        flip = f;
        return name;
    }

    private static string PlantSprite(ItemInstance item, out bool staged)
    {
        staged = false;
        string id = item.Def.Id;
        if (id.StartsWith("item.crop_"))
        {
            string crop = "iso_crop_" + id.Substring("item.crop_".Length);
            int st = item.IsGrown ? 3 : item.GrowthFraction < 0.5f ? 1 : 2;
            for (int k = st; k >= 1; k--) if (Has($"{crop}_{k}_se") != null) { staged = true; return $"{crop}_{k}_se"; }
            return null;
        }
        if (!Trees.TryGetValue(id, out var tree)) return null;
        if (!item.IsGrown)
        {
            if (Has(tree + "_young_se") != null) { staged = true; return tree + "_young_se"; }
            return Has(tree + "_grown_se");
        }
        staged = true;
        return Has(tree + "_grown_se") ?? Has(tree + "_young_se");
    }

    private static void Items(WorldState world, Region r, List<Draw> draws)
    {
        foreach (var item in world.Items)
        {
            var anchor = item.IsDecor && item.ParentId.HasValue ? world.GetItem(item.ParentId.Value)?.Position ?? item.Position : item.Position;
            if (!r.Contains(new GridPos(anchor.X, anchor.Y))) continue;
            int level = item.Position.Z;
            string label = item.Def.Id.StartsWith("item.") ? item.Def.Id.Substring(5) : item.Def.Id;
            if (item.IsWallMounted)
            {
                // Ön duvarın iç yüzündeki eşya kesitte duvarın arkasında kalır; önizlemede de çizilmez (oyunla aynı).
                int n = ((item.Rotation % 4) + 4) % 4;
                if (n == 0 || n == 3) continue;
                var (ax, ay) = WallSides.AlongWall(item.Rotation, 0.5f);
                float band = item.WallBand <= 0 ? 0.6f : item.WallBand == 1 ? 1.3f : 2.1f;
                var e = WallSides.EdgeOf(item.Position, item.Rotation);
                float mx = e.X + (e.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), my = e.Y + (e.Axis == EdgeAxis.Vertical ? 0.5f : 0f);
                var s = ItemSprite(item, out bool wf);
                var pv = PivotOf(item.Def);
                draws.Add(new Draw { k = "item", s = s, flip = wf, gx = item.Position.X + ax, gy = item.Position.Y + ay, lift = band, z = level, px = pv.x, py = pv.y,
                    order = SortOrder(WorldY(mx, my, level), 2, level) + item.WallBand, missing = s == null, label = label, color = new[] { 255, 0, 255, 255 }, scale = s == null ? 0.3f : 1f });
                continue;
            }
            if (item.IsCeiling) continue;   // tavan eşyası kesitte kafa karıştırır; ayrı listelenir
            if (item.IsDecor)
            {
                var parent = item.ParentId.HasValue ? world.GetItem(item.ParentId.Value) : null;
                if (parent == null || !parent.TryGetSlotPosition(item.SlotId, out float gx, out float gy)) continue;
                var s = ItemSprite(item, out bool df);
                var pv = PivotOf(item.Def);
                if (parent.IsWallMounted)
                {
                    int n = ((parent.Rotation % 4) + 4) % 4;
                    if (n == 0 || n == 3) continue;
                    float band = parent.WallBand <= 0 ? 0.6f : parent.WallBand == 1 ? 1.3f : 2.1f;
                    var e = WallSides.EdgeOf(parent.Position, parent.Rotation);
                    float mx = e.X + (e.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), my = e.Y + (e.Axis == EdgeAxis.Vertical ? 0.5f : 0f);
                    draws.Add(new Draw { k = "decor", s = s, flip = df, gx = gx, gy = gy, lift = band + 0.1f, z = level, px = pv.x, py = pv.y,
                        order = SortOrder(WorldY(mx, my, level), 2, level) + parent.WallBand + 1, missing = s == null, label = label, color = new[] { 255, 0, 255, 255 }, scale = s == null ? 0.25f : 1f });
                    continue;
                }
                var slotDef = parent.Def.FindSlot(item.SlotId);
                if (slotDef != null && slotDef.IsCover) continue;
                float topPx = ItemSprite(parent, out _) != null ? 27f : 24f;
                draws.Add(new Draw { k = "decor", s = s, flip = df, gx = gx, gy = gy, lift = topPx / 32f, z = level, px = pv.x, py = pv.y,
                    order = SortOrder(WorldY(gx, gy, level), 0, level) + 2, missing = s == null, label = label, color = new[] { 255, 0, 255, 255 }, scale = s == null ? 0.35f : 1f });
                continue;
            }
            float cx = item.Position.X + item.Width * 0.5f, cy = item.Position.Y + item.Height * 0.5f;
            if (item.IsRug)
            {
                var s = ItemSprite(item, out bool rf);
                draws.Add(new Draw { k = "rug", s = s, flip = rf, gx = cx, gy = cy, z = level, order = -9990 + level * 14000, missing = s == null && item.Def.Id != Decor.Debris,
                    label = label, color = new[] { 255, 0, 255, 255 }, w = item.Width, h = item.Height });
                continue;
            }
            if (GreatTrees.IsGreatTree(item.Def))
            {
                bool big = item.Width >= 3;
                string species = item.Def.Id.StartsWith("item.great_") ? item.Def.Id.Substring("item.great_".Length) : "oak";
                var s = Has($"iso_greattree_{species}_summer_se");
                draws.Add(new Draw { k = "plant", s = s, gx = cx, gy = cy, z = level, px = big ? 128f : 96f, py = big ? 56f : 40f, order = SortOrder(WorldY(cx, cy, level), 1, level),
                    missing = s == null, label = label, color = new[] { 255, 0, 255, 255 }, w = item.Width, h = item.Height });
                continue;
            }
            if (item.IsPlant)
            {
                var s = PlantSprite(item, out bool staged);
                float scale = !staged ? 0.3f + 0.7f * item.GrowthFraction : item.Def.IsCrop || item.IsGrown ? 1f : 0.75f + 0.25f * item.GrowthFraction;
                if (s == null)
                {
                    // Bitki eşyası ama ağaç/ekin tablosunda yok: eşyanın kendi görseli (toplama düğümü, çalı).
                    s = ItemSprite(item, out bool pf);
                    var pv2 = PivotOf(item.Def);
                    draws.Add(new Draw { k = "plant", s = s, flip = pf, gx = cx, gy = cy, z = level, px = pv2.x, py = pv2.y, order = SortOrder(WorldY(cx, cy, level), s != null ? 0 : 1, level),
                        missing = s == null, label = label, color = new[] { 255, 0, 255, 255 } });
                    continue;
                }
                draws.Add(new Draw { k = "plant", s = s, gx = item.Position.X + 0.5f, gy = item.Position.Y + 0.5f, z = level, px = 40f, py = 24f, scale = scale,
                    order = SortOrder(WorldY(item.Position.X + 0.5f, item.Position.Y + 0.5f, level), 1, level), label = label });
                continue;
            }
            {
                var s = ItemSprite(item, out bool f);
                var pv = PivotOf(item.Def);
                draws.Add(new Draw { k = "item", s = s, flip = f, gx = cx, gy = cy, z = level, px = pv.x, py = pv.y, order = SortOrder(WorldY(cx, cy, level), 0, level),
                    missing = s == null, label = label, color = new[] { 255, 0, 255, 255 }, w = item.Width, h = item.Height });
            }
        }
    }

    /// <summary>Oyun katmanının çizdiği, eşya olmayan dekor (kenar bandı ağaçları, kaya sırtı): Core'dan okunur.</summary>
    private static void Extra(WorldState world, Region r, List<Draw> draws)
    {
        PreviewExtras.Add(world, r, Has, (s, gx, gy, px, py, scale, flip, bias) =>
            draws.Add(new Draw { k = "scenery", s = s, gx = gx, gy = gy, px = px, py = py, scale = scale, flip = flip, order = SortOrder(WorldY(gx, gy, 0), bias, 0) }));
    }
}

/// <summary>Zemin karosunun görseli: oyunun WorldView.PaintTile kuralları (yapı modu ve su yolu boyamaları hariç).</summary>
internal static class GroundLookup
{
    private static int Hash(int x, int y) { int h = (x * 73856093) ^ (y * 19349663); return h < 0 ? -h : h; }

    public static void Paint(GridMap map, GridPos p, ContentDatabase db, Func<string, string> has, out string sprite, out bool flip, out float[] tint, out int[] color)
    {
        sprite = null; flip = false; tint = null; color = null;
        int x = p.X, y = p.Y; bool even = ((x + y) & 1) == 0;
        var floor = map.GetFloor(p);
        if (p.Z != 0)
        {
            var part0 = map.GetFloorStyle(p) != null ? map.GetFloorStyle(p) : BuildParts.DefaultFloor(db, floor)?.Id;
            sprite = FloorPart(part0, x, y, has);
            if (sprite == null) color = floor == FloorType.Stone ? (even ? new[] { 140, 140, 148, 255 } : new[] { 130, 130, 138, 255 })
                : floor == FloorType.Dirt ? (even ? new[] { 120, 94, 68, 255 } : new[] { 112, 88, 62, 255 }) : (even ? new[] { 168, 124, 76, 255 } : new[] { 158, 115, 68, 255 });
            return;
        }
        PreviewExtras.Ground(map, p, db, has, out sprite, out flip, out tint, out color);
    }

    public static string FloorPart(string partId, int x, int y, Func<string, string> has)
    {
        if (string.IsNullOrEmpty(partId)) return null;
        string n = partId.StartsWith("floor_") ? partId.Substring(6) : partId;
        return has($"iso_floor_{n}_{(Hash(x, y) % 2 == 0 ? "a" : "b")}") ?? has($"iso_floor_{n}_a");
    }
}
