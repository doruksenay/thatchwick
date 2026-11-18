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
        /// <summary>Ağın parçası mı (kaynak, kanal ya da su çarkı)?</summary>
        public static bool IsNode(ItemDef def) => def.WaterSource || def.NeedsWaterSource || def.PowerSource || def.WaterGate;

        /// <summary>Kaynağa bağlı ağ parçalarının kimlikleri (kaynaklar dahil).</summary>
        public static HashSet<int> Connected(WorldState world)
        {
            var nodes = new List<ItemInstance>();
            foreach (var it in world.Items) if (!it.IsDecor && IsNode(it.Def)) nodes.Add(it);
            var connected = new HashSet<int>();
            var queue = new Queue<ItemInstance>();
            foreach (var n in nodes) if (n.Def.WaterSource) { connected.Add(n.Id); queue.Enqueue(n); }
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur.Def.PowerSource) continue;           // çark suyu iletmez, yalnızca kullanır
                foreach (var n in nodes)
                {
                    if (connected.Contains(n.Id) || !Touches(cur, n)) continue;
                    if (n.Def.WaterGate && n.Toggled) continue;  // kapalı kapak suyu kesir
                    connected.Add(n.Id);
                    queue.Enqueue(n);
                }
            }
            return connected;
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

        /// <summary>Yol onarıldı.</summary>
        public event Action<Character, GridPos> Repaired;

        /// <summary>Çapa kademesinin hız çarpanı (alet); yoksa 1.</summary>
        public Func<Character, double> HoeSpeed { get; set; }

        /// <summary>Çapalandı (bildirim için).</summary>
        public event Action<Character, GridPos> Tilled;

        public FarmWork(WorldState world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public string CanDo(Character actor, GridPos tile, TileAction action)
        {
            var map = _world.Map;
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

        public double Duration(Character actor, TileAction action)
        {
            if (action == TileAction.Repair) return RepairDays / SkillNames.Factor(actor.Skills.GetLevel(SkillType.Masonry), 0.05);
            double speed = HoeSpeed?.Invoke(actor) ?? 1.0;
            double skill = Math.Max(0.5, 1.0 - 0.05 * (actor.Skills.GetLevel(SkillType.Farming) - 1));
            return TillDays * skill / Math.Max(0.25, speed);
        }

        public void Apply(Character actor, GridPos tile, TileAction action)
        {
            if (CanDo(actor, tile, action) != null) return;
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
