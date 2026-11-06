using System;

namespace Thatchwick.Core.Characters
{
    /// <summary>Yaşam evreleri. Oyunda yıl değil evre adı gösterilir.</summary>
    public enum AgeStage
    {
        Baby = 0,   // Bebek
        Child = 1,  // Çocuk
        Youth = 2,  // Genç
        Adult = 3,  // Yetişkin
        Elder = 4   // Yaşlı
    }

    /// <summary>
    /// Yaş evresi sınırları (oyun günü). Varsayılan: 12 / 24 / 36 / 72 / 144.
    ///   Bebek 0-12, Çocuk 12-24, Genç 24-36, Yetişkin 36-72, Yaşlı 72+.
    ///   144 = mutlak üst sınır (bu yaşta herkes ölür).
    ///   96 = en erken doğal ölüm: ömür her karakter için [96, 144] aralığından seçilir
    ///   (yaşlılık 72'de başlar, 72-96 arası ölüm yok, 96 sonrası değişken "yaşam ucu").
    /// Hepsi ayarlanabilir; denge turunda (Adım 28) değişebilir.
    /// </summary>
    public sealed class AgeConfig
    {
        public double ChildStartDay { get; }
        public double YouthStartDay { get; }
        public double AdultStartDay { get; }
        public double ElderStartDay { get; }
        public double EarliestNaturalDeathDay { get; }
        public double MaxLifespanDay { get; }

        public AgeConfig(
            double childStart = 12, double youthStart = 24, double adultStart = 36,
            double elderStart = 72, double earliestDeath = 96, double maxLifespan = 144)
        {
            if (!(0 < childStart && childStart < youthStart && youthStart < adultStart && adultStart < elderStart))
                throw new ArgumentException("Evre sınırları artan sırada ve pozitif olmalı.");
            if (earliestDeath < elderStart) throw new ArgumentException("En erken ölüm, yaşlılık başlangıcından önce olamaz.");
            if (maxLifespan < earliestDeath) throw new ArgumentException("Üst sınır, en erken ölümden küçük olamaz.");
            ChildStartDay = childStart;
            YouthStartDay = youthStart;
            AdultStartDay = adultStart;
            ElderStartDay = elderStart;
            EarliestNaturalDeathDay = earliestDeath;
            MaxLifespanDay = maxLifespan;
        }

        public static AgeConfig Default => new AgeConfig();

        public AgeStage StageAt(double ageDays)
        {
            if (ageDays < ChildStartDay) return AgeStage.Baby;
            if (ageDays < YouthStartDay) return AgeStage.Child;
            if (ageDays < AdultStartDay) return AgeStage.Youth;
            if (ageDays < ElderStartDay) return AgeStage.Adult;
            return AgeStage.Elder;
        }

        /// <summary>Rastgele ömür: [EarliestNaturalDeathDay, MaxLifespanDay] aralığında.</summary>
        public double RollLifespan(Random rng) =>
            EarliestNaturalDeathDay + rng.NextDouble() * (MaxLifespanDay - EarliestNaturalDeathDay);
    }
}
