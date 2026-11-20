using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>
    /// Çocukluk ve nesil aktarımı sistemlerini bir arada tutar ve hayata bağlar (belge 30, 31-8, 34-D1/D3/D4):
    /// oyuncak alışkanlığı, can yoldaşı bağı, çıraklık, aile tarifleri ve itibar.
    /// Karakterin beceri XP'sine bu sistemlerin çarpanı Character.ExtraXpFactor ile eklenir.
    /// </summary>
    public sealed class UpbringingSystems
    {
        /// <summary>Usta yanında oynamak: öğretenin en çok bu kadar karo uzakta olması gerekir.</summary>
        public const int TeachDistance = 3;

        public Population Population { get; }
        public ToyHabitSystem Toys { get; } = new ToyHabitSystem();
        public CompanionBondSystem Companions { get; }
        public ApprenticeshipSystem Apprentices { get; }
        public FamilyRecipeBook Recipes { get; }
        public ReputationSystem Reputation { get; } = new ReputationSystem();
        /// <summary>Yaşlı rolleri: masal, torun bakımı, hafif iş hızı (belge 34-D4).</summary>
        public ElderRoles Elders { get; }
        /// <summary>Şenlik yarışları (atletizm).</summary>
        public FestivalGames Games { get; private set; } = new FestivalGames();
        /// <summary>Düğün ve talip niteliği (belge 34-D2); AttachHouse ile kurulur.</summary>
        public WeddingSystem Weddings { get; private set; }

        public void AttachHouse(Thatchwick.Core.Build.Household house)
        {
            Games = new FestivalGames(_rng.Next()) { House = house, Reputation = Reputation };
            Weddings = new WeddingSystem(house, Population)
            {
                Reputation = Reputation,
                IsPerformer = id => Toys.IsMastered(id, "item.wooden_whistle") || Toys.IsMastered(id, "item.juggling_balls"),
                Notify = (t, id) => Say(t, id),
                Chronicle = t => Chronicle?.Invoke(t)
            };
        }

        public Func<double> Today { get; set; } = () => 0;
        /// <summary>Bildirim (metin, karakter kimliği; 0 = genel).</summary>
        public Action<string, int> Notify { get; set; }
        /// <summary>Kronik/hayat notu (Ledger ya da hanedan defteri).</summary>
        public Action<string> Chronicle { get; set; }

        private readonly Random _rng;

        public UpbringingSystems(Population population, int seed = 0)
        {
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Apprentices = new ApprenticeshipSystem(population);
            Recipes = new FamilyRecipeBook(population);
            Elders = new ElderRoles(population);
            Companions = new CompanionBondSystem(population, null) { Today = () => Today() };
            _rng = new Random(seed != 0 ? seed : 113);

            Toys.Mastered += (c, d) => Say($"{c.Name} {ToyNames.Name(d.ItemId)} oyununda ustalaştı; artık kendi {ToyNames.Name(d.ItemId)}ı var.", c.Id);
            Toys.Broke += (c, p) => Say($"{c.Name}'in {ToyNames.Name(p.ItemId)}ı kırıldı; onarılabilir.", c.Id);
            Toys.MarkGained += (c, d) => { Say($"{c.Name} çocukluğundan bir iz taşıyor: {d.MarkName}.", c.Id); Chronicle?.Invoke($"{c.Name}: {d.MarkName} (çocukluk {ToyNames.Name(d.ItemId)}ı)"); };
            WireCompanions();
            Apprentices.Graduated += (m, a, s) =>
            {
                Say($"{a.Name}, {m.Name}'in yanında {SkillNames.TradeName(s).ToLowerInvariant()} ustası oldu.", a.Id);
                Chronicle?.Invoke($"{a.Name} çıraklığı bitirdi: usta {SkillNames.TradeName(s).ToLowerInvariant()} ({m.Name}'in çırağı)");
                Reputation.Add(ReputationSystem.Reasons.Graduation, "çırak yetiştirdi");
            };
            Recipes.Founded += (c, r) =>
            {
                Say($"Aile sırrı doğdu: {r.Name}. Öğretilmezse ya da yazılmazsa {c.Name} ile birlikte gider.", c.Id);
                Chronicle?.Invoke($"Aile tarifi: {r.Name}");
                Reputation.Add(ReputationSystem.Reasons.FamilyRecipe, r.Name);
            };
            Recipes.LostForever += r =>
            {
                Say($"{r.Name} kayboldu: bilen kimse kalmadı.", 0);
                Chronicle?.Invoke($"Kaybolan tarif: {r.Name}");
                Reputation.Add(ReputationSystem.Reasons.RecipeLost, r.Name);
            };

            population.MemberAdded += Watch;
            population.MemberDied += OnDied;
            foreach (var m in population.Members) Watch(m);
        }

        private void WireCompanions()
        {
            Companions.TraitUnlocked += (c, b, t) =>
            {
                Say($"{c.Name} ile {b.AnimalName} birlikte büyüdü: {CompanionBondSystem.TraitName(t)}.", c.Id);
                Chronicle?.Invoke(Companions.ChronicleLine(c, b));
                Reputation.Add(ReputationSystem.Reasons.CompanionTrait, "can yoldaşı");
            };
            Companions.CompanionLost += (c, b) => Say($"{c.Name} can yoldaşı {b.AnimalName}'i kaybetti.", c.Id);
        }

        private void Say(string text, int id) => Notify?.Invoke(text, id);

        // ---- bağlantılar ----

        /// <summary>Hayvanlar: yakında bulunma örneklemesi, bakım, ölüm, kalite ve binicilik hızı.</summary>
        public void Attach(AnimalSystem animals, AnimalCare care = null)
        {
            Companions.Animals = animals;
            if (animals == null) return;
            animals.Died += Companions.OnAnimalDied;
            animals.QualityBonus = a => Companions.QualityBonus(a.Species.Id);
            animals.RiderSpeedFactor = Companions.RideSpeedFactor;
            if (care != null) care.Done += Companions.OnCare;
        }

        /// <summary>İş bitti: oyuncak eşyasıysa alışkanlık işler (yakında usta varsa öğretme sayılır).</summary>
        public void OnItemUsed(Character c, ItemInstance item)
        {
            if (c == null || item?.Def == null || !Toys.IsToy(item.Def.Id)) return;
            Toys.OnPlayed(c, item.Def.Id, Today(), TeacherNear(c, item.Def.Id));
        }

        /// <summary>Yakında o oyuncakta ustalaşmış genç yetişkin ya da daha büyük hane üyesi (öğreten).</summary>
        public Character TeacherNear(Character c, string itemId)
        {
            Character best = null;
            foreach (var m in Population.Members)
            {
                if (m.IsDead || m.Id == c.Id || m.Stage < AgeStage.YoungAdult || !Toys.IsMastered(m.Id, itemId)) continue;
                if (Distance(m.Position, c.Position) > TeachDistance) continue;
                if (best == null || m.Stage == AgeStage.Elder) best = m;
            }
            return best;
        }

        /// <summary>
        /// Boşta gösterisi: karakter beklerken çağrılır. Yakındaki karakterlere etkisi uygulanır.
        /// Gösteri yoksa null (oyun katmanı normal boşta animasyonunu oynatır).
        /// </summary>
        /// <summary>Oyuncak gösterisi yapıldı (topaç şovu easter egg'i; belge 55 §1).</summary>
        public event Action<ToyShow> Showed;

        public ToyShow IdleShow(Character c, IEnumerable<Character> nearby = null)
        {
            var near = new List<Character>();
            if (nearby != null) near.AddRange(nearby);
            else foreach (var m in Population.Members) if (m.Id != c.Id && !m.IsDead && Distance(m.Position, c.Position) <= TeachDistance) near.Add(m);
            bool childNear = false;
            foreach (var m in near) if (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) childNear = true;
            var show = Toys.PickIdleShow(c, _rng, childNear);
            if (show == null)
            {
                // Belge 45 ortak basamak Sv 7: oyuncağı olmayan usta becerisini sergiler.
                var skill = Progress.Mastery.ShowcaseSkill(c);
                if (skill == null) return null;
                var sk = new ToyShow { Performer = c, Kind = ShowKind.Show, Text = Progress.Mastery.ShowText(c, skill.Value) };
                sk.Watchers = Progress.Mastery.ApplyShowcase(c, skill.Value, near);
                return sk;
            }
            Toys.ApplyShow(show, near, Today());
            Showed?.Invoke(show);
            return show;
        }

        // ---- arayüz eylemleri (aile paneli) ----

        /// <summary>Ustanın çırağa öğretebileceği en iyi beceri (seviye 5+, çırak o beceride usta değil); yoksa null.</summary>
        public SkillType? TeachableSkill(Character master, Character apprentice)
        {
            if (master == null || apprentice == null) return null;
            SkillType? best = null; int bestLevel = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                int lv = master.Skills.GetLevel(s);
                if (lv < ApprenticeshipSystem.MasterLevel || lv <= bestLevel) continue;
                if (Apprentices.Blocker(master, apprentice, s) != null) continue;
                best = s; bestLevel = lv;
            }
            return best;
        }

        /// <summary>Tarifi deftere yazar; hanedeki boş defteri harcar. Olmazsa neden.</summary>
        public string WriteRecipe(Character writer, FamilyRecipe r, Thatchwick.Core.Build.Household house)
        {
            bool hasBook = house != null && house.Count(FamilyRecipeBook.BlankBookItem) > 0;
            string why = Recipes.WriteBlocker(writer, r, hasBook);
            if (why != null) return why;
            if (!house.TryTakeItem(FamilyRecipeBook.BlankBookItem)) return "Boş defter gerekir.";
            Recipes.Write(writer, r, true);
            Say($"{writer.Name}, {r.Name} tarifini deftere yazdı; artık kaybolmaz.", writer.Id);
            return null;
        }

        /// <summary>Kırık kişisel oyuncağı onarır: bir tahta harcar. Olmazsa neden.</summary>
        public string RepairToy(Character owner, string itemId, Thatchwick.Core.Build.Household house)
        {
            var p = Toys.Personal(owner?.Id ?? 0, itemId);
            if (p == null) return "Böyle bir eşyası yok.";
            if (p.Durability >= 100) return "Sağlam.";
            if (house == null || !house.TryTakeItem("item.plank")) return "Onarmak için 1 tahta gerekir.";
            Toys.Repair(owner.Id, itemId);
            return null;
        }

        private readonly Dictionary<int, long> _assistDay = new Dictionary<int, long>();

        /// <summary>Ebeveynin çocukla bakacağı hayvan: çocuğun en güçlü bağı, yoksa ilk sağ hayvan.</summary>
        public Animal AssistTarget(Character child)
        {
            var animals = Companions.Animals;
            if (animals == null || child == null) return null;
            Animal best = null; double bestPts = -1;
            foreach (var a in animals.Animals)
            {
                if (a.IsDead) continue;
                double pts = Companions.Get(child.Id, a.Id)?.Points ?? 0;
                if (pts > bestPts) { best = a; bestPts = pts; }
            }
            return best;
        }

        /// <summary>"Çocukla hayvana bak" (ata bindir, sağıma götür): günde bir kez çocuk başına. Olmazsa neden.</summary>
        public string ParentAssistBlocker(Character parent, Character child)
        {
            if (parent == null || child == null) return "Kişi yok.";
            if (parent.Stage < AgeStage.YoungAdult) return "Yetişkin götürebilir.";
            if (!CompanionBondSystem.IsYoung(child)) return "Yalnızca bebek ve çocuk.";
            if (AssistTarget(child) == null) return "Hayvan yok.";
            if (_assistDay.TryGetValue(child.Id, out var d) && d == (long)Math.Floor(Today())) return "Bugün zaten götürüldü.";
            return null;
        }

        public string ParentAssist(Character parent, Character child)
        {
            string why = ParentAssistBlocker(parent, child);
            if (why != null) return why;
            var a = AssistTarget(child);
            Companions.ParentAssist(parent, child, a);
            _assistDay[child.Id] = (long)Math.Floor(Today());
            string verb = a.Species.Rideable ? "ata bindirdi" : "hayvanlara götürdü";
            Say($"{parent.Name}, {child.Name}'i {verb} ({a.Name}).", child.Id);
            return null;
        }

        public void Tick(double gameDays) => Companions.Tick(gameDays);

        /// <summary>Çocuk başına günlük "oyuncağını ister misin" olayı olasılığı ve ödünç verme olasılığı.</summary>
        public const double BorrowAskChance = 0.25, BorrowLendChance = 0.7;

        /// <summary>
        /// Ödünç isteme: kendi o oyuncakta ustalaşmamış bir çocuk, başka bir çocuğun sağlam kişisel oyuncağını ister.
        /// lend null ise sahibi kendi karar verir (%70 verir). Sonuç metni döner, olay yoksa null.
        /// </summary>
        public string Borrow(Character owner, Character asker, string itemId, bool? lend = null)
        {
            if (owner == null || asker == null || owner == asker) return null;
            var p = Toys.Personal(owner.Id, itemId);
            if (p == null || p.IsBroken) return null;
            bool give = lend ?? _rng.NextDouble() < BorrowLendChance;
            Toys.AskToBorrow(owner, asker, itemId, give, Today());
            string text = give ? $"{owner.Name}, {ToyNames.Name(itemId)}ını {asker.Name}'e ödünç verdi."
                               : $"{owner.Name}, {ToyNames.Name(itemId)}ını {asker.Name}'e vermedi; {asker.Name} kıskandı.";
            Say(text, asker.Id);
            return text;
        }

        private void AutonomousBorrowing()
        {
            var kids = new List<Character>();
            foreach (var m in Population.Members)
                if (!m.IsDead && !m.IsVillager && (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen)) kids.Add(m);
            foreach (var asker in kids)
            {
                if (_rng.NextDouble() >= BorrowAskChance) continue;
                var offers = new List<PersonalToy>();
                foreach (var owner in kids)
                    if (owner != asker)
                        foreach (var p in Toys.PersonalOf(owner.Id))
                            if (!p.IsBroken && Toys.Personal(asker.Id, p.ItemId) == null) offers.Add(p);
                if (offers.Count == 0) continue;
                var pick = offers[_rng.Next(offers.Count)];
                Borrow(Population.Find(pick.OwnerId), asker, pick.ItemId);
            }
        }

        public void OnNewDay()
        {
            AutonomousBorrowing();
            Elders.OnNewDay();
            Toys.DailyUpdate(Today());
            Apprentices.DailyUpdate();
            foreach (var w in Recipes.AtRiskWarnings()) Say(w, 0);
            CheckApprenticeApplication();
        }

        // ---------------- çırak başvurusu (34-D3: itibar etkisi) ----------------

        public const int ApplicationMinReputation = 100;
        public const double ApplicationBaseChance = 0.03, ApplicationMaxChance = 0.10, ApplicationDays = 3;

        /// <summary>Köylü bir gencin hanedeki bir ustaya çırak olma isteği; yanıtlanmazsa 3 günde düşer.</summary>
        public ApprenticeApplication Application { get; private set; }

        /// <summary>İtibar en az "Saygın" ise günde %3 (itibarla %10'a dek): köylü bir çocuk/ergen hanedeki boş ustaya başvurur.</summary>
        public void CheckApprenticeApplication(bool force = false)
        {
            double today = Today();
            if (Application != null)
            {
                var ap = Population.Find(Application.ApplicantId);
                var ms = Population.Find(Application.MasterId);
                if (today - Application.Day >= ApplicationDays || ap == null || ms == null || ap.IsDead || ms.IsDead)
                    Application = null;
                return;
            }
            if (Reputation.Points < ApplicationMinReputation) return;
            double chance = Math.Min(ApplicationMaxChance, ApplicationBaseChance * Reputation.Points / (double)ApplicationMinReputation);
            if (!force && _rng.NextDouble() >= chance) return;

            var masters = new List<(Character m, SkillType skill)>();
            foreach (var m in Population.Members)
            {
                if (m.IsDead || m.IsVillager || m.Stage < AgeStage.YoungAdult || Apprentices.OfMaster(m.Id) != null) continue;
                SkillType best = default; int bestLevel = 0;
                foreach (SkillType sk in Enum.GetValues(typeof(SkillType)))
                {
                    int lv = m.Skills.GetLevel(sk);
                    if (lv > bestLevel) { bestLevel = lv; best = sk; }
                }
                if (bestLevel >= ApprenticeshipSystem.MasterLevel) masters.Add((m, best));
            }
            var applicants = new List<Character>();
            foreach (var c in Population.Members)
                if (!c.IsDead && c.IsVillager && (c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen) && Apprentices.OfApprentice(c.Id) == null) applicants.Add(c);
            if (masters.Count == 0 || applicants.Count == 0) return;

            var (master, skill) = masters[_rng.Next(masters.Count)];
            var who = applicants[_rng.Next(applicants.Count)];
            if (Apprentices.Blocker(master, who, skill) != null) return;
            Application = new ApprenticeApplication { ApplicantId = who.Id, MasterId = master.Id, Skill = skill, Day = today };
            Say($"{who.Name}, {master.Name}'in yanında {SkillNames.Name(skill)} çırağı olmak istiyor (hanenin adı duyulmuş). Aile panelinden yanıtla.", master.Id);
        }

        /// <summary>Başvuruyu yanıtla. Kabulde çıraklık başlar ve itibar +2.</summary>
        public string AnswerApplication(bool accept)
        {
            var a = Application;
            if (a == null) return "Bekleyen başvuru yok.";
            Application = null;
            var ap = Population.Find(a.ApplicantId);
            var ms = Population.Find(a.MasterId);
            if (!accept) return ap != null ? $"{ap.Name}'in başvurusu geri çevrildi." : "Başvuru geri çevrildi.";
            string why = Apprentices.Blocker(ms, ap, a.Skill);
            if (why != null) return why;
            Apprentices.Take(ms, ap, a.Skill, Today());
            Reputation.Add(ApplicationAcceptReputation, "köylü çırak alındı");
            return $"{ap.Name} artık {ms.Name}'in {SkillNames.Name(a.Skill)} çırağı.";
        }

        public const int ApplicationAcceptReputation = 2;

        /// <summary>Hane başına yeni biri geçti (D3: itibarın yarısı kalır).</summary>
        public void OnSuccession() => Reputation.OnSuccession();

        /// <summary>
        /// Karakterin bu sistemlerden gelen toplam beceri XP çarpanı. Bonuslar toplanır (belge 45 karar 4):
        /// 1 + (çocukluk izi − 1) + (can yoldaşı − 1) + (çıraklık − 1) + (aile tarifi − 1). Tavanı Character.GainSkillXp uygular.
        /// </summary>
        public double XpFactor(Character c, SkillType skill) =>
            1.0 + (Toys.XpFactor(c.Id, skill) - 1.0) + (Companions.XpFactor(c.Id, skill) - 1.0)
                + (Apprentices.XpFactor(c.Id, skill) - 1.0) + (Recipes.XpFactor(c.Id, skill) - 1.0)
                + (ExtraXp != null ? ExtraXp(c, skill) - 1.0 : 0.0);

        /// <summary>Başka sistemlerden gelen ek XP çarpanı (yadigâr giysi, belge 48 §11.4); null ise yok.</summary>
        public Func<Character, SkillType, double> ExtraXp { get; set; }

        private readonly HashSet<int> _watched = new HashSet<int>();

        private void Watch(Character c)
        {
            if (c == null || !_watched.Add(c.Id)) return;
            c.ExtraXpFactor = skill => XpFactor(c, skill);
            c.StageChanged += (ch, before, after) =>
            {
                if (after == AgeStage.Teen && before < AgeStage.Teen) Companions.OnBecameTeen(ch);
                if (after == AgeStage.YoungAdult && before < AgeStage.YoungAdult) Toys.OnGrownUp(ch);
            };
            c.Skills.LevelChanged += (skill, before, after) =>
            {
                if (after > before) Recipes.OnLevel(c, skill, after, Today());
            };
        }

        private void OnDied(Character dead)
        {
            Recipes.OnDied(dead);
            Apprentices.End(dead.Id);
            var heir = ToyHeir(dead);
            int n = Toys.Inherit(dead, heir, Today());
            if (n > 0 && heir != null) Say($"{dead.Name}'in çocukluk eşyaları {heir.Name}'e kaldı.", heir.Id);
        }

        /// <summary>Oyuncak mirasçısı: en küçük yaşayan çocuğu ya da torunu; yoksa hanedeki en küçük.</summary>
        private Character ToyHeir(Character dead)
        {
            Character best = null;
            foreach (var m in Population.Members)
            {
                if (m.IsDead || m.Id == dead.Id || m.IsVillager != dead.IsVillager) continue;
                bool kin = m.BirthMotherId == dead.Id || m.BirthFatherId == dead.Id || IsGrandchild(m, dead.Id);
                int score = (kin ? 1000 : 0) - (int)m.AgeDays;
                if (best == null || score > ((best.BirthMotherId == dead.Id || best.BirthFatherId == dead.Id || IsGrandchild(best, dead.Id) ? 1000 : 0) - (int)best.AgeDays)) best = m;
            }
            return best;
        }

        private bool IsGrandchild(Character m, int ancestor)
        {
            foreach (var pid in new[] { m.BirthMotherId, m.BirthFatherId })
            {
                if (!pid.HasValue) continue;
                var p = Population.Find(pid.Value);
                if (p != null && (p.BirthMotherId == ancestor || p.BirthFatherId == ancestor)) return true;
            }
            return false;
        }

        private static int Distance(GridPos a, GridPos b) => a.Z != b.Z ? int.MaxValue : Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        // ---- kayıt ----

        public UpbringingSave Export() => new UpbringingSave
        {
            toys = Toys.Export(), companions = Companions.Export(), apprentices = Apprentices.Export(),
            recipes = Recipes.Export(), reputation = Reputation.Export(), weddings = Weddings?.Export(), elderStories = Elders.Export()
        };

        public void Import(UpbringingSave s)
        {
            Toys.Import(s?.toys);
            Companions.Import(s?.companions);
            Apprentices.Import(s?.apprentices);
            Recipes.Import(s?.recipes);
            Reputation.Import(s?.reputation);
            Weddings?.Import(s?.weddings);
            Elders.Import(s?.elderStories ?? 0);
        }
    }

    public sealed class ApprenticeApplication
    {
        public int ApplicantId, MasterId;
        public SkillType Skill;
        public double Day;
    }

    [Serializable]
    public sealed class UpbringingSave
    {
        public ToyHabitSave toys;
        public CompanionBondSave[] companions;
        public ApprenticeshipSave[] apprentices;
        public FamilyRecipeSave[] recipes;
        public ReputationSave reputation;
        public WeddingSave weddings;
        public int elderStories;
    }
}
