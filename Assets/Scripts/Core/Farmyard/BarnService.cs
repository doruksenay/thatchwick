using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>
    /// Ahır kirliliği ve ahır düzenekleri (belge 57 §4, 57d §4): barınak her gün kirlenir, temizlenince gübre çıkar.
    /// Derin altlık, akıtma oluğu, gübre kapağı ve ağıla alma buradadır; merada kalan tezek, köpek pisliği ve
    /// kedinin gömmesi de buradan tetiklenir.
    /// </summary>
    public sealed class BarnService
    {
        public const string HatchTag = "manure_hatch", DrainTag = "barn_drain", FoldTag = "fold_hurdle";
        public const string StrawId = "item.straw";

        private readonly WorldState _world;
        private readonly AnimalSystem _animals;
        private readonly Household _house;
        private readonly FilthService _filth;
        private readonly CompostService _compost;
        private readonly Dictionary<int, float> _dirt = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _bank = new Dictionary<int, float>();   // temizlikte çıkacak pislik birimi
        private readonly HashSet<int> _deepLitter = new HashSet<int>();
        private readonly List<(double at, int animalId, int kind)> _due = new List<(double, int, int)>();   // günün olayları (kayda girmez)
        private readonly Random _rng;
        private double _dayClock;

        public YardConfig Config => _filth.Config;

        public BarnService(WorldState world, AnimalSystem animals, Household house, FilthService filth, CompostService compost, int seed = 572)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _animals = animals ?? throw new ArgumentNullException(nameof(animals));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _filth = filth ?? throw new ArgumentNullException(nameof(filth));
            _compost = compost ?? throw new ArgumentNullException(nameof(compost));
            _rng = new Random(seed);
        }

        /// <summary>Toprak (ağıla alma için); null ise ağıl işlemez.</summary>
        public PlantSystem Plants { get; set; }
        /// <summary>Hayvanlar bugün dışarı çıkıyor mu (kapı açık, mevsim uygun).</summary>
        public Func<bool> Grazing { get; set; }

        /// <summary>Kedi pisliğini gömdü (yalnızca görsel).</summary>
        public event Action<Animal> CatBuried;
        /// <summary>Köpek yığın bıraktı (poz ve görünüm için).</summary>
        public event Action<Animal> DogDropped;
        /// <summary>Derin altlığa saman yetmedi (barınak).</summary>
        public event Action<ItemInstance> LitterShort;

        // ---------- kirlilik ----------

        /// <summary>Türün günlük kirliliği: büyükbaş ve at 12, orta boy 8, kümes 3; evcil ve arı 0.</summary>
        public float DirtOf(AnimalSpeciesDef s)
        {
            if (s == null || s.IsPet) return 0f;
            switch (s.Housing)
            {
                case "stable": return Config.DirtLarge;
                case "barn": return s.Draft ? Config.DirtLarge : Config.DirtMedium;
                case "sty": return Config.DirtMedium;
                case "coop": case "warren": case "dovecote": return Config.DirtSmall;
                default: return 0f;
            }
        }

        public bool IsBarn(ItemInstance home)
        {
            if (home == null || home.IsDecor || !home.Def.IsHousing) return false;
            switch (home.Def.HousingKind) { case "stable": case "barn": case "sty": case "coop": case "warren": case "dovecote": return true; default: return false; }
        }

        public float DirtAt(ItemInstance home) => home != null && _dirt.TryGetValue(home.Id, out var d) ? d : 0f;
        public bool IsDirty(ItemInstance home) => DirtAt(home) >= Config.DirtyAt;
        public bool IsFilthy(ItemInstance home) => DirtAt(home) >= Config.FilthyAt;
        public bool HasDeepLitter(ItemInstance home) => home != null && _deepLitter.Contains(home.Id);

        public void SetDeepLitter(ItemInstance home, bool on)
        {
            if (home == null) return;
            if (on) _deepLitter.Add(home.Id); else _deepLitter.Remove(home.Id);
        }

        private bool Near(ItemInstance home, string tag)
        {
            foreach (var it in _animals.ItemsNear(home)) if (!it.IsDecor && it.Def.HasTag(tag)) return true;
            // Açıktaki barınak: iki karo içindeki eşya da sayılır.
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.HasTag(tag) || it.Position.Z != home.Position.Z) continue;
                if (Math.Abs(it.Position.X - home.Position.X) <= 3 && Math.Abs(it.Position.Y - home.Position.Y) <= 3) return true;
            }
            return false;
        }

        public bool HasDrain(ItemInstance home) => Near(home, DrainTag);
        /// <summary>Gübre kapağı işler mi: kapak yakında ve haritada gübre yığını var.</summary>
        public bool HasHatch(ItemInstance home) => _compost.AnyDungHeap && Near(home, HatchTag);

        /// <summary>Temizlik süresi çarpanı (gübre kapağı işi yarılar).</summary>
        public double CleanDurationFactor(ItemInstance home) => HasHatch(home) ? 1.0 / Config.HatchSpeed : 1.0;

        /// <summary>Konfor kesintisi (Z5).</summary>
        public float ComfortPenalty(ItemInstance home)
        {
            float d = DirtAt(home);
            return d >= Config.FilthyAt ? Config.FilthyComfort : d >= Config.DirtyAt ? Config.DirtyComfort : 0f;
        }

        /// <summary>Günlük hastalık riskine ek (Z4).</summary>
        public double SickRisk(ItemInstance home)
        {
            float d = DirtAt(home);
            return d >= Config.FilthyAt ? Config.FilthySick : d >= Config.DirtyAt ? Config.DirtySick : 0.0;
        }

        public (int tier, string kind) SmellTier(ItemInstance item)
        {
            if (!IsBarn(item)) return (0, null);
            float d = DirtAt(item);
            return (d >= Config.FilthyAt ? 2 : d >= Config.DirtyAt ? 1 : 0, "barn");
        }

        /// <summary>Temizlenmeyi bekliyor mu (görevli için): kirli ya da birikmiş gübresi var.</summary>
        public bool NeedsCleaning(ItemInstance home) => IsBarn(home) && (HasDeepLitter(home) ? DirtAt(home) >= Config.DirtyAt : DirtAt(home) >= Config.CleanAt);

        public string CleanBlocker(ItemInstance home) =>
            !IsBarn(home) ? "Burası temizlenmez." : DirtAt(home) < 1f && Banked(home) < 1f ? "Temiz; yapacak iş yok." : null;

        private float Banked(ItemInstance home) => _bank.TryGetValue(home.Id, out var b) ? b : 0f;

        /// <summary>Barınağı temizler; çıkan gübre adedini döner (gübre yığını varsa oraya, yoksa envantere).</summary>
        public int Clean(ItemInstance home)
        {
            if (!IsBarn(home)) return 0;
            int units = (int)Math.Round(Banked(home));
            _dirt.Remove(home.Id);
            _bank.Remove(home.Id);
            _world.NotifyChanged(home.Id);
            return _compost.AddDung(units);
        }

        /// <summary>Yeni gün: barınaklar kirlenir; günün dışkı olayları kurulur; ağıl toprağı besler.</summary>
        public void OnNewDay()
        {
            bool grazing = Grazing == null || Grazing();
            _due.Clear();
            _dayClock = 0;
            foreach (var home in new List<ItemInstance>(_world.Items))
            {
                if (!IsBarn(home)) continue;
                float raw = 0f; int large = 0;
                foreach (var a in _animals.ResidentsOf(home.Id))
                {
                    if (a.IsDead) continue;
                    float d = DirtOf(a.Species);
                    raw += d;
                    if (d >= Config.DirtLarge) large++;
                }
                if (raw <= 0f) continue;
                if (grazing) raw *= Config.GrazingDirtFactor;   // gündüzü merada geçer
                float factor = 1f;
                if (HasDeepLitter(home))
                {
                    int straw = Math.Max(1, large);
                    if (_house.Count(StrawId) >= straw) { for (int i = 0; i < straw; i++) _house.TryTakeItem(StrawId); factor *= Config.DeepLitterFactor; }
                    else LitterShort?.Invoke(home);
                }
                if (HasDrain(home))
                {
                    factor *= Config.DrainFactor;
                    foreach (var it in _world.Items)
                        if (!it.IsDecor && it.Def.HasTag(CompostService.SlurryTag) && Math.Abs(it.Position.X - home.Position.X) <= 4 && Math.Abs(it.Position.Y - home.Position.Y) <= 4)
                            _compost.RefillSlurry(it);
                }
                _dirt[home.Id] = Math.Min(100f, DirtAt(home) + raw * factor);
                // Derin altlıkta gübre altlıkla birlikte birikir (tavan yüksek); yoksa kirlilikle sınırlı.
                float cap = HasDeepLitter(home) ? Config.DeepLitterBank : Config.DungBankCap;
                _bank[home.Id] = Math.Min(cap, Banked(home) + raw * Config.DungPerDirt);
            }
            var gone = new List<int>();
            foreach (var id in _dirt.Keys) if (_world.GetItem(id) == null) gone.Add(id);
            foreach (var id in gone) { _dirt.Remove(id); _bank.Remove(id); _deepLitter.Remove(id); }

            foreach (var a in _animals.Animals)
            {
                if (a.IsDead) continue;
                if (a.Species.Id == "animal.dog") { if (_rng.NextDouble() < Config.DogDropChance) _due.Add((0.15 + _rng.NextDouble() * 0.6, a.Id, 1)); }
                else if (a.Species.Id == "animal.cat") _due.Add((0.15 + _rng.NextDouble() * 0.6, a.Id, 2));
                else if (grazing && DirtOf(a.Species) >= Config.DirtMedium && _rng.NextDouble() < Config.PastureDropChance)
                    _due.Add((0.3 + _rng.NextDouble() * 0.4, a.Id, 0));
            }
            if (grazing) Fold();
        }

        /// <summary>Gün içinde: kurulan dışkı olayları saati gelince gerçekleşir.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays <= 0 || _due.Count == 0) { _dayClock += Math.Max(0, gameDays); return; }
            _dayClock += gameDays;
            for (int i = _due.Count - 1; i >= 0; i--)
            {
                var (at, id, kind) = _due[i];
                if (at > _dayClock) continue;
                _due.RemoveAt(i);
                var a = _animals.Get(id);
                if (a == null || a.IsDead) continue;
                var room = _world.Map.Rooms.RoomAt(a.Position);
                bool outdoor = room == null || !room.IsIndoor;
                if (kind == 2) { if (outdoor) CatBuried?.Invoke(a); }
                else if (kind == 1 && outdoor) { if (_filth.Drop(PileKind.Dog, a.Position) != null) DogDropped?.Invoke(a); }
                else if (outdoor)
                    _filth.Drop(PileKind.Animal, a.Position,
                        kind == 0 && a.Species.Housing == "barn" && !a.Species.Draft ? FilthService.SheepPileId : null);   // davar kığı, büyükbaş tezek bırakır
            }
        }

        /// <summary>
        /// Ağıla alma: nadas tarlaya dikilen ağıl çitinin çevresinde davar geceler, toprağı gübreler. Ekili karoya
        /// işlemez. Sürü ne kadar büyükse o kadar etkili (4 davar tam etki).
        /// </summary>
        private void Fold()
        {
            if (Plants == null) return;
            int herd = 0;
            foreach (var a in _animals.Animals)
                if (!a.IsDead && a.Species.Housing == "barn" && !a.Species.Draft) herd++;
            if (herd <= 0) return;
            float amount = Config.FoldEnrich * Math.Min(1f, herd / (float)Config.FoldFullHerd);
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.HasTag(FoldTag)) continue;
                int r = Config.FoldRadius;
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        var p = new GridPos(it.Position.X + dx, it.Position.Y + dy, 0);
                        if (!_world.Map.InBounds(p) || _world.Map.IsWater(p)) continue;
                        var on = _world.GetItemAt(p);
                        if (on != null && on != it) continue;
                        if (Plants.PlotOrNull(p) == null) continue;   // yalnızca sürülmüş toprak
                        Plants.Enrich(p, amount, Config.FoldCap);
                    }
            }
        }

        // ---------- kayıt ----------

        public YardBarnSave Export()
        {
            var dirt = new List<YardTimerSave>();
            foreach (var kv in _dirt) dirt.Add(new YardTimerSave { itemId = kv.Key, value = kv.Value });
            dirt.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            var bank = new List<YardTimerSave>();
            foreach (var kv in _bank) bank.Add(new YardTimerSave { itemId = kv.Key, value = kv.Value });
            bank.Sort((a, b) => a.itemId.CompareTo(b.itemId));
            var deep = new List<int>(_deepLitter);
            deep.Sort();
            return new YardBarnSave { dirt = dirt.ToArray(), bank = bank.ToArray(), deepLitter = deep.ToArray() };
        }

        public void Import(YardBarnSave data)
        {
            _dirt.Clear(); _bank.Clear(); _deepLitter.Clear(); _due.Clear();
            if (data == null) return;
            if (data.dirt != null) foreach (var d in data.dirt) if (d != null && _world.GetItem(d.itemId) != null) _dirt[d.itemId] = (float)d.value;
            if (data.bank != null) foreach (var d in data.bank) if (d != null && _world.GetItem(d.itemId) != null) _bank[d.itemId] = (float)d.value;
            if (data.deepLitter != null) foreach (var id in data.deepLitter) if (_world.GetItem(id) != null) _deepLitter.Add(id);
        }
    }
}
