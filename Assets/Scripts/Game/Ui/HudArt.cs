using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// HUD v2 görselleri (Project belgesi 66 §5 S6): meşe paneller (iş 774, 745 sayfasından kesim), ilmek düğmeler ve rozet
    /// (iş 744), ikonlar (iş 754). Dosyalar Resources/UI/Hud/ altında; ölçüler ve dokuz dilim değerleri 774 raporundan.
    /// Doku yoksa (henüz üretilmemiş ikon gibi) çağıran yazıya ya da düz renge düşer.
    /// </summary>
    public static class HudArt
    {
        public const string Clock = "hud_clock", Money = "hud_money", Tracker = "hud_tracker", Frame = "hud_frame", Bar = "hud_bar",
            FillGreen = "hud_fill_green", FillAmber = "hud_fill_amber", FillRed = "hud_fill_red", Window = "hud_window",
            BtnBuild = "hud_btn_build", BtnBuildDown = "hud_btn_build_down", BtnBook = "hud_btn_book", BtnBookDown = "hud_btn_book_down",
            Badge = "hud_badge";

        /// <summary>Dokuz dilim kenarları (sol, sağ, üst, alt), 774 raporu.</summary>
        public static readonly (int l, int r, int t, int b) TrackerSlice = (21, 22, 0, 0), FrameSlice = (8, 8, 8, 8),
            BarSlice = (8, 8, 0, 0), WindowSlice = (32, 32, 32, 32);

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => Cache.Clear();

        public static Texture2D Tex(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Cache.TryGetValue(name, out var t)) return t;
            t = Resources.Load<Texture2D>("UI/Hud/" + name);
            Cache[name] = t;
            return t;
        }

        /// <summary>HUD ikonu (754): Resources/UI/Hud/Icons/hud_icon_&lt;anahtar&gt;. Henüz yoksa null.</summary>
        public static Texture2D Icon(string key) => string.IsNullOrEmpty(key) ? null : Tex("Icons/hud_icon_" + key);

        /// <summary>Dokuyu arka plan yapar; dilim verilirse dokuz dilimli gerilir. Doku yoksa düz renk.</summary>
        public static bool Apply(VisualElement e, string name, (int l, int r, int t, int b)? slice, Color fallback)
        {
            var tex = Tex(name);
            if (tex == null)
            {
                e.style.backgroundImage = StyleKeyword.None;
                e.style.backgroundColor = fallback;
                return false;
            }
            e.style.backgroundColor = Color.clear;
            e.style.backgroundImage = new StyleBackground(tex);
            if (slice.HasValue)
            {
                var s = slice.Value;
                e.style.unitySliceLeft = s.l; e.style.unitySliceRight = s.r; e.style.unitySliceTop = s.t; e.style.unitySliceBottom = s.b;
            }
            return true;
        }

        /// <summary>Dokuyu kendi boyutunun tam sayı katında gösterir (piksel bulanıklaşmasın). Doku yoksa verilen boyut.</summary>
        public static void Fixed(VisualElement e, string name, int factor, float fallbackW, float fallbackH, Color fallback)
        {
            var tex = Tex(name);
            if (tex == null)
            {
                e.style.width = fallbackW; e.style.height = fallbackH;
                e.style.backgroundImage = StyleKeyword.None;
                e.style.backgroundColor = fallback;
                return;
            }
            e.style.width = tex.width * factor;
            e.style.height = tex.height * factor;
            e.style.backgroundColor = Color.clear;
            e.style.backgroundImage = new StyleBackground(tex);
        }

        public static void Pad(VisualElement e, float l, float r, float t, float b)
        {
            e.style.paddingLeft = l; e.style.paddingRight = r; e.style.paddingTop = t; e.style.paddingBottom = b;
        }

        public static void NoBorder(VisualElement e)
        {
            e.style.borderLeftWidth = 0; e.style.borderRightWidth = 0; e.style.borderTopWidth = 0; e.style.borderBottomWidth = 0;
        }
    }
}
