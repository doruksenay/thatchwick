using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Life
{
    /// <summary>Özellik: karaktere doğuştan gelen, kalıtılan kalıcı etki. En çok 2 tane.</summary>
    public sealed class TraitDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        /// <summary>Birlikte bulunamayacağı özellikler (Neşeli - Kasvetli).</summary>
        public List<string> Conflicts { get; set; } = new List<string>();
        public LifeModifiers Effects { get; set; } = new LifeModifiers();

        public bool ConflictsWith(TraitDef other) =>
            other != null && (Conflicts.Contains(other.Id) || other.Conflicts.Contains(Id));
    }

    /// <summary>Meslek dalı: beceri 5. ve 10. seviyede iki dallı seçim (Stardew gibi).</summary>
    public sealed class BranchDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public SkillType Skill { get; set; }
        public int Level { get; set; }
        /// <summary>İkinci kademe dal için gereken ilk kademe dalı; boşsa ilk kademe.</summary>
        public string Requires { get; set; }
        public LifeModifiers Effects { get; set; } = new LifeModifiers();
    }

    /// <summary>Tamamlanan köy projelerinin köy geneline etkisi.</summary>
    public sealed class VillageEffects
    {
        public float CropGrowth;
        public float MarketSell;
        public float SpoilReduction;
        public float ColdResist;
        public int FestivalMood;
        public int Status;

        public void Clear()
        {
            CropGrowth = 0; MarketSell = 0; SpoilReduction = 0; ColdResist = 0; FestivalMood = 0; Status = 0;
        }

        public void Add(VillageEffects o)
        {
            CropGrowth += o.CropGrowth;
            MarketSell += o.MarketSell;
            SpoilReduction += o.SpoilReduction;
            ColdResist += o.ColdResist;
            FestivalMood += o.FestivalMood;
            Status += o.Status;
        }
    }

    public sealed class CostDef
    {
        public string ItemId { get; set; }
        public int Count { get; set; }
    }

    public sealed class ProjectDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int Akce { get; set; }
        public List<CostDef> Costs { get; set; } = new List<CostDef>();
        public VillageEffects Effects { get; set; } = new VillageEffects();
    }

    public sealed class VillagerEventDef
    {
        public string Id { get; set; }
        public int MinAffection { get; set; }
        public string Text { get; set; }
        public int RewardAkce { get; set; }
        public string RewardItem { get; set; }
        public int RewardCount { get; set; }
    }

    /// <summary>Köylü: bir kez oluşturulur (Id sabit, karakter kimliği oyun başında atanır), programı ve zevkleri vardır.</summary>
    public sealed class VillagerDef : IContentDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public int Age { get; set; }
        public string Role { get; set; }
        public string RoleName { get; set; }
        public List<string> Traits { get; set; } = new List<string>();
        public List<string> Loves { get; set; } = new List<string>();
        public List<string> Likes { get; set; } = new List<string>();
        public List<string> Dislikes { get; set; } = new List<string>();
        public string Home { get; set; }
        public string Work { get; set; }
        public List<VillagerEventDef> Events { get; set; } = new List<VillagerEventDef>();
    }

    public sealed class FestivalDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Season Season { get; set; }
        public int Day { get; set; }
        public float FunRestore { get; set; }
        public float SocialRestore { get; set; }
        public int MoodValue { get; set; }
        public int ExtraOffers { get; set; }
    }

    public sealed class GoalDef : IContentDef
    {
        /// <summary>Hedef ölçütleri (GoalService hesaplar): nesil, yaşayan, akçe, proje, dal, beceri, gün, evlilik, şenlik.</summary>
        public static readonly string[] Metrics =
            { "generations", "living", "akce", "projects", "branches", "skill", "days", "marriages", "festivals" };

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Metric { get; set; }
        public int Target { get; set; }
        public int RewardAkce { get; set; }
        public int RewardXp { get; set; }
    }

    public sealed class ToolTierDef
    {
        public string Name { get; set; }
        public int Akce { get; set; }
        public int Firewood { get; set; }
        public float Speed { get; set; } = 1f;
        public int Level { get; set; }
    }

    public sealed class ToolDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Kademe 0 temel alettir; Tiers[0] ilk yükseltme (kademe 1).</summary>
        public List<ToolTierDef> Tiers { get; set; } = new List<ToolTierDef>();
    }

    public enum TemplateSide { North = 0, East = 1, South = 2, West = 3 }

    public sealed class TemplateItemDef
    {
        public string ItemId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Rotation { get; set; }
    }

    /// <summary>Oda şablonu: duvarları, kapısı, zemini ve eşyalarıyla hazır bir oda (Adım 43).</summary>
    public sealed class RoomTemplateDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TemplateSide DoorSide { get; set; } = TemplateSide.South;
        public int DoorOffset { get; set; }
        /// <summary>Kapının karşı duvarındaki pencere konumları (duvar boyunca sıra).</summary>
        public List<int> Windows { get; set; } = new List<int>();
        public string Floor { get; set; }
        public List<TemplateItemDef> Items { get; set; } = new List<TemplateItemDef>();
    }
}
