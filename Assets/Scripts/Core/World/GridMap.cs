using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Karo kenarı türü. Duvar ve pencere geçişi kapatır, kapı açık bırakır.</summary>
    public enum EdgeKind { None = 0, Wall = 1, Door = 2, Window = 3 }

    /// <summary>Zemin kaplaması (iç mekan göstergesi: Grass dışındakiler).</summary>
    public enum FloorType { Grass = 0, Dirt = 1, Wood = 2, Stone = 3 }

    /// <summary>Karo bölgesi (oyuncunun çizdiği alan). Şimdilik yalnızca depo.</summary>
    public enum ZoneType { None = 0, Storage = 1 }

    public enum EdgeAxis
    {
        /// <summary>X yönünde uzanan kenar: (x,y) karosunun kuzey (y) kenarı, (x,y-1) ile (x,y) arasında.</summary>
        Horizontal = 0,
        /// <summary>Y yönünde uzanan kenar: (x,y) karosunun batı (x) kenarı, (x-1,y) ile (x,y) arasında.</summary>
        Vertical = 1
    }

    /// <summary>İki karo arasındaki kenar. Horizontal x:0..W-1,y:0..H; Vertical x:0..W,y:0..H-1.</summary>
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly int X;
        public readonly int Y;
        public readonly EdgeAxis Axis;
        /// <summary>Kat (0 zemin, 1 üst kat, -1 bodrum).</summary>
        public readonly int Z;

        public Edge(int x, int y, EdgeAxis axis, int z = 0) { X = x; Y = y; Axis = axis; Z = z; }

        /// <summary>Kenarın iki yanındaki karolar: A (daha küçük koordinat, önde) ve B (arkada).</summary>
        public GridPos SideA => Axis == EdgeAxis.Horizontal ? new GridPos(X, Y - 1, Z) : new GridPos(X - 1, Y, Z);
        public GridPos SideB => new GridPos(X, Y, Z);

        public bool Equals(Edge o) => X == o.X && Y == o.Y && Axis == o.Axis && Z == o.Z;
        public override bool Equals(object obj) => obj is Edge e && Equals(e);
        public override int GetHashCode() => unchecked(((X * 397 ^ Y) * 31 + (int)Axis) * 17 + Z);
        public override string ToString() => Z == 0 ? $"{Axis}({X},{Y})" : $"{Axis}({X},{Y},kat {Z})";
    }

    /// <summary>
    /// Baştan karo ızgarası olarak kurulan dünya: karo geçilebilirliği, zemin kaplaması ve
    /// karo kenarlarındaki duvar/kapı/pencereler. Oda tespiti Adım 11'de eklenir.
    /// </summary>
    public sealed class GridMap
    {
        /// <summary>En alt kat (bodrum) ve en üst kat. Zemin kat 0'dır.</summary>
        public const int MinLevel = -1, MaxLevel = 1;
        public static int LevelCount => MaxLevel - MinLevel + 1;
        public static bool IsValidLevel(int z) => z >= MinLevel && z <= MaxLevel;

        private readonly bool[] _blocked;
        private readonly FloorType[] _floor;
        private readonly bool[] _exists;
        private readonly bool[] _owned;
        private readonly ZoneType[] _zone;
        private RoomMap _rooms;
        private readonly Dictionary<Edge, EdgeKind> _edges = new Dictionary<Edge, EdgeKind>();
        private readonly Dictionary<GridPos, List<GridPos>> _links = new Dictionary<GridPos, List<GridPos>>();

        /// <summary>Zemin, kenar ya da kat karosu değişti (görünüm yenilemesi için).</summary>
        public event Action Changed;

        public int Width { get; }
        public int Height { get; }

        public GridMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            int n = width * height * LevelCount;
            _blocked = new bool[n];
            _floor = new FloorType[n];
            _exists = new bool[n];
            _zone = new ZoneType[n];
            _owned = new bool[width * height];
            for (int i = 0; i < _owned.Length; i++) _owned[i] = true; // varsayılan: her yer hanenin
            // Zemin katın her karosu vardır; üst kat ve bodrum karoları yapılınca/kazılınca oluşur.
            for (int i = 0; i < width * height; i++) _exists[(0 - MinLevel) * width * height + i] = true;
        }

        public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height && IsValidLevel(p.Z);

        private bool PlaneInBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        /// <summary>Karo var mı: zemin katta hep; üst katta döşeme yapıldıysa, bodrumda kazıldıysa.</summary>
        public bool HasTile(GridPos p) => InBounds(p) && _exists[Index(p)];

        public bool IsBlocked(GridPos p) => !HasTile(p) || _blocked[Index(p)];

        public bool IsWalkable(GridPos p) => !IsBlocked(p);

        public void SetBlocked(GridPos p, bool blocked)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            _blocked[Index(p)] = blocked;
        }

        // ---- katlar: üst kat döşemesi ve bodrum kazısı ----

        /// <summary>Zemin kat dışındaki bir karoyu var eder ya da kaldırır (üst kat döşemesi, bodrum kazısı).</summary>
        public void SetTile(GridPos p, bool exists)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            if (p.Z == 0) throw new InvalidOperationException("Zemin kat karosu kaldırılamaz.");
            int i = Index(p);
            if (_exists[i] == exists) return;
            _exists[i] = exists;
            if (!exists) { _blocked[i] = false; _floor[i] = FloorType.Grass; _zone[i] = ZoneType.None; }
            _rooms = null;
            Changed?.Invoke();
        }

        /// <summary>Verilen kattaki var olan karolar.</summary>
        public IEnumerable<GridPos> TilesOnLevel(int z)
        {
            if (!IsValidLevel(z)) yield break;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var p = new GridPos(x, y, z);
                    if (_exists[Index(p)]) yield return p;
                }
        }

        public int CountTiles(int z)
        {
            int n = 0;
            foreach (var _ in TilesOnLevel(z)) n++;
            return n;
        }

        // ---- katlar arası bağlantı (merdiven) ----

        /// <summary>İki karo arasına çift yönlü merdiven bağlantısı ekler (farklı katlarda olmalı).</summary>
        public void AddLink(GridPos a, GridPos b)
        {
            if (a.Z == b.Z) throw new ArgumentException("Bağlantı farklı katlar arasında olmalı.");
            AddOneWay(a, b);
            AddOneWay(b, a);
        }

        private void AddOneWay(GridPos a, GridPos b)
        {
            if (!_links.TryGetValue(a, out var list)) _links[a] = list = new List<GridPos>();
            if (!list.Contains(b)) list.Add(b);
        }

        public void RemoveLink(GridPos a, GridPos b)
        {
            if (_links.TryGetValue(a, out var la)) { la.Remove(b); if (la.Count == 0) _links.Remove(a); }
            if (_links.TryGetValue(b, out var lb)) { lb.Remove(a); if (lb.Count == 0) _links.Remove(b); }
        }

        /// <summary>p'den merdivenle gidilebilen (iki ucu da açık) karolar.</summary>
        public IEnumerable<GridPos> LinksFrom(GridPos p)
        {
            if (!_links.TryGetValue(p, out var list) || !IsWalkable(p)) yield break;
            foreach (var q in list) if (IsWalkable(q)) yield return q;
        }

        public bool HasLinkAt(GridPos p) => _links.ContainsKey(p);

        /// <summary>Tek adımda geçilebilir mi: aynı katta CanStep, farklı katta bir merdiven bağlantısı.</summary>
        public bool CanTraverse(GridPos from, GridPos to)
        {
            if (from.Z == to.Z) return CanStep(from, to);
            if (!_links.TryGetValue(from, out var list) || !list.Contains(to)) return false;
            return IsWalkable(from) && IsWalkable(to);
        }

        /// <summary>Bir adımın uzunluğu (karo cinsinden): normal adım 1, merdiven bağlantısı yatay mesafe + 1.</summary>
        public static int StepSpan(GridPos from, GridPos to) =>
            from.Z == to.Z ? 1 : Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y) + 1;

        public void ClearLinks() { _links.Clear(); }

        /// <summary>İki (komşu ya da değil) karo arasında, a'dan b'ye doğru ilk adımda duvar yoksa true. Komşu değillerse true.</summary>
        public bool EdgeFree(GridPos a, GridPos b)
        {
            if (a.Z != b.Z) return true;
            int dx = Math.Abs(a.X - b.X), dy = Math.Abs(a.Y - b.Y);
            if (dx + dy != 1) return true;
            return !EdgeBlocks(EdgeBetween(a, b));
        }

        // ---- zemin ----

        public FloorType GetFloor(GridPos p) => InBounds(p) ? _floor[Index(p)] : FloorType.Grass;

        public void SetFloor(GridPos p, FloorType floor)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            if (_floor[Index(p)] == floor) return;
            _floor[Index(p)] = floor;
            Changed?.Invoke();
        }

        // ---- bölgeler ----

        public ZoneType GetZone(GridPos p) => InBounds(p) ? _zone[Index(p)] : ZoneType.None;

        public void SetZone(GridPos p, ZoneType zone)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            if (_zone[Index(p)] == zone) return;
            _zone[Index(p)] = zone;
            Changed?.Invoke();
        }

        public int CountZone(ZoneType zone)
        {
            int n = 0;
            foreach (var z in _zone) if (z == zone) n++;
            return n;
        }

        // ---- arsa (sahiplik): kata bakmaz, x,y'ye göre ----

        public bool IsOwned(GridPos p) => PlaneInBounds(p) && _owned[p.Y * Width + p.X];

        public void SetOwned(GridPos p, bool owned)
        {
            if (!PlaneInBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            int i = p.Y * Width + p.X;
            if (_owned[i] == owned) return;
            _owned[i] = owned;
            Changed?.Invoke();
        }

        /// <summary>Tüm haritayı sahipsiz yapıp verilen dikdörtgeni (x0..x1, y0..y1 dahil) hanenin yapar.</summary>
        public void RestrictOwnership(int x0, int y0, int x1, int y1)
        {
            for (int i = 0; i < _owned.Length; i++) _owned[i] = false;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if (PlaneInBounds(new GridPos(x, y))) _owned[y * Width + x] = true;
            Changed?.Invoke();
        }

        /// <summary>Oda haritası (duvar kenarlarına göre); kenar değişince yeniden hesaplanır.</summary>
        public RoomMap Rooms => _rooms ?? (_rooms = RoomMap.Compute(this));

        // ---- kenarlar ----

        public bool EdgeInBounds(Edge e) => IsValidLevel(e.Z) && (e.Axis == EdgeAxis.Horizontal
            ? e.X >= 0 && e.X < Width && e.Y >= 0 && e.Y <= Height
            : e.X >= 0 && e.X <= Width && e.Y >= 0 && e.Y < Height);

        public EdgeKind GetEdge(Edge e) => _edges.TryGetValue(e, out var k) ? k : EdgeKind.None;

        public IEnumerable<KeyValuePair<Edge, EdgeKind>> Edges => _edges;

        public void SetEdge(Edge e, EdgeKind kind)
        {
            if (!EdgeInBounds(e)) throw new ArgumentOutOfRangeException(nameof(e), $"{e} ızgara dışında.");
            if (GetEdge(e) == kind) return;
            if (kind == EdgeKind.None) _edges.Remove(e); else _edges[e] = kind;
            _rooms = null;
            Changed?.Invoke();
        }

        /// <summary>İki komşu (dik) karo arasındaki kenar.</summary>
        public static Edge EdgeBetween(GridPos a, GridPos b)
        {
            if (a.Z != b.Z) throw new ArgumentException("Karolar aynı katta değil.");
            int z = a.Z;
            if (b.X == a.X + 1 && b.Y == a.Y) return new Edge(b.X, b.Y, EdgeAxis.Vertical, z);
            if (a.X == b.X + 1 && a.Y == b.Y) return new Edge(a.X, a.Y, EdgeAxis.Vertical, z);
            if (b.Y == a.Y + 1 && b.X == a.X) return new Edge(b.X, b.Y, EdgeAxis.Horizontal, z);
            if (a.Y == b.Y + 1 && a.X == b.X) return new Edge(a.X, a.Y, EdgeAxis.Horizontal, z);
            throw new ArgumentException("Karolar dik komşu değil.");
        }

        /// <summary>Kenar geçişi kapatıyor mu (duvar ya da pencere).</summary>
        public bool EdgeBlocks(Edge e)
        {
            var k = GetEdge(e);
            return k == EdgeKind.Wall || k == EdgeKind.Window;
        }

        /// <summary>
        /// from'dan komşu karo to'ya (dik ya da çapraz) tek adımda geçilebilir mi: hedef karo açık,
        /// aradaki kenarlar duvarsız ve çaprazda köşe kesilmiyor.
        /// </summary>
        public bool CanStep(GridPos from, GridPos to)
        {
            if (from.Z != to.Z) return false;      // katlar arası geçiş yalnızca merdiven bağlantısıyla (LinksFrom)
            if (!IsWalkable(to)) return false;
            int dx = to.X - from.X, dy = to.Y - from.Y;
            if (dx == 0 && dy == 0) return true;
            if (dx == 0 || dy == 0) return !EdgeBlocks(EdgeBetween(from, to));

            var h = new GridPos(from.X + dx, from.Y, from.Z);
            var v = new GridPos(from.X, from.Y + dy, from.Z);
            if (IsBlocked(h) || IsBlocked(v)) return false;
            return !EdgeBlocks(EdgeBetween(from, h)) && !EdgeBlocks(EdgeBetween(h, to)) &&
                   !EdgeBlocks(EdgeBetween(from, v)) && !EdgeBlocks(EdgeBetween(v, to));
        }

        private int Index(GridPos p) => ((p.Z - MinLevel) * Height + p.Y) * Width + p.X;
    }
}
