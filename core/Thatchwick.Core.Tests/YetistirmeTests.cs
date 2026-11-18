using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Upbringing;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 30 (çocukluk eşyaları), 31-8 (can yoldaşı), 34 D1/D3/D4 (çıraklık, aile tarifi, itibar, yaşlı öğretmen).
    public class YetistirmeTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static Character Person(int id, double age, int x = 1, int y = 1)
        {
            var c = new Character(id, "K" + id, new GridPos(x, y));
            c.SetAge(age);
            return c;
        }

        private static void Kill(Character c, Population pop = null)
        {
            c.LifespanDays = c.AgeDays + 0.0001;
            if (pop != null) pop.Tick(0.01); else c.Tick(0.01);
            Assert.True(c.IsDead);
        }

        private const double ChildAge = 14, TeenAge = 25, AdultAge = 45, ElderAge = 80;

        // ---------- oyuncak alışkanlığı ----------

        [Fact]
        public void Oyuncak_EsikteUstalasir_KisiselEsyaOlur_GosteriYapar()
        {
            var t = new ToyHabitSystem();
            var kid = Person(1, ChildAge);
            ToyDef mastered = null;
            t.Mastered += (c, d) => mastered = d;
            for (int i = 0; i < 24; i++) Assert.False(t.OnPlayed(kid, "item.spinning_top", i * 0.1));
            Assert.Null(t.Personal(1, "item.spinning_top"));
            Assert.Null(t.PickIdleShow(kid, new System.Random(1)));
            Assert.True(t.OnPlayed(kid, "item.spinning_top", 3));   // 25. oynayış: 100
            Assert.NotNull(mastered);
            Assert.NotNull(t.Personal(1, "item.spinning_top"));
            var show = t.PickIdleShow(kid, new System.Random(1));
            Assert.NotNull(show);
            Assert.Equal(ShowKind.Show, show.Kind);

            var watcher = Person(2, ChildAge);
            watcher.Needs.Set(NeedType.Fun, 10);
            Assert.Equal(1, t.ApplyShow(show, new[] { watcher, kid }, 3));
            Assert.True(watcher.Needs.Get(NeedType.Fun) > 10);
        }

        [Fact]
        public void Oyuncak_BebekOynamaz_YetiskinYavasOgrenir_OyuncakOlmayanSayilmaz()
        {
            var t = new ToyHabitSystem();
            t.OnPlayed(Person(1, 2), "item.spinning_top", 0);
            Assert.Equal(0, t.HabitOf(1, "item.spinning_top"));
            t.OnPlayed(Person(2, AdultAge), "item.spinning_top", 0);
            Assert.Equal(4 * 0.3, t.HabitOf(2, "item.spinning_top"), 6);
            Assert.False(t.OnPlayed(Person(3, ChildAge), "item.bed_straw", 0));
            Assert.False(t.IsToy("item.bed_straw"));
        }

        [Fact]
        public void Oyuncak_UstaYaninda_IkiKatHizli_YasliUstaDahaHizli()
        {
            var t = new ToyHabitSystem();
            var master = Person(10, AdultAge);
            var elder = Person(11, ElderAge);
            for (int i = 0; i < 100; i++) { t.OnPlayed(master, "item.kite", 0); t.OnPlayed(elder, "item.kite", 0); }
            Assert.True(t.IsMastered(10, "item.kite"));
            var a = Person(1, ChildAge); var b = Person(2, ChildAge);
            t.OnPlayed(a, "item.kite", 0, master);
            t.OnPlayed(b, "item.kite", 0, elder);
            Assert.Equal(8, t.HabitOf(1, "item.kite"), 6);
            Assert.Equal(12, t.HabitOf(2, "item.kite"), 6);
            Assert.True(master.Skills.GetXp(SkillType.Teaching) > 0);
        }

        [Fact]
        public void Oyuncak_Unutma_UstalikKalir_TabanYarisi()
        {
            var t = new ToyHabitSystem();
            var kid = Person(1, ChildAge);
            for (int i = 0; i < 25; i++) t.OnPlayed(kid, "item.marbles", 0);
            Assert.True(t.IsMastered(1, "item.marbles"));
            for (int d = 1; d <= 200; d++) t.DailyUpdate(d);
            Assert.True(t.IsMastered(1, "item.marbles"));
            Assert.Equal(50, t.HabitOf(1, "item.marbles"), 6);
        }

        [Fact]
        public void Oyuncak_Asinir_Kirilir_Onarilir_KirikkenGosteriYok()
        {
            var t = new ToyHabitSystem();
            var kid = Person(1, ChildAge);
            for (int i = 0; i < 25; i++) t.OnPlayed(kid, "item.whittling_knife", 0);
            var p = t.Personal(1, "item.whittling_knife");
            bool broke = false;
            t.Broke += (c, pt) => broke = true;
            for (int i = 0; i < 100 && !p.IsBroken; i++) t.OnPlayed(kid, "item.whittling_knife", 0);
            Assert.True(broke);
            Assert.Null(t.PickIdleShow(kid, new System.Random(1)));
            Assert.True(t.Repair(1, "item.whittling_knife"));
            Assert.NotNull(t.PickIdleShow(kid, new System.Random(1)));
        }

        [Fact]
        public void Oyuncak_YasaGoreGosteri_YetiskinOgretir_YasliNostalji()
        {
            var t = new ToyHabitSystem();
            var adult = Person(1, AdultAge);
            for (int i = 0; i < 100; i++) t.OnPlayed(adult, "item.wooden_whistle", 0);
            Assert.Equal(ShowKind.Show, t.PickIdleShow(adult, null, childNearby: false).Kind);
            var teach = t.PickIdleShow(adult, null, childNearby: true);
            Assert.Equal(ShowKind.Teach, teach.Kind);
            var kid = Person(2, ChildAge);
            t.ApplyShow(teach, new[] { kid }, 0);
            Assert.Equal(8, t.HabitOf(2, "item.wooden_whistle"), 6);   // seyreden çocuk öğrenir (öğretme x2)
            adult.SetAge(ElderAge);
            Assert.Equal(ShowKind.Nostalgia, t.PickIdleShow(adult, null, true).Kind);
        }

        [Fact]
        public void Oyuncak_GencYetiskinlikteIz_XpBonus_Miras()
        {
            var t = new ToyHabitSystem();
            var kid = Person(1, ChildAge);
            for (int i = 0; i < 25; i++) t.OnPlayed(kid, "item.wooden_whistle", 0);
            Assert.Equal(1.0, t.XpFactor(1, SkillType.Music), 6);
            var marks = t.OnGrownUp(kid);
            Assert.Single(marks);
            Assert.Contains("mark.music_soul", t.MarksOf(1));
            Assert.Equal(1.1, t.XpFactor(1, SkillType.Music), 6);
            Assert.Empty(t.OnGrownUp(kid));   // ikinci kez eklenmez

            var heir = Person(2, ChildAge);
            Assert.Equal(1, t.Inherit(kid, heir, 5));
            var p = t.Personal(2, "item.wooden_whistle");
            Assert.True(p.IsHeirloom);
            Assert.Equal(1, p.OriginalOwnerId);
            Assert.Equal(25, t.HabitOf(2, "item.wooden_whistle"), 6);
            Assert.Null(t.Personal(1, "item.wooden_whistle"));
        }

        [Fact]
        public void Oyuncak_OduncVerme()
        {
            var t = new ToyHabitSystem();
            var owner = Person(1, ChildAge); var asker = Person(2, ChildAge);
            for (int i = 0; i < 25; i++) t.OnPlayed(owner, "item.hoop", 0);
            t.AskToBorrow(owner, asker, "item.hoop", lend: true, today: 1);
            Assert.Equal(4, t.HabitOf(2, "item.hoop"), 6);
            Assert.True(owner.Skills.GetXp(SkillType.Charm) > 0);
            Assert.NotNull(t.Personal(1, "item.hoop"));   // eşya sahibinde kalır
        }

        [Fact]
        public void Oyuncak_KayitGeriYukleme()
        {
            var t = new ToyHabitSystem();
            var kid = Person(1, ChildAge);
            for (int i = 0; i < 25; i++) t.OnPlayed(kid, "item.rag_doll", 2);
            t.OnGrownUp(kid);
            var json = JsonSerializer.Serialize(t.Export(), Json);
            var t2 = new ToyHabitSystem();
            t2.Import(JsonSerializer.Deserialize<ToyHabitSave>(json, Json));
            Assert.True(t2.IsMastered(1, "item.rag_doll"));
            Assert.NotNull(t2.Personal(1, "item.rag_doll"));
            Assert.Contains("mark.caring", t2.MarksOf(1));
        }

        [Fact]
        public void Icerik_OyuncakEsyalariVarVeOynanir()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            foreach (var d in ToyHabitSystem.DefaultToys())
            {
                Assert.True(db.Items.TryGet(d.ItemId, out var item), d.ItemId);
                Assert.True(item.Interactions.Count > 0, d.ItemId + " oynanmıyor");
            }
            foreach (var id in new[] { "item.tallow", "item.parchment", "item.ink", "item.blank_book", "item.malt", "item.oak_gall" })
                Assert.True(db.Items.TryGet(id, out _), id);
        }

        // ---------- can yoldaşı ----------

        private sealed class Farm
        {
            public ContentDatabase Db; public WorldState World; public Household House; public AnimalSystem Animals; public Population Pop;
        }

        private static Farm MakeFarm()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(20, 20);
            for (int y = 0; y < 20; y++) for (int x = 0; x < 20; x++) map.SetOwned(new GridPos(x, y), true);
            var f = new Farm { Db = db, World = new WorldState(map), House = new Household(20000), Pop = new Population() };
            f.Animals = new AnimalSystem(f.World, db) { DiseaseEnabled = false };
            f.World.PlaceItem(db.Items.Get("item.barn"), new GridPos(5, 5));
            return f;
        }

        private static Animal Make(Farm f, string species, int id) =>
            new Animal(id, f.Db.Animals.Get(species), new GridPos(8, 8), 0);

        [Fact]
        public void CanYoldasi_EsikVeSure_ErgenliktenAcilir_AtDoğustanBinici()
        {
            var f = MakeFarm();
            var horse = Make(f, "animal.horse", 101);
            Assert.True(f.Animals.Buy("animal.cow", f.House, out var cow).Ok);
            double today = 0;
            var s = new CompanionBondSystem(f.Pop, f.Animals) { Today = () => today };
            var kid = Person(1, ChildAge); var parent = Person(2, AdultAge);
            f.Pop.Add(kid); f.Pop.Add(parent);
            CompanionBond unlocked = null; CompanionTrait got = CompanionTrait.None;
            s.TraitUnlocked += (c, b, t) => { unlocked = b; got = t; };

            for (int d = 0; d <= 3; d++) { today = d; s.ParentAssist(parent, kid, horse); s.OnCare(kid, horse, AnimalAction.Pet); s.OnCare(kid, cow, AnimalAction.Pet); }
            Assert.Null(s.OnBecameTeen(kid));   // süre yetmedi (3 gün < 6)
            for (int d = 4; d <= 8; d++) { today = d; s.ParentAssist(parent, kid, horse); s.OnCare(kid, horse, AnimalAction.Brush); s.OnCare(kid, cow, AnimalAction.Pet); }
            var b = s.Get(1, horse.Id);
            Assert.True(b.Points >= CompanionBondSystem.Threshold);
            Assert.Equal(9, b.ParentAssists);
            Assert.Same(b, s.OnBecameTeen(kid));
            Assert.Equal(CompanionTrait.BornRider, got);
            Assert.True(s.Has(1, CompanionTrait.BornRider));
            Assert.Equal(1.1, s.RideSpeedFactor(1), 6);
            Assert.Equal(1.15, s.XpFactor(1, SkillType.Riding), 6);
            Assert.True(parent.Skills.GetXp(SkillType.Parenting) > 0);
        }

        [Fact]
        public void CanYoldasi_YalnizcaCocukBagKurar_YakinlikOrneklemesi()
        {
            var f = MakeFarm();
            Assert.True(f.Animals.Buy("animal.cow", f.House, out var cat).Ok);
            var s = new CompanionBondSystem(f.Pop, f.Animals);
            var adult = Person(1, AdultAge);
            Assert.Null(s.AddContact(adult, cat, 5));
            var baby = Person(2, 3, cat.Position.X, cat.Position.Y);
            f.Pop.Add(baby);
            for (int i = 0; i < 10; i++) s.Tick(0.1);
            Assert.Equal(10 * CompanionBondSystem.NearPerSample, s.Get(2, cat.Id).Points, 6);
        }

        [Fact]
        public void CanYoldasi_HayvanOlunceAniKalir_Yas_YavruYarimPuan()
        {
            var f = MakeFarm();
            var dog = Make(f, "animal.dog", 102);
            double today = 0;
            var s = new CompanionBondSystem(f.Pop, f.Animals) { Today = () => today };
            var mom = Person(1, ChildAge); f.Pop.Add(mom);
            for (int d = 0; d <= 10; d++) { today = d; s.AddContact(mom, dog, 12); }
            Assert.NotNull(s.OnBecameTeen(mom));
            Assert.Equal(1, s.WolfDefenseBonus());

            // Köpeğin yavrusuyla annenin çocuğu: bağ eşiğin yarısıyla başlar.
            var pup = new Animal(999, dog.Species, dog.Position, dog.HomeItemId);
            typeof(Animal).GetProperty("MotherId").SetValue(pup, dog.Id);
            var kid = Person(2, ChildAge); kid.BirthMotherId = 1; f.Pop.Add(kid);
            var b = s.AddContact(kid, pup, 1);
            Assert.Equal(CompanionBondSystem.Threshold * 0.5 + 1, b.Points, 6);

            CompanionBond lost = null;
            s.CompanionLost += (c, bb) => lost = bb;
            s.OnAnimalDied(dog);
            Assert.NotNull(lost);
            Assert.True(lost.Memory);
            Assert.True(s.Has(1, CompanionTrait.PackTongue));   // yetenek kalır
        }

        [Fact]
        public void CanYoldasi_KaliteBonusuAnimalSystemeBaglanir()
        {
            var f = MakeFarm();
            Assert.True(f.Animals.Buy("animal.cow", f.House, out var cow).Ok);
            double today = 0;
            var s = new CompanionBondSystem(f.Pop, f.Animals) { Today = () => today };
            var kid = Person(1, ChildAge); f.Pop.Add(kid);
            for (int d = 0; d <= 10; d++) { today = d; s.AddContact(kid, cow, 12); }
            s.OnBecameTeen(kid);
            f.Animals.QualityBonus = a => s.QualityBonus(a.Species.Id);
            Assert.Equal(5f, s.QualityBonus("animal.cow"));
            Assert.Equal(0f, s.QualityBonus("animal.horse"));
            // Sınırda: dostluk 70, konfor 30 → 54 (seçme 55 altı); +5 ile seçme.
            typeof(Animal).GetProperty("Friendship").SetValue(cow, 70f);
            typeof(Animal).GetProperty("Comfort").SetValue(cow, 30f);
            Assert.Equal(0, cow.QualityTier);
            Assert.Equal(1, f.Animals.QualityOf(cow));
        }

        // ---------- çıraklık ve aile tarifi ----------

        [Fact]
        public void Ciraklik_UstaSeviye5_CirakHizliOgrenir_YasliUstaDahaHizli_Mezuniyet()
        {
            var pop = new Population();
            var master = Person(1, AdultAge); var app = Person(2, ChildAge); var elder = Person(3, ElderAge); var app2 = Person(4, TeenAge);
            foreach (var c in new[] { master, app, elder, app2 }) pop.Add(c);
            var s = new ApprenticeshipSystem(pop);
            Assert.NotNull(s.Blocker(master, app, SkillType.Smithing));
            master.Skills.AddXp(SkillType.Smithing, SkillRules.XpForLevel(5), 10);
            elder.Skills.AddXp(SkillType.Smithing, SkillRules.XpForLevel(6), 10);
            Assert.True(s.Take(master, app, SkillType.Smithing, 0));
            Assert.NotNull(s.Blocker(master, app2, SkillType.Smithing));   // bir ustaya bir çırak
            Assert.True(s.Take(elder, app2, SkillType.Smithing, 0));
            Assert.Equal(1.5, s.XpFactor(2, SkillType.Smithing), 6);
            Assert.Equal(1.0, s.XpFactor(2, SkillType.Cooking), 6);
            Assert.Equal(2.0, s.XpFactor(4, SkillType.Smithing), 6);

            Character gm = null;
            s.Graduated += (m, a, sk) => gm = a;
            app2.Skills.AddXp(SkillType.Smithing, SkillRules.XpForLevel(5), 7);
            s.DailyUpdate();
            Assert.Same(app2, gm);
            Assert.Null(s.OfApprentice(4));
            Assert.True(master.Skills.GetXp(SkillType.Teaching) > 0);
        }

        [Fact]
        public void AileTarifi_Seviye8deDoğar_SozluAktarilmazsaKaybolur_YaziliKalir()
        {
            var pop = new Population();
            var gran = Person(1, ElderAge); var kid = Person(2, ChildAge);
            kid.Generation = 2;
            pop.Add(gran); pop.Add(kid);
            var book = new FamilyRecipeBook(pop);
            var r = book.OnLevel(gran, SkillType.Cooking, 8, 0);
            Assert.NotNull(r);
            Assert.Equal("K1nin peyniri", r.Name);
            Assert.Null(book.OnLevel(gran, SkillType.Cooking, 9, 0));   // ikinci kez doğmaz
            Assert.Single(book.AtRiskWarnings());
            Assert.Equal(1.1, book.XpFactor(1, SkillType.Cooking), 6);
            Assert.Equal(0.1, book.ExtraYield(SkillType.Cooking), 6);

            // Sözlü: öğretilmeden ölürse kaybolur.
            var lostList = book.OnDied(gran);
            Assert.Single(lostList);
            Assert.True(r.Lost);
            Assert.Equal(0, book.ExtraYield(SkillType.Cooking), 6);

            // Yeniden: bu kez öğretilir ve yazılır.
            var gran2 = Person(3, ElderAge); pop.Add(gran2);
            var r2 = book.OnLevel(gran2, SkillType.Cooking, 8, 1);
            Assert.True(book.Teach(gran2, kid, r2));
            Assert.Equal(2, r2.Generations);
            Assert.NotNull(book.WriteBlocker(gran2, r2, hasBlankBook: true));   // okuryazarlık yok
            gran2.Skills.AddXp(SkillType.Literacy, SkillRules.XpForLevel(3), 10);
            Assert.NotNull(book.WriteBlocker(gran2, r2, hasBlankBook: false));
            Assert.True(book.Write(gran2, r2, true));
            book.OnDied(gran2);
            Kill(kid);
            book.OnDied(kid);
            Assert.False(r2.Lost);   // yazılı tarif kalır
            var reader = Person(4, TeenAge); pop.Add(reader);
            reader.Skills.AddXp(SkillType.Literacy, SkillRules.XpForLevel(2), 7);
            Assert.True(book.Teach(null, reader, r2));   // defterden okur
        }

        // ---------- itibar ----------

        [Fact]
        public void Itibar_SatisBonusu_Kademe_YeniKusakYarisi()
        {
            var r = new ReputationSystem();
            Assert.Equal(0, r.SellBonus(), 6);
            Assert.Equal("Bilinmez", r.TierName);
            r.Add(500, "test");
            Assert.Equal(0.05, r.SellBonus(), 6);
            Assert.Equal("Tanınmış", r.TierName);
            r.Add(5000, "tavan");
            Assert.Equal(ReputationSystem.Max, r.Points);
            r.OnSuccession();
            Assert.Equal(500, r.Points);
            r.Add(-1000, "kötü");
            Assert.True(r.SellBonus() < 0);
            Assert.Equal(-0.05, r.SellBonus(), 6);
        }

        // ---------- hayata bağlantı ----------

        [Fact]
        public void Hayat_XpCarpaniKarakterdenUygulanir_MirasVeKayit()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var pop = new Population();
            var life = new LifeSystems(db, new WorldState(new GridMap(12, 12)), new Household(), pop, new GameClock());
            var master = Person(1, AdultAge); var app = Person(2, ChildAge);
            app.BirthMotherId = 1;
            pop.Add(master); pop.Add(app);
            master.Skills.AddXp(SkillType.Brewing, SkillRules.XpForLevel(5), 10);
            Assert.True(life.Upbringing.Apprentices.Take(master, app, SkillType.Brewing, 0));
            double got = app.GainSkillXp(SkillType.Brewing, 10);
            Assert.Equal(15, got, 6);

            // Ustalaşılmış oyuncak ölünce çocuğa kalır.
            for (int i = 0; i < 100; i++) life.Upbringing.Toys.OnPlayed(master, "item.spinning_top", 0);
            Kill(master, pop);
            Assert.NotNull(life.Upbringing.Toys.Personal(2, "item.spinning_top"));
            Assert.Null(life.Upbringing.Apprentices.OfApprentice(2));

            life.Upbringing.Reputation.Add(100, "test");
            var data = life.Export();
            var json = JsonSerializer.Serialize(data, Json);
            var pop2 = new Population();
            var life2 = new LifeSystems(db, new WorldState(new GridMap(12, 12)), new Household(), pop2, new GameClock());
            life2.Restore(JsonSerializer.Deserialize<Thatchwick.Core.Save.LifeSaveData>(json, Json));
            Assert.Equal(100, life2.Upbringing.Reputation.Points);
            Assert.NotNull(life2.Upbringing.Toys.Personal(2, "item.spinning_top"));
            Assert.True(life2.SellBonus() > 0);
        }

        [Fact]
        public void Hayat_OyuncakEtkilesimiJobRunnerdanSayilir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var world = new WorldState(new GridMap(12, 12));
            var pop = new Population();
            var life = new LifeSystems(db, world, new Household(), pop, new GameClock());
            var kid = Person(1, ChildAge, 2, 2);
            pop.Add(kid);
            var hoop = world.PlaceItem(db.Items.Get("item.hoop"), new GridPos(3, 2));
            life.Upbringing.OnItemUsed(kid, hoop);
            Assert.Equal(4, life.Upbringing.Toys.HabitOf(1, "item.hoop"), 6);
        }
            // ---------- arayüz eylemleri ----------

        [Fact]
        public void Arayuz_CirakBeceriSecimi_DefterYazma_OyuncakOnarma_EbeveynYardimi()
        {
            var f = MakeFarm();
            Assert.True(f.Animals.Buy("animal.cow", f.House, out var cow).Ok);
            double today = 0;
            var u = new UpbringingSystems(f.Pop) { Today = () => today };
            u.Attach(f.Animals);
            var gran = Person(1, ElderAge); var kid = Person(2, ChildAge);
            f.Pop.Add(gran); f.Pop.Add(kid);

            Assert.Null(u.TeachableSkill(gran, kid));
            gran.Skills.AddXp(SkillType.Cooking, SkillRules.XpForLevel(6), 10);
            gran.Skills.AddXp(SkillType.Brewing, SkillRules.XpForLevel(8), 10);
            Assert.Equal(SkillType.Brewing, u.TeachableSkill(gran, kid));

            var r = u.Recipes.Get("recipe.family_ale");   // 8. seviyede kendiliğinden doğdu
            Assert.NotNull(r);
            gran.Skills.AddXp(SkillType.Literacy, SkillRules.XpForLevel(3), 10);
            Assert.NotNull(u.WriteRecipe(gran, r, f.House));   // defter yok
            f.House.AddItem("item.blank_book");
            Assert.Null(u.WriteRecipe(gran, r, f.House));
            Assert.True(r.Written);
            Assert.Equal(0, f.House.Count("item.blank_book"));

            for (int i = 0; i < 25; i++) u.Toys.OnPlayed(kid, "item.whittling_knife", 0);
            var p = u.Toys.Personal(2, "item.whittling_knife");
            p.Durability = 0;
            int planks = f.House.Count("item.plank");
            if (planks > 0) for (int i = 0; i < planks; i++) f.House.TryTakeItem("item.plank");
            Assert.NotNull(u.RepairToy(kid, "item.whittling_knife", f.House));
            f.House.AddItem("item.plank");
            Assert.Null(u.RepairToy(kid, "item.whittling_knife", f.House));
            Assert.False(p.IsBroken);

            Assert.Same(cow, u.AssistTarget(kid));
            Assert.Null(u.ParentAssist(gran, kid));
            Assert.NotNull(u.ParentAssistBlocker(gran, kid));   // günde bir
            today = 1;
            Assert.Null(u.ParentAssistBlocker(gran, kid));
            Assert.NotNull(u.ParentAssistBlocker(kid, gran));
        }
            // ---------- atletizm ----------

        [Fact]
        public void Atletizm_HizYorgunlukSoguk_YurumekXpVerir_OyuncaklarBagli()
        {
            var c = Person(1, AdultAge);
            Assert.Equal(1.0, c.AthleticSpeedFactor, 6);
            c.Skills.AddXp(SkillType.Athletics, SkillRules.XpForLevel(10), 10);
            Assert.Equal(1.18, c.AthleticSpeedFactor, 6);
            Assert.Equal(0.82, c.AthleticEnergyFactor, 4);
            Assert.Equal(0.09, c.AthleticColdResist, 4);
            Assert.Equal("Atletizm", SkillNames.Name(SkillType.Athletics));
            foreach (var id in new[] { "item.jump_rope", "item.hoop", "item.toy_sword_wall" })
                Assert.Equal(SkillType.Athletics, new ToyHabitSystem().Def(id).Skill);

            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(12, 12);
            var world = new WorldState(map);
            var walker = Person(2, AdultAge, 0, 0);
            var runner = new JobRunner(walker, world);
            runner.Command(Job.MoveTo(new GridPos(8, 0)));
            for (int i = 0; i < 400 && !runner.IsIdle; i++) runner.Tick(0.005);
            Assert.Equal(new GridPos(8, 0), walker.Position);
            Assert.True(walker.Skills.GetXp(SkillType.Athletics) > 0.3);
        }
    }
}
