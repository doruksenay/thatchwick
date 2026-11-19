using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Tilki, yaban domuzu, karga hediyesi, karacanın doğaya dönmesi, av aleti tarifleri (belge 31).
    public class YabanEtkiTests
    {
        private sealed class Ctx { public ContentDatabase Db; public WorldState World; public Household House; public AnimalSystem Animals; public WildAdoptions Adopt; public WildMischief M; }

        private static Ctx Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var w = new WorldState(map);
            var animals = new AnimalSystem(w, db) { DiseaseEnabled = false };
            var ad = new WildAdoptions(1);
            return new Ctx { Db = db, World = w, House = new Household(100), Animals = animals, Adopt = ad, M = new WildMischief(animals, w, ad, 2) };
        }

        private static Animal Put(Ctx x, string species, int id, double age) =>
            x.Animals.Restore(id, x.Db.Animals.Get(species), new GridPos(2 + id % 10, 2), 0, 100f, age, 0, 0, 50f);

        private static void Adopt(WildAdoptions ad, string id, int day = 0)
        {
            var l = new System.Collections.Generic.List<string>(ad.Export()) { "?" + id };
            ad.Import(l.ToArray());
            Assert.True(ad.Decide(true, null, day));
        }

        [Fact]
        public void Tilki_OnceCivcivKapar_KopekVeTilkiYavrusuKorur()
        {
            var x = Make();
            var hen = Put(x, "animal.chicken", 10, 20);
            var chick = Put(x, "animal.chicken", 11, 0.5);
            var prey = x.M.FoxNight(Season.Summer, out var msg, force: true);
            Assert.Same(chick, prey);
            Assert.Equal(AnimalDeath.Predator, chick.CauseOfDeath);
            Assert.Contains("kaptı", msg);

            Put(x, "animal.dog", 20, 30);
            Assert.Null(x.M.FoxNight(Season.Summer, out msg, force: true));
            Assert.Contains("köpek", msg.ToLowerInvariant());

            var y = Make();
            Put(y, "animal.chicken", 10, 20);
            Adopt(y.Adopt, "wild.fox");
            Assert.Null(y.M.FoxNight(Season.Summer, out msg, force: true));
            Assert.Contains("tilki", msg);
            y.Animals.Released = false;
            Assert.Null(y.M.FoxNight(Season.Summer, out msg, force: true));
            Assert.Null(msg);   // kümes kapalı: tilki gelmez
        }

        [Fact]
        public void Domuz_EkinCigner_CitVeKopekOnler()
        {
            var x = Make();
            var seed = x.Db.Items.All.First(i => i.IsCrop);
            for (int i = 0; i < 4; i++) x.World.PlaceItem(seed, new GridPos(5 + i, 10));
            Assert.Equal(2, x.M.BoarVisit(Season.Autumn, out var msg, force: true));
            Assert.Contains("çiğnendi", msg);
            Assert.Equal(2, x.World.Items.Count(it => it.IsPlant && it.Def.IsCrop));

            for (int i = 0; i < WildWorld.PenFenceEdges; i++)
                x.World.Map.SetEdge(GridMap.EdgeBetween(new GridPos(i, 12), new GridPos(i, 13)), EdgeKind.Fence);
            Put(x, "animal.dog", 20, 30);
            Assert.Equal(0, x.M.BoarVisit(Season.Autumn, out msg, force: true));
            Assert.Contains("sokmadı", msg);
        }

        [Fact]
        public void Karga_HediyeGetirir_YoksaGetirmez()
        {
            var x = Make();
            Assert.Null(x.M.CrowGift(x.House, force: true));
            Adopt(x.Adopt, "wild.crow");
            int akce = x.House.Akce;
            var gift = x.M.CrowGift(x.House, force: true);
            Assert.NotNull(gift);
            Assert.True(x.House.Akce > akce || x.House.Count("item.mussel_shell") > 0);
        }

        [Fact]
        public void Karaca_BuyuyunceDogayaDoner_UgrarVeKayitlanir()
        {
            var ad = new WildAdoptions(5);
            Adopt(ad, "wild.roe_deer", 10);
            Adopt(ad, "wild.hedgehog", 10);
            Assert.Null(ad.OnGrowUp(12, Season.Spring));
            var back = new WildAdoptions(); back.Import(ad.Export());
            Assert.Equal("returned", back.OnGrowUp(10 + WildAdoptions.RoeReturnDays, Season.Summer));
            Assert.False(back.Has("wild.roe_deer"));
            Assert.True(back.Has("wild.hedgehog"));   // kirpi kalır
            Assert.Contains("wild.roe_deer", back.Returned);
            var again = new WildAdoptions(5); again.Import(back.Export());
            Assert.Contains("wild.roe_deer", again.Returned);
            bool visited = false;
            for (int d = 0; d < 100 && !visited; d++) visited = again.OnGrowUp(30 + d, Season.Spring) == "visit";
            Assert.True(visited);
            Assert.Null(again.OnGrowUp(200, Season.Winter));
            // Eski kayıt (gün bilgisi yok) da açılır.
            var old = new WildAdoptions(); old.Import(new[] { "wild.fox" });
            Assert.True(old.FoxGuard);
        }

        [Fact]
        public void Tarifler_AvAletleriVeOrmeCit()
        {
            var x = Make();
            var bench = x.Db.Items.Get("item.workbench");
            foreach (var id in new[] { "craft:hunting_bow", "craft:snare", "craft:wattle_hurdle" })
                Assert.Contains(bench.Interactions, i => i.Id == id);
        }
    }
}
