using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Haritaya yerleştirilmiş bir eşya (ItemDef'in örneği). Yer eşyası bir kapladığı alan
    /// (Position = sol-üst/en küçük köşe, Rotation = 90° adımlarla saat yönü) ile tanımlanır.
    /// Dekor eşyası bir başka eşyanın yuvasında durur (ParentId + SlotId) ve karo kaplamaz.
    /// </summary>
    public sealed class ItemInstance
    {
        public int Id { get; }
        public ItemDef Def { get; }
        public GridPos Position { get; internal set; }
        /// <summary>0-3: 90° adımlarla saat yönünde dönüş.</summary>
        public int Rotation { get; internal set; }
        /// <summary>Dekor ise üzerinde durduğu eşyanın kimliği.</summary>
        public int? ParentId { get; internal set; }
        public string SlotId { get; internal set; }

        public bool IsDecor => ParentId.HasValue;

        /// <summary>Dönüş uygulanmış genişlik/yükseklik.</summary>
        public int Width => IsDecor ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).w;
        public int Height => IsDecor ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).h;

        public ItemInstance(int id, ItemDef def, GridPos position, int rotation = 0)
        {
            Id = id;
            Def = def;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>Kapladığı karolar (dekor için boş).</summary>
        public IEnumerable<GridPos> OccupiedTiles()
        {
            if (IsDecor) yield break;
            int w = Width, h = Height;
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    yield return new GridPos(Position.X + dx, Position.Y + dy);
        }

        /// <summary>Bir yüzey yuvasının ızgara koordinatındaki (sürekli, karo köşesi 0) konumu.</summary>
        public bool TryGetSlotPosition(string slotId, out float gx, out float gy)
        {
            gx = gy = 0f;
            var slot = Def.FindSlot(slotId);
            if (slot == null) return false;
            var p = Footprint.RotatePoint(slot.U, slot.V, Def.Width, Def.Height, Rotation);
            gx = Position.X + p.x;
            gy = Position.Y + p.y;
            return true;
        }
    }

    /// <summary>Kaplama alanı ve dönüş hesapları.</summary>
    public static class Footprint
    {
        public static (int w, int h) Dimensions(int w, int h, int rotation)
        {
            int r = Normalize(rotation);
            return (r % 2 == 0) ? (w, h) : (h, w);
        }

        public static int Normalize(int rotation) => ((rotation % 4) + 4) % 4;

        /// <summary>
        /// Dönmemiş w x h alanı içindeki (u,v) (0-1) yerel noktayı, 90° adımla saat yönünde
        /// döndürülmüş alandaki sürekli koordinata çevirir.
        /// </summary>
        public static (float x, float y) RotatePoint(float u, float v, int w, int h, int rotation)
        {
            switch (Normalize(rotation))
            {
                case 0: return (u * w, v * h);
                case 1: return ((1f - v) * h, u * w);
                case 2: return ((1f - u) * w, (1f - v) * h);
                default: return (v * h, (1f - u) * w);
            }
        }
    }
}
