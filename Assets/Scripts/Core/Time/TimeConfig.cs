using System;

namespace Thatchwick.Core.Time
{
    /// <summary>
    /// Zaman modeli ayarları. Varsayılanlar tasarım özetindeki değerlerdir:
    /// 1 oyun günü = 12 gerçek dakika (1x; belge 47, 20.11.2026, önce 6 dakika), mevsim = 10 gün, yıl = 4 mevsim.
    /// Mevsim uzunluğu 8/10/12 ile test edilir (T2).
    /// </summary>
    public sealed class TimeConfig
    {
        public const int SeasonsPerYear = 4;

        public double RealSecondsPerGameDay { get; }
        public int DaysPerSeason { get; }

        public int DaysPerYear => DaysPerSeason * SeasonsPerYear;

        /// <summary>İş süreleri ve yürüme hızı bu gün uzunluğuna göre yazıldı (eski 6 dakikalık gün).</summary>
        public const double BaseRealSecondsPerGameDay = 360.0;
        /// <summary>Varsayılan gün uzunluğu: 12 gerçek dakika (belge 47).</summary>
        public const double DefaultRealSecondsPerGameDay = 720.0;

        /// <summary>
        /// İş süresi ölçeği: işler gerçek zamanda sabit kalsın diye oyun saatindeki süreler bununla çarpılır (12 dakikalık günde 0,5).
        /// </summary>
        public double ActionTimeScale => BaseRealSecondsPerGameDay / RealSecondsPerGameDay;

        public TimeConfig(double realSecondsPerGameDay = DefaultRealSecondsPerGameDay, int daysPerSeason = 10)
        {
            if (realSecondsPerGameDay <= 0) throw new ArgumentOutOfRangeException(nameof(realSecondsPerGameDay));
            if (daysPerSeason <= 0) throw new ArgumentOutOfRangeException(nameof(daysPerSeason));
            RealSecondsPerGameDay = realSecondsPerGameDay;
            DaysPerSeason = daysPerSeason;
        }

        public static TimeConfig Default => new TimeConfig();
    }
}
