using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Sabit adımlı simülasyonun görünüm ara değeri: her simülasyon adımında hedef konum itilir, her karede
    /// önceki ile son konum arası <c>alpha</c> ile çizilir. Büyük sıçrama (harita geçişi, ışınlanma) ara değerlenmez.
    /// </summary>
    public struct SmoothPosition
    {
        /// <summary>Bu kadar dünya biriminden uzun sıçramada ara değer yapılmaz (karo 2x1 birim).</summary>
        public const float SnapDistance = 3f;

        public Vector3 Previous;
        public Vector3 Current;
        public bool HasValue;

        public void Push(Vector3 position)
        {
            if (!HasValue)
            {
                Previous = Current = position;
                HasValue = true;
                return;
            }
            float dx = position.x - Current.x, dy = position.y - Current.y;
            Previous = dx * dx + dy * dy > SnapDistance * SnapDistance ? position : Current;
            Current = position;
        }

        public Vector3 At(float alpha) => HasValue ? Vector3.Lerp(Previous, Current, alpha) : Current;

        public void Reset() => HasValue = false;
    }
}
