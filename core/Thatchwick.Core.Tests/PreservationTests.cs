using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Yiyecek saklama: hayvan kesimi, tuzlama, turşu, pastırma, tütsüleme, peynir, kurutma, reçel.
    public class PreservationTests
    {
        private static ContentDatabase Real()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static WorldState World()
        {
            var map = new GridMap(24, 24);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) map.SetOwned(new GridPos(x, y), true);
            return new WorldState(map);
        }

        private static Character Cook(int level = 6)
        {
            var c = new Character(1, "Aşçı", new GridPos(0, 0)) { MoodEnabled = true }; c.SetAge(60);
            c.GainSkillXp(SkillType.Cooking, SkillRules.XpForLevel(level) + 1);
            return c;
        }

        private static void Run(InteractionEffects fx, Character c, ItemInstance item, string id, int times = 1)
        {
            var it = item.Def.Interactions.First(i => i.Id == id);
            for (int n = 0; n < times; n++)
            {
                Assert.Null(fx.CanStart(c, item, it));
                fx.Apply(c, item, it);
            }
        }

        // ---------- kesim ----------

        [Fact]
        public void Kesim_YetiskinDostOlmayanHayvanEtVerir_HayvanKalkar()
        {
            var db = Real();
            var w = World();
            var house = new Household(500);
            var animals = new AnimalSystem(w, db, seed: 3);
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            var barn = w.PlaceItem(db.Items.Get("item.barn"), new GridPos(12, 5));
            var hen = animals.Restore(1, db.Animals.Get("animal.chicken"), new GridPos(4, 5), coop.Id, 100f, 10, 0, 0, friendship: 20f);
            var cow = animals.Restore(2, db.Animals.Get("animal.cow"), new GridPos(11, 5), barn.Id, 100f, 20, 0, 0, friendship: 10f);
            var fx = new InteractionEffects(w, house, db, 1) { Animals = animals };
            var c = Cook();

            Run(fx, c, coop, "slaughter");
            Assert.Equal(1, house.Count("item.poultry"));
            Assert.Equal(0, animals.Residents(coop.Id));

            Run(fx, c, barn, "slaughter");
            Assert.Equal(4, house.Count("item.meat"));
            Assert.Equal(0, animals.Residents(barn.Id));
        }

        [Fact]
        public void Kesim_DostaKiyilmaz_CocugaKiyilmaz_BosBarinaktaOlmaz()
        {
            var db = Real();
            var w = World();
            var animals = new AnimalSystem(w, db, seed: 3);
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            var fx = new InteractionEffects(w, new Household(10), db, 1) { Animals = animals };
            var c = Cook();
            var cut = coop.Def.Interactions.First(i => i.Id == "slaughter");

            Assert.Equal("Burada hayvan yok.", fx.CanStart(c, coop, cut));
            var chick = animals.Restore(1, db.Animals.Get("animal.chicken"), new GridPos(4, 5), coop.Id, 100f, 1, 0, 0, friendship: 0f);
            Assert.Equal("Kesilecek kadar büyümüş hayvan yok.", fx.CanStart(c, coop, cut));
            animals.Restore(2, db.Animals.Get("animal.chicken"), new GridPos(4, 4), coop.Id, 100f, 10, 0, 0, friendship: AnimalSystem.SpareFriendship);
            // Yetişkin ama dost: kıyılmaz; civciv henüz küçük.
            Assert.Equal("Kalan hayvanlar artık dost; kıyamazsın.", fx.CanStart(c, coop, cut));
            Assert.Equal(2, animals.Residents(coop.Id));
        }

        [Fact]
        public void Kesim_EnDusukDostluklaBaslar_EvcilHayvanKesilmez()
        {
            var db = Real();
            var w = World();
            var animals = new AnimalSystem(w, db, seed: 3);
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            var a = animals.Restore(1, db.Animals.Get("animal.chicken"), new GridPos(4, 5), coop.Id, 100f, 10, 0, 0, friendship: 50f);
            var b = animals.Restore(2, db.Animals.Get("animal.chicken"), new GridPos(4, 4), coop.Id, 100f, 10, 0, 0, friendship: 5f);
            Assert.Same(b, animals.SlaughterCandidate(coop.Id));
            Assert.True(animals.Slaughter(coop.Id, out var killed, out var meat, out var n));
            Assert.Same(b, killed); Assert.Equal("item.poultry", meat); Assert.Equal(1, n);

            var kennel = w.PlaceItem(db.Items.Get("item.kennel"), new GridPos(12, 12));
            animals.Restore(3, db.Animals.Get("animal.dog"), new GridPos(11, 12), kennel.Id, 100f, 30, 0, 0, friendship: 0f);
            Assert.Null(animals.SlaughterCandidate(kennel.Id));
            Assert.Equal("Bu hayvanlar kesilmez.", animals.SlaughterBlocker(kennel.Id));
        }

        // ---------- pastırma zinciri ----------

        [Fact]
        public void Pastirma_EtTuzaYatarKurur_TuzluEtCurur_PastirmaCurumez()
        {
            var db = Real();
            var w = World();
            var house = new Household();
            var fx = new InteractionEffects(w, house, db, 1);
            var barrel = w.PlaceItem(db.Items.Get("item.salting_barrel"), new GridPos(4, 4));
            var rack = w.PlaceItem(db.Items.Get("item.drying_rack"), new GridPos(8, 4));
            var c = Cook();
            house.AddItem("item.meat", 4);

            var salt = barrel.Def.Interactions.First(i => i.Id == "salt_meat");
            Assert.Equal("Tuz yok.", fx.CanStart(c, barrel, salt));
            house.AddItem("item.salt", 2);
            Run(fx, c, barrel, "salt_meat", 2);
            Assert.Equal(0, house.Count("item.meat"));
            Assert.Equal(4, house.Count("item.salted_meat"));
            Assert.Equal(0, house.Count("item.salt"));

            Run(fx, c, rack, "cure_pastirma", 1);
            Assert.Equal(2, house.Count("item.pastirma"));
            Assert.Equal(2, house.Count("item.salted_meat"));

            var spoil = new SpoilageService(db, house, w);
            for (int i = 0; i < 450; i++) spoil.Tick(0.1);             // 45 gün
            Assert.Equal(2, house.Count("item.pastirma"));
            Assert.Equal(0, house.Count("item.salted_meat"));            // tuzlu et ara üründür, çürür
        }

        [Fact]
        public void Tutsuluk_TuzluEtiFumeYapar_OdunHarcar()
        {
            var db = Real();
            var w = World();
            var house = new Household();
            var fx = new InteractionEffects(w, house, db, 1);
            var smoke = w.PlaceItem(db.Items.Get("item.smokehouse"), new GridPos(4, 4));
            var c = Cook();
            house.AddItem("item.salted_meat", 2);
            Assert.Equal("Odun yok.", fx.CanStart(c, smoke, smoke.Def.Interactions.First(i => i.Id == "smoke_meat")));
            house.AddItem("item.firewood", 1);
            Run(fx, c, smoke, "smoke_meat");
            Assert.Equal(2, house.Count("item.smoked_meat"));
            Assert.Equal(0, house.Count("item.firewood"));
        }

        [Fact]
        public void Peynir_SutPeyniregePeyniriSalamuraya_TuzIster()
        {
            var db = Real();
            var w = World();
            var house = new Household();
            var fx = new InteractionEffects(w, house, db, 1);
            var press = w.PlaceItem(db.Items.Get("item.cheese_press"), new GridPos(4, 4));
            var barrel = w.PlaceItem(db.Items.Get("item.salting_barrel"), new GridPos(8, 4));
            var c = Cook();
            house.AddItem("item.milk", 3); house.AddItem("item.salt", 1);
            Run(fx, c, press, "make_cheese");
            Assert.Equal(2, house.Count("item.cheese"));
            Run(fx, c, barrel, "brine_cheese");
            Assert.Equal(2, house.Count("item.brined_cheese"));
            Assert.Equal(0, house.Count("item.salt"));
        }

        [Fact]
        public void Tursu_YeniSebzelerTuzlaKurulur_ReceleBalKatilir()
        {
            var db = Real();
            var w = World();
            var house = new Household();
            var fx = new InteractionEffects(w, house, db, 1);
            var barrel = w.PlaceItem(db.Items.Get("item.salting_barrel"), new GridPos(4, 4));
            var pot = w.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(8, 4));
            var c = Cook();
            house.AddItem("item.carrot", 3); house.AddItem("item.salt", 1);
            Run(fx, c, barrel, "pickle_carrot");
            Assert.Equal(3, house.Count("item.pickled_carrot"));

            house.AddItem("item.berries", 3);
            Assert.Equal("Bal yok.", fx.CanStart(c, pot, pot.Def.Interactions.First(i => i.Id == "cook_jam_berry")));
            house.AddItem("item.honey", 1);
            Run(fx, c, pot, "cook_jam_berry");
            Assert.Equal(3, house.Count("item.jam_berry"));
        }

        // ---------- içerik kuralları ----------

        [Fact]
        public void GercekIcerik_KorunmusYiyecekCurumez_HamEtCurur_HerYiyecekYenebilir()
        {
            var db = Real();
            var foods = db.Items.All.Where(i => i.HasTag("food")).ToList();
            foreach (var f in foods.Where(i => i.HasTag("preserved")))
                Assert.True(f.SpoilDays <= 0f, f.Id + ": korunmuş yiyecek çürümemeli");
            foreach (var f in foods.Where(i => i.HasTag("raw")))
                Assert.True(f.SpoilDays > 0f && f.SpoilDays <= 6f, f.Id + ": ham et çabuk çürümeli");
            Assert.True(db.Items.Get("item.salted_meat").SpoilDays > 0f);

            var all = db.Items.All.SelectMany(i => i.Interactions).ToList();
            foreach (var f in foods)
                Assert.True(all.Any(i => i.AllConsumed().Any(c => c.ItemId == f.Id)), f.Id + " yenmiyor ve hiçbir tarifte kullanılmıyor");
            Assert.True(db.Items.Get("item.pastirma").Price >= 15);
        }

        [Fact]
        public void GercekIcerik_TuzPazardanAlinir_Turşular_Sayilir()
        {
            var db = Real();
            var market = new Market(db, new Household(100));
            Assert.Contains(market.Goods(), d => d.Id == "item.salt");
            var barrel = db.Items.Get("item.salting_barrel");
            var pickles = barrel.Interactions.Where(i => i.Id.StartsWith("pickle_")).ToList();
            Assert.True(pickles.Count >= 6);
            foreach (var p in barrel.Interactions)
                Assert.Contains(p.AllConsumed(), c => c.ItemId == "item.salt");   // fıçıdaki her iş tuz harcar
            Assert.True(db.Items.Get("item.drying_rack").Interactions.Count(i => i.Id.StartsWith("dry_")) >= 6);
            Assert.True(db.Items.Get("item.smokehouse").Interactions.Count >= 3);
        }

        [Fact]
        public void HamEt_KisStogunaVeBebekBesleyemeyeSayilmaz()
        {
            var db = Real();
            var house = new Household();
            house.AddItem("item.meat", 3); house.AddItem("item.poultry", 2);
            var pop = new Population();
            var fam = new FamilyService(pop, house, db, 1);
            Assert.Null(fam.FirstFood());
            house.AddItem("item.pastirma", 1);
            Assert.Equal("item.pastirma", fam.FirstFood());
        }
    }
}
