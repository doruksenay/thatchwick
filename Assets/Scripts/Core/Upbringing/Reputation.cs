using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>
    /// Aile itibarı (belge 34, D3). Kaliteli iş, şenlik, köy projesi, düğün, çırak yetiştirme ve aile tarifi yükseltir;
    /// bunalım ve kavga, yavrulu hayvan avlamak düşürür. Etkisi: pazarda satış fiyatı (en çok +%10, en az −%5).
    /// Yeni hane başına geçişte yarısı kalır (yarısı miras).
    /// </summary>
    public sealed class ReputationSystem
    {
        public const int Max = 1000, Min = -200;
        public const double MaxSellBonus = 0.10, MaxSellPenalty = 0.05;
        public const int LogSize = 12;

        public int Points { get; private set; }
        private readonly List<string> _log = new List<string>();
        public IReadOnlyList<string> RecentLog => _log;

        public event Action<int, int, string> Changed;

        public static class Reasons
        {
            public const int Festival = 3, Project = 15, Wedding = 10, Graduation = 10, FamilyRecipe = 8,
                Breakdown = -3, NursingMotherHunted = -10, CompanionTrait = 4, RecipeLost = -5;
        }

        /// <summary>Kazanılan itibar çarpanı (belge 45: Cazibe 10 "köy meclisinde oy" ×1,1); null ise 1. Kayıp etkilenmez.</summary>
        public Func<double> GainFactor { get; set; }

        public void Add(int amount, string reason)
        {
            if (amount > 0 && GainFactor != null) amount = (int)Math.Round(amount * GainFactor(), MidpointRounding.AwayFromZero);
            if (amount == 0) return;
            int before = Points;
            Points = Math.Max(Min, Math.Min(Max, Points + amount));
            if (Points == before) return;
            _log.Insert(0, $"{(amount > 0 ? "+" : "")}{amount} {reason}");
            if (_log.Count > LogSize) _log.RemoveAt(_log.Count - 1);
            Changed?.Invoke(before, Points, reason);
        }

        public string TierName =>
            Points >= 600 ? "Ünlü" : Points >= 300 ? "Tanınmış" : Points >= 100 ? "Saygın" : Points >= 0 ? "Bilinmez" : "Adı kötüye çıkmış";

        /// <summary>Satış fiyatına eklenecek oran.</summary>
        public double SellBonus() =>
            Points >= 0 ? MaxSellBonus * Points / Max : -MaxSellPenalty * Points / (double)Min;

        /// <summary>Hane başına yeni biri geçti: itibarın yarısı kalır.</summary>
        public void OnSuccession()
        {
            int before = Points;
            Points /= 2;
            for (int i = 0; i < FacetCount; i++) _facets[i] /= 2;
            if (Points != before) Changed?.Invoke(before, Points, "yeni kuşak");
        }

        // ---- dört yön, kişi namı, lakap (Project belgesi 67b §7.3, karar 67c) ----

        public const int FacetCount = 4;
        /// <summary>Lakap eşiği: kişinin bir yöndeki namı bunu geçerse (ya da eksi tarafta) köy lakap takar.</summary>
        public const int NicknameAt = 25;
        private readonly int[] _facets = new int[FacetCount];
        private readonly Dictionary<int, int[]> _persons = new Dictionary<int, int[]>();

        /// <summary>Yön puanı (0 Sözünün eri, 1 Eli açık, 2 Ustalık, 3 Huzur). Eski "yönsüz" itibar toplamda sayılır, yönlerde değil.</summary>
        public int Facet(int facet) => facet >= 0 && facet < FacetCount ? _facets[facet] : 0;

        /// <summary>Yönlü itibar: hane toplamına, yöne ve (verilmişse) kişinin namına yazılır.</summary>
        public void Add(int amount, string reason, int facet, int personId = 0)
        {
            if (facet < 0 || facet >= FacetCount) { Add(amount, reason); return; }
            int before = Points;
            Add(amount, reason);
            _facets[facet] += Points - before;
            if (personId != 0 && amount != 0)
            {
                if (!_persons.TryGetValue(personId, out var arr)) { arr = new int[FacetCount]; _persons[personId] = arr; }
                arr[facet] += amount;
            }
        }

        public int PersonFacet(int personId, int facet) =>
            _persons.TryGetValue(personId, out var arr) && facet >= 0 && facet < FacetCount ? arr[facet] : 0;

        /// <summary>Kişinin lakabı: en baskın yön (yön, olumlu mu); yoksa null.</summary>
        public (int facet, bool good)? Nickname(int personId)
        {
            if (!_persons.TryGetValue(personId, out var arr)) return null;
            int best = -1, bestAbs = NicknameAt - 1;
            for (int i = 0; i < FacetCount; i++) if (Math.Abs(arr[i]) > bestAbs) { bestAbs = Math.Abs(arr[i]); best = i; }
            if (best < 0) return null;
            return (best, arr[best] > 0);
        }

        /// <summary>Çocuk, anne ve babasının namının yarısıyla başlar (67b §7.3-4).</summary>
        public void InheritNam(int childId, int motherId, int fatherId)
        {
            var arr = new int[FacetCount];
            bool any = false;
            foreach (var pid in new[] { motherId, fatherId })
                if (pid != 0 && _persons.TryGetValue(pid, out var p)) { any = true; for (int i = 0; i < FacetCount; i++) arr[i] += p[i] / 4; }
            if (any) _persons[childId] = arr;
        }

        public ReputationSave Export()
        {
            var persons = new List<int>();
            foreach (var kv in _persons) { persons.Add(kv.Key); persons.AddRange(kv.Value); }
            return new ReputationSave { points = Points, log = _log.ToArray(), facets = (int[])_facets.Clone(), persons = persons.ToArray() };
        }

        public void Import(ReputationSave s)
        {
            _log.Clear();
            _persons.Clear();
            Array.Clear(_facets, 0, FacetCount);
            Points = s?.points ?? 0;
            if (s?.log != null) _log.AddRange(s.log);
            if (s?.facets != null) for (int i = 0; i < Math.Min(FacetCount, s.facets.Length); i++) _facets[i] = s.facets[i];
            if (s?.persons != null)
                for (int i = 0; i + FacetCount < s.persons.Length; i += FacetCount + 1)
                {
                    var arr = new int[FacetCount];
                    Array.Copy(s.persons, i + 1, arr, 0, FacetCount);
                    _persons[s.persons[i]] = arr;
                }
        }
    }

    [Serializable] public sealed class ReputationSave { public int points; public string[] log; public int[] facets; public int[] persons; }
}
