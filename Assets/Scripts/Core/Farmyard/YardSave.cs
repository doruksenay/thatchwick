using System;

namespace Thatchwick.Core.Farmyard
{
    // Avlu sisteminin kayıt alanları (belge 57-57e). Hepsi eklemeli: eski kayıtta yoktur, boş başlar.

    [Serializable]
    public sealed class YardPileSave
    {
        public int itemId;
        public int kind;
        public double age;
    }

    [Serializable]
    public sealed class YardHeapSave
    {
        public int itemId;
        public double wet, dry, ash, filth, progress, lastTurn, goodDays, totalDays;
        public bool smelly, hot;
        public int readyRaw, readyMature, readyBlack;
        public bool readyFilth;
    }

    [Serializable]
    public sealed class YardTimerSave
    {
        public int itemId;
        public double value;
    }

    [Serializable]
    public sealed class YardCompostSave
    {
        public YardHeapSave[] heaps;
        public YardTimerSave[] timers;
        public bool rawFilth;
        public int dungUnits;
    }

    [Serializable]
    public sealed class YardBarnSave
    {
        public YardTimerSave[] dirt;     // barınak eşyası → kirlilik
        public YardTimerSave[] bank;     // barınak eşyası → temizlikte çıkacak pislik birimi
        public int[] deepLitter;         // derin altlık açık barınaklar
    }

    [Serializable]
    public sealed class YardKeyInt
    {
        public string key;
        public int value;
    }

    [Serializable]
    public sealed class YardHealthSave
    {
        public int[] dirtyHands;
        public int tainted;              // pislikli ham kompostla büyümüş, sofraya gelecek çiğ ürün
        public int mice;
        public int cleanDays, reekDays;
    }

    [Serializable]
    public sealed class YardCartSave
    {
        public string key;               // "item:<id>" ya da "<eşya kimliği>#<sıra>"
        public int assignment;
        public int animalId, driverId;
        public double wear;
    }

    [Serializable]
    public sealed class YardAnimalWorkSave
    {
        public int animalId;
        public long lastDay;
        public int streak;
    }

    [Serializable]
    public sealed class YardFleetSave
    {
        public YardCartSave[] carts;
        public YardAnimalWorkSave[] work;
        public int fertilizeRule;
        public float fertilizeBelow;
    }

    [Serializable]
    public sealed class YardLodgedSave
    {
        public int itemId;
        public float loss;
    }

    [Serializable]
    public sealed class YardSave
    {
        public YardPileSave[] piles;
        public YardCompostSave compost;
        public YardBarnSave barn;
        public YardHealthSave health;
        public YardFleetSave fleet;
        public YardLodgedSave[] lodged;
        public int[] shattered;
        public double spoiledForRefuse;
    }
}
