using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Isı adımı oda sınırlarını önbelleğe alacak biçimde yeniden yazıldı (performans incelemesi 20.11.2026). Eski, karo karo
    /// yürüyen uygulama burada başvuru olarak durur: iki katlı, bodrumlu, kapılı ve pencereli bir evde, duvar yıkılıp
    /// örülürken ve ocak yanarken sıcaklıklar aynı kalmalı.
    /// </summary>
    public class IsiOnbellekTests
    {
        private static readonly TimeConfig Time = TimeConfig.Default;
        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        private static void Box(GridMap m, int x0, int y0, int x1, int y1, int z)
        {
            for (int x = x0; x <= x1; x++)
            {
                m.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal, z), EdgeKind.Wall);
                m.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), EdgeKind.Wall);
            }
            for (int y = y0; y <= y1; y++)
            {
                m.SetEdge(new Edge(x0, y, EdgeAxis.Vertical, z), EdgeKind.Wall);
                m.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical, z), EdgeKind.Wall);
            }
        }

        [Fact]
        public void EskiKaroKaroUygulamaylaAyniSicakliklar()
        {
            var map = new GridMap(16, 14);
            // Zemin: iki oda (aralarında kapı), birinde pencere. Üst kat ve bodrum birinci odanın üstünde/altında.
            Box(map, 2, 2, 5, 5, 0);
            Box(map, 6, 2, 9, 5, 0);
            map.SetEdge(new Edge(6, 3, EdgeAxis.Vertical, 0), EdgeKind.Door);
            map.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal, 0), EdgeKind.Window);
            for (int x = 2; x <= 5; x++) for (int y = 2; y <= 5; y++) { map.SetTile(new GridPos(x, y, 1), true); map.SetTile(new GridPos(x, y, -1), true); }
            Box(map, 2, 2, 5, 5, 1);
            var w = new WorldState(map);
            var hearth = w.PlaceItem(new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f }, new GridPos(3, 3));
            var house = new Household(); house.AddItem("item.firewood", 6);

            var heat = new HeatSystem(w, Time, null, 35.0)
            {
                EdgeLossFactor = e => e.Axis == EdgeAxis.Horizontal ? 0.8f : 1.1f,
                RoofLossFactor = t => 0.7f + 0.05f * t.X
            };
            var reference = new Reference(w, Time, new HeatConfig(), 35.0)
            {
                EdgeLossFactor = e => e.Axis == EdgeAxis.Horizontal ? 0.8f : 1.1f,
                RoofLossFactor = t => 0.7f + 0.05f * t.X
            };

            double day = 35.0;
            float peak = 0f;
            for (int step = 0; step < 400; step++)
            {
                if (step == 20) { Assert.True(heat.TryStoke(hearth.Id, house)); reference.Fuel = hearth.FuelDays; }
                if (step == 150) map.SetEdge(new Edge(8, 2, EdgeAxis.Horizontal, 0), EdgeKind.None);   // ikinci oda dışarı açılır
                if (step == 220) map.SetEdge(new Edge(8, 2, EdgeAxis.Horizontal, 0), EdgeKind.Wall);   // yeniden kapanır
                if (step == 300) map.SetEdge(new Edge(6, 4, EdgeAxis.Vertical, 0), EdgeKind.None);     // iki oda birleşir
                double dt = step % 3 == 0 ? 0.004 : 0.011;
                day += dt;
                heat.Tick(dt, day);
                reference.Tick(dt, day, hearth.Position);
                foreach (var p in new[] { new GridPos(3, 3), new GridPos(8, 4), new GridPos(3, 3, 1), new GridPos(4, 4, -1), new GridPos(12, 12) })
                    Assert.Equal((double)reference.TemperatureAt(p), (double)heat.TemperatureAt(p), 0.02);
                peak = Math.Max(peak, heat.TemperatureAt(new GridPos(3, 3)) - heat.OutdoorTemperature);
            }
            Assert.True(peak > 0.5f, "ocak odayı ısıtmış olmalı (en çok fark " + peak + ")");
        }

        [Fact]
        public void OdaBasinaKayit_YuklenenSicaklikAyni()
        {
            var map = new GridMap(12, 12);
            Box(map, 2, 2, 5, 5, 0);
            var w = new WorldState(map);
            var hearth = w.PlaceItem(new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f }, new GridPos(3, 3));
            var house = new Household(); house.AddItem("item.firewood", 2);
            var heat = new HeatSystem(w, Time, null, 35.0);
            heat.TryStoke(hearth.Id, house);
            for (int i = 0; i < 40; i++) heat.Tick(0.01, 35.0 + i * 0.01);
            float warm = heat.TemperatureAt(new GridPos(3, 3));
            var saved = heat.ExportRoomTemperatures();
            Assert.Equal(4, saved.Length);   // tek iç oda: x, y, z, sıcaklık

            var loaded = new HeatSystem(w, Time, null, 35.4);
            loaded.RestoreRoomTemperatures(saved);
            Assert.Equal((double)warm, (double)loaded.TemperatureAt(new GridPos(3, 3)), 0.001);
            Assert.Equal((double)loaded.OutdoorTemperature, (double)loaded.TemperatureAt(new GridPos(9, 9)), 0.001);
        }

        // ---- eski uygulama: her adımda bütün odaların bütün karoları ----
        private sealed class Reference
        {
            private readonly WorldState _world; private readonly HeatConfig _cfg; private readonly TimeConfig _time;
            private readonly float[] _temp; private readonly int _plane; private float _outdoor;
            private readonly Dictionary<Room, double> _roomTemp = new Dictionary<Room, double>();
            public Func<Edge, float> EdgeLossFactor; public Func<GridPos, float> RoofLossFactor;
            public double Fuel;

            public Reference(WorldState world, TimeConfig time, HeatConfig cfg, double startDays)
            {
                _world = world; _time = time; _cfg = cfg;
                _outdoor = Climate.OutdoorTemperature(time, startDays);
                _plane = world.Map.Width * world.Map.Height;
                _temp = new float[_plane * GridMap.LevelCount];
                for (int i = 0; i < _temp.Length; i++) _temp[i] = _outdoor;
                for (int i = 0; i < _plane; i++) _temp[i] = Ground;
            }

            private float Ground => _cfg.GroundBase + _cfg.GroundFollow * (_outdoor - _cfg.GroundBase);
            private int Idx(GridPos p) => ((p.Z - GridMap.MinLevel) * _world.Map.Height + p.Y) * _world.Map.Width + p.X;

            public float TemperatureAt(GridPos p)
            {
                var room = _world.Map.Rooms.RoomAt(p);
                if (room == null || !room.IsIndoor) return _outdoor;
                double sum = 0; foreach (var t in room.Tiles) sum += _temp[Idx(t)];
                return (float)(sum / room.Tiles.Count);
            }

            private float Loss(EdgeKind k) => k == EdgeKind.Wall ? _cfg.WallLoss : k == EdgeKind.Door ? _cfg.DoorLoss : k == EdgeKind.Window ? _cfg.WindowLoss : 0f;

            public void Tick(double dt, double totalDays, GridPos source)
            {
                _outdoor = Climate.OutdoorTemperature(_time, totalDays);
                var map = _world.Map; var rooms = map.Rooms;
                _roomTemp.Clear();
                foreach (var room in rooms.Rooms)
                {
                    if (!room.IsIndoor) { _roomTemp[room] = _outdoor; continue; }
                    double sum = 0; foreach (var t in room.Tiles) sum += _temp[Idx(t)];
                    _roomTemp[room] = sum / room.Tiles.Count;
                }
                var power = new Dictionary<Room, double>();
                if (Fuel > 0)
                {
                    double burned = Math.Min(Fuel, dt); Fuel -= burned;
                    var sr = rooms.RoomAt(source);
                    if (sr != null && sr.IsIndoor) power[sr] = 80f * (burned / dt);
                }
                foreach (var room in rooms.Rooms)
                {
                    if (!room.IsIndoor) { foreach (var t in room.Tiles) _temp[Idx(t)] = _outdoor; continue; }
                    double temp = _roomTemp[room], flow = 0;
                    foreach (var t in room.Tiles)
                    {
                        foreach (var d in Dirs)
                        {
                            var n = t + d;
                            var edge = GridMap.EdgeBetween(t, n);
                            var kind = map.GetEdge(edge);
                            if (kind == EdgeKind.None) continue;
                            var nr = rooms.RoomAt(n);
                            if (nr == room) continue;
                            double nt = nr != null ? _roomTemp[nr] : _outdoor;
                            flow += Loss(kind) * (EdgeLossFactor != null ? EdgeLossFactor(edge) : 1f) * (nt - temp);
                        }
                        var above = t.OnLevel(t.Z + 1);
                        if (GridMap.IsValidLevel(above.Z) && map.HasTile(above)) flow += _cfg.FloorLoss * (RoomT(rooms, above) - temp);
                        else if (t.Z >= 1) flow += _cfg.RoofLoss * (RoofLossFactor != null ? RoofLossFactor(t) : 1f) * (_outdoor - temp);
                        var below = t.OnLevel(t.Z - 1);
                        if (GridMap.IsValidLevel(below.Z) && map.HasTile(below)) flow += _cfg.FloorLoss * (RoomT(rooms, below) - temp);
                        if (t.Z < 0) flow += _cfg.GroundLoss * (t.Z <= -2 ? 2.0 : 1.0) * (Ground - temp);
                    }
                    double heat = power.TryGetValue(room, out var pw) ? pw : 0.0;
                    float nt2 = (float)(temp + (flow * _cfg.LossScale + heat) / room.Tiles.Count * dt);
                    foreach (var t in room.Tiles) _temp[Idx(t)] = nt2;
                }
            }

            private double RoomT(RoomMap rooms, GridPos p)
            {
                var r = rooms.RoomAt(p);
                return r == null ? _outdoor : _roomTemp.TryGetValue(r, out var v) ? v : _outdoor;
            }
        }
    }
}
