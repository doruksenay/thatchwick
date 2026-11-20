using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Economy
{
    /// <summary>
    /// Şehir dükkânı (belge 18 §2, 41). Mallar (hammadde, yiyecek) Market kurallarıyla, eşyalar (mobilya, alet, dekor)
    /// BuildService.Buy ile satılır; dükkân yalnızca hangi malları ve eşyaları sattığını, hangilerini aldığını,
    /// açılış saatlerini ve sahibini belirler. Sahibi dükkânda değilse ya da dükkân kapalıysa alışveriş olmaz.
    /// </summary>
    public sealed class ShopDef
    {
        public string Id;
        public string Name;
        /// <summary>Sahibinin iş yeri anahtarı (life.json villagers[].work); sahibi bu karonun dükkânındayken açık.</summary>
        public string WorkKey;
        public int OpenHour = 9, CloseHour = 17;
        /// <summary>Haftanın kapalı günü (DayIndex % 7); -1: her gün açık.</summary>
        public int ClosedDay = -1;
        /// <summary>Malların (Market) süzgeci: bu dükkân satar ve oyuncudan alır.</summary>
        public Func<ItemDef, bool> Goods;
        /// <summary>Eşyaların (BuildService) süzgeci: bu dükkân satar.</summary>
        public Func<ItemDef, bool> Wares;
        public string Blurb;

        public bool IsOpenAt(double hour, long dayIndex)
        {
            if (ClosedDay >= 0 && dayIndex % 7 == ClosedDay) return false;
            int close = CloseHour <= OpenHour ? CloseHour + 24 : CloseHour;
            double h = hour < OpenHour && CloseHour <= OpenHour ? hour + 24 : hour;
            return h >= OpenHour && h < close;
        }

        public string HoursText =>
            $"{OpenHour:00}:00-{CloseHour % 24:00}:00" + (ClosedDay >= 0 ? $", {DayName(ClosedDay)} kapalı" : "");

        public static string DayName(int d) => d switch
        {
            0 => "Pazartesi", 1 => "Salı", 2 => "Çarşamba", 3 => "Perşembe", 4 => "Cuma", 5 => "Cumartesi", _ => "Pazar"
        };
    }

    public static class Shops
    {
        private static bool Has(ItemDef d, params string[] tags) { foreach (var t in tags) if (d.HasTag(t)) return true; return false; }
        private static bool IdHas(ItemDef d, params string[] parts) { foreach (var p in parts) if (d.Id.Contains(p)) return true; return false; }
        private static bool Cat(ItemDef d, params string[] cats) { foreach (var c in cats) if (d.Category == c) return true; return false; }
        private static bool IsSeed(ItemDef d) => d.Id.StartsWith("item.seed_") || d.Id.StartsWith("item.sapling_");

        // Uzman dükkânların mal süzgeçleri; hiçbirine girmeyen mallar genel dükkânda.
        // Giysi ve takı yalnızca etiketli dükkânda (belge 48 §9): terzi hazır olarak çok basit giysi satar.
        private static bool IsClothing(ItemDef d) => d.Category == "clothing";
        private static bool SmithGoods(ItemDef d) => Has(d, "shop_smithy") || !IsClothing(d) && (Has(d, "metal", "tool") || IdHas(d, "nail", "iron", "ore", "horseshoe", "hinge", "charcoal"));
        private static bool CarpenterGoods(ItemDef d) => !IsClothing(d) && (IdHas(d, "plank", "log", "beam", "withy", "resin", "shingle"));
        private static bool TavernGoods(ItemDef d) => !IsClothing(d) && (Has(d, "cooked", "drink"));
        private static bool MillGoods(ItemDef d) => !IsClothing(d) && (Has(d, "grain", "sheaf") || IdHas(d, "flour", "bread", "meal"));
        private static bool ScribeGoods(ItemDef d) => !IsClothing(d) && (Has(d, "writing"));
        private static bool HerbGoods(ItemDef d) => !IsClothing(d) && (Has(d, "potion") || IdHas(d, "herb", "nettle", "salve", "remedy"));
        private static bool PotterGoods(ItemDef d) => Has(d, "shop_pottery") || !IsClothing(d) && Has(d, "glass") || !IsClothing(d) && IdHas(d, "clay", "sand");
        private static bool TailorGoods(ItemDef d) => Has(d, "shop_tailor") || !IsClothing(d) && Has(d, "cloth", "fiber", "dye") || !IsClothing(d) && IdHas(d, "wool", "yarn", "linen", "flax", "thread");
        private static bool SaddlerGoods(ItemDef d) => Has(d, "shop_saddler") || !IsClothing(d) && Has(d, "leather", "hide", "saddle");
        private static bool FisherGoods(ItemDef d) => !IsClothing(d) && (Has(d, "fish") || IdHas(d, "fish", "bait", "crayfish", "eel", "mussel"));
        private static bool MasonGoods(ItemDef d) => !IsClothing(d) && (Has(d, "stone") || IdHas(d, "lime", "mortar", "plaster", "slate", "brick", "tile"));
        private static bool AnimalGoods(ItemDef d) => Has(d, "shop_pasture") || !IsClothing(d) && Has(d, "feed", "straw") || !IsClothing(d) && IdHas(d, "hay", "feed", "fodder");

        private static readonly Func<ItemDef, bool>[] Specialists =
        {
            SmithGoods, CarpenterGoods, TavernGoods, MillGoods, ScribeGoods, HerbGoods, PotterGoods, TailorGoods,
            SaddlerGoods, FisherGoods, MasonGoods, AnimalGoods
        };

        private static bool GeneralGoods(ItemDef d)
        {
            if (IsSeed(d)) return true;
            if (IsClothing(d) && !Has(d, "shop_market")) return false;   // pazar yalnızca hasır şapka
            foreach (var f in Specialists) if (f(d)) return false;
            return true;
        }

        public static readonly ShopDef[] All =
        {
            new ShopDef { Id = "general", Name = "Pazar / genel dükkân", WorkKey = "work:market", OpenHour = 8, CloseHour = 18, ClosedDay = 6,
                Blurb = "Tohum, fidan, tuz, temel yiyecek, yakıt; ev ve bahçe eşyası.",
                Goods = GeneralGoods, Wares = GeneralWares },
            new ShopDef { Id = "smithy", Name = "Demirci", WorkKey = "work:smithy", OpenHour = 7, CloseHour = 17, ClosedDay = 6,
                Blurb = "Alet, çivi, nal, menteşe, demir; hurda alır.",
                Goods = SmithGoods, Wares = d => Cat(d, "work") && IdHas(d, "anvil", "forge", "tool", "whetstone", "hammer", "bloomery") },
            new ShopDef { Id = "carpenter", Name = "Marangoz / mobilyacı", WorkKey = "work:carpenter", OpenHour = 8, CloseHour = 17, ClosedDay = 6,
                Blurb = "Mobilya setleri, raflar, yataklar, sandıklar; kalas ve kereste. Katalog siparişini ertesi sabah getirir.",
                Goods = CarpenterGoods, Wares = d => Cat(d, "seating", "surfaces", "storage", "sleep", "kids", "structure") || Cat(d, "work") && IdHas(d, "bench", "table", "rack", "press", "loom", "wheel", "churn", "quern", "sawmill", "stool") },
            new ShopDef { Id = "tavern", Name = "Meyhane / han", WorkKey = "work:tavern", OpenHour = 12, CloseHour = 24,
                Blurb = "Pişmiş yemek ve alkolsüz içecek; akşamları müzik.",
                Goods = TavernGoods, Wares = d => false },
            new ShopDef { Id = "mill", Name = "Değirmen ve fırın", WorkKey = "work:mill", OpenHour = 6, CloseHour = 15, ClosedDay = 6,
                Blurb = "Un, ekmek, yem; tahıl alır.",
                Goods = MillGoods, Wares = d => false },
            new ShopDef { Id = "scribe", Name = "Kâtip ve ulak", WorkKey = "work:scribe", OpenHour = 9, CloseHour = 16, ClosedDay = 5,
                Blurb = "Kitap, mürekkep, parşömen; soy ve miras kayıtları.",
                Goods = ScribeGoods, Wares = d => Cat(d, "memorial") || IdHas(d, "book", "hourglass", "family_tree") },
            new ShopDef { Id = "herb", Name = "Aktar ve ebe", WorkKey = "work:herb", OpenHour = 8, CloseHour = 18,
                Blurb = "İksir, merhem, ot; doğumda ebelik.",
                Goods = HerbGoods, Wares = d => IdHas(d, "herb_pot", "herb_bundle", "drying_rack", "potion_shelf", "mortar") },
            new ShopDef { Id = "potter", Name = "Çömlekçi", WorkKey = "work:pottery", OpenHour = 8, CloseHour = 17, ClosedDay = 6,
                Blurb = "Küp, testi, saksı, çini; kil alır.",
                Goods = PotterGoods, Wares = d => IdHas(d, "jar", "_pot", "pot_", "clay", "cup", "plate", "amphora", "glass", "kiln", "pottery") && !IdHas(d, "copper", "cooking", "potion", "chamber") },
            new ShopDef { Id = "tailor", Name = "Terzi ve dokumacı", WorkKey = "work:tailor", OpenHour = 9, CloseHour = 17, ClosedDay = 6,
                Blurb = "Kumaş, iplik, boya; kilim ve perde; çok basit kıyafetler. Yün ve keten alır.",
                Goods = TailorGoods, Wares = d => Cat(d, "rug") || IdHas(d, "curtain", "tapestry", "embroidery", "tablecloth", "cushion", "sewing", "yarn") },
            new ShopDef { Id = "saddler", Name = "Saraç", WorkKey = "work:saddler", OpenHour = 8, CloseHour = 17, ClosedDay = 6,
                Blurb = "Düz eyer, yular, deri; ham deri ve post alır. İşlemeli eyer yalnızca üretilir.",
                Goods = SaddlerGoods, Wares = d => IdHas(d, "saddle", "tanning", "harness", "bridle") },
            new ShopDef { Id = "fisher", Name = "Balıkçı", WorkKey = "work:fisher", OpenHour = 5, CloseHour = 14,
                Blurb = "Olta, yem, ağ, balık sepeti; balık alır.",
                Goods = FisherGoods, Wares = d => IdHas(d, "fish", "_net", "net_", "oar_rack") },
            new ShopDef { Id = "mason", Name = "Taşçı", WorkKey = "work:mason", OpenHour = 7, CloseHour = 16, ClosedDay = 6,
                Blurb = "Taş, yontma taş, tuğla, kiremit, kireç; ocak ve fırın.",
                Goods = MasonGoods, Wares = d => Cat(d, "heat_light", "work", "structure") && IdHas(d, "hearth", "oven", "stone", "lime_kiln", "stonecutter") && !IdHas(d, "stair") },
            new ShopDef { Id = "animals", Name = "Hayvan pazarı", WorkKey = "work:pasture", OpenHour = 6, CloseHour = 13,
                Blurb = "Yem, saman; ahır ve ağıl eşyaları. (Hayvan alım-satımı hayvan sisteminden.)",
                Goods = AnimalGoods, Wares = d => Cat(d, "barn") },
        };

        // Genel dükkân: ev, bahçe, ışık eşyası ve uzman dükkânların satmadığı dekor.
        private static bool GeneralWares(ItemDef d)
        {
            if (Cat(d, "hygiene", "heat_light", "garden", "farm", "utility", "ceiling", "kitchen")) return true;
            if (!Cat(d, "decor", "decor_wall", "work")) return false;
            foreach (var s in All) if (s.Id != "general" && s.Wares != null && s.Wares(d)) return false;
            return true;
        }

        public static ShopDef Get(string id) { foreach (var s in All) if (s.Id == id) return s; return null; }
    }
}
