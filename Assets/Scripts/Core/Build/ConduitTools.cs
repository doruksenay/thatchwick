using System;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    // Su yolu döşeme (yapı modu): kendi arsanda, zemin katta. Ark ve oluk açık havada; boru eve, döşemenin ve yolun
    // altına girer, eşyanın altından geçer. Envanterde boru (taş ark için yontma taş, oluk için tahta) varsa o harcanır,
    // yoksa fiyatı ödenir. Kaldırınca malzemesi geri gelir (malzemeyle döşendiyse) ya da yarısı iade edilir.
    public sealed partial class BuildService
    {
        public string ConduitBlocker(GridPos tile, ConduitType type)
        {
            var map = _world.Map;
            if (!map.InBounds(tile) || tile.Z != 0) return "Su yolu yalnızca zemin katta döşenir (üst kata kurşun boru kolonuyla çıkar).";
            if (!map.IsOwned(tile)) return NotOwned;
            var old = map.GetConduit(tile);
            if (type == ConduitType.None) return old == ConduitType.None ? "Burada su yolu yok." : null;
            if (old == type) return "Orası zaten öyle.";
            if (map.IsWater(tile)) return "Burası zaten su: arkı kıyıdan başlat.";
            var room = map.Rooms.RoomAt(tile);
            if (room != null && room.IsIndoor && !Conduits.AllowedIndoor(type)) return $"{Conduits.Name(type)} açık havada olur; eve boru (kütük, künk, kurşun) girer.";
            if (map.GetRoad(tile) != RoadType.None && !Conduits.AllowedOnRoad(type)) return "Yolun altından boru, üstünden oluk geçer; arkı yola açma.";
            if (Conduits.IsOpen(type) && type != ConduitType.Flume && map.IsBlocked(tile) && _world.GetItemAt(tile) == null)
                return "Burası kazılamaz.";
            if (AccountLevel != null && AccountLevel() < Conduits.Level(type)) return $"Hesap seviyesi {Conduits.Level(type)} gerekli.";
            return null;
        }

        /// <summary>Su yolu döşer, değiştirir ya da (None) kaldırır. Geri alınabilir.</summary>
        public PlacementCheck PaintConduit(GridPos tile, ConduitType type)
        {
            string why = ConduitBlocker(tile, type);
            if (why != null) return PlacementCheck.Fail(why);
            var map = _world.Map;
            var old = map.GetConduit(tile);

            // Eskisi: malzemesi envantere döner (borular söküp yeniden kullanılır), ark kazısı yarı iade.
            string oldMat = Conduits.Material(old);
            int refund = old == ConduitType.None || oldMat != null ? 0 : Refund(Conduits.Price(old));

            // Yenisi: envanterde malzeme varsa o, yoksa akçe.
            string mat = Conduits.Material(type);
            bool useMat = type != ConduitType.None && mat != null && _house.Count(mat) > 0;
            int cost = type == ConduitType.None || useMat ? 0 : Conduits.Price(type);
            if (cost > refund && !_house.TrySpend(cost - refund)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > cost) _house.AddAkce(refund - cost);
            if (useMat) _house.TryTakeItem(mat);
            if (oldMat != null) _house.AddItem(oldMat);
            map.SetConduit(tile, type);

            int net = refund - cost;
            Record(type == ConduitType.None ? "Su yolu kaldır" : Conduits.Name(type), () =>
            {
                if (oldMat != null && !_house.TryTakeItem(oldMat)) return PlacementCheck.Fail($"{Conduits.Name(old)} envanterde yok, geri alınamaz.");
                if (net > 0 && !_house.TrySpend(net)) { if (oldMat != null) _house.AddItem(oldMat); return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                if (net < 0) _house.AddAkce(-net);
                if (useMat) _house.AddItem(mat);
                map.SetConduit(tile, old);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>İki karo arası L biçimli hat: önce yatay, sonra dikey.</summary>
        public static System.Collections.Generic.List<GridPos> ConduitRun(GridPos from, GridPos to)
        {
            var list = new System.Collections.Generic.List<GridPos>();
            int sx = Math.Sign(to.X - from.X), sy = Math.Sign(to.Y - from.Y);
            for (int x = from.X; ; x += sx) { list.Add(new GridPos(x, from.Y)); if (x == to.X || sx == 0) break; }
            for (int y = from.Y + sy; sy != 0; y += sy) { list.Add(new GridPos(to.X, y)); if (y == to.Y) break; }
            return list;
        }

        /// <summary>
        /// Hattı döşer (tek geri alma adımı). Döşenemeyen karo atlanır; döşenen karo sayısını ve ilk engeli verir.
        /// Zaten aynı türdeki karolar sayılmaz.
        /// </summary>
        public int PaintConduitRun(GridPos from, GridPos to, ConduitType type, out string firstProblem)
        {
            firstProblem = null;
            int done = 0;
            BeginGroup(type == ConduitType.None ? "Su yolu kaldır" : Conduits.Name(type));
            try
            {
                foreach (var t in ConduitRun(from, to))
                {
                    if (_world.Map.GetConduit(t) == type) continue;
                    var r = PaintConduit(t, type);
                    if (r.Ok) done++;
                    else if (firstProblem == null) firstProblem = $"{t}: {r.Reason}";
                }
            }
            finally { EndGroup(); }
            return done;
        }
    }
}
