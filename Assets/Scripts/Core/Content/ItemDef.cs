using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Eşyanın nereye konabildiği: yere (zemin karolarına) ya da başka eşyanın yüzeyine (dekor).</summary>
    public enum ItemPlacement
    {
        Floor = 0,
        Surface = 1,
        /// <summary>Yerleştirilmez; yalnızca envanterde sayılan kaynak (odun gibi).</summary>
        Resource = 2,
        /// <summary>Duvarın iç yüzüne asılır (meşale, duvar rafı, goblen); karo kaplamaz.</summary>
        Wall = 3,
        /// <summary>Halı: yere serilir, yolu kapatmaz, üstüne mobilya konur; halılar sınırsız üst üste serilir (27-ev-dekorasyonu).</summary>
        Rug = 4,
        /// <summary>Tavan ve kiriş: kapalı iç odada karonun tavanına asılır, yolu kapatmaz; karo başına bir tane.</summary>
        Ceiling = 5
    }

    /// <summary>Dekorun boyut sınıfı (Sims gibi): eşya, kendi sınıfından küçük olmayan yuvaya girer.</summary>
    public enum DecorSize
    {
        Small = 1,   // K: mum, bardak, ot demeti
        Medium = 2,  // O: testi, kitap, saksı
        Large = 3    // B: küp, bakır kazan
    }

    /// <summary>Eşyanın durumu: çöken raftan düşen eşya hasar alır, onarılan eşya değerinin bir kısmını kalıcı kaybeder.</summary>
    public enum ItemCondition
    {
        Sound = 0,
        Damaged = 1,
        Repaired = 2
    }

    /// <summary>Dekor sistemi sözlüğü: yuva türleri, duvar bantları, malzemeler ve durum çarpanları (Project belgesi 27).</summary>
    public static class Decor
    {
        // Yuva türleri ("rafa yalnızca rafa uyumlu eşya konur").
        public const string Table = "masa", Shelf = "raf", Hook = "kanca", Mantel = "ocak", Sill = "pencere",
            Pantry = "kiler", Plate = "tabak", Cushion = "minder", Cover = "ortu";
        public static readonly string[] SlotKinds = { Table, Shelf, Hook, Mantel, Sill, Pantry, Plate, Cushion, Cover };
        /// <summary>Uyduğu türler yazılmamış dekorun varsayılanı: masa, raf, ocak ve pencere içi.</summary>
        public static readonly string[] DefaultFits = { Table, Shelf, Mantel, Sill };

        public static string KindName(string kind)
        {
            switch (kind)
            {
                case Table: return "masa";
                case Shelf: return "raf";
                case Hook: return "kanca";
                case Mantel: return "ocak rafı";
                case Sill: return "pencere içi";
                case Pantry: return "kiler";
                case Plate: return "tabak";
                case Cushion: return "minder";
                case Cover: return "örtü";
                default: return kind;
            }
        }

        public static string SizeName(DecorSize s) => s == DecorSize.Small ? "küçük" : s == DecorSize.Medium ? "orta" : "büyük";
        public static string SizeLetter(DecorSize s) => s == DecorSize.Small ? "K" : s == DecorSize.Medium ? "O" : "B";

        // Duvar bantları: bir kenarın her iç yüzünde üç yükseklik.
        public const int BandLow = 0, BandMid = 1, BandHigh = 2, BandCount = 3;
        public static string BandName(int band) => band == BandLow ? "alt" : band == BandMid ? "orta" : "üst";

        // Malzemeler (çökmede akıbet, onarım yeri).
        public const string Wood = "wood", Metal = "metal", Cloth = "cloth", Clay = "clay", Glass = "glass", Stone = "stone";
        public static readonly string[] Materials = { Wood, Metal, Cloth, Clay, Glass, Stone };
        /// <summary>Kırılgan malzeme: düşünce kırılır (kırık çömlek ya da kırık cam).</summary>
        public static bool IsFragile(string material) => material == Clay || material == Glass;
        public const string BrokenPottery = "item.broken_pottery", BrokenGlass = "item.broken_glass", Debris = "item.debris";

        /// <summary>Durumun satış değeri çarpanı (karar: düşen eşyanın değeri azalır).</summary>
        public static double SellFactor(ItemCondition c) => c == ItemCondition.Damaged ? 0.5 : c == ItemCondition.Repaired ? 0.75 : 1.0;
        /// <summary>Durumun güzellik çarpanı.</summary>
        public static double BeautyFactor(ItemCondition c) => c == ItemCondition.Damaged ? 0.5 : c == ItemCondition.Repaired ? 0.9 : 1.0;
        public static string ConditionName(ItemCondition c) => c == ItemCondition.Damaged ? "Hasarlı" : c == ItemCondition.Repaired ? "Onarılmış" : "Sağlam";

        // ---- D2: tarz ekseni (Project belgesi 27 §6.1) ----
        /// <summary>Tarzlar (görünüş ekseni; kademe = styleSet işlevi belirler). 1.0'da Çatkı ve Yörük.</summary>
        public static readonly (string id, string name)[] Themes =
        {
            ("Çatkı", "Çatkı Evi"), ("Yörük", "Yörük Odası"),
            ("Kıyı", "Kıyı Evi"), ("Han", "Han Salonu"), ("Kervan", "Kervan Odası")   // D4
        };
        /// <summary>Yalnızca gezgin tüccarın getirdiği eşya (Kervan Odası): katalogda yalnızca pazar günü görünür.</summary>
        public const string MerchantTag = "merchant";
        public static string ThemeName(string id)
        {
            foreach (var t in Themes) if (t.id == id) return t.name;
            return id;
        }

        // ---- D2: doldurulan dekor (§9) ----
        /// <summary>İçerik tazeliği: taze, bayat (raf ömrünün %60'ı), çürük (raf ömrü doldu).</summary>
        public const int Fresh = 0, Stale = 1, Rotten = 2;
        public const double StaleAt = 0.6;
        public static int Freshness(ItemDef food, double age)
        {
            if (food == null || food.SpoilDays <= 0f) return Fresh;
            if (age >= food.SpoilDays) return Rotten;
            return age >= food.SpoilDays * StaleAt ? Stale : Fresh;
        }
        public static string FreshnessName(int f) => f == Rotten ? "çürük" : f == Stale ? "bayat" : "taze";
        /// <summary>Hanenin kendi ürettiği içerikle tam dolu dekorun güzellik eki ("Kendi emeğimiz").</summary>
        /// <summary>Ağzı bağlı kap (cam kavanoz): içindeki yiyecek bu çarpanla yavaş bozulur (D3 tamamlama).</summary>
        public const string SealedTag = "sealed";
        public const double SealedAgeFactor = 0.25;
        public const int OwnWorkBonus = 2;
        /// <summary>Çürük içerikli dekorun güzelliği (sinek).</summary>
        public const int RottenBeauty = -4;
    }

    /// <summary>Bir eşyanın yüzeyindeki dekor yuvası. U,V: kapladığı alanın içinde 0-1 arası yerel konum.</summary>
    public sealed class SurfaceSlotDef
    {
        public string Id { get; set; }
        public float U { get; set; }
        public float V { get; set; }
        /// <summary>Örtü yuvası mı (masa örtüsü, sedir örtüsü): eşyanın bütün üstünü kaplar.</summary>
        public bool IsCover => Kind == Decor.Cover;
        /// <summary>Yuva türü (Decor.Table, Decor.Shelf ...). Boşsa: duvar eşyasında raf, tavanda kanca, diğerlerinde masa.</summary>
        public string Kind { get; set; }
        /// <summary>Yuvanın boyutu: bundan büyük dekor girmez.</summary>
        public DecorSize Size { get; set; } = DecorSize.Large;
    }

    /// <summary>Yerleştirilebilir/kullanılabilir eşya tanımı. İçerik sonradan paketlerle genişler.</summary>
    /// <summary>Güçlü makinenin günlük işi: envanterden girdi alır, ürün koyar.</summary>
    public sealed class AutoProcessDef
    {
        public string Item { get; set; }
        public int Count { get; set; } = 1;
        public string Produces { get; set; }
        public int ProducesCount { get; set; } = 1;
    }

    /// <summary>Barınak odası eşyalarının rolleri.</summary>
    public static class AnimalRoles
    {
        public const string Water = "water", Bedding = "bedding", Collector = "collector", Feeder = "feeder", Bell = "bell";
    }

    public sealed class ItemDef : IContentDef
    {
        public const int MaxSize = 6;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<InteractionDef> Interactions { get; set; } = new List<InteractionDef>();

        /// <summary>Dönmemiş haldeki kapladığı karo sayısı (1x1, bir yatak 2x1 ...).</summary>
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public ItemPlacement Placement { get; set; } = ItemPlacement.Floor;
        /// <summary>Üstüne dekor konabilen yüzey yuvaları (masa, raf).</summary>
        public List<SurfaceSlotDef> SurfaceSlots { get; set; } = new List<SurfaceSlotDef>();
        /// <summary>Akçe cinsinden fiyat (satış bunun yarısı). 0 = ücretsiz/başlangıç eşyası.</summary>
        public int Price { get; set; }
        /// <summary>Yapı modu menüsü kategorisi (furniture, decor, utility ...).</summary>
        public string Category { get; set; } = "furniture";
        /// <summary>Yanarken bulunduğu odaya verdiği ısı gücü (0 = ısı kaynağı değil). Yakıt gerektirir.</summary>
        public float HeatOutput { get; set; }

        /// <summary>Bitki ise olgunlaşma süresi (oyun günü, yalnızca dış mekânda ve kış dışında büyür). 0 = bitki değil.</summary>
        public float GrowDays { get; set; }
        /// <summary>Fidan/tohum kaynağı ise dikilince oluşan bitkinin kimliği.</summary>
        public string PlantsAs { get; set; }

        public bool IsPlant => GrowDays > 0f;
        /// <summary>Depolama eşyası ise kaynak kapasitesine eklediği birim (sandık 40). 0 = depo değil.</summary>
        public int Storage { get; set; }
        /// <summary>Pazar etiketleri (fuel, food ...): mevsimlik talep çarpanları bunlara göre.</summary>
        public List<string> Tags { get; set; } = new List<string>();
        public bool HasTag(string tag) => Tags.Contains(tag);
        /// <summary>Dükkânda görünmesi için gereken hesap seviyesi (T10). 0/1 = baştan açık.</summary>
        public int UnlockLevel { get; set; } = 1;
        /// <summary>Ekin: yalnızca toprak (sürülmüş) zemine dikilir.</summary>
        public bool NeedsTilledSoil { get; set; }
        /// <summary>Kış gelince donar ve kaybolur (hasat edilmemiş ekin).</summary>
        public bool DiesInWinter { get; set; }
        /// <summary>Yalnızca ılık serada büyür ve meyve verir (kahve çalısı, belge 32).</summary>
        public bool WarmOnly { get; set; }
        /// <summary>Ekinin büyüyebildiği mevsimler; boşsa kış dışı hepsi.</summary>
        public List<Time.Season> GrowSeasons { get; set; } = new List<Time.Season>();
        /// <summary>Ekin (tarla bitkisi): toprağa ekilen, sulanması ve verimli toprağı gereken bitki. Ağaçlar ekin değildir.</summary>
        public bool IsCrop => IsPlant && NeedsTilledSoil;
        /// <summary>Meyve ağacı: bu mevsimlerde meyve verir (olgun ağaç kesilmeden tekrar tekrar toplanır). Boş = meyvesiz.</summary>
        public List<Time.Season> FruitSeasons { get; set; } = new List<Time.Season>();
        /// <summary>Meyvenin olgunlaşması için gereken gün (her toplamadan sonra yeniden başlar).</summary>
        public float FruitRipenDays { get; set; }
        public bool IsFruitTree => FruitSeasons.Count > 0 && FruitRipenDays > 0f;
        /// <summary>Baklagil: hasatta toprağı zenginleştirir (azot bağlar).</summary>
        public bool FixesNitrogen { get; set; }
        /// <summary>Ekin ailesi (tahıl, baklagil, kök, lahanagil, soğangil, kabakgil, lif): nöbetleşe ekim aileye bakar.</summary>
        public string CropFamily { get; set; }
        /// <summary>Ekin demeti: harmanda dövülünce bu taneye (ve samana) döner; boş = demet değil.</summary>
        public string ThreshesTo { get; set; }
        /// <summary>Düven: çeki hayvanı varken her sabah envanterdeki demetlerden bu kadarını döver. 0 = düven değil.</summary>
        public int ThreshPerDay { get; set; }
        /// <summary>Sulama hendeği, kova ve oluk: her sabah bu yarıçaptaki (Chebyshev) ekinleri sular. 0 = sulamaz.</summary>
        public int WaterRadius { get; set; }
        /// <summary>Arı kovanı: yaşayan koloni varken bu yarıçaptaki ekinler ve meyve ağaçları daha çabuk olgunlaşır. 0 = tozlaştırmaz.</summary>
        public int PollinateRadius { get; set; }
        /// <summary>Korkuluk: bu yarıçaptaki ekinleri kuzgunlardan korur. 0 = korumaz.</summary>
        public int ProtectRadius { get; set; }
        /// <summary>Oda konforuna katkısı (dekor ve mobilya). Adım 32.</summary>
        public int Comfort { get; set; }
        /// <summary>Taze yiyecek bozulma süresi (oyun günü); 0 = çürümez.</summary>
        public float SpoilDays { get; set; }
        /// <summary>Kiler eşyası: yiyecek bozulmasını bu çarpanla yavaşlatır (0 = kiler değil).</summary>
        public float Preserves { get; set; }
        public List<string> Variants { get; set; } = new List<string>();
        /// <summary>Mobilya kaplama grubu (wood, cloth, metal, clay); boşsa renk/malzeme seçeneği yok.</summary>
        public string FinishGroup { get; set; }
        public bool IsRug => Placement == ItemPlacement.Rug;
        public bool IsCeiling => Placement == ItemPlacement.Ceiling;

        // ---- ev dekorasyonu (Project belgesi 27, D1) ----
        /// <summary>Dekorun boyut sınıfı (yüzeye konan eşya).</summary>
        public DecorSize DecorSize { get; set; } = DecorSize.Small;
        /// <summary>Dekorun yükü (1-12): rafın taşıması aşılırsa raf çökebilir.</summary>
        public int Weight { get; set; } = 1;
        /// <summary>Dekorun konabildiği yuva türleri (Decor.Table ...). Boşsa Decor.DefaultFits.</summary>
        public List<string> FitsOn { get; set; } = new List<string>();
        /// <summary>Yüzeyin toplam taşıması (bütün yuvalar). 0 = sınırsız (eski içerik).</summary>
        public int MaxLoad { get; set; }
        /// <summary>Duvar eşyasının asılabildiği bantlar (Decor.BandLow ...). Boşsa yalnızca orta.</summary>
        public List<int> WallBands { get; set; } = new List<int>();
        /// <summary>Duvar eşyasının genişliği (kenar sayısı, 1-3).</summary>
        public int WallWidth { get; set; } = 1;
        /// <summary>Uzun mobilya: önündeki duvarın alt ve orta bandını kapatır (dolap, yer rafı, büyük şömine).</summary>
        public bool Tall { get; set; }
        /// <summary>Odanın güzellik puanına katkısı (görünüş). Konfor (Comfort) fiziksel rahatlıktır, ayrı tutulur.</summary>
        public int Beauty { get; set; }
        /// <summary>Malzeme (Decor.Wood ...): çöken raftan düşünce kırılır mı, nerede onarılır.</summary>
        public string Material { get; set; }
        /// <summary>Etkin malzeme: yazılmamışsa kaplama grubundan, o da yoksa ahşap.</summary>
        public string EffectiveMaterial => !string.IsNullOrEmpty(Material) ? Material
            : FinishGroup == "cloth" ? Decor.Cloth : FinishGroup == "metal" ? Decor.Metal : FinishGroup == "clay" ? Decor.Clay : Decor.Wood;
        /// <summary>Ayna: evde bulunursa görünüş değiştirilebilir (Mirror).</summary>
        public bool IsMirror { get; set; }
        /// <summary>Metal ayna: bu kadar günde kararır (güzelliği yarıya iner, "Cilala" düzeltir). 0 = kararmaz.</summary>
        public float TarnishDays { get; set; }
        /// <summary>Tarz ekseni (Çatkı, Yörük ...): kademeden (StyleSet) bağımsız görünüş ailesi.</summary>
        public string StyleTheme { get; set; }

        // ---- D2 ----
        /// <summary>Doldurulan dekor (meyve sepeti, peynir tahtası): kabul ettiği gerçek ürünler (eşya kimlikleri, tercih sırasıyla).</summary>
        public List<string> FillWith { get; set; } = new List<string>();
        /// <summary>Doldurulan dekorun aldığı adet (0 = doldurulmaz).</summary>
        public int FillCount { get; set; }
        public bool IsFillDecor => FillCount > 0 && FillWith.Count > 0;
        public bool AcceptsFill(string itemId) { foreach (var f in FillWith) if (f == itemId) return true; return false; }
        /// <summary>Yalnızca pencere kenarına asılır (pencere içi tahtası, perde); düz duvara değil.</summary>
        public bool WindowMount { get; set; }
        /// <summary>Asıldığı kenarın ısı kaybı çarpanı (perde 0,75; kumaş duvar örtüsü 0,85). 0 = etkisiz.</summary>
        public float Insulation { get; set; }
        /// <summary>Bu istasyonda üretim için yakında olması gereken eşya (camcı tezgâhı → cam ocağı); null ise yok.</summary>
        public string RequiresNearby { get; set; }
        /// <summary>RequiresNearby için en çok uzaklık (karo, ayak izleri arası).</summary>
        public int NearbyRadius { get; set; }
        /// <summary>RequiresNearby'nin görünen adı (içerik yüklenirken doldurulur).</summary>
        public string RequiresNearbyName { get; set; }
        /// <summary>
        /// "craftonly" eşya bu kilit açılınca katalogdan da alınır (cam şişe: "glassworks" = köye camcılık geldi). null ise hep yalnızca yapılır.
        /// </summary>
        public string ShopUnlock { get; set; }

        // ---- D3b ----
        /// <summary>Mevsim süsü: yalnızca bu mevsimlerde taze durur; mevsim dönünce solar (güzelliği eksiye döner). Boş = mevsimsiz.</summary>
        public List<Time.Season> DecorSeasons { get; set; } = new List<Time.Season>();
        public bool IsSeasonal => DecorSeasons.Count > 0;
        public bool InSeason(Time.Season s) => DecorSeasons.Count == 0 || DecorSeasons.Contains(s);
        /// <summary>Kazanılan hatıra (şenlik kupası, ilk ayakkabı): satın alınmaz, satılmaz; yazısı hatıra defterinden gelir.</summary>
        public bool IsKeepsake => HasTag("keepsake");

        /// <summary>Dekorun konabildiği yuva türleri (varsayılan dahil).</summary>
        public IReadOnlyList<string> EffectiveFits => FitsOn.Count > 0 ? (IReadOnlyList<string>)FitsOn : Decor.DefaultFits;
        public bool FitsKind(string kind) { foreach (var k in EffectiveFits) if (k == kind) return true; return false; }
        /// <summary>Duvar eşyasının bantları (varsayılan: orta).</summary>
        public IReadOnlyList<int> EffectiveBands => WallBands.Count > 0 ? (IReadOnlyList<int>)WallBands : new[] { Decor.BandMid };
        public bool AllowsBand(int band) { foreach (var b in EffectiveBands) if (b == band) return true; return false; }
        public int DefaultBand => EffectiveBands[0];

        /// <summary>Yuvanın etkin türü (boşsa yerleşime göre varsayılan).</summary>
        public string SlotKind(SurfaceSlotDef slot) => !string.IsNullOrEmpty(slot.Kind) ? slot.Kind
            : Placement == ItemPlacement.Wall ? Decor.Shelf : Placement == ItemPlacement.Ceiling ? Decor.Hook : Decor.Table;

        // ---- hayvancılık ve otomasyon ----
        /// <summary>Buzluk: içinde buz varken (FuelDays = kalan buz günü) yiyeceği çok soğuk tutar.</summary>
        public bool IsIceBox { get; set; }
        /// <summary>Balık sepeti: bir gölete değerken her sabah mevsimine göre bir balık.</summary>
        public bool IsFishTrap { get; set; }
        /// <summary>Yel değirmeni: kanal gerekmeden güç verir.</summary>
        public bool WindPower { get; set; }
        /// <summary>Su kapağı: açıkken sulama ağında suyu geçirir, kapalıyken keser (dokununca aç/kapa).</summary>
        public bool WaterGate { get; set; }
        /// <summary>Lazımlık/hela: bu kadar kullanımda dolar, boşaltılınca FillPerProduct dolum başına bir FillProduct (gübre).</summary>
        public int FillCapacity { get; set; }
        public string FillProduct { get; set; }
        public int FillPerProduct { get; set; } = 4;
        /// <summary>Hasat tırmığı ve ekim makinesi yarıçapı (güç ister).</summary>
        public int HarvestRadius { get; set; }
        public int SeedRadius { get; set; }
        /// <summary>Buz kuyusu: donlu her sabah toplanan buz.</summary>
        public int IceCollect { get; set; }
        /// <summary>Güç alınca çalışan düzenek mi (değirmen taşı, yem karıştırıcı, hasat tırmığı, ekim makinesi)?</summary>
        public bool NeedsPower => (AutoProcess != null && !IsPassive) || HarvestRadius > 0 || SeedRadius > 0;
        /// <summary>Güç istemeyen sabah işi (kömür yığını, kavurma yığını, kireç ocağı): her sabah bir parti, ateşle/zamanla.</summary>
        public bool IsPassive => AutoProcess != null && HasTag("passive");
        /// <summary>Yiyecek yerleşimindeki kapasite (0 = depo kapasitesi, o da yoksa kiler/buzluk için 60).</summary>
        public int FoodCapacity { get; set; }
        /// <summary>Yalnızca kapalı bir odaya konur (folluk, ahır bölmesi, at bölmesi): barınak odası.</summary>
        public bool RequiresIndoor { get; set; }
        /// <summary>Yemlik: aynı odadaki hayvanlar buradaki yemden yer.</summary>
        public bool IsTrough { get; set; }
        /// <summary>Hayvan konforuna süs katkısı.</summary>
        public int AnimalComfort { get; set; }
        /// <summary>Barınak odasındaki rolü: water, bedding, collector, feeder, bell (AnimalRoles).</summary>
        public string AnimalRole { get; set; }
        /// <summary>Sulama ağının kaynağı (kuyu, kova-makara).</summary>
        public bool WaterSource { get; set; }
        /// <summary>Yalnızca bir kaynağa bağlı sulama ağındayken sular (kanal).</summary>
        public bool NeedsWaterSource { get; set; }
        /// <summary>Su çarkı: kaynağa bağlı bir kanala değince yanındaki makinelere güç verir.</summary>
        public bool PowerSource { get; set; }

        // ---- su şebekesi ve tesisat (17.11.2026) ----
        /// <summary>Kaynağın günde boruya verdiği su (birim); 0 = varsayılan (DefaultWaterFlow), -1 = vermez (kuyudan su kovayla çekilir).</summary>
        public int WaterFlow { get; set; }
        public const int DefaultWaterFlow = 4;
        /// <summary>Kaynaksa günlük suyu (WaterFlow ya da varsayılan); kaynak değilse 0.</summary>
        public int EffectiveWaterFlow => WaterSource ? (WaterFlow > 0 ? WaterFlow : WaterFlow < 0 ? 0 : DefaultWaterFlow) : 0;
        /// <summary>Tesisat: bir su yoluna ya da kaynağa bağlıyken günde bu kadar su ister (leğen, küvet, akarsulu hela, çeşme). 0 = tesisat değil.</summary>
        public int WaterUse { get; set; }
        public bool IsFixture => WaterUse > 0;
        /// <summary>Su terazisi: ağa basınç verir; üst kattaki tesisata kurşun boru kolonuyla su çıkar.</summary>
        public bool WaterHead { get; set; }
        /// <summary>Sarnıç / yağmur fıçısı: bu kadar su biriktirir (Fill = içindeki su). 0 = sarnıç değil.</summary>
        public int CisternCapacity { get; set; }
        /// <summary>Yağmurlu gün sarnıca dolan su.</summary>
        public int CisternRain { get; set; }
        /// <summary>Sarnıcın günde en çok verdiği su (0 = DefaultWaterFlow).</summary>
        public int CisternFlow { get; set; }
        public bool IsCistern => CisternCapacity > 0;
        /// <summary>Çeşme ve lüle: su gelirken yanından kova doldurulur (banyo için kuyuya gitmek gerekmez).</summary>
        public bool IsTap { get; set; }
        /// <summary>Güç alınca her gün kendiliğinden yaptığı iş (değirmen taşı: arpa → yem).</summary>
        public AutoProcessDef AutoProcess { get; set; }
        /// <summary>Yabani toplanabilir: görüneceği mevsimler ve haritada en çok kaç tane.</summary>
        public List<Time.Season> ForageSeasons { get; set; } = new List<Time.Season>();
        public int ForageMax { get; set; }
        public bool IsForage => ForageSeasons.Count > 0 && ForageMax > 0;

        /// <summary>Stil seti (Köylü, Zanaatkâr, Tüccar, Asil): aynı odada 4+ eşya "Uyumlu ev" verir. Boş = setsiz.</summary>
        public string StyleSet { get; set; }
        /// <summary>Satın alma menüsünde oda filtresi (bedroom, kitchen, living, outdoor, kids).</summary>
        public List<string> Rooms { get; set; } = new List<string>();
        /// <summary>Hayvan barınağı türü (coop, barn, kennel ...); boş = barınak değil.</summary>
        public string HousingKind { get; set; }
        /// <summary>Barınağın alabileceği hayvan sayısı.</summary>
        public int HousingCapacity { get; set; }
        public bool IsHousing => !string.IsNullOrEmpty(HousingKind);

        /// <summary>Merdiven ise kat farkı: +1 yukarı çıkar, -1 aşağı (bodrum) iner; 0 = merdiven değil. Ayak izi 1x2'dir.</summary>
        public int StairsDelta { get; set; }
        public bool IsStairs => StairsDelta != 0;
        /// <summary>Eşyanın konabileceği katlar; boşsa kurala göre (bkz. AllowsLevel).</summary>
        public List<int> AllowedLevels { get; set; } = new List<int>();

        /// <summary>
        /// Bu katta durabilir mi: açıkça belirtilen katlar varsa onlar; yoksa bitki, barınak ve toplama düğümü
        /// yalnızca zemin katta, geri kalan her katta (üst kat, bodrum).
        /// </summary>
        public bool AllowsLevel(int level)
        {
            if (AllowedLevels.Count > 0) return AllowedLevels.Contains(level);
            if (IsPlant || IsHousing || IsForage || PlantsAs != null) return level == 0;
            return true;
        }

        public SurfaceSlotDef FindSlot(string slotId)
        {
            foreach (var s in SurfaceSlots)
                if (s.Id == slotId) return s;
            return null;
        }
    }
}
