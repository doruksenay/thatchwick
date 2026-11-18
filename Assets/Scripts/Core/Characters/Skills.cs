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

        /// <summary>
        /// Dal düzeni (karar 18.11.2026, Project belgesi 28): 14 ana beceride tam dal (5. seviyede 2, 10. seviyede 4),
        /// 16 becerinin yalnızca 5. seviyede 2 dalı, 4 destek becerisi dalsız. Toplam 116. life.json bununla uyuşmalıdır.
        /// </summary>
        public static int BranchCount(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: case SkillType.AnimalHusbandry: case SkillType.Cooking: case SkillType.Woodcutting:
                case SkillType.Carpentry: case SkillType.Mining: case SkillType.Smithing: case SkillType.Weaving:
                case SkillType.Leatherwork: case SkillType.Pottery: case SkillType.Fishing: case SkillType.Herbalism:
                case SkillType.Trade: case SkillType.Engineering:
                    return 6;
                case SkillType.WeatherLore: case SkillType.Literacy: case SkillType.Carting: case SkillType.Decorating:
                    return 0;
                default:
                    return 2;
            }
        }

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
        /// Varsayım (denge turunda ayarlanır): Çocuk 3, Ergen 7, Genç yetişkin 9, Yetişkin ve Yaşlı 10.
        /// </summary>
        public static int LevelCap(AgeStage stage)
        {
            switch (stage)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return 3;
                case AgeStage.Teen: return 7;
                case AgeStage.YoungAdult: return 9;
                default: return MaxLevel;
            }
        }
    }

    /// <summary>Yatkınlık (karar 18.11.2026, Project belgesi 28): doğuştan ya da çocukluktan gelen hız.</summary>
    public enum Aptitude
    {
        None = 0,
        /// <summary>Yatkın: XP x1,5.</summary>
        Minor = 1,
        /// <summary>Çok yatkın: XP x2.</summary>
        Major = 2
    }

    /// <summary>
    /// Beceri temposu (karar 18.11.2026, Project belgesi 28): yatkınlık, günlük XP doygunluğu ve yaşlılıkta gerileme.
    /// Seviye asla düşmez; gerileme yalnızca seviye içindeki ilerlemeyi azaltır.
    /// </summary>
    public static class SkillPacing
    {
        /// <summary>Bir karakterde en çok yatkınlık sayısı.</summary>
        public const int MaxAptitudes = 3;
        /// <summary>Günlük eşik: o seviyenin XP aralığının bu oranı (yaklaşık yarım seviye/gün tam hızla).</summary>
        public const double DailyThresholdShare = 0.40;
        /// <summary>Yatkın becerilerde eşik çarpanı.</summary>
        public const double AptitudeThresholdFactor = 1.5;
        /// <summary>Eşikten sonra kazanılan XP oranı.</summary>
        public const double SaturatedRate = 0.25;
        /// <summary>Yaşlılığa girişte bir kereye mahsus kaybedilen seviye içi ilerleme oranı.</summary>
        public const double ElderOnsetLoss = 0.10;
        /// <summary>Yaşlıda bütün mevsim hiç kullanılmayan becerinin mevsim sonunda kaybettiği seviye içi ilerleme oranı.</summary>
        public const double ElderSeasonLoss = 0.25;

        public static double XpMultiplier(Aptitude a) => a == Aptitude.Major ? 2.0 : a == Aptitude.Minor ? 1.5 : 1.0;

        public static string AptitudeName(Aptitude a) => a == Aptitude.Major ? "Çok yatkın" : a == Aptitude.Minor ? "Yatkın" : "";

        /// <summary>Seviyenin XP aralığı (son seviyede bir önceki aralık kullanılır).</summary>
        public static double LevelSpan(int level)
        {
            if (level >= SkillRules.MaxLevel) return SkillRules.XpForLevel(SkillRules.MaxLevel) - SkillRules.XpForLevel(SkillRules.MaxLevel - 1);
            return SkillRules.XpForLevel(level + 1) - SkillRules.XpForLevel(level);
        }

        /// <summary>Günlük doygunluk eşiği (ham XP).</summary>
        public static double DailyThreshold(int level, Aptitude a)
        {
            double t = LevelSpan(level) * DailyThresholdShare;
            return a != Aptitude.None ? t * AptitudeThresholdFactor : t;
        }
    }

    /// <summary>Bir karakterin becerileri: XP tutar, seviyeyi XP'den türetir.</summary>
    public sealed class SkillSet
    {
        private readonly Dictionary<SkillType, double> _xp = new Dictionary<SkillType, double>();
        private readonly Dictionary<SkillType, Aptitude> _aptitudes = new Dictionary<SkillType, Aptitude>();
        private readonly Dictionary<SkillType, double> _today = new Dictionary<SkillType, double>();
        private readonly HashSet<SkillType> _usedThisSeason = new HashSet<SkillType>();

        /// <summary>
        /// Günlük doygunluk ve yaşlılık gerilemesi açık mı. Oyunda hane üyeleri için açılır (LifeSystems);
        /// köylüler ve eski testler etkilenmesin diye varsayılan kapalı (MoodEnabled gibi).
        /// </summary>
        public bool PacingEnabled { get; set; }
        /// <summary>Yaşlılığa giriş kaybı uygulandı mı (bir kereye mahsus).</summary>
        public bool ElderOnsetApplied { get; set; }

        // ---- yatkınlık ----

        public Aptitude GetAptitude(SkillType skill) => _aptitudes.TryGetValue(skill, out var a) ? a : Aptitude.None;

        public IEnumerable<KeyValuePair<SkillType, Aptitude>> Aptitudes => _aptitudes;

        public int AptitudeCount => _aptitudes.Count;

        /// <summary>
        /// Yatkınlık verir. Yeni beceri en çok 3 taneye kadar eklenir; var olanı yalnızca yükseltir (düşürmez).
        /// Değişiklik olduysa true.
        /// </summary>
        public bool SetAptitude(SkillType skill, Aptitude a)
        {
            if (a == Aptitude.None) return _aptitudes.Remove(skill);
            if (_aptitudes.TryGetValue(skill, out var cur))
            {
                if (cur >= a) return false;
                _aptitudes[skill] = a;
                return true;
            }
            if (_aptitudes.Count >= SkillPacing.MaxAptitudes) return false;
            _aptitudes[skill] = a;
            return true;
        }

        // ---- günlük doygunluk ve mevsim kullanımı ----

        /// <summary>Bugün bu beceride kazanılan (doygunluk öncesi) XP.</summary>
        public double TodayXp(SkillType skill) => _today.TryGetValue(skill, out var v) ? v : 0.0;

        /// <summary>Bugün eşik doldu mu ("bugün yoruldu" göstergesi).</summary>
        public bool IsSaturated(SkillType skill) => PacingEnabled && TodayXp(skill) >= SkillPacing.DailyThreshold(GetLevel(skill), GetAptitude(skill));

        public bool UsedThisSeason(SkillType skill) => _usedThisSeason.Contains(skill);

        public IEnumerable<KeyValuePair<SkillType, double>> TodayAll => _today;
        public IEnumerable<SkillType> UsedSeasonAll => _usedThisSeason;

        internal void RestoreToday(SkillType skill, double xp) { if (xp > 0) _today[skill] = xp; }
        internal void RestoreUsed(SkillType skill) { _usedThisSeason.Add(skill); }

        /// <summary>Gece: günlük sayaçlar sıfırlanır.</summary>
        public void NewDay() => _today.Clear();

        /// <summary>
        /// Ham XP'yi doygunluk eşiğine göre kırpar ve sayaca işler (kapalıysa olduğu gibi döner). Kullanım kaydını tutar.
        /// </summary>
        public double ApplyDailySaturation(SkillType skill, double xp)
        {
            if (xp <= 0) return xp;
            _usedThisSeason.Add(skill);
            if (!PacingEnabled) return xp;
            double threshold = SkillPacing.DailyThreshold(GetLevel(skill), GetAptitude(skill));
            double before = TodayXp(skill);
            _today[skill] = before + xp;
            double full = Math.Max(0.0, Math.Min(xp, threshold - before));
            return full + (xp - full) * SkillPacing.SaturatedRate;
        }

        // ---- yaşlılık gerilemesi ----

        /// <summary>Seviye içi ilerlemeyi bu oranda azaltır; seviye düşmez. Kaybedilen XP'yi döner.</summary>
        public double LoseLevelProgress(SkillType skill, double share)
        {
            double xp = GetXp(skill);
            if (xp <= 0 || share <= 0) return 0;
            int level = SkillRules.LevelForXp(xp);
            double floor = SkillRules.XpForLevel(level);
            double progress = xp - floor;
            if (progress <= 0) return 0;
            double loss = progress * Math.Min(1.0, share);
            _xp[skill] = xp - loss;
            return loss;
        }

        /// <summary>Yaşlılığa giriş: bütün becerilerde seviye içi ilerlemenin %10'u gider (bir kez).</summary>
        public double ApplyElderOnset()
        {
            if (!PacingEnabled || ElderOnsetApplied) return 0;
            ElderOnsetApplied = true;
            double sum = 0;
            foreach (var s in new List<SkillType>(_xp.Keys)) sum += LoseLevelProgress(s, SkillPacing.ElderOnsetLoss);
            return sum;
        }

        /// <summary>
        /// Mevsim sonu. Yaşlıysa bu mevsim hiç kullanılmayan beceriler seviye içi ilerlemenin %25'ini kaybeder.
        /// Gerileyen becerileri döner. Kullanım kaydı her durumda sıfırlanır.
        /// </summary>
        public List<SkillType> OnSeasonEnd(bool elder)
        {
            var decayed = new List<SkillType>();
            if (PacingEnabled && elder)
                foreach (var s in new List<SkillType>(_xp.Keys))
                    if (!_usedThisSeason.Contains(s) && LoseLevelProgress(s, SkillPacing.ElderSeasonLoss) > 0) decayed.Add(s);
            _usedThisSeason.Clear();
            return decayed;
        }

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
