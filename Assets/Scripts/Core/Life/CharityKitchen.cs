using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Life
{
    /// <summary>Bereket Sofrası sonucu (belge 52 §5.5).</summary>
    public sealed class FeastResult
    {
        /// <summary>0: kuru ekmek, 1: ekmek ve çorba, 2: yahni, 3: yahni ve tatlı.</summary>
        public int Tier;
        /// <summary>Sofra sahibi: 0 oyuncunun hanesi, &gt;0 komşu hane kimliği, -1 kimse bağış yapmadı.</summary>
        public int HostKey = -1;
        public string HostName;
        public int Spent;
        public int MoodValue => Tier switch { 0 => 8, 1 => 12, 2 => 15, _ => 20 };
        public float Hunger => 25f + 15f * Tier;
    }

    /// <summary>
    /// Kasaba çınarı aşevinin kasası (belge 52 §5.4-5.5): bağışlar kasaya girer, hayır sofrasını ve mevsim sonu Bereket
    /// Sofrası'nı besler. Bağışçı hane itibar kazanır (haftalık tavanla); kasada en çok payı olan hane sofra sahibi olur.
    /// Saf mantık: zaman ve haneler dışarıdan verilir.
    /// </summary>
    public sealed class CharityKitchen
    {
        public const int PlayerKey = 0;
        public const int CheapestMeal = 8;
        public const int CharityCost = 2;
        public const int WeeklyRepCap = 10;
        public const int HostReputation = 5;
        public const int FeastMaxSpend = 150;
        /// <summary>Bolluk eşikleri: kasada bu kadar akçe varsa sofra bir basamak zengin.</summary>
        public static readonly int[] TierAt = { 0, 20, 60, 120 };
        /// <summary>Kasa doluyken hayır sofrası yahni olur (keyif de verir).</summary>
        public const int RichCharityAt = 30;

        public int Fund { get; private set; }
        public int WeekRep { get; private set; }
        public long WeekStart { get; private set; } = -1;
        private readonly Dictionary<int, int> _season = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        private readonly HashSet<int> _ateToday = new HashSet<int>();
        private long _ateDay = -1;
        public long FeastHeldDay { get; private set; } = -1;
        public int SeasonFedPoor { get; private set; }

        public IReadOnlyDictionary<int, int> SeasonDonations => _season;
        public int PlayerSeasonShare => _season.TryGetValue(PlayerKey, out int v) ? v : 0;

        /// <summary>Bağışın itibar karşılığı (10 → 1, 50 → 3, 100 → 5; diğerleri orantılı).</summary>
        public static int RepFor(int amount) => amount <= 0 ? 0 : amount >= 100 ? 5 + (amount - 100) / 40 : amount >= 50 ? 3 + (amount - 50) / 25 : Math.Max(1, amount / 10);

        /// <summary>Bağış: kasaya girer, mevsim payına yazılır. Oyuncunun bağışı için verilecek itibarı döner (haftalık tavan).</summary>
        public int Donate(int amount, long day, int donorKey = PlayerKey, string donorName = null)
        {
            if (amount <= 0) return 0;
            Fund += amount;
            _season[donorKey] = (_season.TryGetValue(donorKey, out int v) ? v : 0) + amount;
            if (donorName != null) _names[donorKey] = donorName;
            if (donorKey != PlayerKey) return 0;
            if (WeekStart < 0 || day - WeekStart >= 7) { WeekStart = day; WeekRep = 0; }
            int rep = Math.Min(RepFor(amount), Math.Max(0, WeeklyRepCap - WeekRep));
            WeekRep += rep;
            return rep;
        }

        /// <summary>Karakter bugün hayır sofrasına oturdu mu.</summary>
        public bool AteCharityToday(int characterId, long day) => _ateDay == day && _ateToday.Contains(characterId);

        /// <summary>Hayır sofrası: kasadan düşer; zengin sofra mı döner.</summary>
        public bool ServeCharity(int characterId, long day)
        {
            if (_ateDay != day) { _ateDay = day; _ateToday.Clear(); }
            _ateToday.Add(characterId);
            bool rich = Fund >= RichCharityAt;
            Fund = Math.Max(0, Fund - CharityCost);
            return rich;
        }

        /// <summary>Yoksul köylüler doydu (günlük); kasadan düşer, mevsim sayacına girer.</summary>
        public int FeedPoor(int count)
        {
            if (count <= 0) return 0;
            int fed = Fund > 0 ? count : 0;
            Fund = Math.Max(0, Fund - fed);
            SeasonFedPoor += fed;
            return fed;
        }

        public static bool IsFeastDay(int dayOfSeason, int daysPerSeason) => dayOfSeason == daysPerSeason - 1;

        public static int TierFor(int fund)
        {
            int t = 0;
            for (int i = 0; i < TierAt.Length; i++) if (fund >= TierAt[i]) t = i;
            return t;
        }

        /// <summary>Mevsimin son akşamı sofra kurulur: bolluk kasaya göre, sahibi en çok bağış yapan hane. Mevsim payları sıfırlanır.</summary>
        public FeastResult HoldFeast(long day)
        {
            var r = new FeastResult { Tier = TierFor(Fund) };
            r.Spent = Math.Min(Fund, FeastMaxSpend);
            Fund -= r.Spent;
            int best = 0;
            foreach (var kv in _season)
                if (kv.Value > best || (kv.Value == best && kv.Value > 0 && kv.Key == PlayerKey)) { best = kv.Value; r.HostKey = kv.Key; }
            if (best <= 0) r.HostKey = -1;
            r.HostName = r.HostKey >= 0 && _names.TryGetValue(r.HostKey, out var n) ? n : null;
            _season.Clear();
            SeasonFedPoor = 0;
            FeastHeldDay = day;
            return r;
        }

        public CharityKitchenSave Export()
        {
            var s = new CharityKitchenSave { fund = Fund, weekRep = WeekRep, weekStart = WeekStart, feastHeldDay = FeastHeldDay, fedPoor = SeasonFedPoor };
            var keys = new List<int>(); var vals = new List<int>(); var names = new List<string>();
            foreach (var kv in _season) { keys.Add(kv.Key); vals.Add(kv.Value); names.Add(_names.TryGetValue(kv.Key, out var nm) ? nm : ""); }
            s.donorKeys = keys.ToArray(); s.donorAmounts = vals.ToArray(); s.donorNames = names.ToArray();
            return s;
        }

        public void Import(CharityKitchenSave s)
        {
            _season.Clear(); _names.Clear(); _ateToday.Clear(); _ateDay = -1;
            if (s == null) { Fund = 0; WeekRep = 0; WeekStart = -1; FeastHeldDay = -1; SeasonFedPoor = 0; return; }
            Fund = Math.Max(0, s.fund); WeekRep = Math.Max(0, s.weekRep); WeekStart = s.weekStart; FeastHeldDay = s.feastHeldDay; SeasonFedPoor = Math.Max(0, s.fedPoor);
            if (s.donorKeys != null)
                for (int i = 0; i < s.donorKeys.Length; i++)
                {
                    _season[s.donorKeys[i]] = s.donorAmounts != null && i < s.donorAmounts.Length ? s.donorAmounts[i] : 0;
                    if (s.donorNames != null && i < s.donorNames.Length && !string.IsNullOrEmpty(s.donorNames[i])) _names[s.donorKeys[i]] = s.donorNames[i];
                }
        }
    }

    [Serializable]
    public sealed class CharityKitchenSave
    {
        public int fund;
        public int weekRep;
        public long weekStart = -1;
        public long feastHeldDay = -1;
        public int fedPoor;
        public int[] donorKeys;
        public int[] donorAmounts;
        public string[] donorNames;
    }
}
