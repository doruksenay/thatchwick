using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>Gölge nesneleri (görsel analiz, 18.11.2026): zeminin ve halıların üstünde, eşyaların altında çizilir.</summary>
    public static class Shadows
    {
        public const float Alpha = 0.26f;

        public static int Order(int level) => -9980 + level * Iso.LevelSortStep;

        /// <summary>Karakter ve hayvanın altındaki gölge: onunla birlikte hareket eden çocuk nesne.</summary>
        public static SpriteRenderer Attach(Transform parent, Vector3 local, float size)
        {
            var go = new GameObject("Shadow");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Shadow;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = new Color(0f, 0f, 0f, Alpha);
            sr.sortingOrder = Order(0);
            return sr;
        }
    }
}
