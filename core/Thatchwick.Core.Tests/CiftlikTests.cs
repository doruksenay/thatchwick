using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Çiftlik ve hayvancılık (17.11.2026): barınak odası, yemlik, konfor, otlak, at ve eyer, deri zinciri,
    // sulama ağı, su çarkı ve değirmen taşı, toplama sandığı, yem oluğu, görevli aile üyesi, canlı modda çapalama.
    public class CiftlikTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public RoomTemplateService Templates;
            public AnimalSystem Animals;
            public InteractionEffects Effects;
            public AnimalCare Care;
        }

        private static Ctx Make(int size = 24)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            var x0 = new Ctx { Db = db, World = new WorldState(map), House = new Household(20000) };
            x0.Build = new BuildService(x0.World, x0.House, db);
            x0.Templates = new RoomTemplateService(x0.Build, x0.World, x0.House, db);
            x0.Animals = new AnimalSystem(x0.World, db);
            x0.Build.HousingResidents = x0.Animals.Residents;
            x0.Effects = new InteractionEffects(x0.World, x0.House, db) { Animals = x0.Animals };
            x0.Care = new AnimalCare(x0.Animals, x0.House);
            return x0;
        }

        // Ahır odası şablonu (2,2)'de: 6x4, güneyde (3,5)-(3,6) arasında kapı. Üç bölme, yemlik, su yalağı, saman, fener.
        private static List<ItemInstance> Barn(Ctx x, string template = "template.barn_room", int ox = 2, int oy = 2)
        {
            var r = x.Templates.Build(template, new GridPos(ox, oy), out var placed);
            Assert.True(r.Ok, r.Reason);
            return placed;
        }

        private static HashSet<GridPos> RoomTiles(Ctx x, GridPos inside) => new HashSet<GridPos>(x.World.Map.Rooms.RoomAt(inside).Tiles);

        private static void Run(AnimalSystem a, double days, double step = 0.01)
        {
            for (double t = 0; t < days - 1e-9; t += step) a.Tick(step);
        }

        // ---------- içerik ----------

        [Fact]
        public void Icerik_AtEyerDeriVeDuzenekler()
        {
            var x = Make();
            var horse = x.Db.Animals.Get("animal.horse");
            Assert.True(horse.Rideable);
            Assert.Equal("stable", horse.Housing);
            Assert.Equal("item.hide", x.Db.Animals.Get("animal.cow").Hide);
            Assert.Equal(2, x.Db.Animals.Get("animal.cow").HideCount);
            Assert.True(x.Db.Items.Get("item.well").WaterSource);
            Assert.True(x.Db.Items.Get("item.canal").NeedsWaterSource);
            Assert.NotNull(x.Db.Items.Get("item.millstone").AutoProcess);
            Assert.True(x.Db.Items.Get("item.saddle_fine").HasTag("craftonly"));
            foreach (var id in new[] { "template.coop_room", "template.barn_room", "template.stable_room" }) Assert.True(x.Db.Templates.Contains(id), id);
            Assert.True(ItemCatalog.InCategory(x.Db, "barn").Count >= 10);
            Assert.Contains("Kapalı odaya", ItemCatalog.Describe(x.Db.Items.Get("item.stall")));
        }

        [Fact]
        public void Bolme_YalnizcaKapaliOdayaKonur()
        {
            var x = Make();
            var nest = x.Db.Items.Get("item.nest_box");
            var outside = x.World.CanPlaceFloor(nest, new GridPos(15, 15), 0);
            Assert.False(outside.Ok);
            Assert.Equal(WorldState.IndoorOnly, outside.Reason);
            Barn(x, "template.coop_room");
            Assert.Equal(3, x.World.Items.Count(i => i.Def.Id == "item.nest_box"));
            Assert.True(x.World.CanPlaceFloor(nest, new GridPos(3, 3), 0).Ok);
        }

        // ---------- barınak odası ----------

        [Fact]
        public void Ahir_HayvanlarOdadaKalirEveGirmez()
        {
            var x = Make();
            Barn(x);
            for (int i = 0; i < 3; i++) Assert.True(x.Animals.Buy("animal.cow", x.House, out _).Ok);
            Assert.False(x.Animals.Buy("animal.cow", x.House, out _).Ok);           // 3 bölme = 3 inek
            var room = RoomTiles(x, new GridPos(3, 4));
            x.Animals.Released = false;
            for (int k = 0; k < 200; k++)
            {
                x.Animals.Tick(0.01);
                foreach (var a in x.Animals.Animals) Assert.Contains(a.Position, room);
            }
        }

        [Fact]
        public void Yemlik_OdadakiHayvanlarYemliktenYer()
        {
            var x = Make();
            var placed = Barn(x);
            var trough = placed.Single(i => i.Def.Id == "item.feed_trough");
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            x.Animals.Released = false;
            AnimalSystem.AddFeed(trough, 10);
            Run(x.Animals, 1.0, 0.05);
            Assert.Equal(10 - 3 * 2.5, trough.FeedStock, 3);
            Assert.All(x.Animals.Animals, a => Assert.Equal(0.0, a.HungryDays, 6));
            Assert.Equal(7.5, x.Animals.DailyFeedNeed(trough), 6);
        }

        [Fact]
        public void Konfor_KurulmusAhirHazirBarinaktanIyi()
        {
            var x = Make();
            var placed = Barn(x);
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            var stall = placed.First(i => i.Def.Id == "item.stall");
            var good = x.Animals.ComfortOf(stall);
            Assert.True(good.IsRoom);
            Assert.True(good.Total >= 80f, $"ahır konforu {good.Total} ({good.Hint()})");

            var coop = x.World.PlaceItem(x.Db.Items.Get("item.coop"), new GridPos(16, 16));
            var poor = x.Animals.ComfortOf(coop);
            Assert.False(poor.IsRoom);
            Assert.True(poor.Total < 70f);
            Assert.Contains("kapalı oda", poor.Hint());

            x.Animals.OnNewDay(Season.Spring);
            Assert.All(x.Animals.Animals, a => Assert.Equal((double)good.Total, (double)a.Comfort, 3));
        }

        [Fact]
        public void Konfor_UrunuHizlandirir()
        {
            var x = Make();
            var placed = Barn(x);
            var stall = placed.First(i => i.Def.Id == "item.stall");
            var cow = x.Db.Animals.Get("animal.cow");
            var low = x.Animals.Restore(100, cow, new GridPos(3, 4), stall.Id, 100f, 20, 0, 0, 0f, 0, null, 0f);
            var high = x.Animals.Restore(101, cow, new GridPos(4, 4), placed.Where(i => i.Def.Id == "item.stall").ElementAt(1).Id, 100f, 20, 0, 0, 0f, 0, null, 100f);
            AnimalSystem.AddFeed(stall, 50);
            AnimalSystem.AddFeed(placed.Where(i => i.Def.Id == "item.stall").ElementAt(1), 50);
            x.Animals.Released = false;
            x.Animals.Tick(0.4);
            Assert.True(high.ProduceProgress > low.ProduceProgress * 1.2, $"{high.ProduceProgress} vs {low.ProduceProgress}");
        }

        // ---------- otlak ----------

        [Fact]
        public void Otlak_GunduzOtlarCaninlaGeriDoner()
        {
            var x = Make();
            Barn(x);
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            var pasture = new HashSet<GridPos>();
            for (int y = 6; y <= 11; y++) for (int px = 2; px <= 8; px++) { var p = new GridPos(px, y); Assert.True(x.Build.SetZone(p, ZoneType.Pasture).Ok); pasture.Add(p); }
            bool outside = true;
            x.Animals.CanGoOut = () => outside;
            bool grazed = false;
            for (int k = 0; k < 300 && !grazed; k++)
            {
                x.Animals.Tick(0.01);
                grazed = x.Animals.ExportGrass().Count > 0;
            }
            Assert.True(grazed, "inekler otlağa çıkıp ot yemedi");
            Assert.Contains(x.Animals.Animals, a => pasture.Contains(a.Position));
            Assert.All(x.Animals.Animals, a => Assert.Equal(0.0, a.HungryDays, 6));   // yemlik boşken ot yetti

            // Gece: içeri döner.
            outside = false;
            Run(x.Animals, 1.0);
            var room = RoomTiles(x, new GridPos(3, 4));
            Assert.All(x.Animals.Animals, a => Assert.Contains(a.Position, room));

            // Ot büyür (kış hariç).
            var before = x.Animals.ExportGrass().ToDictionary(k => k.Key, k => k.Value);
            x.Animals.OnNewDay(Season.Winter);
            Assert.Equal(before.Count, x.Animals.ExportGrass().Count);
            for (int d = 0; d < 3; d++) x.Animals.OnNewDay(Season.Spring);
            Assert.Empty(x.Animals.ExportGrass());
        }

        [Fact]
        public void Otlak_CanCalinincaDisariCikmaz()
        {
            var x = Make();
            Barn(x);
            x.Animals.Buy("animal.cow", x.House, out _);
            for (int y = 6; y <= 11; y++) for (int px = 2; px <= 8; px++) x.Build.SetZone(new GridPos(px, y), ZoneType.Pasture);
            x.Animals.Released = false;
            var room = RoomTiles(x, new GridPos(3, 4));
            for (int k = 0; k < 200; k++) { x.Animals.Tick(0.01); Assert.Contains(x.Animals.Animals[0].Position, room); }
        }

        // ---------- at ----------

        [Fact]
        public void At_TayaBinilmezEyerleyipBinilir()
        {
            var x = Make();
            Barn(x, "template.stable_room");
            Assert.True(x.Animals.Buy("animal.horse", x.House, out var foal).Ok);
            var player = new Character(1, "Doruk", new GridPos(3, 4), true);
            player.SetAge(40);
            x.House.AddItem("item.saddle");
            Assert.Null(x.Care.CanDo(player, foal.Id, AnimalAction.Saddle));
            x.Care.Apply(player, foal.Id, AnimalAction.Saddle);
            Assert.Equal("item.saddle", foal.Saddle);
            Assert.Contains("tay", x.Care.CanDo(player, foal.Id, AnimalAction.Mount));

            // Erişkin ama güvenmeyen at.
            var stall = x.World.Items.Single(i => i.Def.Id == "item.horse_stall");
            x.Animals.Sell(foal.Id, x.House);
            Assert.Equal(1, x.House.Count("item.saddle"));                        // eyer satılan atla gitmez
            var horse = x.Animals.Restore(77, x.Db.Animals.Get("animal.horse"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0, 0f, 0, "item.saddle");
            x.House.TryTakeItem("item.saddle");
            Assert.Contains("güvenmiyor", x.Care.CanDo(player, horse.Id, AnimalAction.Mount));
            x.Care.Apply(player, horse.Id, AnimalAction.Brush);
            Assert.True(horse.Friendship >= AnimalSystem.MountFriendship);
            Assert.NotNull(x.Care.CanDo(player, horse.Id, AnimalAction.Brush));        // günde bir tımar

            x.Care.Apply(player, horse.Id, AnimalAction.Mount);
            Assert.Equal(player.Id, horse.RiderId);
            Assert.Equal(2.2, x.Animals.RideSpeed(player.Id), 3);
            Assert.NotNull(x.Care.CanDo(player, horse.Id, AnimalAction.Unsaddle));     // üstündeyken eyer çıkmaz

            // At biniciyle gider.
            x.Animals.RiderPosition = id => id == player.Id ? player.Position : (GridPos?)null;
            player.Position = new GridPos(10, 12);
            x.Animals.Tick(0.01);
            Assert.Equal(player.Position, horse.Position);

            // Binici bir eve girerse at dışarıda kalır.
            Assert.True(x.Build.SetEdgeLine(14, 14, 17, 14, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(x.Build.SetEdgeLine(14, 17, 17, 17, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(x.Build.SetEdgeLine(14, 14, 14, 17, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(x.Build.SetEdgeLine(17, 14, 17, 17, 0, EdgeKind.Wall, null, out _).Ok);
            var outsidePos = horse.Position;
            int dismounted = 0;
            x.Animals.Dismounted += (an, rid) => dismounted++;
            player.Position = new GridPos(15, 15);
            x.Animals.Tick(0.01);
            Assert.Equal(1, dismounted);
            Assert.Equal(0, horse.RiderId);
            Assert.NotEqual(player.Position, horse.Position);
            player.Position = outsidePos;
            Assert.True(x.Animals.Mount(player.Id, horse));

            Assert.Same(horse, x.Animals.Dismount(player.Id));
            Assert.Equal(0, horse.RiderId);
            Assert.Equal(1.0, x.Animals.RideSpeed(player.Id));
            x.Care.Apply(player, horse.Id, AnimalAction.Unsaddle);
            Assert.Null(horse.Saddle);
            Assert.Equal(1, x.House.Count("item.saddle"));
        }

        [Fact]
        public void At_BinenDahaHizliYururIslemeliEyerDahaDaHizli()
        {
            var x = Make(32);
            Barn(x, "template.stable_room");
            var stall = x.World.Items.Single(i => i.Def.Id == "item.horse_stall");
            var horse = x.Animals.Restore(5, x.Db.Animals.Get("animal.horse"), new GridPos(1, 10), stall.Id, 100f, 30, 0, 0, 50f, 0, "item.saddle_fine");
            var player = new Character(1, "Doruk", new GridPos(1, 10), true);
            player.SetAge(40);
            Assert.True(x.Animals.Mount(player.Id, horse));
            Assert.Equal(2.2 * 1.25, x.Animals.RideSpeed(player.Id), 3);

            var runner = new JobRunner(player, x.World) { Autonomous = false, WalkSpeed = () => x.Animals.RideSpeed(player.Id) };
            runner.Command(Job.MoveTo(new GridPos(21, 10)));
            runner.Tick(20.0 / JobRunner.TilesPerGameDay / 2.0);      // yaya hızıyla 10 karo
            Assert.Equal(new GridPos(21, 10), player.Position);
        }

        [Fact]
        public void HayvanIsi_YuruyupSever()
        {
            var x = Make();
            Barn(x);
            x.Animals.Buy("animal.cow", x.House, out var cow);
            var player = new Character(1, "Doruk", new GridPos(4, 9), true);
            player.SetAge(40);
            var runner = new JobRunner(player, x.World) { Autonomous = false, Animals = x.Care };
            Assert.True(runner.Command(Job.WithAnimal(cow.Id, AnimalAction.Pet)));
            for (int k = 0; k < 50 && !runner.IsIdle; k++) { runner.Tick(0.005); x.Animals.Tick(0.005); }
            Assert.True(runner.IsIdle, runner.LastFailure);
            Assert.True(cow.Friendship >= AnimalSystem.PetFriendship);
        }

        // ---------- deri ve eyer ----------

        [Fact]
        public void Kesim_DeriVerirEyerPazardaIslemelisiYapilir()
        {
            var x = Make();
            var placed = Barn(x);
            var stall = placed.First(i => i.Def.Id == "item.stall");
            x.Animals.Restore(9, x.Db.Animals.Get("animal.cow"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0);
            var butcher = new Character(1, "Doruk", new GridPos(3, 4), true);
            butcher.SetAge(40);
            var slaughter = stall.Def.Interactions.Single(i => i.Slaughter);
            Assert.Null(x.Effects.CanStart(butcher, stall, slaughter));
            x.Effects.Apply(butcher, stall, slaughter);
            Assert.Equal(2, x.House.Count("item.hide"));
            Assert.Equal(4, x.House.Count("item.meat"));

            Assert.Contains(x.Db.Items.Get("item.tanning_rack").Interactions, i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.leather"));
            Assert.Contains(x.Db.Items.Get("item.saddler_bench").Interactions, i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.saddle_fine"));
            Assert.Contains(x.Db.Items.Get("item.saddler_bench").Interactions, i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.saddle"));

            var market = new Market(x.Db, x.House, null, null, seed: 3);
            Assert.True(market.IsBuyable("item.saddle"));
            Assert.True(market.Buy("item.saddle").Ok);
            Assert.False(market.IsBuyable("item.saddle_fine"));
            Assert.False(market.Buy("item.saddle_fine").Ok);
            x.House.AddItem("item.saddle_fine");
            Assert.True(market.Sell("item.saddle_fine", 1) > 0);                     // satılır ama alınmaz
        }

        // ---------- sulama ağı ve mekanizmalar ----------

        private static ItemInstance Crop(Ctx x, GridPos p) => x.World.PlaceItem(x.Db.Items.Get("item.crop_turnip"), p);

        [Fact]
        public void Kanal_KaynagaBagliykenSular()
        {
            var x = Make();
            var plants = new PlantSystem(x.World);
            x.World.PlaceItem(x.Db.Items.Get("item.well"), new GridPos(2, 2));
            var canals = new List<ItemInstance>();
            for (int cx = 3; cx <= 8; cx++) canals.Add(x.World.PlaceItem(x.Db.Items.Get("item.canal"), new GridPos(cx, 2)));
            var crop = Crop(x, new GridPos(10, 4));
            plants.OnNewDay(false);
            Assert.True(crop.Watered);
            Assert.Equal(7, WaterNetwork.Connected(x.World).Count);              // kuyu + 6 kanal
            x.World.RemoveItem(canals[2].Id);                                    // zincir koptu
            plants.OnNewDay(false);
            Assert.False(crop.Watered);
        }

        [Fact]
        public void SuCarki_DegirmenTasiniCevirirArpaYemOlur()
        {
            var x = Make();
            var auto = new FarmAutomation(x.World, x.House);
            x.World.PlaceItem(x.Db.Items.Get("item.well"), new GridPos(2, 2));
            var canal = x.World.PlaceItem(x.Db.Items.Get("item.canal"), new GridPos(3, 2));
            x.World.PlaceItem(x.Db.Items.Get("item.water_wheel"), new GridPos(4, 2));
            var mill = x.World.PlaceItem(x.Db.Items.Get("item.millstone"), new GridPos(7, 2));
            Assert.True(auto.IsPowered(mill));
            x.House.AddItem("item.barley", 10);
            var made = auto.OnNewDay();
            Assert.Equal(6, x.House.Count("item.barley"));
            Assert.Equal(12, x.House.Count("item.feed"));
            Assert.Equal(12, made.Single(k => k.Key == "item.feed").Value);

            // Döner kompost fıçısı güç istemez ama çarkın yanındaysa kendiliğinden döner (CompostService.Powered).
            var tumbler = x.World.PlaceItem(x.Db.Items.Get("item.compost_tumbler"), new GridPos(7, 4));
            Assert.False(auto.IsPowered(tumbler));                               // "güç gerekli" uyarısı çıkmaz
            Assert.True(auto.HasPowerNearby(tumbler));
            var farTumbler = x.World.PlaceItem(x.Db.Items.Get("item.compost_tumbler"), new GridPos(14, 12));
            Assert.False(auto.HasPowerNearby(farTumbler));

            x.World.RemoveItem(canal.Id);                                        // çark durdu
            Assert.False(auto.HasPowerNearby(tumbler));
            Assert.False(auto.IsPowered(mill));
            Assert.Empty(auto.OnNewDay());
            Assert.Equal(6, x.House.Count("item.barley"));
        }

        [Fact]
        public void ToplamaSandigiVeYemOlugu_HerSabahCalisir()
        {
            var x = Make();
            var placed = Barn(x);
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            var trough = placed.Single(i => i.Def.Id == "item.feed_trough");
            var stall = placed.First(i => i.Def.Id == "item.stall");
            x.World.PlaceItem(x.Db.Items.Get("item.collection_box"), new GridPos(7, 3));
            x.World.PlaceItem(x.Db.Items.Get("item.feed_chute"), new GridPos(5, 3));
            AnimalSystem.PutStock(stall, "item.milk", 2);
            x.House.AddItem("item.feed", 20);

            var report = x.Animals.OnNewDay(Season.Spring, x.House);
            Assert.Equal(2, x.House.Count("item.milk"));
            Assert.Equal(0, stall.StockTotal);
            Assert.Equal(2, report.Collected);
            Assert.Equal(12, report.FeedAdded);                                  // 3 inek x 2,5 x 1,5 = 11,25 → 12
            Assert.Equal(12, trough.FeedStock, 6);
            Assert.Equal(8, x.House.Count("item.feed"));
        }

        // ---------- görevler ----------

        private static (Character npc, JobRunner runner, DutyPlanner planner) Worker(Ctx x, DutyKind duty, GridPos at)
        {
            var npc = new Character(2, "Ayşe", at, false) { Duty = duty };
            npc.SetAge(40);
            var planner = new DutyPlanner(x.World, x.Animals, x.Effects, x.Care);
            var runner = new JobRunner(npc, x.World) { Effects = x.Effects, Animals = x.Care, DutyProvider = planner.Next };
            return (npc, runner, planner);
        }

        [Fact]
        public void Gorev_HayvanciYemligiDoldururVeUrunuToplar()
        {
            var x = Make();
            var placed = Barn(x);
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            x.Animals.Released = false;
            var stall = placed.First(i => i.Def.Id == "item.stall");
            AnimalSystem.PutStock(stall, "item.milk", 3);
            x.House.AddItem("item.feed", 30);
            var (npc, runner, planner) = Worker(x, DutyKind.Animals, new GridPos(4, 4));
            Assert.True(planner.Pending(DutyKind.Animals) > 0);
            var log = new List<string>();
            for (int k = 0; k < 120; k++)
            {
                runner.Tick(0.005); x.Animals.Tick(0.005);
                var cur = runner.Queue.Current;
                string s = cur == null ? "-" : cur.Kind + ":" + cur.ItemId + ":" + cur.InteractionId + ":" + cur.AnimalId + " " + runner.LastFailure;
                if (log.Count == 0 || log[log.Count - 1] != s) log.Add(s);
            }
            Assert.True(x.House.Count("item.milk") == 3, string.Join(" | ", log));
            Assert.True(x.House.Count("item.feed") < 30, "yem konmadı");
            Assert.Equal(3, x.House.Count("item.milk"));
            Assert.True(x.Animals.Animals.Count(a => a.Friendship >= AnimalSystem.PetFriendship) >= 1, "hiç sevmedi");
        }

        [Fact]
        public void Gorev_CiftciSularVeHasatEder_CocukGorevAlmaz()
        {
            var x = Make();
            var dry = Crop(x, new GridPos(8, 8));
            var (npc, runner, _) = Worker(x, DutyKind.Farm, new GridPos(2, 8));
            for (int k = 0; k < 60 && !dry.Watered; k++) runner.Tick(0.005);
            Assert.True(dry.Watered);

            var kid = new Character(3, "Kerem", new GridPos(2, 9), false) { Duty = DutyKind.Farm };
            kid.SetAge(14);
            Assert.False(DutyPlanner.CanTakeDuty(kid));
            Assert.Null(new DutyPlanner(x.World, x.Animals, x.Effects, x.Care).Next(kid));

            // İş saati dışında görev yok.
            var night = new DutyPlanner(x.World, x.Animals, x.Effects, x.Care) { WorkHours = () => false };
            Crop(x, new GridPos(9, 9));
            Assert.Null(night.Next(npc));
        }

        [Fact]
        public void Ciftlik_OnGunGorevliVeOtlaklaHayvanlarSaglikli()
        {
            var x = Make();
            Barn(x);
            for (int i = 0; i < 3; i++) x.Animals.Buy("animal.cow", x.House, out _);
            for (int y = 6; y <= 11; y++) for (int px = 2; px <= 8; px++) x.Build.SetZone(new GridPos(px, y), ZoneType.Pasture);
            x.House.AddItem("item.feed", 200);
            double now = 0;
            x.Animals.CanGoOut = () => { double h = (now - Math.Floor(now)) * 24; return h >= 6 && h < 20; };
            var (npc, runner, planner) = Worker(x, DutyKind.Animals, new GridPos(10, 10));
            planner.Now = () => now;
            planner.WorkHours = () => { double h = (now - Math.Floor(now)) * 24; return h >= 7 && h < 19; };
            const double dt = 0.005;
            long day = 0;
            while (now < 10)
            {
                runner.Tick(dt);
                x.Animals.Tick(dt);
                now += dt;
                if ((long)Math.Floor(now) != day) { day = (long)Math.Floor(now); x.Animals.OnNewDay(Season.Summer, x.House); }
            }
            Assert.Equal(3, x.Animals.Animals.Count(a => !a.IsDead));
            Assert.All(x.Animals.Animals, a => Assert.True(a.HungryDays < 0.5, $"{a.Name} aç: {a.HungryDays:0.00} gün"));
            Assert.True(x.House.Count("item.milk") >= 8, $"süt {x.House.Count("item.milk")}");   // buzağılar 8. günde erişkin olur
            Assert.True(x.Animals.Animals.All(a => a.Friendship > 30f), "görevli hayvanları sevmedi");
        }

        // ---------- çapalama ----------

        [Fact]
        public void Capalama_CimiTopragaCevirir()
        {
            var x = Make();
            var player = new Character(1, "Doruk", new GridPos(1, 1), true);
            player.SetAge(40);
            var work = new FarmWork(x.World);
            var runner = new JobRunner(player, x.World) { Autonomous = false, TileWork = work };
            var tile = new GridPos(6, 6);
            Assert.True(runner.Command(Job.OnTile(tile, TileAction.Till)));
            for (int k = 0; k < 40 && !runner.IsIdle; k++) runner.Tick(0.005);
            Assert.Equal(FloorType.Dirt, x.World.Map.GetFloor(tile));
            Assert.NotNull(work.CanDo(player, tile, TileAction.Till));
            x.Build.SetZone(new GridPos(7, 7), ZoneType.Pasture);
            Assert.NotNull(work.CanDo(player, new GridPos(7, 7), TileAction.Till));
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_EyerOtlakCanVeGorevGeriGelir()
        {
            var x = Make();
            Barn(x, "template.stable_room");
            var stall = x.World.Items.Single(i => i.Def.Id == "item.horse_stall");
            var horse = x.Animals.Restore(4, x.Db.Animals.Get("animal.horse"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0, 40f, 0, "item.saddle", 88f);
            x.Build.SetZone(new GridPos(10, 10), ZoneType.Pasture);
            x.Animals.RestoreGrass(new[] { new KeyValuePair<GridPos, float>(new GridPos(10, 10), 0.25f) });
            x.Animals.Released = false;
            var npc = new Character(2, "Ayşe", new GridPos(1, 1)) { Duty = DutyKind.Animals };
            npc.SetAge(40);

            var data = SaveGame.Capture(new GameClock(), x.World, x.House, new[] { npc }, null, null, null, animals: x.Animals);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, x.Db);
            var sys = new AnimalSystem(loaded.World, x.Db);
            SaveGame.RestoreAnimals(loaded, sys, x.Db);
            Assert.Empty(loaded.Warnings);
            var h = sys.Animals.Single();
            Assert.Equal("item.saddle", h.Saddle);
            Assert.Equal(88.0, (double)h.Comfort, 3);
            Assert.Equal(0.25, (double)sys.GrassAt(new GridPos(10, 10)), 3);
            Assert.False(sys.Released);
            Assert.Equal(DutyKind.Animals, loaded.Characters.Single().Duty);
            Assert.Equal(ZoneType.Pasture, loaded.World.Map.GetZone(new GridPos(10, 10)));
        }
    }
}
