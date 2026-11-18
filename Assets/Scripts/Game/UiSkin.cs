using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Goblen/parşömen arayüz teması (391 arayüz kiti, belge 33). Varsayılan IMGUI skin'inin bir kopyası;
    /// panel, düğme, sekme, metin kutusu ve kaydırma çubuğu 9-slice parçalarla çizilir.
    /// Kenar payları 391 raporundan. Parçalar Resources/UI altında.
    /// </summary>
    public static class UiSkin
    {
        public static readonly Color Ink = new Color32(0x2F, 0x29, 0x26, 0xFF);
        public static readonly Color Linen = new Color32(0xE9, 0xDD, 0xC7, 0xFF);
        public static readonly Color Gold = new Color32(0xE8, 0xC2, 0x6A, 0xFF);

        private static GUISkin _skin;
        private static GUIStyle _cell, _cellSel, _cellLock, _badgeNew;

        public static GUISkin Skin { get { Build(); return _skin; } }
        public static GUIStyle Cell { get { Build(); return _cell; } }
        public static GUIStyle CellSelected { get { Build(); return _cellSel; } }
        public static GUIStyle CellLocked { get { Build(); return _cellLock; } }
        public static GUIStyle BadgeNew { get { Build(); return _badgeNew; } }

        private static Texture2D Tex(string n) => Resources.Load<Texture2D>("UI/ui_kit_" + n);

        // Menü ikonları (392-394, 399a/b): Resources/UI/Icons/ui_icon_<anahtar>. Yoksa null (düğme yazıyla kalır).
        private static readonly System.Collections.Generic.Dictionary<string, Texture2D> _icons = new System.Collections.Generic.Dictionary<string, Texture2D>();
        // Projede Play'e girerken alan yeniden yüklenmiyor (Enter Play Mode Options): önbellekler her Play'de sıfırlanır,
        // yoksa önceki oturumda bulunamayan ikon "yok" olarak kalır.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCaches() { _icons.Clear(); _skin = null; }

        public static Texture2D Icon(params string[] keys)
        {
            foreach (var k in keys)
            {
                if (string.IsNullOrEmpty(k)) continue;
                if (!_icons.TryGetValue(k, out var t)) { t = Resources.Load<Texture2D>("UI/Icons/ui_icon_" + k); _icons[k] = t; }
                if (t != null) return t;
            }
            return null;
        }

        /// <summary>Yazı + ikon (ikon yoksa yalnız yazı).</summary>
        public static GUIContent C(string text, params string[] iconKeys) => new GUIContent(text, Icon(iconKeys));

        private static void Build()
        {
            if (_skin != null) return;
            var baseSkin = GUI.skin;
            _skin = Object.Instantiate(baseSkin);
            _skin.name = "ThatchwickSkin";

            var box = _skin.box;
            box.normal.background = Tex("panel");
            box.border = new RectOffset(14, 14, 14, 14);
            box.padding = new RectOffset(6, 6, 6, 6);
            box.normal.textColor = Ink;

            var label = _skin.label;
            label.normal.textColor = Ink;

            var b = _skin.button;
            b.normal.background = Tex("btn");
            b.hover.background = Tex("btn_hover");
            b.active.background = Tex("btn_press");
            b.focused.background = Tex("btn");
            b.onNormal.background = Tex("btn_sel");
            b.onHover.background = Tex("btn_sel");
            b.onActive.background = Tex("btn_press");
            b.onFocused.background = Tex("btn_sel");
            b.border = new RectOffset(12, 12, 12, 12);
            b.padding = new RectOffset(2, 2, 4, 4);   // dar paneller (Pazar "Sat 1") eski skin genişliklerine göre
            b.margin = new RectOffset(1, 1, 1, 1);
            b.fixedHeight = 24f;
            b.fontSize = 11;
            b.clipping = TextClipping.Clip;
            b.normal.textColor = b.hover.textColor = b.active.textColor = b.focused.textColor = Linen;
            b.onNormal.textColor = b.onHover.textColor = b.onActive.textColor = b.onFocused.textColor = Gold;
            b.fontStyle = FontStyle.Normal;   // kalın yazı dar düğmelerde kesiliyordu

            // Toolbar özel stilleri (ButtonLeft/Mid/Right) da aynı düğme görünümünü alsın.
            foreach (var n in new[] { "ButtonLeft", "ButtonMid", "ButtonRight" })
            {
                var st = _skin.FindStyle(n);
                if (st == null) continue;
                st.normal = b.normal; st.hover = b.hover; st.active = b.active; st.focused = b.focused;
                st.onNormal = b.onNormal; st.onHover = b.onHover; st.onActive = b.onActive; st.onFocused = b.onFocused;
                st.border = b.border; st.padding = b.padding; st.margin = b.margin; st.fixedHeight = b.fixedHeight; st.fontStyle = b.fontStyle; st.fontSize = b.fontSize;
            }

            var t = _skin.textField;
            t.normal.background = t.hover.background = t.focused.background = t.active.background = Tex("field");
            t.border = new RectOffset(8, 8, 8, 8);
            t.padding = new RectOffset(6, 6, 4, 4);
            t.fixedHeight = 24f;
            t.normal.textColor = t.hover.textColor = t.focused.textColor = t.active.textColor = Ink;

            var vs = _skin.verticalScrollbar;
            vs.normal.background = Tex("scroll_track");
            vs.border = new RectOffset(6, 6, 8, 8);
            vs.fixedWidth = 14f;
            var th = _skin.verticalScrollbarThumb;
            th.normal.background = Tex("scroll_thumb");
            th.border = new RectOffset(6, 6, 8, 8);
            th.fixedWidth = 14f;

            _skin.window.normal.background = Tex("panel");
            _skin.window.border = new RectOffset(14, 14, 14, 14);

            _cell = Slice("cell", 10); _cellSel = Slice("cell_sel", 10); _cellLock = Slice("cell_lock", 10);
            _badgeNew = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 10 };
            _badgeNew.normal.background = Tex("badge_new");
            _badgeNew.border = new RectOffset(6, 6, 6, 6);
        }

        private static GUIStyle Slice(string n, int border)
        {
            var s = new GUIStyle(_skin.box);
            s.normal.background = Tex(n);
            s.border = new RectOffset(border, border, border, border);
            s.padding = new RectOffset(6, 6, 6, 6);
            return s;
        }
    }
}
