using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Dalga 1: marangoz tezgâhında üretim, duvar eşyaları, hayvanlar ve bunların kaydı.
    public class WorkshopTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        // JSON yolunu da sınamak için içerik DTO üzerinden kurulur (tarifler ve hayvanlar mapper'dan geçer).
        internal static ContentDatabase Content()
        {
            var file = new ContentFileDto
            {
                items = new[]
                {
                    new ItemDto { id = "plank", displayName = "Tahta", placement = "Resource", price = 2 },
                    new ItemDto { id = "nails", displayName = "Çivi", placement = "Resource", price = 1 },
                    new ItemDto { id = "feed", displayName = "Yem", placement = "Resource", price = 1 },
                    new ItemDto { id = "egg", displayName = "Yumurta", placement = "Resource", price = 3, tags = new[] { "food" } },
                    new ItemDto { id = "stool", displayName = "Tabure", price = 8, styleSet = "Köylü" },
                    new ItemDto { id = "chair", displayName = "Oymalı sandalye", price = 40 },
                    new ItemDto { id = "workbench", displayName = "Marangoz tezgâhı", width = 2, price = 45 },
                    new ItemDto { id = "torch", displayName = "Duvar meşalesi", placement = "Wall", price = 12, tags = new[] { "light" } },
                    new ItemDto
                    {
                        id = "wall_shelf", displayName = "Duvar rafı", placement = "Wall", price = 10,
                        surfaceSlots = new[] { new SurfaceSlotDto { id = "a", u = 0.3f, v = 0.5f }, new SurfaceSlotDto { id = "b", u = 0.7f, v = 0.5f } }
                    },
                    new ItemDto { id = "cup", displayName = "Kil bardak", placement = "Surface", price = 2 },
                    new ItemDto
                    {
                        id = "coop", displayName = "Kümes", width = 2, height = 2, price = 40, housing = "coop", capacity = 2,
                        interactions = new[]
                        {
                            new InteractionDto { id = "feed", displayName = "Yem koy", durationDays = 0.05, materials = new[] { new ConsumeDto { item = "feed", count = 3 } }, addFeed = 3 },
                            new InteractionDto { id = "collect", displayName = "Topla", durationDays = 0.05, collect = true, skill = "Farming", skillXp = 3, moodlet = "good_harvest" }
                        }
                    },
                    new ItemDto
                    {
                        id = "kennel", displayName = "Köpek kulübesi", price = 15, housing = "kennel", capacity = 1,
                        interactions = new[]
                        {
                            new InteractionDto { id = "play", displayName = "Köpekle oyna", need = "Fun", restore = 40, durationDays = 0.08, requiresAnimal = true, moodlet = "pet_love" }
                        }
                    }
                },
                recipes = new[]
                {
                    new RecipeDto { id = "stool", station = "workbench", produces = "stool", materials = new[] { new ConsumeDto { item = "plank", count = 2 } }, durationDays = 0.1, skillXp = 20 },
                    new RecipeDto { id = "chair", station = "workbench", produces = "chair", materials = new[] { new ConsumeDto { item = "plank", count = 3 }, new ConsumeDto { item = "nails", count = 2 } }, minLevel = 3, durationDays = 0.2, skillXp = 40 }
                },
                animals = new[]
                {
                    new AnimalDto { id = "chicken", displayName = "Tavuk", price = 6, housing = "coop", feedPerDay = 1, produces = "egg", produceEveryDays = 1, lifespanDays = 60, matureDays = 1 },
                    new AnimalDto { id = "dog", displayName = "Köpek", price = 20, housing = "kennel", feedPerDay = 0.5f, pet = true, lifespanDays = 100, matureDays = 1 }
                }
            };
            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(file, db);
            Assert.Empty(errors);
            return db;
        }

        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public InteractionEffects Effects;
            public AnimalSystem Animals;
            public Character C;
            public JobRunner Runner;
        }

        private static Ctx Make()
        {
            var x = new Ctx { Db = Content() };
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int i = 0; i < 16; i++) map.SetOwned(new GridPos(i, y), true);
            x.World = new WorldState(map);
            x.House = new Household(500);
            x.Build = new BuildService(x.World, x.House, x.Db);
            x.Animals = new AnimalSystem(x.World, x.Db);
            x.Build.HousingResidents = x.Animals.Residents;
            x.Effects = new InteractionEffects(x.World, x.House, x.Db) { Animals = x.Animals };
            x.C = new Character(1, "Wat", new GridPos(0, 0)) { MoodEnabled = true };
            x.C.SetAge(40);
            x.Runner = new JobRunner(x.C, x.World) { Autonomous = false, Effects = x.Effects };
            return x;
        }

        // ---- üretim ----

        [Fact]
        public void Tarif_IstasyonaCraftEtkilesimiOlarakEklenir()
        {
            var db = Content();
            var bench = db.Items.Get("workbench");
            var craft = bench.Interactions.Single(i => i.Id == "craft:chair");
            Assert.True(craft.IsCraft);
            Assert.Equal(SkillType.Carpentry, craft.Skill);
            Assert.Equal(3, craft.MinSkillLevel);
            Assert.Equal("Yap: Oymalı sandalye", craft.DisplayName);
            Assert.Equal(MoodCatalog.Proud, craft.Moodlet);
            Assert.Equal(5, craft.AllConsumed().Sum(c => c.Count));
        }

        [Fact]
        public void Uretim_MalzemeHarcar_EsyayiEnvantereVerir_GururluYapar()
        {
            var x = Make();
            var bench = x.World.PlaceItem(x.Db.Items.Get("workbench"), new GridPos(3, 0));
            x.House.AddItem("plank", 3);
            Assert.True(x.Runner.Command(Job.UseItem(bench.Id, "craft:stool")));
            x.Runner.Tick(0.3);
            Assert.Null(x.Runner.LastFailure);
            Assert.Equal(1, x.House.Count("stool"));
            Assert.Equal(1, x.House.Count("plank"));
            Assert.True(x.C.Skills.GetXp(SkillType.Carpentry) > 0);
            Assert.True(x.C.Mood.Has(MoodCatalog.Proud));
        }

        [Fact]
        public void Uretim_EksikMalzemeVeDusukSeviyede_Baslamaz()
        {
            var x = Make();
            var bench = x.World.PlaceItem(x.Db.Items.Get("workbench"), new GridPos(3, 0));
            x.House.AddItem("plank", 1);
            x.Runner.Command(Job.UseItem(bench.Id, "craft:stool"));
            x.Runner.Tick(0.3);
            Assert.Equal("Tahta yetmiyor (1/2).", x.Runner.LastFailure);

            x.House.AddItem("plank", 5);
            x.House.AddItem("nails", 2);
            x.Runner.Command(Job.UseItem(bench.Id, "craft:chair"));
            x.Runner.Tick(0.3);
            Assert.Equal("Marangozluk 3. seviye gerekli.", x.Runner.LastFailure);
            Assert.Equal(0, x.House.Count("chair"));

            x.C.Skills.AddXp(SkillType.Carpentry, SkillRules.XpForLevel(3), SkillRules.MaxLevel);
            x.Runner.Command(Job.UseItem(bench.Id, "craft:chair"));
            x.Runner.Tick(0.5);
            Assert.Equal(1, x.House.Count("chair"));
            Assert.Equal(0, x.House.Count("nails"));
        }

        [Fact]
        public void Otonomi_UretimYapmaz()
        {
            var x = Make();
            x.World.PlaceItem(x.Db.Items.Get("workbench"), new GridPos(3, 0));
            x.House.AddItem("plank", 10);
            x.C.Needs.Set(NeedType.Fun, 5f);
            x.Runner.Autonomous = true;
            x.Runner.Tick(0.5);
            Assert.Equal(0, x.House.Count("stool"));
        }

        // ---- duvar eşyaları ----

        private static void Wall(WorldState w, int x, int y, EdgeAxis axis) => w.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall);

        [Fact]
        public void DuvarEsyasi_YalnizcaDuzDuvaraAsilir_KaroKaplamaz()
        {
            var x = Make();
            var tile = new GridPos(4, 4);
            x.House.AddItem("torch", 2);
            Assert.False(x.Build.PlaceWallFromInventory("torch", tile, 0, out _).Ok);   // duvar yok

            Wall(x.World, 4, 4, EdgeAxis.Horizontal);   // karonun 0. kenarı
            Assert.True(x.Build.PlaceWallFromInventory("torch", tile, 0, out var torch).Ok);
            Assert.True(torch.IsWallMounted);
            Assert.True(x.World.Map.IsWalkable(tile));
            Assert.Empty(torch.OccupiedTiles());
            Assert.Null(x.World.GetItemAt(tile));
            Assert.Equal("Bu duvarda zaten bir eşya var.", x.Build.PlaceWallFromInventory("torch", tile, 0, out _).Reason);
            // aynı duvarın öbür yüzü (karo 4,3'ün 2. kenarı) ayrı yerdir
            Assert.True(x.Build.PlaceWallFromInventory("torch", new GridPos(4, 3), 2, out _).Ok);

            Assert.False(x.Build.PlaceFromInventory("torch", tile, 0, out _).Ok);   // yere konmaz
        }

        [Fact]
        public void DuvarEsyasi_KapiVeyaDuvarKalkincaEnvantereDoner_DekoruIle()
        {
            var x = Make();
            var tile = new GridPos(4, 4);
            Wall(x.World, 5, 4, EdgeAxis.Vertical);   // karonun 1. kenarı (+x)
            x.House.AddItem("wall_shelf");
            x.House.AddItem("cup");
            Assert.True(x.Build.PlaceWallFromInventory("wall_shelf", tile, 1, out var shelf).Ok);
            Assert.True(x.Build.PlaceDecorFromInventory("cup", shelf.Id, "a", out _).Ok);
            Assert.True(shelf.TryGetSlotPosition("a", out var gx, out var gy));
            Assert.True(gx > 4.8f && gx <= 5f);   // duvara yakın
            Assert.Equal(4.3, (double)gy, 3);

            Assert.True(x.Build.SetEdge(new Edge(5, 4, EdgeAxis.Vertical), EdgeKind.Door).Ok);
            Assert.Empty(x.World.Items);
            Assert.Equal(1, x.House.Count("wall_shelf"));
            Assert.Equal(1, x.House.Count("cup"));
        }

        [Fact]
        public void DuvarEsyasi_OnundekiKarodanKullanilir_VeOdaIsigiSayilir()
        {
            var x = Make();
            // 2x2 oda: (2..3, 2..3)
            for (int i = 2; i <= 3; i++)
            {
                Wall(x.World, i, 2, EdgeAxis.Horizontal); Wall(x.World, i, 4, EdgeAxis.Horizontal);
                Wall(x.World, 2, i, EdgeAxis.Vertical); Wall(x.World, 4, i, EdgeAxis.Vertical);
            }
            var torch = x.World.PlaceOnWall(x.Db.Items.Get("torch"), new GridPos(3, 3), 1);
            var room = x.World.Map.Rooms.RoomAt(new GridPos(2, 2));
            Assert.True(room.IsIndoor);
            Assert.True(RoomMoodSystem.HasLight(room, x.World));
            x.C.Position = new GridPos(2, 2);
            new RoomMoodSystem(x.World).Tick(0.1, new[] { x.C });
            Assert.True(x.C.Mood.Has(MoodCatalog.BrightRoom));
        }

        [Fact]
        public void DuvarEsyasi_TasinirVeKaydedilir()
        {
            var x = Make();
            Wall(x.World, 4, 4, EdgeAxis.Horizontal);
            Wall(x.World, 7, 7, EdgeAxis.Vertical);
            var torch = x.World.PlaceOnWall(x.Db.Items.Get("torch"), new GridPos(4, 4), 0);
            Assert.True(x.Build.Move(torch.Id, new GridPos(7, 7), 3).Ok);
            Assert.Equal(new GridPos(7, 7), torch.Position);

            var data = SaveGame.Capture(new GameClock(), x.World, x.House, new[] { x.C }, null, null, null);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, x.Db);
            Assert.Empty(loaded.Warnings);
            var t = loaded.World.Items.Single();
            Assert.True(t.IsWallMounted);
            Assert.Equal(3, t.Rotation);
        }

        // ---- hayvanlar ----

        [Fact]
        public void Hayvan_BarinaksizAlinamaz_KapasiteKadarAlinir()
        {
            var x = Make();
            Assert.False(x.Animals.Buy("chicken", x.House, out _).Ok);
            x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            Assert.True(x.Animals.Buy("chicken", x.House, out var a).Ok);
            Assert.True(x.Animals.Buy("chicken", x.House, out _).Ok);
            Assert.False(x.Animals.Buy("chicken", x.House, out _).Ok);   // kapasite 2
            Assert.Equal(500 - 12, x.House.Akce);
            Assert.True(x.World.Map.IsWalkable(a.Position));
        }

        [Fact]
        public void Hayvan_YemleTokKalir_UrunBirakir_ToplaHaneyeAlir()
        {
            var x = Make();
            var coop = x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            x.Animals.Buy("chicken", x.House, out var hen);
            x.House.AddItem("feed", 6);
            x.Runner.Command(Job.UseItem(coop.Id, "feed"));
            x.Runner.Tick(0.2);
            Assert.Null(x.Runner.LastFailure);
            Assert.Equal(3.0, coop.FeedStock, 6);
            Assert.Equal(3, x.House.Count("feed"));

            for (int i = 0; i < 29; i++) x.Animals.Tick(0.1);   // ~3 gün: 1 gün sonra erişkin, günde 1 yumurta; yem 3 gün yeter
            Assert.True(hen.Fullness > 50f);
            Assert.True(coop.Stock.TryGetValue("egg", out int eggs) && eggs >= 1);
            Assert.True(coop.FeedStock < 3.0);

            x.Runner.Command(Job.UseItem(coop.Id, "collect"));
            x.Runner.Tick(0.2);
            Assert.Equal(eggs, x.House.Count("egg"));
            Assert.Equal(0, coop.StockTotal);
            Assert.True(x.C.Mood.Has(MoodCatalog.GoodHarvest));

            x.Runner.Command(Job.UseItem(coop.Id, "collect"));
            x.Runner.Tick(0.2);
            Assert.Equal("Toplanacak ürün yok.", x.Runner.LastFailure);
        }

        [Fact]
        public void Hayvan_YemsizKalirsaOlur_OlayTetiklenir()
        {
            var x = Make();
            x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            x.Animals.Buy("chicken", x.House, out var hen);
            Animal died = null;
            x.Animals.Died += a => died = a;
            for (int i = 0; i < 60; i++) x.Animals.Tick(0.1);   // 2 gün tokluk biter, 3 gün açlık
            Assert.Same(hen, died);
            Assert.Equal(AnimalDeath.Starvation, hen.CauseOfDeath);
            Assert.Empty(x.Animals.Animals);
        }

        [Fact]
        public void Hayvan_BarinaginCevresindeGezinir_DuvardanGecmez()
        {
            var x = Make();
            var coop = x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            x.Animals.Buy("chicken", x.House, out var hen);
            var seen = new HashSet<GridPos>();
            for (int i = 0; i < 400; i++)
            {
                x.Animals.Tick(0.002);
                seen.Add(hen.Position);
                Assert.True(x.World.Map.IsWalkable(hen.Position));
                Assert.True(System.Math.Abs(hen.Position.X - 5) <= AnimalSystem.WanderRadius + 1 && System.Math.Abs(hen.Position.Y - 5) <= AnimalSystem.WanderRadius + 1);
            }
            Assert.True(seen.Count > 2);
        }

        [Fact]
        public void EvcilHayvan_OynamakKeyifVerir_HayvansizBaslamaz()
        {
            var x = Make();
            var kennel = x.World.PlaceItem(x.Db.Items.Get("kennel"), new GridPos(4, 0));
            x.C.Needs.Set(NeedType.Fun, 10f);
            x.Runner.Command(Job.UseItem(kennel.Id, "play"));
            x.Runner.Tick(0.2);
            Assert.Equal("Burada hayvan yok.", x.Runner.LastFailure);

            x.Animals.Buy("dog", x.House, out _);
            x.Runner.Autonomous = true;   // otonom da gider: keyif düşük ve köpek var
            x.Runner.Tick(0.3);
            Assert.True(x.C.Needs.Get(NeedType.Fun) >= 50f);
            Assert.True(x.C.Mood.Has(MoodCatalog.PetLove));
        }

        [Fact]
        public void DoluBarinak_KaldirilamazVeSatilamaz()
        {
            var x = Make();
            var coop = x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            x.Animals.Buy("chicken", x.House, out var hen);
            Assert.False(x.Build.StoreToInventory(coop.Id).Ok);
            Assert.False(x.Build.SellPlaced(coop.Id).Ok);
            Assert.True(x.Animals.Sell(hen.Id, x.House).Ok);
            Assert.True(x.Build.StoreToInventory(coop.Id).Ok);
        }

        [Fact]
        public void Kayit_HayvanlarBarinakStoguVeMoodletlerGeriGelir()
        {
            var x = Make();
            var coop = x.World.PlaceItem(x.Db.Items.Get("coop"), new GridPos(5, 5));
            x.Animals.Buy("chicken", x.House, out var hen);
            AnimalSystem.AddFeed(coop, 4.5);
            AnimalSystem.PutStock(coop, "egg", 2);
            x.C.Needs.Set(NeedType.Fun, 33f);
            x.C.Needs.Set(NeedType.Hygiene, 44f);
            x.C.AddMoodlet(MoodCatalog.Rested);

            var data = SaveGame.Capture(new GameClock(), x.World, x.House, new[] { x.C }, null, null, null, animals: x.Animals);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, x.Db);
            var sys = new AnimalSystem(loaded.World, x.Db);
            SaveGame.RestoreAnimals(loaded, sys, x.Db);
            Assert.Empty(loaded.Warnings);

            var c = loaded.Characters.Single();
            Assert.Equal(33.0, (double)c.Needs.Get(NeedType.Fun), 3);
            Assert.Equal(44.0, (double)c.Needs.Get(NeedType.Hygiene), 3);
            Assert.True(c.Mood.Has(MoodCatalog.Rested));

            var lc = loaded.World.GetItem(coop.Id);
            Assert.Equal(4.5, lc.FeedStock, 6);
            Assert.Equal(2, lc.Stock["egg"]);
            var la = sys.Animals.Single();
            Assert.Equal(hen.Id, la.Id);
            Assert.Equal(coop.Id, la.HomeItemId);
            Assert.Equal(1, sys.Residents(coop.Id));
            Assert.True(sys.NextId > hen.Id);
        }

        [Fact]
        public void IcerikDogrulama_BarinaksizHayvan_VeBarinakDisiToplama()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "egg", DisplayName = "Yumurta", Placement = ItemPlacement.Resource });
            db.Items.Register(new ItemDef
            {
                Id = "box", DisplayName = "Kutu",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "c", DisplayName = "Topla", DurationDays = 0.1, Collect = true } }
            });
            db.Animals.Register(new AnimalSpeciesDef { Id = "hen", DisplayName = "Tavuk", Housing = "coop", Produces = "egg" });
            var errors = db.Validate();
            Assert.Contains(errors, e => e.Contains("yalnızca barınakta"));
            Assert.Contains(errors, e => e.Contains("barınak eşyası yok"));
        }

        // ---- gerçek içerik ----

        internal static ContentDatabase RealContent(out List<string> errors)
        {
            var path = Path.Combine(System.AppContext.BaseDirectory, "GameContent", "items.json");
            var file = JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(path), Json);
            var db = new ContentDatabase();
            errors = ContentMapper.Apply(file, db);
            return db;
        }

        [Fact]
        public void GercekIcerik_ItemsJson_Gecerli()
        {
            var db = RealContent(out var errors);
            Assert.Empty(errors);
            Assert.True(db.Items.Count >= 50);
            Assert.True(db.Animals.Count >= 6);
            Assert.Contains(db.Items.All, d => d.Placement == ItemPlacement.Wall);
            Assert.Contains(db.Items.All, d => d.Interactions.Any(i => i.IsCraft));
            // Her setten ve her ihtiyaçtan (keyif, hijyen dahil) en az bir eşya var.
            foreach (var set in new[] { "Köylü", "Zanaatkâr", "Tüccar" }) Assert.Contains(db.Items.All, d => d.StyleSet == set);
            foreach (var need in new[] { NeedType.Hunger, NeedType.Energy, NeedType.Warmth, NeedType.Fun, NeedType.Hygiene })
                Assert.Contains(db.Items.All, d => d.Interactions.Any(i => i.RestoresNeed && i.Need == need));
            // Satın alma menüsü: her kategoride en az 5 eşya, her eşya bilinen bir kategoride, kartı boş değil.
            foreach (var (id, _) in ItemCatalog.Categories)
                Assert.True(ItemCatalog.InCategory(db, id).Count >= 5, $"{id}: {ItemCatalog.InCategory(db, id).Count} eşya");
            var known = new HashSet<string>(ItemCatalog.Categories.Select(c => c.id)) { "resource", "plant", "village" };
            foreach (var d in db.Items.All) Assert.True(known.Contains(d.Category), $"{d.Id}: kategori {d.Category}");
            Assert.StartsWith("Enerji +70 (Dinlenmiş)", ItemCatalog.Describe(db.Items.Get("item.bed_frame")));
            Assert.Contains("Işık", ItemCatalog.Describe(db.Items.Get("item.torch_wall")));
            Assert.Contains("Duvara asılır", ItemCatalog.Describe(db.Items.Get("item.torch_wall")));
            Assert.Contains("12 tarif", ItemCatalog.Describe(db.Items.Get("item.workbench")));
        }
    }
}
