using System;
using Thatchwick.Core.Mood;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 18a (özellik, T17), 18e (mezar, yas), 32 (sosyal/eğlence ihtiyacı, ruh hali, konfor; T21).
    public class LifeTests
    {
        internal static ContentDatabase Db()
        {
            var db = new ContentDatabase();
            db.Traits.Register(new TraitDef { Id = "gluttonous", DisplayName = "Doyumsuz", Effects = new LifeModifiers { HungerDecay = 1.6f } });
            db.Traits.Register(new TraitDef { Id = "hardy", DisplayName = "Dayanıklı", Effects = new LifeModifiers { HungerDecay = 0.7f, ColdResist = 0.3f } });
            db.Traits.Register(new TraitDef { Id = "cheerful", DisplayName = "Neşeli", Conflicts = new List<string> { "gloomy" }, Effects = new LifeModifiers { MoodBase = 8 } });
            db.Traits.Register(new TraitDef { Id = "gloomy", DisplayName = "Kasvetli", Effects = new LifeModifiers { MoodBase = -8 } });
            db.Traits.Register(new TraitDef { Id = "sociable", DisplayName = "Girişken", Effects = new LifeModifiers { SocialDecay = 2f, SocialGain = 1.3f } });
            db.Items.Register(new ItemDef { Id = "item.grave", DisplayName = "Mezar", Category = "memorial" });
            return db;
        }

        private static ItemDef Rug(string id, int comfort, string style = null) =>
            new ItemDef { Id = id, DisplayName = id, Comfort = comfort, StyleSet = style };

        private static Character NewChar(int id, GridPos pos, params string[] traits)
        {
            var c = new Character(id, "K" + id, pos) { CanStarve = false, MoodEnabled = true };
            c.SetAge(60);
            c.SetTraits(traits);
            return c;
        }

        // ---------- T17: özellikler ----------

        [Fact]
        public void Ozellik_IhtiyacAzalmasiniDegistirir()
        {
            var db = Db();
            var glutton = NewChar(1, new GridPos(0, 0), "gluttonous");
            var hardy = NewChar(2, new GridPos(0, 0), "hardy");
            var plain = NewChar(3, new GridPos(0, 0));
            foreach (var c in new[] { glutton, hardy, plain }) LifeRules.Recompute(c, db);

            for (int i = 0; i < 20; i++) foreach (var c in new[] { glutton, hardy, plain }) c.Tick(0.01);

            float g = glutton.Needs.Get(NeedType.Hunger), h = hardy.Needs.Get(NeedType.Hunger), p = plain.Needs.Get(NeedType.Hunger);
            Assert.True(g < p && p < h, $"Doyumsuz {g} < sıradan {p} < dayanıklı {h} olmalı");
        }

        [Fact]
        public void Ozellik_SoguktanKorur()
        {
            var db = Db();
            var hardy = NewChar(1, new GridPos(0, 0), "hardy");
            var plain = NewChar(2, new GridPos(0, 0));
            LifeRules.Recompute(hardy, db); LifeRules.Recompute(plain, db);
            hardy.AmbientTemperature = plain.AmbientTemperature = -8f;
            for (int i = 0; i < 10; i++) { hardy.Tick(0.01); plain.Tick(0.01); }
            Assert.True(hardy.Needs.Get(NeedType.Warmth) > plain.Needs.Get(NeedType.Warmth));
        }

        [Fact]
        public void IkiFarkliOzellik_AyniKosuldaOtonomideFarkliDavranir()
        {
            var db = Db();
            // İki NPC de yemek yiyebilir; doyumsuz daha erken acıkıp yemeğe gider.
            double FirstActionDay(string trait)
            {
                var world = new WorldState(new GridMap(10, 10));
                world.PlaceItem(new ItemDef
                {
                    Id = "pot", DisplayName = "Kazan",
                    Interactions = new List<InteractionDef> { new InteractionDef { Id = "eat", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.05 } }
                }, new GridPos(5, 5));
                var c = NewChar(1, new GridPos(0, 0), trait);
                LifeRules.Recompute(c, db);
                var runner = new JobRunner(c, world) { Autonomous = true };
                for (int i = 0; i < 400; i++)
                {
                    c.Tick(0.005);
                    runner.Tick(0.005);
                    if (!runner.IsIdle) return i * 0.005;
                }
                return double.MaxValue;
            }
            Assert.True(FirstActionDay("gluttonous") < FirstActionDay("hardy"));
        }

        [Fact]
        public void CakisanOzellikler_BirlikteVerilmez()
        {
            var db = Db();
            var rng = new Random(5);
            for (int i = 0; i < 100; i++)
            {
                var traits = LifeRules.RollTraits(db, rng, 2);
                Assert.Equal(2, traits.Count);
                Assert.False(traits.Contains("cheerful") && traits.Contains("gloomy"));
            }
            Assert.False(LifeRules.CanAdd(db, new[] { "cheerful" }, "gloomy"));
            Assert.False(LifeRules.CanAdd(db, new[] { "hardy", "sociable" }, "gluttonous")); // en çok 2
        }

        [Fact]
        public void Cocuk_EbeveynlerdenBirerOzellikAlir()
        {
            var db = Db();
            var pop = new Population(seed: 7) { Content = db };
            var mother = new Character(1, "Ana", new GridPos(0, 0)) { Sex = Sex.Female };
            mother.SetAge(60); mother.SetTraits(new[] { "gluttonous" });
            var father = new Character(2, "Baba", new GridPos(0, 0)) { Sex = Sex.Male };
            father.SetAge(60); father.SetTraits(new[] { "hardy" });
            pop.Add(mother); pop.Add(father);
            Character baby = null;
            pop.BabyBorn += (b, m, f) => baby = b;

            Assert.True(mother.TryStartPregnancy(father.Id));
            for (int i = 0; i < 400 && baby == null; i++) pop.Tick(0.1);

            Assert.NotNull(baby);
            Assert.Equal(2, baby.TraitIds.Count);
            Assert.Contains("gluttonous", baby.TraitIds);
            Assert.Contains("hardy", baby.TraitIds);
            Assert.True(baby.Modifiers.HungerDecay > 0f && baby.Modifiers.ColdResist > 0.2f);   // etkiler işlendi
        }

        [Fact]
        public void EbeveyniOzelliksizse_CocugaRastgeleBirOzellikGelir()
        {
            var db = Db();
            var pop = new Population(seed: 3) { Content = db };
            var mother = new Character(1, "Ana", new GridPos(0, 0)) { Sex = Sex.Female };
            mother.SetAge(60);
            pop.Add(mother);
            Character baby = null;
            pop.BabyBorn += (b, m, f) => baby = b;
            mother.TryStartPregnancy(2);
            for (int i = 0; i < 400 && baby == null; i++) pop.Tick(0.1);
            Assert.NotNull(baby);
            Assert.Single(baby.TraitIds);
        }

        // ---------- Meslek dalları ----------

        [Fact]
        public void MeslekDali_SeviyeVeKademeKurallari()
        {
            var db = Db();
            db.Branches.Register(new BranchDef { Id = "farmer.gardener", Skill = SkillType.Farming, Level = 5, Effects = new LifeModifiers { YieldChance = 0.2f } });
            db.Branches.Register(new BranchDef { Id = "farmer.rancher", Skill = SkillType.Farming, Level = 5, Effects = new LifeModifiers() });
            db.Branches.Register(new BranchDef { Id = "farmer.master", Skill = SkillType.Farming, Level = 10, Requires = "farmer.gardener", Effects = new LifeModifiers { YieldChance = 0.3f } });
            var c = NewChar(1, new GridPos(0, 0));

            Assert.False(LifeRules.ChooseBranch(db, c, "farmer.gardener", out var reason));
            Assert.Contains("seviye", reason);

            c.GainSkillXp(SkillType.Farming, SkillRules.XpForLevel(5) + 1);
            Assert.True(LifeRules.ChooseBranch(db, c, "farmer.gardener", out _));
            Assert.Equal(0.2f, c.Modifiers.YieldChance, 3);
            Assert.False(LifeRules.ChooseBranch(db, c, "farmer.rancher", out reason));   // aynı kademede ikinci dal
            Assert.False(LifeRules.ChooseBranch(db, c, "farmer.master", out _));          // seviye 10 yok

            c.GainSkillXp(SkillType.Farming, SkillRules.XpForLevel(10));
            Assert.True(LifeRules.ChooseBranch(db, c, "farmer.master", out _));
            Assert.Equal(0.5f, c.Modifiers.YieldChance, 3);
            Assert.Equal(SkillType.Farming, LifeRules.DominantBranchSkill(db, new[] { c }));
        }

        // ---------- Sosyal ve eğlence ihtiyacı ----------

        [Fact]
        public void YeniIhtiyaclar_YaslilikVeBebekBakimiNiEngellemez()
        {
            var c = NewChar(1, new GridPos(0, 0));
            c.Needs.Set(NeedType.Social, 0);
            c.Needs.Set(NeedType.Fun, 0);
            Assert.False(c.Needs.AnyCritical());     // yalnızca hayatî ihtiyaçlar kritik sayılır
            Assert.False(NeedKinds.IsSurvival(NeedType.Social));
        }

        [Fact]
        public void SosyalIhtiyac_NPCyiSohbeteGonderir()
        {
            var world = new WorldState(new GridMap(12, 12));
            var db = Db();
            var pop = new Population();
            var house = new Household();
            var family = new FamilyService(pop, house, db);
            var a = NewChar(1, new GridPos(1, 1)); var b = NewChar(2, new GridPos(4, 1));
            pop.Add(a); pop.Add(b);
            a.Needs.Set(NeedType.Social, 10);
            var runner = new JobRunner(a, world) { Social = family, Autonomous = true };

            runner.Tick(0.001);
            Assert.False(runner.IsIdle);
            Assert.Equal(NeedType.Social, runner.CurrentReason);
            for (int i = 0; i < 200 && !runner.IsIdle; i++) runner.Tick(0.005);

            Assert.True(a.Needs.Get(NeedType.Social) > 50f);
            Assert.True(b.Needs.Get(NeedType.Social) > 90f);   // karşı taraf da doyar (zaten doluydu)
            Assert.True(a.Mood.Has(MoodCatalog.NiceChat));
            Assert.True(family.Affection(1, 2) > 0);
        }

        [Fact]
        public void SosyalGain_SohbetinDoygunluguCarpar()
        {
            var db = Db();
            var pop = new Population(); var house = new Household();
            var family = new FamilyService(pop, house, db);
            var a = NewChar(1, new GridPos(0, 0), "sociable"); var b = NewChar(2, new GridPos(1, 0));
            LifeRules.Recompute(a, db);
            pop.Add(a); pop.Add(b);
            a.Needs.Set(NeedType.Social, 0); b.Needs.Set(NeedType.Social, 0);
            family.Apply(a, b, SocialAction.Chat);
            Assert.True(a.Needs.Get(NeedType.Social) > b.Needs.Get(NeedType.Social));
        }

        [Fact]
        public void Otonomi_UygunIsiOlmayanIhtiyacaTakilmaz()
        {
            // En düşük ihtiyaç (Eğlence) için eşya yok; Enerji düşük ve yatak var: yatağa gider.
            var world = new WorldState(new GridMap(10, 10));
            world.PlaceItem(new ItemDef
            {
                Id = "bed", DisplayName = "Yatak",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 } }
            }, new GridPos(5, 5));
            var c = NewChar(1, new GridPos(0, 0));
            c.Needs.Set(NeedType.Fun, 2);
            c.Needs.Set(NeedType.Energy, 20);
            var runner = new JobRunner(c, world) { Autonomous = true };
            runner.Tick(0.001);
            Assert.False(runner.IsIdle);
            Assert.Equal(NeedType.Energy, runner.CurrentReason);
        }

        [Fact]
        public void Otonomi_HayatiIhtiyacSosyalOnundedir()
        {
            var world = new WorldState(new GridMap(10, 10));
            world.PlaceItem(new ItemDef
            {
                Id = "pot", DisplayName = "Kazan",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "eat", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.05 } }
            }, new GridPos(5, 5));
            var c = NewChar(1, new GridPos(0, 0));
            c.Needs.Set(NeedType.Social, 0);       // daha düşük ama hayatî değil
            c.Needs.Set(NeedType.Hunger, 25);
            var runner = new JobRunner(c, world) { Autonomous = true };
            runner.Tick(0.001);
            Assert.Equal(NeedType.Hunger, runner.CurrentReason);
        }

        [Fact]
        public void EglenceEsyasi_EkIhtiyacVeRuhHaliVerir()
        {
            var world = new WorldState(new GridMap(10, 10));
            world.PlaceItem(new ItemDef
            {
                Id = "fire", DisplayName = "Ateş", HeatOutput = 1f,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "sit", Need = NeedType.Fun, Restore = 40, DurationDays = 0.05, RequiresLit = false,
                        ExtraRestore = new List<NeedRestoreDef> { new NeedRestoreDef { Need = NeedType.Social, Amount = 20 } },
                        MoodId = "mood.fire", MoodText = "Ateş başında", MoodValue = 6, MoodDays = 1
                    }
                }
            }, new GridPos(3, 3));
            var c = NewChar(1, new GridPos(0, 0));
            c.Needs.Set(NeedType.Fun, 10); c.Needs.Set(NeedType.Social, 10);
            var runner = new JobRunner(c, world) { Autonomous = true };
            for (int i = 0; i < 200 && (i < 2 || !runner.IsIdle); i++) runner.Tick(0.005);
            Assert.True(c.Needs.Get(NeedType.Fun) >= 50f);
            Assert.True(c.Needs.Get(NeedType.Social) >= 29f);
            Assert.True(c.Mood.Has("mood.fire"));
        }

        [Fact]
        public void RequiresLit_SonukAteseOturulmaz()
        {
            var world = new WorldState(new GridMap(10, 10));
            var fire = world.PlaceItem(new ItemDef
            {
                Id = "fire", DisplayName = "Ateş", HeatOutput = 1f,
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sit", Need = NeedType.Fun, Restore = 40, DurationDays = 0.05, RequiresLit = true } }
            }, new GridPos(3, 3));
            var c = NewChar(1, new GridPos(0, 0));
            var runner = new JobRunner(c, world);
            runner.Command(Job.UseItem(fire.Id, "sit"));
            runner.Tick(0.01);
            Assert.True(runner.IsIdle);
            Assert.Contains("sönük", runner.LastFailure);
        }

        // ---------- T21: ruh hali, konfor ----------

        [Fact]
        public void Ruhhali_VarsayilanNotrdur_VeIsHiziniDegistirmez()
        {
            var c = NewChar(1, new GridPos(0, 0));
            Assert.Equal(0, c.Mood.Total);
            Assert.Equal(MoodStatus.Neutral, c.MoodStatus);
            var world = new WorldState(new GridMap(5, 5));
            var runner = new JobRunner(c, world);
            var i = new InteractionDef { Id = "x", Skill = SkillType.Farming, DurationDays = 1 };
            Assert.Equal(1.0, runner.EffectiveDuration(i), 6);
        }

        [Fact]
        public void KonforluOda_RuhHaliniArtirir_SogukVeYasDusurur()
        {
            var world = new WorldState(new GridMap(8, 8));
            // 4x4'lük kapalı oda (duvarlar), içine konforlu eşyalar.
            for (int i = 0; i < 4; i++)
            {
                world.Map.SetEdge(new Edge(1 + i, 1, EdgeAxis.Horizontal), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(1 + i, 5, EdgeAxis.Horizontal), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(1, 1 + i, EdgeAxis.Vertical), EdgeKind.Wall);
                world.Map.SetEdge(new Edge(5, 1 + i, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            world.PlaceItem(Rug("r1", 10, "koylu"), new GridPos(2, 2));
            world.PlaceItem(Rug("r2", 8, "koylu"), new GridPos(3, 2));
            world.PlaceItem(Rug("r3", 6, "koylu"), new GridPos(2, 3));
            world.PlaceItem(Rug("r4", 4, "koylu"), new GridPos(3, 3));
            var mood = new MoodService(world);

            var inside = NewChar(1, new GridPos(4, 4));
            var outside = NewChar(2, new GridPos(7, 7));
            mood.Update(new[] { inside, outside });

            Assert.True(mood.ComfortOf(world.Map.Rooms.RoomAt(inside.Position)).Total >= 28);   // 28 + uyumlu oda 5
            Assert.True(inside.Mood.Total > outside.Mood.Total);
            Assert.Equal(12, inside.Mood.Total);   // rahat ve güzel ev

            // Yas ruh halini düşürür.
            int before = inside.Mood.Total;
            var dead = NewChar(9, new GridPos(0, 0));
            inside.BirthMotherId = dead.Id;
            Assert.Equal(1, mood.Mourn(dead, new[] { inside }));
            Assert.True(inside.Mood.Total < before);
            Assert.True(inside.Mood.Has(MoodCatalog.Grief));
        }

        [Fact]
        public void RuhHali_IsHiziniEtkiler()
        {
            var world = new WorldState(new GridMap(5, 5));
            var happy = NewChar(1, new GridPos(0, 0)); var sad = NewChar(2, new GridPos(0, 0));
            happy.AddMoodlet(MoodCatalog.Rested);   // dinlenmiş: çiftçilikte hızlı
            sad.AddMoodlet(MoodCatalog.Grief);      // yas: işler %10 yavaş
            var i = new InteractionDef { Id = "x", Skill = SkillType.Farming, DurationDays = 1 };
            var rh = new JobRunner(happy, world); var rs = new JobRunner(sad, world);
            Assert.True(rh.EffectiveDuration(i) < 1.0);
            Assert.True(rs.EffectiveDuration(i) > 1.0);
            // İhtiyaç etkileşimi (uyku) ruh halinden etkilenmez.
            var sleep = new InteractionDef { Id = "s", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 };
            Assert.Equal(0.3, rs.EffectiveDuration(sleep), 6);
        }

        [Fact]
        public void IsHizi_OzellikAletVeHavaCarpanlariBirlesir()
        {
            var db = new ContentDatabase();
            db.Traits.Register(new TraitDef { Id = "diligent", DisplayName = "Çalışkan", Effects = new LifeModifiers { WorkSpeed = 1.25f } });
            var c = NewChar(1, new GridPos(0, 0), "diligent");
            LifeRules.Recompute(c, db);
            var runner = new JobRunner(c, new WorldState(new GridMap(5, 5)));
            var chop = new InteractionDef { Id = "c", Skill = SkillType.Woodcutting, DurationDays = 1 };
            Assert.Equal(0.8, runner.EffectiveDuration(chop), 6);
            runner.SpeedProvider = (ch, inter) => 0.5;   // fırtına
            Assert.Equal(1.6, runner.EffectiveDuration(chop), 6);
        }

        // ---------- 18e: mezar ve yas ----------

        [Fact]
        public void Olen_MezarBirakir_YakinlarYastaOlur()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pop = new Population(seed: 2);
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var notices = new List<string>();
            life.Notice += notices.Add;

            var father = new Character(1, "Baba", new GridPos(5, 5)) { Sex = Sex.Male }; father.SetAge(60); father.LifespanDays = 61;
            var son = new Character(2, "Oğul", new GridPos(6, 5)) { Sex = Sex.Male, BirthFatherId = 1, MoodEnabled = true }; son.SetAge(40);
            var stranger = new Character(3, "Yabancı", new GridPos(7, 5)) { MoodEnabled = true }; stranger.SetAge(40);
            pop.Add(father); pop.Add(son); pop.Add(stranger);

            for (int i = 0; i < 20 && !father.IsDead; i++) pop.Tick(0.1);
            Assert.True(father.IsDead);

            var grave = life.Graves.FindFor(1);
            Assert.NotNull(grave);
            Assert.True(grave.ItemId > 0);
            Assert.True(life.Graves.IsGrave(world.GetItem(grave.ItemId)));
            Assert.True(son.Mood.Has(MoodCatalog.Grief));
            Assert.False(stranger.Mood.Has(MoodCatalog.Grief));
            Assert.Contains(notices, n => n.Contains("Baba"));

            // Mezar taşınmaz ve satılmaz.
            var build = new BuildService(world, house, db);
            Assert.False(build.SellPlaced(grave.ItemId).Ok);
            Assert.False(build.StoreToInventory(grave.ItemId).Ok);
            Assert.False(build.Move(grave.ItemId, new GridPos(0, 0), 0).Ok);
        }

        [Fact]
        public void Mezar_KayittaGeriGelir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household();
            var pop = new Population(seed: 2);
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var c = new Character(1, "Ata", new GridPos(5, 5), true) { MoodEnabled = true }; c.SetAge(60); c.LifespanDays = 60.5;
            c.SetTraits(new[] { "hardy", "cheerful" });
            c.Needs.Set(NeedType.Social, 33); c.Needs.Set(NeedType.Fun, 44);
            c.AddMoodlet("mood.test", "deneme", 7, 2.0);
            var other = new Character(2, "Eş", new GridPos(2, 2)); other.SetAge(30);
            pop.Add(c); pop.Add(other);
            for (int i = 0; i < 20 && !c.IsDead; i++) pop.Tick(0.1);
            Assert.True(c.IsDead);
            float socialBefore = c.Needs.Get(NeedType.Social);

            var data = SaveGame.Capture(clock, world, house, pop.Members, new Thatchwick.Core.Progress.AccountProgress(), null, null, null, null, null, life);
            var json = data.life.graves.Length;
            Assert.Equal(1, json);

            var loaded = SaveGame.Load(data, db);
            var chars = loaded.Characters;
            var ata = chars.First(x => x.Id == 1);
            Assert.True(ata.HasTrait("hardy") && ata.HasTrait("cheerful"));
            Assert.Equal(0.3f, ata.Modifiers.ColdResist, 3);          // etkiler yüklemede yeniden işlendi
            Assert.Equal(socialBefore, ata.Needs.Get(NeedType.Social), 1);
            Assert.True(ata.Mood.Has("mood.test"));

            var pop2 = new Population();
            foreach (var x in chars) pop2.Add(x);
            var life2 = new LifeSystems(db, loaded.World, loaded.Household, pop2, loaded.Clock);
            life2.Restore(loaded.Life);
            var g = life2.Graves.FindFor(1);
            Assert.NotNull(g);
            Assert.True(life2.Graves.IsGrave(loaded.World.GetItem(g.ItemId)));
        }

        [Fact]
        public void EskiKayit_YeniAlanlarOlmadanYuklenir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(6, 6));
            var c = new Character(1, "Eski", new GridPos(1, 1), true); c.SetAge(30);
            var data = SaveGame.Capture(new GameClock(), world, new Household(), new[] { c }, new Thatchwick.Core.Progress.AccountProgress(), null, null);
            // Eski dosya: yeni alanlar yok.
            data.life = null;
            data.version = 1;   // v1 kaydı: keyif/hijyen Dalga 1 geçişiyle 100'e çekilir
            data.characters[0].social = -1f; data.characters[0].fun = -1f;
            data.characters[0].traits = null; data.characters[0].moodlets = null;
            var loaded = SaveGame.Load(data, db);
            var ch = loaded.Characters[0];
            Assert.Equal(100f, ch.Needs.Get(NeedType.Social));
            Assert.Equal(100f, ch.Needs.Get(NeedType.Fun));
            Assert.Empty(ch.TraitIds);
            Assert.Null(loaded.Life);
        }
    }
}
