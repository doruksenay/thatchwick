using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // T5 (akçe akışı) ve T6 (NPC ile takas) çekirdek testleri.
    public class MarketTests
    {
        private static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "fuel" } });
            db.Items.Register(new ItemDef { Id = "log", DisplayName = "Kütük", Placement = ItemPlacement.Resource, Price = 3, Tags = new List<string> { "fuel" } });
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "food" } });
            db.Items.Register(new ItemDef { Id = "sapling", DisplayName = "Fidan", Placement = ItemPlacement.Resource, Price = 5 });
            db.Items.Register(new ItemDef { Id = "chair", DisplayName = "Sandalye", Price = 10 });
            Assert.Empty(db.Validate());
            return db;
        }

        private static (Market m, Household h, StorageService s) Setup(int akce = 100, int cap = 100)
        {
            var db = Content();
            var h = new Household(akce);
            var s = new StorageService(new WorldState(new GridMap(4, 4)), h, db, cap);
            var m = new Market(db, h, s, null, seed: 4);
            m.Tick(0, 0, Season.Spring);
            return (m, h, s);
        }

        [Fact]
        public void Pazar_YalnizcaKaynaklariIsler()
        {
            var (m, _, _) = Setup();
            var ids = m.Goods().Select(g => g.Id).ToList();
            Assert.Contains("log", ids);
            Assert.DoesNotContain("chair", ids);
            Assert.False(m.Buy("chair").Ok);
        }

        [Fact]
        public void Satis_AkceVerir_HerBirimFiyatiDusurur_TalepZamanlaToparlanir()
        {
            var (m, h, _) = Setup(akce: 0);
            h.AddItem("log", 30);
            Assert.Equal(2, m.SellPrice("log"));             // 3 * 0.6 = 1.8 → 2 (yuvarlanır)
            h.AddItem("sapling", 20);
            int saplingBefore = m.SellPrice("sapling");     // 5 * 0.6 = 3
            Assert.Equal(3, saplingBefore);
            int earned = m.Sell("sapling", 15);
            Assert.True(earned < 15 * 3, $"azalan fiyat bekleniyordu: {earned}");
            Assert.Equal(earned, h.Akce);
            Assert.True(m.SellPrice("sapling") < saplingBefore);
            Assert.True(m.Demand("sapling") >= 0.3);
            m.Tick(10, 10, Season.Spring);
            Assert.Equal(1.0, m.Demand("sapling"), 6);
            Assert.Equal(saplingBefore, m.SellPrice("sapling"));
        }

        [Fact]
        public void Kisin_YakitVeYiyecekDahaDegerli()
        {
            var (m, _, _) = Setup();
            int springFuel = m.BuyPrice("log"), springFood = m.BuyPrice("turnip"), springSap = m.BuyPrice("sapling");
            m.Tick(0.01, 31, Season.Winter);
            Assert.True(m.BuyPrice("log") > springFuel);
            Assert.True(m.BuyPrice("turnip") > springFood);
            Assert.Equal(springSap, m.BuyPrice("sapling"));
        }

        [Fact]
        public void Alis_AkceVeDepoyuDenetler()
        {
            var (m, h, _) = Setup(akce: 5, cap: 2);
            Assert.True(m.Buy("firewood", 2).Ok);
            Assert.Equal(1, h.Akce);
            Assert.False(m.Buy("firewood").Ok);   // depo dolu
            var (m2, h2, _) = Setup(akce: 1);
            Assert.False(m2.Buy("log").Ok);       // akçe yetmiyor
            Assert.Equal(1, h2.Akce);
        }

        [Fact]
        public void Teklifler_HerGunYenilenir_AyniGundeAyni()
        {
            var (m, _, _) = Setup();
            Assert.Equal(3, m.Offers.Count);
            var day0 = m.Offers.Select(o => m.Describe(o)).ToList();
            m.Tick(0.3, 0.3, Season.Spring);
            Assert.Equal(day0, m.Offers.Select(o => m.Describe(o)).ToList());
            m.Tick(1, 1.3, Season.Spring);
            Assert.NotEqual(day0[0] + day0[1] + day0[2], string.Concat(m.Offers.Select(o => m.Describe(o))));
            Assert.Contains(m.Offers, o => o.IsContract);
            Assert.Contains(m.Offers, o => !o.IsContract);
        }

        [Fact]
        public void KoyIsi_PazardaSatmaktanKarli_VeBirKezAlinir()
        {
            var (m, h, _) = Setup(akce: 0);
            var job = m.Offers.First(o => o.IsContract);
            h.AddItem(job.GiveItem, job.GiveCount * 2);
            Assert.True(job.RewardAkce > m.SellPrice(job.GiveItem) * job.GiveCount);
            Assert.True(m.Accept(job.Id).Ok);
            Assert.Equal(job.RewardAkce, h.Akce);
            Assert.Equal(job.GiveCount, h.Count(job.GiveItem));
            Assert.False(m.Accept(job.Id).Ok);
        }

        [Fact]
        public void Takas_MalVerirMalAlir_EksikseReddeder()
        {
            var (m, h, _) = Setup();
            var barter = m.Offers.First(o => !o.IsContract);
            var r = m.Accept(barter.Id);
            Assert.False(r.Ok);
            Assert.Contains("yetmiyor", r.Reason);
            h.AddItem(barter.GiveItem, barter.GiveCount);
            Assert.True(m.Accept(barter.Id).Ok);
            Assert.Equal(0, h.Count(barter.GiveItem));
            Assert.Equal(barter.GetCount, h.Count(barter.GetItem));
            Assert.NotEqual(barter.GiveItem, barter.GetItem);
        }

        [Fact]
        public void Takas_DepoDoluysaReddeder()
        {
            var (m, h, s) = Setup(cap: 3);
            var barter = m.Offers.First(o => !o.IsContract);
            h.AddItem(barter.GiveItem, barter.GiveCount);
            if (barter.GetCount > s.Free + barter.GiveCount)
                Assert.False(m.Accept(barter.Id).Ok);
        }

        [Fact]
        public void T5_AkceAkisi_IsKazanciEveDonusur()
        {
            // Bir günlük döngü: oyuncu kütük satıp köy işini yapar, kazandığı akçeyle sandalye alır.
            var db = Content();
            var h = new Household(0);
            var world = new WorldState(new GridMap(8, 8));
            var s = new StorageService(world, h, db, 200);
            var m = new Market(db, h, s, null, seed: 4);
            var build = new BuildService(world, h, db) { Storage = s };
            m.Tick(0, 0, Season.Spring);
            foreach (var g in m.Goods()) h.AddItem(g.Id, 20);
            foreach (var o in m.Offers.Where(o => o.IsContract).ToList()) m.Accept(o.Id);
            m.Sell("log", 10);
            Assert.True(h.Akce >= 10, $"akçe {h.Akce}");
            Assert.True(build.Buy("chair").Ok);
            Assert.True(build.PlaceFromInventory("chair", new GridPos(2, 2), 0, out _).Ok);
        }
    }
}
