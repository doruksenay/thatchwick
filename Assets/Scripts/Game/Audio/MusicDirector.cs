using Thatchwick.Core.Audio;
using UnityEngine;

namespace Thatchwick.Game.Audio
{
    /// <summary>
    /// Müzik (Project 45a bölüm 2.6 ve 3): bağlama göre havuz ("spring.day", "town", "tavern"...), son iki parça
    /// tekrar çalmaz, parçalar arasına rastgele sessizlik girer, bağlam değişince yumuşak geçiş yapılır.
    /// Olay vuruşu (stinger) çalarken müzik kısılır.
    /// </summary>
    public sealed class MusicDirector : MonoBehaviour
    {
        private static MusicDirector _instance;

        public static MusicDirector Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("MusicDirector");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<MusicDirector>();
                }
                return _instance;
            }
        }

        public float FadeSeconds = 3f;
        public float DuckDb = -12f;
        /// <summary>Oyun ilk açıldığında ilk parçadan önceki sessizlik (sn).</summary>
        public float FirstSilence = 4f;

        private MusicPlaylist _list;
        private readonly AudioRandom _rng = new AudioRandom(System.Environment.TickCount);
        private AudioService _svc;
        private AudioSource _src;
        private string _context, _currentKey;
        private float _level, _target, _silenceLeft, _duckLeft, _duck = 1f;
        private bool _fadingOut;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            _svc = AudioService.Instance;
            _list = new MusicPlaylist(System.Environment.TickCount);
            foreach (var e in _svc.Bank.Entries)
                if (e.Group == AudioGroup.Music && e.family == "music")
                    _list.Add(string.IsNullOrEmpty(e.context) ? "default" : e.context, e.key);
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f;
            _src.outputAudioMixerGroup = _svc.MixerGroup(AudioGroup.Music);
            _silenceLeft = FirstSilence;
        }

        /// <summary>Bağlam: "&lt;mevsim&gt;.&lt;gündüz|gece&gt;", "town", "tavern", "mine"... Havuzda yoksa üst bağlama düşer.</summary>
        public void SetContext(string context)
        {
            if (context == _context) return;
            _context = context;
            // çalan parça yeni bağlamın havuzunda yoksa sönerek bitir, kısa sessizlikten sonra yenisi
            if (_currentKey != null && !Contains(_list.PoolFor(context), _currentKey))
            {
                _fadingOut = true;
                _target = 0f;
                _silenceLeft = _rng.Range(3f, 8f);
            }
        }

        /// <summary>Olay vuruşu: müziği kısar, vuruşu çalar.</summary>
        public void Stinger(string key)
        {
            if (!_svc.Bank.TryGet(key, out var e) || e.clips.Length == 0) return;
            var clip = _svc.Bank.Clip(e.clips[0]);
            if (clip == null) return;
            _svc.Play(key);
            _duckLeft = clip.length + 0.5f;
        }

        private static bool Contains(System.Collections.Generic.IReadOnlyList<string> l, string k)
        {
            for (int i = 0; i < l.Count; i++) if (l[i] == k) return true;
            return false;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _level = Mathf.MoveTowards(_level, _target, dt / FadeSeconds);
            _duckLeft -= dt;
            _duck = Mathf.MoveTowards(_duck, _duckLeft > 0 ? SoundVariation.DbToLinear(DuckDb) : 1f, dt * 3f);
            _src.volume = _level * _duck * _svc.GetVolume(AudioGroup.Music);

            if (_fadingOut && _level <= 0f)
            {
                _src.Stop();
                _fadingOut = false;
                _currentKey = null;
            }
            if (_src.isPlaying || _fadingOut) return;
            if (_currentKey != null)
            {
                // parça bitti: sessizlik molası
                _currentKey = null;
                _silenceLeft = _list.NextSilence();
            }
            _silenceLeft -= dt;
            if (_silenceLeft > 0 || _context == null) return;
            string key = _list.Next(_context);
            if (key == null || !_svc.Bank.TryGet(key, out var e) || e.clips.Length == 0) { _silenceLeft = 10f; return; }
            var clip = _svc.Bank.Clip(e.clips[_rng.Range(0, e.clips.Length)]);
            if (clip == null) { _silenceLeft = 10f; return; }
            _src.clip = clip;
            _src.loop = false;
            _src.Play();
            _currentKey = key;
            _level = 0f;
            _target = 1f;
        }
    }
}
