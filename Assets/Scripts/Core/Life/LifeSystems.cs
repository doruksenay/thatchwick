using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yaşam katmanının tek giriş noktası (Adım 18-43): oyun katmanı bunu kurar, her karede Tick'i çağırır,
    /// kayıtta Export/Restore kullanır. Alt sistemler (mezar, ruh hali, ...) burada toplanır; her biri
    /// bağımsız test edilebilir, burası yalnızca bağlama ve sıralamadır.
    /// </summary>
    public sealed class LifeSystems
    {
        public ContentDatabase Content { get; }
        public WorldState World { get; }
        public Household House { get; }
        public Population Population { get; }
        public GameClock Clock { get; }

        public MoodService Mood { get; }
        public GraveService Graves { get; }
        public VillageEffects Village { get; } = new VillageEffects();

        // Okunabilirlik ve planlama (Adım 18b-18d)
        public NotificationLog Log { get; } = new NotificationLog();
        public NeedWatcher Watcher { get; } = new NeedWatcher();
        public DayLedger Ledger { get; }
        public WinterForecast Forecast { get; }
        public SuitorPool Suitors { get; }
        /// <summary>Gezgin tüccar ve panayır (belge 34-D5).</summary>
        public TravelingMerchant Merchant { get; }

        // Dünya sistemleri (Adım 33-36, 42)
        public WeatherSystem Weather { get; }
        public FestivalService Festivals { get; }
        public SpoilageService Spoilage { get; }
        public ForageSystem Forage { get; }

        // Köy ve hanedan katmanı (Adım 38-41)
        public VillagerService Villagers { get; }
        public ToolService Tools { get; }
        public ProjectService Projects { get; }
        public GoalService Goals { get; }

        /// <summary>Miras: Soy Mührü, yadigârlar, komşu hanelere evlendirme, köydeki soy.</summary>
        public Legacy Legacy { get; }
        /// <summary>Komşu NPC hanedanları: köydeki aileler, başları, soy ağaçları (Adım 55).</summary>
        public NeighborHouseService Houses { get; }
        /// <summary>Köylülerin evleri: NPC hanesi oyuncunun arsasında yaşamaz (belge 47 §8).</summary>
        public VillageHomes Homes { get; }

        /// <summary>Hanedan kurucusunun kimliği (köydeki soy sayımı); oyun katmanı bağlar. Null ise sayım yapılmaz.</summary>
        public Func<int?> FounderId { get; set; }
        /// <summary>Kazalar ve kazanılmış izler (doküman 14, 3F).</summary>
        public InjurySystem Injuries { get; }
        /// <summary>Süreli görevler: madene, balığa, ormana gönderilen aile üyeleri (belge 47 §3).</summary>
        public TimedDutyService Duties { get; }
        /// <summary>Çocukların gezme izni ve köyün gözü (belge 47 §7).</summary>
        public Thatchwick.Core.Upbringing.ChildRoaming Roaming { get; } = new Thatchwick.Core.Upbringing.ChildRoaming();
        /// <summary>İstekler (belge 46): ilan panosu, ricalar, siparişler, Mevsim Defteri, çağrılar, hikâyeler, vasiyet ve adak.</summary>
        public Thatchwick.Core.Life.Requests.RequestService Requests { get; }

        /// <summary>
        /// Köyün gözü olayını uygular (belge 47 §7.3): itibar, hane hafızası eksisi ve gece eve getirilen çocuğun ebeveynine
        /// "Mahcup oldu". Konuşma balonu oyun katmanındadır. Eve getirme konumu oyun katmanında.
        /// </summary>
        public void ApplyGaze(Thatchwick.Core.Upbringing.GazeEvent ev)
        {
            if (ev == null || !ev.Scolding) return;
            if (ev.Reputation != 0) Upbringing.Reputation.Add(ev.Reputation, "başıboş çocuk dedikodusu");
            Requests?.Memory.Add(ev.Villager, Thatchwick.Core.Life.Requests.HouseMemory.Gaze, $"{ev.Child.Name} tek başına gezerken görüldü");
            if (!ev.Escort) return;
            foreach (var m in Household())
            {
                if (m == ev.Child || m.Stage == AgeStage.Baby || m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) continue;
                if (m.Id == ev.Child.BirthMotherId || m.Id == ev.Child.BirthFatherId)
                    m.AddMoodlet(Thatchwick.Core.Upbringing.ChildRoaming.AshamedMood, "Mahcup oldu", Thatchwick.Core.Upbringing.ChildRoaming.AshamedValue, 1.0);
            }
            Say($"{ev.Villager.Name}, gece tek başına gezen {ev.Child.Name}'i eve getirdi. \"Bu saatte çocuk sokakta olur mu?\"", NoticeLevel.Warning, ev.Child.Id);
        }
        /// <summary>Ev dekorasyonu: raf çökmesi, ayna kararması, ayna hatırlatması (Project belgesi 27).</summary>
        public Thatchwick.Core.Build.DecorSystem Decor { get; }
        /// <summary>Hatıralar, soy ağacı işlemesi ve çeyiz sandığı (Project belgesi 27, D3b).</summary>
        public KeepsakeService Keepsakes { get; }
        /// <summary>Ev bakımı ve otomasyonu: mum, is ve badana, saksı, yükseltme, oda türleri, desen ve arma, koleksiyon, oda planı, hizmetkâr, ev yarışması (belge 45).</summary>
        public HomeLife Home { get; }
        /// <summary>Ruh hali uyarıları ve bunalım (Project belgesi 23).</summary>
        public BreakdownService Breakdowns { get; }
        /// <summary>Çocukluk eşyaları, can yoldaşı, çıraklık, aile tarifleri ve itibar (belge 30, 31-8, 34).</summary>
        public Thatchwick.Core.Upbringing.UpbringingSystems Upbringing { get; }
        /// <summary>Köy meydanı (bunalımda "Meydana kaçtı"); null ise o tür seçilmez.</summary>
        public Func<GridPos?> SquarePosition { get; set; }
        private readonly Random _upbringingRng;
        /// <summary>Evlilik ve kutlanan şenlik sayıları (hedef ölçütleri için).</summary>
        public int Marriages { get; private set; }
        public int FestivalsCelebrated { get; private set; }
        /// <summary>Hesap seviyesi sağlayıcı (alet yükseltmesi); null ise sınır yok.</summary>
        public Func<int> AccountLevel { get; set; }
        /// <summary>Alet kademesi kilidi Demircilik'e bakar (oyunda açık; eski sınamalar için kapatılabilir).</summary>
        public bool SkillLocks
        {
            get => Tools.SkillLevel != null;
            set => Tools.SkillLevel = value ? BestSkill : (Func<SkillType, int>)null;
        }

        /// <summary>Yaşam olayları (düz metin): mezar, yas vb. Ayrıntılı akış için Log.</summary>
        public event Action<string> Notice;

        private double _moodAccumulator;
        private long _lastDay = -1;
        private int _lastSeasonIndex = -1;
        public const double MoodUpdateStepDays = 0.02;

        /// <summary>Hesap XP'si sağlayıcı (gün özeti kartındaki XP farkı için); null ise 0.</summary>
        public Func<int> AccountXp { get; set; }

        /// <param name="seed">Rastgelelik tohumu (toplama noktaları, talipler, halefler). 0 ise sabit varsayılanlar (testler).</param>
        public LifeSystems(ContentDatabase content, WorldState world, Household house, Population population, GameClock clock, int seed = 0)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            World = world ?? throw new ArgumentNullException(nameof(world));
            House = house ?? throw new ArgumentNullException(nameof(house));
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Mood = new MoodService(world) { Content = content, ArrangerLevel = () => BestSkill(SkillType.Decorating) };
            Decor = new Thatchwick.Core.Build.DecorSystem(world, content, seed != 0 ? Seeds.Derive(seed, 14) : 97)
            {
                Members = () => Population.Members,
                House = house,
                Notify = (text, id) => Say(text, NoticeLevel.Warning, id)
            };
            Graves = new GraveService(world, content);
            Ledger = new DayLedger(house, () => AccountXp != null ? AccountXp() : 0);
            Forecast = new WinterForecast(content, house, world, population, clock.Config);
            Suitors = seed != 0 ? new SuitorPool(content, population, Seeds.Derive(seed, 1)) : new SuitorPool(content, population);
            Merchant = new TravelingMerchant(content, house, seed != 0 ? Seeds.Derive(seed, 16) : 23);
            Weather = new WeatherSystem(clock.Config, seed: 1);
            Festivals = new FestivalService(content, clock) { Village = Village };
            Spoilage = new SpoilageService(content, house, world) { ExtraReduction = SpoilReduction };
            Forage = seed != 0 ? new ForageSystem(world, content, Seeds.Derive(seed, 2)) : new ForageSystem(world, content);
            Villagers = seed != 0 ? new VillagerService(content, population, house, clock, Seeds.Derive(seed, 3)) : new VillagerService(content, population, house, clock);
            Tools = new ToolService(content, house)
            {
                AccountLevel = () => AccountLevel != null ? AccountLevel() : int.MaxValue,
                // Hanede 5. seviye demirci varsa köy demircisi gerekmez.
                SmithAvailable = () => !HasVillagerRole(RoleSmithDefined) || Villagers.HasRole(VillagerService.RoleBlacksmith)
                    || BestSkill(SkillType.Smithing) >= SmithSelfLevel
            };
            Projects = new ProjectService(content, house, Village);
            Goals = new GoalService(content) { Metric = MetricValue };
            Legacy = new Legacy(population, seed != 0 ? Seeds.Derive(seed, 5) : 61) { DaysPerYear = clock.Config.DaysPerYear };
            Mood.ExtraComfort = Legacy.ComfortBonus;
            Keepsakes = new KeepsakeService(world, content, house)
            {
                Generation = () => MetricValue("generations"),
                Year = () => Clock.YearIndex + 1,
                Today = () => Clock.TotalDays,
                People = () => Population.Members,
                Notify = (text, id) => Say(text, NoticeLevel.Info, id)
            };
            Home = new HomeLife(world, content, house, clock)
            {
                Members = () => Population.Members,
                FindPerson = Population.Find,
                Notify = (text, id) => Say(text, NoticeLevel.Info, id),
                GrantKeepsake = (id, label) => Keepsakes.Grant(id, label),
                BeautyOf = Mood.BeautyOf
            };
            Mood.BeautyBonus = it => Keepsakes.BeautyBonus(it) + Thatchwick.Core.Build.DecorSystem.SeasonalBeauty(it, Clock.Season) + Home.BeautyBonus(it);
            Mood.RoomBeautyExtra = Home.RoomBeautyExtra;
            Mood.ExtraComfort = it => Legacy.ComfortBonus(it) + Home.ComfortBonus(it);
            Decor.CollapseFactor = Home.CollapseFactor;
            UpdateMerchant(false);
            // Düğün önce: çeyiz sandığı gelinle gitmeden doluluğu okunur (belge 34-D2).
            Legacy.MarriedOutHappened += (member, villager) =>
            {
                var (filled, max) = Keepsakes.BestDowryFill();
                Upbringing?.Weddings?.Celebrate(member, villager, member.Sex == Sex.Female && max > 0 && filled >= max);
            };
            Legacy.MarriedOutHappened += (member, villager) => Keepsakes.OnMarriedOut(member, villager, _family, Legacy, Household());
            population.BabyBorn += (baby, mother, father) => Keepsakes.OnBirth(baby);
            Houses = new NeighborHouseService(population, Villagers, content, seed != 0 ? Seeds.Derive(seed, 9) : 71) { Today = () => Clock.TotalDays };
            Homes = new VillageHomes(Villagers, Houses, population, () =>
            {
                var list = new List<string>();
                foreach (var st in Villagers.States)
                {
                    var vc = Population.Find(st.CharacterId);
                    if (vc == null || vc.IsDead) continue;
                    if (Content.Villagers.TryGet(st.DefId, out var vd)) list.Add(vd.Home);
                }
                return list;
            });
            Houses.HeadChanged += (h, heir, old) => Say($"{h.Name}: {heir.Name} hanenin başına geçti{(old != null ? $" ({old.Name} yerine)" : "")}.", NoticeLevel.Info, heir.Id);
            Houses.VillagersMarried += (a, b) => Say($"Köyde düğün: {a.Name} ile {b.Name} evlendi.", NoticeLevel.Info, a.Id);
            Houses.HouseEnded += h => Say($"{h.Name}nin soyu tükendi.", NoticeLevel.Info);
            Legacy.MarriedOutHappened += (member, villager) => Houses.JoinSpouses(member, villager);
            Home.IsBusy = c => Villagers.IsNamed(c);
            Home.Raining = () => { var w = Weather.Today(Clock.TotalDays); return w == WeatherType.Rain || w == WeatherType.Storm; };
            Home.NeighborCount = () => Math.Max(2, Houses.Houses.Count);
            Home.CollectionCompleted += c =>
            {
                Upbringing.Reputation.Add(5, $"{c.Name} koleksiyonu");
                Ledger.Note($"Koleksiyon: {c.Name}");
            };
            Home.ContestJudged += r =>
            {
                if (r.Rank == 1) Upbringing.Reputation.Add(8, "en güzel ev");
                else if (r.Rank == 2) Upbringing.Reputation.Add(4, "ev yarışmasında ikinci");
                Ledger.Note($"Ev yarışması: {r.Rank}.");
            };
            Suitors.Arrived += guest => Home.GuestVisit(guest);
            Villagers.RoleInherited += (heir, def) => Say($"{heir.Name}, {def.RoleName.ToLowerInvariant()} dükkânını devraldı.", NoticeLevel.Info, heir.Id);
            Legacy.SealsChanged += e =>
            {
                if (e.Amount > 0) Say($"Soy Mührü +{e.Amount}: {e.Note}.", NoticeLevel.Info);
            };
            Legacy.HeirloomLost += (h, msg) => Say(msg, NoticeLevel.Warning);
            // Doğum günü: mevsim ve gün (karakter panelinde gösterilir).
            population.BabyBorn += (baby, mother, father) => { baby.BirthSeason = (int)Clock.Season; baby.BirthDayOfSeason = Clock.DayOfSeason; };
            Injuries = new InjurySystem(population, seed != 0 ? Seeds.Derive(seed, 4) : 0)
            {
                MinDaysBetween = clock.Config.DaysPerSeason,
                Today = () => Clock.TotalDays,
                IsOutdoors = c => { var room = World.Map.Rooms.RoomAt(c.Position); return room == null || !room.IsIndoor; },
                WeatherFactor = () =>
                {
                    var w = Weather.Today(Clock.TotalDays);
                    return w == WeatherType.Storm ? 2.0 : w == WeatherType.Snow ? 1.5 : 1.0;
                },
                ToolSpeed = id => Tools.SpeedFor(id),
                Notify = (text, id) => Say(text, NoticeLevel.Warning, id)
            };
            Duties = new TimedDutyService(population, house, content, seed != 0 ? Seeds.Derive(seed, 17) : 311)
            {
                Now = () => Clock.TotalDays,
                SeasonNow = () => Clock.Season,
                WeatherNow = () => Weather.Today(Clock.TotalDays),
                ToolTier = id => Tools.Tier(id),
                Injure = (c, id) => Injuries.InjureById(c, id),
                Notify = (text, id) => Say(text, NoticeLevel.Info, id),
                HasLeader = () =>
                {
                    foreach (var c in Household()) if (c.Duty == DutyKind.Steward && !c.Away) return true;
                    return false;
                }
            };
            Duties.Finished += r => Ledger.Note($"{r.CharacterName}: {TimedDutyService.KindName(r.Kind).ToLowerInvariant()} görevinden döndü");
            Breakdowns = new BreakdownService(population, seed != 0 ? Seeds.Derive(seed, 12) : 83)
            {
                SquarePosition = () => SquarePosition?.Invoke(),
                FoodScarce = () => Forecast.Report(Clock.TotalDays).HasDanger
            };
            Breakdowns.Raised += (level, text, id) => Say(text, level, id);
            Breakdowns.Started += (c, kind) => Ledger.Note($"{c.Name}: {MoodBehavior.BreakdownName(kind).ToLowerInvariant()}");
            _upbringingRng = new Random(seed != 0 ? Seeds.Derive(seed, 13) : 89);
            Upbringing = new Thatchwick.Core.Upbringing.UpbringingSystems(population, seed != 0 ? Seeds.Derive(seed, 15) : 0)
            {
                Today = () => Clock.TotalDays,
                Notify = (text, id) => Say(text, NoticeLevel.Info, id),
                Chronicle = text => Ledger.Note(text)
            };
            Upbringing.AttachHouse(house);
            Duties.Toys = Upbringing.Toys;
            Requests = new Thatchwick.Core.Life.Requests.RequestService(population, house, content, Villagers, Houses, seed != 0 ? Seeds.Derive(seed, 18) : 46)
            {
                Now = () => Clock.TotalDays,
                DaysPerSeason = clock.Config.DaysPerSeason,
                MarketDay = () => Festivals.IsMarketDayToday,
                Legacy = Legacy,
                Keepsakes = Keepsakes,
                Duties = Duties,
                AddReputation = (n, why) => Upbringing.Reputation.Add(n, why),
                Notify = (text, id) => Say(text, NoticeLevel.Info, id),
                Chronicle = text => Ledger.Note(text)
            };
            Duties.Finished += Requests.OnDutyFinished;
            population.MemberDied += c => Requests.OnDied(c);
            population.BabyBorn += (baby, mother, father) => Requests.OnBabyBorn(baby, mother);
            Houses.VillagersMarried += (a, b) => Requests.OnVillageWedding(a, b);
            Upbringing.Weddings.DowryFill = Keepsakes.BestDowryFill;
            Suitors.QualityChance = Upbringing.Weddings.SuitorQuality;
            // Nitelikli talibin ailesi de itibarlıdır: çeyiz ya da damat hediyesi iki kat (34-D2).
            Keepsakes.DowryScale = () => Suitors.LastWasNotable ? 2 : 1;
            Upbringing.Weddings.GiftScale = () => Suitors.LastWasNotable ? 2 : 1;
            Merchant.AddReputation = (n, why) => Upbringing.Reputation.Add(n, why);
            Thatchwick.Core.Upbringing.ToyNames.Lookup = id => content.Items.TryGet(id, out var td) ? td.DisplayName : null;
            Breakdowns.Started += (c, kind) => { if (!c.IsVillager) Upbringing.Reputation.Add(Thatchwick.Core.Upbringing.ReputationSystem.Reasons.Breakdown, "bunalım"); };
            Spoilage.Spoiled += (id, n) =>
            {
                bool ice = content.Items.TryGet(id, out var d) && d.HasTag("ice");
                Say(ice ? $"{n} buz eridi (buzluğa koy)." : $"{n} {(d != null ? d.DisplayName : id)} bozuldu.", ice ? NoticeLevel.Info : NoticeLevel.Warning);
            };
            Festivals.Announced += f => Say($"Bugün {f.DisplayName}!", NoticeLevel.Info);
            Festivals.Celebrated += (f, n) =>
            {
                FestivalsCelebrated++;
                Upbringing.Reputation.Add(Thatchwick.Core.Upbringing.ReputationSystem.Reasons.Festival, f.DisplayName);
                Say($"{f.DisplayName}: {n} kişi şenliğe katıldı.", NoticeLevel.Info);
                foreach (var g in Upbringing.Games.Run(Population.Members))   // şenlik yarışları (atletizm)
                    Say(g.HouseholdWon ? $"{g.Game}: {g.Winner.Name} kazandı! +{Thatchwick.Core.Upbringing.FestivalGames.PrizeAkce} akçe, itibar."
                                       : $"{g.Game}: {g.Winner.Name} kazandı ({g.Entrants} yarışçı).", NoticeLevel.Info, g.Winner.Id);
                Keepsakes.OnFestival(f.DisplayName, n);
                if (f.Id == HomeLife.ContestFestival) Home.JudgeContest();   // panayırda ev yarışması (belge 45)
                if (f.Id == VintageFestival) foreach (var note in Vintage()) Say(note, NoticeLevel.Info);
                Ledger.Note(f.DisplayName);
                Goals.Update();
            };
            Tools.Upgraded += (tool, tier) => Say($"{tier.Name} hazır.", NoticeLevel.Info);
            Projects.Completed += p =>
            {
                Population.RecomputeAll();
                Say($"Köy projesi tamamlandı: {p.DisplayName}.", NoticeLevel.Info);
                Ledger.Note(p.DisplayName);
                Legacy.OnProjectCompleted(p.DisplayName, Clock.TotalDays);
                Upbringing.Reputation.Add(Thatchwick.Core.Upbringing.ReputationSystem.Reasons.Project, p.DisplayName);
                Goals.Update();
            };
            Goals.Completed += OnGoalCompleted;
            Villagers.EventTriggered += (v, e) => Say($"{v.Name}: {e.Text}", NoticeLevel.Info, v.Id);
            Villagers.SuccessorArrived += (v, def) => Say($"{v.Name} köye yerleşti ({def.RoleName}).", NoticeLevel.Info, v.Id);
            Villagers.GiftGiven += (v, item, taste, msg) => Say(msg, taste == GiftTaste.Dislike ? NoticeLevel.Warning : NoticeLevel.Info, v.Id);
            Watcher.Raised += (level, text, id) => Say(text, level, id, transient: true);
            population.BabyBorn += (baby, mother, father) =>
            {
                Ledger.OnBorn(baby);
                Say($"{baby.Name} doğdu!", NoticeLevel.Info, baby.Id);
            };

            population.MemberAdded += WatchSkills;
            foreach (var m in population.Members) WatchSkills(m);
            population.Content = content;
            population.Village = Village;
            population.MemberDied += OnMemberDied;
        }

        /// <summary>
        /// Ergen Genç yetişkin oldu: çocukluk ruh hali özelliğe dönüşür (karar 18.11.2026). Not hanedan geçmişine
        /// (kişinin hayat notları) yazılır; hane üyesi için bildirim çıkar.
        /// </summary>
        public string OnGrownUp(Character c)
        {
            var mother = c.BirthMotherId.HasValue ? Population.Find(c.BirthMotherId.Value) : null;
            var father = c.BirthFatherId.HasValue ? Population.Find(c.BirthFatherId.Value) : null;
            string aptitudeNote = LifeRules.ApplyChildhoodAptitude(c);
            if (aptitudeNote != null && !c.IsVillager) Say($"{c.Name}: {aptitudeNote}", NoticeLevel.Info, c.Id);
            string note = LifeRules.ApplyUpbringing(Content, _upbringingRng, c, mother, father);
            if (note == null) return null;
            LifeRules.Recompute(c, Content, Village);
            if (!c.IsVillager) Say($"{c.Name} genç bir yetişkin oldu. {note}", NoticeLevel.Info, c.Id);
            return note;
        }

        /// <summary>Beceri temposu (günlük doygunluk, yaşlılık gerilemesi) hane üyelerinde açılsın mı. Varsayılan açık.</summary>
        public bool SkillPacing
        {
            get => _skillPacing;
            set
            {
                _skillPacing = value;
                foreach (var c in _watched) c.Skills.PacingEnabled = value && !c.IsVillager;
            }
        }
        private bool _skillPacing = true;

        /// <summary>Yaşlılığa giriş: becerilerde bir kereye mahsus küçük gerileme (seviye düşmez).</summary>
        public void OnBecameElder(Character c)
        {
            if (c == null || c.IsDead) return;
            if (c.Skills.ApplyElderOnset() > 0 && !c.IsVillager)
                Say($"{c.Name} yaşlandı: elleri eskisi kadar çabuk değil. Becerilerini her mevsim bir kez çalıştırmalı.", NoticeLevel.Info, c.Id);
        }

        /// <summary>Mevsim sonu: yaşlılarda o mevsim hiç kullanılmayan beceriler geriler; herkesin kullanım kaydı sıfırlanır.</summary>
        public void OnSkillSeasonEnd()
        {
            foreach (var c in Population.Members)
            {
                if (c.IsDead) continue;
                var decayed = c.Skills.OnSeasonEnd(c.Stage == AgeStage.Elder);
                if (decayed.Count > 0 && !c.IsVillager)
                {
                    var names = new List<string>();
                    foreach (var s in decayed) names.Add(SkillNames.Name(s));
                    Say($"{c.Name} bu mevsim {string.Join(", ", names)} çalışmadı; biraz paslandı.", NoticeLevel.Info, c.Id);
                }
            }
        }

        private const string RoleSmithDefined = VillagerService.RoleBlacksmith;

        private bool HasVillagerRole(string role)
        {
            foreach (var v in Content.Villagers.All) if (v.Role == role) return true;
            return false;
        }

        private readonly HashSet<Character> _watched = new HashSet<Character>();

        private void WatchSkills(Character c)
        {
            if (!_watched.Add(c)) return;
            // Beceri temposu (doygunluk, yaşlılık gerilemesi) hane üyeleri için açık; köylüler etkilenmez.
            if (SkillPacing && !c.IsVillager) c.Skills.PacingEnabled = true;
            c.BreakdownEnded += (ch, kind, interrupted) =>
            {
                if (ch.IsDead) return;
                Say(interrupted ? $"{ch.Name} bunalımı bırakıp kendine bakıyor." : $"{ch.Name} toparlandı: içini döktü.", NoticeLevel.Info, ch.Id);
            };
            c.StageChanged += (ch, before, after) =>
            {
                if (after == AgeStage.Elder && before != AgeStage.Elder) { OnBecameElder(ch); return; }
                if (after != AgeStage.YoungAdult || before == AgeStage.YoungAdult) return;
                OnGrownUp(ch);
            };
            c.Skills.LevelChanged += (skill, before, after) =>
            {
                if (c.IsDead || c.IsVillager) return;
                if ((before < LifeRules.BranchLevelFirst && after >= LifeRules.BranchLevelFirst)
                    || (before < LifeRules.BranchLevelSecond && after >= LifeRules.BranchLevelSecond))
                {
                    Say($"{c.Name}, {LifeRules.TradeName(skill)} becerisinde {after}. seviyeye ulaştı: meslek dalı seçebilir.", NoticeLevel.Info, c.Id);
                }
                Goals.Update();
            };
        }

        private int MetricValue(string metric)
        {
            switch (metric)
            {
                case "generations":
                {
                    int g = 1;
                    foreach (var c in Population.Members) if (!c.IsVillager) g = Math.Max(g, c.Generation);
                    return g;
                }
                case "living": { int n = 0; foreach (var _ in Household()) n++; return n; }
                case "akce": return House.Akce;
                case "projects": return Projects.CompletedCount;
                case "branches": { int n = 0; foreach (var c in Household()) n += c.BranchIds.Count; return n; }
                case "skill":
                {
                    int best = 1;
                    foreach (var c in Household())
                        foreach (SkillType s in Enum.GetValues(typeof(SkillType))) best = Math.Max(best, c.Skills.GetLevel(s));
                    return best;
                }
                case "days": return (int)Clock.DayIndex;
                case "marriages": return Marriages;
                case "festivals": return FestivalsCelebrated;
                default: return 0;
            }
        }

        private void OnGoalCompleted(GoalDef goal)
        {
            if (goal.RewardAkce > 0) House.AddAkce(goal.RewardAkce);
            Character target = null;
            foreach (var c in Household()) { if (c.IsPlayerControlled) { target = c; break; } if (target == null) target = c; }
            if (target != null && goal.RewardXp > 0)
            {
                SkillType best = SkillType.Farming; double bestXp = -1;
                foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                {
                    double xp = target.Skills.GetXp(s);
                    if (xp > bestXp) { best = s; bestXp = xp; }
                }
                target.GainSkillXp(best, goal.RewardXp);
            }
            Say($"Hedef tamamlandı: {goal.DisplayName}.", NoticeLevel.Info);
            Ledger.Note(goal.DisplayName);
            Legacy.OnGoalCompleted(goal.DisplayName, Clock.TotalDays);
        }

        /// <summary>Soy mesleği: haneden dal almış karakterlerin en çok seçtiği beceri; yoksa null.</summary>
        public string FamilyTrade()
        {
            var members = new List<Character>();
            foreach (var c in Population.Members) if (!c.IsVillager) members.Add(c);
            var skill = LifeRules.DominantBranchSkill(Content, members);
            return skill.HasValue ? LifeRules.TradeName(skill.Value) + " soyu" : null;
        }

        public string TradeOf(Character c) => LifeRules.TradeOf(Content, c);

        /// <summary>Hanedan başı değişti (Dynasty.OnDeath sonrası): yadigârlar bir nesil yaşlanır, nesil ve soy mesleği mührü.</summary>
        public void OnSuccession(Dynasty dynasty, Character heir)
        {
            Legacy.OnSuccession(dynasty, heir, Clock.TotalDays);
            Upbringing.OnSuccession();
            foreach (var h in Legacy.Heirlooms)
                if (h.Level > 1) { Say($"Yadigârlar bir nesil daha hanede: bonuslar güçlendi.", NoticeLevel.Info); break; }
        }

        /// <summary>Bildirim akışına ve düz metin olayına yazar. Dış sistemler (aile, pazar) bunu çağırabilir.</summary>
        public void Say(string message, NoticeLevel level = NoticeLevel.Info, int characterId = 0, bool transient = false)
        {
            Log.Add(Clock.TotalDays, level, message, characterId, transient);
            Notice?.Invoke(message);
        }

        private void OnMemberDied(Character dead)
        {
            if (!dead.IsVillager) Legacy.OnHouseholdDeath(Clock.Season);
            Keepsakes.OnDeath(dead);
            Ledger.OnDied(dead);
            Graves.Bury(dead, Clock.TotalDays);
            int mourners = Mood.Mourn(dead, Population.Members);
            Say(mourners > 0
                ? $"{dead.Name} toprağa verildi; {mourners} kişi yasta."
                : $"{dead.Name} toprağa verildi.", NoticeLevel.Danger, dead.Id);
        }

        // ---- hane geneli etkiler (özellik/dal en iyisi + köy projeleri) ----

        private IEnumerable<Character> Household()
        {
            foreach (var c in Population.Members) if (!c.IsDead && !c.IsVillager) yield return c;
        }

        /// <summary>Hane üyelerinden birinin (en iyisi) çarpanı: pazarlıkçı, yeşil parmaklı gibi biri yeter.</summary>
        public float BestOf(Func<LifeModifiers, float> pick)
        {
            float best = float.MinValue;
            foreach (var c in Household()) best = Math.Max(best, pick(c.Modifiers));
            return best == float.MinValue ? pick(new LifeModifiers()) : best;
        }

        public float SpoilReduction() => Math.Min(0.8f, BestOf(m => m.SpoilReduction) + Village.SpoilReduction);
        public double SellBonus() => Math.Min(0.75, BestOf(m => m.SellBonus) + Village.MarketSell + Upbringing.Reputation.SellBonus());
        // Yapı ve eşya indirimi yalnızca dallardan ve özelliklerden gelir. Ev düzenleme indirim vermez; oda konforu verir
        // (belge 45 karar 7: MoodService.ArrangementComfort, RoomMoodSystem.TidyHarmonyLevel).
        public float BuildDiscount() => Math.Min(0.5f, BestOf(m => m.BuildDiscount));

        /// <summary>Alet yükseltmesini köy demircisi olmadan yapabilmek için hanedeki demircinin seviyesi.</summary>
        public const int SmithSelfLevel = 5;

        /// <summary>Hanenin (köylü olmayan, yaşayan) üyeleri arasında bir becerinin en yüksek seviyesi; kimse yoksa 1.</summary>
        public int BestSkill(SkillType skill)
        {
            int best = 1;
            foreach (var c in Population.Members)
                if (!c.IsDead && !c.IsVillager) best = Math.Max(best, c.Skills.GetLevel(skill));
            return best;
        }

        /// <summary>Bir beceride hanenin en iyisi (eşitse ilk); kimse yoksa null.</summary>
        public Character BestAt(SkillType skill)
        {
            Character best = null;
            foreach (var c in Population.Members)
                if (!c.IsDead && !c.IsVillager && (best == null || c.Skills.GetXp(skill) > best.Skills.GetXp(skill))) best = c;
            return best;
        }

        /// <summary>Hava bilgisi: kaç gün ilerisi görülür (1. seviye yarın, 4. seviye 2 gün, 7. seviye 3 gün, 10. seviye 4 gün).</summary>
        public int ForecastDays => 1 + (BestSkill(SkillType.WeatherLore) - 1) / 3;

        /// <summary>Önümüzdeki günlerin havası (hava bilgisi seviyesine göre).</summary>
        public List<WeatherType> WeatherAhead(int days)
        {
            var list = new List<WeatherType>();
            for (int d = 1; d <= days; d++) list.Add(Weather.Today(Clock.TotalDays + d));
            return list;
        }

        /// <summary>
        /// Hekimlik (gün başında): hanenin hekimi soğuk ve açlık yarası olan herkesi seviye başına 0,1 gün iyileştirir,
        /// iyileştirdiği kişi başına XP alır. Kimse yaralı değilse bir şey olmaz.
        /// </summary>
        public int DailyHealing()
        {
            var healer = BestAt(SkillType.Medicine);
            if (healer == null) return 0;
            double days = 0.1 * healer.Skills.GetLevel(SkillType.Medicine);
            int n = 0;
            foreach (var c in Population.Members)
                if (!c.IsDead && !c.IsVillager && c.Treat(days)) n++;
            if (n > 0) healer.GainSkillXp(SkillType.Medicine, 6 * n);
            return n;
        }

        /// <summary>Ekin büyüme çarpanı: hava x (1 + köy) x en iyi yeşil parmak.</summary>
        public float GrowthMultiplier() =>
            WeatherSystem.GrowthMultiplier(Weather.Today(Clock.TotalDays)) * (1f + Village.CropGrowth) * BestOf(m => m.GrowthSpeed)
            * (1f + (float)(Adoptions.PestReduction * 0.5));   // kirpi zararlıyı azaltır (belge 31-2)

        /// <summary>İş hızı çarpanı (JobRunner.SpeedProvider): dışarıda hava durumu.</summary>
        public double WorkSpeedFor(Character c, InteractionDef interaction)
        {
            double f = 1.0;
            if (!string.IsNullOrEmpty(interaction.Tool)) f *= Tools.SpeedFor(interaction.Tool);
            if (c.HasBranch(GlassBranchId) && ProducesGlass(interaction)) f *= GlassBranchSpeed;   // Camcı dalı (27 §18.3)
            f *= Thatchwick.Core.Upbringing.ElderRoles.WorkSpeed(c, interaction.Skill);   // yaşlı hafif işlerde hızlı (34-D4)
            var room = World.Map.Rooms.RoomAt(c.Position);
            bool outdoors = room == null || !room.IsIndoor;
            if (outdoors) f *= WeatherSystem.OutdoorWorkSpeed(Weather.Today(Clock.TotalDays));
            return f;
        }

        /// <summary>Çömlekçilik altındaki Camcı dalı: "glass" etiketli ürün veren işler bu kadar hızlı.</summary>
        public const string GlassBranchId = "branch.pot_b";   // "Camcı" (eski adı Çarkçı Çömlekçi; kayıtlar korunur)
        public const string AshId = "item.ash";
        /// <summary>Ocağa atılan yakıt (HeatConfig.FirewoodId ile aynı).</summary>
        public const string FirewoodId = "item.firewood";
        public const double GlassBranchSpeed = 1.25;

        private bool ProducesGlass(InteractionDef interaction)
        {
            foreach (var p in interaction.Produce)
                if (Content.Items.TryGet(p.ItemId, out var d) && d.HasTag("glass")) return true;
            return false;
        }

        // ---- diğer sistemlere bağlama (oyun katmanı kurarken bir kez çağırır) ----

        /// <summary>Aile sistemine bağlanır: köylü hediye/yakınlık kuralları ve evlilik sayacı.</summary>
        public void Attach(FamilyService family)
        {
            Home.AddAffection = (a, b, d) => family.SetAffection(a, b, family.Affection(a, b) + d);
            _family = family;
            family.JoinedHousehold += c => Keepsakes.OnJoined(c);
            family.JoinedHousehold += c => Upbringing.Weddings?.OnGroomJoined(c, id => Content.Items.Contains(id));   // damat tarafı (34-D2)
            Villagers.Family = family;
            Requests.Family = family;
            family.Chatted += (a, b) => { Requests.OnTalk(a, b); Requests.OnTalk(b, a); };
            family.ExtraProposalCheck = Villagers.ProposalCheck;
            family.Happened += (actor, target, action, msg) =>
            {
                if (action != SocialAction.Propose) return;
                Marriages++;
                Upbringing.Weddings.Celebrate(actor, target, false);   // itibar düğün ölçeğinden (belge 34-D2)
                Goals.Update();
            };
        }

        public void Attach(JobRunner runner)
        {
            runner.ActionTimeScale = Clock.Config.ActionTimeScale;   // belge 47: işler gerçek zamanda sabit
            runner.SpeedProvider = WorkSpeedFor;
            runner.SquarePosition = () => SquarePosition?.Invoke();
            runner.Refused += (c, text) => Say($"{c.Name}: \"{text}\"", NoticeLevel.Info, c.Id, transient: true);
            runner.ItemBonus = (it, i) => HomeLife.Combine(Legacy.BonusFor(it, i), Home.BonusFor(it, i));
            runner.InteractionCompleted += (c, interaction) => Injuries.OnWorkDone(c, interaction);
            runner.ItemUsed += (c, item, interaction) => Upbringing.OnItemUsed(c, item);
            runner.ItemUsed += (c, item, interaction) => { if (interaction.Id == ReadBoardInteraction) ReadBoard(c); };
            runner.WorkDone += Requests.OnWork;
            // Üşüyen ocağı kendisi yakar (Denge 41): ısı sistemi bağlıysa envanterden kütük atar.
            runner.Stoke = item => _heat != null && _heat.TryStoke(item.Id, House);
            runner.HasFirewood = () => _heat != null && House.Count(FirewoodId) > 0;
        }
        /// <summary>Hayvanlar: can yoldaşı bağı (yakınlık, bakım, ölüm), kalite ve binicilik bonusu.</summary>
        public void Attach(Thatchwick.Core.Animals.AnimalSystem animals, Thatchwick.Core.Animals.AnimalCare care)
        {
            Upbringing.Attach(animals, care);
            Merchant.Animals = animals;
        }

        public void Attach(PlantSystem plants)
        {
            _plants = plants;
            plants.GrowthMultiplier = GrowthMultiplier;
            if (_heat != null) plants.TemperatureAt = _heat.TemperatureAt;   // sera ısısı
        }
        public void Attach(HeatSystem heat)
        {
            _heat = heat;
            heat.OutdoorOffset = () => WeatherSystem.TemperatureOffset(Weather.Today(Clock.TotalDays));
            Duties.OutdoorTemperature = () => heat.OutdoorTemperature;
            Spoilage.TemperatureAt = heat.TemperatureAt;   // bodrum mahzeni serinlik şartı
            Decor.TemperatureAt = heat.TemperatureAt;      // sepetteki meyve oda sıcaklığında bozulur
            if (Content.Items.Contains(AshId)) heat.AshItemId = AshId;   // ocağa atılan odunun külü (cam zinciri, 27 §18.3)
            if (_plants != null) _plants.TemperatureAt = heat.TemperatureAt;
        }
        private PlantSystem _plants;
        private HeatSystem _heat;
        /// <summary>Hayvanlar: attan düşen binici yaralanır (ölümcül değil), at ürkünce bildirim.</summary>
        /// <summary>Haritada dolaşan yaban hayvanları (kayda yazılmaz, gelip giderler).</summary>
        public WildRoamers Roamers { get; private set; }
        /// <summary>Tilki, yaban domuzu ve karga etkileri (Attach(AnimalSystem) ile kurulur).</summary>
        public WildMischief Mischief { get; private set; }

        public void AttachWild(Thatchwick.Core.World.GridMap map)
        {
            Roamers = new WildRoamers(map) { Journal = Journal };
            Roamers.FirstSeen += sp => Say($"Doğa Defteri: {sp.Name} ilk kez görüldü.", NoticeLevel.Info);
        }

        public void Attach(Thatchwick.Core.Animals.AnimalSystem animals)
        {
            animals.RiderFell += (horse, rider) =>
            {
                Injuries.InjureById(rider, "horse_fall");   // moodlet ve bildirim InjurySystem'den
            };
            animals.HorseRefused += horse => Say($"{horse.Name} engelin önünde ürktü.", NoticeLevel.Info);
            // Kurt saldırısı (31-4.1): çoban = "Hayvanlar" görevli genç yetişkin ve üstü.
            Wolves = new WolfSystem(animals, _wolfSeed)
            {
                Shepherds = () =>
                {
                    int n = 0;
                    foreach (var c in Population.Members)
                        if (!c.IsDead && !c.IsVillager && c.Duty == DutyKind.Animals && c.Stage != AgeStage.Baby && c.Stage != AgeStage.Child && c.Stage != AgeStage.Teen) n++;
                    return n;
                }
            };
            Mischief = new WildMischief(animals, World, Adoptions, _wolfSeed + 1);
            Wolves.Warning += text => Say(text, NoticeLevel.Warning);
            Contests.Finished += (r, a, h) => Say($"{h.Name}: {r.Summary}" + (r.PrizeAkce > 0 ? $" Ödül {r.PrizeAkce} akçe." : ""), r.Won ? NoticeLevel.Info : NoticeLevel.Info, h.Id);
            // Yarışmalar şenlik günü koşar (29-4.1).
            Festivals.Celebrated += (f, n) =>
                Contests.RunFestival(f.Id, (int)(Clock.DayIndex / Clock.Config.DaysPerYear), id => FindAnimal(animals, id), Population.Find, House);
            Wolves.Raided += r =>
            {
                if (r.Pack == 0) return;
                if (r.Repelled) Say($"Gece {r.Pack} kurt sürüye yaklaştı; köpekler ve çoban onları kovdu.", NoticeLevel.Info);
                else
                {
                    var names = new List<string>();
                    foreach (var a in r.Killed) names.Add(a.Name);
                    Say($"Kurtlar gece sürüye saldırdı ({r.Pack} kurt, savunma {r.Defense}): {string.Join(", ", names)} öldü.", NoticeLevel.Danger);
                    foreach (var c in Population.Members)
                        if (!c.IsDead && !c.IsVillager && c.MoodEnabled)
                            c.Mood.Add(new Thatchwick.Core.Mood.MoodletDef { Id = "mood.wolf_loss", DisplayName = "Sürüden kayıp", Description = "Kurtlar hayvanlarımızı aldı.", Value = -6, DurationDays = 2 });
                }
                foreach (var d in r.InjuredDogs) Say($"{d.Name} kurtlarla boğuşurken yaralandı; birkaç gün dinlenecek.", NoticeLevel.Info);
            };
        }

        /// <summary>Kurt saldırısı; hayvan sistemi bağlanınca kurulur.</summary>
        public WolfSystem Wolves { get; private set; }
        /// <summary>Doğa Defteri (31-6); hanedanla kalır.</summary>
        public NatureJournal Journal { get; } = new NatureJournal();
        /// <summary>Öksüz yaban yavruları (31-2).</summary>
        public WildAdoptions Adoptions { get; } = new WildAdoptions();
        private int _wolfSeed = 113;
        /// <summary>Yarışma kayıtları ve şenlik günü simülasyonu (29-4).</summary>
        public ContestBook Contests { get; } = new ContestBook();
        private static Animal FindAnimal(AnimalSystem animals, int id) { foreach (var a in animals.Animals) if (a.Id == id) return a; return null; }

        public void Attach(Market market) { market.SellBonus = SellBonus; market.ExtraOffers = Festivals.ExtraOffers; Merchant.BuyFactor = () => market.TradeBuyFactor; }
        public void Attach(Thatchwick.Core.Build.BuildService build) { build.Discount = BuildDiscount; Home.Build = build; }
        /// <summary>Depo: sandığa demir kuşak kapasite verir (belge 45).</summary>
        public void Attach(Thatchwick.Core.Build.StorageService storage) { storage.ExtraStorage = Home.ExtraStorage; }
        /// <summary>Doldurulan dekora giren yiyecek envanterdeki en eski partinin yaşını taşır.</summary>
        public void Attach(Thatchwick.Core.Work.InteractionEffects effects)
        {
            effects.FoodAge = Spoilage.OldestAge;
            effects.CropHarvested += (c, crop, tier) => Keepsakes.OnCropHarvested(c, crop, tier);
            effects.FishCaught += (c, fish) => Keepsakes.OnFishCaught(c, fish);
            effects.Applied += (c, interaction, _) => OnInteractionApplied(c, interaction);
            effects.AppliedOn += OnInteractionAppliedOn;
            effects.AppliedOn += Home.OnApplied;
            effects.ExtraBlocker = Home.CanStart;
            Home.Effects = effects;
            Coffee.Notice += (c, text) => Say(text, NoticeLevel.Info);
        }

        /// <summary>Kahve alışkanlığı ve falı (belge 32-5, 32-6).</summary>
        public CoffeeHabits Coffee { get; } = new CoffeeHabits();
        public const string FortuneInteraction = "cezve_fal";

        public void OnInteractionApplied(Character c, InteractionDef interaction)
        {
            if (c == null || interaction == null) return;
            bool coffee = false;
            foreach (var con in interaction.AllConsumed())
                if (Content.Items.TryGet(con.ItemId, out var d) && d.HasTag(CoffeeHabits.Tag)) { coffee = true; break; }
            if (coffee) Coffee.OnCoffee(c, Clock.TotalDays);
            if (interaction.Id == FortuneInteraction) Say($"{c.Name} fincanı kapattı, telveye baktı: {Coffee.Fortune()}", NoticeLevel.Info);
            if (IsHoneyHarvest(interaction)) TrySting(c);   // petek kesmede sokma OnInteractionAppliedOn'da
        }

        // ---- Bağbozumu (belge 32-8.3) ----
        public const string VintageFestival = "festival.vintage";
        public const int VintageGrapesPerTread = 4;
        public const int VintageMaxTreads = 2;
        public const int VintagePrize = 15;
        public const double VintageWinBase = 0.15;
        private readonly Random _vintageRng = new Random(887);

        /// <summary>
        /// Bağbozumu şenliği: herkese "Bağbozumu" moodlet'i; hanede üzüm varsa teknede ayakla ezilir (4 üzüm → 2 şıra, en çok 2 tekne);
        /// en iyi üzüm yarışması simüle edilir (Bahçıvanlık başına +%4, kazanırsa akçe ve kronik).
        /// </summary>
        public List<string> Vintage(double? roll = null)
        {
            var notes = new List<string>();
            foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager) m.AddMoodlet("eff_vintage");
            int treads = 0;
            while (treads < VintageMaxTreads && House.Count("item.grape") >= VintageGrapesPerTread)
            {
                for (int i = 0; i < VintageGrapesPerTread; i++) House.TryTakeItem("item.grape");
                House.AddItem("item.must", 2);
                treads++;
            }
            if (treads > 0) notes.Add($"Bağbozumunda teknede ayakla {treads * VintageGrapesPerTread} üzüm ezildi; {treads * 2} şıra çıktı.");
            var grower = BestAt(SkillType.Horticulture);
            if (grower != null && treads > 0)
            {
                double chance = Math.Min(0.6, VintageWinBase + 0.04 * grower.Skills.GetLevel(SkillType.Horticulture));
                if ((roll ?? _vintageRng.NextDouble()) < chance)
                {
                    House.AddAkce(VintagePrize);
                    string text = $"En iyi üzüm yarışmasını {grower.Name} kazandı! +{VintagePrize} akçe.";
                    grower.AddChronicle(text);
                    notes.Add(text);
                }
                else notes.Add("En iyi üzüm yarışmasında bu yıl komşunun salkımı seçildi.");
            }
            return notes;
        }

        // ---- petekli bal, zeytinyağı kandili, yadigâr zeytin (belge 32-7, 8.2) ----
        public OliveHeritage Olives { get; } = new OliveHeritage();
        public const string CombInteraction = "cut_comb";
        public const string LampFillInteraction = "fill_oil";
        /// <summary>Bir dolum (bir zeytinyağı) kandili kaç gün yakar.</summary>
        public const double LampOilDays = 3.0;
        public const double LampMaxDays = 6.0;

        public void OnInteractionAppliedOn(Character c, ItemInstance item, InteractionDef interaction)
        {
            if (item == null || interaction == null) return;
            if (interaction.Id == CombInteraction && item.Def.Id == "item.beehive")
            {
                if (Honeycomb.Cut(item, House)) Say($"{c?.Name} dumanla arıları yatıştırıp bir petek kesti.", NoticeLevel.Info);
                else Say("Kovanda kesilecek kadar bal birikmemiş.", NoticeLevel.Info);
                TrySting(c);
            }
            else if (interaction.Id == LampFillInteraction && item.Def.NeedsOil)
                item.FuelDays = Math.Min(LampMaxDays, item.FuelDays + LampOilDays);
            else if (item.Def.Id == OliveHeritage.TreeId && interaction.TakesFruit)
            {
                int extra = Olives.BonusOlives(item.Id, (int)Clock.TotalDays);
                if (extra > 0) { House.AddItem(interaction.Id == "pick_hand" ? "item.olive_fine" : "item.olive", extra); Say($"Yadigâr zeytin {extra} zeytin fazla verdi.", NoticeLevel.Info); }
            }
        }

        /// <summary>Yağlı kandiller yanarken yağ azalır.</summary>
        private void BurnLamps(double gameDays)
        {
            foreach (var it in World.Items)
                if (it.Def.NeedsOil && it.FuelDays > 0) it.FuelDays = Math.Max(0, it.FuelDays - gameDays);
        }

        private string HeadName()
        {
            Character best = null;
            foreach (var m in Population.Members)
                if (!m.IsDead && !m.IsVillager && m.Stage != AgeStage.Baby && m.Stage != AgeStage.Child && (best == null || m.AgeDays > best.AgeDays)) best = m;
            return best?.Name;
        }

        /// <summary>Bal toplarken arı sokması (32-8.2): Hayvancılık 3'ün altındaysa %25, küçük yara; ölümcül değil.</summary>
        public const int BeeSafeLevel = 3;
        public const double BeeStingChance = 0.25;
        private readonly Random _beeRng = new Random(733);
        private InteractionDef _honeyCollect;

        private bool IsHoneyHarvest(InteractionDef interaction)
        {
            if (_honeyCollect == null && Content.Items.TryGet("item.beehive", out var hive))
                foreach (var i in hive.Interactions) if (i.Collect) { _honeyCollect = i; break; }
            return _honeyCollect != null && ReferenceEquals(interaction, _honeyCollect);
        }

        public bool TrySting(Character c, double? roll = null)
        {
            if (c == null || c.IsDead || c.Skills.GetLevel(SkillType.AnimalHusbandry) >= BeeSafeLevel) return false;
            if ((roll ?? _beeRng.NextDouble()) >= BeeStingChance) return false;
            return Injuries.InjureById(c, "bee_sting") != null;
        }
        /// <summary>Kıyıdan balık: ilk balık hatırası.</summary>
        public void Attach(Thatchwick.Core.Work.FarmWork farm) { farm.FishCaught += (c, fish) => Keepsakes.OnFishCaught(c, fish); }
        private FamilyService _family;

        // ---- şenlik hazırlığı (27 §9 son madde) ----
        /// <summary>Şenlikten kaç gün önce köy süslenir.</summary>
        public const int FestivePrepDays = 3;
        public const string FestiveHomeMoodId = "mood.festive_home";
        public const int FestiveHomeMood = 5;

        /// <summary>Yaklaşan şenlik (en çok FestivePrepDays gün sonra ya da bugün); köy süslüdür. Yoksa null.</summary>
        public FestivalDef FestivePrep { get; private set; }
        private string _festivePrepAnnounced;

        /// <summary>
        /// Günlük: şenlik yaklaşınca köylüler kapılarına çelenk asar ve meydana bayrak dizisi gerilir (oyun katmanı çizer).
        /// Hanenin iç odasında mevsimi gelmiş bir mevsim süsü varsa aileye "Şenliğe hazır ev" (+5).
        /// </summary>
        public void UpdateFestivePrep()
        {
            var next = Festivals.NextFestival(out int days);
            FestivePrep = next != null && days >= 0 && days <= FestivePrepDays ? next : null;
            if (FestivePrep == null) return;
            string key = $"{FestivePrep.Id}:{Clock.DayIndex + days}";
            if (_festivePrepAnnounced != key)
            {
                _festivePrepAnnounced = key;
                Say(days == 0
                    ? $"{FestivePrep.DisplayName} bugün: köy süslendi."
                    : $"{FestivePrep.DisplayName} {days} gün sonra: köylüler kapılarına çelenk astı, meydana bayrak dizisi gerildi. Evine bir mevsim süsü as, aile \"Şenliğe hazır ev\" sevinci yaşasın.",
                    NoticeLevel.Info);
            }
            if (!HomeIsFestive()) return;
            foreach (var c in Household())
                if (!c.IsDead) c.AddMoodlet(FestiveHomeMoodId, "Şenliğe hazır ev", FestiveHomeMood, 1.1);
        }

        /// <summary>Kapalı iç odada mevsimi gelmiş (solmamış) bir mevsim süsü var mı ya da ev şenlikten önce badanalandı mı (belge 45)?</summary>
        public bool HomeIsFestive()
        {
            if (Home.WashedWithin(FestivePrepDays + 1)) return true;
            foreach (var it in World.Items)
            {
                if (!it.Def.IsSeasonal || !it.Def.InSeason(Clock.Season)) continue;
                var room = World.Map.Rooms.RoomAt(it.IsDecor && it.ParentId.HasValue && World.GetItem(it.ParentId.Value) != null ? World.GetItem(it.ParentId.Value).Position : it.Position);
                if (room != null && room.IsIndoor) return true;
            }
            return false;
        }

        /// <summary>Pazar günü gezgin tüccar köydedir: Kervan Odası eşyaları katalogda görünür (27 §6.1).</summary>
        public void UpdateMerchant(bool announce)
        {
            bool here = Festivals.IsMarketDayToday;
            bool arrived = here && !House.MerchantInVillage;
            bool left = !here && House.MerchantInVillage;
            House.MerchantInVillage = here;
            // Belge 34-D5: mevsimin ilk pazar günü panayır. Kayıt yüklenirken (announce=false) mallar kayıttan gelir.
            bool fair = Clock.DayOfSeason < FestivalService.MarketDayInterval;
            if (arrived && (announce || !Merchant.Here)) Merchant.Arrive(Clock.DayIndex, fair);
            if (left) Merchant.Leave();
            if (announce && arrived)
            {
                string order = Merchant.Order != null && Content.Items.TryGet(Merchant.Order.ItemId, out var od)
                    ? $" Siparişi: {Merchant.Order.Count} {od.DisplayName.ToLowerInvariant()}." : "";
                Say(fair ? $"Panayır kuruldu! Gezgin tüccar köyde: uzak diyar malları ve damızlık (pazar paneli).{order}"
                         : $"Gezgin tüccar köyde: uzak diyar malları (pazar paneli) ve Kervan Odası eşyaları (yapı kataloğu).{order}", NoticeLevel.Info);
            }
        }

        /// <summary>Köye camcılık geldi: cam ocağı kurulunca çömlekçi cam şişe, kavanoz ve bardak satar (27 §18.3).</summary>
        public const string GlassworksUnlock = "glassworks";
        public const string GlassFurnaceId = "item.glass_furnace";

        /// <summary>Cam ocağı dünyadaysa ve kilit kapalıysa açar, bildirir. Yeni açıldıysa true.</summary>
        public bool CheckGlassworks()
        {
            if (House.HasShopUnlock(GlassworksUnlock)) return false;
            foreach (var it in World.Items)
            {
                if (it.Def.Id != GlassFurnaceId) continue;
                House.UnlockShop(GlassworksUnlock);
                Say("Köye camcılık geldi: çömlekçi artık cam şişe, kavanoz ve bardak satıyor (yapı kataloğu, Süs).", NoticeLevel.Info);
                return true;
            }
            return false;
        }

        /// <summary>Ruh hali kaynaklarını yeniler (oyun hızından bağımsız, küçük adımlarla).</summary>
        public void Tick(double gameDays)
        {
            _moodAccumulator += gameDays;
            if (_moodAccumulator >= MoodUpdateStepDays)
            {
                double step = _moodAccumulator;
                _moodAccumulator = 0;
                Mood.Update(Population.Members);
                Watcher.Update(Population.Members, day: Clock.TotalDays);
                Breakdowns.Update(step);
            }

            Spoilage.Tick(gameDays);
            Duties.Tick(gameDays);
            Requests.Tick();
            Festivals.Tick(Population.Members);
            Decor.Tick(gameDays);
            Home.Tick(gameDays);
            Upbringing.Tick(gameDays);
            Roamers?.Tick(gameDays, Clock.TotalDays, Clock.Season, Clock.DayFraction);
            Coffee.Tick(Clock.TotalDays, Population.Members);
            BurnLamps(gameDays);

            long day = Clock.DayIndex;
            if (day != _lastDay)
            {
                OnNewDay(day);
                _lastDay = day;
            }
        }

        public Season CurrentSeason => Clock.Season;

        /// <summary>İlan tahtasının "İlanları oku" etkileşimi (items.json, item.notice_post).</summary>
        public const string ReadBoardInteraction = "read_board";

        /// <summary>Panodaki ilanları okur (okuryazar olmayan hane panonun önüne gelir; belge 46 §10).</summary>
        public int ReadBoard(Character reader)
        {
            int n = Requests.ReadBoard();
            Say(n > 0 ? $"{reader?.Name ?? "Hane"} panodan {n} ilan okudu (İstekler, I)." : "Panoda yeni ilan yok.", NoticeLevel.Info, reader?.Id ?? 0, transient: true);
            return n;
        }

        private void OnNewDay(long day)
        {
            var today = Weather.Today(Clock.TotalDays);
            Forage.OnNewDay(Clock.Season, today == WeatherType.Rain || today == WeatherType.Storm);
            _plants?.OnNewDay(today == WeatherType.Rain || today == WeatherType.Storm, today == WeatherType.Storm);
            var tomorrow = Weather.Tomorrow(Clock.TotalDays);
            if (tomorrow != WeatherType.Clear) Say(Weather.ForecastText(Clock.TotalDays), tomorrow == WeatherType.Storm ? NoticeLevel.Warning : NoticeLevel.Info);

            Ledger.Tick(day);
            Wolves?.OnNewDay(Clock.Season, (int)(day % Clock.Config.DaysPerSeason), Clock.Config.DaysPerSeason);
            var orphan = Adoptions.OnNewDay(Clock.Season);
            if (orphan != null) Say($"Tarlanın kenarında öksüz bir {orphan.Name.ToLowerInvariant()} yavrusu bulundu. Sahiplenmek ister misin?", NoticeLevel.Info);
            Coffee.OnNewDay((int)Clock.TotalDays, Population.Find);
            foreach (var note in Olives.OnNewDay((int)Clock.TotalDays, World, HeadName()))
            {
                Say(note, NoticeLevel.Info);
                foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager) m.AddChronicle(note);
            }
            var grow = Adoptions.OnGrowUp((int)Clock.TotalDays, Clock.Season);
            if (grow == "returned") Say("Büyüttüğün karaca artık yetişkin; bir sabah ormana döndü.", NoticeLevel.Info);
            else if (grow == "visit") { Say("Büyüttüğün karaca bahçenin kenarına uğradı.", NoticeLevel.Info); Roamers?.Arrive(WildCatalog.Get("wild.roe_deer"), Clock.TotalDays); }
            if (Mischief != null)
            {
                Mischief.FoxNight(Clock.Season, out var foxMsg);
                if (foxMsg != null) Say(foxMsg, foxMsg.Contains("kaptı") ? NoticeLevel.Warning : NoticeLevel.Info);
                Mischief.BoarVisit(Clock.Season, out var boarMsg);
                if (boarMsg != null) Say(boarMsg, boarMsg.Contains("çiğnendi") ? NoticeLevel.Warning : NoticeLevel.Info);
                var gift = Mischief.CrowGift(House);
                if (gift != null) Say(gift, NoticeLevel.Info);
            }
            if (Roamers != null)
            {
                Roamers.TryBearSighting(Clock.Season);
                int snares = House.Count("item.snare");
                if (snares > 0)
                    foreach (var r in Roamers.CheckSnares(Clock.Season, snares, BestAt(SkillType.Hunting), House)) Say($"Tuzağa {WildCatalog.Get(r.SpeciesId)?.Name.ToLowerInvariant()} düştü.", NoticeLevel.Info);
            }
            Villagers.PollEvents();
            Goals.Update();
            int season = (int)((day / Clock.Config.DaysPerSeason) % TimeConfig.SeasonsPerYear);
            foreach (var m in Population.Members) m.Skills.NewDay();
            if (_lastSeasonIndex >= 0 && season != _lastSeasonIndex)
            {
                OnSkillSeasonEnd();
                Wolves?.OnSeasonEnd();
                Legacy.OnSeasonChanged((Season)_lastSeasonIndex, (Season)season, day, Clock.TotalDays);
                Home.OnSeasonChanged((Season)season);   // kayıtlı mevsim seti: solanlar kalkar, yenisi asılır
                Roaming.OnSeasonChanged();
                Decor.OnSeasonChanged((Season)season);
                var guest = Suitors.OnNewSeason();
                if (guest != null) Say(Suitors.LastWasNotable ? $"{guest.Name} köye geldi; ailenin itibarını ve çeyizini duymuş, becerili biri." : $"{guest.Name} köye geldi.", NoticeLevel.Info, guest.Id);
                Villagers.ReplaceDeparted();   // ölen adlı köylünün (demirci ...) yerine halefi mevsim başında gelir
            }
            _lastSeasonIndex = season;
            int arrivals = Suitors.Ensure();
            if (arrivals > 0) Say($"Köye {arrivals} yeni kişi yerleşti.", NoticeLevel.Info);

            var report = Forecast.Report(Clock.TotalDays);
            if (report.Warnings.Count > 0)
                Say(report.Warnings[0], report.HasDanger ? NoticeLevel.Danger : NoticeLevel.Warning);
            DailyHealing();
            Injuries.DailyUpdate();
            Decor.RemindFreeMirror();
            Home.OnNewDay();
            Keepsakes.Daily(Household());
            CheckGlassworks();
            UpdateFestivePrep();
            UpdateMerchant(true);
            // Miras: dünyadan kalkmış yadigârlar düşer; komşu hanelerde soy sürer; köydeki soy eşikleri.
            Legacy.Validate(World.GetItem);
            foreach (var w in Legacy.NeighborFamiliesDaily()) Say($"{w.Name} çocuk bekliyor.", NoticeLevel.Info, w.Id);
            // Komşu hanedanlar: hanesizleri yerleştir, ölen başın yerine varis, köyde düğün; dükkân yetişkin varise hemen geçer.
            Houses.OnNewDay();
            Homes.AssignAll();
            Villagers.ReplaceDeparted(inheritOnly: true);
            var founder = FounderId != null ? FounderId() : null;
            if (founder.HasValue) Legacy.CheckDescendants(founder.Value, Clock.TotalDays);
            Upbringing.OnNewDay();
            Requests.OnNewDay();
            NewDay?.Invoke(day);
        }

        /// <summary>Yeni gün işlendikten sonra (ekin, hava, köy) tetiklenir: çiftlik otomasyonu, otlak, hayvan konforu buraya bağlanır.</summary>
        public event Action<long> NewDay;

        /// <summary>Bugün dışarısı hayvanlar için uygun mu (fırtına yok)?</summary>
        public bool CalmToday => Weather.Today(Clock.TotalDays) != WeatherType.Storm;

        /// <summary>Bugün yağmur ya da fırtına var mı (yol çamurlanır, daha çok aşınır)?</summary>
        public bool RainyToday => Weather.Today(Clock.TotalDays) == WeatherType.Rain || Weather.Today(Clock.TotalDays) == WeatherType.Storm;

        // ---- kayıt ----

        public LifeSaveData Export()
        {
            var data = new LifeSaveData
            {
                weatherSeed = Weather.Seed,
                lastProcessedDay = _lastDay,
                festivalCelebratedDay = Festivals.CelebratedDay,
                spoilage = Spoilage.Export().ToArray(),
                tools = Tools.Export().ToArray(),
                projectsDone = new List<string>(Projects.Done).ToArray(),
                goalsDone = new List<string>(Goals.Done).ToArray(),
                villagers = Villagers.Export().ToArray(),
                marriages = Marriages,
                festivalsCelebrated = FestivalsCelebrated,
                accidentSetting = (int)Injuries.Setting,
                legacy = Legacy.Export(),
                houses = Houses.Export(),
                lastHouseholdAccidentDay = Injuries.LastHouseholdAccidentDay,
                upbringing = Upbringing.Export(),
                merchant = Merchant.Export(),
                natureJournal = Journal.Export(),
                adoptedWild = Adoptions.Export(),
                coffeeHabits = Coffee.Export(),
                olives = Olives.Export(),
                contests = Contests.Export(),
                wolfRaidIn = Wolves != null ? Wolves.RaidInDays : -1,
                wolfHunted = Wolves != null ? Wolves.HuntedThisSeason : 0,
                keepsakes = Keepsakes.Export(),
                home = Home.Export(),
                timedDuties = Duties.Export(),
                roaming = Roaming.Export(),
                requests = Requests.Export()
            };
            var graves = new List<GraveSave>();
            foreach (var g in Graves.Graves)
                graves.Add(new GraveSave
                {
                    characterId = g.CharacterId, name = g.Name, generation = g.Generation, sex = (int)g.Sex, cause = (int)g.Cause,
                    itemId = g.ItemId, x = g.Position.X, y = g.Position.Y, deathDay = g.DeathDay, ageDays = g.AgeDays
                });
            data.graves = graves.ToArray();
            return data;
        }

        public void Restore(LifeSaveData data)
        {
            if (data == null) { Home.Import(null); Population.RecomputeAll(); return; }
            Weather.Seed = data.weatherSeed == 0 ? Weather.Seed : data.weatherSeed;
            Festivals.CelebratedDay = data.festivalCelebratedDay;
            Upbringing.Import(data.upbringing);
            Merchant.Import(data.merchant);
            Journal.Import(data.natureJournal);
            Adoptions.Import(data.adoptedWild);
            Coffee.Import(data.coffeeHabits);
            Olives.Import(data.olives);
            Contests.Import(data.contests);
            Wolves?.Restore(data.wolfRaidIn, data.wolfHunted);
            Spoilage.Restore(data.spoilage);
            Tools.Restore(data.tools);
            Projects.Restore(data.projectsDone);
            Goals.Restore(data.goalsDone);
            Villagers.Restore(data.villagers);
            Legacy.Restore(data.legacy);
            Houses.Restore(data.houses);
            Homes.AssignAll();
            Keepsakes.Restore(data.keepsakes);
            Home.Import(data.home);
            Duties.Import(data.timedDuties);
            Roaming.Import(data.roaming);
            Requests.Import(data.requests);
            UpdateMerchant(false);
            Marriages = Math.Max(0, data.marriages);
            if (Enum.IsDefined(typeof(AccidentSetting), data.accidentSetting)) Injuries.Setting = (AccidentSetting)data.accidentSetting;
            Injuries.LastHouseholdAccidentDay = data.lastHouseholdAccidentDay;
            // Günlük işler (sulama sıfırlama, kuzgun, fırtına, toplama noktaları) kayıttaki gün için yapılmışsa yeniden yapılmaz.
            // Eski kayıtta alan yoktur (-1): eskisi gibi yüklemeden sonraki ilk turda bir kez çalışır.
            if (data.lastProcessedDay >= 0 && data.lastProcessedDay <= Clock.DayIndex)
            {
                _lastDay = data.lastProcessedDay;
                _lastSeasonIndex = (int)((_lastDay / Clock.Config.DaysPerSeason) % TimeConfig.SeasonsPerYear);
                if (_lastDay == Clock.DayIndex)
                {
                    Festivals.AnnouncedDay = _lastDay;
                    Ledger.Tick(_lastDay);       // gün özeti yüklenen günden saymaya başlar
                }
            }
            FestivalsCelebrated = Math.Max(0, data.festivalsCelebrated);
            var graves = new List<GraveRecord>();
            if (data.graves != null)
                foreach (var g in data.graves)
                    if (g != null)
                        graves.Add(new GraveRecord
                        {
                            CharacterId = g.characterId, Name = g.name, Generation = g.generation,
                            Sex = Enum.IsDefined(typeof(Sex), g.sex) ? (Sex)g.sex : Sex.Female,
                            Cause = Enum.IsDefined(typeof(DeathCause), g.cause) ? (DeathCause)g.cause : DeathCause.None,
                            ItemId = g.itemId, Position = new GridPos(g.x, g.y), DeathDay = g.deathDay, AgeDays = g.ageDays
                        });
            Graves.Restore(graves);
            Population.RecomputeAll();
        }
    }
}
