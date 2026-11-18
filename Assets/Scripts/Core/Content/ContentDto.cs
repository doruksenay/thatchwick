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
        public BuildPartDto[] buildParts;   // yapı modu malzeme ve parça kataloğu (duvar, kapı, pencere, zemin, çatı, kaplama)
        public EffectDto[] effects;         // yemek ve içki etkileri (moodlet olarak kaydedilir; etkileşimin "moodlet" alanından verilir)

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
            buildParts = Concat(buildParts, other.buildParts);
            effects = Concat(effects, other.effects);
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
        public float[] seasonOutput;   // ürün hızı çarpanı mevsime göre [ilkbahar, yaz, güz, kış]; boşsa hep 1
        public float lifespanDays;
        public float matureDays;
        public bool pet;
        public int level;
        public string meat;       // kesilince verdiği et
        public int meatCount;     // 0 = 1
        public string hide;       // kesilince verdiği ham deri
        public int hideCount;     // 0 = 1
        public bool rideable;     // erişkin ve eyerliyken binilir (at)
        public float rideSpeed;   // binince yürüme hızı çarpanı (0 = 2)
        public float space;       // barınak odasında hayvan başına istenen karo (0 = türe göre)
        public float trample = -1f; // bir adımın aşındırması (-1 = barınağa göre: ahır 2, at ahırı 3, kümes 0)
        public float gestationDays;   // gebelik/kuluçka günü (0 = 1)
        public int litterMin, litterMax;  // yavru sayısı (0 = 1)
        public bool femaleProduces;   // ürünü yalnızca dişi verir (yumurta, süt)
        public bool draft;            // çeki hayvanı (düven)
        public string femaleName, maleName;
    }

    [Serializable]
    public sealed class AutoProcessDto
    {
        public string item;          // her gün envanterden alınan girdi
        public int count;
        public string produces;      // envantere eklenen ürün
        public int producesCount;
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
        public string finishGroup;   // mobilya kaplama grubu (wood, cloth, metal, clay): renk/malzeme örnekleri
        // Ev dekorasyonu (Project belgesi 27, D1)
        public string decorSize;     // dekor boyutu: S/M/L ya da K/O/B (boş = küçük)
        public int weight;           // dekor yükü (0 = 1)
        public string[] fitsOn;      // konabildiği yuva türleri: masa, raf, kanca, ocak, pencere, kiler, tabak, minder (boş = masa, raf, ocak, pencere)
        public int maxLoad;          // yüzeyin toplam taşıması (0 = sınırsız)
        public string[] wallBands;   // duvar eşyasının bantları: alt, orta, üst (boş = orta)
        public int wallWidth;        // duvar eşyasının genişliği (kenar; 0 = 1)
        public bool tall;            // uzun mobilya: önündeki duvarın alt ve orta bandını kapatır
        public int beauty;           // oda güzelliğine katkı
        public string material;      // wood, metal, cloth, clay, glass, stone (boş = kaplama grubundan)
        public bool mirror;          // ayna: Mirror (görünüş değiştirme) bunu ister
        public float tarnishDays;    // metal ayna bu kadar günde kararır (0 = kararmaz)
        public string styleTheme;    // tarz ekseni: Çatkı, Yörük ...
        // Hayvancılık ve otomasyon (17.11.2026)
        public bool requiresIndoor;  // yalnızca kapalı bir odaya konur (folluk, ahır bölmesi)
        public bool trough;          // yemlik: odadaki bütün hayvanlar buradan yer
        public int animalComfort;    // hayvan konforuna katkı (süs: saman balyası, tuz yalağı)
        public string animalRole;    // water (su yalağı), bedding (altlık), collector (toplama sandığı), feeder (yem oluğu), bell (çan)
        public bool waterSource;     // sulama ağının kaynağı (kuyu, kova-makara)
        public bool needsSource;     // yalnızca kaynağa bağlı ağdayken sular (kanal)
        public bool powerSource;     // su çarkı: kaynağa bağlı kanala değerse yanındaki makineleri çalıştırır
        public AutoProcessDto autoProcess;   // güç alan makine: her gün kendiliğinden işler (değirmen taşı)
        public bool iceBox;                  // buzluk: içinde buz varken yiyeceği çok soğuk tutar
        public bool fishTrap;                // balık sepeti: gölete değerken her sabah balık
        public bool windPower;               // yel değirmeni: suya gerek olmadan güç verir
        public bool waterGate;               // su kapağı: açıkken suyu geçirir, kapalıyken keser
        public int fillCapacity;             // lazımlık/hela: bu kadar kullanımda dolar
        public string fillProduct;           // boşaltınca verdiği ürün (gübre)
        public int fillPerProduct;           // kaç dolum bir ürün (0 = 4)
        public string cropFamily;            // ekin ailesi (nöbetleşe ekim)
        public string threshesTo;            // demet: harmanda bu taneye döner
        public int threshPerDay;             // düven: çeki hayvanıyla her sabah dövdüğü demet
        public int harvestRadius;            // hasat tırmığı: güç alınca bu yarıçaptaki olgun ekini her sabah biçer
        public int seedRadius;               // ekim makinesi: güç alınca bu yarıçaptaki boş toprağa tohum eker
        public int iceCollect;               // buz kuyusu: donlu sabahlar bu kadar buz toplar
        public int foodCapacity;             // yiyecek yerleşiminde kapasite (0 = depo kapasitesi; kilerde 60)
        // Su şebekesi ve tesisat
        public int waterFlow;                // kaynağın boruya günlük suyu (0 = 4, -1 = vermez: kuyu)
        public int waterUse;                 // tesisat: bağlıyken günde istediği su (leğen, küvet, akarsulu hela, çeşme)
        public bool waterHead;               // su terazisi: üst kat tesisatına basınç
        public int cistern;                  // sarnıç: biriktirdiği en çok su
        public int cisternRain;              // yağmurlu gün dolan su
        public int cisternFlow;              // günde en çok verdiği su (0 = 4)
        public bool tap;                     // çeşme/lüle: su gelirken kova buradan doldurulur
    }

    /// <summary>Yemek/içki etkisi: süreli ruh hali etkisi (moodlet) olarak kaydedilir.</summary>
    [Serializable]
    public sealed class EffectDto
    {
        public string id;
        public string displayName;
        public string description;
        public int value;            // ruh hali
        public float days;           // süre (gün)
        public float allSpeed, allXp;
        public float coldResist;     // soğuğa dayanıklılık (0,2 = %20)
        public float hungerDecay;    // 0 = 1; 0,9 = %10 yavaş acıkır
        public float energyDecay;    // 0 = 1
        public SkillRateDto[] skillSpeed;  // beceriye özel hız (rate = ek oran, 0,1 = %10)
        public SkillRateDto[] skillXp;     // beceriye özel XP
    }

    [Serializable]
    public sealed class BuildPartDto
    {
        public string id;
        public string displayName;
        public string kind;          // wallMaterial, wallFinish, door, window, fence, gate, railing, floor, roof, itemFinish
        public int price;
        public int level;            // 0 = 1
        public string color;         // yer tutucu renk #RRGGBB
        public string floorBase;     // zemin: Dirt, Wood, Stone
        public float heatLoss;       // 0 = 1
        public int comfort;
        public string group;         // itemFinish: wood, cloth, metal, clay
        public float priceFactor;    // itemFinish: 0 = 1
        public int[] levels;         // konabileceği katlar (boş = hepsi)
        public bool isDefault;
        public string material;      // varsa: envanterde bu malzeme varken harcanır, akçe yarıya iner (tuğla, kiremit, yontma taş, boya)
        public int materialCount;    // birim (kenar, karo, eşya) başına; 0 = 1
    }

    [Serializable]
    public sealed class SurfaceSlotDto
    {
        public string id;
        public float u;
        public float v;
        public string kind;          // masa, raf, kanca, ocak, pencere, kiler, tabak, minder (boş = yerleşime göre)
        public string size;          // S/M/L ya da K/O/B (boş = büyük)
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
        public float addIce;             // buzluğa eklenen buz (gün)
        public float befriendAnimals;    // barınaktaki hayvanlara dostluk (merhem; günlük sevme hakkını kullanmaz)
        public bool teach;               // hanedeki çocuk ve gençlere ders (öğretmenlik)
        public float bladder;            // tuvaleti doldurur (içecek 15, yemek 4)
        public bool fetchWater;          // önce en yakın sudan bir kova taşınır (banyo, leğen)
        public float wets;               // ıslatır (0-1)
        public bool bath;                // soğuk/sıcak su kuralı (ocak yanan oda sıcak)
        public int addFill;              // eşyanın dolumunu artırır (lazımlık, hela)
        public bool emptyFill;           // dolumu boşaltır, gübre verir
        public bool dries;               // kurular (havlu, ocak başı)
        public float heal;               // soğuk/açlık yarasını bu kadar gün iyileştirir (hekimlik)
        public bool requiresFrost;       // yalnızca don varken (buz kesmek)
        public bool piped;               // yalnızca tesisata su gelirken (çeşmede yıkanmak)
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
        public string repair;               // onarım istasyonu: wood, metal, cloth (hasarlı eşyayı onarır)
        public bool polish;                 // kararmış metal aynayı cilalar
        public bool collectContents;        // döküntü yığınındakileri envantere alır
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
        public string[] seasons; // yalnızca bu mevsimlerde (boş = her mevsim): balık türleri
    }
}
