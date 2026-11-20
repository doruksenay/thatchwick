using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Thatchwick.Core.Art;
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
            && !path.Contains("/_ham/")
            // anim/: yalnızca gerçekçi taban (84 yetişkin/yaşlı, 64 çocuk) karelerini al (eski 56'lık deneme kareleri kütüphaneye girmez).
            && (!path.Contains("/anim/") || path.Contains("_r84_") || path.Contains("_r64_"));

        private void OnPreprocessTexture()
        {
            if (assetPath.Replace('\\', '/').StartsWith(AtlasRoot + "/"))
            {
                // Atlas sayfası: piksel sanatı ayarları; okunmaz (bellekte tek kopya), mipmap yok.
                var atlas = (TextureImporter)assetImporter;
                atlas.textureType = TextureImporterType.Default;
                atlas.npotScale = TextureImporterNPOTScale.None;
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;
                atlas.textureCompression = TextureImporterCompression.Uncompressed;
                atlas.mipmapEnabled = false;
                atlas.alphaIsTransparency = true;
                atlas.isReadable = false;
                atlas.maxTextureSize = AtlasPageSize;
                return;
            }
            if (!assetPath.Replace('\\', '/').StartsWith(ArtRoot + "/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 32;
            importer.alphaIsTransparency = true;
            // Katmanlı karakter (belge 43): bölge haritaları ve baş parçaları oyunda piksel piksel okunur.
            if (assetPath.Replace('\\', '/').Contains("/katman/")) importer.isReadable = true;
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
            var previous = new Dictionary<string, IsoArtLibrary.Entry>();
            var oldLib = AssetDatabase.LoadAssetAtPath<IsoArtLibrary>(LibraryPath);
            if (oldLib != null) foreach (var e in oldLib.entries) if (e != null && e.name != null) previous[e.name] = e;
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
                string name = Path.GetFileNameWithoutExtension(path);
                string hash = AssetDatabase.GetAssetDependencyHash(path).ToString();
                // Atlasa alınmış ve o günden beri değişmemiş görsel atlasta kalır (özgün doku kütüphaneye girmez);
                // yeni ya da değişen görsel, atlas yeniden kurulana dek kendi dokusuyla çalışır.
                if (previous.TryGetValue(name, out var old) && old.atlas != null && old.sourceHash == hash && AtlasEligible(path, name))
                    entries.Add(new IsoArtLibrary.Entry { name = name, atlas = old.atlas, rect = old.rect, sourceHash = hash });
                else
                    entries.Add(new IsoArtLibrary.Entry { name = name, texture = tex, sourceHash = hash });
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

        // ---------------- atlas (performans incelemesi 20.11.2026) ----------------
        // Görseller 2048'lik sayfalara piksel tam (ölçeklemesiz) dizilir; IsoArt sprite'ları sayfanın alt dikdörtgeninden üretir.
        // Böylece aynı sayfadaki eşya, zemin ve karakterler toplu çizilir ve özgün PNG'ler yapıya girmez.
        // Atlas dışı: katman parçaları (oyunda piksel piksel okunur), ikonlar, cam bahçe ve arayüz görselleri (IMGUI dokunun
        // tamamını çizer) ve sayfaya sığmayanlar.

        public const string AtlasRoot = "Assets/Art/IsoAtlas";
        public const int AtlasPageSize = 2048;
        private const int AtlasPadding = 2;

        private static bool AtlasEligible(string path, string name) =>
            !path.Replace('\\', '/').Contains("/katman/")
            && !name.StartsWith("icon_") && !name.StartsWith("viv_") && !name.StartsWith("ui_");

        // Gruplama: aynı türden görseller (iso_ground, iso_item, iso_char...) aynı sayfalarda toplansın.
        private static string AtlasGroup(string name)
        {
            int a = name.IndexOf('_');
            int b = a >= 0 ? name.IndexOf('_', a + 1) : -1;
            return b > 0 ? name.Substring(0, b) : name;
        }

        [MenuItem("Thatchwick/Görsel atlasını kur")]
        public static void BuildAtlas()
        {
            Build();   // kütüphane güncel olsun
            var lib = AssetDatabase.LoadAssetAtPath<IsoArtLibrary>(LibraryPath);
            if (lib == null) return;

            var paths = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsArt(path)) paths[Path.GetFileNameWithoutExtension(path)] = path;
            }

            // Kaynakları dosyadan oku (içe aktarılmış doku okunabilir değil).
            var items = new List<(IsoArtLibrary.Entry entry, Texture2D pixels)>();
            try
            {
                foreach (var e in lib.entries)
                {
                    if (e == null || !paths.TryGetValue(e.name, out var path) || !AtlasEligible(path, e.name)) continue;
                    var tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!tmp.LoadImage(File.ReadAllBytes(path))) { Object.DestroyImmediate(tmp); continue; }
                    items.Add((e, tmp));
                }
                items.Sort((x, y) =>
                {
                    int c = string.CompareOrdinal(AtlasGroup(x.entry.name), AtlasGroup(y.entry.name));
                    if (c != 0) return c;
                    c = y.pixels.height.CompareTo(x.pixels.height);   // raf içinde boy sırası: daha az boşluk
                    return c != 0 ? c : string.CompareOrdinal(x.entry.name, y.entry.name);
                });

                var sizes = new List<(int w, int h)>();
                foreach (var it in items) sizes.Add((it.pixels.width, it.pixels.height));
                var placed = ShelfPacker.Pack(sizes, AtlasPageSize, AtlasPadding, out int pageCount);

                Directory.CreateDirectory(AtlasRoot);
                var pagePaths = new List<string>();
                for (int p = 0; p < pageCount; p++)
                {
                    var page = new Texture2D(AtlasPageSize, AtlasPageSize, TextureFormat.RGBA32, false);
                    var clear = new Color32[AtlasPageSize * AtlasPageSize];
                    page.SetPixels32(clear);
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (!placed[i].Packed || placed[i].Page != p) continue;
                        var src = items[i].pixels;
                        page.SetPixels32(placed[i].X, placed[i].Y, src.width, src.height, src.GetPixels32());
                    }
                    string pagePath = $"{AtlasRoot}/iso_atlas_{p:00}.png";
                    File.WriteAllBytes(pagePath, page.EncodeToPNG());
                    Object.DestroyImmediate(page);
                    pagePaths.Add(pagePath);
                }
                // Artık kullanılmayan eski sayfalar silinir.
                foreach (var old in Directory.GetFiles(AtlasRoot, "iso_atlas_*.png"))
                {
                    var norm = old.Replace('\\', '/');
                    if (!pagePaths.Contains(norm)) AssetDatabase.DeleteAsset(norm);
                }
                AssetDatabase.Refresh();
                foreach (var pp in pagePaths) AssetDatabase.ImportAsset(pp, ImportAssetOptions.ForceUpdate);

                int atlased = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    var e = items[i].entry;
                    if (!placed[i].Packed) continue;
                    e.atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(pagePaths[placed[i].Page]);
                    if (e.atlas == null) continue;
                    e.rect = new Rect(placed[i].X, placed[i].Y, items[i].pixels.width, items[i].pixels.height);
                    e.texture = null;
                    if (paths.TryGetValue(e.name, out var src)) e.sourceHash = AssetDatabase.GetAssetDependencyHash(src).ToString();
                    atlased++;
                }
                EditorUtility.SetDirty(lib);
                AssetDatabase.SaveAssets();
                IsoArt.Reset();
                Debug.Log($"IsoArt atlası: {atlased} görsel {pageCount} sayfaya ({AtlasPageSize}px) dizildi; {lib.entries.Count - atlased} görsel atlas dışı.");
            }
            finally
            {
                foreach (var it in items) if (it.pixels != null) Object.DestroyImmediate(it.pixels);
            }
        }

    }

    /// <summary>Oyun yapısından (build) önce görsel atlası güncellenir: yeni görseller de sayfalara girer.</summary>
    public sealed class IsoArtAtlasBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => IsoArtLibraryBuilder.BuildAtlas();
    }
}
