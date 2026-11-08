using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class WallTests
    {
        private static GridMap Map() => new GridMap(8, 8);

        [Fact]
        public void KenarSinirlari()
        {
            var m = Map();
            Assert.True(m.EdgeInBounds(new Edge(7, 8, EdgeAxis.Horizontal)));
            Assert.False(m.EdgeInBounds(new Edge(8, 0, EdgeAxis.Horizontal)));
            Assert.True(m.EdgeInBounds(new Edge(8, 7, EdgeAxis.Vertical)));
            Assert.False(m.EdgeInBounds(new Edge(0, 8, EdgeAxis.Vertical)));
        }

        [Fact]
        public void EdgeBetween_IkiYandanAyniKenar()
        {
            var a = new GridPos(3, 3);
            Assert.Equal(GridMap.EdgeBetween(a, new GridPos(4, 3)), GridMap.EdgeBetween(new GridPos(4, 3), a));
            Assert.Equal(GridMap.EdgeBetween(a, new GridPos(3, 4)), GridMap.EdgeBetween(new GridPos(3, 4), a));
            var e = GridMap.EdgeBetween(a, new GridPos(4, 3));
            Assert.Equal(a, e.SideA);
            Assert.Equal(new GridPos(4, 3), e.SideB);
        }

        [Fact]
        public void Duvar_GecisiKapatir_KapiAcar()
        {
            var m = Map();
            var e = GridMap.EdgeBetween(new GridPos(2, 2), new GridPos(3, 2));
            m.SetEdge(e, EdgeKind.Wall);
            Assert.False(m.CanStep(new GridPos(2, 2), new GridPos(3, 2)));
            Assert.False(m.CanStep(new GridPos(3, 2), new GridPos(2, 2)));
            m.SetEdge(e, EdgeKind.Door);
            Assert.True(m.CanStep(new GridPos(2, 2), new GridPos(3, 2)));
            m.SetEdge(e, EdgeKind.Window);
            Assert.False(m.CanStep(new GridPos(2, 2), new GridPos(3, 2)));
        }

        [Fact]
        public void Capraz_DuvarKosesindenGecmez()
        {
            var m = Map();
            m.SetEdge(GridMap.EdgeBetween(new GridPos(2, 2), new GridPos(3, 2)), EdgeKind.Wall);
            Assert.False(m.CanStep(new GridPos(2, 2), new GridPos(3, 3)));
            Assert.True(m.CanStep(new GridPos(4, 4), new GridPos(5, 5)));
        }

        [Fact]
        public void YolBulma_DuvarinCevresindenGider_KapaliOdayaGirmez()
        {
            var m = Map();
            // (3..5, 3..5) odası: çevresi duvar
            for (int i = 3; i <= 5; i++)
            {
                m.SetEdge(new Edge(i, 3, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(i, 6, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(3, i, EdgeAxis.Vertical), EdgeKind.Wall);
                m.SetEdge(new Edge(6, i, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            Assert.Null(Pathfinder.FindPath(m, new GridPos(0, 0), new GridPos(4, 4)));
            m.SetEdge(new Edge(3, 4, EdgeAxis.Vertical), EdgeKind.Door);
            var path = Pathfinder.FindPath(m, new GridPos(0, 0), new GridPos(4, 4));
            Assert.NotNull(path);
            Assert.Contains(new GridPos(3, 4), path);
        }

        [Fact]
        public void Zemin_DegisinceChangedTetiklenir()
        {
            var m = Map();
            int n = 0;
            m.Changed += () => n++;
            m.SetFloor(new GridPos(1, 1), FloorType.Wood);
            m.SetFloor(new GridPos(1, 1), FloorType.Wood);
            Assert.Equal(1, n);
            Assert.Equal(FloorType.Wood, m.GetFloor(new GridPos(1, 1)));
        }

        private static (BuildService svc, Household house, GridMap map) Setup(int akce)
        {
            var map = Map();
            var house = new Household(akce);
            return (new BuildService(new WorldState(map), house, new ContentDatabase()), house, map);
        }

        [Fact]
        public void DuvarKur_AkceDuser_KaldirinceYarisiGeri()
        {
            var (s, h, m) = Setup(10);
            var e = new Edge(2, 2, EdgeAxis.Horizontal);
            Assert.True(s.SetEdge(e, EdgeKind.Wall).Ok);
            Assert.Equal(7, h.Akce);
            Assert.True(s.SetEdge(e, EdgeKind.None).Ok);
            Assert.Equal(8, h.Akce);
            Assert.Equal(EdgeKind.None, m.GetEdge(e));
        }

        [Fact]
        public void KapiPencere_OnceDuvarIster()
        {
            var (s, _, _) = Setup(50);
            var e = new Edge(2, 2, EdgeAxis.Vertical);
            Assert.False(s.SetEdge(e, EdgeKind.Door).Ok);
            s.SetEdge(e, EdgeKind.Wall);
            Assert.True(s.SetEdge(e, EdgeKind.Door).Ok);
        }

        [Fact]
        public void Duvar_AkceYetmezse_Reddeder()
        {
            var (s, h, m) = Setup(2);
            Assert.False(s.SetEdge(new Edge(1, 1, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
            Assert.Equal(2, h.Akce);
            Assert.Equal(EdgeKind.None, m.GetEdge(new Edge(1, 1, EdgeAxis.Horizontal)));
        }

        [Fact]
        public void Zemin_DosemeVeKaldirma()
        {
            var (s, h, m) = Setup(10);
            Assert.True(s.PaintFloor(new GridPos(1, 1), FloorType.Wood).Ok);
            Assert.Equal(8, h.Akce);
            Assert.False(s.PaintFloor(new GridPos(1, 1), FloorType.Wood).Ok);
            Assert.True(s.PaintFloor(new GridPos(1, 1), FloorType.Grass).Ok);
            Assert.Equal(9, h.Akce);
            Assert.False(s.PaintFloor(new GridPos(20, 1), FloorType.Wood).Ok);
        }
    }
}
