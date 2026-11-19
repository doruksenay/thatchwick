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

        /// <summary>
        /// Dünya noktasının altındaki karo kapalı bir odada mı (GameRoot verir). Doluysa damla orada görünmez:
        /// evin içine yağmur yağmaz (Unity bulgusu 19.11.2026).
        /// </summary>
        public System.Func<Vector3, bool> IsSheltered;

        // Yoğunluk ritmi: hava değişince yumuşak geçiş, yağarken yavaş dalga ve ara ara sağanak (fırtınada rüzgâr
        // da hamle yapar). Sabit yoğunluk ve hız yapay duruyordu (kullanıcı, 19.11.2026).
        private float _level;        // 0..1 şu anki yoğunluk (hedefe doğru kayar)
        private float _gust;         // 0..1 sağanak/hamle zarfı
        private float _gustTimer = 4f;
        private float _seed;

        public void Init(Camera cam)
        {
            _cam = cam;
            _rain = MakeSprite(1, 6, new Color32(205, 220, 240, 150));
            _snow = MakeSprite(2, 2, new Color32(245, 248, 255, 220));
            _seed = Rand(0f, 100f);
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
            int max = _weather == WeatherType.Rain ? 170 : _weather == WeatherType.Storm ? 320 : _weather == WeatherType.Snow ? 130 : 0;
            bool snow = _weather == WeatherType.Snow;
            float dt = Time.deltaTime;
            float t = Time.time;

            // Hedef yoğunluk: yavaş dalga (yaklaşık yarım dakikada bir hafifler, koyulaşır) + sağanak.
            if (dt > 0f)
            {
                _gustTimer -= dt;
                if (_gustTimer <= 0f)
                {
                    bool storm = _weather == WeatherType.Storm;
                    _gust = Mathf.Max(_gust, storm ? Rand(0.6f, 1f) : Rand(0.3f, 0.7f));
                    _gustTimer = storm ? Rand(3f, 8f) : Rand(7f, 16f);
                }
                _gust = Mathf.MoveTowards(_gust, 0f, dt * 0.25f);
            }
            float wave = Mathf.PerlinNoise(_seed, t * 0.035f);                 // 0..1, yavaş
            float target = _hidden || max == 0 ? 0f : Mathf.Clamp01(0.35f + 0.5f * wave + 0.4f * _gust);
            _level = Mathf.MoveTowards(_level, target, dt * (target > _level ? 0.35f : 0.2f)); // başlar yavaş, diner daha yavaş
            if (max == 0 && _level > 0f) max = _drops.Count;                  // dinerken eski damlalar bitsin
            int want = Mathf.RoundToInt(max * _level);
            float halfH = _cam.orthographicSize + 1f;
            float halfW = halfH * _cam.aspect + 1f;
            var cp = (Vector2)_cam.transform.position;
            // Damla boyu ekranda sabit kalsın: yakınlaştırınca küçülmesin diye ölçek kamera boyuna bağlı.
            float scale = Mathf.Max(0.6f, _cam.orthographicSize / 6f);

            while (_drops.Count < want) _drops.Add(NewDrop(halfW, halfH));
            // Hız ve rüzgâr da yoğunlukla oynar: sağanakta damlalar hızlanır ve yatar.
            float speedMul = 0.8f + 0.45f * _level + 0.25f * _gust;
            float windBase = _weather == WeatherType.Storm ? -3.2f : -1.4f;
            float wind = windBase * (0.6f + 0.5f * Mathf.PerlinNoise(_seed + 7f, t * 0.25f) + 0.6f * _gust);
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
                    d.Local += new Vector2(Mathf.Sin(d.Phase * 1.3f + i) * 0.35f + wind * 0.08f, -d.Speed * 0.18f * speedMul) * dt * scale;
                }
                else
                {
                    d.Local += new Vector2(wind, -d.Speed * speedMul) * dt * scale;
                }
                if (d.Local.y < -halfH || d.Local.x < -halfW || d.Local.x > halfW)
                    d.Local = new Vector2(Rand(-halfW, halfW + 3f), halfH + Rand(0f, 1.5f));
                var wpos = new Vector3(cp.x + d.Local.x, cp.y + d.Local.y, 0f);
                d.R.transform.position = wpos;
                // Çatı altı: damla görünmez ama düşmeye devam eder (çıkınca yeniden belirir).
                bool show = IsSheltered == null || !IsSheltered(wpos);
                var c = d.R.color; c.a = show ? 1f : 0f; d.R.color = c;
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
