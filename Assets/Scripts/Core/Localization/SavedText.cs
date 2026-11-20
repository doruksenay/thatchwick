using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Kayda yazılan, sonradan ekranda gösterilecek metin (hayat notu, kronik). Kural (docs/yerellestirme.md):
    /// kayıtlara çevrilmiş metin yazılmaz, anahtar yazılır; yoksa dil değişince eski kayıt eski dilde kalır.
    ///
    /// Biçim tek satırlık bir dizgedir: <c>@@anahtar|ad=t:değer|ad=t:değer</c>. Tür harfi: <c>s</c> düz metin (oyuncunun
    /// verdiği ad), <c>n</c> sayı, <c>c</c> içerik kimliği (eşya, özellik: adı gösterilirken seçili dilde çözülür),
    /// <c>k</c> beceri (<c>skill.&lt;Ad&gt;</c> anahtarı), <c>t</c> başka bir tablo anahtarı. <c>@@</c> ile başlamayan dizge eski kayıttan kalan düz metindir
    /// ve olduğu gibi gösterilir.
    /// </summary>
    public static class SavedText
    {
        public const string Prefix = "@@";

        /// <summary>İçerik kimliği argümanı (adı gösterilirken çözülür).</summary>
        public readonly struct ContentRef { public readonly string Id; public ContentRef(string id) { Id = id; } }
        /// <summary>Beceri argümanı (enum adı).</summary>
        public readonly struct SkillRef { public readonly string Name; public SkillRef(string name) { Name = name; } }

        /// <summary>Tablo anahtarı argümanı (ör. yarışma türü): gösterilirken seçili dilin tablosundan okunur.</summary>
        public readonly struct KeyRef { public readonly string Key; public KeyRef(string key) { Key = key; } }
        public static KeyRef Key(string key) => new KeyRef(key);

        public static ContentRef Content(string prefixedId) => new ContentRef(prefixedId);
        public static SkillRef Skill(Enum skill) => new SkillRef(skill.ToString());

        public static bool IsEncoded(string saved) => saved != null && saved.StartsWith(Prefix, StringComparison.Ordinal);

        public static string Encode(string key, params (string name, object value)[] args)
        {
            var sb = new StringBuilder(Prefix).Append(Escape(key));
            if (args != null)
                foreach (var (name, value) in args)
                {
                    sb.Append('|').Append(Escape(name)).Append('=');
                    switch (value)
                    {
                        case ContentRef c: sb.Append("c:").Append(Escape(c.Id)); break;
                        case SkillRef k: sb.Append("k:").Append(Escape(k.Name)); break;
                        case KeyRef t: sb.Append("t:").Append(Escape(t.Key)); break;
                        case sbyte _: case byte _: case short _: case ushort _: case int _: case uint _: case long _: case ulong _:
                        case float _: case double _: case decimal _:
                            sb.Append("n:").Append(Convert.ToString(value, CultureInfo.InvariantCulture)); break;
                        default: sb.Append("s:").Append(Escape(value?.ToString() ?? "")); break;
                    }
                }
            return sb.ToString();
        }

        /// <summary>Kodlanmış dizgeyi parçalar; kodlanmamışsa false.</summary>
        public static bool TryDecode(string saved, out string key, out List<(string name, char type, string value)> args)
        {
            key = null; args = new List<(string, char, string)>();
            if (!IsEncoded(saved)) return false;
            var parts = Split(saved.Substring(Prefix.Length));
            if (parts.Count == 0 || parts[0].Length == 0) return false;
            key = Unescape(parts[0]);
            for (int i = 1; i < parts.Count; i++)
            {
                int eq = parts[i].IndexOf('=');
                if (eq <= 0 || parts[i].Length < eq + 3 || parts[i][eq + 2] != ':') continue;
                args.Add((Unescape(parts[i].Substring(0, eq)), parts[i][eq + 1], Unescape(parts[i].Substring(eq + 3))));
            }
            return true;
        }

        /// <summary>
        /// Ekranda gösterilecek metin. contentName: içerik kimliğinden seçili dildeki ad (null ise kimlik yazılır).
        /// Anahtar tabloda yoksa (eski sürümden kalan kayıt) anahtarın kendisi yerine argümanlar virgülle gösterilir.
        /// </summary>
        public static string Resolve(string saved, Localizer loc, Func<string, string> contentName = null)
        {
            if (!TryDecode(saved, out var key, out var raw)) return saved ?? "";
            var args = new (string name, object value)[raw.Count];
            for (int i = 0; i < raw.Count; i++)
            {
                var (name, type, value) = raw[i];
                object shown;
                switch (type)
                {
                    case 'c': shown = contentName?.Invoke(value) ?? value; break;
                    case 'k': shown = loc != null ? loc.GetOr("skill." + value, value) : value; break;
                    case 't': shown = loc != null ? loc.GetOr(value, value) : value; break;
                    case 'n': shown = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) && d != Math.Floor(d) ? (object)d : (object)value; break;
                    default: shown = value; break;
                }
                args[i] = (name, shown);
            }
            if (loc == null || !loc.TryGet(key, out _))
            {
                var vals = new List<string>();
                foreach (var a in args) vals.Add(Convert.ToString(a.value, CultureInfo.InvariantCulture));
                return string.Join(", ", vals);
            }
            return loc.Format(key, args);
        }

        // Kaçış: yalnızca ayraç (|) ve ters bölü. Tek geçişte çözülür.
        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 4);
            foreach (char ch in s)
            {
                if (ch == '\\') sb.Append("\\\\");
                else if (ch == '|') sb.Append("\\p");
                else if (ch == '\n' || ch == '\r') sb.Append(' ');
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        private static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('\\') < 0) return s ?? "";
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length) { i++; sb.Append(s[i] == 'p' ? '|' : s[i]); }
                else sb.Append(s[i]);
            }
            return sb.ToString();
        }

        private static List<string> Split(string s)
        {
            var list = new List<string>();
            int start = 0;
            for (int i = 0; i <= s.Length; i++)
                if (i == s.Length || s[i] == '|') { list.Add(s.Substring(start, i - start)); start = i + 1; }
            return list;
        }
    }
}
