using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Progress
{
    /// <summary>Bir becerinin tempo özeti (SkillTempo.Analyze).</summary>
    public sealed class SkillTempoRow
    {
        public SkillType Skill;
        /// <summary>En hızlı XP veren iş (eşya.etkileşim); yoksa dolaylı kaynak adı.</summary>
        public string BestSource;
        /// <summary>Seviye 1'de tam gün kesintisiz çalışmada ham XP (günde).</summary>
        public double XpPerWorkDay;
        /// <summary>Odaklı bir yetişkinin 1 → 10 süresi (oyun günü), yatkınsız ve yatkın.</summary>
        public double DaysTo10, DaysTo10Apt;
        /// <summary>Seviye başına gün (2..10).</summary>
        public double[] DaysPerLevel = new double[SkillRules.MaxLevel + 1];
    }

    /// <summary>
    /// Beceri süre tablosu (Project belgesi 28-A1 ve 45 bölüm 4, 7-5): içerikteki işlerin XP ve sürelerinden,
    /// tek işe odaklanan bir yetişkinin her beceride 1 → 10 kaç oyun gününde çıktığını hesaplar. İş hızı seviyeyle
    /// artar (%5/sv, en çok yarı süre), günlük doygunluk eşiği uygulanır, günün <see cref="WorkShare"/> kadarı işe ayrılır
    /// (denge 41: uyku ~%33, yemek ~%12, keyif ve yol). Malzeme bulunduğu varsayılır (üst sınır tahmini).
    /// </summary>
    public static class SkillTempo
    {
        public const double WorkShare = 0.45;
        /// <summary>Hedef (28-A1): ana beceri yetişkin ömrünün (~42 gün) %40'ında 10.</summary>
        public const double TargetDays = 17.0;

        /// <summary>İşe bağlı olmayan becerilerin günlük ham XP tahmini (yürüme, sohbet, ders, pazar...).</summary>
        public static readonly Dictionary<SkillType, (string source, double xpPerDay)> Indirect = new Dictionary<SkillType, (string, double)>
        {
            { SkillType.Athletics, ("yürüme (günde ~1500 karo × 0,05) ve oyun", 80) },
            { SkillType.Charm, ("sohbet (4 XP, 0,08 gün)", 4 / 0.08) },
            { SkillType.Trade, ("pazar (satış ve teklif)", 60) },
            { SkillType.Carting, ("pazara mal taşıma (birim 0,5)", 30) },
            { SkillType.Riding, ("at sırtında karo 0,4", 240) },
            { SkillType.Parenting, ("besleme 5, masal 4", 5 / 0.05) },
            { SkillType.Teaching, ("ders (yazı tahtası)", 60) },
            { SkillType.Stewardship, ("görev dağıtma (hane başı, sabah)", 20) },
            { SkillType.Decorating, ("eşya yerleştirme (2)", 40) },
            { SkillType.Salvage, ("söküm (birim 3)", 60) },
            { SkillType.Masonry, ("yol onarımı", 60) },
            { SkillType.Medicine, ("sabah yara iyileştirme ve hekimlik işleri", 40) },
            { SkillType.WeatherLore, ("rüzgâr fırıldağı ve havayı söyle", 30) },
            { SkillType.Music, ("çalgı çalma", 60) },
            { SkillType.Literacy, ("okuma", 60) },
            { SkillType.Hunting, ("pusu ve tuzak", 60) },
            { SkillType.Mining, ("kazı (karo başına sertlik)", 100) },
        };

        public static double SpeedFactor(int level) => 1.0 / Math.Max(0.5, 1.0 - 0.05 * (level - 1));

        /// <summary>Bu beceride en hızlı XP veren iş: (ad, saf iş gününde ham XP).</summary>
        public static (string source, double xpPerDay) BestDirect(ContentDatabase db, SkillType s)
        {
            string best = null; double rate = 0;
            foreach (var d in db.Items.All)
                foreach (var i in d.Interactions)
                {
                    // Beceri işleri (skillWork: imece, misafir odası, aşı...) seyrek ya da bekleme süreli; tekrar kaynağı sayılmaz.
                    if (i.Skill != s || i.SkillXp <= 0 || i.DurationDays <= 0 || i.Hidden || i.SkillWork != null) continue;
                    double r = i.SkillXp / i.DurationDays;
                    if (r > rate) { rate = r; best = $"{d.Id}.{i.Id}"; }
                }
            return (best, rate);
        }

        /// <summary>Ham XP hızı ve doygunlukla seviye başına süre.</summary>
        public static SkillTempoRow Row(ContentDatabase db, SkillType s)
        {
            var (src, rate) = BestDirect(db, s);
            if (Indirect.TryGetValue(s, out var ind) && ind.xpPerDay > rate) { src = ind.source; rate = ind.xpPerDay; }
            var row = new SkillTempoRow { Skill = s, BestSource = src ?? "kaynak yok", XpPerWorkDay = rate };
            double scaled = rate * SkillPacing.XpScale(s);
            row.DaysTo10 = Days(scaled, Aptitude.None, row.DaysPerLevel);
            row.DaysTo10Apt = Days(scaled, Aptitude.Minor, null);
            return row;
        }

        public static List<SkillTempoRow> Analyze(ContentDatabase db) =>
            ((SkillType[])Enum.GetValues(typeof(SkillType))).Select(s => Row(db, s)).ToList();

        /// <summary>1 → 10 gün sayısı; ham XP hiç yoksa sonsuz.</summary>
        public static double Days(double ratePerWorkDay, Aptitude apt, double[] perLevel)
        {
            if (ratePerWorkDay <= 0) return double.PositiveInfinity;
            double total = 0;
            for (int lv = 1; lv < SkillRules.MaxLevel; lv++)
            {
                double raw = ratePerWorkDay * WorkShare * SpeedFactor(lv) * SkillPacing.XpMultiplier(apt);
                double threshold = SkillPacing.DailyThreshold(lv, apt);
                double gained = raw <= threshold ? raw : threshold + (raw - threshold) * SkillPacing.SaturatedRate;
                double d = SkillPacing.LevelSpan(lv) / gained;
                if (perLevel != null) perLevel[lv + 1] = d;
                total += d;
            }
            return total;
        }
    }
}
