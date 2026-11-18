using System.Collections.Generic;
using Thatchwick.Core.Time;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Ortam ışığı (görsel analiz, 18.11.2026): sahnenin tamamını gün ışığı rengiyle çarpan bir örtü ve
    /// gece ocak, mum ve fenerlerin yere düşen sıcak halesi. Renk Core'daki <see cref="DayLight"/>'tan gelir;
    /// geçişler yumuşatılır (hız değişince ya da yüklemede sıçramasın). Yapı modunda sahne nötr ışıkta görünür.
    /// Shader derlemede yoksa sessizce kapanır.
    /// </summary>
    public sealed class Ambience : MonoBehaviour
    {
        /// <summary>Örtü bütün dünya sprite'larının üstünde çizilir (kat bandları ±14000 içinde kalır).</summary>
        public const int OverlayOrder = 32000;
        public const int GlowOrder = 32010;

        private static readonly Color FireColor = new Color(1f, 0.70f, 0.36f);
        private static readonly Color CandleColor = new Color(1f, 0.82f, 0.52f);

        /// <summary>Gece halesi olan ışık: dünya konumu, güç (yaklaşık karo yarıçapı / 1.5) ve titreyip titremediği.</summary>
        public struct LightSource
        {
            public Vector3 Position;
            public float Strength;
            public bool Fire;
            public int Key;
        }

        private Camera _cam;
        private SpriteRenderer _overlay;
        private readonly List<SpriteRenderer> _glows = new List<SpriteRenderer>();
        private LightTint _current = new LightTint(1f, 1f, 1f);
        private bool _hasCurrent;
        private bool _disabled;

        /// <summary>Son karede uygulanan gece ölçüsü (0 aydınlık, 1 karanlık).</summary>
        public float Night { get; private set; }

        public void Init(Camera cam)
        {
            _cam = cam;
            var mat = SpriteMaterials.Multiply;
            if (mat == null) { _disabled = true; return; }
            var go = new GameObject("AmbientOverlay");
            go.transform.SetParent(transform, false);
            _overlay = go.AddComponent<SpriteRenderer>();
            _overlay.sprite = SpriteMaterials.White;
            _overlay.sharedMaterial = mat;
            _overlay.sortingOrder = OverlayOrder;
        }

        /// <summary>
        /// Her kare çağrılır. target: DayLight rengi; neutral: yapı modu gibi ışığın kapalı olduğu durum;
        /// snap: yüklemede ya da hız sıçramasında renk beklemeden oturur.
        /// </summary>
        public void Apply(LightTint target, bool neutral, IReadOnlyList<LightSource> lights, bool snap = false)
        {
            if (_disabled || _cam == null) return;
            if (neutral) target = new LightTint(1f, 1f, 1f);
            if (!_hasCurrent || snap) { _current = target; _hasCurrent = true; }
            else _current = LightTint.Lerp(_current, target, Mathf.Clamp01(Time.unscaledDeltaTime * 1.5f));
            Night = DayLight.NightFactor(_current);

            // Örtü kamerayı tam kaplar; öğle beyazında hiç çizilmez.
            bool white = _current.R > 0.995f && _current.G > 0.995f && _current.B > 0.995f;
            _overlay.enabled = !white;
            if (!white)
            {
                float h = _cam.orthographicSize * 2f + 1f;
                float w = h * _cam.aspect + 1f;
                var cp = _cam.transform.position;
                _overlay.transform.position = new Vector3(cp.x, cp.y, 0f);
                _overlay.transform.localScale = new Vector3(w / (4f / 32f), h / (4f / 32f), 1f);   // White: 4 px, PPU 32
                _overlay.color = new Color(_current.R, _current.G, _current.B, 1f);
            }

            int used = 0;
            if (Night > 0.02f && lights != null)
            {
                var additive = SpriteMaterials.Additive;
                for (int i = 0; i < lights.Count && additive != null; i++)
                {
                    var l = lights[i];
                    var sr = used < _glows.Count ? _glows[used] : NewGlow(additive);
                    used++;
                    float flicker = l.Fire ? 0.92f + 0.08f * Mathf.PerlinNoise(Time.time * 3.1f, l.Key * 0.37f) : 1f;
                    float scale = 1.5f * l.Strength * flicker;
                    sr.transform.position = l.Position;
                    sr.transform.localScale = new Vector3(scale, scale, 1f);
                    var c = l.Fire ? FireColor : CandleColor;
                    c.a = Night * 0.6f * flicker;
                    sr.color = c;
                    sr.enabled = true;
                }
            }
            for (int i = used; i < _glows.Count; i++) _glows[i].enabled = false;
        }

        private SpriteRenderer NewGlow(Material additive)
        {
            var go = new GameObject("Glow");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Glow;
            sr.sharedMaterial = additive;
            sr.sortingOrder = GlowOrder;
            _glows.Add(sr);
            return sr;
        }
    }
}
