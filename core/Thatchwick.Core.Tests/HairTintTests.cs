using Thatchwick.Core.Customization;

namespace Thatchwick.Core.Tests
{
    // Geçici saç bakımı (belge 59a §6, belge 65): kına, saç suları, saç yağı. Yalnız görünüş; yıkanınca gider.
    public class HairTintTests
    {
        private static Appearance Hair(string hex) => new Appearance { HairColor = Rgb.FromHex(hex) };

        [Fact]
        public void Kina_GorunusuDegistirir_DogalRengiDegistirmez()
        {
            var a = Hair("#8C8C8C");
            string before = a.Key();
            Assert.True(HairTints.Treat(a, HairTints.Henna));
            Assert.Equal(HairTints.Henna, a.HairTintId);
            Assert.Equal(Rgb.FromHex("#8C8C8C"), a.HairColor);          // genom ve çocuk doğal rengi görür
            Assert.NotEqual(a.HairColor, a.ShownHairColor());
            Assert.NotEqual(before, a.Key());                           // sprite önbelleği yenilenir
        }

        [Fact]
        public void Yikaninca_TonVeYagGider()
        {
            var a = Hair("#D9B45A");
            HairTints.Treat(a, HairTints.Purple);
            HairTints.Treat(a, HairTints.Shine);
            Assert.True(a.HairShine);
            Assert.Equal(HairTints.Purple, a.HairTintId);              // yağ tonu silmez
            Assert.True(a.WashHair());
            Assert.Null(a.HairTintId);
            Assert.False(a.HairShine);
            Assert.Equal(a.HairColor, a.ShownHairColor());
            Assert.False(a.WashHair());                                 // silinecek bir şey yok
        }

        [Fact]
        public void YeniTon_EskisininYerineGecer()
        {
            var a = Hair("#5A3A22");
            HairTints.Treat(a, HairTints.Henna);
            HairTints.Treat(a, HairTints.Walnut);
            Assert.Equal(HairTints.Walnut, a.HairTintId);
        }

        [Fact]
        public void BilinmeyenBakim_Reddedilir()
        {
            var a = Hair("#5A3A22");
            Assert.False(HairTints.Treat(a, "blue"));
            Assert.False(HairTints.Treat(null, HairTints.Henna));
            Assert.True(HairTints.IsKnown(HairTints.Shine));
            Assert.False(HairTints.IsTint(HairTints.Shine));
        }

        [Fact]
        public void MorSacSuyu_AcikSactaMor_KoyuSactaKoyuKalir()
        {
            var light = HairTints.Apply(Rgb.FromHex("#D9B45A"), HairTints.Purple, false);
            Assert.True(light.B > light.G, $"açık saç mora dönmeli: {light}");
            var dark = HairTints.Apply(Rgb.FromHex("#1C1A19"), HairTints.Purple, false);
            Assert.True(dark.R < 90 && dark.G < 90 && dark.B < 90, $"kara saç koyu kalmalı: {dark}");
        }

        [Fact]
        public void CevizSuyu_KirSaciKoyulastirir()
        {
            var grey = Rgb.FromHex("#8C8C8C");
            var dyed = HairTints.Apply(grey, HairTints.Walnut, false);
            Assert.True(dyed.R + dyed.G + dyed.B < grey.R + grey.G + grey.B);
        }

        [Fact]
        public void Kirlasma_TondanOnceUygulanir()
        {
            var a = Hair("#1C1A19");
            HairTints.Treat(a, HairTints.Henna);
            var young = a.ShownHairColor(0.0);
            var old = a.ShownHairColor(1.0);
            Assert.True(old.R > young.R);                               // ak saçta kına daha parlak tutar
        }

        [Fact]
        public void Klon_TonuTasir()
        {
            var a = Hair("#5A3A22");
            HairTints.Treat(a, HairTints.Chamomile);
            var c = a.Clone();
            Assert.Equal(HairTints.Chamomile, c.HairTintId);
            c.WashHair();
            Assert.Equal(HairTints.Chamomile, a.HairTintId);
        }
    }
}
