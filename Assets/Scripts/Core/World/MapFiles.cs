using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Bölge taslağı (belge 64a K1): <c>Assets/Resources/Content/maps/&lt;bölge&gt;/terrain.png</c> (1 piksel = 1 karo, sol üst
    /// yerel (0,0), sağa +x, aşağı +y) ve <c>markers.json</c>. Renk → arazi tablosu <see cref="MapPalette"/> (insan için ayrıca
    /// <c>maps/palette.json</c> ve <c>maps/README.md</c>). Taslak oyun açılırken okunur; gürültü ya da rastgelelik yok: dosyada
    /// ne varsa o. Dosyası olmayan bölge eski kod çizimiyle kurulur.
    /// </summary>
    public enum MapTerrain : byte
    {
        Grass = 0, Dirt = 1, Sand = 2, Stone = 3, Water = 4, DeepWater = 5, Path = 6, Bridge = 7,
        /// <summary>Kenar bandı: sık ağaç öbekleri, yürünmez gibi; bölge kenarını kapatır (düz kesim görünmez).</summary>
        Band = 8,
        /// <summary>Kaya sırtı: iri kayalar ve kütükler; yükseklik hissi verir (gerçek yükseklik yok, belge 64a K2).</summary>
        Rock = 9,
        /// <summary>Taş ocağı tabanı (art/queue 790): oyunda taş zemin (Stone), görünümde kırık kireç taşı.</summary>
        Quarry = 10,
        /// <summary>Bilinmeyen renk (yükleme hatası).</summary>
        Unknown = 255,
    }

    public static class MapPalette
    {
        public static readonly (byte r, byte g, byte b, MapTerrain t, string name)[] Entries =
        {
            (96, 140, 72, MapTerrain.Grass, "grass"),
            (139, 110, 80, MapTerrain.Dirt, "dirt"),
            (222, 200, 150, MapTerrain.Sand, "sand"),
            (130, 130, 138, MapTerrain.Stone, "stone"),
            (64, 120, 176, MapTerrain.Water, "water"),
            (36, 72, 132, MapTerrain.DeepWater, "deep_water"),
            (186, 140, 90, MapTerrain.Path, "path"),
            (150, 96, 56, MapTerrain.Bridge, "bridge"),
            (24, 64, 34, MapTerrain.Band, "band"),
            (92, 88, 84, MapTerrain.Rock, "rock"),
            (168, 160, 144, MapTerrain.Quarry, "quarry"),
        };

        public static MapTerrain Of(byte r, byte g, byte b)
        {
            foreach (var e in Entries) if (e.r == r && e.g == g && e.b == b) return e.t;
            return MapTerrain.Unknown;
        }
    }

    [Serializable] public sealed class MapFishDto { public string id; public int n; }
    [Serializable] public sealed class MapPondDto { public int x, y; public MapFishDto[] fish; }
    [Serializable] public sealed class MapItemDto { public string id; public int x, y, rot; public bool grown; }
    [Serializable] public sealed class MapRectDto { public int x0, y0, x1, y1; }

    /// <summary>markers.json: balık noktaları, simge yapılar ve sabit eşyalar, ağaç dikilmeyecek alanlar (yerel koordinat).</summary>
    [Serializable]
    public sealed class MapMarkersDto
    {
        public MapPondDto[] ponds;
        public MapItemDto[] items;
        public MapRectDto[] keep;
    }

    /// <summary>Bir bölgenin okunmuş taslağı.</summary>
    public sealed class RegionMap
    {
        public string RegionId;
        public int W, H;
        public MapTerrain[] Terrain;
        public MapMarkersDto Markers = new MapMarkersDto();

        public MapTerrain At(int x, int y) => x < 0 || y < 0 || x >= W || y >= H ? MapTerrain.Unknown : Terrain[y * W + x];

        public static RegionMap FromRgba(string regionId, int w, int h, byte[] rgba, MapMarkersDto markers, List<string> errors = null)
        {
            var m = new RegionMap { RegionId = regionId, W = w, H = h, Terrain = new MapTerrain[w * h], Markers = markers ?? new MapMarkersDto() };
            int unknown = 0;
            for (int i = 0; i < w * h; i++)
            {
                var t = MapPalette.Of(rgba[i * 4], rgba[i * 4 + 1], rgba[i * 4 + 2]);
                if (t == MapTerrain.Unknown) { unknown++; t = MapTerrain.Grass; }
                m.Terrain[i] = t;
            }
            if (unknown > 0) errors?.Add($"maps/{regionId}/terrain.png: {unknown} pixel(s) not in the palette (read as grass).");
            return m;
        }
    }

    /// <summary>
    /// Taslak kaynağı: oyun katmanı Resources'tan (Texture2D + TextAsset), testler ve araçlar dosyadan (<see cref="FromFolder"/>)
    /// kurar. Null ise taslak yok: bütün bölgeler kod çizimiyle kurulur.
    /// </summary>
    public static class MapFiles
    {
        public static Func<string, RegionMap> Provider;
        private static readonly Dictionary<string, RegionMap> Cache = new Dictionary<string, RegionMap>(StringComparer.Ordinal);
        public static readonly List<string> Errors = new List<string>();

        public static RegionMap Get(string regionId)
        {
            if (Provider == null || regionId == null) return null;
            lock (Cache)
            {
                if (Cache.TryGetValue(regionId, out var m)) return m;
                try { m = Provider(regionId); }
                catch (Exception ex) { Errors.Add($"maps/{regionId}: {ex.Message}"); m = null; }
                Cache[regionId] = m;
                return m;
            }
        }

        public static void Reset() { lock (Cache) Cache.Clear(); Errors.Clear(); }

        /// <summary>Klasörden okuyan kaynak (maps/&lt;bölge&gt;/terrain.png). markers okuyucu dışarıdan (JSON kütüphanesi Core'da yok).</summary>
        public static Func<string, RegionMap> FromFolder(string mapsRoot, Func<string, MapMarkersDto> readMarkers)
        {
            return id =>
            {
                var png = Path.Combine(mapsRoot, id, "terrain.png");
                if (!File.Exists(png)) return null;
                var (w, h, rgba) = PngReader.Decode(File.ReadAllBytes(png));
                var mk = Path.Combine(mapsRoot, id, "markers.json");
                return RegionMap.FromRgba(id, w, h, rgba, File.Exists(mk) ? readMarkers(File.ReadAllText(mk)) : null, Errors);
            };
        }
    }

    /// <summary>Küçük PNG çözücü (8 bit RGB/RGBA/gri/paletli, taramasız): taslak dosyaları için; Unity gerekmez.</summary>
    public static class PngReader
    {
        public static (int w, int h, byte[] rgba) Decode(byte[] data)
        {
            if (data.Length < 8 || data[0] != 0x89 || data[1] != 0x50) throw new InvalidDataException("Not a PNG file.");
            int pos = 8, w = 0, h = 0, depth = 0, type = 0, interlace = 0;
            byte[] palette = null, trns = null;
            var idat = new MemoryStream();
            while (pos + 8 <= data.Length)
            {
                int len = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
                string kind = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4);
                int body = pos + 8;
                switch (kind)
                {
                    case "IHDR":
                        w = Int(data, body); h = Int(data, body + 4); depth = data[body + 8]; type = data[body + 9]; interlace = data[body + 12];
                        break;
                    case "PLTE": palette = new byte[len]; Array.Copy(data, body, palette, 0, len); break;
                    case "tRNS": trns = new byte[len]; Array.Copy(data, body, trns, 0, len); break;
                    case "IDAT": idat.Write(data, body, len); break;
                }
                if (kind == "IEND") break;
                pos = body + len + 4;
            }
            if (depth != 8 || interlace != 0) throw new InvalidDataException($"Only 8-bit non-interlaced PNG is supported (depth {depth}, interlace {interlace}).");
            int bpp = type == 2 ? 3 : type == 6 ? 4 : type == 0 || type == 3 ? 1 : type == 4 ? 2 : throw new InvalidDataException($"PNG colour type {type} is not supported.");
            var raw = new MemoryStream();
            idat.Position = 2;   // zlib başlığı
            using (var z = new DeflateStream(idat, CompressionMode.Decompress)) z.CopyTo(raw);
            var src = raw.ToArray();
            int stride = w * bpp;
            var cur = new byte[stride]; var prev = new byte[stride];
            var rgba = new byte[w * h * 4];
            int s = 0;
            for (int y = 0; y < h; y++)
            {
                int filter = src[s++];
                Array.Copy(src, s, cur, 0, stride); s += stride;
                for (int i = 0; i < stride; i++)
                {
                    int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                    int v = cur[i];
                    switch (filter)
                    {
                        case 1: v += a; break;
                        case 2: v += b; break;
                        case 3: v += (a + b) >> 1; break;
                        case 4: { int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c); v += pa <= pb && pa <= pc ? a : pb <= pc ? b : c; break; }
                    }
                    cur[i] = (byte)v;
                }
                for (int x = 0; x < w; x++)
                {
                    int o = (y * w + x) * 4;
                    switch (type)
                    {
                        case 2: rgba[o] = cur[x * 3]; rgba[o + 1] = cur[x * 3 + 1]; rgba[o + 2] = cur[x * 3 + 2]; rgba[o + 3] = 255; break;
                        case 6: Array.Copy(cur, x * 4, rgba, o, 4); break;
                        case 0: rgba[o] = rgba[o + 1] = rgba[o + 2] = cur[x]; rgba[o + 3] = 255; break;
                        case 4: rgba[o] = rgba[o + 1] = rgba[o + 2] = cur[x * 2]; rgba[o + 3] = cur[x * 2 + 1]; break;
                        case 3:
                            int k = cur[x];
                            rgba[o] = palette[k * 3]; rgba[o + 1] = palette[k * 3 + 1]; rgba[o + 2] = palette[k * 3 + 2];
                            rgba[o + 3] = trns != null && k < trns.Length ? trns[k] : (byte)255;
                            break;
                    }
                }
                var t = prev; prev = cur; cur = t;
            }
            return (w, h, rgba);
        }

        private static int Int(byte[] d, int i) => (d[i] << 24) | (d[i + 1] << 16) | (d[i + 2] << 8) | d[i + 3];
    }
}
