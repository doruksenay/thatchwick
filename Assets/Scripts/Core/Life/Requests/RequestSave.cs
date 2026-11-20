using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Work;

namespace Thatchwick.Core.Life.Requests
{
    /// <summary>İstek sisteminin kaydı (LifeSaveData.requests). JsonUtility uyumlu; alanlar eklemeli, sürüm artmaz.</summary>
    [Serializable]
    public sealed class RequestSave
    {
        public int nextId;
        public long seasonPosted = -1;
        public int playerScore;
        public RequestEntrySave[] requests;
        public int[] memoryKeys, memoryValues;
        public int[] rivalKeys, rivalValues;
        public string[] storyKeys;
        public int[] storySteps;
        public string[] originalDefs;
        public int[] originalIds;
        public string[] vowsDone;
        public string[] setPieces;
    }

    [Serializable]
    public sealed class ObjectiveSave
    {
        public int kind, count, progress, level, villager, duty, minQuality;
        public int skill = -1;
        public string item, act, region;
    }

    [Serializable]
    public sealed class RewardSave
    {
        public int akce, affinity, reputation, seals, memory;
        public double xp;
        public int xpSkill = -1;
        public string[] items;
        public int[] counts;
        /// <summary>Hatıra yazısı: LocCodec ile anahtarlı metin.</summary>
        public string keepsakeItem, keepsakeLabel;
    }

    [Serializable]
    public sealed class CallPathSave
    {
        public string label, item, outcome;   // label ve outcome: ui anahtarı
        public int skill = -1;
        public int level, itemCount, akce;
        public double hours;
    }

    [Serializable]
    public sealed class VowStageSave
    {
        public string title;
        public ObjectiveSave[] objectives;
        public RewardSave reward;
    }

    [Serializable]
    public sealed class RequestEntrySave
    {
        public int id, kind, giverId, houseId, state, assignedTo, penalty, level, chosenPath = -1, solverId;
        public int season = -1, dayFrom, dayTo, stage, vowedBy, storyStep;
        public int skill = -1;
        /// <summary>title, text, ignoreText: LocCodec ile anahtarlı metin (eski kayıtta düz Türkçe; düz metin olarak okunur).</summary>
        public string template, title, text, giverName, giverDef, ignoreText, vowedByName, storyKey;
        public double postedDay, acceptedDay = -1, deadline = double.MaxValue, days, villageProgress, vowedDay;
        public bool read, stretch, master;
        public int deliveredQuality = -1;   // sipariş derecesi (53); eski kayıtta -1
        public ObjectiveSave[] objectives;
        public RewardSave reward;
        public CallPathSave[] paths;
        public VowStageSave[] stages;
    }

    internal static class RequestSaveMapper
    {
        // JsonUtility double.MaxValue'yu yazabilir ama bazı ayrıştırıcılar taşar; süresiz istek -1 olarak saklanır.
        private const double NoDeadline = -1;

        public static RequestEntrySave Save(Request r)
        {
            var e = new RequestEntrySave
            {
                id = r.Id, kind = (int)r.Kind, template = r.TemplateId, title = LocCodec.Encode(r.Title), text = LocCodec.Encode(r.Text), giverId = r.GiverId, giverName = r.GiverName,
                giverDef = r.GiverDefId, houseId = r.HouseId, postedDay = r.PostedDay, acceptedDay = r.AcceptedDay,
                deadline = r.HasDeadline ? r.Deadline : NoDeadline, days = r.Days, state = (int)r.State, read = r.Read, assignedTo = r.AssignedTo,
                penalty = r.Penalty, skill = r.Skill.HasValue ? (int)r.Skill.Value : -1, level = r.Level, stretch = r.Stretch, master = r.Master, deliveredQuality = r.DeliveredQuality,
                chosenPath = r.ChosenPath, solverId = r.SolverId, ignoreText = LocCodec.Encode(r.IgnoreText), season = r.Season, dayFrom = r.DayFrom, dayTo = r.DayTo,
                villageProgress = r.VillageProgress, stage = r.Stage, vowedBy = r.VowedBy, vowedByName = r.VowedByName, vowedDay = r.VowedDay,
                storyStep = r.StoryStep, storyKey = r.StoryKey,
                objectives = SaveObjectives(r.Objectives), reward = SaveReward(r.Reward)
            };
            var paths = new List<CallPathSave>();
            foreach (var p in r.Paths)
                paths.Add(new CallPathSave
                {
                    label = p.LabelKey, skill = p.Skill.HasValue ? (int)p.Skill.Value : -1, level = p.Level, item = p.ItemId, itemCount = p.ItemCount,
                    akce = p.Akce, hours = p.Hours, outcome = p.OutcomeKey
                });
            e.paths = paths.ToArray();
            var stages = new List<VowStageSave>();
            foreach (var st in r.Stages) stages.Add(new VowStageSave { title = st.TitleKey, objectives = SaveObjectives(st.Objectives), reward = SaveReward(st.Reward) });
            e.stages = stages.ToArray();
            return e;
        }

        public static Request Load(RequestEntrySave e)
        {
            if (e == null || !Enum.IsDefined(typeof(RequestKind), e.kind) || !Enum.IsDefined(typeof(RequestState), e.state)) return null;
            var r = new Request
            {
                Id = e.id, Kind = (RequestKind)e.kind, TemplateId = e.template, Title = LocCodec.Decode(e.title), Text = LocCodec.Decode(e.text), GiverId = e.giverId, GiverName = e.giverName,
                GiverDefId = N(e.giverDef), HouseId = e.houseId, PostedDay = e.postedDay, AcceptedDay = e.acceptedDay,
                Deadline = e.deadline < 0 ? double.MaxValue : e.deadline, Days = e.days, State = (RequestState)e.state, Read = e.read,
                AssignedTo = e.assignedTo, Penalty = e.penalty, Skill = Skill(e.skill), Level = e.level, Stretch = e.stretch, Master = e.master, DeliveredQuality = e.deliveredQuality,
                ChosenPath = e.chosenPath, SolverId = e.solverId, IgnoreText = LocCodec.Decode(e.ignoreText), Season = e.season, DayFrom = e.dayFrom, DayTo = e.dayTo,
                VillageProgress = e.villageProgress, Stage = e.stage, VowedBy = e.vowedBy, VowedByName = N(e.vowedByName), VowedDay = e.vowedDay,
                StoryStep = e.storyStep, StoryKey = N(e.storyKey), Reward = LoadReward(e.reward)
            };
            LoadObjectives(e.objectives, r.Objectives);
            if (e.paths != null)
                foreach (var p in e.paths)
                    if (p != null)
                        r.Paths.Add(new CallPath { LabelKey = N(p.label), Skill = Skill(p.skill), Level = p.level, ItemId = N(p.item), ItemCount = p.itemCount, Akce = p.akce, Hours = p.hours, OutcomeKey = N(p.outcome) });
            if (e.stages != null)
                foreach (var st in e.stages)
                {
                    if (st == null) continue;
                    var stage = new VowStage { TitleKey = N(st.title), Reward = LoadReward(st.reward) };
                    LoadObjectives(st.objectives, stage.Objectives);
                    r.Stages.Add(stage);
                }
            return r;
        }

        /// <summary>JsonUtility boş dizgiyi "" yazar; çekirdek null bekler.</summary>
        private static string N(string s) => string.IsNullOrEmpty(s) ? null : s;

        private static SkillType? Skill(int v) => v >= 0 && Enum.IsDefined(typeof(SkillType), v) ? (SkillType?)v : null;

        private static ObjectiveSave[] SaveObjectives(List<Objective> list)
        {
            var arr = new ObjectiveSave[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var o = list[i];
                arr[i] = new ObjectiveSave
                {
                    kind = (int)o.Kind, item = o.ItemId, count = o.Count, progress = o.Progress, skill = o.Skill.HasValue ? (int)o.Skill.Value : -1,
                    level = o.Level, act = o.ActId, region = o.Region, villager = o.VillagerId, duty = (int)o.DutyKind, minQuality = o.MinQuality
                };
            }
            return arr;
        }

        private static void LoadObjectives(ObjectiveSave[] arr, List<Objective> into)
        {
            if (arr == null) return;
            foreach (var o in arr)
            {
                if (o == null || !Enum.IsDefined(typeof(ObjectiveKind), o.kind)) continue;
                into.Add(new Objective
                {
                    Kind = (ObjectiveKind)o.kind, ItemId = N(o.item), Count = o.count, Progress = o.progress, Skill = Skill(o.skill), Level = o.level,
                    ActId = N(o.act), Region = N(o.region), VillagerId = o.villager,
                    DutyKind = Enum.IsDefined(typeof(TimedDutyKind), o.duty) ? (TimedDutyKind)o.duty : TimedDutyKind.None,
                    MinQuality = Math.Max(0, o.minQuality)
                });
            }
        }

        private static RewardSave SaveReward(Reward w)
        {
            if (w == null) return null;
            var s = new RewardSave
            {
                akce = w.Akce, affinity = w.Affinity, reputation = w.Reputation, xp = w.Xp, xpSkill = w.XpSkill.HasValue ? (int)w.XpSkill.Value : -1,
                seals = w.Seals, keepsakeItem = w.KeepsakeItem, keepsakeLabel = LocCodec.Encode(w.KeepsakeLabel), memory = w.Memory,
                items = new string[w.Items.Count], counts = new int[w.Items.Count]
            };
            for (int i = 0; i < w.Items.Count; i++) { s.items[i] = w.Items[i].Key; s.counts[i] = w.Items[i].Value; }
            return s;
        }

        private static Reward LoadReward(RewardSave s)
        {
            var w = new Reward();
            if (s == null) return w;
            w.Akce = s.akce; w.Affinity = s.affinity; w.Reputation = s.reputation; w.Xp = s.xp; w.XpSkill = Skill(s.xpSkill); w.Seals = s.seals;
            w.KeepsakeItem = string.IsNullOrEmpty(s.keepsakeItem) ? null : s.keepsakeItem;
            w.KeepsakeLabel = LocCodec.Decode(s.keepsakeLabel); w.Memory = s.memory;
            if (s.items != null && s.counts != null)
                for (int i = 0; i < Math.Min(s.items.Length, s.counts.Length); i++) if (s.items[i] != null) w.Items.Add(new KeyValuePair<string, int>(s.items[i], s.counts[i]));
            return w;
        }
    }
}
