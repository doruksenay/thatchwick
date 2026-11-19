using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Gerçek izometrik görseller (PixelLab hattı) ile oyun nesneleri arasındaki eşleme. Bir görsel yoksa
    /// çağıran yer tutucu çizime döner; böylece görseller parça parça geldikçe oyun kendiliğinden güzelleşir.
    /// Pivotlar piksel cinsinden, ALTTAN: zemin elmasın merkezi, eşya ayak izi elmasının merkezi,
    /// karakter ayak altı, duvar kenarın başlangıç köşesi.
    /// </summary>
    public static class IsoArt
    {
        private const float Ppu = 32f;

        /// <summary>Oyuncu HUD'dan (G) yer tutucu ile gerçek görsel arasında geçiş yapabilir.</summary>
        public static bool Enabled = true;

        private static Dictionary<string, Texture2D> _textures;
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static int TextureCount { get { EnsureLoaded(); return _textures.Count; } }

        private static void EnsureLoaded()
        {
            if (_textures != null) return;
            _textures = new Dictionary<string, Texture2D>();
            var lib = Resources.Load<IsoArtLibrary>("IsoArt/IsoArtLibrary");
            if (lib == null) { Debug.LogWarning("IsoArt: görsel kütüphanesi yok (Thatchwick > Görsel kütüphanesini yenile). Yer tutucular kullanılıyor."); return; }
            foreach (var e in lib.entries)
                if (e != null && e.texture != null && !string.IsNullOrEmpty(e.name)) _textures[e.name] = e.texture;
        }

        /// <summary>Kayıt (Play'den çıkınca sprite'lar yok edilir; sözlükteki "sahte null" ayıklanır).</summary>
        public static void Reset()
        {
            _textures = null;
            Sprites.Clear();
            IconCache.Clear();
            IconMissing.Clear();
            HeldSprites.Clear();
            FrameCounts.Clear();
        }

        /// <summary>Adı verilen görselden, pivotu piksel (x soldan, y alttan) olan sprite; görsel yoksa null.</summary>
        public static Sprite Get(string name, float pivotX, float pivotY)
        {
            if (!Enabled || string.IsNullOrEmpty(name)) return null;
            EnsureLoaded();
            string key = name + "|" + pivotX + "|" + pivotY;
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            if (!_textures.TryGetValue(name, out var tex) || tex == null) return null;
            tex.filterMode = FilterMode.Point;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(pivotX / tex.width, pivotY / tex.height), Ppu);
            sprite.name = name;
            Sprites[key] = sprite;
            return sprite;
        }

        // ---------------- ürün ikonları ----------------

        /// <summary>Kabul edilen ikonun adı varsayılan addan farklıysa (v2, v3...).</summary>
        private static readonly Dictionary<string, string> IconOverrides = new Dictionary<string, string>
        {
            ["item.blank_book"] = "icon_blank_book_v2",   // 401
            ["item.oak_gall"] = "icon_oak_gall_v3",       // 417
        };

        private static readonly Dictionary<string, Texture2D> IconCache = new Dictionary<string, Texture2D>();
        private static readonly HashSet<string> IconMissing = new HashSet<string>();

        /// <summary>
        /// Eşyanın ürün ikonu (arayüz listeleri için). Sırayla: geçersiz kılma, icon_ad, icon_item_ad.
        /// Yoksa null (liste yalnızca yazı gösterir).
        /// </summary>
        public static Texture2D ItemIcon(string itemId)
        {
            if (!Enabled || string.IsNullOrEmpty(itemId) || IconMissing.Contains(itemId)) return null;
            if (IconCache.TryGetValue(itemId, out var hit) && hit != null) return hit;
            EnsureLoaded();
            string shortId = itemId.StartsWith("item.") ? itemId.Substring(5) : itemId;
            Texture2D tex = null;
            if (IconOverrides.TryGetValue(itemId, out var name)) _textures.TryGetValue(name, out tex);
            if (tex == null) _textures.TryGetValue("icon_" + shortId, out tex);
            if (tex == null) _textures.TryGetValue("icon_item_" + shortId, out tex);
            if (tex == null) { IconMissing.Add(itemId); return null; }
            tex.filterMode = FilterMode.Point;
            IconCache[itemId] = tex;
            return tex;
        }

        /// <summary>IMGUI satırında ikon kutusu (ikon yoksa aynı genişlikte boşluk, hizalı kalsın).</summary>
        public static void IconCell(string itemId, float size = 24f)
        {
            var tex = ItemIcon(itemId);
            if (tex != null) GUILayout.Label(tex, GUILayout.Width(size), GUILayout.Height(size));
            else GUILayout.Space(size + 4f);
        }

        private static Sprite GetCentered(string name)
        {
            if (!Enabled) return null;
            EnsureLoaded();
            if (!_textures.TryGetValue(name, out var tex) || tex == null) return null;
            return Get(name, tex.width * 0.5f, tex.height * 0.5f);
        }

        // ---------------- zemin ----------------

        private static readonly string[] GrassTiles = { "iso_ground_grass_a", "iso_ground_grass_b", "iso_ground_grass_c" };
        private static readonly string[] DirtTiles = { "iso_ground_dirt_a", "iso_ground_dirt_b" };

        /// <summary>Çim ve toprak için gerçek karo (konuma göre sabit varyasyon); diğer zeminler için null.</summary>
        public static Sprite Ground(FloorType floor, int x, int y)
        {
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            switch (floor)
            {
                case FloorType.Grass: return GetCentered(GrassTiles[h % GrassTiles.Length]);
                case FloorType.Dirt: return GetCentered(DirtTiles[h % DirtTiles.Length]);
                default: return null;
            }
        }

        // ---------------- duvar ----------------

        /// <summary>
        /// Kenar duvarı (102x115 tuval). Horizontal kenar (+x, ekranda "/") = sw_ne; Vertical (+y, "\") = se_nw.
        /// Pivot kenarın başlangıç köşesi: sw_ne (32, 19), se_nw (70, 19). Pencere görseli yok: duvar çizilir, renkle ayrılır.
        /// </summary>
        public static Sprite Wall(EdgeAxis axis, EdgeKind kind)
        {
            string a = axis == EdgeAxis.Horizontal ? "sw_ne" : "se_nw";
            string type = kind == EdgeKind.Door ? "door" : "wall";
            return Get($"iso_wall_{type}_{a}", axis == EdgeAxis.Horizontal ? 32f : 70f, 19f);
        }

        /// <summary>
        /// Malzemeli/stilli kenar parçası (yapı parçası kimliğiyle): <c>iso_wall_&lt;parça&gt;[_low|_half]_&lt;eksen&gt;</c>, ör.
        /// iso_wall_wall_stone_sw_ne, iso_wall_door_iron_se_nw, iso_wall_fence_picket_sw_ne. Tuval ve pivot düz duvarla aynı.
        /// Görsel yoksa null (çağıran yer tutucu renkle çizer).
        /// </summary>
        public static Sprite WallPart(string partId, EdgeAxis axis, string variant = null)
        {
            if (string.IsNullOrEmpty(partId)) return null;
            string a = axis == EdgeAxis.Horizontal ? "sw_ne" : "se_nw";
            string v = string.IsNullOrEmpty(variant) ? "" : "_" + variant;
            return Get($"iso_wall_{partId}{v}_{a}", axis == EdgeAxis.Horizontal ? 32f : 70f, 19f);
        }

        /// <summary>Zemin stili karosu: iso_floor_&lt;ad&gt;_a / _b (parça kimliğinden "floor_" atılır; 64x32 elmas, merkez pivot); yoksa null.</summary>
        public static Sprite FloorPart(string partId, int x, int y)
        {
            if (string.IsNullOrEmpty(partId)) return null;
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            string n = partId.StartsWith("floor_") ? partId.Substring(6) : partId;   // floor_oak → iso_floor_oak_a
            return GetCentered($"iso_floor_{n}_{(h % 2 == 0 ? "a" : "b")}") ?? GetCentered($"iso_floor_{n}_a");
        }

        /// <summary>Çatı karosu dokusu: iso_roof_&lt;ad&gt;_a (eğim), _ridge (sırt; "roof_" atılır); motor eğim yönüne göre gölgeler. Yoksa null.</summary>
        public static Sprite RoofPart(string partId, bool ridge)
        {
            if (string.IsNullOrEmpty(partId)) return null;
            string n = partId.StartsWith("roof_") ? partId.Substring(5) : partId;     // roof_slate → iso_roof_slate_a
            return (ridge ? GetCentered($"iso_roof_{n}_ridge") : null) ?? GetCentered($"iso_roof_{n}_a");
        }

        // ---------------- eşyalar ----------------

        private sealed class ItemArt
        {
            public string AlongX;     // uzun ekseni ekranda "/" (dönüş 0/2)
            public string AlongY;     // uzun ekseni "\" (dönüş 1/3)
            public float PivotX, PivotY;
            public string Lit;        // ısı kaynağı yanarken
        }

        // 1x1 eşya (80 px tuval) pivotu (40, 24); 2x1 eşya (112 px) (56, 32); dekor (32 px) (16, 7).
        private static readonly Dictionary<string, ItemArt> Items = new Dictionary<string, ItemArt>
        {
            ["item.bed_straw"] = new ItemArt { AlongX = "iso_item_bed_straw_2x1_sw_ne", AlongY = "iso_item_bed_straw_2x1_se_nw", PivotX = 56, PivotY = 32 },
            ["item.hearth_stone"] = new ItemArt { AlongX = "iso_item_hearth_stone_unlit_se", Lit = "iso_item_hearth_stone_lit_se", PivotX = 40, PivotY = 24 },
            ["item.stool_wood"] = new ItemArt { AlongX = "iso_item_stool_se", PivotX = 40, PivotY = 24 },
            ["item.table_wood"] = new ItemArt { AlongY = "iso_item_table_wood_2x1_se_nw", PivotX = 56, PivotY = 32 },
            ["item.shelf_wood"] = new ItemArt { AlongY = "iso_item_shelf_wood_2x1_se_nw_v2", PivotX = 56, PivotY = 32 },   // 301 kabul (v2)
            ["item.chair_wood"] = new ItemArt { AlongX = "iso_item_chair_wood_sw", PivotX = 40, PivotY = 24 },
            // item.cooking_pot: 301b v2 reddedildi (kullanıcı 18.11.2026); yenisi 301c v3 gelince eşlenir.
            ["item.candle"] = new ItemArt { AlongX = "iso_item_candle_se", PivotX = 16, PivotY = 7 },
            ["item.flower_pot"] = new ItemArt { AlongX = "iso_item_flower_pot_se", PivotX = 16, PivotY = 7 },

            // Görsel parti 2 (18.11.2026): kuyruktan gelen adaylar (_aday). Review bağımlılık için yeterli (CLAUDE.md);
            // kabul edilince dosya Assets/Art/Iso'ya aynı adla taşınır, eşleme değişmez. Pivotlar art/results tablolarından.
            ["item.chair_carved"] = new ItemArt { AlongX = "iso_item_chair_carved_se", PivotX = 40, PivotY = 24 },          // 301
            ["item.chest_wood"] = new ItemArt { AlongX = "iso_item_chest_wood_se", PivotX = 40, PivotY = 24 },              // 301
            ["item.chopping_block"] = new ItemArt { AlongX = "iso_item_chopping_block_se", PivotX = 40, PivotY = 24 },      // 301
            // item.market_stall ve item.clay_cup: 301b reddedildi; yenileri 301c (v3, v2).
            ["item.bench_long"] = new ItemArt { AlongY = "iso_item_bench_long_2x1_se_nw", PivotX = 56, PivotY = 32 },        // 302
            ["item.mattress_straw"] = new ItemArt { AlongX = "iso_item_mattress_straw_se", PivotX = 40, PivotY = 24 },       // 302
            ["item.child_bed"] = new ItemArt { AlongX = "iso_item_child_bed_se", PivotX = 40, PivotY = 24 },                 // 302
            ["item.stump_table"] = new ItemArt { AlongX = "iso_item_stump_table_se", PivotX = 40, PivotY = 24 },             // 302
            ["item.prep_counter"] = new ItemArt { AlongY = "iso_item_prep_counter_2x1_se_nw", PivotX = 56, PivotY = 32 },    // 302
            ["item.armchair_cushion"] = new ItemArt { AlongX = "iso_item_armchair_cushion_se", PivotX = 40, PivotY = 24 },   // 302
            ["item.brazier"] = new ItemArt { AlongX = "iso_item_brazier_se", PivotX = 40, PivotY = 24 },                     // 303
            // item.stone_oven: 303'teki reddedildi (çömlekçi fırını gibi); yenisi 303b v2.
            ["item.barrel"] = new ItemArt { AlongX = "iso_item_barrel_se", PivotX = 40, PivotY = 24 },                       // 303
            ["item.woodshed"] = new ItemArt { AlongY = "iso_item_woodshed_2x1_se_nw", PivotX = 56, PivotY = 32 },            // 303
            ["item.seed_crate"] = new ItemArt { AlongX = "iso_item_seed_crate_se", PivotX = 40, PivotY = 24 },               // 303
            ["item.whetstone"] = new ItemArt { AlongX = "iso_item_whetstone_se", PivotX = 40, PivotY = 24 },                 // 303
            ["item.basin_ewer"] = new ItemArt { AlongX = "iso_item_basin_ewer_se", PivotX = 40, PivotY = 24 },               // 304
            ["item.water_bucket"] = new ItemArt { AlongX = "iso_item_water_bucket_se", PivotX = 40, PivotY = 24 },           // 304
            ["item.barrel_tub"] = new ItemArt { AlongX = "iso_item_barrel_tub_se", PivotX = 40, PivotY = 24 },               // 304
            ["item.outhouse"] = new ItemArt { AlongX = "iso_item_outhouse_se", PivotX = 40, PivotY = 24 },                   // 304
            ["item.well"] = new ItemArt { AlongX = "iso_item_well_se", PivotX = 40, PivotY = 24 },                           // 304
            // item.scarecrow: 304'teki reddedildi; yenisi 304b v2 (review) aşağıda.
            ["item.garden_bench"] = new ItemArt { AlongY = "iso_item_garden_bench_2x1_se_nw", PivotX = 56, PivotY = 32 },    // 304
            ["item.child_chair"] = new ItemArt { AlongX = "iso_item_child_chair_se", PivotX = 40, PivotY = 24 },             // 304
            ["item.toy_chest"] = new ItemArt { AlongX = "iso_item_toy_chest_se", PivotX = 40, PivotY = 24 },                 // 304
            ["item.jug"] = new ItemArt { AlongX = "iso_item_jug_se", PivotX = 16, PivotY = 7 },                              // 304
            ["item.salting_barrel"] = new ItemArt { AlongX = "iso_item_salting_barrel_se", PivotX = 40, PivotY = 24 },       // 312
            ["item.compost_heap"] = new ItemArt { AlongX = "iso_item_compost_heap_se", PivotX = 40, PivotY = 24 },           // 321
            ["item.seed_maker"] = new ItemArt { AlongY = "iso_item_seed_maker_2x1_se", PivotX = 56, PivotY = 32 },           // 321
            ["item.bucket_pulley"] = new ItemArt { AlongX = "iso_item_bucket_pulley_se", PivotX = 40, PivotY = 24 },         // 321 (72 px tuval)
            ["item.smokehouse"] = new ItemArt { AlongY = "iso_item_smokehouse_2x1_se", PivotX = 56, PivotY = 32 },           // 324
            ["item.cheese_press"] = new ItemArt { AlongX = "iso_item_cheese_press_se", PivotX = 40, PivotY = 24 },           // 324 (72 px tuval)
            ["item.forage_pinecone"] = new ItemArt { AlongX = "iso_forage_pinecone_se", PivotX = 40, PivotY = 24 },          // 322
            // ---- Ev dekorasyonu adayları (387-390, 400; onay bekliyor, oyunda denensin diye bağlı). Tek "_se" görsel uzun ekseni "\\" (AlongY). ----
            // ---- Ev bakımı ve otomasyon (belge 45; görsel işleri 440-445, gelene kadar yer tutucu) ----
            ["item.wall_niche"] = new ItemArt { AlongX = "iso_wall_item_wall_niche_sw_ne", PivotX = 24, PivotY = 32 },            // 440
            ["item.wall_niche_lamp"] = new ItemArt { AlongX = "iso_wall_item_wall_niche_lamp_sw_ne", PivotX = 24, PivotY = 32 },  // 440
            ["item.window_seat"] = new ItemArt { AlongX = "iso_wall_item_window_seat_sw_ne", PivotX = 24, PivotY = 24 },          // 440
            ["item.ceiling_painted_beam"] = new ItemArt { AlongX = "iso_ceiling_item_painted_beam_se", PivotX = 32, PivotY = 16 }, // 440
            ["item.embroidery_frame"] = new ItemArt { AlongX = "iso_item_embroidery_frame_se", PivotX = 40, PivotY = 24 },        // 440
            ["item.display_shelf"] = new ItemArt { AlongX = "iso_wall_item_display_shelf_2w_sw_ne", PivotX = 48, PivotY = 32 },   // 441 (2 kenar)
            ["item.display_cabinet"] = new ItemArt { AlongX = "iso_item_display_cabinet_se", PivotX = 40, PivotY = 24 },          // 441
            ["item.clothes_perch"] = new ItemArt { AlongX = "iso_wall_item_clothes_perch_2w_sw_ne", PivotX = 48, PivotY = 32 },   // 441 (2 kenar)
            ["item.home_ribbon"] = new ItemArt { AlongX = "iso_wall_item_home_ribbon_sw_ne", PivotX = 20, PivotY = 20 },          // 441
            ["item.turf_seat"] = new ItemArt { AlongY = "iso_item_turf_seat_2x1_se", PivotX = 56, PivotY = 32 },                  // 442
            ["item.sundial"] = new ItemArt { AlongX = "iso_item_sundial_se", PivotX = 40, PivotY = 24 },                          // 442
            ["item.dovecote"] = new ItemArt { AlongX = "iso_item_dovecote_2x2_se", PivotX = 72, PivotY = 40 },                    // 442
            ["item.dovecote_village"] = new ItemArt { AlongX = "iso_item_dovecote_village_se", PivotX = 40, PivotY = 24 },        // 442
            ["item.courtyard_pool"] = new ItemArt { AlongX = "iso_item_courtyard_pool_2x2_se", PivotX = 72, PivotY = 40 },        // 443
            ["item.joke_fountain"] = new ItemArt { AlongX = "iso_item_joke_fountain_se", PivotX = 40, PivotY = 24 },              // 443
            ["item.jazari_peacock"] = new ItemArt { AlongX = "iso_item_jazari_peacock_se", PivotX = 40, PivotY = 24 },            // 443
            ["item.water_clock"] = new ItemArt { AlongX = "iso_item_water_clock_se", PivotX = 40, PivotY = 24 },                  // 443
            ["item.stairs_up"] = new ItemArt { AlongY = "iso_item_stairs_up_wood_1x2_se", PivotX = 56, PivotY = 32 },             // 450 (+ _accent korkuluk)
            ["item.stairs_down"] = new ItemArt { AlongY = "iso_item_stairs_down_wood_1x2_se", PivotX = 56, PivotY = 32 },         // 450
            ["item.stairs_up_stone"] = new ItemArt { AlongY = "iso_item_stairs_up_stone_1x2_se", PivotX = 56, PivotY = 32 },      // 450
            ["item.stairs_down_stone"] = new ItemArt { AlongY = "iso_item_stairs_down_stone_1x2_se", PivotX = 56, PivotY = 32 },  // 450
            ["item.turning_eagle_lectern"] = new ItemArt { AlongX = "iso_item_eagle_lectern_se", PivotX = 40, PivotY = 24 },      // 443
            ["item.sedir"] = new ItemArt { AlongY = "iso_item_sedir_2x1_se", PivotX = 56, PivotY = 32 },                    // 390
            ["item.sedir_long"] = new ItemArt { AlongY = "iso_item_sedir_long_3x1_se", PivotX = 72, PivotY = 40 },          // 390 (144 px)
            ["item.sedir_bed"] = new ItemArt { AlongY = "iso_item_sedir_bed_2x1_se", PivotX = 56, PivotY = 32 },            // 390
            ["item.floor_bed"] = new ItemArt { AlongY = "iso_item_floor_bed_2x1_se", PivotX = 56, PivotY = 32 },            // 390
            ["item.yukluk"] = new ItemArt { AlongY = "iso_item_yukluk_2x1_se", PivotX = 56, PivotY = 32 },                  // 390 (136 px boy)
            ["item.floor_cushion"] = new ItemArt { AlongX = "iso_item_floor_cushion_se", PivotX = 40, PivotY = 24 },        // 390
            ["item.box_bed"] = new ItemArt { AlongY = "iso_item_box_bed_2x1_se", PivotX = 56, PivotY = 32 },                // 400
            ["item.kilim_small"] = new ItemArt { AlongY = "iso_item_kilim_small_2x1_se", PivotX = 56, PivotY = 32 },        // 390 (halı)
            ["item.kilim_large"] = new ItemArt { AlongY = "iso_item_kilim_large_3x2_se", PivotX = 88, PivotY = 48 },        // 390 (halı, 176 px)
            ["item.cicim"] = new ItemArt { AlongX = "iso_item_cicim_2x2_se", PivotX = 72, PivotY = 40 },                    // 390 (halı, 144 px)
            // Çocukluk oyuncakları (399, 401, 406, 417 kabul; belge 30). Yuva oyuncakları 32 px, pivot taban elması.
            ["item.slingshot"] = new ItemArt { AlongX = "iso_item_slingshot_se", PivotX = 16, PivotY = 7 },                 // 399
            ["item.jump_rope"] = new ItemArt { AlongX = "iso_item_jump_rope_se", PivotX = 16, PivotY = 7 },                 // 399
            ["item.kite"] = new ItemArt { AlongX = "iso_item_kite_se", PivotX = 16, PivotY = 10 },                          // 399 (32x40, dış)
            ["item.hoop"] = new ItemArt { AlongX = "iso_item_hoop_se_v3", PivotX = 16, PivotY = 10 },                       // 406 (dış)
            ["item.marbles"] = new ItemArt { AlongX = "iso_item_marbles_se_v4", PivotX = 16, PivotY = 4 },                  // 417
            ["item.whittling_knife"] = new ItemArt { AlongX = "iso_item_whittling_knife_se", PivotX = 16, PivotY = 7 },     // 401
            ["item.juggling_balls"] = new ItemArt { AlongX = "iso_item_juggling_balls_se", PivotX = 16, PivotY = 7 },       // 401
            ["item.book_almanac"] = new ItemArt { AlongX = "iso_item_book_almanac_se", PivotX = 16, PivotY = 7 },   // 421 (gelince)
            ["item.book_herbal"] = new ItemArt { AlongX = "iso_item_book_herbal_se", PivotX = 16, PivotY = 7 },   // 421 (gelince)
            ["item.book_husbandry"] = new ItemArt { AlongX = "iso_item_book_husbandry_se", PivotX = 16, PivotY = 7 },   // 421 (gelince)
            ["item.book_smithing"] = new ItemArt { AlongX = "iso_item_book_smithing_se", PivotX = 16, PivotY = 7 },   // 421 (gelince)
            // Masa ve raf üstü (32 px tuval; pivot = nesnenin tabanı)
            ["item.bread_basket"] = new ItemArt { AlongX = "iso_item_bread_basket_se", PivotX = 16, PivotY = 2 },           // 387
            ["item.fruit_basket"] = new ItemArt { AlongX = "iso_item_fruit_basket_se", PivotX = 16, PivotY = 2 },           // 387
            ["item.cheese_board"] = new ItemArt { AlongX = "iso_item_cheese_board_se", PivotX = 16, PivotY = 5 },           // 387
            ["item.glass_jar"] = new ItemArt { AlongX = "iso_item_glass_jar_se", PivotX = 16, PivotY = 5 },                 // 400
            ["item.hourglass"] = new ItemArt { AlongX = "iso_item_hourglass_se", PivotX = 16, PivotY = 3 },                 // 400
            ["item.copper_hand_mirror"] = new ItemArt { AlongX = "iso_item_copper_hand_mirror_se", PivotX = 16, PivotY = 4 }, // 384 (32x40)
            // Duvar eşyaları (pivot = görselin ortası, duvardaki bant noktasına oturur; diğer duvarda ayna görüntüsü)
            ["item.flower_wreath"] = new ItemArt { AlongX = "iso_wall_item_flower_wreath_sw_ne", PivotX = 20, PivotY = 20 },   // 381
            ["item.harvest_wreath"] = new ItemArt { AlongX = "iso_wall_item_harvest_wreath_sw_ne", PivotX = 20, PivotY = 20 }, // 387
            ["item.kilim_hanging"] = new ItemArt { AlongX = "iso_wall_item_kilim_hanging_2w_sw_ne", PivotX = 48, PivotY = 32 }, // 381 (2 kenar)
            ["item.large_mirror"] = new ItemArt { AlongX = "iso_wall_item_large_mirror_2w_sw_ne", PivotX = 48, PivotY = 32 },   // 400 (2 kenar)
            // ---- Görsel parti 3 (19.11.2026): kabul edilen ve review'daki görseller ----
            ["item.scarecrow"] = new ItemArt { AlongX = "iso_item_scarecrow_se_v2", PivotX = 40, PivotY = 24 },              // 304b (review)
            ["item.market_stall"] = new ItemArt { AlongY = "iso_item_market_stall_2x1_se_nw_v3", PivotX = 56, PivotY = 32 }, // 301c (review)
            ["item.cooking_pot"] = new ItemArt { AlongX = "iso_item_cooking_pot_se_v3", PivotX = 40, PivotY = 24 },          // 301c (review)
            ["item.clay_cup"] = new ItemArt { AlongX = "iso_item_clay_cup_se_v2", PivotX = 16, PivotY = 4 },                 // 301c (review)
            ["item.grave"] = new ItemArt { AlongX = "iso_item_grave_se", PivotX = 32, PivotY = 24 },                         // 371 (313 kurtarma)
            ["item.hanging_lantern"] = new ItemArt { AlongX = "iso_deco_lantern_se", PivotX = 32, PivotY = 24 },             // 371 (direkte fener)
            // Toplama düğümleri (80x72, pivot 40,24): 371 kurtarma ve 373 kabul. Böğürtlen ve kestane reddedildi (378 v2 gelecek).
            ["item.forage_dry_branch"] = new ItemArt { AlongX = "iso_forage_dry_branch_se", PivotX = 40, PivotY = 24 },
            ["item.forage_hive"] = new ItemArt { AlongX = "iso_forage_hive_se", PivotX = 40, PivotY = 24 },
            ["item.forage_mushroom"] = new ItemArt { AlongX = "iso_forage_mushroom_se", PivotX = 40, PivotY = 24 },
            ["item.forage_wild_garlic"] = new ItemArt { AlongX = "iso_forage_wild_garlic_se", PivotX = 40, PivotY = 24 },
            ["item.forage_cress"] = new ItemArt { AlongX = "iso_forage_cress_se", PivotX = 40, PivotY = 24 },
            ["item.forage_herbs"] = new ItemArt { AlongX = "iso_forage_herbs_se", PivotX = 40, PivotY = 24 },
            ["item.forage_elderflower"] = new ItemArt { AlongX = "iso_forage_elderflower_se", PivotX = 40, PivotY = 24 },
            ["item.forage_strawberry"] = new ItemArt { AlongX = "iso_forage_strawberry_se", PivotX = 40, PivotY = 24 },
            ["item.forage_rosehip"] = new ItemArt { AlongX = "iso_forage_rosehip_se", PivotX = 40, PivotY = 24 },
            ["item.forage_nettle"] = new ItemArt { AlongX = "iso_forage_nettle_se", PivotX = 40, PivotY = 24 },
            ["item.forage_hazel"] = new ItemArt { AlongX = "iso_forage_hazel_se", PivotX = 40, PivotY = 24 },                // 378 (review; dosya yoksa küp)
            ["item.forage_berry_bush"] = new ItemArt { AlongX = "iso_forage_berry_bush_v2_se", PivotX = 40, PivotY = 24 },   // 378 (review)
            ["item.forage_chestnut"] = new ItemArt { AlongX = "iso_forage_chestnut_v2_se", PivotX = 40, PivotY = 24 },       // 378 (review)
            // Duvar eşyaları (306 kabul; pivot = görselin merkezi, alttan)
            ["item.torch_wall"] = new ItemArt { AlongX = "iso_wall_item_torch_wall_sw_ne", PivotX = 24, PivotY = 32 },
            ["item.oil_lamp_wall"] = new ItemArt { AlongX = "iso_wall_item_oil_lamp_wall_sw_ne", PivotX = 24, PivotY = 32 },
            ["item.wall_shelf"] = new ItemArt { AlongX = "iso_wall_item_wall_shelf_sw_ne", PivotX = 32, PivotY = 24 },
            ["item.herb_bundle_wall"] = new ItemArt { AlongX = "iso_wall_item_herb_bundle_wall_sw_ne", PivotX = 20, PivotY = 28 },
            ["item.tool_rack_wall"] = new ItemArt { AlongX = "iso_wall_item_tool_rack_wall_sw_ne", PivotX = 32, PivotY = 32 },
            // Bilinçli olarak eşlenmeyen: item.drying_rack (tanım 2x1, görsel 1x1 üretilmiş; yeniden iş gerekir).
        };

        /// <summary>
        /// Ağaç türleri: görsel öneki (96x112, pivot 40,24). Evreler: "_young_se" (büyürken), "_grown_se" (olgun),
        /// "_fruit_se" (meyve hazır). 319, 371, 372, 377. Evrenin görseli yoksa en yakın evreye düşer.
        /// </summary>
        private static readonly Dictionary<string, string> Trees = new Dictionary<string, string>
        {
            ["item.tree_oak"] = "iso_tree_oak",
            ["item.tree_apple"] = "iso_tree_apple",
            ["item.tree_pear"] = "iso_tree_pear",
            ["item.tree_cherry"] = "iso_tree_cherry",
            ["item.tree_plum"] = "iso_tree_plum",
            ["item.tree_birch"] = "iso_tree_birch",
            ["item.tree_pine"] = "iso_tree_pine",
            ["item.tree_willow"] = "iso_tree_willow",
        };

        /// <summary>Ekinler: evre görselleri "_1_se", "_2_se", "_3_se" (80x72, pivot 40,24). 310 kurtarma (371) ve 374.</summary>
        private static readonly Dictionary<string, string> Crops = new Dictionary<string, string>
        {
            ["item.crop_cabbage"] = "iso_crop_cabbage",
            ["item.crop_leek"] = "iso_crop_leek",
            ["item.crop_pea"] = "iso_crop_pea",
            ["item.crop_pumpkin"] = "iso_crop_pumpkin",
            ["item.crop_turnip"] = "iso_crop_turnip",
        };

        /// <summary>Ekin evresi (1-3) büyüme oranından.</summary>
        public static int CropStage(float growth) => growth < 0.34f ? 1 : growth < 0.67f ? 2 : 3;

        /// <summary>
        /// Bitkinin görseli. staged: görsel bitkinin şimdiki evresine ait (genç ağaç, ekin evresi); çağıran onu ölçekle
        /// küçültmez. false ise olgun görsel döndü, çağıran büyüme oranıyla küçültür. Görsel yoksa null.
        /// </summary>
        public static Sprite Plant(ItemInstance item, out bool staged)
        {
            staged = false;
            if (!Enabled || item == null) return null;
            string id = item.Def.Id;
            if (Crops.TryGetValue(id, out var crop))
            {
                int st = CropStage(item.GrowthFraction);
                for (int k = st; k >= 1; k--)
                {
                    var s = Get($"{crop}_{k}_se", 40f, 24f);
                    if (s != null) { staged = true; return s; }
                }
                return null;
            }
            if (!Trees.TryGetValue(id, out var tree)) return null;
            if (!item.IsGrown)
            {
                var young = Get(tree + "_young_se", 40f, 24f);
                if (young != null) { staged = true; return young; }
                return Get(tree + "_grown_se", 40f, 24f);
            }
            staged = true;
            if (item.FruitReady)
            {
                var fruit = Get(tree + "_fruit_se", 40f, 24f);
                if (fruit != null) return fruit;
            }
            return Get(tree + "_grown_se", 40f, 24f) ?? Get(tree + "_young_se", 40f, 24f);
        }

        public static Sprite Plant(ItemInstance item) => Plant(item, out _);

        // ---------------- yollar ----------------

        /// <summary>
        /// Yol karosu (340): patika a/b, çakıl a, kaldırım yıpranmış ve bozuk. Sağlam kaldırım ve yıpranmış çakıl teslim
        /// edilmedi: kaldırımın sağlam hali de "worn" karosunu kullanır. Yoksa null (renkli elmasa düşer).
        /// </summary>
        // Toprak yol otomatik karo seti (art/results/004): maske bitleri N=1 E=2 S=4 W=8 (PixelLab), dosya = tile_N.
        // Izgarada: N = x+1, E = y-1, S = x-1, W = y+1 (Iso.GridToWorld). 0 (tek başına) ve tam iç: tamamı toprak (17).
        /// <summary>Otomatik karo seti dış çizgisiz yeniden üretildiğinde true yapılır.</summary>
        public static bool PathSetSeamless = false;

        private static readonly int[] PathTileByMask = { 17, 2, 3, 10, 4, 7, 8, 15, 5, 11, 6, 14, 9, 13, 12, 16 };

        /// <summary>Patika karosu komşu maskesine göre (düz, köşe, T, kavşak, yol sonu); fullInterior: dört komşu ve köşeler de yol.</summary>
        public static Sprite PathTile(int mask, bool fullInterior, int x, int y)
        {
            if (!Enabled) return null;
            // 004 seti "outline" moduyla üretilmiş: her karonun çevresinde koyu çizgi var (kenar parlaklığı 0,31, iç 0,47),
            // yol ızgara gibi görünüyor. Set dış çizgisiz yeniden üretilene kadar (art/queue) dikişsiz toprak zemin karosu çizilir.
            if (!PathSetSeamless) return Ground(FloorType.Dirt, x, y);
            // Set 1 karoluk patika için çizilmiş: kenar payı ve köşe çentikleri 2 karoluk yolda zincir gibi görünüyor.
            // Bu yüzden yalnızca yol sonu (tek komşu) kendi karosunu alır; geri kalan her yol karosu kenarsız tam topraktır,
            // yan yana gelince kesintisiz bir şerit olur.
            int m = mask & 15;
            bool end = m == 1 || m == 2 || m == 4 || m == 8;
            int idx = end && !fullInterior ? PathTileByMask[m] : 17;
            return GetCentered($"iso_ground_path_dirt_{idx:00}");
        }

        // ---------------- toprak yol, köşe tabanlı Wang seti (art/queue/004e) ----------------
        // Dosya: iso_path_dirt_wang_<NN>_<v>.png; NN = köşe maskesi (bit 0 üst, 1 sağ, 2 alt, 3 sol; 1 = toprak),
        // v = a/b/c (aynı maskenin bağımsız çizilmiş üç çeşidi). Tam toprak (15): iso_path_dirt_fill_v2_<v>.
        // Karolar dual-grid ile çizilir (WorldView.RebuildPathLayer): her karo bir köşe noktasında durur, dört köşesi
        // o noktaya değen dört ızgara karosundan gelir.
        public const int PathWangVariants = 3;
        private static readonly string[] VariantLetters = { "a", "b", "c" };

        /// <summary>Wang seti yüklü mü (en az düz kenar karosunun bir çeşidi). Yoksa yollar eski düz toprakla çizilir.</summary>
        public static bool PathWangReady
        {
            get
            {
                if (!Enabled) return false;
                EnsureLoaded();
                for (int v = 0; v < PathWangVariants; v++)
                    if (_textures.ContainsKey($"iso_path_dirt_wang_03_{VariantLetters[v]}")) return true;
                return false;
            }
        }

        /// <summary>
        /// Köşe maskesine göre geçiş karosu; istenen çeşit yoksa diğer çeşitlere bakar. Maske 15 tam toprak dolgudur.
        /// Hiç yoksa null (çağıran o köşe noktasını boş bırakır).
        /// </summary>
        public static Sprite PathWang(int mask, int variant)
        {
            if (!Enabled || mask <= 0) return null;
            for (int i = 0; i < PathWangVariants; i++)
            {
                string v = VariantLetters[(variant + i) % PathWangVariants];
                var s = mask >= 15 ? GetCentered($"iso_path_dirt_fill_v2_{v}") : GetCentered($"iso_path_dirt_wang_{mask:00}_{v}");
                if (s != null) return s;
            }
            return mask >= 15 ? GetCentered("iso_path_dirt_fill_a") ?? Ground(FloorType.Dirt, 0, 0) : null;
        }

        public static Sprite Road(RoadType road, int band, int x, int y)
        {
            if (!Enabled) return null;
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            switch (road)
            {
                // 376 kabul: patika b v2 (kenarında çim yok), yıpranmış çakıl; 376b: çakıl a v2, sağlam ve bozuk kaldırım v2.
                case RoadType.Path:
                    return GetCentered(h % 2 == 0 ? "iso_road_path_a" : "iso_road_path_b_v2") ?? GetCentered("iso_road_path_a");
                case RoadType.Gravel:
                    return band >= 1
                        ? GetCentered("iso_road_gravel_worn") ?? GetCentered("iso_road_gravel_a_v2") ?? GetCentered("iso_road_gravel_a")
                        : GetCentered("iso_road_gravel_a_v2") ?? GetCentered("iso_road_gravel_a");
                case RoadType.Paved:
                    return band >= 2
                        ? GetCentered("iso_road_paved_broken_v2") ?? GetCentered("iso_road_paved_broken")
                        : band == 0 ? GetCentered("iso_road_paved_a") ?? GetCentered("iso_road_paved_worn") : GetCentered("iso_road_paved_worn");
                default: return null;
            }
        }

        /// <summary>
        /// Eşyanın görseli. flipX: yalnızca diğer eksen için çizilmiş görselin ayna görüntüsü kullanıldıysa
        /// (ışık yönü ters olur; doğru görsel gelince kendiliğinden düzelir). Görsel yoksa null.
        /// </summary>
        /// <summary>Katalog önizlemesi (yapı menüsü eşya ızgarası, belge 33): eşyanın varsayılan yöndeki görseli; yoksa null.</summary>
        public static Sprite ItemPreview(string itemId)
        {
            if (itemId == null || !Items.TryGetValue(itemId, out var art)) return null;
            return Get(art.AlongX ?? art.AlongY, art.PivotX, art.PivotY);
        }

        /// <summary>
        /// İkinci renk maskesi (belge 45 §10): eşya görselinin "_accent" eki (korkuluk, minder, kulp; beyaz, saydam zemin).
        /// Yoksa null: ikinci renk yalnızca kartta görünür.
        /// </summary>
        public static Sprite ItemAccent(ItemInstance item)
        {
            if (!Items.TryGetValue(item.Def.Id, out var art)) return null;
            bool square = item.Def.Width == item.Def.Height;
            string name = square ? (art.AlongX ?? art.AlongY) : item.Rotation % 2 == 0 ? (art.AlongX ?? art.AlongY) : (art.AlongY ?? art.AlongX);
            return name == null ? null : Get(name + "_accent", art.PivotX, art.PivotY);
        }

        public static Sprite Item(ItemInstance item, out bool flipX)
        {
            flipX = false;
            if (!Items.TryGetValue(item.Def.Id, out var art)) return null;
            bool square = item.Def.Width == item.Def.Height;
            bool alongX = item.Rotation % 2 == 0;
            string name;
            if (square)
            {
                // Kare eşyada tek yön çizildi: tek dönüşlerde ayna görüntüsü (yapı modunda döndürme görünür olsun).
                name = art.Lit != null && item.IsLit ? art.Lit : (art.AlongX ?? art.AlongY);
                flipX = item.Rotation % 2 == 1;
            }
            else if (alongX) { name = art.AlongX; if (name == null) { name = art.AlongY; flipX = true; } }
            else { name = art.AlongY; if (name == null) { name = art.AlongX; flipX = true; } }
            var s = Get(name, art.PivotX, art.PivotY);
            if (s == null) flipX = false;
            return s;
        }

        // ---------------- karakterler ----------------

        /// <summary>Ekran yönleri; görsel adlarının son eki.</summary>
        public static readonly string[] Directions = { "e", "ne", "n", "nw", "w", "sw", "s", "se" };

        /// <summary>Dünya (ekran) hareket vektöründen 8 yönden biri (0 = doğu, saat yönünün tersine).</summary>
        public static int DirectionIndex(Vector2 delta)
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            return Mathf.RoundToInt(angle / 45f) % 8;
        }

        public const int DefaultDirection = 7;   // se: izleyiciye dönük

        /// <summary>
        /// Karakter görsel seti (cinsiyet, yaş evresi, kimliğe göre sabit seçim). Adaylar (_aday) henüz onay
        /// beklese de oyunda denensin diye kullanılır. Ad öneki; yön eki eklenir.
        /// </summary>
        public static string CharacterSet(Character c, out float pivotX, out float pivotY)
        {
            pivotX = 30f; pivotY = 1f;           // 60x60 aday seti: ayaklar en alt satırda
            bool male = c.Sex == Sex.Male;
            switch (c.Stage)
            {
                case AgeStage.Baby:
                case AgeStage.Child:
                    return male ? "iso_char_boy_a_56_cand" : "iso_char_girl_a_56_cand";
                case AgeStage.Elder:
                    return male ? "iso_char_elder_m_56_cand" : "iso_char_elder_f_56_cand";
            }
            // Eski 68x68 köylü (iso_char_villager) bırakıldı: köylüler de 56'lık K2 setini kullanır (boy kararı 56, 2026-11-08).
            if (male)
            {
                if (c.IsPlayerControlled || c.Id == 1) return "iso_char_farmer_a2_56_cand";
                return (c.Id % 2 == 0) ? "iso_char_woodcutter_a_56_cand" : "iso_char_craftsman_a_56_cand";
            }
            return (c.Id % 2 == 0) ? "iso_char_woman_a_56_cand" : "iso_char_farmer_f_56_cand";
        }

        // ---------------- hayvanlar ----------------

        // Hayvan setleri (aday): tür → (ad öneki, pivot x, pivot y alttan). Görseli olmayan tür yer tutucuda kalır.
        private static readonly Dictionary<string, (string set, float px, float py)> Animals = new Dictionary<string, (string, float, float)>
        {
            ["animal.sheep"] = ("iso_animal_sheep_1_cand", 26f, 14f),   // 56x56, ayaklar alttan 14 px
            ["animal.ox"] = ("iso_animal_ox_cand", 28f, 12f),          // iş 411; dosya yoksa yer tutucu
            ["animal.cat"] = ("iso_animal_cat_1_cand", 15f, 2f),       // 375 kabul, 32x32, ayak bandı alttan 2 px
            ["animal.dog"] = ("iso_animal_dog_1_cand", 19f, 4f),       // 379 (review), 40x40; dosya yoksa yer tutucu
            // Yaban hayvanları (iş 408-410): dosya yoksa yer tutucu küp.
            ["wild.rabbit"] = ("iso_wild_rabbit_cand", 24f, 10f),
            ["wild.squirrel"] = ("iso_wild_squirrel_cand", 24f, 10f),
            ["wild.hedgehog"] = ("iso_wild_hedgehog_cand", 24f, 10f),
            ["wild.roe_deer"] = ("iso_wild_roe_deer_cand", 24f, 10f),
            ["wild.frog"] = ("iso_wild_frog_cand", 24f, 10f),
            ["wild.stork"] = ("iso_wild_stork_cand", 24f, 10f),
            ["wild.crow"] = ("iso_wild_crow_cand", 24f, 10f),
            ["wild.owl"] = ("iso_wild_owl_cand", 24f, 10f),
            ["wild.sparrow"] = ("iso_wild_sparrow_cand", 24f, 10f),
            ["wild.butterfly"] = ("iso_wild_butterfly_cand", 24f, 10f),
            ["wild.firefly"] = ("iso_wild_firefly_cand", 24f, 10f),
            ["wild.fox"] = ("iso_wild_fox_cand", 24f, 10f),
            ["wild.wolf"] = ("iso_wild_wolf_cand", 24f, 10f),
            ["wild.boar"] = ("iso_wild_boar_cand", 24f, 10f),
            ["wild.rat"] = ("iso_wild_rat_cand", 24f, 10f),
            ["wild.bear"] = ("iso_wild_bear_cand", 24f, 10f),
        };

        public static Sprite Animal(string speciesId, int direction)
        {
            if (!Animals.TryGetValue(speciesId, out var a)) return null;
            return Get(a.set + "_" + Directions[direction], a.px, a.py) ?? Get(a.set + "_se", a.px, a.py);
        }

        /// <summary>
        /// 84 px gerçekçi oranlı yetişkin taban (012b kabul, 018 kararı 18.11.2026): genç yetişkin ve yetişkin
        /// erkek/kadın. Durma görseli "_idle_<yön>" (ilk aday "_cand_se"), yürüme kareleri anim/ altında
        /// "_walk2_<yön>_NN" (018b2). Görseli olmayan yön 84'lük SE'ye düşer; reddedilen 56'lık K2 seti yalnızca son çare.
        /// </summary>
        private static string AdultR84(Character c)
        {
            switch (c.Stage)
            {
                case AgeStage.Baby:
                case AgeStage.Child:
                    return null;
                case AgeStage.Elder:   // 415: yaşlı da 84 px gerçekçi tabana geçer
                    return c.Sex == Sex.Male ? "iso_char_elder_m_r84" : "iso_char_elder_f_r84";
            }
            return c.Sex == Sex.Male ? "iso_char_man_r84" : "iso_char_woman_r84";
        }

        private const float R84PivotX = 42f, R84PivotY = 2f;

        /// <summary>416: çocuk 84'e büyümez; aynı tarzda 64 px tuval (bebek bunun 0,6 ölçeği).</summary>
        private static string ChildR64(Character c) =>
            c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child
                ? (c.Sex == Sex.Male ? "iso_char_boy_r64" : "iso_char_girl_r64")
                : null;

        private const float R64PivotX = 32f, R64PivotY = 2f;

        public static Sprite Character(Character c, int direction)
        {
            string r64 = ChildR64(c);
            if (r64 != null)
            {
                var s64 = Get(r64 + "_idle_" + Directions[direction], R64PivotX, R64PivotY) ?? Get(r64 + "_idle_se", R64PivotX, R64PivotY);
                if (s64 != null) return s64;
            }
            string r84 = AdultR84(c);
            if (r84 != null)
            {
                // 018b: 8 yönün durma karesi anim/ altında "_idle_<yön>"; ilk aday yalnızca "_cand_se".
                // Düzeltilmiş sürümler önce (423: küreksiz _v2, 423b: se _v3).
                string idle = r84 + "_idle_" + Directions[direction];
                var s84 = Get(idle + "_v3", R84PivotX, R84PivotY)
                          ?? Get(idle + "_v2", R84PivotX, R84PivotY)
                          ?? Get(idle, R84PivotX, R84PivotY)
                          ?? Get(r84 + "_cand_" + Directions[direction], R84PivotX, R84PivotY)
                          // 012 seti reddedildi (19.11.2026): yönün 84'lüğü yoksa 56'lık değil 84'lük SE.
                          ?? Get(r84 + "_idle_se_v3", R84PivotX, R84PivotY)
                          ?? Get(r84 + "_idle_se_v2", R84PivotX, R84PivotY)
                          ?? Get(r84 + "_idle_se", R84PivotX, R84PivotY)
                          ?? Get(r84 + "_cand_se", R84PivotX, R84PivotY);
                if (s84 != null) return s84;
            }
            // Son çare: eski 56'lık K2 seti (012, reddedildi). Yalnızca 84/64 görseli hiç yüklenmemişse.
            string set = CharacterSet(c, out float px, out float py);
            return Get(set + "_" + Directions[direction], px, py) ?? Get(set + "_se", px, py);
        }

        // (set|yön) → kare sayısı; 0 = kare animasyonu yok.
        private static readonly Dictionary<string, int> FrameCounts = new Dictionary<string, int>();

        /// <summary>
        /// Yürüme döngüsü karesi (018a/018b). cycle: 0..1 arası adım evresi (tam döngü = iki adım).
        /// Kareler _00.._NN; son kare ilkine yakın kapanış karesiyse (018a: 08 ≈ 00) döngüde atlanır.
        /// Bu yönün karesi yoksa null: çağıran durma görseli + kodla sallantıya düşer.
        /// </summary>
        public static Sprite CharacterWalk(Character c, int direction, float cycle)
        {
            if (!Enabled) return null;
            // Kabul edilen yürüme "walk2" (018b2/018b3; yaşlı 015a, çocuk 015c). Eski "walk" (018b) reddedildi:
            // zıplıyor; ona düşülmez. Bu yönün walk2 karesi yoksa null: durma görseli + kod sallantısı.
            string r64 = ChildR64(c);
            string set = r64 ?? AdultR84(c);
            if (set == null) return null;
            float px = r64 != null ? R64PivotX : R84PivotX, py = r64 != null ? R64PivotY : R84PivotY;
            string prefix = set + "_walk2_" + Directions[direction] + "_";
            int n = WalkFrameCount(prefix);
            if (n <= 0) return null;
            int frame = Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n;
            return Get(prefix + frame.ToString("00"), px, py);
        }

        /// <summary>
        /// Etkileşim döngüsü karesi (018c-e): anahtar (cook, craft_hammer, work, eat2...), yön ve 0..1 döngü evresi.
        /// Kareler yalnızca çapraz 4 yönde (se, sw, nw, ne); düz yön en yakın çapraza düşer (e → se, n → ne,
        /// w → nw, s → sw), o da yoksa se. Etkileşim karelerinin 9.'su kapanış değil, hepsi oynar.
        /// Kare yoksa null: çağıran kodla iş vuruşuna düşer.
        /// </summary>
        public static Sprite CharacterAction(Character c, string key, int direction, float cycle) =>
            CharacterAction(c, key, direction, cycle, out _, out _);

        /// <summary>Etkileşim karesi; kare adının öneki ve kare numarası da döner (el konumu için).</summary>
        /// <summary>Etkileşim karelerinin çizildiği çapraz yön adı (se, sw, nw, ne).</summary>
        public static string ActionDirection(int direction) => Directions[direction % 2 == 1 ? direction : (direction + 7) % 8];

        public static Sprite CharacterAction(Character c, string key, int direction, float cycle, out string prefix, out int frame)
        {
            prefix = null; frame = 0;
            string r84 = AdultR84(c);
            if (r84 == null || !Enabled || string.IsNullOrEmpty(key)) return null;
            int diag = direction % 2 == 1 ? direction : (direction + 7) % 8;   // e(0)→se(7), n(2)→ne(1), w(4)→nw(3), s(6)→sw(5)
            prefix = r84 + "_" + key + "_" + Directions[diag] + "_";
            int n = FrameCount(prefix, closingFrame: false);
            if (n <= 0)
            {
                prefix = r84 + "_" + key + "_se_";
                n = FrameCount(prefix, closingFrame: false);
                if (n <= 0) { prefix = null; return null; }
            }
            frame = Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n;
            return Get(prefix + frame.ToString("00"), R84PivotX, R84PivotY);
        }

        /// <summary>Animasyon karesinin pivotu (piksel): elde tutulan nesneyi kareye göre yerleştirmek için.</summary>
        public static Vector2 R84Pivot => new Vector2(R84PivotX, R84PivotY);
        public static float PixelsPerUnit => Ppu;

        private static readonly Dictionary<string, Sprite> HeldSprites = new Dictionary<string, Sprite>();

        /// <summary>Elde tutulan nesnenin sprite'ı: ürün ikonu, ortadan pivotlu (ikon yoksa null).</summary>
        public static Sprite HeldItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            if (HeldSprites.TryGetValue(itemId, out var s) && s != null) return s;
            var tex = ItemIcon(itemId);
            if (tex == null) return null;
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), Ppu);
            s.name = "held_" + itemId;
            HeldSprites[itemId] = s;
            return s;
        }

        private static int WalkFrameCount(string prefix) => FrameCount(prefix, closingFrame: true);

        private static int FrameCount(string prefix, bool closingFrame)
        {
            if (FrameCounts.TryGetValue(prefix, out int n)) return n;
            EnsureLoaded();
            int count = 0;
            while (count < 32 && _textures.ContainsKey(prefix + count.ToString("00"))) count++;
            // Yürüme: 9 kare teslim edildiyse son kare kapanıştır (ilk karenin tekrarı), 8 karelik döngü.
            n = closingFrame && count == 9 ? 8 : count;
            FrameCounts[prefix] = n;
            return n;
        }
    }
}
