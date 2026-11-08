using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 33 (hava; T19), 34 (takvim, festival), 35 (ekin, bozulma, koruma; T22), 36 (toplama), 37 (tarif, kalite), 42 (kümes).
    public class WorldSystemsTests
    {
        private static ContentDatabase Real()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static Character Cook(int id, GridPos pos, int cookingLevel)
        {
            var c = new Character(id, "Aşçı" + id, pos); c.SetAge(60);
            c.GainSkillXp(SkillType.Cooking, SkillRules.XpForLevel(cookingLevel) + 1);
            return c;
        }

        // ---------- T19: hava ----------

        [Fact]
        public void Hava_TahminErtesiGuneleBirebirTutar_VeTohumlaTekrarlanir()
        {
            var time = new TimeConfig();
            var a = new WeatherSystem(time, seed: 11);
            var b = new WeatherSystem(time, seed: 11);
            var other = new WeatherSystem(time, seed: 12);
            int differing = 0;
            for (long day = 0; day < 400; day++)
            {
                Assert.Equal(a.WeatherOn(day), b.WeatherOn(day));
                Assert.Equal(a.WeatherOn(day + 1), a.Tomorrow(day + 0.5));       // bugünün tahmini yarının gerçeğiyle aynı
                Assert.Equal(a.Tomorrow(day + 0.9), a.Today(day + 1.1));
                if (a.WeatherOn(day) != other.WeatherOn(day)) differing++;
            }
            Assert.True(differing > 20);
        }

        [Fact]
        public void Hava_MevsimeUygun_YazdaKarYokKisaYagmurYok()
        {
            var w = new WeatherSystem(new TimeConfig(), seed: 5);
            var counts = new Dictionary<(Season, WeatherType), int>();
            for (long day = 0; day < 4000; day++)
            {
                var season = (Season)((day / 10) % 4);
                var t = w.WeatherOn(day);
                counts[(season, t)] = (counts.TryGetValue((season, t), out var n) ? n : 0) + 1;
            }
            int Get(Season s, WeatherType t) => counts.TryGetValue((s, t), out var n) ? n : 0;
            Assert.Equal(0, Get(Season.Summer, WeatherType.Snow));
            Assert.Equal(0, Get(Season.Winter, WeatherType.Rain));
            Assert.True(Get(Season.Winter, WeatherType.Snow) > 400);
            Assert.True(Get(Season.Spring, WeatherType.Rain) > 250);
            Assert.True(Get(Season.Autumn, WeatherType.Storm) > 100);
            Assert.True(Get(Season.Summer, WeatherType.Clear) > Get(Season.Summer, WeatherType.Rain));
        }

        [Fact]
        public void Hava_DisaridaIsHiziniDusurur_IcerideEtkilemez()
        {
            var db = Real();
            var world = new WorldState(new GridMap(10, 10));
            for (int i = 0; i < 4; i++)
            {
                world.Map.SetEdge(new Edge(1 + i, 1, EdgeAxis.Horizontal), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(1 + i, 5, EdgeAxis.Horizontal), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(1, 1 + i, EdgeAxis.Vertical), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(5, 1 + i, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            var clock = new GameClock();
            var life = new LifeSystems(db, world, new Household(), new Population(), clock);
            long stormDay = -1;
            for (long d = 0; d < 200; d++) if (life.Weather.WeatherOn(d) == WeatherType.Storm) { stormDay = d; break; }
            Assert.True(stormDay >= 0);
            clock.Restore(stormDay + 0.5, 1);

            var outside = new Character(1, "Dışarıda", new GridPos(8, 8));
            var inside = new Character(2, "İçeride", new GridPos(3, 3));
            var chop = new InteractionDef { Id = "c", Skill = SkillType.Woodcutting, DurationDays = 1 };
            Assert.Equal(0.7, life.WorkSpeedFor(outside, chop), 6);
            Assert.Equal(1.0, life.WorkSpeedFor(inside, chop), 6);

            var runner = new JobRunner(outside, world);
            life.Attach(runner);
            Assert.True(runner.EffectiveDuration(chop) > 1.4);   // 1 / 0.7
        }

        [Fact]
        public void Hava_KarDisSicakligiDusurur_YagmurEkiniHizlandirir()
        {
            var world = new WorldState(new GridMap(10, 10));
            var time = new TimeConfig();
            var heat = new HeatSystem(world, time);
            heat.Tick(0.1, 15);
            float baseline = heat.OutdoorTemperature;
            heat.OutdoorOffset = () => WeatherSystem.TemperatureOffset(WeatherType.Snow);
            heat.Tick(0.1, 15);
            Assert.Equal(baseline - 3f, heat.OutdoorTemperature, 3);

            var db = Real();
            var w2 = new WorldState(new GridMap(10, 10));
            var plants = new PlantSystem(w2);
            var crop = w2.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(3, 3));
            var crop2 = w2.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(5, 5));
            plants.GrowthMultiplier = () => WeatherSystem.GrowthMultiplier(WeatherType.Rain);
            plants.Tick(1.0, Season.Spring);
            Assert.Equal(1.3, crop.GrowthDays, 3);
            plants.GrowthMultiplier = null;
            plants.Tick(1.0, Season.Spring);
            Assert.Equal(2.3, crop2.GrowthDays, 3);   // 1,3 (yağmurlu) + 1,0 (normal)
        }

        [Fact]
        public void Ekin_YalnizcaKendiMevsimlerindeBuyur()
        {
            var db = Real();
            var world = new WorldState(new GridMap(10, 10));
            var plants = new PlantSystem(world);
            var pea = world.PlaceItem(db.Items.Get("item.crop_pea"), new GridPos(2, 2));          // yalnız ilkbahar
            var leek = world.PlaceItem(db.Items.Get("item.crop_leek"), new GridPos(4, 2));        // yaz ve sonbahar
            plants.Tick(1.0, Season.Summer);
            Assert.Equal(0, pea.GrowthDays);
            Assert.Equal(1, leek.GrowthDays);
            plants.Tick(1.0, Season.Spring);
            Assert.Equal(1, pea.GrowthDays);
            Assert.Equal(1, leek.GrowthDays);
        }

        // ---------- T22: bozulma ve koruma ----------

        [Fact]
        public void Bozulma_TazeYiyecekCurur_KorunanKisaTasinir()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var spoil = new SpoilageService(db, house, world);
            var spoiled = new List<(string, int)>();
            spoil.Spoiled += (id, n) => spoiled.Add((id, n));

            house.AddItem("item.turnip", 6);            // 20 gün
            house.AddItem("item.pickled_turnip", 6);    // çürümez
            house.AddItem("item.honey", 2);             // çürümez
            for (int i = 0; i < 190; i++) spoil.Tick(0.1);   // 19 gün
            Assert.Equal(6, house.Count("item.turnip"));
            for (int i = 0; i < 20; i++) spoil.Tick(0.1);    // 21. gün
            Assert.Equal(0, house.Count("item.turnip"));
            Assert.Contains(spoiled, s => s.Item1 == "item.turnip" && s.Item2 == 6);
            for (int i = 0; i < 1000; i++) spoil.Tick(0.1);  // 100 gün daha
            Assert.Equal(6, house.Count("item.pickled_turnip"));
            Assert.Equal(2, house.Count("item.honey"));
        }

        [Fact]
        public void Mahzen_BozulmayiYavaslatir()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var spoil = new SpoilageService(db, house, world);
            world.PlaceItem(db.Items.Get("item.root_cellar"), new GridPos(3, 3));
            Assert.Equal(0.4, spoil.RateMultiplier(), 6);
            house.AddItem("item.turnip", 5);
            for (int i = 0; i < 400; i++) spoil.Tick(0.1);   // 40 gün: mahzensiz 20'de çürürdü; mahzenle 50'ye kadar dayanır
            Assert.Equal(5, house.Count("item.turnip"));
            for (int i = 0; i < 150; i++) spoil.Tick(0.1);   // 55. gün
            Assert.Equal(0, house.Count("item.turnip"));
            Assert.InRange(spoil.DaysLeftOldest("item.pumpkin"), -1, -1);   // izlenmeyen
        }

        [Fact]
        public void Bozulma_EnEskiTuketilir_YeniPartiSonraCurur()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var spoil = new SpoilageService(db, house, world);
            house.AddItem("item.turnip", 4);
            for (int i = 0; i < 100; i++) spoil.Tick(0.1);   // 10 gün
            house.AddItem("item.turnip", 4);                 // ikinci parti
            spoil.Tick(0.001);
            for (int i = 0; i < 3; i++) house.TryTakeItem("item.turnip");   // 3 tüketim: en eski partiden gider
            for (int i = 0; i < 100; i++) spoil.Tick(0.1);   // 20. gün: eski parti (1 kaldı) çürür, yeni parti (4) 10 günlük
            Assert.Equal(4, house.Count("item.turnip"));
            Assert.True(spoil.DaysLeftOldest("item.turnip") > 8);
        }

        [Fact]
        public void Hasattan_KisaKadarKorunanYiyecekSaglam()
        {
            // Sonbahar hasadı: lahana turşuya, bezelye kuruya çevrilir; taze lahana kışta çürür. 30 gün sonra.
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var spoil = new SpoilageService(db, house, world);
            var barrel = db.Items.Get("item.salting_barrel");
            var pickle = barrel.Interactions.First(i => i.Id == "pickle_cabbage");
            house.AddItem("item.cabbage", 6);
            var effects = new InteractionEffects(world, house, db, seed: 1);
            var cook = Cook(1, new GridPos(0, 0), 1);
            var barrelItem = world.PlaceItem(barrel, new GridPos(4, 4));
            for (int i = 0; i < 2; i++) { Assert.Null(effects.CanStart(cook, barrelItem, pickle)); effects.Apply(cook, barrelItem, pickle); }
            Assert.Equal(0, house.Count("item.cabbage"));
            Assert.Equal(6, house.Count("item.pickled_cabbage"));
            house.AddItem("item.cabbage", 2);                // bir parti taze lahana

            for (int i = 0; i < 300; i++) spoil.Tick(0.1);   // 30 gün
            Assert.Equal(6, house.Count("item.pickled_cabbage"));
            Assert.Equal(0, house.Count("item.cabbage"));    // taze olan 14 günde çürüdü
        }

        // ---------- Tarif ve kalite (37) ----------

        private static (WorldState world, Household house, ItemInstance pot, InteractionEffects effects) Kitchen(ContentDatabase db)
        {
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pot = world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(5, 5));
            return (world, house, pot, new InteractionEffects(world, house, db, seed: 3));
        }

        [Fact]
        public void Tarif_SeviyeKilitliVeKaliteBeceriyeBagli()
        {
            var db = Real();
            var (world, house, pot, effects) = Kitchen(db);
            var stew = pot.Def.Interactions.First(i => i.Id == "cook_stew");

            var novice = Cook(1, new GridPos(5, 6), 1);
            var runner = new JobRunner(novice, world) { Effects = effects };
            house.AddItem("item.turnip", 3); house.AddItem("item.cabbage", 3); house.AddItem("item.leek", 3);
            runner.Command(Job.UseItem(pot.Id, "cook_stew"));
            runner.Tick(0.05);
            Assert.Contains("seviye 2", runner.LastFailure);

            string Produced(int level, int qualityBonus = 0)
            {
                var chef = Cook(2, new GridPos(5, 6), level);
                var h = new Household(); h.AddItem("item.turnip", 1); h.AddItem("item.cabbage", 1); h.AddItem("item.leek", 1);
                var eff = new InteractionEffects(world, h, db, seed: 3);
                eff.Apply(chef, pot, stew);
                foreach (var id in new[] { "item.stew_plain", "item.stew_good", "item.stew_fine" }) if (h.Count(id) > 0) return id;
                return null;
            }
            Assert.Equal("item.stew_plain", Produced(2));
            Assert.Equal("item.stew_good", Produced(5));
            Assert.Equal("item.stew_fine", Produced(8));

            // Usta eller (+1 kalite seviyesi) 3. seviyeyi iyi yapar.
            var db2 = db;
            var handy = Cook(3, new GridPos(5, 6), 3);
            handy.SetTraits(new[] { "trait.handy" });
            LifeRules.Recompute(handy, db2);
            var hh = new Household(); hh.AddItem("item.turnip", 1); hh.AddItem("item.cabbage", 1); hh.AddItem("item.leek", 1);
            new InteractionEffects(world, hh, db, seed: 3).Apply(handy, pot, stew);
            Assert.Equal(2, hh.Count("item.stew_good"));
        }

        [Fact]
        public void KaliteliYemek_DahaCokDoyurur_RuhHaliVerir()
        {
            var db = Real();
            var (world, house, pot, effects) = Kitchen(db);
            float Eat(string interactionId, string foodId)
            {
                var c = Cook(1, new GridPos(5, 6), 1);
                c.Needs.Set(NeedType.Hunger, 0);
                house.AddItem(foodId, 1);
                var runner = new JobRunner(c, world) { Effects = effects };
                runner.Command(Job.UseItem(pot.Id, interactionId));
                for (int i = 0; i < 40 && !runner.IsIdle; i++) runner.Tick(0.02);
                if (interactionId == "eat_stew_fine") Assert.True(c.Mood.Has("mood.meal"));
                return c.Needs.Get(NeedType.Hunger);
            }
            float plain = Eat("eat_stew_plain", "item.stew_plain");
            float good = Eat("eat_stew_good", "item.stew_good");
            float fine = Eat("eat_stew_fine", "item.stew_fine");
            Assert.True(plain < good && good < fine, $"{plain} < {good} < {fine}");
            Assert.True(db.Items.Get("item.stew_fine").Price > db.Items.Get("item.stew_plain").Price);   // satış değeri kaliteyle artar
        }

        [Fact]
        public void Otonomi_EnIyiYemegiSecer()
        {
            var db = Real();
            var (world, house, pot, effects) = Kitchen(db);
            house.AddItem("item.turnip", 2); house.AddItem("item.stew_fine", 2);
            var npc = Cook(1, new GridPos(5, 6), 1);
            npc.Needs.Set(NeedType.Hunger, 10);
            var runner = new JobRunner(npc, world) { Effects = effects, Autonomous = true };
            runner.Tick(0.001);
            Assert.Equal("eat_stew_fine", runner.Queue.Current.InteractionId);
        }

        [Fact]
        public void MarangozTezgahi_KutukTahtaEsya()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var bench = world.PlaceItem(db.Items.Get("item.workbench"), new GridPos(5, 5));
            var effects = new InteractionEffects(world, house, db, seed: 4);
            var c = new Character(1, "Marangoz", new GridPos(5, 7)); c.SetAge(60);
            var runner = new JobRunner(c, world) { Effects = effects };
            house.AddItem("item.log", 2);

            void Run(string id) { runner.Command(Job.UseItem(bench.Id, id)); for (int i = 0; i < 60 && !runner.IsIdle; i++) runner.Tick(0.02); }
            Run("saw_plank"); Run("saw_plank");
            Assert.Equal(4, house.Count("item.plank"));
            Run("craft_table");   // seviye 3 gerekir
            Assert.Contains("seviye 3", runner.LastFailure);
            Run("craft_stool");
            Assert.Equal(1, house.Count("item.stool_wood"));
            Assert.Equal(2, house.Count("item.plank"));
            Assert.True(c.Skills.GetXp(SkillType.Carpentry) > 0);
        }

        // ---------- Toplama (36) ----------

        [Fact]
        public void Toplama_MevsimdeCikarMevsimBitinceKaybolur()
        {
            var db = Real();
            var world = new WorldState(new GridMap(30, 30));
            var forage = new ForageSystem(world, db, seed: 9);
            for (int i = 0; i < 12; i++) forage.OnNewDay(Season.Autumn);
            var mushroom = db.Items.Get("item.forage_mushroom");
            var berry = db.Items.Get("item.forage_berry_bush");
            Assert.InRange(forage.Count(mushroom), 1, mushroom.ForageMax);
            Assert.InRange(forage.Count(berry), 1, berry.ForageMax);
            Assert.Equal(0, forage.Count(db.Items.Get("item.forage_dry_branch")));   // kuru dal yalnızca kışın

            for (int i = 0; i < 12; i++) forage.OnNewDay(Season.Winter);
            Assert.Equal(0, forage.Count(mushroom));
            Assert.Equal(0, forage.Count(berry));
            Assert.InRange(forage.Count(db.Items.Get("item.forage_dry_branch")), 3, 5);
            foreach (var it in world.Items) Assert.True(BuildService.IsLocked(it));   // oyuncu taşıyamaz
        }

        [Fact]
        public void KuruDal_KistaOdunVerir()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var branch = world.PlaceItem(db.Items.Get("item.forage_dry_branch"), new GridPos(4, 4));
            var effects = new InteractionEffects(world, house, db, seed: 1);
            var c = new Character(1, "K", new GridPos(4, 6)); c.SetAge(40);
            var runner = new JobRunner(c, world) { Effects = effects };
            runner.Command(Job.UseItem(branch.Id, "gather"));
            for (int i = 0; i < 40 && !runner.IsIdle; i++) runner.Tick(0.02);
            Assert.Equal(2, house.Count("item.firewood"));
            Assert.Null(world.GetItem(branch.Id));
        }

        // ---------- Kümes (42) ----------

        [Fact]
        public void Kumes_GunlukUretirKistaYavaslar_TopluncaSifirlanir()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var coop = world.PlaceItem(db.Items.Get("item.coop"), new GridPos(4, 4));
            var live = new LivestockSystem(world);
            int ready = 0;
            live.BecameReady += _ => ready++;
            var effects = new InteractionEffects(world, house, db, seed: 1);
            var c = new Character(1, "Çiftçi", new GridPos(4, 6)); c.SetAge(40);
            var runner = new JobRunner(c, world) { Effects = effects };

            runner.Command(Job.UseItem(coop.Id, "collect_eggs"));
            runner.Tick(0.05);
            Assert.Contains("hazır değil", runner.LastFailure);

            live.Tick(0.9, Season.Summer);
            Assert.False(coop.IsReady);
            live.Tick(0.2, Season.Summer);
            Assert.True(coop.IsReady);
            Assert.Equal(1, ready);

            runner.Command(Job.UseItem(coop.Id, "collect_eggs"));
            for (int i = 0; i < 40 && !runner.IsIdle; i++) runner.Tick(0.02);
            Assert.Equal(2, house.Count("item.egg"));
            Assert.False(coop.IsReady);

            live.Tick(1.0, Season.Winter);        // kışın yarı hız
            Assert.False(coop.IsReady);
            live.Tick(1.0, Season.Winter);
            Assert.True(coop.IsReady);
        }

        // ---------- Takvim ve festival (34) ----------

        [Fact]
        public void Festival_KendiGunundeKutlanirBirKez()
        {
            var db = Real();
            var clock = new GameClock();
            var fest = new FestivalService(db, clock);
            var villager = new Character(1, "Köylü", new GridPos(0, 0)); villager.SetAge(40);
            var baby = new Character(2, "Bebek", new GridPos(0, 0));
            villager.Needs.Set(NeedType.Fun, 10);
            var all = new[] { villager, baby };
            int celebrations = 0;
            fest.Celebrated += (f, n) => celebrations += n;

            clock.Restore(4.1, 1);                // İlkbahar 5. gün = gün 4: Tohum Şenliği, sabah
            Assert.Equal("festival.seed", fest.TodaysFestival().Id);
            Assert.Equal(0, fest.Tick(all));      // saat gelmedi
            clock.Restore(4.5, 1);
            Assert.Equal(1, fest.Tick(all));      // bebek katılmaz
            Assert.Equal(0, fest.Tick(all));      // aynı gün ikinci kez olmaz
            Assert.Equal(50f, villager.Needs.Get(NeedType.Fun), 3);
            Assert.True(villager.Mood.Has("mood.festival"));
            Assert.Equal(1, celebrations);

            clock.Restore(5.5, 1);
            Assert.Null(fest.TodaysFestival());
            Assert.Equal(0, fest.Tick(all));
        }

        [Fact]
        public void Takvim_SonrakiFestivalPazarGunuVeEkTeklif()
        {
            var db = Real();
            var clock = new GameClock();
            var fest = new FestivalService(db, clock);
            clock.Restore(0.0, 1);
            var next = fest.NextFestival(out int days);
            Assert.Equal("festival.seed", next.Id);
            Assert.Equal(4, days);

            clock.Restore(36.0, 1);   // Kış 7. gün
            next = fest.NextFestival(out days);
            Assert.Equal("festival.seed", next.Id);   // yıl dönümü: ilkbahar şenliği
            Assert.Equal(8, days);

            Assert.True(fest.IsMarketDay(4)); Assert.True(fest.IsMarketDay(9)); Assert.False(fest.IsMarketDay(5));
            clock.Restore(24.0, 1);              // gün 24: pazar günü (24 % 5 = 4) ama festival değil
            Assert.Equal(1, fest.ExtraOffers());
            clock.Restore(25.0 + 0.1, 1);        // sonbahar 6. gün = hasat şenliği (gün 25? 20 + 5)
            Assert.Equal("Sonbahar 6. gün, 1. yıl", fest.DateText(25));
            Assert.Equal(0, fest.ExtraOffers() - (fest.TodaysFestival() != null ? fest.TodaysFestival().ExtraOffers : 0));
        }

        [Fact]
        public void Pazar_EkTeklifVeSatisBonusuSaglayicilarlaCalisir()
        {
            var db = Real();
            var house = new Household();
            var market = new Market(db, house, null, null, seed: 4);
            market.Tick(0, 0, Season.Spring);
            int baseOffers = market.Offers.Count;
            int basePrice = market.SellPrice("item.turnip");
            market.ExtraOffers = () => 2;
            market.SellBonus = () => 0.2;
            market.Tick(0, 1, Season.Spring);
            Assert.Equal(baseOffers + 2, market.Offers.Count);
            Assert.True(market.SellPrice("item.turnip") >= basePrice);
        }

        // ---------- Kayıt ----------

        [Fact]
        public void YasamKaydi_HavaTohumuBozulmaVeSenlikGunuGeriGelir()
        {
            var db = Real();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, new Population(), clock);
            life.Weather.Seed = 4242;
            life.Festivals.CelebratedDay = 4;
            house.AddItem("item.turnip", 3);
            life.Spoilage.Tick(5.0);
            var data = life.Export();

            var life2 = new LifeSystems(db, world, house, new Population(), clock);
            life2.Restore(data);
            Assert.Equal(4242, life2.Weather.Seed);
            Assert.Equal(4, life2.Festivals.CelebratedDay);
            Assert.InRange(life2.Spoilage.DaysLeftOldest("item.turnip"), 14.9, 15.1);
        }
    }
}
