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
            ["item.shelf_wood"] = new ItemArt { AlongY = "iso_item_shelf_wood_2x1_se_nw", PivotX = 56, PivotY = 32 },
            ["item.chair_wood"] = new ItemArt { AlongX = "iso_item_chair_wood_se", PivotX = 40, PivotY = 24 },
            ["item.chair_carved"] = new ItemArt { AlongX = "iso_item_chair_wood_se", PivotX = 40, PivotY = 24 },
            ["item.cooking_pot"] = new ItemArt { AlongX = "iso_item_cooking_pot_se", PivotX = 40, PivotY = 24 },
            ["item.candle"] = new ItemArt { AlongX = "iso_item_candle_se", PivotX = 16, PivotY = 7 },
            ["item.flower_pot"] = new ItemArt { AlongX = "iso_item_flower_pot_se", PivotX = 16, PivotY = 7 },
        };

        /// <summary>
        /// Eşyanın görseli. flipX: yalnızca diğer eksen için çizilmiş görselin ayna görüntüsü kullanıldıysa
        /// (ışık yönü ters olur; doğru görsel gelince kendiliğinden düzelir). Görsel yoksa null.
        /// </summary>
        public static Sprite Item(ItemInstance item, out bool flipX)
        {
            flipX = false;
            if (!Items.TryGetValue(item.Def.Id, out var art)) return null;
            bool alongX = item.Rotation % 2 == 0 || item.Def.Width == item.Def.Height;
            string name;
            if (art.Lit != null && item.IsLit) name = art.Lit;
            else if (alongX) { name = art.AlongX; if (name == null) { name = art.AlongY; flipX = true; } }
            else { name = art.AlongY; if (name == null) { name = art.AlongX; flipX = item.Def.Width != item.Def.Height; } }
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
            if (c.IsVillager && male) { pivotX = 34f; pivotY = 11f; return "iso_char_villager"; }   // 68x68
            if (male)
            {
                if (c.IsPlayerControlled || c.Id == 1) return "iso_char_farmer_a2_56_cand";
                return (c.Id % 2 == 0) ? "iso_char_woodcutter_a_56_cand" : "iso_char_craftsman_a_56_cand";
            }
            return (c.Id % 2 == 0) ? "iso_char_woman_a_56_cand" : "iso_char_farmer_f_56_cand";
        }

        public static Sprite Character(Character c, int direction)
        {
            string set = CharacterSet(c, out float px, out float py);
            return Get(set + "_" + Directions[direction], px, py) ?? Get(set + "_se", px, py);
        }
    }
}
