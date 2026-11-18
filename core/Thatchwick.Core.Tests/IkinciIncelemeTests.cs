using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // İkinci kod incelemesinde (17.11.2026) bulunan hataların düzeltmeleri.
    public class IkinciIncelemeTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_AcilamayanDosyaCorruptOlarakSaklanir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "thatchwick_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "save.json");
                Assert.Null(SaveFiles.Quarantine(path));                         // dosya yoksa kopya da yok
                File.WriteAllText(path, "{bozuk");
                string kept = SaveFiles.Quarantine(path);
                Assert.Equal(path + SaveFiles.CorruptSuffix, kept);
                Assert.Equal("{bozuk", File.ReadAllText(kept));
                SaveFiles.WriteAtomic(path, "{\"yeni\":1}");                     // sonraki iki kayıt .corrupt'ı silmez
                SaveFiles.WriteAtomic(path, "{\"yeni\":2}");
                Assert.Equal("{bozuk", File.ReadAllText(kept));
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void Kayit_DevHaritaVeGecersizKenarYonuReddedilir()
        {
            var db = Db();
            var data = new SaveData { version = SaveGame.CurrentVersion, map = new MapSave { width = 100000, height = 100000 } };
            var ex = Assert.Throws<InvalidOperationException>(() => SaveGame.Load(data, db));
            Assert.Contains("harita boyutu", ex.Message);

            data.map = new MapSave
            {
                width = 8, height = 8,
                edges = new[] { new EdgeSave { x = 2, y = 2, axis = 99, kind = (int)EdgeKind.Wall }, new EdgeSave { x = 3, y = 3, axis = (int)EdgeAxis.Vertical, kind = (int)EdgeKind.Wall } }
            };
            var loaded = SaveGame.Load(data, db);
            Assert.Equal(EdgeKind.Wall, loaded.World.Map.GetEdge(new Edge(3, 3, EdgeAxis.Vertical)));
        }

        // ---------- yapı modu ----------

        private static ItemDef StairsDown() => new ItemDef { Id = "stairs_down", DisplayName = "Merdiven", Width = 1, Height = 2, StairsDelta = -1, Price = 30 };
        private static ItemDef Chest() => new ItemDef { Id = "chest", DisplayName = "Sandık", Width = 1, Height = 1, Storage = 40, Price = 10 };

        private static (BuildService svc, WorldState w, Household h) LevelBuild()
        {
            var db = new ContentDatabase();
            db.Items.Register(StairsDown());
            db.Items.Register(Chest());
            var w = new WorldState(new GridMap(14, 14));
            var h = new Household(500);
            return (new BuildService(w, h, db), w, h);
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

        [Fact]
        public void GeriAl_EngelKalkincaYenidenDenenir_GrupKaldigiYerdenSurer()
        {
            var (svc, w, h) = LevelBuild();
            Box(w.Map, 3, 3, 10, 9);
            h.AddItem("stairs_down");
            h.AddItem("chest");
            svc.BeginGroup("Oda");
            Assert.True(svc.PlaceFromInventory("stairs_down", new GridPos(4, 4), 0, out var stairs).Ok);   // iniş (4,6,-1)
            Assert.True(svc.PlaceFromInventory("chest", new GridPos(8, 8), 0, out var chest).Ok);
            svc.EndGroup();
            Assert.Equal(1, svc.UndoCount);

            var below = new GridPos(4, 6, -1);
            w.CharacterPositions = () => new[] { below };
            var first = svc.Undo();                         // sandık geri alınır, merdiven altta biri olduğu için kalır
            Assert.False(first.Ok);
            Assert.Null(w.GetItem(chest.Id));
            Assert.NotNull(w.GetItem(stairs.Id));
            Assert.True(svc.CanUndo);                       // geçici engel: adım yığında kalır

            w.CharacterPositions = () => new[] { new GridPos(1, 1) };
            Assert.True(svc.Undo().Ok);                     // kaldığı yerden: sandığı ikinci kez geri almaya çalışmaz
            Assert.Null(w.GetItem(stairs.Id));
            Assert.False(svc.CanUndo);
            Assert.Equal(1, h.Count("stairs_down"));
            Assert.Equal(1, h.Count("chest"));
        }

        [Fact]
        public void Merdiven_TasinincaKattakiBiriInemiyorsaTasinmaz()
        {
            var (svc, w, h) = LevelBuild();
            Box(w.Map, 3, 3, 10, 9);
            h.AddItem("stairs_down");
            Assert.True(svc.PlaceFromInventory("stairs_down", new GridPos(4, 4), 0, out var stairs).Ok);   // iniş (4,6,-1)
            w.Map.SetTile(new GridPos(8, 6, -1), true);     // bodrumda ayrı, bağlantısız bir karo

            var below = new GridPos(4, 6, -1);
            w.CharacterPositions = () => new[] { below };
            var moved = svc.Move(stairs.Id, new GridPos(8, 4), 0);   // yeni iniş (8,6,-1): aşağıdaki kişi oraya yürüyemez
            Assert.False(moved.Ok);
            Assert.Equal(new GridPos(4, 4), w.GetItem(stairs.Id).Position);
            Assert.True(w.Map.HasLinkAt(below));

            w.CharacterPositions = () => new[] { new GridPos(1, 1) };   // aşağıda kimse yokken taşınır
            Assert.True(svc.Move(stairs.Id, new GridPos(8, 4), 0).Ok);
        }

        // ---------- mezar ----------

        [Fact]
        public void Mezar_TekGecidiKapatmaz()
        {
            var db = Db();
            var map = new GridMap(12, 12);
            for (int x = 0; x < 12; x++) if (x != 5) map.SetBlocked(new GridPos(x, 5), true);   // tek geçit (5,5)
            var world = new WorldState(map);
            var graves = new GraveService(world, db);
            var dead = new Character(1, "Aldric", new GridPos(5, 5)) { Sex = Sex.Male };

            var rec = graves.Bury(dead, 10);
            Assert.NotEqual(0, rec.ItemId);
            Assert.NotEqual(new GridPos(5, 5), rec.Position);
            Assert.NotNull(Pathfinder.FindPath(map, new GridPos(5, 1), new GridPos(5, 10)));
        }

        [Fact]
        public void Mezar_KimseyiCebeKapatmaz()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var graves = new GraveService(world, db);
            var center = new GridPos(6, 6);
            world.CharacterPositions = () => new[] { center };     // orada biri duruyor (doğuş karosu gibi)
            // Aynı yerde ölen sekiz kişi: mezarlar o karonun çevresini tamamen kapatmamalı.
            for (int i = 0; i < 8; i++) graves.Bury(new Character(i + 1, "K" + i, center), i);
            Assert.NotNull(Pathfinder.FindPath(world.Map, new GridPos(0, 0), center));
        }

        // ---------- otonomi ----------

        private static ItemDef Food(string id, float restore, double days) => new ItemDef
        {
            Id = id, DisplayName = id,
            Interactions = new List<InteractionDef> { new InteractionDef { Id = "eat", DisplayName = "Ye", Need = NeedType.Hunger, Restore = restore, DurationDays = days } }
        };

        [Fact]
        public void Otonomi_BirimZamandaEnCokDolduraniSecer()
        {
            var w = new WorldState(new GridMap(12, 12));
            var quick = w.PlaceItem(Food("quick", 50, 0.15), new GridPos(8, 8));
            var slow = w.PlaceItem(Food("slow", 60, 0.40), new GridPos(3, 3));   // daha çok doyurur, ama çok yavaş
            var c = new Character(1, "Maud", new GridPos(2, 2), false);
            c.Needs.Set(NeedType.Hunger, 10f);
            var runner = new JobRunner(c, w) { Autonomous = true };
            runner.Tick(0.001);
            Assert.Equal(quick.Id, runner.Queue.Current.ItemId);

            // Eksik az iken fazlası boşa gider: az ama çabuk dolduran seçilir.
            var w2 = new WorldState(new GridMap(12, 12));
            var sip = w2.PlaceItem(Food("sip", 25, 0.03), new GridPos(6, 6));
            w2.PlaceItem(Food("meal", 60, 0.15), new GridPos(6, 7));
            var c2 = new Character(2, "Hob", new GridPos(2, 2), false);
            c2.Needs.Set(NeedType.Hunger, 28f);
            var r2 = new JobRunner(c2, w2) { Autonomous = true };
            r2.Tick(0.001);
            Assert.Equal(sip.Id, r2.Queue.Current.ItemId);
        }

        [Fact]
        public void Otonomi_TabureYerineYatagaGider()
        {
            var w = new WorldState(new GridMap(12, 12));
            var bed = w.PlaceItem(new ItemDef
            {
                Id = "bed", DisplayName = "Yatak", Width = 2, Height = 1,
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 } }
            }, new GridPos(8, 8));
            w.PlaceItem(new ItemDef
            {
                Id = "chair", DisplayName = "Sandalye",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sit", DisplayName = "Otur", Need = NeedType.Energy, Restore = 12, DurationDays = 0.05 } }
            }, new GridPos(3, 3));
            var c = new Character(1, "Maud", new GridPos(2, 2), false);
            c.Needs.Set(NeedType.Energy, 10f);
            var runner = new JobRunner(c, w) { Autonomous = true };
            runner.Tick(0.001);
            Assert.Equal(bed.Id, runner.Queue.Current.ItemId);
        }

        // ---------- bildirimler ----------

        [Fact]
        public void Bildirim_AyniUyariBeklemeSuresiDolmadanTekrarlanmaz()
        {
            var watcher = new NeedWatcher();
            var raised = new List<string>();
            watcher.Raised += (l, t, id) => raised.Add(t);
            var c = new Character(1, "Maud", new GridPos(0, 0)) { Sex = Sex.Female };
            c.SetAge(40);

            c.Needs.Set(NeedType.Hunger, 25); watcher.Update(new[] { c }, day: 1.0);
            c.Needs.Set(NeedType.Hunger, 90); watcher.Update(new[] { c }, day: 1.2);
            c.Needs.Set(NeedType.Hunger, 25); watcher.Update(new[] { c }, day: 1.5);   // yarım gün sonra: sessiz
            Assert.Single(raised);
            c.Needs.Set(NeedType.Hunger, 90); watcher.Update(new[] { c }, day: 2.0);
            c.Needs.Set(NeedType.Hunger, 25); watcher.Update(new[] { c }, day: 2.2);   // bir günden sonra: yeniden
            Assert.Equal(2, raised.Count);
        }

        [Fact]
        public void Bildirim_AkisDoluncaOnceGelipGecenlerSilinir()
        {
            var log = new NotificationLog();
            log.Add(0, NoticeLevel.Danger, "Aldric öldü.");
            for (int i = 0; i < NotificationLog.Capacity + 20; i++)
                log.Add(i, NoticeLevel.Warning, $"K{i} acıktı", i + 10, transient: true);
            Assert.Equal(NotificationLog.Capacity, log.Items.Count);
            Assert.Contains(log.Items, e => e.Text == "Aldric öldü.");
        }

        [Fact]
        public void Bildirim_AraGirseDeAyniMetinBirlesir()
        {
            var log = new NotificationLog();
            log.Add(1.0, NoticeLevel.Info, "Maud acıktı", 1);
            for (int i = 0; i < 8; i++) log.Add(1.05, NoticeLevel.Info, $"Başka {i}", 100 + i);
            log.Add(1.1, NoticeLevel.Info, "Maud acıktı", 1);
            Assert.Equal(2, log.Items.First(e => e.Text == "Maud acıktı").Repeats);
        }

        // ---------- hayvan ----------

        [Fact]
        public void Hayvan_OmruKimligeGoreDegisir_TurOmrununYuzde15Icinde()
        {
            var species = new AnimalSpeciesDef { Id = "chicken", DisplayName = "Tavuk", LifespanDays = 60 };
            var spans = Enumerable.Range(1, 40).Select(id => new Animal(id, species, new GridPos(0, 0), 0).LifespanDays).ToList();
            Assert.All(spans, s => Assert.InRange(s, 60 * 0.85 - 1e-9, 60 * 1.15 + 1e-9));
            Assert.True(spans.Distinct().Count() > 30);
            Assert.True(spans.Max() - spans.Min() > 10);
            Assert.Equal(spans[5], new Animal(6, species, new GridPos(0, 0), 0).LifespanDays);   // aynı kimlik, aynı ömür
        }
    }
}
