using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Customization.Layers;

namespace Thatchwick.Core.Tests
{
    /// <summary>Katmanlı karakter çizimi (belge 43 §4.2-4.5): rampa formülü, bölge haritası, göz/kaş damgası, baş çapası.</summary>
    public class KatmanCizimTests
    {
        // ---------- rampa (P0 palet kilidi) ----------

        [Fact]
        public void Rampa_PythonIleAyni_CharacterRampsJson()
        {
            var path = Path.Combine(ContentFilesTests.ContentDir(), "character_ramps.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            int n = 0;
            foreach (var group in new[] { "skin", "hair", "eye", "cloth" })
            foreach (var entry in doc.RootElement.GetProperty(group).EnumerateArray())
            {
                var ramp = RampMath.Make(Rgb.FromHex(entry.GetProperty("base").GetString()));
                var expected = entry.GetProperty("ramp").EnumerateArray().Select(e => e.GetString()).ToArray();
                Assert.Equal(expected, ramp.Select(c => c.ToHex()).ToArray());
                n++;
            }
            Assert.True(n >= 90, $"yalnız {n} rampa");
            var keys = doc.RootElement.GetProperty("keys");
            Assert.Equal(KeyColors.Shoes.ToHex(), keys.GetProperty("shoes").GetProperty("colors")[2].GetString());
            Assert.Equal(Region.Brow, keys.GetProperty("brow").GetProperty("region").GetInt32());
        }

        [Fact]
        public void Rampa_KatalogdakiHerRenkKilitli()
        {
            var path = Path.Combine(ContentFilesTests.ContentDir(), "character_ramps.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var app = JsonDocument.Parse(File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "appearance.json"))).RootElement;
            foreach (var (catalogKey, rampKey) in new[] { ("skinTones", "skin"), ("hairColors", "hair"), ("eyeColors", "eye"), ("clothColors", "cloth") })
            {
                var locked = doc.RootElement.GetProperty(rampKey).EnumerateArray().Select(e => e.GetProperty("base").GetString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var c in app.GetProperty(catalogKey).EnumerateArray())
                    Assert.True(locked.Contains(c.GetString()), $"{catalogKey} {c.GetString()} rampa dosyasında yok: python tools/art/ramps.py build");
            }
        }

        [Fact]
        public void Rampa_TabanAynen_TonlarKoyudanAciga()
        {
            foreach (var hex in new[] { "#D9A877", "#1C1A19", "#E8D28A", "#3F5F8A", "#8C8C8C" })
            {
                var r = RampMath.Make(Rgb.FromHex(hex));
                Assert.Equal(Rgb.FromHex(hex), r[RampMath.BaseShade]);
                double[] v = r.Select(c => { RampMath.ToHsv(c, out _, out _, out double val); return val; }).ToArray();
                Assert.True(v[0] <= v[1] && v[1] <= v[2] && v[2] <= v[3], hex);
            }
        }

        // ---------- çizim ----------

        // 20x40 manken: baş 8..11 x 2..9 (ten, kontur), gözler (9,6) ve (11,6), üst 6..13 x 12..22, alt 7..12 x 23..34.
        private static (RegionFrame body, FrameAnchor anchor) Mannequin()
        {
            var f = new RegionFrame(20, 40);
            void Fill(int x0, int y0, int x1, int y1, byte region, byte shade = 2)
            {
                for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) { f.Region[y * 20 + x] = region; f.Shade[y * 20 + x] = shade; }
            }
            Fill(7, 2, 12, 10, Region.Outline);
            Fill(8, 3, 11, 9, Region.Skin);
            Fill(9, 6, 9, 6, Region.Iris);
            Fill(11, 6, 11, 6, Region.Iris);
            Fill(5, 11, 14, 22, Region.Outline);
            Fill(6, 12, 13, 21, Region.Main);
            Fill(6, 22, 13, 22, Region.Trim);
            Fill(7, 23, 12, 34, Region.Bottom);
            Fill(7, 35, 12, 36, Region.Shoes);
            var a = new FrameAnchor { HeadX = 10, HeadY = 2 };
            a.Eyes.Add((9, 6)); a.Eyes.Add((11, 6));
            return (f, a);
        }

        private sealed class Parts : IPartSource
        {
            public readonly Dictionary<string, PixelImage> Images = new Dictionary<string, PixelImage>();
            public readonly List<string> Asked = new List<string>();
            public PixelImage Get(string slot, string id, string direction, bool back)
            {
                string key = $"{slot}_{id}_{direction}_{(back ? "bk" : "fr")}";
                Asked.Add(key);
                return Images.TryGetValue(key, out var img) ? img : null;
            }
        }

        /// <summary>Referans saç rampasında, çapanın 2 sağında ve 1 altında tek piksellik bir "saç".</summary>
        private static PixelImage HairDot(int shade = 2, int dx = 2, int dy = 1)
        {
            var img = new PixelImage(PartCanvas.Width, PartCanvas.Height);
            img.Set(PartCanvas.AnchorX + dx, PartCanvas.AnchorY + dy, new Px32(RampMath.Make(KeyColors.ReferenceHair)[shade]));
            return img;
        }

        private static Appearance Look() => new Appearance
        {
            SkinColor = Rgb.FromHex("#C68E5F"), OutfitColor = Rgb.FromHex("#3F5F8A"), TrimColor = Rgb.FromHex("#8A3A32"),
            HairColor = Rgb.FromHex("#A5471F"), FacialHairColor = Rgb.FromHex("#A5471F"), EyeColor = Rgb.FromHex("#2F5D8C"),
            HairStyleId = "short", BangsId = Appearance.None, HairExtraId = Appearance.None, HatId = Appearance.None
        };

        [Fact]
        public void Govde_BolgeRengiGorunumdenGelir()
        {
            var (body, anchor) = Mannequin();
            var a = Look();
            var img = LayerComposer.Compose(body, anchor, "s", LayerLook.From(a), null);
            Assert.Equal(RampMath.Make(a.SkinColor)[2], img.Get(8, 8).Rgb);
            Assert.Equal(a.OutfitColor, img.Get(8, 15).Rgb);
            Assert.Equal(KeyColors.Outline, img.Get(7, 2).Rgb);
            Assert.Equal(0, img.Get(0, 0).A);
        }

        [Fact]
        public void Desen_CizgiliGovdeCapasinaBagli_YurumedeKaymaz()
        {
            var (body, anchor) = Mannequin();
            var a = Look(); a.PatternId = "striped";
            var img = LayerComposer.Compose(body, anchor, "s", LayerLook.From(a), null);
            var main = RampMath.Make(a.OutfitColor);
            var colors = Enumerable.Range(12, 8).Select(y => img.Get(8, y).Rgb).ToList();
            Assert.Contains(main[2], colors);
            Assert.Contains(main[1], colors);

            // Kare 1 piksel aşağı kayınca (yürüme sekmesi) desen de kayar: çapaya göre aynı satır aynı renk.
            var (body2, anchor2) = Mannequin();
            var shifted = new RegionFrame(20, 40);
            for (int y = 39; y >= 1; y--) for (int x = 0; x < 20; x++) { shifted.Region[y * 20 + x] = body2.Region[(y - 1) * 20 + x]; shifted.Shade[y * 20 + x] = body2.Shade[(y - 1) * 20 + x]; }
            var a2 = new FrameAnchor { HeadX = 10, HeadY = 3 }; a2.Eyes.Add((9, 7)); a2.Eyes.Add((11, 7));
            var img2 = LayerComposer.Compose(shifted, a2, "s", LayerLook.From(a), null);
            for (int y = 12; y <= 20; y++) Assert.Equal(img.Get(8, y).Rgb, img2.Get(8, y + 1).Rgb);
        }

        [Fact]
        public void Goz_DesenYuzeBasilir_IrisGozRengi_MankeninGozuOrtulur()
        {
            var (body, anchor) = Mannequin();
            var a = Look();
            var look = LayerLook.From(a);
            look.EyePattern = "small";   // yalnız iris
            var img = LayerComposer.Compose(body, anchor, "s", look, null);
            Assert.Equal(RampMath.Make(a.EyeColor)[1], img.Get(9, 6).Rgb);
            Assert.Equal(RampMath.Make(a.EyeColor)[1], img.Get(11, 6).Rgb);

            look.EyePattern = "sharp";   // üstünde 3 piksel koyu kapak
            var img2 = LayerComposer.Compose(body, anchor, "s", look, null);
            Assert.Equal(KeyColors.Outline, img2.Get(8, 5).Rgb);
            Assert.Equal(KeyColors.Outline, img2.Get(10, 5).Rgb);
        }

        [Fact]
        public void Goz_ArkadanBakinciCizilmez()
        {
            var (body, anchor) = Mannequin();
            var a = Look();
            var img = LayerComposer.Compose(body, anchor, "n", LayerLook.From(a), null);
            Assert.Equal(RampMath.Make(a.SkinColor)[2], img.Get(9, 6).Rgb);   // mankenin gözü tenle örtülü kalır
        }

        [Fact]
        public void Kas_CatikKasIkiGozArasindaKopru()
        {
            var (body, anchor) = Mannequin();
            var look = LayerLook.From(Look());
            look.BrowPattern = "unibrow";
            var img = LayerComposer.Compose(body, anchor, "s", look, null);
            Assert.Equal(look.Brow[0], img.Get(10, 6 + FacePatterns.BrowOffsetY).Rgb);
        }

        [Fact]
        public void KirliSakal_GozunAltindakiTeniKoyulastirir()
        {
            var (body, anchor) = Mannequin();
            var a = Look();
            var plain = LayerComposer.Compose(body, anchor, "s", LayerLook.From(a), null);
            a.StubbleLevel = 3;
            var img = LayerComposer.Compose(body, anchor, "s", LayerLook.From(a), null);
            Assert.NotEqual(plain.Get(10, 9).Rgb, img.Get(10, 9).Rgb);
            Assert.Equal(plain.Get(10, 4).Rgb, img.Get(10, 4).Rgb);   // göz üstü değişmez
        }

        [Fact]
        public void BasParcasi_CapayaOturur_SacRenginiAlir()
        {
            var (body, anchor) = Mannequin();
            var a = Look();
            var parts = new Parts();
            parts.Images["mass_short_s_fr"] = HairDot(2, 2, 1);
            var img = LayerComposer.Compose(body, anchor, "s", LayerLook.From(a), parts);
            Assert.Equal(a.HairColor, img.Get(anchor.HeadX + 2, anchor.HeadY + 1).Rgb);
            Assert.Contains("mass_short_s_bk", parts.Asked);   // önce arka katman istenir
        }

        [Fact]
        public void BasParcasi_BatiYonuDogununAynasi()
        {
            var (body, anchor) = Mannequin();
            var parts = new Parts();
            parts.Images["mass_short_e_fr"] = HairDot(2, 2, 1);
            var img = LayerComposer.Compose(body, anchor, "w", LayerLook.From(Look()), parts);
            Assert.Contains("mass_short_e_fr", parts.Asked);
            Assert.DoesNotContain(parts.Asked, k => k.Contains("_w_"));
            // 32+2 → 2*32-1-34 = 29: çapanın 3 solu (çift genişlikte ayna).
            Assert.Equal(Look().HairColor, img.Get(anchor.HeadX - 3, anchor.HeadY + 1).Rgb);
        }

        [Fact]
        public void Baslik_TamOrtuluSaciCizmez_UstOrtuluCizer()
        {
            var catalog = AppearanceCatalog.Default();
            var (body, anchor) = Mannequin();
            var a = Look();
            var parts = new Parts();
            parts.Images["mass_short_s_fr"] = HairDot();
            var look = LayerLook.From(a, catalog);
            look.HatId = "hood"; look.HatCover = "full";
            var hooded = LayerComposer.Compose(body, anchor, "s", look, parts);
            Assert.NotEqual(a.HairColor, hooded.Get(anchor.HeadX + 2, anchor.HeadY + 1).Rgb);
            look.HatCover = "top";
            var capped = LayerComposer.Compose(body, anchor, "s", look, parts);
            Assert.Equal(a.HairColor, capped.Get(anchor.HeadX + 2, anchor.HeadY + 1).Rgb);
        }

        [Fact]
        public void Renklendirme_KonturVeUzakRenkKalir()
        {
            var refRamp = RampMath.Make(KeyColors.ReferenceHair);
            var target = RampMath.Make(Rgb.FromHex("#E8D28A"));
            Assert.Equal(target[0], LayerComposer.Recolor(refRamp[0], refRamp, target));
            Assert.Equal(target[3], LayerComposer.Recolor(refRamp[3], refRamp, target));
            Assert.Equal(KeyColors.Outline, LayerComposer.Recolor(KeyColors.Outline, refRamp, target));
            var gold = new Rgb(222, 186, 84);
            Assert.Equal(gold, LayerComposer.Recolor(gold, RampMath.Make(KeyColors.ReferenceCloth), target));
        }

        [Fact]
        public void BolgeHaritasi_KodVeTonCozulur()
        {
            var enc = new PixelImage(3, 1);
            enc.Px[0] = new Px32(Region.Main, 1, 0, 255);
            enc.Px[1] = new Px32(Region.Skin, 9, 0, 255);   // ton taşarsa 3'e kırpılır
            enc.Px[2] = new Px32(Region.Main, 1, 0, 0);     // saydam
            var f = RegionFrame.Decode(enc);
            Assert.Equal((Region.Main, (byte)1), (f.Region[0], f.Shade[0]));
            Assert.Equal((byte)3, f.Shade[1]);
            Assert.Equal(Region.Empty, f.Region[2]);
        }

        [Fact]
        public void BolgeHaritasi_CapalarMaviKanaldan_YoksaTahmin()
        {
            var enc = new PixelImage(6, 6);
            for (int y = 1; y < 6; y++) for (int x = 1; x < 5; x++) enc.Px[y * 6 + x] = new Px32(Region.Skin, 2, 0, 255);
            enc.Px[1 * 6 + 3] = new Px32(Region.Outline, 2, RegionFrame.HeadMark, 255);
            enc.Px[3 * 6 + 4] = new Px32(Region.Iris, 2, RegionFrame.EyeMark, 255);
            enc.Px[3 * 6 + 2] = new Px32(Region.Iris, 2, RegionFrame.EyeMark, 255);
            var f = RegionFrame.Decode(enc);
            Assert.Equal((3, 1), (f.Anchor.HeadX, f.Anchor.HeadY));
            Assert.Equal(new[] { (2, 3), (4, 3) }, f.Anchor.Eyes.ToArray());

            enc.Px[1 * 6 + 3] = new Px32(Region.Outline, 2, 0, 255);   // işaret yok: en üst opak satırın ortası
            var g = RegionFrame.Decode(enc);
            Assert.Equal(1, g.Anchor.HeadY);
            Assert.InRange(g.Anchor.HeadX, 2, 3);
        }

        [Fact]
        public void Gorunum_GozVeKasDeseniKatalogTagindenGelir()
        {
            var c = AppearanceCatalog.Default();
            c.EyeShapes.First(o => o.Id == "round").Tag = "round";
            var a = Look(); a.EyeShapeId = "round"; a.HatId = "hood";
            c.Hats.First(o => o.Id == "hood").Tag = "full";
            var look = LayerLook.From(a, c);
            Assert.Equal("round", look.EyePattern);
            Assert.Equal("full", look.HatCover);
            Assert.All(FacePatterns.Eyes.Values, p => Assert.Equal(3, p.Length));
            Assert.Equal(10, FacePatterns.Eyes.Count);
            Assert.Equal(10, FacePatterns.Brows.Count);
        }
    }
}
