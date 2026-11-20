using System;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Life;
using Thatchwick.Core.Progress;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Beceri seti kararları (Project belgesi 45, 19.11.2026): kilit becerisi, adlar, XP bonus havuzu, ergen tavanı,
    // Ev düzenleme konforu, Zanaatkâr defteri, Ticaret XP'si, seviye → açılan tablosu, alkol yalnızca +18 paketinde.
    public class BeceriSeti45Tests
    {
        private static Character Person(int id = 1, double age = 45)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(age);
            return c;
        }

        // ---------- adlar ve kilitler (3.1, karar 1 ve 5) ----------

        [Fact]
        public void Adlar_HerBecerininGorunenVeMeslekAdiVar()
        {
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                Assert.NotEqual(s.ToString(), SkillNames.Name(s));
                Assert.NotEqual("Köylü", SkillNames.TradeName(s));
            }
            Assert.Equal("Aktarlık", SkillNames.Name(SkillType.Herbalism));
            Assert.Equal("İçecekçilik", SkillNames.Name(SkillType.Brewing));
        }

        [Fact]
        public void Kilit_TarifinBecerisine_TarifsizKategoriye()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.Equal(SkillType.Pottery, UnlockGate.SkillFor(db.Items.Get("item.storage_jar")));   // önce Marangozluk
            Assert.Equal(SkillType.Chandlery, UnlockGate.SkillFor(db.Items.Get("item.candle")));
            Assert.Equal(SkillType.Masonry, UnlockGate.SkillFor(db.Items.Get("item.fountain")));       // önce Çiftçilik (garden)
            Assert.Equal(SkillType.Carpentry, UnlockGate.SkillFor(db.Items.Get("item.stool_wood")));
            // Tarifli her yerleştirilebilir eşya, onu üreten tarifin becerisine kilitlenir.
            foreach (var d in db.Items.All)
                if (d.CraftSkill.HasValue) Assert.Equal(d.CraftSkill.Value, UnlockGate.SkillFor(d));
        }

        // ---------- XP bonus havuzu (karar 4) ----------

        [Fact]
        public void Xp_BonuslarToplanir_TavanYuzde100()
        {
            var a = Person(1);
            a.ExtraXpFactor = _ => 1.5;                       // yetiştirme +%50
            a.Modifiers.SetSkillRate(SkillType.Smithing, 1.2f);   // dal +%20
            Assert.Equal(17.0, a.GainSkillXp(SkillType.Smithing, 10), 3);   // 10 x (1 + 0,5 + 0,2); çarpımla 18 olurdu

            var b = Person(2);
            b.ExtraXpFactor = _ => 3.0;                       // +%200 → tavan +%100
            b.Modifiers.SetSkillRate(SkillType.Smithing, 1.2f);
            Assert.Equal(20.0, b.GainSkillXp(SkillType.Smithing, 10), 3);

            var c = Person(3);
            c.ExtraXpFactor = _ => 3.0;
            c.Skills.SetAptitude(SkillType.Smithing, Aptitude.Major);   // yatkınlık havuzun dışında: x2
            Assert.Equal(40.0, c.GainSkillXp(SkillType.Smithing, 10), 3);
        }

        [Fact]
        public void Xp_EksilerCarpanOlarakKalir()
        {
            var a = Person(1);
            a.ExtraXpFactor = _ => 1.5;
            a.Modifiers.SetSkillRate(SkillType.Weaving, 0.8f);   // eksi
            Assert.Equal(12.0, a.GainSkillXp(SkillType.Weaving, 10), 3);   // 10 x 0,8 x 1,5
        }

        // ---------- ergen tavanı (karar 6) ----------

        [Fact]
        public void Ergen_TavanAlti()
        {
            Assert.Equal(6, SkillRules.LevelCap(AgeStage.Teen));
        }

        // ---------- Ev düzenleme (karar 7) ----------

        [Fact]
        public void EvDuzenleme_KonforVerir()
        {
            Assert.Equal(0, MoodService.ArrangementComfort(3));
            Assert.Equal(1, MoodService.ArrangementComfort(4));
            Assert.Equal(2, MoodService.ArrangementComfort(7));
            Assert.Equal(3, MoodService.ArrangementComfort(10));
            var rc = new RoomComfort { Score = 5, Arrangement = 2 };
            Assert.Equal(7, rc.Total);
            Assert.Contains("Yerleştirme +2", rc.Breakdown());
        }

        // ---------- Zanaatkâr defteri (3.3) ----------

        [Fact]
        public void Defter_HerhangiBirBeceri3teAcilir()
        {
            Assert.False(UnlockGate.NotebookOpen(s => 2, accountLevel: 20));
            Assert.True(UnlockGate.NotebookOpen(s => s == SkillType.Music ? 3 : 1, accountLevel: 1));
            Assert.True(UnlockGate.NotebookOpen(null, accountLevel: 5));    // beceri sağlayıcısı yoksa eski kural
            Assert.False(UnlockGate.NotebookOpen(null, accountLevel: 4));
        }

        // ---------- Ticaret XP'si (5.3) ----------

        [Fact]
        public void Ticaret_XpKazancinKarekokuyleBuyur()
        {
            Assert.Equal(0.0, Market.SaleXp(0), 3);
            Assert.Equal(17.0, Market.SaleXp(100), 3);
            Assert.True(Market.SaleXp(1000) < 50);    // eski kuralla 300
            Assert.True(Market.SaleXp(10) > 3);       // küçük satış eskisinden az değil
        }

        // ---------- seviye → açılan tablosu (9. bölüm) ----------

        [Fact]
        public void Tablo_HerBecerininHerSeviyesindeBirSeyVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                for (int lv = 1; lv <= SkillRules.MaxLevel; lv++)
                    Assert.True(db.SkillUnlocks.Any(u => u.Skill == s && u.Level == lv), $"{s} {lv}");
        }

        [Fact]
        public void Tablo_SeviyeKartiGercekAcilanlariGosterir_PlanlananlariGostermez()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var mining2 = UnlockGate.NewAtLevel(db, SkillType.Mining, 2, 20);
            Assert.Contains(mining2, t => t.Contains("Demir cevheri"));
            // Planlanmış satır kartta yok, şeritte "(yakında)".
            Assert.DoesNotContain(UnlockGate.NewAtLevel(db, SkillType.Medicine, 2, 20), t => t.Contains("Muayene"));
            Assert.Contains("(yakında)", UnlockGate.LevelSummary(db, SkillType.Medicine, 2));
        }

        // ---------- baş üstü çubuk (bölüm 10) ----------

        [Fact]
        public void Cubuk_XpOlayiVeSeviyeIciIlerleme()
        {
            var c = Person(1);
            SkillType seen = SkillType.Farming; double got = 0;
            c.Skills.XpGained += (s, x) => { seen = s; got += x; };
            c.GainSkillXp(SkillType.Woodcutting, 75);   // seviye 2 (50) + 25/150
            Assert.Equal(SkillType.Woodcutting, seen);
            Assert.Equal(75.0, got, 3);
            Assert.Equal(25.0 / 150.0, c.Skills.LevelProgress(SkillType.Woodcutting), 3);
            Assert.Equal(1.0, Person(2).Skills.LevelProgress(SkillType.Woodcutting) + 1.0, 3);   // XP'siz: 0
        }

        // ---------- alkol yalnızca +18 paketinde (karar 5) ----------

        [Fact]
        public void Alkol_PaketsizGizli()
        {
            var db = ContentFilesTests.LoadDb(out _);
            bool before = ContentPacks.Adult;
            try
            {
                ContentPacks.Adult = false;
                var barrel = db.Items.Get("item.brew_barrel");
                Assert.True(barrel.Interactions.First(i => i.Id == "craft:ale").Hidden);
                Assert.True(barrel.Interactions.First(i => i.Id == "craft:mead").Hidden);
                Assert.False(barrel.Interactions.First(i => i.Id == "craft:serbet").Hidden);
                Assert.True(ContentPacks.IsHiddenAlcohol("item.ale"));
                Assert.False(ContentPacks.IsHiddenAlcohol("item.boza"));
                ContentPacks.Adult = true;
                Assert.False(barrel.Interactions.First(i => i.Id == "craft:ale").Hidden);
            }
            finally { ContentPacks.Adult = before; }
        }
    }
}
