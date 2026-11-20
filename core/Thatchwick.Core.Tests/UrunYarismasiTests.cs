using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Şenlik ürün yarışmaları (Project belgesi 57): her şenlikte tadım, Hasat Şenliği'nde sergi; puan kaliteden.
    public class UrunYarismasiTests
    {
        private static ContentDatabase Db() => ContentFilesTests.LoadDb(out _);

        [Fact]
        public void Icerik_HerSenliginYarismasiVar_KurdeleHatira()
        {
            var db = Db();
            Assert.Equal(db.Festivals.All.Count(), ProduceShowService.Shows.Length);
            foreach (var s in ProduceShowService.Shows) Assert.True(db.Festivals.Contains(s.FestivalId), s.FestivalId);
            Assert.Equal(ProduceShowKind.Exhibit, ProduceShowService.ForFestival("festival.harvest").Kind);
            foreach (var id in new[] { ProduceShowService.TastingRibbon, ProduceShowService.ExhibitRosette })
                Assert.True(db.Items.Get(id).IsKeepsake, id);
        }

        [Fact]
        public void Tadim_EnIyisiSunulur_UstalikEseriKazanir_UrunTadilir()
        {
            var db = Db();
            var h = new Household();
            h.AddItem("item.cheese", 3);
            h.AddItem("item.cheese", 1, ItemQuality.Masterwork);
            h.AddItem("item.honey", 2, ItemQuality.Fine);
            var svc = new ProduceShowService(db, h);
            var dairy = ProduceShowService.Get("dairy");
            var planned = svc.PlannedEntries(dairy).Single();
            Assert.Equal(("item.cheese", ItemQuality.Masterwork), (planned.Id, planned.Quality));
            int akce = h.Akce;
            var r = svc.Run("festival.fair", seedOverride: 1);
            Assert.Equal(1, r.Rank);
            Assert.Equal(ProduceShowService.RivalCount + 1, r.Entrants);
            Assert.Equal(akce + ProduceShowService.TastingAkce[0], h.Akce);
            Assert.Equal(0, h.Count("item.cheese", ItemQuality.Masterwork));   // jüri tadar
            Assert.Equal(3, h.Count("item.cheese"));
            Assert.Equal(1, svc.Wins("dairy"));

            // Oyuncu başka ürün seçebilir; katılmazsa hiçbir şey harcanmaz.
            svc.Choose("dairy", new ItemKey("item.honey", ItemQuality.Fine));
            Assert.Equal("item.honey", svc.PlannedEntries(dairy).Single().Id);
            svc.SetParticipating("dairy", false);
            Assert.Equal(0, svc.Run("festival.fair", 2).Rank);
            Assert.Equal(2, h.Count("item.honey"));
        }

        [Fact]
        public void Tadim_SiradanUrunYillarSonraZorKazanir_KaliteFarkYaratir()
        {
            var db = Db();
            int Wins(int quality, int year)
            {
                int wins = 0;
                for (int seed = 0; seed < 60; seed++)
                {
                    var h = new Household();
                    h.AddItem("item.stew_plain", 1, quality);
                    var svc = new ProduceShowService(db, h) { Year = () => year };
                    if (svc.Run("festival.winter_night", seed).Rank == 1) wins++;
                }
                return wins;
            }
            int common = Wins(ItemQuality.Common, 6), superior = Wins(ItemQuality.Superior, 6), masterwork = Wins(ItemQuality.Masterwork, 6);
            Assert.True(common < superior && superior <= masterwork, $"{common} < {superior} <= {masterwork}");
            Assert.True(common <= 5, common.ToString());
            Assert.True(masterwork >= 58, masterwork.ToString());
            Assert.InRange(superior, 15, 50);                                   // has orta oyunda yarı yarıya
        }

        [Fact]
        public void Sergi_CesitlilikOnce_EnCokDokuz_UrunlerEveDoner()
        {
            var db = Db();
            var h = new Household();
            foreach (var id in new[] { "item.turnip", "item.cabbage", "item.leek", "item.carrot", "item.onion", "item.apple", "item.pear", "item.plum", "item.cherry", "item.lettuce" })
                h.AddItem(id, 2, ItemQuality.Superior);
            h.AddItem("item.cheese", 1);
            h.AddItem("item.pickled_cabbage", 1);
            h.AddItem("item.stew_plain", 1);
            var svc = new ProduceShowService(db, h);
            var exhibit = ProduceShowService.Get("exhibit");
            var planned = svc.PlannedEntries(exhibit);
            Assert.Equal(ProduceShowService.ExhibitSlots, planned.Count);
            var cats = planned.Select(k => ProduceShowService.Category(db.Items.Get(k.Id))).Distinct().ToList();
            Assert.Contains("animal", cats); Assert.Contains("preserved", cats); Assert.Contains("cooked", cats); Assert.Contains("produce", cats);
            int before = h.InventoryList().Sum(kv => kv.Value);
            var r = svc.Run("festival.harvest", 3);
            Assert.True(r.Rank >= 1);
            Assert.Equal(before, h.InventoryList().Sum(kv => kv.Value));     // sergi eve döner
        }

        [Fact]
        public void Bag_SarapPaketsizSunulmaz_PekmezSunulur()
        {
            var db = Db();
            var h = new Household();
            h.AddItem("item.wine_new", 1, ItemQuality.Heirloom);
            h.AddItem("item.pekmez", 1);
            var svc = new ProduceShowService(db, h);
            bool was = ContentPacks.Adult;
            try
            {
                ContentPacks.Adult = false;
                Assert.Equal("item.pekmez", svc.PlannedEntries(ProduceShowService.Get("grape")).Single().Id);
                ContentPacks.Adult = true;
                Assert.Equal("item.wine_new", svc.PlannedEntries(ProduceShowService.Get("grape")).Single().Id);   // dede şarabı
            }
            finally { ContentPacks.Adult = was; }
        }

        [Fact]
        public void Kayit_SecimKatilmamaBirincilikGeriGelir()
        {
            var db = Db();
            var h = new Household();
            h.AddItem("item.stew_plain", 1, ItemQuality.Masterwork);
            var svc = new ProduceShowService(db, h);
            svc.Choose("feast", new ItemKey("item.stew_plain", ItemQuality.Masterwork));
            svc.SetParticipating("preserves", false);
            svc.Run("festival.winter_night", 1);
            var json = JsonSerializer.Serialize(svc.Export(), new JsonSerializerOptions { IncludeFields = true });
            var back = new ProduceShowService(db, new Household());
            back.Import(JsonSerializer.Deserialize<ProduceShowSave>(json, new JsonSerializerOptions { IncludeFields = true }));
            Assert.False(back.Participating("preserves"));
            Assert.Equal(svc.Wins("feast"), back.Wins("feast"));
        }

        [Fact]
        public void Senlik_BirincilikteKurdeleHatirasiVeItibar()
        {
            var db = Db();
            var pop = new Population();
            var house = new Household(10);
            var ali = new Character(1, "Ali", new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            ali.SetAge(40);
            pop.Add(ali);
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), house, pop, new GameClock(), 3);
            life.Houses.EnsureHouses();
            house.AddItem("item.pickled_cabbage", 1, ItemQuality.Heirloom);
            int rep = life.Upbringing.Reputation.Points;
            var r = life.JudgeShow("festival.seed");
            Assert.NotNull(r);
            if (r.Rank == 1)
            {
                Assert.Equal(1, house.Count(ProduceShowService.TastingRibbon));
                Assert.Contains(ali.Chronicle, n => n.Contains("show.chr.won"));
            }
            Assert.True(life.Upbringing.Reputation.Points >= rep);
            Assert.Null(life.JudgeShow("festival.unknown"));
        }
    }
}
