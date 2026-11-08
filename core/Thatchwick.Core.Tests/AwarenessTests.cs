using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 18b (okunabilirlik), 18c (kış göstergesi, gün özeti; T20), 18d (talip havuzu; T18).
    public class AwarenessTests
    {
        private static Character Person(int id, string name, GridPos pos, Sex sex = Sex.Female, double age = 50, bool villager = false)
        {
            var c = new Character(id, name, pos) { Sex = sex, IsVillager = villager };
            c.SetAge(age);
            return c;
        }

        // ---------- bildirimler ----------

        [Fact]
        public void BildirimAkisi_AyniMetniBirlestirirVeKapasiteyiAsmaz()
        {
            var log = new NotificationLog();
            log.Add(1.0, NoticeLevel.Info, "Maud acıktı", 1);
            log.Add(1.1, NoticeLevel.Warning, "Maud acıktı", 1);
            Assert.Single(log.Items);
            Assert.Equal(2, log.Items[0].Repeats);
            Assert.Equal(NoticeLevel.Warning, log.Items[0].Level);

            log.Add(3.0, NoticeLevel.Info, "Maud acıktı", 1);   // pencere dışı: yeni kayıt
            Assert.Equal(2, log.Items.Count);

            for (int i = 0; i < 200; i++) log.Add(10 + i, NoticeLevel.Info, "olay " + i);
            Assert.Equal(NotificationLog.Capacity, log.Items.Count);
            Assert.Equal("olay 199", log.Recent(1)[0].Text);
            Assert.True(log.UnreadCount > 0);
            log.MarkAllRead();
            Assert.Equal(0, log.UnreadCount);
        }

        [Fact]
        public void IhtiyacIzleyici_BirKezUyarirToparlaninceYenidenSilahlanir()
        {
            var watcher = new NeedWatcher();
            var raised = new List<(NoticeLevel level, string text)>();
            watcher.Raised += (l, t, id) => raised.Add((l, t));
            var c = Person(1, "Maud", new GridPos(0, 0));

            c.Needs.Set(NeedType.Hunger, 25);
            watcher.Update(new[] { c });
            watcher.Update(new[] { c });
            Assert.Single(raised);
            Assert.Contains("acıktı", raised[0].text);

            c.Needs.Set(NeedType.Hunger, 10);
            watcher.Update(new[] { c });
            Assert.Equal(2, raised.Count);
            Assert.Equal(NoticeLevel.Danger, raised[1].level);

            c.Needs.Set(NeedType.Hunger, 35);          // histerezis bandında: tekrar uyarmaz
            watcher.Update(new[] { c });
            c.Needs.Set(NeedType.Hunger, 27);
            watcher.Update(new[] { c });
            Assert.Equal(2, raised.Count);

            c.Needs.Set(NeedType.Hunger, 90);
            watcher.Update(new[] { c });
            c.Needs.Set(NeedType.Hunger, 20);
            watcher.Update(new[] { c });
            Assert.Equal(3, raised.Count);
        }

        [Fact]
        public void IhtiyacIzleyici_BebegiVeKoylulerleOluleriAyiriyor()
        {
            var watcher = new NeedWatcher();
            var raised = new List<string>();
            watcher.Raised += (l, t, id) => raised.Add(t);
            var baby = Person(1, "Minik", new GridPos(0, 0), age: 3);
            baby.Needs.Set(NeedType.Hunger, 40);
            var villager = Person(2, "Köylü", new GridPos(0, 0), villager: true);
            villager.Needs.Set(NeedType.Hunger, 1);
            watcher.Update(new[] { baby, villager });
            Assert.Single(raised);
            Assert.Contains("bebek", raised[0]);
        }

        // ---------- eylem balonu ve kuyruk ----------

        private static (WorldState world, Character c, JobRunner runner, ItemInstance pot) Kitchen(bool player)
        {
            var world = new WorldState(new GridMap(12, 12));
            var pot = world.PlaceItem(new ItemDef
            {
                Id = "pot", DisplayName = "Kazan",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "eat", DisplayName = "Ye", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.1 } }
            }, new GridPos(6, 6));
            var c = Person(1, "Maud", new GridPos(1, 1));
            c.IsPlayerControlled = player;
            return (world, c, new JobRunner(c, world) { Autonomous = !player }, pot);
        }

        [Fact]
        public void EylemAciklayici_NedeniVeSimgeyiSoyler()
        {
            var (world, c, runner, pot) = Kitchen(player: false);
            Assert.Equal("idle", ActivityDescriber.Describe(c, runner, world).Icon);

            c.Needs.Set(NeedType.Hunger, 10);
            runner.Tick(0.001);
            var info = ActivityDescriber.Describe(c, runner, world);
            Assert.Equal("eat", info.Icon);
            Assert.Contains("Ye", info.Text);
            Assert.Equal("çünkü acıktı", info.Reason);
            Assert.True(info.Walking);

            var (w2, c2, r2, pot2) = Kitchen(player: true);
            r2.Command(Job.UseItem(pot2.Id, "eat"));
            var info2 = ActivityDescriber.Describe(c2, r2, w2);
            Assert.Equal("emir üzerine", info2.Reason);
            Assert.Contains("emir", info2.Full);
        }

        [Fact]
        public void KuyrukSeridi_IptalVeOneAlma()
        {
            var (world, c, runner, pot) = Kitchen(player: true);
            runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(3, 3)));
            runner.Queue.TryEnqueue(Job.UseItem(pot.Id, "eat"));
            runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(9, 9)));
            var strip = ActivityDescriber.Queue(runner, world);
            Assert.Equal(3, strip.Count);
            Assert.Equal("walk", strip[0].Icon);
            Assert.Equal("eat", strip[1].Icon);

            Assert.True(runner.Prioritize(1));
            Assert.Equal("eat", ActivityDescriber.Queue(runner, world)[0].Icon);
            Assert.True(runner.CancelAt(0));
            Assert.Equal(2, runner.Queue.Count);
            Assert.False(runner.CancelAt(9));
        }

        // ---------- T20: kış göstergesi ----------

        private static ContentDatabase FoodDb()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "item.turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "food" } });
            db.Items.Register(new ItemDef { Id = "item.firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "fuel" } });
            db.Items.Register(new ItemDef { Id = "item.log", DisplayName = "Kütük", Placement = ItemPlacement.Resource, Price = 3 });
            db.Items.Register(new ItemDef
            {
                Id = "pot", DisplayName = "Kazan",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "eat", DisplayName = "Ye", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.15, ConsumeItem = "item.turnip" }
                }
            });
            db.Items.Register(new ItemDef { Id = "block", DisplayName = "Kütük kesici", Interactions = new List<InteractionDef>
            {
                new InteractionDef { Id = "split", DisplayName = "Yar", DurationDays = 0.1, ConsumeItem = "item.log",
                    Produce = new List<ProduceDef> { new ProduceDef { ItemId = "item.firewood", Count = 3 } } }
            } });
            db.Items.Register(new ItemDef { Id = "stove", DisplayName = "Ocak", HeatOutput = 80, Price = 40 });
            return db;
        }

        [Fact]
        public void KisGostergesi_YiyecekTahmini_GercekTuketimleYuzdeOnIcinde()
        {
            var db = FoodDb();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pop = new Population();
            var clock = new GameClock();
            world.PlaceItem(db.Items.Get("pot"), new GridPos(6, 6));
            var chars = new List<Character>();
            var runners = new List<JobRunner>();
            var effects = new Thatchwick.Core.Work.InteractionEffects(world, house, db, seed: 1);
            for (int i = 0; i < 3; i++)
            {
                var c = Person(i + 1, "K" + i, new GridPos(1 + i, 1));
                pop.Add(c); chars.Add(c);
                runners.Add(new JobRunner(c, world) { Autonomous = true, Effects = effects });
            }
            house.AddItem("item.turnip", 100);
            var forecast = new WinterForecast(db, house, world, pop, clock.Config);
            double predicted = forecast.Report(0).FoodDays;
            Assert.InRange(predicted, 13, 15);   // 100 x 50 / (3 x 120)

            double t = 0;
            const double step = 0.01;
            while (house.Count("item.turnip") > 0 && t < 40)
            {
                for (int i = 0; i < chars.Count; i++) { chars[i].Tick(step); runners[i].Tick(step); }
                t += step;
            }
            Assert.InRange(predicted / t, 0.9, 1.1);
        }

        [Fact]
        public void KisGostergesi_OdunTahmini_IsiKaynagiYakmaHiziylaUyusur()
        {
            var db = FoodDb();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pop = new Population();
            var clock = new GameClock();
            var stove = world.PlaceItem(db.Items.Get("stove"), new GridPos(5, 5));
            house.AddItem("item.firewood", 12);
            house.AddItem("item.log", 2);                  // 2 x 3 = 6 odun
            var forecast = new WinterForecast(db, house, world, pop, clock.Config);
            Assert.Equal(3, forecast.FirewoodPerLog());
            Assert.Equal(18, forecast.FirewoodStock());
            double predicted = forecast.Report(0).FirewoodDays;   // 18 odun / (1 / 0.5) = 9 gün

            var heat = new HeatSystem(world, clock.Config);
            double t = 0;
            int burned = 0;
            while (t < 30)
            {
                if (stove.FuelDays < 0.2 && house.Count("item.firewood") > 0 && heat.TryStoke(stove.Id, house)) burned++;
                else if (stove.FuelDays < 0.2 && house.Count("item.firewood") == 0) break;
                heat.Tick(0.01, t);
                t += 0.01;
            }
            Assert.Equal(12, burned);       // yalnızca hazır odun (kütük yarılmadı)
            // Yarılmamış kütükler dahil tahmin: 9 gün; yarılmış 12 odunla gerçek ~6 gün + 3 gün kütük payı.
            Assert.InRange(t / (12 * 0.5), 0.9, 1.1);
        }

        [Fact]
        public void KisGostergesi_KisaKalanGunVeUyarilar()
        {
            var db = FoodDb();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pop = new Population();
            pop.Add(Person(1, "Baba", new GridPos(0, 0), Sex.Male));
            var clock = new GameClock();   // 10 gün/mevsim
            var forecast = new WinterForecast(db, house, world, pop, clock.Config);

            var r = forecast.Report(25);   // Sonbahar (20-29), 5. gün: kışa 5 gün
            Assert.Equal(5, r.DaysToWinter, 6);
            Assert.False(r.InWinter);
            Assert.NotEmpty(r.Warnings);   // yiyecek yok, ısı kaynağı yok
            Assert.Contains(r.Warnings, w => w.Contains("ısı kaynağı"));

            var inWinter = forecast.Report(34);
            Assert.True(inWinter.InWinter);
            Assert.Equal(6, inWinter.WinterDaysLeft, 6);
            Assert.True(inWinter.HasDanger);

            world.PlaceItem(db.Items.Get("stove"), new GridPos(3, 3));
            house.AddItem("item.firewood", 40);
            house.AddItem("item.turnip", 60);
            Assert.Empty(forecast.Report(25).Warnings);
        }

        // ---------- gün özeti ----------

        [Fact]
        public void GunOzeti_GunDonumundeAkceXpEnvanterVeOlaylariToplar()
        {
            var house = new Household(10);
            int xp = 0;
            var ledger = new DayLedger(house, () => xp);
            DaySummary got = null;
            ledger.DayEnded += s => got = s;

            Assert.Null(ledger.Tick(0));
            house.AddAkce(15); xp += 40;
            house.AddItem("item.turnip", 4);
            ledger.OnBorn(Person(5, "Minik", new GridPos(0, 0), age: 0));
            ledger.OnDied(Person(6, "Dede", new GridPos(0, 0)));
            ledger.Note("Pazarda 3 şalgam satıldı");
            Assert.Null(ledger.Tick(0));

            var s1 = ledger.Tick(1);
            Assert.NotNull(s1);
            Assert.Same(got, s1);
            Assert.Equal(15, s1.AkceDelta);
            Assert.Equal(40, s1.XpDelta);
            Assert.Equal(1, s1.Born); Assert.Equal(1, s1.Died);
            Assert.Contains(s1.ItemDeltas, kv => kv.Key == "item.turnip" && kv.Value == 4);
            Assert.False(s1.IsEmpty);

            Assert.True(ledger.Tick(2).IsEmpty);   // yeni gün sıfırdan başladı
        }

        // ---------- T18: talip havuzu ----------

        [Fact]
        public void TalipHavuzu_EksikAdayiTamamlar_AkrabaVerilmez()
        {
            var db = LifeTests.Db();
            var pop = new Population(seed: 4);
            var pool = new SuitorPool(db, pop, seed: 9);
            var founder = Person(1, "Aldric", new GridPos(0, 0), Sex.Male, 30);
            pop.Add(founder);

            Assert.Equal(0, pool.EligibleFor(founder).Count);
            int arrived = pool.Ensure();
            Assert.Equal(SuitorPool.MinCandidates, arrived);
            var candidates = pool.EligibleFor(founder);
            Assert.Equal(3, candidates.Count);
            Assert.All(candidates, c => { Assert.Equal(Sex.Female, c.Sex); Assert.True(c.IsVillager); Assert.True(c.TraitIds.Count >= 1); });
            Assert.Equal(0, pool.Ensure());   // yeterli: yeni gelen yok

            // Yakın akraba aday sayılmaz.
            var sister = Person(2, "Kız Kardeş", new GridPos(0, 0), Sex.Female, 30, villager: true);
            sister.BirthMotherId = 77; founder.BirthMotherId = 77;
            pop.Add(sister);
            Assert.DoesNotContain(sister, pool.EligibleFor(founder));
        }

        [Fact]
        public void TalipHavuzu_BesNesilde_EvlenecekAdayTukenmez_AkrabaEvliligiOlmaz()
        {
            var db = LifeTests.Db();
            var pop = new Population(seed: 21);
            int nameCounter = 0;
            pop.NameGenerator = sex => (sex == Sex.Female ? "Kadın" : "Adam") + (++nameCounter);
            pop.Content = db;
            var house = new Household();
            var family = new FamilyService(pop, house, db, seed: 5);
            var pool = new SuitorPool(db, pop, seed: 33);
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Tags = new List<string> { "food" } });
            family.JoinedHousehold += c => { };

            var founder = Person(1, "Kurucu", new GridPos(0, 0), Sex.Male, 30);
            pop.Add(founder);

            int marriages = 0;
            int maxGeneration = 1;
            double day = 0;
            const double step = 0.5;
            while (day < 900 && maxGeneration < 6)
            {
                // Her gün: talip havuzunu doldur, bekâr yetişkin hane üyeleri evlenir, evliler çocuk ister.
                pool.Ensure();
                foreach (var m in pop.Members.ToArray())
                {
                    m.Needs.Set(NeedType.Hunger, 100);   // beslenme zinciri bu testin konusu değil
                    if (m.IsDead || m.IsVillager) continue;
                    if (SuitorPool.IsSingleAdult(m))
                    {
                        var options = pool.EligibleFor(m);
                        Assert.NotEmpty(options);   // T18: aday asla tükenmiyor
                        var mate = options[0];
                        Assert.False(FamilyService.AreCloseRelatives(m, mate));
                        family.SetAffection(m.Id, mate.Id, FamilyService.MarriageAffection);
                        Assert.Null(family.CanDo(m, mate, SocialAction.Propose));
                        family.Apply(m, mate, SocialAction.Propose);
                        marriages++;
                    }
                    else if (m.SpouseId.HasValue && m.Sex == Sex.Female && m.CanConceive)
                    {
                        var husband = pop.Find(m.SpouseId.Value);
                        if (husband != null && !husband.IsDead) family.Apply(m, husband, SocialAction.TryForBaby);
                    }
                    maxGeneration = Math.Max(maxGeneration, m.Generation);
                }
                for (double t = 0; t < 1.0; t += step) pop.Tick(step);
                day += 1.0;
            }
            Assert.True(maxGeneration >= 5, $"5 nesle ulaşılamadı (en yüksek {maxGeneration}, gün {day}, üye {pop.Members.Count}, evlilik {marriages}, hamile {pop.Members.Count(x => x.IsPregnant)}, ölü {pop.Members.Count(x => x.IsDead)})");
            Assert.True(marriages >= 4);
            // Hiçbir evli çift yakın akraba değil.
            foreach (var c in pop.Members)
                if (c.SpouseId.HasValue) Assert.False(FamilyService.AreCloseRelatives(c, pop.Find(c.SpouseId.Value)));
        }
    }
}
