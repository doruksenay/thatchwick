using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Kiler/ambar/buzluk yerleşimi ve yollar (17.11.2026).
    public class DepoVeYolTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static (WorldState w, Household h, BuildService svc, ContentDatabase db) Setup(int size = 24)
        {
            var db = Db();
            var w = new WorldState(new GridMap(size, size));
            var h = new Household(5000);
            return (w, h, new BuildService(w, h, db), db);
        }

        private static Character Adult(int id = 1, GridPos? at = null)
        {
            var c = new Character(id, "Doruk", at ?? new GridPos(0, 0), true);
            c.SetAge(40);
            return c;
        }

        // ---------- kiler ve buzluk ----------

        [Fact]
        public void Yerlesim_CabukBozulanEnSogukYere_SigmayanAcikta()
        {
            var (w, h, _, db) = Setup();
            var spoil = new SpoilageService(db, h, w) { TemperatureAt = p => 18f };    // ılık ev
            var box = w.PlaceItem(db.Items.Get("item.ice_box"), new GridPos(2, 2));
            w.PlaceItem(db.Items.Get("item.root_cellar"), new GridPos(5, 5));
            h.AddItem("item.ice", 1);
            var fx = new InteractionEffects(w, h, db);
            fx.Apply(Adult(), box, box.Def.Interactions.Single(i => i.Id == "add_ice"));   // buzlu
            h.AddItem("item.milk", 10);       // 4 gün
            h.AddItem("item.meat", 25);       // 4 gün
            h.AddItem("item.onion", 70);      // 40 gün

            var plan = spoil.Plan();
            Assert.Equal(2, plan.Slots.Count);
            Assert.True(plan.Slots[0].Iced);
            Assert.Equal(SpoilageService.IceRate, plan.Slots[0].Rate, 6);
            // Buzluk 40: et (25) ve süt (10), soğanın 5'i; soğanın 60'ı mahzende, kalan 5'i açıkta.
            Assert.Equal(10, plan.Placed["item.milk"].Single(k => k.Key.Item == box).Value);
            Assert.Equal(25, plan.Placed["item.meat"].Single(k => k.Key.Item == box).Value);
            Assert.Equal(5, plan.Placed["item.onion"].Single(k => k.Key.Item == box).Value);
            Assert.Equal(5, plan.Open["item.onion"]);
            Assert.False(plan.Open.ContainsKey("item.milk"));
            Assert.True(plan.RateOf("item.milk") < plan.RateOf("item.onion"));
        }

        [Fact]
        public void Buzluk_SutuUzunSureSaklar_BuzErir()
        {
            var (w, h, _, db) = Setup();
            float temp = 18f;
            var spoil = new SpoilageService(db, h, w) { TemperatureAt = p => temp };
            var box = w.PlaceItem(db.Items.Get("item.ice_box"), new GridPos(2, 2));
            var fx = new InteractionEffects(w, h, db);
            var cook = Adult();
            var addIce = box.Def.Interactions.Single(i => i.Id == "add_ice");
            Assert.Equal("Buz yok.", fx.CanStart(cook, box, addIce));
            h.AddItem("item.ice", 3);
            for (int i = 0; i < 3; i++) fx.Apply(cook, box, addIce);
            Assert.Equal(12.0, box.FuelDays, 6);

            h.AddItem("item.milk", 4);
            for (int i = 0; i < 60; i++) spoil.Tick(0.1);          // 6 gün: buzsuz 4 günde bozulurdu
            Assert.Equal(4, h.Count("item.milk"));
            Assert.True(box.FuelDays < 12.0 && box.FuelDays > 0);   // 18°C: günde 1,8 erir

            // Dondurucu soğukta buz erimez.
            temp = -5f;
            double before = box.FuelDays;
            spoil.Tick(1.0);
            Assert.Equal(before, box.FuelDays, 6);
            Assert.Equal(0.0, SpoilageService.MeltPerDay(-1f));
        }

        [Fact]
        public void BuzKesmek_DonIster()
        {
            var (w, h, _, db) = Setup();
            float temp = 5f;
            var fx = new InteractionEffects(w, h, db) { TemperatureAt = p => temp };
            var well = w.PlaceItem(db.Items.Get("item.well"), new GridPos(3, 3));
            var cut = well.Def.Interactions.Single(i => i.Id == "cut_ice");
            var c = Adult();
            Assert.NotNull(fx.CanStart(c, well, cut));
            temp = -3f;
            Assert.Null(fx.CanStart(c, well, cut));
            fx.Apply(c, well, cut);
            Assert.Equal(2, h.Count("item.ice"));
        }

        [Fact]
        public void Sicaklik_OcakYanindakiKilerKotuBodrumIyi()
        {
            Assert.Equal(1.4, SpoilageService.TemperatureFactor(25f), 6);
            Assert.Equal(0.75, SpoilageService.TemperatureFactor(12f), 6);
            Assert.Equal(0.5, SpoilageService.TemperatureFactor(6f), 6);
            var (w, h, _, db) = Setup();
            w.Map.SetTile(new GridPos(4, 4, -1), true);
            var spoil = new SpoilageService(db, h, w) { TemperatureAt = p => p.Z < 0 ? 9f : 26f };
            var warm = w.PlaceItem(db.Items.Get("item.pantry_shelf"), new GridPos(2, 2));
            var cellar = w.PlaceItem(db.Items.Get("item.chest_wood"), new GridPos(4, 4, -1));
            h.AddItem("item.cabbage", 50);
            var plan = spoil.Plan();
            Assert.Same(cellar, plan.Slots[0].Item);                                      // bodrum önce dolar
            Assert.Equal(Math.Min(0.75, spoil.BasementFactor), plan.Slots[0].Rate, 6);
            Assert.Equal(1.4, plan.Slots.Single(s => s.Item == warm).Rate, 6);
            Assert.Equal(40, plan.Placed["item.cabbage"].Single(k => k.Key.Item == cellar).Value);
        }

        [Fact]
        public void Ambar_IcerikVeKatalog()
        {
            var db = Db();
            var granary = db.Items.Get("item.granary");
            Assert.True(granary.Storage >= 100);
            Assert.True(db.Items.Get("item.ice_house").IsIceBox);
            Assert.True(db.Items.Get("item.ice").HasTag("ice"));
            Assert.Contains("item.ice_box", ItemCatalog.InCategory(db, "storage").Select(d => d.Id));
        }

        // ---------- yollar ----------

        [Fact]
        public void Yol_HizlandirirKaldirimPatikadanHizli()
        {
            var map = new GridMap(30, 5);
            Assert.Equal(1f, map.WalkSpeedAt(new GridPos(1, 1)));
            for (int x = 0; x < 30; x++) map.SetRoad(new GridPos(x, 1), RoadType.Paved);
            for (int x = 0; x < 30; x++) map.SetRoad(new GridPos(x, 3), RoadType.Path);
            Assert.Equal((double)(2f), (double)(map.WalkSpeedAt(new GridPos(5, 1))), 3);
            Assert.Equal((double)(1.6f), (double)(map.WalkSpeedAt(new GridPos(5, 3))), 3);

            var w = new WorldState(map);
            var bare = new WorldState(new GridMap(30, 5));
            double TimeTo(int y, WorldState world = null)
            {
                var c = new Character(1, "A", new GridPos(0, y), true);
                c.SetAge(40);
                var r = new JobRunner(c, world ?? w) { Autonomous = false };
                r.Command(Job.MoveTo(new GridPos(20, y)));
                double t = 0;
                while (!r.IsIdle && t < 1) { r.Tick(0.0005); t += 0.0005; }
                return t;
            }
            double paved = TimeTo(1), path = TimeTo(3), grass = TimeTo(3, bare);
            Assert.True(paved < path && path < grass, $"{paved} {path} {grass}");
            Assert.InRange(grass / paved, 1.8, 2.2);
        }

        [Fact]
        public void YolBulucu_YoluTercihEder()
        {
            var map = new GridMap(20, 8);
            for (int x = 0; x < 20; x++) map.SetRoad(new GridPos(x, 5), RoadType.Paved);
            var path = Pathfinder.FindPath(map, new GridPos(0, 3), new GridPos(19, 3));
            Assert.NotNull(path);
            Assert.True(path.Count(p => map.GetRoad(p) != RoadType.None) >= 10, "yola sapmadı");
            var plain = Pathfinder.FindPath(new GridMap(20, 8), new GridPos(0, 3), new GridPos(19, 3));
            Assert.Equal(20, plain.Count);                                          // yolsuz harita eskisi gibi düz gider
        }

        [Fact]
        public void Patika_SikGecilenCimdeOlusurKullanilmazsaCimlenir()
        {
            var w = new WorldState(new GridMap(10, 10));
            var map = w.Map;
            var t = new GridPos(4, 4);
            for (int i = 0; i < 13; i++) map.Trample(t, 3f);                       // 39 < 40
            Assert.Equal(RoadType.None, map.GetRoad(t));
            map.Trample(t, 3f);
            Assert.Equal(RoadType.Path, map.GetRoad(t));
            Assert.True(map.IsNaturalRoad(t));
            Assert.True(map.WalkSpeedAt(t) > 1.5f);
            for (int i = 0; i < 20; i++) map.Trample(t, 3f);                       // belirginleşir
            Assert.Equal((double)(1f), (double)(map.GetRoadCondition(t)), 3);
            for (int d = 0; d < 25; d++) map.OnNewDayRoads();
            Assert.Equal(RoadType.None, map.GetRoad(t));
            // Kapalı oda içi ve ekili toprak patikaya dönmez.
            var dirt = new GridPos(1, 1);
            map.SetFloor(dirt, FloorType.Dirt);
            for (int i = 0; i < 30; i++) map.Trample(dirt, 3f);
            Assert.Equal(RoadType.None, map.GetRoad(dirt));
        }

        [Fact]
        public void Yol_AsinirOnarilir()
        {
            var (w, h, svc, db) = Setup();
            var t = new GridPos(6, 6);
            int start = h.Akce;
            Assert.True(svc.PaintRoad(t, RoadType.Gravel).Ok);
            Assert.Equal(start - BuildService.RoadPrice(RoadType.Gravel), h.Akce);
            for (int i = 0; i < 100; i++) w.Map.Trample(t, 3f);                    // 300/600 → yarı
            Assert.Equal(1, GridMap.ConditionBand(w.Map.GetRoadCondition(t)));
            Assert.True(w.Map.WalkSpeedAt(t) < GridMap.GravelSpeed);

            var work = new FarmWork(w) { House = h };
            var c = Adult(1, new GridPos(5, 6));
            Assert.Contains("Taş", work.CanDo(c, t, TileAction.Repair));
            h.AddItem("item.stone", 1);
            Assert.Null(work.CanDo(c, t, TileAction.Repair));
            var runner = new JobRunner(c, w) { Autonomous = false, TileWork = work };
            runner.Command(Job.OnTile(t, TileAction.Repair));
            for (int k = 0; k < 30 && !runner.IsIdle; k++) runner.Tick(0.005);
            Assert.Equal((double)(1f), (double)(w.Map.GetRoadCondition(t)), 3);
            Assert.Equal(0, h.Count("item.stone"));

            // Kaldırım kaldırılınca yarısı iade, geri al.
            int before = h.Akce;
            Assert.True(svc.PaintRoad(t, RoadType.None).Ok);
            Assert.Equal(RoadType.None, w.Map.GetRoad(t));
            Assert.True(svc.Undo().Ok);
            Assert.Equal(RoadType.Gravel, w.Map.GetRoad(t));
            Assert.Equal(before, h.Akce);
        }

        [Fact]
        public void Yol_KapaliOdayaVeEsyaAltinaDosenmez()
        {
            var (w, h, svc, db) = Setup();
            Assert.True(svc.SetEdgeLine(2, 2, 5, 2, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(svc.SetEdgeLine(2, 5, 5, 5, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(svc.SetEdgeLine(2, 2, 2, 5, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.True(svc.SetEdgeLine(5, 2, 5, 5, 0, EdgeKind.Wall, null, out _).Ok);
            Assert.False(svc.PaintRoad(new GridPos(3, 3), RoadType.Path).Ok);
            w.PlaceItem(db.Items.Get("item.well"), new GridPos(10, 10));
            Assert.False(svc.PaintRoad(new GridPos(10, 10), RoadType.Path).Ok);
            Assert.False(svc.PaintRoad(new GridPos(12, 12, 1), RoadType.Path).Ok);
        }

        [Fact]
        public void Yol_HayvanlarBarinaktanUzaktakiOtlagaYoldanGider()
        {
            var (w, h, svc, db) = Setup();
            var tpl = new RoomTemplateService(svc, w, h, db);
            Assert.True(tpl.Build("template.barn_room", new GridPos(2, 2), out _).Ok);
            var animals = new AnimalSystem(w, db);
            for (int i = 0; i < 3; i++) animals.Buy("animal.cow", h, out _);
            // Otlak kapıdan 6 karo uzakta; arası çim (yol yokken gidemezler).
            for (int y = 12; y <= 16; y++) for (int x = 2; x <= 8; x++) svc.SetZone(new GridPos(x, y), ZoneType.Pasture);
            for (int k = 0; k < 150; k++) animals.Tick(0.01);
            Assert.Empty(animals.ExportGrass());
            for (int y = 6; y <= 11; y++) Assert.True(svc.PaintRoad(new GridPos(3, y), RoadType.Path).Ok);
            bool grazed = false;
            for (int k = 0; k < 400 && !grazed; k++) { animals.Tick(0.01); grazed = animals.ExportGrass().Count > 0; }
            Assert.True(grazed, "inekler yoldan otlağa gitmedi");
            Assert.True(w.Map.GetRoadCondition(new GridPos(3, 8)) < 1f, "inekler yolu aşındırmadı");
        }

        [Fact]
        public void Kayit_YollarPatikalarVeIzlerGeriGelir()
        {
            var (w, h, svc, db) = Setup();
            svc.PaintRoad(new GridPos(1, 1), RoadType.Paved);
            w.Map.Trample(new GridPos(1, 1), 300f);
            for (int i = 0; i < 14; i++) w.Map.Trample(new GridPos(5, 5), 3f);
            w.Map.Trample(new GridPos(8, 8), 10f);
            float cond = w.Map.GetRoadCondition(new GridPos(1, 1));

            var data = SaveGame.Capture(new GameClock(), w, h, new Character[0], null, null, null);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var map = SaveGame.Load(data, db).World.Map;
            Assert.Equal(RoadType.Paved, map.GetRoad(new GridPos(1, 1)));
            Assert.Equal((double)(cond), (double)(map.GetRoadCondition(new GridPos(1, 1))), 4);
            Assert.True(map.IsNaturalRoad(new GridPos(5, 5)));
            Assert.Equal((double)(10f), (double)(map.GetTrample(new GridPos(8, 8))), 3);
            Assert.True(map.HasRoads);
        }
    }
}
