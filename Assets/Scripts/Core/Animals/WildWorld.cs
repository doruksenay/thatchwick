using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    /// <summary>Haritada dolaşan bir yaban hayvanı (31-7): kenardan girer, bir süre dolaşır, çıkar.</summary>
    public sealed class WildVisitor
    {
        public int Id;
        public WildSpecies Species;
        public GridPos Position;
        public GridPos? NextTile;
        public double StepProgress;
        public double LeaveAt;      // oyun günü
        public bool Young;
        public bool Fleeing;
    }

    /// <summary>
    /// Yaban ziyaretçileri (belge 31): mevsime ve gündüz/geceye uygun türler arada bir haritaya girer, gezer ve çıkar.
    /// İlk görüldüğünde Doğa Defteri'ne işlenir. Sevilebilir türler "sakin yaklaş" ile sevilir, avlanabilir türler yayla vurulur.
    /// Kurt ve ayı ziyaretçi olarak gelmez (kurt baskın sistemiyle, ayı yalnızca uzakta görülür ve deftere yazılır).
    /// Kement tuzakları her gün tuzağa düşen bir türü yakalayabilir.
    /// </summary>
    public sealed class WildRoamers
    {
        public const int MaxVisitors = 4;
        /// <summary>Gün başına ortalama gelen ziyaretçi.</summary>
        public const double ArrivalsPerDay = 3.0;
        public const double StayDaysMin = 0.15, StayDaysMax = 0.45;
        public const double StepsPerDay = 40.0;
        public const int MaxSnares = 3;
        public const double BearSightingChance = 0.04;

        private readonly GridMap _map;
        private readonly Random _rng;
        private readonly List<WildVisitor> _visitors = new List<WildVisitor>();
        private int _nextId = 1;

        public NatureJournal Journal { get; set; }
        public IReadOnlyList<WildVisitor> Visitors => _visitors;

        /// <summary>Doğa Defteri'ne ilk kez işlenen tür.</summary>
        public event Action<WildSpecies> FirstSeen;

        public WildRoamers(GridMap map, int seed = 211)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _rng = new Random(seed);
        }

        public static bool IsNight(double dayFraction) => dayFraction < 0.22 || dayFraction > 0.85;

        /// <summary>Şu an haritaya gelebilecek türler (kurt ve ayı hariç).</summary>
        public static List<WildSpecies> Candidates(Season season, bool night, string regionId = null)
        {
            var list = new List<WildSpecies>();
            foreach (var s in WildCatalog.All)
            {
                if (s.Id == "wild.wolf" || s.Id == "wild.bear") continue;
                // Kıyı türü yalnızca kıyıda; öteki türler kıyıda görülmez (bölgesiz haritada kıyı türü yok).
                if (s.Regions != null ? regionId == null || Array.IndexOf(s.Regions, regionId) < 0 : regionId == "coast") continue;
                if (s.Seasons != null && Array.IndexOf(s.Seasons, season) < 0) continue;
                if (s.Night != night) continue;
                list.Add(s);
            }
            return list;
        }

        public void Tick(double gameDays, double now, Season season, double dayFraction)
        {
            if (gameDays <= 0) return;
            bool night = IsNight(dayFraction);
            // Gelen
            if (_visitors.Count < MaxVisitors && _rng.NextDouble() < ArrivalsPerDay * gameDays)
            {
                var area = SpawnRegion();
                var c = Candidates(season, night, area?.Id);
                if (c.Count > 0) Arrive(c[_rng.Next(c.Count)], now, season == Season.Spring && _rng.NextDouble() < 0.3, area);
            }
            // Dolaşan
            for (int i = _visitors.Count - 1; i >= 0; i--)
            {
                var v = _visitors[i];
                bool wrongTime = v.Species.Night != night;
                if (now >= v.LeaveAt || wrongTime) v.Fleeing = true;
                v.StepProgress += gameDays * StepsPerDay * (v.Fleeing ? 2 : 1);
                while (v.StepProgress >= 1)
                {
                    v.StepProgress -= 1;
                    if (v.NextTile.HasValue) v.Position = v.NextTile.Value;
                    if (v.Fleeing && IsEdge(v.Position)) { _visitors.RemoveAt(i); goto next; }
                    v.NextTile = PickStep(v);
                }
                next:;
            }
        }

        /// <summary>Gelen ziyaretçi (test ve sahne için de kullanılır).</summary>
        public WildVisitor Arrive(WildSpecies s, double now, bool young = false, Region area = null)
        {
            if (area == null && s.Regions != null && Regions.IsRegional(_map))
                foreach (var r in Regions.All) if (Array.IndexOf(s.Regions, r.Id) >= 0 && Regions.Exists(_map, r)) { area = r; break; }
            var pos = EdgeTile(area);
            if (!pos.HasValue) return null;
            var v = new WildVisitor { Id = _nextId++, Species = s, Position = pos.Value, LeaveAt = now + StayDaysMin + _rng.NextDouble() * (StayDaysMax - StayDaysMin), Young = young };
            _visitors.Add(v);
            if (Journal != null)
            {
                if (Journal.Mark(s.Id, JournalMark.Seen)) FirstSeen?.Invoke(s);
                if (young) Journal.Mark(s.Id, JournalMark.YoungSeen);
            }
            return v;
        }

        /// <summary>Uzaktan ayı görmek: deftere yazılır, haritaya girmez.</summary>
        public bool TryBearSighting(Season season)
        {
            if (Journal == null || season == Season.Winter || _rng.NextDouble() >= BearSightingChance) return false;
            if (Journal.Mark("wild.bear", JournalMark.Seen)) { FirstSeen?.Invoke(WildCatalog.Get("wild.bear")); return true; }
            return false;
        }

        public WildVisitor Find(int id) { foreach (var v in _visitors) if (v.Id == id) return v; return null; }

        public WildVisitor At(GridPos tile)
        {
            WildVisitor best = null;
            foreach (var v in _visitors)
            {
                if (v.Position.Z != tile.Z) continue;
                if (Math.Abs(v.Position.X - tile.X) + Math.Abs(v.Position.Y - tile.Y) > 1) continue;
                if (best == null || v.Position == tile) best = v;
            }
            return best;
        }

        /// <summary>Sevme şansı: (1 − ürkeklik) + Hayvancılık seviye başına %4; yavru daha kolay.</summary>
        public static double PetChance(WildSpecies s, int husbandryLevel, bool young) =>
            s == null || !s.Pettable ? 0 : Math.Max(0.05, Math.Min(0.95, 1 - s.Shyness + 0.04 * (husbandryLevel - 1) + (young ? 0.15 : 0)));

        /// <summary>Sakin yaklaşıp sevmeyi dener. Başarıda deftere "sevildi" yazılır; başarısızlıkta hayvan kaçar.</summary>
        public string TryPet(int visitorId, Character c)
        {
            var v = Find(visitorId);
            if (v == null) return "Gitti bile.";
            if (!v.Species.Pettable) return $"{v.Species.Name} sevilmez; uzaktan izle.";
            double chance = PetChance(v.Species, c?.Skills.GetLevel(SkillType.AnimalHusbandry) ?? 1, v.Young);
            if (_rng.NextDouble() < chance)
            {
                Journal?.Mark(v.Species.Id, JournalMark.Petted);
                c?.GainSkillXp(SkillType.AnimalHusbandry, 2);
                v.LeaveAt = Math.Max(v.LeaveAt, v.LeaveAt + 0.05);
                return $"{v.Species.Name} elini kokladı, kaçmadı.";
            }
            v.Fleeing = true;
            return $"{v.Species.Name} ürktü, kaçtı.";
        }

        /// <summary>Av köpeğinin genetik katkısı (belge 48): (ek isabet şansı, ek et). Yol bilen +%15, Yumuşak ağız +1.</summary>
        public Func<(double, int)> HoundEffects { get; set; }
        /// <summary>Köpekle yapılan av başarılı (efsane yolu "İz Ustası" sayacı).</summary>
        public event Action HoundSucceeded;

        /// <summary>Yayla avlama: av ürünleri haneye eklenir, hayvan haritadan çıkar ya da kaçar.</summary>
        public HuntResult Shoot(int visitorId, Character hunter, Household house, bool houndHelps)
        {
            var v = Find(visitorId);
            if (v == null) return new HuntResult { Message = "Gitti bile." };
            bool motherWithYoung = false;
            foreach (var o in _visitors) if (o != v && o.Young && o.Species == v.Species) motherWithYoung = !v.Young;
            var hound = houndHelps && HoundEffects != null ? HoundEffects() : (0.0, 0);
            var r = Hunting.Try(_rng, v.Species.Id, HuntMethod.Bow, hunter, houndHelps, motherWithYoung, hound.Item1);
            if (r.Success)
            {
                if (houndHelps && r.MeatCount > 0) r.MeatCount += hound.Item2;   // Yumuşak ağız (belge 48)
                if (houndHelps) HoundSucceeded?.Invoke();
                _visitors.Remove(v);
                Give(r, house, hunter, 0);
                Journal?.Mark(v.Species.Id, JournalMark.Hunted);
            }
            else if (v.Species.Huntable) v.Fleeing = true;
            return r;
        }

        /// <summary>Arsadaki kement yabandakinin yarısı kadar tutar: evin yanına av az gelir.</summary>
        public const double HomeSnareFactor = 0.5;
        /// <summary>Kementin kurulabildiği yaban bölgeleri (şehir, yol ve deniz kıyısı hariç).</summary>
        public static readonly string[] SnareRegions = { "forest", "lake", "marsh", "hill", "slopes" };

        public static bool IsSnareGround(GridMap map, GridPos p)
        {
            if (!Regions.IsRegional(map)) return false;
            var region = Regions.At(p);
            return region != null && Array.IndexOf(SnareRegions, region.Id) >= 0;
        }

        /// <summary>
        /// Günlük tuzak kontrolü: haritaya kurulu her boş kement (en çok 3) tuzağa düşen türlerden birini yakalayabilir.
        /// Av haneye kendiliğinden gelmez: tuzakta bekler, biri gidip toplar. Dolu tuzak yeniden tutmaz.
        /// </summary>
        public List<HuntResult> CheckSnares(Season season, IEnumerable<ItemInstance> snares, Character hunter)
        {
            var results = new List<HuntResult>();
            var pool = new List<WildSpecies>();
            foreach (var s in WildCatalog.All)
                if (s.Trappable && (s.Seasons == null || Array.IndexOf(s.Seasons, season) >= 0)) pool.Add(s);
            if (pool.Count == 0 || snares == null) return results;
            int used = 0;
            foreach (var snare in snares)
            {
                if (snare == null || snare.IsDecor || !snare.Def.IsSnare) continue;
                if (used++ >= MaxSnares) break;
                if (snare.StockTotal > 0) continue;                                                   // dolu: önce toplanmalı
                if (!IsSnareGround(_map, snare.Position) && _rng.NextDouble() >= HomeSnareFactor) continue;
                var s = pool[_rng.Next(pool.Count)];
                var r = Hunting.Try(_rng, s.Id, HuntMethod.Trap, hunter);
                if (!r.Success) continue;
                int level = hunter?.Skills.GetLevel(SkillType.Hunting) ?? 1;
                foreach (var (id, n) in new[] { (r.Meat, r.MeatCount), (r.Hide, r.HideCount) })
                {
                    if (id == null || n <= 0) continue;
                    for (int u = 0; u < n; u++)   // tuzak avı bir basamak düşük kalite (belge 53 §4.3)
                        AnimalSystem.PutStock(snare, ItemKey.StockKey(id, Math.Max(0, ItemQuality.Gathered(level, _rng) - 1)), 1);
                }
                Journal?.Mark(s.Id, JournalMark.Hunted);
                results.Add(r);
            }
            return results;
        }

        // Av eti ve postu avcının Avcılığıyla kalite alır; tuzak bir basamak düşük (belge 53 §4.3).
        private void Give(HuntResult r, Household house, Character hunter, int penalty)
        {
            if (house == null) return;
            int level = hunter?.Skills.GetLevel(SkillType.Hunting) ?? 1;
            foreach (var (id, n) in new[] { (r.Meat, r.MeatCount), (r.Hide, r.HideCount) })
            {
                if (id == null || n <= 0) continue;
                for (int u = 0; u < n; u++) house.AddItem(id, 1, Math.Max(0, ItemQuality.Gathered(level, _rng) - penalty));
            }
        }

        // Bölgeli dünyada (çiftlik, köy yolu, şehir ve yaban haritaları) hayvan kendi bölgesinin kenarından girer ve çıkar;
        // ızgaranın kenarı ya da bölgeler arasındaki kapalı boşluk kullanılmaz. Şehre yaban hayvanı girmez; orman en sık.
        private RectI AreaOf(GridPos p)
        {
            if (Regions.IsRegional(_map)) { var r = Regions.At(p); if (r != null) return new RectI(r.X, r.Y, r.W, r.H); }
            return new RectI(0, 0, _map.Width, _map.Height);
        }

        private bool IsEdge(GridPos p)
        {
            var a = AreaOf(p);
            return p.X <= a.X || p.Y <= a.Y || p.X >= a.X + a.W - 1 || p.Y >= a.Y + a.H - 1;
        }

        /// <summary>Gelen hayvanın bölgesi (bölgesiz haritada null). Orman en sık; kıyı (varsa) gölle eşit.</summary>
        private Region SpawnRegion()
        {
            if (!Regions.IsRegional(_map)) return null;
            var pool = new List<Region> { Regions.Farm, Regions.Farm, Regions.Road };
            foreach (var r in new[] { Regions.Forest, Regions.Forest, Regions.Forest, Regions.Lake, Regions.Marsh, Regions.Hill, Regions.Slopes, Regions.Coast })
                if (Regions.Exists(_map, r)) pool.Add(r);
            return pool[_rng.Next(pool.Count)];
        }

        private GridPos? EdgeTile(Region area = null)
        {
            area = area ?? SpawnRegion();
            var a = area != null ? new RectI(area.X, area.Y, area.W, area.H) : new RectI(0, 0, _map.Width, _map.Height);
            for (int guard = 0; guard < 40; guard++)
            {
                int side = _rng.Next(4);
                int x = side == 0 ? a.X : side == 1 ? a.X + a.W - 1 : a.X + _rng.Next(a.W);
                int y = side == 2 ? a.Y : side == 3 ? a.Y + a.H - 1 : a.Y + _rng.Next(a.H);
                var p = new GridPos(x, y);
                if (_map.IsWalkable(p) && !_map.IsPortal(p)) return p;
            }
            return null;
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        private GridPos? PickStep(WildVisitor v)
        {
            var options = new List<GridPos>();
            GridPos best = v.Position; int bestEdge = int.MaxValue;
            foreach (var d in Dirs)
            {
                var n = v.Position + d;
                if (!_map.InBounds(n) || !_map.CanStep(v.Position, n)) continue;
                options.Add(n);
                var ar = AreaOf(v.Position);
                int edge = Math.Min(Math.Min(n.X - ar.X, n.Y - ar.Y), Math.Min(ar.X + ar.W - 1 - n.X, ar.Y + ar.H - 1 - n.Y));
                if (edge < bestEdge) { bestEdge = edge; best = n; }
            }
            if (options.Count == 0) return null;
            if (v.Fleeing) return best;
            return options[_rng.Next(options.Count)];
        }
    }

    /// <summary>
    /// Yaban hayvanlarının çiftliğe etkisi (belge 31): tilki kümesten tavuk kapar (tilki yavrusu ya da köpek önler),
    /// yaban domuzu yaz sonu ve sonbaharda ekin çiğner (ağıl ve köpek önler), sahiplenilen karga arada parlak hediye getirir.
    /// </summary>
    public sealed class WildMischief
    {
        public const double FoxChance = 0.04;
        public const double BoarChance = 0.05;
        public const int BoarCropsMax = 2;

        private readonly AnimalSystem _animals;
        private readonly WorldState _world;
        private readonly WildAdoptions _adoptions;
        private readonly Random _rng;

        /// <summary>Köpek tilkiyi ya da yaban domuzunu savdı (efsane yolu "Kapı Aslanı").</summary>
        public event Action DogGuarded;

        public WildMischief(AnimalSystem animals, WorldState world, WildAdoptions adoptions, int seed = 307)
        {
            _animals = animals; _world = world; _adoptions = adoptions; _rng = new Random(seed);
        }

        /// <summary>Dışarıda, uyanık yetişkin köpek var mı (dinlenen sayılmaz).</summary>
        public bool ActiveDog()
        {
            if (_animals == null) return false;
            foreach (var a in _animals.Animals)
                if (!a.IsDead && a.Species.Id == "animal.dog" && a.IsMature && a.ShyDays <= 0) return true;
            return false;
        }

        public List<Animal> ExposedPoultry()
        {
            var list = new List<Animal>();
            if (_animals == null || !_animals.Released) return list;
            foreach (var a in _animals.Animals)
                if (!a.IsDead && a.Species.Housing == "coop") list.Add(a);
            return list;
        }

        /// <summary>Tilki gecesi: kapılan hayvanı döner (önlenirse ya da olmazsa null). message: oyuncuya not.</summary>
        public Animal FoxNight(Season season, out string message, bool force = false)
        {
            message = null;
            if (season == Season.Winter && !force) return null;
            var poultry = ExposedPoultry();
            if (poultry.Count == 0) return null;
            if (!force && _rng.NextDouble() >= FoxChance) return null;
            if (_adoptions != null && _adoptions.FoxGuard) { message = "Gece bir tilki kümese yaklaştı; büyüttüğün tilki onu kovaladı."; return null; }
            if (ActiveDog()) { message = "Köpek gece kümese yaklaşan tilkiyi kovaladı."; DogGuarded?.Invoke(); return null; }
            poultry.Sort((x, y) => x.AgeDays.CompareTo(y.AgeDays));   // önce civciv
            var prey = poultry[0];
            _animals.KillByPredator(prey);
            message = $"Gece tilki kümesten {prey.Name} adlı {prey.Species.DisplayName.ToLowerInvariant()}ı kaptı.";
            return prey;
        }

        /// <summary>Yaban domuzu: yaz sonu ve sonbaharda ekin çiğner. Çiğnenen ekin sayısı.</summary>
        public int BoarVisit(Season season, out string message, bool force = false)
        {
            message = null;
            if (season != Season.Summer && season != Season.Autumn && !force) return 0;
            if (!force && _rng.NextDouble() >= BoarChance) return 0;
            var crops = new List<ItemInstance>();
            foreach (var item in _world.Items) if (item.IsPlant && item.Def.IsCrop) crops.Add(item);
            if (crops.Count == 0) return 0;
            if (WildWorld.HasPen(_world.Map) && ActiveDog()) { message = "Gece yaban domuzları geldi; çit ve köpek tarlaya sokmadı."; DogGuarded?.Invoke(); return 0; }
            int n = Math.Min(BoarCropsMax, crops.Count);
            for (int i = 0; i < n; i++)
            {
                int k = _rng.Next(crops.Count);
                _world.RemoveItem(crops[k].Id);
                crops.RemoveAt(k);
            }
            message = $"Gece yaban domuzları tarlaya girdi, {n} ekin çiğnendi.";
            return n;
        }

        /// <summary>Karga hediyesi: 1-3 akçe ya da midye kabuğu.</summary>
        public string CrowGift(Household house, bool force = false)
        {
            if (_adoptions == null || house == null) return null;
            if (!force && !_adoptions.CrowGiftToday()) return null;
            if (force && !_adoptions.Has("wild.crow")) return null;
            if (_rng.NextDouble() < 0.6) { int n = 1 + _rng.Next(3); house.AddAkce(n); return $"Karga pencereye {n} parlak akçe bıraktı."; }
            house.AddItem("item.mussel_shell", 1);
            return "Karga pencereye parlak bir kabuk bıraktı.";
        }
    }

    /// <summary>Kurt savunmasının dünyadan okunan parçaları: çitli ağıl ve açık havada yanan ateş (31-4).</summary>
    public static class WildWorld
    {
        /// <summary>Ağıl sayılmak için en az bu kadar çit ya da kapı kenarı.</summary>
        public const int PenFenceEdges = 8;

        private static readonly string[] FireItems = { "item.brazier", "item.torch", "item.bonfire" };

        public static int FenceEdges(GridMap map)
        {
            int n = 0;
            foreach (var kv in map.Edges) if (kv.Value == EdgeKind.Fence || kv.Value == EdgeKind.Gate) n++;
            return n;
        }

        public static bool HasPen(GridMap map) => map != null && FenceEdges(map) >= PenFenceEdges;

        public static bool HasOutdoorFire(WorldState world)
        {
            if (world == null) return false;
            foreach (var item in world.Items)
            {
                if (Array.IndexOf(FireItems, item.Def.Id) < 0) continue;
                var room = world.Map.Rooms.RoomAt(item.Position);
                if (room == null || !room.IsIndoor) return true;
            }
            return false;
        }
    }

    /// <summary>Haritada atla atlanabilecek engel: çit kenarı ya da alçak eşya (örme çit, kütük, yemlik, fıçı).</summary>
    public readonly struct JumpSpot
    {
        public readonly GridPos Landing;
        public readonly int Tiles;
        public readonly string Label;
        public JumpSpot(GridPos landing, int tiles, string label) { Landing = landing; Tiles = tiles; Label = label; }
    }

    public static class JumpObstacles
    {
        private static readonly string[] LowItems = { "item.wattle_hurdle", "item.log", "item.feed_trough", "item.water_trough", "item.barrel" };

        public static bool IsLow(ItemInstance item) => item != null && Array.IndexOf(LowItems, item.Def.Id) >= 0;

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>
        /// Binicinin yanındaki atlanabilir engeller. Çit kenarı: bir karo öteye iner (2 karo atlama).
        /// Alçak eşya: üstünden aşıp öteki yana iner (2 karo); iki alçak eşya üst üste ise 3 karo (Yüksek atlama).
        /// İniş karosu boş ve yürünebilir olmalı.
        /// </summary>
        public static List<JumpSpot> Find(WorldState world, GridPos from)
        {
            var list = new List<JumpSpot>();
            if (world == null) return list;
            var map = world.Map;
            foreach (var d in Dirs)
            {
                var n1 = from + d;
                if (!map.InBounds(n1)) continue;
                var edge = map.GetEdge(GridMap.EdgeBetween(from, n1));
                if (edge == EdgeKind.Fence || edge == EdgeKind.HalfWall || edge == EdgeKind.Railing)
                {
                    if (map.IsWalkable(n1)) list.Add(new JumpSpot(n1, 2, edge == EdgeKind.Fence ? "çit" : edge == EdgeKind.Railing ? "korkuluk" : "alçak duvar"));
                    continue;
                }
                if (edge != EdgeKind.None) continue;
                var low1 = world.GetItemAt(n1);
                if (!IsLow(low1)) continue;
                var n2 = n1 + d;
                if (!map.InBounds(n2)) continue;
                if (map.IsWalkable(n2) && world.GetItemAt(n2) == null) { list.Add(new JumpSpot(n2, 2, low1.Def.DisplayName.ToLowerInvariant())); continue; }
                var low2 = world.GetItemAt(n2);
                var n3 = n2 + d;
                if (IsLow(low2) && map.InBounds(n3) && map.IsWalkable(n3) && world.GetItemAt(n3) == null)
                    list.Add(new JumpSpot(n3, 3, "iki engel"));
            }
            return list;
        }
    }
}
