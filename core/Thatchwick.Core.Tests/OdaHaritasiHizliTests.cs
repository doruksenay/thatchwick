using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Oda tespiti ve CanStep, kenarları doğrudan dizilerden okuyacak biçimde hızlandırıldı (performans incelemesi
    /// 20.11.2026). Eski, sözlükten soran uygulamalar burada başvuru olarak durur: gerçek başlangıç dünyasında ve rastgele
    /// çok katlı haritalarda sonuç birebir aynı olmalı (oda sayısı, karo sırası, iç/dış, kapı ve pencere sayıları).
    /// </summary>
    public class OdaHaritasiHizliTests
    {
        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        private sealed class RefRoom { public bool Indoor = true; public int Level, Doors, Windows; public List<GridPos> Tiles = new List<GridPos>(); }

        private static List<RefRoom> Reference(GridMap map)
        {
            int plane = map.Width * map.Height;
            int Slot(GridPos p) => ((p.Z - GridMap.MinLevel) * map.Height + p.Y) * map.Width + p.X;
            var byTile = new RefRoom[plane * GridMap.LevelCount];
            var rooms = new List<RefRoom>();
            var queue = new Queue<GridPos>();
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                for (int y = 0; y < map.Height; y++)
                    for (int x = 0; x < map.Width; x++)
                    {
                        var start = new GridPos(x, y, z);
                        if (!map.HasTile(start) || byTile[Slot(start)] != null) continue;
                        var room = new RefRoom { Level = z };
                        rooms.Add(room);
                        byTile[Slot(start)] = room;
                        queue.Enqueue(start);
                        var doors = new HashSet<Edge>(); var windows = new HashSet<Edge>();
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
                                    bool openToBelow = z > 0 && map.InBounds(next) && byTile[Slot(next.OnLevel(z - 1))] is RefRoom below && below.Indoor;
                                    if (!EdgeKinds.IsRoomBoundary(kind) && !openToBelow) { if (z >= 0) room.Indoor = false; }
                                    else if (kind == EdgeKind.Door) doors.Add(edge);
                                    continue;
                                }
                                if (kind == EdgeKind.Door) { doors.Add(edge); continue; }
                                if (kind == EdgeKind.Window) { windows.Add(edge); continue; }
                                if (EdgeKinds.IsRoomBoundary(kind)) continue;
                                int ni = Slot(next);
                                if (byTile[ni] != null) continue;
                                byTile[ni] = room;
                                queue.Enqueue(next);
                            }
                        }
                        room.Doors = doors.Count; room.Windows = windows.Count;
                    }
            return rooms;
        }

        // Eski CanStep (her kenar için Edge kurup soran).
        private static bool RefCanStep(GridMap map, GridPos from, GridPos to)
        {
            if (from.Z != to.Z) return false;
            if (!map.IsWalkable(to)) return false;
            int dx = to.X - from.X, dy = to.Y - from.Y;
            if (dx == 0 && dy == 0) return true;
            if (dx == 0 || dy == 0) return !map.EdgeBlocks(GridMap.EdgeBetween(from, to));
            var h = new GridPos(from.X + dx, from.Y, from.Z);
            var v = new GridPos(from.X, from.Y + dy, from.Z);
            if (map.IsBlocked(h) || map.IsBlocked(v)) return false;
            return !map.EdgeBlocks(GridMap.EdgeBetween(from, h)) && !map.EdgeBlocks(GridMap.EdgeBetween(h, to)) &&
                   !map.EdgeBlocks(GridMap.EdgeBetween(from, v)) && !map.EdgeBlocks(GridMap.EdgeBetween(v, to));
        }

        private static void AssertSame(GridMap map)
        {
            var expected = Reference(map);
            var actual = map.Rooms.Rooms;
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Indoor, actual[i].IsIndoor);
                Assert.Equal(expected[i].Level, actual[i].Level);
                Assert.Equal(expected[i].Doors, actual[i].DoorCount);
                Assert.Equal(expected[i].Windows, actual[i].WindowCount);
                Assert.True(expected[i].Tiles.SequenceEqual(actual[i].Tiles), $"oda {i}: karo sırası farklı");
            }
            // CanStep: her karodan sekiz komşuya (kenar ve ızgara dışı dahil).
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                for (int y = -1; y <= map.Height; y++)
                    for (int x = -1; x <= map.Width; x++)
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                var a = new GridPos(x, y, z); var b = new GridPos(x + dx, y + dy, z);
                                bool want = RefCanStep(map, a, b);
                                if (want != map.CanStep(a, b)) Assert.True(false, $"CanStep {a} -> {b}: beklenen {want}");
                            }
        }

        [Fact]
        public void RastgeleCokKatliHaritalar_EskiUygulamaylaAyni()
        {
            var rng = new Random(77);
            var kinds = new[] { EdgeKind.Wall, EdgeKind.Wall, EdgeKind.Wall, EdgeKind.Door, EdgeKind.Window, EdgeKind.Fence, EdgeKind.HalfWall, EdgeKind.Gate, EdgeKind.Railing };
            for (int trial = 0; trial < 25; trial++)
            {
                int w = 6 + rng.Next(14), h = 6 + rng.Next(14);
                var map = new GridMap(w, h);
                // Üst kat ve bodrum karoları, kapalı karolar.
                for (int i = 0; i < w * h / 3; i++) map.SetTile(new GridPos(rng.Next(w), rng.Next(h), 1), true);
                for (int i = 0; i < w * h / 5; i++) map.SetTile(new GridPos(rng.Next(w), rng.Next(h), -1), true);
                for (int i = 0; i < w * h / 8; i++) map.SetBlocked(new GridPos(rng.Next(w), rng.Next(h)), true);
                // Kutular (kapalı odalar) ve rastgele kenarlar.
                for (int b = 0; b < 4; b++)
                {
                    int x0 = rng.Next(w - 3), y0 = rng.Next(h - 3), x1 = Math.Min(w - 1, x0 + 1 + rng.Next(4)), y1 = Math.Min(h - 1, y0 + 1 + rng.Next(4)), z = rng.Next(3) - 1;
                    for (int x = x0; x <= x1; x++) { Set(map, new Edge(x, y0, EdgeAxis.Horizontal, z), EdgeKind.Wall); Set(map, new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), EdgeKind.Wall); }
                    for (int y = y0; y <= y1; y++) { Set(map, new Edge(x0, y, EdgeAxis.Vertical, z), EdgeKind.Wall); Set(map, new Edge(x1 + 1, y, EdgeAxis.Vertical, z), EdgeKind.Wall); }
                }
                for (int i = 0; i < w * h / 4; i++)
                {
                    bool horizontal = rng.Next(2) == 0;
                    var e = horizontal ? new Edge(rng.Next(w), rng.Next(h + 1), EdgeAxis.Horizontal, rng.Next(3) - 1) : new Edge(rng.Next(w + 1), rng.Next(h), EdgeAxis.Vertical, rng.Next(3) - 1);
                    Set(map, e, kinds[rng.Next(kinds.Length)]);
                }
                AssertSame(map);
                // Kenar silme ve tür değiştirme sonrası da aynı (diziler sözlükle birlikte güncellenir).
                foreach (var kv in map.Edges.Take(10).ToList()) map.SetEdge(kv.Key, rng.Next(2) == 0 ? EdgeKind.None : EdgeKind.Door);
                AssertSame(map);
            }
        }

        private static void Set(GridMap map, Edge e, EdgeKind kind) { if (map.EdgeInBounds(e)) map.SetEdge(e, kind); }

        [Fact]
        public void GercekBaslangicDunyasi_EskiUygulamaylaAyni()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var w = new WorldState(StarterWorld.BuildMap(lay));
            StarterWorld.Place(w, db, lay, 1);
            var expected = Reference(w.Map);
            var actual = w.Map.Rooms.Rooms;
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Indoor, actual[i].IsIndoor);
                Assert.Equal(expected[i].Doors, actual[i].DoorCount);
                Assert.Equal(expected[i].Windows, actual[i].WindowCount);
                Assert.True(expected[i].Tiles.SequenceEqual(actual[i].Tiles), $"oda {i}: karo sırası farklı");
            }
        }
    }
}
