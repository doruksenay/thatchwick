using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Duvar, kapı ve pencerelerle çevrili bir karo bölgesi (çit, yarım duvar ve korkuluk odayı bölmez).</summary>
    public sealed class Room
    {
        public int Id { get; }
        public List<GridPos> Tiles { get; } = new List<GridPos>();
        /// <summary>Haritanın dışına açık değilse (tamamen çevrili) iç mekândır.</summary>
        public bool IsIndoor { get; internal set; }
        /// <summary>Bu bölgenin sınırındaki kapı sayısı.</summary>
        public int DoorCount { get; internal set; }
        /// <summary>Bu bölgenin sınırındaki pencere sayısı (sera için ışık).</summary>
        public int WindowCount { get; internal set; }
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
            // Bütün dünya taranır (her duvar değişiminde yeniden): iç döngü karo ve kenar bilgisini doğrudan dizilerden okur,
            // kuyruk karo indeksidir. Karo sırası (Room.Tiles) ve sonuç eski uygulamayla aynıdır (performans incelemesi 20.11.2026).
            int w = map.Width, h = map.Height, plane = w * h;
            var byTile = new Room[plane * GridMap.LevelCount];
            var rooms = new List<Room>();
            var queue = _queue ?? (_queue = new int[1024]);
            int levels = GridMap.LevelCount;

            for (int zi = 0; zi < levels; zi++)
            {
                int z = zi + GridMap.MinLevel, levelBase = zi * plane;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int si = levelBase + y * w + x;
                        if (!map.ExistsRaw(si) || byTile[si] != null) continue;

                        var room = new Room(rooms.Count) { IsIndoor = true, Level = z };
                        rooms.Add(room);
                        byTile[si] = room;
                        int head = 0, tail = 0;
                        queue[tail++] = si;
                        // Kapı ve pencere kümeleri yalnızca gerekince kurulur (çoğu bölgede yoktur).
                        HashSet<Edge> doorEdges = null, windowEdges = null;

                        while (head < tail)
                        {
                            int ci = queue[head++];
                            int cx = (ci - levelBase) % w, cy = (ci - levelBase) / w;
                            room.Tiles.Add(new GridPos(cx, cy, z));
                            for (int d = 0; d < 4; d++)
                            {
                                // Sıra: +x, -x, +y, -y (eski Dirs dizisiyle aynı).
                                int nx = cx, ny = cy;
                                EdgeKind kind;
                                Edge edge;
                                switch (d)
                                {
                                    case 0: nx = cx + 1; kind = map.EdgeVRaw(cx + 1, cy, zi); edge = new Edge(cx + 1, cy, EdgeAxis.Vertical, z); break;
                                    case 1: nx = cx - 1; kind = map.EdgeVRaw(cx, cy, zi); edge = new Edge(cx, cy, EdgeAxis.Vertical, z); break;
                                    case 2: ny = cy + 1; kind = map.EdgeHRaw(cx, cy + 1, zi); edge = new Edge(cx, cy + 1, EdgeAxis.Horizontal, z); break;
                                    default: ny = cy - 1; kind = map.EdgeHRaw(cx, cy, zi); edge = new Edge(cx, cy, EdgeAxis.Horizontal, z); break;
                                }
                                bool inPlane = nx >= 0 && ny >= 0 && nx < w && ny < h;
                                int ni = levelBase + ny * w + nx;
                                if (!inPlane || !map.ExistsRaw(ni))
                                {
                                    // Harita kenarı ya da karo olmayan yan: zemin ve üst katta açık çıkış sayılır
                                    // (balkon); bodrumda kaya olduğu için kapalıdır. Üst kattaki boşluğun altı bir iç
                                    // odaysa (merdiven boşluğu) dışarı açılmaz.
                                    bool openToBelowRoom = z > 0 && inPlane && byTile[ni - plane] is Room below && below.IsIndoor;
                                    if (!EdgeKinds.IsRoomBoundary(kind) && !openToBelowRoom) { if (z >= 0) room.IsIndoor = false; }
                                    else if (kind == EdgeKind.Door) (doorEdges ?? (doorEdges = new HashSet<Edge>())).Add(edge);
                                    continue;
                                }
                                if (kind == EdgeKind.Door) { (doorEdges ?? (doorEdges = new HashSet<Edge>())).Add(edge); continue; }
                                if (kind == EdgeKind.Window) { (windowEdges ?? (windowEdges = new HashSet<Edge>())).Add(edge); continue; }
                                if (EdgeKinds.IsRoomBoundary(kind)) continue;     // çit, yarım duvar, korkuluk odayı bölmez
                                if (byTile[ni] != null) continue;
                                byTile[ni] = room;
                                if (tail == queue.Length) { Array.Resize(ref queue, queue.Length * 2); _queue = queue; }
                                queue[tail++] = ni;
                            }
                        }
                        room.DoorCount = doorEdges?.Count ?? 0;
                        room.WindowCount = windowEdges?.Count ?? 0;
                    }
                }
            }
            return new RoomMap(map.Width, map.Height, byTile, rooms);
        }

        [ThreadStatic] private static int[] _queue;

        private static int Slot(GridMap map, GridPos p) => ((p.Z - GridMap.MinLevel) * map.Height + p.Y) * map.Width + p.X;
    }
}
