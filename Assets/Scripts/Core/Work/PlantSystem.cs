using System;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// Bitki büyümesi: bitkiler yalnızca dış mekânda (oda içinde değil) ve kış dışında büyür.
    /// Olgunlaşan bitki (ör. meşe) kesilebilir hale gelir.
    /// </summary>
    public sealed class PlantSystem
    {
        private readonly WorldState _world;

        public PlantSystem(WorldState world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public static bool CanGrowIn(Season season) => season != Season.Winter;

        public void Tick(double gameDays, Season season)
        {
            if (gameDays <= 0 || !CanGrowIn(season)) return;
            var rooms = _world.Map.Rooms;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant || item.IsGrown) continue;
                var room = rooms.RoomAt(item.Position);
                if (room != null && room.IsIndoor) continue;   // içeride büyümez
                item.GrowthDays = Math.Min(item.Def.GrowDays, item.GrowthDays + gameDays);
            }
        }

        /// <summary>Bitkiyi olgun yapar (haritadaki yabani ağaçlar, test).</summary>
        public static void SetGrown(ItemInstance item)
        {
            if (item.IsPlant) item.GrowthDays = item.Def.GrowDays;
        }
    }
}
