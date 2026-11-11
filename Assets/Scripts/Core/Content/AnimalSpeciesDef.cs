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
        public float LifespanDays { get; set; } = 120f;
        /// <summary>Ürün vermeye başladığı yaş (oyun günü).</summary>
        public float MatureDays { get; set; } = 4f;
        public bool IsPet { get; set; }
        /// <summary>Kesilince verdiği et (yoksa null: kesilmez) ve adedi.</summary>
        public string Meat { get; set; }
        public int MeatCount { get; set; } = 1;
        public int UnlockLevel { get; set; } = 1;
    }
}
