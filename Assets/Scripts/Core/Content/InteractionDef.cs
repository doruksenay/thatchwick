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

        public bool HasEffects => !string.IsNullOrEmpty(ConsumeItem) || Produce.Count > 0 || RemovesItem;
        public bool RestoresNeed => Restore > 0f;
    }
}
