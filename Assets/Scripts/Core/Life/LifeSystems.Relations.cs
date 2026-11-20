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
        /// <summary>Kişi bir karoya dönsün (karakter, karo, bu oyun gününe dek): sahnede karşısındakine, muhabbette halkanın ortasına.</summary>
        public event Action<Character, GridPos, double> FaceCue;
        /// <summary>
        /// Oyuncunun eli olan kişi (seçili karakter): satış ve bahis gibi hane işlerini "kim yaptı" sorusunda o sayılır.
        /// Oyun katmanı kurar; yoksa oyuncu karakteri, o da yoksa hane başı.
        /// </summary>
        public Func<Character> ActingMember { get; set; }
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
            Relations.GatherScene = (host, people, days) =>
            {
                var center = SpotOf(host);
                if (!center.HasValue) return;
                var roles = new List<SceneRole>();
                foreach (var (_, role) in people) roles.Add(role);
                var map = World.Map;
                var seats = SceneSeats.Layout(center.Value, roles, q => map.InBounds(q) && map.IsWalkable(q));
                double until = Clock.TotalDays + days;
                for (int i = 0; i < people.Count; i++)
                {
                    var c = people[i].who;
                    if (c == null) continue;
                    var (seat, face) = seats[i];
                    if (seat.HasValue && seat.Value != c.Position && _runnersById.TryGetValue(c.Id, out var r)) r.Command(Job.MoveTo(seat.Value));
                    FaceCue?.Invoke(c, face, until);
                }
            };
            Chats.FaceTo = (c, at) => FaceCue?.Invoke(c, at, Clock.TotalDays + GroupChatService.MaxLength);
            Relations.HouseMembers = HouseMembersOf;
            Relations.PayAkce = n => House.TrySpend(n);
            Relations.AddAkce = n => { if (n > 0) House.AddAkce(n); };
            Relations.MarryHousehold = (member, other) => _family?.Apply(member, other, SocialAction.Propose);
            Relations.MarryVillagers = (a, b) => Houses.Marry(a, b);
            Relations.HouseReputation = c => c == null ? 0 : !c.IsVillager ? Upbringing.Reputation.Points : Knowledge.HouseScore(Houses.HouseOf(c)?.Id ?? 0) * 10;
            Relations.NearHome = (c, host) => c != null && host != null && c.Position.Z == host.Position.Z
                && Math.Abs(c.Position.X - host.Position.X) + Math.Abs(c.Position.Y - host.Position.Y) <= 10;
            Relations.DaysPerYear = () => Clock.Config.DaysPerSeason * 4;
            Relations.AwayAtNight = AwayAtNight;
            Relations.Tipsy = c => c != null && Thatchwick.Core.Content.ContentPacks.Adult && c.Mood.Has(DrinkMood);
            Relations.Flee = (c, danger) =>
            {
                var spot = FleeSpot(c.Position, danger);
                if (spot.HasValue && _runnersById.TryGetValue(c.Id, out var r)) { r.Command(Job.MoveTo(spot.Value).AsHurry()); r.Hold(0.02); }
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
            Chats.MinAt = p => NearMeal(p) ? 2 : GroupChatService.MinMembers;

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
            // T2 (67c §2): yadigârı satan, oyuncunun o an eli olan kişidir. Eşinin soyundan kalan yadigâr ağır, kendi soyundan
            // kalan hafif kırar ("Annemin yüzüğünü mü sattın?").
            Legacy.HeirloomSold += h =>
            {
                var seller = Seller();
                if (seller == null || !seller.SpouseId.HasValue) return;
                var spouse = Population.Find(seller.SpouseId.Value);
                Relations.ReportTrigger(SpouseTrigger.SoldHeirloom, seller, scale: HeirloomScale(h, seller, spouse));
            };
            // Kumar (+18): bahsi yatıran kişi kaybetti; eşin kırgınlığı yatırılan akçeyle büyür.
            Bets.Settled += o =>
            {
                if (o == null || o.Won || o.Bet == null) return;
                var bettor = o.Bet.BettorId > 0 ? Population.Find(o.Bet.BettorId) : null;
                if (bettor == null || bettor.IsDead || bettor.IsVillager) bettor = Seller();
                if (bettor != null && bettor.SpouseId.HasValue) Relations.ReportTrigger(SpouseTrigger.Gambling, bettor, scale: GambleScale(o.Bet.Stake));
            };
            // T3: çeyiz ve düğün takısı (gelinin getirdikleri) satılırsa gelin, kocasına kırılır.
            Relations.GiveItem = (id, n) => { if (Content.Items.Contains(id)) House.AddItem(id, n); };
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
            // Gelinin getirdikleri (KeepsakeService.OnJoined çeyizi envantere koyduktan sonra) defterine yazılır.
            family.JoinedHousehold += c =>
            {
                if (c == null || c.Sex != Sex.Female) return;
                foreach (var (id, n) in Keepsakes.LastDowry) Relations.RecordDowry(c.Id, id, n);
            };
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
            runner.ItemUsed += (who, item, it) => { if (who != null && !who.IsVillager && it != null && it.Need == NeedType.Hunger && item != null) NoteMeal(who, item.Position); };
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

        /// <summary>Hane işini (satış, bahis) yapan: oyun katmanının seçili kişisi, oyuncu karakteri ya da hane başı.</summary>
        public Character Seller()
        {
            var c = ActingMember?.Invoke();
            if (c != null && !c.IsDead && !c.IsVillager) return c;
            foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager && m.IsPlayerControlled) return m;
            return HouseholdHead();
        }

        /// <summary>Yadigârın eşi kırma katı: eşin kendisinin ya da yakınının yaptığıysa 1,5; satanın soyundansa 0,5; değilse 1.</summary>
        public static double HeirloomScale(Heirloom h, Character seller, Character spouse)
        {
            if (h == null || spouse == null) return 1.0;
            if (h.MakerId == spouse.Id) return 1.5;
            var maker = h.MakerId;
            bool KinOf(Character c) => c != null && (c.BirthMotherId == maker || c.BirthFatherId == maker);
            if (KinOf(spouse)) return 1.5;
            if (seller != null && (h.MakerId == seller.Id || KinOf(seller))) return 0.5;
            return 1.0;
        }

        /// <summary>Kaybedilen bahsin kırgınlık katı: 5 akçe hafif (0,4), 50 akçe ağır (1,4).</summary>
        public static double GambleScale(int stake) => stake <= 5 ? 0.4 : stake <= 10 ? 0.6 : stake <= 25 ? 1.0 : 1.4;

        /// <summary>Hanenin bir eşyadan kaç tane sahip olduğu: envanter ve kurulu olanlar.</summary>
        private int OwnedCount(string id)
        {
            int n = House.Count(id);
            foreach (var it in World.Items) if (it.Def != null && it.Def.Id == id && World.Map.IsOwned(it.Position)) n++;
            return n;
        }

        /// <summary>Bir mal haneden satışla ya da takasla çıktı: gelinin çeyizinden eksildiyse T3.</summary>
        /// <summary>Pazar bağlanınca: satış ve takasla haneden çıkan mal çeyiz defterine bakılır (T3).</summary>
        private void AttachRelations(Economy.Market market)
        {
            market.Sold += (id, n) => DowryLeft(id, n);
            market.OfferAccepted += o => { if (o != null) DowryLeft(o.GiveItem, o.GiveCount); };
        }

        /// <summary>Kurulu eşya satıldı (oyun katmanı BuildService.PlacedItemSold): çeyiz sandığıysa T3.</summary>
        public void OnPlacedItemSold(ItemInstance item)
        {
            if (item?.Def != null) DowryLeft(item.Def.Id, 1);
        }

        private void DowryLeft(string id, int count)
        {
            if (string.IsNullOrEmpty(id) || count <= 0) return;
            Relations.OnGoodsLeft(id, count, OwnedCount(id), Seller());
        }

        // ---- sofra sonrası muhabbet ----

        /// <summary>Yemekten sonra sofrada kalınan süre (oyun günü) ve sofra çevresi (karo).</summary>
        public const double MealChatDays = 0.05;
        public const int MealChatRange = 4;
        private readonly List<(int who, GridPos at, double day)> _meals = new List<(int, GridPos, double)>();

        /// <summary>Hane üyesi yemeğini bitirdi: aynı sofrada az önce yiyen başka biri varsa ikisi de biraz oturur (muhabbet kurulur).</summary>
        internal void NoteMeal(Character who, GridPos at)
        {
            double now = Clock.TotalDays;
            _meals.RemoveAll(m => now - m.day > MealChatDays || m.who == who.Id);
            _meals.Add((who.Id, at, now));
            int diners = 0;
            foreach (var m in _meals) if (NearTiles(m.at, at, MealChatRange)) diners++;
            if (diners < 2) return;
            foreach (var m in _meals)
                if (NearTiles(m.at, at, MealChatRange) && _runnersById.TryGetValue(m.who, out var r) && r.IsIdle) r.Hold(MealChatDays * 0.6);
        }

        private static bool NearTiles(GridPos a, GridPos b, int range) => a.Z == b.Z && Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) <= range;

        /// <summary>Az önce iki ya da daha çok kişinin yemek yediği sofranın çevresi mi.</summary>
        internal bool NearMeal(GridPos pos)
        {
            double now = Clock.TotalDays;
            int n = 0;
            foreach (var m in _meals) if (now - m.day <= MealChatDays && NearTiles(m.at, pos, MealChatRange)) n++;
            return n >= 2;
        }

        // ---- gece dışarıda (T10) ----

        public const string DrinkMood = "mood.drink";

        /// <summary>
        /// Gece evden uzakta mı: iş için uzakta olan (görevde, bir işin başında), şenlik gecesi, sürmekte olan bir düğün ya da
        /// nişan sahnesindeki ve hane evine yakın olan sayılmaz.
        /// </summary>
        internal bool AwayAtNight(Character c)
        {
            if (c == null || c.IsVillager || c.Away) return false;
            var home = HouseholdHome?.Invoke();
            if (!home.HasValue || NearTiles(c.Position, home.Value, 14)) return false;
            if (Festivals.TodaysFestival() != null) return false;
            foreach (var e in Relations.Events) if (e.Started && !e.Done && e.Guests.Contains(c.Id)) return false;
            if (_runnersById.TryGetValue(c.Id, out var r) && IsNightWork(r.Queue.Current)) return false;
            return true;
        }

        /// <summary>Gece işi: hayvan ya da tarla işi, beceri işleyen bir eşya işi (eğlence ve içki sayılmaz).</summary>
        private bool IsNightWork(Job job)
        {
            if (job == null) return false;
            if (job.Kind == JobKind.Animal || job.Kind == JobKind.Tile) return true;
            if (job.Kind != JobKind.UseItem) return false;
            var item = World.GetItem(job.ItemId);
            if (item?.Def == null) return false;
            foreach (var it in item.Def.Interactions)
                if (it.Id == job.InteractionId) return it.Skill.HasValue && it.Need != NeedType.Fun;
            return false;
        }

        /// <summary>Kavgadan kaçış yeri: tehlikenin tersine 6 karo, yoksa yakınında boş bir karo.</summary>
        private GridPos? FleeSpot(GridPos from, GridPos danger)
        {
            int dx = Math.Sign(from.X - danger.X), dy = Math.Sign(from.Y - danger.Y);
            if (dx == 0 && dy == 0) dx = 1;
            var map = World.Map;
            for (int dist = 6; dist >= 3; dist--)
                for (int jitter = 0; jitter <= 2; jitter++)
                    foreach (int s in new[] { jitter, -jitter })
                    {
                        var p = new GridPos(from.X + dx * dist + (dy != 0 ? s : 0), from.Y + dy * dist + (dx != 0 ? s : 0), from.Z);
                        if (map.InBounds(p) && map.IsWalkable(p)) return p;
                    }
            return null;
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
            if (NearMeal(pos)) return true;
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
