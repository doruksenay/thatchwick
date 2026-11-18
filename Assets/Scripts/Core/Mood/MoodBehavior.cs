using System;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Mood
{
    /// <summary>Bunalım türü (Perişan karakterin kısa kontrol kaybı; şiddetsiz, eşyaya zarar yok).</summary>
    public enum BreakdownKind
    {
        None = 0,
        /// <summary>Odasına kapandı: yatağının yanına çekilir, iş yapmaz.</summary>
        Withdraw = 1,
        /// <summary>Ağladı: olduğu yerde oturur.</summary>
        Cry = 2,
        /// <summary>Meydana kaçtı: köy meydanına yürür, orada oyalanır.</summary>
        RunOff = 3,
        /// <summary>Tıkındı: bir öğün fazla yer, sonra oturur.</summary>
        Binge = 4
    }

    /// <summary>
    /// Ruh halinin davranışa etkisi (karar 18.11.2026, Project belgesi 23): uç durumda emir reddi, bunalım,
    /// pişirme hevesi, çocukluk ruh halinden özellik. Sayılar yer tutucudur (Adım 28).
    /// </summary>
    public static class MoodBehavior
    {
        // ---- uç durumda emir reddi (7.2) ----

        /// <summary>Ölümcül ihtiyaç (açlık, enerji, ısınma) bunun altındaysa karakter o ihtiyacı gidermeyen işi başlatmaz.</summary>
        public const float RefuseBelow = 5f;

        /// <summary>Reddederken eylem balonu ve bildirim metni.</summary>
        public static string RefusalText(NeedType need)
        {
            switch (need)
            {
                case NeedType.Hunger: return "Önce bir şeyler yemeliyim.";
                case NeedType.Energy: return "Önce biraz uyumalıyım.";
                case NeedType.Warmth: return "Önce ısınmalıyım.";
                default: return "Önce kendime bakmalıyım.";
            }
        }

        // ---- pişirme hevesi (P0-3) ----

        /// <summary>İyi ya da Mutlu karakter otonomide yemeği pişirmeyi çiğ yemeye bu çarpanla yeğler (eskiden kazara hız farkıydı).</summary>
        public const double EagerCookBonus = 1.35;
        public const string EagerCookNote = "keyfi yerinde, pişirmeye hevesli";

        public static bool IsEagerCook(MoodStatus s) => s == MoodStatus.Good || s == MoodStatus.Happy;

        // ---- sofra çeşidi (P0-2) ----

        /// <summary>Otonomi, iki öğündür yenen yiyeceği yeniden seçerken puanı bu çarpanla düşer (başka yiyecek varsa çeşitlenir).</summary>
        public const double RepeatMealPenalty = 0.55;

        // ---- bunalım (7.1) ----

        /// <summary>Bunalım ancak Perişan'da bu kadar gün kaldıktan sonra başlayabilir (önce uyarı gelir).</summary>
        public const double BreakdownGraceDays = 0.5;
        /// <summary>Perişan'da kalındıkça ortalama bunalım aralığı (oyun günü).</summary>
        public const double BreakdownMeanDays = 2.0;
        /// <summary>Bir bunalımdan sonra yenisi en erken bu kadar gün sonra.</summary>
        public const double BreakdownCooldownDays = 2.0;
        /// <summary>Bunalım süresi: 2-6 saat.</summary>
        public const double BreakdownMinDays = 2.0 / 24.0, BreakdownMaxDays = 6.0 / 24.0;

        public static string BreakdownName(BreakdownKind k)
        {
            switch (k)
            {
                case BreakdownKind.Withdraw: return "Odasına kapandı";
                case BreakdownKind.Cry: return "Ağlıyor";
                case BreakdownKind.RunOff: return "Meydana kaçtı";
                case BreakdownKind.Binge: return "Tıkınıyor";
                default: return "";
            }
        }

        /// <summary>Bildirim metni ("Wat odasına kapandı.").</summary>
        public static string BreakdownNotice(string name, BreakdownKind k)
        {
            switch (k)
            {
                case BreakdownKind.Withdraw: return $"{name} odasına kapandı: kimseyle konuşmak istemiyor.";
                case BreakdownKind.Cry: return $"{name} ağlamaya başladı.";
                case BreakdownKind.RunOff: return $"{name} her şeyi bırakıp meydana gitti.";
                case BreakdownKind.Binge: return $"{name} derdini yemekle bastırıyor.";
                default: return name;
            }
        }

        // ---- çocukluk ruh hali → özellik (7.3) ----

        /// <summary>Çocuk ve Ergen boyunca ruh hali ortalaması bunun üstündeyse Neşeli, altındaysa Kasvetli büyür.</summary>
        public const double UpbringingCheerfulAt = 10.0, UpbringingGloomyAt = -10.0;
        public const string TraitCheerful = "trait.cheerful", TraitGloomy = "trait.gloomy";
        /// <summary>Bu kadar günden az ölçülmüşse (eski kayıt, ortasından başlayan) çocukluk özelliği verilmez, ebeveyn kuralı uygulanır.</summary>
        public const double UpbringingMinDays = 2.0;

        /// <summary>Ortalama ve ölçülen süreye göre çocukluk özelliği; arada ya da ölçü yetersizse null.</summary>
        public static string UpbringingTrait(double average, double days)
        {
            if (days < UpbringingMinDays) return null;
            if (average >= UpbringingCheerfulAt) return TraitCheerful;
            if (average <= UpbringingGloomyAt) return TraitGloomy;
            return null;
        }

        /// <summary>Ergen panelindeki "Yetişme" göstergesi: ortalama ve gidişat.</summary>
        public static string UpbringingSummary(double average, double days)
        {
            string sign = average >= 0 ? "+" : "";
            string toward = average >= UpbringingCheerfulAt ? "Neşeli büyüyor"
                : average <= UpbringingGloomyAt ? "Kasvetli büyüyor"
                : "ebeveyninden bir huy alacak";
            return days <= 0 ? "Yetişme: henüz ölçülmedi" : $"Yetişme: {sign}{average:0} ({toward})";
        }
    }
}
