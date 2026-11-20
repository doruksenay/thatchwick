using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Life.Requests;
using Thatchwick.Core.Localization;

namespace Thatchwick.Game
{
    /// <summary>
    /// İstekler defteri (I, geçici IMGUI; belge 46 §12). Sekmeler: Ricalar · Siparişler · Mevsim · Hikâyeler · Adaklar; açık
    /// çağrılar her sekmenin üstünde. Kurallar RequestService'te; burası yalnızca çağırır ve nedeni gösterir.
    /// Metinler anahtarlıdır ("ui.req.*", Core'dan gelenler <see cref="LocText"/>; belge 21).
    /// Görseller (defter çerçevesi, tür ikonları, pano kâğıtları) PixelLab kuyruğunda; gelene kadar düz kutu.
    /// </summary>
    public sealed class RequestPanel
    {
        private static readonly string[] TabKeys = { "ui.req.tab.favors", "ui.req.tab.orders", "ui.req.tab.season", "ui.req.tab.stories", "ui.req.tab.vows" };

        private readonly LifeSystems _life;
        private readonly Func<Character> _commander;
        private int _tab;
        private Vector2 _scroll;
        private string _status = "";
        private int _assignFor;

        public bool Active { get; private set; }
        /// <summary>Haritada göster: verenin konumuna kamerayı götürür (oyun katmanı).</summary>
        public Action<Character> ShowOnMap { get; set; }

        public RequestPanel(LifeSystems life, Func<Character> commander)
        {
            _life = life;
            _commander = commander;
        }

        private RequestService Req => _life.Requests;
        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string name, object value)[] args) => GameLocalization.T(key, args);
        private static string R(LocText t) => GameLocalization.R(t);

        public void Toggle() { Active = !Active; _status = ""; _assignFor = 0; }
        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 388f, 8f, 380f, Screen.height / guiScale - 16f);

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            int unread = Req.UnreadCount;
            GUILayout.Label(unread > 0 ? T("ui.req.header_unread", ("rep", _life.Upbringing.Reputation.Points), ("n", unread))
                                       : T("ui.req.header", ("rep", _life.Upbringing.Reputation.Points)));
            if (GUILayout.Button(T("ui.req.close"), GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            if (unread > 0 && !Req.Literate) GUILayout.Label(T("ui.req.go_to_board"));
            var tabs = new string[TabKeys.Length];
            for (int i = 0; i < tabs.Length; i++) tabs[i] = T(TabKeys[i]);
            int tab = GUILayout.Toolbar(_tab, tabs);
            if (tab != _tab) { _tab = tab; _status = ""; _assignFor = 0; }
            if (_status.Length > 0) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            DrawCalls();
            switch (_tab)
            {
                case 0: DrawList(RequestKind.Favor, "ui.req.empty.favors"); break;
                case 1: DrawOrders(); break;
                case 2: DrawSeason(); break;
                case 3: DrawStories(); break;
                default: DrawVows(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---- çağrılar ----

        private void DrawCalls()
        {
            bool any = false;
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Call || !r.IsOpen) continue;
                if (!any) { GUILayout.Label(T("ui.req.calls")); any = true; }
                if (!r.Read) { GUILayout.Label(T("ui.req.call_unread")); continue; }
                GUILayout.Label(T("ui.req.call_line", ("title", R(r.Title)), ("giver", r.GiverName), ("left", R(Req.TimeLeft(r)))));
                GUILayout.Label("   " + R(r.Text));
                if (r.State == RequestState.Accepted)
                {
                    var solver = _life.Population.Find(r.SolverId);
                    string path = T(r.Paths[r.ChosenPath].LabelKey);
                    GUILayout.Label(solver != null ? T("ui.req.call_on_way", ("name", solver.Name), ("path", path)) : T("ui.req.call_on_way_someone", ("path", path)));
                    continue;
                }
                var c = _commander();
                for (int i = 0; i < r.Paths.Count; i++)
                {
                    var p = r.Paths[i];
                    var why = Req.PathBlocker(r, i, c);
                    var cost = new List<string>();
                    if (p.Skill.HasValue) cost.Add(T("ui.req.cost.skill", ("skill", T("skill." + p.Skill.Value)), ("level", p.Level)));
                    if (p.ItemId != null) cost.Add(R(LocText.Of("ui.req.cost.item", ("n", p.ItemCount), ("item", p.ItemId))));
                    if (p.Akce > 0) cost.Add(T("ui.req.cost.akce", ("n", p.Akce)));
                    if (p.Hours > 0) cost.Add(T("ui.req.cost.hours", ("n", (int)Math.Round(p.Hours))));
                    GUI.enabled = why.IsEmpty;
                    string label = cost.Count > 0 ? T("ui.req.path_button", ("path", T(p.LabelKey)), ("cost", string.Join(" · ", cost))) : T(p.LabelKey);
                    if (GUILayout.Button(new GUIContent(label, why.IsEmpty ? T(p.OutcomeKey) : R(why))))
                    {
                        Req.ChoosePath(r.Id, i, c, out var msg);
                        _status = R(msg);
                    }
                    GUI.enabled = true;
                    if (!why.IsEmpty) GUILayout.Label("      " + R(why));
                }
            }
            if (any) GUILayout.Space(6f);
        }

        // ---- ortak satır ----

        private void DrawList(RequestKind kind, string emptyKey)
        {
            int n = 0;
            foreach (var r in Req.All)
            {
                if (r.Kind != kind || !r.IsOpen) continue;
                DrawRequest(r);
                n++;
            }
            if (n == 0) GUILayout.Label(T(emptyKey));
            DrawRecent(kind);
        }

        private void DrawRecent(RequestKind kind)
        {
            bool header = false;
            for (int i = Req.All.Count - 1, shown = 0; i >= 0 && shown < 5; i--)
            {
                var r = Req.All[i];
                if (r.Kind != kind || r.IsOpen) continue;
                if (!header) { GUILayout.Label(T("ui.req.history")); header = true; }
                GUILayout.Label(T("ui.req.history_line", ("state", T(StateKey(r.State))), ("title", R(r.Title)), ("giver", r.GiverName)));
                shown++;
            }
        }

        private static string StateKey(RequestState s) => "ui.req.state." + s.ToString().ToLowerInvariant();

        private void DrawRequest(Request r)
        {
            if (!r.Read)
            {
                GUILayout.Label(T("ui.req.unread", ("kind", GameLocalization.Current.ToLower(T(Request.KindKey(r.Kind))))));
                return;
            }
            var tags = new List<string> { r.GiverName };
            if (r.Master) tags.Add(T("ui.req.tag.master"));
            if (r.Stretch) tags.Add(T("ui.req.tag.stretch"));
            if (r.Skill.HasValue) tags.Add(T("ui.req.cost.skill", ("skill", T("skill." + r.Skill.Value)), ("level", r.Level)));
            GUILayout.Label(UiSkin.C(" " + R(r.Title) + " · " + string.Join(" · ", tags), IconKey(r.Kind)));
            string text = R(r.Text);
            if (!string.IsNullOrEmpty(text)) GUILayout.Label("   " + text);
            string state = T(StateKey(r.State));
            string left = R(Req.TimeLeft(r));
            GUILayout.Label(r.Kind == RequestKind.Seasonal
                ? T("ui.req.status_days", ("state", state), ("left", left), ("from", r.DayFrom), ("to", r.DayTo))
                : T("ui.req.status", ("state", state), ("left", left)));
            foreach (var o in r.CurrentObjectives) GUILayout.Label("   ○ " + R(Req.ObjectiveText(o)));
            var reward = r.Kind == RequestKind.Vow && r.Stage < r.Stages.Count ? r.Stages[r.Stage].Reward : r.Reward;
            GUILayout.Label(T("ui.req.reward", ("reward", R(Req.RewardSummary(reward)))));
            if (r.AssignedTo != 0)
            {
                var who = _life.Population.Find(r.AssignedTo);
                if (who != null) GUILayout.Label(T(who.Away ? "ui.req.assigned_away" : "ui.req.assigned", ("name", who.Name)));
            }
            DrawButtons(r);
            if (_assignFor == r.Id) DrawAssign(r);
            GUILayout.Space(4f);
        }

        /// <summary>Tür ikonu (Resources/UI/Icons/ui_icon_request_*.png, PixelLab işi 480); yoksa yalnız yazı.</summary>
        public static string IconKey(RequestKind k) => "request_" + k.ToString().ToLowerInvariant();

        private bool HasDeliverable(Request r)
        {
            foreach (var o in r.CurrentObjectives)
                if (!o.Done && (o.Kind == ObjectiveKind.Deliver || o.Kind == ObjectiveKind.Pay)) return true;
            return false;
        }

        private void DrawButtons(Request r)
        {
            var c = _commander();
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            if (r.State == RequestState.Posted)
            {
                var why = Req.CanAccept(r);
                GUI.enabled = why.IsEmpty;
                if (GUILayout.Button(new GUIContent(T("ui.req.accept"), R(why)), GUILayout.Width(80f))) { Req.Accept(r.Id, out var m); _status = R(m); }
                GUI.enabled = true;
            }
            if (HasDeliverable(r))
            {
                if (GUILayout.Button(T("ui.req.deliver"), GUILayout.Width(80f))) { Req.Deliver(r.Id, c, false, out var m); _status = R(m); }
                var giver = _life.Population.Find(r.GiverId);
                bool courier = giver != null && !giver.IsDead && r.Kind != RequestKind.Vow && r.Kind != RequestKind.Bequest && r.Kind != RequestKind.Seasonal;
                if (courier && GUILayout.Button(T("ui.req.courier", ("fee", RequestService.CourierFee)), GUILayout.Width(110f))) { Req.Deliver(r.Id, c, true, out var m); _status = R(m); }
            }
            if (r.Kind != RequestKind.Vow && r.Kind != RequestKind.Call && GUILayout.Button(T(_assignFor == r.Id ? "ui.req.assign_open" : "ui.req.assign"), GUILayout.Width(50f)))
                _assignFor = _assignFor == r.Id ? 0 : r.Id;
            if (ShowOnMap != null && r.GiverId != 0)
            {
                var giver = _life.Population.Find(r.GiverId);
                if (giver != null && !giver.IsDead && GUILayout.Button(T("ui.req.on_map"), GUILayout.Width(70f))) ShowOnMap(giver);
            }
            if (GUILayout.Button(T(r.Kind == RequestKind.Vow ? "ui.req.drop_vow" : "ui.req.drop"), GUILayout.Width(r.Kind == RequestKind.Vow ? 96f : 50f)))
            {
                Req.Drop(r.Id, out var m);
                _status = R(m);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAssign(Request r)
        {
            GUILayout.Label(T("ui.req.assign_help"));
            foreach (var m in Req.Household())
            {
                if (m.Stage == AgeStage.Baby) continue;
                string duty = null;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Done) continue;
                    var order = Req.DutyFor(r, o, m);
                    if (order != null) { duty = R(Thatchwick.Core.Work.TimedDutyService.PlaceText(order.PlaceId)); break; }
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(18f);
                GUI.enabled = !m.Away;
                string label = duty != null ? T("ui.req.assign_to_place", ("name", m.Name), ("age", AgeStages.Name(m.Stage)), ("place", duty))
                                            : T("ui.req.assign_by_hand", ("name", m.Name), ("age", AgeStages.Name(m.Stage)));
                if (GUILayout.Button(label, GUILayout.Width(300f)))
                {
                    Req.Assign(r.Id, m, out var msg);
                    _status = R(msg);
                    _assignFor = 0;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        // ---- siparişler ----

        private void DrawOrders()
        {
            int accepted = 0;
            foreach (var r in Req.All) if (r.Kind == RequestKind.Order && r.State == RequestState.Accepted) accepted++;
            GUILayout.Label(T("ui.req.orders_help", ("n", accepted), ("max", Req.MaxOrders)));
            DrawList(RequestKind.Order, "ui.req.empty.orders");
            GUILayout.Label(T("ui.req.ranking"));
            int i = 1;
            foreach (var row in Req.Ranking(T("ui.req.your_house")))
            {
                GUILayout.Label(T(row.IsPlayer ? "ui.req.rank_line_you" : "ui.req.rank_line", ("i", i), ("name", row.Name), ("points", row.Points)));
                if (++i > 8) break;
            }
        }

        // ---- mevsim ----

        private void DrawSeason()
        {
            int done = 0, total = 0;
            double seasonStart = Req.SeasonIndex * (double)Req.DaysPerSeason;
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Seasonal || r.PostedDay < seasonStart - 1e-6) continue;
                total++;
                if (r.State == RequestState.Done) done++;
            }
            GUILayout.Label(T("ui.req.season_header", ("day", Req.DayOfSeason), ("done", done), ("total", total), ("min", RequestService.SeasonSetJobs)));
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Seasonal || !r.IsOpen) continue;
                int pct = (int)Math.Round(Req.VillageProgress(r) * 100);
                string when = Req.InWindow(r) ? T("ui.req.when.now") : Req.DayOfSeason < r.DayFrom ? T("ui.req.when.from", ("day", r.DayFrom)) : T("ui.req.when.past");
                GUILayout.Label(T("ui.req.season_line", ("title", R(r.Title)), ("pct", pct), ("when", when)));
                GUILayout.Label("   " + R(r.Text));
                foreach (var o in r.Objectives) GUILayout.Label("   ○ " + R(Req.ObjectiveText(o)));
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUI.enabled = Req.InWindow(r);
                if (HasDeliverable(r) && GUILayout.Button(T("ui.req.deliver"), GUILayout.Width(80f))) { Req.Deliver(r.Id, _commander(), false, out var m); _status = R(m); }
                GUI.enabled = true;
                if (GUILayout.Button(T(_assignFor == r.Id ? "ui.req.assign_open" : "ui.req.assign"), GUILayout.Width(50f))) _assignFor = _assignFor == r.Id ? 0 : r.Id;
                GUILayout.EndHorizontal();
                if (_assignFor == r.Id) DrawAssign(r);
            }
            DrawRecent(RequestKind.Seasonal);
        }

        // ---- hikâyeler ve vasiyetler ----

        private void DrawStories()
        {
            GUILayout.Label(T("ui.req.stories_help"));
            DrawList(RequestKind.Bequest, "ui.req.empty.bequests");
            DrawList(RequestKind.Story, "ui.req.empty.stories");
        }

        // ---- adaklar ----

        private void DrawVows()
        {
            GUILayout.Label(T("ui.req.vows_help"));
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Vow || !r.IsOpen) continue;
                if (r.State == RequestState.Posted)
                {
                    GUILayout.Label(T("ui.req.vow_wish", ("title", R(r.Title)), ("name", r.VowedByName)));
                    GUILayout.Label("   " + R(r.Text));
                    DrawVowButton(r.TemplateId);
                    continue;
                }
                GUILayout.Label(T("ui.req.vow_line", ("title", R(r.Title)), ("name", r.VowedByName), ("stage", r.Stage + 1), ("stages", r.Stages.Count)));
                for (int i = 0; i < r.Stages.Count; i++)
                    GUILayout.Label($"   {(i < r.Stage ? "✓" : i == r.Stage ? ">" : "·")} {T(r.Stages[i].TitleKey)}");
                foreach (var o in r.CurrentObjectives) GUILayout.Label("      ○ " + R(Req.ObjectiveText(o)));
                DrawButtons(r);
            }
            GUILayout.Label(T("ui.req.vows_available"));
            foreach (var t in Req.AvailableVows())
            {
                GUILayout.Label(T("ui.req.vow_offer", ("title", T(RequestKeys.Title(t.Id))), ("text", T(RequestKeys.Text(t.Id))), ("stages", t.Stages.Count), ("seals", t.FinalSeals)));
                DrawVowButton(t.Id);
            }
        }

        private void DrawVowButton(string templateId)
        {
            var c = _commander();
            var why = Req.CanVow(templateId, c);
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUI.enabled = why.IsEmpty;
            if (GUILayout.Button(new GUIContent(c != null ? T("ui.req.vow_by", ("name", c.Name)) : T("ui.req.vow"), R(why)), GUILayout.Width(140f)))
            {
                Req.Vow(templateId, c, out var m);
                _status = R(m);
            }
            GUI.enabled = true;
            if (!why.IsEmpty) GUILayout.Label(R(why));
            GUILayout.EndHorizontal();
        }
    }
}
