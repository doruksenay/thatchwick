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
        public GrassSave[] pasture;      // yenmiş otlak (eski kayıtta yok)
        public bool animalsHeld;         // hayvan çanı: barınakta tutuluyorlar (eski kayıtta false = salınmış)
        /// <summary>Oyunun ana rastgelelik tohumu (eski kayıtta 0) ve yükleme sayacı (bkz. Core.Seeds).</summary>
        public int seed;
        public int rngEpoch;
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
        public double petCooldown;       // sevme bekleme süresi (eski kayıtta 0)
        public string saddle;            // takılı eyer (eski kayıtta yok)
        public float comfort = -1f;      // barınak konforu (eski kayıtta -1 = varsayılan)
        public bool sick;                // hasta (eski kayıtta false)
        public double sickDays;          // kaç gündür hasta
        public int coldDays;             // art arda üşüdüğü gün
        public int sex;                  // 1 dişi, 2 erkek (eski kayıtta 0: kimliğin tekliğinden)
        public double pregnant;          // doğuma kalan gün (0 = gebe değil)
        public int mother;               // annenin kimliği (0 = yok)
        public int generation;           // kuşak (satın alınan 0)
    }

    /// <summary>Yenmiş otlak karosu (dolu karolar kaydedilmez).</summary>
    [Serializable]
    public sealed class GrassSave
    {
        public int x, y;
        public float grass;
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
        public TileStyleSave[] styles;       // zemin ve çatı stili (yalnızca varsayılan olmayan karolar; eski kayıtta yok)
        public RoadSave[] roads;             // yollar, patikalar ve çimdeki izler (eski kayıtta yok)
        public ConduitSave[] conduits;       // su yolları: ark, oluk, boru (eski kayıtta yok)
        public PondSave[] ponds;             // göletler: su seviyesi, kaynak, böcek, balıklar (eski kayıtta yok)
    }

    /// <summary>Bir gölet (x,y: göletin bir karosu).</summary>
    [Serializable]
    public sealed class PondSave { public int x, y; public float level; public bool spring, insects; public double filledDays; public string[] fishIds; public int[] fishCounts; }

    [Serializable]
    public sealed class RoadSave { public int x, y, type; public float cond; public bool natural; public float trample; }

    [Serializable]
    public sealed class ConduitSave { public int x, y, type; }

    [Serializable]
    public sealed class TileStyleSave { public int x, y, z; public string floor; public string roof; }

    [Serializable]
    public sealed class LevelTileSave { public int x, y, z, floor; public int zone; }   // zone: depo bölgesi (eski kayıtta 0 = yok)

    [Serializable]
    public sealed class EdgeSave
    {
        public int x, y, axis, kind;
        public int z;                       // kat (0 zemin)
        public string style;                // malzeme ya da kapı/pencere/çit stili (boş = varsayılan)
        public string finishA, finishB;     // duvar yüzü kaplamaları (boş = çıplak)
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
        public string finish;       // mobilya kaplaması (renk/malzeme; boş = varsayılan)
        public bool toggled;        // su kapağı kapalı (eski kayıtta false)
        public int fill;            // lazımlık/hela dolumu
        public KeyInt[] stock;      // v2: barınakta biriken ürünler
        // Ev dekorasyonu (27). Eski kayıtta: bant -1 (eşyanın varsayılanı), sıra 0, sağlam, kararmamış, boş.
        public int band = -1;       // duvar bandı (0 alt, 1 orta, 2 üst)
        public int layer;           // halı sırası (büyük olan üstte)
        public int condition;       // 0 sağlam, 1 hasarlı, 2 onarılmış
        public double tarnish;      // metal aynanın kararma günü
        public string[] contents;   // döküntü yığını: "kimlik" ya da "kimlik|durum"
    }

    [Serializable]
    public sealed class SoilSave
    {
        public int x, y;
        public float fertility;
        public string lastCrop;
        public string prevCrop;     // ondan önceki ekin (üç tarla nöbeti; eski kayıtta yok)
    }

    [Serializable]
    public sealed class HouseholdSave
    {
        public int akce;
        public KeyInt[] inventory;
        public KeyInt[] damaged;    // envanterdeki hasarlı adetler (27; eski kayıtta yok)
        public KeyInt[] repaired;   // onarılmış adetler
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
        public float bladder = -1f;         // tuvalet (eski kayıtta -1 = rahat)
        public float wet;                   // ıslaklık (eski kayıtta 0)
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
        public int duty;                      // hane görevi (DutyKind; eski kayıtta 0 = yok)
        // Karakter yaratma 2.0 (17.11.2026). Eksik alanlar eski kayıtta varsayılanla gelir.
        public int birthSeason = -1;
        public int birthDay = -1;
        public string likedFood, likedColor, likedHobby, dislikedFood;
        public bool freeMirror;
        public InjurySave injury;
        public double lastAccidentDay = -1;
        // Ruh hali davranışı (18.11.2026). Eski kayıtta: bunalım yok, yetişme ölçülmemiş, not yok.
        public int breakdown;                 // BreakdownKind (0 = yok)
        public double breakdownLeft;
        public double miserableDays;
        public double lastBreakdownAge = -1;
        public double upbringingSum, upbringingDays;
        public string[] chronicle;            // hayat notları ("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.")
    }

    [Serializable]
    public sealed class InjurySave
    {
        public string kind, zone, pattern, reason;
        public double startDay, healDay;
        public bool deep, treated;
    }

    [Serializable]
    public sealed class MarkSave
    {
        public int kind;
        public string id, zone, color, reason;
        public int fade, source;
        public double day = -1;
        public bool hidden;
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
        public string settlement;            // yerleşim adı (eski kayıtta yok: varsayılan)
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
        // Karakter yaratma 2.0 (17.11.2026). null alan = eski kayıt (yeni varsayılan ya da eski alandan çevrilir).
        public int frame = -1;
        public string faceShape, eyeShape, eyeRight, brow, browColor, nose, mouth, ears;
        public int browOwn;                    // 0 = kaş saçı izler (eski kayıt dahil), 1 = ayrı renk
        public string bangs, mustache, beard;  // null ise facialHairStyle'dan çevrilir
        public string pattern, bottom, shoes, outer, outerColor;
        public int voice = -1;
        public int voicePitch;
        public string laugh;
        public MarkSave[] marks;               // null ise faceMark'tan çevrilir
        public string[] genome;                // gizli kalıtım: "gen|A|B"
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
