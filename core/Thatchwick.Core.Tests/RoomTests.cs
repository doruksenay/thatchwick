using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class RoomTests
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

        [Fact]
        public void DuvarsizHarita_TekDisMekan()
        {
            var m = new GridMap(6, 6);
            Assert.Single(m.Rooms.Rooms);
            Assert.False(m.Rooms.Rooms[0].IsIndoor);
        }

        [Fact]
        public void KapaliKutu_IcMekanOda()
        {
            var m = new GridMap(10, 10);
            Box(m, 2, 2, 4, 4);
            var room = m.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(room.IsIndoor);
            Assert.Equal(9, room.Area);
            Assert.False(m.Rooms.RoomAt(new GridPos(0, 0)).IsIndoor);
            Assert.NotSame(room, m.Rooms.RoomAt(new GridPos(0, 0)));
        }

        [Fact]
        public void Bosluk_OdayiDisMekanYapar()
        {
            var m = new GridMap(10, 10);
            Box(m, 2, 2, 4, 4);
            m.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.None);
            Assert.False(m.Rooms.RoomAt(new GridPos(3, 3)).IsIndoor);
            Assert.Same(m.Rooms.RoomAt(new GridPos(3, 3)), m.Rooms.RoomAt(new GridPos(0, 0)));
        }

        [Fact]
        public void Kapi_OdayiAyirirAmaIcMekanKalir_VeSayilir()
        {
            var m = new GridMap(10, 10);
            Box(m, 2, 2, 4, 4);
            m.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Door);
            var room = m.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(room.IsIndoor);
            Assert.Equal(1, room.DoorCount);
        }

        [Fact]
        public void IkiBitisikOda_AyriOdalar()
        {
            var m = new GridMap(12, 8);
            Box(m, 1, 1, 3, 3);
            Box(m, 4, 1, 6, 3);
            Assert.Equal(2, m.Rooms.IndoorRooms.Count());
        }

        [Fact]
        public void HaritaKenarindaDuvarliOda_IcMekan()
        {
            var m = new GridMap(6, 6);
            Box(m, 0, 0, 2, 2);
            Assert.True(m.Rooms.RoomAt(new GridPos(1, 1)).IsIndoor);
        }

        [Fact]
        public void Kenar_DegisinceOdalarYenidenHesaplanir()
        {
            var m = new GridMap(10, 10);
            var before = m.Rooms;
            Box(m, 2, 2, 4, 4);
            Assert.NotSame(before, m.Rooms);
        }

        // ---- yol yenileme ----

        [Fact]
        public void YolUstuneDuvarGelince_YenidenYolBulur()
        {
            var map = new GridMap(10, 6);
            var world = new WorldState(map);
            var c = new Character(1, "A", new GridPos(0, 2), true);
            var runner = new JobRunner(c, world);
            runner.Command(Job.MoveTo(new GridPos(9, 2)));
            runner.Tick(0.0028);   // yaklaşık 3 karo
            Assert.InRange(c.Position.X, 1, 4);
            // önüne tam duvar çek ama (5,5) açık bırak
            for (int y = 0; y < 5; y++) map.SetEdge(new Edge(5, y, EdgeAxis.Vertical), EdgeKind.Wall);
            for (int i = 0; i < 200 && !runner.IsIdle; i++) runner.Tick(0.01);
            Assert.True(runner.IsIdle);
            Assert.Equal(new GridPos(9, 2), c.Position);
        }

        [Fact]
        public void Esya_DuvarinOtekiYanindanKullanilmaz()
        {
            var map = new GridMap(8, 4);
            var world = new WorldState(map);
            var bed = new ItemDef { Id = "bed", DisplayName = "Yatak", Interactions = new List<InteractionDef>
                { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 50, DurationDays = 0.1 } } };
            var item = world.PlaceItem(bed, new GridPos(4, 1));
            // eşyanın etrafını duvarla çevir: kapalı kutu, karakter dışarıda
            map.SetEdge(new Edge(4, 1, EdgeAxis.Horizontal), EdgeKind.Wall);
            map.SetEdge(new Edge(4, 2, EdgeAxis.Horizontal), EdgeKind.Wall);
            map.SetEdge(new Edge(4, 1, EdgeAxis.Vertical), EdgeKind.Wall);
            map.SetEdge(new Edge(5, 1, EdgeAxis.Vertical), EdgeKind.Wall);
            var c = new Character(1, "A", new GridPos(0, 1), true);
            var runner = new JobRunner(c, world);
            runner.Command(Job.UseItem(item.Id, "sleep"));
            runner.Tick(0.01);
            Assert.NotNull(runner.LastFailure);
        }

        // ---- arsa ----

        private static (BuildService svc, Household house, GridMap map) Plot(int akce)
        {
            var map = new GridMap(16, 16);
            map.RestrictOwnership(4, 4, 7, 7);
            var house = new Household(akce);
            return (new BuildService(new WorldState(map), house, new ContentDatabase()), house, map);
        }

        [Fact]
        public void Arsa_DisindaDuvarVeZeminKurulmaz()
        {
            var (s, _, _) = Plot(100);
            Assert.False(s.PaintFloor(new GridPos(0, 0), FloorType.Wood).Ok);
            Assert.False(s.SetEdge(new Edge(0, 0, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
            Assert.True(s.PaintFloor(new GridPos(5, 5), FloorType.Wood).Ok);
            Assert.True(s.SetEdge(new Edge(5, 5, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
        }

        [Fact]
        public void Arsa_SinirKenariIkiYandanBiriSahipseKurulur()
        {
            var (s, _, _) = Plot(100);
            Assert.True(s.SetEdge(new Edge(4, 4, EdgeAxis.Vertical), EdgeKind.Wall).Ok);   // batı sınır
            Assert.True(s.SetEdge(new Edge(8, 5, EdgeAxis.Vertical), EdgeKind.Wall).Ok);   // doğu sınır
        }

        [Fact]
        public void Arsa_BitisikParcaSatinAlinir()
        {
            var (s, h, m) = Plot(100);
            Assert.True(s.BuyChunk(new GridPos(8, 5)).Ok);
            Assert.Equal(80, h.Akce);
            Assert.True(m.IsOwned(new GridPos(11, 7)));
            Assert.False(s.BuyChunk(new GridPos(8, 5)).Ok);
        }

        [Fact]
        public void Arsa_BitisikOlmayanParcaAlinmaz_AkceYetmezse_Reddeder()
        {
            var (s, h, _) = Plot(100);
            Assert.False(s.BuyChunk(new GridPos(12, 12)).Ok);
            Assert.Equal(100, h.Akce);
            var (s2, _, _) = Plot(5);
            Assert.False(s2.BuyChunk(new GridPos(8, 5)).Ok);
        }

        [Fact]
        public void Arsa_EsyaArsaDisinaKonmaz()
        {
            var map = new GridMap(16, 16);
            map.RestrictOwnership(4, 4, 7, 7);
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "bed", DisplayName = "Yatak", Width = 2, Price = 10 });
            var house = new Household(100);
            var s = new BuildService(new WorldState(map), house, db);
            s.Buy("bed");
            Assert.False(s.PlaceFromInventory("bed", new GridPos(7, 5), 0, out _).Ok);   // yarısı dışarıda
            Assert.True(s.PlaceFromInventory("bed", new GridPos(6, 5), 0, out _).Ok);
        }
    }
}
