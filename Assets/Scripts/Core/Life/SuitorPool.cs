using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Family;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Talip havuzu (Adım 18d, T18): hane üyesi ne kadar akraba çıkarsa çıksın evlenecek aday tükenmesin.
    /// Her bekâr genç/yetişkin hane üyesi için karşı cinsten, yakın akrabası olmayan en az 3 bekâr köylü
    /// bulundurulur; eksikse yeni köylü gelir. Ayrıca her mevsim başında bir köylü gelir.
    /// </summary>
    public sealed class SuitorPool
    {
        public const int MinCandidates = 3;
        public const int MaxVillagers = 18;
        public const double ArrivalAgeMin = 24.5, ArrivalAgeMax = 34;

        private readonly ContentDatabase _content;
        private readonly Population _population;
        private readonly Random _rng;

        public AppearanceCatalog AppearanceCatalog { get; set; }
        /// <summary>Yeni köylünün doğacağı karo (oyun katmanı pazar çevresini verir).</summary>
        public Func<GridPos> SpawnPosition { get; set; } = () => new GridPos(0, 0);
        public event Action<Character> Arrived;

        public SuitorPool(ContentDatabase content, Population population, int seed = 101)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _rng = new Random(seed);
        }

        /// <summary>Genç ya da yetişkin, bekâr, yaşayan.</summary>
        public static bool IsSingleAdult(Character c) =>
            !c.IsDead && !c.SpouseId.HasValue && (c.Stage == AgeStage.Youth || c.Stage == AgeStage.Adult);

        public static bool IsEligible(Character household, Character villager) =>
            villager != household && villager.IsVillager && IsSingleAdult(villager) && villager.Sex != household.Sex
            && !FamilyService.AreCloseRelatives(household, villager);

        public List<Character> EligibleFor(Character household)
        {
            var list = new List<Character>();
            foreach (var v in _population.Members) if (IsEligible(household, v)) list.Add(v);
            return list;
        }

        public int VillagerCount()
        {
            int n = 0;
            foreach (var c in _population.Members) if (!c.IsDead && c.IsVillager) n++;
            return n;
        }

        /// <summary>Eksik adayları tamamlar; gelen köylü sayısını döner.</summary>
        public int Ensure()
        {
            int arrivals = 0;
            foreach (var h in _population.Members.ToArrayCopy())
            {
                if (h.IsDead || h.IsVillager || h.Stage == AgeStage.Baby) continue;
                if (h.SpouseId.HasValue) continue;
                var otherSex = h.Sex == Sex.Female ? Sex.Male : Sex.Female;
                int have = EligibleFor(h).Count;
                while (have < MinCandidates && VillagerCount() < MaxVillagers)
                {
                    SpawnVillager(otherSex);
                    arrivals++;
                    have++;
                }
            }
            return arrivals;
        }

        /// <summary>Mevsim başında rastgele cinsiyetten bir köylü gelir (üst sınıra kadar).</summary>
        public Character OnNewSeason()
        {
            if (VillagerCount() >= MaxVillagers) return null;
            return SpawnVillager(_rng.Next(2) == 0 ? Sex.Female : Sex.Male);
        }

        public Character SpawnVillager(Sex sex)
        {
            string name = _population.NameGenerator != null ? _population.NameGenerator(sex) : (sex == Sex.Female ? "Köylü Kadın" : "Köylü Adam");
            if (string.IsNullOrWhiteSpace(name)) name = "Köylü";
            var c = new Character(_population.NextId(), name, SpawnPosition())
            {
                Sex = sex,
                IsVillager = true,
                CanStarve = false
            };
            c.SetAge(ArrivalAgeMin + _rng.NextDouble() * (ArrivalAgeMax - ArrivalAgeMin));
            c.LifespanDays = c.AgeConfig.RollLifespan(_rng);
            if (AppearanceCatalog != null) c.Appearance = AppearanceCatalog.Randomize(_rng, sex);
            if (_content.Traits.Count > 0)
            {
                c.SetTraits(LifeRules.RollTraits(_content, _rng, 1 + _rng.Next(2)));
                c.LifespanDays += LifeRules.LifespanBonus(_content, c.TraitIds);
                LifeRules.Recompute(c, _content, _population.Village);
            }
            _population.Add(c);
            Arrived?.Invoke(c);
            return c;
        }
    }

    internal static class ListCopyExtensions
    {
        public static T[] ToArrayCopy<T>(this IReadOnlyList<T> list)
        {
            var a = new T[list.Count];
            for (int i = 0; i < a.Length; i++) a[i] = list[i];
            return a;
        }
    }
}
