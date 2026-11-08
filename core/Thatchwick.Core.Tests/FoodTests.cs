using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class FoodTests
    {
        private static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2 });
            db.Items.Register(new ItemDef { Id = "seed", DisplayName = "Şalgam tohumu", Placement = ItemPlacement.Resource, Price = 1, PlantsAs = "crop" });
            db.Items.Register(new ItemDef
            {
                Id = "crop", DisplayName = "Şalgam ekini", GrowDays = 6f, NeedsTilledSoil = true, DiesInWinter = true,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "harvest", DisplayName = "Hasat et", DurationDays = 0.05, Skill = SkillType.Farming, SkillXp = 15,
                        RequiresGrown = true, RemovesItem = true,
                        Produce = new List<ProduceDef> { new ProduceDef { ItemId = "turnip", Count = 3 }, new ProduceDef { ItemId = "seed", Count = 1 } }
                    }
                }
            });
            db.Items.Register(new ItemDef
            {
                Id = "pot", DisplayName = "Kazan",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "eat", DisplayName = "Şalgam ye", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.1, ConsumeItem = "turnip" }
                }
            });
            return db;
        }

        private static (ContentDatabase db, WorldState w, Household h, BuildService b, PlantSystem p, InteractionEffects fx) Setup()
        {
            var db = Content();
            Assert.Empty(db.Validate());
            var w = new WorldState(new GridMap(16, 10));
            var h = new Household(50);
            return (db, w, h, new BuildService(w, h, db), new PlantSystem(w), new InteractionEffects(w, h, db, 5));
        }

        [Fact]
        public void Ekin_YalnizcaTopragaEkilir()
        {
            var (_, w, h, b, _, _) = Setup();
            h.AddItem("seed", 2);
            Assert.False(b.Plant("seed", new GridPos(2, 2), out _).Ok);          // çim
            w.Map.SetFloor(new GridPos(2, 2), FloorType.Dirt);
            Assert.True(b.Plant("seed", new GridPos(2, 2), out _).Ok);
        }

        [Fact]
        public void Ekin_Buyur_HasatSalgamVeTohumVerir()
        {
            var (_, w, h, b, p, fx) = Setup();
            h.AddItem("seed", 1);
            w.Map.SetFloor(new GridPos(5, 5), FloorType.Dirt);
            b.Plant("seed", new GridPos(5, 5), out var crop);
            p.Tick(6, Season.Summer);
            Assert.True(crop.IsGrown);

            var ch = new Character(1, "A", new GridPos(1, 5), true);
            ch.SetAge(30);
            var r = new JobRunner(ch, w) { Effects = fx };
            r.Command(Job.UseItem(crop.Id, "harvest"));
            for (int i = 0; i < 50 && !r.IsIdle; i++) r.Tick(0.02);
            Assert.Equal(3, h.Count("turnip"));
            Assert.Equal(1, h.Count("seed"));
            Assert.Null(w.GetItem(crop.Id));
            Assert.True(ch.Skills.GetXp(SkillType.Farming) >= 15);
        }

        [Fact]
        public void Kis_HasatEdilmemisEkiniDondurur_AgacaDokunmaz()
        {
            var (db, w, h, b, p, _) = Setup();
            h.AddItem("seed", 1);
            w.Map.SetFloor(new GridPos(5, 5), FloorType.Dirt);
            b.Plant("seed", new GridPos(5, 5), out var crop);
            var tree = w.PlaceItem(new ItemDef { Id = "oak", DisplayName = "Meşe", GrowDays = 12f }, new GridPos(8, 8));
            int died = 0;
            p.PlantDied += (def, pos) => died++;
            p.Tick(0.1, Season.Winter);
            Assert.Null(w.GetItem(crop.Id));
            Assert.NotNull(w.GetItem(tree.Id));
            Assert.Equal(1, died);
        }

        [Fact]
        public void Yemek_SalgamHarcar_YoksaReddeder()
        {
            var (db, w, h, _, _, fx) = Setup();
            var pot = w.PlaceItem(db.Items.Get("pot"), new GridPos(5, 5));
            var ch = new Character(1, "A", new GridPos(1, 5), true);
            ch.Needs.Set(NeedType.Hunger, 20f);
            var r = new JobRunner(ch, w) { Effects = fx };
            r.Command(Job.UseItem(pot.Id, "eat"));
            r.Tick(0.01);
            Assert.Equal("Şalgam yok.", r.LastFailure);

            h.AddItem("turnip", 2);
            r.Command(Job.UseItem(pot.Id, "eat"));
            for (int i = 0; i < 30 && !r.IsIdle; i++) r.Tick(0.01);
            Assert.Equal(1, h.Count("turnip"));
            Assert.True(ch.Needs.Get(NeedType.Hunger) > 50f);
        }

        [Fact]
        public void Otonomi_SalgamVarsaYer_YoksaGitmez()
        {
            var (db, w, h, _, _, fx) = Setup();
            w.PlaceItem(db.Items.Get("pot"), new GridPos(5, 5));
            var npc = new Character(2, "Maud", new GridPos(1, 5), false);
            npc.Needs.Set(NeedType.Hunger, 10f);
            var r = new JobRunner(npc, w) { Effects = fx };
            r.Tick(0.05);
            Assert.True(r.IsIdle);                                  // şalgam yok: gitmez
            h.AddItem("turnip", 1);
            for (int i = 0; i < 30; i++) r.Tick(0.01);
            Assert.Equal(0, h.Count("turnip"));
            Assert.True(npc.Needs.Get(NeedType.Hunger) > 25f);
        }

        [Fact]
        public void Aclik_UzunSurerseOldurur_Kapaliyken_Oldurmez()
        {
            var c = new Character(1, "A", new GridPos(0, 0)) { CanStarve = true };
            c.SetAge(30);
            for (int i = 0; i < 60 && !c.IsDead; i++) c.Tick(0.1);
            Assert.True(c.IsDead);
            Assert.Equal(DeathCause.Starvation, c.CauseOfDeath);

            var d = new Character(2, "B", new GridPos(0, 0));
            d.SetAge(30);
            for (int i = 0; i < 60; i++) d.Tick(0.1);
            Assert.False(d.IsDead);
        }

        [Fact]
        public void Bebek_BakimsizKalirsaAcliktanOlur()
        {
            // Adım 16'dan beri bebeği büyükler besler (FamilyService); beslenmeyen bebek ölebilir.
            var c = new Character(1, "Bebek", new GridPos(0, 0)) { CanStarve = true };
            for (int i = 0; i < 60 && !c.IsDead; i++) c.Tick(0.1);
            Assert.True(c.IsDead);
            Assert.Equal(DeathCause.Starvation, c.CauseOfDeath);
        }
    }
}
