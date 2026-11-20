using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>Bir ekin karosunun toprağı: verim (0-1) ve oraya en son ekilen ekin (nöbetleşe ekim için).</summary>
    public sealed class SoilPlot
    {
        public float Fertility;
        public string LastCrop;
        /// <summary>Ondan önceki ekin: üç tarla nöbeti (üç ayrı aile art arda) ek verim verir.</summary>
        public string PrevCrop;

        // ---- beslenme ve gübre (belge 57c, 57d) ----
        /// <summary>En az bir hasat görmüş toprak: nadasla ancak nadas tavanına kadar toparlanır.</summary>
        public bool Worked;
        /// <summary>Artık bereket: serilen gübrenin sonraki hasatta toprağa geçecek payı.</summary>
        public float Residual;
        /// <summary>Ondan sonraki hasada kalan pay (kara toprak iki ekim sürer).</summary>
        public float ResidualNext;
        /// <summary>Bu ekim döngüsünde serilen gübreler; hasatta sıfırlanır.</summary>
        public Fertilizer Cycle;
        /// <summary>Bu döngüde serilen ham kompostta pislik vardı (çiğ yenen ekinde risk).</summary>
        public bool RawFilth;
        /// <summary>Taze gübre ekili karoyu yaktı: büyüme bu güne dek durur.</summary>
        public double BurnUntil;
        /// <summary>Marnın etkisi bu güne dek sürer (nadas tavanı yükselir).</summary>
        public double MarlUntil;
        /// <summary>Fazla marn: bu güne dek gübrenin etkisi yarıya iner.</summary>
        public double LimedUntil;
        /// <summary>Yaprak çürüğü (malç) bu güne dek toprağı örter.</summary>
        public double MulchUntil;
        /// <summary>Son kül sermesi (yılda bir).</summary>
        public double AshDay = -1000;
        /// <summary>Bugün serilen taze gübrenin toprağa hemen geçen payı (aynı gün kül serilirse yarısı uçar).</summary>
        public double FreshDay = -1000;
        public float FreshAmount;

        /// <summary>Bu döngüde verim katan bir gübre serildi mi (marn ve malç sayılmaz).</summary>
        public bool Fed => (Cycle & ~(Fertilizer.Marl | Fertilizer.Mulch)) != 0;
    }

    /// <summary>Gübre serme sonucu.</summary>
    public enum FertilizeOutcome
    {
        Ok,
        /// <summary>Bu ekim döngüsünde zaten gübrelendi.</summary>
        Saturated,
        /// <summary>Bu yıl kül serildi.</summary>
        AshUsed,
        /// <summary>Taze ya da sert gübre ekili karoyu yaktı.</summary>
        Burned,
        /// <summary>Marn süresi dolmadan yeniden serildi: toprak kireçlendi.</summary>
        Limed
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
        /// <summary>Üç tarla nöbeti: son üç ekin üç ayrı aileden. Aynı aile üst üste gelince yorgunluk MonocropPenalty x bu.</summary>
        public float ThreeFieldBonus = 0.06f, SameFamilyFactor = 1.5f;
        /// <summary>Boş toprak günde bu kadar varsayılan verime döner.</summary>
        public float FallowRecovery = 0.02f;

        // ---- beslenme tipleri (belge 57c §2) ----
        /// <summary>İşlenmiş toprak nadasla en çok bu verime döner; üstü gübre, baklagil ve ağıla almayla gelir.</summary>
        public float FallowCeiling = 0.40f;
        /// <summary>Marnlı toprağın nadas tavanı.</summary>
        public float MarlCeiling = 0.50f;
        /// <summary>Tavanın üstündeki boş toprak günde bu kadar verim yitirir (yıkanma); malçlı toprak yitirmez.</summary>
        public float FallowLeach = 0.005f;
        /// <summary>Hasatta toprağın yorulması: obur, kanaatkâr, yağsız toprak seven (orta ve toprak yapan HarvestDraw kullanır).</summary>
        public float HeavyDraw = 0.18f, LightDraw = 0.07f, LeanDraw = 0.07f;
        /// <summary>Büyüme eğrileri: çarpan = Base + Span x verim. Orta eğri FertilityBase ve FertilitySpan'dir.</summary>
        public float HeavyBase = 0.55f, HeavySpan = 0.70f, LightBase = 0.90f, LightSpan = 0.20f;
        /// <summary>Yağsız toprak seven ve toprak yapan: bu verimin üstünde "yapraklandı".</summary>
        public float LeafyAbove = 0.70f;
        /// <summary>Yapraklanan yağsız toprak ekini: ürün artar, kalite puanı düşer. Toprak yapan: ürün azalır.</summary>
        public float LeanLeafyYield = 1.15f, LeanLeafyQuality = 0.15f, BuilderLeafyYield = 0.80f;
        /// <summary>Gübresiz obur ekinin ürünü.</summary>
        public float HeavyHungryBelow = 0.45f, HeavyHungryYield = 0.85f;
        /// <summary>Serilen gübrenin sonraki hasada kalan payı (yavaş salım).</summary>
        public float ResidualShare = 0.40f;
        /// <summary>Sevdiği gübreyle beslenen ekinin kalite puanına eklenen.</summary>
        public float LikedFertilizerScore = 0.10f;
        /// <summary>Yakılan ekinin büyümesi bu kadar gün durur.</summary>
        public double BurnDays = 1.0;
        /// <summary>Marn bu kadar gün sürer (3 yıl); erken tekrar bu kadar gün "kireçlendi".</summary>
        public double MarlDays = 120.0, LimedDays = 40.0;
        /// <summary>Malç bu kadar gün sürer; malçlı toprakta sulanmamış ekinin büyüme çarpanı.</summary>
        public double MulchDays = 20.0;
        public float MulchDryGrowth = 0.65f;
        /// <summary>Kül yılda bir serilir.</summary>
        public double AshEveryDays = 40.0;
        /// <summary>Aynı aileyi üst üste sevmeyen ekinlerde (keten, çivit otu) nöbet cezası çarpanı.</summary>
        public float ExhaustingFactor = 3f;
        /// <summary>Bereketli su: sulama ağının suladığı karo günde bu kadar verim alır, bu tavana kadar.</summary>
        public float FertigationPerDay = 0.01f, FertigationCap = 0.80f;

        // ---- hasat baskısı ve tahıl yatması (belge 57d §2, 59 §8) ----
        /// <summary>Olgun tahıl fırtınada bu kadarını döker.</summary>
        public float ShatterLoss = 0.30f;
        /// <summary>Bu verimin üstündeki tahıl fırtınada yatar (uzun boylu çeşitte TallLodgeAbove).</summary>
        public float LodgeAbove = 0.85f, TallLodgeAbove = 0.80f;
        /// <summary>Yatan tahılın kaybı; tarlada kaldığı her gün eklenen; en çok.</summary>
        public float LodgeLoss = 0.50f, LodgeDailyLoss = 0.05f, LodgeMaxLoss = 0.80f;
        /// <summary>Eşiği aşan tahılın yatma olasılığı; büyümenin en az bu kadarı tamamlanmış olmalı.</summary>
        public double LodgeChance = 0.80;
        public float LodgeMinGrowth = 0.60f;

        public float GrowthFactor(FeedType feed, float fertility)
        {
            switch (feed)
            {
                case FeedType.Heavy: return HeavyBase + HeavySpan * fertility;
                case FeedType.Light: return LightBase + LightSpan * fertility;
                case FeedType.Lean:
                case FeedType.Builder: return 1f;
                default: return FertilityBase + FertilitySpan * fertility;
            }
        }

        public float DrawOf(FeedType feed)
        {
            switch (feed)
            {
                case FeedType.Heavy: return HeavyDraw;
                case FeedType.Light: return LightDraw;
                case FeedType.Lean: return LeanDraw;
                default: return HarvestDraw;
            }
        }
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
        /// <summary>Sararma (59 §3): mevsimi dışında bu kadar gün geçince ekin sararır; yarı boyun üstündeyse yan ürün verir.</summary>
        public double WitherAfterDays = 1.0;
        public float WitherMatureFraction = 0.5f;
        /// <summary>Sökülmeyen sararmış ekin bu kadar gün sonra çürüyüp toprağa karışır (verime küçük katkı).</summary>
        public double WitheredRotDays = 10.0;
        public float RotFertility = 0.03f;
        /// <summary>Sırık isteyen ekinde sırıksız ürün çarpanı.</summary>
        public float UnstakedYield = 0.5f;
        /// <summary>Kalıcı yatak her toplamada toprağı bu kadar yorar (sökülmediği için nöbet kuralı işlemez).</summary>
        public float PerennialDraw = 0.04f;
        /// <summary>Dev ürün (59 §2): 2×2 olgun ve sulu karo için sabah şansı; verimli toprakta ek şans; bu Çiftçilik seviyesinde iki kat.</summary>
        public double GiantChance = 0.02, GiantRichBonus = 0.01;
        public float GiantRichFertility = 0.8f;
        public int GiantSkillLevel = 8;
    }

    public enum PlantLoss { Crows, Storm, Frost, Rotted }

    /// <summary>Ekinin sararma nedeni: mevsimi geçti, toplama hakkı bitti (tohuma kaçtı) ya da don vurdu.</summary>
    public enum WitherCause { Season, Spent, Frost }

    /// <summary>Fırtınanın tahıla etkisi (belge 57d §2).</summary>
    public enum CerealDamage { None, Shattered, Lodged }

    /// <summary>Tarlanın özeti (arayüz şeridi): ekin sayısı, susamış, hasada hazır, meyvesi olgun ağaç.</summary>
    public readonly struct FarmSummary
    {
        public readonly int Crops, Thirsty, Ripe, FruitReady, Withered;
        public FarmSummary(int crops, int thirsty, int ripe, int fruitReady, int withered = 0) { Crops = crops; Thirsty = thirsty; Ripe = ripe; FruitReady = fruitReady; Withered = withered; }
        public bool Any => Crops > 0 || FruitReady > 0;
    }

    /// <summary>
    /// Bitki büyümesi ve çiftçilik: ekinler (sulama, toprak verimi, nöbetleşe ekim, kalite), sera (ışıklı ve
    /// ısıtılmış oda kışın da ekin büyütür), meyve ağaçları (olgun ağaç mevsiminde tekrar tekrar meyve verir),
    /// sulama hendeği ve korkuluk. Ağaçlar sulanmaz; kış dışında kendiliğinden büyür.
    /// </summary>
    public sealed class PlantSystem
    {
        public const string QualityNormal = "Sıradan", QualityFine = "Seçme", QualityFinest = "Has";

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
        /// <summary>Karo Güney Yamaçları'nda mı (bölge haritası gelince bağlanır; yoksa hiçbir yer güney değildir).</summary>
        public Func<GridPos, bool> IsSouth { get; set; }
        private bool InSouth(GridPos p) => IsSouth != null && IsSouth(p);

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
        /// <summary>Sera odasının büyüme çarpanı (HomeLife: bakımlı sera ×1,1); null ise 1.</summary>
        public Func<Room, double> GreenhouseGrowth { get; set; }

        public bool IsGreenhouse(Room room) =>
            room != null && room.IsIndoor && room.Level == 0
            && room.WindowCount >= Config.GreenhouseMinWindows
            && room.WindowCount * Config.GreenhouseTilesPerWindow >= room.Area;

        /// <summary>Tarla özeti: kaç ekin var, kaçı susamış (büyüyor ama sulanmamış), kaçı hasada hazır, kaç ağacın meyvesi olgun.</summary>
        public FarmSummary Summary()
        {
            int crops = 0, thirsty = 0, ripe = 0, fruit = 0, withered = 0;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                if (item.Def.IsCrop)
                {
                    crops++;
                    if (item.Withered) withered++;
                    else if (item.IsGrown) ripe++;
                    else if (!item.Watered) thirsty++;
                }
                if (item.FruitReady) fruit++;
            }
            return new FarmSummary(crops, thirsty, ripe, fruit, withered);
        }

        // ---------- toprak ----------

        public IEnumerable<KeyValuePair<GridPos, SoilPlot>> SoilPlots => _soil;

        public float FertilityAt(GridPos p) =>
            _soil.TryGetValue(p.OnLevel(0), out var plot) ? plot.Fertility : Config.DefaultFertility;

        public string LastCropAt(GridPos p) => _soil.TryGetValue(p.OnLevel(0), out var plot) ? plot.LastCrop : null;

        public void SetSoil(GridPos p, float fertility, string lastCrop, string prevCrop = null)
        {
            var plot = PlotAt(p);
            plot.Fertility = Clamp01(fertility);
            plot.LastCrop = string.IsNullOrEmpty(lastCrop) ? null : lastCrop;
            plot.PrevCrop = string.IsNullOrEmpty(prevCrop) ? null : prevCrop;
        }

        /// <summary>Ekin kimliğinden aileyi bulur (içerik yoksa kimliğin kendisi aile sayılır).</summary>
        public Func<string, string> FamilyOf { get; set; }
        private string Family(string cropId) => cropId == null ? null : (FamilyOf?.Invoke(cropId) ?? cropId);

        /// <summary>Bu karoya bu ekin ekilirse nöbet nasıl: "aynı aile", "nöbet", "üç tarla" ya da null (geçmiş yok). Arayüz ipucu.</summary>
        public string RotationHint(GridPos p, Content.ItemDef crop)
        {
            if (crop == null || !_soil.TryGetValue(p.OnLevel(0), out var plot) || plot.LastCrop == null) return null;
            string fam = crop.CropFamily ?? crop.Id;
            if (Family(plot.LastCrop) == fam) return "Aynı aile üst üste: toprak çabuk yorulur";
            if (plot.PrevCrop != null && Family(plot.PrevCrop) != fam && Family(plot.PrevCrop) != Family(plot.LastCrop)) return "Üç tarla nöbeti: toprak dinlenir";
            return "Nöbetleşe ekim: toprak toparlanır";
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

        // ---------- beslenme ve gübre türleri (belge 57c, 57d) ----------

        /// <summary>Oyun zamanı (toplam gün); marn, malç ve kül süreleri için. Yoksa 0.</summary>
        public Func<double> Now { get; set; }
        private double Today => Now != null ? Now() : 0.0;

        /// <summary>Karonun toprak kaydı (yoksa null); arayüz ve kayıt için.</summary>
        public SoilPlot PlotOrNull(GridPos p) => _soil.TryGetValue(p.OnLevel(0), out var plot) ? plot : null;
        /// <summary>Karonun toprak kaydı (yoksa oluşturur).</summary>
        public SoilPlot Plot(GridPos p) => PlotAt(p);

        public bool IsMulched(GridPos p) { var plot = PlotOrNull(p); return plot != null && plot.MulchUntil > Today; }
        public bool IsMarled(GridPos p) { var plot = PlotOrNull(p); return plot != null && plot.MarlUntil > Today; }

        /// <summary>Bu karoya bu gübre şimdi serilebilir mi; serilemezse sonuç (Ok değilse) döner.</summary>
        public FertilizeOutcome CanFertilize(GridPos p, Fertilizer kind)
        {
            var plot = PlotOrNull(p);
            if (plot == null || kind == Fertilizer.Marl || kind == Fertilizer.Mulch) return FertilizeOutcome.Ok;
            if (kind == Fertilizer.Ash) return Today - plot.AshDay < Config.AshEveryDays ? FertilizeOutcome.AshUsed : FertilizeOutcome.Ok;
            return plot.Fed && kind != Fertilizer.Generic ? FertilizeOutcome.Saturated : FertilizeOutcome.Ok;
        }

        /// <summary>
        /// Türüne göre gübre serer. Verimin bir kısmı hemen, kalanı sonraki hasatta toprağa geçer (artık bereket).
        /// Taze ve sert gübre ekili karoyu yakar; marn ve malç verim eklemez. <paramref name="crop"/> karodaki ekin (yoksa null).
        /// </summary>
        public FertilizeOutcome ApplyFertilizer(GridPos p, Fertilizer kind, float amount, ItemInstance crop = null, bool filth = false)
        {
            var plot = PlotAt(p);
            double now = Today;
            if (kind == Fertilizer.Generic || kind == Fertilizer.None)
            {
                plot.Fertility = Clamp01(plot.Fertility + amount);
                plot.Cycle |= Fertilizer.Generic;
                return FertilizeOutcome.Ok;
            }
            if (kind == Fertilizer.Marl)
            {
                bool early = plot.MarlUntil > now;
                plot.MarlUntil = now + Config.MarlDays;
                plot.Cycle |= Fertilizer.Marl;
                if (early) { plot.LimedUntil = now + Config.LimedDays; return FertilizeOutcome.Limed; }
                return FertilizeOutcome.Ok;
            }
            if (kind == Fertilizer.Mulch)
            {
                plot.MulchUntil = now + Config.MulchDays;
                plot.Cycle |= Fertilizer.Mulch;
                return FertilizeOutcome.Ok;
            }
            var block = CanFertilize(p, kind);
            if (block != FertilizeOutcome.Ok) return block;

            if (plot.LimedUntil > now) amount *= 0.5f;
            var outcome = FertilizeOutcome.Ok;
            bool growing = crop != null && crop.IsPlant && crop.Def.IsCrop && !crop.IsGrown;
            if (kind == Fertilizer.Ash)
            {
                plot.AshDay = now;
                // Aynı gün taze gübreyle serilen kül, gübrenin azotunu uçurur (yarısı gider).
                if (Math.Floor(plot.FreshDay) == Math.Floor(now) && plot.FreshAmount > 0f)
                {
                    plot.Fertility = Clamp01(plot.Fertility - plot.FreshAmount * 0.5f);
                    plot.FreshAmount = 0f;
                }
            }
            else if (kind == Fertilizer.FreshManure || kind == Fertilizer.HotManure)
            {
                bool tolerated = kind == Fertilizer.HotManure && crop != null && crop.Def.Feed == FeedType.Heavy;
                if (growing && !tolerated)
                {
                    plot.BurnUntil = now + Config.BurnDays;
                    outcome = FertilizeOutcome.Burned;
                }
            }
            if (kind == Fertilizer.CompostRaw && filth) plot.RawFilth = true;

            float residual = amount * Config.ResidualShare;
            float immediate = amount - residual;
            plot.Fertility = Clamp01(plot.Fertility + immediate);
            if (kind == Fertilizer.BlackSoil) { plot.Residual += residual * 0.5f; plot.ResidualNext += residual * 0.5f; }
            else plot.Residual += residual;
            if (kind == Fertilizer.FreshManure || kind == Fertilizer.HotManure) { plot.FreshDay = now; plot.FreshAmount = immediate; }
            plot.Cycle |= kind;
            return outcome;
        }

        /// <summary>Ağıla alma, otlatma ve bereketli su: karonun verimini tavana kadar artırır (gübre serme sayılmaz).</summary>
        public void Enrich(GridPos p, float amount, float cap = 1f)
        {
            var plot = PlotAt(p);
            if (plot.Fertility >= cap) return;
            plot.Fertility = Math.Min(cap, plot.Fertility + amount);
        }

        /// <summary>Bereketli su açık mı (şerbet fıçısı sulama ağına bağlı); null ise kapalı.</summary>
        public Func<bool> Fertigation { get; set; }

        /// <summary>Ekin taze gübreden zarar gördü mü (kök çatallanır, soğan çürür, ot kokusunu yitirir)?</summary>
        public bool IsSpoiledByManure(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return false;
            var plot = PlotOrNull(item.Position);
            if (plot == null) return false;
            var feed = item.Def.Feed;
            if (feed != FeedType.Light && feed != FeedType.Lean && feed != FeedType.Builder) return false;
            return (plot.Cycle & (Fertilizer.FreshManure | Fertilizer.HotManure | Fertilizer.CompostRaw)) != 0;
        }

        /// <summary>Çiğ yenen ekin pislikli ham kompostla mı büyüdü (yiyende karın ağrısı riski)?</summary>
        public bool HasRawFilth(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop || !item.Def.EatenRaw) return false;
            var plot = PlotOrNull(item.Position);
            return plot != null && plot.RawFilth;
        }

        /// <summary>Yağsız toprak seven ya da toprak yapan ekin fazla verimli toprakta yaprağa mı kaçtı?</summary>
        public bool IsLeafy(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return false;
            var feed = item.Def.Feed;
            return (feed == FeedType.Lean || feed == FeedType.Builder) && FertilityAt(item.Position) > Config.LeafyAbove;
        }

        /// <summary>Hasat miktarı çarpanı: beslenme tipi, sevmediği gübre, fırtına kaybı.</summary>
        public float YieldFactor(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return 1f;
            float f = FertilityAt(item.Position), m = 1f;
            switch (item.Def.Feed)
            {
                case FeedType.Heavy: if (f < Config.HeavyHungryBelow) m *= Config.HeavyHungryYield; break;
                case FeedType.Lean: if (f > Config.LeafyAbove) m *= Config.LeanLeafyYield; break;
                case FeedType.Builder: if (f > Config.LeafyAbove) m *= Config.BuilderLeafyYield; break;
            }
            var plot = PlotOrNull(item.Position);
            if (plot != null && (plot.Cycle & item.Def.DislikesFertilizer) != 0) m *= 0.8f;
            return m * (1f - StormLoss(item));
        }

        // ---------- tahıl: dökme ve yatma ----------

        private readonly Dictionary<int, float> _lodged = new Dictionary<int, float>();
        private readonly HashSet<int> _shattered = new HashSet<int>();

        /// <summary>Fırtınadan sonra: (yatan karo sayısı, döken karo sayısı).</summary>
        public event Action<int, int> CerealsHit;

        public CerealDamage DamageOf(ItemInstance item) =>
            item == null ? CerealDamage.None : _lodged.ContainsKey(item.Id) ? CerealDamage.Lodged : _shattered.Contains(item.Id) ? CerealDamage.Shattered : CerealDamage.None;
        public bool IsLodged(ItemInstance item) => item != null && _lodged.ContainsKey(item.Id);
        public IEnumerable<KeyValuePair<int, float>> LodgedCrops => _lodged;
        public IEnumerable<int> ShatteredCrops => _shattered;

        /// <summary>Fırtınada yitirilen pay (0-1).</summary>
        public float StormLoss(ItemInstance item)
        {
            if (item == null) return 0f;
            if (_lodged.TryGetValue(item.Id, out float loss)) return loss;
            return _shattered.Contains(item.Id) ? Config.ShatterLoss : 0f;
        }

        /// <summary>Kayıttan geri yükleme.</summary>
        public void RestoreCerealDamage(IEnumerable<KeyValuePair<int, float>> lodged, IEnumerable<int> shattered)
        {
            _lodged.Clear(); _shattered.Clear();
            if (lodged != null) foreach (var kv in lodged) _lodged[kv.Key] = Math.Min(Config.LodgeMaxLoss, Math.Max(0f, kv.Value));
            if (shattered != null) foreach (var id in shattered) _shattered.Add(id);
        }

        /// <summary>Bu tahıl bu verimde fırtınada yatar mı (uyarı ve görünüm için)?</summary>
        public bool LodgeRisk(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop || !item.Def.IsCereal || item.Def.HasTag("no_lodge")) return false;
            return FertilityAt(item.Position) >= (item.Def.HasTag("tall") ? Config.TallLodgeAbove : Config.LodgeAbove);
        }

        private void StormOnCereals(List<ItemInstance> crops, RoomMap rooms, bool stormed)
        {
            // Tarlada kalan yatık ekin her gün biraz daha yitirir (kuş, fare, çimlenme); kalkan ekinin kaydı silinir.
            if (_lodged.Count > 0 || _shattered.Count > 0)
            {
                var alive = new HashSet<int>();
                foreach (var c in crops) alive.Add(c.Id);
                var gone = new List<int>();
                foreach (var id in _lodged.Keys) if (!alive.Contains(id)) gone.Add(id);
                foreach (var id in gone) _lodged.Remove(id);
                _shattered.RemoveWhere(id => !alive.Contains(id));
                var keys = new List<int>(_lodged.Keys);
                foreach (var id in keys) _lodged[id] = Math.Min(Config.LodgeMaxLoss, _lodged[id] + Config.LodgeDailyLoss);
            }
            if (!stormed) return;
            int lodged = 0, shattered = 0;
            foreach (var crop in crops)
            {
                if (!crop.Def.IsCereal || _lodged.ContainsKey(crop.Id)) continue;
                var room = rooms.RoomAt(crop.Position);
                if (room != null && room.IsIndoor) continue;
                if (crop.GrowthFraction >= Config.LodgeMinGrowth && LodgeRisk(crop) && _rng.NextDouble() < Config.LodgeChance)
                {
                    _lodged[crop.Id] = Config.LodgeLoss;
                    _shattered.Remove(crop.Id);
                    lodged++;
                }
                else if (crop.IsGrown && _shattered.Add(crop.Id)) shattered++;
            }
            if (lodged > 0 || shattered > 0) CerealsHit?.Invoke(lodged, shattered);
        }

        /// <summary>Ekini sular (bugünlük). Ekin dışındakiler için etkisizdir.</summary>
        public static void Water(ItemInstance item)
        {
            if (item != null && item.IsPlant && item.Def.IsCrop && !item.Withered) item.Watered = true;
        }

        // ---------- hasat döngüsü, sırık, sararma (59 §1, §3) ----------

        public const string HarvestId = "harvest";
        /// <summary>Yan ürünlerde kullanılan ortak eşyalar: kompostluk yeşil (ot), yem ve saman.</summary>
        public const string GreenItemId = "item.herb", FeedItemId = "item.feed", StrawItemId = "item.straw";
        /// <summary>Ekin sırığı: dikilince harcanır, bitki sökülünce geri gelir.</summary>
        public const string StakeItemId = "item.crop_stake";

        /// <summary>Ekin sarardı (eşya, neden). Eşya yerinde kalır; "Sök" ile kaldırılır.</summary>
        public event Action<ItemInstance, WitherCause> PlantWithered;
        /// <summary>2×2 olgun ekin dev ürüne dönüştü (ekin tanımı, yeni dev ürün eşyası).</summary>
        public event Action<Content.ItemDef, ItemInstance> GiantGrown;
        /// <summary>Eşya tanımı arar (dev ürün eşyası için); içerik veritabanına bağlanır.</summary>
        public Func<string, Content.ItemDef> DefOf { get; set; }
        /// <summary>Hanenin en iyi Çiftçilik seviyesi (dev ürün şansı için).</summary>
        public Func<int> FarmingLevel { get; set; }

        /// <summary>Bu hasatta bitki yerinde kalır mı: yeniden veren ekinin olağan hasadı ("Seçme tohum" bitkiyi söker).</summary>
        public static bool KeepsAfterHarvest(ItemInstance item, Content.InteractionDef interaction) =>
            item != null && interaction != null && item.Def.Regrows && !item.Withered && interaction.Id == HarvestId;

        /// <summary>Sırık isteyip sırığı dikilmemiş mi (ürün yarıya iner).</summary>
        public static bool MissingStake(ItemInstance item) => item != null && item.Def.NeedsStake && !item.Staked;

        /// <summary>Kalan toplama hakkı; sınırsızsa (kalıcı yatak, tek hasat) -1.</summary>
        public static int HarvestsLeft(ItemInstance item) =>
            item == null || !item.Def.Regrows || item.Def.Cycle == Content.CropCycle.Perennial || item.Def.MaxHarvests <= 0
                ? -1 : Math.Max(0, item.Def.MaxHarvests - item.Harvests);

        /// <summary>
        /// Yeniden veren ekinden ürün toplandı: bitki kalır, bir sonraki ürün için geri sayar. Toplama hakkı bitince sararır
        /// (tohuma kaçar). Toprak her toplamada değil bitki sökülünce yorulur; kalıcı yatak her toplamada az yorar.
        /// </summary>
        public void OnPicked(ItemInstance item)
        {
            if (item == null || !item.Def.Regrows || item.Withered) return;
            item.Harvests++;
            item.GrowthDays = Math.Max(0, item.Def.GrowDays - item.Def.RegrowDays);
            item.AgeDays = 0;
            item.WetDays = 0;
            if (item.Def.Cycle == Content.CropCycle.Perennial)
            {
                var plot = PlotAt(item.Position);
                plot.Fertility = Clamp01(plot.Fertility - Config.PerennialDraw);
                plot.LastCrop = item.Def.Id;
            }
            else if (item.Def.MaxHarvests > 0 && item.Harvests >= item.Def.MaxHarvests) Wither(item, WitherCause.Spent);
        }

        /// <summary>Ekini sarartır: büyümez, hasat edilmez; yarı boyun üstündeyse (ya da ürün verdiyse) sökülünce yan ürün verir.</summary>
        public void Wither(ItemInstance item, WitherCause cause)
        {
            if (item == null || !item.IsPlant || !item.Def.IsCrop || item.Withered) return;
            item.WitheredMature = item.Harvests > 0 || item.GrowthFraction >= Config.WitherMatureFraction;
            item.Withered = true;
            item.Watered = false;
            item.OffSeasonDays = 0;
            PlantWithered?.Invoke(item, cause);
        }

        /// <summary>Sararmış ekin söküldü: ürün vermiş ya da gelişmiş bitki toprağı hasat gibi etkiler (yorar, baklagilse besler).</summary>
        public void OnUproot(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return;
            if (item.WitheredMature) OnHarvest(item);
        }

        /// <summary>Mevsimi bu ekin için bitti mi: kış ayrı kuraldır (don), kalıcı yatak mevsim dışında uyur.</summary>
        public static bool SeasonOver(Content.ItemDef def, Season season) =>
            def.IsCrop && def.Cycle != Content.CropCycle.Perennial && season != Season.Winter && !CanGrow(def, season);

        /// <summary>
        /// Sararmış ekinin sökülünce verdiği yan ürün (59 §3), ekin ailesine göre: yaprak tohuma kaçar, kök odunsulaşır (yem),
        /// baklagil kabukta kurur (kuru tane), tahıl başak döker (saman ve demetin üçte biri), lif kartlaşır. Gelişmeden
        /// sararan filiz yalnızca kompostluk yeşil verir.
        /// </summary>
        public static List<Content.ProduceDef> WitherYield(ItemInstance item, Content.ContentDatabase content)
        {
            var rows = new List<Content.ProduceDef>();
            if (item == null || content == null || !item.Def.IsCrop) return rows;
            void Row(string id, int count, float chance = 1f)
            {
                if (id != null && count > 0 && content.Items.Contains(id)) rows.Add(new Content.ProduceDef { ItemId = id, Count = count, Chance = chance });
            }
            if (!item.WitheredMature) { Row(GreenItemId, 1); return rows; }

            var def = item.Def;
            string seed = null;
            foreach (var d in content.Items.All) if (d.PlantsAs == def.Id) { seed = d.Id; break; }
            Content.InteractionDef harvest = null;
            foreach (var i in def.Interactions) if (i.Id == HarvestId) { harvest = i; break; }
            string main = harvest != null && harvest.Produce.Count > 0 ? harvest.Produce[0].ItemId : null;
            int mainCount = harvest != null && harvest.Produce.Count > 0 ? harvest.Produce[0].Count : 0;
            switch (def.CropFamily)
            {
                case "yaprak": Row(seed, 2); Row(seed, 1, 0.5f); Row(GreenItemId, 1); break;
                case "kok": Row(FeedItemId, 1); Row(seed, 1); Row(seed, 1, 0.5f); break;
                case "sogan": Row(seed, 2); Row(main, 1); break;
                case "baklagil": Row(seed, 3); Row(FeedItemId, 1); break;
                case "tahil": Row(StrawItemId, 2); Row(main, Math.Max(1, mainCount / 3)); break;
                case "lif": Row(main, 1); Row(seed, 2); break;
                case "kabak": Row(seed, 2); Row(seed, 1, 0.5f); Row(GreenItemId, 1); break;
                case "fig": Row(FeedItemId, 1); Row(seed, 1); break;
                default: Row(seed, 1); Row(GreenItemId, 1); break;
            }
            return rows;
        }

        /// <summary>Bu mevsim dönünce sararacak (ya da donacak) canlı ekinlerin sayısı: mevsim sonu uyarısı için.</summary>
        public int AtRiskNextSeason(Season next)
        {
            int n = 0;
            var rooms = _world.Map.Rooms;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant || !item.Def.IsCrop || item.Withered || item.Position.Z < 0) continue;
                var room = rooms.RoomAt(item.Position);
                if (room != null && room.IsIndoor) continue;   // sera kendi kuralıyla
                if (next == Season.Winter ? item.Def.DiesInWinter : SeasonOver(item.Def, next)) n++;
            }
            return n;
        }

        /// <summary>Olgun ekin hasat edilince toprak yorulur ya da (baklagil, nöbetleşe ekim) toparlanır.</summary>
        public void OnHarvest(ItemInstance item)
        {
            if (item == null || !item.Def.IsCrop) return;
            var plot = PlotAt(item.Position);
            float delta = -Config.DrawOf(item.Def.Feed);
            if (item.Def.FixesNitrogen) delta += Config.NitrogenBonus;
            float monocrop = Config.MonocropPenalty * (item.Def.HasTag("exhausting") ? Config.ExhaustingFactor : 1f);
            // Nöbet aileye bakar: buğdaydan sonra arpa da yorar (ikisi de tahıl); baklagil ve kök araya girince toprak dinlenir.
            string fam = item.Def.CropFamily ?? item.Def.Id;
            if (plot.LastCrop != null)
            {
                string last = Family(plot.LastCrop);
                if (plot.LastCrop == item.Def.Id) delta -= monocrop * Config.SameFamilyFactor;
                else if (last == fam) delta -= monocrop;
                else
                {
                    delta += Config.RotationBonus;
                    string prev = Family(plot.PrevCrop);
                    if (prev != null && prev != fam && prev != last) delta += Config.ThreeFieldBonus;
                }
            }
            // Artık bereket: önceki gübrenin kalan payı hasattan sonra toprağa geçer; sıradaki ekin ondan yararlanır.
            plot.Fertility = Clamp01(plot.Fertility + delta + plot.Residual);
            plot.Residual = plot.ResidualNext;
            plot.ResidualNext = 0f;
            plot.Cycle = Fertilizer.None;
            plot.RawFilth = false;
            plot.BurnUntil = 0;
            plot.Worked = true;
            plot.PrevCrop = plot.LastCrop;
            plot.LastCrop = item.Def.Id;
            _lodged.Remove(item.Id);
            _shattered.Remove(item.Id);
        }

        // ---------- kalite ----------

        /// <summary>0 sıradan, 1 seçme, 2 birinci sınıf. Sulu geçen süre, toprak verimi ve çiftçilik seviyesi belirler.</summary>
        public static int QualityTier(FarmConfig cfg, float wetRatio, float fertility, int skillLevel)
        {
            float score = wetRatio * cfg.WetWeight + fertility * cfg.FertWeight + Math.Min(10, Math.Max(0, skillLevel)) * cfg.SkillWeight;
            return score >= cfg.FinestScore ? 2 : (score >= cfg.FineScore ? 1 : 0);
        }

        public int QualityOf(ItemInstance item, int skillLevel)
        {
            if (item == null || !item.Def.IsCrop) return 0;
            float f = FertilityAt(item.Position);
            // Kaliteye giren verim beslenme tipine göre okunur: kanaatkâr fakir toprakta ceza almaz, yağsız toprak
            // seven fazla verimde kalite yitirir, toprak yapanın kalitesi verime bağlı değildir.
            float fq = f;
            switch (item.Def.Feed)
            {
                case FeedType.Light: fq = Math.Max(f, Config.DefaultFertility); break;
                case FeedType.Lean: fq = f > Config.LeafyAbove ? Config.LeanLeafyQuality : Math.Max(f, 0.7f); break;
                case FeedType.Builder: fq = Math.Max(Math.Min(f, Config.LeafyAbove), Config.DefaultFertility); break;
            }
            var plot = PlotOrNull(item.Position);
            float bonus = plot != null && (plot.Cycle & item.Def.LikesFertilizer) != 0 ? Config.LikedFertilizerScore : 0f;
            float score = item.WetRatio * Config.WetWeight + fq * Config.FertWeight + Math.Min(10, Math.Max(0, skillLevel)) * Config.SkillWeight + bonus;
            int tier = score >= Config.FinestScore ? 2 : (score >= Config.FineScore ? 1 : 0);
            if (plot != null && (plot.Cycle & Fertilizer.BlackSoil) != 0) tier = Math.Min(2, tier + 1);   // kara toprak: bir kademe yukarı
            if (IsSpoiledByManure(item)) tier = Math.Max(0, tier - 1);                                     // çatallanan kök, çürüyen soğan
            if (_lodged.ContainsKey(item.Id) && _lodged[item.Id] > Config.LodgeLoss + Config.LodgeDailyLoss * 2.5f) tier = Math.Max(0, tier - 1);   // başakta çimlenme
            return tier;
        }

        public static string QualityName(int tier) => tier >= 2 ? QualityFinest : (tier == 1 ? QualityFine : QualityNormal);

        /// <summary>Verim çarpanı (53, karar 2: yarıya indi): Seçme +%10, Has +%20. Değerin kalanı kaliteye (fiyat, doyum) geçti.</summary>
        public static float QualityYield(int tier) => (float)(1.0 + Content.ItemQuality.CountBonus(tier));

        // ---------- günlük döngü ----------

        /// <summary>
        /// Yeni gün (LifeSystems çağırır): sulama sıfırlanır; yağmur dışarıdaki ekinleri sular; sulama hendekleri çevresini sular;
        /// boş topraklar toparlanır; kuzgunlar korumasız ekini yer; fırtına büyümeyi geri alır.
        /// </summary>
        public void OnNewDay(bool rained, bool stormed = false)
        {
            var rooms = _world.Map.Rooms;
            var crops = new List<ItemInstance>();
            var wetBefore = new HashSet<int>();
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                if (item.Watered) wetBefore.Add(item.Id);
                item.Watered = false;
                if (item.Def.IsCrop && !item.Withered) crops.Add(item);
            }

            foreach (var crop in crops)
            {
                var room = rooms.RoomAt(crop.Position);
                bool indoor = room != null && room.IsIndoor;
                if (rained && !indoor) crop.Watered = true;
            }

            // Kanal yalnızca kaynağa (kuyu, kova-makara) bağlı bir zincirdeyse sular.
            HashSet<int> network = null;
            foreach (var source in _world.Items)
            {
                int r = source.Def.WaterRadius;
                if (r <= 0) continue;
                if (source.Def.NeedsWaterSource)
                {
                    network = network ?? WaterNetwork.Connected(_world);
                    if (!network.Contains(source.Id)) continue;
                }
                foreach (var crop in crops)
                    if (Near(source.Position, crop.Position, r)) crop.Watered = true;
            }

            // Su yolu katmanı: su taşıyan toprak ve taş ark yanındaki ekini sular (oluk ve boru sulamaz).
            if (_world.Map.HasConduits && crops.Count > 0)
            {
                var wet = WaterNetwork.Analyze(_world).WetConduits;
                foreach (var crop in crops)
                {
                    if (crop.Watered || crop.Position.Z != 0) continue;
                    for (int dy = -1; dy <= 1 && !crop.Watered; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var p = new GridPos(crop.Position.X + dx, crop.Position.Y + dy);
                            var t = _world.Map.GetConduit(p);
                            if (t == ConduitType.None || Math.Max(Math.Abs(dx), Math.Abs(dy)) > Conduits.IrrigateRadius(t) || !wet.Contains(p)) continue;
                            crop.Watered = true;
                            break;
                        }
                }
            }

            // Boş toprak dinlenir. Hiç işlenmemiş toprak varsayılan verime döner (eski kural). İşlenmiş toprak nadasla
            // ancak nadas tavanına (marnlıysa biraz üstüne) kadar toparlanır; tavanın üstündeki verim yavaşça yıkanır,
            // malçlı toprak yitirmez (belge 57c §2, 57d §3).
            var planted = new HashSet<GridPos>();
            foreach (var crop in crops) planted.Add(crop.Position.OnLevel(0));
            double now = Today;
            foreach (var kv in _soil)
            {
                if (planted.Contains(kv.Key)) continue;
                var plot = kv.Value;
                if (!plot.Worked)
                {
                    float d = Config.DefaultFertility - plot.Fertility;
                    float step = Math.Min(Math.Abs(d), Config.FallowRecovery);
                    plot.Fertility += d >= 0 ? step : -step;
                    continue;
                }
                float ceiling = plot.MarlUntil > now ? Config.MarlCeiling : Config.FallowCeiling;
                if (plot.Fertility < ceiling) plot.Fertility = Math.Min(ceiling, plot.Fertility + Config.FallowRecovery);
                else if (plot.Fertility > ceiling && plot.MulchUntil <= now) plot.Fertility = Math.Max(ceiling, plot.Fertility - Config.FallowLeach);
            }

            // Bereketli su: şerbet fıçısı ağa bağlıysa sulama ağının suladığı ekinin toprağı her gün biraz beslenir.
            if (Fertigation != null && Fertigation())
                foreach (var crop in crops)
                    if (crop.Watered && !(rained && !IsIndoor(rooms, crop.Position))) Enrich(crop.Position, Config.FertigationPerDay, Config.FertigationCap);

            StormOnCereals(crops, rooms, stormed);
            GrowGiants(crops, wetBefore);

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

        private static bool IsIndoor(RoomMap rooms, GridPos p) { var room = rooms.RoomAt(p); return room != null && room.IsIndoor; }

        // Dev ürün (59 §2): aynı ekinden 2×2 olgun ve sulu (dün ya da bu sabah) karo küçük şansla tek dev ürüne dönüşür.
        private void GrowGiants(List<ItemInstance> crops, HashSet<int> wetBefore)
        {
            if (DefOf == null) return;
            Dictionary<GridPos, ItemInstance> ready = null;
            foreach (var c in crops)
            {
                if (c.Def.GiantAs == null || !c.IsGrown || !(c.Watered || wetBefore.Contains(c.Id))) continue;
                (ready ?? (ready = new Dictionary<GridPos, ItemInstance>()))[c.Position] = c;
            }
            if (ready == null || ready.Count < 4) return;
            var order = new List<ItemInstance>(ready.Values);
            order.Sort((a, b) => a.Id.CompareTo(b.Id));
            var used = new HashSet<int>();
            int skill = FarmingLevel != null ? FarmingLevel() : 0;
            foreach (var a in order)
            {
                if (used.Contains(a.Id)) continue;
                var p = a.Position;
                var block = new ItemInstance[4];
                block[0] = a;
                bool ok = ready.TryGetValue(new GridPos(p.X + 1, p.Y, p.Z), out block[1]) & ready.TryGetValue(new GridPos(p.X, p.Y + 1, p.Z), out block[2])
                    & ready.TryGetValue(new GridPos(p.X + 1, p.Y + 1, p.Z), out block[3]);
                if (!ok) continue;
                float fert = 0f;
                foreach (var b in block) { if (b.Def != a.Def || used.Contains(b.Id)) ok = false; else fert += FertilityAt(b.Position) / 4f; }
                if (!ok) continue;
                double chance = Config.GiantChance + (fert > Config.GiantRichFertility ? Config.GiantRichBonus : 0.0);
                if (skill >= Config.GiantSkillLevel) chance *= 2.0;
                if (_rng.NextDouble() >= chance) continue;
                var giantDef = DefOf(a.Def.GiantAs);
                if (giantDef == null) continue;
                foreach (var b in block) { used.Add(b.Id); OnHarvest(b); _world.RemoveItem(b.Id); crops.Remove(b); }
                if (_world.CanPlaceFloor(giantDef, p, 0).Ok)
                {
                    var giant = _world.PlaceItem(giantDef, p);
                    GiantGrown?.Invoke(a.Def, giant);
                }
                else
                {
                    // Yer tutmadı (beklenmez): dört ekin olgun hâliyle geri konur.
                    foreach (var b in block)
                        if (_world.CanPlaceFloor(b.Def, b.Position, 0).Ok) { var back = _world.PlaceItem(b.Def, b.Position); SetGrown(back); crops.Add(back); }
                }
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
            List<ItemInstance> rotted = null;
            foreach (var item in _world.Items)
            {
                if (!item.IsPlant) continue;
                if (item.Withered)
                {
                    // Sökülmeyen sararmış ekin bir mevsim sonra çürüyüp toprağa karışır.
                    item.OffSeasonDays += gameDays;
                    if (item.OffSeasonDays >= Config.WitheredRotDays) (rotted ?? (rotted = new List<ItemInstance>())).Add(item);
                    continue;
                }
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
                if (item.Def.WarmOnly && !greenhouse && !InSouth(item.Position)) continue;   // kahve: ılık serada ya da Güney Yamaçları'nda
                if (item.Def.SouthOnly && !InSouth(item.Position)) continue;                  // zeytin: yalnızca Güney Yamaçları'nda

                if (item.Def.IsCrop)
                {
                    // Mevsimi geçen ekin (sera dışında) bir gün içinde sararır; kalıcı yatak uyur ve toplanmamış ürününü yitirir.
                    bool off = !greenhouse && !CanGrow(item.Def, season);
                    if (!off) item.OffSeasonDays = 0;
                    else if (SeasonOver(item.Def, season) || (item.Def.Cycle == Content.CropCycle.Perennial && item.IsGrown))
                    {
                        item.OffSeasonDays += gameDays;
                        if (item.OffSeasonDays >= Config.WitherAfterDays)
                        {
                            if (item.Def.Cycle != Content.CropCycle.Perennial) Wither(item, WitherCause.Season);
                            else { item.GrowthDays = Math.Max(0, item.Def.GrowDays - item.Def.RegrowDays); item.OffSeasonDays = 0; }
                        }
                        continue;
                    }
                }

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
                    if (GreenhouseGrowth != null) rate *= GreenhouseGrowth(room);   // bakımlı sera ×1,1
                }
                else if (!CanGrow(item.Def, season)) continue;   // bu bitki bu mevsimde büyümez

                if (item.Def.IsCrop)
                {
                    var plot = PlotOrNull(item.Position);
                    if (plot != null && plot.BurnUntil > Today) { item.AgeDays += gameDays; continue; }   // taze gübre yaktı: büyüme durdu
                    rate *= Config.GrowthFactor(item.Def.Feed, FertilityAt(item.Position));
                    if (NearActiveHive(item.Position)) rate *= 1f + Config.PollinationGrowth;
                    // Malç toprağın nemini tutar: sulanmamış gün daha az yavaşlatır.
                    if (!item.Watered) rate *= plot != null && plot.MulchUntil > Today ? Math.Max(Config.DryGrowth, Config.MulchDryGrowth) : Config.DryGrowth;
                    item.AgeDays += gameDays;
                    if (item.Watered) item.WetDays += gameDays;
                }
                item.GrowthDays = Math.Min(item.Def.GrowDays, item.GrowthDays + gameDays * rate);
            }
            if (rotted != null)
                foreach (var item in rotted)
                {
                    var plot = PlotAt(item.Position);
                    plot.Fertility = Clamp01(plot.Fertility + Config.RotFertility);
                    _lodged.Remove(item.Id);
                    _shattered.Remove(item.Id);
                    _world.RemoveItem(item.Id);
                    PlantLost?.Invoke(item.Def, item.Position, PlantLoss.Rotted);
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
                if (!item.IsPlant || !item.Def.DiesInWinter || item.Withered) continue;
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
                // Gelişmiş ekin donunca sararır (sökülünce yan ürün verir); filiz donup kaybolur.
                if (item.Def.IsCrop && item.GrowthFraction >= Config.WitherMatureFraction) { Wither(item, WitherCause.Frost); continue; }
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
