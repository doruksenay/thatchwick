using System;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Yürüyüş biçimi (Project belgesi 69 §3.2). Kare anahtarı olan biçimler (<see cref="AnimStyleRules.WalkKey"/>) kendi kareleri
    /// gelince onları oynar; gelene kadar ve kare anahtarı olmayanlar normal yürüme karesini kodla değiştirilmiş adımla oynar.
    /// </summary>
    public enum Gait
    {
        Normal = 0,
        Cheerful = 1,   // Neşeli, Şevkli, Gururlu: hafif sekerek (walk_happy)
        Heavy = 2,      // Kederli, çok yorgun, ağır yük: ayak sürüyerek (walk_heavy)
        Tense = 3,      // Gergin: kısa, hızlı adım (kod)
        Stomp = 4,      // Hırçın: yere basarak (kod)
        Easy = 5,       // Huzurlu: ağır ve rahat (kod)
        Run = 6,        // koşu (run)
        Cane = 7,       // yaşlı, diz sıkıntısı: bastona dayanarak (walk_cane)
        Limp = 8,       // yaralı (kod)
        Cold = 9,       // üşüyen (kod)
        Tipsy = 10      // +18 paketi, içkili (kod)
    }

    /// <summary>Uzun işin türü: ara klip seçiminde ve terlemede kullanılır.</summary>
    public enum WorkContext
    {
        None = 0,
        Heavy = 1,      // odun, kazı, harman, demirci, çapa
        Bend = 2,       // ekim, hasat, yumurta, ot yolma, çamaşır
        Craft = 3,      // tezgâh işleri
        Cook = 4,       // mutfak
        Social = 5,
        Calm = 6        // oturma, okuma, eğlence
    }

    /// <summary>Animasyon biçiminin girdileri: oyun katmanı karakterden doldurur (Core'da Unity yok, test edilebilir).</summary>
    public struct AnimInputs
    {
        public Hal Hal;
        public AgeStage Stage;
        public float Energy, Hunger, Warmth, Hygiene;   // 0-100
        public float Wetness;                           // 0-1
        public float Temperature;                       // °C (karakterin bulunduğu yer)
        public bool Outdoors;
        /// <summary>Şu anki işin beceri seviyesi; beceri işi değilse 0.</summary>
        public int SkillLevel;
        public WorkContext Work;
        /// <summary>Taşınan yükün taşıma gücüne oranı (59-depo; 1 = tam kapasite, 1,5 = sert sınır).</summary>
        public float LoadRatio;
        public bool Injured, LegInjury, Pregnant, KneeTrouble, HasCane, Tipsy, Running, Dusty;

        public static AnimInputs From(Character c)
        {
            var i = new AnimInputs
            {
                Hal = c.Mood != null ? HalRules.Dominant(c.Mood) : Hal.Calm,
                Stage = c.Stage,
                Energy = c.Needs.Get(NeedType.Energy),
                Hunger = c.Needs.Get(NeedType.Hunger),
                Warmth = c.Needs.Get(NeedType.Warmth),
                Hygiene = c.Needs.Get(NeedType.Hygiene),
                Wetness = c.Wetness,
                Temperature = c.AmbientTemperature,
                Injured = c.Injury != null,
                Pregnant = c.IsPregnant,
                Dusty = c.Mood != null && c.Mood.Has(MoodCatalog.Dusty),
            };
            i.LegInjury = c.Injury != null && AnimStyleRules.IsLegInjury(c.Injury.Kind, c.Injury.Zone);
            return i;
        }
    }

    /// <summary>Bir karakterin o anki animasyon biçimi. Bütün çarpanlar 1 = bugünkü davranış.</summary>
    public struct AnimStyle
    {
        /// <summary>İş ve durma döngüsünün hızı (0,75-1,25).</summary>
        public float Tempo;
        /// <summary>Vuruş karesinde rastgele bekleme olasılığı (0 düzgün, 0,6 çok düzensiz).</summary>
        public float Irregularity;
        /// <summary>Bu kadar döngüde bir kısa mola (0 = mola yok).</summary>
        public int RestEvery;
        /// <summary>Yürürken ve dururken iniş çıkış genliği çarpanı.</summary>
        public float Bob;
        public Gait Gait;
        /// <summary>Aynı mesafede atılan adım sayısı çarpanı (Gergin kısa ve sık adım: 1,25).</summary>
        public float Cadence;
        /// <summary>Topallama ya da yalpalama: adımda yana kayma (piksel).</summary>
        public float Sway;
        public bool Sweat, Breath;
    }

    /// <summary>
    /// Kod katmanı (Project belgesi 69 §2, K1-K5): hâl, beceri, yorgunluk, yaş, yük ve havadan bütün fiillere uygulanan
    /// tempo, ritim, salınım ve yürüyüş biçimi. Yeni kare istemez; karesi olan biçim kendi karesini oynar.
    /// </summary>
    public static class AnimStyleRules
    {
        public const float MinTempo = 0.75f, MaxTempo = 1.25f;
        public const float TiredBelow = 20f, SweatFrom = 26f, BreathBelow = 3f;

        /// <summary>Bacağı tutan yara: düşme, göçük, hayvan (tekme), orak; ya da bölgesi bacak/ayak.</summary>
        public static bool IsLegInjury(string kind, string zone)
        {
            if (zone != null && (zone.IndexOf("leg", StringComparison.Ordinal) >= 0 || zone.IndexOf("foot", StringComparison.Ordinal) >= 0
                || zone.IndexOf("knee", StringComparison.Ordinal) >= 0)) return true;
            return kind == "fall" || kind == "cave_in" || kind == "child_fall";
        }

        public static float HalTempo(Hal h)
        {
            switch (h)
            {
                case Hal.Eager: return 1.12f;
                case Hal.Cheerful: return 1.05f;
                case Hal.Tense: return 1.06f;
                case Hal.Cross: return 1.08f;
                case Hal.Peaceful: return 0.96f;
                case Hal.Sad: return 0.88f;
                default: return 1f;
            }
        }

        public static AnimStyle Compute(in AnimInputs i)
        {
            var s = new AnimStyle { Tempo = 1f, Irregularity = 0.15f, Bob = 1f, Cadence = 1f };
            bool tired = i.Energy < TiredBelow;

            // K1 tempo: hâl × beceri × yorgunluk × yaş.
            float t = HalTempo(i.Hal);
            if (i.SkillLevel > 0) t *= i.SkillLevel <= 3 ? 0.92f : i.SkillLevel >= 7 ? 1.08f : 1f;
            if (tired) t *= 0.88f;
            if (i.Stage == AgeStage.Elder) t *= 0.88f;
            else if (i.Stage == AgeStage.Teen || i.Stage == AgeStage.Child) t *= 1.06f;
            if (i.Pregnant) t *= 0.92f;
            s.Tempo = Clamp(t, MinTempo, MaxTempo);

            // K2 ritim: acemi düzensiz, usta düzgün; yorgunluk ve gerginlik bozar; yorgun her 3 döngüde mola verir.
            float irr = i.SkillLevel <= 0 ? 0.15f : i.SkillLevel <= 3 ? 0.35f : i.SkillLevel >= 7 ? 0.05f : 0.15f;
            if (tired) irr += 0.2f;
            if (i.Hal == Hal.Tense) irr += 0.1f;
            s.Irregularity = Clamp(irr, 0f, 0.6f);
            s.RestEvery = tired ? 3 : i.Stage == AgeStage.Elder ? 5 : 0;

            // K5 salınım.
            switch (i.Hal)
            {
                case Hal.Cheerful: s.Bob = 1.5f; break;
                case Hal.Eager: s.Bob = 1.3f; break;
                case Hal.Peaceful: s.Bob = 0.8f; break;
                case Hal.Sad: s.Bob = 0.3f; break;
            }
            if (tired) s.Bob = Math.Min(s.Bob, 0.5f);

            s.Gait = GaitFor(i);
            switch (s.Gait)
            {
                case Gait.Tense: s.Cadence = 1.25f; break;
                case Gait.Stomp: s.Cadence = 1.1f; s.Bob = Math.Max(s.Bob, 1.2f); break;
                case Gait.Easy: s.Cadence = 0.9f; break;
                case Gait.Heavy: s.Cadence = 0.85f; s.Bob = Math.Min(s.Bob, 0.4f); break;
                case Gait.Cold: s.Cadence = 1.15f; s.Bob = 0.5f; break;
                case Gait.Cane: s.Cadence = 0.8f; break;
                case Gait.Limp: s.Cadence = 0.85f; s.Sway = 1f; break;
                case Gait.Tipsy: s.Cadence = 0.9f; s.Sway = 2f; break;
                case Gait.Run: s.Cadence = 1f; break;   // koşu hızı JobRunner'dan; kare koşu karesi
            }

            // K6 parçacık koşulları.
            s.Sweat = i.Outdoors && i.Temperature >= SweatFrom && (i.Work == WorkContext.Heavy || i.Work == WorkContext.Bend || i.Running);
            s.Breath = i.Outdoors && i.Temperature < BreathBelow;
            return s;
        }

        /// <summary>Öncelik: içkili &gt; koşu &gt; baston &gt; topallama &gt; ağır &gt; üşüme &gt; hâl biçimleri.</summary>
        public static Gait GaitFor(in AnimInputs i)
        {
            if (i.Stage == AgeStage.Baby) return Gait.Normal;
            if (i.Tipsy && i.Stage != AgeStage.Child && i.Stage != AgeStage.Teen) return Gait.Tipsy;
            if (i.Running) return Gait.Run;
            if (i.Stage == AgeStage.Elder && (i.KneeTrouble || i.HasCane)) return Gait.Cane;
            if (i.LegInjury) return Gait.Limp;
            if (i.Hal == Hal.Sad || i.Energy < TiredBelow || i.LoadRatio > 1f) return Gait.Heavy;
            if (i.Outdoors && i.Warmth < 40f && i.Temperature < 5f) return Gait.Cold;
            switch (i.Hal)
            {
                case Hal.Cheerful:
                case Hal.Eager:
                case Hal.Proud: return Gait.Cheerful;
                case Hal.Tense: return Gait.Tense;
                case Hal.Cross: return Gait.Stomp;
                case Hal.Peaceful: return Gait.Easy;
                default: return Gait.Normal;
            }
        }

        /// <summary>Yürüyüşün kare anahtarı (iş dosyaları 69); yoksa null ve normal yürüme karesi oynar.</summary>
        public static string WalkKey(Gait g)
        {
            switch (g)
            {
                case Gait.Cheerful: return "walk_happy";
                case Gait.Heavy: return "walk_heavy";
                case Gait.Run: return "run";
                case Gait.Cane: return "walk_cane";
                default: return null;
            }
        }

        /// <summary>Durma karesinin anahtarı: hâl durması (iş 69 dalga 2); Sakin'de null (bugünkü durma).</summary>
        public static string IdleKey(Hal h, AgeStage stage)
        {
            if (stage == AgeStage.Baby) return null;
            if (stage == AgeStage.Child) return "idle_fidget";                       // 69 S4: çocukta kıpır durma
            switch (h)
            {
                case Hal.Peaceful: return "idle_peaceful";
                case Hal.Cheerful: return "idle_cheerful";
                case Hal.Eager: return "idle_eager";
                case Hal.Proud: return "idle_proud";
                case Hal.Tense: return "idle_tense";
                case Hal.Sad: return "idle_sad";
                case Hal.Cross: return "idle_cross";
                default: return null;
            }
        }

        private static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
