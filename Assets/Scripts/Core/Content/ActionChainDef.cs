using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// İzlenen eylem zincirinin bir adımı (Project belgesi 60): karakter işi tek pozda beklemek yerine görünür adımlarla yapar.
    /// Adım yalnızca görünümü ve sırayı tanımlar; işin süresi ve sonucu etkileşimin kendisindedir.
    /// </summary>
    public sealed class ActionStepDef
    {
        /// <summary>Adım kimliği; arayüz metni "chain.step.&lt;Id&gt;" anahtarından gelir.</summary>
        public string Id { get; set; }
        /// <summary>Oynatılacak kare anahtarı (cook, stir, knead...). Karesi yoksa görünüm etkileşimin eski simgesine düşer.</summary>
        public string Pose { get; set; }
        /// <summary>Adım başlarken çalınacak ses anahtarı; yoksa null.</summary>
        public string Sfx { get; set; }
        /// <summary>
        /// Elde görünen eşya: "consume" (etkileşimin harcadığı), "produce" (ürettiği), "item.xyz" (belirli eşya) ya da null.
        /// </summary>
        public string Held { get; set; }
        /// <summary>Adımın toplam süredeki payı (ağırlık). Zincir toplam süreyi değiştirmez, böler.</summary>
        public double Share { get; set; } = 1.0;
        /// <summary>Adım yalnızca bu koşul sağlanırsa oynar (bkz. <see cref="ChainConditions"/>); boşsa her zaman.</summary>
        public string When { get; set; }
        /// <summary>Bu adımda karakter soyunuktur (peştamal; yıkanma). Çıplak gövde çizilmez.</summary>
        public bool Undress { get; set; }
    }

    /// <summary>Zincirin hangi etkileşimlere bağlanacağı. Verilen alanların hepsi tutmalı; boş alan sınamaz.</summary>
    public sealed class ChainMatch
    {
        /// <summary>Eşya kimliği (item.cooking_pot).</summary>
        public string Station { get; set; }
        /// <summary>Eşya kategorisi (sleep, kitchen...).</summary>
        public string Category { get; set; }
        /// <summary>Eşya etiketi.</summary>
        public string Tag { get; set; }
        /// <summary>Etkileşimin doldurduğu ihtiyaç.</summary>
        public NeedType? Need { get; set; }
        public SkillType? Skill { get; set; }
        /// <summary>Etkileşim türü bayrağı (bkz. <see cref="ChainFlags"/>).</summary>
        public string Flag { get; set; }
        /// <summary>Etkileşim kimliği bu önekle başlamalı.</summary>
        public string IdPrefix { get; set; }

        public bool IsEmpty => Station == null && Category == null && Tag == null && Need == null && Skill == null && Flag == null && IdPrefix == null;

        public bool Matches(ItemDef item, InteractionDef i)
        {
            if (Station != null && !string.Equals(item.Id, Station, StringComparison.Ordinal)) return false;
            if (Category != null && !string.Equals(item.Category, Category, StringComparison.Ordinal)) return false;
            if (Tag != null && !item.HasTag(Tag)) return false;
            if (Need.HasValue && !(i.RestoresNeed && i.Need == Need.Value)) return false;
            if (Skill.HasValue && i.Skill != Skill) return false;
            if (Flag != null && !ChainFlags.Has(i, Flag)) return false;
            if (IdPrefix != null && (i.Id == null || !i.Id.StartsWith(IdPrefix, StringComparison.Ordinal))) return false;
            return true;
        }
    }

    /// <summary>Eylem zinciri: sıralı adımlar ve eşleme kuralı. Eşleme sırası dosyadaki sıradır; ilk tutan kazanır.</summary>
    public sealed class ActionChainDef : IContentDef
    {
        public string Id { get; set; }
        public List<ActionStepDef> Steps { get; } = new List<ActionStepDef>();
        /// <summary>Kendiliğinden bağlanma kuralları (herhangi biri tutarsa). Boşsa zincir yalnızca adıyla bağlanır.</summary>
        public List<ChainMatch> Matches { get; } = new List<ChainMatch>();

        public bool MatchesAny(ItemDef item, InteractionDef i)
        {
            foreach (var m in Matches) if (m.Matches(item, i)) return true;
            return false;
        }
    }

    /// <summary>Eşleme kuralında kullanılabilen etkileşim türü bayrakları.</summary>
    public static class ChainFlags
    {
        public static readonly string[] All =
        {
            "bath", "slaughter", "craft", "produce", "harvest", "collect", "water", "fertilize", "eatContents", "fillDecor",
            "feed", "pet", "teach", "heal", "eat", "drink", "sleep", "toilet", "repair"
        };

        public static bool IsKnown(string flag) => Array.IndexOf(All, flag) >= 0;

        public static bool Has(InteractionDef i, string flag)
        {
            switch (flag)
            {
                case "bath": return i.Bath;
                case "slaughter": return i.Slaughter;
                case "craft": return i.IsCraft;
                case "produce": return i.Produce.Count > 0 && !i.RemovesItem && !i.IsCraft;
                case "harvest": return i.RemovesItem && i.Produce.Count > 0;
                case "collect": return i.Collect || i.CollectContents;
                case "water": return i.Water;
                case "fertilize": return i.Fertilize > 0f;
                case "eatContents": return i.EatContents;
                case "fillDecor": return i.FillDecor;
                case "feed": return i.AddFeed > 0;
                case "pet": return i.PetsAnimals || i.BefriendAnimals > 0f;
                case "teach": return i.Teach;
                case "heal": return i.Heal > 0f;
                // Yeme ve içme: açlığı doyuran, üretim yapmayan etkileşim. İçecek tuvaleti daha çok doldurur (BladderCost).
                case "eat": return i.RestoresNeed && i.Need == NeedType.Hunger && i.Produce.Count == 0 && i.BladderCost < 10f;
                case "drink": return i.RestoresNeed && i.Produce.Count == 0 && i.BladderCost >= 10f;
                case "sleep": return i.RestoresNeed && i.Need == NeedType.Energy && i.DurationDays >= 0.1;
                case "toilet": return i.RestoresNeed && i.Need == NeedType.Bladder;
                case "repair": return i.RepairMaterial != null || i.Polish;
                default: return false;
            }
        }
    }

    /// <summary>
    /// Adım koşulları ("when"): adımın bu kişi için, bu an oynayıp oynamayacağı. Bilinmeyen koşul içerik hatasıdır.
    /// </summary>
    public static class ChainConditions
    {
        /// <summary>Beceriksiz: işin becerisinde seviye 3 ve altı.</summary>
        public const string SkillLow = "skillLow";
        /// <summary>Usta: işin becerisinde seviye 7 ve üstü.</summary>
        public const string SkillHigh = "skillHigh";
        /// <summary>Ruh hali İyi ya da Mutlu (ıslık, mırıldanma).</summary>
        public const string MoodGood = "moodGood";
        /// <summary>Ruh hali Mutsuz ya da Perişan (iç çekme).</summary>
        public const string MoodBad = "moodBad";
        /// <summary>Bu iş için kovayla su taşındı (tesisattan gelmedi).</summary>
        public const string CarriedWater = "carriedWater";
        /// <summary>Eşya ısı kaynağı (ocak, mangal): ateşle ilgili adımlar.</summary>
        public const string Fire = "fire";

        public static readonly string[] All = { SkillLow, SkillHigh, MoodGood, MoodBad, CarriedWater, Fire };

        public static bool IsKnown(string when) => Array.IndexOf(All, when) >= 0;

        public const int LowSkillMax = 3, HighSkillMin = 7;
    }
}
