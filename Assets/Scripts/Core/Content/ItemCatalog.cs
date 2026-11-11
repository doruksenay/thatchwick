using System.Collections.Generic;
using System.Text;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Satın alma menüsü (Sims tarzı) için kategori, oda ve stil seti adları ile eşya kartı özeti.
    /// Sıra menüdeki sıradır. Kimlikler items.json'daki category/rooms/styleSet değerleridir.
    /// </summary>
    public static class ItemCatalog
    {
        public static readonly (string id, string name)[] Categories =
        {
            ("seating", "Oturma"), ("sleep", "Uyku"), ("surfaces", "Masa"), ("heat_light", "Isı ve ışık"),
            ("kitchen", "Mutfak"), ("storage", "Depo"), ("work", "İş"), ("hygiene", "Hijyen"),
            ("decor", "Süs (masa)"), ("decor_wall", "Süs (yer, duvar)"), ("garden", "Bahçe"), ("kids", "Çocuk"), ("structure", "Merdiven ve yapı")
        };

        public static readonly (string id, string name)[] Rooms =
        {
            ("bedroom", "Yatak odası"), ("kitchen", "Mutfak"), ("living", "Oturma"), ("outdoor", "Dış"), ("kids", "Çocuk")
        };

        /// <summary>Stil setleri, kalite sırasıyla.</summary>
        public static readonly string[] StyleSets = { "Köylü", "Zanaatkâr", "Tüccar", "Asil" };

        public static string CategoryName(string id)
        {
            foreach (var c in Categories) if (c.id == id) return c.name;
            return id;
        }

        public static string NeedName(NeedType n)
        {
            switch (n)
            {
                case NeedType.Hunger: return "Açlık";
                case NeedType.Energy: return "Enerji";
                case NeedType.Warmth: return "Isınma";
                case NeedType.Fun: return "Keyif";
                default: return "Hijyen";
            }
        }

        /// <summary>Menüde gösterilen eşyalar: fiyatı olan, kaynak ya da bitki olmayan; kategoriye göre.</summary>
        public static List<ItemDef> InCategory(ContentDatabase content, string category, string room = null, string set = null)
        {
            var list = new List<ItemDef>();
            foreach (var d in content.Items.All)
            {
                if (d.Price <= 0 || d.Placement == ItemPlacement.Resource || d.IsPlant) continue;
                if (d.Category != category) continue;
                if (room != null && !d.Rooms.Contains(room)) continue;
                if (set != null && d.StyleSet != set) continue;
                list.Add(d);
            }
            list.Sort((a, b) => a.UnlockLevel != b.UnlockLevel ? a.UnlockLevel.CompareTo(b.UnlockLevel) : a.Price.CompareTo(b.Price));
            return list;
        }

        /// <summary>Kart özeti: "Enerji +60 · Dinlenmiş · Isı 80 · Depo +40 · Işık · 3 yuva · Duvara asılır".</summary>
        public static string Describe(ItemDef def, MoodCatalog moods = null)
        {
            moods = moods ?? MoodCatalog.Default;
            var parts = new List<string>();
            int crafts = 0;
            foreach (var i in def.Interactions)
            {
                if (i.IsCraft) { crafts++; continue; }
                var s = new StringBuilder();
                if (i.RestoresNeed) s.Append($"{NeedName(i.Need)} +{i.Restore:0}");
                else s.Append(i.DisplayName);
                if (i.Moodlet != null && moods.TryGet(i.Moodlet, out var m)) s.Append($" ({m.DisplayName})");
                parts.Add(s.ToString());
            }
            if (crafts > 0) parts.Add($"{crafts} tarif");
            if (def.HeatOutput > 0f) parts.Add($"Isı {def.HeatOutput:0}");
            if (def.Storage > 0) parts.Add($"Depo +{def.Storage}");
            if (def.HasTag("light")) parts.Add("Işık");
            if (def.SurfaceSlots.Count > 0) parts.Add($"{def.SurfaceSlots.Count} yuva");
            if (def.IsHousing) parts.Add($"Barınak ({def.HousingCapacity})");
            if (def.Placement == ItemPlacement.Wall) parts.Add("Duvara asılır");
            if (def.Placement == ItemPlacement.Surface) parts.Add("Masa/raf üstüne");
            return string.Join(" · ", parts);
        }

        public static string Describe(AnimalSpeciesDef a, ContentDatabase content)
        {
            string house = a.Housing;
            foreach (var d in content.Items.All) if (d.HousingKind == a.Housing) { house = d.DisplayName; break; }
            string prod = a.Produces != null && content.Items.TryGet(a.Produces, out var p)
                ? $"{p.DisplayName} / {a.ProduceEveryDays:0.#} gün" : (a.IsPet ? "Evcil: keyif" : "");
            return $"{house} · yem {a.FeedPerDay:0.#}/gün · {prod}";
        }
    }
}
