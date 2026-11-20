using Thatchwick.Core.World;

namespace Thatchwick.Core.Audio
{
    /// <summary>
    /// Oyun olayından ses anahtarına (belge 43 ses envanteri). Kararlar burada, çalma Game katmanında
    /// (GameRoot ses bağlantıları). Anahtar null ise o olayın sesi yoktur.
    /// </summary>
    public static class SoundPicker
    {
        /// <summary>Adım sesi: yer altı, döşeme, yol, kum ve hava durumuna göre.</summary>
        /// <param name="weather">0 açık, 1 yağmur, 2 kar, 3 fırtına (WeatherType sırası)</param>
        public static string Step(bool underground, FloorType floor, RoadType road, string regionId, int weather, bool winter)
        {
            if (underground) return SoundKeys.StepMine;
            if (floor == FloorType.Wood) return SoundKeys.StepWood;
            if (floor == FloorType.Stone || road == RoadType.Paved) return SoundKeys.StepStone;
            if (road == RoadType.Gravel) return SoundKeys.StepStone;
            if (weather == 2 || (winter && weather != 1 && weather != 3 && road == RoadType.None && floor == FloorType.Grass)) return SoundKeys.StepSnow;
            if (weather == 1 || weather == 3) return SoundKeys.StepMud;
            if (regionId == "coast") return SoundKeys.StepSand;
            if (road == RoadType.Path || floor == FloorType.Dirt) return SoundKeys.StepDirt;
            return SoundKeys.StepGrass;
        }

        /// <summary>İş vuruşu sesi: iş simgesine (ActivityDescriber) ve etkileşim kimliğine göre.</summary>
        public static string Work(string icon, string interactionId)
        {
            string id = interactionId ?? "";
            if (icon == "chop") return SoundKeys.WorkChop;
            if (icon == "cook") return SoundKeys.WorkCook;
            if (id.Contains("mine") || id.Contains("dig") || id.Contains("quarry")) return SoundKeys.WorkMine;
            if (id.Contains("water")) return SoundKeys.WorkWater;
            if (id.Contains("harvest") || id.Contains("pick") || id.Contains("reap")) return SoundKeys.WorkHarvest;
            if (icon == "farm") return SoundKeys.WorkHoe;
            if (id.Contains("forge") || id.Contains("smith") || id.Contains("hammer") || id.Contains("repair")) return SoundKeys.WorkHammer;
            if (icon == "craft" || icon == "work") return SoundKeys.WorkCraft;
            return null;
        }

        /// <summary>Hayvan sesi: tür kimliğinden (animal.cow ...). Yetişkin erkek tavuk horozdur.</summary>
        public static string Animal(string speciesId, bool adultMale = false)
        {
            switch (speciesId)
            {
                case "animal.chicken": return adultMale ? SoundKeys.AnimalRooster : SoundKeys.AnimalChicken;
                case "animal.cow": return SoundKeys.AnimalCow;
                case "animal.ox": return SoundKeys.AnimalOx;
                case "animal.sheep": case "animal.goat": return SoundKeys.AnimalSheep;
                case "animal.pig": return SoundKeys.AnimalPig;
                case "animal.horse": case "animal.donkey": case "animal.mule": return SoundKeys.AnimalHorse;
                case "animal.dog": return SoundKeys.AnimalDog;
                case "animal.cat": return SoundKeys.AnimalCat;
                default: return null;
            }
        }

        /// <summary>Bildirim sesi: 0 bilgi, 1 uyarı, 2 tehlike (NoticeLevel sırası).</summary>
        public static string Notice(int level) =>
            level >= 2 ? SoundKeys.UiNotifyDanger : level == 1 ? SoundKeys.UiNotifyWarn : SoundKeys.UiNotifyInfo;

        /// <summary>Yapı modu etkinliği (BuildMode.Activity anahtarı) → arayüz sesi.</summary>
        public static string Build(string activity)
        {
            switch (activity)
            {
                case "sell": return SoundKeys.UiSell;
                case "rotate": return SoundKeys.UiRotate;
                default: return SoundKeys.UiPlace;
            }
        }
    }
}
