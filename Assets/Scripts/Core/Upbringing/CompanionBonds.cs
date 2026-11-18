using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>Birlikte büyünen hayvana göre açılan yetenek (belge 31, bölüm 8.2).</summary>
    public enum CompanionTrait
    {
        None = 0,
        BornRider = 1,      // Doğuştan Binici (at)
        PackTongue = 2,     // Sürü Dili (köpek)
        SilentStep = 3,     // Sessiz Adım (kedi)
        ShepherdSoul = 4,   // Çoban Ruhu (koyun)
        MountainFoot = 5,   // Dağ Ayağı (keçi)
        PatientHand = 6,    // Sabırlı El (inek)
        CoopChild = 7,      // Kümes Çocuğu (tavuk, ördek)
        ForestFriend = 8    // Orman Dostu (sahiplenilmiş yaban yavrusu)
    }

    public sealed class CompanionBond
    {
        public int ChildId;
        public int AnimalId;
        public string SpeciesId;
        public string AnimalName;
        public double Points;
        public double FirstContactDay;
        public double LastContactDay;
        public int ParentAssists;
        /// <summary>Ergenliğe girişte açıldı mı (en güçlü bağ).</summary>
        public bool Unlocked;
        /// <summary>İkinci güçlü bağ: yarım etkili "dost".</summary>
        public bool Friend;
        /// <summary>Hayvan öldü: yetenek kalır, "anı" olarak etiketlenir.</summary>
        public bool Memory;
        public double Span => LastContactDay - FirstContactDay;
    }

    /// <summary>
    /// Çocuk–hayvan iletişimi ve "Can Yoldaşı" bağı (belge 31, bölüm 8; onay 18.11.2026).
    /// Bebeklik ve çocuklukta düzenli temas gizli bağ puanı biriktirir; ergenliğe girişte eşik ve süre tutuyorsa
    /// en güçlü bağın türüne göre yetenek açılır. Ebeveynin yardımı (ata bindirmek, sağıma götürmek) puanı katlar.
    /// </summary>
    public sealed class CompanionBondSystem
    {
        public const double Threshold = 100;
        /// <summary>Bağın en az sürmesi gereken gün (≈ 3 yıl).</summary>
        public const double MinSpanDays = 6;
        /// <summary>Yakında bulunma örneği: aynı anda bu mesafede (karo) olmak temas sayılır.</summary>
        public const int NearDistance = 3;
        public const double NearPerSample = 0.5, SampleStepDays = 0.1;
        public const double PetPoints = 3, BrushPoints = 4, RidePoints = 3, ParentAssistPoints = 8;
        /// <summary>Can yoldaşının yavrusuyla ilk temas eşiğin yarısıyla başlar.</summary>
        public const double LineageStart = 0.5;
        /// <summary>Yetenek beceri XP çarpanı; "dost" yarısı.</summary>
        public const double TraitXpBonus = 0.15;

        private readonly Population _population;
        /// <summary>Hayvan sistemi (yakında bulunma örneklemesi için; sonradan bağlanabilir).</summary>
        public AnimalSystem Animals { get => _animals; set => _animals = value; }
        private AnimalSystem _animals;
        private readonly List<CompanionBond> _bonds = new List<CompanionBond>();
        private double _sampleAcc;

        public CompanionBondSystem(Population population, AnimalSystem animals)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _animals = animals;
        }

        public Func<double> Today { get; set; } = () => 0;

        /// <summary>(çocuk, bağ, yetenek)</summary>
        public event Action<Character, CompanionBond, CompanionTrait> TraitUnlocked;
        /// <summary>Can yoldaşı öldü (yas için).</summary>
        public event Action<Character, CompanionBond> CompanionLost;

        public IReadOnlyList<CompanionBond> Bonds => _bonds;

        public CompanionBond Get(int childId, int animalId)
        {
            foreach (var b in _bonds) if (b.ChildId == childId && b.AnimalId == animalId) return b;
            return null;
        }

        public static bool IsYoung(Character c) => c != null && !c.IsDead && c.Stage <= AgeStage.Child;

        public static CompanionTrait TraitFor(string speciesId)
        {
            switch (speciesId)
            {
                case "animal.horse": return CompanionTrait.BornRider;
                case "animal.dog": return CompanionTrait.PackTongue;
                case "animal.cat": return CompanionTrait.SilentStep;
                case "animal.sheep": return CompanionTrait.ShepherdSoul;
                case "animal.goat": return CompanionTrait.MountainFoot;
                case "animal.cow": return CompanionTrait.PatientHand;
                case "animal.chicken":
                case "animal.duck": return CompanionTrait.CoopChild;
                case "animal.wild_young": return CompanionTrait.ForestFriend;
                default: return CompanionTrait.None;
            }
        }

        public static string TraitName(CompanionTrait t)
        {
            switch (t)
            {
                case CompanionTrait.BornRider: return "Doğuştan Binici";
                case CompanionTrait.PackTongue: return "Sürü Dili";
                case CompanionTrait.SilentStep: return "Sessiz Adım";
                case CompanionTrait.ShepherdSoul: return "Çoban Ruhu";
                case CompanionTrait.MountainFoot: return "Dağ Ayağı";
                case CompanionTrait.PatientHand: return "Sabırlı El";
                case CompanionTrait.CoopChild: return "Kümes Çocuğu";
                case CompanionTrait.ForestFriend: return "Orman Dostu";
                default: return "";
            }
        }

        /// <summary>Yeteneğin beceri bağı (XP çarpanı buna uygulanır).</summary>
        public static SkillType TraitSkill(CompanionTrait t)
        {
            switch (t)
            {
                case CompanionTrait.BornRider: return SkillType.Riding;
                case CompanionTrait.SilentStep: return SkillType.Hunting;
                case CompanionTrait.MountainFoot: return SkillType.Mining;
                case CompanionTrait.ForestFriend: return SkillType.Foraging;
                default: return SkillType.AnimalHusbandry;
            }
        }

        /// <summary>
        /// Temas kaydı (kaynaktan bağımsız: sevme, tımar, binme, yakında bulunma, oyun katmanının özel etkileşimleri).
        /// Yalnızca bebek ve çocuk bağ kurar.
        /// </summary>
        public CompanionBond AddContact(Character child, Animal animal, double points)
        {
            if (!IsYoung(child) || animal == null || animal.IsDead || points <= 0) return null;
            double today = Today();
            var b = Get(child.Id, animal.Id);
            if (b == null)
            {
                b = new CompanionBond
                {
                    ChildId = child.Id, AnimalId = animal.Id, SpeciesId = animal.Species.Id, AnimalName = animal.Name,
                    FirstContactDay = today, LastContactDay = today, Points = LineagePoints(child, animal)
                };
                _bonds.Add(b);
            }
            b.Points += points;
            b.LastContactDay = today;
            return b;
        }

        /// <summary>Ebeveynin yardımı: çocuğu ata bindirmek, sağıma ya da çobanlığa yanında götürmek (puan + sayaç).</summary>
        public CompanionBond ParentAssist(Character parent, Character child, Animal animal)
        {
            if (parent == null || parent.IsDead || parent.Stage < AgeStage.YoungAdult) return null;
            var b = AddContact(child, animal, ParentAssistPoints);
            if (b == null) return null;
            b.ParentAssists++;
            parent.GainSkillXp(SkillType.Parenting, 3);
            child.AddMoodlet("parent_animal", $"{parent.Name} ile {animal.Name}'e baktı", 2, 0.3);
            return b;
        }

        /// <summary>Hayvan bakımı yapıldı (AnimalCare.Done'a bağlanır).</summary>
        public void OnCare(Character actor, Animal animal, AnimalAction action)
        {
            double p = action == AnimalAction.Pet ? PetPoints : action == AnimalAction.Brush ? BrushPoints
                : action == AnimalAction.Mount ? RidePoints : 0;
            if (p > 0) AddContact(actor, animal, p);
        }

        /// <summary>
        /// Zaman ilerler: belli aralıklarla yakın duran bebek/çocuk ile hayvan temas sayılır
        /// (kedi beşiğin yanında uyur, köpek beşik başında bekler).
        /// </summary>
        public void Tick(double gameDays)
        {
            if (_animals == null) return;
            _sampleAcc += gameDays;
            if (_sampleAcc < SampleStepDays) return;
            _sampleAcc = 0;
            foreach (var c in _population.Members)
            {
                if (!IsYoung(c) || c.IsVillager) continue;
                foreach (var a in _animals.Animals)
                {
                    if (a.IsDead) continue;
                    int d = Math.Abs(a.Position.X - c.Position.X) + Math.Abs(a.Position.Y - c.Position.Y);
                    if (d <= NearDistance && a.Position.Z == c.Position.Z) AddContact(c, a, NearPerSample);
                }
            }
        }

        /// <summary>
        /// Ergenliğe giriş: eşiği ve süreyi tutan en güçlü bağ yetenek açar; ikincisi "dost" etiketi alır.
        /// Açılan bağı döner (yoksa null).
        /// </summary>
        public CompanionBond OnBecameTeen(Character c)
        {
            if (c == null) return null;
            CompanionBond best = null, second = null;
            foreach (var b in _bonds)
            {
                if (b.ChildId != c.Id || b.Unlocked || b.Points < Threshold || b.Span < MinSpanDays) continue;
                if (TraitFor(b.SpeciesId) == CompanionTrait.None) continue;
                if (best == null || b.Points > best.Points) { second = best; best = b; }
                else if (second == null || b.Points > second.Points) second = b;
            }
            if (best == null) return null;
            best.Unlocked = true;
            if (second != null && TraitFor(second.SpeciesId) != TraitFor(best.SpeciesId)) second.Friend = true;
            TraitUnlocked?.Invoke(c, best, TraitFor(best.SpeciesId));
            return best;
        }

        public CompanionTrait TraitOf(int characterId)
        {
            foreach (var b in _bonds) if (b.ChildId == characterId && b.Unlocked) return TraitFor(b.SpeciesId);
            return CompanionTrait.None;
        }

        public bool Has(int characterId, CompanionTrait t)
        {
            foreach (var b in _bonds)
                if (b.ChildId == characterId && (b.Unlocked || b.Friend) && TraitFor(b.SpeciesId) == t) return true;
            return false;
        }

        /// <summary>Yeteneklerin beceri XP çarpanı (tam yetenek %15, dost %7,5).</summary>
        public double XpFactor(int characterId, SkillType skill)
        {
            double f = 1.0;
            foreach (var b in _bonds)
            {
                if (b.ChildId != characterId || !(b.Unlocked || b.Friend)) continue;
                var t = TraitFor(b.SpeciesId);
                if (t != CompanionTrait.None && TraitSkill(t) == skill) f += b.Unlocked ? TraitXpBonus : TraitXpBonus / 2;
            }
            return f;
        }

        /// <summary>Sürü Dili: kurt savunmasında hanedeki her köpeğe +1 (belge 31, 4.1). Yeteneğe sahip yaşayan kişi varsa 1.</summary>
        public int WolfDefenseBonus()
        {
            foreach (var c in _population.Members)
                if (!c.IsDead && !c.IsVillager && Has(c.Id, CompanionTrait.PackTongue)) return 1;
            return 0;
        }

        /// <summary>Doğuştan Binici: binerken at hızı çarpanı.</summary>
        public double RideSpeedFactor(int riderId) => Has(riderId, CompanionTrait.BornRider) ? 1.1 : 1.0;

        /// <summary>Sabırlı El ya da Çoban Ruhu sahibi hanedeyse ürün kalitesi puanına eklenecek değer.</summary>
        public float QualityBonus(string speciesId)
        {
            var need = speciesId == "animal.cow" ? CompanionTrait.PatientHand
                : speciesId == "animal.sheep" ? CompanionTrait.ShepherdSoul
                : speciesId == "animal.chicken" || speciesId == "animal.duck" ? CompanionTrait.CoopChild : CompanionTrait.None;
            if (need == CompanionTrait.None) return 0;
            foreach (var c in _population.Members)
                if (!c.IsDead && !c.IsVillager && Has(c.Id, need)) return 5f;
            return 0;
        }

        /// <summary>Hayvan öldü: bağlar anıya döner; açılmış bağın sahibine uzun yas.</summary>
        public void OnAnimalDied(Animal animal)
        {
            if (animal == null) return;
            foreach (var b in _bonds)
            {
                if (b.AnimalId != animal.Id || b.Memory) continue;
                b.Memory = true;
                var c = _population.Find(b.ChildId);
                if (c == null || c.IsDead) continue;
                if (b.Unlocked || b.Points >= Threshold / 2)
                {
                    c.AddMoodlet("companion_grief", $"Can yoldaşı {b.AnimalName} öldü", -4, 4);
                    CompanionLost?.Invoke(c, b);
                }
            }
        }

        /// <summary>Hayvan karakteri ölünce bağları silinmez (kronik); yalnız yaşayanlar sorgulanır.</summary>
        private double LineagePoints(Character child, Animal animal)
        {
            if (animal.MotherId == 0) return 0;
            foreach (var parentId in new[] { child.BirthMotherId, child.BirthFatherId })
            {
                if (!parentId.HasValue) continue;
                foreach (var b in _bonds)
                    if (b.ChildId == parentId.Value && b.Unlocked && b.AnimalId == animal.MotherId) return Threshold * LineageStart;
            }
            return 0;
        }

        public string ChronicleLine(Character c, CompanionBond b) =>
            $"{c.Name} ve {b.AnimalName} birlikte büyüdü ({TraitName(TraitFor(b.SpeciesId))}).";

        // ---- kayıt ----

        public CompanionBondSave[] Export()
        {
            var list = new List<CompanionBondSave>();
            foreach (var b in _bonds)
                list.Add(new CompanionBondSave
                {
                    childId = b.ChildId, animalId = b.AnimalId, species = b.SpeciesId, animalName = b.AnimalName, points = b.Points,
                    firstDay = b.FirstContactDay, lastDay = b.LastContactDay, assists = b.ParentAssists,
                    unlocked = b.Unlocked, friend = b.Friend, memory = b.Memory
                });
            return list.ToArray();
        }

        public void Import(CompanionBondSave[] rows)
        {
            _bonds.Clear();
            if (rows == null) return;
            foreach (var r in rows)
                if (r != null)
                    _bonds.Add(new CompanionBond
                    {
                        ChildId = r.childId, AnimalId = r.animalId, SpeciesId = r.species, AnimalName = r.animalName, Points = r.points,
                        FirstContactDay = r.firstDay, LastContactDay = r.lastDay, ParentAssists = r.assists,
                        Unlocked = r.unlocked, Friend = r.friend, Memory = r.memory
                    });
        }
    }

    [Serializable]
    public sealed class CompanionBondSave
    {
        public int childId, animalId, assists;
        public string species, animalName;
        public double points, firstDay, lastDay;
        public bool unlocked, friend, memory;
    }
}
