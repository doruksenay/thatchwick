using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Mood;
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
        /// Doğumda tek özellik (karar 18.11.2026): rastgele bir ebeveynden bir özellik; o ebeveynin özelliği yoksa öbüründen,
        /// ikisinin de yoksa rastgele. İkinci yuva çocuklukta dolar (ApplyUpbringing, Genç yetişkin'e geçişte).
        /// </summary>
        public static List<string> InheritTraits(ContentDatabase db, Random rng, Character mother, Character father)
        {
            var result = new List<string>();
            var parents = rng.Next(2) == 0 ? new[] { mother, father } : new[] { father, mother };
            foreach (var parent in parents)
            {
                var pick = PickParentTrait(db, rng, parent, result);
                if (pick != null) { result.Add(pick); break; }
            }
            if (result.Count == 0) result = RollTraits(db, rng, 1);
            return result;
        }

        // Ebeveynin, listedekilerle çakışmayan rastgele bir özelliği; yoksa null.
        private static string PickParentTrait(ContentDatabase db, Random rng, Character parent, List<string> current)
        {
            if (parent == null || parent.TraitIds.Count == 0) return null;
            var ids = new List<string>(parent.TraitIds);
            while (ids.Count > 0)
            {
                var pick = ids[rng.Next(ids.Count)];
                ids.Remove(pick);
                if (CanAdd(db, current, pick)) return pick;
            }
            return null;
        }

        /// <summary>
        /// Çocukluk ruh hali özelliğe dönüşür (karar 18.11.2026, Project belgesi 23 bölüm 7.3). Ergen'den Genç yetişkin'e
        /// geçişte çağrılır. Çocuk ve Ergen boyunca ortalama İyi ve üstüyse Neşeli, Mutsuz ve altıysa Kasvetli; arada ise
        /// (ya da ölçü yetersizse) ebeveyninden bir özellik daha (önce doğumda özellik vermeyen ebeveynden).
        /// Kalıtım özelliği çocukluk özelliğiyle çakışırsa çocukluk kazanır ve hanedan geçmişine yazılır.
        /// Özellik yuvaları doluysa ve çakışma yoksa değişmez. Hayat notunu döner (değişiklik yoksa null).
        /// </summary>
        public static string ApplyUpbringing(ContentDatabase db, Random rng, Character c, Character mother, Character father)
        {
            if (db == null || c == null || c.IsDead) return null;
            string grown = MoodBehavior.UpbringingTrait(c.UpbringingAverage, c.UpbringingDays);
            var traits = new List<string>(c.TraitIds);
            string note = null;

            if (grown != null && db.Traits.TryGet(grown, out var grownDef))
            {
                if (traits.Contains(grown)) return null;
                string conflict = null;
                foreach (var id in traits)
                    if (db.Traits.TryGet(id, out var other) && grownDef.ConflictsWith(other)) { conflict = id; break; }
                bool happy = grown == MoodBehavior.TraitCheerful;
                if (conflict != null)
                {
                    traits.Remove(conflict);
                    string lineage = db.Traits.TryGet(conflict, out var cd) ? cd.DisplayName : conflict;
                    note = $"{lineage} bir soydan geldi ama {(happy ? "mutlu" : "zor")} bir çocukluk geçirdi.";
                    c.LifespanDays -= LifespanBonus(db, new[] { conflict });
                }
                else if (traits.Count >= Character.MaxTraits) return null;
                else note = happy ? $"Mutlu bir çocukluk geçirdi: {grownDef.DisplayName} büyüdü." : $"Zor bir çocukluk geçirdi: {grownDef.DisplayName} büyüdü.";
                traits.Add(grown);
                c.LifespanDays += LifespanBonus(db, new[] { grown });
            }
            else
            {
                if (traits.Count >= Character.MaxTraits) return null;
                // Ebeveyn huyu: önce henüz huyunu vermemiş ebeveynden.
                string pick = null;
                foreach (var parent in new[] { mother, father })
                {
                    if (parent == null) continue;
                    bool alreadyGave = false;
                    foreach (var id in parent.TraitIds) if (traits.Contains(id)) { alreadyGave = true; break; }
                    if (alreadyGave) continue;
                    pick = PickParentTrait(db, rng, parent, traits);
                    if (pick != null) break;
                }
                if (pick == null) foreach (var parent in new[] { mother, father }) { pick = PickParentTrait(db, rng, parent, traits); if (pick != null) break; }
                if (pick == null) { var rolled = RollTraits(db, rng, traits.Count + 1, traits); if (rolled.Count > traits.Count) pick = rolled[rolled.Count - 1]; }
                if (pick == null) return null;
                traits.Add(pick);
                c.LifespanDays += LifespanBonus(db, new[] { pick });
                note = db.Traits.TryGet(pick, out var pd) ? $"Ailesinden bir huy aldı: {pd.DisplayName}." : null;
            }

            c.SetTraits(traits);
            if (note != null) c.AddChronicle(note);
            return note;
        }

        // ---- yatkınlık (karar 18.11.2026, Project belgesi 28) ----

        /// <summary>Ebeveynin her yatkınlığının çocuğa geçme şansı.</summary>
        public const double AptitudeInheritChance = 0.35;
        /// <summary>"Çok yatkın" geçtiğinde öyle kalma şansı (yoksa "Yatkın" olarak geçer).</summary>
        public const double MajorStaysMajorChance = 0.5;
        /// <summary>Çocuklukta yatkınlık doğması için en çok çalışılan becerinin ulaşması gereken seviye.</summary>
        public const int ChildhoodAptitudeLevel = 3;

        /// <summary>Doğumda yatkınlık: her ebeveynin her yatkınlığı %35 şansla geçer (en çok 3).</summary>
        /// <summary>Doğumda kalıtılabilecek en çok yatkınlık (belge 45 karar 3).</summary>
        public const int MaxInheritedAptitudes = 1;

        public static void InheritAptitudes(Random rng, Character child, Character mother, Character father)
        {
            if (child == null || rng == null) return;
            // Belge 45 karar 3: doğumda en çok 1 yatkınlık kalıtılır (ilk başarılı zarda durulur). İkinci yuva
            // çocuklukta (ApplyChildhoodAptitude), üçüncüsü yalnızca olayla (çıraklık mezuniyeti, nitelikli talip) dolar.
            foreach (var parent in new[] { mother, father })
            {
                if (parent == null) continue;
                foreach (var kv in new List<KeyValuePair<SkillType, Aptitude>>(parent.Skills.Aptitudes))
                {
                    if (rng.NextDouble() >= AptitudeInheritChance) continue;
                    var a = kv.Value == Aptitude.Major && rng.NextDouble() < MajorStaysMajorChance ? Aptitude.Major : Aptitude.Minor;
                    if (child.Skills.SetAptitude(kv.Key, a) && child.Skills.AptitudeCount >= MaxInheritedAptitudes) return;
                }
            }
        }

        /// <summary>Dışarıdan gelen karakter (talip, köylü) için rastgele yatkınlık: n tane, %20'si "Çok yatkın".</summary>
        public static void RollAptitudes(Random rng, Character c, int n)
        {
            if (c == null || rng == null) return;
            var all = (SkillType[])Enum.GetValues(typeof(SkillType));
            int guard = 0;
            while (c.Skills.AptitudeCount < Math.Min(n, SkillPacing.MaxAptitudes) && guard++ < 50)
            {
                var s = all[rng.Next(all.Length)];
                if (c.Skills.GetAptitude(s) != Aptitude.None) continue;
                c.Skills.SetAptitude(s, rng.NextDouble() < 0.2 ? Aptitude.Major : Aptitude.Minor);
            }
        }

        /// <summary>
        /// Çocuklukta sık yapılan iş yatkınlığa dönüşür (Genç yetişkin'e geçişte): en çok XP'li beceri en az 3. seviyedeyse
        /// ve yuva varsa "Yatkın" olur (zaten yatkınsa "Çok yatkın"a çıkar). Hayat notunu döner; değişiklik yoksa null.
        /// </summary>
        public static string ApplyChildhoodAptitude(Character c)
        {
            if (c == null || c.IsDead) return null;
            SkillType best = SkillType.Farming; double bestXp = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                double xp = c.Skills.GetXp(s);
                if (xp > bestXp) { bestXp = xp; best = s; }
            }
            if (bestXp <= 0 || c.Skills.GetLevel(best) < ChildhoodAptitudeLevel) return null;
            var cur = c.Skills.GetAptitude(best);
            var want = cur == Aptitude.None ? Aptitude.Minor : Aptitude.Major;
            if (cur == Aptitude.Major || !c.Skills.SetAptitude(best, want)) return null;
            string note = want == Aptitude.Major
                ? $"Çocukluktan beri {SkillNames.Name(best)} ile büyüdü: artık çok yatkın."
                : $"Çocukken çok {SkillNames.Name(best).ToLowerInvariant()} işi yaptı: yatkınlık kazandı.";
            c.AddChronicle(note);
            return note;
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

        public static string TradeName(SkillType skill) => SkillNames.TradeName(skill);

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
