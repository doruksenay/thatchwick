using System;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>Yerdeki yığının türü (belge 57 §3, 57d §5).</summary>
    public enum PileKind
    {
        /// <summary>İnsan pisliği: açıkta tuvaletten ya da kazadan.</summary>
        Human = 0,
        /// <summary>Köpek pisliği: hafif kural; komposta girmez.</summary>
        Dog = 1,
        /// <summary>Tezek ve kığı: merada otlayan hayvandan.</summary>
        Animal = 2,
        /// <summary>Süprüntü: kompost yokken atılan çürük ve döküntü.</summary>
        Refuse = 3
    }

    /// <summary>Yığının evresi: taze, sinekli, kokmuş, kurumuş.</summary>
    public enum PileStage { Fresh = 0, Flies = 1, Reeking = 2, Dried = 3 }

    /// <summary>Koku kaynağı: konumu ve kademesi (0 temiz, 1 sinekli, 2 kokmuş).</summary>
    public readonly struct SmellSource
    {
        public readonly int ItemId;
        public readonly GridPos Position;
        public readonly int Tier;
        public readonly bool Outdoor;
        /// <summary>Kaynağın türü: pile, privy, compost, barn, decor, heap.</summary>
        public readonly string Kind;
        public SmellSource(int itemId, GridPos position, int tier, bool outdoor, string kind)
        {
            ItemId = itemId; Position = position; Tier = tier; Outdoor = outdoor; Kind = kind;
        }
    }

    /// <summary>Avlu sisteminin sayıları (belge 57-57e). Hepsi başlangıç değeridir; denge testleri bunları sabitler.</summary>
    public sealed class YardConfig
    {
        // ---- yığınlar ----
        /// <summary>İnsan pisliği: sineklenme, kokma, kuruma, toprağa karışma (oyun günü, 20°C).</summary>
        public double HumanFlies = 0.125, HumanReek = 1.0, HumanDry = 2.0, HumanGone = 4.0;
        /// <summary>Köpek pisliği: sineklenir, kokmaz, 2 günde kaybolur.</summary>
        public double DogFlies = 0.125, DogGone = 2.0;
        /// <summary>Tezek: sineklenir, kurur, toprağa karışır.</summary>
        public double AnimalFlies = 0.25, AnimalDry = 2.0, AnimalGone = 4.0;
        /// <summary>Süprüntü: bir günde sineklenir, üç günde kokar; kendiliğinden gitmez.</summary>
        public double RefuseFlies = 1.0, RefuseReek = 3.0;
        /// <summary>Toprağa karışan yığının karoya kattığı verim.</summary>
        public float DecayFertility = 0.05f;
        /// <summary>Bu sıcaklığın altında sinek yok ve ilerleme yavaş; donda durur; sıcakta hızlanır.</summary>
        public float FlyMinTemp = 8f, HotTemp = 22f;
        public double ColdAgeFactor = 0.3, HotAgeFactor = 1.5;
        /// <summary>Haritadaki yığın tavanı (performans).</summary>
        public int MaxPiles = 60;

        // ---- koku ve sinek ----
        public int SmellMoodRadius = 2, AppetiteRadius = 3, BeautyRadiusTier1 = 2, BeautyRadiusTier2 = 3;
        public int BeautyPenaltyTier1 = 1, BeautyPenaltyTier2 = 3;
        /// <summary>Evin içinde sinek varken açıkta kalan yiyeceğin bozulma çarpanı (Z1).</summary>
        public double FlySpoilFactor = 1.2;
        /// <summary>Kırlangıç yuvasının sinekten koruduğu yarıçap (karo).</summary>
        public int SwallowRadius = 6;
        /// <summary>Kompost avlusu (57c D7): yığının bu kadar karo yakınındaki kümes hayvanı sinek yer, gübresi yığına
        /// eklenir (kuş başına günde yaş birim).</summary>
        public int PoultryHeapRadius = 2;
        public double PoultryDroppings = 0.25;
        /// <summary>Su gücüyle kemik değirmeni günde en çok bu kadar kemik öğütür (2 kemik → 2 kemik unu).</summary>
        public int BoneMillPerDay = 4;
        /// <summary>Depoya sığmayıp açıkta kalan yiyecek bu adedi geçince mutfakta sinek olur; iki gün sürerse fare gelir.</summary>
        public int OpenFoodFlies = 3, OpenFoodMiceDays = 2;
        /// <summary>Yağmurda örtüsüz arabayla pazara inen malın kaybı (örtülü araba korur).</summary>
        public double RainSaleLoss = 0.10;
        /// <summary>Tekerlek takmak için gereken Marangozluk seviyesi.</summary>
        public int RepairCarpentry = 2;

        // ---- açıkta tuvalet ----
        public float ReliefRestore = 70f;
        public double ReliefDays = 0.02;
        public int CoverSearchRadius = 8, WitnessRadiusOpen = 6, WitnessRadiusCovered = 2;
        public float ClosenessLoss = 5f;

        // ---- basma ----
        public double StepChance = 0.25, StepChanceDark = 0.6;
        public float StepHygieneLoss = 10f;

        // ---- sağlık (Z2, Z3) ----
        public double DirtyHandsAche = 0.25, FliesAche = 0.15, RawFilthAche = 0.30, MurkyWaterAche = 0.10;
        public float HandCollectHygiene = 20f;
        public int WaterFoulRadius = 3, WaterFoulPiles = 3;

        // ---- ahır ----
        /// <summary>Hayvan başına günlük kirlilik: büyükbaş ve at, orta boy, kümes.</summary>
        public float DirtLarge = 12f, DirtMedium = 8f, DirtSmall = 3f;
        public float DirtyAt = 50f, FilthyAt = 80f;
        /// <summary>Görevli bu kirlilikten sonra temizler; meradaki günlerde ahır bu oranda kirlenir.</summary>
        public float CleanAt = 30f, GrazingDirtFactor = 0.6f;
        /// <summary>Temizlikte çıkacak pislik biriminin tavanı: olağan ve derin altlıklı.</summary>
        public float DungBankCap = 12f, DeepLitterBank = 48f;
        public float DirtyComfort = 5f, FilthyComfort = 15f;
        public double DirtySick = 0.004, FilthySick = 0.015;
        /// <summary>Temizlikte kirlilik başına çıkan pislik birimi (4 birim = 1 gübre).</summary>
        public float DungPerDirt = 0.1f;
        public int DungPerManure = 4;
        /// <summary>Taze gübre yığında bu kadar günde olgunlaşır (yanmış gübre ekini yakmaz).</summary>
        public double ManureAgeDays = 6.0;
        /// <summary>Derin altlık: kirlilik bu oranda birikir; büyükbaş başına günde bir saman ister.</summary>
        public float DeepLitterFactor = 0.25f;
        /// <summary>Akıtma oluğu olan ahırda kirlilik bu oranda birikir.</summary>
        public float DrainFactor = 0.5f;
        /// <summary>Gübre kapağı (yanında gübre yığınıyla) temizliği bu kadar hızlandırır.</summary>
        public double HatchSpeed = 2.0;
        /// <summary>Otlayan hayvanın merada yığın bırakma şansı (gün başına).</summary>
        public double PastureDropChance = 0.20;
        public double DogDropChance = 0.60;
        /// <summary>Ağıla alma: sürü nadasta geceler; çevresindeki karolar gecede bu kadar verim alır.</summary>
        public float FoldEnrich = 0.04f, FoldCap = 0.70f;
        public int FoldRadius = 1, FoldFullHerd = 4;

        // ---- kompost ----
        public int HeapCapacity = 20, BaysFactor = 3, TumblerCapacity = 12;
        /// <summary>Bu dolgunun altındaki yığın ısınmaz: yalnızca ham kompost verir.</summary>
        public int HeapMinFill = 10, TumblerMinFill = 6;
        public double HeapDays = 7.0, TumblerDays = 4.5, ColdHeapDays = 12.0;
        /// <summary>Yaş payı bu aralıktaysa yığın dengelidir.</summary>
        public double BalanceLow = 0.35, BalanceHigh = 0.65;
        public double WetSpeed = 0.7, DrySpeed = 0.5, TurnedSpeed = 1.5, NeglectedSpeed = 0.6, ShredderSpeed = 1.3, BaysSpeed = 1.15;
        /// <summary>Çevirme bu kadar gün "taze" sayılır; bu kadar gün çevrilmeyen yığın bakımsızdır.</summary>
        public double TurnFreshDays = 2.0, NeglectDays = 3.0;
        /// <summary>İyi geçen günlerin oranı: kara toprak ve olgun kompost eşikleri.</summary>
        public double BlackSoilShare = 0.75, MatureShare = 0.40;
        /// <summary>Külün payı bunu aşarsa kalite bir kademe düşer.</summary>
        public double AshLimit = 0.10;
        /// <summary>Çıkan kompost: girdinin bu kadarı.</summary>
        public double OutputShare = 0.5;
        public int AddUnits = 5;
        /// <summary>Solucan kasası: günde bir yaş girdi yer; üç girdide bir kompost ve bir solucan.</summary>
        public int WormPerCompost = 3;
        /// <summary>Yaprak ağılı: yaprak bu kadar günde malça döner.</summary>
        public double LeafMouldDays = 20.0;
        /// <summary>Şerbet fıçısı: bu kadar günde olur, bu kadar gün yeter.</summary>
        public double SlurryBrewDays = 2.0, SlurryLastDays = 6.0;
        public int SlurryManure = 2;

        // ---- fare (Z7) ----
        public double MiceAttractAge = 3.0;
        public int MiceMax = 3;

        // ---- araba filosu ----
        public int WheelbarrowTiles = 4, HorseCartTiles = 8, OxCartTiles = 12, CoveredCartTiles = 10, SledTiles = 8;
        public double WheelWearPerTrip = 0.02;
        /// <summary>Yağmurlu günde sundurmasız duran arabanın yıpranması; sundurmanın koruduğu yarıçap (karo).</summary>
        public double RainWear = 0.01;
        public int ShedRadius = 2;
        /// <summary>Arabanın gün içindeki seferi: çıkış, yolda olma ve dönüş (gün kesri).</summary>
        public double TripLeave = 0.27, TripGone = 0.33, TripBack = 0.70, TripHome = 0.76;
        /// <summary>Kâhya arabalara görev dağıtmak için bu Kâhyalık seviyesini ister.</summary>
        public int StewardCartLevel = 4;
        /// <summary>Üst üste bu kadar gün çalışan çeki hayvanı ertesi gün dinlenir.</summary>
        public int MaxWorkStreak = 3;
        public float FertilizeBelow = 0.5f;
        public int CoastSeaweedPerTrip = 4, SouthMarlPerTrip = 3;
        public double HaulBonusPerCart = 0.15, HaulBonusMax = 0.30;

        public double AgeFactor(float temperature) =>
            temperature <= 0f ? 0.0 : temperature < FlyMinTemp ? ColdAgeFactor : temperature >= HotTemp ? HotAgeFactor : 1.0;
    }
}
