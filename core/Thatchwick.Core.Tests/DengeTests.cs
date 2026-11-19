using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Denge ve oynama testi (belge 41): otonominin başkasının evini kullanmaması, üşüyenin ocağı yakması,
    /// tarla görevlisinin odun azalınca odun işine bakması ve denge raporu toplayıcısı.
    /// </summary>
    public class DengeTests
    {
        private static ContentDatabase Db()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            return db;
        }

        private static Character Adult(int id = 1, GridPos? at = null)
        {
            var c = new Character(id, "Wat", at ?? new GridPos(1, 1)) { MoodEnabled = true };
            c.SetAge(30);
            return c;
        }

        private static void Box(GridMap m, int x0, int y0, int x1, int y1)
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
            m.SetEdge(new Edge(x0, y0, EdgeAxis.Horizontal), EdgeKind.Door);
        }

        private static void Run(JobRunner r, double days)
        {
            for (double t = 0; t < days; t += 0.01) r.Tick(0.01);
        }

        [Fact]
        public void Otonomi_ArsaDisindakiEvinYataginaGitmez()
        {
            var db = Db();
            var map = new GridMap(24, 12);
            map.RestrictOwnership(0, 0, 9, 11);
            Box(map, 14, 3, 18, 7);                                        // komşunun evi (arsa dışı, kapalı oda)
            var world = new WorldState(map);
            var foreign = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(15, 5));
            var c = Adult(at: new GridPos(2, 2));
            c.Needs.Set(NeedType.Energy, 5f);
            Assert.False(JobRunner.MayUseForNeeds(world, c, foreign));
            var r = new JobRunner(c, world) { Autonomous = true };
            Run(r, 0.05);
            Assert.Null(r.Queue.Current);                                  // tek yatak başkasının: gitmez

            var own = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(5, 5));
            Assert.True(JobRunner.MayUseForNeeds(world, c, own));
            Run(r, 0.05);
            Assert.NotNull(r.Queue.Current);
            Assert.Equal(own.Id, r.Queue.Current.ItemId);
        }

        [Fact]
        public void Otonomi_ArsaDisiAcikHavaEsyasi_BolgesizHaritadaKullanilir()
        {
            var db = Db();
            var map = new GridMap(24, 12);
            map.RestrictOwnership(0, 0, 9, 11);
            var world = new WorldState(map);
            var bench = world.PlaceItem(db.Items.Get("item.garden_bench"), new GridPos(14, 5));   // açık havada, arsa dışı
            Assert.True(JobRunner.MayUseForNeeds(world, Adult(at: new GridPos(2, 2)), bench));
        }

        private static (WorldState w, HeatSystem h, Household house, ItemInstance hearth, JobRunner r, Character c) WarmSetup(ContentDatabase db, int wood)
        {
            var map = new GridMap(14, 14);
            Box(map, 2, 2, 6, 6);
            var w = new WorldState(map);
            var h = new HeatSystem(w, TimeConfig.Default, null, 35.0);     // kış ortası
            var house = new Household(0);
            if (wood > 0) house.AddItem("item.firewood", wood);
            var hearth = w.PlaceItem(db.Items.Get("item.hearth_stone"), new GridPos(4, 4));
            var c = Adult(at: new GridPos(3, 3));
            c.Needs.Set(NeedType.Warmth, 10f);
            var r = new JobRunner(c, w)
            {
                Autonomous = true,
                Effects = new InteractionEffects(w, house, db),
                Stoke = it => h.TryStoke(it.Id, house),
                HasFirewood = () => house.Count("item.firewood") > 0
            };
            return (w, h, house, hearth, r, c);
        }

        [Fact]
        public void Otonomi_UsuyenSonukOcagaOdunAtipIsinir()
        {
            var db = Db();
            var (w, h, house, hearth, r, c) = WarmSetup(db, 5);
            Assert.False(hearth.IsLit);
            Run(r, 0.02);
            Assert.True(r.Queue.Current != null, "iş yok: " + r.LastFailure);
            Run(r, 0.3);
            Assert.True(hearth.IsLit, "sönük: " + r.LastFailure);
            Assert.Equal(5 - JobRunner.StokeMaxLogs, house.Count("item.firewood"));   // bir günlük yakıt: iki kütük
            Assert.True(c.Needs.Get(NeedType.Warmth) > 30f, $"ısınma {c.Needs.Get(NeedType.Warmth)}");
        }

        [Fact]
        public void Otonomi_OdunYoksaSonukOcagaGitmez()
        {
            var db = Db();
            var (w, h, house, hearth, r, c) = WarmSetup(db, 0);
            Run(r, 0.1);
            Assert.False(hearth.IsLit);
            Assert.True(r.Queue.Current == null || r.Queue.Current.ItemId != hearth.Id);
        }

        [Fact]
        public void Otonomi_YananOcakYakitAzsaTakviyeEder()
        {
            var db = Db();
            var (w, h, house, hearth, r, c) = WarmSetup(db, 1);
            Assert.True(h.TryStoke(hearth.Id, house));                     // 0,5 gün yanıyor, odun kalmadı
            house.AddItem("item.firewood", 3);
            Run(r, 0.3);
            Assert.True(hearth.FuelDays >= JobRunner.StokeTargetDays - 0.35, $"yakıt {hearth.FuelDays}");
            Assert.Equal(2, house.Count("item.firewood"));                  // 0,5 + 0,5 = 1 gün: tek kütük yeter
        }

        private static (WorldState w, Household house, DutyPlanner d, Character c) DutySetup(ContentDatabase db)
        {
            var map = new GridMap(20, 20);
            map.RestrictOwnership(0, 0, 14, 19);
            var w = new WorldState(map);
            var house = new Household(0);
            var fx = new InteractionEffects(w, house, db);
            var d = new DutyPlanner(w, null, fx, null);
            var c = Adult(at: new GridPos(2, 2));
            c.Duty = DutyKind.Farm;
            return (w, house, d, c);
        }

        [Fact]
        public void TarlaGorevlisi_OdunAzsaKutukYarar()
        {
            var db = Db();
            var (w, house, d, c) = DutySetup(db);
            var block = w.PlaceItem(db.Items.Get("item.chopping_block"), new GridPos(5, 5));
            house.AddItem("item.log", 2);
            house.AddItem("item.firewood", DutyPlanner.LowFirewood);
            Assert.Null(d.Next(c));                                        // odun yeterli: iş yok
            Assert.True(house.TryTakeItem("item.firewood"));
            var job = d.Next(c);
            Assert.NotNull(job);
            Assert.Equal(block.Id, job.ItemId);
            Assert.Equal("split", job.InteractionId);
            Assert.True(d.Pending(DutyKind.Farm) >= 1);
        }

        [Fact]
        public void TarlaGorevlisi_SonbahardaKisOdunuBiriktirir()
        {
            var db = Db();
            var (w, house, d, c) = DutySetup(db);
            w.PlaceItem(db.Items.Get("item.chopping_block"), new GridPos(5, 5));
            house.AddItem("item.log", 2);
            house.AddItem("item.firewood", 10);
            var season = Season.Summer;
            d.Season = () => season;
            Assert.Null(d.Next(c));                                        // yazın 10 odun yeter
            season = Season.Autumn;
            Assert.Equal(DutyPlanner.WinterFirewood, d.FirewoodTarget());
            Assert.NotNull(d.Next(c));                                     // sonbaharda kış stoğu
            d.Season = null;
            d.Now = () => 25.0;                                            // takvimden: 25. gün sonbahar
            Assert.Equal(DutyPlanner.WinterFirewood, d.FirewoodTarget());
        }

        [Fact]
        public void TarlaGorevlisi_KutukYoksaArsadakiAgaciKeser_MeyveAgaciniKesmez()
        {
            var db = Db();
            var (w, house, d, c) = DutySetup(db);
            var plum = w.PlaceItem(db.Items.Get("item.tree_plum"), new GridPos(4, 4));
            PlantSystem.SetGrown(plum);
            var outside = w.PlaceItem(db.Items.Get("item.tree_oak"), new GridPos(17, 4));   // arsa dışı
            PlantSystem.SetGrown(outside);
            Assert.Null(d.Next(c));                                        // meyve ağacı ve arsa dışı kesilmez
            var oak = w.PlaceItem(db.Items.Get("item.tree_oak"), new GridPos(8, 8));
            PlantSystem.SetGrown(oak);
            var job = d.Next(c);
            Assert.NotNull(job);
            Assert.Equal(oak.Id, job.ItemId);
            Assert.Equal("chop", job.InteractionId);
        }

        [Fact]
        public void DengeRaporu_IlkOlumSikintiVeOzet()
        {
            var house = new Household(100);
            var a = Adult(1); var b = Adult(2);
            var rep = new BalanceReport { Scenario = "test" };
            rep.Add(0, "Spring", 1f, house, 500, 10, new[] { a, b }, c => "boşta");
            a.Needs.Set(NeedType.Hunger, 5f);
            b.GainSkillXp(SkillType.Farming, 500);
            Assert.True(house.TrySpend(30));
            rep.Add(1, "Spring", 2f, house, 300, 4, new[] { a, b });
            var (hungry, cold, total) = rep.DistressSamples();
            Assert.Equal(1, hungry);
            Assert.Equal(2, total);
            Assert.Null(rep.FirstDeath());
            string sum = rep.Summary();
            Assert.Contains("Ölüm yok", sum);
            Assert.Contains("Akçe: 100 → 70 (-30)", sum);
            Assert.Contains("Odun: 10 → 4", sum);
            string csv = rep.ToCsv();
            Assert.Equal(5, csv.Trim().Split('\n').Length);                 // başlık + 2 örnek x 2 üye
            Assert.Contains("boşta", csv);
        }

        [Fact]
        public void Gorevli_RahatlikIhtiyaciniGorevUgrunaErteler_CokDusunceErtelemez()
        {
            var db = Db();
            string First(float fun)
            {
                var world = new WorldState(new GridMap(12, 12));
                world.PlaceItem(db.Items.Get("item.garden_bench"), new GridPos(6, 6));
                var c = Adult(at: new GridPos(2, 2));
                c.Duty = DutyKind.Farm;
                c.Needs.Set(NeedType.Fun, fun);
                var r = new JobRunner(c, world) { Autonomous = true, DutyProvider = ch => Job.MoveTo(new GridPos(9, 9)) };
                r.Tick(0.001);
                return r.Queue.Current == null ? "-" : r.Queue.Current.IsDuty ? "görev" : r.Queue.Current.InteractionId;
            }
            Assert.Equal("görev", First(25f));                                   // eşiğin (30) altında ama bekleyebilir
            Assert.Equal("rest", First(JobRunner.ComfortDeferFloor - 5f));      // çok düştü: önce dinlenir
        }

        [Fact]
        public void BaslangicErzagi_IkiKisiyiIlkHasadaKadarDoyurur()
        {
            var db = Db();
            var house = new Household(100);
            StartingKit.Apply(house);
            double hunger = 0;
            foreach (var (id, count) in StartingKit.Items)
            {
                Assert.True(db.Items.Contains(id), id);
                float best = 0f;
                foreach (var item in db.Items.All)
                    foreach (var i in item.Interactions)
                        if (i.ConsumeItem == id && i.Consume.Count == 0) best = System.Math.Max(best, i.RestoreFor(NeedType.Hunger));
                hunger += best * house.Count(id);
            }
            double days = hunger / (2 * 120.0);                                  // iki yetişkin, kişi başı 120/gün
            Assert.True(days >= 6.5, $"erzak {days:0.0} gün");
            int seeds = house.Count("item.seed_turnip") + house.Count("item.seed_radish");
            Assert.True(seeds >= StartingKit.BedWidth * StartingKit.BedHeight, "bostanın tamamı ilk gün ekilebilmeli");
            Assert.True(house.Count("item.firewood") >= 12);
        }

        [Fact]
        public void BaslangicBostani_ArsadakiCimiCapalar_DoluVeArsaDisiKaroyuAtlar()
        {
            var db = Db();
            var map = new GridMap(20, 12);
            map.RestrictOwnership(0, 0, 9, 11);
            var world = new WorldState(map);
            world.PlaceItem(db.Items.Get("item.scarecrow"), new GridPos(5, 4));
            int n = StartingKit.TillBed(world, new GridPos(4, 3));              // 4..9 x 3..5, (5,4) dolu
            Assert.Equal(StartingKit.BedWidth * StartingKit.BedHeight - 1, n);
            Assert.Equal(FloorType.Dirt, map.GetFloor(new GridPos(4, 3)));
            Assert.Equal(FloorType.Grass, map.GetFloor(new GridPos(5, 4)));
            Assert.Equal(0, StartingKit.TillBed(world, new GridPos(8, 3)));     // 8,9 zaten toprak; 10-13 arsa dışı
        }

        [Fact]
        public void BaslangicBostani_HendekOrtaSirayaDoseniyor_IkiSirayiHerGunSular()
        {
            var db = Db();
            var map = new GridMap(20, 12);
            var world = new WorldState(map);
            var ditch = db.Items.Get("item.irrigation_ditch");
            int n = StartingKit.TillBed(world, new GridPos(4, 3), ditch: ditch);
            Assert.Equal(StartingKit.BedWidth * (StartingKit.BedHeight - 1), n);
            Assert.Equal(ditch.Id, world.GetItemAt(new GridPos(4, 4))?.Def.Id);
            Assert.Equal(FloorType.Dirt, map.GetFloor(new GridPos(9, 5)));
            var crop = world.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(9, 5));
            var plants = new PlantSystem(world, 1);
            plants.OnNewDay(rained: false);
            Assert.True(crop.Watered, "hendek yanındaki ekin sulanmalı");
        }
    }
}
