using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// İzometrik (2:1 dimetrik) izdüşüm. 1 dünya birimi = 32 piksel (PPU 32);
    /// bir karo elmasının boyu 64x32 piksel, yani 2x1 dünya birimi.
    /// Izgara +x ekranda sağ-aşağı, +y sol-aşağı gider.
    /// </summary>
    public static class Iso
    {
        public const float HalfWidth = 1f;
        public const float HalfHeight = 0.5f;

        public static Vector3 GridToWorld(float gx, float gy) =>
            new Vector3((gx - gy) * HalfWidth, (gx + gy) * HalfHeight, 0f);

        public static Vector2 WorldToGrid(Vector2 world) => new Vector2(
            (world.x / HalfWidth + world.y / HalfHeight) * 0.5f,
            (world.y / HalfHeight - world.x / HalfWidth) * 0.5f);

        /// <summary>Harita (width x height karo) elmasının dünya sınırları (kamera kısıtı için).</summary>
        public static Rect MapBounds(int width, int height) =>
            new Rect(-height * HalfWidth, 0f, (width + height) * HalfWidth, (width + height) * HalfHeight);

        /// <summary>Ön tarafta (ekranda aşağıda) olan daha yüksek sıra alır.</summary>
        public static int SortOrder(float worldY, int bias = 0) => -Mathf.RoundToInt(worldY * 20f) + bias;
    }
}
