using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Beceri tamamlama 3 (Project belgesi 45, 9. bölüm): tablonun içerik ve pasif satırları.
    public class BeceriIsleriTests
    {
        private static Character Person(int id)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(40);
            return c;
        }

        private static void Level(Character c, SkillType s, int level) => c.Skills.RestoreXp(s, SkillRules.XpForLevel(level));

        [Fact]
        public void Tablo_PlanlanmisSatirKalmadi_YeniIcerikVar()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            Assert.DoesNotContain(db.SkillUnlocks, u => u.Planned);
            foreach (var id in new[] { "item.crop_saffron", "item.flower_bed", "item.cheese_rack", "item.silk", "item.race_saddle", "item.folk_almanac",
                                       "item.wedding_bread", "item.master_lantern", "item.covered_cart", "item.catki_table", "item.yoruk_chest" })
                Assert.True(db.Items.Contains(id), id);
        }

        [Fact]
        public void Kilit_TohumBitkisininBecerisinde_FidanBahcivanlikta()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var onion = db.Items.Get("item.seed_onion");
            Assert.Equal(SkillType.Farming, UnlockGate.SkillFor(onion));
            Assert.Equal(2, onion.UnlockLevel);
            Assert.Equal(SkillType.Farming, UnlockGate.SkillFor(db.Items.Get("item.seed_wheat")));
            Assert.Equal(3, db.Items.Get("item.seed_wheat").UnlockLevel);
            var grape = db.Items.Get("item.sapling_grape");
            Assert.Equal(SkillType.Horticulture, UnlockGate.SkillFor(grape));
            Assert.Equal(4, grape.UnlockLevel);
            Assert.Equal(SkillType.Horticulture, UnlockGate.SkillFor(db.Items.Get("item.crop_saffron")));
            Assert.Equal(SkillType.Fishing, UnlockGate.SkillFor(db.Items.Get("item.fish_trap")));
            Assert.Equal(SkillType.Carpentry, UnlockGate.SkillFor(db.Items.Get("item.catki_chair")));
            Assert.Equal(5, db.Items.Get("item.catki_chair").UnlockLevel);
        }

        [Fact]
        public void BeceriIsi_IcerikteVar_SeviyeIster()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var turnip = db.Items.Get("item.crop_turnip");
            Assert.Equal(2, turnip.Interactions.First(i => i.Id == "inspect_field").MinSkillLevel);
            Assert.Equal("bulk_sow", turnip.Interactions.First(i => i.Id == "bulk_sow").SkillWork);
            Assert.Equal(3, turnip.Interactions.First(i => i.Id == "manure").MinSkillLevel);
            Assert.Contains(db.Items.Get("item.tree_apple").Interactions, i => i.SkillWork == "graft" && i.MinSkillLevel == 5);
            Assert.Contains(db.Items.Get("item.pond").Interactions, i => i.Id == "ice_fish" && i.MinSkillLevel == 7 && i.RequiresFrost);
        }

        [Fact]
        public void Avcilik_TurSeviyesi_TaziUstaligi()
        {
            Assert.Equal(4, Hunting.MinLevel("wild.roe_deer"));
            Assert.Equal(7, Hunting.MinLevel("wild.wolf"));
            var h = Person(1);
            Level(h, SkillType.Hunting, 3);
            var r = Hunting.Try(new System.Random(1), "wild.roe_deer", HuntMethod.Bow, h);
            Assert.False(r.Success);
            Assert.Contains("Avcılık 4", r.Message);
            var deer = WildCatalog.Get("wild.roe_deer");
            Assert.True(Hunting.Chance(deer, HuntMethod.Bow, 8, true) > Hunting.Chance(deer, HuntMethod.Bow, 7, true) + 0.06 + 0.05);
        }

        [Fact]
        public void Yarisma_BeceriEki()
        {
            var h = Person(1);
            Assert.Equal(8, AnimalContests.SkillBonus(ContestKind.HorseRace, null, h, prepared: true, raceSaddle: false));
            Level(h, SkillType.Riding, 9); Level(h, SkillType.AnimalHusbandry, 10);
            Assert.Equal(5 + 10 + 6, AnimalContests.SkillBonus(ContestKind.HorseRace, null, h, prepared: false, raceSaddle: true));
        }

        [Fact]
        public void Itibar_Cazibe10_KazancCarpani()
        {
            var rep = new Thatchwick.Core.Upbringing.ReputationSystem { GainFactor = () => 1.1 };
            rep.Add(10, "deneme");
            Assert.Equal(11, rep.Points);
            rep.Add(-5, "kayıp");
            Assert.Equal(6, rep.Points);
        }

        [Fact]
        public void BeceriBayraklari_KaydaGirerGeriGelir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), new Household(), new Population(), new GameClock());
            life.SetSkillFlag("graft:12", 1);
            life.SetSkillFlag("stock:4", 33.5);
            life.HiredHand = true;
            var data = life.Export();
            var other = new LifeSystems(db, new WorldState(new GridMap(8, 8)), new Household(), new Population(), new GameClock());
            other.Restore(data);
            Assert.Equal(1, other.SkillFlag("graft:12"));
            Assert.Equal(33.5, other.SkillFlag("stock:4"));
            Assert.True(other.HiredHand);
        }

        [Fact]
        public void Ticaret_TezgahPazarlikToptan()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var house = new Household(500);
            int trade = 1;
            var m = new Thatchwick.Core.Economy.Market(db, house) { HouseTradeLevel = () => trade };
            m.Tick(0, 0.1, Season.Spring);
            string good = m.Goods().First(g => g.Price >= 10).Id;
            int before = m.SellPrice(good);
            Assert.Contains("Ticaret 5", m.SetUpStall());
            trade = 5;
            m.SetUpStall();
            Assert.True(m.StallToday);
            Assert.True(m.SellPrice(good) > before);
            Assert.Contains("Ticaret 2", m.Haggle());   // pazarlık komut alanın becerisine bakar (Trader yok: 1)
            Assert.Null(m.YesterdaySellPrice(good));   // dün yok
            m.Tick(1, 1.1, Season.Spring);
            m.Tick(1, 2.1, Season.Spring);
            Assert.NotNull(m.YesterdaySellPrice(good));
        }

        [Fact]
        public void HesapDefteri_VeHaftalikPlan_MetinVerir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), new Household(), new Population(), new GameClock());
            Assert.StartsWith("Hesap defteri:", life.LedgerText());
            Assert.StartsWith("Haftalık plan:", life.WeeklyPlanText());
        }
    }
}
