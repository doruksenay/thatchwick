using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ruh halinin davranışa etkisi (kararlar 18.11.2026, Project belgesi 23): eşikler, sofra çeşidi, pişirme hevesi,
    // uç durumda emir reddi, bunalım, çocukluk ruh halinden özellik.
    public class RuhHaliDavranisTests
    {
        private static ContentDatabase Real()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private sealed class Home
        {
            public ContentDatabase Db; public WorldState World; public Household House; public InteractionEffects Fx;
            public ItemInstance Pot, Bed, Board;
        }

        private static Home MakeHome(ContentDatabase db, int turnips = 40)
        {
            var world = new WorldState(new GridMap(20, 20));
            var house = new Household(0);
            if (turnips > 0) house.AddItem("item.turnip", turnips);
            var h = new Home { Db = db, World = world, House = house, Fx = new InteractionEffects(world, house, db) };
            h.Bed = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(2, 2));
            h.Pot = world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(6, 2));
            var table = world.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(8, 7));
            h.Board = world.PlaceOnSurface(db.Items.Get("item.morris_board"), table.Id, "b");
            return h;
        }

        private static Character Person(int id, string name, double age = 30, bool player = false)
        {
            var c = new Character(id, name, new GridPos(4, 8), player) { MoodEnabled = true, CanStarve = true };
            c.SetAge(age);
            return c;
        }

        private static void NoDecay(Character c)
        {
            foreach (NeedType n in System.Enum.GetValues(typeof(NeedType))) c.Needs.SetDecayPerDay(n, 0f);
        }

        // ---- P0: eşik ----

        [Fact]
        public void AcVeYorgun_15inAltindaAcilir()
        {
            var c = Person(1, "Wat");
            NoDecay(c);
            c.Needs.Set(NeedType.Hunger, 20f);
            c.Needs.Set(NeedType.Energy, 20f);
            c.Tick(0.001);
            Assert.False(c.Mood.Has(MoodCatalog.Hungry));
            Assert.False(c.Mood.Has(MoodCatalog.Tired));

            c.Needs.Set(NeedType.Hunger, 12f);
            c.Needs.Set(NeedType.Energy, 12f);
            c.Tick(0.001);
            Assert.True(c.Mood.Has(MoodCatalog.Hungry));
            Assert.True(c.Mood.Has(MoodCatalog.Tired));
        }

        // ---- P0: sofra çeşidi ----

        [Fact]
        public void Otonomi_BaskaYiyecekVarkenUcKezAyniYemegiYemez()
        {
            var h = MakeHome(Real());
            h.House.AddItem("item.cabbage", 40);
            var c = Person(1, "Agnes");
            var r = new JobRunner(c, h.World) { Autonomous = true, Effects = h.Fx };
            var meals = new List<string>();
            r.InteractionCompleted += (ch, inter) => { if (inter.Need == NeedType.Hunger && inter.ConsumeItem != null) meals.Add(inter.ConsumeItem); };
            for (int i = 0; i < 1500 && meals.Count < 8; i++)
            {
                c.Needs.Set(NeedType.Energy, 100f); c.Needs.Set(NeedType.Fun, 100f); c.Needs.Set(NeedType.Hygiene, 100f);
                c.Needs.Set(NeedType.Social, 100f); c.Needs.Set(NeedType.Bladder, 100f);
                c.Tick(0.01); r.Tick(0.01);
            }
            Assert.True(meals.Count >= 6, $"öğün sayısı {meals.Count}");
            for (int i = 2; i < meals.Count; i++)
                Assert.False(meals[i] == meals[i - 1] && meals[i] == meals[i - 2], string.Join(",", meals));
            Assert.False(c.Mood.Has(MoodCatalog.SameMeal));
        }

        [Fact]
        public void HepAyniYemek_ElindeBaskaYiyecekYoksaGelmez()
        {
            var h = MakeHome(Real());
            var c = Person(1, "Wat");
            c.RecordMeal("item.turnip", hadAlternative: false);
            c.RecordMeal("item.turnip", hadAlternative: false);
            c.RecordMeal("item.turnip", hadAlternative: false);
            Assert.False(c.Mood.Has(MoodCatalog.SameMeal));
            c.RecordMeal("item.turnip", hadAlternative: true);
            Assert.True(c.Mood.Has(MoodCatalog.SameMeal));
        }

        // ---- P0: pişirme hevesi açık kural ----

        [Fact]
        public void PisirmeHevesi_IyiVeMutluKarakterSicakYemegiSecer_NotrVePerisanCiginiYer()
        {
            string Pick(int mood)
            {
                var h = MakeHome(Real());
                var c = Person(1, "Wat");
                if (mood != 0) c.AddMoodlet("test.mood", "test", mood, 99);
                c.Needs.Set(NeedType.Hunger, 20f);
                var r = new JobRunner(c, h.World) { Autonomous = true, Effects = h.Fx };
                r.Tick(0.001);
                return r.Queue.Current?.InteractionId + "|" + r.Queue.Current?.Note;
            }
            Assert.Equal("cook|" + MoodBehavior.EagerCookNote, Pick(+40));
            Assert.StartsWith("cook|", Pick(+15));
            Assert.Equal("eat|", Pick(0));
            Assert.Equal("eat|", Pick(-60));
        }

        // ---- 7.2: uç durumda emir reddi ----

        [Fact]
        public void EmirReddi_KarniBitikkenOyunOynamaz_OnceYer_SonraEmriYapar()
        {
            var h = MakeHome(Real());
            var c = Person(1, "Wat", player: true);
            c.Needs.Set(NeedType.Hunger, 2f);
            var r = new JobRunner(c, h.World) { Effects = h.Fx };
            string said = null;
            r.Refused += (ch, text) => said = text;

            r.Command(Job.UseItem(h.Board.Id, "play"));
            r.Tick(0.001);

            Assert.Equal("Önce bir şeyler yemeliyim.", said);
            Assert.Equal(NeedType.Hunger, r.Queue.Current.Reason);
            Assert.Equal(2, r.Queue.Count);                              // emir silinmedi, sırada
            Assert.Equal("play", r.Queue.Jobs[1].InteractionId);

            bool played = false;
            r.InteractionCompleted += (ch, inter) => { if (inter.Id == "play") played = true; };
            for (int i = 0; i < 300 && !played; i++) { c.Tick(0.01); r.Tick(0.01); }
            Assert.True(played);
            Assert.True(c.Needs.Get(NeedType.Hunger) > 30f);
        }

        [Fact]
        public void EmirReddi_YalnizUcDurumda_VeGiderecekBirSeyYoksaEngellemez()
        {
            // Açlık 10: uç durum değil, emir hemen başlar.
            var h = MakeHome(Real());
            var c = Person(1, "Wat", player: true);
            c.Needs.Set(NeedType.Hunger, 10f);
            var r = new JobRunner(c, h.World) { Effects = h.Fx };
            r.Command(Job.UseItem(h.Board.Id, "play"));
            r.Tick(0.001);
            Assert.Equal("play", r.Queue.Current.InteractionId);
            Assert.Null(r.LastRefusal);

            // Yiyecek yok: reddetmek bir şeyi çözmez, iş yapılır (hasat edip yiyecek bulabilsin).
            var empty = MakeHome(Real(), turnips: 0);
            var d = Person(2, "Agnes", player: true);
            d.Needs.Set(NeedType.Hunger, 1f);
            var rd = new JobRunner(d, empty.World) { Effects = empty.Fx };
            rd.Command(Job.UseItem(empty.Board.Id, "play"));
            rd.Tick(0.001);
            Assert.Equal("play", rd.Queue.Current.InteractionId);

            // İhtiyacı gideren emir reddedilmez.
            var e = Person(3, "Hob", player: true);
            e.Needs.Set(NeedType.Hunger, 1f);
            var re = new JobRunner(e, h.World) { Effects = h.Fx };
            re.Command(Job.UseItem(h.Pot.Id, "eat"));
            re.Tick(0.001);
            Assert.Equal(1, re.Queue.Count);
            Assert.Equal("eat", re.Queue.Current.InteractionId);
            Assert.Null(re.LastRefusal);
        }

        // ---- 7.1: bunalım ----

        [Fact]
        public void Bunalim_EmirSirayaGirer_BitinceIciniDokerVeEmirYapilir()
        {
            var h = MakeHome(Real());
            var c = Person(1, "Wat", player: true);
            var pop = new Population(); pop.Add(c);
            c.AddMoodlet("test.sad", "test", -60, 99);
            Assert.Equal(MoodStatus.Miserable, c.Mood.Status);
            var svc = new BreakdownService(pop, seed: 1);
            Assert.True(svc.Start(c, BreakdownKind.Cry));

            var r = new JobRunner(c, h.World) { Effects = h.Fx };
            r.Tick(0.001);
            Assert.Equal(JobKind.Breakdown, r.Queue.Current.Kind);
            Assert.Equal("Ağlıyor", ActivityDescriber.Describe(c, r, h.World).Text);

            Assert.True(r.Command(Job.UseItem(h.Board.Id, "play")));
            Assert.Equal(JobKind.Breakdown, r.Queue.Current.Kind);       // bunalım sürüyor
            Assert.Equal(2, r.Queue.Count);
            Assert.NotNull(r.LastRefusal);

            // Oyuncu bunalımı iptal edemez ve emri öne alamaz.
            r.CancelAt(0); r.Tick(0.001);
            Assert.Equal(JobKind.Breakdown, r.Queue.Current.Kind);
            r.Prioritize(1); r.Tick(0.001);
            Assert.Equal(JobKind.Breakdown, r.Queue.Current.Kind);

            bool ended = false;
            c.BreakdownEnded += (ch, k, interrupted) => ended = !interrupted;
            for (int i = 0; i < 40 && c.InBreakdown; i++) { c.Tick(0.01); r.Tick(0.01); }
            Assert.False(c.InBreakdown);
            Assert.True(ended);
            Assert.True(c.Mood.Has(MoodCatalog.Catharsis));
            r.Tick(0.001);
            Assert.Equal("play", r.Queue.Current.InteractionId);
        }

        [Fact]
        public void Bunalim_HayatiIhtiyacKritikkenBaslamaz_KritiklesinceBiter()
        {
            var c = Person(1, "Wat");
            var pop = new Population(); pop.Add(c);
            c.AddMoodlet("test.sad", "test", -60, 99);
            var svc = new BreakdownService(pop, seed: 1);

            c.Needs.Set(NeedType.Hunger, 10f);
            Assert.False(svc.IsSafe(c));
            Assert.False(svc.Start(c, BreakdownKind.Withdraw));

            c.Needs.Set(NeedType.Hunger, 80f);
            Assert.True(svc.Start(c, BreakdownKind.Withdraw));
            bool interrupted = false;
            c.BreakdownEnded += (ch, k, i) => interrupted = i;
            c.Needs.Set(NeedType.Hunger, 10f);
            c.Tick(0.001);
            Assert.False(c.InBreakdown);
            Assert.True(interrupted);
            Assert.True(c.Mood.Has(MoodCatalog.Catharsis));
        }

        [Fact]
        public void Bunalim_AcBebeginBaskaBakaniYoksaBaslamaz()
        {
            var pop = new Population();
            var mum = Person(1, "Maud");
            var baby = Person(2, "Bebek", age: 2);
            pop.Add(mum); pop.Add(baby);
            baby.Needs.Set(NeedType.Hunger, 20f);
            var svc = new BreakdownService(pop, seed: 1);
            Assert.False(svc.IsSafe(mum));

            var dad = Person(3, "Wat");
            pop.Add(dad);
            Assert.True(svc.IsSafe(mum));
            dad.AddMoodlet("test.sad", "test", -60, 99);
            Assert.True(svc.Start(dad, BreakdownKind.Cry));
            Assert.False(svc.IsSafe(mum));                                // tek bakan da bunalımda
        }

        [Fact]
        public void Bunalim_TurKurallari_CocukYalnizAglar_MeydanYoksaAglar_KitliktaTikinmaz()
        {
            var pop = new Population();
            var child = Person(1, "Hob", age: 15);
            var adult = Person(2, "Wat");
            var adult2 = Person(3, "Agnes");
            pop.Add(child); pop.Add(adult); pop.Add(adult2);
            var svc = new BreakdownService(pop, seed: 1) { FoodScarce = () => true };
            Assert.True(svc.Start(child, BreakdownKind.Withdraw));
            Assert.Equal(BreakdownKind.Cry, child.Breakdown);
            Assert.True(svc.Start(adult, BreakdownKind.RunOff));
            Assert.Equal(BreakdownKind.Cry, adult.Breakdown);
            Assert.True(svc.Start(adult2, BreakdownKind.Binge));
            Assert.Equal(BreakdownKind.Withdraw, adult2.Breakdown);
        }

        [Fact]
        public void Bunalim_Tikinma_OnceFazladanBirOgunYer()
        {
            var h = MakeHome(Real());
            var c = Person(1, "Wat");
            var pop = new Population(); pop.Add(c);
            var svc = new BreakdownService(pop, seed: 1);
            Assert.True(svc.IsSafe(c));
            Assert.True(c.BeginBreakdown(BreakdownKind.Binge, 1.0));         // uzun tutulur: öğün bitince bunalım sürsün
            var r = new JobRunner(c, h.World) { Autonomous = true, Effects = h.Fx };
            r.Tick(0.001);
            Assert.True(r.Queue.Count == 2, $"kuyruk {r.Queue.Count}: {string.Join(",", r.Queue.Jobs.Select(j => j.Kind + ":" + j.InteractionId))} {r.LastFailure}");
            Assert.Equal(NeedType.Hunger, r.Queue.Current.Reason);
            Assert.Equal(JobKind.Breakdown, r.Queue.Jobs[1].Kind);
            int before = h.House.Count("item.turnip");
            for (int i = 0; i < 30 && h.House.Count("item.turnip") == before; i++) { c.Tick(0.01); r.Tick(0.01); }
            Assert.Equal(before - 1, h.House.Count("item.turnip"));
            Assert.Equal(JobKind.Breakdown, r.Queue.Current.Kind);
        }

        [Fact]
        public void Bunalim_OnceUyarir_YarimGunPerisanKalmadanBaslamaz_AraEnAz2Gun()
        {
            var c = Person(1, "Wat");
            NoDecay(c);
            var pop = new Population(); pop.Add(c);
            var svc = new BreakdownService(pop, seed: 4);
            var notices = new List<string>();
            var starts = new List<double>();
            svc.Raised += (l, t, id) => notices.Add(t);
            svc.Started += (ch, k) => starts.Add(ch.AgeDays);
            c.AddMoodlet("test.sad", "test", -70, 99);
            double start = c.AgeDays;
            for (int i = 0; i < 1000; i++) { c.Tick(0.02); svc.Update(0.02); }   // 20 gün

            Assert.Contains(notices, t => t.Contains("bunalımın eşiğinde"));
            Assert.InRange(starts.Count, 2, 8);
            Assert.True(starts[0] - start >= MoodBehavior.BreakdownGraceDays - 1e-9);
            for (int i = 1; i < starts.Count; i++)
                Assert.True(starts[i] - starts[i - 1] >= MoodBehavior.BreakdownCooldownDays, $"{starts[i - 1]} -> {starts[i]}");
        }

        [Fact]
        public void PerisanVeNotr_AyniTohumlaFarkliDavranir()
        {
            // Bugünkü otonomi kodunda Perişan ile Nötr birebir aynı işleri seçiyordu (belge 23, bölüm 3A).
            List<JobKind> Run(int mood)
            {
                var h = MakeHome(Real());
                var c = Person(1, "Wat");
                var pop = new Population(); pop.Add(c);
                var svc = new BreakdownService(pop, seed: 2);
                if (mood != 0) c.AddMoodlet("test.mood", "test", mood, 99);
                NoDecay(c);                                                      // ihtiyaçlar sabit: fark yalnız ruh halinden
                var r = new JobRunner(c, h.World) { Autonomous = true, Effects = h.Fx };
                var kinds = new List<JobKind>();
                for (int i = 0; i < 1000; i++)
                {
                    c.Tick(0.01); svc.Update(0.01); r.Tick(0.01);
                    if (r.Queue.Current != null) kinds.Add(r.Queue.Current.Kind);
                }
                return kinds;
            }
            var miserable = Run(-60);
            var neutral = Run(0);
            Assert.Contains(JobKind.Breakdown, miserable);
            Assert.DoesNotContain(JobKind.Breakdown, neutral);
        }

        // ---- 7.3: çocukluk ruh hali → özellik ----

        [Fact]
        public void Yetisme_ZorCocuklukNeseliSoyuBozar_HanedanGecmisineYazilir()
        {
            var db = Real();
            var c = Person(1, "Hob", age: 29);
            c.SetTraits(new[] { MoodBehavior.TraitCheerful });
            c.UpbringingMoodSum = -20 * 18; c.UpbringingDays = 18;
            string note = LifeRules.ApplyUpbringing(db, new System.Random(1), c, null, null);
            Assert.Equal("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.", note);
            Assert.True(c.HasTrait(MoodBehavior.TraitGloomy));
            Assert.False(c.HasTrait(MoodBehavior.TraitCheerful));
            Assert.Contains(note, c.Chronicle);
        }

        [Fact]
        public void Yetisme_MutluCocuklukNeseli_ArasiEbeveyndenHuy()
        {
            var db = Real();
            var mother = Person(1, "Maud"); mother.SetTraits(new[] { "trait.hardy" });
            var father = Person(2, "Wat"); father.SetTraits(new[] { "trait.diligent" });

            var happy = Person(3, "Hob", age: 29);
            happy.SetTraits(new[] { "trait.hardy" });
            happy.UpbringingMoodSum = 15 * 18; happy.UpbringingDays = 18;
            Assert.StartsWith("Mutlu bir çocukluk geçirdi", LifeRules.ApplyUpbringing(db, new System.Random(1), happy, mother, father));
            Assert.True(happy.HasTrait(MoodBehavior.TraitCheerful) && happy.HasTrait("trait.hardy"));

            var plain = Person(4, "Edith", age: 29);
            plain.SetTraits(new[] { "trait.hardy" });          // doğumda anneden
            plain.UpbringingMoodSum = 2 * 18; plain.UpbringingDays = 18;
            LifeRules.ApplyUpbringing(db, new System.Random(1), plain, mother, father);
            Assert.Equal(2, plain.TraitIds.Count);
            Assert.True(plain.HasTrait("trait.diligent"));     // ikinci yuva babadan

            // Ölçü yetersiz (eski kayıt): ebeveyn kuralı.
            var old = Person(5, "Osric", age: 29);
            old.SetTraits(new[] { "trait.diligent" });
            old.UpbringingMoodSum = -40; old.UpbringingDays = 1;
            LifeRules.ApplyUpbringing(db, new System.Random(1), old, mother, father);
            Assert.True(old.HasTrait("trait.hardy"));
            Assert.False(old.HasTrait(MoodBehavior.TraitGloomy));
        }

        [Fact]
        public void Yetisme_YalnizCocukVeErgendeOlculur_GencYetiskindeOzellikVerilir()
        {
            var db = Real();
            var clock = new GameClock();
            var world = new WorldState(new GridMap(10, 10));
            var pop = new Population { Content = db };
            var life = new LifeSystems(db, world, new Household(), pop, clock);

            var adult = Person(1, "Wat");
            adult.AddMoodlet("test.mood", "test", 20, 99);
            adult.Tick(1.0);
            Assert.Equal(0.0, adult.UpbringingDays, 6);

            var teen = Person(2, "Hob", age: 28.5);
            teen.SetTraits(new[] { "trait.hardy" });
            pop.Add(teen);
            NoDecay(teen);
            teen.AddMoodlet("test.mood", "test", 25, 99);
            for (int i = 0; i < 10; i++) teen.Tick(0.1);                     // 29,5: hâlâ Ergen
            Assert.InRange(teen.UpbringingDays, 0.99, 1.01);
            Assert.InRange(teen.UpbringingAverage, 24.0, 26.0);
            teen.UpbringingDays = 12; teen.UpbringingMoodSum = 25 * 12;      // çocukluğun tamamı (kısaltılmış)

            for (int i = 0; i < 6; i++) teen.Tick(0.1);                      // 30,1: Genç yetişkin
            Assert.Equal(AgeStage.YoungAdult, teen.Stage);
            Assert.True(teen.HasTrait(MoodBehavior.TraitCheerful));
            Assert.Contains(teen.Chronicle, n => n.Contains("Mutlu bir çocukluk"));
            Assert.Contains(life.Log.Recent(5), e => e.Text.Contains("genç bir yetişkin oldu"));
        }

        [Fact]
        public void Kayit_BunalimYetismeVeHayatNotlariKorunur()
        {
            var s = SaveTests.BuildState();
            var wat = s.Chars[0];
            wat.MoodEnabled = true;
            Assert.True(wat.BeginBreakdown(BreakdownKind.Withdraw, 0.2));
            wat.MiserableDays = 0.7;
            wat.UpbringingMoodSum = -55; wat.UpbringingDays = 11;
            wat.AddChronicle("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.");
            var data = SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market);
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var back = System.Text.Json.JsonSerializer.Deserialize<Thatchwick.Core.Save.SaveData>(json, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var loaded = SaveGame.Load(back, s.Db);
            var w2 = loaded.Characters.First(c => c.Id == wat.Id);
            Assert.Equal(BreakdownKind.Withdraw, w2.Breakdown);
            Assert.Equal(0.2, w2.BreakdownLeft, 6);
            Assert.Equal(0.7, w2.MiserableDays, 6);
            Assert.Equal(-5.0, w2.UpbringingAverage, 6);
            Assert.Equal("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.", Assert.Single(w2.Chronicle));
        }
    }
}
