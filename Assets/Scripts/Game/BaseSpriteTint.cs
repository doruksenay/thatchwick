using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Katmanlı karakter görselleri (belge 43) gelene kadar köylüler ayırt edilsin diye 84 px ve 64 px taban sprite'ın saçını
    /// ve giysisini karakterin görünümündeki renklere boyar (tester raporu 66 §1.1: bütün yetişkinler tek bir erkek ve tek bir
    /// kadın sprite'ıydı). Katman mankeni yüklendiğinde bu yol hiç çalışmaz.
    ///
    /// Sınıflandırma set başına kuraldır: konum (karakterin dolu yüksekliğinde baş bandı / gövde bandı) ve renk tonu, doygunluk,
    /// parlaklık aralığı. Aralıklar taban setlerin bütün durma ve yürüme karelerinin renk dağılımından çıkarıldı (araç:
    /// tools/art/base_tint_report.py). Boyama tonu hedef renge taşır, ışığı (parlaklık oranı) korur: gölge ve parlama kalır.
    /// </summary>
    public static class BaseSpriteTint
    {
        public static bool Enabled = true;

        private sealed class Band
        {
            public float HueMin, HueMax;      // derece; HueMin > HueMax ise 0'dan geçer (ör. 345..32)
            public float SatMin, SatMax, ValMin, ValMax;
            public float RowMin, RowMax;      // dolu yüksekliğin üstten oranı
            public float RefSat, RefVal;      // sınıfın ortalama doygunluk ve parlaklığı (hedefe ölçek)

            public bool Has(float h, float s, float v, float row)
            {
                if (row < RowMin || row >= RowMax || s < SatMin || s > SatMax || v < ValMin || v > ValMax) return false;
                return HueMin <= HueMax ? h >= HueMin && h <= HueMax : h >= HueMin || h <= HueMax;
            }
        }

        private sealed class Rule { public Band Hair, Outfit; }

        // Set öneki → kural. Yaşlıların saçı ağarmış çizildi: boyanmaz (yalnızca giysi).
        private static readonly Dictionary<string, Rule> Rules = new Dictionary<string, Rule>(System.StringComparer.Ordinal)
        {
            ["iso_char_man_r84"] = new Rule
            {
                Hair = new Band { HueMin = 345, HueMax = 32, SatMin = 0.38f, SatMax = 0.62f, ValMin = 0.15f, ValMax = 0.62f, RowMin = 0f, RowMax = 0.32f, RefSat = 0.48f, RefVal = 0.38f },
                Outfit = new Band { HueMin = 36, HueMax = 62, SatMin = 0.30f, SatMax = 1f, ValMin = 0.20f, ValMax = 0.62f, RowMin = 0.28f, RowMax = 1f, RefSat = 0.42f, RefVal = 0.45f },
            },
            ["iso_char_woman_r84"] = new Rule
            {
                Hair = new Band { HueMin = 350, HueMax = 32, SatMin = 0.62f, SatMax = 1f, ValMin = 0.20f, ValMax = 0.90f, RowMin = 0f, RowMax = 0.60f, RefSat = 0.72f, RefVal = 0.55f },
                Outfit = new Band { HueMin = 25, HueMax = 45, SatMin = 0.45f, SatMax = 0.60f, ValMin = 0.28f, ValMax = 0.55f, RowMin = 0.30f, RowMax = 1f, RefSat = 0.53f, RefVal = 0.42f },
            },
            ["iso_char_elder_m_r84"] = new Rule
            {
                Outfit = new Band { HueMin = 24, HueMax = 40, SatMin = 0.50f, SatMax = 1f, ValMin = 0.30f, ValMax = 0.70f, RowMin = 0.30f, RowMax = 1f, RefSat = 0.58f, RefVal = 0.52f },
            },
            ["iso_char_elder_f_r84"] = new Rule
            {
                Outfit = new Band { HueMin = 100, HueMax = 230, SatMin = 0.18f, SatMax = 1f, ValMin = 0.20f, ValMax = 0.40f, RowMin = 0.30f, RowMax = 1f, RefSat = 0.27f, RefVal = 0.30f },
            },
            ["iso_char_boy_r64"] = new Rule
            {
                Hair = new Band { HueMin = 350, HueMax = 32, SatMin = 0.40f, SatMax = 1f, ValMin = 0.15f, ValMax = 0.70f, RowMin = 0f, RowMax = 0.35f, RefSat = 0.55f, RefVal = 0.42f },
                Outfit = new Band { HueMin = 30, HueMax = 45, SatMin = 0.20f, SatMax = 0.32f, ValMin = 0.25f, ValMax = 0.78f, RowMin = 0.30f, RowMax = 1f, RefSat = 0.27f, RefVal = 0.55f },
            },
            ["iso_char_girl_r64"] = new Rule
            {
                Hair = new Band { HueMin = 350, HueMax = 32, SatMin = 0.60f, SatMax = 1f, ValMin = 0.15f, ValMax = 0.85f, RowMin = 0f, RowMax = 0.40f, RefSat = 0.77f, RefVal = 0.47f },
                Outfit = new Band { HueMin = 20, HueMax = 40, SatMin = 0.60f, SatMax = 1f, ValMin = 0.20f, ValMax = 0.60f, RowMin = 0.40f, RowMax = 1f, RefSat = 0.68f, RefVal = 0.36f },
            },
        };

        private const int CacheLimit = 900;
        private static readonly Thatchwick.Core.Collections.LruCache<string, Sprite> Cache =
            new Thatchwick.Core.Collections.LruCache<string, Sprite>(CacheLimit, (k, s) => { if (s != null) { Object.Destroy(s.texture); Object.Destroy(s); } });
        private static readonly Dictionary<Texture2D, Color32[]> Pixels = new Dictionary<Texture2D, Color32[]>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Cache.Clear();
            Pixels.Clear();
        }

        /// <summary>Setin kuralı varsa karakterin saç ve giysi rengine boyanmış kopya; yoksa (ya da kapalıysa) aynı sprite.</summary>
        public static Sprite Apply(Sprite src, Character c, string setPrefix)
        {
            if (!Enabled || src == null || c == null || setPrefix == null || !Rules.TryGetValue(setPrefix, out var rule)) return src;
            var a = c.Appearance;
            if (a == null) return src;
            var hair = a.HairColor;
            var outfit = a.OutfitColor;
            string key = src.name + "|" + hair.R + "," + hair.G + "," + hair.B + "|" + outfit.R + "," + outfit.G + "," + outfit.B;
            if (Cache.TryGet(key, out var hit) && hit != null) return hit;
            var made = Recolor(src, rule, hair, outfit);
            Cache.Set(key, made);
            return made ?? src;
        }

        private static Sprite Recolor(Sprite src, Rule rule, Rgb hair, Rgb outfit)
        {
            var tex = src.texture;
            var all = ReadAll(tex);
            if (all == null) return null;
            var r = src.textureRect;
            int x0 = Mathf.RoundToInt(r.x), y0 = Mathf.RoundToInt(r.y), w = Mathf.RoundToInt(r.width), h = Mathf.RoundToInt(r.height);
            var px = new Color32[w * h];
            int top = -1, bottom = h;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var p = all[(y0 + y) * tex.width + x0 + x];
                    px[y * w + x] = p;
                    if (p.a > 10) { if (y > top) top = y; if (y < bottom) bottom = y; }
                }
            if (top < 0) return null;
            float span = Mathf.Max(1, top - bottom + 1);
            Color.RGBToHSV(new Color32(hair.R, hair.G, hair.B, 255), out float hh, out float hs, out float hv);
            Color.RGBToHSV(new Color32(outfit.R, outfit.G, outfit.B, 255), out float oh, out float os, out float ov);
            for (int y = 0; y < h; y++)
            {
                float row = (top - y) / span;   // 0 = en üst satır
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    var p = px[i];
                    if (p.a <= 10) continue;
                    Color.RGBToHSV(p, out float ph, out float ps, out float pv);
                    float deg = ph * 360f;
                    Band band = null; float th = 0, ts = 0, tv = 0;
                    if (rule.Hair != null && rule.Hair.Has(deg, ps, pv, row)) { band = rule.Hair; th = hh; ts = hs; tv = hv; }
                    else if (rule.Outfit != null && rule.Outfit.Has(deg, ps, pv, row)) { band = rule.Outfit; th = oh; ts = os; tv = ov; }
                    if (band == null) continue;
                    float ns = Mathf.Clamp01(ts * (ps / band.RefSat));
                    float nv = Mathf.Clamp01(tv * (pv / band.RefVal));
                    var nc = (Color32)Color.HSVToRGB(th, ns, nv);
                    nc.a = p.a;
                    px[i] = nc;
                }
            }
            var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = src.name + "_tint" };
            outTex.SetPixels32(px);
            outTex.Apply(false, true);
            var pivot = new Vector2(src.pivot.x / w, src.pivot.y / h);
            var s = Sprite.Create(outTex, new Rect(0, 0, w, h), pivot, src.pixelsPerUnit);
            s.name = src.name;
            return s;
        }

        /// <summary>Dokunun bütün pikselleri. Okunabilir değilse (içe aktarma Read/Write kapalı) bir kez GPU'dan geri okunur.</summary>
        private static Color32[] ReadAll(Texture2D tex)
        {
            if (tex == null) return null;
            if (Pixels.TryGetValue(tex, out var cached)) return cached;
            Color32[] data = null;
            if (tex.isReadable) data = tex.GetPixels32();
            else
            {
                var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                var prev = RenderTexture.active;
                Graphics.Blit(tex, rt);
                RenderTexture.active = rt;
                var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, false);
                copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                copy.Apply(false);
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                data = copy.GetPixels32();
                Object.Destroy(copy);
            }
            Pixels[tex] = data;
            return data;
        }
    }
}
