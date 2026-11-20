using Thatchwick.Core.Time;
using Xunit;

namespace Thatchwick.Core.Tests
{
    public class FixedStepperTests
    {
        [Fact]
        public void KareHizindanBagimsiz_AyniSureAyniAdim()
        {
            var a = new FixedStepper(0.1);
            var b = new FixedStepper(0.1);
            int sa = 0, sb = 0;
            for (int i = 0; i < 30; i++) sa += a.Accumulate(1.0 / 30);   // mobil: 30 FPS, 1 saniye
            for (int i = 0; i < 60; i++) sb += b.Accumulate(1.0 / 60);   // masaüstü: 60 FPS, 1 saniye
            Assert.InRange(sa, 9, 10);
            Assert.InRange(sb, 9, 10);
            Assert.True(System.Math.Abs(sa - sb) <= 1);
        }

        [Fact]
        public void KucukKarelerBirikir_AlphaIlerler()
        {
            var s = new FixedStepper(0.1);
            Assert.Equal(0, s.Accumulate(0.04));
            Assert.InRange(s.Alpha, 0.39f, 0.41f);
            Assert.Equal(0, s.Accumulate(0.04));
            Assert.Equal(1, s.Accumulate(0.04));
            Assert.InRange(s.Alpha, 0.19f, 0.21f);
        }

        [Fact]
        public void TakilanKare_SinirdaKesilir_FazlaSureAtilir()
        {
            var s = new FixedStepper(0.1, 4);
            Assert.Equal(4, s.Accumulate(5.0));
            Assert.Equal(0f, s.Alpha);
            Assert.Equal(0, s.Accumulate(0.05));
        }

        [Fact]
        public void GecersizSure_YokSayilir()
        {
            var s = new FixedStepper(0.1);
            Assert.Equal(0, s.Accumulate(-1));
            Assert.Equal(0, s.Accumulate(double.NaN));
            Assert.Equal(0f, s.Alpha);
        }
    }
}
