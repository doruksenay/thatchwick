using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// <see cref="LocText"/>'i kayıt için tek satır metne çevirir ve geri okur (belge 21: kayda çevrilmiş metin değil anahtar yazılır).
    /// İç içe anahtarlı argümanlar (ör. sipariş başlığındaki eşya, bildirimdeki istek başlığı) korunur.
    /// Biçim: <c>anahtar|ad:s değer|ad:n 12|ad:t(...)</c>; düz metin <c>=metin</c>; sayılı metin <c>@sayı</c> eki.
    /// Ayraçlar ters bölüyle kaçar. Ayrıştırılamayan eski metin (boşluklu, noktasız) düz metin sayılır.
    /// </summary>
    public static class LocCodec
    {
        public static string Encode(LocText t)
        {
            if (t.IsEmpty) return "";
            var sb = new StringBuilder();
            Write(sb, t);
            return sb.ToString();
        }

        private static void Write(StringBuilder sb, LocText t)
        {
            if (t.Literal != null) { sb.Append('='); Esc(sb, t.Literal); return; }
            Esc(sb, t.Key ?? "");
            if (t.Count.HasValue) sb.Append('@').Append(t.Count.Value.ToString(CultureInfo.InvariantCulture));
            foreach (var (name, value) in t.Args)
            {
                sb.Append('|');
                Esc(sb, name);
                switch (value)
                {
                    case LocText lt: sb.Append(":t("); Write(sb, lt); sb.Append(')'); break;
                    case string s: sb.Append(":s"); Esc(sb, s); break;
                    case null: sb.Append(":s"); break;
                    case IFormattable f: sb.Append(":n"); Esc(sb, f.ToString(null, CultureInfo.InvariantCulture)); break;
                    default: sb.Append(":s"); Esc(sb, value.ToString()); break;
                }
            }
        }

        private static void Esc(StringBuilder sb, string s)
        {
            foreach (char c in s)
            {
                if (c == '\\' || c == '|' || c == ':' || c == '(' || c == ')' || c == '@' || c == '=') sb.Append('\\');
                sb.Append(c);
            }
        }

        public static LocText Decode(string s)
        {
            if (string.IsNullOrEmpty(s)) return default;
            // Eski kayıt: düz Türkçe metin (boşluk içerir ve kaçışsızdır).
            if (s[0] != '=' && LooksPlain(s)) return LocText.Plain(s);
            int i = 0;
            try { return Read(s, ref i); }
            catch (FormatException) { return LocText.Plain(s); }
        }

        private static bool LooksPlain(string s)
        {
            int bar = s.IndexOf('|');
            string head = bar < 0 ? s : s.Substring(0, bar);
            return head.IndexOf(' ') >= 0 || head.IndexOf('.') < 0;
        }

        private static LocText Read(string s, ref int i)
        {
            if (i < s.Length && s[i] == '=') { i++; return LocText.Plain(Token(s, ref i)); }
            string key = Token(s, ref i);
            long? count = null;
            if (i < s.Length && s[i] == '@')
            {
                i++;
                count = long.Parse(Token(s, ref i), CultureInfo.InvariantCulture);
            }
            var args = new List<(string, object)>();
            while (i < s.Length && s[i] == '|')
            {
                i++;
                string name = Token(s, ref i);
                if (i + 1 >= s.Length || s[i] != ':') throw new FormatException();
                char kind = s[i + 1];
                i += 2;
                object value;
                if (kind == 't')
                {
                    if (i >= s.Length || s[i] != '(') throw new FormatException();
                    i++;
                    value = Read(s, ref i);
                    if (i >= s.Length || s[i] != ')') throw new FormatException();
                    i++;
                }
                else if (kind == 'n')
                {
                    string raw = Token(s, ref i);
                    if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l)) value = l;
                    else value = double.Parse(raw, CultureInfo.InvariantCulture);
                }
                else value = Token(s, ref i);
                args.Add((name, value));
            }
            return count.HasValue ? LocText.Counted(key, count.Value, args.ToArray()) : LocText.Of(key, args.ToArray());
        }

        /// <summary>Kaçışsız ayraca ('|', ':', '(', ')', '@') kadar okur.</summary>
        private static string Token(string s, ref int i)
        {
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length) { sb.Append(s[i + 1]); i += 2; continue; }
                if (c == '|' || c == ':' || c == '(' || c == ')' || c == '@') break;
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Anahtarlı metni derinlemesine çözer: iç içe <see cref="LocText"/> argümanları önce çözülür, "item." ile başlayan dizgi
    /// argümanları eşya adına çevrilir (cümle başında değilse dilin kuralıyla küçük harfe iner). Game katmanı ve testler kullanır.
    /// </summary>
    public static class LocResolve
    {
        public static string Deep(Localizer loc, LocText t, Func<string, string> itemName)
        {
            if (t.IsEmpty) return "";
            if (t.Literal != null) return t.Literal;
            string template = t.Count.HasValue ? null : loc.Get(t.Key);
            var args = new (string name, object value)[t.Args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var (n, val) = t.Args[i];
                if (val is LocText lt) val = Deep(loc, lt, itemName);
                else if (val is string str && str.StartsWith("item.", StringComparison.Ordinal))
                {
                    string name = itemName?.Invoke(str) ?? str;
                    bool first = template != null && template.StartsWith("{" + n + "}", StringComparison.Ordinal);
                    val = first ? name : loc.ToLower(name);
                }
                args[i] = (n, val);
            }
            return t.Count.HasValue ? loc.Plural(t.Key, t.Count.Value, args) : loc.Fill(template, args);
        }
    }
}
