using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Art
{
    /// <summary>
    /// Görsel atlası için raf (shelf) yerleştirici (performans incelemesi 20.11.2026): dikdörtgenleri sabit boyutlu
    /// sayfalara piksel tam yerleştirir (ölçekleme yok; Texture2D.PackTextures sığmayanı küçültürdü). Belirleyicidir:
    /// aynı girdi aynı yerleşimi verir. UnityEngine yok; editör betiği kullanır, test edilebilir.
    /// </summary>
    public static class ShelfPacker
    {
        public struct Placement
        {
            public int Page;
            public int X, Y;          // sol-alt köşe (piksel), dolgu hariç
            public bool Packed;       // sayfaya sığmayan (çok büyük) görsel atlas dışında kalır
        }

        /// <param name="sizes">Genişlik ve yükseklikler (girdi sırası korunur; çağıran gruplayıp sıralar).</param>
        /// <param name="pageSize">Sayfa kenarı (piksel).</param>
        /// <param name="padding">Görseller arası boşluk (piksel; Point süzgecinde komşu taşmasın).</param>
        public static Placement[] Pack(IReadOnlyList<(int w, int h)> sizes, int pageSize, int padding, out int pageCount)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (padding < 0) throw new ArgumentOutOfRangeException(nameof(padding));
            var result = new Placement[sizes.Count];
            int page = 0, x = padding, y = padding, shelfHeight = 0;
            bool pageUsed = false;
            for (int i = 0; i < sizes.Count; i++)
            {
                var (w, h) = sizes[i];
                if (w <= 0 || h <= 0 || w + 2 * padding > pageSize || h + 2 * padding > pageSize) continue;   // atlas dışı
                if (x + w + padding > pageSize)
                {
                    // Raf doldu: bir üst rafa geç.
                    x = padding;
                    y += shelfHeight + padding;
                    shelfHeight = 0;
                }
                if (y + h + padding > pageSize)
                {
                    // Sayfa doldu.
                    page++;
                    x = padding;
                    y = padding;
                    shelfHeight = 0;
                }
                result[i] = new Placement { Page = page, X = x, Y = y, Packed = true };
                pageUsed = true;
                x += w + padding;
                shelfHeight = Math.Max(shelfHeight, h);
            }
            pageCount = pageUsed ? page + 1 : 0;
            return result;
        }
    }
}
