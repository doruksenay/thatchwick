using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;

namespace Thatchwick.Game
{
    /// <summary>
    /// Kiler paneli (K, geçici IMGUI): yiyecek depoları (ambar, kiler, mahzen, buzluk) ve hangi yiyeceğin nerede durduğu.
    /// Yerleşim otomatiktir (en çabuk bozulan en soğuk yere); panel yalnızca gösterir. Kurallar Core.Life.SpoilageService'te.
    /// </summary>
    public sealed class StoragePanel
    {
        private readonly SpoilageService _spoil;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private Vector2 _scroll;

        public bool Active { get; private set; }

        public StoragePanel(SpoilageService spoil, Household house, ContentDatabase content)
        {
            _spoil = spoil; _house = house; _content = content;
        }

        public void Toggle() => Active = !Active;
        public void Close() => Active = false;

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 338f, 8f, 330f, Screen.height / guiScale - 16f);

        private static string RateText(double rate) =>
            rate <= 0.3 ? "çok soğuk" : rate <= 0.55 ? "soğuk" : rate <= 0.8 ? "serin" : rate <= 1.05 ? "ılık" : "sıcak";

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            var plan = _spoil.CurrentPlan;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("KİLER VE AMBAR");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            GUILayout.Label("En çabuk bozulan (süt, et) en soğuk yere yerleşir. Ocaksız, taş duvarlı oda ya da bodrum serindir; buzluk buzla çok soğuktur.");
            _scroll = GUILayout.BeginScrollView(_scroll);

            GUILayout.Label("— Depolar —");
            if (plan.Slots.Count == 0) GUILayout.Label("Yiyecek deposu yok: kiler rafı, sandık, ambar, buzluk koy.");
            foreach (var s in plan.Slots)
            {
                string temp = s.Temperature.HasValue ? $"{s.Temperature.Value:0}°C, " : "";
                string ice = s.Item.Def.IsIceBox ? (s.Iced ? $", buz {s.Item.FuelDays:0.#} gün" : ", buzsuz!") : "";
                GUILayout.Label($"{s.Name}: {s.Used}/{s.Capacity} · {temp}{RateText(s.Rate)} (x{s.Rate:0.##}){ice}");
            }

            GUILayout.Label("— Yiyecekler —");
            var ids = new List<string>(plan.Placed.Keys);
            if (ids.Count == 0) GUILayout.Label("Taze yiyecek yok.");
            foreach (var id in ids)
            {
                var def = _content.Items.Get(id);
                var where = new List<string>();
                foreach (var kv in plan.Placed[id]) where.Add($"{kv.Key.Name} {kv.Value}");
                if (plan.Open.TryGetValue(id, out int open)) where.Add($"açıkta {open}");
                double left = _spoil.DaysLeftOldest(id);
                string days = left >= 0 ? $" · en eskisi {left:0.#} gün" : "";
                var old = GUI.color;
                if (plan.Open.ContainsKey(id) || (left >= 0 && left < 2)) GUI.color = new Color(1f, 0.75f, 0.6f);
                GUILayout.Label($"{def.DisplayName} {_house.Count(id)}: {string.Join(", ", where)}{days}");
                GUI.color = old;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
