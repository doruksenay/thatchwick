using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Family
{
    /// <summary>Soy Mührü'nün nereden geldiği (defterde ve bildirimde gösterilir).</summary>
    public enum SealSource
    {
        Generation = 0,      // yeni nesil hanedanın başına geçti
        WinterSurvived = 1,  // kış kayıpsız atlatıldı
        TradeLineage = 2,    // soy mesleği art arda en az 3 baş boyunca sürdü
        Goal = 3,            // hanedan hedefi
        Project = 4,         // köy projesi
        MarriedOut = 5,      // komşu haneye gelin/damat verildi
        Descendants = 6,     // köyde yaşayan soy (hane dışı torunlar) eşiği aştı
        Heirloom = 7,        // yadigâr yapmak için harcandı (eksi)
        Other = 8
    }

    public sealed class SealEntry
    {
        public double Day;
        public int Amount;
        public SealSource Source;
        public string Note;
    }

    public enum HeirloomKind { Item = 0, Tool = 1 }

    /// <summary>Yadigâr: yapanın adını ve tarihini taşır; hanedan elinde kaldığı her nesil bonusu büyür.</summary>
    public sealed class Heirloom
    {
        public int Id;
        public HeirloomKind Kind;
        /// <summary>Eşya yadigârında dünyadaki eşyanın kimliği (taşınınca değişmez).</summary>
        public int ItemInstanceId;
        public string ItemDefId;
        /// <summary>Alet yadigârında alet kimliği (ör. tool.axe); kademe yükseltmede yadigâr korunur.</summary>
        public string ToolId;
        public string BaseName;
        public int MakerId;
        public string MakerName;
        public double MadeDay;
        public int MadeYear;
        /// <summary>Yadigâr yapıldıktan sonra kaç baş değişimi (nesil) gördü.</summary>
        public int GenerationsHeld;

        /// <summary>Bonus basamağı: yapıldığı an 1, her nesil +1.</summary>
        public int Level => 1 + GenerationsHeld;
        public string DisplayName => $"{BaseName} · {MakerName} yadigârı (Yıl {MadeYear})";
    }

    /// <summary>Komşu haneye evlendirilen hane üyesi: köyde yaşamaya devam eder, soyu sayılır.</summary>
    public sealed class MarriedOutRecord
    {
        public int CharacterId;
        public string Name;
        public int SpouseId;
        public string SpouseName;
        public double Day;
    }

    /// <summary>
    /// Miras katmanı (kullanıcı kararı 17.11.2026): Soy Mührü zamanla ve soyla kazanılır, yadigârlar nesille güçlenir,
    /// evlendirilen çocuklar komşu hanelerde yaşamaya devam eder ve köy zamanla hanedanın soyuyla dolar.
    /// Online gerekmez; ortak kasaba gelirse bunlar yalnızca orada sergilenir.
    /// </summary>
    public sealed class Legacy
    {
        // ---- Soy Mührü kaynakları (yer tutucu denge, Adım 28) ----
        public const int SealsPerGeneration = 2;
        public const int WinterSurvivedSeals = 1;
        public const int TradeLineageMin = 3;
        public const int TradeLineageSeals = 2;
        public const int GoalSeals = 1;
        public const int ProjectSeals = 1;
        public const int MarriedOutSeals = 3;
        /// <summary>Hane dışında yaşayan soy (torun, evlendirilen çocuk ve çocukları) bu eşikleri aşınca mühür verir.</summary>
        public static readonly int[] DescendantMilestones = { 3, 6, 12, 24 };

        // ---- Yadigâr ----
        public const double HeirloomPerLevel = 0.05;   // basamak başına %5 hız, XP ve ihtiyaç doldurma
        public const double HeirloomMax = 0.25;        // en çok %25 (5. basamak)
        public const int HeirloomComfortPerLevel = 2;  // odanın konforuna basamak başına +2
        public const int HeirloomComfortMax = 10;
        public const int ToolHeirloomCost = 3;
        public const int MaxHeirlooms = 12;

        /// <summary>Yadigâr yapılabilen eşya kategorileri: hanede kullanılan, elden ele geçen eşyalar.</summary>
        public static readonly HashSet<string> HeirloomCategories = new HashSet<string>
        {
            "sleep", "seating", "surfaces", "storage", "kitchen", "work", "kids", "heat_light",
            "decor", "decor_wall", "rug", "hygiene"
        };

        // ---- Komşu haneye evlendirme ----
        /// <summary>Görücü usulü: haneye almak için gereken 100 yakınlıktan düşük.</summary>
        public const int MarryOutAffection = 60;
        /// <summary>Evli köylü çiftin günlük gebelik olasılığı (komşu hanelerde soy sürer).</summary>
        public const double NeighborConceiveChance = 0.04;
        public const int NeighborMaxChildren = 3;
        /// <summary>Komşu hanelerde yaşayan köylü sayısı bu tavana ulaşınca yeni gebelik olmaz (köy dolmasın).</summary>
        public const int NeighborVillageCap = 32;

        private readonly Population _population;
        private readonly Random _rng;
        private int _nextHeirloomId = 1;

        public int Seals { get; private set; }
        public List<SealEntry> Ledger { get; } = new List<SealEntry>();
        public List<Heirloom> Heirlooms { get; } = new List<Heirloom>();
        public List<MarriedOutRecord> MarriedOut { get; } = new List<MarriedOutRecord>();

        /// <summary>Bir yıl kaç gün (yadigârın "Yıl N" etiketi).</summary>
        public int DaysPerYear { get; set; } = 40;

        // Bir kez verilen ödüllerin izi (kayıtta tutulur)
        private readonly HashSet<int> _generationsRewarded = new HashSet<int>();
        private int _winterYearsRewarded = -1;
        private int _tradeLineageRewarded;
        private int _descendantMilestone;
        private bool _deathThisWinter;

        public event Action<SealEntry> SealsChanged;
        public event Action<Heirloom, string> HeirloomLost;
        public event Action<Character, Character> MarriedOutHappened;

        public Legacy(Population population, int seed = 61)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _rng = new Random(seed);
        }

        // =====================================================================
        // Soy Mührü
        // =====================================================================

        public SealEntry Award(SealSource source, int amount, double day, string note)
        {
            if (amount <= 0) return null;
            Seals += amount;
            var e = new SealEntry { Day = day, Amount = amount, Source = source, Note = note };
            Ledger.Add(e);
            SealsChanged?.Invoke(e);
            return e;
        }

        public bool TrySpend(int amount, SealSource source, double day, string note)
        {
            if (amount <= 0 || Seals < amount) return false;
            Seals -= amount;
            var e = new SealEntry { Day = day, Amount = -amount, Source = source, Note = note };
            Ledger.Add(e);
            SealsChanged?.Invoke(e);
            return true;
        }

        /// <summary>Baş değişimi: yadigârlar bir nesil yaşlanır, yeni nesil ve soy mesleği mühür verir.</summary>
        public void OnSuccession(Dynasty dynasty, Character heir, double day)
        {
            if (dynasty == null || heir == null) return;
            foreach (var h in Heirlooms) h.GenerationsHeld++;

            if (heir.Generation >= 2 && _generationsRewarded.Add(heir.Generation))
                Award(SealSource.Generation, SealsPerGeneration, day, $"{heir.Generation}. nesil hanedanın başında: {heir.Name}");

            // Soy mesleği: sondan geriye aynı meslekle biten başların sayısı.
            int run = 0;
            string trade = null;
            for (int i = dynasty.History.Count - 1; i >= 0; i--)
            {
                var rec = dynasty.History[i];
                if (rec.EndDay < 0) continue;                 // yaşayan baş (yeni varis) sayılmaz
                if (string.IsNullOrEmpty(rec.Trade)) break;
                if (trade == null) trade = rec.Trade;
                if (rec.Trade != trade) break;
                run++;
            }
            if (run >= TradeLineageMin && run > _tradeLineageRewarded)
            {
                _tradeLineageRewarded = run;
                Award(SealSource.TradeLineage, TradeLineageSeals, day, $"{trade}: {run} baş art arda aynı meslek");
            }
        }

        /// <summary>Hane üyesi öldü (kış kaybı izlenir).</summary>
        public void OnHouseholdDeath(Season season)
        {
            if (season == Season.Winter) _deathThisWinter = true;
        }

        /// <summary>Mevsim değişti. Kış bittiyse ve kışın haneden kimse ölmediyse mühür.</summary>
        public void OnSeasonChanged(Season previous, Season current, long dayIndex, double day)
        {
            if (previous == Season.Winter && current != Season.Winter)
            {
                int year = DaysPerYear > 0 ? (int)(dayIndex / DaysPerYear) : 0;
                if (!_deathThisWinter && year != _winterYearsRewarded)
                {
                    _winterYearsRewarded = year;
                    Award(SealSource.WinterSurvived, WinterSurvivedSeals, day, "Kış kayıpsız atlatıldı");
                }
                _deathThisWinter = false;
            }
            if (current == Season.Winter && previous != Season.Winter) _deathThisWinter = false;
        }

        public void OnGoalCompleted(string goalName, double day) => Award(SealSource.Goal, GoalSeals, day, "Hedef: " + goalName);
        public void OnProjectCompleted(string projectName, double day) => Award(SealSource.Project, ProjectSeals, day, "Köy projesi: " + projectName);

        // =====================================================================
        // Köydeki soy
        // =====================================================================

        /// <summary>Hanedan kurucusunun yaşayan torunları (hane dışındakiler: komşu hanelere gidenler ve çocukları).</summary>
        public int LivingDescendantsOutside(int founderId)
        {
            int n = 0;
            foreach (var c in _population.Members)
                if (!c.IsDead && c.IsVillager && IsDescendantOf(c, founderId)) n++;
            return n;
        }

        public bool IsDescendantOf(Character c, int ancestorId)
        {
            var seen = new HashSet<int>();
            var stack = new Stack<Character>();
            stack.Push(c);
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                foreach (var pid in new[] { x.BirthMotherId, x.BirthFatherId })
                {
                    if (!pid.HasValue || !seen.Add(pid.Value)) continue;
                    if (pid.Value == ancestorId) return true;
                    var p = _population.Find(pid.Value);
                    if (p != null) stack.Push(p);
                }
            }
            return false;
        }

        /// <summary>Hane dışı soy eşiklerini denetler; yeni eşik aşıldıysa mühür verir.</summary>
        public void CheckDescendants(int founderId, double day)
        {
            int n = LivingDescendantsOutside(founderId);
            while (_descendantMilestone < DescendantMilestones.Length && n >= DescendantMilestones[_descendantMilestone])
            {
                int seals = _descendantMilestone + 1;
                Award(SealSource.Descendants, seals, day, $"Köyde hanedanın soyundan {DescendantMilestones[_descendantMilestone]} kişi yaşıyor");
                _descendantMilestone++;
            }
        }

        // =====================================================================
        // Komşu haneye evlendirme
        // =====================================================================

        internal static bool ShareGrandparent(Character a, Character b, Population pop)
        {
            var ga = Grandparents(a, pop);
            foreach (var g in Grandparents(b, pop)) if (ga.Contains(g)) return true;
            return false;
        }

        private static HashSet<int> Grandparents(Character c, Population pop)
        {
            var set = new HashSet<int>();
            foreach (var pid in new[] { c.BirthMotherId, c.BirthFatherId })
            {
                if (!pid.HasValue) continue;
                var p = pop.Find(pid.Value);
                if (p == null) continue;
                if (p.BirthMotherId.HasValue) set.Add(p.BirthMotherId.Value);
                if (p.BirthFatherId.HasValue) set.Add(p.BirthFatherId.Value);
            }
            return set;
        }

        /// <summary>Hane üyesi bu köylüyle evlendirilip onun hanesine gönderilebilir mi? Olmazsa nedeni.</summary>
        public string CanMarryOut(Character member, Character villager, Dynasty dynasty, FamilyService family)
        {
            if (member == null || villager == null) return "Kişi bulunamadı.";
            if (member.IsDead || villager.IsDead) return "Ölmüş biri evlendirilemez.";
            if (member.IsVillager) return $"{member.Name} hanede değil.";
            if (!villager.IsVillager) return $"{villager.Name} köylü değil; hane içi evlilik için Evlen kullanılır.";
            if (dynasty != null && dynasty.HeadId == member.Id) return "Hanedanın başı başka haneye gidemez.";
            if (!FamilyService.IsAdultAge(member)) return $"{member.Name} evlenecek yaşta değil.";
            if (!FamilyService.IsAdultAge(villager)) return $"{villager.Name} evlenecek yaşta değil.";
            if (member.SpouseId.HasValue) return $"{member.Name} zaten evli.";
            if (villager.SpouseId.HasValue) return $"{villager.Name} zaten evli.";
            if (member.Sex == villager.Sex) return "Hanedan evliliği kadın ile erkek arasında olur.";
            if (FamilyService.AreCloseRelatives(member, villager) || ShareGrandparent(member, villager, _population))
                return "Akraba evliliği olmaz.";
            int aff = family != null ? family.Affection(member.Id, villager.Id) : 0;
            if (aff < MarryOutAffection) return $"Yakınlık yetmiyor ({aff}/{MarryOutAffection}).";
            return null;
        }

        /// <summary>
        /// Hane üyesini köylüyle evlendirip onun hanesine gönderir: üye köylü olur (oyuncu kontrolünden çıkar), köyde yaşamayı
        /// sürdürür; çocukları hanedanın soyundan sayılır. Veren taraf Soy Mührü alır.
        /// </summary>
        public bool MarryOut(Character member, Character villager, Dynasty dynasty, FamilyService family, double day, out string message)
        {
            string why = CanMarryOut(member, villager, dynasty, family);
            if (why != null) { message = why; return false; }

            member.SpouseId = villager.Id;
            villager.SpouseId = member.Id;
            member.IsVillager = true;
            member.IsPlayerControlled = false;
            member.CanStarve = false;
            Mood.MoodEvents.OnMarriage(member, villager);
            MarriedOut.Add(new MarriedOutRecord
            {
                CharacterId = member.Id, Name = member.Name, SpouseId = villager.Id, SpouseName = villager.Name, Day = day
            });
            Award(SealSource.MarriedOut, MarriedOutSeals, day, $"{member.Name}, {villager.Name} ile evlenip komşu haneye gitti");
            MarriedOutHappened?.Invoke(member, villager);
            message = $"{member.Name}, {villager.Name} ile evlendi ve onun hanesine gitti. Soy Mührü +{MarriedOutSeals}.";
            return true;
        }

        /// <summary>Komşu hanelerde evli köylü çiftler çocuk sahibi olur (soy köyde sürer). Gebe kalanları döner.</summary>
        public List<Character> NeighborFamiliesDaily()
        {
            var started = new List<Character>();
            // Köy nüfus tavanı: komşu hanelerin yaşayan köylüsü bu sayıya ulaşınca yeni gebelik olmaz.
            int villagers = 0;
            foreach (var c in _population.Members) if (!c.IsDead && c.IsVillager) villagers++;
            if (villagers >= NeighborVillageCap) return started;
            foreach (var w in _population.Members)
            {
                if (w.IsDead || !w.IsVillager || w.Sex != Sex.Female || !w.SpouseId.HasValue || w.IsPregnant) continue;
                if (!FamilyService.IsAdultAge(w)) continue;
                var h = _population.Find(w.SpouseId.Value);
                if (h == null || h.IsDead || !h.IsVillager) continue;
                int kids = 0;
                foreach (var c in _population.Members) if (c.BirthMotherId == w.Id) kids++;
                if (kids >= NeighborMaxChildren) continue;
                if (_rng.NextDouble() < NeighborConceiveChance && w.TryStartPregnancy(h.Id)) started.Add(w);
            }
            return started;
        }

        // =====================================================================
        // Yadigâr
        // =====================================================================

        public static int CostFor(ItemDef def) => def.Price < 60 ? 2 : def.Price < 150 ? 3 : 4;

        public Heirloom HeirloomOf(int itemInstanceId)
        {
            foreach (var h in Heirlooms) if (h.Kind == HeirloomKind.Item && h.ItemInstanceId == itemInstanceId) return h;
            return null;
        }

        public Heirloom ToolHeirloom(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return null;
            foreach (var h in Heirlooms) if (h.Kind == HeirloomKind.Tool && h.ToolId == toolId) return h;
            return null;
        }

        public static double BonusOf(Heirloom h) => h == null ? 0.0 : Math.Min(HeirloomMax, HeirloomPerLevel * h.Level);
        public static int ComfortOf(Heirloom h) => h == null ? 0 : Math.Min(HeirloomComfortMax, HeirloomComfortPerLevel * h.Level);

        public static bool IsEligible(ItemDef def) =>
            def != null && !def.IsPlant && def.Price > 0 && HeirloomCategories.Contains(def.Category ?? "");

        public string CanMakeHeirloom(ItemInstance item)
        {
            if (item == null) return "Eşya bulunamadı.";
            if (HeirloomOf(item.Id) != null) return "Bu eşya zaten yadigâr.";
            if (!IsEligible(item.Def)) return $"{item.Def.DisplayName} yadigâr olamaz (yalnızca evde kullanılan eşyalar).";
            if (Heirlooms.Count >= MaxHeirlooms) return $"En çok {MaxHeirlooms} yadigâr olur.";
            int cost = CostFor(item.Def);
            if (Seals < cost) return $"Soy Mührü yetmiyor ({Seals}/{cost}).";
            return null;
        }

        public Heirloom MakeHeirloom(ItemInstance item, Character maker, double day, out string message)
        {
            string why = CanMakeHeirloom(item);
            if (why != null) { message = why; return null; }
            int cost = CostFor(item.Def);
            TrySpend(cost, SealSource.Heirloom, day, $"Yadigâr: {item.Def.DisplayName}");
            var h = new Heirloom
            {
                Id = _nextHeirloomId++, Kind = HeirloomKind.Item, ItemInstanceId = item.Id, ItemDefId = item.Def.Id,
                BaseName = item.Def.DisplayName, MakerId = maker != null ? maker.Id : 0, MakerName = maker != null ? maker.Name : "Hanedan",
                MadeDay = day, MadeYear = YearOf(day)
            };
            Heirlooms.Add(h);
            message = $"{h.DisplayName}: artık yadigâr. Her nesil güçlenir.";
            return h;
        }

        public string CanMakeToolHeirloom(string toolId, string toolName)
        {
            if (string.IsNullOrEmpty(toolId)) return "Alet bulunamadı.";
            if (ToolHeirloom(toolId) != null) return $"{toolName} zaten yadigâr.";
            if (Heirlooms.Count >= MaxHeirlooms) return $"En çok {MaxHeirlooms} yadigâr olur.";
            if (Seals < ToolHeirloomCost) return $"Soy Mührü yetmiyor ({Seals}/{ToolHeirloomCost}).";
            return null;
        }

        public Heirloom MakeToolHeirloom(string toolId, string toolName, Character maker, double day, out string message)
        {
            string why = CanMakeToolHeirloom(toolId, toolName);
            if (why != null) { message = why; return null; }
            TrySpend(ToolHeirloomCost, SealSource.Heirloom, day, $"Yadigâr: {toolName}");
            var h = new Heirloom
            {
                Id = _nextHeirloomId++, Kind = HeirloomKind.Tool, ToolId = toolId, BaseName = toolName,
                MakerId = maker != null ? maker.Id : 0, MakerName = maker != null ? maker.Name : "Hanedan",
                MadeDay = day, MadeYear = YearOf(day)
            };
            Heirlooms.Add(h);
            message = $"{h.DisplayName}: artık yadigâr. Her nesil güçlenir.";
            return h;
        }

        private int YearOf(double day) => DaysPerYear > 0 ? (int)(day / DaysPerYear) + 1 : 1;

        /// <summary>
        /// İşe yadigâr çarpanı (JobRunner.ItemBonus): yadigâr eşyadaki iş (beceri) hızlanır ve daha çok XP verir,
        /// ihtiyaç doldurma (yatak, sandalye) güçlenir; yadigâr aletle yapılan iş de hızlanır ve XP verir.
        /// </summary>
        public ItemBonus BonusFor(ItemInstance item, InteractionDef interaction)
        {
            if (interaction == null) return ItemBonus.None;
            double speed = 1.0, xp = 1.0, restore = 1.0;
            var h = item != null ? HeirloomOf(item.Id) : null;
            if (h != null)
            {
                double b = BonusOf(h);
                if (interaction.Skill.HasValue) { speed *= 1.0 + b; xp *= 1.0 + b; }
                if (interaction.RestoresNeed) restore *= 1.0 + b;
            }
            var t = ToolHeirloom(interaction.Tool);
            if (t != null)
            {
                double b = BonusOf(t);
                speed *= 1.0 + b;
                xp *= 1.0 + b;
            }
            return new ItemBonus(speed, xp, restore);
        }

        /// <summary>Odanın konforuna yadigâr eşyanın katkısı (MoodService.ExtraComfort).</summary>
        public int ComfortBonus(ItemInstance item) => item == null ? 0 : ComfortOf(HeirloomOf(item.Id));

        /// <summary>Eşya envantere alınmak istendiğinde: yadigâr envantere girmez (geçmişi kaybolur); taşınır ya da satılır.</summary>
        public string StoreBlocker(ItemInstance item) =>
            item != null && HeirloomOf(item.Id) != null ? "Yadigâr envantere alınmaz; taşıyabilirsin. Satarsan geçmişi silinir." : null;

        /// <summary>Yadigâr satıldı: geçmişi ve bonusu silinir.</summary>
        public void OnItemSold(ItemInstance item)
        {
            var h = item != null ? HeirloomOf(item.Id) : null;
            if (h == null) return;
            Heirlooms.Remove(h);
            HeirloomLost?.Invoke(h, $"{h.DisplayName} satıldı; yadigârın geçmişi silindi.");
        }

        /// <summary>Dünyada artık olmayan eşya yadigârlarını düşürür (yıkım, yangın, kesim). Kaybolanları döner.</summary>
        public List<Heirloom> Validate(Func<int, ItemInstance> getItem)
        {
            var lost = new List<Heirloom>();
            for (int i = Heirlooms.Count - 1; i >= 0; i--)
            {
                var h = Heirlooms[i];
                if (h.Kind != HeirloomKind.Item || getItem(h.ItemInstanceId) != null) continue;
                Heirlooms.RemoveAt(i);
                lost.Add(h);
                HeirloomLost?.Invoke(h, $"{h.DisplayName} kayboldu.");
            }
            return lost;
        }

        // =====================================================================
        // Kayıt
        // =====================================================================

        public LegacySave Export()
        {
            var s = new LegacySave
            {
                seals = Seals,
                nextHeirloomId = _nextHeirloomId,
                generationsRewarded = new List<int>(_generationsRewarded).ToArray(),
                winterYearRewarded = _winterYearsRewarded,
                tradeLineageRewarded = _tradeLineageRewarded,
                descendantMilestone = _descendantMilestone,
                deathThisWinter = _deathThisWinter
            };
            var ledger = new List<SealEntrySave>();
            foreach (var e in Ledger) ledger.Add(new SealEntrySave { day = e.Day, amount = e.Amount, source = (int)e.Source, note = e.Note });
            s.ledger = ledger.ToArray();
            var hs = new List<HeirloomSave>();
            foreach (var h in Heirlooms)
                hs.Add(new HeirloomSave
                {
                    id = h.Id, kind = (int)h.Kind, itemInstanceId = h.ItemInstanceId, itemDefId = h.ItemDefId, toolId = h.ToolId,
                    baseName = h.BaseName, makerId = h.MakerId, makerName = h.MakerName, madeDay = h.MadeDay, madeYear = h.MadeYear,
                    generationsHeld = h.GenerationsHeld
                });
            s.heirlooms = hs.ToArray();
            var mo = new List<MarriedOutSave>();
            foreach (var m in MarriedOut)
                mo.Add(new MarriedOutSave { characterId = m.CharacterId, name = m.Name, spouseId = m.SpouseId, spouseName = m.SpouseName, day = m.Day });
            s.marriedOut = mo.ToArray();
            return s;
        }

        public void Restore(LegacySave s)
        {
            Ledger.Clear(); Heirlooms.Clear(); MarriedOut.Clear(); _generationsRewarded.Clear();
            Seals = 0; _nextHeirloomId = 1; _winterYearsRewarded = -1; _tradeLineageRewarded = 0; _descendantMilestone = 0; _deathThisWinter = false;
            if (s == null) return;
            Seals = Math.Max(0, s.seals);
            _nextHeirloomId = Math.Max(1, s.nextHeirloomId);
            if (s.generationsRewarded != null) foreach (var g in s.generationsRewarded) _generationsRewarded.Add(g);
            _winterYearsRewarded = s.winterYearRewarded;
            _tradeLineageRewarded = Math.Max(0, s.tradeLineageRewarded);
            _descendantMilestone = Math.Max(0, Math.Min(DescendantMilestones.Length, s.descendantMilestone));
            _deathThisWinter = s.deathThisWinter;
            if (s.ledger != null)
                foreach (var e in s.ledger)
                    if (e != null)
                        Ledger.Add(new SealEntry
                        {
                            Day = e.day, Amount = e.amount, Note = e.note,
                            Source = Enum.IsDefined(typeof(SealSource), e.source) ? (SealSource)e.source : SealSource.Other
                        });
            if (s.heirlooms != null)
                foreach (var h in s.heirlooms)
                    if (h != null)
                    {
                        Heirlooms.Add(new Heirloom
                        {
                            Id = h.id, Kind = h.kind == 1 ? HeirloomKind.Tool : HeirloomKind.Item, ItemInstanceId = h.itemInstanceId,
                            ItemDefId = h.itemDefId, ToolId = h.toolId, BaseName = h.baseName, MakerId = h.makerId, MakerName = h.makerName,
                            MadeDay = h.madeDay, MadeYear = h.madeYear, GenerationsHeld = Math.Max(0, h.generationsHeld)
                        });
                        _nextHeirloomId = Math.Max(_nextHeirloomId, h.id + 1);
                    }
            if (s.marriedOut != null)
                foreach (var m in s.marriedOut)
                    if (m != null)
                        MarriedOut.Add(new MarriedOutRecord { CharacterId = m.characterId, Name = m.name, SpouseId = m.spouseId, SpouseName = m.spouseName, Day = m.day });
        }
    }

    [Serializable]
    public sealed class SealEntrySave
    {
        public double day;
        public int amount, source;
        public string note;
    }

    [Serializable]
    public sealed class HeirloomSave
    {
        public int id, kind, itemInstanceId, makerId, madeYear, generationsHeld;
        public string itemDefId, toolId, baseName, makerName;
        public double madeDay;
    }

    [Serializable]
    public sealed class MarriedOutSave
    {
        public int characterId, spouseId;
        public string name, spouseName;
        public double day;
    }

    [Serializable]
    public sealed class LegacySave
    {
        public int seals, nextHeirloomId, winterYearRewarded = -1, tradeLineageRewarded, descendantMilestone;
        public bool deathThisWinter;
        public int[] generationsRewarded;
        public SealEntrySave[] ledger;
        public HeirloomSave[] heirlooms;
        public MarriedOutSave[] marriedOut;
    }
}
