using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    public enum AnimalDeath { None = 0, Starvation = 1, OldAge = 2 }

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

        internal GridPos Target;
        internal double WaitDays;

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
