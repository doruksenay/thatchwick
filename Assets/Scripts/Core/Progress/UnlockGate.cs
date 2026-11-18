using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Progress
{
    /// <summary>
    /// İçerik kilitleri beceride (karar 18.11.2026, Project belgesi 28 karar 4): eşya, yapı parçası, yol, su yolu,
    /// alet ve hayvan; içeriğin eski "seviye" değeri artık ilgili becerinin hanedeki en iyi seviyesi olarak okunur
    /// (en çok 10). Beceri sağlayıcı verilmemişse eski hesap seviyesi kuralı sürer (eski sınamalar, araçlar).
    /// Hesap seviyesi yalnızca genel ilerleme ve ödül olarak kalır.
    /// </summary>
    public static class UnlockGate
    {
        public static int LevelFor(int unlockLevel) => Math.Max(1, Math.Min(SkillRules.MaxLevel, unlockLevel));

        /// <summary>Eşyanın kilit becerisi: kategoriye göre.</summary>
        public static SkillType SkillFor(ItemDef d)
        {
            switch (d?.Category)
            {
                case "rug": return SkillType.Weaving;
                case "decor": case "decor_wall": case "ceiling": case "kids": return SkillType.Decorating;
                case "farm": case "garden": return SkillType.Farming;
                case "barn": return SkillType.AnimalHusbandry;
                case "kitchen": return SkillType.Cooking;
                case "heat_light": return SkillType.Chandlery;
                case "structure": return SkillType.Masonry;
                case "utility": return SkillType.Engineering;
                case "work": return SkillType.Smithing;
                case "resource": return SkillType.Trade;
                default: return SkillType.Carpentry;   // oturma, yüzey, depo, yatak, hijyen, mobilya
            }
        }

        /// <summary>Yapı parçasının kilit becerisi: kapı/pencere/çit Marangozluk, öteki Taş ustalığı.</summary>
        public static SkillType SkillFor(BuildPartDef p)
        {
            if (p == null) return SkillType.Masonry;
            switch (p.Kind)
            {
                case BuildPartKind.Door: case BuildPartKind.Window: case BuildPartKind.Fence: return SkillType.Carpentry;
                default: return SkillType.Masonry;
            }
        }

        /// <summary>Açık mı: beceri sağlayıcı varsa beceriye, yoksa hesap seviyesine bakar.</summary>
        public static bool IsOpen(Func<SkillType, int> skillLevel, Func<int> accountLevel, SkillType skill, int unlockLevel)
        {
            if (unlockLevel <= 1) return true;
            if (skillLevel != null) return skillLevel(skill) >= LevelFor(unlockLevel);
            return accountLevel == null || accountLevel() >= unlockLevel;
        }

        /// <summary>Kilit ipucu ("Marangozluk 4 gerekli." ya da eski "Hesap seviyesi 4 gerekli.").</summary>
        public static string Text(Func<SkillType, int> skillLevel, SkillType skill, int unlockLevel)
            => skillLevel != null ? $"{SkillNames.Name(skill)} {LevelFor(unlockLevel)} gerekli." : $"Hesap seviyesi {unlockLevel} gerekli.";
    }
}
