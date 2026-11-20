using System;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Animals
{
    /// <summary>Hayvanın soy kademesi (karar 18.11.2026, Project belgesi 29). Efsane doğuştan gelmez, kazanılır.</summary>
    public enum AnimalLineage
    {
        /// <summary>Köy: pazar, ucuz; en çok 3. seviye.</summary>
        Village = 0,
        /// <summary>İyi soy: kasaba tüccarı, üreme; en çok 4. seviye.</summary>
        Good = 1,
        /// <summary>Soylu: yalnızca üreme ya da panayır; en çok 5. seviye.</summary>
        Noble = 2,
        /// <summary>Efsane: Soylu ve 5. seviye bir hayvan bir başarıyla kazanır (unvan).</summary>
        Legendary = 3
    }

    /// <summary>
    /// Hayvan soyu ve seviyesi (1-5): XP kullanım ve bakımla gelir, tavanı soy belirler. Ürün kalitesi soy + seviye + dostluk/konfor.
    /// </summary>
    public static class AnimalProgress
    {
        public const int MaxLevel = 5;
        /// <summary>Seviye eşikleri (toplam XP): 1:0, 2:20, 3:60, 4:140, 5:280.</summary>
        private static readonly double[] Thresholds = { 0, 20, 60, 140, 280 };

        // XP kaynakları
        public const double XpPet = 2, XpBrush = 3, XpProduce = 1, XpPerRideTile = 0.5, XpJump = 5, XpRace = 40;

        // Kalıtım
        public const double LineageUpChance = 0.15, LineageDownChance = 0.10, LegendaryDescentBonus = 0.20;

        public static string LineageName(AnimalLineage l)
        {
            switch (l)
            {
                case AnimalLineage.Good: return "İyi soy";
                case AnimalLineage.Noble: return "Soylu";
                case AnimalLineage.Legendary: return "Efsane";
                default: return "Köy";
            }
        }

        public static int MaxLevelFor(AnimalLineage l) => l == AnimalLineage.Village ? 3 : l == AnimalLineage.Good ? 4 : MaxLevel;

        public static double XpForLevel(int level) => Thresholds[Math.Max(1, Math.Min(MaxLevel, level)) - 1];

        public static int LevelForXp(double xp, AnimalLineage lineage)
        {
            int cap = MaxLevelFor(lineage), level = 1;
            while (level < cap && xp >= Thresholds[level]) level++;
            return level;
        }

        /// <summary>XP ekler; soyun tavanını aşmaz. Seviye atladıysa yeni seviyeyi, yoksa 0 döner.</summary>
        public static int AddXp(Animal a, double xp)
        {
            if (a == null || a.IsDead || xp <= 0) return 0;
            int before = a.Level;
            double ceiling = XpForLevel(MaxLevelFor(a.Lineage));
            a.Xp = Math.Min(ceiling, a.Xp + xp * Genetics.GeneticsService.XpFactor(a.Genome));   // çekingen (akrabalık kusuru) −%20
            int after = a.Level;
            return after > before ? after : 0;
        }

        /// <summary>Kalite puanına eklenen: soy (Köy 0, İyi +5, Soylu +10, Efsane +15) ve seviye başına +2.</summary>
        public static float QualityBonus(AnimalLineage l, int level) => 5f * (int)l + 2f * Math.Max(0, level - 1);

        /// <summary>Satış ve damızlık değeri çarpanı: soy ve seviyeyle katlanır.</summary>
        public static double ValueFactor(AnimalLineage l, int level) => Math.Pow(1.6, (int)l) * (1.0 + 0.15 * Math.Max(0, level - 1));

        /// <summary>
        /// Yavrunun soyu: ebeveynlerin ortalaması (Efsane, Soylu sayılır; yukarı yuvarlanmaz), %15 bir üstü (en çok Soylu),
        /// %10 bir altı. Efsane ebeveynin yavrusunda üst kademe şansı +%20 ve "Efsane soyundan" etiketi.
        /// </summary>
        public static AnimalLineage InheritLineage(Random rng, Animal mother, Animal father, double extraUpChance, out bool legendaryDescent)
        {
            int m = mother != null ? Math.Min((int)mother.Lineage, (int)AnimalLineage.Noble) : 0;
            int f = father != null ? Math.Min((int)father.Lineage, (int)AnimalLineage.Noble) : m;
            legendaryDescent = (mother != null && (mother.Lineage == AnimalLineage.Legendary || mother.LegendaryDescent))
                            || (father != null && (father.Lineage == AnimalLineage.Legendary || father.LegendaryDescent));
            int baseTier = (m + f) / 2;
            double up = LineageUpChance + Math.Max(0, extraUpChance) + (legendaryDescent ? LegendaryDescentBonus : 0);
            double roll = rng.NextDouble();
            if (roll < up) baseTier++;
            else if (roll < up + LineageDownChance) baseTier--;
            return (AnimalLineage)Math.Max(0, Math.Min((int)AnimalLineage.Noble, baseTier));
        }

        /// <summary>
        /// Efsane unvanı: hayvan Soylu ve 5. seviyede olmalı. Başarırsa kademesi Efsane olur, unvanı yazılır (bir kez).
        /// Kural dışıysa ya da zaten efsaneyse false.
        /// </summary>
        public static bool GrantLegend(Animal a, string title)
        {
            if (a == null || a.IsDead || string.IsNullOrEmpty(title)) return false;
            if (a.Lineage != AnimalLineage.Noble || a.Level < MaxLevel) return false;
            a.Lineage = AnimalLineage.Legendary;
            a.LegendaryTitle = title;
            return true;
        }

        /// <summary>Türe göre efsane unvanı (Project belgesi 29, 4.2).</summary>
        public static string TitleFor(string speciesId)
        {
            switch (speciesId)
            {
                case "animal.horse": return "Panayır Rüzgârı";
                case "animal.cow": return "Hasat Güzeli";
                case "animal.ox": return "Demir Omuz";
                case "animal.sheep": return "Altın Post";
                case "animal.goat": return "Dağ Keçisi";
                case "animal.chicken": return "Altın Yumurtlayan";
                case "animal.bees": return "Ana Arı Kraliçesi";
                default: return null;
            }
        }
    }

    /// <summary>At yetenekleri (Project belgesi 29, bölüm 2): her biri atın seviyesi ve binicinin Binicilik seviyesi ister.</summary>
    public enum HorseAbility
    {
        SaddleBag = 0,     // Heybe: +6 envanter yuvası
        Gallop = 1,        // Dörtnala: basılı tut, +%40 hız, dayanıklılık harcar
        Whistle = 2,       // Islıkla çağırma: dostluk 40
        Jump = 3,          // Engel atlama: 2 karo
        Cart = 4,          // Hafif at arabası
        Ford = 5,          // Sığ su geçme
        Slope = 6,         // Dik yamaç
        LongGallop = 7,    // Uzun dörtnala: dayanıklılık %35 yavaş biter
        HighJump = 8,      // Uçarcasına atlama: 3 karo, yalnızca Soylu ve Efsane
        Tireless = 9       // Yorulmaz: dörtnala yarı dayanıklılık
    }

    public static class HorseAbilities
    {
        public const float MaxStamina = 100f;
        public const double GallopSpeed = 1.4;
        /// <summary>Dörtnala gün başına dayanıklılık (100'ü ~0,15 günde, yaklaşık 3,5 oyun saatinde bitirir).</summary>
        public const double GallopDrainPerDay = 700;
        public const double WalkRegenPerDay = 240, StableRegenPerDay = 480;
        public const float TreatStamina = 50f;
        public const int SaddleBagSlots = 6;
        public const float WhistleFriendship = 40f;
        public const double ShyDaysAfterRefusal = 1.0;
        public const float RefusalFriendshipLoss = 3f;

        /// <summary>(at seviyesi, Binicilik seviyesi; 0 = şart yok).</summary>
        public static (int horse, int riding) Requirement(HorseAbility ab)
        {
            switch (ab)
            {
                case HorseAbility.SaddleBag: return (1, 1);
                case HorseAbility.Gallop: return (2, 2);
                case HorseAbility.Whistle: return (2, 0);
                case HorseAbility.Jump: return (3, 4);
                case HorseAbility.Cart: return (3, 0);
                case HorseAbility.Ford: return (3, 3);
                case HorseAbility.Slope: return (4, 5);
                case HorseAbility.LongGallop: return (4, 6);
                case HorseAbility.HighJump: return (5, 8);
                case HorseAbility.Tireless: return (5, 10);
                default: return (MaxLevelUnreachable, MaxLevelUnreachable);
            }
        }
        private const int MaxLevelUnreachable = 99;

        public static string Name(HorseAbility ab)
        {
            switch (ab)
            {
                case HorseAbility.SaddleBag: return "Heybe";
                case HorseAbility.Gallop: return "Dörtnala";
                case HorseAbility.Whistle: return "Islıkla çağırma";
                case HorseAbility.Jump: return "Engel atlama";
                case HorseAbility.Cart: return "Araba çekme";
                case HorseAbility.Ford: return "Sığ su geçme";
                case HorseAbility.Slope: return "Dik yamaç";
                case HorseAbility.LongGallop: return "Uzun dörtnala";
                case HorseAbility.HighJump: return "Uçarcasına atlama";
                case HorseAbility.Tireless: return "Yorulmaz";
                default: return ab.ToString();
            }
        }

        /// <summary>Neden kullanılamaz ("At 3 · Binicilik 4"); kullanılabiliyorsa null. rider null ise Binicilik şartı sayılmaz.</summary>
        public static string Blocker(HorseAbility ab, Animal horse, Character rider)
        {
            if (horse == null || horse.IsDead || !horse.Species.Rideable) return "Binek yok.";
            var (needHorse, needRiding) = Requirement(ab);
            if (ab == HorseAbility.HighJump && horse.Lineage < AnimalLineage.Noble) return "Yalnızca Soylu ve Efsane atlar.";
            if (ab == HorseAbility.Whistle && horse.Friendship < WhistleFriendship && !AnimalSystem.HasGift(horse, Genetics.Gift.Homing)) return $"Dostluk {WhistleFriendship:0} gerekli.";
            int riding = rider != null ? rider.Skills.GetLevel(SkillType.Riding) : 99;
            bool horseOk = horse.Level >= needHorse, riderOk = needRiding == 0 || riding >= needRiding;
            if (horseOk && riderOk) return null;
            return needRiding > 0 ? $"At {needHorse} · Binicilik {needRiding}" : $"At {needHorse}";
        }

        public static bool Has(HorseAbility ab, Animal horse, Character rider) => Blocker(ab, horse, rider) == null;

        /// <summary>Dörtnala dayanıklılık çarpanı (Uzun dörtnala x0,65, Yorulmaz x0,5).</summary>
        public static double DrainFactor(Animal horse, Character rider)
        {
            double gift = AnimalSystem.HasGift(horse, Genetics.Gift.LongBreathHorse) ? 0.85 : 1.0;   // Uzun soluk (belge 48)
            // Dayanıklılık geni: 5'in üstünde her puan %4 daha az harcar (en çok %20).
            if (horse?.Genome != null) gift *= 1.0 - Math.Min(0.20, Math.Max(0, horse.Genome.Stat(Genetics.GeneStat.Stamina) - 5) * 0.04);
            if (Has(HorseAbility.Tireless, horse, rider)) return 0.5 * gift;
            if (Has(HorseAbility.LongGallop, horse, rider)) return 0.65 * gift;
            return gift;
        }

        /// <summary>
        /// Dayanıklılık: dörtnalada harcanır, yürürken ve ahırda dolar (dostluk ve konfor hızlandırır). Sıfırlanınca dörtnala kapanır.
        /// "Ürkek" süresi de burada azalır.
        /// </summary>
        public static void Tick(Animal a, double gameDays, bool ridden, Character rider)
        {
            if (a == null || gameDays <= 0) return;
            if (a.ShyDays > 0) a.ShyDays = Math.Max(0, a.ShyDays - gameDays);
            if (!a.Species.Rideable) return;
            if (ridden && a.Galloping)
            {
                a.Stamina = (float)Math.Max(0, a.Stamina - GallopDrainPerDay * DrainFactor(a, rider) * gameDays);
                if (a.Stamina <= 0) a.Galloping = false;
                return;
            }
            double regen = (ridden ? WalkRegenPerDay : StableRegenPerDay) * (0.8 + 0.2 * a.Friendship / 100.0 + 0.2 * a.Comfort / 100.0);
            a.Stamina = (float)Math.Min(MaxStamina, a.Stamina + regen * gameDays);
        }

        /// <summary>
        /// Atlama başarısı (29-2.1): %85 + 3 x (Binicilik − şart) + 5 x (at − şart) + dostluk/20 puan; düşük dayanıklılık (&lt;20) −15,
        /// yağmur/kar −10; en çok %99, en az %50. Yetenek yoksa 0.
        /// </summary>
        public static double JumpChance(Animal horse, Character rider, int tiles, bool badWeather, int extraPoints = 0)
        {
            var ab = tiles >= 3 ? HorseAbility.HighJump : HorseAbility.Jump;
            if (!Has(ab, horse, rider) || horse.ShyDays > 0) return 0;
            var (needHorse, needRiding) = Requirement(ab);
            int riding = rider != null ? rider.Skills.GetLevel(SkillType.Riding) : needRiding;
            double points = 85 + 3 * (riding - needRiding) + 5 * (horse.Level - needHorse) + horse.Friendship / 20.0 + extraPoints;
            if (AnimalSystem.HasGift(horse, Genetics.Gift.SureFoot)) points += 10;   // Sağlam ayak
            if (horse.Stamina < 20) points -= 15;
            if (badWeather) points -= 10;
            return Math.Max(0.50, Math.Min(0.99, points / 100.0));
        }
    }

    /// <summary>Atlamanın sonucu.</summary>
    public enum JumpOutcome { NotAllowed = 0, Cleared = 1, Refused = 2, Fell = 3 }
}
