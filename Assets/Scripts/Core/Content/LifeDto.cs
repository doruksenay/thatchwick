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
}
