using System;
namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Hayvan türü: hangi barınakta yaşar, ne kadar yem yer, ne üretir. Hayvanlar barınağın yem
    /// stoğundan yer, tok ve erişkinken ürün verir (yumurta, süt, yün); evcil hayvan keyif verir.
    /// </summary>
    public sealed class AnimalSpeciesDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Price { get; set; }
        /// <summary>Barınak türü (ItemDef.HousingKind ile eşleşir).</summary>
        public string Housing { get; set; }
        /// <summary>Günde yediği yem birimi.</summary>
        public float FeedPerDay { get; set; } = 1f;
        /// <summary>Ürün kimliği (yoksa null: evcil hayvan).</summary>
        public string Produces { get; set; }
        public float ProduceEveryDays { get; set; } = 1f;
        /// <summary>Ürün hızının mevsim çarpanı [ilkbahar, yaz, güz, kış] (tavuk kışın az yumurtlar, koyun yılda kırkılır); null ise hep 1.</summary>
        public float[] SeasonOutput { get; set; }

        public float OutputFactor(Time.Season season) =>
            SeasonOutput == null || SeasonOutput.Length < 4 ? 1f : Math.Max(0f, SeasonOutput[(int)season]);
        public float LifespanDays { get; set; } = 120f;
        /// <summary>Ürün vermeye başladığı yaş (oyun günü).</summary>
        public float MatureDays { get; set; } = 4f;
        public bool IsPet { get; set; }
        /// <summary>Kesilince verdiği et (yoksa null: kesilmez) ve adedi.</summary>
        public string Meat { get; set; }
        public int MeatCount { get; set; } = 1;
        /// <summary>Kesilince verdiği ham deri (yoksa null) ve adedi.</summary>
        public string Hide { get; set; }
        /// <summary>Mezbahada çıkan yan ürünler (kemik, iç yağı, sakatat, bağırsak, boynuz, şirden).</summary>
        public System.Collections.Generic.List<Cooking.ByproductDef> Byproducts { get; } = new System.Collections.Generic.List<Cooking.ByproductDef>();
        public int HideCount { get; set; } = 1;
        /// <summary>Binek: erişkin ve eyerliyken binilir; binince yürüme hızı çarpanı.</summary>
        public bool Rideable { get; set; }
        public float RideSpeed { get; set; } = 2f;
        /// <summary>Barınak odasında hayvan başına istenen karo (konfor: sıkışıklık).</summary>
        public float Space { get; set; } = 2f;
        /// <summary>Bir adımda yola/çime bıraktığı iz (aşınma): inek ve at 3, koyun ve keçi 1, tavuk 0.</summary>
        public float Trample { get; set; }
        public int UnlockLevel { get; set; } = 1;

        // ---- üreme (28. belge, 18.11.2026) ----
        /// <summary>Gebelik ya da kuluçka (oyun günü; 1 gün ≈ yarım yıl): inek ~1,5, koyun ve keçi ~1, domuz ~0,7, tavuk kuluçkası bir gece.</summary>
        public float GestationDays { get; set; } = 1f;
        /// <summary>Bir doğumdaki yavru sayısı aralığı (barınakta yer kadarı doğar).</summary>
        public int LitterMin { get; set; } = 1;
        public int LitterMax { get; set; } = 1;
        /// <summary>Ürünü yalnızca dişi verir (yumurta, süt); yün ve gübre ikisinden de.</summary>
        public bool FemaleProduces { get; set; }
        /// <summary>Çeki hayvanı: düven çeker (at, inek ve öküz).</summary>
        public bool Draft { get; set; }
        /// <summary>Cinsiyet adları (tavuk/horoz, inek/boğa ...); null ise dişi/erkek.</summary>
        public string FemaleName { get; set; }
        public string MaleName { get; set; }
        /// <summary>Bu kuşaktan itibaren soy hayvanı.</summary>
        public const int HeritageGeneration = 3;
        /// <summary>Üreyebilir mi (evcil hayvan ve arı hariç).</summary>
        /// <summary>Hep erkek (iğdiş öküz): üremez, satın alınan hep erkektir.</summary>
        public bool MaleOnly { get; set; }
        /// <summary>Evcil hayvan da yavrular (köpek ve kedi; belge 31-2, 48 §6.1).</summary>
        public bool PetBreeds { get; set; }
        /// <summary>Kısır melez (katır, bardo): kendi türünden yavru vermez; dişide nadir "mucize tay" (belge 48 §7.4).</summary>
        public bool Sterile { get; set; }
        /// <summary>Sürü hayvanı: köy merasına ve yaylaya gönderilebilir (belge 48 §8.2).</summary>
        public bool Herd { get; set; }
        public bool Breeds => (!IsPet || PetBreeds) && Housing != "hive" && !MaleOnly && !Sterile;
        /// <summary>Genomu var (köpek, kedi, at ailesi).</summary>
        public bool HasGenetics => Thatchwick.Core.Animals.Genetics.GeneticsCatalog.HasGenetics(Id);
    }
}
