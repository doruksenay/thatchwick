using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Thatchwick.EditorTools
{
    // Assets/Art/Iso altindaki PNG dosyalarini ad kuralina gore otomatik iceri aktarir.
    // Ortak ayar: Sprite, tek parca, PPU 32, Point filtre, sikistirma yok, mipmap yok.
    // Pivot: once Assets/Editor/IsoPivots.txt tablosu (en uzun on ek eslesmesi), sonra kurallar:
    //   iso_ground_*  -> merkez (64x32 elmas beklenir)
    //   iso_char_* ve iso_animal_* -> x merkez, y ayak alti (SE dosyasindan olculur, tum yonler ayni pivotu alir)
    //   diger         -> tabloda yoksa merkez + uyari (tabloya eklenmeli)
    public class IsoImportPostprocessor : AssetPostprocessor
    {
        const string Root = "Assets/Art/Iso/";
        const string PivotTablePath = "Assets/Editor/IsoPivots.txt";
        static readonly string[] DirTokens = { "se", "sw", "nw", "ne", "s", "w", "n", "e" };

        public override uint GetVersion() { return 2; }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root, StringComparison.OrdinalIgnoreCase)) return;
            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;
            var ti = (TextureImporter)assetImporter;
            int w, h;
            if (!TryReadPngSize(assetPath, out w, out h)) { w = 1; h = 1; }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 32;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;

            var pivot = ComputePivot(w, h);
            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            ti.SetTextureSettings(s);
            ti.spritePivot = pivot;
        }

        Vector2 ComputePivot(int w, int h)
        {
            string stem = Path.GetFileNameWithoutExtension(assetPath);

            string best = null;
            var bestPx = Vector2.zero;
            foreach (var kv in LoadTable())
            {
                if (stem.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase) && (best == null || kv.Key.Length > best.Length))
                { best = kv.Key; bestPx = kv.Value; }
            }
            if (best != null) return new Vector2(bestPx.x / w, bestPx.y / h);

            if (stem.StartsWith("iso_ground_", StringComparison.OrdinalIgnoreCase))
            {
                if (w != 64 || h != 32)
                    Debug.LogWarning("IsoImport: " + assetPath + " zemin karosu 64x32 olmali, bulunan " + w + "x" + h);
                return new Vector2(0.5f, 0.5f);
            }

            if (stem.StartsWith("iso_char_", StringComparison.OrdinalIgnoreCase) || stem.StartsWith("iso_animal_", StringComparison.OrdinalIgnoreCase))
            {
                string measurePath = SiblingSePath(assetPath);
                int emptyBelow, hh;
                if (File.Exists(Path.GetFullPath(measurePath)) && TryMeasureFeetRows(measurePath, out emptyBelow, out hh) && hh == h)
                    return new Vector2(0.5f, (float)emptyBelow / h);
                if (TryMeasureFeetRows(assetPath, out emptyBelow, out hh))
                    return new Vector2(0.5f, (float)emptyBelow / h);
            }

            Debug.LogWarning("IsoImport: pivot tablosunda yok, merkez kullanildi: " + assetPath + " (Assets/Editor/IsoPivots.txt dosyasina ekleyin)");
            return new Vector2(0.5f, 0.5f);
        }

        static string SiblingSePath(string path)
        {
            string dir = Path.GetDirectoryName(path).Replace("\\", "/");
            string stem = Path.GetFileNameWithoutExtension(path);
            foreach (var t in DirTokens)
            {
                string suffix = "_" + t;
                if (stem.EndsWith(suffix, StringComparison.Ordinal))
                    return dir + "/" + stem.Substring(0, stem.Length - suffix.Length) + "_se.png";
            }
            return path;
        }

        static List<KeyValuePair<string, Vector2>> LoadTable()
        {
            var list = new List<KeyValuePair<string, Vector2>>();
            if (!File.Exists(PivotTablePath)) return list;
            foreach (var raw in File.ReadAllLines(PivotTablePath))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                var p = line.Split(new string[] { ";" }, StringSplitOptions.None);
                if (p.Length < 3) continue;
                float x, y;
                if (!float.TryParse(p[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x)) continue;
                if (!float.TryParse(p[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y)) continue;
                list.Add(new KeyValuePair<string, Vector2>(p[0].Trim(), new Vector2(x, y)));
            }
            return list;
        }

        static bool TryReadPngSize(string assetPath, out int w, out int h)
        {
            w = 0; h = 0;
            try
            {
                using (var fs = File.OpenRead(Path.GetFullPath(assetPath)))
                {
                    var b = new byte[24];
                    if (fs.Read(b, 0, 24) < 24) return false;
                    w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
                    h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
                    return w > 0 && h > 0;
                }
            }
            catch (Exception) { return false; }
        }

        // Satir 0 = alt satir. En alttaki opak satirin altinda kac bos satir var?
        static bool TryMeasureFeetRows(string path, out int emptyBelow, out int height)
        {
            emptyBelow = 0; height = 0;
            try
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(tex, File.ReadAllBytes(Path.GetFullPath(path)))) { UnityEngine.Object.DestroyImmediate(tex); return false; }
                var px = tex.GetPixels32();
                int w = tex.width;
                height = tex.height;
                UnityEngine.Object.DestroyImmediate(tex);
                for (int row = 0; row < height; row++)
                    for (int x = 0; x < w; x++)
                        if (px[row * w + x].a > 0) { emptyBelow = row; return true; }
            }
            catch (Exception) { }
            return false;
        }

        [MenuItem("Thatchwick/Art/Reimport Iso")]
        public static void ReimportIso()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Iso" });
            int n = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var g in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    if (!p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
                    n++;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            Debug.Log("IsoImport: " + n + " dosya yeniden iceri aktarildi.");
        }
    }
}
