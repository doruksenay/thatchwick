using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>Bir binanın tek katındaki oda bölgesi: türü (yatak odası, mutfak, dükkân ...) ve dikdörtgeni.</summary>
    public sealed class RoomZone
    {
        /// <summary>Oda türleri: tek odalı köylü evinde birden çok (yatak, mutfak, oturma). İlki asıl türdür.</summary>
        public readonly List<string> Types = new List<string>();
        public string Type { get => Types.Count > 0 ? Types[0] : null; set { Types.Clear(); if (value != null) Types.AddRange(value.Split('|')); } }
        public int Level;
        public RectI Rect;
        /// <summary>Oda bir dükkân ya da atölyeyse meslek etiketi (demirci, terzi ...).</summary>
        public string Trade;
        public bool Contains(GridPos p) => p.Z == Level && Rect.Contains(p);
    }

    /// <summary>
    /// Bir arsa: bina (katlarıyla) ve bina dışında kalan bahçe/avlu. Kapı yönü ve ön yüz buradan okunur;
    /// dış donatı (tabela, tezgâh, meslek avlusu) buna göre yerleşir.
    /// </summary>
    public sealed class Site
    {
        public string Id;
        public string Name;
        /// <summary>shop, house, farm, inn, pen (hayvan pazarı)</summary>
        public string Kind;
        public string Trade;
        /// <summary>Sahibinin adlandırılmış yeri (home:mill ...); yoksa null.</summary>
        public string Owner;
        /// <summary>Bina iç alanı (zemin katta); Plot bunu ve çevresindeki bahçeyi kapsar.</summary>
        public RectI Building;
        public RectI Plot;
        public Dir4 DoorDir;
        /// <summary>Kapının iç karosu (zemin kat).</summary>
        public GridPos DoorInside;
        public int MinLevel, MaxLevel;
        public readonly List<RoomZone> Rooms = new List<RoomZone>();

        public bool Contains(GridPos p) => Plot.Contains(p);
        public bool InBuilding(GridPos p) => p.Z >= MinLevel && p.Z <= MaxLevel && Building.Contains(p);
        /// <summary>Kapının dış karosu.</summary>
        public GridPos DoorOutside => Facing.Step(DoorInside, DoorDir);

        public RoomZone ZoneAt(GridPos p)
        {
            foreach (var z in Rooms) if (z.Contains(p)) return z;
            return null;
        }
    }

    /// <summary>Yerleşim planı: arsalar ve oda bölgeleri. Doğrulayıcı oda türü, sınır ve dış mekân kurallarında kullanır.</summary>
    public sealed class LayoutPlan
    {
        public readonly List<Site> Sites = new List<Site>();

        public Site SiteAt(GridPos p)
        {
            foreach (var s in Sites) if (s.Contains(p)) return s;
            return null;
        }

        public Site BuildingAt(GridPos p)
        {
            foreach (var s in Sites) if (s.InBuilding(p)) return s;
            return null;
        }

        public RoomZone ZoneAt(GridPos p)
        {
            foreach (var s in Sites) { var z = s.ZoneAt(p); if (z != null) return z; }
            return null;
        }

        public Site Get(string id)
        {
            foreach (var s in Sites) if (s.Id == id) return s;
            return null;
        }
    }
}
