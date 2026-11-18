using System;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mining;

namespace Thatchwick.Game
{
    /// <summary>
    /// Ocak paneli (M, geçici IMGUI): aile ocağının galeri katı (-1). Tepe bölgesi (yol haritası 48) gelene kadar ocak ekran
    /// dışındadır: oyuncu karoları buradan işaretler, maden görevli aile üyeleri her sabah işaretli karoları kazar
    /// (MineSystem.Shift). Çatırdayan karo, su derinliği ve yarının suyu burada görünür. Kurallar Core.Mining'de.
    /// </summary>
    public sealed class MinePanel
    {
        private readonly MineSystem _mines;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private Vector2 _scroll;
        private string _status;

        /// <summary>Panelden "kendin in" diyen karakter (komut alan); null ise düğme görünmez.</summary>
        public Func<Character> Actor { get; set; }
        /// <summary>Ocak açılabilir mi (Madencilik ve akçe); açılmışsa null.</summary>
        public Func<string> OpenBlocker { get; set; }
        public event Action<string> Message;

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
            GUILayout.Label("AİLE OCAĞI · GALERİ KATI");
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
                if (GUILayout.Button("Ocağı aç"))
                {
                    _house.TrySpend(OpenCostAkce);
                    for (int i = 0; i < OpenCostPlanks; i++) _house.TryTakeItem("item.plank");
                    _mines.EnsureMine();
                    Message?.Invoke("Aile ocağı açıldı: karoları işaretle, maden görevlileri her sabah kazar");
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
                    if (GUI.Button(r, GUIContent.none) && !t.IsOpen && seen)
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
            GUILayout.BeginHorizontal();
            GUI.enabled = creaks > 0 && _house.Count("item.plank") >= MineSystem.WoodSupportPlanks;
            if (GUILayout.Button($"Çatırdayanlara destek ({creaks})"))
            {
                int n = 0;
                foreach (var (x, y, t) in m.All())
                    if (t.Creaking && _mines.PlaceSupport(m, x, y, SupportKind.Wood, Actor?.Invoke())) n++;
                _status = $"{n} destek kuruldu";
            }
            GUI.enabled = m.Water > 0;
            if (GUILayout.Button("Kovayla su boşalt")) { _mines.Bail(m, Actor?.Invoke()); _status = $"Su: {m.Water:0.#}"; }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            var c = Actor?.Invoke();
            if (c != null && GUILayout.Button($"{c.Name} insin: üç saat kaz"))
            {
                int dug = _mines.Shift(m, c, 0.125);
                _status = dug > 0 ? $"{c.Name} {dug} karo kazdı" : "Kazılacak işaretli karo yok (ya da yetişilemiyor).";
                if (dug > 0) c.AddMoodlet(Thatchwick.Core.Mood.MoodCatalog.Dusty);
            }
            GUILayout.EndArea();
        }
    }
}
