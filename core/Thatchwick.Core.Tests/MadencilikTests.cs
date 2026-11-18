using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mining;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Madencilik (23. belge, 28. belgedeki sıra): kazı motoru, arsa kazısı (kat -2) ve aile ocağının galeri katı (-1).
    public class MadencilikTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public MineSystem Mines;
        }

        private static Ctx Make(int size = 16)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            var c = new Ctx { Db = db, World = new WorldState(map), House = new Household(20000) };
            c.Build = new BuildService(c.World, c.House, db);
            c.Mines = new MineSystem(c.World, c.House, seed: 5);
            return c;
        }

        private static Character Miner(int level = 2, int x = 5, int y = 5, int z = -2)
        {
            var c = new Character(1, "Agnes", new GridPos(x, y, z));
            c.SetAge(30);
            while (c.Skills.GetLevel(SkillType.Mining) < level) c.GainSkillXp(SkillType.Mining, 50);
            return c;
        }

        // Arsada (5,5)'te açık bir iniş karosu ve çevresi bilinen kayalar.
        private static MineMap PlotWithShaft(Ctx x, RockKind around = RockKind.Soil)
        {
            var m = x.Mines.EnsurePlot();
            foreach (var (tx, ty, t) in m.All()) { t.Rock = around; t.Ore = null; t.Wet = false; }
            x.World.Map.SetTile(new GridPos(5, 5, -2), true);
            x.Mines.OpenShaft(new GridPos(5, 5, -2));
            return m;
        }

        [Fact]
        public void Uretim_AyniTohumAyniKat_ArsaDerinHammaddeVermez()
        {
            var a = Make(); var b = Make();
            var ma = a.Mines.EnsurePlot(); var mb = b.Mines.EnsurePlot();
            Assert.True(ma.All().Zip(mb.All(), (p, q) => p.t.Rock == q.t.Rock && p.t.Ore == q.t.Ore).All(v => v));
            var plotOres = ma.All().Select(p => p.t.Ore).Where(o => o != null).Distinct().ToList();
            Assert.Contains("item.clay", plotOres);
            Assert.DoesNotContain("item.slate_stone", plotOres);         // arduvaz yalnızca ocakta
            var mine = a.Mines.EnsureMine();
            Assert.Equal(RockKind.Bedrock, mine.Get(0, 0).Rock);
            var (ex, ey) = MineSystem.MineEntrance;
            Assert.True(mine.IsOpen(ex, ey));
            var mineOres = mine.All().Select(p => p.t.Ore).Where(o => o != null).Distinct().ToList();
            Assert.Contains("item.iron_ore", mineOres);
            Assert.Contains("item.limestone", mineOres);
        }

        [Fact]
        public void Kazi_SertlikKazmaVeBeceri_YanindanKazilir_CikanDepoya()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            var c = Miner(2);
            Assert.NotNull(x.Mines.CanDig(m, 7, 5, c));                // açık karoya komşu değil
            m.Get(6, 5).Rock = RockKind.Stone;
            Assert.Null(x.Mines.CanDig(m, 6, 5, c));
            m.Get(5, 4).Rock = RockKind.Hard;
            Assert.Contains("kazman", x.Mines.CanDig(m, 5, 4, c).ToLower());   // temel kazma sertlik 3'e kadar
            x.Mines.PickTier = _ => 1;
            Assert.Null(x.Mines.CanDig(m, 5, 4, c));
            // Süre: vuruş x 4 dk; karanlıkta iki kat.
            double dark = x.Mines.DigDays(m, 6, 5, c);
            m.Get(5, 5).LightRadius = 2; m.Get(5, 5).LightDays = 1;
            double lit = x.Mines.DigDays(m, 6, 5, c);
            Assert.Equal(dark / 2, lit, 6);
            Assert.True(lit < x.Mines.DigDays(m, 5, 4, c));
            var got = x.Mines.Dig(m, 6, 5, c);
            Assert.Equal("item.stone", got.Single().Key);
            Assert.True(m.IsOpen(6, 5));
            Assert.True(x.World.Map.HasTile(new GridPos(6, 5, -2)));   // kazılan yer yapı moduna açılır
            Assert.Equal(1, x.House.Count("item.stone"));
            // Çocuk inmez; Madencilik 1 arsa kazamaz.
            Assert.NotNull(x.Mines.CanDig(m, 7, 5, Miner(1)));
        }

        [Fact]
        public void Damar_CevherVerir_DamarIziGorunur_Kalinti()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            m.Get(7, 5).Ore = "item.coal"; m.Get(7, 5).Rock = RockKind.Soft;
            Assert.Equal("item.coal", m.VeinHint(6, 5, 1));
            Assert.Null(m.VeinHint(4, 5, 1));
            var c = Miner(2);
            x.Mines.Dig(m, 6, 5, c);
            var got = x.Mines.Dig(m, 7, 5, c);
            Assert.Contains(got, kv => kv.Key == "item.coal");
            // Kalıntı tablosu: arsanın üç kalıntısı, eşya olarak var.
            foreach (var r in MineTable.PlotRelics.Concat(MineTable.MineRelics)) Assert.True(x.Db.Items.Contains(r), r);
        }

        [Fact]
        public void Gocuk_GevsekKaroCatirdar_DesteklenmezseCoker_DestekSusturur_OlumYok()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            // 3x3 açık alan (merkez 8,8): merkez gevşek.
            for (int ty = 7; ty <= 9; ty++) for (int tx = 7; tx <= 9; tx++) { m.Get(tx, ty).Rock = RockKind.Open; x.World.Map.SetTile(new GridPos(tx, ty, -2), true); }
            Assert.True(m.IsLoose(8, 8, 2));
            var victim = Miner(2, 8, 8);
            var hit = new List<Character>();
            x.Mines.CharactersAt = p => p == new GridPos(8, 8, -2) ? new[] { victim } : new Character[0];
            x.Mines.CaughtInCaveIn += (c, _) => hit.Add(c);
            int guard = 0;
            while (!m.Get(8, 8).Creaking && guard++ < 200) x.Mines.NewDay();
            Assert.True(m.Get(8, 8).Creaking);                             // bir gün önceden görünür
            x.Mines.NewDay();
            Assert.Equal(RockKind.Rubble, m.Get(8, 8).Rock);
            Assert.False(x.World.Map.HasTile(new GridPos(8, 8, -2)));
            Assert.Single(hit);
            Assert.False(victim.IsDead);

            // İkinci alan: çatırdayınca destek kurulur, çökmez.
            var y = Make();
            var n = PlotWithShaft(y);
            for (int ty = 7; ty <= 9; ty++) for (int tx = 7; tx <= 9; tx++) { n.Get(tx, ty).Rock = RockKind.Open; y.World.Map.SetTile(new GridPos(tx, ty, -2), true); }
            guard = 0;
            while (!n.Get(8, 8).Creaking && guard++ < 200) y.Mines.NewDay();
            y.House.AddItem("item.plank", 2);
            Assert.True(y.Mines.PlaceSupport(n, 8, 8, SupportKind.Wood));
            Assert.False(n.Get(8, 8).Creaking);
            for (int d = 0; d < 30; d++) y.Mines.NewDay();
            Assert.True(n.IsOpen(8, 8));
        }

        [Fact]
        public void EvAlti_DestekOlmadanKazilmaz()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            var c = Miner(2);
            x.World.Map.SetTile(new GridPos(6, 5, -1), true);
            x.World.Map.SetTile(new GridPos(7, 5, -1), true);
            x.World.Map.SetEdge(GridMap.EdgeBetween(new GridPos(6, 5, -1), new GridPos(7, 5, -1)), EdgeKind.Wall);
            Assert.True(x.Mines.HouseAbove(6, 5));
            Assert.Contains("destek", x.Mines.CanDig(m, 6, 5, c));
            x.House.AddItem("item.plank", 2);
            Assert.True(x.Mines.PlaceSupport(m, 5, 5, SupportKind.Wood));
            Assert.Null(x.Mines.CanDig(m, 6, 5, c));
        }

        [Fact]
        public void Su_SuluKaroDoldurur_KovaVeDrenBosaltir_DerinlikKaziyiDurdurur()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            m.Get(5, 5).Wet = true;
            x.Mines.NewDay();
            Assert.Equal(MineSystem.WetFlowPerDay, m.Water, 6);
            Assert.Equal(2, m.WaterDepth);                                 // tek açık karo: sığ su
            Assert.Contains("Su", x.Mines.CanDig(m, 6, 5, Miner(2)));
            Assert.True(x.Mines.Bail(m));
            Assert.Equal(0, m.WaterDepth);
            var pit = x.World.PlaceItem(x.Db.Items.Get("item.drain_pit"), new GridPos(5, 5, -2));
            Assert.NotNull(pit);
            Assert.Equal(1, x.Mines.DrainPits());
            Assert.Equal(0, x.Mines.TomorrowDepth(m));                     // dren günde 6 emer
            for (int d = 0; d < 5; d++) x.Mines.NewDay();
            Assert.Equal(0, m.WaterDepth);
        }

        [Fact]
        public void Ocak_AtesleKirma_SuGalerisi_VardiyaIsaretliKaroyuKazar()
        {
            var x = Make();
            var m = x.Mines.EnsureMine();
            var c = Miner(3);
            var (ex, ey) = MineSystem.MineEntrance;
            var t = m.Get(ex, ey - 2);
            t.Rock = RockKind.Bedrock; t.Ore = null;
            Assert.NotNull(x.Mines.CanDig(m, ex, ey - 2, c));
            x.House.AddItem("item.firewood", 2);
            Assert.Null(x.Mines.CanFireSet(m, ex, ey - 2));
            Assert.True(x.Mines.FireSet(m, ex, ey - 2));
            Assert.Contains("duman", x.Mines.CanDig(m, ex, ey - 2, c));
            x.Mines.Tick(0.2);
            x.Mines.NewDay();
            Assert.True(t.Cracked);
            Assert.Null(x.Mines.CanDig(m, ex, ey - 2, c));                // çatlak: bir vuruş, her kazma
            // Arsada ateş yakılmaz.
            var plot = PlotWithShaft(x, RockKind.Hard);
            x.House.AddItem("item.firewood", 2);
            Assert.Contains("ev", x.Mines.CanFireSet(plot, 6, 5));

            // Vardiya: işaretli karolar girişten başlayarak kazılır.
            var y = Make();
            var mine = y.Mines.EnsureMine();
            foreach (var (tx, ty, tt) in mine.All()) if (!tt.IsOpen && tt.Rock != RockKind.Bedrock) { tt.Rock = RockKind.Soft; tt.Ore = null; tt.Wet = false; }
            for (int k = 2; k <= 5; k++) mine.Get(ex, ey - k).Marked = true;
            int dug = y.Mines.Shift(mine, c, 0.5);
            Assert.Equal(4, dug);
            Assert.True(mine.IsOpen(ex, ey - 5));
            Assert.False(MineSystem.HasAdit(mine));
            for (int xx = ex - 1; xx >= 1; xx--) { mine.Get(xx, ey - 5).Rock = RockKind.Open; }
            Assert.True(MineSystem.HasAdit(mine));                         // batı yamaca ulaşan galeri suyu akıtır
        }

        [Fact]
        public void KuyuInisi_MerdivenleAcilir_YapiModuAkceyleKazamaz_Madencilik2()
        {
            var x = Make();
            x.Build.DeepDigBlocker = p => x.Mines.ShaftBlocker(1);
            Assert.False(x.Build.CanBuildLevelTile(new GridPos(4, 4, -2)).Ok);
            Assert.False(x.Build.CanBuildLevelTile(new GridPos(4, 4, -2), stairEnd: true).Ok);
            x.Build.DeepDigBlocker = p => x.Mines.ShaftBlocker(2);
            GridPos opened = default;
            x.Build.DeepShaftOpened += p => { opened = p; x.Mines.OpenShaft(p); };
            Assert.True(x.Build.BuildLevelTile(new GridPos(4, 4, -2), stairEnd: true).Ok);
            Assert.Equal(new GridPos(4, 4, -2), opened);
            Assert.True(x.Mines.EnsurePlot().IsOpen(4, 4));
            Assert.False(x.Build.RemoveLevelTile(new GridPos(4, 4, -2)).Ok);
        }

        [Fact]
        public void DerinKiler_BozulmaBodrumunYarisi_HazineOdasiYadigariKorur()
        {
            var x = Make();
            var spoil = new Life.SpoilageService(x.Db, x.House, x.World);
            Assert.Equal(0.5, spoil.DeepCellarFactor, 6);
            // Hazine odası: kat -2'de kapalı oda + hazine sandığı.
            for (int ty = 2; ty <= 4; ty++) for (int tx = 2; tx <= 4; tx++) x.World.Map.SetTile(new GridPos(tx, ty, -2), true);
            var chest = x.World.PlaceItem(x.Db.Items.Get("item.treasure_chest"), new GridPos(2, 2, -2));
            var relic = x.World.PlaceItem(x.Db.Items.Get("item.stool_wood"), new GridPos(4, 4, -2));
            Assert.True(MineSystem.InTreasureRoom(x.World, relic));
            x.Build.ProtectedItem = it => MineSystem.InTreasureRoom(x.World, it);
            x.Build.Demolish(2, 2, 4, 4, -2, out int removed, out int skipped);
            Assert.NotNull(x.World.GetItem(relic.Id));
            Assert.NotNull(x.World.GetItem(chest.Id));
            Assert.True(skipped >= 2);
        }

        [Fact]
        public void Gorev_Maden_CatirdayanaDestek_SonraIsaretliyiKazar_ErgenAlamaz()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            var work = new FarmWork(x.World) { House = x.House, Mine = new MineWork(x.Mines) };
            var planner = new DutyPlanner(x.World, null, null, null) { TileWork = work, Mines = x.Mines };
            var c = Miner(2, 5, 5);
            c.Duty = DutyKind.Mine;
            m.Get(6, 5).Marked = true;
            var job = planner.Next(c);
            Assert.Equal(TileAction.Dig, job.TileAction);
            Assert.Equal(new GridPos(6, 5, -2), job.Target);
            Assert.True(work.Duration(c, job.Target, TileAction.Dig) > 0);
            work.Apply(c, job.Target, TileAction.Dig);
            Assert.True(m.IsOpen(6, 5));
            m.Get(5, 5).Creaking = true;
            x.House.AddItem("item.plank", 2);
            Assert.Equal(TileAction.Support, planner.Next(c).TileAction);
            Assert.True(DutyPlanner.CanTakeDuty(c, DutyKind.Mine));
            var teen = new Character(2, "Wat", new GridPos(1, 1));
            teen.SetAge(14);
            if (teen.Stage == AgeStage.Teen) Assert.False(DutyPlanner.CanTakeDuty(teen, DutyKind.Mine));
        }

        [Fact]
        public void Kayit_KaziYerleriSuVeDestekGeriGelir()
        {
            var x = Make();
            var m = PlotWithShaft(x);
            x.House.AddItem("item.plank", 2);
            x.Mines.PlaceSupport(m, 5, 5, SupportKind.Wood);
            m.Get(6, 5).Marked = true;
            m.Water = 3;
            x.Mines.EnsureMine();
            var data = SaveGame.Capture(new GameClock(), x.World, x.House, new Character[0], null, null, null, mines: x.Mines);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, x.Db);
            var sys = new MineSystem(loaded.World, x.House, seed: 5);
            SaveGame.RestoreMines(loaded, sys);
            var r = sys.Get(MineSystem.PlotId);
            Assert.True(r.IsOpen(5, 5));
            Assert.Equal(SupportKind.Wood, r.Get(5, 5).Support);
            Assert.True(r.Get(6, 5).Marked);
            Assert.Equal(3.0, r.Water, 6);
            Assert.NotNull(sys.Get(MineSystem.MineId));
            Assert.True(loaded.World.Map.HasTile(new GridPos(5, 5, -2)));
        }
    }
}
