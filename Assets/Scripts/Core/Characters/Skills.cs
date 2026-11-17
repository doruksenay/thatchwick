using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Characters
{
    /// <summary>İlk prototip becerileri. Beceriler çalışarak öğrenilir (kitapla değil).</summary>
    public enum SkillType
    {
        Farming = 0,          // Çiftçilik: ekin, tarla, toplayıcılık
        Woodcutting = 1,      // Odunculuk
        Cooking = 2,          // Yemek: pişirme, kurutma, turşu, tütsü
        Carpentry = 3,        // Marangozluk: mobilya, tahta
        AnimalHusbandry = 4,  // Hayvancılık: bakım, sağım, kırkım, yavru
        Riding = 5,           // Binicilik
        Fishing = 6,          // Balıkçılık
        Leatherwork = 7,      // Saraçlık (zanaat): deri, eyer
        Weaving = 8,          // Dokumacılık (zanaat): yün, iplik, kumaş, halı
        Trade = 9,            // Ticaret ve pazarlık
        Herbalism = 10        // Otacılık: iksir, merhem
    }

    /// <summary>Becerilerin görünen adları (zanaatler alt başlıklarıyla ayrı beceridir).</summary>
    public static class SkillNames
    {
        public static string Name(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: return "Çiftçilik";
                case SkillType.Woodcutting: return "Odunculuk";
                case SkillType.Cooking: return "Yemek";
                case SkillType.Carpentry: return "Marangozluk";
                case SkillType.AnimalHusbandry: return "Hayvancılık";
                case SkillType.Riding: return "Binicilik";
                case SkillType.Fishing: return "Balıkçılık";
                case SkillType.Leatherwork: return "Saraçlık";
                case SkillType.Weaving: return "Dokumacılık";
                case SkillType.Trade: return "Ticaret";
                case SkillType.Herbalism: return "Otacılık";
                default: return s.ToString();
            }
        }

        /// <summary>Zanaat alt başlığı mı (kasabadaki ustalarla ilişkili)?</summary>
        public static bool IsCraft(SkillType s) => s == SkillType.Carpentry || s == SkillType.Leatherwork || s == SkillType.Weaving;

        /// <summary>Meslek adı (hanedan geçmişi, köy).</summary>
        public static string TradeName(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: return "Çiftçi";
                case SkillType.Woodcutting: return "Oduncu";
                case SkillType.Cooking: return "Aşçı";
                case SkillType.Carpentry: return "Marangoz";
                case SkillType.AnimalHusbandry: return "Hayvancı";
                case SkillType.Riding: return "Binici";
                case SkillType.Fishing: return "Balıkçı";
                case SkillType.Leatherwork: return "Saraç";
                case SkillType.Weaving: return "Dokumacı";
                case SkillType.Trade: return "Tüccar";
                case SkillType.Herbalism: return "Otacı";
                default: return "Köylü";
            }
        }
    }

    /// <summary>Seviye eğrisi ve yaş evresine göre seviye tavanı.</summary>
    public static class SkillRules
    {
        public const int MaxLevel = 10;

        /// <summary>Seviye L'ye ulaşmak için gereken toplam XP: 50 * (L-1)^2 (L1=0, L2=50, L3=200 ... L10=4050).</summary>
        public static double XpForLevel(int level)
        {
            if (level < 1) level = 1;
            if (level > MaxLevel) level = MaxLevel;
            int n = level - 1;
            return 50.0 * n * n;
        }

        public static int LevelForXp(double xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= XpForLevel(level + 1)) level++;
            return level;
        }

        /// <summary>
        /// Evreye göre ulaşılabilecek en yüksek seviye. Bebek öğrenemez (0).
        /// Varsayım (denge turunda ayarlanır): Çocuk 3, Genç 7, Yetişkin ve Yaşlı 10.
        /// </summary>
        public static int LevelCap(AgeStage stage)
        {
            switch (stage)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return 3;
                case AgeStage.Youth: return 7;
                default: return MaxLevel;
            }
        }
    }

    /// <summary>Bir karakterin becerileri: XP tutar, seviyeyi XP'den türetir.</summary>
    public sealed class SkillSet
    {
        private readonly Dictionary<SkillType, double> _xp = new Dictionary<SkillType, double>();

        /// <summary>(beceri, eski seviye, yeni seviye)</summary>
        public event Action<SkillType, int, int> LevelChanged;

        public double GetXp(SkillType skill) => _xp.TryGetValue(skill, out var v) ? v : 0.0;

        public int GetLevel(SkillType skill) => SkillRules.LevelForXp(GetXp(skill));

        /// <summary>Kayıttan yükleme: XP'yi olduğu gibi koyar (olay tetiklemez).</summary>
        internal void RestoreXp(SkillType skill, double xp)
        {
            if (xp > 0) _xp[skill] = xp; else _xp.Remove(skill);
        }

        /// <summary>
        /// XP ekler; seviye tavanını aşan kısım kaybolur (XP tavan seviyenin sınırında durur).
        /// Eklenen gerçek XP'yi döner (tavan yüzünden 0 olabilir).
        /// </summary>
        public double AddXp(SkillType skill, double xp, int levelCap)
        {
            if (xp <= 0 || levelCap <= 0) return 0;
            if (levelCap > SkillRules.MaxLevel) levelCap = SkillRules.MaxLevel;

            double current = GetXp(skill);
            // Tavan seviyenin bir sonraki seviyeye çok az kala sınırı: sonraki eşiğin hemen altı.
            double ceiling = levelCap >= SkillRules.MaxLevel
                ? SkillRules.XpForLevel(SkillRules.MaxLevel) + 1000.0 // son seviyede XP birikmeye devam eder ama seviye artmaz
                : SkillRules.XpForLevel(levelCap + 1) - 0.0001;
            if (current >= ceiling) return 0;

            int before = SkillRules.LevelForXp(current);
            double next = Math.Min(current + xp, ceiling);
            _xp[skill] = next;
            int after = SkillRules.LevelForXp(next);
            if (after != before) LevelChanged?.Invoke(skill, before, after);
            return next - current;
        }
    }
}
