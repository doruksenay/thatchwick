using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;

namespace Thatchwick.Core.Life.Requests
{
    /// <summary>Rica şablonu: köylünün mesleğine göre (rol "*" herkes), mevsimli olabilir.</summary>
    public sealed class FavorTemplate
    {
        public string Id, Role, Item, Title, Text;
        public int Count;
        public Season[] Seasons;
        public ObjectiveKind Kind = ObjectiveKind.Deliver;
        public string Region;
        public FavorTemplate(string id, string role, string item, int count, string title, string text, params Season[] seasons)
        { Id = id; Role = role; Item = item; Count = count; Title = title; Text = text; Seasons = seasons; }
    }

    /// <summary>Mevsim Defteri işi (belge 46 §6.2).</summary>
    public sealed class SeasonalTemplate
    {
        public string Id, Title, Text;
        public Season Season;
        public int From, To;   // mevsim içindeki gün (1 tabanlı)
        public Objective Objective;
    }

    public sealed class CallTemplate
    {
        public string Id, Role, Title, Text, IgnoreText;
        public Season[] Seasons;
        public bool Hard;
        public readonly List<CallPath> Paths = new List<CallPath>();
    }

    public sealed class StoryStep
    {
        public string Title, Text;
        public Objective Objective;
        public Reward Reward;
    }

    public sealed class BequestTemplate
    {
        public string Id, Role, Title, Text;
        public Objective Objective;
    }

    public sealed class VowTemplate
    {
        public string Id, Title, Text;
        public readonly List<VowStage> Stages = new List<VowStage>();
        public int FinalSeals;
        public int FinalReputation;
    }

    /// <summary>
    /// İstek şablonları (belge 46 §15.4, 1.0 asgari): ricalar, mevsim işleri, çağrılar, hikâyeler, vasiyetler ve adaklar veridir;
    /// siparişler tariflerden üretilir (RequestService.MakeOrder). Metinlerde {giver} veren köylünün adıyla değişir.
    /// Eşyası içerikte olmayan şablon kullanılmaz (test bunu denetler).
    /// </summary>
    public static class RequestCatalog
    {
        private static readonly Season Sp = Season.Spring, Su = Season.Summer, Au = Season.Autumn, Wi = Season.Winter;

        // ------------------------------------------------------------------ ricalar
        public static readonly FavorTemplate[] Favors =
        {
            new FavorTemplate("rica.smith.charcoal", "blacksmith", "item.charcoal", 6, "Ocağa kömür", "{giver}: \"Körük sönmek üzere, birkaç torba kömür getirir misin?\""),
            new FavorTemplate("rica.smith.firewood", "blacksmith", "item.firewood", 8, "Ocak odunu", "{giver}: \"Kızgın demir bekler mi? Odun lazım.\""),
            new FavorTemplate("rica.smith.ore", "blacksmith", "item.iron_ore", 3, "Biraz cevher", "{giver}: \"Ocaktan üç cevher getiren olursa nal döverim.\""),
            new FavorTemplate("rica.miller.barley", "miller", "item.barley", 5, "Değirmene arpa", "{giver}: \"Taşlar boşa dönüyor; arpa getirene ekmek benden.\"", Su, Au),
            new FavorTemplate("rica.miller.wheat", "miller", "item.wheat", 5, "Buğday", "{giver}: \"Şenlik ekmeği için buğdayım az.\"", Su, Au),
            new FavorTemplate("rica.miller.firewood", "miller", "item.firewood", 6, "Fırın odunu", "{giver}: \"Fırın soğudu, sabaha ekmek yetişmez.\""),
            new FavorTemplate("rica.herb.chamomile", "herbalist", "item.chamomile", 3, "Papatya", "{giver}: \"Çocukların karnı ağrıyor; taze papatya lazım.\"", Sp, Su),
            new FavorTemplate("rica.herb.nettle", "herbalist", "item.nettle", 4, "Isırgan", "{giver}: \"Isırgan topla ama eldivenle, olur mu?\"", Sp, Su),
            new FavorTemplate("rica.herb.herb", "herbalist", "item.herb", 4, "Yabani ot", "{giver}: \"Merhem için birkaç demet ot getir.\"", Sp, Su),
            new FavorTemplate("rica.herb.rosehip", "herbalist", "item.rosehip", 3, "Kuşburnu", "{giver}: \"Kışa şurup kaynatacağım; kuşburnu bulursan getir.\"", Au),
            new FavorTemplate("rica.shepherd.lamb", "shepherd", null, 1, "Kaybolan kuzu", "{giver}: \"Bir kuzum ormana kaçtı, görürsen getir!\"", Sp, Su) { Kind = ObjectiveKind.Visit, Region = "forest" },
            new FavorTemplate("rica.shepherd.salt", "shepherd", "item.salt", 2, "Yalama tuzu", "{giver}: \"Koyunlar tuz istiyor; iki parça yeter.\""),
            new FavorTemplate("rica.trader.egg", "trader", "item.egg", 6, "Tezgâha yumurta", "{giver}: \"Pazar günü yumurta soran çok, bende kalmadı.\""),
            new FavorTemplate("rica.trader.honey", "trader", "item.honey", 2, "Bal", "{giver}: \"Kervancı bal sordu; iki kavanoz bulursan iyi para var.\"", Su, Au),
            new FavorTemplate("rica.trader.cheese", "trader", "item.cheese", 2, "Peynir", "{giver}: \"Peynirin iyisi sende derler, iki teker getir.\""),
            new FavorTemplate("rica.potter.clay", "potter", "item.clay", 6, "Irmaktan kil", "{giver}: \"Çarkım boş dönüyor, kil getirir misin?\""),
            new FavorTemplate("rica.potter.sand", "potter", "item.sand", 4, "Kum", "{giver}: \"Sır için ince kum lazım.\""),
            new FavorTemplate("rica.carpenter.log", "carpenter", "item.log", 4, "Meşe kütüğü", "{giver}: \"Dört sağlam kütük getir, sana bir tabure borçluyum.\""),
            new FavorTemplate("rica.carpenter.plank", "carpenter", "item.plank", 6, "Tahta", "{giver}: \"Siparişler birikti; biçilmiş tahta lazım.\""),
            new FavorTemplate("rica.inn.bread", "innkeeper", "item.bread", 4, "Han için ekmek", "{giver}: \"Yolcular aç; ekmek yetmedi.\""),
            new FavorTemplate("rica.inn.egg", "innkeeper", "item.egg", 6, "Sahanda yumurta", "{giver}: \"Sabah sofrası için yumurta.\""),
            new FavorTemplate("rica.inn.carp", "innkeeper", "item.fish_carp", 3, "Akşama sazan", "{giver}: \"Bu akşam balık pişireceğim, sazan getir.\""),
            new FavorTemplate("rica.scribe.gall", "scribe", "item.oak_gall", 3, "Mazı", "{giver}: \"Mürekkep bitti; meşe mazısı toplar mısın?\"", Au),
            new FavorTemplate("rica.scribe.wax", "scribe", "item.beeswax", 2, "Mühür mumu", "{giver}: \"Belgelere mühür basacağım; balmumu lazım.\""),
            new FavorTemplate("rica.tailor.wool", "tailor", "item.wool", 3, "Yün", "{giver}: \"Kış gelmeden yün eğirmeliyim.\""),
            new FavorTemplate("rica.tailor.flax", "tailor", "item.flax", 4, "Keten", "{giver}: \"Keten getiren gömleğini benden alır.\"", Su, Au),
            new FavorTemplate("rica.saddler.hide", "saddler", "item.hide", 3, "Ham deri", "{giver}: \"Yeni eyer için deri lazım.\""),
            new FavorTemplate("rica.saddler.tallow", "saddler", "item.tallow", 2, "Don yağı", "{giver}: \"Deriyi yağlayacağım, don yağı getir.\""),
            new FavorTemplate("rica.fisher.bread", "fisher", "item.bread", 3, "Kayıkta azık", "{giver}: \"Sabah erken ağ atacağım, azığım yok.\""),
            new FavorTemplate("rica.fisher.firewood", "fisher", "item.firewood", 4, "Tütsü odunu", "{giver}: \"Balığı tütsüleyeceğim, kuru odun lazım.\""),
            new FavorTemplate("rica.mason.limestone", "mason", "item.limestone", 4, "Kireçtaşı", "{giver}: \"Kireç ocağı boş, kireçtaşı getir.\""),
            new FavorTemplate("rica.mason.stone", "mason", "item.stone", 8, "Taş", "{giver}: \"Duvar yarım kaldı, taş taşıyacak el lazım.\""),
            new FavorTemplate("rica.any.elder_wood", "*", "item.firewood", 6, "Yaşlı komşuya kış odunu", "{giver}: \"Evde ocak sönmesin, biraz odun bırakır mısın?\"", Au, Wi),
            new FavorTemplate("rica.any.soup", "*", "item.stew_plain", 1, "Hastaya çorba", "{giver}: \"Evde hasta var, sıcak bir şey getirir misin?\"", Wi),
            new FavorTemplate("rica.any.mushroom", "*", "item.mushroom", 4, "Mantar", "{giver}: \"Mantar sevenler sofraya mantar ister.\"", Au),
            new FavorTemplate("rica.any.berries", "*", "item.berries", 4, "Yemiş", "{giver}: \"Çocuklar yemiş istiyor.\"", Su),
        };

        // ------------------------------------------------------------------ mevsim defteri (4 × 7)
        public static readonly SeasonalTemplate[] Seasonal =
        {
            S("mev.sp.plough", Sp, 1, 3, "Çift sürme", "Köyün tarlaları ve kendi tarlan sürülsün.", Act(SkillType.Farming, 8)),
            S("mev.sp.lambing", Sp, 1, 5, "Kuzulama", "Murat'ın ağılında doğuma yardım.", Act(SkillType.AnimalHusbandry, 4)),
            S("mev.sp.ditch", Sp, 2, 4, "Ark ve hendek temizliği", "Kışın dolan arklar açılsın.", Del("item.stone", 6)),
            S("mev.sp.seeds", Sp, 4, 4, "Tohum takası", "Tohum Şenliği'nde takas için tohum getir.", Del("item.seed_leek", 4)),
            S("mev.sp.thatch", Sp, 5, 9, "Kış hasarı onarımı", "Yaşlıların çatıları önce onarılsın.", Del("item.plank", 6)),
            S("mev.sp.swarm", Sp, 1, 10, "Arı oğulu", "Oğul veren kovanlar sepete alınsın.", Act(SkillType.AnimalHusbandry, 2)),
            S("mev.sp.shearing", Sp, 8, 10, "Koyun yıkama ve kırkım", "Derede yıkanan koyunlar kırkılsın.", Del("item.wool", 3)),
            S("mev.su.hay", Su, 1, 4, "Ot biçme", "Yağmur gelmeden ot biçilip kurutulsun.", Act(SkillType.Farming, 6)),
            S("mev.su.uplands", Su, 1, 3, "Yayla göçü", "Sürü yaylaya çıksın.", Act(SkillType.AnimalHusbandry, 4)),
            S("mev.su.fair", Su, 5, 5, "Panayırda tezgâh", "Panayıra satılık ekmek götür.", Del("item.bread", 4)),
            S("mev.su.harvest", Su, 6, 9, "Tahıl hasadı ve harman", "Başaklar biçilip harmana taşınsın.", Del("item.wheat", 8)),
            S("mev.su.flax", Su, 6, 8, "Keten çürütme", "Keten havuza yatırılsın.", Del("item.flax", 6)),
            S("mev.su.well", Su, 7, 10, "Köy kuyusunun temizliği", "Kuyunun ağzı taşla örülsün.", Del("item.stone", 6)),
            S("mev.su.dry", Su, 8, 10, "Meyve kurutma", "Kışa kuru meyve dizilsin.", Del("item.dried_apple", 4)),
            S("mev.au.vintage", Au, 1, 3, "Bağbozumu", "Üzümler toplanıp teknede ezilsin.", Del("item.grape", 8)),
            S("mev.au.nuts", Au, 1, 5, "Kestane ve mantar", "Orman ürünleri toplansın.", Del("item.chestnut", 6)),
            S("mev.au.sowing", Au, 2, 5, "Kışlık ekim", "Kışlık buğday ve çavdar ekilsin.", Act(SkillType.Farming, 8)),
            S("mev.au.pekmez", Au, 4, 6, "Pekmez kazanı", "Şenlik sofrası için pekmez kaynatılsın.", Del("item.pekmez", 2)),
            S("mev.au.preserve", Au, 6, 9, "Kışlık hazırlık", "Turşu ve tuzlama küplere girsin.", Del("item.pickled_cabbage", 3)),
            S("mev.au.elderwood", Au, 7, 10, "Yaşlılara kış odunu", "Yalnız yaşayan yaşlıların odunluğu dolsun.", Del("item.firewood", 10)),
            S("mev.au.acorns", Au, 8, 10, "Palamuda salma", "Hayvanlar palamut için ormana salınsın.", Act(SkillType.AnimalHusbandry, 4)),
            S("mev.wi.watch", Wi, 1, 10, "Kurt nöbeti", "Ağıllar gece nöbetle korunsun.", Act(SkillType.AnimalHusbandry, 3)),
            S("mev.wi.ice", Wi, 2, 6, "Buz kesme", "Göl donunca buz kesilip buz çukuruna taşınsın.", Act(SkillType.Mining, 3)),
            S("mev.wi.tools", Wi, 1, 10, "Alet onarımı", "Orhan'ın en yoğun mevsimi: ocağa kömür.", Del("item.charcoal", 6)),
            S("mev.wi.night", Wi, 4, 4, "Kış Gecesi mumları", "Kış Gecesi için mum dökülsün.", Del("item.tallow_candle", 4)),
            S("mev.wi.weaving", Wi, 3, 8, "Çeyiz ve dokuma geceleri", "Işıklı odalarda iplik eğrilsin.", Del("item.yarn", 4)),
            S("mev.wi.snow", Wi, 1, 10, "Damdan kar atma", "Yaşlıların damı karla çökmesin.", Del("item.firewood", 6)),
            S("mev.wi.prune", Wi, 8, 10, "Meyve ağaçlarını budama", "İlkbahar verimi için budama.", Act(SkillType.Horticulture, 3)),
        };

        /// <summary>Mevsim takımı ödülü (belge 46 §6.3): yeni defter parçası önce, sonra mevsimin süsleri.</summary>
        public static string[] SeasonSet(Season s)
        {
            switch (s)
            {
                case Season.Spring: return new[] { "item.defter_spring_ribbon", "item.spring_garland", "item.spring_branch" };
                case Season.Summer: return new[] { "item.defter_summer_sheaf", "item.spring_garland" };
                case Season.Autumn: return new[] { "item.defter_autumn_basket", "item.harvest_heap", "item.door_wreath" };
                default: return new[] { "item.defter_winter_lantern", "item.winter_greenery", "item.door_wreath" };
            }
        }

        // ------------------------------------------------------------------ çağrılar
        public static readonly CallTemplate[] Calls =
        {
            Call("cagri.wolves", "shepherd", "Ağıla kurt dadandı", "{giver}: \"Üç gecedir kurtlar ağılın etrafında dolanıyor!\"", "Kurtlar bir kuzuyu kaptı; Murat günlerce konuşmadı.", true, new[] { Au, Wi },
                P("Pusu kur", SkillType.Hunting, 7, hours: 4, outcome: "Pusuda bir kurt vuruldu, sürü kaçtı."),
                P("Çoban köpeği ver", SkillType.AnimalHusbandry, 5, hours: 3, outcome: "Köpek ağılın başında; kurtlar yaklaşamıyor."),
                P("Gece nöbetine köylüleri topla", SkillType.Charm, 4, hours: 3, outcome: "Köylüler ateş yakıp sabaha kadar nöbet tuttu."),
                P("Bekçi tut", null, 0, akce: 12, outcome: "Ücretli bekçi bir hafta ağılda yattı.")),
            Call("cagri.lost_child", "*", "Çocuk kayboldu", "{giver}: \"Torunum ormana gitti, hâlâ dönmedi!\"", "Çocuğu akşam oduncular buldu; köy seni sormadı.", true, null,
                P("İz oku", SkillType.Hunting, 2, hours: 3, outcome: "Çamurdaki küçük izleri takip edip çocuğu buldun."),
                P("Ormanı tara", SkillType.Foraging, 7, hours: 3, outcome: "Böğürtlenliğin dibinde uyuyakalmıştı."),
                P("Atla ara", SkillType.Riding, 3, hours: 2, outcome: "Atla ormanın kenarını dolaşıp çocuğu buldun.")),
            Call("cagri.storm_roof", "*", "Fırtına damı uçurdu", "{giver}: \"Gece fırtınada çatımız uçtu, çocuklar ıslak!\"", "Komşular kendi aralarında damı yamadı.", false, null,
                P("Damı onar", SkillType.Carpentry, 3, item: "item.plank", count: 4, hours: 4, outcome: "Tahta tahta dam yeniden kapandı."),
                P("Taşla ör", SkillType.Masonry, 2, item: "item.stone", count: 4, hours: 4, outcome: "Yıkılan duvar taşla örüldü."),
                P("İmece düzenle", SkillType.Stewardship, 7, hours: 2, outcome: "İmeceyle dam bir öğleden sonra kapandı."),
                P("Marangoza para ver", null, 0, akce: 10, outcome: "Marangoz Hasan damı onardı.")),
            Call("cagri.birth", "*", "Doğum var, ebe yetişemiyor", "{giver}: \"Gelinimin sancısı başladı, ebe hasta!\"", "Doğum zor geçti ama anne ve bebek kurtuldu.", true, null,
                P("Doğuma yardım et", SkillType.Medicine, 5, hours: 3, outcome: "Sağlıklı bir bebek doğdu; aile sana dua ediyor."),
                P("Papatya çayı götür", SkillType.Herbalism, 2, item: "item.tea_chamomile", count: 2, hours: 1, outcome: "Sıcak çay sancıyı yatıştırdı."),
                P("Gezgin hekime haber sal", null, 0, akce: 15, outcome: "Hekim geç geldi ama işini bildi.")),
            Call("cagri.animal_sick", "*", "Köyde hayvan hastalığı", "{giver}: \"Hayvanlar yem yemiyor, salgından korkuyorum.\"", "Birkaç hayvan öldü; köy tedirgin.", false, null,
                P("Salgını önle", SkillType.Medicine, 8, hours: 3, outcome: "Hasta hayvanlar ayrıldı, salgın yayılmadı."),
                P("Hayvan merhemi götür", SkillType.Herbalism, 2, item: "item.animal_salve", count: 1, hours: 1, outcome: "Merhem ahırdaki yaraları kapattı."),
                P("Hastaları ayır", SkillType.AnimalHusbandry, 6, hours: 3, outcome: "Hasta hayvanlar ayrı ağıla alındı.")),
            Call("cagri.feud", "*", "İki komşu hane küs", "{giver}: \"Komşumla aramız bozuk, köyde herkes bir tarafı tutuyor.\"", "Küslük bir mevsim daha sürdü.", false, null,
                P("Arabuluculuk", SkillType.Charm, 6, hours: 2, outcome: "İki hane aynı sofrada barıştı."),
                P("Halay çektir", SkillType.Music, 9, hours: 2, outcome: "Davul çalınca küslük unutuldu."),
                P("Ziyafet sofrası kur", SkillType.Cooking, 5, item: "item.stew_good", count: 2, hours: 2, outcome: "Güveç kokusu iki haneyi bir araya getirdi.")),
            Call("cagri.well", "*", "Köy kuyusu çöktü", "{giver}: \"Kuyunun ağzı göçtü, su çekemiyoruz!\"", "Köy bir hafta dereden su taşıdı.", false, null,
                P("Kuyuyu ör", SkillType.Masonry, 4, item: "item.stone", count: 6, hours: 4, outcome: "Kuyunun ağzı taşla yeniden örüldü."),
                P("Makarayı kur", SkillType.Engineering, 3, item: "item.plank", count: 3, hours: 3, outcome: "Yeni makarayla su yine çekiliyor."),
                P("Taşçıya para ver", null, 0, akce: 12, outcome: "Taşçı Mustafa kuyuyu ördü.")),
            Call("cagri.millstone", "miller", "Değirmen taşı çatladı", "{giver}: \"Taş çatladı, un öğütemiyorum!\"", "Değirmen üç gün durdu; ekmek pahalandı.", true, null,
                P("Yeni taş yont", SkillType.Masonry, 5, item: "item.stone", count: 4, hours: 4, outcome: "Yeni değirmen taşı yerine oturdu."),
                P("Ocaktan taş çıkar", SkillType.Mining, 5, hours: 4, outcome: "Ocaktan sağlam bir blok çıkarıldı."),
                P("Usta getirt", null, 0, akce: 15, outcome: "Gezgin bir taşçı taşı yontu.")),
            Call("cagri.elder_sick", "*", "Yaşlı komşu hasta", "{giver}: \"Babam yataktan kalkamıyor, ateşi var.\"", "Yaşlı adam bir hafta yatakta kaldı.", false, new[] { Au, Wi },
                P("Muayene et", SkillType.Medicine, 3, hours: 2, outcome: "Ateş düştü; yaşlı adam sana teşekkür etti."),
                P("Papatya çayı götür", SkillType.Herbalism, 1, item: "item.tea_chamomile", count: 1, hours: 1, outcome: "Çay iyi geldi."),
                P("Sıcak çorba götür", SkillType.Cooking, 2, item: "item.stew_plain", count: 1, hours: 1, outcome: "Çorbayı içip uyudu.")),
            Call("cagri.bear_hives", "*", "Ayı kovanlara dadandı", "{giver}: \"Ayı gece kovanları devirdi!\"", "Ayı iki kovanı daha devirdi.", false, new[] { Su, Au },
                P("Ayıyı korkut", SkillType.Hunting, 5, hours: 3, outcome: "Ateş ve gürültüyle ayı ormana döndü."),
                P("Kovanları taşı", SkillType.AnimalHusbandry, 4, hours: 3, outcome: "Kovanlar köyün içine taşındı."),
                P("Bekçi tut", null, 0, akce: 8, outcome: "Bekçi gece kovanların başında bekledi.")),
        };

        // ------------------------------------------------------------------ hikâyeler (adlı köylüler, 3 adım)
        public static readonly Dictionary<string, StoryStep[]> NamedStories = new Dictionary<string, StoryStep[]>
        {
            { "villager.orhan", Steps(
                St("Örsün sesi", "Orhan oğluna bir usta arıyor; önce senin elini görmek istiyor.", Del("item.charcoal", 4), Aff(20)),
                St("Çırak arayışı", "\"Hanende Demircilik bilen biri var mı?\"", Skl(SkillType.Smithing, 3), Aff(25)),
                St("Babadan oğula", "Orhan eski çekicini sana emanet ediyor.", Talk(), Keep("item.elder_tin_cup", "Orhan'ın çekiç sapı"))) },
            { "villager.hatice", Steps(
                St("Ekşi maya", "Hatice büyükannesinin mayasını yaşatıyor; un bitti.", Del("item.flour", 3), Aff(20)),
                St("Mayanın sırrı", "\"Mayayı beslemek sabır ister.\"", Act(SkillType.Baking, 3), Aff(25)),
                St("Maya emaneti", "Hatice mayadan bir parça sana veriyor.", Talk(), Keep("item.bread_bowl", "Hatice'nin maya kabı"))) },
            { "villager.zeynep", Steps(
                St("Ebe olmak", "Zeynep köyün ilk ebesi olmak istiyor; ot lazım.", Del("item.chamomile", 3), Aff(20)),
                St("Şifa merhemi", "\"Bir hekim çırağı bulsam...\"", Skl(SkillType.Medicine, 3), Aff(25)),
                St("Ebenin kesesi", "Zeynep ilk ebelik kesesini sana gösteriyor.", Talk(), Keep("item.baby_first_shoes", "Zeynep'in ebelik kesesi"))) },
            { "villager.murat", Steps(
                St("Yayla anısı", "Murat yaylada kaybettiği köpeğini anlatıyor.", Vis("hill"), Aff(20)),
                St("Kuzu bakımı", "\"Bir kuzu senin yanında büyüsün.\"", Act(SkillType.AnimalHusbandry, 4), Aff(25)),
                St("Çoban kavalı", "Murat'ın eski kavalı artık senin.", Talk(), Keep("item.harvest_ribbon", "Murat'ın kaval kurdelesi"))) },
            { "villager.kerem", Steps(
                St("Uzak diyar", "Kerem'in kardeşi kervanla uzaklara gitti; mektup bekliyor.", Del("item.beeswax", 1), Aff(20)),
                St("Mektup", "\"Okuryazar biri mektup yazsa...\"", Skl(SkillType.Literacy, 5), Aff(25)),
                St("Kardeşin cevabı", "Kardeşinden cevap geldi; Kerem sevinçten ağlıyor.", Talk(), Keep("item.festival_cup", "Kerem'in kervan fincanı"))) },
            { "villager.ayse", Steps(
                St("İlk ayna", "Ayşe köyün ilk aynasını yapmak istiyor; kum lazım.", Del("item.sand", 4), Aff(20)),
                St("Cam ocağı", "\"Çömlekçilikte usta biri yardım etse...\"", Skl(SkillType.Pottery, 6), Aff(25)),
                St("Aynanın ilk yansıması", "Ayşe ilk aynada ikinizi gösteriyor.", Talk(), Keep("item.home_ribbon", "Ayşe'nin sır kurdelesi"))) },
            { "villager.hasan", Steps(
                St("Ceviz korusu", "Hasan ormanın derinindeki ceviz korusunu arıyor.", Vis("forest"), Aff(20)),
                St("Kereste", "\"İyi kereste için yaşlı ağaç gerek.\"", Del("item.log", 4), Aff(25)),
                St("Usta işi", "Hasan cevizden oyduğu bir kaşığı sana veriyor.", Talk(), Keep("item.elder_tin_cup", "Hasan'ın oyma kaşığı"))) },
            { "villager.serife", Steps(
                St("Gizemli yolcu", "Hanın kiralık odasına bir yolcu geldi, kimseyle konuşmuyor.", Talk(), Aff(20)),
                St("Yolcunun derdi", "\"Yolcu sıcak bir yemek istiyor.\"", Del("item.stew_plain", 1), Aff(25)),
                St("Hanın sırrı", "Yolcu Şerife'nin kayıp kardeşi çıktı.", Talk(), Keep("item.festival_cup", "Han kupası"))) },
            { "villager.selim", Steps(
                St("Köy kroniği", "Selim köyün kroniğini yazıyor; mürekkep lazım.", Del("item.oak_gall", 3), Aff(20)),
                St("Senin sayfan", "\"Hanedanının sayfasını yazmak istiyorum.\"", Skl(SkillType.Literacy, 4), Aff(25)),
                St("Kronikteki satır", "Selim senin hanedanını kroniğe yazdı.", Talk(), Keep("item.harvest_ribbon", "Kronik sayfası"))) },
            { "villager.elif", Steps(
                St("Kızının çeyizi", "Elif kızının çeyizini hazırlıyor; yün az.", Del("item.wool", 3), Aff(20)),
                St("İşleme", "\"Dokumacılıkta usta bir el gerek.\"", Skl(SkillType.Weaving, 5), Aff(25)),
                St("Çeyiz sandığı", "Elif işlemelerden birini sana armağan ediyor.", Talk(), Keep("item.home_ribbon", "Elif'in işlemesi"))) },
            { "villager.riza", Steps(
                St("Panayır atı", "Rıza panayırda yarışacak bir at için eyer yapıyor.", Del("item.hide", 2), Aff(20)),
                St("Yarış eyeri", "\"Binicilik bilen biri denese...\"", Skl(SkillType.Riding, 4), Aff(25)),
                St("Kurdele", "Rıza yarış kurdelesini sana bağlıyor.", Talk(), Keep("item.harvest_ribbon", "Rıza'nın yarış kurdelesi"))) },
            { "villager.kemal", Steps(
                St("Kaçan balık", "Kemal babasının kaçırdığı dev balığı anlatıyor.", Vis("lake"), Aff(20)),
                St("Balıkçının sabrı", "\"Bir turna tutarsan inanırım.\"", Del("item.fish_pike", 1), Aff(25)),
                St("Babanın oltası", "Kemal babasının oltasını sana gösteriyor.", Talk(), Keep("item.first_fish_scale", "Kemal'in pulu"))) },
            { "villager.mustafa", Steps(
                St("Köprü hayali", "Mustafa ırmağa taş köprü kurmayı hayal ediyor.", Del("item.stone", 6), Aff(20)),
                St("Kemer taşı", "\"Taş ustalığında elin var mı?\"", Skl(SkillType.Masonry, 4), Aff(25)),
                St("Köprünün ilk taşı", "Mustafa sana köprü adağını öneriyor (Adaklar).", Talk(), Keep("item.elder_tin_cup", "Mustafa'nın keskisi"))) },
        };

        /// <summary>Meslek hikâyeleri: adlı köylünün yerine gelen halef ya da varis (belge 46 §15.3).</summary>
        public static readonly Dictionary<string, StoryStep[]> RoleStories = new Dictionary<string, StoryStep[]>
        {
            { "blacksmith", Steps(St("Yeni demirci", "{giver} ilk siparişini arıyor.", Del("item.charcoal", 4), Aff(20)), St("Ustanın gölgesi", "\"Eski usta gibi olabilir miyim?\"", Talk(), Aff(25)), St("İlk nal", "{giver} ilk nalını sana veriyor.", Talk(), Keep("item.harvest_ribbon", "İlk nal kurdelesi"))) },
            { "miller", Steps(St("Yeni değirmenci", "{giver} değirmeni yeniden döndürmek istiyor.", Del("item.wheat", 4), Aff(20)), St("Taşın sesi", "\"Taşlar yine dönüyor.\"", Talk(), Aff(25)), St("İlk un", "{giver} ilk unundan ekmek pişirdi.", Talk(), Keep("item.bread_bowl", "İlk ekmek kabı"))) },
            { "herbalist", Steps(St("Yeni aktar", "{giver} ot bahçesini kuruyor.", Del("item.herb", 3), Aff(20)), St("Kurutma askısı", "\"Otlar nerede biter, nerede başlar?\"", Talk(), Aff(25)), St("İlk merhem", "{giver} ilk merhemini sana verdi.", Talk(), Keep("item.home_ribbon", "Merhem kesesi"))) },
            { "carpenter", Steps(St("Yeni marangoz", "{giver} tezgâhını kuruyor.", Del("item.log", 3), Aff(20)), St("Oyma", "\"İlk oyma işimi beğendin mi?\"", Talk(), Aff(25)), St("İlk tabure", "{giver} ilk taburesini sana verdi.", Talk(), Keep("item.elder_tin_cup", "Oyma bardak"))) },
            { "innkeeper", Steps(St("Yeni hancı", "{giver} hanı yeniden açıyor.", Del("item.bread", 3), Aff(20)), St("İlk misafir", "\"Han dolsun diye dua et.\"", Talk(), Aff(25)), St("Han kupası", "{giver} hanın kupasını sana kaldırdı.", Talk(), Keep("item.festival_cup", "Han kupası"))) },
            { "tailor", Steps(St("Yeni terzi", "{giver} tezgâhını kuruyor.", Del("item.wool", 2), Aff(20)), St("Kumaş", "\"İlk kumaşım nasıl oldu?\"", Talk(), Aff(25)), St("İlk işleme", "{giver} ilk işlemesini sana verdi.", Talk(), Keep("item.home_ribbon", "İşleme kurdelesi"))) },
            { "fisher", Steps(St("Yeni balıkçı", "{giver} göl kıyısını öğreniyor.", Vis("lake"), Aff(20)), St("Ağ", "\"Ağ örmeyi bilir misin?\"", Talk(), Aff(25)), St("İlk balık", "{giver} ilk balığının pulunu sakladı.", Talk(), Keep("item.first_fish_scale", "İlk balık pulu"))) },
            { "mason", Steps(St("Yeni taşçı", "{giver} ocaktan taş seçiyor.", Del("item.stone", 4), Aff(20)), St("Kemer", "\"Bir kemer kurmak isterim.\"", Talk(), Aff(25)), St("İlk taş", "{giver} ilk yonttuğu taşı sana verdi.", Talk(), Keep("item.elder_tin_cup", "Taş kadeh"))) },
        };

        /// <summary>Kişilik hikâyeleri: adsız köylüler (komşu hane üyeleri, yeni gelenler).</summary>
        public static readonly Dictionary<string, StoryStep[]> TraitStories = new Dictionary<string, StoryStep[]>
        {
            { "trait.generous", Steps(St("Cömert komşu", "{giver} fakir bir aileye yardım topluyor.", Del("item.bread", 3), Aff(20)), St("Paylaşmak", "\"Elindekini paylaşmak güzel.\"", Talk(), Aff(25)), St("İyilik", "{giver} köyde iyiliğini anlatıyor.", Talk(), Rep(4))) },
            { "trait.sociable", Steps(St("Şenlik hazırlığı", "{giver} şenlik için herkesi topluyor.", Talk(), Aff(20)), St("Davet", "\"Şenlikte bizim sofraya otur.\"", Del("item.pekmez", 1), Aff(25)), St("Dost sofrası", "{giver} seni en yakın dostu sayıyor.", Talk(), Rep(4))) },
            { "trait.introvert", Steps(St("Sessiz komşu", "{giver} kimseyle konuşmuyor; bir selam bekliyor.", Talk(), Aff(20)), St("Kitap", "\"Okumayı sever misin?\"", Skl(SkillType.Literacy, 3), Aff(25)), St("İç döküş", "{giver} ilk kez derdini anlattı.", Talk(), Rep(3))) },
            { "trait.gloomy", Steps(St("Kasvetli günler", "{giver} kışı sevmez; bir ışık ister.", Del("item.tallow_candle", 2), Aff(20)), St("Kahkaha", "\"Bir fıkra anlatsana.\"", Talk(), Aff(25)), St("Bahar", "{giver} artık daha sık gülüyor.", Talk(), Rep(3))) },
        };

        // ------------------------------------------------------------------ vasiyetler
        /// <summary>Rolü bu olan vasiyet yalnızca easter egg olarak çıkar (belge 55 §2b).</summary>
        public const string OddBequestRole = "odd", OddBequestId = "vas.top";

        public static readonly BequestTemplate[] Bequests =
        {
            Beq(OddBequestId, OddBequestRole, "Topacım yanımda olsun", "{giver} son nefesinde topacının mezarına konmasını istedi.", Del("item.spinning_top", 1)),
            Beq("vas.smith", "blacksmith", "Örs sönmesin", "{giver} son nefesinde ocağının sönmemesini istedi.", Del("item.charcoal", 8)),
            Beq("vas.miller", "miller", "Mayamı yaşat", "{giver} ekşi mayasının yaşatılmasını istedi.", Del("item.bread", 6)),
            Beq("vas.herb", "herbalist", "Ot bahçem", "{giver} ot bahçesinin bakılmasını istedi.", Del("item.chamomile", 6)),
            Beq("vas.shepherd", "shepherd", "Kuzularıma bak", "{giver} sürüsünün sahipsiz kalmamasını istedi.", Act(SkillType.AnimalHusbandry, 6)),
            Beq("vas.carpenter", "carpenter", "Yarım beşik", "{giver} yarım bıraktığı beşiğin bitirilmesini istedi.", Del("item.plank", 6)),
            Beq("vas.inn", "innkeeper", "Ocağım sönmesin", "{giver} hanın ocağının yanmasını istedi.", Del("item.firewood", 10)),
            Beq("vas.fisher", "fisher", "Gölü besle", "{giver} gölün balıksız kalmamasını istedi.", Del("item.fish_carp", 4)),
            Beq("vas.mason", "mason", "Mezar taşım", "{giver} mezar taşını senin yontmanı istedi.", Del("item.cut_stone", 2)),
            Beq("vas.any.tree", "*", "Ceviz ağacımı dik", "{giver} bahçesine bir ağaç dikilmesini istedi.", Act(SkillType.Horticulture, 2)),
            Beq("vas.any.flowers", "*", "Mezarıma çiçek", "{giver} mezarına papatya bırakılmasını istedi.", Del("item.chamomile", 3)),
            Beq("vas.any.music", "*", "Torunuma kaval öğret", "{giver} torununa kaval öğretilmesini istedi.", Skl(SkillType.Music, 3)),
        };

        // ------------------------------------------------------------------ adaklar (kuşaklar boyu)
        public static readonly VowTemplate[] Vows =
        {
            Vow("adak.bridge", "Taş köprü", "Irmağa taş köprü kurulacak; köyün iki yakası birleşecek.", 3, 10,
                Stage("Temel taşları", Del("item.stone", 20)),
                Stage("Kemer", Del("item.cut_stone", 8), Skl(SkillType.Masonry, 7)),
                Stage("Açılış", Pay(60))),
            Vow("adak.seed", "Soy tohumu", "Hanenin kendi tohum çeşidi kuşaklar boyu seçilecek.", 2, 6,
                Stage("Seçme tohum", Act(SkillType.Farming, 30)),
                Stage("Islah", Skl(SkillType.Farming, 8)),
                Stage("Soy tohumu", Skl(SkillType.Farming, 10))),
            Vow("adak.chronicle", "Köy kroniği", "Selim'le birlikte köyün kroniği yazılacak.", 3, 8,
                Stage("Mürekkep ve mühür", Del("item.oak_gall", 6), Del("item.beeswax", 2)),
                Stage("Yazı", Skl(SkillType.Literacy, 6)),
                Stage("Kronik", Skl(SkillType.Literacy, 10))),
            Vow("adak.school", "Köy okulu", "Köyün çocukları için bir okul açılacak.", 3, 10,
                Stage("Öğretmen", Skl(SkillType.Teaching, 5)),
                Stage("Okul binası", Del("item.plank", 20), Pay(40)),
                Stage("Usta öğretmen", Skl(SkillType.Teaching, 10))),
            Vow("adak.horse", "Efsane soy", "Hanenin atları kuşaklar boyu yetiştirilecek.", 2, 6,
                Stage("Binici", Act(SkillType.Riding, 20)),
                Stage("Usta binici", Skl(SkillType.Riding, 8)),
                Stage("Sürü sahibi", Skl(SkillType.AnimalHusbandry, 9))),
            Vow("adak.mill", "Değirmenin taşları", "Köy değirmeni yeniden kurulacak.", 2, 8,
                Stage("Taş", Del("item.stone", 15)),
                Stage("Düzenek", Skl(SkillType.Engineering, 5)),
                Stage("Açılış", Pay(50))),
            Vow("adak.fountain", "Köy çeşmesi", "Meydana taş bir çeşme yapılacak.", 2, 8,
                Stage("Taş ve kireç", Del("item.stone", 10), Del("item.limestone", 6)),
                Stage("Oluk", Skl(SkillType.Masonry, 6)),
                Stage("Açılış", Pay(40))),
        };

        // ------------------------------------------------------------------ yardımcılar
        private static SeasonalTemplate S(string id, Season s, int from, int to, string title, string text, Objective o) =>
            new SeasonalTemplate { Id = id, Season = s, From = from, To = to, Title = title, Text = text, Objective = o };
        public static Objective Act(SkillType skill, int n) => new Objective { Kind = ObjectiveKind.Act, Skill = skill, Count = n };
        public static Objective Del(string item, int n) => new Objective { Kind = ObjectiveKind.Deliver, ItemId = item, Count = n };
        public static Objective Skl(SkillType skill, int level) => new Objective { Kind = ObjectiveKind.Skill, Skill = skill, Level = level, Count = 1 };
        public static Objective Vis(string region) => new Objective { Kind = ObjectiveKind.Visit, Region = region, Count = 1 };
        public static Objective Talk() => new Objective { Kind = ObjectiveKind.Talk, Count = 1 };
        public static Objective Pay(int akce) => new Objective { Kind = ObjectiveKind.Pay, Count = akce };
        private static Reward Aff(int a) => new Reward { Affinity = a, Memory = 5 };
        private static Reward Rep(int r) => new Reward { Affinity = 20, Reputation = r, Memory = 10 };
        private static Reward Keep(string item, string label) => new Reward { Affinity = 30, Memory = 15, KeepsakeItem = item, KeepsakeLabel = label };
        private static StoryStep St(string title, string text, Objective o, Reward r) => new StoryStep { Title = title, Text = text, Objective = o, Reward = r };
        private static StoryStep[] Steps(params StoryStep[] s) => s;
        private static BequestTemplate Beq(string id, string role, string title, string text, Objective o) => new BequestTemplate { Id = id, Role = role, Title = title, Text = text, Objective = o };

        private static CallPath P(string label, SkillType? skill, int level, string item = null, int count = 0, int akce = 0, double hours = 0, string outcome = null) =>
            new CallPath { Label = label, Skill = skill, Level = level, ItemId = item, ItemCount = count, Akce = akce, Hours = hours, Outcome = outcome };

        private static CallTemplate Call(string id, string role, string title, string text, string ignore, bool hard, Season[] seasons, params CallPath[] paths)
        {
            var c = new CallTemplate { Id = id, Role = role, Title = title, Text = text, IgnoreText = ignore, Hard = hard, Seasons = seasons };
            c.Paths.AddRange(paths);
            return c;
        }

        private static VowStage Stage(string title, params Objective[] objectives)
        {
            var st = new VowStage { Title = title, Reward = new Reward { Reputation = 3 } };
            st.Objectives.AddRange(objectives);
            return st;
        }

        private static VowTemplate Vow(string id, string title, string text, int seals, int rep, params VowStage[] stages)
        {
            var v = new VowTemplate { Id = id, Title = title, Text = text, FinalSeals = seals, FinalReputation = rep };
            v.Stages.AddRange(stages);
            return v;
        }

        /// <summary>Şablonların kullandığı bütün eşya kimlikleri (içerik denetimi için).</summary>
        public static IEnumerable<string> AllItemIds()
        {
            foreach (var f in Favors) if (f.Item != null) yield return f.Item;
            foreach (var s in Seasonal) if (s.Objective.ItemId != null) yield return s.Objective.ItemId;
            foreach (var c in Calls) foreach (var p in c.Paths) if (p.ItemId != null) yield return p.ItemId;
            foreach (var dict in new[] { NamedStories, RoleStories, TraitStories })
                foreach (var kv in dict)
                    foreach (var st in kv.Value)
                    {
                        if (st.Objective.ItemId != null) yield return st.Objective.ItemId;
                        if (st.Reward.KeepsakeItem != null) yield return st.Reward.KeepsakeItem;
                    }
            foreach (var b in Bequests) if (b.Objective.ItemId != null) yield return b.Objective.ItemId;
            foreach (var v in Vows) foreach (var st in v.Stages) foreach (var o in st.Objectives) if (o.ItemId != null) yield return o.ItemId;
            foreach (Season s in Enum.GetValues(typeof(Season))) foreach (var id in SeasonSet(s)) yield return id;
        }

        /// <summary>Şablon sayımı (belge 46 §15.4 asgari hedefle karşılaştırma).</summary>
        public static int TemplateCount()
        {
            int n = Favors.Length + Seasonal.Length + Calls.Length + Bequests.Length + Vows.Length;
            foreach (var dict in new[] { NamedStories, RoleStories, TraitStories }) foreach (var kv in dict) n += kv.Value.Length;
            return n;
        }
    }
}
