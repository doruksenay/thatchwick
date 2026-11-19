using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

/// <summary>
/// Aşama A'nın inceleme kancaları: plan Regions'taki dükkân ve ev alanlarından kurulur; doğrulayıcının henüz
/// yazılmamış kurallarının (yön, etkileşim, yanlış yer, görsel) yerine IsoArt eşlemesine ve kategoriye bakan
/// sezgisel denetimler çalışır. Aşama B-D'de bunların yerini doğrulayıcının kendi kuralları alır.
/// </summary>
internal static class LayoutHooks
{
    private static readonly Dictionary<string, (string x, string y)> Art = new Dictionary<string, (string, string)>();
    private static ContentDatabase _db;

    public static void Load(string root, ContentDatabase db)
    {
        _db = db;
        var src = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Game", "IsoArt.cs"));
        foreach (Match m in Regex.Matches(src, "\\[\"(item\\.[a-z0-9_]+)\"\\] = new ItemArt \\{([^}]*)\\}"))
        {
            var body = m.Groups[2].Value;
            string ax = Regex.Match(body, "AlongX = \"([^\"]+)\"").Groups[1].Value, ay = Regex.Match(body, "AlongY = \"([^\"]+)\"").Groups[1].Value;
            Art[m.Groups[1].Value] = (ax == "" ? null : ax, ay == "" ? null : ay);
        }
    }

    private static readonly (string id, string name)[] ShopNames =
    {
        ("general", "Pazar (Kerem)"), ("smithy", "Demirci"), ("carpenter", "Marangoz"), ("tavern", "Meyhane"), ("mill", "Değirmen ve fırın"),
        ("scribe", "Kâtip"), ("herb", "Otacı"), ("potter", "Çömlekçi"), ("tailor", "Terzi"), ("saddler", "Saraç"), ("fisher", "Balıkçı"),
        ("mason", "Taşçı"), ("animals", "Hayvan pazarı"),
    };

    public static LayoutPlan Plan(WorldState world)
    {
        var plan = new LayoutPlan();
        var areas = Regions.ShopAreas();
        foreach (var (id, name) in ShopNames)
            if (areas.TryGetValue(id, out var r)) plan.Sites.Add(new Site { Id = id, Name = name, Kind = "shop", Building = r, Plot = r, MinLevel = 0, MaxLevel = 0 });
        int n = 1;
        foreach (var (hx, hy) in new[] { (14, 4), (34, 4), (14, 36), (22, 35), (56, 44), (4, 44) })
        {
            var r = new RectI(Regions.Town.X + hx, Regions.Town.Y + hy, 4, 3);
            plan.Sites.Add(new Site { Id = "home:" + n, Name = $"Köylü evi {n}", Kind = "house", Building = r, Plot = r, MinLevel = 0, MaxLevel = 0 });
            n++;
        }
        var farm = new RectI(StarterWorld.HouseX0, StarterWorld.HouseY0, StarterWorld.HouseX1 - StarterWorld.HouseX0 + 1, StarterWorld.HouseY1 - StarterWorld.HouseY0 + 1);
        plan.Sites.Add(new Site { Id = "farmhouse", Name = "Çiftlik evi", Kind = "house", Building = farm, Plot = farm, MinLevel = -1, MaxLevel = 1 });
        var sq = new RectI(Regions.Town.X + 23, Regions.Town.Y + 21, 15, 11);
        plan.Sites.Add(new Site { Id = "square", Name = "Şehir meydanı", Kind = "plaza", Building = new RectI(0, 0, 0, 0), Plot = sq });
        return plan;
    }

    public static List<Violation> Validate(WorldState world, LayoutPlan plan, ContentDatabase db)
    {
        // Bina dikdörtgeni yalnızca iç mekân karolarını kapsar; kat kontrolü için plan.Site.Contains düz (2B) çalışır.
        var list = PlacementValidator.Run(world, null);
        list.AddRange(Heuristics(world, plan));
        return list;
    }

    // ---------- Aşama A sezgisel denetimleri ----------

    private static bool IsSeat(ItemDef d) => d.Category == "seating" && d.Id != "item.stool_wood" && d.Id != "item.milking_stool" && d.Id != "item.floor_cushion";

    /// <summary>Tek görselin baktığı yön (dönüş 0): _sw → W, _se → N; 2x1 "\" eksenli görsel → W, "/" → N.</summary>
    private static Dir4? BaseFacing(ItemDef d)
    {
        if (!Art.TryGetValue(d.Id, out var a)) return null;
        if (d.Width == 1 && d.Height == 1)
        {
            var n = a.x ?? a.y;
            return n.Contains("_sw") ? Dir4.W : Dir4.N;
        }
        return a.y != null ? Dir4.W : Dir4.N;   // eksen görselinin ön yüzü kameraya
    }

    /// <summary>Şu anki çizim (IsoArt.Item) dönüşte hangi yöne bakıyor: tek dönüşte ayna; 2 ve 3 dönüşü 0 ve 1 gibi çizilir.</summary>
    private static Dir4? DrawnFacing(ItemInstance it)
    {
        var b = BaseFacing(it.Def);
        if (b == null) return null;
        return it.Rotation % 2 == 1 ? Facing.Mirror(b.Value) : b.Value;
    }

    private static IEnumerable<Violation> Heuristics(WorldState world, LayoutPlan plan)
    {
        var map = world.Map;
        var occ = new Dictionary<GridPos, ItemInstance>();
        foreach (var it in world.Items) if (PlacementValidator.Occupies(it)) foreach (var t in it.OccupiedTiles()) occ[t] = it;
        foreach (var it in world.Items)
        {
            var d = it.Def;
            // 1) yanlış yön: oturma eşyası masaya/odağa bakmalı, duvara değil
            if (IsSeat(d) && PlacementValidator.Occupies(it))
            {
                var f = DrawnFacing(it) ?? Facing.Rotate(Dir4.N, it.Rotation);
                bool art = DrawnFacing(it) != null;
                bool wall = false, table = false;
                foreach (var t in it.OccupiedTiles())
                {
                    var front = Facing.Step(t, f);
                    if (occ.TryGetValue(front, out var o) && o != it && (o.Def.Category == "surfaces" || o.Def.HeatOutput > 0 || o.Def.Id == "item.singer_stand" || o.Def.Id == "item.bonfire")) table = true;
                    if (EdgeKinds.IsRoomBoundary(map.GetEdge(GridMap.EdgeBetween(t, front)))) wall = true;
                }
                if (wall || !table)
                    yield return V(Rules.Facing, it, wall ? $"{d.DisplayName} duvara bakıyor ({Facing.ScreenName(f)})." : $"{d.DisplayName} bir masaya ya da odağa bakmıyor ({Facing.ScreenName(f)}{(art ? "" : ", görsel yok")}).");
                if (it.Rotation >= 2 && art)
                    yield return V(Rules.NoView, it, $"{d.DisplayName} dönüş {it.Rotation}: arkası dönük görsel yok, ön görsel çiziliyor.");
            }
            // 2) duvar eşyası kameraya arkası dönük yüzde (kenar 0 ve 3): kesit/yukarı kipinde görünmez
            if (it.IsWallMounted && (it.Rotation == 0 || it.Rotation == 3))
                yield return V(Rules.WrongRoom, it, $"{d.DisplayName} ön duvarın iç yüzüne asılı (kenar {it.Rotation}); kamera bu yüzü görmez.");
            // 3) etkileşim noktası: etkileşimi olan yer eşyasının dört yanı da kapalıysa
            if (PlacementValidator.Occupies(it) && d.Interactions.Count > 0 && !d.IsPlant && !d.IsForage)
            {
                bool any = false;
                foreach (var t in it.OccupiedTiles())
                    foreach (var dir in Facing.All)
                    {
                        var n = Facing.Step(t, dir);
                        if (occ.TryGetValue(n, out var o) && o == it) continue;
                        if (!map.HasTile(n) || map.IsBlocked(n) || map.IsWater(n) || occ.ContainsKey(n)) continue;
                        if (EdgeKinds.BlocksMovement(map.GetEdge(GridMap.EdgeBetween(t, n)))) continue;
                        any = true;
                    }
                if (!any) yield return V(Rules.Interaction, it, $"{d.DisplayName} dört yandan kapalı; kullanılamaz.");
            }
            // 4) iç/dış: yalnızca dış mekân eşyası kapalı odada, iç mekân eşyası dışarıda
            if (PlacementValidator.Occupies(it) && !d.IsPlant && !d.IsForage)
            {
                var room = map.Rooms.RoomAt(it.Position);
                bool indoor = room != null && room.IsIndoor;
                bool outOnly = d.Rooms.Count > 0 && d.Rooms.All(r => r == "outdoor");
                bool inOnly = d.Rooms.Count > 0 && !d.Rooms.Contains("outdoor") && !d.Rooms.Contains("barn");
                if (indoor && outOnly) yield return V(Rules.Site, it, $"{d.DisplayName} dış mekân eşyası, kapalı odada.");
                if (!indoor && inOnly) yield return V(Rules.Site, it, $"{d.DisplayName} iç mekân eşyası, dışarıda.");
            }
            // 5) görsel ve ayak izi uyuşmazlığı: görsel adındaki ölçü (2x1, 3x1 ...) tanımla uyuşmuyor
            if (Art.TryGetValue(d.Id, out var a) && (d.Placement == ItemPlacement.Floor || d.Placement == ItemPlacement.Rug))
            {
                var name = a.x ?? a.y;
                var m = Regex.Match(name, "_(\\d)x(\\d)");
                int w = m.Success ? int.Parse(m.Groups[1].Value) : 1, h = m.Success ? int.Parse(m.Groups[2].Value) : 1;
                if (w * h != d.Width * d.Height || Math.Max(w, h) != Math.Max(d.Width, d.Height))
                    yield return V("gorsel-iz", it, $"{d.DisplayName}: görsel {w}x{h}, ayak izi {d.Width}x{d.Height}.");
            }
            else if (d.Id == "item.drying_rack" && PlacementValidator.Occupies(it))
                yield return V("gorsel-iz", it, "Kurutma askısı: tanım 2x1, üretilen görsel 1x1 (IsoArt'ta bilinçli eşlenmedi).");
        }
    }

    /// <summary>Görseli olmayan (kutu olarak çizilen) eşyaların sayısı, yere göre.</summary>
    public static Dictionary<string, (int total, int boxes)> ArtCoverage(WorldState world, LayoutPlan plan, Func<GridPos, string> where)
    {
        var res = new Dictionary<string, (int, int)>();
        foreach (var it in world.Items)
        {
            if (it.IsPlant || it.Def.IsForage) continue;
            var w = where(it.Position);
            res.TryGetValue(w, out var c);
            res[w] = (c.Item1 + 1, c.Item2 + (Art.ContainsKey(it.Def.Id) ? 0 : 1));
        }
        return res;
    }

    private static Violation V(string code, ItemInstance it, string reason) =>
        new Violation { Code = code, At = it.Position, ItemId = it.Id, ItemDefId = it.Def.Id, Reason = reason, Severity = Severity.Error };

    public static string RootCause(Violation v, WorldState world, LayoutPlan plan)
    {
        var it = v.ItemId != 0 ? world.GetItem(v.ItemId) : null;
        bool farm = Regions.At(v.At) == Regions.Farm;
        string src = farm ? "GameRoot.PlaceDemoItems elle koordinat" : "Regions.cs elle koordinat";
        switch (v.Code)
        {
            case Rules.Facing: return $"{src}; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok";
            case Rules.NoView: return "IsoArt dönüş 2-3'ü 0-1 gibi çiziyor (arkası dönük görsel yok)";
            case Rules.DoorFront: return it != null && it.IsPlant ? "ağaç dağıtımında kapı önü yasağı yok" : $"{src}; kapı önü kontrolü yok";
            case Rules.Interaction: return $"{src}; etkileşim hücresi tanımı yok";
            case Rules.WrongRoom: return "Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü)";
            case Rules.Site: return $"{src}; eşyada iç/dış alanı yok";
            case Rules.Overlap: case Rules.Bounds: return src;
            case "gorsel-iz": return "görsel üretimi tanımla uyuşmuyor; ItemDef'te görsel boyutu ayrı tutulmuyor";
            default: return src;
        }
    }
}
