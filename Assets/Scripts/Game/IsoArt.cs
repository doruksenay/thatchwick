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
            ["item.cooking_pot"] = new ItemArt { AlongX = "iso_item_cooking_pot_se_v2", PivotX = 40, PivotY = 24 },     // 301b (v2)
            ["item.candle"] = new ItemArt { AlongX = "iso_item_candle_se", PivotX = 16, PivotY = 7 },
            ["item.flower_pot"] = new ItemArt { AlongX = "iso_item_flower_pot_se", PivotX = 16, PivotY = 7 },

            // Görsel parti 2 (18.11.2026): kuyruktan gelen adaylar (_aday). Review bağımlılık için yeterli (CLAUDE.md);
            // kabul edilince dosya Assets/Art/Iso'ya aynı adla taşınır, eşleme değişmez. Pivotlar art/results tablolarından.
            ["item.chair_carved"] = new ItemArt { AlongX = "iso_item_chair_carved_se", PivotX = 40, PivotY = 24 },          // 301
            ["item.chest_wood"] = new ItemArt { AlongX = "iso_item_chest_wood_se", PivotX = 40, PivotY = 24 },              // 301
            ["item.chopping_block"] = new ItemArt { AlongX = "iso_item_chopping_block_se", PivotX = 40, PivotY = 24 },      // 301
            ["item.market_stall"] = new ItemArt { AlongY = "iso_item_market_stall_2x1_se_nw_v2", PivotX = 56, PivotY = 32 }, // 301b
            ["item.clay_cup"] = new ItemArt { AlongX = "iso_item_clay_cup_se", PivotX = 16, PivotY = 7 },                    // 301b
            ["item.bench_long"] = new ItemArt { AlongY = "iso_item_bench_long_2x1_se_nw", PivotX = 56, PivotY = 32 },        // 302
            ["item.mattress_straw"] = new ItemArt { AlongX = "iso_item_mattress_straw_se", PivotX = 40, PivotY = 24 },       // 302
            ["item.child_bed"] = new ItemArt { AlongX = "iso_item_child_bed_se", PivotX = 40, PivotY = 24 },                 // 302
            ["item.stump_table"] = new ItemArt { AlongX = "iso_item_stump_table_se", PivotX = 40, PivotY = 24 },             // 302
            ["item.prep_counter"] = new ItemArt { AlongY = "iso_item_prep_counter_2x1_se_nw", PivotX = 56, PivotY = 32 },    // 302
            ["item.armchair_cushion"] = new ItemArt { AlongX = "iso_item_armchair_cushion_se", PivotX = 40, PivotY = 24 },   // 302
            ["item.brazier"] = new ItemArt { AlongX = "iso_item_brazier_se", PivotX = 40, PivotY = 24 },                     // 303
            ["item.stone_oven"] = new ItemArt { AlongY = "iso_item_stone_oven_2x1_se_nw", PivotX = 56, PivotY = 32 },        // 303
            ["item.barrel"] = new ItemArt { AlongX = "iso_item_barrel_se", PivotX = 40, PivotY = 24 },                       // 303
            ["item.woodshed"] = new ItemArt { AlongY = "iso_item_woodshed_2x1_se_nw", PivotX = 56, PivotY = 32 },            // 303
            ["item.seed_crate"] = new ItemArt { AlongX = "iso_item_seed_crate_se", PivotX = 40, PivotY = 24 },               // 303
            ["item.whetstone"] = new ItemArt { AlongX = "iso_item_whetstone_se", PivotX = 40, PivotY = 24 },                 // 303
            ["item.basin_ewer"] = new ItemArt { AlongX = "iso_item_basin_ewer_se", PivotX = 40, PivotY = 24 },               // 304
            ["item.water_bucket"] = new ItemArt { AlongX = "iso_item_water_bucket_se", PivotX = 40, PivotY = 24 },           // 304
            ["item.barrel_tub"] = new ItemArt { AlongX = "iso_item_barrel_tub_se", PivotX = 40, PivotY = 24 },               // 304
            ["item.outhouse"] = new ItemArt { AlongX = "iso_item_outhouse_se", PivotX = 40, PivotY = 24 },                   // 304
            ["item.well"] = new ItemArt { AlongX = "iso_item_well_se", PivotX = 40, PivotY = 24 },                           // 304
            ["item.scarecrow"] = new ItemArt { AlongX = "iso_item_scarecrow_se", PivotX = 40, PivotY = 24 },                 // 304
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
            // Bilinçli olarak eşlenmeyen: item.drying_rack (tanım 2x1, görsel 1x1 üretilmiş; yeniden iş gerekir).
        };

        /// <summary>Bitkinin olgun görseli (ağaçlar). Fidan ve genç evreler ölçekle küçültülür. Yoksa null.</summary>
        private static readonly Dictionary<string, string> Plants = new Dictionary<string, string>
        {
            ["item.tree_plum"] = "iso_tree_plum_grown_se",   // 319, 96x112
        };

        public static Sprite Plant(ItemInstance item) =>
            Enabled && Plants.TryGetValue(item.Def.Id, out var name) ? Get(name, 40f, 24f) : null;

        // ---------------- yollar ----------------

        /// <summary>
        /// Yol karosu (340): patika a/b, çakıl a, kaldırım yıpranmış ve bozuk. Sağlam kaldırım ve yıpranmış çakıl teslim
        /// edilmedi: kaldırımın sağlam hali de "worn" karosunu kullanır. Yoksa null (renkli elmasa düşer).
        /// </summary>
        public static Sprite Road(RoadType road, int band, int x, int y)
        {
            if (!Enabled) return null;
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            switch (road)
            {
                case RoadType.Path: return GetCentered(h % 2 == 0 ? "iso_road_path_a" : "iso_road_path_b") ?? GetCentered("iso_road_path_a");
                case RoadType.Gravel: return GetCentered("iso_road_gravel_a");
                case RoadType.Paved: return GetCentered(band >= 2 ? "iso_road_paved_broken" : "iso_road_paved_worn");
                default: return null;
            }
        }

        /// <summary>
        /// Eşyanın görseli. flipX: yalnızca diğer eksen için çizilmiş görselin ayna görüntüsü kullanıldıysa
        /// (ışık yönü ters olur; doğru görsel gelince kendiliğinden düzelir). Görsel yoksa null.
        /// </summary>
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
        };

        public static Sprite Animal(string speciesId, int direction)
        {
            if (!Animals.TryGetValue(speciesId, out var a)) return null;
            return Get(a.set + "_" + Directions[direction], a.px, a.py) ?? Get(a.set + "_se", a.px, a.py);
        }

        /// <summary>
        /// 84 px gerçekçi oranlı yetişkin taban (012b kabul, 018 kararı 18.11.2026): genç yetişkin ve yetişkin
        /// erkek/kadın. Durma görseli "_cand_<yön>", yürüme kareleri anim/ altında "_walk_<yön>_NN".
        /// Görseli henüz olmayan yön ya da evre eski 56'lık sete düşer.
        /// </summary>
        private static string AdultR84(Character c) =>
            c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child || c.Stage == AgeStage.Elder
                ? null
                : (c.Sex == Sex.Male ? "iso_char_man_r84" : "iso_char_woman_r84");

        private const float R84PivotX = 42f, R84PivotY = 2f;

        public static Sprite Character(Character c, int direction)
        {
            string r84 = AdultR84(c);
            if (r84 != null)
            {
                // 018b: 8 yönün durma karesi anim/ altında "_idle_<yön>"; ilk aday yalnızca "_cand_se".
                var s84 = Get(r84 + "_idle_" + Directions[direction], R84PivotX, R84PivotY)
                          ?? Get(r84 + "_cand_" + Directions[direction], R84PivotX, R84PivotY);
                if (s84 != null) return s84;
            }
            string set = CharacterSet(c, out float px, out float py);
            var own = Get(set + "_" + Directions[direction], px, py);
            if (own != null) return own;
            // Bu yönün 56'lık görseli de yoksa: 84'lük SE, o da yoksa 56'lık SE.
            return (r84 != null ? Get(r84 + "_cand_se", R84PivotX, R84PivotY) : null) ?? Get(set + "_se", px, py);
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
            string r84 = AdultR84(c);
            if (r84 == null || !Enabled) return null;
            string prefix = r84 + "_walk_" + Directions[direction] + "_";
            int n = WalkFrameCount(prefix);
            if (n <= 0) return null;
            int frame = Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n;
            return Get(prefix + frame.ToString("00"), R84PivotX, R84PivotY);
        }

        private static int WalkFrameCount(string prefix)
        {
            if (FrameCounts.TryGetValue(prefix, out int n)) return n;
            EnsureLoaded();
            int count = 0;
            while (count < 32 && _textures.ContainsKey(prefix + count.ToString("00"))) count++;
            // 9 kare teslim edildiyse son kare kapanıştır (ilk karenin tekrarı): 8 karelik döngü.
            n = count == 9 ? 8 : count;
            FrameCounts[prefix] = n;
            return n;
        }
    }
}
