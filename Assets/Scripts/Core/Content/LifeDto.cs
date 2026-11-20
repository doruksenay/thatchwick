using System;

namespace Thatchwick.Core.Content
{
    // life.json'un birebir şekli (Unity JsonUtility uyumlu: genel alanlar, diziler, [Serializable]).
    // Çarpanlarda 0 ya da eksik = "değişiklik yok" (1 sayılır).

    [Serializable]
    public sealed class SkillRateDto
    {
        public string skill;
        public float rate;
    }

    /// <summary>Özellik, meslek dalı ve köy projesinin karaktere verdiği etkiler.</summary>
    [Serializable]
    public sealed class ModifierDto
    {
        public float hungerDecay, energyDecay, socialDecay, funDecay;
        public float coldResist;
        public float workSpeed;
        public float socialGain;
        public float fertility;
        public float yieldChance;
        public float buildDiscount;
        public float sellBonus;
        public float spoilReduction;
        public float growthSpeed;
        public int moodBase;
        public int lifespanDays;
        public int qualityLevels;
        public SkillRateDto[] skillRates;
    }

    [Serializable]
    public sealed class TraitDto
    {
        public string id;
        public string displayName;
        public string description;
        public string[] conflicts;
        public ModifierDto effects;
    }

    /// <summary>
    /// Beceri seviyesinde açılan şey (belge 45, 9. bölüm): seviye kartında ve beceri panelinin 1-10 şeridinde gösterilir.
    /// planned = tasarımda var, oyunda henüz yok (gösterilir ama "yakında" diye).
    /// </summary>
    [Serializable]
    public sealed class SkillUnlockDto
    {
        public string skill;
        public int level;
        public string kind;        // interaction | recipe | passive | feature
        public string text;
        public bool planned;
        public bool adultPack;
    }

    [Serializable]
    public sealed class BranchDto
    {
        public string id;
        public string displayName;
        public string description;
        public string skill;
        public int level;          // 5 ya da 10
        public string requires;    // ikinci kademe için önceki dal
        public ModifierDto effects;
    }

    [Serializable]
    public sealed class CostDto
    {
        public string item;
        public int count;
    }

    [Serializable]
    public sealed class VillageEffectDto
    {
        public float cropGrowth;       // bitki büyüme hızına eklenir (0,1 = %10)
        public float marketSell;       // pazar satış fiyatına eklenir
        public float spoilReduction;   // bozulmayı azaltır
        public float coldResist;       // herkese soğuk direnci
        public int festivalMood;       // şenlikte moodlet'e eklenir
        public int status;             // "köy durumu" puanı
    }

    [Serializable]
    public sealed class ProjectDto
    {
        public string id;
        public string displayName;
        public string description;
        public int akce;
        public CostDto[] costs;
        public VillageEffectDto effects;
    }

    [Serializable]
    public sealed class VillagerEventDto
    {
        public string id;
        public int minAffection;
        public string text;
        public int rewardAkce;
        public string rewardItem;
        public int rewardCount;
    }

    [Serializable]
    public sealed class VillagerDto
    {
        public string id;
        public string name;
        public string sex;         // "Male" / "Female"
        public int age;            // başlangıç yaşı (oyun günü)
        public string role;        // blacksmith, miller, herbalist, shepherd, ...
        public string roleName;
        public string[] traits;
        public string[] loves;     // eşya kimliği ya da "tag:food" gibi etiket
        public string[] likes;
        public string[] dislikes;
        public string home;        // adlandırılmış yer: "home:1" ... (oyun katmanı karoya çevirir)
        public string work;
        public VillagerEventDto[] events;
        public string[] maleNames;     // hanenin sonraki kuşaklarına verilecek adlar (belge 61b §5)
        public string[] femaleNames;
    }

    /// <summary>Genel ad havuzu: oyuncu hanesi, talipler, adsız komşular (belge 61b §5).</summary>
    [Serializable]
    public sealed class NamePoolDto
    {
        public string[] maleNames;
        public string[] femaleNames;
    }

    [Serializable]
    public sealed class FestivalDto
    {
        public string id;
        public string displayName;
        public string description;
        public string season;      // Spring/Summer/Autumn/Winter
        public int day;            // mevsim içinde 0 tabanlı gün
        public float funRestore;   // şenliğe katılana verilen Eğlence
        public float socialRestore;
        public int moodValue;
        public int extraOffers;    // pazarda ek teklif
    }

    [Serializable]
    public sealed class GoalDto
    {
        public string id;
        public string displayName;
        public string description;
        public string metric;      // generations, living, akce, projects, branches, skill, days, marriages, festivals
        public int target;
        public int rewardAkce;
        public int rewardXp;
    }

    [Serializable]
    public sealed class ToolTierDto
    {
        public string name;
        public int akce;
        public int firewood;
        public float speed;        // 1,2 = %20 hızlı
        public int level;          // gereken hesap seviyesi
    }

    [Serializable]
    public sealed class ToolDto
    {
        public string id;          // axe, hoe, saw
        public string displayName;
        public ToolTierDto[] tiers;
    }

    [Serializable]
    public sealed class TemplateItemDto
    {
        public string item;
        public int x, y, rotation;
    }

    [Serializable]
    public sealed class RoomTemplateDto
    {
        public string id;
        public string displayName;
        public int width, height;
        public string doorSide;    // North/East/South/West
        public int doorOffset;
        public int[] windows;      // kuzeyden güneye/batıdan doğuya kenar sırası: "kuzey duvarında 1. karo" gibi, doorSide'ın karşı duvarında
        public string floor;       // Dirt/Wood/Stone
        public TemplateItemDto[] items;
    }

    // ---- Yaşayan köy, parti 1 (Project belgeleri 66d, 66e, 66f) ----

    /// <summary>Mesleğin işyerinde sırayla yaptığı bir iş (life.json "villagerWork").</summary>
    [Serializable]
    public sealed class VillagerWorkStepDto
    {
        public string id;          // ui.village: "village.work.<id>"
        public string pose;        // oynayacak kare anahtarı (craft_hammer, bellows ...)
        public string fallback;    // pozun karesi yoksa en yakın genel poz
        public double minutes;     // oyun dakikası; 0 = 20
    }

    /// <summary>Meslek → iş döngüsü (kod sabiti değil, veri).</summary>
    [Serializable]
    public sealed class VillagerWorkDto
    {
        public string role;
        public VillagerWorkStepDto[] steps;
    }

    /// <summary>Hane kilerinde tutulmak istenen mal: eşiğin altına inince hanenin bir yetişkini o gün dükkândan alır.</summary>
    [Serializable]
    public sealed class PantryStapleDto
    {
        public string item;
        public int target;         // alımda bu sayıya tamamlanır
        public int min;            // bunun altına inince alım
    }

    /// <summary>Çiftçi, çoban, balıkçı haneleri kendi ürününü her sabah kilere koyar.</summary>
    [Serializable]
    public sealed class PantryProduceDto
    {
        public string role;
        public string item;
        public int count;
    }

    /// <summary>Köylü öğünü: nerede (home | work), hangi zincir (pozlar), kilerden ne düşer, açlığı ne kadar doyurur.</summary>
    [Serializable]
    public sealed class VillageMealDto
    {
        public string id;
        public string where;
        public string chain;
        public ConsumeDto[] consume;
        public float hunger;       // 0 = 60
    }

    /// <summary>Yaşayan köy ayarları (life.json "villageLife"). 0 ya da eksik = varsayılan.</summary>
    [Serializable]
    public sealed class VillageLifeDto
    {
        public double giftChance;          // keyifli ustanın ikram olasılığı (öneri 0,15)
        public double discount;            // ikramın indirim biçimi (0,05)
        public double breakdownMinDays, breakdownMaxDays;   // köylü bunalımı (1-2 gün)
        public double consoleShare;        // teselli: kalan sürenin gidecek payı (0,5)
        public double sharedLossDays;      // "yakın zamanda kayıp" penceresi; 0 = bir mevsim
        public double criticalNeed;        // öğle arasını öne aldıran ihtiyaç eşiği (10)
        public double eveningTavernShare;  // akşam meyhaneye giden yetişkin payı (0,4)
        public int stockFood, stockCooked, stockRaw, stockSeed, stockOther;   // dükkânın günlük temel stoğu
        public int stockStaple;   // köylü kilerinin malları (ekmek, yumurta ...): köylü alımı için daha çok
        public PantryStapleDto[] pantry;
        public PantryProduceDto[] produce;
        public VillageMealDto[] meals;
        public PantryProduceDto[] treats;   // meslek → ikram eşyası (count kullanılmaz)

        public bool IsEmpty => giftChance <= 0 && (pantry == null || pantry.Length == 0) && (meals == null || meals.Length == 0);
    }
}

