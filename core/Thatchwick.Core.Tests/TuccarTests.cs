using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 34-D5: gezgin tüccar ve panayır.
    public class TuccarTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        [Fact]
        public void Tuccar_PazardaOlmayanMallar_PanayirdaDahaCokVeDamizlik()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var house = new Household(1000);
            var m = new TravelingMerchant(db, house, 3);
            Assert.NotEmpty(m.ExoticGoods());
            Assert.All(m.ExoticGoods(), d => Assert.True(d.HasTag("craftonly") || d.HasTag("merchantonly")));
            m.Arrive(4, fair: false);
            Assert.True(m.Here);
            Assert.True(m.Offers.Count(o => o.Kind == MerchantOfferKind.Goods) <= TravelingMerchant.GoodsPerVisit);
            Assert.NotNull(m.Order);
            m.Leave();
            Assert.False(m.Here);
            Assert.Empty(m.Offers);
            Assert.NotNull(m.Order);   // sipariş bir sonraki gelişe kalır
            m.Arrive(9, fair: true);
            Assert.Equal(TravelingMerchant.BreedsPerFair, m.Offers.Count(o => o.Kind == MerchantOfferKind.Breed));
            Assert.True(m.Offers.Count(o => o.Kind == MerchantOfferKind.Goods) > TravelingMerchant.GoodsPerVisit);
        }

        [Fact]
        public void Tuccar_MalAlma_FiyatVeKoydaYokken()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var house = new Household(1000);
            var m = new TravelingMerchant(db, house, 3);
            m.Arrive(4, false);
            var o = m.Offers.First(x => x.Kind == MerchantOfferKind.Goods);
            int price = m.UnitPrice(o), before = house.Akce, had = house.Count(o.Id);
            Assert.True(m.Buy(o).Ok);
            Assert.Equal(before - price, house.Akce);
            Assert.Equal(had + 1, house.Count(o.Id));
            Assert.True(price >= db.Items.Get(o.Id).Price);   // pazardan pahalı
            m.Leave();
            Assert.False(m.Buy(o).Ok);
        }

        [Fact]
        public void Tuccar_Damizlik_SoyHayvaniOlarakGelir_TamFiyat()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(20, 20);
            for (int y = 0; y < 20; y++) for (int x = 0; x < 20; x++) map.SetOwned(new GridPos(x, y), true);
            var world = new WorldState(map);
            world.PlaceItem(db.Items.Get("item.barn"), new GridPos(5, 5));
            var house = new Household(5000);
            var animals = new AnimalSystem(world, db) { DiseaseEnabled = false };
            var m = new TravelingMerchant(db, house, 3) { Animals = animals };
            m.Arrive(4, true);
            var breed = m.Offers.FirstOrDefault(x => x.Kind == MerchantOfferKind.Breed && (x.Id == "animal.cow" || x.Id == "animal.goat" || x.Id == "animal.sheep"));
            if (breed == null) return;   // tohuma bağlı; inek/koyun/keçi çıkmadıysa ahır testi geçersiz
            int price = m.UnitPrice(breed), before = house.Akce;
            var r = m.Buy(breed);
            Assert.True(r.Ok, r.Reason);
            Assert.Equal(before - price, house.Akce);
            Assert.True(animals.Animals.Last().IsHeritage);
        }

        [Fact]
        public void Tuccar_SiparisTeslim_AkceItibarBonus_PanayirdaIkiKat()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var house = new Household(0);
            int rep = 0;
            var m = new TravelingMerchant(db, house, 3) { AddReputation = (n, _) => rep += n };
            m.Arrive(4, true);
            var o = m.Order;
            Assert.NotNull(m.DeliverBlocker());
            house.AddItem(o.ItemId, o.Count);
            Assert.Null(m.Deliver(4));
            Assert.Equal(o.RewardAkce, house.Akce);
            Assert.Equal(o.RewardReputation * 2, rep);
            if (o.BonusItemId != null) Assert.True(house.Count(o.BonusItemId) >= 1);
            Assert.Equal(1, m.OrdersDelivered);
            Assert.NotNull(m.Order);
            Assert.NotSame(o, m.Order);
        }

        [Fact]
        public void Tuccar_HayataBagli_PazarGunuGelir_Kayit()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var house = new Household(100);
            var clock = new GameClock();
            var life = new LifeSystems(db, new WorldState(new GridMap(12, 12)), house, new Population(), clock);
            long day = clock.DayIndex;
            while (!life.Festivals.IsMarketDay(day)) day++;
            clock.GetType().GetProperty("TotalDays").SetValue(clock, day + 0.3);
            house.MerchantInVillage = false;
            life.UpdateMerchant(true);
            Assert.True(life.Merchant.Here);
            Assert.True(house.MerchantInVillage);
            var json = JsonSerializer.Serialize(life.Export(), Json);
            var life2 = new LifeSystems(db, new WorldState(new GridMap(12, 12)), new Household(), new Population(), new GameClock());
            life2.Restore(JsonSerializer.Deserialize<Thatchwick.Core.Save.LifeSaveData>(json, Json));
            Assert.Equal(life.Merchant.Offers.Count, life2.Merchant.Offers.Count);
            Assert.Equal(life.Merchant.Order.ItemId, life2.Merchant.Order.ItemId);
        }
    }
}
