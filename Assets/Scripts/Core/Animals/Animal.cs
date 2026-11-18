using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    public enum AnimalDeath { None = 0, Starvation = 1, OldAge = 2, Sickness = 3 }

    /// <summary>
    /// Bir hayvan: barınağının çevresinde gezinir, barınağın yem stoğundan yer, tok ve erişkinken
    /// barınağa ürün bırakır. Haritada engel değildir (karakterler içinden geçer).
    /// </summary>
    public sealed class Animal
    {
        public int Id { get; }
        public AnimalSpeciesDef Species { get; }
        public string Name { get; set; }
        public GridPos Position { get; internal set; }
        /// <summary>Yürürken gidilen sonraki karo; duruyorsa null (görünüm ara değerlemesi için).</summary>
        public GridPos? NextTile { get; internal set; }
        public float StepProgress { get; internal set; }
        public int HomeItemId { get; internal set; }
        /// <summary>0-100 tokluk.</summary>
        public float Fullness { get; internal set; } = 100f;
        public double AgeDays { get; internal set; }
        public double ProduceProgress { get; internal set; }
        public double HungryDays { get; internal set; }
        public bool IsDead { get; internal set; }
        public AnimalDeath CauseOfDeath { get; internal set; }
        public bool IsMature => AgeDays >= Species.MatureDays;
        /// <summary>
        /// Bu hayvanın ömrü: tür ömrünün %85-115'i, kimlikten türetilir (aynı gün alınan hayvanlar aynı gün ölmez;
        /// kayda ek alan gerekmez, yüklemede değişmez).
        /// </summary>
        public double LifespanDays => Species.LifespanDays * LifespanFactor(Id);

        internal static double LifespanFactor(int id)
        {
            unchecked
            {
                uint h = (uint)id;          // murmur3 karıştırıcısı: ardışık kimlikler de dağınık çıkar
                h ^= h >> 16; h *= 0x85EBCA6Bu;
                h ^= h >> 13; h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return 0.85 + 0.30 * ((h & 0xFFFF) / 65535.0);
            }
        }
        /// <summary>0-100 dostluk: iyi bakılan ve sevilen hayvan daha çok ve ara sıra ekstra ürün verir.</summary>
        public float Friendship { get; internal set; }
        /// <summary>Son sevilmeden bu kadar gün geçmeden yeniden sevilemez (günde bir kez).</summary>
        public double PetCooldownDays { get; internal set; }

        internal GridPos Target;
        internal double WaitDays;
        // Oda ve otlak içinde yürüyüş yolu (hedefe kadar karolar).
        internal List<GridPos> Path;

        /// <summary>Takılı eyer (eşya kimliği); yoksa null.</summary>
        public string Saddle { get; internal set; }
        /// <summary>Üstündeki binicinin karakter kimliği (0 = binilmiyor).</summary>
        public int RiderId { get; internal set; }
        /// <summary>Barınak konforu 0-100 (gün başında hesaplanır): ürün hızı ve dostluk artışını etkiler.</summary>
        public float Comfort { get; internal set; } = 50f;
        /// <summary>Hasta: ürün vermez, yavrulamaz, aynı barınaktakilere bulaşabilir; merhemle iyileşir (Adım 42).</summary>
        public bool IsSick { get; internal set; }
        /// <summary>Kaç gündür hasta; tedavisiz AnimalSystem.SickDeathDays'e ulaşırsa ölür.</summary>
        public double SickDays { get; internal set; }
        /// <summary>Üşüyor: barınağı soğuk (ya da kışın açıkta). Ürünü yavaşlar, dostluğu azalır, hastalık riski artar.</summary>
        public bool IsCold { get; internal set; }
        /// <summary>Art arda üşüdüğü gün sayısı.</summary>
        public int ColdDays { get; internal set; }

        /// <summary>Kısa sağlık durumu (arayüz için): "Sağlıklı", "Üşüyor", "Hasta (2 gün)".</summary>
        public string HealthText => IsSick ? $"Hasta ({System.Math.Floor(SickDays):0} gün)" : IsCold ? "Üşüyor" : "Sağlıklı";

        /// <summary>Binilebilir: binek türü, erişkin ve eyerli.</summary>
        public bool CanBeRidden => Species.Rideable && IsMature && Saddle != null;

        public Animal(int id, AnimalSpeciesDef species, GridPos position, int homeItemId)
        {
            Id = id;
            Species = species;
            Name = species.DisplayName;
            Position = position;
            Target = position;
            HomeItemId = homeItemId;
        }
    }
}
