using UnityEngine;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// GridMap'i tek bir doku olarak çizer (1 karo = 1 piksel = 1 dünya birimi).
    /// Geçici görünüm: asset hattı (A1) gelince gerçek karo sprite'larıyla değişir.
    /// </summary>
    public sealed class WorldView : MonoBehaviour
    {
        private static readonly Color32 GrassA = new Color32(92, 140, 77, 255);
        private static readonly Color32 GrassB = new Color32(86, 133, 72, 255);
        private static readonly Color32 BlockedColor = new Color32(58, 74, 92, 255);

        private Texture2D _texture;
        private SpriteRenderer _renderer;

        public GridMap Map { get; private set; }

        public void Build(GridMap map)
        {
            Map = map;

            _texture = new Texture2D(map.Width, map.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "WorldTexture"
            };

            var pixels = new Color32[map.Width * map.Height];
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    bool blocked = map.IsBlocked(new GridPos(x, y));
                    pixels[y * map.Width + x] = blocked ? BlockedColor : (((x + y) & 1) == 0 ? GrassA : GrassB);
                }
            }

            _texture.SetPixels32(pixels);
            _texture.Apply(false);

            // Pivot sol alt, 1 piksel = 1 birim: sprite (0,0)-(width,height) aralığını kaplar.
            var sprite = Sprite.Create(_texture, new Rect(0, 0, map.Width, map.Height), Vector2.zero, 1f);

            _renderer = gameObject.GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = 0;
            _renderer.sharedMaterial = SpriteMaterials.Unlit;
        }

        public Vector3 TileToWorld(GridPos p) => new Vector3(p.X + 0.5f, p.Y + 0.5f, 0f);

        public GridPos WorldToTile(Vector3 world) =>
            new GridPos(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
    }
}
