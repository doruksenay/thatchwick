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
        private static readonly string[] Tabs = { "Köylüler", "Proje", "Demirci", "Meslek", "Hedef", "Soy" };

        private readonly LifeSystems _life;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly FamilyService _family;
        private readonly Dynasty _dynasty;
        private readonly Func<Character> _commander;
        private int _tab;
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
            int tab = GUILayout.Toolbar(_tab, Tabs);
            if (tab != _tab) { _tab = tab; _status = ""; _giftTo = null; }
            if (_status.Length > 0) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0: DrawVillagers(); break;
                case 1: DrawProjects(); break;
                case 2: DrawSmith(); break;
                case 3: DrawBranches(); break;
                case 4: DrawGoals(); break;
                default: DrawLineage(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string ItemName(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

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
                        if (GUILayout.Button($"   {item.DisplayName} ({kv.Value})", GUILayout.Width(220f)))
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
                var cost = new System.Text.StringBuilder($"   {p.Akce} akçe");
                foreach (var c in p.Costs) cost.Append($" · {ItemName(c.ItemId)} {_house.Count(c.ItemId)}/{c.Count}");
                GUILayout.Label(cost.ToString());
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
                GUILayout.Label($"{tool.DisplayName}: {_life.Tools.NameOf(tool.Id)} · hız x{_life.Tools.SpeedFor(tool.Id):0.0#} ({tier}/{tool.Tiers.Count})");
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
                string reward = (g.RewardAkce > 0 ? $"{g.RewardAkce} akçe" : "") + (g.RewardAkce > 0 && g.RewardXp > 0 ? " + " : "") + (g.RewardXp > 0 ? $"{g.RewardXp} XP" : "");
                GUILayout.Label($"{(done ? "✓ " : "")}{g.DisplayName} — {progress}/{g.Target}{(reward.Length > 0 ? $" · ödül {reward}" : "")}");
                if (!done) GUILayout.Label($"   {g.Description}");
            }
        }

        // ---- soy ve mezarlar ----

        private void DrawLineage()
        {
            string trade = _life.FamilyTrade();
            GUILayout.Label($"{_dynasty.Name} · {_dynasty.CurrentGeneration}. nesil{(trade != null ? $" · {trade}" : "")}");
            GUILayout.Label("— Baş karakterler —");
            foreach (var h in _dynasty.History)
                GUILayout.Label($"{h.Generation}. nesil {h.Name}{(string.IsNullOrEmpty(h.Trade) ? "" : $" ({h.Trade})")} · gün {h.StartDay:0}{(h.EndDay >= 0 ? $"–{h.EndDay:0}" : "–")}");
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
