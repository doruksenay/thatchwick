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

        /// <summary>Bir katın yüksekliği (dünya birimi; duvar görseli ~3 birim boyunda).</summary>
        public const float LevelHeight = 3f;

        /// <summary>
        /// Katlar arası çizim sırası farkı. SpriteRenderer sıra değeri short'tur (±32767): zemin kat nesneleri
        /// yaklaşık -1100..1100 arasındadır; üst kat bunların hepsinin üstünde, bodrum altında kalır.
        /// </summary>
        public const int LevelSortStep = 14000;

        public static Vector3 GridToWorld(float gx, float gy) =>
            new Vector3((gx - gy) * HalfWidth, (gx + gy) * HalfHeight, 0f);

        /// <summary>Kat yüksekliği eklenmiş dünya konumu (üst kat yukarı, bodrum aşağı çizilir).</summary>
        public static Vector3 GridToWorld(float gx, float gy, int level) =>
            new Vector3((gx - gy) * HalfWidth, (gx + gy) * HalfHeight + level * LevelHeight, 0f);

        public static Vector2 WorldToGrid(Vector2 world) => new Vector2(
            (world.x / HalfWidth + world.y / HalfHeight) * 0.5f,
            (world.y / HalfHeight - world.x / HalfWidth) * 0.5f);

        /// <summary>Verilen katın düzlemine denk gelen ızgara noktası (kat yüksekliği düşülür).</summary>
        public static Vector2 WorldToGrid(Vector2 world, int level) =>
            WorldToGrid(new Vector2(world.x, world.y - level * LevelHeight));

        /// <summary>Harita (width x height karo) elmasının dünya sınırları (kamera kısıtı için); üst kat ve bodrum payı dahil.</summary>
        public static Rect MapBounds(int width, int height) =>
            new Rect(-height * HalfWidth, -LevelHeight, (width + height) * HalfWidth, (width + height) * HalfHeight + 2f * LevelHeight);

        /// <summary>Duvar eşyasının asılma yüksekliği (dünya birimi; duvar görseli ~3 birim boyunda).</summary>
        public const float WallMountHeight = 1.3f;

        /// <summary>Duvar bandının yüksekliği (27-ev-dekorasyonu): alt bel altı, orta göz hizası, üst kiriş altı.</summary>
        public static float BandHeight(int band) => band <= 0 ? 0.6f : band == 1 ? WallMountHeight : 2.1f;

        /// <summary>Tavan eşyasının yüksekliği (karonun ortasından; kat yüksekliğinin altında).</summary>
        public const float CeilingHeight = 2.5f;

        /// <summary>Geniş duvar eşyasının ortası (ilk kenarın başı ile son kenarın sonu arası), bandının yüksekliğinde.</summary>
        public static Vector3 WallMountSpan(Thatchwick.Core.World.GridPos tile, int side, int span, int band)
        {
            float extra = BandHeight(band) - WallMountHeight;
            if (span <= 1) return WallMount(tile, side, 0.5f, extra);
            var (dx, dy) = Thatchwick.Core.World.WallSides.SegmentOffset(side, span - 1);
            var last = new Thatchwick.Core.World.GridPos(tile.X + dx, tile.Y + dy, tile.Z);
            return (WallMount(tile, side, 0f, extra) + WallMount(last, side, 1f, extra)) * 0.5f;
        }

        /// <summary>
        /// Karonun bir kenarına (WallSides) asılı eşyanın dünya konumu: kenar boyunca along (0-1) noktası,
        /// duvardan biraz içeride, asılma yüksekliğinde.
        /// </summary>
        public static Vector3 WallMount(Thatchwick.Core.World.GridPos tile, int side, float along = 0.5f, float extraHeight = 0f)
        {
            var (ax, ay) = Thatchwick.Core.World.WallSides.AlongWall(side, along);
            return GridToWorld(tile.X + ax, tile.Y + ay, tile.Z) + new Vector3(0f, WallMountHeight + extraHeight, 0f);
        }

        /// <summary>
        /// Duvar eşyasının çizim sırası. Kenar 1 (+x) ve 2 (+y) karonun arkasındaki duvardır: eşya kameraya
        /// bakar, duvarın önünde çizilir. Kenar 0 ve 3 öndeki duvarın iç yüzüdür: duvarın arkasında kalır
        /// (kesit modunda saydam duvarın ardından görünür).
        /// </summary>
        public static int WallMountOrder(Thatchwick.Core.World.GridPos tile, int side)
        {
            var e = Thatchwick.Core.World.WallSides.EdgeOf(tile, side);
            var mid = GridToWorld(e.X + (e.Axis == Thatchwick.Core.World.EdgeAxis.Horizontal ? 0.5f : 0f), e.Y + (e.Axis == Thatchwick.Core.World.EdgeAxis.Vertical ? 0.5f : 0f), tile.Z);
            int n = ((side % 4) + 4) % 4;
            return SortOrder(mid.y, n == 1 || n == 2 ? 2 : -2, tile.Z);
        }

        /// <summary>Ön tarafta (ekranda aşağıda) olan daha yüksek sıra alır.</summary>
        public static int SortOrder(float worldY, int bias = 0) => -Mathf.RoundToInt(worldY * 20f) + bias;

        /// <summary>Kat farkındalıklı sıra: dünya Y'sinden kat yüksekliği düşülür, kata göre bant eklenir.</summary>
        public static int SortOrder(float worldY, int bias, int level) =>
            -Mathf.RoundToInt((worldY - level * LevelHeight) * 20f) + bias + level * LevelSortStep;
    }
}
