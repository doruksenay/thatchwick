namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Temel ihtiyaçlar. Değer 0 (kritik) ile 100 (tam doygun) arasındadır.
    /// Karakter (ve ileride hijyen, keyif gibi) ihtiyaçlar buraya eklenir.
    /// </summary>
    public enum NeedType
    {
        Hunger = 0,
        Energy = 1,
        Warmth = 2
    }
}
