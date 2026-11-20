using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Cooking
{
    public enum RecipeState { Unknown = 0, Heard = 1, Known = 2 }

    /// <summary>Yemek defterinin bir sayfası (belge 60b §5.3).</summary>
    public sealed class RecipePage
    {
        public DishDef Dish;
        public RecipeState State;
        public bool Written;
        public List<Character> Knowers = new List<Character>();
        public List<Character> Lovers = new List<Character>();
        public int TimesCooked, BestQuality;
        public string BestCook;
        /// <summary>Yazılmamış tarifi hanede tek kişi biliyor: ölürse tarif kaybolur.</summary>
        public bool OnlyKnower;
    }

    /// <summary>Bir kişinin damak tadı (belge 60b §5.2): iki sevdiği, bir sevmediği yemek.</summary>
    public sealed class Tastes
    {
        public string Loved1, Loved2, Disliked;
        public bool Loves(string dish) => dish != null && (dish == Loved1 || dish == Loved2);
    }

    /// <summary>
    /// Tarif bilgisi ve yemek defteri (belge 58 §4, 60b §5.2-5.3; kararlar Y2-b, 62-S4):
    /// <list type="bullet">
    /// <item>Tarif bilgisi <b>kişide</b>; yazılı defter hanede. Temel tarifleri herkes bilir, seviye tarifini beceri açar.</item>
    /// <item>Öğrenme: köylü yakınlığı, tüccar risalesi, istek ödülü, defterden okuma, hanede öğretme, tadarak.</item>
    /// <item>Yazılmamış tarif bilen ölünce kaybolur.</item>
    /// <item>"Eli alıştı": bir tarifi 5, 15 ve 40 kez pişiren kalite puanında tarif ustalığı payı kazanır.</item>
    /// <item>Damak tadı: sevilen yemeklerden biri çocuklukta en çok yenendir, öteki ebeveynden geçer.</item>
    /// </list>
    /// </summary>
    public sealed class RecipeBook
    {
        private readonly ContentDatabase _content;
        private readonly Dictionary<int, HashSet<string>> _known = new Dictionary<int, HashSet<string>>();
        private readonly Dictionary<int, Dictionary<string, int>> _cooked = new Dictionary<int, Dictionary<string, int>>();
        private readonly Dictionary<int, Dictionary<string, int>> _eatenAsChild = new Dictionary<int, Dictionary<string, int>>();
        private readonly Dictionary<int, Tastes> _tastes = new Dictionary<int, Tastes>();
        private readonly Dictionary<string, (int quality, string cook)> _best = new Dictionary<string, (int, string)>(StringComparer.Ordinal);
        private readonly HashSet<string> _written = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<int, int> _familyMeals = new Dictionary<int, int>();

        public RecipeBook(ContentDatabase content) { _content = content; }

        /// <summary>Yeni tarif öğrenildi (kişi, yemek, kaynak anahtarı: villager, treatise, request, book, taught, taste).</summary>
        public event Action<Character, DishDef, string> Learned;
        /// <summary>"Eli alıştı" basamağı (kişi, yemek, basamak 1-3).</summary>
        public event Action<Character, DishDef, int> HandSettled;

        public IEnumerable<DishDef> Dishes => _content.Dishes.All;
        public bool IsWritten(string dishId) => dishId != null && _written.Contains(dishId);

        private HashSet<string> KnownSet(int id) { if (!_known.TryGetValue(id, out var s)) _known[id] = s = new HashSet<string>(StringComparer.Ordinal); return s; }

        /// <summary>Kişi bu yemeği biliyor mu. Köylüler muaftır (aletleri ve tarifleri var sayılır).</summary>
        public bool Knows(Character c, DishDef dish)
        {
            if (c == null || dish == null) return false;
            if (c.IsVillager) return true;
            switch (dish.Learn)
            {
                case LearnKind.Basic: return true;
                case LearnKind.Level: return c.Skills.GetLevel(dish.Skill) >= dish.MinLevel || HasLearned(c, dish.Id);
                default: return HasLearned(c, dish.Id);
            }
        }

        public bool HasLearned(Character c, string dishId) => c != null && _known.TryGetValue(c.Id, out var s) && s.Contains(dishId);

        /// <summary>Tarifi öğretir; zaten biliyorsa false.</summary>
        public bool Learn(Character c, string dishId, string source)
        {
            if (c == null || c.IsDead || !_content.Dishes.TryGet(dishId, out var dish)) return false;
            if (Knows(c, dish)) return false;
            KnownSet(c.Id).Add(dishId);
            Learned?.Invoke(c, dish, source);
            return true;
        }

        public int TimesCooked(Character c, string dishId) =>
            c != null && _cooked.TryGetValue(c.Id, out var d) && d.TryGetValue(dishId, out int n) ? n : 0;

        /// <summary>"Eli alıştı" basamağı: 0 (yok), 1 (5 kez), 2 (15), 3 (40).</summary>
        public int MasteryTier(Character c, string dishId)
        {
            int n = TimesCooked(c, dishId), tier = 0;
            foreach (int at in DishScore.MasteryAt) if (n >= at) tier++;
            return tier;
        }

        /// <summary>Yemek pişti: sayaç, en iyi kalite ve "eli alıştı" basamağı.</summary>
        public void OnCooked(Character c, DishDef dish, int quality)
        {
            if (c == null || dish == null || c.IsVillager) return;
            if (!_cooked.TryGetValue(c.Id, out var d)) _cooked[c.Id] = d = new Dictionary<string, int>(StringComparer.Ordinal);
            int before = MasteryTier(c, dish.Id);
            d.TryGetValue(dish.Id, out int n);
            d[dish.Id] = n + 1;
            if (!_best.TryGetValue(dish.Id, out var b) || quality > b.quality) _best[dish.Id] = (quality, c.Name);
            int after = MasteryTier(c, dish.Id);
            if (after > before) HandSettled?.Invoke(c, dish, after);
        }

        /// <summary>Kişinin bildiği, deftere yazılmamış tarifleri yazar; yazılan sayısı döner.</summary>
        public int WriteDown(Character c)
        {
            if (c == null) return 0;
            int n = 0;
            foreach (var dish in _content.Dishes.All)
                if (dish.Learn != LearnKind.Basic && Knows(c, dish) && _written.Add(dish.Id)) n++;
            return n;
        }

        public bool HasUnwritten(Character c)
        {
            foreach (var dish in _content.Dishes.All)
                if (dish.Learn != LearnKind.Basic && Knows(c, dish) && !_written.Contains(dish.Id)) return true;
            return false;
        }

        /// <summary>Defterden bilmediği bir tarifi okur (kapısı en düşük olan); yoksa null.</summary>
        public DishDef ReadFromBook(Character c)
        {
            DishDef pick = NextReadable(c);
            if (pick != null) Learn(c, pick.Id, "book");
            return pick;
        }

        public DishDef NextReadable(Character c)
        {
            DishDef pick = null;
            foreach (var id in _written)
                if (_content.Dishes.TryGet(id, out var dish) && !Knows(c, dish) && (pick == null || dish.MinLevel < pick.MinLevel)) pick = dish;
            return pick;
        }

        /// <summary>Hanede öğretme: öğreten bildiği, öğrencinin bilmediği bir tarifi aktarır.</summary>
        public DishDef Teach(Character teacher, Character pupil)
        {
            if (teacher == null || pupil == null) return null;
            foreach (var dish in _content.Dishes.All)
                if (dish.Learn != LearnKind.Basic && Knows(teacher, dish) && !Knows(pupil, dish)) { Learn(pupil, dish.Id, "taught"); return dish; }
            return null;
        }

        /// <summary>
        /// Köylü tarifleri (belge 58 §4): yakınlık eşiği aşılınca köylü tarifini, kendisine en yakın hane üyesine verir.
        /// <paramref name="closeness"/> (hane üyesi, köylü tanım kimliği) → yakınlık; köylü yoksa negatif.
        /// </summary>
        public void PollVillagerRecipes(IEnumerable<Character> members, Func<Character, string, int> closeness)
        {
            if (closeness == null) return;
            var list = members.Where(m => !m.IsDead && !m.IsVillager && m.Stage >= AgeStage.Teen).ToList();
            foreach (var dish in _content.Dishes.All)
            {
                if (dish.Learn != LearnKind.Villager) continue;
                Character best = null; int bestC = -1;
                foreach (var m in list)
                {
                    int cl = closeness(m, dish.LearnFrom);
                    if (cl >= dish.LearnAt && cl > bestC && !Knows(m, dish)) { best = m; bestC = cl; }
                }
                if (best != null && !list.Any(m => Knows(m, dish))) Learn(best, dish.Id, "villager");
            }
        }

        /// <summary>İstek ödülü ya da risale: bilinmeyen bir tarif (verilen türden) öğretir.</summary>
        public DishDef GrantUnknown(Character c, LearnKind kind, Random rng)
        {
            var pool = _content.Dishes.All.Where(d => d.Learn == kind && !Knows(c, d)).ToList();
            if (pool.Count == 0) return null;
            var dish = pool[rng.Next(pool.Count)];
            Learn(c, dish.Id, kind == LearnKind.Request ? "request" : "treatise");
            return dish;
        }

        /// <summary>Tadarak öğrenme: bilmediği yemeği yiyen, becerisi kapının iki üstündeyse dörtte bir şansla çıkarır.</summary>
        public bool TryLearnByTaste(Character c, DishDef dish, Random rng)
        {
            if (c == null || dish == null || Knows(c, dish) || dish.Learn == LearnKind.Family) return false;
            if (c.Skills.GetLevel(dish.Skill) < dish.MinLevel + 2) return false;
            return rng.NextDouble() < 0.25 && Learn(c, dish.Id, "taste");
        }

        // ---- damak tadı ----

        public void OnAte(Character c, string dishId)
        {
            if (c == null || c.Stage != AgeStage.Child || !_content.Dishes.Contains(dishId)) return;
            if (!_eatenAsChild.TryGetValue(c.Id, out var d)) _eatenAsChild[c.Id] = d = new Dictionary<string, int>(StringComparer.Ordinal);
            d.TryGetValue(dishId, out int n);
            d[dishId] = n + 1;
        }

        /// <summary>Hane sofrasında ailesiyle yenen öğün (çocuk ve ergen): "sofrada büyümek" izi için sayılır.</summary>
        public void OnFamilyMeal(Character c) { _familyMeals.TryGetValue(c.Id, out int n); _familyMeals[c.Id] = n + 1; }
        public int FamilyMeals(Character c) => c != null && _familyMeals.TryGetValue(c.Id, out int n) ? n : 0;

        /// <summary>Damak tadı; bebekte yok. Çocukta geçicidir (en çok yediği), ergenlikte kalıcılaşır.</summary>
        public Tastes TastesOf(Character c, Func<int, Character> find = null)
        {
            if (c == null || c.Stage == AgeStage.Baby) return null;
            if (_tastes.TryGetValue(c.Id, out var fixedTastes)) return fixedTastes;
            var meals = _content.Dishes.All.Where(d => d.IsMeal).Select(d => d.Id).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (meals.Count < 3) return null;
            var rng = new Random(c.Id * 7919 + 17);
            var t = new Tastes();
            if (_eatenAsChild.TryGetValue(c.Id, out var eaten) && eaten.Count > 0)
                t.Loved1 = eaten.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.Ordinal).First().Key;
            foreach (int? pid in new[] { c.BirthMotherId, c.BirthFatherId })
            {
                if (t.Loved2 != null || !pid.HasValue) continue;
                if (_tastes.TryGetValue(pid.Value, out var pt) && pt.Loved1 != null && pt.Loved1 != t.Loved1) t.Loved2 = pt.Loved1;
            }
            if (t.Loved1 == null) t.Loved1 = meals[rng.Next(meals.Count)];
            for (int guard = 0; guard < 20 && (t.Loved2 == null || t.Loved2 == t.Loved1); guard++) t.Loved2 = meals[rng.Next(meals.Count)];
            for (int guard = 0; guard < 20 && (t.Disliked == null || t.Loves(t.Disliked)); guard++) t.Disliked = meals[rng.Next(meals.Count)];
            if (t.Loves(t.Disliked)) t.Disliked = null;
            if (c.Stage != AgeStage.Child) _tastes[c.Id] = t;
            return t;
        }

        /// <summary>Ölmüş ebeveynin en çok pişirdiği yemek mi ("Anamın yemeği"). Dönüş: 1 anne, 2 baba, 0 değil.</summary>
        public int ParentDish(Character c, string dishId, Func<int, Character> find)
        {
            if (c == null || find == null) return 0;
            int k = 0;
            foreach (int? pid in new[] { c.BirthMotherId, c.BirthFatherId })
            {
                k++;
                if (!pid.HasValue) continue;
                var parent = find(pid.Value);
                if (parent == null || !parent.IsDead) continue;
                if (!_cooked.TryGetValue(pid.Value, out var d) || d.Count == 0) continue;
                var top = d.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.Ordinal).First();
                if (top.Key == dishId && top.Value >= 3) return k;
            }
            return 0;
        }

        // ---- defter (arayüz) ----

        public List<RecipePage> Pages(IEnumerable<Character> members, Func<int, Character> find = null)
        {
            var list = members.Where(m => !m.IsDead && !m.IsVillager).ToList();
            int bestLevel(SkillType s) => list.Count == 0 ? 0 : list.Max(m => m.Skills.GetLevel(s));
            var pages = new List<RecipePage>();
            foreach (var dish in _content.Dishes.All)
            {
                var page = new RecipePage { Dish = dish, Written = _written.Contains(dish.Id) };
                foreach (var m in list)
                {
                    if (m.Stage >= AgeStage.Teen && Knows(m, dish)) page.Knowers.Add(m);
                    var t = TastesOf(m, find);
                    if (t != null && t.Loves(dish.Id)) page.Lovers.Add(m);
                    page.TimesCooked += TimesCooked(m, dish.Id);
                }
                if (_best.TryGetValue(dish.Id, out var b)) { page.BestQuality = b.quality; page.BestCook = b.cook; }
                page.State = page.Knowers.Count > 0 ? RecipeState.Known
                    : page.Written || dish.Learn == LearnKind.Level || bestLevel(dish.Skill) + 3 >= dish.MinLevel ? RecipeState.Heard : RecipeState.Unknown;
                page.OnlyKnower = page.Knowers.Count == 1 && !page.Written && dish.Learn != LearnKind.Basic && dish.Learn != LearnKind.Level;
                pages.Add(page);
            }
            return pages;
        }

        // ---- kayıt: k:<kişi>:<yemek>,… | c:<kişi>:<yemek>=<n>,… | e:… | t:<kişi>:<a>|<b>|<c> | w:<yemek>,… | b:<yemek>=<kalite>=<ad> | f:<kişi>=<n> ----

        public string[] Export()
        {
            var l = new List<string>();
            string I(int v) => v.ToString(CultureInfo.InvariantCulture);
            foreach (var kv in _known) if (kv.Value.Count > 0) l.Add("k:" + I(kv.Key) + ":" + string.Join(",", kv.Value.OrderBy(x => x, StringComparer.Ordinal)));
            foreach (var (tag, map) in new[] { ("c", _cooked), ("e", _eatenAsChild) })
                foreach (var kv in map)
                    if (kv.Value.Count > 0) l.Add(tag + ":" + I(kv.Key) + ":" + string.Join(",", kv.Value.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Key + "=" + I(x.Value))));
            foreach (var kv in _tastes) l.Add("t:" + I(kv.Key) + ":" + (kv.Value.Loved1 ?? "") + "|" + (kv.Value.Loved2 ?? "") + "|" + (kv.Value.Disliked ?? ""));
            if (_written.Count > 0) l.Add("w:" + string.Join(",", _written.OrderBy(x => x, StringComparer.Ordinal)));
            foreach (var kv in _best) l.Add("b:" + kv.Key + "=" + I(kv.Value.quality) + "=" + (kv.Value.cook ?? ""));
            foreach (var kv in _familyMeals) l.Add("f:" + I(kv.Key) + "=" + I(kv.Value));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _known.Clear(); _cooked.Clear(); _eatenAsChild.Clear(); _tastes.Clear(); _written.Clear(); _best.Clear(); _familyMeals.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 3 || line[1] != ':') continue;
                string body = line.Substring(2);
                switch (line[0])
                {
                    case 'w':
                        foreach (var id in body.Split(',')) if (id.Length > 0) _written.Add(id);
                        break;
                    case 'b':
                    {
                        var p = body.Split(new[] { '=' }, 3);
                        if (p.Length == 3 && int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int q)) _best[p[0]] = (q, p[2]);
                        break;
                    }
                    case 'f':
                    {
                        var p = body.Split('=');
                        if (p.Length == 2 && int.TryParse(p[0], out int id) && int.TryParse(p[1], out int n)) _familyMeals[id] = n;
                        break;
                    }
                    default:
                    {
                        int cut = body.IndexOf(':');
                        if (cut <= 0 || !int.TryParse(body.Substring(0, cut), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id)) break;
                        string rest = body.Substring(cut + 1);
                        if (line[0] == 'k') foreach (var d in rest.Split(',')) { if (d.Length > 0) KnownSet(id).Add(d); }
                        else if (line[0] == 't')
                        {
                            var p = rest.Split('|');
                            if (p.Length == 3) _tastes[id] = new Tastes { Loved1 = Nz(p[0]), Loved2 = Nz(p[1]), Disliked = Nz(p[2]) };
                        }
                        else if (line[0] == 'c' || line[0] == 'e')
                        {
                            var map = line[0] == 'c' ? _cooked : _eatenAsChild;
                            if (!map.TryGetValue(id, out var d)) map[id] = d = new Dictionary<string, int>(StringComparer.Ordinal);
                            foreach (var e in rest.Split(','))
                            {
                                var p = e.Split('=');
                                if (p.Length == 2 && int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) d[p[0]] = n;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private static string Nz(string s) => string.IsNullOrEmpty(s) ? null : s;
    }
}
