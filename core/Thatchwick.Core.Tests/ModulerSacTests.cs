using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Save;

namespace Thatchwick.Core.Tests
{
    /// <summary>Modüler saç ve sakal, 43b çeşit hedefleri, eski kimliklerin göçü (belge 43, 43b).</summary>
    public class ModulerSacTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static AppearanceCatalog Real()
        {
            var path = Path.Combine(ContentFilesTests.ContentDir(), "appearance.json");
            var dto = JsonSerializer.Deserialize<AppearanceFileDto>(File.ReadAllText(path), Json);
            var errors = new List<string>();
            var catalog = AppearanceMapper.Build(dto, errors);
            Assert.True(errors.Count == 0, string.Join(" | ", errors));
            return catalog;
        }

        private static int NonNone(List<OptionDef> l) => l.Count(o => o.Id != Appearance.None);

        [Fact]
        public void GercekKatalog_43bHedefleriniKarsilar()
        {
            var c = Real();
            Assert.Equal(16, c.HairStyles.Count);
            Assert.Equal(14, c.Bangs.Count);                 // "none" = alın açık dahil
            Assert.Equal(12, NonNone(c.HairExtras));
            Assert.Equal(10, NonNone(c.HairOrnaments));
            Assert.Equal(14, NonNone(c.Mustaches));
            Assert.Equal(18, NonNone(c.Beards));
            Assert.Equal(5, NonNone(c.Sideburns));
            Assert.Equal(16, NonNone(c.Hats));
            Assert.Equal(20, c.EyeShapes.Count);
            Assert.Equal(16, c.Brows.Count);
            Assert.Equal(8, c.FaceShapes.Count);
            Assert.Equal(10, c.Noses.Count);
            Assert.Equal(10, c.Mouths.Count);
            Assert.Equal(5, c.Ears.Count);
            Assert.Equal(4, c.Lashes.Count);
            Assert.InRange(c.HairPresets.Count, 60, 80);
        }

        [Fact]
        public void GercekKatalog_HazirStillerVeEskiKimliklerGecerli()
        {
            var c = Real();
            Assert.Empty(c.ValidatePresets());
            // Her kütle en az bir hazır stilde kullanılır; her eklenti de.
            foreach (var m in c.HairStyles) Assert.Contains(c.HairPresets, p => p.Mass == m.Id);
            foreach (var e in c.HairExtras.Where(x => x.Id != Appearance.None)) Assert.Contains(c.HairPresets, p => p.Extra == e.Id);
        }

        [Fact]
        public void GercekKatalog_EskiTamStillerinHepsiCozulur()
        {
            var c = Real();
            // 17.11.2026 kataloğundaki 20 tam stil.
            var old = new[] { "bald", "crop", "short", "side_part", "shaggy", "tousled", "long", "long_wavy", "long_curly", "curls", "afro",
                              "ponytail", "low_tail", "braid", "twin_braids", "crown_braid", "bun", "top_knot", "monk", "shoulder" };
            foreach (var id in old)
            {
                var a = new Appearance { HairStyleId = id, SkinColor = c.SkinTones[0] };
                c.Normalize(a, Sex.Female);
                Assert.Contains(c.HairStyles, h => h.Id == a.HairStyleId);
                Assert.Empty(c.ValidateHair(a.HairStyleId, a.BangsId, a.HairExtraId, a.HairOrnamentId));
                Assert.NotNull(c.HairTextureOf(id) ?? (id == "bald" ? "" : null));
            }
        }

        [Fact]
        public void Goc_AtKuyrugu_KutleVeEklentiOlur_VarOlanOnKorunur()
        {
            var c = Real();
            var a = new Appearance { HairStyleId = "ponytail", BangsId = "straight", SkinColor = c.SkinTones[0] };
            c.Normalize(a, Sex.Female);
            Assert.Equal("shoulder_straight", a.HairStyleId);
            Assert.Equal("ponytail", a.HairExtraId);
            Assert.Equal("straight", a.BangsId);

            var b = new Appearance { HairStyleId = "twin_braids", SkinColor = c.SkinTones[0] };
            c.Normalize(b, Sex.Female);
            Assert.Equal("parted", b.BangsId);       // eski stilin ayrığı, ön boşsa gelir
            Assert.Equal("twin_braids", b.HairExtraId);
        }

        [Fact]
        public void Goc_EskiKirliSakal_DokuyaCevrilir()
        {
            var c = Real();
            var a = new Appearance { FacialHairId = "stubble", SkinColor = c.SkinTones[0] };
            Assert.Equal("stubble", a.BeardId);
            c.Normalize(a, Sex.Male);
            Assert.Equal(Appearance.None, a.BeardId);
            Assert.Equal(2, a.StubbleLevel);
        }

        [Fact]
        public void Uyum_KisaSactaOrguOlmaz_KutleKisalincaEklentiDuser()
        {
            var c = Real();
            Assert.False(c.Fits(c.HairExtras, "braid", "crop"));
            Assert.True(c.Fits(c.HairExtras, "braid", "long"));
            Assert.False(c.Fits(c.Bangs, "straight", "bald"));
            Assert.Contains(c.ValidateHair("crop", Appearance.None, "braid", Appearance.None), e => e.Contains("braid"));

            var a = new Appearance { HairStyleId = "long", HairExtraId = "braid", HairOrnamentId = "bone_comb", BangsId = "braided_front", SkinColor = c.SkinTones[0] };
            Assert.Empty(c.Validate(a, Sex.Female));
            a.HairStyleId = "crop";
            Assert.NotEmpty(c.Validate(a, Sex.Female));
            c.FitHair(a);
            Assert.Equal(Appearance.None, a.HairExtraId);
            Assert.Equal(Appearance.None, a.HairOrnamentId);
            Assert.Equal(Appearance.None, a.BangsId);
            Assert.Empty(c.Validate(a, Sex.Female));
        }

        [Fact]
        public void Uyum_EkrandaYalnizUyanModullerGosterilir()
        {
            var c = Real();
            var forCrop = c.FittingModules(c.HairExtras, "crop").Select(o => o.Id).ToList();
            Assert.Equal(new[] { Appearance.None }, forCrop);
            Assert.Equal(c.HairExtras.Count, c.FittingModules(c.HairExtras, "waist").Count());
        }

        [Fact]
        public void HazirStil_UygulanirVeTanınır()
        {
            var c = Real();
            var p = c.HairPresets.First(x => x.Id == "p_twin_braids_bangs");
            var a = new Appearance { HairOrnamentId = "ribbon" };
            c.ApplyPreset(a, p);
            Assert.Equal(("long", "straight", "twin_braids"), (a.HairStyleId, a.BangsId, a.HairExtraId));
            Assert.Equal("ribbon", a.HairOrnamentId);   // süs korunur
            Assert.Same(p, c.PresetOf(a));
        }

        [Theory]
        [InlineData(Sex.Male)]
        [InlineData(Sex.Female)]
        public void Rastgele_HerZamanGecerli_VeCesitli(Sex sex)
        {
            var c = Real();
            var rng = new Random(7);
            var keys = new HashSet<string>();
            for (int i = 0; i < 600; i++)
            {
                var a = c.Randomize(rng, sex);
                Assert.Empty(c.Validate(a, sex));
                keys.Add($"{a.HairStyleId}|{a.BangsId}|{a.HairExtraId}");
            }
            Assert.True(keys.Count >= 60, $"yalnız {keys.Count} farklı saç");
        }

        [Fact]
        public void Bebek_SakalSusVeEklentiTasimaz()
        {
            var c = Real();
            var rng = new Random(3);
            for (int i = 0; i < 100; i++)
            {
                var mom = c.Randomize(rng, Sex.Female);
                var dad = c.Randomize(rng, Sex.Male);
                var g = Genetics.ForFounder(mom, c, rng);
                var baby = c.FromGenome(g, rng, Sex.Male);
                Assert.Equal(Appearance.None, baby.BeardId);
                Assert.Equal(Appearance.None, baby.SideburnsId);
                Assert.Equal(0, baby.StubbleLevel);
                Assert.Equal(Appearance.None, baby.HairExtraId);
                Assert.Equal(Appearance.None, baby.HairOrnamentId);
                Assert.NotEqual(AppearanceCatalog.SpecialGroup, c.HairStyles.First(h => h.Id == baby.HairStyleId).Group);
                var old = c.Inherit(mom, dad, rng, Sex.Female);
                Assert.Equal(Appearance.None, old.HairOrnamentId);
            }
        }

        [Fact]
        public void Genetik_EskiStilKimligindenDokuOkunur()
        {
            var c = Real();
            var a = new Appearance { HairStyleId = "ponytail", SkinColor = c.SkinTones[0] };   // göç edilmemiş eski kimlik
            var g = Genetics.ForFounder(a, c, new Random(1));
            Assert.Equal("straight", g.Get(Genes.HairTexture).A);
        }

        [Fact]
        public void DunyaDeseni_GozVeKasHepsindeVar()
        {
            var c = Real();
            var eyePatterns = new HashSet<string> { "normal", "wide", "narrow", "hooded", "round", "upturned", "downturned", "small", "deep", "sharp" };
            var browPatterns = new HashSet<string> { "natural", "thick", "thin", "arched", "straight", "bushy", "unibrow", "sparse", "angled", "short" };
            foreach (var e in c.EyeShapes) Assert.Contains(e.Tag, eyePatterns);
            foreach (var b in c.Brows) Assert.Contains(b.Tag, browPatterns);
            Assert.Equal(10, c.EyeShapes.Select(e => e.Tag).Distinct().Count());
            Assert.Equal(10, c.Brows.Select(e => e.Tag).Distinct().Count());
            foreach (var h in c.Hats.Where(x => x.Id != Appearance.None)) Assert.Contains(h.Tag, new[] { "none", "top", "full" });
        }

        [Fact]
        public void Kayit_YeniAlanlarGidipGelir()
        {
            var s = SaveTests.BuildState();
            var a = s.Chars.First();
            a.Appearance.HairStyleId = "long_wavy";
            a.Appearance.BangsId = "curtain";
            a.Appearance.HairExtraId = "half_up";
            a.Appearance.HairOrnamentId = "flower";
            a.Appearance.HairOrnamentColor = new Rgb(200, 40, 60);
            a.Appearance.SideburnsId = "long";
            a.Appearance.StubbleLevel = 3;
            a.Appearance.LashesId = "thick";

            var json = JsonSerializer.Serialize(SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market), Json);
            var data = JsonSerializer.Deserialize<SaveData>(json, Json);
            var b = SaveGame.Load(data, s.Db).Characters.Single(x => x.Id == a.Id);
            Assert.Equal(a.Appearance.Key(), b.Appearance.Key());
            Assert.Equal("half_up", b.Appearance.HairExtraId);
            Assert.Equal(3, b.Appearance.StubbleLevel);
        }

        [Fact]
        public void Kayit_EskiKayittaYeniAlanlarVarsayilan()
        {
            var s = SaveTests.BuildState();
            var d = SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market);
            d.characters[0].appearance = new AppearanceSave { skin = "#F2D6BD", hairStyle = "braid", frame = -1, voice = -1 };
            var c = SaveGame.Load(d, s.Db).Characters.Single(x => x.Id == d.characters[0].id);
            Assert.Equal(Appearance.None, c.Appearance.HairExtraId);
            Assert.Equal(0, c.Appearance.StubbleLevel);
            Assert.Equal("normal", c.Appearance.LashesId);
            Real().MigrateLegacy(c.Appearance);   // GameRoot yüklemeden sonra yapar
            Assert.Equal(("long", "braid"), (c.Appearance.HairStyleId, c.Appearance.HairExtraId));
        }

        [Fact]
        public void Dto_GidisDonusModulleriKorur()
        {
            var c = Real();
            var dto = AppearanceMapper.ToDto(c);
            var errors = new List<string>();
            var back = AppearanceMapper.Build(JsonSerializer.Deserialize<AppearanceFileDto>(JsonSerializer.Serialize(dto, Json), Json), errors);
            Assert.Empty(errors);
            Assert.Equal(c.HairPresets.Count, back.HairPresets.Count);
            Assert.Equal(c.LegacyHairStyles.Count, back.LegacyHairStyles.Count);
            Assert.Equal(c.HairExtras.Select(o => o.Level), back.HairExtras.Select(o => o.Level));
        }

        [Fact]
        public void VarsayilanKatalog_DeModuler()
        {
            var c = AppearanceCatalog.Default();
            Assert.Empty(c.ValidatePresets());
            var rng = new Random(5);
            for (int i = 0; i < 200; i++) Assert.Empty(c.Validate(c.Randomize(rng, i % 2 == 0 ? Sex.Male : Sex.Female), Sex.Male));
        }
    }
}
