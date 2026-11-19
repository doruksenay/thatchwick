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
        var file = JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "items.json")), Json);
        file.Merge(JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(content, "life.json")), Json));
        var db = new ContentDatabase();
        var errors = ContentMapper.Apply(file, db);
        foreach (var e in errors) Console.Error.WriteLine("İÇERİK: " + e);
        LayoutHooks.Load(root, db);

        var map = StarterWorld.BuildMap();
        var world = new WorldState(map);
        var log = new List<string>();
        StarterWorld.PlaceItems(world, db, log.Add);
        var plan = LayoutHooks.Plan(world);

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
