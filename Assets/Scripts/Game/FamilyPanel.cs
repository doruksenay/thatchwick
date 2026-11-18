using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Work;
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

        /// <summary>Görev planlayıcısı (bekleyen iş sayısı için); null ise sayı gösterilmez.</summary>
        public DutyPlanner Duties { get; set; }

        /// <summary>Miras (komşu haneye evlendirme); null ise düğme görünmez.</summary>
        public Legacy Legacy { get; set; }
        public Func<double> Today { get; set; }

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

        private static string StageName(AgeStage s) => AgeStages.Name(s);

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
            if (actor != null && actor.MoodEnabled)
            {
                GUILayout.Label($"Ruh hali: {Thatchwick.Core.Mood.MoodRules.Name(actor.Mood.Status)} ({actor.Mood.Total})");
                foreach (var m in actor.Mood.Active)
                {
                    string left = m.Def.IsCondition ? "" : $", {m.RemainingDays:0.0} gün";
                    GUILayout.Label($"  {(m.Def.Value > 0 ? "+" : "")}{m.Def.Value} {m.Def.DisplayName}{left}: {m.Def.Description}");
                }
            }

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
            string mood = c.MoodEnabled ? $", {Thatchwick.Core.Mood.MoodRules.Name(c.Mood.Status)}" : "";
            string state = c.IsDead ? "öldü" : $"{StageName(c.Stage)} {c.AgeDays:0}g, Açlık {c.Needs.Get(NeedType.Hunger):0}{mood}{(c.IsPregnant ? ", hamile" : "")}";
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{c.Name}{head} ({state}{spouse})");
            if (CanTakeCommands(c) && c != actor && GUILayout.Button("Seç", GUILayout.Width(40f))) { _select(c); _status = $"{c.Name} seçildi."; }
            GUILayout.EndHorizontal();
            // Hanedan geçmişi: kişinin hayat notları ("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.").
            if (!c.IsVillager) foreach (var note in c.Chronicle) GUILayout.Label("    · " + note);

            // Görev (otomasyonun işçi tarafı): ihtiyaçları iyiyken gündüz kendiliğinden tarlaya ya da hayvanlara bakar.
            if (!c.IsVillager && DutyPlanner.CanTakeDuty(c))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUILayout.Label("Görev:", GUILayout.Width(44f));
                foreach (var d in new[] { DutyKind.None, DutyKind.Farm, DutyKind.Animals, DutyKind.Mine, DutyKind.Steward })
                {
                    string label = DutyPlanner.DutyName(d);
                    if (d != DutyKind.None && d != DutyKind.Steward && Duties != null) label += $" ({Duties.Pending(d)})";
                    GUI.enabled = c.Duty != d && DutyPlanner.CanTakeDuty(c, d);
                    if (GUILayout.Button(label, GUILayout.Width(d == DutyKind.None ? 70f : 84f)))
                    {
                        c.Duty = d;
                        _status = d == DutyKind.None ? $"{c.Name} görevden alındı."
                            : d == DutyKind.Steward ? $"{c.Name} kâhya: her sabah öbürlerine bekleyen işe göre görev dağıtır."
                            : $"{c.Name}: {DutyPlanner.DutyName(d)} görevi (gündüz, ihtiyaçları iyiyken).";
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }

            if (!c.IsVillager && !c.IsDead) DrawMarryOut(c);

            if (actor == null || c == actor || c.IsDead) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            if (c.Stage == AgeStage.Baby) Action(actor, c, SocialAction.Feed, "Besle");
            else
            {
                Action(actor, c, SocialAction.Chat, $"Sohbet ({_family.Affection(actor.Id, c.Id)})");
                if (c.Stage == AgeStage.Child && actor.Stage != AgeStage.Child && actor.Stage != AgeStage.Baby) Action(actor, c, SocialAction.TellStory, "Masal anlat");
                if (actor.SpouseId == c.Id) Action(actor, c, SocialAction.TryForBaby, "Çocuk iste");
                else if (FamilyService.IsAdultAge(actor) && FamilyService.IsAdultAge(c) && !actor.SpouseId.HasValue && !c.SpouseId.HasValue)
                    Action(actor, c, SocialAction.Propose, "Evlen");
            }
            GUILayout.EndHorizontal();
        }

        // Görücü usulü: hane üyesini yakın olduğu köylüyle evlendirip onun hanesine gönder (Soy Mührü verir, soy köyde sürer).
        private void DrawMarryOut(Character member)
        {
            if (Legacy == null || member.SpouseId.HasValue || !FamilyService.IsAdultAge(member) || member.Id == _dynasty.HeadId) return;
            var candidates = new List<Character>();
            foreach (var v in _population.Members)
                if (v.IsVillager && !v.IsDead && v.Sex != member.Sex && !v.SpouseId.HasValue && FamilyService.IsAdultAge(v)) candidates.Add(v);
            if (candidates.Count == 0) return;
            candidates.Sort((a, b) => _family.Affection(member.Id, b.Id).CompareTo(_family.Affection(member.Id, a.Id)));
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUILayout.Label("Evlendir:", GUILayout.Width(60f));
            for (int i = 0; i < candidates.Count && i < 3; i++)
            {
                var v = candidates[i];
                string why = Legacy.CanMarryOut(member, v, _dynasty, _family);
                GUI.enabled = why == null;
                if (GUILayout.Button(new GUIContent($"{v.Name} ({_family.Affection(member.Id, v.Id)})", why ?? $"Soy Mührü +{Legacy.MarriedOutSeals}"), GUILayout.Width(110f)))
                {
                    Legacy.MarryOut(member, v, _dynasty, _family, Today != null ? Today() : 0, out var msg);
                    _status = msg;
                }
                GUI.enabled = true;
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
