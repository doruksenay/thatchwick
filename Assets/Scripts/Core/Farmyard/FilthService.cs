using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>
    /// Yerdeki yığınlar ve koku kaynakları (belge 57 §2-3, 57b, 57d §5). Yığın dünyada bir halı katmanı eşyasıdır
    /// (yürümeyi engellemez); yaşı burada tutulur. Sinek, koku dalgası gibi görseller kayda girmez: oyun katmanı
    /// <see cref="Sources"/> listesinden türetir.
    /// </summary>
    public sealed class FilthService
    {
        public const string HumanPileId = "item.dung_pile", DogPileId = "item.dog_pile", AnimalPileId = "item.animal_dung", SheepPileId = "item.sheep_dung", RefusePileId = "item.refuse_pile";
        /// <summary>Evdeki döküntü de süprüntü gibi yaşlanır: bekledikçe sineklenir, sonra kokar.</summary>
        public const string DebrisId = "item.debris";
        public const string PrivyKind = "privy", PileSourceKind = "pile", DecorKind = "decor";

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Dictionary<int, Pile> _piles = new Dictionary<int, Pile>();
        private readonly List<SmellSource> _sources = new List<SmellSource>();
        private readonly Dictionary<int, GridPos> _lastTile = new Dictionary<int, GridPos>();
        private readonly Random _rng;
        private double _refresh;

        public YardConfig Config { get; }

        public sealed class Pile
        {
            public int ItemId;
            public PileKind Kind;
            /// <summary>Etkin yaş (oyun günü): soğukta yavaş, sıcakta hızlı ilerler.</summary>
            public double Age;
        }

        public FilthService(WorldState world, ContentDatabase content, YardConfig config = null, int seed = 571)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            Config = config ?? new YardConfig();
            _rng = new Random(seed);
        }

        /// <summary>Karo sıcaklığı (°C); null ise 15.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }
        /// <summary>Şu an yağıyor mu (açıktaki sinekler kaybolur).</summary>
        public Func<bool> Raining { get; set; }
        /// <summary>Gece mi (sinekler konar, uçmaz).</summary>
        public Func<bool> Night { get; set; }
        /// <summary>Doldurulan dekorun içi çürük mü.</summary>
        public Func<ItemInstance, bool> IsRotten { get; set; }
        /// <summary>Öbür sistemlerin koku kademesi (kompost, ahır): (kademe, tür) ya da kademe 0.</summary>
        public readonly List<Func<ItemInstance, (int tier, string kind)>> TierProviders = new List<Func<ItemInstance, (int, string)>>();

        /// <summary>Yığın toprağa karıştı ya da yağmurda eridi (tür, karo): toprak verimi buradan artar.</summary>
        public event Action<PileKind, GridPos> Decayed;
        /// <summary>Yeni yığın (tür, eşya).</summary>
        public event Action<PileKind, ItemInstance> Dropped;

        public IReadOnlyCollection<Pile> Piles => _piles.Values;
        public Pile PileOf(ItemInstance item) => item != null && _piles.TryGetValue(item.Id, out var p) ? p : null;
        public bool IsPile(ItemInstance item) => item != null && KindOf(item.Def.Id).HasValue;

        public static PileKind? KindOf(string defId)
        {
            switch (defId)
            {
                case HumanPileId: return PileKind.Human;
                case DogPileId: return PileKind.Dog;
                case AnimalPileId: case SheepPileId: return PileKind.Animal;
                case RefusePileId: case DebrisId: return PileKind.Refuse;
                default: return null;
            }
        }

        public static string DefOf(PileKind kind)
        {
            switch (kind)
            {
                case PileKind.Dog: return DogPileId;
                case PileKind.Animal: return AnimalPileId;
                case PileKind.Refuse: return RefusePileId;
                default: return HumanPileId;
            }
        }

        private float Temp(GridPos p) => TemperatureAt != null ? TemperatureAt(p) : 15f;
        private bool IsOutdoor(GridPos p) { var room = _world.Map.Rooms.RoomAt(p); return room == null || !room.IsIndoor; }

        /// <summary>Karoya (olmazsa bitişiğine) yığın bırakır; yer yoksa ya da tavan dolduysa null.</summary>
        public ItemInstance Drop(PileKind kind, GridPos tile, string defId = null)
        {
            if (!_content.Items.TryGet(defId != null && KindOf(defId) == kind ? defId : DefOf(kind), out var def)) return null;
            if (_piles.Count >= Config.MaxPiles) return null;
            GridPos? spot = null;
            if (CanHold(def, tile)) spot = tile;
            else
                for (int r = 1; r <= 2 && !spot.HasValue; r++)
                    for (int dy = -r; dy <= r && !spot.HasValue; dy++)
                        for (int dx = -r; dx <= r; dx++)
                        {
                            var p = new GridPos(tile.X + dx, tile.Y + dy, tile.Z);
                            if (CanHold(def, p)) { spot = p; break; }
                        }
            if (!spot.HasValue) return null;
            var item = _world.PlaceItem(def, spot.Value);
            _piles[item.Id] = new Pile { ItemId = item.Id, Kind = kind, Age = 0 };
            _refresh = 0;
            Dropped?.Invoke(kind, item);
            return item;
        }

        private bool CanHold(ItemDef def, GridPos p)
        {
            if (!_world.Map.InBounds(p) || !_world.Map.IsWalkable(p) || _world.Map.IsWater(p)) return false;
            foreach (var rug in _world.RugsAt(p)) if (KindOf(rug.Def.Id).HasValue) return false;   // aynı karoya ikinci yığın konmaz
            return _world.CanPlaceRug(def, p, 0).Ok;
        }

        /// <summary>Yığını kaldırır (toplandı); kaydı da silinir.</summary>
        public bool Remove(ItemInstance item)
        {
            if (item == null || !_piles.Remove(item.Id)) return false;
            if (_world.GetItem(item.Id) != null) _world.RemoveItem(item.Id);
            _refresh = 0;
            return true;
        }

        /// <summary>Toplanan yığının verdiği pislik birimi (4 birim = 1 gübre). Köpek pisliği ve süprüntü gübre vermez.</summary>
        public static int DungUnits(PileKind kind) => kind == PileKind.Human ? 1 : kind == PileKind.Animal ? 2 : 0;

        public PileStage StageOf(Pile p)
        {
            var c = Config;
            switch (p.Kind)
            {
                case PileKind.Dog: return p.Age >= c.DogFlies ? PileStage.Flies : PileStage.Fresh;
                case PileKind.Animal: return p.Age >= c.AnimalDry ? PileStage.Dried : p.Age >= c.AnimalFlies ? PileStage.Flies : PileStage.Fresh;
                case PileKind.Refuse: return p.Age >= c.RefuseReek ? PileStage.Reeking : p.Age >= c.RefuseFlies ? PileStage.Flies : PileStage.Fresh;
                default:
                    return p.Age >= c.HumanDry ? PileStage.Dried : p.Age >= c.HumanReek ? PileStage.Reeking : p.Age >= c.HumanFlies ? PileStage.Flies : PileStage.Fresh;
            }
        }

        public PileStage StageOf(ItemInstance item) { var p = PileOf(item); return p == null ? PileStage.Fresh : StageOf(p); }

        private double GoneAt(PileKind kind) =>
            kind == PileKind.Dog ? Config.DogGone : kind == PileKind.Animal ? Config.AnimalGone : kind == PileKind.Human ? Config.HumanGone : double.MaxValue;

        private static int TierOf(PileStage stage) => stage == PileStage.Reeking ? 2 : stage == PileStage.Flies ? 1 : 0;

        /// <summary>Zaman ilerler: yığınlar yaşlanır, süresi dolan toprağa karışır. Koku listesi seyrek yenilenir.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            if (_piles.Count > 0)
            {
                List<Pile> gone = null;
                foreach (var p in _piles.Values)
                {
                    var item = _world.GetItem(p.ItemId);
                    if (item == null) { (gone ?? (gone = new List<Pile>())).Add(p); continue; }   // yapı modunda kaldırılmış
                    p.Age += gameDays * Config.AgeFactor(Temp(item.Position));
                    if (p.Age >= GoneAt(p.Kind)) (gone ?? (gone = new List<Pile>())).Add(p);
                }
                if (gone != null) foreach (var p in gone) Dissolve(p);
            }
            _refresh -= gameDays;
            if (_refresh <= 0) { Refresh(); _refresh = RefreshDays; }
        }

        /// <summary>Koku listesinin yenilenme aralığı (≈7 oyun dakikası).</summary>
        public const double RefreshDays = 0.005;

        private void Dissolve(Pile p)
        {
            var item = _world.GetItem(p.ItemId);
            _piles.Remove(p.ItemId);
            if (item == null) return;
            var pos = item.Position;
            _world.RemoveItem(item.Id);
            if (p.Kind != PileKind.Refuse) Decayed?.Invoke(p.Kind, pos);
        }

        /// <summary>Yeni gün: yağmur açıktaki yığını bir evre ilerletir, fırtına siler (süprüntü hariç).</summary>
        public void OnNewDay(bool rained, bool stormed)
        {
            if (!rained && !stormed) return;
            foreach (var p in new List<Pile>(_piles.Values))
            {
                var item = _world.GetItem(p.ItemId);
                if (item == null || p.Kind == PileKind.Refuse || !IsOutdoor(item.Position)) continue;
                if (stormed) { Dissolve(p); continue; }
                p.Age = NextThreshold(p);
                if (p.Age >= GoneAt(p.Kind)) Dissolve(p);
            }
            _refresh = 0;
        }

        private double NextThreshold(Pile p)
        {
            var c = Config;
            double[] steps = p.Kind == PileKind.Dog ? new[] { c.DogFlies, c.DogGone }
                : p.Kind == PileKind.Animal ? new[] { c.AnimalFlies, c.AnimalDry, c.AnimalGone }
                : new[] { c.HumanFlies, c.HumanReek, c.HumanDry, c.HumanGone };
            foreach (var s in steps) if (p.Age < s) return s;
            return steps[steps.Length - 1];
        }

        // ---------- koku kaynakları ----------

        /// <summary>Kademesi 1 ve üstü bütün kaynaklar (yığın, hela, çürük dekor, kompost, ahır).</summary>
        public IReadOnlyList<SmellSource> Sources => _sources;

        /// <summary>Listeyi hemen yeniler (testler ve yeni gün).</summary>
        public void Refresh()
        {
            _sources.Clear();
            foreach (var item in _world.Items)
            {
                if (item.IsDecor && IsRotten == null) continue;
                int tier = 0; string kind = null;
                if (!_piles.TryGetValue(item.Id, out var pile) && !item.IsDecor)
                {
                    var known = KindOf(item.Def.Id);   // başka sistemin bıraktığı döküntü: yaşı buradan başlar
                    if (known.HasValue) _piles[item.Id] = pile = new Pile { ItemId = item.Id, Kind = known.Value, Age = 0 };
                }
                if (pile != null) { tier = TierOf(StageOf(pile)); kind = PileSourceKind; }
                else if (item.Def.FillCapacity > 0 && item.Def.FillProduct != null)
                {
                    // Hela dörtte üçte sineklenir, dolunca kokar; küçük lazımlık dolunca sineklenir.
                    bool small = item.Def.FillCapacity <= 6;
                    if (item.Fill >= item.Def.FillCapacity) tier = small ? 1 : 2;
                    else if (!small && item.Fill * 4 >= item.Def.FillCapacity * 3) tier = 1;
                    kind = PrivyKind;
                }
                else if (IsRotten != null && item.HasContents && IsRotten(item)) { tier = 1; kind = DecorKind; }
                else
                    foreach (var provider in TierProviders)
                    {
                        var (t, k) = provider(item);
                        if (t > tier) { tier = t; kind = k; }
                    }
                if (tier <= 0) continue;
                _sources.Add(new SmellSource(item.Id, item.Position, tier, IsOutdoor(item.Position), kind));
            }
        }

        /// <summary>Bu kaynakta şu an sinek var mı: ılık hava; açıktaysa yağmur yok.</summary>
        public bool HasFlies(SmellSource s)
        {
            if (s.Tier <= 0 || Temp(s.Position) < Config.FlyMinTemp) return false;
            return !(s.Outdoor && Raining != null && Raining());
        }

        /// <summary>Kaynaktaki sinek sayısı (görünüm): kademe 1'de 2-3, kademe 2'de 5-6; gece konarlar.</summary>
        public int FlyCount(SmellSource s) => !HasFlies(s) ? 0 : s.Tier >= 2 ? 5 + (s.ItemId & 1) : 2 + (s.ItemId & 1);
        public bool FliesLanded => Night != null && Night();

        private static int Distance(GridPos a, GridPos b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        /// <summary>Karoya en yakın, verilen kademe ve üstündeki kaynak (yarıçap içinde); yoksa null.</summary>
        public SmellSource? Nearest(GridPos p, int radius, int minTier = 1, bool fliesOnly = false)
        {
            SmellSource? best = null; int bestD = int.MaxValue;
            foreach (var s in _sources)
            {
                if (s.Tier < minTier || s.Position.Z != p.Z) continue;
                int d = Distance(s.Position, p);
                if (d > radius || d >= bestD) continue;
                if (fliesOnly && !HasFlies(s)) continue;
                best = s; bestD = d;
            }
            return best;
        }

        /// <summary>Odanın koku yüzünden yitirdiği güzellik (belge 57 §2.2): odadaki ya da duvarın hemen dışındaki kaynaklar.</summary>
        public int BeautyPenalty(Room room)
        {
            if (room == null || room.Tiles.Count == 0) return 0;
            int worst = 0;
            foreach (var s in _sources)
            {
                if (s.Position.Z != room.Level) continue;
                int radius = s.Tier >= 2 ? Config.BeautyRadiusTier2 : Config.BeautyRadiusTier1;
                foreach (var t in room.Tiles)
                    if (Distance(t, s.Position) <= radius)
                    {
                        worst = Math.Max(worst, s.Tier >= 2 ? Config.BeautyPenaltyTier2 : Config.BeautyPenaltyTier1);
                        break;
                    }
            }
            return worst;
        }

        /// <summary>Evin içinde sinekli bir kaynak var mı (açıkta kalan yiyecek çabuk bozulur).</summary>
        public bool FliesIndoors()
        {
            foreach (var s in _sources) if (!s.Outdoor && HasFlies(s)) return true;
            return false;
        }

        /// <summary>Kademe 2 kaynak sayısı (avlu temizliği, itibar).</summary>
        public int ReekingCount { get { int n = 0; foreach (var s in _sources) if (s.Tier >= 2) n++; return n; } }

        // ---------- basma ----------

        /// <summary>
        /// Karakter yeni bir karoya girdiyse ve orada taze ya da sinekli insan ya da köpek pisliği varsa basabilir.
        /// Basarsa yığın döner. <paramref name="dark"/>: gece ya da koşarken şans yüksek.
        /// </summary>
        public ItemInstance StepCheck(int characterId, GridPos position, bool dark)
        {
            bool moved = !_lastTile.TryGetValue(characterId, out var last) || !last.Equals(position);
            _lastTile[characterId] = position;
            if (!moved || _piles.Count == 0) return null;
            foreach (var rug in _world.RugsAt(position))
            {
                if (!_piles.TryGetValue(rug.Id, out var pile)) continue;
                if (pile.Kind != PileKind.Human && pile.Kind != PileKind.Dog) continue;
                if (StageOf(pile) == PileStage.Dried) continue;
                if (_rng.NextDouble() < (dark ? Config.StepChanceDark : Config.StepChance)) return rug;
            }
            return null;
        }

        // ---------- kayıt ----------

        public YardPileSave[] Export()
        {
            var list = new List<YardPileSave>();
            foreach (var p in _piles.Values) list.Add(new YardPileSave { itemId = p.ItemId, kind = (int)p.Kind, age = p.Age });
            list.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            return list.ToArray();
        }

        public void Import(YardPileSave[] data)
        {
            _piles.Clear();
            if (data != null)
                foreach (var s in data)
                {
                    if (s == null) continue;
                    var item = _world.GetItem(s.itemId);
                    if (item == null || !KindOf(item.Def.Id).HasValue) continue;
                    _piles[s.itemId] = new Pile { ItemId = s.itemId, Kind = Enum.IsDefined(typeof(PileKind), s.kind) ? (PileKind)s.kind : PileKind.Human, Age = Math.Max(0, s.age) };
                }
            // Kayıtta yaşı olmayan yığın eşyaları (eski ya da elle konmuş) taze sayılır.
            foreach (var item in _world.Items)
            {
                var kind = KindOf(item.Def.Id);
                if (kind.HasValue && !_piles.ContainsKey(item.Id)) _piles[item.Id] = new Pile { ItemId = item.Id, Kind = kind.Value, Age = 0 };
            }
            Refresh();
        }
    }
}
