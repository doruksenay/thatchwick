using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Karakter yaratma 2.0 (doküman 14): gizli genetik, işaretler, kaza ve iz, Ayna, kayıt.</summary>
    public class KarakterYaratmaTests
    {
        private static readonly AppearanceCatalog Catalog = AppearanceCatalog.Default();

        private static Character Adult(int id, Sex sex = Sex.Male)
        {
            var c = new Character(id, "Kişi" + id, new GridPos(0, 0)) { Sex = sex };
            c.SetAge(40);
            c.Appearance.SkinColor = Catalog.SkinTones[0];
            return c;
        }

        // ---------- genetik ----------

        [Fact]
        public void Genetik_IkiTasiyicidanCekinikRenk_DortteBirCikar()
        {
            string dark = Catalog.HairColors.First(h => !Catalog.RecessiveHairColors.Contains(h)).ToHex();
            string red = Catalog.RecessiveHairColors[0].ToHex();
            var mom = new Genome(); mom.Set(Genes.HairColor, dark, red);
            var dad = new Genome(); dad.Set(Genes.HairColor, red, dark);
            Assert.Equal(dark, Genetics.Expressed(mom, Genes.HairColor, Catalog));   // taşıyıcı ama görünmez

            var rng = new Random(11);
            int shown = 0, n = 4000;
            for (int i = 0; i < n; i++)
            {
                var kid = Genetics.Inherit(mom, dad, rng, Catalog);
                if (Genetics.Expressed(kid, Genes.HairColor, Catalog) == red) shown++;
            }
            double ratio = shown / (double)n;
            Assert.True(ratio > 0.20 && ratio < 0.31, "oran " + ratio);
        }

        [Fact]
        public void Genetik_KurucuSecilenGorunumuIfadeEder()
        {
            var rng = new Random(3);
            for (int i = 0; i < 200; i++)
            {
                var sex = i % 2 == 0 ? Sex.Female : Sex.Male;
                var a = Catalog.Randomize(rng, sex);
                var g = Genetics.ForFounder(a, Catalog, rng);
                var b = a.Clone();
                Genetics.Express(g, b, Catalog);
                Assert.Equal(a.SkinColor, b.SkinColor);
                Assert.Equal(a.EyeColor, b.EyeColor);
                Assert.Equal(a.HairColor, b.HairColor);
                Assert.Equal(a.FaceShapeId, b.FaceShapeId);
                Assert.Equal(a.NoseId, b.NoseId);
                Assert.Equal(a.Build, b.Build);
            }
        }

        [Fact]
        public void Genetik_BoyaKalitilmaz()
        {
            var rng = new Random(5);
            string dark = Catalog.HairColors.First(h => !Catalog.RecessiveHairColors.Contains(h)).ToHex();
            var mom = Adult(1, Sex.Female); var dad = Adult(2);
            mom.Genome = new Genome(); mom.Genome.Set(Genes.HairColor, dark, dark);
            dad.Genome = new Genome(); dad.Genome.Set(Genes.HairColor, dark, dark);
            mom.Appearance.HairColor = new Rgb(20, 200, 90);   // Ayna'da boyandı
            for (int i = 0; i < 100; i++)
            {
                var kid = Genetics.Inherit(mom.Genome, dad.Genome, rng, Catalog);
                var hair = Genetics.Expressed(kid, Genes.HairColor, Catalog);
                // Boya (yeşil) hiç geçmez; %2 mutasyon yalnızca paletteki komşu renge kaydırır.
                Assert.NotEqual(mom.Appearance.HairColor.ToHex(), hair);
                Assert.Contains(Catalog.HairColors, h => h.ToHex() == hair);
            }
        }

        [Fact]
        public void Genetik_GenomDisaVeriliripGeriAlinir()
        {
            var g = Genetics.ForFounder(Catalog.Randomize(new Random(1), Sex.Female), Catalog, new Random(2));
            var back = Genome.Import(g.Export());
            Assert.Equal(g.Export(), back.Export());
            Assert.Null(Genome.Import(new string[0]));
            Assert.Null(Genome.Import(new[] { "bozuk" }));
        }

        [Fact]
        public void Nufus_DogumdaGenomKalitilirVeGorunumGenomdanGelir()
        {
            var pop = new Population(1, 9) { AppearanceCatalog = Catalog };
            var mom = Adult(1, Sex.Female); var dad = Adult(2);
            mom.PregnancyDays = 1;
            pop.Add(mom); pop.Add(dad);
            Character baby = null;
            pop.BabyBorn += (b, _, __) => baby = b;
            mom.TryStartPregnancy(2);
            pop.Tick(1.5);
            Assert.NotNull(baby);
            Assert.NotNull(baby.Genome);
            Assert.NotNull(mom.Genome);   // eski karakter: genom görünüşünden kuruldu
            Assert.NotNull(dad.Genome);
            var check = baby.Appearance.Clone();
            Genetics.Express(baby.Genome, check, Catalog);
            Assert.Equal(baby.Appearance.SkinColor, check.SkinColor);
            Assert.Equal(baby.Appearance.EyeColor, check.EyeColor);
            Assert.Empty(Catalog.Validate(baby.Appearance, baby.Sex));
        }

        // ---------- işaretler ----------

        [Fact]
        public void Isaretler_EnYeniUcIzGorunur_GizliGorunmez()
        {
            var a = new Appearance { SkinColor = Catalog.SkinTones[0] };
            for (int i = 0; i < 5; i++) a.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "scratch", Zone = "hand", Source = MarkSource.Event, Day = i });
            a.Marks.Add(new Mark { Kind = MarkKind.Tattoo, Id = "knot", Zone = "larm" });
            var visible = MarkRules.Visible(a.Marks);
            Assert.Equal(3, visible.Count(m => m.Kind == MarkKind.Scar));
            Assert.DoesNotContain(visible, m => m.Kind == MarkKind.Scar && m.Day < 2);
            Assert.Contains(visible, m => m.Kind == MarkKind.Tattoo);

            a.Marks.Last(m => m.Kind == MarkKind.Scar).Hidden = true;
            Assert.DoesNotContain(MarkRules.Visible(a.Marks), m => m.Hidden);
        }

        [Fact]
        public void Isaretler_EskiYuzIzi_YeniListeyeCevrilir()
        {
            var a = new Appearance { FaceMarkId = "scar" };
            Assert.Contains(a.Marks, m => m.Kind == MarkKind.Scar && m.Zone == "cheek");
            var b = new Appearance { FaceMarkId = "freckles" };
            Assert.Contains(b.Marks, m => m.Kind == MarkKind.SkinDetail && m.Id == "freckles");
            var c = new Appearance { FacialHairId = "mustache" };
            Assert.Equal("mustache", c.MustacheId);
            Assert.Equal(Appearance.None, c.BeardId);
        }

        [Fact]
        public void Isaretler_KlonDerinKopyadir()
        {
            var a = new Appearance();
            a.Marks.Add(new Mark { Kind = MarkKind.Tattoo, Id = "knot", Zone = "larm" });
            var b = a.Clone();
            b.Marks[0].Zone = "rarm";
            b.Marks.Add(new Mark { Kind = MarkKind.Jewelry, Id = "ring", Zone = "hand" });
            Assert.Equal("larm", a.Marks[0].Zone);
            Assert.Single(a.Marks);
            Assert.NotEqual(a.Key(), b.Key());
        }

        // ---------- kaza ve iz ----------

        private static InteractionDef Chop() => new InteractionDef { Id = "chop", DisplayName = "Odun kes", Skill = SkillType.Woodcutting };

        [Fact]
        public void Kaza_KapaliysaHicOlmaz_BebekVeKoyluOlmaz()
        {
            var pop = new Population();
            var inj = new InjurySystem(pop, 1) { Setting = AccidentSetting.Off };
            var c = Adult(1); pop.Add(c);
            Assert.False(inj.CanHaveAccident(c));

            inj.Setting = AccidentSetting.Normal;
            Assert.True(inj.CanHaveAccident(c));
            var baby = new Character(2, "Bebek", new GridPos(0, 0)); baby.SetAge(0.5);
            Assert.False(inj.CanHaveAccident(baby));
            var villager = Adult(3); villager.IsVillager = true;
            Assert.False(inj.CanHaveAccident(villager));
        }

        [Fact]
        public void Kaza_UstaCirakinBesteBirindenAz_SeyrekNormalinAltinda()
        {
            Assert.True(InjurySystem.SkillFactor(10) * 5 <= InjurySystem.SkillFactor(1));
            Assert.True(InjurySystem.SettingFactor(AccidentSetting.Rare) < InjurySystem.SettingFactor(AccidentSetting.Normal));
            Assert.Equal(0.0, InjurySystem.SettingFactor(AccidentSetting.Off));
        }

        [Fact]
        public void Kaza_MevsimdeBirVeHanedeGundeBir()
        {
            double today = 10;
            var pop = new Population();
            var inj = new InjurySystem(pop, 1) { Setting = AccidentSetting.Normal, MinDaysBetween = 28, Today = () => today };
            var a = Adult(1); var b = Adult(2);
            pop.Add(a); pop.Add(b);
            var def = inj.DefFor(a, Chop(), true);
            Assert.NotNull(def);
            inj.Injure(a, def);
            Assert.False(inj.CanHaveAccident(b));    // aynı gün hanede ikinci kaza yok
            today = 11;
            Assert.True(inj.CanHaveAccident(b));
            a.Injury = null;
            Assert.False(inj.CanHaveAccident(a));    // aynı mevsim
            today = 10 + 28;
            Assert.True(inj.CanHaveAccident(a));
        }

        [Fact]
        public void Iz_HerIzleSansYariyaIner_TedaviUcteBir_DerinKesin()
        {
            var c = Adult(1);
            var inj = new ActiveInjury { Kind = "axe", Zone = "hand", Pattern = "split" };
            double p0 = InjurySystem.ScarChanceFor(c, inj);
            Assert.Equal(InjurySystem.ScarChance, p0, 6);
            c.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "split", Zone = "hand", Source = MarkSource.Event });
            Assert.Equal(p0 * 0.5, InjurySystem.ScarChanceFor(c, inj), 6);
            c.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "split", Zone = "larm", Source = MarkSource.Event });
            Assert.Equal(p0 * 0.25, InjurySystem.ScarChanceFor(c, inj), 6);
            inj.Treated = true;
            Assert.Equal(p0 * 0.25 / 3, InjurySystem.ScarChanceFor(c, inj), 6);
            inj.Deep = true;
            Assert.Equal(1.0, InjurySystem.ScarChanceFor(c, inj), 6);

            // Seçilmiş (yaratmada) iz azalmayı etkilemez.
            var d = Adult(2);
            d.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "scratch", Zone = "cheek", Source = MarkSource.Chosen });
            Assert.Equal(p0, InjurySystem.ScarChanceFor(d, new ActiveInjury()), 6);
        }

        [Fact]
        public void Kaza_IzliKarakterDahaAzKazaGecirir()
        {
            var pop = new Population();
            var inj = new InjurySystem(pop, 1) { Setting = AccidentSetting.Normal };
            var clean = Adult(1); var scarred = Adult(2);
            scarred.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "split", Zone = "hand", Source = MarkSource.Event });
            var def = inj.DefFor(clean, Chop(), true);
            Assert.True(inj.Chance(scarred, Chop(), def, true) < inj.Chance(clean, Chop(), def, true));
        }

        [Fact]
        public void Hekim_YarayiSarar_MerhemIyilesmeyiKisaltir_IyilesinceSargiKalkar()
        {
            double today = 0;
            var pop = new Population();
            var inj = new InjurySystem(pop, 4) { Setting = AccidentSetting.Normal, Today = () => today };
            var a = Adult(1); pop.Add(a);
            var def = inj.Defs.First(d => d.Id == "axe");
            var w = inj.Injure(a, def);
            Assert.False(w.Treated);
            Assert.True(inj.ApplySalve(a));
            Assert.True(w.Treated);
            Assert.Equal(def.HealDays * 0.5, w.HealDay, 6);
            Assert.False(inj.ApplySalve(a));   // ikinci kez işe yaramaz
            Assert.Equal(w.Zone, AgingLook.For(a, Catalog).BandageZone);

            var healer = Adult(2); pop.Add(healer);
            healer.GainSkillXp(SkillType.Medicine, SkillRules.XpForLevel(InjurySystem.HealerLevel));
            var b = Adult(3); pop.Add(b);
            today = 5;
            var w2 = inj.Injure(b, def);
            Assert.True(w2.Treated);
            Assert.Equal(5 + def.HealDays * InjurySystem.HealerHealFactor, w2.HealDay, 6);

            today = 100;
            inj.DailyUpdate();
            Assert.Null(a.Injury);
            Assert.Null(b.Injury);
            Assert.Null(AgingLook.For(a, Catalog).BandageZone);
        }

        // ---------- Ayna ----------

        [Fact]
        public void Ayna_CocukGenceGecinceBirKezUcretsiz()
        {
            var c = new Character(1, "Edith", new GridPos(0, 0)) { Sex = Sex.Female };
            c.SetAge(AgeConfig.Default.YouthStartDay - 0.01);
            Assert.Equal(AgeStage.Child, c.Stage);
            Assert.False(c.FreeMirrorAvailable);
            c.Tick(0.05);
            Assert.Equal(AgeStage.Youth, c.Stage);
            Assert.True(c.FreeMirrorAvailable);

            var after = c.Appearance.Clone();
            after.HairStyleId = Catalog.HairStyles.First(h => h.Id != c.Appearance.HairStyleId).Id;
            int spent = 0;
            Assert.True(MirrorRules.Apply(c, after, null, Catalog, d => { spent += d; return true; }, out _));
            Assert.Equal(0, spent);
            Assert.False(c.FreeMirrorAvailable);

            var again = c.Appearance.Clone();
            again.HairColor = new Rgb(200, 30, 30);
            Assert.True(MirrorRules.Apply(c, again, null, Catalog, d => { spent += d; return true; }, out _));
            Assert.Equal(MirrorRules.AppearanceDiamonds, spent);
        }

        [Fact]
        public void Ayna_GenetikAlanlarKilitli_IsimElmasla()
        {
            var c = Adult(1);
            var after = c.Appearance.Clone();
            after.EyeColor = Catalog.EyeColors.First(e => e != c.Appearance.EyeColor);
            after.NoseId = Catalog.Noses.First(n => n.Id != c.Appearance.NoseId).Id;
            var q = MirrorRules.Quote(c, after, null, Catalog);
            Assert.False(q.CanApply);
            Assert.Contains(q.Errors, e => e.Contains("göz rengi"));
            Assert.Contains(q.Errors, e => e.Contains("burun"));

            var rename = MirrorRules.Quote(c, c.Appearance.Clone(), "Godric", Catalog);
            Assert.True(rename.CanApply);
            Assert.Equal(MirrorRules.RenameDiamonds, rename.Diamonds);

            c.FreeMirrorAvailable = true;   // ücretsiz Ayna da ismi kapsamaz
            Assert.Equal(MirrorRules.RenameDiamonds, MirrorRules.Quote(c, c.Appearance.Clone(), "Godric", Catalog).Diamonds);

            Assert.False(MirrorRules.Apply(c, c.Appearance.Clone(), "Godric", Catalog, _ => false, out var err));
            Assert.Equal("Kişi1", c.Name);
            Assert.NotNull(err);
        }

        [Fact]
        public void Ayna_KazaIziSilinmez_ElmaslaGizlenir()
        {
            var c = Adult(1);
            c.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "split", Zone = "cheek", Source = MarkSource.Event, Day = 3, Reason = "baltayla yanağını kesti" });
            var after = c.Appearance.Clone();
            after.Marks.Clear();

            c.FreeMirrorAvailable = true;
            Assert.False(MirrorRules.Quote(c, after, null, Catalog).CanApply);   // ücretsizde gizlenmez
            c.FreeMirrorAvailable = false;

            Assert.True(MirrorRules.Apply(c, after, null, Catalog, _ => true, out _));
            var scar = Assert.Single(c.Appearance.Marks);
            Assert.True(scar.Hidden);
            Assert.Equal(1, c.EventScarCount);   // hikâyede ve iz azalmasında sayılır
            Assert.Empty(MarkRules.Visible(c.Appearance.Marks));
        }

        // ---------- yaşlanma ----------

        [Fact]
        public void Yaslanma_ErkenKirlasmaDahaErkenBaslar()
        {
            var cfg = AgeConfig.Default;
            double mid = cfg.AdultStartDay + (cfg.ElderStartDay - cfg.AdultStartDay) * 0.4;
            Assert.Equal(0.0, AgingLook.GrayFraction(mid, cfg, false));
            Assert.True(AgingLook.GrayFraction(mid, cfg, true) > 0);
            Assert.Equal(1.0, AgingLook.GrayFraction(cfg.ElderStartDay + 20, cfg, false));
            Assert.False(AgingLook.Receding(cfg.ElderStartDay, cfg, false));
            Assert.True(AgingLook.Receding(cfg.ElderStartDay, cfg, true));
        }

        // ---------- taslak ----------

        [Fact]
        public void Taslak_YeniOyundaKazaAyariSecilmeli_KurucuyaGenomVerilir()
        {
            var d = CharacterDraft.CreateRandom(Catalog, new Random(4), "Edith");
            d.Accidents = AccidentSetting.NotSet;
            Assert.Empty(d.Validate(Catalog));
            Assert.Contains(d.ValidateNewGame(Catalog), e => e.Contains("Kaza"));
            d.Accidents = AccidentSetting.Rare;
            Assert.Empty(d.ValidateNewGame(Catalog));
            var c = d.CreateCharacter(1, new GridPos(0, 0), true, Catalog, new Random(1));
            Assert.NotNull(c.Genome);
            Assert.Equal(100, c.Appearance.HeightPercent);
            Assert.Equal(d.BirthSeason, c.BirthSeason);
            Assert.Equal(d.LikedColor, c.LikedColor);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_YeniAlanlarGidipGelir()
        {
            var s = SaveTests.BuildState();
            var a = s.Chars[0];
            a.Appearance.MustacheId = "mustache";
            a.Appearance.BeardId = Appearance.None;
            a.Appearance.FaceShapeId = Catalog.FaceShapes.Last().Id;
            a.Appearance.EyeColorRight = new Rgb(40, 90, 140);
            a.Appearance.BrowFollowsHair = false;
            a.Appearance.BrowColor = new Rgb(1, 2, 3);
            a.Appearance.Voice = VoiceTimbre.Deep;
            a.Appearance.VoicePitch = -2;
            a.Appearance.OuterId = Catalog.Outers.Last().Id;
            a.Appearance.Marks.Add(new Mark { Kind = MarkKind.Tattoo, Id = "knot", Zone = "larm", Color = new Rgb(30, 40, 90) });
            a.Appearance.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "split", Zone = "hand", Source = MarkSource.Event, Day = 7, Reason = "baltayla elini kesti", Fade = 1, Hidden = true });
            a.Genome = Genetics.ForFounder(a.Appearance, Catalog, new Random(2));
            a.BirthSeason = 2; a.BirthDayOfSeason = 14;
            a.LikedFood = "turnip"; a.LikedColor = "Mavi"; a.LikedHobby = "Balık"; a.DislikedFood = "cabbage";
            a.FreeMirrorAvailable = true;
            a.LastAccidentDay = 31;
            a.Injury = new ActiveInjury { Kind = "burn", Zone = "hand", Pattern = "burn", StartDay = 31, HealDay = 34, Treated = true, Reason = "ocakta elini yaktı" };

            var json = JsonSerializer.Serialize(SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market), new JsonSerializerOptions { IncludeFields = true });
            var data = JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions { IncludeFields = true });
            var loaded = SaveGame.Load(data, s.Db);
            var b = loaded.Characters.Single(c => c.Id == a.Id);

            Assert.Equal(a.Appearance.Key(), b.Appearance.Key());
            Assert.Equal(VoiceTimbre.Deep, b.Appearance.Voice);
            Assert.Equal(-2, b.Appearance.VoicePitch);
            Assert.Equal(a.Genome.Export(), b.Genome.Export());
            Assert.Equal(2, b.BirthSeason); Assert.Equal(14, b.BirthDayOfSeason);
            Assert.Equal("Balık", b.LikedHobby); Assert.Equal("cabbage", b.DislikedFood);
            Assert.True(b.FreeMirrorAvailable);
            Assert.Equal(31, b.LastAccidentDay);
            Assert.Equal("burn", b.Injury.Kind);
            Assert.True(b.Injury.Treated);
            Assert.Equal(1, b.EventScarCount);
        }

        [Fact]
        public void Kayit_EskiAlanlarCevrilir()
        {
            var old = new AppearanceSave { skin = "#F2D6BD", facialHairStyle = "mustache", faceMark = "freckles", hairStyle = "short", heightPercent = 120, frame = -1, voice = -1 };
            var data = new SaveData();
            var s = SaveTests.BuildState();
            var d = SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market);
            d.characters[0].appearance = old;
            d.characters[0].birthSeason = -1;
            var loaded = SaveGame.Load(d, s.Db);
            var c = loaded.Characters.Single(x => x.Id == d.characters[0].id);
            Assert.Equal("mustache", c.Appearance.MustacheId);
            Assert.Contains(c.Appearance.Marks, m => m.Id == "freckles");
            Assert.Equal(100, c.Appearance.HeightPercent);
            Assert.Null(c.Genome);
        }
    }
}
