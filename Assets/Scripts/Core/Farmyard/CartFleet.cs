using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>Arabanın görevi (belge 57e §2.2). Bir araba aynı anda tek görevdedir.</summary>
    public enum CartJob { Idle = 0, Field = 1, Market = 2, Mine = 3, Forest = 4, Coast = 5, South = 6 }

    /// <summary>Bir araba: türü, çeki hayvanı, arabacısı, görevi ve tekerlek yıpranması.</summary>
    public sealed class Cart
    {
        /// <summary>Kalıcı anahtar: yerleştirilmiş araba "item:&lt;kimlik&gt;", envanterdeki "&lt;eşya&gt;#&lt;sıra&gt;".</summary>
        public string Key;
        public string DefId;
        public int ItemId;
        public CartJob Job;
        public int AnimalId, DriverId;
        /// <summary>Tekerlek yıpranması 0-1; 1'de araba yola çıkmaz.</summary>
        public double Wear;
        public bool Broken => Wear >= 1.0;
    }

    /// <summary>
    /// Araba filosu (belge 57e): her araba bir çeki hayvanı, bir arabacı ve bir görev ister. Darboğaz hayvandır:
    /// bir hayvan günde tek arabada çalışır ve üst üste üç gün çalışınca bir gün dinlenir.
    /// </summary>
    public sealed class CartFleet
    {
        public const string WheelbarrowId = "item.wheelbarrow", HorseCartId = "item.horse_cart", OxCartId = "item.ox_cart",
            CoveredCartId = "item.covered_cart", SledId = "item.sled", WheelId = "item.cart_wheel";
        public static readonly string[] CartIds = { WheelbarrowId, HorseCartId, OxCartId, CoveredCartId, SledId };
        /// <summary>Arabacılık bu seviyedeyse alışmış hayvan bilinen yolu arabacısız gider.</summary>
        public const int DriverlessLevel = 7;
        /// <summary>Araba sundurması: altındaki araba yağmurda yıpranmaz.</summary>
        public const string ShedTag = "cart_shed";

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly AnimalSystem _animals;
        private readonly List<Cart> _carts = new List<Cart>();
        private readonly Dictionary<string, Cart> _known = new Dictionary<string, Cart>(StringComparer.Ordinal);
        private readonly Dictionary<int, (long day, int streak)> _work = new Dictionary<int, (long, int)>();
        private readonly List<Cart> _today = new List<Cart>();
        private long _todayDay = long.MinValue;
        private bool _migrated;

        public YardConfig Config { get; }
        /// <summary>Kural: verimi eşiğin altına düşen karoya tarla arabası kendiliğinden gübre serer.</summary>
        public bool FertilizeRule { get; set; }
        public float FertilizeBelow { get; set; }
        /// <summary>Kâhya boşta duran arabalara o günün işini dağıtsın (Kâhyalık 4 ve görevde bir kâhya ister).</summary>
        public bool StewardCarts { get; set; }

        public CartFleet(WorldState world, Household house, AnimalSystem animals, YardConfig config = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _animals = animals ?? throw new ArgumentNullException(nameof(animals));
            Config = config ?? new YardConfig();
            FertilizeBelow = Config.FertilizeBelow;
        }

        public Func<int, Character> FindCharacter { get; set; }
        /// <summary>Hanenin en iyi Arabacılık seviyesi.</summary>
        public Func<int> CartingLevel { get; set; }
        /// <summary>Yerde kar var mı (kızak yalnızca karda gider).</summary>
        public Func<bool> Snow { get; set; }
        /// <summary>Hayvanın görünen adı (tür adı ve adı); null ise hayvanın adı.</summary>
        public Func<Animal, string> AnimalLabel { get; set; }
        /// <summary>Görev bugün yapılır mı (pazar arabası yalnızca sefer günü yola çıkar); null ise her gün.</summary>
        public Func<CartJob, long, bool> JobActive { get; set; }

        // ---------- arabalar ----------

        /// <summary>Hanenin bütün arabaları (envanterdekiler ve avluya konmuş olanlar).</summary>
        public IReadOnlyList<Cart> Carts { get { Sync(); return _carts; } }

        private void Sync()
        {
            _carts.Clear();
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || Array.IndexOf(CartIds, it.Def.Id) < 0) continue;
                // Eski kayıt: avludaki öküz arabası haftalık pazar seferini sürdürür.
                _carts.Add(Known("item:" + it.Id, it.Def.Id, it.Id, it.Def.Id == OxCartId && !_migrated ? CartJob.Market : CartJob.Idle));
            }
            foreach (var id in CartIds)
            {
                int n = _house.Count(id);
                for (int i = 0; i < n; i++) _carts.Add(Known(id + "#" + i, id, 0, CartJob.Idle));
            }
            _migrated = true;
        }

        private Cart Known(string key, string defId, int itemId, CartJob initial)
        {
            if (!_known.TryGetValue(key, out var c)) _known[key] = c = new Cart { Key = key, DefId = defId, ItemId = itemId, Job = initial };
            return c;
        }

        /// <summary>
        /// Envanterdeki arabaları avluya koyar (belge 57e §2.5, kayıt göçü): araba artık yerleştirilen eşyadır. Görevi,
        /// hayvanı, arabacısı ve tekerlek durumu yeni eşyaya taşınır. Yer bulunamayan araba envanterde bekler. Konan adet.
        /// </summary>
        public int ParkInventory(ContentDatabase content, Func<ItemDef, GridPos?> findSpot)
        {
            int placed = 0;
            if (content == null || findSpot == null) return 0;
            foreach (var id in CartIds)
            {
                if (!content.Items.TryGet(id, out var def) || def.Placement == ItemPlacement.Resource) continue;
                for (int i = _house.Count(id) - 1; i >= 0; i--)
                {
                    var spot = findSpot(def);
                    if (!spot.HasValue || !_house.TryTakeItem(id)) break;
                    var item = _world.PlaceItem(def, spot.Value);
                    var cart = new Cart { Key = "item:" + item.Id, DefId = id, ItemId = item.Id };
                    if (_known.TryGetValue(id + "#" + i, out var old))
                    {
                        cart.Job = old.Job; cart.AnimalId = old.AnimalId; cart.DriverId = old.DriverId; cart.Wear = old.Wear;
                        _known.Remove(id + "#" + i);
                    }
                    _known[cart.Key] = cart;
                    placed++;
                }
            }
            return placed;
        }

        /// <summary>Araba bir sundurmanın altında mı (envanterdeki araba kapalı yerde sayılır).</summary>
        public bool Sheltered(Cart c)
        {
            if (c == null || c.ItemId == 0) return true;
            var item = _world.GetItem(c.ItemId);
            if (item == null) return true;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.HasTag(ShedTag) || it.Position.Z != item.Position.Z) continue;
                int dx = item.Position.X - it.Position.X, dy = item.Position.Y - it.Position.Y;
                if (dx >= -Config.ShedRadius && dx <= it.Def.Width - 1 + Config.ShedRadius && dy >= -Config.ShedRadius && dy <= it.Def.Height - 1 + Config.ShedRadius) return true;
            }
            return false;
        }

        /// <summary>Yağmurlu gün: sundurmasız duran arabaların tekerleği yıpranır. Islanan araba sayısı.</summary>
        public int RainOnCarts()
        {
            int n = 0;
            foreach (var c in Carts)
            {
                if (Sheltered(c)) continue;
                c.Wear = Math.Min(1.0, c.Wear + Config.RainWear);
                n++;
            }
            return n;
        }

        /// <summary>
        /// Arabanın gün içindeki seferi (görünüm için): 0 avluda, 1 çıkıyor, 2 yolda (görünmez), 3 dönüyor; t evre içi 0-1.
        /// Tarla arabası çiftlikten çıkmaz.
        /// </summary>
        public (int phase, float t) TripPhase(Cart cart, double dayFraction)
        {
            if (cart == null) return (0, 0f);
            Cart worked = null;
            foreach (var c in _today) if (c.Key == cart.Key) { worked = c; break; }
            if (worked == null || worked.Job == CartJob.Idle || worked.Job == CartJob.Field) return (0, 0f);
            var k = Config;
            if (dayFraction < k.TripLeave || dayFraction >= k.TripHome) return (0, 0f);
            if (dayFraction < k.TripGone) return (1, (float)((dayFraction - k.TripLeave) / (k.TripGone - k.TripLeave)));
            if (dayFraction < k.TripBack) return (2, 0f);
            return (3, (float)((dayFraction - k.TripBack) / (k.TripHome - k.TripBack)));
        }

        public Cart Find(string key) { foreach (var c in Carts) if (c.Key == key) return c; return null; }

        /// <summary>Yük (gübre karosu).</summary>
        public int Capacity(Cart c)
        {
            switch (c?.DefId)
            {
                case WheelbarrowId: return Config.WheelbarrowTiles;
                case HorseCartId: return Config.HorseCartTiles;
                case OxCartId: return Config.OxCartTiles;
                case CoveredCartId: return Config.CoveredCartTiles;
                case SledId: return Config.SledTiles;
                default: return 0;
            }
        }

        public static bool NeedsAnimal(Cart c) => c != null && c.DefId != WheelbarrowId;

        /// <summary>Bu arabayı çekebilen tür mü: el arabası hayvansız; öküz arabası öküz ve inek; kızak at ve öküz; ötekiler at, katır, bardo, eşek.</summary>
        public static bool CanPull(Cart c, Animal a)
        {
            if (c == null || a == null || !a.Species.Draft) return false;
            bool bovine = a.Species.Id == "animal.ox" || a.Species.Id == "animal.cow";
            switch (c.DefId)
            {
                case OxCartId: return true;        // ağır araba: öküz en iyisi, at da çeker
                case SledId: return true;
                case HorseCartId: case CoveredCartId: return !bovine;
                default: return false;
            }
        }

        /// <summary>El arabası yalnızca çiftlik içinde iş görür.</summary>
        public static bool JobAllowed(Cart c, CartJob job) => c != null && (job == CartJob.Idle || job == CartJob.Field || c.DefId != WheelbarrowId);

        private string Label(Animal a) => AnimalLabel != null ? AnimalLabel(a) : a.Name ?? a.Species.DisplayName;

        private bool PullsAnother(Cart cart, int animalId)
        {
            foreach (var o in Carts) if (o != cart && o.Job != CartJob.Idle && o.AnimalId == animalId) return true;
            return false;
        }

        /// <summary>Hayvan dün dahil üst üste çok çalıştı mı (bugün dinlenmeli).</summary>
        public bool IsTired(int animalId, long day) =>
            _work.TryGetValue(animalId, out var w) && w.day == day - 1 && w.streak >= Config.MaxWorkStreak;

        public int WorkStreak(int animalId, long day) => _work.TryGetValue(animalId, out var w) && w.day >= day - 1 ? w.streak : 0;

        /// <summary>
        /// Bu hayvan bu arabaya bugün koşulabilir mi; koşulamıyorsa oyuncuya gösterilecek uyarı ("Öküz Karabaş bugün
        /// çok yoruldu, yapamaz"). Koşulabiliyorsa boş <see cref="LocText"/>.
        /// </summary>
        public LocText AnimalBlocker(Cart cart, Animal a, long day)
        {
            if (a == null || a.IsDead) return LocText.Of("yard.cart.b.no_animal");
            var who = ("animal", (object)Label(a));
            if (!CanPull(cart, a)) return LocText.Of("yard.cart.b.wrong_animal", who);
            if (!a.IsMature) return LocText.Of("yard.cart.b.young", who);
            if (a.IsSick) return LocText.Of("yard.cart.b.sick", who);
            if (a.IsPregnant) return LocText.Of("yard.cart.b.pregnant", who);
            if (a.Fullness < AnimalSystem.ProduceFullness) return LocText.Of("yard.cart.b.hungry", who);
            if (a.RiderId != 0) return LocText.Of("yard.cart.b.ridden", who);
            if (IsTired(a.Id, day)) return LocText.Of("yard.cart.b.tired", who);
            if (PullsAnother(cart, a.Id)) return LocText.Of("yard.cart.b.busy", who);
            return default;
        }

        public LocText DriverBlocker(Cart cart, Character d)
        {
            if (d == null || d.IsDead) return LocText.Of("yard.cart.b.no_driver");
            if (d.Stage == AgeStage.Baby || d.Stage == AgeStage.Child) return LocText.Of("yard.cart.b.driver_young", ("name", d.Name));
            foreach (var o in Carts)
                if (o != cart && o.Job != CartJob.Idle && o.DriverId == d.Id) return LocText.Of("yard.cart.b.driver_busy", ("name", d.Name));
            return default;
        }

        /// <summary>Arabaya görev, hayvan ve arabacı atar; olmuyorsa uyarıyı döner ve hiçbir şeyi değiştirmez.</summary>
        public LocText Assign(Cart cart, CartJob job, int animalId, int driverId, long day)
        {
            if (cart == null) return LocText.Of("yard.cart.b.no_cart");
            if (job == CartJob.Idle) { cart.Job = CartJob.Idle; cart.AnimalId = 0; cart.DriverId = 0; return default; }
            if (!JobAllowed(cart, job)) return LocText.Of("yard.cart.b.barrow_far");
            if (NeedsAnimal(cart))
            {
                var b = AnimalBlocker(cart, _animals.Get(animalId), day);
                if (!b.IsEmpty) return b;
            }
            else animalId = 0;
            bool driverless = NeedsAnimal(cart) && driverId == 0 && (CartingLevel?.Invoke() ?? 1) >= DriverlessLevel;
            if (!driverless)
            {
                var b = DriverBlocker(cart, FindCharacter?.Invoke(driverId));
                if (!b.IsEmpty) return b;
            }
            cart.Job = job; cart.AnimalId = animalId; cart.DriverId = driverId;
            return default;
        }

        /// <summary>Koşulabilecek ilk uygun hayvan (kendiliğinden seçim); yoksa null.</summary>
        public Animal PickAnimal(Cart cart, long day)
        {
            Animal best = null;
            foreach (var a in _animals.Animals)
            {
                if (!AnimalBlocker(cart, a, day).IsEmpty || WorksToday(a.Id)) continue;
                // Öküz arabasına önce öküz; ötekilere tür sırası fark etmez.
                if (best == null || (cart.DefId == OxCartId && a.Species.Id == "animal.ox" && best.Species.Id != "animal.ox")) best = a;
            }
            return best;
        }

        private bool WorksToday(int animalId) { foreach (var c in _today) if (c.AnimalId == animalId) return true; return false; }

        /// <summary>Araba bugün yola çıkabilir mi; çıkamıyorsa neden.</summary>
        public LocText Blocker(Cart cart, long day)
        {
            if (cart == null) return LocText.Of("yard.cart.b.no_cart");
            if (cart.Broken) return LocText.Of("yard.cart.b.wheel");
            if (cart.DefId == SledId && Snow != null && !Snow()) return LocText.Of("yard.cart.b.snow");
            if (NeedsAnimal(cart))
            {
                var a = cart.AnimalId != 0 ? _animals.Get(cart.AnimalId) : PickAnimal(cart, day);
                if (a == null) return LocText.Of("yard.cart.b.no_animal");
                var b = AnimalBlocker(cart, a, day);
                if (!b.IsEmpty) return b;
            }
            if (cart.DriverId != 0)
            {
                var b = DriverBlocker(cart, FindCharacter?.Invoke(cart.DriverId));
                if (!b.IsEmpty) return b;
            }
            else if (!NeedsAnimal(cart)) return LocText.Of("yard.cart.b.no_driver");
            return default;
        }

        // ---------- gün ----------

        /// <summary>
        /// Yeni gün: görevi olan ve yola çıkabilen arabalar çalışır. Hayvanı seçilmemiş arabaya uygun ilk hayvan koşulur.
        /// Çalışan arabaların listesini döner; çıkamayanlar <paramref name="stuck"/> ile bildirilir.
        /// </summary>
        public IReadOnlyList<Cart> OnNewDay(long day, List<(Cart cart, LocText why)> stuck = null)
        {
            _today.Clear();
            _todayDay = day;
            foreach (var cart in new List<Cart>(Carts))
            {
                if (cart.Job == CartJob.Idle || (JobActive != null && !JobActive(cart.Job, day))) continue;
                var why = Blocker(cart, day);
                if (!why.IsEmpty) { stuck?.Add((cart, why)); continue; }
                int animalId = 0;
                if (NeedsAnimal(cart))
                {
                    var a = cart.AnimalId != 0 ? _animals.Get(cart.AnimalId) : PickAnimal(cart, day);
                    if (a == null || WorksToday(a.Id)) { stuck?.Add((cart, LocText.Of("yard.cart.b.no_animal"))); continue; }
                    animalId = a.Id;
                    int streak = _work.TryGetValue(a.Id, out var w) && w.day == day - 1 ? w.streak + 1 : 1;
                    _work[a.Id] = (day, streak);
                }
                cart.Wear = Math.Min(1.0, cart.Wear + Config.WheelWearPerTrip);
                _today.Add(new Cart { Key = cart.Key, DefId = cart.DefId, ItemId = cart.ItemId, Job = cart.Job, AnimalId = animalId, DriverId = cart.DriverId, Wear = cart.Wear });
            }
            return _today;
        }

        /// <summary>Bugün çalışan arabalar.</summary>
        public IReadOnlyList<Cart> Today => _today;

        public int WorkingCount(CartJob job) { int n = 0; foreach (var c in _today) if (c.Job == job) n++; return n; }
        public int WorkingCapacity(CartJob job) { int n = 0; foreach (var c in _today) if (c.Job == job) n += Capacity(c); return n; }

        /// <summary>Madenden ve ormandan gelen yükte ek ürün şansı: araba başına %15, en çok %30.</summary>
        public double HaulBonus(CartJob job) => Math.Min(Config.HaulBonusMax, Config.HaulBonusPerCart * WorkingCount(job));

        /// <summary>Bugün arabaya koşulmamış, düven ve saban için boşta kalan çeki hayvanı sayısı.</summary>
        public int FreeDraftAnimals()
        {
            int n = 0;
            foreach (var a in _animals.Animals)
                if (!a.IsDead && a.Species.Draft && a.IsMature && !a.IsSick && !a.IsPregnant && a.Fullness >= AnimalSystem.ProduceFullness && a.RiderId == 0 && !WorksToday(a.Id)) n++;
            return n;
        }

        /// <summary>Pazar seferi için engel (OxCart): pazara atanmış ve yola çıkabilen araba yoksa neden.</summary>
        public LocText MarketBlocker(long day)
        {
            if (_todayDay == day) foreach (var c in _today) if (c.Job == CartJob.Market) return default;   // bugün yola çıktı
            LocText first = LocText.Of("yard.cart.b.no_market");
            bool any = false;
            foreach (var c in Carts)
            {
                if (c.Job != CartJob.Market) continue;
                var b = Blocker(c, day);
                if (b.IsEmpty) return default;
                if (!any) { first = b; any = true; }
            }
            return first;
        }

        // ---------- tekerlek ----------

        /// <summary>Hanenin en iyi Marangozluk seviyesi (tekerlek takmak ister); null ise kısıt yok.</summary>
        public Func<int> CarpentryLevel { get; set; }

        /// <summary>Tekerlek takılamıyorsa neden; takılabiliyorsa boş.</summary>
        public LocText RepairBlocker(Cart c)
        {
            if (c == null || c.Wear < 0.5) return LocText.Of("yard.cart.b.wheel_fine");
            if (_house.Count(WheelId) <= 0) return LocText.Of("yard.cart.b.no_wheel");
            if (CarpentryLevel != null && CarpentryLevel() < Config.RepairCarpentry) return LocText.Of("yard.cart.b.carpentry", ("lvl", Config.RepairCarpentry));
            return default;
        }

        public bool CanRepair(Cart c) => RepairBlocker(c).IsEmpty;

        public bool Repair(Cart c)
        {
            if (!CanRepair(c) || !_house.TryTakeItem(WheelId)) return false;
            c.Wear = 0;
            return true;
        }

        // ---------- kayıt ----------

        public YardFleetSave Export()
        {
            Sync();
            var carts = new List<YardCartSave>();
            foreach (var c in _carts) carts.Add(new YardCartSave { key = c.Key, assignment = (int)c.Job, animalId = c.AnimalId, driverId = c.DriverId, wear = c.Wear });
            var work = new List<YardAnimalWorkSave>();
            foreach (var kv in _work) work.Add(new YardAnimalWorkSave { animalId = kv.Key, lastDay = kv.Value.day, streak = kv.Value.streak });
            work.Sort((a, b) => a.animalId.CompareTo(b.animalId));
            return new YardFleetSave { carts = carts.ToArray(), work = work.ToArray(), fertilizeRule = FertilizeRule ? 1 : 0, fertilizeBelow = FertilizeBelow, stewardCarts = StewardCarts ? 1 : 0 };
        }

        public void Import(YardFleetSave data)
        {
            _known.Clear(); _work.Clear(); _today.Clear(); _carts.Clear();
            _migrated = data != null;   // eski kayıtta filo yok: avludaki öküz arabası pazar görevine başlar
            FertilizeRule = false; StewardCarts = false; FertilizeBelow = Config.FertilizeBelow;
            if (data == null) return;
            FertilizeRule = data.fertilizeRule != 0;
            StewardCarts = data.stewardCarts != 0;
            if (data.fertilizeBelow > 0f) FertilizeBelow = data.fertilizeBelow;
            if (data.carts != null)
                foreach (var s in data.carts)
                {
                    if (s == null || string.IsNullOrEmpty(s.key)) continue;
                    string defId; int itemId = 0;
                    if (s.key.StartsWith("item:", StringComparison.Ordinal))
                    {
                        if (!int.TryParse(s.key.Substring(5), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out itemId)) continue;
                        var it = _world.GetItem(itemId);
                        if (it == null) continue;
                        defId = it.Def.Id;
                    }
                    else
                    {
                        int hash = s.key.LastIndexOf('#');
                        if (hash <= 0) continue;
                        defId = s.key.Substring(0, hash);
                    }
                    _known[s.key] = new Cart
                    {
                        Key = s.key, DefId = defId, ItemId = itemId, AnimalId = s.animalId, DriverId = s.driverId, Wear = Math.Max(0, Math.Min(1, s.wear)),
                        Job = Enum.IsDefined(typeof(CartJob), s.assignment) ? (CartJob)s.assignment : CartJob.Idle
                    };
                }
            if (data.work != null)
                foreach (var w in data.work) if (w != null) _work[w.animalId] = (w.lastDay, w.streak);
        }
    }
}
