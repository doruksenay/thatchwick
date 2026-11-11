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
        // life.json bölümleri (Adım 18a-43). Hepsi isteğe bağlıdır.
        public TraitDto[] traits;
        public BranchDto[] branches;
        public ProjectDto[] projects;
        public VillagerDto[] villagers;
        public FestivalDto[] festivals;
        public GoalDto[] goals;
        public ToolDto[] tools;
        public RoomTemplateDto[] templates;

        /// <summary>Başka bir içerik dosyasının bölümlerini bunun sonuna ekler (items.json + life.json birleşimi).</summary>
        public void Merge(ContentFileDto other)
        {
            if (other == null) return;
            items = Concat(items, other.items);
            recipes = Concat(recipes, other.recipes);
            animals = Concat(animals, other.animals);
            traits = Concat(traits, other.traits);
            branches = Concat(branches, other.branches);
            projects = Concat(projects, other.projects);
            villagers = Concat(villagers, other.villagers);
            festivals = Concat(festivals, other.festivals);
            goals = Concat(goals, other.goals);
            tools = Concat(tools, other.tools);
            templates = Concat(templates, other.templates);
        }

        private static T[] Concat<T>(T[] a, T[] b)
        {
            if (b == null || b.Length == 0) return a;
            if (a == null || a.Length == 0) return b;
            var r = new T[a.Length + b.Length];
            Array.Copy(a, r, a.Length);
            Array.Copy(b, 0, r, a.Length, b.Length);
            return r;
        }
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
        public string meat;       // kesilince verdiği et
        public int meatCount;     // 0 = 1
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
        public int stairs;           // +1 yukarı, -1 aşağı (bodrum) merdiveni
        public int[] levels;         // eşyanın konabileceği katlar (boşsa kurala göre)
        public string[] growSeasons; // ekin: büyüyebildiği mevsimler (boş = kış dışı hepsi)
        public int comfort;          // oda konforuna katkı
        public float spoilDays;      // taze yiyecek: bu kadar günde çürür; 0 = çürümez
        public float preserves;      // kiler: yiyecek bozulma çarpanı (0,4 = %60 yavaş); 0 = kiler değil
        public string[] variants;    // renk/malzeme varyantları (Adım 43)
        public string[] forageSeasons; // yabani toplanabilir: görüneceği mevsimler
        public int forageMax;        // haritada aynı anda en çok bu kadar
        public string[] fruitSeasons; // meyve ağacı: meyve verdiği mevsimler
        public float fruitDays;      // meyve ağacı: olgunlaşma süresi (gün)
        public bool nitrogen;        // baklagil: hasatta toprağı zenginleştirir
        public int waterRadius;      // sulama hendeği/kova: her sabah sular
        public int protectRadius;    // korkuluk: kuzgunlardan korur
        public int pollinate;        // arı kovanı: tozlaştırma yarıçapı
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
        public NeedRestoreDto[] extraRestore; // ana ihtiyaca ek olarak doyurulan ihtiyaçlar
        public string tool;                 // axe/hoe/saw: alet kademesi iş hızını etkiler
        public string moodId;               // etkileşim sonunda verilen ruh hali etkisi
        public string moodText;
        public int moodValue;
        public float moodDays;
        public bool requiresLit;            // yalnızca yanan ısı kaynağında
        public bool water;                  // ekini sular
        public float fertilize;             // toprak verimine eklenen (0-1)
        public bool requiresFruit;          // meyvesi olgun ağaç gerekir
        public bool takesFruit;             // meyveyi toplar (ağaç kalır)
        public bool petAnimals;             // barınaktaki hayvanları sever
        public bool slaughter;              // barınaktan bir yetişkin hayvanı keser (et verir)
    }

    [Serializable]
    public sealed class NeedRestoreDto
    {
        public string need;
        public float amount;
    }

    [Serializable]
    public sealed class ProduceDto
    {
        public string item;
        public int count;      // 0 ya da eksik = 1
        public float chance;   // 0 ya da eksik = 1 (kesin)
        public int minLevel;   // etkileşimin becerisinde bu seviye ve üstü (0 = sınır yok)
        public int maxLevel;   // bu seviye ve altı (0 = sınır yok)
    }
}
