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
        public static Sprite Get(Appearance a, LookState look = null)
        {
            string key = a.Key() + LookKey(look);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = Create(a, look);
            Cache[key] = sprite;
            return sprite;
        }

        // Kırlaşma 10 adımda önbelleğe girer (her gün yeni doku üretmesin).
        private static string LookKey(LookState l) =>
            l == null ? "" : $"#g{Mathf.RoundToInt((float)l.Gray * 10)}w{(l.Wrinkles ? 1 : 0)}r{(l.Receding ? 1 : 0)}b{l.BandageZone}";

        /// <summary>Önbelleksiz yeni sprite (yaratma ekranı önizlemesi: çağıran yok eder).</summary>
        public static Sprite Create(Appearance a, LookState look = null)
        {
            var buf = new Color32[W * H];
            Draw(buf, a, look);
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

        private static void Draw(Color32[] buf, Appearance a, LookState look)
        {
            for (int i = 0; i < buf.Length; i++) buf[i] = Clear;

            double gray = look?.Gray ?? 0;
            Color32 skin = C(a.SkinColor);
            Color32 outfit = C(a.OutfitColor);
            Color32 trim = C(a.TrimColor);
            Color32 hair = C(AgingLook.GrayedHair(a.HairColor, gray));
            Color32 hat = C(a.HatColor);
            Color32 shoes = a.ShoesId == "clogs" ? new Color32(150, 110, 70, 255) : new Color32(45, 34, 28, 255);
            var visible = MarkRules.Visible(a.Marks);

            int bw = a.Build == BodyBuild.Slim ? 4 : a.Build == BodyBuild.Sturdy || a.Build == BodyBuild.Plump ? 6 : 5;
            if (a.Frame == BodyFrame.Feminine && bw > 4) bw--;   // dar omuz
            const int cx = 12;

            // Kapüşon: baş ve omuzların arkasındaki katman.
            bool hood = a.HatId == "hood";
            if (hood)
            {
                Ellipse(buf, cx, 38f, 7.5f, 8.5f, hat);
                FillRect(buf, cx - bw - 1, 26, cx + bw, 31, hat);
            }

            // Bacaklar (alt giysi) ve ayakkabılar.
            FillRect(buf, cx - 3, 2, cx - 1, 13, a.BottomId == "skirt" ? skin : trim);
            FillRect(buf, cx + 1, 2, cx + 3, 13, a.BottomId == "skirt" ? skin : trim);
            if (a.BottomId == "skirt") for (int y = 4; y < 14; y++) FillRect(buf, cx - 4 - (14 - y) / 4, y, cx + 3 + (14 - y) / 4, y, trim);
            if (a.BottomId == "breeches") { FillRect(buf, cx - 3, 2, cx - 1, 6, skin); FillRect(buf, cx + 1, 2, cx + 3, 6, skin); }
            if (a.ShoesId == "sandals") { FillRect(buf, cx - 4, 0, cx - 1, 0, shoes); FillRect(buf, cx + 1, 0, cx + 4, 0, shoes); }
            else
            {
                int top = a.ShoesId == "boots" ? 3 : 1;
                FillRect(buf, cx - 4, 0, cx - 1, top, shoes);
                FillRect(buf, cx + 1, 0, cx + 4, top, shoes);
            }

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

            // Dış giysi: omuzdan sarkan pelerin ya da şal.
            if (a.OuterId != Appearance.None)
            {
                Color32 outer = C(a.OuterColor);
                int bottom = a.OuterId == "cloak" ? 8 : a.OuterId == "shawl" ? 22 : 18;
                FillRect(buf, cx - bw - 2, bottom, cx - bw - 1, 29, outer);
                FillRect(buf, cx + bw, bottom, cx + bw + 1, 29, outer);
                FillRect(buf, cx - bw, 27, cx + bw - 1, 29, outer);
                if (a.OuterId == "fur") FillRect(buf, cx - bw - 1, 29, cx + bw, 30, Mix(a.OuterColor, new Rgb(235, 225, 205), 0.5));
            }

            // Boyun ve baş.
            FillRect(buf, cx - 1, 30, cx, 32, skin);
            float faceRx = a.FaceShapeId == "round" || a.FaceShapeId == "square" ? 5.5f : a.FaceShapeId == "long" || a.FaceShapeId == "heart" ? 4.5f : 5f;
            Ellipse(buf, cx, 37f, faceRx, 6f, skin);

            // Kol dövmeleri ve izleri (yüz dışı bölgeler).
            foreach (var m in visible) DrawBodyMark(buf, m, cx, bw);

            // Yüz işaretleri.
            foreach (var m in visible) DrawFaceMark(buf, m, a.SkinColor);
            if (look != null && look.Wrinkles)
            {
                var line = Mix(a.SkinColor, new Rgb(60, 30, 20), 0.25);
                Px(buf, 8, 36, line); Px(buf, 16, 36, line); Px(buf, 11, 40, line); Px(buf, 12, 40, line);
            }

            // Kaşlar.
            Color32 brow = C(AgingLook.GrayedHair(a.EffectiveBrowColor, gray));
            int browW = a.BrowId == "thick" || a.BrowId == "bushy" ? 3 : 2;
            int browY = a.BrowId == "arched" ? 40 : 39;
            FillRect(buf, 11 - browW, browY, 10, browY, brow);
            FillRect(buf, 14, browY, 13 + browW, browY, brow);

            // Gözler (iki renk: sol ve sağ).
            Color32 eyeL = C(a.EyeColor);
            Color32 eyeR = C(a.EyeColorRight ?? a.EyeColor);
            Px(buf, 10, 37, eyeL);
            Px(buf, 14, 37, eyeR);
            Px(buf, 10, 38, new Color32(250, 250, 250, 255));
            Px(buf, 14, 38, new Color32(250, 250, 250, 255));

            // Sakal ve bıyık (ayrı katman).
            Color32 fh = C(AgingLook.GrayedHair(a.FacialHairColor, gray));
            switch (a.BeardId)
            {
                case "stubble":
                    FillRect(buf, 8, 31, 15, 34, Mix(a.SkinColor, a.FacialHairColor, 0.3));
                    break;
                case "goatee":
                case "chin":
                    FillRect(buf, 11, 30, 12, 33, fh);
                    if (a.BeardId == "goatee") FillRect(buf, 10, 34, 13, 34, fh);
                    break;
                case "beard":
                    FillRect(buf, 8, 30, 15, 34, fh);
                    FillRect(buf, 10, 35, 13, 35, fh);
                    break;
                case "full_beard":
                case "forked":
                    FillRect(buf, 7, 28, 16, 34, fh);
                    FillRect(buf, 8, 35, 15, 36, fh);
                    Px(buf, 11, 34, skin);
                    Px(buf, 12, 34, skin);
                    if (a.BeardId == "forked") { Px(buf, 11, 28, Clear); Px(buf, 12, 28, Clear); FillRect(buf, 10, 26, 10, 27, fh); FillRect(buf, 13, 26, 13, 27, fh); }
                    break;
            }
            switch (a.MustacheId)
            {
                case "mustache": FillRect(buf, 9, 34, 14, 35, fh); break;
                case "thin": FillRect(buf, 10, 35, 13, 35, fh); break;
                case "handlebar": FillRect(buf, 9, 35, 14, 35, fh); Px(buf, 8, 36, fh); Px(buf, 15, 36, fh); break;
                case "walrus": FillRect(buf, 8, 33, 15, 35, fh); break;
                case "drooping": FillRect(buf, 9, 35, 14, 35, fh); FillRect(buf, 9, 32, 9, 34, fh); FillRect(buf, 14, 32, 14, 34, fh); break;
            }

            // Takılar (kulak, burun, boyun).
            foreach (var m in visible)
            {
                if (m.Kind != MarkKind.Jewelry) continue;
                var gold = new Color32(222, 186, 84, 255);
                if (m.Zone == "ear") { Px(buf, 7, 36, gold); if (m.Id == "earrings" || m.Id == "hoops") Px(buf, 17, 36, gold); if (m.Id == "drop_earring" || m.Id == "hoops") Px(buf, 7, 35, gold); }
                else if (m.Zone == "nose") Px(buf, 13, 36, gold);
                else if (m.Zone == "neck") FillRect(buf, cx - 2, 29, cx + 1, 29, gold);
            }

            // Saç (kapüşonda yalnızca kâkül; başlıklar saçın üstünü örter).
            if (!hood)
            {
                DrawHair(buf, a.HairStyleId, hair, a.HatId);
                if (look != null && look.Receding) FillRect(buf, 9, 42, 14, 43, skin);
                DrawBangs(buf, a.BangsId, hair);
            }
            else FillRect(buf, 8, 42, 15, 43, hat);

            // Taze yara sargısı.
            if (!string.IsNullOrEmpty(look?.BandageZone)) DrawBandage(buf, look.BandageZone, cx, bw);

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
                case "coif":
                    FillRect(buf, 7, 40, 16, 44, hat);
                    if (a.HatId == "kerchief") FillRect(buf, 16, 38, 17, 40, hat);
                    else { FillRect(buf, 7, 34, 7, 40, hat); FillRect(buf, 16, 34, 16, 40, hat); }
                    break;
                case "felt":
                    Ellipse(buf, cx, 42f, 7.5f, 1.8f, hat);
                    FillRect(buf, 9, 43, 15, 46, hat);
                    break;
                case "wreath":
                    for (int x = 7; x <= 16; x += 2) Px(buf, x, 43, hat);
                    for (int x = 8; x <= 15; x += 2) Px(buf, x, 44, Mix(a.HatColor, new Rgb(250, 240, 120), 0.4));
                    break;
            }
        }

        private static Color32 ScarColor(Mark m, Rgb skin)
        {
            // Taze iz pembe, solmuş ten rengine yakın, eski açık çizgi.
            var tint = m.Fade == 0 ? new Rgb(190, 80, 80) : m.Fade == 1 ? new Rgb(200, 130, 120) : new Rgb(235, 215, 200);
            return Mix(skin, tint, m.Fade == 2 ? 0.45 : 0.7);
        }

        private static void DrawFaceMark(Color32[] buf, Mark m, Rgb skin)
        {
            Color32 dark = Mix(skin, new Rgb(60, 30, 20), 0.35);
            switch (m.Kind)
            {
                case MarkKind.SkinDetail:
                    if (m.Zone != "face" && !(m.Id.Contains("freckles") || m.Id == "vitiligo" || m.Id == "birthmark")) return;
                    switch (m.Id)
                    {
                        case "freckles": case "light_freckles": case "heavy_freckles":
                            foreach (int x in new[] { 9, 11, 13, 15 }) Px(buf, x, 35, dark);
                            if (m.Id != "light_freckles") foreach (int x in new[] { 10, 14 }) Px(buf, x, 34, dark);
                            break;
                        case "mole": Px(buf, 15, 34, new Color32(70, 40, 30, 255)); break;
                        case "rosy": foreach (int x in new[] { 8, 9, 15, 16 }) Px(buf, x, 35, new Color32(230, 120, 120, 255)); break;
                        case "birthmark": FillRect(buf, 8, 37, 9, 38, Mix(skin, new Rgb(150, 70, 60), 0.4)); break;
                        case "vitiligo": FillRect(buf, 14, 38, 16, 40, Mix(skin, new Rgb(250, 238, 228), 0.7)); break;
                        case "pox": Px(buf, 9, 34, dark); Px(buf, 15, 39, dark); Px(buf, 12, 33, dark); break;
                    }
                    break;
                case MarkKind.Scar:
                    var c = ScarColor(m, skin);
                    switch (m.Zone)
                    {
                        case "cheek": Px(buf, 14, 35, c); Px(buf, 15, 36, c); Px(buf, 16, 37, c); break;
                        case "brow": Px(buf, 15, 40, c); Px(buf, 15, 39, c); break;
                        case "forehead": FillRect(buf, 10, 41, 13, 41, c); break;
                        case "lip": Px(buf, 12, 33, c); Px(buf, 12, 34, c); break;
                        case "neck": FillRect(buf, 11, 31, 12, 31, c); break;
                    }
                    break;
                case MarkKind.Tattoo:
                    if (m.Zone == "face") { Px(buf, 9, 35, C(m.Color)); Px(buf, 9, 34, C(m.Color)); }
                    else if (m.Zone == "neck") Px(buf, 12, 31, C(m.Color));
                    break;
                case MarkKind.Makeup:
                    var mc = C(m.Color);
                    if (m.Id == "kohl") { Px(buf, 9, 37, mc); Px(buf, 15, 37, mc); }
                    else if (m.Id == "blush") { Px(buf, 9, 35, mc); Px(buf, 15, 35, mc); }
                    else if (m.Id == "lip") FillRect(buf, 11, 34, 13, 34, mc);
                    break;
            }
        }

        private static void DrawBodyMark(Color32[] buf, Mark m, int cx, int bw)
        {
            if (m.Kind != MarkKind.Scar && m.Kind != MarkKind.Tattoo && m.Kind != MarkKind.SkinDetail) return;
            Color32 c = m.Kind == MarkKind.Tattoo ? C(m.Color) : new Color32(185, 110, 100, 255);
            switch (m.Zone)
            {
                case "larm": Px(buf, cx - bw - 2, 24, c); Px(buf, cx - bw - 1, 23, c); break;
                case "rarm": Px(buf, cx + bw, 24, c); Px(buf, cx + bw + 1, 23, c); break;
                case "hand": Px(buf, cx + bw + 1, 14, c); break;
                case "chest": Px(buf, cx - 1, 26, c); Px(buf, cx, 26, c); break;
            }
        }

        private static void DrawBandage(Color32[] buf, string zone, int cx, int bw)
        {
            var linen = new Color32(236, 228, 210, 255);
            switch (zone)
            {
                case "hand": FillRect(buf, cx + bw, 14, cx + bw + 1, 16, linen); break;
                case "larm": FillRect(buf, cx - bw - 2, 21, cx - bw - 1, 24, linen); break;
                case "rarm": FillRect(buf, cx + bw, 21, cx + bw + 1, 24, linen); break;
                case "cheek": FillRect(buf, 14, 35, 16, 37, linen); break;
                default: FillRect(buf, 7, 40, 16, 41, linen); break;   // alın, kaş, dudak: baş sargısı
            }
        }

        private static void DrawBangs(Color32[] buf, string bangs, Color32 hair)
        {
            switch (bangs)
            {
                case "straight": FillRect(buf, 8, 40, 15, 41, hair); break;
                case "side": FillRect(buf, 8, 40, 12, 41, hair); Px(buf, 8, 39, hair); break;
                case "wispy": for (int x = 8; x <= 15; x += 2) Px(buf, x, 40, hair); break;
                case "curly": for (int x = 8; x <= 15; x++) Px(buf, x, 40 + (x % 2), hair); break;
                case "parted": FillRect(buf, 8, 40, 10, 41, hair); FillRect(buf, 13, 40, 15, 41, hair); break;
                case "long": FillRect(buf, 8, 39, 15, 41, hair); break;
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
                case "long_wavy":
                case "long_curly":
                case "shoulder":
                    Ellipse(buf, 12f, 41f, 6.4f, 4.2f, hair);
                    int bottom = style == "shoulder" ? 31 : 27;
                    for (int y = bottom; y <= 41; y++)
                    {
                        int wob = style == "long_wavy" ? (y / 2) % 2 : style == "long_curly" ? (y % 2) : 0;
                        FillRect(buf, 5 + wob, y, 7, y, hair);
                        FillRect(buf, 16, y, 18 - wob, y, hair);
                    }
                    break;
                case "curls":
                case "afro":
                    Ellipse(buf, 12f, 41.5f, style == "afro" ? 8.5f : 7f, style == "afro" ? 6f : 4.6f, hair);
                    for (int x = 6; x <= 17; x += 3) Px(buf, x, 44, dark);
                    break;
                case "low_tail":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    FillRect(buf, 16, 26, 17, 38, hair);
                    break;
                case "twin_braids":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    for (int y = 25; y <= 38; y++) { var c2 = (y % 2 == 0) ? hair : dark; Px(buf, 6, y, c2); Px(buf, 17, y, c2); }
                    break;
                case "crown_braid":
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    for (int x = 7; x <= 16; x++) Px(buf, x, 43, (x % 2 == 0) ? hair : dark);
                    break;
                case "top_knot":
                    FillRect(buf, 8, 42, 15, 43, hair);
                    Ellipse(buf, 12f, 45.5f, 2f, 2f, hair);
                    break;
                case "monk":
                    FillRect(buf, 7, 38, 8, 41, hair);
                    FillRect(buf, 15, 38, 16, 41, hair);
                    break;
                case "side_part":
                case "tousled":
                default:
                    Ellipse(buf, 12f, 41f, 6f, 3.8f, hair);
                    FillRect(buf, 7, 38, 7, 40, hair);
                    FillRect(buf, 16, 38, 16, 40, hair);
                    if (style == "side_part") Px(buf, 10, 44, dark);
                    if (style == "tousled") { Px(buf, 9, 45, hair); Px(buf, 14, 45, hair); }
                    break;
            }
        }
    }
}
