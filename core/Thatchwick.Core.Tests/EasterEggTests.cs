using System;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Easter egg'ler ve Tuhaflıklar (kullanıcı 20.11.2026, Project belgesi 55): hayvanla konuşma, bunu anlatma,
    // bulunan silah, küçük tuhaflıklar. Ortak kural: kalıcı kayıp yok.
    public class EasterEggTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db; public Population Pop; public Household House; public WorldState World;
            public LifeSystems Life; public FamilyService Family; public AnimalSystem Animals; public AnimalCare Care;
            public GameClock Clock;
        }

        private static Kit Make(int seed = 3)
        {
            var k = new Kit { Db = ContentFilesTests.LoadDb(out var errors), Pop = new Population(), House = new Household(50), Clock = new GameClock() };
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            k.World = new WorldState(map);
            k.Life = new LifeSystems(k.Db, k.World, k.House, k.Pop, k.Clock, seed);
            k.Family = new FamilyService(k.Pop, k.House, k.Db);
            k.Life.Attach(k.Family);
            k.Animals = new AnimalSystem(k.World, k.Db) { DiseaseEnabled = false };
            k.Care = new AnimalCare(k.Animals, k.House);
            k.Life.Attach(k.Animals, k.Care);
            return k;
        }

        private static Character Person(Kit k, int id, string name, double age = 40, int charm = 1, bool villager = false, int generation = 1)
        {
            var c = new Character(id, name, new GridPos(2, 2), !villager) { CanStarve = false, MoodEnabled = true, IsVillager = villager, Generation = generation };
            c.SetAge(age);
            if (charm > 1) c.Skills.RestoreXp(SkillType.Charm, SkillRules.XpForLevel(charm));
            k.Pop.Add(c);
            return c;
        }

        private static readonly double ChildAge = AgeConfig.Default.ChildStartDay + 0.5, BabyAge = AgeConfig.Default.ChildStartDay * 0.5,
            ElderAge = AgeConfig.Default.ElderStartDay + 1;

        private static Animal Cow(Kit k, int id = 50, float comfort = 80f, float fullness = 100f) =>
            k.Animals.Restore(id, k.Db.Animals.Get("animal.cow"), new GridPos(4, 4), 0, fullness, 40, 0, 0, 30f, comfort: comfort);

        // ---- moodlet'ler ve içerik ----

        [Fact]
        public void Moodlets_AreRegistered_WithDocValues()
        {
            var cat = MoodCatalog.Default;
            (string id, int value, double days)[] expect =
            {
                (MoodCatalog.AnimalSpoke, 10, 3.0), (MoodCatalog.Confidant, 5, 2.0), (MoodCatalog.NotCrazy, 8, 2.0),
                (MoodCatalog.Believed, 5, 1.0), (MoodCatalog.FamilyTale, 3, 1.0), (MoodCatalog.NobodyBelieves, -3, 0.25),
                (MoodCatalog.OddChat, 2, 0.25), (MoodCatalog.Thoughtful, 3, 1.0), (MoodCatalog.LetDown, -2, 0.25)
            };
            foreach (var (id, value, days) in expect)
            {
                Assert.True(cat.TryGet(id, out var def), id);
                Assert.Equal(value, def.Value);
                Assert.Equal(days, def.DurationDays, 3);
            }
            // Ortak kural: olumsuz easter egg moodlet'i kısa sürer (en çok yarım gün).
            foreach (var (id, value, days) in expect) if (value < 0) Assert.True(days <= 0.5, id);
        }

        [Fact]
        public void Content_HasRelicItemsAndRecipes()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            foreach (var id in new[] { FoundRelicService.SwordItem, FoundRelicService.HelmetItem, FoundRelicService.ShieldItem, FoundRelicService.OldSwordItem,
                         "item.sword_ploughshare", "item.old_sword_wall", FoundRelicService.FlowerItem })
                Assert.True(db.Items.Contains(id), id);
            Assert.True(db.Items.TryGet(FoundRelicService.SwordItem, out var sword));
            foreach (var inter in new[] { FoundRelicService.InspectInteraction, FoundRelicService.BuryInteraction, FoundRelicService.TakeInteraction })
                Assert.Contains(sword.Interactions, i => i.Id == inter);
            // Kalıntılar satın alınmaz, satılınca para etmez.
            Assert.Equal(0, sword.Price);
            // Eski kılıçtan saban demiri ve duvar süsü yapılır: tezgâh etkileşimi üretimi "craft" olarak taşır.
            Assert.True(db.Items.TryGet("item.forge", out var forge));
            Assert.Contains(forge.Interactions, i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.sword_ploughshare"));
            Assert.True(db.Items.TryGet("item.workbench", out var bench));
            Assert.Contains(bench.Interactions, i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.old_sword_wall"));
        }

        // ---- hayvanla konuşma ----

        [Fact]
        public void Talk_NeedsCharm9_AndIsOncePerAnimalPerDay()
        {
            var k = Make();
            var low = Person(k, 1, "Ali", charm: 8);
            var high = Person(k, 2, "Ayla", charm: 9);
            var cow = Cow(k);
            Assert.NotNull(k.Care.CanDo(low, cow.Id, AnimalAction.TalkLikeHuman));
            Assert.False(k.Life.AnimalTalk.Offers(low));
            Assert.Null(k.Care.CanDo(high, cow.Id, AnimalAction.TalkLikeHuman));
            Assert.True(k.Life.AnimalTalk.Offers(high));

            var r = k.Life.AnimalTalk.Try(high, cow, roll: 0.99);
            Assert.False(r.Spoke);
            Assert.False(high.Mood.Has(MoodCatalog.AnimalSpoke));
            Assert.True(k.Life.Oddities.Has(OddityIds.AnimalStare));
            Assert.NotNull(k.Care.CanDo(high, cow.Id, AnimalAction.TalkLikeHuman));   // bugün yeniden olmaz
            Assert.Null(k.Life.AnimalTalk.Try(high, cow, roll: 0.0));
        }

        [Fact]
        public void Talk_IsVeryRare_AndCompanionTriples()
        {
            var svc = new AnimalTalkService();
            var k = Make();
            var c = Person(k, 1, "Ayla", charm: 10);
            var cow = Cow(k);
            Assert.Equal(1.0 / 250.0, svc.ChanceFor(c, cow), 6);
            svc.IsCompanion = (a, b) => true;
            Assert.Equal(3.0 / 250.0, svc.ChanceFor(c, cow), 6);
        }

        [Fact]
        public void Talk_Speaks_GivesMoodlet_Chronicle_Achievement_AndOnlyOnce()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla", charm: 9);
            var cow = Cow(k, comfort: 20f);
            var other = Cow(k, id: 51);
            LocText line = default;
            k.Life.OddityLine += (l, id) => line = l;

            k.Care.Talk.ForceNext = true;
            k.Care.Apply(c, cow.Id, AnimalAction.TalkLikeHuman);

            Assert.True(k.Life.AnimalTalk.HasSpoken(c));
            Assert.True(c.Mood.Has(MoodCatalog.AnimalSpoke));
            Assert.Equal(AnimalComplaint.Draft, k.Life.AnimalTalk.RecordOf(c.Id).Complaint);
            Assert.Equal("core.oddity.talk.complaint.draft", line.Key);
            Assert.Single(c.Chronicle);
            Assert.True(k.Life.Oddities.Has(OddityIds.AnimalTalk));
            Assert.True(k.Life.Oddities.Has(OddityIds.AchAnimalSpoke));
            Assert.False(k.Life.Oddities.Has(OddityIds.AchRunsInFamily));
            // Karakter başına ömürde bir kez: başka hayvanla da olmaz, menüde de görünmez.
            Assert.NotNull(k.Care.CanDo(c, other.Id, AnimalAction.TalkLikeHuman));
            Assert.False(k.Life.AnimalTalk.Offers(c));
        }

        [Fact]
        public void Complaint_ComesFromTheAnimalsRealState()
        {
            var k = Make();
            var talk = k.Life.AnimalTalk;
            var a = Cow(k, id: 50);
            var b = Cow(k, id: 51);
            Assert.Equal(AnimalComplaint.None, talk.ComplaintOf(a));
            a.Comfort = 30f; Assert.Equal(AnimalComplaint.Draft, talk.ComplaintOf(a));
            a.Fullness = 10f; Assert.Equal(AnimalComplaint.Hungry, talk.ComplaintOf(a));
            a.IsCold = true; Assert.Equal(AnimalComplaint.Cold, talk.ComplaintOf(a));
            a.IsSick = true; Assert.Equal(AnimalComplaint.Sick, talk.ComplaintOf(a));
            // Türünden başka hayvan yoksa yalnızdır.
            b.IsDead = true;
            a.IsSick = false; a.IsCold = false; a.Fullness = 100f; a.Comfort = 80f;
            Assert.Equal(AnimalComplaint.Lonely, talk.ComplaintOf(a));
        }

        [Fact]
        public void SolvingTheComplaintInThreeDays_GivesConfidant()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla", charm: 9);
            var cow = Cow(k, comfort: 20f);
            Cow(k, id: 51);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(c, cow);
            float before = cow.Friendship;

            k.Life.AnimalTalk.OnNewDay();                      // dert sürüyor
            Assert.False(c.Mood.Has(MoodCatalog.Confidant));
            cow.Comfort = 70f;
            k.Life.AnimalTalk.OnNewDay();
            Assert.True(c.Mood.Has(MoodCatalog.Confidant));
            Assert.Equal(before + AnimalTalkService.ConfidantFriendship, cow.Friendship, 3);
            Assert.True(k.Life.AnimalTalk.RecordOf(c.Id).Closed);
        }

        [Fact]
        public void LateFix_GivesNothing()
        {
            var k = Make();
            double now = 0;
            var talk = new AnimalTalkService { Today = () => now, Find = k.Pop.Find, Animals = () => k.Animals.Animals };
            var c = Person(k, 1, "Ayla", charm: 9);
            var cow = Cow(k, comfort: 20f);
            Cow(k, id: 51);
            talk.ForceNext = true;
            talk.Try(c, cow);
            now = 4;
            cow.Comfort = 90f;
            talk.OnNewDay();
            Assert.False(c.Mood.Has(MoodCatalog.Confidant));
        }

        // ---- başkasına anlatmak ----

        private static (Kit k, Character teller) Spoken()
        {
            var k = Make();
            var teller = Person(k, 1, "Ayla", charm: 9);
            var cow = Cow(k);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(teller, cow);
            return (k, teller);
        }

        [Fact]
        public void Tell_OnlyTheOneWhoLivedIt_OncePerListener_NotToBabies()
        {
            var (k, teller) = Spoken();
            var villager = Person(k, 2, "Murat", villager: true);
            var baby = Person(k, 3, "Bebek", age: BabyAge);
            Assert.Equal(AgeStage.Baby, baby.Stage);
            Assert.NotNull(k.Family.CanDo(villager, teller, SocialAction.TellAnimalTalk));   // yaşamayan anlatamaz
            Assert.NotNull(k.Family.CanDo(teller, baby, SocialAction.TellAnimalTalk));
            Assert.Null(k.Family.CanDo(teller, villager, SocialAction.TellAnimalTalk));
            k.Family.Apply(teller, villager, SocialAction.TellAnimalTalk);
            Assert.NotNull(k.Family.CanDo(teller, villager, SocialAction.TellAnimalTalk));   // aynı kişiye bir kez
        }

        [Fact]
        public void Tell_StrangerDoesNotBelieve_ButNothingIsLostForGood()
        {
            var (k, teller) = Spoken();
            var villager = Person(k, 2, "Murat", villager: true);
            k.Family.SetAffection(teller.Id, villager.Id, 40);
            TellResult got = null;
            k.Family.ToldAnimalTalk += (a, b, r) => got = r;

            k.Family.Apply(teller, villager, SocialAction.TellAnimalTalk);

            Assert.Equal(TellOutcome.Disbelief, got.Outcome);
            Assert.Equal("core.oddity.tell.disbelief", got.Line.Key);
            Assert.True(teller.Mood.Has(MoodCatalog.NobodyBelieves));
            Assert.True(villager.Mood.Has(MoodCatalog.OddChat));
            Assert.Equal(40, k.Family.Affection(teller.Id, villager.Id));   // yakınlık düşmez
        }

        [Fact]
        public void Tell_ListenerWhoLivedItToo_Shares()
        {
            var (k, teller) = Spoken();
            var other = Person(k, 2, "Zeynep", charm: 9, villager: true);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(other, Cow(k, id: 60));
            k.Family.Apply(teller, other, SocialAction.TellAnimalTalk);
            Assert.True(teller.Mood.Has(MoodCatalog.NotCrazy));
            Assert.True(other.Mood.Has(MoodCatalog.NotCrazy));
            Assert.Equal(AnimalTalkService.SharedAffection, k.Family.Affection(teller.Id, other.Id));
        }

        [Fact]
        public void Tell_FamilyOfSomeoneWhoLivedIt_SaysItHappenedToUsToo()
        {
            var k = Make();
            // Köylü hanesi: nine yaşamış (sonra ölmüş), torunu dinliyor.
            var granny = Person(k, 10, "Nine", age: ElderAge, charm: 9, villager: true);
            var mother = Person(k, 11, "Anne", villager: true);
            mother.BirthMotherId = granny.Id;
            var grandchild = Person(k, 12, "Torun", age: 30, villager: true);
            grandchild.BirthMotherId = mother.Id;
            var stranger = Person(k, 13, "Yabancı", villager: true);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(granny, Cow(k, id: 60));

            var teller = Person(k, 1, "Ayla", charm: 9);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(teller, Cow(k, id: 61));

            Assert.Equal(TellOutcome.FamilyKnows, k.Life.AnimalTalk.OutcomeFor(grandchild));
            Assert.Equal(TellOutcome.FamilyKnows, k.Life.AnimalTalk.OutcomeFor(mother));
            Assert.Equal(TellOutcome.Disbelief, k.Life.AnimalTalk.OutcomeFor(stranger));

            TellResult got = null;
            k.Family.ToldAnimalTalk += (a, b, r) => got = r;
            k.Family.Apply(teller, grandchild, SocialAction.TellAnimalTalk);
            Assert.Equal("core.oddity.tell.family", got.Line.Key);
            Assert.True(teller.Mood.Has(MoodCatalog.Believed));
            Assert.True(grandchild.Mood.Has(MoodCatalog.FamilyTale));
        }

        [Fact]
        public void Tell_InThePlayersOwnHouse_FamilyBelieves_AndSecondGenerationUnlocksHiddenAchievement()
        {
            var (k, teller) = Spoken();
            var spouse = Person(k, 2, "Kemal");
            Assert.Equal(TellOutcome.FamilyKnows, k.Life.AnimalTalk.OutcomeFor(spouse));   // aynı hane
            Assert.False(k.Life.Oddities.Has(OddityIds.AchRunsInFamily));
            var child = Person(k, 3, "Elif", age: 40, charm: 9, generation: 2);
            child.BirthMotherId = teller.Id;
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(child, Cow(k, id: 60));
            Assert.True(k.Life.Oddities.Has(OddityIds.AchRunsInFamily));
        }

        [Fact]
        public void Tell_ChildrenAlwaysBelieve()
        {
            var (k, teller) = Spoken();
            var kid = Person(k, 2, "Can", age: ChildAge, villager: true);
            Assert.Equal(AgeStage.Child, kid.Stage);
            Assert.Equal(TellOutcome.ChildBelieves, k.Life.AnimalTalk.OutcomeFor(kid));
            k.Family.Apply(teller, kid, SocialAction.TellAnimalTalk);
            Assert.False(teller.Mood.Has(MoodCatalog.NobodyBelieves));
        }

        [Fact]
        public void ThreeDisbelievers_GiveASeasonLongNickname()
        {
            var (k, teller) = Spoken();
            for (int i = 0; i < 3; i++)
            {
                var v = Person(k, 20 + i, "Köylü" + i, villager: true);
                Assert.False(k.Life.AnimalTalk.HasNickname(teller));
                var r = k.Life.AnimalTalk.Tell(teller, v);
                Assert.Equal(i == 2, r.NicknameGiven);
            }
            Assert.True(k.Life.AnimalTalk.HasNickname(teller));
            Assert.True(k.Life.Oddities.Has(OddityIds.TalkNickname));
            k.Clock.Restore(k.Clock.TotalDays + k.Clock.Config.DaysPerSeason + 1, 1);
            Assert.False(k.Life.AnimalTalk.HasNickname(teller));
        }

        // ---- bulunan silah ----

        private static FoundRelicService Relics(out int removed, out string given, OddityBook book = null)
        {
            int rem = 0; string giv = null;
            var svc = new FoundRelicService { Book = book ?? new OddityBook(), Place = id => (77, 5, 6) };
            svc.Remove = id => rem = id;
            svc.AddToHouse = (id, n) => giv = id;
            removed = 0; given = null;
            return svc;
        }

        [Fact]
        public void Sword_AppearsAfterSecondSeason_GuaranteedByEndOfFirstYear()
        {
            var svc = Relics(out _, out _);
            const int season = 28;
            Assert.False(svc.ShouldSpawn(RelicKind.Sword, 5, season, 0.0));            // ilk mevsimde çıkmaz
            Assert.False(svc.ShouldSpawn(RelicKind.Sword, season, season, 0.99));
            Assert.True(svc.ShouldSpawn(RelicKind.Sword, season, season, 0.0));
            Assert.True(svc.ShouldSpawn(RelicKind.Sword, 4 * season - 1, season, 0.99)); // yıl sonunda garanti

            Assert.Equal(RelicKind.None, svc.OnNewDay(3, season, false, 1, roll: 0.0));
            Assert.Equal(RelicKind.Sword, svc.OnNewDay(season, season, false, 1, roll: 0.0));
            Assert.Equal(RelicStage.Lying, svc.Stage);
            Assert.Equal(77, svc.ItemId);
            // Yerdeyken ikincisi çıkmaz.
            Assert.Equal(RelicKind.None, svc.OnNewDay(season + 1, season, false, 1, roll: 0.0));
        }

        [Fact]
        public void Sword_NoFreeSpot_TriesAgainNextDay()
        {
            var svc = new FoundRelicService { Place = id => null };
            Assert.Equal(RelicKind.None, svc.OnNewDay(200, 28, false, 1, roll: 0.0));
            Assert.Equal(RelicStage.None, svc.Stage);
            svc.Place = id => (9, 1, 1);
            Assert.Equal(RelicKind.Sword, svc.OnNewDay(201, 28, false, 1, roll: 0.0));
        }

        [Fact]
        public void Inspect_GivesThoughtful_Line_Chronicle_AndAchievement()
        {
            var book = new OddityBook();
            var svc = Relics(out _, out _, book);
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            var adult = new Character(1, "Ayla", new GridPos(0, 0), true) { MoodEnabled = true }; adult.SetAge(40);

            var line = svc.Inspect(adult, RelicKind.Sword);

            Assert.True(line.First);
            Assert.Equal("core.oddity.relic.sword.adult", line.Line.Key);
            Assert.True(adult.Mood.Has(MoodCatalog.Thoughtful));
            Assert.Single(adult.Chronicle);
            Assert.True(book.Has(OddityIds.FoundSword));
            Assert.True(book.Has(OddityIds.AchQuestionTheSword));
            // İkinci bakış sahneyi tekrarlamaz.
            Assert.False(svc.Inspect(adult, RelicKind.Sword).First);
            Assert.Single(adult.Chronicle);
            Assert.Equal(1, svc.FoundCount);
        }

        [Fact]
        public void Inspect_ChildAsks_ParentAnswers_ElderHasOwnLine()
        {
            var svc = Relics(out _, out _);
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            var child = new Character(1, "Can", new GridPos(0, 0), true); child.SetAge(ChildAge);
            var elder = new Character(2, "Dede", new GridPos(0, 0), true); elder.SetAge(ElderAge);
            Assert.Equal(AgeStage.Elder, elder.Stage);

            var alone = svc.Inspect(child, RelicKind.Sword, parentNearby: false);
            Assert.Equal("core.oddity.relic.sword.child", alone.Line.Key);
            Assert.True(alone.Reply.IsEmpty);
            var withParent = svc.Inspect(child, RelicKind.Sword, parentNearby: true);
            Assert.Equal("core.oddity.relic.sword.parent_reply", withParent.Reply.Key);
            Assert.Equal("core.oddity.relic.sword.elder", svc.Inspect(elder, RelicKind.Sword).Line.Key);
        }

        [Fact]
        public void Bury_RemovesSword_AndFlowerBloomsNextSpring()
        {
            var book = new OddityBook();
            int removed = 0; (string id, int x, int y)? flower = null;
            var svc = new FoundRelicService { Book = book, Place = id => (77, 5, 6), Remove = id => removed = id };
            svc.PlaceAt = (id, x, y) => { flower = (id, x, y); return true; };
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            var c = new Character(1, "Ayla", new GridPos(0, 0), true); c.SetAge(40);
            svc.Inspect(c, RelicKind.Sword);

            Assert.True(svc.Bury(41));
            Assert.Equal(77, removed);
            Assert.Equal(SwordFate.Buried, svc.Fate);
            Assert.True(book.Has(OddityIds.SwordBuried));
            Assert.False(svc.Bury(42));

            svc.OnNewDay(50, 28, false, 1, roll: 0.99);   // ilkbahar değil
            Assert.Null(flower);
            svc.OnNewDay(113, 28, true, 1, roll: 0.99);
            Assert.Equal((FoundRelicService.FlowerItem, 5, 6), flower.Value);
            Assert.True(svc.Bloomed);
        }

        [Fact]
        public void Take_GivesOldSword_WhichBecomesPloughshareOrWallPiece()
        {
            var book = new OddityBook();
            string given = null;
            var svc = new FoundRelicService { Book = book, Place = id => (77, 5, 6), Remove = id => { }, AddToHouse = (id, n) => given = id };
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            Assert.Equal(FoundRelicService.OldSwordItem, svc.Take());
            Assert.Equal(FoundRelicService.OldSwordItem, given);
            Assert.Equal(SwordFate.Taken, svc.Fate);
            Assert.Null(svc.Take());
            svc.OnCrafted("item.sword_ploughshare");
            svc.OnCrafted("item.old_sword_wall");
            Assert.True(book.Has(OddityIds.SwordPloughshare));
            Assert.True(book.Has(OddityIds.SwordOnWall));
        }

        [Fact]
        public void LaterGenerations_FindOtherRelics_OnePerGeneration()
        {
            var svc = Relics(out _, out _);
            var c = new Character(1, "Ayla", new GridPos(0, 0), true); c.SetAge(40);
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            svc.Inspect(c, RelicKind.Sword);
            svc.Take();
            // Aynı kuşakta yenisi çıkmaz.
            Assert.Equal(RelicKind.None, svc.OnNewDay(60, 28, false, 1, roll: 0.0));
            // Yeni kuşak: içinde kuş yuvası olan miğfer. Alınmaz; bir mevsim sonra kaybolur.
            Assert.Equal(RelicKind.HelmetNest, svc.OnNewDay(300, 28, false, 2, roll: 0.0));
            Assert.Null(svc.Take());
            Assert.Equal("core.oddity.relic.helmet.adult", svc.Inspect(c, RelicKind.HelmetNest).Line.Key);
            Assert.Equal(RelicKind.None, svc.OnNewDay(300 + 28, 28, false, 2, roll: 0.99));
            Assert.Equal(RelicStage.Resolved, svc.Stage);
            Assert.Equal(RelicKind.BrokenShield, svc.OnNewDay(600, 28, false, 3, roll: 0.0));
        }

        [Fact]
        public void LifeSystems_PlacesNothingOnSmallMaps_AndNeverThrows()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            k.Life.Relics.ForceNext = true;
            k.Clock.Restore(2.0, 1);
            k.Life.Tick(0.01);
            Assert.Equal(RelicStage.None, k.Life.Relics.Stage);   // köy yolu yok: çıkmaz, zorlama yarına kalır
            Assert.True(k.Life.Relics.ForceNext);
        }

        [Fact]
        public void LifeSystems_PlacesTheSwordOnTheVillageRoad()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            Regions.Apply(map);
            var world = new WorldState(map);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, new Household(0), pop, clock, 9);
            var c = new Character(1, "Ayla", new GridPos(2, 2), true) { CanStarve = false }; c.SetAge(40);
            pop.Add(c);
            life.Relics.ForceNext = true;
            clock.Restore(1.0, 1);
            life.Tick(0.01);

            Assert.Equal(RelicKind.Sword, life.Relics.Kind);
            var item = world.Items.FirstOrDefault(i => i.Id == life.Relics.ItemId);
            Assert.NotNull(item);
            Assert.Equal(FoundRelicService.SwordItem, item.Def.Id);
            Assert.True(Regions.Road.Contains(item.Position));
        }

        // ---- küçük tuhaflıklar ----

        [Fact]
        public void ThirdChatOfTheDay_TooManyHellos_OncePerDay()
        {
            var h = new HelloCounter();
            Assert.False(h.OnChat(1, 2, 0));
            Assert.False(h.OnChat(2, 1, 0));
            Assert.True(h.OnChat(1, 2, 0));
            Assert.False(h.OnChat(1, 2, 0));
            Assert.False(h.OnChat(1, 3, 0));
            Assert.False(h.OnChat(1, 2, 1));   // yeni gün sıfırdan
        }

        [Fact]
        public void TooManyHellos_ThroughFamilyChat()
        {
            var k = Make();
            var a = Person(k, 1, "Ayla");
            var b = Person(k, 2, "Murat", villager: true);
            LocText line = default;
            k.Life.OddityLine += (l, id) => line = l;
            for (int i = 0; i < 3; i++) k.Family.Apply(a, b, SocialAction.Chat);
            Assert.Equal("core.oddity.hello", line.Key);
            Assert.True(k.Life.Oddities.Has(OddityIds.TooManyHellos));
        }

        [Fact]
        public void KissingTheFrog_DoesNothing_ExceptRarelyACrown()
        {
            var book = new OddityBook();
            var c = new Character(1, "Ayla", new GridPos(0, 0), true) { MoodEnabled = true }; c.SetAge(40);
            Assert.False(FrogKiss.Kiss(c, book, roll: 0.5));
            Assert.True(c.Mood.Has(MoodCatalog.LetDown));
            Assert.True(book.Has(OddityIds.FrogKiss));
            Assert.False(book.Has(OddityIds.FrogCrown));
            Assert.True(FrogKiss.Kiss(c, book, roll: 0.0));
            Assert.True(book.Has(OddityIds.FrogCrown));
        }

        [Fact]
        public void Fortune_CanBreakTheFourthWall()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            LocText line = default;
            k.Life.OddityLine += (l, id) => line = l;
            k.Life.ForceFortuneOddity = true;
            k.Life.OnInteractionApplied(c, new InteractionDef { Id = LifeSystems.FortuneInteraction });
            Assert.Equal("core.oddity.fortune", line.Key);
            Assert.True(k.Life.Oddities.Has(OddityIds.FortuneFourthWall));
        }

        // ---- Tuhaflıklar sayfası ve kayıt ----

        [Fact]
        public void OddityBook_OpensWithFirstOddity_CountsAchievementsSeparately()
        {
            var book = new OddityBook();
            int events = 0;
            book.Discovered += id => events++;
            Assert.False(book.PageOpen);
            Assert.True(book.Mark(OddityIds.FrogKiss));
            Assert.False(book.Mark(OddityIds.FrogKiss));
            Assert.True(book.Mark(OddityIds.AchAnimalSpoke));
            Assert.Equal(2, events);
            Assert.True(book.PageOpen);
            Assert.Equal(1, book.SeenCount);
            Assert.Equal(1, book.AchievementCount);
            Assert.Equal(OddityIds.All.Length, OddityBook.Total);
            Assert.Equal(OddityIds.All.Length, OddityIds.All.Distinct().Count());
            Assert.All(OddityIds.Achievements, id => Assert.True(OddityIds.IsAchievement(id)));
        }

        [Fact]
        public void Save_RoundTrips_ThroughLifeSaveData()
        {
            var (k, teller) = Spoken();
            var v = Person(k, 2, "Murat", villager: true);
            k.Life.AnimalTalk.Tell(teller, v);
            k.Life.Oddities.Mark(OddityIds.FrogKiss);

            var json = JsonSerializer.Serialize(k.Life.Export(), new JsonSerializerOptions { IncludeFields = true });
            var data = JsonSerializer.Deserialize<LifeSaveData>(json, new JsonSerializerOptions { IncludeFields = true });

            var k2 = Make();
            var teller2 = Person(k2, 1, "Ayla", charm: 9);
            var v2 = Person(k2, 2, "Murat", villager: true);
            k2.Life.Restore(data);
            Assert.True(k2.Life.AnimalTalk.HasSpoken(teller2));
            Assert.NotNull(k2.Life.AnimalTalk.CanTell(teller2, v2));   // anlatılmış kişi hatırlanır
            Assert.Equal(1, k2.Life.AnimalTalk.RecordOf(1).Disbelievers);
            Assert.True(k2.Life.Oddities.Has(OddityIds.FrogKiss));
            Assert.True(k2.Life.Oddities.Has(OddityIds.AchAnimalSpoke));
        }

        [Fact]
        public void OldSave_WithoutOddities_StartsEmpty()
        {
            var k = Make();
            k.Life.Oddities.Mark(OddityIds.FrogKiss);
            k.Life.Restore(new LifeSaveData());
            Assert.False(k.Life.Oddities.PageOpen);
            Assert.Empty(k.Life.AnimalTalk.Records);
            Assert.Equal(RelicStage.None, k.Life.Relics.Stage);
        }

        [Fact]
        public void EveryOddityTextKey_ExistsInBothUiTables()
        {
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var errors = new System.Collections.Generic.List<string>();
            var en = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.en.json")), errors, "ui.en.json");
            var tr = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.tr.json")), errors, "ui.tr.json");
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var keys = new System.Collections.Generic.List<string>();
            foreach (AnimalComplaint c in Enum.GetValues(typeof(AnimalComplaint))) keys.Add("core.oddity.talk.complaint." + AnimalTalkService.ComplaintKey(c));
            keys.AddRange(new[] { "core.oddity.talk.stare", "core.oddity.talk.confided", "core.oddity.tell.shared", "core.oddity.tell.family",
                "core.oddity.tell.child", "core.oddity.tell.disbelief", "core.oddity.tell.disbelief_nickname", "core.oddity.hello",
                "core.oddity.relic.sword.parent_reply", "core.oddity.relic.buried", "core.oddity.relic.taken", "core.oddity.fortune",
                "core.oddity.frog.kiss", "core.oddity.frog.crown" });
            foreach (var what in new[] { "sword", "helmet", "shield" })
                foreach (var age in new[] { "adult", "child", "elder" }) keys.Add($"core.oddity.relic.{what}.{age}");
            foreach (var id in OddityIds.All) keys.Add("ui.oddity.name." + id.Substring("oddity.".Length));
            foreach (var id in OddityIds.Achievements) keys.Add("ui.oddity.ach." + id.Substring("ach.".Length));
            foreach (var key in keys)
            {
                Assert.True(en.TryGet(key, out _), "EN eksik: " + key);
                Assert.True(tr.TryGet(key, out _), "TR eksik: " + key);
            }
        }
    }
}
