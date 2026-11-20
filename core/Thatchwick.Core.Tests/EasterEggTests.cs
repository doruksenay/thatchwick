using System;
using System.Collections.Generic;
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
using Thatchwick.Core.Work;
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
                         "item.sword_ploughshare", "item.old_sword_wall", FoundRelicService.FlowerItem,
                         FoundRelicService.HelmetGardenItem, FoundRelicService.ShieldGardenItem, LifeSystems.OldBootItem,
                         LifeSystems.MessageBottleItem, "item.bone", LifeSystems.RelicCoinItem, LifeSystems.SpinningTopItem })
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
            Assert.Equal((double)(before + AnimalTalkService.ConfidantFriendship), (double)cow.Friendship, 3);
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
        public void LaterGenerations_FindOtherRelics_WhichStayAsOrnaments()
        {
            string given = null; int removed = 0;
            var svc = new FoundRelicService { Book = new OddityBook(), Place = id => (77, 5, 6), Remove = id => removed = id, AddToHouse = (id, n) => given = id };
            var c = new Character(1, "Ayla", new GridPos(0, 0), true); c.SetAge(40);
            svc.OnNewDay(40, 28, false, 1, roll: 0.0);
            svc.Inspect(c, RelicKind.Sword);
            svc.Take();
            // Aynı kuşakta yenisi çıkmaz.
            Assert.Equal(RelicKind.None, svc.OnNewDay(60, 28, false, 1, roll: 0.0));
            // Yeni kuşak: içinde kuş yuvası olan miğfer.
            Assert.Equal(RelicKind.HelmetNest, svc.OnNewDay(300, 28, false, 2, roll: 0.0));
            Assert.Null(svc.Take());                                                         // Take yalnızca kılıç için
            Assert.Equal(RelicKind.None, svc.OnNewDay(301, 28, false, 3, roll: 0.0));        // bulunmadan yenisi çıkmaz
            Assert.Equal("core.oddity.relic.helmet.adult", svc.Inspect(c, RelicKind.HelmetNest).Line.Key);
            // Kaybolmaz: mevsimler sonra da yerinde (kullanıcı 20.11.2026).
            svc.OnNewDay(300 + 90, 28, false, 2, roll: 0.99);
            Assert.Equal(77, svc.ItemId);
            Assert.Equal(RelicStage.Inspected, svc.Stage);
            // Yolda süs olarak bırakılsa da sonraki kuşakta yenisi çıkar; eskisi sonradan da alınabilir.
            svc.Place = id => (88, 7, 7);
            Assert.Equal(RelicKind.BrokenShield, svc.OnNewDay(600, 28, false, 3, roll: 0.0));
            Assert.Equal(FoundRelicService.HelmetGardenItem, svc.TakeLater(RelicKind.HelmetNest, 77));
            Assert.Equal(77, removed);
            Assert.Equal(FoundRelicService.HelmetGardenItem, given);
            Assert.Equal(RelicStage.Lying, svc.Stage);                                       // yeni kalkanın durumu bozulmaz
            Assert.False(svc.Inspect(c, RelicKind.HelmetNest, tracked: false).First);
            Assert.Equal(RelicStage.Lying, svc.Stage);
            Assert.True(svc.Inspect(c, RelicKind.BrokenShield).First);
            Assert.Equal(FoundRelicService.ShieldGardenItem, svc.TakeLater(RelicKind.BrokenShield, 88));
            Assert.Equal(RelicStage.Resolved, svc.Stage);
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
        // ---- hayvanın derdi: yavru ve yem geçmişi ----

        [Fact]
        public void Complaint_LostYoung_WhenOffspringIsSold()
        {
            var k = Make();
            var mother = Cow(k, id: 50);
            Cow(k, id: 51);
            var calf = k.Animals.Restore(52, k.Db.Animals.Get("animal.cow"), new GridPos(5, 4), 0, 100f, 5, 0, 0, 30f, comfort: 80f, motherId: 50);
            Assert.Equal(AnimalComplaint.None, k.Life.AnimalTalk.ComplaintOf(mother));
            Assert.True(k.Animals.Sell(calf.Id, k.House).Ok);
            Assert.Equal(AnimalComplaint.LostYoung, k.Life.AnimalTalk.ComplaintOf(mother));
            Assert.Equal("lost_young", AnimalTalkService.ComplaintKey(AnimalComplaint.LostYoung));
            // Zamanla unutur.
            k.Clock.Restore(AnimalTalkService.LostYoungDays + 1, 1);
            Assert.Equal(AnimalComplaint.None, k.Life.AnimalTalk.ComplaintOf(mother));
        }

        [Fact]
        public void Complaint_SameFeed_AfterThreeIdenticalFeedings_UntilVariety()
        {
            var k = Make();
            var cow = Cow(k, id: 50);
            Cow(k, id: 51);
            var talk = k.Life.AnimalTalk;
            talk.OnFeed("item.feed"); talk.OnFeed("item.feed");
            Assert.Equal(AnimalComplaint.None, talk.ComplaintOf(cow));
            talk.OnFeed("item.feed");
            Assert.Equal(AnimalComplaint.SameFeed, talk.ComplaintOf(cow));
            Assert.True(talk.StillTroubled(cow, AnimalComplaint.SameFeed));
            talk.OnFeed("item.hay");
            Assert.False(talk.StillTroubled(cow, AnimalComplaint.SameFeed));
            // Kayıt: geçmiş korunur.
            var copy = new AnimalTalkService();
            talk.OnFeed("item.hay"); talk.OnFeed("item.hay");
            copy.ImportHistory(talk.ExportHistory());
            Assert.True(copy.BoredOfFeed);
        }

        // ---- küçük easter egg'ler ----

        private static (LocText line, int count) Capture(Kit k, Action act)
        {
            LocText last = default; int n = 0;
            Action<LocText, int> h = (l, id) => { last = l; n++; };
            k.Life.OddityLine += h;
            act();
            k.Life.OddityLine -= h;
            return (last, n);
        }

        private static Animal Pet(Kit k, string species, int id, string name = null)
        {
            var a = k.Animals.Restore(id, k.Db.Animals.Get(species), new GridPos(4, 4), 0, 100f, 40, 0, 0, 30f, comfort: 80f);
            if (name != null) a.Name = name;
            return a;
        }

        [Fact]
        public void OddityEvents_Chance_Force_AndDailyCounters()
        {
            var ev = new OddityEvents { Roll = () => 0.99 };
            Assert.False(ev.Chance(OddityEvents.Crier, 0.5));
            ev.Force(OddityEvents.Crier);
            Assert.True(ev.Chance(OddityEvents.Crier, 0));
            Assert.False(ev.Chance(OddityEvents.Crier, 0));          // zorlama bir kez
            Assert.True(ev.FirstToday("x", 3));
            Assert.False(ev.FirstToday("x", 3));
            Assert.True(ev.FirstToday("x", 4));
            Assert.Equal(1, ev.CountToday("y", 1));
            Assert.Equal(2, ev.CountToday("y", 1));
            Assert.Equal(1, ev.CountToday("y", 2));
            Assert.True(OddityEvents.IsFullMoon(13));
            Assert.False(OddityEvents.IsFullMoon(12));
            Assert.True(OddityEvents.IsNight(0.9));
            Assert.False(OddityEvents.IsNight(0.5));
        }

        [Fact]
        public void JohanWick_PetsTheDog_LeavesACoin_MoreOftenForDaisy()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            var dog = Pet(k, "animal.dog", 70, "Daisy");
            Noon(k);
            float before = dog.Friendship;
            k.Life.OddEvents.Force(OddityEvents.JohanWick);
            var (line, n) = Capture(k, () => { k.Life.OddEventsNewDay(5); RunWalkers(k); });
            Assert.Equal("core.oddity.ev.johan_wick.nofield", line.Key);
            Assert.Equal(before + 5f, dog.Friendship);
            Assert.Equal(1, k.House.Count(LifeSystems.RelicCoinItem));
            Assert.True(k.Life.Oddities.Has(OddityIds.JohanWick));
            Assert.True(OddityEvents.JohanChance * OddityEvents.JohanDaisyFactor > OddityEvents.JohanChance);
        }

        [Fact]
        public void JohanWick_NeedsADog()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            k.Life.OddEvents.Force(OddityEvents.JohanWick);
            k.Life.OddEventsNewDay(5);
            Assert.False(k.Life.Oddities.Has(OddityIds.JohanWick));
            Assert.Equal(0, k.House.Count(LifeSystems.RelicCoinItem));
        }

        [Fact]
        public void DailyEvents_Crier_Goat_Cat_AprilFirst()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            k.Life.OddEvents.Roll = () => 0.999;      // zorlanmayan hiçbir şey olmasın
            Assert.Equal(0, Capture(k, () => k.Life.OddEventsNewDay(1)).count);

            k.Life.OddEvents.Force(OddityEvents.Crier);
            Assert.StartsWith("core.oddity.ev.crier.v", Capture(k, () => k.Life.OddEventsNewDay(2)).line.Key);
            Assert.True(k.Life.Oddities.Has(OddityIds.Crier));

            k.Life.OddEvents.Force(OddityEvents.ScreamingGoat);
            Assert.Equal(0, Capture(k, () => k.Life.OddEventsNewDay(3)).count);   // keçi yok
            k.Life.OddEvents.Chance(OddityEvents.ScreamingGoat, 0);
            Pet(k, "animal.goat", 71);
            k.Life.OddEvents.Force(OddityEvents.ScreamingGoat);
            Assert.StartsWith("core.oddity.ev.screaming_goat.", Capture(k, () => k.Life.OddEventsNewDay(4)).line.Key);

            Pet(k, "animal.cat", 72, "Alex");
            k.Life.OddEvents.Force(OddityEvents.CatGift);
            Assert.Equal("core.oddity.ev.cat_gift", Capture(k, () => k.Life.OddEventsNewDay(5)).line.Key);

            // 1 Nisan: gerçek tarih; gerçek yılda bir kez.
            k.Life.RealNow = () => new DateTime(2027, 4, 1);
            Assert.Equal("core.oddity.ev.april_first.crier", Capture(k, () => k.Life.OddEventsNewDay(6)).line.Key);
            Assert.Equal(0, Capture(k, () => k.Life.OddEventsNewDay(7)).count);
            k.Life.RealNow = () => new DateTime(2027, 4, 2);
            Assert.Equal(0, Capture(k, () => k.Life.OddEventsNewDay(8)).count);
        }

        [Fact]
        public void Chat_NameTwin_NameSurprise_NicknameAndElder()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var a = Person(k, 1, "Murat");
            var twin = Person(k, 2, "Murat", villager: true);
            Assert.Equal("core.oddity.ev.name_twin", Capture(k, () => k.Life.OnOddChat(a, twin)).line.Key);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddChat(a, twin)).count);    // bir kez

            var doruk = Person(k, 3, "Doruk");
            var v = Person(k, 4, "Zeynep", villager: true);
            Assert.Equal("core.oddity.ev.name_surprise.doruk", Capture(k, () => k.Life.OnOddChat(doruk, v)).line.Key);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddChat(doruk, v)).count);
            Assert.Null(LifeSystems.NameSurpriseKey("Ayla"));

            var elder = Person(k, 5, "Dede", age: ElderAge, villager: true);
            k.Life.OddEvents.Force(OddityEvents.AbsentElder);
            Assert.Equal("core.oddity.ev.absent_elder", Capture(k, () => k.Life.OnOddChat(a, elder)).line.Key);
            // İki hane üyesi ya da iki köylü arasında tetiklenmez.
            k.Life.OddEvents.Force(OddityEvents.AbsentElder);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddChat(a, doruk)).count);
        }

        [Fact]
        public void Rumor_ComesBackExaggeratedTheNextDay()
        {
            var ev = new OddityEvents { Book = new OddityBook(), Roll = () => 0.0 };
            Assert.False(ev.OnGossip(1, "Ayla", 3));          // bugün dikkat çekici bir iş yok
            ev.NoteWork(1, "fish", 3);
            Assert.True(ev.OnGossip(1, "Ayla", 3));
            Assert.False(ev.OnGossip(1, "Ayla", 3));          // bekleyen söylenti varken ikincisi olmaz
            Assert.Null(ev.TakeRumor(3));                     // aynı gün dönmez
            var r = ev.TakeRumor(4);
            Assert.Equal(("Ayla", "fish"), r.Value);
            Assert.Null(ev.TakeRumor(5));
            Assert.True(ev.Book.Has(OddityIds.Rumor));
        }

        [Fact]
        public void TheUsual_AfterTenOrders_ThenForTheGrandchildren()
        {
            var ev = new OddityEvents { Book = new OddityBook() };
            for (int i = 1; i < OddityEvents.UsualOrders; i++) Assert.Equal(0, ev.OnTavernOrder("item.stew", "Ayla", 1, i));
            Assert.Equal(1, ev.OnTavernOrder("item.stew", "Ayla", 1, 10));
            Assert.Equal("Ayla", ev.UsualName);
            Assert.Equal(0, ev.OnTavernOrder("item.stew", "Ayla", 1, 11));     // aynı kuşak: tekrar söylenmez
            Assert.Equal(0, ev.OnTavernOrder("item.bread", "Elif", 3, 50));    // başka şey
            Assert.Equal(2, ev.OnTavernOrder("item.stew", "Elif", 3, 50));     // torun: "dedeninkinden mi?"
            Assert.Equal(0, ev.OnTavernOrder("item.stew", "Elif", 3, 50));     // günde bir
        }

        [Fact]
        public void VillageMemory_RemembersAnEarlierGenerationsFall()
        {
            var ev = new OddityEvents { Book = new OddityBook() };
            Assert.Null(ev.OnFall("Dede", 1));
            Assert.Null(ev.OnFall("Kardeş", 1));          // aynı kuşak
            Assert.Equal("Dede", ev.OnFall("Torun", 3));
            Assert.True(ev.Book.Has(OddityIds.VillageMemory));
        }

        [Fact]
        public void SameOutfit_OncePerSeason_ResetsOnChange()
        {
            var ev = new OddityEvents { Book = new OddityBook() };
            Assert.False(ev.SameOutfitTooLong(1, 0, 28));     // ilk bakış: sayaç başlar
            Assert.False(ev.SameOutfitTooLong(1, 20, 28));
            Assert.True(ev.SameOutfitTooLong(1, 28, 28));
            Assert.False(ev.SameOutfitTooLong(1, 30, 28));    // söylendi; bir mevsim sonra
            ev.OnOutfitChanged(1, 50);
            Assert.False(ev.SameOutfitTooLong(1, 70, 28));
            Assert.True(ev.SameOutfitTooLong(1, 78, 28));
        }

        [Fact]
        public void Shoo_ThirdTimeInADay_TheCoopChases()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            var hen = Pet(k, "animal.chicken", 80, "Pıtır");
            var cow = Cow(k);
            Assert.NotNull(k.Care.CanDo(c, cow.Id, AnimalAction.Shoo));       // yalnızca tavuk
            Assert.Null(k.Care.CanDo(c, hen.Id, AnimalAction.Shoo));
            Assert.Equal("core.oddity.ev.shoo", Capture(k, () => k.Care.Apply(c, hen.Id, AnimalAction.Shoo)).line.Key);
            k.Care.Apply(c, hen.Id, AnimalAction.Shoo);
            Assert.False(k.Life.Oddities.Has(OddityIds.ChickenRevenge));
            Assert.Equal("core.oddity.ev.chicken_revenge", Capture(k, () => k.Care.Apply(c, hen.Id, AnimalAction.Shoo)).line.Key);
            Assert.True(k.Life.Oddities.Has(OddityIds.ChickenRevenge));
            Assert.Equal(0, Capture(k, () => k.Care.Apply(c, hen.Id, AnimalAction.Shoo)).count);   // dördüncüde sessiz
        }

        [Fact]
        public void Dog_ThousandthPat_BringsAGift_AndGetsJealousOfTheCat()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            var dog = Pet(k, "animal.dog", 70, "Karabaş");
            var cat = Pet(k, "animal.cat", 72, "Alex");
            k.Life.OddEvents.Set("pets:" + dog.Id, OddityEvents.DogGiftPets - 1);
            var got = Capture(k, () => k.Care.Apply(c, dog.Id, AnimalAction.Pet));
            Assert.Equal("core.oddity.ev.dog_gift.bone", got.line.Key);
            Assert.Equal(1, k.House.Count("item.bone"));
            Assert.True(k.Life.Oddities.Has(OddityIds.DogGift));

            // Kıskanç köpek: bugün sevilmemiş köpek, kedi sevilirken araya girer.
            dog.PetCooldownDays = 0;
            k.Life.OddEvents.Force(OddityEvents.JealousDog);
            Assert.Equal("core.oddity.ev.jealous_dog", Capture(k, () => k.Care.Apply(c, cat.Id, AnimalAction.Pet)).line.Key);
        }

        [Fact]
        public void Fishing_Boot_Bottle_AndFishTale()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            Assert.Equal(0, Capture(k, () => k.Life.OnOddFish(c, "item.fish_carp")).count);
            k.Life.OddEvents.Force(OddityEvents.OldBoot);
            Assert.Equal("core.oddity.ev.old_boot", Capture(k, () => k.Life.OnOddFish(c, "item.fish_carp")).line.Key);
            Assert.Equal(1, k.House.Count(LifeSystems.OldBootItem));
            k.Life.OddEvents.Force(OddityEvents.MessageBottle);
            Assert.StartsWith("core.oddity.ev.message_bottle.v", Capture(k, () => k.Life.OnOddFish(c, "item.fish_carp")).line.Key);
            Assert.Equal(1, k.House.Count(LifeSystems.MessageBottleItem));
        }

        [Fact]
        public void Mine_TabletFirst_ThenSwordInStone_OnceEach()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            Assert.Equal(0, Capture(k, () => k.Life.OnMineDug(c)).count);
            k.Life.OddEvents.Force(OddityEvents.MineTablet);
            Assert.Equal("core.oddity.ev.mine_tablet", Capture(k, () => k.Life.OnMineDug(c)).line.Key);
            k.Life.OddEvents.Force(OddityEvents.MineTablet);
            k.Life.OddEvents.Force(OddityEvents.SwordInStone);
            Assert.Equal("core.oddity.ev.sword_in_stone", Capture(k, () => k.Life.OnMineDug(c)).line.Key);
            k.Life.OddEvents.Force(OddityEvents.SwordInStone);
            Assert.Equal(0, Capture(k, () => k.Life.OnMineDug(c)).count);
        }

        [Fact]
        public void TopShow_MemeReactions_AndTheTopThatNeverStops()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            var top = new Thatchwick.Core.Upbringing.ToyDef { ItemId = LifeSystems.SpinningTopItem };
            var show = new Thatchwick.Core.Upbringing.ToyShow { Performer = c, Toy = top, Kind = Thatchwick.Core.Upbringing.ShowKind.Show, Watchers = 2 };
            Assert.Equal(0, Capture(k, () => k.Life.OnOddShow(show)).count);
            k.Life.OddEvents.Force(OddityEvents.TopShow);
            Assert.StartsWith("core.oddity.ev.top_show.v", Capture(k, () => k.Life.OnOddShow(show)).line.Key);
            k.Life.OddEvents.Force(OddityEvents.NeverStoppingTop);
            Assert.Equal("core.oddity.ev.never_stopping_top", Capture(k, () => k.Life.OnOddShow(show)).line.Key);
            // Seyirci yoksa tepki de yok; başka oyuncak sayılmaz.
            show.Watchers = 0;
            k.Life.OddEvents.Force(OddityEvents.TopShow);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddShow(show)).count);
        }

        [Fact]
        public void Scarecrow_AnswersOnlyTheLonely()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            var scarecrow = k.World.PlaceItem(k.Db.Items.Get(LifeSystems.ScarecrowItem), new GridPos(6, 6));
            var inter = scarecrow.Def.Interactions.Find(i => i.Id == LifeSystems.ScarecrowInteraction);
            Assert.NotNull(inter);
            Assert.Equal("core.oddity.ev.scarecrow.silent", Capture(k, () => k.Life.OnOddItemUsed(c, scarecrow, inter)).line.Key);
            c.AddMoodlet(MoodCatalog.Lonely, "Yalnız", -10, 1);
            c.Needs.Set(NeedType.Social, 10f);
            Assert.Equal("core.oddity.ev.scarecrow.answered", Capture(k, () => k.Life.OnOddItemUsed(c, scarecrow, inter)).line.Key);
            Assert.Equal(10f + LifeSystems.ScarecrowSocial, c.Needs.Get(NeedType.Social));
            Assert.True(MoodCatalog.Default.Contains(MoodCatalog.ScarecrowChat));
        }

        [Fact]
        public void NewBuilds_OxStares_CatSitsInTheBox()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.0;
            Person(k, 1, "Ayla");
            Pet(k, "animal.ox", 73, "Sarı");
            var stool = k.World.PlaceItem(k.Db.Items.Get("item.stool_wood"), new GridPos(6, 6));
            Assert.Equal("core.oddity.ev.ox_stare", Capture(k, () => k.Life.OnOddFurnished(stool)).line.Key);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddFurnished(stool)).count);    // günde bir
            Pet(k, "animal.cat", 72, "Alex");
            var chest = k.World.PlaceItem(k.Db.Items.Get("item.chest_wood"), new GridPos(8, 8));
            Assert.Equal("core.oddity.ev.box_cat", Capture(k, () => k.Life.OnOddFurnished(chest)).line.Key);
        }

        [Fact]
        public void Howl_OnlyOnFullMoonNights_WithADog()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            Assert.NotNull(k.Life.HowlBlocker(c));                 // köpek yok
            Pet(k, "animal.dog", 70, "Karabaş");
            k.Clock.Restore(5.9, 1);
            Assert.NotNull(k.Life.HowlBlocker(c));                 // dolunay değil
            k.Clock.Restore(OddityEvents.MoonCycle - 1 + 0.5, 1);
            Assert.NotNull(k.Life.HowlBlocker(c));                 // gündüz
            k.Clock.Restore(OddityEvents.MoonCycle - 1 + 0.9, 1);
            Assert.Null(k.Life.HowlBlocker(c));
            Assert.StartsWith("core.oddity.ev.howl.", Capture(k, () => Assert.True(k.Life.Howl(c))).line.Key);
            Assert.False(k.Life.Howl(c));                          // gecede bir
            Assert.True(k.Life.Oddities.Has(OddityIds.Howl));
        }

        [Fact]
        public void Ploughshare_IsARealTool_HoeIsFasterWhileOwned()
        {
            var k = Make();
            float before = k.Life.Tools.SpeedFor("hoe");
            k.House.AddItem(LifeSystems.PloughshareItem, 1);
            Assert.True(k.Life.OwnsPloughshare());
            Assert.Equal((double)(before * LifeSystems.PloughshareHoeBonus), (double)k.Life.Tools.SpeedFor("hoe"), 4);
            // Masaya konmuşken de sayılır; balta etkilenmez.
            float axe = k.Life.Tools.SpeedFor("axe");
            k.House.TryTakeItem(LifeSystems.PloughshareItem);
            Assert.Equal((double)before, (double)k.Life.Tools.SpeedFor("hoe"), 4);
            Assert.Equal((double)axe, (double)k.Life.Tools.SpeedFor("axe"), 4);
        }

        [Fact]
        public void OddBequest_ExistsOnlyAsAnEasterEgg()
        {
            var odd = Thatchwick.Core.Life.Requests.RequestCatalog.Bequests.Where(b => b.Role == Thatchwick.Core.Life.Requests.RequestCatalog.OddBequestRole).ToList();
            Assert.Single(odd);
            Assert.Equal(Thatchwick.Core.Life.Requests.RequestCatalog.OddBequestId, odd[0].Id);
            Assert.Equal(LifeSystems.SpinningTopItem, odd[0].Objective.ItemId);
            var k = Make();
            Assert.NotNull(k.Life.Requests.OddBequest);
            k.Life.OddEvents.Roll = () => 0.999;
            Assert.False(k.Life.Requests.OddBequest());
            k.Life.OddEvents.Force(OddityEvents.OddBequest);
            Assert.True(k.Life.Requests.OddBequest());
        }

        [Fact]
        public void VillageMemory_AndRegift_ThroughLifeSystems()
        {
            var k = Make();
            var grand = Person(k, 1, "Dede", generation: 1);
            var kid = Person(k, 2, "Torun", generation: 3);
            Assert.Equal(0, Capture(k, () => k.Life.OnOddFall(grand)).count);
            Assert.Equal("core.oddity.ev.village_memory", Capture(k, () => k.Life.OnOddFall(kid)).line.Key);
        }

        [Fact]
        public void Regift_ShopkeeperGetsTheirOwnWares()
        {
            var k = Make();
            k.Life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            var smith = k.Pop.Members.First(m => m.IsVillager && k.Life.Villagers.RoleOf(m) == "blacksmith");
            var shop = Thatchwick.Core.Economy.Shops.Get("smithy");
            var ware = k.Db.Items.All.First(d => shop.Goods(d));
            var other = k.Db.Items.Get("item.bread");
            Assert.False(shop.Goods(other));
            Assert.Equal(0, Capture(k, () => k.Life.OnOddGift(smith, other, GiftTaste.Like, "")).count);
            Assert.Equal("core.oddity.ev.regift", Capture(k, () => k.Life.OnOddGift(smith, ware, GiftTaste.Like, "")).line.Key);
            Assert.True(k.Life.Oddities.Has(OddityIds.Regift));
        }

        [Fact]
        public void Events_SaveRoundTrip()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            k.Life.OddEvents.Set("mine.tablet", 1);
            k.Life.OddEvents.OnFall("Dede", 1);
            k.Life.AnimalTalk.OnFeed("item.feed");
            var json = JsonSerializer.Serialize(k.Life.Export(), new JsonSerializerOptions { IncludeFields = true });
            var data = JsonSerializer.Deserialize<LifeSaveData>(json, new JsonSerializerOptions { IncludeFields = true });
            var k2 = Make();
            k2.Life.Restore(data);
            Assert.True(k2.Life.OddEvents.Has("mine.tablet"));
            Assert.Equal("Dede", k2.Life.OddEvents.OnFall("Torun", 2));
            Assert.Single(k2.Life.AnimalTalk.ExportHistory());
        }

        [Fact]
        public void EveryEventTextKey_ExistsInBothUiTables()
        {
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var errors = new System.Collections.Generic.List<string>();
            var en = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.en.json")), errors, "ui.en.json");
            var tr = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.tr.json")), errors, "ui.tr.json");
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            string[] keys =
            {
                "johan_wick.field", "johan_wick.nofield", "johan_wick.wolves", "horseless_knight", "crier.v0", "crier.v1", "crier.v2",
                "screaming_goat.scream", "screaming_goat.roof", "cat_gift", "alex_decor", "honey_thief", "servant_caught.hat", "servant_caught.nap",
                "april_first.crier", "april_first.cows", "rumor.fish", "rumor.wood", "rumor.farm", "rumor.mine", "name_twin", "name_surprise.doruk",
                "nickname_talk", "same_outfit", "absent_elder", "scarecrow.answered", "scarecrow.silent", "counting_sheep", "box_cat", "ox_stare",
                "chicken_revenge", "shoo", "dog_gift.relic", "dog_gift.bone", "jealous_dog", "old_boot", "message_bottle.v0", "message_bottle.v1",
                "message_bottle.v2", "fish_tale", "mine_tablet", "sword_in_stone", "never_stopping_top", "top_show.v0", "top_show.v1", "top_show.v2",
                "top_show.v3", "regift", "village_memory", "the_usual", "the_usual.heir", "howl.kids", "howl.alone"
            };
            foreach (var k in keys)
            {
                Assert.True(en.TryGet("core.oddity.ev." + k, out _), "EN eksik: " + k);
                Assert.True(tr.TryGet("core.oddity.ev." + k, out _), "TR eksik: " + k);
            }
            foreach (var extra in new[] { "core.oddity.relic.taken_garden", "ui.animal.shoo", "ui.animal.howl", "ui.oddity.debug.force" })
            {
                Assert.True(en.TryGet(extra, out _), "EN eksik: " + extra);
                Assert.True(tr.TryGet(extra, out _), "TR eksik: " + extra);
            }
        }

        // ---- sahneler: haritada görünen ziyaretçiler, pozlar, yere düşen süs (kullanıcı 20.11.2026) ----

        /// <summary>Öğle vakti: ziyaretçiler gece yarısı değil gündüz gelir.</summary>
        private static void Noon(Kit k, int day = 5) => k.Clock.Restore(day + 0.5, 1);

        private static void RunWalkers(Kit k, int steps = 4000)
        {
            k.Life.OddScenesTick(0.002);
            for (int i = 0; i < steps && k.Life.Walkers.Walkers.Count > 0; i++) k.Life.OddScenesTick(0.002);
            Assert.Empty(k.Life.Walkers.Walkers);
        }

        [Fact]
        public void JohanWick_ComesInTheMorning_NotAtMidnight()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            Pet(k, "animal.dog", 70, "Karabaş");
            k.Clock.Restore(5.0, 1);
            k.Life.OddEvents.Force(OddityEvents.JohanWick);
            k.Life.OddEventsNewDay(5);
            k.Life.OddScenesTick(0.01);
            Assert.Null(k.Life.Walkers.Find(OddWalkerKind.JohanWick));     // gece yarısı: daha gelmedi
            Assert.True(k.Life.OddEvents.Has(LifeSystems.JohanFlag));
            k.Clock.Restore(5.0 + LifeSystems.JohanArrives + 0.01, 1);
            k.Life.OddScenesTick(0.001);
            Assert.NotNull(k.Life.Walkers.Find(OddWalkerKind.JohanWick));
        }

        [Fact]
        public void OddWalkers_WalkThePath_ThenAct_ThenReportReady()
        {
            var walkers = new OddWalkers();
            int ready = 0;
            walkers.Ready += w => ready++;
            var w1 = walkers.Spawn(OddWalkerKind.JohanWick, new GridPos(0, 0));
            walkers.Walk(w1, new List<GridPos> { new GridPos(1, 0), new GridPos(2, 0) });
            walkers.Tick(0.5 / OddWalkers.StepsPerDay);
            Assert.Equal(new GridPos(0, 0), w1.Position);
            Assert.Equal(new GridPos(1, 0), w1.NextTile);
            Assert.Equal(0, ready);
            walkers.Tick(2.0 / OddWalkers.StepsPerDay);
            Assert.Equal(new GridPos(2, 0), w1.Position);
            Assert.Null(w1.NextTile);
            Assert.Equal(1, ready);
            walkers.Act(w1, "pet", 0.01, new GridPos(3, 0));
            walkers.Tick(0.004);
            Assert.Equal("pet", w1.Action);
            walkers.Tick(0.01);
            Assert.Null(w1.Action);
            Assert.Equal(2, ready);
            w1.Gone = true;
            walkers.Tick(0.001);
            Assert.Empty(walkers.Walkers);
        }

        [Fact]
        public void JohanWick_WalksIn_PetsTheDog_WrecksCrops_AndWalksOut()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            var dog = Pet(k, "animal.dog", 70, "Karabaş");
            var seed = k.Db.Items.All.First(i => i.IsCrop);
            for (int i = 0; i < 6; i++) k.World.PlaceItem(seed, new GridPos(8 + i % 3, 8 + i / 3));
            float before = dog.Friendship;
            Noon(k);
            k.Life.OddEvents.Force(OddityEvents.JohanWick);
            var first = Capture(k, () => { k.Life.OddEventsNewDay(5); k.Life.OddScenesTick(0.0001); });
            // Hemen bir şey olmaz: haritanın kenarından girer, köpeğe yürür.
            Assert.Equal(0, first.count);
            var w = k.Life.Walkers.Find(OddWalkerKind.JohanWick);
            Assert.NotNull(w);
            Assert.True(w.Position.X == 0 || w.Position.Y == 0 || w.Position.X == 15 || w.Position.Y == 15);
            Assert.Equal(before, dog.Friendship);
            Assert.Equal(6, k.World.Items.Count(it => it.IsPlant && it.Def.IsCrop));
            bool petted = false, wrecked = false;
            LocText line = default;
            Action<LocText, int> h = (l, id) => line = l;
            k.Life.OddityLine += h;
            for (int i = 0; i < 4000 && k.Life.Walkers.Walkers.Count > 0; i++)
            {
                k.Life.OddScenesTick(0.002);
                petted |= w.Action == "pet";
                wrecked |= w.Action == "wreck";
            }
            k.Life.OddityLine -= h;
            Assert.True(petted && wrecked);
            Assert.Empty(k.Life.Walkers.Walkers);
            Assert.Equal("core.oddity.ev.johan_wick.field", line.Key);
            Assert.Equal(before + 5f, dog.Friendship);
            int left = k.World.Items.Count(it => it.IsPlant && it.Def.IsCrop);
            Assert.InRange(6 - left, LifeSystems.JohanCropsMin, LifeSystems.JohanCropsMax);
            Assert.Equal(1, k.House.Count(LifeSystems.RelicCoinItem));
            Assert.False(k.Life.OddEvents.Has(LifeSystems.JohanFlag));
        }

        [Fact]
        public void JohanWick_SavedMidVisit_StillLeavesTheCoinOnLoad()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            Pet(k, "animal.dog", 70, "Karabaş");
            Noon(k);
            k.Life.OddEvents.Force(OddityEvents.JohanWick);
            k.Life.OddEventsNewDay(5);
            k.Life.OddScenesTick(0.0001);
            Assert.NotNull(k.Life.Walkers.Find(OddWalkerKind.JohanWick));
            var saved = k.Life.ExportOddities();
            var k2 = Make();
            Person(k2, 1, "Ayla");
            Pet(k2, "animal.dog", 70, "Karabaş");
            var (line, n) = Capture(k2, () => k2.Life.ImportOddities(saved));
            Assert.Equal("core.oddity.ev.johan_wick.nofield", line.Key);
            Assert.Equal(1, k2.House.Count(LifeSystems.RelicCoinItem));
            Assert.Empty(k2.Life.Walkers.Walkers);
            // İkinci yüklemede yeniden vermez.
            Assert.Equal(0, Capture(k2, () => k2.Life.ImportOddities(k2.Life.ExportOddities())).count);
            Assert.Equal(1, k2.House.Count(LifeSystems.RelicCoinItem));
        }

        [Fact]
        public void SceneBook_PoseOnlyWhileStandingThere_AndExpires()
        {
            double now = 10;
            var book = new SceneBook { Now = () => now };
            book.SetPose(7, "stare", new GridPos(3, 3), 0.1, itemId: 44);
            Assert.Null(book.PoseOf(7, new GridPos(2, 3), false));     // henüz varmadı
            Assert.Null(book.PoseOf(7, new GridPos(3, 3), true));      // yürüyor
            Assert.Equal("stare", book.PoseOf(7, new GridPos(3, 3), false).Pose);
            Assert.Equal(44, book.PoseOf(7, new GridPos(3, 3), false).ItemId);
            now = 10.2;
            Assert.Null(book.PoseOf(7, new GridPos(3, 3), false));
            book.SetWildFx(5, "crown", 0.05); book.SetCharacterScene(1, "top", 0.02);
            Assert.Equal("crown", book.WildFxOf(5));
            Assert.Equal("top", book.CharacterSceneOf(1));
            now = 10.3;
            Assert.Null(book.WildFxOf(5));
            Assert.Null(book.CharacterSceneOf(1));
        }

        [Fact]
        public void NewBuild_OxWalksOverAndStares_CatGoesToTheChest()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.0;
            Person(k, 1, "Ayla");
            var ox = Pet(k, "animal.ox", 73, "Sarı");
            var stool = k.World.PlaceItem(k.Db.Items.Get("item.stool_wood"), new GridPos(9, 9));
            k.Life.OnOddFurnished(stool);
            Assert.Null(k.Life.PoseOf(ox));                             // daha yolda
            for (int i = 0; i < 4000 && k.Life.PoseOf(ox) == null; i++) k.Animals.Tick(0.0005);
            var pose = k.Life.PoseOf(ox);
            Assert.NotNull(pose);
            Assert.Equal("stare", pose.Pose);
            Assert.Equal(stool.Id, pose.ItemId);
            Assert.Equal(1, Math.Abs(ox.Position.X - 9) + Math.Abs(ox.Position.Y - 9));   // yanında durur
            // Bakıp durur: bekleme süresi dolmadan yerinden kıpırdamaz.
            var at = ox.Position;
            k.Animals.Tick(SceneBook.StareDays * 0.5);
            Assert.Equal(at, ox.Position);

            // Genç kedi (kedinin ömrü kısa; 40 günlük kedi yürürken yaşlılıktan ölür).
            var cat = k.Animals.Restore(72, k.Db.Animals.Get("animal.cat"), new GridPos(4, 4), 0, 100f, 6, 0, 0, 30f, comfort: 80f);
            cat.Name = "Alex";
            var chest = k.World.PlaceItem(k.Db.Items.Get("item.chest_wood"), new GridPos(12, 5));
            k.Life.OnOddFurnished(chest);
            for (int i = 0; i < 4000 && k.Life.PoseOf(cat) == null; i++) k.Animals.Tick(0.0005);
            Assert.Equal("box", k.Life.PoseOf(cat)?.Pose);
            Assert.Equal(chest.Id, k.Life.PoseOf(cat).ItemId);
        }

        [Fact]
        public void Howl_DogStopsAndPoses_HowlerPlaysTheScene()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            var kid = Person(k, 2, "Can", ChildAge);
            var dog = Pet(k, "animal.dog", 70, "Karabaş");
            k.Clock.Restore(OddityEvents.MoonCycle - 1 + 0.9, 1);
            Assert.True(k.Life.Howl(c));
            Assert.Equal("howl", k.Life.PoseOf(dog)?.Pose);
            Assert.Equal("howl", k.Life.Scenes.CharacterSceneOf(c.Id));
            Assert.Equal("howl", k.Life.Scenes.CharacterSceneOf(kid.Id));
        }

        [Fact]
        public void TopShow_PlaysTheTopScene()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            var top = new Thatchwick.Core.Upbringing.ToyDef { ItemId = LifeSystems.SpinningTopItem };
            k.Life.OnOddShow(new Thatchwick.Core.Upbringing.ToyShow { Performer = c, Toy = top, Kind = Thatchwick.Core.Upbringing.ShowKind.Show, Watchers = 0 });
            Assert.Equal("top", k.Life.Scenes.CharacterSceneOf(c.Id));
        }

        [Fact]
        public void FrogKiss_CrownShowsOnThatFrog()
        {
            var k = Make();
            k.Life.AttachWild(k.World.Map);
            var c = Person(k, 1, "Ayla");
            var frog = k.Life.Roamers.Arrive(Thatchwick.Core.Animals.WildCatalog.Get(FrogKiss.FrogSpecies), k.Clock.TotalDays);
            Assert.NotNull(frog);
            Assert.True(k.Life.KissFrog(c, force: true, visitorId: frog.Id));
            Assert.Equal("crown", k.Life.Scenes.WildFxOf(frog.Id));
            Assert.True(frog.LeaveAt >= k.Clock.TotalDays + SceneBook.CrownDays);
            // Sıradan öpücükte taç yok.
            var frog2 = k.Life.Roamers.Arrive(Thatchwick.Core.Animals.WildCatalog.Get(FrogKiss.FrogSpecies), k.Clock.TotalDays);
            k.Life.KissFrog(c, force: false, visitorId: frog2.Id);
            Assert.Null(k.Life.Scenes.WildFxOf(frog2.Id));
        }

        private static ItemInstance DecorOnTable(Kit k, out ItemInstance table)
        {
            table = k.World.PlaceItem(k.Db.Items.Get("item.table_wood"), new GridPos(6, 6));
            int tableId = table.Id;
            var def = k.Db.Items.All.First(i => i.Placement == ItemPlacement.Surface && k.World.CanPlaceOnSurface(i, tableId, "a").Ok);
            return k.World.PlaceOnSurface(def, tableId, "a");
        }

        [Fact]
        public void CatAlex_KnocksDecorToTheFloor_NothingIsLost_CharacterPutsItBack()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            Pet(k, "animal.cat", 72, "Alex");
            var decor = DecorOnTable(k, out var table);
            var farm = new FarmWork(k.World) { House = k.House, Content = k.Db };
            k.Life.Attach(farm);
            int items = k.World.Items.Count;
            k.Life.OddEvents.Roll = () => 0.999;
            k.Life.OddEvents.Force(OddityEvents.AlexDecor);
            var (line, n) = Capture(k, () => k.Life.OddEventsNewDay(5));
            Assert.Equal("core.oddity.ev.alex_decor", line.Key);
            var fallen = k.Life.Scenes.KnockedOf(decor.Id);
            Assert.NotNull(fallen);
            // Eşya dünyada, masadaki yuvasında kayıtlı durur: hiçbir şey kaybolmaz.
            Assert.Equal(items, k.World.Items.Count);
            Assert.Equal(table.Id, k.World.GetItem(decor.Id).ParentId);
            Assert.True(k.World.Map.IsWalkable(fallen.Floor));
            Assert.Same(fallen, k.Life.Scenes.KnockedAt(fallen.Floor));
            // Aynı süs iki kez düşmez.
            k.Life.OddEvents.Force(OddityEvents.AlexDecor);
            Assert.Equal(0, Capture(k, () => k.Life.OddEventsNewDay(6)).count);

            // Kayıt: yerde kalır.
            var saved = k.Life.ExportOddities();
            Assert.Single(saved.knocked);
            k.Life.ImportOddities(saved);
            Assert.NotNull(k.Life.Scenes.KnockedOf(decor.Id));

            // "Yerine koy": yalnızca düşen eşyanın karosunda.
            Assert.NotNull(farm.CanDo(c, new GridPos(1, 1), TileAction.PutBack));
            Assert.Null(farm.CanDo(c, fallen.Floor, TileAction.PutBack));
            Assert.True(farm.Duration(c, TileAction.PutBack) > 0);
            Assert.Equal("pickup", ActivityDescriber.DescribeJob(Job.OnTile(fallen.Floor, TileAction.PutBack), k.World).Icon);
            var back = Capture(k, () => farm.Apply(c, fallen.Floor, TileAction.PutBack));
            Assert.Equal("core.oddity.ev.alex_decor.put_back", back.line.Key);
            Assert.Null(k.Life.Scenes.KnockedOf(decor.Id));
            Assert.Equal(items, k.World.Items.Count);
            Assert.NotNull(farm.CanDo(c, fallen.Floor, TileAction.PutBack));
        }

        [Fact]
        public void KnockedDecor_RecordIsDropped_WhenTheItemLeavesTheWorld()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            var decor = DecorOnTable(k, out _);
            Assert.NotNull(k.Life.KnockDecor());
            k.World.RemoveItem(decor.Id);
            k.Life.OdditiesNewDay(7);
            Assert.Empty(k.Life.Scenes.Knocked);
            Assert.Null(k.Life.KnockDecor());                      // düşürecek süs kalmadı
        }

        // ---- +18 çeşitleri ve ad sürprizleri ----

        private static (LocTable en, LocTable tr) UiTables()
        {
            string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var errors = new List<string>();
            var en = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.en.json")), errors, "ui.en.json");
            var tr = LocTable.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "ui.tr.json")), errors, "ui.tr.json");
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return (en, tr);
        }

        private static List<string> Placeholders(string text)
        {
            var list = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, @"\{(\w+)\}")) list.Add(m.Groups[1].Value);
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        [Fact]
        public void AdultPack_SwapsInTheSwearingVariant_OnlyWhenThePackIsOn()
        {
            var (en, tr) = UiTables();
            var plain = LocText.Of("core.oddity.ev.top_show.v0", ("who", "Ayla"));
            Assert.Equal(plain.Key, OddityLines.ForPack(plain, false, en.Contains).Key);
            var adult = OddityLines.ForPack(plain, true, en.Contains);
            Assert.Equal("core.oddity.ev.top_show.v0.adult", adult.Key);
            Assert.Equal(plain.Args, adult.Args);
            // Çeşidi olmayan satır paket açıkken de aynen kalır.
            var other = LocText.Of("core.oddity.ev.cat_gift", ("animal", "Alex"));
            Assert.Equal(other.Key, OddityLines.ForPack(other, true, en.Contains).Key);

            foreach (var key in OddityLines.AdultKeys)
            {
                foreach (var (table, name) in new[] { (en, "EN"), (tr, "TR") })
                {
                    Assert.True(table.TryGet(key, out var baseText), name + " eksik: " + key);
                    Assert.True(table.TryGet(key + OddityLines.AdultSuffix, out var adultText), name + " +18 eksik: " + key);
                    // Çeşit aynı argümanlarla doldurulur: fazladan yer tutucu olamaz.
                    var have = Placeholders(baseText);
                    foreach (var ph in Placeholders(adultText)) Assert.True(have.Contains(ph), $"{name} {key}: bilinmeyen {{{ph}}}");
                }
            }
        }

        [Theory]
        [InlineData("Doruk", "doruk")]
        [InlineData("sanem", "sanem")]
        [InlineData("BORA", "bora")]
        [InlineData("Alpaslan", "alpaslan")]
        [InlineData("Alex", "alex")]
        [InlineData("Lonta", "lonta")]
        [InlineData("Lontaj", "lontaj")]
        [InlineData("KaosLontaj", "kaoslontaj")]
        [InlineData("  Sanem   Bal ", "sanem_bal")]
        [InlineData("Mars", "mars")]
        public void NameSurprise_ForTheChosenNames(string name, string key)
        {
            Assert.Equal(key, LifeSystems.NameSurpriseKey(name));
            var (en, tr) = UiTables();
            Assert.True(en.Contains("core.oddity.ev.name_surprise." + key));
            Assert.True(tr.Contains("core.oddity.ev.name_surprise." + key));
        }

        [Fact]
        public void NameSurprise_OtherNamesGetNothing()
        {
            Assert.Null(LifeSystems.NameSurpriseKey("Murat"));
            Assert.Null(LifeSystems.NameSurpriseKey("Bal"));
            Assert.Null(LifeSystems.NameSurpriseKey(""));
            Assert.Null(LifeSystems.NameSurpriseKey(null));
        }

        // ---- tepkiler balon olarak; boşta toplama; ziyaretçiye dokunma (kullanıcı 20.11.2026) ----

        private sealed class Bubbles
        {
            public readonly List<(string key, int speaker)> Said = new List<(string, int)>();
            public readonly List<(string key, int animal)> Animals = new List<(string, int)>();
            public readonly List<string> Lines = new List<string>();
        }

        private static Bubbles Listen(Kit k)
        {
            var b = new Bubbles();
            k.Life.OddityBubble += (line, speaker, who) => b.Said.Add((line.Key, speaker));
            k.Life.OddityAnimalBubble += (line, animal, who) => b.Animals.Add((line.Key, animal));
            k.Life.OddityLine += (line, who) => b.Lines.Add(line.Key);
            return b;
        }

        [Fact]
        public void Quote_TakesTheFirstQuotedSpeech()
        {
            Assert.Equal("Efsane be!", OddityLines.Quote("Ayla topaç şovu yaptı. Seyredenler: \"Efsane be!\""));
            Assert.Equal("Bana da konuşur mu?", OddityLines.Quote("Can: \"Bana da konuşur mu?\" (Gidip denedi.)"));
            Assert.Equal("Fancy.", OddityLines.Quote("Mira: \u201CFancy.\u201D"));
            Assert.Null(OddityLines.Quote("Ayla korkuluğa derdini anlattı."));
            Assert.Null(OddityLines.Quote(""));
            Assert.Null(OddityLines.Quote(null));
            // Tabloda ".say" satırı varsa o kullanılır.
            var line = LocText.Of("x.y", ("who", "Ayla"));
            Assert.Equal("SAY x.y.say", OddityLines.BubbleText(line, "anlatım, söz yok", key => key == "x.y.say", t => "SAY " + t.Key));
            Assert.Null(OddityLines.BubbleText(line, "anlatım, söz yok", key => false, t => "SAY " + t.Key));
        }

        [Fact]
        public void EveryReactionLine_HasSomethingToSayInABubble()
        {
            var (en, tr) = UiTables();
            var keys = new List<string>
            {
                "core.oddity.ev.top_show.v0", "core.oddity.ev.top_show.v1", "core.oddity.ev.top_show.v2", "core.oddity.ev.top_show.v3",
                "core.oddity.tell.shared", "core.oddity.tell.family", "core.oddity.tell.child", "core.oddity.tell.disbelief",
                "core.oddity.ev.name_twin", "core.oddity.ev.nickname_talk", "core.oddity.ev.same_outfit", "core.oddity.ev.absent_elder",
                "core.oddity.ev.chicken_revenge", "core.oddity.ev.regift", "core.oddity.hello", "core.oddity.relic.sword.parent_reply"
            };
            foreach (var n in new[] { "doruk", "sanem", "bora", "alpaslan", "alex", "lonta", "lontaj", "kaoslontaj", "sanem_bal", "mars" })
                keys.Add("core.oddity.ev.name_surprise." + n);
            foreach (var what in new[] { "sword", "helmet", "shield" })
                foreach (var age in new[] { "adult", "child", "elder" }) keys.Add($"core.oddity.relic.{what}.{age}");
            foreach (AnimalComplaint c in Enum.GetValues(typeof(AnimalComplaint))) keys.Add("core.oddity.talk.complaint." + AnimalTalkService.ComplaintKey(c));
            foreach (var (table, name) in new[] { (en, "EN"), (tr, "TR") })
                foreach (var key in keys)
                    foreach (var k in new[] { key, key + OddityLines.AdultSuffix })
                    {
                        if (!table.TryGet(k, out var text)) { Assert.False(k == key, name + " eksik: " + key); continue; }
                        Assert.True(table.Contains(k + OddityLines.SaySuffix) || OddityLines.Quote(text) != null, $"{name} {k}: balonda söylenecek söz yok");
                    }
        }

        [Fact]
        public void TopShow_NearbyWatchersReactInBubbles_NotInTheFeed()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var c = Person(k, 1, "Ayla");
            var near1 = Person(k, 2, "Murat", villager: true);
            var near2 = Person(k, 3, "Can", ChildAge);
            var far = Person(k, 4, "Uzak", villager: true);
            far.Position = new GridPos(14, 14);
            Person(k, 5, "Bebek", BabyAge);
            var b = Listen(k);
            var top = new Thatchwick.Core.Upbringing.ToyDef { ItemId = LifeSystems.SpinningTopItem };
            k.Life.OddEvents.Force(OddityEvents.TopShow);
            k.Life.OnOddShow(new Thatchwick.Core.Upbringing.ToyShow { Performer = c, Toy = top, Kind = Thatchwick.Core.Upbringing.ShowKind.Show, Watchers = 2 });
            Assert.Empty(b.Lines);
            Assert.Equal(2, b.Said.Count);
            Assert.Equal(new[] { near1.Id, near2.Id }, b.Said.Select(x => x.speaker).OrderBy(x => x).ToArray());
            Assert.All(b.Said, x => Assert.StartsWith("core.oddity.ev.top_show.v", x.key));
            Assert.NotEqual(b.Said[0].key, b.Said[1].key);                  // iki seyirci aynı sözü söylemez
        }

        [Fact]
        public void Tell_ListenerAnswersInABubble_NicknameStaysAsNarration()
        {
            var (k, teller) = Spoken();
            var b = Listen(k);
            for (int i = 0; i < AnimalTalkService.DisbelieversForNickname; i++)
            {
                var v = Person(k, 10 + i, "Köylü" + i, villager: true);
                b.Said.Clear(); b.Lines.Clear();
                k.Family.Apply(teller, v, SocialAction.TellAnimalTalk);
                Assert.Equal(("core.oddity.tell.disbelief", v.Id), b.Said.Single());
                if (i < AnimalTalkService.DisbelieversForNickname - 1) Assert.Empty(b.Lines);
                else Assert.Equal(LifeSystems.NicknameLineKey, b.Lines.Single());   // lakap takıldı: anlatım bildirimde kalır
            }
        }

        [Fact]
        public void Chat_VillagerSpeaksTheReaction()
        {
            var k = Make();
            k.Life.OddEvents.Roll = () => 0.999;
            var a = Person(k, 1, "Mars");
            var v = Person(k, 2, "Murat", villager: true);
            var b = Listen(k);
            k.Life.OnOddChat(a, v);
            Assert.Equal(("core.oddity.ev.name_surprise.mars", v.Id), b.Said.Single());
            Assert.Empty(b.Lines);
        }

        [Fact]
        public void AnimalComplaint_ShowsOverTheAnimal_AndStaysInTheFeed()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla", charm: 9);
            var cow = Cow(k);
            var b = Listen(k);
            k.Life.AnimalTalk.ForceNext = true;
            k.Life.AnimalTalk.Try(c, cow);
            Assert.Equal(cow.Id, b.Animals.Single().animal);
            Assert.StartsWith("core.oddity.talk.complaint.", b.Animals.Single().key);
            Assert.Contains(b.Animals.Single().key, b.Lines);
        }

        [Fact]
        public void IdleCharacterNearby_PutsTheFallenDecorBack_OnItsOwn()
        {
            var k = Make();
            var near = Person(k, 1, "Ayla");
            near.Position = new GridPos(4, 6);
            var kid = Person(k, 2, "Can", ChildAge);
            kid.Position = new GridPos(5, 5);
            var villager = Person(k, 3, "Murat", villager: true);
            villager.Position = new GridPos(5, 6);
            var decor = DecorOnTable(k, out _);
            Assert.Null(k.Life.TidyJobFor(near));                           // yerde bir şey yok
            var fallen = k.Life.KnockDecor();
            Assert.Null(k.Life.TidyJobFor(kid));                            // çocuk toplamaz
            Assert.Null(k.Life.TidyJobFor(villager));                       // misafir köylü toplamaz
            near.Position = new GridPos(fallen.Floor.X, Math.Min(15, fallen.Floor.Y + LifeSystems.TidyRadius + 1));
            Assert.Null(k.Life.TidyJobFor(near));                           // uzakta: fark etmez
            near.Position = new GridPos(4, 6);
            var job = k.Life.TidyJobFor(near);
            Assert.NotNull(job);
            Assert.Equal(JobKind.Tile, job.Kind);
            Assert.Equal(TileAction.PutBack, job.TileAction);
            Assert.Equal(fallen.Floor, job.Target);
            // Aynı süsü ikinci bir kişi üstlenmez; süre dolunca yeniden denenir.
            var other = Person(k, 4, "Elif");
            other.Position = new GridPos(5, 6);
            Assert.Null(k.Life.TidyJobFor(other));
            k.Clock.Restore(k.Clock.TotalDays + LifeSystems.TidyClaimDays + 0.01, 1);
            Assert.NotNull(k.Life.TidyJobFor(other));
            Assert.NotNull(k.Life.Scenes.KnockedOf(decor.Id));
        }

        [Fact]
        public void IdleRunner_TakesTheChore_WalksOverAndPutsItBack()
        {
            var k = Make();
            var c = Person(k, 1, "Ayla");
            c.Position = new GridPos(3, 6);
            var decor = DecorOnTable(k, out _);
            var farm = new FarmWork(k.World) { House = k.House, Content = k.Db };
            k.Life.Attach(farm);
            var runner = new JobRunner(c, k.World) { Autonomous = true, TileWork = farm };
            k.Life.Attach(runner);
            Assert.NotNull(runner.IdleChore);
            k.Life.KnockDecor();
            var seen = Capture(k, () => { for (int i = 0; i < 400 && k.Life.Scenes.HasKnocked; i++) runner.Tick(0.001); });
            Assert.False(k.Life.Scenes.HasKnocked);
            Assert.Equal("core.oddity.ev.alex_decor.put_back", seen.line.Key);
            Assert.NotNull(k.World.GetItem(decor.Id));

            // Oyuncunun yönettiği (otonomisi kapalı) karakter kendiliğinden toplamaz; tıklayınca yapar.
            k.Life.KnockDecor();
            var manual = new JobRunner(c, k.World) { Autonomous = false, TileWork = farm };
            k.Life.Attach(manual);
            for (int i = 0; i < 50; i++) manual.Tick(0.001);
            Assert.True(k.Life.Scenes.HasKnocked);
        }

        [Fact]
        public void FallenDecor_FollowsTheTable_WhenItIsMoved()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            var decor = DecorOnTable(k, out var table);
            var fallen = k.Life.KnockDecor();
            var old = fallen.Floor;
            Assert.True(k.World.MoveItem(table.Id, new GridPos(11, 11), 0).Ok);
            k.Life.OddScenesTick(0.001);
            var now = k.Life.Scenes.KnockedOf(decor.Id).Floor;
            Assert.NotEqual(old, now);
            Assert.True(Math.Abs(now.X - 11) <= 3 && Math.Abs(now.Y - 11) <= 3);
            Assert.True(k.World.Map.IsWalkable(now));
        }

        [Fact]
        public void Walkers_CanBeFoundByTile()
        {
            var walkers = new OddWalkers();
            var w = walkers.Spawn(OddWalkerKind.HorselessKnight, new GridPos(2, 2));
            walkers.Walk(w, new List<GridPos> { new GridPos(3, 2), new GridPos(4, 2) });
            walkers.Tick(0.2 / OddWalkers.StepsPerDay);
            Assert.Same(w, walkers.At(new GridPos(2, 2)));
            Assert.Same(w, walkers.At(new GridPos(3, 2)));                  // adım attığı karo da sayılır
            Assert.Null(walkers.At(new GridPos(9, 9)));
            var (en, tr) = UiTables();
            foreach (var key in new[] { "ui.oddity.walker.johan_wick", "ui.oddity.walker.johan_wick.note", "ui.oddity.walker.horseless_knight", "ui.oddity.walker.horseless_knight.note" })
                Assert.True(en.Contains(key) && tr.Contains(key), key);
        }

        [Fact]
        public void Knight_QueuedForToday_SurvivesSaveAndLoad()
        {
            var k = Make();
            Person(k, 1, "Ayla");
            k.Clock.Restore(5.1, 1);
            k.Life.OddEvents.Set(LifeSystems.KnightFlag, k.Clock.DayIndex + 1);
            var saved = k.Life.ExportOddities();
            var k2 = Make();
            Person(k2, 1, "Ayla");
            k2.Clock.Restore(5.2, 1);
            k2.Life.ImportOddities(saved);
            Assert.Equal(0, Capture(k2, () => k2.Life.OddScenesTick(0.001)).count);     // daha saati gelmedi
            k2.Clock.Restore(5.0 + LifeSystems.KnightArrives + 0.01, 1);
            Assert.Equal("core.oddity.ev.horseless_knight", Capture(k2, () => k2.Life.OddScenesTick(0.001)).line.Key);
            Assert.False(k2.Life.OddEvents.Has(LifeSystems.KnightFlag));
            // Başka güne ait bayrak (kayıt ertesi gün yüklendi) temizlenir, şövalye gelmez.
            var k3 = Make();
            k3.Clock.Restore(7.9, 1);
            k3.Life.ImportOddities(saved);
            Assert.False(k3.Life.OddEvents.Has(LifeSystems.KnightFlag));
            Assert.Equal(0, Capture(k3, () => k3.Life.OddScenesTick(0.001)).count);
        }
    }
}
