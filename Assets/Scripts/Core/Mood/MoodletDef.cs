using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Mood
{
    /// <summary>
    /// Bir ruh hali etkeni (moodlet): "Dinlenmiş +10, 0,6 gün, odunculuk ve çiftçilik %15 hızlı".
    /// Value ruh hali toplamına eklenir. Süre 0 ise koşula bağlıdır (ihtiyaç düşükken açık kalır).
    /// Beceri etkileri kesirdir: 0,2 = %20 artış, -0,15 = %15 düşüş.
    /// </summary>
    public sealed class MoodletDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Kısa açıklama (arayüzde ipucu).</summary>
        public string Description { get; set; }
        public int Value { get; set; }
        /// <summary>Oyun günü. 0 = koşul moodlet'i (sistem açar ve kapatır).</summary>
        public double DurationDays { get; set; }
        /// <summary>Tüm becerilerde XP etkisi.</summary>
        public float AllXp { get; set; }
        /// <summary>Tüm beceri işlerinde hız etkisi.</summary>
        public float AllSpeed { get; set; }
        public Dictionary<SkillType, float> SkillXp { get; } = new Dictionary<SkillType, float>();
        public Dictionary<SkillType, float> SkillSpeed { get; } = new Dictionary<SkillType, float>();

        public bool IsCondition => DurationDays <= 0.0;
        public bool IsPositive => Value > 0;

        public float XpBonus(SkillType skill) => AllXp + (SkillXp.TryGetValue(skill, out var v) ? v : 0f);
        public float SpeedBonus(SkillType skill) => AllSpeed + (SkillSpeed.TryGetValue(skill, out var v) ? v : 0f);
    }

    /// <summary>Ruh hali basamağı: moodlet değerlerinin toplamından.</summary>
    public enum MoodStatus
    {
        Miserable = 0, // Perişan
        Unhappy = 1,   // Mutsuz
        Neutral = 2,   // Nötr
        Good = 3,      // İyi
        Happy = 4      // Mutlu
    }

    public static class MoodRules
    {
        public const int MiserableAt = -40, UnhappyAt = -10, GoodAt = 10, HappyAt = 30;

        public static MoodStatus StatusFor(int total)
        {
            if (total <= MiserableAt) return MoodStatus.Miserable;
            if (total <= UnhappyAt) return MoodStatus.Unhappy;
            if (total < GoodAt) return MoodStatus.Neutral;
            if (total < HappyAt) return MoodStatus.Good;
            return MoodStatus.Happy;
        }

        /// <summary>Ruh halinin tüm becerilere XP çarpanı.</summary>
        public static double XpMultiplier(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return 0.6;
                case MoodStatus.Unhappy: return 0.85;
                case MoodStatus.Good: return 1.1;
                case MoodStatus.Happy: return 1.25;
                default: return 1.0;
            }
        }

        /// <summary>Ruh halinin beceri işlerine hız çarpanı (süre buna bölünür).</summary>
        public static double SpeedMultiplier(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return 0.8;
                case MoodStatus.Unhappy: return 0.9;
                case MoodStatus.Good: return 1.05;
                case MoodStatus.Happy: return 1.1;
                default: return 1.0;
            }
        }

        public static string Name(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return "Perişan";
                case MoodStatus.Unhappy: return "Mutsuz";
                case MoodStatus.Good: return "İyi";
                case MoodStatus.Happy: return "Mutlu";
                default: return "Nötr";
            }
        }
    }
}
