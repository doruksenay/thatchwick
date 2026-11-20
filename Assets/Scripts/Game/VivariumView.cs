using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Cam bahçe yakın görünümü (Project belgesi 47; geçici IMGUI). Cam bahçeye dokununca kamera ona kayıp yakınlaşır,
    /// ardından büyük ekranda içi açılır: bitkiler sallanır, böcekler türüne göre yürür, zıplar, uçar, yüzer, toprağa girer;
    /// gece ateş böcekleri ışır. Sağda içindekiler, envanterden ekleme (kurulamayan kombinasyon nedeniyle), nem ve günlük.
    /// Kurallar Core.Life.HomeLife (HomeVivarium.cs). Türlerin görseli gelene kadar renkli şekillerle çizilir
    /// (IsoArt "viv_*" görseli varsa o kullanılır; art/queue 455-458).
    /// </summary>
    public sealed class VivariumView
    {
        private sealed class Mote
        {
            public float X, Y, Vx, Vy, Phase, Timer, Alpha = 1f, Facing = 1f;
            public bool Air;     // zıplarken havada
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
            GUI.Label(new Rect(rect.x + 12f, rect.y + 6f, tank.width, 24f), _item.Def.DisplayName.ToUpperInvariant() + "  ·  " + (_home.VivSummary(_item) ?? ""));
            if (GUI.Button(new Rect(rect.xMax - 70f, rect.y + 6f, 60f, 24f), "Kapat")) { Close(); GUI.color = prev; return; }
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

        private void DrawTank(Rect r)
        {
            EnsureTextures();
            var v = _home.VivOf(_item);
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f), now = Time.realtimeSinceStartup;
            bool night = HomeLife.IsNight(_clock.DayFraction);
            string hab = v.Habitat;
            float ground = r.yMax - r.height * (hab == HomeLife.Salt ? 0.12f : 0.2f);

            // arka plan (görsel varsa o)
            var bg = IsoArt.Get("viv_bg_" + (hab == HomeLife.Dry ? "dry" : hab == HomeLife.Salt ? "salt" : "moist"), 0f, 0f);
            if (hab != null && bg != null) GUI.DrawTexture(r, bg.texture);
            else
            {
                Fill(r, new Color(0.80f, 0.86f, 0.80f, 0.95f));                                // camın ardındaki oda ışığı
                if (hab == HomeLife.Salt) Fill(new Rect(r.x, r.y + r.height * 0.14f, r.width, r.height * 0.86f), new Color(0.35f, 0.55f, 0.62f, 0.85f));
                if (hab != null)
                {
                    var soil = hab == HomeLife.Dry ? new Color(0.82f, 0.72f, 0.50f) : hab == HomeLife.Salt ? new Color(0.76f, 0.70f, 0.56f) : new Color(0.33f, 0.24f, 0.16f);
                    Fill(new Rect(r.x, ground, r.width, r.yMax - ground), soil);
                    for (int i = 0; i < 26; i++)                                                 // çakıl ve toprak tanesi
                    {
                        float gx = r.x + (i * 37 % 100) / 100f * r.width, gy = ground + 6f + (i * 53 % 100) / 100f * (r.yMax - ground - 10f);
                        Dot(gx, gy, 6f + i % 3 * 3f, 4f + i % 2 * 2f, soil * (0.7f + (i % 4) * 0.1f));
                    }
                }
                else GUI.Label(new Rect(r.x + 20f, r.center.y - 12f, r.width - 40f, 48f), "Boş kap. Sağdan zemini seç: nemli toprak, kuru kum ya da tuzlu su.");
            }

            // bitkiler: tabanda eşit aralıklı
            int plants = 0;
            foreach (var e in v.Entries) if (V(e.Item)?.IsPlant == true) plants++;
            int pi = 0;
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsPlant) continue;
                float px = r.x + r.width * (pi + 1f) / (plants + 1f);
                DrawPlant(e, s, px, ground, r.height, now + pi * 1.7f);
                pi++;
            }

            // canlılar
            var seen = new HashSet<VivEntry>();
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || s.IsPlant) continue;
                seen.Add(e);
                if (!_motes.TryGetValue(e, out var m)) { m = Spawn(s, r, ground); _motes[e] = m; }
                Step(m, s, r, ground, hab, dt);
                if (!(night && s.Has("glow"))) DrawCreature(e, s, m, now);
            }
            var drop = new List<VivEntry>();
            foreach (var k in _motes.Keys) if (!seen.Contains(k)) drop.Add(k);
            foreach (var k in drop) _motes.Remove(k);

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
            // cam: çerçeve ve parıltı
            Fill(new Rect(r.x, r.y, r.width, 3f), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.x, r.yMax - 3f, r.width, 3f), new Color(0.40f, 0.30f, 0.20f));
            Fill(new Rect(r.x, r.y, 3f, r.height), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.xMax - 3f, r.y, 3f, r.height), new Color(0.55f, 0.66f, 0.62f));
            Fill(new Rect(r.x + r.width * 0.08f, r.y + 10f, 6f, r.height * 0.55f), new Color(1f, 1f, 1f, 0.18f));
            if (v.Habitat != null && v.Habitat != HomeLife.Salt && v.Moisture > HomeLife.MoistRange(v.Habitat).hi - 10)
                for (int i = 0; i < 18; i++) Dot(r.x + (i * 43 % 100) / 100f * r.width, r.y + (i * 71 % 100) / 100f * r.height * 0.7f, 4f, 5f, new Color(1f, 1f, 1f, 0.35f));
        }

        private void DrawPlant(VivEntry e, VivDef s, float x, float ground, float tankH, float t)
        {
            var art = IsoArt.Get("viv_" + e.Item.Replace("item.viv_", ""), 0f, 0f);
            float scale = 0.6f + 0.2f * Mathf.Min(3f, (float)e.Growth);
            float sway = Mathf.Sin(t * (s.Habitats.Contains(HomeLife.Salt) ? 1.2f : 0.7f)) * 4f;
            float wilt = Mathf.Clamp01((float)e.Health / 100f);
            var col = Color.Lerp(new Color(0.55f, 0.45f, 0.25f), Hex(s.Color, new Color(0.3f, 0.6f, 0.3f)), wilt);
            if (art != null)
            {
                float w = art.texture.width * 2f * scale, h = art.texture.height * 2f * scale;
                var p = GUI.color; GUI.color = new Color(col.r * 0.4f + 0.6f, col.g * 0.4f + 0.6f, col.b * 0.4f + 0.6f, p.a);
                GUI.DrawTexture(new Rect(x - w / 2f + sway * 0.5f, ground - h + 4f, w, h), art.texture);
                GUI.color = p;
                return;
            }
            bool low = s.Has("shade") && !e.Item.Contains("fern");
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

        private Mote Spawn(VivDef s, Rect r, float ground)
        {
            var m = new Mote { X = r.x + 20f + (float)_rng.NextDouble() * (r.width - 40f), Phase = (float)_rng.NextDouble() * 6.28f };
            switch (s.Move)
            {
                case "fly": m.Y = r.y + r.height * (0.15f + 0.45f * (float)_rng.NextDouble()); break;
                case "swim": m.Y = r.y + r.height * (0.3f + 0.4f * (float)_rng.NextDouble()); break;
                case "static": m.X = r.xMax - 40f; m.Y = r.y + 30f; break;
                case "burrow": m.Y = ground + (r.yMax - ground) * 0.5f; break;
                default: m.Y = ground; break;
            }
            m.Vx = ((float)_rng.NextDouble() - 0.5f) * 30f;
            return m;
        }

        private void Step(Mote m, VivDef s, Rect r, float ground, string hab, float dt)
        {
            m.Timer -= dt;
            float minX = r.x + 12f, maxX = r.xMax - 12f;
            switch (s.Move)
            {
                case "fly":
                    if (m.Timer <= 0f) { m.Vx = ((float)_rng.NextDouble() - 0.5f) * 90f; m.Vy = ((float)_rng.NextDouble() - 0.5f) * 60f; m.Timer = 0.6f + (float)_rng.NextDouble(); }
                    m.X += m.Vx * dt; m.Y += (m.Vy + Mathf.Sin(Time.realtimeSinceStartup * 7f + m.Phase) * 25f) * dt;
                    m.Y = Mathf.Clamp(m.Y, r.y + 16f, ground - 12f);
                    break;
                case "swim":
                    if (m.Timer <= 0f) { m.Vx = ((float)_rng.NextDouble() - 0.5f) * 50f; m.Vy = ((float)_rng.NextDouble() - 0.5f) * 20f; m.Timer = 1f + (float)_rng.NextDouble() * 2f; }
                    m.X += m.Vx * dt; m.Y = Mathf.Clamp(m.Y + m.Vy * dt, r.y + r.height * 0.2f, ground - 8f);
                    break;
                case "hop":
                    if (!m.Air && m.Timer <= 0f) { m.Air = true; m.Vy = -140f; m.Vx = ((float)_rng.NextDouble() - 0.5f) * 120f; m.Timer = 1.2f + (float)_rng.NextDouble() * 2.5f; }
                    if (m.Air) { m.X += m.Vx * dt; m.Vy += 380f * dt; m.Y += m.Vy * dt; if (m.Y >= ground) { m.Y = ground; m.Air = false; } }
                    break;
                case "static":
                    m.Y = r.y + 30f + Mathf.Sin(Time.realtimeSinceStartup * 0.8f + m.Phase) * 3f;
                    break;
                case "burrow":
                    if (m.Timer <= 0f) { m.Vx = ((float)_rng.NextDouble() - 0.5f) * 14f; m.Timer = 2f + (float)_rng.NextDouble() * 3f; }
                    m.X += m.Vx * dt;
                    m.Alpha = 0.35f + 0.65f * Mathf.Clamp01(Mathf.Sin(Time.realtimeSinceStartup * 0.5f + m.Phase));
                    m.Y = Mathf.Lerp(ground + 4f, r.yMax - 8f, 1f - m.Alpha);
                    break;
                default:   // crawl
                    if (m.Timer <= 0f) { m.Vx = (_rng.NextDouble() < 0.3 ? 0f : ((float)_rng.NextDouble() - 0.5f) * (s.Habitats.Contains(HomeLife.Moist) && s.Has("cleaner") ? 12f : 40f)); m.Timer = 1.5f + (float)_rng.NextDouble() * 3f; }
                    m.X += m.Vx * dt; m.Y = ground - 2f;
                    break;
            }
            if (m.X < minX) { m.X = minX; m.Vx = Mathf.Abs(m.Vx); }
            if (m.X > maxX) { m.X = maxX; m.Vx = -Mathf.Abs(m.Vx); }
            if (Mathf.Abs(m.Vx) > 0.5f) m.Facing = Mathf.Sign(m.Vx);
        }

        private void DrawCreature(VivEntry e, VivDef s, Mote m, float now)
        {
            var col = Hex(s.Color, Color.gray);
            if (e.Health < 40) col = Color.Lerp(col, Color.gray, 0.5f);
            col.a = m.Alpha;
            var art = IsoArt.Get("viv_" + e.Item.Replace("item.viv_", ""), 0f, 0f);
            if (art != null)
            {
                float w = art.texture.width * 2f, h = art.texture.height * 2f;
                var p = GUI.color; GUI.color = new Color(1f, 1f, 1f, m.Alpha * p.a);
                var rr = new Rect(m.X - w / 2f, m.Y - h, w, h);
                if (m.Facing < 0f) GUI.DrawTextureWithTexCoords(rr, art.texture, new Rect(1f, 0f, -1f, 1f));
                else GUI.DrawTexture(rr, art.texture);
                GUI.color = p;
                return;
            }
            float size = s.Move == "fly" ? 7f : s.Move == "hop" ? 12f : s.Move == "static" ? 10f : 9f;
            if (e.Item.Contains("frog") || e.Item.Contains("lizard") || e.Item.Contains("hermit")) size = 15f;
            if (e.Item.Contains("aphid")) size = 5f;
            if (s.Move == "static")
            {
                for (int i = 0; i < 6; i++)   // ağ
                {
                    float a = i / 6f * 6.28f;
                    Fill(new Rect(m.X + Mathf.Cos(a) * 18f, m.Y + Mathf.Sin(a) * 18f, 1.5f, 1.5f), new Color(1f, 1f, 1f, 0.5f));
                }
                Fill(new Rect(m.X, m.Y - 30f, 1f, 26f), new Color(1f, 1f, 1f, 0.4f));
            }
            if (s.Move == "fly")
            {
                float flap = Mathf.Abs(Mathf.Sin(now * 22f + m.Phase));
                Dot(m.X - 4f, m.Y - 3f, 8f, 5f + 4f * flap, new Color(1f, 1f, 1f, 0.55f * m.Alpha));
                Dot(m.X + 4f, m.Y - 3f, 8f, 5f + 4f * flap, new Color(1f, 1f, 1f, 0.55f * m.Alpha));
                if (e.Item.Contains("butterfly")) { Dot(m.X - 6f, m.Y - 2f, 12f, 10f * flap + 3f, col); Dot(m.X + 6f, m.Y - 2f, 12f, 10f * flap + 3f, col); }
            }
            float body = s.Move == "burrow" ? size * 2.2f : size * 1.4f;
            Dot(m.X, m.Y - size / 2f, body, size, col);
            Dot(m.X + m.Facing * body * 0.45f, m.Y - size * 0.55f, size * 0.6f, size * 0.6f, col * 0.7f);
            if (e.Item.Contains("ladybird")) Dot(m.X, m.Y - size / 2f, 3f, 3f, Color.black);
            if (e.Item.Contains("snail") || e.Item.Contains("periwinkle")) Dot(m.X - m.Facing * 2f, m.Y - size, size * 1.1f, size * 1.1f, col * 0.8f);
        }

        // ---------- yan sütun ----------

        private void DrawSide()
        {
            var v = _home.VivOf(_item);
            _scroll = GUILayout.BeginScrollView(_scroll);
            if (v.Habitat == null)
            {
                GUILayout.Label("Zemin seç:");
                foreach (var h in HomeLife.Habitats)
                    if (GUILayout.Button(HomeLife.HabitatName(h) + (h == HomeLife.Salt ? " (1 tuz)" : ""))) _status = _home.SetHabitat(_item, h) ?? "Zemin hazır.";
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(v.Habitat == HomeLife.Salt ? "Suyu tazele (1 tuz)" : "Nemlendir")) _status = _home.Mist(_item) ?? "Tamam.";
                var watch = _item.Def.Interactions.Find(i => i.Id == "watch_viv");
                if (watch != null && GUILayout.Button("Seyret")) Use?.Invoke(_item, watch.Id);
                GUILayout.EndHorizontal();
                var (lo, hi) = HomeLife.MoistRange(v.Habitat);
                GUILayout.Label($"{(v.Habitat == HomeLife.Salt ? "Su tazeliği" : "Nem")}: {v.Moisture:0} (uygun {lo:0}-{hi:0}){(v.Mold >= 10 ? $" · küf {v.Mold:0}" : "")}");
            }
            if (_status != null) GUILayout.Label(_status);

            GUILayout.Space(4f);
            GUILayout.Label($"İÇİNDEKİLER ({_home.CountPlants(v)}/{_item.Def.VivPlants} bitki, {_home.CountCreatures(v)}/{_item.Def.VivCreatures} canlı)");
            for (int i = 0; i < v.Entries.Count; i++)
            {
                var e = v.Entries[i];
                var s = V(e.Item);
                GUILayout.BeginHorizontal();
                string state = s != null && s.IsPlant ? $"sağlık {e.Health:0}" : $"sağlık {e.Health:0}, açlık {e.Hunger:0}";
                GUILayout.Label($"{Name(e.Item)} · {state}");
                if (GUILayout.Button("Al", GUILayout.Width(34f))) { _home.RemoveFromViv(_item, i, false); GUILayout.EndHorizontal(); break; }
                if (s != null && !s.IsPlant && GUILayout.Button("Sal", GUILayout.Width(34f))) { _home.RemoveFromViv(_item, i, true, _actor?.Invoke()); GUILayout.EndHorizontal(); break; }
                GUILayout.EndHorizontal();
            }

            if (v.Habitat != null)
            {
                GUILayout.Space(4f);
                GUILayout.Label("ENVANTERDEN EKLE");
                int shown = 0;
                foreach (var d in _content.Items.All)
                {
                    if (d.Viv == null || _house.Count(d.Id) <= 0) continue;
                    shown++;
                    string why = _home.CanAddToViv(_item, d.Id);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{d.DisplayName} ×{_house.Count(d.Id)}");
                    var prevEnabled = GUI.enabled;
                    GUI.enabled = why == null;
                    if (GUILayout.Button("Koy", GUILayout.Width(44f)))
                    {
                        var warn = _home.AddWarnings(_item, d.Id);
                        if (warn.Count > 0 && _pending != d.Id) { _pending = d.Id; _status = string.Join(" ", warn) + " Yine de koymak için tekrar bas."; }
                        else { _pending = null; _status = _home.AddToViv(_item, d.Id) ? d.DisplayName + " kondu." : "Konamadı."; }
                    }
                    GUI.enabled = prevEnabled;
                    GUILayout.EndHorizontal();
                    if (why != null) GUILayout.Label("  " + why);
                }
                if (shown == 0) GUILayout.Label("Envanterde tür yok. Toplama yerlerinde (mantar, otluk, kuru dal, kum, su teresi) cam kavanozla yakala; deniz türleri pazarda.");
            }

            if (v.Log.Count > 0)
            {
                GUILayout.Space(4f);
                GUILayout.Label("SON OLAYLAR");
                for (int i = v.Log.Count - 1; i >= 0; i--) GUILayout.Label("· " + v.Log[i]);
            }
            GUILayout.EndScrollView();
        }
    }
}
