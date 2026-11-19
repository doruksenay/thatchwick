using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

/// <summary>
/// Odanın ASCII kat planı (ekran görüntüsü alınamayan ortamda gözle denetim için). Izgara: x sağa, y aşağı (N yukarı).
/// Kenarlar: '#' duvar, 'D' kapı, '=' pencere, ':' çit; karo: eşya kısaltması (ön yüzlü eşyada yön oku ^ > v <), '.' boş,
/// 'k' boş kalacak, ' ' karo yok.
/// </summary>
internal static class Dump
{
    public static string Building(WorldState w, RectI r, int z, Dictionary<string, string> legend)
    {
        var map = w.Map;
        var sb = new StringBuilder();
        for (int y = r.Y - 1; y <= r.Y + r.H; y++)
        {
            var top = new StringBuilder(); var row = new StringBuilder();
            for (int x = r.X - 1; x <= r.X + r.W; x++)
            {
                var p = new GridPos(x, y, z);
                char h = EdgeChar(map.GetEdge(new Edge(x, y, EdgeAxis.Horizontal, z)), '-');
                top.Append('+').Append(h).Append(h);
                char v = EdgeChar(map.GetEdge(new Edge(x, y, EdgeAxis.Vertical, z)), '|');
                row.Append(v).Append(Cell(w, p, legend));
            }
            sb.AppendLine(top.ToString()); sb.AppendLine(row.ToString());
        }
        return sb.ToString();
    }

    private static char EdgeChar(EdgeKind k, char wall)
    {
        switch (k)
        {
            case EdgeKind.Wall: return wall;
            case EdgeKind.Door: return 'D';
            case EdgeKind.Window: return '=';
            case EdgeKind.Fence: case EdgeKind.Gate: return ':';
            default: return ' ';
        }
    }

    private static string Cell(WorldState w, GridPos p, Dictionary<string, string> legend)
    {
        if (!w.Map.HasTile(p)) return "  ";
        var it = w.GetItemAt(p);
        if (it == null || !PlacementValidator.Occupies(it))
        {
            if (w.Map.IsWater(p)) return "~~";
            return w.Map.GetRoad(p) != RoadType.None ? ",," : " .";
        }
        string code = Code(it.Def.Id);
        legend[code] = it.Def.DisplayName;
        var f = ItemFacing.Of(it);
        char arrow = !f.HasValue ? ' ' : f.Value == Dir4.N ? '^' : f.Value == Dir4.E ? '>' : f.Value == Dir4.S ? 'v' : '<';
        return code.Substring(0, 1) + (f.HasValue ? arrow : code[1]);
    }

    private static readonly Dictionary<string, string> Codes = new Dictionary<string, string>();
    private static readonly HashSet<string> Used = new HashSet<string>();

    /// <summary>İki harfli benzersiz kısaltma (aynı çalıştırmada aynı eşyaya hep aynı kod).</summary>
    private static string Code(string id)
    {
        if (Codes.TryGetValue(id, out var have)) return have;
        var s = id.StartsWith("item.") ? id.Substring(5) : id;
        var letters = s.Replace("_", "");
        var parts = s.Split('_');
        var cands = new List<string>();
        if (parts.Length > 1) cands.Add($"{parts[0][0]}{parts[1][0]}");
        for (int i = 1; i < letters.Length; i++) cands.Add($"{letters[0]}{letters[i]}");
        foreach (var c0 in cands)
        {
            var c = char.ToUpperInvariant(c0[0]) + c0.Substring(1);
            if (Used.Add(c)) { Codes[id] = c; return c; }
        }
        for (char a = 'A'; a <= 'Z'; a++) for (char b = 'a'; b <= 'z'; b++) { var c = $"{a}{b}"; if (Used.Add(c)) { Codes[id] = c; return c; } }
        return "??";
    }
}
