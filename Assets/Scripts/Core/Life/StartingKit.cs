using Thatchwick.Core.Build;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yeni oyunun başlangıç erzakı ve çapalanmış bostanı (belge 41, denge).
    /// Ölçü: iki yetişkin günde ~240 açlık puanı yer (kişi başı 120). Erzak ilk şalgam hasadına (6 gün) ve biraz
    /// ötesine yetsin diye ~7 günlük; bostan, tarla görevlisinin ya da oyuncunun ilk gün ekebileceği 18 karo toprak.
    /// Odun ilkbahar başının soğuk günlerine (~1 kütük/gün) yeter.
    /// Eski erzak: 12 şalgam, 6 şalgam tohumu, 10 odun, çapalanmış toprak yok; boş bırakılan hane 7. gün açlıktan ölüyordu.
    /// </summary>
    public static class StartingKit
    {
        public static readonly (string itemId, int count)[] Items =
        {
            ("item.firewood", 14),
            ("item.sapling_oak", 2),
            ("item.turnip", 24),
            ("item.bread", 6),
            ("item.cabbage", 4),
            ("item.seed_turnip", 12),
            ("item.seed_radish", 6),
            ("item.plank", 6),
            ("item.nails", 4),
            ("item.compost", 3),
            ("item.salt", 4),
        };

        public const int BedWidth = 6;
        public const int BedHeight = 3;

        /// <summary>Erzakı haneye koyar (yalnızca yeni oyunda).</summary>
        public static void Apply(Household house)
        {
            foreach (var (id, count) in Items) house.AddItem(id, count);
        }

        /// <summary>
        /// Başlangıç bostanı: origin köşesinden w x h karo çimi toprağa çevirir. Arsa dışı, çim olmayan ya da üstünde
        /// eşya olan karo atlanır. Çapalanan karo sayısını döndürür.
        /// </summary>
        public static int TillBed(WorldState world, GridPos origin, int w = BedWidth, int h = BedHeight)
        {
            var map = world.Map;
            int n = 0;
            for (int y = origin.Y; y < origin.Y + h; y++)
                for (int x = origin.X; x < origin.X + w; x++)
                {
                    var p = new GridPos(x, y);
                    if (!map.InBounds(p) || !map.IsOwned(p) || map.GetFloor(p) != FloorType.Grass || world.GetItemAt(p) != null) continue;
                    map.SetFloor(p, FloorType.Dirt);
                    n++;
                }
            return n;
        }
    }
}
