using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Mood
{
    /// <summary>
    /// Hâl (Project belgesi 68a §3): iç seslerin hangi duyguya yazıldığı. Kartta ve animasyonda görünen duygu budur;
    /// Perişan…Mutlu ölçeği arka planda kalır. Varsayılan Sakin.
    /// </summary>
    public enum Hal
    {
        Calm = 0,       // Sakin
        Peaceful = 1,   // Huzurlu
        Cheerful = 2,   // Neşeli
        Eager = 3,      // Şevkli
        Proud = 4,      // Gururlu
        Tense = 5,      // Gergin
        Sad = 6,        // Kederli
        Cross = 7       // Hırçın
    }

    /// <summary>
    /// Baskın hâlin hesabı (68a §3): her iç ses |değerini| kendi hâline yazar; en yüksek toplam baskındır, eşitlikte en yeni
    /// iç sesin hâli. Hiçbir hâl <see cref="CalmBelow"/>'u geçmiyorsa Sakin.
    /// İç sesin hâli <see cref="MoodletDef.Hal"/> ile verilir; verilmemişse <see cref="DefaultFor"/> eşlemesi kullanılır
    /// (hâl katmanı PR'ı iç ses tablosunu yazınca bu eşleme veriye taşınır).
    /// </summary>
    public static class HalRules
    {
        public const int CalmBelow = 5;
        /// <summary>Kurdeledeki "Pek …" eşiği (68a §3).</summary>
        public const int StrongAt = 30;

        public static Hal Dominant(IReadOnlyList<ActiveMoodlet> active) => Dominant(active, out _);

        public static Hal Dominant(IReadOnlyList<ActiveMoodlet> active, out int score)
        {
            score = 0;
            if (active == null || active.Count == 0) return Hal.Calm;
            var sums = new int[8];
            var lastSeen = new int[8];
            for (int i = 0; i < active.Count; i++)
            {
                var def = active[i].Def;
                if (def == null || def.Value == 0) continue;
                int h = (int)(def.Hal ?? DefaultFor(def.Id, def.Value));
                sums[h] += Math.Abs(def.Value);
                lastSeen[h] = i + 1;
            }
            int best = -1;
            for (int h = 1; h < 8; h++)
            {
                if (sums[h] <= CalmBelow) continue;
                if (best < 0 || sums[h] > sums[best] || (sums[h] == sums[best] && lastSeen[h] > lastSeen[best])) best = h;
            }
            if (best < 0) return Hal.Calm;
            score = sums[best];
            return (Hal)best;
        }

        public static Hal Dominant(MoodState mood) => mood == null ? Hal.Calm : Dominant(mood.Active);

        private static readonly Dictionary<string, Hal> Known = Build();

        /// <summary>
        /// Hâli verilmemiş iç sesin hâli. Bilinen kimlikler tablodan (68a §4.1 örnekleri esas alındı); bilinmeyen olumlu iç
        /// ses Huzurlu, olumsuz Gergin sayılır.
        /// </summary>
        public static Hal DefaultFor(string id, int value)
        {
            if (id != null)
            {
                if (Known.TryGetValue(id, out var h)) return h;
                if (id.StartsWith("injured_", StringComparison.Ordinal)) return Hal.Cross;
                if (id.StartsWith("meal_", StringComparison.Ordinal) || id.StartsWith("harmony", StringComparison.Ordinal)) return Hal.Peaceful;
            }
            return value >= 0 ? Hal.Peaceful : Hal.Tense;
        }

        private static Dictionary<string, Hal> Build()
        {
            var d = new Dictionary<string, Hal>(StringComparer.Ordinal);
            void Put(Hal h, params string[] ids) { foreach (var id in ids) d[id] = h; }
            Put(Hal.Peaceful, MoodCatalog.GoodMeal, MoodCatalog.Rested, MoodCatalog.Clean, MoodCatalog.BrightRoom, MoodCatalog.HotBath,
                MoodCatalog.Relieved, MoodCatalog.PotionCalm, MoodCatalog.WellCared, MoodCatalog.Lulled, MoodCatalog.VariedTable,
                MoodCatalog.Catharsis, MoodCatalog.Understood, MoodCatalog.Unburdened, MoodCatalog.MidwifeCare, MoodCatalog.DoctorVisit);
            Put(Hal.Cheerful, MoodCatalog.HadFun, MoodCatalog.NiceChat, MoodCatalog.Newlywed, MoodCatalog.NewBaby, MoodCatalog.PetLove,
                MoodCatalog.NewFoal, MoodCatalog.Complimented, MoodCatalog.Feast, MoodCatalog.PlayedTavern, MoodCatalog.NewSong,
                MoodCatalog.GotLetter, MoodCatalog.FirstSteps, MoodCatalog.Believed, MoodCatalog.FamilyTale, MoodCatalog.OddChat,
                MoodCatalog.AnimalSpoke, MoodCatalog.Confidant, MoodCatalog.Swim, MoodCatalog.ScarecrowChat, MoodCatalog.NotCrazy);
            Put(Hal.Eager, MoodCatalog.Inspired, MoodCatalog.SharpTools, MoodCatalog.PotionVigor, MoodCatalog.PotionFocus,
                MoodCatalog.VeinFound, MoodCatalog.Hopeful, MoodCatalog.Composed, MoodCatalog.Persuasive);
            Put(Hal.Proud, MoodCatalog.Proud, MoodCatalog.GoodHarvest, MoodCatalog.MasterWork, MoodCatalog.SkillShowcase,
                MoodCatalog.FestivalSpeech, MoodCatalog.CaveInSurvived);
            Put(Hal.Tense, MoodCatalog.Tired, MoodCatalog.Cold, MoodCatalog.Bursting, MoodCatalog.Chilled, MoodCatalog.Wet,
                MoodCatalog.ColdWater, MoodCatalog.Bored, MoodCatalog.Ashamed);
            Put(Hal.Sad, MoodCatalog.Grief, MoodCatalog.AnimalDied, MoodCatalog.CropFrozen, MoodCatalog.Lonely, MoodCatalog.LetDown,
                MoodCatalog.NobodyBelieves, MoodCatalog.Thoughtful, MoodCatalog.Slaughtered);
            Put(Hal.Cross, MoodCatalog.Hungry, MoodCatalog.Dirty, MoodCatalog.Accident, MoodCatalog.Dusty, MoodCatalog.SameMeal);
            return d;
        }
    }
}
