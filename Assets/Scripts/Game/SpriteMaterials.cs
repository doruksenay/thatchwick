using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Işıksız (unlit) sprite materyali ve geçici (yer tutucu) izometrik şekiller.
    /// Gerçek varlıklar (A1) gelince şekiller yerini PixelLab sprite'larına bırakır.
    /// Shader.Find derlemelerde shader ayıklanırsa null dönebilir: APK adımından önce
    /// materyal bir asset olarak projeye eklenecek.
    /// </summary>
    public static class SpriteMaterials
    {
        private const int TileW = 64;
        private const int TileH = 32;
        private const int CubeHeight = 24;

        private static Material _unlit;
        private static Sprite _diamond;
        private static readonly Dictionary<Color32, Sprite> Cubes = new Dictionary<Color32, Sprite>();
        private static readonly Dictionary<Color32, Sprite> Pillars = new Dictionary<Color32, Sprite>();

        public static Material Unlit
        {
            get
            {
                if (_unlit != null) return _unlit;

                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                _unlit = new Material(shader) { name = "RuntimeUnlitSprite" };
                return _unlit;
            }
        }

        /// <summary>64x32 beyaz zemin elması (merkez pivot); renk SpriteRenderer.color ile verilir.</summary>
        public static Sprite Diamond
        {
            get
            {
                if (_diamond != null) return _diamond;

                var pixels = new Color32[TileW * TileH];
                for (int y = 0; y < TileH; y++)
                {
                    for (int x = 0; x < TileW; x++)
                    {
                        float dx = Mathf.Abs(x + 0.5f - TileW * 0.5f) / (TileW * 0.5f);
                        float dy = Mathf.Abs(y + 0.5f - TileH * 0.5f) / (TileH * 0.5f);
                        // Komşu karolar arasında çatlak kalmasın diye hafif taşırılır.
                        pixels[y * TileW + x] = dx + dy <= 1.04f ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                    }
                }
                _diamond = MakeSprite(pixels, TileW, TileH, new Vector2(0.5f, 0.5f));
                return _diamond;
            }
        }

        /// <summary>
        /// Bir karoluk yer tutucu küp: üst yüz verilen renk, sol yüz daha koyu, sağ yüz en koyu.
        /// Pivot, tabandaki elmasın merkezidir (karonun ortası).
        /// </summary>
        public static Sprite Cube(Color color)
        {
            Color32 key = color;
            // Play modundan çıkınca çalışma zamanı sprite'ları yok edilir; sözlükte "sahte null" kalabilir.
            if (Cubes.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = TileW;
            int h = TileH + CubeHeight;
            var pixels = new Color32[w * h];
            Color32 top = color;
            Color32 left = Shade(color, 0.78f);
            Color32 right = Shade(color, 0.58f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = x + 0.5f - w * 0.5f;
                    float ax = Mathf.Abs(dx);
                    float lowerEdge = 0.5f * ax;
                    float topLower = lowerEdge + CubeHeight;
                    float topUpper = TileH + CubeHeight - 0.5f * ax;
                    float py = y + 0.5f;

                    Color32 c = new Color32(0, 0, 0, 0);
                    if (py >= topLower && py <= topUpper) c = top;
                    else if (py >= lowerEdge && py < topLower) c = dx < 0f ? left : right;
                    pixels[y * w + x] = c;
                }
            }

            var sprite = MakeSprite(pixels, w, h, new Vector2(0.5f, (TileH * 0.5f) / h));
            Cubes[key] = sprite;
            return sprite;
        }

        /// <summary>Yer tutucu karakter: gövde + baş. Pivot, ayakların altı (karo merkezi).</summary>
        public static Sprite Pillar(Color color)
        {
            Color32 key = color;
            if (Pillars.TryGetValue(key, out var cached) && cached != null) return cached;

            const int w = 20;
            const int h = 44;
            var pixels = new Color32[w * h];
            Color32 body = color;
            Color32 head = new Color32(232, 195, 158, 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 c = new Color32(0, 0, 0, 0);
                    if (y >= 0 && y < 30 && x >= 3 && x < 17) c = body;
                    float hx = x + 0.5f - w * 0.5f;
                    float hy = y + 0.5f - 36f;
                    if (hx * hx + hy * hy <= 7f * 7f) c = head;
                    pixels[y * w + x] = c;
                }
            }

            var sprite = MakeSprite(pixels, w, h, new Vector2(0.5f, 0f));
            Pillars[key] = sprite;
            return sprite;
        }

        private static readonly Dictionary<int, Sprite> Walls = new Dictionary<int, Sprite>();
        public const int WallHeightPx = 40;
        public const int LowWallHeightPx = 10;

        /// <summary>
        /// Karo kenarı duvar parçası (32 px geniş paralelkenar, beyaz; renk SpriteRenderer.color ile).
        /// Horizontal: sol-alttan sağ-üste yükselir, pivot sol-alt. Vertical: sağ-alttan sol-üste, pivot sağ-alt.
        /// kind: Wall/Door/Window (kapı ve pencere açıklıklı); low = kesit modunda alçak duvar.
        /// </summary>
        public static Sprite Wall(Thatchwick.Core.World.EdgeAxis axis, Thatchwick.Core.World.EdgeKind kind, bool low)
        {
            int key = ((int)axis * 4 + (int)kind) * 2 + (low ? 1 : 0);
            if (Walls.TryGetValue(key, out var cached) && cached != null) return cached;

            const int w = 32;
            int wallH = low ? LowWallHeightPx : WallHeightPx;
            int h = 16 + wallH;
            bool horizontal = axis == Thatchwick.Core.World.EdgeAxis.Horizontal;
            var pixels = new Color32[w * h];
            byte shade = horizontal ? (byte)235 : (byte)175;
            for (int x = 0; x < w; x++)
            {
                int bottom = horizontal ? x / 2 : (w - 1 - x) / 2;
                for (int y = bottom; y < bottom + wallH; y++)
                {
                    bool top = y >= bottom + wallH - 3;
                    byte v = top ? (byte)255 : shade;
                    bool hole = false;
                    if (!low && kind == Thatchwick.Core.World.EdgeKind.Door) hole = x >= 9 && x < 23 && y < bottom + 30;
                    if (!low && kind == Thatchwick.Core.World.EdgeKind.Window) hole = x >= 10 && x < 22 && y >= bottom + 16 && y < bottom + 32;
                    if (!hole) pixels[y * w + x] = new Color32(v, v, v, 255);
                }
            }
            var sprite = MakeSprite(pixels, w, h, new Vector2(horizontal ? 0f : 1f, 0f));
            Walls[key] = sprite;
            return sprite;
        }

        private static Sprite _tree;

        /// <summary>Yer tutucu ağaç: kahverengi gövde + yeşil taç. Pivot gövde tabanı (karo merkezi). Büyüme ölçekle gösterilir.</summary>
        public static Sprite Tree
        {
            get
            {
                if (_tree != null) return _tree;
                const int w = 32, h = 56;
                var pixels = new Color32[w * h];
                var trunk = new Color32(110, 78, 48, 255);
                var leaf = new Color32(62, 128, 58, 255);
                var leafDark = new Color32(46, 100, 44, 255);
                var leafLight = new Color32(88, 156, 74, 255);
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = new Color32(0, 0, 0, 0);
                        if (x >= 14 && x < 18 && y < 24) c = trunk;
                        float dx = x + 0.5f - 16f, dy = y + 0.5f - 36f;
                        float d2 = dx * dx + dy * dy * 1.2f;
                        if (d2 <= 14f * 14f)
                            c = dy < -4f ? leafDark : (dx < -3f && dy > 2f ? leafLight : leaf);
                        pixels[y * w + x] = c;
                    }
                }
                _tree = MakeSprite(pixels, w, h, new Vector2(0.5f, 0f));
                return _tree;
            }
        }

        private static Color32 Shade(Color c, float factor) =>
            new Color32((byte)(c.r * factor * 255f), (byte)(c.g * factor * 255f), (byte)(c.b * factor * 255f), 255);

        private static Sprite MakeSprite(Color32[] pixels, int w, int h, Vector2 pivot)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels32(pixels);
            tex.Apply(false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, 32f);
        }
    }
}
