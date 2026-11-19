using System;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Izgaraya göre dört mantıksal yön. Tek tanım burasıdır (docs/gorsel-yonerge.md "Yön kuralı" bölümü buna bakar).
    /// N = -y, E = +x, S = +y, W = -x: Regions'taki kapı harfleri ('N' kapı y0 kenarında) ve WallSides (0 kuzey, 1 doğu,
    /// 2 güney, 3 batı) ile aynı sıra. Sayısal değer saat yönündeki dönüş adımıdır.
    /// </summary>
    public enum Dir4 { N = 0, E = 1, S = 2, W = 3 }

    /// <summary>
    /// Yön ile ekran arasındaki eşleme (Iso.GridToWorld: +x ekranda sağ-yukarı, +y sol-yukarı):
    /// N (-y) ekranda sağ-aşağı "se", E (+x) sağ-yukarı "ne", S (+y) sol-yukarı "nw", W (-x) sol-aşağı "sw".
    /// Kamera ekranın altından bakar: N ve W kameraya dönük (ön yüz görünür), E ve S arkası dönük.
    /// Ayna (yatay çevirme) se ile sw'yi, ne ile nw'yi değiştirir: N ↔ W, E ↔ S. Arkası dönük görünüm aynayla elde edilmez.
    /// </summary>
    public static class Facing
    {
        public static readonly Dir4[] All = { Dir4.N, Dir4.E, Dir4.S, Dir4.W };

        public static (int dx, int dy) Vec(Dir4 d)
        {
            switch (d)
            {
                case Dir4.N: return (0, -1);
                case Dir4.E: return (1, 0);
                case Dir4.S: return (0, 1);
                default: return (-1, 0);
            }
        }

        public static GridPos Step(GridPos p, Dir4 d, int n = 1)
        {
            var (dx, dy) = Vec(d);
            return new GridPos(p.X + dx * n, p.Y + dy * n, p.Z);
        }

        public static Dir4 Rotate(Dir4 d, int steps) => (Dir4)(((int)d + steps % 4 + 4) % 4);
        public static Dir4 Opposite(Dir4 d) => Rotate(d, 2);

        /// <summary>Ekran yönü adı (görsel dosya eki): se, ne, nw, sw.</summary>
        public static string ScreenName(Dir4 d)
        {
            switch (d)
            {
                case Dir4.N: return "se";
                case Dir4.E: return "ne";
                case Dir4.S: return "nw";
                default: return "sw";
            }
        }

        /// <summary>Görsel dosya ekinden yön (se → N ...); tanınmazsa false.</summary>
        public static bool TryFromScreen(string screen, out Dir4 d)
        {
            switch ((screen ?? "").Trim().ToLowerInvariant())
            {
                case "se": d = Dir4.N; return true;
                case "ne": d = Dir4.E; return true;
                case "nw": d = Dir4.S; return true;
                case "sw": d = Dir4.W; return true;
                default: d = Dir4.N; return false;
            }
        }

        public static bool TryParse(string s, out Dir4 d)
        {
            switch ((s ?? "").Trim().ToUpperInvariant())
            {
                case "N": d = Dir4.N; return true;
                case "E": d = Dir4.E; return true;
                case "S": d = Dir4.S; return true;
                case "W": d = Dir4.W; return true;
                default: d = Dir4.N; return false;
            }
        }

        /// <summary>Kameraya dönük mü (ön yüz görünür).</summary>
        public static bool FacesCamera(Dir4 d) => d == Dir4.N || d == Dir4.W;

        /// <summary>Yatay aynanın verdiği yön: N ↔ W, E ↔ S.</summary>
        public static Dir4 Mirror(Dir4 d)
        {
            switch (d)
            {
                case Dir4.N: return Dir4.W;
                case Dir4.W: return Dir4.N;
                case Dir4.E: return Dir4.S;
                default: return Dir4.E;
            }
        }

        /// <summary>Kapı harfinden (Regions.Building: 'N','S','E','W') yön.</summary>
        public static Dir4 FromChar(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'N': return Dir4.N;
                case 'E': return Dir4.E;
                case 'S': return Dir4.S;
                case 'W': return Dir4.W;
                default: throw new ArgumentException($"Bilinmeyen yön '{c}'.");
            }
        }

        /// <summary>İki komşu karo arasındaki yön (a'dan b'ye); komşu değilse false.</summary>
        public static bool Between(GridPos a, GridPos b, out Dir4 d)
        {
            int dx = b.X - a.X, dy = b.Y - a.Y;
            d = Dir4.N;
            if (dx == 0 && dy == -1) { d = Dir4.N; return true; }
            if (dx == 1 && dy == 0) { d = Dir4.E; return true; }
            if (dx == 0 && dy == 1) { d = Dir4.S; return true; }
            if (dx == -1 && dy == 0) { d = Dir4.W; return true; }
            return false;
        }

        /// <summary>Karonun d yönündeki kenarı (WallSides ile aynı: side = (int)d).</summary>
        public static Edge EdgeOf(GridPos tile, Dir4 d) => WallSides.EdgeOf(tile, (int)d);
    }
}
