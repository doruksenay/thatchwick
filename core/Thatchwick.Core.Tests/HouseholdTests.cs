using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class HouseholdTests
    {
        private static (BuildService svc, Household house, WorldState world) Setup(int akce)
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "table", DisplayName = "Masa", Width = 2, Height = 1, Price = 40, Category = "furniture",
                SurfaceSlots = new List<SurfaceSlotDef> { new SurfaceSlotDef { Id = "a", U = 0.3f, V = 0.5f } } });
            db.Items.Register(new ItemDef { Id = "candle", DisplayName = "Mum", Placement = ItemPlacement.Surface, Price = 5, Category = "decor" });
            db.Items.Register(new ItemDef { Id = "bedroll", DisplayName = "Yer yatağı", Price = 0 });
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household(akce);
            return (new BuildService(world, house, db), house, world);
        }

        [Fact]
        public void Satinal_AkceDuserEnvanterArtar()
        {
            var (s, h, _) = Setup(100);
            Assert.True(s.Buy("table").Ok);
            Assert.Equal(60, h.Akce);
            Assert.Equal(1, h.Count("table"));
        }

        [Fact]
        public void Satinal_AkceYetmezse_Reddeder()
        {
            var (s, h, _) = Setup(10);
            var r = s.Buy("table");
            Assert.False(r.Ok);
            Assert.Equal(10, h.Akce);
            Assert.Equal(0, h.Count("table"));
        }

        [Fact]
        public void Satinal_FiyatsizEsyaSatilmaz()
        {
            var (s, _, _) = Setup(100);
            Assert.False(s.Buy("bedroll").Ok);
            Assert.False(s.Buy("yok").Ok);
        }

        [Fact]
        public void Sat_FiyatinYarisiniVerir()
        {
            var (s, h, _) = Setup(100);
            s.Buy("table");
            Assert.True(s.SellFromInventory("table").Ok);
            Assert.Equal(80, h.Akce);
            Assert.False(s.SellFromInventory("table").Ok);
        }

        [Fact]
        public void Yerlestir_EnvandenDuserDuenyayaGirer()
        {
            var (s, h, w) = Setup(100);
            s.Buy("table");
            var r = s.PlaceFromInventory("table", new GridPos(2, 2), 0, out var item);
            Assert.True(r.Ok);
            Assert.Equal(0, h.Count("table"));
            Assert.NotNull(w.GetItem(item.Id));
        }

        [Fact]
        public void Yerlestir_GecersizseEnvanterDegismez()
        {
            var (s, h, _) = Setup(100);
            s.Buy("table");
            var r = s.PlaceFromInventory("table", new GridPos(9, 9), 0, out var item);
            Assert.False(r.Ok);
            Assert.Null(item);
            Assert.Equal(1, h.Count("table"));
        }

        [Fact]
        public void Yerlestir_EnvanterdeOlmayaniReddeder()
        {
            var (s, _, _) = Setup(100);
            Assert.False(s.PlaceFromInventory("table", new GridPos(1, 1), 0, out _).Ok);
        }

        [Fact]
        public void Dekor_MasayaKonur_YuvaDoluysaReddedilir()
        {
            var (s, h, _) = Setup(100);
            s.Buy("table"); s.Buy("candle"); s.Buy("candle");
            s.PlaceFromInventory("table", new GridPos(2, 2), 0, out var table);
            Assert.True(s.PlaceDecorFromInventory("candle", table.Id, "a", out _).Ok);
            Assert.False(s.PlaceDecorFromInventory("candle", table.Id, "a", out _).Ok);
            Assert.Equal(1, h.Count("candle"));
        }

        [Fact]
        public void EnvantereAl_DekorlarlaBirlikteGeriGelir()
        {
            var (s, h, w) = Setup(100);
            s.Buy("table"); s.Buy("candle");
            s.PlaceFromInventory("table", new GridPos(2, 2), 0, out var table);
            s.PlaceDecorFromInventory("candle", table.Id, "a", out _);
            Assert.True(s.StoreToInventory(table.Id).Ok);
            Assert.Equal(1, h.Count("table"));
            Assert.Equal(1, h.Count("candle"));
            Assert.Empty(w.Items);
            Assert.False(w.Map.IsBlocked(new GridPos(2, 2)));
        }

        [Fact]
        public void YerlesikEsyayiSat_DekorlarDaSatilir()
        {
            var (s, h, w) = Setup(100);
            s.Buy("table"); s.Buy("candle");           // 55 akçe kaldı
            s.PlaceFromInventory("table", new GridPos(2, 2), 0, out var table);
            s.PlaceDecorFromInventory("candle", table.Id, "a", out _);
            Assert.True(s.SellPlaced(table.Id).Ok);
            Assert.Equal(55 + 20 + 2, h.Akce);         // 40*0.5 + floor(5*0.5)
            Assert.Empty(w.Items);
        }

        [Fact]
        public void Tasi_PasstroughVeBasarisizsaYerindeKalir()
        {
            var (s, _, w) = Setup(100);
            s.Buy("table");
            s.PlaceFromInventory("table", new GridPos(2, 2), 0, out var table);
            Assert.False(s.Move(table.Id, new GridPos(9, 9), 0).Ok);
            Assert.Equal(new GridPos(2, 2), w.GetItem(table.Id).Position);
            Assert.True(s.Move(table.Id, new GridPos(5, 5), 1).Ok);
        }

        [Fact]
        public void Magaza_SadeceFiyatliEsyalariListeler()
        {
            var (s, _, _) = Setup(0);
            var ids = s.ShopList().Select(d => d.Id).ToList();
            Assert.Contains("table", ids);
            Assert.DoesNotContain("bedroll", ids);
        }
    }
}
