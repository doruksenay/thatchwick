using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Eşya kalitesi (Project belgesi 53): yığın anahtarı, harcama sırası, kayıt ve göç, kaynaklar, işleme, olgunlaşma,
    // ustalık eseri, yadigâr, pazar, sipariş, yerleştirme ve oyma süsle.
    public class KaliteTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
        private static ContentDatabase Db() => ContentFilesTests.LoadDb(out _);

        private static Character Person(int id, SkillType skill, int level, GridPos? pos = null)
        {
            var c = new Character(id, "Usta" + id, pos ?? new GridPos(5, 6)) { CanStarve = false, MoodEnabled = true };
            c.SetAge(60);
            c.Skills.RestoreXp(skill, SkillRules.XpForLevel(level));
            return c;
        }

        // ---- yığın modeli ----

        [Fact]
        public void Yigin_KaliteDurumVeKaynakAyri_ToplamAyni_HarcamaSirasi()
        {
            var h = new Household();
            h.AddItem("item.cheese", 2);
            h.AddItem("item.cheese", 1, ItemQuality.Superior);
            h.Add(new ItemKey("item.cheese", ItemQuality.Fine), 3);
            h.AddItem("item.cheese", 1, ItemCondition.Damaged);
            Assert.Equal(7, h.Count("item.cheese"));
            Assert.Equal(4, h.Count("item.cheese", ItemQuality.Fine));
            Assert.Equal(new[] { 3, 3, 1, 0, 0 }, h.QualityCounts("item.cheese"));
            Assert.Equal(ItemQuality.Superior, h.BestQuality("item.cheese"));
            Assert.Equal(1, h.DamagedCount("item.cheese"));

            Assert.True(h.TryTake("item.cheese", TakePolicy.Lowest, out var low));
            Assert.Equal(ItemQuality.Common, low.Quality);
            Assert.Equal(ItemCondition.Sound, low.Condition);              // sağlam önce, hasarlı en son
            Assert.True(h.TryTake("item.cheese", TakePolicy.Highest, out var high));
            Assert.Equal(ItemQuality.Superior, high.Quality);
            Assert.True(h.TryTake("item.cheese", TakePolicy.Lowest, out var atLeast, ItemQuality.Fine));
            Assert.Equal(ItemQuality.Fine, atLeast.Quality);
            Assert.Equal(4, h.Count("item.cheese"));

            // Pazardan alınan: kendi ürünü önce harcanır; onarılan eşya kalitesini korur.
            var b = new Household();
            b.AddItem("item.egg", 2, ItemQuality.Fine);
            b.AddItem("item.egg", 2);
            b.MarkBought("item.egg", 2);
            Assert.Equal(2, b.BoughtCount("item.egg"));
            Assert.True(b.TryTakePreferOwn("item.egg", out bool own));
            Assert.True(own);
            var c = new Household();
            c.Add(new ItemKey("item.chair_wood", ItemQuality.Superior, ItemCondition.Damaged), 1);
            Assert.True(c.RepairOne("item.chair_wood"));
            Assert.Equal(1, c.RepairedCount("item.chair_wood"));
            Assert.Equal(ItemQuality.Superior, c.BestQuality("item.chair_wood"));
            Assert.Equal(2, h.Requalify("item.cheese", ItemQuality.Common, ItemQuality.Fine, 5));   // olgunlaşma: durum korunur
            Assert.Equal(4, h.Count("item.cheese", ItemQuality.Fine));
            Assert.Equal(1, h.DamagedCount("item.cheese"));
        }

        [Fact]
        public void Sinif_YiyecekVeTekstilKaliteli_HamMaddeKalitesiz_YapilanEsya()
        {
            var db = Db();
            foreach (var id in new[] { "item.milk", "item.cheese", "item.stew_plain", "item.wool", "item.cloth", "item.leather", "item.fish_carp", "item.wine_new" })
                Assert.True(db.Items.Get(id).QualityClass == ItemQualityClass.Goods, id);
            foreach (var id in new[] { "item.stone", "item.log", "item.plank", "item.nails", "item.seed_wheat", "item.salt", "item.iron_ore" })
                Assert.True(db.Items.Get(id).QualityClass == ItemQualityClass.None, id);
            foreach (var id in new[] { "item.chair_wood", "item.chest_wood", "item.saddle" })
                Assert.True(db.Items.Get(id).QualityClass == ItemQualityClass.Crafted, id);
            Assert.Equal(ItemQualityClass.None, db.Items.Get("item.windmill").QualityClass);   // makine kalitesiz
        }

        [Fact]
        public void TakmaAd_EskiAyriKimliklerEmekli_KaliteyeDoner()
        {
            var db = Db();
            foreach (var kv in ItemQuality.Aliases)
            {
                Assert.True(db.Items.Get(kv.Key).IsRetired, kv.Key);
                Assert.True(db.Items.Contains(kv.Value.id), kv.Value.id);
                Assert.True(kv.Value.quality > 0);
            }
            Assert.Equal(("item.stew_plain", ItemQuality.Superior), ItemQuality.Resolve("item.stew_fine"));
            Assert.Equal(("item.olive", ItemQuality.Common), ItemQuality.Resolve("item.olive"));
            // Hiçbir tarif emekli kimliği üretmez.
            foreach (var d in db.Items.All)
                foreach (var i in d.Interactions)
                    foreach (var p in i.Produce)
                        Assert.False(ItemQuality.Aliases.ContainsKey(p.ItemId), $"{d.Id}.{i.Id} -> {p.ItemId}");
        }

        // ---- kayıt ----

        [Fact]
        public void Kayit_YiginlarGidipGelir_EskiKayitVeEskiKimlikGoc()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household(10);
            house.AddItem("item.cheese", 2, ItemQuality.Superior);
            house.Add(new ItemKey("item.chair_wood", ItemQuality.Masterwork, ItemCondition.Repaired, false, 7), 1);
            house.AddItem("item.egg", 3);
            house.MarkBought("item.egg", 1);
            var chair = world.PlaceItem(db.Items.Get("item.chair_wood"), new GridPos(2, 2));
            chair.Quality = ItemQuality.Fine; chair.Maker = 3; chair.Embellished = true;
            var data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(SaveGame.Capture(new GameClock(), world, house, new Character[0], null, null, null), Json), Json);
            var loaded = SaveGame.Load(data, db);
            Assert.Equal(new[] { 0, 0, 2, 0, 0 }, loaded.Household.QualityCounts("item.cheese"));
            var mw = loaded.Household.Stacks("item.chair_wood").Single().Key;
            Assert.Equal(ItemQuality.Masterwork, mw.Quality);
            Assert.Equal(ItemCondition.Repaired, mw.Condition);
            Assert.Equal(7, mw.Maker);
            Assert.Equal(1, loaded.Household.BoughtCount("item.egg"));
            var placed = loaded.World.Items.Single(i => i.Def.Id == "item.chair_wood");
            Assert.Equal(ItemQuality.Fine, placed.Quality);
            Assert.Equal(3, placed.Maker);
            Assert.True(placed.Embellished);

            // Eski kayıt: yığın yok, ayrı kalite kimliği var.
            data.household.stacks = null;
            data.household.inventory = new[] { new KeyInt { key = "item.stew_good", value = 2 }, new KeyInt { key = "item.wine_aged", value = 1 }, new KeyInt { key = "item.egg", value = 4 } };
            data.household.bought = new[] { new KeyInt { key = "item.egg", value = 1 } };
            data.household.damaged = null; data.household.repaired = null;
            var old = SaveGame.Load(data, db);
            Assert.Equal(2, old.Household.Count("item.stew_plain", ItemQuality.Fine));
            Assert.Equal(0, old.Household.Count("item.stew_good"));
            Assert.Equal(1, old.Household.Count("item.wine_new", ItemQuality.Superior));
            Assert.Equal(4, old.Household.Count("item.egg"));
            Assert.Equal(1, old.Household.BoughtCount("item.egg"));
        }

        // ---- kurallar ----

        [Fact]
        public void Kural_IslemeOrtalamaVeTavan_ToplamaTablosu_YapimTabanı()
        {
            var rng = new Random(5);
            for (int i = 0; i < 200; i++)
            {
                // 2 Has + 1 Sıradan: ortalama 1,33 -> taban Seçme; usta değilse yükselmez.
                Assert.Equal(ItemQuality.Fine, ItemQuality.Processed(new[] { 2, 2, 0 }, 0, 3, rng));
                // Sıradan sütten Has peynir olmaz (tavan en iyi girdinin bir üstü).
                Assert.InRange(ItemQuality.Processed(new[] { 0, 0 }, 9, 10, rng), 0, 1);
                Assert.Equal(ItemQuality.Common, ItemQuality.Gathered(1, rng));
                Assert.InRange(ItemQuality.Crafted(0, 10, rng), ItemQuality.Fine, ItemQuality.Superior);   // Sv 10 taban Seçme
                Assert.InRange(ItemQuality.Spread(2, rng), 1, 2);
            }
            var tenth = Enumerable.Range(0, 400).Select(_ => ItemQuality.Gathered(10, rng)).ToList();
            Assert.Contains(ItemQuality.Superior, tenth);
            Assert.InRange(tenth.Count(q => q == ItemQuality.Superior) / 400.0, 0.15, 0.35);
            Assert.Equal(1.1f, Work.PlantSystem.QualityYield(1), 3);       // adet bonusu yarıya indi
            Assert.Equal(1.2f, Work.PlantSystem.QualityYield(2), 3);
            Assert.True(ItemQuality.PriceFactor(ItemQuality.Masterwork) > ItemQuality.PriceFactor(ItemQuality.Superior));
            Assert.Equal(ItemQuality.Superior, ItemQuality.Aged(0, 9, 4));
            Assert.Equal(ItemQuality.Fine, ItemQuality.Aged(0, 5, 4));
        }

        [Fact]
        public void Bozulma_KaliteyeGoreAyriPartiler_Kayit()
        {
            var db = Db();
            var w = new WorldState(new GridMap(8, 8));
            var h = new Household();
            var spoil = new SpoilageService(db, h, w);
            h.AddItem("item.milk", 2);
            h.AddItem("item.milk", 1, ItemQuality.Superior);
            spoil.Tick(0.1);
            var saved = spoil.Export();
            Assert.Contains(saved, s => s.item == "item.milk" && s.quality == ItemQuality.Superior && s.count == 1);
            var again = new SpoilageService(db, h, w);
            again.Restore(saved);
            again.Tick(db.Items.Get("item.milk").SpoilDays + 1);
            Assert.Equal(0, h.Count("item.milk"));
        }

        [Fact]
        public void Olgunlasma_MahzendeSureyleArtar_MahzensizBekler_DedeSarabi()
        {
            var db = Db();
            var w = new WorldState(new GridMap(8, 8));
            var h = new Household();
            double today = 0;
            var aging = new AgingService(db, h, w) { Today = () => today };
            h.AddItem("item.cheese", 3);
            h.AddItem("item.wine_new", 2);
            aging.Tick(10); today += 10;
            Assert.Equal(3, h.Count("item.cheese", 0) - h.Count("item.cheese", 1));   // mahzen yok: olgunlaşmaz
            w.Map.SetTile(new GridPos(4, 4, -1), true);
            w.PlaceItem(db.Items.Get("item.chest_wood"), new GridPos(4, 4, -1));
            Assert.True(aging.HasCellar());
            float step = db.Items.Get("item.cheese").AgeStepDays;
            aging.Tick(step + 0.01); today += step + 0.01;
            Assert.Equal(3, h.Count("item.cheese", ItemQuality.Fine));
            aging.Tick(step + 0.01); today += step + 0.01;
            Assert.Equal(3, h.Count("item.cheese", ItemQuality.Superior));
            Assert.Equal(0, h.Count("item.cheese", ItemQuality.Masterwork));   // olgunlaşma Has'ta durur
            // Dede şarabı: iki kuşak mahzende kalan şarap yadigâr olur.
            for (int i = 0; i < 30; i++) { aging.Tick(4); today += 4; }
            Assert.Equal(2, h.Count("item.wine_new", ItemQuality.Heirloom));
            var saved = aging.Export();
            Assert.Contains(saved, s => s.item == "item.wine_new" && s.quality == ItemQuality.Heirloom);
        }

        // ---- kaynaklar ve işleme (InteractionEffects) ----

        private static (WorldState w, Household h, ItemInstance pot, InteractionDef stew) Kitchen(ContentDatabase db)
        {
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var pot = w.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(5, 5));
            return (w, h, pot, pot.Def.Interactions.First(i => i.Id == "cook_stew"));
        }

        private static void Veg(Household h, int q) { foreach (var id in new[] { "item.turnip", "item.cabbage", "item.leek" }) h.AddItem(id, 1, q); }

        [Fact]
        public void EnIyiMalzeme_SeciliyseEnIyisiHarcanir()
        {
            var db = Db();
            var (w, h, pot, stew) = Kitchen(db);
            var book = new QualityBook();
            Veg(h, ItemQuality.Common); Veg(h, ItemQuality.Superior);
            var fx = new InteractionEffects(w, h, db, seed: 2) { Quality = book };
            var cook = Person(1, SkillType.Cooking, 3);
            fx.Apply(cook, pot, stew);
            Assert.Equal(ItemQuality.Superior, h.BestQuality("item.turnip"));   // önce sıradan harcandı
            Veg(h, ItemQuality.Common);
            book.SetBestIngredientsAt(pot.Def, db, true);
            fx.Apply(cook, pot, stew);
            Assert.Equal(ItemQuality.Common, h.BestQuality("item.turnip"));      // has olan harcandı
        }

        [Fact]
        public void UstalikEseri_BilincliDeneme_GundeBir_YuzdeAltmis_KaydaGirer()
        {
            var db = Db();
            int made = 0, superior = 0;
            for (int seed = 1; seed <= 40; seed++)
            {
                var (w, h, pot, stew) = Kitchen(db);
                var book = new QualityBook();
                var fx = new InteractionEffects(w, h, db, seed: seed) { Quality = book, DayIndex = () => 3, Year = () => 2, Today = () => 3.5 };
                var master = Person(1, SkillType.Cooking, 10);
                book.QueueMasterworkAt(pot.Def, db, true);
                Assert.True(book.WillAttempt(master, pot, stew, 3));
                Veg(h, ItemQuality.Superior);
                fx.Apply(master, pot, stew);
                Assert.False(book.MasterworkQueuedAt(pot.Def, db));               // işaret kalktı
                Assert.False(book.AttemptBlocker(master, stew, 3).IsEmpty);       // günde bir
                int q = h.BestQuality("item.stew_plain");
                if (q == ItemQuality.Masterwork)
                {
                    made++;
                    var key = h.Stacks("item.stew_plain").Single().Key;
                    var entry = book.Find(key.Maker);
                    Assert.NotNull(entry);
                    Assert.Equal(master.Name, entry.MakerName);
                    Assert.Equal(2, entry.Year);
                }
                else { Assert.Equal(ItemQuality.Superior, q); superior++; }
            }
            Assert.InRange(made, 16, 32);                                         // ~%60
            Assert.True(superior > 0);

            // Usta değilse ya da girdiler Has değilse denenmez.
            var (w2, h2, pot2, stew2) = Kitchen(db);
            var book2 = new QualityBook();
            book2.QueueMasterworkAt(pot2.Def, db, true);
            Assert.False(book2.WillAttempt(Person(2, SkillType.Cooking, 9), pot2, stew2, 1));
            var fx2 = new InteractionEffects(w2, h2, db, seed: 1) { Quality = book2, DayIndex = () => 1 };
            Veg(h2, ItemQuality.Fine);
            fx2.Apply(Person(3, SkillType.Cooking, 10), pot2, stew2);
            Assert.InRange(h2.BestQuality("item.stew_plain"), ItemQuality.Common, ItemQuality.Superior);
            Assert.Empty(book2.Works);
        }

        [Fact]
        public void YadigarSofra_UcKusaklikAileTarifiyleEser()
        {
            var db = Db();
            for (int seed = 1; seed <= 20; seed++)
            {
                var (w, h, pot, stew) = Kitchen(db);
                var book = new QualityBook();
                var fx = new InteractionEffects(w, h, db, seed: seed)
                {
                    Quality = book, DayIndex = () => 1, FamilyRecipe = (c, s) => s == SkillType.Cooking ? (3, "Ayşe'nin peyniri") : (0, null)
                };
                book.QueueMasterworkAt(pot.Def, db, true);
                Veg(h, ItemQuality.Superior);
                fx.Apply(Person(1, SkillType.Cooking, 10), pot, stew);
                int q = h.BestQuality("item.stew_plain");
                Assert.True(q == ItemQuality.Heirloom || q == ItemQuality.Superior, q.ToString());
                if (q == ItemQuality.Heirloom)
                {
                    Assert.Equal("Ayşe'nin peyniri", book.Works.Single().Recipe);
                    return;
                }
            }
            Assert.True(false, "20 denemede yadigâr sofra çıkmadı");
        }

        [Fact]
        public void Eser_UcKusakSonraYadigar()
        {
            var db = Db();
            var h = new Household();
            var w = new WorldState(new GridMap(8, 8));
            var book = new QualityBook();
            var maker = Person(1, SkillType.Carpentry, 10);
            maker.Generation = 2;
            var e = book.Register("item.chest_wood", maker, 10, 1);
            h.Add(new ItemKey("item.chest_wood", ItemQuality.Masterwork, ItemCondition.Sound, false, e.Serial), 1);
            var placed = w.PlaceItem(db.Items.Get("item.chair_wood"), new GridPos(2, 2));
            var e2 = book.Register("item.chair_wood", maker, 10, 1);
            placed.Quality = ItemQuality.Masterwork; placed.Maker = e2.Serial;
            Assert.Empty(book.CheckHeirlooms(3, h, w));                          // çocuk kuşağı: henüz değil
            Assert.Equal(2, book.CheckHeirlooms(4, h, w).Count);                 // torun: üç kuşak
            Assert.Equal(1, h.Count("item.chest_wood", ItemQuality.Heirloom));
            Assert.Equal(ItemQuality.Heirloom, placed.Quality);
            var back = new QualityBook();
            back.Import(JsonSerializer.Deserialize<QualityBookSave>(JsonSerializer.Serialize(book.Export(), Json), Json));
            Assert.True(back.Find(e.Serial).Heirloom);
            Assert.Equal(e2.Serial + 1, back.Register("item.jug", maker, 1, 1).Serial);
        }

        [Fact]
        public void Hasat_BirimBasinaKalite_Toplayan_Balik()
        {
            var db = Db();
            var w = new WorldState(new GridMap(8, 8));
            var h = new Household();
            var fx = new InteractionEffects(w, h, db, seed: 4);
            var fisher = Person(1, SkillType.Fishing, 10);
            fx.GrantFish(fisher, "item.fish_carp", 20, perfect: true);
            Assert.Equal(20, h.Count("item.fish_carp"));
            Assert.True(h.Count("item.fish_carp", ItemQuality.Fine) >= 15);     // kusursuz tutuş +1
            fx.GrantFish(fisher, "item.fish_catfish_giant", 1);
            Assert.Equal(ItemQuality.Superior, h.BestQuality("item.fish_catfish_giant"));   // efsane balık

            // Barınaktaki yumurta kalitesini taşır; usta toplayıcı yükseltebilir.
            var coop = w.PlaceItem(db.Items.Get("item.coop"), new GridPos(4, 4));
            Thatchwick.Core.Animals.AnimalSystem.PutStock(coop, ItemKey.StockKey("item.egg", ItemQuality.Fine), 10);
            var collect = coop.Def.Interactions.First(i => i.Collect);
            fx.Apply(Person(2, SkillType.AnimalHusbandry, 10), coop, collect);
            Assert.True(h.Count("item.egg") >= 10);
            Assert.Equal(0, h.Count("item.egg", 0) - h.Count("item.egg", ItemQuality.Fine));   // hiçbiri düşmedi
            Assert.Equal(ItemQuality.Superior, h.BestQuality("item.egg"));
        }

        // ---- ekonomi ve sosyal ----

        [Fact]
        public void Pazar_KaliteFiyati_SecilenKaliteSatilir_TopluSatisEnDusuktan()
        {
            var db = Db();
            var h = new Household();
            var m = new Market(db, h);
            m.Tick(0, 0, Season.Summer);
            int common = m.SellPrice("item.cheese", ItemQuality.Common), sup = m.SellPrice("item.cheese", ItemQuality.Superior);
            Assert.True(sup > common, $"{sup} > {common}");
            Assert.Equal(m.SellPrice("item.stone"), m.SellPrice("item.stone", ItemQuality.Superior));   // kalitesiz eşya
            h.AddItem("item.cheese", 2);
            h.AddItem("item.cheese", 1, ItemQuality.Superior);
            Assert.True(m.Sell("item.cheese", 1, ItemQuality.Superior) > 0);
            Assert.Equal(0, h.Count("item.cheese", ItemQuality.Fine));
            m.Sell("item.cheese", 1);
            Assert.Equal(1, h.Count("item.cheese"));

            // Yerleşik eşyanın ve envanterden satışın değeri kaliteyle artar.
            var w = new WorldState(new GridMap(10, 10));
            var b = new BuildService(w, h, db);
            var chair = db.Items.Get("item.chair_wood");
            Assert.True(b.SellValue(chair, ItemCondition.Sound, ItemQuality.Superior) > b.SellValue(chair, ItemCondition.Sound));
        }

        [Fact]
        public void Yerlesim_EnIyisiYerlesir_KaldirincaKaliteDoner_GuzellikArtar()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household(100);
            var svc = new BuildService(w, h, db);
            h.AddItem("item.chair_wood", 1);
            h.Add(new ItemKey("item.chair_wood", ItemQuality.Superior, ItemCondition.Sound, false, 5), 1);
            Assert.True(svc.PlaceFromInventory("item.chair_wood", new GridPos(3, 3), 0, out var chair).Ok);
            Assert.Equal(ItemQuality.Superior, chair.Quality);
            Assert.Equal(5, chair.Maker);
            Assert.True(svc.StoreToInventory(chair.Id).Ok);
            Assert.Equal(1, h.Count("item.chair_wood", ItemQuality.Superior));
            Assert.Equal(5, h.Stacks("item.chair_wood").Single(s => s.Key.Quality == ItemQuality.Superior).Key.Maker);
            Assert.True(svc.PlaceFromInventory("item.chair_wood", new GridPos(3, 3), 0, out var again).Ok);
            double best = MoodService.BeautyOfItem(again, db);
            again.Quality = 0;
            if (best > 0) Assert.True(best > MoodService.BeautyOfItem(again, db));
        }

        [Fact]
        public void OymaSusle_BirBasamakArtirir_BirKez_EnCokHas()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var chair = w.PlaceItem(db.Items.Get("item.chair_wood"), new GridPos(3, 3));
            var carve = chair.Def.Interactions.Single(i => i.Embellish);
            Assert.Equal(SkillType.Carpentry, carve.Skill);
            Assert.Equal(ContentMapper.EmbellishLevel, carve.MinSkillLevel);
            var fx = new InteractionEffects(w, h, db, seed: 1);
            h.AddItem(ContentMapper.CarveMaterial, 2);
            var carpenter = Person(1, SkillType.Carpentry, 6);
            Assert.Null(fx.CanStart(carpenter, chair, carve));
            fx.Apply(carpenter, chair, carve);
            Assert.Equal(ItemQuality.Fine, chair.Quality);
            Assert.True(chair.Embellished);
            Assert.NotNull(fx.CanStart(carpenter, chair, carve));                 // eşya başına bir kez
            Assert.DoesNotContain(db.Items.All.Where(d => d.IsRug), d => d.Interactions.Any(i => i.Embellish));
        }

        [Fact]
        public void Siparis_UstaSiparisiHasIster_OdulKaliteyle()
        {
            var db = Db();
            var pop = new Population();
            var house = new Household(50);
            var ali = new Character(1, "Ali", new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            ali.SetAge(40);
            pop.Add(ali);
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), house, pop, new GameClock(), 3);
            var family = new Family.FamilyService(pop, house, db);
            life.Attach(family);
            life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            life.Houses.EnsureHouses();
            var req = life.Requests;
            req.Now = () => 0;
            req.Near = (a, b) => true;
            var press = db.Items.Get("item.cheese_press");
            var craft = press.Interactions.First(i => i.Produce.Any(p => p.ItemId == "item.cheese"));
            var giver = pop.Members.First(m => m.IsVillager && !m.IsDead && m.Stage >= AgeStage.YoungAdult);
            var r = req.PostOrder(giver, craft, false, true);
            r.Read = true;
            var del = r.Objectives.Single();
            Assert.Equal(ItemQuality.Superior, del.MinQuality);
            house.AddItem("item.cheese", del.Count, ItemQuality.Fine);
            Assert.False(req.Deliver(r.Id, ali, false, out var why));            // seçme yetmez
            house.AddItem("item.cheese", del.Count, ItemQuality.Superior);
            int before = house.Akce, baseAkce = r.Reward.Akce;
            Assert.True(req.Deliver(r.Id, ali, false, out why), why.ToString());
            Assert.Equal(RequestStateDone(), r.State);
            Assert.Equal(ItemQuality.Superior, r.DeliveredQuality);
            Assert.Equal(before + (int)Math.Round(baseAkce * ItemQuality.OrderAkceFactor(ItemQuality.Superior)), house.Akce);
            Assert.Equal(del.Count, house.Count("item.cheese", ItemQuality.Fine)); // seçmeler dokunulmadı
        }

        private static Life.Requests.RequestState RequestStateDone() => Life.Requests.RequestState.Done;

        [Fact]
        public void Yeme_KaliteDoyumuVeEtkiSuresi()
        {
            Assert.True(ItemQuality.SatietyFactor(ItemQuality.Superior) > ItemQuality.SatietyFactor(ItemQuality.Fine));
            Assert.Null(ItemQuality.MealMood(ItemQuality.Common));
            Assert.Equal(Mood.MoodCatalog.MealHeirloom, ItemQuality.MealMood(ItemQuality.Heirloom));
            foreach (var id in new[] { Mood.MoodCatalog.MealFine, Mood.MoodCatalog.MealSuperior, Mood.MoodCatalog.MealMasterwork, Mood.MoodCatalog.MealHeirloom })
                Assert.True(Mood.MoodCatalog.Default.TryGet(id, out _), id);
        }
    }
}
