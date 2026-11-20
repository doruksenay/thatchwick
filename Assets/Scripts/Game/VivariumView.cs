using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Cam bahçe yakın görünümü (Project belgesi 47; geçici IMGUI). Cam bahçeye dokununca kamera ona kayıp yakınlaşır,
    /// ardından büyük ekranda içi açılır: zemin (toprak, kum, tatlı ya da tuzlu su), oyuncunun dizdiği taş, kabuk, odun ve
    /// bitkiler kendi yerlerinde görünür; böcekler türüne göre yürür, zıplar, uçar, toprağa girer, taşın ardına saklanır;
    /// balıklar yüzer, dipte gezer; gece ateş böcekleri ışır. Sağda içindekiler (bitki ve süs ◀ ▶ ile kaydırılır),
    /// envanterden ekleme (kurulamayan kombinasyon nedeniyle), nem ve günlük.
    /// Kurallar Core.Life.HomeLife (HomeVivarium.cs). Türlerin görseli gelene kadar renkli şekillerle çizilir
    /// (IsoArt "viv_*" görseli varsa o kullanılır; art/queue 455-458).
    /// </summary>
    public sealed class VivariumView
    {
        private sealed class Mote
        {
            public float X, Y, Vx, Vy, Phase, Timer, Alpha = 1f, Facing = 1f;
            public bool Air;     // zıplarken havada
            public float HideX = -1f;   // saklanmaya gittiği süsün yeri (-1 yok)
            public bool Hidden;         // taşın, kabuğun ardında
        }

        private const float OpenDelay = 0.45f;          // kamera yakınlaşırken bekle
        private const float FocusZoom = 1.6f;

        private readonly HomeLife _home;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly GameClock _clock;
        private readonly WorldInput _input;
        private readonly Func<Character> _actor;
        private readonly Dictionary<VivEntry, Mote> _motes = new Dictionary<VivEntry, Mote>();
        private readonly System.Random _rng = new System.Random();
        private ItemInstance _item;
        private float _openedAt;
        private Vector2 _scroll;
        private string _status;
        private string _pending;            // eklenecek tür (uyarı onayı bekliyor)
        private static Texture2D _white, _dot;

        public bool Active => _item != null;
        public event Action<string> Message;
        /// <summary>Etkileşimi oyuncu karakterine iş olarak verir (Seyret).</summary>
        public event Action<ItemInstance, string> Use;

        public VivariumView(HomeLife home, ContentDatabase content, Household house, GameClock clock, WorldInput input, Func<Character> actor)
        {
            _home = home; _content = content; _house = house; _clock = clock; _input = input; _actor = actor;
        }

        public void Open(ItemInstance item, Vector2 worldPos)
        {
            if (item == null || !item.Def.IsVivarium) return;
            _item = item;
            _openedAt = Time.realtimeSinceStartup;
            _status = null; _pending = null;
            _motes.Clear();
            _input?.FocusOn(worldPos, FocusZoom);
        }

        public void Close()
        {
            if (_item == null) return;
            _item = null;
            _motes.Clear();
            _input?.RestoreView();
        }

        public Rect PanelRect(float s)
        {
            float w = Mathf.Min(Screen.width / s - 24f, 860f), h = Mathf.Min(Screen.height / s - 24f, 540f);
            return new Rect((Screen.width / s - w) / 2f, (Screen.height / s - h) / 2f, w, h);
        }

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;
        private VivDef V(string id) => _content.Items.TryGet(id, out var d) ? d.Viv : null;

        public void OnGUI(float s)
        {
            if (_item == null) return;
            if (_home.VivOf(_item) == null) { Close(); return; }   // yerinden kaldırıldı
            float t = (Time.realtimeSinceStartup - _openedAt - OpenDelay) / 0.25f;
            if (t <= 0f) return;                                       // önce kamera yaklaşsın
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t));
            var rect = PanelRect(s);
            GUI.Box(rect, GUIContent.none);
            float side = Mathf.Min(300f, rect.width * 0.38f);
            var tank = new Rect(rect.x + 10f, rect.y + 34f, rect.width - side - 30f, rect.height - 44f);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 6f, tank.width, 24f), GameLocalization.Current.ToUpper(_item.Def.DisplayName) + "  ·  " + (L(_home.VivSummary(_item)) ?? ""));
            if (GUI.Button(new Rect(rect.xMax - 70f, rect.y + 6f, 60f, 24f), T("ui.common.close"))) { Close(); GUI.color = prev; return; }
            DrawTank(tank);
            GUILayout.BeginArea(new Rect(tank.xMax + 12f, rect.y + 34f, side, rect.height - 44f));
            DrawSide();
            GUILayout.EndArea();
            GUI.color = prev;
        }

        // ---------- tank ----------

        private static void EnsureTextures()
        {
            if (_white == null) { _white = new Texture2D(1, 1); _white.SetPixel(0, 0, Color.white); _white.Apply(); }
            if (_dot != null) return;
            const int n = 32;
            _dot = new Texture2D(n, n) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    _dot.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 3f)));
                }
            _dot.Apply();
        }

        private static void Fill(Rect r, Color c) { var p = GUI.color; GUI.color = new Color(c.r, c.g, c.b, c.a * p.a); GUI.DrawTexture(r, _white); GUI.color = p; }
        private static void Dot(float cx, float cy, float w, float h, Color c) { var p = GUI.color; GUI.color = new Color(c.r, c.g, c.b, c.a * p.a); GUI.DrawTexture(new Rect(cx - w / 2f, cy - h / 2f, w, h), _dot); GUI.color = p; }

        private static Color Hex(string hex, Color fallback)
        {
            if (string.IsNullOrEmpty(hex)) return fallback;
            var c = HomePanel.Hex(hex);
            return c.a <= 0f ? fallback : c;
        }

        private static string Short(string id) => id.Replace("item.", "").Replace("viv_", "");
        private static Sprite Art(string id) => IsoArt.Get("viv_" + Short(id), 0f, 0f);

        private void DrawTank(Rect r)
        {
            EnsureTextures();
            var v = _home.VivOf(_item);
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f), now = Time.realtimeSinceStartup;
            bool night = HomeLife.IsNight(_clock.DayFraction);
            string hab = v.Habitat;
            bool water = HomeLife.IsWater(hab);
            float waterTop = r.y + r.height * 0.12f;
            float ground = r.yMax - r.height * (water ? 0.14f : 0.2f);

            // arka plan: zemin görseli varsa o, yoksa çizim
            var bg = hab == null ? null : IsoArt.Get("viv_bg_" + (hab == HomeLife.Dry ? "dry" : hab == HomeLife.Salt ? "salt" : hab == HomeLife.Fresh ? "fresh" : "moist"), 0f, 0f);
            if (bg != null) GUI.DrawTexture(r, bg.texture);
            else
            {
                Fill(r, new Color(0.80f, 0.86f, 0.80f, 0.95f));
                if (water)
                {
                    var wc = hab == HomeLife.Salt ? new Color(0.35f, 0.55f, 0.62f, 0.85f) : new Color(0.42f, 0.58f, 0.46f, 0.85f);
                    Fill(new Rect(r.x, waterTop, r.width, r.yMax - waterTop), wc);
                    Fill(new Rect(r.x, waterTop, r.width, 2f), new Color(1f, 1f, 1f, 0.45f));
                    for (int i = 0; i < 5; i++)   // ışık huzmesi
                        Fill(new Rect(r.x + r.width * (0.1f + i * 0.2f) + Mathf.Sin(now * 0.3f + i) * 6f, waterTop, 10f, (ground - waterTop) * 0.8f), new Color(1f, 1f, 0.9f, 0.05f));
                }
                if (hab != null)
                {
                    // Zeminin rengi konan malzemeden: orman toprağı koyu, göl kumu sarımsı, deniz kumu açık, sazlık çamuru gri-kahve.
                    var soil = v.Substrate == "item.viv_sea_sand" ? new Color(0.88f, 0.83f, 0.68f)
                             : v.Substrate == "item.viv_marsh_mud" ? new Color(0.30f, 0.27f, 0.20f)
                             : v.Substrate == "item.sand" ? new Color(0.80f, 0.68f, 0.45f)
                             : new Color(0.33f, 0.24f, 0.16f);
                    Fill(new Rect(r.x, ground, r.width, r.yMax - ground), soil);
                    for (int i = 0; i < 26; i++)
                    {
                        float gx = r.x + (i * 37 % 100) / 100f * r.width, gy = ground + 6f + (i * 53 % 100) / 100f * (r.yMax - ground - 10f);
                        Dot(gx, gy, 6f + i % 3 * 3f, 4f + i % 2 * 2f, soil * (0.7f + (i % 4) * 0.1f));
                    }
                }
                else GUI.Label(new Rect(r.x + 20f, r.center.y - 12f, r.width - 40f, 48f), T("ui.viv.empty_tank"));
            }

            // 1. bitkiler (oyuncunun dizdiği yerde)
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsPlant) continue;
                DrawPlant(e, s, r.x + (float)e.Pos * r.width, ground, waterTop, r.height, now + (float)e.Pos * 9f, water);
            }

            // 2. canlıların adımı; saklananlar süsün ardında kalsın diye önce çizilir
            var decorX = new List<float>();
            foreach (var e in v.Entries) { var s = V(e.Item); if (s != null && s.IsDecor && s.Has("hide")) decorX.Add(r.x + (float)e.Pos * r.width); }
            var seen = new HashSet<VivEntry>();
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsCreature) continue;
                seen.Add(e);
                if (!_motes.TryGetValue(e, out var m)) { m = Spawn(s, r, ground, waterTop, water); _motes[e] = m; }
                Step(m, s, r, ground, waterTop, water, decorX, dt, now);
                if (m.Hidden && !(night && s.Has("glow"))) DrawCreature(e, s, m, now, 0.45f);
            }
            var drop = new List<VivEntry>();
            foreach (var k in _motes.Keys) if (!seen.Contains(k)) drop.Add(k);
            foreach (var k in drop) _motes.Remove(k);

            // 3. süsler: taş, kabuk, odun
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsDecor) continue;
                DrawDecor(e, s, r.x + (float)e.Pos * r.width, ground);
            }

            // 4. görünen canlılar
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsCreature || !_motes.TryGetValue(e, out var m) || m.Hidden) continue;
                if (!(night && s.Has("glow"))) DrawCreature(e, s, m, now, 1f);
            }
            if (water && v.Entries.Count > 0)   // kabarcık
                for (int i = 0; i < 6; i++)
                {
                    float bt = (now * 0.25f + i * 0.17f) % 1f;
                    Dot(r.x + r.width * (0.15f + i * 0.14f) + Mathf.Sin(now * 2f + i) * 3f, Mathf.Lerp(ground - 4f, waterTop + 4f, bt), 4f, 4f, new Color(1f, 1f, 1f, 0.45f * (1f - bt)));
                }

            // küf, gece, ateş böceği ışığı
            if (v.Mold >= 15)
                for (int i = 0; i < (int)(v.Mold / 6f); i++)
                    Dot(r.x + (i * 61 % 100) / 100f * r.width, r.y + (i * 29 % 100) / 100f * r.height * 0.5f + 8f, 14f, 10f, new Color(0.55f, 0.62f, 0.50f, 0.35f));
            if (night) Fill(r, new Color(0.04f, 0.06f, 0.16f, 0.55f));
            if (night)
                foreach (var e in v.Entries)
                {
                    var s = V(e.Item);
                    if (s == null || !s.Has("glow") || !_motes.TryGetValue(e, out var m)) continue;
                    float pulse = 0.5f + 0.5f * Mathf.Sin(now * 3.1f + m.Phase);
                    Dot(m.X, m.Y, 34f, 34f, new Color(1f, 0.95f, 0.45f, 0.18f + 0.25f * pulse));
                    Dot(m.X, m.Y, 7f, 7f, new Color(1f, 1f, 0.7f, 0.6f + 0.4f * pulse));
                }
            // cam: çerçeve ve parıltı; nemli camda damla
            Fill(new Rect(r.x, r.y, r.width, 3f), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.x, r.yMax - 3f, r.width, 3f), new Color(0.40f, 0.30f, 0.20f));
            Fill(new Rect(r.x, r.y, 3f, r.height), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.xMax - 3f, r.y, 3f, r.height), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.x + r.width * 0.08f, r.y + 10f, 6f, r.height * 0.55f), new Color(1f, 1f, 1f, 0.18f));
            if (hab != null && !water && v.Moisture > HomeLife.MoistRange(hab).hi - 10)
                for (int i = 0; i < 18; i++) Dot(r.x + (i * 43 % 100) / 100f * r.width, r.y + (i * 71 % 100) / 100f * r.height * 0.7f, 4f, 5f, new Color(1f, 1f, 1f, 0.35f));
            if (water && v.Moisture < 45) Fill(new Rect(r.x, waterTop, r.width, ground - waterTop), new Color(0.45f, 0.5f, 0.25f, 0.25f));   // bayat su bulanır
        }

        private void DrawPlant(VivEntry e, VivDef s, float x, float ground, float waterTop, float tankH, float t, bool water)
        {
            var art = Art(e.Item);
            float scale = 0.6f + 0.2f * Mathf.Min(3f, (float)e.Growth);
            float sway = Mathf.Sin(t * (water ? 1.2f : 0.7f)) * 4f;
            float wilt = Mathf.Clamp01((float)e.Health / 100f);
            var col = Color.Lerp(new Color(0.55f, 0.45f, 0.25f), Hex(s.Color, new Color(0.3f, 0.6f, 0.3f)), wilt);
            bool floats = s.Has("float") && water;
            if (art != null)
            {
                float w = art.texture.width * 2f * scale, h = art.texture.height * 2f * scale;
                var p = GUI.color; GUI.color = new Color(col.r * 0.4f + 0.6f, col.g * 0.4f + 0.6f, col.b * 0.4f + 0.6f, p.a);
                GUI.DrawTexture(new Rect(x - w / 2f + sway * 0.5f, (floats ? waterTop + h * 0.3f : ground + 4f) - h, w, h), art.texture);
                GUI.color = p;
                return;
            }
            if (floats)
            {
                if (s.Has("flower"))   // nilüfer: dipten sap, yüzeyde yaprak ve çiçek
                {
                    Fill(new Rect(x, waterTop + 4f, 2f, ground - waterTop - 4f), col * 0.7f);
                    Dot(x - 10f + sway, waterTop + 2f, 26f * scale, 7f, col);
                    Dot(x + 12f + sway, waterTop + 3f, 20f * scale, 6f, col * 0.9f);
                    if (e.Health > 40) Dot(x + sway, waterTop - 2f, 10f, 8f, new Color(0.96f, 0.96f, 0.92f));
                }
                else
                    for (int i = 0; i < 9; i++)   // su mercimeği
                        Dot(x + (i - 4) * 9f * scale + Mathf.Sin(t * 0.5f + i) * 3f, waterTop + 2f + (i % 2), 7f, 4f, col * (0.85f + 0.05f * (i % 3)));
                return;
            }
            bool low = s.Has("shade") && !e.Item.Contains("fern") && !e.Item.Contains("hornwort");
            float h0 = tankH * (low ? 0.10f : 0.30f) * scale * (0.5f + 0.5f * wilt);
            int leaves = low ? 6 : 7;
            for (int i = 0; i < leaves; i++)
            {
                float f = i / (float)(leaves - 1);
                float lx = x + (low ? (f - 0.5f) * 46f * scale : sway * f + (i % 2 == 0 ? -1f : 1f) * 9f * scale);
                float ly = ground - (low ? 6f + (i % 3) * 5f : h0 * f);
                Dot(lx, ly, low ? 22f * scale : 18f * scale, low ? 14f * scale : 9f * scale, col * (0.85f + 0.15f * (i % 2)));
            }
            if (!low) Fill(new Rect(x - 1f + sway * 0.3f, ground - h0, 2f, h0), col * 0.7f);
            if (s.Has("flower") && e.Health > 40)
                Dot(x + sway, ground - h0 - 4f, 12f * scale, 12f * scale, s.Has("sun") ? new Color(0.85f, 0.65f, 0.85f) : new Color(0.62f, 0.45f, 0.85f));
        }

        private void DrawDecor(VivEntry e, VivDef s, float x, float ground)
        {
            var art = Art(e.Item);
            if (art != null)
            {
                float w = art.texture.width * 2f, h = art.texture.height * 2f;
                GUI.DrawTexture(new Rect(x - w / 2f, ground + 6f - h, w, h), art.texture);
                return;
            }
            var c = Hex(s.Color, Color.gray);
            string id = e.Item;
            if (id.Contains("pebble"))
                for (int i = 0; i < 6; i++) Dot(x + (i - 2.5f) * 7f, ground + 1f - (i % 2) * 3f, 9f, 7f, Color.Lerp(c, i % 3 == 0 ? new Color(0.75f, 0.55f, 0.45f) : new Color(0.55f, 0.6f, 0.65f), 0.4f));
            else if (id.Contains("mossy_stone")) { Dot(x, ground - 8f, 50f, 28f, c); Dot(x - 3f, ground - 19f, 40f, 10f, new Color(0.35f, 0.55f, 0.28f)); }
            else if (id.Contains("flat_stone")) { Dot(x, ground - 4f, 66f, 16f, c); Dot(x - 8f, ground - 8f, 36f, 4f, c * 1.15f); }
            else if (id.Contains("bark")) { Dot(x, ground - 7f, 56f, 18f, c); Dot(x, ground - 3f, 44f, 8f, new Color(0.12f, 0.08f, 0.05f, 0.9f)); for (int i = 0; i < 4; i++) Fill(new Rect(x - 20f + i * 12f, ground - 14f, 2f, 6f), c * 0.7f); }
            else if (id.Contains("driftwood")) for (int i = 0; i < 8; i++) Dot(x - 30f + i * 8.5f, ground - 4f - i * 2.2f, 14f, 8f, c * (0.9f + (i % 2) * 0.1f));
            else if (id.Contains("pot_shard")) { Dot(x, ground - 9f, 38f, 24f, c); Dot(x, ground - 4f, 28f, 14f, new Color(0.15f, 0.08f, 0.05f, 0.85f)); }
            else if (id.Contains("coral"))
            {
                for (int i = 0; i < 5; i++)
                {
                    float bx = x + (i - 2) * 7f, bh = 22f + (i % 3) * 10f;
                    Fill(new Rect(bx, ground - bh, 3f, bh), c);
                    Dot(bx + 1f, ground - bh, 7f, 7f, c * 1.1f);
                }
            }
            else if (id.Contains("whelk") || id.Contains("snail_shell")) { Dot(x, ground - 7f, 22f, 16f, c); Dot(x + 5f, ground - 9f, 10f, 9f, c * 0.8f); Dot(x - 6f, ground - 4f, 6f, 6f, new Color(0.2f, 0.15f, 0.1f, 0.8f)); }
            else if (id.Contains("mussel")) { Dot(x, ground - 4f, 26f, 11f, c); Dot(x, ground - 6f, 18f, 3f, new Color(0.75f, 0.8f, 0.85f, 0.7f)); }
            else Dot(x, ground - 6f, 30f, 16f, c);
        }

        private Mote Spawn(VivDef s, Rect r, float ground, float waterTop, bool water)
        {
            var m = new Mote { X = r.x + 20f + (float)_rng.NextDouble() * (r.width - 40f), Phase = (float)_rng.NextDouble() * 6.28f };
            switch (s.Move)
            {
                case "fly": m.Y = r.y + r.height * (0.15f + 0.45f * (float)_rng.NextDouble()); break;
                case "swim": m.Y = Mathf.Lerp(waterTop + 10f, ground - 8f, (float)_rng.NextDouble()); break;
                case "static": m.X = r.xMax - 40f; m.Y = r.y + 30f; break;
                case "burrow": m.Y = ground + (r.yMax - ground) * 0.5f; break;
                case "bottom": m.Y = ground - 3f; break;
                default: m.Y = ground; break;
            }
            m.Vx = ((float)_rng.NextDouble() - 0.5f) * 30f;
            return m;
        }

        private float Rnd => (float)_rng.NextDouble();

        private void Step(Mote m, VivDef s, Rect r, float ground, float waterTop, bool water, List<float> hides, float dt, float now)
        {
            m.Timer -= dt;
            float minX = r.x + 12f, maxX = r.xMax - 12f;
            string move = s.Move;
            if (water && move == "hop") move = "swim";               // kurbağa suda yüzer
            bool ground_ = move == "crawl" || move == "bottom" || move == "burrow";
            // Yürüyen canlı ara sıra taşın, kabuğun ardına saklanır.
            if (ground_ && hides.Count > 0)
            {
                if (m.Hidden)
                {
                    if (m.Timer <= 0f) { m.Hidden = false; m.HideX = -1f; m.Timer = 1.5f + Rnd * 3f; m.Vx = (Rnd - 0.5f) * 30f; }
                    return;
                }
                if (m.HideX >= 0f)
                {
                    float dx = m.HideX - m.X;
                    if (Mathf.Abs(dx) < 4f) { m.Hidden = true; m.Timer = 2f + Rnd * 4f; return; }
                    m.Vx = Mathf.Sign(dx) * 26f; m.X += m.Vx * dt; m.Facing = Mathf.Sign(m.Vx);
                    m.Y = move == "bottom" ? ground - 3f : move == "burrow" ? ground : ground - 2f;
                    return;
                }
                if (m.Timer <= 0f && Rnd < 0.3f) { m.HideX = hides[_rng.Next(hides.Count)]; return; }
            }
            switch (move)
            {
                case "fly":
                    if (m.Timer <= 0f) { m.Vx = (Rnd - 0.5f) * 90f; m.Vy = (Rnd - 0.5f) * 60f; m.Timer = 0.6f + Rnd; }
                    m.X += m.Vx * dt; m.Y += (m.Vy + Mathf.Sin(now * 7f + m.Phase) * 25f) * dt;
                    m.Y = Mathf.Clamp(m.Y, (water ? r.y : r.y) + 16f, (water ? waterTop : ground) - 12f);
                    break;
                case "swim":
                {
                    bool tiny = s.Has("swarm");
                    if (m.Timer <= 0f) { m.Vx = (Rnd - 0.5f) * (tiny ? 30f : 70f); m.Vy = (Rnd - 0.5f) * (tiny ? 40f : 24f); m.Timer = (tiny ? 0.3f : 1f) + Rnd * 2f; }
                    m.X += m.Vx * dt; m.Y = Mathf.Clamp(m.Y + (m.Vy + (tiny ? Mathf.Sin(now * 9f + m.Phase) * 30f : 0f)) * dt, waterTop + 8f, ground - 6f);
                    break;
                }
                case "bottom":
                    if (m.Timer <= 0f) { m.Vx = Rnd < 0.35f ? 0f : (Rnd - 0.5f) * 50f; m.Timer = 1f + Rnd * 2.5f; }
                    m.X += m.Vx * dt; m.Y = ground - 3f + Mathf.Sin(now * 1.5f + m.Phase) * 1.5f;
                    break;
                case "hop":
                    if (!m.Air && m.Timer <= 0f) { m.Air = true; m.Vy = -140f; m.Vx = (Rnd - 0.5f) * 120f; m.Timer = 1.2f + Rnd * 2.5f; }
                    if (m.Air) { m.X += m.Vx * dt; m.Vy += 380f * dt; m.Y += m.Vy * dt; if (m.Y >= ground) { m.Y = ground; m.Air = false; } }
                    break;
                case "static":
                    m.Y = r.y + 30f + Mathf.Sin(now * 0.8f + m.Phase) * 3f;
                    break;
                case "burrow":
                    if (m.Timer <= 0f) { m.Vx = (Rnd - 0.5f) * 14f; m.Timer = 2f + Rnd * 3f; }
                    m.X += m.Vx * dt;
                    m.Alpha = 0.35f + 0.65f * Mathf.Clamp01(Mathf.Sin(now * 0.5f + m.Phase));
                    m.Y = Mathf.Lerp(ground + 4f, r.yMax - 8f, 1f - m.Alpha);
                    break;
                default:   // crawl
                    if (m.Timer <= 0f) { m.Vx = Rnd < 0.3f ? 0f : (Rnd - 0.5f) * (s.Has("cleaner") ? 12f : 40f); m.Timer = 1.5f + Rnd * 3f; }
                    m.X += m.Vx * dt; m.Y = ground - 2f;
                    break;
            }
            if (m.X < minX) { m.X = minX; m.Vx = Mathf.Abs(m.Vx); }
            if (m.X > maxX) { m.X = maxX; m.Vx = -Mathf.Abs(m.Vx); }
            if (Mathf.Abs(m.Vx) > 0.5f) m.Facing = Mathf.Sign(m.Vx);
        }

        private void DrawCreature(VivEntry e, VivDef s, Mote m, float now, float alpha)
        {
            var col = Hex(s.Color, Color.gray);
            if (e.Health < 40) col = Color.Lerp(col, Color.gray, 0.5f);
            float a = m.Alpha * alpha;
            col.a = a;
            var art = Art(e.Item);
            if (art != null)
            {
                float w = art.texture.width * 2f, h = art.texture.height * 2f;
                var p = GUI.color; GUI.color = new Color(1f, 1f, 1f, a * p.a);
                var rr = new Rect(m.X - w / 2f, s.Move == "swim" ? m.Y - h / 2f : m.Y - h, w, h);
                if (m.Facing < 0f) GUI.DrawTextureWithTexCoords(rr, art.texture, new Rect(1f, 0f, -1f, 1f));
                else GUI.DrawTexture(rr, art.texture);
                GUI.color = p;
                return;
            }
            string id = e.Item;
            bool fish = id.Contains("minnow") || id.Contains("stickleback") || id.Contains("loach") || id.Contains("goby") || id.Contains("shrimp");
            if (fish)
            {
                float len = id.Contains("shrimp") ? 12f : id.Contains("stickleback") ? 16f : 20f, ht = id.Contains("loach") || id.Contains("goby") ? 6f : 8f;
                float wag = Mathf.Sin(now * 10f + m.Phase) * 2f;
                Dot(m.X, m.Y, len, ht, col);
                Dot(m.X - m.Facing * (len * 0.55f), m.Y + wag, 8f, ht * 0.9f, col * 0.85f);
                Dot(m.X + m.Facing * len * 0.32f, m.Y - 1f, 3f, 3f, new Color(0.05f, 0.05f, 0.05f, a));
                if (id.Contains("stickleback")) for (int i = 0; i < 3; i++) Fill(new Rect(m.X - 4f + i * 4f, m.Y - ht / 2f - 3f, 1.5f, 3f), col * 0.6f);
                if (id.Contains("shrimp")) for (int i = 0; i < 2; i++) Fill(new Rect(m.X + m.Facing * 6f, m.Y - 3f + i * 2f, m.Facing * 8f, 1f), col);
                return;
            }
            if (id.Contains("water_flea")) { Dot(m.X, m.Y, 4f, 4f, col); return; }
            if (id.Contains("tadpole"))
            {
                Dot(m.X, m.Y, 9f, 7f, col);
                Dot(m.X - m.Facing * 7f, m.Y + Mathf.Sin(now * 14f + m.Phase) * 2f, 9f, 3f, col * 0.9f);
                return;
            }
            float size = s.Move == "fly" ? 7f : s.Move == "hop" ? 12f : s.Move == "static" ? 10f : 9f;
            if (id.Contains("frog") || id.Contains("lizard") || id.Contains("hermit")) size = 15f;
            if (id.Contains("aphid")) size = 5f;
            if (s.Move == "static")
            {
                for (int i = 0; i < 6; i++)   // ağ
                {
                    float an = i / 6f * 6.28f;
                    Fill(new Rect(m.X + Mathf.Cos(an) * 18f, m.Y + Mathf.Sin(an) * 18f, 1.5f, 1.5f), new Color(1f, 1f, 1f, 0.5f * a));
                }
                Fill(new Rect(m.X, m.Y - 30f, 1f, 26f), new Color(1f, 1f, 1f, 0.4f * a));
            }
            if (s.Move == "fly")
            {
                float flap = Mathf.Abs(Mathf.Sin(now * 22f + m.Phase));
                Dot(m.X - 4f, m.Y - 3f, 8f, 5f + 4f * flap, new Color(1f, 1f, 1f, 0.55f * a));
                Dot(m.X + 4f, m.Y - 3f, 8f, 5f + 4f * flap, new Color(1f, 1f, 1f, 0.55f * a));
                if (id.Contains("butterfly")) { Dot(m.X - 6f, m.Y - 2f, 12f, 10f * flap + 3f, col); Dot(m.X + 6f, m.Y - 2f, 12f, 10f * flap + 3f, col); }
            }
            float body = s.Move == "burrow" ? size * 2.2f : id.Contains("lizard") ? size * 2.4f : size * 1.4f;
            Dot(m.X, m.Y - size / 2f, body, size, col);
            Dot(m.X + m.Facing * body * 0.45f, m.Y - size * 0.55f, size * 0.6f, size * 0.6f, col * 0.7f);
            if (id.Contains("lizard")) Dot(m.X - m.Facing * body * 0.7f, m.Y - size * 0.3f, body * 0.6f, size * 0.35f, col * 0.9f);
            if (id.Contains("ladybird")) Dot(m.X, m.Y - size / 2f, 3f, 3f, new Color(0f, 0f, 0f, a));
            if (id.Contains("snail") || id.Contains("periwinkle") || id.Contains("hermit")) Dot(m.X - m.Facing * 2f, m.Y - size, size * 1.1f, size * 1.1f, col * 0.8f);
        }

        // ---------- yan sütun ----------

        /// <summary>Core'un cam bahçe metnini çözer: "item." argümanları o anki dildeki eşya adına çevrilir.</summary>
        private string L(LocText t) => VivText.Resolve(GameLocalization.Current, t, Name);
        private string L(LocText? t) => t.HasValue ? L(t.Value) : null;

        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string name, object value)[] args) => GameLocalization.T(key, args);

        private void DrawSide()
        {
            var v = _home.VivOf(_item);
            _scroll = GUILayout.BeginScrollView(_scroll);
            if (v.Habitat == null)
            {
                // Zemin: her biri doğadan getirilen malzemeyle kurulur (orman toprağı, göl ya da deniz kumu, sazlık çamuru, kavanozda su).
                GUILayout.Label(T("ui.viv.pick_ground"));
                foreach (var need in HomeLife.HabitatNeeds)
                {
                    GUILayout.Label(L(HomeLife.HabitatName(need.Habitat)) + (need.Water != null ? "  ·  " + T("ui.viv.plus_water", ("water", Name(need.Water)), ("have", _house.Count(need.Water))) : ""));
                    foreach (var sub in need.Substrates)
                    {
                        string why = L(_home.HabitatBlocker(_item, need.Habitat, sub));
                        var prevEnabled = GUI.enabled;
                        GUI.enabled = why == null;
                        if (GUILayout.Button("  " + T("ui.viv.ground_with", ("item", Name(sub)), ("have", _house.Count(sub)))))
                            _status = L(_home.SetHabitat(_item, need.Habitat, sub)) ?? T("ui.viv.ground_ready");
                        GUI.enabled = prevEnabled;
                    }
                }
                GUILayout.Label(T("ui.viv.ground_hint"));
            }
            else
            {
                bool water = HomeLife.IsWater(v.Habitat);
                string w = HomeLife.WaterFor(v.Habitat);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(water ? T("ui.viv.refresh_water", ("water", Name(w)), ("have", _house.Count(w))) : T("ui.viv.mist"))) _status = L(_home.Mist(_item)) ?? T("ui.viv.done");
                var watch = _item.Def.Interactions.Find(i => i.Id == "watch_viv");
                if (watch != null && GUILayout.Button(T("ui.viv.watch"))) Use?.Invoke(_item, watch.Id);
                if (v.Entries.Count == 0 && GUILayout.Button(T("ui.viv.empty"))) _status = L(_home.EmptyViv(_item)) ?? T("ui.viv.done");
                GUILayout.EndHorizontal();
                var (lo, hi) = HomeLife.MoistRange(v.Habitat);
                GUILayout.Label(T(water ? "ui.viv.freshness" : "ui.viv.moisture", ("value", Mathf.RoundToInt((float)v.Moisture)), ("lo", (int)lo), ("hi", (int)hi))
                    + (v.Mold >= 10 ? "  ·  " + T("ui.viv.mold", ("value", Mathf.RoundToInt((float)v.Mold))) : ""));
            }
            if (_status != null) GUILayout.Label(_status);

            GUILayout.Space(4f);
            GUILayout.Label(T("ui.viv.contents", ("plants", _home.CountPlants(v)), ("plantMax", _item.Def.VivPlants), ("creatures", _home.CountCreatures(v)),
                ("creatureMax", _item.Def.VivCreatures), ("decor", _home.CountDecor(v)), ("decorMax", _item.Def.VivDecor)));
            for (int i = 0; i < v.Entries.Count; i++)
            {
                var e = v.Entries[i];
                var s = V(e.Item);
                if (s == null) continue;
                GUILayout.BeginHorizontal();
                string state = s.IsDecor ? T("ui.viv.state_decor")
                    : s.IsPlant ? T("ui.viv.state_plant", ("health", Mathf.RoundToInt((float)e.Health)))
                    : T("ui.viv.state_creature", ("health", Mathf.RoundToInt((float)e.Health)), ("hunger", Mathf.RoundToInt((float)e.Hunger)));
                string need = s.IsCreature ? _home.UnmetNeed(v, s) : null;
                GUILayout.Label($"{Name(e.Item)} · {state}{(need != null ? " · " + T("ui.viv.wants", ("need", L(HomeLife.NeedName(need)))) : "")}");
                if (!s.IsCreature)
                {
                    if (GUILayout.Button("<", GUILayout.Width(24f))) _home.MoveInViv(_item, i, -0.06);
                    if (GUILayout.Button(">", GUILayout.Width(24f))) _home.MoveInViv(_item, i, 0.06);
                }
                if (GUILayout.Button(T("ui.viv.take"), GUILayout.Width(40f))) { _home.RemoveFromViv(_item, i, false); GUILayout.EndHorizontal(); break; }
                if (s.IsCreature && GUILayout.Button(T("ui.viv.release"), GUILayout.Width(40f))) { _home.RemoveFromViv(_item, i, true, _actor?.Invoke()); GUILayout.EndHorizontal(); break; }
                GUILayout.EndHorizontal();
            }

            if (v.Habitat != null)
            {
                int shown = 0;
                foreach (var (title, kind) in new[] { ("ui.viv.add_plants", 0), ("ui.viv.add_creatures", 1), ("ui.viv.add_decor", 2) })
                {
                    bool header = false;
                    foreach (var d in _content.Items.All)
                    {
                        if (d.Viv == null || _house.Count(d.Id) <= 0) continue;
                        int k = d.Viv.IsPlant ? 0 : d.Viv.IsDecor ? 2 : 1;
                        if (k != kind) continue;
                        if (!header) { GUILayout.Space(4f); GUILayout.Label(T(title)); header = true; }
                        shown++;
                        string why = L(_home.CanAddToViv(_item, d.Id));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{d.DisplayName} ×{_house.Count(d.Id)}");
                        var prevEnabled = GUI.enabled;
                        GUI.enabled = why == null;
                        if (GUILayout.Button(T("ui.viv.put"), GUILayout.Width(48f)))
                        {
                            var warn = _home.AddWarnings(_item, d.Id);
                            if (warn.Count > 0 && _pending != d.Id) { _pending = d.Id; _status = string.Join(" ", warn.ConvertAll(L)) + " " + T("ui.viv.press_again"); }
                            else { _pending = null; _status = _home.AddToViv(_item, d.Id) ? T("ui.viv.added", ("item", d.DisplayName)) : T("ui.viv.not_added"); }
                        }
                        GUI.enabled = prevEnabled;
                        GUILayout.EndHorizontal();
                        if (why != null) GUILayout.Label("  " + why);
                    }
                }
                if (shown == 0) GUILayout.Label(T("ui.viv.none_in_inventory"));
            }

            if (v.Log.Count > 0)
            {
                GUILayout.Space(4f);
                GUILayout.Label(T("ui.viv.recent"));
                for (int i = v.Log.Count - 1; i >= 0; i--) GUILayout.Label("· " + L(v.Log[i]));
            }
            GUILayout.EndScrollView();
        }
    }
}
