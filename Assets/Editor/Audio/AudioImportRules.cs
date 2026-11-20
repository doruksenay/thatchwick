#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Thatchwick.EditorTools.Audio
{
    /// <summary>
    /// Resources/Audio altındaki seslerin içe aktarma ayarını klasöre göre verir (Project 45 bölüm 8).
    /// Elle ayar yapılmaz; yeni ses klasöre düşünce doğru ayarla gelir.
    ///   Sfx, Ui, Voice  → mono, Decompress On Load (kısa, sık çalan)
    ///   Ambience        → Compressed In Memory
    ///   Music           → Streaming, arka planda yükleme
    /// </summary>
    public sealed class AudioImportRules : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            string p = assetPath.Replace('\\', '/');
            const string root = "/Resources/Audio/";
            int i = p.IndexOf(root, System.StringComparison.Ordinal);
            if (i < 0) return;
            string rest = p.Substring(i + root.Length);
            int slash = rest.IndexOf('/');
            string group = slash > 0 ? rest.Substring(0, slash) : "";

            var imp = (AudioImporter)assetImporter;
            var s = imp.defaultSampleSettings;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            switch (group)
            {
                case "Music":
                    s.loadType = AudioClipLoadType.Streaming;
                    s.quality = 0.6f;
                    imp.forceToMono = false;
                    imp.loadInBackground = true;
                    s.preloadAudioData = false;
                    break;
                case "Ambience":
                    s.loadType = AudioClipLoadType.CompressedInMemory;
                    s.quality = 0.5f;
                    imp.forceToMono = false;
                    imp.loadInBackground = true;
                    s.preloadAudioData = false;
                    break;
                default: // Sfx, Ui, Voice
                    s.loadType = AudioClipLoadType.DecompressOnLoad;
                    s.quality = 0.55f;
                    imp.forceToMono = true;
                    imp.loadInBackground = false;
                    s.preloadAudioData = true;
                    break;
            }
            imp.defaultSampleSettings = s;
        }

        [MenuItem("Thatchwick/Ses/Tekrar raporu")]
        private static void Report()
        {
            if (!Application.isPlaying) { Debug.Log("[Ses] Rapor için oyunu Play'de çalıştır."); return; }
            Debug.Log(Thatchwick.Game.Audio.AudioService.Instance.Report());
        }
    }
}
#endif
