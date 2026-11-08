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

namespace Thatchwick.Core.Tests
{
    public class PregnancyTests
    {
        private static Character Woman(double age = 40)
        {
            var c = new Character(1, "Maud", new GridPos(0, 0)) { Sex = Sex.Female };
            c.SetAge(age);
            return c;
        }

        [Fact]
        public void Yetiskin_Kadin_HamileKalabilir()
        {
            var c = Woman();
            Assert.True(c.TryStartPregnancy(2));
            Assert.True(c.IsPregnant);
            Assert.Equal(2, c.FatherId);
        }

        [Fact]
        public void Erkek_Cocuk_Yasli_Olu_HamileKalamaz()
        {
            var man = new Character(2, "Aldric", new GridPos(0, 0)) { Sex = Sex.Male };
            man.SetAge(40);
            Assert.False(man.TryStartPregnancy(1));

            Assert.False(Woman(13).TryStartPregnancy(1));  // çocuk
            Assert.False(Woman(80).TryStartPregnancy(1));  // yaşlı

            var dead = Woman();
            dead.LifespanDays = 41;
            dead.Tick(5);
            Assert.False(dead.TryStartPregnancy(1));
        }

        [Fact]
        public void IkinciKezHamileKalamaz_AynıAndaCiftHamilelikYok()
        {
            var c = Woman();
            Assert.True(c.TryStartPregnancy(2));
            Assert.False(c.TryStartPregnancy(3));
            Assert.Equal(2, c.FatherId);
        }

        [Fact]
        public void SureDolunca_DogumBirKezOlur_SonraDogumSonrasiBeklenir()
        {
            var c = Woman();
            c.PregnancyDays = 5;
            c.PostpartumDays = 3;
            int births = 0;
            c.GaveBirth += (_, __) => births++;

            c.TryStartPregnancy(2);
            c.Tick(4.9);
            Assert.Equal(0, births);
            c.Tick(0.2);
            Assert.Equal(1, births);
            Assert.False(c.IsPregnant);

            Assert.False(c.TryStartPregnancy(2)); // doğum sonrası bekleme
            c.Tick(3.1);
            Assert.True(c.TryStartPregnancy(2));
        }

        [Fact]
        public void AnneOlurse_DogumOlmaz()
        {
            var c = Woman(40);
            c.LifespanDays = 42;
            c.PregnancyDays = 10;
            int births = 0;
            c.GaveBirth += (_, __) => births++;
            c.TryStartPregnancy(2);
            c.Tick(5);   // yaş 45 > 42: öldü
            c.Tick(10);
            Assert.True(c.IsDead);
            Assert.Equal(0, births);
            Assert.False(c.IsPregnant);
        }

        [Fact]
        public void Nufus_DogumdaBebekUretir_BebekBebekEvresindeVeFarkliKimlikte()
        {
            var pop = new Population(firstFreeId: 1, seed: 3);
            var mother = Woman();
            mother.PregnancyDays = 2;
            var father = new Character(2, "Aldric", new GridPos(1, 1)) { Sex = Sex.Male };
            father.SetAge(40);
            pop.Add(mother);
            pop.Add(father);

            Character born = null;
            int? fatherSeen = null;
            pop.BabyBorn += (b, m, f) => { born = b; fatherSeen = f; };

            mother.TryStartPregnancy(father.Id);
            pop.Tick(2.5);

            Assert.NotNull(born);
            Assert.Equal(father.Id, fatherSeen);
            Assert.Equal(AgeStage.Baby, born.Stage);
            Assert.Equal(3, born.Id);
            Assert.Contains(born, pop.Members);
            Assert.InRange(born.LifespanDays, 96.0, 144.0);
            Assert.Equal(mother.Position, born.Position);
        }

        [Fact]
        public void Nufus_BebekZamanlaBuyur()
        {
            var pop = new Population();
            var mother = Woman();
            mother.PregnancyDays = 1;
            pop.Add(mother);
            Character baby = null;
            pop.BabyBorn += (b, _, __) => baby = b;
            mother.TryStartPregnancy(9);
            pop.Tick(1.1);
            Assert.NotNull(baby);
            pop.Tick(12.0);
            Assert.Equal(AgeStage.Child, baby.Stage);
        }
    }
}
