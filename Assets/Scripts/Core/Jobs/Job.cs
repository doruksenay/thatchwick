using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    public enum JobKind
    {
        MoveTo = 0,
        UseItem = 1,
        /// <summary>Başka bir karakterle yapılan iş: bebeği beslemek, sohbet, evlenme teklifi, çocuk istemek.</summary>
        Social = 2
    }

    public enum SocialAction
    {
        Feed = 0,
        Chat = 1,
        Propose = 2,
        TryForBaby = 3
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

        public Job WithReason(NeedType need)
        {
            Reason = need;
            return this;
        }

        public static Job MoveTo(GridPos target) =>
            new Job { Kind = JobKind.MoveTo, Target = target };

        public static Job UseItem(int itemId, string interactionId) =>
            new Job { Kind = JobKind.UseItem, ItemId = itemId, InteractionId = interactionId };

        public static Job Social(int targetCharacterId, SocialAction action) =>
            new Job { Kind = JobKind.Social, TargetCharacterId = targetCharacterId, Action = action };
    }
}
