using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    public enum JobKind
    {
        MoveTo = 0,
        UseItem = 1
    }

    /// <summary>Karakterin yapacağı bir iş. Oyuncu komutu ve NPC otonomisi aynı işleri üretir.</summary>
    public sealed class Job
    {
        public JobKind Kind { get; private set; }
        public GridPos Target { get; private set; }
        public int ItemId { get; private set; }
        public string InteractionId { get; private set; }

        public static Job MoveTo(GridPos target) =>
            new Job { Kind = JobKind.MoveTo, Target = target };

        public static Job UseItem(int itemId, string interactionId) =>
            new Job { Kind = JobKind.UseItem, ItemId = itemId, InteractionId = interactionId };
    }
}
