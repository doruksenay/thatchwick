using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Life
{
    public enum NoticeLevel { Info = 0, Warning = 1, Danger = 2 }

    public sealed class NoticeEntry
    {
        public double Day;
        public NoticeLevel Level;
        public string Text;
        public int CharacterId;
        public int Repeats = 1;
        public bool Read;
        /// <summary>Gelip geçen durum bildirimi (acıktı, yoruldu): akış dolunca önce bunlar silinir.</summary>
        public bool Transient;
    }

    /// <summary>
    /// Bildirim akışı (Adım 18b): yeni olay üstte, aynı metin kısa sürede tekrar gelirse birleşir ("x3").
    /// Akış dolunca önce en eski gelip geçen ihtiyaç bildirimi silinir; ölüm, doğum gibi olaylar ihtiyaç uyarılarının
    /// arasında kaybolmaz. Arayüz yalnızca bunu okur; üretim NeedWatcher ve LifeSystems olaylarındadır.
    /// </summary>
    public sealed class NotificationLog
    {
        public const int Capacity = 80;
        public const double MergeWindowDays = 0.25;

        private readonly List<NoticeEntry> _items = new List<NoticeEntry>();

        public IReadOnlyList<NoticeEntry> Items => _items;
        public event Action<NoticeEntry> Added;

        public int UnreadCount
        {
            get { int n = 0; foreach (var e in _items) if (!e.Read) n++; return n; }
        }

        public NoticeEntry Add(double day, NoticeLevel level, string text, int characterId = 0, bool transient = false)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            // Pencere içindeki her kayda bakılır (başka kişilerin bildirimleri araya girse de birleşir).
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var e = _items[i];
                if (day - e.Day > MergeWindowDays) continue;
                if (e.Text == text && e.CharacterId == characterId)
                {
                    e.Repeats++;
                    e.Day = day;
                    if (level > e.Level) e.Level = level;
                    e.Read = false;
                    return e;
                }
            }
            var entry = new NoticeEntry { Day = day, Level = level, Text = text, CharacterId = characterId, Transient = transient };
            _items.Add(entry);
            if (_items.Count > Capacity)
            {
                int victim = _items.FindIndex(e => e.Transient);
                _items.RemoveAt(victim >= 0 && victim < _items.Count - 1 ? victim : 0);
            }
            Added?.Invoke(entry);
            return entry;
        }

        public void MarkAllRead() { foreach (var e in _items) e.Read = true; }

        /// <summary>En yeni n kayıt, en yenisi önce.</summary>
        public List<NoticeEntry> Recent(int n)
        {
            var list = new List<NoticeEntry>();
            for (int i = _items.Count - 1; i >= 0 && list.Count < n; i--) list.Add(_items[i]);
            return list;
        }
    }

    /// <summary>
    /// Ekrandaki ihtiyaç uyarıları ("Maud acıktı", "Bebek aç"): eşiği geçince bir kez uyarır, ihtiyaç
    /// toparlanıp biraz yükselince yeniden silahlanır. Aynı kişi ve ihtiyaç için aynı uyarı bekleme süresi dolmadan
    /// tekrarlanmaz (her gün acıkmak olağandır, akışı doldurmasın). Köylüler izlenmez (ihtiyaçları sahnede).
    /// </summary>
    public sealed class NeedWatcher
    {
        public const float RearmMargin = 12f;
        /// <summary>Aynı uyarının (kişi, ihtiyaç, basamak) en erken tekrarı: uyarı 1 gün, tehlike yarım gün sonra.</summary>
        public const double WarnRepeatDays = 1.0, DangerRepeatDays = 0.5;
        private readonly Dictionary<long, int> _level = new Dictionary<long, int>();
        private readonly Dictionary<long, double> _lastRaised = new Dictionary<long, double>();

        public event Action<NoticeLevel, string, int> Raised;

        private struct Rule { public NeedType Need; public float Warn, Danger; public string WarnText, DangerText; public NoticeLevel WarnLevel; }

        private static readonly Rule[] Rules =
        {
            new Rule { Need = NeedType.Hunger, Warn = 30, Danger = 15, WarnText = "acıktı", DangerText = "açlıktan bitkin düştü!", WarnLevel = NoticeLevel.Warning },
            new Rule { Need = NeedType.Energy, Warn = 30, Danger = 15, WarnText = "yoruldu", DangerText = "uykusuzluktan bitkin!", WarnLevel = NoticeLevel.Warning },
            new Rule { Need = NeedType.Warmth, Warn = 35, Danger = 15, WarnText = "üşüyor", DangerText = "donmak üzere!", WarnLevel = NoticeLevel.Warning },
            new Rule { Need = NeedType.Social, Warn = 20, Danger = -1, WarnText = "yalnız hissediyor", DangerText = null, WarnLevel = NoticeLevel.Info },
            new Rule { Need = NeedType.Fun, Warn = 20, Danger = -1, WarnText = "sıkıldı", DangerText = null, WarnLevel = NoticeLevel.Info },
        };

        private static long Key(int id, NeedType n) => ((long)id << 8) | (uint)(int)n;

        /// <param name="day">Şimdiki oyun günü; verilmezse (NaN) bekleme süresi uygulanmaz.</param>
        public void Update(IEnumerable<Character> characters, float babyHungerThreshold = 45f, double day = double.NaN)
        {
            foreach (var c in characters)
            {
                if (c.IsDead || c.IsVillager) continue;
                foreach (var r in Rules)
                {
                    float v = c.Needs.Get(r.Need);
                    long key = Key(c.Id, r.Need);
                    _level.TryGetValue(key, out int level);
                    float warn = r.Need == NeedType.Hunger && c.Stage == AgeStage.Baby ? babyHungerThreshold : r.Warn;

                    int now = r.Danger >= 0 && v <= r.Danger ? 2 : v <= warn ? 1 : 0;
                    if (now == 0 && level > 0 && v < warn + RearmMargin) now = level;   // histerezis: toparlanmadan yeniden uyarma
                    if (now > level && !double.IsNaN(day))
                    {
                        long raisedKey = key * 4 + now;
                        double wait = now == 2 ? DangerRepeatDays : WarnRepeatDays;
                        if (_lastRaised.TryGetValue(raisedKey, out var last) && day - last < wait) { _level[key] = now; continue; }
                        _lastRaised[raisedKey] = day;
                    }
                    if (now > level)
                    {
                        if (now == 2) Raised?.Invoke(NoticeLevel.Danger, $"{c.Name} {r.DangerText}", c.Id);
                        else Raised?.Invoke(r.WarnLevel, c.Stage == AgeStage.Baby && r.Need == NeedType.Hunger ? $"{c.Name} (bebek) aç" : $"{c.Name} {r.WarnText}", c.Id);
                    }
                    _level[key] = now;
                }
            }
        }
    }
}
