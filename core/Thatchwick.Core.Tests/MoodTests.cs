using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Keyif ve hijyen, moodlet'ler, ruh hali basamakları ve becerilere etkisi, oda uyumu (Dalga 1 kararları).
    public class MoodTests
    {
        private static Character Adult(int id = 1, string name = "Wat")
        {
            var c = new Character(id, name, new GridPos(1, 1)) { MoodEnabled = true };
            c.SetAge(30);
            return c;
        }

        [Fact]
        public void KeyifVeHijyen_YalnizcaRuhHaliAcikkenAzalir()
        {
            var off = new Character(1, "A", new GridPos(0, 0));
            off.SetAge(30);
            off.Tick(1.0);
            Assert.Equal(100.0, (double)off.Needs.Get(NeedType.Fun), 3);
            Assert.Equal(100.0, (double)off.Needs.Get(NeedType.Hygiene), 3);

            var on = Adult();
            on.Tick(1.0);
            Assert.Equal(50.0, (double)on.Needs.Get(NeedType.Fun), 3);
            Assert.Equal(65.0, (double)on.Needs.Get(NeedType.Hygiene), 3);

            var baby = new Character(3, "Bebek", new GridPos(0, 0)) { MoodEnabled = true };
            baby.Tick(1.0);
            Assert.Equal(100.0, (double)baby.Needs.Get(NeedType.Fun), 3);
        }

        [Fact]
        public void KonforIhtiyaclari_KritikSayilmaz()
        {
            var c = Adult();
            c.Needs.Set(NeedType.Fun, 0f);
            c.Needs.Set(NeedType.Hygiene, 0f);
            Assert.False(c.Needs.AnyCritical());
            c.Needs.Set(NeedType.Hunger, 5f);
            Assert.True(c.Needs.AnyCritical());
        }

        [Fact]
        public void IhtiyacMoodletleri_EsikteAcilirVeKapanir()
        {
            var c = Adult();
            c.Needs.Set(NeedType.Fun, 10f);
            c.Needs.Set(NeedType.Hunger, 10f);
            c.Tick(0.001);
            Assert.True(c.Mood.Has(MoodCatalog.Bored));
            Assert.True(c.Mood.Has(MoodCatalog.Hungry));
            Assert.Equal(-25, c.Mood.Total);
            Assert.Equal(MoodStatus.Unhappy, c.Mood.Status);

            c.Needs.Set(NeedType.Fun, 90f);
            c.Needs.Set(NeedType.Hunger, 90f);
            c.Tick(0.001);
            Assert.False(c.Mood.Has(MoodCatalog.Bored));
            Assert.False(c.Mood.Has(MoodCatalog.Hungry));
            Assert.Equal(MoodStatus.Neutral, c.Mood.Status);
        }

        [Fact]
        public void OlayMoodleti_SuresiBitinceKalkar_AyniMoodletYiginlanmaz()
        {
            var c = Adult();
            Assert.True(c.AddMoodlet(MoodCatalog.Rested));
            c.AddMoodlet(MoodCatalog.Rested);
            Assert.Single(c.Mood.Active);
            Assert.Equal(10, c.Mood.Total);
            c.Tick(0.5);
            Assert.True(c.Mood.Has(MoodCatalog.Rested));
            c.Tick(0.2);
            Assert.False(c.Mood.Has(MoodCatalog.Rested));
        }

        [Fact]
        public void RuhHaliKapaliysa_MoodletEklenmez()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            Assert.False(c.AddMoodlet(MoodCatalog.Rested));
            Assert.Empty(c.Mood.Active);
        }

        [Fact]
        public void Basamaklar_ToplamDegereGore()
        {
            Assert.Equal(MoodStatus.Miserable, MoodRules.StatusFor(-40));
            Assert.Equal(MoodStatus.Unhappy, MoodRules.StatusFor(-10));
            Assert.Equal(MoodStatus.Neutral, MoodRules.StatusFor(9));
            Assert.Equal(MoodStatus.Good, MoodRules.StatusFor(10));
            Assert.Equal(MoodStatus.Happy, MoodRules.StatusFor(30));
        }

        [Fact]
        public void IlhamDolu_MarangozlukXpsiniArtirir_DigerBecerileriEtkilemez()
        {
            var c = Adult();
            c.AddMoodlet(MoodCatalog.Inspired);   // +15 → İyi (x1,1), marangozluk +%30
            Assert.Equal(MoodStatus.Good, c.Mood.Status);
            double carpentry = c.GainSkillXp(SkillType.Carpentry, 10);
            double farming = c.GainSkillXp(SkillType.Farming, 10);
            Assert.Equal(10 * 1.1 * 1.3, carpentry, 6);
            Assert.Equal(10 * 1.1, farming, 6);
        }

        [Fact]
        public void Yas_XpVeHiziDusurur()
        {
            var c = Adult();
            c.AddMoodlet(MoodCatalog.Grief);   // -30 → Mutsuz (x0,85), tüm XP -%20, hız -%10
            Assert.Equal(MoodStatus.Unhappy, c.Mood.Status);
            Assert.Equal(10 * 0.85 * 0.8, c.GainSkillXp(SkillType.Woodcutting, 10), 6);
            Assert.Equal(0.9 * 0.9, c.WorkSpeed(SkillType.Woodcutting), 6);
        }

        [Fact]
        public void Dinlenmis_OdunculukIsiniKisaltir_UykuyuEtkilemez()
        {
            var world = new WorldState(new GridMap(10, 10));
            var c = Adult();
            var runner = new JobRunner(c, world) { Autonomous = false };
            var chop = new InteractionDef { Id = "chop", DisplayName = "Kes", DurationDays = 0.2, Skill = SkillType.Woodcutting, SkillXp = 5 };
            var sleep = new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 };
            double before = runner.EffectiveDuration(chop);
            c.AddMoodlet(MoodCatalog.Rested);   // +10 → İyi (x1,05) ve odunculuk +%15
            Assert.Equal(before / (1.05 * 1.15), runner.EffectiveDuration(chop), 6);
            Assert.Equal(0.3, runner.EffectiveDuration(sleep), 6);
        }

        [Fact]
        public void Etkilesim_BitinceMoodletVerir_VeKeyifDoldurur()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef
            {
                Id = "board", DisplayName = "Değirmen oyunu",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "play", DisplayName = "Oyna", Need = NeedType.Fun, Restore = 40, DurationDays = 0.1, Moodlet = MoodCatalog.HadFun }
                }
            });
            Assert.Empty(db.Validate());
            var world = new WorldState(new GridMap(10, 10));
            var board = world.PlaceItem(db.Items.Get("board"), new GridPos(3, 1));
            var c = Adult();
            c.Needs.Set(NeedType.Fun, 20f);
            var runner = new JobRunner(c, world) { Autonomous = true };
            runner.Tick(0.3);   // otonom: keyif eşiğin altında, oyun tahtasına gider
            Assert.True(c.Mood.Has(MoodCatalog.HadFun));
            Assert.True(c.Needs.Get(NeedType.Fun) > 50f);
        }

        [Fact]
        public void Otonomi_EnDusukIhtiyacinEsyasiYoksa_SiradakineGecer()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef
            {
                Id = "bed", DisplayName = "Yatak",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 } }
            });
            var world = new WorldState(new GridMap(10, 10));
            world.PlaceItem(db.Items.Get("bed"), new GridPos(4, 1));
            var c = Adult();
            c.Needs.Set(NeedType.Fun, 5f);      // en düşük ama keyif eşyası yok
            c.Needs.Set(NeedType.Energy, 20f);
            var runner = new JobRunner(c, world) { Autonomous = true };
            runner.Tick(0.4);
            Assert.True(c.Needs.Get(NeedType.Energy) > 60f);
        }

        [Fact]
        public void ContentDogrulama_BilinmeyenMoodlet()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef
            {
                Id = "x", DisplayName = "X",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "i", DisplayName = "I", Need = NeedType.Fun, Restore = 5, DurationDays = 0.1, Moodlet = "yok_boyle" } }
            });
            Assert.Contains(db.Validate(), e => e.Contains("moodlet"));
        }

        // ---- oda uyumu ----

        private static (WorldState world, ContentDatabase db) Room()
        {
            var map = new GridMap(10, 10);
            // 1..4 x 1..4 kapalı oda
            for (int i = 1; i <= 4; i++)
            {
                map.SetEdge(new Edge(i, 1, EdgeAxis.Horizontal), EdgeKind.Wall);
                map.SetEdge(new Edge(i, 5, EdgeAxis.Horizontal), EdgeKind.Wall);
                map.SetEdge(new Edge(1, i, EdgeAxis.Vertical), EdgeKind.Wall);
                map.SetEdge(new Edge(5, i, EdgeAxis.Vertical), EdgeKind.Wall);
            }
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "stool", DisplayName = "Tabure", StyleSet = "Köylü" });
            db.Items.Register(new ItemDef { Id = "chair", DisplayName = "Sandalye", StyleSet = "Zanaatkâr" });
            db.Items.Register(new ItemDef { Id = "candle", DisplayName = "Kandil", Tags = new List<string> { "light" } });
            return (new WorldState(map), db);
        }

        [Fact]
        public void OdaUyumu_AyniSettenDortEsya_UyumluEvVerir()
        {
            var (world, db) = Room();
            var c = Adult();
            c.Position = new GridPos(4, 4);
            var sys = new RoomMoodSystem(world);
            world.PlaceItem(db.Items.Get("stool"), new GridPos(1, 1));
            world.PlaceItem(db.Items.Get("stool"), new GridPos(2, 1));
            world.PlaceItem(db.Items.Get("stool"), new GridPos(3, 1));
            world.PlaceItem(db.Items.Get("chair"), new GridPos(4, 1));
            sys.Tick(0.1, new[] { c });
            Assert.False(c.Mood.Has(MoodCatalog.Harmony));   // 3 Köylü + 1 Zanaatkâr: yetmez

            world.PlaceItem(db.Items.Get("stool"), new GridPos(1, 2));
            sys.Tick(0.1, new[] { c });
            Assert.True(c.Mood.Has(MoodCatalog.Harmony));
            Assert.Equal("Köylü", RoomMoodSystem.HarmonySet(world.Map.Rooms.RoomAt(c.Position), world));
        }

        [Fact]
        public void OdaUyumu_DisaridaVeriLmez_IsikAydinlikOdaVerir()
        {
            var (world, db) = Room();
            var c = Adult();
            c.Position = new GridPos(8, 8);   // dışarıda
            for (int i = 1; i <= 4; i++) world.PlaceItem(db.Items.Get("stool"), new GridPos(i, 1));
            world.PlaceItem(db.Items.Get("candle"), new GridPos(1, 3));
            var sys = new RoomMoodSystem(world);
            sys.Tick(0.1, new[] { c });
            Assert.Empty(c.Mood.Active);

            c.Position = new GridPos(3, 3);
            sys.Tick(0.1, new[] { c });
            Assert.True(c.Mood.Has(MoodCatalog.Harmony));
            Assert.True(c.Mood.Has(MoodCatalog.BrightRoom));
        }

        // ---- gerçek içerikle denge ----

        /// <summary>
        /// items.json'daki başlangıç eşyalarıyla (saman yatak, kazan, leğen, masa üstünde değirmen oyunu)
        /// ruh hali açık iki yetişkin 6 gün boyunca kendi başına yaşar: aç kalmaz, keyif ve hijyeni
        /// toparlar, ruh hali Perişan'a düşmez. Keyif/hijyen azalma hızları ile eşya değerlerinin dengesi.
        /// </summary>
        [Fact]
        public void GercekIcerik_RuhHaliAcikHane_AltiGunIyiYasar()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            var world = new WorldState(new GridMap(20, 20));
            var house = new Build.Household(0);
            house.AddItem("item.turnip", 80);
            world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(2, 2));
            world.PlaceItem(db.Items.Get("item.mattress_straw"), new GridPos(2, 4));
            world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(6, 2));
            world.PlaceItem(db.Items.Get("item.basin_ewer"), new GridPos(6, 5));
            var table = world.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(8, 7));
            world.PlaceOnSurface(db.Items.Get("item.morris_board"), table.Id, "b");
            var effects = new Work.InteractionEffects(world, house, db);

            var people = new List<Character>();
            var runners = new List<JobRunner>();
            for (int i = 0; i < 2; i++)
            {
                var c = Adult(i + 1, i == 0 ? "Wat" : "Agnes");
                c.CanStarve = true;
                c.Position = new GridPos(4 + i, 8);
                people.Add(c);
                runners.Add(new JobRunner(c, world) { Autonomous = true, Effects = effects });
            }

            double funSum = 0; int samples = 0;
            for (int step = 0; step < 600; step++)
            {
                for (int i = 0; i < people.Count; i++)
                {
                    people[i].Tick(0.01);
                    runners[i].Tick(0.01);
                }
                if (step >= 100)
                    foreach (var c in people) { funSum += c.Needs.Get(NeedType.Fun); samples++; }
            }
            foreach (var c in people)
            {
                Assert.False(c.IsDead);
                Assert.NotEqual(MoodStatus.Miserable, c.Mood.Status);
            }
            Assert.True(funSum / samples > 25.0, $"ortalama keyif {funSum / samples:0.0}");
            Assert.True(house.Count("item.turnip") < 80);
        }

        // ---- aile olayları ----

        [Fact]
        public void Olum_YakinlaraYasVerir_UzaklaraVermez()
        {
            var father = Adult(1, "Wat");
            var mother = Adult(2, "Agnes");
            father.SpouseId = 2; mother.SpouseId = 1;
            var child = Adult(3, "Tom"); child.BirthMotherId = 2; child.BirthFatherId = 1;
            var sibling = Adult(4, "Ann"); sibling.BirthMotherId = 2;
            var stranger = Adult(5, "Hugh");
            int n = MoodEvents.OnDeath(father, new[] { father, mother, child, sibling, stranger });
            Assert.Equal(2, n);   // eş ve çocuk; kardeş Ann babanın çocuğu değil (yalnızca annesi ortak)
            Assert.True(mother.Mood.Has(MoodCatalog.Grief));
            Assert.True(child.Mood.Has(MoodCatalog.Grief));
            Assert.False(sibling.Mood.Has(MoodCatalog.Grief));
            Assert.False(stranger.Mood.Has(MoodCatalog.Grief));

            MoodEvents.OnDeath(child, new[] { mother, sibling });
            Assert.True(sibling.Mood.Has(MoodCatalog.Grief));   // anne bir kardeş
        }

        [Fact]
        public void Sohbet_KeyifVeMoodletVerir_Evlilik_YeniEvli()
        {
            var a = Adult(1, "Wat"); a.Sex = Sex.Male;
            var b = Adult(2, "Agnes"); b.Sex = Sex.Female;
            a.Needs.Set(NeedType.Fun, 20f);
            var pop = new Population();
            pop.Add(a); pop.Add(b);
            var fam = new FamilyService(pop, new Build.Household(0), new ContentDatabase());
            fam.Apply(a, b, SocialAction.Chat);
            Assert.Equal(45.0, (double)a.Needs.Get(NeedType.Fun), 3);
            Assert.True(a.Mood.Has(MoodCatalog.NiceChat));
            Assert.True(b.Mood.Has(MoodCatalog.NiceChat));

            fam.SetAffection(1, 2, 100);
            fam.Apply(a, b, SocialAction.Propose);
            Assert.True(a.Mood.Has(MoodCatalog.Newlywed));
            Assert.True(b.Mood.Has(MoodCatalog.Newlywed));
        }

        [Fact]
        public void KirliyleSohbet_YakinligiYariArtirir()
        {
            var a = Adult(1, "Wat"); a.Sex = Sex.Male;
            var b = Adult(2, "Agnes"); b.Sex = Sex.Female;
            a.Needs.Set(NeedType.Hygiene, 5f);
            a.Tick(0.001);
            Assert.True(a.Mood.Has(MoodCatalog.Dirty));
            var pop = new Population();
            pop.Add(a); pop.Add(b);
            var fam = new FamilyService(pop, new Build.Household(0), new ContentDatabase());
            fam.Apply(a, b, SocialAction.Chat);
            Assert.Equal(FamilyService.ChatAffection / 2, fam.Affection(1, 2));
        }
    }
}
