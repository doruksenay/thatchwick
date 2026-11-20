using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Thatchwick.Core.Work;

namespace Thatchwick.Game
{
    /// <summary>
    /// Balık tutma mini oyunu ekranı (Project belgesi 50; geçici IMGUI). Kurallar Core.Work.FishingGame'de; burada çizim ve
    /// girdi: fare sol tuşu, dokunma ya da boşluk tuşu. Basılı tutmak çubuğu yükseltir; "Vurdu!" anında basmak çekmeye başlatır.
    /// Açıkken ekranın tamamı arayüz sayılır (dünyaya dokunulmaz). Aynı anda gelen ikinci ısırık sıraya girer.
    /// </summary>
    public sealed class FishingView
    {
        private sealed class Session
        {
            public FishingGame Game;
            public string Title;
            public string FishName;
            public Action<bool, bool> Done;
        }

        private const float ResultSeconds = 1.4f;
        private readonly Queue<Session> _queue = new Queue<Session>();
        private Session _cur;
        private float _resultLeft;
        private static Texture2D _white, _dot;

        public bool Active => _cur != null;

        public void Start(FishingGame game, string title, string fishName, Action<bool, bool> done)
        {
            var s = new Session { Game = game, Title = title, FishName = fishName, Done = done };
            if (_cur == null) { _cur = s; _resultLeft = ResultSeconds; }
            else _queue.Enqueue(s);
        }

        /// <summary>Her karede (GameRoot.Update): girdi ve oyun adımı.</summary>
        public void Update(float dt)
        {
            if (_cur == null) return;
            var g = _cur.Game;
            if (!g.Done)
            {
                var mouse = Mouse.current;
                var touch = Touchscreen.current;
                var kb = Keyboard.current;
                bool hold = (mouse != null && mouse.leftButton.isPressed) || (touch != null && touch.primaryTouch.press.isPressed) || (kb != null && kb.spaceKey.isPressed);
                bool pressed = (mouse != null && mouse.leftButton.wasPressedThisFrame) || (touch != null && touch.primaryTouch.press.wasPressedThisFrame) || (kb != null && kb.spaceKey.wasPressedThisFrame);
                g.Step(Mathf.Min(dt, 0.05f), hold, pressed);
                return;
            }
            _resultLeft -= dt;
            if (_resultLeft > 0f) return;
            var done = _cur;
            _cur = _queue.Count > 0 ? _queue.Dequeue() : null;
            _resultLeft = ResultSeconds;
            done.Done?.Invoke(done.Game.Phase == FishingPhase.Caught, done.Game.Perfect);
        }

        public Rect PanelRect(float s) => new Rect(0f, 0f, Screen.width / s, Screen.height / s);   // açıkken her yer arayüz

        private static void Ensure()
        {
            if (_white == null) { _white = new Texture2D(1, 1); _white.SetPixel(0, 0, Color.white); _white.Apply(); }
            if (_dot != null) return;
            const int n = 32;
            _dot = new Texture2D(n, n) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    _dot.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - Mathf.Sqrt(dx * dx + dy * dy)) * 3f)));
                }
            _dot.Apply();
        }

        private static void Fill(Rect r, Color c) { var p = GUI.color; GUI.color = c; GUI.DrawTexture(r, _white); GUI.color = p; }
        private static void Dot(float cx, float cy, float w, float h, Color c) { var p = GUI.color; GUI.color = c; GUI.DrawTexture(new Rect(cx - w / 2f, cy - h / 2f, w, h), _dot); GUI.color = p; }
        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string name, object value)[] args) => GameLocalization.T(key, args);

        public void OnGUI(float s)
        {
            if (_cur == null) return;
            Ensure();
            var g = _cur.Game;
            Fill(new Rect(0, 0, Screen.width / s, Screen.height / s), new Color(0f, 0f, 0f, 0.35f));
            float w = 260f, h = 400f;
            var panel = new Rect((Screen.width / s - w) / 2f, (Screen.height / s - h) / 2f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, w - 24f, 22f), _cur.Title);

            // Su sütunu
            var track = new Rect(panel.x + 70f, panel.y + 40f, 56f, 300f);
            Fill(track, new Color(0.22f, 0.40f, 0.48f, 1f));
            for (int i = 0; i < 6; i++) Fill(new Rect(track.x, track.y + i * track.height / 6f, track.width, 1f), new Color(1f, 1f, 1f, 0.06f));
            float Y(float v) => track.yMax - v * track.height;   // 0 dip, 1 yüzey

            float now = Time.realtimeSinceStartup;
            switch (g.Phase)
            {
                case FishingPhase.Waiting:
                {
                    float bob = Mathf.Sin(now * 3f) * 3f;
                    Dot(track.center.x, track.y + 18f + bob, 14f, 14f, new Color(0.85f, 0.2f, 0.15f, 1f));   // şamandıra
                    GUI.Label(new Rect(panel.x + 12f, track.yMax + 10f, w - 24f, 40f), T("ui.fishing.wait"));
                    break;
                }
                case FishingPhase.Bite:
                {
                    float flash = Mathf.PingPong(now * 6f, 1f);
                    Dot(track.center.x, track.y + 26f, 18f, 18f, new Color(0.95f, 0.25f, 0.15f, 1f));
                    for (int i = 0; i < 3; i++) Dot(track.center.x, track.y + 26f, 24f + i * 12f, 10f + i * 4f, new Color(1f, 1f, 1f, 0.35f * (1f - flash)));
                    var st = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
                    GUI.Label(new Rect(panel.x, track.center.y - 20f, w, 40f), T("ui.fishing.bite"), st);
                    break;
                }
                default:
                {
                    // Çubuk (yeşil), balık, tutma çizgisi
                    var bar = new Rect(track.x + 3f, Y(g.BarPos + g.BarSize), track.width - 6f, g.BarSize * track.height);
                    Fill(bar, g.Overlapping ? new Color(0.45f, 0.85f, 0.40f, 0.75f) : new Color(0.40f, 0.70f, 0.35f, 0.55f));
                    var icon = IsoArt.ItemIcon(g.FishId);
                    float fy = Y(g.FishPos);
                    if (icon != null) GUI.DrawTexture(new Rect(track.center.x - 14f, fy - 14f, 28f, 28f), icon);
                    else { Dot(track.center.x, fy, 26f, 12f, new Color(0.85f, 0.75f, 0.55f, 1f)); Dot(track.center.x - 14f, fy, 10f, 9f, new Color(0.75f, 0.65f, 0.45f, 1f)); }
                    var prog = new Rect(track.xMax + 16f, track.y, 14f, track.height);
                    Fill(prog, new Color(0.15f, 0.12f, 0.1f, 0.8f));
                    var pc = Color.Lerp(new Color(0.85f, 0.3f, 0.2f), new Color(0.45f, 0.85f, 0.35f), g.Progress);
                    Fill(new Rect(prog.x + 2f, prog.yMax - 2f - (prog.height - 4f) * g.Progress, prog.width - 4f, (prog.height - 4f) * g.Progress), pc);
                    string line = g.Phase == FishingPhase.Caught ? T("ui.fishing.caught", ("fish", _cur.FishName)) + (g.Perfect ? "  " + T("ui.fishing.perfect") : "")
                                : g.Phase == FishingPhase.Escaped ? T(g.MissedBite ? "ui.fishing.missed" : "ui.fishing.escaped")
                                : T("ui.fishing.reel_hint");
                    GUI.Label(new Rect(panel.x + 12f, track.yMax + 10f, w - 24f, 40f), line);
                    break;
                }
            }
        }
    }
}
