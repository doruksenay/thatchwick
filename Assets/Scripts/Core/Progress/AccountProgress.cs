using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Progress
{
    /// <summary>Rozet: bir sayaç belirli değere ulaşınca bir kez kazanılır ve XP verir.</summary>
    public sealed class BadgeDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Counter { get; set; }
        public int Target { get; set; }
        public int RewardXp { get; set; } = 100;
    }

    /// <summary>
    /// Hesap seviyesi (yerel; Adım 23'te buluta bağlanır). XP kaynakları (T9):
    /// günlük işler (oyun günü başına tavanlı), beceri seviye atlamaları, "ilk kez" olayları ve rozetler.
    /// Satın alınamaz. Seviye, dükkânda açılan eşya çeşidini belirler (T10).
    /// </summary>
    public sealed class AccountProgress
    {
        public const int MaxLevel = 20;
        public const int WorkXp = 5;
        public const int DailyWorkCap = 150;
        public const int SkillLevelXp = 25;
        public const int FirstTimeXp = 50;

        private readonly HashSet<string> _firsts = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _badges = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _counters = new Dictionary<string, int>(StringComparer.Ordinal);
        private long _workDay = -1;
        private int _workXpToday;

        public int Xp { get; private set; }
        public int Level => LevelForXp(Xp);
        public List<BadgeDef> Badges { get; } = DefaultBadges();

        public event Action<int, int> LevelChanged;          // eski, yeni
        public event Action<BadgeDef> BadgeEarned;
        public event Action<string> FirstTimeEarned;

        /// <summary>Seviye L için toplam XP: 50·L·(L-1) (Sv2 100, Sv3 300, Sv5 1000, Sv10 4500).</summary>
        public static int XpForLevel(int level) => level <= 1 ? 0 : 50 * level * (level - 1);

        public static int LevelForXp(int xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= XpForLevel(level + 1)) level++;
            return level;
        }

        // ---- kayıt ----
        public long WorkDay => _workDay;
        public int WorkXpTodayRaw => _workXpToday;
        public IEnumerable<string> Firsts => _firsts;
        public IEnumerable<KeyValuePair<string, int>> Counters => _counters;

        /// <summary>Kayıttan yükleme (olay tetiklemez).</summary>
        public void Restore(int xp, IEnumerable<string> firsts, IEnumerable<string> badges, IEnumerable<KeyValuePair<string, int>> counters, long workDay, int workToday)
        {
            Xp = Math.Max(0, xp);
            _firsts.Clear(); if (firsts != null) foreach (var f in firsts) _firsts.Add(f);
            _badges.Clear(); if (badges != null) foreach (var b in badges) _badges.Add(b);
            _counters.Clear(); if (counters != null) foreach (var kv in counters) _counters[kv.Key] = kv.Value;
            _workDay = workDay;
            _workXpToday = workToday;
        }

        public int WorkXpToday(long dayIndex) => dayIndex == _workDay ? _workXpToday : 0;
        public int Counter(string key) => _counters.TryGetValue(key, out var n) ? n : 0;
        public bool HasFirst(string key) => _firsts.Contains(key);
        public bool HasBadge(string id) => _badges.Contains(id);
        public IEnumerable<string> EarnedBadges => _badges;

        private void Grant(int amount)
        {
            if (amount <= 0) return;
            int before = Level;
            Xp += amount;
            int after = Level;
            if (after != before) LevelChanged?.Invoke(before, after);
        }

        /// <summary>Günlük iş XP'si; aynı oyun gününde toplam DailyWorkCap'i aşamaz. Verilen XP'yi döner.</summary>
        public int AddWorkXp(int amount, long dayIndex)
        {
            if (dayIndex != _workDay) { _workDay = dayIndex; _workXpToday = 0; }
            int granted = Math.Max(0, Math.Min(amount, DailyWorkCap - _workXpToday));
            _workXpToday += granted;
            Grant(granted);
            return granted;
        }

        public void OnSkillLevelUp() => Grant(SkillLevelXp);

        /// <summary>"İlk kez" olayı: anahtar başına bir kez XP. İlk kezse true.</summary>
        public bool FirstTime(string key)
        {
            if (!_firsts.Add(key)) return false;
            Grant(FirstTimeXp);
            FirstTimeEarned?.Invoke(key);
            return true;
        }

        /// <summary>Sayaç artırır, rozet koşullarını denetler.</summary>
        public void Count(string key, int by = 1)
        {
            _counters[key] = Counter(key) + by;
            foreach (var b in Badges)
            {
                if (b.Counter != key || _badges.Contains(b.Id) || Counter(key) < b.Target) continue;
                _badges.Add(b.Id);
                Grant(b.RewardXp);
                BadgeEarned?.Invoke(b);
            }
        }

        /// <summary>
        /// Bir etkinliği kaydeder: iş XP'si (tavanlı), "ilk kez" ve sayaç. Anahtar etkileşim kimliğidir
        /// (chop, harvest, split ...) ya da oyun olayıdır (contract, barter, room, fire).
        /// </summary>
        public void Record(string activity, long dayIndex, bool isWork = true)
        {
            if (isWork) AddWorkXp(WorkXp, dayIndex);
            FirstTime("first:" + activity);
            Count("act:" + activity);
        }

        public static List<BadgeDef> DefaultBadges() => new List<BadgeDef>
        {
            new BadgeDef { Id = "badge.woodcutter", DisplayName = "Oduncu", Counter = "act:chop", Target = 10 },
            new BadgeDef { Id = "badge.farmer", DisplayName = "Çiftçi", Counter = "act:harvest", Target = 10 },
            new BadgeDef { Id = "badge.trader", DisplayName = "Köyün işini gören", Counter = "act:contract", Target = 5 },
            new BadgeDef { Id = "badge.splitter", DisplayName = "Balta ustası", Counter = "act:split", Target = 20 }
        };
    }
}
