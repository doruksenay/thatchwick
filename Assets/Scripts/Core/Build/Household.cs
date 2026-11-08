using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Hanenin ortak kasası: akçe ve envanter (yerleştirilmemiş eşyalar, tanım kimliğine göre sayı).
    /// Yapı modunda satın alınanlar buraya girer, buradan yerleştirilir.
    /// </summary>
    public sealed class Household
    {
        private readonly Dictionary<string, int> _inventory = new Dictionary<string, int>(StringComparer.Ordinal);

        public int Akce { get; private set; }

        public event Action Changed;

        public Household(int startingAkce = 0)
        {
            if (startingAkce < 0) throw new ArgumentOutOfRangeException(nameof(startingAkce));
            Akce = startingAkce;
        }

        public void AddAkce(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Akce += amount;
            Changed?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Akce < amount) return false;
            Akce -= amount;
            Changed?.Invoke();
            return true;
        }

        public int Count(string itemId) => itemId != null && _inventory.TryGetValue(itemId, out int n) ? n : 0;

        public void AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Eşya kimliği boş.", nameof(itemId));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            _inventory[itemId] = Count(itemId) + count;
            Changed?.Invoke();
        }

        public bool TryTakeItem(string itemId)
        {
            int n = Count(itemId);
            if (n <= 0) return false;
            if (n == 1) _inventory.Remove(itemId); else _inventory[itemId] = n - 1;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Envanterdeki (kimlik, adet) çiftleri, kimliğe göre sıralı.</summary>
        public List<KeyValuePair<string, int>> InventoryList()
        {
            var list = new List<KeyValuePair<string, int>>(_inventory);
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }
    }
}
