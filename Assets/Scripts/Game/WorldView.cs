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

        public GridMap Map { get; private set; }

        public void Build(GridMap map)
        {
            Map = map;

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
                    sr.color = map.IsBlocked(new GridPos(x, y)) ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB);
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
