using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Jobs
{
    /// <summary>Karakterler arası işlerin kuralları ve etkileri (Core.Family.FamilyService uygular).</summary>
    public interface ISocialEffects
    {
        Character Find(int characterId);
        /// <summary>Yapılabiliyorsa null, yoksa okunur neden.</summary>
        string CanDo(Character actor, Character target, SocialAction action);
        double Duration(SocialAction action);
        void Apply(Character actor, Character target, SocialAction action);
        /// <summary>Otonom karakterin bakması gereken bakmakla yükümlü olduğu kişi (aç bebek); yoksa null.</summary>
        Character DependentNeedingCare(Character carer);
        /// <summary>Sosyal ihtiyacı giderirken sohbet edilecek en uygun kişi (yakın, yaşayan, bebek değil); yoksa null.</summary>
        Character FriendFor(Character character);
    }
}
