using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Katlar: üst kat (Z=1), bodrum (Z=-1), merdiven bağlantısı, ısı, bozulma, kayıt.
    public class LevelTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

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

        private static ItemDef Stairs(string id, int delta) => new ItemDef { Id = id, DisplayName = id, Width = 1, Height = 2, StairsDelta = delta, Price = 30 };
        private static ItemDef Bed() => new ItemDef { Id = "bed", DisplayName = "Yatak", Width = 2, Height = 1, Price = 20,
            Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.2 } } };
        private static ItemDef Chest() => new ItemDef { Id = "chest", DisplayName = "Sandık", Width = 1, Height = 1, Storage = 40, Price = 10 };
        private static ItemDef Oak() => new ItemDef { Id = "oak", DisplayName = "Meşe", GrowDays = 12f };

        private static ContentDatabase Db()
        {
            var db = new ContentDatabase();
            db.Items.Register(Stairs("stairs_up", 1));
            db.Items.Register(Stairs("stairs_down", -1));
            db.Items.Register(Bed());
            db.Items.Register(Chest());
            db.Items.Register(Oak());
            return db;
        }

        // ---------- temel: konum, harita ----------

        [Fact]
        public void KonumVeKenar_KatiDikkateAlir()
        {
            Assert.Equal(new GridPos(2, 3), new GridPos(2, 3, 0));
            Assert.NotEqual(new GridPos(2, 3), new GridPos(2, 3, 1));
            Assert.Equal(new GridPos(2, 3, 1), new GridPos(2, 3).OnLevel(1));
            Assert.NotEqual(new Edge(1, 1, EdgeAxis.Horizontal), new Edge(1, 1, EdgeAxis.Horizontal, 1));
            var e = GridMap.EdgeBetween(new GridPos(2, 2, 1), new GridPos(3, 2, 1));
            Assert.Equal(1, e.Z);
            Assert.Equal(new GridPos(2, 2, 1), e.SideA);
        }

        [Fact]
        public void UstKatVeBodrum_BaslangictaYok_ZeminKatHepVar()
        {
            var m = new GridMap(8, 8);
            Assert.True(m.HasTile(new GridPos(3, 3)));
            Assert.False(m.HasTile(new GridPos(3, 3, 1)));
            Assert.False(m.HasTile(new GridPos(3, 3, -1)));
            Assert.True(m.IsBlocked(new GridPos(3, 3, 1)));
            m.SetTile(new GridPos(3, 3, 1), true);
            Assert.True(m.IsWalkable(new GridPos(3, 3, 1)));
            Assert.Equal(1, m.CountTiles(1));
            Assert.Throws<System.InvalidOperationException>(() => m.SetTile(new GridPos(3, 3), false));
            Assert.False(m.InBounds(new GridPos(3, 3, 2)));
        }

        [Fact]
        public void KatlarAyriDuvarlarTasir()
        {
            var m = new GridMap(8, 8);
            m.SetTile(new GridPos(2, 2, 1), true);
            m.SetTile(new GridPos(3, 2, 1), true);
            m.SetEdge(new Edge(3, 2, EdgeAxis.Vertical, 1), EdgeKind.Wall);
            Assert.False(m.CanStep(new GridPos(2, 2, 1), new GridPos(3, 2, 1)));
            Assert.True(m.CanStep(new GridPos(2, 2), new GridPos(3, 2)));    // zemin katta duvar yok
        }

        // ---------- oda tespiti ----------

        [Fact]
        public void UstKatOdasi_DuvarliysaIcMekan_DuvarsizsaAcik()
        {
            var m = new GridMap(10, 10);
            for (int x = 2; x <= 4; x++) for (int y = 2; y <= 4; y++) m.SetTile(new GridPos(x, y, 1), true);
            Assert.False(m.Rooms.RoomAt(new GridPos(3, 3, 1)).IsIndoor);        // duvarsız: açık teras
            Box(m, 2, 2, 4, 4, 1);
            var room = m.Rooms.RoomAt(new GridPos(3, 3, 1));
            Assert.True(room.IsIndoor);
            Assert.Equal(1, room.Level);
            Assert.Equal(9, room.Area);
            Assert.Null(m.Rooms.RoomAt(new GridPos(8, 8, 1)));                  // döşeme yok
        }

        [Fact]
        public void Bodrum_DuvarsizOlsaBileIcMekan()
        {
            var m = new GridMap(10, 10);
            m.SetTile(new GridPos(3, 3, -1), true);
            m.SetTile(new GridPos(4, 3, -1), true);
            var room = m.Rooms.RoomAt(new GridPos(3, 3, -1));
            Assert.True(room.IsIndoor);
            Assert.Equal(-1, room.Level);
            Assert.Equal(2, room.Area);
        }

        // ---------- yol bulma ----------

        [Fact]
        public void YolBulma_MerdivenBaglantisiylaKatDegistirir()
        {
            var m = new GridMap(10, 10);
            for (int x = 1; x <= 8; x++) m.SetTile(new GridPos(x, 5, 1), true);
            Assert.Null(Pathfinder.FindPath(m, new GridPos(1, 5), new GridPos(8, 5, 1)));   // bağlantı yok
            m.AddLink(new GridPos(1, 5), new GridPos(2, 5, 1));
            var path = Pathfinder.FindPath(m, new GridPos(0, 5), new GridPos(8, 5, 1));
            Assert.NotNull(path);
            Assert.Equal(new GridPos(8, 5, 1), path[path.Count - 1]);
            Assert.Contains(path, p => p.Z == 1);
            Assert.Contains(path, p => p.Z == 0);
        }

        [Fact]
        public void YolBulma_IkiYonluVeUcuKapaliysaGecmez()
        {
            var m = new GridMap(10, 10);
            m.SetTile(new GridPos(2, 5, 1), true);
            m.AddLink(new GridPos(1, 5), new GridPos(2, 5, 1));
            Assert.NotNull(Pathfinder.FindPath(m, new GridPos(2, 5, 1), new GridPos(0, 5)));
            m.SetBlocked(new GridPos(2, 5, 1), true);
            Assert.Null(Pathfinder.FindPath(m, new GridPos(0, 5), new GridPos(2, 5, 1)));
        }

        // ---------- yerleştirme kuralları ----------

        [Fact]
        public void Esya_DosemesizUstKataVeBitkiBodruma_Konmaz()
        {
            var w = new WorldState(new GridMap(10, 10));
            Assert.False(w.CanPlaceFloor(Chest(), new GridPos(3, 3, 1), 0).Ok);       // döşeme yok
            w.Map.SetTile(new GridPos(3, 3, 1), true);
            Assert.True(w.CanPlaceFloor(Chest(), new GridPos(3, 3, 1), 0).Ok);
            Assert.False(w.CanPlaceFloor(Oak(), new GridPos(3, 3, 1), 0).Ok);         // bitki yalnızca zeminde
            w.Map.SetTile(new GridPos(3, 3, -1), true);
            Assert.False(w.CanPlaceFloor(Oak(), new GridPos(3, 3, -1), 0).Ok);
        }

        [Fact]
        public void IkiKatinEsyasi_BirbirineEngelOlmaz()
        {
            var w = new WorldState(new GridMap(10, 10));
            w.Map.SetTile(new GridPos(3, 3, 1), true);
            var lower = w.PlaceItem(Chest(), new GridPos(3, 3));
            var upper = w.PlaceItem(Chest(), new GridPos(3, 3, 1));
            Assert.Same(lower, w.GetItemAt(new GridPos(3, 3)));
            Assert.Same(upper, w.GetItemAt(new GridPos(3, 3, 1)));
            w.RemoveItem(upper.Id);
            Assert.True(w.Map.IsBlocked(new GridPos(3, 3)));
            Assert.False(w.Map.IsBlocked(new GridPos(3, 3, 1)));
        }

        // ---------- yapı servisi: döşeme, kazı, merdiven ----------

        private static (BuildService svc, WorldState w, Household h, ContentDatabase db) Build(int akce = 500)
        {
            var db = Db();
            var map = new GridMap(14, 14);
            var w = new WorldState(map);
            var h = new Household(akce);
            return (new BuildService(w, h, db), w, h, db);
        }

        [Fact]
        public void UstKatDosemesi_YalnizcaDuvarliOdaninUstunde()
        {
            var (svc, w, h, _) = Build();
            Assert.False(svc.CanBuildLevelTile(new GridPos(4, 4, 1)).Ok);              // açık arazi
            Box(w.Map, 3, 3, 6, 6);
            Assert.True(svc.BuildLevelTile(new GridPos(4, 4, 1)).Ok);
            Assert.True(w.Map.HasTile(new GridPos(4, 4, 1)));
            Assert.Equal(500 - BuildService.SlabPrice, h.Akce);
            Assert.False(svc.BuildLevelTile(new GridPos(4, 4, 1)).Ok);                 // zaten var
            Assert.True(svc.Undo().Ok);
            Assert.False(w.Map.HasTile(new GridPos(4, 4, 1)));
            Assert.Equal(500, h.Akce);                                                 // geri alma tam iade eder (kaldırma yarısını verir)
        }

        [Fact]
        public void BodrumKazisi_MerdivenleBaslar_BitisigindenGenisler()
        {
            var (svc, w, h, _) = Build();
            Assert.False(svc.BuildLevelTile(new GridPos(5, 5, -1)).Ok);                // komşu bodrum yok
            h.AddItem("stairs_down");
            Box(w.Map, 3, 3, 8, 8);
            var r = svc.PlaceFromInventory("stairs_down", new GridPos(4, 4), 0, out var placed);   // giriş (4,3), iniş (4,6,-1)
            Assert.True(r.Ok, r.Reason);
            var landing = new GridPos(4, 6, -1);
            Assert.True(w.Map.HasTile(landing));
            Assert.Equal(500 - BuildService.DigPrice, h.Akce);
            Assert.True(svc.BuildLevelTile(new GridPos(5, 6, -1)).Ok);                 // bitişik kazı
            Assert.False(svc.BuildLevelTile(new GridPos(9, 9, -1)).Ok);                // uzak kazı
            Assert.True(svc.Undo().Ok);                                                // yalnızca son kazı geri
            Assert.True(svc.Undo().Ok);                                                // merdiven + iniş karosu tek adım
            Assert.False(w.Map.HasTile(landing));
            Assert.Null(w.GetItem(placed.Id));
        }

        [Fact]
        public void Merdiven_UstKatKurar_IniskaroDestekliDegilseReddedilir()
        {
            var (svc, w, h, _) = Build();
            h.AddItem("stairs_up", 2);
            // Açık arazide: iniş karosunun altı duvarsız -> destek yok.
            Assert.False(svc.CanPlaceFromInventory("stairs_up", new GridPos(4, 4), 0).Ok);
            Box(w.Map, 3, 3, 8, 8);
            var r = svc.PlaceFromInventory("stairs_up", new GridPos(4, 4), 0, out var placed);
            Assert.True(r.Ok, r.Reason);
            Assert.True(w.Map.HasTile(new GridPos(4, 6, 1)));
            var path = Pathfinder.FindPath(w.Map, new GridPos(4, 3), new GridPos(4, 6, 1));
            Assert.NotNull(path);
            Assert.Equal(1, path[path.Count - 1].Z);
        }

        [Fact]
        public void Merdiven_OnundeDuvarVarsaKonmaz_YakinindaKaldirilamaz()
        {
            var (svc, w, h, _) = Build();
            Box(w.Map, 3, 3, 8, 8);
            h.AddItem("stairs_up");
            // Giriş karosunun (4,3) ile merdiven (4,4) arasında duvar.
            w.Map.SetEdge(new Edge(4, 4, EdgeAxis.Horizontal), EdgeKind.Wall);
            Assert.False(svc.CanPlaceFromInventory("stairs_up", new GridPos(4, 4), 0).Ok);
            w.Map.SetEdge(new Edge(4, 4, EdgeAxis.Horizontal), EdgeKind.None);
            Assert.True(svc.PlaceFromInventory("stairs_up", new GridPos(4, 4), 0, out var placed).Ok);
            // Merdivenin ucundaki üst kat karosu merdiven bağlı olduğu için kaldırılamaz.
            Assert.False(svc.RemoveLevelTile(new GridPos(4, 6, 1)).Ok);
            svc.StoreToInventory(placed.Id);
            Assert.True(svc.RemoveLevelTile(new GridPos(4, 6, 1)).Ok);
            Assert.False(w.Map.HasTile(new GridPos(4, 6, 1)));
        }

        [Fact]
        public void DosemeKaldirma_UstundeEsyaVarsaOlmaz()
        {
            var (svc, w, h, _) = Build();
            Box(w.Map, 3, 3, 8, 8);
            Assert.True(svc.BuildLevelTile(new GridPos(5, 5, 1)).Ok);
            h.AddItem("chest");
            Assert.True(svc.PlaceFromInventory("chest", new GridPos(5, 5, 1), 0, out _).Ok);
            Assert.False(svc.RemoveLevelTile(new GridPos(5, 5, 1)).Ok);
        }

        [Fact]
        public void UstKatDuvari_DosemeOlmadanKurulmaz()
        {
            var (svc, w, _, _) = Build();
            Assert.False(svc.SetEdge(new Edge(5, 5, EdgeAxis.Horizontal, 1), EdgeKind.Wall).Ok);
            Box(w.Map, 3, 3, 8, 8);
            svc.BuildLevelTile(new GridPos(5, 5, 1));
            Assert.True(svc.SetEdge(new Edge(5, 5, EdgeAxis.Horizontal, 1), EdgeKind.Wall).Ok);
        }

        // ---------- karakter merdivenle kat değiştirir ----------

        [Fact]
        public void Karakter_UstKattakiYatagaMerdivenleGiderUyur()
        {
            var db = Db();
            var map = new GridMap(14, 14);
            Box(map, 3, 3, 8, 8);
            var w = new WorldState(map);
            var house = new Household(500);
            var svc = new BuildService(w, house, db);
            house.AddItem("stairs_up"); house.AddItem("bed");
            Assert.True(svc.PlaceFromInventory("stairs_up", new GridPos(4, 4), 0, out _).Ok);
            for (int x = 3; x <= 7; x++) svc.BuildLevelTile(new GridPos(x, 6, 1));
            Assert.True(svc.PlaceFromInventory("bed", new GridPos(6, 6, 1), 0, out var bed).Ok);
            var c = new Character(1, "Aldric", new GridPos(6, 4), isPlayerControlled: true);
            c.Needs.Set(NeedType.Energy, 10);
            var runner = new JobRunner(c, w);
            runner.Command(Job.UseItem(bed.Id, "sleep"));
            for (int i = 0; i < 400 && !runner.IsIdle; i++) runner.Tick(0.01);
            Assert.True(runner.IsIdle, $"meşgul: konum {c.Position} sıradaki {runner.NextTile} hata {runner.LastFailure}");
            Assert.True(c.Needs.Get(NeedType.Energy) > 60f, $"enerji {c.Needs.Get(NeedType.Energy)} konum {c.Position} hata {runner.LastFailure}");
            Assert.Equal(1, c.Position.Z);
        }

        // ---------- ısı ----------

        private static ContentDatabase HeatDb() => Db();

        [Fact]
        public void KisinBodrum_DisaridanSicak_UstKatCatidanKaybeder()
        {
            var map = new GridMap(14, 14);
            Box(map, 2, 2, 6, 6);
            for (int x = 2; x <= 6; x++) for (int y = 2; y <= 6; y++) { map.SetTile(new GridPos(x, y, 1), true); map.SetTile(new GridPos(x, y, -1), true); }
            Box(map, 2, 2, 6, 6, 1);
            var w = new WorldState(map);
            var heat = new HeatSystem(w, TimeConfig.Default, null, 35.0);     // kış ortası: dış -8°C
            for (int i = 0; i < 400; i++) heat.Tick(0.05, 35.0);
            float outdoor = heat.OutdoorTemperature;
            float basement = heat.TemperatureAt(new GridPos(4, 4, -1));
            float ground = heat.TemperatureAt(new GridPos(4, 4));
            float upper = heat.TemperatureAt(new GridPos(4, 4, 1));
            Assert.True(basement > outdoor + 8f, $"bodrum {basement} dış {outdoor}");
            Assert.True(basement < 12f);                                       // ısıtılmamış bodrum serin
            Assert.True(basement >= ground - 0.01f);
            Assert.True(upper >= outdoor - 0.01f);
        }

        [Fact]
        public void AltKattakiOcak_UstKatiIsitir()
        {
            var map = new GridMap(14, 14);
            Box(map, 2, 2, 6, 6);
            for (int x = 2; x <= 6; x++) for (int y = 2; y <= 6; y++) map.SetTile(new GridPos(x, y, 1), true);
            Box(map, 2, 2, 6, 6, 1);
            var w = new WorldState(map);
            var hearth = w.PlaceItem(new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f }, new GridPos(3, 3));
            var wood = new Household(0);
            wood.AddItem("item.firewood", 200);
            var heatOn = new HeatSystem(w, TimeConfig.Default, null, 35.0);
            for (int i = 0; i < 300; i++) { heatOn.TryStoke(hearth.Id, wood); heatOn.Tick(0.05, 35.0); }
            float upperWarm = heatOn.TemperatureAt(new GridPos(4, 4, 1));
            var cold = new WorldState(map.GetType() == typeof(GridMap) ? CloneWithoutHearth(map) : map);
            var heatOff = new HeatSystem(cold, TimeConfig.Default, null, 35.0);
            for (int i = 0; i < 300; i++) heatOff.Tick(0.05, 35.0);
            float upperCold = heatOff.TemperatureAt(new GridPos(4, 4, 1));
            Assert.True(upperWarm > upperCold + 2f, $"{upperWarm} / {upperCold}");
        }

        private static GridMap CloneWithoutHearth(GridMap src)
        {
            var map = new GridMap(14, 14);
            Box(map, 2, 2, 6, 6);
            for (int x = 2; x <= 6; x++) for (int y = 2; y <= 6; y++) map.SetTile(new GridPos(x, y, 1), true);
            Box(map, 2, 2, 6, 6, 1);
            return map;
        }

        [Fact]
        public void EskiSicaklikKaydi_ZeminKataYuklenir()
        {
            var map = new GridMap(6, 6);
            var w = new WorldState(map);
            var heat = new HeatSystem(w, TimeConfig.Default, null, 0);
            var old = new float[36];
            for (int i = 0; i < old.Length; i++) old[i] = 7f;
            heat.RestoreTemperatures(old);
            Assert.Equal(7f, heat.ExportTemperatures()[36 + 8]);   // zemin kat dilimi
        }

        // ---------- bozulma: bodrum mahzeni ----------

        [Fact]
        public void BodrumdakiDepo_BozulmayiYavaslatir()
        {
            var db = Db();
            var map = new GridMap(10, 10);
            var w = new WorldState(map);
            map.SetTile(new GridPos(4, 4, -1), true);
            var spoil = new SpoilageService(db, new Household(0), w);
            Assert.Equal(1.0, spoil.RateMultiplier(), 6);
            w.PlaceItem(Chest(), new GridPos(2, 2));                       // zemin kat sandığı etkilemez
            Assert.Equal(1.0, spoil.RateMultiplier(), 6);
            w.PlaceItem(Chest(), new GridPos(4, 4, -1));
            Assert.Equal(spoil.BasementFactor, spoil.RateMultiplier(), 6);
            spoil.TemperatureAt = _ => 20f;                                // ısınmış bodrum serin değil
            Assert.Equal(1.0, spoil.RateMultiplier(), 6);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_KatlariEsyaVeKarakteriKorur()
        {
            var db = Db();
            var map = new GridMap(14, 14);
            Box(map, 3, 3, 8, 8);
            var w = new WorldState(map);
            var house = new Household(500);
            var svc = new BuildService(w, house, db);
            house.AddItem("stairs_up"); house.AddItem("stairs_down"); house.AddItem("chest");
            Assert.True(svc.PlaceFromInventory("stairs_up", new GridPos(4, 4), 0, out _).Ok);
            Assert.True(svc.PlaceFromInventory("stairs_down", new GridPos(7, 4), 0, out _).Ok);
            Assert.True(svc.BuildLevelTile(new GridPos(5, 6, 1)).Ok);
            Assert.True(svc.PlaceFromInventory("chest", new GridPos(5, 6, 1), 0, out _).Ok);
            svc.SetEdge(new Edge(5, 6, EdgeAxis.Horizontal, 1), EdgeKind.Wall);
            var c = new Character(1, "Aldric", new GridPos(4, 6, 1), isPlayerControlled: true);
            var heat = new HeatSystem(w, TimeConfig.Default, null, 10.0);
            var data = SaveGame.Capture(new GameClock(), w, house, new List<Character> { c }, new Thatchwick.Core.Progress.AccountProgress(), heat, null);
            var back = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(back, db);
            var map2 = loaded.World.Map;
            Assert.True(map2.HasTile(new GridPos(5, 6, 1)));
            Assert.True(map2.HasTile(new GridPos(7, 6, -1)));
            Assert.Equal(EdgeKind.Wall, map2.GetEdge(new Edge(5, 6, EdgeAxis.Horizontal, 1)));
            Assert.NotNull(loaded.World.GetItemAt(new GridPos(5, 6, 1)));
            Assert.Equal(1, loaded.Characters[0].Position.Z);
            // Merdiven bağlantıları yüklemede yeniden kurulur.
            Assert.NotNull(Pathfinder.FindPath(map2, new GridPos(4, 3), new GridPos(4, 6, 1)));
            Assert.NotNull(Pathfinder.FindPath(map2, new GridPos(7, 3), new GridPos(7, 6, -1)));
        }
    }
}
