using System.Collections.Generic;
using Thatchwick.Core.Audio;
using UnityEngine;

namespace Thatchwick.Game.Audio
{
    /// <summary>
    /// Ortam sesi (Project 45a bölüm 2.4): yatak parçaları karışık sırayla çapraz geçişle; üstüne hava katmanı
    /// (yağmur, fırtına) ve nokta sesleri. Oyun her karede ya da değişince <see cref="SetBed"/>, <see cref="SetWeatherBed"/>
    /// ve <see cref="SetContext"/> çağırır (DayLight, WeatherFx, bölge/iç mekân).
    /// </summary>
    public sealed class AmbienceDirector : MonoBehaviour
    {
        private static AmbienceDirector _instance;

        public static AmbienceDirector Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AmbienceDirector");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AmbienceDirector>();
                }
                return _instance;
            }
        }

        public float Crossfade = 2.5f;
        public float IndoorBedDb = -9f;   // iç mekânda dış yatak kısılır ve boğuklaşır

        private AmbienceScheduler _sched;
        private AmbienceContext _ctx;
        private BedChannel _bed, _weather;
        private AudioService _svc;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            _svc = AudioService.Instance;
            _sched = new AmbienceScheduler(System.Environment.TickCount);
            _bed = new BedChannel(gameObject, "bed", new AudioRandom(System.Environment.TickCount ^ 0x51));
            _weather = new BedChannel(gameObject, "weather", new AudioRandom(System.Environment.TickCount ^ 0x77));
            foreach (var e in _svc.Bank.Entries)
            {
                if (string.IsNullOrEmpty(e.spotOf)) continue;
                _sched.AddSpot(new SpotDef
                {
                    Key = e.key,
                    Bed = e.spotOf,
                    IntervalMin = e.spotIntervalMin > 0 ? e.spotIntervalMin : 8f,
                    IntervalMax = e.spotIntervalMax > 0 ? e.spotIntervalMax : 20f,
                    Tod = e.todWeights != null && e.todWeights.Length == 4 ? e.todWeights : new float[] { 1, 1, 1, 1 },
                    Season = e.seasonWeights != null && e.seasonWeights.Length == 4 ? e.seasonWeights : new float[] { 1, 1, 1, 1 },
                });
            }
        }

        /// <summary>Ana yatak: ör. SoundKeys.AmbFarmDay. null: sessiz.</summary>
        public void SetBed(string key)
        {
            int n = 0;
            SoundEntry e = null;
            if (key != null && _svc.Bank.TryGet(key, out e)) n = e.clips.Length;
            _sched.SetBed(key, n);
            _bed.Switch(e, Crossfade, () => _sched.NextSegment());
        }

        /// <summary>Hava katmanı: SoundKeys.WeatherRain, WeatherStorm ya da null.</summary>
        public void SetWeatherBed(string key)
        {
            SoundEntry e = null;
            if (key != null) _svc.Bank.TryGet(key, out e);
            _weather.Switch(e, Crossfade * 1.5f, null);
        }

        public void SetContext(AmbienceContext ctx) { _ctx = ctx; }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float bedDb = _ctx.Indoor ? IndoorBedDb : 0f;
            float cutoff = _ctx.Indoor ? 1800f : 22000f;
            float g = _svc.GetVolume(AudioGroup.Ambience);
            _bed.Tick(dt, Crossfade, () => _sched.NextSegment(), g * SoundVariation.DbToLinear(bedDb), cutoff);
            _weather.Tick(dt, Crossfade * 1.5f, null, g * SoundVariation.DbToLinear(bedDb * 0.5f), cutoff);
            foreach (var ev in _sched.Update(dt, _ctx))
                _svc.PlayAt(ev.Key, ev.Pan, _ctx.Indoor ? Mathf.Max(0.8f, ev.Distance) : ev.Distance);
        }

        /// <summary>İki kaynaklı yatak kanalı: parça biterken bir sonrakine çapraz geçiş.</summary>
        private sealed class BedChannel
        {
            private readonly AudioSource[] _src = new AudioSource[2];
            private readonly AudioLowPassFilter[] _lp = new AudioLowPassFilter[2];
            private readonly float[] _target = new float[2];
            private readonly float[] _level = new float[2];
            private readonly AudioRandom _rng;
            private ShuffleBag _ownBag;
            private SoundEntry _entry;
            private int _cur;

            public BedChannel(GameObject host, string name, AudioRandom rng)
            {
                _rng = rng;
                for (int i = 0; i < 2; i++)
                {
                    var go = new GameObject(name + i);
                    go.transform.SetParent(host.transform, false);
                    _src[i] = go.AddComponent<AudioSource>();
                    _src[i].playOnAwake = false;
                    _src[i].spatialBlend = 0f;
                    _src[i].outputAudioMixerGroup = AudioService.Instance.MixerGroup(AudioGroup.Ambience);
                    _lp[i] = go.AddComponent<AudioLowPassFilter>();
                }
            }

            public void Switch(SoundEntry e, float fade, System.Func<int> next)
            {
                if (e == _entry) return;
                _entry = e;
                _ownBag = e != null && e.clips.Length > 0 ? new ShuffleBag(e.clips.Length, _rng) : null;
                _target[_cur] = 0f; // eski yatak söner
                if (e != null) StartNext(next, fade);
            }

            private void StartNext(System.Func<int> next, float fade)
            {
                if (_entry == null || _entry.clips.Length == 0) return;
                int idx = next != null ? next() : _ownBag.Next();
                if (idx < 0) return;
                var clip = AudioService.Instance.Bank.Clip(_entry.clips[idx]);
                if (clip == null) return;
                _target[_cur] = 0f;
                _cur = 1 - _cur;
                var s = _src[_cur];
                s.clip = clip;
                s.loop = _entry.loop;
                s.pitch = 1f;
                s.time = _entry.loop ? _rng.Range(0f, clip.length * 0.9f) : 0f; // aynı döngü iki yerde aynı anda başlamasın
                _level[_cur] = 0f;
                _target[_cur] = 1f;
                s.volume = 0f;
                s.Play();
            }

            public void Tick(float dt, float fade, System.Func<int> next, float gain, float cutoff)
            {
                var cur = _src[_cur];
                if (_entry != null && !_entry.loop && cur.clip != null && cur.isPlaying &&
                    cur.clip.length - cur.time <= fade && _target[_cur] > 0f)
                    StartNext(next, fade);
                float step = fade > 0 ? dt / fade : 1f;
                for (int i = 0; i < 2; i++)
                {
                    _level[i] = Mathf.MoveTowards(_level[i], _target[i], step);
                    // eşit güç eğrisi
                    _src[i].volume = Mathf.Sin(_level[i] * Mathf.PI * 0.5f) * gain;
                    _lp[i].cutoffFrequency = Mathf.Lerp(_lp[i].cutoffFrequency, cutoff, dt * 2f);
                    if (_level[i] <= 0f && _target[i] <= 0f && _src[i].isPlaying) _src[i].Stop();
                }
            }
        }
    }
}
