using System;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Tests
{
    public class GameClockTests
    {
        [Fact]
        public void BirOyunGunu_1xHizda_AltiGercekDakikadir()
        {
            var clock = new GameClock();
            clock.Advance(360);
            Assert.Equal(1.0, clock.TotalDays, 6);
            Assert.Equal(1, clock.DayIndex);
        }

        [Theory] // T2: mevsim uzunluğu 8 / 10 / 12
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(12)]
        public void MevsimUzunlugu_AyarlanabilirVeYilDortMevsimdir(int daysPerSeason)
        {
            var config = new TimeConfig(daysPerSeason: daysPerSeason);
            Assert.Equal(daysPerSeason * 4, config.DaysPerYear);

            var clock = new GameClock(config);
            Assert.Equal(Season.Spring, clock.Season);

            clock.Advance(config.RealSecondsPerGameDay * daysPerSeason);
            Assert.Equal(Season.Summer, clock.Season);
            Assert.Equal(0, clock.DayOfSeason);

            clock.Advance(config.RealSecondsPerGameDay * daysPerSeason * 2);
            Assert.Equal(Season.Winter, clock.Season);

            clock.Advance(config.RealSecondsPerGameDay * daysPerSeason);
            Assert.Equal(Season.Spring, clock.Season);
            Assert.Equal(1, clock.YearIndex);
        }

        [Theory] // T3: hız yalnızca gerçek saniyeyi oyun gününe çevirir
        [InlineData(1, 1.0)]
        [InlineData(2, 2.0)]
        [InlineData(3, 3.0)]
        public void Hiz_OyunGunuIlerlemesiniCarpar(int speed, double expectedDays)
        {
            var clock = new GameClock();
            clock.SetSpeed(speed);
            clock.Advance(360);
            Assert.Equal(expectedDays, clock.TotalDays, 6);
        }

        [Fact]
        public void GecersizHiz_Reddedilir()
        {
            var clock = new GameClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetSpeed(4));
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetSpeed(0));
        }

        [Fact]
        public void KritikAn_HiziOtomatik1xeDusurur_BitinceEskiHizaDoner()
        {
            var clock = new GameClock();
            clock.SetSpeed(3);

            clock.EnterCriticalMoment();
            Assert.Equal(1, clock.EffectiveSpeed);
            Assert.Equal(3, clock.RequestedSpeed);
            clock.Advance(360);
            Assert.Equal(1.0, clock.TotalDays, 6);

            clock.ExitCriticalMoment();
            Assert.Equal(3, clock.EffectiveSpeed);
        }

        [Fact]
        public void KritikAn_IcIceAcilabilir()
        {
            var clock = new GameClock();
            clock.SetSpeed(2);
            clock.EnterCriticalMoment();
            clock.EnterCriticalMoment();
            clock.ExitCriticalMoment();
            Assert.True(clock.IsCriticalMoment);
            clock.ExitCriticalMoment();
            Assert.False(clock.IsCriticalMoment);
            Assert.Equal(2, clock.EffectiveSpeed);
        }

        [Fact]
        public void FazlaCikis_HataFirlatir()
        {
            var clock = new GameClock();
            Assert.Throws<InvalidOperationException>(() => clock.ExitCriticalMoment());
        }

        [Fact]
        public void ParcaliIlerleme_TekAdimlaAyniSonucuVerir()
        {
            var a = new GameClock();
            var b = new GameClock();
            a.SetSpeed(2);
            b.SetSpeed(2);

            a.Advance(1000);
            for (int i = 0; i < 1000; i++) b.Advance(1);

            Assert.Equal(a.TotalDays, b.TotalDays, 6);
        }
    }
}
