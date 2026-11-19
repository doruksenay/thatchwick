using System;
using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Ulaşılabilirlik: bir başlangıç karosundan yürünebilen karolar (dört yön, duvar/çit kenarı geçilmez,
    /// merdiven bağlantıları ve bölge geçitleri dahil). Doğrulayıcı (etkileşim hücresi kapıdan ulaşılabilir mi),
    /// döşeyici (dolaşım yolu kopmasın) ve debug katmanı (ulaşılamayan karolar) kullanır.
    /// </summary>
    public static class Reach
    {
        /// <summary>start'tan ulaşılabilen karolar. inside: aramayı sınırlar (null = sınırsız). blocked: ek engel (taslak yerleşim).</summary>
        public static HashSet<GridPos> From(GridMap map, GridPos start, Func<GridPos, bool> inside = null, Func<GridPos, bool> blocked = null)
        {
            var seen = new HashSet<GridPos>();
            if (!Open(map, start, blocked)) return seen;
            var q = new Queue<GridPos>();
            seen.Add(start); q.Enqueue(start);
            while (q.Count > 0)
            {
                var c = q.Dequeue();
                foreach (var d in Facing.All)
                {
                    var n = Facing.Step(c, d);
                    if (seen.Contains(n) || (inside != null && !inside(n)) || !Open(map, n, blocked)) continue;
                    if (map.EdgeBlocks(GridMap.EdgeBetween(c, n))) continue;
                    seen.Add(n); q.Enqueue(n);
                }
                foreach (var n in map.LinksFrom(c))
                {
                    if (seen.Contains(n) || (inside != null && !inside(n)) || !Open(map, n, blocked)) continue;
                    seen.Add(n); q.Enqueue(n);
                }
            }
            return seen;
        }

        private static bool Open(GridMap map, GridPos p, Func<GridPos, bool> blocked) =>
            map.HasTile(p) && !map.IsBlocked(p) && !map.IsWater(p) && (blocked == null || !blocked(p));

        /// <summary>Hedefin kendisi ya da dört komşusundan biri (araya kenar girmeden) ulaşılabilir kümede mi.</summary>
        public static bool Touches(GridMap map, HashSet<GridPos> reach, GridPos target)
        {
            if (reach.Contains(target)) return true;
            foreach (var d in Facing.All)
            {
                var n = Facing.Step(target, d);
                if (reach.Contains(n) && !map.EdgeBlocks(GridMap.EdgeBetween(n, target))) return true;
            }
            return false;
        }
    }
}
