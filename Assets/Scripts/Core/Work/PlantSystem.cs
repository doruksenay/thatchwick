using System;
using System.Collections.Generic;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>Bir ekin karosunun toprağı: verim (0-1) ve oraya en son ekilen ekin (nöbetleşe ekim için).</summary>
    public sealed class SoilPlot
    {
        public float Fertility;
        public string LastCrop;
    }

    /// <summary>Çiftçilik ayarları: sayılar tek yerde, testlerde değiştirilebilir.</summary>
    public sealed class FarmConfig
    {
        /// <summary>Hiç işlenmemiş toprağın verimi.</summary>
        public float DefaultFertility = 0.5f;
        /// <summary>Sulanmamış ekin bu hızla büyür (sulu 1).</summary>
        public float DryGrowth = 0.3f;
        /// <summary>Büyüme çarpanı = Base + Span x verim (verim 0,5 iken 1).</summary>
        public float FertilityBase = 0.75f, FertilitySpan = 0.5f;
        /// <summary>Hasat toprağı yorar; farklı ekin sonra gelirse nefes alır; baklagil zenginleştirir; aynı ekin yorar.</summary>
        public float HarvestDraw = 0.12f, RotationBonus = 0.08f, NitrogenBonus = 0.2f, MonocropPenalty = 0.06f;
        /// <summary>Boş toprak günde bu kadar varsayılan verime döner.</summary>
        public float FallowRecovery = 0.02f;
        /// <summary>Kalite puanı = sulu oran x WetWeight + verim x FertWeight + beceri x SkillWeight.</summary>
        public float WetWeight = 0.55f, FertWeight = 0.35f, SkillWeight = 0.01f;
        public float FineScore = 0.55f, FinestScore = 0.78f;
        /// <summary>Sera: ışık için pencere oranı ve ısı sınırları (°C).</summary>
        public int GreenhouseMinWindows = 2;
        public float GreenhouseTilesPerWindow = 3f;
        public float GreenhouseMinTemp = 5f, GreenhouseFullTemp = 14f, FrostTemp = 0f;
        /// <summary>Korkuluksuz ekini bir gecede kuzgunlar yer; fırtına büyümeyi geri alır.</summary>
        public double CrowChance = 0.025, StormChance = 0.12;
        public float StormGrowthLoss = 0.3f;
        /// <summary>Çalışan bir kovan yakındaysa ekin büyümesi ve meyve olgunlaşması bu kadar artar.</summary>
        public float PollinationGrowth = 0.1f, PollinationFruit = 0.25f;
    }

    public enum PlantLoss { Crows, Storm, Frost }

    /// <summary>Tarlanın özeti (arayüz şeridi): ekin sayısı, susamış, hasada hazır, meyvesi olgun ağaç.</summary>
    public readonly struct FarmSummary
    {
        public readonly int Crops, Thirsty, Ripe, FruitReady;
        public FarmSummary(int crops, int thirsty, int ripe, int fruitReady) { Crops = crops; Thirsty = thirsty; Ripe = ripe; FruitReady = fruitReady; }
        public bool Any => Crops > 0 || FruitReady > 0;
    }

    /// <summary>
    /// Bitki büyümesi ve çiftçilik: ekinler (sulama, toprak verimi, nöbetleşe ekim, kalite), sera (ışıklı ve
    /// ısıtılmış oda kışın da ekin büyütür), meyve ağaçları (olgun ağaç mevsiminde tekrar tekrar meyve verir),
    /// sulama hendeği ve korkuluk. Ağaçlar sulanmaz; kış dışında kendiliğinden büyür.
    /// </summary>
    public sealed class PlantSystem
    {
        public const string QualityNormal = "Sıradan", QualityFine = "Seçme", QualityFinest = "Birinci sınıf";

        private readonly WorldState _world;
        private readonly Dictionary<GridPos, SoilPlot> _soil = new Dictionary<GridPos, SoilPlot>();
        private readonly Random _rng;

        public FarmConfig Config { get; } = new FarmConfig();

        public PlantSystem(WorldState world, int seed = 31)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _rng = new Random(seed);
        }

        public static bool CanGrowIn(Season season) => season != Season.Winter;

        /// <summary>Hava ve köy projelerinden gelen büyüme çarpanı (yağmur +, kuyu +); yoksa 1.</summary>
        public Func<float> GrowthMultiplier { get; set; }

        /// <summary>Bu kovanda yaşayan koloni var mı (oyun katmanı hayvan sistemine bağlar); null ise kovan her zaman çalışır sayılır.</summary>
        public Func<ItemInstance, bool> HiveActive { get; set; }

        /// <summary>Yakında çalışan bir kovan var mı.</summary>
        public bool IsPollinated(GridPos p)
        {
            foreach (var it in _world.Items)
            {
                int r = it.Def.PollinateRadius;
                if (r <= 0 || !Near(it.Position, p, r)) continue;
                if (HiveActive == null || HiveActive(it)) return true;
            }
            return false;
        }

        /// <summary>Bir karonun sıcaklığı (°C); sera için. Yoksa sera her zaman yeterince sıcak sayılır.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }

        /// <summary>Bitki bu mevsimde büyür mü: kış hiç; ekin türünün growSeasons listesi varsa yalnızca onlar.</summary>
        public static bool CanGrow(Content.ItemDef def, Season season) =>
            CanGrowIn(season) && (def.GrowSeasons.Count == 0 || def.GrowSeasons.Contains(season));

        /// <summary>Kışın donan ya da kuzgunların yediği ekin kalktığında (eşya tanımı, konum, neden) tetiklenir.</summary>
        public event Action<Content.ItemDef, GridPos> PlantDied;
        public event Action<Content.ItemDef, GridPos, PlantLoss> PlantLost;

        /// <summary>Pencereli, geniş ışıklı iç mekân: ekin bu odada büyür (ısı ayrıca gerekir).</summary>
        public bool IsGreenhouse(Room room) =>
            room != null && room.IsIndoor && room.Level == 0
            && room.WindowCount >= Config.GreenhouseMinWindows
            && room.WindowCount * Config.GreenhouseTilesPerWindow >= room.Area;

        /// <summary>Tarla özeti: kaç ekin var, kaçı susamış (büyüyor ama sulanmamış), kaçı hasada hazır, kaç ağacın meyvesi olgun.</summary>
        public FarmSummary Summary()
        {
            int crops = 0, thirsty = 0, ripe = 0, fruit = 0;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                if (item.Def.IsCrop)
                {
                    crops++;
                    if (item.IsGrown) ripe++; else if (!item.Watered) thirsty++;
                }
                if (item.FruitReady) fruit++;
            }
            return new FarmSummary(crops, thirsty, ripe, fruit);
        }

        // ---------- toprak ----------

        public IEnumerable<KeyValuePair<GridPos, SoilPlot>> SoilPlots => _soil;

        public float FertilityAt(GridPos p) =>
            _soil.TryGetValue(p.OnLevel(0), out var plot) ? plot.Fertility : Config.DefaultFertility;

        public string LastCropAt(GridPos p) => _soil.TryGetValue(p.OnLevel(0), out var plot) ? plot.LastCrop : null;

        public void SetSoil(GridPos p, float fertility, string lastCrop)
        {
            var plot = PlotAt(p);
            plot.Fertility = Clamp01(fertility);
            plot.LastCrop = string.IsNullOrEmpty(lastCrop) ? null : lastCrop;
        }

        private SoilPlot PlotAt(GridPos p)
        {
            p = p.OnLevel(0);
            if (!_soil.TryGetValue(p, out var plot))
            {
                plot = new SoilPlot { Fertility = Config.DefaultFertility };
                _soil[p] = plot;
            }
            return plot;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        /// <summary>Gübre ya da kompost: karonun verimini artırır (en çok 1).</summary>
        public void Fertilize(GridPos p, float amount)
        {
            var plot = PlotAt(p);
            plot.Fertility = Clamp01(plot.Fertility + amount);
        }

        /// <summary>Ekini sular (bugünlük). Ekin dışındakiler için etkisizdir.</summary>
        public static void Water(ItemInstance item)
        {
            if (item != null && item.IsPlant && item.Def.IsCrop) item.Watered = true;
        }

        /// <summary>Olgun ekin hasat edilince toprak yorulur ya da (baklagil, nöbetleşe ekim) toparlanır.</summary>
        public void OnHarvest(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return;
            var plot = PlotAt(item.Position);
            float delta = -Config.HarvestDraw;
            if (item.Def.FixesNitrogen) delta += Config.NitrogenBonus;
            if (plot.LastCrop != null)
                delta += plot.LastCrop == item.Def.Id ? -Config.MonocropPenalty : Config.RotationBonus;
            plot.Fertility = Clamp01(plot.Fertility + delta);
            plot.LastCrop = item.Def.Id;
        }

        // ---------- kalite ----------

        /// <summary>0 sıradan, 1 seçme, 2 birinci sınıf. Sulu geçen süre, toprak verimi ve çiftçilik seviyesi belirler.</summary>
        public static int QualityTier(FarmConfig cfg, float wetRatio, float fertility, int skillLevel)
        {
            float score = wetRatio * cfg.WetWeight + fertility * cfg.FertWeight + Math.Min(10, Math.Max(0, skillLevel)) * cfg.SkillWeight;
            return score >= cfg.FinestScore ? 2 : (score >= cfg.FineScore ? 1 : 0);
        }

        public int QualityOf(ItemInstance item, int skillLevel) =>
            item == null || !item.Def.IsCrop ? 0 : QualityTier(Config, item.WetRatio, FertilityAt(item.Position), skillLevel);

        public static string QualityName(int tier) => tier >= 2 ? QualityFinest : (tier == 1 ? QualityFine : QualityNormal);

        /// <summary>Verim çarpanı: seçme +%25, birinci sınıf +%50.</summary>
        public static float QualityYield(int tier) => tier >= 2 ? 1.5f : (tier == 1 ? 1.25f : 1f);

        // ---------- günlük döngü ----------

        /// <summary>
        /// Yeni gün (LifeSystems çağırır): sulama sıfırlanır; yağmur dışarıdaki ekinleri sular; sulama hendekleri çevresini sular;
        /// boş topraklar toparlanır; kuzgunlar korumasız ekini yer; fırtına büyümeyi geri alır.
        /// </summary>
        public void OnNewDay(bool rained, bool stormed = false)
        {
            var rooms = _world.Map.Rooms;
            var crops = new List<ItemInstance>();
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                item.Watered = false;
                if (item.Def.IsCrop) crops.Add(item);
            }

            foreach (var crop in crops)
            {
                var room = rooms.RoomAt(crop.Position);
                bool indoor = room != null && room.IsIndoor;
                if (rained && !indoor) crop.Watered = true;
            }

            foreach (var source in _world.Items)
            {
                int r = source.Def.WaterRadius;
                if (r <= 0) continue;
                foreach (var crop in crops)
                    if (Near(source.Position, crop.Position, r)) crop.Watered = true;
            }

            // Boş toprak varsayılan verime döner.
            var planted = new HashSet<GridPos>();
            foreach (var crop in crops) planted.Add(crop.Position.OnLevel(0));
            foreach (var kv in _soil)
            {
                if (planted.Contains(kv.Key)) continue;
                var plot = kv.Value;
                float d = Config.DefaultFertility - plot.Fertility;
                float step = Math.Min(Math.Abs(d), Config.FallowRecovery);
                plot.Fertility += d >= 0 ? step : -step;
            }

            // Kuzgunlar ve fırtına yalnızca dışarıdaki ekini bulur.
            var lost = new List<ItemInstance>();
            foreach (var crop in crops)
            {
                var room = rooms.RoomAt(crop.Position);
                if (room != null && room.IsIndoor) continue;
                if (stormed && !crop.IsGrown && _rng.NextDouble() < Config.StormChance)
                {
                    crop.GrowthDays *= 1.0 - Config.StormGrowthLoss;
                    PlantLost?.Invoke(crop.Def, crop.Position, PlantLoss.Storm);
                }
                if (_rng.NextDouble() < Config.CrowChance && !Protected(crop.Position)) lost.Add(crop);
            }
            foreach (var crop in lost)
            {
                _world.RemoveItem(crop.Id);
                PlantLost?.Invoke(crop.Def, crop.Position, PlantLoss.Crows);
            }
        }

        private bool Protected(GridPos p)
        {
            foreach (var it in _world.Items)
                if (it.Def.ProtectRadius > 0 && Near(it.Position, p, it.Def.ProtectRadius)) return true;
            return false;
        }

        private static bool Near(GridPos a, GridPos b, int radius) =>
            a.Z == b.Z && Math.Abs(a.X - b.X) <= radius && Math.Abs(a.Y - b.Y) <= radius;

        // ---------- büyüme ----------

        /// <summary>Seranın o karodaki ısı çarpanı: eşiğin altında 0, tam ısıda 1.</summary>
        private float WarmthFactor(GridPos p)
        {
            if (TemperatureAt == null) return 1f;
            float t = TemperatureAt(p);
            if (t <= Config.GreenhouseMinTemp) return 0f;
            if (t >= Config.GreenhouseFullTemp) return 1f;
            return (t - Config.GreenhouseMinTemp) / (Config.GreenhouseFullTemp - Config.GreenhouseMinTemp);
        }

        private readonly List<ItemInstance> _activeHives = new List<ItemInstance>();

        // Bu turda çalışan kovanlardan biri yakında mı (kovan listesi tur başında bir kez çıkarılır).
        private bool NearActiveHive(GridPos p)
        {
            foreach (var hive in _activeHives)
                if (Near(hive.Position, p, hive.Def.PollinateRadius)) return true;
            return false;
        }

        public void Tick(double gameDays, Season season)
        {
            if (gameDays <= 0) return;
            KillWinterCrops(season);
            var rooms = _world.Map.Rooms;
            float mult = GrowthMultiplier != null ? Math.Max(0f, GrowthMultiplier()) : 1f;
            _activeHives.Clear();
            foreach (var it in _world.Items)
                if (it.Def.PollinateRadius > 0 && (HiveActive == null || HiveActive(it))) _activeHives.Add(it);
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                // Bodrum bitkisi (mantar yatağı): serin ve nemli, mevsimden ve havadan bağımsız büyür.
                if (item.Position.Z < 0)
                {
                    if (!item.IsGrown) item.GrowthDays = Math.Min(item.Def.GrowDays, item.GrowthDays + gameDays);
                    else TickFruit(item, season, gameDays, 1f, anySeason: true);
                    continue;
                }

                var room = rooms.RoomAt(item.Position);
                bool indoor = room != null && room.IsIndoor;
                bool greenhouse = indoor && IsGreenhouse(room);
                if (indoor && !greenhouse) continue;   // içeride (seradan başka) büyümez

                if (item.IsGrown)
                {
                    // Tozlaşma yalnızca meyve ağacı için sorulur (olgun meşe ya da hasat bekleyen ekin için boşuna taranmasın).
                    if (item.Def.IsFruitTree)
                        TickFruit(item, season, gameDays, mult * (NearActiveHive(item.Position) ? 1f + Config.PollinationFruit : 1f));
                    continue;
                }

                double rate = mult;
                if (greenhouse)
                {
                    // Sera mevsim kısıtını kaldırır ama ısı ister; kışın da ekin büyür.
                    float warm = WarmthFactor(item.Position);
                    if (warm <= 0f) continue;
                    rate *= warm;
                }
                else if (!CanGrow(item.Def, season)) continue;   // bu bitki bu mevsimde büyümez

                if (item.Def.IsCrop)
                {
                    rate *= Config.FertilityBase + Config.FertilitySpan * FertilityAt(item.Position);
                    if (NearActiveHive(item.Position)) rate *= 1f + Config.PollinationGrowth;
                    if (!item.Watered) rate *= Config.DryGrowth;
                    item.AgeDays += gameDays;
                    if (item.Watered) item.WetDays += gameDays;
                }
                item.GrowthDays = Math.Min(item.Def.GrowDays, item.GrowthDays + gameDays * rate);
            }
        }

        // Olgun meyve ağacı: meyve mevsiminde olgunlaşır, mevsim bitince dökülür; toplayınca yeniden başlar.
        private void TickFruit(ItemInstance item, Season season, double gameDays, float mult, bool anySeason = false)
        {
            if (!item.Def.IsFruitTree) return;
            if (!anySeason && !item.Def.FruitSeasons.Contains(season))
            {
                item.FruitReady = false;
                item.FruitDays = 0;
                return;
            }
            if (item.FruitReady) return;
            item.FruitDays += gameDays * mult;
            if (item.FruitDays >= item.Def.FruitRipenDays) item.FruitReady = true;
        }

        private void KillWinterCrops(Season season)
        {
            var dead = new List<ItemInstance>();
            var rooms = _world.Map.Rooms;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant || !item.Def.DiesInWinter) continue;
                var room = rooms.RoomAt(item.Position);
                bool indoor = room != null && room.IsIndoor;
                if (indoor)
                {
                    // Sera: ısıtılmamış ve donan oda ekini öldürür.
                    if (!IsGreenhouse(room) || TemperatureAt == null) continue;
                    if (TemperatureAt(item.Position) < Config.FrostTemp) dead.Add(item);
                    continue;
                }
                if (season == Season.Winter) dead.Add(item);
            }
            foreach (var item in dead)
            {
                _world.RemoveItem(item.Id);
                PlantDied?.Invoke(item.Def, item.Position);
                PlantLost?.Invoke(item.Def, item.Position, PlantLoss.Frost);
            }
        }

        /// <summary>Bitkiyi olgun yapar (haritadaki yabani ağaçlar, test).</summary>
        public static void SetGrown(ItemInstance item)
        {
            if (item.IsPlant) item.GrowthDays = item.Def.GrowDays;
        }
    }
}
