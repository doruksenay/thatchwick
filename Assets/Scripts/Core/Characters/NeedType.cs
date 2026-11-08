namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Temel ihtiyaçlar. Değer 0 (kritik) ile 100 (tam doygun) arasındadır.
    /// Açlık, Enerji ve Isınma hayatta kalma ihtiyaçlarıdır (0'da ölüm riski ya da kritik durum);
    /// Keyif ve Hijyen konfor ihtiyaçlarıdır: öldürmez, ruh halini (moodlet) etkiler.
    /// </summary>
    public enum NeedType
    {
        Hunger = 0,
        Energy = 1,
        Warmth = 2,
        Fun = 3,      // Keyif
        Hygiene = 4,  // Hijyen
        Social = 5    // Sosyal (sohbet, şenlik; azalması ruh halini düşürür, öldürmez)
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
                case NeedType.Fun: return "Keyif";
                case NeedType.Hygiene: return "Hijyen";
                default: return "Sosyal";
            }
        }
    }
}
