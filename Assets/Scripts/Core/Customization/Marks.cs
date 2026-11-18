using System;
using System.Collections.Generic;
using System.Linq;

namespace Thatchwick.Core.Customization
{
    /// <summary>Görünüme eklenen işaretin türü (proje dokümanı 14, bölüm 3F).</summary>
    public enum MarkKind
    {
        SkinDetail = 0,   // çil, ben, doğum lekesi, alacalık, al yanak, çiçek bozuğu
        Scar = 1,         // yara izi (yaratmada seçilen ya da kazadan kalan)
        Tattoo = 2,       // dövme (bölge başına bir)
        Jewelry = 3,      // küpe, burun halkası, kolye
        Makeup = 4        // sürme, allık, dudak boyası, kına
    }

    /// <summary>İşaretin nereden geldiği.</summary>
    public enum MarkSource
    {
        Chosen = 0,   // yaratmada ya da Ayna'da oyuncu seçti
        Event = 1,    // oyun içi olay (kaza)
        System = 2    // sistemin koyduğu (ileride: yaşa bağlı)
    }

    /// <summary>
    /// Görünüme eklenen tek işaret: tür, desen (Id), bölge, renk, tazelik. Kalıtılmaz.
    /// Kazadan kalan izler gün ve nedenle birlikte tutulur (karakter panelinde hikâyesiyle gösterilir).
    /// </summary>
    public sealed class Mark
    {
        public MarkKind Kind { get; set; }
        public string Id { get; set; }
        public string Zone { get; set; }
        /// <summary>Dövme mürekkebi, takı ya da makyaj rengi. İz ve ten detayında kullanılmaz (tenden gelir).</summary>
        public Rgb Color { get; set; } = new Rgb(47, 78, 122);
        /// <summary>0 taze, 1 soluk, 2 eski. Kazadan kalan iz taze başlar, zamanla solar.</summary>
        public int Fade { get; set; }
        public MarkSource Source { get; set; }
        /// <summary>Oyun günü (olayla gelenlerde); seçilenlerde -1.</summary>
        public double Day { get; set; } = -1;
        /// <summary>Olayla gelen işaretin nedeni ("baltayla elini kesti").</summary>
        public string Reason { get; set; }
        /// <summary>Ayna'da elmasla gizlenmiş iz: çizilmez ama kayıtta ve hikâyede kalır.</summary>
        public bool Hidden { get; set; }

        public Mark Clone() => (Mark)MemberwiseClone();

        public string Key() => $"{(int)Kind}:{Id}:{Zone}:{Color}:{Fade}:{(Hidden ? 1 : 0)}";
    }

    /// <summary>İşaret sınırları ve yardımcıları.</summary>
    public static class MarkRules
    {
        public const int MaxSkinDetails = 3;
        /// <summary>Görünen en çok iz sayısı. Fazlası kayıtta kalır, en eskisi görünümde çizilmez.</summary>
        public const int MaxVisibleScars = 3;
        /// <summary>Yaratmada ya da Ayna'da seçilebilecek en çok iz.</summary>
        public const int MaxChosenScars = 3;
        public const int MaxJewelry = 3;
        public const int MaxMakeup = 4;
        /// <summary>Taze iz bu kadar gün sonra solar (1), iki katında eski olur (2). 2 mevsim = 20 gün varsayılanı.</summary>
        public const double ScarFadeDays = 20.0;

        public static int Count(IEnumerable<Mark> marks, MarkKind kind) => marks.Count(m => m.Kind == kind);

        /// <summary>Çizilecek işaretler: izlerin yalnızca en yeni 3'ü, diğerleri olduğu gibi.</summary>
        public static List<Mark> Visible(IReadOnlyList<Mark> marks)
        {
            var scars = marks.Where(m => m.Kind == MarkKind.Scar && !m.Hidden).ToList();
            var hidden = new HashSet<Mark>(scars.Take(Math.Max(0, scars.Count - MaxVisibleScars)));
            return marks.Where(m => !m.Hidden && !hidden.Contains(m)).ToList();
        }

        /// <summary>Kazadan kalan izlerin tazeliğini güne göre günceller.</summary>
        public static void UpdateFade(IEnumerable<Mark> marks, double today)
        {
            foreach (var m in marks)
            {
                if (m.Kind != MarkKind.Scar || m.Source != MarkSource.Event || m.Day < 0) continue;
                double age = today - m.Day;
                m.Fade = age >= 2 * ScarFadeDays ? 2 : age >= ScarFadeDays ? 1 : 0;
            }
        }
    }
}
