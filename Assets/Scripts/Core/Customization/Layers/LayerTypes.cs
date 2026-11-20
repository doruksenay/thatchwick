using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Customization.Layers
{
    /// <summary>
    /// Mankenin bölge kodları (belge 43 §4.2-4.3). Bölge haritası PNG'sinde kırmızı kanal = bölge kodu,
    /// yeşil kanal = ton sırası (0-3), mavi kanal = çapa işareti (1 baş, 2 göz), alfa = örtü.
    /// tools/art/manken.py aynı kodları yazar.
    /// </summary>
    public static class Region
    {
        public const byte Empty = 0;
        public const byte Outline = 1;
        public const byte Skin = 2;
        public const byte Main = 3;      // üst giysi
        public const byte Trim = 4;      // kenar, yen ağzı
        public const byte Bottom = 5;    // alt giysi
        public const byte Shoes = 6;
        public const byte Belt = 7;
        public const byte EyeWhite = 8;
        public const byte Iris = 9;
        public const byte Brow = 10;
        public const byte Count = 11;
    }

    /// <summary>
    /// Mankenin anahtar renkleri: PixelLab mankeni bu renklerde üretir, betik her pikseli en yakın anahtar
    /// rampaya eşleyip bölge haritasını çıkarır. Ten doğal bir rampadır (yüz okunur kalsın diye).
    /// </summary>
    public static class KeyColors
    {
        public static readonly Rgb Outline = Rgb.FromHex("#2A1E1E");
        public static readonly Rgb Skin = Rgb.FromHex("#D9A877");
        public static readonly Rgb Main = Rgb.FromHex("#D83030");
        public static readonly Rgb Trim = Rgb.FromHex("#3050D8");
        public static readonly Rgb Bottom = Rgb.FromHex("#30A848");
        public static readonly Rgb Shoes = Rgb.FromHex("#D8D800");
        public static readonly Rgb Belt = Rgb.FromHex("#28C0C0");
        public static readonly Rgb EyeWhite = Rgb.FromHex("#F0F0F0");
        public static readonly Rgb Iris = Rgb.FromHex("#E040E0");
        public static readonly Rgb Brow = Rgb.FromHex("#7030C0");

        /// <summary>Baş parçaları (saç, sakal) bu kahve rampada çizilir; oyunda saç rengine çevrilir.</summary>
        public static readonly Rgb ReferenceHair = Rgb.FromHex("#7A4B25");
        /// <summary>Başlık ve süs bu gri rampada çizilir; oyunda başlık/süs rengine çevrilir.</summary>
        public static readonly Rgb ReferenceCloth = Rgb.FromHex("#9A9A9A");
        /// <summary>Ayakkabı ve kemer: sabit deri rampası.</summary>
        public static readonly Rgb Leather = Rgb.FromHex("#5E4630");
    }

    /// <summary>Üst üste bindirme için RGBA piksel.</summary>
    public struct Px32
    {
        public byte R, G, B, A;
        public Px32(byte r, byte g, byte b, byte a) { R = r; G = g; B = b; A = a; }
        public Px32(Rgb c) { R = c.R; G = c.G; B = c.B; A = 255; }
        public Rgb Rgb => new Rgb(R, G, B);
        public bool Opaque => A >= 128;
        public static readonly Px32 Clear = new Px32(0, 0, 0, 0);
    }

    /// <summary>Satırlar yukarıdan aşağı (0 = üst satır). Unity dokusu aşağıdan yukarıdır; Game katmanı çevirir.</summary>
    public sealed class PixelImage
    {
        public int Width { get; }
        public int Height { get; }
        public Px32[] Px { get; }

        public PixelImage(int width, int height)
        {
            Width = width; Height = height;
            Px = new Px32[width * height];
        }

        public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public Px32 Get(int x, int y) => Inside(x, y) ? Px[y * Width + x] : Px32.Clear;
        public void Set(int x, int y, Px32 p) { if (Inside(x, y)) Px[y * Width + x] = p; }
    }

    /// <summary>Mankenin bir karesi: bölge ve ton haritası (bölge haritası PNG'sinden).</summary>
    public sealed class RegionFrame
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Region { get; }
        public byte[] Shade { get; }

        public RegionFrame(int width, int height)
        {
            Width = width; Height = height;
            Region = new byte[width * height];
            Shade = new byte[width * height];
        }

        /// <summary>Görselin içindeki çapalar (mavi kanal 1 baş, 2 göz). İşaret yoksa baş en üst opak satırın ortası.</summary>
        public FrameAnchor Anchor { get; private set; } = new FrameAnchor();

        public const byte HeadMark = 1, EyeMark = 2;

        /// <summary>Bölge haritası görselinden: R = bölge, G = ton, B = çapa işareti, alfa yarıdan azsa boş.</summary>
        public static RegionFrame Decode(PixelImage encoded)
        {
            var f = new RegionFrame(encoded.Width, encoded.Height);
            bool head = false;
            var anchor = new FrameAnchor();
            for (int i = 0; i < encoded.Px.Length; i++)
            {
                var p = encoded.Px[i];
                if (!p.Opaque || p.R == 0 || p.R >= Layers.Region.Count) continue;
                f.Region[i] = p.R;
                f.Shade[i] = (byte)Math.Min(RampMath.Size - 1, (int)p.G);
                int x = i % encoded.Width, y = i / encoded.Width;
                if (p.B == HeadMark) { anchor.HeadX = x; anchor.HeadY = y; head = true; }
                else if (p.B == EyeMark) anchor.Eyes.Add((x, y));
            }
            if (!head) GuessHead(f, anchor);
            anchor.Eyes.Sort((a, b) => a.x.CompareTo(b.x));
            f.Anchor = anchor;
            return f;
        }

        private static void GuessHead(RegionFrame f, FrameAnchor anchor)
        {
            for (int y = 0; y < f.Height; y++)
            {
                int sum = 0, n = 0;
                for (int x = 0; x < f.Width; x++) if (f.Region[y * f.Width + x] != Layers.Region.Empty) { sum += x; n++; }
                if (n == 0) continue;
                anchor.HeadX = sum / n; anchor.HeadY = y;
                return;
            }
        }

        public byte RegionAt(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height ? Layers.Region.Empty : Region[y * Width + x];
        public byte ShadeAt(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height ? (byte)0 : Shade[y * Width + x];
    }

    /// <summary>
    /// Bir karenin çapaları (bölge haritasının mavi kanalından; tools/art/manken.py yazar): baş çapası (kel kafanın tepe noktası) ve görünen
    /// göz merkezleri. Göz ve kaş desenleri, kirli sakal ve baş parçaları buna göre yerleşir.
    /// </summary>
    public sealed class FrameAnchor
    {
        public int HeadX { get; set; }
        public int HeadY { get; set; }
        /// <summary>Görünen gözler (0, 1 ya da 2): (x, y) piksel; soldan sağa sıralı.</summary>
        public List<(int x, int y)> Eyes { get; } = new List<(int, int)>();
    }

    /// <summary>Baş parçası tuvali: 64x96, çapa (kel kafanın tepesi) (32, 24). Parça görselleri bu sözleşmeyle çizilir.</summary>
    public static class PartCanvas
    {
        public const int Width = 64, Height = 96, AnchorX = 32, AnchorY = 24;
    }

    /// <summary>Baş parçası yuvaları; dosya adı: lay_&lt;yuva&gt;_&lt;kimlik&gt;_&lt;yön&gt;_fr|bk.</summary>
    public static class PartSlot
    {
        public const string Mass = "mass", Front = "front", Extra = "extra", Ornament = "orn",
            Mustache = "must", Beard = "beard", Sideburns = "side", Hat = "hat";
    }

    /// <summary>Baş parçası görseli kaynağı (Unity'de doku kütüphanesi, testte sözlük). Yoksa null.</summary>
    public interface IPartSource
    {
        PixelImage Get(string slot, string id, string direction, bool back);
    }

    /// <summary>
    /// Sekiz yönden beş benzersiz çizim yönüne: batı tarafı doğunun aynasıdır (belge 43 §4.2, katman B).
    /// Yön adları IsoArt.Directions ile aynı (e, ne, n, nw, w, sw, s, se).
    /// </summary>
    public static class PartDirections
    {
        public static string Drawn(string dir, out bool mirror)
        {
            mirror = false;
            switch (dir)
            {
                case "sw": mirror = true; return "se";
                case "w": mirror = true; return "e";
                case "nw": mirror = true; return "ne";
                default: return dir;
            }
        }

        /// <summary>Yüzün göründüğü yönler: göz, kaş, kirli sakal bu yönlerde çizilir.</summary>
        public static bool FaceVisible(string dir) => dir == "s" || dir == "se" || dir == "sw" || dir == "e" || dir == "w";
    }
}
