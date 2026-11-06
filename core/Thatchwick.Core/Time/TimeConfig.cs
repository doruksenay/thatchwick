using System;

namespace Thatchwick.Core.Time
{
    /// <summary>
    /// Zaman modeli ayarları. Varsayılanlar tasarım özetindeki değerlerdir:
    /// 1 oyun günü = 6 gerçek dakika (1x), mevsim = 10 gün, yıl = 4 mevsim.
    /// Mevsim uzunluğu 8/10/12 ile test edilir (T2).
    /// </summary>
    public sealed class TimeConfig
    {
        public const int SeasonsPerYear = 4;

        public double RealSecondsPerGameDay { get; }
        public int DaysPerSeason { get; }

        public int DaysPerYear => DaysPerSeason * SeasonsPerYear;

        public TimeConfig(double realSecondsPerGameDay = 360.0, int daysPerSeason = 10)
        {
            if (realSecondsPerGameDay <= 0) throw new ArgumentOutOfRangeException(nameof(realSecondsPerGameDay));
            if (daysPerSeason <= 0) throw new ArgumentOutOfRangeException(nameof(daysPerSeason));
            RealSecondsPerGameDay = realSecondsPerGameDay;
            DaysPerSeason = daysPerSeason;
        }

        public static TimeConfig Default => new TimeConfig();
    }
}
