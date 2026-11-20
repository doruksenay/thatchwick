using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Relations;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// İlişki paneli (UI Toolkit; Project belgeleri 67, 67a-d): kişiler (aşama, romantik, durum, uyum, aile türü, sıcaklık,
    /// hisler, gönül tercihleri ve sosyal işler), köyde konuşulanlar, itibar (dört yön, lakaplar, söylenti hazırlama),
    /// ihtiyar heyeti, bekleyen kararlar ve muhabbet seçimleri.
    /// </summary>
    public sealed class RelationsPanel
    {
        private enum Tab { People, Talk, Rep, Council, Decide }

        private readonly LifeSystems _life;
        private readonly Func<UiHost> _host;
        private ListWindow _window;
        private Tab _tab;
        private int _who;
        private string _status;

        /// <summary>Hane üyeleri (seçilebilecek "kimin gözünden").</summary>
        public Func<IEnumerable<Character>> Household { get; set; }
        public Func<Character> Selected { get; set; }
        public Func<Character, JobRunner> RunnerOf { get; set; }
        public event Action Opening;

        public RelationsPanel(LifeSystems life, Func<UiHost> host)
        {
            _life = life; _host = host;
        }

        public bool Active => _window != null && _window.Visible;

        public void Toggle()
        {
            if (Active) { Close(); return; }
            var host = _host?.Invoke();
            if (host == null) return;
            if (_window == null) _window = new ListWindow(host, Build, () => T("ui.rel.close"), 640f, 6);
            Opening?.Invoke();
            _status = null;
            var sel = Selected?.Invoke();
            if (sel != null && !sel.IsVillager) _who = sel.Id;
            _window.Visible = true;
        }

        public void Close() { if (_window != null) _window.Visible = false; }
        public void Refresh() => _window?.Refresh();

        private static string T(string key, params (string, object)[] args) => GameLocalization.T(key, args);
        private string R(LocText t) => GameLocalization.R(t);

        private List<Character> Members()
        {
            var list = new List<Character>();
            if (Household != null) foreach (var c in Household()) if (!c.IsDead && c.Stage != AgeStage.Baby) list.Add(c);
            return list;
        }

        private ListWindow.Model Build()
        {
            var m = new ListWindow.Model { Title = T("ui.rel.title"), Status = _status };
            var tabs = m.AddTabRow();
            void TabAct(Tab t, string key) => tabs.Add(new ListWindow.Act(T(key), () => { _tab = t; _status = null; }, _tab == t));
            TabAct(Tab.People, "ui.rel.tab.people");
            TabAct(Tab.Talk, "ui.rel.tab.talk");
            TabAct(Tab.Rep, "ui.rel.tab.rep");
            TabAct(Tab.Council, "ui.rel.tab.council");
            TabAct(Tab.Decide, "ui.rel.tab.decide");

            var members = Members();
            var who = members.Find(c => c.Id == _who) ?? (members.Count > 0 ? members[0] : null);
            if (who != null)
            {
                _who = who.Id;
                var row = m.AddTabRow();
                foreach (var c in members)
                {
                    var cc = c;
                    row.Add(new ListWindow.Act(c.Name, () => { _who = cc.Id; _status = null; }, c.Id == _who));
                }
            }
            switch (_tab)
            {
                case Tab.People: People(m, who); break;
                case Tab.Talk: Talk(m); break;
                case Tab.Rep: Rep(m, who, members); break;
                case Tab.Council: Council(m, who); break;
                default: Decide(m); break;
            }
            return m;
        }

        // ---- kişiler ----

        private static readonly SocialAction[] RelationActs =
        {
            SocialAction.Apologize, SocialAction.MakeUp, SocialAction.Wink, SocialAction.RomanticCompliment, SocialAction.GiveFlowers,
            SocialAction.HoldHands, SocialAction.Hug, SocialAction.KissCheek, SocialAction.Kiss, SocialAction.GiveHandkerchief,
            SocialAction.AskHand, SocialAction.CelebrateAnniversary, SocialAction.Gossip, SocialAction.DenyRumor, SocialAction.SpreadRumor,
            SocialAction.Lobby, SocialAction.JoinChat, SocialAction.Argue, SocialAction.Jab, SocialAction.Sulk, SocialAction.Fight,
            SocialAction.CaneThreat, SocialAction.ReturnHandkerchief, SocialAction.BreakEngagement
        };

        private void People(ListWindow.Model m, Character who)
        {
            var rel = _life.Relations;
            if (who == null) { m.Info = T("ui.rel.none"); return; }
            m.Info = T("ui.rel.who", ("name", who.Name));
            ChatRow(m, who);
            var others = new List<(Character c, int f)>();
            foreach (var c in _life.Population.Members)
            {
                if (c == who || c.IsDead || c.Stage == AgeStage.Baby) continue;
                int f = rel.Friendship(who.Id, c.Id);
                var p = rel.Find(who.Id, c.Id);
                if (f == 0 && p == null && !RelationService.IsKin(who, c)) continue;
                others.Add((c, f));
            }
            others.Sort((a, b) => b.f.CompareTo(a.f));
            if (others.Count == 0) { m.Rows.Add(new ListWindow.Row(T("ui.rel.none"))); return; }
            foreach (var (c, f) in others) m.Rows.Add(PersonRow(who, c, f));
        }

        private ListWindow.Row PersonRow(Character who, Character c, int f)
        {
            var rel = _life.Relations;
            var p = rel.Find(who.Id, c.Id);
            string title = c.Name + " · " + R(RelationRules.StageText(RelationRules.StageOf(f))) + " (" + f + ")";
            if (p != null && p.Sulking) title += " · " + T("ui.rel.sulking");
            var parts = new List<string>();
            int r = rel.Romance(who.Id, c.Id);
            if (r != 0) parts.Add(T("ui.rel.romance", ("stage", R(RelationRules.RomanceText(RelationRules.RomanceStageOf(r)))), ("n", r)));
            var status = rel.Status(who.Id, c.Id);
            if (status != CoupleStatus.None) parts.Add(T("ui.rel.status", ("status", R(RelationRules.StatusText(status)))));
            parts.Add(T("ui.rel.compat", ("c", R(RelationRules.CompatText(RelationRules.Compatibility(who, c))))));
            var dyn = rel.DynamicOf(who.Id, c.Id);
            if (dyn != FamilyDynamic.None) parts.Add(T("ui.rel.dynamic", ("d", R(RelationRules.DynamicText(dyn)))));
            int w = rel.Warmth(who.Id, c.Id);
            if (w >= 0 && who.SpouseId == c.Id) parts.Add(T("ui.rel.warmth", ("n", w)));
            var feels = new List<string>();
            foreach (var x in rel.Feelings(who.Id, c.Id)) feels.Add(R(LocText.Of(RelationRules.Feelings[x.Kind].Key)));
            if (feels.Count > 0) parts.Add(T("ui.rel.feels", ("list", string.Join(", ", feels))));
            int known = rel.KnownPrefs(who.Id, c.Id);
            if (c.Stage >= AgeStage.YoungAdult)
            {
                if (known >= 3)
                {
                    var pr = rel.PrefsOf(c);
                    parts.Add(T("ui.rel.prefs", ("l1", R(RelationRules.TagText(pr.Like1))), ("l2", R(RelationRules.TagText(pr.Like2))), ("d", R(RelationRules.TagText(pr.Dislike)))));
                }
                else parts.Add(T("ui.rel.prefs_unknown", ("n", known)));
            }
            var row = new ListWindow.Row(title, string.Join(" · ", parts)) { Marked = rel.IsCouple(who.Id, c.Id) };
            // Aile ilişki türü: yalnızca oyunun başında seçilir (karar 67a §8).
            if (rel.CanChooseDynamic(who, c) == null)
            {
                var next = (FamilyDynamic)((int)dyn % 6 + 1);
                var cc = c;
                row.Actions.Add(new ListWindow.Act(T("ui.rel.dyn.choose", ("d", R(RelationRules.DynamicText(next)))), () => rel.ChooseDynamic(who, cc, next)));
            }
            int shown = 0;
            foreach (var act in RelationActs)
            {
                if (shown >= 6 || _life.Relations.CanDo(who, c, act) != null) continue;
                var a = act; var target = c;
                row.Actions.Add(new ListWindow.Act(T("rel.act." + RelationRules.Snake(act.ToString())), () =>
                {
                    RunnerOf?.Invoke(who)?.Command(Job.Social(target.Id, a));
                    _status = who.Name + " → " + target.Name + ": " + T("rel.act." + RelationRules.Snake(a.ToString()));
                }));
                shown++;
            }
            return row;
        }

        private void ChatRow(ListWindow.Model m, Character who)
        {
            var chats = _life.Chats;
            var circle = chats.CircleOf(who.Id);
            if (circle == null) return;
            var row = new ListWindow.Row(T("ui.rel.chat.title", ("n", circle.Members.Count))) { Header = true };
            foreach (GroupAct act in Enum.GetValues(typeof(GroupAct)))
            {
                if (chats.CanAct(who, act) != null) continue;
                var a = act;
                row.Actions.Add(new ListWindow.Act(T("rel.gact." + RelationRules.Snake(act.ToString())), () => chats.Queue(who, a)));
            }
            m.Rows.Add(row);
        }

        // ---- köyde konuşulanlar ----

        private void Talk(ListWindow.Model m)
        {
            int total = 0;
            foreach (var c in _life.Population.Members) if (!c.IsDead && c.IsVillager && c.Stage >= AgeStage.Child) total++;
            int n = 0;
            foreach (var (fact, knowers) in _life.Knowledge.AboutHouse(0))
            {
                if (knowers == 0) continue;
                string what = R(_life.Knowledge.Describe(fact)) + (fact.Truth ? "" : " " + T("ui.rel.talk.rumor"));
                m.Rows.Add(new ListWindow.Row(T("ui.rel.talk.row", ("what", what), ("n", knowers), ("total", Math.Max(1, total)))) { Marked = fact.Value < 0 });
                n++;
            }
            if (n == 0) m.Info = T("ui.rel.talk.empty");
        }

        // ---- itibar ----

        private void Rep(ListWindow.Model m, Character who, List<Character> members)
        {
            var rep = _life.Upbringing.Reputation;
            m.Info = T("ui.rel.rep.total", ("n", rep.Points), ("tier", rep.TierName));
            foreach (RepFacet f in Enum.GetValues(typeof(RepFacet)))
                m.Rows.Add(new ListWindow.Row(T("ui.rel.rep.facet", ("name", R(RelationRules.FacetText(f))), ("n", rep.Facet((int)f)))));
            foreach (var c in members)
            {
                var nick = rep.Nickname(c.Id);
                if (nick == null) continue;
                string key = "ui.rel.nick." + RelationRules.Snake(((RepFacet)nick.Value.facet).ToString()) + "." + (nick.Value.good ? 1 : 0);
                m.Rows.Add(new ListWindow.Row(T("ui.rel.rep.nick", ("name", c.Name), ("nick", T(key)))));
            }
            if (who == null) return;
            // Söylenti hazırla (rakip haneler, karar 67c): sonra köylüye "Söylenti fısılda".
            foreach (var h in _life.Houses.Houses)
            {
                if (h.Extinct) continue;
                var hh = h;
                var row = new ListWindow.Row(h.Name);
                foreach (RepFacet f in Enum.GetValues(typeof(RepFacet)))
                {
                    var ff = f;
                    row.Actions.Add(new ListWindow.Act(R(RelationRules.FacetText(f)), () =>
                    {
                        _life.Knowledge.PrepareRumor(who, hh.Id, ff);
                        _status = T("ui.rel.rep.prepare", ("house", hh.Name), ("facet", R(RelationRules.FacetText(ff))));
                    }));
                }
                m.Rows.Add(row);
            }
        }

        // ---- heyet ----

        private void Council(ListWindow.Model m, Character who)
        {
            var council = _life.Council;
            var names = new List<string>();
            foreach (var id in council.Members) { var c = _life.Population.Find(id); if (c != null) names.Add(c.Name); }
            m.Info = T("ui.rel.council.members", ("list", names.Count > 0 ? string.Join(", ", names) : "-")) + "  "
                + T(council.PlayerIsMember ? "ui.rel.council.player_member" : "ui.rel.council.player_not") + "  " + T("ui.rel.council.next");
            if (council.ImeceToday) m.Rows.Add(new ListWindow.Row(T("ui.rel.council.imece")) { Header = true });
            if (!string.IsNullOrEmpty(council.Proposal)) m.Rows.Add(new ListWindow.Row(T("ui.rel.council.proposal", ("what", ProjectName(council.Proposal)))) { Marked = true });
            foreach (var p in _life.Content.Projects.All)
            {
                if (_life.Projects.IsDone(p.Id)) continue;
                var id = p.Id;
                var row = new ListWindow.Row(p.DisplayName);
                if (council.PlayerIsMember)
                    row.Actions.Add(new ListWindow.Act(T("ui.rel.council.propose", ("what", p.DisplayName)), () => council.Proposal = id, council.Proposal == id));
                else
                {
                    string why = council.CanPetition();
                    row.Actions.Add(new ListWindow.Act(T("ui.rel.council.petition", ("n", ElderCouncil.PetitionCost), ("what", p.DisplayName)),
                        () => { if (!council.Petition(id)) _status = why; }, false, why != null));
                }
                m.Rows.Add(row);
            }
        }

        private string ProjectName(string id) => _life.Content.Projects.TryGet(id, out var p) ? p.DisplayName : id;

        // ---- kararlar ----

        private void Decide(ListWindow.Model m)
        {
            var rel = _life.Relations;
            if (rel.Decisions.Count == 0) { m.Info = T("ui.rel.decide.empty"); }
            foreach (var d in new List<PendingDecision>(rel.Decisions))
            {
                var a = _life.Population.Find(d.A); var b = _life.Population.Find(d.B);
                string an = a?.Name ?? "?", bn = b?.Name ?? "?";
                string key = d.Kind == DecisionKind.Coffee ? "coffee" : d.Kind == DecisionKind.Separation ? "sep" : "med";
                var row = new ListWindow.Row(T("ui.rel.decide." + key, ("a", an), ("b", bn), ("n", d.Amount))) { Marked = true };
                int options = d.Kind == DecisionKind.Coffee ? 3 : 2;
                for (int i = 0; i < options; i++)
                {
                    int opt = i; int id = d.Id;
                    row.Actions.Add(new ListWindow.Act(T("ui.rel.decide." + key + "." + i), () => rel.Decide(id, opt)));
                }
                m.Rows.Add(row);
            }
            var coming = new List<string>();
            foreach (var e in rel.Events)
            {
                if (e.Done || e.Offscreen) continue;
                var g = _life.Population.Find(e.Groom); var br = _life.Population.Find(e.Bride);
                coming.Add(T("ui.rel.event.item", ("what", T("ui.rel.event." + RelationRules.Snake(e.Kind.ToString()))), ("groom", g?.Name ?? "?"), ("bride", br?.Name ?? "?"),
                    ("days", Math.Max(0, (int)Math.Ceiling(e.StartDay - _life.Clock.TotalDays)))));
            }
            if (coming.Count > 0) m.Rows.Add(new ListWindow.Row(T("ui.rel.events", ("list", string.Join("; ", coming)))) { Header = true });
        }
    }
}
