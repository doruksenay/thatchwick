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
        public bool HasUpper => Upper != null || InnRooms.Count > 0;
        public bool HasCellar => Cellar != null;
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
            foreach (var b in file.buildings ?? new LayoutBuildingDto[0])
            {
                if (string.IsNullOrWhiteSpace(b.id)) continue;
                var t = new BuildingTemplate
                {
                    Id = b.id, Ground = Blank(b.ground), Upper = Blank(b.upper), UpperKind = Blank(b.upperKind) ?? "home",
                    Cellar = Blank(b.cellar), CellarType = Blank(b.cellarType) ?? Placements.Cellar, Garden = Blank(b.garden),
                };
                if (b.innRooms != null) foreach (var x in b.innRooms) if (!string.IsNullOrWhiteSpace(x)) t.InnRooms.Add(x);
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
