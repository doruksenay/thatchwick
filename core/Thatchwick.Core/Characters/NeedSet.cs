using System;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Bir karakterin ihtiyaç değerleri. Azalma oyun günü cinsinden verilir
    /// (biyoloji oyun saatinde işler); hız çarpanı zaten GameClock'ta uygulanır.
    /// </summary>
    public sealed class NeedSet
    {
        public const float Max = 100f;
        public const float Min = 0f;
        public const float CriticalThreshold = 15f;

        private static readonly int NeedCount = Enum.GetValues(typeof(NeedType)).Length;

        // Oyun günü başına varsayılan azalma. Denge adımında (28) ayarlanır.
        private static readonly float[] DefaultDecayPerDay = { 120f, 90f, 0f };

        private readonly float[] _values = new float[NeedCount];
        private readonly float[] _decayPerDay = new float[NeedCount];

        public NeedSet()
        {
            for (int i = 0; i < NeedCount; i++)
            {
                _values[i] = Max;
                _decayPerDay[i] = DefaultDecayPerDay[i];
            }
        }

        public float Get(NeedType need) => _values[(int)need];

        public void Set(NeedType need, float value) => _values[(int)need] = Clamp(value);

        public void SetDecayPerDay(NeedType need, float perDay)
        {
            if (perDay < 0) throw new ArgumentOutOfRangeException(nameof(perDay));
            _decayPerDay[(int)need] = perDay;
        }

        public float GetDecayPerDay(NeedType need) => _decayPerDay[(int)need];

        /// <summary>İhtiyacı verilen miktar kadar doyurur (yemek, uyku, ısınma).</summary>
        public void Restore(NeedType need, float amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            _values[(int)need] = Clamp(_values[(int)need] + amount);
        }

        public bool IsCritical(NeedType need) => _values[(int)need] <= CriticalThreshold;

        public bool AnyCritical()
        {
            for (int i = 0; i < NeedCount; i++)
                if (_values[i] <= CriticalThreshold) return true;
            return false;
        }

        /// <summary>Geçen oyun günü kadar tüm ihtiyaçları azaltır.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));
            for (int i = 0; i < NeedCount; i++)
                _values[i] = Clamp(_values[i] - (float)(_decayPerDay[i] * gameDays));
        }

        private static float Clamp(float v) => v < Min ? Min : (v > Max ? Max : v);
    }
}
