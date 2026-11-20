using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Customization.Layers
{
    /// <summary>
    /// Bir görünümün katman çizimi için hazır hâli: rampalar, desenler ve parça kimlikleri (belge 43 §4.2).
    /// Appearance'tan bir kez kurulur; kareler arasında paylaşılır.
    /// </summary>
    public sealed class LayerLook
    {
        public Rgb[] Skin, Main, Trim, Bottom, Leather, Iris, IrisRight, Hair, Brow, Facial, Hat, Ornament;
        public string EyePattern = FacePatterns.DefaultEye;
        public string BrowPattern = FacePatterns.DefaultBrow;
        public string ClothPattern = "plain";
        public int Stubble;
        public string Mass, Front, Extra, OrnamentId, Mustache, Beard, Sideburns, HatId;
        /// <summary>Başlığın saçı örtmesi: "full" saç parçaları çizilmez, "top"/"none" çizilir.</summary>
        public string HatCover = "none";

        /// <summary>
        /// Görünümden. grey: 0..1 kırlaşma (AgingLook); saç ve sakal rengi griye karışır.
        /// Katalog verilirse göz/kaş deseni ve başlık örtüsü Tag'den okunur.
        /// </summary>
        public static LayerLook From(Appearance a, AppearanceCatalog catalog = null, double grey = 0.0)
        {
            var hair = AgingLook.GrayedHair(a.HairColor, grey);
            var facial = AgingLook.GrayedHair(a.FacialHairColor, grey);
            var look = new LayerLook
            {
                Skin = RampMath.Make(a.SkinColor),
                Main = RampMath.Make(a.OutfitColor),
                Trim = RampMath.Make(a.TrimColor),
                Bottom = RampMath.Make(a.TrimColor),
                Leather = RampMath.Make(KeyColors.Leather),
                Iris = RampMath.Make(a.EyeColor),
                IrisRight = RampMath.Make(a.EyeColorRight ?? a.EyeColor),
                Hair = RampMath.Make(hair),
                Brow = RampMath.Make(a.BrowFollowsHair ? hair : a.BrowColor),
                Facial = RampMath.Make(facial),
                Hat = RampMath.Make(a.HatColor),
                Ornament = RampMath.Make(a.HairOrnamentColor),
                ClothPattern = a.PatternId ?? "plain",
                Stubble = a.StubbleLevel,
                Mass = a.HairStyleId, Front = a.BangsId, Extra = a.HairExtraId, OrnamentId = a.HairOrnamentId,
                Mustache = a.MustacheId, Beard = a.BeardId, Sideburns = a.SideburnsId, HatId = a.HatId
            };
            if (catalog != null)
            {
                look.EyePattern = TagOf(catalog.EyeShapes, a.EyeShapeId) ?? FacePatterns.DefaultEye;
                look.BrowPattern = TagOf(catalog.Brows, a.BrowId) ?? FacePatterns.DefaultBrow;
                look.HatCover = TagOf(catalog.Hats, a.HatId) ?? "top";
            }
            return look;
        }

        private static string TagOf(List<OptionDef> list, string id)
        {
            foreach (var o in list) if (o.Id == id) return o.Tag;
            return null;
        }
    }

    /// <summary>
    /// Katmanlı karakter karesi (belge 43 §4.2): mankenin bölge haritası + rampa takası + göz/kaş damgası +
    /// baş çapasına oturan baş parçaları. Saf C#: Unity dokusu Game katmanında çevrilir, burada test edilir.
    ///
    /// Sıra: arka parçalar (saç kütlesi, eklenti, süs, başlık arkası) → gövde → yüz (göz, kaş, kirli sakal) →
    /// ön parçalar (favori, sakal, bıyık, saç kütlesi önü, saç önü, eklenti, süs, başlık).
    /// </summary>
    public static class LayerComposer
    {
        /// <summary>Parçada rampa tonu sayılan en büyük uzaklık; daha uzak pikseller (metal, çiçek) olduğu gibi kalır.</summary>
        public const double RecolorTolerance = 28.0;
        private const double OutlineTolerance = 12.0;
        private static readonly Rgb[] RefHair = RampMath.Make(KeyColors.ReferenceHair);
        private static readonly Rgb[] RefCloth = RampMath.Make(KeyColors.ReferenceCloth);

        public static PixelImage Compose(RegionFrame body, FrameAnchor anchor, string direction, LayerLook look, IPartSource parts)
        {
            var img = new PixelImage(body.Width, body.Height);
            bool hairHidden = look.HatCover == "full" && !IsNone(look.HatId);

            // 1. Arka parçalar (gövdenin arkasında kalır).
            if (!hairHidden)
            {
                Part(img, parts, PartSlot.Mass, look.Mass, direction, true, anchor, RefHair, look.Hair);
                Part(img, parts, PartSlot.Extra, look.Extra, direction, true, anchor, RefHair, look.Hair);
                Part(img, parts, PartSlot.Ornament, look.OrnamentId, direction, true, anchor, RefCloth, look.Ornament);
            }
            Part(img, parts, PartSlot.Hat, look.HatId, direction, true, anchor, RefCloth, look.Hat);

            // 2. Gövde.
            DrawBody(img, body, anchor, look);

            // 3. Yüz.
            if (PartDirections.FaceVisible(direction))
            {
                DrawStubble(img, body, anchor, look);
                DrawEyesAndBrows(img, body, anchor, direction, look);
            }

            // 4. Ön parçalar.
            Part(img, parts, PartSlot.Sideburns, look.Sideburns, direction, false, anchor, RefHair, look.Facial);
            Part(img, parts, PartSlot.Beard, look.Beard, direction, false, anchor, RefHair, look.Facial);
            Part(img, parts, PartSlot.Mustache, look.Mustache, direction, false, anchor, RefHair, look.Facial);
            if (!hairHidden)
            {
                Part(img, parts, PartSlot.Mass, look.Mass, direction, false, anchor, RefHair, look.Hair);
                Part(img, parts, PartSlot.Front, look.Front, direction, false, anchor, RefHair, look.Hair);
                Part(img, parts, PartSlot.Extra, look.Extra, direction, false, anchor, RefHair, look.Hair);
                Part(img, parts, PartSlot.Ornament, look.OrnamentId, direction, false, anchor, RefCloth, look.Ornament);
            }
            Part(img, parts, PartSlot.Hat, look.HatId, direction, false, anchor, RefCloth, look.Hat);
            return img;
        }

        private static bool IsNone(string id) => string.IsNullOrEmpty(id) || id == Appearance.None;

        // ---------------- gövde ----------------

        private static void DrawBody(PixelImage img, RegionFrame body, FrameAnchor anchor, LayerLook look)
        {
            var outline = new Px32(KeyColors.Outline);
            for (int y = 0; y < body.Height; y++)
            for (int x = 0; x < body.Width; x++)
            {
                int i = y * body.Width + x;
                byte r = body.Region[i];
                if (r == Region.Empty) continue;
                int shade = body.Shade[i];
                Rgb c;
                switch (r)
                {
                    case Region.Outline: img.Px[i] = outline; continue;
                    case Region.Main: c = look.Main[PatternShade(look.ClothPattern, x - anchor.HeadX, y - anchor.HeadY, shade)]; break;
                    case Region.Trim: c = look.Trim[shade]; break;
                    case Region.Bottom: c = look.Bottom[Math.Max(0, shade - 1)]; break;
                    case Region.Shoes:
                    case Region.Belt: c = look.Leather[shade]; break;
                    // Mankenin kendi gözü ve kaşı tenle örtülür; yerine desen basılır (DrawEyesAndBrows).
                    case Region.EyeWhite:
                    case Region.Iris:
                    case Region.Brow: c = look.Skin[RampMath.BaseShade]; break;
                    default: c = look.Skin[shade]; break;
                }
                if (r == Region.Main && look.ClothPattern == "embroidered" && Embroidered(body, x, y))
                    c = look.Trim[3];
                img.Px[i] = new Px32(c);
            }
        }

        /// <summary>Desen gövde çapasına bağlıdır, yürürken kaymaz (belge 43 §4.2).</summary>
        private static int PatternShade(string pattern, int rx, int ry, int shade)
        {
            int ux = rx + 1000, uy = ry + 1000;   // negatifte de düzgün mod
            switch (pattern)
            {
                case "striped": return (uy / 2) % 2 == 1 ? Math.Max(0, shade - 1) : shade;
                case "checked": return ((uy / 2) + (ux / 2)) % 2 == 1 ? Math.Max(0, shade - 1) : shade;
                default: return shade;
            }
        }

        /// <summary>İşleme: üst giysinin üst kenarındaki (yaka) 2 satırda seyrek nakış.</summary>
        private static bool Embroidered(RegionFrame body, int x, int y)
        {
            if ((x + y) % 2 != 0) return false;
            return body.RegionAt(x, y - 1) != Region.Main || body.RegionAt(x, y - 2) != Region.Main;
        }

        // ---------------- yüz ----------------

        private static void DrawEyesAndBrows(PixelImage img, RegionFrame body, FrameAnchor anchor, string direction, LayerLook look)
        {
            if (anchor.Eyes.Count == 0) return;
            PartDirections.Drawn(direction, out bool mirrored);
            var eye = FacePatterns.Eye(look.EyePattern);
            var brow = FacePatterns.Brow(look.BrowPattern);
            for (int k = 0; k < anchor.Eyes.Count; k++)
            {
                var (ex, ey) = anchor.Eyes[k];
                // İki gözde sağdaki aynalanır; tek gözde (yandan) yöne göre.
                bool flip = anchor.Eyes.Count == 2 ? k == 1 : mirrored;
                var iris = anchor.Eyes.Count == 2 && k == 1 ? look.IrisRight : look.Iris;
                Stamp(img, body, eye, ex - FacePatterns.EyeOriginX, ey - FacePatterns.EyeOriginY, flip, look, iris);
                int bx = flip ? ex - (brow[0].Length - 1) - FacePatterns.BrowOffsetX : ex + FacePatterns.BrowOffsetX;
                Stamp(img, body, brow, bx, ey + FacePatterns.BrowOffsetY, flip, look, iris);
            }
            if (look.BrowPattern == "unibrow" && anchor.Eyes.Count == 2)
            {
                int y = anchor.Eyes[0].y + FacePatterns.BrowOffsetY;
                for (int x = anchor.Eyes[0].x + 1; x < anchor.Eyes[1].x; x++)
                    if (body.RegionAt(x, y) != Region.Empty && body.RegionAt(x, y) != Region.Outline) img.Set(x, y, new Px32(look.Brow[0]));
            }
        }

        private static void Stamp(PixelImage img, RegionFrame body, string[] pattern, int left, int top, bool flip, LayerLook look, Rgb[] iris)
        {
            for (int row = 0; row < pattern.Length; row++)
            {
                string line = pattern[row];
                for (int col = 0; col < line.Length; col++)
                {
                    char ch = flip ? line[line.Length - 1 - col] : line[col];
                    if (ch == '.') continue;
                    int x = left + col, y = top + row;
                    byte r = body.RegionAt(x, y);
                    if (r == Region.Empty || r == Region.Outline) continue;   // desen yüzün dışına taşmaz
                    Rgb c;
                    switch (ch)
                    {
                        case 'w': c = new Rgb(240, 236, 228); break;
                        case 'i': c = iris[1]; break;
                        case 'd': c = KeyColors.Outline; break;
                        case 's': c = look.Skin[1]; break;
                        case 'b': c = look.Brow[0]; break;
                        default: continue;
                    }
                    img.Set(x, y, new Px32(c));
                }
            }
        }

        /// <summary>Kirli sakal: göz hizasının 3-8 satır altındaki ten, düzeye göre seyrek ya da sık koyulaşır.</summary>
        private static void DrawStubble(PixelImage img, RegionFrame body, FrameAnchor anchor, LayerLook look)
        {
            if (look.Stubble <= 0 || anchor.Eyes.Count == 0) return;
            int eyeY = anchor.Eyes[0].y;
            int left = int.MaxValue, right = int.MinValue;
            foreach (var (x, _) in anchor.Eyes) { left = Math.Min(left, x); right = Math.Max(right, x); }
            left -= 3; right += 3;
            double t = 0.18 * look.Stubble;
            for (int y = eyeY + 3; y <= eyeY + 8; y++)
            for (int x = left; x <= right; x++)
            {
                if (body.RegionAt(x, y) != Region.Skin) continue;
                if (look.Stubble == 1 && (x + y) % 2 != 0) continue;
                var cur = img.Get(x, y).Rgb;
                img.Set(x, y, new Px32(Rgb.Lerp(cur, look.Facial[0], t)));
            }
        }

        // ---------------- baş parçaları ----------------

        private static void Part(PixelImage img, IPartSource parts, string slot, string id, string direction, bool back,
                                 FrameAnchor anchor, Rgb[] reference, Rgb[] target)
        {
            if (parts == null || IsNone(id)) return;
            string drawn = PartDirections.Drawn(direction, out bool mirror);
            var src = parts.Get(slot, id, drawn, back);
            if (src == null) return;
            int ox = anchor.HeadX - PartCanvas.AnchorX, oy = anchor.HeadY - PartCanvas.AnchorY;
            for (int y = 0; y < src.Height; y++)
            for (int x = 0; x < src.Width; x++)
            {
                var p = src.Px[y * src.Width + x];
                if (!p.Opaque) continue;
                // Ayna çapa sütunu etrafında: tuval genişliği çift, çapa 32 → x' = 2*32 - 1 - x.
                int dx = mirror ? 2 * PartCanvas.AnchorX - 1 - x : x;
                img.Set(ox + dx, oy + y, new Px32(Recolor(p.Rgb, reference, target)));
            }
        }

        /// <summary>Referans rampada çizilmiş pikseli hedef rampaya çevirir; kontur ve uzak renkler (metal, çiçek) kalır.</summary>
        public static Rgb Recolor(Rgb c, Rgb[] reference, Rgb[] target)
        {
            if (RampMath.Distance(c, KeyColors.Outline) <= OutlineTolerance) return c;
            int shade = RampMath.NearestShade(reference, c, out double d);
            return d <= RecolorTolerance ? target[shade] : c;
        }
    }
}
