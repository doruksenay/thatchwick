using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// HUD v2 (Project belgesi 66 §3, kararlar S1-S6): dört köşe, ortası boş.
    /// - Sol üst: zincirli meşe saat levhası (ortada mevsim), tarih şeridi, hava/kış satırı, hız düğmeleri.
    /// - Sağ üst: akçe ve elmas paneli, tek satır görev izleyici, altında kayan bildirim kartları (en fazla 3).
    /// - Sol alt: hane portreleri; portreye dokunmak karakter kartını açar/kapatır (S1).
    /// - Sağ alt: Yapı (mod) ve Defter (pencere) düğmeleri; Defter'de okunmamış varsa rozet (S2).
    /// Öğeler bir kez kurulur; <see cref="Refresh"/> saniyede ~10 kez yalnızca değişeni yazar. Oyun durumunu
    /// <see cref="Hooks"/> üzerinden okur (GameRoot'un iç alanlarını görmez).
    /// </summary>
    public sealed class HudV2
    {
        public sealed class Hooks
        {
            public Func<string> Date, Status, Weather, SeasonKey, Quest, Message;
            public Func<bool> WeatherDanger;
            public IReadOnlyList<int> Speeds;
            public Func<int> Speed;
            public Action<int> SetSpeed;
            public Func<int> Akce, Diamonds;
            public Func<Color?> AkceTint;
            public Func<string> AkceTip, DiamondTip;
            public Action QuestClick;
            public Func<IReadOnlyList<Character>> Household;
            public Func<Character, Sprite> Portrait;
            public Func<Character> Selected;
            public Func<bool> CardOpen;
            public Action<Character> PortraitTap;
            public Action Build, Book;
            public Func<bool> BuildOn, BookOn, Badge;
            public Func<string> BuildTip, BookTip;
        }

        private const float RefreshSeconds = 0.1f, Margin = 6f;
        private const int ButtonFactor = 2;          // ilmek düğmeleri 32 px; 2× = 64 birim (dokunma hedefi)
        private const float PortraitSize = 44f, MaxPortraits = 8;
        private static readonly Color Cream = new Color32(0xF3, 0xEA, 0xD6, 0xFF);
        private static readonly Color OakFallback = new Color32(0x4A, 0x33, 0x24, 0xF0);
        private static readonly Color WarnInk = new Color32(0x9A, 0x4A, 0x1C, 0xFF);
        private static readonly Color DangerInk = new Color32(0x8E, 0x24, 0x24, 0xFF);

        private readonly Hooks _h;
        private readonly VisualElement _root, _topLeft, _topRight, _bottomLeft, _bottomRight, _toasts, _portraitRow;
        private readonly VisualElement _season, _questStrip, _messageStrip, _badge, _buildBtn, _bookBtn;
        private readonly Label _seasonText, _date, _status, _weather, _akce, _diamonds, _quest, _message;
        private readonly List<(int speed, VisualElement button, Label label)> _speedButtons = new List<(int, VisualElement, Label)>();
        private readonly List<(VisualElement root, VisualElement face)> _portraitPool = new List<(VisualElement, VisualElement)>();
        private readonly List<(VisualElement root, Label label)> _toastPool = new List<(VisualElement, Label)>();
        private readonly ToastQueue _queue = new ToastQueue();
        private float _nextRefresh;
        private string _portraitSignature = "";
        private readonly List<Character> _shownPortraits = new List<Character>();

        public HudV2(UiHost host, Hooks hooks)
        {
            _h = hooks;
            _root = new VisualElement { pickingMode = PickingMode.Ignore };
            _root.style.position = Position.Absolute;
            _root.style.left = 0; _root.style.right = 0; _root.style.top = 0; _root.style.bottom = 0;

            // ---- sol üst: saat levhası + tarih, hava, hız ----
            _topLeft = Corner(left: true, top: true);
            _topLeft.style.flexDirection = FlexDirection.Row;
            _topLeft.style.alignItems = Align.FlexStart;
            var clock = new VisualElement { pickingMode = PickingMode.Position };
            HudArt.Fixed(clock, HudArt.Clock, 1, 77f, 73f, OakFallback);
            clock.style.alignItems = Align.Center;
            clock.style.justifyContent = Justify.Center;
            clock.style.flexShrink = 0;
            _season = new VisualElement { pickingMode = PickingMode.Ignore };
            _season.style.width = 16; _season.style.height = 16;
            _season.style.marginTop = 8;   // levhanın ortasındaki koyu yuva zincirlerin altında
            _season.style.alignItems = Align.Center;
            _season.style.justifyContent = Justify.Center;
            _seasonText = Small("", 10f);
            _seasonText.style.color = Cream;
            _season.Add(_seasonText);
            clock.Add(_season);
            _topLeft.Add(clock);

            var info = new VisualElement { pickingMode = PickingMode.Ignore };
            info.style.marginLeft = 4;
            info.style.marginTop = 10;
            var dateStrip = Strip(out _date, 210f);
            info.Add(dateStrip);
            _status = Small("", 10f);
            _status.style.marginLeft = 8;
            info.Add(_status);
            _weather = Small("", 10f);
            _weather.style.marginLeft = 8;
            _weather.style.whiteSpace = WhiteSpace.NoWrap;
            _weather.style.color = Cream;
            info.Add(_weather);
            var speeds = new VisualElement { pickingMode = PickingMode.Ignore };
            speeds.style.flexDirection = FlexDirection.Row;
            speeds.style.marginTop = 2;
            foreach (int s in hooks.Speeds) speeds.Add(SpeedButton(s));
            info.Add(speeds);
            _topLeft.Add(info);
            _root.Add(_topLeft);

            // ---- sağ üst: para, görev, bildirim kartları ----
            _topRight = Corner(left: false, top: true);
            _topRight.style.alignItems = Align.FlexEnd;
            var money = new VisualElement { pickingMode = PickingMode.Position };
            HudArt.Fixed(money, HudArt.Money, 1, 65f, 45f, OakFallback);
            // 774: panelde iki yuva (üstte akçe, altta elmas); sayılar yuvaların içine, ikonların sağına.
            _akce = Small("", 11f);
            Place(_akce, 18f, 5f, 42f);
            _akce.style.color = Cream;
            money.Add(_akce);
            _diamonds = Small("", 11f);
            Place(_diamonds, 18f, 22f, 42f);
            _diamonds.style.color = Cream;
            money.Add(_diamonds);
            _topRight.Add(money);

            _questStrip = Strip(out _quest, 230f);
            _questStrip.style.marginTop = 3;
            _questStrip.pickingMode = PickingMode.Position;
            _questStrip.RegisterCallback<PointerDownEvent>(e => { _h.QuestClick?.Invoke(); e.StopPropagation(); });
            _topRight.Add(_questStrip);
            _messageStrip = Strip(out _message, 230f);
            _messageStrip.style.marginTop = 3;
            _topRight.Add(_messageStrip);
            _toasts = new VisualElement { pickingMode = PickingMode.Ignore };
            _toasts.style.alignItems = Align.FlexEnd;
            _topRight.Add(_toasts);
            _root.Add(_topRight);

            // ---- sol alt: hane portreleri ----
            _bottomLeft = Corner(left: true, top: false);
            _portraitRow = new VisualElement { pickingMode = PickingMode.Ignore };
            _portraitRow.style.flexDirection = FlexDirection.Row;
            _bottomLeft.Add(_portraitRow);
            _root.Add(_bottomLeft);

            // ---- sağ alt: Yapı + Defter ----
            _bottomRight = Corner(left: false, top: false);
            _bottomRight.style.flexDirection = FlexDirection.Row;
            _bottomRight.style.alignItems = Align.FlexEnd;
            _buildBtn = ImageButton(HudArt.BtnBuild, HudArt.BtnBuildDown, () => _h.Build?.Invoke(), "B");
            _bottomRight.Add(_buildBtn);
            _bookBtn = ImageButton(HudArt.BtnBook, HudArt.BtnBookDown, () => _h.Book?.Invoke(), "D");
            _bookBtn.style.marginLeft = 6;
            _badge = new VisualElement { pickingMode = PickingMode.Ignore };
            _badge.style.position = Position.Absolute;
            _badge.style.right = -4; _badge.style.top = -6;
            HudArt.Fixed(_badge, HudArt.Badge, ButtonFactor, 20f, 20f, DangerInk);
            _bookBtn.Add(_badge);
            _bottomRight.Add(_bookBtn);
            _root.Add(_bottomRight);

            host.Root.Add(_root);
            Refresh(true);
        }

        public bool Visible
        {
            get => _root.style.display != DisplayStyle.None;
            set { var d = value ? DisplayStyle.Flex : DisplayStyle.None; if (_root.style.display != d) _root.style.display = d; }
        }

        /// <summary>Yapı modunda ve tam ekran pencerelerde köşeler gizlenir; Yapı düğmesi kalır (moddan çıkış).</summary>
        public void SetCompact(bool compact)
        {
            var d = compact ? DisplayStyle.None : DisplayStyle.Flex;
            if (_topLeft.style.display != d) _topLeft.style.display = d;
            if (_topRight.style.display != d) _topRight.style.display = d;
            if (_bottomLeft.style.display != d) _bottomLeft.style.display = d;
            if (_bookBtn.style.display != d) _bookBtn.style.display = d;
        }

        /// <summary>Sol üst köşenin alt kenarı (arayüz birimi): IMGUI ayrıntı alanı bunun altından başlar.</summary>
        public float LeftBottom
        {
            get
            {
                float y = _topLeft.worldBound.yMax;
                return float.IsNaN(y) || y <= 0f ? Margin + 80f : y;
            }
        }

        /// <summary>Yeni bildirim (NotificationLog.Added): değerliyse kayan kart olur.</summary>
        public void Notify(NoticeEntry e)
        {
            if (!ToastQueue.Worth(e)) return;
            _queue.Push(e.Text, e.Level, Time.unscaledTime);
            DrawToasts();
        }

        public void Refresh(bool force = false)
        {
            float now = Time.unscaledTime;
            if (_queue.Tick(now)) DrawToasts();
            if (!force && now < _nextRefresh) return;
            _nextRefresh = now + RefreshSeconds;

            SetText(_date, _h.Date?.Invoke());
            SetOptional(_status, _h.Status?.Invoke());
            SetOptional(_weather, _h.Weather?.Invoke());
            bool danger = _h.WeatherDanger != null && _h.WeatherDanger();
            _weather.style.color = danger ? new StyleColor(new Color(1f, 0.62f, 0.55f)) : new StyleColor(Cream);
            string season = _h.SeasonKey?.Invoke() ?? "";
            var seasonIcon = HudArt.Icon("season_" + season);
            if (seasonIcon != null)
            {
                _season.style.backgroundImage = new StyleBackground(seasonIcon);
                SetText(_seasonText, "");
            }
            else
            {
                _season.style.backgroundImage = StyleKeyword.None;
                SetText(_seasonText, season.Length > 0 ? season.Substring(0, 1).ToUpperInvariant() : "");
            }

            int speed = _h.Speed != null ? _h.Speed() : 1;
            foreach (var (s, b, _) in _speedButtons) b.style.opacity = s == speed ? 1f : 0.55f;

            SetText(_akce, (_h.Akce != null ? _h.Akce() : 0).ToString());
            var tint = _h.AkceTint?.Invoke();
            _akce.style.color = tint.HasValue ? new StyleColor(tint.Value) : new StyleColor(Cream);
            SetText(_diamonds, (_h.Diamonds != null ? _h.Diamonds() : 0).ToString());
            string akceTip = _h.AkceTip?.Invoke();
            if (_akce.parent != null && _akce.parent.tooltip != akceTip) _akce.parent.tooltip = akceTip;

            string quest = _h.Quest?.Invoke();
            Show(_questStrip, !string.IsNullOrEmpty(quest));
            SetText(_quest, quest ?? "");
            string message = _h.Message?.Invoke();
            Show(_messageStrip, !string.IsNullOrEmpty(message));
            SetText(_message, message ?? "");

            RefreshPortraits();

            bool buildOn = _h.BuildOn != null && _h.BuildOn();
            _buildBtn.style.opacity = 1f;
            SetPressed(_buildBtn, buildOn);
            SetPressed(_bookBtn, _h.BookOn != null && _h.BookOn());
            Show(_badge, _h.Badge != null && _h.Badge());
            string bt = _h.BuildTip?.Invoke(), kt = _h.BookTip?.Invoke();
            if (_buildBtn.tooltip != bt) _buildBtn.tooltip = bt;
            if (_bookBtn.tooltip != kt) _bookBtn.tooltip = kt;
        }

        // ---------------- portreler ----------------

        private void RefreshPortraits()
        {
            var list = _h.Household != null ? _h.Household() : null;
            var selected = _h.Selected?.Invoke();
            bool cardOpen = _h.CardOpen != null && _h.CardOpen();
            var sig = new System.Text.StringBuilder();
            if (list != null) foreach (var c in list) sig.Append(c.Id).Append(',');
            string signature = sig.ToString();
            if (signature != _portraitSignature)
            {
                _portraitSignature = signature;
                _portraitRow.Clear();
                _shownPortraits.Clear();
                if (list != null)
                {
                    int i = 0;
                    foreach (var c in list)
                    {
                        if (i >= MaxPortraits) break;
                        var (root, face) = PortraitCell(i);
                        var sprite = _h.Portrait?.Invoke(c);
                        face.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : (StyleBackground)StyleKeyword.None;
                        root.tooltip = c.Name;
                        _portraitRow.Add(root);
                        _shownPortraits.Add(c);
                        i++;
                    }
                }
            }
            for (int i = 0; i < _shownPortraits.Count; i++)
            {
                bool sel = ReferenceEquals(_shownPortraits[i], selected);
                var (root, _) = _portraitPool[i];
                root.style.opacity = sel ? 1f : 0.72f;
                root.style.marginBottom = sel && cardOpen ? 6f : 0f;   // açık kartın sahibi biraz yukarıda
            }
        }

        private (VisualElement root, VisualElement face) PortraitCell(int index)
        {
            while (_portraitPool.Count <= index)
            {
                int captured = _portraitPool.Count;
                var root = new VisualElement { pickingMode = PickingMode.Position };
                root.style.width = PortraitSize; root.style.height = PortraitSize;
                root.style.marginRight = 3;
                HudArt.Apply(root, HudArt.Frame, HudArt.FrameSlice, OakFallback);
                HudArt.Pad(root, 6, 6, 6, 6);
                var inner = new VisualElement { pickingMode = PickingMode.Ignore };
                inner.style.flexGrow = 1;
                inner.style.backgroundColor = Cream;
                var face = new VisualElement { pickingMode = PickingMode.Ignore };
                face.style.flexGrow = 1;
                inner.Add(face);
                root.Add(inner);
                root.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (captured < _shownPortraits.Count) _h.PortraitTap?.Invoke(_shownPortraits[captured]);
                    e.StopPropagation();
                    Refresh(true);
                });
                _portraitPool.Add((root, face));
            }
            return _portraitPool[index];
        }

        // ---------------- bildirim kartları ----------------

        private void DrawToasts()
        {
            var items = _queue.Items;
            while (_toastPool.Count < ToastQueue.MaxVisible)
            {
                var strip = Strip(out var label, 230f);
                strip.style.marginTop = 3;
                _toastPool.Add((strip, label));
            }
            _toasts.Clear();
            // En yenisi üstte.
            for (int i = items.Count - 1, slot = 0; i >= 0; i--, slot++)
            {
                var t = items[i];
                var (root, label) = _toastPool[slot];
                SetText(label, t.Text);
                label.style.color = t.Level == NoticeLevel.Danger ? new StyleColor(DangerInk)
                    : t.Level == NoticeLevel.Warning ? new StyleColor(WarnInk) : new StyleColor(UiSkin.Ink);
                root.style.opacity = t.Alpha;
                _toasts.Add(root);
            }
        }

        // ---------------- kurulum yardımcıları ----------------

        private static VisualElement Corner(bool left, bool top)
        {
            var c = new VisualElement { pickingMode = PickingMode.Ignore };
            c.style.position = Position.Absolute;
            if (left) c.style.left = Margin; else c.style.right = Margin;
            if (top) c.style.top = Margin; else c.style.bottom = Margin;
            return c;
        }

        /// <summary>Meşe şerit (774 tracker, yatay dokuz dilim): ortası krem, içinde tek satır yazı.</summary>
        private static VisualElement Strip(out Label label, float width)
        {
            var s = new VisualElement { pickingMode = PickingMode.Ignore };
            s.style.width = width;
            s.style.height = 25;
            s.style.flexDirection = FlexDirection.Row;
            s.style.alignItems = Align.Center;
            HudArt.Apply(s, HudArt.Tracker, HudArt.TrackerSlice, Cream);
            HudArt.Pad(s, 20, 20, 0, 1);
            label = Small("", 10f);
            label.style.flexGrow = 1;
            label.style.flexShrink = 1;
            label.style.overflow = Overflow.Hidden;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.color = UiSkin.Ink;
            s.Add(label);
            return s;
        }

        private VisualElement SpeedButton(int speed)
        {
            var b = new VisualElement { pickingMode = PickingMode.Position };
            b.style.width = 30; b.style.height = 30;
            b.style.marginRight = 2;
            b.style.alignItems = Align.Center;
            b.style.justifyContent = Justify.Center;
            HudArt.Apply(b, HudArt.Frame, HudArt.FrameSlice, OakFallback);
            var icon = HudArt.Icon("speed_" + speed);
            Label label = null;
            if (icon != null)
            {
                var img = new VisualElement { pickingMode = PickingMode.Ignore };
                img.style.width = 16; img.style.height = 16;
                img.style.backgroundImage = new StyleBackground(icon);
                b.Add(img);
            }
            else
            {
                label = Small(new string('>', Mathf.Clamp(speed, 1, 3)), 10f);
                label.style.color = UiSkin.Ink;
                b.Add(label);
            }
            int captured = speed;
            b.RegisterCallback<PointerDownEvent>(e => { _h.SetSpeed?.Invoke(captured); e.StopPropagation(); Refresh(true); });
            _speedButtons.Add((speed, b, label));
            return b;
        }

        private static VisualElement ImageButton(string normal, string down, Action click, string fallback)
        {
            var b = new VisualElement { pickingMode = PickingMode.Position };
            HudArt.Fixed(b, normal, ButtonFactor, 64f, 64f, OakFallback);
            b.style.flexShrink = 0;
            b.style.alignItems = Align.Center;
            b.style.justifyContent = Justify.Center;
            if (HudArt.Tex(normal) == null)
            {
                var l = Small(fallback, 14f);
                l.style.color = Cream;
                b.Add(l);
            }
            b.userData = new[] { normal, down };
            b.RegisterCallback<PointerEnterEvent>(e => b.style.unityBackgroundImageTintColor = new Color(1.08f, 1.08f, 1.08f, 1f));
            b.RegisterCallback<PointerLeaveEvent>(e => b.style.unityBackgroundImageTintColor = Color.white);
            b.RegisterCallback<PointerDownEvent>(e => { click(); e.StopPropagation(); });
            return b;
        }

        /// <summary>Basılı (açık) hâl: ilmek düğmesinin _down dokusu.</summary>
        private static void SetPressed(VisualElement b, bool pressed)
        {
            if (!(b.userData is string[] names) || names.Length < 2) return;
            var tex = HudArt.Tex(pressed ? names[1] : names[0]);
            if (tex != null) b.style.backgroundImage = new StyleBackground(tex);
        }

        private static Label Small(string text, float size)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.style.fontSize = size;
            l.style.marginLeft = 0; l.style.marginRight = 0; l.style.marginTop = 0; l.style.marginBottom = 0;
            l.style.paddingLeft = 0; l.style.paddingRight = 0; l.style.paddingTop = 0; l.style.paddingBottom = 0;
            return l;
        }

        private static void Place(VisualElement e, float left, float top, float width)
        {
            e.style.position = Position.Absolute;
            e.style.left = left; e.style.top = top; e.style.width = width;
            e.style.unityTextAlign = TextAnchor.MiddleRight;
        }

        private static void Show(VisualElement e, bool show)
        {
            var d = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (e.style.display != d) e.style.display = d;
        }

        private static void SetText(Label label, string text)
        {
            text = text ?? "";
            if (label.text != text) label.text = text;
        }

        private static void SetOptional(Label label, string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            Show(label, show);
            if (show) SetText(label, text);
        }
    }
}
