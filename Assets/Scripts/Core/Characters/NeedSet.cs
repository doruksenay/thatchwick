using System;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Bir karakterin ihtiyaç değerleri. Azalma oyun günü cinsinden verilir
    /// (biyoloji oyun saatinde işler); hız çarpanı zaten GameClock'ta uygulanır.
    /// Özellikler ve diğer etkiler azalmayı çarpanla değiştirir (SetDecayMultiplier).
    /// </summary>
    public sealed class NeedSet
    {
        public const float Max = 100f;
        public const float Min = 0f;
        public const float CriticalThreshold = 15f;

        private static readonly int NeedCount = Enum.GetValues(typeof(NeedType)).Length;

        // Oyun günü başına varsayılan azalma (Açlık, Enerji, Isınma, Sosyal, Eğlence). Denge adımında (28) ayarlanır.
        private static readonly float[] DefaultDecayPerDay = { 120f, 90f, 0f, 30f, 24f };

        private readonly float[] _values = new float[NeedCount];
        private readonly float[] _decayPerDay = new float[NeedCount];
        private readonly float[] _decayMultiplier = new float[NeedCount];

        public NeedSet()
        {
            for (int i = 0; i < NeedCount; i++)
            {
                _values[i] = Max;
                _decayPerDay[i] = DefaultDecayPerDay[i];
                _decayMultiplier[i] = 1f;
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

        /// <summary>Azalma çarpanı (özellik, meslek dalı). 1 = değişiklik yok.</summary>
        public void SetDecayMultiplier(NeedType need, float multiplier)
        {
            if (multiplier < 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
            _decayMultiplier[(int)need] = multiplier;
        }

        public float GetDecayMultiplier(NeedType need) => _decayMultiplier[(int)need];

        /// <summary>İhtiyacı verilen miktar kadar doyurur (yemek, uyku, ısınma).</summary>
        public void Restore(NeedType need, float amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            _values[(int)need] = Clamp(_values[(int)need] + amount);
        }

        public bool IsCritical(NeedType need) => _values[(int)need] <= CriticalThreshold;

        /// <summary>Hayatta kalma ihtiyaçlarından biri kritik mi (Sosyal/Eğlence sayılmaz: bakım işini engellemesin).</summary>
        public bool AnyCritical()
        {
            for (int i = 0; i < NeedCount; i++)
                if (NeedKinds.IsSurvival((NeedType)i) && _values[i] <= CriticalThreshold) return true;
            return false;
        }

        /// <summary>Geçen oyun günü kadar tüm ihtiyaçları azaltır.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));
            for (int i = 0; i < NeedCount; i++)
                _values[i] = Clamp(_values[i] - (float)(_decayPerDay[i] * _decayMultiplier[i] * gameDays));
        }

        private static float Clamp(float v) => v < Min ? Min : (v > Max ? Max : v);
    }
}
