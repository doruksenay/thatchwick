using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Hanenin ortak kasası: akçe ve envanter (yerleştirilmemiş eşyalar, tanım kimliğine göre sayı).
    /// Yapı modunda satın alınanlar buraya girer, buradan yerleştirilir.
    /// </summary>
    public sealed class Household
    {
        private readonly Dictionary<string, int> _inventory = new Dictionary<string, int>(StringComparer.Ordinal);
        // Envanterdeki hasarlı ve onarılmış adetler (kalan sağlamdır). Çöken raftan düşen eşya hasarlı gelir (27-ev-dekorasyonu).
        private readonly Dictionary<string, int> _damaged = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _repaired = new Dictionary<string, int>(StringComparer.Ordinal);

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

        public void AddItem(string itemId, int count = 1) => AddItem(itemId, count, ItemCondition.Sound);

        public void AddItem(string itemId, int count, ItemCondition condition)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Eşya kimliği boş.", nameof(itemId));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            _inventory[itemId] = Count(itemId) + count;
            if (condition == ItemCondition.Damaged) Bump(_damaged, itemId, count);
            else if (condition == ItemCondition.Repaired) Bump(_repaired, itemId, count);
            Changed?.Invoke();
        }

        /// <summary>Bir adet alır (önce sağlam, sonra onarılmış, en son hasarlı).</summary>
        public bool TryTakeItem(string itemId) => TryTakeItem(itemId, out _);

        public bool TryTakeItem(string itemId, out ItemCondition taken)
        {
            taken = ItemCondition.Sound;
            int n = Count(itemId);
            if (n <= 0) return false;
            if (SoundCount(itemId) > 0) taken = ItemCondition.Sound;
            else if (RepairedCount(itemId) > 0) taken = ItemCondition.Repaired;
            else taken = ItemCondition.Damaged;
            return TakeCondition(itemId, taken);
        }

        /// <summary>Belirli durumdaki bir adedi alır.</summary>
        public bool TakeCondition(string itemId, ItemCondition condition)
        {
            int n = Count(itemId);
            if (n <= 0) return false;
            if (condition == ItemCondition.Sound && SoundCount(itemId) <= 0) return false;
            if (condition == ItemCondition.Damaged && DamagedCount(itemId) <= 0) return false;
            if (condition == ItemCondition.Repaired && RepairedCount(itemId) <= 0) return false;
            if (n == 1) _inventory.Remove(itemId); else _inventory[itemId] = n - 1;
            if (condition == ItemCondition.Damaged) Bump(_damaged, itemId, -1);
            else if (condition == ItemCondition.Repaired) Bump(_repaired, itemId, -1);
            Changed?.Invoke();
            return true;
        }

        public int DamagedCount(string itemId) => itemId != null && _damaged.TryGetValue(itemId, out int n) ? n : 0;
        public int RepairedCount(string itemId) => itemId != null && _repaired.TryGetValue(itemId, out int n) ? n : 0;
        public int SoundCount(string itemId) => Math.Max(0, Count(itemId) - DamagedCount(itemId) - RepairedCount(itemId));

        /// <summary>Hasarlı bir adedi onarılmış yapar.</summary>
        public bool RepairOne(string itemId)
        {
            if (DamagedCount(itemId) <= 0) return false;
            Bump(_damaged, itemId, -1);
            Bump(_repaired, itemId, 1);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Hasarlı eşyaların (kimlik, adet) listesi, kimliğe göre sıralı.</summary>
        public List<KeyValuePair<string, int>> DamagedList() => Sorted(_damaged);
        public List<KeyValuePair<string, int>> RepairedList() => Sorted(_repaired);

        private static List<KeyValuePair<string, int>> Sorted(Dictionary<string, int> d)
        {
            var list = new List<KeyValuePair<string, int>>(d);
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        private static void Bump(Dictionary<string, int> d, string id, int delta)
        {
            d.TryGetValue(id, out int n);
            n += delta;
            if (n <= 0) d.Remove(id); else d[id] = n;
        }

        /// <summary>Kayıttan yükleme: envanterdeki adetlerin bir kısmını hasarlı ya da onarılmış işaretler (adet aşılmaz).</summary>
        public void RestoreCondition(string itemId, int count, ItemCondition condition)
        {
            if (count <= 0 || condition == ItemCondition.Sound) return;
            int room = SoundCount(itemId);
            int n = Math.Min(room, count);
            if (n <= 0) return;
            Bump(condition == ItemCondition.Damaged ? _damaged : _repaired, itemId, n);
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
