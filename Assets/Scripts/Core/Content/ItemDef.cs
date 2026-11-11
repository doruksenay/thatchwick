using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Eşyanın nereye konabildiği: yere (zemin karolarına) ya da başka eşyanın yüzeyine (dekor).</summary>
    public enum ItemPlacement
    {
        Floor = 0,
        Surface = 1,
        /// <summary>Yerleştirilmez; yalnızca envanterde sayılan kaynak (odun gibi).</summary>
        Resource = 2,
        /// <summary>Duvarın iç yüzüne asılır (meşale, duvar rafı, goblen); karo kaplamaz.</summary>
        Wall = 3
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
        /// <summary>Ekin (tarla bitkisi): toprağa ekilen, sulanması ve verimli toprağı gereken bitki. Ağaçlar ekin değildir.</summary>
        public bool IsCrop => IsPlant && NeedsTilledSoil;
        /// <summary>Meyve ağacı: bu mevsimlerde meyve verir (olgun ağaç kesilmeden tekrar tekrar toplanır). Boş = meyvesiz.</summary>
        public List<Time.Season> FruitSeasons { get; set; } = new List<Time.Season>();
        /// <summary>Meyvenin olgunlaşması için gereken gün (her toplamadan sonra yeniden başlar).</summary>
        public float FruitRipenDays { get; set; }
        public bool IsFruitTree => FruitSeasons.Count > 0 && FruitRipenDays > 0f;
        /// <summary>Baklagil: hasatta toprağı zenginleştirir (azot bağlar).</summary>
        public bool FixesNitrogen { get; set; }
        /// <summary>Sulama hendeği, kova ve oluk: her sabah bu yarıçaptaki (Chebyshev) ekinleri sular. 0 = sulamaz.</summary>
        public int WaterRadius { get; set; }
        /// <summary>Arı kovanı: yaşayan koloni varken bu yarıçaptaki ekinler ve meyve ağaçları daha çabuk olgunlaşır. 0 = tozlaştırmaz.</summary>
        public int PollinateRadius { get; set; }
        /// <summary>Korkuluk: bu yarıçaptaki ekinleri kuzgunlardan korur. 0 = korumaz.</summary>
        public int ProtectRadius { get; set; }
        /// <summary>Oda konforuna katkısı (dekor ve mobilya). Adım 32.</summary>
        public int Comfort { get; set; }
        /// <summary>Taze yiyecek bozulma süresi (oyun günü); 0 = çürümez.</summary>
        public float SpoilDays { get; set; }
        /// <summary>Kiler eşyası: yiyecek bozulmasını bu çarpanla yavaşlatır (0 = kiler değil).</summary>
        public float Preserves { get; set; }
        public List<string> Variants { get; set; } = new List<string>();
        /// <summary>Yabani toplanabilir: görüneceği mevsimler ve haritada en çok kaç tane.</summary>
        public List<Time.Season> ForageSeasons { get; set; } = new List<Time.Season>();
        public int ForageMax { get; set; }
        public bool IsForage => ForageSeasons.Count > 0 && ForageMax > 0;

        /// <summary>Stil seti (Köylü, Zanaatkâr, Tüccar, Asil): aynı odada 4+ eşya "Uyumlu ev" verir. Boş = setsiz.</summary>
        public string StyleSet { get; set; }
        /// <summary>Satın alma menüsünde oda filtresi (bedroom, kitchen, living, outdoor, kids).</summary>
        public List<string> Rooms { get; set; } = new List<string>();
        /// <summary>Hayvan barınağı türü (coop, barn, kennel ...); boş = barınak değil.</summary>
        public string HousingKind { get; set; }
        /// <summary>Barınağın alabileceği hayvan sayısı.</summary>
        public int HousingCapacity { get; set; }
        public bool IsHousing => !string.IsNullOrEmpty(HousingKind);

        /// <summary>Merdiven ise kat farkı: +1 yukarı çıkar, -1 aşağı (bodrum) iner; 0 = merdiven değil. Ayak izi 1x2'dir.</summary>
        public int StairsDelta { get; set; }
        public bool IsStairs => StairsDelta != 0;
        /// <summary>Eşyanın konabileceği katlar; boşsa kurala göre (bkz. AllowsLevel).</summary>
        public List<int> AllowedLevels { get; set; } = new List<int>();

        /// <summary>
        /// Bu katta durabilir mi: açıkça belirtilen katlar varsa onlar; yoksa bitki, barınak ve toplama düğümü
        /// yalnızca zemin katta, geri kalan her katta (üst kat, bodrum).
        /// </summary>
        public bool AllowsLevel(int level)
        {
            if (AllowedLevels.Count > 0) return AllowedLevels.Contains(level);
            if (IsPlant || IsHousing || IsForage || PlantsAs != null) return level == 0;
            return true;
        }

        public SurfaceSlotDef FindSlot(string slotId)
        {
            foreach (var s in SurfaceSlots)
                if (s.Id == slotId) return s;
            return null;
        }
    }
}
