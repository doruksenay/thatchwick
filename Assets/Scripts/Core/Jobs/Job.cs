using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    public enum JobKind
    {
        MoveTo = 0,
        UseItem = 1,
        /// <summary>Başka bir karakterle yapılan iş: bebeği beslemek, sohbet, evlenme teklifi, çocuk istemek.</summary>
        Social = 2,
        /// <summary>Bir hayvanla yapılan iş: sev, tımar, eyer, bin.</summary>
        Animal = 3,
        /// <summary>Bir karoda yapılan iş: çapala.</summary>
        Tile = 4,
        /// <summary>Bunalım: karakter (varsa) bir yere yürür ve bunalım bitene kadar orada kalır. Sistem verir, oyuncu veremez.</summary>
        Breakdown = 5
    }

    public enum SocialAction
    {
        Feed = 0,
        Chat = 1,
        Propose = 2,
        TryForBaby = 3,
        /// <summary>Çocuğa masal anlat (kitap gerekmez; ortaçağda masal sözlüydü).</summary>
        TellStory = 4,
        // Beceri seviyesiyle açılan sosyal işler (Project belgesi 45, 9. bölüm; FamilyService.SkillSocials).
        Compliment = 10, Joke = 11, Listen = 12, Mediate = 13, Matchmake = 14, Persuade = 15, FestivalSpeech = 16,
        LearnSong = 20, PlayTavern = 21, OpenFestival = 22, Compose = 23, LeadDance = 24,
        Lullaby = 30, PlayWithBaby = 31, FirstSteps = 32, ShowWork = 33, Soothe = 34, TalkTeen = 35, SingLullaby = 36,
        TeachLetters = 40, VillageLesson = 41, Assess = 42, ElderLesson = 43,
        Diagnose = 50, Bandage = 51, Midwife = 52, SetBone = 53, VisitPatient = 54, ElderCare = 55,
        TellWeather = 60, WriteLetter = 61
    }

    /// <summary>Karakterin yapacağı bir iş. Oyuncu komutu ve NPC otonomisi aynı işleri üretir.</summary>
    public sealed class Job
    {
        public JobKind Kind { get; private set; }
        public GridPos Target { get; private set; }
        public int ItemId { get; private set; }
        public string InteractionId { get; private set; }
        public int TargetCharacterId { get; private set; }
        public SocialAction Action { get; private set; }
        /// <summary>Otonom seçilen işin nedeni (hangi ihtiyaç); oyuncu komutunda null. Eylem balonunda gösterilir.</summary>
        public NeedType? Reason { get; private set; }
        public int AnimalId { get; private set; }
        public AnimalAction AnimalAction { get; private set; }
        public TileAction TileAction { get; private set; }
        /// <summary>Görev (tarla, hayvanlar) için otonom seçilen iş: eylem balonunda "görev" yazar.</summary>
        public bool IsDuty { get; private set; }

        public Job AsDuty()
        {
            IsDuty = true;
            return this;
        }

        public Job WithReason(NeedType need)
        {
            Reason = need;
            return this;
        }

        /// <summary>Eylem balonunda nedenin yerine yazılan kısa not ("keyfi yerinde, pişirmeye hevesli"); yoksa null.</summary>
        public string Note { get; private set; }

        public Job WithNote(string note)
        {
            Note = note;
            return this;
        }

        /// <summary>Bunalım işi: hedef karo (meydan) ya da eşya (yatak) verilirse oraya yürür; ikisi de yoksa yerinde kalır.</summary>
        public static Job BreakdownAt(GridPos? target, int itemId = 0)
        {
            var j = new Job { Kind = JobKind.Breakdown, ItemId = itemId };
            if (target.HasValue) { j.Target = target.Value; j.HasTarget = true; }
            return j;
        }

        /// <summary>Bunalım işinde hedef karo verildi mi.</summary>
        public bool HasTarget { get; private set; }

        public static Job MoveTo(GridPos target) =>
            new Job { Kind = JobKind.MoveTo, Target = target };

        public static Job UseItem(int itemId, string interactionId) =>
            new Job { Kind = JobKind.UseItem, ItemId = itemId, InteractionId = interactionId };

        public static Job Social(int targetCharacterId, SocialAction action) =>
            new Job { Kind = JobKind.Social, TargetCharacterId = targetCharacterId, Action = action };

        public static Job WithAnimal(int animalId, AnimalAction action) =>
            new Job { Kind = JobKind.Animal, AnimalId = animalId, AnimalAction = action };

        public static Job OnTile(GridPos tile, TileAction action) =>
            new Job { Kind = JobKind.Tile, Target = tile, TileAction = action };

        /// <summary>Ek bilgili karo işi (gölete atılacak balık türü; InteractionId'de taşınır).</summary>
        public static Job OnTile(GridPos tile, TileAction action, string arg) =>
            new Job { Kind = JobKind.Tile, Target = tile, TileAction = action, InteractionId = arg };
    }
}
