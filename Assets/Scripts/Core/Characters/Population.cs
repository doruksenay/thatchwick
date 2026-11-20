using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Bir hanenin/köyün karakter topluluğu: hepsini ilerletir, doğumda bebek karakter üretir.
    /// Bebeğin kimliği, cinsiyeti ve ömrü burada belirlenir (rastgelelik tohumlanabilir = test edilebilir).
    /// </summary>
    public sealed class Population
    {
        private readonly List<Character> _members = new List<Character>();
        private readonly Random _rng;
        private int _nextId;

        public IReadOnlyList<Character> Members => _members;
        public Func<Sex, string> NameGenerator { get; set; }
        /// <summary>Köylü hanesinin ad listesinden ad (cinsiyet, köylü tanımı kimliği); yoksa NameGenerator (belge 61b §5).</summary>
        public Func<Sex, string, string> HouseholdNameGenerator { get; set; }
        /// <summary>Doğan bebeğin hangi köylü hanesine ait olduğu (anne, baba kimliği → köylü tanımı kimliği ya da null).</summary>
        public Func<Character, int?, string> BirthHousehold { get; set; }

        /// <summary>Bir haneye uygun yeni ad: önce hanenin listesi, sonra genel havuz.</summary>
        public string NameFor(Sex sex, string householdId)
        {
            string name = householdId != null && HouseholdNameGenerator != null ? HouseholdNameGenerator(sex, householdId) : null;
            if (string.IsNullOrWhiteSpace(name) && NameGenerator != null) name = NameGenerator(sex);
            return name;
        }
        /// <summary>Verilirse bebeğin görünümü ebeveynlerinden türetilir; yoksa varsayılan görünüm kalır.</summary>
        public AppearanceCatalog AppearanceCatalog { get; set; }

        /// <summary>Verilirse bebek ebeveynlerinden birer özellik alır ve özellik etkileri hesaplanır.</summary>
        public ContentDatabase Content { get; set; }
        /// <summary>Köy projelerinden gelen etkiler (soğuk direnci gibi); karaktere işlenirken kullanılır.</summary>
        public VillageEffects Village { get; set; }

        /// <summary>(bebek, anne, babanın kimliği)</summary>
        public event Action<Character, Character, int?> BabyBorn;
        /// <summary>Üyelerden biri öldüğünde (Character.Died'ın topluluk düzeyi karşılığı).</summary>
        public event Action<Character> MemberDied;
        /// <summary>Topluluğa yeni biri katıldığında (bebek, köylü, kayıttan yükleme).</summary>
        public event Action<Character> MemberAdded;

        public Population(int firstFreeId = 1, int seed = 12345)
        {
            _nextId = firstFreeId;
            _rng = new Random(seed);
        }

        public int NextId() => _nextId++;

        public void Add(Character c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            if (_members.Contains(c)) return;
            _members.Add(c);
            if (!_byId.ContainsKey(c.Id)) _byId[c.Id] = c;   // aynı kimlikte ilk eklenen geçerli (eski doğrusal aramayla aynı)
            c.GaveBirth += OnGaveBirth;
            c.Died += OnMemberDied;
            if (c.Id >= _nextId) _nextId = c.Id + 1;
            MemberAdded?.Invoke(c);
        }

        // Kimliğe göre dizin: ölüler listeden hiç çıkmadığı için (soy ağacı, mezar) üye sayısı nesiller boyunca büyür;
        // Find her çağrıda bütün listeyi geziyordu.
        private readonly Dictionary<int, Character> _byId = new Dictionary<int, Character>();

        public Character Find(int id) => _byId.TryGetValue(id, out var c) ? c : null;

        /// <summary>Tüm karakterlerin biyolojisini ilerletir (ihtiyaç, yaş, hamilelik, ölüm).</summary>
        public void Tick(double gameDays)
        {
            // Doğumlar listeyi değiştirebilir; üzerinde dolaşırken kopya kullan.
            var snapshot = _members.ToArray();
            foreach (var c in snapshot) c.Tick(gameDays);
        }

        private void OnMemberDied(Character c)
        {
            ReleaseSpouseOf(c);
            MemberDied?.Invoke(c);
        }

        // Dul kalan yeniden evlenebilsin: yaşayan eşin bağı çözülür. Ölenin SpouseId'si kalır (yas, miras ve soy ağacı için).
        private void ReleaseSpouseOf(Character dead)
        {
            foreach (var m in _members)
                if (m != dead && !m.IsDead && m.SpouseId == dead.Id) m.SpouseId = null;
        }

        /// <summary>Tüm üyelerin özellik/dal etkilerini yeniden hesaplar (köy projesi bitince, yüklemeden sonra).</summary>
        public void RecomputeAll()
        {
            if (Content == null) return;
            foreach (var c in _members) LifeRules.Recompute(c, Content, Village);
        }

        /// <summary>
        /// Bebeğin konacağı yer (ör. annenin bulunduğu kapalı oda, değilse evdeki yatağın yanı). Null ise annenin karosu.
        /// Bebek kendi yürümez: dışarıda (helada, tarlada) doğarsa kışın donmasın diye eve konur.
        /// </summary>
        public Func<Character, World.GridPos> BirthPlace { get; set; }

        /// <summary>Kapalı odadaysa olduğu yer; değilse en yakın yatağın (Enerji eşyası) kapalı odadaki boş komşusu.</summary>
        public static World.GridPos ShelterNear(World.WorldState world, World.GridPos at)
        {
            var room = world.Map.Rooms.RoomAt(at);
            if (room != null && room.IsIndoor) return at;
            World.GridPos best = at; int bestD = int.MaxValue;
            foreach (var it in world.Items)
            {
                if (it.IsDecor) continue;
                bool bed = false;
                foreach (var i in it.Def.Interactions) if (i.Need == NeedType.Energy && i.RestoresNeed) { bed = true; break; }
                if (!bed) continue;
                var r = world.Map.Rooms.RoomAt(it.Position);
                if (r == null || !r.IsIndoor) continue;
                foreach (var t in it.OccupiedTiles())
                    foreach (var o in new[] { new World.GridPos(1, 0), new World.GridPos(-1, 0), new World.GridPos(0, 1), new World.GridPos(0, -1) })
                    {
                        var n = t + o;
                        if (!world.Map.IsWalkable(n) || world.Map.Rooms.RoomAt(n) != r) continue;
                        int d = Math.Abs(n.X - at.X) + Math.Abs(n.Y - at.Y) + Math.Abs(n.Z - at.Z) * 20;
                        if (d < bestD) { best = n; bestD = d; }
                    }
            }
            return best;
        }

        private void OnGaveBirth(Character mother, int? fatherId)
        {
            var sex = _rng.Next(2) == 0 ? Sex.Female : Sex.Male;
            string name = NameFor(sex, BirthHousehold?.Invoke(mother, fatherId));
            if (string.IsNullOrWhiteSpace(name)) name = sex == Sex.Female ? "Kız" : "Oğlan";
            if (string.IsNullOrWhiteSpace(name)) name = "Bebek";

            var father0 = fatherId.HasValue ? Find(fatherId.Value) : null;
            var baby = new Character(NextId(), name, BirthPlace != null ? BirthPlace(mother) : mother.Position, false)
            {
                Sex = sex,
                AgeConfig = mother.AgeConfig,
                BirthMotherId = mother.Id,
                BirthFatherId = fatherId,
                Generation = Math.Max(mother.Generation, father0 != null ? father0.Generation : 0) + 1,
                IsVillager = mother.IsVillager,
                CanStarve = mother.CanStarve
            };
            baby.LifespanDays = mother.AgeConfig.RollLifespan(_rng);
            LifeRules.InheritAptitudes(_rng, baby, mother, father0);
            if (Content != null && Content.Traits.Count > 0)
            {
                baby.SetTraits(LifeRules.InheritTraits(Content, _rng, mother, father0));
                baby.LifespanDays += LifeRules.LifespanBonus(Content, baby.TraitIds);
                LifeRules.Recompute(baby, Content, Village);
            }
            if (AppearanceCatalog != null)
            {
                var cat = AppearanceCatalog;
                var father = fatherId.HasValue ? Find(fatherId.Value) : null;
                // Gizli genetik: genomu olmayan ebeveynin (eski kayıt, köylü) genomu görünüşünden kurulur.
                if (mother.Genome == null) mother.Genome = Genetics.FromAppearance(mother.Appearance, cat, _rng);
                if (father != null && father.Genome == null) father.Genome = Genetics.FromAppearance(father.Appearance, cat, _rng);
                if (father != null)
                {
                    baby.Genome = Genetics.Inherit(mother.Genome, father.Genome, _rng, cat);
                    baby.Appearance = cat.FromGenome(baby.Genome, _rng, sex);
                }
                else
                {
                    baby.Appearance = cat.Randomize(_rng, sex);
                    baby.Genome = Genetics.FromAppearance(baby.Appearance, cat, _rng);
                }
            }
            Add(baby);
            BabyBorn?.Invoke(baby, mother, fatherId);
        }
    }
}
