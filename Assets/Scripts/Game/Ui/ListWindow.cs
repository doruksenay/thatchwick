using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Sekmeli, sayfalı liste penceresi (UI Toolkit; karar 20.11.2026: yeni pencere IMGUI ile yazılmaz). Ortada durur,
    /// arkayı karartır ve dokunmayı yutar. Yarış bahsi, gardırop ve terzi ekranı bunu kullanır.
    /// Oyun durumunu görmez: içeriği GameRoot'un verdiği işlev kurar (<see cref="Model"/>). Açılırken ve her tıklamadan
    /// sonra yeniden kurulur; açıkken her karede iş yapmaz. İçerik dışarıdan değiştiyse <see cref="Refresh"/> çağrılır.
    /// </summary>
    public sealed class ListWindow
    {
        /// <summary>Düğme: sekme, satır eylemi ya da alt satır düğmesi.</summary>
        public sealed class Act
        {
            public string Text;
            public Action Click;
            /// <summary>Seçili görünüm (açık sekme, seçili tutar).</summary>
            public bool On;
            /// <summary>Tıklanamaz: soluk yazı olarak görünür.</summary>
            public bool Disabled;

            public Act(string text, Action click, bool on = false, bool disabled = false)
            {
                Text = text; Click = click; On = on; Disabled = disabled;
            }
        }

        public sealed class Row
        {
            public string Title;
            /// <summary>Başlığın altındaki küçük satır (boş olabilir).</summary>
            public string Detail;
            /// <summary>Bölüm başlığı: kutusuz, iri yazı.</summary>
            public bool Header;
            /// <summary>Seçili ya da öne çıkan satır.</summary>
            public bool Marked;
            /// <summary>Doluysa bütün satır tek düğmedir (seçim listesi).</summary>
            public Action Click;
            public readonly List<Act> Actions = new List<Act>();

            public Row(string title, string detail = null) { Title = title; Detail = detail; }
        }

        public sealed class Model
        {
            public string Title;
            /// <summary>Sekmelerin altındaki açıklama (boş olabilir).</summary>
            public string Info;
            /// <summary>Son işlemin sonucu ya da hata (boş olabilir).</summary>
            public string Status;
            /// <summary>Alt satırın başındaki yazı ("Yatırılacak:").</summary>
            public string FooterLabel;
            /// <summary>Üst üste sekme satırları (sayfa, kişi, takım). Sekmeye basınca liste ilk sayfaya döner.</summary>
            public readonly List<List<Act>> TabRows = new List<List<Act>>();
            public readonly List<Row> Rows = new List<Row>();
            public readonly List<Act> Footer = new List<Act>();

            public List<Act> AddTabRow()
            {
                var row = new List<Act>();
                TabRows.Add(row);
                return row;
            }
        }

        private static readonly Color Dim = new Color(0.45f, 0.4f, 0.36f);

        private readonly VisualElement _overlay, _tabs, _list, _footer;
        private readonly Label _title, _info, _status, _page;
        private readonly KitButton _prev, _next;
        private readonly VisualElement _pager;
        private readonly Func<Model> _build;
        private readonly int _pageSize;
        private int _pageIndex;

        public ListWindow(UiHost host, Func<Model> build, Func<string> closeText, float width = 560f, int pageSize = 8)
        {
            _build = build;
            _pageSize = Math.Max(1, pageSize);

            _overlay = new VisualElement();
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0; _overlay.style.top = 0; _overlay.style.right = 0; _overlay.style.bottom = 0;
            _overlay.style.alignItems = Align.Center;
            _overlay.style.justifyContent = Justify.Center;
            _overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            _overlay.style.display = DisplayStyle.None;

            var panel = UiHost.Panel();
            panel.style.width = width;

            var head = UiHost.Row();
            head.style.flexWrap = Wrap.NoWrap;
            _title = UiHost.Text();
            _title.style.fontSize = 15;
            _title.style.flexGrow = 1f;
            head.Add(_title);
            head.Add(new KitButton(closeText(), () => Visible = false));
            panel.Add(head);

            _tabs = new VisualElement();
            panel.Add(_tabs);

            _info = UiHost.Text();
            _info.style.fontSize = 11;
            panel.Add(_info);

            _list = new VisualElement();
            panel.Add(_list);

            _pager = UiHost.Row();
            _pager.style.flexWrap = Wrap.NoWrap;
            _pager.style.justifyContent = Justify.Center;
            _prev = new KitButton("<", () => { _pageIndex--; Rebuild(); });
            _prev.style.width = 34;
            _page = UiHost.Text();
            _page.style.unityTextAlign = TextAnchor.MiddleCenter;
            _page.style.width = 70;
            _next = new KitButton(">", () => { _pageIndex++; Rebuild(); });
            _next.style.width = 34;
            _pager.Add(_prev); _pager.Add(_page); _pager.Add(_next);
            panel.Add(_pager);

            _footer = UiHost.Row();
            panel.Add(_footer);

            _status = UiHost.Text();
            _status.style.fontSize = 11;
            panel.Add(_status);

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

        /// <summary>Açıkken içerik dışarıdan değiştiyse (gün döndü, yarış koştu, dil değişti) yeniden kurar.</summary>
        public void Refresh()
        {
            if (Visible) Rebuild();
        }

        private static void Show(VisualElement e, bool on) => e.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;

        private void Rebuild()
        {
            var model = _build();
            if (model == null) { _overlay.style.display = DisplayStyle.None; return; }   // gösterilecek bir şey kalmadı

            _title.text = model.Title ?? "";
            _info.text = model.Info ?? "";
            Show(_info, !string.IsNullOrEmpty(model.Info));
            _status.text = model.Status ?? "";
            Show(_status, !string.IsNullOrEmpty(model.Status));

            _tabs.Clear();
            foreach (var tabRow in model.TabRows)
            {
                if (tabRow.Count == 0) continue;
                var line = UiHost.Row();
                foreach (var tab in tabRow) line.Add(Button(tab, resetPage: true));
                _tabs.Add(line);
            }

            int pages = Math.Max(1, (model.Rows.Count + _pageSize - 1) / _pageSize);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pages - 1);
            _page.text = (_pageIndex + 1) + " / " + pages;
            Show(_pager, pages > 1);

            _list.Clear();
            int end = Math.Min(model.Rows.Count, (_pageIndex + 1) * _pageSize);
            for (int i = _pageIndex * _pageSize; i < end; i++) _list.Add(BuildRow(model.Rows[i]));

            _footer.Clear();
            bool footer = model.Footer.Count > 0 || !string.IsNullOrEmpty(model.FooterLabel);
            Show(_footer, footer);
            if (!string.IsNullOrEmpty(model.FooterLabel)) _footer.Add(UiHost.Text(model.FooterLabel));
            foreach (var act in model.Footer) _footer.Add(Button(act, resetPage: false));
        }

        /// <summary>Eylemi çalıştırır, sonra pencereyi yeniden kurar (liste değişmiş olabilir).</summary>
        private void Run(Act act, bool resetPage)
        {
            act.Click?.Invoke();
            if (resetPage) _pageIndex = 0;
            if (Visible) Rebuild();
        }

        private VisualElement Button(Act act, bool resetPage)
        {
            if (act.Disabled || act.Click == null)
            {
                var off = UiHost.Text(act.Text);
                off.style.fontSize = 11;
                off.style.color = Dim;
                off.style.marginLeft = 6; off.style.marginRight = 6;
                return off;
            }
            var button = new KitButton(act.Text, () => Run(act, resetPage));
            button.On = act.On;
            return button;
        }

        private VisualElement BuildRow(Row row)
        {
            if (row.Header)
            {
                var header = UiHost.Text(row.Title);
                header.style.fontSize = 13;
                header.style.marginTop = 5;
                return header;
            }
            if (row.Click != null)
            {
                // Seçim satırı: bütün satır tek düğme; uzun yazı sığsın diye yükseklik serbest.
                var pick = new KitButton(string.IsNullOrEmpty(row.Detail) ? row.Title : row.Title + "\n" + row.Detail,
                    () => { row.Click(); if (Visible) Rebuild(); });
                pick.On = row.Marked;
                pick.style.height = StyleKeyword.Auto;
                pick.style.whiteSpace = WhiteSpace.Normal;
                pick.style.unityTextAlign = TextAnchor.MiddleLeft;
                pick.style.paddingTop = 4; pick.style.paddingBottom = 4;
                return pick;
            }

            var box = new VisualElement();
            box.style.marginTop = 2; box.style.marginBottom = 2;
            box.style.paddingLeft = 6; box.style.paddingRight = 6; box.style.paddingTop = 3; box.style.paddingBottom = 3;
            box.style.backgroundColor = row.Marked ? new Color(0.36f, 0.24f, 0.2f, 0.92f) : new Color(0f, 0f, 0f, 0.10f);

            var top = UiHost.Row();
            top.style.flexWrap = Wrap.NoWrap;
            var name = UiHost.Text(row.Title);
            name.style.fontSize = 12;
            name.style.flexGrow = 1f;
            name.style.color = row.Marked ? UiSkin.Gold : UiSkin.Ink;
            top.Add(name);
            foreach (var act in row.Actions) top.Add(Button(act, resetPage: false));
            box.Add(top);

            if (!string.IsNullOrEmpty(row.Detail))
            {
                var detail = UiHost.Text(row.Detail);
                detail.style.fontSize = 11;
                detail.style.color = row.Marked ? UiSkin.Linen : UiSkin.Ink;
                box.Add(detail);
            }
            return box;
        }
    }
}
