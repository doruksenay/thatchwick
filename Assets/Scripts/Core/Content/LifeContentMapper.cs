using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Content
{
    /// <summary>life.json (özellik, dal, proje, köylü, şenlik, hedef, alet, şablon) bölümlerini kayıt defterlerine işler.</summary>
    public static class LifeContentMapper
    {
        public static bool TryParseSeason(string text, out Season season)
        {
            season = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!Enum.TryParse(text, true, out season)) return false;
            return Enum.IsDefined(typeof(Season), season);
        }

        /// <summary>Ad listesi: boş ya da yinelenen ad içerik hatasıdır (belge 61b §5).</summary>
        private static List<string> CleanNames(string[] names, string where, List<string> errors)
        {
            var list = new List<string>();
            if (names == null) return list;
            foreach (var n in names)
            {
                var t = n?.Trim();
                if (string.IsNullOrEmpty(t)) { errors.Add($"{where}: boş ad."); continue; }
                if (list.Contains(t)) { errors.Add($"{where}: '{t}' iki kez yazılmış."); continue; }
                list.Add(t);
            }
            return list;
        }

        private static float Mult(float v) => v <= 0f ? 1f : v;

        /// <summary>Yaşayan köy (belge 66f): ayarlar ve meslek iş döngüleri. 0 ya da eksik alan = varsayılan.</summary>
        private static void MapVillageLife(ContentDatabase db, ContentFileDto file, List<string> errors)
        {
            var cfg = db.VillageLife;
            var d = file.villageLife;
            if (d != null && !d.IsEmpty)
            {
                if (d.giftChance > 0) cfg.GiftChance = Math.Min(1.0, d.giftChance);
                if (d.discount > 0) cfg.Discount = Math.Min(0.5, d.discount);
                if (d.breakdownMinDays > 0) cfg.BreakdownMinDays = d.breakdownMinDays;
                if (d.breakdownMaxDays > 0) cfg.BreakdownMaxDays = Math.Max(cfg.BreakdownMinDays, d.breakdownMaxDays);
                if (d.consoleShare > 0) cfg.ConsoleShare = Math.Min(1.0, d.consoleShare);
                if (d.sharedLossDays > 0) cfg.SharedLossDays = d.sharedLossDays;
                if (d.criticalNeed > 0) cfg.CriticalNeed = d.criticalNeed;
                if (d.eveningTavernShare > 0) cfg.EveningTavernShare = Math.Min(1.0, d.eveningTavernShare);
                if (d.stockFood > 0) cfg.StockFood = d.stockFood;
                if (d.stockCooked > 0) cfg.StockCooked = d.stockCooked;
                if (d.stockRaw > 0) cfg.StockRaw = d.stockRaw;
                if (d.stockSeed > 0) cfg.StockSeed = d.stockSeed;
                if (d.stockOther > 0) cfg.StockOther = d.stockOther;
                if (d.stockStaple > 0) cfg.StockStaple = d.stockStaple;
                cfg.Pantry.Clear(); cfg.Produce.Clear(); cfg.Meals.Clear(); cfg.Treats.Clear();
                if (d.treats != null)
                    foreach (var t in d.treats)
                        if (t != null && !string.IsNullOrWhiteSpace(t.role) && !string.IsNullOrWhiteSpace(t.item)) cfg.Treats[t.role] = t.item;
                if (d.pantry != null)
                    foreach (var p in d.pantry)
                        if (p != null && !string.IsNullOrWhiteSpace(p.item))
                            cfg.Pantry.Add(new PantryStaple { Item = p.item, Target = Math.Max(1, p.target), Min = Math.Max(0, Math.Min(p.min, p.target)) });
                if (d.produce != null)
                    foreach (var p in d.produce)
                        if (p != null && !string.IsNullOrWhiteSpace(p.item) && !string.IsNullOrWhiteSpace(p.role))
                            cfg.Produce.Add(new PantryProduce { Role = p.role, Item = p.item, Count = Math.Max(1, p.count) });
                if (d.meals != null)
                    foreach (var m in d.meals)
                    {
                        if (m == null || string.IsNullOrWhiteSpace(m.id)) continue;
                        if (m.where != "home" && m.where != "work") { errors.Add($"villageLife.meals/{m.id}: yer 'home' ya da 'work' olmalı."); continue; }
                        var meal = new VillageMeal { Id = m.id, Where = m.where, Chain = m.chain, Hunger = m.hunger > 0f ? m.hunger : 60f };
                        if (m.consume != null)
                            foreach (var c in m.consume)
                                if (c != null && !string.IsNullOrWhiteSpace(c.item)) meal.Consume.Add(new KeyValuePair<string, int>(c.item, c.count <= 0 ? 1 : c.count));
                        cfg.Meals.Add(meal);
                    }
            }
            if (file.villagerWork == null) return;
            foreach (var w in file.villagerWork)
            {
                if (w == null || string.IsNullOrWhiteSpace(w.role)) continue;
                var list = new List<VillagerWorkStep>();
                if (w.steps != null)
                    foreach (var st in w.steps)
                    {
                        if (st == null || string.IsNullOrWhiteSpace(st.id) || string.IsNullOrWhiteSpace(st.pose)) { errors.Add($"villagerWork/{w.role}: adımın kimliği ya da pozu boş."); continue; }
                        list.Add(new VillagerWorkStep { Id = st.id, Pose = st.pose, Fallback = string.IsNullOrWhiteSpace(st.fallback) ? null : st.fallback, Minutes = st.minutes > 0 ? st.minutes : 20 });
                    }
                if (list.Count == 0) { errors.Add($"villagerWork/{w.role}: iş listesi boş."); continue; }
                cfg.Work[w.role] = list;
            }
        }

        /// <summary>DTO'daki etkileri çarpan nesnesine çevirir (0 = değişiklik yok).</summary>
        public static LifeModifiers ToModifiers(ModifierDto d, List<string> errors, string where)
        {
            var m = new LifeModifiers();
            if (d == null) return m;
            m.HungerDecay = Mult(d.hungerDecay);
            m.EnergyDecay = Mult(d.energyDecay);
            m.SocialDecay = Mult(d.socialDecay);
            m.FunDecay = Mult(d.funDecay);
            m.ColdResist = Math.Max(0f, Math.Min(0.8f, d.coldResist));
            m.WorkSpeed = Mult(d.workSpeed);
            m.SocialGain = Mult(d.socialGain);
            m.Fertility = Mult(d.fertility);
            m.YieldChance = Math.Max(0f, Math.Min(1f, d.yieldChance));
            m.BuildDiscount = Math.Max(0f, Math.Min(0.5f, d.buildDiscount));
            m.SellBonus = Math.Max(0f, Math.Min(0.5f, d.sellBonus));
            m.SpoilReduction = Math.Max(0f, Math.Min(0.8f, d.spoilReduction));
            m.GrowthSpeed = Mult(d.growthSpeed);
            m.MoodBase = d.moodBase;
            m.LifespanDays = d.lifespanDays;
            m.QualityLevels = d.qualityLevels;
            if (d.skillRates != null)
                foreach (var r in d.skillRates)
                {
                    if (r == null) continue;
                    if (!Enum.TryParse(r.skill, true, out SkillType skill) || !Enum.IsDefined(typeof(SkillType), skill))
                    {
                        errors.Add($"{where}: bilinmeyen beceri '{r.skill}'.");
                        continue;
                    }
                    m.SetSkillRate(skill, Mult(r.rate));
                }
            return m;
        }

        public static void Apply(ContentFileDto file, ContentDatabase db, List<string> errors)
        {
            if (file.skillUnlocks != null)
                foreach (var u in file.skillUnlocks)
                {
                    if (u == null) continue;
                    if (!Enum.TryParse(u.skill, true, out SkillType us) || !Enum.IsDefined(typeof(SkillType), us))
                    {
                        errors.Add($"skillUnlocks: bilinmeyen beceri '{u.skill}'.");
                        continue;
                    }
                    if (u.level < 1 || u.level > SkillRules.MaxLevel) { errors.Add($"skillUnlocks {u.skill}: seviye 1-10 olmalı ({u.level})."); continue; }
                    if (string.IsNullOrWhiteSpace(u.text)) { errors.Add($"skillUnlocks {u.skill} {u.level}: metin boş."); continue; }
                    db.SkillUnlocks.Add(new SkillUnlockDef
                    {
                        Skill = us, Level = u.level, Kind = string.IsNullOrWhiteSpace(u.kind) ? "feature" : u.kind.Trim(),
                        Text = u.text.Trim(), Planned = u.planned, AdultPack = u.adultPack
                    });
                }

            if (file.traits != null)
                foreach (var t in file.traits)
                {
                    if (t == null) continue;
                    Register(() => db.Traits.Register(new TraitDef
                    {
                        Id = t.id, DisplayName = t.displayName, Description = t.description,
                        Conflicts = t.conflicts != null ? new List<string>(t.conflicts) : new List<string>(),
                        Effects = ToModifiers(t.effects, errors, t.id)
                    }), errors);
                }

            if (file.branches != null)
                foreach (var b in file.branches)
                {
                    if (b == null) continue;
                    if (!Enum.TryParse(b.skill, true, out SkillType skill) || !Enum.IsDefined(typeof(SkillType), skill))
                    {
                        errors.Add($"{b.id}: bilinmeyen beceri '{b.skill}'.");
                        continue;
                    }
                    Register(() => db.Branches.Register(new BranchDef
                    {
                        Id = b.id, DisplayName = b.displayName, Description = b.description, Skill = skill,
                        Level = b.level, Requires = string.IsNullOrWhiteSpace(b.requires) ? null : b.requires,
                        Effects = ToModifiers(b.effects, errors, b.id)
                    }), errors);
                }

            if (file.projects != null)
                foreach (var p in file.projects)
                {
                    if (p == null) continue;
                    var def = new ProjectDef { Id = p.id, DisplayName = p.displayName, Description = p.description, Akce = p.akce };
                    if (p.costs != null) foreach (var c in p.costs) if (c != null) def.Costs.Add(new CostDef { ItemId = c.item, Count = c.count <= 0 ? 1 : c.count });
                    if (p.effects != null)
                        def.Effects = new VillageEffects
                        {
                            CropGrowth = p.effects.cropGrowth, MarketSell = p.effects.marketSell, SpoilReduction = p.effects.spoilReduction,
                            ColdResist = p.effects.coldResist, FestivalMood = p.effects.festivalMood, Status = p.effects.status
                        };
                    Register(() => db.Projects.Register(def), errors);
                }

            if (file.villagers != null)
                foreach (var v in file.villagers)
                {
                    if (v == null) continue;
                    var sex = Sex.Female;
                    if (!string.IsNullOrWhiteSpace(v.sex) && !(Enum.TryParse(v.sex, true, out sex) && Enum.IsDefined(typeof(Sex), sex)))
                    {
                        errors.Add($"{v.id}: bilinmeyen cinsiyet '{v.sex}'.");
                        continue;
                    }
                    var def = new VillagerDef
                    {
                        Id = v.id, Name = v.name, Sex = sex, Age = v.age, Role = v.role, RoleName = v.roleName, Home = v.home, Work = v.work,
                        Traits = v.traits != null ? new List<string>(v.traits) : new List<string>(),
                        Loves = v.loves != null ? new List<string>(v.loves) : new List<string>(),
                        Likes = v.likes != null ? new List<string>(v.likes) : new List<string>(),
                        Dislikes = v.dislikes != null ? new List<string>(v.dislikes) : new List<string>(),
                        MaleNames = CleanNames(v.maleNames, v.id + ".maleNames", errors),
                        FemaleNames = CleanNames(v.femaleNames, v.id + ".femaleNames", errors)
                    };
                    if (v.events != null)
                        foreach (var e in v.events)
                            if (e != null) def.Events.Add(new VillagerEventDef
                            {
                                Id = e.id, MinAffection = e.minAffection, Text = e.text, RewardAkce = e.rewardAkce,
                                RewardItem = string.IsNullOrWhiteSpace(e.rewardItem) ? null : e.rewardItem, RewardCount = e.rewardCount <= 0 ? 1 : e.rewardCount
                            });
                    Register(() => db.Villagers.Register(def), errors);
                }

            if (file.namePool != null)
            {
                db.MaleNamePool.Clear(); db.MaleNamePool.AddRange(CleanNames(file.namePool.maleNames, "namePool.maleNames", errors));
                db.FemaleNamePool.Clear(); db.FemaleNamePool.AddRange(CleanNames(file.namePool.femaleNames, "namePool.femaleNames", errors));
            }

            MapVillageLife(db, file, errors);

            if (file.festivals != null)
                foreach (var f in file.festivals)
                {
                    if (f == null) continue;
                    if (!TryParseSeason(f.season, out var season)) { errors.Add($"{f.id}: bilinmeyen mevsim '{f.season}'."); continue; }
                    Register(() => db.Festivals.Register(new FestivalDef
                    {
                        Id = f.id, DisplayName = f.displayName, Description = f.description, Season = season, Day = f.day,
                        FunRestore = f.funRestore, SocialRestore = f.socialRestore, MoodValue = f.moodValue, ExtraOffers = f.extraOffers
                    }), errors);
                }

            if (file.goals != null)
                foreach (var g in file.goals)
                {
                    if (g == null) continue;
                    Register(() => db.Goals.Register(new GoalDef
                    {
                        Id = g.id, DisplayName = g.displayName, Description = g.description, Metric = g.metric,
                        Target = g.target, RewardAkce = g.rewardAkce, RewardXp = g.rewardXp
                    }), errors);
                }

            if (file.tools != null)
                foreach (var t in file.tools)
                {
                    if (t == null) continue;
                    var def = new ToolDef { Id = t.id, DisplayName = t.displayName };
                    if (t.tiers != null)
                        foreach (var tier in t.tiers)
                            if (tier != null) def.Tiers.Add(new ToolTierDef { Name = tier.name, Akce = tier.akce, Firewood = tier.firewood, Speed = Mult(tier.speed), Level = Math.Max(1, tier.level) });
                    Register(() => db.Tools.Register(def), errors);
                }

            if (file.templates != null)
                foreach (var t in file.templates)
                {
                    if (t == null) continue;
                    var side = TemplateSide.South;
                    if (!string.IsNullOrWhiteSpace(t.doorSide) && !(Enum.TryParse(t.doorSide, true, out side) && Enum.IsDefined(typeof(TemplateSide), side)))
                    {
                        errors.Add($"{t.id}: bilinmeyen kapı yönü '{t.doorSide}'.");
                        continue;
                    }
                    var def = new RoomTemplateDef
                    {
                        Id = t.id, DisplayName = t.displayName, Width = t.width, Height = t.height, DoorSide = side, DoorOffset = t.doorOffset,
                        Windows = t.windows != null ? new List<int>(t.windows) : new List<int>(),
                        Floor = string.IsNullOrWhiteSpace(t.floor) ? null : t.floor
                    };
                    if (t.items != null) foreach (var i in t.items) if (i != null) def.Items.Add(new TemplateItemDef { ItemId = i.item, X = i.x, Y = i.y, Rotation = i.rotation });
                    Register(() => db.Templates.Register(def), errors);
                }
        }

        private static void Register(Action action, List<string> errors)
        {
            try { action(); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
        }
    }
}
