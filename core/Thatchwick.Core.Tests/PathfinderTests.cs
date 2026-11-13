using System;
using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class GridMapTests
    {
        [Fact]
        public void Sinirlar_VeKapaliKaro()
        {
            var map = new GridMap(4, 3);
            Assert.True(map.InBounds(new GridPos(3, 2)));
            Assert.False(map.InBounds(new GridPos(4, 0)));
            Assert.False(map.InBounds(new GridPos(-1, 0)));

            Assert.True(map.IsWalkable(new GridPos(1, 1)));
            map.SetBlocked(new GridPos(1, 1), true);
            Assert.True(map.IsBlocked(new GridPos(1, 1)));
            map.SetBlocked(new GridPos(1, 1), false);
            Assert.True(map.IsWalkable(new GridPos(1, 1)));
        }

        [Fact]
        public void IzgaraDisi_GecilemezVeAyarlanamaz()
        {
            var map = new GridMap(2, 2);
            Assert.True(map.IsBlocked(new GridPos(5, 5)));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.SetBlocked(new GridPos(5, 5), true));
        }

        [Fact]
        public void GecersizBoyut_Reddedilir()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridMap(0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridMap(3, -1));
        }
    }

    public class PathfinderTests
    {
        [Fact]
        public void BosIzgara_DuzYol()
        {
            var map = new GridMap(10, 10);
            var path = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(5, 0));
            Assert.NotNull(path);
            Assert.Equal(6, path!.Count);
            Assert.Equal(new GridPos(0, 0), path[0]);
            Assert.Equal(new GridPos(5, 0), path[^1]);
        }

        [Fact]
        public void Capraz_EnKisaYoluTercihEder()
        {
            var map = new GridMap(10, 10);
            var path = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(4, 4));
            Assert.NotNull(path);
            Assert.Equal(5, path!.Count); // 4 çapraz adım
        }

        [Fact]
        public void BaslangicHedefAyni_TekElemanliYol()
        {
            var map = new GridMap(3, 3);
            var path = Pathfinder.FindPath(map, new GridPos(1, 1), new GridPos(1, 1));
            Assert.NotNull(path);
            Assert.Single(path!);
        }

        [Fact]
        public void Duvar_EtrafindanDolasir()
        {
            var map = new GridMap(7, 5);
            for (int y = 0; y < 4; y++) map.SetBlocked(new GridPos(3, y), true);

            var path = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(6, 0));
            Assert.NotNull(path);
            foreach (var p in path!) Assert.True(map.IsWalkable(p));
            Assert.Contains(new GridPos(3, 4), path);
        }

        [Fact]
        public void TamKapaliHedef_YolYok()
        {
            var map = new GridMap(5, 5);
            for (int i = 0; i < 5; i++) map.SetBlocked(new GridPos(2, i), true);
            Assert.Null(Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(4, 4)));
        }

        [Fact]
        public void KapaliHedef_YolYok_KapaliBaslangictanCikilir()
        {
            var map = new GridMap(5, 5);
            map.SetBlocked(new GridPos(4, 4), true);
            Assert.Null(Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(4, 4)));
            // Kapalı karoda duran (üstüne eşya konmuş) karakter dışarı yürüyebilir; yoksa kalıcı olarak hapsolurdu.
            var escape = Pathfinder.FindPath(map, new GridPos(4, 4), new GridPos(0, 0));
            Assert.NotNull(escape);
            Assert.Equal(new GridPos(0, 0), escape![escape.Count - 1]);
            Assert.NotNull(Pathfinder.FindPathToAny(map, new GridPos(4, 4), new List<GridPos> { new GridPos(0, 0) }));
            Assert.Null(Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(9, 9)));
        }

        [Fact]
        public void Capraz_KoseKesmez()
        {
            // (1,0) kapalı: (0,0)->(1,1) çaprazı yalnızca (0,1) üzerinden gidebilir.
            var map = new GridMap(3, 3);
            map.SetBlocked(new GridPos(1, 0), true);
            var path = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(1, 1));
            Assert.NotNull(path);
            Assert.Equal(3, path!.Count);
            Assert.Equal(new GridPos(0, 1), path[1]);
        }

        [Fact]
        public void Sonuc_Belirleyici()
        {
            var map = new GridMap(12, 12);
            map.SetBlocked(new GridPos(5, 5), true);
            map.SetBlocked(new GridPos(5, 6), true);
            var a = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(11, 11));
            var b = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(11, 11));
            Assert.Equal(a, b);
        }
    
        [Fact]
        public void CokHedef_EnUcuzaGider()
        {
            var map = new GridMap(12, 12);
            var goals = new List<GridPos> { new GridPos(10, 0), new GridPos(3, 0), new GridPos(11, 11) };
            var path = Pathfinder.FindPathToAny(map, new GridPos(0, 0), goals);
            Assert.Equal(new GridPos(3, 0), path[path.Count - 1]);
            Assert.Equal(4, path.Count);
        }

        [Fact]
        public void CokHedef_BaslangicHedefse_TekElemanli_HicbiriYoksaNull()
        {
            var map = new GridMap(6, 6);
            var one = Pathfinder.FindPathToAny(map, new GridPos(2, 2), new List<GridPos> { new GridPos(2, 2), new GridPos(5, 5) });
            Assert.Single(one);
            Assert.Null(Pathfinder.FindPathToAny(map, new GridPos(0, 0), new List<GridPos>()));
            map.SetBlocked(new GridPos(4, 4), true);
            Assert.Null(Pathfinder.FindPathToAny(map, new GridPos(0, 0), new List<GridPos> { new GridPos(4, 4) }));
        }

        [Fact]
        public void CokHedef_TekHedefliAramaylaAyniMaliyet()
        {
            var map = new GridMap(14, 14);
            for (int y = 0; y < 12; y++) map.SetBlocked(new GridPos(6, y), true);
            var a = Pathfinder.FindPath(map, new GridPos(0, 0), new GridPos(12, 3));
            var b = Pathfinder.FindPathToAny(map, new GridPos(0, 0), new List<GridPos> { new GridPos(12, 3) });
            Assert.Equal(a, b);
        }
    }
}
