using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 42 kalanı (18.11.2026): hayvan hastalığı ve kış etkisi.
    public class HayvanSagligiTests
    {
        private sealed class Farm
        {
            public ContentDatabase Db; public WorldState W; public Household H; public AnimalSystem A; public InteractionEffects Fx;
        }

        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        // Dış mekânda hazır kümes (kapalı oda değil).
        private static (Farm f, ItemInstance coop) OutdoorCoop()
        {
            var db = Db();
            var f = new Farm { Db = db, W = new WorldState(new GridMap(16, 16)), H = new Household(5000) };
            var coop = f.W.PlaceItem(db.Items.Get("item.coop"), new GridPos(5, 5));
            f.A = new AnimalSystem(f.W, db, seed: 5) { Released = false };   // otlağa çıkmaz: yalnızca kümes yemi
            f.Fx = new InteractionEffects(f.W, f.H, db, seed: 3) { Animals = f.A };
            return (f, coop);
        }

        // Kapalı ahır odası (oda şablonu).
        private static (Farm f, ItemInstance stall) Barn()
        {
            var db = Db();
            var f = new Farm { Db = db, W = new WorldState(new GridMap(24, 24)), H = new Household(20000) };
            var svc = new BuildService(f.W, f.H, db);
            Assert.True(new RoomTemplateService(svc, f.W, f.H, db).Build("template.barn_room", new GridPos(2, 2), out _).Ok);
            f.A = new AnimalSystem(f.W, db, seed: 5);
            f.Fx = new InteractionEffects(f.W, f.H, db, seed: 3) { Animals = f.A };
            return (f, f.W.Items.First(i => i.Def.Id == "item.stall"));
        }

        // Tavuk ömrü ~14 gün, 3 günde erişkin: testler 4 günlük tavukla başlar.
        private static Animal Hen(Farm f, ItemInstance coop, int id, float friendship = 50f, double age = 4) =>
            f.A.Restore(id, f.Db.Animals.Get("animal.chicken"), coop.Position + new GridPos(1, 0), coop.Id, 100f, age, 0, 0, friendship);

        // ---------- kış ----------

        [Fact]
        public void Kis_YemIhtiyaciArtar()
        {
            var (f, coop) = OutdoorCoop();
            var hen = Hen(f, coop, 1);
            var season = Season.Spring;
            f.A.SeasonNow = () => season;
            AnimalSystem.AddFeed(coop, 100);
            double s0 = coop.FeedStock;
            f.A.Tick(1.0);
            double spring = s0 - coop.FeedStock;
            double w0 = coop.FeedStock;
            season = Season.Winter;
            f.A.Tick(1.0);
            double winter = w0 - coop.FeedStock;
            Assert.True(spring > 0);
            Assert.Equal(spring * AnimalSystem.WinterFeedMultiplier, winter, 6);
        }

        [Fact]
        public void Kis_AciktakiKumesteHayvanUsur_UrunYavaslar_DostlukAzalir()
        {
            var (f, coop) = OutdoorCoop();
            f.A.DiseaseEnabled = false;
            var warm = Hen(f, coop, 1);
            f.A.OnNewDay(Season.Autumn);
            Assert.False(warm.IsCold);

            var report = f.A.OnNewDay(Season.Winter);
            Assert.True(warm.IsCold);
            Assert.Equal(1, report.Cold);
            Assert.Equal("Üşüyor", warm.HealthText);
            float friendBefore = warm.Friendship;
            AnimalSystem.AddFeed(coop, 100);
            f.A.Tick(0.5);
            Assert.True(warm.Friendship < friendBefore);

            // Isı sistemi bağlıysa sıcaklığa bakılır: ılık barınakta kışın da üşümez.
            f.A.Temperature = _ => 8f;
            f.A.OnNewDay(Season.Winter);
            Assert.False(warm.IsCold);
            Assert.Equal(0, warm.ColdDays);
            f.A.Temperature = _ => -6f;
            f.A.OnNewDay(Season.Spring);
            Assert.True(warm.IsCold);
        }

        [Fact]
        public void Kis_UsuyenHayvanDahaAzUrunVerir()
        {
            int Eggs(bool cold)
            {
                var (f, coop) = OutdoorCoop();
                f.A.DiseaseEnabled = false;
                var hen = Hen(f, coop, 1);
                f.A.Temperature = _ => cold ? -5f : 10f;
                f.A.OnNewDay(Season.Spring);
                AnimalSystem.AddFeed(coop, 1000);
                for (int i = 0; i < 24; i++) f.A.Tick(0.25);
                return coop.Stock.TryGetValue("item.egg", out var n) ? n : 0;
            }
            int warm = Eggs(false), cold = Eggs(true);
            Assert.True(warm > 0);
            Assert.True(cold < warm, $"soğuk {cold}, ılık {warm}");
        }

        // ---------- hastalık ----------

        [Fact]
        public void Hasta_UrunVermez_Yavrulamaz()
        {
            var (f, coop) = OutdoorCoop();
            f.A.DiseaseEnabled = false;
            var a = Hen(f, coop, 1, 90f);
            var b = Hen(f, coop, 2, 90f);
            f.A.MakeSick(a); f.A.MakeSick(b);
            Assert.Equal("Hasta (0 gün)", a.HealthText);
            AnimalSystem.AddFeed(coop, 100);
            f.A.Tick(3.0);
            Assert.False(coop.Stock.ContainsKey("item.egg") && coop.Stock["item.egg"] > 0);
            f.A.HusbandryLevel = () => 10;
            int births = 0;
            f.A.Born += (_, __) => births++;
            // Hasta çift yavrulamaz (iyileşenler hariç): iyileşmeden önceki günlerde doğum olmamalı.
            for (int d = 0; d < 3 && a.IsSick && b.IsSick; d++) f.A.OnNewDay(Season.Spring);
            if (a.IsSick && b.IsSick) Assert.Equal(0, births);
        }

        [Fact]
        public void Hastalik_RiskleriToplanir_UstalikAzaltir_Bulasir()
        {
            var (f, coop) = OutdoorCoop();
            f.A.DiseaseEnabled = false;
            var a = Hen(f, coop, 1);
            double healthy = f.A.SickChance(a, 0, 1);
            Assert.Equal(AnimalSystem.SickBaseChance, healthy, 9);
            f.A.OnNewDay(Season.Winter);   // açıktaki kümeste kış: üşür
            Assert.True(a.IsCold);
            Assert.True(f.A.SickChance(a, 0, 1) > healthy + 0.02);
            Assert.True(f.A.SickChance(a, 2, 1) > f.A.SickChance(a, 0, 1) + 0.09);   // iki hasta komşu
            Assert.True(f.A.SickChance(a, 2, 10) < f.A.SickChance(a, 2, 1) * 0.6);   // usta hayvancı
        }

        [Fact]
        public void Hastalik_TedavisizUzarsaOldurur()
        {
            var (f, coop) = OutdoorCoop();
            f.A.HealthSeed = 11;
            var hens = Enumerable.Range(1, 6).Select(i => Hen(f, coop, i, age: 1)).ToList();   // yaşlılıktan ölmesin
            foreach (var h in hens) f.A.MakeSick(h);
            int deaths = 0;
            for (int d = 0; d < AnimalSystem.SickDeathDays + 1; d++) deaths += f.A.OnNewDay(Season.Winter).DiedOfSickness;
            Assert.True(deaths > 0);
            Assert.All(hens.Where(h => h.IsDead), h => Assert.Equal(AnimalDeath.Sickness, h.CauseOfDeath));
        }

        [Fact]
        public void Merhem_HastaHayvaniIyilestirir()
        {
            var (f, stall) = Barn();
            var cow = f.A.Restore(1, f.Db.Animals.Get("animal.cow"), new GridPos(3, 4), stall.Id, 100f, 30, 0, 0);
            f.A.MakeSick(cow);
            int recovered = 0;
            f.A.Recovered += _ => recovered++;
            var salve = f.Db.Items.Get("item.stall").Interactions.Single(i => i.Id == "salve");
            f.H.AddItem("item.animal_salve");
            var c = new Character(9, "Murat", new GridPos(3, 3), true);
            c.SetAge(45);
            f.Fx.Apply(c, stall, salve);
            Assert.False(cow.IsSick);
            Assert.Equal(1, recovered);
        }

        [Fact]
        public void Hastalik_KapaliysaKimseHastalanmaz()
        {
            var (f, coop) = OutdoorCoop();
            f.A.DiseaseEnabled = false;
            var hens = Enumerable.Range(1, 6).Select(i => Hen(f, coop, i)).ToList();
            for (int d = 0; d < 60; d++) Assert.Equal(0, f.A.OnNewDay(Season.Winter).Sickened);
        }

        [Fact]
        public void Kayit_SaglikAlanlariGeriGelir()
        {
            var (f, coop) = OutdoorCoop();
            var a = f.A.Restore(7, f.Db.Animals.Get("animal.chicken"), coop.Position, coop.Id, 80f, 30, 0, 0, 40f,
                sick: true, sickDays: 3, coldDays: 2);
            Assert.True(a.IsSick);
            Assert.Equal(3.0, a.SickDays);
            Assert.True(a.IsCold);
            Assert.Equal(2, a.ColdDays);
        }
    }
}
