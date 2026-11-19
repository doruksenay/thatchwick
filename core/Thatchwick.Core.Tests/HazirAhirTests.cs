using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Kullanıcı kararı 19.11.2026: büyük hayvanlar için hazır ahır kalkar (oda + Ahır bölmesi); küçük hayvanların hazır barınakları kalır.
    public class HazirAhirTests
    {
        [Fact]
        public void HazirAhir_KatalogdaVeDukkandaYok_AlinamazAmaYerlesikOlanCalisir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var barn = db.Items.Get("item.barn");
            Assert.True(barn.IsRetired);

            foreach (var c in ItemCatalog.Categories)
                Assert.DoesNotContain(ItemCatalog.InCategory(db, c.id), d => d.Id == "item.barn");

            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(10000);
            var build = new BuildService(world, house, db) { AccountLevel = () => 99 };
            Assert.DoesNotContain(build.ShopList(), d => d.Id == "item.barn");
            var buy = build.Buy("item.barn");
            Assert.False(buy.Ok);
            Assert.Contains("Ahır bölmesi", buy.Reason);
            Assert.False(build.Order("item.barn").Ok);
            Assert.Equal(10000, house.Akce);

            // Eski kayıttaki yerleşik ahır barınak olmayı sürdürür.
            var placed = world.PlaceItem(barn, new GridPos(4, 4));
            Assert.NotNull(placed);
            Assert.True(placed.Def.IsHousing);
        }

        [Fact]
        public void HazirAhir_KucukHayvanBarinaklariVeOdaYoluSatilir()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var id in new[] { "item.coop", "item.pigsty", "item.kennel", "item.cat_basket", "item.beehive", "item.stall", "item.horse_stall", "item.pig_pen", "item.nest_box" })
                Assert.False(db.Items.Get(id).IsRetired, id);
            var all = ItemCatalog.Categories.SelectMany(c => ItemCatalog.InCategory(db, c.id)).Select(d => d.Id).ToList();
            Assert.Contains("item.stall", all);
            Assert.Contains("item.coop", all);
        }
    }
}
