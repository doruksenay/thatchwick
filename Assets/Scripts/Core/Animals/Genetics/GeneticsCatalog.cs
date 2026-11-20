using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>
    /// Genetik verisi (Project belgesi 48): köpek, kedi, at, eşek, katır, bardo. Görünüm lokusları sadeleştirilmiş gerçek
    /// genetik (köpekte E, K, A, B, D, S; kedide X'e bağlı kızıl O; atta E, A, krem, kır). Merle bilinçli olarak yok (§4.4).
    /// Adlar arayüz tablosunda: <c>breed.&lt;tip&gt;</c>, <c>gift.*.name</c>, <c>gene.*</c>.
    /// </summary>
    public static class GeneticsCatalog
    {
        public const string Dog = "animal.dog", Cat = "animal.cat", Horse = "animal.horse", Donkey = "animal.donkey",
            Mule = "animal.mule", Hinny = "animal.hinny";
        /// <summary>Binek ailesi (hüner havuzu ortak).</summary>
        public const string Equine = "equine";

        public static bool IsEquine(string species) =>
            species == Horse || species == Donkey || species == Mule || species == Hinny;

        public static bool HasGenetics(string species) =>
            species == Dog || species == Cat || IsEquine(species);

        // ---------------- lokuslar ----------------
        private static LocusDef L(string id, string[] alleles, double[] weights, bool xLinked = false, bool dosage = false) =>
            new LocusDef { Id = id, Alleles = alleles, Weights = weights, XLinked = xLinked, Dosage = dosage };

        private static readonly LocusDef[] DogLoci =
        {
            L("E", new[] { "Em", "E", "e" }, new[] { 0.25, 0.55, 0.20 }),      // maske > normal > kırmızı-krem
            L("K", new[] { "KB", "kbr", "ky" }, new[] { 0.20, 0.10, 0.70 }),   // baskın siyah > kaplan > A'ya bırak
            L("A", new[] { "ay", "at" }, new[] { 0.5, 0.5 }),                   // kum > iki renkli
            L("B", new[] { "B", "b" }, new[] { 0.8, 0.2 }),                     // siyah > karaciğer
            L("D", new[] { "D", "d" }, new[] { 0.85, 0.15 }),                   // koyu > seyreltik (mavi)
            L("S", new[] { "S", "sp" }, new[] { 0.6, 0.4 }),                    // beyazsız > beyaz leke
            L("L", new[] { "Lc", "l" }, new[] { 0.6, 0.4 }),                    // kısa > uzun tüy
        };

        private static readonly LocusDef[] CatLoci =
        {
            L("O", new[] { "O", "o" }, new[] { 0.3, 0.7 }, xLinked: true),      // kızıl (X kromozomunda)
            L("A", new[] { "A", "a" }, new[] { 0.6, 0.4 }),                     // tekir > düz
            L("Tb", new[] { "Mc", "Sp", "mc" }, new[] { 0.55, 0.15, 0.30 }),    // uskumru > benekli > klasik
            L("B", new[] { "B", "b" }, new[] { 0.9, 0.1 }),                     // siyah > çikolata
            L("D", new[] { "D", "d" }, new[] { 0.8, 0.2 }),                     // koyu > seyreltik
            L("S", new[] { "S", "s" }, new[] { 0.35, 0.65 }, dosage: true),     // beyaz leke dozu 0-2
            L("W", new[] { "W", "w" }, new[] { 0.04, 0.96 }),                   // baskın beyaz
            L("L", new[] { "L", "l" }, new[] { 0.75, 0.25 }),                   // kısa > uzun
        };

        private static readonly LocusDef[] HorseLoci =
        {
            L("E", new[] { "E", "e" }, new[] { 0.6, 0.4 }),                     // siyah pigment var > al (kızıl)
            L("A", new[] { "A", "a" }, new[] { 0.6, 0.4 }),                     // doru > yağız
            L("Cr", new[] { "Cr", "n" }, new[] { 0.12, 0.88 }, dosage: true),   // krem: tek kopya kula, iki kopya krem
            L("G", new[] { "G", "g" }, new[] { 0.12, 0.88 }),                   // kır: yaşla ağarır
            L("Rn", new[] { "Rn", "rn" }, new[] { 0.08, 0.92 }),                // kırçıl
            L("W", new[] { "Wm", "w" }, new[] { 0.3, 0.7 }, dosage: true),      // beyaz işaret: sakar/seki, iki kopya alaca
        };

        private static readonly LocusDef[] DonkeyLoci =
        {
            L("C", new[] { "Gy", "Br", "Bk" }, new[] { 0.6, 0.25, 0.15 }),     // kurşuni > kahve > kara
            L("Sp", new[] { "Sp", "sp" }, new[] { 0.05, 0.95 }),                // benekli
        };

        public static IReadOnlyList<LocusDef> Loci(string species)
        {
            switch (species)
            {
                case Dog: return DogLoci;
                case Cat: return CatLoci;
                case Horse: case Mule: case Hinny: return HorseLoci;   // melezin rengi at ebeveyninden
                case Donkey: return DonkeyLoci;
                default: return Array.Empty<LocusDef>();
            }
        }

        public static LocusDef Locus(string species, string id)
        {
            foreach (var l in Loci(species)) if (string.Equals(l.Id, id, StringComparison.Ordinal)) return l;
            return null;
        }

        // ---------------- istatistikler ----------------
        private static readonly string[] DogStats = { GeneStat.Speed, GeneStat.Nose, GeneStat.Courage, GeneStat.Stamina, GeneStat.Temper, GeneStat.Size };
        private static readonly string[] CatStats = { GeneStat.Hunting, GeneStat.Agility, GeneStat.Temper, GeneStat.Curiosity, GeneStat.Health };
        private static readonly string[] EquineStats = { GeneStat.Speed, GeneStat.Stamina, GeneStat.Carry, GeneStat.Draft, GeneStat.Gait, GeneStat.Temper };

        public static IReadOnlyList<string> Stats(string species) =>
            species == Dog ? DogStats : species == Cat ? CatStats : IsEquine(species) ? EquineStats : (IReadOnlyList<string>)Array.Empty<string>();

        // ---------------- tipler ----------------
        private static BreedTypeDef T(string id, string species, string body, string stats, string bias = null,
            bool market = true, double weight = 1, bool cross = false, bool child = false, int bag = 0)
        {
            var t = new BreedTypeDef { Id = id, Species = species, Body = body, Market = market, MarketWeight = weight, Cross = cross, ChildMount = child, SaddleBagExtra = bag };
            foreach (var part in stats.Split(' '))
            {
                var kv = part.Split('=');
                t.Stats[kv[0]] = int.Parse(kv[1]);
            }
            if (bias != null)
                foreach (var part in bias.Split(' '))
                {
                    var kv = part.Split('=');
                    t.LookBias[kv[0]] = double.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture);
                }
            return t;
        }

        /// <summary>Bütün tipler. Kimlik sırası kalıcıdır (kayıtta tip kimliği yazılır).</summary>
        public static readonly BreedTypeDef[] Types =
        {
            // Köpek (belge 48 §4.1): dönemde köpekler işle tanımlanırdı (Master of Game).
            T("dog.shepherd", Dog, "dog_shepherd", "speed=5 nose=5 courage=8 stamina=7 temper=6 size=7", "E:Em=3 A:ay=1.5 L:l=1.5", weight: 3),
            T("dog.sighthound", Dog, "dog_sighthound", "speed=9 nose=4 courage=5 stamina=6 temper=6 size=6", "L:Lc=3 S:sp=1.5"),
            T("dog.scenthound", Dog, "dog_scenthound", "speed=6 nose=8 courage=6 stamina=8 temper=5 size=5", "A:at=2 S:sp=2"),
            T("dog.tracker", Dog, "dog_tracker", "speed=4 nose=9 courage=5 stamina=6 temper=8 size=6", "A:at=2 L:Lc=2", weight: 0.7),
            T("dog.mastiff", Dog, "dog_mastiff", "speed=3 nose=4 courage=9 stamina=6 temper=5 size=9", "E:Em=3 K:kbr=2 L:Lc=2"),
            T("dog.waterdog", Dog, "dog_waterdog", "speed=5 nose=7 courage=6 stamina=7 temper=7 size=5", "L:l=3 B:b=2", weight: 0.7),
            T("dog.lapdog", Dog, "dog_lapdog", "speed=3 nose=4 courage=3 stamina=4 temper=9 size=2", "L:l=2 S:sp=2", weight: 0.5),
            // Adlı kırmalar (§4.2)
            T("dog.lurcher", Dog, "dog_sighthound", "speed=8 nose=6 courage=5 stamina=7 temper=6 size=6", market: false, cross: true),
            T("dog.alaunt", Dog, "dog_mastiff", "speed=6 nose=5 courage=9 stamina=7 temper=4 size=8", market: false, cross: true),
            T("dog.wolfbreaker", Dog, "dog_shepherd", "speed=4 nose=5 courage=10 stamina=7 temper=5 size=9", market: false, cross: true),
            T("dog.laketracker", Dog, "dog_waterdog", "speed=5 nose=9 courage=5 stamina=7 temper=8 size=5", market: false, cross: true),

            // Kedi (§5.1): ortaçağda ırk yok; tip gövde ve iştir.
            T("cat.barn", Cat, "cat_medium", "hunting=7 agility=6 temper=5 curiosity=5 health=6", "A:A=1.5", weight: 3),
            T("cat.forest", Cat, "cat_large", "hunting=8 agility=6 temper=4 curiosity=5 health=8", "L:l=3 A:A=2"),
            T("cat.shore", Cat, "cat_slim", "hunting=6 agility=8 temper=6 curiosity=7 health=6", "L:L=2"),
            T("cat.van", Cat, "cat_large", "hunting=6 agility=6 temper=6 curiosity=6 health=7", "S:S=4 L:l=1.5", weight: 0.5),
            T("cat.angora", Cat, "cat_slim", "hunting=3 agility=6 temper=8 curiosity=5 health=5", "W:W=6 L:l=4", weight: 0.4),
            T("cat.library", Cat, "cat_medium", "hunting=6 agility=5 temper=8 curiosity=4 health=6", "L:L=2", weight: 0.8),

            // At (§7.1): dönemin iş tipleri.
            T("horse.rouncey", Horse, "horse_medium", "speed=5 stamina=5 carry=5 draft=5 gait=4 temper=6", weight: 3),
            T("horse.palfrey", Horse, "horse_light", "speed=5 stamina=7 carry=4 draft=3 gait=9 temper=7"),
            T("horse.courser", Horse, "horse_light", "speed=8 stamina=6 carry=3 draft=3 gait=5 temper=5", weight: 0.6),
            T("horse.carthorse", Horse, "horse_heavy", "speed=3 stamina=7 carry=5 draft=9 gait=4 temper=7"),
            T("horse.sumpter", Horse, "horse_heavy", "speed=3 stamina=8 carry=9 draft=6 gait=4 temper=7", bag: 4),
            T("horse.pony", Horse, "pony", "speed=3 stamina=6 carry=3 draft=3 gait=6 temper=8", weight: 0.8, child: true),
            T("horse.crossbred", Horse, "horse_medium", "speed=5 stamina=6 carry=5 draft=5 gait=5 temper=6", market: false, cross: true),

            // Eşek ve melezler (§7.3-7.4)
            T("donkey.common", Donkey, "donkey", "speed=2 stamina=8 carry=7 draft=5 gait=5 temper=7", child: true, bag: 2),
            T("mule.common", Mule, "mule", "speed=4 stamina=9 carry=9 draft=7 gait=5 temper=6", market: false, bag: 8),
            T("hinny.common", Hinny, "hinny", "speed=3 stamina=8 carry=7 draft=6 gait=6 temper=8", market: false, bag: 4),
        };

        public static BreedTypeDef Type(string id)
        {
            if (id == null) return null;
            foreach (var t in Types) if (string.Equals(t.Id, id, StringComparison.Ordinal)) return t;
            return null;
        }

        public static IEnumerable<BreedTypeDef> TypesOf(string species)
        {
            foreach (var t in Types) if (t.Species == species) yield return t;
        }

        /// <summary>Türün varsayılan tipi (eski kayıt, satın alma).</summary>
        public static BreedTypeDef DefaultType(string species)
        {
            foreach (var t in Types) if (t.Species == species) return t;
            return null;
        }

        public static readonly NamedCross[] NamedCrosses =
        {
            new NamedCross { A = "dog.sighthound", B = "dog.scenthound", Result = "dog.lurcher" },
            new NamedCross { A = "dog.sighthound", B = "dog.mastiff", Result = "dog.alaunt" },
            new NamedCross { A = "dog.shepherd", B = "dog.mastiff", Result = "dog.wolfbreaker" },
            new NamedCross { A = "dog.tracker", B = "dog.waterdog", Result = "dog.laketracker" },
        };

        /// <summary>Adlı kırma doğma şansı (§4.2).</summary>
        public const double NamedCrossChance = 0.35;
        /// <summary>Farklı at tiplerinden "kırma at" doğma şansı (§7.1).</summary>
        public const double HorseCrossbredChance = 0.35;

        public static readonly SpeciesCross[] SpeciesCrosses =
        {
            new SpeciesCross { FemaleSpecies = Horse, MaleSpecies = Donkey, Offspring = Mule, ChanceFactor = 0.6 },
            new SpeciesCross { FemaleSpecies = Donkey, MaleSpecies = Horse, Offspring = Hinny, ChanceFactor = 0.3 },   // bardo daha nadir
        };

        public static SpeciesCross CrossFor(string femaleSpecies, string maleSpecies)
        {
            foreach (var c in SpeciesCrosses)
                if (c.FemaleSpecies == femaleSpecies && c.MaleSpecies == maleSpecies) return c;
            return null;
        }

        /// <summary>Kısır melez (katır, bardo). Dişi katırda nadir "mucize tay" (§7.4).</summary>
        public static bool IsSterileHybrid(string species) => species == Mule || species == Hinny;
        public const double MiracleFoalChance = 0.005;

        // ---------------- hünerler (§3.3, §4.3, §5.3) ----------------
        private static GiftDef G(string id, string species, GiftRarity r, params string[] types) =>
            new GiftDef { Id = id, Species = species, Rarity = r, Types = types.Length == 0 ? null : types };

        public static readonly GiftDef[] Gifts =
        {
            // Köpek
            G(Gift.SoftMouth, Dog, GiftRarity.Common),
            G(Gift.LongBreathDog, Dog, GiftRarity.Common),
            G(Gift.NightEye, Dog, GiftRarity.Rare),
            G(Gift.ChildFriend, Dog, GiftRarity.Rare),
            G(Gift.MushroomNose, Dog, GiftRarity.Choice, "dog.tracker", "dog.scenthound", "dog.waterdog", "dog.laketracker", "dog.lurcher"),
            G(Gift.BornLeader, Dog, GiftRarity.Choice, "dog.shepherd", "dog.mastiff", "dog.wolfbreaker", "dog.alaunt"),
            G(Gift.TreasureNose, Dog, GiftRarity.Legendary),
            G(Gift.Pathfinder, Dog, GiftRarity.Legendary),
            // Kedi
            G(Gift.SharpClaw, Cat, GiftRarity.Common),
            G(Gift.HearthCat, Cat, GiftRarity.Common),
            G(Gift.FisherPaw, Cat, GiftRarity.Rare),
            G(Gift.SilentStep, Cat, GiftRarity.Rare),
            G(Gift.WeatherSense, Cat, GiftRarity.Choice),
            G(Gift.HealingPurr, Cat, GiftRarity.Choice),
            G(Gift.NineLives, Cat, GiftRarity.Legendary),
            G(Gift.HouseSpirit, Cat, GiftRarity.Legendary),
            // Binek (at, eşek, katır, bardo)
            G(Gift.SureFoot, Equine, GiftRarity.Common),
            G(Gift.LongBreathHorse, Equine, GiftRarity.Common),
            G(Gift.SoftGait, Equine, GiftRarity.Rare),
            G(Gift.RaceHeart, Equine, GiftRarity.Rare),
            G(Gift.IronBack, Equine, GiftRarity.Choice),
            G(Gift.Calm, Equine, GiftRarity.Choice),
            G(Gift.WindBorn, Equine, GiftRarity.Legendary),
            G(Gift.Homing, Equine, GiftRarity.Legendary),
        };

        public static GiftDef GiftById(string id)
        {
            foreach (var g in Gifts) if (string.Equals(g.Id, id, StringComparison.Ordinal)) return g;
            return null;
        }

        public static IEnumerable<GiftDef> GiftsFor(string species, string typeId)
        {
            string pool = IsEquine(species) ? Equine : species;
            foreach (var g in Gifts) if (g.Species == pool && g.AllowedFor(typeId)) yield return g;
        }

        // ---------------- nadir renk (mutasyon, §3.2) ----------------
        public static string[] RareCoats(string species)
        {
            switch (species)
            {
                case Dog: return new[] { "rare.silver" };
                case Cat: return new[] { "rare.smoke", "rare.golden" };
                case Donkey: return new[] { "rare.white_donkey" };
                default: return IsEquine(species) ? new[] { "rare.pearl" } : Array.Empty<string>();
            }
        }
    }

    /// <summary>Hüner kimlikleri (oyun etkileri bunlara bakar).</summary>
    public static class Gift
    {
        public const string SoftMouth = "gift.dog.soft_mouth";          // avda +1 et
        public const string LongBreathDog = "gift.dog.long_breath";     // kurtla boğuşmada yaralanma yarıya
        public const string NightEye = "gift.dog.night_eye";            // kurt savunması +1
        public const string ChildFriend = "gift.dog.child_friend";      // can yoldaşı bağı x1,5
        public const string MushroomNose = "gift.dog.mushroom_nose";    // günlük mantar buluşu
        public const string BornLeader = "gift.dog.born_leader";        // 3. seviyede kurt başı
        public const string TreasureNose = "gift.dog.treasure_nose";    // haftada bir eski eşya
        public const string Pathfinder = "gift.dog.pathfinder";         // avda isabet +%15, yaralı av kaçmaz

        public const string SharpClaw = "gift.cat.sharp_claw";          // ambar faresi etkisi −%15 → −%22
        public const string HearthCat = "gift.cat.hearth_cat";          // sevince ek keyif
        public const string FisherPaw = "gift.cat.fisher_paw";          // günlük küçük balık
        public const string SilentStep = "gift.cat.silent_step";        // can yoldaşı bağı x1,5
        public const string WeatherSense = "gift.cat.weather_sense";    // fırtınadan bir gün önce haber
        public const string HealingPurr = "gift.cat.healing_purr";      // yaralı hane üyesi daha hızlı iyileşir
        public const string NineLives = "gift.cat.nine_lives";          // ömür +%40
        public const string HouseSpirit = "gift.cat.house_spirit";      // haneye her gün küçük keyif

        public const string SureFoot = "gift.eq.sure_foot";             // atlama +10 puan
        public const string LongBreathHorse = "gift.eq.long_breath";    // dörtnala dayanıklılığı x0,85
        public const string SoftGait = "gift.eq.soft_gait";             // binene "Rahat yol"
        public const string RaceHeart = "gift.eq.race_heart";           // yarışma puanı +%10
        public const string IronBack = "gift.eq.iron_back";             // heybe +4
        public const string Calm = "gift.eq.calm";                      // ürkek kalma süresi yarıya
        public const string WindBorn = "gift.eq.wind_born";             // binek hızı x1,08
        public const string Homing = "gift.eq.homing";                  // dostluk şartsız ıslıkla gelir
    }
}
