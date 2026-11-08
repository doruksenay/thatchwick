using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Life
{
    public sealed class DaySummary
    {
        public long Day;
        public int AkceDelta;
        public int XpDelta;
        public int Born, Died;
        public List<string> Names = new List<string>();          // doğan/ölen adları
        public List<KeyValuePair<string, int>> ItemDeltas = new List<KeyValuePair<string, int>>();
        public List<string> Notes = new List<string>();
        public bool IsEmpty => AkceDelta == 0 && XpDelta == 0 && Born == 0 && Died == 0 && ItemDeltas.Count == 0 && Notes.Count == 0;
    }

    /// <summary>
    /// Gün sonu özeti (Adım 18c): gün dönümünde akçe/XP farkı, envanter değişimi (tüketim ve üretim),
    /// doğan/ölen ve notları toplar. Arayüz kapatılabilir bir kart olarak gösterir.
    /// </summary>
    public sealed class DayLedger
    {
        private readonly Household _house;
        private readonly Func<int> _xp;
        private long _day = -1;
        private int _akceAtStart, _xpAtStart;
        private Dictionary<string, int> _inventoryAtStart = new Dictionary<string, int>();
        private DaySummary _current = new DaySummary();

        public DaySummary Last { get; private set; }
        public event Action<DaySummary> DayEnded;

        public DayLedger(Household house, Func<int> xp = null)
        {
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _xp = xp ?? (() => 0);
        }

        private Dictionary<string, int> Snapshot()
        {
            var d = new Dictionary<string, int>();
            foreach (var kv in _house.InventoryList()) if (kv.Value > 0) d[kv.Key] = kv.Value;
            return d;
        }

        public void OnBorn(Character baby) { _current.Born++; _current.Names.Add(baby.Name + " doğdu"); }
        public void OnDied(Character c) { _current.Died++; _current.Names.Add(c.Name + " öldü"); }
        public void Note(string text) { if (!string.IsNullOrWhiteSpace(text) && _current.Notes.Count < 8) _current.Notes.Add(text); }

        /// <summary>Her karede çağrılır; gün değişince önceki günü kapatır. Dönen: kapanan özet ya da null.</summary>
        public DaySummary Tick(long dayIndex)
        {
            if (_day < 0)
            {
                Begin(dayIndex);
                return null;
            }
            if (dayIndex == _day) return null;

            var summary = _current;
            summary.Day = _day;
            summary.AkceDelta = _house.Akce - _akceAtStart;
            summary.XpDelta = _xp() - _xpAtStart;
            var now = Snapshot();
            var keys = new HashSet<string>(_inventoryAtStart.Keys);
            foreach (var k in now.Keys) keys.Add(k);
            foreach (var k in keys)
            {
                _inventoryAtStart.TryGetValue(k, out int before);
                now.TryGetValue(k, out int after);
                if (after != before) summary.ItemDeltas.Add(new KeyValuePair<string, int>(k, after - before));
            }
            summary.ItemDeltas.Sort((a, b) => Math.Abs(b.Value).CompareTo(Math.Abs(a.Value)));
            Last = summary;
            Begin(dayIndex);
            DayEnded?.Invoke(summary);
            return summary;
        }

        private void Begin(long dayIndex)
        {
            _day = dayIndex;
            _akceAtStart = _house.Akce;
            _xpAtStart = _xp();
            _inventoryAtStart = Snapshot();
            _current = new DaySummary();
        }
    }
}
