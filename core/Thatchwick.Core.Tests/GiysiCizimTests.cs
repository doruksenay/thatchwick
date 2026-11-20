using System.Collections.Generic;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Customization.Layers;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Parti 6 (belge 48 §4.1, §13.1; 0 gen): kesim maskeleri, karın betiği, çapalar, doku, kir, yama, ikon kırpma.</summary>
    public class GiysiCizimTests
    {
        // 20x40 yapay manken: gövde x 7-12, kollar x 4-6 ve 13-15 (satır 8-22), bacaklar 20-38, ayak 38-39.
        private static RegionFrame Mannequin()
        {
            var f = new RegionFrame(20, 40);
            void Set(int x, int y, byte r) { f.Region[y * 20 + x] = r; f.Shade[y * 20 + x] = 2; }
            for (int y = 0; y < 6; y++) for (int x = 8; x <= 11; x++) Set(x, y, Region.Skin);
            for (int y = 6; y < 20; y++) for (int x = 7; x <= 12; x++) Set(x, y, Region.Main);
            for (int y = 8; y < 22; y++) { for (int x = 4; x <= 6; x++) Set(x, y, Region.Main); for (int x = 13; x <= 15; x++) Set(x, y, Region.Main); }
            for (int y = 20; y < 38; y++) for (int x = 7; x <= 12; x++) Set(x, y, Region.Bottom);
            for (int y = 38; y < 40; y++) for (int x = 7; x <= 12; x++) Set(x, y, Region.Shoes);
            for (int y = 6; y < 38; y++) Set(16, y, Region.Outline);
            return f;
        }

        [Fact]
        public void EklemCizgileri_VeGovdeAcikligi()
        {
            var j = JointLines.Estimate(Mannequin());
            Assert.Equal(0, j.Top);
            Assert.Equal(39, j.Bottom);
            Assert.True(j.Shoulder < j.Elbow && j.Elbow < j.Wrist && j.Waist < j.Knee && j.Knee < j.Ankle);
            Assert.Equal(7, j.TorsoLeft);
            Assert.Equal(12, j.TorsoRight);
        }

        [Fact]
        public void Kesim_KisaKolTenOlur_GovdeDegismez()
        {
            var f = Mannequin();
            var j = JointLines.Estimate(f);
            var cut = ClothingMasks.ApplyCuts(f, j, new CutSpec { Sleeve = "short" });
            Assert.Equal(Region.Main, cut.RegionAt(5, 9));          // omuz ucu kumaş
            Assert.Equal(Region.Skin, cut.RegionAt(5, 20));         // ön kol ten
            Assert.Equal(Region.Main, cut.RegionAt(9, 15));         // gövde kumaş
            Assert.Equal(Region.Main, f.RegionAt(5, 20));           // manken değişmez
            var under = ClothingMasks.ApplyCuts(f, j, new CutSpec { Sleeve = "none", ShirtUnder = true });
            Assert.Equal(Region.Under, under.RegionAt(5, 12));      // kolsuz üst: kollar gömleğin rengi
        }

        [Fact]
        public void Kesim_BoyCizmeOnluk()
        {
            var f = Mannequin();
            var j = JointLines.Estimate(f);
            var knee = ClothingMasks.ApplyCuts(f, j, new CutSpec { Length = "knee_above" });
            Assert.Equal(Region.Main, knee.RegionAt(9, j.Knee - 3));   // tunik dize kadar
            Assert.Equal(Region.Bottom, knee.RegionAt(9, j.Knee + 1));
            var boots = ClothingMasks.ApplyCuts(f, j, new CutSpec { Boots = "tall" });
            Assert.Equal(Region.Shoes, boots.RegionAt(9, j.Knee + 1));
            Assert.Equal(Region.Bottom, boots.RegionAt(9, j.Knee - 2));
            var apron = ClothingMasks.ApplyCuts(f, j, new CutSpec { Apron = true });
            Assert.Equal(Region.Apron, apron.RegionAt(9, 15));
            Assert.Equal(Region.Main, apron.RegionAt(5, 15));   // kol önlüksüz
        }

        [Fact]
        public void Gebelik_KarinBetigi_KonturuKaydirir()
        {
            var f = Mannequin();
            var j = JointLines.Estimate(f);
            var cut = ClothingMasks.ApplyCuts(f, j, new CutSpec { BellyPx = 2 });
            Assert.Equal(Region.Main, cut.RegionAt(16, j.Waist));
            Assert.Equal(Region.Outline, cut.RegionAt(18, j.Waist));
            Assert.Equal(Region.Outline, cut.RegionAt(16, j.Shoulder));   // göğüs hizası değişmez
        }

        [Fact]
        public void Capalar_OmuzBelElGozluk()
        {
            var f = Mannequin();
            var j = JointLines.Estimate(f);
            Assert.Equal((9, j.Shoulder), ClothingMasks.Shoulder(j));
            Assert.Equal((9, j.Waist), ClothingMasks.Waist(j));
            var a = new FrameAnchor();
            Assert.Null(ClothingMasks.Glasses(a));
            a.Eyes.Add((8, 3)); a.Eyes.Add((11, 3));
            Assert.Equal((9, 3), ClothingMasks.Glasses(a));
            var hand = ClothingMasks.Hand(f, j);
            Assert.True(hand.x > j.TorsoRight);
        }

        [Fact]
        public void Doku_KirVeYamaDamgasi_IkonKirpma()
        {
            Assert.Equal(2, ClothingMasks.MaterialShade("linen", 3, 3, 2, false));
            Assert.Equal(3, ClothingMasks.MaterialShade("fur", 3, 3, 2, true));
            Assert.InRange(ClothingMasks.MaterialShade("knit", 1, 0, 2, false), 1, 2);
            var f = Mannequin();
            var j = JointLines.Estimate(f);
            var ramp = RampMath.Make(new Rgb(160, 110, 70));
            var img = new PixelImage(20, 40);
            for (int i = 0; i < img.Px.Length; i++) if (f.Region[i] != Region.Empty) img.Px[i] = new Px32(ramp[2]);
            Assert.Equal(0, ClothingMasks.ApplyDirt(img, f, 0f, ramp));
            Assert.True(ClothingMasks.ApplyDirt(img, f, 1f, ramp) > 0);
            var patch = new Rgb(30, 60, 200);
            Assert.True(ClothingMasks.StampPatches(img, f, j, 2, patch) >= 1);
            var icon = ClothingMasks.CropIcon(img, f, new HashSet<byte> { Region.Shoes }, 16);
            Assert.Equal(16, icon.Width);
            int opaque = 0;
            foreach (var p in icon.Px) if (p.A > 0) opaque++;
            Assert.Equal(12, opaque);   // 6x2 ayakkabı
        }
    }
}
