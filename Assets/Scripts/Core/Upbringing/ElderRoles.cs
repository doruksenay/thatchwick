using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>
    /// Yaşlı rolleri (belge 34-D4, onay 19.11.2026). Yaşlı ağır işte yavaştır ama:
    /// akşam ocak başında çocuklara masal anlatır, hanedeki bebeğe göz kulak olur (anne-babaya rahatlık)
    /// ve hafif işlerde (eğirme, dokuma, otacılık, sepet, tohum, öğretme) daha hızlıdır.
    /// Öğretme ve çıraklık katkısı ToyHabitSystem ve ApprenticeshipSystem'de.
    /// </summary>
    public sealed class ElderRoles
    {
        public const float StoryFun = 25f, StorySocial = 15f;
        public const double StoryLiteracyXp = 2, StoryCharmXp = 2;
        public const float BabyFun = 30f, BabySocial = 30f;
        public const double LightWorkSpeed = 1.25;

        /// <summary>Hafif işler: yaşlının hızlı olduğu beceriler.</summary>
        public static readonly HashSet<SkillType> LightSkills = new HashSet<SkillType>
        {
            SkillType.Weaving, SkillType.Herbalism, SkillType.Teaching, SkillType.Literacy,
            SkillType.Horticulture, SkillType.Chandlery, SkillType.Decorating, SkillType.Music
        };

        private readonly Population _population;

        public ElderRoles(Population population)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
        }

        public int StoriesTold { get; private set; }

        /// <summary>Yaşlının bu işte hız çarpanı (yaşlı değilse ya da ağır işse 1).</summary>
        public static double WorkSpeed(Character c, SkillType? skill) =>
            c != null && c.Stage == AgeStage.Elder && skill.HasValue && LightSkills.Contains(skill.Value) ? LightWorkSpeed : 1.0;

        private IEnumerable<Character> Household()
        {
            foreach (var m in _population.Members) if (!m.IsDead && !m.IsVillager) yield return m;
        }

        /// <summary>
        /// Akşam (gün dönümü) masalı: hanedeki her yaşlıdan biri, evdeki çocuk ve ergenlere masal anlatır.
        /// Dinleyen sayısını döner.
        /// </summary>
        public int TellEveningStories()
        {
            Character teller = null;
            var listeners = new List<Character>();
            foreach (var m in Household())
            {
                if (m.Stage == AgeStage.Elder && (teller == null || m.Skills.GetLevel(SkillType.Charm) > teller.Skills.GetLevel(SkillType.Charm))) teller = m;
                if (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) listeners.Add(m);
            }
            if (teller == null || listeners.Count == 0) return 0;
            foreach (var k in listeners)
            {
                k.Needs.Restore(NeedType.Fun, StoryFun);
                k.Needs.Restore(NeedType.Social, StorySocial);
                k.AddMoodlet("elder_story", $"{teller.Name}'in masalı", 2, 1.0);
                k.GainSkillXp(SkillType.Literacy, StoryLiteracyXp);
            }
            teller.Needs.Restore(NeedType.Social, StorySocial);
            teller.AddMoodlet("told_story", "Torunlarına masal anlattı", 2, 1.0);
            teller.GainSkillXp(SkillType.Charm, StoryCharmXp);
            teller.GainSkillXp(SkillType.Teaching, 1);
            StoriesTold++;
            return listeners.Count;
        }

        /// <summary>
        /// Torun bakımı: hanede yaşlı ve bebek varsa bebeğin keyfi ve sosyal ihtiyacı dolar, yaşlı Ebeveynlik XP alır,
        /// bebeğin anne-babasına "yaşlıya emanet" rahatlığı. Bakılan bebek sayısı.
        /// </summary>
        public int MindGrandchildren()
        {
            Character elder = null;
            var babies = new List<Character>();
            foreach (var m in Household())
            {
                if (m.Stage == AgeStage.Elder && elder == null) elder = m;
                if (m.Stage == AgeStage.Baby) babies.Add(m);
            }
            if (elder == null || babies.Count == 0) return 0;
            foreach (var b in babies)
            {
                b.Needs.Restore(NeedType.Fun, BabyFun);
                b.Needs.Restore(NeedType.Social, BabySocial);
                foreach (var pid in new[] { b.BirthMotherId, b.BirthFatherId })
                {
                    if (!pid.HasValue || pid.Value == elder.Id) continue;
                    var p = _population.Find(pid.Value);
                    if (p != null && !p.IsDead) p.AddMoodlet("elder_minds_baby", $"Bebek {elder.Name}'e emanet", 2, 1.0);
                }
            }
            elder.GainSkillXp(SkillType.Parenting, 2 * babies.Count);
            return babies.Count;
        }

        public void OnNewDay()
        {
            TellEveningStories();
            MindGrandchildren();
        }

        public int Export() => StoriesTold;
        public void Import(int stories) => StoriesTold = stories;
    }
}
