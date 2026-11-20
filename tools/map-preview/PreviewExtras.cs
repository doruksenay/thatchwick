using System;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

/// <summary>Zemin kat karosu ve eşya olmayan dekor: oyunun WorldView kuralları (Core.World.GroundLook ve Scenery üzerinden).</summary>
internal static class PreviewExtras
{
    private static int Hash(int x, int y) { int h = (x * 73856093) ^ (y * 19349663); return h < 0 ? -h : h; }

    public static Func<GridPos, bool> TerrainBlocked = _ => false;

    public static void Ground(GridMap map, GridPos p, ContentDatabase db, Func<string, string> has, out string sprite, out bool flip, out float[] tint, out int[] color)
    {
        sprite = null; flip = false; tint = null; color = null;
        int x = p.X, y = p.Y; bool even = ((x + y) & 1) == 0;
        var floor = map.GetFloor(p);
        bool sea = Regions.IsSea(map, p);
        if (TerrainBlocked(p)) { color = new[] { 58, 74, 92, 255 }; return; }
        if (floor == FloorType.Water)
        {
            sprite = sea ? (has(Hash(x, y) % 2 == 0 ? "iso_ground_sea_a" : "iso_ground_sea_b") ?? has("iso_ground_sea_a")) : null;
            if (sprite == null)
            {
                sprite = has(Hash(x, y) % 4 == 0 ? "iso_ground_water_b" : "iso_ground_water_a") ?? has("iso_ground_water_a");
                if (sea && sprite != null) tint = new[] { 0.78f, 0.9f, 1f };
            }
            if (sprite != null && !sea)
            {
                bool landX = Land(map, x + 1, y), landY = Land(map, x, y + 1);
                var shore = landX || landY ? has("iso_ground_water_shore") : null;
                if (shore != null) { sprite = shore; flip = !landX; }
            }
            if (sprite == null) color = even ? new[] { 64, 120, 176, 255 } : new[] { 58, 112, 168, 255 };
        }
        else if (sea && floor == FloorType.Dirt && map.GetFloorStyle(p) == null)
        {
            sprite = has(Hash(x, y) % 2 == 0 ? "iso_ground_beach_a" : "iso_ground_beach_b") ?? has("iso_ground_beach_a");
            if (sprite == null) { sprite = has(Hash(x, y) % 2 == 0 ? "iso_ground_dirt_a" : "iso_ground_dirt_b"); if (sprite != null) tint = new[] { 1.0f, 0.93f, 0.72f }; }
        }
        else
        {
            var style = map.GetFloorStyle(p);
            if (style != null) sprite = GroundLookup.FloorPart(style, x, y, has);
            else if (floor == FloorType.Grass) sprite = has(new[] { "iso_ground_grass_a", "iso_ground_grass_b", "iso_ground_grass_c" }[Hash(x, y) % 3]);
            else if (floor == FloorType.Dirt) sprite = has(new[] { "iso_ground_dirt_a", "iso_ground_dirt_b" }[Hash(x, y) % 2]);
            else if (floor == FloorType.Sand) { sprite = has(new[] { "iso_ground_sand_a", "iso_ground_sand_b", "iso_ground_sand_c" }[Hash(x, y) % 3]) ?? has("iso_ground_beach_a"); if (sprite == null) color = even ? new[] { 222, 200, 150, 255 } : new[] { 214, 192, 142, 255 }; }
            else sprite = GroundLookup.FloorPart(BuildParts.DefaultFloor(db, floor)?.Id, x, y, has);
            if (sprite == null)
                color = floor == FloorType.Wood ? (even ? new[] { 168, 124, 76, 255 } : new[] { 158, 115, 68, 255 })
                    : floor == FloorType.Stone ? (even ? new[] { 140, 140, 148, 255 } : new[] { 130, 130, 138, 255 })
                    : floor == FloorType.Dirt ? (even ? new[] { 139, 110, 80, 255 } : new[] { 131, 103, 74, 255 }) : (even ? new[] { 92, 140, 77, 255 } : new[] { 86, 133, 72, 255 });
        }
        var road = map.GetRoad(p);
        if (road != RoadType.None)
        {
            bool natural = map.IsNaturalRoad(p); float cond = map.GetRoadCondition(p);
            bool wang = has("iso_path_dirt_wang_03_a") != null && road == RoadType.Path && !(natural && cond < 0.5f);
            if (wang) { sprite = has(new[] { "iso_ground_grass_a", "iso_ground_grass_b", "iso_ground_grass_c" }[Hash(x, y) % 3]); tint = null; color = null; }
            else if (natural && cond < 0.5f) { float t = 0.25f + 0.5f * cond; tint = new[] { 1f + (0.86f - 1f) * t, 1f + (0.74f - 1f) * t, 1f + (0.58f - 1f) * t }; }
            else if (road == RoadType.Path) sprite = has(Hash(x, y) % 2 == 0 ? "iso_ground_dirt_a" : "iso_ground_dirt_b");
            else if (road == RoadType.Gravel) sprite = has("iso_road_gravel_a_v2") ?? has("iso_road_gravel_a");
            else sprite = has("iso_road_paved_a") ?? has("iso_road_paved_worn");
        }
        if (map.GetZone(p) == ZoneType.Pasture) tint = Mul(tint, 0.86f, 0.96f, 0.84f);
    }

    private static float[] Mul(float[] t, float r, float g, float b) => t == null ? new[] { r, g, b } : new[] { t[0] * r, t[1] * g, t[2] * b };

    private static bool Land(GridMap map, int x, int y) => x >= 0 && y >= 0 && x < map.Width && y < map.Height && map.GetFloor(new GridPos(x, y, 0)) != FloorType.Water;

    public delegate void Emit(string sprite, float gx, float gy, float px, float py, float scale, bool flip, int bias);

    public static void Add(WorldState world, Region r, Func<string, string> has, Emit emit) { }
}
