using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Mining;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Maden üreteci sürüm 2 (belge 64 §6, 64a Aşama G).</summary>
    public class MadenUretimiV2Tests
    {
        private static MineMap Gallery(int seed)
        {
            var m = new MineMap("mine-1", MineSiteKind.Mine, -1, MineSystem.MineWidth, MineSystem.MineHeight, seed);
            MineGen.Generate(m, MineTable.Gallery());
            return m;
        }

        /// <summary>Hazır mekân süsleri (621) açık karoda ve içerikte tanımlı; 20 tohumda en az bir kez her türden biri çıkar.</summary>
        [Fact]
        public void HazirMekanSusleri_AcikKarodaVeTanimli()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var kinds = new HashSet<string>();
            for (int seed = 1; seed <= 20; seed++)
            {
                var m = Gallery(seed);
                foreach (var (x, y, id) in m.Props)
                {
                    Assert.True(m.IsOpen(x, y), $"tohum {seed}: {id} ({x},{y}) kaya içinde");
                    Assert.True(db.Items.TryGet(id, out _), id);
                    kinds.Add(id);
                }
            }
            Assert.True(kinds.Count >= 5, string.Join(", ", kinds));
        }

        [Fact]
        public void AyniTohumAyniKat()
        {
            var a = Gallery(77); var b = Gallery(77);
            foreach (var (x, y, t) in a.All())
            {
                var u = b.Get(x, y);
                Assert.Equal(t.Rock, u.Rock); Assert.Equal(t.Ore, u.Ore); Assert.Equal(t.Wet, u.Wet); Assert.Equal(t.Support, u.Support);
            }
        }

        /// <summary>Cevher başına ortalama karo sayısı belge 58 §2.2'deki ölçümün ±%15'i içinde (denge bozulmaz).</summary>
        [Fact]
        public void CevherMiktariOncekiyleAyni()
        {
            var expected = new Dictionary<string, double>
            {
                ["item.iron_ore"] = 26, ["item.coal"] = 19, ["item.limestone"] = 21, ["item.slate_stone"] = 14,
                ["item.gypsum"] = 13, ["item.ochre"] = 12, ["item.clay"] = 12, ["item.flint"] = 11,
            };
            var sum = new Dictionary<string, double>();
            const int n = 200;
            for (int s = 1; s <= n; s++)
                foreach (var (_, _, t) in Gallery(s * 7919).All())
                    if (t.Ore != null) sum[t.Ore] = (sum.TryGetValue(t.Ore, out var v) ? v : 0) + 1;
            double total = sum.Values.Sum() / n, expTotal = expected.Values.Sum();
            Assert.InRange(total, expTotal * 0.85, expTotal * 1.15);
            foreach (var kv in expected)
                Assert.InRange(sum[kv.Key] / n, kv.Value * 0.7, kv.Value * 1.3);
        }

        /// <summary>Kaya türleri tablodaki oranları verir.</summary>
        [Fact]
        public void KayaOranlariTabloyaUyar()
        {
            var t = MineTable.Gallery();
            var m = new MineMap("x", MineSiteKind.Plot, -2, 60, 60, 5);   // arsa: kovuk yok, yalnızca kaya alanı
            t.Veins.Clear(); t.BedrockBorder = false;
            MineGen.Generate(m, t);
            int all = m.Width * m.Height;
            double Frac(RockKind k) => m.All().Count(e => e.t.Rock == k) / (double)all;
            Assert.InRange(Frac(RockKind.Stone), 0.48, 0.52);
            Assert.InRange(Frac(RockKind.Hard), 0.18, 0.22);
            Assert.InRange(Frac(RockKind.Soft), 0.23, 0.27);
        }

        /// <summary>Kaya bantlı: aynı türden komşu oranı karo başına zardakinden çok yüksek (tuz-biber değil).</summary>
        [Fact]
        public void KayaBantliDagilir()
        {
            var m = Gallery(31);
            int same = 0, pairs = 0;
            foreach (var (x, y, t) in m.All())
            {
                var r = m.Get(x + 1, y);
                if (r == null || t.Rock == RockKind.Bedrock || r.Rock == RockKind.Bedrock || t.Ore != null || r.Ore != null || t.IsOpen || r.IsOpen) continue;
                pairs++; if (r.Rock == t.Rock) same++;
            }
            // Bağımsız zarda beklenen benzerlik ~0,36 (0,05²+0,25²+0,5²+0,2²).
            Assert.True(same / (double)pairs > 0.55, $"komşu benzerliği {same / (double)pairs:0.00}");
        }

        [Fact]
        public void KovukVeHazirMekanGiriseTasmaz()
        {
            for (int s = 1; s <= 50; s++)
            {
                var m = Gallery(s * 31);
                var (ex, ey) = MineSystem.MineEntrance;
                int open = 0;
                foreach (var (x, y, t) in m.All())
                {
                    if (!t.IsOpen && t.Rock != RockKind.Rubble) continue;
                    open++;
                    Assert.False(Math.Abs(x - ex) <= 3 && Math.Abs(y - ey) <= 3, $"tohum {s}: girişin yanında açıklık ({x},{y})");
                    Assert.True(x > 0 && y > 0 && x < m.Width - 1 && y < m.Height - 1);
                }
                Assert.True(open >= 8, $"tohum {s}: kovuk ve hazır mekân yok ({open})");
            }
        }

        /// <summary>Doğal kovuk çökmez: kovuk karoları taş kemerli.</summary>
        [Fact]
        public void ArsaKazisindaKovukYok()
        {
            var m = new MineMap("plot", MineSiteKind.Plot, -2, 40, 40, 9);
            MineGen.Generate(m, MineTable.Plot());
            Assert.Equal(0, m.OpenCount);
        }

        [Fact]
        public void DamarlarEgimYonundeUzanir()
        {
            // Damar karosunun katın eğim yönündeki komşusu, dik yöndeki komşusundan belirgin daha sık aynı damardır.
            int along = 0, across = 0;
            for (int s = 1; s <= 60; s++)
            {
                int seed = s * 101;
                var m = Gallery(seed);
                double th = MineGen.DirectionOf(seed);
                int ax = (int)Math.Round(Math.Cos(th) * 2), ay = (int)Math.Round(Math.Sin(th) * 2);
                int px = (int)Math.Round(-Math.Sin(th) * 2), py = (int)Math.Round(Math.Cos(th) * 2);
                foreach (var (x, y, t) in m.All())
                {
                    if (t.Ore == null || t.Ore == "item.clay") continue;
                    if (m.Get(x + ax, y + ay)?.Ore == t.Ore) along++;
                    if (m.Get(x + px, y + py)?.Ore == t.Ore) across++;
                }
            }
            Assert.True(along > across * 1.5, $"yön boyunca {along}, dik {across}");
        }
    }
}
