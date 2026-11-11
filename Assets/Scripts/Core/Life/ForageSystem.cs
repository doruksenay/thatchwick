using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Mevsimlik toplama noktaları (Adım 36): yabani eşyalar (böğürtlen, mantar, yemiş, ot, bal, kuru dal)
    /// kendi mevsiminde dış mekânın boş karolarında belirir, mevsim bitince kaybolur, toplanınca yeniden doğar.
    /// Tamamen veri odaklı: ItemDef.ForageSeasons ve ForageMax.
    /// </summary>
    public sealed class ForageSystem
    {
        public const double SpawnChancePerDay = 0.7;
        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Random _rng;

        /// <summary>Konulabilecek karo filtresi (ör. harita kenarı payı); yoksa tüm dış mekân.</summary>
        public Func<GridPos, bool> TileFilter { get; set; }

        public event Action<ItemDef, GridPos> Spawned;

        public ForageSystem(WorldState world, ContentDatabase content, int seed = 77)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        public int Count(ItemDef def)
        {
            int n = 0;
            foreach (var it in _world.Items) if (it.Def == def) n++;
            return n;
        }

        /// <summary>Yeni günde: sezon dışı olanlar kalkar, sezonluklar eksiğini tamamlamaya çalışır. Yeni doğan sayısı döner.</summary>
        public int OnNewDay(Season season, bool rained = false)
        {
            // mevsimi biten yabani eşyalar solar
            var stale = new List<int>();
            foreach (var it in _world.Items)
                if (it.Def.IsForage && !it.Def.ForageSeasons.Contains(season)) stale.Add(it.Id);
            foreach (var id in stale) _world.RemoveItem(id);

            int spawned = 0;
            foreach (var def in _content.Items.All)
            {
                if (!def.IsForage || !def.ForageSeasons.Contains(season)) continue;
                int have = Count(def);
                int missing = def.ForageMax - have;
                // Mevsim başında hızlı dolsun: eksik yarısı kadar (en az 1) deneme.
                int attempts = (missing + 1) / 2;
                // Yağmurdan sonra nemi seven türler (mantar, yabani sarımsak) bir kat fazla çıkar.
                if (rained && def.HasTag("rain_loving")) attempts = Math.Max(attempts, 1) * 2;
                for (int i = 0; i < attempts; i++)
                {
                    if (_rng.NextDouble() >= SpawnChancePerDay) continue;
                    if (TrySpawn(def)) spawned++;
                }
            }
            return spawned;
        }

        private bool TrySpawn(ItemDef def)
        {
            var map = _world.Map;
            var rooms = map.Rooms;
            for (int attempt = 0; attempt < 40; attempt++)
            {
                var p = new GridPos(_rng.Next(map.Width), _rng.Next(map.Height));
                if (map.IsBlocked(p)) continue;
                var room = rooms.RoomAt(p);
                if (room != null && room.IsIndoor) continue;
                if (map.IsOwned(p) && map.GetFloor(p) != FloorType.Grass) continue;   // işlenmiş arsayı bozma
                if (TileFilter != null && !TileFilter(p)) continue;
                if (!_world.CanPlaceFloor(def, p, 0).Ok) continue;
                _world.PlaceItem(def, p);
                Spawned?.Invoke(def, p);
                return true;
            }
            return false;
        }
    }
}
