using System;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Customization
{
    /// <summary>
    /// Kendiliğinden gelen görünüm (doküman 14, 3E ve 3F; ücretsiz): kırlaşma, kırışıklık, saç açılması, sargı.
    /// Görünüme yazılmaz; çizim anında karakterden hesaplanır. Genler: erken kırlaşma (baskın), kellik (çekinik).
    /// </summary>
    public static class AgingLook
    {
        /// <summary>Saçın ne kadarı ak (0-1). Yetişkinliğin %60'ında başlar (erken kırlaşmada %20'sinde), yaşlılığın 10. gününde tamdır.</summary>
        public static double GrayFraction(double ageDays, AgeConfig cfg, bool earlyGray)
        {
            double adultSpan = cfg.ElderStartDay - cfg.AdultStartDay;
            double start = cfg.AdultStartDay + adultSpan * (earlyGray ? 0.2 : 0.6);
            double full = cfg.ElderStartDay + 10;
            if (ageDays <= start) return 0;
            if (ageDays >= full) return 1;
            return (ageDays - start) / (full - start);
        }

        /// <summary>Kırışıklık: yaşlılıkta ya da yetişkinliğin son çeyreğinde.</summary>
        public static bool Wrinkled(double ageDays, AgeConfig cfg) =>
            ageDays >= cfg.AdultStartDay + (cfg.ElderStartDay - cfg.AdultStartDay) * 0.75;

        /// <summary>Saç açılması: kellik geni görünüyorsa yetişkinliğin ortasından sonra.</summary>
        public static bool Receding(double ageDays, AgeConfig cfg, bool baldingGene) =>
            baldingGene && ageDays >= cfg.AdultStartDay + (cfg.ElderStartDay - cfg.AdultStartDay) * 0.5;

        /// <summary>Kırlaşmış saç rengi (saç rengi ile ak arasında).</summary>
        public static Rgb GrayedHair(Rgb hair, double fraction) => Rgb.Lerp(hair, new Rgb(214, 210, 204), Math.Max(0, Math.Min(1, fraction)));

        /// <summary>Karakter için hesaplanmış durum.</summary>
        public static LookState For(Character c, AppearanceCatalog catalog)
        {
            bool early = catalog != null && c.Genome != null && Genetics.Shows(c.Genome, Genes.EarlyGray, catalog);
            bool bald = catalog != null && c.Genome != null && Genetics.Shows(c.Genome, Genes.Balding, catalog);
            return new LookState
            {
                Gray = GrayFraction(c.AgeDays, c.AgeConfig, early),
                Wrinkles = Wrinkled(c.AgeDays, c.AgeConfig),
                Receding = Receding(c.AgeDays, c.AgeConfig, bald),
                BandageZone = c.Injury?.Zone
            };
        }
    }

    /// <summary>Çizim anında uygulanacak, kayda yazılmayan görünüm durumu.</summary>
    public sealed class LookState
    {
        public double Gray;
        public bool Wrinkles;
        public bool Receding;
        /// <summary>Taze yara varsa sargının bölgesi; yoksa null.</summary>
        public string BandageZone;
    }
}
