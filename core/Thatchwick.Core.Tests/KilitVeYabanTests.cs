using System;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Kilitler beceride (28 karar 4), kurt saldırısı, Doğa Defteri, öksüz yavru, avcılık, yarışma kaydı (29, 31).
    public class KilitVeYabanTests
    {
        private sealed class Ctx { public ContentDatabase Db; public WorldState World; public Household House; public AnimalSystem Animals; }

        private static Ctx Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var w = new WorldState(map);
            return new Ctx { Db = db, World = w, House = new Household(5000), Animals = new AnimalSystem(w, db) { DiseaseEnabled = false } };
        }

        private static Animal Put(Ctx x, string species, int id, double age, bool? female = null, int lineage = 0, double xp = 0) =>
            x.Animals.Restore(id, x.Db.Animals.Get(species), new GridPos(2 + id % 10, 2), 0, 100f, age, 0, 0, 50f, female: female, lineage: lineage, xp: xp);

        private static Character Adult(int id, SkillType? skill = null, int level = 1)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(40);
            if (skill.HasValue && level > 1) c.Skills.RestoreXp(skill.Value, SkillRules.XpForLevel(level));
            return c;
        }

        // ---------- kilitler beceride ----------

        [Fact]
        public void Kilit_BeceriSaglayiciVarsaBeceriyeBakar()
        {
            Assert.True(UnlockGate.IsOpen(s => 1, () => 1, SkillType.Carpentry, 1));
            Assert.False(UnlockGate.IsOpen(s => 3, () => 99, SkillType.Carpentry, 4));
            Assert.True(UnlockGate.IsOpen(s => 4, () => 1, SkillType.Carpentry, 4));
            Assert.True(UnlockGate.IsOpen(s => 10, null, SkillType.Carpentry, 15));   // 10 üstü 10 sayılır
            Assert.False(UnlockGate.IsOpen(null, () => 3, SkillType.Carpentry, 4));   // sağlayıcı yoksa eski hesap kuralı
            Assert.Equal("Marangozluk 4 gerekli.", UnlockGate.Text(s => 1, SkillType.Carpentry, 4));
            Assert.Equal("Hesap seviyesi 4 gerekli.", UnlockGate.Text(null, SkillType.Carpentry, 4));
        }

        [Fact]
        public void Kilit_KategoriBecerisi()
        {
            var x = Make();
            Assert.Equal(SkillType.Carpentry, UnlockGate.SkillFor(x.Db.Items.Get("item.chair_carved")));
            Assert.Equal(SkillType.Weaving, UnlockGate.SkillFor(x.Db.Items.Get("item.rug_wool")));
            Assert.Equal(SkillType.Chandlery, UnlockGate.SkillFor(x.Db.Items.Get("item.candlestick")));
            Assert.Equal(SkillType.Decorating, UnlockGate.SkillFor(x.Db.Items.Get("item.tapestry")));
        }

        [Fact]
        public void Kilit_SatinAlmaBeceriIster()
        {
            var x = Make();
            int carpentry = 1;
            var svc = new BuildService(x.World, x.House, x.Db) { SkillLevel = s => s == SkillType.Carpentry ? carpentry : 1, AccountLevel = () => 99 };
            var def = x.Db.Items.Get("item.chair_carved");   // eski seviye 10
            var r = svc.Buy(def.Id);
            Assert.False(r.Ok);
            Assert.Equal("Marangozluk 10 gerekli.", r.Reason);
            carpentry = 10;
            Assert.True(svc.Buy(def.Id).Ok);
        }

        [Fact]
        public void Kilit_HayvanHayvancilikIster()
        {
            var x = Make();
            x.Animals.SkillLevel = s => 1;
            var r = x.Animals.Buy("animal.cow", x.House, out _);
            Assert.False(r.Ok);
            Assert.Contains("Hayvancılık", r.Reason);
        }

        // ---------- kurt ----------

        [Fact]
        public void Kurt_SuruyleBuyur_IcerdeykenGuvende()
        {
            var x = Make();
            var wolves = new WolfSystem(x.Animals, 1);
            for (int i = 0; i < 13; i++) Put(x, "animal.sheep", 10 + i, 30);
            Assert.Equal(13, wolves.Exposed().Count);
            Assert.Equal(3, wolves.PackSize(13, Season.Autumn));
            Assert.Equal(4, wolves.PackSize(13, Season.Winter));
            Assert.Equal(WolfSystem.MaxPack, wolves.PackSize(100, Season.Winter));
            x.Animals.Released = false;
            var r = wolves.Raid(Season.Winter);
            Assert.Equal(0, r.Pack);
            Assert.Empty(r.Killed);
        }

        [Fact]
        public void Kurt_SavunmaYetmezseOnceYavruOlur()
        {
            var x = Make();
            var wolves = new WolfSystem(x.Animals, 1);
            var lamb = Put(x, "animal.sheep", 10, 0.5);
            for (int i = 0; i < 6; i++) Put(x, "animal.sheep", 11 + i, 30 + i);
            // 7 koyun, kış: 1 + 7/6 + 1 = 3 kurt; köpek yok, çoban yok → 3 ölüm, önce kuzu.
            var r = wolves.Raid(Season.Winter);
            Assert.Equal(3, r.Pack);
            Assert.Equal(3, r.Killed.Count);
            Assert.Same(lamb, r.Killed[0]);
            Assert.True(lamb.IsDead);
            Assert.Equal(AnimalDeath.Predator, lamb.CauseOfDeath);
        }

        [Fact]
        public void Kurt_KopekCobanVeCitKovar()
        {
            var x = Make();
            var wolves = new WolfSystem(x.Animals, 1) { Shepherds = () => 1, HasFire = () => true };
            for (int i = 0; i < 7; i++) Put(x, "animal.sheep", 10 + i, 30);
            Put(x, "animal.dog", 40, 30, lineage: (int)AnimalLineage.Noble, xp: AnimalProgress.XpForLevel(3));   // 2 kurt
            Assert.Equal(4, wolves.DefenseScore());   // köpek 2 + çoban 1 + ateş 1
            var r = wolves.Raid(Season.Winter);
            Assert.True(r.Repelled);
            // Çit yarısını oyalar: 6 kurtluk sürüde köpeksiz bile 3'e iner.
            var y = Make();
            var w2 = new WolfSystem(y.Animals, 1) { Fenced = () => true };
            for (int i = 0; i < 40; i++) Put(y, "animal.sheep", 10 + i, 30);
            Assert.Equal((3, 0), w2.Risk(Season.Winter));
            Assert.Equal(3, w2.Raid(Season.Winter).Killed.Count);
            // Avlanan kurt bu mevsim sürüyü küçültür.
            w2.OnWolfHunted();
            Assert.Equal(5, w2.PackSize(40, Season.Winter));
        }

        [Fact]
        public void Kurt_UyariVeGeriSayim()
        {
            var x = Make();
            for (int i = 0; i < 6; i++) Put(x, "animal.sheep", 10 + i, 30);
            var wolves = new WolfSystem(x.Animals, 5);
            string warning = null; WolfRaidResult raid = null;
            wolves.Warning += t => warning = t;
            wolves.Raided += r => raid = r;
            Assert.Null(wolves.OnNewDay(Season.Summer, 3, 10));
            Assert.Equal(-1, wolves.RaidInDays);
            for (int d = 0; d < 60 && raid == null; d++) wolves.OnNewDay(Season.Winter, d % 10, 10);
            Assert.NotNull(warning);
            Assert.NotNull(raid);
            Assert.True(WolfSystem.IsWolfSeason(Season.Autumn, 5, 10));
            Assert.False(WolfSystem.IsWolfSeason(Season.Autumn, 4, 10));
        }

        // ---------- defter, yavru, av ----------

        [Fact]
        public void Defter_IsaretlerVeKayit()
        {
            var j = new NatureJournal();
            Assert.True(j.Mark("wild.fox", JournalMark.Seen));
            Assert.False(j.Mark("wild.fox", JournalMark.Seen));
            Assert.True(j.Mark("wild.fox", JournalMark.Hunted));
            Assert.False(j.Mark("wild.unicorn", JournalMark.Seen));
            Assert.Equal(1, j.SeenCount);
            var back = new NatureJournal();
            back.Import(j.Export());
            Assert.Equal(JournalMark.Seen | JournalMark.Hunted, back.Get("wild.fox"));
            Assert.True(NatureJournal.Total >= 14);
        }

        [Fact]
        public void Yavru_BulunurSahiplenilirKayitlanir()
        {
            var ad = new WildAdoptions(7);
            WildSpecies found = null;
            for (int d = 0; d < 400 && found == null; d++) found = ad.OnNewDay(Season.Spring);
            Assert.NotNull(found);
            Assert.Null(ad.OnNewDay(Season.Spring));   // bekleyen varken yenisi gelmez
            var j = new NatureJournal();
            Assert.True(ad.Decide(true, j));
            Assert.True(ad.Has(found.Id));
            Assert.True((j.Get(found.Id) & JournalMark.Adopted) != 0);
            var back = new WildAdoptions();
            back.Import(ad.Export());
            Assert.True(back.Has(found.Id));
            Assert.Null(ad.OnNewDay(Season.Winter));
        }

        [Fact]
        public void Av_AyiAvlanmaz_TuzakVeYay()
        {
            var hunter = Adult(1, SkillType.Hunting, 5);
            var bear = Hunting.Try(new Random(1), "wild.bear", HuntMethod.Bow, hunter);
            Assert.False(bear.Success);
            Assert.Contains("Ayı", bear.Message);
            Assert.Equal(0, Hunting.Chance(WildCatalog.Get("wild.roe_deer"), HuntMethod.Trap, 5, false));   // karaca tuzağa düşmez
            Assert.True(Hunting.Chance(WildCatalog.Get("wild.rabbit"), HuntMethod.Bow, 5, true) > Hunting.Chance(WildCatalog.Get("wild.rabbit"), HuntMethod.Bow, 1, false));
            bool got = false;
            for (int s = 0; s < 50 && !got; s++) got = Hunting.Try(new Random(s), "wild.rabbit", HuntMethod.Trap, hunter).Success;
            Assert.True(got);
        }

        // ---------- yarışma kaydı ----------

        [Fact]
        public void Yarisma_KayitUcretVeSenlikGunuKosar()
        {
            var x = Make();
            var h = Adult(1, SkillType.Riding, 6);
            var horse = x.Animals.Restore(50, x.Db.Animals.Get("animal.horse"), new GridPos(4, 4), 0, 100f, 40, 0, 0, 80f, 0, "item.saddle",
                lineage: (int)AnimalLineage.Noble, xp: 10000);
            var book = new ContestBook(3);
            int before = x.House.Akce;
            Assert.Null(book.Register(ContestKind.HorseRace, horse, h, x.House));
            Assert.Equal(before - AnimalContests.EntryFee, x.House.Akce);
            Assert.NotNull(book.Register(ContestKind.HorseRace, horse, h, x.House));   // ikinci kez olmaz
            Assert.Empty(book.RunFestival("festival.harvest", 0, id => horse, id => h, x.House));   // at yarışı hasatta değil
            var back = new ContestBook(); back.Import(book.Export());
            Assert.True(back.IsRegistered(horse));
            var results = book.RunFestival("festival.fair", 0, id => horse, id => h, x.House);
            Assert.Single(results);
            Assert.False(book.IsRegistered(horse));
            Assert.Contains(results[0].Summary, h.Chronicle);
        }
    }
}
