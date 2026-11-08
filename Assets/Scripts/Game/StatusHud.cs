using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Okunabilirlik katmanı (Adım 18b, 18c; geçici IMGUI): üstte tarih/hava/tahmin ve kış göstergesi, altta komut alan
    /// kişinin ihtiyaç çubukları, ruh hali, iş kuyruğu şeridi (iptal/öne al), bildirim kaydı, gün sonu özet kartı ve
    /// karakterlerin üstünde eylem balonu. Kurallar ve metinler Core.Life'ta, burada yalnızca çizim vardır.
    /// </summary>
    public sealed class StatusHud
    {
        private readonly LifeSystems _life;
        private readonly ContentDatabase _content;
        private readonly Func<Character> _commander;
        private readonly Func<Character, JobRunner> _runnerOf;
        private readonly List<Rect> _rects = new List<Rect>();
        private DaySummary _card;
        private bool _logOpen;
        private Vector2 _logScroll;
        private GUIStyle _small, _balloon;

        public StatusHud(LifeSystems life, ContentDatabase content, Func<Character> commander, Func<Character, JobRunner> runnerOf)
        {
            _life = life; _content = content; _commander = commander; _runnerOf = runnerOf;
            _life.Ledger.DayEnded += s => { if (s != null && !s.IsEmpty) _card = s; };
        }

        public bool LogOpen => _logOpen;
        public void ToggleLog() { _logOpen = !_logOpen; if (_logOpen) _life.Log.MarkAllRead(); }

        /// <summary>Bu karede çizilen alanlardan biri imleç altında mı (haritaya dokunuşu engeller).</summary>
        public bool IsOver(Vector2 guiPos)
        {
            foreach (var r in _rects) if (r.Contains(guiPos)) return true;
            return false;
        }

        private void EnsureStyles()
        {
            if (_small != null) return;
            _small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = false };
            _balloon = new GUIStyle(GUI.skin.box) { fontSize = 11, alignment = TextAnchor.MiddleCenter, wordWrap = false, normal = { textColor = Color.white } };
        }

        private static Color NeedColor(float v)
        {
            if (v < 15f) return new Color(0.85f, 0.25f, 0.2f);
            if (v < 30f) return new Color(0.9f, 0.65f, 0.2f);
            return new Color(0.45f, 0.75f, 0.4f);
        }

        private static string NeedName(NeedType n) => NeedKinds.DisplayName(n);

        private static void Bar(string label, float value, float width)
        {
            GUILayout.BeginVertical(GUILayout.Width(width));
            GUILayout.Label($"{label} {value:0}", GUILayout.Width(width));
            var r = GUILayoutUtility.GetRect(width, 8f);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = NeedColor(value);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(value / 100f), r.height), Texture2D.whiteTexture);
            GUI.color = prev;
            GUILayout.EndVertical();
        }

        public void OnGUI(float s)
        {
            EnsureStyles();
            _rects.Clear();
            float w = Screen.width / s;
            float h = Screen.height / s;
            DrawTopBar(w);
            DrawBottom(w, h);
            DrawLogButton(w, h);
            if (_logOpen) DrawLog(w, h);
            if (_card != null) DrawCard(w, h);
        }

        // ---- üst şerit: tarih, hava, tahmin, şenlik, kış göstergesi ----

        private void DrawTopBar(float w)
        {
            var clock = _life.Clock;
            var today = _life.Weather.Today(clock.TotalDays);
            var fest = _life.Festivals.TodaysFestival();
            string line1 = $"{_life.Festivals.DateText(clock.DayIndex)} · {WeatherSystem.Name(today)} · {_life.Weather.ForecastText(clock.TotalDays)}";
            string line2;
            if (fest != null) line2 = $"Bugün: {fest.DisplayName}";
            else
            {
                var next = _life.Festivals.NextFestival(out int days);
                line2 = (_life.Festivals.IsMarketDayToday ? "Bugün pazar günü · " : "") + (next != null ? $"Sıradaki şenlik: {next.DisplayName} ({days} gün)" : "");
            }
            var rep = _life.Forecast.Report(clock.TotalDays);
            string line3 = rep.InWinter
                ? $"Kış: {rep.WinterDaysLeft:0} gün kaldı · yiyecek {rep.FoodDays:0} gün · odun {rep.FirewoodDays:0} gün"
                : $"Kışa {rep.DaysToWinter:0} gün · yiyecek {rep.FoodDays:0} gün · odun {rep.FirewoodDays:0} gün yeter";

            var rect = new Rect(w * 0.5f - 250f, 4f, 500f, 62f);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(line1, _small);
            if (line2.Length > 0) GUILayout.Label(line2, _small);
            var prev = GUI.color;
            GUI.color = rep.HasDanger ? new Color(1f, 0.55f, 0.5f) : (rep.Warnings.Count > 0 ? new Color(1f, 0.85f, 0.5f) : prev);
            GUILayout.Label(line3, _small);
            GUI.color = prev;
            GUILayout.EndArea();
        }

        // ---- alt şerit: kuyruk + ihtiyaç çubukları + ruh hali ----

        private void DrawBottom(float w, float h)
        {
            var c = _commander();
            if (c == null || c.IsDead) return;
            var runner = _runnerOf(c);
            var queue = runner != null ? ActivityDescriber.Queue(runner, _life.World) : new List<ActivityInfo>();
            float height = 78f + queue.Count * 20f;
            var rect = new Rect(w * 0.5f - 300f, h - height - 6f, 600f, height);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{c.Name} · {MoodRules.Name(c.Mood.Status)} ({(c.Mood.Total > 0 ? "+" : "")}{c.Mood.Total})", _small, GUILayout.Width(190f));
            string traits = TraitsText(c);
            if (traits.Length > 0) GUILayout.Label(traits, _small);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
                Bar(NeedName(n), c.Needs.Get(n), 108f);
            GUILayout.EndHorizontal();

            if (c.Mood.Active.Count > 0)
            {
                var parts = new System.Text.StringBuilder();
                foreach (var m in c.Mood.Active)
                {
                    if (parts.Length > 0) parts.Append(" · ");
                    parts.Append($"{m.Def.DisplayName} {(m.Def.Value > 0 ? "+" : "")}{m.Def.Value}");
                }
                GUILayout.Label(parts.ToString(), _small);
            }

            if (runner != null)
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{(i == 0 ? "▶" : $"{i}.")} {queue[i].Full}", _small);
                    GUILayout.FlexibleSpace();
                    if (i > 0 && GUILayout.Button("Öne al", GUILayout.Width(56f))) { runner.Prioritize(i); GUILayout.EndHorizontal(); break; }
                    if (GUILayout.Button("İptal", GUILayout.Width(48f))) { runner.CancelAt(i); GUILayout.EndHorizontal(); break; }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
        }

        private string TraitsText(Character c)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var id in c.TraitIds)
                if (_content.Traits.TryGet(id, out var t)) { if (sb.Length > 0) sb.Append(", "); sb.Append(t.DisplayName); }
            foreach (var id in c.BranchIds)
                if (_content.Branches.TryGet(id, out var b)) { if (sb.Length > 0) sb.Append(", "); sb.Append(b.DisplayName); }
            return sb.ToString();
        }

        // ---- bildirimler ----

        private void DrawLogButton(float w, float h)
        {
            var rect = new Rect(8f, h - 30f, 150f, 24f);
            _rects.Add(rect);
            int unread = _life.Log.UnreadCount;
            if (GUI.Button(rect, unread > 0 ? $"Bildirimler ({unread}) [N]" : "Bildirimler [N]")) ToggleLog();
        }

        private void DrawLog(float w, float h)
        {
            var rect = new Rect(8f, h - 330f, 380f, 296f);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("BİLDİRİMLER");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) { _logOpen = false; GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();
            _logScroll = GUILayout.BeginScrollView(_logScroll);
            var prev = GUI.color;
            foreach (var e in _life.Log.Recent(40))
            {
                GUI.color = e.Level == NoticeLevel.Danger ? new Color(1f, 0.55f, 0.5f) : e.Level == NoticeLevel.Warning ? new Color(1f, 0.85f, 0.5f) : prev;
                GUILayout.Label($"{_life.Festivals.DateText((long)e.Day)}: {e.Text}{(e.Repeats > 1 ? $" (x{e.Repeats})" : "")}", _small);
            }
            GUI.color = prev;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---- gün sonu özet kartı ----

        private void DrawCard(float w, float h)
        {
            var d = _card;
            var rect = new Rect(w * 0.5f - 160f, h * 0.5f - 130f, 320f, 260f);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label($"Gün sonu · {_life.Festivals.DateText(d.Day)}");
            GUILayout.Label($"Akçe {Signed(d.AkceDelta)} · XP {Signed(d.XpDelta)}");
            if (d.Born > 0 || d.Died > 0) GUILayout.Label($"Doğan {d.Born} · Ölen {d.Died}: {string.Join(", ", d.Names)}", _small);
            foreach (var kv in d.ItemDeltas)
            {
                string name = _content.Items.TryGet(kv.Key, out var def) ? def.DisplayName : kv.Key;
                GUILayout.Label($"   {name} {Signed(kv.Value)}", _small);
            }
            foreach (var n in d.Notes) GUILayout.Label("• " + n, _small);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Tamam")) _card = null;
            GUILayout.EndArea();
        }

        private static string Signed(int v) => v > 0 ? "+" + v : v.ToString();

        // ---- eylem balonu: karakterin başının üstünde ne yaptığı ve neden ----

        public void DrawBalloon(float guiScale, Vector3 screenPoint, Character c, JobRunner runner)
        {
            if (c.IsDead || screenPoint.z < 0f) return;
            EnsureStyles();
            var info = ActivityDescriber.Describe(c, runner, _life.World);
            if (info.Icon == "idle" && string.IsNullOrEmpty(info.Reason)) return;   // boştakiler için balon çizilmez
            float x = screenPoint.x / guiScale;
            float y = (Screen.height - screenPoint.y) / guiScale;
            string text = info.Full;
            float width = Mathf.Clamp(text.Length * 6.2f + 12f, 60f, 260f);
            GUI.Box(new Rect(x - width * 0.5f, y - 46f, width, 20f), text, _balloon);
        }
    }
}
