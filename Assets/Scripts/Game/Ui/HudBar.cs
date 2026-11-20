using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Sol üstteki küçük HUD'un UI Toolkit sürümü (IMGUI'den taşınan ilk ekran): tarih ve hız, panel düğmeleri, akçe ve
    /// itibar, kısa mesaj. Öğeler bir kez kurulur; <see cref="Refresh"/> saniyede ~10 kez yalnızca değişen metni ve
    /// durumu yazar. Oyun durumunu GameRoot'un verdiği işlevlerden okur (GameRoot'un iç alanlarını görmez).
    /// </summary>
    public sealed class HudBar
    {
        /// <summary>Bir HUD öğesi: düğme (Click dolu) ya da yazı.</summary>
        public sealed class Item
        {
            public Func<string> Text;
            public Action Click;
            public Func<bool> On;
            public Func<bool> Visible;
            public Func<string> Tooltip;
            /// <summary>Yazı rengi (null: varsayılan). Akçe artınca yeşil, azalınca kırmızı yanıp söner.</summary>
            public Func<Color?> Tint;
            public float Width;
        }

        public const float Left = 8f, Top = 8f, Width = 340f;
        private const float RefreshSeconds = 0.1f;

        private readonly VisualElement _panel;
        private readonly Label _date, _status, _message;
        private readonly Func<string> _dateText, _statusText, _messageText;
        private readonly Func<int> _speed;
        private readonly List<(int speed, KitButton button)> _speedButtons = new List<(int, KitButton)>();
        private readonly List<(Item item, VisualElement element)> _items = new List<(Item, VisualElement)>();
        private float _nextRefresh;

        public HudBar(UiHost host, Func<string> dateText, Func<string> statusText, Func<string> messageText,
            IReadOnlyList<int> speeds, Func<int> speed, Action<int> setSpeed, IReadOnlyList<Item> panelRow, IReadOnlyList<Item> infoRow)
        {
            _dateText = dateText;
            _statusText = statusText;
            _messageText = messageText;
            _speed = speed;

            _panel = UiHost.Panel();
            _panel.style.position = Position.Absolute;
            _panel.style.left = Left;
            _panel.style.top = Top;
            _panel.style.width = Width;

            var top = UiHost.Row();
            top.style.flexWrap = Wrap.NoWrap;
            _date = UiHost.Text();
            _date.style.flexGrow = 1f;
            top.Add(_date);
            foreach (int s in speeds)
            {
                int captured = s;
                var b = new KitButton(s + "x", () => { setSpeed(captured); Refresh(true); });
                b.style.width = 34;
                top.Add(b);
                _speedButtons.Add((s, b));
            }
            _panel.Add(top);

            _status = UiHost.Text();
            _panel.Add(_status);
            _panel.Add(BuildRow(panelRow));
            _panel.Add(BuildRow(infoRow));
            _message = UiHost.Text();
            _panel.Add(_message);

            host.Root.Add(_panel);
            Refresh(true);
        }

        public bool Visible
        {
            get => _panel.style.display != DisplayStyle.None;
            set => _panel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Panelin alt kenarı (540 birimlik arayüz ölçüsünde): IMGUI ayrıntı alanı bunun altından başlar.</summary>
        public float Bottom
        {
            get
            {
                float y = _panel.worldBound.yMax;
                return float.IsNaN(y) || y <= 0f ? Top + 96f : y;
            }
        }

        private VisualElement BuildRow(IReadOnlyList<Item> items)
        {
            var row = UiHost.Row();
            foreach (var item in items)
            {
                VisualElement e;
                if (item.Click != null)
                {
                    var captured = item;
                    e = new KitButton("", () => { captured.Click(); Refresh(true); });
                }
                else e = UiHost.Text();
                if (item.Width > 0f) e.style.width = item.Width;
                row.Add(e);
                _items.Add((item, e));
            }
            return row;
        }

        /// <summary>Metin ve durumları günceller (saniyede ~10 kez; <paramref name="force"/> hemen).</summary>
        public void Refresh(bool force = false)
        {
            float now = Time.unscaledTime;
            if (!force && now < _nextRefresh) return;
            _nextRefresh = now + RefreshSeconds;

            SetText(_date, _dateText());
            SetOptional(_status, _statusText());
            SetOptional(_message, _messageText());
            int speed = _speed();
            foreach (var (s, b) in _speedButtons) b.On = s == speed;

            foreach (var (item, e) in _items)
            {
                bool visible = item.Visible == null || item.Visible();
                var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (e.style.display != display) e.style.display = display;
                if (!visible) continue;
                string text = item.Text != null ? item.Text() : "";
                if (e is KitButton button)
                {
                    if (button.text != text) button.text = text;
                    button.On = item.On != null && item.On();
                }
                else if (e is Label label)
                {
                    SetText(label, text);
                    var tint = item.Tint != null ? item.Tint() : null;
                    label.style.color = tint.HasValue ? new StyleColor(tint.Value) : new StyleColor(StyleKeyword.Null);
                }
                string tip = item.Tooltip != null ? item.Tooltip() : null;
                if (e.tooltip != tip) e.tooltip = tip;
            }
        }

        private static void SetText(Label label, string text)
        {
            if (label.text != text) label.text = text;
        }

        private static void SetOptional(Label label, string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            var display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (label.style.display != display) label.style.display = display;
            if (show) SetText(label, text);
        }
    }
}
