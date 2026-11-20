using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Geçici portre (karar 20.11.2026): gerçek portre parçaları (belge 43, sanat işleri duraklatıldı) gelene kadar
    /// karakterin dünyadaki duruş karesinin baş kısmı büyütülüp panelde gösterilir. Katman görseli yoksa prosedürel
    /// yer tutucudan kırpılır. Portre görselleri gelince yalnızca <see cref="SpriteFor"/> değişir.
    /// </summary>
    public static class Portraits
    {
        /// <summary>Başın, karakterin dolu piksel yüksekliğine oranı (gerçekçi oranlı 84 px set için yaklaşık).</summary>
        private const float HeadFraction = 0.34f;

        private sealed class Fallback { public string Key; public Sprite Sprite; }
        private static readonly Dictionary<int, Fallback> Fallbacks = new Dictionary<int, Fallback>();
        private static readonly Dictionary<Texture2D, Rect> Crops = new Dictionary<Texture2D, Rect>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Fallbacks.Clear(); Crops.Clear(); }

        /// <summary>Portrenin kırpılacağı kare: güneydoğuya bakan duruş. Bebekte ve görsel yokken yer tutucu.</summary>
        public static Sprite SpriteFor(Character c, LookState look)
        {
            if (c == null) return null;
            var layered = LayeredCharacterArt.Frame(c, "idle", "se", 0);
            if (layered != null) return layered;
            string key = c.Appearance.Key() + "|" + c.Stage;
            if (Fallbacks.TryGetValue(c.Id, out var f) && f.Key == key && f.Sprite != null) return f.Sprite;
            if (f != null) PersonSprites.Destroy(f.Sprite);
            f = new Fallback { Key = key, Sprite = PersonSprites.Create(c.Appearance, look) };
            Fallbacks[c.Id] = f;
            return f.Sprite;
        }

        /// <summary>Çerçeveli portre çizer; kare yoksa yalnızca boş çerçeve.</summary>
        public static void Draw(Rect r, Sprite s)
        {
            var prev = GUI.color;
            GUI.color = new Color(0.16f, 0.13f, 0.10f, 0.9f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = new Color(0.36f, 0.42f, 0.33f, 1f);
            var inner = new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f);
            GUI.DrawTexture(inner, Texture2D.whiteTexture);
            GUI.color = prev;
            if (s == null || s.texture == null) return;
            var crop = HeadCrop(s);
            var tex = s.texture;
            var uv = new Rect(crop.x / tex.width, crop.y / tex.height, crop.width / tex.width, crop.height / tex.height);
            GUI.DrawTextureWithTexCoords(inner, tex, uv, true);
        }

        /// <summary>Dolu piksellerin üst kısmından kare bir baş kutusu (doku pikseli, alttan). Doku okunamıyorsa üst orta.</summary>
        private static Rect HeadCrop(Sprite s)
        {
            var tex = s.texture;
            if (Crops.TryGetValue(tex, out var cached)) return cached;
            if (Crops.Count > 256) Crops.Clear();
            var tr = s.textureRect;
            Rect result;
            Color32[] px = null;
            try { if (tex.isReadable) px = tex.GetPixels32(); } catch (System.Exception) { px = null; }
            int x0 = Mathf.RoundToInt(tr.x), y0 = Mathf.RoundToInt(tr.y), w = Mathf.RoundToInt(tr.width), h = Mathf.RoundToInt(tr.height);
            int top = -1, bottom = -1;
            if (px != null && px.Length == tex.width * tex.height)
            {
                for (int y = y0 + h - 1; y >= y0 && top < 0; y--)
                    for (int x = x0; x < x0 + w; x++) if (px[y * tex.width + x].a > 40) { top = y; break; }
                for (int y = y0; y < y0 + h && bottom < 0; y++)
                    for (int x = x0; x < x0 + w; x++) if (px[y * tex.width + x].a > 40) { bottom = y; break; }
            }
            if (top < 0 || bottom < 0 || top <= bottom)
            {
                float side0 = Mathf.Min(tr.width, tr.height * 0.4f);
                result = new Rect(tr.x + (tr.width - side0) / 2f, tr.yMax - side0, side0, side0);
            }
            else
            {
                int side = Mathf.Clamp(Mathf.CeilToInt((top - bottom + 1) * HeadFraction) + 2, 8, Mathf.Min(w, h));
                int yLow = Mathf.Max(y0, top + 2 - side);
                // Yatay merkez: baş bandındaki dolu piksellerin ortası.
                int minX = int.MaxValue, maxX = int.MinValue;
                for (int y = yLow; y <= top; y++)
                    for (int x = x0; x < x0 + w; x++)
                        if (px[y * tex.width + x].a > 40) { if (x < minX) minX = x; if (x > maxX) maxX = x; }
                float cx = minX <= maxX ? (minX + maxX + 1) / 2f : x0 + w / 2f;
                float left = Mathf.Clamp(cx - side / 2f, x0, x0 + w - side);
                result = new Rect(left, yLow, side, side);
            }
            Crops[tex] = result;
            return result;
        }
    }
}
