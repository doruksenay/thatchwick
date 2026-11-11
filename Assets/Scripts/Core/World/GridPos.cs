using System;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Karo ızgarasında bir konum (x sağa, y yukarı/aşağı, 0 tabanlı). Z kat numarasıdır:
    /// 0 zemin kat, 1 üst kat, -1 bodrum. Z verilmezse zemin kattır.
    /// </summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public GridPos(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Aynı x,y; başka bir katta.</summary>
        public GridPos OnLevel(int z) => new GridPos(X, Y, z);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => unchecked(((X * 397) ^ Y) * 31 + Z);
        public override string ToString() => Z == 0 ? $"({X},{Y})" : $"({X},{Y},kat {Z})";

        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);
        /// <summary>Toplam: ofset (Z dahil) eklenir; ofsetin Z'si 0 ise sol tarafın katı korunur.</summary>
        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
}
