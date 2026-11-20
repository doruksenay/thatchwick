using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Localization;

namespace Thatchwick.Game
{
    /// <summary>
    /// İçerik JSON'unu Resources/Content/items.json ve life.json'dan okur (tüm platformlarda çalışır).
    /// Çözümleme ve doğrulama Core'daki ContentMapper'da; burada yalnızca dosya okuma var.
    /// </summary>
    public static class ContentLoader
    {
        private const string ItemsPath = "Content/items";
        private const string LifePath = "Content/life";
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

            // life.json (özellik, köylü, festival, ...) isteğe bağlıdır; varsa items.json ile birleştirilir.
            var lifeAsset = Resources.Load<TextAsset>(LifePath);
            var loadErrors = new List<string>();
            if (lifeAsset != null)
            {
                try { file.Merge(JsonUtility.FromJson<ContentFileDto>(lifeAsset.text)); }
                catch (ArgumentException ex) { loadErrors.Add($"life.json çözümlenemedi: {ex.Message}"); }
            }

            // Oyuncuya görünen adlar seçili dile çevrilir (Resources/Localization/content.<dil>.json).
            ContentLocalizer.Apply(file, GameLocalization.Current);
            ContentMapper.GeneratedText = ContentLocalizer.Generated(GameLocalization.Current);

            errors = ContentMapper.Apply(file, db);
            errors.AddRange(loadErrors);
            return db;
        }

        private const string ClothingPath = "Content/clothing";

        /// <summary>Giysi kataloğunu (clothing.json; belge 48, 51) okur. Dosya yoksa boş katalog.</summary>
        public static Thatchwick.Core.Clothing.ClothingCatalog LoadClothing(ContentDatabase content, out List<string> errors)
        {
            errors = new List<string>();
            var asset = Resources.Load<TextAsset>(ClothingPath);
            if (asset == null) { errors.Add($"Resources/{ClothingPath}.json bulunamadı."); return new Thatchwick.Core.Clothing.ClothingCatalog(); }
            try
            {
                var file = JsonUtility.FromJson<Thatchwick.Core.Clothing.ClothingFileDto>(asset.text);
                return Thatchwick.Core.Clothing.ClothingCatalog.Map(file, content, errors);
            }
            catch (ArgumentException ex) { errors.Add($"clothing.json çözümlenemedi: {ex.Message}"); return new Thatchwick.Core.Clothing.ClothingCatalog(); }
        }

        private const string LayoutsPath = "Content/layouts";

        /// <summary>
        /// Harita taslakları (belge 64a): Resources/Content/maps/&lt;bölge&gt;/terrain.png (okunabilir doku, IsoImportPostprocessor)
        /// ve markers.json. Core taslağı ilk istendiğinde bu kaynaktan okur; dosyası olmayan bölge kod çizimiyle kurulur.
        /// </summary>
        public static void InstallMaps()
        {
            Thatchwick.Core.World.MapFiles.Reset();
            Thatchwick.Core.World.MapFiles.Provider = id =>
            {
                var tex = Resources.Load<Texture2D>($"Content/maps/{id}/terrain");
                if (tex == null) return null;
                Color32[] px;
                try { px = tex.GetPixels32(); }
                catch (Exception ex) { Debug.LogError($"maps/{id}/terrain.png okunamıyor (Read/Write kapalı?): {ex.Message}"); return null; }
                int w = tex.width, h = tex.height;
                var rgba = new byte[w * h * 4];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var c = px[(h - 1 - y) * w + x];   // Unity'de satır 0 alttadır; taslakta üstte
                        int o = (y * w + x) * 4;
                        rgba[o] = c.r; rgba[o + 1] = c.g; rgba[o + 2] = c.b; rgba[o + 3] = c.a;
                    }
                var mk = Resources.Load<TextAsset>($"Content/maps/{id}/markers");
                var markers = mk != null ? JsonUtility.FromJson<Thatchwick.Core.World.MapMarkersDto>(mk.text) : null;
                var map = Thatchwick.Core.World.RegionMap.FromRgba(id, w, h, rgba, markers, Thatchwick.Core.World.MapFiles.Errors);
                foreach (var e in Thatchwick.Core.World.MapFiles.Errors) Debug.LogWarning(e);
                return map;
            };
        }

        /// <summary>Yerleşim şablonlarını (layouts.json: oda, bina, bahçe; belge 44) okur. Dosya yoksa null (iç mekânlar boş kalır).</summary>
        public static Thatchwick.Core.Layout.LayoutCatalog LoadLayouts(ContentDatabase content, out List<string> errors)
        {
            errors = new List<string>();
            var asset = Resources.Load<TextAsset>(LayoutsPath);
            if (asset == null) { errors.Add($"Resources/{LayoutsPath}.json bulunamadı."); return null; }
            try
            {
                var file = JsonUtility.FromJson<Thatchwick.Core.Layout.LayoutFileDto>(asset.text);
                return Thatchwick.Core.Layout.LayoutCatalog.Map(file, content, errors);
            }
            catch (ArgumentException ex) { errors.Add($"layouts.json çözümlenemedi: {ex.Message}"); return null; }
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

            ContentLocalizer.Apply(file, GameLocalization.Current);
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
