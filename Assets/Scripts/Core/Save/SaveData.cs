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
    }

    [Serializable]
    public sealed class EdgeSave
    {
        public int x, y, axis, kind;
    }

    [Serializable]
    public sealed class ItemSave
    {
        public int id;
        public string def;
        public int x, y, rotation;
        public int parentId = -1;
        public string slotId;
        public double fuelDays;
        public double growthDays;
        /// <summary>Üreten eşyanın hazırlanma ilerlemesi (kümes).</summary>
        public double readyProgress;
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
        public double ageDays, lifespanDays;
        public bool dead;
        public int deathCause;
        public bool pregnant;
        public int fatherId = -1;
        public double pregnancyLeft, postpartumLeft, freezeDays, starveDays;
        public float hunger, energy, warmth;
        public KeyFloat[] skills;
        public AppearanceSave appearance;
        // Aile (Adım 16-17). Eksik alan eski kayıtta 0/false gelir: kimlikler 1'den başladığı için 0 = yok.
        public int spouseId;
        public int motherId;
        public int birthFatherId;
        public int generation;
        public bool isVillager;
        // Adım 18a/32: eksik alan eski kayıtta -1 gelir (= tam doygun), diziler boş kalır.
        public float social = -1f, fun = -1f;
        public string[] traits;
        public string[] branches;
        public MoodletSave[] moodlets;
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
