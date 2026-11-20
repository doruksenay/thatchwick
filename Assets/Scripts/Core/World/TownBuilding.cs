namespace Thatchwick.Core.World
{
    /// <summary>
    /// Şehir planındaki bir bina (belge 40): yerel dikdörtgen, kapı, zemin, kapıdan yola patika. İç döşeme ve katlar
    /// layouts.json'daki bina şablonundan (Template; boşsa Id) gelir (belge 44 C).
    /// </summary>
    public sealed class TownBuilding
    {
        public readonly string Id, Name, Kind, Owner;
        public string Template;
        /// <summary>Meslek etiketi (eşyaların "trades" alanı); boşsa Id. Ev için kullanılmaz.</summary>
        public string Trade;
        public readonly int X, Y, W, H, DoorAt;
        public readonly char Door;
        public readonly FloorType Floor;
        public readonly (int x, int y)[] PathTo;

        public TownBuilding(string id, string name, string kind, int x, int y, int w, int h, char door, int doorAt, FloorType floor, string owner, params (int x, int y)[] pathTo)
        {
            Id = id; Name = name; Kind = kind; X = x; Y = y; W = w; H = h; Door = door; DoorAt = doorAt; Floor = floor; Owner = owner; PathTo = pathTo;
            Template = id;
        }

        /// <summary>
        /// Ek yapı (belge 64a, dikdörtgen olmayan bina): ana binanın bir duvarına yaslanan tek katlı oda (kiler, odunluk,
        /// mutfak kanadı, ocak sundurması). Yerel dikdörtgen; ana binayla ortak duvarda tek bir iç kapı (AnnexDoor, ekin
        /// içindeki karo). AnnexTemplate: layouts.json oda şablonu. AW = 0 ise ek yok.
        /// </summary>
        public int AX, AY, AW, AH;
        public (int x, int y) AnnexDoor;
        public string AnnexTemplate;
        public bool HasAnnex => AW > 0 && AH > 0;
        /// <summary>Ekin iç alanı (dünya); ek yoksa boş dikdörtgen.</summary>
        public RectI AnnexRect => HasAnnex ? new RectI(Regions.Town.X + AX, Regions.Town.Y + AY, AW, AH) : new RectI(0, 0, 0, 0);
        /// <summary>Ekin ana binaya açılan karosu (zemin kat, dünya).</summary>
        public GridPos AnnexDoorInside => new GridPos(Regions.Town.X + AnnexDoor.x, Regions.Town.Y + AnnexDoor.y);

        /// <summary>Ek yapı ekler (zincirleme yazım için kendini döndürür).</summary>
        public TownBuilding WithAnnex(int x, int y, int w, int h, int doorX, int doorY, string template)
        {
            AX = x; AY = y; AW = w; AH = h; AnnexDoor = (doorX, doorY); AnnexTemplate = template;
            return this;
        }

        /// <summary>Karo binanın (ana gövde ya da zemin kattaki ek) içinde mi; yalnızca x, y.</summary>
        public bool Covers(GridPos p)
        {
            var q = new GridPos(p.X, p.Y);
            return Rect.Contains(q) || (HasAnnex && p.Z == 0 && AnnexRect.Contains(q));
        }

        /// <summary>Binanın dış hattına (ana gövde ve ek) Chebyshev uzaklığı; içeride 0.</summary>
        public int DistanceTo(GridPos p)
        {
            int d = RectDist(Rect, p);
            return HasAnnex ? System.Math.Min(d, RectDist(AnnexRect, p)) : d;
        }

        private static int RectDist(RectI r, GridPos p)
        {
            int dx = System.Math.Max(0, System.Math.Max(r.X - p.X, p.X - (r.X + r.W - 1)));
            int dy = System.Math.Max(0, System.Math.Max(r.Y - p.Y, p.Y - (r.Y + r.H - 1)));
            return System.Math.Max(dx, dy);
        }

        /// <summary>İç alan (dünya koordinatı).</summary>
        public RectI Rect => new RectI(Regions.Town.X + X, Regions.Town.Y + Y, W, H);
        public int X0 => Regions.Town.X + X;
        public int Y0 => Regions.Town.Y + Y;
        public int X1 => X0 + W - 1;
        public int Y1 => Y0 + H - 1;

        /// <summary>Kapının iç karosu (zemin kat, dünya).</summary>
        public GridPos DoorInside
        {
            get
            {
                switch (char.ToUpperInvariant(Door))
                {
                    case 'N': return new GridPos(Regions.Town.X + DoorAt, Y0);
                    case 'S': return new GridPos(Regions.Town.X + DoorAt, Y1);
                    case 'W': return new GridPos(X0, Regions.Town.Y + DoorAt);
                    default: return new GridPos(X1, Regions.Town.Y + DoorAt);
                }
            }
        }
    }
}
