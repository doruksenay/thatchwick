using System;

namespace Thatchwick.Core.Content
{
    // JSON dosyasının birebir şekli. Alan adları JSON anahtarlarıdır (Unity JsonUtility
    // genel alan ve [Serializable] ister); Unity'siz kod bu sınıfları doğrudan doldurabilir.

    [Serializable]
    public sealed class ContentFileDto
    {
        public ItemDto[] items;
        public RecipeDto[] recipes;     // marangoz tezgâhı tarifleri (istasyon eşyasına "craft:<id>" etkileşimi olur)
        public AnimalDto[] animals;     // hayvan türleri
    }

    [Serializable]
    public sealed class RecipeDto
    {
        public string id;
        public string displayName;      // boş = "Yap: <ürün adı>"
        public string station;          // istasyon eşyası kimliği (marangoz tezgâhı)
        public string produces;
        public int count;               // 0 = 1
        public ConsumeDto[] materials;
        public string skill;            // varsayılan Carpentry
        public int minLevel;
        public double durationDays;
        public float skillXp;
        public string moodlet;          // boş = "proud"
    }

    [Serializable]
    public sealed class AnimalDto
    {
        public string id;
        public string displayName;
        public int price;
        public string housing;
        public float feedPerDay;
        public string produces;
        public float produceEveryDays;
        public float lifespanDays;
        public float matureDays;
        public bool pet;
        public int level;
    }

    [Serializable]
    public sealed class ConsumeDto
    {
        public string item;
        public int count;    // 0 = 1
    }

    [Serializable]
    public sealed class ItemDto
    {
        public string id;
        public string displayName;
        public InteractionDto[] interactions;
        public int width;            // 0 ya da eksik = 1
        public int height;           // 0 ya da eksik = 1
        public string placement;     // "Floor" (varsayılan) ya da "Surface"
        public SurfaceSlotDto[] surfaceSlots;
        public int price;
        public string category;      // boş = furniture
        public float heat;           // ısı gücü; 0 = ısı kaynağı değil
        public float growDays;       // bitki: olgunlaşma süresi (gün); 0 = bitki değil
        public string plantsAs;      // fidan: dikilince oluşan bitki kimliği
        public bool tilled;          // ekin: yalnızca toprak zemine
        public int storage;          // depo kapasitesi (sandık); 0 = depo değil
        public string[] tags;        // pazar etiketleri: fuel, food
        public int level;            // dükkânda açılma seviyesi; 0 = 1
        public bool diesInWinter;    // ekin: kışın donar
        public string styleSet;      // Köylü / Zanaatkâr / Tüccar / Asil
        public string[] rooms;       // menü oda filtresi
        public string housing;       // hayvan barınağı türü (coop, barn, kennel)
        public int capacity;         // barınak kapasitesi
    }

    [Serializable]
    public sealed class SurfaceSlotDto
    {
        public string id;
        public float u;
        public float v;
    }

    [Serializable]
    public sealed class InteractionDto
    {
        public string id;
        public string displayName;
        public string need;
        public float restore;
        public double durationDays;
        public string skill;   // isteğe bağlı: Farming/Woodcutting/Cooking/Carpentry
        public float skillXp;
        public bool requiresGrown;
        public string consume;           // harcanan kaynak kimliği (1 adet)
        public ProduceDto[] produce;
        public bool removeItem;
        public ConsumeDto[] materials;   // çok malzemeli harcama
        public string moodlet;           // bitince verilen moodlet
        public int minLevel;             // gereken beceri seviyesi
        public int addFeed;              // barınağa eklenen yem
        public bool collect;             // barınak ürünlerini topla
        public bool requiresAnimal;      // barınakta hayvan olmalı
    }

    [Serializable]
    public sealed class ProduceDto
    {
        public string item;
        public int count;      // 0 ya da eksik = 1
        public float chance;   // 0 ya da eksik = 1 (kesin)
    }
}
