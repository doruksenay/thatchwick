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
            // Başlangıç kapalı olabilir (üstüne eşya konmuş karakter): oradan dışarı adım atılabilir, böylece kimse hapsolmaz.
            if (!map.HasTile(start) || !map.IsWalkable(goal)) return null;
            if (start == goal) return new List<GridPos> { start };
            return Search(map, start, goal, null, null);
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
            if (!map.HasTile(start)) return null;

            var targets = new List<GridPos>();
            foreach (var g in goals) if (map.IsWalkable(g)) targets.Add(g);
            if (targets.Count == 0) return null;
            if (targets.Contains(start)) return new List<GridPos> { start };

            var set = new HashSet<GridPos>(targets);
            return Search(map, start, default, set, targets);
        }

        // ---- arama (dizi tabanlı; performans incelemesi 20.11.2026) ----
        // Düğüm başına nesne, konum sözlüğü ve kapalı küme yerine karo indeksli diziler: maliyet (g), geldiği karo ve
        // "bu aramada görüldü / kapandı" damgaları. Diziler iş parçacığı başına bir kez ayrılır; her arama yeni bir damga
        // numarası kullanır, temizlik gerekmez. Açık küme ikili yığındır; sıralama kuralı (F, H, Y, X, Z, G) eskisiyle
        // aynıdır, bu yüzden bulunan yollar birebir aynıdır (PathfinderHeapTests eski uygulamayla karşılaştırır).
        private struct Entry { public int F, H, G, Idx, X, Y, Z; }

        private sealed class Buffers
        {
            public int[] G, From, Seen, Closed;
            public int Stamp;
            public Entry[] Heap = new Entry[256];
            public int Count;

            public void Ensure(int n)
            {
                if (G != null && G.Length >= n) return;
                G = new int[n]; From = new int[n]; Seen = new int[n]; Closed = new int[n];
                Stamp = 0;
            }

            private static bool Less(in Entry a, in Entry b)
            {
                if (a.F != b.F) return a.F < b.F;
                if (a.H != b.H) return a.H < b.H;
                if (a.Y != b.Y) return a.Y < b.Y;
                if (a.X != b.X) return a.X < b.X;
                if (a.Z != b.Z) return a.Z < b.Z;
                return a.G < b.G;
            }

            public void Push(in Entry e)
            {
                if (Count == Heap.Length) Array.Resize(ref Heap, Heap.Length * 2);
                int i = Count++;
                while (i > 0)
                {
                    int parent = (i - 1) >> 1;
                    if (!Less(e, Heap[parent])) break;
                    Heap[i] = Heap[parent];
                    i = parent;
                }
                Heap[i] = e;
            }

            public Entry Pop()
            {
                var top = Heap[0];
                var last = Heap[--Count];
                int i = 0;
                while (true)
                {
                    int l = 2 * i + 1;
                    if (l >= Count) break;
                    int r = l + 1;
                    int m = r < Count && Less(Heap[r], Heap[l]) ? r : l;
                    if (!Less(Heap[m], last)) break;
                    Heap[i] = Heap[m];
                    i = m;
                }
                Heap[i] = last;
                return top;
            }
        }

        [ThreadStatic] private static Buffers _buffers;

        // Tek hedef (goalSet null) ya da hedef kümesi (goalSet + targets). Kestirim: hedef(ler)e octile mesafenin en küçüğü.
        private static List<GridPos> Search(GridMap map, GridPos start, GridPos goal, HashSet<GridPos> goalSet, List<GridPos> targets)
        {
            int w = map.Width, h = map.Height, plane = w * h;
            var buf = _buffers ?? (_buffers = new Buffers());
            buf.Ensure(plane * GridMap.LevelCount);
            if (++buf.Stamp == int.MaxValue) { Array.Clear(buf.Seen, 0, buf.Seen.Length); Array.Clear(buf.Closed, 0, buf.Closed.Length); buf.Stamp = 1; }
            int stamp = buf.Stamp;
            buf.Count = 0;
            int[] gArr = buf.G, from = buf.From, seen = buf.Seen, closed = buf.Closed;

            // Yol varsa adım maliyeti yolun hızına bölünür (yol tercih edilir); kesirler için maliyetler x10 ölçeklenir.
            bool roads = map.HasTerrainSpeed;

            int startIdx = ((start.Z - GridMap.MinLevel) * h + start.Y) * w + start.X;
            gArr[startIdx] = 0; from[startIdx] = -1; seen[startIdx] = stamp;
            buf.Push(new Entry { F = H(start), H = H(start), G = 0, Idx = startIdx, X = start.X, Y = start.Y, Z = start.Z });

            while (buf.Count > 0)
            {
                var cur = buf.Pop();
                // Daha ucuz yolla yeniden eklenmiş karonun eski kaydı atlanır (tembel silme).
                if (gArr[cur.Idx] != cur.G) continue;
                var pos = new GridPos(cur.X, cur.Y, cur.Z);

                if (goalSet != null ? goalSet.Contains(pos) : pos == goal) return Rebuild(cur.Idx, from, w, h);
                if (closed[cur.Idx] == stamp) continue;
                closed[cur.Idx] = stamp;

                for (int d = 0; d < Directions.Length; d++)
                {
                    var dir = Directions[d];
                    int nx = cur.X + dir.X, ny = cur.Y + dir.Y;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    int ni = cur.Idx + dir.Y * w + dir.X;
                    if (closed[ni] == stamp) continue;
                    var next = new GridPos(nx, ny, cur.Z);
                    if (!map.CanStep(pos, next)) continue;

                    int cost = d >= 4 ? DiagonalCost : StraightCost;
                    if (roads) cost = (int)Math.Round(cost * RoadScale / map.StepSpeed(pos, next));
                    Relax(buf, stamp, cur, ni, next, cur.G + cost);
                }

                // Merdiven bağlantıları ve geçitler: kat ya da harita değiştirir; maliyet yatay mesafe + 1 adım.
                if (map.HasLinksFrom(pos))
                    foreach (var next in map.LinksFrom(pos))
                    {
                        int ni = ((next.Z - GridMap.MinLevel) * h + next.Y) * w + next.X;
                        if (closed[ni] == stamp) continue;
                        int span = Math.Abs(next.X - cur.X) + Math.Abs(next.Y - cur.Y) + 1;
                        Relax(buf, stamp, cur, ni, next, cur.G + StraightCost * span * (roads ? RoadScale : 1));
                    }
            }

            return null;

            int H(GridPos p)
            {
                if (goalSet == null) return Heuristic(p, goal, roads);
                int best = int.MaxValue;
                for (int t = 0; t < targets.Count; t++) best = Math.Min(best, Heuristic(p, targets[t], roads));
                return best;
            }

            void Relax(Buffers b, int st, in Entry current, int ni, GridPos next, int g)
            {
                if (b.Seen[ni] == st && g >= b.G[ni]) return;
                b.Seen[ni] = st;
                b.G[ni] = g;
                b.From[ni] = current.Idx;
                int hh = H(next);
                b.Push(new Entry { F = g + hh, H = hh, G = g, Idx = ni, X = next.X, Y = next.Y, Z = next.Z });
            }
        }

        private static List<GridPos> Rebuild(int idx, int[] from, int w, int h)
        {
            var path = new List<GridPos>();
            int plane = w * h;
            for (int i = idx; i >= 0; i = from[i])
                path.Add(new GridPos(i % w, (i / w) % h, i / plane + GridMap.MinLevel));
            path.Reverse();
            return path;
        }

        // Octile mesafe (kat farkı yok sayılır, böylece hâlâ kabul edilebilir): 8 yönlü harekette kabul edilebilir (admissible) tahmin.
        private static int Heuristic(GridPos a, GridPos b, bool roads = false)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            int diag = Math.Min(dx, dy);
            int h = DiagonalCost * diag + StraightCost * (Math.Max(dx, dy) - diag);
            // Yollu haritada en hızlı yol (kaldırım x2) varsayılır: tahmin hâlâ kabul edilebilir.
            return roads ? (int)(h * RoadScale / GridMap.PavedSpeed) : h;
        }

        private const int RoadScale = 10;
    }
}
