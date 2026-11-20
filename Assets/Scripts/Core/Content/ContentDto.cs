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
        public SkillUnlockDto[] skillUnlocks;   // beceri seviyesi → açılan (belge 45, life.json)
        public ChainDto[] chains;               // izlenen eylem zincirleri (belge 60, life.json)

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
            skillUnlocks = Concat(skillUnlocks, other.skillUnlocks);
            chains = Concat(chains, other.chains);
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
        public string requiresBranch;   // yalnızca bu dalı seçmiş karakter yapar (D3: Camcı)
        public bool adultPack;          // +18 paketi (şarap): paket kapalıysa görünmez
        public string cook;             // pişirme yöntemi (63 §2.3)
        public string[] needTools;      // ek gerekenler
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
        public bool maleOnly;              // iğdiş öküz gibi: hep erkek, üremez
        public bool petBreeds;             // evcil hayvan da yavrular (köpek, kedi)
        public bool sterile;               // kısır melez (katır, bardo)
        public bool herd;                  // köy merasına ve yaylaya gönderilir
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
        public string lockSkill;     // kilit becerisi (belge 45): tarif ve kategori yerine bu beceri
        public string plantsAs;      // fidan: dikilince oluşan bitki kimliği
        public bool tilled;          // ekin: yalnızca toprak zemine
        public int storage;          // depo kapasitesi (sandık); 0 = depo değil
        public string[] tags;        // pazar etiketleri: fuel, food
        public int level;            // dükkânda açılma seviyesi; 0 = 1
        public bool diesInWinter;    // ekin: kışın donar
        public bool warmOnly;        // yalnızca ılık serada büyür ve meyve verir (kahve, belge 32)
        public bool needsOil;        // ışığı yalnızca yağı varken verir (zeytinyağı kandili)
        public bool southOnly;       // yalnızca Güney Yamaçları'nda büyür (zeytin, belge 32-7)
        public string styleSet;      // Köylü / Zanaatkâr / Tüccar / Asil
        public string[] rooms;       // menü oda filtresi
        public string housing;       // hayvan barınağı türü (coop, barn, kennel)
        public int capacity;         // barınak kapasitesi
        public int stairs;           // +1 yukarı, -1 aşağı (bodrum) merdiveni
        public int[] levels;         // eşyanın konabileceği katlar (boşsa kurala göre)
        public string[] growSeasons; // ekin: büyüyebildiği mevsimler (boş = kış dışı hepsi)
        public int comfort;          // oda konforuna katkı
        public float spoilDays;      // taze yiyecek: bu kadar günde çürür; 0 = çürümez
        public string quality;       // kalite sınıfı (53): "none" | "goods" | "crafted"; boşsa etiket ve tariften
        public float ageDays;        // mahzende olgunlaşma: bu kadar günde bir kalite basamağı (53, karar 4); 0 = olgunlaşmaz
        public float preserves;      // kiler: yiyecek bozulma çarpanı (0,4 = %60 yavaş); 0 = kiler değil
        public string[] variants;    // renk/malzeme varyantları (Adım 43)
        public string[] forageSeasons; // yabani toplanabilir: görüneceği mevsimler
        public int forageMax;        // haritada aynı anda en çok bu kadar
        public string[] forageRegions; // yalnızca bu bölgelerde belirir (coast, lake, forest ...); boş = her yerde
        public int fishDifficulty;   // balık tutma mini oyunu: zorluk 0-100 (0 = 30)
        public string fishBehavior;  // smooth | darting | sinker | floater | mixed
        public string[] fruitSeasons; // meyve ağacı: meyve verdiği mevsimler
        public float fruitDays;      // meyve ağacı: olgunlaşma süresi (gün)
        public bool nitrogen;        // baklagil: hasatta toprağı zenginleştirir
        public int waterRadius;      // sulama hendeği/kova: her sabah sular
        public int protectRadius;    // korkuluk: kuzgunlardan korur
        public int pollinate;        // arı kovanı: tozlaştırma yarıçapı
        public string finishGroup;   // mobilya kaplama grubu (wood, cloth, metal, clay, stone): renk/malzeme örnekleri
        public string accentGroup;   // ikinci kaplama grubu: korkuluk, minder, kulp (belge 45 §10)
        public string accentLabel;   // ikinci kaplamanın adı (Korkuluk, Minder, Kulp ve kuşak)
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
        public string[] fillWith;    // doldurulan dekor: kabul ettiği ürünler (D2)
        public int fillCount;        // doldurulan dekorun aldığı adet
        public bool window;          // yalnızca pencere kenarına asılır (pencere içi tahtası, perde)
        public float insulation;     // asıldığı kenarın ısı kaybı çarpanı (0 = etkisiz)
        public string equip;         // mutfak rolü (63, 63a): vessel:boil|fry|stew|grill|bake, tool:knife|board|spoon|ladle|pin|peel|colander, tableware, basket
        public int equipTier;        // kabın kademesi 1-3
        public bool cookFire;        // üstünde pişirilen ateş: yakıt harcar
        public string washPlace;     // bulaşık yıkama yeri: tub, spout, stone
        public string requiresNear;  // üretim için yakında olması gereken istasyon (D3 tamamlama)
        public int nearRadius;       // requiresNear için en çok karo (0 = 4)
        public string shopUnlock;    // craftonly eşya bu kilitle katalogda da satılır (glassworks)
        public string[] seasonal;    // mevsim süsü: taze durduğu mevsimler (D3b)
        // Ev bakımı ve otomasyon (belge 45)
        public float burnDays;       // mum: bir mumun yanma süresi (akşam saatlerinde azalır); 0 = yanıp bitmez
        public string[] burnRefill;  // mum değiştirirken harcanan mum (sırayla; boş = kendisi, sonra item.candle)
        public int lightPower;       // ışık gücü (mum 1, fener 2, avize 4); 0 = ışık eşyasında 1
        public string potHerb;       // canlı saksı: hasatta verdiği ot
        public float potGrowDays;    // canlı saksı: olgunlaşma günü (0 = 3)
        public bool livePlant;       // canlı süs bitkisi (ot vermez, çiçek açar)
        public VivDto viv;           // cam bahçe türü: bitki ya da canlı (belge 47)
        public CatchDto[] catches;   // bu toplama yerinde cam kavanozla yakalanabilenler (belge 47)
        public int vivCreatures;     // cam bahçe: en çok canlı
        public int vivPlants;        // cam bahçe: en çok bitki
        public string[] gathers;     // toplama yerinde kavanoza doldurulan zemin malzemesi (belge 47 ek)
        public int vivDecor;         // cam bahçe: en çok süs (taş, kabuk)
        public string[] bloomSeasons; // çiçek açtığı mevsimler
        public bool shadePlant;      // gölge sever (eğrelti, yosun, sarmaşık)
        public float thirst;         // susama çarpanı (0 = 1; su teknesi 0,4)
        public UpgradeDto[] upgrades; // eşya yükseltmeleri (şilte, demir kuşak, cila, baca)
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
        public bool snare;                   // kement tuzağı: haritaya kurulur, av tuzakta bekler
        public bool fishTrap;                // balık sepeti: gölete değerken her sabah balık
        public bool windPower;               // yel değirmeni: suya gerek olmadan güç verir
        public bool waterGate;               // su kapağı: açıkken suyu geçirir, kapalıyken keser
        public int fillCapacity;             // lazımlık/hela: bu kadar kullanımda dolar
        public string fillProduct;           // boşaltınca verdiği ürün (gübre)
        public int fillPerProduct;           // kaç dolum bir ürün (0 = 4)
        public string cropFamily;            // ekin ailesi (nöbetleşe ekim)
        public string feed;                  // beslenme tipi (57c): heavy | medium | light | lean | builder (boş: nitrogen ise builder, değilse medium)
        public bool eatenRaw;                // çiğ yenen ekin (pislikli ham kompost riski)
        public string[] likesFertilizer;     // kalite veren gübreler: ash, bone, hot, seaweed...
        public string[] dislikesFertilizer;  // ürünü azaltan gübreler
        public string cycle;                 // hasat döngüsü: single (varsayılan), repeat, cut, perennial (59 §1)
        public int maxHarvests;              // repeat/cut: en çok kaç toplama
        public float regrowDays;             // toplamadan sonra yeniden olgunlaşma (gün)
        public bool needsStake;              // sırık ister; sırıksız ürün yarım
        public string giantAs;               // 2×2 olgun karo bu dev ürün eşyasına dönüşebilir (59 §2)
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
        // Yerleşim sistemi (belge 44, Aşama B): görsel, ön yüz, yer ve ilişki
        public ItemArtDto art;               // görseller (yön/eksen başına), pivot ve görsel boyutu; ayak izinden ayrı
        public string front;                 // ön yüzü olan eşyanın kullanımı: sit, open, work, stand (boş = ön yüz yok)
        public int frontDepth;               // ön yüzde boş kalacak derinlik (0 = 1)
        public string site;                  // indoor, outdoor, any (boş = any)
        public string[] belongs;             // ait olduğu oda türleri (bedroom, kitchen, dining, living, pantry, cellar, storehouse, workshop, shop, inn_room, barn, outdoor, kids)
        public string[] trades;              // meslek etiketleri (smithy, tailor, potter ...)
        public bool anywhere;                // bilinçli "her yer" (oda türü denetimi yapılmaz)
        public string relation;              // faceTable, bedside, nearHearth (uzun eşya duvar kuralı tall'dan gelir)
    }

    /// <summary>
    /// Eşyanın görselleri (belge 44 B). Yön görselleri: n/e/s/w (Core.Layout.Dir4; n = ekranda se, w = sw, e = ne, s = nw).
    /// Eksen görselleri: alongX çift dönüşte (0/2), alongY tek dönüşte (1/3). mirror: sol/sağ simetrik, yatay ayna geçerli
    /// (se ↔ sw); arkası dönük görünüm aynayla elde edilmez. Pivot piksel, alttan; 0 ise ayak izine göre varsayılan.
    /// </summary>
    [Serializable]
    public sealed class ItemArtDto
    {
        public string alongX, alongY, lit;
        public string n, e, s, w;
        public bool mirror;
        public float pivotX, pivotY;
        public int pxW, pxH;
        public string young, grown, fruit;   // bitki evreleri
        public int[] rotations;              // yalnızca bu dönüşler (merdiven: görsel tek yöne çıkar); boş = görsele göre
        public bool pending;                 // görsel eşlendi ama dosya henüz depoda yok (gelene kadar yer tutucu)
    }

    /// <summary>Cam bahçe türü (belge 47): bitki ya da canlı.</summary>
    [Serializable]
    public sealed class VivDto
    {
        public string kind;          // plant | creature | decor (taş, kabuk, kabuk parçası: canlı değil, süs ve sığınak)
        public string[] habitats;    // nemli | kuru | tuzlu | tatli; süste boş = her zemin
        public string[] eats;        // yediği türler; "*plant" her bitki, "*flower" çiçekli bitki
        public string[] tags;        // flower, cleaner, pest, toxic, glow, sings, shade, sun; süste hide (sığınak), bask (güneşlenme taşı), shell (boş kabuk)
        public string[] needs;       // canlının istediği süs etiketi (kertenkele bask, keşiş yengeci shell); yoksa hastalanır
        public float beauty;         // süsün güzelliği (0 = 1)
        public string move;          // crawl | hop | fly | static | swim | burrow | sway
        public float lifeDays;       // ömür (gün); 0 = sınırsız (bitki)
        public bool breeds;          // iki tane ve tokken çoğalır
        public string becomes;       // dönüşür (tırtıl → kelebek)
        public float becomeDays;
        public string[] avoid;       // bir arada olamaz (eklenemez)
        public bool solitary;        // aynı türden ikincisi olmaz
        public string color;         // görsel gelene kadar yer tutucu renk
    }

    /// <summary>Toplama yerinde yakalanan canlı ya da fidan (belge 47).</summary>
    [Serializable]
    public sealed class CatchDto
    {
        public string item;
        public float chance;
        public string[] seasons;
        public bool night;           // yalnızca gece (ateş böceği)
        public bool rain;            // yalnızca yağmurlu günde (salyangoz)
    }

    /// <summary>Eşya yükseltmesi (belge 45 §6): yerinde yapılır, malzeme harcar; etkisi eşyaya kalır.</summary>
    [Serializable]
    public sealed class UpgradeDto
    {
        public string id;
        public string displayName;
        public string requires;      // önce yapılması gereken yükseltme (saman → yün → kuş tüyü)
        public string replaces;      // yerine geçtiği yükseltme (aynı kademe zinciri: biri kalır)
        public ConsumeDto[] materials;
        public string skill;         // varsayılan Carpentry
        public int minLevel;
        public double durationDays;  // 0 = 0,1
        public int comfort;
        public int beauty;
        public int storage;
        public float restore;        // ihtiyaç doldurma çarpanı eki (0,1 = %10)
        public float collapse;       // çökme şansı çarpanı (0,5 = yarısı); 0 = etkisiz
        public bool chimney;         // ocağa baca: is %75 az
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
        public int beauty;           // duvar kaplaması: oda güzelliğine katkı (belge 45 §8)
        public string skill;         // açılış becerisi (boş = türüne/grubuna göre)
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
        public bool requiresWithered;    // yalnızca sararmış ekinde (sök)
        public bool stake;               // ekine sırık diker
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
        public int[] bestHours;             // [başlangıç, bitiş] saat: bu aralıkta ilk üründen bestHoursBonus fazla (safran şafakta)
        public int bestHoursBonus;
        public string moodId;               // etkileşim sonunda verilen ruh hali etkisi
        public string moodText;
        public int moodValue;
        public float moodDays;
        public bool requiresLit;            // yalnızca yanan ısı kaynağında
        public bool water;                  // ekini sular
        public float fertilize;             // toprak verimine eklenen (0-1)
        public string fertilizerKind;       // gübre türü (57c): compostRaw, compost, blackSoil, fresh, hot, bone, ash, seaweed, marl, mulch, lime
        public string farmAction;           // pislik, kompost, ahır ve araba işleri (belge 57): LifeSystems.Farmyard
        public bool requiresFruit;          // meyvesi olgun ağaç gerekir
        public bool adultOnly;              // çocuk ve ergen yapamaz (kahve, belge 32-6)
        public bool adultPack;              // +18 paketi (şarap içmek): paket kapalıysa görünmez
        public bool takesFruit;             // meyveyi toplar (ağaç kalır)
        public bool petAnimals;             // barınaktaki hayvanları sever
        public bool slaughter;              // barınaktan bir yetişkin hayvanı keser (et verir)
        public string repair;               // onarım istasyonu: wood, metal, cloth (hasarlı eşyayı onarır)
        public bool polish;                 // kararmış metal aynayı cilalar
        public bool collectContents;        // döküntü yığınındakileri envantere alır (doldurulan dekorda: boşalt)
        public bool fillDecor;              // doldurulan dekoru envanterdeki gerçek ürünle doldurur
        public bool eatContents;            // doldurulan dekordan bir tane yer (açlık)
        public string skillWork;   // beceri işi (belge 45): LifeSystems.OnSkillWorkApplied
        public string chain;       // eylem zinciri kimliği (belge 60); boşsa zincirlerin eşleme kuralı bağlar
        public string cook;        // pişirme yöntemi (63 §2.3): boil, fry, stew, grill, bake, ember, raw
        public string[] needTools; // ek gerekenler: tool:knife, tool:pin, tool:peel, vessel:bake
        public bool washDishes;    // bulaşık yıkar (63a §4)
        public string homeAction;           // ev bakımı (belge 45): candle, strew, whitewash, waterPot, harvestPot, upgrade
        public string upgrade;              // homeAction upgrade: yükseltme kimliği
        public string treeAction;           // ulu ağaç ve ağaç ev (belge 52): found, build, repair, play, lookout, siege, sleep, stash, dig, carve, gather, shade, dine, charity, donate, innToggle
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
        public int quality;    // en az bu kalite (53): elle toplanan zeytin Seçme; 0 = kurala göre
    }
    /// <summary>İzlenen eylem zinciri (belge 60): sıralı adımlar ve hangi etkileşimlere bağlanacağı.</summary>
    [Serializable]
    public sealed class ChainDto
    {
        public string id;
        public ChainMatchDto[] match;   // herhangi biri tutarsa bağlanır; boşsa yalnızca "chain" alanıyla
        public ChainStepDto[] steps;
    }

    [Serializable]
    public sealed class ChainMatchDto
    {
        public string station;    // eşya kimliği
        public string category;   // eşya kategorisi
        public string tag;        // eşya etiketi
        public string need;       // doldurulan ihtiyaç
        public string skill;
        public string flag;       // ChainFlags
        public string idPrefix;   // etkileşim kimliği öneki
    }

    [Serializable]
    public sealed class ChainStepDto
    {
        public string id;
        public string pose;
        public string sfx;
        public string held;       // "consume", "produce" ya da eşya kimliği
        public float share;       // 0 ise 1
        public string when;       // ChainConditions
        public bool undress;
    }
}
