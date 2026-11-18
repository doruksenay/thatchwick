using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Yeni beceriler (17.11.2026): hayvancılık, binicilik, balıkçılık, saraçlık, dokumacılık, ticaret, otacılık;
    // yemek yapan Yemek XP'si alır.
    public class BecerilerTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static Character Person(int level = 1, SkillType skill = SkillType.Farming, int id = 1, GridPos? at = null)
        {
            var c = new Character(id, "Doruk", at ?? new GridPos(0, 0), true);
            c.SetAge(45);
            if (level > 1) c.GainSkillXp(skill, SkillRules.XpForLevel(level));
            Assert.Equal(level, c.Skills.GetLevel(skill));
            return c;
        }

        private static InteractionDef Inter(ContentDatabase db, string item, string id) => db.Items.Get(item).Interactions.Single(i => i.Id == id);

        // ---------- genel ----------

        [Fact]
        public void Beceriler_AdlariVeMeslekleriFarkli()
        {
            var all = (SkillType[])Enum.GetValues(typeof(SkillType));
            Assert.Equal(34, all.Length);
            Assert.Equal(all.Length, all.Select(SkillNames.Name).Distinct().Count());
            Assert.Equal(all.Length, all.Select(SkillNames.TradeName).Distinct().Count());
            Assert.True(SkillNames.IsCraft(SkillType.Leatherwork) && SkillNames.IsCraft(SkillType.Weaving) && SkillNames.IsCraft(SkillType.Carpentry));
            var db = Db();
            // Her yeni becerinin onu geliştiren en az bir işi var.
            foreach (var s in new[] { SkillType.AnimalHusbandry, SkillType.Fishing, SkillType.Leatherwork, SkillType.Weaving, SkillType.Herbalism })
                Assert.True(db.Items.All.Any(d => d.Interactions.Any(i => i.Skill == s && i.SkillXp > 0)), s.ToString());
        }

        [Fact]
        public void Yemek_PisirenYemekXpAlir()
        {
            var db = Db();
            foreach (var d in db.Items.All)
                foreach (var i in d.Interactions)
                {
                    string n = i.DisplayName.ToLowerInvariant();
                    if (n.Contains("pişir") || n.Contains("haşla") || n.Contains("kızart") || n.Contains("tüttür") || n.Contains("tuzla"))
                        Assert.True((i.Skill == SkillType.Cooking || i.Skill == SkillType.Baking || i.Skill == SkillType.Pottery) && i.SkillXp > 0, $"{d.Id}.{i.Id} yemek XP'si vermiyor");
                }
            var w = new WorldState(new GridMap(8, 8));
            var h = new Household();
            var pot = w.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(3, 3));
            var cook = Person();
            var runner = new JobRunner(cook, w) { Autonomous = false, Effects = new InteractionEffects(w, h, db) };
            h.AddItem("item.fish_carp");
            runner.Command(Job.UseItem(pot.Id, "cook_fish_carp"));
            for (int k = 0; k < 60 && !runner.IsIdle; k++) runner.Tick(0.01);
            Assert.True(cook.Skills.GetXp(SkillType.Cooking) > 0);
        }

        // ---------- hayvancılık ----------

        private sealed class Farm
        {
            public ContentDatabase Db; public WorldState W; public Household H; public BuildService Svc; public AnimalSystem A; public InteractionEffects Fx;
        }

        private static Farm Barn(string template = "template.barn_room")
        {
            var db = Db();
            var f = new Farm { Db = db, W = new WorldState(new GridMap(24, 24)), H = new Household(20000) };
            f.Svc = new BuildService(f.W, f.H, db);
            Assert.True(new RoomTemplateService(f.Svc, f.W, f.H, db).Build(template, new GridPos(2, 2), out _).Ok);
            f.A = new AnimalSystem(f.W, db, seed: 5);
            f.Fx = new InteractionEffects(f.W, f.H, db, seed: 3) { Animals = f.A };
            return f;
        }

        [Fact]
        public void Hayvancilik_UstaDahaCokSeverDahaCokToplar()
        {
            var f = Barn();
            var stalls = f.W.Items.Where(i => i.Def.Id == "item.stall").ToList();
            var cow = f.Db.Animals.Get("animal.cow");
            var a1 = f.A.Restore(1, cow, new GridPos(3, 4), stalls[0].Id, 100f, 30, 0, 0);
            var a2 = f.A.Restore(2, cow, new GridPos(4, 4), stalls[1].Id, 100f, 30, 0, 0);
            var pet = Inter(f.Db, "item.stall", "pet_animals");
            var novice = Person(1); var master = Person(10, SkillType.AnimalHusbandry, 2);
            f.Fx.Apply(novice, stalls[0], pet);
            f.Fx.Apply(master, stalls[1], pet);
            Assert.True(a2.Friendship > a1.Friendship * 1.4f, $"{a2.Friendship} vs {a1.Friendship}");
            Assert.Equal(SkillType.AnimalHusbandry, pet.Skill);                         // hayvan işi artık çiftçilik değil

            // Toplama: usta her üründen ara sıra bir fazla alır.
            var collect = Inter(f.Db, "item.stall", "collect");
            int noviceTotal = 0, masterTotal = 0;
            for (int k = 0; k < 40; k++)
            {
                AnimalSystem.PutStock(stalls[0], "item.milk", 5);
                int before = f.H.Count("item.milk");
                f.Fx.Apply(novice, stalls[0], collect);
                noviceTotal += f.H.Count("item.milk") - before;
                AnimalSystem.PutStock(stalls[1], "item.milk", 5);
                before = f.H.Count("item.milk");
                f.Fx.Apply(master, stalls[1], collect);
                masterTotal += f.H.Count("item.milk") - before;
            }
            Assert.Equal(200, noviceTotal);
            Assert.True(masterTotal > 230, $"usta {masterTotal}");
        }

        [Fact]
        public void Hayvancilik_MerhemSevmeHakkiniKullanmaz()
        {
            var f = Barn();
            var stall = f.W.Items.First(i => i.Def.Id == "item.stall");
            var a = f.A.Restore(1, f.Db.Animals.Get("animal.cow"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0);
            var salve = Inter(f.Db, "item.stall", "salve");
            var c = Person();
            Assert.Contains("merhem", f.Fx.CanStart(c, stall, salve).ToLowerInvariant());
            f.H.AddItem("item.animal_salve");
            f.Fx.Apply(c, stall, salve);
            Assert.True(a.Friendship >= 12f);
            Assert.Equal(0.0, a.PetCooldownDays);
        }

        [Fact]
        public void Hayvancilik_YerVarsaYavrular_UstaylaDahaSik()
        {
            var f = Barn("template.coop_room");                                       // 3 folluk x 2 = 6 yer
            var hen = f.Db.Animals.Get("animal.chicken");
            var nests = f.W.Items.Where(i => i.Def.Id == "item.nest_box").ToList();
            f.A.Restore(1, hen, new GridPos(3, 3), nests[0].Id, 100f, 30, 0, 0, 60f);
            f.A.Restore(2, hen, new GridPos(4, 3), nests[0].Id, 100f, 30, 0, 0, 60f);
            Assert.True(f.A.BreedChance(10) > f.A.BreedChance(1) * 3);
            int level = 1;
            f.A.HusbandryLevel = () => level;
            f.A.DiseaseEnabled = false;                                              // bu test yalnızca yavrulamayı ölçer
            var born = new List<Animal>();
            f.A.Born += (m, b) => born.Add(b);
            for (int d = 0; d < 200 && f.A.Animals.Count < 6; d++) f.A.OnNewDay(Season.Spring);
            Assert.Equal(6, f.A.Animals.Count);                                      // yer dolunca durur
            Assert.All(born, b => Assert.False(b.IsMature));
            for (int d = 0; d < 30; d++) f.A.OnNewDay(Season.Spring);
            Assert.Equal(6, f.A.Animals.Count);

            // Dost olmayan çift yavrulamaz.
            var g = Barn("template.coop_room");
            var gn = g.W.Items.First(i => i.Def.Id == "item.nest_box");
            g.A.Restore(1, hen, new GridPos(3, 3), gn.Id, 100f, 30, 0, 0, 5f);
            g.A.Restore(2, hen, new GridPos(4, 3), gn.Id, 100f, 30, 0, 0, 5f);
            for (int d = 0; d < 100; d++) g.A.OnNewDay(Season.Spring);
            Assert.Equal(2, g.A.Animals.Count);
        }

        // ---------- binicilik ----------

        [Fact]
        public void Binicilik_SeviyeHiziArtirirSurusXpVerir()
        {
            var f = Barn("template.stable_room");
            var stall = f.W.Items.Single(i => i.Def.Id == "item.horse_stall");
            var horse = f.A.Restore(5, f.Db.Animals.Get("animal.horse"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0, 50f, 0, "item.saddle");
            var rider = Person(1, SkillType.Riding, 7, new GridPos(3, 4));
            f.A.FindCharacter = id => id == rider.Id ? rider : null;
            f.A.RiderPosition = id => rider.Position;
            Assert.True(f.A.Mount(rider.Id, horse));
            double slow = f.A.RideSpeed(rider.Id);
            for (int x = 4; x < 20; x++) { rider.Position = new GridPos(x, 12); f.A.Tick(0.002); }
            Assert.True(rider.Skills.GetXp(SkillType.Riding) > 5);
            rider.GainSkillXp(SkillType.Riding, SkillRules.XpForLevel(10));
            Assert.Equal(slow * 1.27, f.A.RideSpeed(rider.Id), 3);

            var care = new AnimalCare(f.A, f.H);
            var groom = Person(1, SkillType.Farming, 8);
            care.Apply(groom, horse.Id, AnimalAction.Brush);
            Assert.True(groom.Skills.GetXp(SkillType.Riding) > 0 && groom.Skills.GetXp(SkillType.AnimalHusbandry) > 0);
        }

        // ---------- balıkçılık ----------

        [Fact]
        public void Balikcilik_MevsimeVeUstaligaGoreBalik()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var season = Season.Winter;
            var fx = new InteractionEffects(w, h, db, seed: 9) { Season = () => season };
            var pond = w.PlaceItem(db.Items.Get("item.pond"), new GridPos(4, 4));
            var fish = Inter(db, "item.pond", "fish");
            var novice = Person();
            for (int k = 0; k < 200; k++) fx.Apply(novice, pond, fish);
            Assert.Equal(0, h.Count("item.fish_carp"));                              // kışın sazan yok
            Assert.True(h.Count("item.fish_crayfish") > 0);
            Assert.Equal(0, h.Count("item.fish_trout"));                             // alabalık 2. seviye ister
            Assert.Equal(0, h.Count("item.fish_pike"));                              // turna 4. seviye ister
            Assert.Equal(0, h.Count("item.fish_eel"));
            Assert.Equal(SkillType.Fishing, fish.Skill);

            season = Season.Summer;
            var master = Person(8, SkillType.Fishing, 2);
            for (int k = 0; k < 200; k++) fx.Apply(master, pond, fish);
            Assert.True(h.Count("item.fish_carp") > 0, "sazan " + h.Count("item.fish_carp"));
            Assert.True(h.Count("item.fish_pike") > 0, "turna " + h.Count("item.fish_pike"));
            Assert.True(h.Count("item.fish_eel") > 0, "yılan " + h.Count("item.fish_eel"));                               // yazın, 6. seviyede yılan balığı
            Assert.True(db.Items.Get("item.pond").WaterSource);                      // gölet kanala su verir
        }

        [Fact]
        public void Balikcilik_SepetHerSabahBalikVerir_TuzlanipTutsulenir()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            w.PlaceItem(db.Items.Get("item.pond"), new GridPos(4, 4));
            var trap = w.PlaceItem(db.Items.Get("item.fish_trap"), new GridPos(7, 4));
            var lone = w.PlaceItem(db.Items.Get("item.fish_trap"), new GridPos(10, 10));
            var auto = new FarmAutomation(w, h);
            int total = 0;
            for (int d = 0; d < 10; d++) total += auto.OnNewDay(Season.Spring).Sum(k => k.Value);
            Assert.Equal(10, total);                                                 // yalnızca göletteki sepet
            Assert.Contains(db.Items.Get("item.salting_barrel").Interactions, i => i.Produce.Any(p => p.ItemId == "item.salted_fish"));
            Assert.Contains(db.Items.Get("item.smokehouse").Interactions, i => i.Produce.Any(p => p.ItemId == "item.smoked_fish"));
            Assert.Equal(0f, db.Items.Get("item.smoked_fish").SpoilDays);
        }

        // ---------- zanaatler: saraçlık, dokumacılık; marangozluk ----------

        [Fact]
        public void Zanaat_SaraclikVeDokumacilikAyriBeceri()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var fx = new InteractionEffects(w, h, db);
            var c = Person();
            var wheel = w.PlaceItem(db.Items.Get("item.spinning_wheel"), new GridPos(2, 2));
            h.AddItem("item.wool", 3);
            var spin = wheel.Def.Interactions.Single(i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.yarn"));
            Assert.Equal(SkillType.Weaving, spin.Skill);
            Assert.Null(fx.CanStart(c, wheel, spin));
            fx.Apply(c, wheel, spin);
            c.GainSkillXp(spin.Skill.Value, spin.SkillXp);
            Assert.Equal(2, h.Count("item.yarn"));
            Assert.True(c.Skills.GetXp(SkillType.Weaving) > 0);

            var bench = db.Items.Get("item.saddler_bench");
            Assert.All(bench.Interactions.Where(i => i.IsCraft), i => Assert.Equal(SkillType.Leatherwork, i.Skill));
            Assert.All(db.Items.Get("item.tanning_rack").Interactions.Where(i => i.IsCraft), i => Assert.Equal(SkillType.Leatherwork, i.Skill));
            var loom = db.Items.Get("item.loom");
            Assert.Contains(loom.Interactions, i => i.Produce.Any(p => p.ItemId == "item.rug_woven_large") && i.MinSkillLevel >= 5);
            // Mobilya marangozlukla yapılmaya devam eder.
            Assert.True(db.Items.Get("item.workbench").Interactions.Count(i => i.IsCraft && i.Skill == SkillType.Carpentry) >= 10);
        }

        // ---------- ticaret ----------

        [Fact]
        public void Ticaret_UstaDahaPahaliSatarXpKazanir()
        {
            var db = Db();
            var h = new Household(1000);
            var market = new Market(db, h, null, null, seed: 2);
            var trader = Person();
            market.Trader = () => trader;
            int noviceSell = market.SellPrice("item.saddle"), noviceBuy = market.BuyPrice("item.saddle");
            h.AddItem("item.wool", 5);
            Assert.True(market.Sell("item.wool", 5) > 0);
            Assert.True(trader.Skills.GetXp(SkillType.Trade) > 0);
            trader.GainSkillXp(SkillType.Trade, SkillRules.XpForLevel(10));
            Assert.True(market.TradeSellFactor > 1.17);
            Assert.True(market.SellPrice("item.saddle") > noviceSell);
            Assert.True(market.BuyPrice("item.saddle") < noviceBuy);
        }

        // ---------- otacılık ----------

        [Fact]
        public void Otacilik_IksirYapilirIcilinceEtkiEder()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var fx = new InteractionEffects(w, h, db);
            var alembic = w.PlaceItem(db.Items.Get("item.alembic"), new GridPos(2, 2));
            var brew = alembic.Def.Interactions.Single(i => i.Produce.Any(p => p.ItemId == "item.potion_energy"));
            Assert.Equal(SkillType.Herbalism, brew.Skill);
            h.AddItem("item.herb", 2); h.AddItem("item.honey", 1);
            var c = Person();
            Assert.Null(fx.CanStart(c, alembic, brew));
            fx.Apply(c, alembic, brew);
            Assert.Equal(1, h.Count("item.potion_energy"));

            // İksir rafında içilir: enerji ve "Dinç" (işler %15 hızlı).
            c.MoodEnabled = true;
            c.Needs.Set(NeedType.Energy, 20f);
            var shelf = w.PlaceItem(db.Items.Get("item.potion_shelf"), new GridPos(6, 6));
            var runner = new JobRunner(c, w) { Autonomous = false, Effects = fx };
            runner.Command(Job.UseItem(shelf.Id, "drink_energy"));
            for (int k = 0; k < 40 && !runner.IsIdle; k++) runner.Tick(0.005);
            Assert.True(c.Needs.Get(NeedType.Energy) >= 69f);
            Assert.True(c.Mood.Has(MoodCatalog.PotionVigor));
            Assert.True(c.WorkSpeed(SkillType.Weaving) > 1.1);
            Assert.Equal(0, h.Count("item.potion_energy"));

            // Bereket iksiri ekin toprağını besler; zihin iksiri pazardan alınmaz.
            Assert.Contains(db.Items.Get("item.crop_turnip").Interactions, i => i.Id == "growth_potion" && i.Fertilize > 0f);
            Assert.False(new Market(db, h).IsBuyable("item.potion_focus"));
            Assert.True(MoodCatalog.Default.TryGet(MoodCatalog.PotionFocus, out var focus) && focus.AllXp > 0.25f);
        }
    }
}
