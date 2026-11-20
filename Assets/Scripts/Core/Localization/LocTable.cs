using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Tek dilin metin tablosu: anahtar -> metin. Dosya biçimi düz bir JSON nesnesidir
    /// (<c>{"item.egg.name": "Egg", ...}</c>); Unity'nin JsonUtility'si sözlük okuyamadığı için
    /// küçük bir çözümleyici burada, motor bağımlılığı olmadan durur.
    /// </summary>
    public sealed class LocTable
    {
        private readonly Dictionary<string, string> _entries = new Dictionary<string, string>(StringComparer.Ordinal);

        public int Count => _entries.Count;
        public IEnumerable<string> Keys => _entries.Keys;

        public bool TryGet(string key, out string value)
        {
            if (key == null) { value = null; return false; }
            return _entries.TryGetValue(key, out value);
        }

        public bool Contains(string key) => key != null && _entries.ContainsKey(key);

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Anahtar boş olamaz.", nameof(key));
            _entries[key] = value ?? "";
        }

        /// <summary>Başka bir tablonun girişlerini ekler; aynı anahtarda <paramref name="other"/> kazanır.</summary>
        public void Merge(LocTable other)
        {
            if (other == null) return;
            foreach (var kv in other._entries) _entries[kv.Key] = kv.Value;
        }

        /// <summary>
        /// Düz JSON nesnesini okur. Hatalı dosyada okunabilen girişler kalır, hatalar <paramref name="errors"/>'a eklenir.
        /// Değerler yalnızca metin olabilir; iç içe nesne ya da dizi hata sayılır.
        /// </summary>
        public static LocTable Parse(string json, List<string> errors, string sourceName = "tablo")
        {
            var table = new LocTable();
            if (string.IsNullOrWhiteSpace(json)) { errors?.Add($"{sourceName}: dosya boş."); return table; }

            var p = new Parser(json, sourceName);
            try
            {
                p.SkipWs();
                if (p.Peek() == '﻿') { p.Pos++; p.SkipWs(); }
                p.Expect('{');
                p.SkipWs();
                if (p.Peek() == '}') { p.Pos++; return table; }
                while (true)
                {
                    p.SkipWs();
                    string key = p.ReadString();
                    p.SkipWs();
                    p.Expect(':');
                    p.SkipWs();
                    if (p.Peek() != '"') throw p.Error("değer metin olmalı");
                    string value = p.ReadString();
                    if (table._entries.ContainsKey(key)) errors?.Add($"{sourceName}: '{key}' anahtarı iki kez var; sonuncusu geçerli.");
                    table._entries[key] = value;
                    p.SkipWs();
                    char c = p.Next();
                    if (c == ',') continue;
                    if (c == '}') break;
                    throw p.Error("',' ya da '}' bekleniyordu");
                }
                p.SkipWs();
                if (!p.End) throw p.Error("nesneden sonra fazladan içerik");
            }
            catch (FormatException ex)
            {
                errors?.Add(ex.Message);
            }
            return table;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private readonly string _name;
            public int Pos;

            public Parser(string s, string name) { _s = s; _name = name; }

            public bool End => Pos >= _s.Length;
            public char Peek() => End ? '\0' : _s[Pos];
            public char Next() { if (End) throw Error("beklenmedik dosya sonu"); return _s[Pos++]; }

            public void SkipWs()
            {
                while (!End && (_s[Pos] == ' ' || _s[Pos] == '\t' || _s[Pos] == '\n' || _s[Pos] == '\r')) Pos++;
            }

            public void Expect(char c)
            {
                if (Peek() != c) throw Error($"'{c}' bekleniyordu");
                Pos++;
            }

            public string ReadString()
            {
                Expect('"');
                var sb = new StringBuilder();
                while (true)
                {
                    char c = Next();
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    char e = Next();
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (Pos + 4 > _s.Length) throw Error("eksik \\u kaçışı");
                            if (!int.TryParse(_s.Substring(Pos, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                                throw Error("geçersiz \\u kaçışı");
                            sb.Append((char)code);
                            Pos += 4;
                            break;
                        default: throw Error($"geçersiz kaçış '\\{e}'");
                    }
                }
            }

            public FormatException Error(string message)
            {
                int line = 1, col = 1;
                for (int i = 0; i < Pos && i < _s.Length; i++)
                {
                    if (_s[i] == '\n') { line++; col = 1; } else col++;
                }
                return new FormatException($"{_name}: satır {line}, sütun {col}: {message}.");
            }
        }
    }
}
