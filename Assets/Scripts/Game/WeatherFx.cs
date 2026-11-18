using System.Collections.Generic;
using Thatchwick.Core.Life;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Yağmur, fırtına ve kar parçacıkları (görsel parti 2, 18.11.2026). Kameranın gördüğü alana düşen basit
    /// pikselart damlalar ve kar taneleri; havaya göre yoğunluk ve hız. Ortam örtüsünün altında çizilir, böylece gece
    /// karanlığına uyar. Oyun duraklayınca (Time.timeScale 0) durur. Yapı modunda gizlenir.
    /// </summary>
    public sealed class WeatherFx : MonoBehaviour
    {
        /// <summary>Ortam örtüsünün (Ambience.OverlayOrder) hemen altı.</summary>
        public const int Order = Ambience.OverlayOrder - 10;

        private struct Drop { public SpriteRenderer R; public Vector2 Local; public float Speed; public float Phase; }

        private Camera _cam;
        private readonly List<Drop> _drops = new List<Drop>();
        private WeatherType _weather = WeatherType.Clear;
        private bool _hidden;
        private Sprite _rain, _snow;
        private System.Random _rng = new System.Random(4242);

        public void Init(Camera cam)
        {
            _cam = cam;
            _rain = MakeSprite(1, 6, new Color32(205, 220, 240, 150));
            _snow = MakeSprite(2, 2, new Color32(245, 248, 255, 220));
        }

        /// <summary>Her kare: bugünkü hava ve gizli olup olmadığı (yapı modu, karakter yaratma).</summary>
        public void Set(WeatherType weather, bool hidden)
        {
            _weather = weather;
            _hidden = hidden;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            int want = _hidden ? 0 : _weather == WeatherType.Rain ? 140 : _weather == WeatherType.Storm ? 260 : _weather == WeatherType.Snow ? 110 : 0;
            bool snow = _weather == WeatherType.Snow;
            float halfH = _cam.orthographicSize + 1f;
            float halfW = halfH * _cam.aspect + 1f;
            var cp = (Vector2)_cam.transform.position;
            // Damla boyu ekranda sabit kalsın: yakınlaştırınca küçülmesin diye ölçek kamera boyuna bağlı.
            float scale = Mathf.Max(0.6f, _cam.orthographicSize / 6f);

            while (_drops.Count < want) _drops.Add(NewDrop(halfW, halfH));
            float dt = Time.deltaTime;
            for (int i = 0; i < _drops.Count; i++)
            {
                var d = _drops[i];
                bool on = i < want;
                if (d.R.enabled != on) d.R.enabled = on;
                if (!on) continue;
                d.R.sprite = snow ? _snow : _rain;
                if (snow)
                {
                    d.Phase += dt;
                    d.Local += new Vector2(Mathf.Sin(d.Phase * 1.3f + i) * 0.35f, -d.Speed * 0.18f) * dt * scale;
                }
                else
                {
                    float wind = _weather == WeatherType.Storm ? -3.2f : -1.4f;
                    d.Local += new Vector2(wind, -d.Speed) * dt * scale;
                }
                if (d.Local.y < -halfH || d.Local.x < -halfW || d.Local.x > halfW)
                    d.Local = new Vector2(Rand(-halfW, halfW + 3f), halfH + Rand(0f, 1.5f));
                d.R.transform.position = new Vector3(cp.x + d.Local.x, cp.y + d.Local.y, 0f);
                d.R.transform.localScale = new Vector3(scale, scale, 1f);
                _drops[i] = d;
            }
        }

        private Drop NewDrop(float halfW, float halfH)
        {
            var go = new GameObject("Drop");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.sortingOrder = Order;
            return new Drop
            {
                R = sr,
                Local = new Vector2(Rand(-halfW, halfW), Rand(-halfH, halfH)),
                Speed = Rand(11f, 16f),
                Phase = Rand(0f, 6f)
            };
        }

        private float Rand(float a, float b) => a + (float)_rng.NextDouble() * (b - a);

        private static Sprite MakeSprite(int w, int h, Color32 c)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
