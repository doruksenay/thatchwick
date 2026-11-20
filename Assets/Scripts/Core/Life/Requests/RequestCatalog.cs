using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;

namespace Thatchwick.Core.Life.Requests
{
    /// <summary>Rica şablonu: köylünün mesleğine göre (rol "*" herkes), mevsimli olabilir.</summary>
    public sealed class FavorTemplate
    {
        public string Id, Role, Item;
        public int Count;
        public Season[] Seasons;
        public ObjectiveKind Kind = ObjectiveKind.Deliver;
        public string Region;
        public FavorTemplate(string id, string role, string item, int count, params Season[] seasons)
        { Id = id; Role = role; Item = item; Count = count; Seasons = seasons; }
    }

    /// <summary>Mevsim Defteri işi (belge 46 §6.2).</summary>
    public sealed class SeasonalTemplate
    {
        public string Id;
        public Season Season;
        public int From, To;   // mevsim içindeki gün (1 tabanlı)
        public Objective Objective;
    }

    public sealed class CallTemplate
    {
        public string Id, Role;
        public Season[] Seasons;
        public bool Hard;
        public readonly List<CallPath> Paths = new List<CallPath>();
    }

    public sealed class StoryStep
    {
        public Objective Objective;
        public Reward Reward;
    }

    public sealed class BequestTemplate
    {
        public string Id, Role;
        public Objective Objective;
    }

    public sealed class VowTemplate
    {
        public string Id;
        public readonly List<VowStage> Stages = new List<VowStage>();
        public int FinalSeals;
        public int FinalReputation;
    }

    /// <summary>
    /// İstek şablonları (belge 46 §15.4, 1.0 asgari): ricalar, mevsim işleri, çağrılar, hikâyeler, vasiyetler ve adaklar veridir;
    /// siparişler tariflerden üretilir (RequestService.MakeOrder). Eşyası içerikte olmayan şablon kullanılmaz (test bunu denetler).
    /// Metinler kodda değil, arayüz tablolarında (ui.en.json, ui.tr.json): anahtarlar şablon kimliğinden türer (<see cref="RequestKeys"/>);
    /// metindeki {giver} veren köylünün adıyla dolar.
    /// </summary>
    /// <summary>İstek metin anahtarları (ui tabloları). Şablon kimliğinden türer; kayıtta metin değil anahtar durur.</summary>
    public static class RequestKeys
    {
        public static string Title(string templateId) => "req." + templateId + ".title";
        public static string Text(string templateId) => "req." + templateId + ".text";
        public static string Ignore(string templateId) => "req." + templateId + ".ignore";
        public static string Path(string callId, int i) => "req." + callId + ".path" + i;
        public static string PathOutcome(string callId, int i) => Path(callId, i) + ".out";
        public static string Stage(string vowId, int i) => "req." + vowId + ".stage" + i;
        public static string StoryTitle(string storyKey, int step) => "req.story." + storyKey + "." + step + ".title";
        public static string StoryText(string storyKey, int step) => "req.story." + storyKey + "." + step + ".text";
        public static string StoryKeep(string storyKey, int step) => "req.story." + storyKey + "." + step + ".keep";
    }

    public static class RequestCatalog
    {
        private static readonly Season Sp = Season.Spring, Su = Season.Summer, Au = Season.Autumn, Wi = Season.Winter;

        // ------------------------------------------------------------------ ricalar
        public static readonly FavorTemplate[] Favors =
        {
            new FavorTemplate("rica.smith.charcoal", "blacksmith", "item.charcoal", 6),
            new FavorTemplate("rica.smith.firewood", "blacksmith", "item.firewood", 8),
            new FavorTemplate("rica.smith.ore", "blacksmith", "item.iron_ore", 3),
            new FavorTemplate("rica.miller.barley", "miller", "item.barley", 5, Su, Au),
            new FavorTemplate("rica.miller.wheat", "miller", "item.wheat", 5, Su, Au),
            new FavorTemplate("rica.miller.firewood", "miller", "item.firewood", 6),
            new FavorTemplate("rica.herb.chamomile", "herbalist", "item.chamomile", 3, Sp, Su),
            new FavorTemplate("rica.herb.nettle", "herbalist", "item.nettle", 4, Sp, Su),
            new FavorTemplate("rica.herb.herb", "herbalist", "item.herb", 4, Sp, Su),
            new FavorTemplate("rica.herb.rosehip", "herbalist", "item.rosehip", 3, Au),
            new FavorTemplate("rica.shepherd.lamb", "shepherd", null, 1, Sp, Su) { Kind = ObjectiveKind.Visit, Region = "forest" },
            new FavorTemplate("rica.shepherd.salt", "shepherd", "item.salt", 2),
            new FavorTemplate("rica.trader.egg", "trader", "item.egg", 6),
            new FavorTemplate("rica.trader.honey", "trader", "item.honey", 2, Su, Au),
            new FavorTemplate("rica.trader.cheese", "trader", "item.cheese", 2),
            new FavorTemplate("rica.potter.clay", "potter", "item.clay", 6),
            new FavorTemplate("rica.potter.sand", "potter", "item.sand", 4),
            new FavorTemplate("rica.carpenter.log", "carpenter", "item.log", 4),
            new FavorTemplate("rica.carpenter.plank", "carpenter", "item.plank", 6),
            new FavorTemplate("rica.inn.bread", "innkeeper", "item.bread", 4),
            new FavorTemplate("rica.inn.egg", "innkeeper", "item.egg", 6),
            new FavorTemplate("rica.inn.carp", "innkeeper", "item.fish_carp", 3),
            new FavorTemplate("rica.scribe.gall", "scribe", "item.oak_gall", 3, Au),
            new FavorTemplate("rica.scribe.wax", "scribe", "item.beeswax", 2),
            new FavorTemplate("rica.tailor.wool", "tailor", "item.wool", 3),
            new FavorTemplate("rica.tailor.flax", "tailor", "item.flax", 4, Su, Au),
            new FavorTemplate("rica.saddler.hide", "saddler", "item.hide", 3),
            new FavorTemplate("rica.saddler.tallow", "saddler", "item.tallow", 2),
            new FavorTemplate("rica.fisher.bread", "fisher", "item.bread", 3),
            new FavorTemplate("rica.fisher.firewood", "fisher", "item.firewood", 4),
            new FavorTemplate("rica.mason.limestone", "mason", "item.limestone", 4),
            new FavorTemplate("rica.mason.stone", "mason", "item.stone", 8),
            new FavorTemplate("rica.any.elder_wood", "*", "item.firewood", 6, Au, Wi),
            new FavorTemplate("rica.any.soup", "*", "item.stew_plain", 1, Wi),
            new FavorTemplate("rica.any.mushroom", "*", "item.mushroom", 4, Au),
            new FavorTemplate("rica.any.berries", "*", "item.berries", 4, Su),
        };

        // ------------------------------------------------------------------ mevsim defteri (4 × 7)
        public static readonly SeasonalTemplate[] Seasonal =
        {
            S("mev.sp.plough", Sp, 1, 3, Act(SkillType.Farming, 8)),
            S("mev.sp.lambing", Sp, 1, 5, Act(SkillType.AnimalHusbandry, 4)),
            S("mev.sp.ditch", Sp, 2, 4, Del("item.stone", 6)),
            S("mev.sp.seeds", Sp, 4, 4, Del("item.seed_leek", 4)),
            S("mev.sp.thatch", Sp, 5, 9, Del("item.plank", 6)),
            S("mev.sp.swarm", Sp, 1, 10, Act(SkillType.AnimalHusbandry, 2)),
            S("mev.sp.shearing", Sp, 8, 10, Del("item.wool", 3)),
            S("mev.su.hay", Su, 1, 4, Act(SkillType.Farming, 6)),
            S("mev.su.uplands", Su, 1, 3, Act(SkillType.AnimalHusbandry, 4)),
            S("mev.su.fair", Su, 5, 5, Del("item.bread", 4)),
            S("mev.su.harvest", Su, 6, 9, Del("item.wheat", 8)),
            S("mev.su.flax", Su, 6, 8, Del("item.flax", 6)),
            S("mev.su.well", Su, 7, 10, Del("item.stone", 6)),
            S("mev.su.dry", Su, 8, 10, Del("item.dried_apple", 4)),
            S("mev.au.vintage", Au, 1, 3, Del("item.grape", 8)),
            S("mev.au.nuts", Au, 1, 5, Del("item.chestnut", 6)),
            S("mev.au.sowing", Au, 2, 5, Act(SkillType.Farming, 8)),
            S("mev.au.pekmez", Au, 4, 6, Del("item.pekmez", 2)),
            S("mev.au.preserve", Au, 6, 9, Del("item.pickled_cabbage", 3)),
            S("mev.au.elderwood", Au, 7, 10, Del("item.firewood", 10)),
            S("mev.au.acorns", Au, 8, 10, Act(SkillType.AnimalHusbandry, 4)),
            S("mev.wi.watch", Wi, 1, 10, Act(SkillType.AnimalHusbandry, 3)),
            S("mev.wi.ice", Wi, 2, 6, Act(SkillType.Mining, 3)),
            S("mev.wi.tools", Wi, 1, 10, Del("item.charcoal", 6)),
            S("mev.wi.night", Wi, 4, 4, Del("item.tallow_candle", 4)),
            S("mev.wi.weaving", Wi, 3, 8, Del("item.yarn", 4)),
            S("mev.wi.snow", Wi, 1, 10, Del("item.firewood", 6)),
            S("mev.wi.prune", Wi, 8, 10, Act(SkillType.Horticulture, 3)),
        };

        /// <summary>Mevsim takımı ödülü (belge 46 §6.3): yeni defter parçası önce, sonra mevsimin süsleri.</summary>
        public static string[] SeasonSet(Season s)
        {
            switch (s)
            {
                case Season.Spring: return new[] { "item.defter_spring_ribbon", "item.spring_garland", "item.spring_branch" };
                case Season.Summer: return new[] { "item.defter_summer_sheaf", "item.spring_garland" };
                case Season.Autumn: return new[] { "item.defter_autumn_basket", "item.harvest_heap", "item.door_wreath" };
                default: return new[] { "item.defter_winter_lantern", "item.winter_greenery", "item.door_wreath" };
            }
        }

        // ------------------------------------------------------------------ çağrılar
        public static readonly CallTemplate[] Calls =
        {
            Call("cagri.wolves", "shepherd", true, new[] { Au, Wi },
                P(SkillType.Hunting, 7, hours: 4),
                P(SkillType.AnimalHusbandry, 5, hours: 3),
                P(SkillType.Charm, 4, hours: 3),
                P(null, 0, akce: 12)),
            Call("cagri.lost_child", "*", true, null,
                P(SkillType.Hunting, 2, hours: 3),
                P(SkillType.Foraging, 7, hours: 3),
                P(SkillType.Riding, 3, hours: 2)),
            Call("cagri.storm_roof", "*", false, null,
                P(SkillType.Carpentry, 3, item: "item.plank", count: 4, hours: 4),
                P(SkillType.Masonry, 2, item: "item.stone", count: 4, hours: 4),
                P(SkillType.Stewardship, 7, hours: 2),
                P(null, 0, akce: 10)),
            Call("cagri.birth", "*", true, null,
                P(SkillType.Medicine, 5, hours: 3),
                P(SkillType.Herbalism, 2, item: "item.tea_chamomile", count: 2, hours: 1),
                P(null, 0, akce: 15)),
            Call("cagri.animal_sick", "*", false, null,
                P(SkillType.Medicine, 8, hours: 3),
                P(SkillType.Herbalism, 2, item: "item.animal_salve", count: 1, hours: 1),
                P(SkillType.AnimalHusbandry, 6, hours: 3)),
            Call("cagri.feud", "*", false, null,
                P(SkillType.Charm, 6, hours: 2),
                P(SkillType.Music, 9, hours: 2),
                P(SkillType.Cooking, 5, item: "item.stew_plain", count: 2, hours: 2)),
            Call("cagri.well", "*", false, null,
                P(SkillType.Masonry, 4, item: "item.stone", count: 6, hours: 4),
                P(SkillType.Engineering, 3, item: "item.plank", count: 3, hours: 3),
                P(null, 0, akce: 12)),
            Call("cagri.millstone", "miller", true, null,
                P(SkillType.Masonry, 5, item: "item.stone", count: 4, hours: 4),
                P(SkillType.Mining, 5, hours: 4),
                P(null, 0, akce: 15)),
            Call("cagri.elder_sick", "*", false, new[] { Au, Wi },
                P(SkillType.Medicine, 3, hours: 2),
                P(SkillType.Herbalism, 1, item: "item.tea_chamomile", count: 1, hours: 1),
                P(SkillType.Cooking, 2, item: "item.stew_plain", count: 1, hours: 1)),
            Call("cagri.bear_hives", "*", false, new[] { Su, Au },
                P(SkillType.Hunting, 5, hours: 3),
                P(SkillType.AnimalHusbandry, 4, hours: 3),
                P(null, 0, akce: 8)),
        };

        // ------------------------------------------------------------------ hikâyeler (adlı köylüler, 3 adım)
        public static readonly Dictionary<string, StoryStep[]> NamedStories = new Dictionary<string, StoryStep[]>
        {
            { "villager.orhan", Steps(
                St(Del("item.charcoal", 4), Aff(20)),
                St(Skl(SkillType.Smithing, 3), Aff(25)),
                St(Talk(), Keep("item.elder_tin_cup"))) },
            { "villager.hatice", Steps(
                St(Del("item.flour", 3), Aff(20)),
                St(Act(SkillType.Baking, 3), Aff(25)),
                St(Talk(), Keep("item.bread_bowl"))) },
            { "villager.zeynep", Steps(
                St(Del("item.chamomile", 3), Aff(20)),
                St(Skl(SkillType.Medicine, 3), Aff(25)),
                St(Talk(), Keep("item.baby_first_shoes"))) },
            { "villager.murat", Steps(
                St(Vis("hill"), Aff(20)),
                St(Act(SkillType.AnimalHusbandry, 4), Aff(25)),
                St(Talk(), Keep("item.harvest_ribbon"))) },
            { "villager.kerem", Steps(
                St(Del("item.beeswax", 1), Aff(20)),
                St(Skl(SkillType.Literacy, 5), Aff(25)),
                St(Talk(), Keep("item.festival_cup"))) },
            { "villager.ayse", Steps(
                St(Del("item.sand", 4), Aff(20)),
                St(Skl(SkillType.Pottery, 6), Aff(25)),
                St(Talk(), Keep("item.home_ribbon"))) },
            { "villager.hasan", Steps(
                St(Vis("forest"), Aff(20)),
                St(Del("item.log", 4), Aff(25)),
                St(Talk(), Keep("item.elder_tin_cup"))) },
            { "villager.serife", Steps(
                St(Talk(), Aff(20)),
                St(Del("item.stew_plain", 1), Aff(25)),
                St(Talk(), Keep("item.festival_cup"))) },
            { "villager.selim", Steps(
                St(Del("item.oak_gall", 3), Aff(20)),
                St(Skl(SkillType.Literacy, 4), Aff(25)),
                St(Talk(), Keep("item.harvest_ribbon"))) },
            { "villager.elif", Steps(
                St(Del("item.wool", 3), Aff(20)),
                St(Skl(SkillType.Weaving, 5), Aff(25)),
                St(Talk(), Keep("item.home_ribbon"))) },
            { "villager.riza", Steps(
                St(Del("item.hide", 2), Aff(20)),
                St(Skl(SkillType.Riding, 4), Aff(25)),
                St(Talk(), Keep("item.harvest_ribbon"))) },
            { "villager.kemal", Steps(
                St(Vis("lake"), Aff(20)),
                St(Del("item.fish_pike", 1), Aff(25)),
                St(Talk(), Keep("item.first_fish_scale"))) },
            { "villager.mustafa", Steps(
                St(Del("item.stone", 6), Aff(20)),
                St(Skl(SkillType.Masonry, 4), Aff(25)),
                St(Talk(), Keep("item.elder_tin_cup"))) },
        };

        /// <summary>Meslek hikâyeleri: adlı köylünün yerine gelen halef ya da varis (belge 46 §15.3).</summary>
        public static readonly Dictionary<string, StoryStep[]> RoleStories = new Dictionary<string, StoryStep[]>
        {
            { "blacksmith", Steps(St(Del("item.charcoal", 4), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.harvest_ribbon"))) },
            { "miller", Steps(St(Del("item.wheat", 4), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.bread_bowl"))) },
            { "herbalist", Steps(St(Del("item.herb", 3), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.home_ribbon"))) },
            { "carpenter", Steps(St(Del("item.log", 3), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.elder_tin_cup"))) },
            { "innkeeper", Steps(St(Del("item.bread", 3), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.festival_cup"))) },
            { "tailor", Steps(St(Del("item.wool", 2), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.home_ribbon"))) },
            { "fisher", Steps(St(Vis("lake"), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.first_fish_scale"))) },
            { "mason", Steps(St(Del("item.stone", 4), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Keep("item.elder_tin_cup"))) },
        };

        /// <summary>Kişilik hikâyeleri: adsız köylüler (komşu hane üyeleri, yeni gelenler).</summary>
        public static readonly Dictionary<string, StoryStep[]> TraitStories = new Dictionary<string, StoryStep[]>
        {
            { "trait.generous", Steps(St(Del("item.bread", 3), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Rep(4))) },
            { "trait.sociable", Steps(St(Talk(), Aff(20)), St(Del("item.pekmez", 1), Aff(25)), St(Talk(), Rep(4))) },
            { "trait.introvert", Steps(St(Talk(), Aff(20)), St(Skl(SkillType.Literacy, 3), Aff(25)), St(Talk(), Rep(3))) },
            { "trait.gloomy", Steps(St(Del("item.tallow_candle", 2), Aff(20)), St(Talk(), Aff(25)), St(Talk(), Rep(3))) },
        };

        // ------------------------------------------------------------------ vasiyetler
        public static readonly BequestTemplate[] Bequests =
        {
            Beq("vas.smith", "blacksmith", Del("item.charcoal", 8)),
            Beq("vas.miller", "miller", Del("item.bread", 6)),
            Beq("vas.herb", "herbalist", Del("item.chamomile", 6)),
            Beq("vas.shepherd", "shepherd", Act(SkillType.AnimalHusbandry, 6)),
            Beq("vas.carpenter", "carpenter", Del("item.plank", 6)),
            Beq("vas.inn", "innkeeper", Del("item.firewood", 10)),
            Beq("vas.fisher", "fisher", Del("item.fish_carp", 4)),
            Beq("vas.mason", "mason", Del("item.cut_stone", 2)),
            Beq("vas.any.tree", "*", Act(SkillType.Horticulture, 2)),
            Beq("vas.any.flowers", "*", Del("item.chamomile", 3)),
            Beq("vas.any.music", "*", Skl(SkillType.Music, 3)),
        };

        // ------------------------------------------------------------------ adaklar (kuşaklar boyu)
        public static readonly VowTemplate[] Vows =
        {
            Vow("adak.bridge", 3, 10,
                Stage(Del("item.stone", 20)),
                Stage(Del("item.cut_stone", 8), Skl(SkillType.Masonry, 7)),
                Stage(Pay(60))),
            Vow("adak.seed", 2, 6,
                Stage(Act(SkillType.Farming, 30)),
                Stage(Skl(SkillType.Farming, 8)),
                Stage(Skl(SkillType.Farming, 10))),
            Vow("adak.chronicle", 3, 8,
                Stage(Del("item.oak_gall", 6), Del("item.beeswax", 2)),
                Stage(Skl(SkillType.Literacy, 6)),
                Stage(Skl(SkillType.Literacy, 10))),
            Vow("adak.school", 3, 10,
                Stage(Skl(SkillType.Teaching, 5)),
                Stage(Del("item.plank", 20), Pay(40)),
                Stage(Skl(SkillType.Teaching, 10))),
            Vow("adak.horse", 2, 6,
                Stage(Act(SkillType.Riding, 20)),
                Stage(Skl(SkillType.Riding, 8)),
                Stage(Skl(SkillType.AnimalHusbandry, 9))),
            Vow("adak.mill", 2, 8,
                Stage(Del("item.stone", 15)),
                Stage(Skl(SkillType.Engineering, 5)),
                Stage(Pay(50))),
            Vow("adak.fountain", 2, 8,
                Stage(Del("item.stone", 10), Del("item.limestone", 6)),
                Stage(Skl(SkillType.Masonry, 6)),
                Stage(Pay(40))),
        };

        // ------------------------------------------------------------------ yardımcılar
        private static SeasonalTemplate S(string id, Season s, int from, int to, Objective o) =>
            new SeasonalTemplate { Id = id, Season = s, From = from, To = to, Objective = o };
        public static Objective Act(SkillType skill, int n) => new Objective { Kind = ObjectiveKind.Act, Skill = skill, Count = n };
        public static Objective Del(string item, int n) => new Objective { Kind = ObjectiveKind.Deliver, ItemId = item, Count = n };
        public static Objective Skl(SkillType skill, int level) => new Objective { Kind = ObjectiveKind.Skill, Skill = skill, Level = level, Count = 1 };
        public static Objective Vis(string region) => new Objective { Kind = ObjectiveKind.Visit, Region = region, Count = 1 };
        public static Objective Talk() => new Objective { Kind = ObjectiveKind.Talk, Count = 1 };
        public static Objective Pay(int akce) => new Objective { Kind = ObjectiveKind.Pay, Count = akce };
        private static Reward Aff(int a) => new Reward { Affinity = a, Memory = 5 };
        private static Reward Rep(int r) => new Reward { Affinity = 20, Reputation = r, Memory = 10 };
        private static Reward Keep(string item) => new Reward { Affinity = 30, Memory = 15, KeepsakeItem = item };
        private static StoryStep St(Objective o, Reward r) => new StoryStep { Objective = o, Reward = r };
        private static StoryStep[] Steps(params StoryStep[] s) => s;
        private static BequestTemplate Beq(string id, string role, Objective o) => new BequestTemplate { Id = id, Role = role, Objective = o };

        private static CallPath P(SkillType? skill, int level, string item = null, int count = 0, int akce = 0, double hours = 0) =>
            new CallPath { Skill = skill, Level = level, ItemId = item, ItemCount = count, Akce = akce, Hours = hours };

        private static CallTemplate Call(string id, string role, bool hard, Season[] seasons, params CallPath[] paths)
        {
            var c = new CallTemplate { Id = id, Role = role, Hard = hard, Seasons = seasons };
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i].LabelKey = RequestKeys.Path(id, i);
                paths[i].OutcomeKey = RequestKeys.PathOutcome(id, i);
            }
            c.Paths.AddRange(paths);
            return c;
        }

        private static VowStage Stage(params Objective[] objectives)
        {
            var st = new VowStage { Reward = new Reward { Reputation = 3 } };
            st.Objectives.AddRange(objectives);
            return st;
        }

        private static VowTemplate Vow(string id, int seals, int rep, params VowStage[] stages)
        {
            var v = new VowTemplate { Id = id, FinalSeals = seals, FinalReputation = rep };
            for (int i = 0; i < stages.Length; i++) stages[i].TitleKey = RequestKeys.Stage(id, i);
            v.Stages.AddRange(stages);
            return v;
        }

        /// <summary>Kataloğun bütün metin anahtarları (yerelleştirme testi: ikisi de ui tablolarında olmalı).</summary>
        public static IEnumerable<string> AllKeys()
        {
            foreach (var f in Favors) { yield return RequestKeys.Title(f.Id); yield return RequestKeys.Text(f.Id); }
            foreach (var s in Seasonal) { yield return RequestKeys.Title(s.Id); yield return RequestKeys.Text(s.Id); }
            foreach (var c in Calls)
            {
                yield return RequestKeys.Title(c.Id); yield return RequestKeys.Text(c.Id); yield return RequestKeys.Ignore(c.Id);
                foreach (var p in c.Paths) { yield return p.LabelKey; yield return p.OutcomeKey; }
            }
            foreach (var dict in new[] { NamedStories, RoleStories, TraitStories })
                foreach (var kv in dict)
                    for (int i = 0; i < kv.Value.Length; i++)
                    {
                        yield return RequestKeys.StoryTitle(kv.Key, i); yield return RequestKeys.StoryText(kv.Key, i);
                        if (kv.Value[i].Reward.KeepsakeItem != null) yield return RequestKeys.StoryKeep(kv.Key, i);
                    }
            foreach (var b in Bequests) { yield return RequestKeys.Title(b.Id); yield return RequestKeys.Text(b.Id); }
            foreach (var v in Vows)
            {
                yield return RequestKeys.Title(v.Id); yield return RequestKeys.Text(v.Id);
                foreach (var st in v.Stages) yield return st.TitleKey;
            }
        }

        /// <summary>Şablonların kullandığı bütün eşya kimlikleri (içerik denetimi için).</summary>
        public static IEnumerable<string> AllItemIds()
        {
            foreach (var f in Favors) if (f.Item != null) yield return f.Item;
            foreach (var s in Seasonal) if (s.Objective.ItemId != null) yield return s.Objective.ItemId;
            foreach (var c in Calls) foreach (var p in c.Paths) if (p.ItemId != null) yield return p.ItemId;
            foreach (var dict in new[] { NamedStories, RoleStories, TraitStories })
                foreach (var kv in dict)
                    foreach (var st in kv.Value)
                    {
                        if (st.Objective.ItemId != null) yield return st.Objective.ItemId;
                        if (st.Reward.KeepsakeItem != null) yield return st.Reward.KeepsakeItem;
                    }
            foreach (var b in Bequests) if (b.Objective.ItemId != null) yield return b.Objective.ItemId;
            foreach (var v in Vows) foreach (var st in v.Stages) foreach (var o in st.Objectives) if (o.ItemId != null) yield return o.ItemId;
            foreach (Season s in Enum.GetValues(typeof(Season))) foreach (var id in SeasonSet(s)) yield return id;
        }

        /// <summary>Şablon sayımı (belge 46 §15.4 asgari hedefle karşılaştırma).</summary>
        public static int TemplateCount()
        {
            int n = Favors.Length + Seasonal.Length + Calls.Length + Bequests.Length + Vows.Length;
            foreach (var dict in new[] { NamedStories, RoleStories, TraitStories }) foreach (var kv in dict) n += kv.Value.Length;
            return n;
        }
    }
}
