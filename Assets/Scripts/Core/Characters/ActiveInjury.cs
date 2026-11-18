namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// İyileşmekte olan yara (kaza). İyileşme günü gelince InjurySystem iz kalıp kalmayacağını belirler.
    /// Doküman 14, bölüm 3F.
    /// </summary>
    public sealed class ActiveInjury
    {
        /// <summary>Kaza türü (InjuryCatalog kimliği): axe, burn, animal, fall, sickle, child_fall.</summary>
        public string Kind { get; set; }
        public string Zone { get; set; }
        /// <summary>İz kalırsa desen: split, scratch, burn, bite.</summary>
        public string Pattern { get; set; }
        public double StartDay { get; set; }
        public double HealDay { get; set; }
        /// <summary>Derin yara: iyileşince iz kesin kalır.</summary>
        public bool Deep { get; set; }
        /// <summary>Şifalı merhem sürüldü ya da hekim sardı: iz şansı üçte bire iner.</summary>
        public bool Treated { get; set; }
        /// <summary>Hanedan geçmişine ve iz kaydına yazılan neden ("baltayla elini kesti").</summary>
        public string Reason { get; set; }
    }
}
