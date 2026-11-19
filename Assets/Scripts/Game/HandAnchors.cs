using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Animasyon karelerinde elin konumu (elde tutulan yiyecek ya da alet buraya çizilir). Kaynak:
    /// Resources/Content/hand_anchors.txt, satır başına "önek kare x y [b]" (x soldan, y alttan, piksel; "b": el gövdenin
    /// arkasında, nesne karakterin arkasına çizilir). Önek IsoArt'taki kare adı gibidir (ör. iso_char_man_r84_eat3_se_).
    /// Dosya yoksa ya da önek yazılmamışsa Has false döner; oyun eski (nesnesi çizili) animasyonu oynatır.
    /// </summary>
    public static class HandAnchors
    {
        public struct Anchor { public float X, Y; public bool Behind; }

        private const string Path = "Content/hand_anchors";
        private static Dictionary<string, Dictionary<int, Anchor>> _data;

        private static void Load()
        {
            if (_data != null) return;
            _data = new Dictionary<string, Dictionary<int, Anchor>>();
            var asset = Resources.Load<TextAsset>(Path);
            if (asset != null) Parse(asset.text, _data);
        }

        /// <summary>Metni ayrıştırır (testlenebilsin diye ayrı).</summary>
        public static void Parse(string text, Dictionary<string, Dictionary<int, Anchor>> into)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (var raw in text.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                var p = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 4 || !int.TryParse(p[1], out int frame) ||
                    !float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) ||
                    !float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y)) continue;
                var key = p[0].TrimEnd('_');
                if (!into.TryGetValue(key, out var frames)) { frames = new Dictionary<int, Anchor>(); into[key] = frames; }
                frames[frame] = new Anchor { X = x, Y = y, Behind = p.Length > 4 && p[4] == "b" };
            }
        }

        public static bool Has(string prefix) { Load(); return prefix != null && _data.ContainsKey(prefix.TrimEnd('_')); }

        public static bool TryGet(string prefix, int frame, out Anchor a)
        {
            Load();
            a = default;
            return prefix != null && _data.TryGetValue(prefix.TrimEnd('_'), out var f) && f.TryGetValue(frame, out a);
        }
    }
}
