using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

/// <summary>
/// Rapor kancaları: plan Regions'taki dükkân ve ev alanlarından kurulur; doğrulayıcının henüz
/// yazılmamış kurallarının (yön, etkileşim, yanlış yer, görsel) yerine IsoArt eşlemesine ve kategoriye bakan
/// sezgisel denetimler çalışır. Aşama B-D'de bunların yerini doğrulayıcının kendi kuralları alır.
/// </summary>
internal static class LayoutHooks
{
    private static ContentDatabase _db;

    public static void Load(string root, ContentDatabase db) { _db = db; }

    private static readonly (string id, string name)[] ShopNames =
    {
        ("general", "Pazar (Kerem)"), ("smithy", "Demirci"), ("carpenter", "Marangoz"), ("tavern", "Meyhane"), ("mill", "Değirmen ve fırın"),
        ("scribe", "Kâtip"), ("herb", "Aktar"), ("potter", "Çömlekçi"), ("tailor", "Terzi"), ("saddler", "Saraç"), ("fisher", "Balıkçı"),
        ("mason", "Taşçı"), ("animals", "Hayvan pazarı"),
    };

    public static LayoutPlan Plan(WorldState world)
    {
        var plan = new LayoutPlan();
        var areas = Regions.ShopAreas();
        foreach (var (id, name) in ShopNames)
            if (areas.TryGetValue(id, out var r))
            {
                var site = new Site { Id = id, Name = name, Kind = "shop", Trade = id, Building = r, Plot = r, MinLevel = 0, MaxLevel = 0 };
                site.Rooms.Add(new RoomZone { Type = id == "animals" ? "outdoor" : "shop", Trade = id, Level = 0, Rect = r });
                plan.Sites.Add(site);
            }
        int n = 1;
        foreach (var (hx, hy) in new[] { (14, 4), (34, 4), (14, 36), (22, 35), (56, 44), (4, 44) })
        {
            var r = new RectI(Regions.Town.X + hx, Regions.Town.Y + hy, 4, 3);
            var home = new Site { Id = "home:" + n, Name = $"Köylü evi {n}", Kind = "house", Building = r, Plot = r, MinLevel = 0, MaxLevel = 0 };
            home.Rooms.Add(new RoomZone { Type = "living|bedroom|kitchen|dining", Level = 0, Rect = r });
            plan.Sites.Add(home);
            n++;
        }
        var farm = new RectI(StarterWorld.HouseX0, StarterWorld.HouseY0, StarterWorld.HouseX1 - StarterWorld.HouseX0 + 1, StarterWorld.HouseY1 - StarterWorld.HouseY0 + 1);
        var fh = new Site { Id = "farmhouse", Name = "Çiftlik evi", Kind = "house", Building = farm, Plot = farm, MinLevel = -1, MaxLevel = 1 };
        fh.Rooms.Add(new RoomZone { Type = "kitchen|living|dining", Level = 0, Rect = farm });
        fh.Rooms.Add(new RoomZone { Type = "bedroom|living", Level = 1, Rect = farm });
        fh.Rooms.Add(new RoomZone { Type = "cellar|pantry|workshop|storehouse", Level = -1, Rect = farm });
        plan.Sites.Add(fh);
        var sq = new RectI(Regions.Town.X + 23, Regions.Town.Y + 21, 15, 11);
        plan.Sites.Add(new Site { Id = "square", Name = "Şehir meydanı", Kind = "plaza", Building = new RectI(0, 0, 0, 0), Plot = sq });
        return plan;
    }

    public static List<Violation> Validate(WorldState world, LayoutPlan plan, ContentDatabase db)
    {
        var v = new PlacementValidator(world, plan);
        LayoutRules.AddAll(v, db);
        return v.Validate();
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
            res[w] = (c.Item1 + 1, c.Item2 + (it.Def.Art != null ? 0 : 1));
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
            case Rules.NoView: return v.Reason.Contains("iç yüz") ? "Painter.Wall yan 0/3: yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü" : "IsoArt dönüş 2-3'ü 0-1 gibi çiziyordu (arkası dönük görsel yok)";
            case Rules.DoorFront: return it != null && it.IsPlant ? "ağaç dağıtımında kapı önü yasağı yok" : $"{src}; kapı önü kontrolü yok";
            case Rules.Interaction: return $"{src}; eşya kuzey/batı (ön) duvara sırtını değil önünü veriyor, etkileşim hücresi tanımı yoktu";
            case Rules.WrongRoom: return $"{src}; eşyada oda türü/meslek etiketi yoktu";
            case Rules.Relation: return $"{src}; ilişki kuralı (duvara sırt, ocak yanı) yoktu";
            case Rules.Site: return $"{src}; eşyada iç/dış alanı yok";
            case Rules.Overlap: case Rules.Bounds: return src;
            case "gorsel-iz": return "görsel üretimi tanımla uyuşmuyor; ItemDef'te görsel boyutu ayrı tutulmuyor";
            default: return src;
        }
    }
}
