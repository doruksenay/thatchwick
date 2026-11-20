using Thatchwick.Core.Animals.Genetics;
using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    /// <summary>
    /// Hayvanlar: satın alma (barınakta yer varsa), beslenme (barınağın yem stoğundan), ürün
    /// (yumurta, süt, yün barınağa birikir; "Topla" etkileşimi haneye alır), açlıktan ve yaşlılıktan
    /// ölüm, barınak çevresinde gezinme. Tohumlu rastgele sayı: testlerde tekrarlanabilir.
    /// </summary>
    public sealed partial class AnimalSystem
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
        /// <summary>Soy hayvanının (3. kuşak ve sonrası) ürün hızı çarpanı.</summary>
        public const double HeritageOutput = 1.05;

        /// <summary>Hayvan ürün kalitesi puanı = dostluk x 0,6 + konfor x 0,4 (soy hayvanına +10). Seçme 55, birinci sınıf 80.</summary>
        public const float QualityFineScore = 55f, QualityFinestScore = 80f, HeritageQualityBonus = 10f;

        public static int QualityTierOf(float friendship, float comfort, bool heritage, float bonus = 0f)
        {
            float score = friendship * 0.6f + comfort * 0.4f + (heritage ? HeritageQualityBonus : 0f) + bonus;
            return score >= QualityFinestScore ? 2 : (score >= QualityFineScore ? 1 : 0);
        }

        /// <summary>Ek kalite puanı (can yoldaşı yetenekleri: Sabırlı El, Çoban Ruhu, Kümes Çocuğu; belge 31-8).</summary>
        public Func<Animal, float> QualityBonus { get; set; }

        /// <summary>Hayvanın ürün kalitesi basamağı, ek puanlar dahil.</summary>
        public int QualityOf(Animal a) => QualityTierOf(a.Friendship, a.Comfort, a.IsHeritage, (QualityBonus != null ? QualityBonus(a) : 0f) + AnimalProgress.QualityBonus(a.Lineage, a.Level));

        public static string QualityName(int tier) => Work.PlantSystem.QualityName(tier);
        public static float QualityYield(int tier) => Work.PlantSystem.QualityYield(tier);

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

        /// <summary>Şu anki mevsim (ürünün mevsim çarpanı için); null ise hep 1.</summary>
        public Func<Time.Season> SeasonNow { get; set; }

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
        /// <summary>Hanenin en iyi beceri seviyesi; verilirse hayvan kilidi Hayvancılık'a bakar (karar 28-4).</summary>
        public Func<SkillType, int> SkillLevel { get; set; }

        public PlacementCheck Buy(string speciesId, Household house, out Animal animal, int accountLevel = int.MaxValue, bool? female = null, string typeId = null)
        {
            animal = null;
            if (!_content.Animals.TryGet(speciesId, out var species)) return PlacementCheck.Fail("Böyle bir hayvan yok.");
            if (SkillLevel != null ? !Thatchwick.Core.Progress.UnlockGate.IsOpen(SkillLevel, null, SkillType.AnimalHusbandry, species.UnlockLevel) : accountLevel < species.UnlockLevel)
                return PlacementCheck.Fail(Thatchwick.Core.Progress.UnlockGate.Text(SkillLevel, SkillType.AnimalHusbandry, species.UnlockLevel));

            var home = FindHome(species);
            if (home == null) return PlacementCheck.Fail($"Önce barınak koy ({HousingName(species.Housing)}) ya da yer aç.");
            if (AtPetCap(species)) return PlacementCheck.Fail($"Hanede en çok {PetCap} {species.DisplayName.ToLowerInvariant()} bakılır.");
            if (!house.TrySpend(species.Price)) return PlacementCheck.Fail("Akçe yetmiyor.");
            animal = Spawn(species, home, BuyAsFemale(species, female));
            AssignGenome(animal, typeId);   // genomlu tür: istenen tip ya da pazar tipi (belge 48)
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Satın alınanın cinsiyeti: oyuncu seçtiyse o; seçmediyse sürüde dişi varken erkek yoksa erkek (yavru için), yoksa dişi
        /// (süt ve yumurta dişiden). Böylece ilk iki hayvan bir çift olur, gerisi dişi.
        /// </summary>
        public bool BuyAsFemale(AnimalSpeciesDef species, bool? female = null)
        {
            if (female.HasValue) return female.Value;
            if (!species.Breeds) return true;
            bool anyFemale = false, anyMale = false;
            foreach (var a in _animals)
            {
                if (a.IsDead || a.Species.Id != species.Id) continue;
                if (a.IsFemale) anyFemale = true; else anyMale = true;
            }
            return !(anyFemale && !anyMale);
        }

        private string HousingName(string kind)
        {
            foreach (var d in _content.Items.All) if (d.HousingKind == kind) return d.DisplayName;
            return kind;
        }

        /// <summary>Hayvanı satar (fiyatın yarısı).</summary>
        /// <summary>Hayvan satıldı ya da kesildi (anası yavrusunu arar; belge 55 §3.2).</summary>
        public event Action<Animal> TakenAway;

        /// <summary>Satış bedeli: tür fiyatının yarısı, soy ve seviyeyle katlanır (AnimalProgress.ValueFactor).</summary>
        public static int SellValue(Animal a) =>
            a == null ? 0 : (int)Math.Floor(a.Species.Price * SellRatio * AnimalProgress.ValueFactor(a.Lineage, a.Level));

        public PlacementCheck Sell(int animalId, Household house)
        {
            var a = Get(animalId);
            if (a == null || a.IsDead) return PlacementCheck.Fail("Hayvan bulunamadı.");
            if (a.RiderId != 0) return PlacementCheck.Fail("Önce attan in.");
            if (a.Saddle != null) house.AddItem(a.Saddle);    // eyer hayvanla gitmez
            _animals.Remove(a);
            TakenAway?.Invoke(a);
            house.AddAkce(SellValue(a));
            return PlacementCheck.Success;
        }

        /// <summary>Bu dostluğun üstündeki hayvana kıyılmaz.</summary>
        public const float SpareFriendship = 70f;

        /// <summary>Kesilecek hayvan: yetişkin, eti olan ve dostu olmayan; dostluğu en düşük (eşitse en yaşlı). Yoksa null.</summary>
        public Animal SlaughterCandidate(int homeItemId)
        {
            Animal best = null;
            foreach (var a in _animals)
            {
                if (a.IsDead || a.HomeItemId != homeItemId || a.Species.Meat == null) continue;
                if (!a.IsMature || a.Friendship >= SpareFriendship) continue;
                if (best == null || a.Friendship < best.Friendship || (a.Friendship == best.Friendship && a.AgeDays > best.AgeDays)) best = a;
            }
            return best;
        }

        /// <summary>Barınaktaki en uygun hayvanı keser; hayvan kalkar, eti ve adedi döner. Aday yoksa false.</summary>
        public bool Slaughter(int homeItemId, out Animal animal, out string meatId, out int meatCount)
        {
            animal = SlaughterCandidate(homeItemId);
            meatId = null; meatCount = 0;
            if (animal == null) return false;
            meatId = animal.Species.Meat;
            meatCount = Math.Max(1, animal.Species.MeatCount);
            _animals.Remove(animal);
            TakenAway?.Invoke(animal);
            return true;
        }

        /// <summary>Mezbaha ya da pazar kesimi (belge 60a): seçilen hayvan sürüden çıkar; ürünleri Cooking.Slaughterhouse verir.</summary>
        public bool RemoveForSlaughter(Animal a)
        {
            if (a == null || !_animals.Remove(a)) return false;
            TakenAway?.Invoke(a);
            return true;
        }

        /// <summary>Kesim işi başlamadan neden olmayacağı (null: olur).</summary>
        public string SlaughterBlocker(int homeItemId)
        {
            bool any = false, mature = false, hasMeat = false;
            foreach (var a in _animals)
            {
                if (a.IsDead || a.HomeItemId != homeItemId) continue;
                any = true;
                if (a.Species.Meat != null) hasMeat = true;
                if (a.Species.Meat != null && a.IsMature) mature = true;
            }
            if (!any) return "Burada hayvan yok.";
            if (!hasMeat) return "Bu hayvanlar kesilmez.";
            if (!mature) return "Kesilecek kadar büyümüş hayvan yok.";
            return SlaughterCandidate(homeItemId) == null ? "Kalan hayvanlar artık dost; kıyamazsın." : null;
        }

        /// <summary>Düven çekebilecek hayvan: çeki türü, erişkin, sağlıklı, tok, kimsenin altında değil.</summary>
        public int DraftAnimals()
        {
            int n = 0;
            foreach (var a in _animals)
                if (!a.IsDead && a.Species.Draft && a.IsMature && !a.IsSick && !a.IsPregnant && a.Fullness >= ProduceFullness && a.RiderId == 0) n++;
            return n;
        }

        /// <summary>Binicinin atını yeni karoya taşır (engel atlama).</summary>
        public void MoveMount(int riderId, GridPos to)
        {
            var a = MountOf(riderId);
            if (a != null) a.Position = to;
        }

        public Animal Get(int id)
        {
            foreach (var a in _animals) if (a.Id == id) return a;
            return null;
        }

        internal Animal Spawn(AnimalSpeciesDef species, ItemInstance home, bool female = true)
        {
            var pos = SpawnTile(home);
            var a = new Animal(_nextId++, species, pos, home.Id) { IsFemale = female && !species.MaleOnly };
            a.WaitDays = _rng.NextDouble() * MaxWaitDays;
            _animals.Add(a);
            RecordPedigree(a);
            return a;
        }

        private GridPos SpawnTile(ItemInstance home)
        {
            foreach (var t in home.OccupiedTiles())
                foreach (var o in Offsets)
                {
                    var n = t + o;
                    // Duvarın öbür yanına (ahırın dışına) doğmasın.
                    if (_world.Map.IsWalkable(n) && !_world.Map.EdgeBlocks(GridMap.EdgeBetween(t, n))) return n;
                }
            return home.Position;
        }

        private static readonly GridPos[] Offsets = { new GridPos(1, 0), new GridPos(0, 1), new GridPos(-1, 0), new GridPos(0, -1) };

        /// <summary>Kayıttan yükleme.</summary>
        public Animal Restore(int id, AnimalSpeciesDef species, GridPos pos, int homeId, float fullness, double age, double produce, double hungryDays, float friendship = 0f, double petCooldown = 0,
            string saddle = null, float comfort = 50f, bool sick = false, double sickDays = 0, int coldDays = 0,
            bool? female = null, double pregnantDays = 0, int motherId = 0, int generation = 0,
            int lineage = 0, double xp = 0, bool legendaryDescent = false, string legendTitle = null, float stamina = -1f, double shyDays = 0,
            string[] genome = null, int fatherId = 0, string[] deeds = null, int sireId = 0, string sireSpecies = null, string[] sireGenome = null)
        {
            var a = new Animal(id, species, pos, homeId)
            {
                IsFemale = !species.MaleOnly && (female ?? (id % 2 == 1)),          // eski kayıt: tek kimlik dişi, çift kimlik erkek
                PregnantDays = (species.Breeds || species.Sterile) && (female ?? (id % 2 == 1)) ? Math.Max(0, pregnantDays) : 0,
                MotherId = Math.Max(0, motherId),
                Generation = Math.Max(0, generation),
                Friendship = Math.Max(0f, Math.Min(100f, friendship)),
                Fullness = Math.Max(0f, Math.Min(100f, fullness)),
                AgeDays = Math.Max(0, age),
                ProduceProgress = Math.Max(0, produce),
                HungryDays = Math.Max(0, hungryDays),
                PetCooldownDays = Math.Max(0, Math.Min(1.0, petCooldown)),
                Saddle = species.Rideable && saddle != null && _content.Items.Contains(saddle) ? saddle : null,
                Comfort = Math.Max(0f, Math.Min(100f, comfort)),
                IsSick = sick,
                SickDays = sick ? Math.Max(0, sickDays) : 0,
                ColdDays = Math.Max(0, coldDays),
                IsCold = coldDays > 0,
                Lineage = (AnimalLineage)Math.Max(0, Math.Min((int)AnimalLineage.Legendary, lineage)),
                Xp = Math.Max(0, xp),
                LegendaryDescent = legendaryDescent,
                LegendaryTitle = string.IsNullOrEmpty(legendTitle) ? null : legendTitle,
                Stamina = stamina < 0f ? HorseAbilities.MaxStamina : Math.Min(HorseAbilities.MaxStamina, stamina),
                ShyDays = Math.Max(0, shyDays)
            };
            a.FatherId = Math.Max(0, fatherId);
            a.PregnancySireId = a.IsPregnant ? Math.Max(0, sireId) : 0;
            a.PregnancySireSpecies = a.IsPregnant ? sireSpecies : null;
            a.PregnancySireGenome = a.IsPregnant && sireSpecies != null ? Genetics.Genome.Import(sireSpecies, sireGenome) : null;
            if (species.HasGenetics)
            {
                a.Genome = Genetics.Genome.Import(species.Id, genome);
                if (a.Genome == null)
                {
                    // Eski kayıt: kimlikten türetilen sabit genom (yüklemede değişmez). Tip türün ilk tipi (beygir, çoban köpeği,
                    // ambar kedisi), istatistikler tipin tabanı: eski atın hızı değişmez, görünüş rastgele.
                    var rng = new Random(id * 7919 + 17);
                    var legacyType = Genetics.GeneticsCatalog.DefaultType(species.Id);
                    a.Genome = Genetics.GeneticsService.Random(species.Id, legacyType?.Id, a.Lineage, a.IsFemale, rng);
                    if (legacyType != null)
                        foreach (var kv in legacyType.Stats) a.Genome.Stats[kv.Key] = Math.Min(Genetics.GeneStat.Cap(a.Lineage), kv.Value);
                }
            }
            if (deeds != null)
                foreach (var d in deeds)
                {
                    int i = d?.LastIndexOf(':') ?? -1;
                    if (i > 0 && int.TryParse(d.Substring(i + 1), out int n) && n > 0) a.Deeds[d.Substring(0, i)] = n;
                }
            _animals.Add(a);
            if (!_pedigree.ContainsKey(id)) RecordPedigree(a);
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
                if (a.AgeDays >= a.LifespanDays) { Kill(a, AnimalDeath.OldAge); continue; }

                // Binek: binicinin altında gider (binici yoksa iner).
                bool ridden = false;
                if (a.RiderId != 0)
                {
                    var rider = RiderPosition?.Invoke(a.RiderId);
                    if (rider.HasValue && CanCarryTo(home, rider.Value))
                    {
                        if (a.Position != rider.Value)
                        {
                            _world.Map.Trample(rider.Value, a.Species.Trample);   // at yolu aşındırır
                            int dist = Math.Max(Math.Abs(a.Position.X - rider.Value.X), Math.Abs(a.Position.Y - rider.Value.Y));
                            var riderChar = FindCharacter?.Invoke(a.RiderId);
                            riderChar?.GainSkillXp(SkillType.Riding, RideXpPerTile * Math.Min(dist, 4));
                            // Atın kendi XP'si: iyi binici atını daha hızlı eğitir (Binicilik seviye başına %5).
                            double trainer = riderChar != null ? SkillNames.Factor(riderChar.Skills.GetLevel(SkillType.Riding), 0.05) : 1.0;
                            AnimalProgress.AddXp(a, AnimalProgress.XpPerRideTile * Math.Min(dist, 4) * trainer);
                        }
                        a.Position = rider.Value; a.NextTile = null; a.Path = null; ridden = true;
                    }
                    else
                    {
                        // Binici eve (ya da üst kata) girdi: at kapıda kalır, binici iner.
                        int riderId = a.RiderId;
                        a.RiderId = 0;
                        a.Path = null;
                        if (rider.HasValue) Dismounted?.Invoke(a, riderId);
                    }
                }

                HorseAbilities.Tick(a, gameDays, ridden, ridden ? FindCharacter?.Invoke(a.RiderId) : null);
                if (!ridden) a.Galloping = false;

                // Beslenme: önce otlaktaki ot, sonra barınağın yemi, sonra aynı odadaki yemlikler.
                double need = a.Species.FeedPerDay * gameDays * WinterFeedFactor(a) * GeneticsService.FeedFactor(a.Genome);   // "Kötü diş": +%15 yem
                double eaten = Graze(a, need);
                if (need - eaten > 0 && home != null)
                {
                    double take = Math.Min(need - eaten, home.FeedStock);
                    home.FeedStock -= take;
                    eaten += take;
                }
                if (need - eaten > 0 && home != null)
                    foreach (var trough in TroughsFor(home))
                    {
                        double take = Math.Min(need - eaten, trough.FeedStock);
                        trough.FeedStock -= take;
                        eaten += take;
                        if (need - eaten <= 1e-12) break;
                    }
                double fed = need > 0 ? Math.Min(1.0, eaten / need) : 1.0;
                a.Fullness = (float)Math.Max(0, Math.Min(100, a.Fullness + (fed * 2 - 1) * FullnessChangePerDay * gameDays));

                // Dostluk: iyi bakılınca yavaş yavaş artar, aç bırakılınca düşer.
                if (a.PetCooldownDays > 0) a.PetCooldownDays = Math.Max(0, a.PetCooldownDays - gameDays);
                // Konforlu barınakta dostluk daha çabuk artar (konfor 50 = eski hız).
                if (a.Fullness >= 60f && !a.IsCold) a.Friendship = Math.Min(100f, a.Friendship + (float)(FriendshipPerDay * gameDays * (0.5 + a.Comfort / 100.0)));
                else if (a.Fullness < 20f) a.Friendship = Math.Max(0f, a.Friendship - (float)(HungerFriendLoss * gameDays));
                if (a.IsCold) a.Friendship = Math.Max(0f, a.Friendship - (float)(ColdFriendLoss * gameDays));

                if (a.Fullness <= 0f) a.HungryDays += gameDays;
                else a.HungryDays = Math.Max(0, a.HungryDays - gameDays);
                if (a.HungryDays >= StarveDeathDays) { Kill(a, AnimalDeath.Starvation); continue; }

                // Ürün: tok ve erişkinken barınağa birikir.
                if (a.Species.Produces != null && home != null && a.IsMature && a.Fullness >= ProduceFullness && !a.IsSick && (a.IsFemale || !a.Species.FemaleProduces))
                {
                    // Dostluk ve barınak konforu ürünü hızlandırır (konfor 0: x0,85, 50: x1, 100: x1,15).
                    float seasonal = SeasonNow != null ? a.Species.OutputFactor(SeasonNow()) : 1f;
                    a.ProduceProgress += gameDays / a.Species.ProduceEveryDays * seasonal * (1.0 + FriendshipBoost * a.Friendship / 100.0) * (0.85 + 0.3 * a.Comfort / 100.0) * (a.IsCold ? ColdOutputFactor : 1.0) * (a.IsHeritage ? HeritageOutput : 1.0);
                    while (a.ProduceProgress >= 1.0)
                    {
                        a.ProduceProgress -= 1.0;
                        int cap = StockPerResident * Math.Max(1, home.Def.HousingCapacity);
                        if (home.StockTotal >= cap) { a.ProduceProgress = 0; break; }
                        int units = 1;
                        if (a.Friendship >= BondedFriendship && _rng.NextDouble() < BondedExtraChance) units++;
                        // Kalite (53): hayvanın puanı ürünün kalitesi olur (birim başına %70 / %20 / %10 dağılım);
                        // fazladan adet yarıya indi (Seçme %10, Has %20). Barınak yığını kaliteyi "kimlik#kalite" anahtarıyla tutar.
                        bool graded = _content.Items.TryGet(a.Species.Produces, out var pdef) && pdef.HasQuality;
                        int q = graded ? ItemQuality.Spread(QualityOf(a), _rng) : 0;
                        double extra = QualityYield(QualityOf(a)) - 1.0;
                        if (extra > 0 && _rng.NextDouble() < extra) units++;
                        home.AddStock(ItemKey.StockKey(a.Species.Produces, q), units);
                        AnimalProgress.AddXp(a, AnimalProgress.XpProduce);
                        Produced?.Invoke(a, a.Species.Produces);
                    }
                }

                if (!ridden) Wander(a, home, gameDays);
            }
            _allowedCache = null;
            _roadCache = null;
        }

        /// <summary>Yırtıcı (kurt) öldürdü.</summary>
        public void KillByPredator(Animal a) { if (a != null && !a.IsDead) Kill(a, AnimalDeath.Predator); }

        private void Kill(Animal a, AnimalDeath cause)
        {
            a.IsDead = true;
            a.CauseOfDeath = cause;
            a.NextTile = null;
            _animals.Remove(a);
            Died?.Invoke(a);
        }

        /// <summary>
        /// Hayvanı bir karoya yürütür ve varınca <paramref name="waitDays"/> kadar orada tutar (easter egg sahneleri:
        /// öküz yeni yapılana bakar, kedi sandığa gider, köpek ulurken durur). Binili hayvan gitmez; yol yoksa false.
        /// </summary>
        public bool SendTo(Animal a, GridPos target, double waitDays)
        {
            if (a == null || a.IsDead || a.RiderId != 0) return false;
            var from = a.NextTile ?? a.Position;
            if (from == target)
            {
                a.Path = null; a.Target = target; a.WaitDays = waitDays; a.SceneWaitDays = waitDays;
                return true;
            }
            var path = Pathfinder.FindPath(_world.Map, from, target);
            if (path == null || path.Count == 0) return false;
            if (path[0] == from) path.RemoveAt(0);
            a.Path = path; a.Target = target; a.WaitDays = 0; a.SceneWaitDays = waitDays;
            return true;
        }

        // Yaşam alanında (barınak odası ya da barınak çevresi; gündüz ve salınmışken otlak) rastgele bir karoya yol
        // bulup adım adım yürür, varınca bir süre durur. Eve girmez: yaşam alanı dışına yalnızca eve dönerken çıkar.
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
                    if (a.Path == null || a.Path.Count == 0) PlanWalk(a, home);
                    if (a.Path == null || a.Path.Count == 0)
                    {
                        a.Path = null;
                        a.Target = a.Position;
                        a.WaitDays = MinWaitDays + _rng.NextDouble() * (MaxWaitDays - MinWaitDays);
                        continue;
                    }
                    var next = a.Path[0];
                    a.Path.RemoveAt(0);
                    if (!_world.Map.CanStep(a.Position, next)) { a.Path = null; continue; }   // yol kapandı
                    a.NextTile = next;
                    a.StepProgress = 0f;
                }

                double stepDays = StepDays / _world.Map.StepSpeed(a.Position, a.NextTile.Value);   // yolda daha hızlı
                double needed = (1.0 - a.StepProgress) * stepDays;
                if (budget >= needed)
                {
                    budget -= needed;
                    a.Position = a.NextTile.Value;
                    a.NextTile = null;
                    a.StepProgress = 0f;
                    _world.Map.Trample(a.Position, a.Species.Trample);
                    if (a.Path == null || a.Path.Count == 0) { a.Path = null; a.WaitDays = a.SceneWaitDays > 0 ? a.SceneWaitDays : MinWaitDays + _rng.NextDouble() * (MaxWaitDays - MinWaitDays); a.SceneWaitDays = 0; }
                }
                else
                {
                    a.StepProgress += (float)(budget / stepDays);
                    budget = 0;
                }
            }
        }

        /// <summary>Barınaktaki, bugün henüz sevilmemiş hayvan sayısı.</summary>
        public int PettableResidents(int homeItemId)
        {
            int n = 0;
            foreach (var a in _animals) if (a.HomeItemId == homeItemId && a.PetCooldownDays <= 0) n++;
            return n;
        }

        /// <summary>
        /// Barınaktaki hayvanları sever: dostluk artar, günlük bekleme başlar. Sevilen sayı döner.
        /// husbandryLevel: sevenin hayvancılık seviyesi (seviye başına %5 daha çok dostluk).
        /// </summary>
        public int PetResidents(int homeItemId, int husbandryLevel = 1)
        {
            int n = 0;
            foreach (var a in _animals)
            {
                if (a.HomeItemId != homeItemId || a.PetCooldownDays > 0) continue;
                a.Friendship = Math.Min(100f, a.Friendship + PetFriendship * HusbandryFactor(husbandryLevel));
                a.PetCooldownDays = 1.0;
                n++;
            }
            return n;
        }

        /// <summary>Hayvancılık seviyesinin sevgi çarpanı: 1. seviye 1, 10. seviye 1,45.</summary>
        public static float HusbandryFactor(int level) => 1f + 0.05f * Math.Max(0, level - 1);

        /// <summary>Merhem: barınaktaki hayvanlara dostluk (günlük sevme hakkını kullanmaz) ve hastaları iyileştirir. Etkilenen sayı.</summary>
        public int Befriend(int homeItemId, float amount) => Befriend(homeItemId, amount, out _);

        /// <summary>Merhem: dostluk ve konfor; hasta hayvanlar iyileşir (cured: iyileşen sayısı).</summary>
        public int Befriend(int homeItemId, float amount, out int cured)
        {
            int n = 0;
            cured = 0;
            foreach (var a in _animals)
            {
                if (a.IsDead || a.HomeItemId != homeItemId) continue;
                a.Friendship = Math.Min(100f, a.Friendship + amount);
                a.Comfort = Math.Min(100f, a.Comfort + 5f);
                if (a.IsSick) { Cure(a); cured++; }
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
