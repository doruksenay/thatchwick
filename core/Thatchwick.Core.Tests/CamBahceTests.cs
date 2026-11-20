using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Cam bahçe (Project belgesi 47): teraryumun dönem karşılığı. Habitat, kurulamayan kombinasyonlar, besin zinciri, yakalama, kayıt.
    public class CamBahceTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public GameClock Clock;
            public HomeLife Home;
            public List<string> Notices = new List<string>();
            public ItemDef D(string id) => Db.Items.Get(id);
        }

        private static Kit Make(int seed = 7)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(20, 20)), House = new Household(5000), Clock = new GameClock() };
            k.Home = new HomeLife(k.World, db, k.House, k.Clock) { Notify = (t, _) => k.Notices.Add(t), Rng = new System.Random(seed) };
            for (int x = 2; x <= 7; x++) { Wall(k, x, 2, EdgeAxis.Horizontal); Wall(k, x, 6, EdgeAxis.Horizontal); }
            for (int y = 2; y <= 5; y++) { Wall(k, 2, y, EdgeAxis.Vertical); Wall(k, 8, y, EdgeAxis.Vertical); }
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 5; y++) k.World.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
            return k;
        }

        private static void Wall(Kit k, int x, int y, EdgeAxis axis) => k.World.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall, null);

        private static ItemInstance Garden(Kit k, bool large = false)
        {
            if (large) return k.World.PlaceItem(k.D("item.glass_garden_large"), new GridPos(4, 4));
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            return k.World.PlaceOnSurface(k.D("item.glass_garden"), table.Id, table.Def.SurfaceSlots[0].Id);
        }

        private static readonly Localizer Tr = new Localizer("tr", LocTable.Parse(File.ReadAllText(LocPath("ui.tr.json")), new List<string>()),
            LocTable.Parse(File.ReadAllText(LocPath("ui.en.json")), new List<string>()));
        private static string LocPath(string f) => Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization", f));

        /// <summary>Core'un cam bahçe metnini Türkçe çözer (oyundaki gibi).</summary>
        private static string S(Kit k, LocText? t) => t.HasValue ? VivText.Resolve(Tr, t.Value, id => k.Db.Items.TryGet(id, out var d) ? d.DisplayName : id) : null;

        private static void Give(Kit k, params string[] ids) { foreach (var id in ids) k.House.AddItem(id, 1); }

        /// <summary>Zemin malzemesini (doğadan gelmiş say) verip zemini kurar.</summary>
        private static void Ground(Kit k, ItemInstance g, string habitat)
        {
            var need = HomeLife.NeedFor(habitat);
            Give(k, need.Substrates[0]);
            if (need.Water != null) Give(k, need.Water);
            Assert.Null(k.Home.SetHabitat(g, habitat));
        }

        [Fact]
        public void Icerik_TurlerVeKaplarYuklenir_YeniDunyaTuruYok()
        {
            var k = Make();
            var g = k.D("item.glass_garden");
            Assert.True(g.IsVivarium);
            Assert.Equal(4, g.VivCreatures);
            Assert.Equal(3, g.VivPlants);
            var species = k.Db.Items.All.Where(d => d.Viv != null).ToList();
            Assert.True(species.Count >= 20);
            Assert.Contains(species, d => d.Viv.Habitats.Contains(HomeLife.Salt));
            Assert.Contains(k.D("item.forage_mushroom").Interactions, i => i.HomeAction == HomeActions.Catch);
            Assert.Contains(k.D("item.glass_bench").Interactions, i => i.Id == "craft:glass_garden");
        }

        [Fact]
        public void Zemin_SecilmedenTurEklenmez_DoluykenZeminDegismez()
        {
            var k = Make();
            var g = Garden(k);
            Give(k, "item.viv_moss");
            Assert.Contains("zemini", S(k, k.Home.CanAddToViv(g, "item.viv_moss")));
            Assert.Contains("doğadan", S(k, k.Home.SetHabitat(g, HomeLife.Moist)));   // orman toprağı yok
            Ground(k, g, HomeLife.Moist);
            Assert.Equal(0, k.House.Count("item.viv_forest_soil"));
            Assert.True(k.Home.AddToViv(g, "item.viv_moss"));
            Give(k, "item.sand");
            Assert.NotNull(k.Home.SetHabitat(g, HomeLife.Dry));
            Assert.NotNull(k.Home.EmptyViv(g));                                  // içinde yosun var
            Assert.Equal(0, k.House.Count("item.viv_moss"));
        }

        [Fact]
        public void TuzluSu_DenizSuyuVeDenizKumuIster_TuzOlmaz()
        {
            var k = Make();
            var g = Garden(k);
            Give(k, "item.salt", "item.viv_sea_sand");
            Assert.Contains("deniz suyu", S(k, k.Home.SetHabitat(g, HomeLife.Salt)));
            Assert.NotNull(k.Home.SetHabitat(g, HomeLife.Salt, "item.sand"));   // göl kumu tuzlu suya olmaz
            Give(k, HomeLife.SeaWaterItem);
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Salt));
            Assert.Equal(0, k.House.Count(HomeLife.SeaWaterItem));
            Assert.Equal(0, k.House.Count("item.viv_sea_sand"));
            Assert.Equal(1, k.House.Count("item.salt"));
            Assert.Equal("item.viv_sea_sand", k.Home.VivOf(g).Substrate);
        }

        [Fact]
        public void KuruKum_GolKumuYaDaDenizKumu_SecilenKalir_Bosaltilir()
        {
            var k = Make();
            var g = Garden(k);
            Give(k, "item.sand", "item.viv_sea_sand");
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Dry, "item.viv_sea_sand"));
            Assert.Equal("item.viv_sea_sand", k.Home.VivOf(g).Substrate);
            Assert.Equal(1, k.House.Count("item.sand"));
            Assert.Null(k.Home.EmptyViv(g));
            Assert.Null(k.Home.VivOf(g).Habitat);
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Dry, "item.sand"));
            var save = k.Home.Export();
            var k2 = Make();
            var g2 = Garden(k2);
            k2.Home.Import(save);
            Assert.Equal("item.sand", k2.Home.VivOf(g2).Substrate);
        }

        [Fact]
        public void OlusmamasiGerekenKombinasyonlar_Reddedilir()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_firefly", "item.viv_frog", "item.viv_spider", "item.viv_spider", "item.viv_lizard", "item.viv_seaweed");

            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_lizard"));      // kertenkele kuru kum ister
            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_seaweed"));     // deniz yosunu tuzlu su ister
            Assert.True(k.Home.AddToViv(g, "item.viv_firefly"));
            Assert.Contains("zehir", S(k, k.Home.CanAddToViv(g, "item.viv_frog")));
            Assert.True(k.Home.AddToViv(g, "item.viv_spider"));
            Assert.Contains("İki", S(k, k.Home.CanAddToViv(g, "item.viv_spider")));
        }

        [Fact]
        public void Kapasite_BitkiVeCanliAyriSayilir()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            for (int i = 0; i < 4; i++) Give(k, "item.viv_moss");
            for (int i = 0; i < 3; i++) Assert.True(k.Home.AddToViv(g, "item.viv_moss"));
            Assert.Contains("Bitki yeri dolu", S(k, k.Home.CanAddToViv(g, "item.viv_moss")));
            Give(k, "item.viv_worm");
            Assert.True(k.Home.AddToViv(g, "item.viv_worm"));
        }

        [Fact]
        public void Uyari_KimKimiYer()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_aphid", "item.viv_ladybird");
            k.Home.AddToViv(g, "item.viv_aphid");
            var w = k.Home.AddWarnings(g, "item.viv_ladybird");
            Assert.Contains(w, s => S(k, s).Contains("yaprak biti yer"));
        }

        [Fact]
        public void BesinZinciri_UgurBocegiYaprakBitiniBitirir_BitkiKurtulur()
        {
            var k = Make(11);
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_fern", "item.viv_aphid", "item.viv_aphid", "item.viv_ladybird", "item.viv_worm");
            foreach (var id in new[] { "item.viv_moss", "item.viv_fern", "item.viv_aphid", "item.viv_aphid", "item.viv_ladybird", "item.viv_worm" })
                Assert.True(k.Home.AddToViv(g, id), id);
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 10; d++) { v.Moisture = 80; k.Home.VivariumDay(); }
            Assert.DoesNotContain(v.Entries, e => e.Item == "item.viv_aphid");
            Assert.Contains(v.Entries, e => e.Item == "item.viv_moss");
            Assert.Contains(v.Log, s => S(k, s).Contains("yedi"));
        }

        [Fact]
        public void YaprakBiti_AvcisizCogalir_BitkiyiKurutur()
        {
            var k = Make(3);
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_violet", "item.viv_aphid", "item.viv_aphid");
            k.Home.AddToViv(g, "item.viv_violet");
            k.Home.AddToViv(g, "item.viv_aphid");
            k.Home.AddToViv(g, "item.viv_aphid");
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 12; d++) { v.Moisture = 80; k.Home.VivariumDay(); }
            Assert.DoesNotContain(v.Entries, e => e.Item == "item.viv_violet");
        }

        [Fact]
        public void Kuruma_NemlendirilmezseBitkilerOlur_KufBirikir()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_snail");
            k.Home.AddToViv(g, "item.viv_moss");
            k.Home.AddToViv(g, "item.viv_snail");
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 15 && v.Entries.Count > 0; d++) { k.Home.VivariumTick(1); k.Home.VivariumDay(); }
            Assert.Empty(v.Entries);
            Assert.True(v.Mold > 0);
            Assert.Contains(v.Log, s => S(k, s).Contains("kurudu") || S(k, s).Contains("öldü"));
        }

        [Fact]
        public void Nemlendirme_EvGorevidir_VeKuruKumdaFazlasiYapilmaz()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_moss");
            k.Home.AddToViv(g, "item.viv_moss");
            k.Home.VivOf(g).Moisture = 30;
            Assert.True(k.Home.NeedsMist(g));
            Assert.Contains(k.Home.Chores(), c => c.item.Id == g.Id && c.interaction.HomeAction == HomeActions.Mist);
            Assert.Null(k.Home.Mist(g));
            Assert.False(k.Home.NeedsMist(g));
            Assert.NotNull(k.Home.Mist(g));
        }

        [Fact]
        public void Tirtil_KelebegeDoner()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_violet", "item.viv_violet", "item.viv_caterpillar");
            k.Home.AddToViv(g, "item.viv_violet");
            k.Home.AddToViv(g, "item.viv_violet");
            k.Home.AddToViv(g, "item.viv_caterpillar");
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 5; d++) { v.Moisture = 80; k.Home.VivariumDay(); }
            Assert.Contains(v.Entries, e => e.Item == "item.viv_butterfly");
            Assert.DoesNotContain(v.Entries, e => e.Item == "item.viv_caterpillar");
        }

        [Fact]
        public void Denge_GuzellikVerir_AtesBocegiGeceIsik()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_fern", "item.viv_worm", "item.viv_firefly", "item.viv_firefly", "item.viv_firefly");
            foreach (var id in new[] { "item.viv_moss", "item.viv_fern", "item.viv_worm", "item.viv_firefly", "item.viv_firefly", "item.viv_firefly" })
                k.Home.AddToViv(g, id);
            Assert.True(k.Home.IsBalanced(k.Home.VivOf(g)));
            k.Clock.Restore(0.5, 1);
            double day = k.Home.BeautyBonus(g);
            Assert.True(day >= 4 * HomeLife.VivBeautyPerSpecies + HomeLife.VivBalancedBeauty, day.ToString());
            Assert.Equal(0, k.Home.VivLight(g));
            k.Clock.Restore(0.85, 1);
            Assert.Equal(1, k.Home.VivLight(g));
            Assert.True(k.Home.BeautyBonus(g) > day);
        }

        [Fact]
        public void Yakalama_KavanozIster_MevsimVeGeceyeBakar()
        {
            var k = Make(1);
            var node = k.World.PlaceItem(k.D("item.forage_herbs"), new GridPos(12, 12));
            k.Clock.Restore(10 + 0.5, 1);                         // yaz öğle
            Assert.Contains("kavanoz", k.Home.CatchBlocker(node));
            Give(k, HomeLife.CatchJar);
            Assert.Null(k.Home.CatchBlocker(node));
            string got = null;
            for (int i = 0; i < 20 && got == null; i++) got = k.Home.Catch(node);
            Assert.NotNull(got);
            Assert.NotEqual("item.viv_firefly", got);          // ateş böceği yalnızca gece
            Assert.Equal(1, k.House.Count(got));
            Assert.Equal(1, k.House.Count(HomeLife.CatchJar)); // kavanoz harcanmaz

            k.Clock.Restore(30 + 0.5, 1);                         // kış: otlukta bir şey yok
            Assert.NotNull(k.Home.CatchBlocker(node));
        }

        [Fact]
        public void Kayit_GidipGelir()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_snail");
            k.Home.AddToViv(g, "item.viv_moss");
            k.Home.AddToViv(g, "item.viv_snail");
            k.Home.VivOf(g).Entries[1].Hunger = 25;
            var save = k.Home.Export();

            var k2 = Make();
            var g2 = Garden(k2);
            Assert.Equal(g.Id, g2.Id);
            k2.Home.Import(save);
            var v = k2.Home.VivOf(g2);
            Assert.Equal(HomeLife.Moist, v.Habitat);
            Assert.Equal(2, v.Entries.Count);
            Assert.Equal(25, v.Entries[1].Hunger, 3);
        }

        // ---------- tatlı su, balık ve süs (kullanıcı 20.11: balık, taş, yosun, kabuk) ----------

        [Fact]
        public void TatliSu_GolSuyuIster_BalikKonur_KaraHayvaniKonmaz()
        {
            var k = Make();
            var g = Garden(k);
            Give(k, "item.viv_marsh_mud");
            Assert.NotNull(k.Home.SetHabitat(g, HomeLife.Fresh));            // su yok
            Give(k, HomeLife.FreshWaterItem);
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Fresh));
            Give(k, "item.viv_minnow", "item.viv_hornwort", "item.viv_lizard");
            Assert.True(k.Home.AddToViv(g, "item.viv_hornwort"));
            Assert.True(k.Home.AddToViv(g, "item.viv_minnow"));
            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_lizard"));
            var v = k.Home.VivOf(g);
            v.Moisture = 50;
            Assert.NotNull(k.Home.Mist(g));                    // tazelemek için de göl suyu gerekir
            Give(k, HomeLife.FreshWaterItem);
            Assert.Null(k.Home.Mist(g));
            Assert.Equal(100, v.Moisture, 3);
        }

        [Fact]
        public void Sus_AyriSayilir_HerZemineUyanVeYalnizDenizeUyanAyrilir()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_flat_stone", "item.river_pebble", "item.viv_mossy_stone", "item.viv_bark", "item.viv_coral");
            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_coral"));     // mercan tuzlu suda
            Assert.True(k.Home.AddToViv(g, "item.viv_flat_stone"));
            Assert.True(k.Home.AddToViv(g, "item.river_pebble"));
            Assert.True(k.Home.AddToViv(g, "item.viv_mossy_stone"));
            Assert.Contains("Süs yeri dolu", S(k, k.Home.CanAddToViv(g, "item.viv_bark")));
            var v = k.Home.VivOf(g);
            Assert.Equal(3, k.Home.CountDecor(v));
            Assert.Equal(0, k.Home.CountCreatures(v));
            Assert.Equal(0, k.Home.CountPlants(v));
            // süsler tabanda ayrı yerlere dizilir, kaydırılır
            Assert.Equal(3, v.Entries.Select(e => System.Math.Round(e.Pos, 2)).Distinct().Count());
            k.Home.MoveInViv(g, 0, -5);
            Assert.True(v.Entries[0].Pos >= 0.05 && v.Entries[0].Pos < 0.1);
            // süsler gün geçince bozulmaz
            for (int d = 0; d < 5; d++) { k.Home.VivariumTick(1); k.Home.VivariumDay(); }
            Assert.Equal(3, v.Entries.Count);
            Assert.True(k.Home.BeautyBonus(g) >= 3);
        }

        [Fact]
        public void Istek_KesisYengeciBosKabukIster_KertenkeleGunesTasi()
        {
            var k = Make();
            var g = Garden(k);
            Give(k, "item.viv_hermit", "item.viv_seaweed", "item.viv_whelk");
            Ground(k, g, HomeLife.Salt);
            k.Home.AddToViv(g, "item.viv_seaweed");
            Assert.Contains(k.Home.AddWarnings(g, "item.viv_hermit"), w => S(k, w).Contains("boş kabuk"));
            k.Home.AddToViv(g, "item.viv_hermit");
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 2; d++) { v.Moisture = 90; k.Home.VivariumDay(); }
            Assert.False(k.Home.IsBalanced(v));
            double before = v.Entries.First(e => e.Item == "item.viv_hermit").Health;
            Assert.True(before < 100);
            k.Home.AddToViv(g, "item.viv_whelk");
            Assert.Null(k.Home.UnmetNeed(v, k.D("item.viv_hermit").Viv));
            Assert.True(k.Home.IsBalanced(v));

            var k2 = Make();
            var dry = Garden(k2);
            Ground(k2, dry, HomeLife.Dry);
            Give(k2, "item.viv_lizard", "item.viv_flat_stone");
            Assert.Contains(k2.Home.AddWarnings(dry, "item.viv_lizard"), w => S(k2, w).Contains("güneşlenme taşı"));
            k2.Home.AddToViv(dry, "item.viv_flat_stone");
            Assert.DoesNotContain(k2.Home.AddWarnings(dry, "item.viv_lizard"), w => S(k2, w).Contains("güneşlenme taşı"));
        }

        [Fact]
        public void Iribas_KurbagayaDoner_TatliSuda()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Fresh);
            Give(k, "item.viv_hornwort", "item.viv_duckweed", "item.viv_tadpole");
            foreach (var id in new[] { "item.viv_hornwort", "item.viv_duckweed", "item.viv_tadpole" }) k.Home.AddToViv(g, id);
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 7; d++) { v.Moisture = 90; k.Home.VivariumDay(); }
            Assert.Contains(v.Entries, e => e.Item == "item.viv_frog");
        }

        [Fact]
        public void Golet_YazinBalikVerir_KisinBalikYok()
        {
            var k = Make(5);
            var pond = k.World.PlaceItem(k.D("item.pond"), new GridPos(12, 12));
            Give(k, HomeLife.CatchJar);
            k.Clock.Restore(10 + 0.5, 1);
            var got = new HashSet<string>();
            for (int i = 0; i < 60; i++) { var c = k.Home.Catch(pond); if (c != null) got.Add(c); }
            Assert.Contains(got, id => k.D(id).Viv?.Move == "swim" && k.D(id).Viv.IsCreature);
            k.Clock.Restore(30 + 0.5, 1);
            var winter = new HashSet<string>();
            for (int i = 0; i < 40; i++) { var c = k.Home.Catch(pond); if (c != null) winter.Add(c); }
            Assert.DoesNotContain(winter, id => k.D(id).Viv.IsCreature);
        }

        [Fact]
        public void Kayit_SusYeriniKorur()
        {
            var k = Make();
            var g = Garden(k);
            Ground(k, g, HomeLife.Moist);
            Give(k, "item.viv_bark");
            k.Home.AddToViv(g, "item.viv_bark");
            k.Home.MoveInViv(g, 0, 0.3);
            double pos = k.Home.VivOf(g).Entries[0].Pos;
            var save = k.Home.Export();
            var k2 = Make();
            var g2 = Garden(k2);
            k2.Home.Import(save);
            Assert.Equal(pos, k2.Home.VivOf(g2).Entries[0].Pos, 3);
        }

        // ---------- her şey doğadan (kullanıcı 20.11.2026) ----------

        [Fact]
        public void YabanMali_PazardaSatinAlinmaz()
        {
            var k = Make();
            var market = new Thatchwick.Core.Economy.Market(k.Db, k.House);
            foreach (var id in new[] { "item.viv_goby", "item.viv_coral", "item.viv_whelk", "item.viv_sea_water", "item.viv_forest_soil", "item.river_pebble" })
                Assert.False(market.IsBuyable(id), id);
        }

        [Fact]
        public void OrmanTopragi_YalnizcaOrmandaDoldurulur()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var world = new WorldState(new GridMap(Regions.WorldWidth, Regions.WorldHeight));
            var house = new Household(0);
            var home = new HomeLife(world, db, house, new GameClock());
            house.AddItem(HomeLife.CatchJar, 1);
            var farm = world.PlaceItem(db.Items.Get("item.forage_mushroom"), new GridPos(10, 10));
            var forest = world.PlaceItem(db.Items.Get("item.forage_mushroom"), new GridPos(Regions.Forest.X + 22, Regions.Forest.Y + 20));
            Assert.Contains("Orman", home.FillBlocker(farm));
            Assert.Null(home.FillBlocker(forest));
            home.Fill(forest);
            Assert.Equal(1, house.Count("item.viv_forest_soil"));
            Assert.Contains(db.Items.Get("item.forage_mushroom").Interactions, i => i.HomeAction == HomeActions.Fill);
        }

        [Fact]
        public void DenizKumsali_YalnizcaKiyidaBelirir_GolKumuKiyidaBelirmez()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var seaSand = db.Items.Get("item.forage_sea_sand");
            var lakeSand = db.Items.Get("item.forage_sand");
            var coastTile = new GridPos(Regions.Coast.X + 5, Regions.Coast.Y + 10);
            var lakeTile = new GridPos(Regions.Lake.X + 5, Regions.Lake.Y + 10);
            Assert.True(ForageSystem.InRegion(seaSand, coastTile));
            Assert.False(ForageSystem.InRegion(seaSand, lakeTile));
            Assert.True(ForageSystem.InRegion(lakeSand, lakeTile));
            Assert.False(ForageSystem.InRegion(lakeSand, coastTile));
        }

        [Fact]
        public void Kiyi_BolgesiCizilir_DenizGelgitCukuruVeGecitVar()
        {
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            Regions.BuildTerrain(map);
            Regions.Apply(map);
            int sea = 0;
            for (int y = Regions.Coast.Y; y < Regions.Coast.Y + Regions.Coast.H; y++)
                for (int x = Regions.Coast.X; x < Regions.Coast.X + Regions.Coast.W; x++)
                    if (map.GetFloor(new GridPos(x, y)) == FloorType.Water) sea++;
            Assert.True(sea > 300, sea.ToString());
            Assert.False(map.IsBlocked(new GridPos(Regions.Coast.X + 5, Regions.Coast.Y)));
            var items = Regions.Items();
            Assert.Contains(items, i => i.Id == "item.forage_tidepool" && Regions.Coast.Contains(new GridPos(i.X, i.Y)));
            Assert.Contains(items, i => i.Id == "item.forage_driftline");
            Assert.Equal("coast", HomeLife.WaterKindAt(new GridPos(Regions.Coast.X + 10, Regions.Coast.Y + 28)) == HomeLife.SeaWater ? "coast" : "?");
            // Eski kayıt: kıyı kapalıyken açılır, ikinci kez dokunmaz
            var old = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            for (int y = Regions.Coast.Y; y < Regions.Coast.Y + Regions.Coast.H; y++)          // eski kayıtta kıyı bölge dışıydı: kapalı
                for (int x = Regions.Coast.X; x < Regions.Coast.X + Regions.Coast.W; x++) old.SetBlocked(new GridPos(x, y), true);
            Assert.True(old.IsBlocked(new GridPos(Regions.Coast.X + 24, Regions.Coast.Y + 4)));
            Assert.True(Regions.EnsureCoast(old));
            Assert.False(old.IsBlocked(new GridPos(Regions.Coast.X + 24, Regions.Coast.Y + 4)));
            Assert.False(Regions.EnsureCoast(old));
        }

        [Fact]
        public void SuHavuzlari_IcerikteVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var kv in HomeLife.WaterPools)
                foreach (var c in kv.Value) Assert.True(db.Items.Contains(c.Item), c.Item);
            foreach (var n in HomeLife.HabitatNeeds)
            {
                foreach (var s in n.Substrates) Assert.True(db.Items.Get(s).HasTag("substrate"), s);
                if (n.Water != null) Assert.True(db.Items.Contains(n.Water));
            }
        }
    }
}
