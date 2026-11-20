using System;
using System.Globalization;
using System.Text;

namespace Thatchwick.Core.Save
{
    /// <summary>
    /// Karo dizileri için koşu uzunluğu kodlaması (kayıt boyutu, performans incelemesi 20.11.2026). Harita dizileri
    /// (engel, zemin, sahiplik, bölge) uzun aynı değer koşularından oluşur; JSON'da karo başına bir değer yazmak
    /// 428×72'lik dünyada dört dizi için ~450 KB tutuyordu. Biçim: "adet:değer,adet:değer"; adet 1 ise yalnızca "değer".
    /// </summary>
    public static class TileRle
    {
        public static string Encode(int[] values)
        {
            if (values == null || values.Length == 0) return "";
            var sb = new StringBuilder();
            int i = 0;
            while (i < values.Length)
            {
                int v = values[i], run = 1;
                while (i + run < values.Length && values[i + run] == v) run++;
                if (sb.Length > 0) sb.Append(',');
                if (run > 1) sb.Append(run.ToString(CultureInfo.InvariantCulture)).Append(':');
                sb.Append(v.ToString(CultureInfo.InvariantCulture));
                i += run;
            }
            return sb.ToString();
        }

        public static string Encode(bool[] values)
        {
            if (values == null || values.Length == 0) return "";
            var ints = new int[values.Length];
            for (int i = 0; i < values.Length; i++) ints[i] = values[i] ? 1 : 0;
            return Encode(ints);
        }

        /// <summary>Çözer; beklenen uzunluktan kısa çıkarsa kalan 0 kalır, uzun çıkarsa fazlası atılır. Bozuk parça yok sayılır.</summary>
        public static int[] Decode(string text, int length)
        {
            var result = new int[Math.Max(0, length)];
            if (string.IsNullOrEmpty(text)) return result;
            int at = 0, pos = 0;
            while (pos < text.Length && at < result.Length)
            {
                int end = text.IndexOf(',', pos);
                if (end < 0) end = text.Length;
                int colon = text.IndexOf(':', pos, end - pos);
                int run = 1, value;
                bool ok = colon < 0
                    ? int.TryParse(text.Substring(pos, end - pos), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                    : int.TryParse(text.Substring(pos, colon - pos), NumberStyles.Integer, CultureInfo.InvariantCulture, out run)
                      & int.TryParse(text.Substring(colon + 1, end - colon - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
                if (ok && run > 0)
                    for (int k = 0; k < run && at < result.Length; k++) result[at++] = value;
                pos = end + 1;
            }
            return result;
        }

        public static bool[] DecodeBools(string text, int length)
        {
            var ints = Decode(text, length);
            var result = new bool[ints.Length];
            for (int i = 0; i < ints.Length; i++) result[i] = ints[i] != 0;
            return result;
        }
    }
}
