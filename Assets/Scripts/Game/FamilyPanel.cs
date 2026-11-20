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
        /// <summary>Hane beceri panosu, kâhyalık ayarları (belge 45); null ise bölüm görünmez.</summary>
        public Thatchwick.Core.Life.LifeSystems Life { get; set; }
        private bool _boardOpen;

        /// <summary>Süreli görevler (belge 47 §3); null ise "Göreve gönder" görünmez.</summary>
        public TimedDutyService TimedDuties { get; set; }
        /// <summary>Çocukların gezme izni (belge 47 §7.2); null ise görünmez.</summary>
        public Thatchwick.Core.Upbringing.ChildRoaming Roaming { get; set; }
        /// <summary>Eşya adı (azık ve ürün listesi).</summary>
        public Func<string, string> ItemName { get; set; }
        /// <summary>Çocuk annesinden kasabaya gitmek için izin ister; dönen metin cevaptır (oyun katmanı saati ve havayı verir).</summary>
        public Func<Character, string> AskRoam { get; set; }

        /// <summary>Ayna'yı açar (görünüş ve ad değiştirme); null ise düğme görünmez.</summary>
        public Action<Character> OpenMirror { get; set; }
        /// <summary>Portre karesi (geçici: dünya karakterinden baş kırpımı); null ise portre çizilmez.</summary>
        public Func<Character, Sprite> PortraitOf { get; set; }
        private const float PortraitSize = 40f;

        // "Göreve gönder" formu (tek kişi için açılır).
        private int _dutyFor;
        private TimedDutyKind _dutyKind = TimedDutyKind.Mine;
        private string _dutyPlace;
        private double _dutyHours = 4;
        private bool _dutyRepeat;

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
            DrawJewelry(actor);
            DrawSkillBoard();
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
            bool portrait = PortraitOf != null && !c.IsVillager && !c.IsDead;
            if (portrait) Portraits.Draw(GUILayoutUtility.GetRect(PortraitSize, PortraitSize, GUILayout.Width(PortraitSize), GUILayout.Height(PortraitSize)), PortraitOf(c));
            UiSkin.Wrap($"{Thatchwick.Core.Progress.Mastery.DisplayName(c)}{head} ({state}{spouse})", UiSkin.FamilyTextW - (portrait ? PortraitSize + 4f : 0f));   // usta unvanı (belge 45)
            if (CanTakeCommands(c) && c != actor && GUILayout.Button("Seç", GUILayout.Width(40f))) { _select(c); _status = $"{c.Name} seçildi."; }
            GUILayout.EndHorizontal();
            if (OpenMirror != null && !c.IsVillager && !c.IsDead && c.Stage != AgeStage.Baby
                && GUILayout.Button(GameLocalization.T(c.FreeMirrorAvailable ? "ui.mirror.open_free" : "ui.mirror.open"), GUILayout.Width(c.FreeMirrorAvailable ? 120f : 60f)))
                OpenMirror(c);
            // Hanedan geçmişi: kişinin hayat notları ("Neşeli bir soydan geldi ama zor bir çocukluk geçirdi.").
            if (!c.IsVillager) foreach (var note in c.Chronicle) UiSkin.Wrap("    · " + GameLocalization.Saved(note), UiSkin.FamilyTextW);

            // Görev (otomasyonun işçi tarafı): ihtiyaçları iyiyken gündüz kendiliğinden tarlaya ya da hayvanlara bakar.
            if (!c.IsVillager && DutyPlanner.CanTakeDuty(c))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUILayout.Label("Görev:", GUILayout.Width(44f));
                foreach (var d in new[] { DutyKind.None, DutyKind.Farm, DutyKind.Animals, DutyKind.Mine, DutyKind.House, DutyKind.Steward })
                {
                    string label = DutyPlanner.DutyName(d);
                    if (d != DutyKind.None && d != DutyKind.Steward && Duties != null) label += $" ({Duties.Pending(d)})";
                    GUI.enabled = c.Duty != d && DutyPlanner.CanTakeDuty(c, d);
                    if (GUILayout.Button(label, GUILayout.Width(d == DutyKind.None ? 70f : 84f)))
                    {
                        c.Duty = d;
                        _status = d == DutyKind.None ? $"{c.Name} görevden alındı."
                            : d == DutyKind.Steward ? $"{c.Name} kâhya: her sabah öbürlerine bekleyen işe göre görev dağıtır."
                            : d == DutyKind.House ? $"{c.Name}: Ev görevi (mum, sepet, döküntü, ayna, saksı, ot, badana; akşama kadar)."
                            : $"{c.Name}: {DutyPlanner.DutyName(d)} görevi (gündüz, ihtiyaçları iyiyken).";
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }

            if (!c.IsVillager && !c.IsDead) DrawTimedDuty(c);
            if (!c.IsVillager && !c.IsDead) DrawRoaming(c);
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
                // Hayvanın konuştuğunu anlat (belge 55 §3.4): yalnızca yaşayan, her dinleyene bir kez.
                if (_family.CanDo(actor, c, SocialAction.TellAnimalTalk) == null) Action(actor, c, SocialAction.TellAnimalTalk, GameLocalization.T("ui.social.tell_animal_talk"));
                if (actor.SpouseId == c.Id) Action(actor, c, SocialAction.TryForBaby, "Çocuk iste");
                else if (FamilyService.IsAdultAge(actor) && FamilyService.IsAdultAge(c) && !actor.SpouseId.HasValue && !c.SpouseId.HasValue)
                    Action(actor, c, SocialAction.Propose, "Evlen");
            }
            GUILayout.EndHorizontal();
            DrawSkillSocials(actor, c);
        }

        // Süreli görev (belge 47 §3.7): tür → yer → süre → azık → başlat; görevdeyken ilerleme ve "Geri çağır".
        private void DrawTimedDuty(Character c)
        {
            var svc = TimedDuties;
            if (svc == null || c.Stage == AgeStage.Baby) return;
            var running = svc.Find(c.Id);
            if (running != null)
            {
                var place = TimedDutyService.Place(running.Order.PlaceId);
                var got = new List<string>();
                foreach (var kv in TimedDutyService.SoFar(running)) got.Add(GameLocalization.R(Thatchwick.Core.Localization.LocText.Of("duty.sum.item", ("n", kv.Value), ("item", kv.Key))));
                var dutyIcon = UiSkin.Icon("duty_" + running.Order.Kind.ToString().ToLowerInvariant());   // PixelLab işi 480
                if (dutyIcon != null) GUILayout.Label(dutyIcon, GUILayout.Width(20f), GUILayout.Height(20f));
                string line = GameLocalization.T("ui.duty.running", ("kind", GameLocalization.R(TimedDutyService.KindText(running.Order.Kind))),
                    ("place", GameLocalization.R(TimedDutyService.PlaceText(running.Order.PlaceId))),
                    ("done", running.ElapsedHours.ToString("0.0")), ("planned", running.PlannedHours.ToString("0.0")));
                if (running.Playing) line += GameLocalization.T("ui.duty.playing");
                if (got.Count > 0) line += GameLocalization.T("ui.duty.so_far", ("items", string.Join(", ", got)));
                UiSkin.Wrap(line, UiSkin.FamilyTextW);
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                if (GUILayout.Button(new GUIContent(GameLocalization.T("ui.duty.recall"), GameLocalization.T("ui.duty.recall_tip")), GUILayout.Width(100f))) { var r = svc.Recall(c.Id); _status = r != null ? GameLocalization.R(r.Summary) : ""; }
                GUILayout.EndHorizontal();
                return;
            }
            bool repeating = false;
            foreach (var o in svc.Repeats) if (o.CharacterId == c.Id) repeating = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            if (GUILayout.Button(GameLocalization.T(_dutyFor == c.Id ? "ui.duty.send_open" : "ui.duty.send"), GUILayout.Width(120f)))
            {
                _dutyFor = _dutyFor == c.Id ? 0 : c.Id;
                _dutyPlace = null;
            }
            if (repeating && GUILayout.Button(new GUIContent(GameLocalization.T("ui.duty.stop_repeat"), GameLocalization.T("ui.duty.stop_repeat_tip")), GUILayout.Width(100f))) { svc.StopRepeat(c.Id); _status = GameLocalization.T("ui.duty.repeat_stopped", ("name", c.Name)); }
            GUILayout.EndHorizontal();
            if (_dutyFor != c.Id) return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            foreach (var k in new[] { TimedDutyKind.Mine, TimedDutyKind.Fish, TimedDutyKind.Forest, TimedDutyKind.Forage, TimedDutyKind.Traps, TimedDutyKind.Clay })
            {
                if (TimedDutyService.MaxHours(c, k) <= 0) continue;
                GUI.color = _dutyKind == k ? new Color(1f, 0.9f, 0.55f) : Color.white;
                if (GUILayout.Button(GameLocalization.R(TimedDutyService.KindText(k)), GUILayout.Width(70f))) { _dutyKind = k; _dutyPlace = null; }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            foreach (var p in TimedDutyService.Places)
            {
                if (p.Kind != _dutyKind) continue;
                if (_dutyPlace == null) _dutyPlace = p.Id;
                GUI.color = _dutyPlace == p.Id ? new Color(1f, 0.9f, 0.55f) : Color.white;
                if (GUILayout.Button(GameLocalization.R(TimedDutyService.PlaceText(p.Id)), GUILayout.Width(118f))) _dutyPlace = p.Id;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            foreach (var h in new[] { 2.0, 4.0, 8.0, 0.0 })
            {
                GUI.color = Math.Abs(_dutyHours - h) < 0.01 ? new Color(1f, 0.9f, 0.55f) : Color.white;
                if (GUILayout.Button(h > 0 ? GameLocalization.T("ui.duty.hours", ("n", (int)h)) : GameLocalization.T("ui.duty.until_evening"), GUILayout.Width(h > 0 ? 56f : 96f))) _dutyHours = h;
            }
            GUI.color = Color.white;
            _dutyRepeat = GUILayout.Toggle(_dutyRepeat, GameLocalization.T("ui.duty.every_day"));
            GUILayout.EndHorizontal();

            var order = new TimedDutyOrder { CharacterId = c.Id, Kind = _dutyKind, PlaceId = _dutyPlace, Hours = _dutyHours, Repeat = _dutyRepeat };
            var why = svc.CanStart(c, order);
            double hours = svc.HoursFor(order);
            var food = svc.SuggestFood(c, hours);
            int meals = svc.MealsNeeded(c, hours);
            var foodText = new List<string>();
            int have = 0;
            foreach (var kv in food) { foodText.Add(GameLocalization.R(Thatchwick.Core.Localization.LocText.Of("duty.sum.item", ("n", kv.Value), ("item", kv.Key)))); have += kv.Value; }
            var place2 = TimedDutyService.Place(_dutyPlace);
            string foodLine = GameLocalization.T("ui.duty.food", ("food", foodText.Count > 0 ? string.Join(", ", foodText) : GameLocalization.T("ui.duty.food_none")));
            if (have < meals) foodLine += GameLocalization.T("ui.duty.food_short", ("have", have), ("meals", meals));
            if (place2 != null && place2.Kind != TimedDutyKind.Errand) foodLine += GameLocalization.T("ui.duty.efficiency", ("pct", (int)Math.Round(svc.Efficiency(c, place2) * 100)));
            UiSkin.Wrap(foodLine, UiSkin.FamilyTextW);
            if (c.Stage == AgeStage.Child) UiSkin.Wrap(GameLocalization.T("ui.duty.child_note"), UiSkin.FamilyTextW);
            if (c.Needs.Get(NeedType.Energy) < 40f) UiSkin.Wrap(GameLocalization.T("ui.duty.tired_note", ("name", c.Name)), UiSkin.FamilyTextW);
            if (!why.IsEmpty) UiSkin.Wrap("      " + GameLocalization.R(why), UiSkin.FamilyTextW);
            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            GUI.enabled = why.IsEmpty;
            if (GUILayout.Button(GameLocalization.T("ui.duty.start"), GUILayout.Width(80f)))
            {
                if (svc.Start(order, out var reason) != null)
                {
                    _runnerOf(c)?.Cancel();
                    string where = GameLocalization.R(TimedDutyService.PlaceText(_dutyPlace));
                    _status = hours > 0 ? GameLocalization.T("ui.duty.started", ("name", c.Name), ("place", where), ("hours", hours.ToString("0.#")))
                                        : GameLocalization.T("ui.duty.started_evening", ("name", c.Name), ("place", where));
                    _dutyFor = 0;
                }
                else _status = GameLocalization.R(reason);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        // Çocuğun gezme izni (belge 47 §7.2): yalnızca arsa · köy yolu · gündüz kasaba.
        private void DrawRoaming(Character c)
        {
            if (Roaming == null || c.Stage != AgeStage.Child) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUILayout.Label(GameLocalization.T("ui.roam.permission"), GUILayout.Width(76f));
            foreach (var p in new[] { Thatchwick.Core.Upbringing.RoamPermission.Home, Thatchwick.Core.Upbringing.RoamPermission.Road, Thatchwick.Core.Upbringing.RoamPermission.Town })
            {
                GUI.color = Roaming.PermissionOf(c.Id) == p ? new Color(1f, 0.9f, 0.55f) : Color.white;
                string perm = GameLocalization.R(Thatchwick.Core.Upbringing.ChildRoaming.PermissionText(p));
                if (GUILayout.Button(perm, GUILayout.Width(96f))) { Roaming.SetPermission(c.Id, p); _status = GameLocalization.T("ui.roam.set", ("name", c.Name), ("permission", GameLocalization.Current.ToLower(perm))); }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            if (AskRoam == null) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(88f);
            if (GUILayout.Button(new GUIContent(GameLocalization.T("ui.roam.ask"), GameLocalization.T("ui.roam.ask_tip")), GUILayout.Width(190f)))
                _status = AskRoam(c);
            GUILayout.EndHorizontal();
        }

        private string Name(string id) => ItemName != null ? ItemName(id) : id;

        // Çocukluk eşyaları, can yoldaşı, çıraklık (belge 30, 31-8, 34-D1).
        private void DrawUpbringing(Character c, Character actor)
        {
            var u = Upbringing;
            if (u == null) return;
            var info = new List<string>();
            foreach (var p in u.Toys.PersonalOf(c.Id))
                info.Add($"{ToyNames.Name(p.ItemId)}{(p.IsHeirloom ? " (yadigâr)" : "")}{(p.IsBroken ? " kırık" : "")}");
            foreach (var m in u.Toys.MarksOf(c.Id)) { var mn = u.Toys.MarkName(m); if (mn != null) info.Add(mn); }
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
        /// <summary>
        /// Takılar (belge 48 §5.7): seçili kişinin taktıkları ve hanenin çekmecesindeki takılar. Gardırop ekranı
        /// gelene kadar takıp çıkarma buradan yapılır; etkiler (pazarlık, gönül, kaza, keyif) satırda yazar.
        /// </summary>
        private void DrawJewelry(Character actor)
        {
            if (Life == null || Content?.Clothing == null || Content.Clothing.Count == 0) return;
            var svc = Life.Clothing;
            var drawer = new List<Thatchwick.Core.Clothing.ClothingInstance>(svc.UnwornJewelry());
            var worn = actor != null && !actor.IsVillager
                ? new List<Thatchwick.Core.Clothing.ClothingInstance>(svc.JewelryOwnedBy(actor))
                : new List<Thatchwick.Core.Clothing.ClothingInstance>();
            var stock = new List<KeyValuePair<string, int>>(Life.JewelryInInventory());
            if (drawer.Count == 0 && worn.Count == 0 && stock.Count == 0) return;
            UiSkin.Wrap(GameLocalization.T("ui.family.jewelry.title"), UiSkin.FamilyTextW);
            if (actor != null && !actor.IsVillager)
            {
                UiSkin.Wrap(GameLocalization.T("ui.family.jewelry.effects", ("name", actor.Name),
                    ("trade", Math.Round(svc.TradeBonus(actor) * 100)), ("court", Math.Round(svc.CourtBonus(actor) * 100)),
                    ("luck", Math.Round((1.0 - svc.AccidentFactor(actor)) * 100)), ("joy", svc.JewelryJoy(actor))), UiSkin.FamilyTextW);
                foreach (var it in worn)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12f);
                    GUILayout.Label(JewelryLabel(svc, it));
                    if (GUILayout.Button(GameLocalization.T("ui.family.jewelry.take_off"), GUILayout.Width(64f))) svc.TakeOffJewelry(actor, it);
                    GUILayout.EndHorizontal();
                }
            }
            // Envanterdeki takı eşyaları (dükkândan, tüccardan alınan): buradan takılır.
            foreach (var kv in stock)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUILayout.Label(GameLocalization.T("ui.family.jewelry.in_stock", ("item", Name(kv.Key)), ("n", kv.Value)));
                if (actor != null && !actor.IsVillager && GUILayout.Button(GameLocalization.T("ui.family.jewelry.wear"), GUILayout.Width(64f)))
                    _status = JewelryError(Life.WearJewelryFromInventory(actor, kv.Key), actor);
                GUILayout.EndHorizontal();
            }
            if (drawer.Count == 0) return;
            UiSkin.Wrap(GameLocalization.T("ui.family.jewelry.drawer"), UiSkin.FamilyTextW);
            foreach (var it in drawer)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUILayout.Label(JewelryLabel(svc, it));
                if (actor != null && !actor.IsVillager && GUILayout.Button(GameLocalization.T("ui.family.jewelry.wear"), GUILayout.Width(64f)))
                    _status = JewelryError(svc.WearJewelry(actor, it), actor);
                if (!it.Heirloom && GUILayout.Button(new GUIContent(GameLocalization.T("ui.family.jewelry.to_stock"), GameLocalization.T("ui.family.jewelry.to_stock_tip")), GUILayout.Width(96f)))
                    Life.JewelryToInventory(it);
                GUILayout.EndHorizontal();
            }
        }

        private static string JewelryError(Thatchwick.Core.Clothing.EquipError err, Character actor) =>
            err == Thatchwick.Core.Clothing.EquipError.None ? ""
            : GameLocalization.T(err == Thatchwick.Core.Clothing.EquipError.Age ? "ui.family.jewelry.err_age"
                : err == Thatchwick.Core.Clothing.EquipError.JewelryLimit ? "ui.family.jewelry.err_limit"
                : "ui.family.jewelry.err_other", ("name", actor.Name));

        private string JewelryLabel(Thatchwick.Core.Clothing.ClothingService svc, Thatchwick.Core.Clothing.ClothingInstance it)
        {
            var def = svc.JewelryOf(it);
            string name = def != null ? Name(def.ItemId) : it.ModelId;
            if (it.Heirloom && !string.IsNullOrEmpty(it.HeirloomNote)) return GameLocalization.T("ui.family.jewelry.heirloom", ("item", name), ("from", it.HeirloomNote));
            return string.IsNullOrEmpty(it.MakerName) ? name : GameLocalization.T("ui.family.jewelry.made_by", ("item", name), ("maker", it.MakerName));
        }

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

        // Beceri seviyesiyle açılan sosyal işler (belge 45): yalnızca açılmış ve bu kişiye uygun olanlar, satır satır.
        // ---------- hane beceri panosu ve "tek usta" uyarısı (belge 45, 7. bölüm madde 8) ----------

        /// <summary>Bir beceride hanede bu seviyede ve üstünde tek kişi varsa "tek usta" uyarısı verilir.</summary>
        public const int SoleMasterLevel = 5;

        /// <summary>Hanenin beceri tablosu: her becerinin en iyisi; tek ustası olan beceriler uyarı alır.</summary>
        public static List<(SkillType skill, Character best, int level, int holders)> SkillTable(IEnumerable<Character> household)
        {
            var rows = new List<(SkillType, Character, int, int)>();
            var members = new List<Character>();
            foreach (var m in household) if (!m.IsDead && !m.IsVillager) members.Add(m);
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                Character best = null; int lv = 1, holders = 0;
                foreach (var m in members)
                {
                    int l = m.Skills.GetLevel(s);
                    if (l >= SoleMasterLevel) holders++;
                    if (l > lv) { lv = l; best = m; }
                }
                if (best != null) rows.Add((s, best, lv, holders));
            }
            rows.Sort((a, b) => b.Item3.CompareTo(a.Item3));
            return rows;
        }

        private void DrawSkillBoard()
        {
            if (Life == null) return;
            if (GUILayout.Button(_boardOpen ? GameLocalization.T("ui.family.skill_board_close") : GameLocalization.T("ui.family.skill_board"))) _boardOpen = !_boardOpen;
            var table = SkillTable(_population.Members);
            // Tek usta uyarısı her zaman görünür: bilgi tek kişide kalırsa ölümüyle kaybolur.
            foreach (var r in table)
                if (r.holders == 1 && (r.best.Stage == AgeStage.Elder || r.level >= 7))
                    UiSkin.Wrap(GameLocalization.T("ui.family.sole_master", ("skill", SkillNames.Name(r.skill)), ("name", r.best.Name), ("level", r.level)), UiSkin.FamilyTextW);
            if (!_boardOpen) return;
            foreach (var r in table)
            {
                string mark = r.holders == 1 ? " !" : "";
                UiSkin.Wrap(new GUIContent($"{SkillNames.Name(r.skill)} {r.level} · {r.best.Name}{mark}",
                    Thatchwick.Core.Progress.UnlockGate.LevelSummary(Content, r.skill, Math.Min(10, r.level + 1))), UiSkin.FamilyTextW);
            }
            DrawStewardship();
        }

        /// <summary>İçerik veritabanı (seviye özeti ipucu); null ise ipucu boş.</summary>
        public Thatchwick.Core.Content.ContentDatabase Content { get; set; }

        private static readonly DutyKind[] PriorityKinds = { DutyKind.Farm, DutyKind.Animals, DutyKind.Mine, DutyKind.House };
        private static readonly string[] PriorityNames = { "ui.family.priority.low", "ui.family.priority.mid", "ui.family.priority.high" };

        // Kâhyalık: öncelik (2), ırgat (3), haftalık plan (4), yarıcılık (5); kiler düzeni (6) ve imece (7) pasif ve kürsüde.
        private void DrawStewardship()
        {
            int lv = Life.BestSkill(SkillType.Stewardship);
            if (lv < 2) { UiSkin.Wrap(GameLocalization.T("ui.family.priority_locked"), UiSkin.FamilyTextW); return; }
            UiSkin.Wrap(GameLocalization.T("ui.family.priorities"), UiSkin.FamilyTextW);
            foreach (var k in PriorityKinds)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(DutyPlanner.DutyName(k), GUILayout.Width(90f));
                int p = Life.DutyPriority(k);
                for (int i = 0; i < 3; i++)
                    if (GUILayout.Toggle(p == i, GameLocalization.T(PriorityNames[i]), GUI.skin.button, GUILayout.Width(60f)) && p != i) Life.SetDutyPriority(k, i);
                GUILayout.EndHorizontal();
            }
            if (lv >= 3)
            {
                bool hired = GUILayout.Toggle(Life.HiredHand, GameLocalization.T(lv >= 9 ? "ui.family.hired_two" : "ui.family.hired", ("wage", Thatchwick.Core.Life.LifeSystems.HiredWage)));
                if (hired != Life.HiredHand) Life.HiredHand = hired;
            }
            if (lv >= 4 && GUILayout.Button(GameLocalization.T("ui.family.weekly_plan"))) _status = Life.WeeklyPlanText();
            if (lv >= 5)
            {
                bool share = GUILayout.Toggle(Life.Sharecrop, GameLocalization.T("ui.family.sharecrop"));
                if (share != Life.Sharecrop) Life.Sharecrop = share;
            }
            if (lv >= Thatchwick.Core.Life.LifeSystems.SalesRouteLevel)
            {
                bool route = GUILayout.Toggle(Life.SalesRoute, GameLocalization.T("ui.family.sales_route", ("keep", Thatchwick.Core.Life.LifeSystems.SalesRouteKeep)));
                if (route != Life.SalesRoute) Life.SalesRoute = route;
            }
        }

        private void DrawSkillSocials(Character actor, Character target)
        {
            int n = 0;
            foreach (var d in FamilyService.UnlockedSocials(actor))
            {
                if (_family.CanDo(actor, target, d.Action) != null) continue;
                if (n % 3 == 0) { if (n > 0) GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); GUILayout.Space(12f); }
                Action(actor, target, d.Action, d.Label);
                n++;
            }
            if (n > 0) GUILayout.EndHorizontal();
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
