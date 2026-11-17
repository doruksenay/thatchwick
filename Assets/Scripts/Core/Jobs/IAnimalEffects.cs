using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>Bir hayvanla yapılan iş (oyuncu ya da görevli aile üyesi).</summary>
    public enum AnimalAction
    {
        /// <summary>Tek hayvanı sev (dostluk).</summary>
        Pet = 0,
        /// <summary>Tımar et (binek): dostluk ve konfor.</summary>
        Brush = 1,
        /// <summary>Envanterdeki eyeri tak.</summary>
        Saddle = 2,
        /// <summary>Eyeri çıkar (envantere döner).</summary>
        Unsaddle = 3,
        /// <summary>Bin.</summary>
        Mount = 4
    }

    /// <summary>Hayvan işlerinin kuralları ve etkileri (Core.Animals.AnimalCare uygular).</summary>
    public interface IAnimalEffects
    {
        /// <summary>Hayvanın şu anki karosu; yoksa (öldü, satıldı) null.</summary>
        GridPos? PositionOf(int animalId);
        /// <summary>Yapılabiliyorsa null, yoksa okunur neden.</summary>
        string CanDo(Character actor, int animalId, AnimalAction action);
        double Duration(AnimalAction action);
        void Apply(Character actor, int animalId, AnimalAction action);
    }

    /// <summary>Karoda yapılan iş (çapalama): oyuncunun ve görevlilerin tarlayı canlı modda hazırlaması.</summary>
    public enum TileAction
    {
        /// <summary>Çimi çapalayıp ekilebilir toprağa çevir.</summary>
        Till = 0
    }

    /// <summary>Karo işlerinin kuralları ve etkileri (Core.Work.FarmWork uygular).</summary>
    public interface ITileWork
    {
        string CanDo(Character actor, GridPos tile, TileAction action);
        double Duration(Character actor, TileAction action);
        void Apply(Character actor, GridPos tile, TileAction action);
    }
}
