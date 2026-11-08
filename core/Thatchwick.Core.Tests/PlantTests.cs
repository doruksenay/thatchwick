using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class PlantTests
    {
        private static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "log", DisplayName = "Kütük", Placement = ItemPlacement.Resource, Price = 3 });
            db.Items.Register(new ItemDef { Id = "firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2 });
            db.Items.Register(new ItemDef { Id = "sapling", DisplayName = "Meşe fidanı", Placement = ItemPlacement.Resource, Price = 5, PlantsAs = "oak" });
            db.Items.Register(new ItemDef
            {
                Id = "oak", DisplayName = "Meşe", GrowDays = 12f,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "chop", DisplayName = "Kes", DurationDays = 0.15, Skill = SkillType.Woodcutting, SkillXp = 20,
                        RequiresGrown = true, RemovesItem = true,
                        Produce = new List<ProduceDef>
                        {
                            new ProduceDef { ItemId = "log", Count = 3 },
                            new ProduceDef { ItemId = "sapling", Count = 1 }
                        }
                    }
                }
            });
            db.Items.Register(new ItemDef
            {
                Id = "block", DisplayName = "Yarma kütüğü", Price = 10,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "split", DisplayName = "Odun yar", DurationDays = 0.05, Skill = SkillType.Woodcutting, SkillXp = 8,
                        ConsumeItem = "log", Produce = new List<ProduceDef> { new ProduceDef { ItemId = "firewood", Count = 3 } }
                    }
                }
            });
            return db;
        }

        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public PlantSystem Plants;
            public InteractionEffects Effects;
        }

        private static Ctx Setup()
        {
            var db = Content();
            Assert.Empty(db.Validate());
            var world = new WorldState(new GridMap(16, 12));
            var house = new Household(100);
            return new Ctx
            {
                Db = db, World = world, House = house,
                Build = new BuildService(world, house, db),
                Plants = new PlantSystem(world),
                Effects = new InteractionEffects(world, house, db, seed: 3)
            };
        }

        private static JobRunner Runner(Ctx c, Character ch) => new JobRunner(ch, c.World) { Effects = c.Effects };

        // ---- dikim ----

        [Fact]
        public void Fidan_DisaridaCimeDikilir_EnvantendenDuser()
        {
            var c = Setup();
            c.House.AddItem("sapling", 2);
            Assert.True(c.Build.Plant("sapling", new GridPos(3, 3), out var tree).Ok);
            Assert.Equal("oak", tree.Def.Id);
            Assert.False(tree.IsGrown);
            Assert.Equal(1, c.House.Count("sapling"));
            Assert.True(c.World.Map.IsBlocked(new GridPos(3, 3)));
        }

        [Fact]
        public void Fidan_EvIcineTahtaZemineArsaDisinaDoluKaroyaDikilmez()
        {
            var c = Setup();
            c.House.AddItem("sapling", 5);
            var m = c.World.Map;
            // 2x2 oda (8..9, 2..3)
            for (int x = 8; x <= 9; x++) { m.SetEdge(new Edge(x, 2, EdgeAxis.Horizontal), EdgeKind.Wall); m.SetEdge(new Edge(x, 4, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = 2; y <= 3; y++) { m.SetEdge(new Edge(8, y, EdgeAxis.Vertical), EdgeKind.Wall); m.SetEdge(new Edge(10, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            Assert.False(c.Build.Plant("sapling", new GridPos(8, 2), out _).Ok);
            m.SetFloor(new GridPos(1, 1), FloorType.Wood);
            Assert.False(c.Build.Plant("sapling", new GridPos(1, 1), out _).Ok);
            m.SetOwned(new GridPos(5, 5), false);
            Assert.False(c.Build.Plant("sapling", new GridPos(5, 5), out _).Ok);
            m.SetFloor(new GridPos(6, 6), FloorType.Dirt);
            Assert.True(c.Build.Plant("sapling", new GridPos(6, 6), out _).Ok);
            Assert.False(c.Build.Plant("sapling", new GridPos(6, 6), out _).Ok);   // dolu
            Assert.Equal(4, c.House.Count("sapling"));
        }

        [Fact]
        public void Fidan_EnvanterdeYoksaDikilmez_DikilemeyenEsyaReddedilir()
        {
            var c = Setup();
            Assert.False(c.Build.Plant("sapling", new GridPos(3, 3), out _).Ok);
            c.House.AddItem("log", 1);
            Assert.False(c.Build.Plant("log", new GridPos(3, 3), out _).Ok);
        }

        // ---- büyüme ----

        [Fact]
        public void Buyume_DisaridaBuyur_KistaDurur()
        {
            var c = Setup();
            c.House.AddItem("sapling", 1);
            c.Build.Plant("sapling", new GridPos(3, 3), out var tree);
            c.Plants.Tick(5, Season.Spring);
            Assert.Equal(5.0, tree.GrowthDays, 3);
            c.Plants.Tick(5, Season.Winter);
            Assert.Equal(5.0, tree.GrowthDays, 3);
            c.Plants.Tick(10, Season.Summer);
            Assert.True(tree.IsGrown);
            Assert.Equal(12.0, tree.GrowthDays, 3);
            Assert.Equal(1f, tree.GrowthFraction);
        }

        [Fact]
        public void Buyume_EtrafinaDuvarOrulunceDurur()
        {
            var c = Setup();
            c.House.AddItem("sapling", 1);
            c.Build.Plant("sapling", new GridPos(3, 3), out var tree);
            var m = c.World.Map;
            m.SetEdge(new Edge(3, 3, EdgeAxis.Horizontal), EdgeKind.Wall);
            m.SetEdge(new Edge(3, 4, EdgeAxis.Horizontal), EdgeKind.Wall);
            m.SetEdge(new Edge(3, 3, EdgeAxis.Vertical), EdgeKind.Wall);
            m.SetEdge(new Edge(4, 3, EdgeAxis.Vertical), EdgeKind.Wall);
            c.Plants.Tick(5, Season.Spring);
            Assert.Equal(0.0, tree.GrowthDays, 3);
        }

        [Fact]
        public void Bitkiler_TasinmazSatilmazEnvantereAlinmaz()
        {
            var c = Setup();
            c.House.AddItem("sapling", 1);
            c.Build.Plant("sapling", new GridPos(3, 3), out var tree);
            Assert.False(c.Build.Move(tree.Id, new GridPos(5, 5), 0).Ok);
            Assert.False(c.Build.SellPlaced(tree.Id).Ok);
            Assert.False(c.Build.StoreToInventory(tree.Id).Ok);
        }

        // ---- kesme ve yarma ----

        [Fact]
        public void GencAgac_Kesilemez()
        {
            var c = Setup();
            c.House.AddItem("sapling", 1);
            c.Build.Plant("sapling", new GridPos(5, 5), out var tree);
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            var r = Runner(c, ch);
            r.Command(Job.UseItem(tree.Id, "chop"));
            r.Tick(0.01);
            Assert.Contains("büyümedi", r.LastFailure);
            Assert.NotNull(c.World.GetItem(tree.Id));
        }

        [Fact]
        public void OlgunAgac_Kesilir_KutukFidanXpVerir_AgacKalkar()
        {
            var c = Setup();
            var tree = c.World.PlaceItem(c.Db.Items.Get("oak"), new GridPos(5, 5));
            PlantSystem.SetGrown(tree);
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            string summary = null;
            c.Effects.Applied += (_, __, s) => summary = s;
            var r = Runner(c, ch);
            r.Command(Job.UseItem(tree.Id, "chop"));
            for (int i = 0; i < 50 && !r.IsIdle; i++) r.Tick(0.02);

            Assert.True(r.IsIdle);
            Assert.Null(r.LastFailure);
            Assert.Null(c.World.GetItem(tree.Id));
            Assert.False(c.World.Map.IsBlocked(new GridPos(5, 5)));
            Assert.Equal(3, c.House.Count("log"));
            Assert.Equal(1, c.House.Count("sapling"));
            Assert.True(ch.Skills.GetXp(SkillType.Woodcutting) >= 20);
            Assert.Contains("+3 Kütük", summary);
        }

        [Fact]
        public void EtkiUygulayicisiYoksa_IsBaslamaz()
        {
            var c = Setup();
            var tree = c.World.PlaceItem(c.Db.Items.Get("oak"), new GridPos(5, 5));
            PlantSystem.SetGrown(tree);
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            var r = new JobRunner(ch, c.World);
            r.Command(Job.UseItem(tree.Id, "chop"));
            r.Tick(0.01);
            Assert.NotNull(r.LastFailure);
        }

        [Fact]
        public void OdunYarma_KutukHarcar_UcOdunVerir_KutukYoksaReddeder()
        {
            var c = Setup();
            var block = c.World.PlaceItem(c.Db.Items.Get("block"), new GridPos(5, 5));
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            var r = Runner(c, ch);

            r.Command(Job.UseItem(block.Id, "split"));
            r.Tick(0.01);
            Assert.Equal("Kütük yok.", r.LastFailure);

            c.House.AddItem("log", 1);
            r.Command(Job.UseItem(block.Id, "split"));
            for (int i = 0; i < 50 && !r.IsIdle; i++) r.Tick(0.02);
            Assert.Equal(0, c.House.Count("log"));
            Assert.Equal(3, c.House.Count("firewood"));
            Assert.NotNull(c.World.GetItem(block.Id));
        }

        [Fact]
        public void Beceri_IsiHizlandirir_EnCokYariSure()
        {
            var c = Setup();
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            ch.SetAge(40);
            var r = Runner(c, ch);
            var chop = c.Db.Items.Get("oak").Interactions[0];
            Assert.Equal(0.15, r.EffectiveDuration(chop), 6);
            ch.GainSkillXp(SkillType.Woodcutting, SkillRules.XpForLevel(5));
            Assert.Equal(0.15 * 0.8, r.EffectiveDuration(chop), 6);
            ch.GainSkillXp(SkillType.Woodcutting, SkillRules.XpForLevel(10));
            Assert.Equal(0.15 * 0.55, r.EffectiveDuration(chop), 6);
        }

        [Fact]
        public void Otonomi_IsEtkilesiminiSecmez()
        {
            var c = Setup();
            var tree = c.World.PlaceItem(c.Db.Items.Get("oak"), new GridPos(5, 5));
            PlantSystem.SetGrown(tree);
            var npc = new Character(2, "Maud", new GridPos(1, 5), false);
            npc.Needs.Set(NeedType.Energy, 5f);
            var r = Runner(c, npc);
            r.Tick(0.5);
            Assert.NotNull(c.World.GetItem(tree.Id));
            Assert.Equal(0, c.House.Count("log"));
        }

        [Fact]
        public void OlasilikliUrun_BazenGelir()
        {
            var db = Content();
            db.Items.Get("oak").Interactions[0].Produce[1].Chance = 0.5f;
            var world = new WorldState(new GridMap(40, 6));
            var house = new Household();
            var fx = new InteractionEffects(world, house, db, seed: 7);
            var chop = db.Items.Get("oak").Interactions[0];
            for (int i = 0; i < 40; i++)
            {
                var t = world.PlaceItem(db.Items.Get("oak"), new GridPos(i, 0));
                fx.Apply(new Character(1, "A", new GridPos(0, 3)), t, chop);
            }
            Assert.Equal(120, house.Count("log"));
            Assert.InRange(house.Count("sapling"), 10, 30);
        }

        // ---- içerik ----

        [Fact]
        public void Icerik_DikimVeUretimBaglantilariDogrulanir()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "seed1", DisplayName = "x", Placement = ItemPlacement.Resource, PlantsAs = "yok" });
            db.Items.Register(new ItemDef { Id = "chair", DisplayName = "x" });
            db.Items.Register(new ItemDef { Id = "seed2", DisplayName = "x", Placement = ItemPlacement.Resource, PlantsAs = "chair" });
            db.Items.Register(new ItemDef
            {
                Id = "w", DisplayName = "x",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "a", DisplayName = "a", DurationDays = 0.1, Produce = new List<ProduceDef> { new ProduceDef { ItemId = "hayalet" } } },
                    new InteractionDef { Id = "b", DisplayName = "b", DurationDays = 0.1, RequiresGrown = true, RemovesItem = true }
                }
            });
            var errors = db.Validate();
            Assert.Contains(errors, e => e.Contains("'yok' bulunamadı"));
            Assert.Contains(errors, e => e.Contains("bir bitki değil"));
            Assert.Contains(errors, e => e.Contains("'hayalet' bulunamadı"));
            Assert.Contains(errors, e => e.Contains("requiresGrown"));
        }

        [Fact]
        public void Mapper_IhtiyacsizIsEtkilesiminiYukler_VarsayilanlarUygulanir()
        {
            var file = new ContentFileDto
            {
                items = new[]
                {
                    new ItemDto { id = "log", displayName = "Kütük", placement = "Resource" },
                    new ItemDto
                    {
                        id = "oak", displayName = "Meşe", growDays = 12,
                        interactions = new[]
                        {
                            new InteractionDto { id = "chop", displayName = "Kes", durationDays = 0.1, requiresGrown = true, removeItem = true,
                                produce = new[] { new ProduceDto { item = "log", count = 3 } } }
                        }
                    }
                }
            };
            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(file, db);
            Assert.Empty(errors);
            var chop = db.Items.Get("oak").Interactions[0];
            Assert.True(chop.HasEffects);
            Assert.False(chop.RestoresNeed);
            Assert.Equal(1f, chop.Produce[0].Chance);
            Assert.True(db.Items.Get("oak").IsPlant);
        }
    }
}
