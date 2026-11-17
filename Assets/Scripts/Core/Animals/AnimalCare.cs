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
                if (room != null && it.Def.HasTag("light")) r.Light = 5f;
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

        /// <summary>Hayvanın yaşam alanı: barınak odası (yoksa barınak çevresi) ve dışarı çıkabiliyorsa otlak.</summary>
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
            if (Outside) foreach (var t in PastureTiles()) if (t.Z == a.Position.Z) set.Add(t);
            return set;
        }

        // Yaşam alanından rastgele bir hedef seçip oraya alan içinde yol bulur. Alanın dışındaysa (gece otlakta kaldı,
        // yeni barınağa taşındı) eve haritadaki en kısa yoldan döner.
        private void PlanWalk(Animal a, ItemInstance home)
        {
            var area = LivingArea(a, home);
            if (area.Count == 0) { a.Path = null; return; }
            var list = new List<GridPos>(area);
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
            public int Collected, FeedAdded;
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

        /// <summary>Binicinin yürüme hızı çarpanı (binmiyorsa 1). İşlenmiş eyer %25 daha hızlı.</summary>
        public double RideSpeed(int riderId)
        {
            var a = MountOf(riderId);
            if (a == null) return 1.0;
            double speed = a.Species.RideSpeed;
            if (a.Saddle != null && _content.Items.TryGet(a.Saddle, out var s) && s.HasTag("fine")) speed *= 1.25;
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

        /// <summary>Tek hayvanı sever (günde bir).</summary>
        public bool PetOne(Animal a)
        {
            if (a == null || a.IsDead || a.PetCooldownDays > 0) return false;
            a.Friendship = Math.Min(100f, a.Friendship + PetFriendship);
            a.PetCooldownDays = 1.0;
            return true;
        }

        /// <summary>Tımar (binek ve büyükbaş): sevmeyle aynı günlük hak, biraz daha dostluk ve o günün konforuna +10.</summary>
        public bool Brush(Animal a)
        {
            if (a == null || a.IsDead || a.PetCooldownDays > 0) return false;
            a.Friendship = Math.Min(100f, a.Friendship + PetFriendship + BrushFriendship);
            a.Comfort = Math.Min(100f, a.Comfort + 10f);
            a.PetCooldownDays = 1.0;
            return true;
        }
    }

    /// <summary>Karakterin hayvanla yaptığı işler (JobRunner'ın hayvan işi): sev, tımar, eyer, bin.</summary>
    public sealed class AnimalCare : IAnimalEffects
    {
        private readonly AnimalSystem _animals;
        private readonly Household _house;

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
                default: return 0.02;
            }
        }

        public void Apply(Character actor, int animalId, AnimalAction action)
        {
            var a = _animals.Get(animalId);
            if (a == null) return;
            bool ok;
            switch (action)
            {
                case AnimalAction.Pet: ok = _animals.PetOne(a); break;
                case AnimalAction.Brush: ok = _animals.Brush(a); break;
                case AnimalAction.Saddle: ok = _animals.SaddleUp(a, _house); break;
                case AnimalAction.Unsaddle: ok = _animals.Unsaddle(a, _house); break;
                case AnimalAction.Mount: ok = _animals.Mount(actor.Id, a); break;
                default: ok = false; break;
            }
            if (ok) Done?.Invoke(actor, a, action);
        }
    }
}
