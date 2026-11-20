using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Eşyanın kilit becerisi: eşyayı bir tarif üretiyorsa o tarifin becerisi (küp Çömlekçilik, mum Mumculuk,
        /// eyer Saraçlık); tarifi yoksa kategoriye göre (belge 45, 3.1).
        /// </summary>
        public static SkillType SkillFor(ItemDef d)
        {
            if (d?.CraftSkill != null) return d.CraftSkill.Value;
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
                default: return SkillType.Carpentry;   // tarifsiz oturma, yüzey, depo, yatak, hijyen, mobilya
            }
        }

        /// <summary>Yapı parçasının kilit becerisi: kapı/pencere/çit Marangozluk, öteki Taş ustalığı.</summary>
        public static SkillType SkillFor(BuildPartDef p)
        {
            if (p == null) return SkillType.Masonry;
            if (p.Skill.HasValue) return p.Skill.Value;
            if (p.Kind == BuildPartKind.ItemFinish)
                switch (p.Group)
                {
                    case "wood": return SkillType.Carpentry;
                    case "cloth": return SkillType.Weaving;
                    case "metal": return SkillType.Smithing;
                    case "clay": return SkillType.Pottery;
                    default: return SkillType.Masonry;   // taş
                }
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

        /// <summary>
        /// Bir beceri bu seviyeye çıkınca oyunda gerçekten açılanlar (seviye kartı, belge 45 bölüm 10): tablodaki
        /// "var" satırları + bu seviyeyi isteyen tarifler + kilidi bu beceri ve seviyede olan eşyalar. Planlanmış
        /// (henüz kodu olmayan) satırlar ve paketsiz +18 satırları girmez. En çok <paramref name="max"/> kalem.
        /// </summary>
        public static List<string> NewAtLevel(ContentDatabase db, SkillType skill, int level, int max = 6)
        {
            var list = new List<string>();
            if (db == null || level < 1) return list;
            void Add(string t)
            {
                if (string.IsNullOrWhiteSpace(t) || list.Contains(t)) return;
                list.Add(t);
            }
            foreach (var u in db.SkillUnlocks)
                if (u.Skill == skill && u.Level == level && !u.Planned && (!u.AdultPack || ContentPacks.Adult)) Add(u.Text);
            foreach (var d in db.Items.All)
            {
                if (d.IsRetired) continue;
                foreach (var i in d.Interactions)
                    if (i.IsCraft && i.Skill == skill && i.MinSkillLevel == level && !i.Hidden && i.Produce.Count > 0)
                        Add(db.Items.TryGet(i.Produce[0].ItemId, out var made) ? made.DisplayName : i.DisplayName);
                if (level > 1 && d.UnlockLevel == level && d.Placement != ItemPlacement.Resource && SkillFor(d) == skill) Add(d.DisplayName);
            }
            if (list.Count > max) { int more = list.Count - max; list.RemoveRange(max, more); list.Add($"+{more} daha"); }
            return list;
        }

        /// <summary>
        /// Beceri panelinin 1-10 şeridi için bir seviyenin özeti: oyunda olanlar önce; yoksa tablodaki planlanmış satır
        /// "(yakında)" ekiyle. Boşsa boş dizgi.
        /// </summary>
        public static string LevelSummary(ContentDatabase db, SkillType skill, int level)
        {
            var now = NewAtLevel(db, skill, level, 3);
            if (now.Count > 0) return string.Join(", ", now);
            if (db == null) return "";
            foreach (var u in db.SkillUnlocks)
                if (u.Skill == skill && u.Level == level && (!u.AdultPack || ContentPacks.Adult)) return u.Text + " (yakında)";
            return "";
        }

        /// <summary>Zanaatkâr defteri hanede herhangi bir beceri bu seviyeye çıkınca açılır (belge 45, 3.3; önce hesap seviyesi 5).</summary>
        public const int NotebookSkillLevel = 3;

        /// <summary>
        /// Zanaatkâr defteri açık mı: beceri sağlayıcı varsa hanenin herhangi bir becerisi 3+; yoksa eski hesap seviyesi kuralı.
        /// </summary>
        public static bool NotebookOpen(Func<SkillType, int> skillLevel, int accountLevel, int legacyAccountLevel = 5)
        {
            if (skillLevel == null) return accountLevel >= legacyAccountLevel;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                if (skillLevel(s) >= NotebookSkillLevel) return true;
            return false;
        }

        /// <summary>Kilit ipucu ("Marangozluk 4 gerekli." ya da eski "Hesap seviyesi 4 gerekli.").</summary>
        public static string Text(Func<SkillType, int> skillLevel, SkillType skill, int unlockLevel)
            => skillLevel != null ? $"{SkillNames.Name(skill)} {LevelFor(unlockLevel)} gerekli." : $"Hesap seviyesi {unlockLevel} gerekli.";
    }
}
