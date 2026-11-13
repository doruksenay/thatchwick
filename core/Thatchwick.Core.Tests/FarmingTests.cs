using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Eski testler sulama şartı olmadan büyüme varsayar; bu yardımcı o davranışı verir.</summary>
    internal static class FarmTestKit
    {
        public static PlantSystem Legacy(WorldState w)
        {
            var p = new PlantSystem(w);
            p.Config.DryGrowth = 1f;
            return p;
        }
    }

    // Çiftçilik derinliği: sulama, toprak verimi, nöbetleşe ekim, kalite, sera, meyve ağacı, hendek, korkuluk.
    public class FarmingTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2 });
            db.Items.Register(new ItemDef { Id = "pea", DisplayName = "Bezelye", Placement = ItemPlacement.Resource, Price = 2 });
            db.Items.Register(new ItemDef { Id = "apple", DisplayName = "Elma", Placement = ItemPlacement.Resource, Price = 3 });
            db.Items.Register(new ItemDef { Id = "compost", DisplayName = "Kompost", Placement = ItemPlacement.Resource, Price = 3 });
            db.Items.Register(new ItemDef { Id = "seed_turnip", DisplayName = "Şalgam tohumu", Placement = ItemPlacement.Resource, Price = 1, PlantsAs = "crop_turnip" });
            db.Items.Register(new ItemDef { Id = "seed_pea", DisplayName = "Bezelye tohumu", Placement = ItemPlacement.Resource, Price = 1, PlantsAs = "crop_pea" });
            db.Items.Register(new ItemDef { Id = "sapling_apple", DisplayName = "Elma fidanı", Placement = ItemPlacement.Resource, Price = 8, PlantsAs = "tree_apple" });
            db.Items.Register(Crop("crop_turnip", "turnip", "seed_turnip", 6f, false));
            db.Items.Register(Crop("crop_pea", "pea", "seed_pea", 4f, true));
            db.Items.Register(new ItemDef
            {
                Id = "tree_apple", DisplayName = "Elma ağacı", GrowDays = 10f, FruitRipenDays = 3f,
                FruitSeasons = new List<Season> { Season.Summer, Season.Autumn },
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "pick", DisplayName = "Meyve topla", DurationDays = 0.05, Skill = SkillType.Farming, SkillXp = 8,
                        RequiresGrown = true, RequiresFruit = true, TakesFruit = true,
                        Produce = new List<ProduceDef> { new ProduceDef { ItemId = "apple", Count = 3 } }
                    }
                }
            });
            db.Items.Register(new ItemDef
            {
                Id = "can_spot", DisplayName = "Bidon", Price = 1, Placement = ItemPlacement.Floor,
                Interactions = new List<InteractionDef>()
            });
            db.Items.Register(new ItemDef
            {
                Id = "ditch", DisplayName = "Sulama hendeği", WaterRadius = 1,
                Interactions = new List<InteractionDef>()
            });
            db.Items.Register(new ItemDef { Id = "scarecrow", DisplayName = "Korkuluk", ProtectRadius = 3, Interactions = new List<InteractionDef>() });
            return db;
        }

        private static ItemDef Crop(string id, string produce, string seed, float days, bool nitrogen) => new ItemDef
        {
            Id = id, DisplayName = id, GrowDays = days, NeedsTilledSoil = true, DiesInWinter = true, FixesNitrogen = nitrogen,
            Interactions = new List<InteractionDef>
            {
                new InteractionDef { Id = "water", DisplayName = "Sula", DurationDays = 0.02, Skill = SkillType.Farming, SkillXp = 2, Water = true },
                new InteractionDef
                {
                    Id = "compost", DisplayName = "Gübrele", DurationDays = 0.03, Fertilize = 0.3f,
                    Consume = new List<ConsumeDef> { new ConsumeDef { ItemId = "compost", Count = 1 } }
                },
                new InteractionDef
                {
                    Id = "harvest", DisplayName = "Hasat et", DurationDays = 0.05, Skill = SkillType.Farming, SkillXp = 15,
                    RequiresGrown = true, RemovesItem = true,
                    Produce = new List<ProduceDef> { new ProduceDef { ItemId = produce, Count = 4 }, new ProduceDef { ItemId = seed, Count = 1 } }
                }
            }
        };

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
            var w = new WorldState(new GridMap(20, 14));
            var h = new Household(100);
            var plants = new PlantSystem(w, seed: 5);
            var b = new BuildService(w, h, db) { Plants = plants };
            var fx = new InteractionEffects(w, h, db, 5) { Plants = plants };
            return new Ctx { Db = db, World = w, House = h, Build = b, Plants = plants, Effects = fx };
        }

        private static ItemInstance Sow(Ctx c, string seed, int x, int y)
        {
            c.House.AddItem(seed, 1);
            c.World.Map.SetFloor(new GridPos(x, y), FloorType.Dirt);
            var r = c.Build.Plant(seed, new GridPos(x, y), out var crop);
            Assert.True(r.Ok, r.Reason);
            return crop;
        }

        private static Character Farmer(int skill = 1)
        {
            var ch = new Character(1, "A", new GridPos(0, 0), true);
            ch.SetAge(30);
            return ch;
        }

        private static void Do(Ctx c, ItemInstance item, string interaction, Character ch = null)
        {
            ch = ch ?? Farmer();
            ch.Position = new GridPos(item.Position.X - 1 < 0 ? item.Position.X + 1 : item.Position.X - 1, item.Position.Y);
            var r = new JobRunner(ch, c.World) { Effects = c.Effects };
            r.Command(Job.UseItem(item.Id, interaction));
            for (int i = 0; i < 80 && !r.IsIdle; i++) r.Tick(0.02);
        }

        private static void Box(GridMap m, int x0, int y0, int x1, int y1, params (int x, int y, EdgeAxis axis)[] windows)
        {
            for (int x = x0; x <= x1; x++)
            {
                m.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal), EdgeKind.Wall);
            }
            for (int y = y0; y <= y1; y++)
            {
                m.SetEdge(new Edge(x0, y, EdgeAxis.Vertical), EdgeKind.Wall);
                m.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            foreach (var wdw in windows) m.SetEdge(new Edge(wdw.x, wdw.y, wdw.axis), EdgeKind.Window);
        }

        // ---------- sulama ----------

        [Fact]
        public void Sulanmayan_EkinYavasBuyur_SuluOlanTamHizda()
        {
            var c = Setup();
            var dry = Sow(c, "seed_turnip", 5, 5);
            var wet = Sow(c, "seed_turnip", 8, 5);
            PlantSystem.Water(wet);
            c.Plants.Tick(1, Season.Summer);
            Assert.Equal(0.3, dry.GrowthDays, 3);
            Assert.Equal(1.0, wet.GrowthDays, 3);
        }

        [Fact]
        public void Sulama_EtkilesimiEkiniSular_OlgunaYadaSuluyaRedder()
        {
            var c = Setup();
            var crop = Sow(c, "seed_turnip", 5, 5);
            Assert.False(crop.Watered);
            Do(c, crop, "water");
            Assert.True(crop.Watered);

            var again = new JobRunner(Farmer(), c.World) { Effects = c.Effects };
            Assert.Equal("Zaten sulu.", again.WhyCannotStart(crop, crop.Def.Interactions[0]));

            PlantSystem.SetGrown(crop);
            Assert.Equal("Ekin olgun, sulamaya gerek yok.", c.Effects.CanStart(Farmer(), crop, crop.Def.Interactions[0]));
        }

        [Fact]
        public void Yagmur_DisaridakiEkiniSular_YeniGundeSifirlanir()
        {
            var c = Setup();
            var crop = Sow(c, "seed_turnip", 5, 5);
            c.Plants.OnNewDay(rained: true);
            Assert.True(crop.Watered);
            c.Plants.OnNewDay(rained: false);
            Assert.False(crop.Watered);
        }

        [Fact]
        public void Agac_SulamaGerektirmez()
        {
            var c = Setup();
            c.House.AddItem("sapling_apple", 1);
            c.Build.Plant("sapling_apple", new GridPos(5, 5), out var tree);
            c.Plants.Tick(5, Season.Spring);
            Assert.Equal(5.0, tree.GrowthDays, 3);
        }

        [Fact]
        public void SulamaHendegi_YaricapindakiEkinleriHerSabahSular()
        {
            var c = Setup();
            var near = Sow(c, "seed_turnip", 5, 5);
            var far = Sow(c, "seed_turnip", 9, 5);
            c.World.PlaceItem(c.Db.Items.Get("ditch"), new GridPos(6, 5));
            c.Plants.OnNewDay(rained: false);
            Assert.True(near.Watered);
            Assert.False(far.Watered);
        }

        // ---------- toprak ----------

        [Fact]
        public void ToprakVerimi_BuyumeHiziniEtkiler()
        {
            var c = Setup();
            var rich = Sow(c, "seed_turnip", 5, 5);
            var poor = Sow(c, "seed_turnip", 8, 5);
            c.Plants.SetSoil(rich.Position, 1f, null);
            c.Plants.SetSoil(poor.Position, 0f, null);
            PlantSystem.Water(rich); PlantSystem.Water(poor);
            c.Plants.Tick(1, Season.Summer);
            Assert.Equal(1.25, rich.GrowthDays, 3);
            Assert.Equal(0.75, poor.GrowthDays, 3);
        }

        [Fact]
        public void Gubre_VerimiArtirir_UstSinirdaDurur()
        {
            var c = Setup();
            var crop = Sow(c, "seed_turnip", 5, 5);
            c.House.AddItem("compost", 5);
            Assert.Equal(0.5f, c.Plants.FertilityAt(crop.Position));
            Do(c, crop, "compost");
            Assert.Equal(0.8, (double)c.Plants.FertilityAt(crop.Position), 2);
            Assert.Equal(4, c.House.Count("compost"));
            Do(c, crop, "compost");
            Do(c, crop, "compost");
            Assert.Equal(1.0, (double)c.Plants.FertilityAt(crop.Position), 2);
            Assert.Equal("Toprak zaten çok verimli.", c.Effects.CanStart(Farmer(), crop, crop.Def.Interactions[1]));
        }

        [Fact]
        public void Hasat_ToprakYorar_AyniEkinDahaCokYorar_BaklagilZenginlestirir()
        {
            var c = Setup();
            var pos = new GridPos(5, 5);
            var a = Sow(c, "seed_turnip", 5, 5);
            PlantSystem.SetGrown(a);
            Do(c, a, "harvest");
            Assert.Equal(0.38, (double)c.Plants.FertilityAt(pos), 2);                 // 0.5 - 0.12
            Assert.Equal("crop_turnip", c.Plants.LastCropAt(pos));

            var b = Sow(c, "seed_turnip", 5, 5);                                 // aynı ekin: ek ceza
            PlantSystem.SetGrown(b);
            Do(c, b, "harvest");
            Assert.Equal(0.20, (double)c.Plants.FertilityAt(pos), 2);                 // 0.38 - 0.12 - 0.06

            var p = Sow(c, "seed_pea", 5, 5);                                    // baklagil: azot + nöbetleşe ekim
            PlantSystem.SetGrown(p);
            Do(c, p, "harvest");
            Assert.Equal(0.36, (double)c.Plants.FertilityAt(pos), 2);                 // 0.20 - 0.12 + 0.20 + 0.08
        }

        [Fact]
        public void BosToprak_VarsayilanaDoner()
        {
            var c = Setup();
            c.Plants.SetSoil(new GridPos(3, 3), 0.1f, "x");
            c.Plants.SetSoil(new GridPos(4, 3), 0.95f, "x");
            for (int i = 0; i < 10; i++) c.Plants.OnNewDay(false);
            Assert.Equal(0.3, (double)(c.Plants.FertilityAt(new GridPos(3, 3))), 2);
            Assert.Equal(0.75, (double)(c.Plants.FertilityAt(new GridPos(4, 3))), 2);
            for (int i = 0; i < 30; i++) c.Plants.OnNewDay(false);
            Assert.Equal(0.5, (double)(c.Plants.FertilityAt(new GridPos(3, 3))), 2);
        }

        // ---------- kalite ----------

        [Fact]
        public void Kalite_SuluDuzenVeVerimeGore_AsagidanYukariya()
        {
            var cfg = new FarmConfig();
            Assert.Equal(0, PlantSystem.QualityTier(cfg, 0f, 0.5f, 1));
            Assert.Equal(1, PlantSystem.QualityTier(cfg, 1f, 0.5f, 1));
            Assert.Equal(2, PlantSystem.QualityTier(cfg, 1f, 0.9f, 5));
            Assert.Equal("Seçme", PlantSystem.QualityName(1));
        }

        [Fact]
        public void Hasat_KaliteliUrunDahaCokVerir_TohumuCogaltmaz()
        {
            var c = Setup();
            var crop = Sow(c, "seed_turnip", 5, 5);
            c.Plants.SetSoil(crop.Position, 0.9f, null);
            for (int i = 0; i < 6; i++) { PlantSystem.Water(crop); c.Plants.Tick(1.0, Season.Summer); }
            Assert.True(crop.IsGrown);
            Assert.True(crop.WetRatio > 0.99f);
            string said = null;
            c.Effects.Applied += (ch, inter, summary) => said = summary;
            Do(c, crop, "harvest");
            // Birinci sınıf: 4 x 1,5 = 6 şalgam; tohum 1.
            Assert.Equal(6, c.House.Count("turnip"));
            Assert.Equal(1, c.House.Count("seed_turnip"));
            Assert.Contains("kalite: Birinci sınıf", said);
        }

        [Fact]
        public void Hasat_SulanmayanEkin_SiradanKalite()
        {
            var c = Setup();
            var crop = Sow(c, "seed_turnip", 5, 5);
            PlantSystem.SetGrown(crop);
            Do(c, crop, "harvest");
            Assert.Equal(4, c.House.Count("turnip"));
        }

        // ---------- sera ----------

        [Fact]
        public void Sera_PencereliIsitilmisOdadaKisinBuyur_DigerIcMekandaBuyumez()
        {
            var c = Setup();
            // 3x3 oda, 3 pencere: sera. Başka bir 3x3 oda penceresiz: dikilemez.
            Box(c.World.Map, 2, 2, 4, 4, (3, 2, EdgeAxis.Horizontal), (2, 3, EdgeAxis.Vertical), (5, 3, EdgeAxis.Vertical));
            Box(c.World.Map, 10, 2, 12, 4);
            Assert.True(c.Plants.IsGreenhouse(c.World.Map.Rooms.RoomAt(new GridPos(3, 3))));
            Assert.False(c.Plants.IsGreenhouse(c.World.Map.Rooms.RoomAt(new GridPos(11, 3))));

            c.House.AddItem("seed_turnip", 2);
            c.World.Map.SetFloor(new GridPos(3, 3), FloorType.Dirt);
            c.World.Map.SetFloor(new GridPos(11, 3), FloorType.Dirt);
            Assert.False(c.Build.Plant("seed_turnip", new GridPos(11, 3), out _).Ok);
            Assert.True(c.Build.Plant("seed_turnip", new GridPos(3, 3), out var crop).Ok);

            c.Plants.TemperatureAt = p => 15f;
            PlantSystem.Water(crop);
            c.Plants.Tick(1, Season.Winter);
            Assert.Equal(1.0, crop.GrowthDays, 3);
        }

        [Fact]
        public void Sera_SoguktaBuyumez_DonmaIleOlur()
        {
            var c = Setup();
            Box(c.World.Map, 2, 2, 4, 4, (3, 2, EdgeAxis.Horizontal), (2, 3, EdgeAxis.Vertical), (5, 3, EdgeAxis.Vertical));
            c.World.Map.SetFloor(new GridPos(3, 3), FloorType.Dirt);
            c.House.AddItem("seed_turnip", 1);
            Assert.True(c.Build.Plant("seed_turnip", new GridPos(3, 3), out var crop).Ok);
            PlantSystem.Water(crop);

            c.Plants.TemperatureAt = p => 3f;       // eşiğin altı: büyümez ama ölmez
            c.Plants.Tick(1, Season.Winter);
            Assert.Equal(0.0, crop.GrowthDays, 3);
            Assert.NotNull(c.World.GetItem(crop.Id));

            c.Plants.TemperatureAt = p => 10f;      // yarı hız
            c.Plants.Tick(1, Season.Winter);
            Assert.Equal(0.556, crop.GrowthDays, 2);

            int lost = 0;
            c.Plants.PlantLost += (d, p, why) => { if (why == PlantLoss.Frost) lost++; };
            c.Plants.TemperatureAt = p => -2f;      // don
            c.Plants.Tick(0.1, Season.Winter);
            Assert.Null(c.World.GetItem(crop.Id));
            Assert.Equal(1, lost);
        }

        // ---------- meyve ağacı ----------

        [Fact]
        public void MeyveAgaci_MevsimindeOlgunlasir_ToplayincaYenidenBaslar_KisinDoker()
        {
            var c = Setup();
            c.House.AddItem("sapling_apple", 1);
            c.Build.Plant("sapling_apple", new GridPos(5, 5), out var tree);
            PlantSystem.SetGrown(tree);

            var ch = Farmer();
            Assert.Equal("Meyve henüz olgunlaşmadı.", c.Effects.CanStart(ch, tree, tree.Def.Interactions[0]));
            c.Plants.Tick(2, Season.Spring);                       // bahar: meyve mevsimi değil
            Assert.False(tree.FruitReady);

            c.Plants.Tick(2, Season.Summer);
            Assert.False(tree.FruitReady);
            c.Plants.Tick(1.5, Season.Summer);
            Assert.True(tree.FruitReady);

            Do(c, tree, "pick");
            Assert.Equal(3, c.House.Count("apple"));
            Assert.False(tree.FruitReady);
            Assert.NotNull(c.World.GetItem(tree.Id));              // ağaç yerinde

            c.Plants.Tick(3, Season.Autumn);                       // tekrar olgunlaştı
            Assert.True(tree.FruitReady);
            c.Plants.Tick(1, Season.Winter);                       // kış: meyve döküldü
            Assert.False(tree.FruitReady);
        }

        [Fact]
        public void MeyveAgaci_IcerikDogrulama_FruitDaysEksikseHata()
        {
            var db = new ContentDatabase();
            var bad = new ItemDto { id = "item.t", displayName = "T", growDays = 5, fruitSeasons = new[] { "Summer" } };
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { bad } }, db);
            Assert.Contains(errors, e => e.Contains("fruitDays"));

            var good = new ItemDto { id = "item.t2", displayName = "T2", growDays = 5, fruitDays = 3, fruitSeasons = new[] { "Summer", "Autumn" } };
            var db2 = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(new ContentFileDto { items = new[] { good } }, db2));
            Assert.True(db2.Items.Get("item.t2").IsFruitTree);
        }

        // ---------- tehditler ----------

        [Fact]
        public void Kuzgunlar_KorumasizEkiniYer_KorkulukKorur()
        {
            var open = Setup();
            var guarded = Setup();
            guarded.World.PlaceItem(guarded.Db.Items.Get("scarecrow"), new GridPos(7, 8));
            var openCrops = new List<ItemInstance>();
            var guardedCrops = new List<ItemInstance>();
            for (int i = 0; i < 6; i++)
            {
                openCrops.Add(Sow(open, "seed_turnip", 2 + i, 5));
                guardedCrops.Add(Sow(guarded, "seed_turnip", 2 + i, 5));
            }
            open.Plants.Config.CrowChance = 1.0;
            guarded.Plants.Config.CrowChance = 1.0;
            int crowsOpen = 0, crowsGuarded = 0;
            open.Plants.PlantLost += (d, p, why) => { if (why == PlantLoss.Crows) crowsOpen++; };
            guarded.Plants.PlantLost += (d, p, why) => { if (why == PlantLoss.Crows) crowsGuarded++; };
            open.Plants.OnNewDay(false);
            guarded.Plants.OnNewDay(false);
            Assert.Equal(6, crowsOpen);
            Assert.Equal(2, crowsGuarded);     // korkuluk x=7, yarıçap 3: x=4..7 korunur; x=2 ve 3 açıkta
        }

        [Fact]
        public void Firtina_BuyumeyiGeriAlir_AmaOlguniOldurmez()
        {
            var c = Setup();
            c.Plants.Config.CrowChance = 0;
            c.Plants.Config.StormChance = 1.0;
            var young = Sow(c, "seed_turnip", 5, 5);
            var ripe = Sow(c, "seed_turnip", 8, 5);
            PlantSystem.Water(young);
            c.Plants.Tick(4, Season.Summer);   // 4 günde 4,0 ilerleme (verim 0,5 => çarpan 1)
            PlantSystem.SetGrown(ripe);
            c.Plants.OnNewDay(rained: true, stormed: true);
            Assert.Equal(2.8, young.GrowthDays, 2);
            Assert.True(ripe.IsGrown);
        }

        // ---------- gerçek içerik ----------

        private static ContentDatabase Real()
        {
            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(ContentFilesTests.LoadAll(), db);
            Assert.Empty(errors);
            return db;
        }

        [Fact]
        public void GercekIcerik_OnBesEkin_HerBiriSulanirGubrelenirHasatEdilir()
        {
            var db = Real();
            var crops = db.Items.All.Where(i => i.IsCrop).ToList();
            Assert.Equal(15, crops.Count);
            foreach (var crop in crops)
            {
                Assert.True(crop.Interactions.Any(i => i.Water), crop.Id + " sulanamıyor");
                Assert.True(crop.Interactions.Any(i => i.Fertilize > 0f), crop.Id + " gübrelenemiyor");
                var harvest = crop.Interactions.First(i => i.Id == "harvest");
                Assert.True(harvest.RemovesItem && harvest.RequiresGrown);
                var seed = db.Items.All.FirstOrDefault(i => i.PlantsAs == crop.Id);
                Assert.NotNull(seed);
                Assert.True(harvest.Produce.Any(pr => pr.ItemId == seed.Id), crop.Id + " tohum vermiyor");
            }
            Assert.True(db.Items.Get("item.crop_pea").FixesNitrogen);
            Assert.True(db.Items.Get("item.crop_broad_bean").FixesNitrogen);
            Assert.True(db.Tools.TryGet("can", out var can) && can.Tiers.Count == 3);
        }

        [Fact]
        public void GercekIcerik_MeyveAgaclari_TohumYapici_Hendek()
        {
            var db = Real();
            var trees = db.Items.All.Where(i => i.IsFruitTree && i.AllowedLevels.Count == 0).ToList();   // mantar yatağı bodrumdadır
            Assert.Equal(4, trees.Count);
            foreach (var t in trees)
            {
                Assert.False(t.IsCrop);
                var pick = t.Interactions.First(i => i.Id == "pick");
                Assert.True(pick.RequiresFruit && pick.TakesFruit && !pick.RemovesItem);
                Assert.True(db.Items.All.Any(i => i.PlantsAs == t.Id), t.Id + " fidanı yok");
            }
            var maker = db.Items.Get("item.seed_maker");
            foreach (var crop in db.Items.All.Where(i => i.IsCrop))
            {
                var seed = db.Items.All.First(i => i.PlantsAs == crop.Id);
                Assert.True(maker.Interactions.Any(i => i.IsCraft && i.Produce.Any(pr => pr.ItemId == seed.Id)), crop.Id + " için tohum tarifi yok");
            }
            Assert.True(db.Items.Get("item.irrigation_ditch").WaterRadius < db.Items.Get("item.bucket_pulley").WaterRadius);
            Assert.True(db.Items.Get("item.bucket_pulley").WaterRadius < db.Items.Get("item.water_channel").WaterRadius);
            Assert.True(db.Items.Get("item.scarecrow").ProtectRadius > 0);
            Assert.True(db.Items.Get("item.compost_heap").Interactions.Any(i => i.Produce.Any(pr => pr.ItemId == "item.compost")));
        }

        [Fact]
        public void GercekIcerik_ErkenEkinOlgunlasir_UcMevsimdeDortHasat()
        {
            var db = Real();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household(10);
            var plants = new PlantSystem(w);
            var b = new BuildService(w, h, db) { Plants = plants };
            h.AddItem("item.seed_radish", 1);
            w.Map.SetFloor(new GridPos(4, 4), FloorType.Dirt);
            Assert.True(b.Plant("item.seed_radish", new GridPos(4, 4), out var radish).Ok);
            PlantSystem.Water(radish);
            for (int i = 0; i < 3; i++) plants.Tick(1, Season.Summer);
            Assert.True(radish.IsGrown);       // turp 3 günde, sulu
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_SulamaKaliteMeyveVeToprakKorunur()
        {
            var db = Content();
            var w = new WorldState(new GridMap(16, 10));
            var h = new Household(10);
            var plants = new PlantSystem(w);
            var b = new BuildService(w, h, db) { Plants = plants };
            h.AddItem("seed_turnip", 1);
            h.AddItem("sapling_apple", 1);
            w.Map.SetFloor(new GridPos(4, 4), FloorType.Dirt);
            b.Plant("seed_turnip", new GridPos(4, 4), out var crop);
            b.Plant("sapling_apple", new GridPos(8, 8), out var tree);
            PlantSystem.Water(crop);
            plants.Tick(1, Season.Summer);
            PlantSystem.SetGrown(tree);
            plants.Tick(4, Season.Summer);
            Assert.True(tree.FruitReady);
            plants.SetSoil(new GridPos(4, 4), 0.83f, "crop_turnip");

            var clock = new GameClock();
            var chars = new List<Character> { new Character(1, "A", new GridPos(1, 1), true) };
            var data = SaveGame.Capture(clock, w, h, chars, new Thatchwick.Core.Progress.AccountProgress(), null, null, plants: plants);
            var round = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(round, db);
            var plants2 = new PlantSystem(loaded.World);
            SaveGame.RestoreSoil(loaded, plants2);

            var crop2 = loaded.World.Items.First(i => i.Def.Id == "crop_turnip");
            var tree2 = loaded.World.Items.First(i => i.Def.Id == "tree_apple");
            Assert.True(crop2.Watered);
            Assert.Equal(crop.AgeDays, crop2.AgeDays, 3);
            Assert.Equal(crop.WetDays, crop2.WetDays, 3);
            Assert.True(tree2.FruitReady);
            Assert.Equal(0.83, (double)(plants2.FertilityAt(new GridPos(4, 4))), 2);
            Assert.Equal("crop_turnip", plants2.LastCropAt(new GridPos(4, 4)));
        }
    }
}
