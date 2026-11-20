using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Başarım listesi (UI Toolkit; yayın altyapısı 20.11.2026). Ortada duran, sayfalı bir pencere: her satırda ad,
    /// açıklama ve ilerleme. Öğeler açılırken ve sayfa değişince kurulur; açıkken her karede iş yapmaz.
    /// Oyun durumunu görmez: satırları GameRoot'un verdiği işlevden okur.
    /// </summary>
    public sealed class AchievementsPanel
    {
        /// <summary>Listenin bir satırı (GameRoot hazırlar; metinler yerelleştirilmiş gelir).</summary>
        public struct Row
        {
            public string Name;
            public string Description;
            /// <summary>"37 / 100" ya da açılış tarihi.</summary>
            public string Progress;
            public bool Unlocked;
        }

        public const int PageSize = 7;
        private const float Width = 520f;

        private readonly VisualElement _overlay, _list;
        private readonly Label _title, _page;
        private readonly KitButton _prev, _next;
        private readonly Func<IReadOnlyList<Row>> _rows;
        private readonly Func<string> _titleText;
        private int _pageIndex;

        public AchievementsPanel(UiHost host, Func<IReadOnlyList<Row>> rows, Func<string> titleText, Func<string> closeText)
        {
            _rows = rows;
            _titleText = titleText;

            // Bütün ekranı kaplayan, dokunmayı yutan yarı saydam zemin: arkadaki dünyaya tıklanmasın.
            _overlay = new VisualElement();
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0; _overlay.style.top = 0; _overlay.style.right = 0; _overlay.style.bottom = 0;
            _overlay.style.alignItems = Align.Center;
            _overlay.style.justifyContent = Justify.Center;
            _overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            _overlay.style.display = DisplayStyle.None;

            var panel = UiHost.Panel();
            panel.style.width = Width;

            var head = UiHost.Row();
            head.style.flexWrap = Wrap.NoWrap;
            _title = UiHost.Text();
            _title.style.fontSize = 15;
            _title.style.flexGrow = 1f;
            head.Add(_title);
            head.Add(new KitButton(closeText(), () => Visible = false));
            panel.Add(head);

            _list = new VisualElement();
            panel.Add(_list);

            var foot = UiHost.Row();
            foot.style.flexWrap = Wrap.NoWrap;
            foot.style.justifyContent = Justify.Center;
            _prev = new KitButton("<", () => { _pageIndex--; Rebuild(); });
            _prev.style.width = 34;
            _page = UiHost.Text();
            _page.style.unityTextAlign = TextAnchor.MiddleCenter;
            _page.style.width = 70;
            _next = new KitButton(">", () => { _pageIndex++; Rebuild(); });
            _next.style.width = 34;
            foot.Add(_prev); foot.Add(_page); foot.Add(_next);
            panel.Add(foot);

            _overlay.Add(panel);
            host.Root.Add(_overlay);
        }

        public bool Visible
        {
            get => _overlay.style.display != DisplayStyle.None;
            set
            {
                if (value == Visible) return;
                _overlay.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                if (value) { _pageIndex = 0; Rebuild(); }
            }
        }

        /// <summary>Açıkken bir başarım açıldıysa ya da dil değiştiyse listeyi yeniden kurar.</summary>
        public void Refresh()
        {
            if (Visible) Rebuild();
        }

        private void Rebuild()
        {
            var rows = _rows() ?? Array.Empty<Row>();
            int pages = Math.Max(1, (rows.Count + PageSize - 1) / PageSize);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pages - 1);
            _title.text = _titleText();
            _page.text = (_pageIndex + 1) + " / " + pages;
            _prev.style.display = pages > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            _next.style.display = pages > 1 ? DisplayStyle.Flex : DisplayStyle.None;

            _list.Clear();
            int end = Math.Min(rows.Count, (_pageIndex + 1) * PageSize);
            for (int i = _pageIndex * PageSize; i < end; i++) _list.Add(BuildRow(rows[i]));
        }

        private static VisualElement BuildRow(Row row)
        {
            var box = new VisualElement();
            box.style.marginTop = 2; box.style.marginBottom = 2;
            box.style.paddingLeft = 6; box.style.paddingRight = 6; box.style.paddingTop = 3; box.style.paddingBottom = 3;
            box.style.backgroundColor = row.Unlocked ? new Color(0.36f, 0.24f, 0.2f, 0.92f) : new Color(0f, 0f, 0f, 0.10f);

            var top = UiHost.Row();
            top.style.flexWrap = Wrap.NoWrap;
            var name = UiHost.Text(row.Name);
            name.style.fontSize = 13;
            name.style.flexGrow = 1f;
            name.style.color = row.Unlocked ? UiSkin.Gold : UiSkin.Ink;
            top.Add(name);
            var progress = UiHost.Text(row.Progress);
            progress.style.fontSize = 11;
            progress.style.color = row.Unlocked ? UiSkin.Linen : UiSkin.Ink;
            top.Add(progress);
            box.Add(top);

            var desc = UiHost.Text(row.Description);
            desc.style.fontSize = 11;
            desc.style.color = row.Unlocked ? UiSkin.Linen : UiSkin.Ink;
            box.Add(desc);
            return box;
        }
    }
}
