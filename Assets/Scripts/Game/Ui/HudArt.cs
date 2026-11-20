using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// HUD v2 görselleri (Project belgesi 66; kullanıcı kararı 21.11.2026: saf meşe, HUD dünya ile aynı piksel boyutu).
    /// İki takım aranır, önce dünya ölçeğindeki:
    /// - <c>Resources/UI/Hud/x2/</c>: "mese2" parçaları (işler 810, 811, 812). Yerel düzen 480x270; arayüz biriminde 2× çizilir.
    /// - <c>Resources/UI/Hud/</c>: 774'ün kestiği büyük meşe parçaları (745 sayfasından), 1× çizilir. 810 gelene kadar paneller buradan.
    /// Doku yoksa (henüz üretilmemiş) çağıran yazıya ya da düz renge düşer.
    /// </summary>
    public static class HudArt
    {
        public const string Clock = "hud_clock", Money = "hud_money", Tracker = "hud_tracker", Frame = "hud_frame", Bar = "hud_bar",
            FillGreen = "hud_fill_green", FillAmber = "hud_fill_amber", FillRed = "hud_fill_red", Window = "hud_window",
            BtnBuild = "hud_btn_build", BtnBuildDown = "hud_btn_build_down", BtnBook = "hud_btn_book", BtnBookDown = "hud_btn_book_down",
            Badge = "hud_badge";

        /// <summary>Bir HUD dokusu ve arayüz biriminde kaç kat çizileceği.</summary>
        public readonly struct Art
        {
            public readonly Texture2D Tex;
            public readonly int Factor;
            public Art(Texture2D tex, int factor) { Tex = tex; Factor = factor; }
            public bool Ok => Tex != null;
            public float Width => Tex != null ? Tex.width * Factor : 0f;
            public float Height => Tex != null ? Tex.height * Factor : 0f;
            /// <summary>Dünya ölçeğindeki (mese2) takım mı.</summary>
            public bool World => Factor >= 2;
        }

        // Dokuz dilim kenarları (sol, sağ, üst, alt), doku pikseli. 774: rapor değerleri; mese2: 810 iş tanımı.
        private static readonly Dictionary<string, (int l, int r, int t, int b)> Slice1 = new Dictionary<string, (int, int, int, int)>
        {
            { Tracker, (21, 22, 0, 0) }, { Frame, (8, 8, 8, 8) }, { Bar, (8, 8, 0, 0) }, { Window, (32, 32, 32, 32) },
        };
        private static readonly Dictionary<string, (int l, int r, int t, int b)> Slice2 = new Dictionary<string, (int, int, int, int)>
        {
            { Tracker, (6, 6, 0, 0) }, { Frame, (6, 6, 6, 6) }, { Bar, (3, 3, 0, 0) },
        };

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => Cache.Clear();

        private static Texture2D Load(string path)
        {
            if (Cache.TryGetValue(path, out var t)) return t;
            t = Resources.Load<Texture2D>("UI/Hud/" + path);
            Cache[path] = t;
            return t;
        }

        /// <summary>Önce dünya ölçeğindeki (x2) doku, yoksa büyük (1×) doku.</summary>
        public static Art Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return default;
            var t = Load("x2/" + name);
            if (t != null) return new Art(t, 2);
            return new Art(Load(name), 1);
        }

        public static Texture2D Tex(string name) => Get(name).Tex;

        /// <summary>HUD ikonu (812): <c>Icons/hud_icon_&lt;anahtar&gt;</c>, önce x2 takımı. Henüz yoksa boş.</summary>
        public static Art Icon(string key) => string.IsNullOrEmpty(key) ? default : Get("Icons/hud_icon_" + key);

        /// <summary>Dokuyu arka plan yapar, varsa dokuz dilimli gerer (dilimler takımın ölçeğiyle büyür). Doku yoksa düz renk.</summary>
        public static Art Apply(VisualElement e, string name, Color fallback)
        {
            var art = Get(name);
            if (!art.Ok)
            {
                e.style.backgroundImage = StyleKeyword.None;
                e.style.backgroundColor = fallback;
                return art;
            }
            e.style.backgroundColor = Color.clear;
            e.style.backgroundImage = new StyleBackground(art.Tex);
            var table = art.World ? Slice2 : Slice1;
            if (table.TryGetValue(name, out var s))
            {
                e.style.unitySliceLeft = s.l; e.style.unitySliceRight = s.r; e.style.unitySliceTop = s.t; e.style.unitySliceBottom = s.b;
                e.style.unitySliceScale = art.Factor;
            }
            return art;
        }

        /// <summary>Dokuyu kendi boyutunun tam sayı katında gösterir (takım ölçeği × <paramref name="extra"/>). Doku yoksa verilen boyut.</summary>
        public static Art Fixed(VisualElement e, string name, int extra, float fallbackW, float fallbackH, Color fallback)
        {
            var art = Get(name);
            if (!art.Ok)
            {
                e.style.width = fallbackW; e.style.height = fallbackH;
                e.style.backgroundImage = StyleKeyword.None;
                e.style.backgroundColor = fallback;
                return art;
            }
            e.style.width = art.Width * extra;
            e.style.height = art.Height * extra;
            e.style.backgroundColor = Color.clear;
            e.style.backgroundImage = new StyleBackground(art.Tex);
            return art;
        }

        public static void Pad(VisualElement e, float l, float r, float t, float b)
        {
            e.style.paddingLeft = l; e.style.paddingRight = r; e.style.paddingTop = t; e.style.paddingBottom = b;
        }
    }
}
