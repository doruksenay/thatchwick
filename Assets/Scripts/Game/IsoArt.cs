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
        // Bütün görseller: doku (atlas sayfası ya da kendi dokusu) ve içindeki piksel dikdörtgeni (performans incelemesi
        // 20.11.2026). Atlastaki görseller aynı sayfayı paylaşır; SpriteRenderer'lar toplu (batch) çizilebilir.
        private struct ArtRef { public Texture2D Tex; public Rect Rect; }
        private static Dictionary<string, ArtRef> _art;
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static int TextureCount { get { EnsureLoaded(); return _art.Count; } }

        /// <summary>Görsel kütüphanede var mı (atlasta ya da kendi dokusuyla).</summary>
        public static bool Has(string name) { EnsureLoaded(); return name != null && _art.ContainsKey(name); }

        private static void EnsureLoaded()
        {
            if (_textures != null) return;
            _textures = new Dictionary<string, Texture2D>();
            _art = new Dictionary<string, ArtRef>();
            var lib = Resources.Load<IsoArtLibrary>("IsoArt/IsoArtLibrary");
            if (lib == null) { Debug.LogWarning("IsoArt: görsel kütüphanesi yok (Thatchwick > Görsel kütüphanesini yenile). Yer tutucular kullanılıyor."); return; }
            foreach (var e in lib.entries)
            {
                if (e == null || string.IsNullOrEmpty(e.name)) continue;
                if (e.atlas != null && e.rect.width > 0f && e.rect.height > 0f)
                    _art[e.name] = new ArtRef { Tex = e.atlas, Rect = e.rect };
                else if (e.texture != null)
                {
                    _textures[e.name] = e.texture;
                    _art[e.name] = new ArtRef { Tex = e.texture, Rect = new Rect(0, 0, e.texture.width, e.texture.height) };
                }
            }
        }

        /// <summary>Kayıt (Play'den çıkınca sprite'lar yok edilir; sözlükteki "sahte null" ayıklanır).</summary>
        public static void Reset()
        {
            LayeredCharacterArt.Reset();
            _textures = null;
            _art = null;
            Sprites.Clear();
            IconCache.Clear();
            IconMissing.Clear();
            HeldSprites.Clear();
            FrameCounts.Clear();
        }

        /// <summary>Adı verilen görselden, pivotu piksel (x soldan, y alttan) olan sprite; görsel yoksa null.</summary>
        /// <summary>Kütüphanedeki doku (yoksa null). Katmanlı karakter (LayeredCharacterArt) bölge haritalarını buradan okur.</summary>
        public static Texture2D Texture(string name)
        {
            if (!Enabled || string.IsNullOrEmpty(name)) return null;
            EnsureLoaded();
            return _textures.TryGetValue(name, out var tex) ? tex : null;
        }

        public static Sprite Get(string name, float pivotX, float pivotY)
        {
            if (!Enabled || string.IsNullOrEmpty(name)) return null;
            EnsureLoaded();
            string key = name + "|" + pivotX + "|" + pivotY;
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            if (!_art.TryGetValue(name, out var art) || art.Tex == null) return null;
            var tex = art.Tex;
            tex.filterMode = FilterMode.Point;
            // Pivot görselin kendi pikselleriyle verilir; atlasta da dikdörtgenin içine göre (Sprite.Create pivotu orana çevirir).
            var sprite = Sprite.Create(tex, art.Rect,
                new Vector2(pivotX / art.Rect.width, pivotY / art.Rect.height), Ppu);
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
            if (!_art.TryGetValue(name, out var art) || art.Tex == null) return null;
            return Get(name, art.Rect.width * 0.5f, art.Rect.height * 0.5f);
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

        /// <summary>
        /// Su karosu (346): kuru çukur, buz, su (iki değişke). Denizde önce iso_ground_sea_a/b (484) aranır; yoksa göl suyu
        /// döner ve <paramref name="seaFallback"/> true olur (oyun katmanı denize mavi ton verir). Görsel yoksa null.
        /// </summary>
        public static Sprite Water(int x, int y, bool filled, bool frozen, bool sea, out bool seaFallback)
        {
            seaFallback = false;
            if (!filled) return GetCentered("iso_ground_pond_dry");
            if (frozen) return GetCentered("iso_ground_ice_a");
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            if (sea)
            {
                var s = GetCentered(h % 2 == 0 ? "iso_ground_sea_a" : "iso_ground_sea_b") ?? GetCentered("iso_ground_sea_a");
                if (s != null) return s;
                seaFallback = true;
            }
            return GetCentered(h % 4 == 0 ? "iso_ground_water_b" : "iso_ground_water_a") ?? GetCentered("iso_ground_water_a");
        }

        /// <summary>Kıyı karosu (346): kara (çamur ve saz) karonun sağ üst kenarında, yani +x komşusu yönünde. Sol üst (+y) için aynalanır.</summary>
        public static Sprite Shore() => GetCentered("iso_ground_water_shore");

        /// <summary>Kumsal karosu (484): iso_ground_beach_a/b; yoksa null (oyun katmanı toprağı kum rengine boyar).</summary>
        public static Sprite Beach(int x, int y)
        {
            int h = (x * 73856093) ^ (y * 19349663);
            if (h < 0) h = -h;
            return GetCentered(h % 2 == 0 ? "iso_ground_beach_a" : "iso_ground_beach_b") ?? GetCentered("iso_ground_beach_a");
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
        // Core.Layout.ItemFacing'te tek yerde. Eski el tablosu (Items) oraya taşındı (tools/layout/item_meta.py).
        // Bitki evreleri (ağaç, ekin) aşağıdaki Trees/Crops adlandırmasıyla çizilir.

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
            if (name == null) return null;
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
            ["item.crop_herb_bed"] = "iso_crop_herb_bed",   // belge 45 (art/queue 478a)
            ["item.crop_saffron"] = "iso_crop_saffron",
            ["item.crop_vetch"] = "iso_crop_vetch",
            // 58 §4 bulgu 2: evre görseli olup eşlemesi olmayan ekinler (317, 318, 318t) ve 478a/545 ile gelenler.
            ["item.crop_parsnip"] = "iso_crop_parsnip",
            ["item.crop_carrot"] = "iso_crop_carrot",
            ["item.crop_onion"] = "iso_crop_onion",
            ["item.crop_garlic"] = "iso_crop_garlic",
            ["item.crop_beet"] = "iso_crop_beet",
            ["item.crop_radish"] = "iso_crop_radish",
            ["item.crop_lettuce"] = "iso_crop_lettuce",
            ["item.crop_broad_bean"] = "iso_crop_broad_bean",
            ["item.crop_barley"] = "iso_crop_barley",
            ["item.crop_spinach"] = "iso_crop_spinach",
            ["item.crop_wheat"] = "iso_crop_wheat",
            ["item.crop_rye"] = "iso_crop_rye",
            ["item.crop_oats"] = "iso_crop_oats",
            ["item.crop_flax"] = "iso_crop_flax",
            ["item.crop_hemp"] = "iso_crop_hemp",
            ["item.crop_woad"] = "iso_crop_woad",
            ["item.crop_madder"] = "iso_crop_madder",
            ["item.crop_weld"] = "iso_crop_weld",
        };

        /// <summary>
        /// Ekin evresi (1-3). Üçüncü (olgun) görsel yalnızca ekin biçilmeye hazırken gelir (58 §4 bulgu 4): tarlaya bakınca
        /// neyin olgun olduğu etiketsiz anlaşılsın. Büyürken yarıya kadar filiz, sonra gelişmiş bitki.
        /// </summary>
        public static int CropStage(float growth, bool grown) => grown ? 3 : growth < 0.5f ? 1 : 2;

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
                int st = CropStage(item.GrowthFraction, item.IsGrown);
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
                    if (_art.ContainsKey($"iso_path_dirt_wang_03_{VariantLetters[v]}")) return true;
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
        /// <summary>
        /// İkinci renk maskesi (belge 45 §10): eşya görselinin "_accent" eki (korkuluk, minder, kulp; beyaz, saydam zemin).
        /// Yoksa null: ikinci renk yalnızca kartta görünür. Görsel adı ve ayna, eşyanın kendi görseliyle aynı kuraldan.
        /// </summary>
        public static Sprite ItemAccent(ItemInstance item)
        {
            if (!Thatchwick.Core.Layout.ItemFacing.TryView(item.Def, item.Rotation, false, out var name, out _)) return null;
            var p = PivotOf(item.Def);
            return Get(name + "_accent", p.x, p.y);
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
            ["wild.gull"] = ("iso_wild_gull_cand", 24f, 10f),     // iş 485
            ["wild.crab"] = ("iso_wild_crab_cand", 24f, 10f),     // iş 485
        };

        // Genetik gövdeleri (belge 48 §11, işler 484-487): tip başına nötr renkli tek gövde; renk paletle (tint) gelir.
        private static readonly Dictionary<string, (float px, float py)> Bodies = new Dictionary<string, (float, float)>
        {
            ["dog_shepherd"] = (21f, 4f), ["dog_sighthound"] = (21f, 4f), ["dog_scenthound"] = (19f, 4f), ["dog_tracker"] = (19f, 4f),
            ["dog_mastiff"] = (21f, 4f), ["dog_waterdog"] = (19f, 4f), ["dog_lapdog"] = (16f, 2f),
            ["cat_medium"] = (15f, 2f), ["cat_large"] = (15f, 2f), ["cat_slim"] = (15f, 2f),
            ["horse_light"] = (28f, 12f), ["horse_medium"] = (28f, 12f), ["horse_heavy"] = (28f, 12f),
            ["pony"] = (22f, 8f), ["donkey"] = (22f, 8f), ["mule"] = (28f, 12f), ["hinny"] = (24f, 10f),
        };

        /// <summary>Genetik gövde görseli: önce uzun tüylü eş (<c>_long</c>), sonra gövde; yoksa null (tür görseline düşülür).</summary>
        public static Sprite AnimalBody(string body, bool longCoat, int direction)
        {
            if (body == null || !Bodies.TryGetValue(body, out var b)) return null;
            string dir = Directions[direction];
            if (longCoat)
            {
                var l = Get("iso_animal_" + body + "_long_cand_" + dir, b.px, b.py) ?? Get("iso_animal_" + body + "_long_cand_se", b.px, b.py);
                if (l != null) return l;
            }
            return Get("iso_animal_" + body + "_cand_" + dir, b.px, b.py) ?? Get("iso_animal_" + body + "_cand_se", b.px, b.py);
        }

        /// <summary>
        /// Easter egg pozu (belge 55; işler 513-514): "howl" (köpek ulur), "box" (kedi kutuda), "stare" (öküz bakar).
        /// Kareler yalnızca se ve sw yönünde; batıya bakan yönler sw'ye, ötekiler se'ye düşer. Önce tipin gövdesi
        /// (iso_animal_&lt;gövde&gt;_&lt;poz&gt;_&lt;yön&gt;), sonra türün karesi; yoksa null (olağan görsel kalır).
        /// Kare dizisi varsa (…_&lt;yön&gt;_00, _01 …) döngü oynar; yoksa tek poz karesi çizilir.
        /// </summary>
        public static Sprite AnimalPose(string body, string speciesId, string pose, int direction, float cycle)
        {
            if (string.IsNullOrEmpty(pose) || !Enabled) return null;
            string dir = direction >= 3 && direction <= 6 ? "sw" : "se";
            if (body != null && Bodies.TryGetValue(body, out var b))
            {
                var s = PoseFrame("iso_animal_" + body + "_" + pose + "_", dir, b.px, b.py, cycle);
                if (s != null) return s;
            }
            if (speciesId == null || !Animals.TryGetValue(speciesId, out var a)) return null;
            string set = a.set.EndsWith("_cand", System.StringComparison.Ordinal) ? a.set.Substring(0, a.set.Length - 5) : a.set;
            return PoseFrame(set + "_" + pose + "_", dir, a.px, a.py, cycle);
        }

        private static Sprite PoseFrame(string stem, string dir, float px, float py, float cycle)
        {
            foreach (var d in dir == "se" ? new[] { "se" } : new[] { dir, "se" })
            {
                string prefix = stem + d + "_";
                int n = FrameCount(prefix, closingFrame: false);
                if (n > 0) return Get(prefix + (Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n).ToString("00"), px, py);
                var still = Get(stem + d, px, py);
                if (still != null) return still;
            }
            return null;
        }

        /// <summary>Kurbağanın başında beliren minik taç (iş 495; 16x16, pivot alt orta).</summary>
        public static Sprite TinyCrown => Get("iso_fx_tiny_crown", 8f, 0f);

        // ---- nüfusa girmeyen ziyaretçiler (Johan Wick, atsız şövalye; iş 496 ve 510) ----

        public static Sprite SetIdle(string set, int direction) =>
            Get(set + "_idle_" + Directions[direction], R84PivotX, R84PivotY) ?? Get(set + "_idle_se", R84PivotX, R84PivotY);

        /// <summary>Takımın yürüme karesi (walk2); bu yönün karesi yoksa null (durma görseli + kod sallantısı).</summary>
        public static Sprite SetWalk(string set, int direction, float cycle)
        {
            if (!Enabled) return null;
            string prefix = set + "_walk2_" + Directions[direction] + "_";
            int n = WalkFrameCount(prefix);
            if (n <= 0) return null;
            return Get(prefix + (Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n).ToString("00"), R84PivotX, R84PivotY);
        }

        /// <summary>Takımın yerinde hareket karesi ("pet", "wreck"): çapraz 4 yön, yoksa se, o da yoksa null.</summary>
        public static Sprite SetAction(string set, string key, int direction, float cycle)
        {
            if (!Enabled || string.IsNullOrEmpty(key)) return null;
            int diag = direction % 2 == 1 ? direction : (direction + 7) % 8;
            string prefix = set + "_" + key + "_" + Directions[diag] + "_";
            int n = FrameCount(prefix, closingFrame: false);
            // Bu yönün karesi yoksa eldeki en yakın çapraz yön (510: Johan'ın sevmesi yalnızca ne geldi): se, sonra ötekiler.
            for (int i = 0; n <= 0 && i < ActionFallback.Length; i++)
            {
                prefix = set + "_" + key + "_" + ActionFallback[i] + "_";
                n = FrameCount(prefix, closingFrame: false);
            }
            if (n <= 0) return null;
            return Get(prefix + (Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n) % n).ToString("00"), R84PivotX, R84PivotY);
        }

        private static readonly string[] ActionFallback = { "se", "sw", "ne", "nw" };

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
            // Katmanlı karakter (belge 43): manken görselleri varsa görünüm (saç, sakal, renkler, beden) gerçekten çizilir.
            var layered = LayeredCharacterArt.Frame(c, "idle", Directions[direction], 0);
            if (layered != null) return layered;
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
            int ln = LayeredCharacterArt.FrameCount(c, "walk2", Directions[direction]);
            if (ln > 0) return LayeredCharacterArt.Frame(c, "walk2", Directions[direction], Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * ln) % ln);
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
            {
                int ld = direction % 2 == 1 ? direction : (direction + 7) % 8;
                int ln = string.IsNullOrEmpty(key) ? 0 : LayeredCharacterArt.FrameCount(c, key, Directions[ld]);
                if (ln > 0)
                {
                    // El konumu (HandAnchors) 84 px etkileşim karelerinin adına bağlı: aynı kare öneki verilir.
                    frame = Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * ln) % ln;
                    string r84Prefix = AdultR84(c);
                    prefix = r84Prefix != null ? r84Prefix + "_" + key + "_" + Directions[ld] + "_" : null;
                    var lf = LayeredCharacterArt.Frame(c, key, Directions[ld], frame);
                    if (lf != null) return lf;
                    prefix = null; frame = 0;
                }
            }
            string r84 = AdultR84(c);
            if (!Enabled || string.IsNullOrEmpty(key)) return null;
            int diag = direction % 2 == 1 ? direction : (direction + 7) % 8;   // e(0)→se(7), n(2)→ne(1), w(4)→nw(3), s(6)→sw(5)
            if (r84 == null)
            {
                // Çocuk (64 px): yalnızca kendi kareleri varsa (iş 512: topaç, uluma, yerden alma). El konumu verilmez.
                if (c.Stage != AgeStage.Child) return null;
                string p64 = ChildR64(c) + "_" + key + "_" + Directions[diag] + "_";
                int n64 = FrameCount(p64, closingFrame: false);
                if (n64 <= 0) { p64 = ChildR64(c) + "_" + key + "_se_"; n64 = FrameCount(p64, closingFrame: false); }
                if (n64 <= 0) return null;
                return Get(p64 + (Mathf.FloorToInt(Mathf.Repeat(cycle, 1f) * n64) % n64).ToString("00"), R64PivotX, R64PivotY);
            }
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
            while (count < 32 && _art.ContainsKey(prefix + count.ToString("00"))) count++;
            // Yürüme: 9 kare teslim edildiyse son kare kapanıştır (ilk karenin tekrarı), 8 karelik döngü.
            n = closingFrame && count == 9 ? 8 : count;
            FrameCounts[prefix] = n;
            return n;
        }
    }
}
