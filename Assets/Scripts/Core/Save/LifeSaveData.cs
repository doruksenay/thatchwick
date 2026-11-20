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
        /// <summary>Günlük işlerin (OnNewDay) yapıldığı son gün; eski kayıtta -1.</summary>
        public long lastProcessedDay = -1;
        public long festivalCelebratedDay = -1;
        public SpoilBatchSave[] spoilage;
        // Adım 38-43
        public ToolTierSave[] tools;
        public string[] projectsDone;
        public string[] goalsDone;
        public VillagerSave[] villagers;
        public int marriages, festivalsCelebrated;
        // Karakter yaratma 2.0 (17.11.2026): eski kayıtta 0 = belirsiz (Seyrek sayılır), -1 yerine 0 gelir.
        public int accidentSetting;
        public double lastHouseholdAccidentDay = -1;
        // Miras (17.11.2026): Soy Mührü, yadigârlar, komşu haneye evlendirilenler. Eski kayıtta null: boş başlar.
        public Thatchwick.Core.Family.LegacySave legacy;
        /// <summary>Çocukluk eşyaları, can yoldaşı, çıraklık, aile tarifleri, itibar (belge 30, 31-8, 34). Eski kayıtta null.</summary>
        public Thatchwick.Core.Upbringing.UpbringingSave upbringing;
        /// <summary>Gezgin tüccar: mallar, sipariş (belge 34-D5). Eski kayıtta null.</summary>
        public Thatchwick.Core.Economy.MerchantSave merchant;
        /// <summary>Beceri işlerinin kalıcı bayrakları (belge 45: aşı, stoklu gölet, kara toprak, imece, ırgat). Eski kayıtta null.</summary>
        public string[] skillFlags;
        // Hatıralar ve "ilk"ler (D3b, 18.11.2026). Eski kayıtta null: boş başlar.
        public Thatchwick.Core.Life.KeepsakeSave keepsakes;
        // Ev bakımı ve otomasyonu (belge 45); eski kayıtta null.
        public Thatchwick.Core.Life.HomeSave home;
        // Komşu NPC hanedanları (18.11.2026). Eski kayıtta null: haneler mevcut köylülerden kurulur.
        public NeighborHouseSave[] houses;
        // Yaban hayvanları (18.11.2026, belge 31). Eski kayıtta null / 0: defter boş, tehdit yok.
        public string[] natureJournal;
        public string[] adoptedWild;
        public string[] coffeeHabits;
        public string[] olives;         // zeytin ağaçları (32-7): "itemId:dikimGünü:yadigâr:diken"   // kahve alışkanlığı (belge 32-6): "id:sonGün:seri:alışkanlık"
        public string[] contests;
        public int wolfRaidIn = -1;
        public int wolfHunted;
    }

    [Serializable]
    public sealed class NeighborHouseSave
    {
        public int id;
        public string name;
        public string defId;
        public int headId, founderId;
        public double foundedDay;
        public bool extinct;
        /// <summary>Haneye girmiş herkes (ölüler dahil).</summary>
        public int[] members;
        /// <summary>Şu an bu hanede yaşayanlar (evlenip başka haneye gidenler hariç).</summary>
        public int[] current;
        public HouseHeadSave[] heads;
    }

    [Serializable]
    public sealed class HouseHeadSave
    {
        public int id;
        public string name;
        public double day;
    }
}
