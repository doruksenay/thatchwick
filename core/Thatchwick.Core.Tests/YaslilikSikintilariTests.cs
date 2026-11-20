using System.Linq;
using Thatchwick.Core.Elders;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Yaşlılık sıkıntıları, Zihin dinçliği, kahve ve çaylar (belge 51 §3, Parti 4-5).</summary>
    public class YaslilikSikintilariTests
    {
        private static Character Elder(int id = 1, double age = 75)
        {
            var c = new Character(id, "Y" + id, new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(age);
            return c;
        }

        private static void Activate(ElderAilmentService a, Character c, AgingCondition cond)
        {
            a.OnBecameElder(c, 0);
            var s = a.StateOf(c);
            s.Pending.Clear();
            s.Active.Add(cond);
        }

        [Fact]
        public void Sira_NormalIkiHafifBir_KapaliYok_UcuncuOnIkinciGunde()
        {
            var a = new ElderAilmentService(5);
            var c = Elder();
            a.OnBecameElder(c, 100);
            var s = a.StateOf(c);
            Assert.Equal(2, s.Pending.Count);
            Assert.All(s.Pending, p => Assert.NotEqual(AgingCondition.Tremor, p.cond));
            Assert.All(s.Pending, p => Assert.NotEqual(AgingCondition.Absent, p.cond));
            Assert.InRange(s.Pending[0].day, 102, 108);
            Assert.InRange(s.Pending[1].day, 106, 114);
            for (int d = 100; d <= 125; d++) a.Daily(c, d, false, false, false);
            Assert.True(s.ThirdRolled);
            Assert.InRange(s.Active.Count, 2, 3);
            var mild = new ElderAilmentService(5) { Setting = AgingSetting.Mild };
            var m = Elder(2);
            mild.OnBecameElder(m, 0);
            Assert.Single(mild.StateOf(m).Pending);
            for (int d = 0; d <= 30; d++) mild.Daily(m, d, false, false, false);
            Assert.Single(mild.StateOf(m).Active);   // Hafif: 3. sıkıntı yok
            var off = new ElderAilmentService { Setting = AgingSetting.Off };
            off.OnBecameElder(Elder(3), 0);
            Assert.Null(off.StateOf(Elder(3)));
        }

        [Fact]
        public void Goz_GozlukSiler_OkumaTasiKismi_UykudaCikar()
        {
            var a = new ElderAilmentService();
            var c = Elder();
            Activate(a, c, AgingCondition.Eyes);
            Assert.Equal(0.7, a.WorkSpeed(c, SkillType.Tailoring, false, 1), 6);
            Assert.Equal(1.0, a.WorkSpeed(c, SkillType.Farming, false, 1), 6);
            Assert.Equal(0.5, a.XpFactor(c, SkillType.Literacy, 1), 6);
            Assert.Equal(1, a.QualityPenalty(c, SkillType.Tailoring));
            a.EquipAid(c, AgingAids.ReadingStone);
            Assert.Equal(0.9, a.WorkSpeed(c, SkillType.Literacy, false, 1), 6);
            Assert.Equal(0.7, a.WorkSpeed(c, SkillType.Tailoring, false, 1), 6);   // taş yalnız okuma ve yazmada
            a.EquipAid(c, AgingAids.GlassesBrass);
            Assert.Equal(1.0, a.WorkSpeed(c, SkillType.Tailoring, false, 1), 6);
            Assert.Equal(0, a.QualityPenalty(c, SkillType.Tailoring));
            c.Asleep = true;
            Assert.False(a.Glasses(c));
            c.Asleep = false;
            a.RemoveAid(c, AgingAids.GlassesBrass);
            a.RemoveAid(c, AgingAids.ReadingStone);
            Assert.Equal(0.7, a.WorkSpeed(c, SkillType.Tailoring, false, 1), 6);   // çıkarınca geri gelir
        }

        [Fact]
        public void Diz_BastonKolunaGir_Tavan40()
        {
            var a = new ElderAilmentService();
            var c = Elder();
            Activate(a, c, AgingCondition.Knees);
            Assert.Equal(0.7, a.WalkFactor(c, 1), 6);
            a.EquipAid(c, AgingAids.Cane);
            Assert.Equal(0.9, a.WalkFactor(c, 1), 6);
            a.RemoveAid(c, AgingAids.Cane);
            a.ArmInArm(c, 1);
            Assert.Equal(0.85, a.WalkFactor(c, 1), 6);
            Assert.Equal(0.7, a.WalkFactor(c, 2), 6);   // ertesi gün geçer
            Assert.Equal(0.5, a.StairFactor(c, false, 2), 6);
            Assert.Equal(0.75, a.StairFactor(c, true, 2), 6);
            // aynı işte iki sıkıntı en çok −%40
            a.StateOf(c).Active.Add(AgingCondition.Eyes);
            a.StateOf(c).Active.Add(AgingCondition.Joints);
            Assert.Equal(0.6, a.WorkSpeed(c, SkillType.Tailoring, true, 3), 6);
            a.Tincture(c, 3);
            Assert.Equal(0.7, a.WorkSpeed(c, SkillType.Tailoring, true, 3), 6);
            a.Care(c, 4);
            Assert.Equal(1 - 0.45 / 2, a.WorkSpeed(c, SkillType.Tailoring, true, 4), 6);   // Yaşlıya bak yarıya
        }

        [Fact]
        public void DigerSikintilar_CozumleriVeArtilari()
        {
            var a = new ElderAilmentService();
            var c = Elder();
            Activate(a, c, AgingCondition.Teeth);
            Assert.True(a.HardFoodPenalty(c, "item.bread_rye", 1));
            Assert.False(a.HardFoodPenalty(c, "item.cheese", 1));
            a.CrumbleBite(c, 1);
            Assert.False(a.HardFoodPenalty(c, "item.bread_rye", 1));
            var d = Elder(2);
            Activate(a, d, AgingCondition.Chill);
            Assert.Equal(1.3, a.WarmthNeed(d, false, false, 1), 6);
            Assert.Equal(1.0, a.WarmthNeed(d, true, false, 1), 6);
            var h = Elder(3);
            Activate(a, h, AgingCondition.Hearing);
            Assert.True(a.Misheard(h, 0, 1, roll: 0.1));
            Assert.False(a.Misheard(h, 70, 1, roll: 0.1));
            a.SpeakUp(h, 1);
            Assert.False(a.Misheard(h, 0, 1, roll: 0.1));
            Assert.Equal(0.75, a.XpFactor(h, SkillType.Charm, 2), 6);
            Assert.True(a.NoiseImmune(h));
            var j = Elder(4);
            Activate(a, j, AgingCondition.Joints);
            Assert.True(a.ForetellsRain(new[] { j }));
            var s = Elder(5);
            Activate(a, s, AgingCondition.Sleep);
            Assert.Equal(0.8, a.SleepQuality(s, 1), 6);
            a.Chamomile(s, 1);
            Assert.Equal(1.0, a.SleepQuality(s, 1), 6);
            Assert.Equal(1.25, a.EarlyRiser(s, 6.0 / 24, true));
            var t = Elder(6);
            Activate(a, t, AgingCondition.Tremor);
            t.Skills.RestoreXp(SkillType.Tailoring, SkillRules.XpForLevel(9));
            Assert.False(a.CanMakeMasterwork(t, SkillType.Tailoring));
            Assert.Equal(1.5, a.MasterEyeXp(t, Elder(7, 20), SkillType.Tailoring));
            // Hafif ayarda eksiler yarı
            var mild = new ElderAilmentService { Setting = AgingSetting.Mild };
            var m = Elder(8);
            Activate(mild, m, AgingCondition.Knees);
            Assert.Equal(0.85, mild.WalkFactor(m, 1), 6);
        }

        [Fact]
        public void Zihin_EtkinlikGunlukTavanli_YaslidaIkiKat_YastaSayilmaz()
        {
            var a = new ElderAilmentService();
            var adult = Elder(1, 60);
            Assert.True(a.AddMind(adult, MindActivity.Reading, 1));
            Assert.False(a.AddMind(adult, MindActivity.Reading, 1));   // aynı gün aynı etkinlik
            Assert.True(a.AddMind(adult, MindActivity.Music, 1));
            Assert.Equal(2, a.StateOf(adult).Mind);
            var old = Elder(2, 80);
            a.AddMind(old, MindActivity.Reading, 1);
            Assert.Equal(2, a.StateOf(old).Mind);
            var young = Elder(3, 40);
            Assert.False(a.AddMind(young, MindActivity.Reading, 1));   // 55. günden önce birikmez
            Assert.False(a.ShowMind(adult));
            Assert.True(a.ShowMind(old));
            for (int d = 0; d < 3; d++) a.Daily(old, d, false, true, true);   // yas: sayılmaz
            Assert.Equal(2, a.StateOf(old).Mind);
            for (int d = 3; d < 6; d++) a.Daily(old, d, true, false, false);  // 3 gün yalnız
            Assert.True(a.StateOf(old).Mind < 2);
            for (int d = 0; d < 6; d++) a.AddMind(adult, (MindActivity)d, 5);
            Assert.Equal(MindLevel.Fresh, a.MindOf(adult));
        }

        [Fact]
        public void Dalginlik_IsaretSonraDurum_IsBirakma_KahveCay()
        {
            var a = new ElderAilmentService(3);
            var c = Elder();
            a.OnBecameElder(c, 0);
            var s = a.StateOf(c);
            s.Pending.Clear();
            s.Pending.Add((AgingCondition.Absent, 12));
            int signals = 0;
            a.Signal += _ => signals++;
            a.Daily(c, 12, false, false, false);
            Assert.False(a.Has(c, AgingCondition.Absent));
            Assert.Equal(1, signals);
            a.Daily(c, 15, false, false, false);
            Assert.True(a.Has(c, AgingCondition.Absent));
            long day = 16;
            Assert.Equal(0.15, a.AbsentDropChance(c, day), 6);
            c.AddMoodlet(ElderAilmentService.Awake, CoffeeMood());
            Assert.Equal(0.075, a.AbsentDropChance(c, day), 6);
            c.Mood.Remove(ElderAilmentService.Awake);
            c.AddMoodlet(ElderAilmentService.Mahmur, CoffeeMood());
            Assert.Equal(0.225, a.AbsentDropChance(c, day), 6);
            c.Mood.Remove(ElderAilmentService.Mahmur);
            Assert.False(a.TryDrop(c, day, waitTolerant: false, roll: 0.0));   // pişirme, kazan: durmaz
            Assert.True(a.TryDrop(c, day, true, roll: 0.0));
            Assert.False(a.TryDrop(c, day, true, roll: 0.0));                  // günde bir kez
            a.Remind(c, day + 1);
            Assert.Equal(0, a.AbsentDropChance(c, day + 1));
            // önleme: işaret döneminde zihin etkinliği %25
            var b = Elder(2);
            a.OnBecameElder(b, 0);
            a.StateOf(b).Pending.Clear();
            a.StateOf(b).Pending.Add((AgingCondition.Absent, 12));
            a.Daily(b, 12, false, false, false);
            Assert.True(a.TryPreventAbsent(b, roll: 0.1));
            a.Daily(b, 15, false, false, false);
            Assert.False(a.Has(b, AgingCondition.Absent));
        }

        private static MoodCatalog CoffeeMood() => ContentFilesTests.LoadDb(out _).Moods ?? MoodCatalog.Default;

        [Fact]
        public void Kayit_GeriGelir()
        {
            var a = new ElderAilmentService();
            var c = Elder();
            a.OnBecameElder(c, 3);
            a.StateOf(c).Active.Add(AgingCondition.Eyes);
            a.EquipAid(c, AgingAids.GlassesSilver);
            a.AddMind(c, MindActivity.Music, 4);
            var b = new ElderAilmentService();
            b.Import(a.Export());
            Assert.True(b.Has(c, AgingCondition.Eyes));
            Assert.True(b.Glasses(c));
            Assert.Equal(a.StateOf(c).Pending.Count, b.StateOf(c).Pending.Count);
            Assert.Equal(a.StateOf(c).Mind, b.StateOf(c).Mind);
        }

        [Fact]
        public void Yasam_IceceklerVeCaySohbeti_CocukCayIcmez()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var w = new WorldState(new GridMap(8, 8));
            var house = new Household(0);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, w, house, pop, clock);
            var a = Elder(1); var b = Elder(2, 40); var kid = Elder(3, 15);
            pop.Add(a); pop.Add(b); pop.Add(kid);
            var pot = db.Items.Get("item.cooking_pot");
            var sage = pot.Interactions.First(i => i.Id == "tea_sage");
            life.OnInteractionApplied(a, sage);
            Assert.True(a.Mood.Has("eff_sage_clear"));
            var teapot = db.Items.Get("item.teapot");
            var drink = teapot.Interactions.First(i => i.Id == "drink_black_tea");
            life.OnInteractionApplied(a, drink);
            life.OnInteractionApplied(b, drink);
            Assert.True(a.Mood.Has("eff_tea_talk") && b.Mood.Has("eff_tea_talk"));
            var fx = new Thatchwick.Core.Work.InteractionEffects(w, house, db);
            life.Attach(fx);
            Assert.Contains("Çocuk", fx.ExtraBlocker(kid, null, drink) ?? "");
            Assert.Null(fx.ExtraBlocker(b, null, drink));
            // gebelik giysisi yok: hiçbir eksi yok (Parti 5 kuralı)
            Assert.True(b.TryStartPregnancy(9) || b.Sex == Sex.Male);
        }
    }
}
