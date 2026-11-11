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

        /// <summary>Duvara asılı eşya: Position = önündeki karo, Rotation = asıldığı kenar (WallSide).</summary>
        public bool IsWallMounted => Def.Placement == ItemPlacement.Wall && !IsDecor;
        /// <summary>Duvar eşyasının asıldığı kenar.</summary>
        public Edge WallEdge => WallSides.EdgeOf(Position, Rotation);

        // ---- barınak (Adım: hayvanlar) ----
        /// <summary>Barınaktaki yem stoğu (birim).</summary>
        public double FeedStock { get; internal set; }
        private Dictionary<string, int> _stock;
        /// <summary>Barınakta toplanmayı bekleyen ürünler (yumurta, süt, yün).</summary>
        public IReadOnlyDictionary<string, int> Stock => _stock ?? (_stock = new Dictionary<string, int>());
        public int StockTotal { get { int n = 0; if (_stock != null) foreach (var v in _stock.Values) n += v; return n; } }

        internal void AddStock(string itemId, int count)
        {
            if (_stock == null) _stock = new Dictionary<string, int>();
            _stock.TryGetValue(itemId, out int n);
            _stock[itemId] = n + count;
        }

        internal void SetStock(string itemId, int count)
        {
            if (_stock == null) _stock = new Dictionary<string, int>();
            if (count > 0) _stock[itemId] = count; else _stock.Remove(itemId);
        }

        /// <summary>Isı kaynağının kalan yanma süresi (oyun günü). 0 = sönük.</summary>
        public double FuelDays { get; internal set; }
        public bool IsLit => FuelDays > 0;

        /// <summary>Bitkinin büyüme ilerlemesi (oyun günü).</summary>
        public double GrowthDays { get; internal set; }
        public bool IsPlant => Def.IsPlant;
        /// <summary>Bitki değilse ya da olgunlaştıysa true.</summary>
        public bool IsGrown => !IsPlant || GrowthDays >= Def.GrowDays;
        /// <summary>0-1 büyüme oranı (bitki değilse 1).</summary>
        public float GrowthFraction => IsPlant ? (float)System.Math.Min(1.0, GrowthDays / Def.GrowDays) : 1f;

        /// <summary>Dönüş uygulanmış genişlik/yükseklik.</summary>
        public int Width => IsDecor || IsWallMounted ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).w;
        public int Height => IsDecor || IsWallMounted ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).h;

        public ItemInstance(int id, ItemDef def, GridPos position, int rotation = 0)
        {
            Id = id;
            Def = def;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>Kapladığı karolar (dekor ve duvar eşyası için boş).</summary>
        public IEnumerable<GridPos> OccupiedTiles()
        {
            if (IsDecor || IsWallMounted) yield break;
            int w = Width, h = Height;
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    yield return new GridPos(Position.X + dx, Position.Y + dy, Position.Z);
        }

        /// <summary>Bir yüzey yuvasının ızgara koordinatındaki (sürekli, karo köşesi 0) konumu.</summary>
        public bool TryGetSlotPosition(string slotId, out float gx, out float gy)
        {
            gx = gy = 0f;
            var slot = Def.FindSlot(slotId);
            if (slot == null) return false;
            if (IsWallMounted)
            {
                // Duvar rafı: yuvalar duvar boyunca dizilir (U), duvarın hemen önünde.
                var (ax, ay) = WallSides.AlongWall(Rotation, slot.U);
                gx = Position.X + ax;
                gy = Position.Y + ay;
                return true;
            }
            var p = Footprint.RotatePoint(slot.U, slot.V, Def.Width, Def.Height, Rotation);
            gx = Position.X + p.x;
            gy = Position.Y + p.y;
            return true;
        }
    }

    /// <summary>
    /// Bir karonun dört kenarı: 0 = -y (Horizontal(x,y)), 1 = +x (Vertical(x+1,y)),
    /// 2 = +y (Horizontal(x,y+1)), 3 = -x (Vertical(x,y)). Duvar eşyası bu kenara asılır.
    /// </summary>
    public static class WallSides
    {
        public static Edge EdgeOf(GridPos tile, int side)
        {
            switch (Footprint.Normalize(side))
            {
                case 0: return new Edge(tile.X, tile.Y, EdgeAxis.Horizontal, tile.Z);
                case 1: return new Edge(tile.X + 1, tile.Y, EdgeAxis.Vertical, tile.Z);
                case 2: return new Edge(tile.X, tile.Y + 1, EdgeAxis.Horizontal, tile.Z);
                default: return new Edge(tile.X, tile.Y, EdgeAxis.Vertical, tile.Z);
            }
        }

        /// <summary>Kenar üzerindeki yerel nokta (u: kenar boyunca 0-1), karo köşesine göre, duvardan biraz içeride.</summary>
        public static (float x, float y) AlongWall(int side, float u)
        {
            const float inset = 0.12f;
            switch (Footprint.Normalize(side))
            {
                case 0: return (u, inset);
                case 1: return (1f - inset, u);
                case 2: return (u, 1f - inset);
                default: return (inset, u);
            }
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
