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
        // Görseller ve pivotlar items.json'da (ItemDef.Art, yerleşim sistemi belge 44 B); yön ile görsel arasındaki bağ
        // Core.Layout.ItemFacing'te tek yerde. Eski el tablosu (Items) oraya taşındı.

        /// <summary>Pivot: veride yoksa ayak izine göre varsayılan (1x1 80 px tuval (40, 24); 2x1 (56, 32); dekor (16, 7)).</summary>
        private static Vector2 PivotOf(Thatchwick.Core.Content.ItemDef def)
        {
            var a = def.Art;
            if (a != null && (a.PivotX > 0f || a.PivotY > 0f)) return new Vector2(a.PivotX, a.PivotY);
            if (def.Placement == Thatchwick.Core.Content.ItemPlacement.Surface) return new Vector2(16f, 7f);
            int big = Mathf.Max(def.Width, def.Height);
            return big <= 1 ? new Vector2(40f, 24f) : def.Width * def.Height == 2 ? new Vector2(56f, 32f) : new Vector2(72f, 40f);
        }

        /// <summary>Katalog önizlemesi (yapı menüsü eşya ızgarası, belge 33): eşyanın varsayılan yöndeki görseli; yoksa null.</summary>
        public static Sprite ItemPreview(Thatchwick.Core.Content.ItemDef def)
        {
            if (def?.Art == null) return null;
            int rot = Thatchwick.Core.Layout.ItemFacing.Snap(def, 0);
            if (!Thatchwick.Core.Layout.ItemFacing.TryView(def, rot, false, out var name, out _)) name = def.Art.Grown;
            var p = PivotOf(def);
            return Get(name, p.x, p.y);
        }

        /// <summary>
        /// Eşyanın görseli. flipX: görsel yatay aynayla kullanıldıysa (yalnızca sol/sağ simetrik eşyada). Yön görseli yoksa
        /// (arkası dönük) ya da dosya henüz gelmediyse null: çağıran yer tutucu çizer, başka yönün görselini koymaz.
        /// </summary>
        public static Sprite Item(ItemInstance item, out bool flipX)
        {
            flipX = false;
            if (!Thatchwick.Core.Layout.ItemFacing.TryView(item.Def, item.Rotation, item.IsLit, out var name, out var flip)) return null;
            var p = PivotOf(item.Def);
            var s = Get(name, p.x, p.y);
            if (s != null) flipX = flip;
            return s;
        }

        /// <summary>Bitkinin görseli: meyvesi hazırsa meyveli, büyümediyse genç (varsa), yoksa olgun. young: genç evre görseli.</summary>
        public static Sprite Plant(ItemInstance item, out bool young)
        {
            young = false;
            var a = item.Def.Art;
            if (!Enabled || a == null) return null;
            string name = null;
            if (item.FruitReady && a.Fruit != null) name = a.Fruit;
            else if (!item.IsGrown && a.Young != null) { name = a.Young; young = true; }
            else name = a.Grown;
            var s = name != null ? Get(name, 40f, 24f) : null;
            if (s == null && young) { young = false; s = a.Grown != null ? Get(a.Grown, 40f, 24f) : null; }
            return s;
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
                case RoadType.Path: return GetCentered(h % 2 == 0 ? "iso_road_path_a" : "iso_road_path_b") ?? GetCentered("iso_road_path_a");
                case RoadType.Gravel: return GetCentered("iso_road_gravel_a");
                case RoadType.Paved: return GetCentered(band >= 2 ? "iso_road_paved_broken" : "iso_road_paved_worn");
                default: return null;
            }
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
        /// erkek/kadın. Durma görseli "_cand_<yön>", yürüme kareleri anim/ altında "_walk_<yön>_NN".
        /// Görseli henüz olmayan yön ya da evre eski 56'lık sete düşer.
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
