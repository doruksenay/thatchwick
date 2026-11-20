using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Animals.Genetics;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;
using UnityEngine;

namespace Thatchwick.Game
{
    // Hayvan genetiği, soy defteri, köy merası ve yayla, böcek ve balık defteri, gölet temizliği (Project belgesi 48).
    // Basit IMGUI; asıl ekranlar PixelLab arayüz işleriyle gelecek. Metinler ui.*.json anahtarlarıyla.
    public sealed partial class GameRoot
    {
        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string name, object value)[] args) => GameLocalization.T(key, args);

        // Görünüş her karede yeniden hesaplanmasın: hayvan başına, yaş gününe göre önbellek (kır at yaşla ağarır).
        private readonly Dictionary<int, (int day, Phenotype look)> _lookCache = new Dictionary<int, (int, Phenotype)>();

        private Phenotype LookOf(Animal a)
        {
            if (a?.Genome == null) return null;
            int day = (int)a.AgeDays;
            if (_lookCache.TryGetValue(a.Id, out var c) && c.day == day && c.look != null) return c.look;
            var look = a.Look;
            _lookCache[a.Id] = (day, look);
            return look;
        }

        private int _namingAnimalId;
        private string _animalNameDraft;
        private bool _herdOpen, _bookOpen;

        /// <summary>Hayvan satırına eklenen genetik özet: tip, görünüş, hünerler, kısırlık.</summary>
        private string GeneticsSummary(Animal a)
        {
            if (a?.Genome == null) return "";
            var parts = new List<string>();
            if (a.TypeId != null) parts.Add(T("breed." + a.TypeId));
            var look = a.Look;
            if (look.ColorKey != null) parts.Add(T(look.ColorKey) + (look.PatternKey != null ? " " + T(look.PatternKey) : ""));
            foreach (var g in a.Genome.Gifts)
            {
                var def = GeneticsCatalog.GiftById(g);
                parts.Add((def != null && def.Rarity == GiftRarity.Legendary ? "★ " : "") + T(g + ".name"));
            }
            foreach (var d in a.Genome.Defects) parts.Add(T(GeneDefect.NameKey(d)));
            if (a.IsSterile) parts.Add(T("ui.animal.sterile"));
            return parts.Count > 0 ? " · " + string.Join(", ", parts) : "";
        }

        /// <summary>Soy defteri metni: ebeveynler, akrabalık, istatistikler, bilinen aleller, efsane yolu.</summary>
        private string PedigreeText(Animal a)
        {
            var g = a.Genome;
            if (g == null) return T("ui.animal.no_genome");
            var lines = new List<string>();
            var (m, f) = _animals.ParentsOf(a.Id);
            string Name(int id) => id == 0 ? "?" : (_animals.Get(id)?.Name ?? T("ui.animal.departed"));
            lines.Add(T("ui.animal.parents", ("mother", Name(m)), ("father", Name(f))));
            if (g.Inbreeding > 0) lines.Add(T("ui.animal.inbreeding", ("value", (g.Inbreeding * 100).ToString("0"))));
            var stats = new List<string>();
            foreach (var s in GeneticsCatalog.Stats(g.Species)) stats.Add($"{T(GeneStat.NameKey(s))} {g.Stat(s)}");
            lines.Add(string.Join(" · ", stats));
            int husbandry = _animals.HusbandryLevel?.Invoke() ?? 1;
            var genes = new List<string>();
            foreach (var l in GeneticsCatalog.Loci(g.Species))
            {
                var (x, y) = GeneticsService.KnownPair(g, l.Id, husbandry);
                genes.Add($"{l.Id}: {x}/{(y == Genome.YAllele ? "-" : y)}");
            }
            lines.Add(T("ui.animal.genes") + " " + string.Join("  ", genes));
            var (have, need) = PetLegends.Progress(a);
            if (need > 0 && a.LegendaryTitle == null) lines.Add(T("ui.animal.legend_path", ("have", have), ("need", need)));
            return string.Join("\n", lines);
        }

        // Hayvan menüsüne: ad ver, soy defteri, köy merası ve yayla.
        private void AddGeneticsOptions(Animal a)
        {
            if (a.Genome != null)
            {
                _menuOptions.Add(new MenuOption { Label = T("ui.animal.rename", ("name", a.Name)), Run = () => { _namingAnimalId = a.Id; _animalNameDraft = a.Name; } });
                _menuOptions.Add(new MenuOption { Label = T("ui.animal.pedigree", ("name", a.Name)), Run = () => ShowMessage(PedigreeText(a), 8f) });
            }
            if (a.Species.Herd || a.Species.Id == GeneticsCatalog.Dog)
            {
                _menuOptions.Add(new MenuOption
                {
                    Label = T("ui.herd.send_common", ("name", a.Name), ("used", _animals.AwayCount(AwayPlace.Common)), ("quota", _animals.StintQuota)),
                    Blocked = _animals.SendAwayBlocker(a, AwayPlace.Common, _clock.Season),
                    Run = () => { if (_animals.SendAway(a, AwayPlace.Common, _clock.Season, _clock.TotalDays)) ShowMessage(T("ui.herd.sent_common", ("name", a.Name))); }
                });
                _menuOptions.Add(new MenuOption
                {
                    Label = T("ui.herd.send_yayla", ("name", a.Name)),
                    Blocked = _animals.SendAwayBlocker(a, AwayPlace.Yayla, _clock.Season),
                    Run = () => { if (_animals.SendAway(a, AwayPlace.Yayla, _clock.Season, _clock.TotalDays)) ShowMessage(T("ui.herd.sent_yayla", ("name", a.Name))); }
                });
            }
        }

        // Gölet: boşalt ve temizle (çamur gübre olur).
        private void AddPondCleanOption(GridPos tile, Pond pond)
        {
            if (pond == null || !pond.IsFilled) return;
            _menuOptions.Add(new MenuOption
            {
                Label = T("ui.pond.clean", ("silt", pond.Silt.ToString("0"))),
                Blocked = pond.Silt < Pond.SiltPerManure ? T("ui.pond.clean_not_needed") : _world.Map.WaterFrozen ? T("ui.pond.frozen") : null,
                Run = () =>
                {
                    int manure = _world.Map.CleanPond(tile);
                    if (manure < 0) return;
                    if (manure > 0) _household.AddItem("item.manure", manure);
                    ShowMessage(T("ui.pond.cleaned", ("manure", manure)));
                }
            });
        }

        // Hayvanlar panelinde: köy merası ve yayla, böcek ve balık defteri, perhiz.
        private void DrawHerdAndBook()
        {
            if (_life == null) return;
            if (_life.IsFasting) GUILayout.Label("   " + T("ui.fasting.today"));
            int common = _animals.AwayCount(AwayPlace.Common), yayla = _animals.AwayCount(AwayPlace.Yayla);
            if (GUILayout.Button("   " + T("ui.herd.header", ("common", common), ("quota", _animals.StintQuota), ("yayla", yayla)) + (_herdOpen ? " ▲" : " ▼"))) _herdOpen = !_herdOpen;
            if (_herdOpen)
                foreach (var w in new List<AwayAnimal>(_animals.Away))
                {
                    GUILayout.BeginHorizontal();
                    int stock = 0; foreach (var kv in w.Stock) stock += kv.Value;
                    GUILayout.Label($"      {w.Animal.Name} ({w.Animal.Species.DisplayName}) · {T(w.Place == AwayPlace.Common ? "ui.herd.common" : "ui.herd.yayla")}{(stock > 0 ? " · " + T("ui.herd.stock", ("count", stock)) : "")}");
                    string why = _animals.RecallBlocker(w);
                    GUI.enabled = why == null;
                    if (GUILayout.Button(T("ui.herd.recall"))) { if (_animals.Recall(w, _household)) ShowMessage(T("ui.herd.recalled", ("name", w.Animal.Name))); }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            var book = _life.Book;
            if (GUILayout.Button("   " + T("ui.book.header", ("have", book.SeenCount), ("total", CreatureBook.TotalCount)) + (_bookOpen ? " ▲" : " ▼"))) _bookOpen = !_bookOpen;
            if (_bookOpen)
                foreach (var set in CreatureBook.Sets)
                {
                    var (have, total) = book.Progress(set);
                    var names = new List<string>();
                    foreach (var m in set.Members) names.Add(book.Has(m) ? ItemName(m) : "?");
                    names.Add(book.Has(set.Legendary) ? "★ " + ItemName(set.Legendary) : "★ ?");
                    GUILayout.Label($"      {T(set.NameKey)} {have}/{total}{(book.IsDone(set) ? " ✓" : "")}: {string.Join(", ", names)}");
                }
        }

        private string ItemName(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        /// <summary>Köpek ve kedinin yarışması (tazı koşusu, kedi güzellik yarışması); yoksa null.</summary>
        private static ContestKind? PetContest(Animal a)
        {
            if (a.TypeId == "dog.sighthound" || a.TypeId == "dog.lurcher") return ContestKind.HoundRace;
            if (a.Species.Id == GeneticsCatalog.Cat) return ContestKind.CatShow;
            return null;
        }

        /// <summary>Yeni doğan köpek ve kedi: görünüş genden, oyuncu yalnız adı seçer (karar 20.11.2026).</summary>
        private void OnPetBorn(Animal baby)
        {
            if (baby?.Genome == null || !baby.Species.IsPet) return;
            _namingAnimalId = baby.Id;
            _animalNameDraft = baby.Name;
        }

        private void DrawAnimalNaming()
        {
            if (_namingAnimalId == 0) return;
            var a = _animals?.Get(_namingAnimalId);
            if (a == null) { _namingAnimalId = 0; return; }
            float s = HudScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            var r = new Rect(Screen.width / s * 0.5f - 200f, Screen.height / s * 0.5f - 90f, 400f, 180f);
            GUILayout.BeginArea(r, GUI.skin.box);
            GUILayout.Label(T("ui.animal.naming_title", ("species", a.Species.DisplayName)));
            GUILayout.Label(GeneticsSummary(a).TrimStart(' ', '·'));
            _animalNameDraft = GUILayout.TextField(_animalNameDraft ?? "", 20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(T("ui.animal.naming_ok")))
            {
                string name = (_animalNameDraft ?? "").Trim();
                if (name.Length > 0) a.Name = name;
                _namingAnimalId = 0;
            }
            if (GUILayout.Button(T("ui.animal.naming_keep"))) _namingAnimalId = 0;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.matrix = Matrix4x4.identity;
        }

        // ---- at yarışı bahsi (+18 paketi; karar 20.11.2026): yalnızca metin, at seçilir. UI Toolkit penceresi. ----

        private Ui.ListWindow _betWindow;
        private int _betPick = -1, _betStake = 10;
        private string _betStatus = "";

        private bool BetVisible => _betWindow != null && _betWindow.Visible;

        /// <summary>Ayrıntı panelindeki giriş: bahse açık yarış varsa düğme, bahis yatırıldıysa durumu.</summary>
        private void DrawBetEntry()
        {
            var bet = _life.Bets.Current;
            if (bet != null)
            {
                GUILayout.Label("   " + GameLocalization.T("ui.bet.current", ("horse", bet.RunnerName), ("stake", bet.Stake),
                    ("payout", AnimalContests.BetPayout(bet.Stake, bet.Chance))));
                return;
            }
            if (_life.OpenRaceCard() == null) return;
            if (GUILayout.Button("   " + T("ui.bet.open"))) OpenBet();
        }

        private void OpenBet()
        {
            var host = EnsureUi();
            if (host == null) { ShowMessage(T("ui.ach.unavailable")); return; }
            if (_betWindow == null)
            {
                _betWindow = new Ui.ListWindow(host, BetModel, () => T("ui.bet.close"), 520f, 8);
                _life.Bets.Settled += _ => _betWindow.Refresh();   // yarış koştu: pencere açıksa güncellenir ya da kapanır
            }
            _betPick = -1; _betStatus = "";
            _betWindow.Visible = true;
        }

        /// <summary>Bahis penceresinin içeriği; bahse açık yarış da bekleyen bahis de yoksa null (pencere kapanır).</summary>
        private Ui.ListWindow.Model BetModel()
        {
            var card = _life.OpenRaceCard();
            var bet = _life.Bets.Current;
            if (card == null && bet == null) return null;

            var model = new Ui.ListWindow.Model { Status = _betStatus };
            string festival = card != null ? _life.Festivals.FestivalOn(card.Day)?.DisplayName ?? "" : "";
            model.Title = card == null ? T("ui.bet.title")
                : T(card.Day == _clock.DayIndex ? "ui.bet.title_today" : "ui.bet.title_tomorrow", ("festival", festival));
            model.Info = T("ui.bet.rules");
            if (bet != null)
                model.Info += "\n" + T("ui.bet.current", ("horse", bet.RunnerName), ("stake", bet.Stake), ("payout", AnimalContests.BetPayout(bet.Stake, bet.Chance)));
            if (card == null) return model;

            for (int i = 0; i < card.Runners.Count; i++)
            {
                var r = card.Runners[i];
                int index = i;
                var row = new Ui.ListWindow.Row(T(r.Mine ? "ui.bet.runner_mine" : string.IsNullOrEmpty(r.Owner) ? "ui.bet.runner_plain" : "ui.bet.runner",
                    ("n", i + 1), ("horse", r.Name), ("owner", r.Owner), ("chance", Mathf.RoundToInt((float)(r.Chance * 100))),
                    ("stake", _betStake), ("payout", RaceBets.PayoutFor(r, _betStake))));
                if (bet == null) { row.Marked = _betPick == i; row.Click = () => { _betPick = index; _betStatus = ""; }; }
                model.Rows.Add(row);
            }
            if (bet != null) return model;

            model.FooterLabel = T("ui.bet.stake", ("akce", _household.Akce));
            foreach (int stake in RaceBets.Stakes)
            {
                int amount = stake;
                model.Footer.Add(new Ui.ListWindow.Act(stake.ToString(), () => _betStake = amount, on: _betStake == stake));
            }
            bool picked = _betPick >= 0 && _betPick < card.Runners.Count;
            model.Footer.Add(new Ui.ListWindow.Act(T("ui.bet.place"), () =>
            {
                var err = _life.Bets.Place(card, _betPick, _betStake, _household, Commander?.Character?.Id ?? 0);
                _betStatus = err.IsEmpty ? "" : GameLocalization.R(err);
            }, disabled: !picked));
            return model;
        }
    }
}
