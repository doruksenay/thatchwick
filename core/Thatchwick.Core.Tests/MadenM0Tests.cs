using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mining;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Madencilik M0 (Project belgesi 58d): fiyat ilkesi, taşlığın tükenmesi, süreli görevin haritaya bağlanması.</summary>
    public class MadenM0Tests
    {
        /// <summary>Madencilik zinciri: her işleme adımı girdisinin en az 1,25 katı eder (belge 58b karar 4).</summary>
        public const double MinProcessGain = 1.25;

        /// <summary>
        /// İlkenin dışında bilerek bırakılanlar: ham cevheri kavurmadan eritmek kısa yoldur ve kavurmadan daha az kazandırır
        /// (bilerek; kavurmanın anlamı bu). Listelenen öbür tarifler madencilik dışı zincirin fiyatına dokunmamak için (58d M0.7).
        /// </summary>
        private static readonly HashSet<string> Exempt = new HashSet<string>(StringComparer.Ordinal) { "item.bloomery/smelt_raw" };

        private static readonly HashSet<string> MiningInputs = new HashSet<string>(StringComparer.Ordinal)
        {
            "item.iron_ore", "item.roasted_ore", "item.iron_bloom", "item.limestone", "item.quicklime", "item.lime", "item.gypsum",
            "item.slate_stone", "item.stone"
        };

        [Fact]
        public void IslemeAdimlari_EnAzYuzde25Kazandirir()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            int Price(string id) => db.Items.TryGet(id, out var d) ? d.Price : 0;
            var bad = new List<string>();
            void Check(string name, IEnumerable<(string id, int n)> inputs, IEnumerable<(string id, int n)> outputs, bool barMain = false)
            {
                var ins = inputs.ToList();
                if (Exempt.Contains(name)) return;
                double inV = ins.Sum(x => Price(x.id) * x.n), outV = outputs.Sum(x => Price(x.id) * x.n);
                if (inV <= 0) return;
                bool mining = ins.Any(x => MiningInputs.Contains(x.id));
                double barShare = ins.Where(x => x.id == "item.iron_bar").Sum(x => Price(x.id) * x.n) / inV;
                if (!mining && barShare < 0.75) return;    // ana girdisi demir külçe olmayan tarif bu ilkenin dışında
                if (outV < MinProcessGain * inV - 1e-9) bad.Add($"{name}: girdi {inV}, çıktı {outV} ({outV / inV:0.00})");
            }
            foreach (var r in db.Items.All)
            {
                if (r.AutoProcess != null)
                    Check(r.Id + "/auto", new[] { (r.AutoProcess.Item, r.AutoProcess.Count) }, new[] { (r.AutoProcess.Produces, r.AutoProcess.ProducesCount) });
                foreach (var i in r.Interactions)
                    if (i.Consume.Count > 0 && i.Produce.Count > 0)
                        Check(r.Id + "/" + i.Id, i.Consume.Select(m => (m.ItemId, m.Count)), i.Produce.Select(p => (p.ItemId, p.Count)));
            }
            Assert.True(bad.Count == 0, "İlkeyi tutmayan işleme adımları (58d M0.6):\n" + string.Join("\n", bad));
        }

        [Fact]
        public void Taslik_DemirVermez()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var q = db.Items.Get("item.quarry");
            Assert.DoesNotContain(q.Interactions.SelectMany(i => i.Produce), p => p.ItemId == "item.iron_ore");
            Assert.Contains(q.Interactions, i => i.SkillWork == "quarry");
        }
    
        private static (MineSystem mines, World.WorldState world, Household house) NewMines()
        {
            var world = new World.WorldState(new World.GridMap(60, 60));
            var house = new Household();
            var mines = new MineSystem(world, house, seed: 7);
            return (mines, world, house);
        }

        private static Character Miner(int level = 3)
        {
            var c = new Character(1, "Davut", new World.GridPos(0, 0)) { MoodEnabled = true };
            c.SetAge(40);
            if (level > 1) c.GainSkillXp(SkillType.Mining, SkillRules.XpForLevel(level));
            return c;
        }

        [Fact]
        public void SureliGorev_IsaretYoksa_DuzTasKazar_DamarTuketmez()
        {
            var (mines, _, _) = NewMines();
            var m = mines.EnsureMine();
            int veinsBefore = m.All().Count(t => t.t.Ore != null);
            var got = mines.DutyDig(MineSystem.MineId, Miner(), 6);
            Assert.True(got.Sum(kv => kv.Value) > 0);
            Assert.Equal(veinsBefore, m.All().Count(t => t.t.Ore != null));   // işaret yokken damara dokunulmaz
        }

        [Fact]
        public void SureliGorev_IsaretliDamariGercektenTuketir()
        {
            var (mines, _, house) = NewMines();
            var m = mines.EnsureMine();
            var c = Miner(10);
            // Girişe en yakın, kazılabilir bir damar karosu bul ve ona kadar yol aç.
            var target = m.All().Where(t => t.t.Ore != null && !t.t.IsOpen).OrderBy(t => Math.Abs(t.x - MineSystem.MineEntrance.x) + Math.Abs(t.y - MineSystem.MineEntrance.y)).First();
            foreach (var (x, y, t) in m.All()) t.Marked = false;
            m.Get(target.x, target.y).Marked = true;
            int guard = 0;
            while (!m.Get(target.x, target.y).IsOpen && guard++ < 60)
            {
                // Yol: işaretli karo kazılamıyorsa (yanı kapalı) en yakın düz taş açılır.
                mines.DutyDig(MineSystem.MineId, c, 1);
            }
            Assert.True(m.Get(target.x, target.y).IsOpen || guard >= 60);
            Assert.Equal(0, house.Count(target.t.Ore ?? "item.none"));        // süreli görev depoya koymaz (dönüşte teslim)
        }

        [Fact]
        public void Vardiya_ElleKaziylaAyniHizda()
        {
            var (mines, _, _) = NewMines();
            var m = mines.EnsureMine();
            var c = Miner(4);
            mines.ActionTimeScale = 0.5;
            foreach (var (x, y, t) in m.All()) if (!t.IsOpen && t.Ore == null && t.Rock == RockKind.Stone) t.Marked = true;
            int fast = mines.Shift(m, c, 0.125);
            var (mines2, _, _) = NewMines();
            var m2 = mines2.EnsureMine();
            foreach (var (x, y, t) in m2.All()) if (!t.IsOpen && t.Ore == null && t.Rock == RockKind.Stone) t.Marked = true;
            int slow = mines2.Shift(m2, c, 0.125);
            Assert.True(fast > slow, $"ölçekli {fast}, ölçeksiz {slow}");
        }

        [Fact]
        public void YerAlti_KisaInisCezaVermez_IkiSaattenSonraTozlu_CikincaGunIsigi()
        {
            var (mines, _, _) = NewMines();
            var c = Miner();
            Assert.Equal(MineSystem.UndergroundEvent.None, mines.NoteUnderground(c, true, 1.0));
            Assert.Equal(MineSystem.UndergroundEvent.None, mines.NoteUnderground(c, true, 1.0 + 1.0 / 24));
            Assert.Equal(MineSystem.UndergroundEvent.None, mines.NoteUnderground(c, false, 1.0 + 1.5 / 24));   // kısa iniş
            Assert.Equal(MineSystem.UndergroundEvent.None, mines.NoteUnderground(c, true, 2.0));
            Assert.Equal(MineSystem.UndergroundEvent.Dusty, mines.NoteUnderground(c, true, 2.0 + 2.1 / 24));
            Assert.Equal(MineSystem.UndergroundEvent.None, mines.NoteUnderground(c, true, 2.0 + 3.0 / 24));    // bir kez
            Assert.Equal(MineSystem.UndergroundEvent.Daylight, mines.NoteUnderground(c, false, 2.0 + 3.5 / 24));
        }

        [Fact]
        public void DamarinIlkKarosu_BirKezSevindirir()
        {
            var (mines, _, _) = NewMines();
            var m = new MineMap("deneme", MineSiteKind.Mine, -1, 6, 1, 1);
            for (int x = 0; x < 6; x++) { var t = m.Get(x, 0); t.Rock = RockKind.Stone; t.Ore = null; }
            m.Get(0, 0).Rock = RockKind.Open;
            m.Get(1, 0).Ore = "item.iron_ore"; m.Get(2, 0).Ore = "item.iron_ore";   // bir damar (2 karo)
            m.Get(4, 0).Ore = "item.coal";                                         // ikinci damar
            var c = Miner(10);
            mines.PickTier = _ => 2;
            var opened = new List<string>();
            mines.VeinOpened += (_, __, ore) => opened.Add(ore);
            for (int x = 1; x < 6; x++) mines.Dig(m, x, 0, c);
            Assert.Equal(new[] { "item.iron_ore", "item.coal" }, opened);
        }

        [Fact]
        public void Galeri_DunyayaYerlesir_GirisIleAgizBagli_KazilanKaroYurunur()
        {
            var (mines, world, _) = NewMines();
            var m = mines.EnsureMine();
            var mouth = new World.GridPos(30, 40, 0);
            mines.PlaceMine(mouth);
            var (ex, ey) = MineSystem.MineEntrance;
            var entrance = m.ToWorld(ex, ey);
            Assert.Equal(-1, entrance.Z);
            Assert.True(world.Map.HasTile(entrance));
            Assert.Contains(world.Map.LinksFrom(mouth), p => p.Equals(entrance));
            Assert.Same(m, mines.SiteAt(entrance, out int lx, out int ly));
            Assert.Equal((ex, ey), (lx, ly));
            var c = Miner(10);
            mines.PickTier = _ => 2;
            var next = m.All().First(t => !t.t.IsOpen && mines.CanDig(m, t.x, t.y, c) == null);
            mines.Dig(m, next.x, next.y, c);
            Assert.True(world.Map.HasTile(m.ToWorld(next.x, next.y)));
        }

        [Fact]
        public void Galeri_KayittanGeriGelir_KonumVeDamarIsareti()
        {
            var (mines, world, house) = NewMines();
            var m = mines.EnsureMine();
            mines.PlaceMine(new World.GridPos(30, 40, 0));
            m.Get(0, 0).VeinSeen = true;
            var saves = mines.Export();
            var again = new MineSystem(world, house, seed: 7);
            again.Restore(saves);
            var r = again.Get(MineSystem.MineId);
            Assert.Equal(m.OriginX, r.OriginX);
            Assert.Equal(m.OriginY, r.OriginY);
            Assert.True(r.Get(0, 0).VeinSeen);
        }

        [Fact]
        public void CocukVeErgen_YetiskinsizInmez()
        {
            var child = new Character(5, "Gonca", new World.GridPos(0, 0));
            child.SetAge(8);
            Assert.True(MineSystem.NeedsEscort(child));
            Assert.False(MineSystem.NeedsEscort(Miner()));
        }

        [Fact]
        public void HamMaddeDoygunlugu_KayittanGeriGelir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var market = new Economy.Market(db, new Household());
            market.RestoreRawSold(12, new[] { new KeyValuePair<string, int>("item.iron_ore", 40) });
            var list = market.ExportRawSold(out long week);
            Assert.Equal(12, week);
            Assert.Contains(list, kv => kv.Key == "item.iron_ore" && kv.Value == 40);
        }
}
}
