using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Customization
{
    /// <summary>
    /// Geçici saç bakımı (belge 59a §6, belge 65 D2): kına, papatya suyu, ceviz kabuğu suyu, mor saç suyu ve saç yağı.
    /// Yalnızca görünüşe uygulanır: doğal saç rengi (<see cref="Appearance.HairColor"/>) ve genom değişmez, çocuğa geçmez.
    /// Yıkanınca geçer (leğen, küvet, kuyu, kova, dereye girmek); yağmurda ıslanmak ve el yüz yıkamak silmez.
    /// Kalıcı değişiklik yine Ayna'da. Mor saç temel palette yok; yalnızca mor saç suyuyla elde edilir.
    /// </summary>
    public static class HairTints
    {
        public const string Henna = "henna", Chamomile = "chamomile", Walnut = "walnut", Purple = "purple";
        /// <summary>Saç yağı: renk değil, parlaklık. Tonla birlikte durabilir.</summary>
        public const string Shine = "shine";

        private sealed class TintDef
        {
            public Rgb Target;
            public double Strength;
            /// <summary>0 = her saçta aynı tutar; 1 = yalnızca açık saçta tutar (boya koyu saçta hafif bir yansıma bırakır).</summary>
            public double LightDependence;
        }

        private static readonly Dictionary<string, TintDef> Defs = new Dictionary<string, TintDef>
        {
            // Kına: kızıl bakır. Ak ve kır saçta parlak turuncu-kızıl, kara saçta kızıl yansıma.
            [Henna] = new TintDef { Target = Rgb.FromHex("#A5471F"), Strength = 0.75, LightDependence = 0.65 },
            // Papatya suyu: altın sarısına açar; koyu saçta çok az etki.
            [Chamomile] = new TintDef { Target = Rgb.FromHex("#E8D28A"), Strength = 0.40, LightDependence = 0.70 },
            // Ceviz kabuğu suyu: koyu kahveye boyar, akları da örter.
            [Walnut] = new TintDef { Target = Rgb.FromHex("#3B2A1D"), Strength = 0.60, LightDependence = 0.0 },
            // Mor saç suyu (mor boyadan): açık saçta mor, koyu saçta mürdüm.
            [Purple] = new TintDef { Target = Rgb.FromHex("#6A3E66"), Strength = 0.90, LightDependence = 0.45 },
        };

        private static readonly Rgb White = new Rgb(255, 255, 255);

        /// <summary>Renk tonları (saç yağı hariç).</summary>
        public static IEnumerable<string> Tints => Defs.Keys;

        public static bool IsTint(string id) => id != null && Defs.ContainsKey(id);

        /// <summary>Eylemde kullanılabilen bakım kimliği (ton ya da saç yağı).</summary>
        public static bool IsKnown(string id) => id == Shine || IsTint(id);

        /// <summary>Görünen saç rengi: doğal (kırlaşmış) renk üstüne ton, sonra yağ parlaklığı.</summary>
        public static Rgb Apply(Rgb natural, string tintId, bool shine)
        {
            var c = natural;
            if (tintId != null && Defs.TryGetValue(tintId, out var d))
            {
                double s = d.Strength * (1.0 - d.LightDependence + d.LightDependence * Luma(c));
                c = Rgb.Lerp(c, d.Target, s);
            }
            if (shine) c = Rgb.Lerp(c, White, 0.08);
            return c;
        }

        /// <summary>Bakımı uygular: ton bir öncekinin yerini alır; yağ tonu silmez.</summary>
        public static bool Treat(Appearance a, string id)
        {
            if (a == null || !IsKnown(id)) return false;
            if (id == Shine) a.HairShine = true;
            else a.HairTintId = id;
            return true;
        }

        private static double Luma(Rgb c) => (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
    }
}
