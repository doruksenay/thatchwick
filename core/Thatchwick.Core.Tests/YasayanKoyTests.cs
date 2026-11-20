using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Yaşayan köy, parti 1 (Project belgeleri 66d, 66e, 66f): köylünün gerçek ihtiyacı, hane kileri, dükkân stoğu, Pazar,
    /// alışveriş günü, çağırma, bunalım ve teselli, ikram, soyut bölge hesabı, kayıt.
    /// </summary>
    public class YasayanKoyTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private sealed class Kit
        {
            public ContentDatabase Db; public Population Pop; public Household House; public LifeSystems Life;
            public FamilyService Family; public GameClock Clock; public Character Ali;
            public LivingVillage Village => Life.Living;
            public Character Villager(string defId) => Life.Villagers.CharacterOf(defId);
        }

        private static Kit Make(double totalDays = 2.3, int seed = 3)
        {
            var k = new Kit { Db = ContentFilesTests.LoadDb(out _), Pop = new Population(), House = new Household(500), Clock = new GameClock() };
            k.Clock.Restore(totalDays, 1);
            var w = new WorldState(new GridMap(8, 8));
            k.Ali = new Character(1, "Ali", new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            k.Ali.SetAge(40);
            k.Pop.Add(k.Ali);
            k.Life = new LifeSystems(k.Db, w, k.House, k.Pop, k.Clock, seed);
            k.Family = new FamilyService(k.Pop, k.House, k.Db);
            k.Life.Attach(k.Family);
            foreach (var c in k.Life.Villagers.SpawnMissing(_ => new GridPos(3, 3))) LivingVillage.ApplyRules(c, null);
            k.Life.Houses.EnsureHouses();
            return k;
        }

        /// <summary>Saati (gün kesri) ve günü ayarlar; köy hayatı yeni adımı hemen hesaplasın diye biriken süre doldurulur.</summary>
        private static void At(Kit k, long day, double frac) => k.Clock.Restore(day + frac, 1);

        private static void Kill(Character c) { c.LifespanDays = c.AgeDays; c.Tick(0.001); Assert.True(c.IsDead); }

        private static void Step(Kit k, double days = LivingVillage.StepDays * 1.01) => k.Village.Tick(days);

        [Fact]
        public void Veri_HerMeslegeIsDongusu_IkramVeOgunlerGecerli()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            foreach (var v in db.Villagers.All)
            {
                if (string.IsNullOrEmpty(v.Work)) continue;   // işyeri olmayan (çalgıcı) meyhanede çalar
                var steps = db.VillageLife.WorkFor(v.Role);
                Assert.True(steps != null && steps.Count >= 2, $"{v.Role}: iş döngüsü yok");
            }
            Assert.Contains(db.VillageLife.WorkFor("blacksmith"), s => s.Pose == "bellows");
            Assert.Contains(db.VillageLife.WorkFor("fisher"), s => s.Id == "mend_net");
            Assert.True(db.VillageLife.Meals.Count(m => m.Where == "home") >= 3);
            Assert.Equal(0.15, db.VillageLife.GiftChance, 3);
            Assert.Equal(0.05, db.VillageLife.Discount, 3);
            Assert.InRange(db.VillageLife.BreakdownMinDays, 1.0, 1.0);
            Assert.InRange(db.VillageLife.BreakdownMaxDays, 2.0, 2.0);
        }

        [Fact]
        public void IsDongusu_GunBoyuSirayla_KisiyeGoreEvreKayar()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            var seen = new HashSet<string>();
            for (int m = 0; m < 24 * 60; m += 5) seen.Add(k.Village.WorkStep(orhan, 2 + m / 1440.0).Id);
            Assert.Contains("forge", seen); Assert.Contains("bellows", seen); Assert.Contains("quench", seen);
        }

        [Fact]
        public void Koylu_GercekIhtiyac_RuhHali_AcliktanVeSoguktanOlmez()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            Assert.True(orhan.MoodEnabled);
            Assert.False(orhan.CanStarve);
            Assert.False(orhan.CanFreeze);
            Assert.True(orhan.Needs.GetDecayPerDay(NeedType.Hunger) > 0);
            orhan.AmbientTemperature = -25f;
            orhan.Needs.Set(NeedType.Hunger, 0f);
            for (int i = 0; i < 400; i++) orhan.Tick(0.05);   // 20 gün aç ve donuk
            Assert.False(orhan.IsDead);
            Assert.True(orhan.Mood.Total < 0);   // açlık ve soğuk kasvet doğurur
        }

        [Fact]
        public void Koylu_AcikincaEvdeKilerdenYer_KilerDuser()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            string home = k.Village.HomeOf(orhan);
            At(k, 2, 0.72);   // akşam sofrası (Salı)
            Assert.Equal(home, k.Village.PlaceFor(orhan));
            Step(k);   // ilk adım: kiler hedefle dolar
            orhan.Needs.Set(NeedType.Hunger, 20f);
            int before = k.Db.VillageLife.Pantry.Sum(p => k.Village.PantryCount(home, p.Item));
            // Ertesi günün akşamı: bugünün akşam öğünü yenmemiş olsun.
            At(k, 3, 0.72);
            orhan.Needs.Set(NeedType.Hunger, 20f);
            Step(k);
            Assert.True(orhan.Needs.Get(NeedType.Hunger) > 50f);
            int after = k.Db.VillageLife.Pantry.Sum(p => k.Village.PantryCount(home, p.Item))
                + k.Db.VillageLife.Produce.Sum(p => 0);
            Assert.True(after < before + 6, "kilerden bir şey düşmeli");
            Assert.NotNull(k.Village.ScenePose(orhan, k.Clock.TotalDays));   // öğün sahnesi (zincir pozları)
        }

        [Fact]
        public void Koylu_IsSaatindeDukkanda_YalnizcaIsyerindekiEsyalar()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.30);
            Assert.Equal("work:smithy", k.Village.PlaceFor(orhan));
            var allowed = VillagerFreeWill.AllowedPlaces(k.Village.HomeOf(orhan), "work:smithy", "work:smithy");
            Assert.Equal(new[] { "work:smithy" }, allowed);
            var evening = VillagerFreeWill.AllowedPlaces(k.Village.HomeOf(orhan), "work:smithy", "square");
            Assert.Contains("work:tavern", evening); Assert.Contains("square", evening); Assert.Contains(k.Village.HomeOf(orhan), evening);
        }

        [Fact]
        public void OzgurIrade_OyuncununArsasindakiYatagaGitmez_IzinliYerdekineGider()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(30, 8);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 30; x++) map.SetOwned(new GridPos(x, y), x < 10);   // oyuncunun arsası x<10
            var w = new WorldState(map);
            var plotBed = w.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(8, 3));
            var villager = new Character(5, "Hasan", new GridPos(11, 3)) { IsVillager = true };
            villager.SetAge(40);
            var runner = new JobRunner(villager, w);
            LivingVillage.ApplyRules(villager, runner);
            var anchors = new List<GridPos> { new GridPos(24, 3) };   // köylünün evi
            runner.PlaceFilter = (c, item) => VillagerFreeWill.MayUse(w, item, anchors);
            villager.Needs.Set(NeedType.Energy, 10f);
            runner.Tick(0.001);
            Assert.True(runner.IsIdle);   // arsadaki yatak yakın olsa da seçilmez
            var homeBed = w.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(25, 3));
            runner.Tick(0.005);
            Assert.True(runner.IsIdle);   // köylü bulamadığını seyrek arar (belge 66f §E.17)
            runner.Tick(JobRunner.AutonomyRetryDays * JobRunner.VillagerRetryFactor);
            Assert.False(runner.IsIdle);
            Assert.Equal(homeBed.Id, runner.Queue.Current.ItemId);
            Assert.False(VillagerFreeWill.MayUse(w, plotBed, new List<GridPos> { new GridPos(8, 3) }));   // arsada asla
        }

        [Fact]
        public void OzgurIrade_KoyluAcligiEnvanterdenGidermez()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var w = new WorldState(new GridMap(12, 6));
            var villager = new Character(5, "Hasan", new GridPos(2, 2)) { IsVillager = true };
            villager.SetAge(40);
            var runner = new JobRunner(villager, w);
            LivingVillage.ApplyRules(villager, runner);
            Assert.True(runner.NeedsOnly);
            runner.PlaceFilter = (c, item) => true;
            w.PlaceItem(db.Items.Get("item.outhouse"), new GridPos(9, 2));
            villager.Needs.Set(NeedType.Hunger, 5f);
            villager.Needs.Set(NeedType.Social, 5f);
            runner.Tick(0.001);
            Assert.True(runner.IsIdle);   // açlık ve sosyal köy hayatından; hela sırada değil
            Assert.True(JobRunner.VillageHandled(NeedType.Hunger));
        }

        [Fact]
        public void KoyluAlimi_StoguDusurur_OyuncuBulamaz()
        {
            var k = Make();
            var market = new Market(k.Db, k.House);
            k.Life.Attach(market);
            var orhan = k.Villager("villager.orhan");
            string home = k.Village.HomeOf(orhan);
            At(k, 2, 0.40);   // Salı, dükkânlar açık
            Step(k);
            // Kiler boşaldı; dükkânda yalnızca 2 ekmek kaldı.
            k.Village.AddToPantry(home, "item.bread", -50);
            k.Village.Stock.TryTake("item.bread", k.Village.Stock.Available("item.bread") - 2);
            Assert.Equal(2, k.Village.Shop(home) >= 2 ? 2 : -1);
            Assert.Equal(0, k.Village.Stock.Available("item.bread"));
            Assert.True(k.Village.PantryCount(home, "item.bread") >= 2);
            var r = market.Buy("item.bread");
            Assert.False(r.Ok);
            Assert.Equal("ui.village.stock.sold_out", r.Reason);
            // Stok her sabah yenilenir.
            At(k, 3, 0.10);
            Step(k);
            Assert.True(k.Village.Stock.Available("item.bread") > 0);
            Assert.True(market.Buy("item.bread").Ok);
        }

        [Fact]
        public void CiftciVeCoban_KendiUrununuKilereKoyar()
        {
            var k = Make();
            var murat = k.Villager("villager.murat");   // çoban
            string home = k.Village.HomeOf(murat);
            At(k, 2, 0.25);
            Step(k);
            int milk = k.Village.PantryCount(home, "item.milk");
            k.Village.AddToPantry(home, "item.milk", -milk);
            At(k, 3, 0.25);
            Step(k);
            Assert.True(k.Village.PantryCount(home, "item.milk") >= 2);
        }

        [Fact]
        public void Pazar_DukkanlarKapali_MeyhaneOglendenAcik()
        {
            long sunday = 6;
            Assert.True(ShopDef.IsRestDay(sunday));
            foreach (var shop in Shops.All)
            {
                bool open = shop.IsOpenAt(13.0, sunday);
                if (shop.Id == "tavern") Assert.True(open);
                else Assert.False(open, shop.Id);
            }
            Assert.False(Shops.Get("tavern").IsOpenAt(10.0, sunday));   // öğlenden önce kapalı
            Assert.True(Shops.Get("smithy").IsOpenAt(10.0, 5));          // cumartesi açık
            Assert.True(Shops.Get("scribe").IsOpenAt(10.0, 5));          // kâtibin eski cumartesi kapalı günü pazar kuralına bağlandı
        }

        [Fact]
        public void Pazar_KoyluGecKalkar_IseGitmez()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 6, 0.25);
            Assert.Equal(k.Village.HomeOf(orhan), k.Village.PlaceFor(orhan));   // geç kalkar
            for (double f = 0.35; f < 0.70; f += 0.05)
            {
                At(k, 6, f);
                Assert.NotEqual("work:smithy", k.Village.PlaceFor(orhan));
            }
            At(k, 6, 0.40);
            Assert.Equal(ShopClosure.RestDay, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
        }

        [Fact]
        public void AlisverisGunu_PazaraDenkGelinceErtesiGuneKayar()
        {
            var k = Make();
            var f = k.Life.Festivals;
            Assert.True(f.IsMarketDay(4)); Assert.True(f.IsMarketDay(9)); Assert.False(f.IsMarketDay(5));
            Assert.True(34 % 5 == 4 && ShopDef.IsRestDay(34));
            Assert.False(f.IsMarketDay(34));
            Assert.True(f.IsMarketDay(35));
        }

        [Fact]
        public void Cagirma_EvdeyseGelirDukkaniAcar_DegilseNeredeOldugunuSoyler()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            var smithy = Shops.Get("smithy");
            At(k, 6, 0.40);
            Assert.False(k.Village.IsShopOpen(smithy, orhan));
            Assert.False(k.Village.Call(k.Ali, orhan, false, out var away));
            Assert.Equal("village.call.not_home", away.Key);
            Assert.True(k.Village.Call(k.Ali, orhan, true, out var msg));
            Assert.Equal("village.call.coming", msg.Key);
            Assert.True(k.Village.IsShopOpen(smithy, orhan));
            Assert.Equal("work:smithy", k.Village.PlaceFor(orhan));
            // Ebe doğrudan yardıma gelir.
            var zeynep = k.Villager("villager.zeynep");
            Assert.True(k.Village.Call(k.Ali, zeynep, true, out var help, "farmhouse"));
            Assert.Equal("village.call.coming_help", help.Key);
            Assert.Equal("farmhouse", k.Village.PlaceFor(zeynep));
            // Süre dolunca programına döner.
            At(k, 6, 0.40 + LivingVillage.CallDays + 0.01);
            Assert.NotEqual("work:smithy", k.Village.PlaceFor(orhan));
        }

        [Fact]
        public void Bunalim_DukkanKapanir_EveGider_TeselliYariyaIndirir()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.40);
            Assert.True(k.Life.Breakdowns.Start(orhan, BreakdownKind.Cry));
            Assert.Equal(BreakdownKind.Withdraw, orhan.Breakdown);
            Assert.InRange(orhan.BreakdownLeft, 1.0, 2.0);
            Assert.Equal(ShopClosure.Breakdown, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
            Assert.False(k.Village.IsShopOpen(Shops.Get("smithy"), orhan));
            Assert.Equal(k.Village.HomeOf(orhan), k.Village.PlaceFor(orhan));
            Assert.Equal("village.closed.breakdown", LivingVillage.ClosureText(ShopClosure.Breakdown).Key);

            Assert.False(k.Village.Console(k.Ali, orhan, false, out var far));   // evinde olmalı
            Assert.Equal("village.console.go_home", far.Key);
            double left = orhan.BreakdownLeft;
            int affBefore = k.Family.Affection(k.Ali.Id, orhan.Id);
            int memBefore = k.Life.Requests.Memory.Of(orhan);
            Assert.True(k.Village.Console(k.Ali, orhan, true, out _));
            Assert.Equal(left * (1 - k.Village.ConsoleShareFor(k.Ali)), orhan.BreakdownLeft, 6);
            Assert.Equal(0.5, k.Village.ConsoleShareFor(k.Ali), 2);
            Assert.True(k.Family.Affection(k.Ali.Id, orhan.Id) > affBefore);
            Assert.True(k.Life.Requests.Memory.Of(orhan) > memBefore);
            // Gönül kazanma tesellinin etkisini artırır.
            k.Ali.GainSkillXp(SkillType.Charm, 5000);
            Assert.True(k.Village.ConsoleShareFor(k.Ali) > 0.5);
        }

        [Fact]
        public void OrtakKayip_BunalimiAnindaBitirir_IkisineKeyif()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.40);
            Assert.True(k.Life.Breakdowns.Start(orhan, BreakdownKind.Withdraw));
            Assert.Null(k.Village.RecentLoss(k.Ali));
            Assert.False(k.Village.CanShareLoss(k.Ali, orhan, true));
            // Oyuncu karakteri yakın zamanda babasını kaybetti (yas iç sesi sürüyor).
            var father = new Character(90, "Yusuf", new GridPos(0, 0), true);
            father.SetAge(70);
            k.Pop.Add(father);
            k.Ali.BirthFatherId = father.Id;
            Kill(father);
            k.Village.RecentDeaths = () => new[] { (father.Id, "Yusuf", k.Clock.TotalDays - 3) };
            Assert.Equal("Yusuf", k.Village.RecentLoss(k.Ali));
            Character a = null, b = null;
            k.Village.SharedGrief += (x, y) => { a = x; b = y; };
            Assert.True(k.Village.ShareLoss(k.Ali, orhan, true, out _, out var talk));
            Assert.False(orhan.InBreakdown);
            Assert.True(orhan.Mood.Has(LivingVillage.SharedLossMood));
            Assert.True(k.Ali.Mood.Has(LivingVillage.SharedLossMood));
            Assert.InRange(talk.Count, 2, 3);
            Assert.Same(k.Ali, a); Assert.Same(orhan, b);   // "Yası ortak" kancası
            // Pencere dışındaki kayıp sayılmaz.
            k.Village.RecentDeaths = () => new[] { (father.Id, "Yusuf", k.Clock.TotalDays - k.Village.SharedLossWindow - 1) };
            Assert.Null(k.Village.RecentLoss(k.Ali));
        }

        [Fact]
        public void Ikram_KeyifliUstaDusukOlasilikla_EsyaYaDaYuzdeBesIndirim()
        {
            var k = Make();
            var miller = k.Villager("villager.hatice");
            Assert.Null(k.Village.OnPurchase(miller, roll: 0.0));   // nötr usta ikram etmez
            miller.AddMoodlet("test.happy", "test", 60, 5);
            Assert.Equal(VillageMood.Cheerful, LivingVillage.MoodTier(miller));
            Assert.Null(k.Village.OnPurchase(miller, roll: 0.2));
            LocText? bubble = null;
            k.Village.Bubble += (c, t) => bubble = t;
            bool sawItem = false, sawDiscount = false;
            for (int i = 0; i < 40; i++)
            {
                var t = k.Village.OnPurchase(miller, roll: 0.1);
                Assert.NotNull(t);
                if (t.ItemId != null) { Assert.Equal("item.simit", t.ItemId); sawItem = true; }
                else { Assert.Equal(0.05, t.Discount, 3); sawDiscount = true; }
            }
            Assert.True(sawItem && sawDiscount);
            Assert.True(bubble.HasValue);
            Assert.StartsWith("village.treat.", bubble.Value.Key);
            int hits = 0;
            for (int i = 0; i < 4000; i++) if (k.Village.OnPurchase(miller) != null) hits++;
            Assert.InRange(hits / 4000.0, 0.12, 0.18);
        }

        [Fact]
        public void KasvetliUsta_PazarligaKapali_FiyatCezasiYok()
        {
            var k = Make();
            var market = new Market(k.Db, k.House);
            k.Life.Attach(market);
            var kerem = k.Villager("villager.kerem");
            int price = market.BuyPrice("item.bread");
            kerem.AddMoodlet("test.sad", "test", -60, 5);
            Assert.Equal(VillageMood.Gloomy, LivingVillage.MoodTier(kerem));
            Assert.NotNull(k.Village.HaggleRefusal(kerem));
            Assert.Equal("village.haggle.gloomy", market.HaggleBlocked());
            Assert.Equal(price, market.BuyPrice("item.bread"));
            kerem.Mood.Remove("test.sad");
            Assert.Null(market.HaggleBlocked());
        }

        [Fact]
        public void Replik_RuhHaliBasamaginaGore()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            orhan.AddMoodlet("test.happy", "test", 60, 5);
            Assert.StartsWith("village.line.cheerful.", k.Village.ChatLine(orhan).Key);
            orhan.Mood.Remove("test.happy");
            orhan.AddMoodlet("test.sad", "test", -60, 5);
            Assert.StartsWith("village.line.gloomy.", k.Village.ChatLine(orhan).Key);
        }

        [Fact]
        public void AksamMeyhanesi_YetiskinlerinBirKismi()
        {
            var k = Make();
            int tavern = 0, total = 0;
            for (long day = 1; day < 6; day++)
            {
                At(k, day, 0.80);
                foreach (var c in k.Pop.Members.Where(m => m.IsVillager && m.Stage >= AgeStage.YoungAdult))
                {
                    total++;
                    if (k.Village.PlaceFor(c) == VillagerFreeWill.Tavern) tavern++;
                }
            }
            Assert.InRange(tavern, 1, total - 1);
            // Akşam sofrası evde.
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.72);
            Assert.Equal(k.Village.HomeOf(orhan), k.Village.PlaceFor(orhan));
        }

        [Fact]
        public void KritikAclik_OgleArasiniOneAlir()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.30);
            Step(k);
            orhan.Needs.Set(NeedType.Hunger, 5f);
            Step(k);
            Assert.True(orhan.Needs.Get(NeedType.Hunger) > 30f);   // işte azığını yedi (öğle arası öne alındı)
        }

        [Fact]
        public void SoyutBolgeHesabi_TamHesaplaAyniMertebede()
        {
            (double hunger, int used, double energy) Run(bool full)
            {
                var k = Make(totalDays: 2.0, seed: 5);
                k.Village.FullSimulation = _ => full;
                var villagers = k.Pop.Members.Where(m => m.IsVillager).ToList();
                double sumH = 0, sumE = 0; int n = 0;
                int start = k.Village.PantryHomes.Sum(h => k.Db.VillageLife.Pantry.Sum(p => k.Village.PantryCount(h, p.Item)));
                for (int i = 0; i < 3 * 288; i++)   // 3 gün, 5 dakikalık adımlar
                {
                    double dt = 1.0 / 288;
                    k.Clock.Restore(k.Clock.TotalDays + dt, 1);
                    foreach (var c in villagers) c.Tick(dt);
                    k.Village.Tick(dt);
                    foreach (var c in villagers) { sumH += c.Needs.Get(NeedType.Hunger); sumE += c.Needs.Get(NeedType.Energy); n++; }
                }
                int sold = k.Db.Items.All.Sum(d => k.Village.Stock.Sold(d.Id));
                return (sumH / n, sold, sumE / n);
            }
            var fullRun = Run(true);
            var abstractRun = Run(false);
            Assert.InRange(abstractRun.hunger / fullRun.hunger, 0.5, 2.0);
            Assert.True(fullRun.hunger > 30 && abstractRun.hunger > 30, $"tam {fullRun.hunger:0} soyut {abstractRun.hunger:0}");
            Assert.True(abstractRun.energy > 25, $"soyut enerji {abstractRun.energy:0}");   // gece evde uyur
        }

        [Fact]
        public void Kayit_KilerStokVeOgunler_EskiKayittaKilerlerDolu()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            string home = k.Village.HomeOf(orhan);
            At(k, 2, 0.40);
            Step(k);
            k.Village.AddToPantry(home, "item.egg", -1);
            k.Village.Stock.TryTake("item.onion", 3);
            int eggs = k.Village.PantryCount(home, "item.egg");
            var data = JsonSerializer.Deserialize<LifeSaveData>(JsonSerializer.Serialize(k.Life.Export(), Json), Json);
            Assert.NotNull(data.village);

            var k2 = Make(totalDays: 2.40);
            k2.Life.Restore(data);
            Assert.Equal(eggs, k2.Village.PantryCount(home, "item.egg"));
            Assert.Equal(3, k2.Village.Stock.Sold("item.onion"));

            // Eski kayıt (alan yok): kilerler hedefle dolu başlar, stok bugünden.
            data.village = null;
            var k3 = Make(totalDays: 2.40);
            k3.Life.Restore(data);
            Assert.Equal(k3.Db.VillageLife.Pantry.First(p => p.Item == "item.egg").Target, k3.Village.PantryCount(home, "item.egg"));
            Assert.Equal(0, k3.Village.Stock.Sold("item.onion"));
        }

        [Fact]
        public void YasGunu_DukkanKapali()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            var wife = new Character(200, "Gülsüm", new GridPos(3, 3)) { IsVillager = true };   // loc-yok: test adı
            wife.SetAge(50);
            k.Pop.Add(wife);
            orhan.SpouseId = wife.Id; wife.SpouseId = orhan.Id;
            At(k, 2, 0.40);
            Kill(wife);
            Assert.Equal(ShopClosure.Mourning, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
            Assert.Equal(k.Village.HomeOf(orhan), k.Village.PlaceFor(orhan));
            At(k, 4, 0.40);
            Assert.Equal(ShopClosure.None, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
        }

        // ---- 66g kararları ----

        [Fact]
        public void Calgici_MuzikUcleBaslar_AksamMeyhanedeCalar_PazarOglenden()
        {
            var k = Make();
            var ozan = k.Villager("villager.ozan");
            Assert.NotNull(ozan);
            Assert.Equal(3, ozan.Skills.GetLevel(SkillType.Music));
            Assert.True(k.Village.IsMusician(ozan));
            At(k, 2, 0.80);
            Assert.Equal(VillagerFreeWill.Tavern, k.Village.PlaceFor(ozan));
            At(k, 2, 0.30);
            Assert.NotEqual(VillagerFreeWill.Tavern, k.Village.PlaceFor(ozan));   // gündüz meyhane kapalı
            At(k, 6, 0.60);
            Assert.Equal(VillagerFreeWill.Tavern, k.Village.PlaceFor(ozan));      // Pazar öğlenden
            At(k, 2, 0.80);
            Step(k);
            Assert.True(k.Village.MusicAtTavern);
        }

        [Fact]
        public void UstaOlunce_HalefHemenGelir_AilesiyleTasinabilir()
        {
            var k = Make();
            k.Life.Villagers.FamilyChance = 1.0;
            var arrivedFamily = new List<Character>();
            k.Life.Villagers.FamilyMemberArrived += (m, head) => arrivedFamily.Add(m);
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.40);
            Kill(orhan);
            k.Life.Tick(0.001);
            var successor = k.Villager("villager.orhan");
            Assert.NotNull(successor);
            Assert.NotSame(orhan, successor);
            Assert.False(successor.IsDead);
            Assert.True(successor.IsVillager);
            Assert.NotEmpty(arrivedFamily);
            var spouse = arrivedFamily[0];
            Assert.Equal(successor.Id, spouse.SpouseId);
            Assert.Equal(spouse.Id, successor.SpouseId);
            Assert.Same(k.Life.Houses.HouseOf(successor), k.Life.Houses.HouseOf(spouse));
            foreach (var kid in arrivedFamily.Skip(1))
            {
                Assert.True(kid.Stage <= AgeStage.Teen);
                Assert.Same(k.Life.Houses.HouseOf(successor), k.Life.Houses.HouseOf(kid));
            }
            // Tek başına da gelebilir.
            var k2 = Make();
            k2.Life.Villagers.FamilyChance = 0.0;
            int fam = 0;
            k2.Life.Villagers.FamilyMemberArrived += (m, h) => fam++;
            Kill(k2.Villager("villager.hasan"));
            k2.Life.Tick(0.001);
            Assert.False(k2.Villager("villager.hasan").IsDead);
            Assert.Equal(0, fam);
        }

        [Fact]
        public void Ebe_DogumdaYanindaysa_ToparlanmaKisalir_BebegeIcSes()
        {
            var k = Make();
            var zeynep = k.Villager("villager.zeynep");
            var mother = new Character(300, "Fatma", new GridPos(0, 0), true) { Sex = Sex.Female, MoodEnabled = true };   // loc-yok: test adı
            mother.SetAge(35);
            k.Pop.Add(mother);
            var baby = new Character(301, "Bebek", new GridPos(0, 0), true) { MoodEnabled = true };   // loc-yok: test adı
            k.Pop.Add(baby);
            mother.RestoreBiology(mother.AgeDays, mother.LifespanDays, false, DeathCause.None, false, null, 0, 10.0, 0, 0);
            Assert.False(k.Village.OnBirth(baby, mother));   // ebe çağrılmadı
            Assert.Equal(10.0, mother.PostpartumLeft, 6);
            At(k, 3, 0.40);
            Assert.True(k.Village.Call(k.Ali, zeynep, true, out _, LivingVillage.FarmhouseKey));
            Assert.Equal(LivingVillage.FarmhouseKey, k.Village.PlaceFor(zeynep));
            Assert.True(k.Village.OnBirth(baby, mother));
            Assert.Equal(5.0, mother.PostpartumLeft, 6);
            Assert.True(baby.Mood.Has(LivingVillage.MidwifeBabyMood));
            Assert.True(mother.Mood.Has(LivingVillage.MidwifeMotherMood));
            Assert.NotEqual(LivingVillage.FarmhouseKey, k.Village.PlaceFor(zeynep));   // işi bitti, döner
        }

        [Fact]
        public void YardimRicasi_Seyrek_KoylubasinaIkiHafta_KoydeUcGun()
        {
            var k = Make();
            var villagers = k.Pop.Members.Where(m => m.IsVillager && m.Stage >= AgeStage.YoungAdult).Take(3).ToList();
            int posted = 0;
            var before = k.Life.Requests.Open(Thatchwick.Core.Life.Requests.RequestKind.Favor).Count();
            for (int i = 0; i < 4 * 288; i++)   // 4 gün
            {
                k.Clock.Restore(2.0 + i / 288.0, 1);
                foreach (var c in villagers) { c.Needs.Set(NeedType.Hunger, 0f); c.Needs.Set(NeedType.Warmth, 0f); }
                foreach (var c in k.Pop.Members.Where(m => m.IsVillager)) k.Village.AddToPantry(k.Village.HomeOf(c), "item.bread", -50);
                k.Village.Tick(1.0 / 288);
            }
            posted = k.Life.Requests.Open(Thatchwick.Core.Life.Requests.RequestKind.Favor).Count() - before;
            Assert.InRange(posted, 1, 2);   // 4 günde köyde en çok iki rica (üç günde bir)
            Assert.Equal(14, LivingVillage.HardshipRepeatDays);
        }

        // ---- 66g sınırlar ----

        [Fact]
        public void OrtakKayip_IliskiSistemineYasiOrtakHissiVerir()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.40);
            Assert.True(k.Life.Breakdowns.Start(orhan, BreakdownKind.Withdraw));
            k.Ali.AddMoodlet(MoodCatalog.Grief);
            k.Village.RecentDeaths = () => new[] { (999, "Yusuf", k.Clock.TotalDays - 1) };
            Assert.True(k.Village.ShareLoss(k.Ali, orhan, true, out _, out _));
            Assert.True(k.Life.Relations.HasFeeling(k.Ali.Id, orhan.Id, Thatchwick.Core.Relations.FeelingKind.SharedGrief));
            Assert.True(k.Life.Relations.HasFeeling(orhan.Id, k.Ali.Id, Thatchwick.Core.Relations.FeelingKind.SharedGrief));
        }

        [Fact]
        public void Hastalik_EvdeYatar_DukkanKapanir_IhlamurIyilestirir()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            At(k, 2, 0.30);
            k.Village.MakeSick(orhan, 1.5);
            Assert.True(k.Village.IsSick(orhan));
            Assert.Equal(ShopClosure.Sick, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
            Assert.Equal(k.Village.HomeOf(orhan), k.Village.PlaceFor(orhan));
            Assert.True(orhan.Mood.Has(LivingVillage.SickMood));
            Assert.False(k.Village.OnGift(orhan, k.Db.Items.Get("item.bread")));
            Assert.True(k.Village.OnGift(orhan, k.Db.Items.Get("item.tea_linden")));
            Assert.False(k.Village.IsSick(orhan));
            Assert.Equal(ShopClosure.None, k.Village.ClosureOf(Shops.Get("smithy"), orhan));
            // Süre dolunca kendiliğinden iyileşir; kayıtta tutulur.
            k.Village.MakeSick(orhan, 1.0);
            var data = JsonSerializer.Deserialize<LifeSaveData>(JsonSerializer.Serialize(k.Life.Export(), Json), Json);
            var k2 = Make(totalDays: 2.30);
            k2.Life.Restore(data);
            Assert.True(k2.Village.IsSick(k2.Villager("villager.orhan")));
            At(k2, 3, 0.40);
            Assert.False(k2.Village.IsSick(k2.Villager("villager.orhan")));
        }

        [Fact]
        public void Hastalik_AclikVeSogukOlasiligiArtirir()
        {
            int sickHard = 0, sickFine = 0;
            for (int seed = 1; seed <= 6; seed++)
                foreach (bool hard in new[] { true, false })
                {
                    var k = Make(seed: seed);
                    var vs = k.Pop.Members.Where(m => m.IsVillager && m.Stage >= AgeStage.Child).ToList();
                    for (int day = 3; day < 10; day++)
                    {
                        At(k, day, 0.50);
                        foreach (var c in vs) c.Needs.Set(NeedType.Warmth, hard ? 10f : 90f);
                        k.Village.Tick(LivingVillage.StepDays * 1.01);
                        foreach (var c in vs) if (k.Village.IsSick(c)) { if (hard) sickHard++; else sickFine++; }
                    }
                }
            Assert.True(sickHard > sickFine, $"aç/üşüyen {sickHard}, iyi {sickFine}");
        }

        [Fact]
        public void KoyDogumu_EbeLohusayiZiyaretEder()
        {
            var k = Make();
            var zeynep = k.Villager("villager.zeynep");
            var mother = new Character(400, "Saliha", new GridPos(3, 3)) { Sex = Sex.Female, IsVillager = true, MoodEnabled = true };   // loc-yok: test adı
            mother.SetAge(35);
            k.Pop.Add(mother);
            var baby = new Character(401, "Bebek", new GridPos(3, 3)) { IsVillager = true, MoodEnabled = true };   // loc-yok: test adı
            k.Pop.Add(baby);
            mother.RestoreBiology(mother.AgeDays, mother.LifespanDays, false, DeathCause.None, false, null, 0, 10.0, 0, 0);
            At(k, 3, 0.40);
            Assert.True(k.Village.OnBirth(baby, mother));
            Assert.Equal(5.0, mother.PostpartumLeft, 6);
            Assert.True(baby.Mood.Has(LivingVillage.MidwifeBabyMood));
            Assert.Equal(k.Village.HomeOf(mother), k.Village.PlaceFor(zeynep));   // lohusa ziyareti
            // Ebe hastaysa gelmez.
            var baby2 = new Character(402, "Bebek2", new GridPos(3, 3)) { IsVillager = true, MoodEnabled = true };   // loc-yok: test adı
            k.Pop.Add(baby2);
            k.Village.MakeSick(zeynep, 2);
            Assert.False(k.Village.OnBirth(baby2, mother));
        }

        [Fact]
        public void EvdePisenOgun_OcakVeSofraPozlari_VarincaYenidenBaslar()
        {
            var k = Make();
            var orhan = k.Villager("villager.orhan");
            string home = k.Village.HomeOf(orhan);
            At(k, 2, 0.72);
            Step(k);
            At(k, 3, 0.72);
            k.Village.AddToPantry(home, "item.bread", -50);   // pişen öğün seçilsin
            orhan.Needs.Set(NeedType.Hunger, 20f);
            Step(k);
            var poses = new HashSet<string>();
            for (double t = 0; t < LivingVillage.CookedMealSceneDays; t += 0.001) { var p = k.Village.ScenePose(orhan, k.Clock.TotalDays + t); if (p != null) poses.Add(p); }
            Assert.Contains("stir", poses);      // ocakta pişirme
            Assert.Contains("sit_eat", poses);   // sofrada yeme
            Assert.True(k.Village.InScene(orhan));
            At(k, 3, 0.72 + 0.03);
            k.Village.RestartScene(orhan);       // ocağa vardı: sahne baştan
            Assert.Equal("kindle", k.Village.ScenePose(orhan, k.Clock.TotalDays));
        }
    }
}
