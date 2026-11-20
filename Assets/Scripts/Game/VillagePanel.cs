using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;

namespace Thatchwick.Game
{
    /// <summary>
    /// Köy paneli (V, geçici IMGUI): köylüler ve hediye, köy projeleri, demirci (alet yükseltme), meslek dalı seçimi,
    /// hanedan hedefleri, soy ve mezarlar. Kurallar Core.Life'taki servislerde; burası yalnızca çağırır ve nedeni gösterir.
    /// </summary>
    public sealed class VillagePanel
    {
        private static readonly string[] Tabs = { "Köylüler", "Haneler", "Proje", "Demirci", "Meslek", "Hedef", "Soy" };

        private readonly LifeSystems _life;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly FamilyService _family;
        private readonly Dynasty _dynasty;
        private readonly Func<Character> _commander;
        private int _tab;
        private int _openHouse = -1;
        private Vector2 _scroll;
        private string _status = "";
        private string _giftTo;

        public bool Active { get; private set; }

        public VillagePanel(LifeSystems life, ContentDatabase content, Household house, FamilyService family, Dynasty dynasty, Func<Character> commander)
        {
            _life = life; _content = content; _house = house; _family = family; _dynasty = dynasty; _commander = commander;
        }

        public void Toggle() { Active = !Active; _status = ""; _giftTo = null; }
        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 368f, 8f, 360f, Screen.height / guiScale - 16f);

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"KÖY · durum {_life.Projects.VillageStatus}");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            var tabs = new string[Tabs.Length + 1];
            Tabs.CopyTo(tabs, 0);
            tabs[Tabs.Length] = GameLocalization.T("ui.show.tab");
            int tab = GUILayout.Toolbar(_tab, tabs);
            if (tab != _tab) { _tab = tab; _status = ""; _giftTo = null; }
            if (_status.Length > 0) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0: DrawVillagers(); break;
                case 1: DrawHouses(); break;
                case 2: DrawProjects(); break;
                case 3: DrawSmith(); break;
                case 4: DrawBranches(); break;
                case 5: DrawGoals(); break;
                case 6: DrawLineage(); break;
                default: DrawShows(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string ItemName(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        // ---- şenlik ürün yarışmaları (belge 57) ----

        private void DrawShows()
        {
            var svc = _life.Shows;
            UiSkin.Wrap(GameLocalization.T("ui.show.help"), 330f);
            int year = svc.Year?.Invoke() ?? 0;
            foreach (var show in ProduceShowService.Shows)
            {
                string fest = _content.Festivals.TryGet(show.FestivalId, out var f) ? f.DisplayName : show.FestivalId;
                GUILayout.Label(GameLocalization.T("ui.show.line", ("show", GameLocalization.R(show.Name)), ("festival", fest), ("wins", svc.Wins(show.Id))));
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                bool on = svc.Participating(show.Id);
                if (GUILayout.Button(GameLocalization.T(on ? "ui.show.participating" : "ui.show.not_participating"), GUILayout.Width(110f)))
                    svc.SetParticipating(show.Id, !on);
                var entries = svc.PlannedEntries(show);
                if (on && show.Kind == ProduceShowKind.Tasting)
                {
                    var cands = svc.Candidates(show);
                    if (cands.Count > 1 && GUILayout.Button(GameLocalization.T("ui.show.change"), GUILayout.Width(70f)))
                    {
                        int i = entries.Count > 0 ? cands.FindIndex(c => c.Id == entries[0].Id) : -1;
                        var next = cands[(i + 1) % cands.Count];
                        svc.Choose(show.Id, next);
                        entries = svc.PlannedEntries(show);
                    }
                }
                GUILayout.EndHorizontal();
                if (!on) continue;
                if (entries.Count == 0) { GUILayout.Label("   " + GameLocalization.T("ui.show.no_entry")); continue; }
                var names = new System.Collections.Generic.List<string>();
                foreach (var e in entries) names.Add(UiSkin.QualityTag(e.Quality) + ItemName(e.Id));
                GUILayout.Label("   " + string.Join(", ", names), UiSkin.Rich);
                double mine = svc.EntryScore(show, entries), rivals = ProduceShowService.RivalBase(show.Kind, year);
                GUILayout.Label("   " + GameLocalization.T("ui.show.score", ("score", (int)System.Math.Round(mine)), ("rivals", (int)System.Math.Round(rivals))));
            }
        }

        // ---- köylüler ----

        private void DrawVillagers()
        {
            var giver = _commander();
            foreach (var s in _life.Villagers.States)
            {
                var c = _life.Population.Find(s.CharacterId);
                if (c == null || !_content.Villagers.TryGet(s.DefId, out var def)) continue;
                string state = c.IsDead ? "öldü" : c.IsVillager ? "köyde" : "hanede";
                GUILayout.Label($"{c.Name} · {def.RoleName} ({state})");
                if (c.IsDead) continue;
                int aff = giver != null ? _family.Affection(giver.Id, c.Id) : 0;
                string romance = giver != null && VillagerService.CanRomance(giver, c) ? $" · romantik {s.Romance}/{VillagerService.MaxRomance}" : "";
                GUILayout.Label($"   dostluk {aff}/100{romance} · olay {s.EventsSeen.Count}/{def.Events.Count}");
                if (!c.IsVillager || giver == null) continue;

                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                bool giftedToday = s.LastGiftDay == _life.Clock.DayIndex;
                GUI.enabled = FirstGiftable() != null && !giftedToday;
                if (GUILayout.Button(_giftTo == s.DefId ? "Hediye seçiliyor…" : "Hediye ver", GUILayout.Width(120f))) _giftTo = _giftTo == s.DefId ? null : s.DefId;
                GUI.enabled = true;
                if (giftedToday) GUILayout.Label("bugün hediye aldı");
                GUILayout.EndHorizontal();

                if (_giftTo == s.DefId)
                    foreach (var kv in _house.InventoryList())
                    {
                        if (kv.Value <= 0 || !_content.Items.TryGet(kv.Key, out var item) || item.Placement != ItemPlacement.Resource) continue;
                        var icon = IsoArt.ItemIcon(kv.Key);
                        if (GUILayout.Button(new GUIContent($"   {item.DisplayName} ({kv.Value})", icon), GUILayout.Width(220f), GUILayout.Height(icon != null ? 28f : 20f)))
                        {
                            _life.Villagers.Give(giver, s.DefId, kv.Key, out var msg);
                            _status = msg;
                            _giftTo = null;
                            break;
                        }
                    }
            }
            GUILayout.Label("İpucu: köylülerin sevdiği ve sevmediği şeyleri hediye verince öğrenirsin. Evlenmek için hem dostluk hem romantik yakınlık 100 olmalı.");
        }

        private string FirstGiftable()
        {
            foreach (var kv in _house.InventoryList())
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var item) && item.Placement == ItemPlacement.Resource) return kv.Key;
            return null;
        }

        // ---- projeler ----

        private void DrawProjects()
        {
            foreach (var p in _content.Projects.All)
            {
                bool done = _life.Projects.IsDone(p.Id);
                GUILayout.Label($"{p.DisplayName}{(done ? " ✓" : "")}");
                GUILayout.Label($"   {p.Description}");
                if (done) continue;
                GUILayout.Label($"   {p.Akce} akçe");
                foreach (var c in p.Costs)   // malzemeler ikonlu satır satır
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12f);
                    IsoArt.IconCell(c.ItemId, 18f);
                    GUILayout.Label($"{ItemName(c.ItemId)} {_house.Count(c.ItemId)}/{c.Count}{(_house.Count(c.ItemId) >= c.Count ? " ✓" : "")}");
                    GUILayout.EndHorizontal();
                }
                string why = _life.Projects.CanBuild(p.Id);
                GUI.enabled = why == null;
                if (GUILayout.Button("Yap", GUILayout.Width(80f)))
                {
                    _life.Projects.Build(p.Id, out var reason);
                    _status = reason ?? $"{p.DisplayName} tamamlandı.";
                }
                GUI.enabled = true;
                if (why != null) GUILayout.Label("   " + why);
            }
        }

        // ---- demirci ----

        private void DrawSmith()
        {
            GUILayout.Label("Demirci aletlerini geliştirir (akçe + odun). Daha hızlı kesim, ekim ve marangozluk.");
            foreach (var tool in _content.Tools.All)
            {
                int tier = _life.Tools.Tier(tool.Id);
                // Tırpanın taban kademesi yoktur: alınana kadar "yok" yazar (öteki aletlerin taban hâli hanede hep vardır).
                if (tier == 0 && tool.Id == Thatchwick.Core.Work.HarvestOrders.ScytheTool)
                    GUILayout.Label($"{tool.DisplayName}: {GameLocalization.T("ui.farm.tool.none")} ({tier}/{tool.Tiers.Count})");
                else
                GUILayout.Label($"{tool.DisplayName}: {_life.Tools.NameOf(tool.Id)} · hız x{_life.Tools.SpeedFor(tool.Id):0.0#} ({tier}/{tool.Tiers.Count})");
                var heir = _life.Legacy.ToolHeirloom(tool.Id);
                if (heir != null)
                    GUILayout.Label(GameLocalization.T("ui.legacy.heir_line", ("name", heir.DisplayName), ("pct", (Legacy.BonusOf(heir) * 100).ToString("0")), ("level", heir.Level)));
                else
                {
                    string hw = _life.Legacy.CanMakeToolHeirloom(tool.Id, _life.Tools.NameOf(tool.Id));
                    GUI.enabled = hw == null;
                    if (GUILayout.Button(new GUIContent($"Yadigâr yap ({Legacy.ToolHeirloomCost} mühür)", hw ?? GameLocalization.T("ui.legacy.heirloom_tip")), GUILayout.Width(170f)))
                    {
                        _life.Legacy.MakeToolHeirloom(tool.Id, _life.Tools.NameOf(tool.Id), _commander(), _life.Clock.TotalDays, out var hm);
                        _status = hm;
                    }
                    GUI.enabled = true;
                }
                var next = _life.Tools.Next(tool.Id);
                if (next == null) continue;
                string why = _life.Tools.CanUpgrade(tool.Id);
                GUILayout.Label($"   → {next.Name} (x{next.Speed:0.0#}): {next.Akce} akçe, {next.Firewood} odun, hesap Sv {next.Level}");
                GUI.enabled = why == null;
                if (GUILayout.Button("Geliştir", GUILayout.Width(90f)))
                {
                    _life.Tools.Upgrade(tool.Id, out var reason);
                    _status = reason ?? $"{next.Name} hazır.";
                }
                GUI.enabled = true;
                if (why != null) GUILayout.Label("   " + why);
            }
        }

        // ---- meslek dalları ----

        private void DrawBranches()
        {
            var c = _commander();
            if (c == null) { GUILayout.Label("Komut alan kişi yok."); return; }
            GUILayout.Label($"{c.Name} · beceri seviyesi 5 ve 10'da meslek dalı seçilir (kademe başına bir tane).");
            foreach (var id in c.BranchIds)
                if (_content.Branches.TryGet(id, out var b)) GUILayout.Label($"✓ {b.DisplayName}: {b.Description}");
            var open = LifeRules.AvailableBranches(_content, c);
            if (open.Count == 0) GUILayout.Label("Şu an seçilebilecek dal yok.");
            foreach (var b in open)
            {
                GUILayout.Label($"{b.DisplayName} ({b.Level}. seviye): {b.Description}");
                if (GUILayout.Button("Seç", GUILayout.Width(70f)))
                {
                    LifeRules.ChooseBranch(_content, c, b.Id, out var reason, _life.Village);
                    _status = reason ?? $"{c.Name}: {b.DisplayName} seçildi.";
                    _life.Goals.Update();
                    break;
                }
            }
        }

        // ---- hedefler ----

        private void DrawGoals()
        {
            foreach (var g in _content.Goals.All)
            {
                bool done = _life.Goals.IsDone(g.Id);
                int progress = _life.Goals.Progress(g);
                string reward = (g.RewardAkce > 0 ? $"{g.RewardAkce} akçe" : "") + (g.RewardAkce > 0 && g.RewardXp > 0 ? " + " : "") + (g.RewardXp > 0 ? GameLocalization.T("ui.goal.reward_xp", ("n", g.RewardXp)) : "");
                GUILayout.Label($"{(done ? "✓ " : "")}{g.DisplayName} — {progress}/{g.Target}{(reward.Length > 0 ? $" · ödül {reward}" : "")}");
                if (!done) GUILayout.Label($"   {g.Description}");
            }
        }

        // ---- soy ve mezarlar ----

        // ---- komşu hanedanlar ----

        private void DrawHouses()
        {
            var svc = _life.Houses;
            int? founder = _dynasty.History.Count > 0 ? _dynasty.History[0].CharacterId : (int?)null;
            GUILayout.Label("Köydeki aileler. Başı ölen hanenin yerine varisi geçer; dükkânı yetişkin varis sürdürür.");
            // İlan sıralaması (belge 46 §8.5): panodan alınan siparişlerin puanı; rakip haneler alınmayan siparişi üstlenir.
            var rank = _life.Requests.Ranking(_dynasty.Name);
            var top = new List<string>();
            for (int i = 0; i < rank.Count && i < 5; i++)
                top.Add(GameLocalization.T(rank[i].IsPlayer ? "ui.req.rank_short_you" : "ui.req.rank_short", ("i", i + 1), ("name", rank[i].Name), ("points", rank[i].Points)));
            GUILayout.Label(GameLocalization.T("ui.req.rank_inline", ("list", string.Join(" · ", top))));
            foreach (var h in svc.Houses)
            {
                if (h.Extinct && _openHouse != h.Id) continue;
                var head = _life.Population.Find(h.HeadId);
                int ours = 0;
                foreach (var m in svc.LivingMembers(h)) if (founder.HasValue && _life.Legacy.IsDescendantOf(m, founder.Value)) ours++;
                string headText = head != null && !head.IsDead ? $"{head.Name} ({AgeStages.Name(head.Stage)})" : "başsız";
                int memory = _life.Requests.Memory.Of(h.Id);
                string line = $"{h.Name} · baş {headText} · {svc.LivingCount(h)} kişi · {svc.Generations(h)} kuşak · {h.HeadCount}. baş"
                              + (memory != 0 ? $" · hafıza {(memory > 0 ? "+" : "")}{memory}" : "")
                              + (ours > 0 ? $" · senin soyundan {ours}" : "") + (h.Extinct ? " · soyu tükendi" : "");
                if (GUILayout.Button(line, GUI.skin.label)) _openHouse = _openHouse == h.Id ? -1 : h.Id;
                if (_openHouse != h.Id) continue;
                foreach (var n in svc.TreeOf(h))
                {
                    var p = n.Person;
                    string indent = new string(' ', (n.Depth - 1) * 3);
                    string state = p.IsDead ? "öldü" : AgeStages.Name(p.Stage);
                    string spouse = n.Spouse != null ? $" ∞ {n.Spouse.Name}" : "";
                    string mark = n.IsHead ? "★ " : "";
                    string blood = founder.HasValue && _life.Legacy.IsDescendantOf(p, founder.Value) ? " · senin soyun" : "";
                    GUILayout.Label($"  {indent}{mark}{p.Name} ({state}){spouse}{blood}");
                }
                if (h.Heads.Count > 1)
                {
                    var names = new List<string>();
                    foreach (var r in h.Heads) names.Add(r.Name);
                    GUILayout.Label("  Başlar: " + string.Join(" → ", names));
                }
            }
        }

        private void DrawLineage()
        {
            string trade = _life.FamilyTrade();
            GUILayout.Label($"{_dynasty.Name} · {_dynasty.CurrentGeneration}. nesil{(trade != null ? $" · {trade}" : "")}");
            GUILayout.Label("— Baş karakterler —");
            foreach (var h in _dynasty.History)
                GUILayout.Label($"{h.Generation}. nesil {h.Name}{(string.IsNullOrEmpty(h.Trade) ? "" : $" ({h.Trade})")} · gün {h.StartDay:0}{(h.EndDay >= 0 ? $"–{h.EndDay:0}" : "–")}");
            var lg = _life.Legacy;
            int? founder = _dynasty.History.Count > 0 ? _dynasty.History[0].CharacterId : (int?)null;
            GUILayout.Label($"— Soy Mührü: {lg.Seals} —");
            for (int i = Math.Max(0, lg.Ledger.Count - 6); i < lg.Ledger.Count; i++)
            {
                var e = lg.Ledger[i];
                GUILayout.Label($"  {(e.Amount > 0 ? "+" : "")}{e.Amount} · gün {e.Day:0} · {e.Note}");
            }
            GUILayout.Label("— Yadigârlar —");
            if (lg.Heirlooms.Count == 0) GUILayout.Label("Henüz yok. Yapı modunda bir eşyayı ya da demircide bir aleti yadigâr yap.");
            foreach (var h in lg.Heirlooms)
                GUILayout.Label($"  ✦ {h.DisplayName} · {h.Level}. basamak · +%{Legacy.BonusOf(h) * 100:0}{(h.Kind == HeirloomKind.Item ? $", konfor +{Legacy.ComfortOf(h)}" : "")}");
            GUILayout.Label($"— Köydeki soy: {(founder.HasValue ? lg.LivingDescendantsOutside(founder.Value) : 0)} kişi —");
            foreach (var m in lg.MarriedOut)
                GUILayout.Label($"  {m.Name} → {m.SpouseName} hanesi (gün {m.Day:0})");
            GUILayout.Label("— Mezarlık —");
            if (_life.Graves.Graves.Count == 0) GUILayout.Label("Henüz kimse yok.");
            foreach (var g in _life.Graves.Graves)
                GUILayout.Label($"{g.Name} · {g.Generation}. nesil · {AgeText(g.AgeDays)} · {CauseText(g.Cause)}");
        }

        private static string AgeText(double days) => $"{days:0} günlük";

        private static string CauseText(DeathCause c)
        {
            switch (c)
            {
                case DeathCause.Cold: return "soğuktan";
                case DeathCause.Starvation: return "açlıktan";
                case DeathCause.OldAge: return "yaşlılıktan";
                default: return "";
            }
        }
    }
}
