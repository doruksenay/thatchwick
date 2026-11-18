using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Gün ışığı: saat, mevsim ve hava ile ortam rengi (görsel analiz, 18.11.2026).</summary>
    public class GunIsigiTests
    {
        [Fact]
        public void Ogle_notr_gece_koyu_ve_mavi()
        {
            var noon = DayLight.ForHour(12.0);
            Assert.Equal(1f, noon.R, 3);
            Assert.Equal(1f, noon.G, 3);
            Assert.Equal(1f, noon.B, 3);

            var night = DayLight.ForHour(2.0);
            Assert.True(night.Luminance < 0.45f);
            Assert.True(night.B > night.R, "gece mavi tonlu olmalı");
        }

        [Fact]
        public void Aksam_sicak_tonlu()
        {
            var dusk = DayLight.ForHour(18.5);
            Assert.True(dusk.R > dusk.B + 0.3f);
        }

        [Fact]
        public void Gece_yarisi_iki_ucta_ayni()
        {
            var a = DayLight.ForHour(0.0);
            var b = DayLight.ForHour(24.0);
            Assert.Equal(a.R, b.R, 3);
            Assert.Equal(a.B, b.B, 3);
            var c = DayLight.ForHour(-1.0);
            var d = DayLight.ForHour(23.0);
            Assert.Equal(c.G, d.G, 3);
        }

        [Fact]
        public void Kisin_aksam_erken_kararir()
        {
            var summer = DayLight.Ambient(19.0, Season.Summer, WeatherType.Clear);
            var winter = DayLight.Ambient(19.0, Season.Winter, WeatherType.Clear);
            Assert.True(winter.Luminance < summer.Luminance - 0.1f);
            Assert.Equal(12.0, DayLight.SeasonalHour(12.0, Season.Winter), 6);
        }

        [Fact]
        public void Firtina_yagmurdan_karanlik()
        {
            var clear = DayLight.Ambient(12.0, Season.Spring, WeatherType.Clear);
            var rain = DayLight.Ambient(12.0, Season.Spring, WeatherType.Rain);
            var storm = DayLight.Ambient(12.0, Season.Spring, WeatherType.Storm);
            Assert.True(rain.Luminance < clear.Luminance);
            Assert.True(storm.Luminance < rain.Luminance);
        }

        [Fact]
        public void Gece_olcusu_gunduz_sifir_gece_bire_yakin()
        {
            Assert.Equal(0f, DayLight.NightFactor(DayLight.Ambient(12.0, Season.Summer, WeatherType.Clear)));
            Assert.True(DayLight.NightFactor(DayLight.Ambient(2.0, Season.Summer, WeatherType.Clear)) > 0.9f);
        }

        [Fact]
        public void Renkler_hep_sifir_bir_araliginda()
        {
            foreach (Season s in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
                foreach (WeatherType w in new[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Snow, WeatherType.Storm })
                    for (double h = 0; h <= 24; h += 0.25)
                    {
                        var c = DayLight.Ambient(h, s, w);
                        Assert.InRange(c.R, 0f, 1f);
                        Assert.InRange(c.G, 0f, 1f);
                        Assert.InRange(c.B, 0f, 1f);
                    }
        }
    }
}
