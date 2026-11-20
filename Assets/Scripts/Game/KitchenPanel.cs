using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Cooking;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using static Thatchwick.Game.GameLocalization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Mutfak paneli (Y, geçici IMGUI; Project belgeleri 58-62): yemek defteri ve tarif kartı, Mutfak görevinin öğün
    /// çizelgesi, kiler planı, kesim listesi, günlük rutin ve pazar yemek tezgâhı. Kurallar Core.Cooking'te; panel
    /// yalnızca gösterir ve emri iletir.
    /// </summary>
    public sealed class KitchenPanel
    {
        private readonly LifeSystems _life;
        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly GameClock _clock;
        private readonly Func<Character> _selected;
        private readonly Func<Character, JobRunner> _runnerOf;
        private readonly Func<IEnumerable<Character>> _household;
        private Vector2 _scroll;
        private int _tab;
        private string _pick;            // defterde seçili yemek
        private int _batch = 1;
        private readonly List<string> _spices = new List<string>();
        private string _message;
        private bool _confirmMarket;

        public bool Active { get; private set; }

        public KitchenPanel(LifeSystems life, WorldState world, Household house, ContentDatabase content, GameClock clock,
            Func<Character> selected, Func<Character, JobRunner> runnerOf, Func<IEnumerable<Character>> household)
        {
            _life = life; _world = world; _house = house; _content = content; _clock = clock;
            _selected = selected; _runnerOf = runnerOf; _household = household;
        }

        public void Toggle() { Active = !Active; _message = null; _confirmMarket = false; }
        public void Close() => Active = false;

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 428f, 8f, 420f, Screen.height / guiScale - 16f);

        private string Name(string itemId) => _content.Items.TryGet(itemId, out var d) ? d.DisplayName : _content.Groups.TryGet(itemId, out var g) ? g.DisplayName : itemId;
        private static string Stars(int q) => ItemQuality.Badge(q);

        private static readonly string[] Tabs = { "cook.ui.tab.book", "cook.ui.tab.plan", "cook.ui.tab.pantry", "cook.ui.tab.slaughter", "cook.ui.tab.routine", "cook.ui.tab.stall" };

        public void OnGUI(float guiScale)
        {
            if (!Active || _life == null) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("cook.ui.title"));
            if (GUILayout.Button(T("cook.ui.close"), GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (int k = 0; k < Tabs.Length; k++)
            {
                var old = GUI.color;
                if (k == _tab) GUI.color = new Color(1f, 0.9f, 0.6f);
                if (GUILayout.Button(T(Tabs[k]))) { _tab = k; _message = null; _confirmMarket = false; }
                GUI.color = old;
            }
            GUILayout.EndHorizontal();
            var who = _selected?.Invoke();
            if (who != null) GUILayout.Label(T("cook.ui.selected", ("name", who.Name)));
            if (!string.IsNullOrEmpty(_message))
            {
                var old = GUI.color; GUI.color = new Color(1f, 0.8f, 0.6f);
                GUILayout.Label(_message);
                GUI.color = old;
            }
            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case 0: Book(who); break;
                case 1: Plan(who); break;
                case 2: Pantry(); break;
                case 3: Slaughter(); break;
                case 4: Routine(who); break;
                default: Stall(who); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---- yemek defteri ve tarif kartı (58 §4, 60b §5.3) ----

        private void Book(Character who)
        {
            var pages = _life.Recipes.Pages(_household());
            int known = pages.Count(p => p.State == RecipeState.Known);
            GUILayout.Label(T("cook.ui.book.count", ("known", known), ("total", pages.Count(p => p.Dish.IsMeal))));
            foreach (var cls in pages.GroupBy(p => p.Dish.Class).OrderBy(g => (int)g.Key))
            {
                GUILayout.Label("— " + T(DishClasses.Key(cls.Key)) + " —");
                foreach (var p in cls.OrderBy(p => p.Dish.MinLevel))
                {
                    if (p.State == RecipeState.Unknown) { GUILayout.Label("• ???"); continue; }
                    string line = "• " + Name(p.Dish.Id);
                    if (p.State == RecipeState.Heard) line += "  (" + LearnHint(p.Dish) + ")";
                    else
                    {
                        if (p.TimesCooked > 0) line += "  " + T("cook.ui.book.cooked", ("n", p.TimesCooked)) + (p.BestQuality > 0 ? " " + Stars(p.BestQuality) : "");
                        if (p.OnlyKnower) line += "  " + T("cook.ui.book.only_knower", ("name", p.Knowers[0].Name));
                        if (p.Written) line += "  " + T("cook.ui.book.written");
                    }
                    GUILayout.BeginHorizontal();
                    var old = GUI.color;
                    if (p.State == RecipeState.Heard) GUI.color = new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(line);
                    GUI.color = old;
                    if (p.State == RecipeState.Known && GUILayout.Button(_pick == p.Dish.Id ? "▾" : "▸", GUILayout.Width(28f)))
                    { _pick = _pick == p.Dish.Id ? null : p.Dish.Id; _batch = 1; _spices.Clear(); _message = null; }
                    GUILayout.EndHorizontal();
                    if (_pick == p.Dish.Id && p.State == RecipeState.Known) Card(who, p);
                }
            }
        }

        private string LearnHint(DishDef dish)
        {
            switch (dish.Learn)
            {
                case LearnKind.Villager:
                    var v = _life.Villagers?.CharacterOf(dish.LearnFrom);
                    return T("cook.learn.villager", ("name", v != null ? v.Name : dish.LearnFrom), ("n", dish.LearnAt));
                case LearnKind.Treatise: return T("cook.learn.treatise");
                case LearnKind.Request: return T("cook.learn.request");
                case LearnKind.Family: return T("cook.learn.family");
                default: return T("cook.learn.level", ("skill", SkillNames.Name(dish.Skill)), ("n", dish.MinLevel));
            }
        }

        /// <summary>Tarif kartı: malzemeler, kimler biliyor ve seviyor, porsiyon katı, ek baharat, tahmini kalite dökümü, "Pişir".</summary>
        private void Card(Character who, RecipePage p)
        {
            var source = p.Dish.Sources[0];
            GUILayout.Label("   " + string.Join(", ", source.interaction.AllConsumed().Select(c => c.Count + " " + Name(c.ItemId) + " (" + _house.Count(c.ItemId) + ")")));
            if (p.Knowers.Count > 0) GUILayout.Label("   " + T("cook.ui.book.knowers", ("names", string.Join(", ", p.Knowers.Select(k => k.Name)))));
            if (p.Lovers.Count > 0) GUILayout.Label("   " + T("cook.ui.book.lovers", ("names", string.Join(", ", p.Lovers.Select(k => k.Name)))));
            if (who == null) return;
            if (!_life.Recipes.Knows(who, p.Dish)) { GUILayout.Label("   " + T("cook.block.unknown_recipe", ("name", who.Name))); return; }
            var (item, inter) = _life.Kitchen.Resolve(who, p.Dish.Id, out string why);
            GUILayout.BeginHorizontal();
            GUILayout.Label("   " + T("cook.ui.card.batch"));
            foreach (int b in new[] { 1, 2, 4 })
            {
                var old = GUI.color; if (_batch == b) GUI.color = new Color(1f, 0.9f, 0.6f);
                if (GUILayout.Button("×" + b, GUILayout.Width(36f))) _batch = b;
                GUI.color = old;
            }
            GUILayout.EndHorizontal();
            int slots = DishScore.SpiceSlots(who.Skills.GetLevel(SkillType.Cooking));
            if (slots > 0)
            {
                GUILayout.Label("   " + T("cook.ui.card.spices", ("n", slots)));
                foreach (var s in _content.Spices.All)
                {
                    if (_house.Count(s.Id) <= 0) continue;
                    bool on = _spices.Contains(s.Id);
                    bool fits = s.FitsClass(p.Dish.Class);
                    bool now = GUILayout.Toggle(on, "   " + Name(s.Id) + (fits ? "" : "  " + T("cook.ui.card.clash")));
                    if (now && !on && _spices.Count < slots) _spices.Add(s.Id);
                    else if (!now && on) _spices.Remove(s.Id);
                }
            }
            else GUILayout.Label("   " + T("cook.ui.card.no_slots"));
            if (inter != null)
            {
                var est = _life.Meals.Estimate(who, item, inter, _spices);
                if (est != null)
                    GUILayout.Label("   " + T("cook.ui.card.score", ("ing", est.Ingredients), ("skill", est.Skill), ("mastery", est.Mastery), ("equip", est.Equipment),
                        ("kitchen", est.Kitchen), ("cook", est.Cook), ("spice", est.Spice), ("total", est.Total)) + " " + Stars(est.Quality));
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(T("cook.ui.card.cook", ("name", who.Name))))
            {
                if (inter == null) _message = why != null && why.Length > 12 ? why : T("cook.ui.card.cannot");
                else
                {
                    _life.Meals.Order(who, item, inter.Id, _batch, _spices, serve: true);
                    string blocked = _life.Meals.Blocker(who, item, inter);
                    if (blocked != null) { _life.Meals.ClearOrder(who); _message = blocked; }
                    else { _runnerOf(who)?.Command(Job.UseItem(item.Id, inter.Id)); _message = null; }
                }
            }
            if (GUILayout.Button(T("cook.ui.card.plan_morning"))) _life.Kitchen.Add(MealSlot.Morning, p.Dish.Id, _batch, _spices);
            if (GUILayout.Button(T("cook.ui.card.plan_evening"))) _life.Kitchen.Add(MealSlot.Evening, p.Dish.Id, _batch, _spices);
            GUILayout.EndHorizontal();
        }

        // ---- Mutfak görevi: öğün çizelgesi (58a §4) ----

        private void Plan(Character who)
        {
            var cooks = _household().Where(m => !m.IsDead && m.Duty == DutyKind.Kitchen).ToList();
            GUILayout.Label(cooks.Count > 0 ? T("cook.ui.plan.cooks", ("names", string.Join(", ", cooks.Select(c => c.Name)))) : T("cook.ui.plan.no_cook"));
            if (who != null && Thatchwick.Core.Work.DutyPlanner.CanTakeDuty(who, DutyKind.Kitchen))
            {
                bool on = who.Duty == DutyKind.Kitchen;
                if (GUILayout.Button(on ? T("cook.ui.plan.unassign", ("name", who.Name)) : T("cook.ui.plan.assign", ("name", who.Name))))
                    who.Duty = on ? DutyKind.None : DutyKind.Kitchen;
            }
            GUILayout.Label(T("cook.ui.plan.rule"));
            foreach (MealSlot slot in new[] { MealSlot.Morning, MealSlot.Evening })
            {
                GUILayout.Label("— " + T(slot == MealSlot.Morning ? "cook.slot.morning" : "cook.slot.evening") + " —");
                var plan = _life.Kitchen.Plan(slot);
                if (plan.Count == 0) GUILayout.Label(T("cook.ui.plan.empty"));
                var next = _life.Kitchen.NextEntry(slot);
                for (int k = 0; k < plan.Count; k++)
                {
                    var e = plan[k];
                    GUILayout.BeginHorizontal();
                    string line = (ReferenceEquals(e, next) ? "▸ " : "• ") + Name(e.DishId) + (e.Batch > 1 ? " ×" + e.Batch : "")
                        + (e.Spices.Count > 0 ? " + " + string.Join(", ", e.Spices.Select(Name)) : "");
                    var lovers = _household().Where(m => !m.IsDead && _life.Recipes.TastesOf(m)?.Loves(e.DishId) == true).Select(m => m.Name).ToList();
                    if (lovers.Count > 0) line += "  ♥ " + string.Join(", ", lovers);
                    GUILayout.Label(line);
                    if (GUILayout.Button("✕", GUILayout.Width(28f))) { _life.Kitchen.RemoveAt(slot, k); GUILayout.EndHorizontal(); break; }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Label(T("cook.ui.plan.hint"));
        }

        // ---- kiler planı (60b §5.4) ----

        private void Pantry()
        {
            int eaters = _household().Count(m => !m.IsDead);
            var lines = _life.Kitchen.Pantry(eaters, out float days);
            GUILayout.Label(T("cook.ui.pantry.days", ("days", Mathf.FloorToInt(days)), ("n", eaters)));
            foreach (var l in lines) GUILayout.Label("• " + T(l.ClassKey) + ": " + l.Count + "  (" + T("cook.ui.pantry.meals", ("n", Mathf.RoundToInt(l.Meals))) + ")");
            GUILayout.Label("— " + T("cook.ui.pantry.targets") + " —");
            GUILayout.Label(T("cook.ui.pantry.targets_hint"));
            foreach (var d in _content.Items.All)
            {
                if (!d.HasTag("preserved") || !d.HasTag("food")) continue;
                _life.Kitchen.Targets.TryGetValue(d.Id, out int target);
                int have = _house.Count(d.Id);
                if (target == 0 && have == 0) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(d.DisplayName + ": " + have + (target > 0 ? " / " + target : ""));
                if (GUILayout.Button("−", GUILayout.Width(28f))) _life.Kitchen.SetTarget(d.Id, Mathf.Max(0, target - 5));
                if (GUILayout.Button("+", GUILayout.Width(28f))) _life.Kitchen.SetTarget(d.Id, target + 5);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label(T("cook.ui.pantry.dishes", ("n", _life.Meals.Sink)));
        }

        // ---- kesim listesi (60b §2) ----

        private void Slaughter()
        {
            var sh = _life.Butchery;
            bool own = sh.HasSlaughterhouse != null && sh.HasSlaughterhouse();
            GUILayout.Label(own ? T("cook.ui.sl.own") : T("cook.ui.sl.no_own"));
            GUILayout.Label(T("cook.ui.sl.rule"));
            var rows = sh.Rows();
            if (rows.Count == 0) GUILayout.Label(T("cook.ui.sl.none"));
            foreach (var r in rows)
            {
                var a = r.Animal;
                string stage = T("cook.ui.sl.stage." + r.AgeStage);
                string line = (a.Name ?? a.Species.DisplayName) + " · " + a.Species.DisplayName + " · " + a.SexText + " · " + stage;
                if (a.IsSick) line += " · " + T("cook.ui.sl.sick");
                GUILayout.BeginHorizontal();
                var old = GUI.color;
                if (r.Blocked != null) GUI.color = new Color(0.65f, 0.65f, 0.65f);
                bool now = GUILayout.Toggle(r.Marked, line);
                GUI.color = old;
                if (now != r.Marked) sh.Mark(a, now);
                GUILayout.EndHorizontal();
                if (r.Blocked != null) { GUILayout.Label("   " + r.Blocked); continue; }
                string yield = "   " + r.MeatCount + " " + Name(r.MeatId) + " " + Stars(r.ExpectedQuality);
                if (r.HideId != null) yield += ", " + r.HideCount + " " + Name(r.HideId);
                if (r.Byproducts.Count > 0) yield += ", " + string.Join(", ", r.Byproducts.Select(b => b.Count + " " + Name(b.ItemId)));
                GUILayout.Label(yield);
                var warn = new List<string>();
                if (r.Warnings.HasFlag(SlaughterWarning.Pregnant)) warn.Add(T("cook.ui.sl.warn.pregnant"));
                if (r.Warnings.HasFlag(SlaughterWarning.Pedigree)) warn.Add(T("cook.ui.sl.warn.pedigree"));
                if (r.Warnings.HasFlag(SlaughterWarning.LastMale)) warn.Add(T("cook.ui.sl.warn.last_male"));
                if (r.Warnings.HasFlag(SlaughterWarning.Loved)) warn.Add(T("cook.ui.sl.warn.loved"));
                if (r.Warnings.HasFlag(SlaughterWarning.Old)) warn.Add(T("cook.ui.sl.warn.old"));
                if (warn.Count > 0)
                {
                    old = GUI.color; GUI.color = new Color(1f, 0.75f, 0.6f);
                    GUILayout.Label("   " + string.Join(" · ", warn));
                    GUI.color = old;
                }
            }
            GUILayout.Label(T("cook.ui.sl.listed", ("n", sh.List.Count), ("depot", sh.DepotCount), ("market", sh.PendingAtMarket)));
            if (sh.List.Count == 0) return;
            // Pazarda kestir: yalnızca et gelir; bu açıkça yazılır ve onay istenir (60b-M1).
            var warnColor = GUI.color; GUI.color = new Color(1f, 0.75f, 0.6f);
            GUILayout.Label(T("cook.ui.sl.market_warning"));
            GUI.color = warnColor;
            if (!_confirmMarket) { if (GUILayout.Button(T("cook.ui.sl.market"))) _confirmMarket = true; }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(T("cook.ui.sl.market_confirm")))
                {
                    int n = sh.SendListToMarket(_clock.DayIndex);
                    _message = T("cook.ui.sl.market_sent", ("n", n));
                    _confirmMarket = false;
                }
                if (GUILayout.Button(T("cook.ui.cancel"))) _confirmMarket = false;
                GUILayout.EndHorizontal();
            }
        }

        // ---- günlük rutin (60b §5.1) ----

        private void Routine(Character who)
        {
            if (who == null) { GUILayout.Label(T("cook.ui.routine.pick")); return; }
            GUILayout.Label(T("cook.ui.routine.rule"));
            for (int part = 0; part < 2; part++)
            {
                GUILayout.Label("— " + T(part == 0 ? "cook.slot.morning" : "cook.slot.evening") + " —");
                var list = _life.Routines.Of(who, part).ToList();
                GUILayout.Label(list.Count == 0 ? T("cook.ui.routine.empty") : string.Join(" › ", list.Select(b => T("cook.routine." + b))));
                GUILayout.BeginHorizontal();
                foreach (var b in Routines.Blocks)
                    if (GUILayout.Button("+ " + T("cook.routine." + b))) { list.Add(b); _life.Routines.Set(who, part, list); }
                GUILayout.EndHorizontal();
                if (list.Count > 0 && GUILayout.Button(T("cook.ui.routine.clear"))) _life.Routines.Set(who, part, null);
            }
            if (GUILayout.Button(T("cook.ui.routine.defaults"))) _life.Routines.UseDefaults(who);
        }

        // ---- pazar yemek tezgâhı (60b §5.7) ----

        private void Stall(Character who)
        {
            var o = _life.Outings;
            GUILayout.Label(T("cook.ui.stall.rule"));
            if (o.VillageFavorite != null) GUILayout.Label(T("cook.ui.stall.favorite", ("item", Name(o.VillageFavorite))));
            if (o.StallOpen)
            {
                GUILayout.Label(T("cook.ui.stall.open"));
                if (GUILayout.Button(T("cook.ui.stall.close")))
                {
                    var seller = _household().FirstOrDefault(m => m.Id == o.StallSeller) ?? who;
                    var r = o.CloseStall(seller);
                    _message = T("cook.n.stall_closed", ("sold", r.Sold), ("akce", r.Revenue), ("left", r.Unsold));
                }
                return;
            }
            var goods = o.Sellable();
            if (goods.Count == 0) { GUILayout.Label(T("cook.ui.stall.nothing")); return; }
            foreach (var (dish, count) in goods)
                GUILayout.Label("• " + Name(dish) + " × " + count + "  (" + Outings.PortionPrice(_content.Items.Get(dish), _house.BestQuality(dish), goods.Count, o.VillageFavorite == dish) + ")");
            string blocked = o.StallBlocker(who);
            if (blocked != null) { GUILayout.Label(blocked); return; }
            if (GUILayout.Button(T("cook.ui.stall.open_all", ("name", who.Name))) && !o.OpenStall(who, goods)) _message = T("cook.ui.card.cannot");
        }
    }
}
