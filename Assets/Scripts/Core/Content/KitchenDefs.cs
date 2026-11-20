using System;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Pişirme yöntemleri (Project belgeleri 63 §2.3, 63a). Tarif yöntemini söyler; yöntem neye ihtiyaç olduğunu belirler:
    /// ateş, kap ve el aleti. Oyuncu alet seçmez; karakter gerekeni kendisi bulur ve getirir.
    /// </summary>
    public static class CookMethods
    {
        public const string Boil = "boil";     // kaynatma: tencere, kazan
        public const string Fry = "fry";       // kızartma, kavurma: sac, tava
        public const string Stew = "stew";     // güveç: güveç çömleği
        public const string Grill = "grill";   // ızgara, çevirme: şiş
        public const string Bake = "bake";     // fırın: (tepsi), kürek
        public const string Ember = "ember";   // közde: kap istemez
        public const string Raw = "raw";       // çiğ hazırlama: ateş istemez

        public static readonly string[] All = { Boil, Fry, Stew, Grill, Bake, Ember, Raw };
        public static bool IsKnown(string m) => Array.IndexOf(All, m) >= 0;

        /// <summary>Yöntemin kendiliğinden istediği kap sınıfı; fırın, köz ve çiğ hazırlamada yok (tarif ayrıca isteyebilir).</summary>
        public static string DefaultVessel(string method)
        {
            switch (method) { case Boil: case Fry: case Stew: case Grill: return method; default: return null; }
        }

        /// <summary>Mutfak ocağında pişirirken yanan yakıt, odun karşılığı (63a §3). Fırın kendi odununu tarifte ister.</summary>
        public static double Fuel(string method)
        {
            switch (method)
            {
                case Ember: return 0.25;
                case Boil: case Fry: case Grill: return 0.5;
                case Stew: return 1.0;
                default: return 0.0;
            }
        }

        /// <summary>Kap sınıfının oyuncuya görünen adının anahtarı (ui.kitchen: "tencere ya da kazan").</summary>
        public static string VesselKey(string vessel) => "kitchen.vessel." + vessel;
    }

    /// <summary>
    /// Mutfak eşyasının rolü (ItemDef.Equip): "vessel:&lt;yöntem&gt;" pişirme kabı, "tool:&lt;ad&gt;" el aleti,
    /// "tableware" sofra takımı, "basket" bulaşık sepeti.
    /// </summary>
    public static class Equip
    {
        public const string VesselPrefix = "vessel:", ToolPrefix = "tool:";
        public const string Tableware = "tableware", Basket = "basket";
        public const string Knife = "knife", Board = "board", Spoon = "spoon", Ladle = "ladle", Pin = "pin", Peel = "peel", Colander = "colander";
        public static readonly string[] Tools = { Knife, Board, Spoon, Ladle, Pin, Peel, Colander };

        /// <summary>Yoksa iş hiç yapılamayan aletler (63a-K1). Kalanı yardımcıdır: yoksa yavaş ve düşük kalite.</summary>
        public static bool IsRequiredTool(string tool) => tool == Knife || tool == Pin || tool == Peel;

        public static bool IsKnown(string equip)
        {
            if (equip == Tableware || equip == Basket) return true;
            if (equip.StartsWith(VesselPrefix, StringComparison.Ordinal)) return CookMethods.DefaultVessel(equip.Substring(VesselPrefix.Length)) != null || equip.Substring(VesselPrefix.Length) == CookMethods.Bake;
            if (equip.StartsWith(ToolPrefix, StringComparison.Ordinal)) return Array.IndexOf(Tools, equip.Substring(ToolPrefix.Length)) >= 0;
            return false;
        }

        /// <summary>El aletinin oyuncuya görünen adının anahtarı (ui.kitchen: "bıçak").</summary>
        public static string ToolKey(string tool) => "kitchen.tool." + tool;
    }

    /// <summary>Bulaşık yıkama yerleri (ItemDef.WashPlace; 63a §4.4).</summary>
    public static class WashPlaces
    {
        public const string Tub = "tub";       // evde leğen: su taşınır
        public const string Spout = "spout";   // taş yalak ve lüle: tesisattan
        public const string Stone = "stone";   // dere ya da göl kıyısındaki yıkama taşı
        public static readonly string[] All = { Tub, Spout, Stone };
        public static bool IsKnown(string w) => Array.IndexOf(All, w) >= 0;
    }
}
