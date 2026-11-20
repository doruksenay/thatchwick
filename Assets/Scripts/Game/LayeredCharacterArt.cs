using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Customization.Layers;

namespace Thatchwick.Game
{
    /// <summary>
    /// Katmanlı karakter görseli (belge 43 §4.2-4.3): mankenin bölge haritası kareleri + görünümün rampaları +
    /// baş çapasına oturan saç, sakal ve başlık parçaları. Çizim Core'daki LayerComposer'da; burada yalnızca
    /// Unity dokusu okunur ve sonuç önbelleğe alınır.
    ///
    /// Görseller (Assets/Art/Iso/katman/, okunabilir doku):
    ///   lay_body_&lt;gövde&gt;_&lt;anim&gt;_&lt;yön&gt;_&lt;NN&gt;   gövde: m|f|cm|cf + _ince|_orta|_gurbuz|_dolgun
    ///   lay_&lt;yuva&gt;_&lt;kimlik&gt;_&lt;yön&gt;_fr|bk            yuva: mass, front, extra, orn, must, beard, side, hat
    /// Çapalar bölge haritasının içinde (mavi kanal: 1 baş, 2 göz; tools/art/manken.py regions yazar).
    ///
    /// Mankenin karesi yoksa null döner; çağıran (IsoArt) bugünkü 84 px görsele düşer. Pilot görselleri gelene kadar
    /// oyunda hiçbir şey değişmez.
    /// </summary>
    public static class LayeredCharacterArt
    {
        public static bool Enabled = true;
        /// <summary>Göz/kaş deseni ve başlık örtüsü için (GameRoot yükler).</summary>
        public static AppearanceCatalog Catalog;

        private const int CacheLimit = 1500;
        private const float Ppu = 32f;

        /// <summary>Pivot: tuvalin yatay ortası, alttan 2 px (84 px ve 64 px tabanlarla aynı kural).</summary>
        private const float PivotFromBottom = 2f;
        private static readonly Dictionary<string, RegionFrame> Bodies = new Dictionary<string, RegionFrame>();
        private static readonly Dictionary<string, PixelImage> PartImages = new Dictionary<string, PixelImage>();
        private static readonly HashSet<string> Missing = new HashSet<string>();
        private static readonly Dictionary<string, int> FrameCounts = new Dictionary<string, int>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, LayerLook> Looks = new Dictionary<string, LayerLook>();
        private static bool _unreadableWarned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            foreach (var s in Sprites.Values) if (s != null) { Object.Destroy(s.texture); Object.Destroy(s); }
            Sprites.Clear();
            Bodies.Clear();
            PartImages.Clear();
            Missing.Clear();
            FrameCounts.Clear();
            Looks.Clear();
            _any = -1;
            _unreadableWarned = false;
        }

        /// <summary>Çizilmiş kareleri bırakır (görünüm değişince ya da önbellek dolunca); okunmuş dokular kalır.</summary>
        public static void ClearSprites()
        {
            foreach (var s in Sprites.Values) if (s != null) { Object.Destroy(s.texture); Object.Destroy(s); }
            Sprites.Clear();
            Looks.Clear();
        }

        // ---------------- giriş noktaları ----------------

        /// <summary>Karakterin bir karesi: anim "idle", "walk2" ya da etkileşim anahtarı (cook, work...).</summary>
        public static Sprite Frame(Character c, string anim, string direction, int frame)
        {
            if (c == null || !AnyBody()) return null;   // her karede çağrılır: görsel yoksa hiçbir şey hesaplanmaz
            double grey = Catalog != null ? AgingLook.For(c, Catalog).Gray : 0.0;
            return Frame(c.Appearance, c.Sex, c.Stage, anim, direction, frame, grey);
        }

        public static Sprite Frame(Appearance a, Sex sex, AgeStage stage, string anim, string direction, int frame, double grey = 0.0)
        {
            if (!Enabled || a == null || stage == AgeStage.Baby || !AnyBody()) return null;
            string body = ResolveBody(a, sex, stage, anim, direction, frame);
            if (body == null) return null;

            int greyStep = (int)System.Math.Round(grey * 10.0);
            string lookKey = a.Key() + "|g" + greyStep;
            string key = lookKey + "|" + body + "|" + anim + "|" + direction + "|" + frame;
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;

            var bodyFrame = LoadBody(BodyName(body, anim, direction, frame));
            if (bodyFrame == null) return null;

            if (!Looks.TryGetValue(lookKey, out var look))
            {
                look = LayerLook.From(a, Catalog, greyStep / 10.0);
                Looks[lookKey] = look;
            }
            var img = LayerComposer.Compose(bodyFrame, bodyFrame.Anchor, direction, look, PartSource.Instance);
            var sprite = ToSprite(img, key);
            if (Sprites.Count >= CacheLimit) ClearSprites();
            Sprites[key] = sprite;
            return sprite;
        }

        /// <summary>Bu animasyon ve yönde kaç kare var (0 = katman görseli yok).</summary>
        public static int FrameCount(Character c, string anim, string direction)
        {
            if (!Enabled || c == null || c.Stage == AgeStage.Baby || !AnyBody()) return 0;
            string body = ResolveBody(c.Appearance, c.Sex, c.Stage, anim, direction, 0);
            return body == null ? 0 : CountFrames(body, anim, direction);
        }

        /// <summary>Katman görseli olan bir gövde var mı (yaratma ekranı önizlemesi için).</summary>
        public static bool Available(Sex sex, AgeStage stage) =>
            Enabled && ResolveBody(new Appearance(), sex, stage, "idle", "se", 0) != null;

        // ---------------- gövde seçimi ----------------

        public static string BodyKey(Appearance a, Sex sex, AgeStage stage, bool exactBuild)
        {
            bool child = stage == AgeStage.Child;
            string frame = a.Frame == BodyFrame.Masculine ? "m" : a.Frame == BodyFrame.Feminine ? "f" : (sex == Sex.Male ? "m" : "f");
            if (child) frame = "c" + (sex == Sex.Male ? "m" : "f");
            if (!exactBuild) return frame + "_orta";
            string build;
            switch (a.Build)
            {
                case BodyBuild.Slim: build = "ince"; break;
                case BodyBuild.Sturdy: build = child ? "orta" : "gurbuz"; break;
                case BodyBuild.Plump: build = child ? "orta" : "dolgun"; break;
                default: build = "orta"; break;
            }
            return frame + "_" + build;
        }

        /// <summary>Önce beden yapısının kendi gövdesi, yoksa Orta (beden betikle türetilmemişse).</summary>
        private static string ResolveBody(Appearance a, Sex sex, AgeStage stage, string anim, string direction, int frame)
        {
            string exact = BodyKey(a, sex, stage, true);
            if (HasTexture(BodyName(exact, anim, direction, frame))) return exact;
            string orta = BodyKey(a, sex, stage, false);
            return HasTexture(BodyName(orta, anim, direction, frame)) ? orta : null;
        }

        private static string BodyName(string body, string anim, string direction, int frame) =>
            "lay_body_" + body + "_" + anim + "_" + direction + "_" + frame.ToString("00");

        private static int CountFrames(string body, string anim, string direction)
        {
            string k = body + "|" + anim + "|" + direction;
            if (FrameCounts.TryGetValue(k, out int n)) return n;
            n = 0;
            while (n < 64 && HasTexture(BodyName(body, anim, direction, n))) n++;
            FrameCounts[k] = n;
            return n;
        }

        private static bool HasTexture(string name) => IsoArt.Texture(name) != null;

        private static int _any = -1;   // -1 bilinmiyor, 0 yok, 1 var

        /// <summary>Herhangi bir manken karesi var mı (pilot gelene kadar hayır; o zaman katman yolu hiç çalışmaz).</summary>
        private static bool AnyBody()
        {
            if (_any < 0)
            {
                _any = 0;
                foreach (var b in new[] { "m", "f", "cm", "cf" })
                    if (HasTexture("lay_body_" + b + "_orta_idle_se_00")) { _any = 1; break; }
            }
            return _any == 1;
        }

        // ---------------- doku okuma ----------------

        private static RegionFrame LoadBody(string name)
        {
            if (Bodies.TryGetValue(name, out var f)) return f;
            var img = Read(name);
            f = img == null ? null : RegionFrame.Decode(img);
            Bodies[name] = f;
            return f;
        }

        /// <summary>Unity dokusu (alttan yukarı) → PixelImage (üstten aşağı). Doku okunabilir değilse null.</summary>
        private static PixelImage Read(string name)
        {
            if (Missing.Contains(name)) return null;
            var tex = IsoArt.Texture(name);
            if (tex == null) { Missing.Add(name); return null; }
            if (!tex.isReadable)
            {
                if (!_unreadableWarned)
                {
                    Debug.LogWarning($"Katman görseli okunamıyor ({name}): Read/Write kapalı. Assets/Art/Iso/katman yeniden içe aktarılmalı.");
                    _unreadableWarned = true;
                }
                Missing.Add(name);
                return null;
            }
            var src = tex.GetPixels32();
            int w = tex.width, h = tex.height;
            var img = new PixelImage(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var p = src[(h - 1 - y) * w + x];
                img.Px[y * w + x] = new Px32(p.r, p.g, p.b, p.a);
            }
            return img;
        }

        private static Sprite ToSprite(PixelImage img, string name)
        {
            var tex = new Texture2D(img.Width, img.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "lay_" + name.GetHashCode().ToString("X8")
            };
            var buf = new Color32[img.Width * img.Height];
            for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var p = img.Px[y * img.Width + x];
                buf[(img.Height - 1 - y) * img.Width + x] = new Color32(p.R, p.G, p.B, p.A);
            }
            tex.SetPixels32(buf);
            tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, img.Width, img.Height),
                new Vector2(0.5f, PivotFromBottom / img.Height), Ppu);
            sprite.name = tex.name;
            return sprite;
        }

        private sealed class PartSource : IPartSource
        {
            public static readonly PartSource Instance = new PartSource();

            public PixelImage Get(string slot, string id, string direction, bool back)
            {
                string name = "lay_" + slot + "_" + id + "_" + direction + (back ? "_bk" : "_fr");
                if (PartImages.TryGetValue(name, out var img)) return img;
                img = Read(name);
                PartImages[name] = img;
                return img;
            }
        }
    }
}
