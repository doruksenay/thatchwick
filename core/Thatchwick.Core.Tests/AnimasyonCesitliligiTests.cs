using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Animasyon çeşitliliği kod katmanı (Project belgesi 69): hâl, biçim, ara klip, koşu.</summary>
    public class AnimasyonCesitliligiTests
    {
        private static MoodletDef M(string id, int value, Hal? hal = null) =>
            new MoodletDef { Id = id, DisplayName = id, Value = value, DurationDays = 1, Hal = hal };

        private static Character Person(double age = 50)
        {
            var c = new Character(1, "Maud", new GridPos(0, 0)) { MoodEnabled = true };
            c.SetAge(age);
            return c;
        }

        // ---------- hâl ----------

        [Fact]
        public void Hal_IcSesYoksa_Sakin()
        {
            Assert.Equal(Hal.Calm, HalRules.Dominant(new MoodState()));
        }

        [Fact]
        public void Hal_EsigiGecmeyen_Sakin_GecenBaskin()
        {
            var m = new MoodState();
            m.Add(M(MoodCatalog.HadFun, 5));
            Assert.Equal(Hal.Calm, HalRules.Dominant(m));
            m.Add(M(MoodCatalog.NiceChat, 4));
            Assert.Equal(Hal.Cheerful, HalRules.Dominant(m));
        }

        [Fact]
        public void Hal_MutlakDegerToplanir_AcHircinYapar()
        {
            var m = new MoodState();
            m.Add(M(MoodCatalog.GoodMeal, 8));
            m.Add(M(MoodCatalog.Hungry, -12));
            Assert.Equal(Hal.Cross, HalRules.Dominant(m));
        }

        [Fact]
        public void Hal_EsitlikteEnYeniKazanir_VeVeriEslemeyiEzer()
        {
            var m = new MoodState();
            m.Add(M("a", 10, Hal.Proud));
            m.Add(M("b", 10, Hal.Sad));
            Assert.Equal(Hal.Sad, HalRules.Dominant(m));
            Assert.Equal(Hal.Eager, HalRules.DefaultFor(MoodCatalog.Inspired, 10));
            Assert.Equal(Hal.Peaceful, HalRules.DefaultFor("bilinmeyen", 3));
            Assert.Equal(Hal.Tense, HalRules.DefaultFor("bilinmeyen", -3));
        }

        // ---------- biçim ----------

        private static AnimInputs Base() => new AnimInputs
        {
            Hal = Hal.Calm, Stage = AgeStage.Adult, Energy = 80, Hunger = 80, Warmth = 80, Hygiene = 80, Temperature = 15, Outdoors = true
        };

        [Fact]
        public void Bicim_Sakin_BugunkuDavranis()
        {
            var s = AnimStyleRules.Compute(Base());
            Assert.Equal(1f, s.Tempo);
            Assert.Equal(Gait.Normal, s.Gait);
            Assert.Equal(0, s.RestEvery);
        }

        [Fact]
        public void Bicim_Kederli_AgirVeYavas_Sevkli_Sekerek()
        {
            var i = Base(); i.Hal = Hal.Sad;
            var sad = AnimStyleRules.Compute(i);
            Assert.Equal(Gait.Heavy, sad.Gait);
            Assert.True(sad.Tempo < 1f);
            i.Hal = Hal.Eager;
            var eager = AnimStyleRules.Compute(i);
            Assert.Equal(Gait.Cheerful, eager.Gait);
            Assert.True(eager.Tempo > 1f);
            Assert.Equal("walk_happy", AnimStyleRules.WalkKey(eager.Gait));
        }

        [Fact]
        public void Bicim_Tempo_HerBilesimdeSinirIcinde()
        {
            foreach (Hal h in System.Enum.GetValues(typeof(Hal)))
            foreach (AgeStage st in System.Enum.GetValues(typeof(AgeStage)))
            foreach (int skill in new[] { 0, 1, 5, 10 })
            foreach (float energy in new[] { 5f, 80f })
            {
                var i = Base(); i.Hal = h; i.Stage = st; i.SkillLevel = skill; i.Energy = energy; i.Pregnant = true;
                var s = AnimStyleRules.Compute(i);
                Assert.InRange(s.Tempo, AnimStyleRules.MinTempo, AnimStyleRules.MaxTempo);
                Assert.InRange(s.Irregularity, 0f, 0.6f);
            }
        }

        [Fact]
        public void Bicim_AcemiDuzensiz_UstaDuzgun_YorgunMolaVerir()
        {
            var i = Base(); i.SkillLevel = 2;
            float novice = AnimStyleRules.Compute(i).Irregularity;
            i.SkillLevel = 9;
            float master = AnimStyleRules.Compute(i).Irregularity;
            Assert.True(novice > master);
            i.Energy = 10;
            Assert.Equal(3, AnimStyleRules.Compute(i).RestEvery);
        }

        [Fact]
        public void Bicim_YaslidaDiz_Baston_CocukIckiliOlmaz_YaraTopallatir()
        {
            var i = Base(); i.Stage = AgeStage.Elder; i.KneeTrouble = true;
            Assert.Equal(Gait.Cane, AnimStyleRules.GaitFor(i));
            i = Base(); i.Stage = AgeStage.Child; i.Tipsy = true;
            Assert.NotEqual(Gait.Tipsy, AnimStyleRules.GaitFor(i));
            i = Base(); i.LegInjury = true;
            Assert.Equal(Gait.Limp, AnimStyleRules.GaitFor(i));
            Assert.True(AnimStyleRules.IsLegInjury("fall", null));
            Assert.False(AnimStyleRules.IsLegInjury("burn", "hand"));
        }

        [Fact]
        public void Bicim_Ter_SicakAgirIste_Nefes_Soguk()
        {
            var i = Base(); i.Temperature = 30; i.Work = WorkContext.Heavy;
            Assert.True(AnimStyleRules.Compute(i).Sweat);
            i.Outdoors = false;
            Assert.False(AnimStyleRules.Compute(i).Sweat);
            i = Base(); i.Temperature = 0;
            Assert.True(AnimStyleRules.Compute(i).Breath);
        }

        [Fact]
        public void Durma_HalAnahtari_CocukKipir_SakindeYok()
        {
            Assert.Null(AnimStyleRules.IdleKey(Hal.Calm, AgeStage.Adult));
            Assert.Equal("idle_sad", AnimStyleRules.IdleKey(Hal.Sad, AgeStage.Adult));
            Assert.Equal("idle_fidget", AnimStyleRules.IdleKey(Hal.Cheerful, AgeStage.Child));
            Assert.Null(AnimStyleRules.IdleKey(Hal.Sad, AgeStage.Baby));
        }

        // ---------- ara klip ----------

        [Fact]
        public void AraKlip_NedenYoksa_Oynamaz()
        {
            var p = new InterjectionPicker(1);
            Assert.Equal(Interjection.None, p.Pick(1, Base(), InterjectionMoment.Idle, false, false, 0));
            var list = new List<Interjection>();
            InterjectionPicker.Candidates(Base(), InterjectionMoment.Working, false, false, list);
            Assert.Empty(list);
        }

        [Fact]
        public void AraKlip_KosulaBagli()
        {
            var list = new List<Interjection>();
            var i = Base(); i.Temperature = 28; i.Work = WorkContext.Heavy;
            InterjectionPicker.Candidates(i, InterjectionMoment.Working, false, false, list);
            Assert.Contains(Interjection.WipeBrow, list);
            i = Base(); i.Hunger = 10;
            InterjectionPicker.Candidates(i, InterjectionMoment.Idle, false, false, list);
            Assert.Contains(Interjection.HoldBelly, list);
            i = Base(); i.Hal = Hal.Proud;
            InterjectionPicker.Candidates(i, InterjectionMoment.WorkDone, false, false, list);
            Assert.Contains(Interjection.SmallCheer, list);
            InterjectionPicker.Candidates(Base(), InterjectionMoment.Walking, true, false, list);
            Assert.Contains(Interjection.Wave, list);
        }

        [Fact]
        public void AraKlip_AyniKlipIkiDakikaBekler_EkrandaEnCokIki()
        {
            var p = new InterjectionPicker(2);
            var i = Base(); i.Hunger = 10;   // tek aday: karnını tutar
            Assert.Equal(Interjection.HoldBelly, p.Pick(1, i, InterjectionMoment.Idle, false, false, 0));
            Assert.Equal(Interjection.None, p.Pick(1, i, InterjectionMoment.Idle, false, false, 60));
            Assert.Equal(Interjection.HoldBelly, p.Pick(1, i, InterjectionMoment.Idle, false, false, 121));
            // Aynı anda: 2 ve 3 oynarken 4 bekler.
            Assert.NotEqual(Interjection.None, p.Pick(2, i, InterjectionMoment.Idle, false, false, 200));
            Assert.NotEqual(Interjection.None, p.Pick(3, i, InterjectionMoment.Idle, false, false, 200.1));
            Assert.Equal(Interjection.None, p.Pick(4, i, InterjectionMoment.Idle, false, false, 200.2));
            Assert.NotEqual(Interjection.None, p.Pick(4, i, InterjectionMoment.Idle, false, false, 202));
        }

        // ---------- koşu ----------

        private static double DaysToArrive(Character c, bool autonomous, Job job, GridPos target)
        {
            var world = new WorldState(new GridMap(40, 6));
            var r = new JobRunner(c, world) { Autonomous = autonomous };
            r.Command(job);
            double days = 0;
            for (int k = 0; k < 20000 && !c.Position.Equals(target); k++) { r.Tick(0.0005); days += 0.0005; }
            return days;
        }

        [Fact]
        public void Kosu_OyuncuEmriUzunYolda_Kosar_KisaYoldaYurur()
        {
            var target = new GridPos(30, 0);
            double run = DaysToArrive(Person(), false, Job.MoveTo(target), target);
            double walk = DaysToArrive(Person(), true, Job.MoveTo(target), target);
            Assert.True(run < walk * 0.75, $"koşu {run}, yürüyüş {walk}");

            var c = Person();
            var job = Job.MoveTo(new GridPos(5, 0));
            Assert.False(RunRules.ShouldRun(c, job, 5, true));
            Assert.True(RunRules.ShouldRun(c, job, RunRules.LongWalkTiles, true));
        }

        [Fact]
        public void Kosu_Acil_Kosar_YasliVeYorgunKosmaz()
        {
            var c = Person();
            var hurry = Job.MoveTo(new GridPos(3, 0)).AsHurry();
            Assert.True(RunRules.ShouldRun(c, hurry, 3, false));
            Assert.True(RunRules.ShouldRun(c, Job.Social(2, SocialAction.Midwife), 3, false));
            var elder = Person(); elder.SetAge(elder.AgeConfig.ElderStartDay + 1);
            Assert.False(RunRules.ShouldRun(elder, hurry, 3, false));
            c.Needs.Set(NeedType.Energy, 5f);
            Assert.False(RunRules.ShouldRun(c, hurry, 3, false));
        }

        [Fact]
        public void Kosu_CocukOyunda_Kosar()
        {
            var child = Person(); child.SetAge(child.AgeConfig.ChildStartDay + 0.1);
            Assert.Equal(AgeStage.Child, child.Stage);
            var play = Job.MoveTo(new GridPos(6, 0)).WithReason(NeedType.Fun);
            Assert.True(RunRules.ShouldRun(child, play, 6, false));
            Assert.False(RunRules.ShouldRun(child, play, 2, false));
        }
    }
}
