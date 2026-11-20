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
            // Yalıtım katsayıları duvar malzemesine, çatı stiline ve perdelere bağlıdır: harita ya da eşya değişince yenilenir.
            world.Map.Changed += () => _coefDirty = true;
            world.Changed += () => _coefDirty = true;
        }

        public HeatConfig Config => _cfg;

        /// <summary>Hava durumundan gelen dış sıcaklık farkı (kar eksi, yağmur biraz eksi); yoksa 0.</summary>
        public Func<float> OutdoorOffset { get; set; }

        /// <summary>Kayıt için karo sıcaklıkları (kopya).</summary>
        public float[] ExportTemperatures() { Flush(); return (float[])_temp.Clone(); }

        /// <summary>
        /// Kayıt için oda başına tek sıcaklık: (x, y, z, sıcaklık) dörtlüleri, her iç oda için bir karosu. Karo başına dizi
        /// (dünya büyüdükçe yüzlerce KB) yerine birkaç yüz bayt; dış mekân kaydedilmez (hep dış sıcaklıktadır).
        /// </summary>
        public float[] ExportRoomTemperatures()
        {
            EnsureGeometry();
            var list = new List<float>(_heatRooms.Count * 4);
            foreach (var r in _heatRooms)
            {
                var t = r.Room.Tiles[0];
                list.Add(t.X); list.Add(t.Y); list.Add(t.Z); list.Add((float)r.Temp);
            }
            return list.ToArray();
        }

        /// <summary>Kayıttan yükleme (oda başına): dörtlüdeki karonun odasının bütün karoları o sıcaklığı alır.</summary>
        public void RestoreRoomTemperatures(float[] quads)
        {
            if (quads == null || quads.Length < 4) return;
            Flush();
            var rooms = _world.Map.Rooms;
            for (int i = 0; i + 3 < quads.Length; i += 4)
            {
                var room = rooms.RoomAt(new GridPos((int)quads[i], (int)quads[i + 1], (int)quads[i + 2]));
                if (room == null || !room.IsIndoor) continue;
                foreach (var t in room.Tiles) _temp[TempIndex(t)] = quads[i + 3];
            }
            _roomsRef = null;   // oda sıcaklıkları karolardan yeniden okunur
        }

        /// <summary>Kayıttan yükleme; boyut uymazsa yok sayılır (dış sıcaklıkla başlar).</summary>
        public void RestoreTemperatures(float[] temps)
        {
            if (temps == null || temps.Length == 0) return;
            Flush();
            _roomsRef = null;   // oda sıcaklıkları karolardan yeniden okunur
            if (temps.Length == _temp.Length) { Array.Copy(temps, _temp, _temp.Length); return; }
            // Eski kayıt: kat -2 (arsa kazısı) eklenmeden önceki üç kat (-1, 0, 1).
            if (temps.Length == _plane * 3) { Array.Copy(temps, 0, _temp, _plane * (-1 - GridMap.MinLevel), temps.Length); return; }
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
            EnsureGeometry();
            return _byRoom.TryGetValue(room, out var h) ? (float)h.Temp : _outdoor;
        }

        private Func<Edge, float> _edgeLossFactor;
        private Func<GridPos, float> _roofLossFactor;

        /// <summary>Kenarın malzeme çarpanı (taş duvar daha iyi yalıtır); null ise 1. Oyun katmanı içerikten verir.</summary>
        public Func<Edge, float> EdgeLossFactor { get => _edgeLossFactor; set { _edgeLossFactor = value; _coefDirty = true; } }

        /// <summary>Çatı karosunun malzeme çarpanı (saz, şindıl, arduvaz); null ise 1.</summary>
        public Func<GridPos, float> RoofLossFactor { get => _roofLossFactor; set { _roofLossFactor = value; _coefDirty = true; } }

        /// <summary>Yalıtım katsayılarını bir sonraki adımda yeniden hesaplatır (çarpanların bağlı olduğu bir şey değiştiyse).</summary>
        public void InvalidateInsulation() => _coefDirty = true;

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

        /// <summary>Ocağa atılan odunun külü (null = kül yok). Her LogsPerAsh odunda envantere bir kül.</summary>
        public string AshItemId { get; set; }
        /// <summary>Zeytin posası (pirina): odun yoksa yakılır, odunun yarısı kadar yanar.</summary>
        public const string PomaceId = "item.pomace";
        public const float PomaceBurnFactor = 0.5f;
        public const int LogsPerAsh = 4;
        private readonly System.Collections.Generic.Dictionary<int, int> _ashLogs = new System.Collections.Generic.Dictionary<int, int>();

        /// <summary>Isı kaynağına odun atar (envanterden 1 kütük). Yakıt dolu ise ya da odun yoksa false.</summary>
        public bool TryStoke(int itemId, Household house)
        {
            var item = _world.GetItem(itemId);
            if (item == null || item.Def.HeatOutput <= 0f) return false;
            if (item.FuelDays + _cfg.BurnDaysPerLog > _cfg.MaxFuelDays + 1e-9) return false;
            if (house.TryTakeItem(_cfg.FirewoodId)) item.FuelDays += _cfg.BurnDaysPerLog;
            else if (house.TryTakeItem(PomaceId)) { item.FuelDays += _cfg.BurnDaysPerLog * PomaceBurnFactor; return true; }   // odun yoksa pirina (32-7), külü sayılmaz
            else return false;
            if (AshItemId != null)
            {
                // Her LogsPerAsh odunda bir kül (cam ocağının eriticisi, 27 §18.3). Sayaç kayda girmez.
                _ashLogs.TryGetValue(itemId, out int n);
                if (++n >= LogsPerAsh) { n = 0; house.AddItem(AshItemId); }
                _ashLogs[itemId] = n;
            }
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

        // ---- oda önbelleği (performans incelemesi 20.11.2026) ----
        // Eskiden her adımda dünyanın bütün karoları gezilirdi: dış mekân karolarına sıcaklık yazılır, her iç karonun dört
        // kenarı sözlükten sorulur, bütün eşyalar yakıt için taranırdı. Artık odaların sınırları oda haritası değişince bir kez
        // çıkarılır; adım yalnızca iç odalar ve komşuluk katsayıları üzerinden yürür. Sonuç aynıdır (akış komşu sıcaklıklarında
        // doğrusaldır; yalnızca toplama sırası değişir).
        private sealed class RoomHeat
        {
            public Room Room;
            public int[] Tiles;                 // sıcaklık dizisi indeksleri
            public double Temp, Next, Power;
            // Geometri (oda haritası değişince):
            public readonly List<(Edge edge, EdgeKind kind, RoomHeat neighbor)> Edges = new List<(Edge, EdgeKind, RoomHeat)>();
            public readonly List<(RoomHeat neighbor, int count)> Floors = new List<(RoomHeat, int)>();   // tavan/döşeme komşuları
            public int OutdoorFloors;           // üstü ya da altı dış mekân olan döşemeler
            public readonly List<GridPos> RoofTiles = new List<GridPos>();
            public double GroundWeight;
            // Katsayılar (malzeme, perde, çatı stili değişince):
            public readonly List<(RoomHeat neighbor, double coef)> Links = new List<(RoomHeat, double)>();
            public double CoefOutdoor, CoefGround;
        }

        private RoomMap _roomsRef;
        private readonly List<RoomHeat> _heatRooms = new List<RoomHeat>();
        private readonly Dictionary<Room, RoomHeat> _byRoom = new Dictionary<Room, RoomHeat>();
        private bool _coefDirty = true;
        private double _coefAge;
        /// <summary>Katsayılar olay gelmese de en geç bu kadar oyun gününde bir yenilenir (emniyet).</summary>
        private const double CoefRefreshDays = 0.02;
        private readonly List<ItemInstance> _sources = new List<ItemInstance>();
        private int _sourcesVersion = -1;

        /// <summary>Önbellekteki oda sıcaklıklarını karo dizisine yazar; dış mekân karoları dış sıcaklığı alır.</summary>
        private void Flush()
        {
            if (_roomsRef == null) return;
            foreach (var r in _heatRooms)
            {
                float v = (float)r.Temp;
                foreach (int i in r.Tiles) _temp[i] = v;
            }
            foreach (var room in _roomsRef.Rooms)
                if (!room.IsIndoor) foreach (var t in room.Tiles) _temp[TempIndex(t)] = _outdoor;
        }

        private void EnsureGeometry()
        {
            var map = _world.Map;
            var rooms = map.Rooms;
            if (ReferenceEquals(rooms, _roomsRef)) return;
            Flush();   // eski odaların sıcaklığı karolara: yeni odalar karolarının ortalamasıyla başlar
            _roomsRef = rooms;
            _heatRooms.Clear();
            _byRoom.Clear();
            foreach (var room in rooms.Rooms)
            {
                if (!room.IsIndoor || room.Tiles.Count == 0) continue;
                var h = new RoomHeat { Room = room, Tiles = new int[room.Tiles.Count] };
                double sum = 0;
                for (int i = 0; i < room.Tiles.Count; i++) { h.Tiles[i] = TempIndex(room.Tiles[i]); sum += _temp[h.Tiles[i]]; }
                h.Temp = (float)(sum / room.Tiles.Count);
                _heatRooms.Add(h);
                _byRoom[room] = h;
            }
            var floorCount = new Dictionary<RoomHeat, int>();
            foreach (var h in _heatRooms)
            {
                floorCount.Clear();
                foreach (var t in h.Room.Tiles)
                {
                    foreach (var d in Dirs)
                    {
                        var n = t + d;
                        var edge = GridMap.EdgeBetween(t, n);
                        var kind = map.GetEdge(edge);
                        if (kind == EdgeKind.None) continue;
                        var nr = rooms.RoomAt(n);
                        if (nr == h.Room) continue;
                        h.Edges.Add((edge, kind, nr != null && _byRoom.TryGetValue(nr, out var nh) ? nh : null));   // null: dış hava
                    }
                    // Kat yönü: tavan/döşeme komşusu, üst katta çatı, bodrumda toprak.
                    var above = t.OnLevel(t.Z + 1);
                    if (GridMap.IsValidLevel(above.Z) && map.HasTile(above)) AddFloor(h, rooms.RoomAt(above), floorCount);
                    else if (t.Z >= 1) h.RoofTiles.Add(t);   // en üst kat: çatıdan kaybeder
                    var below = t.OnLevel(t.Z - 1);
                    if (GridMap.IsValidLevel(below.Z) && map.HasTile(below)) AddFloor(h, rooms.RoomAt(below), floorCount);
                    if (t.Z < 0) h.GroundWeight += t.Z <= -2 ? 2.0 : 1.0;   // bodrum toprakla dengelenir; derin kat daha çok
                }
                foreach (var kv in floorCount) h.Floors.Add((kv.Key, kv.Value));
            }
            _coefDirty = true;
        }

        private void AddFloor(RoomHeat h, Room other, Dictionary<RoomHeat, int> counts)
        {
            if (other != null && _byRoom.TryGetValue(other, out var oh))
                counts[oh] = (counts.TryGetValue(oh, out int c) ? c : 0) + 1;
            else h.OutdoorFloors++;
        }

        private readonly Dictionary<RoomHeat, double> _linkScratch = new Dictionary<RoomHeat, double>();

        private void EnsureCoefficients()
        {
            if (!_coefDirty) return;
            _coefDirty = false;
            _coefAge = 0;
            foreach (var h in _heatRooms)
            {
                _linkScratch.Clear();
                double outdoor = 0;
                foreach (var (edge, kind, neighbor) in h.Edges)
                {
                    float factor = _edgeLossFactor != null ? _edgeLossFactor(edge) : 1f;
                    double c = EdgeLoss(kind) * factor;
                    if (neighbor == null) outdoor += c;
                    else _linkScratch[neighbor] = (_linkScratch.TryGetValue(neighbor, out var v) ? v : 0.0) + c;
                }
                foreach (var (neighbor, count) in h.Floors)
                    _linkScratch[neighbor] = (_linkScratch.TryGetValue(neighbor, out var v) ? v : 0.0) + (double)_cfg.FloorLoss * count;
                outdoor += (double)_cfg.FloorLoss * h.OutdoorFloors;
                foreach (var t in h.RoofTiles) outdoor += _cfg.RoofLoss * (_roofLossFactor != null ? _roofLossFactor(t) : 1f);
                h.Links.Clear();
                foreach (var kv in _linkScratch) h.Links.Add((kv.Key, kv.Value));
                h.CoefOutdoor = outdoor;
                h.CoefGround = _cfg.GroundLoss * h.GroundWeight;
            }
        }

        private void EnsureSources()
        {
            if (_sourcesVersion == _world.ItemsVersion) return;
            _sourcesVersion = _world.ItemsVersion;
            _sources.Clear();
            foreach (var item in _world.Items) if (item.Def.HeatOutput > 0f) _sources.Add(item);
        }

        private void Step(double dt)
        {
            EnsureGeometry();
            _coefAge += dt;
            if (_coefAge >= CoefRefreshDays) _coefDirty = true;
            EnsureCoefficients();
            EnsureSources();
            var rooms = _roomsRef;

            // Yanan kaynakların gücü ve yakıt tüketimi.
            foreach (var h in _heatRooms) h.Power = 0;
            foreach (var item in _sources)
            {
                if (item.IsDecor || item.FuelDays <= 0) continue;
                double burned = Math.Min(item.FuelDays, dt);
                item.FuelDays -= burned;
                var room = rooms.RoomAt(item.Position);
                if (room == null || !room.IsIndoor || !_byRoom.TryGetValue(room, out var hr)) continue;   // açık havada yanan kaynak odayı ısıtmaz
                hr.Power += item.Def.HeatOutput * (burned / dt);
            }

            // Açık uçlu Euler: herkes adım başındaki sıcaklıklardan hesaplanır, sonra birlikte güncellenir.
            double ground = GroundTemperature;
            foreach (var h in _heatRooms)
            {
                double temp = h.Temp;
                double flow = h.CoefOutdoor * (_outdoor - temp) + h.CoefGround * (ground - temp);
                foreach (var (neighbor, coef) in h.Links) flow += coef * (neighbor.Temp - temp);
                h.Next = (float)(temp + (flow * _cfg.LossScale + h.Power) / h.Tiles.Length * dt);
            }
            foreach (var h in _heatRooms) h.Temp = h.Next;
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
    }
}
