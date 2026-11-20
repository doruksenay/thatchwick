using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>Kompostun kalitesi (belge 57c §3): ham, olgun, kara toprak.</summary>
    public enum CompostGrade { Raw = 0, Mature = 1, BlackSoil = 2 }

    /// <summary>Bir kompost yığınının durumu.</summary>
    public sealed class CompostHeap
    {
        public int ItemId;
        /// <summary>Yaş girdi (artık, çürük, pislik, ot), kuru girdi (saman, yaprak, altlık) ve kül (birim).</summary>
        public double Wet, Dry, Ash;
        /// <summary>Yaş girdinin içindeki insan pisliği (birim): sıcak yığın bunu zararsız kılar.</summary>
        public double Filth;
        /// <summary>Olgunlaşma 0-1.</summary>
        public double Progress;
        /// <summary>Son çevirme günü (hiç çevrilmediyse çok eski).</summary>
        public double LastTurn = -1000;
        /// <summary>İyi geçen (dolu, dengeli, çevrilmiş) ve toplam olgunlaşma günleri: kalite bunların oranından çıkar.</summary>
        public double GoodDays, TotalDays;
        /// <summary>Bugün kokuyor mu (fazla yaş ya da bakımsız).</summary>
        public bool Smelly;
        /// <summary>Sıcak evrede mi (dolu, dengeli, çevrilmiş): buharı tüter, pislikten gelen risk silinir.</summary>
        public bool Hot;
        /// <summary>Alınmayı bekleyen kompost: ham, olgun, kara toprak.</summary>
        public int[] Ready = new int[3];
        /// <summary>Bekleyen ham kompost pislikli mi.</summary>
        public bool ReadyFilth;

        public double Total => Wet + Dry + Ash;
        public bool IsEmpty => Total <= 0.0001;
        public int ReadyTotal => Ready[0] + Ready[1] + Ready[2];
        /// <summary>Yaş payı (0-1); boş yığında 0,5.</summary>
        public double WetShare => Wet + Dry <= 0.0001 ? 0.5 : Wet / (Wet + Dry);
    }

    /// <summary>
    /// Kompost süreci, gübre yığını (ayrı stok), solucan kasası, yaprak ağılı ve şerbet fıçısı (belge 57b §3-4, 57c §3-4).
    /// Kompost artık anlık bir tarif değil: yaş ve kuru girdi dengesi, çevirme ve sıcak evre kaliteyi belirler.
    /// </summary>
    public sealed class CompostService : IItemReserve
    {
        public const string HeapTag = "compost", BaysTag = "compost_bays", TumblerTag = "compost_tumbler", WormTag = "worm_bin",
            LeafTag = "leaf_bin", SlurryTag = "slurry_barrel", DungHeapTag = "dung_heap", ShredderTag = "chaff_cutter";
        public const string ManureId = "item.manure", RawId = "item.compost_raw", MatureId = "item.compost", BlackId = "item.black_soil",
            LeavesId = "item.leaves", LeafMouldId = "item.leaf_mould", AshId = "item.ash", WormId = "item.viv_worm";

        /// <summary>Yaş girdiler ve birim değerleri.</summary>
        public static readonly KeyValuePair<string, int>[] WetInputs =
        {
            new KeyValuePair<string, int>("item.kitchen_scraps", 1), new KeyValuePair<string, int>("item.weeds", 1),
            new KeyValuePair<string, int>("item.herb", 1), new KeyValuePair<string, int>("item.seaweed", 1),
            new KeyValuePair<string, int>(ManureId, 2)
        };
        /// <summary>Kuru girdiler ve birim değerleri.</summary>
        public static readonly KeyValuePair<string, int>[] DryInputs =
        {
            new KeyValuePair<string, int>(LeavesId, 1), new KeyValuePair<string, int>("item.straw", 1),
            new KeyValuePair<string, int>("item.pomace", 1), new KeyValuePair<string, int>("item.coffee_spent", 1),
            new KeyValuePair<string, int>("item.sawdust", 1)
        };

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Dictionary<int, CompostHeap> _heaps = new Dictionary<int, CompostHeap>();
        private readonly Dictionary<int, double> _timers = new Dictionary<int, double>();   // yaprak ağılı ve şerbet fıçısı: başlangıç günü
        private bool _rawFilthStock;
        private int _dungUnits;

        public YardConfig Config { get; }

        public CompostService(WorldState world, Household house, ContentDatabase content, YardConfig config = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            Config = config ?? new YardConfig();
        }

        /// <summary>Toplam oyun günü.</summary>
        public Func<double> Now { get; set; }
        private double Today => Now != null ? Now() : 0.0;
        /// <summary>Karo sıcaklığı (°C); null ise 15.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }
        /// <summary>Düzenek güç alıyor mu (su çarkı, yel değirmeni): döner fıçı ve saman kıyıcı için.</summary>
        public Func<ItemInstance, bool> Powered { get; set; }
        /// <summary>Yığının yanında eşeleyen kümes hayvanı var mı (çevirmeyi onlar yapar).</summary>
        public Func<ItemInstance, bool> PoultryNear { get; set; }

        /// <summary>Kompost hazır (yığın, kalite, adet).</summary>
        public event Action<ItemInstance, CompostGrade, int> Finished;

        // ---------- yığınlar ----------

        public bool IsHeap(ItemInstance item) => item != null && !item.IsDecor && item.Def.HasTag(HeapTag);
        public bool IsTumbler(ItemInstance item) => item != null && item.Def.HasTag(TumblerTag);
        public bool IsBays(ItemInstance item) => item != null && item.Def.HasTag(BaysTag);

        public CompostHeap HeapOf(ItemInstance item)
        {
            if (!IsHeap(item)) return null;
            if (!_heaps.TryGetValue(item.Id, out var h)) _heaps[item.Id] = h = new CompostHeap { ItemId = item.Id };
            return h;
        }

        public int CapacityOf(ItemInstance item) =>
            IsTumbler(item) ? Config.TumblerCapacity : Config.HeapCapacity * (IsBays(item) ? Config.BaysFactor : 1);
        private int MinFillOf(ItemInstance item) => IsTumbler(item) ? Config.TumblerMinFill : Config.HeapMinFill;
        public double RoomIn(ItemInstance item) { var h = HeapOf(item); return h == null ? 0 : Math.Max(0, CapacityOf(item) - h.Total); }

        /// <summary>Girdi alabilen ilk yığın (çürük ve pislik oraya gider); yoksa null.</summary>
        public ItemInstance HeapWithRoom(double units = 1)
        {
            foreach (var it in _world.Items) if (IsHeap(it) && RoomIn(it) >= units) return it;
            return null;
        }

        public bool AnyHeap { get { foreach (var it in _world.Items) if (IsHeap(it)) return true; return false; } }

        /// <summary>Girdi ekler; sığan birimi döner. Olgunlaşması ilerlemiş tek gözlü yığına taze girdi süreci geri atar.</summary>
        public double Add(ItemInstance heapItem, double wet, double dry, double ash = 0, double filth = 0)
        {
            var h = HeapOf(heapItem);
            if (h == null) return 0;
            double want = wet + dry + ash;
            if (want <= 0) return 0;
            double room = RoomIn(heapItem);
            double share = Math.Min(1.0, room / want);
            if (share <= 0) return 0;
            if (h.IsEmpty) { h.Progress = 0; h.GoodDays = 0; h.TotalDays = 0; }
            else if (!IsBays(heapItem) && h.Progress > 0.5) h.Progress = Math.Max(0, h.Progress - 0.2 * share);
            h.Wet += wet * share; h.Dry += dry * share; h.Ash += ash * share; h.Filth += filth * share;
            _world.NotifyChanged(heapItem.Id);
            return want * share;
        }

        /// <summary>Envanterden en çok <see cref="YardConfig.AddUnits"/> birim yaş ya da kuru girdi alıp yığına koyar; konan birim.</summary>
        public int AddFromInventory(ItemInstance heapItem, bool wet)
        {
            int added = 0;
            foreach (var kv in wet ? WetInputs : DryInputs)
                while (added < Config.AddUnits && _house.Count(kv.Key) > 0 && RoomIn(heapItem) >= kv.Value)
                {
                    if (!_house.TryTakeItem(kv.Key)) break;
                    Add(heapItem, wet ? kv.Value : 0, wet ? 0 : kv.Value);
                    added += kv.Value;
                }
            return added;
        }

        /// <summary>Envanterde bu türden girdi var mı.</summary>
        public bool HasInput(bool wet)
        {
            foreach (var kv in wet ? WetInputs : DryInputs) if (_house.Count(kv.Key) > 0) return true;
            return false;
        }

        public bool AddAsh(ItemInstance heapItem)
        {
            if (RoomIn(heapItem) < 1 || !_house.TryTakeItem(AshId)) return false;
            Add(heapItem, 0, 0, 1);
            return true;
        }

        public bool CanTurn(ItemInstance heapItem) { var h = HeapOf(heapItem); return h != null && !h.IsEmpty && Today - h.LastTurn >= 0.5; }

        public void Turn(ItemInstance heapItem)
        {
            var h = HeapOf(heapItem);
            if (h == null) return;
            h.LastTurn = Today;
            h.Smelly = false;
        }

        /// <summary>Yığın çevrilmeyi bekliyor mu (görevli için): dolu ve çevirme tazeliğini yitirmiş.</summary>
        public bool NeedsTurn(ItemInstance heapItem)
        {
            var h = HeapOf(heapItem);
            if (h == null || h.IsEmpty || AutoTurned(heapItem)) return false;
            return Today - h.LastTurn >= Config.TurnFreshDays;
        }

        private bool AutoTurned(ItemInstance item) =>
            (IsTumbler(item) && Powered != null && Powered(item)) || (PoultryNear != null && PoultryNear(item));

        private bool ShredderNear(ItemInstance heap)
        {
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.HasTag(ShredderTag)) continue;
                if (it.Position.Z == heap.Position.Z && Math.Abs(it.Position.X - heap.Position.X) <= 2 && Math.Abs(it.Position.Y - heap.Position.Y) <= 2) return true;
            }
            return false;
        }

        /// <summary>Hazır kompostu envantere alır; (ham, olgun, kara toprak) adetleri.</summary>
        public int[] Take(ItemInstance heapItem)
        {
            var h = HeapOf(heapItem);
            var got = new int[3];
            if (h == null) return got;
            string[] ids = { RawId, MatureId, BlackId };
            for (int q = 0; q < 3; q++)
            {
                if (h.Ready[q] <= 0) continue;
                string id = _content.Items.Contains(ids[q]) ? ids[q] : MatureId;
                _house.AddItem(id, h.Ready[q]);
                got[q] = h.Ready[q];
                h.Ready[q] = 0;
            }
            if (got[0] > 0 && h.ReadyFilth) _rawFilthStock = true;
            h.ReadyFilth = false;
            _world.NotifyChanged(heapItem.Id);
            return got;
        }

        /// <summary>Envanterdeki ham kompost pislik içeriyor mu (çiğ yenen ekinde risk). Ham kompost bitince sıfırlanır.</summary>
        public bool RawCompostHasFilth()
        {
            if (_rawFilthStock && _house.Count(RawId) <= 0) _rawFilthStock = false;
            return _rawFilthStock;
        }

        /// <summary>Koku kademesi: dengeli ve bakımlı dolu yığın ılık havada 1 (sinek), kokan yığın 2; boş yığın 0.</summary>
        public int SmellTier(ItemInstance item)
        {
            if (item == null || item.IsDecor) return 0;
            if (item.Def.HasTag(DungHeapTag)) return item.Fill > 0 ? 1 : 0;
            if (!IsHeap(item) || IsTumbler(item)) return 0;
            var h = HeapOf(item);
            if (h.IsEmpty) return 0;
            return h.Smelly ? 2 : 1;
        }

        private double TempFactor(GridPos p)
        {
            float t = TemperatureAt != null ? TemperatureAt(p) : 15f;
            return t <= 0f ? 0.0 : t <= 8f ? 0.4 : t <= 15f ? 0.8 : 1.0;
        }

        /// <summary>Yeni gün: yığınlar olgunlaşır, solucan kasası ve yaprak ağılı işler.</summary>
        public void OnNewDay()
        {
            double now = Today;
            foreach (var item in new List<ItemInstance>(_world.Items))
            {
                if (item.IsDecor) continue;
                if (IsHeap(item)) StepHeap(item, now);
                else if (item.Def.HasTag(WormTag)) StepWorms(item);
                else if (item.Def.HasTag(LeafTag)) StepLeaves(item, now);
            }
            // Dünyadan kalkan yığınların kaydı düşer.
            var gone = new List<int>();
            foreach (var id in _heaps.Keys) if (_world.GetItem(id) == null) gone.Add(id);
            foreach (var id in gone) _heaps.Remove(id);
        }

        private void StepHeap(ItemInstance item, double now)
        {
            var h = HeapOf(item);
            if (h.IsEmpty) { h.Smelly = false; h.Hot = false; return; }
            double share = h.WetShare;
            bool balanced = share >= Config.BalanceLow && share <= Config.BalanceHigh;
            bool tooWet = share > Config.BalanceHigh;
            bool turned = AutoTurned(item) || now - h.LastTurn < Config.TurnFreshDays;
            bool neglected = !turned && now - h.LastTurn >= Config.NeglectDays;
            bool full = h.Total >= MinFillOf(item);
            double temp = TempFactor(item.Position);

            double speed = balanced ? 1.0 : tooWet ? Config.WetSpeed : Config.DrySpeed;
            if (turned) speed *= Config.TurnedSpeed; else if (neglected) speed *= Config.NeglectedSpeed;
            if (ShredderNear(item)) speed *= Config.ShredderSpeed;
            if (IsBays(item)) speed *= Config.BaysSpeed;
            double days = !full ? Config.ColdHeapDays : IsTumbler(item) ? Config.TumblerDays : Config.HeapDays;

            h.Hot = full && balanced && turned && temp > 0;
            h.Smelly = temp > 0 && (tooWet || neglected) && !IsTumbler(item);
            if (temp > 0)
            {
                h.TotalDays += 1;
                if (h.Hot) h.GoodDays += 1;
            }
            h.Progress += speed * temp / days;
            if (h.Progress >= 1.0) Finish(item, h);
        }

        private void Finish(ItemInstance item, CompostHeap h)
        {
            double good = h.TotalDays <= 0 ? 0 : h.GoodDays / h.TotalDays;
            int grade = good >= Config.BlackSoilShare ? 2 : good >= Config.MatureShare ? 1 : 0;
            if (h.Total > 0 && h.Ash / h.Total > Config.AshLimit) grade = Math.Max(0, grade - 1);
            int count = Math.Max(1, (int)Math.Round((h.Wet + h.Dry) * Config.OutputShare));
            h.Ready[grade] += count;
            if (grade == 0 && h.Filth > 0) h.ReadyFilth = true;   // soğuk yığın pisliği zararsız kılamaz
            h.Wet = h.Dry = h.Ash = h.Filth = 0;
            h.Progress = 0; h.GoodDays = 0; h.TotalDays = 0; h.Smelly = false; h.Hot = false;
            _world.NotifyChanged(item.Id);
            Finished?.Invoke(item, (CompostGrade)grade, count);
        }

        // ---------- solucan kasası, yaprak ağılı, şerbet fıçısı ----------

        /// <summary>Solucan kasası: her gün bir yaş girdi yer; üç girdide bir olgun kompost ve bir solucan verir. Bakım istemez, kokmaz.</summary>
        private void StepWorms(ItemInstance bin)
        {
            if (bin.Fill <= 0) return;
            bin.Fill -= 1;
            double eaten = (_timers.TryGetValue(bin.Id, out var e) ? e : 0) + 1;
            if (eaten >= Config.WormPerCompost)
            {
                eaten -= Config.WormPerCompost;
                _house.AddItem(MatureId, 1);
                if (_content.Items.Contains(WormId)) _house.AddItem(WormId, 1);
            }
            _timers[bin.Id] = eaten;
        }

        /// <summary>Solucan kasasına envanterden yaş girdi koyar (gübre hariç: solucan yakar); konan birim.</summary>
        public int FeedWorms(ItemInstance bin)
        {
            if (bin == null || !bin.Def.HasTag(WormTag)) return 0;
            int cap = Math.Max(1, bin.Def.FillCapacity), added = 0;
            foreach (var kv in WetInputs)
            {
                if (kv.Key == ManureId) continue;
                while (bin.Fill < cap && added < Config.AddUnits && _house.TryTakeItem(kv.Key)) { bin.Fill += 1; added++; }
            }
            return added;
        }

        /// <summary>Yaprak ağılı: doldurulduktan sonra bekler; süresi dolunca içindeki yaprağın yarısı malç olur.</summary>
        private void StepLeaves(ItemInstance bin, double now)
        {
            if (bin.Fill <= 0 || !_timers.TryGetValue(bin.Id, out var start)) return;
            if (now - start < Config.LeafMouldDays) return;
            int mould = Math.Max(1, bin.Fill / 2);
            bin.Fill = 0;
            _timers.Remove(bin.Id);
            if (_content.Items.Contains(LeafMouldId)) _house.AddItem(LeafMouldId, mould);
            _world.NotifyChanged(bin.Id);
        }

        public int FillLeaves(ItemInstance bin)
        {
            if (bin == null || !bin.Def.HasTag(LeafTag)) return 0;
            int cap = Math.Max(1, bin.Def.FillCapacity), added = 0;
            while (bin.Fill < cap && _house.TryTakeItem(LeavesId)) { bin.Fill += 1; added++; }
            if (added > 0 && !_timers.ContainsKey(bin.Id)) _timers[bin.Id] = Today;
            if (added > 0) _world.NotifyChanged(bin.Id);
            return added;
        }

        /// <summary>Yaprak ağılının malça kalan günü; boşsa -1.</summary>
        public double LeafDaysLeft(ItemInstance bin) =>
            bin == null || bin.Fill <= 0 || !_timers.TryGetValue(bin.Id, out var start) ? -1 : Math.Max(0, Config.LeafMouldDays - (Today - start));

        /// <summary>Şerbet fıçısını gübreyle kurar; fıçı zaten doluysa false.</summary>
        public bool BrewSlurry(ItemInstance barrel)
        {
            if (barrel == null || !barrel.Def.HasTag(SlurryTag) || SlurryState(barrel) != 0) return false;
            if (Count(ManureId) + _house.Count(ManureId) < Config.SlurryManure) return false;
            int need = Config.SlurryManure;
            while (need > 0 && _house.TryTakeItem(ManureId)) need--;
            if (need > 0) Withdraw(ManureId, need);
            _timers[barrel.Id] = Today;
            _world.NotifyChanged(barrel.Id);
            return true;
        }

        /// <summary>Akıtma oluğu fıçıyı kendiliğinden besler: fıçı boşsa hemen kurulur.</summary>
        public void RefillSlurry(ItemInstance barrel)
        {
            if (barrel != null && barrel.Def.HasTag(SlurryTag) && SlurryState(barrel) == 0) { _timers[barrel.Id] = Today; _world.NotifyChanged(barrel.Id); }
        }

        /// <summary>0 boş, 1 oluyor, 2 hazır (bereketli su verir).</summary>
        public int SlurryState(ItemInstance barrel)
        {
            if (barrel == null || !_timers.TryGetValue(barrel.Id, out var start)) return 0;
            double age = Today - start;
            if (age < Config.SlurryBrewDays) return 1;
            if (age < Config.SlurryBrewDays + Config.SlurryLastDays) return 2;
            _timers.Remove(barrel.Id);
            return 0;
        }

        /// <summary>Bereketli su açık mı: hazır bir şerbet fıçısı sulama yoluna ya da sulama eşyasına bitişik.</summary>
        public bool FertigationActive()
        {
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.HasTag(SlurryTag) || SlurryState(it) != 2) continue;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        var p = new GridPos(it.Position.X + dx, it.Position.Y + dy, it.Position.Z);
                        if (_world.Map.GetConduit(p) != ConduitType.None) return true;
                        var near = _world.GetItemAt(p);
                        if (near != null && near != it && (near.Def.WaterRadius > 0 || near.Def.WaterSource)) return true;
                    }
            }
            return false;
        }

        // ---------- gübre yığını (ayrı stok) ----------

        private IEnumerable<ItemInstance> DungHeaps()
        {
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.HasTag(DungHeapTag) && it.Def.FillCapacity > 0) yield return it;
        }

        public bool AnyDungHeap { get { foreach (var _ in DungHeaps()) return true; return false; } }

        public int Count(string itemId)
        {
            if (itemId != ManureId) return 0;
            int n = 0;
            foreach (var h in DungHeaps()) n += h.Fill;
            return n;
        }

        public int Room(string itemId)
        {
            if (itemId != ManureId) return 0;
            int n = 0;
            foreach (var h in DungHeaps()) n += Math.Max(0, h.Def.FillCapacity - h.Fill);
            return n;
        }

        public int Deposit(string itemId, int count)
        {
            if (itemId != ManureId || count <= 0) return 0;
            int left = count;
            foreach (var h in DungHeaps())
            {
                int put = Math.Min(left, Math.Max(0, h.Def.FillCapacity - h.Fill));
                if (put <= 0) continue;
                h.Fill += put; left -= put;
                _world.NotifyChanged(h.Id);
                if (left == 0) break;
            }
            return count - left;
        }

        public int Withdraw(string itemId, int count)
        {
            if (itemId != ManureId || count <= 0) return 0;
            int left = count;
            foreach (var h in DungHeaps())
            {
                int take = Math.Min(left, h.Fill);
                if (take <= 0) continue;
                h.Fill -= take; left -= take;
                _world.NotifyChanged(h.Id);
                if (left == 0) break;
            }
            return count - left;
        }

        /// <summary>
        /// Toplanan pislik ve ahır temizliği: birimler birikir, her dört birim bir gübre olur. Gübre önce gübre yığınına,
        /// sığmayan hane envanterine gider. Üretilen gübre adedini döner.
        /// </summary>
        public int AddDung(int units)
        {
            if (units <= 0) return 0;
            _dungUnits += units;
            int manure = _dungUnits / Math.Max(1, Config.DungPerManure);
            if (manure <= 0) return 0;
            _dungUnits -= manure * Config.DungPerManure;
            int heaped = Deposit(ManureId, manure);
            if (manure > heaped) _house.AddItem(ManureId, manure - heaped);
            return manure;
        }

        public int PendingDungUnits => _dungUnits;

        // ---------- kayıt ----------

        public YardCompostSave Export()
        {
            var heaps = new List<YardHeapSave>();
            foreach (var h in _heaps.Values)
                heaps.Add(new YardHeapSave
                {
                    itemId = h.ItemId, wet = h.Wet, dry = h.Dry, ash = h.Ash, filth = h.Filth, progress = h.Progress, lastTurn = h.LastTurn,
                    goodDays = h.GoodDays, totalDays = h.TotalDays, smelly = h.Smelly, hot = h.Hot,
                    readyRaw = h.Ready[0], readyMature = h.Ready[1], readyBlack = h.Ready[2], readyFilth = h.ReadyFilth
                });
            heaps.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            var timers = new List<YardTimerSave>();
            foreach (var kv in _timers) timers.Add(new YardTimerSave { itemId = kv.Key, value = kv.Value });
            timers.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            return new YardCompostSave { heaps = heaps.ToArray(), timers = timers.ToArray(), rawFilth = _rawFilthStock, dungUnits = _dungUnits };
        }

        public void Import(YardCompostSave data)
        {
            _heaps.Clear(); _timers.Clear(); _rawFilthStock = false; _dungUnits = 0;
            if (data == null) return;
            _rawFilthStock = data.rawFilth;
            _dungUnits = Math.Max(0, data.dungUnits);
            if (data.heaps != null)
                foreach (var s in data.heaps)
                {
                    if (s == null || _world.GetItem(s.itemId) == null) continue;
                    var h = new CompostHeap
                    {
                        ItemId = s.itemId, Wet = s.wet, Dry = s.dry, Ash = s.ash, Filth = s.filth, Progress = s.progress, LastTurn = s.lastTurn,
                        GoodDays = s.goodDays, TotalDays = s.totalDays, Smelly = s.smelly, Hot = s.hot, ReadyFilth = s.readyFilth
                    };
                    h.Ready[0] = s.readyRaw; h.Ready[1] = s.readyMature; h.Ready[2] = s.readyBlack;
                    _heaps[s.itemId] = h;
                }
            if (data.timers != null)
                foreach (var t in data.timers) if (t != null && _world.GetItem(t.itemId) != null) _timers[t.itemId] = t.value;
        }
    }
}
