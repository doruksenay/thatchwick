using System;
using System.Collections.Generic;
using Thatchwick.Core.Customization;

namespace Thatchwick.Core.Customization.Layers
{
    /// <summary>
    /// Mankenin eklem çizgileri (belge 48 §4.1): omuz, dirsek, bilek, bel, diz, ayak bileği satırları. Bölge haritasının mavi
    /// kanalında işaret varsa oradan (3 omuz, 4 dirsek, 5 bilek, 6 bel, 7 diz, 8 ayak bileği), yoksa opak yüksekliğin oranından.
    /// Gövde açıklığı kalça hizasındaki alt giysinin genişliğidir; açıklığın dışındaki Main pikselleri koldur.
    /// </summary>
    public sealed class JointLines
    {
        public int Top, Bottom, Shoulder, Elbow, Wrist, Waist, Knee, Ankle;
        public int TorsoLeft, TorsoRight;

        public bool InTorso(int x) => x >= TorsoLeft && x <= TorsoRight;

        /// <summary>Oranlardan tahmin (P7 pilotunda manken.py gerçek çizgileri yazar).</summary>
        public static JointLines Estimate(RegionFrame f)
        {
            int top = -1, bottom = -1;
            for (int y = 0; y < f.Height; y++)
                for (int x = 0; x < f.Width; x++)
                    if (f.RegionAt(x, y) != Region.Empty) { if (top < 0) top = y; bottom = y; break; }
            if (top < 0) { top = 0; bottom = f.Height - 1; }
            int h = Math.Max(1, bottom - top);
            int L(double k) => top + (int)Math.Round(h * k);
            var j = new JointLines
            {
                Top = top, Bottom = bottom, Shoulder = L(0.24), Elbow = L(0.40), Wrist = L(0.54), Waist = L(0.50), Knee = L(0.76), Ankle = L(0.93)
            };
            // gövde açıklığı: kalça hizasında alt giysinin (bacakların) kapladığı genişlik; kollar gövdeye değse de dışında kalır
            int bestL = int.MaxValue, bestR = -1;
            for (int y = j.Waist + 1; y <= j.Waist + Math.Max(2, (j.Knee - j.Waist) / 2) && y < f.Height; y++)
                for (int x = 0; x < f.Width; x++)
                {
                    byte r = f.RegionAt(x, y);
                    if (r != Region.Bottom && r != Region.Belt) continue;
                    bestL = Math.Min(bestL, x); bestR = Math.Max(bestR, x);
                }
            if (bestR < 0) { bestL = f.Width / 3; bestR = f.Width * 2 / 3; }
            j.TorsoLeft = bestL; j.TorsoRight = bestR;
            return j;
        }
    }

    /// <summary>Çizilecek kesim (ClothingInstance'tan): kol, boy, çizme, önlük, kolsuz üst ve altındaki gömlek.</summary>
    public sealed class CutSpec
    {
        public string Sleeve = "long";      // none, short, elbow, long, rolled
        public string Length = "";          // hip, knee_above, knee_below (tunik); ankle, floor (aile 2: dokunulmaz)
        public string Boots = "";           // half, tall
        public bool Apron;
        /// <summary>Kolsuz üstün altında gömlek var: kollar Under bölgesine (gömleğin rengi) boyanır.</summary>
        public bool ShirtUnder;
        public int BellyPx;                 // gebelik karnı: 0-2 piksel
    }

    /// <summary>
    /// Kesim maskeleri ve doku betikleri (belge 48 §4.1, §13.1; 0 gen): bölge haritası eklem çizgileriyle alt bölgelere
    /// ayrılır, kesim hangi alt bölgenin hangi rampayla boyanacağını belirler. Ayrıca B katmanı çapaları (omuz, bel, el),
    /// gözlük için göz çapası, gebelik karnı, malzeme dokusu, kir ve yama damgası, envanter ikonu kırpma.
    /// </summary>
    public static class ClothingMasks
    {
        /// <summary>Kesimi uygular: yeni bir bölge haritası döner (manken değişmez).</summary>
        public static RegionFrame ApplyCuts(RegionFrame src, JointLines j, CutSpec cut)
        {
            var f = new RegionFrame(src.Width, src.Height);
            Array.Copy(src.Region, f.Region, src.Region.Length);
            Array.Copy(src.Shade, f.Shade, src.Shade.Length);
            if (cut == null) return f;
            int sleeveEnd;
            switch (cut.Sleeve)
            {
                case "none": sleeveEnd = j.Shoulder; break;
                case "short": sleeveEnd = j.Shoulder + (j.Elbow - j.Shoulder) / 2; break;
                case "elbow": sleeveEnd = j.Elbow; break;
                case "rolled": sleeveEnd = j.Elbow + (j.Wrist - j.Elbow) / 2; break;
                default: sleeveEnd = int.MaxValue; break;
            }
            int lengthLine = cut.Length == "hip" ? j.Waist + (j.Knee - j.Waist) / 4
                : cut.Length == "knee_above" ? j.Knee - 2 : cut.Length == "knee_below" ? j.Knee + 3 : -1;
            int bootLine = cut.Boots == "tall" ? j.Knee : cut.Boots == "half" ? j.Knee + (j.Ankle - j.Knee) / 2 : int.MaxValue;
            for (int y = 0; y < f.Height; y++)
            for (int x = 0; x < f.Width; x++)
            {
                int i = y * f.Width + x;
                byte r = f.Region[i];
                bool arm = !j.InTorso(x) && y < j.Wrist + 2;
                if (r == Region.Main && arm && y >= sleeveEnd)
                    f.Region[i] = cut.Sleeve == "none" && cut.ShirtUnder ? Region.Under : Region.Skin;
                if (lengthLine >= 0 && j.InTorso(x))
                {
                    if (r == Region.Main && y > lengthLine && y > j.Waist) f.Region[i] = Region.Bottom;
                    else if (r == Region.Bottom && y <= lengthLine && y > j.Waist) f.Region[i] = Region.Main;
                }
                if (r == Region.Bottom && y >= bootLine && cut.Boots.Length > 0) f.Region[i] = Region.Shoes;
                if (cut.Apron && j.InTorso(x) && y > j.Shoulder + 2 && y < j.Knee && (f.Region[i] == Region.Main || f.Region[i] == Region.Bottom))
                {
                    int w = j.TorsoRight - j.TorsoLeft;
                    if (x > j.TorsoLeft + w / 5 && x < j.TorsoRight - w / 5) f.Region[i] = Region.Apron;
                }
            }
            if (cut.BellyPx > 0) Belly(f, j, Math.Min(2, cut.BellyPx));
            return f;
        }

        /// <summary>Gebelik karnı (B1 betik yöntemi): bel ile kalça arasında gövde sağa doğru 1-2 piksel genişler, kontur kayar.</summary>
        public static void Belly(RegionFrame f, JointLines j, int px)
        {
            int from = j.Waist - 3, to = j.Waist + (j.Knee - j.Waist) / 3;
            for (int y = Math.Max(0, from); y <= Math.Min(f.Height - 1, to); y++)
            {
                int edge = -1;
                for (int x = f.Width - 1; x >= 0; x--) if (f.RegionAt(x, y) == Region.Outline && x > j.TorsoRight - 1) { edge = x; break; }
                if (edge < 0 || edge + px >= f.Width) continue;
                int i = y * f.Width + edge;
                for (int k = 0; k < px; k++) { f.Region[i + k] = Region.Main; f.Shade[i + k] = f.ShadeAt(edge - 1, y); }
                f.Region[i + px] = Region.Outline;
            }
        }

        // ---------------- çapalar (B katmanı) ----------------

        /// <summary>Omuz (pelerin), bel (kese, çanta) ve el (baston) çapaları; gözlük göz çapasını kullanır (FrameAnchor.Eyes).</summary>
        public static (int x, int y) Shoulder(JointLines j) => ((j.TorsoLeft + j.TorsoRight) / 2, j.Shoulder);
        public static (int x, int y) Waist(JointLines j) => ((j.TorsoLeft + j.TorsoRight) / 2, j.Waist);
        public static (int x, int y) Hand(RegionFrame f, JointLines j)
        {
            for (int x = f.Width - 1; x > j.TorsoRight; x--)
                for (int y = j.Wrist - 2; y <= j.Wrist + 3; y++)
                    if (f.RegionAt(x, y) == Region.Skin) return (x, y);
            return (j.TorsoRight + 2, j.Wrist);
        }

        /// <summary>Gözlük merkezi: görünen gözlerin ortası; göz yoksa (arkadan) null.</summary>
        public static (int x, int y)? Glasses(FrameAnchor a)
        {
            if (a == null || a.Eyes.Count == 0) return null;
            int sx = 0, sy = 0;
            foreach (var e in a.Eyes) { sx += e.x; sy += e.y; }
            return (sx / a.Eyes.Count, sy / a.Eyes.Count);
        }

        // ---------------- doku, kir, yama ----------------

        private static int Hash(int x, int y) { unchecked { int h = x * 73856093 ^ y * 19349663; return (h ^ (h >> 13)) & 0x7fffffff; } }

        /// <summary>Malzeme dokusu: bölge tonunu desenle oynatır (keçe pürüzü, örgü ilmeği, ipek parıltısı, kürk kenar tüyü).</summary>
        public static int MaterialShade(string material, int x, int y, int shade, bool edge)
        {
            switch (material)
            {
                case "felt": return Hash(x, y) % 5 == 0 ? Math.Max(0, shade - 1) : shade;
                case "knit": return (y % 2 == 0) == (x % 2 == 0) ? shade : Math.Max(0, shade - 1);
                case "silk": return shade >= 2 && Hash(x, y) % 3 == 0 ? Math.Min(RampMath.Size - 1, shade + 1) : shade;
                case "fur": return edge ? Math.Min(RampMath.Size - 1, shade + 1) : shade;
                default: return shade;
            }
        }

        /// <summary>Kir: kir oranı kadar giysi pikselini bir ton koyulaştırır (aşağıdan yukarı, desenli).</summary>
        public static int ApplyDirt(PixelImage img, RegionFrame f, float dirt01, Rgb[] mainRamp)
        {
            if (dirt01 <= 0f || mainRamp == null) return 0;
            int n = 0;
            for (int y = 0; y < f.Height; y++)
            for (int x = 0; x < f.Width; x++)
            {
                byte r = f.RegionAt(x, y);
                if (r != Region.Main && r != Region.Bottom && r != Region.Apron) continue;
                double bias = (double)y / Math.Max(1, f.Height);   // etek ucu daha kirli
                if (Hash(x, y) % 100 >= dirt01 * 60 * (0.5 + bias)) continue;
                var p = img.Get(x, y);
                if (p.A == 0) continue;
                var c = new Rgb(p.R, p.G, p.B);
                int s = RampMath.NearestShade(mainRamp, c, out _);
                var d = mainRamp[Math.Max(0, s - 1)];
                img.Set(x, y, new Px32(d));
                n++;
            }
            return n;
        }

        /// <summary>Yama damgası: gövdede 2x2 piksel, kumaşın rengiyle; yama sayısı kadar, sabit yerlerde.</summary>
        public static int StampPatches(PixelImage img, RegionFrame f, JointLines j, int patches, Rgb patchColor)
        {
            int placed = 0;
            for (int k = 0; k < patches && k < 4; k++)
            {
                int x = j.TorsoLeft + 1 + (k * 3) % Math.Max(1, j.TorsoRight - j.TorsoLeft - 2);
                int y = j.Shoulder + 4 + k * 5;
                bool ok = true;
                for (int dy = 0; dy < 2 && ok; dy++) for (int dx = 0; dx < 2 && ok; dx++) if (f.RegionAt(x + dx, y + dy) != Region.Main) ok = false;
                if (!ok) continue;
                for (int dy = 0; dy < 2; dy++) for (int dx = 0; dx < 2; dx++) img.Set(x + dx, y + dy, new Px32(patchColor));
                placed++;
            }
            return placed;
        }

        // ---------------- envanter ikonu ----------------

        /// <summary>Envanter ikonu: verilen bölgelerin kapladığı alanı kırpar ve kareye ortalar (katman çiziminden, 0 gen).</summary>
        public static PixelImage CropIcon(PixelImage composed, RegionFrame f, ICollection<byte> regions, int size, int pad = 1)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            for (int y = 0; y < f.Height; y++)
            for (int x = 0; x < f.Width; x++)
            {
                byte r = f.RegionAt(x, y);
                if (!regions.Contains(r)) continue;
                minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
            }
            var icon = new PixelImage(size, size);
            if (maxX < 0) return icon;
            minX -= pad; minY -= pad; maxX += pad; maxY += pad;
            int w = maxX - minX + 1, h = maxY - minY + 1;
            int ox = (size - Math.Min(size, w)) / 2, oy = (size - Math.Min(size, h)) / 2;
            for (int y = 0; y < Math.Min(size, h); y++)
            for (int x = 0; x < Math.Min(size, w); x++)
            {
                int sx = minX + x, sy = minY + y;
                byte r = f.RegionAt(sx, sy);
                if (!regions.Contains(r) && r != Region.Outline) continue;
                icon.Set(ox + x, oy + y, composed.Get(sx, sy));
            }
            return icon;
        }
    }
}
