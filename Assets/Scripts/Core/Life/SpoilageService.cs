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
    /// Yiyecek depolara otomatik yerleşir (17.11.2026): en çabuk bozulan en soğuk yere; her deponun hızı bulunduğu yerin
    /// sıcaklığından ve kendi koruma çarpanından gelir, buzluk buz varken çok soğuktur. Sığmayan açıkta (x1) kalır.
    /// Tuzlama/kurutma ürünleri hiç çürümez. Süresi dolan taze yiyecek atılır.
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
        /// <summary>Derin kiler (kat -2) ek çarpanı: bozulma bodrumun yarısı (23. belge 3.4).</summary>
        public double DeepCellarFactor = 0.5;
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

        /// <summary>
        /// En iyi deponun bozulma çarpanı (1 = normal; mahzen 0,4 ise 0,4) x (1 - ek azaltma). Yiyecek artık her depoya
        /// sığdığı kadar yerleşir (Plan); bu değer en iyi yerin hızıdır.
        /// </summary>
        public double RateMultiplier()
        {
            double best = 1.0;
            foreach (var slot in Slots()) if (slot.Rate < best) best = slot.Rate;
            return best * Extra();
        }

        private double Extra() => 1.0 - (ExtraReduction != null ? Math.Max(0.0, Math.Min(0.8, ExtraReduction())) : 0.0);

        // ---------- yiyecek yerleşimi (ambar, kiler, mahzen, buzluk) ----------

        /// <summary>Sıcaklık bandının bozulma çarpanı: ≤2°C 0,25 · ≤8 0,5 · ≤14 0,75 · ≤22 1 · daha sıcak 1,4.</summary>
        public static double TemperatureFactor(float t) => t <= 2f ? 0.25 : t <= 8f ? 0.5 : t <= 14f ? 0.75 : t <= 22f ? 1.0 : 1.4;

        /// <summary>Buzlu buzluğun çarpanı ve buzsuz/kilersiz yiyeceğin (açıkta, sıcak mutfak) çarpanı.</summary>
        public const double IceRate = 0.2, OpenRate = 1.0;
        /// <summary>Kapasitesi yazılmamış kiler/buzluk eşyasının yiyecek kapasitesi.</summary>
        public const int DefaultPreserveCapacity = 60;

        /// <summary>Bir yiyecek deposu: eşya, kapasite, bozulma çarpanı ve (varsa) sıcaklığı.</summary>
        public sealed class FoodSlot
        {
            public ItemInstance Item;
            public int Capacity;
            public double Rate;
            public float? Temperature;
            public bool Iced;
            public int Used;
            public string Name => Item.Def.DisplayName;
        }

        /// <summary>Yerleşim: hangi yiyecek nerede, kaçı açıkta, yiyecek başına ortalama bozulma hızı.</summary>
        public sealed class FoodPlan
        {
            public readonly List<FoodSlot> Slots = new List<FoodSlot>();
            public readonly Dictionary<string, List<KeyValuePair<FoodSlot, int>>> Placed = new Dictionary<string, List<KeyValuePair<FoodSlot, int>>>();
            public readonly Dictionary<string, int> Open = new Dictionary<string, int>();
            public readonly Dictionary<string, double> Rate = new Dictionary<string, double>();
            public double RateOf(string id) => Rate.TryGetValue(id, out var r) ? r : OpenRate;
        }

        // Yiyecek tutan eşyalar: depo (sandık, raf, ambar), kiler/mahzen (Preserves) ve buzluk. Çarpan, bulunduğu yerin
        // sıcaklığından (verilmemişse eski kural: bodrum serin, zemin kat normal) ve eşyanın kendi koruma çarpanından gelir.
        private List<FoodSlot> Slots()
        {
            var list = new List<FoodSlot>();
            foreach (var it in _world.Items)
            {
                if (it.IsDecor) continue;
                var d = it.Def;
                if (d.Storage <= 0 && d.Preserves <= 0f && !d.IsIceBox && d.FoodCapacity <= 0) continue;
                int cap = d.FoodCapacity > 0 ? d.FoodCapacity : d.Storage > 0 ? d.Storage : DefaultPreserveCapacity;
                double rate;
                float? temp = TemperatureAt != null ? TemperatureAt(it.Position) : (float?)null;
                if (temp.HasValue)
                {
                    rate = TemperatureFactor(temp.Value);
                    if (it.Position.Z < 0 && temp.Value <= CoolLimit) rate = Math.Min(rate, BasementFactor);   // toprak altı serin
                }
                else rate = it.Position.Z < 0 ? BasementFactor : 1.0;
                if (it.Position.Z <= -2) rate *= DeepCellarFactor;           // derin kiler (arsa kazısı): bodrumun yarısı
                if (d.Preserves > 0f) rate *= d.Preserves;
                bool iced = d.IsIceBox && it.FuelDays > 0;
                if (iced) rate = Math.Min(rate, IceRate);
                list.Add(new FoodSlot { Item = it, Capacity = cap, Rate = rate, Temperature = temp, Iced = iced });
            }
            // En soğuk (en yavaş bozan) depo önce; eşitse büyük olan.
            list.Sort((a, b) => a.Rate != b.Rate ? a.Rate.CompareTo(b.Rate) : b.Capacity.CompareTo(a.Capacity));
            return list;
        }

        /// <summary>
        /// Ortak envanterdeki taze yiyeceği depolara otomatik yerleştirir: en çabuk bozulan (süt, et) en soğuk yere,
        /// dayanıklı olan (soğan, kabak) kalan yere; sığmayan açıkta kalır. Bozulmayan yiyecek (tuzlu, kurutulmuş) yer kaplamaz.
        /// </summary>
        public FoodPlan Plan()
        {
            var plan = new FoodPlan();
            plan.Slots.AddRange(Slots());
            var foods = new List<ItemDef>();
            foreach (var d in _content.Items.All) if (d.SpoilDays > 0f && !d.HasTag("ice") && _house.Count(d.Id) > 0) foods.Add(d);
            foods.Sort((a, b) => a.SpoilDays != b.SpoilDays ? a.SpoilDays.CompareTo(b.SpoilDays) : string.CompareOrdinal(a.Id, b.Id));
            double extra = Extra();
            foreach (var d in foods)
            {
                int left = _house.Count(d.Id);
                int total = left;
                double weighted = 0;
                var placed = new List<KeyValuePair<FoodSlot, int>>();
                foreach (var slot in plan.Slots)
                {
                    if (left <= 0) break;
                    int put = Math.Min(left, slot.Capacity - slot.Used);
                    if (put <= 0) continue;
                    slot.Used += put;
                    left -= put;
                    weighted += put * slot.Rate;
                    placed.Add(new KeyValuePair<FoodSlot, int>(slot, put));
                }
                weighted += left * OpenRate;
                if (left > 0) plan.Open[d.Id] = left;
                plan.Placed[d.Id] = placed;
                plan.Rate[d.Id] = weighted / total * extra;
            }
            return plan;
        }

        public const double PlanRefreshDays = 0.01;
        private FoodPlan _plan;
        private double _planAge;

        /// <summary>Son yerleşim (arayüz için); yoksa hesaplanır.</summary>
        public FoodPlan CurrentPlan => _plan ?? (_plan = Plan());

        /// <summary>Buzlukların buzu erir: dondurucu soğukta hiç, serinde yavaş, sıcakta hızlı (gün başına buz günü).</summary>
        public static double MeltPerDay(float t) => t <= 0f ? 0.0 : t <= 10f ? 0.5 : 1.0 + (t - 10f) / 10f;

        private void Melt(double gameDays)
        {
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.IsIceBox || it.FuelDays <= 0) continue;
                float t = TemperatureAt != null ? TemperatureAt(it.Position) : 12f;
                it.FuelDays = Math.Max(0, it.FuelDays - gameDays * MeltPerDay(t));
            }
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
            Melt(gameDays);
            foreach (var def in _content.Items.All) if (def.SpoilDays > 0f) Reconcile(def);
            // Yerleşim her karede değil, ~15 oyun dakikasında bir yenilenir (yeni hasat da en geç o zaman yerleşir).
            _planAge += gameDays;
            if (_plan == null || _planAge >= PlanRefreshDays) { _plan = Plan(); _planAge = 0; }
            var plan = _plan;
            foreach (var def in _content.Items.All)
            {
                if (def.SpoilDays <= 0f) continue;
                double rate = plan.RateOf(def.Id) * (plan.Rate.ContainsKey(def.Id) ? 1.0 : Extra());
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
            double rate = Math.Max(0.01, CurrentPlan.RateOf(itemId) * (_house.Count(itemId) > 0 ? 1.0 : Extra()));
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
