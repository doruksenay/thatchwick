using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Miras (17.11.2026): Soy Mührü kaynakları, hissedilir ve nesille büyüyen yadigârlar, komşu haneye evlendirme, köydeki soy.
    public class LegacyTests
    {
        private static ContentDatabase Real()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static WorldState World(int size = 20)
        {
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            return new WorldState(map);
        }

        private static Character Person(Population pop, string name, Sex sex, double age, int? mother = null, int? father = null, bool villager = false)
        {
            var c = new Character(pop.NextId(), name, new GridPos(1, 1)) { Sex = sex, BirthMotherId = mother, BirthFatherId = father, IsVillager = villager };
            c.SetAge(age);
            pop.Add(c);
            return c;
        }

        // ---------- Soy Mührü ----------

        [Fact]
        public void Muhur_KazanilirHarcanir_DefteriTutulur()
        {
            var legacy = new Legacy(new Population());
            legacy.Award(SealSource.Goal, 2, 1.0, "Hedef: Deneme");
            Assert.Equal(2, legacy.Seals);
            Assert.False(legacy.TrySpend(3, SealSource.Heirloom, 2.0, "fazla"));
            Assert.True(legacy.TrySpend(2, SealSource.Heirloom, 2.0, "yadigâr"));
            Assert.Equal(0, legacy.Seals);
            Assert.Equal(new[] { 2, -2 }, legacy.Ledger.Select(e => e.Amount).ToArray());
        }

        [Fact]
        public void Muhur_YeniNesilBirKezVerilir_SoyMeslegiUcBastaVerilir()
        {
            var pop = new Population();
            var legacy = new Legacy(pop);
            var dynasty = new Dynasty();
            var founder = Person(pop, "Kurucu", Sex.Male, 60);
            dynasty.Found(founder, 0);

            // Üç baş art arda "Odunculuk soyu" ile öldü; dördüncü başa geçildi.
            Character head = founder;
            for (int g = 2; g <= 4; g++)
            {
                var heir = Person(pop, "Varis" + g, Sex.Male, 40, father: head.Id);
                heir.Generation = g;
                var r = dynasty.OnDeath(head, pop.Members, g * 10, trade: "Odunculuk soyu");
                Assert.Same(heir, r.Heir);
                legacy.OnSuccession(dynasty, heir, g * 10);
                head = heir;
            }
            var bySource = legacy.Ledger.GroupBy(e => e.Source).ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
            Assert.Equal(3 * Legacy.SealsPerGeneration, bySource[SealSource.Generation]);     // 2., 3., 4. nesil
            Assert.Equal(Legacy.TradeLineageSeals, bySource[SealSource.TradeLineage]);       // 3 baş aynı meslek

            // Aynı nesil numarası ikinci kez ödül vermez (ör. kardeş varis).
            legacy.OnSuccession(dynasty, head, 50);
            Assert.Equal(3 * Legacy.SealsPerGeneration, legacy.Ledger.Where(e => e.Source == SealSource.Generation).Sum(e => e.Amount));
        }

        [Fact]
        public void Muhur_KisKayipsizAtlatilincaVerilir_KisinOlumVarsaVerilmez()
        {
            var legacy = new Legacy(new Population()) { DaysPerYear = 40 };
            legacy.OnSeasonChanged(Season.Autumn, Season.Winter, 30, 30);
            legacy.OnSeasonChanged(Season.Winter, Season.Spring, 40, 40);
            Assert.Equal(Legacy.WinterSurvivedSeals, legacy.Seals);

            legacy.OnSeasonChanged(Season.Autumn, Season.Winter, 70, 70);
            legacy.OnHouseholdDeath(Season.Winter);
            legacy.OnSeasonChanged(Season.Winter, Season.Spring, 80, 80);
            Assert.Equal(Legacy.WinterSurvivedSeals, legacy.Seals);                         // ikinci kışta kayıp: mühür yok
        }

        [Fact]
        public void Muhur_YasamSistemleri_KisBitinceVeHedefteVerir()
        {
            var db = Real();
            var world = World();
            var clock = new GameClock();
            clock.Restore(38.5, 1);                                                          // kış ortası
            var life = new LifeSystems(db, world, new Household(), new Population(), clock);
            life.Tick(0.001);
            clock.Restore(40.2, 1);                                                          // ilkbahar
            life.Tick(0.001);
            Assert.Contains(life.Legacy.Ledger, e => e.Source == SealSource.WinterSurvived);

            // Kayıttan dönünce aynı kış ikinci kez ödüllenmez.
            var saved = life.Export();
            Assert.NotNull(saved.legacy);
            var clock2 = new GameClock();
            clock2.Restore(40.2, 1);
            var life2 = new LifeSystems(db, World(), new Household(), new Population(), clock2);
            life2.Restore(saved);
            Assert.Equal(life.Legacy.Seals, life2.Legacy.Seals);
        }

        // ---------- Yadigâr ----------

        [Fact]
        public void Yadigar_UygunEsyadaMuhurleYapilir_UygunsuzdaYapilmaz()
        {
            var db = Real();
            var world = World();
            var legacy = new Legacy(new Population());
            var bed = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 3));
            var maker = new Character(1, "Orhan", new GridPos(0, 0));

            Assert.StartsWith("Soy Mührü yetmiyor", legacy.CanMakeHeirloom(bed));
            legacy.Award(SealSource.Other, 10, 0, "test");
            var h = legacy.MakeHeirloom(bed, maker, 5, out var msg);
            Assert.NotNull(h);
            Assert.Equal(10 - Legacy.CostFor(bed.Def), legacy.Seals);
            Assert.Contains("Orhan yadigârı", h.DisplayName);
            Assert.Equal("Bu eşya zaten yadigâr.", legacy.CanMakeHeirloom(bed));

            var tree = world.PlaceItem(db.Items.Get("item.tree_oak"), new GridPos(8, 8));
            Assert.Contains("yadigâr olamaz", legacy.CanMakeHeirloom(tree));
        }

        [Fact]
        public void Yadigar_BonusuHissedilir_HerNesilBuyur_TavaniVar()
        {
            var db = Real();
            var world = World();
            var pop = new Population();
            var legacy = new Legacy(pop);
            legacy.Award(SealSource.Other, 20, 0, "test");
            var bed = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 3));
            var sleep = bed.Def.Interactions.First(i => i.RestoresNeed);
            var h = legacy.MakeHeirloom(bed, null, 0, out _);

            Assert.Equal(1.05, legacy.BonusFor(bed, sleep).Restore, 3);                      // yapıldığı an %5
            Assert.Equal(2, legacy.ComfortBonus(bed));

            var dynasty = new Dynasty();
            var head = Person(pop, "Baş", Sex.Male, 60);
            dynasty.Found(head, 0);
            for (int g = 2; g <= 7; g++)
            {
                var heir = Person(pop, "V" + g, Sex.Male, 40, father: head.Id);
                heir.Generation = g;
                dynasty.OnDeath(head, pop.Members, g, null);
                legacy.OnSuccession(dynasty, heir, g);
                head = heir;
                if (g == 3) Assert.Equal(1.15, legacy.BonusFor(bed, sleep).Restore, 3);      // iki nesil sonra %15
            }
            Assert.Equal(1.0 + Legacy.HeirloomMax, legacy.BonusFor(bed, sleep).Restore, 3);  // tavan %25
            Assert.Equal(Legacy.HeirloomComfortMax, legacy.ComfortBonus(bed));
            Assert.Equal(6, h.GenerationsHeld);
        }

        [Fact]
        public void Yadigar_IsHiziniVeXpyiArtirir_JobRunnerUzerinden()
        {
            var db = Real();
            var world = World();
            var legacy = new Legacy(new Population());
            legacy.Award(SealSource.Other, 20, 0, "test");
            var cradle = world.PlaceItem(db.Items.Get("item.cradle"), new GridPos(3, 3));
            var rock = cradle.Def.Interactions.First(i => i.Skill.HasValue);
            var c = new Character(1, "Anne", new GridPos(2, 3)) { Sex = Sex.Female };
            c.SetAge(40);
            var runner = new JobRunner(c, world);
            double plain = runner.EffectiveDuration(rock, cradle);

            runner.ItemBonus = legacy.BonusFor;
            Assert.Equal(plain, runner.EffectiveDuration(rock, cradle), 6);                  // yadigâr değil: fark yok
            legacy.MakeHeirloom(cradle, c, 0, out _);
            Assert.Equal(plain / 1.05, runner.EffectiveDuration(rock, cradle), 6);           // yadigâr: %5 hızlı

            double before = c.Skills.GetXp(rock.Skill.Value);
            runner.Command(Job.UseItem(cradle.Id, rock.Id));
            for (int i = 0; i < 200 && !runner.IsIdle; i++) runner.Tick(0.01);
            double gained = c.Skills.GetXp(rock.Skill.Value) - before;
            Assert.True(gained > rock.SkillXp * 1.04, $"yadigârda XP artmalı: {gained} / {rock.SkillXp}");
        }

        [Fact]
        public void Yadigar_Alet_IlgiliIsiHizlandirir()
        {
            var legacy = new Legacy(new Population());
            legacy.Award(SealSource.Other, 10, 0, "test");
            var chop = new InteractionDef { Id = "chop", Tool = "tool.axe", Skill = SkillType.Woodcutting, SkillXp = 5 };
            var other = new InteractionDef { Id = "dig", Tool = "tool.hoe", Skill = SkillType.Farming, SkillXp = 5 };
            Assert.NotNull(legacy.MakeToolHeirloom("tool.axe", "Balta", null, 0, out _));
            Assert.Equal(1.05, legacy.BonusFor(null, chop).Speed, 3);
            Assert.Equal(1.05, legacy.BonusFor(null, chop).Xp, 3);
            Assert.True(legacy.BonusFor(null, other).IsNone);
            Assert.Equal(10 - Legacy.ToolHeirloomCost, legacy.Seals);
        }

        [Fact]
        public void Yadigar_EnvantereAlinmaz_SatilincaGecmisiSilinir_YikilincaKaybolur()
        {
            var db = Real();
            var world = World();
            var house = new Household(1000);
            var legacy = new Legacy(new Population());
            legacy.Award(SealSource.Other, 20, 0, "test");
            var build = new BuildService(world, house, db) { StoreBlocker = legacy.StoreBlocker };
            build.PlacedItemSold += legacy.OnItemSold;
            var chair = world.PlaceItem(db.Items.Get("item.chair_wood"), new GridPos(3, 3));
            var stool = world.PlaceItem(db.Items.Get("item.stool_wood"), new GridPos(6, 6));
            legacy.MakeHeirloom(chair, null, 0, out _);
            legacy.MakeHeirloom(stool, null, 0, out _);

            var stored = build.StoreToInventory(chair.Id);
            Assert.False(stored.Ok);
            Assert.Contains("envantere alınmaz", stored.Reason);

            // Taşımak yadigârı korur (eşya kimliği değişmez).
            Assert.True(build.Move(chair.Id, new GridPos(4, 4), 0).Ok);
            Assert.NotNull(legacy.HeirloomOf(chair.Id));

            string lost = null;
            legacy.HeirloomLost += (h, m) => lost = m;
            Assert.True(build.SellPlaced(chair.Id).Ok);
            Assert.Null(legacy.HeirloomOf(chair.Id));
            Assert.Contains("satıldı", lost);

            world.RemoveItem(stool.Id);                                                      // yangın, yıkım
            Assert.Single(legacy.Validate(world.GetItem));
            Assert.Empty(legacy.Heirlooms);
        }

        [Fact]
        public void Yadigar_OdaKonforunuArtirir()
        {
            var db = Real();
            var world = World();
            var map = world.Map;
            for (int x = 2; x <= 5; x++) { map.SetEdge(new Edge(x, 2, EdgeAxis.Horizontal), EdgeKind.Wall); map.SetEdge(new Edge(x, 6, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = 2; y <= 5; y++) { map.SetEdge(new Edge(2, y, EdgeAxis.Vertical), EdgeKind.Wall); map.SetEdge(new Edge(6, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            var bed = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 3));
            var mood = new MoodService(world) { Content = db };
            var room = map.Rooms.RoomAt(new GridPos(4, 4));
            Assert.NotNull(room);
            int plain = mood.ComfortOf(room).Score;

            var legacy = new Legacy(new Population());
            legacy.Award(SealSource.Other, 10, 0, "test");
            legacy.MakeHeirloom(bed, null, 0, out _);
            mood.ExtraComfort = legacy.ComfortBonus;
            Assert.Equal(plain + Legacy.HeirloomComfortPerLevel, mood.ComfortOf(room).Score);
        }

        // ---------- Komşu haneye evlendirme ----------

        [Fact]
        public void Evlendirme_Kurallar_BasGidemez_YakinlikVeAkrabalikGerekir()
        {
            var db = Real();
            var pop = new Population();
            var house = new Household();
            var family = new FamilyService(pop, house, db);
            var legacy = new Legacy(pop);
            var dynasty = new Dynasty();
            var head = Person(pop, "Baba", Sex.Male, 60);
            dynasty.Found(head, 0);
            var daughter = Person(pop, "Kız", Sex.Female, 30, father: head.Id);
            var son = Person(pop, "Oğul", Sex.Male, 30, father: head.Id);
            var smith = Person(pop, "Demirci", Sex.Male, 32, villager: true);
            var cousin = Person(pop, "Kuzen", Sex.Male, 31, father: Person(pop, "Amca", Sex.Male, 55, father: 999).Id, villager: true);

            Assert.StartsWith("Yakınlık yetmiyor", legacy.CanMarryOut(daughter, smith, dynasty, family));
            family.SetAffection(daughter.Id, smith.Id, Legacy.MarryOutAffection);
            Assert.Null(legacy.CanMarryOut(daughter, smith, dynasty, family));
            Assert.Equal("Hanedanın başı başka haneye gidemez.", legacy.CanMarryOut(head, smith, dynasty, family));
            Assert.Equal("Hanedan evliliği kadın ile erkek arasında olur.", legacy.CanMarryOut(son, smith, dynasty, family));

            // Ortak büyükbaba: kuzen evliliği olmaz.
            var grandpa = Person(pop, "Dede", Sex.Male, 90);
            head.BirthFatherId = grandpa.Id;
            var uncle = pop.Find(cousin.BirthFatherId.Value);
            uncle.BirthFatherId = grandpa.Id;
            family.SetAffection(daughter.Id, cousin.Id, 100);
            Assert.Equal("Akraba evliliği olmaz.", legacy.CanMarryOut(daughter, cousin, dynasty, family));
        }

        [Fact]
        public void Evlendirme_UyeKoyluOlur_KoydeYasar_MuhurVerir_KayitTutulur()
        {
            var db = Real();
            var pop = new Population();
            var family = new FamilyService(pop, new Household(), db);
            var legacy = new Legacy(pop);
            var dynasty = new Dynasty();
            var head = Person(pop, "Baba", Sex.Male, 60);
            dynasty.Found(head, 0);
            var daughter = Person(pop, "Kız", Sex.Female, 30, father: head.Id);
            daughter.IsPlayerControlled = true;
            var smith = Person(pop, "Demirci", Sex.Male, 32, villager: true);
            family.SetAffection(daughter.Id, smith.Id, 80);

            Character gone = null;
            legacy.MarriedOutHappened += (m, v) => gone = m;
            Assert.True(legacy.MarryOut(daughter, smith, dynasty, family, 12, out var msg));
            Assert.True(daughter.IsVillager);
            Assert.False(daughter.IsPlayerControlled);
            Assert.Equal(smith.Id, daughter.SpouseId);
            Assert.Equal(daughter.Id, smith.SpouseId);
            Assert.Same(daughter, gone);
            Assert.Equal(Legacy.MarriedOutSeals, legacy.Seals);
            Assert.Single(legacy.MarriedOut);
            Assert.Contains("Soy Mührü", msg);
            Assert.False(pop.Find(daughter.Id).IsDead);                                       // köyde yaşamaya devam eder
        }

        [Fact]
        public void KomsuHane_CocukSahibiOlur_KoydekiSoySayilir_EsiklerMuhurVerir()
        {
            var db = Real();
            var pop = new Population { Content = db };
            var family = new FamilyService(pop, new Household(), db);
            var legacy = new Legacy(pop, seed: 7);
            var dynasty = new Dynasty();
            var head = Person(pop, "Baba", Sex.Male, 60);
            dynasty.Found(head, 0);
            var daughter = Person(pop, "Kız", Sex.Female, 30, father: head.Id);
            var smith = Person(pop, "Demirci", Sex.Male, 32, villager: true);
            family.SetAffection(daughter.Id, smith.Id, 100);
            legacy.MarryOut(daughter, smith, dynasty, family, 0, out _);
            Assert.Equal(1, legacy.LivingDescendantsOutside(head.Id));

            // Günlük olasılıkla gebe kalır; doğum nüfus saatiyle olur.
            bool pregnant = false;
            for (int d = 0; d < 200 && !pregnant; d++) pregnant = legacy.NeighborFamiliesDaily().Contains(daughter);
            Assert.True(pregnant, "komşu hanede çocuk olmalı");

            // Torunlar (köylü) soy sayılır; eşik 3'te mühür.
            Person(pop, "Torun1", Sex.Male, 2, mother: daughter.Id, father: smith.Id, villager: true);
            Person(pop, "Torun2", Sex.Female, 1, mother: daughter.Id, father: smith.Id, villager: true);
            int before = legacy.Seals;
            legacy.CheckDescendants(head.Id, 50);
            Assert.Equal(3, legacy.LivingDescendantsOutside(head.Id));
            Assert.Equal(before + 1, legacy.Seals);
            legacy.CheckDescendants(head.Id, 51);
            Assert.Equal(before + 1, legacy.Seals);                                          // aynı eşik tekrar ödül vermez
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_MuhurYadigarVeEvlendirmeGeriGelir_OdulTekrarlanmaz()
        {
            var db = Real();
            var world = World();
            var pop = new Population();
            var legacy = new Legacy(pop);
            legacy.Award(SealSource.Other, 10, 0, "test");
            var bed = world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 3));
            legacy.MakeHeirloom(bed, new Character(5, "Orhan", new GridPos(0, 0)), 3, out _);
            legacy.MakeToolHeirloom("tool.axe", "Balta", null, 4, out _);
            legacy.MarriedOut.Add(new MarriedOutRecord { CharacterId = 9, Name = "Kız", SpouseId = 10, SpouseName = "Demirci", Day = 7 });
            var dynasty = new Dynasty();
            var head = Person(pop, "Baş", Sex.Male, 60);
            dynasty.Found(head, 0);
            var heir = Person(pop, "Varis", Sex.Male, 40, father: head.Id);
            heir.Generation = 2;
            dynasty.OnDeath(head, pop.Members, 20, null);
            legacy.OnSuccession(dynasty, heir, 20);

            var json = UnityLikeJson.RoundTrip(legacy.Export());
            var restored = new Legacy(pop);
            restored.Restore(json);
            Assert.Equal(legacy.Seals, restored.Seals);
            Assert.Equal(2, restored.Heirlooms.Count);
            Assert.Equal(1, restored.HeirloomOf(bed.Id).GenerationsHeld);
            Assert.Equal("Orhan", restored.HeirloomOf(bed.Id).MakerName);
            Assert.NotNull(restored.ToolHeirloom("tool.axe"));
            Assert.Single(restored.MarriedOut);
            Assert.Equal(legacy.Ledger.Count, restored.Ledger.Count);

            int seals = restored.Seals;
            restored.OnSuccession(dynasty, heir, 21);                                       // 2. nesil zaten ödüllendi
            Assert.Equal(seals, restored.Seals);
            Assert.Null(new Legacy(pop).HeirloomOf(bed.Id));
            var empty = new Legacy(pop);
            empty.Restore(null);                                                            // eski kayıt
            Assert.Equal(0, empty.Seals);
        }
    }

    /// <summary>JsonUtility'ye yakın gidiş-dönüş: alanlar (property değil) serileştirilir.</summary>
    internal static class UnityLikeJson
    {
        public static T RoundTrip<T>(T value)
        {
            var opts = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
            return System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(value, opts), opts);
        }
    }
}
