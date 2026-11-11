using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Heat
{
    /// <summary>Dış sıcaklık eğrisi: mevsim ortalarında sabit değerler, aralarında doğrusal geçiş.</summary>
    public static class Climate
    {
        // Mevsim ortası sıcaklıkları (°C): İlkbahar, Yaz, Sonbahar, Kış. Denge adımında ayarlanır.
        public static readonly float[] SeasonMid = { 10f, 20f, 8f, -8f };

        public static float OutdoorTemperature(TimeConfig config, double totalDays)
        {
            double year = config.DaysPerYear;
            double p = totalDays % year;
            if (p < 0) p += year;
            double q = p - config.DaysPerSeason * 0.5;
            if (q < 0) q += year;
            int i = (int)(q / config.DaysPerSeason) % 4;
            double f = (q - i * config.DaysPerSeason) / config.DaysPerSeason;
            return (float)(SeasonMid[i] + (SeasonMid[(i + 1) % 4] - SeasonMid[i]) * f);
        }
    }

    /// <summary>Isı modelinin ayarları (yer tutucu değerler; Adım 28'de dengelenir).</summary>
    public sealed class HeatConfig
    {
        /// <summary>Kenar başına ısı kaybı katsayıları: duvar iyi yalıtır, kapı ve pencere kaçırır.</summary>
        public float WallLoss = 0.4f, DoorLoss = 1.0f, WindowLoss = 1.2f;
        /// <summary>Genel kayıp ölçeği (1/gün).</summary>
        public float LossScale = 0.6f;
        /// <summary>Katlar arası döşeme/tavan alışverişi (karo başına), üst kat çatı kaybı, bodrumun toprakla alışverişi.</summary>
        public float FloorLoss = 0.25f, RoofLoss = 0.3f, GroundLoss = 0.6f;
        /// <summary>Toprak sıcaklığı: bu taban değer + dış sıcaklığın (tabandan farkının) bu oranı; yıl boyu serin ve dengeli.</summary>
        public float GroundBase = 8f, GroundFollow = 0.15f;
        /// <summary>Bir odun kütüğünün yanma süresi (oyun günü).</summary>
        public double BurnDaysPerLog = 0.5;
        /// <summary>Bir ısı kaynağının alabileceği en çok yakıt (oyun günü).</summary>
        public double MaxFuelDays = 1.5;
        /// <summary>Sayısal adım üst sınırı (oyun günü); kararlılık için.</summary>
        public double MaxStep = 0.05;
        public string FirewoodId = "item.firewood";
    }

    /// <summary>
    /// Isı mekaniği: oda başına tek sıcaklık. Sıcaklık karo başına saklanır (oda tespiti kenar
    /// değişince yeniden hesaplandığı için); bir odanın sıcaklığı karolarının ortalamasıdır.
    /// Oda; komşusuyla (dış mekân dahil) duvar/kapı/pencere kenarları üzerinden ısı alışverişi yapar,
    /// yanan kaynaklar (ocak) ısı verir ve yakıt tüketir.
    /// </summary>
    public sealed class HeatSystem
    {
        private readonly WorldState _world;
        private readonly HeatConfig _cfg;
        private readonly TimeConfig _time;
        private readonly float[] _temp;
        private readonly Dictionary<Room, double> _roomTemp = new Dictionary<Room, double>();
        private float _outdoor;
        private readonly int _plane;

        private int TempIndex(GridPos p) => ((p.Z - GridMap.MinLevel) * _world.Map.Height + p.Y) * _world.Map.Width + p.X;

        /// <summary>Bodrumun doğal (ısıtılmamış) sıcaklığı: toprak yıl boyu dengeli kalır.</summary>
        public float GroundTemperature => _cfg.GroundBase + _cfg.GroundFollow * (_outdoor - _cfg.GroundBase);

        public HeatSystem(WorldState world, TimeConfig time, HeatConfig config = null, double startDays = 0)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _cfg = config ?? new HeatConfig();
            _outdoor = Climate.OutdoorTemperature(_time, startDays);
            _plane = world.Map.Width * world.Map.Height;
            _temp = new float[_plane * GridMap.LevelCount];
            for (int i = 0; i < _temp.Length; i++) _temp[i] = _outdoor;
            // Bodrum toprakla dengede başlar.
            for (int i = 0; i < _plane; i++) _temp[i] = GroundTemperature;
        }

        public HeatConfig Config => _cfg;

        /// <summary>Hava durumundan gelen dış sıcaklık farkı (kar eksi, yağmur biraz eksi); yoksa 0.</summary>
        public Func<float> OutdoorOffset { get; set; }

        /// <summary>Kayıt için karo sıcaklıkları (kopya).</summary>
        public float[] ExportTemperatures() => (float[])_temp.Clone();

        /// <summary>Kayıttan yükleme; boyut uymazsa yok sayılır (dış sıcaklıkla başlar).</summary>
        public void RestoreTemperatures(float[] temps)
        {
            if (temps == null) return;
            if (temps.Length == _temp.Length) { Array.Copy(temps, _temp, _temp.Length); return; }
            // Eski kayıt: yalnızca zemin kat sıcaklıkları (kat eklenmeden önce).
            if (temps.Length == _plane) Array.Copy(temps, 0, _temp, _plane * (0 - GridMap.MinLevel), _plane);
        }
        public float OutdoorTemperature => _outdoor;

        public float TemperatureAt(GridPos p)
        {
            var map = _world.Map;
            if (!map.InBounds(p)) return _outdoor;
            var room = map.Rooms.RoomAt(p);
            if (room == null || !room.IsIndoor) return _outdoor;
            return RoomTemperature(room);
        }

        private float RoomTemperature(Room room)
        {
            double sum = 0;
            foreach (var t in room.Tiles) sum += _temp[TempIndex(t)];
            return (float)(sum / room.Tiles.Count);
        }

        private float EdgeLoss(EdgeKind k)
        {
            switch (k)
            {
                case EdgeKind.Wall: return _cfg.WallLoss;
                case EdgeKind.Door: return _cfg.DoorLoss;
                case EdgeKind.Window: return _cfg.WindowLoss;
                default: return 0f;
            }
        }

        /// <summary>Isı kaynağına odun atar (envanterden 1 kütük). Yakıt dolu ise ya da odun yoksa false.</summary>
        public bool TryStoke(int itemId, Household house)
        {
            var item = _world.GetItem(itemId);
            if (item == null || item.Def.HeatOutput <= 0f) return false;
            if (item.FuelDays + _cfg.BurnDaysPerLog > _cfg.MaxFuelDays + 1e-9) return false;
            if (!house.TryTakeItem(_cfg.FirewoodId)) return false;
            item.FuelDays += _cfg.BurnDaysPerLog;
            return true;
        }

        public void Tick(double gameDays, double totalDays)
        {
            if (gameDays <= 0) return;
            _outdoor = Climate.OutdoorTemperature(_time, totalDays) + (OutdoorOffset != null ? OutdoorOffset() : 0f);

            double left = gameDays;
            while (left > 1e-12)
            {
                double dt = Math.Min(left, _cfg.MaxStep);
                Step(dt);
                left -= dt;
            }
        }

        private void Step(double dt)
        {
            var map = _world.Map;
            var rooms = map.Rooms;

            // Adım başındaki oda sıcaklıkları (açık uçlu Euler: herkes aynı anlık değerden hesaplanır).
            _roomTemp.Clear();
            foreach (var room in rooms.Rooms)
            {
                if (!room.IsIndoor) { _roomTemp[room] = _outdoor; continue; }
                double sum = 0;
                foreach (var t in room.Tiles) sum += _temp[TempIndex(t)];
                _roomTemp[room] = sum / room.Tiles.Count;
            }

            // Yanan kaynakların gücü ve yakıt tüketimi.
            var power = new Dictionary<Room, double>();
            foreach (var item in _world.Items)
            {
                if (item.IsDecor || item.Def.HeatOutput <= 0f || item.FuelDays <= 0) continue;
                double burned = Math.Min(item.FuelDays, dt);
                item.FuelDays -= burned;
                var room = rooms.RoomAt(item.Position);
                if (room == null || !room.IsIndoor) continue;      // açık havada yanan kaynak odayı ısıtmaz
                power[room] = (power.TryGetValue(room, out var pw) ? pw : 0.0) + item.Def.HeatOutput * (burned / dt);
            }

            foreach (var room in rooms.Rooms)
            {
                if (!room.IsIndoor)
                {
                    foreach (var t in room.Tiles) _temp[TempIndex(t)] = _outdoor;
                    continue;
                }

                double temp = _roomTemp[room];
                double flow = 0;
                foreach (var t in room.Tiles)
                {
                    foreach (var d in Dirs)
                    {
                        var n = t + d;
                        var kind = map.GetEdge(GridMap.EdgeBetween(t, n));
                        if (kind == EdgeKind.None) continue;
                        var nr = rooms.RoomAt(n);
                        if (nr == room) continue;
                        double neighborTemp = nr != null ? _roomTemp[nr] : _outdoor;   // harita dışı = dış hava
                        flow += EdgeLoss(kind) * (neighborTemp - temp);
                    }
                    flow += VerticalFlow(map, rooms, t, temp);
                }

                double heat = power.TryGetValue(room, out var p) ? p : 0.0;
                float newTemp = (float)(temp + (flow * _cfg.LossScale + heat) / room.Tiles.Count * dt);
                foreach (var t in room.Tiles) _temp[TempIndex(t)] = newTemp;
            }
        }

        /// <summary>Bir karonun kat yönündeki ısı akışı: tavan/döşeme komşusu, üst katta çatı, bodrumda toprak.</summary>
        private double VerticalFlow(GridMap map, RoomMap rooms, GridPos t, double temp)
        {
            double flow = 0;
            var above = t.OnLevel(t.Z + 1);
            if (GridMap.IsValidLevel(above.Z) && map.HasTile(above))
                flow += _cfg.FloorLoss * (TempOfRoomAt(rooms, above) - temp);
            else if (t.Z >= 1)
                flow += _cfg.RoofLoss * (_outdoor - temp);          // en üst kat: çatıdan kaybeder

            var below = t.OnLevel(t.Z - 1);
            if (GridMap.IsValidLevel(below.Z) && map.HasTile(below))
                flow += _cfg.FloorLoss * (TempOfRoomAt(rooms, below) - temp);
            if (t.Z < 0)
                flow += _cfg.GroundLoss * (GroundTemperature - temp); // bodrum toprakla dengelenir
            return flow;
        }

        private double TempOfRoomAt(RoomMap rooms, GridPos p)
        {
            var r = rooms.RoomAt(p);
            if (r == null) return _outdoor;
            return _roomTemp.TryGetValue(r, out var v) ? v : _outdoor;
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
    }
}
