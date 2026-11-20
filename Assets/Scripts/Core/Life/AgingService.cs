using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Mahzende olgunlaşma (Project belgesi 53, karar 4): peynir, pastırma, şarap, sirke ve bal şarabı mahzende bekledikçe
    /// kalite basamağı atlar (eşyanın <see cref="ItemDef.AgeStepDays"/> günü başına +1, en çok Has). Süre uzadıkça kalite artar.
    /// Dede şarabı: bir kuşağın mahzene koyduğu şarap iki kuşak sonra (2 × <see cref="ItemQuality.GenerationDays"/>) Yadigâr olur.
    /// Mahzen: bodrum katında (Z &lt; 0) bir depo, kiler ya da fıçı. Mahzen yoksa ürün bekler ama olgunlaşmaz.
    /// Partiler envanterle (kimlik + kalite) eşitlenir: fazlası yeni parti, eksiği en yeni partiden düşer (eski fıçı korunur).
    /// </summary>
    public sealed class AgingService
    {
        public const string WineId = "item.wine_new";

        private sealed class Batch { public int Count; public double Age; public double LaidDay; }

        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly WorldState _world;
        private readonly Dictionary<string, List<Batch>> _batches = new Dictionary<string, List<Batch>>(StringComparer.Ordinal);

        /// <summary>Bugünün günü (dede şarabının kuşak sayımı); null ise 0.</summary>
        public Func<double> Today { get; set; }
        /// <summary>Yükselen ürün (kimlik, yeni kalite, adet).</summary>
        public event Action<string, int, int> Matured;

        public AgingService(ContentDatabase content, Household house, WorldState world)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        private static string Key(string id, int q) => ItemKey.StockKey(id, q);

        private List<Batch> Batches(string key)
        {
            if (!_batches.TryGetValue(key, out var list)) { list = new List<Batch>(); _batches[key] = list; }
            return list;
        }

        /// <summary>Hanede mahzen var mı: bodrum katında depo, kiler ya da fıçı.</summary>
        public bool HasCellar()
        {
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Position.Z < 0 && (it.Def.Storage > 0 || it.Def.Preserves > 0f || it.Def.FoodCapacity > 0)) return true;
            return false;
        }

        private double Now => Today?.Invoke() ?? 0;

        private void Reconcile(ItemDef def)
        {
            var counts = _house.QualityCounts(def.Id);
            for (int q = 0; q < counts.Length; q++)
            {
                string key = Key(def.Id, q);
                if (counts[q] <= 0 && !_batches.ContainsKey(key)) continue;
                var list = Batches(key);
                int tracked = 0;
                foreach (var b in list) tracked += b.Count;
                if (counts[q] > tracked) list.Add(new Batch { Count = counts[q] - tracked, Age = 0, LaidDay = Now });
                else if (counts[q] < tracked)
                {
                    int remove = tracked - counts[q];
                    for (int i = list.Count - 1; i >= 0 && remove > 0; i--)   // en yeni fıçıdan
                    {
                        int take = Math.Min(remove, list[i].Count);
                        list[i].Count -= take; remove -= take;
                        if (list[i].Count <= 0) list.RemoveAt(i);
                    }
                }
                if (list.Count == 0) _batches.Remove(key);
            }
        }

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            bool cellar = HasCellar();
            foreach (var def in _content.Items.All)
            {
                if (def.AgeStepDays <= 0f) continue;
                Reconcile(def);
                if (!cellar) continue;
                for (int q = ItemQuality.Common; q <= ItemQuality.Superior; q++)
                {
                    if (!_batches.TryGetValue(Key(def.Id, q), out var list)) continue;
                    for (int i = 0; i < list.Count; )
                    {
                        var b = list[i];
                        b.Age += gameDays;
                        int target = q;
                        if (q < ItemQuality.NormalMax && b.Age >= def.AgeStepDays) target = q + 1;
                        else if (q == ItemQuality.Superior && def.Id == WineId && Now - b.LaidDay >= 2 * ItemQuality.GenerationDays) target = ItemQuality.Heirloom;
                        if (target == q) { i++; continue; }
                        int moved = _house.Requalify(def.Id, q, target, b.Count);
                        list.RemoveAt(i);
                        if (moved <= 0) continue;
                        Batches(Key(def.Id, target)).Add(new Batch { Count = moved, Age = 0, LaidDay = b.LaidDay });
                        Matured?.Invoke(def.Id, target, moved);
                    }
                }
            }
        }

        /// <summary>Bir ürünün en eski partisinin mahzende geçirdiği gün (arayüz: "12 gündür mahzende"); yoksa 0.</summary>
        public double OldestDays(string itemId)
        {
            double best = 0;
            double now = Now;
            for (int q = 0; q <= ItemQuality.Max; q++)
                if (_batches.TryGetValue(Key(itemId, q), out var list))
                    foreach (var b in list) best = Math.Max(best, now - b.LaidDay);
            return best;
        }

        public AgingBatchSave[] Export()
        {
            var list = new List<AgingBatchSave>();
            foreach (var kv in _batches)
            {
                var k = ItemKey.FromStockKey(kv.Key);
                foreach (var b in kv.Value)
                    if (b.Count > 0) list.Add(new AgingBatchSave { item = k.Id, quality = k.Quality, count = b.Count, age = b.Age, laid = b.LaidDay });
            }
            return list.ToArray();
        }

        public void Import(AgingBatchSave[] saved)
        {
            _batches.Clear();
            if (saved == null) return;
            foreach (var s in saved)
                if (s != null && s.count > 0 && _content.Items.Contains(s.item))
                    Batches(Key(s.item, s.quality)).Add(new Batch { Count = s.count, Age = Math.Max(0, s.age), LaidDay = s.laid });
        }
    }

    [Serializable]
    public sealed class AgingBatchSave
    {
        public string item;
        public int quality, count;
        public double age, laid;
    }
}
