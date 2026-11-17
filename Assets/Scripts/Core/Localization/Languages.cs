using System;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Oyunun dilleri (karar 17.11.2026, Project dokümanı 21): kaynak dil İngilizce; 1.0'da İngilizce ve Türkçe,
    /// İspanyolca, Fransızca ve Almanca sonra. Kodlar ISO 639-1, küçük harf.
    /// </summary>
    public static class Languages
    {
        public const string English = "en";
        public const string Turkish = "tr";
        public const string Spanish = "es";
        public const string French = "fr";
        public const string German = "de";

        /// <summary>Metinlerin önce yazıldığı dil; çeviriler buradan yapılır, eksik çeviride buna düşülür.</summary>
        public const string Source = English;

        /// <summary>Desteklenmesi planlanan bütün diller.</summary>
        public static readonly string[] All = { English, Turkish, Spanish, French, German };

        /// <summary>Oyuncunun seçebildiği diller (tabloları hazır olanlar). Yeni dil eklenince buraya da eklenir.</summary>
        public static readonly string[] Released = { English, Turkish };

        public static bool IsSupported(string code) => IndexOf(All, Normalize(code)) >= 0;

        public static bool IsReleased(string code) => IndexOf(Released, Normalize(code)) >= 0;

        /// <summary>"tr-TR", "TR", " en_US " gibi girdileri "tr"/"en" yapar. Kültürden bağımsızdır (Türkçe cihazda da doğru).</summary>
        public static string Normalize(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var c = code.Trim();
            int cut = c.IndexOfAny(new[] { '-', '_' });
            if (cut > 0) c = c.Substring(0, cut);
            return c.ToLowerInvariant();
        }

        /// <summary>
        /// Açılışta kullanılacak dil: oyuncunun kaydettiği seçim, yoksa cihaz dili, o da yayında değilse kaynak dil.
        /// </summary>
        public static string Pick(string saved, string system)
        {
            var s = Normalize(saved);
            if (s != null && IsReleased(s)) return s;
            var d = Normalize(system);
            if (d != null && IsReleased(d)) return d;
            return Source;
        }

        /// <summary>Sayı ve tarih biçimi için kültür adı.</summary>
        public static string CultureName(string code)
        {
            switch (Normalize(code))
            {
                case Turkish: return "tr-TR";
                case Spanish: return "es-ES";
                case French: return "fr-FR";
                case German: return "de-DE";
                default: return "en-US";
            }
        }

        /// <summary>Dil seçicide gösterilen ad (her dil kendi adıyla).</summary>
        public static string NativeName(string code)
        {
            switch (Normalize(code))
            {
                case Turkish: return "Türkçe";
                case Spanish: return "Español";
                case French: return "Français";
                case German: return "Deutsch";
                case English: return "English";
                default: return code ?? "";
            }
        }

        private static int IndexOf(string[] list, string code)
        {
            if (code == null) return -1;
            for (int i = 0; i < list.Length; i++)
                if (string.Equals(list[i], code, StringComparison.Ordinal)) return i;
            return -1;
        }
    }
}
