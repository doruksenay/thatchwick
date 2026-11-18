using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 32 kalanı: oda konfor puanına odanın büyüklüğü ve pencereleri girer (zemin ve duvar kaplaması zaten sayılıyordu).
    public class OdaKonforuTests
    {
        private static void Box(GridMap m, int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++)
            {
                m.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal), EdgeKind.Wall);
            }
            for (int y = y0; y <= y1; y++)
            {
                m.SetEdge(new Edge(x0, y, EdgeAxis.Vertical), EdgeKind.Wall);
                m.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical), EdgeKind.Wall);
            }
        }

        [Theory]
        [InlineData(2, -3)]
        [InlineData(4, -1)]
        [InlineData(9, 0)]
        [InlineData(16, 1)]
        [InlineData(25, 2)]
        [InlineData(36, 3)]
        public void Buyukluk_DarOdaEksiFerahOdaArti(int area, int expected)
        {
            Assert.Equal(expected, MoodService.SizeComfort(area));
        }

        [Fact]
        public void Pencere_PenceresizEksi_HerPencereArti_UcteDurur()
        {
            var world = new WorldState(new GridMap(12, 12));
            Box(world.Map, 1, 1, 3, 3);   // 3x3 = 9 karo: büyüklük 0
            var mood = new MoodService(world);
            var room = world.Map.Rooms.RoomAt(new GridPos(2, 2));
            Assert.Equal(-2, mood.ComfortOf(room).Windows);
            Assert.Equal(0, mood.ComfortOf(room).Size);

            world.Map.SetEdge(new Edge(2, 1, EdgeAxis.Horizontal), EdgeKind.Window);
            room = world.Map.Rooms.RoomAt(new GridPos(2, 2));
            Assert.Equal(1, mood.ComfortOf(room).Windows);

            world.Map.SetEdge(new Edge(1, 2, EdgeAxis.Vertical), EdgeKind.Window);
            world.Map.SetEdge(new Edge(4, 2, EdgeAxis.Vertical), EdgeKind.Window);
            world.Map.SetEdge(new Edge(2, 4, EdgeAxis.Horizontal), EdgeKind.Window);
            room = world.Map.Rooms.RoomAt(new GridPos(2, 2));
            Assert.Equal(4, room.WindowCount);
            Assert.Equal(3, mood.ComfortOf(room).Windows);   // tavan
        }

        [Fact]
        public void Bodrum_PencereBeklemez()
        {
            var m = new GridMap(8, 8);
            m.SetTile(new GridPos(3, 3, -1), true);
            m.SetTile(new GridPos(4, 3, -1), true);
            var room = m.Rooms.RoomAt(new GridPos(3, 3, -1));
            Assert.True(room.IsIndoor);
            Assert.Equal(-1, room.Level);
            Assert.Equal(0, MoodService.WindowComfort(room));
        }

        [Fact]
        public void SikisikKaranlikOda_EksiMoodlet_FerahPencereliOda_ArtiMoodlet()
        {
            var world = new WorldState(new GridMap(16, 16));
            Box(world.Map, 1, 1, 1, 2);          // 1x2 = 2 karo, penceresiz: -3 - 2 = -5
            Box(world.Map, 5, 5, 10, 9);         // 6x5 = 30 karo: +3
            world.Map.SetEdge(new Edge(6, 5, EdgeAxis.Horizontal), EdgeKind.Window);
            world.Map.SetEdge(new Edge(8, 5, EdgeAxis.Horizontal), EdgeKind.Window);
            var mood = new MoodService(world);

            var cramped = new Character(1, "Dar", new GridPos(1, 1)) { CanStarve = false, MoodEnabled = true };
            var airy = new Character(2, "Ferah", new GridPos(7, 7)) { CanStarve = false, MoodEnabled = true };
            cramped.SetAge(45); airy.SetAge(45);
            mood.Update(new[] { cramped, airy });

            var c1 = mood.ComfortOf(world.Map.Rooms.RoomAt(cramped.Position));
            Assert.Equal(-5, c1.Total);
            Assert.Contains("penceresiz", c1.Breakdown());
            Assert.Contains("dar", c1.Breakdown());
            Assert.Equal(-4, cramped.Mood.Total);

            var c2 = mood.ComfortOf(world.Map.Rooms.RoomAt(airy.Position));
            Assert.Equal(5, c2.Total);                // büyüklük +3, pencere +2
            Assert.Equal(2, airy.Mood.Total);          // yaşanır bir ev
        }

        [Fact]
        public void ZeminVeDuvar_YapiPuaniOlarakKalir()
        {
            // Zemin ve duvar kaplaması Structure'da sayılıyordu; Total artık Size ve Windows'u da içerir.
            var world = new WorldState(new GridMap(10, 10));
            Box(world.Map, 1, 1, 3, 3);
            var room = world.Map.Rooms.RoomAt(new GridPos(2, 2));
            var rc = new MoodService(world).ComfortOf(room);
            Assert.Equal(rc.Score + rc.Structure + rc.Size + rc.Windows, rc.Total);
        }
    }
}
