using System;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    // Yol döşeme (yapı modu): kendi arsanda, zemin katta, açık havada; altında eşya olmamalı.
    public sealed partial class BuildService
    {
        public static int RoadPrice(RoadType t)
        {
            switch (t)
            {
                case RoadType.Path: return 1;
                case RoadType.Gravel: return 3;
                case RoadType.Paved: return 6;
                default: return 0;
            }
        }

        /// <summary>Yolun açıldığı hesap seviyesi (kaldırım 3).</summary>
        public static int RoadLevel(RoadType t) => t == RoadType.Paved ? 3 : 1;

        public static string RoadName(RoadType t)
        {
            switch (t)
            {
                case RoadType.Path: return "Patika";
                case RoadType.Gravel: return "Çakıl yol";
                case RoadType.Paved: return "Kaldırım";
                default: return "Yol yok";
            }
        }

        /// <summary>Onarımın taş gideri (patika bedava: çiğnenip düzeltilir).</summary>
        public static int RepairStone(RoadType t) => t == RoadType.Paved ? 2 : t == RoadType.Gravel ? 1 : 0;

        public string RoadBlocker(GridPos tile, RoadType type)
        {
            var map = _world.Map;
            if (!map.InBounds(tile) || tile.Z != 0) return "Yol yalnızca zemin katta döşenir.";
            if (!map.IsOwned(tile)) return NotOwned;
            var room = map.Rooms.RoomAt(tile);
            if (room != null && room.IsIndoor) return "Yol açık havaya döşenir (içeriye zemin döşe).";
            if (type != RoadType.None && (map.IsBlocked(tile) || _world.GetItemAt(tile) != null)) return "Üstünde bir şey var.";
            if (type != RoadType.None && AccountLevel != null && AccountLevel() < RoadLevel(type)) return $"Hesap seviyesi {RoadLevel(type)} gerekli.";
            var old = map.GetRoad(tile);
            if (old == type && (type == RoadType.None || !map.IsNaturalRoad(tile) && map.GetRoadCondition(tile) >= 0.999f)) return type == RoadType.None ? "Burada yol yok." : "Orası zaten öyle.";
            return null;
        }

        /// <summary>Yol döşer (aynı yolu yeniden döşemek onarır), RoadType.None kaldırır (yarı iade). Geri alınabilir.</summary>
        public PlacementCheck PaintRoad(GridPos tile, RoadType type)
        {
            string why = RoadBlocker(tile, type);
            if (why != null) return PlacementCheck.Fail(why);
            var map = _world.Map;
            var old = map.GetRoad(tile);
            float oldCond = map.GetRoadCondition(tile);
            bool oldNatural = map.IsNaturalRoad(tile);
            // Doğal patikanın değeri yok; aynı türü yeniden döşemek (onarım) tam fiyat değil, yarısıdır.
            int oldPrice = oldNatural ? 0 : RoadPrice(old);
            int cost = type == RoadType.None ? 0 : old == type && !oldNatural ? Math.Max(1, RoadPrice(type) / 2) : Math.Max(0, RoadPrice(type) - Refund(oldPrice));
            int refund = type == RoadType.None ? Refund(oldPrice) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > 0) _house.AddAkce(refund);
            map.SetRoad(tile, type);
            int net = refund - cost;
            Record(type == RoadType.None ? "Yol kaldır" : RoadName(type), () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                if (old == RoadType.None) map.SetRoad(tile, RoadType.None);
                else map.SetRoad(tile, old, oldCond, oldNatural);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }
    }
}
