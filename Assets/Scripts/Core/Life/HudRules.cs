using System.Collections.Generic;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Kayan bildirim kartları (Project belgesi 66 §3, HUD v2): kalıcı bildirim paneli yerine yeni bildirim sağ üstte kısa
    /// bir şerit olarak görünür ve solar. Aynı anda en fazla <see cref="MaxVisible"/> kart; yenisi gelince en eskisi düşer.
    /// Zaman dışarıdan verilir (gerçek saniye), bu sınıf Unity'ye bağlı değildir. Geçmiş Defter'deki bildirim listesindedir.
    /// </summary>
    public sealed class ToastQueue
    {
        public const int MaxVisible = 3;
        public const float ShowSeconds = 3.5f;
        public const float FadeSeconds = 0.6f;

        public sealed class Toast
        {
            public string Text;
            public NoticeLevel Level;
            public float Born;
            /// <summary>0-1 arası görünürlük: son <see cref="FadeSeconds"/> içinde azalır.</summary>
            public float Alpha;
        }

        private readonly List<Toast> _items = new List<Toast>();

        public IReadOnlyList<Toast> Items => _items;

        /// <summary>Gösterilmeye değer mi: gelip geçen ihtiyaç bildirimi yalnızca uyarı ya da tehlikeyse kart olur.</summary>
        public static bool Worth(NoticeEntry e) => e != null && !string.IsNullOrWhiteSpace(e.Text) && (!e.Transient || e.Level >= NoticeLevel.Warning);

        public void Push(string text, NoticeLevel level, float now)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            // Aynı metin hâlâ ekrandaysa süresi yenilenir, ikinci kart açılmaz.
            foreach (var t in _items)
                if (t.Text == text) { t.Born = now; if (level > t.Level) t.Level = level; t.Alpha = 1f; return; }
            _items.Add(new Toast { Text = text, Level = level, Born = now, Alpha = 1f });
            while (_items.Count > MaxVisible) _items.RemoveAt(0);
        }

        /// <summary>Süresi dolanları atar, kalanların görünürlüğünü günceller. Değişiklik olduysa true.</summary>
        public bool Tick(float now)
        {
            bool changed = false;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var t = _items[i];
                float age = now - t.Born;
                if (age >= ShowSeconds) { _items.RemoveAt(i); changed = true; continue; }
                float left = ShowSeconds - age;
                float a = left < FadeSeconds ? left / FadeSeconds : 1f;
                if (a < 0f) a = 0f;
                if (a != t.Alpha) { t.Alpha = a; changed = true; }
            }
            return changed;
        }

        public void Clear() => _items.Clear();
    }

    /// <summary>
    /// Karakter kartının açılma kuralı (belge 66 karar S1): kart yalnızca bir karaktere (portre ya da haritada kendisi)
    /// dokununca açılır. Açıkken aynı karaktere yeniden dokunmak kapatır, başka karaktere dokunmak kartı ona geçirir.
    /// </summary>
    public sealed class CardToggle
    {
        public bool Open { get; private set; }
        public int CharacterId { get; private set; }

        /// <summary>Karaktere dokunuldu. Kart açık kalırsa true.</summary>
        public bool Tap(int characterId)
        {
            if (Open && CharacterId == characterId) { Open = false; return false; }
            Open = true;
            CharacterId = characterId;
            return true;
        }

        public void Close() => Open = false;
    }

    /// <summary>Defter penceresinin bölümleri (karar S2): sağ alttaki Defter düğmesinin açtığı tek pencerenin sekmeleri.</summary>
    public static class LedgerSections
    {
        public const string Family = "family", Storage = "storage", Market = "market", Mine = "mine", Village = "village",
            Relations = "relations", Requests = "requests", Keepsakes = "keepsakes", Kitchen = "kitchen", Home = "home",
            Carts = "carts", Achievements = "achievements", Notices = "notices", Settings = "settings";

        /// <summary>Görünme sırası: en çok açılanlar önce.</summary>
        public static readonly IReadOnlyList<string> Order = new[]
        {
            Family, Storage, Market, Requests, Village, Relations, Mine, Kitchen, Home, Carts, Keepsakes, Notices, Achievements, Settings
        };

        /// <summary>Defter düğmesindeki "!" rozeti: okunmamış bildirim ya da istek varsa.</summary>
        public static bool Badge(int unreadNotices, int unreadRequests) => unreadNotices > 0 || unreadRequests > 0;

        /// <summary>Bölümün başlığındaki sayı (0 ise gösterilmez).</summary>
        public static int Count(string section, int unreadNotices, int unreadRequests) =>
            section == Notices ? unreadNotices : section == Requests ? unreadRequests : 0;
    }
}
