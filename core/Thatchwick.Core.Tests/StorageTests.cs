using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class StorageTests
    {
        private static (ContentDatabase db, WorldState w, Household h, StorageService s, BuildService b) Setup(int baseCap = 10, int akce = 100)
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "log", DisplayName = "Kütük", Placement = ItemPlacement.Resource, Price = 1 });
            db.Items.Register(new ItemDef { Id = "chest", DisplayName = "Sandık", Price = 10, Storage = 40 });
            db.Items.Register(new ItemDef { Id = "chair", DisplayName = "Sandalye", Price = 5 });
            db.Items.Register(new ItemDef
            {
                Id = "oak", DisplayName = "Meşe", GrowDays = 1f,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "chop", DisplayName = "Kes", DurationDays = 0.1, RemovesItem = true,
                        Produce = new List<ProduceDef> { new ProduceDef { ItemId = "log", Count = 5 } } }
                }
            });
            Assert.Empty(db.Validate());
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household(akce);
            var s = new StorageService(w, h, db, baseCap);
            return (db, w, h, s, new BuildService(w, h, db) { Storage = s });
        }

        [Fact]
        public void Kapasite_TemelBolgeVeSandiktanOlusur()
        {
            var (db, w, _, s, b) = Setup();
            Assert.Equal(10, s.Capacity);
            Assert.True(b.SetZone(new GridPos(1, 1), ZoneType.Storage).Ok);
            Assert.True(b.SetZone(new GridPos(1, 2), ZoneType.Storage).Ok);
            Assert.Equal(30, s.Capacity);
            w.PlaceItem(db.Items.Get("chest"), new GridPos(5, 5));
            Assert.Equal(70, s.Capacity);
            Assert.True(b.SetZone(new GridPos(1, 1), ZoneType.None).Ok);
            Assert.Equal(60, s.Capacity);
        }

        [Fact]
        public void YalnizcaKaynaklarSayilir()
        {
            var (_, _, h, s, _) = Setup();
            h.AddItem("log", 4);
            h.AddItem("chair", 3);
            Assert.Equal(4, s.Used);
            Assert.Equal(6, s.Free);
        }

        [Fact]
        public void DepoDoluyken_KaynakSatinAlinmaz_MobilyaAlinir()
        {
            var (_, _, h, _, b) = Setup(baseCap: 2);
            Assert.True(b.Buy("log").Ok);
            Assert.True(b.Buy("log").Ok);
            var r = b.Buy("log");
            Assert.False(r.Ok);
            Assert.Contains("Depo dolu", r.Reason);
            Assert.True(b.Buy("chair").Ok);
            Assert.Equal(2, h.Count("log"));
        }

        [Fact]
        public void Uretim_KapasiteyeKadarEklenir_FazlasiKaybolur()
        {
            var (db, w, h, s, _) = Setup(baseCap: 3);
            var fx = new InteractionEffects(w, h, db) { Storage = s };
            string summary = null;
            fx.Applied += (_, __, sm) => summary = sm;
            var tree = w.PlaceItem(db.Items.Get("oak"), new GridPos(3, 3));
            fx.Apply(new Character(1, "A", new GridPos(0, 0)), tree, db.Items.Get("oak").Interactions[0]);
            Assert.Equal(3, h.Count("log"));
            Assert.Contains("+3 Kütük", summary);
            Assert.Contains("2 adet kayboldu", summary);
        }

        [Fact]
        public void KapasiteAzalinca_EldekiKaybolmaz()
        {
            var (_, _, h, s, b) = Setup(baseCap: 0);
            b.SetZone(new GridPos(1, 1), ZoneType.Storage);
            h.AddItem("log", 8);
            b.SetZone(new GridPos(1, 1), ZoneType.None);
            Assert.Equal(8, h.Count("log"));
            Assert.Equal(0, s.Free);
            Assert.Equal(0, s.AddUpTo("log", 3));
        }

        [Fact]
        public void Bolge_YalnizcaArsadaCizilir()
        {
            var (_, w, _, _, b) = Setup();
            w.Map.SetOwned(new GridPos(4, 4), false);
            Assert.False(b.SetZone(new GridPos(4, 4), ZoneType.Storage).Ok);
            Assert.False(b.SetZone(new GridPos(40, 4), ZoneType.Storage).Ok);
        }

        [Fact]
        public void Icerik_DepoKapasitesiDogrulanir()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "a", DisplayName = "x", Storage = -1 });
            db.Items.Register(new ItemDef { Id = "b", DisplayName = "x", Storage = 5, Placement = ItemPlacement.Surface });
            Assert.Equal(2, db.Validate().Count);
        }
    }
}
