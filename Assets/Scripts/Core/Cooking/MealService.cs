using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Cooking
{
    /// <summary>Oyuncunun (ya da Mutfak çizelgesinin) verdiği pişirme emri: porsiyon katı, ek baharat, sofraya servis.</summary>
    public sealed class CookOrder
    {
        public int StationId;
        public string InteractionId;
        /// <summary>Porsiyon katı: 1, 2 ya da 4 (malzeme ×1 / ×2 / ×4).</summary>
        public int Batch = 1;
        public List<string> Spices = new List<string>();
        public bool Serve = true;
    }

    /// <summary>Sofradaki servis kabı (belge 58 §3.2): masanın üstünde durur, porsiyon porsiyon azalır.</summary>
    public sealed class Serving
    {
        public int TableId;
        public string DishId;
        public int Quality;
        public int Portions, Total;
        public double CookedAt;
        public int CookId;
        public List<string> Spices = new List<string>();
        public bool Burnt, Stale;
        /// <summary>Bu sofradan yiyenler (kişi, an): birlikte yeme için.</summary>
        public List<(int id, double at)> Eaters = new List<(int, double)>();
        /// <summary>Doluluk karesi: 4 dolu, 3 dörtte üç, 2 yarım, 1 çeyrek, 0 boş.</summary>
        public int FillStage => Portions <= 0 || Total <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(4.0 * Portions / Total));
    }

    /// <summary>
    /// Pişirme emri, sofra ve servis (belge 58 §2-3, 60c; kararlar Y1, Y3, 62-S3, S6):
    /// yemek servis kabıyla masaya gelir, herkes kendi tabağına alır, ana kaptaki yemek azalır; artan bayatlar ve bozulur;
    /// kirli tabaklar toplanır ve yıkanır. Pişen yemeğin kalitesi <see cref="DishScore"/> ile hesaplanır.
    /// </summary>
    public sealed class MealService
    {
        public const string TableTag = "diningtable";
        public const string EatServing = "eatServing", ClearTable = "clearTable", WashDishes = "washDishes", FeedScrap = "feedScrap";
        public const string EatInteractionId = "eat_serving";
        /// <summary>Açıkta kalan yemek bu kadar günde bayatlar, şu kadar günde bozulur.</summary>
        public const double StaleAfter = 0.35, SpoilAfter = 1.0;
        /// <summary>Aynı sofrada bu süre içinde yiyenler birlikte yemiş sayılır.</summary>
        public const double TogetherWindow = 0.1;
        public const int SinkLimit = 8;
        public const float CallBelowHunger = 75f;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly RecipeBook _book;
        private readonly Random _rng;
        private readonly Dictionary<int, CookOrder> _orders = new Dictionary<int, CookOrder>();
        private readonly Dictionary<int, Serving> _servings = new Dictionary<int, Serving>();
        private readonly Dictionary<int, int> _dirty = new Dictionary<int, int>();
        private (int cookId, DishScoreBreakdown score, List<SpiceDef> spices)? _last;

        public MealService(WorldState world, Household house, ContentDatabase content, RecipeBook book, int seed = 58)
        {
            _world = world; _house = house; _content = content; _book = book; _rng = new Random(seed);
        }

        public Func<double> Now { get; set; } = () => 0;
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";
        public Func<int, Character> Find { get; set; }
        /// <summary>Mutfak odasının puanı (0-10): düzen, ışık, baharat rafı, tezgâh.</summary>
        public Func<ItemInstance, int> KitchenPoints { get; set; }
        /// <summary>Aşçının aile tarifi var mı (beceriye göre).</summary>
        public Func<Character, SkillType, bool> HasFamilyRecipe { get; set; }
        public Func<IEnumerable<Character>> Members { get; set; }

        /// <summary>Emirsiz pişen yemek de (masa varsa) sofraya gitsin mi. Oyunda açık; testlerde ve köylüde kapalı.</summary>
        public bool AutoServe { get; set; }
        /// <summary>Yıkanmayı bekleyen kirli tabak.</summary>
        public int Sink { get; private set; }
        public IReadOnlyDictionary<int, Serving> Servings => _servings;
        public int DirtyAt(int tableId) => _dirty.TryGetValue(tableId, out int n) ? n : 0;
        public Serving ServingAt(int tableId) => _servings.TryGetValue(tableId, out var s) ? s : null;
        public DishScoreBreakdown LastScore => _last?.score;

        public event Action<Character, ItemInstance, DishDef, DishScoreBreakdown, IReadOnlyList<SpiceDef>> Cooked;
        /// <summary>Yemek sofraya kondu: hane çağrılır.</summary>
        public event Action<Serving, ItemInstance, Character> Served;
        public event Action<Character, Serving, DishDef> Ate;
        public event Action<Serving, ItemInstance> Spoiled;
        public event Action<LocText, Character> Notice;

        // ---- emir ----

        public void Order(Character c, ItemInstance station, string interactionId, int batch = 1, IEnumerable<string> spices = null, bool serve = true)
        {
            if (c == null || station == null) return;
            var o = new CookOrder { StationId = station.Id, InteractionId = interactionId, Batch = batch >= 4 ? 4 : batch >= 2 ? 2 : 1, Serve = serve };
            if (spices != null) foreach (var s in spices) if (s != null && _content.Spices.Contains(s) && !o.Spices.Contains(s)) o.Spices.Add(s);
            _orders[c.Id] = o;
        }

        public CookOrder OrderFor(Character c, ItemInstance station, InteractionDef i) =>
            c != null && station != null && i != null && _orders.TryGetValue(c.Id, out var o) && o.StationId == station.Id && o.InteractionId == i.Id ? o : null;

        public void ClearOrder(Character c) { if (c != null) _orders.Remove(c.Id); }

        private List<SpiceDef> SpicesOf(CookOrder o)
        {
            var list = new List<SpiceDef>();
            if (o != null) foreach (var id in o.Spices) if (_content.Spices.TryGet(id, out var s)) list.Add(s);
            return list;
        }

        /// <summary>Tarif kartı için tahmini puan (rastlantısız; yanık şansı ayrı gösterilir).</summary>
        public DishScoreBreakdown Estimate(Character c, ItemInstance station, InteractionDef i, IEnumerable<string> spices = null)
        {
            if (c == null || i?.Dish == null) return null;
            var taken = new List<ItemKey>();
            foreach (var con in i.AllConsumed())
            {
                // Tahmin: harcama politikası en düşük kaliteyi alır.
                string id = con.ItemId;
                if (IngredientGroupDef.IsGroup(id) && _content.Groups.TryGet(id, out var g))
                    id = g.Members.Select(m => m.ItemId).FirstOrDefault(m => _house.Count(m) > 0) ?? g.Members[0].ItemId;
                int q = 0;
                var counts = _house.QualityCounts(id);
                for (int k = 0; counts != null && k < counts.Length; k++) if (counts[k] > 0) { q = k; break; }
                for (int k = 0; k < con.Count; k++) taken.Add(new ItemKey(id, q));
            }
            var sp = new List<SpiceDef>();
            if (spices != null) foreach (var s in spices) if (_content.Spices.TryGet(s, out var d)) sp.Add(d);
            return Score(c, station, i, taken, sp);
        }

        private DishScoreBreakdown Score(Character c, ItemInstance station, InteractionDef i, List<ItemKey> taken, List<SpiceDef> spices)
        {
            var dish = i.Dish;
            int level = c.Skills.GetLevel(i.Skill ?? dish.Skill);
            int trait = (c.HasTrait("trait.handy") ? 1 : 0) + Math.Min(2, c.Modifiers.QualityLevels);
            return new DishScoreBreakdown
            {
                Ingredients = DishScore.IngredientPoints(taken, _content),
                Skill = DishScore.SkillPoints(level, i.MinSkillLevel),
                Mastery = DishScore.MasteryPoints(_book.TimesCooked(c, dish.Id), HasFamilyRecipe != null && HasFamilyRecipe(c, i.Skill ?? dish.Skill), trait),
                Equipment = DishScore.EquipmentPoints(station),
                Kitchen = Math.Max(0, Math.Min(DishScore.MaxKitchen, (KitchenPoints?.Invoke(station) ?? 4) - (Sink > SinkLimit ? 2 : 0))),
                Cook = DishScore.CookPoints(c),
                Spice = DishScore.SpicePoints(spices, dish.Class)
            };
        }

        /// <summary>InteractionEffects.DishQuality: pişen yemeğin kalitesi. Köylüde null (53 kuralı).</summary>
        public int? QualityFor(Character c, ItemInstance station, InteractionDef i, List<ItemKey> taken)
        {
            _last = null;
            if (c == null || c.IsVillager || i?.Dish == null) return null;
            var order = OrderFor(c, station, i);
            var spices = SpicesOf(order).Where(s => _house.Count(s.Id) > 0).ToList();
            var score = Score(c, station, i, taken, spices);
            int level = c.Skills.GetLevel(i.Skill ?? i.Dish.Skill);
            score.Burnt = _rng.NextDouble() < DishScore.BurnChance(level, i.MinSkillLevel);
            _last = (c.Id, score, spices);
            return score.Quality;
        }

        // ---- engeller ----

        /// <param name="reserved">İlk partinin malzemesi zaten ayrıldı (iş bitiyor): porsiyon katında yalnızca ek partiler sayılır.</param>
        public string Blocker(Character c, ItemInstance item, InteractionDef i, bool reserved = false)
        {
            if (c == null || item == null || i == null) return null;
            switch (i.MealAction)
            {
                case EatServing:
                    return ServingAt(item.Id) == null ? Text(LocText.Of("cook.block.no_serving")) : null;
                case ClearTable:
                    return ServingAt(item.Id) == null && DirtyAt(item.Id) == 0 ? Text(LocText.Of("cook.block.nothing_to_clear")) : null;
                case WashDishes:
                    return Sink <= 0 ? Text(LocText.Of("cook.block.no_dishes")) : null;
            }
            if (i.Dish == null || c.IsVillager) return null;
            if (!_book.Knows(c, i.Dish)) return Text(LocText.Of("cook.block.unknown_recipe", ("name", c.Name)));
            var order = OrderFor(c, item, i);
            if (order == null) return null;
            if (order.Spices.Count > DishScore.SpiceSlots(c.Skills.GetLevel(SkillType.Cooking)))
                return Text(LocText.Of("cook.block.spice_slots", ("n", DishScore.SpiceSlots(c.Skills.GetLevel(SkillType.Cooking)))));
            foreach (var s in order.Spices)
                if (_house.Count(s) < 1) return Text(LocText.Of("cook.block.no_spice", ("item", LocText.Of(ContentKeys.Name(s)))));
            if (order.Batch > 1)
                foreach (var con in i.AllConsumed())
                    if (_house.Count(con.ItemId) < con.Count * (order.Batch - (reserved ? 1 : 0)))
                        return Text(LocText.Of("cook.block.batch", ("n", order.Batch)));
            return null;
        }

        // ---- uygulama ----

        /// <summary>InteractionEffects.AppliedOn: pişirme bitince porsiyon katı, baharat, defter, servis; sofra işleri.</summary>
        public void OnApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (c == null || i == null) return;
            switch (i.MealAction)
            {
                case EatServing: EatPortion(c, item); return;
                case ClearTable: Clear(c, item); return;
                case WashDishes: Sink = 0; return;
            }
            if (i.Dish == null || c.IsVillager) return;
            var dish = i.Dish;
            var order = OrderFor(c, item, i);
            var last = _last.HasValue && _last.Value.cookId == c.Id ? _last : null;
            _last = null;
            int quality = last?.score.Quality ?? 0;
            var spices = last?.spices ?? new List<SpiceDef>();
            foreach (var s in spices) _house.TryTakeItem(s.Id);
            // Seviyeyle çıkan ürün satırı (sıradan / iyi / usta güveç): sofraya o gider.
            int skillLevel = c.Skills.GetLevel(i.Skill ?? dish.Skill) + c.Modifiers.QualityLevels;
            var row = i.Produce[0];
            foreach (var p in i.Produce)
                if ((p.MinLevel == 0 || skillLevel >= p.MinLevel) && (p.MaxLevel == 0 || skillLevel <= p.MaxLevel) && p.Chance >= 1f) { row = p; break; }
            string product = row.ItemId;
            int portions = row.Count;
            // Porsiyon katı: fazladan partilerin malzemesi şimdi harcanır, ürünü aynı kaliteyle eklenir.
            for (int b = 1; order != null && b < order.Batch; b++)
            {
                bool enough = true;
                foreach (var con in i.AllConsumed()) if (_house.Count(con.ItemId) < con.Count) { enough = false; break; }
                if (!enough) break;
                foreach (var con in i.AllConsumed()) for (int k = 0; k < con.Count; k++) _house.TryTake(con.ItemId, TakePolicy.Lowest, out _);
                _house.AddItem(product, row.Count, quality);
                portions += row.Count;
            }
            _book.OnCooked(c, dish, quality);
            Cooked?.Invoke(c, item, dish, last?.score, spices);
            if (order != null) _orders.Remove(c.Id);
            if (!(order?.Serve ?? AutoServe) || !dish.IsMeal) return;
            var table = FreeTableNear(item);
            if (table == null) return;   // masa yok: yemek envanterde kalır (ocak başında)
            int moved = 0;
            for (int k = 0; k < portions; k++) { if (_house.TryTakeQuality(product, quality)) moved++; else break; }
            if (moved == 0) return;
            var serving = new Serving
            {
                TableId = table.Id, DishId = product, Quality = quality, Portions = moved, Total = moved, CookedAt = Now(), CookId = c.Id,
                Burnt = last?.score.Burnt ?? false
            };
            foreach (var s in spices) serving.Spices.Add(s.Id);
            _servings[table.Id] = serving;
            _world.NotifyChanged();
            Served?.Invoke(serving, table, c);
        }

        public static bool IsTable(ItemDef d) => d != null && d.HasTag(TableTag);

        /// <summary>Servis kabının konacağı masa: aynı katta, üstünde servis olmayan en yakın sofra.</summary>
        public ItemInstance FreeTableNear(ItemInstance station)
        {
            ItemInstance best = null; int bestD = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (!IsTable(it.Def) || it.Position.Z != station.Position.Z || _servings.ContainsKey(it.Id)) continue;
                int d = Math.Abs(it.Position.X - station.Position.X) + Math.Abs(it.Position.Y - station.Position.Y);
                if (d < bestD && d <= 24) { best = it; bestD = d; }
            }
            return best;
        }

        /// <summary>Hazır yemeği (envanterdeki porsiyonları) masaya koyar; "Sofraya koy" emri ve piknik dışı ikram.</summary>
        public Serving ServeFromInventory(Character c, ItemInstance table, string dishId, int portions)
        {
            if (table == null || _servings.ContainsKey(table.Id) || !_content.TryDish(dishId, out _)) return null;
            int q = _house.BestQuality(dishId), moved = 0;
            for (int k = 0; k < portions; k++) { if (_house.TryTake(dishId, TakePolicy.Highest, out var took)) { moved++; q = Math.Min(q, took.Quality); } else break; }
            if (moved == 0) return null;
            var s = new Serving { TableId = table.Id, DishId = dishId, Quality = q, Portions = moved, Total = moved, CookedAt = Now(), CookId = c?.Id ?? 0 };
            _servings[table.Id] = s;
            _world.NotifyChanged();
            Served?.Invoke(s, table, c);
            return s;
        }

        private void EatPortion(Character c, ItemInstance table)
        {
            var s = ServingAt(table.Id);
            if (s == null || s.Portions <= 0) return;
            s.Portions--;
            _content.TryDish(s.DishId, out var dish);
            // Yemeğin kendi yeme verisi: doyum ve etki; sofra etkileşimindeki doyum yalnızca yer tutucudur.
            float nominal = 0f;
            foreach (var i in table.Def.Interactions) if (i.MealAction == EatServing) { nominal = i.Restore; break; }
            if (_content.Foods.TryGetValue(s.DishId, out var food))
            {
                float actual = food.Restore * (1f + 0.08f * Math.Min(2, s.Quality)) * (s.Burnt ? 0.85f : 1f);
                float now = c.Needs.Get(NeedType.Hunger);
                if (Math.Abs(actual - nominal) > 0.01f) c.Needs.Set(NeedType.Hunger, Math.Max(0f, Math.Min(100f, now + actual - nominal)));
                if (food.Moodlet != null) c.AddMoodlet(food.Moodlet);
                if (food.GivesMood) c.AddMoodlet(food.MoodId, food.MoodText ?? food.DisplayName, food.MoodValue, food.MoodDays);
            }
            c.RecordMeal(s.DishId, true);
            if (s.Burnt) c.AddMoodlet("cook.hard_to_swallow", Text(LocText.Of("cook.mood.hard_to_swallow")), -2, 0.3);
            else if (s.Stale) c.AddMoodlet("cook.stale", Text(LocText.Of("cook.mood.stale")), -1, 0.3);
            else if (s.Quality >= ItemQuality.Fine)
                c.AddMoodlet("cook.fine_meal", Text(LocText.Of(s.Quality >= ItemQuality.Superior ? "cook.mood.superb_meal" : "cook.mood.fine_meal")), s.Quality >= ItemQuality.Superior ? 4 : 2, 0.5);
            foreach (var sp in s.Spices) if (_content.Spices.TryGet(sp, out var spice) && dish != null && spice.FitsClass(dish.Class)) c.AddMoodlet(spice.Effect);
            if (dish != null && s.Spices.Any(sp => _content.Spices.TryGet(sp, out var x) && !x.FitsClass(dish.Class)))
                c.AddMoodlet("cook.odd_taste", Text(LocText.Of("cook.mood.odd_taste")), -1, 0.3);
            ApplyTaste(c, s.DishId);
            _book.OnAte(c, dish?.Id ?? s.DishId);
            if (dish != null) _book.TryLearnByTaste(c, dish, _rng);

            // Birlikte yeme ve aile sofrası.
            double t = Now();
            s.Eaters.RemoveAll(e => e.id == c.Id);
            s.Eaters.Add((c.Id, t));
            var together = s.Eaters.Where(e => t - e.at <= TogetherWindow).Select(e => Find?.Invoke(e.id)).Where(x => x != null && !x.IsDead).ToList();
            if (together.Count >= 2)
                foreach (var m in together)
                {
                    if (m.AddMoodlet("cook.ate_together", Text(LocText.Of("cook.mood.ate_together")), 3, 0.5)) m.Needs.Restore(NeedType.Social, 12f);
                    if ((m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) && together.Any(o => o.Id == m.BirthMotherId || o.Id == m.BirthFatherId)
                        && m.AddMoodlet("cook.family_care", Text(LocText.Of("cook.mood.family_care")), 4, 1.0))
                        _book.OnFamilyMeal(m);
                }
            _dirty.TryGetValue(table.Id, out int d);
            _dirty[table.Id] = d + 1;
            Ate?.Invoke(c, s, dish);
            if (s.Portions <= 0) _servings.Remove(table.Id);
            _world.NotifyChanged();
        }

        /// <summary>Damak tadı (60b §5.2): sevilen, sevilmeyen ve ölmüş ebeveynin yemeği. Kilerden yenen yemekte de çağrılır.</summary>
        public void ApplyTaste(Character c, string dishId)
        {
            if (c == null || c.IsVillager || !_content.TryDish(dishId, out var tasted)) return;
            dishId = tasted.Id;
            var tastes = _book.TastesOf(c, Find);
            if (tastes != null && tastes.Loves(dishId)) c.AddMoodlet("cook.loved_dish", Text(LocText.Of("cook.mood.loved_dish")), 3, 0.5);
            else if (tastes != null && tastes.Disliked == dishId) c.AddMoodlet("cook.disliked_dish", Text(LocText.Of("cook.mood.disliked_dish")), -2, 0.3);
            int parent = _book.ParentDish(c, dishId, Find);
            if (parent != 0) c.AddMoodlet("cook.parent_dish", Text(LocText.Of(parent == 1 ? "cook.mood.mothers_dish" : "cook.mood.fathers_dish")), 5, 1.0);
        }

        /// <summary>Kedi bir porsiyon kapar (60c §2.5); alınan yemeğin kimliği, yoksa null.</summary>
        public string StealPortion(int tableId)
        {
            var s = ServingAt(tableId);
            if (s == null || s.Portions <= 0) return null;
            s.Portions--;
            string id = s.DishId;
            if (s.Portions <= 0) _servings.Remove(tableId);
            _world.NotifyChanged();
            return id;
        }

        /// <summary>Misafire ya da tezgâha bir porsiyon ayırır; kalite döner (yoksa −1).</summary>
        public int TakePortion(int tableId)
        {
            var s = ServingAt(tableId);
            if (s == null || s.Portions <= 0) return -1;
            int q = s.Quality;
            StealPortion(tableId);
            return q;
        }

        private void Clear(Character c, ItemInstance table)
        {
            var s = ServingAt(table.Id);
            if (s != null)
            {
                int q = Math.Max(0, s.Quality - (s.Stale ? 1 : 0));
                if (s.Portions > 0) _house.AddItem(s.DishId, s.Portions, q);
                _servings.Remove(table.Id);
            }
            if (_dirty.TryGetValue(table.Id, out int d)) { Sink += d; _dirty.Remove(table.Id); }
            _world.NotifyChanged();
        }

        /// <summary>Sürekli: açıkta kalan yemek bayatlar ve bozulur (58 §3.2, 57b).</summary>
        public void Tick()
        {
            double now = Now();
            List<int> gone = null;
            foreach (var kv in _servings)
            {
                var s = kv.Value;
                double age = now - s.CookedAt;
                if (!s.Stale && age >= StaleAfter) s.Stale = true;
                if (age >= SpoilAfter) (gone ?? (gone = new List<int>())).Add(kv.Key);
            }
            if (gone == null) return;
            foreach (int id in gone)
            {
                var s = _servings[id];
                _servings.Remove(id);
                _dirty.TryGetValue(id, out int d);
                _dirty[id] = d + 1;   // bozulan yemeğin kabı da yıkanacak
                var table = _world.GetItem(id);
                Notice?.Invoke(LocText.Of("cook.n.spoiled", ("item", LocText.Of(ContentKeys.Name(s.DishId))), ("n", s.Portions)), null);
                Spoiled?.Invoke(s, table);
            }
            _world.NotifyChanged();
        }

        /// <summary>Ev ve Mutfak görevinin sofra işleri: toplanacak masa, yıkanacak bulaşık.</summary>
        public IEnumerable<(ItemInstance item, InteractionDef interaction)> Chores()
        {
            double now = Now();
            foreach (var it in _world.Items)
            {
                if (!IsTable(it.Def)) continue;
                var s = ServingAt(it.Id);
                bool leftover = s != null && now - s.CookedAt >= 0.12;
                if (!leftover && DirtyAt(it.Id) == 0) continue;
                if (s != null && !leftover) continue;   // sofra hâlâ kurulu: yiyenler bitirsin
                var i = it.Def.Interactions.FirstOrDefault(x => x.MealAction == ClearTable);
                if (i != null) yield return (it, i);
            }
            if (Sink > 0)
                foreach (var it in _world.Items)
                {
                    var i = it.Def.Interactions.FirstOrDefault(x => x.MealAction == WashDishes);
                    if (i != null) { yield return (it, i); break; }
                }
        }

        public bool AnyServing => _servings.Count > 0;

        // ---- kayıt: s:<masa>:<yemek>:<kalite>:<kalan>:<toplam>:<an>:<aşçı>:<yanık>:<baharat,…> | d:<masa>=<n> | k:<n> ----

        public string[] Export()
        {
            var l = new List<string>();
            string I(int v) => v.ToString(CultureInfo.InvariantCulture);
            foreach (var s in _servings.Values)
                l.Add(string.Join(":", "s", I(s.TableId), s.DishId, I(s.Quality), I(s.Portions), I(s.Total),
                    s.CookedAt.ToString("R", CultureInfo.InvariantCulture), I(s.CookId), s.Burnt ? "1" : "0", string.Join(",", s.Spices)));
            foreach (var kv in _dirty) if (kv.Value > 0) l.Add("d:" + I(kv.Key) + "=" + I(kv.Value));
            if (Sink > 0) l.Add("k:" + I(Sink));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _servings.Clear(); _dirty.Clear(); _orders.Clear(); Sink = 0;
            if (data == null) return;
            foreach (var line in data)
            {
                var p = (line ?? "").Split(':');
                if (p[0] == "s" && p.Length >= 10 && int.TryParse(p[1], out int table) && int.TryParse(p[3], out int q) && int.TryParse(p[4], out int left)
                    && int.TryParse(p[5], out int total) && double.TryParse(p[6], NumberStyles.Float, CultureInfo.InvariantCulture, out double at) && int.TryParse(p[7], out int cook))
                {
                    var s = new Serving { TableId = table, DishId = p[2], Quality = q, Portions = left, Total = total, CookedAt = at, CookId = cook, Burnt = p[8] == "1" };
                    foreach (var sp in p[9].Split(',')) if (sp.Length > 0) s.Spices.Add(sp);
                    if (left > 0 && _content.Items.Contains(s.DishId)) _servings[table] = s;
                }
                else if (p[0] == "d" && p.Length == 2)
                {
                    var e = p[1].Split('=');
                    if (e.Length == 2 && int.TryParse(e[0], out int t) && int.TryParse(e[1], out int n)) _dirty[t] = n;
                }
                else if (p[0] == "k" && p.Length == 2 && int.TryParse(p[1], out int sink)) Sink = sink;
            }
        }
    }
}
