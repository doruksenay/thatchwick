using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Doğa: bodrum mantar yatağı, arı tozlaşması, hayvan dostluğu, yağmurlu toplama, yeni ağaç türleri ve toplama düğümleri.
    public class NatureTests
    {
        private static ContentDatabase Real()
        {
            var db = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(ContentFilesTests.LoadAll(), db));
            return db;
        }

        private static WorldState World(int size = 24)
        {
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            return new WorldState(map);
        }

        private static void Box(GridMap m, int x0, int y0, int x1, int y1, int z = 0)
        {
            for (int x = x0; x <= x1; x++)
            {
                m.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal, z), EdgeKind.Wall);
                m.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), EdgeKind.Wall);
            }
            for (int y = y0; y <= y1; y++)
            {
                m.SetEdge(new Edge(x0, y, EdgeAxis.Vertical, z), EdgeKind.Wall);
                m.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical, z), EdgeKind.Wall);
            }
        }

        // ---------- bodrum mantar yatağı ----------

        [Fact]
        public void MantarYatagi_YalnizBodrumdaDurur_MevsimsizBuyurVeTekrarVerir()
        {
            var db = Real();
            var w = World();
            var bed = db.Items.Get("item.mushroom_bed");
            Assert.False(w.CanPlaceFloor(bed, new GridPos(5, 5, 0), 0).Ok);       // zemin katta olmaz

            var svc = new BuildService(w, new Household(500), db);
            // Bodrum: merdivenle aç, yanını kaz.
            Assert.True(svc.BuildLevelTile(new GridPos(6, 6, -1), stairEnd: true).Ok);
            Assert.True(svc.BuildLevelTile(new GridPos(7, 6, -1)).Ok);
            Assert.True(w.CanPlaceFloor(bed, new GridPos(7, 6, -1), 0).Ok);
            var inst = w.PlaceItem(bed, new GridPos(7, 6, -1));

            var plants = new PlantSystem(w);
            plants.Tick(3, Season.Winter);                                         // kış: bodrumda yine büyür
            Assert.True(inst.IsGrown);
            plants.Tick(3.9, Season.Winter);
            Assert.False(inst.FruitReady);
            plants.Tick(0.2, Season.Winter);
            Assert.True(inst.FruitReady);

            var house = new Household();
            var fx = new InteractionEffects(w, house, db, 3) { Plants = plants };
            var c = new Character(1, "A", new GridPos(6, 6, -1)); c.SetAge(30);
            var r = new JobRunner(c, w) { Effects = fx };
            r.Command(Job.UseItem(inst.Id, "pick"));
            for (int i = 0; i < 60 && !r.IsIdle; i++) r.Tick(0.02);
            Assert.Equal(3, house.Count("item.mushroom"));
            Assert.False(inst.FruitReady);
        }

        // ---------- arı tozlaşması ----------

        [Fact]
        public void Kovan_YakindakiEkiniVeAgaciHizlandirir_KoloniYoksaBosa()
        {
            var db = Real();
            var w = World();
            var plants = new PlantSystem(w);
            var hive = w.PlaceItem(db.Items.Get("item.beehive"), new GridPos(6, 5));
            var near = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(5, 5));
            var far = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(18, 18));
            PlantSystem.Water(near); PlantSystem.Water(far);

            plants.HiveActive = h => true;
            plants.Tick(1, Season.Summer);
            Assert.Equal(1.1, near.GrowthDays, 3);
            Assert.Equal(1.0, far.GrowthDays, 3);

            var near2 = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(5, 6));
            PlantSystem.Water(near2);
            var plants2 = new PlantSystem(w) { HiveActive = h => false };          // koloni yok: tozlaşma yok
            plants2.Tick(1, Season.Summer);
            Assert.Equal(1.0, near2.GrowthDays, 3);
        }

        // ---------- hayvan dostluğu ----------

        [Fact]
        public void Hayvan_SevilinceDostlukArtar_GunlukBir_VeUrunuHizlandirir()
        {
            var db = Real();
            var w = World();
            var house = new Household(500);
            var animals = new AnimalSystem(w, db, seed: 3);
            var build = new BuildService(w, house, db) { HousingResidents = animals.Residents };
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            Assert.True(animals.Buy("animal.chicken", house, out var hen).Ok);
            Assert.Equal(0f, hen.Friendship);

            var fx = new InteractionEffects(w, house, db, 1) { Animals = animals };
            var c = new Character(1, "A", new GridPos(3, 5)); c.SetAge(30);
            var runner = new JobRunner(c, w) { Effects = fx };
            runner.Command(Job.UseItem(coop.Id, "pet_animals"));
            for (int i = 0; i < 60 && !runner.IsIdle; i++) runner.Tick(0.02);
            Assert.Equal(AnimalSystem.PetFriendship, hen.Friendship);

            // Aynı gün tekrar sevilemez.
            var pet = coop.Def.Interactions.First(i => i.PetsAnimals);
            Assert.Equal("Hayvanlar bugün zaten sevildi.", fx.CanStart(c, coop, pet));
            animals.Tick(1.1);
            Assert.Null(fx.CanStart(c, coop, pet));
        }

        [Fact]
        public void Hayvan_AclikDostlugaZarar_TokluktaDostlukBuyur_DostUrunuHizlanir()
        {
            var db = Real();
            var w = World();
            var house = new Household(500);
            var animals = new AnimalSystem(w, db, seed: 3);
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            animals.Buy("animal.chicken", house, out var hen);
            AnimalSystem.AddFeed(coop, 100);
            for (int i = 0; i < 10; i++) animals.Tick(1.0);
            Assert.True(hen.Friendship >= 25f);                                   // tokken günde +3

            // Yemsiz barınaktaki hayvan aç kalır, dostluğu düşer.
            var coop2 = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(12, 12));
            var hungry = animals.Restore(10, db.Animals.Get("animal.chicken"), new GridPos(11, 12), coop2.Id, 100f, 10, 0, 0, friendship: 50f);
            animals.Tick(1.0);
            animals.Tick(1.0);
            Assert.True(hungry.Fullness < 20f);
            Assert.True(hungry.Friendship < 50f, $"aç kalan hayvanın dostluğu düşmeli: {hungry.Friendship}");
        }

        [Fact]
        public void Hayvan_DostUrunuDahaCabukVerir()
        {
            var db = Real();
            var w = World();
            var animals = new AnimalSystem(w, db, seed: 3);
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            AnimalSystem.AddFeed(coop, 100);
            var species = db.Animals.Get("animal.chicken");
            var stranger = animals.Restore(1, species, new GridPos(4, 4), coop.Id, 100f, 10, 0, 0, friendship: 0f);
            var friend = animals.Restore(2, species, new GridPos(4, 5), coop.Id, 100f, 10, 0, 0, friendship: 100f);
            animals.Tick(0.5);
            Assert.Equal(0.5, stranger.ProduceProgress, 2);
            Assert.Equal(0.7, friend.ProduceProgress, 2);                          // 1 + 0,4 x dostluk
        }

        // ---------- yağmurlu toplama ----------

        [Fact]
        public void Toplama_YagmurNemSeverTurleriArtirir()
        {
            var db = Real();
            int dry = 0, wet = 0;
            for (int seed = 1; seed <= 8; seed++)
            {
                var w1 = World(32); var f1 = new ForageSystem(w1, db, seed);
                var w2 = World(32); var f2 = new ForageSystem(w2, db, seed);
                var mush = db.Items.Get("item.forage_mushroom");
                var garlic = db.Items.Get("item.forage_wild_garlic");
                f1.OnNewDay(Season.Autumn, rained: false);
                f2.OnNewDay(Season.Autumn, rained: true);
                dry += f1.Count(mush);
                wet += f2.Count(mush);
                f1.OnNewDay(Season.Spring, rained: false);
                f2.OnNewDay(Season.Spring, rained: true);
            }
            Assert.True(wet > dry, $"yağmurlu gün daha çok mantar vermeli: kuru {dry}, yağmurlu {wet}");
        }

        // ---------- yeni içerik ----------

        [Fact]
        public void GercekIcerik_MevsimlikToplamaDugumleriHerMevsimdeVar()
        {
            var db = Real();
            foreach (Season s in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            {
                var nodes = db.Items.All.Where(i => i.IsForage && i.ForageSeasons.Contains(s)).ToList();
                Assert.True(nodes.Count >= 3, $"{s}: en az 3 toplama düğümü olmalı (var {nodes.Count})");
            }
            Assert.True(db.Items.Get("item.forage_mushroom").HasTag("rain_loving"));
            Assert.True(db.Items.Get("item.beehive").PollinateRadius > 0);
            foreach (var n in db.Items.All.Where(i => i.IsForage))
                Assert.True(n.Interactions.Any(i => i.RemovesItem && i.Produce.Count > 0), n.Id + " ürün vermiyor");
        }

        [Fact]
        public void GercekIcerik_AgacTurleri_FarkliSureVeUrun()
        {
            var db = Real();
            var oak = db.Items.Get("item.tree_oak");
            var birch = db.Items.Get("item.tree_birch");
            var pine = db.Items.Get("item.tree_pine");
            var willow = db.Items.Get("item.tree_willow");
            Assert.True(birch.GrowDays < oak.GrowDays && oak.GrowDays < pine.GrowDays);
            int Logs(ItemDef t) => t.Interactions.First(i => i.Id == "chop").Produce.First(p => p.ItemId == "item.log").Count;
            Assert.True(Logs(pine) > Logs(oak) && Logs(oak) > Logs(birch) - 1);
            Assert.True(pine.Interactions.First(i => i.Id == "chop").Produce.Any(p => p.ItemId == "item.resin"));
            Assert.True(willow.Interactions.First(i => i.Id == "chop").Produce.Any(p => p.ItemId == "item.withy"));
            foreach (var t in new[] { birch, pine, willow })
                Assert.True(db.Items.All.Any(i => i.PlantsAs == t.Id), t.Id + " fidanı yok");
        }
    }
}
