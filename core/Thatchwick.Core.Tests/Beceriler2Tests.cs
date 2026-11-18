using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // İkinci dalga beceriler (17.11.2026): mühendislik, geri kazanım, taş ustalığı, madencilik, demircilik, çömlekçilik,
    // biracılık, fırıncılık, mumculuk, bahçıvanlık, toplayıcılık, avcılık, hava bilgisi, toprak bakımı, ebeveynlik,
    // öğretmenlik, okuryazarlık, gönül kazanma, müzik, kâhyalık, arabacılık, ev düzenleme, hekimlik.
    public class Beceriler2Tests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static Character Person(int id = 1, int age = 45, SkillType? skill = null, int level = 1)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true);
            c.SetAge(age);
            if (skill.HasValue && level > 1) c.GainSkillXp(skill.Value, SkillRules.XpForLevel(level));
            return c;
        }

        private static readonly SkillType[] Wave2 =
        {
            SkillType.Engineering, SkillType.Salvage, SkillType.Masonry, SkillType.Mining, SkillType.Smithing, SkillType.Pottery,
            SkillType.Brewing, SkillType.Baking, SkillType.Chandlery, SkillType.Horticulture, SkillType.Foraging, SkillType.Hunting,
            SkillType.WeatherLore, SkillType.SoilCare, SkillType.Parenting, SkillType.Teaching, SkillType.Literacy, SkillType.Charm,
            SkillType.Music, SkillType.Stewardship, SkillType.Carting, SkillType.Decorating, SkillType.Medicine
        };

        [Fact]
        public void IcerikVeIsler_HerBeceriyiGelistirenBirSeyVar()
        {
            var db = Db();
            // XP'si eşya etkileşiminden ya da tariften gelenler.
            var fromItems = new[] { SkillType.Engineering, SkillType.Mining, SkillType.Smithing, SkillType.Pottery, SkillType.Brewing,
                SkillType.Baking, SkillType.Chandlery, SkillType.Horticulture, SkillType.Foraging, SkillType.Hunting, SkillType.WeatherLore,
                SkillType.SoilCare, SkillType.Parenting, SkillType.Teaching, SkillType.Literacy, SkillType.Music, SkillType.Medicine };
            foreach (var s in fromItems)
                Assert.True(db.Items.All.Any(d => d.Interactions.Any(i => i.Skill == s && i.SkillXp > 0)), s.ToString());
            // Geri kalanlar kodla: geri kazanım (söküm), taş ustalığı (yol onarımı), gönül (sohbet), kâhyalık (görev),
            // arabacılık (pazar), ev düzenleme (eşya yerleştirme). Hepsinin adı ve meslek adı var; dallar 116 düzenine uyar.
            foreach (var s in Wave2)
            {
                Assert.NotEqual(s.ToString(), SkillNames.Name(s));
                Assert.Equal(SkillNames.BranchCount(s), db.Branches.All.Count(b => b.Skill == s));
            }
            Assert.True(db.Tools.Contains("pickaxe"));
        }

        [Fact]
        public void EkUrun_UstaToplayiciVeMadenciDahaCokBulur()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h1 = new Household(); var h2 = new Household();
            var quarry = w.PlaceItem(db.Items.Get("item.quarry"), new GridPos(3, 3));
            var mine = quarry.Def.Interactions.Single(i => i.Id == "mine");
            Assert.Equal(SkillType.Mining, mine.Skill);
            var fx1 = new InteractionEffects(w, h1, db, seed: 4);
            var fx2 = new InteractionEffects(w, h2, db, seed: 4);
            var novice = Person(1); var master = Person(2, 45, SkillType.Mining, 10);
            for (int k = 0; k < 100; k++) { fx1.Apply(novice, quarry, mine); fx2.Apply(master, quarry, mine); }
            Assert.Equal(200, h1.Count("item.stone"));
            Assert.True(h2.Count("item.stone") > 220, $"usta {h2.Count("item.stone")}");
            Assert.Equal(0, h1.Count("item.iron_ore"));                              // cevher 2. seviye ister
            Assert.True(h2.Count("item.iron_ore") > 0);
            Assert.Equal(0.04, SkillNames.ExtraYieldPerLevel(SkillType.Foraging), 6);
            Assert.Equal(SkillType.Foraging, db.Items.Get("item.forage_mushroom").Interactions.First().Skill);
            Assert.Equal(SkillType.Horticulture, db.Items.Get("item.tree_apple").Interactions.Single(i => i.Id == "pick").Skill);
        }

        [Fact]
        public void Okuryazarlik_ButunBecerilerdeFazlaXp()
        {
            var plain = Person(1);
            var reader = Person(2, 45, SkillType.Literacy, 10);
            plain.GainSkillXp(SkillType.Farming, 100);
            reader.GainSkillXp(SkillType.Farming, 100);
            Assert.Equal(100.0, plain.Skills.GetXp(SkillType.Farming), 3);
            Assert.Equal(118.0, reader.Skills.GetXp(SkillType.Farming), 3);
        }

        [Fact]
        public void Muhendislik_DuzenekDahaCokIslerDahaUzagaGucVerir()
        {
            var db = Db();
            var w = new WorldState(new GridMap(16, 16));
            var h = new Household();
            var auto = new FarmAutomation(w, h);
            w.PlaceItem(db.Items.Get("item.well"), new GridPos(2, 2));
            w.PlaceItem(db.Items.Get("item.canal"), new GridPos(3, 2));
            w.PlaceItem(db.Items.Get("item.water_wheel"), new GridPos(4, 2));
            var far = w.PlaceItem(db.Items.Get("item.millstone"), new GridPos(8, 2));   // çarktan 3 karo
            Assert.False(auto.IsPowered(far));
            int level = 1;
            auto.EngineeringLevel = () => level;
            level = 5;
            Assert.True(auto.IsPowered(far));
            h.AddItem("item.barley", 20);
            auto.OnNewDay();
            Assert.Equal(20 - auto.Batches, h.Count("item.barley"));
            Assert.Equal(5, auto.Batches);
            Assert.Contains(db.Items.Get("item.water_wheel").Interactions, i => i.Id == "maintain" && i.Skill == SkillType.Engineering);
        }

        [Fact]
        public void GeriKazanim_SokulenEsyadanMalzemeDoner_GeriAlinir()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household(100);
            var svc = new BuildService(w, h, db);
            h.AddItem("item.stool_wood");                                            // tarif: 2 tahta
            Assert.True(svc.PlaceFromInventory("item.stool_wood", new GridPos(3, 3), 0, out var stool).Ok);
            Assert.True(svc.SellPlaced(stool.Id).Ok);
            Assert.Equal(0, h.Count("item.plank"));                                  // geri kazanım yokken yalnızca akçe

            int level = 10;
            svc.SalvageLevel = () => level;
            List<KeyValuePair<string, int>> got = null;
            svc.Salvaged += l => got = l;
            h.AddItem("item.chest_wood");
            Assert.True(svc.PlaceFromInventory("item.chest_wood", new GridPos(5, 5), 0, out var chest).Ok);
            int akce = h.Akce;
            Assert.True(svc.SellPlaced(chest.Id).Ok);
            Assert.NotNull(got);
            Assert.True(h.Count("item.plank") > 0);
            Assert.True(h.Akce > akce);                                              // akçe de gelir
            Assert.True(svc.Undo().Ok);
            Assert.Equal(0, h.Count("item.plank"));
            Assert.Equal(0.55, BuildService.SalvageFraction(10), 6);
        }

        [Fact]
        public void TasUstaligi_OnarimdaTasAzXpVerir()
        {
            var w = new WorldState(new GridMap(10, 10));
            var h = new Household();
            var t = new GridPos(4, 4);
            w.Map.SetRoad(t, RoadType.Paved, 0.2f);
            var work = new FarmWork(w) { House = h };
            var novice = Person(1); var mason = Person(2, 45, SkillType.Masonry, 5);
            Assert.Equal(2, FarmWork.RepairStoneFor(novice, RoadType.Paved));
            Assert.Equal(1, FarmWork.RepairStoneFor(mason, RoadType.Paved));
            h.AddItem("item.stone", 1);
            Assert.NotNull(work.CanDo(novice, t, TileAction.Repair));
            Assert.Null(work.CanDo(mason, t, TileAction.Repair));
            double before = mason.Skills.GetXp(SkillType.Masonry);
            work.Apply(mason, t, TileAction.Repair);
            Assert.True(mason.Skills.GetXp(SkillType.Masonry) > before);
            Assert.Equal(0, h.Count("item.stone"));
        }

        [Fact]
        public void Hane_DemirciHavaBilgisiHekimVeIndirim()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, w, h, pop, clock);
            life.SkillPacing = false;   // bu sınamalar büyük XP'yi tek seferde verir (doygunluk 28)
            var head = Person(1);
            pop.Add(head);
            Assert.Equal(1, life.ForecastDays);
            Assert.Equal(0.0, (double)life.BuildDiscount(), 3);
            head.GainSkillXp(SkillType.WeatherLore, SkillRules.XpForLevel(10));
            head.GainSkillXp(SkillType.Decorating, SkillRules.XpForLevel(10));
            head.GainSkillXp(SkillType.Smithing, SkillRules.XpForLevel(5));
            Assert.Equal(4, life.ForecastDays);
            Assert.Equal(4, life.WeatherAhead(life.ForecastDays).Count);
            Assert.Equal(0.18, (double)life.BuildDiscount(), 3);
            Assert.True(life.BestSkill(SkillType.Smithing) >= LifeSystems.SmithSelfLevel, "smith");

            // Hekim: soğuktan yaralı birini iyileştirir.
            var sick = Person(2);
            pop.Add(sick);
            sick.AmbientTemperature = -20f;
            sick.Needs.Set(NeedType.Warmth, 0f);
            for (int k = 0; k < 10; k++) sick.Tick(0.05);
            Assert.True(sick.FreezeDays > 0, "freeze " + sick.FreezeDays);
            sick.Needs.Set(NeedType.Warmth, 80f);
            head.GainSkillXp(SkillType.Medicine, SkillRules.XpForLevel(5));
            double fd = sick.FreezeDays;
            Assert.True(life.DailyHealing() >= 1, "heal");
            Assert.True(sick.FreezeDays < fd, "less");
        }

        [Fact]
        public void Aile_EbeveynBeslerGonulSohbetiOgretmenDers()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var pop = new Population();
            var family = new FamilyService(pop, h, db);
            var mom = Person(1, 45, SkillType.Parenting, 10);
            var baby = Person(2, 1);
            pop.Add(mom); pop.Add(baby);
            h.AddItem("item.turnip", 3);
            baby.Needs.Set(NeedType.Hunger, 10f);
            Assert.Null(family.CanDo(mom, baby, SocialAction.Feed));
            family.Apply(mom, baby, SocialAction.Feed);
            Assert.True(baby.Needs.Get(NeedType.Hunger) >= 10f + FamilyService.FeedRestore * 1.4f);

            var a = Person(3, 45, SkillType.Charm, 10); var b = Person(4);
            pop.Add(a); pop.Add(b);
            family.Apply(a, b, SocialAction.Chat);
            Assert.True(family.Affection(a.Id, b.Id) > FamilyService.ChatAffection);
            Assert.True(a.Skills.GetXp(SkillType.Charm) > SkillRules.XpForLevel(10));

            // Öğretmen: çocuklar okuryazarlık ve öğretmenin en iyi becerisinden XP alır.
            var teacher = Person(5, 45, SkillType.Teaching, 5);
            teacher.GainSkillXp(SkillType.Cooking, 500);
            var kid = Person(6, 14);
            pop.Add(teacher); pop.Add(kid);
            var fx = new InteractionEffects(w, h, db) { Household = () => pop.Members };
            var slate = w.PlaceItem(db.Items.Get("item.slate"), new GridPos(3, 3));
            var teach = slate.Def.Interactions.Single(i => i.Teach);
            Assert.Null(fx.CanStart(teacher, slate, teach));
            fx.Apply(teacher, slate, teach);
            Assert.True(kid.Skills.GetXp(SkillType.Literacy) > 0);
            Assert.True(kid.Skills.GetXp(SkillType.Cooking) > 0);
            Assert.NotNull(new InteractionEffects(w, h, db) { Household = () => new[] { teacher } }.CanStart(teacher, slate, teach));
        }

        [Fact]
        public void Muzik_KahyalikArabacilik()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var lute = w.PlaceItem(db.Items.Get("item.lute"), new GridPos(4, 4));
            double FunAfter(Character c)
            {
                c.Needs.Set(NeedType.Fun, 0f);
                c.Position = new GridPos(3, 4);
                var r = new JobRunner(c, w) { Autonomous = false };
                r.Command(Job.UseItem(lute.Id, "play_music"));
                for (int k = 0; k < 40 && !r.IsIdle; k++) r.Tick(0.01);
                return c.Needs.Get(NeedType.Fun);
            }
            Assert.True(FunAfter(Person(2, 45, SkillType.Music, 10)) > FunAfter(Person(1)) * 1.4);

            // Arabacılık: depo büyür. Kâhyalık: görev işleri hızlanır.
            var storage = new StorageService(w, h, db);
            int cap = storage.Capacity;
            storage.CartingLevel = () => 10;
            Assert.True(storage.Capacity > cap * 1.25);
            var market = new Market(db, h, null, null, seed: 1);
            var trader = Person(3);
            market.Trader = () => trader;
            h.AddItem("item.wool", 4);
            market.Sell("item.wool", 4);
            Assert.True(trader.Skills.GetXp(SkillType.Carting) > 0);
        }

        [Fact]
        public void Uretim_YeniTariflerVeBagliIsler()
        {
            var db = Db();
            bool Makes(string station, string product, SkillType skill) =>
                db.Items.Get(station).Interactions.Any(i => i.Skill == skill && i.Produce.Any(p => p.ItemId == product));
            Assert.True(Makes("item.forge", "item.iron_bar", SkillType.Smithing));
            Assert.True(Makes("item.forge", "item.nails", SkillType.Smithing));
            Assert.True(Makes("item.workbench", "item.gear", SkillType.Engineering));
            Assert.True(Makes("item.pottery_wheel", "item.storage_jar", SkillType.Pottery));
            Assert.True(Makes("item.brew_barrel", "item.ale", SkillType.Brewing));
            Assert.True(Makes("item.millstone", "item.flour", SkillType.Baking));
            Assert.True(Makes("item.stone_oven", "item.bread", SkillType.Baking));
            Assert.True(Makes("item.chandler_table", "item.candle", SkillType.Chandlery));
            Assert.True(Makes("item.hunting_blind", "item.game_meat", SkillType.Hunting));
            Assert.True(Makes("item.alembic", "item.remedy", SkillType.Medicine));
            Assert.Contains(db.Items.Get("item.potion_shelf").Interactions, i => i.Heal > 0f);
            Assert.Contains(db.Items.Get("item.cooking_pot").Interactions, i => i.ConsumeItem == "item.ale" && i.Need == NeedType.Fun);
            Assert.True(db.Items.Get("item.storage_jar").Preserves > 0f);              // küp kiler yerleşiminde serin yer
        }
    }
}
