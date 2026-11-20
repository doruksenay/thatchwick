using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;

namespace Thatchwick.Core.Life
{
    /// <summary>Bir hane başının kaydı (soy ağacı ve kronik için).</summary>
    public sealed class HouseHeadRecord
    {
        public int CharacterId;
        public string Name;
        public double FromDay;
    }

    /// <summary>
    /// Köydeki bir komşu hane (Adım 55 kalanı, 18.11.2026): kendi başı, üyeleri ve baş kronikleri vardır.
    /// Adlı köylünün hanesi (Demirci hanesi gibi) dükkânı da taşır: baş ölünce dükkânı yetişkin varis devralır.
    /// </summary>
    public sealed class NeighborHouse
    {
        public int Id;
        public string Name;
        /// <summary>Hanenin taşıdığı adlı köylü tanımı (dükkân); yoksa null.</summary>
        public string DefId;
        public int HeadId;
        public int FounderId;
        public double FoundedDay;
        /// <summary>Soyu tükendi: yaşayan üyesi kalmadı.</summary>
        public bool Extinct;
        /// <summary>
        /// Dükkânsız hanenin evi (kasabadaki köylü evi "home:N" ya da handaki oda "inn:N"; belge 47 §8). Dükkân hanesi dükkânında
        /// yaşar (null). VillageHomes atar; soyu tükenince boşalır.
        /// </summary>
        public string HomeKey;
        /// <summary>Haneye girmiş herkes (ölüler dahil, kayıt sırasıyla).</summary>
        public readonly List<int> MemberIds = new List<int>();
        public readonly List<HouseHeadRecord> Heads = new List<HouseHeadRecord>();
        /// <summary>Kaçıncı baş (1 = kurucu).</summary>
        public int HeadCount => Heads.Count;
    }

    /// <summary>Soy ağacı satırı: bir hane üyesi ve hanedeki kuşağı.</summary>
    public sealed class HouseTreeNode
    {
        public Character Person;
        /// <summary>Hanedeki kuşak (kurucu kuşağı 1).</summary>
        public int Depth;
        public bool IsHead;
        public Character Spouse;
    }

    /// <summary>
    /// Komşu NPC hanedanları: köylüler hanelerde yaşar; haneler evlenir, çocuk yapar (Legacy.NeighborFamiliesDaily),
    /// başları ölünce varis başa geçer, dükkân (adlı köylü rolü) varise kalır. Varis yoksa VillagerService eskisi gibi
    /// dışarıdan bir halef getirir ve onun için yeni hane kurulur. Bekâr yetişkin köylüler zamanla birbirleriyle evlenir;
    /// böylece köy oyuncunun hanedanı olmadan da kuşaktan kuşağa sürer. Genetik Population doğumunda işler.
    /// </summary>
    public sealed class NeighborHouseService
    {
        /// <summary>İki bekâr yetişkin köylünün bir günde evlenme şansı (çift başına değil, gün başına tek deneme).</summary>
        public const double VillageMarriageChance = 0.12;
        /// <summary>Oyuncunun kur yaptığı köylü (romantik yakınlık bu ve üstü) başkasıyla evlendirilmez.</summary>
        public const int CourtedRomance = 15;

        private readonly Population _population;
        private readonly VillagerService _villagers;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly List<NeighborHouse> _houses = new List<NeighborHouse>();
        private readonly Dictionary<int, int> _houseOf = new Dictionary<int, int>();   // karakter → hane
        private int _nextId = 1;

        public IReadOnlyList<NeighborHouse> Houses => _houses;

        /// <summary>Bugünün günü (kurulma ve baş kaydı için).</summary>
        public Func<double> Today { get; set; }

        /// <summary>Yeni bir baş haneyi devraldı (hane, yeni baş, önceki baş).</summary>
        public event Action<NeighborHouse, Character, Character> HeadChanged;
        /// <summary>Dükkân varise geçti (hane, varis, köylü tanımı).</summary>
        public event Action<NeighborHouse, Character, VillagerDef> ShopInherited;
        /// <summary>İki köylü evlendi (gelin ya da damat, eş).</summary>
        public event Action<Character, Character> VillagersMarried;
        /// <summary>Hanenin soyu tükendi.</summary>
        public event Action<NeighborHouse> HouseEnded;

        public NeighborHouseService(Population population, VillagerService villagers, ContentDatabase content, int seed = 71)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _villagers = villagers ?? throw new ArgumentNullException(nameof(villagers));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
            _villagers.Inheritor = InheritorFor;
            _population.BabyBorn += OnBabyBorn;
        }

        private double Now => Today != null ? Today() : 0;

        public NeighborHouse Get(int houseId)
        {
            foreach (var h in _houses) if (h.Id == houseId) return h;
            return null;
        }

        public NeighborHouse HouseOf(Character c) => c != null && _houseOf.TryGetValue(c.Id, out var id) ? Get(id) : null;

        public NeighborHouse HouseOfDef(string defId)
        {
            foreach (var h in _houses) if (!h.Extinct && h.DefId == defId) return h;
            return null;
        }

        public IEnumerable<Character> LivingMembers(NeighborHouse h)
        {
            foreach (var id in h.MemberIds)
            {
                var c = _population.Find(id);
                if (c != null && !c.IsDead && c.IsVillager && _houseOf.TryGetValue(id, out var hid) && hid == h.Id) yield return c;
            }
        }

        public int LivingCount(NeighborHouse h)
        {
            int n = 0;
            foreach (var _ in LivingMembers(h)) n++;
            return n;
        }

        // =====================================================================
        // Kuruluş ve üyelik
        // =====================================================================

        private NeighborHouse Found(Character founder, string defId)
        {
            string name = HouseNameFor(founder, defId);
            var h = new NeighborHouse { Id = _nextId++, Name = name, DefId = defId, HeadId = founder.Id, FounderId = founder.Id, FoundedDay = Now };
            h.Heads.Add(new HouseHeadRecord { CharacterId = founder.Id, Name = founder.Name, FromDay = Now });
            _houses.Add(h);
            Join(h, founder);
            return h;
        }

        private string HouseNameFor(Character founder, string defId)
        {
            string name = defId != null && _content.Villagers.TryGet(defId, out var def) && !string.IsNullOrEmpty(def.RoleName)
                ? $"{def.RoleName} hanesi" : $"{founder.Name} hanesi";
            foreach (var h in _houses) if (h.Name == name) return $"{name} ({founder.Name})";
            return name;
        }

        private void Join(NeighborHouse h, Character c)
        {
            if (_houseOf.TryGetValue(c.Id, out var old) && old == h.Id) return;
            _houseOf[c.Id] = h.Id;
            if (!h.MemberIds.Contains(c.Id)) h.MemberIds.Add(c.Id);
        }

        /// <summary>
        /// Hanesiz köylüleri yerleştirir: adlı köylü kendi dükkân hanesini kurar; evli köylü eşinin hanesine girer
        /// (yoksa koca hane kurar); çocuk babasının (yoksa annesinin) hanesine; kalan bekâr yetişkin kendi hanesini kurar.
        /// Eski kayıtlar da bu yolla hanelere ayrılır.
        /// </summary>
        public void EnsureHouses()
        {
            // 1) Adlı köylüler: dükkân hanesi.
            foreach (var s in _villagers.States)
            {
                var c = _population.Find(s.CharacterId);
                if (c == null || c.IsDead || !c.IsVillager || _houseOf.ContainsKey(c.Id)) continue;
                var existing = HouseOfDef(s.DefId);
                if (existing != null && existing.HeadId == c.Id) { Join(existing, c); continue; }
                // Dışarıdan gelen halef: dükkân eski haneden çıkar, halef kendi hanesini kurar.
                if (existing != null) existing.DefId = null;
                Found(c, s.DefId);
            }
            // 2) Evliler, 3) çocuklar (yaşa göre: önce büyükler, ebeveynleri yerleşsin), 4) bekâr yetişkinler.
            var list = new List<Character>();
            foreach (var c in _population.Members) if (!c.IsDead && c.IsVillager && !_houseOf.ContainsKey(c.Id)) list.Add(c);
            list.Sort((a, b) => b.AgeDays.CompareTo(a.AgeDays));
            foreach (var c in list)
            {
                if (_houseOf.ContainsKey(c.Id)) continue;
                var spouse = c.SpouseId.HasValue ? _population.Find(c.SpouseId.Value) : null;
                if (spouse != null && spouse.IsVillager && !spouse.IsDead)
                {
                    var sh = HouseOf(spouse);
                    if (sh != null) { Join(sh, c); continue; }
                    var husband = c.Sex == Sex.Male ? c : spouse;
                    var wife = c.Sex == Sex.Male ? spouse : c;
                    var nh = Found(husband, null);
                    Join(nh, wife);
                    continue;
                }
                // Bekâr çocuk (yaşı ne olursa olsun) baba ocağında kalır; ebeveyni hanesizse kendi hanesini kurar.
                var parentHouse = ParentHouse(c);
                if (parentHouse != null) { Join(parentHouse, c); continue; }
                Found(c, null);
            }
        }

        private NeighborHouse ParentHouse(Character c)
        {
            foreach (var pid in new[] { c.BirthFatherId, c.BirthMotherId })
            {
                if (!pid.HasValue) continue;
                var p = _population.Find(pid.Value);
                var h = HouseOf(p);
                if (h != null && !h.Extinct) return h;
            }
            return null;
        }

        private bool IsAlive(int id)
        {
            var c = _population.Find(id);
            return c != null && !c.IsDead;
        }

        private void OnBabyBorn(Character baby, Character mother, int? fatherId)
        {
            if (!baby.IsVillager) return;
            var father = fatherId.HasValue ? _population.Find(fatherId.Value) : null;
            var h = HouseOf(father) ?? HouseOf(mother);
            if (h != null) Join(h, baby);
        }

        // =====================================================================
        // Evlilik
        // =====================================================================

        /// <summary>
        /// İki köylüyü evlendirir ve aynı haneye toplar: dükkân hanesinde olan hanesinde kalır (ikisi de dükkân hanesindeyse ya
        /// da hiçbiri değilse gelin damadın hanesine gider). Boşalan hanenin soyu tükenir.
        /// </summary>
        public void Marry(Character a, Character b)
        {
            a.SpouseId = b.Id;
            b.SpouseId = a.Id;
            MoodEvents.OnMarriage(a, b);
            JoinSpouses(a, b);
            VillagersMarried?.Invoke(a, b);
        }

        /// <summary>Evlilikten sonra eşleri aynı haneye toplar (komşu haneye evlendirme de buradan geçer).</summary>
        public void JoinSpouses(Character a, Character b)
        {
            EnsureHouses();
            var ha = HouseOf(a);
            var hb = HouseOf(b);
            if (ha == null || hb == null || ha == hb) return;
            bool shopA = ha.DefId != null && ha.HeadId == a.Id;
            bool shopB = hb.DefId != null && hb.HeadId == b.Id;
            Character mover; NeighborHouse target;
            if (shopA && !shopB) { mover = b; target = ha; }
            else if (shopB && !shopA) { mover = a; target = hb; }
            else
            {
                var wife = a.Sex == Sex.Female ? a : b;
                mover = wife;
                target = wife == a ? hb : ha;
            }
            var from = HouseOf(mover);
            Join(target, mover);
            if (from != null) AfterLeave(from);
        }

        private void AfterLeave(NeighborHouse h)
        {
            if (h.Extinct) return;
            if (LivingCount(h) == 0) { End(h); return; }
            if (!IsAlive(h.HeadId) || HouseOf(_population.Find(h.HeadId)) != h) Succeed(h);
        }

        /// <summary>Köyde bekâr yetişkin köylüler arasında günde bir evlilik denemesi; evlenen çifti döner (yoksa null).</summary>
        public (Character, Character)? TryVillageMarriage()
        {
            var singles = new List<Character>();
            foreach (var c in _population.Members)
            {
                if (c.IsDead || !c.IsVillager || c.SpouseId.HasValue || !AgeStages.CanMarry(c.Stage)) continue;
                var st = _villagers.StateOf(c);
                if (st != null && st.Romance >= CourtedRomance) continue;    // oyuncu kur yapıyor
                singles.Add(c);
            }
            if (singles.Count < 2 || _rng.NextDouble() >= VillageMarriageChance) return null;
            var a = singles[_rng.Next(singles.Count)];
            var options = new List<Character>();
            foreach (var b in singles)
                if (b != a && b.Sex != a.Sex && !FamilyService.AreCloseRelatives(a, b) && !Legacy.ShareGrandparent(a, b, _population))
                    options.Add(b);
            if (options.Count == 0) return null;
            var mate = options[_rng.Next(options.Count)];
            Marry(a, mate);
            return (a, mate);
        }

        // =====================================================================
        // Baş değişimi ve dükkân
        // =====================================================================

        /// <summary>
        /// Varis sırası: başın en büyük çocuğu (ergen ve üstü), sonra en büyük çocuğu, sonra eşi, sonra hanenin en yaşlı üyesi.
        /// </summary>
        public Character ChooseHeir(NeighborHouse h, Character oldHead)
        {
            var living = new List<Character>(LivingMembers(h));
            if (living.Count == 0) return null;
            Character Oldest(Func<Character, bool> f)
            {
                Character best = null;
                foreach (var c in living) if (f(c) && (best == null || c.AgeDays > best.AgeDays)) best = c;
                return best;
            }
            bool IsChildOf(Character c) => oldHead != null && (c.BirthFatherId == oldHead.Id || c.BirthMotherId == oldHead.Id);
            return Oldest(c => IsChildOf(c) && c.Stage >= AgeStage.Teen)
                ?? Oldest(IsChildOf)
                ?? (oldHead != null && oldHead.SpouseId.HasValue ? living.Find(c => c.Id == oldHead.SpouseId.Value) : null)
                ?? Oldest(c => true);
        }

        private void SetHead(NeighborHouse h, Character heir)
        {
            var old = _population.Find(h.HeadId);
            h.HeadId = heir.Id;
            h.Heads.Add(new HouseHeadRecord { CharacterId = heir.Id, Name = heir.Name, FromDay = Now });
            HeadChanged?.Invoke(h, heir, old);
        }

        private void Succeed(NeighborHouse h)
        {
            var old = _population.Find(h.HeadId);
            var heir = ChooseHeir(h, old);
            if (heir == null) { End(h); return; }
            SetHead(h, heir);
        }

        private void End(NeighborHouse h)
        {
            if (h.Extinct) return;
            h.Extinct = true;
            HouseEnded?.Invoke(h);
        }

        /// <summary>
        /// VillagerService halef getirmeden önce sorar: ölen adlı köylünün hanesinde dükkânı sürdürecek yetişkin
        /// (genç yetişkin ve üstü) varis var mı? Varsa dükkân ona geçer, dışarıdan halef gelmez.
        /// </summary>
        private Character InheritorFor(VillagerDef def, Character departed)
        {
            var h = HouseOfDef(def.Id);
            if (h == null) return null;
            if (!IsAlive(h.HeadId)) Succeed(h);
            if (h.Extinct) return null;
            var head = _population.Find(h.HeadId);
            if (head == null || head.IsDead || head.Stage < AgeStage.YoungAdult) return null;
            ShopInherited?.Invoke(h, head, def);
            return head;
        }

        /// <summary>
        /// Gün başı: hanesizleri yerleştirir, ölen başların yerine varis geçirir, boşalan haneleri kapatır,
        /// köyde bir evlilik dener. Dışarıdan gelen halefler (VillagerService) bir sonraki çağrıda kendi hanelerini kurar.
        /// </summary>
        public void OnNewDay()
        {
            EnsureHouses();
            foreach (var h in new List<NeighborHouse>(_houses))
            {
                if (h.Extinct) continue;
                if (LivingCount(h) == 0) { End(h); continue; }
                if (!IsAlive(h.HeadId)) Succeed(h);
            }
            TryVillageMarriage();
        }

        // =====================================================================
        // Soy ağacı
        // =====================================================================

        /// <summary>
        /// Hanenin soy ağacı: kurucudan başlayarak ebeveyn-çocuk bağıyla kuşaklara dizilmiş üyeler (ölüler dahil).
        /// Haneye evlilikle girenler eşlerinin kuşağında gösterilir.
        /// </summary>
        public List<HouseTreeNode> TreeOf(NeighborHouse h)
        {
            var depth = new Dictionary<int, int>();
            var members = new List<Character>();
            foreach (var id in h.MemberIds) { var c = _population.Find(id); if (c != null) members.Add(c); }
            bool changed = true;
            if (members.Count > 0) depth[h.FounderId] = 1;
            for (int guard = 0; changed && guard < 64; guard++)
            {
                changed = false;
                foreach (var c in members)
                {
                    if (depth.ContainsKey(c.Id)) continue;
                    int d = 0;
                    foreach (var pid in new[] { c.BirthFatherId, c.BirthMotherId })
                        if (pid.HasValue && depth.TryGetValue(pid.Value, out var pd)) d = Math.Max(d, pd + 1);
                    if (d == 0 && c.SpouseId.HasValue && depth.TryGetValue(c.SpouseId.Value, out var sd)) d = sd;
                    if (d > 0) { depth[c.Id] = d; changed = true; }
                }
            }
            var nodes = new List<HouseTreeNode>();
            foreach (var c in members)
                nodes.Add(new HouseTreeNode
                {
                    Person = c,
                    Depth = depth.TryGetValue(c.Id, out var d) ? d : 1,
                    IsHead = c.Id == h.HeadId,
                    Spouse = c.SpouseId.HasValue ? _population.Find(c.SpouseId.Value) : null
                });
            nodes.Sort((a, b) => a.Depth != b.Depth ? a.Depth.CompareTo(b.Depth) : b.Person.AgeDays.CompareTo(a.Person.AgeDays));
            return nodes;
        }

        /// <summary>Hanenin kuşak sayısı (soy ağacının en derin kuşağı).</summary>
        public int Generations(NeighborHouse h)
        {
            int max = 0;
            foreach (var n in TreeOf(h)) max = Math.Max(max, n.Depth);
            return max;
        }

        // =====================================================================
        // Kayıt
        // =====================================================================

        public NeighborHouseSave[] Export()
        {
            var list = new List<NeighborHouseSave>();
            foreach (var h in _houses)
            {
                var heads = new List<HouseHeadSave>();
                foreach (var r in h.Heads) heads.Add(new HouseHeadSave { id = r.CharacterId, name = r.Name, day = r.FromDay });
                var current = new List<int>();
                foreach (var id in h.MemberIds) if (_houseOf.TryGetValue(id, out var hid) && hid == h.Id) current.Add(id);
                list.Add(new NeighborHouseSave
                {
                    id = h.Id, name = h.Name, defId = h.DefId, headId = h.HeadId, founderId = h.FounderId, foundedDay = h.FoundedDay,
                    extinct = h.Extinct, members = h.MemberIds.ToArray(), current = current.ToArray(), heads = heads.ToArray(), homeKey = h.HomeKey
                });
            }
            return list.ToArray();
        }

        /// <summary>Kayıttan yükler. Eski kayıtta (null) haneler mevcut köylülerden kurulur (EnsureHouses).</summary>
        public void Restore(NeighborHouseSave[] data)
        {
            _houses.Clear();
            _houseOf.Clear();
            _nextId = 1;
            if (data != null)
                foreach (var s in data)
                {
                    if (s == null) continue;
                    var h = new NeighborHouse
                    {
                        Id = s.id, Name = s.name, DefId = s.defId, HeadId = s.headId, FounderId = s.founderId, FoundedDay = s.foundedDay, Extinct = s.extinct,
                        HomeKey = s.homeKey
                    };
                    if (s.members != null) h.MemberIds.AddRange(s.members);
                    if (s.heads != null) foreach (var r in s.heads) if (r != null) h.Heads.Add(new HouseHeadRecord { CharacterId = r.id, Name = r.name, FromDay = r.day });
                    if (s.current != null) foreach (var id in s.current) _houseOf[id] = h.Id;
                    _houses.Add(h);
                    if (h.Id >= _nextId) _nextId = h.Id + 1;
                }
            EnsureHouses();
        }
    }
}
