using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Depo kapasitesi (kapasite modeli, 08.11.2026): kaynaklar (odun, kütük, şalgam, tohum...) hane
    /// envanterinde sayılır ama toplamı kapasiteyi aşamaz. Kapasite = temel (kulübe) + depo bölgesi
    /// karosu başına birim + yerleştirilmiş depo eşyaları (sandık). Mobilya gibi kaynak olmayan
    /// envanter eşyaları kapasiteye sayılmaz. Kapasite azalırsa (bölge silindi) eldeki kaybolmaz;
    /// yalnızca yenisi eklenemez.
    /// </summary>
    public sealed class StorageService
    {
        public const int DefaultBaseCapacity = 30;
        public const int UnitsPerZoneTile = 10;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;

        public int BaseCapacity { get; }

        /// <summary>Hanenin en iyi arabacısının seviyesi; null ise etkisiz.</summary>
        public Func<int> CartingLevel { get; set; }

        public StorageService(WorldState world, Household house, ContentDatabase content, int baseCapacity = DefaultBaseCapacity)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            if (baseCapacity < 0) throw new ArgumentOutOfRangeException(nameof(baseCapacity));
            BaseCapacity = baseCapacity;
        }

        public bool IsResource(string itemId) =>
            _content.Items.TryGet(itemId, out var def) && def.Placement == ItemPlacement.Resource;

        /// <summary>Eşyanın ek depo kapasitesi (sandığa demir kuşak, belge 45); null ise yok.</summary>
        public Func<ItemInstance, int> ExtraStorage { get; set; }

        public int Capacity
        {
            get
            {
                int cap = BaseCapacity + _world.Map.CountZone(ZoneType.Storage) * UnitsPerZoneTile;
                foreach (var item in _world.Items)
                    if (!item.IsDecor) cap += item.Def.Storage + (ExtraStorage != null ? ExtraStorage(item) : 0);
                // Arabacılık: düzenli istif ve taşıma, seviye başına %3 fazla yer.
                if (CartingLevel != null) cap = (int)(cap * SkillNames.Factor(CartingLevel(), 0.03));
                return cap;
            }
        }

        public int Used
        {
            get
            {
                int used = 0;
                foreach (var kv in _house.InventoryList())
                    if (IsResource(kv.Key)) used += kv.Value;
                return used;
            }
        }

        public int Free => Math.Max(0, Capacity - Used);

        /// <summary>Kaynaksa kapasite kadarını ekler, eklenen adedi döner; kaynak değilse hepsini ekler.</summary>
        public int AddUpTo(string itemId, int count)
        {
            if (count <= 0) return 0;
            int add = IsResource(itemId) ? Math.Min(count, Free) : count;
            if (add > 0) _house.AddItem(itemId, add);
            return add;
        }

        public bool HasRoomFor(string itemId, int count = 1) => !IsResource(itemId) || Free >= count;
    }
}
