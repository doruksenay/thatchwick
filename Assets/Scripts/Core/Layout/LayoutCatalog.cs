using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Layout
{
    // ---------------- layouts.json (Unity JsonUtility ve System.Text.Json ile okunur: yalnızca alan, dizi, iç içe sınıf) ----------------

    [Serializable]
    public sealed class LayoutFileDto
    {
        public LayoutRoomDto[] rooms;          // oda şablonları (yatak odası, mutfak, dükkân zeminleri ...)
        public LayoutBuildingDto[] buildings;  // bina şablonları: hangi katta hangi oda (Ek tablosu)
        public LayoutGardenDto[] gardens;      // bahçe ve avlu şablonları (Aşama D)
        public LayoutKitDto[] kits;            // bina kimlik kartları: cephe, iç malzeme, renk (belge 64 §5)
    }

    /// <summary>
    /// Bina kimlik kartı (belge 64 §5): cephe (duvar malzemesi, kapı, pencere, çatı), iç (zemin, üst kat zemini, duvar
    /// kaplaması) ve eşya kaplamaları (tek ahşap tonu, tek kumaş rengi, metal, kil). Değerler yapı parçası ve eşya kaplaması
    /// kimlikleridir (items.json buildParts); boş alan varsayılanı bırakır.
    /// </summary>
    [Serializable]
    public sealed class LayoutKitDto
    {
        public string id;
        public string wall, door, window, roof, floor, upperFloor, finish;
        public string wood, cloth, metal, clay, stone;
        public string accent;      // vurgu rengi (kapı ve pencere kepengi rengiyle uyumlu; belgede ve denetimde)
    }

    /// <summary>Oda şablonu: sırayla uygulanan adımlar (ana eşya → ilişkili eşya → süs). Koordinat yok.</summary>
    [Serializable]
    public sealed class LayoutRoomDto
    {
        public string id;
        public string type;        // oda türü (Placements.RoomTypes); tek odalı evde "living|bedroom|kitchen"
        public LayoutStepDto[] steps;
    }

    [Serializable]
    public sealed class LayoutStepDto
    {
        public string item;        // tek eşya
        public string[] any;       // seçeneklerden biri (sabit tohumla)
        public int count;          // 0 = 1
        public bool optional;      // konamazsa şablon yine geçer
        public float chance;       // 0 = 1: isteğe bağlı eşyanın çıkma olasılığı
        public string anchor;      // wall, backWall, corner, center, any, nearDoor, far, counter
        public string with;        // seatsAround, bedside, nearHearth, nearItem, onSurface, wall, ceiling, rug
        public string on;          // seatsAround/onSurface/nearItem hedefi: eşya kimliği ya da "surfaces", "shelves"
    }

    [Serializable]
    public sealed class LayoutBuildingDto
    {
        public string id;          // meslek (Economy.Shops kimliği) ya da "cottage", "farmhouse"
        public string ground;      // zemin kat oda şablonu
        public string upper;       // üst kat (boş = tek kat)
        public string upperKind;   // home (tek oda) ya da inn (koridor + odalar)
        public string[] innRooms;  // inn: koridorun iki yanındaki odaların şablonları
        public string cellar;      // bodrum (boş = yok)
        public string cellarType;  // bodrumun oda türü (cellar, pantry, storehouse)
        public string garden;      // bahçe şablonu (Aşama D)
        public string[] kits;      // kimlik kartı (tek ya da seçenekler: köylü evleri tohum ve komşuya göre seçer)
    }

    [Serializable]
    public sealed class LayoutGardenDto
    {
        public string id;
        public string enclosure;   // fence, wall, none
        public LayoutStepDto[] steps;
    }

    // ---------------- çözümlenmiş şablonlar ----------------

    public sealed class LayoutStep
    {
        public readonly List<string> Items = new List<string>();
        public int Count = 1;
        public bool Optional;
        public float Chance = 1f;
        public string Anchor = Anchors.Any;
        public string With;
        public string On;
    }

    public static class Anchors
    {
        public const string Wall = "wall", BackWall = "backWall", Corner = "corner", Center = "center", Any = "any",
            NearDoor = "nearDoor", Far = "far", Counter = "counter";
        // Dış mekân (bahçe/avlu) çapaları: kapı yanı, binanın arkası, yanı, duvar dibi, köşe, su kıyısı, açık alan, ön, ağıl içi, yol kesişimi.
        public const string Back = "back", Side = "side", WallBase = "wallBase", Water = "water", OpenArea = "open", Front = "front", Pen = "pen", Junction = "junction";
        public static readonly string[] All = { Wall, BackWall, Corner, Center, Any, NearDoor, Far, Counter, Back, Side, WallBase, Water, OpenArea, Front, Pen, Junction };
        public const string SeatsAround = "seatsAround", Bedside = "bedside", NearHearth = "nearHearth", NearItem = "nearItem",
            OnSurface = "onSurface", OnWall = "wall", Ceiling = "ceiling", Rug = "rug", Bed = "bed";
        public static readonly string[] Withs = { SeatsAround, Bedside, NearHearth, NearItem, OnSurface, OnWall, Ceiling, Rug, Bed };
    }

    public sealed class RoomTemplate
    {
        public string Id;
        public string Type;
        public readonly List<LayoutStep> Steps = new List<LayoutStep>();
    }

    public sealed class BuildingTemplate
    {
        public string Id, Ground, Upper, UpperKind, Cellar, CellarType, Garden;
        public readonly List<string> InnRooms = new List<string>();
        public readonly List<string> Kits = new List<string>();
        public bool HasUpper => Upper != null || InnRooms.Count > 0;
        public bool HasCellar => Cellar != null;
    }

    public sealed class BuildingKit
    {
        public string Id, Wall, Door, Window, Roof, Floor, UpperFloor, Finish, Wood, Cloth, Metal, Clay, Stone, Accent;

        /// <summary>Eşya kaplama grubu (wood, cloth, metal, clay, stone) için kitin kaplaması; yoksa null.</summary>
        public string FinishFor(string group) => group == "wood" ? Wood : group == "cloth" ? Cloth : group == "metal" ? Metal : group == "clay" ? Clay : group == "stone" ? Stone : null;
    }

    public sealed class GardenTemplate
    {
        public string Id;
        public string Enclosure;
        public readonly List<LayoutStep> Steps = new List<LayoutStep>();
    }

    /// <summary>layouts.json'un çözümlenmiş hali. Map hataları döndürür (bilinmeyen eşya, çapa, şablon).</summary>
    public sealed class LayoutCatalog
    {
        public readonly Dictionary<string, RoomTemplate> Rooms = new Dictionary<string, RoomTemplate>(StringComparer.Ordinal);
        public readonly Dictionary<string, BuildingTemplate> Buildings = new Dictionary<string, BuildingTemplate>(StringComparer.Ordinal);
        public readonly Dictionary<string, GardenTemplate> Gardens = new Dictionary<string, GardenTemplate>(StringComparer.Ordinal);
        public readonly Dictionary<string, BuildingKit> Kits = new Dictionary<string, BuildingKit>(StringComparer.Ordinal);
        public BuildingKit Kit(string id) => id != null && Kits.TryGetValue(id, out var k) ? k : null;

        public RoomTemplate Room(string id) => id != null && Rooms.TryGetValue(id, out var r) ? r : null;
        public BuildingTemplate Building(string id) => id != null && Buildings.TryGetValue(id, out var b) ? b : null;
        public GardenTemplate Garden(string id) => id != null && Gardens.TryGetValue(id, out var g) ? g : null;

        public static LayoutCatalog Map(LayoutFileDto file, ContentDatabase content, List<string> errors)
        {
            var cat = new LayoutCatalog();
            if (file == null) return cat;
            foreach (var r in file.rooms ?? new LayoutRoomDto[0])
            {
                if (string.IsNullOrWhiteSpace(r.id)) { errors.Add("layouts: kimliksiz oda şablonu."); continue; }
                var t = new RoomTemplate { Id = r.id, Type = r.type };
                foreach (var type in (r.type ?? "").Split('|'))
                    if (Array.IndexOf(Placements.RoomTypes, type) < 0 && type != "shop") errors.Add($"layouts {r.id}: bilinmeyen oda türü '{type}'.");
                foreach (var s in r.steps ?? new LayoutStepDto[0]) { var st = Step(s, $"oda {r.id}", content, errors); if (st != null) t.Steps.Add(st); }
                cat.Rooms[r.id] = t;
            }
            foreach (var g in file.gardens ?? new LayoutGardenDto[0])
            {
                if (string.IsNullOrWhiteSpace(g.id)) continue;
                var t = new GardenTemplate { Id = g.id, Enclosure = string.IsNullOrWhiteSpace(g.enclosure) ? "none" : g.enclosure };
                foreach (var s in g.steps ?? new LayoutStepDto[0]) { var st = Step(s, $"bahçe {g.id}", content, errors); if (st != null) t.Steps.Add(st); }
                cat.Gardens[g.id] = t;
            }
            foreach (var k in file.kits ?? new LayoutKitDto[0])
            {
                if (string.IsNullOrWhiteSpace(k.id)) { errors.Add("layouts: kimliksiz kit."); continue; }
                var kit = new BuildingKit
                {
                    Id = k.id, Wall = Blank(k.wall), Door = Blank(k.door), Window = Blank(k.window), Roof = Blank(k.roof), Floor = Blank(k.floor),
                    UpperFloor = Blank(k.upperFloor), Finish = Blank(k.finish), Wood = Blank(k.wood), Cloth = Blank(k.cloth), Metal = Blank(k.metal),
                    Clay = Blank(k.clay), Stone = Blank(k.stone), Accent = Blank(k.accent),
                };
                if (content != null)
                {
                    void Part(string id, BuildPartKind kind)
                    {
                        if (id == null) return;
                        if (!content.Parts.TryGet(id, out var part) || part.Kind != kind) errors.Add($"layouts kit {k.id}: {kind} parçası yok '{id}'.");
                    }
                    Part(kit.Wall, BuildPartKind.WallMaterial); Part(kit.Door, BuildPartKind.Door); Part(kit.Window, BuildPartKind.Window);
                    Part(kit.Roof, BuildPartKind.Roof); Part(kit.Floor, BuildPartKind.Floor); Part(kit.UpperFloor, BuildPartKind.Floor);
                    Part(kit.Finish, BuildPartKind.WallFinish);
                    foreach (var f in new[] { kit.Wood, kit.Cloth, kit.Metal, kit.Clay, kit.Stone }) Part(f, BuildPartKind.ItemFinish);
                }
                cat.Kits[k.id] = kit;
            }
            foreach (var b in file.buildings ?? new LayoutBuildingDto[0])
            {
                if (string.IsNullOrWhiteSpace(b.id)) continue;
                var t = new BuildingTemplate
                {
                    Id = b.id, Ground = Blank(b.ground), Upper = Blank(b.upper), UpperKind = Blank(b.upperKind) ?? "home",
                    Cellar = Blank(b.cellar), CellarType = Blank(b.cellarType) ?? Placements.Cellar, Garden = Blank(b.garden),
                };
                if (b.innRooms != null) foreach (var x in b.innRooms) if (!string.IsNullOrWhiteSpace(x)) t.InnRooms.Add(x);
                if (b.kits != null) foreach (var x in b.kits) if (!string.IsNullOrWhiteSpace(x)) t.Kits.Add(x.Trim());
                foreach (var id in t.Kits) if (!cat.Kits.ContainsKey(id)) errors.Add($"layouts bina {b.id}: kit yok '{id}'.");
                foreach (var id in new[] { t.Ground, t.Upper, t.Cellar })
                    if (id != null && !cat.Rooms.ContainsKey(id)) errors.Add($"layouts bina {b.id}: oda şablonu yok '{id}'.");
                foreach (var id in t.InnRooms) if (!cat.Rooms.ContainsKey(id)) errors.Add($"layouts bina {b.id}: oda şablonu yok '{id}'.");
                if (t.Garden != null && !cat.Gardens.ContainsKey(t.Garden)) errors.Add($"layouts bina {b.id}: bahçe şablonu yok '{t.Garden}'.");
                cat.Buildings[b.id] = t;
            }
            return cat;
        }

        private static string Blank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static LayoutStep Step(LayoutStepDto s, string where, ContentDatabase content, List<string> errors)
        {
            var st = new LayoutStep
            {
                Count = s.count <= 0 ? 1 : s.count, Optional = s.optional, Chance = s.chance <= 0f ? 1f : Math.Min(1f, s.chance),
                Anchor = Blank(s.anchor) ?? Anchors.Any, With = Blank(s.with), On = Blank(s.on),
            };
            if (!string.IsNullOrWhiteSpace(s.item)) st.Items.Add(s.item.Trim());
            if (s.any != null) foreach (var a in s.any) if (!string.IsNullOrWhiteSpace(a)) st.Items.Add(a.Trim());
            if (st.Items.Count == 0) { errors.Add($"layouts {where}: eşyasız adım."); return null; }
            foreach (var id in st.Items)
                if (content != null && !content.Items.Contains(id)) errors.Add($"layouts {where}: eşya yok '{id}'.");
            if (Array.IndexOf(Anchors.All, st.Anchor) < 0) errors.Add($"layouts {where}: bilinmeyen çapa '{st.Anchor}'.");
            if (st.With != null && Array.IndexOf(Anchors.Withs, st.With) < 0) errors.Add($"layouts {where}: bilinmeyen ilişki '{st.With}'.");
            return st;
        }
    }
}
