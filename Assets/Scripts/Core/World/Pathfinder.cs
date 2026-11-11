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
            return Search(map, start, p => p == goal, p => Heuristic(p, goal));
        }

        /// <summary>
        /// Başlangıçtan verilen hedeflerden en ucuz ulaşılanına yol (tek A* araması). Eşyanın
        /// çevresindeki karolardan birine gitmek gibi durumlar için. Hedef yoksa ya da hiçbirine
        /// ulaşılamıyorsa null; başlangıç zaten bir hedefse tek elemanlı yol.
        /// </summary>
        public static List<GridPos> FindPathToAny(GridMap map, GridPos start, ICollection<GridPos> goals)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (goals == null) throw new ArgumentNullException(nameof(goals));
            if (!map.IsWalkable(start)) return null;

            var targets = new List<GridPos>();
            foreach (var g in goals) if (map.IsWalkable(g)) targets.Add(g);
            if (targets.Count == 0) return null;
            if (targets.Contains(start)) return new List<GridPos> { start };

            var set = new HashSet<GridPos>(targets);
            return Search(map, start, set.Contains, p =>
            {
                int best = int.MaxValue;
                foreach (var t in targets) best = Math.Min(best, Heuristic(p, t));
                return best;
            });
        }

        private static List<GridPos> Search(GridMap map, GridPos start, Func<GridPos, bool> isGoal, Func<GridPos, int> heuristic)
        {
            var open = new SortedSet<Node>(NodeComparer.Instance);
            var best = new Dictionary<GridPos, Node>();
            var closed = new HashSet<GridPos>();

            var first = new Node(start, 0, heuristic(start), null);
            open.Add(first);
            best[start] = first;

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);

                if (isGoal(current.Pos)) return Rebuild(current);
                if (!closed.Add(current.Pos)) continue;

                foreach (var dir in Directions)
                {
                    var next = current.Pos + dir;
                    if (closed.Contains(next) || !map.CanStep(current.Pos, next)) continue;

                    bool diagonal = dir.X != 0 && dir.Y != 0;
                    Relax(open, best, current, next, current.G + (diagonal ? DiagonalCost : StraightCost), heuristic);
                }

                // Merdiven bağlantıları: kat değiştirir; maliyet kat farkı + yatay mesafe kadar adım.
                foreach (var next in map.LinksFrom(current.Pos))
                {
                    if (closed.Contains(next)) continue;
                    int span = Math.Abs(next.X - current.Pos.X) + Math.Abs(next.Y - current.Pos.Y) + 1;
                    Relax(open, best, current, next, current.G + StraightCost * span, heuristic);
                }
            }

            return null;
        }

        private static void Relax(SortedSet<Node> open, Dictionary<GridPos, Node> best, Node current, GridPos next, int g, Func<GridPos, int> heuristic)
        {
            if (best.TryGetValue(next, out var known))
            {
                if (g >= known.G) return;
                open.Remove(known);
            }
            var node = new Node(next, g, heuristic(next), current);
            best[next] = node;
            open.Add(node);
        }

        // Octile mesafe (kat farkı yok sayılır, böylece hâlâ kabul edilebilir): 8 yönlü harekette kabul edilebilir (admissible) tahmin.
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
                c = a.Pos.Z.CompareTo(b.Pos.Z);
                if (c != 0) return c;
                return a.G.CompareTo(b.G);
            }
        }
    }
}
