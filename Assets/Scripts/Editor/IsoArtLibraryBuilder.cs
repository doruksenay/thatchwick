using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Thatchwick.Game;

namespace Thatchwick.Editor
{
    /// <summary>
    /// Assets/Art/Iso altındaki PNG'leri (ham taslak ve animasyon denemeleri hariç) tarayıp
    /// Resources/IsoArt/IsoArtLibrary.asset'i kurar. Görseller eklenince/silinince kendiliğinden yenilenir;
    /// elle: Thatchwick > Görsel kütüphanesini yenile. İçe aktarma ayarlarını da piksel sanatına uygun yapar
    /// (Sprite, Point, sıkıştırmasız, mipmap yok, PPU 32).
    /// </summary>
    public sealed class IsoArtLibraryBuilder : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/Art/Iso";
        public const string LibraryPath = "Assets/Resources/IsoArt/IsoArtLibrary.asset";
        private static bool _queued;

        private static bool IsArt(string path) =>
            path.Replace('\\', '/').StartsWith(ArtRoot + "/") && path.EndsWith(".png")
            && !path.Contains("/_ham/") && !path.Contains("/anim/");

        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(ArtRoot + "/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 32;
            importer.alphaIsTransparency = true;
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            bool changed = false;
            foreach (var p in imported) if (IsArt(p)) changed = true;
            foreach (var p in deleted) if (IsArt(p)) changed = true;
            foreach (var p in moved) if (IsArt(p)) changed = true;
            if (changed) Queue();
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (!File.Exists(LibraryPath)) Queue();
        }

        private static void Queue()
        {
            if (_queued) return;
            _queued = true;
            EditorApplication.delayCall += () => { _queued = false; Build(); };
        }

        [MenuItem("Thatchwick/Görsel kütüphanesini yenile")]
        public static void Build()
        {
            var entries = new List<IsoArtLibrary.Entry>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsArt(path)) continue;
                // Daha önce varsayılan ayarlarla (bulanık/sıkıştırılmış) içe aktarılmış dosyaları düzelt.
                if (AssetImporter.GetAtPath(path) is TextureImporter ti &&
                    (ti.filterMode != FilterMode.Point || ti.textureCompression != TextureImporterCompression.Uncompressed || ti.mipmapEnabled))
                {
                    ti.filterMode = FilterMode.Point;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.mipmapEnabled = false;
                    ti.SaveAndReimport();
                }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                entries.Add(new IsoArtLibrary.Entry { name = Path.GetFileNameWithoutExtension(path), texture = tex });
            }
            entries.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath));
            var lib = AssetDatabase.LoadAssetAtPath<IsoArtLibrary>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<IsoArtLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }
            lib.entries = entries;
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            IsoArt.Reset();
            Debug.Log($"IsoArt: {entries.Count} görsel kütüphaneye yazıldı ({LibraryPath}).");
        }
    }
}
