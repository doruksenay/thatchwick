using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Mining;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // 28. belge sırası, son iki adım: kâhya ve öküz arabası; maden ürünlerini işleme zincirleri (23. belge 8, 53.3).
    public class KahyaVeIslemeTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static WorldState World(int size = 16)
        {
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            return new WorldState(map);
        }

        private static Character Adult(int id, string name = "Agnes")
        {
            var c = new Character(id, name, new GridPos(1, 1));
            c.SetAge(30);
            return c;
        }

        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);

        // ---------- işleme ----------

        [Fact]
        public void Demir_KavurmaCicekOcagiDovme_SuCekiciCicegiDover()
        {
            var db = Db();
            var w = World();
            var house = new Household(0);
            var auto = new FarmAutomation(w, house) { Content = db };
            w.PlaceItem(db.Items.Get("item.roasting_heap"), new GridPos(2, 2));
            w.PlaceItem(db.Items.Get("item.charcoal_clamp"), new GridPos(6, 2));
            house.AddItem("item.iron_ore", 4);
            house.AddItem("item.firewood", 6);
            auto.OnNewDay(Season.Summer);                                           // güç istemez: sabah bir parti
            Assert.Equal(4, house.Count("item.roasted_ore"));
            Assert.Equal(4, house.Count("item.charcoal"));
            Assert.False(db.Items.Get("item.charcoal_clamp").NeedsPower);

            var fx = new InteractionEffects(w, house, db);
            var bloomery = w.PlaceItem(db.Items.Get("item.bloomery"), new GridPos(10, 2));
            var smith = Adult(1);
            var smelt = Inter(bloomery.Def, "smelt_roasted");
            Assert.Null(fx.CanStart(smith, bloomery, smelt));
            fx.Apply(smith, bloomery, smelt);
            Assert.Equal(1, house.Count("item.iron_bloom"));
            Assert.Equal(1, house.Count("item.slag"));

            // Su çekici (karar 4): 2 demir çiçeği → 2 demir.
            var hammer = db.Items.Get("item.trip_hammer").AutoProcess;
            Assert.Equal("item.iron_bloom", hammer.Item);
            Assert.Equal(2, hammer.Count);
            Assert.Equal(2, hammer.ProducesCount);
            // Demirci: çiçeği döverek demir.
            Assert.Contains(db.Items.All.SelectMany(d => d.Interactions), i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.iron_bar") && i.Consume.Any(m => m.ItemId == "item.iron_bloom"));
        }

        [Fact]
        public void Kirec_OcaktaYanar_SonerHarcOlur_ArduvazCatiLevhaIster()
        {
            var db = Db();
            var w = World();
            var house = new Household(0);
            var kiln = w.PlaceItem(db.Items.Get("item.lime_kiln"), new GridPos(3, 3));
            new FarmAutomation(w, house) { Content = db }.OnNewDay(Season.Spring);
            Assert.Equal(0, house.Count("item.quicklime"));
            house.AddItem("item.limestone", 4);
            new FarmAutomation(w, house) { Content = db }.OnNewDay(Season.Spring);
            Assert.Equal(3, house.Count("item.quicklime"));
            var fx = new InteractionEffects(w, house, db);
            var c = Adult(1);
            fx.Apply(c, kiln, Inter(kiln.Def, "slake"));
            Assert.Equal(2, house.Count("item.lime"));
            house.AddItem("item.sand", 2);
            fx.Apply(c, kiln, Inter(kiln.Def, "mix_mortar"));
            Assert.Equal(3, house.Count("item.mortar"));
            Assert.Equal("item.slate_tile", db.Parts.Get("roof_slate").MaterialItem);   // karar 5
            Assert.Equal("item.lime", db.Parts.Get("wall_whitewash").MaterialItem);
        }

        [Fact]
        public void Pazar_HamMaddeHaftada30danSonraUcuzlar_IslenmisDusmez_HaftaDonunceToparlanir()
        {
            var db = Db();
            var house = new Household(0);
            var market = new Market(db, house, config: new MarketConfig { DemandDropPerUnit = 0 });
            market.Tick(0, 0, Season.Spring);
            house.AddItem("item.limestone", 60);
            house.AddItem("item.lime", 60);
            int before = market.SellPrice("item.limestone");
            market.Sell("item.limestone", 30);
            Assert.Equal(1.0, market.RawSaturation("item.limestone"), 6);
            market.Sell("item.limestone", 20);
            Assert.True(market.RawSaturation("item.limestone") < 1.0);
            Assert.True(market.RawSaturation("item.limestone") >= 0.8);
            market.Sell("item.lime", 50);
            Assert.Equal(1.0, market.RawSaturation("item.lime"), 6);              // işlenmiş: düşmez
            market.Tick(7, 7, Season.Spring);
            Assert.Equal(1.0, market.RawSaturation("item.limestone"), 6);
            Assert.Equal(before, market.SellPrice("item.limestone"));
        }

        // ---------- kâhya ----------

        /// <summary>Kemente düşen avı Hayvanlar görevlisi kendiliğinden toplar (karar 20.11.2026).</summary>
        [Fact]
        public void HayvanGorevlisi_KementeDusenAviToplar()
        {
            var db = Db();
            var w = World();
            var house = new Household(0);
            var animals = new AnimalSystem(w, db) { DiseaseEnabled = false };
            var fx = new InteractionEffects(w, house, db);
            var planner = new DutyPlanner(w, animals, fx, null);
            var snare = w.PlaceItem(db.Items.Get("item.snare"), new GridPos(6, 6));
            var keeper = Adult(1); keeper.Duty = DutyKind.Animals;
            int before = planner.Pending(DutyKind.Animals);
            Assert.Null(planner.Next(keeper));                                  // boş tuzak iş değildir

            AnimalSystem.PutStock(snare, "item.game_meat", 1);
            Assert.Equal(before + 1, planner.Pending(DutyKind.Animals));
            var job = planner.Next(keeper);
            Assert.NotNull(job);
            Assert.Equal(snare.Id, job.ItemId);
            Assert.Equal("check_snare", job.InteractionId);
            fx.Apply(keeper, snare, db.Items.Get("item.snare").Interactions.Single(i => i.Id == "check_snare"));
            Assert.True(house.Count("item.game_meat") >= 1);
            Assert.Null(planner.Next(keeper));
        }

        [Fact]
        public void Kahya_BekleyenIseGoreDagitir_OyuncuyuVeKendiniAtamaz()
        {
            var db = Db();
            var w = World();
            var house = new Household(0);
            var animals = new AnimalSystem(w, db) { DiseaseEnabled = false };
            var planner = new DutyPlanner(w, animals, new InteractionEffects(w, house, db), null);
            // Tarlada 3 olgun ekin, ahırda 1 iş.
            for (int i = 0; i < 3; i++)
            {
                w.Map.SetFloor(new GridPos(2 + i, 8), FloorType.Dirt);
                var crop = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(2 + i, 8));
                PlantSystem.SetGrown(crop);
            }
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(10, 10));
            animals.Restore(1, db.Animals.Get("animal.chicken"), new GridPos(9, 9), coop.Id, 100f, 10, 0, 0, female: true);
            int farm = planner.Pending(DutyKind.Farm), anim = planner.Pending(DutyKind.Animals);
            Assert.True(farm >= 3 && anim >= 1);

            var steward = Adult(1, "Reeve"); steward.Duty = DutyKind.Steward;
            var a = Adult(2, "Maud"); var b = Adult(3, "Wat"); var c = Adult(4, "Joan");
            var player = Adult(5, "Oyuncu"); player.IsPlayerControlled = true;
            var changes = planner.AutoAssign(new[] { steward, a, b, c, player });
            Assert.Equal(DutyKind.Steward, steward.Duty);
            Assert.Equal(DutyKind.None, player.Duty);
            var duties = new[] { a.Duty, b.Duty }.ToList();
            Assert.Contains(DutyKind.Farm, duties);
            Assert.True(steward.Skills.GetXp(SkillType.Stewardship) > 0);
            Assert.NotEmpty(changes);
            // Kâhyalık 1: en az 2 görevli; üçüncüye sıra gelmez.
            Assert.Equal(DutyKind.None, c.Duty);

            // Kâhya yoksa kimse değişmez.
            var x = Adult(6); x.Duty = DutyKind.Animals;
            Assert.Empty(planner.AutoAssign(new[] { x }));
            Assert.Equal(DutyKind.Animals, x.Duty);
        }

        // ---------- öküz arabası ----------

        [Fact]
        public void OkuzArabasi_HaftadaBirFazlayiSatar_TohumVeYemKalir_CekiHayvaniIster()
        {
            var db = Db();
            var w = World();
            var house = new Household(0);
            var market = new Market(db, house);
            market.Tick(0, 0, Season.Summer);
            var cart = new OxCart(w, house, market, db) { DraftAnimals = () => 1 };
            Assert.NotNull(cart.Blocker(6));                                         // araba yok
            w.PlaceItem(db.Items.Get(OxCart.CartItem), new GridPos(3, 3));
            house.AddItem("item.limestone", 50);
            house.AddItem("item.seed_wheat", 50);
            house.AddItem("item.feed", 50);
            house.AddItem("item.turnip", 25);
            Assert.NotNull(cart.Blocker(3));                                         // sırası değil
            Assert.Null(cart.Blocker(6));
            int earned = cart.OnNewDay(6, out var sold);
            Assert.True(earned > 0);
            Assert.Equal(OxCart.Keep, house.Count("item.limestone"));
            Assert.Equal(OxCart.Keep, house.Count("item.turnip"));
            Assert.Equal(50, house.Count("item.seed_wheat"));
            Assert.Equal(50, house.Count("item.feed"));
            Assert.Equal(35, sold.Sum(kv => kv.Value));
            cart.DraftAnimals = () => 0;
            Assert.Contains("çekecek", cart.Blocker(13));
        }
    }
}
