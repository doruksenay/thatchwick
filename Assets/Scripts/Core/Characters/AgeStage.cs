using System;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Yaşam evreleri. Oyunda yıl değil evre adı gösterilir.
    /// 17.11.2026 kararı: eski "Genç" evresi Sims gibi ikiye bölündü: Ergen ve Genç yetişkin.
    /// Evre kayda yazılmaz, yaştan türetilir; eski kayıtlardaki "Genç" karakterler yaşlarına göre
    /// kendiliğinden Ergen ya da Genç yetişkin olur.
    /// </summary>
    public enum AgeStage
    {
        Baby = 0,        // Bebek
        Child = 1,       // Çocuk
        Teen = 2,        // Ergen
        YoungAdult = 3,  // Genç yetişkin
        Adult = 4,       // Yetişkin
        Elder = 5        // Yaşlı
    }

    /// <summary>Evrelerle ilgili ortak kurallar (tek yerde tutulur).</summary>
    public static class AgeStages
    {
        public static string Name(AgeStage s)
        {
            switch (s)
            {
                case AgeStage.Baby: return "Bebek";
                case AgeStage.Child: return "Çocuk";
                case AgeStage.Teen: return "Ergen";
                case AgeStage.YoungAdult: return "Genç yetişkin";
                case AgeStage.Adult: return "Yetişkin";
                default: return "Yaşlı";
            }
        }

        /// <summary>Evlilik, talip havuzu ve görücü usulü: Genç yetişkin ve Yetişkin.</summary>
        public static bool CanMarry(AgeStage s) => s == AgeStage.YoungAdult || s == AgeStage.Adult;

        /// <summary>Hamilelik: yalnızca Genç yetişkin ve Yetişkin (Ergen değil).</summary>
        public static bool CanConceive(AgeStage s) => s == AgeStage.YoungAdult || s == AgeStage.Adult;

        /// <summary>+18 paketi (alkol, kenevir): Genç yetişkin ve üstü.</summary>
        public static bool IsAdultContentAge(AgeStage s) => s >= AgeStage.YoungAdult;

        /// <summary>Karakter yaratmada seçilebilen başlangıç evreleri.</summary>
        public static bool IsStartStage(AgeStage s) => s == AgeStage.YoungAdult || s == AgeStage.Adult;

        /// <summary>Çocuk ya da Ergen: ders alabilir, yetişkin işleri kısıtlıdır.</summary>
        public static bool IsMinor(AgeStage s) => s <= AgeStage.Teen;
    }

    /// <summary>
    /// Yaş evresi sınırları (oyun günü). Varsayılan: 12 / 24 / 30 / 42 / 72; üst sınır 144.
    ///   Bebek 0-12, Çocuk 12-24, Ergen 24-30, Genç yetişkin 30-42, Yetişkin 42-72, Yaşlı 72+.
    ///   144 = mutlak üst sınır (bu yaşta herkes ölür).
    ///   96 = en erken doğal ölüm: ömür her karakter için [96, 144] aralığından seçilir
    ///   (yaşlılık 72'de başlar, 72-96 arası ölüm yok, 96 sonrası değişken "yaşam ucu").
    /// Ergen ve Genç yetişkin sınırları yer tutucudur; denge turunda (Adım 28) değişebilir.
    /// </summary>
    public sealed class AgeConfig
    {
        public double ChildStartDay { get; }
        public double TeenStartDay { get; }
        public double YoungAdultStartDay { get; }
        public double AdultStartDay { get; }
        public double ElderStartDay { get; }
        public double EarliestNaturalDeathDay { get; }
        public double MaxLifespanDay { get; }

        public AgeConfig(
            double childStart = 12, double teenStart = 24, double youngAdultStart = 30, double adultStart = 42,
            double elderStart = 72, double earliestDeath = 96, double maxLifespan = 144)
        {
            if (!(0 < childStart && childStart < teenStart && teenStart < youngAdultStart
                  && youngAdultStart < adultStart && adultStart < elderStart))
                throw new ArgumentException("Evre sınırları artan sırada ve pozitif olmalı.");
            if (earliestDeath < elderStart) throw new ArgumentException("En erken ölüm, yaşlılık başlangıcından önce olamaz.");
            if (maxLifespan < earliestDeath) throw new ArgumentException("Üst sınır, en erken ölümden küçük olamaz.");
            ChildStartDay = childStart;
            TeenStartDay = teenStart;
            YoungAdultStartDay = youngAdultStart;
            AdultStartDay = adultStart;
            ElderStartDay = elderStart;
            EarliestNaturalDeathDay = earliestDeath;
            MaxLifespanDay = maxLifespan;
        }

        public static AgeConfig Default => new AgeConfig();

        /// <summary>Evrenin başladığı gün.</summary>
        public double StartOf(AgeStage s)
        {
            switch (s)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return ChildStartDay;
                case AgeStage.Teen: return TeenStartDay;
                case AgeStage.YoungAdult: return YoungAdultStartDay;
                case AgeStage.Adult: return AdultStartDay;
                default: return ElderStartDay;
            }
        }

        public AgeStage StageAt(double ageDays)
        {
            if (ageDays < ChildStartDay) return AgeStage.Baby;
            if (ageDays < TeenStartDay) return AgeStage.Child;
            if (ageDays < YoungAdultStartDay) return AgeStage.Teen;
            if (ageDays < AdultStartDay) return AgeStage.YoungAdult;
            if (ageDays < ElderStartDay) return AgeStage.Adult;
            return AgeStage.Elder;
        }

        /// <summary>Rastgele ömür: [EarliestNaturalDeathDay, MaxLifespanDay] aralığında.</summary>
        public double RollLifespan(Random rng) =>
            EarliestNaturalDeathDay + rng.NextDouble() * (MaxLifespanDay - EarliestNaturalDeathDay);
    }
}
