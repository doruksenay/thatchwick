using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>Harita ve üzerindeki eşyalar. Eşya kapladığı karoyu geçilemez yapar.</summary>
    public sealed class WorldState
    {
        private readonly List<ItemInstance> _items = new List<ItemInstance>();
        private int _nextId = 1;

        public GridMap Map { get; }
        public IReadOnlyList<ItemInstance> Items => _items;

        public WorldState(GridMap map)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public ItemInstance PlaceItem(ItemDef def, GridPos position)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (!Map.IsWalkable(position))
                throw new InvalidOperationException($"{position} karosuna eşya konamaz (dolu ya da ızgara dışı).");

            var item = new ItemInstance(_nextId++, def, position);
            Map.SetBlocked(position, true);
            _items.Add(item);
            return item;
        }

        /// <summary>Kimliğe göre eşya; yoksa null.</summary>
        public ItemInstance GetItem(int id)
        {
            foreach (var item in _items)
                if (item.Id == id) return item;
            return null;
        }
    }
}
