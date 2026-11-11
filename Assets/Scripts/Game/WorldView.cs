using System;
using UnityEngine;
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

        // Kat başına bir karo dizisi: dizin ((z - MinLevel) * H + y) * W + x.
        private SpriteRenderer[] _tiles;

        /// <summary>Yapı modunda sahipsiz karolar koyulaştırılır.</summary>
        public bool ShowPlot;
        private bool[] _terrainBlocked;
        private int _viewLevel;

        public GridMap Map { get; private set; }

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
                    var real = terrainBlocked ? null : IsoArt.Ground(floor, x, y);
                    sr.sprite = real != null ? real : SpriteMaterials.Diamond;
                    if (real != null) sr.color = Color.white;
                    else switch (floor)
                    {
                        case FloorType.Wood: sr.color = ((x + y) & 1) == 0 ? new Color32(168, 124, 76, 255) : new Color32(158, 115, 68, 255); break;
                        case FloorType.Stone: sr.color = ((x + y) & 1) == 0 ? new Color32(140, 140, 148, 255) : new Color32(130, 130, 138, 255); break;
                        case FloorType.Dirt: sr.color = ((x + y) & 1) == 0 ? new Color32(139, 110, 80, 255) : new Color32(131, 103, 74, 255); break;
                        default: sr.color = terrainBlocked ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB); break;
                    }
                    if (Map.GetZone(p) == ZoneType.Storage)
                    {
                        var c = sr.color;
                        sr.color = new Color(c.r * 0.7f + 0.25f, c.g * 0.7f + 0.2f, c.b * 0.7f + 0.05f, 1f); // depo: sarımsı
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
            switch (floor)
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
