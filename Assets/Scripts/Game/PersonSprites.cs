using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Customization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Görünüm verisinden (Appearance) geçici, prosedürel karakter sprite'ı çizer: gövde, kıyafet,
    /// baş, saç, sakal, yüz işareti ve başlık katmanları. Gerçek (PixelLab) katmanlı sprite'lar
    /// gelince bu sınıf yerini onlara bırakır; Appearance verisi aynı kalır.
    /// Tuval 24x48 px (PPU 32), pivot ayak altı ortası.
    /// </summary>
    public static class PersonSprites
    {
        private const int W = 24;
        private const int H = 48;
        private const float Ppu = 32f;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        private static readonly Color32 OutlineColor = new Color32(35, 28, 28, 255);

        /// <summary>Oyun içi (önbellekli) sprite. Play'den çıkınca yok edilmiş olanları yeniden üretir.</summary>
        public static Sprite Get(Appearance a)
        {
            string key = a.Key();
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = Create(a);
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Önbelleksiz yeni sprite (yaratma ekranı önizlemesi: çağıran yok eder).</summary>
        public static Sprite Create(Appearance a)
        {
            var buf = new Color32[W * H];
            Draw(buf, a);
            Outline(buf);

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels32(buf);
            tex.Apply(false);
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), Ppu);
        }

        public static void Destroy(Sprite sprite)
        {
            if (sprite == null) return;
            var tex = sprite.texture;
            UnityEngine.Object.Destroy(sprite);
            if (tex != null) UnityEngine.Object.Destroy(tex);
        }

        // ---- çizim yardımcıları ----

        private static Color32 C(Rgb c) => new Color32(c.R, c.G, c.B, 255);
        private static Color32 Mix(Rgb a, Rgb b, double t) => C(Rgb.Lerp(a, b, t));

        private static void Px(Color32[] buf, int x, int y, Color32 c)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) return;
            buf[y * W + x] = c;
        }

        private static void FillRect(Color32[] buf, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Px(buf, x, y, c);
        }

        private static void Ellipse(Color32[] buf, float cx, float cy, float rx, float ry, Color32 c)
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    if (dx * dx + dy * dy <= 1f) buf[y * W + x] = c;
                }
            }
        }

        private static void Outline(Color32[] buf)
        {
            var copy = (Color32[])buf.Clone();
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if (copy[y * W + x].a != 0) continue;
                    bool near =
                        (x > 0 && copy[y * W + x - 1].a != 0) || (x < W - 1 && copy[y * W + x + 1].a != 0) ||
                        (y > 0 && copy[(y - 1) * W + x].a != 0) || (y < H - 1 && copy[(y + 1) * W + x].a != 0);
                    if (near) buf[y * W + x] = OutlineColor;
                }
            }
        }

        private static void Draw(Color32[] buf, Appearance a)
        {
            for (int i = 0; i < buf.Length; i++) buf[i] = Clear;

            Color32 skin = C(a.SkinColor);
            Color32 outfit = C(a.OutfitColor);
            Color32 trim = C(a.TrimColor);
            Color32 hair = C(a.HairColor);
            Color32 hat = C(a.HatColor);
            Color32 shoes = new Color32(45, 34, 28, 255);

            int bw = a.Build == Thatchwick.Core.Customization.BodyBuild.Slim ? 4
                   : a.Build == Thatchwick.Core.Customization.BodyBuild.Sturdy ? 6 : 5;
            const int cx = 12;

            // Kapüşon: baş ve omuzların arkasındaki katman.
            bool hood = a.HatId == "hood";
            if (hood)
            {
                Ellipse(buf, cx, 38f, 7.5f, 8.5f, hat);
                FillRect(buf, cx - bw - 1, 26, cx + bw, 31, hat);
            }

            // Bacaklar ve ayakkabılar.
            bool longOutfit = a.OutfitId == "long_tunic" || a.OutfitId == "robe" || a.OutfitId == "dress";
            FillRect(buf, cx - 3, 2, cx - 1, 13, trim);
            FillRect(buf, cx + 1, 2, cx + 3, 13, trim);
            FillRect(buf, cx - 4, 0, cx - 1, 1, shoes);
            FillRect(buf, cx + 1, 0, cx + 4, 1, shoes);

            // Gövde ve kıyafet.
            int hem = a.OutfitId == "robe" ? 3 : a.OutfitId == "dress" ? 4 : a.OutfitId == "long_tunic" ? 7 : 12;
            Color32 sleeve = a.OutfitId == "jerkin" ? Mix(a.OutfitColor, new Rgb(240, 235, 220), 0.6) : outfit;
            FillRect(buf, cx - bw, 14, cx + bw - 1, 29, outfit);
            if (hem < 14)
            {
                for (int y = hem; y < 14; y++)
                {
                    int flare = (a.OutfitId == "dress" || a.OutfitId == "robe") ? (14 - y) / 3 : 0;
                    FillRect(buf, cx - bw - flare, y, cx + bw - 1 + flare, y, outfit);
                }
            }
            if (a.OutfitId == "jerkin")
                FillRect(buf, cx - bw, 14, cx + bw - 1, 29, trim); // yelek gövdesi koyu, kollar açık
            if (a.OutfitId == "jerkin") FillRect(buf, cx - bw + 1, 15, cx + bw - 2, 28, outfit);
            if (a.OutfitId == "apron") FillRect(buf, cx - 3, 8, cx + 2, 24, trim);

            // Kemer (uzun kıyafetlerde de bel çizgisi).
            if (a.OutfitId != "robe") FillRect(buf, cx - bw, 19, cx + bw - 1, 20, trim);

            // Kollar ve eller.
            FillRect(buf, cx - bw - 2, 16, cx - bw - 1, 28, sleeve);
            FillRect(buf, cx + bw, 16, cx + bw + 1, 28, sleeve);
            FillRect(buf, cx - bw - 2, 14, cx - bw - 1, 15, skin);
            FillRect(buf, cx + bw, 14, cx + bw + 1, 15, skin);

            // Boyun ve baş.
            FillRect(buf, cx - 1, 30, cx, 32, skin);
            Ellipse(buf, cx, 37f, 5f, 6f, skin);

            // Yüz işareti.
            Color32 darkSkin = Mix(a.SkinColor, new Rgb(60, 30, 20), 0.35);
            switch (a.FaceMarkId)
            {
                case "freckles":
                    foreach (int x in new[] { 9, 11, 13, 15 }) Px(buf, x, 35, darkSkin);
                    foreach (int x in new[] { 10, 14 }) Px(buf, x, 34, darkSkin);
                    break;
                case "scar":
                    Px(buf, 14, 38, new Color32(160, 70, 70, 255));
                    Px(buf, 15, 37, new Color32(160, 70, 70, 255));
                    Px(buf, 16, 36, new Color32(160, 70, 70, 255));
                    break;
                case "mole":
                    Px(buf, 15, 34, new Color32(70, 40, 30, 255));
                    break;
                case "rosy":
                    foreach (int x in new[] { 8, 9, 15, 16 }) Px(buf, x, 35, new Color32(230, 120, 120, 255));
                    break;
            }

            // Gözler.
            Color32 eye = C(a.EyeColor);
            Px(buf, 10, 37, eye);
            Px(buf, 14, 37, eye);
            Px(buf, 10, 38, new Color32(250, 250, 250, 255));
            Px(buf, 14, 38, new Color32(250, 250, 250, 255));

            // Sakal ve bıyık.
            Color32 fh = C(a.FacialHairColor);
            switch (a.FacialHairId)
            {
                case "stubble":
                    FillRect(buf, 8, 31, 15, 34, Mix(a.SkinColor, a.FacialHairColor, 0.3));
                    break;
                case "mustache":
                    FillRect(buf, 9, 34, 14, 35, fh);
                    break;
                case "goatee":
                    FillRect(buf, 11, 30, 12, 33, fh);
                    FillRect(buf, 10, 34, 13, 34, fh);
                    break;
                case "beard":
                    FillRect(buf, 8, 30, 15, 34, fh);
                    FillRect(buf, 10, 35, 13, 35, fh);
                    break;
                case "full_beard":
                    FillRect(buf, 7, 28, 16, 34, fh);
                    FillRect(buf, 8, 35, 15, 36, fh);
                    Px(buf, 11, 34, skin);
                    Px(buf, 12, 34, skin);
                    break;
            }

            // Saç (kapüşonda yalnızca kâkül; başlıklar saçın üstünü örter).
            if (!hood) DrawHair(buf, a.HairStyleId, hair, a.HatId);
            else FillRect(buf, 8, 42, 15, 43, hat);

            // Başlıklar.
            switch (a.HatId)
            {
                case "cap":
                    Ellipse(buf, cx, 41.5f, 5.5f, 3.5f, hat);
                    break;
                case "straw":
                    Ellipse(buf, cx, 42f, 9.5f, 2.2f, hat);
                    FillRect(buf, 9, 43, 14, 46, hat);
                    FillRect(buf, 9, 43, 14, 43, Mix(a.HatColor, new Rgb(0, 0, 0), 0.3));
                    break;
                case "kerchief":
                    FillRect(buf, 7, 40, 16, 44, hat);
                    FillRect(buf, 16, 38, 17, 40, hat);
                    break;
            }
        }

        private static void DrawHair(Color32[] buf, string style, Color32 hair, string hatId)
        {
            Color32 dark = new Color32((byte)(hair.r * 0.75f), (byte)(hair.g * 0.75f), (byte)(hair.b * 0.75f), 255);
            switch (style)
            {
                case "bald":
                    return;
                case "crop":
                    FillRect(buf, 8, 42, 15, 43, hair);
                    break;
                case "short":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    FillRect(buf, 7, 38, 7, 40, hair);
                    FillRect(buf, 16, 38, 16, 40, hair);
                    break;
                case "shaggy":
                    Ellipse(buf, 12f, 41.5f, 6.5f, 4.2f, hair);
                    FillRect(buf, 6, 36, 7, 40, hair);
                    FillRect(buf, 16, 36, 17, 40, hair);
                    Px(buf, 9, 40, dark);
                    Px(buf, 13, 40, dark);
                    Px(buf, 15, 41, dark);
                    Px(buf, 10, 45, hair);
                    Px(buf, 14, 45, hair);
                    break;
                case "long":
                    Ellipse(buf, 12f, 41f, 6.2f, 4f, hair);
                    FillRect(buf, 6, 28, 7, 41, hair);
                    FillRect(buf, 16, 28, 17, 41, hair);
                    FillRect(buf, 7, 28, 16, 30, dark);
                    break;
                case "ponytail":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    FillRect(buf, 7, 38, 7, 40, hair);
                    FillRect(buf, 16, 38, 16, 40, hair);
                    FillRect(buf, 17, 30, 18, 40, hair);
                    FillRect(buf, 18, 28, 18, 31, dark);
                    break;
                case "braid":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    for (int y = 25; y <= 40; y++)
                    {
                        var c = (y % 2 == 0) ? hair : dark;
                        FillRect(buf, 6, y, 7, y, c);
                        FillRect(buf, 16, y, 17, y, c);
                    }
                    break;
                case "bun":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    FillRect(buf, 7, 38, 7, 40, hair);
                    FillRect(buf, 16, 38, 16, 40, hair);
                    Ellipse(buf, 12f, 45.5f, 2.6f, 2.2f, hair);
                    break;
                default:
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    break;
            }
        }
    }
}
