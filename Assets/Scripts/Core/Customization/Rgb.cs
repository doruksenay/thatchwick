using System;
using System.Globalization;

namespace Thatchwick.Core.Customization
{
    /// <summary>Unity'siz basit renk (0-255). Görünümde renkler bununla tutulur, Unity katmanı Color'a çevirir.</summary>
    public readonly struct Rgb : IEquatable<Rgb>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>"#RRGGBB" ya da "RRGGBB".</summary>
        public static bool TryParseHex(string hex, out Rgb color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex)) return false;
            string h = hex.Trim();
            if (h.StartsWith("#", StringComparison.Ordinal)) h = h.Substring(1);
            if (h.Length != 6) return false;
            if (!byte.TryParse(h.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)) return false;
            if (!byte.TryParse(h.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)) return false;
            if (!byte.TryParse(h.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) return false;
            color = new Rgb(r, g, b);
            return true;
        }

        public static Rgb FromHex(string hex)
        {
            if (!TryParseHex(hex, out var c)) throw new FormatException($"Geçersiz renk: '{hex}'");
            return c;
        }

        public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";

        public static Rgb Lerp(Rgb a, Rgb b, double t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return new Rgb(
                (byte)Math.Round(a.R + (b.R - a.R) * t),
                (byte)Math.Round(a.G + (b.G - a.G) * t),
                (byte)Math.Round(a.B + (b.B - a.B) * t));
        }

        public bool Equals(Rgb other) => R == other.R && G == other.G && B == other.B;
        public override bool Equals(object obj) => obj is Rgb o && Equals(o);
        public override int GetHashCode() => (R << 16) | (G << 8) | B;
        public override string ToString() => ToHex();
        public static bool operator ==(Rgb a, Rgb b) => a.Equals(b);
        public static bool operator !=(Rgb a, Rgb b) => !a.Equals(b);
    }
}
