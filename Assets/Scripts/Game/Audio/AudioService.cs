using System.Collections.Generic;
using Thatchwick.Core.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Thatchwick.Game.Audio
{
    /// <summary>
    /// Tek giriş noktası: <c>AudioService.Instance.Play(SoundKeys.WorkChop, worldPos)</c>.
    /// Çeşitlilik kuralları (Project 45a bölüm 3): karıştırma torbası, perde/ses oynaması, zamanlama sapması,
    /// kişiye özgü perde, yoğunluk sınırı, uzaklıkla kısılma ve boğuklaşma. Katmanlı seslerde her katmandan bir parça.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        private const int PoolSize = 24;
        private static AudioService _instance;

        public static AudioService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AudioService");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AudioService>();
                }
                return _instance;
            }
        }

        public SoundBank Bank { get; private set; }
        public readonly VoiceLimiter Limiter = new VoiceLimiter();

        /// <summary>Ekranın yarı genişliği kadar uzaktaki ses tam duyulur; 2 katından uzak ses çalınmaz.</summary>
        public float HearingHalfWidth = 0f; // 0: kameranın yarı genişliği

        /// <summary>Hayvan ve köylü sesinde kişiye özgü perde aralığı (yarım ton).</summary>
        public float EntityPitchSemitones = 1.5f;

        private readonly List<AudioSource> _pool = new List<AudioSource>();
        private readonly List<AudioLowPassFilter> _lp = new List<AudioLowPassFilter>();
        private readonly Dictionary<string, ShuffleBag> _bags = new Dictionary<string, ShuffleBag>();
        private readonly float[] _groupVol = { 1, 1, 1, 1, 1 };
        private readonly Dictionary<string, PlayStats> _stats = new Dictionary<string, PlayStats>();
        private AudioRandom _rng;
        private AudioMixer _mixer;
        private int _next;

        /// <summary>Tekrar ölçümü (45a bölüm 4): anahtar başına çalma sayısı ve art arda aynı klip.</summary>
        public sealed class PlayStats
        {
            public int Plays;
            public int SameAsPrevious;
            public string LastClip;
            public readonly Dictionary<string, int> PerClip = new Dictionary<string, int>();
        }

        public IReadOnlyDictionary<string, PlayStats> Stats => _stats;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            _rng = new AudioRandom(System.Environment.TickCount);
            Bank = SoundBank.Load();
            _mixer = Resources.Load<AudioMixer>("Audio/ThatchwickMixer"); // isteğe bağlı
            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject("src" + i);
                go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f; // 2D: panlama ve kısmayı biz yaparız
                _pool.Add(s);
                var lp = go.AddComponent<AudioLowPassFilter>();
                lp.cutoffFrequency = 22000f;
                _lp.Add(lp);
            }
            for (int g = 0; g < _groupVol.Length; g++)
                _groupVol[g] = PlayerPrefs.GetFloat("audio.vol." + (AudioGroup)g, 1f);
        }

        // ------------------------------------------------------------ ayarlar

        public float GetVolume(AudioGroup g) => _groupVol[(int)g];

        public void SetVolume(AudioGroup g, float v)
        {
            _groupVol[(int)g] = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("audio.vol." + g, _groupVol[(int)g]);
        }

        public AudioMixerGroup MixerGroup(AudioGroup g)
        {
            if (_mixer == null) return null;
            var found = _mixer.FindMatchingGroups(g.ToString());
            return found != null && found.Length > 0 ? found[0] : null;
        }

        // ------------------------------------------------------------ çalma

        /// <summary>Dünyadaki bir olay sesi. entityId verilirse o varlığın sabit perdesi eklenir.</summary>
        public bool Play(string key, Vector3? worldPos = null, long entityId = 0, float volumeScale = 1f, float pitchScale = 1f)
        {
            if (Bank == null || !Bank.TryGet(key, out var e)) return false;
            Spatial(worldPos, out float pan, out float dist);
            if (dist > 1f) return false; // ekrandan çok uzak: sessiz
            return PlayEntry(e, pan, dist, entityId, volumeScale, pitchScale);
        }

        /// <summary>Yönü ve uzaklığı hazır verilen ses (ortam nokta sesleri). dist 0 yakın .. 1 uzak.</summary>
        public bool PlayAt(string key, float pan, float dist, float volumeScale = 1f)
        {
            if (Bank == null || !Bank.TryGet(key, out var e)) return false;
            return PlayEntry(e, pan, Mathf.Clamp01(dist), 0, volumeScale, 1f);
        }

        /// <summary>Hazır bir klibi grubuyla çalar (köy dili heceleri). Döndürür: süre (sn).</summary>
        public float PlayClip(AudioClip clip, AudioGroup g, float pitch, float volumeDb, float pan, float dist, float delay)
        {
            if (clip == null) return 0f;
            var s = Take();
            Setup(s, clip, g, pitch, volumeDb, pan, dist, false);
            if (delay > 0f) s.PlayDelayed(delay); else s.Play();
            return clip.length / Mathf.Max(0.05f, pitch);
        }

        private bool PlayEntry(SoundEntry e, float pan, float dist, long entityId, float volScale, float pitchScale)
        {
            var p = SoundVariation.Sample(_rng, e.pitchVar, e.volVarDb, e.timingJitterMs);
            float pitch = p.Pitch * pitchScale;
            if (entityId != 0 && (e.family == "animals" || e.family == "voice"))
                pitch *= SoundVariation.EntityPitch(entityId, EntityPitchSemitones);

            string first = e.IsLayered ? Pick(e.key + "#" + e.layers[0].name, e.layers[0].clips) : Pick(e.key, e.clips);
            var firstClip = Bank.Clip(first);
            if (firstClip == null) return false;
            if (!Limiter.TryStart(e.key, Time.unscaledTime, firstClip.length / Mathf.Max(0.05f, pitch), e.maxVoices, e.cooldownMs, out float duck))
                return false;

            float vol = e.volumeDb + p.VolumeDb + duck + 20f * (float)System.Math.Log10(Mathf.Max(0.0001f, volScale));
            float delay = p.DelayMs / 1000f;
            Fire(firstClip, e, pitch, vol, pan, dist, delay);
            Record(e.key, first);
            if (e.IsLayered)
            {
                for (int i = 1; i < e.layers.Length; i++)
                {
                    var L = e.layers[i];
                    // katmanlar küçük farkla oynar: aynı vuruşun parçaları gibi ama makine gibi değil
                    var lp = SoundVariation.Sample(_rng, e.pitchVar * 0.5f, e.volVarDb, 8f);
                    Fire(Bank.Clip(Pick(e.key + "#" + L.name, L.clips)), e, pitch * lp.Pitch, vol + lp.VolumeDb, pan, dist, delay + lp.DelayMs / 1000f);
                }
            }
            return true;
        }

        private void Fire(AudioClip clip, SoundEntry e, float pitch, float volDb, float pan, float dist, float delay)
        {
            if (clip == null) return;
            var s = Take();
            Setup(s, clip, e.Group, pitch, volDb, pan, dist, false);
            if (delay > 0.0005f) s.PlayDelayed(delay); else s.Play();
        }

        private void Setup(AudioSource s, AudioClip clip, AudioGroup g, float pitch, float volDb, float pan, float dist, bool loop)
        {
            s.clip = clip;
            s.loop = loop;
            s.pitch = pitch;
            s.panStereo = Mathf.Clamp(pan, -1f, 1f) * 0.8f;
            float distGain = Mathf.Lerp(1f, 0.25f, Mathf.InverseLerp(0.5f, 1f, dist));
            s.volume = Mathf.Clamp01(SoundVariation.DbToLinear(volDb) * distGain * _groupVol[(int)g]);
            s.outputAudioMixerGroup = MixerGroup(g);
            int i = _pool.IndexOf(s);
            if (i >= 0) _lp[i].cutoffFrequency = Mathf.Lerp(22000f, 2500f, Mathf.InverseLerp(0.45f, 1f, dist));
        }

        private string Pick(string bagKey, string[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            if (!_bags.TryGetValue(bagKey, out var bag) || bag.Count != clips.Length)
                _bags[bagKey] = bag = new ShuffleBag(clips.Length, _rng);
            return clips[bag.Next()];
        }

        private AudioSource Take()
        {
            // önce boşta olan; yoksa en eski (halka)
            for (int k = 0; k < _pool.Count; k++)
            {
                var s = _pool[(_next + k) % _pool.Count];
                if (!s.isPlaying) { _next = (_next + k + 1) % _pool.Count; return s; }
            }
            var o = _pool[_next];
            _next = (_next + 1) % _pool.Count;
            o.Stop();
            return o;
        }

        /// <summary>Kamera merkezine göre pan (-1..1) ve uzaklık (0 yakın, 1 duyma sınırı, &gt;1 duyulmaz).</summary>
        private void Spatial(Vector3? pos, out float pan, out float dist)
        {
            pan = 0f; dist = 0f;
            var cam = Camera.main;
            if (pos == null || cam == null) return;
            float half = HearingHalfWidth > 0 ? HearingHalfWidth : (cam.orthographic ? cam.orthographicSize * cam.aspect : 10f);
            Vector3 d = pos.Value - cam.transform.position;
            pan = Mathf.Clamp(d.x / half, -1f, 1f);
            float r = new Vector2(d.x, d.y).magnitude / half; // 1 = ekran kenarı
            dist = Mathf.Clamp01((r - 0.4f) / 1.6f) + (r > 2f ? 1f : 0f);
        }

        private void Record(string key, string clip)
        {
            if (!Debug.isDebugBuild) return;
            if (!_stats.TryGetValue(key, out var st)) _stats[key] = st = new PlayStats();
            st.Plays++;
            if (st.LastClip == clip) st.SameAsPrevious++;
            st.LastClip = clip;
            st.PerClip.TryGetValue(clip, out int n);
            st.PerClip[clip] = n + 1;
        }

        /// <summary>Geliştirici raporu: en çok çalan anahtarlar, art arda aynı klip sayısı, eksik anahtarlar.</summary>
        public string Report()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[Ses] tekrar raporu (anahtar: çalma, art arda aynı, klip sayısı)");
            foreach (var kv in _stats)
                sb.AppendLine($"  {kv.Key}: {kv.Value.Plays}, {kv.Value.SameAsPrevious}, {kv.Value.PerClip.Count}");
            if (Bank != null && Bank.MissingKeys.Count > 0)
                sb.AppendLine("  eksik: " + string.Join(", ", Bank.MissingKeys));
            return sb.ToString();
        }
    }
}
