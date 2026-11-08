using System;

namespace Thatchwick.Core.Content
{
    // JSON dosyasının birebir şekli. Alan adları JSON anahtarlarıdır (Unity JsonUtility
    // genel alan ve [Serializable] ister); Unity'siz kod bu sınıfları doğrudan doldurabilir.

    [Serializable]
    public sealed class ContentFileDto
    {
        public ItemDto[] items;
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
    }

    [Serializable]
    public sealed class ProduceDto
    {
        public string item;
        public int count;      // 0 ya da eksik = 1
        public float chance;   // 0 ya da eksik = 1 (kesin)
    }
}
