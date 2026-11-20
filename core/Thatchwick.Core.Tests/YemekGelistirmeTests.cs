using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Cooking;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Bütün arayüz tabloları: ana ui.&lt;dil&gt;.json ve alan dosyaları (ui.&lt;alan&gt;.&lt;dil&gt;.json).</summary>
    internal sealed class UiKeys
    {
        private readonly Dictionary<string, string> _all = new Dictionary<string, string>(StringComparer.Ordinal);
        public bool Contains(string key) => _all.ContainsKey(key);
        public bool TryGet(string key, out string value) => _all.TryGetValue(key, out value);
        public IEnumerable<string> Keys => _all.Keys;

        public static UiKeys All(string language)
        {
            string dir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var keys = new UiKeys();
            foreach (var file in Directory.GetFiles(dir, "ui*." + language + ".json"))
                using (var doc = JsonDocument.Parse(File.ReadAllText(file)))
                    foreach (var p in doc.RootElement.EnumerateObject()) keys._all[p.Name] = p.Value.GetString();
            return keys;
        }
    }

    // Yemek geliştirmesi (Project belgeleri 58, 58a, 60a-60c, 62): gruplar, tarif bilgisi, sofra, mezbaha, kalite puanı,
    // Mutfak görevi, rutin, koku ve konuşmalar, baş başa etkinlikler; zincirde adım yeri, etki anı ve güvenli kesme.
    public class YemekGelistirmeTests
    {
        // Pişirme ocakta (belge 63a): yemekler taş ocakta pişer; kazan ocağa götürülen kaptır.
        private const string Pot = "item.hearth_stone";

        private sealed class Rig
        {
            public ContentDatabase Db; public WorldState World; public Household House; public Population Pop; public GameClock Clock;
            public LifeSystems Life; public InteractionEffects Fx; public List<string> Bubbles = new List<string>();
            public Dictionary<int, JobRunner> Runners = new Dictionary<int, JobRunner>();

            public Character Person(int id, string name, GridPos at, double age = 45)
            {
                var c = new Character(id, name, at, true) { CanStarve = false, MoodEnabled = true };
                c.SetAge(age);
                Pop.Add(c);
                var r = new JobRunner(c, World) { Effects = Fx };
                Life.Attach(r);
                Runners[id] = r;
                return c;
            }

            public ItemInstance Place(string id, int x, int y) => World.PlaceItem(Db.Items.Get(id), new GridPos(x, y));
            public InteractionDef Act(ItemInstance item, string id) => item.Def.Interactions.First(i => i.Id == id);
            public InteractionDef Cook(ItemInstance station, string dish) => station.Def.Interactions.First(i => i.Dish != null && i.Dish.Id == dish);

            /// <summary>İşi başından sonuna yürütür (yürüme dahil).</summary>
            public void Run(Character c, ItemInstance item, InteractionDef i, double days = 2.0)
            {
                var r = Runners[c.Id];
                r.Command(Job.UseItem(item.Id, i.Id));
                for (int k = 0; k < 4000 && !r.IsIdle; k++) r.Tick(days / 4000);
                Assert.True(r.IsIdle, "iş bitmedi: " + i.Id + " (" + r.LastFailure + ")");
            }
        }

        private static Rig Make(double startDay = 10.4)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors.Take(10)));
            var rig = new Rig { Db = db, World = new WorldState(new GridMap(40, 40)), House = new Household(200), Pop = new Population(), Clock = new GameClock() };
            rig.Clock.Restore(startDay, 1);
            rig.Life = new LifeSystems(db, rig.World, rig.House, rig.Pop, rig.Clock);
            rig.Fx = new InteractionEffects(rig.World, rig.House, db, 7);
            rig.Life.Attach(rig.Fx);
            rig.Life.AttachEffects(rig.Fx);
            // Bu testler yemeğin kendi kurallarına bakar; kap, alet ve yakıt gereği kapalı (MutfakAletleriTests ve aşağıdaki "Mutfak_" testleri açık sınar).
            rig.Life.KitchenTools.Enforce = false;
            rig.Life.Bubble += (c, t) => rig.Bubbles.Add(c.Name + ":" + t.Key);
            return rig;
        }

        private static void Level(Character c, SkillType s, int level) => c.Skills.RestoreXp(s, SkillRules.XpForLevel(level));

        // ---------- içerik ----------

        [Fact]
        public void Icerik_GruplarBaharatlarVeYemekler_Tutarli()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            Assert.True(db.Groups.Count >= 16);
            Assert.True(db.Groups.Get("group.fish").Members.Count >= 12);
            Assert.DoesNotContain(db.Groups.Get("group.fish").Members, m => m.ItemId.Contains("_old") || m.ItemId.Contains("gold"));   // efsane balık yemeğe girmez
            Assert.True(db.Spices.Count >= 18);
            int meals = db.Dishes.All.Count(d => d.IsMeal);
            Assert.True(meals >= 80, "öğün sayısı " + meals);                       // belge 58 §7 hedefi
            foreach (var d in db.Dishes.All)
            {
                Assert.True(db.Foods.ContainsKey(d.Id), d.Id + ": yeme verisi yok (sofrada ve kilerde yenemez)");
                Assert.NotEmpty(d.Sources);
                if (d.Learn == LearnKind.Villager) Assert.True(db.Villagers.Contains(d.LearnFrom), d.Id + ": köylü yok " + d.LearnFrom);
            }
            // Beceri tablosunun vaat ettiği altı yemek artık oyunda (belge 57 §3).
            foreach (var id in new[] { "fish_fry", "venison_roast", "kome", "sarma_leaf", "saffron_pilaf", "saffron_bun" })
                Assert.True(db.Dishes.Contains("item." + id), id);
            // Her yemek sınıfında en az iki öğün; her çiğ et ve balık en az bir tarife (doğrudan ya da grupla) girer.
            foreach (DishClass cls in Enum.GetValues(typeof(DishClass)))
                if (cls != DishClass.None && cls != DishClass.Drink) Assert.True(db.Dishes.All.Count(d => d.Class == cls) >= 2, cls.ToString());
            var used = new HashSet<string>();
            foreach (var d in db.Dishes.All)
                foreach (var (_, i) in d.Sources)
                    foreach (var c in i.AllConsumed())
                    {
                        used.Add(c.ItemId);
                        if (db.Groups.TryGet(c.ItemId, out var g)) foreach (var m in g.Members) used.Add(m.ItemId);
                    }
            foreach (var item in db.Items.All)
                if ((item.HasTag("raw") || item.HasTag("fish")) && item.HasTag("food") && !item.Id.Contains("_old") && !item.Id.Contains("gold") && !item.Id.Contains("giant"))
                    Assert.True(used.Contains(item.Id), item.Id + " hiçbir yemeğe girmiyor");
        }

        [Fact]
        public void Icerik_KonusmaSatirlarininVeYeniMetinlerinIkiDildeKarsiligiVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var en = UiKeys.All("en"); var tr = UiKeys.All("tr");
            Assert.True(db.Talk.Count >= 60);
            foreach (var t in db.Talk.All)
                for (int k = 0; k < t.Lines; k++)
                {
                    string key = "talk." + t.Id + "." + (char)('a' + k);
                    Assert.True(en.Contains(key), "EN eksik: " + key);
                    Assert.True(tr.Contains(key), "TR eksik: " + key);
                }
            // Kodun kullandığı cook.* anahtarları: kaynak dosyalardan toplanır.
            string root = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "..", "Scripts"));
            var rx = new System.Text.RegularExpressions.Regex("\"(cook\\.[a-z0-9_.]+)\"");
            var keys = new HashSet<string>();
            foreach (var file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
                foreach (System.Text.RegularExpressions.Match m in rx.Matches(File.ReadAllText(file)))
                    if (!m.Groups[1].Value.EndsWith(".")) keys.Add(m.Groups[1].Value);
            Assert.True(keys.Count > 60);
            foreach (var key in keys)
            {
                bool moodId = !key.StartsWith("cook.mood.") && !key.StartsWith("cook.n.") && !key.StartsWith("cook.block.") && !key.StartsWith("cook.out.")
                    && !key.StartsWith("cook.sl.") && !key.StartsWith("cook.say.") && !key.StartsWith("cook.stall.") && !key.StartsWith("cook.rep.")
                    && !key.StartsWith("cook.ui.") && !key.StartsWith("cook.slot.") && !key.StartsWith("cook.pantry.") && !key.StartsWith("cook.class.")
                    && !key.StartsWith("cook.learn.") && !key.StartsWith("cook.routine.") && !key.StartsWith("cook.idle.");
                if (moodId) continue;   // moodlet kimliği (metin değil)
                Assert.True(en.Contains(key), "EN eksik: " + key);
                Assert.True(tr.Contains(key), "TR eksik: " + key);
            }
            foreach (DishClass cls in Enum.GetValues(typeof(DishClass)))
                if (cls != DishClass.None) Assert.True(tr.Contains(DishClasses.Key(cls)) && en.Contains(DishClasses.Key(cls)), cls.ToString());
        }

        // ---------- gruplar ve yeme verisi ----------

        [Fact]
        public void Grup_HaneUyeleriSayarVeHarcar_EskiTarifBaskaEtleDePiser()
        {
            var x = Make();
            x.House.AddItem("item.fish_carp", 1);
            x.House.AddItem("item.fish_salmon", 1, ItemQuality.Fine);
            Assert.Equal(2, x.House.Count("group.fish"));
            Assert.True(x.House.TryTake("group.fish", TakePolicy.Lowest, out var took));
            Assert.Equal("item.fish_carp", took.Id);                 // en düşük kalite ve grup sırası önce
            Assert.True(x.House.TryTake("group.fish", TakePolicy.Lowest, out took));
            Assert.Equal("item.fish_salmon", took.Id);
            Assert.False(x.House.TryTake("group.fish", TakePolicy.Lowest, out _));

            // "Et" isteyen eski tarif artık herhangi bir kırmızı etle pişer (belge 58 §8).
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6));
            Level(cook, SkillType.Cooking, 5);
            var pot = x.Place(Pot, 5, 5);
            var stew = x.Cook(pot, "item.stew_meat");
            Assert.NotNull(x.Fx.CanStart(cook, pot, stew));
            x.House.AddItem("item.mutton", 1); x.House.AddItem("item.onion", 1); x.House.AddItem("item.carrot", 1);
            Assert.Null(x.Fx.CanStart(cook, pot, stew));
            x.Run(cook, pot, stew);
            Assert.Equal(0, x.House.Count("item.mutton"));
            Assert.True(x.House.Count("item.stew_meat") > 0);
        }

        [Fact]
        public void Yeme_KazanaBagliDegil_KilerdenDeYenir()
        {
            var x = Make();
            Assert.Contains(x.Db.Items.Get("item.pantry_shelf").Interactions, i => i.Id == "eat_apple");
            Assert.True(x.Db.Foods.ContainsKey("item.apple"));
            Assert.True(x.Db.Foods.ContainsKey("item.fish_fry"));
            var c = x.Person(1, "Ali", new GridPos(5, 6));
            var shelf = x.Place("item.pantry_shelf", 5, 5);
            x.House.AddItem("item.apple", 1);
            c.Needs.Set(NeedType.Hunger, 30f);
            x.Run(c, shelf, x.Act(shelf, "eat_apple"));
            Assert.Equal(0, x.House.Count("item.apple"));
            Assert.True(c.Needs.Get(NeedType.Hunger) > 30f);
        }

        // ---------- kalite puanı ----------

        [Fact]
        public void Kalite_AltiBilesen_EsiklerVeYanik()
        {
            Assert.Equal(10, DishScore.SkillPoints(3, 3));
            Assert.Equal(25, DishScore.SkillPoints(10, 3));
            Assert.Equal(2, DishScore.MasteryPoints(0, false, 0));
            Assert.Equal(4, DishScore.MasteryPoints(5, false, 0));
            Assert.Equal(6, DishScore.MasteryPoints(15, false, 0));
            Assert.Equal(9, DishScore.MasteryPoints(40, true, 0));
            Assert.Equal(ItemQuality.Common, DishScore.TierOf(49));
            Assert.Equal(ItemQuality.Fine, DishScore.TierOf(50));
            Assert.Equal(ItemQuality.Superior, DishScore.TierOf(75));
            Assert.Equal(0, DishScore.SpiceSlots(2));
            Assert.Equal(1, DishScore.SpiceSlots(3));
            Assert.Equal(2, DishScore.SpiceSlots(6));
            Assert.True(DishScore.BurnChance(1, 1) > DishScore.BurnChance(3, 1));
            Assert.Equal(0.0, DishScore.BurnChance(4, 1));
            var burnt = new DishScoreBreakdown { Ingredients = 35, Skill = 25, Mastery = 10, Equipment = 10, Kitchen = 10, Cook = 10, Burnt = true };
            Assert.Equal(ItemQuality.Fine, burnt.Quality);   // yanık bir basamak düşürür
            int total = DishScore.MaxIngredients + DishScore.MaxSkill + DishScore.MaxMastery + DishScore.MaxEquipment + DishScore.MaxKitchen + DishScore.MaxCook;
            Assert.Equal(100, total);
        }

        [Fact]
        public void Kalite_UstaIyiMalzemeyleHasPisirir_AcemiSiradan()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var master = x.Person(1, "Usta", new GridPos(5, 6));
            Level(master, SkillType.Cooking, 10);
            var novice = x.Person(2, "Çırak", new GridPos(6, 6));
            Level(novice, SkillType.Cooking, 4);
            var yahni = x.Cook(pot, "item.beef_yahni");
            void Stock(int q) { x.House.AddItem("item.meat", 2, q); x.House.AddItem("item.onion", 2, q); x.House.AddItem("item.carrot", 1, q); }
            Stock(ItemQuality.Superior);
            var est = x.Life.Meals.Estimate(master, pot, yahni);
            Assert.Equal(35, est.Ingredients);
            Assert.Equal(25, est.Skill);
            x.Run(master, pot, yahni);
            Assert.True(x.House.Count("item.beef_yahni", ItemQuality.Superior) > 0, "usta has pişirmeli");
            while (x.House.TryTakeItem("item.beef_yahni")) { }
            Stock(ItemQuality.Common);
            x.Run(novice, pot, yahni);
            Assert.Equal(0, x.House.Count("item.beef_yahni", ItemQuality.Fine));
            Assert.True(x.House.Count("item.beef_yahni") > 0);
        }

        // ---------- mutfak aletleriyle birlikte (belge 63, 63a) ----------

        [Fact]
        public void Mutfak_EkipmanPuaniSofraPuaninaGirer_KapTavaniKeser()
        {
            var x = Make();
            x.Life.KitchenTools.Enforce = true;
            var hearth = x.Place(Pot, 5, 5);
            var table = x.Place("item.table_wood", 10, 5);
            var master = x.Person(1, "Usta", new GridPos(5, 6));
            Level(master, SkillType.Cooking, 10);
            var yahni = x.Cook(hearth, "item.beef_yahni");
            Assert.Equal("stew", yahni.Cook);                                          // yahni güveçte pişer
            void Stock() { x.House.AddItem("item.meat", 2, ItemQuality.Superior); x.House.AddItem("item.onion", 2, ItemQuality.Superior); x.House.AddItem("item.carrot", 1, ItemQuality.Superior); }
            Stock();
            x.House.AddItem("item.firewood", 8); x.House.AddItem("item.kitchen_knife", 1);
            Assert.NotNull(x.Fx.CanStart(master, hearth, yahni));                      // güveç kabı yok: emir verilemez

            x.House.AddItem("item.guvec_plain", 1);                                    // kademe 1: tavan 70, en çok Seçme
            Assert.Null(x.Fx.CanStart(master, hearth, yahni));
            var use = x.Life.KitchenTools.Preview(master, hearth, yahni, out _);
            var est = x.Life.Meals.Estimate(master, hearth, yahni);
            Assert.Equal(use.Score, est.Equipment);                                    // ekipman bileşeni mutfak aletlerinden gelir
            Assert.Equal(ItemQuality.Fine, est.MaxQuality);
            Assert.True(est.Quality <= ItemQuality.Fine);
            Assert.Equal(0, x.Life.KitchenTools.QualityFor(master, hearth, yahni).Value.levelShift);   // yemekte seviye kaymaz, puan sofra puanında

            x.Life.Meals.Order(master, hearth, yahni.Id, batch: 1, serve: true);
            x.Run(master, hearth, yahni);
            var s = x.Life.Meals.ServingAt(table.Id);
            Assert.NotNull(s);                                                         // tavanlı kalite sofraya da aynı gider
            Assert.True(s.Quality <= ItemQuality.Fine);
            Assert.Equal(0, x.House.Count("item.beef_yahni", ItemQuality.Superior));
            Assert.Equal(1, x.Life.KitchenTools.DirtyCount("item.guvec_plain"));       // pişirme kabı kirletir
            Assert.True(x.Life.KitchenTools.DirtyCount("item.kitchen_knife") == 1);

            // Daha iyi kap: puan yükselir, tavan kalkar; aynı usta ve malzemeyle Has çıkar.
            x.House.AddItem("item.guvec_fine", 1, ItemQuality.Superior);
            x.House.AddItem("item.wooden_spoon", 1); x.House.AddItem("item.ladle", 1); x.House.AddItem("item.cutting_board", 1);
            Stock();
            var better = x.Life.Meals.Estimate(master, hearth, yahni);
            Assert.True(better.Equipment > est.Equipment);
            Assert.Equal(ItemQuality.Max, better.MaxQuality);
            Assert.True(better.Total >= DishScore.SuperiorAt, "puan " + better.Total);
            Assert.Equal(ItemQuality.Superior, better.Quality);

            // Tavan puanın kendisinde: aynı toplam, kil kapta Seçme'de kalır.
            Assert.Equal(ItemQuality.Fine, new DishScoreBreakdown { Ingredients = 35, Skill = 25, Mastery = 10, Equipment = 10, MaxQuality = ItemQuality.Fine }.Quality);
        }

        [Fact]
        public void Mutfak_AyakIsiVeAdimYeriTekSirada_IptalMalzemeyiVeKabiBirakir()
        {
            var x = Make();
            for (int yy = 0; yy < 40; yy++) for (int xx = 0; xx < 40; xx++) x.World.Map.SetOwned(new GridPos(xx, yy), true);
            x.Life.KitchenTools.Enforce = true;
            var hearth = x.Place(Pot, 5, 5);
            var shelf = x.Place("item.pantry_shelf", 14, 5);                           // "malzemeyi al" adımının yeri
            var side = x.Place("item.table_wood", 5, 12);
            var vessel = x.World.PlaceOnSurface(x.Db.Items.Get("item.pot_clay"), side.Id, side.Def.SurfaceSlots[0].Id);   // kap yerinden alınır, iş bitince yerine konur
            x.Place("item.water_barrel", 9, 12);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 5);
            var other = x.Person(2, "Fatma", new GridPos(6, 6)); Level(other, SkillType.Cooking, 5);
            var soup = x.Cook(hearth, "item.soup_cabbage");
            Assert.Equal("boil", soup.Cook);
            void Stock() { x.House.AddItem("item.cabbage", 2); x.House.AddItem("item.onion", 1); }
            Stock();
            x.House.AddItem("item.firewood", 6); x.House.AddItem("item.kitchen_knife", 1);

            var r = x.Runners[1];
            var steps = new List<string>();
            r.StepChanged += (_, st) => { if (st != null) steps.Add(st.Id); };
            bool atPantry = false, heldVessel = false;
            r.Command(Job.UseItem(hearth.Id, soup.Id));
            for (int k = 0; k < 20000 && !r.IsIdle; k++)
            {
                r.Tick(0.0002);
                if (r.IsUsingItem && Math.Abs(cook.Position.X - shelf.Position.X) + Math.Abs(cook.Position.Y - shelf.Position.Y) <= 1) atPantry = true;
                if (r.HeldItemId == "item.pot_clay") heldVessel = true;
            }
            Assert.True(r.IsIdle, "iş bitmedi (" + r.LastFailure + ")");
            // Tek sıra: kabı al, su doldur (ayak işi) → malzemeyi kilerden al, doğra, pişir (zincir) → kabı yerine koy.
            int fetch = steps.IndexOf("fetch_vessel"), fill = steps.IndexOf("fill_water"), gather = steps.IndexOf("gather"), stir = steps.IndexOf("stir"), back = steps.LastIndexOf("put_back");
            Assert.True(fetch >= 0 && fill > fetch && gather > fill && stir > gather && back > stir, string.Join(",", steps));
            Assert.True(heldVessel, "kap elde taşınmalı");
            Assert.True(atPantry, "malzeme için kilere yürümeli");
            Assert.Equal(0, vessel.TakenBy);                                           // yerine kondu
            Assert.Equal(0, x.House.Count("item.cabbage"));
            Assert.True(x.House.Count("item.soup_cabbage") > 0);
            Assert.Equal(1, x.Life.KitchenTools.DirtyCount("item.pot_clay"));

            // İptal: ayrılan malzeme geri konur (yemek geliştirmesi), kap ve bıçak serbest kalır (mutfak aletleri).
            Stock();
            r.Command(Job.UseItem(hearth.Id, soup.Id));
            for (int k = 0; k < 20000 && x.House.Count("item.cabbage") > 0; k++) r.Tick(0.0002);
            Assert.Equal(0, x.House.Count("item.cabbage"));                            // "malzemeyi al" adımında ayrıldı
            Assert.Equal(cook.Id, vessel.TakenBy);
            Assert.NotNull(x.Fx.CanStart(other, hearth, soup));                        // tek tencere meşgul
            r.Cancel();
            Assert.Equal(2, x.House.Count("item.cabbage"));
            Assert.Equal(1, x.House.Count("item.onion"));
            Assert.Equal(0, vessel.TakenBy);
            Assert.Null(x.Fx.CanStart(other, hearth, soup));
        }

        // ---------- tarif bilgisi ve defter ----------

        [Fact]
        public void Tarif_BilinmeyenPisirilemez_OgrenilirYazilirOkunur()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var a = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(a, SkillType.Cooking, 8);
            var b = x.Person(2, "Bora", new GridPos(6, 6)); Level(b, SkillType.Cooking, 8);
            var steam = x.Cook(pot, "item.fish_steam");
            x.House.AddItem("item.fish_carp", 4); x.House.AddItem("item.onion", 2); x.House.AddItem("item.vinegar", 2);
            Assert.NotNull(x.Fx.CanStart(a, pot, steam));                         // köylüden öğrenilen tarif: bilmiyor
            Assert.True(x.Life.Recipes.Knows(a, x.Db.Dishes.Get("item.soup_cabbage")));   // temel tarifi herkes bilir
            Assert.True(x.Life.Recipes.Knows(a, x.Db.Dishes.Get("item.fish_fry")));       // seviye tarifi beceriyle açılır
            Assert.True(x.Life.Recipes.Learn(a, "item.fish_steam", "villager"));
            Assert.False(x.Life.Recipes.Learn(a, "item.fish_steam", "villager"));
            Assert.Null(x.Fx.CanStart(a, pot, steam));
            Assert.NotNull(x.Fx.CanStart(b, pot, steam));                         // bilgi kişide (62-S4)

            var page = x.Life.Recipes.Pages(x.Pop.Members).First(p => p.Dish.Id == "item.fish_steam");
            Assert.Equal(RecipeState.Known, page.State);
            Assert.True(page.OnlyKnower);                                         // "tek bilen!"
            Assert.True(x.Life.Recipes.WriteDown(a) > 0);
            Assert.True(x.Life.Recipes.IsWritten("item.fish_steam"));
            DishDef learned = null;
            for (int k = 0; k < 40 && !x.Life.Recipes.Knows(b, steam.Dish); k++) learned = x.Life.Recipes.ReadFromBook(b);
            Assert.True(x.Life.Recipes.Knows(b, steam.Dish));
            Assert.NotNull(learned);
            Assert.Null(x.Fx.CanStart(b, pot, steam));
        }

        [Fact]
        public void Tarif_EliAlisti_KoyluYakinligi_DamakTadi_Kayit()
        {
            var x = Make();
            var a = x.Person(1, "Ayşe", new GridPos(5, 6));
            var dish = x.Db.Dishes.Get("item.beef_yahni");
            var settled = new List<int>();
            x.Life.Recipes.HandSettled += (c, d, tier) => settled.Add(tier);
            for (int k = 0; k < 15; k++) x.Life.Recipes.OnCooked(a, dish, ItemQuality.Fine);
            Assert.Equal(new[] { 1, 2 }, settled);                                // 5 ve 15 (60b §5.3)
            Assert.Equal(2, x.Life.Recipes.MasteryTier(a, dish.Id));

            // Köylü yakınlığı eşiği aşınca tarifini verir (58 §4).
            x.Life.Recipes.PollVillagerRecipes(x.Pop.Members, (m, def) => def == "villager.kemal" ? 29 : -1);
            Assert.False(x.Life.Recipes.HasLearned(a, "item.fish_steam"));
            x.Life.Recipes.PollVillagerRecipes(x.Pop.Members, (m, def) => def == "villager.kemal" ? 30 : -1);
            Assert.True(x.Life.Recipes.HasLearned(a, "item.fish_steam"));
            Assert.False(x.Life.Recipes.HasLearned(a, "item.soup_crayfish"));    // o 60'ta

            var t = x.Life.Recipes.TastesOf(a);
            Assert.NotNull(t.Loved1); Assert.NotNull(t.Loved2);
            Assert.NotEqual(t.Loved1, t.Loved2);
            Assert.False(t.Loves(t.Disliked));

            var saved = x.Life.Recipes.Export();
            var book = new RecipeBook(x.Db);
            book.Import(saved);
            Assert.Equal(15, book.TimesCooked(a, dish.Id));
            Assert.True(book.HasLearned(a, "item.fish_steam"));
            Assert.Equal(t.Loved1, book.TastesOf(a).Loved1);
        }

        // ---------- sofra ----------

        [Fact]
        public void Sofra_ServisKabiMasayaGelir_PorsiyonAzalir_ToplanirYikanir()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var table = x.Place("item.table_wood", 10, 5);
            var tub = x.Place("item.dish_tub", 14, 5);
            x.Place("item.water_barrel", 16, 5);
            x.House.AddItem("item.wooden_bowl", 6); x.House.AddItem("item.dish_basket", 1);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 6);
            var soup = x.Cook(pot, "item.soup_cabbage");
            x.House.AddItem("item.cabbage", 4); x.House.AddItem("item.onion", 2);
            Assert.NotNull(x.Fx.CanStart(cook, table, x.Act(table, "eat_serving")));   // sofra boş
            x.Life.Meals.Order(cook, pot, soup.Id, batch: 2, serve: true);
            x.Run(cook, pot, soup);
            Assert.Equal(0, x.House.Count("item.cabbage"));                            // iki kat malzeme
            var s = x.Life.Meals.ServingAt(table.Id);
            Assert.NotNull(s);
            Assert.Equal(6, s.Portions);                                               // 3 porsiyon × 2
            Assert.Equal(4, s.FillStage);
            Assert.Equal(0, x.House.Count("item.soup_cabbage"));                       // yemek envanterde değil, masada
            Assert.Contains(x.Bubbles, b => b.EndsWith("cook.say.table_ready"));

            cook.Needs.Set(NeedType.Hunger, 20f);
            x.Runners[1].Cancel();   // "sofra hazır" çağrısıyla sıraya giren işi temizle
            x.Run(cook, table, x.Act(table, "eat_serving"));
            Assert.Equal(5, x.Life.Meals.ServingAt(table.Id).Portions);
            Assert.True(cook.Needs.Get(NeedType.Hunger) > 60f);
            Assert.Equal(1, x.Life.Meals.DirtyAt(table.Id));
            for (int k = 0; k < 3; k++) { cook.Needs.Set(NeedType.Hunger, 20f); x.Run(cook, table, x.Act(table, "eat_serving")); }
            Assert.Equal(2, x.Life.Meals.ServingAt(table.Id).FillStage);               // yarım

            // Bulaşık tek sistemde (belge 63a §4): sofradan kalkan tabaklar mutfak aletlerinin bulaşığına girer,
            // sepetle leğene taşınıp yıkanır. (Kap ve alet gereği bu testte kapalıydı; bulaşık için açılır.)
            x.Life.KitchenTools.Enforce = true;
            Assert.Equal(4, x.Life.Meals.PlatesOnTables);                              // tabaklar masada bekliyor
            Assert.Equal(2, x.Life.KitchenTools.CleanTableware);                       // altı kâseden dördü kullanıldı
            Assert.NotNull(x.Fx.CanStart(cook, tub, x.Act(tub, "wash_dishes")));       // toplanmadan yıkanacak bulaşık yok
            x.Run(cook, table, x.Act(table, "clear_table"));
            Assert.Null(x.Life.Meals.ServingAt(table.Id));
            Assert.Equal(2, x.House.Count("item.soup_cabbage"));                       // artan kilere
            Assert.Equal(0, x.Life.Meals.PlatesOnTables);
            Assert.Equal(4, x.Life.KitchenTools.DirtyTableware);
            x.Run(cook, tub, x.Act(tub, "wash_dishes"));
            Assert.Equal(0, x.Life.KitchenTools.DirtyPieces);
            Assert.Equal(6, x.Life.KitchenTools.CleanTableware);
            Assert.DoesNotContain(x.Db.Items.Get("item.basin_ewer").Interactions, i => i.Id == "wash_dishes");   // basit bulaşık işi kalktı
        }

        [Fact]
        public void Sofra_BirlikteYeme_AileSefkati_ArtanBozulur()
        {
            var x = Make();
            var table = x.Place("item.table_wood", 10, 5);
            var mother = x.Person(1, "Anne", new GridPos(10, 6));
            var child = x.Person(2, "Çocuk", new GridPos(11, 6), 15);
            child.BirthMotherId = mother.Id;
            x.House.AddItem("item.soup_cabbage", 4);
            Assert.NotNull(x.Life.Meals.ServeFromInventory(mother, table, "item.soup_cabbage", 4));
            foreach (var r in x.Runners.Values) r.Cancel();
            mother.Needs.Set(NeedType.Hunger, 30f); child.Needs.Set(NeedType.Hunger, 30f);
            x.Run(mother, table, x.Act(table, "eat_serving"), 0.2);
            x.Run(child, table, x.Act(table, "eat_serving"), 0.2);
            Assert.True(mother.Mood.Has("cook.ate_together"));
            Assert.True(child.Mood.Has("cook.family_care"));                           // "Aile şefkati" (60b §5.5)
            Assert.Equal(1, x.Life.Recipes.FamilyMeals(child));

            // Açıkta kalan yemek bayatlar, sonra bozulur.
            var s = x.Life.Meals.ServingAt(table.Id);
            s.CookedAt = x.Clock.TotalDays - MealService.StaleAfter - 0.01;
            x.Life.Meals.Tick();
            Assert.True(s.Stale);
            s.CookedAt = x.Clock.TotalDays - MealService.SpoilAfter - 0.01;
            x.Life.Meals.Tick();
            Assert.Null(x.Life.Meals.ServingAt(table.Id));
        }

        [Fact]
        public void Sofra_EkBaharat_YuvaBeceriyeBagli_EtkiYiyeneGecer()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var table = x.Place("item.table_wood", 10, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 2);
            var soup = x.Cook(pot, "item.soup_cabbage");
            x.House.AddItem("item.cabbage", 2); x.House.AddItem("item.onion", 1); x.House.AddItem("item.mint_ground", 1);
            x.Life.Meals.Order(cook, pot, soup.Id, 1, new[] { "item.mint_ground" });
            Assert.NotNull(x.Fx.CanStart(cook, pot, soup));                            // Yemek 3'ten önce yuva yok
            Level(cook, SkillType.Cooking, 5);
            Assert.Null(x.Fx.CanStart(cook, pot, soup));
            x.Run(cook, pot, soup);
            Assert.Equal(0, x.House.Count("item.mint_ground"));
            Assert.Contains("item.mint_ground", x.Life.Meals.ServingAt(table.Id).Spices);
            x.Runners[1].Cancel();
            cook.Needs.Set(NeedType.Hunger, 20f);
            x.Run(cook, table, x.Act(table, "eat_serving"));
            Assert.True(cook.Mood.Has("eff_spice_mint"));
            Assert.Equal(3, DishScore.SpicePoints(new[] { x.Db.Spices.Get("item.mint_ground") }, DishClass.Soup));
            Assert.Equal(-3, DishScore.SpicePoints(new[] { x.Db.Spices.Get("item.garlic") }, DishClass.Sweet));   // tatlıya sarımsak
        }

        // ---------- zincir: adım yeri, etki anı, güvenli kesme, eşya dışı işler ----------

        private static (WorldState world, Character c, JobRunner runner, ItemInstance item) Bench(ActionChainDef chain, InteractionDef interaction = null)
        {
            var world = new WorldState(new GridMap(16, 16));
            interaction = interaction ?? new InteractionDef { Id = "work", DisplayName = "Çalış", DurationDays = 0.1, Skill = SkillType.Cooking };
            interaction.ChainDef = chain;
            var item = world.PlaceItem(new ItemDef { Id = "bench", DisplayName = "Tezgâh", Interactions = new List<InteractionDef> { interaction } }, new GridPos(6, 6));
            var c = new Character(1, "Maud", new GridPos(6, 5)) { IsPlayerControlled = true };
            c.SetAge(50);
            return (world, c, new JobRunner(c, world), item);
        }

        [Fact]
        public void Zincir_AdimYeri_KarakterOrayaYurur_SureyeEklenir()
        {
            var chain = new ActionChainDef { Id = "z" };
            chain.Steps.Add(new ActionStepDef { Id = "al", Share = 1, At = "storage" });
            chain.Steps.Add(new ActionStepDef { Id = "yap", Share = 3 });
            var x = Bench(chain);
            var far = new GridPos(12, 5);
            x.runner.StepPlace = (c, item, role) => role == "storage" ? far : (GridPos?)null;
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.0001);
            Assert.True(x.runner.IsStepWalking);
            Assert.Equal("al", x.runner.CurrentStep.Id);
            bool reached = false;
            double spent = 0;
            for (int k = 0; k < 20000 && !x.runner.IsIdle; k++)
            {
                x.runner.Tick(0.0005); spent += 0.0005;
                if (x.c.Position.Equals(far)) reached = true;
            }
            Assert.True(reached, "kilere yürümedi");
            Assert.True(x.runner.IsIdle);
            Assert.True(spent > 0.1, "yürüme süresi işe eklenmeli");
            Assert.True(Math.Abs(x.c.Position.X - 6) + Math.Abs(x.c.Position.Y - 6) <= 1, "iş bitince eşyanın başında olmalı");
        }

        [Fact]
        public void Zincir_EtkiAni_MalzemeAdimdaAlinir_IptalGeriKoyar()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 5);
            var soup = x.Cook(pot, "item.soup_cabbage");
            Assert.Contains(soup.ChainDef.Steps, s => s.Take);
            x.House.AddItem("item.cabbage", 2); x.House.AddItem("item.onion", 1);
            var r = x.Runners[1];
            r.Command(Job.UseItem(pot.Id, soup.Id));
            r.Tick(0.002);
            Assert.NotNull(r.CurrentStep);
            Assert.Equal(0, x.House.Count("item.cabbage"));          // "malzemeyi al" adımında envanterden çıktı
            r.Cancel();
            Assert.Equal(2, x.House.Count("item.cabbage"));          // iş kesildi: geri kondu
            Assert.Equal(1, x.House.Count("item.onion"));
            x.Run(cook, pot, soup);
            Assert.Equal(0, x.House.Count("item.cabbage"));
        }

        [Fact]
        public void Zincir_GuvenliKesme_KapanisAdimlariOynar_EtkiVermez()
        {
            var chain = new ActionChainDef { Id = "banyo" };
            chain.Steps.Add(new ActionStepDef { Id = "soyun", Share = 1 });
            chain.Steps.Add(new ActionStepDef { Id = "yikan", Share = 5, Undress = true });
            chain.Steps.Add(new ActionStepDef { Id = "kurulan", Share = 1, Undress = true, Closing = true });
            chain.Steps.Add(new ActionStepDef { Id = "giyin", Share = 1, Closing = true });
            var bath = new InteractionDef { Id = "work", DisplayName = "Yıkan", DurationDays = 0.1, Need = NeedType.Hygiene, Restore = 60 };
            var x = Bench(chain, bath);
            x.c.Needs.Set(NeedType.Hygiene, 10f);
            var seen = new List<string>();
            x.runner.StepChanged += (_, s) => seen.Add(s.Id);
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.04);
            Assert.Equal("yikan", x.runner.CurrentStep.Id);
            Assert.True(x.runner.IsUndressed);
            x.runner.Cancel();                                       // küvetten hemen fırlamaz
            Assert.False(x.runner.IsIdle);
            Assert.True(x.runner.IsClosingOut);
            Assert.Equal("kurulan", x.runner.CurrentStep.Id);
            x.runner.Tick(JobRunner.ClosingStepDays * 1.5);
            Assert.Equal("giyin", x.runner.CurrentStep.Id);
            Assert.False(x.runner.IsUndressed);
            x.runner.Tick(JobRunner.ClosingStepDays);
            Assert.True(x.runner.IsIdle);
            Assert.Equal(new[] { "soyun", "yikan", "kurulan", "giyin" }, seen);
            Assert.InRange(x.c.Needs.Get(NeedType.Hygiene), 9f, 11f);   // yarıda kesilen iş etki vermez

            // Kapanış sürerken ikinci iptal işi anında keser; kapanış adımı olmayan zincir de anında kesilir.
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.04);
            x.runner.Cancel(); x.runner.Cancel();
            Assert.True(x.runner.IsIdle);
        }

        [Fact]
        public void Zincir_EsyaDisiIsler_SosyalHayvanKaro_IcerikteBagli()
        {
            var db = ContentFilesTests.LoadDb(out _);
            Assert.Equal("soc_chat", db.JobChain("social:Chat").Id);
            Assert.Equal("soc_picnic", db.JobChain("social:Picnic").Id);
            Assert.Equal("ani_brush", db.JobChain("animal:Brush").Id);
            Assert.Equal("tile_till", db.JobChain("tile:Till").Id);
            Assert.Null(db.JobChain("social:OlmayanIs"));
            // Yumurta toplarken inek sağılmaz (belge 63 §3.3): toplama zinciri barınağa göre ayrıldı.
            Assert.Equal("collect_eggs", db.Items.Get("item.coop").Interactions.First(i => i.Collect).ChainDef.Id);
            Assert.Equal("collect_milk", db.Items.Get("item.barn").Interactions.First(i => i.Collect).ChainDef.Id);
            Assert.Equal("eat_serving", db.Items.Get("item.table_wood").Interactions.First(i => i.MealAction == MealService.EatServing).ChainDef.Id);
            Assert.Contains(db.Chains.Get("bath").Steps, s => s.Closing);
            Assert.Contains(db.Chains.Get("cook_pot").Steps, s => s.At == "storage");
        }

        // ---------- mezbaha ----------

        [Fact]
        public void Mezbaha_SecilenHayvan_YanUrunlerDepoya_GencHayvandanSirden()
        {
            var x = Make();
            var animals = new AnimalSystem(x.World, x.Db, seed: 3);
            x.Life.Attach(animals);
            var barn = x.Place("item.barn", 12, 5);
            var hook = x.Place("item.slaughter_hook", 20, 20);
            var butcher = x.Person(1, "Kasap", new GridPos(20, 21));
            Level(butcher, SkillType.AnimalHusbandry, 10);
            var cow = animals.Restore(1, x.Db.Animals.Get("animal.cow"), new GridPos(11, 5), barn.Id, 100f, 40, 0, 0, friendship: 10f);
            var lamb = animals.Restore(2, x.Db.Animals.Get("animal.sheep"), new GridPos(11, 6), barn.Id, 100f, x.Db.Animals.Get("animal.sheep").MatureDays + 0.5, 0, 0, friendship: 10f);
            var friend = animals.Restore(3, x.Db.Animals.Get("animal.goat"), new GridPos(11, 7), barn.Id, 100f, 30, 0, 0, friendship: 90f);
            var sh = x.Life.Butchery;
            sh.InRoom = _ => true;   // oda türü ayrı test edilir (HomeRooms)
            var cut = x.Act(hook, "slaughter");
            Assert.NotNull(x.Fx.CanStart(butcher, hook, cut));                    // liste boş: kimse kendi başına hayvan seçmez
            Assert.Equal(3, sh.Rows().Count);
            Assert.NotNull(sh.Rows().First(r => r.Animal == friend).Blocked);
            Assert.False(sh.Mark(friend, true));
            Assert.True(sh.Mark(cow, true));
            Assert.True(sh.Mark(lamb, true));
            Assert.Null(x.Fx.CanStart(butcher, hook, cut));

            x.Run(butcher, hook, cut);
            Assert.DoesNotContain(animals.Animals, a => a.Id == cow.Id);
            Assert.Equal(0, x.House.Count("item.meat"));                          // ürünler depoda
            var depot = sh.Depot.ToList();
            int In(string id) => depot.Where(e => e.key.Id == id).Sum(e => e.count);
            Assert.Equal(4, In("item.meat"));
            Assert.Equal(2, In("item.hide"));                                     // usta deriyi yırtmaz
            Assert.Equal(3, In("item.bone"));
            Assert.Equal(2, In("item.offal"));
            Assert.Equal(2, In("item.gut"));
            Assert.Equal(0, In("item.rennet"));                                   // yaşlı inekten şirden çıkmaz
            Assert.True(butcher.Mood.Has(Thatchwick.Core.Mood.MoodCatalog.Slaughtered));

            x.Run(butcher, hook, cut);                                            // sıradaki: kuzu
            Assert.Equal(1, sh.Depot.Where(e => e.key.Id == "item.rennet").Sum(e => e.count));
            Assert.Equal(2, sh.Depot.Where(e => e.key.Id == "item.mutton").Sum(e => e.count));
            Assert.NotNull(x.Fx.CanStart(butcher, hook, cut));                    // liste bitti

            x.Run(butcher, hook, x.Act(hook, "collect_meat"));
            Assert.Equal(0, sh.DepotCount);
            Assert.Equal(4, x.House.Count("item.meat"));
            Assert.Equal(1, x.House.Count("item.rennet"));
        }

        [Fact]
        public void Mezbaha_PazardaKestir_ErtesiSabahYalnizEt_UyarilarVeOdaTuru()
        {
            var x = Make();
            var animals = new AnimalSystem(x.World, x.Db, seed: 3);
            x.Life.Attach(animals);
            var barn = x.Place("item.barn", 12, 5);
            var cow = animals.Restore(1, x.Db.Animals.Get("animal.cow"), new GridPos(11, 5), barn.Id, 100f, 40, 0, 0, friendship: 45f, female: true, pregnantDays: 1);
            var sh = x.Life.Butchery;
            var row = sh.Row(cow);
            Assert.True(row.Warnings.HasFlag(SlaughterWarning.Pregnant));
            Assert.True(row.Warnings.HasFlag(SlaughterWarning.Loved));
            Assert.Contains(row.Byproducts, b => b.ItemId == "item.horn");
            Assert.True(sh.Mark(cow, true));
            long today = x.Clock.DayIndex;
            Assert.Equal(1, sh.SendListToMarket(today));
            Assert.Empty(animals.Animals);
            sh.OnNewDay(today);
            Assert.Equal(0, x.House.Count("item.meat"));                          // ertesi sabah
            sh.OnNewDay(today + 1);
            Assert.Equal(4, x.House.Count("item.meat"));
            Assert.Equal(0, x.House.Count("item.hide"));                          // deri ve öbür parçalar kasapta kalır
            Assert.Equal(0, x.House.Count("item.bone"));

            // Kayıt gidip gelir.
            var cow2 = animals.Restore(5, x.Db.Animals.Get("animal.cow"), new GridPos(11, 5), barn.Id, 100f, 40, 0, 0);
            sh.Mark(cow2, true);
            var data = sh.Export();
            sh.Import(data);
            Assert.True(sh.IsMarked(cow2));
            // Kesim barınaktan kalktı; kanca, mezbaha odasının anahtar eşyası.
            foreach (var id in new[] { "item.coop", "item.barn", "item.pigsty", "item.stall" })
                Assert.DoesNotContain(x.Db.Items.Get(id).Interactions, i => i.Slaughter);
            Assert.True(x.Db.Items.Get("item.slaughter_hook").HasTag(Slaughterhouse.HookTag));
            Assert.Equal("Mezbaha", HomeLife.RoomKindName(RoomKind.Slaughterhouse));
        }

        // ---------- Mutfak görevi, kiler planı, rutin ----------

        [Fact]
        public void MutfakGorevi_CizelgedekiniPisirir_MalzemeYoksaBaskaTarifeGecmez()
        {
            var x = Make(10.30);   // sabah öğünü penceresi
            var pot = x.Place(Pot, 5, 5);
            var table = x.Place("item.table_wood", 10, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 6);
            cook.Duty = DutyKind.Kitchen;
            var k = x.Life.Kitchen;
            Assert.True(k.Add(MealSlot.Morning, "item.soup_cabbage", 1));
            Assert.True(k.Add(MealSlot.Morning, "item.beef_yahni", 1));
            Assert.False(k.Add(MealSlot.Morning, "item.honey_milk"));             // içecek öğün değil
            var notices = new List<string>();
            k.Notice += (t, c) => notices.Add(t.Key);

            Assert.Equal(MealSlot.Morning, k.CurrentSlot());
            Assert.DoesNotContain(k.Chores(cook), ch => ch.interaction.Dish != null);   // malzeme yok
            Assert.Single(notices);
            Assert.Equal("cook.n.duty_missing", notices[0]);
            Assert.Equal("item.soup_cabbage", k.NextEntry(MealSlot.Morning).DishId);    // sıradaki tarife geçmedi (58a §4)

            x.House.AddItem("item.cabbage", 2); x.House.AddItem("item.onion", 1);
            var chore = k.Chores(cook).First();
            Assert.Equal("item.soup_cabbage", chore.interaction.Dish.Id);
            x.Run(cook, chore.item, chore.interaction);
            Assert.NotNull(x.Life.Meals.ServingAt(table.Id));                           // görevli sofrayı kurar
            Assert.Equal("item.beef_yahni", k.NextEntry(MealSlot.Morning).DishId);      // çizelge döner
            Assert.DoesNotContain(k.Chores(cook), ch => ch.interaction.Dish != null);   // bu öğün tamam

            // Planlayıcıya bağlanır; kayıt gidip gelir.
            var planner = new DutyPlanner(x.World, null, x.Fx, null);
            x.Life.Attach(planner);
            Assert.NotNull(planner.KitchenChores);
            Assert.Equal("Mutfak", DutyPlanner.DutyName(DutyKind.Kitchen));
            k.SetTarget("item.pickled_cabbage", 20);
            var saved = k.Export();
            k.Import(saved);
            Assert.Equal(2, k.Plan(MealSlot.Morning).Count);
            Assert.Equal(20, k.Targets["item.pickled_cabbage"]);
            Assert.Equal("item.beef_yahni", k.NextEntry(MealSlot.Morning).DishId);
        }

        [Fact]
        public void KilerPlani_SinifDokumuVeGun_SaklamaHedefiIsCikarir()
        {
            var x = Make();
            x.House.AddItem("item.bread", 10); x.House.AddItem("item.pastirma", 6); x.House.AddItem("item.pickled_cabbage", 5); x.House.AddItem("item.apple", 8);
            var lines = x.Life.Kitchen.Pantry(4, out float days);
            Assert.True(days > 0.5f);
            Assert.Contains(lines, l => l.ClassKey == "cook.pantry.grain" && l.Count == 10);
            Assert.Contains(lines, l => l.ClassKey == "cook.pantry.meat" && l.Count == 6);
            Assert.Contains(lines, l => l.ClassKey == "cook.pantry.pickle" && l.Count == 5);
            Assert.Contains(lines, l => l.ClassKey == "cook.pantry.fresh");

            var barrel = x.Place("item.salting_barrel", 5, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 6);
            x.Life.Kitchen.SetTarget("item.pickled_cabbage", 10);
            Assert.DoesNotContain(x.Life.Kitchen.Chores(cook), c => c.item == barrel);          // lahana ve tuz yok
            x.House.AddItem("item.cabbage", 3); x.House.AddItem("item.salt", 1);
            Assert.Contains(x.Life.Kitchen.Chores(cook), c => c.item == barrel && c.interaction.Produce[0].ItemId == "item.pickled_cabbage");
        }

        [Fact]
        public void Rutin_SabahBloklari_SofraVarsaOncelik_KayitGidipGelir()
        {
            var x = Make(10.30);
            var table = x.Place("item.table_wood", 10, 5);
            var shelf = x.Place("item.pantry_shelf", 14, 5);
            var basin = x.Place("item.basin_ewer", 6, 5);
            var c = x.Person(1, "Ali", new GridPos(8, 6));
            c.Needs.Set(NeedType.Hunger, 50f); c.Needs.Set(NeedType.Hygiene, 50f);
            Assert.Null(x.Life.Routines.Next(c));                                 // rutini yok
            x.Life.Routines.UseDefaults(c);
            x.House.AddItem("item.apple", 2);
            var first = x.Life.Routines.Next(c);
            Assert.Equal(basin.Id, first.ItemId);                                 // önce yüzünü yıka
            var second = x.Life.Routines.Next(c);
            Assert.Equal(shelf.Id, second.ItemId);                                // sofra kurulu değil: kilerden
            Assert.Null(x.Life.Routines.Next(c));                                 // sabah rutini bitti

            x.House.AddItem("item.soup_cabbage", 2);
            x.Life.Meals.ServeFromInventory(c, table, "item.soup_cabbage", 2);
            x.Life.Routines.Set(c, 0, new[] { "meal" });
            Assert.Equal(table.Id, x.Life.Routines.Next(c).ItemId);               // sofra kuruluysa önce sofra
            var saved = x.Life.Routines.Export();
            x.Life.Routines.Import(saved);
            Assert.Equal(new[] { "meal" }, x.Life.Routines.Of(c, 0));
            Assert.Equal(Routines.DefaultEvening, x.Life.Routines.Of(c, 1));
        }

        // ---------- koku, kedi, birlikte iş, boşta ----------

        [Fact]
        public void Koku_IyiYemekOvguAlir_YanikYemekMahcupEder()
        {
            var x = Make();
            var pot = x.Place(Pot, 5, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(5, 6));
            var near = x.Person(2, "Bora", new GridPos(7, 6));
            var far = x.Person(3, "Uzak", new GridPos(30, 30));
            near.Needs.Set(NeedType.Hunger, 40f);
            var dish = x.Db.Dishes.Get("item.beef_yahni");
            x.Life.Social.OnCooked(cook, pot, dish, new DishScoreBreakdown { Ingredients = 30, Skill = 25, Mastery = 5, Equipment = 6, Kitchen = 5, Cook = 8 }, new List<SpiceDef>());
            Assert.True(near.Mood.Has("cook.smell_good"));
            Assert.True(near.Mood.Has("cook.mouth_waters"));                      // karnı aç: "Ağzı sulandı"
            Assert.False(far.Mood.Has("cook.smell_good"));
            Assert.True(cook.Mood.Has("cook.praised"));
            Assert.Contains(x.Bubbles, b => b.StartsWith("Bora:talk.sg"));
            Assert.Contains(x.Bubbles, b => b.StartsWith("Ayşe:talk.sr"));

            var cook2 = x.Person(4, "Cem", new GridPos(5, 7));
            x.Life.Social.OnCooked(cook2, pot, dish, new DishScoreBreakdown { Ingredients = 15, Skill = 10, Burnt = true }, new List<SpiceDef>());
            Assert.True(near.Mood.Has("cook.smell_burnt"));
            Assert.True(cook2.Mood.Has("cook.embarrassed"));
        }

        [Fact]
        public void Kedi_BakanYokkenBalikCalar_TekPorsiyon_GorenlerSasirir()
        {
            var x = Make();
            var animals = new AnimalSystem(x.World, x.Db, seed: 3);
            x.Life.Attach(animals);
            var table = x.Place("item.table_wood", 10, 5);
            var cook = x.Person(1, "Ayşe", new GridPos(14, 6));
            var kid = x.Person(2, "Çocuk", new GridPos(14, 7), 15);
            var cat = animals.Restore(1, x.Db.Animals.Get("animal.cat"), new GridPos(9, 9), 0, 20f, 30, 0, 0);
            cat.Fullness = 0.2f;
            x.House.AddItem("item.fish_fry", 3);
            x.Life.Meals.ServeFromInventory(cook, table, "item.fish_fry", 3);
            bool stole = false;
            x.Life.PetScene += (a, t, kind) => { if (kind == "steal") stole = true; };

            cook.Position = new GridPos(10, 6);                                   // masanın başında biri var
            x.Life.Social.Tick(0.01);
            Assert.False(stole);
            cook.Position = new GridPos(14, 6);
            x.Life.Social.Tick(0.01);
            Assert.True(stole);
            Assert.Equal(2, x.Life.Meals.ServingAt(table.Id).Portions);           // en çok tek porsiyon
            Assert.True(kid.Mood.Has("cook.cat_laugh"));
            Assert.True(cook.Mood.Has("cook.cat_ate"));
            Assert.True(x.Life.Oddities.Has(OddityIds.CatThief));
            x.Life.Social.Tick(0.01);
            Assert.Equal(2, x.Life.Meals.ServingAt(table.Id).Portions);           // tok kedi çalmaz
        }

        [Fact]
        public void BirlikteIs_SureKisalir_Konusurlar_UstaOgretir()
        {
            var x = Make();
            var family = new FamilyService(x.Pop, x.House, x.Db);
            x.Life.Attach(family);
            var pot = x.Place(Pot, 5, 5);
            var cook = x.Person(1, "Usta", new GridPos(5, 6)); Level(cook, SkillType.Cooking, 8);
            var helper = x.Person(2, "Çırak", new GridPos(7, 6)); Level(helper, SkillType.Cooking, 2);
            foreach (var r in x.Runners.Values) r.Social = family;
            var yahni = x.Cook(pot, "item.beef_yahni");
            x.House.AddItem("item.meat", 2); x.House.AddItem("item.onion", 2); x.House.AddItem("item.carrot", 1);
            Assert.NotNull(family.CanDo(helper, cook, SocialAction.HelpWork));    // çalışmıyor: yardım edilecek iş yok
            x.Runners[1].Command(Job.UseItem(pot.Id, yahni.Id));
            x.Runners[1].Tick(0.002);
            Assert.Null(family.CanDo(helper, cook, SocialAction.HelpWork));
            double xpBefore = helper.Skills.GetXp(SkillType.Cooking);
            x.Runners[2].Command(Job.Social(cook.Id, SocialAction.HelpWork));
            double solo = x.Runners[1].SpeedFactor(yahni, pot);   // yardımcı gelmeden önceki hız
            for (int k = 0; k < 3000 && !(x.Runners[1].IsIdle && x.Runners[2].IsIdle); k++)
            {
                x.Clock.Restore(x.Clock.TotalDays + 0.0005, 1);
                x.Runners[1].Tick(0.0005); x.Runners[2].Tick(0.0005);
                x.Life.Social.Tick(0.0005);
                if (k == 60) Assert.True(x.Life.Social.HasHelper(cook), "yardım başlamadı: " + x.Runners[2].LastFailure);
                if (k == 60) Assert.True(x.Runners[1].SpeedFactor(yahni, pot) > solo * 1.25, "yardımcıyla iş hızlanmalı");
            }
            Assert.True(x.Runners[1].IsIdle, "aşçı bitirmedi");
            Assert.True(x.Runners[2].IsIdle, "yardımcı bitirmedi");
            Assert.False(x.Life.Social.HasHelper(cook));
            Assert.Contains(x.Bubbles, b => b.Contains(":talk.") && b.EndsWith(".a"));
            Assert.Contains(x.Bubbles, b => b.Contains(":talk.") && b.EndsWith(".b"));
            Assert.True(helper.Skills.GetXp(SkillType.Cooking) > xpBefore);       // usta öğretir
            Assert.True(family.Affection(cook.Id, helper.Id) > 0);
            Assert.True(helper.Mood.Has("cook.worked_together"));
            Assert.True(solo > 0);
        }

        [Fact]
        public void Bosta_MeTime_CanSikintisi_HuyFarki_CocukTaklidi()
        {
            var x = Make();
            var bench = x.Place(Pot, 5, 5);
            var worker = x.Person(1, "Ayşe", new GridPos(5, 6));
            var diligent = x.Person(2, "Çalışkan", new GridPos(20, 20));
            diligent.SetTraits(new[] { SocialLife.Diligent });
            var placid = x.Person(3, "Ağırkanlı", new GridPos(22, 20));
            placid.SetTraits(new[] { SocialLife.Placid });
            var acts = new List<string>();
            x.Life.IdleAct += (c, a) => acts.Add(c.Name + ":" + a);

            x.Life.Social.Tick(0.3);
            Assert.True(diligent.Mood.Has("cook.bored"));                         // Çalışkan iki kat çabuk sıkılır
            Assert.False(worker.Mood.Has("cook.bored"));
            x.Life.Social.Tick(0.25);
            Assert.True(worker.Mood.Has("cook.bored"));
            Assert.True(placid.Mood.Has("cook.at_ease"));                         // Ağırkanlı sıkılmaz
            Assert.False(placid.Mood.Has("cook.bored"));
            x.Life.Social.Tick(0.5);
            Assert.True(worker.Mood.Has("cook.idle"));                            // "Aylaklıktan bunaldı"
            for (int k = 0; k < 10; k++) { x.Clock.Restore(x.Clock.TotalDays + 0.05, 1); x.Life.Social.Tick(0.05); }
            Assert.NotEmpty(acts);

            // Uzun işten sonra kısa mola: "Me time" (60c §3).
            var fresh = x.Person(4, "Bora", new GridPos(5, 7)); Level(fresh, SkillType.Cooking, 5);
            x.House.AddItem("item.cabbage", 2); x.House.AddItem("item.onion", 1);
            var soup = x.Cook(bench, "item.soup_cabbage");
            x.Runners[4].Command(Job.UseItem(bench.Id, soup.Id));
            x.Runners[4].Tick(0.002);
            x.Life.Social.Tick(SocialLife.MeTimeWork + 0.01);
            x.Runners[4].Cancel();
            x.Life.Social.Tick(SocialLife.MeTimeRest + 0.01);
            Assert.True(fresh.Mood.Has("cook.me_time"));

            // Çocuk, işini izlediği yetişkini taklit eder (60b §5.6).
            var kid = x.Person(5, "Çocuk", new GridPos(6, 7), 15);
            x.Life.Social.OnAdultWorking(fresh, SkillType.Cooking);
            x.Life.Social.Tick(0.001);
            Assert.Equal(1, x.Life.Social.Imitations(kid, SkillType.Cooking));
        }

        // ---------- piknik, davet, tezgâh ----------

        [Fact]
        public void Piknik_SepetVeYiyecekIster_YakinlikVeKeyif_EslereBasBasa()
        {
            var x = Make(10.5);
            var family = new FamilyService(x.Pop, x.House, x.Db);
            x.Life.Attach(family);
            x.Life.Outings.BadWeather = () => false;
            var a = x.Person(1, "Ayşe", new GridPos(5, 6));
            var b = x.Person(2, "Bora", new GridPos(6, 6));
            Assert.NotNull(family.CanDo(a, b, SocialAction.Picnic));              // sepet yok
            x.House.AddItem(Outings.BasketId, 1);
            Assert.NotNull(family.CanDo(a, b, SocialAction.Picnic));              // yiyecek yok
            x.House.AddItem("item.bread", 2); x.House.AddItem("item.beef_yahni", 2, ItemQuality.Fine);
            Assert.Null(family.CanDo(a, b, SocialAction.Picnic));
            a.Needs.Set(NeedType.Fun, 20f);
            int before = family.Affection(a.Id, b.Id);
            family.Apply(a, b, SocialAction.Picnic);
            Assert.True(family.Affection(a.Id, b.Id) >= before + 6);
            Assert.True(a.Needs.Get(NeedType.Fun) > 40f);
            Assert.True(a.Mood.Has("cook.picnic") && b.Mood.Has("cook.picnic"));
            Assert.False(a.Mood.Has("cook.together"));                            // eş ya da gönüllü değil
            Assert.Equal(0, x.House.Count("item.beef_yahni"));
            Assert.Equal(1, x.House.Count(Outings.BasketId));                     // sepet harcanmaz
            Assert.True(family.Duration(SocialAction.Picnic) > family.Duration(SocialAction.EveningWalk));
            Assert.False(x.Life.Outings.IsRomantic(a, b));

            // Akşam yürüyüşü yalnızca akşam; ateş başı yanan ocak ister.
            Assert.NotNull(family.CanDo(a, b, SocialAction.EveningWalk));
            x.Clock.Restore(10.75, 1);
            Assert.Null(family.CanDo(a, b, SocialAction.EveningWalk));
            Assert.NotNull(family.CanDo(a, b, SocialAction.Fireside));
        }

        [Fact]
        public void YemegeDavet_TekMisafirBirPorsiyon_PazarTezgahiPorsiyonlaSatar()
        {
            var x = Make(10.5);
            var family = new FamilyService(x.Pop, x.House, x.Db);
            x.Life.Attach(family);
            var host = x.Person(1, "Ayşe", new GridPos(5, 6));
            var guest = new Character(50, "Kemal", new GridPos(6, 6)) { IsVillager = true, MoodEnabled = true };
            guest.SetAge(45);
            x.Pop.Add(guest);
            var table = x.Place("item.table_wood", 10, 5);
            family.SetAffection(host.Id, guest.Id, 30);
            Assert.NotNull(family.CanDo(host, guest, SocialAction.InviteDinner)); // sofrada yemek yok
            x.House.AddItem("item.beef_yahni", 3, ItemQuality.Superior);
            x.Life.Meals.ServeFromInventory(host, table, "item.beef_yahni", 3);
            Assert.Null(family.CanDo(host, guest, SocialAction.InviteDinner));
            int rep = x.Life.Upbringing.Reputation.Points;
            family.Apply(host, guest, SocialAction.InviteDinner);
            Assert.Equal(2, x.Life.Meals.ServingAt(table.Id).Portions);
            Assert.True(family.Affection(host.Id, guest.Id) > 30);
            Assert.True(x.Life.Upbringing.Reputation.Points > rep);               // has yemek itibar kazandırır
            Assert.NotNull(family.CanDo(host, guest, SocialAction.InviteDinner)); // aynı gün ikinci kez olmaz

            // Pazar tezgâhı (60b §5.7).
            var o = x.Life.Outings;
            o.MarketDay = () => false;
            Assert.NotNull(o.StallBlocker(host));
            o.MarketDay = () => true;
            x.House.AddItem("item.soup_cabbage", 4); x.House.AddItem("item.fish_fry", 3, ItemQuality.Fine);
            Assert.Contains(o.Sellable(), s => s.dish == "item.fish_fry" && s.count == 3);
            int akce = x.House.Akce;
            Assert.True(o.OpenStall(host, new[] { ("item.soup_cabbage", 4), ("item.fish_fry", 3) }));
            Assert.Equal(0, x.House.Count("item.fish_fry"));
            Assert.True(o.StallOpen);
            var result = o.CloseStall(host);
            Assert.Equal(7, result.Sold + result.Unsold);
            Assert.True(result.Sold > 0);
            Assert.True(x.House.Akce > akce);
            Assert.NotNull(result.Favorite);
            Assert.Equal(result.Favorite, o.VillageFavorite);                     // "köyün sevdiği"
            var soup = x.Db.Items.Get("item.soup_cabbage");
            Assert.True(Outings.PortionPrice(soup, ItemQuality.Superior, 3, true) > Outings.PortionPrice(soup, ItemQuality.Common, 1, false));
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_YemekDurumuGidipGelir_EskiKayittaBosBaslar()
        {
            var x = Make();
            var table = x.Place("item.table_wood", 10, 5);
            var a = x.Person(1, "Ayşe", new GridPos(5, 6));
            x.House.AddItem("item.soup_cabbage", 3);
            x.Life.Meals.ServeFromInventory(a, table, "item.soup_cabbage", 3);
            x.Life.Recipes.Learn(a, "item.fish_steam", "villager");
            x.Life.Kitchen.Add(MealSlot.Evening, "item.soup_cabbage", 2, new[] { "item.mint_ground" });
            x.Life.Routines.UseDefaults(a);
            var data = x.Life.Export();
            Assert.NotNull(data.cooking);
            string json = JsonSerializer.Serialize(data.cooking, new JsonSerializerOptions { IncludeFields = true });
            var back = JsonSerializer.Deserialize<Thatchwick.Core.Save.CookingSave>(json, new JsonSerializerOptions { IncludeFields = true });

            var y = Make();
            var table2 = y.Place("item.table_wood", 10, 5);
            Assert.Equal(table.Id, table2.Id);
            var a2 = y.Person(1, "Ayşe", new GridPos(5, 6));
            var fresh = y.Life.Export();
            fresh.cooking = back;
            y.Life.Restore(fresh);
            Assert.Equal(3, y.Life.Meals.ServingAt(table2.Id).Portions);
            Assert.True(y.Life.Recipes.HasLearned(a2, "item.fish_steam"));
            Assert.Equal(2, y.Life.Kitchen.Plan(MealSlot.Evening)[0].Batch);
            Assert.Equal("item.mint_ground", y.Life.Kitchen.Plan(MealSlot.Evening)[0].Spices.Single());
            Assert.True(y.Life.Routines.HasRoutine(a2));

            fresh.cooking = null;                                                 // eski kayıt
            y.Life.Restore(fresh);
            Assert.Null(y.Life.Meals.ServingAt(table2.Id));
            Assert.False(y.Life.Routines.HasRoutine(a2));
        }
    }
}
