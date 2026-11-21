using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Ekinin beslenme tipi (belge 57c §2). Gerçek tarımdaki "çok yiyen, az yiyen, toprak yapan" ayrımı ve gübreye
    /// verilen tepki: kök ve soğan taze gübreden zarar görür, yağsız toprak seven fazla gübrede kalite kaybeder.
    /// </summary>
    public enum FeedType
    {
        /// <summary>Orta: bugünkü varsayılan davranış.</summary>
        Medium = 0,
        /// <summary>Obur: çok çeker, gübresiz belirgin yavaş; iyi gübreyle en hızlı ve en kaliteli.</summary>
        Heavy = 1,
        /// <summary>Kanaatkâr (kök, soğan): az çeker; taze gübre zarar verir.</summary>
        Light = 2,
        /// <summary>Yağsız toprak seven (ot, asma, çavdar): fazla verimde miktar artar, kalite düşer.</summary>
        Lean = 3,
        /// <summary>Toprak yapan (baklagil, fiğ): toprağa verim katar; fazla gübrede yaprağa kaçar.</summary>
        Builder = 4
    }

    /// <summary>Gübre türleri (belge 57c §2.2, 57d §3). Bayrak: bir ekim döngüsünde serilenler toprak karosunda birikir.</summary>
    [Flags]
    public enum Fertilizer
    {
        None = 0,
        /// <summary>Türü verilmemiş eski "fertilize" etkileşimi (bereket iksiri, balık unu...).</summary>
        Generic = 1,
        CompostRaw = 2,
        Compost = 4,
        BlackSoil = 8,
        /// <summary>Taze gübre: boş toprağa tam etki; ekili karoyu yakar.</summary>
        FreshManure = 16,
        /// <summary>Güvercin ve kümes gübresi: sert; yalnızca boş toprağa ve obur ekine.</summary>
        HotManure = 32,
        BoneMeal = 64,
        Ash = 128,
        /// <summary>Deniz yosunu: taze serilir, yakmaz, otu bastırır.</summary>
        Seaweed = 256,
        /// <summary>Marn: verim eklemez; nadas tavanını yıllarca yükseltir.</summary>
        Marl = 512,
        /// <summary>Yaprak çürüğü (malç): verim eklemez; su tutar, otu bastırır.</summary>
        Mulch = 1024,
        Lime = 2048,
        /// <summary>Olgun gübre: yığında bekleyip yanmış ahır gübresi; yakmaz (belge 57c).</summary>
        Manure = 4096
    }

    /// <summary>Ekinin hasat döngüsü (Project belgesi 59 §1).</summary>
    public enum CropCycle
    {
        /// <summary>Biçilir, karo boşalır (kökler, tahıllar, soğangil).</summary>
        Single,
        /// <summary>Devşirme: ilk üründen sonra birkaç kez daha verir, sonra sararır (bezelye, bakla).</summary>
        Repeat,
        /// <summary>Kes, yine gelir: yaprak kesilir, bitki kalır; birkaç kesimden sonra tohuma kaçar (ıspanak, marul).</summary>
        Cut,
        /// <summary>Kalıcı yatak: bir kez dikilir, her yıl mevsiminde verir; mevsim dışında uyur (safran, kökboya).</summary>
        Perennial
    }

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
        // ---- belge 45 ----
        /// <summary>Gömme eşya: yalnızca bu duvar malzemelerine oyulur.</summary>
        public const string NicheTag = "niche";
        public static readonly string[] ThickWalls = { "wall_cob", "wall_stone", "wall_ashlar", "wall_rubble_lime", "wall_brick", "wall_black_stone", "wall_red_ashlar", "wall_marble" };
        /// <summary>Sergi rafı: aynı eşyadan ikincisini almaz (koleksiyon).</summary>
        public const string DistinctTag = "distinct";

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

    /// <summary>Eşyanın durabildiği yer: kapalı oda, açık hava ya da ikisi.</summary>
    public enum ItemSite { Any = 0, Indoor = 1, Outdoor = 2 }

    /// <summary>Yerleşim sözlüğü (belge 44): ön yüz kullanımları, ilişki kuralları ve oda türleri.</summary>
    public static class Placements
    {
        public const string Sit = "sit", Open = "open", Work = "work", Stand = "stand";
        public static readonly string[] FrontUses = { Sit, Open, Work, Stand };
        public const string FaceTable = "faceTable", Bedside = "bedside", NearHearth = "nearHearth";
        public static readonly string[] Relations = { FaceTable, Bedside, NearHearth };
        public const string Bedroom = "bedroom", Kitchen = "kitchen", Dining = "dining", Living = "living", Pantry = "pantry",
            Cellar = "cellar", Storehouse = "storehouse", Workshop = "workshop", Shop = "shop", InnRoom = "inn_room",
            Barn = "barn", Outdoor = "outdoor", Kids = "kids";
        public static readonly string[] RoomTypes = { Bedroom, Kitchen, Dining, Living, Pantry, Cellar, Storehouse, Workshop, Shop, InnRoom, Barn, Outdoor, Kids };
        /// <summary>Meslek etiketleri (Economy.Shops kimlikleriyle aynı; hayvan pazarı "animals").</summary>
        public static readonly string[] Trades = { "tavern", "general", "scribe", "herb", "smithy", "carpenter", "mill", "tailor", "potter", "fisher", "saddler", "mason", "animals" };
    }

    /// <summary>
    /// Eşyanın görselleri (yerleşim sistemi). Yön görselleri Faces[Dir4] (0 N=ekranda se, 1 E=ne, 2 S=nw, 3 W=sw);
    /// eksen görselleri AlongX (çift dönüş) ve AlongY (tek dönüş). Mirror: sol/sağ simetrik, yatay ayna geçerli.
    /// Görsel boyutu ve pivot ayak izinden ayrı tutulur.
    /// </summary>
    public sealed class ItemArtDef
    {
        public string AlongX, AlongY, Lit;
        public readonly string[] Faces = new string[4];
        public bool Mirror;
        public float PivotX, PivotY;
        public int PxW, PxH;
        public string Young, Grown, Fruit;
        /// <summary>Yalnızca bu dönüşler geçerli (merdiven görseli tek yöne çıkar); boş = görsele göre.</summary>
        public readonly System.Collections.Generic.List<int> Rotations = new System.Collections.Generic.List<int>();
        /// <summary>
        /// Görsel eşlendi ama dosyası henüz gelmedi (yer tutucu çizilir). Yalnızca not: dosya geldiyse <see cref="Awaiting"/>
        /// false olur ve görsel kullanılır (kullanıcı kararı 21.11.2026, işçi teslimi main'i bozmasın). İşaret
        /// <c>python tools/art/art_link.py --apply</c> ile temizlenir.
        /// </summary>
        public bool Pending;

        /// <summary>Görsel dosyası var mı (oyun katmanı kurar: IsoArt.Has). Kurulmamışsa yalnızca Pending işaretine bakılır.</summary>
        public static System.Func<string, bool> FileExists { get; set; }

        /// <summary>Görsel gerçekten bekleniyor mu: işaretli ve dosyalarından biri yok.</summary>
        public bool Awaiting
        {
            get
            {
                if (!Pending) return false;
                if (FileExists == null) return true;
                foreach (var n in Names()) if (n != null && !FileExists(n)) return true;
                return false;
            }
        }

        /// <summary>Eşlenen bütün görsel adları (boşlar dahil değil).</summary>
        public System.Collections.Generic.IEnumerable<string> Names()
        {
            foreach (var n in new[] { AlongX, AlongY, Lit, Young, Grown, Fruit }) if (n != null) yield return n;
            foreach (var f in Faces) if (f != null) yield return f;
        }
        public bool HasFaces => Faces[0] != null || Faces[1] != null || Faces[2] != null || Faces[3] != null;
        public bool HasAny => HasFaces || AlongX != null || AlongY != null || Grown != null;
    }

    /// <summary>Barınak odası eşyalarının rolleri.</summary>
    public static class AnimalRoles
    {
        public const string Water = "water", Bedding = "bedding", Collector = "collector", Feeder = "feeder", Bell = "bell";
    }

    /// <summary>Eşya yükseltmesi (belge 45 §6): yatak şiltesi, sandığa demir kuşak, masaya cila, ocağa baca.</summary>
    public sealed class UpgradeDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Önce yapılmış olması gereken yükseltme (kademe zinciri); null = yok.</summary>
        public string Requires { get; set; }
        /// <summary>Yapılınca kaldırılan yükseltme (yün şilte saman şiltenin yerine geçer); null = yok.</summary>
        public string Replaces { get; set; }
        public int Comfort { get; set; }
        public int Beauty { get; set; }
        public int Storage { get; set; }
        /// <summary>İhtiyaç doldurma eki (0,1 = %10 daha çok doyurur).</summary>
        public float Restore { get; set; }
        /// <summary>Çökme şansı çarpanı (0,5 = yarısı); 0 = etkisiz.</summary>
        public float Collapse { get; set; }
        /// <summary>Ocağa baca: is çok azalır.</summary>
        public bool Chimney { get; set; }
    }

    /// <summary>Cam bahçe türü (belge 47).</summary>
    public sealed class VivDef
    {
        public bool IsPlant;
        public bool IsDecor;
        public float Beauty = 1f;
        public List<string> Needs = new List<string>();
        public List<string> Habitats = new List<string>();
        public bool FitsHabitat(string h) => Habitats.Count == 0 || Habitats.Contains(h);
        public bool IsCreature => !IsPlant && !IsDecor;
        public List<string> Eats = new List<string>();
        public List<string> Tags = new List<string>();
        public string Move = "crawl";
        public float LifeDays;
        public bool Breeds;
        public string Becomes;
        public float BecomeDays;
        public List<string> Avoid = new List<string>();
        public bool Solitary;
        public string Color;
        public bool Has(string tag) => Tags.Contains(tag);
    }

    public sealed class CatchDef
    {
        public string Item;
        public float Chance;
        public List<Time.Season> Seasons = new List<Time.Season>();
        public bool Night;
        public bool Rain;
    }

    public sealed class ItemDef : IContentDef
    {
        public const int MaxSize = 6;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<InteractionDef> Interactions { get; set; } = new List<InteractionDef>();

        // Bu tanımın herhangi bir ihtiyacı gideren etkileşimi var mı (otonomi adayı). Etkileşim listesi yükleme sırasında
        // büyüyebildiği için sonuç liste boyuyla birlikte saklanır.
        private int _needCheckedCount = -1;
        private bool _restoresAnyNeed;

        /// <summary>
        /// Herhangi bir ihtiyacı gideren etkileşimi olan eşya (yatak, sandalye, yiyecek ...). Otonomi yalnızca bunları tarar;
        /// dünyadaki eşyaların çoğu (ağaç, dekor, çit) aday değildir.
        /// </summary>
        public bool RestoresAnyNeed
        {
            get
            {
                if (_needCheckedCount == Interactions.Count) return _restoresAnyNeed;
                _restoresAnyNeed = false;
                foreach (var i in Interactions)
                    if ((i.RestoresNeed && i.Restore > 0f) || HasPositive(i.ExtraRestore)) { _restoresAnyNeed = true; break; }
                _needCheckedCount = Interactions.Count;
                return _restoresAnyNeed;
            }
        }

        // İhtiyaç başına otonominin seçebileceği etkileşimler (tanım düzeyindeki elemeler önceden yapılır).
        private InteractionDef[][] _autonomy;
        private int _autonomyCount = -1;
        private static readonly InteractionDef[] NoInteractions = new InteractionDef[0];
        private static readonly int NeedTypeCount = System.Enum.GetValues(typeof(Characters.NeedType)).Length;

        /// <summary>
        /// Otonominin bu ihtiyaç için seçebileceği etkileşimler, <see cref="Interactions"/> sırasıyla: ihtiyacı gideren ve iş
        /// olmayan (eşyayı kaldırmayan, hasat etmeyen, üretmeyen, yem koymayan, toplamayan, beceri istemeyen) etkileşimler.
        /// Duruma bağlı elemeler (yanıyor mu, kullanabilir mi, ulaşılır mı) çağıranda kalır.
        /// </summary>
        public InteractionDef[] AutonomyInteractions(Characters.NeedType need)
        {
            if (_autonomy == null || _autonomyCount != Interactions.Count)
            {
                var lists = new List<InteractionDef>[NeedTypeCount];
                foreach (var i in Interactions)
                {
                    if (i.RemovesItem || i.RequiresGrown || i.Produce.Count > 0 || i.IsCraft || i.AddFeed > 0 || i.Collect || i.MinSkillLevel > 0) continue;
                    for (int n = 0; n < NeedTypeCount; n++)
                        if (i.RestoreFor((Characters.NeedType)n) > 0f) (lists[n] ?? (lists[n] = new List<InteractionDef>())).Add(i);
                }
                var result = new InteractionDef[NeedTypeCount][];
                for (int n = 0; n < NeedTypeCount; n++) result[n] = lists[n] != null ? lists[n].ToArray() : NoInteractions;
                _autonomy = result;
                _autonomyCount = Interactions.Count;
            }
            int k = (int)need;
            return k >= 0 && k < _autonomy.Length ? _autonomy[k] : NoInteractions;
        }

        private static bool HasPositive(List<NeedRestoreDef> extra)
        {
            if (extra == null) return false;
            foreach (var e in extra) if (e.Amount > 0f) return true;
            return false;
        }

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

        /// <summary>
        /// "retired": artık satılmayan eşya (katalogda, dükkânda, oda şablonunda yok). Eski kayıtlardaki yerleşik
        /// örnekleri çalışmayı sürdürür ve satılabilir. İlk örnek: hazır ahır (kullanıcı kararı 19.11.2026).
        /// </summary>
        public bool IsRetired => HasTag(RetiredTag);
        public const string RetiredTag = "retired";
        /// <summary>Dükkânda görünmesi için gereken hesap seviyesi (T10). 0/1 = baştan açık.</summary>
        public int UnlockLevel { get; set; } = 1;
        /// <summary>
        /// Bu eşyayı üreten ilk tarifin becerisi (ContentMapper doldurur); kilit bu beceriye bağlanır (Project belgesi 45, 3.1).
        /// Tarifi olmayan eşyada null; kilit kategoriden gelir.
        /// </summary>
        public Characters.SkillType? CraftSkill { get; set; }
        /// <summary>İçerikte açıkça verilen kilit becerisi ("lockSkill"); tarif ve bitki kuralından önce gelir.</summary>
        public Characters.SkillType? LockSkill { get; set; }
        /// <summary>Ekin: yalnızca toprak (sürülmüş) zemine dikilir.</summary>
        public bool NeedsTilledSoil { get; set; }
        /// <summary>Kış gelince donar ve kaybolur (hasat edilmemiş ekin).</summary>
        public bool DiesInWinter { get; set; }
        /// <summary>Yalnızca ılık serada büyür ve meyve verir (kahve çalısı, belge 32).</summary>
        public bool WarmOnly { get; set; }
        /// <summary>Yalnızca Güney Yamaçları'nda büyür ve meyve verir (zeytin, belge 32-7); seraya konmaz.</summary>
        public bool SouthOnly { get; set; }
        /// <summary>Işık eşyası yalnızca yağı varken yanar (zeytinyağı kandili, belge 32-7).</summary>
        public bool NeedsOil { get; set; }
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
        /// <summary>Beslenme tipi (belge 57c): toprağı ne kadar yorduğu ve gübreye nasıl karşılık verdiği.</summary>
        public FeedType Feed { get; set; } = FeedType.Medium;
        /// <summary>Çiğ yenen ekin (marul, turp, mutfak otu): pislikli ham kompostla gübrelenirse yiyeni hasta edebilir.</summary>
        public bool EatenRaw { get; set; }
        /// <summary>Bu gübrelerle beslenince kalitesi artar (kül: lahana ailesi; kemik unu: kök ve soğan).</summary>
        public Fertilizer LikesFertilizer { get; set; }
        /// <summary>Bu gübreler ürünü azaltır (kül: ahududu).</summary>
        public Fertilizer DislikesFertilizer { get; set; }
        /// <summary>Tahıl mı (fırtınada döker, aşırı gübreliyse yatar)?</summary>
        public bool IsCereal => CropFamily == "tahil";
        /// <summary>Hasat döngüsü (59 §1): tek hasat, devşirme, kes-yine-gelir ya da kalıcı yatak.</summary>
        public CropCycle Cycle { get; set; }
        /// <summary>Devşirme ve kes-yine-gelir ekinde en çok kaç kez toplanır; sonra bitki sararır. Kalıcı yatakta sınır yok.</summary>
        public int MaxHarvests { get; set; }
        /// <summary>Toplamadan sonra yeniden olgunlaşma süresi (gün).</summary>
        public float RegrowDays { get; set; }
        /// <summary>Sırık ister (bezelye, hıyar, şerbetçiotu): sırıksız ürün yarıya iner.</summary>
        public bool NeedsStake { get; set; }
        /// <summary>Dev ürün (59 §2): aynı ekinden 2×2 olgun ve sulu karo küçük şansla bu eşyaya dönüşür; boş = dev olmaz.</summary>
        public string GiantAs { get; set; }
        /// <summary>Hasatta bitki yerinde kalır mı (tek hasat dışındaki döngüler).</summary>
        public bool Regrows => IsCrop && Cycle != CropCycle.Single;
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
        /// <summary>Kalite sınıfı (53 §4.2): içerikten ("quality") ya da etiket ve tariften (<see cref="ContentMapper"/>).</summary>
        public ItemQualityClass QualityClass { get; set; }
        /// <summary>İçerikte açıkça yazılan sınıf adı (none, goods, crafted); null ise türetilir.</summary>
        public string QualityClassName { get; set; }
        public bool HasQuality => QualityClass != ItemQualityClass.None;
        /// <summary>Mahzende olgunlaşma: bu kadar günde bir kalite basamağı (peynir, pastırma, şarap, sirke, bal şarabı). 0 = olgunlaşmaz.</summary>
        public float AgeStepDays { get; set; }
        /// <summary>Kiler eşyası: yiyecek bozulmasını bu çarpanla yavaşlatır (0 = kiler değil).</summary>
        public float Preserves { get; set; }
        public List<string> Variants { get; set; } = new List<string>();
        /// <summary>Mobilya kaplama grubu (wood, cloth, metal, clay); boşsa renk/malzeme seçeneği yok.</summary>
        public string FinishGroup { get; set; }
        /// <summary>İkinci kaplama grubu (merdivende korkuluk, sandalyede minder, sandıkta kulp); boşsa yok (belge 45 §10).</summary>
        public string AccentGroup { get; set; }
        /// <summary>İkinci kaplamanın görünen adı ("Korkuluk").</summary>
        public string AccentLabel { get; set; }
        public bool IsRug => Placement == ItemPlacement.Rug;
        public bool IsCeiling => Placement == ItemPlacement.Ceiling;

        // ---- ev dekorasyonu (Project belgesi 27, D1) ----
        /// <summary>Dekorun boyut sınıfı (yüzeye konan eşya).</summary>
        public DecorSize DecorSize { get; set; } = DecorSize.Small;
        /// <summary>Dekorun yükü (1-12): rafın taşıması aşılırsa raf çökebilir.</summary>
        /// <summary>Taşıma ağırlığı (kg, belge 59 §6). 0 = kategori tablosundan (<see cref="Characters.ItemMass"/>). Dekor yükü <see cref="Weight"/> ayrı.</summary>
        public float Mass { get; set; }
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

        // ---- mutfak aletleri (Project belgeleri 63, 63a) ----
        /// <summary>Mutfak rolü (bkz. <see cref="Content.Equip"/>): "vessel:boil", "tool:knife", "tableware", "basket"; yoksa null.</summary>
        public string Equip { get; set; }
        /// <summary>Kabın kademesi (1 Köylü, 2 Zanaatkâr, 3 Tüccar): ekipman puanı ve kalite tavanı (63a §5.2). Alette 0.</summary>
        public int EquipTier { get; set; }
        /// <summary>Üstünde yemek pişirilen ateş (mutfak ocağı): pişirme yakıt harcar, sönükse tutuşturulur.</summary>
        public bool CookFire { get; set; }
        /// <summary>Bulaşık yıkama yeri türü (bkz. <see cref="WashPlaces"/>); yoksa null.</summary>
        public string WashPlace { get; set; }

        public bool IsVessel => Equip != null && Equip.StartsWith(Content.Equip.VesselPrefix, StringComparison.Ordinal);
        /// <summary>Kabın pişirme yöntemi (boil, fry, stew, grill, bake); kap değilse null.</summary>
        public string VesselFor => IsVessel ? Equip.Substring(Content.Equip.VesselPrefix.Length) : null;
        public bool IsKitchenTool => Equip != null && Equip.StartsWith(Content.Equip.ToolPrefix, StringComparison.Ordinal);
        /// <summary>El aletinin adı (knife, spoon...); alet değilse null.</summary>
        public string ToolKind => IsKitchenTool ? Equip.Substring(Content.Equip.ToolPrefix.Length) : null;
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

        // ---- ev bakımı ve otomasyon (belge 45) ----
        /// <summary>Mum: akşam saatlerinde yanar, bu kadar günde biter ("Mum değiştir"). 0 = bitmez.</summary>
        public float BurnDays { get; set; }
        public bool Burns => BurnDays > 0f;
        /// <summary>Mum değiştirirken harcanan mumlar (sırayla). Boşsa kendisi, sonra item.candle.</summary>
        public List<string> BurnRefill { get; set; } = new List<string>();
        /// <summary>Işık gücü (mum 1, fener 2, avize 4). 0 ise ışık eşyasında 1.</summary>
        public int LightPower { get; set; }
        public int EffectiveLightPower => !HasTag("light") ? 0 : LightPower > 0 ? LightPower : 1;
        /// <summary>Canlı saksı: sulandıkça büyür, olgunlaşınca bu otu verir.</summary>
        public string PotHerb { get; set; }
        public float PotGrowDays { get; set; } = 3f;
        public bool IsLivePot => PotHerb != null || LivePlant;
        /// <summary>Cam bahçe türü (bitki ya da canlı); null ise değil (belge 47).</summary>
        public VivDef Viv { get; set; }
        /// <summary>Bu toplama yerinde cam kavanozla yakalananlar.</summary>
        public List<CatchDef> Catches { get; set; } = new List<CatchDef>();
        /// <summary>Cam bahçe kabı: en çok canlı ve bitki (0 = cam bahçe değil).</summary>
        public int VivCreatures { get; set; }
        public int VivPlants { get; set; }
        public int VivDecor { get; set; }
        /// <summary>Toplama yerinde "Kavanoza doldur" ile kesin gelen zemin malzemesi (orman toprağı, kum).</summary>
        public List<string> Gathers { get; } = new List<string>();
        public bool IsVivarium => VivCreatures > 0 || VivPlants > 0;
        /// <summary>Canlı süs bitkisi (ot vermez; sulanır, büyür, mevsiminde çiçek açar).</summary>
        public bool LivePlant { get; set; }
        /// <summary>Çiçek açtığı mevsimler: olgun ve suluyken bu mevsimlerde güzelliğe ek. Boş = çiçeksiz (yaprak süsü).</summary>
        public List<Time.Season> BloomSeasons { get; set; } = new List<Time.Season>();
        /// <summary>Gölge sever (eğrelti, yosun, sarmaşık): pencereden uzakta tam hızla büyür, güneşte yavaş.</summary>
        public bool ShadePlant { get; set; }
        /// <summary>Susama çarpanı: su teknesi 0,4 (yavaş kurur), eğrelti 1,5 (çabuk kurur). 0 = 1.</summary>
        public float Thirst { get; set; } = 1f;
        /// <summary>Yükseltmeler (her biri "upgrade:&lt;id&gt;" etkileşimiyle yapılır).</summary>
        public List<UpgradeDef> Upgrades { get; set; } = new List<UpgradeDef>();
        public UpgradeDef FindUpgrade(string id) { foreach (var u in Upgrades) if (u.Id == id) return u; return null; }
        /// <summary>Gömme eşya (duvar nişi): yalnızca kalın duvara (kerpiç, taş, kesme taş) oyulur.</summary>
        public bool IsNiche => HasTag(Decor.NicheTag);

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
        /// <summary>Kement tuzağı: arsaya ya da yaban bölgesine kurulur; her sabah av düşebilir, av toplanana dek tuzakta durur.</summary>
        public bool IsSnare { get; set; }
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
        /// <summary>Toplama noktasının belirdiği bölgeler (Regions kimliği); boşsa her yerde.</summary>
        public List<string> ForageRegions { get; } = new List<string>();
        /// <summary>Balık tutma mini oyunu: zorluk (0-100) ve hareket biçimi (belge 50).</summary>
        public int FishDifficulty { get; set; } = 30;
        public string FishBehavior { get; set; } = "smooth";
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

        // ---- yerleşim sistemi (belge 44, Aşama B) ----
        /// <summary>Görseller ve pivot (ayak izinden ayrı); null = görsel yok, yer tutucu çizilir.</summary>
        public ItemArtDef Art { get; set; }
        /// <summary>Ön yüzü olan eşyanın kullanımı: sit (oturma), open (açılır: sandık, dolap), work (çalışma: ocak, tezgâh),
        /// stand (önünde durulur: leğen). null = ön yüz yok (her yönden kullanılır, yön kavramı yok).</summary>
        public string FrontUse { get; set; }
        public bool HasFront => FrontUse != null;
        public bool IsSeat => FrontUse == Placements.Sit;
        /// <summary>Ön yüzde boş kalacak derinlik (karo).</summary>
        public int FrontDepth { get; set; } = 1;
        /// <summary>İç mekân, dış mekân ya da ikisi (bilinçli istisna).</summary>
        public ItemSite Site { get; set; } = ItemSite.Any;
        /// <summary>Ait olduğu oda türleri (Placements.Bedroom ...). Boş ve Anywhere değilse oda türü denetlenmez (bitki, kaynak).</summary>
        public List<string> Belongs { get; set; } = new List<string>();
        /// <summary>Meslek etiketleri: dükkânda ancak etiketi tutan eşya durur (demirci, terzi ...).</summary>
        public List<string> Trades { get; set; } = new List<string>();
        /// <summary>Bilinçli "her yer": oda türü denetiminden muaf.</summary>
        public bool Anywhere { get; set; }
        /// <summary>İlişki kuralı: faceTable, bedside, nearHearth.</summary>
        public string Relation { get; set; }
        public bool BelongsTo(string roomType) => Anywhere || Belongs.Contains(roomType);
        public bool HasTrade(string trade) => trade != null && Trades.Contains(trade);

        public SurfaceSlotDef FindSlot(string slotId)
        {
            foreach (var s in SurfaceSlots)
                if (s.Id == slotId) return s;
            return null;
        }
    }
}
