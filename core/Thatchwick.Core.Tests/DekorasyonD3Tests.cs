using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ev dekorasyonu D3, cam zinciri (Project belgesi 27 §18): kumsal, kül, kalbur, cam ocağı, camcı tezgâhı, Camcı dalı,
    // cam eşyalar ve aynalar, kutu yatak, kurşunlu pencere.
    public class DekorasyonD3Tests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static InteractionDef Craft(ContentDatabase db, string station, string recipe) =>
            db.Items.Get(station).Interactions.First(i => i.Id == "craft:" + recipe);

        [Fact]
        public void Kumsal_YalnizcaSuyunKenarindaBelirir_KumVerir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(20, 20));
            for (int x = 5; x <= 8; x++) for (int y = 5; y <= 8; y++) world.Map.SetFloor(new GridPos(x, y), FloorType.Water);
            var forage = new ForageSystem(world, db, seed: 3);
            for (int day = 0; day < 6; day++) forage.OnNewDay(Season.Summer);
            var sand = world.Items.Where(i => i.Def.Id == "item.forage_sand").ToList();
            Assert.NotEmpty(sand);
            Assert.All(sand, s => Assert.True(ForageSystem.IsShore(world.Map, s.Position), $"{s.Position} kıyı değil"));
            Assert.Empty(forage.OnNewDay(Season.Winter) >= 0 ? world.Items.Where(i => i.Def.Id == "item.forage_sand") : null);   // kışın yok

            var dig = db.Items.Get("item.forage_sand").Interactions[0];
            Assert.Equal(SkillType.Mining, dig.Skill);
            Assert.Contains(dig.Produce, p => p.ItemId == "item.sand" && p.Count == 3);
        }

        [Fact]
        public void Kul_OcagaAtilanHerDortOdundaBir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household();
            var life = new LifeSystems(db, world, house, new Population(), new GameClock());
            var heat = new HeatSystem(world, new GameClock().Config);
            life.Attach(heat);
            Assert.Equal("item.ash", heat.AshItemId);
            var hearth = world.PlaceItem(db.Items.Get("item.hearth_stone"), new GridPos(3, 3));
            house.AddItem("item.firewood", 20);
            int stoked = 0;
            for (int i = 0; i < 40 && stoked < 8; i++)
            {
                if (heat.TryStoke(hearth.Id, house)) stoked++;
                else hearth.GetType().GetProperty("FuelDays").SetValue(hearth, 0.0);   // yanıp bitti
            }
            Assert.Equal(8, stoked);
            Assert.Equal(2, house.Count("item.ash"));
        }

        [Fact]
        public void Zincir_TarifleriVeSeviyeleri()
        {
            var db = Db();
            var sift = Craft(db, "item.sieve", "sift_sand");
            Assert.Equal("item.fine_sand", sift.Produce[0].ItemId);
            var glass = Craft(db, "item.glass_furnace", "glass");
            Assert.Equal(SkillType.Pottery, glass.Skill);
            Assert.Equal(6, glass.MinSkillLevel);
            Assert.Contains(glass.Consume, c => c.ItemId == "item.ash");
            Assert.Contains(glass.Consume, c => c.ItemId == "item.sand" && c.Count == 2);
            var remelt = Craft(db, "item.glass_furnace", "glass_remelt");
            Assert.Contains(remelt.Consume, c => c.ItemId == Decor.BrokenGlass);           // kırık cam döngüsü kapanır
            Assert.Equal(2, Craft(db, "item.glass_bench", "glass_bottle").Produce[0].Count);
            Assert.Equal(3, Craft(db, "item.glass_bench", "mirror_glass").Produce[0].Count);
            Assert.Equal(SkillType.Carpentry, Craft(db, "item.workbench", "convex_mirror").Skill);
            Assert.Equal(5, Craft(db, "item.workbench", "box_bed").MinSkillLevel);
            // Kurşunlu pencere yapı parçası envanterdeki kurşunlu camı kullanır.
            Assert.Equal("item.leaded_glass", db.Parts.Get("window_leaded").MaterialItem);
        }

        [Fact]
        public void CamciDali_BerracCamVeAynaKuresiIster_CamIsleriHizli()
        {
            var db = Db();
            Assert.Equal("Camcı", db.Branches.Get(LifeSystems.GlassBranchId).DisplayName);
            var clear = Craft(db, "item.glass_furnace", "clear_glass");
            var sphere = Craft(db, "item.glass_bench", "mirror_sphere");
            Assert.Equal(LifeSystems.GlassBranchId, clear.RequiresBranch);
            Assert.Equal("Camcı", sphere.RequiresBranchName);
            Assert.Null(Craft(db, "item.glass_furnace", "glass").RequiresBranch);

            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var c = new Character(1, "Ayşe", new GridPos(2, 2));
            c.SetAge(30);
            c.GainSkillXp(SkillType.Pottery, SkillRules.XpForLevel(9));
            var bench = world.PlaceItem(db.Items.Get("item.glass_bench"), new GridPos(4, 4));
            var runner = new JobRunner(c, world);
            Assert.Equal("Camcı dalı gerekir.", runner.WhyCannotStart(bench, sphere));

            var life = new LifeSystems(db, world, house, new Population(), new GameClock());
            var bottle = Craft(db, "item.glass_bench", "glass_bottle");
            var jug = db.Items.Get("item.pottery_wheel").Interactions.First(i => i.Id == "craft:jug");
            double before = life.WorkSpeedFor(c, bottle);
            Assert.True(LifeRules.ChooseBranch(db, c, LifeSystems.GlassBranchId, out var why), why);
            Assert.Equal(before * LifeSystems.GlassBranchSpeed, life.WorkSpeedFor(c, bottle), 6);
            Assert.Equal(life.WorkSpeedFor(new Character(2, "Wat", new GridPos(2, 2)), jug), life.WorkSpeedFor(c, jug), 6);   // çömlek değil
            Assert.NotEqual("Camcı dalı gerekir.", runner.WhyCannotStart(bench, sphere));
        }

        [Fact]
        public void CamEsyalar_YalnizcaYapilir_KirilganAynaSayilir()
        {
            var db = Db();
            foreach (var id in new[] { "item.glass_bottle", "item.glass_jar", "item.glass_cup", "item.glass_goblet", "item.hourglass", "item.convex_mirror", "item.large_mirror", "item.box_bed" })
            {
                var d = db.Items.Get(id);
                Assert.True(d.HasTag("craftonly"), id);
                Assert.DoesNotContain(ItemCatalog.InCategory(db, d.Category), x => x.Id == id);
            }
            Assert.True(Decor.IsFragile(db.Items.Get("item.hourglass").EffectiveMaterial));
            var world = new WorldState(new GridMap(10, 10));
            Assert.False(DecorSystem.HasMirror(world));
            for (int x = 2; x <= 4; x++) world.Map.SetEdge(new Edge(x, 2, EdgeAxis.Horizontal), EdgeKind.Wall);
            world.PlaceOnWall(db.Items.Get("item.large_mirror"), new GridPos(2, 2), 0);
            Assert.True(DecorSystem.HasMirror(world));
            Assert.Equal(0f, db.Items.Get("item.convex_mirror").TarnishDays);                // cam ayna kararmaz

            // Kavanoz reçel ya da turşuyla dolar.
            var jar = db.Items.Get("item.glass_jar");
            Assert.True(jar.AcceptsFill("item.jam_plum"));
            Assert.Equal(1, jar.FillCount);
        }
    }
}
