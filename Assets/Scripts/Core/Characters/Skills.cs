using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Characters
{
    /// <summary>İlk prototip becerileri. Beceriler çalışarak öğrenilir (kitapla değil).</summary>
    public enum SkillType
    {
        Farming = 0,          // Çiftçilik: ekin, tarla, toplayıcılık
        Woodcutting = 1,      // Odunculuk
        Cooking = 2,          // Yemek: pişirme, kurutma, turşu, tütsü
        Carpentry = 3,        // Marangozluk: mobilya, tahta
        AnimalHusbandry = 4,  // Hayvancılık: bakım, sağım, kırkım, yavru
        Riding = 5,           // Binicilik
        Fishing = 6,          // Balıkçılık
        Leatherwork = 7,      // Saraçlık (zanaat): deri, eyer
        Weaving = 8,          // Dokumacılık (zanaat): yün, iplik, kumaş, halı
        Trade = 9,            // Ticaret ve pazarlık
        Herbalism = 10,       // Otacılık: iksir, merhem
        // 17.11.2026 (ikinci dalga)
        Engineering = 11,     // Mühendislik: düzenekler, dişli, bakım
        Salvage = 12,         // Geri kazanım: sökülen eşyadan malzeme
        Masonry = 13,         // Taş ustalığı: yol onarımı, yolların dayanıklılığı
        Mining = 14,          // Madencilik: taş ocağı, kil, cevher
        Smithing = 15,        // Demircilik: demir, çivi, nal, alet yükseltme
        Pottery = 16,         // Çömlekçilik: küp, testi
        Brewing = 17,         // Biracılık ve şarapçılık
        Baking = 18,          // Fırıncılık: un, ekmek
        Chandlery = 19,       // Mumculuk: balmumu, mum
        Horticulture = 20,    // Bahçıvanlık: meyve ağacı, tohum seçimi
        Foraging = 21,        // Toplayıcılık
        Hunting = 22,         // Avcılık
        WeatherLore = 23,     // Hava bilgisi: kaç gün ilerisini görür
        SoilCare = 24,        // Toprak bakımı: kompost, gübre, nadas
        Parenting = 25,       // Ebeveynlik: bebek bakımı
        Teaching = 26,        // Öğretmenlik: çocuklara ders
        Literacy = 27,        // Okuryazarlık: bütün becerilerde fazla XP
        Charm = 28,           // Gönül kazanma: sohbetle yakınlık
        Music = 29,           // Müzik
        Stewardship = 30,     // Kâhyalık: görevlilerin verimi
        Carting = 31,         // Arabacılık / taşımacılık: depo kapasitesi
        Decorating = 32,      // Ev düzenleme: eşya ve yapı indirimi
        Medicine = 33         // Hekimlik: soğuk ve açlık yarasını iyileştirir
    }

    /// <summary>Becerilerin görünen adları (zanaatler alt başlıklarıyla ayrı beceridir).</summary>
    public static class SkillNames
    {
        public static string Name(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: return "Çiftçilik";
                case SkillType.Woodcutting: return "Odunculuk";
                case SkillType.Cooking: return "Yemek";
                case SkillType.Carpentry: return "Marangozluk";
                case SkillType.AnimalHusbandry: return "Hayvancılık";
                case SkillType.Riding: return "Binicilik";
                case SkillType.Fishing: return "Balıkçılık";
                case SkillType.Leatherwork: return "Saraçlık";
                case SkillType.Weaving: return "Dokumacılık";
                case SkillType.Trade: return "Ticaret";
                case SkillType.Herbalism: return "Otacılık";
                case SkillType.Engineering: return "Mühendislik";
                case SkillType.Salvage: return "Geri kazanım";
                case SkillType.Masonry: return "Taş ustalığı";
                case SkillType.Mining: return "Madencilik";
                case SkillType.Smithing: return "Demircilik";
                case SkillType.Pottery: return "Çömlekçilik";
                case SkillType.Brewing: return "Biracılık";
                case SkillType.Baking: return "Fırıncılık";
                case SkillType.Chandlery: return "Mumculuk";
                case SkillType.Horticulture: return "Bahçıvanlık";
                case SkillType.Foraging: return "Toplayıcılık";
                case SkillType.Hunting: return "Avcılık";
                case SkillType.WeatherLore: return "Hava bilgisi";
                case SkillType.SoilCare: return "Toprak bakımı";
                case SkillType.Parenting: return "Ebeveynlik";
                case SkillType.Teaching: return "Öğretmenlik";
                case SkillType.Literacy: return "Okuryazarlık";
                case SkillType.Charm: return "Gönül kazanma";
                case SkillType.Music: return "Müzik";
                case SkillType.Stewardship: return "Kâhyalık";
                case SkillType.Carting: return "Arabacılık";
                case SkillType.Decorating: return "Ev düzenleme";
                case SkillType.Medicine: return "Hekimlik";
                default: return s.ToString();
            }
        }

        /// <summary>Zanaat alt başlığı mı (kasabadaki ustalarla ilişkili)?</summary>
        public static bool IsCraft(SkillType s) => s == SkillType.Carpentry || s == SkillType.Leatherwork || s == SkillType.Weaving
            || s == SkillType.Smithing || s == SkillType.Pottery || s == SkillType.Chandlery || s == SkillType.Masonry;

        /// <summary>
        /// Bu beceriyle yapılan işte ilk üründen seviye başına bir fazlası çıkma şansı (toplayıcılık %4, maden %4, av %3…);
        /// 0 = etkisiz. 10. seviyede %36'ya kadar.
        /// </summary>
        public static double ExtraYieldPerLevel(SkillType s)
        {
            switch (s)
            {
                case SkillType.Foraging:
                case SkillType.Horticulture:
                case SkillType.Mining: return 0.04;
                case SkillType.Hunting:
                case SkillType.Brewing:
                case SkillType.Baking:
                case SkillType.Chandlery: return 0.03;
                case SkillType.Pottery:
                case SkillType.Smithing: return 0.02;
                default: return 0.0;
            }
        }

        /// <summary>Seviye başına yüzde etkisi olan beceriler için ortak çarpan: 1 + oran x (seviye - 1).</summary>
        public static double Factor(int level, double perLevel) => 1.0 + perLevel * Math.Max(0, level - 1);

        /// <summary>Meslek adı (hanedan geçmişi, köy).</summary>
        public static string TradeName(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: return "Çiftçi";
                case SkillType.Woodcutting: return "Oduncu";
                case SkillType.Cooking: return "Aşçı";
                case SkillType.Carpentry: return "Marangoz";
                case SkillType.AnimalHusbandry: return "Hayvancı";
                case SkillType.Riding: return "Binici";
                case SkillType.Fishing: return "Balıkçı";
                case SkillType.Leatherwork: return "Saraç";
                case SkillType.Weaving: return "Dokumacı";
                case SkillType.Trade: return "Tüccar";
                case SkillType.Herbalism: return "Otacı";
                case SkillType.Engineering: return "Mühendis";
                case SkillType.Salvage: return "Hurdacı";
                case SkillType.Masonry: return "Taş ustası";
                case SkillType.Mining: return "Madenci";
                case SkillType.Smithing: return "Demirci";
                case SkillType.Pottery: return "Çömlekçi";
                case SkillType.Brewing: return "Biracı";
                case SkillType.Baking: return "Fırıncı";
                case SkillType.Chandlery: return "Mumcu";
                case SkillType.Horticulture: return "Bahçıvan";
                case SkillType.Foraging: return "Toplayıcı";
                case SkillType.Hunting: return "Avcı";
                case SkillType.WeatherLore: return "Hava bilgesi";
                case SkillType.SoilCare: return "Toprak bilgesi";
                case SkillType.Parenting: return "Ebeveyn";
                case SkillType.Teaching: return "Hoca";
                case SkillType.Literacy: return "Kâtip";
                case SkillType.Charm: return "Gönül ehli";
                case SkillType.Music: return "Çalgıcı";
                case SkillType.Stewardship: return "Kâhya";
                case SkillType.Carting: return "Arabacı";
                case SkillType.Decorating: return "Süslemeci";
                case SkillType.Medicine: return "Hekim";
                default: return "Köylü";
            }
        }
    }

    /// <summary>Seviye eğrisi ve yaş evresine göre seviye tavanı.</summary>
    public static class SkillRules
    {
        public const int MaxLevel = 10;

        /// <summary>Seviye L'ye ulaşmak için gereken toplam XP: 50 * (L-1)^2 (L1=0, L2=50, L3=200 ... L10=4050).</summary>
        public static double XpForLevel(int level)
        {
            if (level < 1) level = 1;
            if (level > MaxLevel) level = MaxLevel;
            int n = level - 1;
            return 50.0 * n * n;
        }

        public static int LevelForXp(double xp)
        {
            int level = 1;
            while (level < MaxLevel && xp >= XpForLevel(level + 1)) level++;
            return level;
        }

        /// <summary>
        /// Evreye göre ulaşılabilecek en yüksek seviye. Bebek öğrenemez (0).
        /// Varsayım (denge turunda ayarlanır): Çocuk 3, Genç 7, Yetişkin ve Yaşlı 10.
        /// </summary>
        public static int LevelCap(AgeStage stage)
        {
            switch (stage)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return 3;
                case AgeStage.Youth: return 7;
                default: return MaxLevel;
            }
        }
    }

    /// <summary>Bir karakterin becerileri: XP tutar, seviyeyi XP'den türetir.</summary>
    public sealed class SkillSet
    {
        private readonly Dictionary<SkillType, double> _xp = new Dictionary<SkillType, double>();

        /// <summary>(beceri, eski seviye, yeni seviye)</summary>
        public event Action<SkillType, int, int> LevelChanged;

        public double GetXp(SkillType skill) => _xp.TryGetValue(skill, out var v) ? v : 0.0;

        public int GetLevel(SkillType skill) => SkillRules.LevelForXp(GetXp(skill));

        /// <summary>Kayıttan yükleme: XP'yi olduğu gibi koyar (olay tetiklemez).</summary>
        internal void RestoreXp(SkillType skill, double xp)
        {
            if (xp > 0) _xp[skill] = xp; else _xp.Remove(skill);
        }

        /// <summary>
        /// XP ekler; seviye tavanını aşan kısım kaybolur (XP tavan seviyenin sınırında durur).
        /// Eklenen gerçek XP'yi döner (tavan yüzünden 0 olabilir).
        /// </summary>
        public double AddXp(SkillType skill, double xp, int levelCap)
        {
            if (xp <= 0 || levelCap <= 0) return 0;
            if (levelCap > SkillRules.MaxLevel) levelCap = SkillRules.MaxLevel;

            double current = GetXp(skill);
            // Tavan seviyenin bir sonraki seviyeye çok az kala sınırı: sonraki eşiğin hemen altı.
            double ceiling = levelCap >= SkillRules.MaxLevel
                ? SkillRules.XpForLevel(SkillRules.MaxLevel) + 1000.0 // son seviyede XP birikmeye devam eder ama seviye artmaz
                : SkillRules.XpForLevel(levelCap + 1) - 0.0001;
            if (current >= ceiling) return 0;

            int before = SkillRules.LevelForXp(current);
            double next = Math.Min(current + xp, ceiling);
            _xp[skill] = next;
            int after = SkillRules.LevelForXp(next);
            if (after != before) LevelChanged?.Invoke(skill, before, after);
            return next - current;
        }
    }
}
