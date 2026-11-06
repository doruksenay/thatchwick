using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// A* yol bulma. 8 yönlü hareket; çapraz geçişte iki komşu karodan biri
    /// kapalıysa köşe kesilmez. Maliyet: düz 10, çapraz 14 (tamsayı, belirleyici).
    /// </summary>
    public static class Pathfinder
    {
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        private static readonly GridPos[] Directions =
        {
            new GridPos(0, -1), new GridPos(1, 0), new GridPos(0, 1), new GridPos(-1, 0),
            new GridPos(1, -1), new GridPos(1, 1), new GridPos(-1, 1), new GridPos(-1, -1)
        };

        /// <summary>
        /// Başlangıçtan hedefe yol (başlangıç ve hedef dahil). Yol yoksa null.
        /// Başlangıç = hedef ise tek elemanlı yol döner.
        /// </summary>
        public static List<GridPos> FindPath(GridMap map, GridPos start, GridPos goal)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (!map.IsWalkable(start) || !map.IsWalkable(goal)) return null;
            if (start == goal) return new List<GridPos> { start };

            var open = new SortedSet<Node>(NodeComparer.Instance);
            var best = new Dictionary<GridPos, Node>();
            var closed = new HashSet<GridPos>();

            var first = new Node(start, 0, Heuristic(start, goal), null);
            open.Add(first);
            best[start] = first;

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);

                if (current.Pos == goal) return Rebuild(current);
                if (!closed.Add(current.Pos)) continue;

                foreach (var dir in Directions)
                {
                    var next = current.Pos + dir;
                    if (!map.IsWalkable(next) || closed.Contains(next)) continue;

                    bool diagonal = dir.X != 0 && dir.Y != 0;
                    if (diagonal &&
                        (map.IsBlocked(new GridPos(current.Pos.X + dir.X, current.Pos.Y)) ||
                         map.IsBlocked(new GridPos(current.Pos.X, current.Pos.Y + dir.Y))))
                        continue;

                    int g = current.G + (diagonal ? DiagonalCost : StraightCost);
                    if (best.TryGetValue(next, out var known))
                    {
                        if (g >= known.G) continue;
                        open.Remove(known);
                    }

                    var node = new Node(next, g, Heuristic(next, goal), current);
                    best[next] = node;
                    open.Add(node);
                }
            }

            return null;
        }

        // Octile mesafe: 8 yönlü harekette kabul edilebilir (admissible) tahmin.
        private static int Heuristic(GridPos a, GridPos b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            int diag = Math.Min(dx, dy);
            return DiagonalCost * diag + StraightCost * (Math.Max(dx, dy) - diag);
        }

        private static List<GridPos> Rebuild(Node end)
        {
            var path = new List<GridPos>();
            for (var n = end; n != null; n = n.Parent) path.Add(n.Pos);
            path.Reverse();
            return path;
        }

        private sealed class Node
        {
            public readonly GridPos Pos;
            public readonly int G;
            public readonly int H;
            public readonly Node Parent;

            public Node(GridPos pos, int g, int h, Node parent)
            {
                Pos = pos;
                G = g;
                H = h;
                Parent = parent;
            }

            public int F => G + H;
        }

        // F, sonra H, sonra konum: tam sıralama, böylece sonuç her çalışmada aynıdır.
        private sealed class NodeComparer : IComparer<Node>
        {
            public static readonly NodeComparer Instance = new NodeComparer();

            public int Compare(Node a, Node b)
            {
                if (ReferenceEquals(a, b)) return 0;
                int c = a.F.CompareTo(b.F);
                if (c != 0) return c;
                c = a.H.CompareTo(b.H);
                if (c != 0) return c;
                c = a.Pos.Y.CompareTo(b.Pos.Y);
                if (c != 0) return c;
                c = a.Pos.X.CompareTo(b.Pos.X);
                if (c != 0) return c;
                return a.G.CompareTo(b.G);
            }
        }
    }
}
