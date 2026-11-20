using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Progress;
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
        private float _cardShownAt;
        /// <summary>Gün sonu kartının ekranda kalma süresi (gerçek saniye).</summary>
        private const float CardSeconds = 10f;

        /// <summary>Sol panelin sağ kenarı (GUI birimi); üst şerit bunun sağında kalır.</summary>
        public float LeftInset { get; set; }
        // Kış tahmini pahalıdır (envanter x tüm etkileşimler): her çizimde değil, yarım saniyede bir hesaplanır.
        private WinterReport _forecast;
        private float _forecastUntil;
        private bool _logOpen;
        private Vector2 _logScroll;
        private GUIStyle _small, _balloon;

        private readonly Func<Thatchwick.Core.Work.FarmSummary> _farm;

        public StatusHud(LifeSystems life, ContentDatabase content, Func<Character> commander, Func<Character, JobRunner> runnerOf,
            Func<Thatchwick.Core.Work.FarmSummary> farm = null)
        {
            _life = life; _content = content; _commander = commander; _runnerOf = runnerOf; _farm = farm;
            _life.Ledger.DayEnded += s => { if (s != null && !s.IsEmpty) { _card = s; _cardShownAt = Time.unscaledTime; } };
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
            DrawLevelCards(w);
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
            line2 = $"Soy Mührü {_life.Legacy.Seals}" + (line2.Length > 0 ? " · " + line2 : "");
            if (_forecast == null || Time.unscaledTime >= _forecastUntil)
            {
                _forecast = _life.Forecast.Report(clock.TotalDays);
                _forecastUntil = Time.unscaledTime + 0.5f;
            }
            var rep = _forecast;
            string D(double d) => double.IsInfinity(d) || double.IsNaN(d) ? "?" : Math.Max(0, d).ToString("0");
            string line3 = rep.InWinter
                ? $"Kış: {D(rep.WinterDaysLeft)} gün kaldı · yiyecek {D(rep.FoodDays)} gün · odun {D(rep.FirewoodDays)} gün"
                : $"Kışa {D(rep.DaysToWinter)} gün · yiyecek {D(rep.FoodDays)} gün · odun {D(rep.FirewoodDays)} gün yeter";

            var farm = _farm != null ? _farm() : default;
            string line4 = farm.Any
                ? $"Tarla: {farm.Crops} ekin · {farm.Thirsty} susamış · {farm.Ripe} hasada hazır" + (farm.FruitReady > 0 ? $" · meyve hazır: {farm.FruitReady}" : "")
                : "";
            // Sol paneli örtmesin: ortalanır, yer yoksa panelin sağına kayar ve daralır.
            float left = Mathf.Max(w * 0.5f - 250f, LeftInset + 8f);
            var rect = new Rect(left, 4f, Mathf.Min(500f, w - left - 8f), line4.Length > 0 ? 88f : 70f);   // üçüncü satır kesilmesin
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(line1, _small);
            if (line2.Length > 0) GUILayout.Label(line2, _small);
            var prev = GUI.color;
            GUI.color = rep.HasDanger ? new Color(1f, 0.55f, 0.5f) : (rep.Warnings.Count > 0 ? new Color(1f, 0.85f, 0.5f) : prev);
            GUILayout.Label(line3, _small);
            GUI.color = prev;
            if (line4.Length > 0) GUILayout.Label(line4, _small);
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
            float left = Mathf.Max(w * 0.5f - 380f, 166f);   // bildirim düğmesinin sağında
            var rect = new Rect(left, h - height - 6f, Mathf.Min(760f, w - left - 8f), height);
            // İhtiyaç çubukları şeride sığsın (dar ekranda son çubuk kesilmesin).
            int needCount = Enum.GetValues(typeof(NeedType)).Length;
            float barWidth = Mathf.Min(108f, (rect.width - 16f) / needCount - 4f);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{c.Name} · {MoodRules.Name(c.Mood.Status)} ({(c.Mood.Total > 0 ? "+" : "")}{c.Mood.Total})", _small, GUILayout.Width(190f));
            string traits = TraitsText(c);
            if (traits.Length > 0) GUILayout.Label(traits, _small);
            GUILayout.EndHorizontal();

            // Çocuk ve Ergen: yetişme göstergesi (çocukluk ruh hali Genç yetişkin'de özelliğe dönüşür).
            if (c.MoodEnabled && (c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen))
                GUILayout.Label(MoodBehavior.UpbringingSummary(c.UpbringingAverage, c.UpbringingDays), _small);
            if (c.InBreakdown)
                GUILayout.Label($"Bunalımda: {MoodBehavior.BreakdownName(c.Breakdown)} (emirler bitince yapılır)", _small);

            GUILayout.BeginHorizontal();
            foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
                Bar(NeedName(n), c.Needs.Get(n), barWidth);
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
            // Ekranın ortasını kapatmasın: sağ üstte, üst şeridin altında. Üstüne gelinmezse
            // CardSeconds sonra kendiliğinden kapanır, son iki saniyede solar (görsel analiz, 18.11.2026).
            var rect = new Rect(w - 328f, 90f, 320f, 260f);
            if (rect.Contains(Event.current.mousePosition)) _cardShownAt = Mathf.Max(_cardShownAt, Time.unscaledTime - 2f);
            float age = Time.unscaledTime - _cardShownAt;
            if (age >= CardSeconds) { _card = null; return; }
            float alpha = Mathf.Clamp01((CardSeconds - age) / 2f);
            var prevCardColor = GUI.color;
            GUI.color = new Color(prevCardColor.r, prevCardColor.g, prevCardColor.b, prevCardColor.a * alpha);
            _rects.Add(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label($"Gün sonu · {_life.Festivals.DateText(d.Day)}");
            GUILayout.Label($"Akçe {Signed(d.AkceDelta)} · XP {Signed(d.XpDelta)}");
            if (d.Born > 0 || d.Died > 0) GUILayout.Label($"Doğan {d.Born} · Ölen {d.Died}: {string.Join(", ", d.Names)}", _small);
            foreach (var kv in d.ItemDeltas)
            {
                string name = _content.Items.TryGet(kv.Key, out var def) ? def.DisplayName : kv.Key;
                GUILayout.BeginHorizontal();
                IsoArt.IconCell(kv.Key, 16f);
                GUILayout.Label($"{name} {Signed(kv.Value)}", _small);
                GUILayout.EndHorizontal();
            }
            foreach (var n in d.Notes) GUILayout.Label("• " + n, _small);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Tamam")) _card = null;
            GUILayout.EndArea();
            GUI.color = prevCardColor;
        }

        private static string Signed(int v) => v > 0 ? "+" + v : v.ToString();

        // ---- eylem balonu: karakterin başının üstünde ne yaptığı ve neden ----

        public void DrawBalloon(float guiScale, Vector3 screenPoint, Character c, JobRunner runner)
        {
            if (c.IsDead || screenPoint.z < 0f) return;
            EnsureStyles();
            float x = screenPoint.x / guiScale;
            float y = (Screen.height - screenPoint.y) / guiScale;
            var info = ActivityDescriber.Describe(c, runner, _life.World);
            if (!(info.Icon == "idle" && string.IsNullOrEmpty(info.Reason)))   // boştakiler için balon çizilmez
            {
                string text = info.Full;
                float width = Mathf.Clamp(text.Length * 6.2f + 12f, 60f, 260f);
                GUI.Box(new Rect(x - width * 0.5f, y - 46f, width, 20f), text, _balloon);
            }
            DrawSkillFlash(x, y, c);
        }

        // ---- beceri çubuğu ve seviye kartı (Project belgesi 45 bölüm 10) ----
        // XP gelirken balonun altında 3 sn beceri çubuğu; seviye atlayınca baş üstünde "Beceri N!" ve köşede kart.
        // Görseller (ikon, çerçeve) art/queue 45x işinden gelene kadar düz renkli yer tutucu. Ses hattı henüz yok.

        public const float SkillBarSeconds = 3f, LevelTextSeconds = 2.5f, LevelCardSeconds = 7f;
        /// <summary>Bundan küçük XP (yürürken Atletizm) çubuk açmaz; yoksa yürüyen herkesin başında çubuk olurdu.</summary>
        public const double MinBarXp = 0.25;
        public const int MaxLevelCards = 3;

        private sealed class SkillFlash { public SkillType Skill; public float Until; public int LevelTo; public float LevelUntil; }
        private sealed class LevelCard { public Character Who; public SkillType Skill; public int Level; public List<string> Items; public float Until; }
        private readonly Dictionary<int, SkillFlash> _flash = new Dictionary<int, SkillFlash>();
        private readonly List<LevelCard> _levelCards = new List<LevelCard>();
        private GUIStyle _levelUp;

        /// <summary>Kartı tıklayınca: o karakterin beceri şeridini açar (GameRoot bağlar).</summary>
        public Action<Character, SkillType> OpenSkills { get; set; }

        /// <summary>Karakterin XP ve seviye olaylarını dinler (GameRoot.CreateActorFor).</summary>
        public void Watch(Character c)
        {
            if (c == null) return;
            c.Skills.XpGained += (skill, xp) =>
            {
                if (xp < MinBarXp) return;
                var f = Flash(c.Id);
                f.Skill = skill;
                f.Until = Time.unscaledTime + SkillBarSeconds;
            };
            c.Skills.LevelChanged += (skill, from, to) =>
            {
                if (to <= from || c.IsVillager) return;
                var f = Flash(c.Id);
                f.Skill = skill;
                f.Until = Time.unscaledTime + SkillBarSeconds;
                f.LevelTo = to;
                f.LevelUntil = Time.unscaledTime + LevelTextSeconds;
                _levelCards.Add(new LevelCard
                {
                    Who = c, Skill = skill, Level = to, Items = UnlockGate.NewAtLevel(_content, skill, to), Until = Time.unscaledTime + LevelCardSeconds
                });
                if (_levelCards.Count > MaxLevelCards) _levelCards.RemoveAt(0);
            };
        }

        /// <summary>Beceri ikonu: Resources/UI/Icons/ui_icon_skill_&lt;beceri&gt; (art/queue 455a-c); yoksa yalnızca yazı.</summary>
        public static string SkillIconKey(SkillType s) => "skill_" + s.ToString().ToLowerInvariant();

        private SkillFlash Flash(int id)
        {
            if (!_flash.TryGetValue(id, out var f)) _flash[id] = f = new SkillFlash();
            return f;
        }

        private void DrawSkillFlash(float x, float y, Character c)
        {
            if (!_flash.TryGetValue(c.Id, out var f)) return;
            float now = Time.unscaledTime;
            if (now < f.Until)
            {
                const float bw = 110f;
                int level = c.Skills.GetLevel(f.Skill);
                var apt = c.Skills.GetAptitude(f.Skill);
                bool tired = c.Skills.IsSaturated(f.Skill);
                string label = $"{SkillNames.Name(f.Skill)} {level}{(apt == Aptitude.Major ? " ★★" : apt == Aptitude.Minor ? " ★" : "")}{(tired ? " ☾" : "")}";
                var prev = GUI.color;
                GUI.color = tired ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
                GUI.Label(new Rect(x - bw * 0.5f, y - 26f, bw, 14f), UiSkin.C(label, SkillIconKey(f.Skill)), _small);
                var r = new Rect(x - bw * 0.5f, y - 13f, bw, 5f);
                GUI.color = new Color(0f, 0f, 0f, 0.55f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = tired ? new Color(0.6f, 0.6f, 0.65f, 0.8f) : new Color(0.55f, 0.16f, 0.2f);   // nakış bordosu
                GUI.DrawTexture(new Rect(r.x, r.y, r.width * (float)c.Skills.LevelProgress(f.Skill), r.height), Texture2D.whiteTexture);
                GUI.color = prev;
            }
            if (now < f.LevelUntil && f.LevelTo > 0)
            {
                if (_levelUp == null) _levelUp = new GUIStyle(_small) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.35f) } };
                float rise = (LevelTextSeconds - (f.LevelUntil - now)) * 8f;
                GUI.Label(new Rect(x - 90f, y - 72f - rise, 180f, 20f), $"{SkillNames.Name(f.Skill)} {f.LevelTo}!", _levelUp);
            }
        }

        private void DrawLevelCards(float w)
        {
            float now = Time.unscaledTime;
            _levelCards.RemoveAll(k => now >= k.Until || k.Who == null || k.Who.IsDead);
            float y = 96f;
            for (int i = _levelCards.Count - 1; i >= 0; i--)
            {
                var k = _levelCards[i];
                float hgt = 38f + 15f * Mathf.Max(1, k.Items.Count);
                var rect = new Rect(w - 248f, y, 240f, hgt);
                _rects.Add(rect);
                GUILayout.BeginArea(rect, GUI.skin.box);
                if (GUILayout.Button(UiSkin.C($"{k.Who.Name}: {SkillNames.Name(k.Skill)} {k.Level}!", SkillIconKey(k.Skill)), _small)) { OpenSkills?.Invoke(k.Who, k.Skill); k.Until = 0f; }
                if (k.Items.Count == 0) GUILayout.Label("Ustalık arttı: işler daha hızlı.", _small);
                else
                {
                    GUILayout.Label("Yeni:", _small);
                    foreach (var t in k.Items) GUILayout.Label("· " + t, _small);
                }
                GUILayout.EndArea();
                y += hgt + 4f;
            }
        }
    }
}
