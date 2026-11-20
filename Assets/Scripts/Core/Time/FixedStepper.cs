using System;

namespace Thatchwick.Core.Time
{
    /// <summary>
    /// Sabit adımlı simülasyon sayacı (performans incelemesi 20.11.2026). Kare süresi biriktirilir; her
    /// <see cref="StepSeconds"/> dolunca bir simülasyon adımı çalışır. Böylece mantık kare hızından bağımsızdır
    /// (mobil 30 FPS ile masaüstü 60 FPS aynı sonucu verir) ve ağır sistemler kare başına değil adım başına çalışır.
    /// Görünüm iki adım arasını <see cref="Alpha"/> ile ara değerler. UnityEngine yok; test edilebilir.
    /// </summary>
    public sealed class FixedStepper
    {
        /// <summary>Varsayılan: saniyede 10 simülasyon adımı.</summary>
        public const double DefaultStepSeconds = 0.1;
        /// <summary>Bir karede en çok bu kadar adım (takılan karede "ölüm sarmalı" olmasın; fazla süre düşülür).</summary>
        public const int DefaultMaxSteps = 4;

        private double _accumulator;

        public FixedStepper(double stepSeconds = DefaultStepSeconds, int maxStepsPerFrame = DefaultMaxSteps)
        {
            if (stepSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(stepSeconds));
            if (maxStepsPerFrame < 1) throw new ArgumentOutOfRangeException(nameof(maxStepsPerFrame));
            StepSeconds = stepSeconds;
            MaxStepsPerFrame = maxStepsPerFrame;
        }

        public double StepSeconds { get; }
        public int MaxStepsPerFrame { get; }

        /// <summary>Son adımdan bu yana geçen sürenin adıma oranı (0..1): görünüm ara değeri.</summary>
        public float Alpha => (float)Math.Min(1.0, _accumulator / StepSeconds);

        /// <summary>
        /// Kare süresini ekler ve bu karede çalışacak adım sayısını döner. Sınırı aşan birikim atılır
        /// (oyun saati takılmadan sonra öne fırlamaz).
        /// </summary>
        public int Accumulate(double realSeconds)
        {
            if (realSeconds > 0 && !double.IsNaN(realSeconds) && !double.IsInfinity(realSeconds)) _accumulator += realSeconds;
            int steps = (int)Math.Floor(_accumulator / StepSeconds);
            if (steps > MaxStepsPerFrame)
            {
                steps = MaxStepsPerFrame;
                _accumulator = 0;
                return steps;
            }
            _accumulator -= steps * StepSeconds;
            if (_accumulator < 0) _accumulator = 0;
            return steps;
        }

        /// <summary>Birikimi sıfırlar (yükleme, sahne değişimi).</summary>
        public void Reset() => _accumulator = 0;
    }
}
