using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Eşyanın nereye konabildiği: yere (zemin karolarına) ya da başka eşyanın yüzeyine (dekor).</summary>
    public enum ItemPlacement
    {
        Floor = 0,
        Surface = 1,
        /// <summary>Yerleştirilmez; yalnızca envanterde sayılan kaynak (odun gibi).</summary>
        Resource = 2
    }

    /// <summary>Bir eşyanın yüzeyindeki dekor yuvası. U,V: kapladığı alanın içinde 0-1 arası yerel konum.</summary>
    public sealed class SurfaceSlotDef
    {
        public string Id { get; set; }
        public float U { get; set; }
        public float V { get; set; }
    }

    /// <summary>Yerleştirilebilir/kullanılabilir eşya tanımı. İçerik sonradan paketlerle genişler.</summary>
    public sealed class ItemDef : IContentDef
    {
        public const int MaxSize = 6;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<InteractionDef> Interactions { get; set; } = new List<InteractionDef>();

        /// <summary>Dönmemiş haldeki kapladığı karo sayısı (1x1, bir yatak 2x1 ...).</summary>
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public ItemPlacement Placement { get; set; } = ItemPlacement.Floor;
        /// <summary>Üstüne dekor konabilen yüzey yuvaları (masa, raf).</summary>
        public List<SurfaceSlotDef> SurfaceSlots { get; set; } = new List<SurfaceSlotDef>();
        /// <summary>Akçe cinsinden fiyat (satış bunun yarısı). 0 = ücretsiz/başlangıç eşyası.</summary>
        public int Price { get; set; }
        /// <summary>Yapı modu menüsü kategorisi (furniture, decor, utility ...).</summary>
        public string Category { get; set; } = "furniture";
        /// <summary>Yanarken bulunduğu odaya verdiği ısı gücü (0 = ısı kaynağı değil). Yakıt gerektirir.</summary>
        public float HeatOutput { get; set; }

        /// <summary>Bitki ise olgunlaşma süresi (oyun günü, yalnızca dış mekânda ve kış dışında büyür). 0 = bitki değil.</summary>
        public float GrowDays { get; set; }
        /// <summary>Fidan/tohum kaynağı ise dikilince oluşan bitkinin kimliği.</summary>
        public string PlantsAs { get; set; }

        public bool IsPlant => GrowDays > 0f;

        public SurfaceSlotDef FindSlot(string slotId)
        {
            foreach (var s in SurfaceSlots)
                if (s.Id == slotId) return s;
            return null;
        }
    }
}
