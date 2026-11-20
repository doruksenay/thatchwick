using System.Collections.Generic;
using System.Linq;
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
    // Tarım parti 1 (Project belgeleri 58 §4, 59, 59a): orak ve tırpan, hasat emirleri, seçme tohum düzeltmesi,
    // safranın şafak kuralı, tohum kalburu.
    public class HasatEmriTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db; public Household House; public WorldState World; public LifeSystems Life;
            public InteractionEffects Fx; public PlantSystem Plants; public GameClock Clock;
            public List<Job> Commands = new List<Job>();
            /// <summary>true ise emredilen hasat hemen uygulanır (iş koşucusunun yerine).</summary>
            public bool AutoRun = true;
        }

        private static Kit Make()
        {
            var k = new Kit { Db = ContentFilesTests.LoadDb(out var errors), House = new Household(50), Clock = new GameClock() };
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            k.World = new WorldState(map);
            k.Life = new LifeSystems(k.Db, k.World, k.House, new Population(), k.Clock, 5);
            k.Plants = new PlantSystem(k.World);
            k.Fx = new InteractionEffects(k.World, k.House, k.Db, seed: 4) { Plants = k.Plants };
            k.Life.Attach(k.Fx);
            k.Life.Fields.Command = (c, job) =>
            {
                k.Commands.Add(job);
                if (!k.AutoRun) return true;
                var item = k.World.GetItem(job.ItemId);
                k.Fx.Apply(c, item, item.Def.Interactions.First(i => i.Id == job.InteractionId));
                return true;
            };
            return k;
        }

        private static Character Farmer(int level, int id = 1)
        {
            var c = new Character(id, "Çiftçi", new GridPos(1, 1), true) { CanStarve = false };
            if (level > 1) c.Skills.RestoreXp(SkillType.Farming, SkillRules.XpForLevel(level));
            return c;
        }

        private static ItemInstance Ripe(Kit k, string cropId, int x, int y)
        {
            var item = k.World.PlaceItem(k.Db.Items.Get(cropId), new GridPos(x, y));
            PlantSystem.SetGrown(item);
            return item;
        }

        // ---- içerik ----

        [Fact]
        public void HasatAleti_Orak_CapaYalnizCapalama()
        {
            var db = ContentFilesTests.LoadDb(out _);
            Assert.True(db.Tools.TryGet("sickle", out var sickle) && sickle.Tiers.Count == 3);
            Assert.True(db.Tools.TryGet("scythe", out var scythe) && scythe.Tiers.Count == 3);
            foreach (var crop in db.Items.All.Where(d => d.IsCrop))
                foreach (var i in crop.Interactions.Where(i => i.Id == "harvest" || i.Id == "select_seed"))
                    Assert.True(i.Tool != "hoe", $"{crop.Id}.{i.Id} hâlâ çapayla");
        }

        [Fact]
        public void SecmeTohum_HerEkinKendiUrununuVeTohumunuVerir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            int checkedCrops = 0;
            foreach (var crop in db.Items.All.Where(d => d.IsCrop))
            {
                var harvest = crop.Interactions.FirstOrDefault(i => i.Id == "harvest");
                var select = crop.Interactions.FirstOrDefault(i => i.Id == "select_seed");
                var seed = db.Items.All.FirstOrDefault(d => d.PlantsAs == crop.Id);
                if (harvest == null || select == null || seed == null) continue;
                checkedCrops++;
                Assert.Equal(harvest.Produce[0].ItemId, select.Produce[0].ItemId);                 // kendi ürünü
                Assert.Contains(select.Produce, p => p.ItemId == seed.Id && p.Count == 3);         // kendi tohumu
                Assert.DoesNotContain(select.Produce, p => p.ItemId != harvest.Produce[0].ItemId && p.ItemId != seed.Id);
            }
            Assert.True(checkedCrops >= 20, checkedCrops.ToString());
            // 58 §4 bulgu 1: kendir keten veriyordu.
            var hemp = db.Items.Get("item.crop_hemp").Interactions.First(i => i.Id == "select_seed");
            Assert.Equal("item.hemp", hemp.Produce[0].ItemId);
            Assert.Contains(hemp.Produce, p => p.ItemId == "item.seed_hemp");
        }

        [Fact]
        public void TohumKalburu_UrunuTohumaCevirir()
        {
            var k = Make();
            var sieve = k.Db.Items.Get("item.seed_maker");
            Assert.True(sieve.Interactions.Count >= 20);
            var carrot = sieve.Interactions.First(i => i.Id == "sieve_carrot");
            var wheat = sieve.Interactions.First(i => i.Id == "sieve_wheat");
            Assert.Equal("item.wheat", wheat.AllConsumed().Single().ItemId);   // demet değil tane elenir
            var placed = k.World.PlaceItem(sieve, new GridPos(5, 5));
            k.House.AddItem("item.carrot", 1);
            k.Fx.Apply(Farmer(1), placed, carrot);
            Assert.Equal(0, k.House.Count("item.carrot"));
            Assert.Equal(2, k.House.Count("item.seed_carrot"));
        }

        [Fact]
        public void Safran_SafaktaBirTelFazla()
        {
            int Pick(double hour)
            {
                var k = Make();
                k.Fx.HourOfDay = () => hour;
                var crop = Ripe(k, "item.crop_saffron", 4, 4);
                k.Fx.Apply(Farmer(1), crop, HarvestOrders.HarvestOf(crop.Def));
                return k.House.Count("item.saffron");
            }
            Assert.Equal(Pick(14) + 1, Pick(6));
        }

        // ---- hasat emirleri ----

        [Fact]
        public void TarlayiBic_BitisikOlgunEkinlerinHepsi_Seviye3()
        {
            var k = Make();
            // 2x3 olgun şalgam tarlası + ayrı duran bir lahana + tarlaya bitişik ama olgun olmayan bir ekin.
            var field = new List<ItemInstance>();
            for (int x = 3; x <= 4; x++) for (int y = 3; y <= 5; y++) field.Add(Ripe(k, "item.crop_turnip", x, y));
            var apart = Ripe(k, "item.crop_cabbage", 9, 9);
            var unripe = k.World.PlaceItem(k.Db.Items.Get("item.crop_turnip"), new GridPos(5, 3));
            Assert.Equal(6, HarvestOrders.RipeField(k.World, field[0]).Count);

            int finished = 0;
            k.Life.Fields.Finished += (c, n) => finished = n;
            var farmer = Farmer(HarvestOrders.FieldLevel);
            Assert.Equal(6, k.Life.Fields.OrderFrom(farmer, field[0]));
            Assert.Equal(6, k.Commands.Count);                                   // sırayla altısı da emredildi
            Assert.All(k.Commands, j => Assert.Equal(HarvestOrders.HarvestId, j.InteractionId));
            Assert.All(field, f => Assert.Null(k.World.GetItem(f.Id)));
            Assert.NotNull(k.World.GetItem(apart.Id));
            Assert.NotNull(k.World.GetItem(unripe.Id));
            Assert.True(k.House.Count("item.turnip") >= 18);
            Assert.Equal(6, finished);
            Assert.Equal(0, k.Life.Fields.PendingCount(farmer));
        }

        [Fact]
        public void TekDokunus_AcemiYalnizDokunduguEkiniBicer()
        {
            var k = Make();
            var a = Ripe(k, "item.crop_turnip", 3, 3);
            var b = Ripe(k, "item.crop_turnip", 4, 3);
            Assert.Equal(1, k.Life.Fields.OrderFrom(Farmer(1), a));
            Assert.Null(k.World.GetItem(a.Id));
            Assert.NotNull(k.World.GetItem(b.Id));
            // Olgun olmayan ekin emredilmez.
            var young = k.World.PlaceItem(k.Db.Items.Get("item.crop_turnip"), new GridPos(8, 8));
            Assert.Equal(0, k.Life.Fields.OrderFrom(Farmer(5, 2), young));
        }

        [Fact]
        public void Surukleme_SirayaEkler_BaskaIsEmriDusurur()
        {
            var k = Make();
            k.AutoRun = false;
            var farmer = Farmer(1);
            var a = Ripe(k, "item.crop_turnip", 3, 3);
            var b = Ripe(k, "item.crop_cabbage", 6, 6);
            var c = Ripe(k, "item.crop_leek", 14, 3);
            Assert.True(k.Life.Fields.Add(farmer, a));
            Assert.True(k.Life.Fields.Add(farmer, b));
            Assert.False(k.Life.Fields.Add(farmer, b));                          // aynı ekin iki kez eklenmez
            Assert.True(k.Life.Fields.Add(farmer, c));
            Assert.Equal(3, k.Life.Fields.PendingCount(farmer));
            Assert.Single(k.Commands);                                           // yalnızca ilki emredildi; ötekiler bekliyor
            Assert.True(k.Life.Fields.IsPending(c.Id));

            // İlki biçilince en yakın olan (b) emredilir.
            k.Fx.Apply(farmer, a, HarvestOrders.HarvestOf(a.Def));
            Assert.Equal(2, k.Commands.Count);
            Assert.Equal(b.Id, k.Commands[1].ItemId);

            // Karakter başka bir işe geçerse (komposta yaş atık eklemek; belge 57b) emir düşer.
            var heap = k.World.PlaceItem(k.Db.Items.Get("item.compost_heap"), new GridPos(12, 12));
            k.House.AddItem("item.herb", 3);
            k.Fx.Apply(farmer, heap, heap.Def.Interactions.First(i => i.Id == "add_wet"));
            Assert.Equal(0, k.Life.Fields.PendingCount(farmer));
        }

        [Fact]
        public void Tirpan_TahildaYandakiIkiKaroyuDaBicer_KokteBicmez()
        {
            var k = Make();
            var farmer = Farmer(1);
            // Buğday sırası: ortadakini biçince iki yanı da biçilir; üçüncü komşu kalır.
            var mid = Ripe(k, "item.crop_wheat", 5, 5);
            var left = Ripe(k, "item.crop_wheat", 4, 5);
            var right = Ripe(k, "item.crop_wheat", 6, 5);
            var up = Ripe(k, "item.crop_wheat", 5, 6);
            var harvest = HarvestOrders.HarvestOf(mid.Def);

            // Tırpan yokken yalnızca biçilen karo.
            Assert.Equal(0, k.Life.Tools.Tier(HarvestOrders.ScytheTool));
            k.Fx.Apply(farmer, mid, harvest);
            Assert.NotNull(k.World.GetItem(left.Id));

            // Tırpan alınınca (demirci, akçe, odun) yandaki iki karo da biçilir.
            k.House.AddAkce(500); k.House.AddItem("item.firewood", 20);
            k.Life.Tools.SkillLevel = _ => 10;
            k.Life.Tools.SmithAvailable = () => true;
            Assert.True(k.Life.Tools.Upgrade(HarvestOrders.ScytheTool, out var why), why);
            int reaped = 0;
            k.Life.Fields.Reaped += (c, n) => reaped += n;
            int before = k.House.Count("item.sheaf_wheat");
            k.Fx.Apply(farmer, left, harvest);                                    // left'in komşusu yok (mid biçildi)
            Assert.Equal(0, reaped);
            var a = Ripe(k, "item.crop_wheat", 5, 5);
            k.Fx.Apply(farmer, a, harvest);                                       // komşuları: right, up
            Assert.Equal(HarvestOrders.ScytheExtraTiles, reaped);
            Assert.Null(k.World.GetItem(right.Id));
            Assert.Null(k.World.GetItem(up.Id));
            Assert.True(k.House.Count("item.sheaf_wheat") >= before + 5 * 4);

            // Kök ekinde tırpan işlemez.
            var t1 = Ripe(k, "item.crop_turnip", 10, 10);
            var t2 = Ripe(k, "item.crop_turnip", 11, 10);
            reaped = 0;
            k.Fx.Apply(farmer, t1, HarvestOrders.HarvestOf(t1.Def));
            Assert.Equal(0, reaped);
            Assert.NotNull(k.World.GetItem(t2.Id));
        }
    }
}
