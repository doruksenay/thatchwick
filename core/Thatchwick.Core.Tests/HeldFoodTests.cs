using Thatchwick.Core.Build;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Yemek animasyonunda elde görünen yiyecek: neyi yiyorsa o (kullanıcı isteği 18.11.2026).
    public class HeldFoodTests
    {
        [Fact]
        public void ElmaYiyorsaElma_SepettenYiyorsaSiradakiMeyve_YemekDegilseYok()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var world = new WorldState(new GridMap(12, 12));
            var pot = world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(2, 2));
            Assert.Equal("item.apple", HeldFood.Of(Job.UseItem(pot.Id, "eat_apple"), world, db));
            Assert.Equal("item.egg", HeldFood.Of(Job.UseItem(pot.Id, "eat_egg"), world, db));

            var table = world.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(5, 5));
            var basket = world.PlaceOnSurface(db.Items.Get("item.fruit_basket"), table.Id, "b");
            Assert.Null(HeldFood.Of(Job.UseItem(basket.Id, "eat_one"), world, db));     // boş sepet
            basket.Contents.Add(new StoredItem("item.pear", 1.0, false));
            basket.Contents.Add(new StoredItem("item.plum", 3.0, false));
            Assert.Equal("item.plum", HeldFood.Of(Job.UseItem(basket.Id, "eat_one"), world, db));   // en eskisi
            Assert.Equal("item.plum", DecorSystem.EatOne(basket, db));
            Assert.Equal("item.pear", HeldFood.Of(Job.UseItem(basket.Id, "eat_one"), world, db));

            Assert.Null(HeldFood.Of(Job.UseItem(basket.Id, "fill"), world, db));              // yemek işi değil
            Assert.Null(HeldFood.Of(Job.MoveTo(new GridPos(1, 1)), world, db));
            Assert.Null(HeldFood.Of(null, world, db));
        }
    }
}
