using System;

namespace Thatchwick.Core.Customization.Layers
{
    /// <summary>
    /// Renk rampası (belge 43 §4.5, palet kilidi): bir taban renkten 4 tonluk piksel sanatı rampası.
    /// 0 en koyu, 1 gölge, 2 taban (renk aynen), 3 ışık. Gölgeler maviye-mora, ışık sarıya kayar (ton kaydırma).
    /// Aynı formül tools/art/ramps.py'de birebir vardır; ikisi Assets/Resources/Content/character_ramps.json ile
    /// karşılaştırılarak test edilir. Formül değişirse ikisi birlikte değişir ve dosya yeniden üretilir.
    /// </summary>
    public static class RampMath
    {
        public const int Size = 4;
        public const int BaseShade = 2;

        private const double ShadowHue = 250.0, LightHue = 55.0;
        private const double MinSatForHueShift = 0.08;

        public static Rgb[] Make(Rgb baseColor)
        {
            ToHsv(baseColor, out double h, out double s, out double v);
            var ramp = new Rgb[Size];
            ramp[0] = Shade(h, s, v, ShadowHue, 14.0, 1.12, 0.52, 0.0);
            ramp[1] = Shade(h, s, v, ShadowHue, 7.0, 1.06, 0.74, 0.0);
            ramp[2] = baseColor;
            ramp[3] = Shade(h, s, v, LightHue, 8.0, 0.80, 1.10, 0.06);
            return ramp;
        }

        private static Rgb Shade(double h, double s, double v, double targetHue, double amount, double satMul, double valMul, double valAdd)
        {
            double nh = s >= MinSatForHueShift ? ShiftHue(h, targetHue, amount) : h;
            double ns = Math.Min(1.0, s * satMul);
            double nv = Math.Min(1.0, v * valMul + valAdd);
            return FromHsv(nh, ns, nv);
        }

        /// <summary>Tonu hedefe doğru en kısa yaydan en çok 'amount' derece kaydırır.</summary>
        public static double ShiftHue(double h, double target, double amount)
        {
            double diff = ((target - h + 540.0) % 360.0) - 180.0;
            double step = Math.Sign(diff) * Math.Min(Math.Abs(diff), amount);
            return (h + step + 360.0) % 360.0;
        }

        public static void ToHsv(Rgb c, out double h, out double s, out double v)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double d = max - min;
            v = max;
            s = max <= 0.0 ? 0.0 : d / max;
            if (d <= 0.0) h = 0.0;
            else if (max == r) h = 60.0 * ((g - b) / d);
            else if (max == g) h = 60.0 * ((b - r) / d + 2.0);
            else h = 60.0 * ((r - g) / d + 4.0);
            if (h < 0.0) h += 360.0;
        }

        public static Rgb FromHsv(double h, double s, double v)
        {
            double c = v * s;
            double hp = h / 60.0;
            double mod2 = hp - 2.0 * Math.Floor(hp / 2.0);
            double x = c * (1.0 - Math.Abs(mod2 - 1.0));
            double r1, g1, b1;
            if (hp < 1.0) { r1 = c; g1 = x; b1 = 0.0; }
            else if (hp < 2.0) { r1 = x; g1 = c; b1 = 0.0; }
            else if (hp < 3.0) { r1 = 0.0; g1 = c; b1 = x; }
            else if (hp < 4.0) { r1 = 0.0; g1 = x; b1 = c; }
            else if (hp < 5.0) { r1 = x; g1 = 0.0; b1 = c; }
            else { r1 = c; g1 = 0.0; b1 = x; }
            double m = v - c;
            return new Rgb(Channel(r1 + m), Channel(g1 + m), Channel(b1 + m));
        }

        private static byte Channel(double x)
        {
            int i = (int)Math.Floor(x * 255.0 + 0.5);
            return (byte)(i < 0 ? 0 : i > 255 ? 255 : i);
        }

        /// <summary>Algısal ağırlıklı RGB uzaklığı (tools/art/style.py dist ile aynı "redmean").</summary>
        public static double Distance(Rgb a, Rgb b)
        {
            double rm = (a.R + b.R) / 2.0;
            double dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return Math.Sqrt((2 + rm / 256) * dr * dr + 4 * dg * dg + (2 + (255 - rm) / 256) * db * db) / 3.0;
        }

        /// <summary>Rampadaki en yakın tonun sırası ve uzaklığı.</summary>
        public static int NearestShade(Rgb[] ramp, Rgb c, out double distance)
        {
            int best = 0; distance = double.MaxValue;
            for (int i = 0; i < ramp.Length; i++)
            {
                double d = Distance(ramp[i], c);
                if (d < distance) { distance = d; best = i; }
            }
            return best;
        }
    }
}
