using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Yerleşim doğrulayıcısı, Aşama A kuralları: çakışma/sınır ve kapı önü. Her kural için geçen ve kalan örnek.</summary>
    public class YerlesimDogrulayiciTests
    {
        internal static ItemDef Def(string id, int w = 1, int h = 1) => new ItemDef { Id = id, DisplayName = id, Width = w, Height = h };

        /// <summary>İç 3-7 x 3-6 oda; güney duvarında (y=7 kenarı) x=5'te kapı. Dışı açık çim.</summary>
        internal static WorldState Room(int w = 14, int h = 14)
        {
            var m = new GridMap(w, h);
            for (int x = 3; x <= 7; x++)
            {
                m.SetEdge(new Edge(x, 3, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, 7, EdgeAxis.Horizontal), x == 5 ? EdgeKind.Door : EdgeKind.Wall);
            }
            for (int y = 3; y <= 6; y++)
            {
                m.SetEdge(new Edge(3, y, EdgeAxis.Vertical), EdgeKind.Wall);
                m.SetEdge(new Edge(8, y, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            return new WorldState(m);
        }

        [Fact]
        public void KapiOnu_BosOda_IhlalYok()
        {
            var w = Room();
            w.PlaceItem(Def("sandik"), new GridPos(3, 3));
            Assert.DoesNotContain(PlacementValidator.Run(w), v => v.Code == Rules.DoorFront);
        }

        [Fact]
        public void KapiOnu_IcKaroDolu_Ihlal()
        {
            var w = Room();
            w.PlaceItem(Def("sandik"), new GridPos(5, 6));   // kapının iç karosu
            var v = Assert.Single(PlacementValidator.Run(w), x => x.Code == Rules.DoorFront);
            Assert.Equal(new GridPos(5, 6), v.At);
        }

        [Fact]
        public void KapiOnu_DisariIkiKaro_IkinciKaroDolu_Ihlal()
        {
            var w = Room();
            w.PlaceItem(Def("kuyu"), new GridPos(5, 8));   // dışarıda ikinci karo (ilk karo 5,7)
            Assert.Contains(PlacementValidator.Run(w), v => v.Code == Rules.DoorFront && v.ItemDefId == "kuyu");
        }

        [Fact]
        public void KapiOnu_DisariUcuncuKaro_Serbest()
        {
            var w = Room();
            w.PlaceItem(Def("kuyu"), new GridPos(5, 9));
            Assert.DoesNotContain(PlacementValidator.Run(w), v => v.Code == Rules.DoorFront);
        }

        [Fact]
        public void KapiOnu_YanKaroSerbest()
        {
            var w = Room();
            w.PlaceItem(Def("fici"), new GridPos(6, 7));   // kapının çaprazında, önünde değil
            Assert.DoesNotContain(PlacementValidator.Run(w), v => v.Code == Rules.DoorFront);
        }

        [Fact]
        public void Sinir_DuvariAsanUzunEsya_Ihlal()
        {
            var w = Room();
            w.PlaceItem(Def("masa", 2, 1), new GridPos(7, 4));   // 7,4 içeride, 8,4 duvarın öbür yanında
            Assert.Contains(PlacementValidator.Run(w), v => v.Code == Rules.Bounds);
        }

        [Fact]
        public void Sinir_IcerideUzunEsya_Gecer()
        {
            var w = Room();
            w.PlaceItem(Def("masa", 2, 1), new GridPos(4, 4));
            Assert.DoesNotContain(PlacementValidator.Run(w), v => v.Code == Rules.Bounds || v.Code == Rules.Overlap);
        }

        [Fact]
        public void Sinir_PlanliArsaDisinaTasma_Ihlal()
        {
            var w = Room();
            var plan = new LayoutPlan();
            plan.Sites.Add(new Site { Id = "a", Name = "arsa", Building = new RectI(3, 3, 5, 4), Plot = new RectI(2, 2, 7, 9) });
            w.PlaceItem(Def("bank", 2, 1), new GridPos(8, 9));   // 8,9 arsada, 9,9 dışında
            Assert.Contains(PlacementValidator.Run(w, plan), v => v.Code == Rules.Bounds);
        }

        [Fact]
        public void Kapilar_IcVeDisYuzuBulunur()
        {
            var w = Room();
            var d = Assert.Single(PlacementValidator.Doors(w.Map));
            Assert.Equal(new GridPos(5, 6), d.Inside);
            Assert.Equal(new GridPos(5, 7), d.Outside);
            Assert.Equal(Dir4.S, d.OutDir);
            Assert.True(d.Exterior);
        }

        [Fact]
        public void OyuncuKipi_KapiOnuUyari_CakismaHata()
        {
            var w = Room();
            w.PlaceItem(Def("sandik"), new GridPos(5, 6));
            var v = new PlacementValidator(w) { PlayerMode = true }.Validate();
            Assert.All(v.Where(x => x.Code == Rules.DoorFront), x => Assert.Equal(Severity.Warning, x.Severity));
        }

        [Fact]
        public void Ulasilabilirlik_DuvarGecilmez_KapidanGirilir()
        {
            var w = Room();
            var r = Reach.From(w.Map, new GridPos(5, 10));
            Assert.Contains(new GridPos(4, 4), r);            // kapıdan içeri
            w.Map.SetEdge(new Edge(5, 7, EdgeAxis.Horizontal), EdgeKind.Wall);
            r = Reach.From(w.Map, new GridPos(5, 10));
            Assert.DoesNotContain(new GridPos(4, 4), r);
        }
    }
}
