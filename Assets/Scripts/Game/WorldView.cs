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

        private static readonly Color GrassA = new Color32(92, 140, 77, 255);
        private static readonly Color GrassB = new Color32(86, 133, 72, 255);
        private static readonly Color BlockedColor = new Color32(58, 74, 92, 255);
        private static readonly Color EarthColor = new Color32(38, 30, 26, 255);
        private static readonly Color VoidColor = new Color(1f, 1f, 1f, 0.07f);

        /// <summary>Haritanın dışı (kamera arka planı): kenar şeridi bu renge solar.</summary>
        public static readonly Color OutsideColor = new Color32(22, 31, 24, 255);
        /// <summary>Harita kenarındaki solan çim şeridinin derinliği (karo). Sert kenar yerine koyu ormana karışır.</summary>
        private const int FringeDepth = 7;

        private struct FringeTile { public SpriteRenderer R; public int X, Y; public float T; }
        private FringeTile[] _fringe;

        // Kat başına bir karo dizisi: dizin ((z - MinLevel) * H + y) * W + x.
        private SpriteRenderer[] _tiles;

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

        /// <summary>Bu katın nesneleri (eşya, duvar, karakter) şu an çizilmeli mi.</summary>
        public bool IsLevelVisible(int z) => _viewLevel < 0 ? z == _viewLevel : z >= 0 && z <= _viewLevel;

        private int TileIndex(int x, int y, int z) => ((z - GridMap.MinLevel) * Map.Height + y) * Map.Width + x;

        public void Build(GridMap map)
        {
            Map = map;
            int plane = map.Width * map.Height;
            _tiles = new SpriteRenderer[plane * GridMap.LevelCount];
            _terrainBlocked = new bool[plane];

            var diamond = SpriteMaterials.Diamond;
            var material = SpriteMaterials.Unlit;

            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        var p = new GridPos(x, y, z);
                        var go = new GameObject(z == 0 ? $"Tile {x},{y}" : $"Tile {x},{y},{z}");
                        go.transform.SetParent(transform, false);
                        go.transform.position = TileToWorld(p);

                        var sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = diamond;
                        sr.sharedMaterial = material;
                        sr.sortingOrder = GroundSortOrder + z * Iso.LevelSortStep;
                        _tiles[TileIndex(x, y, z)] = sr;
                        if (z == 0)
                        {
                            _terrainBlocked[y * map.Width + x] = map.IsBlocked(p);
                            sr.color = map.IsBlocked(p) ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB);
                        }
                        else sr.enabled = false;
                    }
                }
            }
            BuildFringe();
        }

        // Harita kenarı (görsel analiz, 18.11.2026): haritanın çevresine, dışa doğru koyulaşıp saydamlaşan bir çim şeridi.
        // Yalnızca görsel; oyun mantığında karşılığı yok. Orman sınırı görseli gelince (art/queue/370) üstüne ağaç dizilecek.
        private void BuildFringe()
        {
            var list = new System.Collections.Generic.List<FringeTile>();
            for (int y = -FringeDepth; y < Map.Height + FringeDepth; y++)
            {
                for (int x = -FringeDepth; x < Map.Width + FringeDepth; x++)
                {
                    if (x >= 0 && y >= 0 && x < Map.Width && y < Map.Height) continue;
                    int dx = x < 0 ? -x : (x >= Map.Width ? x - Map.Width + 1 : 0);
                    int dy = y < 0 ? -y : (y >= Map.Height ? y - Map.Height + 1 : 0);
                    int d = Mathf.Max(dx, dy);
                    // Halkalar düz görünmesin diye karoya sabit küçük bir sapma.
                    int h = (x * 92837111) ^ (y * 689287499);
                    float jitter = ((h & 0xFF) / 255f - 0.5f) * 0.12f;
                    float t = Mathf.Clamp01((d - 0.5f) / FringeDepth + jitter);

                    var go = new GameObject($"Edge {x},{y}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = TileToWorld(new GridPos(x, y, 0));
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sharedMaterial = SpriteMaterials.Unlit;
                    sr.sortingOrder = GroundSortOrder - 1;
                    list.Add(new FringeTile { R = sr, X = x, Y = y, T = t });
                }
            }
            _fringe = list.ToArray();
            RefreshFringe();
        }

        private void RefreshFringe()
        {
            if (_fringe == null) return;
            bool visible = _viewLevel >= 0;
            foreach (var f in _fringe)
            {
                f.R.enabled = visible;
                if (!visible) continue;
                var real = IsoArt.Ground(FloorType.Grass, f.X, f.Y);
                f.R.sprite = real != null ? real : SpriteMaterials.Diamond;
                Color baseColor = real != null ? Color.white : (((f.X + f.Y) & 1) == 0 ? GrassA : GrassB);
                float k = Mathf.Lerp(0.92f, 0.45f, f.T);
                float a = 1f - f.T * f.T;
                f.R.color = new Color(baseColor.r * k, baseColor.g * k, baseColor.b * k, a);
            }
        }

        /// <summary>Bu karo arazi engeli değil (üstündeki eşya engelliyordu); mavi boyanmaz.</summary>
        public void ClearTerrainBlock(GridPos p)
        {
            if (_terrainBlocked != null && Map.InBounds(p)) _terrainBlocked[p.Y * Map.Width + p.X] = false;
        }

        /// <summary>Zemin kaplamalarını haritadan yeniden boyar (Grass dışındakiler döşeme rengi alır).</summary>
        public void RefreshFloors()
        {
            if (Map == null || _tiles == null) return;
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                RefreshLevel(z);
            RefreshFringe();
        }

        private void RefreshLevel(int z)
        {
            bool visible = IsLevelVisible(z);
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    var sr = _tiles[TileIndex(x, y, z)];
                    if (sr == null) continue;
                    var p = new GridPos(x, y, z);
                    if (z != 0) { RefreshUpperOrLower(sr, p, visible); continue; }
                    sr.enabled = visible;
                    if (!visible) continue;

                    var floor = Map.GetFloor(p);
                    bool terrainBlocked = _terrainBlocked[y * Map.Width + x];
                    // Gerçek karo varsa (çim, toprak) onu kullan; yoksa yer tutucu elmas + renk.
                    // Gerçek karo yalnızca varsayılan stilde (stil seçilmiş zemin, görseli gelene kadar renkle çizilir).
                    var real = terrainBlocked || floor == FloorType.Water ? null
                        : Map.GetFloorStyle(p) != null ? IsoArt.FloorPart(Map.GetFloorStyle(p), x, y)
                        : IsoArt.Ground(floor, x, y) ?? IsoArt.FloorPart(Content != null ? BuildParts.DefaultFloor(Content, floor)?.Id : null, x, y);
                    sr.sprite = real != null ? real : SpriteMaterials.Diamond;
                    var styled = real == null ? StyleColor(p, floor) : null;
                    if (real != null) sr.color = Color.white;
                    else if (styled.HasValue) sr.color = styled.Value;
                    else switch (floor)
                    {
                        case FloorType.Wood: sr.color = ((x + y) & 1) == 0 ? new Color32(168, 124, 76, 255) : new Color32(158, 115, 68, 255); break;
                        case FloorType.Stone: sr.color = ((x + y) & 1) == 0 ? new Color32(140, 140, 148, 255) : new Color32(130, 130, 138, 255); break;
                        case FloorType.Dirt: sr.color = ((x + y) & 1) == 0 ? new Color32(139, 110, 80, 255) : new Color32(131, 103, 74, 255); break;
                        case FloorType.Water:   // görseli gelene kadar: kuru çukur çamur, mavi su, donunca soluk buz
                            sr.color = !Map.IsFilled(p)
                                ? (((x + y) & 1) == 0 ? new Color32(104, 84, 62, 255) : new Color32(98, 79, 58, 255))
                                : Map.WaterFrozen
                                ? (((x + y) & 1) == 0 ? new Color32(206, 224, 236, 255) : new Color32(196, 216, 230, 255))
                                : (((x + y) & 1) == 0 ? new Color32(64, 120, 176, 255) : new Color32(58, 112, 168, 255));
                            break;
                        default: sr.color = terrainBlocked ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB); break;
                    }
                    var road = Map.GetRoad(p);
                    if (road != RoadType.None)
                    {
                        // Yol görseli gelene kadar renk: patika toprak, çakıl kum grisi, kaldırım taş; yıprandıkça koyulaşır.
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
        }

        // Üst kat döşemesi ve bodrum: yer tutucu elmaslar. Kazılmamış toprak (bodrum görünümünde) koyu çizilir;
        // yapı modunda boş üst kat düzlemi çok soluk gösterilir ki dokunulacak yer görünsün.
        private void RefreshUpperOrLower(SpriteRenderer sr, GridPos p, bool visible)
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
