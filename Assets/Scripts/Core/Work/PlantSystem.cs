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

        /// <summary>Hava ve köy projelerinden gelen büyüme çarpanı (yağmur +, kuyu +); yoksa 1.</summary>
        public Func<float> GrowthMultiplier { get; set; }

        /// <summary>Bitki bu mevsimde büyür mü: kış hiç; ekin türünün growSeasons listesi varsa yalnızca onlar.</summary>
        public static bool CanGrow(Content.ItemDef def, Season season) =>
            CanGrowIn(season) && (def.GrowSeasons.Count == 0 || def.GrowSeasons.Contains(season));

        /// <summary>Kışın donan ekin kalktığında (eşya tanımı, konum) ile tetiklenir.</summary>
        public event Action<Content.ItemDef, GridPos> PlantDied;

        public void Tick(double gameDays, Season season)
        {
            if (gameDays <= 0) return;
            if (!CanGrowIn(season))
            {
                KillWinterCrops();
                return;
            }
            var rooms = _world.Map.Rooms;
            float mult = GrowthMultiplier != null ? Math.Max(0f, GrowthMultiplier()) : 1f;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant || item.IsGrown) continue;
                if (!CanGrow(item.Def, season)) continue;      // bu ekin bu mevsimde büyümez
                var room = rooms.RoomAt(item.Position);
                if (room != null && room.IsIndoor) continue;   // içeride büyümez
                item.GrowthDays = Math.Min(item.Def.GrowDays, item.GrowthDays + gameDays * mult);
            }
        }

        private void KillWinterCrops()
        {
            var dead = new System.Collections.Generic.List<ItemInstance>();
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant || !item.Def.DiesInWinter) continue;
                var room = _world.Map.Rooms.RoomAt(item.Position);
                if (room != null && room.IsIndoor) continue;
                dead.Add(item);
            }
            foreach (var item in dead)
            {
                _world.RemoveItem(item.Id);
                PlantDied?.Invoke(item.Def, item.Position);
            }
        }

        /// <summary>Bitkiyi olgun yapar (haritadaki yabani ağaçlar, test).</summary>
        public static void SetGrown(ItemInstance item)
        {
            if (item.IsPlant) item.GrowthDays = item.Def.GrowDays;
        }
    }
}
