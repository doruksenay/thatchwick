using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Hanenin ortak kasası: akçe ve envanter (yerleştirilmemiş eşyalar; kimlik, kalite, durum ve kaynağa göre yığınlar).
    /// Yapı modunda satın alınanlar buraya girer, buradan yerleştirilir.
    /// </summary>
    public sealed class Household
    {
        // Yığınlar (Project belgesi 53 §5.1): kimlik → (anahtar, adet) listesi. Anahtar kalite, durum (27: hasarlı, onarılmış),
        // pazardan alınmış mı (D2: "Kendi emeğimiz" yalnızca hanenin kendi ürününe) ve ustayı taşır. Kimlik başına toplam önbellekte.
        private sealed class Stack { public ItemKey Key; public int Count; }
        private readonly Dictionary<string, List<Stack>> _stacks = new Dictionary<string, List<Stack>>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _totals = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _shopUnlocks = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Köyde açılan satış (ör. "glassworks": çömlekçi cam eşya satar). Yeni açıldıysa true.</summary>
        public bool UnlockShop(string key) => !string.IsNullOrEmpty(key) && _shopUnlocks.Add(key);
        public bool HasShopUnlock(string key) => key != null && _shopUnlocks.Contains(key);
        public IEnumerable<string> ShopUnlocks => _shopUnlocks;

        /// <summary>Gezgin tüccar bugün köyde mi (pazar günü; D4 Kervan Odası eşyaları). Kayda yazılmaz, her gün yeniden hesaplanır.</summary>
        public bool MerchantInVillage { get; set; }

        public int Akce { get; private set; }

        // Katalog siparişleri (belge 18 karar 1, belge 42): parası ödenmiş, marangozun ertesi sabah getireceği eşyalar.
        private readonly Dictionary<string, int> _orders = new Dictionary<string, int>();

        public IEnumerable<KeyValuePair<string, int>> Orders => _orders;
        public int OrderedCount(string itemId) => itemId != null && _orders.TryGetValue(itemId, out int n) ? n : 0;

        public void AddOrder(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return;
            _orders[itemId] = OrderedCount(itemId) + count;
            Changed?.Invoke();
        }

        /// <summary>Bekleyen siparişleri envantere koyar (yeni günün sabahı); teslim edilenleri döndürür.</summary>
        public List<KeyValuePair<string, int>> DeliverOrders()
        {
            var done = new List<KeyValuePair<string, int>>(_orders);
            _orders.Clear();
            foreach (var kv in done) AddItem(kv.Key, kv.Value);
            return done;
        }

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

        /// <summary>Envanter adetleri her değiştiğinde artar; dinleyiciler (bozulma partileri) yalnızca değişince eşitler.</summary>
        public int InventoryVersion { get; private set; }

        public int Count(string itemId) => itemId != null && _totals.TryGetValue(itemId, out int n) ? n : 0;

        /// <summary>En az bu kalitedeki adet.</summary>
        public int Count(string itemId, int minQuality)
        {
            if (minQuality <= 0) return Count(itemId);
            int n = 0;
            if (itemId != null && _stacks.TryGetValue(itemId, out var list))
                foreach (var st in list) if (st.Key.Quality >= minQuality) n += st.Count;
            return n;
        }

        /// <summary>Kalite başına adet (indeks = kalite, 0..4).</summary>
        public int[] QualityCounts(string itemId)
        {
            var a = new int[ItemQuality.Max + 1];
            if (itemId != null && _stacks.TryGetValue(itemId, out var list))
                foreach (var st in list) a[st.Key.Quality] += st.Count;
            return a;
        }

        /// <summary>En iyi kalite (eşya yoksa -1).</summary>
        public int BestQuality(string itemId)
        {
            int best = -1;
            if (itemId != null && _stacks.TryGetValue(itemId, out var list))
                foreach (var st in list) if (st.Count > 0 && st.Key.Quality > best) best = st.Key.Quality;
            return best;
        }

        /// <summary>Bir eşyanın yığınları (kaliteye, sonra duruma göre sıralı).</summary>
        public List<KeyValuePair<ItemKey, int>> Stacks(string itemId)
        {
            var result = new List<KeyValuePair<ItemKey, int>>();
            if (itemId != null && _stacks.TryGetValue(itemId, out var list))
                foreach (var st in list) if (st.Count > 0) result.Add(new KeyValuePair<ItemKey, int>(st.Key, st.Count));
            result.Sort((a, b) => a.Key.Quality != b.Key.Quality ? a.Key.Quality.CompareTo(b.Key.Quality)
                : a.Key.Condition != b.Key.Condition ? a.Key.Condition.CompareTo(b.Key.Condition)
                : a.Key.Bought != b.Key.Bought ? a.Key.Bought.CompareTo(b.Key.Bought) : a.Key.Maker.CompareTo(b.Key.Maker));
            return result;
        }

        /// <summary>Bütün yığınlar (kayıt), kimliğe göre sıralı.</summary>
        public List<KeyValuePair<ItemKey, int>> AllStacks()
        {
            var ids = new List<string>(_stacks.Keys);
            ids.Sort(string.CompareOrdinal);
            var result = new List<KeyValuePair<ItemKey, int>>();
            foreach (var id in ids) result.AddRange(Stacks(id));
            return result;
        }

        public void AddItem(string itemId, int count = 1) => AddItem(itemId, count, ItemCondition.Sound);

        public void AddItem(string itemId, int count, ItemCondition condition) => Add(new ItemKey(itemId, 0, condition), count);

        /// <summary>Kaliteli ekleme (53): kalite taşımayan eşyada da anahtar olduğu gibi tutulur; kalitesizliği içerik belirler.</summary>
        public void AddItem(string itemId, int count, int quality) => Add(new ItemKey(itemId, quality), count);

        public void Add(ItemKey key, int count)
        {
            if (string.IsNullOrWhiteSpace(key.Id)) throw new ArgumentException("Eşya kimliği boş.", nameof(key));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            Bump(key, count);
            Changed?.Invoke();
        }

        private void Bump(ItemKey key, int delta)
        {
            if (!_stacks.TryGetValue(key.Id, out var list)) { list = new List<Stack>(2); _stacks[key.Id] = list; }
            Stack found = null;
            foreach (var st in list) if (st.Key.Equals(key)) { found = st; break; }
            if (found == null) { if (delta <= 0) return; found = new Stack { Key = key }; list.Add(found); }
            found.Count += delta;
            InventoryVersion++;   // yığınların tek değişme noktası (ekleme, alma, kalite/durum değişimi)
            if (found.Count <= 0) list.Remove(found);
            int total = Count(key.Id) + delta;
            if (total <= 0 || list.Count == 0) { _totals.Remove(key.Id); if (list.Count == 0) _stacks.Remove(key.Id); }
            else _totals[key.Id] = total;
        }

        // Harcama sırası: önce sağlam, sonra onarılmış, en son hasarlı; içinde önce hanenin kendi ürünü, sonra kalite politikası.
        private Stack Pick(string itemId, TakePolicy policy, int minQuality, ItemCondition? condition, bool? bought)
        {
            if (itemId == null || !_stacks.TryGetValue(itemId, out var list)) return null;
            Stack best = null;
            foreach (var st in list)
            {
                if (st.Count <= 0 || st.Key.Quality < minQuality) continue;
                if (condition.HasValue && st.Key.Condition != condition.Value) continue;
                if (bought.HasValue && st.Key.Bought != bought.Value) continue;
                if (best == null || Better(st.Key, best.Key, policy)) best = st;
            }
            return best;
        }

        private static int ConditionRank(ItemCondition c) => c == ItemCondition.Sound ? 0 : c == ItemCondition.Repaired ? 1 : 2;

        private static bool Better(ItemKey a, ItemKey b, TakePolicy policy)
        {
            int ca = ConditionRank(a.Condition), cb = ConditionRank(b.Condition);
            if (ca != cb) return ca < cb;
            if (a.Bought != b.Bought) return !a.Bought;
            if (a.Quality != b.Quality) return policy == TakePolicy.Highest ? a.Quality > b.Quality : a.Quality < b.Quality;
            return a.Maker < b.Maker;
        }

        private bool TakeFrom(Stack st, out ItemKey taken)
        {
            taken = default;
            if (st == null) return false;
            taken = st.Key;
            Bump(st.Key, -1);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Bir adet alır (önce sağlam, sonra onarılmış, en son hasarlı; en düşük kalite önce).</summary>
        public bool TryTakeItem(string itemId) => TryTake(itemId, TakePolicy.Lowest, out _);

        public bool TryTakeItem(string itemId, out ItemCondition taken)
        {
            bool ok = TryTake(itemId, TakePolicy.Lowest, out var key);
            taken = ok ? key.Condition : ItemCondition.Sound;
            return ok;
        }

        /// <summary>Politikayla bir adet alır; alınan yığının anahtarı döner (kalite, usta).</summary>
        public bool TryTake(string itemId, TakePolicy policy, out ItemKey taken, int minQuality = 0) =>
            TakeFrom(Pick(itemId, policy, minQuality, null, null), out taken);

        /// <summary>Tam olarak bu anahtardan bir adet alır.</summary>
        public bool TryTakeKey(ItemKey key)
        {
            if (key.Id == null || !_stacks.TryGetValue(key.Id, out var list)) return false;
            foreach (var st in list) if (st.Key.Equals(key) && st.Count > 0) return TakeFrom(st, out _);
            return false;
        }

        /// <summary>Tam olarak bu kalitedeki bir adedi alır (bozulma partisi, olgunlaşma).</summary>
        public bool TryTakeQuality(string itemId, int quality)
        {
            if (itemId == null || !_stacks.TryGetValue(itemId, out var list)) return false;
            Stack best = null;
            foreach (var st in list)
                if (st.Count > 0 && st.Key.Quality == quality && (best == null || Better(st.Key, best.Key, TakePolicy.Lowest))) best = st;
            return TakeFrom(best, out _);
        }

        /// <summary>Belirli durumdaki bir adedi alır.</summary>
        public bool TakeCondition(string itemId, ItemCondition condition) =>
            TakeFrom(Pick(itemId, TakePolicy.Lowest, 0, condition, null), out _);

        /// <summary>
        /// Olgunlaşma ya da yükseltme: <paramref name="from"/> kalitesindeki en çok <paramref name="count"/> adedi
        /// <paramref name="to"/> kalitesine taşır (durum ve kaynak korunur). Taşınan adet döner.
        /// </summary>
        public int Requalify(string itemId, int from, int to, int count)
        {
            if (count <= 0 || from == to || itemId == null || !_stacks.TryGetValue(itemId, out var list)) return 0;
            int moved = 0;
            foreach (var st in new List<Stack>(list))
            {
                if (moved >= count) break;
                if (st.Key.Quality != from || st.Count <= 0) continue;
                int n = Math.Min(count - moved, st.Count);
                var key = st.Key;
                Bump(key, -n);
                Bump(key.WithQuality(to), n);
                moved += n;
            }
            if (moved > 0) Changed?.Invoke();
            return moved;
        }

        private int CountWhere(string itemId, Func<ItemKey, bool> pred)
        {
            int n = 0;
            if (itemId != null && _stacks.TryGetValue(itemId, out var list))
                foreach (var st in list) if (pred(st.Key)) n += st.Count;
            return n;
        }

        public int DamagedCount(string itemId) => CountWhere(itemId, k => k.Condition == ItemCondition.Damaged);
        public int RepairedCount(string itemId) => CountWhere(itemId, k => k.Condition == ItemCondition.Repaired);
        public int SoundCount(string itemId) => CountWhere(itemId, k => k.Condition == ItemCondition.Sound);

        /// <summary>Hasarlı bir adedi onarılmış yapar (en iyi kalitedeki önce).</summary>
        public bool RepairOne(string itemId)
        {
            var st = Pick(itemId, TakePolicy.Highest, 0, ItemCondition.Damaged, null);
            if (st == null) return false;
            var key = st.Key;
            Bump(key, -1);
            Bump(key.WithCondition(ItemCondition.Repaired), 1);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Hasarlı eşyaların (kimlik, adet) listesi, kimliğe göre sıralı.</summary>
        public List<KeyValuePair<string, int>> DamagedList() => ListWhere(k => k.Condition == ItemCondition.Damaged);
        public List<KeyValuePair<string, int>> RepairedList() => ListWhere(k => k.Condition == ItemCondition.Repaired);

        private List<KeyValuePair<string, int>> ListWhere(Func<ItemKey, bool> pred)
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var id in _stacks.Keys) { int n = CountWhere(id, pred); if (n > 0) list.Add(new KeyValuePair<string, int>(id, n)); }
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        // Bir durumdan ötekine (ya da alınmışa) en çok n adet taşır; kaynak yığınlar politikaya göre seçilir.
        private int Convert(string itemId, int count, Func<ItemKey, bool> from, Func<ItemKey, ItemKey> to)
        {
            if (count <= 0 || itemId == null || !_stacks.TryGetValue(itemId, out var list)) return 0;
            int moved = 0;
            var sources = new List<Stack>(list);
            sources.Sort((a, b) => a.Key.Quality.CompareTo(b.Key.Quality));
            foreach (var st in sources)
            {
                if (moved >= count) break;
                if (st.Count <= 0 || !from(st.Key)) continue;
                int n = Math.Min(count - moved, st.Count);
                var key = st.Key;
                Bump(key, -n);
                Bump(to(key), n);
                moved += n;
            }
            return moved;
        }

        /// <summary>Kayıttan yükleme: envanterdeki sağlam adetlerin bir kısmını hasarlı ya da onarılmış işaretler (adet aşılmaz).</summary>
        public void RestoreCondition(string itemId, int count, ItemCondition condition)
        {
            if (count <= 0 || condition == ItemCondition.Sound) return;
            Convert(itemId, count, k => k.Condition == ItemCondition.Sound, k => k.WithCondition(condition));
        }

        // ---- pazardan alınan ve hanenin kendi ürünü (D2) ----

        /// <summary>Envantere pazardan alınarak giren adetleri işaretler (AddItem'dan sonra çağrılır; en düşük kalite önce).</summary>
        public void MarkBought(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return;
            if (Convert(itemId, count, k => !k.Bought, k => k.WithBought(true)) > 0) Changed?.Invoke();
        }

        /// <summary>Envanterdeki pazardan alınmış adet.</summary>
        public int BoughtCount(string itemId) => CountWhere(itemId, k => k.Bought);
        /// <summary>Envanterdeki hanenin kendi ürünü (toplam − alınmış).</summary>
        public int OwnCount(string itemId) => Count(itemId) - BoughtCount(itemId);

        /// <summary>Bir adet alır; önce hanenin kendi ürününden. own: alınan adet kendi ürünü mü.</summary>
        public bool TryTakePreferOwn(string itemId, out bool own) => TryTakePreferOwn(itemId, out own, out _);

        /// <summary>Kendi ürünü önce; içinde politikaya göre kalite (doldurulan dekor en iyisini sergiler). Alınan anahtar döner.</summary>
        public bool TryTakePreferOwn(string itemId, out bool own, out ItemKey taken, TakePolicy policy = TakePolicy.Lowest)
        {
            own = false;
            var st = Pick(itemId, policy, 0, null, null);   // Better() kendi ürününü önce seçer
            if (!TakeFrom(st, out taken)) return false;
            own = !taken.Bought;
            return true;
        }

        /// <summary>Alınmış adetler (kayıt).</summary>
        public List<KeyValuePair<string, int>> BoughtList() => ListWhere(k => k.Bought);

        /// <summary>Envanterdeki (kimlik, adet) çiftleri, kimliğe göre sıralı.</summary>
        public List<KeyValuePair<string, int>> InventoryList()
        {
            var list = new List<KeyValuePair<string, int>>(_totals);
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }
    }
}
