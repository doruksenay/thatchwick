using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Koşu (Project belgesi 69, karar S3, 21.11.2026): acil durumda (kavgadan kaçış, ebe çağrısı, <see cref="Job.Hurry"/>
    /// işaretli iş), çocuğun oyununda ve oyuncu emriyle uzun yolda koşulur. Atletizm yürüyüşü hızlandırdığı gibi koşuyu da
    /// hızlandırır (çarpanlar çarpılır). Bebek, yaşlı, gebe, bacağı yaralı, dizi tutmayan ve çok yorgun koşmaz.
    /// </summary>
    public static class RunRules
    {
        /// <summary>Koşu hızı yürümenin bu katı.</summary>
        public const double RunSpeed = 1.6;
        /// <summary>Oyuncu emrinde koşulan en kısa yol (karo).</summary>
        public const int LongWalkTiles = 14;
        /// <summary>Çocuk oyununda koşulan en kısa yol (karo).</summary>
        public const int ChildRunTiles = 4;
        public const float MinEnergy = 20f;

        public static bool CanRun(Character c)
        {
            if (c == null || c.Stage == AgeStage.Baby || c.Stage == AgeStage.Elder) return false;
            if (c.IsPregnant || c.WalkFactor < 1.0 || c.Needs.Get(NeedType.Energy) < MinEnergy) return false;
            if (!LoadRules.CanRun(c.LoadRatio)) return false;   // aşırı yükte koşulmaz (59 §7.3)
            if (c.Injury != null && AnimStyleRules.IsLegInjury(c.Injury.Kind, c.Injury.Zone)) return false;
            return true;
        }

        /// <summary>Bu iş için, <paramref name="pathTiles"/> karoluk yolda koşulur mu. <paramref name="playerCommand"/>: oyuncunun verdiği emir.</summary>
        public static bool ShouldRun(Character c, Job job, int pathTiles, bool playerCommand)
        {
            if (job == null || pathTiles <= 0 || !CanRun(c)) return false;
            if (job.Hurry) return true;
            if (job.Kind == JobKind.Social && job.Action == SocialAction.Midwife) return true;
            if (c.Stage == AgeStage.Child && pathTiles >= ChildRunTiles
                && (job.Reason == NeedType.Fun || job.Reason == NeedType.Social)) return true;
            return playerCommand && pathTiles >= LongWalkTiles;
        }

        /// <summary>Oyuncu emri: otonom ihtiyaç ya da görev işi değil, bunalım değil.</summary>
        public static bool IsPlayerCommand(Job job, bool autonomousRunner) =>
            job != null && !autonomousRunner && job.Reason == null && !job.IsDuty && job.Kind != JobKind.Breakdown;
    }
}
