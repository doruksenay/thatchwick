using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Zeytincilik ve zeytinyağı, bal süzme ve arı sokması, bitki çayları (belge 32-7, 8.1, 8.2).
    public class ZeytinBalCayTests
    {
        [Fact]
        public void Zeytin_YalnizcaGuneyde_ZincirTam()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var tree = db.Items.Get("item.tree_olive");
            Assert.True(tree.SouthOnly && tree.IsFruitTree);
            Assert.Contains(tree.Interactions, i => i.Id == "pick_hand");
            foreach (var (station, craft) in new[] { ("item.brine_crock", "brine_olives"), ("item.olive_crusher", "crush_olives"), ("item.olive_press", "press_virgin"),
                     ("item.olive_press", "press_common"), ("item.olive_press", "pomace"), ("item.chandler_table", "soap_olive"), ("item.honey_strainer", "strain_honey") })
                Assert.Contains(db.Items.Get(station).Interactions, i => i.Id == "craft:" + craft);
            Assert.True(db.Items.Get("item.olive_oil_virgin").Price > db.Items.Get("item.olive_oil").Price);

            // Güney yoksa zeytin büyümez; güneyse büyür.
            var map = new GridMap(8, 8);
            var w = new WorldState(map);
            var plants = new PlantSystem(w);
            var t = w.PlaceItem(tree, new GridPos(3, 3));
            plants.Tick(2, Season.Summer);
            Assert.Equal(0, t.GrowthDays, 3);
            plants.IsSouth = p => true;
            plants.Tick(2, Season.Summer);
            Assert.True(t.GrowthDays > 1);
        }

        [Fact]
        public void Kahve_GuneydeSerasizDaBuyur()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var w = new WorldState(new GridMap(8, 8));
            var plants = new PlantSystem(w);
            var bush = w.PlaceItem(db.Items.Get("item.bush_coffee"), new GridPos(2, 2));
            plants.Tick(1, Season.Summer);
            Assert.Equal(0, bush.GrowthDays, 3);
            plants.IsSouth = p => true;
            plants.Tick(1, Season.Summer);
            Assert.True(bush.GrowthDays > 0.5);
        }

        [Fact]
        public void Cay_KurutulurDemlenir_EtkileriVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var rack = db.Items.Get("item.drying_rack");
            var pot = db.Items.Get("item.cooking_pot");
            foreach (var h in new[] { "chamomile", "linden", "sage", "mint", "rosehip" })
            {
                Assert.Contains(rack.Interactions, i => i.Id == "craft:dry_" + h);
                var brew = pot.Interactions.First(i => i.Id == "tea_" + h);
                Assert.Equal("item.tea_" + h, brew.ConsumeItem);
                Assert.True(Thatchwick.Core.Mood.MoodCatalog.Default.TryGet("eff_tea_" + h, out _));
            }
            foreach (var f in new[] { "item.forage_chamomile", "item.forage_linden", "item.forage_sage", "item.forage_mint" })
                Assert.True(db.Items.Get(f).IsForage);
        }

        [Fact]
        public void Ari_AcemiyiSokar_UstayiSokmaz_Olumcul_Degil()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(8, 8);
            var w = new WorldState(map);
            var house = new Household(100);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, w, house, pop, clock);
            var novice = new Character(1, "Acemi", new GridPos(0, 0), true) { CanStarve = false };
            novice.SetAge(30);
            Assert.True(life.TrySting(novice, roll: 0.0));
            Assert.NotNull(novice.Injury);
            Assert.False(novice.IsDead);
            var master = new Character(2, "Usta", new GridPos(0, 0), true) { CanStarve = false };
            master.SetAge(30);
            master.Skills.RestoreXp(SkillType.AnimalHusbandry, SkillRules.XpForLevel(LifeSystems.BeeSafeLevel));
            Assert.False(life.TrySting(master, roll: 0.0));
            Assert.False(life.TrySting(novice, roll: 0.9));
        }
    }
}
