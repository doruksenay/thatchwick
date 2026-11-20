using System;
using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// A* açık kümesi SortedSet'ten ikili yığına geçti (performans incelemesi 20.11.2026): aynı sıralama kuralıyla
    /// yollar birebir aynı kalmalı. Eski uygulama burada başvuru olarak durur.
    /// </summary>
    public class PathfinderHeapTests
    {
        [Fact]
        public void RastgeleHaritalar_EskiUygulamaylaAyniYol()
        {
            var rng = new Random(1234);
            int compared = 0;
            for (int trial = 0; trial < 60; trial++)
            {
                int w = 12 + rng.Next(20), h = 12 + rng.Next(20);
                var map = new GridMap(w, h);
                for (int i = 0; i < w * h / 4; i++) map.SetBlocked(new GridPos(rng.Next(w), rng.Next(h)), true);
                if (trial % 2 == 0)
                    for (int i = 0; i < w * h / 6; i++)
                    {
                        var p = new GridPos(rng.Next(w), rng.Next(h));
                        if (!map.IsBlocked(p)) map.SetRoad(p, rng.Next(2) == 0 ? RoadType.Path : RoadType.Gravel);
                    }
                for (int q = 0; q < 10; q++)
                {
                    var a = new GridPos(rng.Next(w), rng.Next(h));
                    var b = new GridPos(rng.Next(w), rng.Next(h));
                    var expected = Reference.FindPath(map, a, b);
                    var actual = Pathfinder.FindPath(map, a, b);
                    if (expected == null) { Assert.Null(actual); continue; }
                    Assert.NotNull(actual);
                    Assert.Equal(expected, actual);
                    compared++;

                    var goals = new HashSet<GridPos> { b, new GridPos(Math.Max(0, b.X - 1), b.Y), new GridPos(b.X, Math.Max(0, b.Y - 1)) };
                    var e2 = Reference.FindPathToAny(map, a, goals);
                    var a2 = Pathfinder.FindPathToAny(map, a, goals);
                    if (e2 == null) Assert.Null(a2); else Assert.Equal(e2, a2);
                }
            }
            Assert.True(compared > 100);
        }

        [Fact]
        public void ArdArdaAramalar_TamponTemizlenir()
        {
            var map = new GridMap(10, 10);
            for (int y = 0; y < 9; y++) map.SetBlocked(new GridPos(5, y), true);
            var first = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(9, 0));
            var again = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(9, 0));
            Assert.Equal(first, again);
            Assert.Null(Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(5, 0)));   // kapalı hedef
            Assert.Equal(first, Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(9, 0)));
        }

        // ---- eski uygulama (SortedSet, Remove ile güncelleme) ----
        private static class Reference
        {
            private const int StraightCost = 10, DiagonalCost = 14, RoadScale = 10;
            private static readonly GridPos[] Directions =
            {
                new GridPos(0, -1), new GridPos(1, 0), new GridPos(0, 1), new GridPos(-1, 0),
                new GridPos(1, -1), new GridPos(1, 1), new GridPos(-1, 1), new GridPos(-1, -1)
            };

            public static List<GridPos> FindPath(GridMap map, GridPos start, GridPos goal)
            {
                if (!map.HasTile(start) || !map.IsWalkable(goal)) return null;
                if (start == goal) return new List<GridPos> { start };
                return Search(map, start, p => p == goal, p => H(p, goal, map.HasTerrainSpeed));
            }

            public static List<GridPos> FindPathToAny(GridMap map, GridPos start, ICollection<GridPos> goals)
            {
                if (!map.HasTile(start)) return null;
                var targets = new List<GridPos>();
                foreach (var g in goals) if (map.IsWalkable(g)) targets.Add(g);
                if (targets.Count == 0) return null;
                if (targets.Contains(start)) return new List<GridPos> { start };
                var set = new HashSet<GridPos>(targets);
                return Search(map, start, set.Contains, p =>
                {
                    int best = int.MaxValue;
                    foreach (var t in targets) best = Math.Min(best, H(p, t, map.HasTerrainSpeed));
                    return best;
                });
            }

            private static int H(GridPos a, GridPos b, bool roads)
            {
                int dx = Math.Abs(a.X - b.X), dy = Math.Abs(a.Y - b.Y), diag = Math.Min(dx, dy);
                int h = DiagonalCost * diag + StraightCost * (Math.Max(dx, dy) - diag);
                return roads ? (int)(h * RoadScale / GridMap.PavedSpeed) : h;
            }

            private sealed class Node
            {
                public readonly GridPos Pos; public readonly int G, H; public readonly Node Parent;
                public Node(GridPos p, int g, int h, Node parent) { Pos = p; G = g; H = h; Parent = parent; }
                public int F => G + H;
            }

            private sealed class Cmp : IComparer<Node>
            {
                public int Compare(Node a, Node b)
                {
                    if (ReferenceEquals(a, b)) return 0;
                    int c = a.F.CompareTo(b.F); if (c != 0) return c;
                    c = a.H.CompareTo(b.H); if (c != 0) return c;
                    c = a.Pos.Y.CompareTo(b.Pos.Y); if (c != 0) return c;
                    c = a.Pos.X.CompareTo(b.Pos.X); if (c != 0) return c;
                    c = a.Pos.Z.CompareTo(b.Pos.Z); if (c != 0) return c;
                    return a.G.CompareTo(b.G);
                }
            }

            private static List<GridPos> Search(GridMap map, GridPos start, Func<GridPos, bool> isGoal, Func<GridPos, int> heuristic)
            {
                var open = new SortedSet<Node>(new Cmp());
                var best = new Dictionary<GridPos, Node>();
                var closed = new HashSet<GridPos>();
                bool roads = map.HasTerrainSpeed;
                var first = new Node(start, 0, heuristic(start), null);
                open.Add(first); best[start] = first;
                while (open.Count > 0)
                {
                    var cur = open.Min; open.Remove(cur);
                    if (isGoal(cur.Pos)) { var path = new List<GridPos>(); for (var n = cur; n != null; n = n.Parent) path.Add(n.Pos); path.Reverse(); return path; }
                    if (!closed.Add(cur.Pos)) continue;
                    foreach (var dir in Directions)
                    {
                        var next = cur.Pos + dir;
                        if (closed.Contains(next) || !map.CanStep(cur.Pos, next)) continue;
                        int cost = dir.X != 0 && dir.Y != 0 ? DiagonalCost : StraightCost;
                        if (roads) cost = (int)Math.Round(cost * RoadScale / map.StepSpeed(cur.Pos, next));
                        Relax(open, best, cur, next, cur.G + cost, heuristic);
                    }
                    foreach (var next in map.LinksFrom(cur.Pos))
                    {
                        if (closed.Contains(next)) continue;
                        int span = Math.Abs(next.X - cur.Pos.X) + Math.Abs(next.Y - cur.Pos.Y) + 1;
                        Relax(open, best, cur, next, cur.G + StraightCost * span * (roads ? RoadScale : 1), heuristic);
                    }
                }
                return null;
            }

            private static void Relax(SortedSet<Node> open, Dictionary<GridPos, Node> best, Node cur, GridPos next, int g, Func<GridPos, int> h)
            {
                if (best.TryGetValue(next, out var known)) { if (g >= known.G) return; open.Remove(known); }
                var node = new Node(next, g, h(next), cur);
                best[next] = node; open.Add(node);
            }
        }
    }
}
