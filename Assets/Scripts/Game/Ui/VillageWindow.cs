using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Köy penceresi (V; UI Toolkit, <see cref="ListWindow"/>; eski IMGUI VillagePanel'in yerine, belge 66g): köylüler (hediye,
    /// ruh hali, teselli, ortak kayıp, kapıyı çalıp çağırma), komşu haneler, köy projeleri, demirci, meslek dalı, hedefler, soy,
    /// şenlik ürün yarışmaları. Kurallar Core.Life'taki servislerde; burası yalnızca çağırır ve nedeni gösterir.
    /// </summary>
    public sealed class VillageWindow
    {
        private static readonly string[] TabKeys =
        {
            "ui.villagew.tab.villagers", "ui.villagew.tab.houses", "ui.villagew.tab.projects", "ui.villagew.tab.smith",
            "ui.villagew.tab.branches", "ui.villagew.tab.goals", "ui.villagew.tab.lineage", "ui.show.tab"
        };

        private readonly LifeSystems _life;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly FamilyService _family;
        private readonly Dynasty _dynasty;
        private readonly Func<Character> _commander;
        private readonly Func<UiHost> _host;
        private ListWindow _window;
        private int _tab;
        private int _openHouse = -1;
        private string _status = "";
        private string _giftTo;

        /// <summary>Kapıda iş (köylü, 1 teselli, 2 ortak kayıp, 3 çağır): oyun katmanı komutanı kapıya yürütür; dönen metin durum satırı.</summary>
        public Func<Character, int, string> Door { get; set; }
        /// <summary>Pencere açılmadan önce (altta kalacak IMGUI panellerini kapatmak için).</summary>
        public event Action Opening;

        public VillageWindow(LifeSystems life, ContentDatabase content, Household house, FamilyService family, Dynasty dynasty, Func<Character> commander, Func<UiHost> host)
        {
            _life = life; _content = content; _house = house; _family = family; _dynasty = dynasty; _commander = commander; _host = host;
        }

        public bool Active => _window != null && _window.Visible;

        public void Toggle()
        {
            if (Active) { Close(); return; }
            var host = _host?.Invoke();
            if (host == null) return;
            if (_window == null) _window = new ListWindow(host, Build, () => T("ui.common.close"), 640f, 7);
            Opening?.Invoke();
            _status = ""; _giftTo = null;
            _window.Visible = true;
            _window.Refresh();
        }

        public void Close() { if (_window != null) _window.Visible = false; }
        public void Refresh() { if (Active) _window.Refresh(); }

        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string, object)[] args) => GameLocalization.T(key, args);
        private string ItemName(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        private ListWindow.Model Build()
        {
            var m = new ListWindow.Model { Title = T("ui.villagew.title", ("status", _life.Projects.VillageStatus)), Status = _status };
            var tabs = m.AddTabRow();
            for (int i = 0; i < TabKeys.Length; i++)
            {
                int index = i;
                tabs.Add(new ListWindow.Act(T(TabKeys[i]), () => { _tab = index; _status = ""; _giftTo = null; }, on: _tab == i));
            }
            switch (_tab)
            {
                case 0: Villagers(m); break;
                case 1: Houses(m); break;
                case 2: Projects(m); break;
                case 3: Smith(m); break;
                case 4: Branches(m); break;
                case 5: Goals(m); break;
                case 6: Lineage(m); break;
                default: Shows(m); break;
            }
            return m;
        }

        // ---- köylüler ----

        private void Villagers(ListWindow.Model m)
        {
            var giver = _commander();
            if (_giftTo != null && giver != null) { GiftPicker(m, giver); return; }
            m.Info = T("ui.villagew.villagers_help");
            var lv = _life.Living;
            foreach (var s in _life.Villagers.States)
            {
                var c = _life.Population.Find(s.CharacterId);
                if (c == null || !_content.Villagers.TryGet(s.DefId, out var def)) continue;
                string state = T(c.IsDead ? "ui.villagew.state.dead" : c.IsVillager ? "ui.villagew.state.village" : "ui.villagew.state.household");
                var row = new ListWindow.Row(T("ui.villagew.villager", ("name", c.Name), ("role", def.RoleName), ("state", state)));
                m.Rows.Add(row);
                if (c.IsDead) continue;
                int aff = giver != null ? _family.Affection(giver.Id, c.Id) : 0;
                var detail = new List<string> { T("ui.villagew.affection", ("n", aff), ("seen", s.EventsSeen.Count), ("total", def.Events.Count)) };
                if (giver != null && VillagerService.CanRomance(giver, c)) detail.Add(T("ui.villagew.romance", ("n", s.Romance), ("max", VillagerService.MaxRomance)));
                if (c.IsVillager)
                {
                    var tier = LivingVillage.MoodTier(c);
                    string mood = T(tier == VillageMood.Cheerful ? "ui.village.mood.cheerful" : tier == VillageMood.Gloomy ? "ui.village.mood.gloomy" : "ui.village.mood.neutral");
                    detail.Add(T("ui.villagew.mood_place", ("mood", mood), ("where", T(LivingVillage.PlaceTextKey(lv.PlaceFor(c))))));
                    if (c.InBreakdown) detail.Add(T("ui.villagew.breakdown"));
                    if (lv.IsSick(c)) detail.Add(T("ui.villagew.sick"));
                }
                row.Detail = string.Join(" · ", detail);
                if (!c.IsVillager || giver == null) continue;

                bool giftedToday = s.LastGiftDay == _life.Clock.DayIndex;
                string defId = s.DefId;
                row.Actions.Add(new ListWindow.Act(T(giftedToday ? "ui.villagew.gifted_today" : "ui.villagew.give"), () => _giftTo = defId,
                    disabled: giftedToday || FirstGiftable() == null));
                if (Door == null) continue;
                if (c.InBreakdown)
                {
                    row.Actions.Add(new ListWindow.Act(T("ui.village.console"), () => _status = Door(c, 1) ?? _status));
                    string lost = lv.RecentLoss(giver);
                    if (lost != null) row.Actions.Add(new ListWindow.Act(T("ui.village.share_loss", ("name", lost)), () => _status = Door(c, 2) ?? _status));
                }
                else
                {
                    var shop = lv.ShopOf(c);
                    if (shop != null && !lv.IsShopOpen(shop, c)) row.Actions.Add(new ListWindow.Act(T("ui.village.call"), () => _status = Door(c, 3) ?? _status));
                }
            }
        }

        private void GiftPicker(ListWindow.Model m, Character giver)
        {
            var c = _life.Villagers.CharacterOf(_giftTo);
            m.Info = T("ui.villagew.gift_pick", ("name", c?.Name ?? ""));
            m.Footer.Add(new ListWindow.Act(T("ui.villagew.back"), () => _giftTo = null));
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var item) || item.Placement != ItemPlacement.Resource) continue;
                string id = kv.Key;
                var row = new ListWindow.Row(T("ui.villagew.gift_item", ("item", item.DisplayName), ("n", kv.Value)));
                row.Click = () =>
                {
                    _life.Villagers.Give(giver, _giftTo, id, out var msg);
                    _status = msg;
                    _giftTo = null;
                };
                m.Rows.Add(row);
            }
        }

        private string FirstGiftable()
        {
            foreach (var kv in _house.InventoryList())
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var item) && item.Placement == ItemPlacement.Resource) return kv.Key;
            return null;
        }

        // ---- komşu hanedanlar ----

        private void Houses(ListWindow.Model m)
        {
            var svc = _life.Houses;
            int? founder = _dynasty.History.Count > 0 ? _dynasty.History[0].CharacterId : (int?)null;
            // İlan sıralaması (belge 46 §8.5): panodan alınan siparişlerin puanı; rakip haneler alınmayan siparişi üstlenir.
            var rank = _life.Requests.Ranking(_dynasty.Name);
            var top = new List<string>();
            for (int i = 0; i < rank.Count && i < 5; i++)
                top.Add(T(rank[i].IsPlayer ? "ui.req.rank_short_you" : "ui.req.rank_short", ("i", i + 1), ("name", rank[i].Name), ("points", rank[i].Points)));
            m.Info = T("ui.villagew.houses_help") + "\n" + T("ui.req.rank_inline", ("list", string.Join(" · ", top)));
            foreach (var h in svc.Houses)
            {
                if (h.Extinct && _openHouse != h.Id) continue;
                var head = _life.Population.Find(h.HeadId);
                int ours = 0;
                foreach (var p in svc.LivingMembers(h)) if (founder.HasValue && _life.Legacy.IsDescendantOf(p, founder.Value)) ours++;
                string headText = head != null && !head.IsDead ? T("ui.villagew.head", ("name", head.Name), ("stage", AgeStages.Name(head.Stage))) : T("ui.villagew.headless");
                var detail = new List<string> { T("ui.villagew.house_counts", ("people", svc.LivingCount(h)), ("gens", svc.Generations(h)), ("heads", h.HeadCount)) };
                int memory = _life.Requests.Memory.Of(h.Id);
                if (memory != 0) detail.Add(T("ui.villagew.memory", ("n", (memory > 0 ? "+" : "") + memory)));
                if (ours > 0) detail.Add(T("ui.villagew.ours", ("n", ours)));
                if (h.Extinct) detail.Add(T("ui.villagew.extinct"));
                int id = h.Id;
                var row = new ListWindow.Row(h.Name + " · " + headText, string.Join(" · ", detail))
                {
                    Marked = _openHouse == h.Id,
                    Click = () => _openHouse = _openHouse == id ? -1 : id
                };
                m.Rows.Add(row);
                if (_openHouse != h.Id) continue;
                foreach (var n in svc.TreeOf(h))
                {
                    var p = n.Person;
                    string indent = new string(' ', (n.Depth - 1) * 3);
                    string state = p.IsDead ? T("ui.villagew.state.dead") : AgeStages.Name(p.Stage);
                    string line = indent + (n.IsHead ? "★ " : "") + p.Name + " (" + state + ")" + (n.Spouse != null ? " ∞ " + n.Spouse.Name : "");
                    if (founder.HasValue && _life.Legacy.IsDescendantOf(p, founder.Value)) line += " · " + T("ui.villagew.your_line");
                    m.Rows.Add(new ListWindow.Row("  " + line));
                }
                if (h.Heads.Count > 1)
                {
                    var names = new List<string>();
                    foreach (var r in h.Heads) names.Add(r.Name);
                    m.Rows.Add(new ListWindow.Row("  " + T("ui.villagew.heads", ("list", string.Join(" > ", names)))));
                }
            }
        }

        // ---- projeler ----

        private void Projects(ListWindow.Model m)
        {
            foreach (var p in _content.Projects.All)
            {
                bool done = _life.Projects.IsDone(p.Id);
                var row = new ListWindow.Row((done ? "✓ " : "") + p.DisplayName, p.Description) { Marked = done };
                m.Rows.Add(row);
                if (done) continue;
                var costs = new List<string> { T("ui.villagew.akce", ("n", p.Akce)) };
                foreach (var c in p.Costs)
                    costs.Add(ItemName(c.ItemId) + " " + _house.Count(c.ItemId) + "/" + c.Count + (_house.Count(c.ItemId) >= c.Count ? " ✓" : ""));
                row.Detail += "\n" + string.Join(" · ", costs);
                string why = _life.Projects.CanBuild(p.Id);
                if (why != null) row.Detail += "\n" + why;
                string pid = p.Id, pname = p.DisplayName;
                row.Actions.Add(new ListWindow.Act(T("ui.villagew.build"), () =>
                {
                    _life.Projects.Build(pid, out var reason);
                    _status = reason ?? T("ui.villagew.built", ("name", pname));
                }, disabled: why != null));
            }
        }

        // ---- demirci ----

        private void Smith(ListWindow.Model m)
        {
            m.Info = T("ui.villagew.smith_help");
            foreach (var tool in _content.Tools.All)
            {
                int tier = _life.Tools.Tier(tool.Id);
                string id = tool.Id;
                // Tırpanın taban kademesi yoktur: alınana kadar "yok" yazar (öteki aletlerin taban hâli hanede hep vardır).
                string title = tier == 0 && tool.Id == Thatchwick.Core.Work.HarvestOrders.ScytheTool
                    ? T("ui.villagew.tool_none", ("tool", tool.DisplayName), ("none", T("ui.farm.tool.none")), ("tier", tier), ("max", tool.Tiers.Count))
                    : T("ui.villagew.tool", ("tool", tool.DisplayName), ("name", _life.Tools.NameOf(id)), ("speed", _life.Tools.SpeedFor(id).ToString("0.0#")), ("tier", tier), ("max", tool.Tiers.Count));
                var row = new ListWindow.Row(title);
                var detail = new List<string>();
                var heir = _life.Legacy.ToolHeirloom(id);
                if (heir != null)
                    detail.Add(T("ui.legacy.heir_line", ("name", heir.DisplayName), ("pct", (Legacy.BonusOf(heir) * 100).ToString("0")), ("level", heir.Level)));
                else
                {
                    string hw = _life.Legacy.CanMakeToolHeirloom(id, _life.Tools.NameOf(id));
                    row.Actions.Add(new ListWindow.Act(T("ui.villagew.heirloom", ("cost", Legacy.ToolHeirloomCost)), () =>
                    {
                        _life.Legacy.MakeToolHeirloom(id, _life.Tools.NameOf(id), _commander(), _life.Clock.TotalDays, out var hm);
                        _status = hm;
                    }, disabled: hw != null));
                    if (hw != null) detail.Add(hw);
                }
                var next = _life.Tools.Next(id);
                if (next != null)
                {
                    string why = _life.Tools.CanUpgrade(id);
                    detail.Add(T("ui.villagew.next_tier", ("name", next.Name), ("speed", next.Speed.ToString("0.0#")), ("akce", next.Akce), ("wood", next.Firewood), ("level", next.Level)));
                    if (why != null) detail.Add(why);
                    string nextName = next.Name;
                    row.Actions.Add(new ListWindow.Act(T("ui.villagew.upgrade"), () =>
                    {
                        _life.Tools.Upgrade(id, out var reason);
                        _status = reason ?? T("ui.villagew.upgraded", ("name", nextName));
                    }, disabled: why != null));
                }
                row.Detail = string.Join("\n", detail);
                m.Rows.Add(row);
            }
        }

        // ---- meslek dalları ----

        private void Branches(ListWindow.Model m)
        {
            var c = _commander();
            if (c == null) { m.Info = T("ui.villagew.no_commander"); return; }
            m.Info = T("ui.villagew.branches_help", ("name", c.Name));
            foreach (var id in c.BranchIds)
                if (_content.Branches.TryGet(id, out var b)) m.Rows.Add(new ListWindow.Row("✓ " + b.DisplayName, b.Description) { Marked = true });
            var open = LifeRules.AvailableBranches(_content, c);
            if (open.Count == 0) m.Rows.Add(new ListWindow.Row(T("ui.villagew.no_branch")));
            foreach (var b in open)
            {
                var branch = b;
                var row = new ListWindow.Row(T("ui.villagew.branch", ("name", b.DisplayName), ("level", b.Level)), b.Description);
                row.Actions.Add(new ListWindow.Act(T("ui.villagew.choose"), () =>
                {
                    LifeRules.ChooseBranch(_content, c, branch.Id, out var reason, _life.Village);
                    _status = reason ?? T("ui.villagew.chosen", ("name", c.Name), ("branch", branch.DisplayName));
                    _life.Goals.Update();
                }));
                m.Rows.Add(row);
            }
        }

        // ---- hedefler ----

        private void Goals(ListWindow.Model m)
        {
            foreach (var g in _content.Goals.All)
            {
                bool done = _life.Goals.IsDone(g.Id);
                int progress = _life.Goals.Progress(g);
                var reward = new List<string>();
                if (g.RewardAkce > 0) reward.Add(T("ui.villagew.akce", ("n", g.RewardAkce)));
                if (g.RewardXp > 0) reward.Add(T("ui.goal.reward_xp", ("n", g.RewardXp)));
                string title = (done ? "✓ " : "") + g.DisplayName + " · " + progress + "/" + g.Target
                    + (reward.Count > 0 ? " · " + T("ui.villagew.reward", ("list", string.Join(" + ", reward))) : "");
                m.Rows.Add(new ListWindow.Row(title, done ? null : g.Description) { Marked = done });
            }
        }

        // ---- soy ve mezarlar ----

        private void Lineage(ListWindow.Model m)
        {
            string trade = _life.FamilyTrade();
            m.Info = T("ui.villagew.dynasty", ("name", _dynasty.Name), ("gen", _dynasty.CurrentGeneration)) + (trade != null ? " · " + trade : "");
            m.Rows.Add(new ListWindow.Row(T("ui.villagew.heads_title")) { Header = true });
            foreach (var h in _dynasty.History)
                m.Rows.Add(new ListWindow.Row(T("ui.villagew.head_line", ("gen", h.Generation), ("name", h.Name), ("from", h.StartDay.ToString("0")),
                    ("to", h.EndDay >= 0 ? h.EndDay.ToString("0") : "")) + (string.IsNullOrEmpty(h.Trade) ? "" : " (" + h.Trade + ")")));
            var lg = _life.Legacy;
            int? founder = _dynasty.History.Count > 0 ? _dynasty.History[0].CharacterId : (int?)null;
            m.Rows.Add(new ListWindow.Row(T("ui.villagew.seals", ("n", lg.Seals))) { Header = true });
            for (int i = Math.Max(0, lg.Ledger.Count - 6); i < lg.Ledger.Count; i++)
            {
                var e = lg.Ledger[i];
                m.Rows.Add(new ListWindow.Row((e.Amount > 0 ? "+" : "") + e.Amount + " · " + T("ui.villagew.day", ("n", e.Day.ToString("0"))) + " · " + e.Note));
            }
            m.Rows.Add(new ListWindow.Row(T("ui.villagew.heirlooms")) { Header = true });
            if (lg.Heirlooms.Count == 0) m.Rows.Add(new ListWindow.Row(T("ui.villagew.no_heirloom")));
            foreach (var h in lg.Heirlooms)
                m.Rows.Add(new ListWindow.Row("✦ " + T(h.Kind == HeirloomKind.Item ? "ui.villagew.heirloom_item" : "ui.villagew.heirloom_tool",
                    ("name", h.DisplayName), ("level", h.Level), ("pct", (Legacy.BonusOf(h) * 100).ToString("0")), ("comfort", Legacy.ComfortOf(h)))));
            m.Rows.Add(new ListWindow.Row(T("ui.villagew.line_in_village", ("n", founder.HasValue ? lg.LivingDescendantsOutside(founder.Value) : 0))) { Header = true });
            foreach (var mo in lg.MarriedOut)
                m.Rows.Add(new ListWindow.Row(T("ui.villagew.married_out", ("name", mo.Name), ("spouse", mo.SpouseName), ("day", mo.Day.ToString("0")))));
            m.Rows.Add(new ListWindow.Row(T("ui.villagew.graves")) { Header = true });
            if (_life.Graves.Graves.Count == 0) m.Rows.Add(new ListWindow.Row(T("ui.villagew.no_graves")));
            foreach (var g in _life.Graves.Graves)
                m.Rows.Add(new ListWindow.Row(T("ui.villagew.grave", ("name", g.Name), ("gen", g.Generation), ("age", g.AgeDays.ToString("0")), ("cause", CauseText(g.Cause)))));
        }

        private static string CauseText(DeathCause c)
        {
            switch (c)
            {
                case DeathCause.Cold: return T("ui.villagew.cause.cold");
                case DeathCause.Starvation: return T("ui.villagew.cause.starvation");
                case DeathCause.OldAge: return T("ui.villagew.cause.old_age");
                default: return "";
            }
        }

        // ---- şenlik ürün yarışmaları (belge 57) ----

        private void Shows(ListWindow.Model m)
        {
            var svc = _life.Shows;
            m.Info = T("ui.show.help");
            int year = svc.Year?.Invoke() ?? 0;
            foreach (var show in ProduceShowService.Shows)
            {
                var sh = show;
                string fest = _content.Festivals.TryGet(show.FestivalId, out var f) ? f.DisplayName : show.FestivalId;
                var row = new ListWindow.Row(T("ui.show.line", ("show", GameLocalization.R(show.Name)), ("festival", fest), ("wins", svc.Wins(show.Id))));
                bool on = svc.Participating(show.Id);
                row.Actions.Add(new ListWindow.Act(T(on ? "ui.show.participating" : "ui.show.not_participating"), () => svc.SetParticipating(sh.Id, !svc.Participating(sh.Id)), on: on));
                var entries = svc.PlannedEntries(show);
                if (on && show.Kind == ProduceShowKind.Tasting)
                {
                    var cands = svc.Candidates(show);
                    if (cands.Count > 1)
                        row.Actions.Add(new ListWindow.Act(T("ui.show.change"), () =>
                        {
                            var now = svc.PlannedEntries(sh);
                            int i = now.Count > 0 ? cands.FindIndex(c => c.Id == now[0].Id) : -1;
                            svc.Choose(sh.Id, cands[(i + 1) % cands.Count]);
                        }));
                }
                if (on)
                {
                    if (entries.Count == 0) row.Detail = T("ui.show.no_entry");
                    else
                    {
                        var names = new List<string>();
                        foreach (var e in entries) names.Add(UiSkin.QualityTag(e.Quality) + ItemName(e.Id));
                        double mine = svc.EntryScore(show, entries), rivals = ProduceShowService.RivalBase(show.Kind, year);
                        row.Detail = string.Join(", ", names) + "\n" + T("ui.show.score", ("score", (int)Math.Round(mine)), ("rivals", (int)Math.Round(rivals)));
                    }
                }
                m.Rows.Add(row);
            }
        }
    }
}
