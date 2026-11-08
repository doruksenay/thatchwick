using System;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// Üreten eşyalar (tavuk kümesi, Adım 42): her gün ürün hazırlanır; kışın üretim yavaşlar. Hazır olunca
    /// "Yumurta topla" gibi RequiresReady etkileşimi yapılabilir. Yem/zaman ayrıntısı veri tarafında.
    /// </summary>
    public sealed class LivestockSystem
    {
        public const float WinterRate = 0.5f;
        private readonly WorldState _world;

        /// <summary>Ürün yeni hazır olduğunda.</summary>
        public event Action<ItemInstance> BecameReady;

        public LivestockSystem(WorldState world) { _world = world ?? throw new ArgumentNullException(nameof(world)); }

        public void Tick(double gameDays, Season season)
        {
            if (gameDays <= 0) return;
            float rate = season == Season.Winter ? WinterRate : 1f;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.Produces || it.IsReady) continue;
                it.ReadyProgress = Math.Min(it.Def.ReadyDays, it.ReadyProgress + gameDays * rate);
                if (it.IsReady) BecameReady?.Invoke(it);
            }
        }
    }
}
