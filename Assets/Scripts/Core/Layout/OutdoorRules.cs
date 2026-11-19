using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Dış mekân kuralları (belge 44 D): kapıdan yola patika ve yol ağı, kuyu yeri, bank yönü, yağmur fıçısı ve odun yığını,
    /// ağaç mesafesi, uzun ağacın kameraya bakan cepheyi örtmesi, meslek dış eşyasının kendi dükkânına yakınlığı, tabela yeri.
    /// Plan (arsalar) verilmişse bina ve meydan bilgisi ondan okunur.
    /// </summary>
    public static class OutdoorRules
    {
        /// <summary>Meslek dış eşyasının kendi binasına en çok uzaklığı (karo).</summary>
        public const int TradeYardReach = 5;
        /// <summary>Uzun ağacın kameraya bakan cepheden en az uzaklığı (karo).</summary>
        public const int FacadeClear = 2;

        public static bool IsTree(ItemDef d) => d.IsPlant && d.Id.StartsWith("item.tree_");
        public static bool IsWell(ItemDef d) => d.Id == "item.well";
        public static bool IsOutdoorSeat(ItemDef d) => d.IsSeat && d.Site != ItemSite.Indoor;
        public static bool IsSign(ItemDef d) => d.Id == "item.shop_sign";

        public static void Add(PlacementValidator v) => v.ExtraRules.Add(Check);

        private static void Check(PlacementValidator v)
        {
            var map = v.Map;
            CheckDoorsToRoads(v);
            List<GridPos> junctions = null;   // yalnızca kuyu varsa hesaplanır
            foreach (var it in v.World.Items)
            {
                if (!PlacementValidator.Occupies(it) || it.Position.Z != 0 || !v.Scoped(it.Position)) continue;
                var room = map.Rooms.RoomAt(it.Position);
                if (room != null && room.IsIndoor) continue;
                var d = it.Def;
                var site = v.Plan?.SiteAt(it.Position);

                // ---- yol ve patika üstünde eşya yok (köprü ve meydan yerleştiricide yasak alan) ----
                foreach (var t in it.OccupiedTiles())
                    if (map.GetRoad(t) != RoadType.None)
                    { v.Add(Rules.Path, t, it, $"{d.DisplayName} yolun ya da patikanın üstünde."); break; }

                // ---- kuyu: meydanın ortasında ya da yol kesişiminin yanında ----
                if (IsWell(d))
                {
                    bool plaza = v.Plan?.Get("square") is Site sq && sq.Plot.Contains(it.Position) && DistToCenter(sq.Plot, it.Position) <= 2;
                    bool junction = false;
                    if (junctions == null) junctions = Junctions(map);
                    foreach (var j in junctions) if (Math.Abs(j.X - it.Position.X) + Math.Abs(j.Y - it.Position.Y) <= 3) { junction = true; break; }
                    if (!plaza && !junction) v.Add(Rules.WellSite, it.Position, it, "Kuyu meydanın ortasında ya da bir yol kesişiminin yanında olmalı.");
                }

                // ---- bank: yola ya da meydana bakar, sırtı duvara ya da çite ----
                if (IsOutdoorSeat(d))
                {
                    var f = ItemFacing.Of(it);
                    if (f.HasValue)
                    {
                        bool faces = false, backed = true;
                        foreach (var c in ItemFacing.InteractionCells(it))
                        {
                            var p = c;
                            for (int i = 0; i < 4 && !faces; i++)
                            {
                                if (map.GetRoad(p) != RoadType.None || map.GetFloor(p) == FloorType.Stone) faces = true;
                                p = Facing.Step(p, f.Value);
                            }
                        }
                        foreach (var (_, e) in ItemFacing.BackEdges(d, it.Position, it.Rotation, f.Value))
                        {
                            var k = map.GetEdge(e);
                            if (k != EdgeKind.Wall && k != EdgeKind.Window && k != EdgeKind.Fence && k != EdgeKind.HalfWall && k != EdgeKind.Railing && k != EdgeKind.Gate) backed = false;
                        }
                        if (!faces) v.Add(Rules.BenchFacing, it.Position, it, $"{d.DisplayName} yola ya da meydana bakmıyor.");
                        else if (!backed) v.Add(Rules.BenchFacing, it.Position, it, $"{d.DisplayName} sırtı duvara ya da çite dayalı değil.");
                    }
                }

                // ---- yağmur fıçısı bina köşesinde duvar dibinde; odun yığını yan ya da arka duvarda ----
                if (d.Id == "item.rain_barrel" && !AtCorner(v, it.Position))
                    v.Add(Rules.Barrel, it.Position, it, "Yağmur fıçısı bir bina köşesinde, duvar dibinde olmalı.");
                if (d.Id == "item.woodshed" && !OnSideOrBackWall(v, it))
                    v.Add(Rules.Barrel, it.Position, it, "Odun yığını binanın yan ya da arka duvarında olmalı.");

                // ---- ağaç: binaya en az 1 karo boşluk; kameraya bakan cepheyi örtmez ----
                if (IsTree(d) && v.Plan != null)
                {
                    foreach (var s in v.Plan.Sites)
                    {
                        if (s.Building.W == 0) continue;
                        int dx = Math.Max(Math.Max(s.Building.X - it.Position.X, it.Position.X - (s.Building.X + s.Building.W - 1)), 0);
                        int dy = Math.Max(Math.Max(s.Building.Y - it.Position.Y, it.Position.Y - (s.Building.Y + s.Building.H - 1)), 0);
                        if (Math.Max(dx, dy) < 2) { v.Add(Rules.TreeClear, it.Position, it, $"Ağaç {s.Name} binasına çok yakın."); break; }
                        if (InFacadeBand(s, it.Position)) { v.Add(Rules.TreeFront, it.Position, it, $"Ağaç {s.Name} binasının kameraya bakan cephesini örtüyor."); break; }
                    }
                }

                // ---- meslek dış eşyası kendi dükkânının yanında ----
                if (d.Site == ItemSite.Outdoor && d.Trades.Count > 0 && v.Plan != null && !d.IsPlant)
                {
                    bool near = false;
                    foreach (var s in v.Plan.Sites)
                    {
                        if (s.Trade == null || !d.HasTrade(s.Trade)) continue;
                        if (Grow(s.Building, TradeYardReach).Contains(it.Position)) { near = true; break; }
                    }
                    if (!near) v.Add(Rules.TradeYard, it.Position, it, $"{d.DisplayName} kendi dükkânından uzak.");
                }

                // ---- tabela kapının 1-2 karo yanında, yolun değdiği yerde ----
                if (IsSign(d) && v.Plan != null)
                {
                    bool ok = false;
                    foreach (var s in v.Plan.Sites)
                    {
                        if (s.Kind == "house" || s.Building.W == 0) continue;
                        var o = s.DoorOutside;
                        int dist = Math.Abs(o.X - it.Position.X) + Math.Abs(o.Y - it.Position.Y);
                        if (dist >= 1 && dist <= 2 && TouchesRoad(map, it.Position)) { ok = true; break; }
                    }
                    if (!ok) v.Add(Rules.Sign, it.Position, it, "Tabela kapının 1-2 karo yanında, yolun buluştuğu yerde olmalı.");
                }
            }
        }

        private static RectI Grow(RectI r, int n) => new RectI(r.X - n, r.Y - n, r.W + 2 * n, r.H + 2 * n);

        private static int DistToCenter(RectI r, GridPos p) => Math.Abs(p.X - (r.X + r.W / 2)) + Math.Abs(p.Y - (r.Y + r.H / 2));

        public static bool TouchesRoad(GridMap map, GridPos p)
        {
            foreach (var d in Facing.All) if (map.GetRoad(Facing.Step(p, d)) != RoadType.None) return true;
            return false;
        }

        /// <summary>Kameraya bakan cephe şeridi: binanın batı (x0'ın batısı) ve kuzey (y0'ın kuzeyi) cephesinin önündeki iki karo.</summary>
        public static bool InFacadeBand(Site s, GridPos p)
        {
            var b = s.Building;
            bool west = p.X >= b.X - FacadeClear && p.X < b.X && p.Y >= b.Y - 1 && p.Y <= b.Y + b.H;
            bool north = p.Y >= b.Y - FacadeClear && p.Y < b.Y && p.X >= b.X - 1 && p.X <= b.X + b.W;
            return west || north;
        }

        private static bool AtCorner(PlacementValidator v, GridPos p)
        {
            if (v.Plan == null) return true;
            foreach (var s in v.Plan.Sites)
            {
                var b = s.Building;
                if (b.W == 0) continue;
                // Duvar dibi: binaya dik komşu; köşe: duvarın uç karosunun yanında.
                bool westOrEast = (p.X == b.X - 1 || p.X == b.X + b.W) && (p.Y == b.Y || p.Y == b.Y + b.H - 1);
                bool northOrSouth = (p.Y == b.Y - 1 || p.Y == b.Y + b.H) && (p.X == b.X || p.X == b.X + b.W - 1);
                if (westOrEast || northOrSouth) return true;
            }
            return false;
        }

        private static bool OnSideOrBackWall(PlacementValidator v, ItemInstance it)
        {
            if (v.Plan == null) return true;
            foreach (var s in v.Plan.Sites)
            {
                var b = s.Building;
                if (b.W == 0) continue;
                foreach (var t in it.OccupiedTiles())
                    foreach (var d in Facing.All)
                    {
                        var n = Facing.Step(t, d);
                        if (!b.Contains(n)) continue;
                        // n bina içinde: eşya binanın Opposite(d) duvarında değil, d yönündeki duvarında; kapı duvarı = DoorDir yönü.
                        var wallSide = Facing.Opposite(d);
                        if (wallSide != s.DoorDir) return true;
                    }
            }
            return false;
        }

        // ---------------- kapı → yol ----------------

        /// <summary>Bina dış kapısının önü bir yola bağlı mı ve o yol ana ağa bağlı mı (kopuk parça yok).</summary>
        private static void CheckDoorsToRoads(PlacementValidator v)
        {
            var map = v.Map;
            Dictionary<GridPos, int> comp = null; int main = 0;
            foreach (var d in PlacementValidator.Doors(map))
            {
                if (!d.Exterior || d.Kind != EdgeKind.Door || d.Outside.Z != 0 || !v.Scoped(d.Outside)) continue;
                if (comp == null) comp = RoadComponents(map, out main);
                if (!Walkway(map, d.Outside))
                {
                    v.Add(Rules.Path, d.Outside, null, $"Kapının önünden yola patika yok ({d.Edge}).");
                    continue;
                }
                if (comp.TryGetValue(d.Outside, out int c) && c != main)
                    v.Add(Rules.RoadNet, d.Outside, null, $"Kapının patikası ana yol ağına bağlı değil ({d.Edge}).");
            }
        }

        /// <summary>Yürüyüş yolu: yol karosu ya da açık havadaki taş (meydan) ve ahşap (köprü) döşeme.</summary>
        public static bool Walkway(GridMap map, GridPos p)
        {
            if (p.Z != 0 || !map.InBounds(p)) return false;
            if (map.GetRoad(p) != RoadType.None) return true;
            var f = map.GetFloor(p);
            if (f != FloorType.Stone && f != FloorType.Wood) return false;
            var room = map.Rooms.RoomAt(p);
            return room == null || !room.IsIndoor;
        }

        /// <summary>Yol karolarının bağlı bileşenleri (geçitler dahil). main: en büyük bileşen.</summary>
        public static Dictionary<GridPos, int> RoadComponents(GridMap map, out int main)
        {
            var comp = new Dictionary<GridPos, int>();
            var sizes = new List<int>();
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var s = new GridPos(x, y);
                    if (!Walkway(map, s) || comp.ContainsKey(s)) continue;
                    int id = sizes.Count, n = 0;
                    var q = new Queue<GridPos>();
                    comp[s] = id; q.Enqueue(s);
                    while (q.Count > 0)
                    {
                        var c = q.Dequeue(); n++;
                        foreach (var d in Facing.All)
                        {
                            var t = Facing.Step(c, d);
                            if (!map.InBounds(t) || comp.ContainsKey(t) || !Walkway(map, t)) continue;
                            if (map.EdgeBlocks(GridMap.EdgeBetween(c, t))) continue;
                            comp[t] = id; q.Enqueue(t);
                        }
                        foreach (var t in map.LinksFrom(c))   // bölge geçitleri (komşu olmayan karolar)
                        {
                            if (!map.InBounds(t) || t.Z != 0 || comp.ContainsKey(t) || !Walkway(map, t)) continue;
                            comp[t] = id; q.Enqueue(t);
                        }
                    }
                    sizes.Add(n);
                }
            main = 0;
            for (int i = 1; i < sizes.Count; i++) if (sizes[i] > sizes[main]) main = i;
            return comp;
        }

        /// <summary>
        /// Yol kesişimleri: üç ya da dört yönde (3 karo ötede) yol devam eden yol karoları, birbirine 4 karodan yakın olanlardan biri.
        /// </summary>
        public static List<GridPos> Junctions(GridMap map)
        {
            var list = new List<GridPos>();
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (map.GetRoad(p) == RoadType.None) continue;
                    int dirs = 0;
                    foreach (var d in Facing.All)
                    {
                        bool run = true;
                        for (int i = 1; i <= 3 && run; i++) run = map.GetRoad(Facing.Step(p, d, i)) != RoadType.None;
                        if (run) dirs++;
                    }
                    if (dirs < 3) continue;
                    bool near = false;
                    foreach (var j in list) if (Math.Abs(j.X - x) + Math.Abs(j.Y - y) < 4) { near = true; break; }
                    if (!near) list.Add(p);
                }
            return list;
        }
    }
}
