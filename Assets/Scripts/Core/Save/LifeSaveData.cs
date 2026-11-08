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
    public sealed class LifeSaveData
    {
        public GraveSave[] graves;
        public int weatherSeed;
        public long festivalCelebratedDay = -1;
        public SpoilBatchSave[] spoilage;
    }
}
