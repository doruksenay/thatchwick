using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Cooking
{
    /// <summary>
    /// Yemek içeriğini bağlar (belge 58): malzeme grupları, ek baharatlar, konuşma satırları, yemek kaydı ve
    /// yiyeceğin kendi yeme verisi. <see cref="ContentMapper.Apply"/> tarifler ve etkilerden sonra çağırır.
    /// </summary>
    public static class CookingContentMapper
    {
        /// <summary>Hazır yiyeceğin alınıp yendiği yerler: kiler, yemek dolabı, mahzen (kazan olmadan da yenir).</summary>
        public const string FoodPointTag = "foodpoint";
        public const string PotId = "item.cooking_pot";

        public static void Apply(ContentFileDto file, ContentDatabase db, List<string> errors)
        {
            ApplyGroups(file.groups, db, errors);
            ApplySpices(file.spices, db, errors);
            ApplyTalk(file.talk, db, errors);
            ValidateByproducts(db, errors);
            BuildFoods(db);
            BuildDishes(db, errors);
            SpreadToFoodPoints(db);
        }

        private static void ApplyGroups(GroupDto[] groups, ContentDatabase db, List<string> errors)
        {
            if (groups == null) return;
            foreach (var g in groups)
            {
                if (g == null) continue;
                if (!IngredientGroupDef.IsGroup(g.id)) { errors.Add($"grup '{g.id}': kimlik 'group.' ile başlamalı."); continue; }
                var def = new IngredientGroupDef { Id = g.id.Trim(), DisplayName = string.IsNullOrWhiteSpace(g.displayName) ? g.id : g.displayName };
                if (g.members != null)
                    foreach (var m in g.members)
                    {
                        if (m == null || string.IsNullOrWhiteSpace(m.item)) continue;
                        if (!db.Items.TryGet(m.item, out var item)) { errors.Add($"{g.id}: üye '{m.item}' bulunamadı."); continue; }
                        if (item.Placement != ItemPlacement.Resource) { errors.Add($"{g.id}: üye '{m.item}' bir kaynak olmalı."); continue; }
                        if (m.bonus < -3 || m.bonus > 3) { errors.Add($"{g.id}: '{m.item}' payı −3..+3 arasında olmalı."); continue; }
                        if (def.Contains(m.item)) { errors.Add($"{g.id}: üye '{m.item}' tekrar ediyor."); continue; }
                        def.Members.Add(new GroupMember { ItemId = m.item, Bonus = m.bonus, Kind = string.IsNullOrWhiteSpace(m.kind) ? null : m.kind.Trim() });
                    }
                if (def.Members.Count == 0) { errors.Add($"{g.id}: grubun üyesi yok."); continue; }
                try { db.Groups.Register(def); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
            }
        }

        private static void ApplySpices(SpiceDto[] spices, ContentDatabase db, List<string> errors)
        {
            if (spices == null) return;
            foreach (var s in spices)
            {
                if (s == null) continue;
                if (!db.Items.Contains(s.item)) { errors.Add($"baharat '{s.item}': eşya bulunamadı."); continue; }
                if (string.IsNullOrWhiteSpace(s.effect) || db.Moods == null || !db.Moods.Contains(s.effect)) { errors.Add($"baharat {s.item}: etki '{s.effect}' bulunamadı."); continue; }
                var def = new SpiceDef { Id = s.item, Effect = s.effect, Processed = s.processed, Smell = s.smell };
                bool bad = false;
                foreach (var (list, target) in new[] { (s.fits, def.Fits), (s.clashes, def.Clashes) })
                    if (list != null)
                        foreach (var c in list)
                        {
                            if (DishClasses.TryParse(c, out var dc)) target.Add(dc);
                            else { errors.Add($"baharat {s.item}: bilinmeyen yemek sınıfı '{c}'."); bad = true; }
                        }
                if (bad) continue;
                try { db.Spices.Register(def); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
            }
        }

        private static void ApplyTalk(TalkDto[] talk, ContentDatabase db, List<string> errors)
        {
            if (talk == null) return;
            foreach (var t in talk)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.id)) { errors.Add("Konuşma satırının kimliği boş."); continue; }
                var def = new TalkLineDef { Id = t.id.Trim(), Lines = t.lines <= 0 ? 2 : Math.Min(3, t.lines), AdultPack = t.adultPack, Rare = t.rare };
                if (t.tags != null) foreach (var tag in t.tags) if (!string.IsNullOrWhiteSpace(tag)) def.Tags.Add(tag.Trim());
                if (def.Tags.Count == 0) { errors.Add($"konuşma {t.id}: en az bir etiket gerekli."); continue; }
                try { db.Talk.Register(def); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
            }
        }

        private static void ValidateByproducts(ContentDatabase db, List<string> errors)
        {
            foreach (var a in db.Animals.All)
                foreach (var b in a.Byproducts)
                {
                    if (!db.Items.Contains(b.ItemId)) errors.Add($"{a.Id}: yan ürün '{b.ItemId}' bulunamadı.");
                    if (b.Chance < 0f || b.Chance > 1f) errors.Add($"{a.Id}: yan ürün '{b.ItemId}' şansı 0-1 arasında olmalı.");
                }
        }

        /// <summary>Çiğ yeme ya da içme: tek eşya harcar, ürün vermez, beceri istemez.</summary>
        public static bool IsPlainEat(InteractionDef i) =>
            i != null && i.RestoresNeed && i.ConsumeItem != null && i.Consume.Count == 0 && i.Produce.Count == 0
            && !i.Skill.HasValue && !i.RequiresLit && i.MealAction == null && !i.IsCraft;

        /// <summary>
        /// Her yiyeceğin yeme verisi: o eşyayı tek başına harcayan yeme etkileşimi (önce çiğ yeme, yoksa pişirip yeme).
        /// Sofradaki porsiyon ve kilerden yeme bu veriyi kullanır; "ye" artık kazana bağlı değildir.
        /// </summary>
        private static void BuildFoods(ContentDatabase db)
        {
            db.Foods.Clear();
            foreach (var pass in new[] { true, false })
                foreach (var item in db.Items.All)
                    foreach (var i in item.Interactions)
                    {
                        if (i.ConsumeItem == null || i.Consume.Count > 0 || i.Produce.Count > 0 || !i.RestoresNeed || i.IsCraft) continue;
                        if (IngredientGroupDef.IsGroup(i.ConsumeItem)) continue;
                        if (pass != IsPlainEat(i)) continue;
                        if (!db.Foods.ContainsKey(i.ConsumeItem)) db.Foods[i.ConsumeItem] = i;
                    }
        }

        private static void BuildDishes(ContentDatabase db, List<string> errors)
        {
            db.DishByProduct.Clear();
            foreach (var item in db.Items.All)
                foreach (var i in item.Interactions)
                {
                    i.Dish = null;
                    if (i.DishRaw == null) continue;
                    string where = $"{item.Id}/{i.Id}";
                    if (!DishClasses.TryParse(i.DishRaw, out var cls) || cls == DishClass.None) { errors.Add($"{where}: bilinmeyen yemek sınıfı '{i.DishRaw}'."); continue; }
                    if (i.Produce.Count == 0) { errors.Add($"{where}: yemek etkileşimi ürün vermeli."); continue; }
                    if (!i.Skill.HasValue) { errors.Add($"{where}: yemek etkileşimi beceri istemeli."); continue; }
                    string product = i.Produce[0].ItemId;
                    if (!db.Dishes.TryGet(product, out var dish))
                    {
                        var (vessel, look) = DishClasses.DefaultServe(cls);
                        dish = new DishDef { Id = product, Class = cls, Vessel = vessel, Look = look, Skill = i.Skill.Value, MinLevel = i.MinSkillLevel };
                        if (!ParseLearn(i.LearnRaw, dish, db, errors, where)) continue;
                        if (i.ServeRaw != null)
                        {
                            var p = i.ServeRaw.Split(':');
                            if (p.Length != 2 || p[0].Length == 0 || p[1].Length == 0) { errors.Add($"{where}: servis 'kap:görünüm' biçiminde olmalı."); continue; }
                            dish.Vessel = p[0]; dish.Look = p[1];
                        }
                        db.Dishes.Register(dish);
                    }
                    else if (dish.Class != cls) { errors.Add($"{where}: '{product}' başka tarifte {dish.Class} sınıfında."); continue; }
                    dish.MinLevel = Math.Min(dish.MinLevel, i.MinSkillLevel);
                    dish.Sources.Add((item, i));
                    i.Dish = dish;
                }
            // Aynı ürünü veren ama sınıfı yazılmamış tarifler de o yemeğin kaynağıdır (güvecin üç seviyesi gibi).
            foreach (var item in db.Items.All)
                foreach (var i in item.Interactions)
                    if (i.Dish == null && i.Produce.Count > 0 && i.Skill.HasValue && db.Dishes.TryGet(i.Produce[0].ItemId, out var d)
                        && (i.Skill == Characters.SkillType.Cooking || i.Skill == Characters.SkillType.Baking))
                    { i.Dish = d; d.Sources.Add((item, i)); d.MinLevel = Math.Min(d.MinLevel, i.MinSkillLevel); }
            foreach (var d in db.Dishes.All)
            {
                db.DishByProduct[d.Id] = d;
                foreach (var (_, i) in d.Sources)
                    foreach (var p in i.Produce)
                        if (!db.DishByProduct.ContainsKey(p.ItemId) && db.Items.TryGet(p.ItemId, out var pd) && pd.HasTag("food")) db.DishByProduct[p.ItemId] = d;
            }
        }

        private static bool ParseLearn(string raw, DishDef dish, ContentDatabase db, List<string> errors, string where)
        {
            if (raw == null) { dish.Learn = LearnKind.Level; return true; }
            var p = raw.Split(':');
            switch (p[0])
            {
                case "level": dish.Learn = LearnKind.Level; return true;
                case "basic": dish.Learn = LearnKind.Basic; return true;
                case "treatise": dish.Learn = LearnKind.Treatise; return true;
                case "request": dish.Learn = LearnKind.Request; return true;
                case "family": dish.Learn = LearnKind.Family; return true;
                case "villager":
                    if (p.Length != 3 || !int.TryParse(p[2], out int at) || at < 1 || at > 100) { errors.Add($"{where}: 'villager:<kimlik>:<yakınlık>' bekleniyor."); return false; }
                    dish.Learn = LearnKind.Villager; dish.LearnFrom = p[1]; dish.LearnAt = at;
                    return true;
                default:
                    errors.Add($"{where}: bilinmeyen öğrenme yolu '{raw}'.");
                    return false;
            }
        }

        /// <summary>Çiğ yeme ve içme etkileşimlerini kiler ve yemek dolabı gibi eşyalara da bağlar: kazanı olmayan hane de yer.</summary>
        private static void SpreadToFoodPoints(ContentDatabase db)
        {
            var plain = new List<InteractionDef>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in db.Foods)
                if (IsPlainEat(kv.Value) && seen.Add(kv.Value.Id)) plain.Add(kv.Value);
            if (plain.Count == 0) return;
            foreach (var item in db.Items.All)
            {
                if (!item.HasTag(FoodPointTag)) continue;
                var have = new HashSet<string>(StringComparer.Ordinal);
                foreach (var i in item.Interactions) have.Add(i.Id);
                foreach (var i in plain) if (have.Add(i.Id)) item.Interactions.Add(i);
            }
        }
    }
}
