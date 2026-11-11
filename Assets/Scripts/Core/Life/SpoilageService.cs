using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Save;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yiyecek bozulması ve koruma (Adım 35, T22). Hane envanteri yalnızca adet tutar; bozulma "parti" (adet, yaş)
    /// kayıtlarıyla izlenir ve her turda envanterle eşitlenir (tüketilen en eski partiden düşer, eklenen yeni parti olur).
    /// Kiler/mahzen (Preserves) bozulma hızını azaltır; tuzlama/kurutma ürünleri hiç çürümez. Süresi dolan taze yiyecek atılır.
    /// </summary>
    public sealed class SpoilageService
    {
        private sealed class Batch { public int Count; public double Age; }

        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly WorldState _world;
        private readonly Dictionary<string, List<Batch>> _batches = new Dictionary<string, List<Batch>>();

        /// <summary>Ek bozulma azaltma oranı (köy projesi, özellik); yoksa 0.</summary>
        public Func<float> ExtraReduction { get; set; }

        /// <summary>Bodrum depolarının bozulma çarpanı (yer tutucu; Adım 28).</summary>
        public double BasementFactor = 0.7;
        /// <summary>Bodrum sıcaklığı sağlayıcı (HeatSystem); verilmezse bodrum hep serin sayılır.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }
        /// <summary>Bodrumun serin sayıldığı en yüksek sıcaklık (°C).</summary>
        public float CoolLimit = 14f;

        private bool BasementIsCool(GridPos p) => TemperatureAt == null || TemperatureAt(p) <= CoolLimit;

        /// <summary>(yiyecek kimliği, atılan adet)</summary>
        public event Action<string, int> Spoiled;

        public SpoilageService(ContentDatabase content, Household house, WorldState world)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>Bozulma hız çarpanı (1 = normal; mahzen 0,4 ise 0,4): en iyi kiler x (1 - ek azaltma).</summary>
        public double RateMultiplier()
        {
            double pantry = 1.0;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor) continue;
                if (it.Def.Preserves > 0f && it.Def.Preserves < pantry) pantry = it.Def.Preserves;
                // Bodrumdaki depo/kiler eşyası serin toprakta yiyeceği daha geç bozar (Adım: bodrum mahzeni).
                if (it.Position.Z < 0 && (it.Def.Storage > 0 || it.Def.Preserves > 0f) && BasementIsCool(it.Position))
                {
                    double v = (it.Def.Preserves > 0f ? it.Def.Preserves : 1.0) * BasementFactor;
                    if (v < pantry) pantry = v;
                }
            }
            double extra = ExtraReduction != null ? Math.Max(0.0, Math.Min(0.8, ExtraReduction())) : 0.0;
            return pantry * (1.0 - extra);
        }

        private List<Batch> Batches(string id)
        {
            if (!_batches.TryGetValue(id, out var list)) { list = new List<Batch>(); _batches[id] = list; }
            return list;
        }

        // Envanter ile partileri eşitle: fazlalık yeni parti, eksiklik en eski partilerden düşer.
        private void Reconcile(ItemDef def)
        {
            var list = Batches(def.Id);
            int tracked = 0;
            foreach (var b in list) tracked += b.Count;
            int actual = _house.Count(def.Id);
            if (actual > tracked) list.Add(new Batch { Count = actual - tracked, Age = 0 });
            else if (actual < tracked)
            {
                int remove = tracked - actual;
                for (int i = 0; i < list.Count && remove > 0; )
                {
                    int take = Math.Min(remove, list[i].Count);
                    list[i].Count -= take; remove -= take;
                    if (list[i].Count <= 0) list.RemoveAt(i); else i++;
                }
            }
        }

        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));
            double rate = RateMultiplier();
            foreach (var def in _content.Items.All)
            {
                if (def.SpoilDays <= 0f) continue;
                Reconcile(def);
                var list = Batches(def.Id);
                int spoiled = 0;
                for (int i = 0; i < list.Count; )
                {
                    list[i].Age += gameDays * rate;
                    if (list[i].Age >= def.SpoilDays) { spoiled += list[i].Count; list.RemoveAt(i); }
                    else i++;
                }
                if (spoiled > 0)
                {
                    int removed = 0;
                    for (int i = 0; i < spoiled; i++) if (_house.TryTakeItem(def.Id)) removed++;
                    if (removed > 0) Spoiled?.Invoke(def.Id, removed);
                }
            }
        }

        /// <summary>Bir yiyeceğin en taze ve en eski partisinin kalan günü (kiler çarpanıyla); izlenmiyorsa -1.</summary>
        public double DaysLeftOldest(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def) || def.SpoilDays <= 0f) return -1;
            Reconcile(def);
            var list = Batches(itemId);
            if (list.Count == 0) return -1;
            double rate = Math.Max(0.01, RateMultiplier());
            return (def.SpoilDays - list[0].Age) / rate;
        }

        // ---- kayıt ----

        public List<SpoilBatchSave> Export()
        {
            var result = new List<SpoilBatchSave>();
            foreach (var kv in _batches)
                foreach (var b in kv.Value)
                    if (b.Count > 0) result.Add(new SpoilBatchSave { item = kv.Key, count = b.Count, age = b.Age });
            return result;
        }

        public void Restore(IEnumerable<SpoilBatchSave> saved)
        {
            _batches.Clear();
            if (saved == null) return;
            foreach (var s in saved)
                if (s != null && s.count > 0 && _content.Items.Contains(s.item))
                    Batches(s.item).Add(new Batch { Count = s.count, Age = Math.Max(0, s.age) });
        }
    }
}
