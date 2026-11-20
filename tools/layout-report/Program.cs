using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

// Yerleşim raporu: yeni oyunun dünyasını kurar, doğrulayıcıyı çalıştırır, ihlalleri yere ve kurala göre sayar.
// --list: her ihlali tek satır yazar (konum, eşya, sorun türü, kök neden). --md: Markdown tablo.
internal static class Program
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets", "Resources", "Content"))) dir = dir.Parent;
        if (dir == null) throw new DirectoryNotFoundException("Depo kökü bulunamadı.");
        return dir.FullName;
    }

    public static int Main(string[] args)
    {
        bool list = args.Contains("--list") || args.Contains("list"), md = args.Contains("--md") || args.Contains("md");
        var root = RepoRoot();
        var content = Path.Combine(root, "Assets", "Resources", "Content");
        MapFiles.Provider = MapFiles.FromFolder(Path.Combine(content, "maps"), t => JsonSerializer.Deserialize<MapMarkersDto>(t, Json));
        var file = JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "items.json")), Json);
        file.Merge(JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "life.json")), Json));
        var db = new ContentDatabase();
        var errors = ContentMapper.Apply(file, db);
        foreach (var e in errors) Console.Error.WriteLine("İÇERİK: " + e);
        LayoutHooks.Load(root, db);
        bool legacy = args.Contains("legacy");
        var layErr = new List<string>();
        LayoutCatalog layouts = null;
        var layPath = Path.Combine(content, "layouts.json");
        if (!legacy && File.Exists(layPath))
            layouts = LayoutCatalog.Map(JsonSerializer.Deserialize<LayoutFileDto>(File.ReadAllText(layPath), Json), db, layErr);
        foreach (var e in layErr) Console.Error.WriteLine("ŞABLON: " + e);
        int seed = 1;
        foreach (var a in args) if (a.StartsWith("seed=")) seed = int.Parse(a.Substring(5));

        var map = StarterWorld.BuildMap(layouts);
        var world = new WorldState(map);
        var log = new List<string>();
        var res = StarterWorld.Place(world, db, layouts, seed, log.Add);
        var plan = layouts != null ? res.Plan : LayoutHooks.Plan(world);
        int missing = res.Reports.Sum(r => r.Missing.Count), dropped = res.Reports.Sum(r => r.Dropped.Count);
        Console.WriteLine($"Şablon: {res.Reports.Count} oda, kusursuz {res.Reports.Count(r => r.Ok)}, eksik zorunlu {missing}, doğrulayıcının çıkardığı isteğe bağlı {dropped}");
        var dumps = args.Where(a => a.StartsWith("dump=")).Select(a => a.Substring(5)).ToList();
        if (dumps.Contains("all")) { dumps = new List<string> { "farmhouse" }; dumps.AddRange(Regions.TownBuildings.Select(b => b.Id)); }
        foreach (var id in dumps)
        {
            var reg = Regions.All.FirstOrDefault(x => x.Id == id);
            if (reg != null)
            {
                var leg = new Dictionary<string, string>();
                Console.WriteLine($"== {reg.Name} (bölge) ==");
                Console.Write(Dump.Building(world, new RectI(reg.X, reg.Y, reg.W, reg.H), 0, leg));
                Console.WriteLine("   " + string.Join(", ", leg.OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}")));
                continue;
            }
            var b = Regions.TownBuildingById(id);
            RectI r = b != null ? TownBuilder.Grow(b.Rect, args.Contains("wide") ? 4 : 0) : new RectI(StarterWorld.HouseX0 - (args.Contains("wide") ? 6 : 0), StarterWorld.HouseY0 - (args.Contains("wide") ? 6 : 0), 9 + (args.Contains("wide") ? 12 : 0), 7 + (args.Contains("wide") ? 14 : 0));
            for (int z = 1; z >= -1; z--)
            {
                if (!Enumerable.Range(r.X, r.W).Any(x => world.Map.HasTile(new GridPos(x, r.Y, z)))) continue;
                var legend = new Dictionary<string, string>();
                Console.WriteLine($"== {(b != null ? b.Name : "Çiftlik evi")} kat {z} ==");
                Console.Write(Dump.Building(world, r, z, legend));
                Console.WriteLine("   " + string.Join(", ", legend.OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}")));
            }
        }
        // json=<bölge|bina>: karo, kenar ve eşya dökümü (tools/layout-report/render.py ile PNG; gözle denetim)
        foreach (var id in args.Where(a => a.StartsWith("json=")).Select(a => a.Substring(5)))
        {
            var reg = Regions.All.FirstOrDefault(x => x.Id == id);
            var bb = reg == null ? Regions.TownBuildingById(id) : null;
            RectI rr = reg != null ? new RectI(reg.X, reg.Y, reg.W, reg.H) : bb != null ? TownBuilder.Grow(bb.Rect, 3) : new RectI(StarterWorld.HouseX0 - 4, StarterWorld.HouseY0 - 4, 17, 15);
            var levels = reg != null ? new[] { 0 } : new[] { 0, 1, -1 };
            var outp = new List<object>();
            foreach (var z in levels)
            {
                if (!Enumerable.Range(rr.X, rr.W).Any(x => Enumerable.Range(rr.Y, rr.H).Any(y => world.Map.HasTile(new GridPos(x, y, z))))) continue;
                var tiles = new List<object>();
                for (int y = rr.Y; y < rr.Y + rr.H; y++)
                    for (int x = rr.X; x < rr.X + rr.W; x++)
                    {
                        var p = new GridPos(x, y, z);
                        if (!world.Map.HasTile(p)) continue;
                        tiles.Add(new { x = x - rr.X, y = y - rr.Y, f = world.Map.GetFloor(p).ToString(), r = world.Map.GetRoad(p).ToString(), b = world.Map.IsBlocked(p),
                            eh = world.Map.GetEdge(new Edge(x, y, EdgeAxis.Horizontal, z)).ToString(), ev = world.Map.GetEdge(new Edge(x, y, EdgeAxis.Vertical, z)).ToString() });
                    }
                var items = world.Items.Where(it => it.Position.Z == z && it.Position.X >= rr.X && it.Position.X < rr.X + rr.W && it.Position.Y >= rr.Y && it.Position.Y < rr.Y + rr.H)
                    .Select(it => new { x = it.Position.X - rr.X, y = it.Position.Y - rr.Y, id = it.Def.Id, name = it.Def.DisplayName, pl = it.Def.Placement.ToString(),
                        occ = PlacementValidator.Occupies(it), tiles = world.Map.HasTile(it.Position) ? FootTiles(world, it, rr) : new List<int[]>() }).ToList();
                outp.Add(new { z, w = rr.W, h = rr.H, tiles, items });
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"layout_{id}.json"), JsonSerializer.Serialize(outp));
            Console.WriteLine($"json: {Path.Combine(Path.GetTempPath(), $"layout_{id}.json")}");
        }
        // noart: dünyadaki görselsiz (items.json "art" yok) eşyaların kimlik ve adet listesi (bitki ve toplama hariç)
        if (args.Contains("noart"))
            foreach (var g in world.Items.Where(it => !it.IsPlant && !it.Def.IsForage && it.Def.Art == null).GroupBy(it => it.Def.Id).OrderByDescending(g => g.Count()))
                Console.WriteLine($"NOART {g.Key} {g.Count()} {g.First().Def.DisplayName}");
        if (args.Contains("rooms"))
            foreach (var r in res.Reports)
                Console.WriteLine($"  {r.Room}: {r.Placed.Count} eşya, deneme {r.Attempt + 1}" + (r.Missing.Count > 0 ? $", EKSİK {string.Join(",", r.Missing)}" : "") + (r.Violations.Count > 0 ? $", İHLAL {string.Join(" | ", r.Violations)}" : "") + (r.Dropped.Count > 0 ? $", çıkarılan: {string.Join(" | ", r.Dropped)}" : ""));

        var violations = LayoutHooks.Validate(world, plan, db);
        var sb = new StringBuilder();
        sb.AppendLine($"Eşya: {world.Items.Count}, yerleştirilemeyen: {log.Count}, ihlal: {violations.Count} (hata {violations.Count(v => v.Severity == Severity.Error)})");
        foreach (var g in violations.GroupBy(v => v.Code).OrderByDescending(g => g.Count()))
            sb.AppendLine($"  {Rules.Name(g.Key),-34} {g.Count(),4}");
        sb.AppendLine("Yere göre:");
        foreach (var g in violations.GroupBy(v => Where(plan, v.At)).OrderBy(g => g.Key))
            sb.AppendLine($"  {g.Key,-34} {g.Count(),4}   " + string.Join(", ", g.GroupBy(v => v.Code).Select(c => $"{Rules.Name(c.Key)} {c.Count()}")));
        sb.AppendLine("Görselsiz (kutu) eşya / toplam, yere göre (bitki hariç):");
        foreach (var kv in LayoutHooks.ArtCoverage(world, plan, p => Where(plan, p)).OrderBy(k => k.Key))
            sb.AppendLine($"  {kv.Key,-34} {kv.Value.boxes,4} / {kv.Value.total}");
        if (list || md)
        {
            sb.AppendLine();
            if (md) sb.AppendLine("| Yer | Konum | Eşya | Sorun | Neden | Kök neden |\n|---|---|---|---|---|---|");
            foreach (var v in violations.OrderBy(v => Where(plan, v.At)).ThenBy(v => v.Code))
            {
                var name = v.ItemDefId != null && db.Items.TryGet(v.ItemDefId, out var d) ? d.DisplayName : "-";
                if (md) sb.AppendLine($"| {Where(plan, v.At)} | {Local(v.At)} | {name} | {Rules.Name(v.Code)} | {v.Reason} | {LayoutHooks.RootCause(v, world, plan)} |");
                else sb.AppendLine($"{Where(plan, v.At)} {Local(v.At)} | {name} | {Rules.Name(v.Code)} | {v.Reason} | {LayoutHooks.RootCause(v, world, plan)}");
            }
            if (log.Count > 0) { sb.AppendLine(); sb.AppendLine("Yerleştirilemeyenler:"); foreach (var l in log) sb.AppendLine("  " + l); }
        }
        Console.Write(sb.ToString());
        return 0;
    }

    private static List<int[]> FootTiles(WorldState w, Thatchwick.Core.World.ItemInstance it, RectI rr)
    {
        var list = new List<int[]>();
        for (int dy = -3; dy <= 3; dy++)
            for (int dx = -3; dx <= 3; dx++)
            {
                var p = new GridPos(it.Position.X + dx, it.Position.Y + dy, it.Position.Z);
                if (w.GetItemAt(p) == it) list.Add(new[] { p.X - rr.X, p.Y - rr.Y });
            }
        return list;
    }

    private static string Local(GridPos p)
    {
        var r = Regions.At(p);
        if (r == null) return p.ToString();
        var lp = new GridPos(p.X - r.X, p.Y - r.Y, p.Z);
        return $"{lp}";
    }

    internal static string Where(LayoutPlan plan, GridPos p)
    {
        var site = plan?.SiteAt(p);
        if (site != null) return site.Name + (p.Z > 0 ? " (üst kat)" : p.Z < 0 ? " (bodrum)" : "");
        var r = Regions.At(p);
        return r == null ? "harita dışı" : r.Name;
    }
}
