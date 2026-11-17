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
        public static bool IsNode(ItemDef def) => def.WaterSource || def.NeedsWaterSource || def.PowerSource;

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
        /// <summary>Güç alan makine günde en çok bu kadar parti işler.</summary>
        public const int BatchesPerDay = 4;
        /// <summary>Su çarkının güç verdiği uzaklık (karo, Chebyshev, kapladığı karolardan).</summary>
        public const int PowerReach = 2;

        private readonly WorldState _world;
        private readonly Household _house;

        /// <summary>Depo kapasitesi; null ise sınırsız.</summary>
        public StorageService Storage { get; set; }

        public FarmAutomation(WorldState world, Household house)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
        }

        /// <summary>Çalışan su çarkları (kaynağa bağlı kanala değenler).</summary>
        public List<ItemInstance> TurningWheels(HashSet<int> network = null)
        {
            network = network ?? WaterNetwork.Connected(_world);
            var list = new List<ItemInstance>();
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Def.PowerSource && network.Contains(it.Id)) list.Add(it);
            return list;
        }

        /// <summary>Makine güç alıyor mu (çalışan bir su çarkına PowerReach karo yakın)?</summary>
        public bool IsPowered(ItemInstance machine, List<ItemInstance> wheels = null)
        {
            if (machine == null || machine.Def.AutoProcess == null) return false;
            wheels = wheels ?? TurningWheels();
            foreach (var w in wheels)
            {
                if (w.Position.Z != machine.Position.Z) continue;
                foreach (var tw in w.OccupiedTiles())
                    foreach (var tm in machine.OccupiedTiles())
                        if (Math.Max(Math.Abs(tw.X - tm.X), Math.Abs(tw.Y - tm.Y)) <= PowerReach) return true;
            }
            return false;
        }

        /// <summary>Gün başı: güç alan makineler envanterden girdi alıp ürün koyar. (ürün kimliği, adet) özeti döner.</summary>
        public List<KeyValuePair<string, int>> OnNewDay()
        {
            var made = new Dictionary<string, int>();
            var order = new List<string>();
            var wheels = TurningWheels();
            if (wheels.Count == 0) return new List<KeyValuePair<string, int>>();
            foreach (var machine in new List<ItemInstance>(_world.Items))
            {
                if (machine.IsDecor || !IsPowered(machine, wheels)) continue;
                var p = machine.Def.AutoProcess;
                for (int b = 0; b < BatchesPerDay; b++)
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
                int stone = BuildService.RepairStone(road);
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

        public double Duration(Character actor, TileAction action)
        {
            if (action == TileAction.Repair) return RepairDays;
            double speed = HoeSpeed?.Invoke(actor) ?? 1.0;
            double skill = Math.Max(0.5, 1.0 - 0.05 * (actor.Skills.GetLevel(SkillType.Farming) - 1));
            return TillDays * skill / Math.Max(0.25, speed);
        }

        public void Apply(Character actor, GridPos tile, TileAction action)
        {
            if (CanDo(actor, tile, action) != null) return;
            if (action == TileAction.Repair)
            {
                int stone = BuildService.RepairStone(_world.Map.GetRoad(tile));
                if (House != null) for (int i = 0; i < stone; i++) House.TryTakeItem("item.stone");
                _world.Map.RepairRoad(tile);
                Repaired?.Invoke(actor, tile);
                return;
            }
            _world.Map.SetFloor(tile, FloorType.Dirt);
            actor.GainSkillXp(SkillType.Farming, TillXp);
            Tilled?.Invoke(actor, tile);
        }
    }
}
