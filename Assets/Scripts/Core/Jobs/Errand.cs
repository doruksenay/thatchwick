using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Bir işin ara durağı (Project belgeleri 63, 63a): karakter asıl eşyaya gitmeden önce (ya da iş bitince) buraya yürür,
    /// kısa bir an durur (uzanıp alır, yerine koyar) ve yoluna devam eder. Yürüme gerçek zamandır; iş süresine eklenir.
    /// </summary>
    public sealed class JobStop
    {
        /// <summary>Yanına gidilecek eşya (kanca, raf, sandık, su fıçısı). Null ise <see cref="Goals"/> kullanılır.</summary>
        public ItemInstance At { get; set; }
        /// <summary>Doğrudan hedef karolar (su kıyısı gibi); <see cref="At"/> yoksa.</summary>
        public ICollection<GridPos> Goals { get; set; }
        /// <summary>Balonda yazan adım ("chain.step.&lt;StepId&gt;").</summary>
        public string StepId { get; set; }
        /// <summary>Varınca oynayan poz (reach, pour, bend).</summary>
        public string Pose { get; set; }
        /// <summary>Varınca çalınan ses; yoksa null.</summary>
        public string Sfx { get; set; }
        /// <summary>Bu durağa yürürken elde görünen eşya; yoksa null (el boş).</summary>
        public string Carry { get; set; }
        /// <summary>Varınca beklenen süre (oyun günü, ölçeklenmemiş).</summary>
        public double PauseDays { get; set; } = DefaultPauseDays;
        /// <summary>Duraktaki iş bitince çağrılır (eşyayı al, yerine koy).</summary>
        public Action<Character> OnDone { get; set; }

        /// <summary>Uzanıp alma ya da yerine koyma: 6 dakikalık günde yaklaşık 1 saniye.</summary>
        public const double DefaultPauseDays = 0.003;
    }

    /// <summary>
    /// Bir eşya işinin ayak işleri: önce uğranacak duraklar (kabı al, su doldur, bıçağı al), asıl eşyaya yürürken elde
    /// taşınan eşya ve iş bitince uğranacak duraklar (yerine koy). Yoksa iş eskisi gibi doğrudan eşyada yapılır.
    /// </summary>
    public sealed class Errand
    {
        public List<JobStop> Before { get; } = new List<JobStop>();
        public List<JobStop> After { get; } = new List<JobStop>();
        /// <summary>Son duraktan asıl eşyaya yürürken elde görünen eşya.</summary>
        public string CarryToMain { get; set; }
        /// <summary>İş bitince, kesilince ya da başarısız olunca bir kez çağrılır (ayrılan eşyaları serbest bırak).</summary>
        public Action OnEnd { get; set; }
    }
}
