using UnityEngine;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// GridMap'i izometrik elmas karolar olarak çizer. Yer tutucu: asset hattı (A1) gelince
    /// gerçek zemin sprite'larıyla değişir. Zemin her zaman her şeyin arkasında çizilir.
    /// </summary>
    public sealed class WorldView : MonoBehaviour
    {
        private const int GroundSortOrder = -10000;

        private static readonly Color GrassA = new Color32(92, 140, 77, 255);
        private static readonly Color GrassB = new Color32(86, 133, 72, 255);
        private static readonly Color BlockedColor = new Color32(58, 74, 92, 255);

        private SpriteRenderer[] _tiles;

        /// <summary>Yapı modunda sahipsiz karolar koyulaştırılır.</summary>
        public bool ShowPlot;
        private bool[] _terrainBlocked;

        public GridMap Map { get; private set; }

        public void Build(GridMap map)
        {
            Map = map;
            _tiles = new SpriteRenderer[map.Width * map.Height];
            _terrainBlocked = new bool[map.Width * map.Height];

            var diamond = SpriteMaterials.Diamond;
            var material = SpriteMaterials.Unlit;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var go = new GameObject($"Tile {x},{y}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = TileToWorld(new GridPos(x, y));

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = diamond;
                    sr.sharedMaterial = material;
                    sr.sortingOrder = GroundSortOrder;
                    _tiles[y * map.Width + x] = sr;
                    _terrainBlocked[y * map.Width + x] = map.IsBlocked(new GridPos(x, y));
                    sr.color = map.IsBlocked(new GridPos(x, y)) ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB);
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
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    var sr = _tiles[y * Map.Width + x];
                    if (sr == null) continue;
                    var p = new GridPos(x, y);
                    switch (Map.GetFloor(p))
                    {
                        case FloorType.Wood: sr.color = ((x + y) & 1) == 0 ? new Color32(168, 124, 76, 255) : new Color32(158, 115, 68, 255); break;
                        case FloorType.Stone: sr.color = ((x + y) & 1) == 0 ? new Color32(140, 140, 148, 255) : new Color32(130, 130, 138, 255); break;
                        case FloorType.Dirt: sr.color = ((x + y) & 1) == 0 ? new Color32(139, 110, 80, 255) : new Color32(131, 103, 74, 255); break;
                        default: sr.color = _terrainBlocked[y * Map.Width + x] ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB); break;
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

        /// <summary>Karonun merkezinin dünya konumu.</summary>
        public Vector3 TileToWorld(GridPos p) => Iso.GridToWorld(p.X + 0.5f, p.Y + 0.5f);

        public GridPos WorldToTile(Vector3 world)
        {
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y));
            return new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y));
        }
    }
}
