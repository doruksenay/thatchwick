using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Bir eşyanın tanımladığı etkileşim ("akıllı nesne"): eşya kendi etkileşimini söyler,
    /// karakter sistemi yalnızca uygular. Örnek: yatak = enerji +60, 0.3 oyun günü.
    /// </summary>
    public sealed class InteractionDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public NeedType Need { get; set; }
        public float Restore { get; set; }
        public double DurationDays { get; set; }
    }
}
