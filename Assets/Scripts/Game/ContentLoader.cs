using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Content;

namespace Thatchwick.Game
{
    /// <summary>
    /// İçerik JSON'unu Resources/Content/items.json'dan okur (tüm platformlarda çalışır).
    /// Çözümleme ve doğrulama Core'daki ContentMapper'da; burada yalnızca dosya okuma var.
    /// </summary>
    public static class ContentLoader
    {
        private const string ItemsPath = "Content/items";

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
    }
}
