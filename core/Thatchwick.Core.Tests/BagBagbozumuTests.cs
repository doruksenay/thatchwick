using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Bağ, şıra, pekmez, sirke, şarap (+18 paketi) ve Bağbozumu şenliği (belge 32-8.3).
    public class BagBagbozumuTests
    {
        [Fact]
        public void Icerik_AsmaSiraPekmezSirkeVeSarap()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.True(db.Items.Get("item.vine_grape").IsFruitTree);
            Assert.Contains(db.Items.Get("item.grape_tub").Interactions, i => i.Id == "craft:tread_grapes");
            var barrel = db.Items.Get("item.brew_barrel");
            Assert.Contains(barrel.Interactions, i => i.Id == "craft:vinegar_grape" && !i.AdultPack);
            Assert.True(barrel.Interactions.First(i => i.Id == "craft:wine").AdultPack);
            Assert.DoesNotContain(barrel.Interactions, i => i.Id == "craft:wine_age");   // şarap artık mahzende kendiliğinden yıllanır (belge 53)
            Assert.True(db.Items.Get("item.wine_new").AgeStepDays > 0f);
            var pot = db.Items.Get("item.cooking_pot");
            Assert.Contains(db.Items.Get("item.hearth_stone").Interactions, i => i.Id == "make_pekmez_grape" && !i.AdultPack);   // pişirme ocakta (63a)
            Assert.True(pot.Interactions.First(i => i.Id == "drink_wine").AdultPack);
            Assert.Contains(db.Festivals.All, f => f.Id == LifeSystems.VintageFestival && f.Season == Season.Autumn);
            var vintage = db.Festivals.Get(LifeSystems.VintageFestival);
            Assert.True(vintage.Day < db.Festivals.Get("festival.harvest").Day);   // Hasat Şenliği'nden önce
        }

        [Fact]
        public void Paket18_KapaliyGizli_AcikkenGencYetiskin()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var wine = db.Items.Get("item.brew_barrel").Interactions.First(i => i.Id == "craft:wine");
            bool old = ContentPacks.Adult;
            try
            {
                ContentPacks.Adult = false;
                Assert.True(wine.Hidden);
                ContentPacks.Adult = true;
                Assert.False(wine.Hidden);
                Assert.False(AgeStages.IsAdultContentAge(AgeStage.Teen));
                Assert.True(AgeStages.IsAdultContentAge(AgeStage.YoungAdult));
            }
            finally { ContentPacks.Adult = old; }
        }

        [Fact]
        public void Bagbozumu_UzumEzilir_YarismaKazanilir_Herkese_Moodlet()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var w = new WorldState(new GridMap(8, 8));
            var house = new Household(0);
            var pop = new Population();
            var c = new Character(1, "Ayşe", new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(40);
            c.Skills.RestoreXp(SkillType.Horticulture, SkillRules.XpForLevel(5));
            pop.Add(c);
            var life = new LifeSystems(db, w, house, pop, new GameClock());
            house.AddItem("item.grape", 10);
            var notes = life.Vintage(roll: 0.0);
            Assert.Equal(2, house.Count("item.grape"));
            Assert.Equal(4, house.Count("item.must"));
            Assert.Equal(LifeSystems.VintagePrize, house.Akce);
            Assert.Contains(notes, n => n.Contains("kazandı"));
            Assert.Contains(c.Chronicle, n => n.StartsWith("@@core.chronicle.vintage_won"));
            Assert.True(c.Mood.Has("eff_vintage"));
            // Üzüm yoksa yarışmaya girilmez.
            var empty = new Household(0);
            var life2 = new LifeSystems(db, w, empty, pop, new GameClock());
            Assert.DoesNotContain(life2.Vintage(roll: 0.0), n => n.Contains("yarışma"));
            Assert.Equal(0, empty.Akce);
        }
    }
}
