using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Collections
{
    /// <summary>
    /// En uzun süredir kullanılmayanı atan sabit kapasiteli önbellek (performans incelemesi 20.11.2026).
    /// Kapasite dolunca yalnızca en eski kayıt atılır (eskiden önbelleğin tamamı boşaltılıyordu, bu da karakter
    /// karelerinin topluca yeniden üretilmesine ve takılmaya yol açıyordu). Atılan değer <c>evicted</c> ile bildirilir.
    /// </summary>
    public sealed class LruCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _map;
        private readonly LinkedList<(TKey key, TValue value)> _order = new LinkedList<(TKey, TValue)>();
        private readonly Action<TKey, TValue> _evicted;

        public LruCache(int capacity, Action<TKey, TValue> evicted = null, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
            _evicted = evicted;
            _map = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(comparer ?? EqualityComparer<TKey>.Default);
        }

        public int Capacity { get; }
        public int Count => _map.Count;

        public bool TryGet(TKey key, out TValue value)
        {
            if (_map.TryGetValue(key, out var node))
            {
                _order.Remove(node);
                _order.AddFirst(node);
                value = node.Value.value;
                return true;
            }
            value = default;
            return false;
        }

        public void Set(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                _order.Remove(existing);
                _map.Remove(key);
                _evicted?.Invoke(existing.Value.key, existing.Value.value);
            }
            var node = _order.AddFirst((key, value));
            _map[key] = node;
            while (_map.Count > Capacity)
            {
                var last = _order.Last;
                _order.RemoveLast();
                _map.Remove(last.Value.key);
                _evicted?.Invoke(last.Value.key, last.Value.value);
            }
        }

        public void Clear()
        {
            if (_evicted != null) foreach (var e in _order) _evicted(e.key, e.value);
            _order.Clear();
            _map.Clear();
        }
    }
}
