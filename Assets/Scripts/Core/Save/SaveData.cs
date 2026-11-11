using System;

namespace Thatchwick.Core.Save
{
    // Kayıt dosyasının birebir şekli. Unity JsonUtility uyumu için: genel alanlar, diziler,
    // sözlük ve null'lanabilir tür yok (-1 = yok). Alan eklemek uyumludur (eski dosyada varsayılan
    // değer gelir); alan silmek/anlam değiştirmek için Version artırılır ve SaveMigrator güncellenir (T16).

    [Serializable]
    public sealed class SaveData
    {
        public int version;
        public string savedAtUtc;
        public double totalDays;
        public int speed;
        public MapSave map;
        public ItemSave[] items;
        public HouseholdSave household;
        public CharacterSave[] characters;
        public AccountSave account;
        public float[] temperatures;
        public KeyFloat[] marketDemand;
        public int[] marketTakenOffers;
        public AffectionSave[] affection;
        public DynastySave dynasty;
        /// <summary>Yaşam katmanı (mezar, hava, takvim, köylüler, projeler ...). Eski kayıtta null.</summary>
        public LifeSaveData life;
        public AnimalSave[] animals;     // v2
        public SoilSave[] soil;          // toprak verimi (eski kayıtta yok)
    }

    [Serializable]
    public sealed class AnimalSave
    {
        public int id;
        public string species;
        public int x, y;
        public int home;
        public float fullness;
        public double age, produce, hungryDays;
        public float friendship;
    }

    [Serializable]
    public sealed class MapSave
    {
        public int width;
        public int height;
        /// <summary>Arazi engeli (eşyaların kapattığı karolar hariç), satır satır.</summary>
        public bool[] terrainBlocked;
        public int[] floor;
        public bool[] owned;
        public int[] zone;
        public EdgeSave[] edges;
        public LevelTileSave[] levelTiles;   // üst kat döşemeleri ve bodrum kazıları (zemin kat dizilerde)
    }

    [Serializable]
    public sealed class LevelTileSave { public int x, y, z, floor; }

    [Serializable]
    public sealed class EdgeSave
    {
        public int x, y, axis, kind;
        public int z;                       // kat (0 zemin)
    }

    [Serializable]
    public sealed class ItemSave
    {
        public int id;
        public string def;
        public int x, y, rotation;
        public int z;                       // kat
        public int parentId = -1;
        public string slotId;
        public double fuelDays;
        public double growthDays;
        public bool watered;        // çiftçilik (eski kayıtta yok = false)
        public double ageDays, wetDays;
        public bool fruitReady;
        public double fruitDays;
        public double feedStock;    // v2: barınak yemi
        public KeyInt[] stock;      // v2: barınakta biriken ürünler
    }

    [Serializable]
    public sealed class SoilSave
    {
        public int x, y;
        public float fertility;
        public string lastCrop;
    }

    [Serializable]
    public sealed class HouseholdSave
    {
        public int akce;
        public KeyInt[] inventory;
    }

    [Serializable]
    public sealed class CharacterSave
    {
        public int id;
        public string name;
        public bool isPlayer;
        public int sex;
        public int x, y;
        public int z;                       // kat
        public double ageDays, lifespanDays;
        public bool dead;
        public int deathCause;
        public bool pregnant;
        public int fatherId = -1;
        public double pregnancyLeft, postpartumLeft, freezeDays, starveDays;
        public float hunger, energy, warmth;
        public float fun, hygiene;          // v2
        public KeyFloat[] moodlets;         // v2: (kimlik, kalan gün)
        public KeyFloat[] skills;
        public AppearanceSave appearance;
        // Aile (Adım 16-17). Eksik alan eski kayıtta 0/false gelir: kimlikler 1'den başladığı için 0 = yok.
        public int spouseId;
        public int motherId;
        public int birthFatherId;
        public int generation;
        public bool isVillager;
        // Adım 18a/32: eksik alan eski kayıtta -1 gelir (= tam doygun), diziler boş kalır.
        public float social = -1f;
        public string[] traits;
        public string[] branches;
        public MoodletSave[] extraMoodlets;   // katalog dışı anlık moodlet'ler (şenlik, hediye, oda konforu ...)
    }

    [Serializable]
    public sealed class MoodletSave
    {
        public string id, text;
        public int value;
        public double days;
    }

    [Serializable]
    public sealed class AffectionSave { public int a, b, value; }

    [Serializable]
    public sealed class DynastySave
    {
        public string name;
        public int headId;
        public bool ended;
        public HeadSave[] history;
    }

    [Serializable]
    public sealed class HeadSave
    {
        public int characterId;
        public string name;
        public int generation;
        public double startDay;
        public double endDay = -1;
        public int endCause;
        public string trade;
    }

    [Serializable]
    public sealed class AppearanceSave
    {
        public int build;
        public int heightPercent;
        public string skin, eye, hairStyle, hair, facialHairStyle, facialHair, faceMark, outfit, outfitColor, trim, hat, hatColor;
    }

    [Serializable]
    public sealed class AccountSave
    {
        public int xp;
        public string[] firsts;
        public string[] badges;
        public KeyInt[] counters;
        public long workDay = -1;
        public int workToday;
    }

    [Serializable]
    public sealed class KeyInt { public string key; public int value; }

    [Serializable]
    public sealed class KeyFloat { public string key; public double value; }
}
