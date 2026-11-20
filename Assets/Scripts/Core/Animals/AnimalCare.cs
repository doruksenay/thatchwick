using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    /// <summary>Bir barınağın konfor dökümü (0-100): oyuncuya neyin eksik olduğunu söyler.</summary>
    public sealed class AnimalComfortReport
    {
        public float Shelter, Space, Warmth, Water, Bedding, Light, Decor;
        public float Total => Math.Min(100f, Shelter + Space + Warmth + Water + Bedding + Light + Decor);
        public bool IsRoom;
        public int Animals, Tiles;

        /// <summary>Kısa açıklama: eksik olanlar.</summary>
        public string Hint()
        {
            var missing = new List<string>();
            if (!IsRoom) missing.Add("kapalı oda");
            if (IsRoom && Space < 20f) missing.Add("yer dar");
            if (Warmth < 15f) missing.Add("sıcaklık");
            if (Water <= 0f) missing.Add("su yalağı");
            if (Bedding <= 0f) missing.Add("saman altlık");
            if (IsRoom && Light <= 0f) missing.Add("ışık ya da pencere");
            if (IsRoom && Decor < 5f) missing.Add("süs");
            return missing.Count == 0 ? "her şey yerinde" : "eksik: " + string.Join(", ", missing);
        }
    }

    // Hayvancılık (17.11.2026): barınak odası, otlak, konfor, otomasyon ve binek.
    public sealed partial class AnimalSystem
    {
        /// <summary>Karakteri kimliğiyle bulur (binicilik XP'si ve hızı, yavrulama ustalığı); null ise beceri etkisi yok.</summary>
        public Func<int, Character> FindCharacter { get; set; }

        /// <summary>Hanenin en iyi hayvancılık ustalığı (seviye + dal ustalığı); yoksa 1. Yavrulama şansını belirler.</summary>
        public Func<int> HusbandryLevel { get; set; }

        /// <summary>Binicinin at sırtında bir karo gidince aldığı binicilik XP'si.</summary>
        public const float RideXpPerTile = 0.4f;

        /// <summary>Yavru doğdu (anne, yavru).</summary>
        public event Action<Animal, Animal> Born;

        /// <summary>Hayvanlar dışarı çıkabilir mi (gündüz, havası iyi); null ise hep. Oyun katmanı saat ve havadan verir.</summary>
        public Func<bool> CanGoOut { get; set; }
        /// <summary>Hayvan çanı: false ise hayvanlar barınakta tutulur (otlağa çıkmaz).</summary>
        public bool Released { get; set; } = true;
        /// <summary>Karonun sıcaklığı (ısı sistemi); null ise ılık sayılır.</summary>
        public Func<GridPos, float> Temperature { get; set; }
        /// <summary>Binicinin karosu (karakter kimliğiyle); binici yoksa null.</summary>
        public Func<int, GridPos?> RiderPosition { get; set; }

        /// <summary>Bir otlak karosunun otu (1 = dolu, bir hayvanın bir birim yemi); sözlükte olmayan otlak karosu doludur.</summary>
        private readonly Dictionary<GridPos, float> _grass = new Dictionary<GridPos, float>();
        public const float GrassRegrowPerDay = 0.34f;
        /// <summary>Altlık sayılan zemin stilleri (saman, saz).</summary>
        public static readonly string[] BeddingFloors = { "floor_straw", "floor_rush" };

        private List<GridPos> _allowedCache;      // bir tur içinde otlak karoları
        private List<GridPos> _roadCache;         // bir tur içinde arsadaki dış yol karoları

        private bool Outside => Released && (CanGoOut == null || CanGoOut());

        // ---------- barınak odası ----------

        /// <summary>Barınağın bulunduğu kapalı oda (barınak odası); yoksa null (hazır kümes gibi dış mekân barınağı).</summary>
        public Room RoomOf(ItemInstance home)
        {
            if (home == null) return null;
            var room = _world.Map.Rooms.RoomAt(home.Position);
            return room != null && room.IsIndoor ? room : null;
        }

        private static bool InRoom(Room room, ItemInstance item)
        {
            foreach (var t in room.Tiles) if (t == item.Position) return true;
            return false;
        }

        /// <summary>Barınağın odasındaki eşyalar (oda yoksa barınağın 6 karo çevresi).</summary>
        public List<ItemInstance> ItemsNear(ItemInstance home)
        {
            var list = new List<ItemInstance>();
            if (home == null) return list;
            var room = RoomOf(home);
            HashSet<GridPos> tiles = room != null ? new HashSet<GridPos>(room.Tiles) : null;
            foreach (var it in _world.Items)
            {
                if (it == home || it.IsDecor) continue;
                if (tiles != null) { if (tiles.Contains(it.Position)) list.Add(it); }
                else if (it.Position.Z == home.Position.Z && Math.Abs(it.Position.X - home.Position.X) <= 6 && Math.Abs(it.Position.Y - home.Position.Y) <= 6) list.Add(it);
            }
            return list;
        }

        // Barınağın odasındaki yemlikler (dış mekân barınağında yakındaki yemlikler).
        private IEnumerable<ItemInstance> TroughsFor(ItemInstance home)
        {
            foreach (var it in ItemsNear(home)) if (it.Def.IsTrough) yield return it;
        }

        /// <summary>Bir yemlik ya da barınak için günlük yem ihtiyacı (odasındaki hayvanların toplamı).</summary>
        public double DailyFeedNeed(ItemInstance feeder)
        {
            if (feeder == null) return 0;
            double sum = 0;
            if (feeder.Def.IsHousing)
            {
                foreach (var a in _animals) if (!a.IsDead && a.HomeItemId == feeder.Id) sum += a.Species.FeedPerDay;
                return sum;
            }
            var room = _world.Map.Rooms.RoomAt(feeder.Position);
            foreach (var a in _animals)
            {
                if (a.IsDead) continue;
                var home = _world.GetItem(a.HomeItemId);
                if (home == null) continue;
                var hr = RoomOf(home);
                bool same = hr != null ? hr == room : Math.Abs(home.Position.X - feeder.Position.X) <= 6 && Math.Abs(home.Position.Y - feeder.Position.Y) <= 6;
                if (same) sum += a.Species.FeedPerDay;
            }
            return sum;
        }

        // ---------- konfor ----------

        /// <summary>
        /// Barınak konforu: barınak (kapalı oda 25, hazır barınak 20), alan (20), sıcaklık (15), su (10), saman altlık (10),
        /// ışık (5), süs (15). Hazır dış barınak en çok ~70, iyi kurulmuş ve süslenmiş oda 100.
        /// </summary>
        public AnimalComfortReport ComfortOf(ItemInstance home)
        {
            var r = new AnimalComfortReport();
            if (home == null) return r;
            var map = _world.Map;
            var room = RoomOf(home);
            r.IsRoom = room != null;
            var near = ItemsNear(home);

            if (room != null)
            {
                r.Shelter = 25f;
                // Alan: odadaki bütün barınakların hayvanları için istenen karo / yürünebilir karo.
                var homes = new HashSet<int>();
                foreach (var it in near) if (it.Def.IsHousing) homes.Add(it.Id);
                homes.Add(home.Id);
                double needed = 0;
                foreach (var a in _animals) if (!a.IsDead && homes.Contains(a.HomeItemId)) { needed += a.Species.Space; r.Animals++; }
                foreach (var t in room.Tiles) if (map.IsWalkable(t)) r.Tiles++;
                r.Space = needed <= 0 ? 20f : (float)(20.0 * Math.Min(1.0, r.Tiles / needed));
                if (room.WindowCount > 0) r.Light = 5f;
            }
            else
            {
                r.Shelter = home.Def.RequiresIndoor ? 5f : 20f;   // açıkta kalmış bölme neredeyse barınak değildir
                r.Space = 15f;
                r.Bedding = home.Def.RequiresIndoor ? 0f : 5f;    // hazır kümesin içinde saman var
            }

            float temp = Temperature != null ? Temperature(home.Position) : 12f;
            r.Warmth = temp >= 4f && temp <= 26f ? 15f : temp >= -2f && temp <= 32f ? 7f : 0f;

            float decor = 0f;
            foreach (var it in near)
            {
                if (it.Def.AnimalRole == AnimalRoles.Water || it.Def.WaterSource) r.Water = 10f;
                if (it.Def.AnimalRole == AnimalRoles.Bedding) r.Bedding = 10f;
                if (room != null && it.GivesLight) r.Light = 5f;
                decor += it.Def.AnimalComfort;
            }
            if (room != null)
            {
                int bedded = 0;
                foreach (var t in room.Tiles)
                {
                    var style = map.GetFloorStyle(t);
                    if (style != null && Array.IndexOf(BeddingFloors, style) >= 0) bedded++;
                }
                if (bedded * 2 >= room.Tiles.Count) r.Bedding = 10f;           // odanın yarısı saman/saz serili
            }
            r.Decor = Math.Min(15f, decor);
            return r;
        }

        // ---------- otlak ----------

        public float GrassAt(GridPos p)
        {
            if (_world.Map.GetZone(p) != ZoneType.Pasture || _world.Map.GetFloor(p) != FloorType.Grass) return 0f;
            if (_world.Map.GetRoad(p) != RoadType.None) return 0f;          // çiğnenmiş patikada ot yok
            return _grass.TryGetValue(p, out var g) ? g : 1f;
        }

        // Otlaktaki karosunun otunu yer (gündüz, salınmışken); yenilen miktarı döner.
        private double Graze(Animal a, double need)
        {
            if (need <= 0 || !Outside) return 0;
            float g = GrassAt(a.Position);
            if (g <= 0f) return 0;
            double eat = Math.Min(need, g);
            _grass[a.Position] = (float)(g - eat);
            return eat;
        }

        private List<GridPos> PastureTiles()
        {
            if (_allowedCache != null) return _allowedCache;
            var list = new List<GridPos>();
            var map = _world.Map;
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (map.GetZone(p) == ZoneType.Pasture && map.IsWalkable(p)) list.Add(p);
                }
            _allowedCache = list;
            return list;
        }

        // Arsadaki açık hava yol karoları: hayvanlar barınaktan otlağa yoldan gider (yolda durmaz, yalnızca geçer).
        private List<GridPos> RoadTiles()
        {
            if (_roadCache != null) return _roadCache;
            var list = new List<GridPos>();
            var map = _world.Map;
            if (map.HasRoads)
                foreach (var r in map.RoadTiles())
                {
                    if (r.type == RoadType.None || !map.IsOwned(r.pos) || !map.IsWalkable(r.pos)) continue;
                    var room = map.Rooms.RoomAt(r.pos);
                    if (room == null || !room.IsIndoor) list.Add(r.pos);
                }
            _roadCache = list;
            return list;
        }

        /// <summary>Hayvanın dolaşacağı yerler (yol hariç): barınak odası (yoksa çevresi) ve dışarı çıkabiliyorsa otlak.</summary>
        public HashSet<GridPos> WanderTargets(Animal a, ItemInstance home)
        {
            var set = LivingArea(a, home);
            if (Outside) foreach (var t in RoadTiles()) if (_world.Map.GetZone(t) != ZoneType.Pasture) set.Remove(t);
            return set;
        }

        /// <summary>Hayvanın yaşam alanı: barınak odası (yoksa barınak çevresi), dışarı çıkabiliyorsa otlak ve arsadaki yollar.</summary>
        public HashSet<GridPos> LivingArea(Animal a, ItemInstance home)
        {
            var set = new HashSet<GridPos>();
            var map = _world.Map;
            var room = RoomOf(home);
            if (room != null)
            {
                foreach (var t in room.Tiles) if (map.IsWalkable(t)) set.Add(t);
            }
            else
            {
                var c = home != null ? home.Position : a.Position;
                for (int dy = -WanderRadius; dy <= WanderRadius; dy++)
                    for (int dx = -WanderRadius; dx <= WanderRadius; dx++)
                    {
                        var t = new GridPos(c.X + dx, c.Y + dy, c.Z);
                        if (map.IsWalkable(t)) set.Add(t);
                    }
            }
            if (Outside)
            {
                foreach (var t in PastureTiles()) if (t.Z == a.Position.Z) set.Add(t);
                if (a.Position.Z == 0) foreach (var t in RoadTiles()) set.Add(t);
            }
            return set;
        }

        // Yaşam alanından rastgele bir hedef seçip oraya alan içinde yol bulur. Alanın dışındaysa (gece otlakta kaldı,
        // yeni barınağa taşındı) eve haritadaki en kısa yoldan döner.
        private void PlanWalk(Animal a, ItemInstance home)
        {
            var area = LivingArea(a, home);
            if (area.Count == 0) { a.Path = null; return; }
            var list = new List<GridPos>(WanderTargets(a, home));
            if (list.Count == 0) list.AddRange(area);
            if (!area.Contains(a.Position))
            {
                var back = Pathfinder.FindPathToAny(_world.Map, a.Position, area);
                if (back != null && back.Count > 1) { back.RemoveAt(0); a.Path = back; a.Target = back[back.Count - 1]; }
                else a.Path = null;
                return;
            }
            for (int tries = 0; tries < 6; tries++)
            {
                var target = list[_rng.Next(list.Count)];
                if (target == a.Position) continue;
                var path = PathWithin(area, a.Position, target);
                if (path == null) continue;
                a.Target = target;
                a.Path = path;
                return;
            }
            a.Path = null;
        }

        private static readonly GridPos[] Steps4 = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        // Alan içinde BFS yolu (başlangıç hariç); yoksa null. Alan küçük olduğu için yeterince ucuz.
        private List<GridPos> PathWithin(HashSet<GridPos> area, GridPos from, GridPos to)
        {
            var prev = new Dictionary<GridPos, GridPos>();
            var queue = new Queue<GridPos>();
            queue.Enqueue(from);
            prev[from] = from;
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur == to)
                {
                    var path = new List<GridPos>();
                    for (var p = to; p != from; p = prev[p]) path.Add(p);
                    path.Reverse();
                    return path;
                }
                foreach (var d in Steps4)
                {
                    var n = cur + d;
                    if (!area.Contains(n) || prev.ContainsKey(n) || !_world.Map.CanStep(cur, n)) continue;
                    prev[n] = cur;
                    queue.Enqueue(n);
                }
            }
            return null;
        }

        // ---------- gün başı: ot, konfor, otomasyon ----------

        /// <summary>Gün başı raporu (bildirim için).</summary>
        public sealed class DayReport
        {
            public int Collected, FeedAdded, Births, Conceived;
            /// <summary>Sağlık (Adım 42): bugün hastalanan, kendiliğinden iyileşen, hastalıktan ölen, üşüyen hayvan sayısı.</summary>
            public int Sickened, Recovered, DiedOfSickness, Cold;
        }

        /// <summary>Gebe kalma: günlük temel şans ve hayvancılık seviyesi başına ek (10. seviyede ~%17,5).</summary>
        public const double BreedBaseChance = 0.04, BreedPerLevel = 0.015;
        /// <summary>Dişinin dostluğu en az bu olmalı (sevilmeyen hayvan yavrulamaz).</summary>
        public const float BreedFriendship = 30f;
        /// <summary>Barınak konforu bunun altındaysa gebe kalınmaz.</summary>
        public const float BreedComfort = 25f;
        /// <summary>Yavru annesinin dostluğunun bu oranıyla doğar.</summary>
        public const float InheritFriendship = 0.5f;

        public double BreedChance(int husbandryLevel) => BreedBaseChance + BreedPerLevel * Math.Max(0, husbandryLevel - 1) + (BroodBonus != null ? BroodBonus() : 0.0);

        /// <summary>Kuluçka sepeti (Hayvancılık 6, belge 45): yerleştirilmişse yavru şansı +%10; null ise yok.</summary>
        public Func<double> BroodBonus { get; set; }

        /// <summary>Bir dişi gebe kaldı (anne, babanın kimliği).</summary>
        public event Action<Animal, int> Conceived;
        /// <summary>Doğum zamanı geldi ama barınakta yer yok (anne): yer açılana dek bekler.</summary>
        public event Action<Animal> BirthDelayed;

        /// <summary>Bu dişi neden gebe kalamaz (arayüz için); null = kalabilir.</summary>
        public string BreedBlocker(Animal a)
        {
            if (a == null || a.IsDead) return "Hayvan bulunamadı.";
            if (!a.Species.Breeds) return "Bu hayvan yavrulamaz.";
            if (!a.IsFemale) return null;
            if (a.IsPregnant) return $"Gebe; doğuma {Math.Ceiling(a.PregnantDays):0} gün.";
            if (!a.IsMature) return "Daha genç.";
            if (a.IsSick) return "Hasta.";
            if (a.Fullness < 60f) return "Aç.";
            if (a.Friendship < BreedFriendship) return "Sana henüz ısınmadı (sev, tımar et).";
            if (a.Comfort < BreedComfort) return "Barınağı rahat değil.";
            var home = _world.GetItem(a.HomeItemId);
            if (home == null) return "Barınağı yok.";
            if (FindMate(a) == null) return $"Yanında erişkin {a.Species.MaleName ?? "erkek"} yok.";
            return FreeHomeNear(a.Species, home) == null ? "Barınakta yavruya yer yok." : null;
        }

        // Aynı odadaki (dış barınakta aynı barınaktaki) sağlıklı erişkin erkek.
        private Animal FindMate(Animal female)
        {
            var home = _world.GetItem(female.HomeItemId);
            if (home == null) return null;
            var room = RoomOf(home);
            foreach (var m in _animals)
            {
                if (m.IsDead || m.IsFemale || m.IsSick || !m.IsMature || m.Species.Id != female.Species.Id) continue;
                var mh = _world.GetItem(m.HomeItemId);
                if (mh == null) continue;
                if (room != null ? RoomOf(mh) == room : mh.Id == home.Id) return m;
            }
            return null;
        }

        // Gün başı: gebelerin günü azalır, zamanı gelen doğurur; sonra her sürüde (tür + oda) bir dişi gebe kalabilir.
        // Gebe kalmak için: erişkin erkek, tok ve sağlıklı dişi, dostluk ve konfor eşiği, barınakta yavruya yer.
        private int Breed(DayReport report = null)
        {
            int births = 0;
            foreach (var mother in new List<Animal>(_animals))
            {
                if (mother.IsDead || !mother.IsPregnant) continue;
                mother.PregnantDays = Math.Max(0, mother.PregnantDays - 1.0);
                if (mother.PregnantDays > 1e-9) continue;
                births += GiveBirth(mother);
            }

            double chance = BreedChance(Math.Max(1, HusbandryLevel?.Invoke() ?? 1));
            var groups = new Dictionary<string, List<Animal>>();
            foreach (var a in _animals)
            {
                if (a.IsDead || !a.IsFemale || !a.Species.Breeds || a.IsPregnant || a.IsSick || !a.IsMature || a.Fullness < 60f) continue;
                if (a.Friendship < BreedFriendship || a.Comfort < BreedComfort) continue;
                var home = _world.GetItem(a.HomeItemId);
                if (home == null) continue;
                var room = RoomOf(home);
                string key = a.Species.Id + "|" + (room != null ? "r" + room.Tiles[0] : "h" + home.Id);
                if (!groups.TryGetValue(key, out var list)) { list = new List<Animal>(); groups[key] = list; }
                list.Add(a);
            }
            var keys = new List<string>(groups.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (var key in keys)
            {
                var list = groups[key];
                list.Sort((x, y) => y.Friendship != x.Friendship ? y.Friendship.CompareTo(x.Friendship) : x.Id.CompareTo(y.Id));
                var female = list[0];                                     // en dost dişi
                var father = FindMate(female);
                if (father == null) continue;
                if (FreeHomeNear(female.Species, _world.GetItem(female.HomeItemId)) == null) continue;
                if (_rng.NextDouble() >= chance) continue;
                female.PregnantDays = Math.Max(1.0, Math.Ceiling(female.Species.GestationDays));
                _fathers[female.Id] = father.Id;
                if (report != null) report.Conceived++;
                Conceived?.Invoke(female, father.Id);
            }
            return births;
        }

        // Babanın kimliği (kuşak hesabı için; kayda girmez, yoksa annenin kuşağı sayılır).
        private readonly Dictionary<int, int> _fathers = new Dictionary<int, int>();

        // Doğum: yavru sayısı türün aralığından, barınakta yer kadar. Yer yoksa doğum bekler (gün sonra yeniden).
        private int GiveBirth(Animal mother)
        {
            var home = _world.GetItem(mother.HomeItemId);
            var nest = home != null ? FreeHomeNear(mother.Species, home) : null;
            if (nest == null) { mother.PregnantDays = 0.0001; BirthDelayed?.Invoke(mother); return 0; }
            mother.PregnantDays = 0;
            int litter = mother.Species.LitterMin + _rng.Next(mother.Species.LitterMax - mother.Species.LitterMin + 1);
            int gen = mother.Generation;
            Animal father = null;
            if (_fathers.TryGetValue(mother.Id, out var fid)) { father = Get(fid); if (father != null) gen = Math.Max(gen, father.Generation); _fathers.Remove(mother.Id); }
            int born = 0;
            for (int i = 0; i < litter; i++)
            {
                nest = FreeHomeNear(mother.Species, home);
                if (nest == null) break;
                var baby = Spawn(mother.Species, nest, _rng.NextDouble() < 0.5);
                baby.MotherId = mother.Id;
                baby.Generation = gen + 1;
                // Soy kademesi (29): ebeveyn ortalaması, %15 bir üstü (Hayvancılık dalıyla artabilir), %10 bir altı.
                baby.Lineage = AnimalProgress.InheritLineage(_rng, mother, father, LineageUpBonus?.Invoke() ?? 0.0, out var descent);
                baby.LegendaryDescent = descent;
                baby.Friendship = mother.Friendship * InheritFriendship;
                baby.Fullness = 100f;
                born++;
                Born?.Invoke(mother, baby);
            }
            return born;
        }

        // Annenin barınağıyla aynı odadaki (dış barınakta kendisi) boş yeri olan barınak.
        private ItemInstance FreeHomeNear(AnimalSpeciesDef species, ItemInstance home)
        {
            var room = RoomOf(home);
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || it.Def.HousingKind != species.Housing || Residents(it.Id) >= it.Def.HousingCapacity) continue;
                if (room != null ? InRoom(room, it) : it.Id == home.Id) return it;
            }
            return null;
        }

        /// <summary>
        /// Yeni gün: otlak otu büyür (kış hariç), konfor yeniden hesaplanır; toplama sandığı odadaki barınakların ürününü
        /// depoya alır, yem oluğu odadaki yemlikleri ve barınakları envanterdeki yemle doldurur.
        /// </summary>
        public DayReport OnNewDay(Season season, Household house = null, StorageService storage = null, string feedItemId = "item.feed")
        {
            var report = new DayReport();
            if (season != Season.Winter)
            {
                var keys = new List<GridPos>(_grass.Keys);
                foreach (var k in keys)
                {
                    float g = Math.Min(1f, _grass[k] + GrassRegrowPerDay);
                    if (g >= 1f) _grass.Remove(k); else _grass[k] = g;
                }
            }
            foreach (var a in _animals)
            {
                if (a.IsDead) continue;
                a.Comfort = ComfortOf(_world.GetItem(a.HomeItemId)).Total;
            }
            UpdateHealth(season, report);
            report.Births = Breed(report);
            if (house == null) return report;

            foreach (var helper in new List<ItemInstance>(_world.Items))
            {
                if (helper.IsDecor) continue;
                if (helper.Def.AnimalRole == AnimalRoles.Collector)
                    foreach (var home in HousingAround(helper))
                        foreach (var kv in TakeStock(home))
                        {
                            int added = storage != null ? storage.AddUpTo(kv.Key, kv.Value) : AddAll(house, kv.Key, kv.Value);
                            PutStock(home, kv.Key, kv.Value - added);
                            report.Collected += added;
                        }
                if (helper.Def.AnimalRole == AnimalRoles.Feeder)
                    foreach (var feeder in FeedersAround(helper))
                    {
                        double want = DailyFeedNeed(feeder) * 1.5 - feeder.FeedStock;
                        while (want > 0 && house.TryTakeItem(feedItemId))
                        {
                            feeder.FeedStock += 1;
                            want -= 1;
                            report.FeedAdded++;
                        }
                    }
            }
            return report;
        }

        private static int AddAll(Household house, string id, int count) { house.AddItem(id, count); return count; }

        // Yardımcı eşyanın (toplama sandığı, yem oluğu) odasındaki barınaklar.
        private List<ItemInstance> HousingAround(ItemInstance helper)
        {
            var list = new List<ItemInstance>();
            var room = _world.Map.Rooms.RoomAt(helper.Position);
            if (room == null || !room.IsIndoor) return list;
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.IsHousing && InRoom(room, it)) list.Add(it);
            return list;
        }

        // Yem oluğunun dolduracağı yemlikler (yoksa odadaki barınakların kendisi).
        private List<ItemInstance> FeedersAround(ItemInstance helper)
        {
            var list = new List<ItemInstance>();
            var room = _world.Map.Rooms.RoomAt(helper.Position);
            if (room == null || !room.IsIndoor) return list;
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.IsTrough && InRoom(room, it)) list.Add(it);
            if (list.Count == 0) list.AddRange(HousingAround(helper));
            return list;
        }

        /// <summary>Kayıt: yenmiş otlak karoları.</summary>
        public List<KeyValuePair<GridPos, float>> ExportGrass() => new List<KeyValuePair<GridPos, float>>(_grass);

        public void RestoreGrass(IEnumerable<KeyValuePair<GridPos, float>> grass)
        {
            _grass.Clear();
            if (grass == null) return;
            foreach (var kv in grass) if (kv.Value >= 0f && kv.Value < 1f) _grass[kv.Key] = kv.Value;
        }

        // ---------- tek hayvanla iş: sevme, tımar, eyer, binme ----------

        /// <summary>Binici bineği giremeyeceği yere (ev, üst kat) girince kendiliğinden indi (hayvan, binici kimliği).</summary>
        public event Action<Animal, int> Dismounted;

        // Binek binicisini buraya taşıyabilir mi? Zemin kat ve dışarısı ya da kendi ahırı; evin içine girmez.
        private bool CanCarryTo(ItemInstance home, GridPos p)
        {
            if (p.Z != 0) return false;
            var room = _world.Map.Rooms.RoomAt(p);
            return room == null || !room.IsIndoor || room == RoomOf(home);
        }

        /// <summary>Binicinin üstünde olduğu hayvan; yoksa null.</summary>
        public Animal MountOf(int riderId)
        {
            if (riderId == 0) return null;
            foreach (var a in _animals) if (!a.IsDead && a.RiderId == riderId) return a;
            return null;
        }

        /// <summary>Binicinin kendinden gelen ek hız çarpanı (Doğuştan Binici; belge 31-8).</summary>
        public Func<int, double> RiderSpeedFactor { get; set; }

        /// <summary>Binicinin yürüme hızı çarpanı (binmiyorsa 1). İşlenmiş eyer %25 daha hızlı.</summary>
        public double RideSpeed(int riderId)
        {
            var a = MountOf(riderId);
            if (a == null) return 1.0;
            double speed = a.Species.RideSpeed;
            if (a.Saddle != null && _content.Items.TryGet(a.Saddle, out var s) && s.HasTag("fine")) speed *= 1.25;
            // Binicilik: seviye başına %3 (10. seviyede %27).
            var rider = FindCharacter?.Invoke(riderId);
            if (rider != null) speed *= 1.0 + 0.03 * Math.Max(0, rider.Skills.GetLevel(SkillType.Riding) - 1);
            if (RiderSpeedFactor != null) speed *= RiderSpeedFactor(riderId);
            // Dörtnala (29): +%40, dayanıklılık bitene dek.
            if (a.Galloping && a.Stamina > 0) speed *= HorseAbilities.GallopSpeed;
            // Efsane "Panayır Rüzgârı": +%10.
            if (a.Lineage == AnimalLineage.Legendary && a.Species.Rideable) speed *= 1.1;
            return speed;
        }

        public const float MountFriendship = 10f;
        public const float BrushFriendship = 5f;

        public string MountBlocker(int riderId, Animal a)
        {
            if (a == null || a.IsDead) return "Hayvan bulunamadı.";
            if (!a.Species.Rideable) return $"{a.Name} binek değil.";
            if (!a.IsMature) return $"{a.Name} daha tay; büyüyünce binilir.";
            if (a.Saddle == null) return "Önce eyerle.";
            if (a.RiderId != 0) return "Üstünde zaten biri var.";
            if (MountOf(riderId) != null) return "Zaten bir hayvanın üstündesin.";
            if (a.Friendship < MountFriendship) return $"{a.Name} sana henüz güvenmiyor (sev, tımar et).";
            return null;
        }

        public bool Mount(int riderId, Animal a)
        {
            if (MountBlocker(riderId, a) != null) return false;
            a.RiderId = riderId;
            a.Path = null;
            a.NextTile = null;
            return true;
        }

        /// <summary>Binicinin hayvanından iner; hayvan olduğu yerden barınağına yürür.</summary>
        public Animal Dismount(int riderId)
        {
            var a = MountOf(riderId);
            if (a == null) return null;
            a.RiderId = 0;
            a.Path = null;
            a.Target = a.Position;
            return a;
        }

        /// <summary>Envanterdeki en iyi eyer (işlenmiş önce); yoksa null.</summary>
        public string BestSaddle(Household house)
        {
            string best = null;
            foreach (var d in _content.Items.All)
            {
                if (!d.HasTag("saddle") || house.Count(d.Id) <= 0) continue;
                if (best == null || d.HasTag("fine")) best = d.Id;
            }
            return best;
        }

        public string SaddleBlocker(Animal a, Household house)
        {
            if (a == null || a.IsDead) return "Hayvan bulunamadı.";
            if (!a.Species.Rideable) return $"{a.Name} eyerlenmez.";
            if (a.Saddle != null) return "Zaten eyerli.";
            if (BestSaddle(house) == null) return "Envanterde eyer yok (saraç tezgâhında yap ya da pazardan al).";
            return null;
        }

        public bool SaddleUp(Animal a, Household house)
        {
            if (SaddleBlocker(a, house) != null) return false;
            var saddle = BestSaddle(house);
            house.TryTakeItem(saddle);
            a.Saddle = saddle;
            return true;
        }

        public bool Unsaddle(Animal a, Household house)
        {
            if (a == null || a.Saddle == null || a.RiderId != 0) return false;
            house.AddItem(a.Saddle);
            a.Saddle = null;
            return true;
        }

        /// <summary>Tek hayvanı sever (günde bir). husbandryLevel: sevenin hayvancılık seviyesi.</summary>
        public bool PetOne(Animal a, int husbandryLevel = 1)
        {
            if (a == null || a.IsDead || a.PetCooldownDays > 0) return false;
            a.Friendship = Math.Min(100f, a.Friendship + PetFriendship * HusbandryFactor(husbandryLevel));
            a.PetCooldownDays = 1.0;
            AnimalProgress.AddXp(a, AnimalProgress.XpPet);
            return true;
        }

        /// <summary>Tımar (binek ve büyükbaş): sevmeyle aynı günlük hak, biraz daha dostluk ve o günün konforuna +10.</summary>
        public bool Brush(Animal a, int husbandryLevel = 1)
        {
            if (a == null || a.IsDead || a.PetCooldownDays > 0) return false;
            a.Friendship = Math.Min(100f, a.Friendship + (PetFriendship + BrushFriendship) * HusbandryFactor(husbandryLevel));
            a.Comfort = Math.Min(100f, a.Comfort + 10f);
            a.PetCooldownDays = 1.0;
            AnimalProgress.AddXp(a, AnimalProgress.XpBrush);
            return true;
        }

        /// <summary>Yavrunun soyunun bir üst kademe çıkma şansına eklenen (Hayvancılık dalı vb.); yoksa 0.</summary>
        public Func<double> LineageUpBonus { get; set; }

        /// <summary>Tüccar, panayır ya da hile: hayvanın soy kademesini koyar (Efsane buradan verilmez).</summary>
        public void SetLineage(Animal a, AnimalLineage lineage)
        {
            if (a == null || lineage == AnimalLineage.Legendary) return;
            a.Lineage = lineage;
        }

        /// <summary>Hayvana doğrudan XP verir (yarış, olay); seviye atladıysa yeni seviye, yoksa 0.</summary>
        public int AddAnimalXp(Animal a, double xp) => AnimalProgress.AddXp(a, xp);

        // ---------- at yetenekleri (29) ----------

        /// <summary>Atın binicisi yere düştü (yaralanma ölümcül değildir; InjurySystem'e bağlanır).</summary>
        public event Action<Animal, Character> RiderFell;
        /// <summary>At engelin önünde ürktü (binici düşmedi).</summary>
        public event Action<Animal> HorseRefused;

        /// <summary>Dörtnala aç/kapa (buton basılı tutulurken açık). Yetenek, dayanıklılık ya da ürkeklik izin vermezse false.</summary>
        public bool SetGallop(int riderId, bool on)
        {
            var a = MountOf(riderId);
            if (a == null) return false;
            if (!on) { a.Galloping = false; return true; }
            var rider = FindCharacter?.Invoke(riderId);
            if (!HorseAbilities.Has(HorseAbility.Gallop, a, rider) || a.Stamina <= 0 || a.ShyDays > 0) return false;
            a.Galloping = true;
            return true;
        }

        /// <summary>İkram (yulaf, elma): dayanıklılık +50, dostluk +2.</summary>
        public bool GiveTreat(Animal a)
        {
            if (a == null || a.IsDead || !a.Species.Rideable) return false;
            a.Stamina = Math.Min(HorseAbilities.MaxStamina, a.Stamina + HorseAbilities.TreatStamina);
            a.Friendship = Math.Min(100f, a.Friendship + 2f);
            return true;
        }

        /// <summary>
        /// Binici engel atlar (2 karo; 3 karo yalnızca Soylu/Efsane). Başarıda at ve binici XP alır. Başarısızlıkta at ürker
        /// (1 gün dörtnala/atlama yok, dostluk −3) ve %50 şansla binici düşer: iner, RiderFell tetiklenir.
        /// </summary>
        public JumpOutcome TryJump(int riderId, int tiles, bool badWeather = false, int extraPoints = 0)
        {
            var a = MountOf(riderId);
            var rider = FindCharacter?.Invoke(riderId);
            if (a == null) return JumpOutcome.NotAllowed;
            double chance = HorseAbilities.JumpChance(a, rider, tiles, badWeather, extraPoints);
            if (chance <= 0) return JumpOutcome.NotAllowed;
            if (_rng.NextDouble() < chance)
            {
                AnimalProgress.AddXp(a, AnimalProgress.XpJump);
                rider?.GainSkillXp(SkillType.Riding, 3);
                return JumpOutcome.Cleared;
            }
            a.ShyDays = HorseAbilities.ShyDaysAfterRefusal;
            a.Galloping = false;
            a.Friendship = Math.Max(0f, a.Friendship - HorseAbilities.RefusalFriendshipLoss);
            if (_rng.NextDouble() < 0.5)
            {
                Dismount(riderId);
                if (rider != null) RiderFell?.Invoke(a, rider);
                return JumpOutcome.Fell;
            }
            HorseRefused?.Invoke(a);
            return JumpOutcome.Refused;
        }
    }

    /// <summary>Karakterin hayvanla yaptığı işler (JobRunner'ın hayvan işi): sev, tımar, eyer, bin.</summary>
    public sealed class AnimalCare : IAnimalEffects
    {
        private readonly AnimalSystem _animals;
        private readonly Household _house;

        // ---- beceri işleri (belge 45) ----
        public const int TrainLevel = 5, HoundTrainLevel = 8, TrickLevel = 7, TreatLevel = 4, PrepLevel = 9;
        public const double PrepScore = 8;
        /// <summary>Yarışa hazırlanmış hayvanlar (ilk yarışmada +8 puan; yarışma kullanınca düşer).</summary>
        public HashSet<int> Prepared { get; } = new HashSet<int>();
        /// <summary>Bugünün sayacı (gün); bağlanmamışsa günlük sınır uygulanmaz.</summary>
        public Func<long> Day { get; set; }
        private long Today => Day != null ? Day() : -1;
        private readonly HashSet<(int, AnimalAction, long)> _done = new HashSet<(int, AnimalAction, long)>();
        private readonly Random _rng = new Random(4517);
        private bool DoneToday(int id, AnimalAction act) => Today >= 0 && _done.Contains((id, act, Today));

        public AnimalCare(AnimalSystem animals, Household house)
        {
            _animals = animals ?? throw new ArgumentNullException(nameof(animals));
            _house = house ?? throw new ArgumentNullException(nameof(house));
        }

        /// <summary>Bir iş yapıldı (bildirim ve hesap XP'si için).</summary>
        public event Action<Character, Animal, AnimalAction> Done;

        public GridPos? PositionOf(int animalId)
        {
            var a = _animals.Get(animalId);
            return a == null || a.IsDead ? (GridPos?)null : a.Position;
        }

        public string CanDo(Character actor, int animalId, AnimalAction action)
        {
            var a = _animals.Get(animalId);
            if (a == null || a.IsDead) return "Hayvan bulunamadı.";
            switch (action)
            {
                case AnimalAction.Pet:
                case AnimalAction.Brush:
                    return a.PetCooldownDays > 0 ? $"{a.Name} bugün zaten sevildi." : null;
                case AnimalAction.Saddle: return _animals.SaddleBlocker(a, _house);
                case AnimalAction.Unsaddle: return a.Saddle == null ? "Eyer yok." : a.RiderId != 0 ? "Önce in." : null;
                case AnimalAction.Mount: return _animals.MountBlocker(actor.Id, a);
                case AnimalAction.Train:
                {
                    bool hound = a.Species.Id.Contains("dog");
                    bool can = actor.Skills.GetLevel(SkillType.AnimalHusbandry) >= TrainLevel || (hound && actor.Skills.GetLevel(SkillType.Hunting) >= HoundTrainLevel);
                    if (!can) return hound ? $"Hayvancılık {TrainLevel} ya da Avcılık {HoundTrainLevel} gerekli." : $"Hayvancılık {TrainLevel} gerekli.";
                    return DoneToday(a.Id, action) ? $"{a.Name} bugün eğitildi." : null;
                }
                case AnimalAction.Trick:
                    if (!a.Species.Rideable) return "Yalnızca binek hayvanına.";
                    if (actor.Skills.GetLevel(SkillType.Riding) < TrickLevel) return $"Binicilik {TrickLevel} gerekli.";
                    return DoneToday(a.Id, action) ? $"{a.Name} bugün çalıştı." : null;
                case AnimalAction.Treat:
                    if (actor.Skills.GetLevel(SkillType.Medicine) < TreatLevel) return $"Hekimlik {TreatLevel} gerekli.";
                    if (!a.IsSick) return $"{a.Name} hasta değil.";
                    return DoneToday(a.Id, action) ? $"{a.Name} bugün muayene edildi." : null;
                case AnimalAction.PrepContest:
                    if (actor.Skills.GetLevel(SkillType.AnimalHusbandry) < PrepLevel) return $"Hayvancılık {PrepLevel} gerekli.";
                    return Prepared.Contains(a.Id) ? $"{a.Name} yarışmaya hazır." : null;
                default: return "Bilinmeyen iş.";
            }
        }

        public double Duration(AnimalAction action)
        {
            switch (action)
            {
                case AnimalAction.Brush: return 0.05;
                case AnimalAction.Saddle: return 0.03;
                case AnimalAction.Unsaddle: return 0.02;
                case AnimalAction.Mount: return 0.01;
                case AnimalAction.Train: case AnimalAction.Trick: return 0.08;
                case AnimalAction.Treat: return 0.06;
                case AnimalAction.PrepContest: return 0.1;
                default: return 0.02;
            }
        }

        public void Apply(Character actor, int animalId, AnimalAction action)
        {
            var a = _animals.Get(animalId);
            if (a == null) return;
            bool ok;
            int husb = actor.Skills.GetLevel(SkillType.AnimalHusbandry);
            switch (action)
            {
                case AnimalAction.Pet: ok = _animals.PetOne(a, husb); break;
                case AnimalAction.Brush: ok = _animals.Brush(a, husb); break;
                case AnimalAction.Saddle: ok = _animals.SaddleUp(a, _house); break;
                case AnimalAction.Unsaddle: ok = _animals.Unsaddle(a, _house); break;
                case AnimalAction.Mount: ok = _animals.Mount(actor.Id, a); break;
                case AnimalAction.Train:
                    _done.Add((a.Id, action, Today));
                    _animals.AddAnimalXp(a, 15);
                    a.Friendship = Math.Min(100f, a.Friendship + 4f);
                    if (a.Species.Id.Contains("dog")) actor.GainSkillXp(SkillType.Hunting, 6);
                    ok = true; break;
                case AnimalAction.Trick:
                    _done.Add((a.Id, action, Today));
                    _animals.AddAnimalXp(a, 12);
                    a.Friendship = Math.Min(100f, a.Friendship + 3f);
                    actor.Needs.Restore(Thatchwick.Core.Characters.NeedType.Fun, 15f);
                    ok = true; break;
                case AnimalAction.Treat:
                {
                    _done.Add((a.Id, action, Today));
                    int med = actor.Skills.GetLevel(SkillType.Medicine);
                    if (_rng.NextDouble() < Math.Min(0.95, 0.4 + 0.07 * med)) _animals.Cure(a);
                    actor.GainSkillXp(SkillType.Medicine, 10);
                    ok = true; break;
                }
                case AnimalAction.PrepContest:
                    Prepared.Add(a.Id);
                    _animals.AddAnimalXp(a, 5);
                    ok = true; break;
                default: ok = false; break;
            }
            if (ok)
            {
                // Beceri: sevmek hayvancılık; tımar, eyer ve binmek binicilik (tımar biraz da hayvancılık).
                switch (action)
                {
                    case AnimalAction.Pet: actor.GainSkillXp(SkillType.AnimalHusbandry, 4); break;
                    case AnimalAction.Brush: actor.GainSkillXp(SkillType.Riding, 5); actor.GainSkillXp(SkillType.AnimalHusbandry, 2); break;
                    case AnimalAction.Saddle: actor.GainSkillXp(SkillType.Riding, 2); break;
                    case AnimalAction.Mount: actor.GainSkillXp(SkillType.Riding, 1); break;
                    case AnimalAction.Train: actor.GainSkillXp(SkillType.AnimalHusbandry, 8); break;
                    case AnimalAction.Trick: actor.GainSkillXp(SkillType.Riding, 10); break;
                    case AnimalAction.PrepContest: actor.GainSkillXp(SkillType.AnimalHusbandry, 10); break;
                }
                Done?.Invoke(actor, a, action);
            }
        }
    }
}
