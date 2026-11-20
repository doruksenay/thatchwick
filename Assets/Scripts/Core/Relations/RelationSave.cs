using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Relations
{
    [Serializable] public sealed class PairSave
    {
        public int a, b, romance, status, dynamic, warmth = -1, sulkBy, betrayals, coldDays, chats, anniversaryYear = -1;
        public double statusDay = -1, lastMet = -1, drift, sulkDay = -1, weddingDay = -1, marriedDay = -1, lastFlirt = -1;
        public long chatDay = -1;
        public bool driftWarned, sulking, introduced;
    }

    [Serializable] public sealed class FeelingSave { public int from, to, kind; public double fromDay, untilDay; }
    [Serializable] public sealed class PrefsSave { public int character, like1, like2, dislike; }
    [Serializable] public sealed class KnownPrefsSave { public int knower, about, n; }
    [Serializable] public sealed class CourtEventSave
    {
        public int id, kind, groom, bride, step, coffee = -1;
        public double startDay;
        public bool started, done, offscreen;
        public int[] guests;
    }
    [Serializable] public sealed class DecisionSave { public int id, kind, a, b, eventId, amount; public double deadline; }
    [Serializable] public sealed class IssueSave { public int a, b, trigger, offender, count; public double day; }

    /// <summary>İlişki sisteminin kaydı (eklemeli alan; eski kayıtta null: romantik VillagerState'ten taşınır).</summary>
    [Serializable] public sealed class RelationsSave
    {
        public PairSave[] pairs;
        public FeelingSave[] feelings;
        public PrefsSave[] prefs;
        public KnownPrefsSave[] knownPrefs;
        public CourtEventSave[] events;
        public DecisionSave[] decisions;
        public IssueSave[] issues;
        public int nextEventId = 1, nextDecisionId = 1;
        public KnowledgeSave knowledge;
        public CouncilSave council;
    }

    public sealed partial class RelationService
    {
        public RelationsSave Export()
        {
            var s = new RelationsSave { nextEventId = _nextEventId, nextDecisionId = _nextDecisionId };
            var pairs = new List<PairSave>();
            foreach (var p in _pairs.Values)
                pairs.Add(new PairSave
                {
                    a = p.A, b = p.B, romance = p.Romance, status = (int)p.Status, dynamic = (int)p.Dynamic, warmth = p.Warmth, sulkBy = p.SulkBy,
                    betrayals = p.Betrayals, coldDays = p.ColdDays, chats = p.ChatsToday, anniversaryYear = p.AnniversaryYear, statusDay = p.StatusDay,
                    lastMet = p.LastMet, drift = p.Drift, sulkDay = p.SulkDay, weddingDay = p.WeddingDay, marriedDay = p.MarriedDay, lastFlirt = p.LastFlirt,
                    chatDay = p.ChatDay, driftWarned = p.DriftWarned, sulking = p.Sulking, introduced = p.Introduced
                });
            s.pairs = pairs.ToArray();
            var feelings = new List<FeelingSave>();
            foreach (var (from, to, f) in AllFeelings()) feelings.Add(new FeelingSave { from = from, to = to, kind = (int)f.Kind, fromDay = f.FromDay, untilDay = f.UntilDay });
            s.feelings = feelings.ToArray();
            var prefs = new List<PrefsSave>();
            foreach (var kv in _prefs) prefs.Add(new PrefsSave { character = kv.Key, like1 = (int)kv.Value.Like1, like2 = (int)kv.Value.Like2, dislike = (int)kv.Value.Dislike });
            s.prefs = prefs.ToArray();
            var known = new List<KnownPrefsSave>();
            foreach (var kv in _prefsKnown) known.Add(new KnownPrefsSave { knower = (int)(kv.Key >> 32), about = (int)(kv.Key & 0xffffffff), n = kv.Value });
            s.knownPrefs = known.ToArray();
            var events = new List<CourtEventSave>();
            foreach (var e in _events)
                if (!e.Done)
                    events.Add(new CourtEventSave { id = e.Id, kind = (int)e.Kind, groom = e.Groom, bride = e.Bride, step = e.Step, coffee = e.Coffee, startDay = e.StartDay, started = e.Started, offscreen = e.Offscreen, guests = e.Guests.ToArray() });
            s.events = events.ToArray();
            var decisions = new List<DecisionSave>();
            foreach (var d in _decisions) decisions.Add(new DecisionSave { id = d.Id, kind = (int)d.Kind, a = d.A, b = d.B, eventId = d.EventId, amount = d.Amount, deadline = d.Deadline });
            s.decisions = decisions.ToArray();
            var issues = new List<IssueSave>();
            foreach (var kv in _issues) issues.Add(new IssueSave { a = (int)(kv.Key >> 32), b = (int)(kv.Key & 0xffffffff), trigger = (int)kv.Value.Trigger, offender = kv.Value.Offender, count = kv.Value.Count, day = kv.Value.Day });
            s.issues = issues.ToArray();
            s.knowledge = Knowledge?.Export();
            s.council = Council?.Export();
            return s;
        }

        /// <summary>Eski kayıtta (null) romantik yakınlık sonradan <see cref="MigrateLegacyRomance"/> ile taşınır.</summary>
        public bool NeedsLegacyMigration { get; private set; }

        public void Import(RelationsSave s)
        {
            _pairs.Clear(); _feelings.Clear(); _prefs.Clear(); _prefsKnown.Clear(); _events.Clear(); _decisions.Clear(); _issues.Clear();
            NeedsLegacyMigration = s == null;
            Knowledge?.Import(s?.knowledge);
            Council?.Import(s?.council);
            if (s == null) return;
            _nextEventId = Math.Max(1, s.nextEventId); _nextDecisionId = Math.Max(1, s.nextDecisionId);
            if (s.pairs != null)
                foreach (var x in s.pairs)
                {
                    if (x == null) continue;
                    var p = Pair(x.a, x.b);
                    p.Romance = x.romance; p.Status = (CoupleStatus)x.status; p.Dynamic = (FamilyDynamic)x.dynamic; p.Warmth = x.warmth; p.SulkBy = x.sulkBy;
                    p.Betrayals = x.betrayals; p.ColdDays = x.coldDays; p.ChatsToday = x.chats; p.AnniversaryYear = x.anniversaryYear; p.StatusDay = x.statusDay;
                    p.LastMet = x.lastMet; p.Drift = x.drift; p.SulkDay = x.sulkDay; p.WeddingDay = x.weddingDay; p.MarriedDay = x.marriedDay; p.LastFlirt = x.lastFlirt;
                    p.ChatDay = x.chatDay; p.DriftWarned = x.driftWarned; p.Sulking = x.sulking; p.Introduced = x.introduced;
                }
            if (s.feelings != null)
                foreach (var f in s.feelings)
                {
                    if (f == null) continue;
                    long k = Dir(f.from, f.to);
                    if (!_feelings.TryGetValue(k, out var list)) { list = new List<Feeling>(); _feelings[k] = list; }
                    list.Add(new Feeling { Kind = (FeelingKind)f.kind, FromDay = f.fromDay, UntilDay = f.untilDay });
                }
            if (s.prefs != null) foreach (var x in s.prefs) if (x != null) _prefs[x.character] = new AttractionPrefs { Like1 = (AttractionTag)x.like1, Like2 = (AttractionTag)x.like2, Dislike = (AttractionTag)x.dislike };
            if (s.knownPrefs != null) foreach (var x in s.knownPrefs) if (x != null) _prefsKnown[Dir(x.knower, x.about)] = x.n;
            if (s.events != null)
                foreach (var x in s.events)
                {
                    if (x == null) continue;
                    var e = new CourtEvent { Id = x.id, Kind = (CourtEventKind)x.kind, Groom = x.groom, Bride = x.bride, Step = x.step, Coffee = x.coffee, StartDay = x.startDay, Started = x.started, Offscreen = x.offscreen };
                    if (x.guests != null) e.Guests.AddRange(x.guests);
                    _events.Add(e);
                }
            if (s.decisions != null) foreach (var x in s.decisions) if (x != null) _decisions.Add(new PendingDecision { Id = x.id, Kind = (DecisionKind)x.kind, A = x.a, B = x.b, EventId = x.eventId, Amount = x.amount, Deadline = x.deadline });
            if (s.issues != null) foreach (var x in s.issues) if (x != null) _issues[Key(x.a, x.b)] = new SpouseIssue { Trigger = (SpouseTrigger)x.trigger, Offender = x.offender, Count = x.count, Day = x.day };
        }

        /// <summary>
        /// Eski kayıt: köylü başına tek romantik sayı (VillagerState.Romance) hanede o köylüyle arkadaşlığı en yüksek üyeye
        /// taşınır (67 H1). Evli çiftlere sıcaklık ve evlilik günü verilir.
        /// </summary>
        public void MigrateLegacyRomance(IEnumerable<(Characters.Character villager, int romance)> legacy)
        {
            NeedsLegacyMigration = false;
            if (legacy != null)
                foreach (var (v, r) in legacy)
                {
                    if (v == null || r <= 0) continue;
                    Characters.Character best = null; int bestF = int.MinValue;
                    foreach (var m in _population.Members)
                        if (!m.IsDead && !m.IsVillager && CanRomance(m, v) && Friendship(m.Id, v.Id) > bestF) { best = m; bestF = Friendship(m.Id, v.Id); }
                    if (best != null) Pair(best.Id, v.Id).Romance = Math.Min(RelationRules.Max, r);
                }
            foreach (var m in _population.Members)
                if (!m.IsDead && m.SpouseId.HasValue && m.Id < m.SpouseId.Value)
                {
                    var p = Pair(m.Id, m.SpouseId.Value);
                    if (p.Warmth < 0) p.Warmth = WarmthStart;
                    if (p.MarriedDay < 0) { p.MarriedDay = Math.Floor(Today()); p.AnniversaryYear = 0; }
                }
        }
    }
}
