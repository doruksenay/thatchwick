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
            if (Points != before) Changed?.Invoke(before, Points, "yeni kuşak");
        }

        public ReputationSave Export() => new ReputationSave { points = Points, log = _log.ToArray() };

        public void Import(ReputationSave s)
        {
            _log.Clear();
            Points = s?.points ?? 0;
            if (s?.log != null) _log.AddRange(s.log);
        }
    }

    [Serializable] public sealed class ReputationSave { public int points; public string[] log; }
}
