using System;
using System.Collections.Generic;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Life
{
    [Serializable]
    public sealed class HomeItemSave
    {
        public int id;
        public string[] upgrades;
        public string pattern;
        public double potDry, potGrowth;
        public double strewUntil = -1;
    }

    [Serializable]
    public sealed class HomeRoomSave
    {
        public string key;
        public double soot;
        public double whitewashUntil = -1;
        public bool warned;
    }

    [Serializable]
    public sealed class HomePatternSave
    {
        public string id, name, cells;
    }

    [Serializable]
    public sealed class HomePlanSave
    {
        public string name;
        public int width, height;
        public string[] entries;
    }

    [Serializable]
    public sealed class HomeSeasonSetSave
    {
        public int season;
        public string[] entries;
    }

    /// <summary>Ev bakımının kaydı (belge 45). Eski kayıtta yok: her şey boş başlar, mumlar dolu yanar.</summary>
    [Serializable]
    public sealed class HomeSave
    {
        public HomeItemSave[] items;
        public HomeRoomSave[] rooms;
        public int[] candlesSeen;
        public HomePatternSave[] patterns;
        public int patternSeq;
        public string arma;
        public string[] collectionsDone;
        public HomePlanSave[] plans;
        public HomeSeasonSetSave[] seasonSets;
        public bool autoSeasonSwap = true;
        public int servantId, servantWage, servantDays;
        public double servantLoyalty;
        public long[] contestYears;
        public int contestWins;
    }

    public sealed partial class HomeLife
    {
        private void ExportExtras(HomeSave save)
        {
            var pats = new List<HomePatternSave>();
            foreach (var p in _patterns) pats.Add(new HomePatternSave { id = p.Id, name = p.Name, cells = p.Cells });
            save.patterns = pats.ToArray();
            save.patternSeq = _patternSeq;
            save.arma = _arma?.Encode();
            save.collectionsDone = new List<string>(_collectionsDone).ToArray();
            var plans = new List<HomePlanSave>();
            foreach (var p in _plans)
            {
                var e = new List<string>();
                foreach (var x in p.Entries) e.Add(x.Encode());
                plans.Add(new HomePlanSave { name = p.Name, width = p.Width, height = p.Height, entries = e.ToArray() });
            }
            save.plans = plans.ToArray();
            var sets = new List<HomeSeasonSetSave>();
            foreach (var kv in _seasonSets)
            {
                var e = new List<string>();
                foreach (var x in kv.Value) e.Add(x.Encode());
                sets.Add(new HomeSeasonSetSave { season = (int)kv.Key, entries = e.ToArray() });
            }
            save.seasonSets = sets.ToArray();
            save.autoSeasonSwap = AutoSeasonSwap;
            save.servantId = ServantId; save.servantWage = ServantWage; save.servantDays = ServantDays; save.servantLoyalty = ServantLoyalty;
            save.contestYears = _contestYears.ToArray();
            save.contestWins = ContestWins;
        }

        private void ImportExtras(HomeSave save)
        {
            _patterns.Clear(); _arma = null; _patternSeq = 0; _collectionsDone.Clear(); _plans.Clear(); _seasonSets.Clear(); _contestYears.Clear();
            ServantId = 0; ServantWage = 0; ServantDays = 0; ServantLoyalty = 0; ContestWins = 0; AutoSeasonSwap = true;
            if (save == null) return;
            if (save.patterns != null)
                foreach (var p in save.patterns)
                    if (p != null && ValidCells(p.cells)) _patterns.Add(new PatternDef { Id = p.id, Name = p.name, Cells = p.cells });
            _patternSeq = Math.Max(save.patternSeq, _patterns.Count);
            _arma = ArmaDef.Decode(save.arma);
            if (_arma != null && ArmaRule(_arma) != null) _arma = null;
            if (save.collectionsDone != null) foreach (var c in save.collectionsDone) if (c != null) _collectionsDone.Add(c);
            if (save.plans != null)
                foreach (var p in save.plans)
                {
                    if (p == null) continue;
                    var plan = new RoomPlan { Name = p.name, Width = p.width, Height = p.height };
                    if (p.entries != null) foreach (var s in p.entries) { var e = PlanEntry.Decode(s); if (e != null) plan.Entries.Add(e); }
                    _plans.Add(plan);
                }
            if (save.seasonSets != null)
                foreach (var s in save.seasonSets)
                {
                    if (s == null || !Enum.IsDefined(typeof(Season), s.season)) continue;
                    var list = new List<PlanEntry>();
                    if (s.entries != null) foreach (var x in s.entries) { var e = PlanEntry.Decode(x); if (e != null) list.Add(e); }
                    _seasonSets[(Season)s.season] = list;
                }
            AutoSeasonSwap = save.autoSeasonSwap;
            ServantId = save.servantId; ServantWage = save.servantWage; ServantDays = save.servantDays; ServantLoyalty = save.servantLoyalty;
            if (save.contestYears != null) _contestYears.AddRange(save.contestYears);
            ContestWins = save.contestWins;
        }
    }
}
