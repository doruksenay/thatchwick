using System;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Life
{
    public enum WeatherType { Clear = 0, Rain = 1, Snow = 2, Storm = 3 }

    /// <summary>
    /// Hava durumu (Adım 33, T19): her gün tohum + gün numarasından belirlenir; bu yüzden tahmin (yarın) ertesi
    /// günün gerçek havasıyla birebir tutar ve kayıtta yalnızca tohum saklanır. Dünkü hava bugünü biraz etkiler
    /// (yağmur ve fırtına ardı ardına gelebilir). Etkiler: dışarıdaki iş hızı, dış sıcaklık, ekin büyümesi.
    /// </summary>
    public sealed class WeatherSystem
    {
        private readonly TimeConfig _time;

        public int Seed { get; set; }

        // Mevsim başına (açık, yağmur, kar, fırtına) olasılıkları; toplam 1.
        private static readonly double[][] Table =
        {
            new[] { 0.55, 0.35, 0.00, 0.10 },   // ilkbahar
            new[] { 0.65, 0.25, 0.00, 0.10 },   // yaz
            new[] { 0.45, 0.40, 0.00, 0.15 },   // sonbahar
            new[] { 0.40, 0.00, 0.45, 0.15 },   // kış
        };

        public WeatherSystem(TimeConfig time, int seed = 1)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            Seed = seed;
        }

        private int SeasonIndex(long day) => (int)(((day < 0 ? 0 : day) / _time.DaysPerSeason) % TimeConfig.SeasonsPerYear);

        private double Roll(long day)
        {
            unchecked
            {
                int h = Seed * 73856093 ^ (int)day * 19349663 ^ 0x5bd1e995;
                return new Random(h).NextDouble();
            }
        }

        private WeatherType Pick(double[] p, double u)
        {
            double acc = 0;
            for (int i = 0; i < p.Length; i++) { acc += p[i]; if (u < acc) return (WeatherType)i; }
            return WeatherType.Clear;
        }

        // Bugün ve yarın her karede sorulur; her sorguda yeni Random üretmemek için son iki gün önbellekte tutulur.
        private readonly long[] _cachedDay = { long.MinValue, long.MinValue };
        private readonly WeatherType[] _cachedWeather = new WeatherType[2];
        private int _cachedSeed;
        private int _cacheNext;

        public WeatherType WeatherOn(long day)
        {
            if (_cachedSeed != Seed) { _cachedSeed = Seed; _cachedDay[0] = _cachedDay[1] = long.MinValue; }
            for (int i = 0; i < 2; i++) if (_cachedDay[i] == day) return _cachedWeather[i];
            var w = Compute(day);
            _cachedDay[_cacheNext] = day;
            _cachedWeather[_cacheNext] = w;
            _cacheNext = 1 - _cacheNext;
            return w;
        }

        private WeatherType Compute(long day)
        {
            var p = (double[])Table[SeasonIndex(day)].Clone();
            if (day > 0)
            {
                // Süreklilik: dünkü (kendi tablosundan) hava yağmur/kar/fırtınaysa tekrarı biraz artar.
                var yesterday = Pick(Table[SeasonIndex(day - 1)], Roll(day - 1));
                if (yesterday != WeatherType.Clear && p[(int)yesterday] > 0)
                {
                    double boost = 0.15;
                    p[(int)yesterday] += boost;
                    p[(int)WeatherType.Clear] = Math.Max(0, p[(int)WeatherType.Clear] - boost);
                }
            }
            return Pick(p, Roll(day));
        }

        public WeatherType Today(double totalDays) => WeatherOn((long)Math.Floor(totalDays));
        public WeatherType Tomorrow(double totalDays) => WeatherOn((long)Math.Floor(totalDays) + 1);

        /// <summary>Dışarıdaki iş hızı çarpanı (içeride 1).</summary>
        public static float OutdoorWorkSpeed(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.Rain: return 0.9f;
                case WeatherType.Snow: return 0.85f;
                case WeatherType.Storm: return 0.7f;
                default: return 1f;
            }
        }

        /// <summary>Dış sıcaklığa eklenen °C.</summary>
        public static float TemperatureOffset(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.Rain: return -1f;
                case WeatherType.Snow: return -3f;
                case WeatherType.Storm: return -5f;
                default: return 0f;
            }
        }

        /// <summary>Ekin büyüme çarpanı: yağmur sular (+%30), fırtına zarar verir (-%20).</summary>
        public static float GrowthMultiplier(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.Rain: return 1.3f;
                case WeatherType.Storm: return 0.8f;
                default: return 1f;
            }
        }

        public static string Name(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.Rain: return "Yağmurlu";
                case WeatherType.Snow: return "Karlı";
                case WeatherType.Storm: return "Fırtınalı";
                default: return "Açık";
            }
        }

        public static string Icon(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.Rain: return "rain";
                case WeatherType.Snow: return "snow";
                case WeatherType.Storm: return "storm";
                default: return "sun";
            }
        }

        /// <summary>"Yarın kar bekleniyor" gibi tahmin cümlesi.</summary>
        public string ForecastText(double totalDays)
        {
            switch (Tomorrow(totalDays))
            {
                case WeatherType.Rain: return "Yarın yağmur bekleniyor.";
                case WeatherType.Snow: return "Yarın kar bekleniyor.";
                case WeatherType.Storm: return "Yarın fırtına bekleniyor; dışarıdaki işler yavaşlar.";
                default: return "Yarın hava açık.";
            }
        }
    }
}
