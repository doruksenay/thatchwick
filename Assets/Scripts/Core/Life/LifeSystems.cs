using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
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
            Spoilage.Spoiled += (id, n) =>
                Say($"{n} {(content.Items.TryGet(id, out var d) ? d.DisplayName : id)} bozuldu.", NoticeLevel.Warning);
            Festivals.Announced += f => Say($"Bugün {f.DisplayName}!", NoticeLevel.Info);
            Festivals.Celebrated += (f, n) =>
            {
                Say($"{f.DisplayName}: {n} kişi şenliğe katıldı.", NoticeLevel.Info);
                Ledger.Note(f.DisplayName);
            };
            Livestock.BecameReady += item => Say($"{item.Def.DisplayName}: ürün hazır.", NoticeLevel.Info);
            Watcher.Raised += (level, text, id) => Say(text, level, id);
            population.BabyBorn += (baby, mother, father) =>
            {
                Ledger.OnBorn(baby);
                Say($"{baby.Name} doğdu!", NoticeLevel.Info, baby.Id);
            };

            population.Content = content;
            population.Village = Village;
            population.MemberDied += OnMemberDied;
        }

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
            var room = World.Map.Rooms.RoomAt(c.Position);
            bool outdoors = room == null || !room.IsIndoor;
            if (outdoors) f *= WeatherSystem.OutdoorWorkSpeed(Weather.Today(Clock.TotalDays));
            return f;
        }

        // ---- diğer sistemlere bağlama (oyun katmanı kurarken bir kez çağırır) ----

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
                spoilage = Spoilage.Export().ToArray()
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
