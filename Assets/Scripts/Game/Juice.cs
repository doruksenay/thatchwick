using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Animasyon partisi 1 (18.11.2026, 0 gen): kare animasyonu gelmeden oyuna hareket katan küçük, kodla
    /// yapılan efektler. Parçacıklar ve tween'ler tek bileşende, havuzlu SpriteRenderer'larla (ParticleSystem
    /// kurulumu ve sahne asset'i gerektirmez). Bütün kaymalar 1/32 birime (bir sanat pikseline) yuvarlanır.
    /// Oyun duraklatıldığında (<see cref="Paused"/>) parçacıklar donar, yeni parçacık çıkmaz.
    /// </summary>
    public sealed class Juice : MonoBehaviour
    {
        public const float Px = 1f / 32f;

        public enum Kind { Smoke, Spark, Dust, Chip, Leaf, Steam, Soil, Pop, Fly, Moth, Firefly, Butterfly, Bee, Stink, Drip, Splash, Glint, Breath, Dragonfly }

        private struct Particle
        {
            public SpriteRenderer R;
            public Vector3 Pos;
            public Vector3 Vel;
            public float Age, Life, Size, Drift, Phase;
            public Kind Kind;
            public Color Color;
        }

        private struct Tween
        {
            public Transform T;
            public Vector3 BaseScale;
            public float Age, Life;
        }

        /// <summary>Aynı anda en çok bu kadar parçacık (telefon bütçesi).</summary>
        private const int MaxParticles = 220;

        private readonly List<Particle> _live = new List<Particle>();
        private readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        private readonly List<Tween> _tweens = new List<Tween>();
        private readonly System.Random _rng = new System.Random(18112026);

        /// <summary>Oyun duraklatıldı mı (parçacıklar ve tween'ler bekler).</summary>
        public bool Paused { get; set; }

        /// <summary>Oyun hızı (1-3); parçacıklar biraz hızlanır ama sim hızı kadar değil, göz yorulmasın.</summary>
        public float SpeedScale { get; set; } = 1f;

        /// <summary>Rüzgâr (birim/sn, işaretli): duman, buhar, yaprak ve koku bununla eğilir (belge 57 §8).</summary>
        public float Wind { get; set; }

        private float Rand(float a, float b) => a + (float)_rng.NextDouble() * (b - a);

        /// <summary>Bir noktadan parçacık çıkarır. order: sprite sıralaması (genelde kaynağın biraz üstü).</summary>
        public void Emit(Kind kind, Vector3 pos, int count, int order)
        {
            if (Paused) return;
            for (int i = 0; i < count && _live.Count < MaxParticles; i++)
            {
                var p = new Particle { Kind = kind, Pos = pos, Phase = Rand(0f, 6.28f) };
                switch (kind)
                {
                    case Kind.Smoke:
                        p.Vel = new Vector3(Rand(-0.04f, 0.04f), Rand(0.22f, 0.34f), 0f);
                        p.Life = Rand(2.2f, 3.4f); p.Size = Rand(3f, 4f); p.Drift = Rand(0.05f, 0.12f);
                        // Unity'de denendi (19.11): 0,55 alfa ve 2-3 px gündüz neredeyse görünmüyordu.
                        float g = Rand(0.62f, 0.8f); p.Color = new Color(g, g, g * 1.02f, 0.75f);
                        break;
                    case Kind.Steam:
                        p.Vel = new Vector3(Rand(-0.03f, 0.03f), Rand(0.18f, 0.26f), 0f);
                        p.Life = Rand(1.0f, 1.6f); p.Size = 2f; p.Drift = 0.06f;
                        p.Color = new Color(0.95f, 0.95f, 0.97f, 0.5f);
                        break;
                    case Kind.Spark:
                        p.Vel = new Vector3(Rand(-0.12f, 0.12f), Rand(0.35f, 0.6f), 0f);
                        p.Life = Rand(0.5f, 0.9f); p.Size = 1f; p.Drift = 0.2f;
                        p.Color = _rng.NextDouble() < 0.5 ? new Color(1f, 0.78f, 0.3f, 1f) : new Color(1f, 0.5f, 0.18f, 1f);
                        break;
                    case Kind.Dust:
                        p.Vel = new Vector3(Rand(-0.18f, 0.18f), Rand(0.04f, 0.1f), 0f);
                        p.Life = Rand(0.35f, 0.55f); p.Size = Rand(1f, 2f);
                        p.Color = new Color(0.72f, 0.64f, 0.5f, 0.6f);
                        break;
                    case Kind.Soil:
                        p.Vel = new Vector3(Rand(-0.3f, 0.3f), Rand(0.5f, 0.8f), 0f);
                        p.Life = Rand(0.4f, 0.6f); p.Size = Rand(1f, 2f);
                        p.Color = new Color(0.42f, 0.3f, 0.2f, 1f);
                        break;
                    case Kind.Chip:
                        p.Vel = new Vector3(Rand(-0.45f, 0.45f), Rand(0.6f, 0.95f), 0f);
                        p.Life = Rand(0.45f, 0.7f); p.Size = Rand(1f, 2f);
                        p.Color = _rng.NextDouble() < 0.5 ? new Color(0.86f, 0.7f, 0.46f, 1f) : new Color(0.66f, 0.5f, 0.32f, 1f);
                        break;
                    case Kind.Leaf:
                        p.Pos += new Vector3(Rand(-0.45f, 0.45f), Rand(-0.1f, 0.3f), 0f);
                        p.Vel = new Vector3(Rand(0.02f, 0.1f), Rand(-0.22f, -0.14f), 0f);
                        p.Life = Rand(3f, 4.5f); p.Size = 2f; p.Drift = Rand(0.18f, 0.3f);
                        p.Color = LeafColor;
                        break;
                    case Kind.Fly:
                        // Sinek (belge 57 §2): kaynağın üstünde kısa, düzensiz turlar atan koyu tek piksel.
                        p.Pos += new Vector3(Rand(-0.22f, 0.22f), Rand(0.05f, 0.4f), 0f);
                        p.Life = Rand(1.4f, 2.4f); p.Size = 1f;
                        p.Color = new Color(0.1f, 0.09f, 0.08f, 0.95f);
                        break;
                    case Kind.Moth:
                        // Güve: yanan ışığın çevresinde dönen soluk nokta.
                        p.Pos += new Vector3(Rand(-0.18f, 0.18f), Rand(-0.05f, 0.25f), 0f);
                        p.Life = Rand(1.6f, 2.8f); p.Size = 1f;
                        p.Color = new Color(0.86f, 0.8f, 0.68f, 0.9f);
                        break;
                    case Kind.Firefly:
                        p.Pos += new Vector3(Rand(-0.5f, 0.5f), Rand(0.1f, 0.7f), 0f);
                        p.Vel = new Vector3(Rand(-0.06f, 0.06f), Rand(-0.03f, 0.05f), 0f);
                        p.Life = Rand(3f, 5f); p.Size = 1f;
                        p.Color = new Color(0.82f, 1f, 0.42f, 1f);
                        break;
                    case Kind.Butterfly:
                        p.Pos += new Vector3(Rand(-0.3f, 0.3f), Rand(0.15f, 0.5f), 0f);
                        p.Life = Rand(3f, 5f); p.Size = 2f;
                        p.Color = _rng.NextDouble() < 0.5 ? new Color(0.98f, 0.96f, 0.86f, 1f) : new Color(0.96f, 0.82f, 0.32f, 1f);
                        break;
                    case Kind.Dragonfly:
                        p.Pos += new Vector3(Rand(-0.4f, 0.4f), Rand(0.2f, 0.5f), 0f);
                        p.Life = Rand(2.5f, 4f); p.Size = 2f;
                        p.Color = _rng.NextDouble() < 0.5 ? new Color(0.3f, 0.62f, 0.86f, 1f) : new Color(0.42f, 0.74f, 0.5f, 1f);
                        break;
                    case Kind.Bee:
                        p.Pos += new Vector3(Rand(-0.25f, 0.25f), Rand(0.1f, 0.4f), 0f);
                        p.Life = Rand(1.6f, 2.6f); p.Size = 1f;
                        p.Color = new Color(0.9f, 0.7f, 0.15f, 1f);
                        break;
                    case Kind.Stink:
                        // Koku dalgası: kokmuş kaynağın üstünde yükselen yeşilimsi kıvrım.
                        p.Pos += new Vector3(Rand(-0.15f, 0.15f), 0.15f, 0f);
                        p.Vel = new Vector3(0f, Rand(0.12f, 0.18f), 0f);
                        p.Life = Rand(1.4f, 2f); p.Size = 1f; p.Drift = 0.09f;
                        p.Color = new Color(0.55f, 0.66f, 0.3f, 0.75f);
                        break;
                    case Kind.Drip:
                        p.Vel = new Vector3(0f, -Rand(1.6f, 2.2f), 0f);
                        p.Life = Rand(0.45f, 0.65f); p.Size = 1f;
                        p.Color = new Color(0.72f, 0.82f, 0.95f, 0.9f);
                        break;
                    case Kind.Splash:
                        p.Vel = new Vector3(Rand(-0.25f, 0.25f), Rand(0.3f, 0.5f), 0f);
                        p.Life = Rand(0.18f, 0.28f); p.Size = 1f;
                        p.Color = new Color(0.8f, 0.88f, 0.98f, 0.8f);
                        break;
                    case Kind.Glint:
                        // Su parıltısı ve yağmur halkası: su karosunda kısa süre yanıp sönen açık nokta.
                        p.Pos += new Vector3(Rand(-0.4f, 0.4f), Rand(-0.18f, 0.18f), 0f);
                        p.Life = Rand(0.5f, 0.9f); p.Size = _rng.NextDouble() < 0.3 ? 2f : 1f;
                        p.Color = new Color(0.86f, 0.94f, 1f, 0.85f);
                        break;
                    case Kind.Breath:
                        p.Vel = new Vector3(Rand(-0.02f, 0.04f), Rand(0.08f, 0.14f), 0f);
                        p.Life = Rand(0.7f, 1f); p.Size = 2f; p.Drift = 0.03f;
                        p.Color = new Color(0.95f, 0.96f, 1f, 0.4f);
                        break;
                    case Kind.Pop:
                        float a = Rand(0f, 6.28f);
                        p.Vel = new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.3f + 0.2f, 0f);
                        p.Life = Rand(0.3f, 0.45f); p.Size = 1f;
                        p.Color = new Color(1f, 0.95f, 0.75f, 0.9f);
                        break;
                }
                p.R = Rent(order);
                _live.Add(p);
            }
        }

        /// <summary>Düşen yaprağın rengi; mevsime göre GameRoot ayarlar (sonbahar turuncu, yaz yeşil).</summary>
        public Color LeafColor { get; set; } = new Color(0.82f, 0.5f, 0.2f, 1f);

        /// <summary>Yeni konan eşya, hasat edilen ekin: kısa ezilip esneme (squash &amp; stretch) ve toz.</summary>
        public void Pop(Transform t, float seconds = 0.28f)
        {
            if (t == null) return;
            for (int i = 0; i < _tweens.Count; i++) if (_tweens[i].T == t) return;
            _tweens.Add(new Tween { T = t, BaseScale = t.localScale, Life = seconds });
        }

        private SpriteRenderer Rent(int order)
        {
            SpriteRenderer sr;
            if (_pool.Count > 0) { sr = _pool.Pop(); sr.enabled = true; }
            else
            {
                var go = new GameObject("Fx");
                go.transform.SetParent(transform, false);
                sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteMaterials.White;
                sr.sharedMaterial = SpriteMaterials.Unlit;
            }
            sr.sortingOrder = order;
            return sr;
        }

        private void Update()
        {
            if (Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f) * (0.75f + 0.25f * SpeedScale);
            float t = Time.time;

            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var p = _live[i];
                p.Age += dt;
                if (p.Age >= p.Life || p.R == null)
                {
                    if (p.R != null) { p.R.enabled = false; _pool.Push(p.R); }
                    _live.RemoveAt(i);
                    continue;
                }
                float k = p.Age / p.Life;
                switch (p.Kind)
                {
                    case Kind.Chip:
                    case Kind.Soil:
                        p.Vel.y -= 2.6f * dt;   // yerçekimi: yonga ve toprak kavis çizip düşer
                        break;
                    case Kind.Dust:
                    case Kind.Pop:
                        p.Vel *= 1f - Mathf.Min(1f, 4f * dt);
                        break;
                    case Kind.Fly:
                    case Kind.Bee:
                        p.Vel = new Vector3(Mathf.Sin(t * 9f + p.Phase) * 0.4f, Mathf.Cos(t * 7f + p.Phase * 1.7f) * 0.25f, 0f);
                        break;
                    case Kind.Dragonfly:
                        // Kızböceği: durur, birden atılır (suyun üstünde kısa sıçrayışlar).
                        p.Vel = Mathf.Repeat(t * 0.7f + p.Phase, 1f) < 0.25f ? new Vector3(Mathf.Sin(p.Phase * 5f) * 1.2f, Mathf.Cos(p.Phase * 3f) * 0.4f, 0f) : Vector3.zero;
                        break;
                    case Kind.Moth:
                        p.Vel = new Vector3(Mathf.Sin(t * 5f + p.Phase) * 0.3f, Mathf.Cos(t * 6f + p.Phase * 1.3f) * 0.2f, 0f);
                        break;
                    case Kind.Butterfly:
                        p.Vel = new Vector3(Mathf.Sin(t * 1.3f + p.Phase) * 0.22f, Mathf.Sin(t * 6f + p.Phase) * 0.18f + 0.01f, 0f);
                        break;
                    case Kind.Splash:
                        p.Vel.y -= 2.6f * dt;
                        break;
                    case Kind.Smoke:
                    case Kind.Steam:
                    case Kind.Stink:
                    case Kind.Leaf:
                    case Kind.Breath:
                        p.Pos.x += Wind * (p.Kind == Kind.Leaf ? 0.5f : 0.22f) * k * dt;   // rüzgâr yükseldikçe daha çok eğer
                        break;
                }
                p.Pos += p.Vel * dt;
                float sway = p.Drift > 0f ? Mathf.Sin(t * 2.2f + p.Phase) * p.Drift : 0f;
                var draw = p.Pos + new Vector3(sway, 0f, 0f);
                draw.x = Mathf.Round(draw.x / Px) * Px;
                draw.y = Mathf.Round(draw.y / Px) * Px;
                p.R.transform.position = draw;

                // White sprite 4 px: piksel boyu = Size / 4 ölçek. Duman büyüyerek dağılır.
                float size = p.Kind == Kind.Smoke ? Mathf.Round(p.Size + k * 4f) : p.Size;
                float s = size / 4f;
                p.R.transform.localScale = new Vector3(s, s, 1f);
                var c = p.Color;
                c.a *= p.Kind == Kind.Smoke || p.Kind == Kind.Steam || p.Kind == Kind.Breath || p.Kind == Kind.Stink ? (1f - k) * Mathf.Min(1f, k * 6f)
                    : p.Kind == Kind.Firefly || p.Kind == Kind.Glint ? Mathf.Sin(k * 3.14159f) * (p.Kind == Kind.Firefly ? 0.5f + 0.5f * Mathf.Sin(t * 5f + p.Phase) : 1f)
                    : 1f - k * k;
                p.R.color = c;
                _live[i] = p;
            }

            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                var tw = _tweens[i];
                tw.Age += dt;
                if (tw.T == null) { _tweens.RemoveAt(i); continue; }
                if (tw.Age >= tw.Life)
                {
                    tw.T.localScale = tw.BaseScale;
                    _tweens.RemoveAt(i);
                    continue;
                }
                float k = tw.Age / tw.Life;
                // Ezilir (geniş, alçak), sonra esner (dar, uzun), sönümlenerek oturur.
                float wave = Mathf.Sin(k * Mathf.PI * 2f) * (1f - k);
                tw.T.localScale = new Vector3(tw.BaseScale.x * (1f + 0.16f * wave), tw.BaseScale.y * (1f - 0.16f * wave), tw.BaseScale.z);
                _tweens[i] = tw;
            }
        }

        /// <summary>Nefes: boştayken gövde 1 px iner kalkar (yaklaşık 2,6 sn'de bir). seed kişiyi ayırır.</summary>
        public static float BreathOffset(int seed)
        {
            float phase = Time.time * 2.4f + seed * 1.37f;
            return Mathf.Sin(phase) > 0.35f ? Px : 0f;
        }

        /// <summary>İş vuruşu: eşyada çalışırken her vuruşta gövde 1-2 px çöker. Dönen değer 0..1 vuruş anı.</summary>
        public static float WorkBeat(int seed, float perSecond)
        {
            float x = Time.time * perSecond + seed * 0.29f;
            return x - Mathf.Floor(x);
        }
    }
}
