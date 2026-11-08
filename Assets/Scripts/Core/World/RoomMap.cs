using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Duvar, kapı ve pencerelerle çevrili bir karo bölgesi.</summary>
    public sealed class Room
    {
        public int Id { get; }
        public List<GridPos> Tiles { get; } = new List<GridPos>();
        /// <summary>Haritanın dışına açık değilse (tamamen çevrili) iç mekândır.</summary>
        public bool IsIndoor { get; internal set; }
        /// <summary>Bu bölgenin sınırındaki kapı sayısı.</summary>
        public int DoorCount { get; internal set; }
        public int Area => Tiles.Count;

        internal Room(int id) { Id = id; }
    }

    /// <summary>
    /// Oda tespiti: karolar, aralarında duvar/kapı/pencere kenarı olmayan komşulukla bölgelere ayrılır
    /// (kapı odaları birbirinden ayırır ama geçişe izin verir). Haritanın kenarından açık çıkış
    /// bulan bölge dış mekândır; diğerleri iç mekân odalarıdır.
    /// </summary>
    public sealed class RoomMap
    {
        private readonly Room[] _byTile;
        private readonly int _width;

        public IReadOnlyList<Room> Rooms { get; }

        private RoomMap(int width, Room[] byTile, List<Room> rooms)
        {
            _width = width;
            _byTile = byTile;
            Rooms = rooms;
        }

        public Room RoomAt(GridPos p) => p.X < 0 || p.Y < 0 || p.X >= _width || p.Y * _width + p.X >= _byTile.Length || p.X >= _width ? null : _byTile[p.Y * _width + p.X];

        public IEnumerable<Room> IndoorRooms
        {
            get { foreach (var r in Rooms) if (r.IsIndoor) yield return r; }
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        public static RoomMap Compute(GridMap map)
        {
            var byTile = new Room[map.Width * map.Height];
            var rooms = new List<Room>();
            var queue = new Queue<GridPos>();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (byTile[y * map.Width + x] != null) continue;

                    var room = new Room(rooms.Count) { IsIndoor = true };
                    rooms.Add(room);
                    var start = new GridPos(x, y);
                    byTile[y * map.Width + x] = room;
                    queue.Enqueue(start);
                    var doorEdges = new HashSet<Edge>();

                    while (queue.Count > 0)
                    {
                        var cur = queue.Dequeue();
                        room.Tiles.Add(cur);
                        foreach (var d in Dirs)
                        {
                            var next = cur + d;
                            var edge = GridMap.EdgeBetween(cur, next);
                            var kind = map.GetEdge(edge);
                            if (!map.InBounds(next))
                            {
                                if (kind == EdgeKind.None) room.IsIndoor = false; // açık çıkış
                                else if (kind == EdgeKind.Door) doorEdges.Add(edge);
                                continue;
                            }
                            if (kind == EdgeKind.Door) { doorEdges.Add(edge); continue; }
                            if (kind != EdgeKind.None) continue;
                            if (byTile[next.Y * map.Width + next.X] != null) continue;
                            byTile[next.Y * map.Width + next.X] = room;
                            queue.Enqueue(next);
                        }
                    }
                    room.DoorCount = doorEdges.Count;
                }
            }
            return new RoomMap(map.Width, byTile, rooms);
        }
    }
}
