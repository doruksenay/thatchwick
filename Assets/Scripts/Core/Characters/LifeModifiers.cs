using System;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Bir karakterin kalıcı etki çarpanları: özellikler, meslek dalları ve köy projeleri bunları doldurur
    /// (LifeRules.Recompute). Varsayılan değerler "etki yok" demektir; böylece özelliksiz karakter
    /// eski davranışın aynısını gösterir.
    /// </summary>
    public sealed class LifeModifiers
    {
        // ---- ihtiyaçlar ----
        public float HungerDecay = 1f, EnergyDecay = 1f, SocialDecay = 1f, FunDecay = 1f;
        /// <summary>0-0.8: soğuktan Isınma kaybını bu oranda azaltır.</summary>
        public float ColdResist;

        // ---- iş ve beceri ----
        /// <summary>İş hızı çarpanı (1,1 = %10 hızlı).</summary>
        public float WorkSpeed = 1f;
        private readonly float[] _skillRate = NewSkillRates();
        /// <summary>Hasat ve kesimde ek ürün şansı (0-1).</summary>
        public float YieldChance;
        /// <summary>Yemek kalitesine eklenen beceri seviyesi.</summary>
        public int QualityLevels;
        /// <summary>Yapı/eşya satın alımında indirim (0-0.5).</summary>
        public float BuildDiscount;
        /// <summary>Pazar satışına eklenen oran (0-0.5).</summary>
        public float SellBonus;
        /// <summary>Bu karakterin ürünlerinin bozulma süresini uzatma oranı (0-0.8).</summary>
        public float SpoilReduction;
        /// <summary>Bitki büyüme hızı çarpanı (yalnızca bu karakterin diktikleri için değil, köy genelinde okunur).</summary>
        public float GrowthSpeed = 1f;

        // ---- sosyal ve biyoloji ----
        public float SocialGain = 1f;
        public int MoodBase;
        /// <summary>Doğumda ömre eklenen gün (özellik).</summary>
        public int LifespanDays;
        public float Fertility = 1f;

        private static float[] NewSkillRates()
        {
            var a = new float[Enum.GetValues(typeof(SkillType)).Length];
            for (int i = 0; i < a.Length; i++) a[i] = 1f;
            return a;
        }

        public float SkillRate(SkillType skill) => _skillRate[(int)skill];

        public void SetSkillRate(SkillType skill, float rate) => _skillRate[(int)skill] = rate;

        public void MultiplySkillRate(SkillType skill, float rate) => _skillRate[(int)skill] *= rate;

        /// <summary>Başka bir etki kümesini üstüne bindirir: çarpanlar çarpılır, eklenenler toplanır.</summary>
        public void Apply(LifeModifiers o)
        {
            HungerDecay *= o.HungerDecay;
            EnergyDecay *= o.EnergyDecay;
            SocialDecay *= o.SocialDecay;
            FunDecay *= o.FunDecay;
            ColdResist = Math.Min(0.8f, ColdResist + o.ColdResist);
            WorkSpeed *= o.WorkSpeed;
            for (int i = 0; i < _skillRate.Length; i++) _skillRate[i] *= o._skillRate[i];
            YieldChance = Math.Min(1f, YieldChance + o.YieldChance);
            QualityLevels += o.QualityLevels;
            BuildDiscount = Math.Min(0.5f, BuildDiscount + o.BuildDiscount);
            SellBonus = Math.Min(0.5f, SellBonus + o.SellBonus);
            SpoilReduction = Math.Min(0.8f, SpoilReduction + o.SpoilReduction);
            GrowthSpeed *= o.GrowthSpeed;
            SocialGain *= o.SocialGain;
            MoodBase += o.MoodBase;
            LifespanDays += o.LifespanDays;
            Fertility *= o.Fertility;
        }
    }
}
