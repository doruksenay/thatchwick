using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;

namespace Thatchwick.Game
{
    /// <summary>
    /// İçerik JSON'unu Resources/Content/items.json'dan okur (tüm platformlarda çalışır).
    /// Çözümleme ve doğrulama Core'daki ContentMapper'da; burada yalnızca dosya okuma var.
    /// </summary>
    public static class ContentLoader
    {
        private const string ItemsPath = "Content/items";
        private const string AppearancePath = "Content/appearance";

        public static ContentDatabase Load(out List<string> errors)
        {
            var db = new ContentDatabase();

            var asset = Resources.Load<TextAsset>(ItemsPath);
            if (asset == null)
            {
                errors = new List<string> { $"Resources/{ItemsPath}.json bulunamadı." };
                return db;
            }

            ContentFileDto file;
            try
            {
                file = JsonUtility.FromJson<ContentFileDto>(asset.text);
            }
            catch (ArgumentException ex)
            {
                errors = new List<string> { $"items.json çözümlenemedi: {ex.Message}" };
                return db;
            }

            errors = ContentMapper.Apply(file, db);
            return db;
        }

        /// <summary>
        /// Karakter görünüm kataloğunu (appearance.json) okur. Dosya yoksa ya da hatalıysa
        /// yerleşik varsayılan katalog kullanılır ve hatalar döndürülür.
        /// </summary>
        public static AppearanceCatalog LoadAppearance(out List<string> errors)
        {
            errors = new List<string>();
            var asset = Resources.Load<TextAsset>(AppearancePath);
            if (asset == null)
            {
                errors.Add($"Resources/{AppearancePath}.json bulunamadı; varsayılan katalog kullanılıyor.");
                return AppearanceCatalog.Default();
            }

            AppearanceFileDto file;
            try
            {
                file = JsonUtility.FromJson<AppearanceFileDto>(asset.text);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"appearance.json çözümlenemedi ({ex.Message}); varsayılan katalog kullanılıyor.");
                return AppearanceCatalog.Default();
            }

            var catalog = AppearanceMapper.Build(file, errors);
            if (errors.Count > 0)
            {
                errors.Add("Görünüm kataloğu hatalı; varsayılan katalog kullanılıyor.");
                return AppearanceCatalog.Default();
            }
            return catalog;
        }
    }
}
