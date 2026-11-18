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

        /// <summary>Hanedan kurucusunun kimliği (köydeki soy sayımı); oyun katmanı bağlar. Null ise sayım yapılmaz.</summary>
        public Func<int?> FounderId { get; set; }
        /// <summary>Kazalar ve kazanılmış izler (doküman 14, 3F).</summary>
        public InjurySystem Injuries { get; }
        /// <summary>Ev dekorasyonu: raf çökmesi, ayna kararması, ayna hatırlatması (Project belgesi 27).</summary>
        public Thatchwick.Core.Build.DecorSystem Decor { get; }
        /// <summary>Hatıralar, soy ağacı işlemesi ve çeyiz sandığı (Project belgesi 27, D3b).</summary>
        public KeepsakeService Keepsakes { get; }
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

            Mood = new MoodService(world) { Content = content };
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
            Mood.BeautyBonus = it => Keepsakes.BeautyBonus(it) + Thatchwick.Core.Build.DecorSystem.SeasonalBeauty(it, Clock.Season);
            Legacy.MarriedOutHappened += (member, villager) => Keepsakes.OnMarriedOut(member, villager, _family, Legacy, Household());
            population.BabyBorn += (baby, mother, father) => Keepsakes.OnBirth(baby);
            Houses = new NeighborHouseService(population, Villagers, content, seed != 0 ? Seeds.Derive(seed, 9) : 71) { Today = () => Clock.TotalDays };
            Houses.HeadChanged += (h, heir, old) => Say($"{h.Name}: {heir.Name} hanenin başına geçti{(old != null ? $" ({old.Name} yerine)" : "")}.", NoticeLevel.Info, heir.Id);
            Houses.VillagersMarried += (a, b) => Say($"Köyde düğün: {a.Name} ile {b.Name} evlendi.", NoticeLevel.Info, a.Id);
            Houses.HouseEnded += h => Say($"{h.Name}nin soyu tükendi.", NoticeLevel.Info);
            Legacy.MarriedOutHappened += (member, villager) => Houses.JoinSpouses(member, villager);
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
                Keepsakes.OnFestival(f.DisplayName, n);
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
        // Ev düzenleme: hanenin en iyi süslemecisi seviye başına %2 indirim (10. seviyede %18) dallarla toplanır.
        public float BuildDiscount() => Math.Min(0.5f, BestOf(m => m.BuildDiscount) + 0.02f * Math.Max(0, BestSkill(SkillType.Decorating) - 1));

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
            WeatherSystem.GrowthMultiplier(Weather.Today(Clock.TotalDays)) * (1f + Village.CropGrowth) * BestOf(m => m.GrowthSpeed);

        /// <summary>İş hızı çarpanı (JobRunner.SpeedProvider): dışarıda hava durumu.</summary>
        public double WorkSpeedFor(Character c, InteractionDef interaction)
        {
            double f = 1.0;
            if (!string.IsNullOrEmpty(interaction.Tool)) f *= Tools.SpeedFor(interaction.Tool);
            if (c.HasBranch(GlassBranchId) && ProducesGlass(interaction)) f *= GlassBranchSpeed;   // Camcı dalı (27 §18.3)
            var room = World.Map.Rooms.RoomAt(c.Position);
            bool outdoors = room == null || !room.IsIndoor;
            if (outdoors) f *= WeatherSystem.OutdoorWorkSpeed(Weather.Today(Clock.TotalDays));
            return f;
        }

        /// <summary>Çömlekçilik altındaki Camcı dalı: "glass" etiketli ürün veren işler bu kadar hızlı.</summary>
        public const string GlassBranchId = "branch.pot_b";   // "Camcı" (eski adı Çarkçı Çömlekçi; kayıtlar korunur)
        public const string AshId = "item.ash";
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
            _family = family;
            family.JoinedHousehold += c => Keepsakes.OnJoined(c);
            Villagers.Family = family;
            family.ExtraProposalCheck = Villagers.ProposalCheck;
            family.Happened += (actor, target, action, msg) =>
            {
                if (action != SocialAction.Propose) return;
                Marriages++;
                Upbringing.Reputation.Add(Thatchwick.Core.Upbringing.ReputationSystem.Reasons.Wedding, "düğün");
                Goals.Update();
            };
        }

        public void Attach(JobRunner runner)
        {
            runner.SpeedProvider = WorkSpeedFor;
            runner.SquarePosition = () => SquarePosition?.Invoke();
            runner.Refused += (c, text) => Say($"{c.Name}: \"{text}\"", NoticeLevel.Info, c.Id, transient: true);
            runner.ItemBonus = Legacy.BonusFor;
            runner.InteractionCompleted += (c, interaction) => Injuries.OnWorkDone(c, interaction);
            runner.ItemUsed += (c, item, interaction) => Upbringing.OnItemUsed(c, item);
        }
        /// <summary>Hayvanlar: can yoldaşı bağı (yakınlık, bakım, ölüm), kalite ve binicilik bonusu.</summary>
        public void Attach(Thatchwick.Core.Animals.AnimalSystem animals, Thatchwick.Core.Animals.AnimalCare care) => Upbringing.Attach(animals, care);

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
            Spoilage.TemperatureAt = heat.TemperatureAt;   // bodrum mahzeni serinlik şartı
            Decor.TemperatureAt = heat.TemperatureAt;      // sepetteki meyve oda sıcaklığında bozulur
            if (Content.Items.Contains(AshId)) heat.AshItemId = AshId;   // ocağa atılan odunun külü (cam zinciri, 27 §18.3)
            if (_plants != null) _plants.TemperatureAt = heat.TemperatureAt;
        }
        private PlantSystem _plants;
        private HeatSystem _heat;
        /// <summary>Hayvanlar: attan düşen binici yaralanır (ölümcül değil), at ürkünce bildirim.</summary>
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

        public void Attach(Market market) { market.SellBonus = SellBonus; market.ExtraOffers = Festivals.ExtraOffers; }
        public void Attach(Thatchwick.Core.Build.BuildService build) { build.Discount = BuildDiscount; }
        /// <summary>Doldurulan dekora giren yiyecek envanterdeki en eski partinin yaşını taşır.</summary>
        public void Attach(Thatchwick.Core.Work.InteractionEffects effects)
        {
            effects.FoodAge = Spoilage.OldestAge;
            effects.CropHarvested += (c, crop, tier) => Keepsakes.OnCropHarvested(c, crop, tier);
            effects.FishCaught += (c, fish) => Keepsakes.OnFishCaught(c, fish);
        }
        /// <summary>Kıyıdan balık: ilk balık hatırası.</summary>
        public void Attach(Thatchwick.Core.Work.FarmWork farm) { farm.FishCaught += (c, fish) => Keepsakes.OnFishCaught(c, fish); }
        private FamilyService _family;

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
            Festivals.Tick(Population.Members);
            Decor.Tick(gameDays);
            Upbringing.Tick(gameDays);

            long day = Clock.DayIndex;
            if (day != _lastDay)
            {
                OnNewDay(day);
                _lastDay = day;
            }
        }

        public Season CurrentSeason => Clock.Season;

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
            Villagers.PollEvents();
            Goals.Update();
            int season = (int)((day / Clock.Config.DaysPerSeason) % TimeConfig.SeasonsPerYear);
            foreach (var m in Population.Members) m.Skills.NewDay();
            if (_lastSeasonIndex >= 0 && season != _lastSeasonIndex)
            {
                OnSkillSeasonEnd();
                Wolves?.OnSeasonEnd();
                Legacy.OnSeasonChanged((Season)_lastSeasonIndex, (Season)season, day, Clock.TotalDays);
                Decor.OnSeasonChanged((Season)season);
                var guest = Suitors.OnNewSeason();
                if (guest != null) Say($"{guest.Name} köye geldi.", NoticeLevel.Info, guest.Id);
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
            Keepsakes.Daily(Household());
            // Miras: dünyadan kalkmış yadigârlar düşer; komşu hanelerde soy sürer; köydeki soy eşikleri.
            Legacy.Validate(World.GetItem);
            foreach (var w in Legacy.NeighborFamiliesDaily()) Say($"{w.Name} çocuk bekliyor.", NoticeLevel.Info, w.Id);
            // Komşu hanedanlar: hanesizleri yerleştir, ölen başın yerine varis, köyde düğün; dükkân yetişkin varise hemen geçer.
            Houses.OnNewDay();
            Villagers.ReplaceDeparted(inheritOnly: true);
            var founder = FounderId != null ? FounderId() : null;
            if (founder.HasValue) Legacy.CheckDescendants(founder.Value, Clock.TotalDays);
            Upbringing.OnNewDay();
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
                natureJournal = Journal.Export(),
                adoptedWild = Adoptions.Export(),
                contests = Contests.Export(),
                wolfRaidIn = Wolves != null ? Wolves.RaidInDays : -1,
                wolfHunted = Wolves != null ? Wolves.HuntedThisSeason : 0,
                keepsakes = Keepsakes.Export()
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
            if (data == null) { Population.RecomputeAll(); return; }
            Weather.Seed = data.weatherSeed == 0 ? Weather.Seed : data.weatherSeed;
            Festivals.CelebratedDay = data.festivalCelebratedDay;
            Upbringing.Import(data.upbringing);
            Journal.Import(data.natureJournal);
            Adoptions.Import(data.adoptedWild);
            Contests.Import(data.contests);
            Wolves?.Restore(data.wolfRaidIn, data.wolfHunted);
            Spoilage.Restore(data.spoilage);
            Tools.Restore(data.tools);
            Projects.Restore(data.projectsDone);
            Goals.Restore(data.goalsDone);
            Villagers.Restore(data.villagers);
            Legacy.Restore(data.legacy);
            Houses.Restore(data.houses);
            Keepsakes.Restore(data.keepsakes);
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
