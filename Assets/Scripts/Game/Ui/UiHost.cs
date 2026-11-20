using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// UI Toolkit kökü (performans incelemesi 20.11.2026, IMGUI'den geçişin altyapısı). Sahne ve asset gerektirmez:
    /// UIDocument ve PanelSettings koddan kurulur, görünüm arayüz kitinin (UiSkin) dokuları ve renkleriyle satır içi
    /// stil olarak verilir. Ölçek tam sayıdır (HUD v2, belge 66 §3.1): ekran yüksekliği / 540'ın tabanı (en az 1), böylece
    /// piksel sanatı bulanıklaşmaz; arayüz alanı en az 540 birim yüksekliktir.
    /// IMGUI her karede birkaç kez yeniden çizilir ve metin üretir; burada öğeler bir kez kurulur, yalnızca değişen metin yazılır.
    /// </summary>
    public sealed class UiHost
    {
        public const int ReferenceHeight = 540;

        private readonly GameObject _go;
        private readonly UIDocument _document;
        private readonly PanelSettings _settings;
        private int _lastHeight;

        public VisualElement Root { get; }

        public UiHost(Transform parent)
        {
            _go = new GameObject("UiHost");
            _go.SetActive(false);   // panel ayarı atanmadan OnEnable çalışmasın
            _go.transform.SetParent(parent, false);

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "ThatchwickPanel";
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            settings.scale = IntegerScale(Screen.height);
            settings.sortingOrder = 100;
            _settings = settings;

            _document = _go.AddComponent<UIDocument>();
            _document.panelSettings = settings;
            _go.SetActive(true);

            Root = _document.rootVisualElement;
            if (Root == null) return;
            Root.pickingMode = PickingMode.Ignore;   // boş alan dokunmayı dünyaya bırakır
            Root.style.flexGrow = 1f;
            Root.style.color = UiSkin.Ink;
            Root.style.fontSize = 12;
            // Tema stil sayfası yok: yazı tipi kökte verilir ve bütün öğelere miras kalır.
            var font = Resources.Load<Font>("UI/PixelifySans-Regular");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) Root.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
        }

        public bool Ready => Root != null;

        /// <summary>Arayüz biriminin ekran pikseli karşılığı (tam sayı).</summary>
        public float Scale => _settings != null ? _settings.scale : 1f;

        /// <summary>Ekran yüksekliğine göre tam sayı ölçek: 540'ın katları (1080 → 2, 1170 → 2, 1620 → 3).</summary>
        public static float IntegerScale(int screenHeight) => Mathf.Max(1f, Mathf.Floor(screenHeight / (float)ReferenceHeight));

        /// <summary>Çözünürlük değiştiyse ölçeği yeniler (her karede çağrılabilir; yalnızca değişince yazar).</summary>
        public void UpdateScale()
        {
            if (_settings == null || Screen.height == _lastHeight) return;
            _lastHeight = Screen.height;
            _settings.scale = IntegerScale(Screen.height);
        }

        public bool Visible
        {
            get => Root != null && Root.style.display != DisplayStyle.None;
            set { if (Root != null) Root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None; }
        }

        /// <summary>Ekran noktası (sol-alt köşe sıfır) bir arayüz öğesinin üstünde mi (dokunma dünyaya gitmesin).</summary>
        public bool IsPointerOver(Vector2 screenPosition)
        {
            if (Root == null || Root.panel == null || !Visible) return false;
            var flipped = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            var panelPos = RuntimePanelUtils.ScreenToPanel(Root.panel, flipped);
            return Root.panel.Pick(panelPos) != null;
        }

        public void Destroy()
        {
            if (_go != null) UnityEngine.Object.Destroy(_go);
        }

        // ---------------- kit ----------------

        /// <summary>Dokuz dilimli arka plan (arayüz kiti dokusu). Doku yoksa düz renk.</summary>
        public static void SetSliced(VisualElement e, string kitTexture, int slice, Color fallback)
        {
            var tex = UiSkin.KitTexture(kitTexture);
            if (tex == null)
            {
                e.style.backgroundImage = StyleKeyword.None;
                e.style.backgroundColor = fallback;
                return;
            }
            e.style.backgroundColor = Color.clear;
            e.style.backgroundImage = new StyleBackground(tex);
            e.style.unitySliceLeft = slice;
            e.style.unitySliceRight = slice;
            e.style.unitySliceTop = slice;
            e.style.unitySliceBottom = slice;
        }

        public static VisualElement Panel()
        {
            var p = new VisualElement();
            p.style.paddingLeft = 6; p.style.paddingRight = 6; p.style.paddingTop = 6; p.style.paddingBottom = 6;
            SetSliced(p, "panel", 14, new Color(0.91f, 0.87f, 0.78f, 0.96f));
            return p;
        }

        public static VisualElement Row()
        {
            var r = new VisualElement();
            r.style.flexDirection = FlexDirection.Row;
            r.style.flexWrap = Wrap.Wrap;
            r.style.alignItems = Align.Center;
            return r;
        }

        public static Label Text(string text = "")
        {
            var l = new Label(text);
            l.pickingMode = PickingMode.Ignore;
            l.style.marginLeft = 2; l.style.marginRight = 2; l.style.marginTop = 1; l.style.marginBottom = 1;
            l.style.paddingLeft = 0; l.style.paddingRight = 0; l.style.paddingTop = 0; l.style.paddingBottom = 0;
            l.style.whiteSpace = WhiteSpace.Normal;
            return l;
        }
    }

    /// <summary>
    /// Arayüz kitinin düğmesi: normal, üstünde, basılı ve seçili (açık) görünüm. UI Toolkit'in Button'ı tıklamayı yönetir;
    /// görünüm tema yerine kit dokularıyla satır içi verilir.
    /// </summary>
    public sealed class KitButton : Button
    {
        private bool _on, _hover, _down;

        public KitButton(string text, Action onClick) : base(onClick)
        {
            this.text = text;
            style.height = 22;
            style.fontSize = 11;
            style.marginLeft = 1; style.marginRight = 1; style.marginTop = 1; style.marginBottom = 1;
            style.paddingLeft = 5; style.paddingRight = 5; style.paddingTop = 2; style.paddingBottom = 2;
            style.borderLeftWidth = 0; style.borderRightWidth = 0; style.borderTopWidth = 0; style.borderBottomWidth = 0;
            style.unityTextAlign = TextAnchor.MiddleCenter;
            RegisterCallback<PointerEnterEvent>(e => { _hover = true; Apply(); });
            RegisterCallback<PointerLeaveEvent>(e => { _hover = false; _down = false; Apply(); });
            RegisterCallback<PointerDownEvent>(e => { _down = true; Apply(); }, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(e => { _down = false; Apply(); }, TrickleDown.TrickleDown);
            Apply();
        }

        /// <summary>Açık (seçili) durum: altın yazı ve seçili doku.</summary>
        public bool On
        {
            get => _on;
            set { if (_on == value) return; _on = value; Apply(); }
        }

        private void Apply()
        {
            string tex = _down ? "btn_press" : _on ? "btn_sel" : _hover ? "btn_hover" : "btn";
            UiHost.SetSliced(this, tex, 12, _on ? new Color(0.36f, 0.24f, 0.2f) : new Color(0.28f, 0.22f, 0.2f));
            style.color = _on ? UiSkin.Gold : UiSkin.Linen;
        }
    }
}
