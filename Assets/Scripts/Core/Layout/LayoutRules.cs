using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Doğrulayıcının eşya kuralları (belge 44 B): etkileşim, yön (oturma), ilişki (komodin, ocak, uzun eşya), oda türü,
    /// iç/dış mekân ve görsel yönü. Dış mekân kuralları (Aşama D) OutdoorRules'ta.
    /// </summary>
    public static class LayoutRules
    {
        /// <summary>Oturma eşyasının baktığı odak: masa ve yüzeyler, ocak ve şenlik ateşi, şarkıcı kürsüsü (etiket "focus").</summary>
        public static bool IsFocus(ItemDef d) =>
            d.Category == "surfaces" || d.HeatOutput > 0f || d.HasTag("focus") || d.Id == "item.market_stall";

        public static bool IsBed(ItemDef d) => d.Category == "sleep" && d.Width * d.Height >= 2;
        public static bool IsHearth(ItemDef d) => d.HasTag("hearth") || (d.HeatOutput > 0f && d.Placement == ItemPlacement.Floor && d.Id != "item.candlestick");

        /// <summary>Oturma eşyasının odağa uzaklığı (karo): masa hemen önünde (1), ocak ve kürsü en çok 3.</summary>
        public const int TableReach = 1, FocusReach = 3;

        public static void AddAll(PlacementValidator v, ContentDatabase content)
        {
            v.ExtraRules.Add(Items);
            OutdoorRules.Add(v);
        }

        private static void Items(PlacementValidator v)
        {
            var map = v.Map;
            HashSet<GridPos> reach = null;
            HashSet<GridPos> Reachable()
            {
                if (reach != null) return reach;
                // Kapıdan ulaşılabilirlik: bütün yol karolarından (kapılar yola bağlıdır, Aşama D) çok kaynaklı arama.
                reach = new HashSet<GridPos>();
                var seeds = new List<GridPos>();
                for (int y = 0; y < map.Height; y++)
                    for (int x = 0; x < map.Width; x++)
                    {
                        var p = new GridPos(x, y);
                        if (map.GetRoad(p) != RoadType.None && !map.IsBlocked(p)) seeds.Add(p);
                    }
                foreach (var s in seeds)
                    if (!reach.Contains(s)) foreach (var t in Reach.From(map, s, q => !reach.Contains(q))) reach.Add(t);
                // Yolu olmayan dünya (testler, yol çizilmemiş arsa): kapıların dış karoları kaynak olur.
                if (seeds.Count == 0)
                    foreach (var d in PlacementValidator.Doors(map))
                        foreach (var t in Reach.From(map, d.Exterior ? d.Outside : d.Inside)) reach.Add(t);
                return reach;
            }

            foreach (var it in v.World.Items)
            {
                if (!v.Scoped(it.Position)) continue;
                var d = it.Def;
                bool floor = PlacementValidator.Occupies(it);
                var room = map.Rooms.RoomAt(it.Position);
                bool indoor = room != null && room.IsIndoor;

                // ---- görsel yönü ----
                if (floor && !ItemFacing.IsValid(d, it.Rotation))
                    v.Add(Rules.NoView, it.Position, it, $"{d.DisplayName}: {Facing.ScreenName(ItemFacing.FacingFor(d, it.Rotation))} yönünün görseli yok.");

                // ---- iç/dış ----
                if ((floor || it.IsWallMounted) && !d.IsPlant)
                {
                    if (indoor && d.Site == ItemSite.Outdoor) v.Add(Rules.Site, it.Position, it, $"{d.DisplayName} dış mekân eşyası, kapalı odada.");
                    if (!indoor && d.Site == ItemSite.Indoor && it.Position.Z == 0) v.Add(Rules.Site, it.Position, it, $"{d.DisplayName} iç mekân eşyası, dışarıda.");
                }

                // ---- oda türü / dükkân ----
                if (v.Plan != null && (floor || it.IsWallMounted || it.IsCeiling) && !d.IsPlant && !d.Anywhere && (d.Belongs.Count > 0 || d.Trades.Count > 0))
                {
                    var zone = v.Plan.ZoneAt(it.Position);
                    if (zone != null && !Fits(d, zone))
                        v.Add(Rules.WrongRoom, it.Position, it, $"{d.DisplayName} {RoomName(zone)} için uygun değil.");
                }

                // ---- duvar eşyası kameraya arkası dönük yüzde (ön duvarın iç yüzü): yukarı kipinde çizilmez ----
                if (it.IsWallMounted)
                {
                    foreach (var seg in it.WallSegments())
                        if (seg.tile == seg.edge.SideB)
                        {
                            v.Add(Rules.NoView, it.Position, it, $"{d.DisplayName} ön duvarın iç yüzünde (kenar {it.Rotation}); kamera bu yüzü yalnızca kesitte görür.", Severity.Warning);
                            break;
                        }
                }

                if (!floor) continue;
                var f = ItemFacing.Of(it);

                // ---- etkileşim ----
                if (d.HasFront && !d.IsSeat && f.HasValue)
                {
                    foreach (var c in ItemFacing.InteractionCells(it))
                    {
                        var occ = v.Occupant(c);
                        var edge = map.GetEdge(GridMap.EdgeBetween(Facing.Step(c, Facing.Opposite(f.Value)), c));
                        if (EdgeKinds.BlocksMovement(edge)) { v.Add(Rules.Interaction, it.Position, it, $"{d.DisplayName} önü {PlacementValidator.EdgeName(edge)}."); break; }
                        if (occ != null && occ != it) { v.Add(Rules.Interaction, it.Position, it, $"{d.DisplayName} önü dolu: {occ.Def.DisplayName}."); break; }
                        if (!map.HasTile(c) || map.IsWater(c)) { v.Add(Rules.Interaction, it.Position, it, $"{d.DisplayName} önünde durulacak yer yok."); break; }
                        if (!Reachable().Contains(c)) { v.Add(Rules.Interaction, it.Position, it, $"{d.DisplayName} önüne kapıdan ulaşılamıyor."); break; }
                    }
                }
                else if (!d.HasFront && d.Interactions.Count > 0 && !d.IsPlant && !d.IsForage && !d.IsStairs)
                {
                    bool any = false;
                    foreach (var t in it.OccupiedTiles())
                        foreach (var dir in Facing.All)
                        {
                            var n = Facing.Step(t, dir);
                            if (v.Occupant(n) == it || map.EdgeBlocks(GridMap.EdgeBetween(t, n))) continue;
                            if (Reachable().Contains(n)) any = true;
                        }
                    if (!any) v.Add(Rules.Interaction, it.Position, it, $"{d.DisplayName} hiçbir yandan kullanılamıyor (çevresi kapalı ya da ulaşılamıyor).");
                }

                // ---- yön: oturma eşyası (iç mekân; dış mekân bankı Aşama D kuralında) ----
                if (d.IsSeat && f.HasValue && indoor) CheckSeat(v, it, f.Value);

                // ---- ilişki ----
                if (d.Relation == Placements.Bedside && !Near(v, it, IsBed, 1))
                    v.Add(Rules.Relation, it.Position, it, $"{d.DisplayName} bir yatağın başucunda değil.");
                if (d.Relation == Placements.NearHearth && !Near(v, it, IsHearth, 2))
                    v.Add(Rules.Relation, it.Position, it, $"{d.DisplayName} ocaktan uzak.");
                if (d.Tall && indoor)
                {
                    bool wall = false;
                    if (f.HasValue)
                    {
                        wall = true;
                        foreach (var (_, e) in ItemFacing.BackEdges(d, it.Position, it.Rotation, f.Value))
                            if (map.GetEdge(e) != EdgeKind.Wall && map.GetEdge(e) != EdgeKind.Window) { wall = false; break; }
                    }
                    else
                    {
                        // Ön yüzsüz uzun eşya: uzun kenarlarından biri boydan boya duvar.
                        foreach (var side in Facing.All)
                        {
                            bool all = true;
                            foreach (var (_, e) in ItemFacing.BackEdges(d, it.Position, it.Rotation, Facing.Opposite(side)))
                                if (map.GetEdge(e) != EdgeKind.Wall && map.GetEdge(e) != EdgeKind.Window) { all = false; break; }
                            if (all) { wall = true; break; }
                        }
                    }
                    if (!wall) v.Add(Rules.Relation, it.Position, it, $"{d.DisplayName} uzun eşya, sırtı duvara dayalı değil.");
                }
            }
        }

        /// <summary>
        /// Yapı modu önizlemesi (açık karar: kapı önü, etkileşim ve yön ihlali uyarı ve kırmızı vurgu, yerleştirme yine yapılır):
        /// hayalet eşyanın (def, konum, dönüş) yol açacağı yumuşak ihlaller. ignoreItemId: taşınan eşyanın kendisi.
        /// </summary>
        public static List<Violation> Preview(WorldState world, ItemDef def, GridPos pos, int rotation, int? ignoreItemId = null)
        {
            var list = new List<Violation>();
            if (def == null || def.Placement != ItemPlacement.Floor) return list;
            var map = world.Map;
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            var foot = new HashSet<GridPos>();
            for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) foot.Add(new GridPos(pos.X + dx, pos.Y + dy, pos.Z));
            void Warn(string code, GridPos at, string reason) =>
                list.Add(new Violation { Code = code, At = at, ItemDefId = def.Id, Reason = reason, Severity = Severity.Warning });
            ItemInstance Occ(GridPos p)
            {
                var o = world.GetItemAt(p);
                return o != null && o.Id != ignoreItemId && PlacementValidator.Occupies(o) ? o : null;
            }

            // Kapı önü
            foreach (var d in PlacementValidator.Doors(map))
            {
                if (d.Inside.Z != pos.Z) continue;
                if (foot.Contains(d.Inside)) { Warn(Rules.DoorFront, d.Inside, "Kapının önünü kapatıyor."); break; }
                var o = d.Outside; bool hit = false;
                int depth = d.Exterior ? PlacementValidator.DoorOutside : PlacementValidator.DoorInside;
                for (int i = 0; i < depth; i++) { if (foot.Contains(o)) { hit = true; break; } o = Facing.Step(o, d.OutDir); }
                if (hit) { Warn(Rules.DoorFront, o, "Kapının önünü kapatıyor."); break; }
            }
            // Kendi önü
            if (def.HasFront && !def.IsSeat)
                foreach (var c in ItemFacing.FrontCells(def, pos, rotation, def.FrontDepth))
                {
                    var from = Facing.Step(c, Facing.Opposite(ItemFacing.FacingFor(def, rotation)));
                    var k = map.GetEdge(GridMap.EdgeBetween(from, c));
                    var o = Occ(c);
                    if (EdgeKinds.BlocksMovement(k) || o != null || !map.HasTile(c))
                    { Warn(Rules.Interaction, c, o != null ? $"Önü dolu: {o.Def.DisplayName}." : "Önü duvara bakıyor, kullanılamaz."); break; }
                }
            // Başka eşyanın önünü kapatma
            foreach (var other in world.Items)
            {
                if (other.Id == ignoreItemId || other.Position.Z != pos.Z || !other.Def.HasFront || other.Def.IsSeat) continue;
                foreach (var c in ItemFacing.InteractionCells(other))
                    if (foot.Contains(c)) { Warn(Rules.Interaction, c, $"{other.Def.DisplayName} önünü kapatıyor."); goto doneOthers; }
            }
            doneOthers:
            // Oturma yönü (iç mekân)
            if (def.IsSeat)
            {
                var room = map.Rooms.RoomAt(pos);
                if (room != null && room.IsIndoor)
                {
                    var f = ItemFacing.FacingFor(def, rotation);
                    bool wall = false, focus = false;
                    foreach (var c in ItemFacing.FrontCells(def, pos, rotation))
                    {
                        var k = map.GetEdge(GridMap.EdgeBetween(Facing.Step(c, Facing.Opposite(f)), c));
                        if (EdgeKinds.IsRoomBoundary(k) || EdgeKinds.BlocksMovement(k)) { wall = true; continue; }
                        var p = c;
                        for (int i = 0; i < FocusReach && !focus; i++)
                        {
                            var o = Occ(p);
                            if (o != null) { focus = (o.Def.Category == "surfaces" && i < TableReach) || (IsFocus(o.Def) && o.Def.Category != "surfaces"); break; }
                            var next = Facing.Step(p, f);
                            if (map.EdgeBlocks(GridMap.EdgeBetween(p, next))) break;
                            p = next;
                        }
                    }
                    if (wall) Warn(Rules.Facing, pos, "Duvara bakıyor.");
                    else if (!focus) Warn(Rules.Facing, pos, "Bir masaya ya da odağa bakmıyor.");
                }
            }
            return list;
        }

        private static void CheckSeat(PlacementValidator v, ItemInstance it, Dir4 f)
        {
            var map = v.Map;
            bool wall = false, focus = false;
            foreach (var c in ItemFacing.InteractionCells(it))
            {
                var from = Facing.Step(c, Facing.Opposite(f));
                var k = map.GetEdge(GridMap.EdgeBetween(from, c));
                if (EdgeKinds.IsRoomBoundary(k) || EdgeKinds.BlocksMovement(k)) { wall = true; continue; }
                // Önündeki karoda masa; ya da düz ilerisinde (en çok 3 karo) ocak/kürsü gibi bir odak.
                var p = c;
                for (int i = 0; i < FocusReach && !focus; i++)
                {
                    var occ = v.Occupant(p);
                    if (occ != null && occ != it)
                    {
                        if (occ.Def.Category == "surfaces" && i < TableReach) focus = true;
                        else if (IsFocus(occ.Def) && occ.Def.Category != "surfaces") focus = true;
                        break;
                    }
                    var next = Facing.Step(p, f);
                    if (map.EdgeBlocks(GridMap.EdgeBetween(p, next))) break;
                    p = next;
                }
            }
            if (wall) v.Add(Rules.Facing, it.Position, it, $"{it.Def.DisplayName} duvara bakıyor ({Facing.ScreenName(f)}).");
            else if (!focus) v.Add(Rules.Facing, it.Position, it, $"{it.Def.DisplayName} bir masaya ya da odağa bakmıyor ({Facing.ScreenName(f)}).");
        }

        private static bool Near(PlacementValidator v, ItemInstance it, System.Func<ItemDef, bool> what, int dist)
        {
            foreach (var t in it.OccupiedTiles())
                for (int dy = -dist; dy <= dist; dy++)
                    for (int dx = -dist; dx <= dist; dx++)
                    {
                        if (System.Math.Abs(dx) + System.Math.Abs(dy) > dist) continue;
                        var o = v.Occupant(new GridPos(t.X + dx, t.Y + dy, t.Z));
                        if (o != null && o != it && what(o.Def)) return true;
                    }
            return false;
        }

        /// <summary>Eşya bu oda bölgesine ait mi: dükkân/atölyede meslek etiketi ya da oda türü; diğer odalarda oda türü.</summary>
        public static bool Fits(ItemDef d, RoomZone zone)
        {
            if (d.Anywhere) return true;
            if (zone.Trade != null && d.HasTrade(zone.Trade)) return true;
            foreach (var t in zone.Types) if (d.Belongs.Contains(t)) return true;
            return false;
        }

        public static string RoomName(RoomZone z)
        {
            string t;
            switch (z.Type)
            {
                case Placements.Bedroom: t = "yatak odası"; break;
                case Placements.Kitchen: t = "mutfak"; break;
                case Placements.Dining: t = "yemek odası"; break;
                case Placements.Living: t = "oturma odası"; break;
                case Placements.Pantry: t = "kiler"; break;
                case Placements.Cellar: t = "mahzen"; break;
                case Placements.Storehouse: t = "ambar"; break;
                case Placements.Workshop: t = "atölye"; break;
                case Placements.Shop: t = "dükkân"; break;
                case Placements.InnRoom: t = "han odası"; break;
                case Placements.Barn: t = "ahır"; break;
                case Placements.Kids: t = "çocuk odası"; break;
                default: t = z.Type; break;
            }
            return z.Trade != null ? $"{t} ({z.Trade})" : t;
        }
    }
}
