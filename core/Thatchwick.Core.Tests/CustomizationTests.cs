using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class CustomizationTests
    {
        private static readonly AppearanceCatalog Catalog = AppearanceCatalog.Default();

        [Fact]
        public void RenkHex_OkunurVeYazilir()
        {
            Assert.Equal(new Rgb(255, 0, 128), Rgb.FromHex("#FF0080"));
            Assert.Equal("#FF0080", new Rgb(255, 0, 128).ToHex());
            Assert.Equal(new Rgb(1, 2, 3), Rgb.FromHex("010203"));
            Assert.False(Rgb.TryParseHex("#12345", out _));
            Assert.False(Rgb.TryParseHex("#GG0000", out _));
            Assert.False(Rgb.TryParseHex(null, out _));
        }

        [Fact]
        public void Lerp_OrtayiVerir()
        {
            var mid = Rgb.Lerp(new Rgb(0, 0, 0), new Rgb(100, 200, 50), 0.5);
            Assert.Equal(new Rgb(50, 100, 25), mid);
        }

        [Fact]
        public void VarsayilanKatalog_GecerliVeVarsayilanGorunumGecer()
        {
            var a = new Appearance { SkinColor = Catalog.SkinTones[0] };
            Assert.Empty(Catalog.Validate(a, Sex.Male));
            Assert.Empty(Catalog.Validate(a, Sex.Female));
        }

        [Fact]
        public void Rastgele_HerZamanGecerli()
        {
            var rng = new Random(5);
            for (int i = 0; i < 500; i++)
            {
                var sex = i % 2 == 0 ? Sex.Female : Sex.Male;
                var a = Catalog.Randomize(rng, sex);
                var errors = Catalog.Validate(a, sex);
                Assert.True(errors.Count == 0, string.Join("; ", errors));
            }
        }

        [Fact]
        public void KadinaSakalVerilemez_ErkegeElbiseVerilemez()
        {
            var a = new Appearance { SkinColor = Catalog.SkinTones[0], FacialHairId = "beard" };
            Assert.NotEmpty(Catalog.Validate(a, Sex.Female));

            var b = new Appearance { SkinColor = Catalog.SkinTones[0], OutfitId = "dress" };
            Assert.NotEmpty(Catalog.Validate(b, Sex.Male));
        }

        [Fact]
        public void BilinmeyenSecenek_Boy_TenRengi_Reddedilir()
        {
            var a = new Appearance { SkinColor = Catalog.SkinTones[0], HairStyleId = "mohawk" };
            Assert.Contains(Catalog.Validate(a, Sex.Male), e => e.Contains("mohawk"));

            var tall = new Appearance { SkinColor = Catalog.SkinTones[0], HeightPercent = 150 };
            Assert.NotEmpty(Catalog.Validate(tall, Sex.Male));

            var blue = new Appearance { SkinColor = new Rgb(0, 0, 255) };
            Assert.NotEmpty(Catalog.Validate(blue, Sex.Male));
        }

        [Fact]
        public void OzgurSacGozKiyafetRengi_Serbest()
        {
            var a = new Appearance
            {
                SkinColor = Catalog.SkinTones[2],
                HairColor = new Rgb(12, 200, 99),
                EyeColor = new Rgb(1, 2, 3),
                OutfitColor = new Rgb(250, 0, 250)
            };
            Assert.Empty(Catalog.Validate(a, Sex.Male));
        }

        [Fact]
        public void CinsiyetDegisince_UygunsuzSecenekler_Duzelir()
        {
            var d = new CharacterDraft { Name = "Aldric" };
            d.SetSex(Sex.Male, Catalog);
            d.Appearance.FacialHairId = "beard";
            d.Appearance.OutfitId = "tunic";
            d.SetSex(Sex.Female, Catalog);
            Assert.Equal(Appearance.None, d.Appearance.FacialHairId);
            Assert.Empty(d.Validate(Catalog));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("Al", true)]
        [InlineData("Maud of Thatchwick", false)]   // 18 karakter
        [InlineData("Joan d'Arc", true)]
        [InlineData("Jean-Luc", true)]
        [InlineData("Wat123", false)]
        [InlineData("  Edith  ", true)]
        public void Isim_Kurallari(string name, bool valid)
        {
            var d = new CharacterDraft { Name = name };
            d.Appearance.SkinColor = Catalog.SkinTones[0];
            d.SetSex(Sex.Male, Catalog);
            Assert.Equal(valid, d.Validate(Catalog).Count == 0);
        }

        [Fact]
        public void Taslak_Karakter_Uretir_YasVeBeceriEgilimiUygulanir()
        {
            var d = new CharacterDraft { Name = "Aldric", StartStage = AgeStage.Youth, SkillFocus = SkillType.Carpentry };
            d.Appearance.SkinColor = Catalog.SkinTones[1];
            d.SetSex(Sex.Male, Catalog);
            d.Appearance.HairColor = new Rgb(10, 20, 30);

            var c = d.CreateCharacter(7, new GridPos(2, 3), true, Catalog, new Random(1));

            Assert.Equal(7, c.Id);
            Assert.Equal("Aldric", c.Name);
            Assert.Equal(Sex.Male, c.Sex);
            Assert.True(c.IsPlayerControlled);
            Assert.Equal(AgeStage.Youth, c.Stage);
            Assert.Equal(30.0, c.AgeDays, 6);
            Assert.Equal(2, c.Skills.GetLevel(SkillType.Carpentry));
            Assert.Equal(1, c.Skills.GetLevel(SkillType.Cooking));
            Assert.Equal(new Rgb(10, 20, 30), c.Appearance.HairColor);
            Assert.InRange(c.LifespanDays, 96.0, 144.0);
        }

        [Fact]
        public void Taslak_Gorunumu_Kopyalar_SonradanDegisiklikEtkilemez()
        {
            var d = new CharacterDraft { Name = "Maud" };
            d.Appearance.SkinColor = Catalog.SkinTones[0];
            d.SetSex(Sex.Female, Catalog);
            var c = d.CreateCharacter(1, new GridPos(0, 0), false, Catalog, new Random(1));
            d.Appearance.HairColor = new Rgb(1, 1, 1);
            Assert.NotEqual(new Rgb(1, 1, 1), c.Appearance.HairColor);
        }

        [Fact]
        public void GecersizTaslak_KarakterUretmez()
        {
            var d = new CharacterDraft { Name = "" };
            Assert.Throws<ArgumentException>(() => d.CreateCharacter(1, new GridPos(0, 0), true, Catalog, new Random(1)));
        }

        [Fact]
        public void CocukOlarakBaslanamaz()
        {
            var d = new CharacterDraft { Name = "Wat", StartStage = AgeStage.Child };
            d.Appearance.SkinColor = Catalog.SkinTones[0];
            d.SetSex(Sex.Male, Catalog);
            Assert.NotEmpty(d.Validate(Catalog));
        }

        [Fact]
        public void Miras_TenOrtalamasi_SacGozEbeveyndenBiri()
        {
            var mother = new Appearance { SkinColor = Catalog.SkinTones[0], HairColor = new Rgb(1, 1, 1), EyeColor = new Rgb(2, 2, 2) };
            var father = new Appearance { SkinColor = Catalog.SkinTones[7], HairColor = new Rgb(9, 9, 9), EyeColor = new Rgb(8, 8, 8) };
            var rng = new Random(2);
            for (int i = 0; i < 100; i++)
            {
                var baby = Catalog.Inherit(mother, father, rng, i % 2 == 0 ? Sex.Female : Sex.Male);
                Assert.Contains(baby.HairColor, new[] { mother.HairColor, father.HairColor });
                Assert.Contains(baby.EyeColor, new[] { mother.EyeColor, father.EyeColor });
                Assert.Contains(baby.SkinColor, Catalog.SkinTones);
                Assert.Equal(Appearance.None, baby.FacialHairId);
                Assert.Equal(Appearance.None, baby.HatId);
                Assert.Empty(Catalog.Validate(baby, i % 2 == 0 ? Sex.Female : Sex.Male));
            }
        }

        [Fact]
        public void Nufus_DogumdaGorunumuEbeveynlerdenTureter()
        {
            var pop = new Population(1, 4) { AppearanceCatalog = Catalog };
            var mother = new Character(1, "Maud", new GridPos(0, 0)) { Sex = Sex.Female, Appearance = new Appearance { SkinColor = Catalog.SkinTones[0], HairColor = new Rgb(1, 1, 1) } };
            var father = new Character(2, "Aldric", new GridPos(0, 0)) { Sex = Sex.Male, Appearance = new Appearance { SkinColor = Catalog.SkinTones[0], HairColor = new Rgb(9, 9, 9) } };
            mother.SetAge(40); father.SetAge(40);
            mother.PregnancyDays = 1;
            pop.Add(mother); pop.Add(father);
            Character baby = null;
            pop.BabyBorn += (b, _, __) => baby = b;
            mother.TryStartPregnancy(2);
            pop.Tick(1.5);
            Assert.NotNull(baby);
            Assert.Contains(baby.Appearance.HairColor, new[] { new Rgb(1, 1, 1), new Rgb(9, 9, 9) });
        }

        [Fact]
        public void Mapper_VarsayilanKatalogDtoyaVeGeriDonusur()
        {
            var dto = AppearanceMapper.ToDto(Catalog);
            var errors = new List<string>();
            var back = AppearanceMapper.Build(dto, errors);
            Assert.Empty(errors);
            Assert.Equal(Catalog.HairStyles.Count, back.HairStyles.Count);
            Assert.Equal(Catalog.SkinTones.Count, back.SkinTones.Count);
            Assert.Equal(Catalog.Outfits.Single(o => o.Id == "dress").OnlyFor, back.Outfits.Single(o => o.Id == "dress").OnlyFor);
        }

        [Fact]
        public void Mapper_HatalariToplar()
        {
            var dto = AppearanceMapper.ToDto(Catalog);
            dto.hairColors = new[] { "#GGGGGG", "#112233" };
            dto.hats = new[] { new OptionDto { id = "cap", displayName = "Takke", sex = "alien" } };
            var errors = new List<string>();
            AppearanceMapper.Build(dto, errors);
            Assert.Contains(errors, e => e.Contains("#GGGGGG"));
            Assert.Contains(errors, e => e.Contains("alien"));
            Assert.Contains(errors, e => e.Contains("hats") && e.Contains("none"));

            var empty = new List<string>();
            AppearanceMapper.Build(null, empty);
            Assert.NotEmpty(empty);
        }
    }
}
