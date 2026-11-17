using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Seçili dilin metinlerini verir. Önce seçili dilin tablosuna, yoksa kaynak dilin (İngilizce) tablosuna bakar;
    /// ikisinde de yoksa anahtarın kendisini döndürür ve <see cref="MissingKeys"/>'e yazar (eksik çeviri ekranda görünsün).
    ///
    /// Yer tutucular adlıdır: <c>"{name} bought {item}"</c>. Kelime sırası dile göre değişebildiği için
    /// sıra numarası ({0}) kullanılmaz. Süslü parantezin kendisi için <c>{{</c> ve <c>}}</c>.
    /// Sayılar seçili dilin kültürüyle biçimlenir (TR/DE/FR/ES "1,5", EN "1.5").
    /// </summary>
    public sealed class Localizer
    {
        private readonly LocTable _table;
        private readonly LocTable _fallback;
        private readonly HashSet<string> _missing = new HashSet<string>(StringComparer.Ordinal);

        public string Language { get; }
        public CultureInfo Culture { get; }

        /// <summary>Bu oturumda istenip hiçbir tabloda bulunamayan anahtarlar (hata ayıklama ve test için).</summary>
        public IReadOnlyCollection<string> MissingKeys => _missing;

        /// <param name="language">Seçili dil kodu.</param>
        /// <param name="table">Seçili dilin tablosu.</param>
        /// <param name="fallback">Kaynak dilin (İngilizce) tablosu; seçili dil zaten kaynak dilse null ya da aynı tablo olabilir.</param>
        public Localizer(string language, LocTable table, LocTable fallback = null)
        {
            Language = Languages.Normalize(language) ?? Languages.Source;
            _table = table ?? new LocTable();
            _fallback = ReferenceEquals(fallback, table) ? null : fallback;
            Culture = SafeCulture(Languages.CultureName(Language));
        }

        /// <summary>Seçili dilde ya da kaynak dilde metni bulur.</summary>
        public bool TryGet(string key, out string value)
        {
            if (_table.TryGet(key, out value)) return true;
            if (_fallback != null && _fallback.TryGet(key, out value)) return true;
            value = null;
            return false;
        }

        /// <summary>Yalnızca seçili dilin tablosuna bakar (kaynak dile düşmeden).</summary>
        public bool TryGetExact(string key, out string value) => _table.TryGet(key, out value);

        public bool Has(string key) => TryGet(key, out _);

        /// <summary>Metni döndürür; bulunamazsa anahtarın kendisi (eksik çeviri ekranda göze batsın).</summary>
        public string Get(string key)
        {
            if (TryGet(key, out var v)) return v;
            if (key != null) _missing.Add(key);
            return key ?? "";
        }

        /// <summary>Metni bulamazsa verilen yedeği döndürür (eksik sayılmaz).</summary>
        public string GetOr(string key, string fallbackText) => TryGet(key, out var v) ? v : fallbackText;

        /// <summary>Anahtarın metnini adlı yer tutucularla doldurur.</summary>
        public string Format(string key, params (string name, object value)[] args) => Fill(Get(key), args);

        /// <summary>
        /// Sayıya göre tekil/çoğul seçer: önce <c>key.one</c> ya da <c>key.other</c>, sonra <c>key</c>.
        /// <c>{count}</c> yer tutucusu sayıyla doldurulur. Türkçede sayıdan sonra isim tekil kaldığı için TR tablosunda
        /// yalnızca <c>.other</c> yeterlidir.
        /// </summary>
        public string Plural(string key, long count, params (string name, object value)[] args)
        {
            string category = PluralCategory(Language, count);
            string text;
            // Önce seçili dilin kendi biçimleri (TR'de .one yoksa .other), ancak hiçbiri yoksa kaynak dile düşülür.
            if (!TryGetExact(key + "." + category, out text) && !TryGetExact(key + ".other", out text) && !TryGetExact(key, out text)
                && !TryGet(key + "." + PluralCategory(Languages.Source, count), out text) && !TryGet(key + ".other", out text) && !TryGet(key, out text))
            {
                _missing.Add(key + ".other");
                text = key;
            }
            var all = new (string name, object value)[(args?.Length ?? 0) + 1];
            all[0] = ("count", count);
            if (args != null) Array.Copy(args, 0, all, 1, args.Length);
            return Fill(text, all);
        }

        /// <summary>Core'un ürettiği anahtarlı metni çözer (bkz. <see cref="LocText"/>).</summary>
        public string Resolve(LocText text)
        {
            if (text.IsEmpty) return "";
            if (text.Literal != null) return text.Literal;
            return text.Count.HasValue ? Plural(text.Key, text.Count.Value, text.Args) : Format(text.Key, text.Args);
        }

        /// <summary>Hazır bir şablonu doldurur (tablodan gelmeyen metinler için).</summary>
        public string Fill(string template, params (string name, object value)[] args)
        {
            if (string.IsNullOrEmpty(template)) return template ?? "";
            if (template.IndexOf('{') < 0 && template.IndexOf('}') < 0) return template;

            var sb = new StringBuilder(template.Length + 16);
            for (int i = 0; i < template.Length; i++)
            {
                char c = template[i];
                if (c == '{')
                {
                    if (i + 1 < template.Length && template[i + 1] == '{') { sb.Append('{'); i++; continue; }
                    int close = template.IndexOf('}', i + 1);
                    if (close < 0) { sb.Append(template, i, template.Length - i); break; }
                    string name = template.Substring(i + 1, close - i - 1);
                    if (TryArg(args, name, out var value)) sb.Append(ToText(value));
                    else sb.Append('{').Append(name).Append('}'); // eksik argüman görünür kalsın
                    i = close;
                    continue;
                }
                if (c == '}' && i + 1 < template.Length && template[i + 1] == '}') { sb.Append('}'); i++; continue; }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Seçili dilin kurallarıyla büyük harf (Türkçede i -> İ). Anahtar karşılaştırmasında kullanma.</summary>
        public string ToUpper(string s) => s?.ToUpper(Culture);

        /// <summary>Seçili dilin kurallarıyla küçük harf (Türkçede I -> ı).</summary>
        public string ToLower(string s) => s?.ToLower(Culture);

        /// <summary>Sayıyı seçili dilin ondalık ayırıcısıyla yazar.</summary>
        public string Number(double value, int decimals = 0) =>
            value.ToString("N" + Math.Max(0, decimals).ToString(CultureInfo.InvariantCulture), Culture);

        /// <summary>Yüzdeyi seçili dilin biçimiyle yazar (0,12 -> TR "%12", EN "12%", FR "12 %").</summary>
        public string Percent(double ratio, int decimals = 0) =>
            ratio.ToString("P" + Math.Max(0, decimals).ToString(CultureInfo.InvariantCulture), Culture);

        /// <summary>CLDR'nin sade hâli: Fransızcada 0 ve 1 tekil, diğerlerinde yalnız 1.</summary>
        public static string PluralCategory(string language, long count)
        {
            if (Languages.Normalize(language) == Languages.French) return count == 0 || count == 1 ? "one" : "other";
            return count == 1 ? "one" : "other";
        }

        private string ToText(object value)
        {
            switch (value)
            {
                case null: return "";
                case string s: return s;
                case LocText t: return Resolve(t);
                case IFormattable f: return f.ToString(null, Culture);
                default: return value.ToString();
            }
        }

        private static bool TryArg((string name, object value)[] args, string name, out object value)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i].name, name, StringComparison.Ordinal)) { value = args[i].value; return true; }
                }
            }
            value = null;
            return false;
        }

        private static CultureInfo SafeCulture(string name)
        {
            // Bazı mobil derlemelerde (InvariantGlobalization) kültür verisi olmayabilir; o zaman sabit kültüre düş.
            try { return CultureInfo.GetCultureInfo(name); }
            catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
        }
    }
}
