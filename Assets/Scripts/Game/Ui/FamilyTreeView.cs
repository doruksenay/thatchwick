using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Çizgili soy ağacı (UI Toolkit; Project belgesi 67e): odaktaki kişinin dedeleri ve nineleri, anne-babası, kardeşleri,
    /// eşi, çocukları ve torunları. Eşler yatay çizgiyle, çocuklar anne-babanın ortasından inen dikey çizgi ve kardeş
    /// çizgisiyle bağlanır. Bir kişiye dokununca ağaç onun çevresinde yeniden kurulur. Çizgiler düz kutularla çizilir
    /// (yazı tipinde kutu çizgisi karakteri yok).
    /// </summary>
    public static class FamilyTreeView
    {
        private const float NodeW = 76f, NodeH = 34f, MiniW = 58f, MiniH = 26f, Line = 2f, Stub = 10f, MarryBar = 10f;
        private const int MaxSiblings = 5, MaxChildren = 5, MaxGrandchildren = 4;
        private static readonly Color LineColor = new Color(0.36f, 0.24f, 0.2f, 0.9f);

        public sealed class Options
        {
            public Character Focus;
            public IReadOnlyList<Character> People;
            /// <summary>Düğümün ikinci satırı (bakanın gözünden ilişki aşaması gibi).</summary>
            public Func<Character, string> Sub;
            public Action<Character> Pick;
            public Func<int, string> More;
        }

        public static VisualElement Build(Options o)
        {
            var root = new VisualElement();
            root.style.alignItems = Align.Center;
            root.style.paddingTop = 4; root.style.paddingBottom = 6;
            if (o?.Focus == null) return root;

            var father = Find(o, o.Focus.BirthFatherId);
            var mother = Find(o, o.Focus.BirthMotherId);
            if (father != null || mother != null)
            {
                var siblings = ChildrenOf(o, father, mother);
                Trim(siblings, o.Focus, MaxSiblings, out int hidden);
                bool gp = HasParents(o, father) || HasParents(o, mother);
                root.Add(Unit(o, father, mother, siblings, hidden, gp, c => c == o.Focus ? 2 : 0));
            }
            else
            {
                var spouse = SpouseOf(o, o.Focus);
                var kids = ChildrenOf(o, o.Focus, spouse);
                Trim(kids, null, MaxChildren, out int hidden);
                root.Add(Unit(o, o.Focus, spouse, kids, hidden, false, _ => 1));
            }
            return root;
        }

        /// <summary>
        /// Bir aile birimi: çift (eşler yatay çizgiyle), ortadan inen çizgi, kardeş çizgisi ve çocuklar. depth: çocuğun
        /// kendi alt ağacı kaç kuşak açılır (0: yalnız kendisi ve eşi).
        /// </summary>
        private static VisualElement Unit(Options o, Character a, Character b, List<Character> kids, int hidden, bool grandparents, Func<Character, int> depth)
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;

            var couple = UiHost.Row();
            couple.style.flexWrap = Wrap.NoWrap;
            couple.style.alignItems = Align.FlexEnd;
            if (a == null) { a = b; b = null; }
            couple.Add(PersonColumn(o, a, grandparents));
            if (b != null)
            {
                var bar = Bar(MarryBar, Line);
                bar.style.marginBottom = NodeH / 2f - Line / 2f;
                couple.Add(bar);
                couple.Add(PersonColumn(o, b, grandparents));
            }
            col.Add(couple);

            int total = kids.Count + (hidden > 0 ? 1 : 0);
            if (total == 0) return col;
            col.Add(Bar(Line, Stub));
            var row = UiHost.Row();
            row.style.flexWrap = Wrap.NoWrap;
            row.style.alignItems = Align.FlexStart;
            for (int i = 0; i < total; i++)
            {
                var cell = new VisualElement();
                cell.style.alignItems = Align.Center;
                cell.style.paddingLeft = 3; cell.style.paddingRight = 3;
                // Kardeş çizgisi: hücrenin üstünde iki yarım çizgi; uçtakilerde dış yarı yok.
                if (total > 1)
                {
                    if (i > 0) cell.Add(HalfLine(left: true));
                    if (i < total - 1) cell.Add(HalfLine(left: false));
                }
                cell.Add(Bar(Line, Stub));
                if (i < kids.Count)
                {
                    var k = kids[i];
                    int d = depth(k);
                    var spouse = SpouseOf(o, k);
                    if (d > 0)
                    {
                        var grand = ChildrenOf(o, k, spouse);
                        Trim(grand, null, d > 1 ? MaxChildren : MaxGrandchildren, out int gh);
                        cell.Add(Unit(o, k, spouse, grand, gh, false, _ => d - 1));
                    }
                    else cell.Add(Couple(o, k, spouse));
                }
                else cell.Add(Node(o?.More?.Invoke(hidden) ?? "+" + hidden, null, false, NodeW, NodeH, null));
                row.Add(cell);
            }
            col.Add(row);
            return col;
        }

        /// <summary>Kişi düğümü; anne-baba kuşağında üstünde kendi anne-babası (dede ve nine) küçük düğümlerle durur.</summary>
        private static VisualElement PersonColumn(Options o, Character c, bool grandparents)
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;
            if (grandparents)
            {
                col.style.width = MiniW * 2 + MarryBar + 4;
                var gf = Find(o, c.BirthFatherId); var gm = Find(o, c.BirthMotherId);
                if (gf != null || gm != null)
                {
                    var pair = UiHost.Row();
                    pair.style.flexWrap = Wrap.NoWrap;
                    pair.style.alignItems = Align.Center;
                    if (gf != null) pair.Add(Person(o, gf, MiniW, MiniH));
                    if (gf != null && gm != null) pair.Add(Bar(MarryBar, Line));
                    if (gm != null) pair.Add(Person(o, gm, MiniW, MiniH));
                    col.Add(pair);
                    col.Add(Bar(Line, Stub));
                }
                else
                {
                    var spacer = new VisualElement();
                    spacer.style.height = MiniH + Stub;
                    col.Add(spacer);
                }
            }
            col.Add(Person(o, c, NodeW, NodeH));
            return col;
        }

        private static VisualElement Couple(Options o, Character a, Character b)
        {
            var row = UiHost.Row();
            row.style.flexWrap = Wrap.NoWrap;
            row.style.alignItems = Align.Center;
            row.Add(Person(o, a, NodeW, NodeH));
            if (b != null) { row.Add(Bar(MarryBar, Line)); row.Add(Person(o, b, NodeW, NodeH)); }
            return row;
        }

        private static VisualElement Person(Options o, Character c, float w, float h)
        {
            string name = c.Name + (c.IsDead ? " †" : "");
            string sub = c == o.Focus ? null : o.Sub?.Invoke(c);
            return Node(name, sub, c == o.Focus, w, h, o.Pick == null ? null : (Action)(() => o.Pick(c)));
        }

        private static VisualElement Node(string name, string sub, bool focus, float w, float h, Action click)
        {
            var b = new KitButton(string.IsNullOrEmpty(sub) ? name : name + "\n" + sub, click ?? (() => { }));
            b.On = focus;
            b.style.width = w;
            b.style.height = h;
            b.style.fontSize = h < NodeH ? 9 : 10;
            b.style.whiteSpace = WhiteSpace.Normal;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.paddingLeft = 1; b.style.paddingRight = 1; b.style.paddingTop = 0; b.style.paddingBottom = 0;
            b.style.marginLeft = 0; b.style.marginRight = 0; b.style.marginTop = 0; b.style.marginBottom = 0;
            return b;
        }

        private static VisualElement Bar(float w, float h)
        {
            var e = new VisualElement();
            e.style.width = w; e.style.height = h;
            e.style.flexShrink = 0f;
            e.style.backgroundColor = LineColor;
            return e;
        }

        private static VisualElement HalfLine(bool left)
        {
            var e = new VisualElement();
            e.style.position = Position.Absolute;
            e.style.top = 0;
            e.style.left = left ? Length.Percent(0) : Length.Percent(50);
            e.style.width = Length.Percent(50);
            e.style.height = Line;
            e.style.backgroundColor = LineColor;
            return e;
        }

        // ---- soy ilişkileri ----

        private static Character Find(Options o, int? id)
        {
            if (!id.HasValue) return null;
            foreach (var c in o.People) if (c.Id == id.Value) return c;
            return null;
        }

        private static bool HasParents(Options o, Character c) => c != null && (Find(o, c.BirthFatherId) != null || Find(o, c.BirthMotherId) != null);

        private static Character SpouseOf(Options o, Character c) => c != null && c.SpouseId.HasValue ? Find(o, c.SpouseId) : null;

        /// <summary>İki ebeveynden birinin çocuğu olanlar, büyükten küçüğe.</summary>
        private static List<Character> ChildrenOf(Options o, Character a, Character b)
        {
            var list = new List<Character>();
            foreach (var c in o.People)
            {
                bool of(Character p) => p != null && (c.BirthMotherId == p.Id || c.BirthFatherId == p.Id);
                if (of(a) || of(b)) list.Add(c);
            }
            list.Sort((x, y) => y.AgeDays.CompareTo(x.AgeDays));
            return list;
        }

        /// <summary>Kalabalıkta en çok max kişi gösterilir (odaktaki kişi hep kalır); kalanların sayısı döner.</summary>
        private static void Trim(List<Character> list, Character keep, int max, out int hidden)
        {
            hidden = 0;
            if (list.Count <= max) return;
            var kept = new List<Character>();
            if (keep != null && list.Contains(keep)) kept.Add(keep);
            foreach (var c in list) if (kept.Count < max && !kept.Contains(c)) kept.Add(c);
            kept.Sort((x, y) => list.IndexOf(x).CompareTo(list.IndexOf(y)));
            hidden = list.Count - kept.Count;
            list.Clear();
            list.AddRange(kept);
        }
    }
}
