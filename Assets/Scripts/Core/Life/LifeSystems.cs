using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
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
        public LivestockSystem Livestock { get; }

        // Köy ve hanedan katmanı (Adım 38-41)
        public VillagerService Villagers { get; }
        public ToolService Tools { get; }
        public ProjectService Projects { get; }
        public GoalService Goals { get; }
        /// <summary>Evlilik ve kutlanan şenlik sayıları (hedef ölçütleri için).</summary>
        public int Marriages { get; private set; }
        public int FestivalsCelebrated { get; private set; }
        /// <summary>Hesap seviyesi sağlayıcı (alet yükseltmesi); null ise sınır yok.</summary>
        public Func<int> AccountLevel { get; set; }

        /// <summary>Yaşam olayları (düz metin): mezar, yas vb. Ayrıntılı akış için Log.</summary>
        public event Action<string> Notice;

        private double _moodAccumulator;
        private long _lastDay = -1;
        private int _lastSeasonIndex = -1;
        public const double MoodUpdateStepDays = 0.02;

        public LifeSystems(ContentDatabase content, WorldState world, Household house, Population population, GameClock clock)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            World = world ?? throw new ArgumentNullException(nameof(world));
            House = house ?? throw new ArgumentNullException(nameof(house));
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Mood = new MoodService(world);
            Graves = new GraveService(world, content);
            Ledger = new DayLedger(house);
            Forecast = new WinterForecast(content, house, world, population, clock.Config);
            Suitors = new SuitorPool(content, population);
            Weather = new WeatherSystem(clock.Config, seed: 1);
            Festivals = new FestivalService(content, clock) { Village = Village };
            Spoilage = new SpoilageService(content, house, world) { ExtraReduction = SpoilReduction };
            Forage = new ForageSystem(world, content);
            Livestock = new LivestockSystem(world);
            Villagers = new VillagerService(content, population, house, clock);
            Tools = new ToolService(content, house)
            {
                AccountLevel = () => AccountLevel != null ? AccountLevel() : int.MaxValue,
                SmithAvailable = () => !HasVillagerRole(RoleSmithDefined) || Villagers.HasRole(VillagerService.RoleBlacksmith)
            };
            Projects = new ProjectService(content, house, Village);
            Goals = new GoalService(content) { Metric = MetricValue };
            Spoilage.Spoiled += (id, n) =>
                Say($"{n} {(content.Items.TryGet(id, out var d) ? d.DisplayName : id)} bozuldu.", NoticeLevel.Warning);
            Festivals.Announced += f => Say($"Bugün {f.DisplayName}!", NoticeLevel.Info);
            Festivals.Celebrated += (f, n) =>
            {
                FestivalsCelebrated++;
                Say($"{f.DisplayName}: {n} kişi şenliğe katıldı.", NoticeLevel.Info);
                Ledger.Note(f.DisplayName);
                Goals.Update();
            };
            Tools.Upgraded += (tool, tier) => Say($"{tier.Name} hazır.", NoticeLevel.Info);
            Projects.Completed += p =>
            {
                Population.RecomputeAll();
                Say($"Köy projesi tamamlandı: {p.DisplayName}.", NoticeLevel.Info);
                Ledger.Note(p.DisplayName);
                Goals.Update();
            };
            Goals.Completed += OnGoalCompleted;
            Villagers.EventTriggered += (v, e) => Say($"{v.Name}: {e.Text}", NoticeLevel.Info, v.Id);
            Villagers.GiftGiven += (v, item, taste, msg) => Say(msg, taste == GiftTaste.Dislike ? NoticeLevel.Warning : NoticeLevel.Info, v.Id);
            Livestock.BecameReady += item => Say($"{item.Def.DisplayName}: ürün hazır.", NoticeLevel.Info);
            Watcher.Raised += (level, text, id) => Say(text, level, id);
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

        /// <summary>Bildirim akışına ve düz metin olayına yazar. Dış sistemler (aile, pazar) bunu çağırabilir.</summary>
        public void Say(string message, NoticeLevel level = NoticeLevel.Info, int characterId = 0)
        {
            Log.Add(Clock.TotalDays, level, message, characterId);
            Notice?.Invoke(message);
        }

        private void OnMemberDied(Character dead)
        {
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
        public double SellBonus() => Math.Min(0.75, BestOf(m => m.SellBonus) + Village.MarketSell);
        public float BuildDiscount() => Math.Min(0.5f, BestOf(m => m.BuildDiscount));

        /// <summary>Ekin büyüme çarpanı: hava x (1 + köy) x en iyi yeşil parmak.</summary>
        public float GrowthMultiplier() =>
            WeatherSystem.GrowthMultiplier(Weather.Today(Clock.TotalDays)) * (1f + Village.CropGrowth) * BestOf(m => m.GrowthSpeed);

        /// <summary>İş hızı çarpanı (JobRunner.SpeedProvider): dışarıda hava durumu.</summary>
        public double WorkSpeedFor(Character c, InteractionDef interaction)
        {
            double f = 1.0;
            if (!string.IsNullOrEmpty(interaction.Tool)) f *= Tools.SpeedFor(interaction.Tool);
            var room = World.Map.Rooms.RoomAt(c.Position);
            bool outdoors = room == null || !room.IsIndoor;
            if (outdoors) f *= WeatherSystem.OutdoorWorkSpeed(Weather.Today(Clock.TotalDays));
            return f;
        }

        // ---- diğer sistemlere bağlama (oyun katmanı kurarken bir kez çağırır) ----

        /// <summary>Aile sistemine bağlanır: köylü hediye/yakınlık kuralları ve evlilik sayacı.</summary>
        public void Attach(FamilyService family)
        {
            Villagers.Family = family;
            family.ExtraProposalCheck = Villagers.ProposalCheck;
            family.Happened += (actor, target, action, msg) =>
            {
                if (action != SocialAction.Propose) return;
                Marriages++;
                Goals.Update();
            };
        }

        public void Attach(JobRunner runner) { runner.SpeedProvider = WorkSpeedFor; }
        public void Attach(PlantSystem plants) { plants.GrowthMultiplier = GrowthMultiplier; }
        public void Attach(HeatSystem heat) { heat.OutdoorOffset = () => WeatherSystem.TemperatureOffset(Weather.Today(Clock.TotalDays)); }
        public void Attach(Market market) { market.SellBonus = SellBonus; market.ExtraOffers = Festivals.ExtraOffers; }
        public void Attach(Thatchwick.Core.Build.BuildService build) { build.Discount = BuildDiscount; }

        /// <summary>Ruh hali kaynaklarını yeniler (oyun hızından bağımsız, küçük adımlarla).</summary>
        public void Tick(double gameDays)
        {
            _moodAccumulator += gameDays;
            if (_moodAccumulator >= MoodUpdateStepDays)
            {
                _moodAccumulator = 0;
                Mood.Update(Population.Members);
                Watcher.Update(Population.Members);
            }

            Spoilage.Tick(gameDays);
            Livestock.Tick(gameDays, CurrentSeason);
            Festivals.Tick(Population.Members);

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
            Forage.OnNewDay(Clock.Season);
            var tomorrow = Weather.Tomorrow(Clock.TotalDays);
            if (tomorrow != WeatherType.Clear) Say(Weather.ForecastText(Clock.TotalDays), tomorrow == WeatherType.Storm ? NoticeLevel.Warning : NoticeLevel.Info);

            Ledger.Tick(day);
            Villagers.PollEvents();
            Goals.Update();
            int season = (int)((day / Clock.Config.DaysPerSeason) % TimeConfig.SeasonsPerYear);
            if (_lastSeasonIndex >= 0 && season != _lastSeasonIndex)
            {
                var guest = Suitors.OnNewSeason();
                if (guest != null) Say($"{guest.Name} köye geldi.", NoticeLevel.Info, guest.Id);
            }
            _lastSeasonIndex = season;
            int arrivals = Suitors.Ensure();
            if (arrivals > 0) Say($"Köye {arrivals} yeni kişi yerleşti.", NoticeLevel.Info);

            var report = Forecast.Report(Clock.TotalDays);
            if (report.Warnings.Count > 0)
                Say(report.Warnings[0], report.HasDanger ? NoticeLevel.Danger : NoticeLevel.Warning);
        }

        // ---- kayıt ----

        public LifeSaveData Export()
        {
            var data = new LifeSaveData
            {
                weatherSeed = Weather.Seed,
                festivalCelebratedDay = Festivals.CelebratedDay,
                spoilage = Spoilage.Export().ToArray(),
                tools = Tools.Export().ToArray(),
                projectsDone = new List<string>(Projects.Done).ToArray(),
                goalsDone = new List<string>(Goals.Done).ToArray(),
                villagers = Villagers.Export().ToArray(),
                marriages = Marriages,
                festivalsCelebrated = FestivalsCelebrated
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
            Spoilage.Restore(data.spoilage);
            Tools.Restore(data.tools);
            Projects.Restore(data.projectsDone);
            Goals.Restore(data.goalsDone);
            Villagers.Restore(data.villagers);
            Marriages = Math.Max(0, data.marriages);
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
