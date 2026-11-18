using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Mining
{
    /// <summary>Kazılacak karonun türü. Sertlik = kazmak için gereken vuruş (23. belge 4.1).</summary>
    public enum RockKind { Open = 0, Soil = 1, Soft = 2, Stone = 3, Hard = 4, Bedrock = 5, Rubble = 6 }

    /// <summary>Galeri desteği: ahşap çatkı (çürür), taş kemer (kalıcı).</summary>
    public enum SupportKind { None = 0, Wood = 1, StoneArch = 2 }

    /// <summary>Kazı yeri türü: evin altındaki arsa kazısı (kat -2) ya da tepedeki aile ocağının bir katı.</summary>
    public enum MineSiteKind { Plot = 0, Mine = 1 }

    /// <summary>Bir kazı karosu.</summary>
    public sealed class MineTile
    {
        public RockKind Rock;
        /// <summary>Damar ya da cep: kazılınca bu hammadde çıkar (null: yalnızca taş/moloz/kil).</summary>
        public string Ore;
        /// <summary>Sulu karo: açılınca her gün su verir.</summary>
        public bool Wet;
        /// <summary>Ateşle kırıldı: bir vuruşta kazılır.</summary>
        public bool Cracked;
        /// <summary>Kazılacak diye işaretli (iş kuyruğu).</summary>
        public bool Marked;
        /// <summary>Çatırdıyor: bugün desteklenmezse yarın çöker.</summary>
        public bool Creaking;
        public SupportKind Support;
        /// <summary>Desteğin yaşı (gün): ahşap çatkı ömrünü aşınca çürür.</summary>
        public double SupportAge;
        /// <summary>Işık (mum, kandil, meşale): yarıçap ve kalan gün; 0 = ışık yok.</summary>
        public int LightRadius;
        public double LightDays;

        public bool IsOpen => Rock == RockKind.Open;
    }

    /// <summary>
    /// Bir kazı yerinin ızgarası (arsa kazısında harita koordinatları, ocakta kendi koordinatları). Kalıcıdır: kazılan şekil
    /// kayda girer ve nesilden nesile kalır. Su, açık karolara yayılır (derinlik = su / açık karo).
    /// </summary>
    public sealed class MineMap
    {
        public string Id { get; }
        public MineSiteKind Kind { get; }
        /// <summary>Dünya katı: arsa -2; ocak katları -1, -2, -3 (ocağın kendi derinliği).</summary>
        public int Level { get; }
        public int Width { get; }
        public int Height { get; }
        public int Seed { get; }
        private readonly MineTile[] _tiles;

        /// <summary>Katta birikmiş su (birim).</summary>
        public double Water { get; internal set; }
        /// <summary>Ateşle kırmanın dumanı (gün): sıfıra inene kadar burada çalışılmaz.</summary>
        public double SmokeDays { get; internal set; }

        public MineMap(string id, MineSiteKind kind, int level, int width, int height, int seed)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            Id = id; Kind = kind; Level = level; Width = width; Height = height; Seed = seed;
            _tiles = new MineTile[width * height];
            for (int i = 0; i < _tiles.Length; i++) _tiles[i] = new MineTile { Rock = RockKind.Stone };
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public MineTile Get(int x, int y) => InBounds(x, y) ? _tiles[y * Width + x] : null;
        public bool IsOpen(int x, int y) { var t = Get(x, y); return t != null && t.IsOpen; }

        public int OpenCount
        {
            get { int n = 0; foreach (var t in _tiles) if (t.IsOpen) n++; return n; }
        }

        /// <summary>Su derinliği: 0 kuru, 1 ıslak, 2 sığ su, 3 dolu (23. belge 7.2).</summary>
        public int WaterDepth
        {
            get
            {
                int open = OpenCount;
                if (open == 0 || Water <= 0) return 0;
                return (int)Math.Min(3, Math.Floor(Water / open));
            }
        }

        public IEnumerable<(int x, int y, MineTile t)> All()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    yield return (x, y, _tiles[y * Width + x]);
        }

        public static readonly (int dx, int dy)[] Four = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        public static readonly (int dx, int dy)[] Eight = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };

        /// <summary>Açık bir karoya dört yönden komşu mu (kazmak için yanına varılmalı)?</summary>
        public bool TouchesOpen(int x, int y)
        {
            foreach (var (dx, dy) in Four) if (IsOpen(x + dx, y + dy)) return true;
            return false;
        }

        /// <summary>Karo bir ışığın yarıçapında mı?</summary>
        public bool IsLit(int x, int y, int extraX = int.MinValue, int extraY = 0, int extraRadius = 0)
        {
            if (extraRadius > 0 && Math.Max(Math.Abs(x - extraX), Math.Abs(y - extraY)) <= extraRadius) return true;
            for (int ty = Math.Max(0, y - 4); ty <= Math.Min(Height - 1, y + 4); ty++)
                for (int tx = Math.Max(0, x - 4); tx <= Math.Min(Width - 1, x + 4); tx++)
                {
                    var t = _tiles[ty * Width + tx];
                    if (t.LightRadius > 0 && t.LightDays > 0 && Math.Max(Math.Abs(x - tx), Math.Abs(y - ty)) <= t.LightRadius) return true;
                }
            return false;
        }

        /// <summary>Bu mesafe içinde (Chebyshev) bir destek var mı?</summary>
        public bool SupportWithin(int x, int y, int dist)
        {
            for (int ty = y - dist; ty <= y + dist; ty++)
                for (int tx = x - dist; tx <= x + dist; tx++)
                {
                    var t = Get(tx, ty);
                    if (t != null && t.Support != SupportKind.None) return true;
                }
            return false;
        }

        /// <summary>Gevşek karo: 8 komşusundan en az 5'i açık ve destek mesafesinde destek yok (23. belge 7.1).</summary>
        public bool IsLoose(int x, int y, int supportDist)
        {
            var t = Get(x, y);
            if (t == null || !t.IsOpen) return false;
            int open = 0;
            foreach (var (dx, dy) in Eight) if (IsOpen(x + dx, y + dy)) open++;
            return open >= 5 && !SupportWithin(x, y, supportDist);
        }

        /// <summary>Damar izi: karonun 1 (ustaysa 2) yakınında henüz kazılmamış bir damar karosu var mı ve karo aydınlık mı.</summary>
        public string VeinHint(int x, int y, int reach)
        {
            for (int ty = y - reach; ty <= y + reach; ty++)
                for (int tx = x - reach; tx <= x + reach; tx++)
                {
                    var t = Get(tx, ty);
                    if (t != null && !t.IsOpen && t.Ore != null) return t.Ore;
                }
            return null;
        }
    }
}
