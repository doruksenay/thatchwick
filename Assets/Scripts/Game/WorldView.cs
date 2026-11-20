using System;
using UnityEngine;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// GridMap'i izometrik elmas karolar olarak çizer. Yer tutucu: asset hattı (A1) gelince
    /// gerçek zemin sprite'larıyla değişir. Zemin her zaman her şeyin arkasında çizilir.
    /// Katlar: ViewLevel görünen en üst kattır. 0 ve 1'de zemin kat (ve 1'de üst kat) görünür; -1'de yalnızca bodrum
    /// (kazılmamış toprak koyu çizilir). Her katın zemini kendi çizim bandındadır (Iso.LevelSortStep).
    /// </summary>
    public sealed class WorldView : MonoBehaviour
    {
        private const int GroundSortOrder = -10000;
        private GroundLayer _shimmerLayer, _decalLayer, _snowLayer;
        private readonly TileCell _shimmerCell = new TileCell(), _decalCell = new TileCell(), _snowCell = new TileCell();

        private static readonly Color GrassA = new Color32(92, 140, 77, 255);
        private static readonly Color GrassB = new Color32(86, 133, 72, 255);
        private static readonly Color BlockedColor = new Color32(58, 74, 92, 255);
        private static readonly Color EarthColor = new Color32(38, 30, 26, 255);
        private static readonly Color VoidColor = new Color(1f, 1f, 1f, 0.07f);

        /// <summary>Haritanın dışı (kamera arka planı): kenar şeridi bu renge solar.</summary>
        public static readonly Color OutsideColor = new Color32(22, 31, 24, 255);
        /// <summary>Harita kenarındaki solan çim şeridinin derinliği (karo). Sert kenar yerine koyu ormana karışır.</summary>
        private const int FringeDepth = 7;

        private struct FringeTile { public int X, Y; public float T; }
        private FringeTile[] _fringe;
        private GroundLayer _fringeLayer;

        // Kat başına bir zemin katmanı (Tilemap; dizin z - MinLevel). Karo başına GameObject yok.
        private GroundLayer[] _layers;
        private readonly TileCell _cell = new TileCell();

        /// <summary>Yapı modunda sahipsiz karolar koyulaştırılır.</summary>
        public bool ShowPlot;
        private bool[] _terrainBlocked;
        private int _viewLevel;

        public GridMap Map { get; private set; }

        /// <summary>Zemin stillerinin rengi için yapı parçası kataloğu (yoksa türün sabit rengi).</summary>
        public ContentDatabase Content { get; set; }
        /// <summary>Su yolu karosu su taşıyor mu (su işleri); null ise hepsi kuru çizilir.</summary>
        public Func<GridPos, bool> WetConduit { get; set; }

        // Karonun zemin stili rengi (dama deseni için çift/tek karoda biraz koyulaşır); stil ya da katalog yoksa null.
        private Color? StyleColor(GridPos p, FloorType floor)
        {
            if (Content == null || floor == FloorType.Grass) return null;
            var style = Map.GetFloorStyle(p);
            BuildPartDef part = null;
            if (style != null) Content.Parts.TryGet(style, out part);
            if (part == null) part = BuildParts.DefaultFloor(Content, floor);
            if (part == null || !part.TryGetRgb(out byte r, out byte g, out byte b)) return null;
            float k = ((p.X + p.Y) & 1) == 0 ? 1f : 0.94f;
            return new Color(r / 255f * k, g / 255f * k, b / 255f * k, 1f);
        }

        /// <summary>Görünen en üst kat (-1 bodrum, 0 zemin, 1 üst kat). Değişince LevelChanged tetiklenir.</summary>
        public int ViewLevel
        {
            get => _viewLevel;
            set
            {
                int v = Mathf.Clamp(value, GridMap.MinLevel, GridMap.MaxLevel);
                if (v == _viewLevel) return;
                _viewLevel = v;
                RefreshFloors();
                LevelChanged?.Invoke();
            }
        }

        public event Action LevelChanged;

        /// <summary>Kat -2'de kazılmamış karonun rengi (kazı motoru); null ise toprak rengi.</summary>
        public Func<GridPos, Color?> RockTint { get; set; }

        /// <summary>Kazılmamış karonun kaya görseli (taban adı, çeşit eki olmadan; IsoArt.RockTop) ve üstüne çarpılacak renk.</summary>
        public Func<GridPos, (string art, Color tint)?> RockArt { get; set; }

        /// <summary>Bu katın nesneleri (eşya, duvar, karakter) şu an çizilmeli mi.</summary>
        public bool IsLevelVisible(int z) => _viewLevel < 0 ? z == _viewLevel : z >= 0 && z <= _viewLevel;

        /// <summary>Çizilen alan (etkin bölge). Karolar yalnızca bu dikdörtgende oluşturulur; diğer bölgeler ekran dışıdır.</summary>
        public RectInt Area => _area;
        private RectInt _area;
        private readonly System.Collections.Generic.List<GameObject> _spawned = new System.Collections.Generic.List<GameObject>();

        /// <summary>
        /// Nokta etkin bölgede mi (kenar dahil: bölgenin doğu/kuzey sınırındaki duvar kenarı x = xMax'ta durur).
        /// Duvar ve çatı görünümü yalnızca etkin bölgeyi çizer (komşu bölgenin binaları kenarda görünmesin).
        /// </summary>
        public bool InArea(int x, int y) => x >= _area.xMin && y >= _area.yMin && x <= _area.xMax && y <= _area.yMax;

        public void Build(GridMap map) => Build(map, new RectInt(0, 0, map.Width, map.Height));

        public void Build(GridMap map, RectInt area)
        {
            Map = map;
            int plane = map.Width * map.Height;
            _terrainBlocked = new bool[plane];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    _terrainBlocked[y * map.Width + x] = map.IsBlocked(new GridPos(x, y));
            _area = area;
            BuildTiles();
        }

        /// <summary>Etkin bölge değişti (harita geçişi): eski karolar silinir, yeni bölgenin karoları kurulur.</summary>
        public void SetArea(RectInt area)
        {
            if (Map == null || area.Equals(_area)) return;
            _area = area;
            BuildTiles();
            RefreshFloors();
        }

        private void BuildTiles()
        {
            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();
            if (_layers == null)
            {
                _layers = new GroundLayer[GridMap.LevelCount];
                for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                    _layers[z - GridMap.MinLevel] = new GroundLayer(transform, $"Ground {z}", new Vector3(0f, z * Iso.LevelHeight, 0f),
                        GroundSortOrder + z * Iso.LevelSortStep, SpriteMaterials.Unlit);
                _fringeLayer = new GroundLayer(transform, "Ground edge", Vector3.zero, GroundSortOrder - 1, SpriteMaterials.Unlit);
                // Su parıltısı (shader varsa) ve zemin çıkartmaları (birikinti, çamur, köpük, ot): zeminin ve patikanın üstünde.
                var shimmer = SpriteMaterials.WaterShimmer;
                if (shimmer != null) _shimmerLayer = new GroundLayer(transform, "Water shimmer", Vector3.zero, GroundSortOrder + 2, shimmer);
                var snow = SpriteMaterials.SnowCover;
                if (snow != null) _snowLayer = new GroundLayer(transform, "Snow cover", Vector3.zero, GroundSortOrder + 4, snow);
                _decalLayer = new GroundLayer(transform, "Ground decals", Vector3.zero, GroundSortOrder + 3, SpriteMaterials.Unlit);
            }
            _shimmerLayer?.SetArea(_area);
            _snowLayer?.SetArea(_area);
            _decalLayer?.SetArea(_area);
            foreach (var layer in _layers) layer.SetArea(_area);
            BuildFringe();
        }

        // Harita kenarı (görsel analiz, 18.11.2026): haritanın çevresine, dışa doğru koyulaşıp saydamlaşan bir çim şeridi.
        // Yalnızca görsel; oyun mantığında karşılığı yok. Orman sınırı görseli gelince (art/queue/370) üstüne ağaç dizilecek.
        private void BuildFringe()
        {
            var list = new System.Collections.Generic.List<FringeTile>();
            for (int y = _area.yMin - FringeDepth; y < _area.yMax + FringeDepth; y++)
            {
                for (int x = _area.xMin - FringeDepth; x < _area.xMax + FringeDepth; x++)
                {
                    if (x >= _area.xMin && y >= _area.yMin && x < _area.xMax && y < _area.yMax) continue;
                    int dx = x < _area.xMin ? _area.xMin - x : (x >= _area.xMax ? x - _area.xMax + 1 : 0);
                    int dy = y < _area.yMin ? _area.yMin - y : (y >= _area.yMax ? y - _area.yMax + 1 : 0);
                    int d = Mathf.Max(dx, dy);
                    // Halkalar düz görünmesin diye karoya sabit küçük bir sapma.
                    int h = (x * 92837111) ^ (y * 689287499);
                    // Basamaklı halka görünmesin (65b B8): karo sapması büyük, üstüne kenar boyunca yavaş değişen gürültü.
                    float jitter = ((h & 0xFF) / 255f - 0.5f) * 0.22f;
                    float wave = (Noise((x + y) / 5, (x - y) / 5, 77) - 0.5f) * 0.35f;
                    float t = Mathf.Clamp01((d - 0.5f) / FringeDepth + jitter + wave);

                    list.Add(new FringeTile { X = x, Y = y, T = t });
                }
            }
            _fringe = list.ToArray();
            _fringeLayer.SetArea(new RectInt(_area.xMin - FringeDepth, _area.yMin - FringeDepth, _area.width + 2 * FringeDepth, _area.height + 2 * FringeDepth));
            RefreshFringe();
        }

        private void RefreshFringe()
        {
            if (_fringe == null) return;
            bool visible = _viewLevel >= 0;
            _fringeLayer.SetVisible(visible);
            if (!visible) return;
            foreach (var f in _fringe)
            {
                var real = IsoArt.Ground(FloorType.Grass, f.X, f.Y);
                var cell = _cell.Reset();
                cell.sprite = real != null ? real : SpriteMaterials.Diamond;
                Color baseColor = real != null ? Color.white : (((f.X + f.Y) & 1) == 0 ? GrassA : GrassB);
                // Bölge kenarı taslakta koyu orman bandıyla biter: dışı açık çim değil, karanlığa inen koyu çim.
                float k = Mathf.Lerp(0.62f, 0.22f, f.T);
                float a = 1f - f.T * f.T * f.T;
                cell.color = new Color(baseColor.r * k, baseColor.g * k, baseColor.b * k, a);
                _fringeLayer.Set(f.X, f.Y, cell);
            }
            _fringeLayer.Flush();
        }

        /// <summary>Bu karo arazi engeli değil (üstündeki eşya engelliyordu); mavi boyanmaz.</summary>
        public void ClearTerrainBlock(GridPos p)
        {
            if (_terrainBlocked != null && Map.InBounds(p)) _terrainBlocked[p.Y * Map.Width + p.X] = false;
        }

        /// <summary>Zemin kaplamalarını haritadan yeniden boyar (Grass dışındakiler döşeme rengi alır).</summary>
        /// <summary>Zeminin tonu: yağmurda ıslak ve koyu (belge 57 §9). Kat katmanları ve harita kenarı birlikte.</summary>
        /// <summary>
        /// Zemin çıkartmaları: verilen işlev karo başına çıkartma sprite'ı döner (yoksa null). Oyun katmanı birkaç saniyede
        /// bir çağırır (yağmur birikintisi, çamur, kıyı köpüğü, yabani ot). Yalnızca zemin katında çizilir.
        /// </summary>
        public void RefreshDecals(System.Func<int, int, Sprite> decalAt)
        {
            if (_decalLayer == null || Map == null) return;
            bool visible = IsLevelVisible(0);
            _decalLayer.SetVisible(visible);
            if (!visible) return;
            for (int y = _area.yMin; y < _area.yMax; y++)
                for (int x = _area.xMin; x < _area.xMax; x++)
                {
                    var c = _decalCell.Reset();
                    c.sprite = decalAt != null ? decalAt(x, y) : null;
                    c.flipX = ((x * 7 + y * 13) & 1) == 1;   // aynı çıkartma her karoda aynı yöne bakmasın
                    _decalLayer.Set(x, y, c);
                }
            _decalLayer.Flush();
        }

        public void SetGroundTint(Color tint)
        {
            if (_layers != null) foreach (var layer in _layers) layer?.SetTint(tint);
            _fringeLayer?.SetTint(tint);
        }

        public void RefreshFloors()
        {
            if (Map == null || _layers == null) return;
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                RefreshLevel(z);
            RefreshFringe();
        }

        private void RefreshLevel(int z)
        {
            if (z == 0) { _pathWang = IsoArt.PathWangReady; RebuildPathLayer(IsLevelVisible(0)); }
            bool visible = IsLevelVisible(z);
            var layer = _layers[z - GridMap.MinLevel];
            // Görünmeyen kat bütünüyle gizlenir; karoları gezilmez (eskiden her karo tek tek kapatılırdı).
            layer.SetVisible(visible);
            if (!visible) return;
            for (int y = _area.yMin; y < _area.yMax; y++)
            {
                for (int x = _area.xMin; x < _area.xMax; x++)
                {
                    var sr = _cell.Reset();
                    PaintTile(sr, x, y, z, visible);
                    layer.Set(x, y, sr);
                    if (z == 0 && _shimmerLayer != null)
                    {
                        var p = new GridPos(x, y, 0);
                        bool water = sr.enabled && sr.sprite != null && Map.GetFloor(p) == FloorType.Water && Map.IsFilled(p) && !Map.FrozenAt(p);
                        var w = _shimmerCell.Reset();
                        w.sprite = water ? sr.sprite : null;
                        w.flipX = sr.flipX;
                        _shimmerLayer.Set(x, y, w);
                    }
                    if (z == 0 && _snowLayer != null)
                    {
                        // Kar yalnızca açıktaki kara karosunu örter (oda içi, su ve kapalı karo hariç).
                        var p = new GridPos(x, y, 0);
                        var room = Map.Rooms.RoomAt(p);
                        bool open = sr.enabled && sr.sprite != null && Map.GetFloor(p) != FloorType.Water && (room == null || !room.IsIndoor);
                        var c = _snowCell.Reset();
                        c.sprite = open ? sr.sprite : null;
                        c.flipX = sr.flipX;
                        _snowLayer.Set(x, y, c);
                    }
                }
            }
            layer.Flush();
            if (z == 0) { _shimmerLayer?.SetVisible(visible); _shimmerLayer?.Flush(); _snowLayer?.SetVisible(visible); _snowLayer?.Flush(); }
        }

        // Bir karonun zemin görünümü (eski karo SpriteRenderer'ının yerine TileCell'e boyanır).
        private void PaintTile(TileCell sr, int x, int y, int z, bool visible)
        {
                {
                    var p = new GridPos(x, y, z);
                    if (z != 0) { RefreshUpperOrLower(sr, p, visible); return; }
                    sr.enabled = visible;
                    if (!visible) return;

                    var floor = Map.GetFloor(p);
                    bool terrainBlocked = _terrainBlocked[y * Map.Width + x];
                    // Gerçek karo varsa (çim, toprak) onu kullan; yoksa yer tutucu elmas + renk.
                    // Gerçek karo yalnızca varsayılan stilde (stil seçilmiş zemin, görseli gelene kadar renkle çizilir).
                    // Deniz kıyısı (belge 50): deniz suyu ve kumsal kendi karosunu arar; yoksa göl suyu ve toprak tonlanır.
                    bool sea = Thatchwick.Core.World.Regions.IsSea(Map, p);
                    bool seaTint = false, beachTint = false, flip = false;
                    Sprite real;
                    if (terrainBlocked) real = null;
                    else if (floor == FloorType.Water)
                    {
                        real = IsoArt.Water(x, y, Map.IsFilled(p), Map.FrozenAt(p), sea, out seaTint);
                        // Kıyı karosu: arkadaki (+x ya da +y) komşu karaysa çamurlu kıyı ve saz; önden bakan kenarlarda karo yok.
                        if (real != null && !sea && Map.IsFilled(p) && !Map.FrozenAt(p))
                        {
                            bool landX = IsLand(x + 1, y), landY = IsLand(x, y + 1);
                            var shore = landX || landY ? IsoArt.Shore(x, y) : null;
                            if (shore != null) { real = shore; flip = !landX; }
                        }
                    }
                    else if (sea && floor == FloorType.Dirt && Map.GetFloorStyle(p) == null)
                    {
                        real = IsoArt.Beach(x, y);
                        if (real == null) { real = IsoArt.Ground(FloorType.Dirt, x, y); beachTint = real != null; }
                    }
                    else real = Map.GetFloorStyle(p) != null ? IsoArt.FloorPart(Map.GetFloorStyle(p), x, y)
                        : IsoArt.Ground(floor, x, y) ?? IsoArt.FloorPart(Content != null ? BuildParts.DefaultFloor(Content, floor)?.Id : null, x, y);
                    sr.sprite = real != null ? real : SpriteMaterials.Diamond;
                    sr.flipX = flip;
                    var styled = real == null ? StyleColor(p, floor) : null;
                    if (real != null) sr.color = seaTint ? new Color(0.78f, 0.9f, 1f) : beachTint ? new Color(1.0f, 0.93f, 0.72f) : Color.white;
                    else if (sea && floor == FloorType.Dirt) sr.color = ((x + y) & 1) == 0 ? new Color32(214, 196, 150, 255) : new Color32(206, 188, 142, 255);
                    else if (sea && floor == FloorType.Water) sr.color = ((x + y) & 1) == 0 ? new Color32(46, 104, 150, 255) : new Color32(42, 98, 142, 255);
                    else if (styled.HasValue) sr.color = styled.Value;
                    else switch (floor)
                    {
                        case FloorType.Wood: sr.color = ((x + y) & 1) == 0 ? new Color32(168, 124, 76, 255) : new Color32(158, 115, 68, 255); break;
                        case FloorType.Stone: sr.color = ((x + y) & 1) == 0 ? new Color32(140, 140, 148, 255) : new Color32(130, 130, 138, 255); break;
                        case FloorType.Dirt: sr.color = ((x + y) & 1) == 0 ? new Color32(139, 110, 80, 255) : new Color32(131, 103, 74, 255); break;
                        case FloorType.Sand: sr.color = ((x + y) & 1) == 0 ? new Color32(222, 200, 150, 255) : new Color32(214, 192, 142, 255); break;
                        case FloorType.Water:   // görseli gelene kadar: kuru çukur çamur, mavi su, donunca soluk buz
                            sr.color = !Map.IsFilled(p)
                                ? (((x + y) & 1) == 0 ? new Color32(104, 84, 62, 255) : new Color32(98, 79, 58, 255))
                                : Map.FrozenAt(p)
                                ? (((x + y) & 1) == 0 ? new Color32(206, 224, 236, 255) : new Color32(196, 216, 230, 255))
                                : (((x + y) & 1) == 0 ? new Color32(64, 120, 176, 255) : new Color32(58, 112, 168, 255));
                            break;
                        default: sr.color = terrainBlocked ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB); break;
                    }
                    var road = Map.GetRoad(p);
                    if (road != RoadType.None)
                    {
                        float roadCond = Map.GetRoadCondition(p);
                        int band = GridMap.ConditionBand(roadCond);
                        bool natural = Map.IsNaturalRoad(p);
                        // Yol karosu (340). Kendiliğinden oluşan patika silikken çim kalır, belirginleşince patika karosuna döner.
                        // Wang seti varsa toprak patika ayrı katmanda (dual-grid) çizilir; bu karo çimen kalır.
                        bool wang = _pathWang && road == RoadType.Path && !(natural && roadCond < 0.5f);
                        var roadArt = wang || natural && roadCond < 0.5f ? null
                            : road == RoadType.Path ? IsoArt.Ground(FloorType.Dirt, x, y) ?? IsoArt.Road(road, band, x, y)   // Wang seti yoksa dikişsiz toprak
                            : IsoArt.Road(road, band, x, y);
                        if (wang)
                        {
                            var grassArt = IsoArt.Ground(FloorType.Grass, x, y);
                            sr.sprite = grassArt != null ? grassArt : SpriteMaterials.Diamond;
                            sr.color = grassArt != null ? Color.white : (((x + y) & 1) == 0 ? GrassA : GrassB);
                        }
                        else if (roadArt != null)
                        {
                            sr.sprite = roadArt;
                            float k = band == 0 ? 1f : band == 1 ? 0.9f : 0.8f;
                            sr.color = new Color(k, k, k, 1f);
                        }
                        else if (natural && real != null && roadCond < 0.5f)
                        {
                            // Silik patika: çim karosu toprağa doğru hafifçe kararır.
                            float t = 0.25f + 0.5f * roadCond;
                            sr.color = Color.Lerp(Color.white, new Color(0.86f, 0.74f, 0.58f), t);
                        }
                        else
                        {
                            // Yol görseli yoksa renk: patika toprak, çakıl kum grisi, kaldırım taş; yıprandıkça koyulaşır.
                            sr.sprite = SpriteMaterials.Diamond;
                            bool even = ((x + y) & 1) == 0;
                            Color rc = road == RoadType.Path ? (Color)(even ? new Color32(150, 122, 88, 255) : new Color32(143, 116, 83, 255))
                                : road == RoadType.Gravel ? (Color)(even ? new Color32(170, 160, 140, 255) : new Color32(162, 152, 133, 255))
                                : (Color)(even ? new Color32(128, 126, 132, 255) : new Color32(120, 118, 124, 255));
                            float cond = Map.GetRoadCondition(p);
                            if (Map.IsNaturalRoad(p)) rc = Color.Lerp(sr.color, rc, 0.4f + 0.6f * cond);       // silik patika çimle karışır
                            else if (GridMap.ConditionBand(cond) == 1) rc *= 0.85f;
                            else if (GridMap.ConditionBand(cond) == 2) rc *= 0.7f;
                            rc.a = 1f;
                            sr.color = rc;
                        }
                    }
                    var conduit = Map.GetConduit(p);
                    if (conduit != ConduitType.None && (Conduits.IsOpen(conduit) || ShowPlot))
                    {
                        // Su yolu görseli gelene kadar renk: açık ark ve oluk hep görünür (ıslakken mavi), gömülü boru
                        // yalnızca yapı modunda (künk kiremit, kütük kahve, kurşun gri).
                        sr.sprite = SpriteMaterials.Diamond;
                        bool wet = WetConduit != null && WetConduit(p);
                        bool frozen = wet == false && Map.WaterFrozen && Conduits.Freezes(conduit);
                        Color bed = conduit == ConduitType.Ditch ? new Color32(112, 88, 62, 255)
                            : conduit == ConduitType.StoneChannel ? new Color32(126, 128, 134, 255)
                            : conduit == ConduitType.Flume ? new Color32(150, 110, 70, 255)
                            : conduit == ConduitType.WoodPipe ? new Color32(120, 84, 50, 255)
                            : conduit == ConduitType.ClayPipe ? new Color32(176, 102, 72, 255)
                            : new Color32(110, 116, 124, 255);
                        Color water = frozen ? new Color32(206, 224, 236, 255) : new Color32(64, 120, 176, 255);
                        var c = Conduits.IsOpen(conduit) ? Color.Lerp(bed, water, wet || frozen ? 0.55f : 0f)
                            : Color.Lerp(sr.color, bed, 0.7f) * (wet ? 1f : 0.8f);
                        c.a = 1f;
                        sr.color = c;
                    }
                    if (Map.GetZone(p) == ZoneType.Storage)
                    {
                        var c = sr.color;
                        sr.color = new Color(c.r * 0.7f + 0.25f, c.g * 0.7f + 0.2f, c.b * 0.7f + 0.05f, 1f); // depo: sarımsı
                    }
                    else if (Map.GetZone(p) == ZoneType.Pasture)
                    {
                        var c = sr.color;
                        sr.color = new Color(c.r * 0.8f + 0.02f, c.g * 0.8f + 0.16f, c.b * 0.8f, 1f);     // otlak: koyu yeşil
                    }
                    if (ShowPlot && !Map.IsOwned(p))
                    {
                        var c = sr.color;
                        sr.color = new Color(c.r * 0.55f, c.g * 0.55f, c.b * 0.6f, 1f);
                    }
                }
        }

        // Patika komşusu: yol, meydan taşı, köprü ya da kapıdan girilen döşeme (arada duvar yoksa). Harita kenarındaki
        // geçit karosu dışarıya bağlı sayılır (yol haritadan çıkıyormuş gibi görünür).
        private bool Connects(GridPos from, GridPos to)
        {
            if (!Map.InBounds(to)) return Map.IsPortal(from);
            if (!_area.Contains(new Vector2Int(to.X, to.Y))) return Map.IsPortal(from);
            if (!Map.EdgeFree(from, to)) return false;
            if (Map.GetRoad(to) != RoadType.None) return true;
            var f = Map.GetFloor(to);
            return f == FloorType.Stone || f == FloorType.Wood;
        }

        /// <summary>Toprak patika: komşulara göre otomatik karo (düz, köşe, T, kavşak, yol sonu). Görsel yoksa null.</summary>
        // ---------------- toprak yol katmanı (dual-grid, art/queue/004e) ----------------
        // Her köşe noktasında (dört ızgara karosunun buluştuğu yer) bir geçiş karosu durur. Karonun dört köşesi o noktaya
        // değen dört karodan gelir: üst = (x+1, y+1), sağ = (x+1, y), alt = (x, y), sol = (x, y+1) (Iso.GridToWorld).
        // Yol karosu toprak sayılır; meydan taşı, köprü ve kapı döşemesi yalnızca bir yola değiyorsa toprak sayılır
        // (yol oraya bağlanır, ama meydanın içi boyanmaz). Çeşit (a/b/c) köşe noktasının konumundan sabit gürültüyle
        // seçilir: uzun yolda aynı kenar tekrar etmez, iki kenar birbirinden bağımsızdır.
        private bool _pathWang;
        private readonly System.Collections.Generic.List<GameObject> _pathObjects = new System.Collections.Generic.List<GameObject>();

        private bool IsDirtPath(int x, int y)
        {
            var q = new GridPos(x, y);
            if (!Map.InBounds(q) || !_area.Contains(new Vector2Int(x, y))) return false;
            if (Map.GetRoad(q) != RoadType.Path) return false;
            return !(Map.IsNaturalRoad(q) && Map.GetRoadCondition(q) < 0.5f);
        }

        private bool IsPathJoin(int x, int y)
        {
            var q = new GridPos(x, y);
            if (!Map.InBounds(q) || !_area.Contains(new Vector2Int(x, y))) return false;
            var f = Map.GetFloor(q);
            return f == FloorType.Stone || f == FloorType.Wood;
        }

        private void RebuildPathLayer(bool visible)
        {
            foreach (var go in _pathObjects) if (go != null) Destroy(go);
            _pathObjects.Clear();
            if (visible && Map != null) BuildTransitions();
            if (!_pathWang || !visible || Map == null) return;
            for (int vy = _area.yMin - 1; vy < _area.yMax; vy++)
                for (int vx = _area.xMin - 1; vx < _area.xMax; vx++)
                {
                    // Köşe noktası (vx+1, vy+1): dört karo (vx, vy), (vx+1, vy), (vx, vy+1), (vx+1, vy+1).
                    bool top = IsDirtPath(vx + 1, vy + 1), right = IsDirtPath(vx + 1, vy), bottom = IsDirtPath(vx, vy), left = IsDirtPath(vx, vy + 1);
                    if (!top && !right && !bottom && !left) continue;
                    // Yola bağlanan taş/ahşap döşeme köşesi toprak sayılır (geçiş oraya doğru açılır).
                    top |= IsPathJoin(vx + 1, vy + 1); right |= IsPathJoin(vx + 1, vy);
                    bottom |= IsPathJoin(vx, vy); left |= IsPathJoin(vx, vy + 1);
                    int mask = (top ? 1 : 0) | (right ? 2 : 0) | (bottom ? 4 : 0) | (left ? 8 : 0);
                    int variant = (int)(Noise(vx, vy, 404) * IsoArt.PathWangVariants) % IsoArt.PathWangVariants;
                    var sprite = IsoArt.PathWang(mask, variant);
                    if (sprite == null) continue;
                    var go = new GameObject("PathWang");
                    go.transform.SetParent(transform, false);
                    go.transform.position = Iso.GridToWorld(vx + 1f, vy + 1f, 0);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sharedMaterial = SpriteMaterials.Unlit;
                    sr.sortingOrder = GroundSortOrder + 1;
                    _pathObjects.Add(go);
                }
        }

        /// <summary>Geçiş seti çiftleri (birinci, ikinci): dosya adındaki sırayla.</summary>
        private static readonly (string a, string b)[] TransitionPairs = { ("grass", "sand"), ("grass", "stone"), ("sand", "water"), ("grass", "water"), ("grass", "meadow") };

        /// <summary>Karonun geçiş malzemesi (kat 0): çim, kum, açık havadaki taş döşeme, su; yol ve diğerleri null.</summary>
        private string TransitionKind(int x, int y)
        {
            var p = new GridPos(x, y);
            if (!Map.InBounds(p) || Map.GetRoad(p) != RoadType.None) return null;
            switch (Map.GetFloor(p))
            {
                case FloorType.Grass: return IsoArt.IsMeadow(x, y) && Map.GetFloorStyle(p) == null ? "meadow" : "grass";
                case FloorType.Sand: return "sand";
                case FloorType.Water: return "water";
                case FloorType.Stone:
                    var room = Map.Rooms.RoomAt(p);
                    return room == null || !room.IsIndoor ? "stone" : null;
                default: return null;
            }
        }

        /// <summary>
        /// Zemin geçişleri (613-617, dual-grid): iki malzemenin buluştuğu her köşe noktasına maskeli geçiş karosu. Görseli olmayan
        /// çift ya da maske çizilmez (taban karolar olduğu gibi kalır).
        /// </summary>
        private void BuildTransitions()
        {
            for (int vy = _area.yMin - 1; vy < _area.yMax; vy++)
                for (int vx = _area.xMin - 1; vx < _area.xMax; vx++)
                {
                    string top = TransitionKind(vx + 1, vy + 1), right = TransitionKind(vx + 1, vy), bottom = TransitionKind(vx, vy), left = TransitionKind(vx, vy + 1);
                    if (top == null || right == null || bottom == null || left == null) continue;
                    if (top == right && right == bottom && bottom == left) continue;
                    foreach (var (a, b) in TransitionPairs)
                    {
                        bool Ok(string k) => k == a || k == b;
                        if (!Ok(top) || !Ok(right) || !Ok(bottom) || !Ok(left)) continue;
                        int mask = (top == b ? 1 : 0) | (right == b ? 2 : 0) | (bottom == b ? 4 : 0) | (left == b ? 8 : 0);
                        var sprite = IsoArt.Transition(a, b, mask, (int)(Noise(vx, vy, 613) * IsoArt.TransitionVariants) % IsoArt.TransitionVariants);
                        if (sprite == null) break;
                        var go = new GameObject("Transition");
                        go.transform.SetParent(transform, false);
                        go.transform.position = Iso.GridToWorld(vx + 1f, vy + 1f, 0);
                        var sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = sprite;
                        sr.sharedMaterial = SpriteMaterials.Unlit;
                        sr.sortingOrder = GroundSortOrder + 1;
                        _pathObjects.Add(go);
                        break;
                    }
                }
        }

        // Yol kenarı geçişi Wang setinin kendi karolarıyla (dual-grid) çizilir; kodla üretilen kenar denemesi (ProceduralPathFringe) kaldırıldı.
        /// <summary>Konuma bağlı sabit gürültü (0..1): her yenilemede aynı desen.</summary>
        private static float Noise(int x, int y, int salt)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + salt * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        // Üst kat döşemesi ve bodrum: yer tutucu elmaslar. Kazılmamış toprak (bodrum görünümünde) koyu çizilir;
        // yapı modunda boş üst kat düzlemi çok soluk gösterilir ki dokunulacak yer görünsün.
        private void RefreshUpperOrLower(TileCell sr, GridPos p, bool visible)
        {
            bool has = Map.HasTile(p);
            bool isViewPlane = p.Z == _viewLevel;
            bool show = visible && (has || (p.Z > 0 ? ShowPlot && isViewPlane : isViewPlane));
            if (!show)
            {
                sr.enabled = false;
                return;
            }
            sr.enabled = true;
            sr.sprite = SpriteMaterials.Diamond;
            if (!has)
            {
                sr.color = p.Z < 0 ? EarthColor : VoidColor;
                if (p.Z < 0 && ShowPlot && Map.IsOwned(p)) sr.color = new Color32(52, 42, 36, 255);
                // Arsa kazısı (kat -2) ve tepenin altındaki galeri (kat -1): kaya karoları türüne ve durumuna göre boyanır.
                if (p.Z < 0 && RockArt != null && RockArt(p) is (string art, Color rockTint))
                {
                    var top = IsoArt.RockTop(art, p.X, p.Y);
                    if (top != null) { sr.sprite = top; sr.color = rockTint; return; }
                }
                if (p.Z < 0 && RockTint != null) { var tint = RockTint(p); if (tint.HasValue) sr.color = tint.Value; }
                return;
            }
            var floor = Map.GetFloor(p);
            bool even = ((p.X + p.Y) & 1) == 0;
            var styled = StyleColor(p, floor);
            if (styled.HasValue) sr.color = styled.Value;
            else switch (floor)
            {
                case FloorType.Stone: sr.color = even ? new Color32(140, 140, 148, 255) : new Color32(130, 130, 138, 255); break;
                case FloorType.Dirt: sr.color = even ? new Color32(120, 94, 68, 255) : new Color32(112, 88, 62, 255); break;
                default: sr.color = even ? new Color32(168, 124, 76, 255) : new Color32(158, 115, 68, 255); break;
            }
            if (Map.GetZone(p) == ZoneType.Storage)
            {
                var c = sr.color;
                sr.color = new Color(c.r * 0.7f + 0.25f, c.g * 0.7f + 0.2f, c.b * 0.7f + 0.05f, 1f);
            }
        }

        /// <summary>Karonun merkezinin dünya konumu (kat yüksekliği dahil).</summary>
        /// <summary>Kıyı karosu için: komşu karo kara mı (harita içinde ve su değil).</summary>
        private bool IsLand(int x, int y) => x >= 0 && y >= 0 && x < Map.Width && y < Map.Height && Map.GetFloor(new GridPos(x, y, 0)) != FloorType.Water;

        public Vector3 TileToWorld(GridPos p) => Iso.GridToWorld(p.X + 0.5f, p.Y + 0.5f, p.Z);

        /// <summary>Dünya noktasının ViewLevel katındaki karosu (dokunma bu katta çözülür).</summary>
        public GridPos WorldToTile(Vector3 world) => WorldToTile(world, _viewLevel);

        public GridPos WorldToTile(Vector3 world, int level)
        {
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y), level);
            return new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y), level);
        }
    }
}
