using System;
using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Relations
{
    /// <summary>
    /// Gönül yolu sahnelerinin oturma düzeni (Project belgesi 67b §3): damat ve gelin evin önünde karşılıklı durur, iki tarafın
    /// büyükleri kendi taraflarının arkasında, davetliler çevrede halka olur. Kına gecesinde gelin ortada, annesi ve büyükleri
    /// yanı başında. Her yer yürünebilir ve boş bir karodur; istenen karo doluysa en yakın boş karo alınır.
    /// </summary>
    public static class SceneSeats
    {
        /// <summary>Taraf sırası: önce iki yan, sonra arka.</summary>
        private static readonly int[] SideOffsets = { 1, -1, 2, -2, 0, 3, -3 };

        private static readonly (int dx, int dy)[] Beside = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1) };

        /// <summary>Kişi başına (yer, döneceği karo); yer bulunamazsa yer null (kişi olduğu yerde kalır, yine de döner).</summary>
        public static List<(GridPos? seat, GridPos face)> Layout(GridPos center, IReadOnlyList<SceneRole> roles, Func<GridPos, bool> walkable)
        {
            var taken = new HashSet<GridPos>();
            var result = new List<(GridPos?, GridPos)>();
            int groomSide = 0, brideSide = 0, guests = 0, beside = 0;
            var ring = RingCells();
            bool henna = false;
            foreach (var r in roles) if (r == SceneRole.Center) henna = true;
            GridPos groomSeat = Offset(center, -1, 0), brideSeat = Offset(center, 1, 0);

            foreach (var role in roles)
            {
                GridPos want;
                switch (role)
                {
                    case SceneRole.Groom: want = groomSeat; break;
                    case SceneRole.Bride: want = brideSeat; break;
                    case SceneRole.Center: want = center; break;
                    case SceneRole.GroomSide: want = Offset(center, -2, SideOffsets[groomSide++ % SideOffsets.Length]); break;
                    case SceneRole.BrideSide:
                        if (henna) { var b = Beside[beside++ % Beside.Length]; want = Offset(center, b.dx, b.dy); }
                        else want = Offset(center, 2, SideOffsets[brideSide++ % SideOffsets.Length]);
                        break;
                    default: { var g = ring[guests++ % ring.Count]; want = Offset(center, g.dx, g.dy); break; }
                }
                var seat = Nearest(want, walkable, taken);
                if (seat.HasValue) taken.Add(seat.Value);
                GridPos face = role == SceneRole.Groom ? brideSeat : role == SceneRole.Bride ? groomSeat : center;
                if (role == SceneRole.Center) face = Offset(center, 0, 1);
                result.Add((seat, face));
            }
            return result;
        }

        private static GridPos Offset(GridPos p, int dx, int dy) => new GridPos(p.X + dx, p.Y + dy, p.Z);

        /// <summary>İstenen karo ya da 2 karo içindeki en yakın boş, yürünebilir karo.</summary>
        private static GridPos? Nearest(GridPos want, Func<GridPos, bool> walkable, HashSet<GridPos> taken)
        {
            for (int d = 0; d <= 2; d++)
                for (int dx = -d; dx <= d; dx++)
                {
                    int rest = d - Math.Abs(dx);
                    foreach (int dy in rest == 0 ? new[] { 0 } : new[] { rest, -rest })
                    {
                        var p = new GridPos(want.X + dx, want.Y + dy, want.Z);
                        if (!taken.Contains(p) && (walkable == null || walkable(p))) return p;
                    }
                }
            return null;
        }

        /// <summary>Davetli halkası: 3 karo uzaklıkta, önden (güneyden) başlayıp iki yana açılarak; dolunca 4 karo.</summary>
        private static List<(int dx, int dy)> RingCells()
        {
            var list = new List<(int dx, int dy, double key)>();
            for (int radius = 3; radius <= 4; radius++)
                for (int dx = -radius; dx <= radius; dx++)
                    for (int dy = -radius; dy <= radius; dy++)
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) == radius && (radius == 3 || (dx + dy) % 2 == 0))
                            list.Add((dx, dy, radius * 10 + Math.Abs(Math.Atan2(dx, dy))));
            list.Sort((a, b) => a.key.CompareTo(b.key));
            var cells = new List<(int, int)>();
            foreach (var c in list) cells.Add((c.dx, c.dy));
            return cells;
        }
    }
}
