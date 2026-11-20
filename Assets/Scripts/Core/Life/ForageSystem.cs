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

        /// <summary>Kıyı karosu: kendisi su değil, dört komşusundan biri su (göl kumsalı, 27 §18.2).</summary>
        public static bool IsShore(GridMap map, GridPos p)
        {
            if (!map.InBounds(p) || map.GetFloor(p) == FloorType.Water) return false;
            foreach (var n in new[] { new GridPos(p.X + 1, p.Y, p.Z), new GridPos(p.X - 1, p.Y, p.Z), new GridPos(p.X, p.Y + 1, p.Z), new GridPos(p.X, p.Y - 1, p.Z) })
                if (map.InBounds(n) && map.GetFloor(n) == FloorType.Water) return true;
            return false;
        }

        /// <summary>Bölgeye bağlı toplama noktası (deniz kumu yalnızca kıyıda, göl kumu kıyıda değil).</summary>
        public static bool InRegion(ItemDef def, GridPos p)
        {
            if (def.ForageRegions.Count == 0) return true;
            var r = Regions.At(p);
            return r != null && def.ForageRegions.Contains(r.Id);
        }

        private bool TrySpawn(ItemDef def)
        {
            var map = _world.Map;
            var rooms = map.Rooms;
            if (def.HasTag("shore"))
            {
                // Kumsal: yalnızca suyun kenarında (rastgele deneme kıyıyı nadiren bulur; aday listesi).
                var shore = new List<GridPos>();
                for (int y = 0; y < map.Height; y++)
                    for (int x = 0; x < map.Width; x++)
                    {
                        var q = new GridPos(x, y);
                        if (!IsShore(map, q) || map.IsBlocked(q) || !InRegion(def, q)) continue;
                        if (TileFilter != null && !TileFilter(q)) continue;
                        if (_world.CanPlaceFloor(def, q, 0).Ok) shore.Add(q);
                    }
                if (shore.Count == 0) return false;
                var at = shore[_rng.Next(shore.Count)];
                _world.PlaceItem(def, at);
                Spawned?.Invoke(def, at);
                return true;
            }
            for (int attempt = 0; attempt < 40; attempt++)
            {
                var p = new GridPos(_rng.Next(map.Width), _rng.Next(map.Height));
                if (map.IsBlocked(p) || !InRegion(def, p)) continue;
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
