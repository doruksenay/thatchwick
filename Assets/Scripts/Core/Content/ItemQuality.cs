using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Eşyanın kalite taşıyıp taşımadığı (Project belgesi 53 §4.2).</summary>
    public enum ItemQualityClass
    {
        /// <summary>Ham madde, yakıt, tohum, yapı malzemesi: kalitesiz (hep Sıradan).</summary>
        None = 0,
        /// <summary>Yiyecek, içecek, hayvan ürünü, tekstil ve deri zinciri: hasat, toplama ve işlemede kalite alır.</summary>
        Goods = 1,
        /// <summary>Tarifle yapılan eşya (mobilya, çömlek, eyer, oyuncak): yapanın becerisiyle kalite alır.</summary>
        Crafted = 2
    }

    /// <summary>Harcama sırası (53 §4.7).</summary>
    public enum TakePolicy
    {
        /// <summary>En düşük kalite önce (tarif, yeme, azık, toplu satış).</summary>
        Lowest = 0,
        /// <summary>En iyi kalite önce ("En iyi malzemeyi kullan", hediye, yerleştirme).</summary>
        Highest = 1
    }

    /// <summary>
    /// Hane envanterinde bir yığının anahtarı: kimlik + kalite + durum + pazardan alınmış mı + usta (ustalık eseri ve yadigârda
    /// yapanın kimliği; diğerlerinde 0). 53 §5.1.
    /// </summary>
    public readonly struct ItemKey : IEquatable<ItemKey>
    {
        public readonly string Id;
        public readonly int Quality;
        public readonly ItemCondition Condition;
        public readonly bool Bought;
        public readonly int Maker;

        public ItemKey(string id, int quality = 0, ItemCondition condition = ItemCondition.Sound, bool bought = false, int maker = 0)
        {
            Id = id;
            Quality = ItemQuality.Clamp(quality);
            Condition = condition;
            Bought = bought;
            Maker = Math.Max(0, maker);
        }

        public ItemKey WithQuality(int q) => new ItemKey(Id, q, Condition, Bought, Maker);
        public ItemKey WithCondition(ItemCondition c) => new ItemKey(Id, Quality, c, Bought, Maker);
        public ItemKey WithBought(bool b) => new ItemKey(Id, Quality, Condition, b, Maker);

        public bool Equals(ItemKey o) => string.Equals(Id, o.Id, StringComparison.Ordinal) && Quality == o.Quality && Condition == o.Condition && Bought == o.Bought && Maker == o.Maker;
        public override bool Equals(object obj) => obj is ItemKey k && Equals(k);
        public override int GetHashCode()
        {
            unchecked
            {
                int h = Id != null ? StringComparer.Ordinal.GetHashCode(Id) : 0;
                h = h * 31 + Quality;
                h = h * 31 + (int)Condition;
                h = h * 31 + (Bought ? 1 : 0);
                return h * 31 + Maker;
            }
        }
        public override string ToString() => $"{Id}#{Quality}/{Condition}{(Bought ? "/b" : "")}{(Maker != 0 ? "/m" + Maker : "")}";

        /// <summary>
        /// Barınak yığını (<c>ItemInstance.Stock</c>) gibi dizgi anahtarlı depolar için: kalite 0 ise yalnızca kimlik, değilse
        /// "kimlik#kalite". Eski kayıtlar böylece değişmeden okunur.
        /// </summary>
        public static string StockKey(string id, int quality) => quality <= 0 ? id : id + "#" + ItemQuality.Clamp(quality);

        /// <summary>
        /// Anahtarın tam dizgisi (azık gibi dizgi anahtarlı depolar ve kayıt): kimlik, sonra yalnızca varsayılan olmayan
        /// alanlar: "#kalite", "!" (pazardan), "^durum", "~eser". Sıradan, sağlam, kendi ürünü eşyada yalnızca kimlik;
        /// eski kayıtlardaki düz kimlik böylece değişmeden okunur.
        /// </summary>
        public string Token()
        {
            var s = Id ?? "";
            if (Quality > 0) s += "#" + Quality;
            if (Bought) s += "!";
            if (Condition != ItemCondition.Sound) s += "^" + (int)Condition;
            if (Maker > 0) s += "~" + Maker;
            return s;
        }

        public static ItemKey FromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return default;
            int cut = token.IndexOfAny(new[] { '#', '!', '^', '~' });
            if (cut <= 0) return new ItemKey(token);
            string id = token.Substring(0, cut);
            int q = 0, m = 0; bool b = false; var c = ItemCondition.Sound;
            int i = cut;
            while (i < token.Length)
            {
                char tag = token[i++];
                int start = i;
                while (i < token.Length && char.IsDigit(token[i])) i++;
                int.TryParse(token.Substring(start, i - start), out int n);
                switch (tag)
                {
                    case '#': q = n; break;
                    case '!': b = true; break;
                    case '^': c = Enum.IsDefined(typeof(ItemCondition), n) ? (ItemCondition)n : ItemCondition.Sound; break;
                    case '~': m = n; break;
                }
            }
            return new ItemKey(id, q, c, b, m);
        }

        public static ItemKey FromStockKey(string key)
        {
            if (key == null) return default;
            int hash = key.LastIndexOf('#');
            if (hash > 0 && hash < key.Length - 1 && int.TryParse(key.Substring(hash + 1), out int q))
                return new ItemKey(key.Substring(0, hash), q);
            return new ItemKey(key);
        }
    }

    /// <summary>
    /// Kalite basamakları ve kuralları (Project belgesi 53, kullanıcı kararları 20.11.2026). Beş renkli düzen:
    /// Sıradan (gri, rozetsiz) · Seçme (yeşil ★) · Has (mavi ★★) · Ustalık eseri (mor ✦) · Yadigâr (altın ✦).
    /// Kalite kelimesi eşyanın adına eklenmez; rozet, renk ve ipucuyla görünür.
    /// </summary>
    public static class ItemQuality
    {
        public const int Common = 0, Fine = 1, Superior = 2, Masterwork = 3, Heirloom = 4;
        /// <summary>Olağan üretimin tavanı (Has). Üstü yalnızca "Ustalık eseri dene" ve kuşak kuralıyla.</summary>
        public const int NormalMax = Superior;
        public const int Max = Heirloom;

        public static int Clamp(int q) => q < Common ? Common : q > Max ? Max : q;

        /// <summary>Basamak adının ui anahtarı: "ui.quality.0" .. "ui.quality.4".</summary>
        public static string NameKey(int q) => "ui.quality." + Clamp(q);
        /// <summary>Rozet simgesi (yazı yedeği; PixelLab rozetleri gelince ikon).</summary>
        public static string Badge(int q) => q == Fine ? "★" : q == Superior ? "★★" : q >= Masterwork ? "✦" : "";
        /// <summary>Rozet ve ad rengi (RGB, 0-1): gri, yeşil, mavi, mor, altın.</summary>
        public static (float r, float g, float b) Color(int q)
        {
            switch (Clamp(q))
            {
                case Fine: return (0.45f, 0.78f, 0.36f);
                case Superior: return (0.36f, 0.6f, 0.95f);
                case Masterwork: return (0.72f, 0.45f, 0.92f);
                case Heirloom: return (0.95f, 0.76f, 0.25f);
                default: return (0.78f, 0.78f, 0.78f);
            }
        }
        /// <summary>Rozet ikonu (Resources/UI/Icons/ui_icon_quality_*.png, PixelLab işi); Sıradan'da yok.</summary>
        public static string IconKey(int q) => q <= Common ? null : "quality_" + Clamp(q);

        private static double Pick(int q, double c, double f, double s, double m, double h)
        {
            switch (Clamp(q)) { case Fine: return f; case Superior: return s; case Masterwork: return m; case Heirloom: return h; default: return c; }
        }

        /// <summary>Pazar satış fiyatı çarpanı (53 §4.5): ×1 / 1,25 / 1,5 / 3 / 4.</summary>
        public static double PriceFactor(int q) => Pick(q, 1.0, 1.25, 1.5, 3.0, 4.0);
        /// <summary>Yeme: açlık doyumu çarpanı.</summary>
        public static double SatietyFactor(int q) => Pick(q, 1.0, 1.1, 1.25, 1.4, 1.5);
        /// <summary>Yemek etkisinin (effect.*, moodlet) süre çarpanı.</summary>
        public static double EffectFactor(int q) => Pick(q, 1.0, 1.25, 1.5, 2.0, 2.0);
        /// <summary>Sevilen ya da beğenilen hediyede yakınlık çarpanı (sevilmeyende etkisiz).</summary>
        public static double GiftFactor(int q) => Pick(q, 1.0, 1.25, 1.5, 2.0, 2.0);
        /// <summary>Yapılan eşya: güzellik çarpanı.</summary>
        public static double BeautyFactor(int q) => Pick(q, 1.0, 1.15, 1.3, 1.5, 1.6);
        /// <summary>Yapılan eşya: konfor çarpanı.</summary>
        public static double ComfortFactor(int q) => Pick(q, 1.0, 1.1, 1.2, 1.3, 1.35);
        /// <summary>Yapılan eşya: hasar şansı çarpanı (raf çökmesi, fırtına).</summary>
        public static double DamageFactor(int q) => Pick(q, 1.0, 0.75, 0.5, 0.4, 0.3);
        /// <summary>Ekin ve hayvan ürününde kalan fazladan adet (karar 2: yarıya iner, +%10 / +%20).</summary>
        public static double CountBonus(int q) => q >= Superior ? 0.2 : q == Fine ? 0.1 : 0.0;
        /// <summary>Sipariş ödülü (46 §11): akçe çarpanı ve itibar.</summary>
        public static double OrderAkceFactor(int q) => Pick(q, 1.0, 1.5, 2.0, 3.0, 3.0);
        public static int OrderReputation(int q) => q >= Masterwork ? 6 : q == Superior ? 4 : q == Fine ? 2 : 0;
        /// <summary>Yeme ruh hali (yalnızca pişmiş yemek): kimlik, değer. Sıradan'da yok.</summary>
        public static string MealMood(int q) =>
            q >= Heirloom ? "meal_heirloom" : q == Masterwork ? "meal_masterwork" : q == Superior ? "meal_superior" : q == Fine ? "meal_fine" : null;

        // ---- kalite kaynakları ----

        /// <summary>
        /// Puan basamağından birim dağılımı (53 §4.3): basamak %70, bir altı %20, bir üstü %10; [0, Has] içinde.
        /// </summary>
        public static int Spread(int tier, Random rng)
        {
            tier = Math.Max(Common, Math.Min(NormalMax, tier));
            double r = rng.NextDouble();
            int q = r < 0.7 ? tier : r < 0.9 ? tier - 1 : tier + 1;
            return Math.Max(Common, Math.Min(NormalMax, q));
        }

        /// <summary>
        /// Beceriyle toplanan (toplama, balık, av, kırkım, bal): Sv 1-2 hep Sıradan; Sv 5 %70/%25/%5; Sv 10 %30/%45/%25.
        /// Aradaki seviyeler doğrusal.
        /// </summary>
        public static int Gathered(int level, Random rng)
        {
            level = Math.Max(1, Math.Min(10, level));
            double fine, sup;
            if (level <= 2) { fine = 0; sup = 0; }
            else if (level <= 5) { double t = (level - 2) / 3.0; fine = 0.25 * t; sup = 0.05 * t; }
            else { double t = (level - 5) / 5.0; fine = 0.25 + 0.20 * t; sup = 0.05 + 0.20 * t; }
            double r = rng.NextDouble();
            return r < sup ? Superior : r < sup + fine ? Fine : Common;
        }

        /// <summary>
        /// Toplayanın becerisi (kim toplarsa onun becerisi, karar 4): ürünün kalitesine seviye 6'da %25, 9'da %50 şansla +1 (en çok Has).
        /// </summary>
        public static int CollectorBonus(int baseQuality, int level, Random rng)
        {
            double chance = level >= 9 ? 0.5 : level >= 6 ? 0.25 : 0.0;
            if (chance > 0 && baseQuality < NormalMax && rng.NextDouble() < chance) return baseQuality + 1;
            return baseQuality;
        }

        /// <summary>
        /// İşleme (53 §4.4): kaliteli girdilerin adetle ağırlıklı ortalaması (aşağı yuvarlanır) taban; çalışan tarif kapısının
        /// 4 üstündeyse %25, 7 üstündeyse %50 şansla +1; tavan en iyi girdinin bir üstü, en çok Has. Kalite taşıyan girdi yoksa
        /// <see cref="Crafted"/> kuralı.
        /// </summary>
        public static int Processed(IReadOnlyList<int> inputQualities, int levelAboveGate, int skillLevel, Random rng)
        {
            if (inputQualities == null || inputQualities.Count == 0) return Crafted(levelAboveGate, skillLevel, rng);
            int sum = 0, best = 0;
            foreach (var q in inputQualities) { int c = Math.Min(NormalMax, q); sum += c; if (c > best) best = c; }
            int baseQ = sum / inputQualities.Count;
            double chance = levelAboveGate >= 7 ? 0.5 : levelAboveGate >= 4 ? 0.25 : 0.0;
            if (chance > 0 && rng.NextDouble() < chance) baseQ++;
            return Math.Max(Common, Math.Min(Math.Min(NormalMax, best + 1), baseQ));
        }

        /// <summary>
        /// Girdisi kalitesiz tarif (53 §4.6): kapının üstü +0-2 %85/%15/0, +3-5 %50/%40/%10, +6 ve üstü %15/%55/%30;
        /// Sv 10 taban Seçme (%60/%40).
        /// </summary>
        public static int Crafted(int levelAboveGate, int skillLevel, Random rng)
        {
            double r = rng.NextDouble();
            if (skillLevel >= 10) return r < 0.4 ? Superior : Fine;
            if (levelAboveGate >= 6) return r < 0.3 ? Superior : r < 0.85 ? Fine : Common;
            if (levelAboveGate >= 3) return r < 0.1 ? Superior : r < 0.5 ? Fine : Common;
            return r < 0.15 ? Fine : Common;
        }

        /// <summary>
        /// "Ustalık eseri dene" (karar 1): usta Sv 10, kaliteli girdilerin hepsi Has (girdi kalitesizse yalnızca usta şartı),
        /// %60 başarı; olmazsa Has.
        /// </summary>
        public const double MasterworkChance = 0.6;
        public static bool CanAttemptMasterwork(int skillLevel, IReadOnlyList<int> inputQualities)
        {
            if (skillLevel < 10) return false;
            if (inputQualities != null) foreach (var q in inputQualities) if (q < Superior) return false;
            return true;
        }

        /// <summary>
        /// Olgunlaşma (karar 4): mahzende bekleyen ürün, beklediği gün arttıkça basamak atlar. <paramref name="stepDays"/> gün
        /// başına +1, en çok Has. Süre uzadıkça kalite artar.
        /// </summary>
        public static int Aged(int baseQuality, double daysAged, double stepDays)
        {
            if (stepDays <= 0 || daysAged <= 0) return baseQuality;
            int steps = (int)Math.Floor(daysAged / stepDays);
            return Math.Max(baseQuality, Math.Min(NormalMax, baseQuality + steps));
        }

        /// <summary>Bir kuşak (oyun günü; 1 gün ≈ yarım yıl, kuşak ≈ 25 yıl). Dede şarabı ve yadigâr için.</summary>
        public const double GenerationDays = 50.0;
        /// <summary>Yadigâr için gereken kuşak (karar 1): ailede 3 kuşak.</summary>
        public const int HeirloomGenerations = 3;

        // ---- eski ayrı kimlikler (karar 3) ----

        /// <summary>Kaliteye dönen eski kimlikler: eski kimlik → (yeni kimlik, kalite). Kayıt yüklenirken çevrilir.</summary>
        public static readonly IReadOnlyDictionary<string, (string id, int quality)> Aliases = new Dictionary<string, (string, int)>(StringComparer.Ordinal)
        {
            ["item.stew_good"] = ("item.stew_plain", Fine),
            ["item.stew_fine"] = ("item.stew_plain", Superior),
            ["item.soup_pea_good"] = ("item.soup_pea", Fine),
            ["item.coffee_roasted_fine"] = ("item.coffee_roasted", Superior),
            ["item.coffee_ground_fine"] = ("item.coffee_ground", Superior),
            ["item.olive_fine"] = ("item.olive", Fine),
            ["item.wine_aged"] = ("item.wine_new", Superior),
        };

        /// <summary>Eski kimliği yenisine çevirir; değilse olduğu gibi (kalite 0).</summary>
        public static (string id, int quality) Resolve(string id) =>
            id != null && Aliases.TryGetValue(id, out var a) ? a : (id, Common);
    }
}
