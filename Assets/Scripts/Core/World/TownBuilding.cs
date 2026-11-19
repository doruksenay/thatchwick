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
        public readonly int X, Y, W, H, DoorAt;
        public readonly char Door;
        public readonly FloorType Floor;
        public readonly (int x, int y)[] PathTo;

        public TownBuilding(string id, string name, string kind, int x, int y, int w, int h, char door, int doorAt, FloorType floor, string owner, params (int x, int y)[] pathTo)
        {
            Id = id; Name = name; Kind = kind; X = x; Y = y; W = w; H = h; Door = door; DoorAt = doorAt; Floor = floor; Owner = owner; PathTo = pathTo;
            Template = id;
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
