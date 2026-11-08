using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;

namespace Thatchwick.Game
{
    /// <summary>
    /// Aile paneli (F, geçici IMGUI): hane üyeleri ve köylüler. "Seç" ile haritadaki dokunuş komutlarını
    /// alacak kişi seçilir (aile içi iş verme). Seçili kişiyle diğerleri arasında sohbet, evlenme teklifi,
    /// çocuk isteme ve bebek besleme işleri verilir. Kurallar Core.Family.FamilyService'te.
    /// </summary>
    public sealed class FamilyPanel
    {
        private readonly Population _population;
        private readonly FamilyService _family;
        private readonly Dynasty _dynasty;
        private readonly Func<Character> _selected;
        private readonly Action<Character> _select;
        private readonly Func<Character, JobRunner> _runnerOf;
        private Vector2 _scroll;
        private string _status = "";

        public bool Active { get; private set; }

        public FamilyPanel(Population population, FamilyService family, Dynasty dynasty,
            Func<Character> selected, Action<Character> select, Func<Character, JobRunner> runnerOf)
        {
            _population = population; _family = family; _dynasty = dynasty;
            _selected = selected; _select = select; _runnerOf = runnerOf;
        }

        public void Toggle() { Active = !Active; _status = ""; }
        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 338f, 8f, 330f, Screen.height / guiScale - 16f);

        private static string StageName(AgeStage s)
        {
            switch (s)
            {
                case AgeStage.Baby: return "Bebek";
                case AgeStage.Child: return "Çocuk";
                case AgeStage.Youth: return "Genç";
                case AgeStage.Adult: return "Yetişkin";
                default: return "Yaşlı";
            }
        }

        public static bool CanTakeCommands(Character c) => c != null && !c.IsDead && !c.IsVillager && c.Stage != AgeStage.Baby;

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"AİLE · {_dynasty.CurrentGeneration}. nesil");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            var actor = _selected();
            GUILayout.Label(actor != null ? $"Komut alan: {actor.Name} (haritaya dokununca o gider)" : "Komut alacak kişiyi seç.");
            if (_status.Length > 0) GUILayout.Label(_status);

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label("— Hane —");
            foreach (var c in _population.Members) if (!c.IsVillager) DrawRow(c, actor);
            GUILayout.Label("— Köylüler (pazar çevresi) —");
            foreach (var c in _population.Members) if (c.IsVillager) DrawRow(c, actor);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawRow(Character c, Character actor)
        {
            string spouse = c.SpouseId.HasValue ? $", eşi {_population.Find(c.SpouseId.Value)?.Name}" : "";
            string head = c.Id == _dynasty.HeadId ? " ★" : "";
            string state = c.IsDead ? "öldü" : $"{StageName(c.Stage)} {c.AgeDays:0}g, Açlık {c.Needs.Get(NeedType.Hunger):0}{(c.IsPregnant ? ", hamile" : "")}";
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{c.Name}{head} ({state}{spouse})");
            if (CanTakeCommands(c) && c != actor && GUILayout.Button("Seç", GUILayout.Width(40f))) { _select(c); _status = $"{c.Name} seçildi."; }
            GUILayout.EndHorizontal();

            if (actor == null || c == actor || c.IsDead) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            if (c.Stage == AgeStage.Baby) Action(actor, c, SocialAction.Feed, "Besle");
            else
            {
                Action(actor, c, SocialAction.Chat, $"Sohbet ({_family.Affection(actor.Id, c.Id)})");
                if (actor.SpouseId == c.Id) Action(actor, c, SocialAction.TryForBaby, "Çocuk iste");
                else if (FamilyService.IsAdultAge(actor) && FamilyService.IsAdultAge(c) && !actor.SpouseId.HasValue && !c.SpouseId.HasValue)
                    Action(actor, c, SocialAction.Propose, "Evlen");
            }
            GUILayout.EndHorizontal();
        }

        private void Action(Character actor, Character target, SocialAction action, string label)
        {
            string reason = _family.CanDo(actor, target, action);
            GUI.enabled = reason == null;
            if (GUILayout.Button(new GUIContent(label, reason ?? ""), GUILayout.Width(110f)))
            {
                _runnerOf(actor)?.Command(Job.Social(target.Id, action));
                _status = $"{actor.Name} → {target.Name}: {label}";
            }
            GUI.enabled = true;
            if (reason != null && action != SocialAction.Chat) GUILayout.Label(reason);
        }
    }
}
