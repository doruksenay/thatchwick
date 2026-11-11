using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    /// <summary>
    /// Hayvanlar: satın alma (barınakta yer varsa), beslenme (barınağın yem stoğundan), ürün
    /// (yumurta, süt, yün barınağa birikir; "Topla" etkileşimi haneye alır), açlıktan ve yaşlılıktan
    /// ölüm, barınak çevresinde gezinme. Tohumlu rastgele sayı: testlerde tekrarlanabilir.
    /// </summary>
    public sealed class AnimalSystem
    {
        /// <summary>Tokluğun günlük değişimi: yem yetiyorsa artar, yetmiyorsa düşer.</summary>
        public const float FullnessChangePerDay = 50f;
        /// <summary>Tokluk 0'da bu kadar gün kalırsa açlıktan ölür.</summary>
        public const double StarveDeathDays = 3.0;
        /// <summary>Ürün vermesi için gereken en düşük tokluk.</summary>
        public const float ProduceFullness = 50f;
        /// <summary>Barınakta biriken ürün tavanı (hayvan başına).</summary>
        public const int StockPerResident = 4;
        /// <summary>Barınaktan en çok bu kadar karo uzağa gezinir.</summary>
        public const int WanderRadius = 3;
        /// <summary>Bir karo yürüme süresi (oyun günü) ve duraklama aralığı.</summary>
        public const double StepDays = 0.004, MinWaitDays = 0.006, MaxWaitDays = 0.03;
        public const double SellRatio = 0.5;
        /// <summary>Dostluk: tokken günde +FriendshipPerDay, aç kalınca günde -HungerFriendLoss; sevmek +PetFriendship. Çarpan = 1 + FriendshipBoost x dostluk/100.</summary>
        public const float FriendshipPerDay = 3f, HungerFriendLoss = 2f, PetFriendship = 8f, FriendshipBoost = 0.4f;
        /// <summary>Bu dostlukta üründe ekstra bir adet şansı.</summary>
        public const float BondedFriendship = 70f;
        public const double BondedExtraChance = 0.25;

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly List<Animal> _animals = new List<Animal>();
        private int _nextId = 1;

        public IReadOnlyList<Animal> Animals => _animals;

        /// <summary>Hayvan öldü (açlık ya da yaşlılık): moodlet ve bildirim için.</summary>
        public event Action<Animal> Died;
        /// <summary>Barınağa ürün bıraktı.</summary>
        public event Action<Animal, string> Produced;

        public AnimalSystem(WorldState world, ContentDatabase content, int seed = 7)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        public int NextId => _nextId;

        public int Residents(int homeItemId)
        {
            int n = 0;
            foreach (var a in _animals) if (!a.IsDead && a.HomeItemId == homeItemId) n++;
            return n;
        }

        public List<Animal> ResidentsOf(int homeItemId)
        {
            var list = new List<Animal>();
            foreach (var a in _animals) if (!a.IsDead && a.HomeItemId == homeItemId) list.Add(a);
            return list;
        }

        /// <summary>Bu türe yer olan ilk barınak; yoksa null.</summary>
        public ItemInstance FindHome(AnimalSpeciesDef species)
        {
            foreach (var item in _world.Items)
                if (item.Def.HousingKind == species.Housing && !item.IsDecor && Residents(item.Id) < item.Def.HousingCapacity)
                    return item;
            return null;
        }

        /// <summary>Hayvan satın alır: akçe ve barınakta boş yer gerekir. Barınağın yanında belirir.</summary>
        public PlacementCheck Buy(string speciesId, Household house, out Animal animal, int accountLevel = int.MaxValue)
        {
            animal = null;
            if (!_content.Animals.TryGet(speciesId, out var species)) return PlacementCheck.Fail("Böyle bir hayvan yok.");
            if (accountLevel < species.UnlockLevel) return PlacementCheck.Fail($"Hesap seviyesi {species.UnlockLevel} gerekli.");
            var home = FindHome(species);
            if (home == null) return PlacementCheck.Fail($"Önce barınak koy ({HousingName(species.Housing)}) ya da yer aç.");
            if (!house.TrySpend(species.Price)) return PlacementCheck.Fail("Akçe yetmiyor.");
            animal = Spawn(species, home);
            return PlacementCheck.Success;
        }

        private string HousingName(string kind)
        {
            foreach (var d in _content.Items.All) if (d.HousingKind == kind) return d.DisplayName;
            return kind;
        }

        /// <summary>Hayvanı satar (fiyatın yarısı).</summary>
        public PlacementCheck Sell(int animalId, Household house)
        {
            var a = Get(animalId);
            if (a == null || a.IsDead) return PlacementCheck.Fail("Hayvan bulunamadı.");
            _animals.Remove(a);
            house.AddAkce((int)Math.Floor(a.Species.Price * SellRatio));
            return PlacementCheck.Success;
        }

        public Animal Get(int id)
        {
            foreach (var a in _animals) if (a.Id == id) return a;
            return null;
        }

        internal Animal Spawn(AnimalSpeciesDef species, ItemInstance home)
        {
            var pos = SpawnTile(home);
            var a = new Animal(_nextId++, species, pos, home.Id);
            a.WaitDays = _rng.NextDouble() * MaxWaitDays;
            _animals.Add(a);
            return a;
        }

        private GridPos SpawnTile(ItemInstance home)
        {
            foreach (var t in home.OccupiedTiles())
                foreach (var o in Offsets)
                {
                    var n = t + o;
                    if (_world.Map.IsWalkable(n)) return n;
                }
            return home.Position;
        }

        private static readonly GridPos[] Offsets = { new GridPos(1, 0), new GridPos(0, 1), new GridPos(-1, 0), new GridPos(0, -1) };

        /// <summary>Kayıttan yükleme.</summary>
        public Animal Restore(int id, AnimalSpeciesDef species, GridPos pos, int homeId, float fullness, double age, double produce, double hungryDays, float friendship = 0f)
        {
            var a = new Animal(id, species, pos, homeId)
            {
                Friendship = Math.Max(0f, Math.Min(100f, friendship)),
                Fullness = Math.Max(0f, Math.Min(100f, fullness)),
                AgeDays = Math.Max(0, age),
                ProduceProgress = Math.Max(0, produce),
                HungryDays = Math.Max(0, hungryDays)
            };
            _animals.Add(a);
            if (id >= _nextId) _nextId = id + 1;
            return a;
        }

        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));
            if (gameDays == 0) return;

            for (int i = _animals.Count - 1; i >= 0; i--)
            {
                var a = _animals[i];
                if (a.IsDead) continue;
                var home = _world.GetItem(a.HomeItemId);

                a.AgeDays += gameDays;
                if (a.AgeDays >= a.Species.LifespanDays) { Kill(a, AnimalDeath.OldAge); continue; }

                // Beslenme: barınaktaki yemden payına düşeni yer.
                double need = a.Species.FeedPerDay * gameDays;
                double eaten = 0;
                if (need > 0 && home != null)
                {
                    eaten = Math.Min(need, home.FeedStock);
                    home.FeedStock -= eaten;
                }
                double fed = need > 0 ? eaten / need : 1.0;
                a.Fullness = (float)Math.Max(0, Math.Min(100, a.Fullness + (fed * 2 - 1) * FullnessChangePerDay * gameDays));

                // Dostluk: iyi bakılınca yavaş yavaş artar, aç bırakılınca düşer.
                if (a.PetCooldownDays > 0) a.PetCooldownDays = Math.Max(0, a.PetCooldownDays - gameDays);
                if (a.Fullness >= 60f) a.Friendship = Math.Min(100f, a.Friendship + (float)(FriendshipPerDay * gameDays));
                else if (a.Fullness < 20f) a.Friendship = Math.Max(0f, a.Friendship - (float)(HungerFriendLoss * gameDays));

                if (a.Fullness <= 0f) a.HungryDays += gameDays;
                else a.HungryDays = Math.Max(0, a.HungryDays - gameDays);
                if (a.HungryDays >= StarveDeathDays) { Kill(a, AnimalDeath.Starvation); continue; }

                // Ürün: tok ve erişkinken barınağa birikir.
                if (a.Species.Produces != null && home != null && a.IsMature && a.Fullness >= ProduceFullness)
                {
                    a.ProduceProgress += gameDays / a.Species.ProduceEveryDays * (1.0 + FriendshipBoost * a.Friendship / 100.0);
                    while (a.ProduceProgress >= 1.0)
                    {
                        a.ProduceProgress -= 1.0;
                        int cap = StockPerResident * Math.Max(1, home.Def.HousingCapacity);
                        if (home.StockTotal >= cap) { a.ProduceProgress = 0; break; }
                        int units = 1;
                        if (a.Friendship >= BondedFriendship && _rng.NextDouble() < BondedExtraChance) units++;
                        home.AddStock(a.Species.Produces, units);
                        Produced?.Invoke(a, a.Species.Produces);
                    }
                }

                Wander(a, home, gameDays);
            }
        }

        private void Kill(Animal a, AnimalDeath cause)
        {
            a.IsDead = true;
            a.CauseOfDeath = cause;
            a.NextTile = null;
            _animals.Remove(a);
            Died?.Invoke(a);
        }

        // Barınak çevresinde rastgele bir karoya adım adım yürür, varınca bir süre durur.
        private void Wander(Animal a, ItemInstance home, double gameDays)
        {
            double budget = gameDays;
            for (int guard = 0; guard < 32 && budget > 1e-12; guard++)
            {
                if (a.NextTile == null)
                {
                    if (a.WaitDays > 0)
                    {
                        double w = Math.Min(a.WaitDays, budget);
                        a.WaitDays -= w;
                        budget -= w;
                        continue;
                    }
                    if (a.Position == a.Target) a.Target = PickTarget(a, home);
                    var step = StepToward(a.Position, a.Target);
                    if (step == null)
                    {
                        a.Target = a.Position;
                        a.WaitDays = MinWaitDays + _rng.NextDouble() * (MaxWaitDays - MinWaitDays);
                        continue;
                    }
                    a.NextTile = step;
                    a.StepProgress = 0f;
                }

                double needed = (1.0 - a.StepProgress) * StepDays;
                if (budget >= needed)
                {
                    budget -= needed;
                    a.Position = a.NextTile.Value;
                    a.NextTile = null;
                    a.StepProgress = 0f;
                    if (a.Position == a.Target) a.WaitDays = MinWaitDays + _rng.NextDouble() * (MaxWaitDays - MinWaitDays);
                }
                else
                {
                    a.StepProgress += (float)(budget / StepDays);
                    budget = 0;
                }
            }
        }

        private GridPos PickTarget(Animal a, ItemInstance home)
        {
            var center = home != null ? home.Position : a.Position;
            for (int tries = 0; tries < 8; tries++)
            {
                var t = new GridPos(center.X + _rng.Next(-WanderRadius, WanderRadius + 1), center.Y + _rng.Next(-WanderRadius, WanderRadius + 1));
                if (_world.Map.IsWalkable(t)) return t;
            }
            return a.Position;
        }

        // Hedefe bir karo yaklaşan, duvarla kesilmeyen adım; yoksa null.
        private GridPos? StepToward(GridPos from, GridPos to)
        {
            if (from == to) return null;
            int dx = Math.Sign(to.X - from.X), dy = Math.Sign(to.Y - from.Y);
            var options = Math.Abs(to.X - from.X) >= Math.Abs(to.Y - from.Y)
                ? new[] { new GridPos(dx, 0), new GridPos(0, dy) }
                : new[] { new GridPos(0, dy), new GridPos(dx, 0) };
            foreach (var o in options)
            {
                if (o.X == 0 && o.Y == 0) continue;
                var n = from + o;
                if (_world.Map.CanStep(from, n)) return n;
            }
            return null;
        }

        /// <summary>Barınaktaki, bugün henüz sevilmemiş hayvan sayısı.</summary>
        public int PettableResidents(int homeItemId)
        {
            int n = 0;
            foreach (var a in _animals) if (a.HomeItemId == homeItemId && a.PetCooldownDays <= 0) n++;
            return n;
        }

        /// <summary>Barınaktaki hayvanları sever: dostluk artar, günlük bekleme başlar. Sevilen sayı döner.</summary>
        public int PetResidents(int homeItemId)
        {
            int n = 0;
            foreach (var a in _animals)
            {
                if (a.HomeItemId != homeItemId || a.PetCooldownDays > 0) continue;
                a.Friendship = Math.Min(100f, a.Friendship + PetFriendship);
                a.PetCooldownDays = 1.0;
                n++;
            }
            return n;
        }

        // ---- barınak etkileşimleri (InteractionEffects çağırır) ----

        public static void AddFeed(ItemInstance home, double units) { if (units > 0) home.FeedStock += units; }

        /// <summary>Barınaktaki ürünleri boşaltır ve (kimlik, adet) listesini döner.</summary>
        public static List<KeyValuePair<string, int>> TakeStock(ItemInstance home)
        {
            var list = new List<KeyValuePair<string, int>>(home.Stock);
            foreach (var kv in list) home.SetStock(kv.Key, 0);
            return list;
        }

        /// <summary>Kayıttan ya da depo dolu olunca geri koyma.</summary>
        public static void PutStock(ItemInstance home, string itemId, int count) { if (count > 0) home.AddStock(itemId, count); }
    }
}
