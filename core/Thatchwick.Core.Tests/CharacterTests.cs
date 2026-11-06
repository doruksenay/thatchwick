using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class NeedSetTests
    {
        [Fact]
        public void YeniKarakter_TamDoygunBaslar()
        {
            var needs = new NeedSet();
            foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
                Assert.Equal(100f, needs.Get(n));
        }

        [Fact]
        public void Tick_OyunGunuUzerindenAzaltir()
        {
            var needs = new NeedSet();
            needs.SetDecayPerDay(NeedType.Hunger, 50f);
            needs.Tick(0.5);
            Assert.Equal(75.0, (double)needs.Get(NeedType.Hunger), 3);
        }

        [Fact]
        public void Deger_SifirinAltinaInmez_YuzuAsmaz()
        {
            var needs = new NeedSet();
            needs.Tick(10);
            Assert.Equal(0f, needs.Get(NeedType.Hunger));
            needs.Restore(NeedType.Hunger, 500f);
            Assert.Equal(100f, needs.Get(NeedType.Hunger));
        }

        [Fact]
        public void KritikEsik_VeAnyCritical()
        {
            var needs = new NeedSet();
            Assert.False(needs.AnyCritical());
            needs.Set(NeedType.Energy, NeedSet.CriticalThreshold);
            Assert.True(needs.IsCritical(NeedType.Energy));
            Assert.True(needs.AnyCritical());
            Assert.False(needs.IsCritical(NeedType.Hunger));
        }

        [Fact]
        public void GecersizGirdi_Reddedilir()
        {
            var needs = new NeedSet();
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.Tick(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.Restore(NeedType.Hunger, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.SetDecayPerDay(NeedType.Hunger, -1));
        }

        [Fact]
        public void HizBiyolojiyiDegistirmez_OyunGunuAyniAzalirKritikAnDahil()
        {
            // T3: 1x'te 2 oyun günü ile 3x'te 2 oyun günü aynı azalmayı verir.
            var slow = new GameClock();
            var fast = new GameClock();
            fast.SetSpeed(3);
            var a = new NeedSet();
            var b = new NeedSet();

            a.Tick(slow.Advance(720));          // 2 gün, 1x
            b.Tick(fast.Advance(240));          // 240 sn * 3 / 360 = 2 gün
            Assert.Equal((double)a.Get(NeedType.Hunger), (double)b.Get(NeedType.Hunger), 3);
        }
    }

    public class CharacterTests
    {
        [Fact]
        public void OyuncuVeNpc_AyniTip_FarkBayrak()
        {
            var player = new Character(1, "Aldric", new GridPos(2, 2), isPlayerControlled: true);
            var npc = new Character(2, "Maud", new GridPos(3, 2));
            Assert.True(player.IsPlayerControlled);
            Assert.False(npc.IsPlayerControlled);
            Assert.Equal(player.GetType(), npc.GetType());
        }

        [Fact]
        public void Tick_IhtiyaclariAzaltir()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.Tick(0.25);
            Assert.True(c.Needs.Get(NeedType.Hunger) < 100f);
        }

        [Fact]
        public void BosIsim_Reddedilir()
        {
            Assert.Throws<ArgumentException>(() => new Character(1, " ", new GridPos(0, 0)));
        }
    }
}

namespace Thatchwick.Core.Tests
{
    public class AgeTests
    {
        [Theory]
        [InlineData(0, AgeStage.Baby)]
        [InlineData(11.99, AgeStage.Baby)]
        [InlineData(12, AgeStage.Child)]
        [InlineData(24, AgeStage.Youth)]
        [InlineData(36, AgeStage.Adult)]
        [InlineData(71.99, AgeStage.Adult)]
        [InlineData(72, AgeStage.Elder)]
        [InlineData(143, AgeStage.Elder)]
        public void EvreSiniri_Dogru(double age, AgeStage expected)
        {
            Assert.Equal(expected, AgeConfig.Default.StageAt(age));
        }

        [Fact]
        public void Gecersiz_Sinirlar_Reddedilir()
        {
            Assert.Throws<ArgumentException>(() => new AgeConfig(childStart: 30, youthStart: 24));
            Assert.Throws<ArgumentException>(() => new AgeConfig(elderStart: 72, earliestDeath: 60));
            Assert.Throws<ArgumentException>(() => new AgeConfig(earliestDeath: 96, maxLifespan: 90));
        }

        [Fact]
        public void RollLifespan_AraliktaKalir()
        {
            var cfg = AgeConfig.Default;
            var rng = new Random(1);
            for (int i = 0; i < 1000; i++)
            {
                double l = cfg.RollLifespan(rng);
                Assert.InRange(l, 96.0, 144.0);
            }
        }

        [Fact]
        public void Tick_YasIlerler_EvreDegisince_OlayTetiklenir()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            var changes = new System.Collections.Generic.List<(AgeStage, AgeStage)>();
            c.StageChanged += (_, from, to) => changes.Add((from, to));

            c.Tick(11.0);
            Assert.Empty(changes);
            c.Tick(1.5);
            Assert.Single(changes);
            Assert.Equal((AgeStage.Baby, AgeStage.Child), changes[0]);
            Assert.Equal(AgeStage.Child, c.Stage);
        }

        [Fact]
        public void BuyukAdim_BirdenFazlaEvreAtlayabilir_TekOlayVerir()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            int count = 0;
            c.StageChanged += (_, __, ___) => count++;
            c.Tick(40.0); // Bebek -> Yetişkin
            Assert.Equal(AgeStage.Adult, c.Stage);
            Assert.Equal(1, count);
        }

        [Fact]
        public void YaslilikBaslar_AmaErkenOlumYok_96ya_Kadar()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.LifespanDays = 96;
            c.SetAge(71);
            c.Tick(24.9);  // 95.9
            Assert.Equal(AgeStage.Elder, c.Stage);
            Assert.False(c.IsDead);
            c.Tick(0.2);   // 96.1
            Assert.True(c.IsDead);
        }

        [Fact]
        public void Olum_BirKezTetiklenir_OluKarakterIlerlemez()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.LifespanDays = 100;
            int died = 0;
            c.Died += _ => died++;
            c.Tick(150);
            c.Tick(10);
            Assert.Equal(1, died);
            Assert.True(c.IsDead);
            Assert.Equal(150.0, c.AgeDays, 6);
        }

        [Fact]
        public void VarsayilanOmur_UstSinir144()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.Tick(143.9);
            Assert.False(c.IsDead);
            c.Tick(0.2);
            Assert.True(c.IsDead);
        }
    }
}
