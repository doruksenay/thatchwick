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
        /// <summary>Odanın katı (0 zemin, 1 üst kat, -1 bodrum).</summary>
        public int Level { get; internal set; }

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
        private readonly int _height;

        public IReadOnlyList<Room> Rooms { get; }

        private RoomMap(int width, int height, Room[] byTile, List<Room> rooms)
        {
            _width = width;
            _height = height;
            _byTile = byTile;
            Rooms = rooms;
        }

        /// <summary>Karonun odası; harita dışıysa ya da karo yoksa (yapılmamış üst kat, kazılmamış bodrum) null.</summary>
        public Room RoomAt(GridPos p)
        {
            if (p.X < 0 || p.Y < 0 || p.X >= _width || p.Y >= _height || !GridMap.IsValidLevel(p.Z)) return null;
            int i = ((p.Z - GridMap.MinLevel) * _height + p.Y) * _width + p.X;
            return i < _byTile.Length ? _byTile[i] : null;
        }

        public IEnumerable<Room> IndoorRooms
        {
            get { foreach (var r in Rooms) if (r.IsIndoor) yield return r; }
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        public static RoomMap Compute(GridMap map)
        {
            int plane = map.Width * map.Height;
            var byTile = new Room[plane * GridMap.LevelCount];
            var rooms = new List<Room>();
            var queue = new Queue<GridPos>();

            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        var start = new GridPos(x, y, z);
                        if (!map.HasTile(start) || byTile[Slot(map, start)] != null) continue;

                        var room = new Room(rooms.Count) { IsIndoor = true, Level = z };
                        rooms.Add(room);
                        byTile[Slot(map, start)] = room;
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
                                if (!map.InBounds(next) || !map.HasTile(next))
                                {
                                    // Harita kenarı ya da karo olmayan yan: zemin ve üst katta açık çıkış sayılır
                                    // (balkon); bodrumda kaya olduğu için kapalıdır.
                                    if (kind == EdgeKind.None) { if (z >= 0) room.IsIndoor = false; }
                                    else if (kind == EdgeKind.Door) doorEdges.Add(edge);
                                    continue;
                                }
                                if (kind == EdgeKind.Door) { doorEdges.Add(edge); continue; }
                                if (kind != EdgeKind.None) continue;
                                int ni = Slot(map, next);
                                if (byTile[ni] != null) continue;
                                byTile[ni] = room;
                                queue.Enqueue(next);
                            }
                        }
                        room.DoorCount = doorEdges.Count;
                    }
                }
            }
            return new RoomMap(map.Width, map.Height, byTile, rooms);
        }

        private static int Slot(GridMap map, GridPos p) => ((p.Z - GridMap.MinLevel) * map.Height + p.Y) * map.Width + p.X;
    }
}
