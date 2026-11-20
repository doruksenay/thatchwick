using System.Collections.Generic;
using Thatchwick.Core.Art;
using Thatchwick.Core.Collections;

namespace Thatchwick.Core.Tests
{
    public class AtlasVeLruTests
    {
        [Fact]
        public void Raf_CakismaYok_SayfaIcinde_DolguVar()
        {
            var rng = new System.Random(5);
            var sizes = new List<(int, int)>();
            for (int i = 0; i < 400; i++) sizes.Add((8 + rng.Next(120), 8 + rng.Next(120)));
            sizes.Add((3000, 10));   // sığmaz: atlas dışı
            var placed = ShelfPacker.Pack(sizes, 512, 2, out int pages);
            Assert.True(pages > 1);
            Assert.False(placed[400].Packed);
            for (int i = 0; i < 400; i++)
            {
                var a = placed[i]; var (aw, ah) = sizes[i];
                Assert.True(a.Packed);
                Assert.InRange(a.X, 2, 512 - aw - 2);
                Assert.InRange(a.Y, 2, 512 - ah - 2);
                for (int j = i + 1; j < 400; j++)
                {
                    var b = placed[j]; var (bw, bh) = sizes[j];
                    if (a.Page != b.Page) continue;
                    bool apart = a.X + aw + 2 <= b.X || b.X + bw + 2 <= a.X || a.Y + ah + 2 <= b.Y || b.Y + bh + 2 <= a.Y;
                    Assert.True(apart, $"{i} ve {j} çakışıyor");
                }
            }
        }

        [Fact]
        public void Raf_Belirleyici()
        {
            var sizes = new List<(int, int)> { (10, 20), (30, 5), (64, 64), (70, 10) };
            var a = ShelfPacker.Pack(sizes, 128, 1, out int pa);
            var b = ShelfPacker.Pack(sizes, 128, 1, out int pb);
            Assert.Equal(pa, pb);
            for (int i = 0; i < sizes.Count; i++) { Assert.Equal(a[i].X, b[i].X); Assert.Equal(a[i].Y, b[i].Y); Assert.Equal(a[i].Page, b[i].Page); }
        }

        [Fact]
        public void Lru_YalnizcaEnEskiAtilir()
        {
            var evicted = new List<string>();
            var lru = new LruCache<string, int>(3, (k, v) => evicted.Add(k));
            lru.Set("a", 1); lru.Set("b", 2); lru.Set("c", 3);
            Assert.True(lru.TryGet("a", out _));   // a tazelendi
            lru.Set("d", 4);                         // en eski: b
            Assert.Equal(new[] { "b" }, evicted.ToArray());
            Assert.False(lru.TryGet("b", out _));
            Assert.True(lru.TryGet("c", out int c) && c == 3);
            Assert.Equal(3, lru.Count);
            lru.Clear();
            Assert.Equal(0, lru.Count);
            Assert.Equal(4, evicted.Count);
        }

        [Fact]
        public void Lru_AyniAnahtar_EskiDegerBildirilir()
        {
            var evicted = new List<int>();
            var lru = new LruCache<string, int>(2, (k, v) => evicted.Add(v));
            lru.Set("a", 1);
            lru.Set("a", 2);
            Assert.Equal(new[] { 1 }, evicted.ToArray());
            Assert.True(lru.TryGet("a", out int v) && v == 2);
        }
    }
}
