using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>İş etkileşimlerinin dünya ve hane üzerindeki etkileri (Core.Work.InteractionEffects uygular).</summary>
    public interface IInteractionEffects
    {
        /// <summary>Başlanabiliyorsa null, yoksa okunur neden (ör. "Kütük yok.").</summary>
        string CanStart(Character character, ItemInstance item, InteractionDef interaction);

        /// <summary>İş bitince çağrılır: kaynak harcar, ürün verir, gerekirse eşyayı kaldırır.</summary>
        void Apply(Character character, ItemInstance item, InteractionDef interaction);
    }
}
