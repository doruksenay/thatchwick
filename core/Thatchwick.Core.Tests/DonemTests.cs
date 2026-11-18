using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Dönem tutarlılığı (17.11.2026): hayali yerleşim adı, ad düzeltmeleri, masal anlatma, tahıl ve keten,
    // mevsimlik hayvan ürünü, ömür oranları, yılan balığı ve turna.
    public class DonemTests
    {
        private static ContentDatabase Db()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            return db;
        }

        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);

        // ---------- yerleşim adı ----------

        [Fact]
        public void Yerlesim_AdKurallari()
        {
            Assert.Null(Settlement.Validate("Kızılçam"));
            Assert.Null(Settlement.Validate("Ash-on-Mere"));
            Assert.NotNull(Settlement.Validate(""));
            Assert.NotNull(Settlement.Validate("A"));
            Assert.NotNull(Settlement.Validate("1Köy"));
            Assert.NotNull(Settlement.Validate("Köy!"));
            Assert.NotNull(Settlement.Validate(new string('a', Settlement.MaxLength + 1)));
            Assert.Equal(Settlement.DefaultName, Settlement.Clean("??"));
            Assert.Equal("Saztepe", Settlement.Clean("  Saztepe "));
            Assert.Contains(Settlement.Random(new System.Random(1)), Settlement.Suggestions);
        }

        [Fact]
        public void Yerlesim_AdiKaydaGirer()
        {
            var db = Db();
            var w = new WorldState(new GridMap(8, 8));
            var head = new Character(1, "Elif", new GridPos(1, 1), true);
            head.SetAge(30);
            var dynasty = new Dynasty { Name = "Elif hanedanı", SettlementName = "Değirmendere" };
            dynasty.Found(head, 0);
            var data = SaveGame.Capture(new GameClock(), w, new Household(0), new[] { head }, null, null, null, dynasty: dynasty);
            var opts = new JsonSerializerOptions { IncludeFields = true };
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, opts), opts);
            var back = SaveGame.Load(data, db).Dynasty;
            Assert.Equal("Değirmendere", back.SettlementName);
            Assert.Equal("Elif hanedanı", back.Name);

            data.dynasty.settlement = null;                       // eski kayıt
            Assert.Equal(Settlement.DefaultName, SaveGame.Load(data, db).Dynasty.SettlementName);
        }

        // ---------- ad düzeltmeleri ----------

        [Fact]
        public void Anakronizmler_Duzeltildi()
        {
            var db = Db();
            Assert.Equal("Duvar kandili", db.Items.Get("item.oil_lamp_wall").DisplayName);
            Assert.Equal("Ot tınazı", db.Items.Get("item.hay_bale").DisplayName);
            Assert.Equal("Buz çukuru", db.Items.Get("item.ice_house").DisplayName);
            Assert.Equal("Soğuk kiler", db.Items.Get("item.ice_box").DisplayName);
            Assert.Equal("Su kabağı", db.Items.Get("item.pumpkin").DisplayName);
            Assert.False(db.Items.Contains("item.story_book"));
            foreach (var d in db.Items.All)
                foreach (var word in new[] { "Gaz lambası", "balya", "Buzhane", "Buzluk", "Masal kitabı", "Kabak" })
                    Assert.DoesNotContain(word, d.DisplayName);
            Assert.True(db.Items.Get("item.fish_eel").HasTag("nobreed"));
            Assert.True(db.Items.Get("item.fish_pike").HasTag("predator"));
            // Bulgur buğdaydan.
            var pot = db.Items.Get("item.cooking_pot");
            Assert.Contains(Inter(pot, "make_pilaf").Consume, m => m.ItemId == "item.wheat");
        }

        [Fact]
        public void Masal_BuyukCocugaAnlatir()
        {
            var db = Db();
            var pop = new Population(1, seed: 2);
            var house = new Household(0);
            var fam = new FamilyService(pop, house, db);
            var mother = new Character(1, "Esma", new GridPos(1, 1)) { MoodEnabled = true };
            mother.SetAge(40);
            var kid = new Character(2, "Alp", new GridPos(1, 2)) { MoodEnabled = true };
            kid.SetAge(15);
            Assert.Equal(AgeStage.Child, kid.Stage);
            pop.Add(mother); pop.Add(kid);
            Assert.NotNull(fam.CanDo(kid, mother, SocialAction.TellStory));      // çocuk anlatamaz
            Assert.Null(fam.CanDo(mother, kid, SocialAction.TellStory));
            kid.Needs.Set(NeedType.Fun, 10f);
            fam.Apply(mother, kid, SocialAction.TellStory);
            Assert.True(kid.Needs.Get(NeedType.Fun) >= 10f + FamilyService.StoryFun - 1f);
            Assert.True(mother.Skills.GetXp(SkillType.Parenting) > 0);
        }

        // ---------- gölet: yılan balığı ve turna ----------

        [Fact]
        public void Golet_YilanBaligiUremez_TurnaOburleriniYer()
        {
            var db = Db();
            var map = new GridMap(10, 10);
            for (int x = 2; x < 6; x++) for (int y = 2; y < 5; y++) map.SetFloor(new GridPos(x, y), FloorType.Water);
            var a = new GridPos(2, 2);
            map.SetSpring(a);
            var pond = map.PondAt(a);
            map.RestorePond(a, 1f, true, true, 10, new[] { new KeyValuePair<string, int>("item.fish_eel", 4), new KeyValuePair<string, int>("item.fish_carp", 4) });
            pond = map.PondAt(a);
            System.Func<string, bool> breeds = id => !db.Items.Get(id).HasTag("nobreed");
            System.Func<string, bool> predator = id => db.Items.Get(id).HasTag("predator");
            var rng = new System.Random(4);
            for (int d = 0; d < 15; d++) map.OnNewDayPonds(0f, 14f, false, rng, breeds, predator);
            Assert.Equal(4, pond.Fish["item.fish_eel"]);                          // yılan balığı denize göç edip ürer
            Assert.True(pond.Fish["item.fish_carp"] > 4);

            pond.AddFish("item.fish_pike", 3);
            int prey = pond.FishTotal - pond.Fish["item.fish_pike"];
            var notes = new List<string>();
            for (int d = 0; d < 10; d++) notes.AddRange(map.OnNewDayPonds(0f, 14f, true, rng, breeds, predator));   // kış: üreme yok
            Assert.True(pond.FishTotal - pond.Fish["item.fish_pike"] < prey, "turna avlanmadı");
            Assert.Contains(notes, n => n.Contains("Turna"));
        }

        // ---------- tahıl ve keten ----------

        [Fact]
        public void Tahil_BugdayCavdarKisiToprakta_GecirirYulafKetenDonar()
        {
            var db = Db();
            foreach (var id in new[] { "item.crop_wheat", "item.crop_rye" })
            {
                var c = db.Items.Get(id);
                Assert.False(c.DiesInWinter, id);
                Assert.Contains(Season.Autumn, c.GrowSeasons);
                Assert.DoesNotContain(Season.Winter, c.GrowSeasons);       // kışın uyur, ilkbaharda sürer
            }
            Assert.True(db.Items.Get("item.crop_oats").DiesInWinter);
            Assert.True(db.Items.Get("item.crop_flax").DiesInWinter);

            var world = new WorldState(new GridMap(8, 8));
            var plants = new PlantSystem(world);
            var wheat = world.PlaceItem(db.Items.Get("item.crop_wheat"), new GridPos(2, 2));
            var oats = world.PlaceItem(db.Items.Get("item.crop_oats"), new GridPos(5, 5));
            plants.Tick(1.0, Season.Winter);
            Assert.NotNull(world.GetItem(wheat.Id));                         // kışlık buğday kışı geçirir
            Assert.Null(world.GetItem(oats.Id));                             // yulaf donar
        }

        [Fact]
        public void Tahil_HasatSamanVerir_ElDegirmeni_KarmaEkmek_Lapa()
        {
            var db = Db();
            foreach (var id in new[] { "item.crop_wheat", "item.crop_rye", "item.crop_oats", "item.crop_barley" })
                Assert.Contains(Inter(db.Items.Get(id), "harvest").Produce, p => p.ItemId == "item.straw");

            var w = new WorldState(new GridMap(10, 10));
            var house = new Household(0);
            var fx = new InteractionEffects(w, house, db);
            var quern = w.PlaceItem(db.Items.Get("item.quern"), new GridPos(2, 2));
            var c = new Character(1, "Kaya", new GridPos(1, 1));
            c.SetAge(30);
            house.AddItem("item.wheat", 2);
            house.AddItem("item.rye", 2);
            Assert.Equal("Köylü", quern.Def.StyleSet);
            foreach (var i in new[] { "quern_wheat", "quern_rye" })
            {
                var inter = Inter(quern.Def, i);
                Assert.Null(fx.CanStart(c, quern, inter));
                fx.Apply(c, quern, inter);
            }
            Assert.True(house.Count("item.flour") >= 1);
            Assert.True(house.Count("item.rye_flour") >= 1);

            var oven = db.Items.Get("item.stone_oven");
            Assert.Contains(Inter(oven, "bake_maslin").Consume, m => m.ItemId == "item.rye_flour");
            Assert.Contains(Inter(oven, "bake_maslin").Consume, m => m.ItemId == "item.flour");
            var pot = db.Items.Get("item.cooking_pot");
            Assert.Contains(Inter(pot, "make_porridge_oats").Consume, m => m.ItemId == "item.oats");
            Assert.Equal("item.porridge_oats", Inter(pot, "eat_porridge_oats").ConsumeItem);
            Assert.True(db.Items.Get("item.bread_rye").SpoilDays > db.Items.Get("item.bread").SpoilDays);   // çavdar ekmeği dayanıklı
        }

        [Fact]
        public void Keten_EgrilirDokunur_HavlulukVeYelDegirmeniKetenIster()
        {
            var db = Db();
            bool Makes(string station, string product, string material) =>
                db.Items.Get(station).Interactions.Any(i => i.IsCraft && i.Produce.Any(p => p.ItemId == product) && (material == null || i.Consume.Any(m => m.ItemId == material)));
            Assert.True(Makes("item.spinning_wheel", "item.linen_yarn", "item.flax"));
            Assert.True(Makes("item.loom", "item.linen", "item.linen_yarn"));
            Assert.True(Makes("item.workbench", "item.towel_rack", "item.linen"));
            Assert.True(Makes("item.workbench", "item.windmill", "item.linen"));
            Assert.True(Makes("item.seed_maker", "item.feed", "item.oats"));
            Assert.True(Makes("item.loom", "item.mattress_straw", "item.straw"));
            Assert.Equal("item.straw", db.Parts.Get("floor_straw").MaterialItem);
        }

        // ---------- hayvan: mevsim ve ömür ----------

        [Fact]
        public void Hayvan_KisinAzUretir_KoyunKisinKirkilmaz()
        {
            var db = Db();
            var hen = db.Animals.Get("animal.chicken");
            Assert.True(hen.OutputFactor(Season.Winter) < 0.5f);
            Assert.Equal(1f, hen.OutputFactor(Season.Spring));
            Assert.Equal(0f, db.Animals.Get("animal.sheep").OutputFactor(Season.Winter));
            Assert.Equal(0f, db.Animals.Get("animal.bees").OutputFactor(Season.Winter));
            Assert.True(db.Animals.Get("animal.sheep").ProduceEveryDays >= 10f);
            Assert.Equal(1f, db.Animals.Get("animal.dog").OutputFactor(Season.Winter));   // tanımsız: 1

            // Kışın yumurta yavaş birikir.
            float Progress(Season s)
            {
                var w = new WorldState(new GridMap(10, 10));
                var animals = new AnimalSystem(w, db, seed: 3) { SeasonNow = () => s };
                var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
                AnimalSystem.AddFeed(coop, 100);
                var a = animals.Restore(1, hen, new GridPos(4, 4), coop.Id, 100f, 5, 0, 0, friendship: 0f);
                animals.Tick(0.5);
                return (float)a.ProduceProgress;
            }
            Assert.True(Progress(Season.Winter) < Progress(Season.Spring) * 0.5f);
        }

        [Fact]
        public void Omur_OranlarTutarli()
        {
            var db = Db();
            var horse = db.Animals.Get("animal.horse");
            var hen = db.Animals.Get("animal.chicken");
            Assert.True(horse.LifespanDays < new AgeConfig().EarliestNaturalDeathDay * 0.6, "at insandan uzun yaşamamalı");
            Assert.True(hen.LifespanDays < horse.LifespanDays / 2);
            foreach (var a in db.Animals.All) Assert.True(a.MatureDays < a.LifespanDays / 2, a.Id);
            Assert.True(new Character(1, "X", new GridPos(0, 0)).PregnancyDays <= 6);
        }
    }
}
