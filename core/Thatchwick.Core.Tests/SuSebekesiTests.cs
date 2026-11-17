using System.Collections.Generic;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Su şebekesi (17.11.2026): göletten ark ve boruyla su çekmek, tesisat, sarnıç, su terazisi, su gücüyle düzenekler.
    public class SuSebekesiTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static ContentDatabase Db()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            return db;
        }

        private static GridMap OwnedMap(int size = 20)
        {
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            return map;
        }

        private static void Pond(GridMap map, int x0, int y0, int w, int h)
        {
            for (int y = y0; y < y0 + h; y++) for (int x = x0; x < x0 + w; x++) map.SetFloor(new GridPos(x, y), FloorType.Water);
        }

        private static void Line(GridMap map, ConduitType t, int x0, int y0, int x1, int y1)
        {
            for (int y = System.Math.Min(y0, y1); y <= System.Math.Max(y0, y1); y++)
                for (int x = System.Math.Min(x0, x1); x <= System.Math.Max(x0, x1); x++) map.SetConduit(new GridPos(x, y), t);
        }

        private static ItemDef Fixture(string id, int use) => new ItemDef { Id = id, DisplayName = id, WaterUse = use };

        private static Character Adult(GridPos at)
        {
            var c = new Character(1, "Wat", at);
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
        }

        // ---------- ark: göletten su çekmek ----------

        [Fact]
        public void Ark_GolettenSuCekerVeYanindakiEkiniSular()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 1, 4, 2, 3);
            Line(map, ConduitType.Ditch, 3, 5, 10, 5);
            Line(map, ConduitType.Ditch, 10, 6, 10, 9);       // sağa, sonra aşağı
            var world = new WorldState(map);
            var crop = db.Items.Get("item.crop_turnip");
            map.SetFloor(new GridPos(11, 8), FloorType.Dirt);
            map.SetFloor(new GridPos(14, 8), FloorType.Dirt);
            var near = world.PlaceItem(crop, new GridPos(11, 8));
            var far = world.PlaceItem(crop, new GridPos(14, 8));

            var report = WaterNetwork.Analyze(world);
            Assert.True(report.IsWet(new GridPos(10, 9)));
            new PlantSystem(world).OnNewDay(rained: false);
            Assert.True(near.Watered);
            Assert.False(far.Watered);
        }

        [Fact]
        public void Ark_KaynaksizKuru_KapaliKapakKeser()
        {
            var db = Db();
            var map = OwnedMap();
            Line(map, ConduitType.Ditch, 5, 5, 12, 5);
            var world = new WorldState(map);
            Assert.Empty(WaterNetwork.Analyze(world).WetConduits);   // kaynak yok

            Pond(map, 3, 5, 2, 1);
            var gate = world.PlaceItem(db.Items.Get("item.water_gate"), new GridPos(8, 5));   // arkın üstüne kapak
            Assert.True(WaterNetwork.Analyze(world).IsWet(new GridPos(12, 5)));
            gate.Toggled = true;
            var r = WaterNetwork.Analyze(world);
            Assert.True(r.IsWet(new GridPos(7, 5)));
            Assert.False(r.IsWet(new GridPos(8, 5)));
            Assert.False(r.IsWet(new GridPos(12, 5)));
        }

        [Fact]
        public void Ark_KisinDonar_BoruDonmaz()
        {
            var map = OwnedMap();
            Pond(map, 1, 1, 2, 2);
            Line(map, ConduitType.Ditch, 3, 1, 8, 1);
            Line(map, ConduitType.WoodPipe, 3, 2, 8, 2);
            var world = new WorldState(map);
            map.WaterFrozen = true;
            var r = WaterNetwork.Analyze(world);
            Assert.False(r.IsWet(new GridPos(8, 1)));
            Assert.True(r.IsWet(new GridPos(8, 2)));              // buzun altından alan gömülü boru
        }

        [Fact]
        public void Cark_ArklaDoner_BoruylaDonmez()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 1, 1, 1, 1);
            Line(map, ConduitType.WoodPipe, 2, 1, 6, 1);
            Line(map, ConduitType.Ditch, 1, 2, 1, 8);
            var world = new WorldState(map);
            var wheel = db.Items.Get("item.water_wheel");
            var byPipe = world.PlaceItem(wheel, new GridPos(7, 0));     // borunun ucuna değer
            var byDitch = world.PlaceItem(wheel, new GridPos(2, 7));    // arka değer
            var net = WaterNetwork.Connected(world);
            Assert.DoesNotContain(byPipe.Id, net);
            Assert.Contains(byDitch.Id, net);
        }

        // ---------- yapı kuralları ----------

        [Fact]
        public void Doseme_BoruEveGirer_ArkGirmez_OlukYoldanGecer()
        {
            var db = Db();
            var map = OwnedMap();
            Box(map, 4, 4, 7, 7);
            var world = new WorldState(map);
            var house = new Household(500);
            var svc = new BuildService(world, house, db);
            Assert.True(map.Rooms.RoomAt(new GridPos(5, 5)).IsIndoor);
            Assert.False(svc.PaintConduit(new GridPos(5, 5), ConduitType.Ditch).Ok);
            Assert.True(svc.PaintConduit(new GridPos(5, 5), ConduitType.ClayPipe).Ok);

            svc.PaintRoad(new GridPos(12, 12), RoadType.Gravel);
            Assert.False(svc.PaintConduit(new GridPos(12, 12), ConduitType.StoneChannel).Ok);
            Assert.True(svc.PaintConduit(new GridPos(12, 12), ConduitType.Flume).Ok);

            map.SetFloor(new GridPos(15, 15), FloorType.Water);
            Assert.False(svc.PaintConduit(new GridPos(15, 15), ConduitType.Ditch).Ok);
            Assert.False(svc.PaintConduit(new GridPos(1, 1, 1), ConduitType.WoodPipe).Ok);
        }

        [Fact]
        public void Doseme_BoruEnvanterdenHarcanir_YoksaAkce_GeriAlinir()
        {
            var db = Db();
            var world = new WorldState(OwnedMap());
            var house = new Household(100);
            house.AddItem("item.pipe_wood");
            var svc = new BuildService(world, house, db);
            Assert.True(svc.PaintConduit(new GridPos(2, 2), ConduitType.WoodPipe).Ok);
            Assert.Equal(0, house.Count("item.pipe_wood"));
            Assert.Equal(100, house.Akce);
            Assert.True(svc.PaintConduit(new GridPos(3, 2), ConduitType.WoodPipe).Ok);
            Assert.Equal(100 - Conduits.Price(ConduitType.WoodPipe), house.Akce);

            Assert.True(svc.PaintConduit(new GridPos(2, 2), ConduitType.None).Ok);    // söküp yeniden kullanılır
            Assert.Equal(1, house.Count("item.pipe_wood"));
            Assert.True(svc.Undo().Ok);
            Assert.Equal(ConduitType.WoodPipe, world.Map.GetConduit(new GridPos(2, 2)));
            Assert.Equal(0, house.Count("item.pipe_wood"));
        }

        [Fact]
        public void Golet_SuYolununUstuneKazilmaz()
        {
            var db = Db();
            var world = new WorldState(OwnedMap());
            var svc = new BuildService(world, new Household(500), db);
            world.Map.SetConduit(new GridPos(3, 3), ConduitType.Ditch);
            Assert.False(svc.PaintFloor(new GridPos(3, 3), FloorType.Water).Ok);
        }

        [Fact]
        public void Kayit_SuYollariKaydaGirer()
        {
            var db = Db();
            var world = new WorldState(OwnedMap());
            var h = new Household(0);
            world.Map.SetConduit(new GridPos(2, 2), ConduitType.LeadPipe);
            world.Map.SetConduit(new GridPos(3, 2), ConduitType.Ditch);
            var cistern = world.PlaceItem(db.Items.Get("item.cistern"), new GridPos(8, 8));
            cistern.Fill = 17;
            var data = SaveGame.Capture(new GameClock(), world, h, new Character[0], null, null, null);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, db).World;
            Assert.Equal(ConduitType.LeadPipe, loaded.Map.GetConduit(new GridPos(2, 2)));
            Assert.Equal(ConduitType.Ditch, loaded.Map.GetConduit(new GridPos(3, 2)));
            Assert.True(loaded.Map.HasConduits);
            foreach (var it in loaded.Items) if (it.Def.IsCistern) Assert.Equal(17, it.Fill);
        }

        // ---------- tesisat ----------

        [Fact]
        public void Tesisat_BoruluKuvetteKovaTasinmaz()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 1, 1, 2, 2);
            Line(map, ConduitType.WoodPipe, 3, 2, 9, 2);
            var world = new WorldState(map);
            var house = new Household(0);
            var tub = world.PlaceItem(db.Items.Get("item.wooden_tub"), new GridPos(9, 3));    // borunun ucunun yanında
            var works = new WaterWorks(world);
            Assert.True(works.IsSupplied(tub), works.Report.WhyDry(tub));

            var c = Adult(new GridPos(12, 12));
            c.Needs.Set(NeedType.Hygiene, 10f);
            var fx = new InteractionEffects(world, house, db) { PipedWater = works.IsSupplied };
            var r = new JobRunner(c, world) { Effects = fx, WaterNetworkAccess = () => new HashSet<GridPos>(), PipedWater = works.IsSupplied };
            r.Command(Job.UseItem(tub.Id, "bath"));
            bool fetched = false;
            for (int i = 0; i < 6000 && r.Queue.Current != null; i++) { c.Tick(0.002); r.Tick(0.002); fetched |= r.IsFetchingWater; }
            Assert.False(fetched);
            Assert.Null(r.LastFailure);
            Assert.True(c.Needs.Get(NeedType.Hygiene) > 50f);
        }

        [Fact]
        public void Tesisat_KuyuBoruyaSuVermez()
        {
            var db = Db();
            var map = OwnedMap();
            var world = new WorldState(map);
            world.PlaceItem(db.Items.Get("item.well"), new GridPos(2, 2));
            Line(map, ConduitType.ClayPipe, 3, 2, 8, 2);
            var tub = world.PlaceItem(Fixture("tub", 2), new GridPos(8, 3));
            var r = WaterWorks.Solve(world);
            Assert.False(r.IsSupplied(tub));
            Assert.Contains("kovayla", r.WhyDry(tub));
            Assert.Equal(0, db.Items.Get("item.well").EffectiveWaterFlow);
            Assert.True(db.Items.Get("item.noria").EffectiveWaterFlow > db.Items.Get("item.bucket_pulley").EffectiveWaterFlow);
        }

        [Fact]
        public void Tesisat_InceBoruDarbogaz_KunkYeter()
        {
            var map = OwnedMap();
            Pond(map, 0, 0, 4, 4);                                // 16 karo: günde 12 su
            Line(map, ConduitType.WoodPipe, 4, 1, 8, 1);          // ana hat: kütük boru (4)
            Line(map, ConduitType.ClayPipe, 9, 1, 9, 8);          // dağıtım: künk
            var world = new WorldState(map);
            var def = Fixture("tub", 2);
            var a = world.PlaceItem(def, new GridPos(10, 2));
            var b = world.PlaceItem(def, new GridPos(10, 4));
            var c = world.PlaceItem(def, new GridPos(10, 6));
            var r = WaterWorks.Solve(world);
            Assert.True(r.IsSupplied(a));
            Assert.True(r.IsSupplied(b));
            Assert.False(r.IsSupplied(c));
            Assert.Contains("dar", r.WhyDry(c));

            Line(map, ConduitType.ClayPipe, 4, 1, 8, 1);          // ana hattı künkle değiştir (8)
            r = WaterWorks.Solve(world);
            Assert.True(r.IsSupplied(c));
            Assert.Equal(6, r.SuppliedDemand);
        }

        [Fact]
        public void Tesisat_KaynakYetmezseSonrakilerKurur()
        {
            var map = OwnedMap();
            Pond(map, 0, 0, 1, 1);                                // tek karo: günde 4 su
            Line(map, ConduitType.LeadPipe, 1, 0, 1, 8);
            var world = new WorldState(map);
            var def = Fixture("tub", 3);
            var a = world.PlaceItem(def, new GridPos(2, 2));
            var b = world.PlaceItem(def, new GridPos(2, 5));
            var r = WaterWorks.Solve(world);
            Assert.True(r.IsSupplied(a));
            Assert.False(r.IsSupplied(b));
            Assert.Equal(WaterGraph.BodyFlow(1), r.TotalSupply);
        }

        [Fact]
        public void UstKat_KursunKolonVeSuTerazisiIster()
        {
            var db = Db();
            var map = OwnedMap();
            map.SetTile(new GridPos(6, 6, 1), true);
            Pond(map, 1, 6, 2, 2);
            Line(map, ConduitType.LeadPipe, 3, 6, 6, 6);
            var world = new WorldState(map);
            var upstairs = world.PlaceItem(Fixture("tub", 2), new GridPos(6, 6, 1));
            var r = WaterWorks.Solve(world);
            Assert.False(r.IsSupplied(upstairs));
            Assert.Contains("su terazisi", r.WhyDry(upstairs));

            world.PlaceItem(db.Items.Get("item.water_tower"), new GridPos(4, 7));    // kurşun hatta değer
            r = WaterWorks.Solve(world);
            Assert.True(r.IsSupplied(upstairs), r.WhyDry(upstairs));

            map.SetConduit(new GridPos(6, 6), ConduitType.ClayPipe);                  // künk dikine çıkmaz
            r = WaterWorks.Solve(world);
            Assert.False(r.IsSupplied(upstairs));
        }

        [Fact]
        public void Sarnic_YagmurlaDolar_SuyuVerir_Azalir()
        {
            var db = Db();
            var map = OwnedMap();
            var world = new WorldState(map);
            var cistern = world.PlaceItem(db.Items.Get("item.cistern"), new GridPos(2, 2));
            Line(map, ConduitType.WoodPipe, 4, 2, 7, 2);
            var tub = world.PlaceItem(Fixture("tub", 2), new GridPos(7, 3));
            var works = new WaterWorks(world);
            Assert.False(works.IsSupplied(tub));

            works.OnNewDay(rained: true);
            Assert.Equal(cistern.Def.CisternRain, cistern.Fill);
            Assert.True(works.IsSupplied(tub), works.Report.WhyDry(tub));
            works.OnNewDay(rained: false);
            Assert.Equal(cistern.Def.CisternRain - 2, cistern.Fill);
        }

        [Fact]
        public void Sarnic_AgdakiArtanSuylaDolar()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 0, 0, 2, 2);
            Line(map, ConduitType.ClayPipe, 2, 0, 5, 0);
            var world = new WorldState(map);
            var cistern = world.PlaceItem(db.Items.Get("item.cistern"), new GridPos(6, 0));
            var works = new WaterWorks(world);
            works.OnNewDay(rained: false);
            Assert.Equal(WaterGraph.BodyFlow(4), cistern.Fill);       // tüketen yok: bütün su sarnıca
            for (int i = 0; i < 10; i++) works.OnNewDay(rained: false);
            Assert.Equal(cistern.Def.CisternCapacity, cistern.Fill);

            map.WaterFrozen = true;                                    // kış: boru buzun altından alır, sarnıç dolu kalır
            var tub = world.PlaceItem(Fixture("tub", 2), new GridPos(3, 1));
            Assert.True(works.IsSupplied(tub));
        }

        [Fact]
        public void AkarsuluHela_SuGelirkenDolmaz_KuruykenDolar()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 0, 0, 2, 2);
            Line(map, ConduitType.WoodPipe, 2, 1, 6, 1);
            var world = new WorldState(map);
            var house = new Household(0);
            var privy = world.PlaceItem(db.Items.Get("item.flush_privy"), new GridPos(6, 2));
            var works = new WaterWorks(world);
            var fx = new InteractionEffects(world, house, db) { PipedWater = works.IsSupplied };
            var c = Adult(new GridPos(7, 2));
            InteractionDef use = null;
            foreach (var i in privy.Def.Interactions) if (i.Id == "use") use = i;
            fx.Apply(c, privy, use);
            Assert.Equal(0, privy.Fill);

            map.SetConduit(new GridPos(4, 1), ConduitType.None);       // boru kesildi
            works.Invalidate();
            fx.Apply(c, privy, use);
            Assert.Equal(1, privy.Fill);
            map.SetConduit(new GridPos(4, 1), ConduitType.WoodPipe);
            works.OnNewDay(rained: false);                              // su gelince yıkanır
            Assert.Equal(0, privy.Fill);
            Assert.Equal(0, house.Count("item.manure"));
        }

        [Fact]
        public void Cesme_SuGelinceYikanilir_KovaBuradanDolar()
        {
            var db = Db();
            var map = OwnedMap();
            var world = new WorldState(map);
            var fountain = world.PlaceItem(db.Items.Get("item.fountain"), new GridPos(8, 8));
            Line(map, ConduitType.ClayPipe, 3, 9, 8, 9);
            var works = new WaterWorks(world);
            var fx = new InteractionEffects(world, new Household(0), db) { PipedWater = works.IsSupplied };
            var c = Adult(new GridPos(8, 7));
            InteractionDef wash = null;
            foreach (var i in fountain.Def.Interactions) if (i.Id == "wash_face") wash = i;
            Assert.Contains("Su gelmiyor", fx.CanStart(c, fountain, wash));
            Assert.Empty(works.TapAccess());

            Pond(map, 0, 8, 3, 3);
            Assert.Null(fx.CanStart(c, fountain, wash));
            Assert.Contains(new GridPos(8, 7), works.TapAccess());
            Assert.Contains(new GridPos(8, 7), works.AllWaterAccess());
        }

        [Fact]
        public void Hat_LBicimindeDoserTekAdimdaGeriAlinir()
        {
            var db = Db();
            var world = new WorldState(OwnedMap());
            var house = new Household(500);
            var svc = new BuildService(world, house, db);
            var run = BuildService.ConduitRun(new GridPos(2, 2), new GridPos(5, 4));
            Assert.Equal(6, run.Count);
            Assert.Equal(new GridPos(5, 2), run[3]);
            Assert.Equal(new GridPos(5, 4), run[5]);
            Assert.Single(BuildService.ConduitRun(new GridPos(3, 3), new GridPos(3, 3)));

            world.Map.SetFloor(new GridPos(4, 2), FloorType.Water);       // hattın ortasında su: atlanır
            int n = svc.PaintConduitRun(new GridPos(2, 2), new GridPos(5, 4), ConduitType.Ditch, out var problem);
            Assert.Equal(5, n);
            Assert.NotNull(problem);
            Assert.Equal(500 - 5 * Conduits.Price(ConduitType.Ditch), house.Akce);
            Assert.True(svc.Undo().Ok);
            Assert.False(world.Map.HasConduits);
            Assert.Equal(500, house.Akce);
        }

        [Fact]
        public void SuHizari_ArkaDegenCarklaKendiligindenBicer()
        {
            var db = Db();
            var map = OwnedMap();
            Pond(map, 0, 0, 2, 2);
            Line(map, ConduitType.Ditch, 2, 0, 8, 0);
            var world = new WorldState(map);
            var house = new Household(0);
            house.AddItem("item.log", 3);
            world.PlaceItem(db.Items.Get("item.water_wheel"), new GridPos(6, 1));
            world.PlaceItem(db.Items.Get("item.sawmill"), new GridPos(9, 2));
            var made = new FarmAutomation(world, house).OnNewDay();
            Assert.True(house.Count("item.plank") >= 3);
            Assert.True(house.Count("item.log") < 3);
            Assert.NotEmpty(made);
        }

        // ---------- içerik ----------

        [Fact]
        public void Icerik_BoruTarifleriVeSuGucuDuzenekleri()
        {
            var db = Db();
            foreach (var id in new[] { "item.pipe_wood", "item.pipe_clay", "item.pipe_lead" })
                Assert.True(db.Items.Contains(id), id);
            foreach (var t in Conduits.All)
            {
                var mat = Conduits.Material(t);
                if (mat != null) Assert.True(db.Items.Contains(mat), mat);
            }
            foreach (var id in new[] { "item.sawmill", "item.fulling_mill", "item.trip_hammer" })
            {
                var d = db.Items.Get(id);
                Assert.NotNull(d.AutoProcess);
                Assert.True(d.NeedsPower);
            }
            foreach (var id in new[] { "item.basin_ewer", "item.wooden_tub", "item.copper_tub", "item.flush_privy", "item.fountain", "item.kitchen_spout" })
                Assert.True(db.Items.Get(id).IsFixture, id);
            Assert.True(db.Items.Get("item.cistern").IsCistern);
            Assert.True(db.Items.Get("item.water_tower").WaterHead);
            var desc = ItemCatalog.Describe(db.Items.Get("item.flush_privy"));
            Assert.Contains("Tesisat", desc);
        }
    }
}
