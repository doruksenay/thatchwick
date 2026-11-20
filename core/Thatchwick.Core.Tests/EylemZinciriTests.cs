using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // İzlenen eylem zincirleri (Project belgesi 60): iş tek pozda beklemek yerine görünür adımlara bölünür.
    // Zincir işin süresini ve sonucunu değiştirmez.
    public class EylemZinciriTests
    {
        private static ActionChainDef Chain(string id, params (string id, double share, string when)[] steps)
        {
            var c = new ActionChainDef { Id = id };
            foreach (var s in steps) c.Steps.Add(new ActionStepDef { Id = s.id, Pose = s.id, Share = s.share, When = s.when });
            return c;
        }

        private static (WorldState world, Character c, JobRunner runner, ItemInstance item) Setup(ActionChainDef chain, SkillType? skill = null)
        {
            var world = new WorldState(new GridMap(12, 12));
            var item = world.PlaceItem(new ItemDef
            {
                Id = "bench", DisplayName = "Tezgâh",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "work", DisplayName = "Çalış", DurationDays = 0.1, Skill = skill, ChainDef = chain },
                    new InteractionDef { Id = "plain", DisplayName = "Düz iş", DurationDays = 0.1 }
                }
            }, new GridPos(6, 6));
            var c = new Character(1, "Maud", new GridPos(6, 5)) { IsPlayerControlled = true };
            c.SetAge(50);
            return (world, c, new JobRunner(c, world), item);
        }

        [Fact]
        public void Adimlar_PaylarinaGoreSirayla_VeTekSeferHaberVerir()
        {
            var x = Setup(Chain("z", ("hazirla", 1, null), ("yap", 3, null)));
            var seen = new List<string>();
            x.runner.StepChanged += (_, s) => seen.Add(s.Id);
            Assert.Null(x.runner.CurrentStep);                       // iş yok

            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.01);                                     // %10: ilk adım (pay 1/4)
            Assert.Equal("hazirla", x.runner.CurrentStep.Id);
            x.runner.Tick(0.01);                                     // %20: hâlâ ilk adım
            Assert.Equal("hazirla", x.runner.CurrentStep.Id);
            x.runner.Tick(0.02);                                     // %40: ikinci adım
            Assert.Equal("yap", x.runner.CurrentStep.Id);
            x.runner.Tick(0.03);
            Assert.Equal(new[] { "hazirla", "yap" }, seen);

            x.runner.Tick(0.05);                                     // iş biter
            Assert.True(x.runner.IsIdle);
            Assert.Null(x.runner.CurrentStep);
            Assert.Equal(new[] { "hazirla", "yap" }, seen);          // bitişte ek haber yok
        }

        [Fact]
        public void Zincir_ToplamSureyiDegistirmez()
        {
            // Aynı süreli iki iş: biri zincirli, biri düz. İkisi de aynı anda biter.
            foreach (var id in new[] { "work", "plain" })
            {
                var x = Setup(Chain("z", ("a", 1, null), ("b", 2, null), ("c", 5, null)));
                x.runner.Command(Job.UseItem(x.item.Id, id));
                x.runner.Tick(0.099);
                Assert.False(x.runner.IsIdle, id);
                x.runner.Tick(0.002);
                Assert.True(x.runner.IsIdle, id);
            }
        }

        [Fact]
        public void ZincirsizIs_AdimYok()
        {
            var x = Setup(Chain("z", ("a", 1, null)));
            x.runner.Command(Job.UseItem(x.item.Id, "plain"));
            x.runner.Tick(0.02);
            Assert.True(x.runner.IsUsingItem);
            Assert.Null(x.runner.CurrentStep);
            Assert.Empty(x.runner.ActiveSteps);
        }

        [Fact]
        public void Kosul_BeceriyeGore_AcemiVeUstaAdimi()
        {
            var chain = Chain("z", ("yap", 4, null), ("sakarlik", 1, ChainConditions.SkillLow), ("huner", 1, ChainConditions.SkillHigh));

            var acemi = Setup(chain, SkillType.Cooking);
            acemi.runner.Command(Job.UseItem(acemi.item.Id, "work"));
            acemi.runner.Tick(0.001);
            Assert.Equal(new[] { "yap", "sakarlik" }, acemi.runner.ActiveSteps.Select(s => s.Id));

            var usta = Setup(chain, SkillType.Cooking);
            usta.c.Skills.RestoreXp(SkillType.Cooking, SkillRules.XpForLevel(8));
            usta.runner.Command(Job.UseItem(usta.item.Id, "work"));
            usta.runner.Tick(0.001);
            Assert.Equal(new[] { "yap", "huner" }, usta.runner.ActiveSteps.Select(s => s.Id));
        }

        [Fact]
        public void Kosul_RuhHali_IslikYalnizKeyfiYerindeyken()
        {
            var chain = Chain("z", ("yap", 4, null), ("islik", 1, ChainConditions.MoodGood), ("ic_cek", 1, ChainConditions.MoodBad));

            var notr = Setup(chain);
            notr.c.MoodEnabled = true;
            notr.runner.Command(Job.UseItem(notr.item.Id, "work"));
            notr.runner.Tick(0.001);
            Assert.Equal(new[] { "yap" }, notr.runner.ActiveSteps.Select(s => s.Id));

            var mutlu = Setup(chain);
            mutlu.c.MoodEnabled = true;
            mutlu.c.AddMoodlet("test.iyi", "Keyifli", 40, 1.0);
            mutlu.runner.Command(Job.UseItem(mutlu.item.Id, "work"));
            mutlu.runner.Tick(0.001);
            Assert.Equal(new[] { "yap", "islik" }, mutlu.runner.ActiveSteps.Select(s => s.Id));

            var mutsuz = Setup(chain);
            mutsuz.c.MoodEnabled = true;
            mutsuz.c.AddMoodlet("test.kotu", "Keyifsiz", -50, 1.0);
            mutsuz.runner.Command(Job.UseItem(mutsuz.item.Id, "work"));
            mutsuz.runner.Tick(0.001);
            Assert.Equal(new[] { "yap", "ic_cek" }, mutsuz.runner.ActiveSteps.Select(s => s.Id));
        }

        [Fact]
        public void Iptal_AdimiTemizler_YeniIsBastanBaslar()
        {
            var x = Setup(Chain("z", ("a", 1, null), ("b", 1, null)));
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.06);
            Assert.Equal("b", x.runner.CurrentStep.Id);
            x.runner.Cancel();
            Assert.Null(x.runner.CurrentStep);

            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.01);
            Assert.Equal("a", x.runner.CurrentStep.Id);
        }

        [Fact]
        public void Soyunma_YalnizIsaretliAdimda()
        {
            var chain = new ActionChainDef { Id = "banyo" };
            chain.Steps.Add(new ActionStepDef { Id = "soyun", Share = 1 });
            chain.Steps.Add(new ActionStepDef { Id = "yikan", Share = 2, Undress = true });
            chain.Steps.Add(new ActionStepDef { Id = "giyin", Share = 1 });
            var x = Setup(chain);
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.01);
            Assert.False(x.runner.IsUndressed);
            x.runner.Tick(0.04);
            Assert.True(x.runner.IsUndressed);
            x.runner.Tick(0.04);
            Assert.False(x.runner.IsUndressed);
        }

        [Fact]
        public void Balon_AdimKimligiVePozuTasir()
        {
            var x = Setup(Chain("z", ("karistir", 1, null)));
            x.runner.Command(Job.UseItem(x.item.Id, "work"));
            x.runner.Tick(0.01);
            var info = ActivityDescriber.Describe(x.c, x.runner, x.world);
            Assert.Equal("karistir", info.StepId);
            Assert.Equal("karistir", info.StepPose);
            Assert.False(info.Walking);
        }

        // ---------- gerçek içerik ----------

        private static InteractionDef Find(ContentDatabase db, string itemId, string interactionId) =>
            db.Items.Get(itemId).Interactions.First(i => i.Id == interactionId);

        [Fact]
        public void GercekIcerik_TemelIslerZincireBagli()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.True(db.Chains.Count >= 40);

            Assert.Equal("cook_pot", Find(db, "item.cooking_pot", "cook_stew").ChainDef.Id);
            Assert.Equal("eat", Find(db, "item.cooking_pot", "eat_apple").ChainDef.Id);
            Assert.Equal("drink", Find(db, "item.cooking_pot", "drink_milk").ChainDef.Id);
            Assert.Equal("bake", db.Items.Get("item.stone_oven").Interactions.First(i => i.Skill == SkillType.Baking && i.Produce.Count > 0).ChainDef.Id);
            Assert.Equal("carpentry", db.Items.Get("item.workbench").Interactions.First(i => i.IsCraft).ChainDef.Id);
            Assert.Equal("sew", db.Items.Get("item.sewing_table").Interactions.First(i => i.IsCraft).ChainDef.Id);

            // Uyku yatağın uzun işidir; "Otur" zincire girmez.
            foreach (var item in db.Items.All)
                foreach (var i in item.Interactions)
                {
                    if (i.Bath) Assert.Equal("bath", i.ChainDef?.Id);
                    if (i.RestoresNeed && i.Need == NeedType.Energy && i.DurationDays >= 0.1) Assert.Equal("sleep", i.ChainDef?.Id);
                    if (i.Id == "sit") Assert.NotEqual("sleep", i.ChainDef?.Id);
                }
        }

        [Fact]
        public void GercekIcerik_HerZincirKullaniliyor_VeYikanmadaSoyunmaVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var used = new HashSet<string>();
            foreach (var item in db.Items.All)
                foreach (var i in item.Interactions)
                    if (i.ChainDef != null) used.Add(i.ChainDef.Id);
            // Eşya dışı işlerin (sosyal, hayvan, karo) zincirleri iş anahtarıyla bağlanır.
            var dead = db.Chains.All.Where(c => !used.Contains(c.Id) && c.Jobs.Count == 0).Select(c => c.Id).ToList();
            Assert.True(dead.Count == 0, "Hiçbir etkileşime bağlanmayan zincir: " + string.Join(", ", dead));

            Assert.True(db.Chains.TryGet("bath", out var bath));
            Assert.Contains(bath.Steps, s => s.Undress);
            Assert.False(bath.Steps[bath.Steps.Count - 1].Undress);   // son adımda giyinik
        }

        [Fact]
        public void GercekIcerik_HerAdiminIkiDildeMetniVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            string dir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var errors = new List<string>();
            var en = UiKeys.All("en");   // adım metinleri ui.chains.<dil>.json alan dosyasında
            var tr = UiKeys.All("tr");
            foreach (var c in db.Chains.All)
                foreach (var s in c.Steps)
                {
                    Assert.True(en.Contains("chain.step." + s.Id), $"{c.Id}/{s.Id}: İngilizce metin yok");
                    Assert.True(tr.Contains("chain.step." + s.Id), $"{c.Id}/{s.Id}: Türkçe metin yok");
                }
        }

        // ---------- içerik doğrulaması ----------

        private static List<string> Apply(ChainDto chain, out ContentDatabase db)
        {
            db = new ContentDatabase();
            var file = new ContentFileDto
            {
                items = new[]
                {
                    new ItemDto
                    {
                        id = "item.bench", displayName = "Tezgâh", category = "work",
                        interactions = new[] { new InteractionDto { id = "work", displayName = "Çalış", need = "Fun", restore = 10, durationDays = 0.1f } }
                    }
                },
                chains = new[] { chain }
            };
            return ContentMapper.Apply(file, db);
        }

        [Fact]
        public void Icerik_KuralIleBaglanir_HataliZincirReddedilir()
        {
            var ok = Apply(new ChainDto
            {
                id = "z", match = new[] { new ChainMatchDto { station = "item.bench" } },
                steps = new[] { new ChainStepDto { id = "a" }, new ChainStepDto { id = "b", share = 2, when = "moodGood" } }
            }, out var db);
            Assert.True(ok.Count == 0, string.Join("\n", ok));
            Assert.Equal("z", db.Items.Get("item.bench").Interactions[0].ChainDef.Id);

            Assert.Contains(Apply(new ChainDto { id = "z", steps = new[] { new ChainStepDto { id = "a", when = "yok" } } }, out _), e => e.Contains("bilinmeyen koşul"));
            Assert.Contains(Apply(new ChainDto { id = "z", steps = new[] { new ChainStepDto { id = "a", when = "moodGood" } } }, out _), e => e.Contains("koşulsuz"));
            Assert.Contains(Apply(new ChainDto { id = "z", match = new[] { new ChainMatchDto { flag = "ucmak" } }, steps = new[] { new ChainStepDto { id = "a" } } }, out _),
                e => e.Contains("bilinmeyen eşleme bayrağı"));
            Assert.Contains(Apply(new ChainDto { id = "z", match = new[] { new ChainMatchDto() }, steps = new[] { new ChainStepDto { id = "a" } } }, out _), e => e.Contains("boş eşleme"));
            Assert.Contains(Apply(new ChainDto { id = "z", steps = new[] { new ChainStepDto { id = "a", held = "item.yok" } } }, out _), e => e.Contains("bulunamadı"));
        }
    }
}
