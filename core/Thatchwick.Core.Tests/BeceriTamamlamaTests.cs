using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Progress;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Beceri tamamlama (Project belgesi 45, 9. bölüm): ortak basamaklar (Sv 3 konu, Sv 7 gösteri, Sv 10 usta) ve
    // beceri sosyal işleri.
    public class BeceriTamamlamaTests
    {
        private static Character Person(int id, double age = 45, Population pop = null)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(age);
            pop?.Add(c);
            return c;
        }

        private static void Level(Character c, SkillType s, int level) => c.Skills.RestoreXp(s, SkillRules.XpForLevel(level));

        private sealed class Ctx { public Population Pop; public Household House; public FamilyService Family; }

        private static Ctx Make()
        {
            var db = new ContentDatabase();
            var house = new Household();
            var pop = new Population();
            return new Ctx { Pop = pop, House = house, Family = new FamilyService(pop, house, db, seed: 2) };
        }

        // ---------- Sv 3: ortak konu ----------

        [Fact]
        public void Sohbet_OrtakBeceri3teKonuOlur_YakinlikFazla()
        {
            var x = Make();
            var a = Person(1, 45, x.Pop); var b = Person(2, 45, x.Pop);
            var c = Person(3, 45, x.Pop); var d = Person(4, 45, x.Pop);
            Level(a, SkillType.Smithing, 4); Level(b, SkillType.Smithing, 3);
            Assert.Equal(SkillType.Smithing, FamilyService.SharedTopic(a, b));
            Assert.Null(FamilyService.SharedTopic(c, d));
            x.Family.Apply(a, b, SocialAction.Chat);
            x.Family.Apply(c, d, SocialAction.Chat);
            Assert.True(x.Family.Affection(1, 2) > x.Family.Affection(3, 4));
        }

        // ---------- beceri sosyal işleri ----------

        [Fact]
        public void SosyalIs_SeviyeyleAcilir_HedefKurali()
        {
            var x = Make();
            var a = Person(1, 45, x.Pop); var b = Person(2, 45, x.Pop);
            Assert.Contains("Gönül kazanma 2", x.Family.CanDo(a, b, SocialAction.Compliment));
            Level(a, SkillType.Charm, 2);
            Assert.Null(x.Family.CanDo(a, b, SocialAction.Compliment));
            Assert.Contains(FamilyService.UnlockedSocials(a), d => d.Action == SocialAction.Compliment);
            Assert.DoesNotContain(FamilyService.UnlockedSocials(a), d => d.Action == SocialAction.Joke);
            Level(a, SkillType.Parenting, 3);
            Assert.NotNull(x.Family.CanDo(a, b, SocialAction.PlayWithBaby));   // yetişkine olmaz
        }

        [Fact]
        public void DertDinle_KotuMoodletiKaldirir()
        {
            var x = Make();
            var a = Person(1, 45, x.Pop); var b = Person(2, 45, x.Pop);
            Level(a, SkillType.Charm, 4);
            b.AddMoodlet(MoodCatalog.Ashamed);
            Assert.True(b.Mood.Has(MoodCatalog.Ashamed));
            x.Family.Apply(a, b, SocialAction.Listen);
            Assert.False(b.Mood.Has(MoodCatalog.Ashamed));
            Assert.True(b.Mood.Has(MoodCatalog.Unburdened));
        }

        [Fact]
        public void IsGoster_CocugaUstaninBecerisindenXp()
        {
            var x = Make();
            var a = Person(1, 45, x.Pop); var kid = Person(2, 15, x.Pop);
            Level(a, SkillType.Parenting, 6); Level(a, SkillType.Carpentry, 8);
            x.Family.Apply(a, kid, SocialAction.ShowWork);
            Assert.True(kid.Skills.GetXp(SkillType.Carpentry) > 0);
        }

        [Fact]
        public void YasliyaDers_BuMevsimPaslanmaz()
        {
            var x = Make();
            var t = Person(1, 45, x.Pop); var elder = Person(2, 80, x.Pop);
            Level(t, SkillType.Teaching, 9);
            elder.Skills.RestoreXp(SkillType.Weaving, 500);
            Assert.False(elder.Skills.UsedThisSeason(SkillType.Weaving));
            x.Family.Apply(t, elder, SocialAction.ElderLesson);
            Assert.True(elder.Skills.UsedThisSeason(SkillType.Weaving));
        }

        [Fact]
        public void MeyhanedeCal_BahsisVerir_GundeBir()
        {
            var x = Make();
            x.Family.RoleOf = c => c.Id == 2 ? VillagerService.RoleInnkeeper : null;
            var a = Person(1, 45, x.Pop); var inn = Person(2, 45, x.Pop); inn.IsVillager = true;
            Level(a, SkillType.Music, 5);
            Assert.Null(x.Family.CanDo(a, inn, SocialAction.PlayTavern));
            int before = x.House.Akce;
            x.Family.Apply(a, inn, SocialAction.PlayTavern);
            Assert.Equal(before + FamilyService.TavernPayBase + 5, x.House.Akce);
            Assert.NotNull(x.Family.CanDo(a, inn, SocialAction.PlayTavern));
        }

        [Fact]
        public void Gorucu_TalipCagirir()
        {
            var x = Make();
            Character called = null;
            x.Family.OnMatchmake = c => called = c;
            var a = Person(1, 45, x.Pop); var single = Person(2, 34, x.Pop);
            Level(a, SkillType.Charm, 7);
            Assert.Null(x.Family.CanDo(a, single, SocialAction.Matchmake));
            x.Family.Apply(a, single, SocialAction.Matchmake);
            Assert.Same(single, called);
        }

        // ---------- Sv 7 ve Sv 10: ustalık ----------

        [Fact]
        public void Usta_UnvanVeGosteri()
        {
            var a = Person(1);
            Assert.Null(Mastery.Title(a));
            Level(a, SkillType.Smithing, 10);
            Assert.Equal("Demirci", Mastery.Title(a));
            Assert.Equal("Demirci K1", Mastery.DisplayName(a));
            Level(a, SkillType.Woodcutting, 10);
            a.Skills.RestoreXp(SkillType.Woodcutting, SkillRules.XpForLevel(10) + 500);
            Assert.Equal("Baltacı K1", Mastery.DisplayName(a));

            var b = Person(2); Level(b, SkillType.Music, 7);
            var kid = Person(3, 15);
            Assert.Equal(SkillType.Music, Mastery.ShowcaseSkill(b));
            Assert.Equal(1, Mastery.ApplyShowcase(b, SkillType.Music, new[] { kid }));
            Assert.True(kid.Skills.GetXp(SkillType.Music) > 0);
            Assert.True(kid.Mood.Has(MoodCatalog.SkillShowcase));
        }

        [Fact]
        public void UstaSiparisi_GercekIcerikten()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var orders = Mastery.Orders(db, new[] { SkillType.Smithing, SkillType.Weaving }, 9);
            Assert.Equal(2, orders.Count);
            foreach (var o in orders)
            {
                Assert.True(db.Items.Contains(o.item));
                Assert.Equal(Mastery.OrderCount, o.count);
                Assert.True(o.reward > 0);
            }
            Assert.Equal(orders, Mastery.Orders(db, new[] { SkillType.Smithing, SkillType.Weaving }, 9));   // aynı gün aynı sipariş
        }
    }
}
