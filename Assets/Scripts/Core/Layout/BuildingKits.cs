using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Bina kimlik kartları (belge 64 §5): her bina dışarıdan ayırt edilsin, içindeki her şey aynı aileden görünsün.
    /// Arazi geçişinde (ApplyTerrain) dış duvar malzemesi, kapı, pencere, çatı, zemin ve iç duvar kaplaması; eşya geçişinde
    /// (ApplyFinishes) kaplama grubu olan mobilyalar kitin tek ahşap tonuna, tek kumaş rengine, metaline ve kiline boyanır.
    /// Birden çok kit seçeneği olan bina (köylü evleri) kendi kimliğinin özetiyle seçer ve yakındaki (6 karo) binayla aynı
    /// duvar + çatı bileşimini almaz. Yeni görsel gerekmez: depodaki yapı parçaları kullanılır.
    /// </summary>
    public static class BuildingKits
    {
        public const int NeighbourReach = 6;

        /// <summary>Şehir binalarının kitleri (bina kimliği → kit). Her çağrıda aynı sonucu verir.</summary>
        public static Dictionary<string, BuildingKit> Assign(LayoutCatalog cat)
        {
            var result = new Dictionary<string, BuildingKit>(StringComparer.Ordinal);
            if (cat == null) return result;
            var done = new List<(TownBuilding b, BuildingKit k)>();
            foreach (var b in Regions.TownBuildings)
            {
                var t = cat.Building(b.Template);
                if (t == null || t.Kits.Count == 0) continue;
                int start = TownBuilder.StableHash(b.Id) % t.Kits.Count;
                BuildingKit chosen = null;
                for (int i = 0; i < t.Kits.Count && chosen == null; i++)
                {
                    var k = cat.Kit(t.Kits[(start + i) % t.Kits.Count]);
                    if (k == null) continue;
                    bool clash = false;
                    foreach (var (ob, ok) in done)
                        if (OutdoorBuilder.Dist(ob.Rect, new GridPos(b.X0, b.Y0)) <= NeighbourReach + Math.Max(b.W, b.H) && Same(ok, k)) clash = true;
                    if (!clash) chosen = k;
                }
                chosen = chosen ?? cat.Kit(t.Kits[start]);
                if (chosen == null) continue;
                result[b.Id] = chosen;
                done.Add((b, chosen));
            }
            return result;
        }

        /// <summary>İki kit dışarıdan aynı mı görünür (duvar malzemesi ve çatı aynı).</summary>
        public static bool Same(BuildingKit a, BuildingKit b) => a != null && b != null && a.Wall == b.Wall && a.Roof == b.Roof;

        /// <summary>Cephe ve iç malzemeler (yeni oyunda, katlar kurulduktan sonra).</summary>
        public static void ApplyTerrain(GridMap map, LayoutCatalog cat)
        {
            foreach (var kv in Assign(cat))
            {
                var b = Regions.TownBuildingById(kv.Key);
                var k = kv.Value;
                if (b == null) continue;
                int top = 0;
                for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                {
                    bool any = false;
                    for (int y = b.Y0; y <= b.Y1 && !any; y++) for (int x = b.X0; x <= b.X1 && !any; x++) any = map.HasTile(new GridPos(x, y, z));
                    if (!any) continue;
                    if (z > top) top = z;
                    Perimeter(map, b, z, k);
                    string floor = z == 0 ? k.Floor : z > 0 ? k.UpperFloor ?? k.Floor : null;
                    if (floor == null) continue;
                    for (int y = b.Y0; y <= b.Y1; y++)
                        for (int x = b.X0; x <= b.X1; x++)
                        {
                            var p = new GridPos(x, y, z);
                            if (!map.HasTile(p)) continue;
                            var cur = map.GetFloor(p);
                            if (cur == FloorType.Water || cur == FloorType.Grass) continue;
                            map.SetFloor(p, cur, floor);
                        }
                }
                if (k.Roof != null)
                    for (int y = b.Y0; y <= b.Y1; y++)
                        for (int x = b.X0; x <= b.X1; x++)
                        {
                            var p = new GridPos(x, y, top);
                            if (map.InBounds(p)) map.SetRoofStyle(p, k.Roof);
                        }
                if (b.HasAnnex) ApplyAnnex(map, b, k);
            }
        }

        /// <summary>Ek yapı ana binanın kitini alır: duvar, pencere, iç kaplama, zemin ve çatı (tek kat).</summary>
        private static void ApplyAnnex(GridMap map, TownBuilding b, BuildingKit k)
        {
            var r = b.AnnexRect;
            var edges = new List<Edge>();
            for (int x = r.X; x < r.X + r.W; x++) { edges.Add(new Edge(x, r.Y, EdgeAxis.Horizontal)); edges.Add(new Edge(x, r.Y + r.H, EdgeAxis.Horizontal)); }
            for (int y = r.Y; y < r.Y + r.H; y++) { edges.Add(new Edge(r.X, y, EdgeAxis.Vertical)); edges.Add(new Edge(r.X + r.W, y, EdgeAxis.Vertical)); }
            StyleEdges(map, edges, r, k);
            for (int y = r.Y; y < r.Y + r.H; y++)
                for (int x = r.X; x < r.X + r.W; x++)
                {
                    var p = new GridPos(x, y);
                    if (!map.HasTile(p)) continue;
                    var cur = map.GetFloor(p);
                    if (k.Floor != null && cur != FloorType.Water && cur != FloorType.Grass) map.SetFloor(p, cur, k.Floor);
                    if (k.Roof != null) map.SetRoofStyle(p, k.Roof);
                }
        }

        private static void Perimeter(GridMap map, TownBuilding b, int z, BuildingKit k)
        {
            var rect = b.Rect;
            var edges = new List<Edge>();
            for (int x = b.X0; x <= b.X1; x++) { edges.Add(new Edge(x, b.Y0, EdgeAxis.Horizontal, z)); edges.Add(new Edge(x, b.Y1 + 1, EdgeAxis.Horizontal, z)); }
            for (int y = b.Y0; y <= b.Y1; y++) { edges.Add(new Edge(b.X0, y, EdgeAxis.Vertical, z)); edges.Add(new Edge(b.X1 + 1, y, EdgeAxis.Vertical, z)); }
            StyleEdges(map, edges, rect, k, z);
        }

        private static void StyleEdges(GridMap map, List<Edge> edges, RectI rect, BuildingKit k, int z = 0)
        {
            foreach (var e in edges)
            {
                if (!map.EdgeInBounds(e)) continue;
                var kind = map.GetEdge(e);
                string style = kind == EdgeKind.Wall || kind == EdgeKind.HalfWall ? k.Wall : kind == EdgeKind.Door ? k.Door : kind == EdgeKind.Window ? k.Window : null;
                if (style == null) continue;
                var look = map.GetLook(e)?.Clone() ?? new EdgeLook();
                look.Style = style;
                // İç yüz kaplaması (bodrumda yok): yüz, binanın içindeki karoya bakar.
                if (k.Finish != null && z >= 0 && kind == EdgeKind.Wall)
                {
                    bool insideB = rect.Contains(new GridPos(e.SideB.X, e.SideB.Y));
                    if (insideB) look.FinishB = k.Finish; else look.FinishA = k.Finish;
                }
                map.SetEdgeFull(e, kind, look);
            }
        }

        /// <summary>
        /// Eşya kaplamaları: binanın içindeki (her kat), kaplama grubu olan ve henüz kaplaması seçilmemiş eşyalar kitin
        /// kaplamasını alır. Dönen değer boyanan eşya sayısı.
        /// </summary>
        public static int ApplyFinishes(WorldState world, LayoutCatalog cat, ContentDatabase content)
        {
            int n = 0;
            var kits = Assign(cat);
            if (kits.Count == 0) return 0;
            foreach (var it in world.Items)
            {
                if (string.IsNullOrEmpty(it.Def.FinishGroup) || it.FinishId != null) continue;
                var b = BuildingAt(it.Position);
                if (b == null || !kits.TryGetValue(b.Id, out var k)) continue;
                var f = k.FinishFor(it.Def.FinishGroup);
                if (f == null || content == null || !content.Parts.TryGet(f, out var part) || part.Kind != BuildPartKind.ItemFinish) continue;
                it.FinishId = f;
                n++;
            }
            return n;
        }

        public static TownBuilding BuildingAt(GridPos p)
        {
            foreach (var b in Regions.TownBuildings) if (b.Covers(p)) return b;
            return null;
        }
    }
}
