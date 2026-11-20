using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Balık tutma mini oyunu (Project belgesi 50; kullanıcı 20.11.2026): oyuncu kendisi tutunca oyun, görevli kendiliğinden.
    public class BalikTutmaTests
    {
        /// <summary>İyi oyuncu: vurunca basar, çubuğun ortasını balığa yaklaştırır.</summary>
        private static FishingGame Play(FishingGame g, bool track = true, bool pressBite = true, int maxSteps = 4000)
        {
            for (int i = 0; i < maxSteps && !g.Done; i++)
            {
                bool hold = track && g.BarPos + g.BarSize / 2f + g.BarVelocity * 0.3f < g.FishPos;
                bool pressed = pressBite && g.Phase == FishingPhase.Bite;
                g.Step(1f / 60f, hold, pressed);
            }
            return g;
        }

        [Fact]
        public void IyiOyuncu_KolayBaligiTutar()
        {
            int caught = 0;
            for (int seed = 1; seed <= 10; seed++)
                if (Play(new FishingGame("item.fish_carp", 30, FishBehavior.Smooth, 3, 0f, seed)).Phase == FishingPhase.Caught) caught++;
            Assert.True(caught >= 9, caught.ToString());
        }

        [Fact]
        public void HicBasmayan_ZorBaligiKacirir()
        {
            for (int seed = 1; seed <= 5; seed++)
            {
                var g = Play(new FishingGame("item.fish_pike", 70, FishBehavior.Mixed, 1, 0f, seed), track: false);
                Assert.Equal(FishingPhase.Escaped, g.Phase);
                Assert.False(g.Perfect);
            }
        }

        [Fact]
        public void Vurunca_GecBasan_Kacirir()
        {
            var g = Play(new FishingGame("item.fish_carp", 30, FishBehavior.Smooth, 3, 0f, 4), pressBite: false);
            Assert.Equal(FishingPhase.Escaped, g.Phase);
            Assert.True(g.MissedBite);
        }

        [Fact]
        public void Cubuk_BeceriyleBuyur_Sinirli()
        {
            var low = new FishingGame("x", 30, FishBehavior.Smooth, 1);
            var high = new FishingGame("x", 30, FishBehavior.Smooth, 10, 0.1f);
            Assert.Equal(FishingGame.BaseBar, low.BarSize, 1e-4f);
            Assert.True(high.BarSize > low.BarSize);
            Assert.True(high.BarSize <= FishingGame.MaxBar + 1e-4f);
        }

        [Fact]
        public void Zorluk_TutmaOraniniDusurur()
        {
            int easy = 0, hard = 0;
            for (int seed = 1; seed <= 20; seed++)
            {
                if (Play(new FishingGame("a", 15, FishBehavior.Sinker, 1, 0f, seed)).Phase == FishingPhase.Caught) easy++;
                if (Play(new FishingGame("b", 95, FishBehavior.Darting, 1, 0f, seed)).Phase == FishingPhase.Caught) hard++;
            }
            Assert.True(easy > hard, $"{easy} / {hard}");
        }

        [Fact]
        public void Kavanoz_BeklemedenCekmeyeBaslar_AyniTohumAyniSonuc()
        {
            var g = new FishingGame("item.viv_minnow", 15, FishBehavior.Darting, 2, 0.08f, 9, skipWait: true);
            Assert.Equal(FishingPhase.Reeling, g.Phase);
            var a = Play(new FishingGame("t", 50, FishBehavior.Mixed, 4, 0f, 77));
            var b = Play(new FishingGame("t", 50, FishBehavior.Mixed, 4, 0f, 77));
            Assert.Equal(a.Phase, b.Phase);
            Assert.Equal(a.Progress, b.Progress, 1e-5f);
        }

        [Fact]
        public void IcerikBaliklari_ZorlukVeDavranisTasir()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.True(db.Items.Get("item.fish_eel").FishDifficulty > db.Items.Get("item.fish_crayfish").FishDifficulty);
            Assert.Equal(FishBehavior.Sinker, FishingGame.ParseBehavior(db.Items.Get("item.fish_eel").FishBehavior));
            Assert.True(db.Items.Get("item.fish_herring").HasTag("fish"));
        }

        // ---------- oyuna bağlantı ----------

        private static Character Adult(GridPos at)
        {
            var c = new Character(1, "Wat", at) { MoodEnabled = true };
            c.SetAge(30);
            return c;
        }

        private static void RunUntilIdle(Character c, JobRunner r) { for (int i = 0; i < 4000 && r.Queue.Current != null; i++) { c.Tick(0.002); r.Tick(0.002); } }

        [Fact]
        public void Kiyidan_OyuncuOltasi_VuranBalikOyunaGider_SonraEnvantere()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(10, 10);
            var water = new GridPos(5, 5);
            map.SetFloor(water, FloorType.Water);
            map.SetSpring(water);
            map.RestorePond(water, 1f, spring: true, insects: true, filledDays: 30, new[] { new KeyValuePair<string, int>("item.fish_carp", 10) });
            var world = new WorldState(map);
            var house = new Household(0);
            var fw = new FarmWork(world) { House = house, Content = db, TemperatureAt = _ => 20f, Season = () => Season.Summer };
            var c = Adult(new GridPos(1, 1));
            var r = new JobRunner(c, world) { TileWork = fw };
            fw.PlayerFishing = _ => true;
            string bit = null;
            fw.FishBite += (ch, t, fish) => bit = fish;
            for (int i = 0; i < 20 && bit == null; i++) { r.Command(Job.OnTile(water, TileAction.Fish)); RunUntilIdle(c, r); }
            Assert.Equal("item.fish_carp", bit);
            Assert.Equal(0, house.Count("item.fish_carp"));          // henüz oyunda
            int before = map.PondAt(water).Fish["item.fish_carp"];
            fw.CompleteFish(c, water, bit, caught: false, perfect: false);
            Assert.Equal(0, house.Count("item.fish_carp"));
            fw.CompleteFish(c, water, bit, caught: true, perfect: true);
            Assert.Equal(1, house.Count("item.fish_carp"));
            Assert.Equal(before - 1, map.PondAt(water).Fish["item.fish_carp"]);

            // Görevli (oyuncu değil): eskisi gibi kendiliğinden
            fw.PlayerFishing = _ => false;
            bit = null;
            for (int i = 0; i < 30 && house.Count("item.fish_carp") < 2; i++) { r.Command(Job.OnTile(water, TileAction.Fish)); RunUntilIdle(c, r); }
            Assert.Null(bit);
            Assert.Equal(2, house.Count("item.fish_carp"));
        }

        [Fact]
        public void Kavanoz_SuKenarinda_DenizdeDenizSuyu_GoldeGolSuyu()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            var sea = new GridPos(Regions.Coast.X + 10, Regions.Coast.Y + 25);
            var lake = new GridPos(Regions.Lake.X + 26, Regions.Lake.Y + 20);
            foreach (var p in new[] { sea, lake }) { map.SetFloor(p, FloorType.Water); map.SetSpring(p); map.RestorePond(p, 1f, spring: true, insects: true, filledDays: 30, new KeyValuePair<string, int>[0]); }
            var world = new WorldState(map);
            var house = new Household(0);
            var clock = new GameClock();
            clock.Restore(10.5, 1);   // yaz
            var home = new HomeLife(world, db, house, clock) { Rng = new System.Random(3) };
            var fw = new FarmWork(world) { House = house, Content = db };
            fw.JarBlocker = (t, fill) => home.WaterJarBlocker(t, fill);
            fw.JarFill = t => home.FillAtWater(t);
            fw.JarCatch = (ch, t) => home.CatchAtWater(ch, t);
            var c = Adult(sea);

            Assert.NotNull(fw.CanDo(c, sea, TileAction.JarFill));   // kavanoz yok
            house.AddItem(HomeLife.CatchJar, 1);
            Assert.Null(fw.CanDo(c, sea, TileAction.JarFill));
            string why = fw.CanDo(c, sea, TileAction.JarFill);
            Assert.True(why == null, why);
            string note = null;
            fw.Worked += (ch, t, n) => note = n;
            fw.Apply(c, sea, TileAction.JarFill);
            Assert.True(note != null && note.Length > 0, "not:" + note);
            fw.Apply(c, lake, TileAction.JarFill);
            Assert.Equal(1, house.Count(HomeLife.SeaWaterItem));
            Assert.Equal(1, house.Count(HomeLife.FreshWaterItem));

            var seaGot = new HashSet<string>();
            for (int i = 0; i < 40; i++) { var id = home.CatchAtWater(null, sea); if (id != null) seaGot.Add(id); }
            Assert.Contains("item.viv_seaweed", seaGot);                                   // yosun gerçekten denizden
            Assert.All(seaGot, id => Assert.True(db.Items.Get(id).Viv?.Habitats.Contains(HomeLife.Salt) != false, id));
            var lakeGot = new HashSet<string>();
            for (int i = 0; i < 40; i++) { var id = home.CatchAtWater(null, lake); if (id != null) lakeGot.Add(id); }
            Assert.DoesNotContain("item.viv_seaweed", lakeGot);
            Assert.Contains(lakeGot, id => db.Items.Get(id).Viv?.Move == "swim");
        }

        [Fact]
        public void Kavanoz_OyuncuBalikYakalarsaOyunAcilir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            var lake = new GridPos(Regions.Lake.X + 26, Regions.Lake.Y + 20);
            map.SetFloor(lake, FloorType.Water);
            var world = new WorldState(map);
            var house = new Household(0);
            var clock = new GameClock();
            clock.Restore(10.5, 1);
            var home = new HomeLife(world, db, house, clock) { Rng = new System.Random(5) };
            var c = Adult(lake);
            home.PlayerCatches = (ch, id) => ch == c;
            string bite = null;
            home.CatchBite += (ch, id) => bite = id;
            string got = null;
            for (int i = 0; i < 200 && bite == null; i++) got = home.CatchAtWater(c, lake);
            Assert.NotNull(bite);
            Assert.True(home.IsSwimmer(bite));
            Assert.Equal(0, house.Count(bite));
            home.GrantCatch(bite);
            Assert.Equal(1, house.Count(bite));
        }

        [Fact]
        public void Deniz_Donmaz_GolDonar()
        {
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            var sea = new GridPos(Regions.Coast.X + 10, Regions.Coast.Y + 25);
            var lake = new GridPos(Regions.Lake.X + 26, Regions.Lake.Y + 20);
            foreach (var p in new[] { sea, lake }) { map.SetFloor(p, FloorType.Water); map.SetSpring(p); map.RestorePond(p, 1f, spring: true, insects: true, filledDays: 30, new KeyValuePair<string, int>[0]); }
            map.WaterFrozen = true;
            Assert.True(map.FrozenAt(lake));
            Assert.False(map.FrozenAt(sea));
            Assert.True(map.IsOpenWater(sea));
            var fw = new FarmWork(new WorldState(map));
            var c = Adult(sea);
            Assert.Null(fw.CanDo(c, sea, TileAction.Bathe));
            Assert.NotNull(fw.CanDo(c, sea, TileAction.CutIce));
            Assert.Null(fw.CanDo(c, lake, TileAction.CutIce));
        }

        [Fact]
        public void KiyiCanlilari_YalnizcaKiyida()
        {
            Assert.DoesNotContain(Thatchwick.Core.Animals.WildRoamers.Candidates(Season.Summer, false), sp => sp.Id == "wild.gull" || sp.Id == "wild.crab");
            Assert.DoesNotContain(Thatchwick.Core.Animals.WildRoamers.Candidates(Season.Summer, false, "farm"), sp => sp.Id == "wild.gull");
            var coast = Thatchwick.Core.Animals.WildRoamers.Candidates(Season.Summer, false, "coast");
            Assert.Contains(coast, sp => sp.Id == "wild.gull");
            Assert.Contains(coast, sp => sp.Id == "wild.crab");
            Assert.DoesNotContain(coast, sp => sp.Id == "wild.rabbit");

            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            Regions.BuildTerrain(map);
            Regions.Apply(map);
            var roam = new Thatchwick.Core.Animals.WildRoamers(map, 5);
            var gull = roam.Arrive(Thatchwick.Core.Animals.WildCatalog.Get("wild.gull"), 0);
            Assert.NotNull(gull);
            Assert.True(Regions.Coast.Contains(gull.Position), gull.Position.ToString());
        }
    }
}
