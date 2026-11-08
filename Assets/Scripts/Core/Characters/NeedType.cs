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
        Hygiene = 4   // Hijyen
    }
}
