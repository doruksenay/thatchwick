using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Hatıra defteri (J, geçici IMGUI; Project belgesi 27d): kazanılan hatıralar (kimin, hangi nesil, yerinde mi),
    /// soy ağacı ve Köklerimiz durumu, solmuş mevsim süsleri. Kurallar Core.Life.KeepsakeService'te; panel yalnızca gösterir.
    /// </summary>
    public sealed class KeepsakePanel
    {
        private readonly KeepsakeService _keepsakes;
        private readonly WorldState _world;
        private readonly GameClock _clock;
        private Vector2 _scroll;

        public bool Active { get; private set; }

        public KeepsakePanel(KeepsakeService keepsakes, WorldState world, GameClock clock)
        {
            _keepsakes = keepsakes; _world = world; _clock = clock;
        }

        public void Toggle() => Active = !Active;
        public void Close() => Active = false;

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 368f, 8f, 360f, Screen.height / guiScale - 16f);

        public void OnGUI(float guiScale)
        {
            if (!Active || _keepsakes == null) return;
            _keepsakes.Reconcile();
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("HATIRA DEFTERİ");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            GUILayout.Label("Hatıralar satın alınmaz, kazanılır; satılmaz. Nesiller geçtikçe güzelleşir.");

            // Soy ağacı
            bool tree = _keepsakes.HasFamilyTree();
            GUILayout.Label(tree
                ? $"{_keepsakes.TreeLabel()} · evde asılı: aileye \"Köklerimiz\", yas kısalır."
                : "Soy ağacı işlemesi evde asılı değil (dokuma tezgâhı, Dokumacılık 5).");

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label($"— Hatıralar ({_keepsakes.Records.Count}) —");
            if (_keepsakes.Records.Count == 0)
                GUILayout.Label("Henüz yok. Şenliğe katıl, birinci sınıf hasat kaldır, ilk balığını tut; doğan her bebeğin ilk ayakkabısı da buraya yazılır.");
            var placed = new Dictionary<int, ItemInstance>();
            foreach (var it in _world.Items) if (it.Def.IsKeepsake) placed[it.Id] = it;
            for (int i = _keepsakes.Records.Count - 1; i >= 0; i--)
            {
                var r = _keepsakes.Records[i];
                string where = r.InstanceId != 0 && placed.ContainsKey(r.InstanceId) ? "yerinde" : "envanterde";
                GUILayout.Label($"• {r.Label} — {r.Generation}. nesil, {where}");
            }

            // Solmuş mevsim süsleri
            var wilted = new List<string>();
            foreach (var it in _world.Items)
                if (DecorSystem.IsWilted(it, _clock.Season)) wilted.Add(it.Def.DisplayName);
            if (wilted.Count > 0)
            {
                GUILayout.Label("— Solmuş mevsim süsleri —");
                var old = GUI.color;
                GUI.color = new Color(1f, 0.75f, 0.6f);
                GUILayout.Label(string.Join(", ", wilted) + ": kaldırıp mevsimin süsünü as.");
                GUI.color = old;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
