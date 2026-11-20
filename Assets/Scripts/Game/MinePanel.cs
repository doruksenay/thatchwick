using System;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mining;

namespace Thatchwick.Game
{
    /// <summary>
    /// Maden defteri (M, geçici IMGUI): hane madeninin galeri katı (-1). Galeri tepenin altındadır; karakter ağızdan iner ve
    /// işleri orada görerek yapar (belge 58d M1). Defter madene inmeden işaret koymak ve durumu görmek içindir: anında sonuç
    /// veren düğme yoktur. Maden görevli aile üyeleri her sabah işaretli karoları kazar (MineSystem.Shift).
    /// </summary>
    public sealed class MinePanel
    {
        private readonly MineSystem _mines;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private Vector2 _scroll;
        private string _status;
        private bool _fireMode;   // ateşle kırma kipi: dokunulan sert karoya karşı odun yakılır (belge 23)

        /// <summary>Panelden "kendin in" diyen karakter (komut alan); null ise düğme görünmez.</summary>
        public Func<Character> Actor { get; set; }
        /// <summary>Ocak açılabilir mi (Madencilik ve akçe); açılmışsa null.</summary>
        public Func<string> OpenBlocker { get; set; }
        public event Action<string> Message;
        /// <summary>Maden açıldı (oyun katmanı galeriyi dünyaya yerleştirir).</summary>
        public event Action Opened;
        /// <summary>"Madene in": komut alan karakter ağızdan iner.</summary>
        public event Action<Character> GoDown;

        public bool Active { get; private set; }

        public MinePanel(MineSystem mines, Household house, ContentDatabase content)
        {
            _mines = mines; _house = house; _content = content;
        }

        public void Toggle() => Active = !Active;
        public void Close() => Active = false;

        public const int OpenCostAkce = 30, OpenCostPlanks = 4;

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 418f, 8f, 410f, Screen.height / guiScale - 16f);

        private string Name(string id) => id != null && _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(GameLocalization.T("ui.mine.panel.title"));
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            var m = _mines.Get(MineSystem.MineId);
            if (m == null)
            {
                GUILayout.Label("Tepedeki eski galeri ağzı. Açmak için 30 akçe ve 4 tahta (giriş çatkısı) gerekir.");
                string why = OpenBlocker?.Invoke();
                if (why == null && _house.Akce < OpenCostAkce) why = "Akçe yetmiyor.";
                if (why == null && _house.Count("item.plank") < OpenCostPlanks) why = "4 tahta gerekir.";
                GUI.enabled = why == null;
                if (GUILayout.Button(GameLocalization.T("ui.mine.open")))
                {
                    _house.TrySpend(OpenCostAkce);
                    for (int i = 0; i < OpenCostPlanks; i++) _house.TryTakeItem("item.plank");
                    _mines.EnsureMine();
                    Opened?.Invoke();
                    Message?.Invoke(GameLocalization.T("ui.mine.opened"));
                }
                GUI.enabled = true;
                if (why != null) GUILayout.Label(why);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Su: derinlik {m.WaterDepth} (yarın {_mines.TomorrowDepth(m)}){(MineSystem.HasAdit(m) ? " · su galerisi akıyor" : " · batı yamaca bir galeri açarsan su kendiliğinden akar")}");
            if (m.SmokeDays > 0) GUILayout.Label("Galeri dumanlı.");
            GUILayout.Label("Dokun: işaretle / kaldır. Sarı işaretli, kırmızı çatırdıyor, mor damar izi, koyu sis.");
            _scroll = GUILayout.BeginScrollView(_scroll);
            const float cell = 15f;
            var area = GUILayoutUtility.GetRect(m.Width * cell, m.Height * cell);
            var prev = GUI.color;
            for (int y = 0; y < m.Height; y++)
                for (int x = 0; x < m.Width; x++)
                {
                    var t = m.Get(x, y);
                    bool seen = t.IsOpen || m.TouchesOpen(x, y);
                    GUI.color = t.IsOpen ? (t.Support != SupportKind.None ? new Color(0.75f, 0.6f, 0.4f) : t.LightDays > 0 ? new Color(1f, 0.9f, 0.6f) : new Color(0.55f, 0.5f, 0.45f))
                        : !seen ? new Color(0.18f, 0.16f, 0.15f)
                        : t.Creaking ? new Color(0.8f, 0.3f, 0.25f)
                        : t.Marked ? new Color(0.85f, 0.75f, 0.35f)
                        : t.Ore != null && m.VeinHint(x, y, 0) != null ? new Color(0.55f, 0.45f, 0.7f)
                        : t.Rock == RockKind.Bedrock ? new Color(0.25f, 0.25f, 0.28f)
                        : new Color(0.38f, 0.36f, 0.34f);
                    var r = new Rect(area.x + x * cell, area.y + y * cell, cell - 1f, cell - 1f);
                    if (!GUI.Button(r, GUIContent.none) || t.IsOpen || !seen) continue;
                    if (_fireMode)
                    {
                        string why = _mines.CanFireSet(m, x, y);
                        _status = why ?? (_mines.FireSet(m, x, y) ? GameLocalization.T("ui.mine.fire.lit", ("rock", MineSystem.RockName(t.Rock))) : GameLocalization.T("ui.mine.fire.failed"));
                    }
                    else
                    {
                        _mines.Mark(m, x, y, !t.Marked);
                        _status = t.Marked ? $"İşaretlendi: {MineSystem.RockName(t.Rock)}{(t.Ore != null ? ", " + Name(t.Ore) : "")}" : "İşaret kaldırıldı";
                    }
                }
            GUI.color = prev;
            GUILayout.EndScrollView();
            if (_status != null) GUILayout.Label(_status);

            int creaks = 0;
            foreach (var (_, _, t) in m.All()) if (t.Creaking) creaks++;
            if (creaks > 0) GUILayout.Label(GameLocalization.T("ui.mine.panel.creaks", ("n", creaks)));
            GUILayout.Label(GameLocalization.T("ui.mine.panel.work_below"));
            bool fire = GUILayout.Toggle(_fireMode, " " + GameLocalization.T("ui.mine.fire.mode", ("wood", MineSystem.FireWood)));
            if (fire != _fireMode) { _fireMode = fire; _status = fire ? GameLocalization.T("ui.mine.fire.help") : null; }
            var c = Actor?.Invoke();
            if (c != null && GoDown != null && GUILayout.Button(GameLocalization.T("ui.mine.go_down", ("name", c.Name)))) GoDown(c);
            GUILayout.EndArea();
        }
    }
}
