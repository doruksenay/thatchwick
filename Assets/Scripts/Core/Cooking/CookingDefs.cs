using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Cooking
{
    /// <summary>
    /// Malzeme grubu (belge 58 §5.1): tarifte bir yuva belirli bir eşya yerine grubu isteyebilir ("herhangi bir balık").
    /// Kimlik <c>group.</c> ile başlar; hane envanteri grubu üyelerinin toplamı olarak sayar ve harcar
    /// (<see cref="Build.Household.GroupMembers"/>). Üyenin payı yemeğin sofra puanına geçer; yeni eşya kimliği doğmaz.
    /// </summary>
    public sealed class IngredientGroupDef : IContentDef
    {
        public const string Prefix = "group.";
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Üyeler, harcama sırasıyla (en sıradan önce).</summary>
        public List<GroupMember> Members { get; } = new List<GroupMember>();

        public static bool IsGroup(string id) => id != null && id.StartsWith(Prefix, StringComparison.Ordinal);

        public bool Contains(string itemId)
        {
            foreach (var m in Members) if (m.ItemId == itemId) return true;
            return false;
        }

        public int BonusOf(string itemId)
        {
            foreach (var m in Members) if (m.ItemId == itemId) return m.Bonus;
            return 0;
        }
    }

    public sealed class GroupMember
    {
        public string ItemId;
        /// <summary>Sofra puanına katkı (−3..+3): sızma zeytinyağı artı, iç yağı eksi.</summary>
        public int Bonus;
        /// <summary>Üyenin alt sınıfı (beyaz etli, yağlı, kabuklu): tarif kartında görünür.</summary>
        public string Kind;
    }

    /// <summary>Yemek sınıfı: servis kabı görünümü, damak tadı ve baharat uyumu buna göre.</summary>
    public enum DishClass
    {
        None = 0, Soup = 1, Meat = 2, Fish = 3, Vegetable = 4, Grain = 5, Dairy = 6, Pastry = 7, Sweet = 8, Snack = 9, Drink = 10
    }

    /// <summary>Tarif nasıl öğrenilir (belge 58 §4).</summary>
    public enum LearnKind
    {
        /// <summary>Beceri seviyesi yetince karakter kendi çıkarır (bugünkü davranış).</summary>
        Level = 0,
        /// <summary>Herkesin bildiği temel tarif.</summary>
        Basic = 1,
        /// <summary>Köylü yakınlığıyla: <see cref="DishDef.LearnFrom"/> köylü kimliği, <see cref="DishDef.LearnAt"/> yakınlık.</summary>
        Villager = 2,
        /// <summary>Tüccar risalesi okununca.</summary>
        Treatise = 3,
        /// <summary>İstek ödülü.</summary>
        Request = 4,
        /// <summary>Yalnızca aile defterinden ya da hanede öğretmeyle.</summary>
        Family = 5
    }

    /// <summary>
    /// Yemek tanımı: tarif ve kazan etkileşimlerinin tek ortak modeli (belge 58 §11, "tek tarif modeli").
    /// Kimlik, ürünün eşya kimliğidir; aynı yemeği veren birden çok tarif tek yemek sayılır.
    /// </summary>
    public sealed class DishDef : IContentDef
    {
        public string Id { get; set; }
        public DishClass Class { get; set; }
        public LearnKind Learn { get; set; }
        public string LearnFrom { get; set; }
        public int LearnAt { get; set; }
        /// <summary>Servis kabı (pot, crock, tray, sini, bowl) ve görünüm (soup, stew, pilaf, sarma, pastry, fish, fry, sweet, salad).</summary>
        public string Vessel { get; set; } = "pot";
        public string Look { get; set; } = "stew";
        public Characters.SkillType Skill { get; set; }
        public int MinLevel { get; set; }
        /// <summary>Bu yemeği veren (istasyon, etkileşim) çiftleri.</summary>
        public List<(ItemDef station, InteractionDef interaction)> Sources { get; } = new List<(ItemDef, InteractionDef)>();
        public bool IsMeal => Class != DishClass.None && Class != DishClass.Drink;
    }

    /// <summary>Ek baharat (belge 58 §5.2): yemeğe bir özellik katar, yiyene moodlet verir.</summary>
    public sealed class SpiceDef : IContentDef
    {
        /// <summary>Baharat eşyasının kimliği.</summary>
        public string Id { get; set; }
        /// <summary>Yiyene verilen etki (items.json effects).</summary>
        public string Effect { get; set; }
        /// <summary>İşlenmiş baharat güçlüdür ve bozulmaz; taze ot zayıftır.</summary>
        public bool Processed { get; set; }
        /// <summary>Uyduğu sınıflar; boşsa tatlı ve içecek dışındaki hepsi.</summary>
        public List<DishClass> Fits { get; } = new List<DishClass>();
        public List<DishClass> Clashes { get; } = new List<DishClass>();
        /// <summary>Kokuya katkı (defne): pişerken koku menzili ve gücü artar.</summary>
        public int Smell { get; set; }

        public bool FitsClass(DishClass c)
        {
            if (Clashes.Contains(c)) return false;
            if (Fits.Count > 0) return Fits.Contains(c);
            return c != DishClass.Sweet && c != DishClass.Drink;
        }
    }

    /// <summary>Kesim yan ürünü (belge 60a §2.2): türe göre; <see cref="YoungOnly"/> şirden mayası gibi yalnızca genç hayvandan.</summary>
    public sealed class ByproductDef
    {
        public string ItemId;
        public int Count = 1;
        /// <summary>Acemi elinde ziyan olma payı için temel şans (0-1); Hayvancılık seviyesi artırır.</summary>
        public float Chance = 1f;
        public bool YoungOnly;
    }

    /// <summary>Birlikte iş, koku ve boşta konuşma satırı (belge 60b §3). Metinler <c>talk.&lt;id&gt;.a/.b/.c</c> anahtarlarında.</summary>
    public sealed class TalkLineDef : IContentDef
    {
        public string Id { get; set; }
        public List<string> Tags { get; } = new List<string>();
        /// <summary>Satır sayısı: 1 tek söz, 2 söz ve cevap, 3 üçlü.</summary>
        public int Lines { get; set; } = 2;
        public bool AdultPack { get; set; }
        public bool Rare { get; set; }
        public bool HasTag(string t) => Tags.Contains(t);
    }

    public static class DishClasses
    {
        public static bool TryParse(string text, out DishClass c)
        {
            c = DishClass.None;
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Enum.TryParse(text.Trim(), true, out c) && Enum.IsDefined(typeof(DishClass), c);
        }

        public static string Key(DishClass c) => "cook.class." + c.ToString().ToLowerInvariant();

        /// <summary>Görünüm sınıfının varsayılan kabı ve görünümü.</summary>
        public static (string vessel, string look) DefaultServe(DishClass c)
        {
            switch (c)
            {
                case DishClass.Soup: return ("pot", "soup");
                case DishClass.Meat: return ("crock", "stew");
                case DishClass.Fish: return ("tray", "fish");
                case DishClass.Vegetable: return ("crock", "stew");
                case DishClass.Grain: return ("sini", "pilaf");
                case DishClass.Dairy: return ("bowl", "soup");
                case DishClass.Pastry: return ("tray", "pastry");
                case DishClass.Sweet: return ("bowl", "sweet");
                case DishClass.Snack: return ("bowl", "salad");
                default: return ("pot", "stew");
            }
        }
    }
}
