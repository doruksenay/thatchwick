using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Audio
{
    public enum AmbWeather { Clear, Rain, Storm, Snow }

    /// <summary>Ortamın anlık bağlamı (DayLight / WeatherFx'ten doldurulur).</summary>
    public struct AmbienceContext
    {
        public float Hour;          // 0-24
        public int Season;          // 0 ilkbahar, 1 yaz, 2 sonbahar, 3 kış
        public AmbWeather Weather;
        public bool Indoor;
    }

    /// <summary>Nokta sesi tanımı (soundbank'te spotOf dolu girdiler).</summary>
    public sealed class SpotDef
    {
        public string Key;
        public string Bed;              // hangi yatakta çalar (boşsa hepsinde)
        public float IntervalMin = 8f;  // sn
        public float IntervalMax = 20f;
        public float[] Tod = { 1, 1, 1, 1 };     // şafak, gündüz, akşam, gece
        public float[] Season = { 1, 1, 1, 1 };  // ilkbahar, yaz, sonbahar, kış
        public float RainFactor = 0.3f;          // yağmurda seyrekleşir (kuşlar susar)
    }

    public struct SpotEvent
    {
        public string Key;
        public float Pan;       // -1 sol .. 1 sağ
        public float Distance;  // 0 yakın .. 1 uzak (uzaksa kısılır ve boğuklaşır)
    }

    /// <summary>
    /// Ortam (Project 45a bölüm 2.4): yatak tek döngü değil, parçalar karışık sırayla çapraz geçişle çalar;
    /// nokta sesleri rastgele aralıkta, rastgele yönden, saat/mevsim/havaya göre ağırlıklı gelir.
    /// Unity'den bağımsızdır; AmbienceDirector her karede Update çağırır.
    /// </summary>
    public sealed class AmbienceScheduler
    {
        private readonly AudioRandom _rng;
        private readonly List<SpotDef> _spots = new List<SpotDef>();
        private readonly Dictionary<SpotDef, float> _timers = new Dictionary<SpotDef, float>();
        private ShuffleBag _bedBag;
        private int _bedCount;

        public string Bed { get; private set; }
        public float Crossfade = 2.5f;

        public AmbienceScheduler(int seed) { _rng = new AudioRandom(seed); }

        public void AddSpot(SpotDef s)
        {
            _spots.Add(s);
            _timers[s] = _rng.Range(s.IntervalMin * 0.3f, s.IntervalMax);
        }

        /// <summary>Yatağı değiştirir; parça sayısı soundbank'teki klip sayısıdır.</summary>
        public void SetBed(string bed, int segmentCount)
        {
            if (bed == Bed && segmentCount == _bedCount) return;
            Bed = bed;
            _bedCount = segmentCount;
            _bedBag = segmentCount > 0 ? new ShuffleBag(segmentCount, _rng) : null;
        }

        /// <summary>Sıradaki yatak parçası (-1: yatak yok).</summary>
        public int NextSegment() => _bedBag?.Next() ?? -1;

        /// <summary>Saat → [şafak, gündüz, akşam, gece] ağırlıkları; sınırlarda 1 saatte yumuşak geçiş.</summary>
        public static float[] TodBlend(float hour)
        {
            hour = ((hour % 24f) + 24f) % 24f;
            // merkezler: şafak 6,5  gündüz 12,5  akşam 18,5  gece 0,5
            float[] centers = { 6.5f, 12.5f, 18.5f, 0.5f };
            float[] half = { 1.5f, 4.5f, 1.5f, 4.5f };
            var w = new float[4];
            float sum = 0;
            for (int i = 0; i < 4; i++)
            {
                float d = Math.Abs(hour - centers[i]);
                d = Math.Min(d, 24f - d);
                w[i] = Math.Max(0f, Math.Min(1f, (half[i] + 0.5f - d) / 1f));
                sum += w[i];
            }
            if (sum <= 0) { w[3] = 1; sum = 1; }
            for (int i = 0; i < 4; i++) w[i] /= sum;
            return w;
        }

        public static float Weight(SpotDef s, in AmbienceContext ctx)
        {
            var tod = TodBlend(ctx.Hour);
            float t = 0;
            for (int i = 0; i < 4; i++) t += tod[i] * s.Tod[i];
            float w = t * s.Season[Math.Max(0, Math.Min(3, ctx.Season))];
            if (ctx.Weather == AmbWeather.Rain || ctx.Weather == AmbWeather.Snow) w *= s.RainFactor;
            if (ctx.Weather == AmbWeather.Storm) w *= s.RainFactor * 0.3f;
            return w;
        }

        /// <summary>Zamanı ilerletir; bu karede çalınacak nokta seslerini döndürür.</summary>
        public List<SpotEvent> Update(float dt, in AmbienceContext ctx)
        {
            var fired = new List<SpotEvent>();
            foreach (var s in _spots)
            {
                if (!string.IsNullOrEmpty(s.Bed) && s.Bed != Bed) continue;
                _timers[s] -= dt;
                if (_timers[s] > 0) continue;
                float w = Weight(s, ctx);
                // ağırlık hem olasılık hem sıklık: düşük ağırlıkta seyrek gelir
                if (w > 0.01f && _rng.Value() < Math.Min(1f, w))
                {
                    float dist = ctx.Indoor ? _rng.Range(0.7f, 1f) : _rng.Range(0.15f, 1f);
                    fired.Add(new SpotEvent { Key = s.Key, Pan = _rng.Range(-0.9f, 0.9f), Distance = dist });
                }
                float mul = 1f / Math.Max(0.25f, Math.Min(1.5f, w));
                _timers[s] = _rng.Range(s.IntervalMin, s.IntervalMax) * mul;
            }
            return fired;
        }
    }
}
