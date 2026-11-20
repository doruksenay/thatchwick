using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Cooking
{
    public enum MealSlot { Morning = 0, Evening = 1 }

    /// <summary>Öğün çizelgesinin bir satırı: hangi yemek, kaç kat, hangi baharat.</summary>
    public sealed class MealPlanEntry
    {
        public string DishId;
        public int Batch = 1;
        public List<string> Spices = new List<string>();
    }

    /// <summary>Kiler planının bir sınıfı (belge 60b §5.4).</summary>
    public sealed class PantryLine
    {
        public string ClassKey;
        public int Count;
        public float Meals;
    }

    /// <summary>
    /// Mutfak görevi (belge 58 §9, 58a §4, 60b §5.4; kararlar Y8, 62-S7):
    /// <list type="bullet">
    /// <item>Oyuncu her öğün yuvasına (sabah, akşam) sıralı bir yemek listesi koyar; görevli sıradakini pişirir, sofrayı
    /// kurar. <b>Görevli kendi başına tarif seçmez:</b> malzeme yoksa pişirmez, bildirim verir.</item>
    /// <item>Sofrayı toplar, bulaşığı yıkar, mezbaha deposunu kilere taşır.</item>
    /// <item>Kiler planı: saklama hedefi verilen ürün hedefin altındaysa ve malzemesi varsa saklamaya çevirir.</item>
    /// </list>
    /// </summary>
    public sealed class KitchenDuty
    {
        /// <summary>Öğün pişirme pencereleri (günün kesri).</summary>
        public static readonly (double from, double until)[] Windows = { (0.26, 0.42), (0.68, 0.86) };
        /// <summary>Bir yetişkinin günde yediği öğün (kiler planı tahmini).</summary>
        public const float MealsPerDay = 2.5f;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly MealService _meals;
        private readonly RecipeBook _book;
        private readonly List<MealPlanEntry>[] _plan = { new List<MealPlanEntry>(), new List<MealPlanEntry>() };
        private readonly int[] _rotation = new int[2];
        private readonly long[] _doneDay = { -1, -1 };
        private readonly long[] _warnedDay = { -1, -1 };
        private readonly Dictionary<string, int> _targets = new Dictionary<string, int>(StringComparer.Ordinal);

        public KitchenDuty(WorldState world, Household house, ContentDatabase content, MealService meals, RecipeBook book)
        {
            _world = world; _house = house; _content = content; _meals = meals; _book = book;
        }

        public Func<double> Now { get; set; } = () => 0;
        public Func<Character, ItemInstance, InteractionDef, string> CanStart { get; set; }
        public Func<IEnumerable<(ItemInstance item, InteractionDef interaction)>> ExtraChores { get; set; }
        public event Action<LocText, Character> Notice;

        public IReadOnlyList<MealPlanEntry> Plan(MealSlot slot) => _plan[(int)slot];
        public IReadOnlyDictionary<string, int> Targets => _targets;

        public bool Add(MealSlot slot, string dishId, int batch = 1, IEnumerable<string> spices = null)
        {
            if (!_content.Dishes.TryGet(dishId, out var dish) || !dish.IsMeal || _plan[(int)slot].Count >= 7) return false;
            var e = new MealPlanEntry { DishId = dishId, Batch = batch >= 4 ? 4 : batch >= 2 ? 2 : 1 };
            if (spices != null) foreach (var s in spices) if (_content.Spices.Contains(s)) e.Spices.Add(s);
            _plan[(int)slot].Add(e);
            return true;
        }

        public bool RemoveAt(MealSlot slot, int index)
        {
            var l = _plan[(int)slot];
            if (index < 0 || index >= l.Count) return false;
            l.RemoveAt(index);
            return true;
        }

        public void SetTarget(string itemId, int count)
        {
            if (!_content.Items.Contains(itemId)) return;
            if (count <= 0) _targets.Remove(itemId); else _targets[itemId] = Math.Min(999, count);
        }

        /// <summary>Şu an hangi öğün penceresindeyiz; değilse null.</summary>
        public MealSlot? CurrentSlot()
        {
            double f = Now() - Math.Floor(Now());
            for (int k = 0; k < Windows.Length; k++) if (f >= Windows[k].from && f < Windows[k].until) return (MealSlot)k;
            return null;
        }

        /// <summary>Sıradaki yemek (çizelge döner).</summary>
        public MealPlanEntry NextEntry(MealSlot slot)
        {
            var l = _plan[(int)slot];
            return l.Count == 0 ? null : l[_rotation[(int)slot] % l.Count];
        }

        /// <summary>Görevlinin bu yemeği pişirebileceği istasyon ve etkileşim (bildiği, seviyesi yeten ilk kaynak).</summary>
        public (ItemInstance item, InteractionDef interaction) Resolve(Character c, string dishId, out string why)
        {
            why = null;
            if (!_content.Dishes.TryGet(dishId, out var dish)) return (null, null);
            if (c != null && !_book.Knows(c, dish)) { why = "unknown"; return (null, null); }
            (ItemInstance, InteractionDef) best = (null, null);
            foreach (var (station, i) in dish.Sources.OrderByDescending(s => s.interaction.MinSkillLevel))
            {
                if (c != null && c.Skills.GetLevel(i.Skill ?? dish.Skill) < i.MinSkillLevel) { why = why ?? "level"; continue; }
                foreach (var it in _world.Items)
                {
                    if (it.Def != station) continue;
                    string blocked = c != null && CanStart != null ? CanStart(c, it, i) : null;
                    if (blocked == null) return (it, i);
                    why = blocked;
                    if (best.Item1 == null) best = (it, i);
                }
                if (best.Item1 == null && why == null) why = "station";
            }
            return (null, null);
        }

        /// <summary>
        /// Mutfak görevlisinin işleri, öncelik sırasıyla (DutyPlanner.KitchenChores). Karakter null ise yalnızca sayım.
        /// </summary>
        public IEnumerable<(ItemInstance item, InteractionDef interaction)> Chores(Character c)
        {
            long day = (long)Math.Floor(Now());
            var slot = CurrentSlot();
            if (slot.HasValue && _doneDay[(int)slot.Value] != day)
            {
                int s = (int)slot.Value;
                var entry = NextEntry(slot.Value);
                if (entry != null)
                {
                    var (item, inter) = Resolve(c, entry.DishId, out string why);
                    if (item != null)
                    {
                        if (c != null) _meals.Order(c, item, inter.Id, entry.Batch, entry.Spices, serve: true);
                        // Porsiyon katı ya da baharat yetmiyorsa emir engellenir: görevli başka tarife geçmez.
                        string blocked = c != null ? _meals.Blocker(c, item, inter) : null;
                        if (blocked == null) yield return (item, inter);
                        else { if (c != null) _meals.ClearOrder(c); Warn(s, day, c, entry, blocked); }
                    }
                    else if (c != null) Warn(s, day, c, entry, why);
                }
            }
            foreach (var chore in _meals.Chores()) yield return chore;
            if (ExtraChores != null) foreach (var chore in ExtraChores()) yield return chore;
            // Kiler planı: hedefin altındaki saklanmış ürünü, malzemesi varsa yapar.
            foreach (var kv in _targets)
            {
                if (_house.Count(kv.Key) >= kv.Value) continue;
                var made = Producer(c, kv.Key);
                if (made.item != null) yield return made;
            }
        }

        private void Warn(int slot, long day, Character c, MealPlanEntry entry, string why)
        {
            if (_warnedDay[slot] == day) return;
            _warnedDay[slot] = day;
            string key = why == "unknown" ? "cook.n.duty_unknown" : why == "level" ? "cook.n.duty_level" : why == "station" ? "cook.n.duty_station" : "cook.n.duty_missing";
            Notice?.Invoke(LocText.Of(key, ("name", c?.Name ?? ""), ("meal", LocText.Of(slot == 0 ? "cook.slot.morning" : "cook.slot.evening")),
                ("item", LocText.Of(ContentKeys.Name(entry.DishId))), ("why", why ?? "")), c);
        }

        private (ItemInstance item, InteractionDef interaction) Producer(Character c, string product)
        {
            foreach (var it in _world.Items)
                foreach (var i in it.Def.Interactions)
                {
                    if (i.Produce.Count == 0 || i.Produce[0].ItemId != product || i.RemovesItem || i.RequiresGrown) continue;
                    if (c != null && i.Skill.HasValue && c.Skills.GetLevel(i.Skill.Value) < i.MinSkillLevel) continue;
                    if (c != null && CanStart != null && CanStart(c, it, i) != null) continue;
                    if (c == null) { bool ok = true; foreach (var con in i.AllConsumed()) if (_house.Count(con.ItemId) < con.Count) { ok = false; break; } if (!ok) continue; }
                    return (it, i);
                }
            return (null, null);
        }

        /// <summary>Çizelgedeki yemek pişti: öğün tamam, çizelge bir sonrakine döner.</summary>
        public void OnCooked(Character c, DishDef dish)
        {
            if (c == null || c.Duty != DutyKind.Kitchen) return;
            var slot = CurrentSlot();
            if (!slot.HasValue) return;
            var entry = NextEntry(slot.Value);
            if (entry == null || entry.DishId != dish.Id) return;
            _doneDay[(int)slot.Value] = (long)Math.Floor(Now());
            _rotation[(int)slot.Value]++;
        }

        // ---- kiler planı ----

        public static string PantryClass(ItemDef d)
        {
            if (d.HasTag("grain") || d.Id.Contains("flour") || d.Id.Contains("bread") || d.Id.Contains("rusk") || d.Id.Contains("tarhana")) return "grain";
            if (d.HasTag("salted") || d.Id.Contains("smoked_") || d.Id.Contains("pastirma") || d.HasTag("raw")) return "meat";
            if (d.Id.Contains("pickled_") || d.Id.Contains("brined")) return "pickle";
            if (d.Id.Contains("dried_") || d.Id.Contains("jam_") || d.Id.Contains("pekmez") || d.Id.Contains("honey")) return "dried";
            if (d.Id.Contains("cheese") || d.Id.Contains("butter") || d.Id.Contains("yogurt") || d.Id.Contains("kasar") || d.Id.Contains("milk")) return "dairy";
            if (d.HasTag("cooked")) return "cooked";
            return "fresh";
        }

        /// <summary>Kiler dökümü: sınıf sınıf adet ve kaç öğünlük; <paramref name="days"/> hanenin kaç günlük yiyeceği olduğu.</summary>
        public List<PantryLine> Pantry(int eaters, out float days)
        {
            var lines = new Dictionary<string, PantryLine>(StringComparer.Ordinal);
            float meals = 0f;
            foreach (var kv in _house.InventoryList())
            {
                if (!_content.Items.TryGet(kv.Key, out var d) || !d.HasTag("food") || !_content.Foods.TryGetValue(kv.Key, out var food)) continue;
                if (food.Need != NeedType.Hunger) continue;
                string cls = PantryClass(d);
                if (!lines.TryGetValue(cls, out var line)) lines[cls] = line = new PantryLine { ClassKey = "cook.pantry." + cls };
                float m = kv.Value * food.Restore / 60f;   // bir öğün ≈ 60 doyum
                line.Count += kv.Value; line.Meals += m; meals += m;
            }
            days = eaters > 0 ? meals / (eaters * MealsPerDay) : 0f;
            return lines.Values.OrderByDescending(l => l.Meals).ToList();
        }

        // ---- kayıt: p:<yuva>:<yemek>*<kat>*<baharat+…>;… | r:<a>,<b> | g:<eşya>=<n> | o:<gün>,<gün> ----

        public string[] Export()
        {
            var l = new List<string>();
            for (int s = 0; s < 2; s++)
                if (_plan[s].Count > 0)
                    l.Add("p:" + s + ":" + string.Join(";", _plan[s].Select(e => e.DishId + "*" + e.Batch + "*" + string.Join("+", e.Spices))));
            l.Add("r:" + _rotation[0] + "," + _rotation[1]);
            l.Add("o:" + _doneDay[0].ToString(CultureInfo.InvariantCulture) + "," + _doneDay[1].ToString(CultureInfo.InvariantCulture));
            foreach (var kv in _targets) l.Add("g:" + kv.Key + "=" + kv.Value);
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _plan[0].Clear(); _plan[1].Clear(); _targets.Clear();
            _rotation[0] = _rotation[1] = 0; _doneDay[0] = _doneDay[1] = -1; _warnedDay[0] = _warnedDay[1] = -1;
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 3) continue;
                string body = line.Substring(2);
                switch (line[0])
                {
                    case 'p':
                    {
                        int cut = body.IndexOf(':');
                        if (cut != 1 || !int.TryParse(body.Substring(0, 1), out int s) || s < 0 || s > 1) break;
                        foreach (var e in body.Substring(2).Split(';'))
                        {
                            var p = e.Split('*');
                            if (p.Length != 3 || !_content.Dishes.Contains(p[0]) || !int.TryParse(p[1], out int b)) continue;
                            Add((MealSlot)s, p[0], b, p[2].Split('+'));
                        }
                        break;
                    }
                    case 'r': { var p = body.Split(','); if (p.Length == 2) { int.TryParse(p[0], out _rotation[0]); int.TryParse(p[1], out _rotation[1]); } break; }
                    case 'o': { var p = body.Split(','); if (p.Length == 2) { long.TryParse(p[0], out _doneDay[0]); long.TryParse(p[1], out _doneDay[1]); } break; }
                    case 'g': { var p = body.Split('='); if (p.Length == 2 && int.TryParse(p[1], out int n)) SetTarget(p[0], n); break; }
                }
            }
        }
    }

    /// <summary>
    /// Günlük rutin (belge 60b §5.1, F1): karakter başına sabah ve akşam blokları. Emri ve acil ihtiyacı olmayan karakter
    /// rutinini izler; öncelik: oyuncu emri > acil ihtiyaç > rutin > görev > boşta hareket.
    /// </summary>
    public sealed class Routines
    {
        /// <summary>Bloklar: wash (yüzünü yıka), bath (yıkan), meal (sofra ya da kiler), fireside (ateş başı), tidy (sofrayı topla).</summary>
        public static readonly string[] Blocks = { "wash", "bath", "meal", "fireside", "tidy" };
        public static readonly string[] DefaultMorning = { "wash", "meal" };
        public static readonly string[] DefaultEvening = { "meal", "fireside" };
        public static readonly (double from, double until)[] Windows = { (0.25, 0.40), (0.74, 0.92) };

        private sealed class State
        {
            public List<string>[] Lists = { new List<string>(), new List<string>() };
            public long Day = -1; public int Part = -1, Index;
        }

        private readonly WorldState _world;
        private readonly Dictionary<int, State> _states = new Dictionary<int, State>();

        public Routines(WorldState world) { _world = world; }

        public Func<double> Now { get; set; } = () => 0;
        public Func<Character, ItemInstance, InteractionDef, string> CanStart { get; set; }
        public Func<int, bool> HasServing { get; set; }

        private State Get(int id) { if (!_states.TryGetValue(id, out var s)) _states[id] = s = new State(); return s; }
        public bool HasRoutine(Character c) => c != null && _states.TryGetValue(c.Id, out var s) && (s.Lists[0].Count > 0 || s.Lists[1].Count > 0);
        public IReadOnlyList<string> Of(Character c, int part) => c != null && _states.TryGetValue(c.Id, out var s) ? s.Lists[part] : (IReadOnlyList<string>)Array.Empty<string>();

        public void Set(Character c, int part, IEnumerable<string> blocks)
        {
            if (c == null || part < 0 || part > 1) return;
            var s = Get(c.Id);
            s.Lists[part].Clear();
            if (blocks != null) foreach (var b in blocks) if (Array.IndexOf(Blocks, b) >= 0 && s.Lists[part].Count < 6) s.Lists[part].Add(b);
            s.Part = -1;
        }

        public void UseDefaults(Character c) { Set(c, 0, DefaultMorning); Set(c, 1, DefaultEvening); }

        /// <summary>JobRunner.RoutineProvider: rutinin sıradaki bloğu için iş; yoksa null.</summary>
        public Job Next(Character c)
        {
            if (c == null || c.IsVillager || c.Stage == AgeStage.Baby || !_states.TryGetValue(c.Id, out var s)) return null;
            double now = Now(), f = now - Math.Floor(now);
            long day = (long)Math.Floor(now);
            int part = f >= Windows[0].from && f < Windows[0].until ? 0 : f >= Windows[1].from && f < Windows[1].until ? 1 : -1;
            if (part < 0) return null;
            if (s.Day != day || s.Part != part) { s.Day = day; s.Part = part; s.Index = 0; }
            while (s.Index < s.Lists[part].Count)
            {
                string block = s.Lists[part][s.Index++];
                var job = JobFor(c, block);
                if (job != null) return job.WithNote("routine:" + block);
            }
            return null;
        }

        private Job JobFor(Character c, string block)
        {
            ItemInstance best = null; InteractionDef bestI = null; int bestD = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (it.Position.Z != c.Position.Z) continue;
                foreach (var i in it.Def.Interactions)
                {
                    if (!Fits(block, it, i, c)) continue;
                    if (CanStart != null && CanStart(c, it, i) != null) continue;
                    int d = Math.Abs(it.Position.X - c.Position.X) + Math.Abs(it.Position.Y - c.Position.Y);
                    if (block == "meal" && i.MealAction == MealService.EatServing) d -= 1000;   // sofra kuruluysa önce sofra
                    if (d < bestD) { best = it; bestI = i; bestD = d; }
                    break;
                }
            }
            return best == null ? null : Job.UseItem(best.Id, bestI.Id);
        }

        public const float WashMaxRestore = 50f;

        private bool Fits(string block, ItemInstance it, InteractionDef i, Character c)
        {
            switch (block)
            {
                // Yüz yıkama küçük bir temizliktir (leğen, kova); yıkanma küvet ve yıkanma kulübesidir.
                case "wash": return i.RestoresNeed && i.Need == NeedType.Hygiene && i.Restore <= WashMaxRestore;
                case "bath": return i.RestoresNeed && i.Need == NeedType.Hygiene && i.Restore > WashMaxRestore;
                case "meal":
                    if (c.Needs.Get(NeedType.Hunger) > 85f) return false;
                    if (i.MealAction == MealService.EatServing) return HasServing == null || HasServing(it.Id);
                    return CookingContentMapper.IsPlainEat(i) && i.Need == NeedType.Hunger && it.Def.HasTag(CookingContentMapper.FoodPointTag);
                case "fireside": return i.RestoresNeed && i.Need == NeedType.Warmth && it.Def.HeatOutput > 0f && it.IsLit;
                case "tidy": return i.MealAction == MealService.ClearTable;
                default: return false;
            }
        }

        // kayıt: <kişi>:<sabah,…>:<akşam,…>
        public string[] Export() => _states.Where(kv => kv.Value.Lists[0].Count + kv.Value.Lists[1].Count > 0)
            .Select(kv => kv.Key.ToString(CultureInfo.InvariantCulture) + ":" + string.Join(",", kv.Value.Lists[0]) + ":" + string.Join(",", kv.Value.Lists[1])).ToArray();

        public void Import(string[] data)
        {
            _states.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                var p = (line ?? "").Split(':');
                if (p.Length != 3 || !int.TryParse(p[0], out int id)) continue;
                var s = Get(id);
                for (int part = 0; part < 2; part++)
                    foreach (var b in p[part + 1].Split(',')) if (Array.IndexOf(Blocks, b) >= 0) s.Lists[part].Add(b);
            }
        }
    }
}
