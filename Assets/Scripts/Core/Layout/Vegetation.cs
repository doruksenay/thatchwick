using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Doğal bitki örtüsü (belge 44 D): rastgele ama kurallı, sabit tohumla. Seyrek Poisson-disk benzeri dağıtım (türe göre
    /// en az uzaklık), kümelenme (ağaç toplulukları, çalı ve çiçek öbekleri), farklı boylar. Yoğunluk haritası: bölge kenarında ve
    /// su boyunda yoğun, meydanda ve ana yolda seyrek. Tür yere göre: su kıyısında söğüt ve saz, kenarda meşe, huş ve çam,
    /// duvar diplerinde çalı ve çiçek, yol kenarında çim öbeği. Yasak: yol, patika, kapı önü, meydan, köprü, binaya 1 karodan
    /// yakın (ağaç), kameraya bakan cephenin önü (uzun ağaç), ağıl, çitli tarh.
    /// </summary>
    internal static class Vegetation
    {
        private const string Oak = "item.tree_oak", Birch = "item.tree_birch", Pine = "item.tree_pine", Willow = "item.tree_willow",
            Shrub = "item.shrub", Flower = "item.flower_clump", Grass = "item.grass_tuft", Reeds = "item.reeds";

        private static int MinGap(string id) => id.StartsWith("item.tree_") ? 3 : id == Shrub || id == Reeds ? 2 : 1;

        public static void Scatter(OutdoorBuilder.Context ctx, Region region, int seed)
        {
            var map = ctx.Map;
            var rng = new LayoutRng(seed);
            var placed = new List<ItemInstance>();
            var plants = new List<(GridPos p, string id)>();
            foreach (var it in ctx.World.Items)
                if (it.Def.IsPlant && region.Contains(it.Position)) plants.Add((it.Position, it.Def.Id));

            // Seyrek aday ızgarası: 2x2 hücrede bir aday, karışık sırayla.
            var cands = new List<GridPos>();
            for (int y = region.Y; y < region.Y + region.H; y += 2)
                for (int x = region.X; x < region.X + region.W; x += 2)
                    cands.Add(new GridPos(x + rng.Next(2), y + rng.Next(2)));
            cands = cands.OrderBy(_ => rng.NextDouble()).ToList();

            foreach (var p in cands)
            {
                if (!Allowed(ctx, region, p)) continue;
                var env = Env(ctx, region, p);
                if (rng.NextDouble() > env.density) continue;
                var id = Pick(ctx, env, rng);
                if (id == null || !Fits(ctx, plants, p, id)) continue;
                if (!Plant(ctx, id, p, rng, placed, plants)) continue;
                // Küme: ağaç topluluğu ya da çalı/çiçek öbeği.
                bool tree = id.StartsWith("item.tree_");
                if (rng.NextDouble() < (tree ? 0.45 : 0.6))
                {
                    int extra = 1 + rng.Next(tree ? 2 : 3);
                    for (int k = 0; k < extra; k++)
                    {
                        var q = new GridPos(p.X + rng.Next(5) - 2, p.Y + rng.Next(5) - 2);
                        if (!Allowed(ctx, region, q)) continue;
                        var qid = tree ? (rng.NextDouble() < 0.7 ? id : Pick(ctx, Env(ctx, region, q), rng)) : (rng.NextDouble() < 0.5 ? Flower : Shrub);
                        if (qid == null || !Fits(ctx, plants, q, qid, cluster: true)) continue;
                        Plant(ctx, qid, q, rng, placed, plants);
                    }
                }
            }
            var rep = new FurnishReport { Room = $"{region.Name} (bitki örtüsü)" };
            OutdoorBuilder.Validate(ctx, placed, rep);
            rep.Placed.AddRange(placed.Where(i => ctx.World.GetItem(i.Id) != null));
            ctx.Reports.Add(rep);
        }

        private struct Envir { public double density; public int water, edge, road, building, plaza; public bool wallBase; }

        private static bool Allowed(OutdoorBuilder.Context ctx, Region region, GridPos p)
        {
            if (!region.Contains(p) || !OutdoorBuilder.Open(ctx, p)) return false;
            var map = ctx.Map;
            if (map.GetZone(p) == ZoneType.Pasture || map.GetFloor(p) == FloorType.Dirt) return false;   // ağıl, tarh
            if (Regions.PlazaRect.Contains(p)) return false;
            if (OutdoorBuilder.Reserved(region, p)) return false;           // taslağın kenar bandı, kaya sırtı ve boş kalacak alanı
            return true;
        }

        private static Envir Env(OutdoorBuilder.Context ctx, Region region, GridPos p)
        {
            var map = ctx.Map;
            var e = new Envir { water = 9, road = 9, plaza = 99, building = 99 };
            for (int dy = -4; dy <= 4; dy++)
                for (int dx = -4; dx <= 4; dx++)
                {
                    var q = new GridPos(p.X + dx, p.Y + dy);
                    if (!map.InBounds(q)) continue;
                    int d = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    if (map.IsWater(q)) e.water = Math.Min(e.water, d);
                    if (map.GetRoad(q) != RoadType.None) e.road = Math.Min(e.road, d);
                }
            e.edge = Math.Min(Math.Min(p.X - region.X, region.X + region.W - 1 - p.X), Math.Min(p.Y - region.Y, region.Y + region.H - 1 - p.Y));
            var pc = new GridPos(Regions.Town.X + Regions.PlazaCX, Regions.Town.Y + Regions.PlazaCY);
            e.plaza = region == Regions.Town ? Math.Abs(p.X - pc.X) + Math.Abs(p.Y - pc.Y) : 99;
            foreach (var b in Regions.TownBuildings)
            {
                int d = OutdoorBuilder.Dist(b.Rect, p);
                if (d < e.building) e.building = d;
                if (d == 1) e.wallBase = true;
            }
            double den = 0.16;
            if (e.edge <= 5) den += 0.45;
            if (e.water <= 2) den += 0.35;
            if (e.plaza <= 9) den -= 0.2;
            if (e.road <= 1) den -= 0.08;
            if (e.wallBase) den += 0.1;
            e.density = Math.Max(0.03, Math.Min(0.9, den));
            return e;
        }

        private static string Pick(OutdoorBuilder.Context ctx, Envir e, LayoutRng rng)
        {
            double r = rng.NextDouble();
            string id;
            if (e.water == 1) id = r < 0.6 ? Reeds : Willow;
            else if (e.water <= 3) id = r < 0.6 ? Willow : r < 0.8 ? Birch : Shrub;
            else if (e.wallBase) id = r < 0.45 ? Shrub : r < 0.8 ? Flower : Grass;
            else if (e.edge <= 5) id = r < 0.45 ? Oak : r < 0.75 ? Birch : r < 0.85 ? Pine : Shrub;
            else if (e.road <= 1) id = r < 0.7 ? Grass : Flower;
            else id = r < 0.25 ? Oak : r < 0.45 ? Birch : r < 0.7 ? Shrub : r < 0.85 ? Flower : Grass;
            if (!ctx.Content.Items.TryGet(id, out var def)) return null;
            if (def.HasTag("veg") && def.Art == null) return null;           // görseli gelmemiş bitki örtüsü konmaz
            return id;
        }

        /// <summary>Türe göre en az uzaklık ve ağaç kuralları (bina boşluğu, cephe, kapı önü).</summary>
        private static bool Fits(OutdoorBuilder.Context ctx, List<(GridPos p, string id)> plants, GridPos p, string id, bool cluster = false)
        {
            int gap = MinGap(id) - (cluster ? 1 : 0);
            foreach (var (q, qid) in plants)
            {
                int g = Math.Max(gap, MinGap(qid) - (cluster ? 1 : 0));
                if (Math.Max(Math.Abs(q.X - p.X), Math.Abs(q.Y - p.Y)) < Math.Max(1, g)) return false;
            }
            if (id.StartsWith("item.tree_"))
            {
                foreach (var s in ctx.Plan.Sites)
                {
                    if (s.Building.W == 0) continue;
                    if (OutdoorBuilder.Dist(s.Building, p) < 2 || OutdoorRules.InFacadeBand(s, p)) return false;
                }
                if (ctx.Map.GetRoad(p) != RoadType.None) return false;
            }
            return true;
        }

        private static bool Plant(OutdoorBuilder.Context ctx, string id, GridPos p, LayoutRng rng, List<ItemInstance> placed, List<(GridPos, string)> plants)
        {
            if (!ctx.Content.Items.TryGet(id, out var def)) return false;
            bool ok = def.IsRug ? ctx.World.CanPlaceRug(def, p, 0).Ok : ctx.World.CanPlaceFloor(def, p, 0).Ok;
            if (!ok) return false;
            var it = ctx.World.PlaceItem(def, p, 0);
            // Farklı boylar: ağaçların çoğu olgun, bir kısmı genç.
            if (def.IsPlant) it.GrowthDays = def.GrowDays * (rng.NextDouble() < 0.7 ? 1.0 : 0.45 + 0.4 * rng.NextDouble());
            placed.Add(it);
            plants.Add((p, id));
            return true;
        }
    }
}
