using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Customization
{
    /// <summary>Ayna değişikliğinin fiyatı ve engelleri.</summary>
    public sealed class MirrorQuote
    {
        public int Diamonds { get; set; }
        /// <summary>Bu değişiklik ücretsiz Ayna hakkıyla yapılıyor (Ergen'e geçiş).</summary>
        public bool UsesFreeMirror { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public bool CanApply => Errors.Count == 0;
    }

    /// <summary>
    /// Ayna: yaratmadan sonra görünüm ve ad değişikliği (karar 17.11.2026, proje dokümanı 01 ve 14).
    /// - Görünüm değişikliği elmasla; çocuk Ergen'e geçerken bir kez ücretsiz (isim hariç).
    /// - Genetik alanlar değişmez: ten, göz rengi, yüz şekli, göz şekli, burun, kulak. Boya (saç rengi) serbesttir, geni değiştirmez.
    /// - Kazadan kalan iz silinmez; elmasla gizlenebilir (hikâyede kalır). Ücretsiz Ayna'da gizlenemez.
    /// - Beden yapısı yaşamla değişir, Ayna'da seçilmez.
    /// Elmas fiyatları yer tutucudur (01, Doğrulanacaklar).
    /// </summary>
    public static class MirrorRules
    {
        public const int AppearanceDiamonds = 20;
        public const int RenameDiamonds = 20;

        /// <summary>Kilitli bir alan değiştiyse adlarını döner.</summary>
        public static List<string> LockedChanges(Appearance before, Appearance after)
        {
            var list = new List<string>();
            if (before.SkinColor != after.SkinColor) list.Add("ten rengi");
            if (before.EyeColor != after.EyeColor || !Nullable.Equals(before.EyeColorRight, after.EyeColorRight)) list.Add("göz rengi");
            if (before.FaceShapeId != after.FaceShapeId) list.Add("yüz şekli");
            if (before.EyeShapeId != after.EyeShapeId) list.Add("göz şekli");
            if (before.NoseId != after.NoseId) list.Add("burun");
            if (before.EarsId != after.EarsId) list.Add("kulak");
            if (before.Build != after.Build) list.Add("beden yapısı");
            return list;
        }

        public static bool AppearanceChanged(Appearance before, Appearance after) => before.Key() != after.Key()
            || before.Voice != after.Voice || before.VoicePitch != after.VoicePitch || before.LaughId != after.LaughId;

        public static MirrorQuote Quote(Character c, Appearance after, string newName, AppearanceCatalog catalog)
        {
            var q = new MirrorQuote();
            var before = c.Appearance;
            foreach (var f in LockedChanges(before, after)) q.Errors.Add($"Ayna {f} değiştiremez.");
            q.Errors.AddRange(catalog.Validate(after, c.Sex));

            var removedEventScars = RemovedEventScars(before, after);
            bool rename = !string.IsNullOrWhiteSpace(newName) && newName.Trim() != c.Name;
            bool changed = AppearanceChanged(before, after) || removedEventScars.Count > 0;
            if (changed)
            {
                if (c.FreeMirrorAvailable && removedEventScars.Count == 0) q.UsesFreeMirror = true;
                else
                {
                    if (c.FreeMirrorAvailable && removedEventScars.Count > 0) q.Errors.Add("İz gizlemek ücretsiz Ayna'da yapılamaz; elmasla yapılır.");
                    q.Diamonds += AppearanceDiamonds;
                }
            }
            if (rename) q.Diamonds += RenameDiamonds;
            return q;
        }

        /// <summary>
        /// Değişikliği uygular. trySpend(elmas) ödemeyi yapar (oyun katmanı verir); false dönerse hiçbir şey değişmez.
        /// </summary>
        public static bool Apply(Character c, Appearance after, string newName, AppearanceCatalog catalog, Func<int, bool> trySpend, out string error)
        {
            var q = Quote(c, after, newName, catalog);
            error = null;
            if (!q.CanApply) { error = string.Join(" ", q.Errors); return false; }
            if (q.Diamonds > 0 && (trySpend == null || !trySpend(q.Diamonds))) { error = "Yeterli elmas yok."; return false; }

            var result = after.Clone();
            // Kazadan kalan izler korunur: Ayna'da kaldırılan iz silinmez, gizlenir.
            foreach (var removed in RemovedEventScars(c.Appearance, after))
            {
                var keep = removed.Clone();
                keep.Hidden = true;
                result.Marks.Add(keep);
            }
            result.HeightPercent = 100;
            c.Appearance = result;
            if (q.UsesFreeMirror) c.FreeMirrorAvailable = false;
            if (!string.IsNullOrWhiteSpace(newName)) c.Name = newName.Trim();
            return true;
        }

        private static List<Mark> RemovedEventScars(Appearance before, Appearance after)
        {
            var afterKeys = new HashSet<string>(after.Marks.Where(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Event && !m.Hidden).Select(EventKey));
            return before.Marks.Where(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Event && !m.Hidden && !afterKeys.Contains(EventKey(m))).ToList();
        }

        private static string EventKey(Mark m) => $"{m.Id}|{m.Zone}|{m.Day}|{m.Reason}";
    }
}
