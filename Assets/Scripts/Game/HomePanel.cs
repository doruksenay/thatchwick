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
    /// Ev paneli (E, geçici IMGUI; Project belgesi 45): odalar (tür, ışık, is, güzellik), oda planları ve mevsim setleri,
    /// hizmetkâr, desen ve arma çizimi, koleksiyonlar, ev yarışması. Kurallar Core.Life.HomeLife'ta; panel gösterir ve çağırır.
    /// </summary>
    public sealed class HomePanel
    {
        private readonly HomeLife _home;
        private readonly LifeSystems _life;
        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly Func<Character> _actor;
        private Vector2 _scroll;
        private int _tab;
        private string _status;
        private string _planName = "";
        private string _patternName = "Desen";
        private char[] _cells = new string('0', 64).ToCharArray();
        private string _editingId;
        private int _brush = 1;
        private ArmaDef _arma = new ArmaDef { Field = 1, Division = "duz", Second = 2, Charge = "yildiz", ChargeColor = 2 };
        private static Texture2D _white;

        private static readonly string[] Tabs = { "Odalar", "Planlar", "Hizmetkâr", "Desen", "Arma", "Koleksiyon", "Yarışma" };

        public bool Active { get; private set; }
        public event Action<string> Message;

        public HomePanel(LifeSystems life, WorldState world, ContentDatabase content, Household house, Func<Character> actor)
        {
            _life = life; _home = life.Home; _world = world; _content = content; _house = house; _actor = actor;
        }

        public void Toggle() => Active = !Active;
        public void Close() => Active = false;

        public Rect PanelRect(float guiScale) => new Rect(Screen.width / guiScale - 418f, 8f, 410f, Screen.height / guiScale - 16f);

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        private Room CurrentRoom()
        {
            var c = _actor?.Invoke();
            var room = c != null ? _world.Map.Rooms.RoomAt(c.Position) : null;
            return room != null && room.IsIndoor ? room : null;
        }

        private IEnumerable<Room> HouseRooms()
        {
            foreach (var r in _world.Map.Rooms.Rooms)
                if (r.IsIndoor && r.Tiles.Count > 0 && _world.Map.IsOwned(r.Tiles[0])) yield return r;
        }

        public void OnGUI(float guiScale)
        {
            if (!Active || _home == null) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("EV");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (i == 4) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                if (GUILayout.Toggle(_tab == i, Tabs[i], GUI.skin.button)) _tab = i;
            }
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_status)) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0: Rooms(); break;
                case 1: Plans(); break;
                case 2: Servant(); break;
                case 3: Pattern(); break;
                case 4: Arma(); break;
                case 5: Collections(); break;
                case 6: Contest(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void Say(string s) { _status = s; Message?.Invoke(s); }

        // ---- odalar ve mevsim setleri ----

        private void Rooms()
        {
            int n = 0;
            foreach (var room in HouseRooms())
            {
                n++;
                GUILayout.Label($"— {_home.RoomSummary(room)} ({room.Area} karo, kat {room.Level}) —");
                var beauty = _life.Mood.BeautyOf(room);
                GUILayout.Label($"Güzellik: {beauty.Breakdown()}");
                var (kind, tidy) = _home.Classify(room);
                if (tidy && HomeLife.TidyName(kind) != null) GUILayout.Label(HomeLife.TidyName(kind));
                var hint = _home.TidyHint(room);
                if (hint != null) GUILayout.Label("İpucu: " + hint);
                if (_home.SootOf(room) >= HomeLife.SootWarnAt) GUILayout.Label("Duvarlar isli: ocakta \"Badana\" (2 kireç); davlumbaz ve baca isi azaltır.");
            }
            if (n == 0) GUILayout.Label("Henüz kapalı bir oda yok.");

            GUILayout.Space(6);
            GUILayout.Label("— Oda planı —");
            var cur = CurrentRoom();
            GUILayout.Label(cur != null ? "Seçili karakterin bulunduğu oda kaydedilir." : "Kaydetmek için seçili karakter bir odanın içinde olmalı.");
            GUILayout.BeginHorizontal();
            _planName = GUILayout.TextField(_planName, 24, GUILayout.Width(200f));
            GUI.enabled = cur != null;
            if (GUILayout.Button("Planı kaydet")) { var why = _home.SavePlan(cur, _planName); Say(why ?? $"Plan kaydedildi ({_home.Plans.Count}/{HomeLife.MaxPlans})."); }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("— Mevsim setleri —");
            GUILayout.Label("Şu an asılı mevsim süslerini o mevsimin seti olarak kaydet. Mevsim dönünce solanlar kalkar, set envanterden asılır.");
            _home.AutoSeasonSwap = GUILayout.Toggle(_home.AutoSeasonSwap, " Mevsim dönünce seti kendiliğinden as");
            foreach (Season s in Enum.GetValues(typeof(Season)))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{SeasonName(s)}: {(_home.HasSeasonSet(s) ? _home.SeasonSetSize(s) + " süs" : "yok")}", GUILayout.Width(150f));
                if (GUILayout.Button("Kaydet")) Say($"{SeasonName(s)} seti: {_home.SaveSeasonSet(s)} süs kaydedildi.");
                if (_home.HasSeasonSet(s) && GUILayout.Button("Sil")) { _home.ClearSeasonSet(s); Say($"{SeasonName(s)} seti silindi."); }
                GUILayout.EndHorizontal();
            }
        }

        private static string SeasonName(Season s) => s == Season.Spring ? "İlkbahar" : s == Season.Summer ? "Yaz" : s == Season.Autumn ? "Sonbahar" : "Kış";

        private void Plans()
        {
            if (_home.Plans.Count == 0) { GUILayout.Label("Kayıtlı plan yok. Odalar sekmesinden bir odayı plan olarak kaydet."); return; }
            var target = CurrentRoom();
            GUILayout.Label(target != null ? $"Hedef oda: {_home.RoomSummary(target)}" : "Uygulamak için seçili karakter hedef odanın içinde olmalı.");
            for (int i = 0; i < _home.Plans.Count; i++)
            {
                var plan = _home.Plans[i];
                var (missing, cost) = _home.PlanNeeds(plan);
                GUILayout.Label($"• {plan.Name} ({plan.Width}x{plan.Height}, {plan.Entries.Count} eşya)" + (missing.Count > 0 ? $" · eksik {missing.Count} çeşit, {cost} akçe" : " · hepsi envanterde"));
                GUILayout.BeginHorizontal();
                GUI.enabled = target != null;
                if (GUILayout.Button("Envanterden")) Say(_home.ApplyPlan(plan, target, buy: false, order: false).Text);
                if (GUILayout.Button("Eksikleri satın al")) Say(_home.ApplyPlan(plan, target, buy: true, order: false).Text);
                if (GUILayout.Button("Sipariş et")) Say(_home.ApplyPlan(plan, target, buy: false, order: true).Text);
                GUI.enabled = true;
                if (GUILayout.Button("Sil", GUILayout.Width(40f))) { _home.DeletePlan(i); Say("Plan silindi."); break; }
                GUILayout.EndHorizontal();
            }
        }

        // ---- hizmetkâr ----

        private void Servant()
        {
            var s = _home.Servant;
            if (s != null)
            {
                GUILayout.Label($"{s.Name} ({AgeStages.Name(s.Stage).ToLowerInvariant()}) · günde {_home.ServantWage} akçe · sadakat {_home.ServantLoyalty:0} · {_home.ServantDays} gündür");
                GUILayout.Label("Sabahları gelir; mum, sepet, döküntü, ayna, saksı, ot serpme, badana ve solan süs işlerini görür. Sadakat arttıkça daha çok iş yapar.");
                if (_home.LastServantReport != null) GUILayout.Label(_home.LastServantReport);
                if (GUILayout.Button("Hizmetten çıkar")) { _home.Dismiss(); Say("Hizmetkâr ayrıldı."); }
                return;
            }
            GUILayout.Label("Köyün ergen ve genç yetişkinleri hizmete girer (ortaçağda evlenmeden önce başka hanede hizmet yaygındı). Aynı evde günler geçtikçe aileyle yakınlaşır; bekâr genç yetişkin talip de olabilir.");
            var list = _home.ServantCandidates();
            if (list.Count == 0) GUILayout.Label("Şu an hizmete girecek kimse yok.");
            foreach (var c in list)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{c.Name} · {AgeStages.Name(c.Stage).ToLowerInvariant()} · günde {HomeLife.WageFor(c)} akçe", GUILayout.Width(280f));
                if (GUILayout.Button("Hizmete al")) Say(_home.Hire(c) ?? $"{c.Name} hizmete girdi.");
                GUILayout.EndHorizontal();
            }
        }

        // ---- desen ----

        private static Texture2D White
        {
            get
            {
                if (_white == null) { _white = new Texture2D(1, 1); _white.SetPixel(0, 0, Color.white); _white.Apply(); }
                return _white;
            }
        }

        public static Color Hex(string hex)
        {
            if (hex == null || hex.Length != 7 || hex[0] != '#') return Color.magenta;
            int v = Convert.ToInt32(hex.Substring(1), 16);
            return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, 1f);
        }

        /// <summary>8x8 hücreyi verilen dikdörtgene çizer.</summary>
        public static void DrawCells(Rect r, string cells)
        {
            if (cells == null || cells.Length != 64) return;
            float cw = r.width / 8f, ch = r.height / 8f;
            var old = GUI.color;
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int idx = cells[y * 8 + x] - '0';
                    GUI.color = Hex(HomeLife.Palette[Mathf.Clamp(idx, 0, HomeLife.Palette.Length - 1)].Hex);
                    GUI.DrawTexture(new Rect(r.x + x * cw, r.y + y * ch, cw - 1, ch - 1), White);
                }
            GUI.color = old;
        }

        private void Pattern()
        {
            var why = _home.DesignBlocker();
            if (why != null) GUILayout.Label(why);
            GUILayout.Label("Fırça rengi:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < HomeLife.Palette.Length; i++)
            {
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Hex(HomeLife.Palette[i].Hex);
                if (GUILayout.Toggle(_brush == i, _brush == i ? "●" : " ", GUI.skin.button, GUILayout.Width(36f))) _brush = i;
                GUI.backgroundColor = old;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(HomeLife.Palette[_brush].Name + (HomeLife.Palette[_brush].Dyes.Length == 0 ? " (bedava)" : $" ({string.Join(" + ", Array.ConvertAll(HomeLife.Palette[_brush].Dyes, Name))})"));

            // Hücreler: tıkla, boya.
            const float size = 22f;
            var area = GUILayoutUtility.GetRect(size * 8, size * 8, GUILayout.Width(size * 8), GUILayout.Height(size * 8));
            var e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && area.Contains(e.mousePosition))
            {
                int x = Mathf.Clamp((int)((e.mousePosition.x - area.x) / size), 0, 7);
                int y = Mathf.Clamp((int)((e.mousePosition.y - area.y) / size), 0, 7);
                _cells[y * 8 + x] = (char)('0' + _brush);
                e.Use();
            }
            DrawCells(area, new string(_cells));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Temizle")) _cells = new string('0', 64).ToCharArray();
            if (GUILayout.Button("Ayna (sol → sağ)")) for (int y = 0; y < 8; y++) for (int x = 0; x < 4; x++) _cells[y * 8 + 7 - x] = _cells[y * 8 + x];
            GUILayout.EndHorizontal();
            var cost = HomeLife.PatternCost(new string(_cells));
            var parts = new List<string>();
            foreach (var kv in cost) parts.Add($"{kv.Value} {Name(kv.Key)} ({_house.Count(kv.Key)} var)");
            GUILayout.Label("İşleme bedeli: " + (parts.Count == 0 ? "boya yok" : string.Join(", ", parts)));
            GUILayout.BeginHorizontal();
            _patternName = GUILayout.TextField(_patternName, 20, GUILayout.Width(160f));
            if (GUILayout.Button(_editingId != null ? "Güncelle" : "Kaydet"))
            {
                var id = _home.SavePattern(_patternName, new string(_cells), _editingId);
                Say(id != null ? $"\"{_patternName}\" kaydedildi." : _home.LastPatternProblem);
                if (id != null) _editingId = id;
            }
            if (_editingId != null && GUILayout.Button("Yeni")) { _editingId = null; _patternName = "Desen"; }
            GUILayout.EndHorizontal();

            GUILayout.Label($"— Kitaplık ({_home.Patterns.Count}/{HomeLife.MaxPatterns}) —");
            foreach (var p in new List<PatternDef>(_home.Patterns))
            {
                GUILayout.BeginHorizontal();
                var r = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32), GUILayout.Height(32));
                DrawCells(r, p.Cells);
                GUILayout.Label(p.Name, GUILayout.Width(120f));
                if (GUILayout.Button("Aç")) { _cells = p.Cells.ToCharArray(); _patternName = p.Name; _editingId = p.Id; }
                if (GUILayout.Button("Sil")) { _home.DeletePattern(p.Id); if (_editingId == p.Id) _editingId = null; }
                GUILayout.EndHorizontal();
            }

            ApplyList(_editingId, "Açık deseni işle");
        }

        // Evdeki desen alan eşyalar: seçili deseni işle.
        private void ApplyList(string patternId, string title)
        {
            if (patternId == null) return;
            GUILayout.Label($"— {title} —");
            int n = 0;
            foreach (var it in _world.Items)
            {
                if (!it.Def.HasTag(HomeLife.PatternableTag) || !_world.Map.IsOwned(it.Position)) continue;
                if (patternId == HomeLife.ArmaPatternId && !it.Def.HasTag(HomeLife.HeraldicTag)) continue;
                n++;
                var on = _home.PatternOn(it);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{it.Def.DisplayName}" + (on != null ? $" ({on.Name})" : ""), GUILayout.Width(220f));
                var block = _home.ApplyPatternBlocker(it, patternId);
                GUI.enabled = block == null;
                if (GUILayout.Button("İşle")) { var item = it; if (_home.ApplyPattern(item, patternId)) Say($"{item.Def.DisplayName} işlendi."); }
                GUI.enabled = true;
                if (on != null && GUILayout.Button("Sök", GUILayout.Width(40f))) _home.ClearPattern(it);
                GUILayout.EndHorizontal();
                if (block != null) GUILayout.Label("  " + block);
            }
            if (n == 0) GUILayout.Label(patternId == HomeLife.ArmaPatternId ? "Evde arma işlenecek eşya yok (hanedan sancağı, armalı kalkan, armalı halı)." : "Evde desen işlenecek eşya yok (kilim, halı, örtü, pano, perde).");
        }

        // ---- arma ----

        private void Arma()
        {
            GUILayout.Label("Hanedan arması: alan rengi, bölünüş, ikinci renk, şekil. Kural: renk üstüne renk, maden üstüne maden konmaz (maden: sarı, doğal yün).");
            _arma.Field = ColorRow("Alan", _arma.Field);
            GUILayout.Label("Bölünüş:");
            GUILayout.BeginHorizontal();
            foreach (var d in HomeLife.Divisions) if (GUILayout.Toggle(_arma.Division == d.id, d.name, GUI.skin.button)) _arma.Division = d.id;
            GUILayout.EndHorizontal();
            if (_arma.Division != "duz") _arma.Second = ColorRow("İkinci renk", _arma.Second);
            GUILayout.Label("Şekil:");
            GUILayout.BeginHorizontal();
            int k = 0;
            foreach (var c in HomeLife.Charges)
            {
                if (k++ == 6) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                if (GUILayout.Toggle(_arma.Charge == c.id, c.name, GUI.skin.button)) _arma.Charge = c.id;
            }
            GUILayout.EndHorizontal();
            if (_arma.Charge != "yok") _arma.ChargeColor = ColorRow("Şekil rengi", _arma.ChargeColor);

            var r = GUILayoutUtility.GetRect(128, 128, GUILayout.Width(128), GUILayout.Height(128));
            DrawCells(r, HomeLife.ArmaCells(_arma));
            var rule = HomeLife.ArmaRule(_arma);
            GUILayout.Label(rule ?? "Kurala uygun.");
            GUI.enabled = rule == null;
            if (GUILayout.Button("Hanedan arması yap")) Say(_home.SetArma(new ArmaDef { Field = _arma.Field, Division = _arma.Division, Second = _arma.Second, Charge = _arma.Charge, ChargeColor = _arma.ChargeColor }) ?? "Arma kaydedildi.");
            GUI.enabled = true;
            if (_home.Arma != null)
            {
                GUILayout.Label("Kayıtlı arma:");
                var r2 = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
                DrawCells(r2, HomeLife.ArmaCells(_home.Arma));
                ApplyList(HomeLife.ArmaPatternId, "Armayı işle");
            }
        }

        private int ColorRow(string label, int current)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(90f));
            for (int i = 0; i < HomeLife.Palette.Length; i++)
            {
                if (HomeLife.Palette[i].Tincture == null) continue;
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Hex(HomeLife.Palette[i].Hex);
                if (GUILayout.Toggle(current == i, current == i ? "●" : " ", GUI.skin.button, GUILayout.Width(32f))) current = i;
                GUI.backgroundColor = old;
            }
            GUILayout.EndHorizontal();
            return current;
        }

        // ---- koleksiyon ----

        private void Collections()
        {
            GUILayout.Label("Sergi rafı ve sergi dolabı her eşyadan bir tane alır. Bir rafta tam set: güzellik +4; tamamlanan set köyde konuşulur (itibar).");
            var shown = _home.Displayed();
            foreach (var c in _home.Collections)
            {
                var (have, total) = _home.Progress(c);
                GUILayout.Label($"— {c.Name}: {have}/{total}{(_home.IsCollectionDone(c.Id) ? " · tamamlandı" : "")} —");
                var miss = new List<string>();
                foreach (var id in c.Items) if (!shown.Contains(id)) miss.Add(Name(id) + (_house.Count(id) > 0 ? " (envanterde)" : ""));
                if (miss.Count > 0) GUILayout.Label("Eksik: " + string.Join(", ", miss));
            }
        }

        // ---- yarışma ----

        private void Contest()
        {
            GUILayout.Label("Yaz Panayırı'nda köy büyükleri evleri gezer. Puan: en güzel üç oda + bahçe süsü (en çok 20) + koleksiyon ×3 + desenli eşya (en çok 5) + arma 2.");
            GUILayout.Label($"Şu anki puan: {_home.HomeScore()} (bahçe {_home.GardenBeauty()}) · kazanılan kurdele: {_home.ContestWins}");
            if (_home.LastContest != null) GUILayout.Label("Son yarışma: " + _home.LastContest.Text);
        }
    }
}
