using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// Sulama ağı: kaynaktan (kuyu, kova-makara) başlayıp birbirine değen kanal, su çarkı ve kaynaklardan oluşan zincir.
    /// Kanal (needsSource) yalnızca kaynağa bağlıyken sular; su çarkı bağlı bir kanala değerken güç verir.
    /// </summary>
    public static class WaterNetwork
    {
        /// <summary>Ağın parçası mı (kaynak, kanal, kapak, su çarkı, sarnıç ya da su terazisi)? Tesisat ağ parçası değildir: suyu iletmez.</summary>
        public static bool IsNode(ItemDef def) => def.WaterSource || def.NeedsWaterSource || def.PowerSource || def.WaterGate || def.IsCistern || def.WaterHead;

        /// <summary>
        /// Kaynağa bağlı ağ parçalarının kimlikleri (kaynaklar dahil). Su yolu katmanındaki ark ve borular da ağı birleştirir
        /// (17.11.2026); donmuş açık ark ve kapalı kapak suyu keser, çark yalnızca açık sudan (ark, oluk, kanal) döner.
        /// </summary>
        public static HashSet<int> Connected(WorldState world) => Analyze(world).ConnectedItems;

        /// <summary>Bağlantı hesabı (tesisat dağıtımı olmadan): ıslak su yolları ve bağlı eşyalar.</summary>
        public static WaterReport Analyze(WorldState world)
        {
            var report = new WaterReport();
            new WaterGraph(world).Connect(report);
            return report;
        }

        /// <summary>Eşya bir su karosuna (dere, gölet) değiyor mu?</summary>
        public static bool TouchesWater(GridMap map, ItemInstance it)
        {
            if (!map.HasWater || it.Position.Z != 0) return false;
            foreach (var t in it.OccupiedTiles())
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (Math.Abs(dx) + Math.Abs(dy) <= 1 && map.IsFilled(new GridPos(t.X + dx, t.Y + dy))) return true;
            return false;
        }

        /// <summary>Kova doldurulabilecek karolar: su kaynağı eşyalarının (kuyu, gölet) ve su karolarının kıyısı.</summary>
        public static HashSet<GridPos> WaterAccess(WorldState world)
        {
            var goals = new HashSet<GridPos>();
            var map = world.Map;
            foreach (var it in world.Items)
            {
                if (it.IsDecor || !it.Def.WaterSource) continue;
                foreach (var t in it.OccupiedTiles())
                    foreach (var o in new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) })
                    {
                        var n = t + o;
                        if (map.IsWalkable(n) && !map.EdgeBlocks(GridMap.EdgeBetween(t, n))) goals.Add(n);
                    }
            }
            // Gölet ve dere: kıyı karosundan kova daldırılır (suya girmeden; donmuşsa buz kırılır).
            if (map.HasWater)
                for (int y = 0; y < map.Height; y++)
                    for (int x = 0; x < map.Width; x++)
                    {
                        var p = new GridPos(x, y);
                        if (!map.IsFilled(p)) continue;                // kuru çukurdan su alınmaz
                        foreach (var o in new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) })
                        {
                            var n = p + o;
                            if (map.InBounds(n) && !map.IsWater(n) && map.IsWalkable(n) && !map.EdgeBlocks(GridMap.EdgeBetween(p, n))) goals.Add(n);
                        }
                    }
            return goals;
        }

        /// <summary>İki eşyanın kapladığı karolar aynı katta yan yana (dik komşu) ya da üst üste mi?</summary>
        public static bool Touches(ItemInstance a, ItemInstance b)
        {
            if (a.Position.Z != b.Position.Z) return false;
            foreach (var ta in a.OccupiedTiles())
                foreach (var tb in b.OccupiedTiles())
                    if (Math.Abs(ta.X - tb.X) + Math.Abs(ta.Y - tb.Y) <= 1) return true;
            return false;
        }
    }

    /// <summary>
    /// Mekanizmalar (Minecraft'tan esinli, ortaçağ halinde): kuyudan çekilen kanal tarlaları sular; kanala konan su çarkı
    /// yanındaki değirmen taşını çevirir ve taş her gün envanterdeki arpayı yeme öğütür. Günde bir kez çalışır.
    /// </summary>
    public sealed class FarmAutomation
    {
        /// <summary>Güç alan makine günde en çok bu kadar parti işler (mühendislik her 3 seviyede +1).</summary>
        public const int BatchesPerDay = 4;
        /// <summary>Su çarkının güç verdiği uzaklık (karo, Chebyshev, kapladığı karolardan); mühendislik 5. ve 9. seviyede +1.</summary>
        public const int PowerReach = 2;

        /// <summary>Hanenin en iyi mühendisinin seviyesi; null ise 1.</summary>
        public Func<int> EngineeringLevel { get; set; }
        private int Eng => Math.Max(1, EngineeringLevel?.Invoke() ?? 1);
        public int Batches => BatchesPerDay + (Eng - 1) / 3;
        public int Reach => PowerReach + (Eng >= 5 ? 1 : 0) + (Eng >= 9 ? 1 : 0);

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly Random _rng;

        /// <summary>Toprak verimi (hasat tırmığı toprağı yorar); null ise yormaz.</summary>
        public PlantSystem Plants { get; set; }
        /// <summary>İçerik (tohum → ekin); ekim makinesi için.</summary>
        public ContentDatabase Content { get; set; }
        /// <summary>Ekim makinesi tohumu bununla eker (yapı kuralları); null ise ekmez.</summary>
        public BuildService Build { get; set; }
        /// <summary>Karo sıcaklığı (buz kuyusu don ister); null ise buz toplanmaz.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }

        /// <summary>Depo kapasitesi; null ise sınırsız.</summary>
        public StorageService Storage { get; set; }

        public FarmAutomation(WorldState world, Household house, int seed = 23)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _rng = new Random(seed);
        }

        // Balık sepeti: değdiği göletin balık tutma ürünlerinden, mevsimine uyan ve ustalık istemeyen birini seçer.
        private string TrapCatch(ItemInstance trap, Time.Season? season)
        {
            foreach (var pond in _world.Items)
            {
                if (pond.IsDecor || pond == trap || !WaterNetwork.Touches(trap, pond)) continue;
                var pool = new List<ProduceDef>();
                foreach (var i in pond.Def.Interactions)
                {
                    if (i.Skill != SkillType.Fishing) continue;
                    foreach (var p in i.Produce)
                        if (p.MinLevel <= 0 && (p.Seasons == null || !season.HasValue || p.Seasons.Contains(season.Value))) pool.Add(p);
                }
                if (pool.Count > 0) return pool[_rng.Next(pool.Count)].ItemId;
            }
            return null;
        }

        /// <summary>Çalışan su çarkları (kaynağa bağlı kanala değenler).</summary>
        public List<ItemInstance> TurningWheels(HashSet<int> network = null)
        {
            network = network ?? WaterNetwork.Connected(_world);
            var list = new List<ItemInstance>();
            foreach (var it in _world.Items)
                if (!it.IsDecor && (it.Def.PowerSource && network.Contains(it.Id) || it.Def.WindPower)) list.Add(it);   // yel değirmeni suya bakmaz
            return list;
        }

        /// <summary>Makine güç alıyor mu (çalışan bir su çarkına PowerReach karo yakın)?</summary>
        public bool IsPowered(ItemInstance machine, List<ItemInstance> wheels = null)
        {
            if (machine == null || !machine.Def.NeedsPower) return false;
            wheels = wheels ?? TurningWheels();
            foreach (var w in wheels)
            {
                if (w.Position.Z != machine.Position.Z) continue;
                foreach (var tw in w.OccupiedTiles())
                    foreach (var tm in machine.OccupiedTiles())
                        if (Math.Max(Math.Abs(tw.X - tm.X), Math.Abs(tw.Y - tm.Y)) <= Reach) return true;
            }
            return false;
        }

        /// <summary>Gün başı: güç alan makineler envanterden girdi alıp ürün koyar. (ürün kimliği, adet) özeti döner.</summary>
        public List<KeyValuePair<string, int>> OnNewDay(Time.Season? season = null)
        {
            var made = new Dictionary<string, int>();
            var order = new List<string>();
            foreach (var trap in new List<ItemInstance>(_world.Items))
            {
                if (trap.IsDecor || !trap.Def.IsFishTrap) continue;
                var fish = TrapCatch(trap, season);
                if (fish == null) continue;
                int got = Storage != null ? Storage.AddUpTo(fish, 1) : AddAll(fish, 1);
                if (got <= 0) continue;
                if (!made.ContainsKey(fish)) { made[fish] = 0; order.Add(fish); }
                made[fish] += got;
            }
            void Made(string id, int n)
            {
                if (n <= 0) return;
                if (!made.ContainsKey(id)) { made[id] = 0; order.Add(id); }
                made[id] += n;
            }
            // Buz kuyusu: donlu sabahlarda buz toplar.
            foreach (var well in new List<ItemInstance>(_world.Items))
                if (!well.IsDecor && well.Def.IceCollect > 0 && TemperatureAt != null && TemperatureAt(well.Position) <= 0f)
                    Made("item.ice", Storage != null ? Storage.AddUpTo("item.ice", well.Def.IceCollect) : AddAll("item.ice", well.Def.IceCollect));
            var wheels = TurningWheels();
            foreach (var machine in new List<ItemInstance>(_world.Items))
            {
                if (machine.IsDecor || wheels.Count == 0 || !IsPowered(machine, wheels)) continue;
                if (machine.Def.HarvestRadius > 0) foreach (var kv in HarvestAround(machine)) Made(kv.Key, kv.Value);
                if (machine.Def.SeedRadius > 0) foreach (var kv in SeedAround(machine, season)) Made(kv.Key, kv.Value);
                var p = machine.Def.AutoProcess;
                if (p == null) continue;
                for (int b = 0; b < Batches; b++)
                {
                    if (_house.Count(p.Item) < p.Count) break;
                    if (Storage != null && Storage.IsResource(p.Produces) && Storage.Free + p.Count < p.ProducesCount) break;   // yer yoksa öğütmez
                    for (int k = 0; k < p.Count; k++) _house.TryTakeItem(p.Item);
                    int added = Storage != null ? Storage.AddUpTo(p.Produces, p.ProducesCount) : AddAll(p.Produces, p.ProducesCount);
                    if (!made.ContainsKey(p.Produces)) { made[p.Produces] = 0; order.Add(p.Produces); }
                    made[p.Produces] += added;
                }
            }
            var result = new List<KeyValuePair<string, int>>();
            foreach (var id in order) if (made[id] > 0) result.Add(new KeyValuePair<string, int>(id, made[id]));
            return result;
        }

        private int AddAll(string id, int count) { _house.AddItem(id, count); return count; }

        private static bool Within(ItemInstance machine, GridPos p, int r)
        {
            if (p.Z != machine.Position.Z) return false;
            foreach (var t in machine.OccupiedTiles())
                if (Math.Max(Math.Abs(t.X - p.X), Math.Abs(t.Y - p.Y)) <= r) return true;
            return false;
        }

        /// <summary>Hasat tırmığı: yarıçaptaki olgun ekinleri biçer; kesin ürünleri (şanslı ve ustalık isteyen hariç) depoya koyar.</summary>
        private List<KeyValuePair<string, int>> HarvestAround(ItemInstance rake)
        {
            var got = new List<KeyValuePair<string, int>>();
            foreach (var crop in new List<ItemInstance>(_world.Items))
            {
                if (crop.IsDecor || !crop.IsPlant || !crop.Def.IsCrop || !crop.IsGrown || !Within(rake, crop.Position, rake.Def.HarvestRadius)) continue;
                InteractionDef harvest = null;
                foreach (var i in crop.Def.Interactions) if (i.RequiresGrown && i.RemovesItem) { harvest = i; break; }
                if (harvest == null) continue;
                foreach (var p in harvest.Produce)
                {
                    if (p.Chance < 1f || p.MinLevel > 0) continue;   // makine el kadar özenli değildir
                    int added = Storage != null ? Storage.AddUpTo(p.ItemId, p.Count) : AddAll(p.ItemId, p.Count);
                    if (added > 0) got.Add(new KeyValuePair<string, int>(p.ItemId, added));
                }
                Plants?.OnHarvest(crop);
                _world.RemoveItem(crop.Id);
            }
            return got;
        }

        /// <summary>Ekim makinesi: yarıçaptaki boş toprağa, envanterde olup bu mevsim büyüyebilen ilk tohumu eker.</summary>
        private List<KeyValuePair<string, int>> SeedAround(ItemInstance seeder, Time.Season? season)
        {
            var got = new List<KeyValuePair<string, int>>();
            if (Build == null) return got;
            var map = _world.Map;
            int r = seeder.Def.SeedRadius, planted = 0;
            string seedUsed = null;
            for (int dy = -r - 2; dy <= r + 2; dy++)
                for (int dx = -r - 2; dx <= r + 2; dx++)
                {
                    var t = new GridPos(seeder.Position.X + dx, seeder.Position.Y + dy, seeder.Position.Z);
                    if (!map.InBounds(t) || !Within(seeder, t, r) || map.GetFloor(t) != FloorType.Dirt || _world.GetItemAt(t) != null) continue;
                    string seed = PickSeed(season);
                    if (seed == null) goto done;
                    if (Build.Plant(seed, t, out _).Ok) { planted++; seedUsed = seed; }
                }
            done:
            if (planted > 0) got.Add(new KeyValuePair<string, int>("ekim:" + seedUsed, planted));
            return got;
        }

        private string PickSeed(Time.Season? season)
        {
            foreach (var kv in _house.InventoryList())
            {
                if (kv.Value <= 0) continue;
                if (Content == null || !Content.Items.TryGet(kv.Key, out var def)) continue;
                if (def.PlantsAs == null || !Content.Items.TryGet(def.PlantsAs, out var crop) || !crop.IsCrop) continue;
                if (season.HasValue && !PlantSystem.CanGrow(crop, season.Value)) continue;
                return kv.Key;
            }
            return null;
        }
    }

    /// <summary>Canlı modda karo işi: çimi çapalayıp toprak yapmak (yapı modunda akçeyle, burada emekle) ve yol onarmak.</summary>
    public sealed class FarmWork : ITileWork
    {
        public const double TillDays = 0.02;
        public const double RepairDays = 0.03;
        public const float TillXp = 1f;
        /// <summary>Bu durumun altındaki döşenmiş yol onarılabilir.</summary>
        public const float RepairBelow = 0.95f;

        private readonly WorldState _world;

        /// <summary>Onarımın taşını veren hane; null ise taş istenmez.</summary>
        public Household House { get; set; }
        /// <summary>İçerik (balık türleri: gölet eşyasının "fish" ürünleri); null ise dereden balık tutulmaz.</summary>
        public ContentDatabase Content { get; set; }
        /// <summary>Mevsim ve karo sıcaklığı (balık türü, soğuk su); null ise her mevsim / ılık.</summary>
        public Func<Time.Season> Season { get; set; }
        public Func<GridPos, float> TemperatureAt { get; set; }
        /// <summary>Olta kademesinin hızı; null ise 1.</summary>
        public Func<Character, double> RodSpeed { get; set; }
        private readonly Random _rng = new Random(29);

        public const double BatheDays = 0.08, FishDays = 0.12, CutIceDays = 0.06;
        /// <summary>Suya girip yıkanmanın hijyeni; yaz serinliği keyfi.</summary>
        public const float BatheHygiene = 60f, SwimFun = 15f;

        /// <summary>Yol onarıldı.</summary>
        public event Action<Character, GridPos> Repaired;
        /// <summary>Su işi bitti (karakter, karo, özet).</summary>
        public event Action<Character, GridPos, string> Worked;

        // Dereye girip yıkanmak: hijyen; tamamen ıslanır. Soğuk suda Isınma düşer, yazın serinletir.
        private void BatheIn(Character c, GridPos tile)
        {
            c.Needs.Restore(NeedType.Hygiene, BatheHygiene);
            c.Soak(1f);
            float t = TemperatureAt != null ? TemperatureAt(tile) : 15f;
            string note;
            if (t < InteractionEffects.ColdBathTemperature)
            {
                c.Needs.Set(NeedType.Warmth, c.Needs.Get(NeedType.Warmth) - Math.Min(50f, (InteractionEffects.ColdBathTemperature - t) * 2.5f));
                c.AddMoodlet(Mood.MoodCatalog.ColdWater);
                note = "buz gibi suda yıkandı";
            }
            else
            {
                if (t >= 18f) { c.Needs.Restore(NeedType.Fun, SwimFun); c.AddMoodlet(Mood.MoodCatalog.Swim); }
                note = "suda yıkandı";
            }
            Worked?.Invoke(c, tile, note);
        }

        // Kıyıdan balık: göletteki balıklardan (sayıyla ağırlıklı) tutulur, gölette eksilir. Gölet doluluğu şansı
        // artırır; türün ustalık eşiği (gölet balık listesi) altındaysa o tür kaçar. Balıkçılık XP'si.
        private void FishAt(Character c, GridPos tile)
        {
            var pond = _world.Map.PondAt(tile);
            var fish = PondFishing();
            int level = c.Skills.GetLevel(SkillType.Fishing) + c.Modifiers.QualityLevels;
            var catchable = new List<KeyValuePair<string, int>>();
            int total = 0;
            foreach (var kv in pond.Fish)
            {
                int min = 0;
                if (fish != null) foreach (var p in fish.Produce) if (p.ItemId == kv.Key) min = p.MinLevel;
                if (level < min) continue;
                catchable.Add(kv);
                total += kv.Value;
            }
            string note = "bu sefer bir şey çıkmadı";
            double chance = Math.Min(0.75, 0.2 + 0.6 * pond.FishTotal / Math.Max(1.0, pond.FishCapacity));
            if (total == 0) note = "buradaki balıklar için ustalığı yetmiyor";
            else if (_rng.NextDouble() < chance)
            {
                int pick = _rng.Next(total);
                foreach (var kv in catchable)
                {
                    if (pick >= kv.Value) { pick -= kv.Value; continue; }
                    pond.TakeFish(kv.Key);
                    House?.AddItem(kv.Key, 1);
                    note = $"balık: {Content.Items.Get(kv.Key).DisplayName} (gölette {pond.FishTotal} kaldı)";
                    break;
                }
            }
            c.GainSkillXp(SkillType.Fishing, fish != null ? fish.SkillXp : 10f);
            Worked?.Invoke(c, c.Position, note);
        }

        /// <summary>Çapa kademesinin hız çarpanı (alet); yoksa 1.</summary>
        public Func<Character, double> HoeSpeed { get; set; }

        /// <summary>Çapalandı (bildirim için).</summary>
        public event Action<Character, GridPos> Tilled;

        public FarmWork(WorldState world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public string CanDo(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            var map = _world.Map;
            switch (action)
            {
                case TileAction.Bathe:
                case TileAction.Fish:
                case TileAction.CutIce:
                case TileAction.Stock:
                    if (!map.IsWater(tile)) return "Burada su yok.";
                    if (!map.IsFilled(tile)) return EmptyPondText;
                    break;
            }
            switch (action)
            {
                case TileAction.Bathe:
                    return map.WaterFrozen ? "Su donmuş: önce buz kes." : null;
                case TileAction.Fish:                                   // buz deliğinden de olur
                    return map.PondAt(tile).FishTotal == 0 ? NoFishText : null;
                case TileAction.CutIce:
                    return map.WaterFrozen ? null : "Su donmamış (kışın dondurucu soğukta donar).";
                case TileAction.Stock:
                    if (arg == null || Content == null || !Content.Items.Contains(arg) || !IsFishSpecies(arg)) return "Gölete yalnızca balık atılır.";
                    if (House != null && House.Count(arg) <= 0) return $"Elinde {Content.Items.Get(arg).DisplayName} yok.";
                    var pond = map.PondAt(tile);
                    return pond.FishTotal >= pond.FishCapacity ? "Gölet dolu: daha fazla balık sığmaz (göleti büyüt)." : null;
            }
            if (action == TileAction.Repair)
            {
                var road = map.GetRoad(tile);
                if (road == RoadType.None) return "Burada yol yok.";
                if (map.IsNaturalRoad(tile)) return "Kendiliğinden oluşmuş patika onarılmaz (yapı modunda yol döşe).";
                if (map.GetRoadCondition(tile) >= RepairBelow) return "Yol sağlam.";
                int stone = RepairStoneFor(actor, road);
                if (stone > 0 && House != null && House.Count("item.stone") < stone) return $"Taş yetmiyor ({House.Count("item.stone")}/{stone}).";
                return null;
            }
            if (action != TileAction.Till) return "Bilinmeyen iş.";
            if (!map.InBounds(tile) || tile.Z != 0) return "Yalnızca zemin katta çapalanır.";
            if (!map.IsOwned(tile)) return "Arsa senin değil.";
            if (map.GetFloor(tile) != FloorType.Grass) return map.GetFloor(tile) == FloorType.Dirt ? "Orası zaten toprak." : "Yalnızca çim çapalanır.";
            if (_world.GetItemAt(tile) != null) return "Üstünde bir şey var.";
            if (map.GetZone(tile) == ZoneType.Pasture) return "Otlak çapalanmaz (önce bölgeyi sil).";
            return null;
        }

        /// <summary>Onarımın taşı: taş ustası (5. seviye) bir taş eksik harcar.</summary>
        public static int RepairStoneFor(Character actor, RoadType road)
        {
            int stone = BuildService.RepairStone(road);
            if (actor != null && actor.Skills.GetLevel(SkillType.Masonry) >= 5) stone = Math.Max(0, stone - 1);
            return stone;
        }

        public const string EmptyPondText = "Gölet boş: kazılan gölet yalnızca yağmurla dolar.";
        public const string NoFishText = "Gölette balık yok: önce aynı türden en az iki balık at, böcekler gelince ürerler.";

        /// <summary>Gölete atılabilen, gölette yaşayan balık türleri (gölet eşyasının balık listesi).</summary>
        public IEnumerable<string> FishSpecies()
        {
            var fish = PondFishing();
            if (fish == null) yield break;
            foreach (var p in fish.Produce) yield return p.ItemId;
        }

        public bool IsFishSpecies(string itemId) { foreach (var s in FishSpecies()) if (s == itemId) return true; return false; }

        private InteractionDef PondFishing()
        {
            if (Content == null || !Content.Items.TryGet("item.pond", out var pond)) return null;
            foreach (var i in pond.Interactions) if (i.Skill == SkillType.Fishing) return i;
            return null;
        }

        public double Duration(Character actor, TileAction action)
        {
            if (action == TileAction.Stock) return 0.02;
            if (action == TileAction.Bathe) return BatheDays;
            if (action == TileAction.CutIce) return CutIceDays;
            if (action == TileAction.Fish) return FishDays / Math.Max(0.25, RodSpeed?.Invoke(actor) ?? 1.0);
            if (action == TileAction.Repair) return RepairDays / SkillNames.Factor(actor.Skills.GetLevel(SkillType.Masonry), 0.05);
            double speed = HoeSpeed?.Invoke(actor) ?? 1.0;
            double skill = Math.Max(0.5, 1.0 - 0.05 * (actor.Skills.GetLevel(SkillType.Farming) - 1));
            return TillDays * skill / Math.Max(0.25, speed);
        }

        public void Apply(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            if (CanDo(actor, tile, action, arg) != null) return;
            if (action == TileAction.Bathe) { BatheIn(actor, tile); return; }
            if (action == TileAction.Fish) { FishAt(actor, tile); return; }
            if (action == TileAction.Stock)
            {
                House?.TryTakeItem(arg);
                var pond = _world.Map.PondAt(tile);
                pond.AddFish(arg);
                pond.Fish.TryGetValue(arg, out int n);
                string name = Content.Items.Get(arg).DisplayName;
                Worked?.Invoke(actor, tile, n >= 2
                    ? $"gölete {name} attı ({n} tane){(pond.Insects ? ": üreyebilirler" : ": böcekler gelince ürerler")}"
                    : $"gölete {name} attı (1 tane: üremesi için bir tane daha gerek)");
                return;
            }
            if (action == TileAction.CutIce)
            {
                House?.AddItem("item.ice", 3);
                Worked?.Invoke(actor, tile, "+3 Buz");
                return;
            }
            if (action == TileAction.Repair)
            {
                int stone = RepairStoneFor(actor, _world.Map.GetRoad(tile));
                if (House != null) for (int i = 0; i < stone; i++) House.TryTakeItem("item.stone");
                _world.Map.RepairRoad(tile);
                actor.GainSkillXp(SkillType.Masonry, 4 + 2 * stone);
                Repaired?.Invoke(actor, tile);
                return;
            }
            _world.Map.SetFloor(tile, FloorType.Dirt);
            actor.GainSkillXp(SkillType.Farming, TillXp);
            Tilled?.Invoke(actor, tile);
        }
    }
}
