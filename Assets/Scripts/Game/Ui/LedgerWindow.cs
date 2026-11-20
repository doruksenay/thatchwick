using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Thatchwick.Core.Life;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Defter penceresi (Project belgesi 66 karar S2): sağ alttaki Defter düğmesi açar. Sarmaşıklı meşe çerçeve (774
    /// <c>hud_window</c>, 2× sabit boy), ortada bölüm düğmeleri. Bir bölüm seçilince pencere kapanır ve o bölümün paneli
    /// açılır (paneller henüz IMGUI'de; tek tek UI Toolkit'e taşınınca pencerenin içine gömülecek). "Bildirimler" ve
    /// "Seçenekler" bölümleri pencerenin içinde UI Toolkit ile çizilir.
    /// Bir anda tek pencere; arka plan karartılır; dışına dokunmak ya da Esc kapatır.
    /// </summary>
    public sealed class LedgerWindow
    {
        public sealed class Section
        {
            public string Id;
            public Func<string> Title;
            public Func<bool> Available;
            public Func<int> Count;
            /// <summary>Seçilince yapılacak iş (panel açmak). Null ise bölüm pencerenin içinde çizilir.</summary>
            public Action Open;
        }

        /// <summary>"Seçenekler" bölümündeki aç-kapa satırı (etiketler, efekt, kaydet...).</summary>
        public sealed class Option
        {
            public Func<string> Text;
            public Func<bool> On;
            public Func<bool> Visible;
            public Action Click;
        }

        private const int Factor = 2;
        private const float FallbackW = 450f, FallbackH = 344f, Inset = 58f;
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.4f);

        private readonly VisualElement _overlay, _window, _grid, _page;
        private readonly Label _title, _pageTitle;
        private readonly KitButton _back;
        private readonly VisualElement _head;
        private readonly List<(Section section, KitButton button)> _buttons = new List<(Section, KitButton)>();
        private readonly List<(Option option, KitButton button)> _options = new List<(Option, KitButton)>();
        private readonly Func<IReadOnlyList<NoticeEntry>> _notices;
        private readonly Func<double, string> _dateText;
        private readonly Action _markRead;
        private readonly Func<string> _titleText, _noticesTitle, _optionsTitle, _backText, _emptyText;

        public LedgerWindow(UiHost host, IReadOnlyList<Section> sections, IReadOnlyList<Option> options,
            Func<IReadOnlyList<NoticeEntry>> notices, Func<double, string> dateText, Action markRead,
            Func<string> title, Func<string> noticesTitle, Func<string> optionsTitle, Func<string> backText, Func<string> emptyText)
        {
            _notices = notices; _dateText = dateText; _markRead = markRead;
            _titleText = title; _noticesTitle = noticesTitle; _optionsTitle = optionsTitle; _backText = backText; _emptyText = emptyText;

            _overlay = new VisualElement { pickingMode = PickingMode.Position };
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0; _overlay.style.right = 0; _overlay.style.top = 0; _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = Dim;
            _overlay.style.alignItems = Align.Center;
            _overlay.style.justifyContent = Justify.Center;
            _overlay.RegisterCallback<PointerDownEvent>(e => { Visible = false; e.StopPropagation(); });

            _window = new VisualElement { pickingMode = PickingMode.Position };
            HudArt.Fixed(_window, HudArt.Window, Factor, FallbackW, FallbackH, new Color(0.29f, 0.2f, 0.14f, 0.97f));
            HudArt.Pad(_window, Inset, Inset, Inset - 8f, Inset);
            _window.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());   // pencerenin içi kapatmaz

            _title = Heading();
            _window.Add(_title);

            _grid = new VisualElement { pickingMode = PickingMode.Ignore };
            _grid.style.flexDirection = FlexDirection.Row;
            _grid.style.flexWrap = Wrap.Wrap;
            _grid.style.justifyContent = Justify.Center;
            _grid.style.marginTop = 4;
            foreach (var s in sections)
            {
                var captured = s;
                var b = new KitButton("", () => Choose(captured));
                b.style.width = 104;
                b.style.height = 26;
                b.style.marginLeft = 2; b.style.marginRight = 2; b.style.marginTop = 2; b.style.marginBottom = 2;
                _grid.Add(b);
                _buttons.Add((s, b));
            }
            _window.Add(_grid);

            _page = new VisualElement { pickingMode = PickingMode.Ignore };
            _page.style.flexGrow = 1;
            _page.style.overflow = Overflow.Hidden;
            _page.style.display = DisplayStyle.None;
            _head = new VisualElement { pickingMode = PickingMode.Ignore };
            _head.style.flexDirection = FlexDirection.Row;
            _head.style.alignItems = Align.Center;
            _back = new KitButton("", () => ShowPage(null));
            _back.style.width = 64;
            _head.Add(_back);
            _pageTitle = Heading();
            _pageTitle.style.marginLeft = 6;
            _head.Add(_pageTitle);
            _page.Add(_head);
            _window.Add(_page);

            foreach (var o in options)
            {
                var captured = o;
                var b = new KitButton("", () => { captured.Click?.Invoke(); Refresh(); });
                b.style.width = 160;
                b.style.marginTop = 2;
                _options.Add((o, b));
            }

            _overlay.Add(_window);
            host.Root.Add(_overlay);
            Visible = false;
        }

        public bool Visible
        {
            get => _overlay.style.display != DisplayStyle.None;
            set
            {
                var d = value ? DisplayStyle.Flex : DisplayStyle.None;
                if (_overlay.style.display == d) return;
                _overlay.style.display = d;
                if (value) { ShowPage(null); Refresh(); }
            }
        }

        public void Toggle() => Visible = !Visible;

        /// <summary>Bildirimler sayfasıyla açar (görev izleyici, bildirim kartı).</summary>
        public void OpenNotices() { Visible = true; ShowPage("notices"); }

        public void Refresh()
        {
            if (!Visible) return;
            SetText(_title, _titleText?.Invoke());
            foreach (var (s, b) in _buttons)
            {
                bool available = s.Available == null || s.Available();
                var d = available ? DisplayStyle.Flex : DisplayStyle.None;
                if (b.style.display != d) b.style.display = d;
                if (!available) continue;
                int n = s.Count != null ? s.Count() : 0;
                string t = s.Title?.Invoke() ?? s.Id;
                if (n > 0) t += " (" + n + ")";
                if (b.text != t) b.text = t;
                b.On = n > 0;
            }
            foreach (var (o, b) in _options)
            {
                bool visible = o.Visible == null || o.Visible();
                var d = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (b.style.display != d) b.style.display = d;
                string t = o.Text?.Invoke() ?? "";
                if (b.text != t) b.text = t;
                b.On = o.On != null && o.On();
            }
            string bt = _backText?.Invoke() ?? "<";
            if (_back.text != bt) _back.text = bt;
        }

        private void Choose(Section s)
        {
            if (s.Open != null) { Visible = false; s.Open(); return; }
            ShowPage(s.Id);
        }

        private void ShowPage(string kind)
        {
            _grid.style.display = kind == null ? DisplayStyle.Flex : DisplayStyle.None;
            _page.style.display = kind == null ? DisplayStyle.None : DisplayStyle.Flex;
            // İç sayfanın gövdesi her açılışta baştan kurulur (başlık satırı kalır).
            _page.Clear();
            _page.Add(_head);
            if (kind == LedgerSections.Notices) BuildNotices();
            else if (kind == LedgerSections.Settings) BuildOptions();
            Refresh();
        }

        private void BuildNotices()
        {
            SetText(_pageTitle, _noticesTitle?.Invoke());
            var list = _notices != null ? _notices() : null;
            if (list == null || list.Count == 0) { _page.Add(Line(_emptyText?.Invoke() ?? "", UiSkin.Ink)); return; }
            int shown = 0;
            foreach (var e in list)
            {
                if (shown++ >= 14) break;   // pencere sabit boy: en yeni 14 kayıt (kaydırma sonra)
                string text = (_dateText != null ? _dateText(e.Day) + ": " : "") + e.Text + (e.Repeats > 1 ? " (x" + e.Repeats + ")" : "");
                Color color = e.Level == NoticeLevel.Danger ? new Color(0.56f, 0.14f, 0.14f)
                    : e.Level == NoticeLevel.Warning ? new Color(0.6f, 0.29f, 0.11f) : UiSkin.Ink;
                _page.Add(Line(text, color));
            }
            _markRead?.Invoke();
        }

        private void BuildOptions()
        {
            SetText(_pageTitle, _optionsTitle?.Invoke());
            var col = new VisualElement { pickingMode = PickingMode.Ignore };
            col.style.alignItems = Align.Center;
            col.style.marginTop = 4;
            foreach (var (_, b) in _options) col.Add(b);
            _page.Add(col);
        }

        private static Label Line(string text, Color color)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.style.fontSize = 10;
            l.style.whiteSpace = WhiteSpace.NoWrap;
            l.style.overflow = Overflow.Hidden;
            l.style.color = color;
            l.style.marginTop = 1; l.style.marginBottom = 0; l.style.marginLeft = 0; l.style.marginRight = 0;
            return l;
        }

        private static Label Heading()
        {
            var l = new Label("") { pickingMode = PickingMode.Ignore };
            l.style.fontSize = 14;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.style.unityTextAlign = TextAnchor.MiddleCenter;
            l.style.color = UiSkin.Ink;
            return l;
        }

        private static void SetText(Label l, string text)
        {
            text = text ?? "";
            if (l.text != text) l.text = text;
        }
    }
}
