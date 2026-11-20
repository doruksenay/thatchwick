using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Dış mekân (belge 44 D): şehir planı elle (belge 40) ve korunur; donatı bağlam ve kuralla yerleşir. Her binanın kapı yönü
    /// ve ön yüzü plandan okunur. Sıra: meydan (kuyu, ilan tahtası, banklar, tezgâh) → her bina için tabela, meslek avlusu ve
    /// bahçe şablonu (layouts.json "gardens") → yol kesişimlerinde tabela → köy yolu (tüccar durağı, toplama düğümleri) →
    /// doğal bitki örtüsü (seyrek, kümeli, yoğunluk haritalı, sabit tohumla). Her adımdan sonra doğrulayıcı: geçmeyen isteğe
    /// bağlı eşya çıkarılır. Rastgelelik yalnızca şablon ve kural içinde.
    /// </summary>
    public static class OutdoorBuilder
    {
        public sealed class Context
        {
            public WorldState World;
            public ContentDatabase Content;
            public LayoutCatalog Catalog;
            public LayoutPlan Plan;
            public LayoutRng Rng;
            /// <summary>Hiç eşya konmayacak karolar: kapı önü (2 karo) ve yanları, geçitler, yol.</summary>
            public HashSet<GridPos> Reserved = new HashSet<GridPos>();
            public readonly List<FurnishReport> Reports = new List<FurnishReport>();
            public Action<string> Log;
            public List<GridPos> JunctionList;
            public GridMap Map => World.Map;
        }

        public static List<FurnishReport> Place(WorldState world, ContentDatabase content, LayoutCatalog cat, LayoutPlan plan, int seed, Action<string> log = null)
        {
            var ctx = new Context { World = world, Content = content, Catalog = cat, Plan = plan, Rng = new LayoutRng(Seeds.Derive(seed, 4404)), Log = log };
            Reserve(ctx);
            Plaza(ctx);
            foreach (var b in Regions.TownBuildings) Sign(ctx, b);     // önce bütün tabelalar (kapı yanını komşu bahçe kapmasın)
            foreach (var b in Regions.TownBuildings) Yard(ctx, b);
            Pen(ctx);
            Farm(ctx);
            Crossroads(ctx);
            foreach (var r in Regions.All) EdgeBand(ctx, r, Seeds.Derive(seed, 4410 + Array.IndexOf(Regions.All, r)));   // taslaktaki kenar bandı ve kaya sırtı (belge 64a)
            VillageRoad(ctx, seed);
            Vegetation.Scatter(ctx, Regions.Town, Seeds.Derive(seed, 4405));
            Vegetation.Scatter(ctx, Regions.Road, Seeds.Derive(seed, 4406));
            return ctx.Reports;
        }

        // ---------------- ayrılan karolar ----------------

        private static void Reserve(Context ctx)
        {
            var map = ctx.Map;
            foreach (var d in PlacementValidator.Doors(map))
            {
                if (d.Outside.Z != 0 || !d.Exterior) continue;
                var p = d.Outside;
                for (int i = 0; i < PlacementValidator.DoorOutside + 1; i++)
                {
                    ctx.Reserved.Add(p);
                    // Kapı önünün iki yanı da (geniş giriş, tabela ve bank kapıyı sıkıştırmasın).
                    if (i == 0) { ctx.Reserved.Add(Facing.Step(p, Facing.Rotate(d.OutDir, 1))); ctx.Reserved.Add(Facing.Step(p, Facing.Rotate(d.OutDir, 3))); }
                    p = Facing.Step(p, d.OutDir);
                }
            }
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (map.IsPortal(p)) ctx.Reserved.Add(p);
                }
            // Ek yapı ile ana gövdenin iç köşesi: iki duvar arasında sıkışan karoya eşya konmaz (kullanılamaz kalır).
            foreach (var b in Regions.TownBuildings)
            {
                if (!b.HasAnnex) continue;
                var a = b.AnnexRect;
                for (int y = a.Y - 1; y <= a.Y + a.H; y++)
                    for (int x = a.X - 1; x <= a.X + a.W; x++)
                    {
                        var p = new GridPos(x, y);
                        if (b.Covers(p)) continue;
                        bool nearA = false, nearB = false;
                        foreach (var d in Facing.All)
                        {
                            var q = Facing.Step(p, d);
                            if (a.Contains(q)) nearA = true;
                            if (b.Rect.Contains(q)) nearB = true;
                        }
                        if (nearA && nearB) ctx.Reserved.Add(p);
                    }
            }
        }

        internal static bool Open(Context ctx, GridPos p)
        {
            var map = ctx.Map;
            if (!map.HasTile(p) || map.IsBlocked(p) || map.IsWater(p) || ctx.Reserved.Contains(p)) return false;
            if (map.GetRoad(p) != RoadType.None) return false;
            var f = map.GetFloor(p);
            if (f != FloorType.Grass && f != FloorType.Dirt) return false;           // meydan taşı, köprü, bina içi değil
            var room = map.Rooms.RoomAt(p);
            return room == null || !room.IsIndoor;
        }

        private static bool BuildingClear(Context ctx, GridPos p, int dist)
        {
            foreach (var s in ctx.Plan.Sites)
            {
                if (s.Building.W == 0) continue;
                int dx = Math.Max(Math.Max(s.Building.X - p.X, p.X - (s.Building.X + s.Building.W - 1)), 0);
                int dy = Math.Max(Math.Max(s.Building.Y - p.Y, p.Y - (s.Building.Y + s.Building.H - 1)), 0);
                if (Math.Max(dx, dy) < dist) return false;
            }
            return true;
        }

        // ---------------- yerleştirme ----------------

        private static ItemInstance Put(Context ctx, string id, GridPos p, int rot, List<ItemInstance> placed, bool grown = true)
        {
            if (!ctx.Content.Items.TryGet(id, out var def)) { ctx.Log?.Invoke($"Eşya tanımı yok: {id}"); return null; }
            if (!(def.IsRug ? ctx.World.CanPlaceRug(def, p, rot).Ok : ctx.World.CanPlaceFloor(def, p, rot).Ok)) return null;
            var it = ctx.World.PlaceItem(def, p, rot);
            if (def.IsPlant && grown) PlantSystem.SetGrown(it);
            placed?.Add(it);
            return it;
        }

        /// <summary>Adayları sırayla dener; ayak izi açık, ayrılmamış ve verilen koşulu sağlayan ilk yere koyar.</summary>
        private static ItemInstance TryAt(Context ctx, ItemDef def, IEnumerable<(GridPos p, int rot, double score)> cands, List<ItemInstance> placed)
        {
            foreach (var c in cands.OrderByDescending(c => c.score))
            {
                var (w, h) = Footprint.Dimensions(def.Width, def.Height, c.rot);
                bool ok = true;
                for (int dy = 0; dy < h && ok; dy++) for (int dx = 0; dx < w && ok; dx++) ok = Open(ctx, new GridPos(c.p.X + dx, c.p.Y + dy));
                if (!ok) continue;
                if (def.HasFront && !def.IsSeat)
                    foreach (var f in ItemFacing.FrontCells(def, c.p, c.rot)) if (!Open(ctx, f) && ctx.Map.GetRoad(f) == RoadType.None) { ok = false; break; }
                if (!ok) continue;
                var it = Put(ctx, def.Id, c.p, c.rot, placed);
                if (it == null) continue;
                // Önü açık kalsın: ön karolar ayrılır.
                foreach (var f in ItemFacing.InteractionCells(it)) ctx.Reserved.Add(f);
                return it;
            }
            return null;
        }

        /// <summary>Binanın bahçe bölgesi: binadan 1-4 karo, en yakın bina bu olan açık karolar (yol, kapı önü, meydan hariç).</summary>
        public static HashSet<GridPos> Zone(Context ctx, RectI b, int reach = 4)
        {
            var z = new HashSet<GridPos>();
            for (int y = b.Y - reach; y < b.Y + b.H + reach; y++)
                for (int x = b.X - reach; x < b.X + b.W + reach; x++)
                {
                    var p = new GridPos(x, y);
                    if (b.Contains(p) || !Open(ctx, p)) continue;
                    if (Regions.At(p) != Regions.Town && Regions.At(p) != Regions.Farm) continue;
                    if (Regions.PlazaRect.Contains(p) && Regions.InPlazaLocal(p.X - Regions.Town.X, p.Y - Regions.Town.Y)) continue;
                    int mine = Dist(b, p), best = int.MaxValue;
                    foreach (var o in Regions.TownBuildings) if (!o.Rect.Equals(b)) best = Math.Min(best, o.DistanceTo(p));
                    if (mine <= best) z.Add(p);
                }
            return z;
        }

        internal static int Dist(RectI b, GridPos p)
        {
            int dx = Math.Max(Math.Max(b.X - p.X, p.X - (b.X + b.W - 1)), 0);
            int dy = Math.Max(Math.Max(b.Y - p.Y, p.Y - (b.Y + b.H - 1)), 0);
            return Math.Max(dx, dy);
        }

        /// <summary>Karo binanın hangi duvarının dibinde (dik komşu); değilse null.</summary>
        private static Dir4? WallSide(RectI b, GridPos p)
        {
            if (p.X == b.X - 1 && p.Y >= b.Y && p.Y < b.Y + b.H) return Dir4.W;
            if (p.X == b.X + b.W && p.Y >= b.Y && p.Y < b.Y + b.H) return Dir4.E;
            if (p.Y == b.Y - 1 && p.X >= b.X && p.X < b.X + b.W) return Dir4.N;
            if (p.Y == b.Y + b.H && p.X >= b.X && p.X < b.X + b.W) return Dir4.S;
            return null;
        }

        private static bool IsCornerTile(RectI b, GridPos p)
        {
            var s = WallSide(b, p);
            if (!s.HasValue) return false;
            return s == Dir4.W || s == Dir4.E ? p.Y == b.Y || p.Y == b.Y + b.H - 1 : p.X == b.X || p.X == b.X + b.W - 1;
        }

        // ---------------- bahçe/avlu adımı ----------------

        /// <summary>Bir bahçe şablonu adımını binanın bölgesine uygular. anchor: nearDoor, back, side, wallBase, corner, water, open, front, pen, bed.</summary>
        private static int Step(Context ctx, TownSiteView site, LayoutStep step, List<ItemInstance> placed)
        {
            int n = 0;
            for (int k = 0; k < step.Count; k++)
            {
                if (step.Chance < 1f && ctx.Rng.NextDouble() > step.Chance) continue;
                var id = step.Items[ctx.Rng.Next(step.Items.Count)];
                if (!ctx.Content.Items.TryGet(id, out var def)) continue;
                // Görseli olmayan bitki örtüsü konmaz (görünmez engel olmasın; görsel gelince yeni oyunda çıkar).
                if (def.HasTag("veg") && def.Art == null) continue;
                if (step.With == "bed") { if (Bed(ctx, site, def, step, placed)) n++; continue; }
                var cands = Candidates(ctx, site, def, step);
                if (TryAt(ctx, def, cands, placed) != null) n++;
            }
            return n;
        }

        private static IEnumerable<(GridPos p, int rot, double score)> Candidates(Context ctx, TownSiteView site, ItemDef def, LayoutStep step)
        {
            var b = site.Building;
            var rots = ItemFacing.ValidRotations(def);
            var area = step.Anchor == "water" ? site.Wide : step.Anchor == "pen" ? site.Pen : site.Zone;
            if (step.Anchor == "nearDoor")
            {
                // Kapı yanı komşu binanın bahçe bölgesine düşebilir (bitişik dükkânlar): kapının çevresi doğrudan taranır.
                area = new HashSet<GridPos>();
                for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++) area.Add(new GridPos(site.DoorOutside.X + dx, site.DoorOutside.Y + dy));
            }
            if (area == null) yield break;
            GridPos? target = null;
            if (step.With == Anchors.NearItem && step.On != null)
                foreach (var it in ctx.World.Items)
                    if (it.Def.Id == step.On && site.Wide.Contains(it.Position)) { target = it.Position; break; }
            foreach (var p in area)
                foreach (var rot in rots)
                {
                    double score = ctx.Rng.NextDouble();
                    var f = def.HasFront ? ItemFacing.FacingFor(def, rot) : (Dir4?)null;
                    var side = b.W > 0 ? WallSide(b, p) : null;
                    int dist = b.W > 0 ? Dist(b, p) : 0;
                    if (target.HasValue)
                    {
                        int td = Math.Abs(target.Value.X - p.X) + Math.Abs(target.Value.Y - p.Y);
                        if (td > 3) continue;
                        score -= td;
                    }
                    switch (step.Anchor)
                    {
                        case "nearDoor":
                        {
                            var o = site.DoorOutside;
                            int dd = Math.Abs(o.X - p.X) + Math.Abs(o.Y - p.Y);
                            if (dd < 1 || dd > 2 || !OutdoorRules.TouchesRoad(ctx.Map, p)) continue;
                            score -= dd;
                            break;
                        }
                        case "front":
                            if (side != site.DoorDir && dist > 2) continue;
                            if (dist < 1 || dist > 2) continue;
                            score += OutdoorRules.TouchesRoad(ctx.Map, p) ? 1 : 0;
                            break;
                        case "back":
                            if (b.W == 0 || !Behind(b, site.DoorDir, p) || dist > 3) continue;
                            score -= dist * 0.5;
                            break;
                        case "side":
                            if (b.W == 0 || !side.HasValue || side == site.DoorDir || side == Facing.Opposite(site.DoorDir)) continue;
                            break;
                        case "wallBase":
                            if (!side.HasValue || side == site.DoorDir) continue;
                            if (f.HasValue && f.Value != side.Value) continue;          // ön yüz binadan dışarı, sırt duvara
                            if (f.HasValue)
                            {
                                bool backed = true;
                                foreach (var (_, e) in ItemFacing.BackEdges(def, p, rot, f.Value))
                                {
                                    var k = ctx.Map.GetEdge(e);
                                    if (k != EdgeKind.Wall && k != EdgeKind.Window && k != EdgeKind.Fence && k != EdgeKind.HalfWall) backed = false;
                                }
                                if (!backed) continue;
                            }
                            break;
                        case "corner":
                            if (!IsCornerTile(b, p) || side == site.DoorDir) continue;
                            break;
                        case "water":
                        {
                            bool wet = false;
                            var (fw, fh) = Footprint.Dimensions(def.Width, def.Height, rot);
                            for (int dy = 0; dy < fh && !wet; dy++) for (int dx = 0; dx < fw && !wet; dx++)
                                foreach (var d in Facing.All) if (ctx.Map.IsWater(Facing.Step(new GridPos(p.X + dx, p.Y + dy), d))) wet = true;
                            if (!wet) continue;
                            score -= dist * 0.3;
                            break;
                        }
                        case "open":
                            if (b.W > 0 && dist < 2) continue;
                            break;
                        case "pen":
                            break;
                        case "junction":
                        {
                            if (ctx.JunctionList == null) ctx.JunctionList = OutdoorRules.Junctions(ctx.Map);
                            int best = int.MaxValue;
                            foreach (var j in ctx.JunctionList) best = Math.Min(best, Math.Abs(j.X - p.X) + Math.Abs(j.Y - p.Y));
                            if (best > 3 || best < 2) continue;
                            score -= best;
                            break;
                        }
                        default:
                            break;
                    }
                    // Meslek dış eşyası kendi dükkânına yakın (ayak izi dahil).
                    if (def.Site == ItemSite.Outdoor && def.Trades.Count > 0 && b.W > 0 && dist + Math.Max(def.Width, def.Height) - 1 > OutdoorRules.TradeYardReach) continue;
                    // Dış mekân bankı: önünde (4 karo içinde) yol ya da meydan.
                    if (OutdoorRules.IsOutdoorSeat(def) && f.HasValue && !FacesWalkway(ctx, def, p, rot, f.Value)) continue;
                    // Ağaç: binaya en az bir karo boşluk ve kameraya bakan cepheyi örtmez.
                    if (OutdoorRules.IsTree(def) && (!BuildingClear(ctx, p, 2) || ctx.Plan.Sites.Any(s => s.Building.W > 0 && OutdoorRules.InFacadeBand(s, p)))) continue;
                    yield return (p, rot, score);
                }
        }

        private static bool FacesWalkway(Context ctx, ItemDef def, GridPos pos, int rot, Dir4 f)
        {
            foreach (var c in ItemFacing.FrontCells(def, pos, rot))
            {
                var p = c;
                for (int i = 0; i < 4; i++) { if (OutdoorRules.Walkway(ctx.Map, p)) return true; p = Facing.Step(p, f); }
            }
            return false;
        }

        private static bool Behind(RectI b, Dir4 door, GridPos p)
        {
            switch (door)
            {
                case Dir4.N: return p.Y >= b.Y + b.H && p.X >= b.X - 1 && p.X <= b.X + b.W;
                case Dir4.S: return p.Y < b.Y && p.X >= b.X - 1 && p.X <= b.X + b.W;
                case Dir4.W: return p.X >= b.X + b.W && p.Y >= b.Y - 1 && p.Y <= b.Y + b.H;
                default: return p.X < b.X && p.Y >= b.Y - 1 && p.Y <= b.Y + b.H;
            }
        }

        /// <summary>
        /// Çitli tarh (sebze ya da ot): binadan en az 1 karo uzakta 3x2 ya da 4x2 açık alan; toprak, çit, bahçe kapısı yola
        /// ya da yola bitişik karoya açılır; içine şablonun eşyası (ekin, ot öbeği) dikilir.
        /// </summary>
        private static bool Bed(Context ctx, TownSiteView site, ItemDef plant, LayoutStep step, List<ItemInstance> placed)
        {
            var map = ctx.Map;
            foreach (var (w, h) in new[] { (4, 2), (3, 2), (2, 3), (2, 2) })
                foreach (var p in site.Zone.OrderBy(_ => ctx.Rng.NextDouble()))
                {
                    var cells = new List<GridPos>();
                    bool ok = true;
                    for (int dy = 0; dy < h && ok; dy++) for (int dx = 0; dx < w && ok; dx++)
                    {
                        var c = new GridPos(p.X + dx, p.Y + dy);
                        ok = site.Zone.Contains(c) && Open(ctx, c) && map.GetFloor(c) == FloorType.Grass && BuildingClear(ctx, c, 2);
                        cells.Add(c);
                    }
                    if (!ok) continue;
                    // Çevresinde kenar olmamalı (başka çit/duvar) ve bahçe kapısı bir yola açılmalı.
                    var rect = new RectI(p.X, p.Y, w, h);
                    Edge? gate = null; GridPos gateOut = default;
                    foreach (var c in cells)
                        foreach (var d in Facing.All)
                        {
                            var o = Facing.Step(c, d);
                            if (rect.Contains(o)) continue;
                            if (map.GetEdge(Facing.EdgeOf(c, d)) != EdgeKind.None) { ok = false; break; }
                            if (gate == null && (map.GetRoad(o) != RoadType.None || (Open(ctx, o) && OutdoorRules.TouchesRoad(map, o))))
                            { gate = Facing.EdgeOf(c, d); gateOut = o; }
                        }
                    if (!ok || gate == null) continue;
                    // Kur: toprak, çit, kapı.
                    foreach (var c in cells)
                    {
                        map.SetFloor(c, FloorType.Dirt);
                        foreach (var d in Facing.All)
                        {
                            var o = Facing.Step(c, d);
                            if (rect.Contains(o)) continue;
                            var e = Facing.EdgeOf(c, d);
                            map.SetEdge(e, e.Equals(gate.Value) ? EdgeKind.Gate : EdgeKind.Fence);
                        }
                    }
                    ctx.Reserved.Add(gateOut);
                    var gateIn = gate.Value.SideA.Equals(gateOut) ? gate.Value.SideB : gate.Value.SideA;
                    foreach (var c in cells)
                    {
                        if (c.Equals(gateIn)) continue;                      // bahçe kapısının içi boş (girilir)
                        if (ctx.Rng.NextDouble() < 0.15) continue;          // tarhta ara ara boş toprak
                        var it = Put(ctx, plant.Id, c, 0, placed, grown: !plant.IsCrop);
                        if (it != null && plant.IsCrop) it.GrowthDays = plant.GrowDays * (0.4 + 0.5 * ctx.Rng.NextDouble());
                    }
                    return true;
                }
            return false;
        }

        // ---------------- site görünümü ----------------

        internal sealed class TownSiteView
        {
            public RectI Building;
            public Dir4 DoorDir;
            public GridPos DoorOutside;
            public HashSet<GridPos> Zone;     // bahçe bölgesi
            public HashSet<GridPos> Wide;     // su kıyısı vb. için daha geniş çevre (6 karo)
            public HashSet<GridPos> Pen;      // ağıl içi
            public string Name;
        }

        private static TownSiteView View(Context ctx, TownBuilding b) => new TownSiteView
        {
            Building = b.Rect, DoorDir = Facing.FromChar(b.Door), DoorOutside = Facing.Step(b.DoorInside, Facing.FromChar(b.Door)),
            Zone = Zone(ctx, b.Rect, 4), Wide = Zone(ctx, b.Rect, 7), Name = b.Name,
        };

        private static void RunGarden(Context ctx, TownSiteView view, string gardenId, string name)
        {
            var g = ctx.Catalog?.Garden(gardenId);
            if (g == null) return;
            var placed = new List<ItemInstance>();
            var rep = new FurnishReport { Room = name };
            foreach (var step in g.Steps)
            {
                int n = Step(ctx, view, step, placed);
                if (n == 0 && !step.Optional) rep.Missing.Add(step.Items[0]);
            }
            // Doğrulayıcı: bu adımların eşyalarından kurala uymayan isteğe bağlılar çıkarılır.
            Validate(ctx, placed, rep);
            rep.Placed.AddRange(placed.Where(p => ctx.World.GetItem(p.Id) != null));
            ctx.Reports.Add(rep);
            foreach (var m in rep.Missing) ctx.Log?.Invoke($"{name}: zorunlu dış eşya konamadı {m}");
        }

        internal static void Validate(Context ctx, List<ItemInstance> placed, FurnishReport rep)
        {
            if (placed.Count == 0) return;
            var ids = new HashSet<int>(placed.Select(p => p.Id));
            var tiles = new HashSet<GridPos>(placed.SelectMany(p => p.OccupiedTiles()));
            var v = new PlacementValidator(ctx.World, ctx.Plan, tiles.Contains);
            LayoutRules.AddAll(v, ctx.Content);
            foreach (var x in v.Validate())
            {
                if (x.Severity != Severity.Error || !ids.Contains(x.ItemId) || ctx.World.GetItem(x.ItemId) == null) continue;
                ctx.World.RemoveItem(x.ItemId);
                rep.Dropped.Add($"{x.ItemDefId}: {x.Reason}");
            }
        }

        // ---------------- meydan ----------------

        private static void Plaza(Context ctx)
        {
            var map = ctx.Map;
            var placed = new List<ItemInstance>();
            var rep = new FurnishReport { Room = "Köy meydanı" };   // loc-yok: yerleşim raporu (geliştirici)
            var c = new GridPos(Regions.Town.X + Regions.PlazaCX, Regions.Town.Y + Regions.PlazaCY);
            // Kuyu ortada; yanında kova ve leğen.
            var well = PutOnStone(ctx, "item.well", c, placed);
            if (well == null) rep.Missing.Add("item.well");
            else
            {
                ctx.Reserved.Add(Facing.Step(c, Dir4.N)); ctx.Reserved.Add(Facing.Step(c, Dir4.W));   // kuyunun kameraya bakan önü açık
                PutOnStone(ctx, "item.water_bucket", Facing.Step(c, Dir4.E), placed);
                PutOnStone(ctx, "item.wash_tub", Facing.Step(c, Dir4.S), placed);
            }
            // Meydanın kenarı: sekizgenin kenar taşları. Banklar meydana bakar, arkasına alçak taş duvar (yalnızca kameraya
            // bakan yönler N ve W: güney ve doğu kenarı).
            var rim = new List<(GridPos p, Dir4 face, Dir4 back)>();
            for (int y = Regions.PlazaRect.Y; y < Regions.PlazaRect.Y + Regions.PlazaRect.H; y++)
                for (int x = Regions.PlazaRect.X; x < Regions.PlazaRect.X + Regions.PlazaRect.W; x++)
                {
                    var p = new GridPos(x, y);
                    if (map.GetFloor(p) != FloorType.Stone || map.GetRoad(p) != RoadType.None) continue;
                    foreach (var back in new[] { Dir4.S, Dir4.E })
                    {
                        var o = Facing.Step(p, back);
                        if (map.GetFloor(o) == FloorType.Stone || map.GetRoad(o) != RoadType.None || ctx.Reserved.Contains(o)) continue;
                        rim.Add((p, Facing.Opposite(back), back));
                    }
                }
            int benches = 0;
            foreach (var (p, face, back) in rim.OrderBy(_ => ctx.Rng.NextDouble()))
            {
                if (benches >= 3) break;
                if (!ctx.Content.Items.TryGet("item.garden_bench", out var bench)) break;
                int rot = ItemFacing.RotationFor(bench, face);
                if (!ItemFacing.IsValid(bench, rot)) continue;
                var (w, h) = Footprint.Dimensions(bench.Width, bench.Height, rot);
                var cells = new List<GridPos>();
                for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) cells.Add(new GridPos(p.X + dx, p.Y + dy));
                if (cells.Any(q => map.GetFloor(q) != FloorType.Stone || map.IsBlocked(q) || ctx.Reserved.Contains(q) || map.GetRoad(q) != RoadType.None)) continue;
                if (cells.Any(q => { var o = Facing.Step(q, back); return map.GetFloor(o) == FloorType.Stone || map.GetRoad(o) != RoadType.None; })) continue;
                if (placed.Any(i => i.Def.Id == "item.garden_bench" && Math.Abs(i.Position.X - p.X) + Math.Abs(i.Position.Y - p.Y) < 5)) continue;
                if (!ctx.World.CanPlaceFloor(bench, p, rot).Ok) continue;
                foreach (var q in cells) map.SetEdge(Facing.EdgeOf(q, back), EdgeKind.HalfWall);
                placed.Add(ctx.World.PlaceItem(bench, p, rot));
                foreach (var q in ItemFacing.InteractionCells(placed[placed.Count - 1])) ctx.Reserved.Add(q);
                benches++;
            }
            // Bekçi ateşi ve ilan tahtası meydanın kenarında (yolu ve kuyunun önünü kapatmadan). Pazar tezgâhı meydanda değil:
            // pazar Kerem'in genel dükkânında (belge 42), tezgâh dükkânın önünde mal sergisi olur (layouts.json yard_general).
            // Meydan süsü (kullanıcı 20.11: "dekore edilmeli"): güvercinlik ve güneş saati taş kenarda.
            foreach (var id in new[] { "item.dovecote_village", "item.sundial" })
            {
                if (!ctx.Content.Items.TryGet(id, out var def)) continue;
                var cands = new List<(GridPos, int, double)>();
                foreach (var (p, _, _) in rim)
                    if (!placed.Any(i => Math.Abs(i.Position.X - p.X) + Math.Abs(i.Position.Y - p.Y) < 3))
                        foreach (var rot in ItemFacing.ValidRotations(def)) cands.Add((p, rot, ctx.Rng.NextDouble()));
                TryOnStone(ctx, def, cands, placed);
            }
            // Gölge ağaçları: meydanın dört köşegeninde, taşın hemen dışındaki çimde birer ıhlamur/meşe.
            var cx = Regions.Town.X + Regions.PlazaCX; var cy = Regions.Town.Y + Regions.PlazaCY;
            foreach (var (dx, dy) in new[] { (-6, -5), (6, -5), (-6, 5), (6, 5) })
            {
                ItemInstance tree = null;
                for (int r = 0; r <= 2 && tree == null; r++)
                    foreach (var (ox, oy) in new[] { (0, 0), (Math.Sign(dx) * r, 0), (0, Math.Sign(dy) * r), (Math.Sign(dx) * r, Math.Sign(dy) * r) })
                    {
                        var p = new GridPos(cx + dx + ox, cy + dy + oy);
                        if (!Open(ctx, p) || ctx.Reserved.Contains(p) || !BuildingClear(ctx, p, 3) || OutdoorRules.TouchesRoad(map, p)) continue;
                        tree = Put(ctx, ctx.Rng.NextDouble() < 0.5 ? "item.tree_oak" : "item.tree_birch", p, 0, placed);
                        if (tree != null) break;
                    }
            }
            foreach (var id in new[] { "item.notice_post", "item.bonfire" })
            {
                if (!ctx.Content.Items.TryGet(id, out var def)) continue;
                var cands = new List<(GridPos, int, double)>();
                foreach (var (p, _, _) in rim) foreach (var rot in ItemFacing.ValidRotations(def)) cands.Add((p, rot, ctx.Rng.NextDouble()));
                var it = TryOnStone(ctx, def, cands, placed);
                if (it == null && id != "item.bonfire") rep.Missing.Add(id);
            }
            Validate(ctx, placed, rep);
            rep.Placed.AddRange(placed.Where(p => ctx.World.GetItem(p.Id) != null));
            ctx.Reports.Add(rep);
        }

        private static ItemInstance PutOnStone(Context ctx, string id, GridPos p, List<ItemInstance> placed)
        {
            if (!ctx.Content.Items.TryGet(id, out var def) || ctx.Reserved.Contains(p) || ctx.Map.GetRoad(p) != RoadType.None) return null;
            if (!ctx.World.CanPlaceFloor(def, p, 0).Ok) return null;
            var it = ctx.World.PlaceItem(def, p, 0);
            placed.Add(it);
            return it;
        }

        private static ItemInstance TryOnStone(Context ctx, ItemDef def, List<(GridPos p, int rot, double score)> cands, List<ItemInstance> placed)
        {
            foreach (var c in cands.OrderByDescending(c => c.score))
            {
                var (w, h) = Footprint.Dimensions(def.Width, def.Height, c.rot);
                bool ok = true;
                for (int dy = 0; dy < h && ok; dy++) for (int dx = 0; dx < w && ok; dx++)
                {
                    var q = new GridPos(c.p.X + dx, c.p.Y + dy);
                    ok = ctx.Map.GetFloor(q) == FloorType.Stone && !ctx.Map.IsBlocked(q) && !ctx.Reserved.Contains(q) && ctx.Map.GetRoad(q) == RoadType.None;
                }
                if (!ok || !ctx.World.CanPlaceFloor(def, c.p, c.rot).Ok) continue;
                var it = ctx.World.PlaceItem(def, c.p, c.rot);
                placed.Add(it);
                foreach (var f in ItemFacing.InteractionCells(it)) ctx.Reserved.Add(f);
                return it;
            }
            return null;
        }

        // ---------------- binalar ----------------

        private static void Sign(Context ctx, TownBuilding b)
        {
            // Tabela: dükkân ve hanın kapısının 1-2 karo yanında, yolun değdiği yerde.
            if (b.Kind != "house")
            {
                var view = View(ctx, b);
                var placed = new List<ItemInstance>();
                var rep = new FurnishReport { Room = $"{b.Name} (tabela)" };
                var step = new LayoutStep { Anchor = "nearDoor" }; step.Items.Add("item.shop_sign");
                if (Step(ctx, view, step, placed) == 0) rep.Missing.Add("item.shop_sign");
                Validate(ctx, placed, rep);
                rep.Placed.AddRange(placed);
                ctx.Reports.Add(rep);
            }
        }

        private static void Yard(Context ctx, TownBuilding b)
        {
            var t = ctx.Catalog?.Building(b.Template);
            if (t?.Garden == null) return;
            var g = ctx.Catalog.Garden(t.Garden);
            if (g != null && g.Enclosure == "fence") BackYardFence(ctx, b);
            RunGarden(ctx, View(ctx, b), t.Garden, $"{b.Name} (avlu ve bahçe)");
        }

        /// <summary>
        /// Arka avlu çiti (belge 64a, köy meydanı sadeleştirme 20.11): bahçe şablonu "fence" ise binanın arka duvarı boyunca
        /// 3 (olmazsa 2) karo derin çitli avlu. Avlunun her karosu açık, yolsuz ve ayrılmamış olmalı; kapı yola en yakın
        /// yüzde, dışı ve içi boş kalır. Uymazsa çit kurulmaz (avlu eşyaları yine konur).
        /// </summary>
        private static void BackYardFence(Context ctx, TownBuilding b)
        {
            var map = ctx.Map;
            var back = Facing.Opposite(Facing.FromChar(b.Door));
            var r = b.Rect;
            bool alongX = back == Dir4.N || back == Dir4.S;
            int wallLen = alongX ? r.W : r.H;
            var doorOut = Facing.Step(b.DoorInside, Facing.FromChar(b.Door));
            foreach (int depth in new[] { 3, 2 })
                for (int len = wallLen; len >= 3; len--)
                    for (int off = 0; off + len <= wallLen; off++)
                    {
                        int x0, y0, w, h;
                        switch (back)
                        {
                            case Dir4.N: x0 = r.X + off; y0 = r.Y - depth; w = len; h = depth; break;
                            case Dir4.S: x0 = r.X + off; y0 = r.Y + r.H; w = len; h = depth; break;
                            case Dir4.W: x0 = r.X - depth; y0 = r.Y + off; w = depth; h = len; break;
                            default: x0 = r.X + r.W; y0 = r.Y + off; w = depth; h = len; break;
                        }
                        if (TryFenceYard(ctx, b, new RectI(x0, y0, w, h), doorOut)) return;
                    }
        }

        private static bool IsBand(GridPos p)
        {
            var t = Regions.TerrainAt(p);
            return t == MapTerrain.Band || t == MapTerrain.Rock;
        }

        private static bool TryFenceYard(Context ctx, TownBuilding b, RectI yard, GridPos doorOut)
        {
            var map = ctx.Map;
            var r = b.Rect;
            var cells = new List<GridPos>();
            for (int y = yard.Y; y < yard.Y + yard.H; y++)
                for (int x = yard.X; x < yard.X + yard.W; x++)
                {
                    var c = new GridPos(x, y);
                    if (Regions.At(c) != Regions.Town || !Open(ctx, c) || ctx.Reserved.Contains(c) || Regions.InPlazaLocal(x - Regions.Town.X, y - Regions.Town.Y)) return false;
                    cells.Add(c);
                }
            // Dış kenarlar: binaya bakan yüz duvardır; diğerleri çit olacak, hepsi boş olmalı.
            var outer = new List<(GridPos inside, Dir4 d, GridPos outside)>();
            foreach (var c in cells)
                foreach (var d in Facing.All)
                {
                    var o = Facing.Step(c, d);
                    if (yard.Contains(o) || b.Covers(o)) continue;
                    if (map.GetEdge(Facing.EdgeOf(c, d)) != EdgeKind.None) return false;
                    outer.Add((c, d, o));
                }
            // Kapı: dışı yürünebilir açık karo ya da yol; yola değen önce, sonra binanın kapısına en yakın.
            (GridPos inside, Dir4 d, GridPos outside)? gate = null;
            int best = int.MaxValue;
            foreach (var e in outer)
            {
                bool road = map.GetRoad(e.outside) != RoadType.None;
                var far = Facing.Step(e.outside, e.d);
                if (!road && !Open(ctx, e.outside)) continue;
                if (IsBand(e.outside) || IsBand(far) || (map.GetRoad(far) == RoadType.None && !Open(ctx, far))) continue;   // kapı önü iki karo açık kalır
                int score = (road || OutdoorRules.TouchesRoad(map, e.outside) ? 0 : 1000) + Math.Abs(e.outside.X - doorOut.X) + Math.Abs(e.outside.Y - doorOut.Y);
                if (score < best) { best = score; gate = e; }
            }
            if (gate == null) return false;
            foreach (var e in outer)
                map.SetEdge(Facing.EdgeOf(e.inside, e.d), e.Equals(gate.Value) ? EdgeKind.Gate : EdgeKind.Fence);
            ctx.Reserved.Add(gate.Value.outside);
            ctx.Reserved.Add(Facing.Step(gate.Value.outside, gate.Value.d));
            ctx.Reserved.Add(gate.Value.inside);
            return true;
        }

        private static void Pen(Context ctx)
        {
            var pen = Regions.PenRect;
            var view = new TownSiteView { Building = new RectI(0, 0, 0, 0), Name = "Hayvan pazarı", Pen = new HashSet<GridPos>(), Zone = new HashSet<GridPos>(), Wide = new HashSet<GridPos>() };
            for (int y = pen.Y; y < pen.Y + pen.H; y++) for (int x = pen.X; x < pen.X + pen.W; x++) { var p = new GridPos(x, y); if (Open(ctx, p)) view.Pen.Add(p); }
            RunGarden(ctx, view, "yard_animals", "Hayvan pazarı (ağıl)");
        }

        private static void Farm(Context ctx)
        {
            var house = new RectI(StarterWorld.HouseX0, StarterWorld.HouseY0, StarterWorld.HouseX1 - StarterWorld.HouseX0 + 1, StarterWorld.HouseY1 - StarterWorld.HouseY0 + 1);
            // Kuyu bir yol kesişiminin yanında olur: kapı patikasından batıya kuyuya giden kısa bir patika ayrılır (T kavşak).
            int spurY = StarterWorld.HouseY1 + 5;
            for (int x = StarterWorld.HouseDoorX - 1; x >= StarterWorld.HouseDoorX - 5; x--)
            {
                var p = new GridPos(x, spurY);
                if (ctx.Map.GetFloor(p) == FloorType.Grass && !ctx.Map.IsBlocked(p)) ctx.Map.SetRoad(p, RoadType.Path);
            }
            ctx.JunctionList = null;
            var view = new TownSiteView
            {
                Building = house, DoorDir = Dir4.S, DoorOutside = new GridPos(StarterWorld.HouseDoorX, StarterWorld.HouseY1 + 1), Name = "Çiftlik evi",
                Zone = FarmZone(ctx, house, 8), Wide = FarmZone(ctx, house, 10),
            };
            RunGarden(ctx, view, "garden_farm", "Çiftlik avlusu");
            // Avlu masasının üstünde dokuz taş (eski başlangıç evinden).
            foreach (var it in ctx.World.Items.ToList())
                if (it.Def.Id == "item.stump_table" && view.Zone.Contains(it.Position) && ctx.Content.Items.TryGet("item.morris_board", out var board)
                    && ctx.World.CanPlaceOnSurface(board, it.Id, "a").Ok)
                { ctx.World.PlaceOnSurface(board, it.Id, "a"); break; }
        }

        private static HashSet<GridPos> FarmZone(Context ctx, RectI b, int reach)
        {
            var z = new HashSet<GridPos>();
            for (int y = b.Y - reach; y < b.Y + b.H + reach; y++)
                for (int x = b.X - reach; x < b.X + b.W + reach; x++)
                {
                    var p = new GridPos(x, y);
                    if (!b.Contains(p) && Open(ctx, p) && x < 40) z.Add(p);   // tarla (x ≥ 42) avluya karışmaz
                }
            return z;
        }

        // ---------------- yol kesişimleri ----------------

        private static void Crossroads(Context ctx)
        {
            if (!ctx.Content.Items.TryGet("item.signpost", out var post)) return;
            var placed = new List<ItemInstance>();
            var rep = new FurnishReport { Room = "Yol kesişimleri (tabela)" };
            foreach (var j in OutdoorRules.Junctions(ctx.Map))
            {
                var r = Regions.At(j);
                if (r != Regions.Town && r != Regions.Road) continue;
                if (Regions.PlazaRect.Contains(j)) continue;
                if (placed.Any(p => Math.Abs(p.Position.X - j.X) + Math.Abs(p.Position.Y - j.Y) < 10)) continue;
                var cands = new List<(GridPos, int, double)>();
                for (int dy = -3; dy <= 3; dy++) for (int dx = -3; dx <= 3; dx++)
                {
                    var p = new GridPos(j.X + dx, j.Y + dy);
                    if (OutdoorRules.TouchesRoad(ctx.Map, p) && BuildingClear(ctx, p, 2)) cands.Add((p, 0, -Math.Abs(dx) - Math.Abs(dy) + ctx.Rng.NextDouble()));
                }
                TryAt(ctx, post, cands, placed);
            }
            Validate(ctx, placed, rep);
            rep.Placed.AddRange(placed);
            ctx.Reports.Add(rep);
        }

        // ---------------- kenar bandı ve kaya sırtı (belge 64a, taslak) ----------------

        private static readonly string[] Thickets = { "item.edge_thicket_oak", "item.edge_thicket_pine", "item.edge_thicket_young" };

        /// <summary>
        /// Taslağı olan bölgede kenar bandını sık ağaç öbekleri ve gerçek ağaçlarla, kaya sırtını iri kaya ve kütükle doldurur.
        /// Bant düz bir kesim değil, iç sınırı girintili; öbekler 2x2 hücrelerde, hücre içinde kayık konur (dizi görünmez).
        /// </summary>
        private static void EdgeBand(Context ctx, Region region, int seed)
        {
            var data = MapFiles.Get(region.Id);
            if (data == null || !Regions.Exists(ctx.Map, region)) return;
            var rng = new LayoutRng(seed);
            var placed = new List<ItemInstance>();
            var rep = new FurnishReport { Room = $"{region.Id}: edge band" };
            for (int cy = 0; cy < data.H; cy += 2)
                for (int cx = 0; cx < data.W; cx += 2)
                {
                    // Hücrenin bant ya da kaya karoları; birini kayık seç.
                    var cells = new List<(int x, int y, MapTerrain t)>();
                    for (int dy = 0; dy < 2; dy++) for (int dx = 0; dx < 2; dx++)
                    {
                        var t = data.At(cx + dx, cy + dy);
                        if (t == MapTerrain.Band || t == MapTerrain.Rock) cells.Add((cx + dx, cy + dy, t));
                    }
                    if (cells.Count == 0) continue;
                    var (x, y, kind) = cells[rng.Next(cells.Count)];
                    var p = new GridPos(region.X + x, region.Y + y);
                    if (!Open(ctx, p)) continue;
                    double r = rng.NextDouble();
                    if (kind == MapTerrain.Rock)
                    {
                        string id = r < 0.3 ? "item.boulder_large" : r < 0.45 ? "item.fallen_log" : r < 0.75 ? "item.boulder" : "item.boulder_round";
                        if (Put(ctx, id, p, rng.Next(2), placed) == null) Put(ctx, "item.boulder", p, 0, placed);
                        continue;
                    }
                    // Bandın içi (bölge kenarına yakın) öbek, iç sınırı gerçek ağaç: oyuncu kenara ağaç gibi bakar, arkası koyu orman.
                    int edge = Math.Min(Math.Min(x, data.W - 1 - x), Math.Min(y, data.H - 1 - y));
                    if (edge <= 1 || r < 0.5) Put(ctx, Thickets[rng.Next(Thickets.Length)], p, rng.Next(2), placed, grown: false);
                    else
                    {
                        double s = rng.NextDouble();
                        var tree = s < 0.45 ? "item.tree_oak" : s < 0.75 ? "item.tree_birch" : "item.tree_pine";
                        var it = Put(ctx, tree, p, 0, placed);
                        if (it != null && rng.NextDouble() < 0.25) it.GrowthDays = it.Def.GrowDays * (0.55 + 0.35 * rng.NextDouble());
                    }
                }
            rep.Placed.AddRange(placed);
            ctx.Reports.Add(rep);
        }

        /// <summary>Karo taslakta kenar bandı, kaya sırtı ya da markers "keep" alanında mı (bitki örtüsü buraya serpilmez).</summary>
        internal static bool Reserved(Region region, GridPos p)
        {
            var data = MapFiles.Get(region.Id);
            if (data == null) return false;
            var t = data.At(p.X - region.X, p.Y - region.Y);
            if (t == MapTerrain.Band || t == MapTerrain.Rock) return true;
            if (data.Markers.keep != null)
                foreach (var k in data.Markers.keep)
                {
                    int lx = p.X - region.X, ly = p.Y - region.Y;
                    if (lx >= k.x0 && lx <= k.x1 && ly >= k.y0 && ly <= k.y1) return true;
                }
            return false;
        }

        /// <summary>Karonun en yakın suya uzaklığı (en çok 'max').</summary>
        private static int WaterDistance(GridMap map, GridPos p, int max)
        {
            for (int d = 0; d <= max; d++)
                for (int dy = -d; dy <= d; dy++)
                    for (int dx = -d; dx <= d; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != d) continue;
                        var q = new GridPos(p.X + dx, p.Y + dy);
                        if (map.InBounds(q) && map.IsWater(q)) return d;
                    }
            return max + 1;
        }

        // ---------------- köy yolu ----------------

        /// <summary>Gezgin tüccar durağı (yola bakan bank, arkasında çit; fıçı ve tohum sandığı) ve toplama düğümleri.</summary>
        private static void VillageRoad(Context ctx, int seed)
        {
            var map = ctx.Map;
            var placed = new List<ItemInstance>();
            var rep = new FurnishReport { Room = "Köy yolu" };
            var road = Regions.Road;
            // Durak: doğu ucuna yakın, yolun kuzeyinde (bank güneye bakar: arkası dönük) değil, güneyinde N'ye bakan bank.
            if (ctx.Content.Items.TryGet("item.garden_bench", out var bench))
            {
                int rot = ItemFacing.RotationFor(bench, Dir4.N);
                bool done = false;
                for (int x = road.X + road.W - 6; x > road.X + 20 && !done; x--)
                    for (int y = road.Y; y < road.Y + road.H && !done; y++)
                    {
                        var p = new GridPos(x, y);
                        var q = new GridPos(x + 1, y);
                        var front = new GridPos(x, y - 1);
                        if (!Open(ctx, p) || !Open(ctx, q) || map.GetRoad(front) == RoadType.None || map.GetRoad(new GridPos(x + 1, y - 1)) == RoadType.None) continue;
                        if (!ctx.World.CanPlaceFloor(bench, p, rot).Ok) continue;
                        map.SetEdge(Facing.EdgeOf(p, Dir4.S), EdgeKind.Fence);
                        map.SetEdge(Facing.EdgeOf(q, Dir4.S), EdgeKind.Fence);
                        placed.Add(ctx.World.PlaceItem(bench, p, rot));
                        Put(ctx, "item.barrel", new GridPos(x + 2, y), 0, placed);
                        Put(ctx, "item.seed_crate", new GridPos(x - 1, y), 0, placed);
                        done = true;
                    }
            }
            Validate(ctx, placed, rep);
            rep.Placed.AddRange(placed.Where(p => ctx.World.GetItem(p.Id) != null));
            ctx.Reports.Add(rep);
            // Toplama düğümleri: her türden bir tane; dere boyunca ıslak sevenler, yol kenarında ötekiler.
            var forage = new[] { "item.forage_berry_bush", "item.forage_strawberry", "item.forage_dry_branch", "item.forage_elderflower",
                                 "item.forage_mushroom", "item.forage_wild_garlic", "item.forage_nettle" };
            var rng = new LayoutRng(Seeds.Derive(seed, 4407));
            foreach (var id in forage)
            {
                if (!ctx.Content.Items.TryGet(id, out var def)) continue;
                bool wet = def.HasTag("rain_loving");
                for (int tries = 0; tries < 200; tries++)
                {
                    var p = new GridPos(road.X + rng.Next(road.W), road.Y + rng.Next(road.H));
                    if (!Open(ctx, p)) continue;
                    if (Reserved(road, p)) continue;
                    int stream = WaterDistance(map, p, 5);
                    if (wet ? stream > 4 || stream < 2 : stream < 3) continue;
                    if (Put(ctx, id, p, 0, null, grown: true) != null) break;
                }
            }
        }
    }
}
