using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
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

        private static void Give(Kit k, params string[] ids) { foreach (var id in ids) k.House.AddItem(id, 1); }

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
            Assert.Contains("zemini", k.Home.CanAddToViv(g, "item.viv_moss"));
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Moist));
            Assert.True(k.Home.AddToViv(g, "item.viv_moss"));
            Assert.NotNull(k.Home.SetHabitat(g, HomeLife.Dry));
            Assert.Equal(0, k.House.Count("item.viv_moss"));
        }

        [Fact]
        public void TuzluSu_TuzIster()
        {
            var k = Make();
            var g = Garden(k);
            Assert.NotNull(k.Home.SetHabitat(g, HomeLife.Salt));
            Give(k, "item.salt");
            Assert.Null(k.Home.SetHabitat(g, HomeLife.Salt));
            Assert.Equal(0, k.House.Count("item.salt"));
        }

        [Fact]
        public void OlusmamasiGerekenKombinasyonlar_Reddedilir()
        {
            var k = Make();
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
            Give(k, "item.viv_firefly", "item.viv_frog", "item.viv_spider", "item.viv_spider", "item.viv_lizard", "item.viv_seaweed");

            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_lizard"));      // kertenkele kuru kum ister
            Assert.NotNull(k.Home.CanAddToViv(g, "item.viv_seaweed"));     // deniz yosunu tuzlu su ister
            Assert.True(k.Home.AddToViv(g, "item.viv_firefly"));
            Assert.Contains("zehir", k.Home.CanAddToViv(g, "item.viv_frog"));
            Assert.True(k.Home.AddToViv(g, "item.viv_spider"));
            Assert.Contains("İki", k.Home.CanAddToViv(g, "item.viv_spider"));
        }

        [Fact]
        public void Kapasite_BitkiVeCanliAyriSayilir()
        {
            var k = Make();
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
            for (int i = 0; i < 4; i++) Give(k, "item.viv_moss");
            for (int i = 0; i < 3; i++) Assert.True(k.Home.AddToViv(g, "item.viv_moss"));
            Assert.Contains("Bitki yeri dolu", k.Home.CanAddToViv(g, "item.viv_moss"));
            Give(k, "item.viv_worm");
            Assert.True(k.Home.AddToViv(g, "item.viv_worm"));
        }

        [Fact]
        public void Uyari_KimKimiYer()
        {
            var k = Make();
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
            Give(k, "item.viv_aphid", "item.viv_ladybird");
            k.Home.AddToViv(g, "item.viv_aphid");
            var w = k.Home.AddWarnings(g, "item.viv_ladybird");
            Assert.Contains(w, s => s.Contains("yaprak biti yer"));
        }

        [Fact]
        public void BesinZinciri_UgurBocegiYaprakBitiniBitirir_BitkiKurtulur()
        {
            var k = Make(11);
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_fern", "item.viv_aphid", "item.viv_aphid", "item.viv_ladybird", "item.viv_worm");
            foreach (var id in new[] { "item.viv_moss", "item.viv_fern", "item.viv_aphid", "item.viv_aphid", "item.viv_ladybird", "item.viv_worm" })
                Assert.True(k.Home.AddToViv(g, id), id);
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 10; d++) { v.Moisture = 80; k.Home.VivariumDay(); }
            Assert.DoesNotContain(v.Entries, e => e.Item == "item.viv_aphid");
            Assert.Contains(v.Entries, e => e.Item == "item.viv_moss");
            Assert.Contains(v.Log, s => s.Contains("yedi"));
        }

        [Fact]
        public void YaprakBiti_AvcisizCogalir_BitkiyiKurutur()
        {
            var k = Make(3);
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
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
            k.Home.SetHabitat(g, HomeLife.Moist);
            Give(k, "item.viv_moss", "item.viv_snail");
            k.Home.AddToViv(g, "item.viv_moss");
            k.Home.AddToViv(g, "item.viv_snail");
            var v = k.Home.VivOf(g);
            for (int d = 0; d < 15 && v.Entries.Count > 0; d++) { k.Home.VivariumTick(1); k.Home.VivariumDay(); }
            Assert.Empty(v.Entries);
            Assert.True(v.Mold > 0);
            Assert.Contains(v.Log, s => s.Contains("kurudu") || s.Contains("öldü"));
        }

        [Fact]
        public void Nemlendirme_EvGorevidir_VeKuruKumdaFazlasiYapilmaz()
        {
            var k = Make();
            var g = Garden(k);
            k.Home.SetHabitat(g, HomeLife.Moist);
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
            k.Home.SetHabitat(g, HomeLife.Moist);
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
            k.Home.SetHabitat(g, HomeLife.Moist);
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
            k.Home.SetHabitat(g, HomeLife.Moist);
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
    }
}
