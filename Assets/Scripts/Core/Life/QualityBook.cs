using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Bir ustalık eserinin defter kaydı (53 §4.6): yazı, yapan, yıl; kuşaklar geçince yadigâr.</summary>
    public sealed class MasterworkEntry
    {
        public int Serial;
        public string ItemId;
        public int MakerId;
        public string MakerName;
        /// <summary>Yapanın kuşağı: hanede iki kuşak sonrası yaşayınca eser yadigâr olur (3 kuşak elden ele).</summary>
        public int MakerGeneration;
        public double Day;
        public int Year;
        public bool Heirloom;
        /// <summary>Yemek eseri: aile tarifiyle yapıldıysa tarifin adı (yadigâr sofra).</summary>
        public string Recipe;
    }

    /// <summary>
    /// Kalite defteri (Project belgesi 53): tezgâhta "En iyi malzemeyi kullan" ve "Ustalık eseri dene" seçimleri (tarif başına
    /// hatırlanır), ustalık eserlerinin kaydı ve kuşak geçince yadigâra dönmeleri.
    /// </summary>
    public sealed class QualityBook
    {
        private readonly HashSet<string> _bestIngredients = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _masterworkNext = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<int, long> _lastAttempt = new Dictionary<int, long>();
        private readonly List<MasterworkEntry> _works = new List<MasterworkEntry>();
        private int _nextSerial = 1;

        public static string RecipeKey(ItemDef station, InteractionDef i) => (station?.Id ?? "") + "|" + (i?.Id ?? "");

        // ---- tezgâh seçimleri ----

        public bool BestIngredients(string recipeKey) => recipeKey != null && _bestIngredients.Contains(recipeKey);
        public void SetBestIngredients(string recipeKey, bool on) { if (on) _bestIngredients.Add(recipeKey); else _bestIngredients.Remove(recipeKey); }

        public bool MasterworkQueued(string recipeKey) => recipeKey != null && _masterworkNext.Contains(recipeKey);
        public void QueueMasterwork(string recipeKey, bool on) { if (on) _masterworkNext.Add(recipeKey); else _masterworkNext.Remove(recipeKey); }

        /// <summary>
        /// Ustalık eseri denemesinin engeli (boş = denenebilir): usta Sv 10 (karar 1), günde bir kez. Girdilerin Has olma şartı
        /// tarif anında (malzeme seçilince) denetlenir.
        /// </summary>
        public LocText AttemptBlocker(Character c, InteractionDef i, long day)
        {
            if (c == null || i == null || !i.Skill.HasValue) return LocText.Of("quality.err.no_skill");
            if (c.Skills.GetLevel(i.Skill.Value) < 10) return LocText.Of("quality.err.master", ("skill", LocText.Of("skill." + i.Skill.Value)));
            if (_lastAttempt.TryGetValue(c.Id, out long last) && last == day) return LocText.Of("quality.err.once_a_day", ("name", c.Name));
            return default;
        }

        /// <summary>Bu iş ustalık eseri denemesi olacak mı (tezgâhta işaretli ve usta uygun). İş süresi iki katına çıkar.</summary>
        public bool WillAttempt(Character c, ItemInstance station, InteractionDef i, long day) =>
            station != null && MasterworkQueued(RecipeKey(station.Def, i)) && AttemptBlocker(c, i, day).IsEmpty;

        /// <summary>Denemeyi işler (günlük hakkı kullanır; tezgâhtaki bütün tariflerin işaretini kaldırır).</summary>
        public void MarkAttempted(Character c, ItemInstance station, InteractionDef i, long day)
        {
            if (c != null) _lastAttempt[c.Id] = day;
            if (station != null) _masterworkNext.RemoveWhere(k => k.StartsWith(station.Def.Id + "|", StringComparison.Ordinal));
        }

        // ---- tezgâh düzeyinde seçim (arayüz): tezgâhın kaliteli ürün veren bütün tarifleri ----

        /// <summary>Tezgâhın kaliteyle ilgili tarifleri: beceri ister ve kaliteli ürün verir ya da kaliteli girdi harcar.</summary>
        public static IEnumerable<InteractionDef> QualityRecipes(ItemDef station, ContentDatabase content)
        {
            if (station == null) yield break;
            foreach (var i in station.Interactions)
            {
                if (!i.Skill.HasValue || i.Produce.Count == 0) continue;
                bool graded = false;
                foreach (var p in i.Produce) if (content.Items.TryGet(p.ItemId, out var d) && d.HasQuality) graded = true;
                if (graded) yield return i;
            }
        }

        public bool BestIngredientsAt(ItemDef station, ContentDatabase content)
        {
            foreach (var i in QualityRecipes(station, content)) if (BestIngredients(RecipeKey(station, i))) return true;
            return false;
        }

        public void SetBestIngredientsAt(ItemDef station, ContentDatabase content, bool on)
        {
            foreach (var i in QualityRecipes(station, content)) SetBestIngredients(RecipeKey(station, i), on);
        }

        public bool MasterworkQueuedAt(ItemDef station, ContentDatabase content)
        {
            foreach (var i in QualityRecipes(station, content)) if (MasterworkQueued(RecipeKey(station, i))) return true;
            return false;
        }

        public void QueueMasterworkAt(ItemDef station, ContentDatabase content, bool on)
        {
            foreach (var i in QualityRecipes(station, content)) QueueMasterwork(RecipeKey(station, i), on);
        }

        // ---- eser defteri ----

        public IReadOnlyList<MasterworkEntry> Works => _works;

        public MasterworkEntry Find(int serial)
        {
            foreach (var w in _works) if (w.Serial == serial) return w;
            return null;
        }

        public MasterworkEntry Register(string itemId, Character maker, double day, int year, string recipe = null, bool heirloom = false)
        {
            var e = new MasterworkEntry
            {
                Serial = _nextSerial++, ItemId = itemId, MakerId = maker?.Id ?? 0, MakerName = maker?.Name ?? "",
                MakerGeneration = maker?.Generation ?? 1, Day = day, Year = year, Recipe = recipe, Heirloom = heirloom
            };
            _works.Add(e);
            return e;
        }

        /// <summary>Eser yazısı: "Ali'nin işi, 3. yıl" (ui anahtarı, adlı yer tutucular).</summary>
        public LocText Inscription(int serial)
        {
            var e = Find(serial);
            if (e == null) return default;
            return e.Recipe != null
                ? LocText.Of("quality.inscription.recipe", ("maker", e.MakerName), ("item", e.ItemId), ("year", e.Year), ("recipe", e.Recipe))
                : LocText.Of("quality.inscription", ("maker", e.MakerName), ("item", e.ItemId), ("year", e.Year));
        }

        /// <summary>
        /// Kuşak denetimi (karar 1: ailede 3 kuşak süren şey yadigâr olur): yapanın kuşağından iki kuşak sonrası hanede yaşıyorsa
        /// eser yadigâra döner. Envanterdeki ve yerleşik örnekler yükselir. Yeni yadigâr olanların kayıtları döner.
        /// </summary>
        public List<MasterworkEntry> CheckHeirlooms(int youngestHouseholdGeneration, Household house, WorldState world)
        {
            var changed = new List<MasterworkEntry>();
            foreach (var e in _works)
            {
                if (e.Heirloom || youngestHouseholdGeneration - e.MakerGeneration < ItemQuality.HeirloomGenerations - 1) continue;
                e.Heirloom = true;
                changed.Add(e);
                if (house != null)
                    foreach (var kv in house.Stacks(e.ItemId))
                        if (kv.Key.Maker == e.Serial && kv.Key.Quality < ItemQuality.Heirloom)
                            for (int n = 0; n < kv.Value; n++)
                                if (house.TryTakeKey(kv.Key)) house.Add(kv.Key.WithQuality(ItemQuality.Heirloom), 1);
                if (world != null)
                    foreach (var it in world.Items)
                        if (it.Maker == e.Serial && it.Quality < ItemQuality.Heirloom) it.Quality = ItemQuality.Heirloom;
            }
            return changed;
        }

        // ---- kayıt ----

        public QualityBookSave Export() => new QualityBookSave
        {
            best = new List<string>(_bestIngredients).ToArray(),
            next = new List<string>(_masterworkNext).ToArray(),
            nextSerial = _nextSerial,
            works = _works.ConvertAll(w => new MasterworkSave
            {
                serial = w.Serial, item = w.ItemId, makerId = w.MakerId, makerName = w.MakerName, makerGen = w.MakerGeneration,
                day = w.Day, year = w.Year, heirloom = w.Heirloom, recipe = w.Recipe
            }).ToArray()
        };

        public void Import(QualityBookSave s)
        {
            _bestIngredients.Clear(); _masterworkNext.Clear(); _lastAttempt.Clear(); _works.Clear(); _nextSerial = 1;
            if (s == null) return;
            if (s.best != null) foreach (var k in s.best) if (k != null) _bestIngredients.Add(k);
            if (s.next != null) foreach (var k in s.next) if (k != null) _masterworkNext.Add(k);
            if (s.works != null)
                foreach (var w in s.works)
                    if (w != null)
                        _works.Add(new MasterworkEntry
                        {
                            Serial = w.serial, ItemId = w.item, MakerId = w.makerId, MakerName = w.makerName ?? "", MakerGeneration = w.makerGen,
                            Day = w.day, Year = w.year, Heirloom = w.heirloom, Recipe = string.IsNullOrEmpty(w.recipe) ? null : w.recipe
                        });
            _nextSerial = Math.Max(1, s.nextSerial);
            foreach (var w in _works) _nextSerial = Math.Max(_nextSerial, w.Serial + 1);
        }
    }

    [Serializable]
    public sealed class QualityBookSave
    {
        public string[] best, next;
        public int nextSerial;
        public MasterworkSave[] works;
        public AgingBatchSave[] aging;
    }

    [Serializable]
    public sealed class MasterworkSave
    {
        public int serial, makerId, makerGen, year;
        public string item, makerName, recipe;
        public double day;
        public bool heirloom;
    }
}
