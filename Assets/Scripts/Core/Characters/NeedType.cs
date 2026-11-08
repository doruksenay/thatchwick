namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// İhtiyaçlar. Değer 0 (kritik) ile 100 (tam doygun) arasındadır.
    /// Hayatta kalma ihtiyaçları (Açlık, Enerji, Isınma) ölüme götürür; Sosyal ve Eğlence ruh halini
    /// etkiler (Sims'teki "keyif" tarafı), ölüm sebebi değildir.
    /// </summary>
    public enum NeedType
    {
        Hunger = 0,
        Energy = 1,
        Warmth = 2,
        Social = 3,
        Fun = 4
    }

    public static class NeedKinds
    {
        /// <summary>Açlık, Enerji ve Isınma: kritik düşünce karakter ölebilir, bakım işi onlara öncelik verir.</summary>
        public static bool IsSurvival(NeedType need) =>
            need == NeedType.Hunger || need == NeedType.Energy || need == NeedType.Warmth;

        public static string DisplayName(NeedType need)
        {
            switch (need)
            {
                case NeedType.Hunger: return "Açlık";
                case NeedType.Energy: return "Enerji";
                case NeedType.Warmth: return "Isınma";
                case NeedType.Social: return "Sosyal";
                default: return "Eğlence";
            }
        }
    }
}
