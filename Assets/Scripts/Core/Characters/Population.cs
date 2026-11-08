using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Bir hanenin/köyün karakter topluluğu: hepsini ilerletir, doğumda bebek karakter üretir.
    /// Bebeğin kimliği, cinsiyeti ve ömrü burada belirlenir (rastgelelik tohumlanabilir = test edilebilir).
    /// </summary>
    public sealed class Population
    {
        private readonly List<Character> _members = new List<Character>();
        private readonly Random _rng;
        private int _nextId;

        public IReadOnlyList<Character> Members => _members;
        public Func<Sex, string> NameGenerator { get; set; }
        /// <summary>Verilirse bebeğin görünümü ebeveynlerinden türetilir; yoksa varsayılan görünüm kalır.</summary>
        public AppearanceCatalog AppearanceCatalog { get; set; }

        /// <summary>Verilirse bebek ebeveynlerinden birer özellik alır ve özellik etkileri hesaplanır.</summary>
        public ContentDatabase Content { get; set; }
        /// <summary>Köy projelerinden gelen etkiler (soğuk direnci gibi); karaktere işlenirken kullanılır.</summary>
        public VillageEffects Village { get; set; }

        /// <summary>(bebek, anne, babanın kimliği)</summary>
        public event Action<Character, Character, int?> BabyBorn;
        /// <summary>Üyelerden biri öldüğünde (Character.Died'ın topluluk düzeyi karşılığı).</summary>
        public event Action<Character> MemberDied;

        public Population(int firstFreeId = 1, int seed = 12345)
        {
            _nextId = firstFreeId;
            _rng = new Random(seed);
        }

        public int NextId() => _nextId++;

        public void Add(Character c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (_members.Contains(c)) return;
            _members.Add(c);
            c.GaveBirth += OnGaveBirth;
            c.Died += OnMemberDied;
            if (c.Id >= _nextId) _nextId = c.Id + 1;
        }

        public Character Find(int id)
        {
            foreach (var c in _members) if (c.Id == id) return c;
            return null;
        }

        /// <summary>Tüm karakterlerin biyolojisini ilerletir (ihtiyaç, yaş, hamilelik, ölüm).</summary>
        public void Tick(double gameDays)
        {
            // Doğumlar listeyi değiştirebilir; üzerinde dolaşırken kopya kullan.
            var snapshot = _members.ToArray();
            foreach (var c in snapshot) c.Tick(gameDays);
        }

        private void OnMemberDied(Character c) => MemberDied?.Invoke(c);

        /// <summary>Tüm üyelerin özellik/dal etkilerini yeniden hesaplar (köy projesi bitince, yüklemeden sonra).</summary>
        public void RecomputeAll()
        {
            if (Content == null) return;
            foreach (var c in _members) LifeRules.Recompute(c, Content, Village);
        }

        private void OnGaveBirth(Character mother, int? fatherId)
        {
            var sex = _rng.Next(2) == 0 ? Sex.Female : Sex.Male;
            string name = NameGenerator != null ? NameGenerator(sex) : (sex == Sex.Female ? "Kız" : "Oğlan");
            if (string.IsNullOrWhiteSpace(name)) name = "Bebek";

            var father0 = fatherId.HasValue ? Find(fatherId.Value) : null;
            var baby = new Character(NextId(), name, mother.Position, false)
            {
                Sex = sex,
                AgeConfig = mother.AgeConfig,
                BirthMotherId = mother.Id,
                BirthFatherId = fatherId,
                Generation = Math.Max(mother.Generation, father0 != null ? father0.Generation : 0) + 1,
                IsVillager = mother.IsVillager,
                CanStarve = mother.CanStarve
            };
            baby.LifespanDays = mother.AgeConfig.RollLifespan(_rng);
            if (Content != null && Content.Traits.Count > 0)
            {
                baby.SetTraits(LifeRules.InheritTraits(Content, _rng, mother, father0));
                baby.LifespanDays += LifeRules.LifespanBonus(Content, baby.TraitIds);
                LifeRules.Recompute(baby, Content, Village);
            }
            if (AppearanceCatalog != null)
            {
                var father = fatherId.HasValue ? Find(fatherId.Value) : null;
                baby.Appearance = father != null
                    ? AppearanceCatalog.Inherit(mother.Appearance, father.Appearance, _rng, sex)
                    : AppearanceCatalog.Randomize(_rng, sex);
            }
            Add(baby);
            BabyBorn?.Invoke(baby, mother, fatherId);
        }
    }
}
