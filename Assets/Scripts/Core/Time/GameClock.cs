using System;

namespace Thatchwick.Core.Time
{
    /// <summary>
    /// Oyun saati. Gerçek zaman (saniye) ve hız çarpanıyla ilerler; biyoloji oyun
    /// gününe bağlıdır, hız yalnızca gerçek saniyeyi oyun gününe çevirir.
    /// Kritik anlarda hız otomatik 1x'e düşer (T3).
    /// UnityEngine'e bağımlı değildir.
    /// </summary>
    public sealed class GameClock
    {
        public static readonly int[] AllowedSpeeds = { 1, 2, 3 };

        private readonly TimeConfig _config;
        private int _requestedSpeed = 1;
        private int _criticalLocks;

        /// <summary>Başlangıçtan beri geçen toplam oyun günü (kesirli).</summary>
        public double TotalDays { get; private set; }

        public GameClock(TimeConfig config = null)
        {
            _config = config ?? TimeConfig.Default;
        }

        public TimeConfig Config => _config;

        /// <summary>Oyuncunun seçtiği hız (1, 2 ya da 3).</summary>
        public int RequestedSpeed => _requestedSpeed;

        /// <summary>Gerçekte uygulanan hız: kritik an varsa 1x.</summary>
        public int EffectiveSpeed => _criticalLocks > 0 ? 1 : _requestedSpeed;

        public bool IsCriticalMoment => _criticalLocks > 0;

        public void SetSpeed(int speed)
        {
            if (Array.IndexOf(AllowedSpeeds, speed) < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), "Hız 1, 2 ya da 3 olmalı.");
            _requestedSpeed = speed;
        }

        /// <summary>Doğum, ölüm, yangın gibi kritik an başlar. İç içe çağrılabilir.</summary>
        public void EnterCriticalMoment() => _criticalLocks++;

        public void ExitCriticalMoment()
        {
            if (_criticalLocks == 0) throw new InvalidOperationException("Kritik an zaten kapalı.");
            _criticalLocks--;
        }

        /// <summary>Kayıttan yükleme: toplam gün ve oyuncunun seçtiği hız.</summary>
        public void Restore(double totalDays, int speed)
        {
            if (totalDays < 0) throw new ArgumentOutOfRangeException(nameof(totalDays));
            TotalDays = totalDays;
            if (Array.IndexOf(AllowedSpeeds, speed) >= 0) _requestedSpeed = speed;
        }

        /// <summary>Gerçek geçen süreyi (saniye) oyun saatine uygular; geçen oyun günü döner.</summary>
        public double Advance(double realSeconds)
        {
            if (realSeconds < 0) throw new ArgumentOutOfRangeException(nameof(realSeconds));
            double gameDays = realSeconds * EffectiveSpeed / _config.RealSecondsPerGameDay;
            TotalDays += gameDays;
            return gameDays;
        }

        /// <summary>Tam geçen gün sayısı (0 tabanlı gün indeksi).</summary>
        public long DayIndex => (long)Math.Floor(TotalDays);

        /// <summary>Günün içindeki ilerleme, 0 (gün başı) ile 1 (gün sonu) arası.</summary>
        public double DayFraction => TotalDays - Math.Floor(TotalDays);

        /// <summary>Mevsim içindeki gün (0 tabanlı).</summary>
        public int DayOfSeason => (int)(DayIndex % _config.DaysPerSeason);

        public Season Season => (Season)((DayIndex / _config.DaysPerSeason) % TimeConfig.SeasonsPerYear);

        /// <summary>Kaçıncı oyun yılı (0 tabanlı).</summary>
        public long YearIndex => DayIndex / _config.DaysPerYear;
    }
}
