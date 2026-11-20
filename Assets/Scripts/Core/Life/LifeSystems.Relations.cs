using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Relations;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// İlişki sistemi bağlantıları (Project belgeleri 67, 67a-d): RelationService, bilgi, muhabbet, ihtiyar heyeti. Kancalar
    /// burada kurulur; oyun katmanı yalnızca poz, balon ve panelle bağlanır.
    /// </summary>
    public sealed partial class LifeSystems
    {
        public RelationService Relations { get; private set; }
        public KnowledgeService Knowledge { get; private set; }
        public GroupChatService Chats { get; private set; }
        public ElderCouncil Council { get; private set; }

        /// <summary>Kısa poz isteği (karakter, poz, gerçek saniye): oyun katmanı oynatır.</summary>
        public event Action<Character, string, float> PoseCue;
        /// <summary>Hanenin evinin önü (sahnelerde oyuncu hanesine toplanma); oyun katmanı kurar.</summary>
        public Func<GridPos?> HouseholdHome { get; set; }
        /// <summary>Meyhane ve sofra gibi ek muhabbet yerleri (oyun katmanı).</summary>
        public Func<GridPos, bool> ExtraGathering { get; set; }

        private readonly Dictionary<int, JobRunner> _runnersById = new Dictionary<int, JobRunner>();
        private int _imeceWork;

        private void InitRelations(int seed)
        {
            Relations = new RelationService(Population, seed != 0 ? Seeds.Derive(seed, 67) : 67);
            Knowledge = new KnowledgeService(Population, seed != 0 ? Seeds.Derive(seed, 673) : 673);
            Chats = new GroupChatService(Population, seed != 0 ? Seeds.Derive(seed, 677) : 677);
            Council = new ElderCouncil(Population, seed != 0 ? Seeds.Derive(seed, 679) : 679);
            Relations.Knowledge = Knowledge; Relations.Chats = Chats; Relations.Council = Council;
            Knowledge.Relations = Relations; Chats.Relations = Relations; Chats.Knowledge = Knowledge; Council.Relations = Relations;

            Func<double> now = () => Clock.TotalDays;
            Relations.Today = now; Knowledge.Today = now; Council.Today = now;
            Action<LocText, int> notice = (t, id) => Say(t, NoticeLevel.Info, id);
            Relations.Notice = notice; Knowledge.Notice = notice; Council.Notice = notice;
            Action<Character, LocText> bubble = (c, t) => { if (c != null && !t.IsEmpty) Bubble?.Invoke(c, t); };
            Relations.Bubble = bubble; Knowledge.Bubble = bubble; Chats.Bubble = bubble;
            Action<Character, string, float> cue = (c, pose, secs) => PoseCue?.Invoke(c, pose, secs);
            Relations.Cue = cue; Chats.Cue = cue;
            Action<RepFacet, int, LocText, Character> rep = (facet, n, why, who) =>
            {
                if (who != null && who.IsVillager) return;
                Upbringing.Reputation.Add(n, Text(why), (int)facet, who?.Id ?? 0);
            };
            Relations.Reputation = rep; Knowledge.Reputation = rep; Council.Reputation = rep;
            Func<Character, int> houseOf = c => c == null ? -1 : !c.IsVillager ? 0 : Houses.HouseOf(c)?.Id ?? -1;
            Relations.HouseOf = houseOf; Knowledge.HouseOf = houseOf;
            Relations.IsFestivalDay = () => Festivals.TodaysFestival() != null;
            Action<Character, double> hold = (c, days) => { if (c != null && _runnersById.TryGetValue(c.Id, out var r)) r.Hold(days); };
            Relations.Hold = hold; Chats.Hold = hold;
            Relations.Command = (c, job) => { if (c != null && _runnersById.TryGetValue(c.Id, out var r)) r.Command(job); };
            Relations.TakeItem = pred =>
            {
                foreach (var kv in House.InventoryList())
                    if (kv.Value > 0 && pred(kv.Key) && House.TryTakeItem(kv.Key)) return kv.Key;
                return null;
            };
            Relations.HasItem = pred =>
            {
                foreach (var kv in House.InventoryList()) if (kv.Value > 0 && pred(kv.Key)) return true;
                return false;
            };
            Relations.Gather = (c, host) =>
            {
                if (c == null || host == null || !_runnersById.TryGetValue(c.Id, out var r)) return;
                var spot = SpotOf(host);
                if (spot.HasValue) r.Command(Job.MoveTo(spot.Value));
            };
            Relations.HouseMembers = HouseMembersOf;
            Relations.PayAkce = n => House.TrySpend(n);
            Relations.AddAkce = n => { if (n > 0) House.AddAkce(n); };
            Relations.MarryHousehold = (member, other) => _family?.Apply(member, other, SocialAction.Propose);
            Relations.MarryVillagers = (a, b) => Houses.Marry(a, b);
            Relations.HouseReputation = c => c == null ? 0 : !c.IsVillager ? Upbringing.Reputation.Points : Knowledge.HouseScore(Houses.HouseOf(c)?.Id ?? 0) * 10;
            Relations.NearHome = (c, host) => c != null && host != null && c.Position.Z == host.Position.Z
                && Math.Abs(c.Position.X - host.Position.X) + Math.Abs(c.Position.Y - host.Position.Y) <= 10;
            Relations.DaysPerYear = () => Clock.Config.DaysPerSeason * 4;
            Relations.AwayAtNight = c =>
            {
                var home = HouseholdHome?.Invoke();
                return home.HasValue && !c.IsVillager && Math.Abs(c.Position.X - home.Value.X) + Math.Abs(c.Position.Y - home.Value.Y) > 14;
            };
            Relations.SendToBirthHouse = c =>
            {
                if (c == null) return;
                c.IsVillager = true;
                c.CanStarve = false;
                Houses.ReturnToBirthHouse(c);
                Homes.AssignAll();
            };

            Knowledge.AbsentMinded = c => c != null && c.Stage == AgeStage.Elder && Ailments.Has(c, Thatchwick.Core.Elders.AgingCondition.Absent);
            Knowledge.HouseName = id => Houses.Get(id)?.Name ?? "";
            Knowledge.RivalHouses = RivalHouseIds;
            Knowledge.DaysPerYear = () => Clock.Config.DaysPerSeason * 4;
            Knowledge.HouseMembersById = id => { var h = Houses.Get(id); return h == null ? Array.Empty<Character>() : (IEnumerable<Character>)Houses.LivingMembers(h); };

            Chats.IsFree = c => !_runnersById.TryGetValue(c.Id, out var r) || r.IsIdle || r.HoldDays > 0;
            Chats.IsGathering = IsGatheringSpot;

            Council.Heads = CouncilHeads;
            Council.PlayerReputation = () => Upbringing.Reputation.Points;
            Council.PlayerHead = HouseholdHead;
            Council.Options = CouncilOptions;
            Council.Prioritize = id => Say(LocText.Of("rel.n.council_priority", ("what", ProjectName(id))), NoticeLevel.Info, 0);
            Council.ScribeAlive = () => Villagers.HasRole("scribe");
            Council.PayAkce = n => House.TrySpend(n);
            Council.AtSquare = c =>
            {
                var sq = SquarePosition?.Invoke();
                return sq.HasValue && c.Position.Z == sq.Value.Z && Math.Abs(c.Position.X - sq.Value.X) + Math.Abs(c.Position.Y - sq.Value.Y) <= 10;
            };
            Council.ImeceWork = n => _imeceWork = Math.Min(10, _imeceWork + n);
            // Heyetin seçtiği proje ve imece emeği köy projesinin akçesini düşürür (heyet kararı, imece).
            var baseFactor = Projects.AkceFactor;
            Projects.AkceFactor = id =>
            {
                double f = baseFactor != null ? baseFactor(id) : 1.0;
                if (id == Council.LastDecision) f *= 0.8 * (1.0 - _imeceWork * 0.02);
                return f;
            };
            Projects.Completed += p => { if (p.Id == Council.LastDecision) _imeceWork = 0; };

            Houses.Courted = c => Relations.CourtedByHousehold(c) || Relations.PartnerOf(c) != null;
            Houses.Engage = (a, b) => Relations.EngageVillagers(a, b);
            Population.BabyBorn += (baby, mother, father) =>
            {
                Relations.OnBorn(baby);
                if (!baby.IsVillager) Upbringing.Reputation.InheritNam(baby.Id, mother?.Id ?? 0, father ?? 0);
            };
            Legacy.HeirloomSold += h =>
            {
                var head = HouseholdHead();
                if (head != null && head.SpouseId.HasValue) Relations.ReportTrigger(SpouseTrigger.SoldHeirloom, head);
            };
            Bets.Settled += o =>
            {
                if (o == null || o.Won) return;
                var head = HouseholdHead();
                if (head != null && head.SpouseId.HasValue) Relations.ReportTrigger(SpouseTrigger.Gambling, head);
            };
            SteppedInIt += (c, item) =>
            {
                if (c == null || c.IsVillager || !c.SpouseId.HasValue) return;
                var spouse = Population.Find(c.SpouseId.Value);
                if (spouse != null && Math.Abs(spouse.Position.X - c.Position.X) + Math.Abs(spouse.Position.Y - c.Position.Y) <= 6) Relations.ReportTrigger(SpouseTrigger.Mess, c);
            };
            Villagers.Relations = Relations;
            Villagers.EventTriggeredFor += (v, member, ev) => Relations.Met(v.Id, member.Id);
        }

        private void AttachRelations(FamilyService family)
        {
            family.Relations = Relations;
            Relations.Family = family;
            Relations.Happened += (a, b, action, text) => { if (action == SocialAction.Fight || action == SocialAction.Argue) Goals.Update(); };
            Outings.IsCouple = (a, b) => Relations.IsCouple(a.Id, b.Id);
            Outings.AddPairRomance = (a, b, n) => Relations.AddRomance(a.Id, b.Id, n);
            Outings.Warm = (a, b, n) => { if (a.SpouseId == b.Id) Relations.AddWarmth(a.Id, b.Id, n); };
        }

        private void AttachRelations(JobRunner runner)
        {
            var c = runner.Owner;
            if (c != null) _runnersById[c.Id] = runner;
            runner.SocialStarted += (a, t, action) => Relations.OnSocialStarted(a, t, action);
            runner.WorkDone += (who, skill, id) => { if (who != null && who.Duty != DutyKind.None) Relations.NoteChore(who); };
        }

        /// <summary>Karakterin iş yürütücüsü (oyun katmanı köylüler için de kaydeder).</summary>
        public void RegisterRunner(JobRunner runner)
        {
            if (runner?.Owner == null) return;
            if (_runnersById.ContainsKey(runner.Owner.Id) && ReferenceEquals(_runnersById[runner.Owner.Id], runner)) return;
            AttachRelations(runner);
        }

        private void RelationsTick(double gameDays)
        {
            if (Relations.NeedsLegacyMigration && _family != null) MigrateRelations();
            Relations.Tick();
            Relations.VillageTick(gameDays);
        }

        private void RelationsMoodStep()
        {
            Relations.UpdateNearMoods();
            Chats.Form(Clock.TotalDays);
            Council.TickImece(Household());
        }

        private void RelationsNewDay(long day)
        {
            Relations.OnNewDay(day);
            Knowledge.OnNewDay(day);
            Council.OnNewDay(day, Clock.DayOfSeason);
        }

        private void MigrateRelations()
        {
            var legacy = new List<(Character, int)>();
            foreach (var s in Villagers.States)
            {
                var v = Population.Find(s.CharacterId);
                if (v != null && s.Romance > 0) legacy.Add((v, s.Romance));
            }
            Relations.MigrateLegacyRomance(legacy);
        }

        private IEnumerable<Character> HouseMembersOf(Character c)
        {
            if (c == null) yield break;
            if (!c.IsVillager) { foreach (var m in Household()) yield return m; yield break; }
            var h = Houses.HouseOf(c);
            if (h == null) { yield return c; yield break; }
            foreach (var m in Houses.LivingMembers(h)) yield return m;
        }

        public Character HouseholdHead()
        {
            Character best = null;
            foreach (var m in Population.Members)
                if (!m.IsDead && !m.IsVillager && m.Stage >= AgeStage.YoungAdult && (best == null || m.AgeDays > best.AgeDays)) best = m;
            return best;
        }

        private GridPos? SpotOf(Character host)
        {
            if (host == null) return null;
            if (!host.IsVillager) return HouseholdHome?.Invoke() ?? host.Position;
            var def = Villagers.DefOf(host);
            var resolve = Villagers.PlaceResolver;
            if (def != null && resolve != null) return resolve(def.Home);
            var h = Houses.HouseOf(host);
            if (h != null && !string.IsNullOrEmpty(h.HomeKey) && resolve != null) return resolve(h.HomeKey);
            return host.Position;
        }

        private IEnumerable<int> RivalHouseIds()
        {
            foreach (var h in Houses.Houses) if (!h.Extinct) yield return h.Id;
        }

        private IEnumerable<(int, Character)> CouncilHeads()
        {
            foreach (var h in Houses.Houses)
            {
                if (h.Extinct) continue;
                var head = Population.Find(h.HeadId);
                if (head != null && !head.IsDead) yield return (h.Id, head);
            }
        }

        private IEnumerable<CouncilOption> CouncilOptions()
        {
            foreach (var p in Content.Projects.All)
                if (!Projects.IsDone(p.Id)) yield return new CouncilOption { Id = p.Id, Name = ProjectName(p.Id) };
        }

        private LocText ProjectName(string id) =>
            Content.Projects.TryGet(id ?? "", out var p) ? LocText.Plain(p.DisplayName) : LocText.Plain(id ?? "");

        /// <summary>Muhabbet yeri: şenlik günü her yer, akşam meydan, imece günü meydan, düğün evinin önü, meyhane ve sofra (oyun katmanı).</summary>
        private bool IsGatheringSpot(GridPos pos)
        {
            if (ExtraGathering != null && ExtraGathering(pos)) return true;
            var sq = SquarePosition?.Invoke();
            bool nearSquare = sq.HasValue && pos.Z == sq.Value.Z && Math.Abs(pos.X - sq.Value.X) + Math.Abs(pos.Y - sq.Value.Y) <= 12;
            if (Festivals.TodaysFestival() != null) return true;
            double frac = Clock.DayFraction;
            if (nearSquare && frac >= VillagerService.WorkUntil && frac < VillagerService.SquareUntil + 0.03) return true;
            if (nearSquare && Council.ImeceToday) return true;
            foreach (var e in Relations.Events)
            {
                if (!e.Started || e.Done || e.Offscreen) continue;
                var host = Population.Find(e.Kind == CourtEventKind.Wedding ? e.Groom : e.Bride);
                if (host != null && host.Position.Z == pos.Z && Math.Abs(host.Position.X - pos.X) + Math.Abs(host.Position.Y - pos.Y) <= 8) return true;
            }
            return false;
        }

        private RelationsSave ExportRelations() => Relations.Export();

        private void ImportRelations(RelationsSave s) => Relations.Import(s);

        /// <summary>Ölüm (67 H5): yası ilişkiye göre ölçekler, arkadaşa yas.</summary>
        private void RelationsOnDeath(Character dead) => Relations.OnDeath(dead);

        /// <summary>Çocuk yalnız dolaşırken görüldü (T4 ve dedikodu).</summary>
        private void RelationsOnGaze(Thatchwick.Core.Upbringing.GazeEvent ev)
        {
            if (ev == null || !ev.Scolding || ev.Child == null) return;
            foreach (var pid in new[] { ev.Child.BirthMotherId, ev.Child.BirthFatherId })
            {
                var parent = pid.HasValue ? Population.Find(pid.Value) : null;
                if (parent == null || parent.IsDead || parent.IsVillager) continue;
                Knowledge.Record(FactKind.LostChild, parent, ev.Child, RepFacet.Word, -1, ev.Villager != null ? new[] { ev.Villager } : null);
                if (parent.SpouseId.HasValue) { Relations.ReportTrigger(SpouseTrigger.NeglectedChild, parent); break; }
            }
        }
    }
}
