using System;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Baştan karo ızgarası olarak kurulan dünya. Şimdilik her karo yalnızca
    /// "geçilebilir mi" bilgisini taşır; zemin tipi, duvar kenarları ve oda
    /// bilgisi sonraki adımlarda (10-11) eklenir.
    /// </summary>
    public sealed class GridMap
    {
        private readonly bool[] _blocked;

        public int Width { get; }
        public int Height { get; }

        public GridMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            _blocked = new bool[width * height];
        }

        public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        public bool IsBlocked(GridPos p) => !InBounds(p) || _blocked[Index(p)];

        public bool IsWalkable(GridPos p) => !IsBlocked(p);

        public void SetBlocked(GridPos p, bool blocked)
        {
            if (!InBounds(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p} ızgara dışında.");
            _blocked[Index(p)] = blocked;
        }

        private int Index(GridPos p) => p.Y * Width + p.X;
    }
}
