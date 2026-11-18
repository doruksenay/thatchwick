using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Content
{
    /// <summary>Etkileşim sonunda haneye giren ürün (ör. ağaç kesince 3 kütük). Chance 0-1 arası olasılık.</summary>
    public sealed class ProduceDef
    {
        public string ItemId { get; set; }
        public int Count { get; set; } = 1;
        public float Chance { get; set; } = 1f;
        /// <summary>Etkileşimin becerisinde en az bu seviye (0 = sınır yok). Yemek kalitesi bununla ayrılır.</summary>
        public int MinLevel { get; set; }
        /// <summary>En çok bu seviye (0 = sınır yok).</summary>
        public int MaxLevel { get; set; }
        /// <summary>Yalnızca bu mevsimlerde çıkar (null = her mevsim): balık türleri.</summary>
        public List<Time.Season> Seasons { get; set; }
    }

    public sealed class NeedRestoreDef
    {
        public NeedType Need { get; set; }
        public float Amount { get; set; }
    }

    /// <summary>Etkileşimin harcadığı malzeme (marangozlukta 2 tahta + 1 çivi gibi).</summary>
    public sealed class ConsumeDef
    {
        public string ItemId { get; set; }
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// Bir eşyanın tanımladığı etkileşim ("akıllı nesne"): eşya kendi etkileşimini söyler,
    /// karakter sistemi yalnızca uygular. İki tür vardır:
    /// ihtiyaç etkileşimi (yatak = Enerji +60, 0.3 gün) ve iş etkileşimi (ağaç kes = 3 kütük,
    /// ağaç kalkar; odun yar = 1 kütük harcar, 3 odun üretir). İş etkileşiminde Restore 0 olabilir.
    /// </summary>
    public sealed class InteractionDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Doyurulan ihtiyaç; Restore 0 ise anlamsızdır (saf iş etkileşimi).</summary>
        public NeedType Need { get; set; }
        public float Restore { get; set; }
        public double DurationDays { get; set; }

        /// <summary>Kullanım sonunda beceri kazanımı (isteğe bağlı). SkillXp 0 ise kazanım yok.</summary>
        public SkillType? Skill { get; set; }
        public float SkillXp { get; set; }

        // ---- iş etkileri (Adım 13) ----
        /// <summary>Yalnızca büyümesini tamamlamış bitkide yapılabilir (ağaç kesmek).</summary>
        public bool RequiresGrown { get; set; }
        /// <summary>Başlamak için hane envanterinde olması gereken ve sonunda harcanan kaynak (1 adet).</summary>
        public string ConsumeItem { get; set; }
        public List<ProduceDef> Produce { get; set; } = new List<ProduceDef>();
        /// <summary>İş bitince eşya dünyadan kalkar (kesilen ağaç).</summary>
        public bool RemovesItem { get; set; }


        // ---- yaşam katmanı (v0) ----
        /// <summary>Ana ihtiyaca ek olarak doyurulanlar (ateş başında oturmak Keyif + Sosyal).</summary>
        public List<NeedRestoreDef> ExtraRestore { get; set; } = new List<NeedRestoreDef>();
        /// <summary>axe/hoe/saw: bu işin hızı alet kademesine bağlı.</summary>
        public string Tool { get; set; }
        /// <summary>Katalog dışı anlık moodlet (kimlik, metin, değer, gün); Moodlet kataloğa başvurur.</summary>
        public string MoodId { get; set; }
        public string MoodText { get; set; }
        public int MoodValue { get; set; }
        public double MoodDays { get; set; }
        public bool RequiresLit { get; set; }

        /// <summary>Çok malzemeli harcama (ConsumeItem'a ek olarak).</summary>
        public List<ConsumeDef> Consume { get; set; } = new List<ConsumeDef>();

        // ---- ruh hali, üretim, hayvanlar ----
        /// <summary>İş bitince karaktere verilen moodlet kimliği (MoodCatalog).</summary>
        public string Moodlet { get; set; }
        /// <summary>Başlamak için gereken beceri seviyesi (Skill ile birlikte). 0 = yok.</summary>
        public int MinSkillLevel { get; set; }
        /// <summary>Marangoz tezgâhı tarifinden üretilmiş etkileşim (menüde "Yap" grubunda).</summary>
        public bool IsCraft { get; set; }
        /// <summary>Barınağın yem stoğuna eklenen birim (harcanan malzemeyle).</summary>
        public int AddFeed { get; set; }
        /// <summary>Buzluğa eklenen buz (gün).</summary>
        public float AddIce { get; set; }
        /// <summary>Barınaktaki hayvanlara dostluk (merhem); günlük sevme hakkını kullanmaz.</summary>
        public float BefriendAnimals { get; set; }
        /// <summary>Tuvaleti bu kadar doldurur (içecek 15, yemek 4).</summary>
        public float BladderCost { get; set; }
        /// <summary>Banyo/leğen: başlamadan önce en yakın sudan (kuyu, gölet, dere) bir kova su taşınır.</summary>
        public bool FetchWater { get; set; }
        /// <summary>Yıkananı bu kadar ıslatır (0-1): dışarıda ya da suya girerek yıkanmak.</summary>
        public float Wets { get; set; }
        /// <summary>Soğuk su: ocak yanmayan yerde ve soğukta yıkanınca üşür; ocak yanan odada sıcak banyo.</summary>
        public bool Bath { get; set; }
        /// <summary>Kullanınca eşyanın dolumunu artırır (lazımlık, hela çukuru).</summary>
        public int AddFill { get; set; }
        /// <summary>Eşyanın dolumunu boşaltır; dolum başına ürün verir (gübre).</summary>
        public bool EmptyFill { get; set; }
        /// <summary>Kurular (havlu): ıslaklığı alır.</summary>
        public bool Dries { get; set; }
        /// <summary>Hanedeki çocuk ve gençlere ders verir (öğretmenlik).</summary>
        public bool Teach { get; set; }
        /// <summary>Kullananın soğuk/açlık yarasını bu kadar gün iyileştirir (şifa merhemi).</summary>
        public float Heal { get; set; }
        /// <summary>Yalnızca don varken yapılır (buz kesmek).</summary>
        public bool RequiresFrost { get; set; }
        /// <summary>Yalnızca tesisata su gelirken yapılır (çeşmede yüz yıkamak).</summary>
        public bool NeedsPipedWater { get; set; }
        /// <summary>Barınaktaki ürünleri (yumurta, süt, yün) haneye toplar.</summary>
        public bool Collect { get; set; }
        /// <summary>Barınakta yaşayan bir hayvan gerektirir (köpekle oyna).</summary>
        public bool RequiresAnimal { get; set; }

        // ---- çiftçilik ----
        /// <summary>Büyüyen ekini sular (o gün büyümesi tam hızda olur, kalite artar).</summary>
        public bool Water { get; set; }
        /// <summary>Ekinin toprağına bu kadar verim ekler (0-1); harcanan gübre Consume ile verilir.</summary>
        public float Fertilize { get; set; }
        /// <summary>Barınaktaki hayvanları sever: dostlukları artar (hayvan başına günde bir kez).</summary>
        public bool PetsAnimals { get; set; }
        /// <summary>Barınaktaki yetişkin, dostu olmayan bir hayvanı keser; hayvanın türüne göre et verir.</summary>
        public bool Slaughter { get; set; }

        // ---- ev dekorasyonu (Project belgesi 27) ----
        /// <summary>Onarım istasyonu: bu malzemeden (wood, metal, cloth) hasarlı bir eşyayı onarır (önce envanterdeki).</summary>
        public string RepairMaterial { get; set; }
        /// <summary>Kararmış metal aynayı cilalar.</summary>
        public bool Polish { get; set; }
        /// <summary>İçindekileri (döküntü yığını) durumlarıyla envantere alır.</summary>
        public bool CollectContents { get; set; }
        /// <summary>Doldurulan dekoru envanterdeki gerçek ürünle doldurur (D2).</summary>
        public bool FillDecor { get; set; }
        /// <summary>Doldurulan dekordan bir tane yer (Need/Restore ile açlık giderir).</summary>
        public bool EatContents { get; set; }
        /// <summary>Yalnızca meyvesi olgun ağaçta yapılabilir.</summary>
        public bool RequiresFruit { get; set; }
        /// <summary>Meyveyi toplar: ağaç yerinde kalır, meyve yeniden olgunlaşmaya başlar.</summary>
        public bool TakesFruit { get; set; }

        /// <summary>Harcanan tüm malzemeler (ConsumeItem 1 adet + Consume listesi), aynı kimlikler toplanır.</summary>
        public List<ConsumeDef> AllConsumed()
        {
            var list = new List<ConsumeDef>();
            if (!string.IsNullOrEmpty(ConsumeItem)) Add(list, ConsumeItem, 1);
            foreach (var c in Consume) Add(list, c.ItemId, c.Count);
            return list;
        }

        private static void Add(List<ConsumeDef> list, string id, int count)
        {
            foreach (var c in list) if (c.ItemId == id) { c.Count += count; return; }
            list.Add(new ConsumeDef { ItemId = id, Count = count });
        }

        public bool HasEffects => !string.IsNullOrEmpty(ConsumeItem) || Consume.Count > 0 || Produce.Count > 0 || RemovesItem
            || AddFeed > 0 || Collect || RequiresAnimal || AddIce > 0f || RequiresFrost || NeedsPipedWater || BefriendAnimals > 0f || Teach || Heal > 0f || Wets > 0f || Bath || AddFill > 0 || EmptyFill || Dries
            || Water || Fertilize > 0f || TakesFruit || RequiresFruit || PetsAnimals || Slaughter
            || RepairMaterial != null || Polish || CollectContents || FillDecor || EatContents;
        public bool RestoresNeed => Restore > 0f;
        public bool GivesMood => !string.IsNullOrEmpty(MoodId) && MoodValue != 0 && MoodDays > 0.0;

        /// <summary>Bu etkileşimin verilen ihtiyaca sağladığı doygunluk (ana + ek); yoksa 0.</summary>
        public float RestoreFor(NeedType need)
        {
            float amount = RestoresNeed && Need == need ? Restore : 0f;
            foreach (var e in ExtraRestore) if (e.Need == need) amount += e.Amount;
            return amount;
        }
    }
}
