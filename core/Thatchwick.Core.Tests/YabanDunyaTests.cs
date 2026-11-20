using System;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Öküz türü, yaban ziyaretçileri, tuzak, atlama engelleri, ağıl/ateş algısı, kurucu yatkınlığı.
    public class YabanDunyaTests
    {
        private sealed class Ctx { public ContentDatabase Db; public WorldState World; public Household House; public AnimalSystem Animals; }

        private static Ctx Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var w = new WorldState(map);
            return new Ctx { Db = db, World = w, House = new Household(5000), Animals = new AnimalSystem(w, db) { DiseaseEnabled = false } };
        }

        private static Character Adult(int id, SkillType? skill = null, int level = 1)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(40);
            if (skill.HasValue && level > 1) c.Skills.RestoreXp(skill.Value, SkillRules.XpForLevel(level));
            return c;
        }

        [Fact]
        public void Okuz_AyriTur_HepErkek_Uremez_KutukCeker()
        {
            var x = Make();
            var ox = x.Db.Animals.Get("animal.ox");
            Assert.True(ox.Draft);
            Assert.True(ox.MaleOnly);
            Assert.False(ox.Breeds);
            var a = x.Animals.Restore(5, ox, new GridPos(2, 2), 0, 100f, 30, 0, 0, 50f, female: true);
            Assert.False(a.IsFemale);
            Assert.Equal("öküz", a.SexText);
            Assert.Null(AnimalContests.Blocker(ContestKind.LogPull, a, Adult(1)));
            Assert.Equal("boğa", x.Db.Animals.Get("animal.cow").MaleName);
        }

        [Fact]
        public void Ziyaretci_GelirDeftereYazilirCikar()
        {
            var map = new GridMap(12, 12);
            var j = new NatureJournal();
            var r = new WildRoamers(map, 3) { Journal = j };
            string first = null;
            r.FirstSeen += s => first = s.Id;
            var v = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
            Assert.NotNull(v);
            Assert.Equal("wild.rabbit", first);
            Assert.True((j.Get("wild.rabbit") & JournalMark.Seen) != 0);
            Assert.Same(v, r.At(v.Position));
            for (int i = 0; i < 400 && r.Visitors.Count > 0; i++) r.Tick(0.01, 1 + i * 0.01, Season.Summer, 0.5);
            Assert.Empty(r.Visitors);
            Assert.DoesNotContain(WildRoamers.Candidates(Season.Winter, false), s => s.Id == "wild.wolf" || s.Id == "wild.bear");
            Assert.Contains(WildRoamers.Candidates(Season.Summer, true), s => s.Id == "wild.hedgehog");
        }

        [Fact]
        public void Ziyaretci_SevilirVeAvlanir()
        {
            var x = Make();
            var r = new WildRoamers(x.World.Map, 4) { Journal = new NatureJournal() };
            var c = Adult(1, SkillType.AnimalHusbandry, 10);
            bool petted = false;
            for (int i = 0; i < 30 && !petted; i++)
            {
                var v = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
                petted = r.TryPet(v.Id, c).Contains("kaçmadı");
            }
            Assert.True(petted);
            Assert.True((r.Journal.Get("wild.rabbit") & JournalMark.Petted) != 0);
            var fox = r.Arrive(WildCatalog.Get("wild.fox"), 0);
            Assert.Contains("sevilmez", r.TryPet(fox.Id, c));

            var hunter = Adult(2, SkillType.Hunting, 10);
            bool got = false;
            for (int i = 0; i < 30 && !got; i++)
            {
                var rab = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
                got = r.Shoot(rab.Id, hunter, x.House, true).Success;
            }
            Assert.True(got);
            Assert.True(x.House.Count("item.game_meat") >= 1);
            Assert.True((r.Journal.Get("wild.rabbit") & JournalMark.Hunted) != 0);
        }

        /// <summary>At yarışı bahsi (+18 paketi; karar 20.11.2026): karttan at seçilir, tutan bahis şansa göre öder.</summary>
        [Fact]
        public void YarisBahsi_AtSecilir_TutanBahisOder()
        {
            bool pack = ContentPacks.Adult;
            try
            {
                var house = new Household(100);
                var bets = new RaceBets(7);
                Assert.Null(bets.Card("festival.harvest", 10, 0, null, null));           // hasatta at yarışı yok
                var card = bets.Card("festival.fair", 10, 0, null, new[] { "Demirciler" });
                Assert.Equal(AnimalContests.Rivals, card.Runners.Count);
                Assert.Same(card, bets.Card("festival.fair", 10, 0, null, new[] { "Demirciler" }));   // kart gün içinde sabit
                Assert.Equal(1.0, card.Runners.Sum(r => r.Chance), 6);
                Assert.True(card.Runners.Last().Chance > card.Runners.First().Chance);   // favori sonda
                Assert.True(RaceBets.PayoutFor(card.Runners.First(), 10) > RaceBets.PayoutFor(card.Runners.Last(), 10));   // az şans çok öder

                ContentPacks.Adult = false;
                Assert.Equal("bet.err.pack", bets.Place(card, 0, 10, house).Key);
                ContentPacks.Adult = true;
                Assert.Equal("bet.err.stake", bets.Place(card, 0, 7, house).Key);
                Assert.Equal("bet.err.akce", bets.Place(card, 0, 50, new Household(20)).Key);
                Assert.True(bets.Place(card, 4, 10, house).IsEmpty);
                Assert.Equal(90, house.Akce);
                Assert.Equal("bet.err.already", bets.Place(card, 1, 5, house).Key);

                // Kayıt: bahis yüklenir.
                var loaded = new RaceBets(7);
                loaded.Import(bets.Export());
                Assert.Equal(card.Runners[4].Name, loaded.Current.RunnerName);
                Assert.Equal(10, loaded.Current.Stake);
                Assert.Equal(card.Runners[4].Chance, loaded.Current.Chance, 9);

                // Yarışlar: kazanan hep rakiplerden biri; tutan bahis öder, tutmayan gider.
                int won = 0, lost = 0, paid = 0;
                for (int n = 0; n < 60; n++)
                {
                    var b = new RaceBets(100 + n);
                    var h = new Household(10);
                    var c = b.Card("festival.fair", 10, 0, null, null);
                    Assert.True(b.Place(c, 4, 10, h).IsEmpty);
                    BetOutcome outcome = null;
                    b.Settled += o => outcome = o;
                    string winner = b.Resolve(c, 0, h);
                    Assert.Contains(winner, RaceBets.RivalHorses);
                    Assert.NotNull(outcome);
                    Assert.Null(b.Current);
                    if (outcome.Won) { won++; paid = outcome.Payout; Assert.Equal(outcome.Payout, h.Akce); Assert.Equal(c.Runners[4].Name, winner); }
                    else { lost++; Assert.Equal(0, h.Akce); }
                }
                Assert.True(won > 0 && lost > 0, $"kazanan {won}, kaybeden {lost}");
                Assert.Equal(AnimalContests.BetPayout(10, card.Runners[4].Chance), paid);

                // Yarış koşulmadan günü geçen bahis iade edilir.
                var stale = new RaceBets(1); var purse = new Household(10);
                Assert.True(stale.Place(stale.Card("festival.fair", 10, 0, null, null), 0, 5, purse).IsEmpty);
                Assert.Equal(0, stale.RefundStale(10, purse));
                Assert.Equal(5, stale.RefundStale(11, purse));
                Assert.Equal(10, purse.Akce);
            }
            finally { ContentPacks.Adult = pack; }
        }

        /// <summary>Kement haritaya kurulur (karar 20.11.2026): arsaya ya da yabana, en çok 3; av tuzakta bekler, biri toplar.</summary>
        [Fact]
        public void Kement_HaritayaKurulur_AvTuzaktaBekler()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            map.RestrictOwnership(0, 0, 10, 10);   // arsa: çiftliğin köşesi
            var w = new WorldState(map);
            var house = new Household(100);
            var build = new BuildService(w, house, db);
            house.AddItem("item.snare", 5);

            var forest = new GridPos(Regions.Forest.X + 10, Regions.Forest.Y + 10);
            var town = new GridPos(Regions.Town.X + 10, Regions.Town.Y + 10);
            Assert.Equal("build.err.snare_ground", build.CanPlaceFromInventory("item.snare", town, 0).Reason);   // şehre kurulmaz
            Assert.True(build.PlaceFromInventory("item.snare", forest, 0, out var s1).Ok);
            Assert.True(build.PlaceFromInventory("item.snare", new GridPos(forest.X + 3, forest.Y), 0, out var s2).Ok);
            Assert.True(build.PlaceFromInventory("item.snare", new GridPos(5, 5), 0, out var s3).Ok);             // kendi arsası
            Assert.Equal("build.err.snare_limit", build.CanPlaceFromInventory("item.snare", new GridPos(forest.X, forest.Y + 3), 0).Reason);
            Assert.Equal(2, house.Count("item.snare"));

            var r = new WildRoamers(map, 9) { Journal = new NatureJournal() };
            var hunter = Adult(1, SkillType.Hunting, 8);
            int caught = 0;
            for (int d = 0; d < 30; d++) caught += r.CheckSnares(Season.Autumn, w.Items, hunter).Count;
            Assert.InRange(caught, 1, WildRoamers.MaxSnares);                 // dolu tuzak toplanana dek yeniden tutmaz
            Assert.Equal(0, house.Count("item.game_meat") + house.Count("item.hide"));   // av eve kendiliğinden gelmez
            var full = new[] { s1, s2, s3 }.First(s => s.StockTotal > 0);

            var fx = new Thatchwick.Core.Work.InteractionEffects(w, house, db, seed: 4);
            var collect = db.Items.Get("item.snare").Interactions.Single(i => i.Id == "check_snare");
            Assert.True(collect.Collect);
            fx.Apply(hunter, full, collect);
            Assert.Equal(0, full.StockTotal);
            Assert.True(house.Count("item.game_meat") + house.Count("item.hide") > 0);
            caught = 0;
            for (int d = 0; d < 30 && caught == 0; d++) caught += r.CheckSnares(Season.Autumn, new[] { full }, hunter).Count;
            Assert.Equal(1, caught);                                          // boşalan tuzak yine tutar
        }

        [Fact]
        public void Atlama_CitVeAlcakEsya()
        {
            var x = Make();
            var from = new GridPos(5, 5);
            x.World.Map.SetEdge(GridMap.EdgeBetween(from, new GridPos(6, 5)), EdgeKind.Fence);
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(5, 6));
            var spots = JumpObstacles.Find(x.World, from);
            Assert.Contains(spots, s => s.Landing == new GridPos(6, 5) && s.Tiles == 2 && s.Label == "çit");
            Assert.Contains(spots, s => s.Landing == new GridPos(5, 7) && s.Tiles == 2);
            // İki engel üst üste: 3 karo.
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(4, 5));
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(3, 5));
            Assert.Contains(JumpObstacles.Find(x.World, from), s => s.Landing == new GridPos(2, 5) && s.Tiles == 3);
        }

        [Fact]
        public void Agil_VeAtes_KurtSavunmasinaGirer()
        {
            var x = Make();
            Assert.False(WildWorld.HasPen(x.World.Map));
            for (int i = 0; i < WildWorld.PenFenceEdges; i++)
                x.World.Map.SetEdge(GridMap.EdgeBetween(new GridPos(i, 8), new GridPos(i, 9)), EdgeKind.Fence);
            Assert.True(WildWorld.HasPen(x.World.Map));
            Assert.False(WildWorld.HasOutdoorFire(x.World));
            x.World.PlaceItem(x.Db.Items.Get("item.bonfire"), new GridPos(12, 12));
            Assert.True(WildWorld.HasOutdoorFire(x.World));
            var wolves = new WolfSystem(x.Animals, 1) { Fenced = () => WildWorld.HasPen(x.World.Map), HasFire = () => WildWorld.HasOutdoorFire(x.World), Shepherds = () => 1 };
            Assert.Equal(2, wolves.DefenseScore());   // çoban 1 + ateş 1
        }

        [Fact]
        public void Kurucu_YatkinlikSecer()
        {
            var d = new CharacterDraft();
            // Belge 45 karar 3: kurucu en çok 1 yatkınlık seçer.
            Assert.True(d.ToggleAptitude(SkillType.Carpentry));
            Assert.False(d.ToggleAptitude(SkillType.Riding));
            Assert.True(d.ToggleAptitude(SkillType.Carpentry));   // kaldırır
            Assert.Empty(d.Aptitudes);
            Assert.True(d.ToggleAptitude(SkillType.Riding));
            Assert.Single(d.Aptitudes);
        }
    }
}
