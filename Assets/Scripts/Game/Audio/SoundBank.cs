using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game.Audio
{
    public enum AudioGroup { Music, Ambience, Sfx, Voice, Ui }

    [Serializable]
    public class SoundLayer
    {
        public string name;
        public string[] clips;
    }

    /// <summary>soundbank.json girdisi (tools/audio/promote.py yazar; elle düzenlenmez).</summary>
    [Serializable]
    public class SoundEntry
    {
        public string key;
        public string group;
        public string family;
        public string freq;
        public bool loop;
        public string[] clips;
        public SoundLayer[] layers;
        public float pitchVar;
        public float volVarDb;
        public float timingJitterMs;
        public float cooldownMs;
        public int maxVoices;
        public float volumeDb;
        public string context;
        public string spotOf;
        public float spotIntervalMin;
        public float spotIntervalMax;
        public float[] todWeights;
        public float[] seasonWeights;
        public string[] syllables;

        [NonSerialized] public AudioGroup Group;
        public bool IsLayered => layers != null && layers.Length > 1;
    }

    [Serializable]
    public class SoundBankFile
    {
        public int version;
        public string updated;
        public SoundEntry[] entries;
    }

    /// <summary>
    /// Ses bankası: Resources/Audio/soundbank.json. Klipler Resources'tan ilk çalışta yüklenir ve önbelleğe girer.
    /// Klibi olmayan anahtar sessiz geçer; geliştirici sürümünde "eksik sesler" listesine düşer (Project 45 bölüm 8).
    /// </summary>
    public sealed class SoundBank
    {
        private readonly Dictionary<string, SoundEntry> _entries = new Dictionary<string, SoundEntry>();
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        public readonly HashSet<string> MissingKeys = new HashSet<string>();
        public IEnumerable<SoundEntry> Entries => _entries.Values;
        public int Count => _entries.Count;

        public static SoundBank Load(string resourcePath = "Audio/soundbank")
        {
            var bank = new SoundBank();
            var ta = Resources.Load<TextAsset>(resourcePath);
            if (ta == null)
            {
                Debug.Log("[Ses] soundbank.json yok; oyun sessiz çalışır.");
                return bank;
            }
            var file = JsonUtility.FromJson<SoundBankFile>(ta.text);
            if (file?.entries == null) return bank;
            foreach (var e in file.entries)
            {
                if (string.IsNullOrEmpty(e.key)) continue;
                e.Group = ParseGroup(e.group);
                if (e.clips == null) e.clips = Array.Empty<string>();
                if (e.layers == null) e.layers = Array.Empty<SoundLayer>();
                bank._entries[e.key] = e;
            }
            return bank;
        }

        public static AudioGroup ParseGroup(string g)
        {
            switch (g)
            {
                case "Music": return AudioGroup.Music;
                case "Ambience": return AudioGroup.Ambience;
                case "Voice": return AudioGroup.Voice;
                case "Ui": return AudioGroup.Ui;
                default: return AudioGroup.Sfx;
            }
        }

        public bool TryGet(string key, out SoundEntry e)
        {
            if (key != null && _entries.TryGetValue(key, out e)) return true;
            e = null;
            if (key != null && MissingKeys.Add(key) && Debug.isDebugBuild)
                Debug.Log("[Ses] eksik ses anahtarı: " + key + " (tools/audio/gaps.py)");
            return false;
        }

        public bool Has(string key) => key != null && _entries.ContainsKey(key);

        public AudioClip Clip(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_clips.TryGetValue(path, out var c) && c != null) return c;
            c = Resources.Load<AudioClip>(path);
            if (c == null && Debug.isDebugBuild) Debug.LogWarning("[Ses] klip bulunamadı: " + path);
            _clips[path] = c;
            return c;
        }
    }
}
