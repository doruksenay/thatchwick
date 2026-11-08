using System;

namespace Thatchwick.Core.Save
{
    // Yaşam katmanı kaydı (Adım 18-43). SaveData.life altında tek paket: alan eklemek uyumludur,
    // eski kayıtta paket null gelir ve her sistem varsayılan durumla başlar.

    [Serializable]
    public sealed class GraveSave
    {
        public int characterId, generation, sex, cause, itemId, x, y;
        public string name;
        public double deathDay, ageDays;
    }

    [Serializable]
    public sealed class SpoilBatchSave
    {
        public string item;
        public int count;
        public double age;
    }

    [Serializable]
    public sealed class ToolTierSave
    {
        public string tool;
        public int tier;
    }

    [Serializable]
    public sealed class VillagerSave
    {
        public string id;
        public int characterId, romance;
        public long lastGiftDay = -1;
        public string[] events;
    }

    [Serializable]
    public sealed class LifeSaveData
    {
        public GraveSave[] graves;
        public int weatherSeed;
        public long festivalCelebratedDay = -1;
        public SpoilBatchSave[] spoilage;
        // Adım 38-43
        public ToolTierSave[] tools;
        public string[] projectsDone;
        public string[] goalsDone;
        public VillagerSave[] villagers;
        public int marriages, festivalsCelebrated;
    }
}
