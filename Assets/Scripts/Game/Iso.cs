using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// İzometrik (2:1 dimetrik) izdüşüm. 1 dünya birimi = 32 piksel (PPU 32);
    /// bir karo elmasının boyu 64x32 piksel, yani 2x1 dünya birimi.
    /// Izgara +x ekranda sağ-yukarı, +y sol-yukarı gider (dünya Y'si yukarı). Ekranda aşağıda
    /// (küçük x+y) olan öndedir ve üste çizilir.
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

        /// <summary>Duvar eşyasının asılma yüksekliği (dünya birimi; duvar görseli ~3 birim boyunda).</summary>
        public const float WallMountHeight = 1.3f;

        /// <summary>
        /// Karonun bir kenarına (WallSides) asılı eşyanın dünya konumu: kenar boyunca along (0-1) noktası,
        /// duvardan biraz içeride, asılma yüksekliğinde.
        /// </summary>
        public static Vector3 WallMount(Thatchwick.Core.World.GridPos tile, int side, float along = 0.5f, float extraHeight = 0f)
        {
            var (ax, ay) = Thatchwick.Core.World.WallSides.AlongWall(side, along);
            return GridToWorld(tile.X + ax, tile.Y + ay) + new Vector3(0f, WallMountHeight + extraHeight, 0f);
        }

        /// <summary>
        /// Duvar eşyasının çizim sırası. Kenar 1 (+x) ve 2 (+y) karonun arkasındaki duvardır: eşya kameraya
        /// bakar, duvarın önünde çizilir. Kenar 0 ve 3 öndeki duvarın iç yüzüdür: duvarın arkasında kalır
        /// (kesit modunda saydam duvarın ardından görünür).
        /// </summary>
        public static int WallMountOrder(Thatchwick.Core.World.GridPos tile, int side)
        {
            var e = Thatchwick.Core.World.WallSides.EdgeOf(tile, side);
            var mid = GridToWorld(e.X + (e.Axis == Thatchwick.Core.World.EdgeAxis.Horizontal ? 0.5f : 0f), e.Y + (e.Axis == Thatchwick.Core.World.EdgeAxis.Vertical ? 0.5f : 0f));
            int n = ((side % 4) + 4) % 4;
            return SortOrder(mid.y, n == 1 || n == 2 ? 2 : -2);
        }

        /// <summary>Ön tarafta (ekranda aşağıda) olan daha yüksek sıra alır.</summary>
        public static int SortOrder(float worldY, int bias = 0) => -Mathf.RoundToInt(worldY * 20f) + bias;
    }
}
