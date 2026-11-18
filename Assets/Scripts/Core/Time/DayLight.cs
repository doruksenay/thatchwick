using System;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Time
{
    /// <summary>Ortam ışığı rengi (0-1 çarpan). Oyun katmanı sahneyi bununla çarpar.</summary>
    public readonly struct LightTint
    {
        public readonly float R, G, B;
        public LightTint(float r, float g, float b) { R = r; G = g; B = b; }

        /// <summary>Algılanan parlaklık (Rec. 601 ağırlıkları).</summary>
        public float Luminance => 0.299f * R + 0.587f * G + 0.114f * B;

        public LightTint Times(float r, float g, float b) =>
            new LightTint(Clamp01(R * r), Clamp01(G * g), Clamp01(B * b));

        public static LightTint Lerp(LightTint a, LightTint b, float t) =>
            new LightTint(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);

        internal static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }

    /// <summary>
    /// Gün ışığı (görsel analiz, 18.11.2026): saate, mevsime ve havaya göre ortam rengi. Gece mavi ve koyu,
    /// şafak ve akşam sıcak, öğle nötr. Kışın günler kısa ve ışık soğuk; yağmur ve fırtına sahneyi karartır.
    /// UnityEngine'e bağımlı değildir; test edilebilir.
    /// </summary>
    public static class DayLight
    {
        private static readonly (double hour, LightTint tint)[] Keys =
        {
            (0.0,  new LightTint(0.30f, 0.34f, 0.54f)),
            (4.5,  new LightTint(0.32f, 0.36f, 0.56f)),
            (6.0,  new LightTint(0.82f, 0.66f, 0.62f)),   // şafak
            (8.0,  new LightTint(1.00f, 0.95f, 0.88f)),
            (12.0, new LightTint(1.00f, 1.00f, 1.00f)),   // öğle
            (16.5, new LightTint(1.00f, 0.96f, 0.90f)),
            (18.5, new LightTint(0.98f, 0.74f, 0.56f)),   // akşam
            (20.0, new LightTint(0.52f, 0.48f, 0.68f)),   // alacakaranlık
            (21.5, new LightTint(0.32f, 0.36f, 0.56f)),
            (24.0, new LightTint(0.30f, 0.34f, 0.54f)),
        };

        /// <summary>Kışın gün kısalır: şafak geç, akşam erken (saat ekseni öğleye doğru sıkışır).</summary>
        public static double SeasonalHour(double hour, Season season)
        {
            double squeeze = season == Season.Winter ? 0.82 : season == Season.Summer ? 1.08 : 1.0;
            double h = 12.0 + (hour - 12.0) / squeeze;
            return h < 0.0 ? 0.0 : (h > 24.0 ? 24.0 : h);
        }

        /// <summary>Yalnızca saate bağlı renk (mevsim ve hava yok).</summary>
        public static LightTint ForHour(double hour)
        {
            hour = ((hour % 24.0) + 24.0) % 24.0;
            for (int i = 1; i < Keys.Length; i++)
            {
                if (hour > Keys[i].hour) continue;
                var a = Keys[i - 1];
                var b = Keys[i];
                float t = (float)((hour - a.hour) / (b.hour - a.hour));
                t = t * t * (3f - 2f * t);   // yumuşak geçiş
                return LightTint.Lerp(a.tint, b.tint, t);
            }
            return Keys[Keys.Length - 1].tint;
        }

        /// <summary>Saat, mevsim ve hava ile ortam rengi.</summary>
        public static LightTint Ambient(double hour, Season season, WeatherType weather)
        {
            var c = ForHour(SeasonalHour(hour, season));
            switch (season)
            {
                case Season.Spring: c = c.Times(0.98f, 1.00f, 0.97f); break;
                case Season.Summer: c = c.Times(1.00f, 0.99f, 0.95f); break;
                case Season.Autumn: c = c.Times(1.00f, 0.95f, 0.86f); break;
                case Season.Winter: c = c.Times(0.92f, 0.96f, 1.00f); break;
            }
            switch (weather)
            {
                case WeatherType.Rain: c = c.Times(0.78f, 0.80f, 0.86f); break;
                case WeatherType.Storm: c = c.Times(0.62f, 0.64f, 0.74f); break;
                case WeatherType.Snow: c = c.Times(0.90f, 0.93f, 1.00f); break;
            }
            return c;
        }

        /// <summary>Gece ölçüsü: 0 aydınlık, 1 tam karanlık. Ocak ve mum ışığının görünürlüğü bununla ölçeklenir.</summary>
        public static float NightFactor(LightTint ambient) => LightTint.Clamp01((0.92f - ambient.Luminance) / 0.52f);
    }
}
