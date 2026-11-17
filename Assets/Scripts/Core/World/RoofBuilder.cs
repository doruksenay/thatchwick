using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Otomatik çatının bir karosu: saçaktan sırta doğru yüksekliği ve eğimin indiği yön.</summary>
    public readonly struct RoofTile
    {
        public readonly GridPos Pos;
        /// <summary>Saçaktaki karo 1, sırta doğru artar.</summary>
        public readonly int Height;
        /// <summary>Eğimin aşağı indiği yön (sırtta da en yakın saçağa); x ya da y ekseninde birim vektör.</summary>
        public readonly GridPos Down;
        /// <summary>Sırt karosu (iki yanı da aynı yükseklikte ya da tek karo genişlik).</summary>
        public readonly bool IsRidge;

        public RoofTile(GridPos pos, int height, GridPos down, bool isRidge) { Pos = pos; Height = height; Down = down; IsRidge = isRidge; }
    }

    /// <summary>Tek parça çatı: aynı kattaki bitişik çatı karoları (bir evin birden çok odası tek çatı altında).</summary>
    public sealed class RoofRegion
    {
        public int Level { get; internal set; }
        public List<RoofTile> Tiles { get; } = new List<RoofTile>();
        /// <summary>Sırt x yönünde uzanır (eğimler -y/+y yönüne iner); değilse y yönünde.</summary>
        public bool RidgeAlongX { get; internal set; }
        public int MaxHeight { get; internal set; }
        /// <summary>Çatı stili (bölgenin karolarında kayıtlı ilk stil); null = varsayılan.</summary>
        public string Style { get; internal set; }
        /// <summary>Bölge kullanıcı tarafından çatısız bırakıldı (GridMap.NoRoof).</summary>
        public bool Hidden { get; internal set; }

        public bool Contains(GridPos p)
        {
            foreach (var t in Tiles) if (t.Pos == p) return true;
            return false;
        }
    }

    /// <summary>
    /// Otomatik çatı (Sims gibi): her iç mekân odasının üstü açık karoları (üstünde döşeme olmayan) çatı karosudur.
    /// Aynı kattaki bitişik çatı karoları tek çatı olur; çatı bölgenin uzun ekseni boyunca sırtlı beşik çatıdır.
    /// Bodrumun çatısı yoktur (üstü zemin kattır). Elle çatı çizimi sonraya kaldı.
    /// </summary>
    public static class RoofBuilder
    {
        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <param name="includeHidden">Çatısız bırakılmış bölgeler de dönsün (yapı aracı çatıyı yeniden açabilsin).</param>
        public static List<RoofRegion> Compute(GridMap map, bool includeHidden = false)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            var roofTiles = new HashSet<GridPos>();
            foreach (var room in map.Rooms.IndoorRooms)
            {
                if (room.Level < 0) continue;
                foreach (var t in room.Tiles)
                {
                    var above = t.OnLevel(t.Z + 1);
                    if (GridMap.IsValidLevel(above.Z) && map.HasTile(above)) continue;   // üstünde kat var: tavan, çatı değil
                    roofTiles.Add(t);
                }
            }

            var regions = new List<RoofRegion>();
            var seen = new HashSet<GridPos>();
            var queue = new Queue<GridPos>();
            foreach (var start in roofTiles)
            {
                if (seen.Contains(start)) continue;
                var tiles = new List<GridPos>();
                seen.Add(start);
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    tiles.Add(cur);
                    foreach (var d in Dirs)
                    {
                        var n = cur + d;
                        if (roofTiles.Contains(n) && seen.Add(n)) queue.Enqueue(n);
                    }
                }

                string style = null;
                bool hidden = true;
                foreach (var t in tiles)
                {
                    var s = map.GetRoofStyle(t);
                    if (s != GridMap.NoRoof) hidden = false;
                    if (style == null && s != null && s != GridMap.NoRoof) style = s;
                }
                if (hidden && !includeHidden) continue;
                regions.Add(Shape(tiles, style, hidden));
            }
            // Kararlı sıra: kat, sonra en küçük karo.
            regions.Sort((a, b) => a.Level != b.Level ? a.Level.CompareTo(b.Level) : Key(a).CompareTo(Key(b)));
            return regions;
        }

        private static int Key(RoofRegion r)
        {
            int best = int.MaxValue;
            foreach (var t in r.Tiles) best = Math.Min(best, t.Pos.Y * 10000 + t.Pos.X);
            return best;
        }

        // Beşik çatı: sırt bölgenin uzun ekseninde. Her karonun yüksekliği, kısa eksen boyunca kendi sütunundaki
        // (ya da satırındaki) kesintisiz karo dizisinin kenarına uzaklığıdır; L biçimli evlerde de çalışır.
        private static RoofRegion Shape(List<GridPos> tiles, string style, bool hidden)
        {
            int x0 = int.MaxValue, x1 = int.MinValue, y0 = int.MaxValue, y1 = int.MinValue;
            foreach (var t in tiles) { x0 = Math.Min(x0, t.X); x1 = Math.Max(x1, t.X); y0 = Math.Min(y0, t.Y); y1 = Math.Max(y1, t.Y); }
            bool alongX = (x1 - x0) >= (y1 - y0);
            var set = new HashSet<GridPos>(tiles);
            var region = new RoofRegion { Level = tiles[0].Z, RidgeAlongX = alongX, Style = style, Hidden = hidden };
            var down = alongX ? new GridPos(0, 1) : new GridPos(1, 0);
            foreach (var t in tiles)
            {
                int before = 0, after = 0;
                while (set.Contains(new GridPos(t.X - down.X * (before + 1), t.Y - down.Y * (before + 1), t.Z))) before++;
                while (set.Contains(new GridPos(t.X + down.X * (after + 1), t.Y + down.Y * (after + 1), t.Z))) after++;
                int h = Math.Min(before, after) + 1;
                bool ridge = Math.Abs(before - after) <= 1;   // tek genişlikte orta karo, çift genişlikte ortadaki iki karo
                // Eğim yakın saçağa iner: önde (küçük koordinat) saçak daha yakınsa -yön.
                var dir = before < after ? new GridPos(-down.X, -down.Y) : down;
                region.Tiles.Add(new RoofTile(t, h, dir, ridge));
                region.MaxHeight = Math.Max(region.MaxHeight, h);
            }
            region.Tiles.Sort((a, b) => a.Pos.Y != b.Pos.Y ? a.Pos.Y.CompareTo(b.Pos.Y) : a.Pos.X.CompareTo(b.Pos.X));
            return region;
        }
    }
}
