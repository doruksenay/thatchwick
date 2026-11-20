using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Şehir binalarının katları ve iç döşemesi (belge 44 C, Ek tablosu). Plan (bina dikdörtgeni, kapı) Regions.TownBuildings'te,
    /// katlar ve odalar layouts.json'daki bina şablonunda. Ortaçağ düzeni: altta dükkân, üstte ev (handa kiralık odalar),
    /// altta kiler. Merdiven ve iniş yerleri plandan sabit hesaplanır: arazi geçişi (BuildFloors) ile eşya geçişi (Furnish)
    /// aynı yeri bulur.
    /// </summary>
    public static class TownBuilder
    {
        public struct StairsSpot
        {
            public GridPos Pos; public int Rot; public GridPos Entrance, Landing;
            public IEnumerable<GridPos> Foot() { yield return Pos; yield return Rot == 0 ? new GridPos(Pos.X, Pos.Y + 1, Pos.Z) : new GridPos(Pos.X + 1, Pos.Y, Pos.Z); }
        }

        private static StairsSpot Spot(int x, int y, int rot, int delta)
        {
            var s = new StairsSpot { Pos = new GridPos(x, y), Rot = rot };
            if (rot == 0) { s.Entrance = new GridPos(x, y - 1); s.Landing = new GridPos(x, y + 2, delta); }
            else { s.Entrance = new GridPos(x - 1, y); s.Landing = new GridPos(x + 2, y, delta); }
            return s;
        }

        /// <summary>Kapının iç karosu ve bir içerisi: merdiven ve eşya konmaz.</summary>
        private static HashSet<GridPos> DoorKeep(TownBuilding b)
        {
            var d = Facing.FromChar(b.Door);
            var inside = b.DoorInside;
            return new HashSet<GridPos> { inside, Facing.Step(inside, Facing.Opposite(d)) };
        }

        /// <summary>
        /// Merdiven yerleri: yan duvara dayalı, girişi ve inişi bina içinde, kapıdan uzak. Handa üst merdivenin inişi orta sırada
        /// (koridor). Bodrum merdiveni üst merdivenle çakışmaz.
        /// </summary>
        public static void PlanStairs(TownBuilding b, bool up, bool down, bool inn, out StairsSpot? upS, out StairsSpot? downS)
        {
            upS = null; downS = null;
            var keep = DoorKeep(b);
            var door = b.DoorInside;
            var cands = new List<(StairsSpot s, double score)>();
            for (int y = b.Y0; y <= b.Y1; y++)
                for (int x = b.X0; x <= b.X1; x++)
                    for (int rot = 0; rot <= 1; rot++)
                    {
                        if (rot == 0 && x != b.X0 && x != b.X1) continue;
                        if (rot == 1 && y != b.Y0 && y != b.Y1) continue;
                        var s = Spot(x, y, rot, 0);
                        var cells = s.Foot().Concat(new[] { s.Entrance, new GridPos(s.Landing.X, s.Landing.Y) }).ToList();
                        if (cells.Any(c => !b.Rect.Contains(c) || keep.Contains(c))) continue;
                        double score = Math.Abs(x - door.X) + Math.Abs(y - door.Y);
                        // Ön duvarlar (batı, kuzey): arka duvarlar kameraya dönük dolap ve tezgâhlara kalsın (merdiven kesitte görünür).
                        if ((rot == 0 && x == b.X0) || (rot == 1 && y == b.Y0)) score += 6;
                        cands.Add((s, score));
                    }
            cands = cands.OrderByDescending(c => c.score).ThenBy(c => c.s.Pos.Y).ThenBy(c => c.s.Pos.X).ThenBy(c => c.s.Rot).ToList();
            HashSet<GridPos> used = new HashSet<GridPos>();
            if (up)
            {
                foreach (var (s, _) in cands)
                {
                    if (inn && (s.Rot != 0 || s.Landing.Y != b.Y0 + b.H / 2)) continue;
                    var u = Spot(s.Pos.X, s.Pos.Y, s.Rot, 1);
                    upS = u;
                    foreach (var c in u.Foot()) used.Add(c);
                    used.Add(u.Entrance);
                    break;
                }
            }
            if (down)
            {
                foreach (var (s, _) in cands)
                {
                    var d = Spot(s.Pos.X, s.Pos.Y, s.Rot, -1);
                    var cells = d.Foot().Concat(new[] { d.Entrance }).ToList();
                    // Üst merdivenle çakışmasın ve bitişik olmasın (ikisinin girişi ayrı kalsın).
                    if (cells.Any(c => used.Contains(c) || Facing.All.Any(dir => used.Contains(Facing.Step(c, dir))))) continue;
                    downS = d;
                    break;
                }
            }
        }

        // ---------------- arazi: katlar ----------------

        /// <summary>Bina şablonuna göre üst kat ve bodrum karoları, duvarları, merdiven boşluğu ve han odaları (yeni oyunda).</summary>
        public static void BuildFloors(GridMap map, LayoutCatalog cat)
        {
            if (cat == null) return;
            foreach (var b in Regions.TownBuildings)
            {
                var t = cat.Building(b.Template);
                if (t == null || (!t.HasUpper && !t.HasCellar)) continue;
                bool inn = t.UpperKind == "inn";
                PlanStairs(b, t.HasUpper, t.HasCellar, inn, out var up, out var down);
                if (t.HasUpper && up.HasValue) BuildLevel(map, b, 1, FloorType.Wood, up.Value);
                if (t.HasCellar && down.HasValue) BuildLevel(map, b, -1, FloorType.Stone, null);
                if (inn && up.HasValue) BuildInnRooms(map, b, up.Value);
            }
        }

        private static void BuildLevel(GridMap map, TownBuilding b, int z, FloorType floor, StairsSpot? hole)
        {
            for (int y = b.Y0; y <= b.Y1; y++)
                for (int x = b.X0; x <= b.X1; x++)
                {
                    var p = new GridPos(x, y, z);
                    map.SetTile(p, true);
                    map.SetFloor(p, floor);
                }
            // Duvarlar: üst katta zemin kattaki kapı ve pencerelerin üstü pencere, bodrumda düz duvar.
            for (int x = b.X0; x <= b.X1; x++)
            {
                map.SetEdge(new Edge(x, b.Y0, EdgeAxis.Horizontal, z), Upper(map.GetEdge(new Edge(x, b.Y0, EdgeAxis.Horizontal)), z));
                map.SetEdge(new Edge(x, b.Y1 + 1, EdgeAxis.Horizontal, z), Upper(map.GetEdge(new Edge(x, b.Y1 + 1, EdgeAxis.Horizontal)), z));
            }
            for (int y = b.Y0; y <= b.Y1; y++)
            {
                map.SetEdge(new Edge(b.X0, y, EdgeAxis.Vertical, z), Upper(map.GetEdge(new Edge(b.X0, y, EdgeAxis.Vertical)), z));
                map.SetEdge(new Edge(b.X1 + 1, y, EdgeAxis.Vertical, z), Upper(map.GetEdge(new Edge(b.X1 + 1, y, EdgeAxis.Vertical)), z));
            }
            if (hole.HasValue) foreach (var c in hole.Value.Foot()) map.SetTile(new GridPos(c.X, c.Y, z), false);
        }

        private static EdgeKind Upper(EdgeKind ground, int z) => z < 0 ? EdgeKind.Wall : ground == EdgeKind.Door || ground == EdgeKind.Window ? EdgeKind.Window : EdgeKind.Wall;

        /// <summary>Han üst katı: inişin sırası koridor, iki yanında ikişer oda (kuzeybatı, kuzeydoğu, güneybatı, güneydoğu).</summary>
        public static List<(string name, HashSet<GridPos> tiles, GridPos doorInside)> InnRooms(GridMap map, TownBuilding b, StairsSpot up)
        {
            int r = up.Landing.Y, xs = b.X0 + b.W / 2;
            var rooms = new List<(string, HashSet<GridPos>, GridPos)>();
            var parts = new[] { ("kuzeybatı", b.X0, xs - 1, b.Y0, r - 1, Dir4.S), ("kuzeydoğu", xs, b.X1, b.Y0, r - 1, Dir4.S),
                                ("güneybatı", b.X0, xs - 1, r + 1, b.Y1, Dir4.N), ("güneydoğu", xs, b.X1, r + 1, b.Y1, Dir4.N) };
            foreach (var (name, x0, x1, y0, y1, toCorridor) in parts)
            {
                var tiles = new HashSet<GridPos>();
                for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) { var p = new GridPos(x, y, 1); if (map == null || map.HasTile(p)) tiles.Add(p); }
                int row = toCorridor == Dir4.S ? y1 : y0;
                // Kapı: koridora bakan sıranın ortasına yakın, boşluk olmayan ve inişin hizasında olmayan sütun.
                var cols = Enumerable.Range(x0, x1 - x0 + 1).OrderBy(x => Math.Abs(x - (x0 + x1) / 2.0)).ThenBy(x => x);
                GridPos door = new GridPos(x0, row, 1);
                foreach (var x in cols)
                {
                    var p = new GridPos(x, row, 1);
                    if (!tiles.Contains(p) || x == up.Landing.X) continue;
                    door = p; break;
                }
                rooms.Add((name, tiles, door));
            }
            return rooms;
        }

        private static void BuildInnRooms(GridMap map, TownBuilding b, StairsSpot up)
        {
            int r = up.Landing.Y, xs = b.X0 + b.W / 2;
            var rooms = InnRooms(map, b, up);
            var doors = new HashSet<Edge>();
            foreach (var (_, _, d) in rooms) doors.Add(d.Y < r ? new Edge(d.X, r, EdgeAxis.Horizontal, 1) : new Edge(d.X, r + 1, EdgeAxis.Horizontal, 1));
            for (int x = b.X0; x <= b.X1; x++)
            {
                var n = new Edge(x, r, EdgeAxis.Horizontal, 1); var s = new Edge(x, r + 1, EdgeAxis.Horizontal, 1);
                map.SetEdge(n, doors.Contains(n) ? EdgeKind.Door : EdgeKind.Wall);
                map.SetEdge(s, doors.Contains(s) ? EdgeKind.Door : EdgeKind.Wall);
            }
            for (int y = b.Y0; y <= b.Y1; y++)
                if (y != r) map.SetEdge(new Edge(xs, y, EdgeAxis.Vertical, 1), EdgeKind.Wall);
        }

        /// <summary>Üst katta oturan sahibinin evi: tek odalı üst katta ortası, handa güneydoğu odası; üst kat yoksa null.</summary>
        public static GridPos? HomeSpot(GridMap map, TownBuilding b)
        {
            var c = new GridPos(b.X0 + b.W / 2, b.Y0 + b.H / 2, 1);
            if (!map.InBounds(c) || map.CountTiles(1) == 0) return null;
            bool any = false;
            for (int y = b.Y0; y <= b.Y1 && !any; y++) for (int x = b.X0; x <= b.X1; x++) if (map.HasTile(new GridPos(x, y, 1))) { any = true; break; }
            if (!any) return null;
            if (b.Kind == "inn")
            {
                PlanStairs(b, true, false, true, out var up, out _);
                if (up.HasValue)
                {
                    var owner = InnRooms(map, b, up.Value)[3];
                    var t = owner.tiles.OrderBy(p => Math.Abs(p.X - owner.doorInside.X) + Math.Abs(p.Y - owner.doorInside.Y)).Skip(1).FirstOrDefault();
                    return owner.tiles.Count > 0 ? t : (GridPos?)null;
                }
            }
            for (int r = 0; r < 4; r++)
                for (int dy = -r; dy <= r; dy++) for (int dx = -r; dx <= r; dx++)
                {
                    var p = new GridPos(c.X + dx, c.Y + dy, 1);
                    if (b.Rect.Contains(p) && map.HasTile(p)) return p;
                }
            return null;
        }

        // ---------------- plan ----------------

        /// <summary>Şehrin (ve çiftlik evinin) yerleşim planı: arsalar ve oda bölgeleri (doğrulayıcının oda türü kuralı için).</summary>
        public static LayoutPlan Plan(GridMap map, LayoutCatalog cat)
        {
            var plan = new LayoutPlan();
            foreach (var b in Regions.TownBuildings)
            {
                var t = cat?.Building(b.Template);
                var site = new Site
                {
                    Id = b.Id, Name = b.Name, Kind = b.Kind, Trade = b.Kind == "house" ? null : b.Trade ?? b.Id, Owner = b.Owner, Building = b.Rect,
                    Plot = Grow(b.Rect, 2), DoorDir = Facing.FromChar(b.Door), DoorInside = b.DoorInside, MinLevel = 0, MaxLevel = 0,
                };
                string groundType = cat?.Room(t?.Ground)?.Type ?? (b.Kind == "house" ? "living|bedroom|kitchen|dining" : "shop");
                site.Rooms.Add(new RoomZone { Type = groundType, Trade = site.Trade, Level = 0, Rect = b.Rect });
                if (t != null && map != null)
                {
                    bool inn = t.UpperKind == "inn";
                    PlanStairs(b, t.HasUpper, t.HasCellar, inn, out var up, out var down);
                    if (t.HasUpper && up.HasValue && map.HasTile(up.Value.Landing))
                    {
                        site.MaxLevel = 1;
                        if (inn)
                        {
                            var rooms = InnRooms(map, b, up.Value);
                            for (int i = 0; i < rooms.Count; i++)
                            {
                                var ids = t.InnRooms;
                                var rt = cat.Room(i < ids.Count ? ids[i] : null);
                                var tiles = rooms[i].tiles;
                                if (tiles.Count == 0) continue;
                                int minX = tiles.Min(p => p.X), minY = tiles.Min(p => p.Y), maxX = tiles.Max(p => p.X), maxY = tiles.Max(p => p.Y);
                                site.Rooms.Add(new RoomZone { Type = rt?.Type ?? Placements.InnRoom, Level = 1, Rect = new RectI(minX, minY, maxX - minX + 1, maxY - minY + 1) });
                            }
                        }
                        else site.Rooms.Add(new RoomZone { Type = cat.Room(t.Upper)?.Type ?? "bedroom|living", Level = 1, Rect = b.Rect });
                    }
                    if (t.HasCellar && down.HasValue && map.HasTile(down.Value.Landing))
                    {
                        site.MinLevel = -1;
                        site.Rooms.Add(new RoomZone { Type = t.CellarType, Trade = site.Trade, Level = -1, Rect = b.Rect });
                    }
                }
                plan.Sites.Add(site);
            }
            // Meydan ve hayvan pazarının ağılı (binasız arsalar)
            plan.Sites.Add(new Site { Id = "square", Name = "Köy meydanı", Kind = "plaza", Building = new RectI(0, 0, 0, 0), Plot = Regions.PlazaRect });   // loc-yok: yerleşim planı adı (geliştirici)
            plan.Sites.Add(new Site { Id = "animals", Name = "Hayvan pazarı", Kind = "pen", Trade = "animals", Building = new RectI(0, 0, 0, 0), Plot = Grow(Regions.PenRect, 1) });
            // Çiftlik evi
            var farm = new RectI(StarterWorld.HouseX0, StarterWorld.HouseY0, StarterWorld.HouseX1 - StarterWorld.HouseX0 + 1, StarterWorld.HouseY1 - StarterWorld.HouseY0 + 1);
            var fh = new Site { Id = "farmhouse", Name = "Çiftlik evi", Kind = "house", Building = farm, Plot = Grow(farm, 3), DoorDir = Dir4.S,
                                DoorInside = new GridPos(StarterWorld.HouseDoorX, StarterWorld.HouseY1), MinLevel = -1, MaxLevel = 1 };
            var ft = cat?.Building("farmhouse");
            fh.Rooms.Add(new RoomZone { Type = cat?.Room(ft?.Ground)?.Type ?? "kitchen|dining|living", Level = 0, Rect = farm });
            fh.Rooms.Add(new RoomZone { Type = cat?.Room(ft?.Upper)?.Type ?? "bedroom|living", Level = 1, Rect = farm });
            fh.Rooms.Add(new RoomZone { Type = ft?.CellarType ?? "cellar|pantry|workshop", Level = -1, Rect = farm });
            plan.Sites.Add(fh);
            return plan;
        }

        public static RectI Grow(RectI r, int n) => new RectI(r.X - n, r.Y - n, r.W + 2 * n, r.H + 2 * n);

        // ---------------- eşyalar ----------------

        public static int StableHash(string s)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in s) { h ^= c; h *= 16777619u; }
                return (int)(h & 0x7FFFFFFF);
            }
        }

        /// <summary>Şehir binalarının merdivenlerini koyar ve bütün odaları şablonlardan döşer. Raporları döndürür.</summary>
        public static List<FurnishReport> Furnish(WorldState world, ContentDatabase content, LayoutCatalog cat, LayoutPlan plan, int seed, Action<string> log = null)
        {
            var reports = new List<FurnishReport>();
            if (cat == null) return reports;
            var map = world.Map;
            var furnisher = new RoomFurnisher(world, content, plan);
            foreach (var b in Regions.TownBuildings)
            {
                var t = cat.Building(b.Template);
                if (t == null) { log?.Invoke($"Bina şablonu yok: {b.Template}"); continue; }
                bool inn = t.UpperKind == "inn";
                PlanStairs(b, t.HasUpper, t.HasCellar, inn, out var up, out var down);
                bool hasUp = t.HasUpper && up.HasValue && map.HasTile(up.Value.Landing);
                bool hasDown = t.HasCellar && down.HasValue && map.HasTile(down.Value.Landing);
                if (hasUp) PutStairs(world, content, "item.stairs_up", up.Value, log);
                if (hasDown) PutStairs(world, content, "item.stairs_down", down.Value, log);

                var site = plan?.Get(b.Id);
                // Zemin kat
                var ground = new RoomSpec { Name = $"{b.Name} (zemin)", Level = 0, DoorDir = Facing.FromChar(b.Door), Zone = site?.Rooms.FirstOrDefault(z => z.Level == 0) };
                for (int y = b.Y0; y <= b.Y1; y++) for (int x = b.X0; x <= b.X1; x++) ground.Tiles.Add(new GridPos(x, y));
                ground.Entrances.Add(b.DoorInside);
                foreach (var k in DoorKeep(b)) ground.Keep.Add(k);
                // Merdiven girişleri boş ve kapıdan ulaşılabilir kalır (üst kata ve bodruma yol kopmasın).
                if (hasUp) { ground.Keep.Add(up.Value.Entrance); ground.Entrances.Add(up.Value.Entrance); }
                if (hasDown) { ground.Keep.Add(down.Value.Entrance); ground.Entrances.Add(down.Value.Entrance); }
                Run(furnisher, cat, t.Ground, ground, seed, b.Id + ":0", reports, log);

                if (hasUp)
                {
                    if (inn)
                    {
                        var rooms = InnRooms(map, b, up.Value);
                        for (int i = 0; i < rooms.Count; i++)
                        {
                            var (name, tiles, door) = rooms[i];
                            var spec = new RoomSpec { Name = $"{b.Name} (üst kat, {name})", Level = 1, Zone = site?.ZoneAt(door) };
                            foreach (var p in tiles) spec.Tiles.Add(p);
                            spec.Entrances.Add(door);
                            spec.Keep.Add(door);
                            Run(furnisher, cat, i < t.InnRooms.Count ? t.InnRooms[i] : null, spec, seed, $"{b.Id}:1:{i}", reports, log);
                        }
                    }
                    else
                    {
                        var spec = new RoomSpec { Name = $"{b.Name} (üst kat)", Level = 1, Zone = site?.Rooms.FirstOrDefault(z => z.Level == 1) };
                        for (int y = b.Y0; y <= b.Y1; y++) for (int x = b.X0; x <= b.X1; x++) { var p = new GridPos(x, y, 1); if (map.HasTile(p)) spec.Tiles.Add(p); }
                        spec.Entrances.Add(up.Value.Landing);
                        spec.Keep.Add(up.Value.Landing);
                        Run(furnisher, cat, t.Upper, spec, seed, b.Id + ":1", reports, log);
                    }
                }
                if (hasDown)
                {
                    var spec = new RoomSpec { Name = $"{b.Name} (bodrum)", Level = -1, Zone = site?.Rooms.FirstOrDefault(z => z.Level == -1) };
                    for (int y = b.Y0; y <= b.Y1; y++) for (int x = b.X0; x <= b.X1; x++) spec.Tiles.Add(new GridPos(x, y, -1));
                    spec.Entrances.Add(down.Value.Landing);
                    spec.Keep.Add(down.Value.Landing);
                    Run(furnisher, cat, t.Cellar, spec, seed, b.Id + ":-1", reports, log);
                }
            }
            return reports;
        }

        internal static void Run(RoomFurnisher f, LayoutCatalog cat, string templateId, RoomSpec spec, int seed, string key, List<FurnishReport> reports, Action<string> log)
        {
            var rt = cat.Room(templateId);
            if (rt == null) return;
            var rep = f.Furnish(spec, rt, Seeds.Derive(seed, StableHash(key)));
            reports.Add(rep);
            foreach (var m in rep.Missing) log?.Invoke($"{spec.Name}: zorunlu eşya konamadı {m}");
            foreach (var v in rep.Violations) log?.Invoke($"{spec.Name}: {v}");
        }

        private static void PutStairs(WorldState world, ContentDatabase content, string id, StairsSpot s, Action<string> log)
        {
            if (!content.Items.TryGet(id, out var def)) { log?.Invoke($"Eşya tanımı yok: {id}"); return; }
            try { world.PlaceItem(def, s.Pos, s.Rot); }
            catch (InvalidOperationException ex) { log?.Invoke($"Merdiven konamadı {id} {s.Pos}: {ex.Message}"); }
        }
    }
}
