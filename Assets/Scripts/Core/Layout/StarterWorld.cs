using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Yeni oyunun dünyası: harita (çiftlik, köy yolu, şehir, yaban haritaları) ve başlangıç eşyaları.
    /// Önceden GameRoot'taydı (BuildDemoMap, PlaceDemoItems); yerleşim doğrulayıcısı ve testler Unity'siz aynı dünyayı
    /// kurabilsin diye Core'a taşındı (yerleşim sistemi, Aşama A). Davranış aynı kaldı; döşeme kuralları Aşama C-D'de
    /// buradan şablonlara bağlanır.
    /// </summary>
    public static class StarterWorld
    {
        public const int MapWidth = Regions.WorldWidth, MapHeight = Regions.WorldHeight;

        /// <summary>Başlangıç evi (iki katlı, iç 28-36 x 26-32): kapı güney duvarında x=32.</summary>
        public const int HouseX0 = 28, HouseX1 = 36, HouseY0 = 26, HouseY1 = 32, HouseDoorX = 32;

        public static GridMap BuildMap(LayoutCatalog layouts = null)
        {
            var map = new GridMap(MapWidth, MapHeight);
            // Test haritası: kaynaklı doğal gölet (hep dolu; böcekli, içinde sazan, kerevit ve alabalık).
            for (int x = 8; x < 20; x++)
                for (int y = 44; y < 52; y++)
                    map.SetFloor(new GridPos(x, y), FloorType.Water);
            var natural = new GridPos(8, 44);
            map.SetSpring(natural);
            map.RestorePond(natural, 1f, spring: true, insects: true, filledDays: 30, new[]
            {
                new KeyValuePair<string, int>("item.fish_carp", 8),
                new KeyValuePair<string, int>("item.fish_crayfish", 10),
                new KeyValuePair<string, int>("item.fish_trout", 6),
                new KeyValuePair<string, int>("item.fish_pike", 2)
            });

            BuildStarterHouse(map);
            // Çiftlik yolu doğu kenarına, şehir geçidine kadar uzar; şehir haritası kurulur (belge 39).
            for (int x = 73; x <= 95; x++) map.SetRoad(new GridPos(x, 40), RoadType.Path);
            map.SetRoad(new GridPos(94, 41), RoadType.Path); map.SetRoad(new GridPos(95, 41), RoadType.Path);
            Regions.BuildTerrain(map);
            TownBuilder.BuildFloors(map, layouts);   // dükkânların üst katı, bodrumu, han odaları (belge 44 C)
            Regions.Naturalize(map);
            Regions.Apply(map);
            return map;
        }

        // Başlangıç evi: iki katlı 9x7 ev (iç 28-36 x 26-32), ahşap zemin; altında taş döşeli bodrum.
        // Güneyde kapı (32), her katta pencereler; çatı en üst katın duvarlarından otomatik. Merdiven boşluğu üst katta açık.
        private static void BuildStarterHouse(GridMap map)
        {
            const int x0 = HouseX0, x1 = HouseX1, y0 = HouseY0, y1 = HouseY1;
            for (int z = -1; z <= 1; z++)
            {
                for (int x = x0; x <= x1; x++)
                    for (int y = y0; y <= y1; y++)
                    {
                        var p = new GridPos(x, y, z);
                        if (z != 0) map.SetTile(p, true);
                        map.SetFloor(p, z < 0 ? FloorType.Stone : FloorType.Wood);
                    }
                bool cellar = z < 0;
                for (int x = x0; x <= x1; x++)
                {
                    bool win = !cellar && (x == 30 || x == 34);
                    map.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal, z), win ? EdgeKind.Window : EdgeKind.Wall);
                    var south = z == 0 && x == HouseDoorX ? EdgeKind.Door : win ? EdgeKind.Window : EdgeKind.Wall;
                    map.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), south);
                }
                for (int y = y0; y <= y1; y++)
                {
                    bool win = !cellar && y == 29;
                    map.SetEdge(new Edge(x0, y, EdgeAxis.Vertical, z), win ? EdgeKind.Window : EdgeKind.Wall);
                    map.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical, z), win ? EdgeKind.Window : EdgeKind.Wall);
                }
            }
            // Merdiven boşluğu: üst kata çıkan merdivenin (35,27-28) üstü açık.
            map.SetTile(new GridPos(35, 27, 1), false);
            map.SetTile(new GridPos(35, 28, 1), false);

            // Kapıdan güneye patika, sonra doğuya köye giden yol.
            for (int y = 33; y <= 40; y++) map.SetRoad(new GridPos(HouseDoorX, y), RoadType.Path);
            for (int x = 33; x <= 72; x++) map.SetRoad(new GridPos(x, 40), RoadType.Path);
        }

        /// <summary>Başlangıç eşyalarını koyar. Dönen değer pazar tezgâhının kimliği (yoksa 0).</summary>
        public static int PlaceItems(WorldState world, ContentDatabase content, Action<string> log = null) =>
            Place(world, content, null, 1, log).StallId;

        /// <summary>Yeni oyunun eşyaları ve şablon raporları.</summary>
        public sealed class Result
        {
            public int StallId;
            public LayoutPlan Plan;
            public readonly List<FurnishReport> Reports = new List<FurnishReport>();
        }

        /// <summary>
        /// Başlangıç eşyaları: evlerin ve dükkânların içi oda şablonlarından (layouts.json, sabit tohumla), dış mekân ve
        /// bitki örtüsü kurallarla. layouts null ise iç mekânlar boş kalır (yalnızca yapısal merdivenler).
        /// </summary>
        public static Result Place(WorldState world, ContentDatabase content, LayoutCatalog layouts, int seed, Action<string> log = null)
        {
            var res = new Result { Plan = TownBuilder.Plan(world.Map, layouts) };
            var s = new Seeder(world, content, log);
            s.PlaceFarm(layouts, res, seed);
            var wild = s.PlaceRegions();
            DropInvalid(world, content, res.Plan, wild, log);   // yaban haritalarının elle konmuş eşyaları da aynı kurallardan geçer
            res.Reports.AddRange(TownBuilder.Furnish(world, content, layouts, res.Plan, seed, log));
            res.Reports.AddRange(OutdoorBuilder.Place(world, content, layouts, res.Plan, seed, log));   // dış mekân, bahçeler, bitki örtüsü (D)
            s.PlaceWildOaks();
            foreach (var it in world.Items) if (it.Def.Id == "item.market_stall") { res.StallId = it.Id; break; }
            return res;
        }

        private static void DropInvalid(WorldState world, ContentDatabase content, LayoutPlan plan, List<ItemInstance> items, Action<string> log)
        {
            if (items.Count == 0) return;
            var tiles = new HashSet<GridPos>();
            foreach (var it in items) foreach (var t in it.OccupiedTiles()) tiles.Add(t);
            var v = new PlacementValidator(world, plan, tiles.Contains);
            LayoutRules.AddAll(v, content);
            var ids = new HashSet<int>(); foreach (var it in items) ids.Add(it.Id);
            foreach (var x in v.Validate())
                if (x.Severity == Severity.Error && ids.Contains(x.ItemId) && world.GetItem(x.ItemId) != null)
                {
                    world.RemoveItem(x.ItemId);
                    log?.Invoke($"Kurala uymayan eşya konmadı: {x}");
                }
        }

        /// <summary>Yerleştirme yardımcıları (konamayan eşya günlüğe yazılır, oyun durmaz).</summary>
        internal sealed class Seeder
        {
            private readonly WorldState _world;
            private readonly ContentDatabase _content;
            private readonly Action<string> _log;
            public int StallId;

            public Seeder(WorldState world, ContentDatabase content, Action<string> log)
            {
                _world = world; _content = content; _log = log ?? (_ => { });
            }

            public ItemInstance Put(string id, int x, int y, int z = 0, int rotation = 0)
            {
                if (!_content.Items.TryGet(id, out var def)) { _log($"Eşya tanımı yok: {id}"); return null; }
                try { return _world.PlaceItem(def, new GridPos(x, y, z), rotation); }
                catch (Exception ex) { _log($"Başlangıç eşyası konamadı {id} ({x},{y},{z}): {ex.Message}"); return null; }
            }

            public ItemInstance PutGrown(string id, int x, int y, bool quiet = false)
            {
                if (!_content.Items.TryGet(id, out var def)) return null;
                var check = _world.CanPlaceFloor(def, new GridPos(x, y), 0);
                if (!check.Ok) { if (!quiet) _log($"Başlangıç ağacı konamadı {id} ({x},{y}): {check.Reason}"); return null; }
                var item = _world.PlaceItem(def, new GridPos(x, y));
                PlantSystem.SetGrown(item);
                return item;
            }

            public ItemInstance PutWall(string id, GridPos tile, int side)
            {
                if (!_content.Items.TryGet(id, out var def)) { _log($"Eşya tanımı yok: {id}"); return null; }
                try { return _world.PlaceOnWall(def, tile, side); }
                catch (Exception ex) { _log($"Başlangıç duvar eşyası konamadı {id} {tile}: {ex.Message}"); return null; }
            }

            public ItemInstance PutOn(string id, int x, int y, int z, string slot)
            {
                var surface = _world.GetItemAt(new GridPos(x, y, z));
                if (surface == null || !_content.Items.TryGet(id, out var def)) return null;
                try { return _world.PlaceOnSurface(def, surface.Id, slot); }
                catch (Exception ex) { _log($"Başlangıç dekoru konamadı {id}: {ex.Message}"); return null; }
            }

            // Başlangıç evi (BuildStarterHouse): iç 28-36 x 26-32; zemin kat mutfak ve oturma, üst kat yatak odaları, bodrum kiler.
            // Oyuncu (31,31), Maud (33,31), kapı içi (32,32) ve merdiven uçları boş kalır.
            public void PlaceFarm(LayoutCatalog layouts, Result res, int seed)
            {
                // --- yapısal: merdivenler (merdiven boşluğu BuildStarterHouse'ta) ---
                Put("item.stairs_down", 29, 27);              // bodruma (giriş 29,26; iniş 29,29 kat -1)
                Put("item.stairs_up", 35, 27);                // üst kata (giriş 35,26; iniş 35,29 kat 1)
                FurnishFarmhouse(layouts, res, seed);

                // --- avlu: kuyu, kütük, yağmur fıçısı, avlu masası kurallarla (OutdoorBuilder, bahçe şablonu garden_farm) ---
                Put("item.scarecrow", 48, 32);                // tarla alanı: 42-56 x 26-38
                StartingKit.TillBed(_world, new GridPos(43, 27), ditch: _content.Items.TryGet("item.irrigation_ditch", out var ditch) ? ditch : null);

                // --- ağaçlar: meyve bahçesi (kuzeybatı), gölet söğütleri, arsa kenarı ---
                string[] fruit = { "item.tree_apple", "item.tree_pear", "item.tree_cherry", "item.tree_plum" };
                int k = 0;
                for (int y = 10; y <= 19; y += 3)
                    for (int x = 9; x <= 21; x += 4) PutGrown(fruit[k++ % fruit.Length], x, y);
                PutGrown("item.forage_berry_bush", 24, 10);
                PutGrown("item.forage_berry_bush", 26, 12);
                PutGrown("item.tree_willow", 7, 42);
                PutGrown("item.tree_willow", 21, 50);
                PutGrown("item.tree_willow", 13, 54);
                var edgeRng = new Random(64);
                for (int i = 0; i < 40; i++)
                {
                    int side = edgeRng.Next(4), t = 6 + edgeRng.Next(58);
                    int ex = side == 0 ? 5 + edgeRng.Next(2) : side == 1 ? 65 + edgeRng.Next(2) : t;
                    int ey = side == 2 ? 5 + edgeRng.Next(2) : side == 3 ? 65 + edgeRng.Next(2) : t;
                    if (_world.Map.GetRoad(new GridPos(ex, ey)) != RoadType.None) continue;
                    PutGrown(edgeRng.Next(3) == 0 ? "item.tree_pine" : "item.tree_birch", ex, ey, quiet: true);
                }
            }

            /// <summary>
            /// Çiftlik evi (belge 38: iki katlı, merdiven, bodrum) şablonlardan: zemin kat mutfak-yemek-oturma, üst kat yatak odası,
            /// bodrum kiler ve atölye. Oyuncu (31,31), Maud (33,31), kapı içi ve merdiven uçları boş kalır. Oyuncunun evi yeni oyunda
            /// bir kez döşenir, sonra tamamen oyuncunundur.
            /// </summary>
            private void FurnishFarmhouse(LayoutCatalog layouts, Result res, int seed)
            {
                var t = layouts?.Building("farmhouse");
                if (t == null) return;
                var f = new RoomFurnisher(_world, _content, res.Plan);
                var site = res.Plan.Get("farmhouse");
                var ground = new RoomSpec { Name = "Çiftlik evi (zemin)", Level = 0, DoorDir = Dir4.S, Zone = site?.ZoneAt(new GridPos(HouseX0, HouseY0, 0)) };
                var upper = new RoomSpec { Name = "Çiftlik evi (üst kat)", Level = 1, Zone = site?.ZoneAt(new GridPos(HouseX0, HouseY0, 1)) };
                var cellar = new RoomSpec { Name = "Çiftlik evi (bodrum)", Level = -1, Zone = site?.ZoneAt(new GridPos(HouseX0, HouseY0, -1)) };
                for (int y = HouseY0; y <= HouseY1; y++)
                    for (int x = HouseX0; x <= HouseX1; x++)
                    {
                        ground.Tiles.Add(new GridPos(x, y));
                        if (_world.Map.HasTile(new GridPos(x, y, 1))) upper.Tiles.Add(new GridPos(x, y, 1));
                        cellar.Tiles.Add(new GridPos(x, y, -1));
                    }
                var door = new GridPos(HouseDoorX, HouseY1);
                ground.Entrances.Add(door);
                ground.Entrances.Add(new GridPos(29, 26)); ground.Entrances.Add(new GridPos(35, 26));   // merdiven girişleri ulaşılabilir kalır
                foreach (var k in new[] { door, new GridPos(HouseDoorX, HouseY1 - 1), new GridPos(31, 31), new GridPos(33, 31), new GridPos(29, 26), new GridPos(35, 26) })
                    ground.Keep.Add(k);
                upper.Entrances.Add(new GridPos(35, 29, 1)); upper.Keep.Add(new GridPos(35, 29, 1));
                cellar.Entrances.Add(new GridPos(29, 29, -1)); cellar.Keep.Add(new GridPos(29, 29, -1));
                var reports = new List<FurnishReport>();
                TownBuilder.Run(f, layouts, t.Ground, ground, seed, "farmhouse:0", reports, _log);
                TownBuilder.Run(f, layouts, t.Upper, upper, seed, "farmhouse:1", reports, _log);
                TownBuilder.Run(f, layouts, t.Cellar, cellar, seed, "farmhouse:-1", reports, _log);
                res.Reports.AddRange(reports);
            }

            /// <summary>Köy yolu ve şehir: dükkân içleri (meslek eşyaları, duvar ve yüzey dekoru), meydan, evler, ağaçlar.</summary>
            public List<ItemInstance> PlaceRegions()
            {
                var placedAll = new List<ItemInstance>();
                foreach (var ti in Regions.Items())
                {
                    var at = new GridPos(ti.X, ti.Y);
                    if (ti.Grown)
                    {
                        // Ağaç ve çalı yalnızca açık toprağa: yol, meydan taşı, su, köprü ve bina içi atlanır.
                        if (_world.Map.GetRoad(at) != RoadType.None || _world.Map.GetFloor(at) != FloorType.Grass) continue;
                        var g = PutGrown(ti.Id, ti.X, ti.Y, quiet: true);
                        if (g != null) placedAll.Add(g);
                        continue;
                    }
                    if (ti.Slot != null) { PutOn(ti.Id, ti.X, ti.Y, 0, ti.Slot); continue; }
                    if (_world.Map.GetRoad(at) != RoadType.None) { _log($"Yol üstüne eşya konmaz: {ti.Id} {at}"); continue; }
                    if (ti.WallSide >= 0) { PutWall(ti.Id, at, ti.WallSide); continue; }
                    var placed = Put(ti.Id, ti.X, ti.Y);
                    if (placed != null) placedAll.Add(placed);
                }
                return placedAll;
            }

            /// <summary>Arsanın dışındaki yabani meşeler (olgun): herkes kesebilir, fidanlarıyla arsaya ağaç dikilir.</summary>
            public void PlaceWildOaks()
            {
                var rng = new Random(13);
                int placed = 0;
                for (int i = 0; i < 400 && placed < 24; i++)
                {
                    var p = new GridPos(rng.Next(70, 94), rng.Next(36, 70));
                    if (_world.Map.IsBlocked(p) || _world.Map.GetRoad(p) != RoadType.None || !_content.Items.TryGet("item.tree_oak", out var oak)) continue;
                    PlantSystem.SetGrown(_world.PlaceItem(oak, p));
                    placed++;
                }
            }
        }
    }
}
