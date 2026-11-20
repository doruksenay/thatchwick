using Thatchwick.Core.Localization;
using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;

namespace Thatchwick.Core.Life.Requests
{
    /// <summary>
    /// Hane hafızası (belge 46 §8.4): her NPC hanesi oyuncu hanedanına dair bir puan tutar. Yardım, teslim ve vasiyet artı;
    /// kabul edilip bitirilmeyen sipariş ve tek başına gezen çocuk eksi. Yeni kuşak köylünün oyuncunun yeni kuşağına başlangıç
    /// yakınlığı puanın %25'idir (karar 5, varsayılan).
    /// </summary>
    public sealed class HouseMemory
    {
        public const int Favor = 5, Order = 8, Call = 10, Story = 10, Bequest = 40, FailedOrder = -10, Gaze = -5;
        public const double InheritShare = 0.25;
        public const int Min = -200, Max = 400;

        private readonly Dictionary<int, int> _points = new Dictionary<int, int>();
        private readonly List<string> _log = new List<string>();

        /// <summary>Köylünün hanesi (yoksa null; o zaman kişinin kendisi -id anahtarıyla tutulur).</summary>
        public Func<Character, NeighborHouse> HouseOf { get; set; }
        public IReadOnlyList<string> Log => _log;

        public int KeyOf(Character villager)
        {
            if (villager == null) return 0;
            var h = HouseOf?.Invoke(villager);
            return h != null ? h.Id : -villager.Id;
        }

        public void Add(Character villager, int delta, string reason)
        {
            int key = KeyOf(villager);
            if (key == 0 || delta == 0) return;
            _points[key] = Math.Max(Min, Math.Min(Max, Of(key) + delta));
            if (!string.IsNullOrEmpty(reason))
            {
                _log.Add($"{(delta > 0 ? "+" : "")}{delta} {reason}");
                if (_log.Count > 20) _log.RemoveAt(0);
            }
        }

        public int Of(int key) => _points.TryGetValue(key, out int v) ? v : 0;
        public int Of(Character villager) => Of(KeyOf(villager));

        /// <summary>Hafızadan doğan başlangıç yakınlığı (0-100).</summary>
        public int InitialAffection(int key) => Math.Max(0, Math.Min(100, (int)Math.Round(Of(key) * InheritShare)));

        public IEnumerable<KeyValuePair<int, int>> All => _points;
        public void Set(int key, int value) { if (key != 0) _points[key] = value; }
        public void Clear() { _points.Clear(); _log.Clear(); }
    }

    /// <summary>Rakip hane sıralamasındaki bir satır.</summary>
    public struct HouseRank
    {
        public string Name;
        public int Points;
        public bool IsPlayer;
    }

    /// <summary>
    /// İstek sistemi (belge 46): ilan panosu, ricalar, siparişler, Mevsim Defteri, çağrılar, hikâyeler, vasiyetler ve adaklar.
    /// Üretim günlük (OnNewDay); ilerleme olaylarla (iş bitti, sohbet, bölgeye girdi, süreli görev döndü). UnityEngine yok.
    /// </summary>
    public sealed class RequestService
    {
        // ---- sıklık ve sınırlar (belge 46 §4, §11, §15; 12 dakikalık gün) ----
        public const double FavorChancePerDay = 0.65;
        public const int MaxFavorsPosted = 3, MaxFavorsAccepted = 3;
        public const int MaxOrdersAccepted = 2, StewardOrderLevel = 4;
        public const int OrderOptions = 2;
        public const double CallChancePerDay = 0.12;
        public const double FavorDays = 2, OrderDays = 4, CallDays = 1, BoardDays = 2;
        public const double RivalAfterDays = 1;
        public const int CourierFee = 2;
        public const int MasterLevel = 10;
        public const double FavorAkceFactor = 2.5, OrderAkceFactor = 2.0, MasterAkceFactor = 4.0;
        public const int FavorAkceMax = 40, OrderAkceMax = 80;
        public const int FavorAffinity = 30, OrderAffinity = 15, MasterAffinity = 20, CallAffinity = 40;
        public const int OrderRep = 2, MasterRep = 8, CallRep = 4, HardCallRep = 8, SeasonJobRep = 1, SeasonSetRep = 4, FailedOrderRep = -2;
        public const int SeasonSetJobs = 5;
        public const int MaxVows = 2;
        public const int BequestAffection = 60;
        public static readonly int[] StoryThresholds = { 30, 60, 100 };
        public const string BountifulMood = "mood.bountiful_season";

        private readonly Population _population;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly VillagerService _villagers;
        private readonly NeighborHouseService _houses;
        private readonly Random _rng;
        private readonly List<Request> _requests = new List<Request>();
        private readonly Dictionary<string, int> _storySteps = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _originals = new Dictionary<string, int>();
        private readonly Dictionary<int, int> _rivalScore = new Dictionary<int, int>();
        private readonly HashSet<string> _vowsDone = new HashSet<string>();
        private readonly HashSet<string> _setPieces = new HashSet<string>();
        private List<(ItemDef station, InteractionDef craft)> _recipes;
        private int _nextId = 1;
        private long _seasonPosted = -1;
        private double _lastTick = -1;

        public HouseMemory Memory { get; } = new HouseMemory();
        public int PlayerScore { get; private set; }
        public IReadOnlyList<Request> All => _requests;

        // ---- bağlantılar (LifeSystems kurar) ----
        public Func<double> Now { get; set; } = () => 0;
        public int DaysPerSeason { get; set; } = 10;
        public Func<bool> MarketDay { get; set; }
        public FamilyService Family { get; set; }
        public Legacy Legacy { get; set; }
        public KeepsakeService Keepsakes { get; set; }
        public TimedDutyService Duties { get; set; }
        public Action<int, string> AddReputation { get; set; }
        public Action<string, int> Notify { get; set; }
        public Action<string> Chronicle { get; set; }
        /// <summary>Teslim eden, verenin yanında mı? (oyun katmanı; null: her zaman yakın)</summary>
        public Func<Character, Character, bool> Near { get; set; }

        public event Action<Request> Completed;
        public event Action<Request> Posted;

        public RequestService(Population population, Household house, ContentDatabase content, VillagerService villagers, NeighborHouseService houses, int seed = 46)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _villagers = villagers;
            _houses = houses;
            _rng = new Random(seed);
            Memory.HouseOf = c => _houses?.HouseOf(c);
        }

        // ================================================================== sorgular

        public Request Get(int id)
        {
            foreach (var r in _requests) if (r.Id == id) return r;
            return null;
        }

        public IEnumerable<Request> Open(RequestKind kind)
        {
            foreach (var r in _requests) if (r.Kind == kind && r.IsOpen) yield return r;
        }

        public int CountOpen(RequestKind kind, RequestState state)
        {
            int n = 0;
            foreach (var r in _requests) if (r.Kind == kind && r.State == state) n++;
            return n;
        }

        public Season SeasonNow => (Season)(int)(SeasonIndex % TimeConfig.SeasonsPerYear);
        public long SeasonIndex => (long)Math.Floor(Now() / Math.Max(1, DaysPerSeason));
        /// <summary>Mevsimin kaçıncı günü (1 tabanlı).</summary>
        public int DayOfSeason => (int)((long)Math.Floor(Now()) % Math.Max(1, DaysPerSeason)) + 1;

        public IEnumerable<Character> Household()
        {
            foreach (var m in _population.Members) if (!m.IsDead && !m.IsVillager) yield return m;
        }

        public int Best(SkillType skill)
        {
            int best = 0;
            foreach (var m in Household()) best = Math.Max(best, m.Skills.GetLevel(skill));
            return best;
        }

        /// <summary>
        /// Okuryazarlık yumuşak bonusu (belge 46 §10): hanede Okuryazarlık 1 olan biri (ilk XP'yi almış) varsa pano defterden okunur
        /// ve bir sipariş fazla görünür.
        /// </summary>
        public bool Literate
        {
            get
            {
                foreach (var m in Household()) if (m.Stage >= AgeStage.Child && m.Skills.GetXp(SkillType.Literacy) > 0) return true;
                return false;
            }
        }

        public int MaxOrders => MaxOrdersAccepted + (Best(SkillType.Stewardship) >= StewardOrderLevel ? 1 : 0);
        public int UnreadCount
        {
            get
            {
                int n = 0;
                foreach (var r in _requests) if (r.State == RequestState.Posted && !r.Read && OnBoard(r)) n++;
                return n;
            }
        }

        public static bool OnBoard(Request r) => r.Kind == RequestKind.Favor || r.Kind == RequestKind.Order || r.Kind == RequestKind.Call;

        public string ItemName(string id) => id != null && _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        public string ObjectiveText(Objective o)
        {
            if (!string.IsNullOrEmpty(o.Text)) return o.Text;
            switch (o.Kind)
            {
                case ObjectiveKind.Deliver: return $"{o.Count} {ItemName(o.ItemId)} getir ({Math.Min(o.Progress, o.Count)}/{o.Count}; elde {_house.Count(o.ItemId)})";
                case ObjectiveKind.Act: return $"{(o.Skill.HasValue ? SkillNames.Name(o.Skill.Value) : "İş")}: {o.Count} kez çalış ({Math.Min(o.Progress, o.Count)}/{o.Count})";
                case ObjectiveKind.Visit: return $"{RegionName(o.Region)} bölgesine git";
                case ObjectiveKind.Talk: return "Konuş";
                case ObjectiveKind.Skill: return $"Hanede {(o.Skill.HasValue ? SkillNames.Name(o.Skill.Value) : "?")} {o.Level} olsun{(o.Done ? " ✓" : $" (en iyi {Best(o.Skill ?? SkillType.Farming)})")}";
                case ObjectiveKind.Pay: return $"{o.Count} akçe ver{(o.Done ? " ✓" : "")}";
                case ObjectiveKind.Duty: return $"{TimedDutyService.KindName(o.DutyKind)}: {o.Count} saat ({o.Progress}/{o.Count})";
                default: return "?";
            }
        }

        private static string RegionName(string id)
        {
            foreach (var r in Thatchwick.Core.World.Regions.All) if (r.Id == id) return r.Name;
            return id;
        }

        public string RewardText(Reward w)
        {
            var parts = new List<string>();
            if (w.Akce > 0) parts.Add($"{w.Akce} akçe");
            if (w.Affinity > 0) parts.Add($"yakınlık +{w.Affinity}");
            if (w.Reputation > 0) parts.Add($"itibar +{w.Reputation}");
            if (w.Xp > 0 && w.XpSkill.HasValue) parts.Add($"{SkillNames.Name(w.XpSkill.Value)} +{w.Xp:0} XP");
            foreach (var kv in w.Items) parts.Add($"{kv.Value} {ItemName(kv.Key)}");
            if (w.Seals > 0) parts.Add($"Soy Mührü +{w.Seals}");
            if (w.KeepsakeItem != null) parts.Add($"hatıra: {w.KeepsakeLabel}");
            return parts.Count == 0 ? "—" : string.Join(", ", parts);
        }

        /// <summary>Kalan süre metni ("1 gün 6 saat").</summary>
        public string TimeLeft(Request r)
        {
            if (!r.HasDeadline) return "süresiz";
            double left = r.Deadline - Now();
            if (left <= 0) return "doldu";
            int days = (int)Math.Floor(left);
            int hours = (int)Math.Round((left - days) * 24);
            if (hours == 24) { days++; hours = 0; }
            return days > 0 ? $"{days} gün {hours} saat" : $"{hours} saat";
        }

        /// <summary>Mevsim işinde köyün doluluğu (0-1): pencere ilerledikçe köy kendi payını yapar, hanenin payı %30.</summary>
        public double VillageProgress(Request r)
        {
            if (r.Kind != RequestKind.Seasonal) return 0;
            int day = DayOfSeason;
            int span = Math.Max(1, r.DayTo - r.DayFrom + 1);
            double village = Math.Max(0, Math.Min(1, (day - r.DayFrom + 1) / (double)span));
            if (r.State == RequestState.Expired || r.State == RequestState.Done) village = 1;
            double own = 0;
            foreach (var o in r.Objectives) own += o.Count > 0 ? Math.Min(1.0, o.Progress / (double)o.Count) : 0;
            own /= Math.Max(1, r.Objectives.Count);
            return Math.Min(1.0, village * 0.7 + own * 0.3);
        }

        /// <summary>Hanenin ilan sıralaması (belge 46 §8.5): siparişlerden kazanılan puan.</summary>
        public List<HouseRank> Ranking(string playerHouseName)
        {
            var list = new List<HouseRank> { new HouseRank { Name = playerHouseName ?? "Senin hanen", Points = PlayerScore, IsPlayer = true } };
            if (_houses != null)
                foreach (var h in _houses.Houses)
                {
                    if (h.Extinct) continue;
                    list.Add(new HouseRank { Name = h.Name, Points = _rivalScore.TryGetValue(h.Id, out int p) ? p : 0 });
                }
            list.Sort((a, b) => b.Points != a.Points ? b.Points.CompareTo(a.Points) : (a.IsPlayer ? -1 : b.IsPlayer ? 1 : string.CompareOrdinal(a.Name, b.Name)));
            return list;
        }

        public int RivalScore(int houseId) => _rivalScore.TryGetValue(houseId, out int p) ? p : 0;

        // ================================================================== günlük döngü

        /// <summary>Süreleri ve beceri hedeflerini denetler (sık çağrılabilir; ucuz).</summary>
        public void Tick()
        {
            double now = Now();
            if (_lastTick >= 0 && now - _lastTick < 0.02) return;
            _lastTick = now;
            RefreshSkills();
            foreach (var r in _requests.ToArray())
            {
                if (!r.IsOpen || !r.HasDeadline || now < r.Deadline) continue;
                Expire(r);
            }
        }

        /// <summary>Yeni gün: rakip haneler, dükkân varisleri, yeni ilanlar, mevsim defteri, hikâyeler.</summary>
        public void OnNewDay()
        {
            Tick();
            RecordOriginals();
            ReassignOrphans();
            RivalsTake();
            UpdateSeason();
            GenerateFavors();
            if (MarketDay != null && MarketDay()) GenerateOrders();
            if (_rng.NextDouble() < CallChancePerDay) GenerateCall();
            UpdateStories();
            if (Literate) ReadBoard();
            Prune();
        }

        private void Expire(Request r)
        {
            if (r.Kind == RequestKind.Seasonal) { r.State = RequestState.Expired; return; }
            if (r.State == RequestState.Accepted && r.Kind == RequestKind.Order)
            {
                r.State = RequestState.Failed;
                AddReputation?.Invoke(r.Penalty != 0 ? r.Penalty : FailedOrderRep, $"sipariş yetişmedi: {r.Title}");
                var giver = _population.Find(r.GiverId);
                if (giver != null) Memory.Add(giver, HouseMemory.FailedOrder, $"{r.Title} yetişmedi");
                Notify?.Invoke($"{r.GiverName}: \"{r.Title}\" siparişi yetişmedi. İtibar {r.Penalty}.", r.GiverId);
                return;
            }
            if (r.Kind == RequestKind.Call)
            {
                r.State = RequestState.Expired;
                if (!string.IsNullOrEmpty(r.IgnoreText)) Notify?.Invoke(r.IgnoreText, r.GiverId);
                return;
            }
            r.State = r.State == RequestState.Accepted ? RequestState.Failed : RequestState.Expired;
            if (r.Kind == RequestKind.Favor && r.State == RequestState.Failed) Notify?.Invoke($"{r.GiverName}: \"{r.Title}\" için geç kalındı (ceza yok).", r.GiverId);
        }

        /// <summary>Biten istekler defterde iki mevsim kalır, sonra silinir (kayıt büyümesin).</summary>
        private void Prune()
        {
            double now = Now();
            _requests.RemoveAll(r => !r.IsOpen && r.Kind != RequestKind.Vow && now - Math.Max(r.PostedDay, r.AcceptedDay) > DaysPerSeason * 2);
        }

        private void RecordOriginals()
        {
            if (_villagers == null) return;
            foreach (var st in _villagers.States)
                if (!_originals.ContainsKey(st.DefId) && st.CharacterId != 0) _originals[st.DefId] = st.CharacterId;
        }

        /// <summary>Veren ölmüşse sipariş dükkânı devralan varise geçer; yoksa cezasız düşer (belge 46 §8.1).</summary>
        private void ReassignOrphans()
        {
            foreach (var r in _requests)
            {
                if (!r.IsOpen || r.Kind != RequestKind.Order) continue;
                var giver = _population.Find(r.GiverId);
                if (giver != null && !giver.IsDead) continue;
                var heir = r.GiverDefId != null ? _villagers?.CharacterOf(r.GiverDefId) : null;
                if (heir != null && !heir.IsDead)
                {
                    r.GiverId = heir.Id;
                    r.GiverName = heir.Name;
                    Notify?.Invoke($"{heir.Name}, \"{r.Title}\" siparişini devraldı.", heir.Id);
                }
                else r.State = RequestState.Dropped;
            }
        }

        /// <summary>Panoda 1 gün alınmayan sipariş bir komşu haneye gider (belge 46 §8.5).</summary>
        private void RivalsTake()
        {
            if (_houses == null) return;
            double now = Now();
            var houses = new List<NeighborHouse>();
            foreach (var h in _houses.Houses) if (!h.Extinct && _houses.LivingCount(h) > 0) houses.Add(h);
            if (houses.Count == 0) return;
            foreach (var r in _requests)
            {
                if (r.Kind != RequestKind.Order || r.State != RequestState.Posted || now - r.PostedDay < RivalAfterDays) continue;
                var h = houses[_rng.Next(houses.Count)];
                r.State = RequestState.Rival;
                r.HouseId = h.Id;
                _rivalScore[h.Id] = RivalScore(h.Id) + (r.Master ? MasterRep : OrderRep);
                Notify?.Invoke($"{h.Name}, panodaki \"{r.Title}\" siparişini üstlendi.", 0);
            }
        }

        // ================================================================== üretim: ricalar

        private List<Character> LivingVillagers()
        {
            var list = new List<Character>();
            foreach (var m in _population.Members) if (m.IsVillager && !m.IsDead && m.Stage >= AgeStage.Teen) list.Add(m);
            return list;
        }

        private VillagerDef DefOf(Character c) => _villagers?.DefOf(c);

        private string RoleOf(Character c) => DefOf(c)?.Role;

        private bool HasOpenFrom(int giverId, RequestKind kind)
        {
            foreach (var r in _requests) if (r.IsOpen && r.Kind == kind && r.GiverId == giverId) return true;
            return false;
        }

        private bool HasOpenTemplate(string templateId)
        {
            foreach (var r in _requests) if (r.IsOpen && r.TemplateId == templateId) return true;
            return false;
        }

        private static bool InSeason(Season[] seasons, Season s)
        {
            if (seasons == null || seasons.Length == 0) return true;
            foreach (var x in seasons) if (x == s) return true;
            return false;
        }

        /// <summary>Günde bir kez: %65 olasılıkla yeni bir rica (en çok 3 askıda). Köylü sevmediği şeyi istemez (Coral Island kuralı).</summary>
        public Request GenerateFavors(double? roll = null)
        {
            if (CountOpen(RequestKind.Favor, RequestState.Posted) >= MaxFavorsPosted) return null;
            if ((roll ?? _rng.NextDouble()) >= FavorChancePerDay) return null;
            var season = SeasonNow;
            var candidates = new List<(Character giver, FavorTemplate t)>();
            foreach (var v in LivingVillagers())
            {
                if (HasOpenFrom(v.Id, RequestKind.Favor)) continue;
                var def = DefOf(v);
                foreach (var t in RequestCatalog.Favors)
                {
                    if (t.Role != "*" && (def == null || def.Role != t.Role)) continue;
                    if (!InSeason(t.Seasons, season) || HasOpenTemplate(t.Id)) continue;
                    if (t.Item != null)
                    {
                        if (!_content.Items.TryGet(t.Item, out var item)) continue;
                        if (def != null && VillagerService.TasteFor(def, item) == GiftTaste.Dislike) continue;
                    }
                    candidates.Add((v, t));
                }
            }
            if (candidates.Count == 0) return null;
            var pick = candidates[_rng.Next(candidates.Count)];
            return PostFavor(pick.giver, pick.t);
        }

        public Request PostFavor(Character giver, FavorTemplate t)
        {
            var r = NewRequest(RequestKind.Favor, t.Id, t.Title, t.Text, giver);
            r.Days = FavorDays;
            r.Deadline = Now() + BoardDays;
            if (t.Kind == ObjectiveKind.Visit) r.Objectives.Add(new Objective { Kind = ObjectiveKind.Visit, Region = t.Region, Count = 1 });
            else r.Objectives.Add(RequestCatalog.Del(t.Item, t.Count));
            int value = t.Item != null && _content.Items.TryGet(t.Item, out var d) ? d.Price * t.Count : 6;
            r.Reward = new Reward { Akce = Math.Max(3, Math.Min(FavorAkceMax, (int)Math.Round(value * FavorAkceFactor))), Affinity = FavorAffinity, Memory = HouseMemory.Favor };
            return Post(r);
        }

        /// <summary>Sistemik rica: komşu hanede bebek doğdu (beşik ya da battaniye), köylü öldü (cenaze helvası).</summary>
        public Request PostSystemicFavor(Character giver, string id, string title, string text, string item, int count, double days = 3)
        {
            if (giver == null || giver.IsDead || !_content.Items.Contains(item) || HasOpenTemplate(id + ":" + giver.Id)) return null;
            var r = NewRequest(RequestKind.Favor, id + ":" + giver.Id, title, text, giver);
            r.Days = days;
            r.Deadline = Now() + days;
            r.Objectives.Add(RequestCatalog.Del(item, count));
            int value = _content.Items.Get(item).Price * count;
            r.Reward = new Reward { Akce = Math.Max(3, Math.Min(FavorAkceMax, (int)Math.Round(value * 1.5))), Affinity = FavorAffinity, Memory = HouseMemory.Favor * 2 };
            return Post(r);
        }

        // ================================================================== üretim: siparişler

        private static readonly Dictionary<SkillType, string> SkillRole = new Dictionary<SkillType, string>
        {
            { SkillType.Smithing, "blacksmith" }, { SkillType.Baking, "miller" }, { SkillType.Herbalism, "herbalist" },
            { SkillType.AnimalHusbandry, "shepherd" }, { SkillType.Trade, "trader" }, { SkillType.Pottery, "potter" },
            { SkillType.Carpentry, "carpenter" }, { SkillType.Cooking, "innkeeper" }, { SkillType.Brewing, "innkeeper" },
            { SkillType.Literacy, "scribe" }, { SkillType.Weaving, "tailor" }, { SkillType.Leatherwork, "saddler" },
            { SkillType.Fishing, "fisher" }, { SkillType.Masonry, "mason" }, { SkillType.Chandlery, "trader" },
            { SkillType.Engineering, "carpenter" }, { SkillType.Horticulture, "herbalist" }
        };

        public static string RoleForSkill(SkillType s) => SkillRole.TryGetValue(s, out var r) ? r : null;

        private List<(ItemDef station, InteractionDef craft)> Recipes()
        {
            if (_recipes != null) return _recipes;
            _recipes = new List<(ItemDef, InteractionDef)>();
            var seen = new HashSet<string>();
            foreach (var item in _content.Items.All)
                foreach (var i in item.Interactions)
                {
                    if (!i.IsCraft || !i.Skill.HasValue || i.Produce.Count == 0) continue;
                    var pid = i.Produce[0].ItemId;
                    if (!_content.Items.TryGet(pid, out var prod) || prod.Price <= 0 || prod.HasTag("keepsake") || !seen.Add(pid)) continue;
                    if (RoleForSkill(i.Skill.Value) == null) continue;
                    _recipes.Add((item, i));
                }
            return _recipes;
        }

        private Character GiverForRole(string role)
        {
            if (_villagers == null || role == null) return null;
            foreach (var st in _villagers.States)
            {
                if (!_content.Villagers.TryGet(st.DefId, out var def) || def.Role != role) continue;
                var c = _population.Find(st.CharacterId);
                if (c != null && !c.IsDead) return c;
            }
            return null;
        }

        /// <summary>
        /// Pazar günü siparişler (belge 46 §5.1): hanenin yapabildiği tariflerden, en çok biri bir seviye üstte ("zorlayıcı").
        /// Okuryazar hanede bir seçenek fazla. Beceri 10'da usta siparişi.
        /// </summary>
        public List<Request> GenerateOrders()
        {
            var made = new List<Request>();
            int options = OrderOptions + (Literate ? 1 : 0);
            var fits = new List<(ItemDef, InteractionDef, Character)>();
            var stretch = new List<(ItemDef, InteractionDef, Character)>();
            foreach (var (station, craft) in Recipes())
            {
                var skill = craft.Skill.Value;
                var giver = GiverForRole(RoleForSkill(skill));
                if (giver == null) continue;
                int best = Best(skill);
                int need = Math.Max(1, craft.MinSkillLevel);
                if (HasOpenTemplate("order:" + craft.Produce[0].ItemId)) continue;
                if (need <= best) fits.Add((station, craft, giver));
                else if (need == best + 1) stretch.Add((station, craft, giver));
            }
            Shuffle(fits);
            for (int i = 0; i < fits.Count && made.Count < options; i++)
            {
                var (_, craft, giver) = fits[i];
                if (made.Exists(r => r.GiverId == giver.Id)) continue;
                made.Add(PostOrder(giver, craft, false, false));
            }
            if (stretch.Count > 0 && made.Count < options + 1)
            {
                var (_, craft, giver) = stretch[_rng.Next(stretch.Count)];
                made.Add(PostOrder(giver, craft, true, false));
            }
            // Usta siparişi: hanede Sv 10 bir beceri varsa, açık bir usta siparişi yoksa; o becerinin en değerli tarifi.
            bool masterOpen = false;
            foreach (var r in _requests) if (r.IsOpen && r.Master) masterOpen = true;
            if (!masterOpen)
            {
                var best = new Dictionary<SkillType, InteractionDef>();
                foreach (var (station, craft) in Recipes())
                {
                    var skill = craft.Skill.Value;
                    if (Best(skill) < MasterLevel || GiverForRole(RoleForSkill(skill)) == null) continue;
                    int price = _content.Items.Get(craft.Produce[0].ItemId).Price;
                    if (!best.TryGetValue(skill, out var cur) || price > _content.Items.Get(cur.Produce[0].ItemId).Price) best[skill] = craft;
                }
                if (best.Count > 0)
                {
                    var list = new List<InteractionDef>(best.Values);
                    var craft = list[_rng.Next(list.Count)];
                    made.Add(PostOrder(GiverForRole(RoleForSkill(craft.Skill.Value)), craft, false, true));
                }
            }
            return made;
        }

        public Request PostOrder(Character giver, InteractionDef craft, bool stretch, bool master)
        {
            var pid = craft.Produce[0].ItemId;
            var prod = _content.Items.Get(pid);
            int n = Math.Max(1, Math.Min(4, (int)Math.Ceiling(24.0 / Math.Max(1, prod.Price))));
            if (master) n = Math.Min(6, n * 2);
            string title = master ? $"Usta siparişi: {prod.DisplayName}" : prod.DisplayName;
            var r = NewRequest(RequestKind.Order, "order:" + pid, title,
                $"{giver.Name}: \"{n} {prod.DisplayName.ToLowerInvariant()} lazım; {(master ? "usta elinden çıkmış olsun." : "zamanında getirirsen iyi öderim.")}\"", giver);
            r.Days = OrderDays;
            r.Deadline = Now() + OrderDays;
            r.Skill = craft.Skill;
            r.Level = Math.Max(1, craft.MinSkillLevel);
            r.Stretch = stretch;
            r.Master = master;
            r.Penalty = FailedOrderRep;
            r.Objectives.Add(RequestCatalog.Del(pid, n));
            double factor = master ? MasterAkceFactor : OrderAkceFactor;
            r.Reward = new Reward
            {
                Akce = Math.Max(4, Math.Min(master ? OrderAkceMax * 2 : OrderAkceMax, (int)Math.Round(prod.Price * n * factor))),
                Affinity = master ? MasterAffinity : OrderAffinity,
                Reputation = master ? MasterRep : OrderRep,
                Xp = 8 * n * (master ? 1.5 : 1.25),
                XpSkill = craft.Skill,
                Memory = HouseMemory.Order
            };
            return Post(r);
        }

        // ================================================================== üretim: çağrılar

        public Request GenerateCall(string templateId = null)
        {
            if (CountOpen(RequestKind.Call, RequestState.Posted) + CountOpen(RequestKind.Call, RequestState.Accepted) > 0 && templateId == null) return null;
            var season = SeasonNow;
            var options = new List<(CallTemplate, Character)>();
            var villagers = LivingVillagers();
            foreach (var t in RequestCatalog.Calls)
            {
                if (templateId != null && t.Id != templateId) continue;
                if (!InSeason(t.Seasons, season)) continue;
                Character giver = t.Role == "*" ? (villagers.Count > 0 ? villagers[_rng.Next(villagers.Count)] : null) : GiverForRole(t.Role);
                if (giver != null) options.Add((t, giver));
            }
            if (options.Count == 0) return null;
            var (tpl, g) = options[_rng.Next(options.Count)];
            var r = NewRequest(RequestKind.Call, tpl.Id, tpl.Title, tpl.Text, g);
            r.Days = CallDays;
            r.Deadline = Now() + CallDays;
            r.IgnoreText = tpl.IgnoreText;
            foreach (var p in tpl.Paths) r.Paths.Add(p);
            r.Reward = new Reward { Affinity = CallAffinity, Reputation = tpl.Hard ? HardCallRep : CallRep, Memory = HouseMemory.Call };
            return Post(r);
        }

        /// <summary>Yol hanede açık mı (beceri, eşya, akçe)? Açık değilse nedeni.</summary>
        public string PathBlocker(Request r, int index, Character solver)
        {
            if (r == null || index < 0 || index >= r.Paths.Count) return "Yol yok.";
            var p = r.Paths[index];
            if (p.Skill.HasValue)
            {
                if (solver == null) return "Kimin gideceğini seç.";
                if (solver.Skills.GetLevel(p.Skill.Value) < p.Level) return $"{SkillNames.Name(p.Skill.Value)} {p.Level} gerekli ({solver.Name}: {solver.Skills.GetLevel(p.Skill.Value)}).";
            }
            if (p.ItemId != null && _house.Count(p.ItemId) < p.ItemCount) return $"{p.ItemCount} {ItemName(p.ItemId)} gerekli.";
            if (p.Akce > 0 && _house.Akce < p.Akce) return $"{p.Akce} akçe gerekli.";
            if (p.Hours > 0 && solver != null && Duties != null)
            {
                var why = Duties.CanStart(solver, ErrandOrder(r, p, solver));
                if (why != null) return why;
            }
            return null;
        }

        private static TimedDutyOrder ErrandOrder(Request r, CallPath p, Character solver) =>
            new TimedDutyOrder { CharacterId = solver.Id, Kind = TimedDutyKind.Errand, PlaceId = "errand:village", Hours = p.Hours, RequestId = r.Id, Label = r.Title };

        /// <summary>Çağrıyı bir yolla üstlenir: eşya ve akçe hemen gider; süreli yolda çözen köy işine çıkar, dönünce biter.</summary>
        public bool ChoosePath(int requestId, int index, Character solver, out string message)
        {
            var r = Get(requestId);
            message = PathBlocker(r, index, solver);
            if (message != null) return false;
            if (!r.IsOpen || r.Kind != RequestKind.Call) { message = "Çağrı kapandı."; return false; }
            var p = r.Paths[index];
            if (p.ItemId != null) for (int i = 0; i < p.ItemCount; i++) _house.TryTakeItem(p.ItemId);
            if (p.Akce > 0 && !_house.TrySpend(p.Akce)) { message = "Akçe yetmedi."; return false; }
            r.ChosenPath = index;
            r.SolverId = solver?.Id ?? 0;
            r.State = RequestState.Accepted;
            r.AcceptedDay = Now();
            if (p.Hours > 0 && solver != null && Duties != null)
            {
                r.Deadline = double.MaxValue;   // çözen yolda; süre dolmaz
                var d = Duties.Start(ErrandOrder(r, p, solver), out var why);
                if (d == null) { message = why; r.ChosenPath = -1; r.State = RequestState.Posted; return false; }
                message = $"{solver.Name} {p.Hours:0} saatliğine yola çıktı: {p.Label}.";
                return true;
            }
            CompleteCall(r, solver);
            message = p.Outcome;
            return true;
        }

        private void CompleteCall(Request r, Character solver)
        {
            var p = r.ChosenPath >= 0 ? r.Paths[r.ChosenPath] : null;
            var reward = r.Reward.Clone();
            if (p != null && p.Paid) { reward.Reputation /= 2; reward.Affinity /= 2; }
            if (p != null && p.Skill.HasValue && solver != null) { reward.Xp = 15; reward.XpSkill = p.Skill; }
            if (p != null && !string.IsNullOrEmpty(p.Outcome)) Notify?.Invoke($"{r.Title}: {p.Outcome}", r.GiverId);
            Complete(r, solver, reward);
        }

        // ================================================================== mevsim defteri

        private void UpdateSeason()
        {
            long idx = SeasonIndex;
            if (idx == _seasonPosted) return;
            if (_seasonPosted >= 0) CloseSeason((Season)(int)(_seasonPosted % TimeConfig.SeasonsPerYear));
            _seasonPosted = idx;
            PostSeasonBook(SeasonNow, idx * DaysPerSeason);
        }

        public List<Request> PostSeasonBook(Season season, double seasonStartDay)
        {
            var list = new List<Request>();
            double scale = DaysPerSeason / 10.0;
            foreach (var t in RequestCatalog.Seasonal)
            {
                if (t.Season != season) continue;
                if (t.Objective.ItemId != null && !_content.Items.Contains(t.Objective.ItemId)) continue;
                var r = NewRequest(RequestKind.Seasonal, t.Id, t.Title, t.Text, null);
                r.GiverName = "Köy";
                r.Season = (int)season;
                r.DayFrom = Math.Max(1, (int)Math.Ceiling(t.From * scale));
                r.DayTo = Math.Max(r.DayFrom, (int)Math.Ceiling(t.To * scale));
                r.Deadline = seasonStartDay + r.DayTo;
                r.State = RequestState.Accepted;
                r.AcceptedDay = Now();
                r.Read = true;
                r.Objectives.Add(t.Objective.Clone());
                int value = t.Objective.ItemId != null ? _content.Items.Get(t.Objective.ItemId).Price * t.Objective.Count : 10;
                r.Reward = new Reward { Akce = Math.Max(2, Math.Min(15, value / 2)), Reputation = SeasonJobRep };
                list.Add(Post(r, announce: false));
            }
            if (list.Count > 0) Notify?.Invoke($"Mevsim Defteri açıldı: {list.Count} iş (İstekler → Mevsim).", 0);
            return list;
        }

        public bool InWindow(Request r) => r.Kind != RequestKind.Seasonal || (DayOfSeason >= r.DayFrom && DayOfSeason <= r.DayTo);

        /// <summary>Mevsim biter: hane en az 5 iş bitirdiyse mevsim takımından bir parça, itibar ve "Bereketli mevsim".</summary>
        public int CloseSeason(Season season)
        {
            int done = 0;
            foreach (var r in _requests)
            {
                if (r.Kind != RequestKind.Seasonal || r.Season != (int)season) continue;
                if (r.State == RequestState.Done) done++;
            }
            foreach (var r in _requests) if (r.Kind == RequestKind.Seasonal && r.IsOpen && r.Season == (int)season) r.State = RequestState.Expired;
            if (done < SeasonSetJobs) return done;
            string piece = null;
            foreach (var id in RequestCatalog.SeasonSet(season))
            {
                if (!_content.Items.Contains(id) || _setPieces.Contains(id)) continue;
                piece = id;
                break;
            }
            if (piece == null)
            {
                foreach (var id in RequestCatalog.SeasonSet(season)) if (_content.Items.Contains(id)) { piece = id; break; }
            }
            if (piece != null) { _house.AddItem(piece); _setPieces.Add(piece); }
            AddReputation?.Invoke(SeasonSetRep, "Mevsim Defteri");
            foreach (var m in Household()) if (m.MoodEnabled) m.AddMoodlet(BountifulMood, "Bereketli mevsim", 4, 2.0);
            Notify?.Invoke($"Mevsim Defteri: {done} iş bitti. {(piece != null ? $"Mevsim takımından {ItemName(piece)} kazanıldı. " : "")}Hane bereketli bir mevsim geçirdi.", 0);
            Chronicle?.Invoke($"Mevsim Defteri: {done} iş");
            return done;
        }

        // ================================================================== hikâyeler

        private void UpdateStories()
        {
            if (_villagers == null || Family == null) return;
            foreach (var st in _villagers.States)
            {
                var v = _population.Find(st.CharacterId);
                if (v == null || v.IsDead || !_content.Villagers.TryGet(st.DefId, out var def)) continue;
                bool original = _originals.TryGetValue(st.DefId, out int first) && first == v.Id;
                StoryStep[] steps;
                string key;
                if (original && RequestCatalog.NamedStories.TryGetValue(st.DefId, out steps)) key = st.DefId;
                else if (RequestCatalog.RoleStories.TryGetValue(def.Role ?? "", out steps)) key = $"{st.DefId}:{v.Id}";
                else continue;
                TryPostStory(v, key, steps);
            }
            // Kişilik hikâyesi: adsız köylüler, aynı anda en çok biri.
            foreach (var r in _requests) if (r.IsOpen && r.Kind == RequestKind.Story && r.StoryKey != null && r.StoryKey.StartsWith("trait:")) return;
            foreach (var v in LivingVillagers())
            {
                if (_villagers.IsNamed(v)) continue;
                foreach (var tid in v.TraitIds)
                {
                    if (!RequestCatalog.TraitStories.TryGetValue(tid, out var steps)) continue;
                    if (TryPostStory(v, $"trait:{v.Id}", steps) != null) return;
                }
            }
        }

        private int BestAffection(Character v)
        {
            if (Family == null) return 0;
            int best = 0;
            foreach (var m in Household()) best = Math.Max(best, Family.Affection(m.Id, v.Id));
            return best;
        }

        public int StoryStepOf(string key) => _storySteps.TryGetValue(key, out int n) ? n : 0;

        private Request TryPostStory(Character v, string key, StoryStep[] steps)
        {
            int step = StoryStepOf(key);
            if (step >= steps.Length || step >= StoryThresholds.Length) return null;
            foreach (var r in _requests) if (r.IsOpen && r.StoryKey == key) return null;
            if (BestAffection(v) < StoryThresholds[step]) return null;
            var s = steps[step];
            var req = NewRequest(RequestKind.Story, key + "#" + step, s.Title, s.Text, v);
            req.StoryKey = key;
            req.StoryStep = step;
            req.State = RequestState.Accepted;
            req.AcceptedDay = Now();
            req.Read = true;
            var o = s.Objective.Clone();
            if (o.Kind == ObjectiveKind.Talk && o.VillagerId == 0) o.VillagerId = v.Id;
            req.Objectives.Add(o);
            req.Reward = s.Reward.Clone();
            Post(req, announce: false);
            Notify?.Invoke($"Hikâye ({v.Name}, {step + 1}/{steps.Length}): {s.Title}.", v.Id);
            return req;
        }

        // ================================================================== vasiyet ve adak

        /// <summary>
        /// Ölüm kancası (belge 46 §8.1, §8.3). Köylü öldüyse ricaları düşer, siparişi varise kalır; oyuncuyla yakınlığı 60+ ya da
        /// açık hikâyesi varsa vasiyet bırakır, hanesine cenaze helvası ricası düşer. Hane yaşlısı öldüyse bir adak tohumu.
        /// </summary>
        public Request OnDied(Character c)
        {
            if (c == null) return null;
            foreach (var r in _requests)
                if (r.IsOpen && r.AssignedTo == c.Id) r.AssignedTo = 0;
            if (!c.IsVillager) return OnHouseholdDied(c);

            bool story = false;
            foreach (var r in _requests)
            {
                if (!r.IsOpen || r.GiverId != c.Id) continue;
                if (r.Kind == RequestKind.Favor || r.Kind == RequestKind.Call) r.State = RequestState.Dropped;
                if (r.Kind == RequestKind.Story) { r.State = RequestState.Dropped; story = true; }
            }
            // Cenaze helvası: hanesinden yaşayan biri ister.
            var house = _houses?.HouseOf(c);
            if (house != null)
                foreach (var m in _houses.LivingMembers(house))
                    if (m != c && m.Stage >= AgeStage.Teen)
                    {
                        PostSystemicFavor(m, "rica.funeral", "Cenaze helvası", $"{m.Name}: \"{c.Name} için helva kavuracağız; bir tepsi getirir misin?\"", "item.helva", 1, 2);
                        break;
                    }
            if (!story && BestAffection(c) < BequestAffection) return null;
            string role = RoleOf(c);
            var options = new List<BequestTemplate>();
            foreach (var t in RequestCatalog.Bequests) if (t.Role == role) options.Add(t);
            if (options.Count == 0) foreach (var t in RequestCatalog.Bequests) if (t.Role == "*") options.Add(t);
            if (options.Count == 0) return null;
            var tpl = options[_rng.Next(options.Count)];
            var req = NewRequest(RequestKind.Bequest, tpl.Id, tpl.Title, tpl.Text, c);
            req.State = RequestState.Accepted;
            req.AcceptedDay = Now();
            req.Read = true;
            req.Objectives.Add(tpl.Objective.Clone());
            req.Reward = new Reward { Affinity = 0, Memory = HouseMemory.Bequest, KeepsakeItem = "item.elder_tin_cup", KeepsakeLabel = $"{c.Name}'in vasiyeti", Reputation = 3 };
            if (!_content.Items.Contains(req.Reward.KeepsakeItem)) req.Reward.KeepsakeItem = null;
            Post(req, announce: false);
            Notify?.Invoke($"{c.Name} bir vasiyet bıraktı: \"{tpl.Title}\" (İstekler → Hikâyeler).", c.Id);
            Chronicle?.Invoke($"{c.Name}'in vasiyeti: {tpl.Title}");
            return req;
        }

        private Request OnHouseholdDied(Character c)
        {
            if (c.Stage != AgeStage.Elder) return null;
            int open = 0;
            foreach (var r in _requests) if (r.Kind == RequestKind.Vow && r.IsOpen) open++;
            if (open > 0) return null;
            var options = new List<VowTemplate>();
            foreach (var t in RequestCatalog.Vows) if (!_vowsDone.Contains(t.Id)) options.Add(t);
            if (options.Count == 0) return null;
            var tpl = options[_rng.Next(options.Count)];
            var r2 = MakeVow(tpl, null);
            r2.State = RequestState.Posted;
            r2.Read = true;
            r2.VowedByName = c.Name;
            r2.Text = $"{c.Name} ölmeden önce bunu dilemişti: {tpl.Text}";
            Post(r2, announce: false);
            Notify?.Invoke($"{c.Name}'in son dileği bir adak olabilir: \"{tpl.Title}\" (İstekler → Adaklar).", c.Id);
            return r2;
        }

        /// <summary>Adanabilecek adaklar (bitmemiş ve açık olmayan).</summary>
        public IEnumerable<VowTemplate> AvailableVows()
        {
            foreach (var t in RequestCatalog.Vows)
            {
                if (_vowsDone.Contains(t.Id)) continue;
                bool open = false;
                foreach (var r in _requests) if (r.IsOpen && r.TemplateId == t.Id) open = true;
                if (!open) yield return t;
            }
        }

        public string CanVow(string templateId, Character vower)
        {
            if (vower == null || vower.IsDead || vower.IsVillager) return "Hane üyesi seç.";
            if (vower.Stage < AgeStage.YoungAdult) return "Adağı yetişkin biri adar.";
            int open = 0;
            foreach (var r in _requests) if (r.Kind == RequestKind.Vow && r.State == RequestState.Accepted) open++;
            if (open >= MaxVows) return $"Aynı anda en çok {MaxVows} adak.";
            if (_vowsDone.Contains(templateId)) return "Bu adak yerine getirildi.";
            foreach (var r in _requests) if (r.IsOpen && r.TemplateId == templateId && r.State == RequestState.Accepted) return "Bu adak zaten adandı.";
            return null;
        }

        /// <summary>Hane reisi (ya da yetişkin biri) adak adar. Yaşlının son dileği olarak askıda duran adak da böyle kabul edilir.</summary>
        public Request Vow(string templateId, Character vower, out string message)
        {
            message = CanVow(templateId, vower);
            if (message != null) return null;
            Request r = null;
            foreach (var x in _requests) if (x.IsOpen && x.TemplateId == templateId && x.Kind == RequestKind.Vow) r = x;
            if (r == null)
            {
                VowTemplate tpl = null;
                foreach (var t in RequestCatalog.Vows) if (t.Id == templateId) tpl = t;
                if (tpl == null) { message = "Adak bulunamadı."; return null; }
                r = Post(MakeVow(tpl, vower), announce: false);
            }
            r.State = RequestState.Accepted;
            r.AcceptedDay = Now();
            r.VowedBy = vower.Id;
            if (string.IsNullOrEmpty(r.VowedByName)) r.VowedByName = vower.Name;
            r.VowedDay = Now();
            string note = $"{vower.Name} adak adadı: {r.Title}";
            Chronicle?.Invoke(note);
            vower.AddChronicle(SavedText.Encode("core.chronicle.vow_made", ("name", vower.Name), ("title", r.Title)));
            message = note + ".";
            return r;
        }

        private Request MakeVow(VowTemplate tpl, Character vower)
        {
            var r = NewRequest(RequestKind.Vow, tpl.Id, tpl.Title, tpl.Text, null);
            r.GiverName = vower?.Name ?? "Hane";
            r.Read = true;
            foreach (var st in tpl.Stages)
            {
                var copy = new VowStage { Title = st.Title, Reward = st.Reward.Clone() };
                foreach (var o in st.Objectives) copy.Objectives.Add(o.Clone());
                r.Stages.Add(copy);
            }
            r.Reward = new Reward { Seals = tpl.FinalSeals, Reputation = tpl.FinalReputation };
            return r;
        }

        // ================================================================== oyuncu eylemleri

        /// <summary>Panodaki ilanları okur (okuryazar hane uzaktan; değilse panonun önünde "İlanları oku").</summary>
        public int ReadBoard()
        {
            int n = 0;
            foreach (var r in _requests)
                if (r.State == RequestState.Posted && !r.Read) { r.Read = true; n++; }
            return n;
        }

        public string CanAccept(Request r)
        {
            if (r == null || r.State != RequestState.Posted) return "İlan kalktı.";
            if (!r.Read) return "Önce panodaki ilanı oku (meydandaki ilan tahtası).";
            if (r.Kind == RequestKind.Favor)
            {
                int n = 0;
                foreach (var x in _requests) if (x.Kind == RequestKind.Favor && x.State == RequestState.Accepted) n++;
                if (n >= MaxFavorsAccepted) return $"En çok {MaxFavorsAccepted} rica birden üstlenilir.";
            }
            if (r.Kind == RequestKind.Order)
            {
                int n = 0;
                foreach (var x in _requests) if (x.Kind == RequestKind.Order && x.State == RequestState.Accepted) n++;
                if (n >= MaxOrders) return $"En çok {MaxOrders} sipariş birden (Kâhyalık {StewardOrderLevel}'le +1).";
            }
            if (r.Kind == RequestKind.Call) return "Çağrıda bir yol seç.";
            if (r.Kind == RequestKind.Vow) return "Adağı bir yetişkin adar.";
            return null;
        }

        public bool Accept(int id, out string message)
        {
            var r = Get(id);
            message = CanAccept(r);
            if (message != null) return false;
            r.State = RequestState.Accepted;
            r.AcceptedDay = Now();
            if (r.Days > 0) r.Deadline = Now() + r.Days;
            message = $"Üstlenildi: {r.Title} ({TimeLeft(r)}).";
            return true;
        }

        /// <summary>Bırak: rica, mevsim işi ve hikâye cezasız; kabul edilmiş sipariş itibar −2; adak kronikte "yarım kaldı".</summary>
        public bool Drop(int id, out string message)
        {
            var r = Get(id);
            message = null;
            if (r == null || !r.IsOpen) { message = "İstek yok."; return false; }
            if (r.Kind == RequestKind.Order && r.State == RequestState.Accepted)
            {
                r.State = RequestState.Failed;
                AddReputation?.Invoke(FailedOrderRep, $"sipariş bırakıldı: {r.Title}");
                var giver = _population.Find(r.GiverId);
                if (giver != null) Memory.Add(giver, HouseMemory.FailedOrder, $"{r.Title} bırakıldı");
                message = $"Sipariş bırakıldı; itibar {FailedOrderRep}.";
                return true;
            }
            if (r.Kind == RequestKind.Vow)
            {
                string note = $"Adak yarım kaldı: {r.Title} ({r.Stage}/{r.Stages.Count} aşama)";
                Chronicle?.Invoke(note);
                r.State = RequestState.Dropped;
                message = note + ".";
                return true;
            }
            r.State = RequestState.Dropped;
            message = $"Bırakıldı: {r.Title}.";
            return true;
        }

        /// <summary>
        /// Teslim et: elde olan eşyalar ve akçe hedeflere gider (kısmi teslim olur). Verenin yanında olunmalı; ulakla 2 akçeye
        /// uzaktan gönderilir (belge 46 §10). Vasiyet ve adakta yanında olmak gerekmez.
        /// </summary>
        public bool Deliver(int id, Character by, bool courier, out string message)
        {
            var r = Get(id);
            message = null;
            if (r == null || !r.IsOpen) { message = "İstek yok."; return false; }
            if (!InWindow(r)) { message = $"Mevsimin {r.DayFrom}-{r.DayTo}. günleri arasında."; return false; }
            if (r.State == RequestState.Posted)
            {
                if (!Accept(id, out message)) return false;
            }
            var giver = _population.Find(r.GiverId);
            bool needPresence = giver != null && !giver.IsDead && r.Kind != RequestKind.Vow && r.Kind != RequestKind.Bequest && r.Kind != RequestKind.Seasonal;
            if (needPresence && !courier && by != null && Near != null && !Near(by, giver))
            {
                message = $"{giver.Name}'in yanına git ya da ulakla gönder ({CourierFee} akçe).";
                return false;
            }
            bool any = false;
            foreach (var o in r.CurrentObjectives)
            {
                if (o.Done) continue;
                if (o.Kind == ObjectiveKind.Deliver)
                {
                    int give = Math.Min(o.Left, _house.Count(o.ItemId));
                    for (int i = 0; i < give; i++) if (_house.TryTakeItem(o.ItemId)) { o.Progress++; any = true; }
                }
                else if (o.Kind == ObjectiveKind.Pay && _house.Akce >= o.Left)
                {
                    if (_house.TrySpend(o.Left)) { o.Progress = o.Count; any = true; }
                }
            }
            if (!any) { message = "Teslim edecek bir şey yok."; return false; }
            if (needPresence && courier && !_house.TrySpend(CourierFee)) { message = $"Ulak için {CourierFee} akçe gerekli."; return false; }
            if (!CheckComplete(r, by)) message = "Kısmen teslim edildi.";
            else message = r.State == RequestState.Done ? $"Teslim edildi: {r.Title}." : $"Aşama tamam: {r.Title}.";
            return true;
        }

        /// <summary>
        /// "Ata" (belge 46 §5.4): istek aile üyesine geçici görev olur. Eşya teslimi süreli göreve (madene, balığa, ormana...)
        /// hedef eşya ile çevrilir; görev dönünce eşya elde yeterliyse istek kendiliğinden teslim edilir. Uygun yer yoksa kişi
        /// yalnızca işaretlenir (elle yapılır).
        /// </summary>
        public bool Assign(int id, Character c, out string message)
        {
            var r = Get(id);
            message = null;
            if (r == null || !r.IsOpen) { message = "İstek yok."; return false; }
            if (c == null || c.IsDead || c.IsVillager) { message = "Hane üyesi seç."; return false; }
            if (r.Kind == RequestKind.Order && c.Stage <= AgeStage.Child) { message = "Çocuk siparişe atanamaz."; return false; }
            if (r.Kind == RequestKind.Call) { message = "Çağrıda bir yol seç."; return false; }
            if (r.State == RequestState.Posted && !Accept(id, out message)) return false;
            message = null;
            r.AssignedTo = c.Id;
            if (Duties != null)
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Done) continue;
                    var order = DutyFor(r, o, c);
                    if (order == null) continue;
                    var d = Duties.Start(order, out var why);
                    if (d != null)
                    {
                        var place = TimedDutyService.Place(order.PlaceId);
                        message = $"{c.Name} {place?.Name.ToLowerInvariant()} görevine çıktı ({r.Title}).";
                        return true;
                    }
                    message = why;
                }
            message = message != null ? $"{c.Name} atandı; göreve çıkamadı: {message}" : $"{c.Name} bu işe atandı (elle yapılır).";
            return true;
        }

        /// <summary>Hedef için uygun süreli görev; yoksa null.</summary>
        public TimedDutyOrder DutyFor(Request r, Objective o, Character c)
        {
            if (Duties == null || c == null) return null;
            foreach (var place in TimedDutyService.Places)
            {
                if (place.Kind == TimedDutyKind.Errand) continue;
                var order = new TimedDutyOrder { CharacterId = c.Id, Kind = place.Kind, PlaceId = place.Id, Hours = 0, RequestId = r.Id, Label = r.Title };
                if (o.Kind == ObjectiveKind.Deliver)
                {
                    bool yields = false;
                    foreach (var y in Duties.YieldTable(c, place)) if (y.item == o.ItemId && !y.rare) yields = true;
                    if (!yields) continue;
                    order.TargetItem = o.ItemId;
                    order.TargetCount = Math.Max(1, o.Left - _house.Count(o.ItemId));
                }
                else if (o.Kind == ObjectiveKind.Act && o.Skill.HasValue && place.Skill == o.Skill.Value && place.BaseRate > 0)
                    order.Hours = Math.Min(8, Math.Max(2, o.Left * 2));
                else continue;
                if (order.Hours <= 0)
                {
                    double left = Duties.HoursFor(order);   // akşama kadar; yaş tavanını aşmasın
                    if (left >= 0.5) order.Hours = Math.Min(left, TimedDutyService.MaxHours(c, place.Kind));
                }
                if (Duties.CanStart(c, order) == null) return order;
            }
            return null;
        }

        // ================================================================== ilerleme olayları

        /// <summary>Bir iş bitti (JobRunner.WorkDone): "N kez yap" hedefleri ilerler.</summary>
        public void OnWork(Character c, SkillType? skill, string actId)
        {
            if (c == null || c.IsVillager) return;
            foreach (var r in _requests.ToArray())
            {
                if (r.State != RequestState.Accepted || !InWindow(r)) continue;
                bool moved = false;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Kind != ObjectiveKind.Act || o.Done) continue;
                    bool match = o.ActId != null ? o.ActId == actId : (o.Skill.HasValue && skill == o.Skill);
                    if (!match) continue;
                    o.Progress++;
                    moved = true;
                }
                if (moved) CheckComplete(r, c);
            }
        }

        /// <summary>Sohbet edildi: "Konuş" hedefleri; konuşulan köylünün askıdaki ricası okunmuş olur.</summary>
        public void OnTalk(Character actor, Character target)
        {
            if (actor == null || target == null || actor.IsVillager) return;
            foreach (var r in _requests.ToArray())
            {
                if (r.State == RequestState.Posted && r.GiverId == target.Id && r.Kind == RequestKind.Favor) r.Read = true;
                if (r.State != RequestState.Accepted) continue;
                bool moved = false;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Kind != ObjectiveKind.Talk || o.Done) continue;
                    int who = o.VillagerId != 0 ? o.VillagerId : r.GiverId;
                    if (who != target.Id) continue;
                    o.Progress = o.Count;
                    moved = true;
                }
                if (moved) CheckComplete(r, actor);
            }
        }

        /// <summary>Hane üyesi bir bölgeye girdi: "git ve bul" hedefleri.</summary>
        public void OnVisit(Character c, string regionId)
        {
            if (c == null || c.IsVillager || regionId == null) return;
            foreach (var r in _requests.ToArray())
            {
                if (r.State != RequestState.Accepted) continue;
                bool moved = false;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Kind != ObjectiveKind.Visit || o.Done || !string.Equals(o.Region, regionId, StringComparison.OrdinalIgnoreCase)) continue;
                    o.Progress = o.Count;
                    moved = true;
                }
                if (moved)
                {
                    if (r.TemplateId == "rica.shepherd.lamb") Notify?.Invoke($"{c.Name} kaybolan kuzuyu buldu; {r.GiverName}'a götürmeli.", c.Id);
                    CheckComplete(r, c);
                }
            }
        }

        /// <summary>Süreli görev döndü: çağrı yolu biter; "Ata" ile verilen istek elde yeterliyse teslim edilir; "çalış" hedefleri saatle ilerler.</summary>
        public void OnDutyFinished(TimedDutyResult res)
        {
            if (res == null) return;
            var c = _population.Find(res.CharacterId);
            var r = res.RequestId != 0 ? Get(res.RequestId) : null;
            if (r != null && r.IsOpen && r.Kind == RequestKind.Call && r.ChosenPath >= 0)
            {
                var p = r.Paths[r.ChosenPath];
                if (res.Hours >= p.Hours * 0.5 - 1e-9 && res.End != TimedDutyEnd.Died) CompleteCall(r, c);
                else
                {
                    r.ChosenPath = -1;
                    r.State = RequestState.Posted;
                    r.Deadline = Now() + 0.25;
                    Notify?.Invoke($"{res.CharacterName} işi yarım bırakıp döndü; \"{r.Title}\" hâlâ bekliyor.", res.CharacterId);
                }
                return;
            }
            // Görev saatleri "çalış" hedeflerine sayılır (2 saat ≈ 1 iş).
            var place = TimedDutyService.Place(res.Kind == TimedDutyKind.Errand ? null : FindPlaceId(res));
            if (place != null && c != null)
            {
                int acts = Math.Max(1, (int)Math.Round(res.Hours / 2.0));
                for (int i = 0; i < acts; i++) OnWork(c, place.Skill, "duty:" + place.Id);
            }
            foreach (var x in _requests.ToArray())
            {
                if (x.State != RequestState.Accepted) continue;
                bool moved = false;
                foreach (var o in x.CurrentObjectives)
                    if (o.Kind == ObjectiveKind.Duty && !o.Done && o.DutyKind == res.Kind) { o.Progress += (int)Math.Round(res.Hours); moved = true; }
                if (moved) CheckComplete(x, c);
            }
            if (r != null && r.IsOpen)
            {
                r.AssignedTo = 0;
                bool enough = true;
                foreach (var o in r.CurrentObjectives) if (o.Kind == ObjectiveKind.Deliver && !o.Done && _house.Count(o.ItemId) < o.Left) enough = false;
                if (enough && Deliver(r.Id, c, courier: true, out var msg)) Notify?.Invoke($"{res.CharacterName} döndü; {r.Title}: {msg}", res.CharacterId);
                else if (!enough) Notify?.Invoke($"{res.CharacterName} döndü ama \"{r.Title}\" için yetmedi.", res.CharacterId);
            }
        }

        private static string FindPlaceId(TimedDutyResult res)
        {
            foreach (var p in TimedDutyService.Places) if (p.Kind == res.Kind && p.Name == res.PlaceName) return p.Id;
            return null;
        }

        /// <summary>Komşu hanede bebek doğdu: beşik ya da battaniye ricası; hane hafızası yeni kuşağa %25 başlangıç yakınlığı verir.</summary>
        public void OnBabyBorn(Character baby, Character mother)
        {
            if (baby == null || Family == null) return;
            if (mother != null && mother.IsVillager)
            {
                int key = Memory.KeyOf(mother);
                int init = Memory.InitialAffection(key);
                if (init > 0) foreach (var m in Household()) Family.SetAffection(m.Id, baby.Id, Math.Max(Family.Affection(m.Id, baby.Id), init));
                bool cradle = _rng.NextDouble() < 0.5;
                PostSystemicFavor(mother, cradle ? "rica.cradle" : "rica.blanket", cradle ? "Beşik" : "Bebek battaniyesi",
                    $"{mother.Name}: \"Bebek geldi ama {(cradle ? "beşiğimiz yok" : "battaniye yetmiyor")}; yardım eder misin?\"",
                    cradle ? "item.cradle" : "item.blanket", 1, 4);
                return;
            }
            // Oyuncunun yeni kuşağı: hafızası olan hanelerin yaşayan üyeleri başlangıç yakınlığıyla başlar.
            if (_houses == null) return;
            foreach (var h in _houses.Houses)
            {
                int init = Memory.InitialAffection(h.Id);
                if (init <= 0) continue;
                foreach (var v in _houses.LivingMembers(h)) Family.SetAffection(v.Id, baby.Id, Math.Max(Family.Affection(v.Id, baby.Id), init));
            }
        }

        /// <summary>Köyde düğün kararlaştı: düğün ekmeği siparişi (34-D2).</summary>
        public Request OnVillageWedding(Character a, Character b)
        {
            var giver = a != null && !a.IsDead ? a : b;
            if (giver == null) return null;
            var r = NewRequest(RequestKind.Order, "order.wedding:" + giver.Id, "Düğün sofrası", $"{giver.Name}: \"Düğünümüz var; sofraya ekmek ve mum lazım.\"", giver);
            r.Days = OrderDays;
            r.Deadline = Now() + OrderDays;
            r.Penalty = FailedOrderRep;
            r.Skill = SkillType.Baking;
            r.Level = 1;
            r.Objectives.Add(RequestCatalog.Del("item.bread", 6));
            if (_content.Items.Contains("item.tallow_candle")) r.Objectives.Add(RequestCatalog.Del("item.tallow_candle", 2));
            r.Reward = new Reward { Akce = 30, Affinity = OrderAffinity, Reputation = 4, Memory = HouseMemory.Order };
            return Post(r);
        }

        private void RefreshSkills()
        {
            foreach (var r in _requests.ToArray())
            {
                if (r.State != RequestState.Accepted) continue;
                bool moved = false;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Kind != ObjectiveKind.Skill || o.Done || !o.Skill.HasValue) continue;
                    if (Best(o.Skill.Value) >= o.Level) { o.Progress = o.Count; moved = true; }
                }
                if (moved) CheckComplete(r, null);
            }
        }

        // ================================================================== bitirme ve ödül

        /// <summary>Hedefler tamamsa isteği (adakta aşamayı) bitirir. Bitti mi?</summary>
        public bool CheckComplete(Request r, Character by)
        {
            if (r == null || !r.IsOpen || !r.AllDone) return false;
            if (r.Kind == RequestKind.Vow)
            {
                var stage = r.Stages[r.Stage];
                Apply(stage.Reward, r, by);
                string who = by?.Name ?? HeadName();
                Chronicle?.Invoke($"Adak \"{r.Title}\": {stage.Title} ({who})");
                r.Stage++;
                if (r.Stage < r.Stages.Count)
                {
                    Notify?.Invoke($"Adak \"{r.Title}\": {stage.Title} tamam. Sıradaki: {r.Stages[r.Stage].Title}.", 0);
                    return true;
                }
                _vowsDone.Add(r.TemplateId);
                string note = r.VowedByName != null && who != r.VowedByName
                    ? $"{r.Title}: {r.VowedByName} başlattı, {who} bitirdi."
                    : $"{r.Title}: adak yerine geldi.";
                Chronicle?.Invoke(note);
                string savedNote = r.VowedByName != null && who != r.VowedByName
                    ? SavedText.Encode("core.chronicle.vow_finished_by", ("title", r.Title), ("starter", r.VowedByName), ("finisher", who))
                    : SavedText.Encode("core.chronicle.vow_done", ("title", r.Title));
                foreach (var m in Household()) m.AddChronicle(savedNote);
            }
            Complete(r, by, r.Reward);
            return true;
        }

        private string HeadName()
        {
            foreach (var m in Household()) if (m.Stage >= AgeStage.YoungAdult) return m.Name;
            return "hane";
        }

        private void Complete(Request r, Character by, Reward reward)
        {
            r.State = RequestState.Done;
            r.AssignedTo = 0;
            Apply(reward, r, by);
            if (r.Kind == RequestKind.Story && r.StoryKey != null) _storySteps[r.StoryKey] = Math.Max(StoryStepOf(r.StoryKey), r.StoryStep + 1);
            if (r.Kind == RequestKind.Order) PlayerScore += reward.Reputation;
            string text = r.Kind == RequestKind.Vow ? $"Adak yerine geldi: {r.Title}! {RewardText(reward)}." : $"{Request.KindName(r.Kind)} bitti: {r.Title}. {RewardText(reward)}.";
            Notify?.Invoke(text, by?.Id ?? 0);
            Completed?.Invoke(r);
        }

        private void Apply(Reward w, Request r, Character by)
        {
            if (w == null) return;
            if (w.Akce > 0) _house.AddAkce(w.Akce);
            foreach (var kv in w.Items) if (_content.Items.Contains(kv.Key)) _house.AddItem(kv.Key, kv.Value);
            var giver = _population.Find(r.GiverId);
            if (w.Affinity > 0 && giver != null && !giver.IsDead && Family != null)
                foreach (var m in Household())
                {
                    int add = m == by || by == null ? w.Affinity : w.Affinity / 3;
                    Family.SetAffection(m.Id, giver.Id, Family.Affection(m.Id, giver.Id) + add);
                }
            if (w.Reputation != 0) AddReputation?.Invoke(w.Reputation, r.Title);
            var xpTo = by ?? Find(r.AssignedTo);
            if (w.Xp > 0 && w.XpSkill.HasValue && xpTo != null) xpTo.GainSkillXp(w.XpSkill.Value, w.Xp);
            if (w.Seals > 0 && Legacy != null) Legacy.Award(SealSource.Other, w.Seals, Now(), "Adak: " + r.Title);
            if (w.KeepsakeItem != null && Keepsakes != null) Keepsakes.Grant(w.KeepsakeItem, w.KeepsakeLabel ?? r.Title, by?.Id ?? 0);
            if (w.Memory != 0 && giver != null) Memory.Add(giver, w.Memory, r.Title);
            if (!string.IsNullOrEmpty(w.MoodText) && by != null && by.MoodEnabled) by.AddMoodlet("mood.request_" + r.Kind.ToString().ToLowerInvariant(), w.MoodText, w.MoodValue, 1.0);
        }

        private Character Find(int id) => id != 0 ? _population.Find(id) : null;

        // ================================================================== yardımcılar

        private Request NewRequest(RequestKind kind, string template, string title, string text, Character giver)
        {
            var r = new Request
            {
                Id = _nextId++, Kind = kind, TemplateId = template, Title = title, PostedDay = Now(),
                GiverId = giver?.Id ?? 0, GiverName = giver?.Name ?? "Köy",
                Text = text != null && giver != null ? text.Replace("{giver}", giver.Name) : text
            };
            if (giver != null)
            {
                var def = DefOf(giver);
                r.GiverDefId = def?.Id;
                var h = _houses?.HouseOf(giver);
                r.HouseId = h?.Id ?? 0;
            }
            return r;
        }

        private Request Post(Request r, bool announce = true)
        {
            if (Literate) r.Read = true;
            _requests.Add(r);
            Posted?.Invoke(r);
            if (announce && OnBoard(r))
                Notify?.Invoke(r.Read ? $"Panoda yeni {Request.KindName(r.Kind).ToLowerInvariant()}: {r.Title} ({r.GiverName})." : "İlan tahtasına yeni bir kâğıt asıldı.", r.GiverId);
            return r;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ================================================================== kayıt

        public RequestSave Export()
        {
            var s = new RequestSave { nextId = _nextId, seasonPosted = _seasonPosted, playerScore = PlayerScore };
            var list = new List<RequestEntrySave>();
            foreach (var r in _requests) list.Add(RequestSaveMapper.Save(r));
            s.requests = list.ToArray();
            var mk = new List<int>(); var mv = new List<int>();
            foreach (var kv in Memory.All) { mk.Add(kv.Key); mv.Add(kv.Value); }
            s.memoryKeys = mk.ToArray(); s.memoryValues = mv.ToArray();
            var rk = new List<int>(); var rv = new List<int>();
            foreach (var kv in _rivalScore) { rk.Add(kv.Key); rv.Add(kv.Value); }
            s.rivalKeys = rk.ToArray(); s.rivalValues = rv.ToArray();
            var sk = new List<string>(); var sv = new List<int>();
            foreach (var kv in _storySteps) { sk.Add(kv.Key); sv.Add(kv.Value); }
            s.storyKeys = sk.ToArray(); s.storySteps = sv.ToArray();
            var ok = new List<string>(); var ov = new List<int>();
            foreach (var kv in _originals) { ok.Add(kv.Key); ov.Add(kv.Value); }
            s.originalDefs = ok.ToArray(); s.originalIds = ov.ToArray();
            s.vowsDone = new List<string>(_vowsDone).ToArray();
            s.setPieces = new List<string>(_setPieces).ToArray();
            return s;
        }

        public void Import(RequestSave s)
        {
            _requests.Clear(); Memory.Clear(); _rivalScore.Clear(); _storySteps.Clear(); _originals.Clear(); _vowsDone.Clear(); _setPieces.Clear();
            _nextId = 1; _seasonPosted = -1; PlayerScore = 0; _lastTick = -1;
            if (s == null) return;
            _nextId = Math.Max(1, s.nextId);
            _seasonPosted = s.seasonPosted;
            PlayerScore = s.playerScore;
            if (s.requests != null)
                foreach (var e in s.requests)
                {
                    var r = RequestSaveMapper.Load(e);
                    if (r == null) continue;
                    _requests.Add(r);
                    _nextId = Math.Max(_nextId, r.Id + 1);
                }
            Pairs(s.memoryKeys, s.memoryValues, (k, v) => Memory.Set(k, v));
            Pairs(s.rivalKeys, s.rivalValues, (k, v) => _rivalScore[k] = v);
            if (s.storyKeys != null && s.storySteps != null)
                for (int i = 0; i < Math.Min(s.storyKeys.Length, s.storySteps.Length); i++) if (s.storyKeys[i] != null) _storySteps[s.storyKeys[i]] = s.storySteps[i];
            if (s.originalDefs != null && s.originalIds != null)
                for (int i = 0; i < Math.Min(s.originalDefs.Length, s.originalIds.Length); i++) if (s.originalDefs[i] != null) _originals[s.originalDefs[i]] = s.originalIds[i];
            if (s.vowsDone != null) foreach (var v in s.vowsDone) if (v != null) _vowsDone.Add(v);
            if (s.setPieces != null) foreach (var v in s.setPieces) if (v != null) _setPieces.Add(v);
        }

        private static void Pairs(int[] keys, int[] values, Action<int, int> set)
        {
            if (keys == null || values == null) return;
            for (int i = 0; i < Math.Min(keys.Length, values.Length); i++) set(keys[i], values[i]);
        }
    }
}
