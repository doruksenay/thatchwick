using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Mood
{
    /// <summary>İhtiyaç çubuğunun rengi (Project belgesi 66 §3.1 kural 7): sayı gösterilmez, bant gösterilir.</summary>
    public enum NeedBand
    {
        Critical = 0, // < 15, yanıp söner
        Low = 1,      // 15-30, kırmızı
        Fair = 2,     // 30-60, hardal
        Good = 3      // > 60, yeşil
    }

    /// <summary>
    /// Karakter kartının gösterim kuralları (Project belgesi 68, Sims ve Stardew karşılaştırması): oyuncu sayıyı değil,
    /// temsilini görür. Ruh hali yüz ikonu ve sözcük, ihtiyaç ikon ve renkli çubuk, iç ses çerçeveli ikon ve 1-3 güç işareti.
    /// Gerçek değer yalnızca öğeye dokununca açılan ayrıntıda görünür. Burada yalnızca kurallar var, çizim Game katmanında.
    /// </summary>
    public static class InnerVoiceView
    {
        /// <summary>Güç işareti eşikleri: |değer| bunlara ulaşınca 2 ve 3 işaret (Sims'teki "çok mutlu" basamağı gibi).</summary>
        public const int StrongAt = 10, VeryStrongAt = 20;

        /// <summary>Süreli iç ses süresinin bu payı kalınca (ya da <see cref="ExpiringDays"/> altında) yanıp söner (Stardew'deki bitmek üzere olan etki gibi).</summary>
        public const double ExpiringShare = 0.2, ExpiringDays = 0.1;

        /// <summary>1, 2 ya da 3 güç işareti; 0 değerli (yalnızca durum bildiren) iç seste 0.</summary>
        public static int Strength(int value)
        {
            int a = Math.Abs(value);
            if (a == 0) return 0;
            if (a >= VeryStrongAt) return 3;
            if (a >= StrongAt) return 2;
            return 1;
        }

        /// <summary>Süreli iç ses bitmek üzere mi (koşullu olanlar hiç yanıp sönmez).</summary>
        public static bool Expiring(ActiveMoodlet m)
        {
            if (m?.Def == null || m.Def.IsCondition) return false;
            return m.RemainingDays <= ExpiringDays || m.RemainingDays <= m.Def.DurationDays * ExpiringShare;
        }

        public static NeedBand Band(float value)
        {
            if (value < 15f) return NeedBand.Critical;
            if (value < 30f) return NeedBand.Low;
            if (value <= 60f) return NeedBand.Fair;
            return NeedBand.Good;
        }

        /// <summary>Ruh hali yüzünün ikon anahtarı: ui_icon_mood_&lt;anahtar&gt;.</summary>
        public static string MoodIconKey(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return "mood_miserable";
                case MoodStatus.Unhappy: return "mood_unhappy";
                case MoodStatus.Good: return "mood_good";
                case MoodStatus.Happy: return "mood_happy";
                default: return "mood_neutral";
            }
        }

        /// <summary>İhtiyaç ikonunun anahtarı (ui_icon_need_&lt;anahtar&gt;; dosya adları 315'ten, Temizlik dosyası "hygiene").</summary>
        public static string NeedIconKey(NeedType n)
        {
            switch (n)
            {
                case NeedType.Hunger: return "need_hunger";
                case NeedType.Energy: return "need_energy";
                case NeedType.Warmth: return "need_warmth";
                case NeedType.Fun: return "need_fun";
                case NeedType.Hygiene: return "need_hygiene";
                case NeedType.Social: return "need_social";
                default: return "need_bladder";
            }
        }

        // Kimliği başka bir iç sesin ikonunu paylaşanlar (aynı anlam, ayrı kayıt). Sağ taraf ui_icon_ sonrası tam ad.
        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // İhtiyaç iç sesleri ihtiyacın kendi ikonunu taşır.
            { MoodCatalog.Hungry, "need_hunger" }, { MoodCatalog.Tired, "need_energy" }, { MoodCatalog.Cold, "need_warmth" },
            { MoodCatalog.Bored, "need_fun" }, { MoodCatalog.Dirty, "need_hygiene" }, { MoodCatalog.Lonely, "need_social" },
            { MoodCatalog.Bursting, "need_bladder" }, { MoodCatalog.Chilled, "need_warmth" },
            { MoodCatalog.Ashamed, "moodlet_embarrassed" },
            { MoodCatalog.InjuredCaveIn, "moodlet_injured" },
            // Mutfak ve zincir iç sesleri (Project belgeleri 58, 60).
            { "cook.ate_together", "moodlet_ate_together" }, { "cook.together", "moodlet_ate_together" },
            { "cook.cat_ate", "moodlet_cat_ate_it" }, { "cook.mouth_waters", "moodlet_mouth_watering" },
            { "cook.smell_good", "moodlet_mouth_watering" }, { "cook.smell_burnt", "moodlet_burnt_smell" },
            { "cook.burnt", "moodlet_burnt_smell" }, { "cook.fine_meal", "moodlet_good_meal" },
            { "cook.loved_dish", "moodlet_good_meal" }, { "cook.parent_dish", "moodlet_family_warmth" },
            { "cook.family_care", "moodlet_family_warmth" }, { "cook.hard_to_swallow", "moodlet_hard_to_swallow" },
            { "cook.disliked_dish", "moodlet_hard_to_swallow" }, { "cook.stale", "moodlet_hard_to_swallow" },
            { "cook.me_time", "moodlet_me_time" }, { "cook.resting", "moodlet_taking_it_easy" },
            { "cook.at_ease", "moodlet_taking_it_easy" }, { "cook.idle", "moodlet_idle_too_long" },
            { "cook.bored", "moodlet_bored" }, { "cook.embarrassed", "moodlet_embarrassed" },
            { "cook.picnic", "moodlet_picnic" }, { "cook.praised", "moodlet_proud" },
            { "cook.cat_laugh", "moodlet_laughed_a_lot" }, { "cook.cat_stunned", "moodlet_astonished" },
            { "cook.sad_animal", "moodlet_animal_died" }, { "cook.worked_together", "moodlet_harmony" },
            { "mood.festival", "moodlet_festival" }, { "mood.gift", "moodlet_gift" }, { "eff_gift_glasses", "moodlet_gift" },
            { "mood.daylight", "moodlet_bright_room" }, { "mood.vein_opened", "moodlet_vein_found" },
            { "mood.mine_wet", "moodlet_wet" }, { "mood.duty_tired", "need_energy" }, { "companion_grief", "moodlet_grief" },
            { "wedding_fine", "moodlet_newlywed" }, { "wedding_grand", "moodlet_newlywed" },
            { "wedding_plain", "moodlet_newlywed" }, { "wedding_self", "moodlet_newlywed" },
        };

        // Önekle aile: injured_axe, injured_burn ... tek "Yaralı" ikonu; meal_* yemek; harmony_* uyum.
        private static readonly (string prefix, string icon)[] Families =
        {
            ("injured_", "moodlet_injured"), ("meal_", "moodlet_good_meal"), ("harmony_", "moodlet_harmony"),
        };

        /// <summary>Genel yedek ikonlar: kendi ikonu olmayan keyif ve kasvet.</summary>
        public const string FallbackJoy = "moodlet_joy", FallbackGloom = "moodlet_gloom";

        /// <summary>
        /// İç sesin ikon adayları, sırayla (her biri ui_icon_ sonrası ad): kimliğin eşi, kendi ikonu (ön eksiz kimlikle de),
        /// ailesi, genel keyif ya da kasvet. Arayüz ilk bulduğunu çizer; hiçbiri yoksa yalnızca çerçeve ve güç işareti kalır.
        /// </summary>
        public static List<string> IconKeys(string id, int value)
        {
            var keys = new List<string>(5);
            if (!string.IsNullOrEmpty(id))
            {
                if (Aliases.TryGetValue(id, out var alias)) keys.Add(alias);
                keys.Add("moodlet_" + id);
                string bare = StripPrefix(id);
                if (!string.Equals(bare, id, StringComparison.Ordinal)) keys.Add("moodlet_" + bare);
                foreach (var (prefix, icon) in Families)
                    if (bare.StartsWith(prefix, StringComparison.Ordinal)) { keys.Add(icon); break; }
            }
            keys.Add(value < 0 ? FallbackGloom : FallbackJoy);
            return keys;
        }

        private static string StripPrefix(string id)
        {
            int dot = id.LastIndexOf('.');
            if (dot >= 0) return id.Substring(dot + 1);
            return id.StartsWith("eff_", StringComparison.Ordinal) ? id.Substring(4) : id;
        }

        /// <summary>Ayrıntıdaki tür satırı: "+10 · keyif", "-15 · kasvet", "durum".</summary>
        public static LocText KindLine(int value)
        {
            if (value > 0) return LocText.Of("ui.mood.card.kind_joy", ("value", "+" + value));
            if (value < 0) return LocText.Of("ui.mood.card.kind_gloom", ("value", value));
            return LocText.Of("ui.mood.card.kind_status");
        }

        /// <summary>Kalan süre, oyun saatiyle ("5 saat kaldı"); koşullu olanlarda "durum sürdükçe".</summary>
        public static LocText RemainingText(ActiveMoodlet m)
        {
            if (m?.Def == null || m.Def.IsCondition) return LocText.Of("ui.mood.card.while_condition");
            int hours = (int)Math.Round(m.RemainingDays * 24.0);
            if (hours < 1) return LocText.Of("ui.mood.card.ending");
            if (hours < 48) return LocText.Counted("ui.mood.card.hours_left", hours, ("n", hours));
            int days = (int)Math.Round(m.RemainingDays);
            return LocText.Counted("ui.mood.card.days_left", days, ("n", days));
        }

        /// <summary>
        /// Ruh hali ayrıntısı: toplam ve bir üst basamağa ne kadar kaldığı ("Mutlu için 11 daha"). En üst basamakta yalnızca toplam.
        /// </summary>
        public static LocText MoodDetail(int total)
        {
            var status = MoodRules.StatusFor(total);
            string sign = total > 0 ? "+" + total : total.ToString();
            int next;
            switch (status)
            {
                case MoodStatus.Miserable: next = MoodRules.MiserableAt + 1; break;
                case MoodStatus.Unhappy: next = MoodRules.UnhappyAt + 1; break;
                case MoodStatus.Neutral: next = MoodRules.GoodAt; break;
                case MoodStatus.Good: next = MoodRules.HappyAt; break;
                default: return LocText.Of("ui.mood.card.total_top", ("total", sign));
            }
            return LocText.Of("ui.mood.card.total_next", ("total", sign), ("need", next - total),
                ("next", StatusName((MoodStatus)((int)status + 1))));
        }

        /// <summary>Ruh hali basamağının adı, seçili dilde (MoodRules.Name'in anahtarlı karşılığı).</summary>
        public static LocText StatusName(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return LocText.Of("ui.mood.status.miserable");
                case MoodStatus.Unhappy: return LocText.Of("ui.mood.status.unhappy");
                case MoodStatus.Good: return LocText.Of("ui.mood.status.good");
                case MoodStatus.Happy: return LocText.Of("ui.mood.status.happy");
                default: return LocText.Of("ui.mood.status.neutral");
            }
        }

        /// <summary>Ruh halinin işe etkisi: "Tecrübe ×1,1 · iş hızı ×1,05" (Nötr'de boş).</summary>
        public static LocText EffectLine(MoodStatus s)
        {
            if (s == MoodStatus.Neutral) return LocText.Plain("");
            return LocText.Of("ui.mood.card.effects", ("xp", MoodRules.XpMultiplier(s)), ("speed", MoodRules.SpeedMultiplier(s)));
        }
    }
}
