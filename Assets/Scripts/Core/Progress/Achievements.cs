using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Progress
{
    /// <summary>Başarımın neye baktığı.</summary>
    public enum AchievementKind
    {
        /// <summary>Hesap sayacı (<see cref="AccountProgress.Counter"/>): "act:harvest", "act:contract"…</summary>
        Counter,
        /// <summary>Hesap seviyesi.</summary>
        Level,
        /// <summary>Hanedanın kuşağı (kurucu 1).</summary>
        Generation,
        /// <summary>Geçen oyun yılı.</summary>
        Years,
        /// <summary>Ailedeki yadigâr sayısı.</summary>
        Heirlooms
    }

    /// <summary>
    /// Başarım tanımı. Kimlik kalıcıdır (profil dosyasına ve mağaza tarafına yazılır): yayından sonra değiştirilmez.
    /// Ad ve açıklama yerelleştirme tablosundadır: <c>ui.ach.&lt;kısa ad&gt;.name</c> / <c>.desc</c>.
    /// </summary>
    public sealed class AchievementDef
    {
        public string Id { get; }
        public AchievementKind Kind { get; }
        /// <summary>Sayaç türünde sayaç anahtarı; diğerlerinde boş.</summary>
        public string Key { get; }
        public int Target { get; }
        /// <summary>Açılana kadar listede adı ve açıklaması gizlenir.</summary>
        public bool Hidden { get; }

        public AchievementDef(string id, AchievementKind kind, int target, string key = null, bool hidden = false)
        {
            Id = id; Kind = kind; Target = target; Key = key; Hidden = hidden;
        }

        /// <summary>"ach.first_harvest" → "first_harvest".</summary>
        public string ShortName => Id.StartsWith("ach.", StringComparison.Ordinal) ? Id.Substring(4) : Id;
        public string NameKey => "ui.ach." + ShortName + ".name";
        public string DescKey => "ui.ach." + ShortName + ".desc";
        /// <summary>Mağaza (Steam) tarafındaki API adı: "ACH_FIRST_HARVEST". Kültürden bağımsız üretilir.</summary>
        public string PlatformName => Id.Replace('.', '_').ToUpperInvariant();
    }

    /// <summary>Başarımların baktığı değerlerin o anki görüntüsü (Game katmanı doldurur).</summary>
    public sealed class AchievementSnapshot
    {
        public Func<string, int> Counter { get; set; }
        public int Level { get; set; }
        public int Generation { get; set; }
        public int Years { get; set; }
        public int Heirlooms { get; set; }

        public int ValueOf(AchievementDef def)
        {
            switch (def.Kind)
            {
                case AchievementKind.Counter: return Counter != null && def.Key != null ? Counter(def.Key) : 0;
                case AchievementKind.Level: return Level;
                case AchievementKind.Generation: return Generation;
                case AchievementKind.Years: return Years;
                case AchievementKind.Heirlooms: return Heirlooms;
                default: return 0;
            }
        }
    }

    /// <summary>Profil dosyasının birebir şekli (JsonUtility uyumlu: genel alanlar, diziler). Alanlar eklemelidir.</summary>
    [Serializable]
    public sealed class AchievementProfile
    {
        public int version = 1;
        public AchievementEntry[] entries;
    }

    [Serializable]
    public sealed class AchievementEntry
    {
        public string id;
        /// <summary>Bugüne kadar görülen en yüksek değer (kayıt yuvalarından bağımsız ilerleme çubuğu için).</summary>
        public int best;
        public bool unlocked;
        /// <summary>Açıldığı an (UTC, ISO 8601); açılmadıysa boş.</summary>
        public string unlockedUtc;
    }

    /// <summary>
    /// Başarım servisi (yayın altyapısı, 20.11.2026). Başarımlar kayıt yuvasına değil oyuncu profiline bağlıdır:
    /// bir yuvada açılan başarım öbür yuvada ve yeni oyunda açık kalır (mağaza başarımları da hesap düzeyindedir).
    /// Oyun rozetlerinden (<see cref="BadgeDef"/>) ayrıdır: rozet oyun içi ödül verir ve kayda yazılır; başarım ödül
    /// vermez, yalnızca kalıcı bir kayıttır. Bilinmeyen kimlikli profil girdileri (eski sürümden kalan) saklanır.
    /// </summary>
    public sealed class AchievementService
    {
        private readonly List<AchievementDef> _defs;
        private readonly Dictionary<string, AchievementEntry> _entries = new Dictionary<string, AchievementEntry>(StringComparer.Ordinal);
        private readonly Func<DateTime> _now;

        /// <summary>Bir başarım açıldığında (bildirim ve mağaza eşlemesi için).</summary>
        public event Action<AchievementDef> Unlocked;

        public AchievementService(IEnumerable<AchievementDef> defs, AchievementProfile profile = null, Func<DateTime> now = null)
        {
            _defs = new List<AchievementDef>(defs ?? throw new ArgumentNullException(nameof(defs)));
            _now = now ?? (() => DateTime.UtcNow);
            if (profile?.entries != null)
                foreach (var e in profile.entries)
                    if (e != null && !string.IsNullOrEmpty(e.id)) _entries[e.id] = e;
        }

        public IReadOnlyList<AchievementDef> Defs => _defs;
        /// <summary>Son kaydedilmesinden beri değişti mi (dosyaya yazılması gerekiyor).</summary>
        public bool Dirty { get; private set; }
        public void MarkSaved() => Dirty = false;

        public bool IsUnlocked(string id) => _entries.TryGetValue(id, out var e) && e.unlocked;
        public int Best(string id) => _entries.TryGetValue(id, out var e) ? e.best : 0;

        public int UnlockedCount
        {
            get
            {
                int n = 0;
                foreach (var d in _defs) if (IsUnlocked(d.Id)) n++;
                return n;
            }
        }

        /// <summary>0-1 arası ilerleme (açıksa 1).</summary>
        public double Progress(AchievementDef def)
        {
            if (IsUnlocked(def.Id)) return 1.0;
            if (def.Target <= 0) return 0.0;
            return Math.Min(1.0, Best(def.Id) / (double)def.Target);
        }

        /// <summary>
        /// Görüntüye göre ilerlemeyi günceller; yeni açılanları döndürür (yoksa boş liste; ayırma yapmaz).
        /// İlerleme yalnızca artar: başka bir yuvaya geçmek ya da yeni oyuna başlamak ilerlemeyi düşürmez.
        /// </summary>
        public IReadOnlyList<AchievementDef> Evaluate(AchievementSnapshot snapshot)
        {
            if (snapshot == null) return Array.Empty<AchievementDef>();
            List<AchievementDef> fresh = null;
            foreach (var def in _defs)
            {
                if (IsUnlocked(def.Id)) continue;
                int value = snapshot.ValueOf(def);
                if (value <= 0) continue;
                var entry = Entry(def.Id);
                if (value > entry.best) { entry.best = value; Dirty = true; }
                if (entry.best < def.Target) continue;
                Open(def, entry);
                (fresh ??= new List<AchievementDef>()).Add(def);
            }
            if (fresh == null) return Array.Empty<AchievementDef>();
            foreach (var def in fresh) Unlocked?.Invoke(def);
            return fresh;
        }

        /// <summary>Olaya bağlı başarımı doğrudan açar. Zaten açıksa ya da kimlik bilinmiyorsa false.</summary>
        public bool Unlock(string id)
        {
            var def = Find(id);
            if (def == null || IsUnlocked(id)) return false;
            Open(def, Entry(id));
            Unlocked?.Invoke(def);
            return true;
        }

        public AchievementDef Find(string id)
        {
            foreach (var d in _defs) if (string.Equals(d.Id, id, StringComparison.Ordinal)) return d;
            return null;
        }

        /// <summary>Dosyaya yazılacak profil (kimliğe göre sıralı: dosya farkları okunur kalsın).</summary>
        public AchievementProfile ToProfile()
        {
            var list = new List<AchievementEntry>(_entries.Values);
            list.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
            return new AchievementProfile { entries = list.ToArray() };
        }

        private AchievementEntry Entry(string id)
        {
            if (!_entries.TryGetValue(id, out var e)) _entries[id] = e = new AchievementEntry { id = id };
            return e;
        }

        private void Open(AchievementDef def, AchievementEntry entry)
        {
            entry.unlocked = true;
            if (entry.best < def.Target) entry.best = def.Target;
            entry.unlockedUtc = _now().ToString("o");
            Dirty = true;
        }
    }

    /// <summary>
    /// 1.0 başarım listesi. Yalnızca ana oyunun içeriğine bakar (+18 paketine bağlı başarım yoktur: paket ayrı satılır).
    /// Sayaç anahtarları <see cref="AccountProgress.Record"/> ile yazılan etkinliklerdir.
    /// Yeni başarım eklerken: kimliği burada, adı ve açıklamasını <c>ui.achievements.en/tr.json</c>'da, mağaza tarafını
    /// <c>docs/yayin-altyapisi.md</c>'deki tabloda güncelle (test ilk ikisini denetler).
    /// </summary>
    public static class AchievementCatalog
    {
        public static IReadOnlyList<AchievementDef> Default { get; } = new List<AchievementDef>
        {
            // tarla ve orman
            new AchievementDef("ach.first_harvest", AchievementKind.Counter, 1, "act:harvest"),
            new AchievementDef("ach.harvest_100", AchievementKind.Counter, 100, "act:harvest"),
            new AchievementDef("ach.harvest_1000", AchievementKind.Counter, 1000, "act:harvest"),
            new AchievementDef("ach.chop_50", AchievementKind.Counter, 50, "act:chop"),
            // ev ve köy
            new AchievementDef("ach.first_room", AchievementKind.Counter, 1, "act:room"),
            new AchievementDef("ach.first_contract", AchievementKind.Counter, 1, "act:contract"),
            new AchievementDef("ach.contracts_25", AchievementKind.Counter, 25, "act:contract"),
            new AchievementDef("ach.chat_100", AchievementKind.Counter, 100, "act:chat"),
            // hayvanlar
            new AchievementDef("ach.pet_25", AchievementKind.Counter, 25, "act:pet_animal"),
            new AchievementDef("ach.first_ride", AchievementKind.Counter, 1, "act:ride"),
            // aile ve hanedan
            new AchievementDef("ach.marriage", AchievementKind.Counter, 1, "act:marriage"),
            new AchievementDef("ach.first_heir", AchievementKind.Generation, 2),
            new AchievementDef("ach.generation_3", AchievementKind.Generation, 3),
            new AchievementDef("ach.generation_5", AchievementKind.Generation, 5),
            new AchievementDef("ach.generation_10", AchievementKind.Generation, 10),
            new AchievementDef("ach.first_heirloom", AchievementKind.Heirlooms, 1),
            // zaman ve hesap
            new AchievementDef("ach.year_1", AchievementKind.Years, 1),
            new AchievementDef("ach.year_10", AchievementKind.Years, 10),
            new AchievementDef("ach.year_25", AchievementKind.Years, 25),
            new AchievementDef("ach.level_5", AchievementKind.Level, 5),
            new AchievementDef("ach.level_10", AchievementKind.Level, 10),
            new AchievementDef("ach.level_20", AchievementKind.Level, AccountProgress.MaxLevel)
        };
    }
}
