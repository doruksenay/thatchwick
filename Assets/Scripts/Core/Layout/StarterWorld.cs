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

        public static GridMap BuildMap()
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
        public static int PlaceItems(WorldState world, ContentDatabase content, Action<string> log = null)
        {
            var s = new Seeder(world, content, log);
            s.PlaceFarm();
            s.PlaceRegions();
            s.PlaceWildOaks();
            return s.StallId;
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
            public void PlaceFarm()
            {
                // --- zemin kat: merdivenler, mutfak, yemek ve oturma ---
                Put("item.stairs_down", 29, 27);              // bodruma (giriş 29,26; iniş 29,29 kat -1)
                Put("item.stairs_up", 35, 27);                // üst kata (giriş 35,26; iniş 35,29 kat 1)
                Put("item.pantry_shelf", 30, 26);
                Put("item.hearth_stone", 32, 26);
                Put("item.cooking_pot", 33, 26);
                Put("item.water_barrel", 36, 26);
                Put("item.rug_wool", 31, 30);
                Put("item.table_wood", 31, 29);
                Put("item.chair_wood", 30, 29);
                Put("item.chair_wood", 33, 29);
                Put("item.basin_ewer", 28, 30);
                Put("item.candlestick", 28, 32);
                Put("item.armchair_cushion", 36, 31);
                Put("item.chest_wood", 36, 32);
                Put("item.stool_wood", 34, 32);
                PutOn("item.candle", 31, 29, 0, "a");
                PutOn("item.flower_pot", 31, 29, 0, "c");
                PutOn("item.clay_cup", 31, 29, 0, "b");

                // --- üst kat: üç yatak, sandıklar, kilimler, okuma köşesi ---
                Put("item.bed_straw", 28, 26, 1);
                Put("item.chest_wood", 30, 26, 1);
                Put("item.rug_wool_small", 28, 27, 1);
                Put("item.bed_straw", 32, 26, 1);
                Put("item.chest_wood", 34, 26, 1);
                Put("item.bed_straw", 28, 32, 1);
                Put("item.rug_woven_large", 30, 29, 1);
                Put("item.shelf_wood", 31, 32, 1);
                Put("item.armchair_cushion", 36, 30, 1);
                Put("item.candlestick", 36, 32, 1);
                Put("item.stool_wood", 33, 32, 1);
                PutOn("item.clay_cup", 31, 32, 1, "a");

                // --- bodrum: kiler ve atölye ---
                Put("item.barrel", 28, 26, -1);
                Put("item.barrel", 28, 27, -1);
                Put("item.salting_barrel", 31, 26, -1);
                Put("item.pantry_shelf", 32, 26, -1);
                Put("item.barrel", 35, 26, -1);
                Put("item.seed_crate", 36, 26, -1);
                Put("item.brew_barrel", 36, 28, -1);
                Put("item.barrel_tub", 36, 30, -1);
                Put("item.workbench", 28, 32, -1);
                Put("item.shelf_wood", 33, 32, -1);
                Put("item.barn_lantern", 32, 30, -1);

                // --- avlu ---
                Put("item.well", 25, 30);
                Put("item.chopping_block", 24, 27);
                Put("item.rain_barrel", 38, 26);
                Put("item.hay_bale", 24, 34);
                Put("item.garden_bench", 29, 35);
                Put("item.stool_wood", 34, 35);
                Put("item.stump_table", 35, 35);
                var stump = _world.GetItemAt(new GridPos(35, 35));
                if (stump != null && _content.Items.TryGet("item.morris_board", out var board)) _world.PlaceOnSurface(board, stump.Id, "a");
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

            /// <summary>Köy yolu ve şehir: dükkân içleri (meslek eşyaları, duvar ve yüzey dekoru), meydan, evler, ağaçlar.</summary>
            public void PlaceRegions()
            {
                foreach (var ti in Regions.Items())
                {
                    var at = new GridPos(ti.X, ti.Y);
                    if (ti.Grown)
                    {
                        // Ağaç ve çalı yalnızca açık toprağa: yol, meydan taşı, su, köprü ve bina içi atlanır.
                        if (_world.Map.GetRoad(at) != RoadType.None || _world.Map.GetFloor(at) != FloorType.Grass) continue;
                        PutGrown(ti.Id, ti.X, ti.Y, quiet: true);
                        continue;
                    }
                    if (ti.Slot != null) { PutOn(ti.Id, ti.X, ti.Y, 0, ti.Slot); continue; }
                    if (ti.WallSide >= 0) { PutWall(ti.Id, at, ti.WallSide); continue; }
                    var placed = Put(ti.Id, ti.X, ti.Y);
                    if (ti.Id == "item.market_stall" && placed != null) StallId = placed.Id;
                }
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
