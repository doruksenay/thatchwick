using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Özellik ve meslek dalı kuralları (Adım 18a, 40): özellik atama/kalıtım, dal seçimi, etkilerin
    /// karaktere işlenmesi. Tohumlu rastgelelik kullanır (testte tekrarlanabilir).
    /// </summary>
    public static class LifeRules
    {
        public const int BranchLevelFirst = 5;
        public const int BranchLevelSecond = 10;

        /// <summary>Özellik, dal ve köy etkilerinden karakterin çarpanlarını baştan kurar.</summary>
        public static void Recompute(Character c, ContentDatabase db, VillageEffects village = null)
        {
            var m = new LifeModifiers();
            foreach (var id in c.TraitIds)
                if (db.Traits.TryGet(id, out var t)) m.Apply(t.Effects);
            foreach (var id in c.BranchIds)
                if (db.Branches.TryGet(id, out var b)) m.Apply(b.Effects);
            if (village != null) m.ColdResist = Math.Min(0.8f, m.ColdResist + village.ColdResist);
            c.Modifiers = m;
        }

        /// <summary>Çakışmayan en çok `count` özellik çeker (hariç tutulanlar dışında).</summary>
        public static List<string> RollTraits(ContentDatabase db, Random rng, int count, IEnumerable<string> have = null)
        {
            var result = new List<string>();
            if (have != null) result.AddRange(have);
            var pool = new List<TraitDef>(db.Traits.All);
            int guard = 0;
            while (result.Count < count && pool.Count > 0 && guard++ < 100)
            {
                var pick = pool[rng.Next(pool.Count)];
                pool.Remove(pick);
                if (CanAdd(db, result, pick.Id)) result.Add(pick.Id);
            }
            return result;
        }

        public static bool CanAdd(ContentDatabase db, IReadOnlyCollection<string> current, string traitId)
        {
            if (!db.Traits.TryGet(traitId, out var t)) return false;
            if (current.Count >= Character.MaxTraits) return false;
            foreach (var c in current)
            {
                if (c == traitId) return false;
                if (db.Traits.TryGet(c, out var other) && t.ConflictsWith(other)) return false;
            }
            return true;
        }

        /// <summary>
        /// Çocuk anneden ve babadan birer özellik alır; çakışırsa ikinci atlanır ve rastgele tamamlanır.
        /// Ebeveynin özelliği yoksa rastgele çekilir. Her zaman en az 1 özellik döner (içerik varsa).
        /// </summary>
        public static List<string> InheritTraits(ContentDatabase db, Random rng, Character mother, Character father)
        {
            var result = new List<string>();
            foreach (var parent in new[] { mother, father })
            {
                if (parent == null || parent.TraitIds.Count == 0) continue;
                var ids = new List<string>(parent.TraitIds);
                // Çocukta olmayan, çakışmayan bir tanesini rastgele seç.
                while (ids.Count > 0)
                {
                    var pick = ids[rng.Next(ids.Count)];
                    ids.Remove(pick);
                    if (CanAdd(db, result, pick)) { result.Add(pick); break; }
                }
            }
            if (result.Count == 0) result = RollTraits(db, rng, 1);
            return result;
        }

        /// <summary>Doğumda ömre eklenecek gün (özelliklerden).</summary>
        public static int LifespanBonus(ContentDatabase db, IEnumerable<string> traitIds)
        {
            int sum = 0;
            foreach (var id in traitIds) if (db.Traits.TryGet(id, out var t)) sum += t.Effects.LifespanDays;
            return sum;
        }

        // ---- meslek dalları ----

        /// <summary>Karakterin şu an seçebileceği dallar (beceri seviyesi yeter, kademe boş, gereken dal alınmış).</summary>
        public static List<BranchDef> AvailableBranches(ContentDatabase db, Character c)
        {
            var list = new List<BranchDef>();
            foreach (var b in db.Branches.All)
                if (CanChoose(db, c, b, out _)) list.Add(b);
            return list;
        }

        public static bool CanChoose(ContentDatabase db, Character c, BranchDef b, out string reason)
        {
            reason = null;
            if (c.IsDead) { reason = "Ölü karakter dal seçemez."; return false; }
            if (c.HasBranch(b.Id)) { reason = "Bu dal zaten seçildi."; return false; }
            if (c.Skills.GetLevel(b.Skill) < b.Level) { reason = $"{Thatchwick.Core.Jobs.JobRunner.SkillName(b.Skill)} seviye {b.Level} gerekir."; return false; }
            if (b.Requires != null && !c.HasBranch(b.Requires)) { reason = "Önce ilk kademe dal seçilmeli."; return false; }
            foreach (var id in c.BranchIds)
                if (db.Branches.TryGet(id, out var taken) && taken.Skill == b.Skill && taken.Level == b.Level)
                { reason = "Bu kademede zaten bir dal seçildi."; return false; }
            return true;
        }

        public static bool ChooseBranch(ContentDatabase db, Character c, string branchId, out string reason, VillageEffects village = null)
        {
            reason = null;
            if (!db.Branches.TryGet(branchId, out var b)) { reason = "Dal bulunamadı."; return false; }
            if (!CanChoose(db, c, b, out reason)) return false;
            c.AddBranch(b.Id);
            Recompute(c, db, village);
            return true;
        }

        public static string TradeName(SkillType skill)
        {
            switch (skill)
            {
                case SkillType.Farming: return "Çiftçi";
                case SkillType.Woodcutting: return "Oduncu";
                case SkillType.Cooking: return "Aşçı";
                default: return "Marangoz";
            }
        }

        /// <summary>Tek karakterin mesleği: en yüksek kademeli dalın becerisi; dal yoksa null.</summary>
        public static string TradeOf(ContentDatabase db, Character c)
        {
            BranchDef best = null;
            foreach (var id in c.BranchIds)
                if (db.Branches.TryGet(id, out var b) && (best == null || b.Level > best.Level)) best = b;
            return best == null ? null : TradeName(best.Skill);
        }

        /// <summary>Soy mesleği: hanedan başlarının en çok dal aldığı beceri (eşitlikte seviye toplamı); dal yoksa null.</summary>
        public static SkillType? DominantBranchSkill(ContentDatabase db, IEnumerable<Character> members)
        {
            var counts = new Dictionary<SkillType, int>();
            foreach (var c in members)
                foreach (var id in c.BranchIds)
                    if (db.Branches.TryGet(id, out var b))
                        counts[b.Skill] = (counts.TryGetValue(b.Skill, out var n) ? n : 0) + 1;
            SkillType? best = null; int bestCount = 0;
            foreach (var kv in counts) if (kv.Value > bestCount) { best = kv.Key; bestCount = kv.Value; }
            return best;
        }
    }
}
