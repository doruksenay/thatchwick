using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ev bakımı ve otomasyonu (Project belgesi 45): mum, ışık, is ve badana, ot serpme, canlı saksı, yükseltmeler,
    // oda türleri, ev görevi, hizmetkâr, desen ve arma, koleksiyon, oda planı, mevsim seti, ev yarışması, gömme niş.
    public class EvBakimiTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public GameClock Clock;
            public BuildService Build;
            public InteractionEffects Effects;
            public HomeLife Home;
            public MoodService Mood;
            public List<Character> Members = new List<Character>();
            public List<string> Notices = new List<string>();
            public ItemDef D(string id) => Db.Items.Get(id);
            public Room Room => World.Map.Rooms.RoomAt(new GridPos(3, 3));
        }

        private static Kit Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(20, 20)), House = new Household(5000), Clock = new GameClock() };
            k.Build = new BuildService(k.World, k.House, db);
            k.Effects = new InteractionEffects(k.World, k.House, db);
            k.Mood = new MoodService(k.World) { Content = db };
            k.Home = new HomeLife(k.World, db, k.House, k.Clock)
            {
                Members = () => k.Members, Notify = (t, _) => k.Notices.Add(t), Effects = k.Effects, Build = k.Build, BeautyOf = k.Mood.BeautyOf
            };
            k.Effects.ExtraBlocker = k.Home.CanStart;
            k.Effects.AppliedOn += k.Home.OnApplied;
            k.Mood.RoomBeautyExtra = k.Home.RoomBeautyExtra;
            k.Mood.BeautyBonus = k.Home.BeautyBonus;
            // 2..7 x 2..5 iç oda, duvarlarla kapalı.
            for (int x = 2; x <= 7; x++) { Wall(k, x, 2, EdgeAxis.Horizontal); Wall(k, x, 6, EdgeAxis.Horizontal); }
            for (int y = 2; y <= 5; y++) { Wall(k, 2, y, EdgeAxis.Vertical); Wall(k, 8, y, EdgeAxis.Vertical); }
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 5; y++) k.World.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
            return k;
        }

        private static void Wall(Kit k, int x, int y, EdgeAxis axis, string style = null) => k.World.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall, style);
        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);
        private static Character Person(int id = 1, string name = "Wat")
        {
            var c = new Character(id, name, new GridPos(3, 3)) { MoodEnabled = true };
            c.SetAge(30);
            return c;
        }
        private static void At(Kit k, double totalDays) => k.Clock.Restore(totalDays, 1);

        // ---------- mum ve ışık ----------

        [Fact]
        public void Mum_YeniDoluGelir_AksamYanarBiter_DegistirilirEnvanterdenMumAlir()
        {
            var k = Make();
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var candle = k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            Assert.True(candle.GivesLight);
            Assert.Equal(candle.Def.BurnDays, candle.FuelDays, 6);

            At(k, 0.5);                       // öğlen yanmaz
            k.Home.Tick(0.5);
            Assert.Equal(candle.Def.BurnDays, candle.FuelDays, 6);
            At(k, 0.8);                       // akşam yanar
            for (int i = 0; i < 20; i++) k.Home.Tick(0.1);
            Assert.True(candle.BurntOut);
            Assert.False(candle.GivesLight);

            var change = Inter(candle.Def, "replace_candle");
            var c = Person();
            Assert.Contains("mum yok", k.Effects.CanStart(c, candle, change));
            Assert.Contains(k.Home.Chores(), ch => ch.item == candle && ch.interaction == change);
            k.House.AddItem("item.tallow_candle", 1);
            Assert.Null(k.Effects.CanStart(c, candle, change));
            k.Effects.Apply(c, candle, change);
            Assert.True(candle.GivesLight);
            Assert.Equal(0, k.House.Count("item.tallow_candle"));
            Assert.Contains("Mum değiştir", Inter(candle.Def, "replace_candle").DisplayName);
        }

        [Fact]
        public void Isik_PuaniVeBasamagi_GeceIncelIsKaranliktaYavas()
        {
            var k = Make();
            var room = k.Room;
            Assert.Equal(0, k.Home.LightLevel(room));
            var loom = k.World.PlaceItem(k.D("item.loom"), new GridPos(6, 4));
            var craft = loom.Def.Interactions.First(i => i.IsCraft);
            At(k, 0.9);                                                           // gece
            Assert.Equal(0.8, k.Home.BonusFor(loom, craft).Speed, 6);
            k.World.PlaceItem(k.D("item.iron_chandelier"), new GridPos(3, 3));   // avize 4
            Assert.Equal(4, k.Home.LightPoints(room));
            Assert.Equal(1, k.Home.LightLevel(room));                             // 24 karoda 4 puan: loş
            Assert.Equal(0.9, k.Home.BonusFor(loom, craft).Speed, 6);
            At(k, 0.5);                                                           // gündüz ışığa bakılmaz
            Assert.Equal(1.0, k.Home.BonusFor(loom, craft).Speed, 6);
        }

        // ---------- is, badana, baca ----------

        [Fact]
        public void Is_BacasizOcakOdayiKarartir_BadanaSiler_BacaAzaltir()
        {
            var k = Make();
            var heat = new HeatSystem(k.World, k.Clock.Config);
            var hearth = k.World.PlaceItem(k.D("item.hearth_stone"), new GridPos(4, 3));
            k.House.AddItem("item.firewood", 20);
            for (int i = 0; i < 6; i++) heat.TryStoke(hearth.Id, k.House);
            Assert.True(hearth.IsLit);
            int before = k.Mood.BeautyOf(k.Room).Raw;
            for (int i = 0; i < 9; i++) k.Home.Tick(1.0);
            Assert.True(k.Home.SootOf(k.Room) >= HomeLife.SootWarnAt);
            Assert.Contains(k.Notices, n => n.Contains("isle karardı"));
            Assert.True(k.Mood.BeautyOf(k.Room).Raw < before);

            var wash = Inter(hearth.Def, "whitewash");
            var c = Person();
            Assert.Contains(k.Home.Chores(), ch => ch.interaction == wash);
            Assert.NotNull(k.Effects.CanStart(c, hearth, wash));                  // kireç yok
            k.House.AddItem("item.lime", 2);
            Assert.Null(k.Effects.CanStart(c, hearth, wash));
            k.Effects.Apply(c, hearth, wash);
            Assert.Equal(0, k.Home.SootOf(k.Room), 6);
            Assert.True(k.Home.IsWhitewashed(k.Room));
            Assert.True(k.Mood.BeautyOf(k.Room).Raw > before);                    // ak badana +3
            Assert.True(k.Home.WashedWithin(1));

            // Baca: is dörtte bir.
            var hood = Inter(hearth.Def, "upgrade:hood");
            k.House.AddItem("item.stone", 6); k.House.AddItem("item.lime", 2); k.House.AddItem("item.plank", 2);
            c.GainSkillXp(SkillType.Masonry, SkillRules.XpForLevel(2));
            Assert.Null(k.Effects.CanStart(c, hearth, hood));
            k.Effects.Apply(c, hearth, hood);
            Assert.True(k.Home.HasChimney(hearth));
            k.Home.Tick(2.0);
            Assert.Equal(2.0 * HomeLife.SootPerDay * HomeLife.ChimneySootFactor, k.Home.SootOf(k.Room), 6);
            Assert.True(k.Home.HasChimney(k.World.PlaceItem(k.D("item.fireplace_large"), new GridPos(12, 12))));   // büyük şöminenin bacası kendinden
        }

        // ---------- ot serpme ve canlı saksı ----------

        [Fact]
        public void OtSerpme_HasiraOtSerpilir_FerahOda_UcGunSonraBayat()
        {
            var k = Make();
            var mat = k.World.PlaceItem(k.D("item.rush_mat_large"), new GridPos(3, 3));
            var strew = Inter(mat.Def, "strew");
            var c = Person();
            k.Members.Add(c);
            Assert.Contains("ot yok", k.Effects.CanStart(c, mat, strew));
            k.House.AddItem("item.mint", 1);
            int before = k.Mood.BeautyOf(k.Room).Home;
            k.Effects.Apply(c, mat, strew);
            Assert.Equal(0, k.House.Count("item.mint"));
            Assert.True(k.Home.IsStrewnFresh(mat));
            Assert.Equal(before + HomeLife.FreshRoomBeauty, k.Mood.BeautyOf(k.Room).Home);
            k.Home.Tick(0.06);
            Assert.True(c.Mood.Has(HomeLife.FreshRoomMoodId));
            Assert.Contains("hâlâ taze", k.Effects.CanStart(c, mat, strew));
            At(k, 3.5);
            Assert.False(k.Home.IsStrewnFresh(mat));
            Assert.True(k.Home.IsStrewnStale(mat));
        }

        [Fact]
        public void CanliSaksi_SulaninGunesteBuyur_OtVerir_SulanmazsaSolar()
        {
            var k = Make();
            k.World.Map.SetEdge(new Edge(5, 2, EdgeAxis.Horizontal), EdgeKind.Window);
            var sill = k.World.PlaceOnWall(k.D("item.window_sill_board"), new GridPos(5, 2), 0);
            var pot = k.World.PlaceOnSurface(k.D("item.herb_pot_basil"), sill.Id, sill.Def.SurfaceSlots[0].Id);
            Assert.True(k.Home.Sunlit(pot));
            var c = Person();
            var water = Inter(pot.Def, "water_pot");
            var harvest = Inter(pot.Def, "harvest_pot");
            Assert.Null(k.Effects.CanStart(c, pot, water));
            k.Effects.Apply(c, pot, water);
            for (int day = 0; day < 3; day++) { k.Home.Tick(0.5); k.Effects.Apply(c, pot, water); k.Home.Tick(0.5); }
            Assert.True(k.Home.PotReady(pot), k.Home.PotLabel(pot));
            k.Effects.Apply(c, pot, harvest);
            Assert.Equal(1, k.House.Count("item.herb"));
            k.Home.Tick(3.5);
            Assert.True(k.Home.PotWilted(pot));
            Assert.Equal(HomeLife.WiltedPotBeauty - pot.Def.Beauty, k.Home.BeautyBonus(pot), 6);
            Assert.Contains("solmuş", k.Effects.CanStart(c, pot, harvest));
        }

        // ---------- yükseltmeler ----------

        [Fact]
        public void Yukseltme_SiltZinciri_DemirKusakDepo_CilaCokmeyiAzaltir()
        {
            var k = Make();
            var bed = k.World.PlaceItem(k.D("item.bed_frame"), new GridPos(3, 4));
            var c = Person();
            c.GainSkillXp(SkillType.Weaving, SkillRules.XpForLevel(4));
            var wool = Inter(bed.Def, "upgrade:mattress_wool");
            var feather = Inter(bed.Def, "upgrade:mattress_feather");
            Assert.Contains("Önce", k.Home.UpgradeBlocker(bed, "mattress_feather"));
            k.House.AddItem("item.wool", 4); k.House.AddItem("item.linen", 3); k.House.AddItem("item.down_feather", 6);
            k.Effects.Apply(c, bed, wool);
            Assert.Equal(2, k.Home.ComfortBonus(bed));
            Assert.Null(k.Effects.CanStart(c, bed, feather));
            k.Effects.Apply(c, bed, feather);
            Assert.Equal(new[] { "mattress_feather" }, k.Home.UpgradesOf(bed.Id).ToArray());   // yün şiltenin yerine geçti
            Assert.Equal(4, k.Home.ComfortBonus(bed));
            Assert.Contains("Daha iyisi", k.Home.UpgradeBlocker(bed, "mattress_wool"));
            var sleep = bed.Def.Interactions.First(i => i.Need == NeedType.Energy);
            Assert.Equal(1.12, k.Home.BonusFor(bed, sleep).Restore, 6);

            var chest = k.World.PlaceItem(k.D("item.chest_wood"), new GridPos(6, 5));
            var storage = new StorageService(k.World, k.House, k.Db) { ExtraStorage = k.Home.ExtraStorage };
            int cap = storage.Capacity;
            k.Home.ApplyUpgrade(chest, "iron_bands");
            Assert.Equal(cap + 20, storage.Capacity);

            var shelf = k.World.PlaceItem(k.D("item.shelf_wood"), new GridPos(4, 5));
            Assert.Equal(1.0, k.Home.CollapseFactor(shelf), 6);
            k.Home.ApplyUpgrade(shelf, "wax_polish");
            Assert.Equal(0.5, k.Home.CollapseFactor(shelf), 6);
            Assert.Contains("Yükseltilir", ItemCatalog.Describe(k.D("item.bed_frame")));
        }

        // ---------- oda türleri ----------

        [Fact]
        public void OdaTuru_Atolye_DuzenliOlunca_ZanaatHizli_Ipucu()
        {
            var k = Make();
            var bench = k.World.PlaceItem(k.D("item.workbench"), new GridPos(4, 4));
            var room = k.Room;
            var (kind, tidy) = k.Home.Classify(room);
            Assert.Equal(RoomKind.Workshop, kind);
            Assert.False(tidy);
            Assert.NotNull(k.Home.TidyHint(room));
            k.World.PlaceOnWall(k.D("item.tool_rack_wall"), new GridPos(3, 2), 0);
            k.World.PlaceOnWall(k.D("item.wall_shelf"), new GridPos(4, 2), 0);
            k.World.PlaceItem(k.D("item.hanging_lantern"), new GridPos(6, 3));
            (kind, tidy) = k.Home.Classify(room);
            Assert.True(tidy, k.Home.TidyHint(room));
            var craft = bench.Def.Interactions.First(i => i.IsCraft && i.Skill == SkillType.Carpentry);
            At(k, 0.5);
            Assert.Equal(HomeLife.TidySpeed, k.Home.BonusFor(bench, craft).Speed, 6);
            Assert.Contains("Atölye · düzenli", k.Home.RoomSummary(room));

            var k2 = Make();
            k2.World.PlaceItem(k2.D("item.bed_frame"), new GridPos(3, 4));
            Assert.Equal(RoomKind.Bedroom, k2.Home.Classify(k2.Room).kind);
        }

        [Fact]
        public void OdaTuru_Sera_AitEsyaGuzellik_BakimliSeraBuyumeHizli()
        {
            var k = Make();
            var room = k.Room;
            k.Home.IsGreenhouseRoom = r => r == room;   // pencere koşulu PlantSystem'de ayrıca sınanır
            k.World.PlaceItem(k.D("item.pot_bay"), new GridPos(3, 4));
            var (kind, tidy) = k.Home.Classify(room);
            Assert.Equal(RoomKind.Greenhouse, kind);
            Assert.False(tidy);
            Assert.Contains("fidan rafı", k.Home.TidyHint(room));
            Assert.Equal(1, k.Home.GreenhouseBeauty(room));
            Assert.Equal(1.0, k.Home.GreenhouseGrowth(room), 6);

            k.World.PlaceItem(k.D("item.seedling_rack"), new GridPos(4, 4));
            k.World.PlaceItem(k.D("item.pot_myrtle"), new GridPos(5, 4));
            (kind, tidy) = k.Home.Classify(room);
            Assert.True(tidy, k.Home.TidyHint(room));
            Assert.Equal(3, k.Home.GreenhouseBeauty(room));
            At(k, 0.5);
            Assert.Equal(HomeLife.TidySpeed, k.Home.GreenhouseGrowth(room), 6);
            Assert.StartsWith("Sera · düzenli", k.Home.RoomSummary(room));
            Assert.True(k.Home.RoomBeautyExtra(room).plus >= 3);

            // Sera koşulu (pencere ve ışık) yoksa saksılı oda oturma odasıdır.
            var k2 = Make();
            k2.Home.IsGreenhouseRoom = r => false;
            k2.World.PlaceItem(k2.D("item.pot_bay"), new GridPos(3, 4));
            Assert.Equal(RoomKind.Living, k2.Home.Classify(k2.Room).kind);
        }

        // ---------- ev görevi ve hizmetkâr ----------

        [Fact]
        public void EvGorevi_PlanlayiciBakimIsiVerir()
        {
            var k = Make();
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var candle = k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            candle.SetCandle(0);
            k.House.AddItem("item.candle", 1);
            var duties = new DutyPlanner(k.World, null, k.Effects, null) { HouseChores = k.Home.Chores };
            var c = Person();
            c.Duty = DutyKind.House;
            Assert.Equal(1, duties.Pending(DutyKind.House));
            var job = duties.Next(c);
            Assert.NotNull(job);
            Assert.Equal("Ev", DutyPlanner.DutyName(DutyKind.House));
        }

        [Fact]
        public void Hizmetkar_UcretAlir_SabahIsleriniGorur_OdenmezseGider()
        {
            var k = Make();
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var candle = k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            candle.SetCandle(0);
            k.House.AddItem("item.candle", 2);
            var debris = k.World.PlaceItem(k.D("item.debris"), new GridPos(5, 5));
            debris.Contents.Add(new StoredItem("item.jug", ItemCondition.Damaged));

            var wat = Person(1, "Wat");
            var girl = new Character(2, "Edith", new GridPos(10, 10)) { IsVillager = true, Sex = Sex.Female };
            girl.SetAge(AgeConfig.Default.TeenStartDay + 0.5);
            Assert.Equal(AgeStage.Teen, girl.Stage);
            k.Members.Add(wat); k.Members.Add(girl);
            var aff = new Dictionary<(int, int), int>();
            k.Home.AddAffection = (a, b, d) => aff[(a, b)] = (aff.TryGetValue((a, b), out var v) ? v : 0) + d;
            Assert.Contains(girl, k.Home.ServantCandidates());
            Assert.Null(k.Home.Hire(girl));
            Assert.Equal(HomeLife.ServantWageTeen, k.Home.ServantWage);
            int akce = k.House.Akce;
            k.Home.OnNewDay();
            Assert.Equal(akce - HomeLife.ServantWageTeen, k.House.Akce);
            Assert.True(candle.GivesLight, k.Home.LastServantReport);
            Assert.False(debris.HasContents);
            Assert.Equal(1, aff[(2, 1)]);
            Assert.Contains("Edith", k.Home.LastServantReport);

            // Ücret ödenmezse sadakat düşer, sonunda gider.
            while (k.House.Akce > 0) k.House.TrySpend(1);
            for (int i = 0; i < 4 && k.Home.ServantId != 0; i++) k.Home.OnNewDay();
            Assert.Equal(0, k.Home.ServantId);
            Assert.Contains(k.Notices, n => n.Contains("hizmetten ayrıldı"));
        }

        // ---------- desen ve arma ----------

        [Fact]
        public void Desen_KasnakGerekir_BoyaHarcanir_DesenGuzellikVerir()
        {
            var k = Make();
            string cells = new string('0', 32) + new string('1', 16) + new string('5', 16);   // yün, kırmızı, yeşil
            Assert.Null(k.Home.SavePattern("Çiçekli", cells));
            Assert.Contains("nakış kasnağı", k.Home.LastPatternProblem);
            k.World.PlaceItem(k.D("item.embroidery_frame"), new GridPos(6, 5));
            string id = k.Home.SavePattern("Çiçekli", cells);
            Assert.NotNull(id);
            var cost = HomeLife.PatternCost(cells);
            Assert.Equal(1, cost["item.dye_red"]);
            Assert.Equal(1, cost["item.dye_yellow"]);
            Assert.Equal(1, cost["item.dye_blue"]);

            var kilim = k.World.PlaceItem(k.D("item.kilim_small"), new GridPos(3, 3));
            Assert.Contains("yetmiyor", k.Home.ApplyPatternBlocker(kilim, id));
            k.House.AddItem("item.dye_red", 1); k.House.AddItem("item.dye_yellow", 1); k.House.AddItem("item.indigo", 1);   // çivit kalıbı maviye geçer
            Assert.True(k.Home.ApplyPattern(kilim, id));
            Assert.Equal(0, k.House.Count("item.indigo"));
            Assert.Equal(HomeLife.PatternBeautyBonus, k.Home.BeautyBonus(kilim), 6);
            Assert.NotNull(k.Home.PatternOn(kilim));
            Assert.Contains("desen almaz", k.Home.ApplyPatternBlocker(k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(7, 5)), id));
        }

        [Fact]
        public void Arma_RenkUstuneRenkKonmaz_BolunmusAlanSerbest()
        {
            var k = Make();
            Assert.Contains("Renk üstüne renk", HomeLife.ArmaRule(new ArmaDef { Field = 1, Division = "duz", Charge = "yildiz", ChargeColor = 3 }));   // kırmızı üstüne mavi
            Assert.Null(HomeLife.ArmaRule(new ArmaDef { Field = 1, Division = "duz", Charge = "yildiz", ChargeColor = 2 }));                        // kırmızı üstüne sarı
            Assert.Contains("Renk üstüne renk", HomeLife.ArmaRule(new ArmaDef { Field = 2, Division = "duz", Charge = "basak", ChargeColor = 0 }));  // sarı üstüne yün (maden/maden)
            Assert.Null(HomeLife.ArmaRule(new ArmaDef { Field = 1, Division = "dikine", Second = 3, Charge = "kule", ChargeColor = 4 }));             // bölünmüş alan serbest
            Assert.Contains("armada kullanılmaz", HomeLife.ArmaRule(new ArmaDef { Field = 6 }));
            Assert.Contains("Renk üstüne renk", HomeLife.ArmaRule(new ArmaDef { Field = 3, Division = "bant", Second = 1 }));                        // mavi üstüne kırmızı bant
            var arma = new ArmaDef { Field = 3, Division = "bant", Second = 2, Charge = "yok" };
            Assert.Null(k.Home.SetArma(arma));
            string cells = HomeLife.ArmaCells(arma);
            Assert.Equal(64, cells.Length);
            Assert.Equal('2', cells[0]);   // bant köşegenden geçer
            Assert.Equal('3', cells[7]);
            k.World.PlaceItem(k.D("item.embroidery_frame"), new GridPos(6, 5));
            var banner = k.World.PlaceOnWall(k.D("item.dynasty_banner"), new GridPos(3, 2), 0, Decor.BandHigh);
            k.House.AddItem("item.dye_blue", 1); k.House.AddItem("item.dye_yellow", 1);
            Assert.True(k.Home.ApplyPattern(banner, HomeLife.ArmaPatternId));
            Assert.Equal(HomeLife.ArmaBeautyBonus, k.Home.BeautyBonus(banner), 6);
        }

        // ---------- koleksiyon ve güvercinlik ----------

        [Fact]
        public void Koleksiyon_SergiRafiHerEsyadanBirAlir_TamamlanincaBildirim()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.display_shelf"), new GridPos(3, 2), 0);
            k.House.AddItem("item.mussel_shell", 3);
            k.House.AddItem("item.snail_shell", 1);
            k.House.AddItem("item.river_pebble", 1);
            Assert.Equal(3, DecorSystem.Fill(shelf, k.House, k.Db));   // aynı kabuktan bir tane
            Assert.Equal(2, k.House.Count("item.mussel_shell"));
            var shore = k.Home.Collections.First(c => c.Id == "col.shore");
            Assert.Equal((3, 4), k.Home.Progress(shore));
            CollectionDef done = null;
            k.Home.CollectionCompleted += c => done = c;
            k.House.AddItem("item.heron_feather", 1);
            DecorSystem.Fill(shelf, k.House, k.Db);
            k.Home.OnNewDay();
            Assert.Same(shore, done);
            Assert.True(k.Home.IsCollectionDone("col.shore"));
            Assert.Equal(HomeLife.CollectionBeautyBonus, k.Home.BeautyBonus(shelf), 6);

            k.World.PlaceItem(k.D("item.dovecote_village"), new GridPos(12, 12));
            int manure = k.House.Count("item.manure");
            k.Home.OnNewDay();
            Assert.True(k.House.Count("item.manure") > manure);
        }

        // ---------- oda planı ve mevsim seti ----------

        [Fact]
        public void OdaPlani_KaydedilirBaskaOdayaUygulanir_EksikSatinAlinir()
        {
            var k = Make();
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            k.World.PlaceOnWall(k.D("item.wall_shelf"), new GridPos(3, 2), 0);
            Assert.Null(k.Home.SavePlan(k.Room, "Oturma"));
            var plan = k.Home.Plans[0];
            Assert.Equal(3, plan.Entries.Count);

            // İkinci oda: 10..15 x 10..13.
            for (int x = 10; x <= 15; x++) { Wall(k, x, 10, EdgeAxis.Horizontal); Wall(k, x, 14, EdgeAxis.Horizontal); }
            for (int y = 10; y <= 13; y++) { Wall(k, 10, y, EdgeAxis.Vertical); Wall(k, 16, y, EdgeAxis.Vertical); }
            var room2 = k.World.Map.Rooms.RoomAt(new GridPos(11, 11));
            var (missing, cost) = k.Home.PlanNeeds(plan);
            Assert.Equal(3, missing.Count);
            Assert.True(cost > 0);
            var r = k.Home.ApplyPlan(plan, room2, buy: true, order: false);
            Assert.Equal(3, r.Placed);
            Assert.Equal(3, r.Bought);
            Assert.Contains(k.World.Items, it => it.Def.Id == "item.candle" && it.IsDecor && k.World.Map.Rooms.RoomAt(it.Position) == room2);
            k.Build.Undo();
            Assert.DoesNotContain(k.World.Items, it => k.World.Map.Rooms.RoomAt(it.Position) == room2);
        }

        [Fact]
        public void MevsimSeti_MevsimDonunceSolanKalkar_YenisiAsilir()
        {
            var k = Make();
            var winter = k.World.PlaceOnWall(k.D("item.winter_greenery"), new GridPos(3, 2), 0, Decor.BandHigh);
            Assert.Equal(1, k.Home.SaveSeasonSet(Season.Winter));
            var spring = k.World.PlaceOnWall(k.D("item.spring_garland"), new GridPos(5, 2), 0);
            Assert.Equal(1, k.Home.SaveSeasonSet(Season.Spring));
            k.World.RemoveItemInstances(spring.Id);
            k.House.AddItem("item.spring_garland", 1);
            var (hung, missing) = k.Home.OnSeasonChanged(Season.Spring);
            Assert.Equal(1, hung);
            Assert.Equal(0, missing);
            Assert.Null(k.World.GetItem(winter.Id));                    // solan kış dalı envantere
            Assert.Equal(1, k.House.Count("item.winter_greenery"));
            Assert.Contains(k.World.Items, it => it.Def.Id == "item.spring_garland");
            // Kaldır etkileşimi solan süste görünür.
            var w2 = k.World.PlaceOnWall(k.D("item.winter_greenery"), new GridPos(6, 2), 0, Decor.BandHigh);
            At(k, k.Clock.Config.DaysPerSeason + 0.5);                 // bahar
            Assert.Contains(k.Home.Chores(), ch => ch.item == w2 && ch.interaction.HomeAction == HomeActions.Stow);
        }

        // ---------- ev yarışması ve konuk ----------

        [Fact]
        public void EvYarismasi_YildaBirKez_BirinciyeKurdele_KonukYorumu()
        {
            var k = Make();
            string granted = null;
            k.Home.GrantKeepsake = (id, label) => { granted = id; return null; };
            k.Home.NeighborCount = () => 3;
            var low = k.Home.JudgeContest(seedOverride: 5);
            Assert.NotNull(low);
            Assert.True(low.Rank > 1);
            Assert.Null(k.Home.JudgeContest(seedOverride: 5));      // aynı yıl ikinci kez yok

            // Güzel ev, ilk yıl (yeni kurulum): komşuların ilk yıl en yüksek puanı taban + yayılım.
            k = Make();
            k.Home.GrantKeepsake = (id, label) => { granted = id; return null; };
            k.Home.NeighborCount = () => 3;
            var table = k.World.PlaceItem(k.D("item.walnut_table"), new GridPos(4, 4));
            foreach (var s in table.Def.SurfaceSlots.Where(s => !s.IsCover).Take(3)) k.World.PlaceOnSurface(k.D("item.beeswax_candle"), table.Id, s.Id);
            k.World.PlaceItem(k.D("item.rug_kufic"), new GridPos(3, 3));
            k.World.PlaceItem(k.D("item.iron_chandelier"), new GridPos(6, 3));
            k.World.PlaceOnWall(k.D("item.tapestry_hunt"), new GridPos(3, 2), 0);
            k.World.PlaceOnWall(k.D("item.tapestry_garden"), new GridPos(3, 5), 2);
            k.World.PlaceItem(k.D("item.courtyard_pool"), new GridPos(12, 12));   // bahçe süsü
            k.World.PlaceItem(k.D("item.sundial"), new GridPos(15, 12));
            Assert.True(k.Home.HomeScore() > HomeLife.NeighborBase + HomeLife.NeighborSpread, k.Mood.BeautyOf(k.Room).Breakdown());
            var win = k.Home.JudgeContest(seedOverride: 1);
            Assert.Equal(1, win.Rank);
            Assert.Equal(HomeLife.HomeRibbon, granted);
            Assert.Equal(1, k.Home.ContestWins);

            var guest = new Character(9, "Godric", new GridPos(0, 0)) { IsVillager = true, Sex = Sex.Male };
            guest.SetAge(32);
            var daughter = new Character(3, "Agnes", new GridPos(3, 3)) { Sex = Sex.Female };
            daughter.SetAge(32);
            k.Members.Add(daughter);
            int given = 0;
            k.Home.AddAffection = (a, b, d) => given += d;
            Assert.Contains("Godric evi gezdi", k.Home.GuestVisit(guest));
            Assert.True(given > 0);
        }

        // ---------- gömme niş ----------

        [Fact]
        public void Nis_YalnizcaKalinDuvaraOyulur()
        {
            var k = Make();
            var niche = k.D("item.wall_niche");
            var thin = k.World.CanPlaceOnWall(niche, new GridPos(3, 2), 0);
            Assert.False(thin.Ok);
            Assert.Contains("kalın duvara", thin.Reason);
            Wall(k, 3, 2, EdgeAxis.Horizontal, "wall_stone");
            Assert.True(k.World.CanPlaceOnWall(niche, new GridPos(3, 2), 0).Ok);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_EvBakimiGeriGelir()
        {
            var k = Make();
            var bed = k.World.PlaceItem(k.D("item.bed_frame"), new GridPos(3, 4));
            k.Home.ApplyUpgrade(bed, "mattress_wool");
            k.World.PlaceItem(k.D("item.embroidery_frame"), new GridPos(6, 5));
            string pid = k.Home.SavePattern("Kare", new string('3', 64));
            k.Home.SetArma(new ArmaDef { Field = 1, Division = "duz", Charge = "yildiz", ChargeColor = 2 });
            k.Home.SaveSeasonSet(Season.Spring);
            var girl = new Character(2, "Edith", new GridPos(10, 10)) { IsVillager = true };
            girl.SetAge(AgeConfig.Default.TeenStartDay + 0.5);
            k.Members.Add(girl);
            k.Home.Hire(girl);
            var save = k.Home.Export();

            var k2 = Make();
            var bed2 = k2.World.PlaceItem(k2.D("item.bed_frame"), new GridPos(3, 4));
            Assert.Equal(bed.Id, bed2.Id);
            k2.Home.Import(save);
            Assert.True(k2.Home.HasUpgrade(bed2.Id, "mattress_wool"));
            Assert.NotNull(k2.Home.FindPattern(pid));
            Assert.NotNull(k2.Home.Arma);
            Assert.Equal(2, k2.Home.ServantId);
            k2.Home.Import(null);
            Assert.Equal(0, k2.Home.ServantId);
        }

        [Fact]
        public void Icerik_DonemKurallari_YeniEsyalarGecerli()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            foreach (var id in new[] { "item.jazari_peacock", "item.water_clock", "item.turning_eagle_lectern", "item.turf_seat", "item.sundial",
                                       "item.dovecote", "item.courtyard_pool", "item.joke_fountain", "item.window_seat", "item.wall_niche", "item.display_shelf",
                                       "item.clothes_perch", "item.ceiling_painted_beam", "item.embroidery_frame", "item.home_ribbon" })
                Assert.True(db.Items.Contains(id), id);
            Assert.Equal(4, db.Parts.Get("finish_star_tile").Beauty);
            Assert.Contains(db.Items.Get("item.spring_branch").Interactions, i => i.HomeAction == HomeActions.Stow);
            Assert.Equal("item.down_feather", db.Animals.Get("animal.chicken").Hide);
        }

        // ---------- varyant: gövde ve ikinci renk (belge 45 §10) ----------

        [Fact]
        public void Varyant_TasMerdivenKaraGovde_KirmiziKorkuluk_BeceriyleAcilir()
        {
            var k = Make();
            int masonry = 1, smithing = 1;
            k.Build.SkillLevel = s => s == SkillType.Masonry ? masonry : s == SkillType.Smithing ? smithing : 1;
            WorldState.StairsEnds(k.D("item.stairs_up_stone"), new GridPos(5, 3), 0, out _, out var landing);
            var lt = k.Build.BuildLevelTile(landing, stairEnd: true);
            Assert.True(lt.Ok, lt.Reason);
            var stairs = k.World.PlaceItem(k.D("item.stairs_up_stone"), new GridPos(5, 3));
            Assert.Contains(BuildParts.FinishesFor(k.Db, stairs.Def), f => f.Id == "stone_black");
            Assert.Contains(BuildParts.AccentsFor(k.Db, stairs.Def), f => f.Id == "metal_red");
            Assert.Equal("Korkuluk", stairs.Def.AccentLabel);

            var locked = k.Build.SetItemFinish(stairs.Id, "stone_black");
            Assert.False(locked.Ok);
            Assert.Contains("Taş ustalığı", locked.Reason);
            masonry = 10; smithing = 10;
            int akce = k.House.Akce;
            var fr = k.Build.SetItemFinish(stairs.Id, "stone_black");
            Assert.True(fr.Ok, fr.Reason);
            k.House.AddItem("item.dye_red", 1);
            var ar = k.Build.SetItemAccent(stairs.Id, "metal_red");
            Assert.True(ar.Ok, ar.Reason);
            Assert.Equal("stone_black", stairs.FinishId);
            Assert.Equal("metal_red", stairs.AccentId);
            Assert.True(k.House.Akce < akce, $"{k.House.Akce} {akce}");                 // pahalı kaplama farkı ödendi
            Assert.Equal(0, k.House.Count("item.dye_red"));   // kırmızı boya harcandı
            k.Build.Undo();
            Assert.Null(stairs.AccentId);
            Assert.Contains("Bu eşyanın ikinci rengi yok", k.Build.SetItemAccent(k.World.PlaceItem(k.D("item.rug_wool"), new GridPos(14, 14)).Id, "metal_red").Reason);
        }

        [Fact]
        public void Varyant_IcerikCesitliligi_HerTurdeCokSecenek()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var kind in new[] { BuildPartKind.WallMaterial, BuildPartKind.WallFinish, BuildPartKind.Door, BuildPartKind.Window, BuildPartKind.Railing, BuildPartKind.Floor, BuildPartKind.Roof })
                Assert.True(BuildParts.OfKind(db, kind).Count >= 7, kind.ToString());
            foreach (var g in new[] { "wood", "stone", "metal", "cloth", "clay" })
                Assert.True(BuildParts.AccentsFor(db, new ItemDef { AccentGroup = g }).Count >= 5, g);
            Assert.Equal(SkillType.Masonry, Thatchwick.Core.Progress.UnlockGate.SkillFor(db.Parts.Get("stone_black")));
            Assert.Equal(SkillType.Smithing, Thatchwick.Core.Progress.UnlockGate.SkillFor(db.Parts.Get("railing_iron_red")));
            Assert.Equal(SkillType.Weaving, Thatchwick.Core.Progress.UnlockGate.SkillFor(db.Parts.Get("cloth_navy")));
        }

        // ---------- yürüyen hizmetkâr ve ev bitkileri (belge 45 ek) ----------

        [Fact]
        public void Hizmetkar_Yururken_SabahEvGorevineGecer_OglenBirakir_Rapor()
        {
            var k = Make();
            k.Home.ServantWalks = true;
            var girl = new Character(2, "Edith", new GridPos(10, 10)) { IsVillager = true };
            girl.SetAge(AgeConfig.Default.TeenStartDay + 0.5);
            k.Members.Add(girl);
            Assert.Null(k.Home.Hire(girl));
            At(k, 0.1);
            k.Home.Tick(0.01);
            Assert.Equal(DutyKind.None, girl.Duty);
            At(k, 0.35);
            k.Home.Tick(0.01);
            Assert.True(k.Home.InService);
            Assert.Equal(DutyKind.House, girl.Duty);
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var candle = k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            candle.SetCandle(0);
            k.House.AddItem("item.candle", 1);
            k.Effects.Apply(girl, candle, Inter(candle.Def, "replace_candle"));   // iş koşucusu yapmış gibi
            At(k, 0.55);
            k.Home.Tick(0.01);
            Assert.Equal(DutyKind.None, girl.Duty);
            Assert.Contains("1 mum", k.Home.LastServantReport);
            int akce = k.House.Akce;
            k.Home.OnNewDay();
            Assert.Equal(akce - HomeLife.ServantWageTeen, k.House.Akce);   // ücret yine sabah; işler anında yapılmaz
        }

        [Fact]
        public void JobRunner_OtonomiKapaliyken_GorevIsiAlir()
        {
            var k = Make();
            var c = Person();
            c.Duty = DutyKind.House;
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var candle = k.World.PlaceOnSurface(k.D("item.candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            candle.SetCandle(0);
            k.House.AddItem("item.candle", 1);
            var duties = new DutyPlanner(k.World, null, k.Effects, null) { HouseChores = k.Home.Chores };
            var runner = new JobRunner(c, k.World) { Effects = k.Effects, Autonomous = false, DutyProvider = duties.Next };
            runner.Tick(0.01);
            Assert.True(runner.IsIdle);                 // görev almaz
            runner.DutyWhenIdle = true;
            runner.Tick(0.01);
            Assert.False(runner.IsIdle);
        }

        [Fact]
        public void SusBitkisi_GolgeSever_CicekAcar_SuTeknesiYavasKurur()
        {
            var k = Make();
            var c = Person();
            // Eğrelti gölgede tam hızla büyür.
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var fern = k.World.PlaceOnSurface(k.D("item.fern_trough"), table.Id, table.Def.SurfaceSlots.First(s => !s.IsCover && s.Size != DecorSize.Small).Id);
            Assert.False(k.Home.Sunlit(fern));
            var water = Inter(fern.Def, "water_pot");
            Assert.DoesNotContain(fern.Def.Interactions, i => i.HomeAction == HomeActions.HarvestPot);   // süs bitkisi ot vermez
            for (int d = 0; d < 6; d++) { k.Effects.Apply(c, fern, water); k.Home.Tick(0.5); k.Effects.Apply(c, fern, water); k.Home.Tick(0.5); }
            Assert.True(k.Home.PotGrown(fern), k.Home.PotLabel(fern));
            Assert.Equal(HomeLife.GrownPlantBeauty, k.Home.BeautyBonus(fern), 6);   // çiçeksiz: yalnızca gelişmiş eki

            // Siklamen kışın çiçek açar.
            var cyc = k.World.PlaceOnSurface(k.D("item.pot_cyclamen"), table.Id, table.Def.SurfaceSlots.First(s => !s.IsCover && k.World.CanPlaceOnSurface(k.D("item.pot_cyclamen"), table.Id, s.Id).Ok).Id);
            for (int d = 0; d < 6; d++) { k.Effects.Apply(c, cyc, water); k.Home.Tick(0.5); }
            At(k, 3 * k.Clock.Config.DaysPerSeason + 0.5);   // kış
            k.Effects.Apply(c, cyc, Inter(cyc.Def, "water_pot"));
            Assert.True(k.Home.InBloom(cyc), k.Home.PotLabel(cyc));
            Assert.Contains("çiçekte", k.Home.PotLabel(cyc));
            Assert.Equal(HomeLife.GrownPlantBeauty + HomeLife.BloomBeauty, k.Home.BeautyBonus(cyc), 6);

            // Su teknesi bahçesi yavaş kurur.
            var pool = k.World.PlaceItem(k.D("item.water_garden"), new GridPos(6, 5));
            k.Effects.Apply(c, pool, Inter(pool.Def, "water_pot"));
            k.Home.Tick(3.0);
            Assert.False(k.Home.PotWilted(pool));
            Assert.False(k.Home.PotThirsty(pool) && k.Home.StateOf(pool.Id).PotDry > HomeLife.PotWiltDays);
        }
    }
}
