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
        /// <summary>Depolama eşyası ise kaynak kapasitesine eklediği birim (sandık 40). 0 = depo değil.</summary>
        public int Storage { get; set; }
        /// <summary>Pazar etiketleri (fuel, food ...): mevsimlik talep çarpanları bunlara göre.</summary>
        public List<string> Tags { get; set; } = new List<string>();
        public bool HasTag(string tag) => Tags.Contains(tag);
        /// <summary>Dükkânda görünmesi için gereken hesap seviyesi (T10). 0/1 = baştan açık.</summary>
        public int UnlockLevel { get; set; } = 1;
        /// <summary>Ekin: yalnızca toprak (sürülmüş) zemine dikilir.</summary>
        public bool NeedsTilledSoil { get; set; }
        /// <summary>Kış gelince donar ve kaybolur (hasat edilmemiş ekin).</summary>
        public bool DiesInWinter { get; set; }
        /// <summary>Ekinin büyüyebildiği mevsimler; boşsa kış dışı hepsi.</summary>
        public List<Time.Season> GrowSeasons { get; set; } = new List<Time.Season>();
        /// <summary>Oda konforuna katkısı (dekor ve mobilya). Adım 32.</summary>
        public int Comfort { get; set; }
        /// <summary>Stil seti (koyulu, zanaatkar, tuccar, asil); aynı odada 4+ eşya "uyumlu oda" verir.</summary>
        public string StyleSet { get; set; }
        /// <summary>Taze yiyecek bozulma süresi (oyun günü); 0 = çürümez.</summary>
        public float SpoilDays { get; set; }
        /// <summary>Kiler eşyası: yiyecek bozulmasını bu çarpanla yavaşlatır (0 = kiler değil).</summary>
        public float Preserves { get; set; }
        /// <summary>Üreten eşya (kümes): ürün hazır olma süresi (oyun günü); 0 = üretmez.</summary>
        public float ReadyDays { get; set; }
        public List<string> Variants { get; set; } = new List<string>();
        /// <summary>Yabani toplanabilir: görüneceği mevsimler ve haritada en çok kaç tane.</summary>
        public List<Time.Season> ForageSeasons { get; set; } = new List<Time.Season>();
        public int ForageMax { get; set; }
        public bool IsForage => ForageSeasons.Count > 0 && ForageMax > 0;
        public bool Produces => ReadyDays > 0f;

        public SurfaceSlotDef FindSlot(string slotId)
        {
            foreach (var s in SurfaceSlots)
                if (s.Id == slotId) return s;
            return null;
        }
    }
}
