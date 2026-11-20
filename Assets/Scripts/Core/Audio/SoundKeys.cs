// Ses anahtarları (Project 45, 45a). Oyun kodu sesi yalnız bu anahtarlarla çağırır.
// Satır sonundaki yorum tools/audio/gaps.py içindir: "// <sıklık A|B|C> <aile>".
// Yeni anahtar eklemek: buraya ekle → python tools/audio/gaps.py --draft → Claude prompt'u yazar.
namespace Thatchwick.Core.Audio
{
    public static class SoundKeys
    {
        // Arayüz
        public const string UiTap = "ui.tap";                 // A ui
        public const string UiOpen = "ui.open";               // B ui
        public const string UiClose = "ui.close";             // B ui
        public const string UiTab = "ui.tab";                 // B ui
        public const string UiCoin = "ui.coin";               // B ui
        public const string UiSpend = "ui.spend";             // B ui
        public const string UiError = "ui.error";             // B ui
        public const string UiNotifyInfo = "ui.notify.info";  // B ui
        public const string UiNotifyWarn = "ui.notify.warn";  // C ui
        public const string UiNotifyDanger = "ui.notify.danger"; // C ui
        public const string UiPlace = "ui.place";             // A ui
        public const string UiRotate = "ui.rotate";           // B ui
        public const string UiSell = "ui.sell";               // B ui

        // Ayak sesleri (zemin anahtarı: "step." + zemin)
        public const string StepGrass = "step.grass";         // A foley
        public const string StepDirt = "step.dirt";           // A foley
        public const string StepStone = "step.stone";         // A foley
        public const string StepWood = "step.wood";           // A foley
        public const string StepSnow = "step.snow";           // A foley
        public const string StepMud = "step.mud";             // A foley
        public const string StepSand = "step.sand";           // A foley
        public const string StepMine = "step.mine";           // A foley

        // İşler (Juice iş vuruşu anı; Project 37)
        public const string WorkChop = "work.chop";           // A tools
        public const string WorkHoe = "work.hoe";             // A tools
        public const string WorkMine = "work.mine";           // A tools
        public const string WorkWater = "work.water";         // A tools
        public const string WorkHarvest = "work.harvest";     // A tools
        public const string WorkCook = "work.cook";           // B craft
        public const string WorkCraft = "work.craft";         // B craft
        public const string WorkHammer = "work.hammer";       // B craft
        public const string TreeFall = "tree.fall";           // C tools
        public const string ShelfCollapse = "shelf.collapse"; // C foley
        public const string CaveIn = "mine.cavein";           // C tools

        // Ev
        public const string DoorOpen = "door.open";           // B foley
        public const string DoorClose = "door.close";         // B foley
        public const string ChestOpen = "chest.open";         // B foley

        // Hayvanlar
        public const string AnimalChicken = "animal.chicken"; // A animals
        public const string AnimalRooster = "animal.rooster"; // C animals
        public const string AnimalCow = "animal.cow";         // B animals
        public const string AnimalOx = "animal.ox";           // B animals
        public const string AnimalSheep = "animal.sheep";     // B animals
        public const string AnimalPig = "animal.pig";         // B animals
        public const string AnimalHorse = "animal.horse";     // B animals
        public const string AnimalDog = "animal.dog";         // B animals
        public const string AnimalCat = "animal.cat";         // B animals
        public const string AnimalWolf = "animal.wolf";       // C animals

        // Ortam yatakları ve nokta sesleri
        public const string AmbFarmDay = "amb.farm.day";      // C ambience
        public const string AmbFarmNight = "amb.farm.night";  // C ambience
        public const string AmbTown = "amb.town";             // C ambience
        public const string AmbInterior = "amb.interior";     // C ambience
        public const string AmbMine = "amb.mine";             // C ambience
        public const string WeatherRain = "weather.rain";     // C weather
        public const string WeatherStorm = "weather.storm";   // C weather
        public const string WeatherThunder = "weather.thunder"; // C spots
        public const string SpotBirdSong = "spot.bird.song";  // B spots
        public const string AmbCoast = "amb.coast";           // C ambience (deniz kıyısı, belge 50)
        public const string SpotGull = "spot.gull";           // B spots
        public const string SpotWave = "spot.wave";           // B spots

        // Balık tutma mini oyunu ve kavanoz (belge 50)
        public const string FishCast = "fish.cast";           // B tools
        public const string FishBite = "fish.bite";           // B foley
        public const string FishReel = "fish.reel";           // A tools
        public const string FishCatch = "fish.catch";         // B foley
        public const string FishEscape = "fish.escape";       // C foley
        public const string JarScoop = "jar.scoop";           // B foley

        // Olay vuruşları
        public const string StingLevelUp = "sting.levelup";   // C stinger
        public const string StingBirth = "sting.birth";       // C stinger
        public const string StingWedding = "sting.wedding";   // C stinger
        public const string StingDeath = "sting.death";       // C stinger
        public const string StingDayEnd = "sting.dayend";     // C stinger

        /// <summary>Zemin adından ayak sesi anahtarı.</summary>
        public static string Step(string ground) => "step." + ground;

        /// <summary>Karakter sesi anahtarları: voice.&lt;ses&gt;.&lt;durum&gt;; hece bankası voice.&lt;ses&gt;.syl</summary>
        public static string Voice(string voice, string state) => "voice." + voice + "." + state;
        public static string VoiceSyllables(string voice) => "voice." + voice + ".syl";
    }
}
