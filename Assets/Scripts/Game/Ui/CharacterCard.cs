using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Alttaki karakter kartı (Project belgesi 68; StatusHud.DrawBottom'un UI Toolkit sürümü). The Sims 4 ve Stardew'deki gibi
    /// oyuncu sayı görmez, temsilini görür:
    /// - ruh hali: yüz ikonu ve sözcük ("Mutlu"), basamağın renginde levha; toplam puan yok;
    /// - ihtiyaçlar: ikon ve renkli çubuk (yeşil / hardal / kırmızı, 15 altı yanıp söner); sayı ve ad yok;
    /// - iç sesler: çerçeveli ikon (keyif yeşil, kasvet bordo), köşede 1-3 güç işareti, bitmek üzereyken yanıp söner.
    /// Bir öğeye dokununca kartın üstünde ayrıntı açılır: gerçek değer, açıklama, kalan süre. Aynı öğeye ya da ayrıntıya
    /// yeniden dokununca kapanır. Öğeler bir kez kurulur; Refresh saniyede ~10 kez yalnızca değişeni yazar.
    /// </summary>
    public sealed class CharacterCard
    {
        private const float RefreshSeconds = 0.1f, Width = 470f, TileSize = 24f, IconSize = 16f, MaxTiles = 12;

        private static readonly Color JoyFrame = new Color32(0x6E, 0x8F, 0x4E, 0xFF);    // soluk yosun yeşili (66 §3.1 kural 11)
        private static readonly Color GloomFrame = new Color32(0x8E, 0x3B, 0x3B, 0xFF);  // bordo
        private static readonly Color StatusFrame = new Color32(0x8A, 0x7A, 0x66, 0xFF);
        private static readonly Color TileFill = new Color32(0xF3, 0xEA, 0xD6, 0xFF);
        private static readonly Color BarBack = new Color(0.18f, 0.15f, 0.13f, 0.55f);

        private readonly LifeSystems _life;
        private readonly ContentDatabase _content;
        private readonly Func<Character> _who;
        private readonly Func<Character, JobRunner> _runnerOf;

        private readonly VisualElement _root, _card, _detail, _tiles, _queue;
        private readonly VisualElement _moodPlaque, _moodFace;
        private readonly Label _moodWord, _name, _traits, _notice, _header, _detailTitle, _detailBody;
        private readonly VisualElement _detailIcon;
        private readonly List<NeedCell> _needs = new List<NeedCell>();
        private readonly List<Tile> _tilePool = new List<Tile>();
        private readonly Label _more;

        private float _nextRefresh;
        private string _tileSignature = "", _queueSignature = "";
        private Character _shown;
        // Açık ayrıntının kaynağı: "mood", "need:<n>", "voice:<id>". Her yenilemede içerik baştan yazılır (süre akar).
        private string _detailKey;

        private sealed class NeedCell { public NeedType Need; public VisualElement Root, Fill; }
        private sealed class Tile { public VisualElement Root, Icon, Pips; public ActiveMoodlet Moodlet; }

        public CharacterCard(UiHost host, LifeSystems life, ContentDatabase content, Func<Character> who, Func<Character, JobRunner> runnerOf)
        {
            _life = life; _content = content; _who = who; _runnerOf = runnerOf;

            // Kök: ekranın altında ortalanmış dikey yığın (ayrıntı kartın üstünde açılır). Boşluk dokunmayı dünyaya bırakır.
            _root = new VisualElement { pickingMode = PickingMode.Ignore };
            _root.style.position = Position.Absolute;
            _root.style.left = 0; _root.style.right = 0; _root.style.bottom = 6;
            _root.style.alignItems = Align.Center;

            _detail = UiHost.Panel();
            _detail.style.width = 300;
            _detail.style.marginBottom = 4;
            _detail.style.display = DisplayStyle.None;
            var detailHead = UiHost.Row();
            detailHead.style.flexWrap = Wrap.NoWrap;
            _detailIcon = Icon(IconSize);
            detailHead.Add(_detailIcon);
            _detailTitle = UiHost.Text();
            _detailTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            detailHead.Add(_detailTitle);
            _detail.Add(detailHead);
            _detailBody = UiHost.Text();
            _detailBody.style.fontSize = 11;
            _detail.Add(_detailBody);
            _detail.RegisterCallback<PointerDownEvent>(e => { CloseDetail(); e.StopPropagation(); });
            _root.Add(_detail);

            _card = UiHost.Panel();
            _card.style.width = Width;
            // HUD v2 (belge 66 §5 S6): sarmaşıklı meşe çerçeve (774 hud_window, dokuz dilim). Doku yoksa kit paneli kalır.
            if (HudArt.Tex(HudArt.Window) != null)
            {
                HudArt.Apply(_card, HudArt.Window, HudArt.WindowSlice, Color.clear);
                HudArt.Pad(_card, 28, 28, 24, 26);
                _card.style.width = Width + 44f;
            }

            // 1. satır: ruh hali levhası, ad, özellikler.
            var top = UiHost.Row();
            top.style.flexWrap = Wrap.NoWrap;
            _moodPlaque = UiHost.Row();
            _moodPlaque.style.flexWrap = Wrap.NoWrap;
            _moodPlaque.style.paddingLeft = 3; _moodPlaque.style.paddingRight = 5;
            _moodPlaque.style.height = 20;
            SetBorder(_moodPlaque, 1, StatusFrame);
            _moodFace = Icon(IconSize);
            _moodPlaque.Add(_moodFace);
            _moodWord = UiHost.Text();
            _moodWord.style.unityFontStyleAndWeight = FontStyle.Bold;
            _moodPlaque.Add(_moodWord);
            Tap(_moodPlaque, () => ToggleDetail("mood"));
            top.Add(_moodPlaque);
            _name = UiHost.Text();
            _name.style.marginLeft = 6;
            _name.style.unityFontStyleAndWeight = FontStyle.Bold;
            top.Add(_name);
            _traits = UiHost.Text();
            _traits.style.fontSize = 10;
            _traits.style.flexShrink = 1;
            _traits.style.overflow = Overflow.Hidden;
            _traits.style.whiteSpace = WhiteSpace.NoWrap;
            top.Add(_traits);
            _card.Add(top);

            _notice = UiHost.Text();
            _notice.style.fontSize = 10;
            _card.Add(_notice);

            // 2. satır: ihtiyaçlar (ikon + çubuk).
            var needRow = UiHost.Row();
            needRow.style.flexWrap = Wrap.NoWrap;
            needRow.style.marginTop = 3;
            foreach (NeedType n in Enum.GetValues(typeof(NeedType))) needRow.Add(BuildNeed(n));
            _card.Add(needRow);

            // 3. satır: İç Sesler.
            _header = UiHost.Text();
            _header.style.fontSize = 10;
            _header.style.marginTop = 3;
            _card.Add(_header);
            _tiles = UiHost.Row();
            _card.Add(_tiles);
            _more = UiHost.Text();
            _more.style.fontSize = 10;
            _more.style.unityTextAlign = TextAnchor.MiddleCenter;
            _more.style.width = TileSize;

            // 4. satır: iş sırası (Öne al / İptal).
            _queue = new VisualElement();
            _queue.style.marginTop = 2;
            _card.Add(_queue);

            _root.Add(_card);
            host.Root.Add(_root);
            Visible = false;
        }

        public bool Visible
        {
            get => _root.style.display != DisplayStyle.None;
            set
            {
                var d = value ? DisplayStyle.Flex : DisplayStyle.None;
                if (_root.style.display != d) _root.style.display = d;
                if (!value) CloseDetail();
            }
        }

        // ---------------- kurulum yardımcıları ----------------

        private static VisualElement Icon(float size)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.style.width = size; e.style.height = size;
            e.style.flexShrink = 0;
            e.style.unityBackgroundImageTintColor = Color.white;
            return e;
        }

        private static void SetBorder(VisualElement e, float w, Color c)
        {
            e.style.borderLeftWidth = w; e.style.borderRightWidth = w; e.style.borderTopWidth = w; e.style.borderBottomWidth = w;
            e.style.borderLeftColor = c; e.style.borderRightColor = c; e.style.borderTopColor = c; e.style.borderBottomColor = c;
        }

        private static void Tap(VisualElement e, Action onTap)
        {
            e.pickingMode = PickingMode.Position;
            e.RegisterCallback<PointerDownEvent>(ev => { onTap(); ev.StopPropagation(); });
        }

        /// <summary>İkonu sırayla dener: kütüphanedeki 32'lik ve 16'lık PixelLab ikonu, sonra Resources/UI/Icons. Hiçbiri yoksa boş.</summary>
        private static bool SetIcon(VisualElement e, IEnumerable<string> keys)
        {
            foreach (var k in keys)
            {
                if (string.IsNullOrEmpty(k)) continue;
                var sprite = IsoArt.Get("ui_icon_" + k + "_32", 16f, 16f) ?? IsoArt.Get("ui_icon_" + k, 8f, 8f);
                if (sprite != null) { e.style.backgroundImage = new StyleBackground(sprite); return true; }
                var tex = UiSkin.Icon(k);
                if (tex != null) { e.style.backgroundImage = new StyleBackground(tex); return true; }
            }
            e.style.backgroundImage = StyleKeyword.None;
            return false;
        }

        private VisualElement BuildNeed(NeedType n)
        {
            var cell = new NeedCell { Need = n };
            cell.Root = UiHost.Row();
            cell.Root.style.flexWrap = Wrap.NoWrap;
            cell.Root.style.width = (Width - 16f) / 7f;
            cell.Root.style.height = 20;
            var icon = Icon(IconSize);
            if (!SetIcon(icon, new[] { InnerVoiceView.NeedIconKey(n) }))
            {
                // İkon gelene dek ilk harf (Tuvalet ikonu henüz yok).
                var letter = UiHost.Text(NeedKinds.DisplayName(n).Substring(0, 1));
                letter.style.width = IconSize;
                letter.style.unityTextAlign = TextAnchor.MiddleCenter;
                cell.Root.Add(letter);
            }
            else cell.Root.Add(icon);
            var back = new VisualElement { pickingMode = PickingMode.Ignore };
            back.style.flexGrow = 1;
            back.style.height = 6;
            back.style.marginLeft = 2; back.style.marginRight = 4;
            back.style.backgroundColor = BarBack;
            cell.Fill = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.Fill.style.height = Length.Percent(100);
            back.Add(cell.Fill);
            cell.Root.Add(back);
            cell.Root.tooltip = NeedKinds.DisplayName(n);
            Tap(cell.Root, () => ToggleDetail("need:" + (int)n));
            _needs.Add(cell);
            return cell.Root;
        }

        private Tile NewTile()
        {
            var t = new Tile { Root = new VisualElement() };
            t.Root.style.width = TileSize; t.Root.style.height = TileSize;
            t.Root.style.marginRight = 2; t.Root.style.marginTop = 1;
            t.Root.style.alignItems = Align.Center;
            t.Root.style.justifyContent = Justify.Center;
            t.Root.style.backgroundColor = TileFill;
            t.Icon = Icon(IconSize);
            t.Root.Add(t.Icon);
            // Güç işareti: sağ altta 1-3 küçük kare (yazı tipine bağlı ok karakteri yerine; her dilde aynı).
            t.Pips = new VisualElement { pickingMode = PickingMode.Ignore };
            t.Pips.style.position = Position.Absolute;
            t.Pips.style.right = 1; t.Pips.style.bottom = 1;
            t.Pips.style.flexDirection = FlexDirection.Row;
            for (int i = 0; i < 3; i++)
            {
                var pip = new VisualElement { pickingMode = PickingMode.Ignore };
                pip.style.width = 3; pip.style.height = 3; pip.style.marginLeft = 1;
                t.Pips.Add(pip);
            }
            t.Root.Add(t.Pips);
            Tap(t.Root, () => { if (t.Moodlet?.Def != null) ToggleDetail("voice:" + t.Moodlet.Def.Id); });
            return t;
        }

        // ---------------- yenileme ----------------

        public void Refresh(bool show)
        {
            var c = show ? _who() : null;
            if (c == null || c.IsDead) { Visible = false; _shown = null; return; }
            Visible = true;
            if (!ReferenceEquals(c, _shown)) { _shown = c; _tileSignature = ""; _queueSignature = ""; CloseDetail(); }

            float now = Time.unscaledTime;
            bool blinkOn = Mathf.Repeat(now, 0.8f) < 0.5f;
            // Yanıp sönme her karede; metin ve dizilim saniyede ~10 kez.
            foreach (var cell in _needs)
            {
                float v = c.Needs.Get(cell.Need);
                bool critical = InnerVoiceView.Band(v) == NeedBand.Critical;
                cell.Fill.style.opacity = critical && !blinkOn ? 0.25f : 1f;
            }
            foreach (var t in _tilePool)
                if (t.Moodlet != null && t.Root.parent != null)
                    t.Root.style.opacity = InnerVoiceView.Expiring(t.Moodlet) && !blinkOn ? 0.4f : 1f;

            if (now < _nextRefresh) return;
            _nextRefresh = now + RefreshSeconds;

            // Ruh hali levhası: yüz + sözcük, sayı yok.
            var status = c.Mood.Status;
            SetText(_moodWord, GameLocalization.R(InnerVoiceView.StatusName(status)));
            SetIcon(_moodFace, new[] { InnerVoiceView.MoodIconKey(status) });
            var tint = StatusColor(status);
            SetBorder(_moodPlaque, 1, tint);
            _moodPlaque.style.backgroundColor = new Color(tint.r, tint.g, tint.b, 0.22f);
            SetText(_name, c.Name);
            SetText(_traits, TraitsText(c));

            string notice = "";
            if (c.InBreakdown) notice = GameLocalization.T("ui.mood.card.breakdown", ("name", MoodBehavior.BreakdownName(c.Breakdown)));
            else if (c.MoodEnabled && (c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen))
                notice = MoodBehavior.UpbringingSummary(c.UpbringingAverage, c.UpbringingDays);
            SetOptional(_notice, notice);

            foreach (var cell in _needs)
            {
                float v = c.Needs.Get(cell.Need);
                cell.Fill.style.width = Length.Percent(Mathf.Clamp(v, 0f, 100f));
                cell.Fill.style.backgroundColor = BandColor(InnerVoiceView.Band(v));
            }

            RefreshTiles(c);
            RefreshQueue(c);
            if (_detailKey != null) FillDetail(c);
        }

        private void RefreshTiles(Character c)
        {
            var ordered = MoodRules.InnerVoicesOrdered(c.Mood.Active);
            var sig = new System.Text.StringBuilder();
            foreach (var m in ordered) sig.Append(m.Def.Id).Append('|');
            SetOptional(_header, ordered.Count > 0 ? GameLocalization.R(MoodRules.InnerVoicesHeader(c.Mood.Active)) : "");
            if (sig.ToString() == _tileSignature)
            {
                // Aynı dizilim: yalnızca kayıtları tazele (süre akar, nesne yenilenmiş olabilir).
                for (int i = 0; i < ordered.Count && i < _tilePool.Count; i++) _tilePool[i].Moodlet = ordered[i];
                return;
            }
            _tileSignature = sig.ToString();
            _tiles.Clear();
            int shown = Math.Min(ordered.Count, (int)MaxTiles - (ordered.Count > MaxTiles ? 1 : 0));
            for (int i = 0; i < shown; i++)
            {
                while (_tilePool.Count <= i) _tilePool.Add(NewTile());
                var t = _tilePool[i];
                var m = ordered[i];
                t.Moodlet = m;
                int v = m.Def.Value;
                SetBorder(t.Root, 2, v > 0 ? JoyFrame : v < 0 ? GloomFrame : StatusFrame);
                SetIcon(t.Icon, InnerVoiceView.IconKeys(m.Def.Id, v));
                int pips = InnerVoiceView.Strength(v);
                for (int p = 0; p < 3; p++)
                {
                    var pip = t.Pips[p];
                    pip.style.display = p < pips ? DisplayStyle.Flex : DisplayStyle.None;
                    pip.style.backgroundColor = v > 0 ? JoyFrame : GloomFrame;
                }
                t.Root.tooltip = m.Def.DisplayName;
                t.Root.style.opacity = 1f;
                _tiles.Add(t.Root);
            }
            for (int i = shown; i < _tilePool.Count; i++) _tilePool[i].Moodlet = null;
            if (ordered.Count > shown)
            {
                _more.text = "+" + (ordered.Count - shown);
                _tiles.Add(_more);
            }
            // Ayrıntısı açık iç ses bittiyse ayrıntı kapanır.
            if (_detailKey != null && _detailKey.StartsWith("voice:", StringComparison.Ordinal) && Find(c, _detailKey.Substring(6)) == null) CloseDetail();
        }

        private void RefreshQueue(Character c)
        {
            var runner = _runnerOf(c);
            var queue = runner != null ? ActivityDescriber.Queue(runner, _life.World) : new List<ActivityInfo>();
            var sig = new System.Text.StringBuilder();
            foreach (var q in queue) sig.Append(q.Full).Append('|');
            if (sig.ToString() == _queueSignature) return;
            _queueSignature = sig.ToString();
            _queue.Clear();
            for (int i = 0; i < queue.Count; i++)
            {
                int index = i;
                var row = UiHost.Row();
                row.style.flexWrap = Wrap.NoWrap;
                var text = UiHost.Text((i == 0 ? "▶ " : i + ". ") + queue[i].Full);
                text.style.fontSize = 10;
                text.style.flexGrow = 1;
                text.style.flexShrink = 1;
                text.style.overflow = Overflow.Hidden;
                text.style.whiteSpace = WhiteSpace.NoWrap;
                row.Add(text);
                if (i > 0) row.Add(SmallButton(GameLocalization.T("ui.mood.card.queue_first"), () => { runner.Prioritize(index); _queueSignature = ""; }));
                row.Add(SmallButton(GameLocalization.T("ui.mood.card.queue_cancel"), () => { runner.CancelAt(index); _queueSignature = ""; }));
                _queue.Add(row);
            }
        }

        private static KitButton SmallButton(string text, Action click)
        {
            var b = new KitButton(text, click);
            b.style.height = 16;
            b.style.fontSize = 9;
            b.style.paddingTop = 0; b.style.paddingBottom = 0;
            return b;
        }

        // ---------------- ayrıntı (dokununca gerçek değer) ----------------

        private void ToggleDetail(string key)
        {
            if (_detailKey == key) { CloseDetail(); return; }
            _detailKey = key;
            _detail.style.display = DisplayStyle.Flex;
            var c = _shown ?? _who();
            if (c != null) FillDetail(c);
        }

        private void CloseDetail()
        {
            _detailKey = null;
            if (_detail != null) _detail.style.display = DisplayStyle.None;
        }

        private void FillDetail(Character c)
        {
            string title, body;
            IEnumerable<string> icon;
            if (_detailKey == "mood")
            {
                var status = c.Mood.Status;
                title = GameLocalization.R(InnerVoiceView.StatusName(status));
                body = GameLocalization.R(InnerVoiceView.MoodDetail(c.Mood.Total));
                string effects = GameLocalization.R(InnerVoiceView.EffectLine(status));
                if (effects.Length > 0) body += "\n" + effects;
                icon = new[] { InnerVoiceView.MoodIconKey(status) };
            }
            else if (_detailKey.StartsWith("need:", StringComparison.Ordinal))
            {
                var n = (NeedType)int.Parse(_detailKey.Substring(5));
                title = NeedKinds.DisplayName(n);
                body = GameLocalization.T("ui.mood.card.need_value", ("value", Mathf.RoundToInt(c.Needs.Get(n))));
                icon = new[] { InnerVoiceView.NeedIconKey(n) };
            }
            else
            {
                var m = Find(c, _detailKey.Substring(6));
                if (m == null) { CloseDetail(); return; }
                title = m.Def.DisplayName;
                body = GameLocalization.R(InnerVoiceView.KindLine(m.Def.Value)) + " · " + GameLocalization.R(InnerVoiceView.RemainingText(m));
                if (!string.IsNullOrEmpty(m.Def.Description)) body = m.Def.Description + "\n" + body;
                icon = InnerVoiceView.IconKeys(m.Def.Id, m.Def.Value);
            }
            SetText(_detailTitle, title);
            SetText(_detailBody, body);
            SetIcon(_detailIcon, icon);
        }

        private static ActiveMoodlet Find(Character c, string id)
        {
            foreach (var m in c.Mood.Active) if (m?.Def != null && string.Equals(m.Def.Id, id, StringComparison.Ordinal)) return m;
            return null;
        }

        // ---------------- renk ve metin ----------------

        private static Color StatusColor(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return new Color32(0x8E, 0x3B, 0x3B, 0xFF);
                case MoodStatus.Unhappy: return new Color32(0xB0, 0x6A, 0x3A, 0xFF);
                case MoodStatus.Good: return new Color32(0x6E, 0x8F, 0x4E, 0xFF);
                case MoodStatus.Happy: return new Color32(0xC8, 0x9A, 0x3A, 0xFF);
                default: return new Color32(0x8A, 0x7A, 0x66, 0xFF);
            }
        }

        private static Color BandColor(NeedBand b)
        {
            switch (b)
            {
                case NeedBand.Good: return new Color(0.45f, 0.72f, 0.38f);
                case NeedBand.Fair: return new Color(0.82f, 0.66f, 0.26f);
                default: return new Color(0.80f, 0.27f, 0.22f);
            }
        }

        private string TraitsText(Character c)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var id in c.TraitIds)
                if (_content.Traits.TryGet(id, out var t)) { sb.Append(sb.Length > 0 ? ", " : "· ").Append(t.DisplayName); }
            foreach (var id in c.BranchIds)
                if (_content.Branches.TryGet(id, out var b)) { sb.Append(sb.Length > 0 ? ", " : "· ").Append(b.DisplayName); }
            return sb.ToString();
        }

        private static void SetText(Label l, string text)
        {
            if (l.text != text) l.text = text;
        }

        private static void SetOptional(Label l, string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            var d = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (l.style.display != d) l.style.display = d;
            if (show) SetText(l, text);
        }
    }
}
