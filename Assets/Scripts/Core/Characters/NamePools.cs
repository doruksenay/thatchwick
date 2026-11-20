using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Ad havuzları (Project belgesi 61b §5): sabit köy kadrosundan sonraki kuşaklar için her köylü hanesinin kendi kısa
    /// ad listesi ve oyuncu hanesi, talipler ve adsız komşular için genel havuz. Adlar her dilde aynıdır (çevrilmez).
    /// Veri life.json'dan gelir (köylü kaydında maleNames ve femaleNames, kökte namePool).
    /// Sıra: hanenin listesinde yaşayan biriyle çakışmayan ilk ad, sonra genel havuz (sırayla), o da biterse sıra eki ("Timur II").
    /// </summary>
    public sealed class NamePools
    {
        private readonly Dictionary<string, (List<string> male, List<string> female)> _households =
            new Dictionary<string, (List<string> male, List<string> female)>(StringComparer.Ordinal);
        private readonly List<string> _male = new List<string>();
        private readonly List<string> _female = new List<string>();
        private int _counter;

        public IReadOnlyList<string> GeneralMale => _male;
        public IReadOnlyList<string> GeneralFemale => _female;

        public void SetGeneral(IEnumerable<string> male, IEnumerable<string> female)
        {
            _male.Clear(); _female.Clear();
            AddClean(_male, male); AddClean(_female, female);
        }

        public void SetHousehold(string householdId, IEnumerable<string> male, IEnumerable<string> female)
        {
            if (string.IsNullOrWhiteSpace(householdId)) return;
            var m = new List<string>(); var f = new List<string>();
            AddClean(m, male); AddClean(f, female);
            if (m.Count == 0 && f.Count == 0) { _households.Remove(householdId); return; }
            _households[householdId] = (m, f);
        }

        public bool HasHousehold(string householdId) => householdId != null && _households.ContainsKey(householdId);

        /// <summary>
        /// Yeni ad. <paramref name="householdId"/> köylü tanımının kimliği (villager.orhan gibi) ya da null (genel havuz).
        /// <paramref name="taken"/> yaşayanların adları; aynı ad verilmez.
        /// </summary>
        public string Pick(Sex sex, string householdId, ICollection<string> taken)
        {
            taken = taken ?? new HashSet<string>(StringComparer.Ordinal);
            if (householdId != null && _households.TryGetValue(householdId, out var h))
                foreach (var n in sex == Sex.Female ? h.female : h.male)
                    if (!taken.Contains(n)) return n;

            var pool = sex == Sex.Female ? _female : _male;
            if (pool.Count == 0) return null;
            for (int i = 0; i < pool.Count; i++)
            {
                string n = pool[_counter++ % pool.Count];
                if (!taken.Contains(n)) return n;
            }
            string baseName = pool[_counter++ % pool.Count];
            for (int k = 2; ; k++)
            {
                string n = baseName + " " + Roman(k);
                if (!taken.Contains(n)) return n;
            }
        }

        private static void AddClean(List<string> into, IEnumerable<string> names)
        {
            if (names == null) return;
            foreach (var n in names)
            {
                var t = n?.Trim();
                if (!string.IsNullOrEmpty(t) && !into.Contains(t)) into.Add(t);
            }
        }

        public static string Roman(int n)
        {
            var map = new[] { (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I") };
            var s = new System.Text.StringBuilder();
            foreach (var (v, r) in map) while (n >= v) { s.Append(r); n -= v; }
            return s.ToString();
        }
    }
}
