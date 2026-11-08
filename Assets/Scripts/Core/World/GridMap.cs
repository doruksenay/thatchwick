using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Karo kenarı türü. Duvar ve pencere geçişi kapatır, kapı açık bırakır.</summary>
    public enum EdgeKind { None = 0, Wall = 1, Door = 2, Window = 3 }

    /// <summary>Zemin kaplaması (iç mekan göstergesi: Grass dışındakiler).</summary>
    public enum FloorType { Grass = 0, Dirt = 1, Wood = 2, Stone = 3 }

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

        public Edge(int x, int y, EdgeAxis axis) { X = x; Y = y; Axis = axis; }

        /// <summary>Kenarın iki yanındaki karolar: A (daha küçük koordinat, önde) ve B (arkada).</summary>
        public GridPos SideA => Axis == EdgeAxis.Horizontal ? new GridPos(X, Y - 1) : new GridPos(X - 1, Y);
        public GridPos SideB => new GridPos(X, Y);

        public bool Equals(Edge o) => X == o.X && Y == o.Y && Axis == o.Axis;
        public override bool Equals(object obj) => obj is Edge e && Equals(e);
        public override int GetHashCode() => unchecked((X * 397 ^ Y) * 31 + (int)Axis);
        public override string ToString() => $"{Axis}({X},{Y})";
    }

    /// <summary>
    /// Baştan karo ızgarası olarak kurulan dünya: karo geçilebilirliği, zemin kaplaması ve
    /// karo kenarlarındaki duvar/kapı/pencereler. Oda tespiti Adım 11'de eklenir.
    /// </summary>
    public sealed class GridMap
    {
        private readonly bool[] _blocked;
        private readonly FloorType[] _floor;
        private readonly Dictionary<Edge, EdgeKind> _edges = new Dictionary<Edge, EdgeKind>();

        /// <summary>Zemin ya da kenar değişti (görünüm yenilemesi için).</summary>
        public event Action Changed;

        public int Width { get; }
        public int Height { get; }

        public GridMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            _blocked = new bool[width * height];
            _floor = new FloorType[width * height];
        }

        public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        public bool IsBlocked(GridPos p) => !InBounds(p) || _blocked[Index(p)];

        public bool IsWalkable(GridPos p) => !IsBlocked(p);

        public void SetBlocked(GridPos p, bool blocked)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            _blocked[Index(p)] = blocked;
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

        // ---- kenarlar ----

        public bool EdgeInBounds(Edge e) => e.Axis == EdgeAxis.Horizontal
            ? e.X >= 0 && e.X < Width && e.Y >= 0 && e.Y <= Height
            : e.X >= 0 && e.X <= Width && e.Y >= 0 && e.Y < Height;

        public EdgeKind GetEdge(Edge e) => _edges.TryGetValue(e, out var k) ? k : EdgeKind.None;

        public IEnumerable<KeyValuePair<Edge, EdgeKind>> Edges => _edges;

        public void SetEdge(Edge e, EdgeKind kind)
        {
            if (!EdgeInBounds(e)) throw new ArgumentOutOfRangeException(nameof(e), $"{e} ızgara dışında.");
            if (GetEdge(e) == kind) return;
            if (kind == EdgeKind.None) _edges.Remove(e); else _edges[e] = kind;
            Changed?.Invoke();
        }

        /// <summary>İki komşu (dik) karo arasındaki kenar.</summary>
        public static Edge EdgeBetween(GridPos a, GridPos b)
        {
            if (b.X == a.X + 1 && b.Y == a.Y) return new Edge(b.X, b.Y, EdgeAxis.Vertical);
            if (a.X == b.X + 1 && a.Y == b.Y) return new Edge(a.X, a.Y, EdgeAxis.Vertical);
            if (b.Y == a.Y + 1 && b.X == a.X) return new Edge(b.X, b.Y, EdgeAxis.Horizontal);
            if (a.Y == b.Y + 1 && a.X == b.X) return new Edge(a.X, a.Y, EdgeAxis.Horizontal);
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
            if (!IsWalkable(to)) return false;
            int dx = to.X - from.X, dy = to.Y - from.Y;
            if (dx == 0 && dy == 0) return true;
            if (dx == 0 || dy == 0) return !EdgeBlocks(EdgeBetween(from, to));

            var h = new GridPos(from.X + dx, from.Y);
            var v = new GridPos(from.X, from.Y + dy);
            if (IsBlocked(h) || IsBlocked(v)) return false;
            return !EdgeBlocks(EdgeBetween(from, h)) && !EdgeBlocks(EdgeBetween(h, to)) &&
                   !EdgeBlocks(EdgeBetween(from, v)) && !EdgeBlocks(EdgeBetween(v, to));
        }

        private int Index(GridPos p) => p.Y * Width + p.X;
    }
}
