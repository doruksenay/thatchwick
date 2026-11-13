using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Kod incelemesinde (13.11.2026) bulunan hataların düzeltmeleri: her test, düzeltilen davranışı kilitler.
    public class IncelemeTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static Character Person(int id, string name, GridPos pos, Sex sex = Sex.Female, double age = 40, bool villager = false)
        {
            var c = new Character(id, name, pos) { Sex = sex, IsVillager = villager };
            c.SetAge(age);
            return c;
        }

        private static void Kill(Character c)
        {
            c.LifespanDays = c.AgeDays;
            c.Tick(0.001);
            Assert.True(c.IsDead);
        }

        // ---------- 1: karakterin durduğu karo ----------

        [Fact]
        public void KarakterinDurduguKaroyaEsyaKonmaz_KapaliKarodanDaCikilir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(8, 8));
            var c = Person(1, "Aldric", new GridPos(3, 3), Sex.Male);
            var chest = db.Items.Get("item.chest_wood");
            world.CharacterPositions = () => new[] { c.Position };

            var check = world.CanPlaceFloor(chest, new GridPos(3, 3), 0);
            Assert.False(check.Ok);
            Assert.Equal("Orada biri duruyor.", check.Reason);
            Assert.True(world.CanPlaceFloor(chest, new GridPos(4, 3), 0).Ok);

            // Eski kayıtlardaki gibi yine de kapalı karoda kalmışsa: yürüyüp çıkabilir.
            world.CharacterPositions = null;
            world.PlaceItem(chest, new GridPos(3, 3));
            var runner = new JobRunner(c, world) { Autonomous = false };
            runner.Command(Job.MoveTo(new GridPos(6, 6)));
            runner.Tick(1.0);
            Assert.Equal(new GridPos(6, 6), c.Position);
            Assert.Null(runner.LastFailure);
        }

        [Fact]
        public void Mezar_BaskasininDurduguKaroyaKonmaz()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var dead = Person(1, "Aldric", new GridPos(5, 5), Sex.Male, 100);
            var widow = Person(2, "Edith", new GridPos(5, 5));
            Kill(dead);
            world.CharacterPositions = () => new[] { widow.Position };

            var rec = new GraveService(world, db).Bury(dead, 0);
            Assert.NotEqual(0, rec.ItemId);
            Assert.NotEqual(widow.Position, rec.Position);
            Assert.True(world.Map.IsWalkable(widow.Position));
        }

        // ---------- 2: dul yeniden evlenebilir ----------

        [Fact]
        public void EsiOlen_YenidenEvlenebilir()
        {
            var db = Db();
            var pop = new Population();
            var house = new Household();
            var family = new FamilyService(pop, house, db, seed: 3);
            var husband = Person(1, "Aldric", new GridPos(1, 1), Sex.Male, 100);
            var wife = Person(2, "Edith", new GridPos(2, 1), Sex.Female, 30);
            var suitor = Person(3, "Wat", new GridPos(3, 1), Sex.Male, 30, villager: true);
            pop.Add(husband); pop.Add(wife); pop.Add(suitor);
            husband.SpouseId = 2; wife.SpouseId = 1;
            family.SetAffection(2, 3, 100);
            Assert.Equal("Zaten evli.", family.CanDo(wife, suitor, SocialAction.Propose));

            husband.LifespanDays = husband.AgeDays;
            pop.Tick(0.001);
            Assert.True(husband.IsDead);
            Assert.Null(wife.SpouseId);
            Assert.Equal(2, husband.SpouseId);          // ölenin kaydı kalır (yas, miras, soy ağacı)
            Assert.Null(family.CanDo(wife, suitor, SocialAction.Propose));
        }

        [Fact]
        public void Kayit_DulKalaninBagiYuklemedeCozulur()
        {
            var db = Db();
            var world = new WorldState(new GridMap(6, 6));
            var alive = Person(1, "Edith", new GridPos(1, 1));
            var dead = Person(2, "Aldric", new GridPos(2, 1), Sex.Male, 100);
            alive.SpouseId = 2; dead.SpouseId = 1;
            Kill(dead);
            var data = SaveGame.Capture(new GameClock(), world, new Household(), new[] { alive, dead }, null, null, null);
            var loaded = SaveGame.Load(data, db);
            Assert.Null(loaded.Characters.Single(c => c.Id == 1).SpouseId);
            Assert.Equal(1, loaded.Characters.Single(c => c.Id == 2).SpouseId);
        }

        // ---------- 3: adlı köylünün halefi ----------

        [Fact]
        public void DemirciOlunce_HalefiGelir_AletYukseltmeKilitlenmez()
        {
            var db = Db();
            var world = new WorldState(new GridMap(16, 16));
            var house = new Household(1000);
            var pop = new Population();
            var life = new LifeSystems(db, world, house, pop, new GameClock());
            life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            Assert.True(life.Villagers.HasRole(VillagerService.RoleBlacksmith));
            Assert.Empty(life.Villagers.ReplaceDeparted());             // herkes yaşıyor: halef gelmez

            var smith = life.Villagers.CharacterOf("villager.orhan");
            var state = life.Villagers.StateOf("villager.orhan");
            state.Romance = 40; state.EventsSeen.Add("olay");
            smith.LifespanDays = smith.AgeDays;
            pop.Tick(0.001);
            Assert.True(smith.IsDead);
            Assert.False(life.Villagers.HasRole(VillagerService.RoleBlacksmith));
            Assert.Equal("Köyde demirci yok.", life.Tools.CanUpgrade("axe"));

            Character arrived = null;
            life.Villagers.SuccessorArrived += (c, def) => arrived = c;
            var successors = life.Villagers.ReplaceDeparted();
            var heir = Assert.Single(successors);
            Assert.Same(heir, arrived);
            Assert.NotEqual(smith.Id, heir.Id);
            Assert.True(heir.IsVillager && !heir.IsDead);
            Assert.True(heir.LifespanDays > heir.AgeDays + 50);
            Assert.Equal(new GridPos(3, 3), heir.Position);
            Assert.True(life.Villagers.HasRole(VillagerService.RoleBlacksmith));
            Assert.NotEqual("Köyde demirci yok.", life.Tools.CanUpgrade("axe"));
            Assert.Equal(heir.Id, state.CharacterId);
            Assert.Equal(0, state.Romance);                               // yeni kişi: yakınlık sıfırdan
            Assert.Contains("olay", state.EventsSeen);                    // ödüller yeniden verilmez
            Assert.Empty(life.Villagers.ReplaceDeparted());
        }

        // ---------- 4: günlük işler yüklemede tekrarlanmaz ----------

        private static (LifeSystems life, ItemInstance crop) LifeAtMidDay(ContentDatabase db, double totalDays)
        {
            var world = new WorldState(new GridMap(12, 12));
            var clock = new GameClock();
            clock.Restore(totalDays, 1);
            var life = new LifeSystems(db, world, new Household(), new Population(), clock);
            var plants = new PlantSystem(world);
            life.Attach(plants);
            var crop = world.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(4, 4));
            PlantSystem.Water(crop);
            Assert.True(crop.Watered);
            return (life, crop);
        }

        [Fact]
        public void GunOrtasiKayit_YuklemedeGunlukIslerTekrarlanmaz()
        {
            var db = Db();
            var (first, _) = LifeAtMidDay(db, 3.4);
            first.Tick(0.001);                                           // 3. günün işleri yapıldı
            var saved = first.Export();
            Assert.Equal(3, saved.lastProcessedDay);

            var (second, crop) = LifeAtMidDay(db, 3.4);
            second.Restore(saved);
            second.Tick(0.001);
            Assert.True(crop.Watered);                                   // elle sulanan ekin kurumadı

            // Gün başı kaydı (işler henüz yapılmamış) ve eski kayıt (alan yok): işler bir kez yapılır.
            var weather = second.Weather.Today(3.4);
            bool dry = weather == WeatherType.Clear || weather == WeatherType.Snow;
            foreach (long last in new long[] { 2, -1 })
            {
                var (third, crop3) = LifeAtMidDay(db, 3.4);
                saved.lastProcessedDay = last;
                third.Restore(saved);
                third.Tick(0.001);
                if (dry) Assert.False(crop3.Watered);
                Assert.Equal(3, third.Export().lastProcessedDay);
            }
        }

        // ---------- 5: aynı yiyecek iki kez yenmez ----------

        [Fact]
        public void TekYiyecegiIkiKisiYiyemez()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household();
            house.AddItem("item.turnip", 1);
            var pot = world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(5, 5));
            var effects = new InteractionEffects(world, house, db);
            var a = Person(1, "Aldric", new GridPos(4, 5), Sex.Male);
            var b = Person(2, "Edith", new GridPos(6, 5));
            var runners = new List<JobRunner>();
            foreach (var c in new[] { a, b })
            {
                c.Needs.Set(NeedType.Hunger, 10f);
                var r = new JobRunner(c, world) { Autonomous = false, Effects = effects };
                Assert.True(r.Command(Job.UseItem(pot.Id, "eat")));
                runners.Add(r);
            }
            for (int i = 0; i < 40; i++) foreach (var r in runners) r.Tick(0.01);

            Assert.Equal(0, house.Count("item.turnip"));
            Assert.Equal(70.0, (double)(a.Needs.Get(NeedType.Hunger) + b.Needs.Get(NeedType.Hunger)), 3);   // 10 + 10 + tek şalgam (50)
            Assert.Equal(1, runners.Count(r => r.LastFailure != null));
        }

        [Fact]
        public void TokBebek_YenidenBeslenmez()
        {
            var db = Db();
            var pop = new Population();
            var house = new Household();
            house.AddItem("item.turnip", 3);
            var family = new FamilyService(pop, house, db, seed: 3);
            var mother = Person(1, "Edith", new GridPos(1, 1));
            var baby = Person(2, "Bebek", new GridPos(1, 2), Sex.Male, 2);
            pop.Add(mother); pop.Add(baby);

            baby.Needs.Set(NeedType.Hunger, 30f);
            Assert.Null(family.CanDo(mother, baby, SocialAction.Feed));
            family.Apply(mother, baby, SocialAction.Feed);
            Assert.Equal(2, house.Count("item.turnip"));
            Assert.NotNull(family.CanDo(mother, baby, SocialAction.Feed));    // 90: tok
            family.Apply(mother, baby, SocialAction.Feed);                     // ikinci bakıcı boşuna yiyecek harcamaz
            Assert.Equal(2, house.Count("item.turnip"));
        }

        // ---------- 6: ulaşılamayan kişi için bekleme ----------

        private sealed class UnreachableFriend : ISocialEffects
        {
            public Character Friend;
            public int FriendCalls;
            public Character Find(int characterId) => Friend != null && Friend.Id == characterId ? Friend : null;
            public string CanDo(Character actor, Character target, SocialAction action) => null;
            public double Duration(SocialAction action) => 0.05;
            public void Apply(Character actor, Character target, SocialAction action) { }
            public Character DependentNeedingCare(Character carer) => null;
            public Character FriendFor(Character character) { FriendCalls++; return Friend; }
        }

        [Fact]
        public void UlasilamayanArkadas_HerTurdaYenidenDenenmez()
        {
            var map = new GridMap(10, 10);
            for (int y = 0; y < 10; y++) map.SetBlocked(new GridPos(4, y), true);     // duvar: öte yana geçilmez
            var world = new WorldState(map);
            var lonely = Person(1, "Maud", new GridPos(1, 1));
            lonely.Needs.Set(NeedType.Social, 0f);
            var social = new UnreachableFriend { Friend = Person(2, "Wat", new GridPos(8, 8), Sex.Male) };
            var runner = new JobRunner(lonely, world) { Autonomous = true, Social = social };

            runner.Tick(0.001);
            Assert.InRange(social.FriendCalls, 1, 2);                                  // eskiden tur başına 64 yol araması
            for (int i = 0; i < 50; i++) runner.Tick(0.001);                           // bekleme süresi içinde
            Assert.InRange(social.FriendCalls, 1, 60);
            Assert.True(runner.IsIdle);
        }

        // ---------- 7: kış uyarısı ----------

        [Fact]
        public void KisYiyecekUyarisi_YalnizcaKisYaklasinca()
        {
            var db = Db();
            var world = new WorldState(new GridMap(8, 8));
            var house = new Household();
            house.AddItem("item.turnip", 3);
            var pop = new Population();
            pop.Add(Person(1, "Aldric", new GridPos(0, 0), Sex.Male));
            var forecast = new WinterForecast(db, house, world, pop, new GameClock().Config);

            Assert.Empty(forecast.Report(1.0).Warnings);                               // ilkbaharın ilk günleri: uyarı yok
            Assert.Contains(forecast.Report(25.0).Warnings, w => w.Contains("yiyecek"));   // kışa 5 gün
            Assert.Contains(forecast.Report(32.0).Warnings, w => w.Contains("Yiyecek"));   // kışın içinde
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_KatBolgesi_Tohum_VeSevmeSuresi()
        {
            var db = Db();
            var map = new GridMap(8, 8);
            var cellar = new GridPos(2, 2, -1);
            map.SetTile(cellar, true);
            map.SetZone(cellar, ZoneType.Storage);
            var world = new WorldState(map);
            var data = SaveGame.Capture(new GameClock(), world, new Household(), new Character[0], null, null, null, seed: 77, rngEpoch: 3);
            var loaded = SaveGame.Load(data, db);
            Assert.Equal(ZoneType.Storage, loaded.World.Map.GetZone(cellar));
            Assert.Equal(77, loaded.Seed);
            Assert.Equal(3, loaded.RngEpoch);
        }

        [Fact]
        public void Kayit_MerdiveninOnuDoluOlsaDaMerdivenKaybolmaz_SigmayanEnvantereDoner()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(100);
            var svc = new BuildService(world, house, db);
            var chest = world.PlaceItem(db.Items.Get("item.chest_wood"), new GridPos(9, 9));      // merdivenden eski eşya
            house.AddItem("item.stairs_down");
            Assert.True(svc.PlaceFromInventory("item.stairs_down", new GridPos(4, 4), 0, out var stairs).Ok);   // giriş (4,3)

            // Artık oyun içinde merdivenin önü kapatılamaz...
            Assert.False(world.MoveItem(chest.Id, new GridPos(4, 3), 0).Ok);
            // ...ama eski kayıtlarda böyle durumlar var: yüklemede merdiven düşmemeli.
            var data = SaveGame.Capture(new GameClock(), world, house, new Character[0], null, null, null);
            var saved = data.items.Single(i => i.id == chest.Id);
            saved.x = 4; saved.y = 3;
            // Aynı karoya ikinci bir sandık: sığmaz, kaybolmak yerine envantere döner.
            data.items = data.items.Concat(new[] { new ItemSave { id = 90, def = "item.chest_wood", x = 4, y = 3 } }).ToArray();

            var loaded = SaveGame.Load(data, db);
            Assert.NotNull(loaded.World.GetItem(stairs.Id));
            Assert.NotNull(loaded.World.GetItem(chest.Id));
            Assert.Single(loaded.Warnings);
            Assert.Equal(1, loaded.Household.Count("item.chest_wood"));
        }

        [Fact]
        public void KayitDosyasi_YedekVeYarimKalanYazma()
        {
            string dir = Path.Combine(Path.GetTempPath(), "thatchwick-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "save.json");
                Assert.False(SaveFiles.Exists(path));
                Assert.False(SaveFiles.TryRead(path, null, out _, out _));

                SaveFiles.WriteAtomic(path, "bir");
                SaveFiles.WriteAtomic(path, "iki");
                Assert.True(SaveFiles.TryRead(path, null, out var text, out var fallback));
                Assert.Equal("iki", text);
                Assert.False(fallback);
                Assert.Equal("bir", File.ReadAllText(path + SaveFiles.BackupSuffix));
                Assert.False(File.Exists(path + SaveFiles.TempSuffix));

                // Ana dosya bozuk: yedeğe düşer.
                File.WriteAllText(path, "bozuk");
                Assert.True(SaveFiles.TryRead(path, t => t != "bozuk", out text, out fallback));
                Assert.Equal("bir", text);
                Assert.True(fallback);

                // Taşıma yarım kaldı (ana dosya yok, geçici dosya tam): yeni kayıt kurtarılır.
                File.Delete(path);
                File.WriteAllText(path + SaveFiles.TempSuffix, "uc");
                Assert.True(SaveFiles.Exists(path));
                Assert.True(SaveFiles.TryRead(path, null, out text, out fallback));
                Assert.Equal("uc", text);
                Assert.True(fallback);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void Tohumlar_BelirleyiciVeAkisBasinaFarkli()
        {
            Assert.NotEqual(0, Seeds.NewMaster());
            Assert.Equal(Seeds.Derive(5, 1), Seeds.Derive(5, 1));
            Assert.NotEqual(Seeds.Derive(5, 1), Seeds.Derive(5, 2));
            Assert.NotEqual(Seeds.Derive(5, 1, 0), Seeds.Derive(5, 1, 1));         // her yüklemede yeni zar dizisi
            Assert.NotEqual(Seeds.Derive(5, 1), Seeds.Derive(6, 1));
            Assert.True(Seeds.Derive(-123456, 9, 4) > 0);
        }

        [Fact]
        public void Hava_OnbellekSonucuDegistirmez()
        {
            var cfg = new GameClock().Config;
            var cached = new WeatherSystem(cfg, 5);
            for (long day = 0; day < 60; day++)
            {
                var expected = new WeatherSystem(cfg, 5).WeatherOn(day);
                Assert.Equal(expected, cached.WeatherOn(day));
                Assert.Equal(expected, cached.WeatherOn(day));
                Assert.Equal(new WeatherSystem(cfg, 5).WeatherOn(day + 1), cached.Tomorrow(day));
            }
            cached.Seed = 6;                                                        // tohum değişince önbellek geçersiz
            Assert.Equal(new WeatherSystem(cfg, 6).WeatherOn(59), cached.WeatherOn(59));
        }

        // ---------- yapı modu ----------

        [Fact]
        public void GeriAl_DuvarEsyasiSatisi_VeBasarisizAdimYiginiTikamaz()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(100);
            var svc = new BuildService(world, house, db);
            world.Map.SetEdge(new Edge(5, 5, EdgeAxis.Horizontal), EdgeKind.Wall);
            house.AddItem("item.torch_wall");
            Assert.True(svc.PlaceWallFromInventory("item.torch_wall", new GridPos(5, 5), 0, out var torch).Ok);
            Assert.True(svc.SellPlaced(torch.Id).Ok);
            Assert.True(svc.Undo().Ok);                                             // eskiden hep "Eski yeri doldu" derdi
            Assert.Single(world.WallItemsOn(new Edge(5, 5, EdgeAxis.Horizontal)));
            svc.ClearUndo();

            var chest = db.Items.Get("item.chest_wood");
            var loose = world.PlaceItem(chest, new GridPos(2, 2));
            house.AddItem("item.chest_wood");
            Assert.True(svc.PlaceFromInventory("item.chest_wood", new GridPos(1, 1), 0, out _).Ok);
            Assert.True(svc.SellPlaced(loose.Id).Ok);
            world.PlaceItem(chest, new GridPos(2, 2));                              // eski yeri doldu: satış geri alınamaz
            Assert.False(svc.Undo().Ok);
            Assert.True(svc.Undo().Ok);                                             // alttaki adım hâlâ geri alınabilir
            Assert.Equal(1, house.Count("item.chest_wood"));
            Assert.False(svc.CanUndo);
        }

        [Fact]
        public void Merdiven_KattaBiriVarkenKaldirilmaz_EkinSokulur()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(100);
            var svc = new BuildService(world, house, db);
            house.AddItem("item.stairs_down");
            Assert.True(svc.PlaceFromInventory("item.stairs_down", new GridPos(4, 4), 0, out var stairs).Ok);   // iniş (4,6,-1)

            var below = new GridPos(4, 6, -1);
            world.CharacterPositions = () => new[] { below };
            Assert.False(svc.SellPlaced(stairs.Id).Ok);
            Assert.False(svc.StoreToInventory(stairs.Id).Ok);
            Assert.False(svc.Undo().Ok);                                            // yerleştirmeyi geri almak da bırakmaz
            Assert.NotNull(world.GetItem(stairs.Id));
            world.CharacterPositions = () => new[] { new GridPos(1, 1) };
            Assert.True(svc.StoreToInventory(stairs.Id).Ok);

            var crop = world.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(8, 8));
            int akce = house.Akce;
            Assert.True(svc.SellPlaced(crop.Id).Ok);                                // ekin sökülür, akçe vermez
            Assert.Null(world.GetItem(crop.Id));
            Assert.Equal(akce, house.Akce);
        }

        // ---------- aile ----------

        [Fact]
        public void Miras_YasliCocukEstenOnceGelir_TalipYalnizcaGencVeYetiskineGelir()
        {
            var head = Person(1, "Aldric", new GridPos(0, 0), Sex.Male, 130);
            var elderChild = Person(2, "Hob", new GridPos(1, 0), Sex.Male, 80);
            elderChild.BirthFatherId = 1;
            var wife = Person(3, "Edith", new GridPos(2, 0), Sex.Female, 120);
            head.SpouseId = 3; wife.SpouseId = 1;
            Assert.Equal(AgeStage.Elder, elderChild.Stage);
            Assert.Same(elderChild, Dynasty.ChooseHeir(head, new[] { head, elderChild, wife }));

            var db = Db();
            var pop = new Population();
            pop.Add(Person(1, "Çocuk", new GridPos(0, 0), Sex.Male, 15));
            pop.Add(Person(2, "Nine", new GridPos(0, 1), Sex.Female, 90));
            var pool = new SuitorPool(db, pop, seed: 9);
            Assert.Equal(0, pool.Ensure());
            pop.Add(Person(3, "Genç", new GridPos(0, 2), Sex.Male, 30));
            Assert.Equal(SuitorPool.MinCandidates, pool.Ensure());
        }

        [Fact]
        public void EvlenerekGelenKoylu_EsininNeslindenSayilir()
        {
            var db = Db();
            var pop = new Population();
            var family = new FamilyService(pop, new Household(), db, seed: 3);
            var heir = Person(1, "Hob", new GridPos(1, 1), Sex.Male, 30);
            heir.Generation = 3;
            var bride = Person(2, "Alys", new GridPos(2, 1), Sex.Female, 30, villager: true);
            pop.Add(heir); pop.Add(bride);
            family.SetAffection(1, 2, 100);
            family.Apply(heir, bride, SocialAction.Propose);
            Assert.Equal(1, bride.SpouseId);
            Assert.False(bride.IsVillager);
            Assert.Equal(3, bride.Generation);
        }
    }
}
