using System.Collections.Generic;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>Yerleşmiş eşyanın mantıksal yönü ve etkileşim hücreleri (Aşama B'de eşya metadata'sından okunur).</summary>
    public static class ItemFacing
    {
        /// <summary>Ön yüzü olan eşyanın baktığı yön; ön yüzü yoksa null.</summary>
        public static Dir4? Of(ItemInstance item) => null;

        /// <summary>Eşyanın kullanıldığı (önünde durulan) karolar.</summary>
        public static IEnumerable<GridPos> InteractionCells(ItemInstance item) { yield break; }
    }
}
