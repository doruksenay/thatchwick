using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Work;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Upbringing;
using Thatchwick.Core.Build;

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

        /// <summary>Çocukluk, can yoldaşı, çıraklık, aile tarifi, itibar (belge 30, 31-8, 34); null ise bölümler görünmez.</summary>
        public UpbringingSystems Upbringing { get; set; }
        /// <summary>Boş defter ve onarım tahtası için.</summary>
        public Household House { get; set; }

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
            if (Upbringing != null)
                UiSkin.Wrap(new GUIContent($"İtibar: {Upbringing.Reputation.TierName} ({Upbringing.Reputation.Points}) · satış {Upbringing.Reputation.SellBonus() * 100:+0.#;-0.#;0}%",
                    string.Join("\n", Upbringing.Reputation.RecentLog)), UiSkin.FamilyTextW);
            DrawWeddingPrep();
            DrawApprenticeApplication();
            UiSkin.Wrap(actor != null ? $"Komut alan: {actor.Name} (haritaya dokununca o gider)" : "Komut alacak kişiyi seç.", UiSkin.FamilyTextW);
            if (_status.Length > 0) UiSkin.Wrap(_status, UiSkin.FamilyTextW);
            if (actor != null && actor.MoodEnabled)
            {
                UiSkin.Wrap($"Ruh hali: {Thatchwick.Core.Mood.MoodRules.Name(actor.Mood.Status)} ({actor.Mood.Total})", UiSkin.FamilyTextW);
                foreach (var m in actor.Mood.Active)
                {
                    string left = m.Def.IsCondition ? "" : $", {m.RemainingDays:0.0} gün";
                    UiSkin.Wrap($"  {(m.Def.Value > 0 ? "+" : "")}{m.Def.Value} {m.Def.DisplayName}{left}: {m.Def.Description}", UiSkin.FamilyTextW);
                }
            }

            _scroll = GUILayout.BeginScrollView(_scroll);
            UiSkin.Wrap("— Hane —", UiSkin.FamilyTextW);
            foreach (var c in _population.Members) if (!c.IsVillager) DrawRow(c, actor);
            DrawRecipes(actor);
            UiSkin.Wrap("— Köylüler (pazar çevresi) —", UiSkin.FamilyTextW);
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
            UiSkin.Wrap($"{c.Name}{head} ({state}{spouse})", UiSkin.FamilyTextW);
            if (CanTakeCommands(c) && c != actor && GUILayout.Button("Seç", GUILayout.Width(40f))) { _select(c); _status = $"{c.Name} seçildi."; }
            GUILayout.EndHorizontal();
            // Hanedan geçmişi: kişinin hayat notları ("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.").
            if (!c.IsVillager) foreach (var note in c.Chronicle) UiSkin.Wrap("    · " + note, UiSkin.FamilyTextW);

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
            if (!c.IsVillager && !c.IsDead) DrawUpbringing(c, actor);

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

        // Çocukluk eşyaları, can yoldaşı, çıraklık (belge 30, 31-8, 34-D1).
        private void DrawUpbringing(Character c, Character actor)
        {
            var u = Upbringing;
            if (u == null) return;
            var info = new List<string>();
            foreach (var p in u.Toys.PersonalOf(c.Id))
                info.Add($"{ToyNames.Name(p.ItemId)}{(p.IsHeirloom ? " (yadigâr)" : "")}{(p.IsBroken ? " kırık" : "")}");
            foreach (var m in u.Toys.MarksOf(c.Id))
                foreach (var d in u.Toys.Toys) if (d.MarkId == m) info.Add(d.MarkName);
            foreach (var b in u.Companions.Bonds)
                if (b.ChildId == c.Id && (b.Unlocked || b.Friend))
                    info.Add($"{(b.Unlocked ? CompanionBondSystem.TraitName(CompanionBondSystem.TraitFor(b.SpeciesId)) : "dostu")}: {b.AnimalName}{(b.Memory ? " (anı)" : "")}");
            var ap = u.Apprentices.OfApprentice(c.Id);
            if (ap != null) info.Add($"çırak: {SkillNames.Name(ap.Skill)}, ustası {_population.Find(ap.MasterId)?.Name}");
            var ms = u.Apprentices.OfMaster(c.Id);
            if (ms != null) info.Add($"çırağı: {_population.Find(ms.ApprenticeId)?.Name} ({SkillNames.Name(ms.Skill)})");
            if (info.Count > 0) UiSkin.Wrap("    ◦ " + string.Join(" · ", info), UiSkin.FamilyTextW);

            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            foreach (var p in u.Toys.PersonalOf(c.Id))
            {
                if (!p.IsBroken) continue;
                if (GUILayout.Button(new GUIContent($"Onar: {ToyNames.Name(p.ItemId)}", "1 tahta"), GUILayout.Width(120f)))
                    _status = u.RepairToy(c, p.ItemId, House) ?? $"{c.Name}'in {ToyNames.Name(p.ItemId)}ı onarıldı.";
            }
            if (ap != null && GUILayout.Button("Çıraklığı bitir", GUILayout.Width(100f))) { u.Apprentices.End(c.Id); _status = $"{c.Name} çıraklığı bıraktı."; }
            // Seçili kişi kendi oyuncağını bu çocuğa ödünç verir.
            if (actor != null && actor != c && c.Stage >= AgeStage.Child && c.Stage <= AgeStage.Teen)
                foreach (var p in u.Toys.PersonalOf(actor.Id))
                {
                    if (p.IsBroken || u.Toys.Personal(c.Id, p.ItemId) != null) continue;
                    if (GUILayout.Button(new GUIContent($"Ödünç ver: {ToyNames.Name(p.ItemId)}", $"{actor.Name} oyuncağını {c.Name}'e verir; ikisi de sevinir"), GUILayout.Width(150f)))
                        _status = u.Borrow(actor, c, p.ItemId, lend: true);
                }
            if (actor != null && actor != c)
            {
                var skill = u.TeachableSkill(actor, c);
                if (skill.HasValue && GUILayout.Button(new GUIContent($"Çırak al: {SkillNames.Name(skill.Value)}", $"{c.Name}, {actor.Name}'in yanında x{ApprenticeshipSystem.BaseFactor:0.#}+ hızlı öğrenir"), GUILayout.Width(150f)))
                {
                    u.Apprentices.Take(actor, c, skill.Value, Today != null ? Today() : 0);
                    _status = $"{c.Name}, {actor.Name}'in yanında {SkillNames.Name(skill.Value).ToLowerInvariant()} çırağı oldu.";
                }
                if (CompanionBondSystem.IsYoung(c))
                {
                    string why = u.ParentAssistBlocker(actor, c);
                    var a = u.AssistTarget(c);
                    GUI.enabled = why == null;
                    string label = a != null && a.Species.Rideable ? $"Ata bindir ({a.Name})" : a != null ? $"Hayvana götür ({a.Name})" : "Hayvana götür";
                    if (GUILayout.Button(new GUIContent(label, why ?? "Can yoldaşı bağını güçlendirir"), GUILayout.Width(150f)))
                        _status = u.ParentAssist(actor, c) ?? $"{actor.Name}, {c.Name} ile {a?.Name}'e baktı.";
                    GUI.enabled = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        // Düğün hazırlığı (belge 34-D2): bekâr yetişkin varsa ekmek, içki, mum, çalgıcı ve beklenen ölçek.
        private void DrawApprenticeApplication()
        {
            var a = Upbringing?.Application;
            if (a == null) return;
            var ap = _population.Find(a.ApplicantId);
            var ms = _population.Find(a.MasterId);
            if (ap == null || ms == null) return;
            UiSkin.Wrap($"Çırak başvurusu: {ap.Name}, {ms.Name}'in yanında {Thatchwick.Core.Characters.SkillNames.Name(a.Skill)} öğrenmek istiyor.", UiSkin.FamilyTextW);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kabul et", GUILayout.Width(90f))) _status = Upbringing.AnswerApplication(true);
            if (GUILayout.Button("Geri çevir", GUILayout.Width(90f))) _status = Upbringing.AnswerApplication(false);
            GUILayout.EndHorizontal();
        }

        private void DrawWeddingPrep()
        {
            var w = Upbringing?.Weddings;
            if (w == null) return;
            bool single = false;
            foreach (var m in _population.Members)
                if (!m.IsVillager && !m.IsDead && FamilyService.IsAdultAge(m) && !m.SpouseId.HasValue) single = true;
            if (!single) return;
            var parts = new List<string>();
            foreach (var sup in w.Prep())
                parts.Add(sup.ItemIds.Length == 0 ? $"{sup.Name} {(sup.Ok ? "✓" : "✗")}" : $"{sup.Name} {Math.Min(sup.Have, sup.Need)}/{sup.Need}{(sup.Ok ? " ✓" : "")}");
            var (filled, max) = w.DowryFill != null ? w.DowryFill() : (0, 0);
            bool full = max > 0 && filled >= max;
            string dowry = max > 0 ? $" · çeyiz {filled}/{max}" : " · çeyiz sandığı yok";
            UiSkin.Wrap(new GUIContent($"Düğün hazırlığı: {string.Join(" · ", parts)}{dowry} → {WeddingSystem.ScaleName(w.ExpectedScale(full))}",
                $"Ekmek {WeddingSystem.BreadNeed}, içki {WeddingSystem.DrinkNeed}, mum {WeddingSystem.LightNeed} ve çalgıcı (Müzik {WeddingSystem.MusicLevel}+ ya da kavalda usta).\n" +
                "Hazır kalemler düğünde harcanır. 4 kalem görkemli, 2-3 güzel; dolu çeyiz sandığı bir basamak artırır.\n" +
                $"Talip niteliği şansı: %{w.SuitorQuality() * 100:0}"), UiSkin.FamilyTextW);
        }

        // Aile sırları: bilenler, öğret (seçili kişi bilir ve hedef hane üyesi bilmez), deftere yaz.
        private void DrawRecipes(Character actor)
        {
            var u = Upbringing;
            if (u == null || u.Recipes.Recipes.Count == 0) return;
            UiSkin.Wrap("— Aile sırları —", UiSkin.FamilyTextW);
            foreach (var r in u.Recipes.Recipes)
            {
                var names = new List<string>();
                foreach (var id in r.Knowers) { var k = _population.Find(id); if (k != null && !k.IsDead) names.Add(k.Name); }
                string state = r.Lost ? "kayboldu" : r.Written ? "yazılı" : names.Count == 1 ? "tek bilen!" : "sözlü";
                UiSkin.Wrap($"{r.Name} ({state}, {r.Generations}. kuşak): {(names.Count > 0 ? string.Join(", ", names) : "bilen yok")}", UiSkin.FamilyTextW);
                if (r.Lost) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                if (actor != null && u.Recipes.Knows(actor.Id, r))
                {
                    int shown = 0;
                    foreach (var m in _population.Members)
                    {
                        if (m.IsVillager || m.IsDead || m == actor || shown >= 3) continue;
                        if (u.Recipes.TeachBlocker(actor, m, r) != null) continue;
                        shown++;
                        if (GUILayout.Button($"Öğret: {m.Name}", GUILayout.Width(100f)))
                        {
                            u.Recipes.Teach(actor, m, r);
                            _status = $"{actor.Name}, {r.Name} tarifini {m.Name}'e öğretti.";
                        }
                    }
                    if (!r.Written)
                    {
                        bool hasBook = House != null && House.Count(FamilyRecipeBook.BlankBookItem) > 0;
                        string why = u.Recipes.WriteBlocker(actor, r, hasBook);
                        GUI.enabled = why == null;
                        if (GUILayout.Button(new GUIContent("Deftere yaz", why ?? "Boş defter harcanır; tarif kaybolmaz"), GUILayout.Width(90f)))
                            _status = u.WriteRecipe(actor, r, House) ?? $"{r.Name} deftere yazıldı.";
                        GUI.enabled = true;
                        if (why != null) UiSkin.Wrap(why, UiSkin.FamilyTextW);
                    }
                }
                else if (actor != null && r.Written && u.Recipes.TeachBlocker(null, actor, r) == null
                         && GUILayout.Button("Defterden oku", GUILayout.Width(100f)))
                {
                    u.Recipes.Teach(null, actor, r);
                    _status = $"{actor.Name}, {r.Name} tarifini defterden öğrendi.";
                }
                GUILayout.EndHorizontal();
            }
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
            if (reason != null && action != SocialAction.Chat) UiSkin.Wrap(reason, UiSkin.FamilyTextW);
        }
    }
}
