using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>Döşenecek bir oda (tek kat): karoları, girişleri (kapı içi, merdiven inişi) ve boş kalacak karolar.</summary>
    public sealed class RoomSpec
    {
        public string Name;
        public int Level;
        public readonly HashSet<GridPos> Tiles = new HashSet<GridPos>();
        /// <summary>Odaya girilen karolar (kapının iç karosu, merdiven inişi): hepsi birbirine bağlı kalır.</summary>
        public readonly List<GridPos> Entrances = new List<GridPos>();
        /// <summary>Kapının dışarı yönü (tezgâh müşteri tarafı için); kapısız odada null.</summary>
        public Dir4? DoorDir;
        /// <summary>Hiç eşya konmayacak karolar (kapı önü, merdiven uçları, başlangıç noktaları).</summary>
        public readonly HashSet<GridPos> Keep = new HashSet<GridPos>();
        /// <summary>Doğrulama için oda bölgesi (tür ve meslek).</summary>
        public RoomZone Zone;
    }

    public sealed class FurnishReport
    {
        public string Room;
        public readonly List<ItemInstance> Placed = new List<ItemInstance>();
        /// <summary>Konamayan zorunlu eşyalar (şablon geçmedi).</summary>
        public readonly List<string> Missing = new List<string>();
        /// <summary>Doğrulayıcıdan geçmeyen ve çıkarılamayan (zorunlu) eşyaların ihlalleri.</summary>
        public readonly List<Violation> Violations = new List<Violation>();
        /// <summary>Doğrulayıcı yüzünden çıkarılan isteğe bağlı eşyalar.</summary>
        public readonly List<string> Dropped = new List<string>();
        public int Attempt;
        public int Seed;
        public bool Ok => Missing.Count == 0 && Violations.Count == 0;
    }

    /// <summary>Sabit tohumlu küçük üreteç (platformdan bağımsız: aynı tohum aynı odayı verir).</summary>
    public sealed class LayoutRng
    {
        private uint _s;
        public LayoutRng(int seed) { _s = (uint)seed * 2654435761u ^ 0x9E3779B9u; if (_s == 0) _s = 1; }
        public uint NextU() { _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5; return _s; }
        public double NextDouble() => (NextU() & 0xFFFFFF) / (double)0x1000000;
        public int Next(int n) => n <= 1 ? 0 : (int)(NextU() % (uint)n);
    }

    /// <summary>
    /// Oda döşeyici (belge 44 C). Süreç: oda geometrisi → kapı, merdiven ve boş kalacak karolar → her adımda ana eşya
    /// (duvara sırt, köşe, orta ...) → ilişkili eşya (sandalye masaya bakarak, komodin yatak başında, ocak eşyası ocak yanında)
    /// → süs yalnızca kalan geçerli yerlere (yüzey yuvası, arka duvar, tavan, halı) → doğrulayıcı. Her eşya konmadan önce
    /// dolaşım denetlenir: girişler, bütün etkileşim karoları ve merdiven uçları birbirine bağlı kalmalı.
    /// Doğrulayıcıdan geçmeyen isteğe bağlı eşya çıkarılır; zorunlu eşya geçmezse başka tohumla yeniden denenir.
    /// </summary>
    public sealed class RoomFurnisher
    {
        public const int MaxAttempts = 6;

        private readonly WorldState _w;
        private readonly ContentDatabase _c;
        private readonly LayoutPlan _plan;

        public RoomFurnisher(WorldState world, ContentDatabase content, LayoutPlan plan = null)
        {
            _w = world; _c = content; _plan = plan;
        }

        // Bir denemenin yerleştirme kaydı (en iyi deneme sonunda yeniden kurulur).
        private sealed class Desc
        {
            public ItemDef Def; public GridPos Pos; public int Rot; public string Kind; public int Side = -1; public int Parent = -1; public string Slot;
            public bool Optional;
        }

        private sealed class Attempt
        {
            public readonly List<Desc> Descs = new List<Desc>();
            public readonly List<ItemInstance> Items = new List<ItemInstance>();
            public readonly List<string> Missing = new List<string>();
            public readonly List<string> Dropped = new List<string>();
            public readonly List<Violation> Violations = new List<Violation>();
            public int Score => Missing.Count * 100 + Violations.Count * 10 + Dropped.Count;
        }

        public FurnishReport Furnish(RoomSpec room, RoomTemplate t, int seed)
        {
            Attempt best = null; int bestSeed = seed, bestIndex = 0;
            for (int a = 0; a < MaxAttempts; a++)
            {
                int s = seed + a * 7919;
                var at = Run(room, t, s);
                Prune(room, at);
                if (at.Missing.Count == 0 && at.Violations.Count == 0) return Report(room, at, s, a);
                if (best == null || at.Score < best.Score) { best = Snapshot(at); bestSeed = s; bestIndex = a; }
                Undo(at);
            }
            // Kusursuz deneme yok: en iyisi aynı kayıtla yeniden kurulur ve yeniden doğrulanır.
            Reapply(best);
            Prune(room, best);
            return Report(room, best, bestSeed, bestIndex);
        }

        private static FurnishReport Report(RoomSpec room, Attempt at, int seed, int attempt)
        {
            var rep = new FurnishReport { Room = room.Name, Attempt = attempt, Seed = seed };
            foreach (var it in at.Items) if (it != null) rep.Placed.Add(it);
            rep.Missing.AddRange(at.Missing);
            rep.Dropped.AddRange(at.Dropped);
            rep.Violations.AddRange(at.Violations);
            return rep;
        }

        /// <summary>Denemenin yalnızca yerinde kalan eşyalarının kaydı (çıkarılanlar ve onlara bağlı dekor atlanır).</summary>
        private static Attempt Snapshot(Attempt at)
        {
            var c = new Attempt();
            var index = new Dictionary<int, int>();
            for (int i = 0; i < at.Descs.Count; i++)
            {
                if (at.Items[i] == null) continue;
                var d = at.Descs[i];
                if (d.Kind == "surface" && !index.ContainsKey(d.Parent)) continue;
                var copy = new Desc { Def = d.Def, Pos = d.Pos, Rot = d.Rot, Kind = d.Kind, Side = d.Side, Slot = d.Slot, Optional = d.Optional,
                                      Parent = d.Kind == "surface" ? index[d.Parent] : -1 };
                index[i] = c.Descs.Count;
                c.Descs.Add(copy);
            }
            c.Missing.AddRange(at.Missing); c.Dropped.AddRange(at.Dropped);
            return c;
        }

        private void Undo(Attempt at)
        {
            for (int i = at.Items.Count - 1; i >= 0; i--)
                if (at.Items[i] != null && _w.GetItem(at.Items[i].Id) != null) _w.RemoveItem(at.Items[i].Id);
        }

        private void Reapply(Attempt at)
        {
            at.Items.Clear();
            for (int i = 0; i < at.Descs.Count; i++)
            {
                var d = at.Descs[i];
                ItemInstance it = null;
                try
                {
                    switch (d.Kind)
                    {
                        case "wall": it = _w.PlaceOnWall(d.Def, d.Pos, d.Side); break;
                        case "ceiling": it = _w.PlaceOnCeiling(d.Def, d.Pos); break;
                        case "surface":
                            var parent = d.Parent >= 0 && d.Parent < at.Items.Count ? at.Items[d.Parent] : null;
                            if (parent != null) it = _w.PlaceOnSurface(d.Def, parent.Id, d.Slot);
                            break;
                        default: it = _w.PlaceItem(d.Def, d.Pos, d.Rot); break;
                    }
                }
                catch (InvalidOperationException) { it = null; }
                at.Items.Add(it);
            }
        }

        // ---------------- tek deneme ----------------

        private sealed class Ctx
        {
            public RoomSpec Room;
            public LayoutRng Rng;
            public Attempt At;
            /// <summary>Boş kalması gereken karolar: Keep + konmuş eşyaların ön karoları.</summary>
            public HashSet<GridPos> MustFree;
            /// <summary>Ulaşılabilir kalması gereken karolar (girişler, ön karolar) ve "en az bir yanı" istekleri.</summary>
            public List<GridPos[]> Reqs;
            public ItemInstance LastSurface;
            public ItemInstance LastBed;
            public List<GridPos> Sorted;
            public (double x, double y) Center;
        }

        private Attempt Run(RoomSpec room, RoomTemplate t, int seed)
        {
            var ctx = new Ctx
            {
                Room = room, Rng = new LayoutRng(seed), At = new Attempt(), MustFree = new HashSet<GridPos>(room.Keep),
                Reqs = new List<GridPos[]>(), Sorted = room.Tiles.OrderBy(p => p.Y).ThenBy(p => p.X).ToList(),
            };
            double cx = 0, cy = 0;
            foreach (var p in room.Tiles) { cx += p.X; cy += p.Y; }
            ctx.Center = (cx / Math.Max(1, room.Tiles.Count), cy / Math.Max(1, room.Tiles.Count));
            foreach (var e in room.Entrances) ctx.Reqs.Add(new[] { e });
            // Odada önceden duran eşyalar (merdiven) ve onların ön karoları.
            foreach (var it in _w.Items)
                if (PlacementValidator.Occupies(it) && room.Tiles.Contains(it.Position))
                    foreach (var c in ItemFacing.InteractionCells(it)) ctx.MustFree.Add(c);

            foreach (var step in t.Steps)
            {
                if (step.Chance < 1f && ctx.Rng.NextDouble() > step.Chance) continue;
                for (int k = 0; k < step.Count; k++)
                {
                    var id = step.Items[ctx.Rng.Next(step.Items.Count)];
                    if (!_c.Items.TryGet(id, out var def)) { if (!step.Optional) ctx.At.Missing.Add(id); continue; }
                    bool ok = Place(ctx, step, def);
                    // Seçenekli adımda ilki olmazsa diğer seçenekler denenir.
                    if (!ok && step.Items.Count > 1)
                        foreach (var alt in step.Items)
                            if (alt != id && _c.Items.TryGet(alt, out var altDef) && Place(ctx, step, altDef)) { ok = true; break; }
                    if (!ok && !step.Optional) ctx.At.Missing.Add(id);
                }
            }
            return ctx.At;
        }

        private bool Place(Ctx ctx, LayoutStep step, ItemDef def)
        {
            switch (step.With)
            {
                case Anchors.SeatsAround: return PlaceSeat(ctx, step, def);
                case Anchors.Bedside: return PlaceNear(ctx, step, def, o => LayoutRules.IsBed(o.Def), 1, ctx.LastBed);
                case Anchors.NearHearth: return PlaceNear(ctx, step, def, o => LayoutRules.IsHearth(o.Def), 2, null);
                case Anchors.NearItem: return PlaceNear(ctx, step, def, o => o.Def.Id == step.On || o.Def.Category == step.On, 2, null);
                case Anchors.OnSurface: return PlaceDecor(ctx, step, def);
                case Anchors.OnWall: return PlaceWall(ctx, def, step.Optional);
                case Anchors.Ceiling: return PlaceCeiling(ctx, def, step.Optional);
                case Anchors.Rug: return PlaceRug(ctx, def, step.Optional);
                default: return PlaceFloor(ctx, step, def, null);
            }
        }

        // ---------------- yer eşyası ----------------

        private static List<GridPos> Foot(ItemDef def, GridPos pos, int rot)
        {
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rot);
            var list = new List<GridPos>(w * h);
            for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) list.Add(new GridPos(pos.X + dx, pos.Y + dy, pos.Z));
            return list;
        }

        /// <summary>Ayak izinin s yönündeki kenarı boydan boya duvar mı (uzun eşyada pencere sayılmaz).</summary>
        private bool Walled(ItemDef def, GridPos pos, int rot, Dir4 side)
        {
            foreach (var (_, e) in ItemFacing.BackEdges(def, pos, rot, Facing.Opposite(side)))
            {
                var k = _w.Map.GetEdge(e);
                if (k == EdgeKind.Wall) continue;
                if (k == EdgeKind.Window && !def.Tall) continue;
                return false;
            }
            return true;
        }

        private bool Touches(GridPos c, Dir4 d) => EdgeKinds.IsRoomBoundary(_w.Map.GetEdge(Facing.EdgeOf(c, d)));

        private double? AnchorScore(Ctx ctx, ItemDef def, GridPos pos, int rot, List<GridPos> foot, string anchor)
        {
            var room = ctx.Room;
            Dir4? f = def.HasFront ? ItemFacing.FacingFor(def, rot) : (Dir4?)null;
            double score = 0;
            // Görseli olmayan eşyada da ön yüz kameraya, sırt arka duvara (doğu, güney) gelsin: oda boyunca aynı görünüm.
            if (f.HasValue && Facing.FacesCamera(f.Value)) score += 1;
            switch (anchor)
            {
                case Anchors.Wall:
                case Anchors.BackWall:
                case Anchors.Corner:
                {
                    Dir4? wallSide = null;
                    // Ön yüzsüz eşya önce ön duvarlara (batı, kuzey): arka duvarlar kameraya dönük dolap ve tezgâhlara kalsın.
                    // Uzun ön yüzsüz eşya arka duvara (önündeki odayı örtmesin).
                    var order = def.Tall ? new[] { Dir4.E, Dir4.S, Dir4.N, Dir4.W } : new[] { Dir4.W, Dir4.N, Dir4.E, Dir4.S };
                    if (f.HasValue) { if (Walled(def, pos, rot, Facing.Opposite(f.Value))) wallSide = Facing.Opposite(f.Value); }
                    else foreach (var s in order) if (Walled(def, pos, rot, s)) { wallSide = s; break; }
                    if (!wallSide.HasValue) return null;
                    bool back = wallSide == Dir4.E || wallSide == Dir4.S;
                    if (f.HasValue || def.Tall) { if (back) score += 2; else if (anchor == Anchors.BackWall) return null; }
                    else score += back ? 0.5 : 1.5;
                    if (f.HasValue)                                                           // ön karolar odada olmalı
                        foreach (var c in ItemFacing.FrontCells(def, pos, rot)) if (!room.Tiles.Contains(c)) return null;
                    if (anchor == Anchors.Corner)
                    {
                        var side = wallSide.Value;
                        var perp = new[] { Facing.Rotate(side, 1), Facing.Rotate(side, 3) };
                        if (!perp.Any(p => Walled(def, pos, rot, p))) return null;
                        score += 1;
                    }
                    return score;
                }
                case Anchors.Center:
                {
                    // Çevresinde dolaşılır ve oturulur: bir karoluk halka odada, duvarsız ve başka eşyasız.
                    foreach (var c in foot)
                        foreach (var d in Facing.All)
                        {
                            var n = Facing.Step(c, d);
                            if (foot.Contains(n)) continue;
                            if (!room.Tiles.Contains(n) || Touches(c, d) || _w.Map.IsBlocked(n)) return null;
                        }
                    double mx = foot.Average(p => p.X), my = foot.Average(p => p.Y);
                    return score - (Math.Abs(mx - ctx.Center.x) + Math.Abs(my - ctx.Center.y)) * 0.5;
                }
                case Anchors.NearDoor:
                {
                    int dist = DistToEntrance(ctx, foot);
                    if (dist > 3) return null;
                    return score - dist;
                }
                case Anchors.Far:
                    return score + DistToEntrance(ctx, foot) * 0.6;
                case Anchors.Counter:
                {
                    // Tezgâh: kapı duvarına paralel, kapıya 2-4 karo; kapı tarafı (müşteri) boş, arkasına dükkâncı geçer.
                    if (!room.DoorDir.HasValue) return null;
                    var dd = room.DoorDir.Value;
                    if (f.HasValue && f.Value != dd) return null;
                    var (fw, fh) = Footprint.Dimensions(def.Width, def.Height, rot);
                    bool parallel = dd == Dir4.N || dd == Dir4.S ? fw >= fh : fh >= fw;
                    if (!parallel) return null;
                    int dist = DistToEntrance(ctx, foot);
                    if (dist < 2 || dist > 4) return null;
                    foreach (var c in foot)
                    {
                        var cust = Facing.Step(c, dd); var keeper = Facing.Step(c, Facing.Opposite(dd));
                        if ((!foot.Contains(cust) && !room.Tiles.Contains(cust)) || (!foot.Contains(keeper) && !room.Tiles.Contains(keeper))) return null;
                    }
                    return score - dist;
                }
                default:
                    return score;
            }
        }

        private static int DistToEntrance(Ctx ctx, List<GridPos> foot)
        {
            int best = int.MaxValue;
            foreach (var e in ctx.Room.Entrances)
                foreach (var c in foot) best = Math.Min(best, Math.Abs(c.X - e.X) + Math.Abs(c.Y - e.Y));
            return best == int.MaxValue ? 0 : best;
        }

        private bool PlaceFloor(Ctx ctx, LayoutStep step, ItemDef def, Func<GridPos, int, List<GridPos>, double?> extra)
        {
            if (def.Placement != ItemPlacement.Floor) return false;
            var cands = new List<(GridPos pos, int rot, double score)>();
            foreach (var rot in ItemFacing.ValidRotations(def))
                foreach (var pos in ctx.Sorted)
                {
                    var foot = Foot(def, pos, rot);
                    if (!FootOk(ctx, foot)) continue;
                    double? sc = extra != null ? extra(pos, rot, foot) : AnchorScore(ctx, def, pos, rot, foot, step.Anchor);
                    if (!sc.HasValue) continue;
                    if (!_w.CanPlaceFloor(def, pos, rot).Ok) continue;
                    cands.Add((pos, rot, sc.Value + ctx.Rng.NextDouble() * 0.9));
                }
            foreach (var c in cands.OrderByDescending(c => c.score))
                if (TryCommit(ctx, def, c.pos, c.rot, step.Anchor == Anchors.Counter, step.Optional)) return true;
            return false;
        }

        private bool FootOk(Ctx ctx, List<GridPos> foot)
        {
            foreach (var c in foot) if (!ctx.Room.Tiles.Contains(c) || ctx.MustFree.Contains(c)) return false;
            return true;
        }

        /// <summary>Eşyayı koymadan önce dolaşımı denetler: girişler, ön karolar ve "en az bir yan" istekleri ulaşılabilir kalmalı.</summary>
        private bool TryCommit(Ctx ctx, ItemDef def, GridPos pos, int rot, bool counter, bool optional)
        {
            var foot = Foot(def, pos, rot);
            var footSet = new HashSet<GridPos>(foot);
            var newReqs = new List<GridPos[]>();
            var newFree = new List<GridPos>();
            if (def.HasFront && !def.IsSeat)
            {
                foreach (var c in ItemFacing.FrontCells(def, pos, rot, def.FrontDepth))
                {
                    if (!ctx.Room.Tiles.Contains(c) || _w.Map.IsBlocked(c)) return false;
                    var back = Facing.Step(c, Facing.Opposite(ItemFacing.FacingFor(def, rot)));
                    if (_w.Map.EdgeBlocks(GridMap.EdgeBetween(back, c))) return false;
                    newFree.Add(c);
                }
                if (newFree.Count > 0) newReqs.Add(newFree.ToArray());
            }
            if (counter && ctx.Room.DoorDir.HasValue)
            {
                // Tezgâhın müşteri tarafı (kapıya bakan) boş kalır ve ulaşılır; arkası (dükkâncının yeri) da ulaşılır.
                var dd = ctx.Room.DoorDir.Value;
                var cust = foot.Select(c => Facing.Step(c, dd)).Where(c => !footSet.Contains(c) && ctx.Room.Tiles.Contains(c)).ToArray();
                var behind = foot.Select(c => Facing.Step(c, Facing.Opposite(dd))).Where(c => !footSet.Contains(c) && ctx.Room.Tiles.Contains(c)).ToArray();
                if (cust.Length == 0 || behind.Length == 0) return false;
                foreach (var c in cust) { if (_w.Map.IsBlocked(c)) return false; newFree.Add(c); }
                newReqs.Add(cust); newReqs.Add(behind);
            }
            else if (!def.IsSeat && (def.Interactions.Count > 0 || def.Storage > 0 || def.SurfaceSlots.Count > 0))
            {
                var around = new List<GridPos>();
                foreach (var c in foot)
                    foreach (var d in Facing.All)
                    {
                        var n = Facing.Step(c, d);
                        if (!footSet.Contains(n) && ctx.Room.Tiles.Contains(n) && !_w.Map.EdgeBlocks(GridMap.EdgeBetween(c, n))) around.Add(n);
                    }
                if (around.Count == 0) return false;
                newReqs.Add(around.ToArray());
            }
            // Ön karoları başka eşyanın ön karosuna ya da boş kalacak karoya denk gelmesin.
            foreach (var c in foot) if (ctx.MustFree.Contains(c)) return false;

            // Dolaşım: ilk girişten oda içinde arama.
            if (ctx.Room.Entrances.Count > 0)
            {
                var start = ctx.Room.Entrances[0];
                if (footSet.Contains(start)) return false;
                var reach = Reach.From(_w.Map, start, p => ctx.Room.Tiles.Contains(p), p => footSet.Contains(p));
                foreach (var req in ctx.Reqs.Concat(newReqs))
                {
                    bool any = false;
                    foreach (var c in req) if (reach.Contains(c) && !footSet.Contains(c)) { any = true; break; }
                    if (!any) return false;
                }
            }

            ItemInstance it;
            try { it = _w.PlaceItem(def, pos, rot); }
            catch (InvalidOperationException) { return false; }
            foreach (var c in newFree) ctx.MustFree.Add(c);
            ctx.Reqs.AddRange(newReqs);
            Record(ctx, it, new Desc { Def = def, Pos = pos, Rot = rot, Kind = "floor", Optional = optional });
            if (def.Category == "surfaces" || LayoutRules.IsFocus(def) && def.Category != "heat_light") ctx.LastSurface = it;
            if (LayoutRules.IsBed(def)) ctx.LastBed = it;
            return true;
        }

        private void Record(Ctx ctx, ItemInstance it, Desc d)
        {
            ctx.At.Items.Add(it);
            ctx.At.Descs.Add(d);
        }

        // ---------------- ilişkili eşya ----------------

        private bool PlaceSeat(Ctx ctx, LayoutStep step, ItemDef def)
        {
            var targets = new List<ItemInstance>();
            if (step.On == null && ctx.LastSurface != null) targets.Add(ctx.LastSurface);
            foreach (var it in RoomItems(ctx))
                if (!targets.Contains(it) && (step.On != null ? it.Def.Id == step.On || it.Def.Category == step.On : it.Def.Category == "surfaces")) targets.Add(it);
            foreach (var target in targets)
            {
                var tf = new HashSet<GridPos>(target.OccupiedTiles());
                bool ok = PlaceFloor(ctx, step, def, (pos, rot, foot) =>
                {
                    if (foot.Any(tf.Contains)) return null;
                    if (def.HasFront)
                    {
                        var fronts = ItemFacing.FrontCells(def, pos, rot).ToList();
                        if (!fronts.All(tf.Contains)) return null;            // bütün oturak masaya baksın
                    }
                    else if (!foot.Any(c => Facing.All.Any(d => tf.Contains(Facing.Step(c, d))))) return null;
                    return 0;
                });
                if (ok) return true;
            }
            return false;
        }

        private bool PlaceNear(Ctx ctx, LayoutStep step, ItemDef def, Func<ItemInstance, bool> what, int dist, ItemInstance prefer)
        {
            var targets = RoomItems(ctx).Where(what).ToList();
            if (prefer != null && targets.Remove(prefer)) targets.Insert(0, prefer);
            foreach (var target in targets)
            {
                var tf = target.OccupiedTiles().ToList();
                var tset = new HashSet<GridPos>(tf);
                var tfront = new HashSet<GridPos>(ItemFacing.InteractionCells(target));
                bool ok = PlaceFloor(ctx, step, def, (pos, rot, foot) =>
                {
                    int best = int.MaxValue;
                    foreach (var c in foot)
                    {
                        if (tset.Contains(c) || tfront.Contains(c)) return null;
                        foreach (var t in tf) best = Math.Min(best, Math.Abs(c.X - t.X) + Math.Abs(c.Y - t.Y));
                    }
                    if (best > dist) return null;
                    var a = AnchorScore(ctx, def, pos, rot, foot, step.Anchor);
                    return a.HasValue ? a.Value - best : (double?)null;
                });
                if (ok) return true;
            }
            return false;
        }

        private IEnumerable<ItemInstance> RoomItems(Ctx ctx)
        {
            foreach (var it in _w.Items)
                if (PlacementValidator.Occupies(it) && ctx.Room.Tiles.Contains(it.Position)) yield return it;
        }

        // ---------------- süs ----------------

        private bool PlaceDecor(Ctx ctx, LayoutStep step, ItemDef def)
        {
            if (def.Placement != ItemPlacement.Surface) return false;
            var surfaces = new List<ItemInstance>();
            foreach (var it in _w.Items)
            {
                if (it.IsDecor || it.Def.SurfaceSlots.Count == 0) continue;
                if (!ctx.Room.Tiles.Contains(it.Position)) continue;
                bool match = step.On == null || it.Def.Id == step.On || it.Def.Category == step.On
                             || (step.On == "shelves" && (it.Def.Placement == ItemPlacement.Wall || it.Def.Category == "storage"));
                if (match) surfaces.Add(it);
            }
            foreach (var s in surfaces.OrderBy(_ => ctx.Rng.NextDouble()))
            {
                var desc = ctx.At.Items.IndexOf(s);
                foreach (var slot in s.Def.SurfaceSlots.OrderBy(_ => ctx.Rng.NextDouble()))
                {
                    if (!_w.CanPlaceOnSurface(def, s.Id, slot.Id).Ok) continue;
                    var it = _w.PlaceOnSurface(def, s.Id, slot.Id);
                    Record(ctx, it, new Desc { Def = def, Kind = "surface", Parent = desc, Slot = slot.Id, Optional = step.Optional });
                    return true;
                }
            }
            return false;
        }

        private bool PlaceWall(Ctx ctx, ItemDef def, bool optional)
        {
            if (def.Placement != ItemPlacement.Wall) return false;
            // Yalnızca kameraya bakan yüzler: karonun doğu (1) ve güney (2) kenarı (odanın arka duvarı).
            var cands = new List<(GridPos tile, int side, double score)>();
            foreach (var t in ctx.Sorted)
                foreach (var side in new[] { 1, 2 })
                {
                    if (_w.Map.GetEdge(WallSides.EdgeOf(t, side)) != (def.WindowMount ? EdgeKind.Window : EdgeKind.Wall)) continue;
                    if (!_w.CanPlaceOnWall(def, t, side).Ok) continue;
                    bool ok = true;
                    foreach (var (seg, _) in WallSides.Segments(t, side, Math.Max(1, def.WallWidth))) if (!ctx.Room.Tiles.Contains(seg)) ok = false;
                    if (ok) cands.Add((t, side, ctx.Rng.NextDouble()));
                }
            foreach (var c in cands.OrderByDescending(c => c.score))
            {
                ItemInstance it;
                try { it = _w.PlaceOnWall(def, c.tile, c.side); } catch (InvalidOperationException) { continue; }
                Record(ctx, it, new Desc { Def = def, Pos = c.tile, Side = c.side, Kind = "wall", Optional = optional });
                return true;
            }
            return false;
        }

        private bool PlaceCeiling(Ctx ctx, ItemDef def, bool optional)
        {
            if (!def.IsCeiling) return false;
            foreach (var t in ctx.Sorted.OrderBy(_ => ctx.Rng.NextDouble()))
            {
                if (!_w.CanPlaceCeiling(def, t).Ok) continue;
                var it = _w.PlaceOnCeiling(def, t);
                Record(ctx, it, new Desc { Def = def, Pos = t, Kind = "ceiling", Optional = optional });
                return true;
            }
            return false;
        }

        private bool PlaceRug(Ctx ctx, ItemDef def, bool optional)
        {
            if (!def.IsRug) return false;
            var cands = new List<(GridPos pos, int rot, double score)>();
            foreach (var rot in ItemFacing.ValidRotations(def))
                foreach (var pos in ctx.Sorted)
                {
                    var foot = Foot(def, pos, rot);
                    if (foot.Any(c => !ctx.Room.Tiles.Contains(c) || ctx.Room.Keep.Contains(c))) continue;
                    if (!_w.CanPlaceRug(def, pos, rot).Ok) continue;
                    double mx = foot.Average(p => p.X), my = foot.Average(p => p.Y);
                    cands.Add((pos, rot, -(Math.Abs(mx - ctx.Center.x) + Math.Abs(my - ctx.Center.y)) + ctx.Rng.NextDouble()));
                }
            foreach (var c in cands.OrderByDescending(c => c.score))
            {
                ItemInstance it;
                try { it = _w.PlaceItem(def, c.pos, c.rot); } catch (InvalidOperationException) { continue; }
                Record(ctx, it, new Desc { Def = def, Pos = c.pos, Rot = c.rot, Kind = "rug", Optional = optional });
                return true;
            }
            return false;
        }

        // ---------------- doğrulama ----------------

        /// <summary>Odayı doğrular; ihlali olan isteğe bağlı eşyaları çıkarır (zorunlu eşyanın ihlali kalır).</summary>
        private void Prune(RoomSpec room, Attempt at)
        {
            for (int pass = 0; pass < 8; pass++)
            {
                var v = Validate(room);
                var ours = new HashSet<int>(at.Items.Where(i => i != null).Select(i => i.Id));
                var bad = v.Where(x => x.Severity == Severity.Error && ours.Contains(x.ItemId)).ToList();
                at.Violations.Clear();
                if (bad.Count == 0) return;
                bool removed = false;
                foreach (var x in bad)
                {
                    int idx = at.Items.FindIndex(i => i != null && i.Id == x.ItemId);
                    if (idx < 0 || !at.Descs[idx].Optional) { at.Violations.Add(x); continue; }
                    if (_w.GetItem(x.ItemId) != null)
                    {
                        _w.RemoveItem(x.ItemId);
                        at.Dropped.Add($"{at.Descs[idx].Def.Id}: {x.Reason}");
                        removed = true;
                    }
                    at.Items[idx] = null;
                }
                // Çıkarılan eşyanın üstündeki dekorlar da gitti.
                for (int i = 0; i < at.Items.Count; i++) if (at.Items[i] != null && _w.GetItem(at.Items[i].Id) == null) at.Items[i] = null;
                if (!removed) return;
            }
        }

        public List<Violation> Validate(RoomSpec room)
        {
            var v = new PlacementValidator(_w, _plan, p => room.Tiles.Contains(p));
            v.ReachProvider = () =>
            {
                var reach = new HashSet<GridPos>();
                foreach (var e in room.Entrances) foreach (var t in Reach.From(_w.Map, e, p => room.Tiles.Contains(p))) reach.Add(t);
                return reach;
            };
            LayoutRules.AddAll(v, _c);
            return v.Validate();
        }
    }
}
