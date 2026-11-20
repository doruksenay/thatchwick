using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Localization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Oyunun seçili dili ve metin tabloları. Tablolar Resources/Localization altındadır:
    /// <c>content.&lt;dil&gt;.json</c> (eşya, tarif, beceri dalı ... adları) ve <c>ui.&lt;dil&gt;.json</c> (arayüz metinleri).
    /// Seçim PlayerPrefs'e yazılır; ilk açılışta cihaz dili, desteklenmiyorsa İngilizce.
    /// </summary>
    public static class GameLocalization
    {
        private const string PrefKey = "thatchwick.language";
        private static readonly string[] TableNames = { "content", "ui" };

        private static Localizer _current;

        /// <summary>Dil değişince (yeni seçim kaydedildikten sonra) çağrılır.</summary>
        public static event Action<string> LanguageChanged;

        public static Localizer Current => _current ?? (_current = Build(Languages.Pick(SavedLanguage, SystemLanguageCode())));

        public static string SavedLanguage => PlayerPrefs.HasKey(PrefKey) ? PlayerPrefs.GetString(PrefKey) : null;

        /// <summary>
        /// Dili kaydeder ve tabloları yeniden yükler. Arayüz metinleri hemen değişir; içerik adları (eşya, tarif ...)
        /// içerik yeniden yüklenince, yani oyun yeniden açılınca değişir.
        /// </summary>
        public static void SetLanguage(string code)
        {
            var lang = Languages.Normalize(code);
            if (!Languages.IsReleased(lang)) return;
            PlayerPrefs.SetString(PrefKey, lang);
            PlayerPrefs.Save();
            if (_current != null && _current.Language == lang) return;
            _current = Build(lang);
            LanguageChanged?.Invoke(lang);
        }

        /// <summary>Kısa yol: seçili dilde metin.</summary>
        public static string T(string key) => Current.Get(key);

        /// <summary>İçerik kimliğinden (item.…, trait.…) seçili dildeki ad; GameRoot içerik yüklenince kurar.</summary>
        public static Func<string, string> ContentName { get; set; }

        /// <summary>Kayıttan gelen metni (hayat notu, kronik) seçili dilde gösterir; eski düz metin olduğu gibi kalır.</summary>
        public static string Saved(string saved) => SavedText.Resolve(saved, Current, ContentName);

        public static string T(string key, params (string name, object value)[] args) => Current.Format(key, args);

        /// <summary>Eşya kimliğinden görünen ad (GameRoot içerik yüklenince verir; yoksa kimlik).</summary>
        public static Func<string, string> ItemName { get; set; }

        /// <summary>Core'un anahtarlı metnini seçili dilde çözer: iç içe metinler ve "item." argümanları dahil.</summary>
        public static string R(LocText t) => LocResolve.Deep(Current, t, ItemName);

        private static Localizer Build(string language)
        {
            var errors = new List<string>();
            var table = LoadTables(language, errors);
            var fallback = language == Languages.Source ? null : LoadTables(Languages.Source, errors);
            foreach (var e in errors) Debug.LogWarning($"Yerelleştirme: {e}");
            return new Localizer(language, table, fallback);
        }

        private static LocTable LoadTables(string language, List<string> errors)
        {
            var table = new LocTable();
            foreach (var name in TableNames)
            {
                // Resources yolunda uzantı yazılmaz: "Localization/content.en" -> content.en.json
                var asset = Resources.Load<TextAsset>($"Localization/{name}.{language}");
                if (asset == null) continue; // henüz çevrilmemiş tablo: kaynak dile düşülür
                table.Merge(LocTable.Parse(asset.text, errors, $"{name}.{language}.json"));
            }
            return table;
        }

        private static string SystemLanguageCode()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Turkish: return Languages.Turkish;
                case SystemLanguage.Spanish: return Languages.Spanish;
                case SystemLanguage.French: return Languages.French;
                case SystemLanguage.German: return Languages.German;
                case SystemLanguage.English: return Languages.English;
                default: return null;
            }
        }
    }
}
