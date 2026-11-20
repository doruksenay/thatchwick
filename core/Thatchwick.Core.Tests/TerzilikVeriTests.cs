using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Terzilik ve giyim, Parti 1 (belge 48 §5-8, belge 51 §2): katalog, palet, boya karışımı, yaş kuralı, beceri.</summary>
    public class TerzilikVeriTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        internal static ClothingCatalog LoadCatalog(ContentDatabase content, out List<string> errors)
        {
            var path = Path.Combine(ContentFilesTests.ContentDir(), "clothing.json");
            var file = JsonSerializer.Deserialize<ClothingFileDto>(File.ReadAllText(path), Json);
            errors = new List<string>();
            return ClothingCatalog.Map(file, content, errors);
        }

        internal static ClothingCatalog Catalog()
        {
            var db = ContentFilesTests.LoadDb(out _);
            return LoadCatalog(db, out _);
        }

        [Fact]
        public void Katalog_HatasizYuklenir_102ArtiYasSetleriVe8Taki()
        {
            var db = ContentFilesTests.LoadDb(out var cerr);
            Assert.Empty(cerr);
            var cat = LoadCatalog(db, out var errors);
            Assert.Empty(errors);
            // 102 yetişkin modeli + bebek seti 5 (bebek takkesi 102'de) + çocuğa özgü 11 + gebelik 2
            Assert.Equal(120, cat.Count);
            int baby = cat.Models.Count(m => m.Ages.Contains(AgeStage.Baby));
            Assert.Equal(6, baby);
            int child = cat.Models.Count(m => m.Ages.Contains(AgeStage.Child));
            Assert.True(child >= 20, $"çocuk seti {child}");
            Assert.Equal(9, cat.JewelryCount);   // 8 takı + mühür yüzüğü (yüzük çeşidi)
            Assert.Equal(32, cat.InSlot(ClothingSlot.Head).Count(m => !m.Ages.Contains(AgeStage.Baby) || m.Id == "baby_bonnet"));
        }

        [Fact]
        public void Katalog_HerModelinEsyasiVeTarifiVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var cat = LoadCatalog(db, out _);
            foreach (var m in cat.Models)
            {
                Assert.True(db.Items.TryGet(m.ItemId, out var item), m.Id);
                Assert.Equal("clothing", item.Category);
                if (m.CanAcquire("craft") && !m.HasFlag("rewardonly"))
                    Assert.True(item.CraftSkill.HasValue, $"{m.Id}: tarif yok");
            }
        }

        [Fact]
        public void Donem_DizUstuEtekVeEntariYok()
        {
            var cat = Catalog();
            foreach (var m in cat.Models.Where(x => x.Family == 2 || x.Slot == ClothingSlot.Legs && x.Layer == "C"))
                if (m.Cuts.TryGetValue(CutKind.Length, out var lens)) Assert.DoesNotContain("knee_above", lens);
            // erkeğin diz boyu tuniği olağandır
            Assert.Contains("knee_above", cat.Get("tunic").Cuts[CutKind.Length]);
        }

        // ---------- yaş ----------

        [Fact]
        public void Yas_ErgenAcikKesimGiyemez_GencYetiskinGiyer()
        {
            Assert.False(ClothingAge.CanUseCut(CutKind.Collar, "boat", AgeStage.Teen));
            Assert.False(ClothingAge.CanUseCut(CutKind.Slit, "high", AgeStage.Teen));
            Assert.True(ClothingAge.CanUseCut(CutKind.Collar, "round", AgeStage.Teen));
            Assert.True(ClothingAge.CanUseCut(CutKind.Collar, "deep", AgeStage.YoungAdult));
            Assert.True(ClothingAge.CanUseCut(CutKind.Slit, "high", AgeStage.Elder));
            var cat = Catalog();
            // gömleksiz kolsuz üst
            Assert.True(ClothingAge.IsBareArmed(null, cat.Get("felt_vest")));
            Assert.False(ClothingAge.IsBareArmed(cat.Get("linen_undershirt"), cat.Get("felt_vest")));
            // düğün kaftanı ergende yok
            Assert.False(ClothingAge.CanWear(cat.Get("wedding_kaftan"), AgeStage.Teen));
            Assert.True(ClothingAge.CanWear(cat.Get("wedding_kaftan"), AgeStage.YoungAdult));
        }

        [Fact]
        public void Yas_CocukCocukBoyuOlmayanBParcasiniGiyemez()
        {
            var cat = Catalog();
            Assert.False(ClothingAge.CanWear(cat.Get("long_cloak"), AgeStage.Child));     // B, çocuk boyu yok
            Assert.True(ClothingAge.CanWear(cat.Get("child_cloak"), AgeStage.Child));
            Assert.True(ClothingAge.CanWear(cat.Get("caped_hood"), AgeStage.Child));      // çocuk boyu çizilir (48 §13.4)
            Assert.True(ClothingAge.CanWear(cat.Get("carik"), AgeStage.Child));           // A katmanı maske
            foreach (var m in cat.Models.Where(x => x.Ages.Contains(AgeStage.Child)))
                Assert.True(m.ChildSize, m.Id);
            // bebek yalnızca bebek setini giyer
            foreach (var m in cat.Models)
                Assert.Equal(m.Ages.Contains(AgeStage.Baby), ClothingAge.CanWear(m, AgeStage.Baby));
            Assert.False(ClothingAge.CanWear(cat.Get("tunic"), AgeStage.Baby));
        }

        [Fact]
        public void Yas_TakiKurali()
        {
            var cat = Catalog();
            Assert.True(ClothingAge.CanWear(cat.GetJewelry("evil_eye"), AgeStage.Baby));
            Assert.False(ClothingAge.CanWear(cat.GetJewelry("glass_beads"), AgeStage.Baby));
            Assert.True(ClothingAge.CanWear(cat.GetJewelry("glass_beads"), AgeStage.Child));
            Assert.False(ClothingAge.CanWear(cat.GetJewelry("earring"), AgeStage.Child));
            Assert.True(ClothingAge.CanWear(cat.GetJewelry("earring"), AgeStage.Teen));
        }

        // ---------- palet ve karışım ----------

        private static ClothColor C(string id, ClothTone t = ClothTone.Mid) => new ClothColor(id, t);

        [Fact]
        public void Palet_21RenkUcTonArtiAdliTonlar()
        {
            Assert.Equal(21, DyePalette.All.Count(d => d.Toned));
            Assert.Equal(63 + DyePalette.All.Count(d => !d.Toned), DyePalette.Colors().Count());
            Assert.InRange(DyePalette.Colors().Count(), 80, 90);
            foreach (var c in DyePalette.Colors())
            {
                Assert.True(ClothColor.TryParse(c.Key, out var back), c.Key);
                Assert.Equal(c, back);
            }
        }

        [Fact]
        public void Palet_HerRenkKarakterYaratmaPaletindeVeRampasiVar()
        {
            var raw = File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "appearance.json"));
            var app = JsonSerializer.Deserialize<AppearanceFileDto>(raw, Json);
            var set = new HashSet<string>(app.clothColors.Select(h => h.ToUpperInvariant()), StringComparer.Ordinal);
            foreach (var c in DyePalette.Colors()) Assert.Contains(DyePalette.Hex(c).ToUpperInvariant(), set);
        }

        [Fact]
        public void Karisim_KaraYunKokboya_KoyuBordo()
        {
            Assert.Equal(C("oxblood", ClothTone.Single), DyePalette.Mix(DyePalette.Fleece(FleeceColor.Black), DyePalette.Madder, ClothTone.Mid));
            Assert.Equal(C("rosewood", ClothTone.Single), DyePalette.Mix(DyePalette.Fleece(FleeceColor.Grey), DyePalette.Madder, ClothTone.Mid));
            Assert.Equal(C("midnight", ClothTone.Single), DyePalette.Mix(DyePalette.Fleece(FleeceColor.Black), DyePalette.Woad, ClothTone.Mid));
            // ak yün boyanın gerçek rengini, istenen tonda alır
            Assert.Equal(C(DyePalette.Madder, ClothTone.Light), DyePalette.Mix(DyePalette.Fleece(FleeceColor.White), DyePalette.Madder, ClothTone.Light));
            // sarı kara yüne tutmaz
            Assert.Equal(DyePalette.Fleece(FleeceColor.Black), DyePalette.Mix(DyePalette.Fleece(FleeceColor.Black), DyePalette.Weld, ClothTone.Mid, out bool took));
            Assert.False(took);
        }

        [Fact]
        public void Karisim_KirmiziGiysiyeCivit_KoyuMor()
        {
            var red = DyePalette.Mix(DyePalette.Fleece(FleeceColor.White), DyePalette.Madder, ClothTone.Mid);
            Assert.Equal(C(DyePalette.Purple, ClothTone.Dark), DyePalette.Mix(red, DyePalette.Woad, ClothTone.Mid));
        }

        [Fact]
        public void Karisim_BoyaYalnizcaKoyulastirir()
        {
            foreach (var b in DyePalette.Colors())
                foreach (var d in DyePalette.All.Where(x => x.DyeItem != null))
                    foreach (var t in new[] { ClothTone.Light, ClothTone.Mid, ClothTone.Dark })
                    {
                        var r = DyePalette.Mix(b, d.Id, t);
                        Assert.True(DyePalette.Has(r), $"{b}+{d.Id}");
                        Assert.True(DyePalette.Luminance(r) <= DyePalette.Luminance(b) + 1e-9, $"{b} + {d.Id} ({t}) → {r} açıldı");
                    }
        }

        [Fact]
        public void Agartma_YalnizcaAkYunVeKetende()
        {
            Assert.True(DyePalette.CanBleach(C(DyePalette.RawLinen), "linen"));
            Assert.Equal(C(DyePalette.BleachedLinen), DyePalette.Bleach(C(DyePalette.RawLinen), "linen"));
            Assert.True(DyePalette.CanBleach(C(DyePalette.WhiteWool), "wool"));
            Assert.False(DyePalette.CanBleach(C(DyePalette.GreyWool), "wool"));
            Assert.False(DyePalette.CanBleach(C(DyePalette.Madder), "linen"));    // boyalı giysi açılmaz
            Assert.False(DyePalette.CanBleach(C(DyePalette.WhiteWool), "leather"));
        }

        [Fact]
        public void Tonlar_TabanaGore()
        {
            Assert.Equal(3, DyePalette.TonesOn(DyePalette.Fleece(FleeceColor.White)).Length);
            Assert.Equal(2, DyePalette.TonesOn(DyePalette.Fleece(FleeceColor.Grey)).Length);
            Assert.Single(DyePalette.TonesOn(DyePalette.Fleece(FleeceColor.Black)));
            Assert.Equal(3, DyePalette.DyeUnits(ClothTone.Dark));
        }

        [Fact]
        public void BoyaEsyalari_PaletRenginiVerir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var d in DyePalette.All.Where(x => x.DyeItem != null))
            {
                Assert.True(db.Items.Contains(d.DyeItem), d.DyeItem);
                Assert.Same(d, DyePalette.ForDyeItem(d.DyeItem));
            }
        }

        // ---------- beceri ----------

        [Fact]
        public void Terzilik_36BeceriVeDallari()
        {
            Assert.Equal(36, Enum.GetValues(typeof(SkillType)).Length);
            Assert.Equal("Terzilik", SkillNames.Name(SkillType.Tailoring));
            Assert.Equal("Terzi", SkillNames.TradeName(SkillType.Tailoring));
            Assert.True(SkillNames.IsCraft(SkillType.Tailoring));
            Assert.Equal(2, SkillNames.BranchCount(SkillType.Tailoring));
            var db = ContentFilesTests.LoadDb(out _);
            var br = db.Branches.All.Where(b => b.Skill == SkillType.Tailoring).ToList();
            Assert.Equal(2, br.Count);
            Assert.All(br, b => Assert.Equal(5, b.Level));
            Assert.Contains(br, b => b.Id == "branch.tailor_embroider");
            Assert.Contains(br, b => b.Id == "branch.tailor_thrifty");
            // "Giysi dik" Dokumacılık'tan Terzilik'e: cepken tarifi artık Terzilik
            Assert.True(db.Items.TryGet("item.tunic", out var tunic));
            Assert.Equal(SkillType.Tailoring, tunic.CraftSkill);
        }

        [Fact]
        public void HamMadde_TiftikKecisiVeKendir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            Assert.True(db.Animals.TryGet("animal.angora_goat", out var g));
            Assert.Equal("item.mohair", g.Produces);
            Assert.True(db.Items.Contains("item.crop_hemp"));
            Assert.True(db.Items.Contains("item.hemp_cloth"));
            Assert.True(db.Items.Contains("item.tea_black"));
            Assert.True(db.Items.Contains("item.sewing_table"));
        }
    }
}
