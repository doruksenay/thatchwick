using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>WorldState kimlik ve karo dizinleri (performans incelemesi 20.11.2026): doğrusal aramayla aynı sonuç.</summary>
    public class WorldIndexTests
    {
        [Fact]
        public void KimlikVeKaroDizini_YerlestirTasiKaldirSonrasiTutarli()
        {
            var world = new WorldState(new GridMap(16, 16));
            var small = new ItemDef { Id = "item.test_stool", DisplayName = "Tabure" };
            var wide = new ItemDef { Id = "item.test_table", DisplayName = "Masa", Width = 2, Height = 2 };
            var rug = new ItemDef { Id = "item.test_rug", DisplayName = "Halı", Placement = ItemPlacement.Rug, Width = 3, Height = 3 };
            world.PlaceItem(small, new GridPos(1, 1));
            world.PlaceItem(wide, new GridPos(4, 1));
            world.PlaceItem(wide, new GridPos(8, 8), 1);
            world.PlaceItem(rug, new GridPos(10, 2));
            world.PlaceItem(small, new GridPos(11, 3));
            Assert.Equal(5, world.Items.Count);
            AssertConsistent(world);

            var first = world.Items[0];
            Assert.True(world.MoveItem(first.Id, new GridPos(2, 10), 0).Ok);
            Assert.Null(world.GetItemAt(new GridPos(1, 1)));
            Assert.Same(first, world.GetItemAt(new GridPos(2, 10)));
            AssertConsistent(world);

            world.RemoveItem(world.Items[world.Items.Count - 1].Id);
            AssertConsistent(world);
            Assert.Null(world.GetItem(999999));
        }

        private static void AssertConsistent(WorldState world)
        {
            foreach (var it in world.Items) Assert.Same(it, world.GetItem(it.Id));
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                var p = new GridPos(x, y);
                var linear = world.Items.FirstOrDefault(i => !i.IsDecor && !i.IsRug && i.OccupiedTiles().Contains(p));
                Assert.Same(linear, world.GetItemAt(p));
            }
        }
    }
}
