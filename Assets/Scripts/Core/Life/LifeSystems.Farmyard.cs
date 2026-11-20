using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Farmyard;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Avlu sistemi (belge 57-57e): yerdeki pislik ve sinek, açıkta tuvalet ve azar, kompost süreci, ahır kirliliği,
    /// pisliğin sağlığa etkisi, fare ve araba filosu. Kurallar <c>Core/Farmyard</c> altındadır; burası bağlar.
    /// </summary>
    public sealed partial class LifeSystems
    {
        public const string MoodStomachAche = "eff_stomach_ache", MoodScolded = "eff_scolded", MoodSawTooMuch = "eff_saw_too_much",
            MoodSteppedInIt = "eff_stepped_in_it", MoodBadSmell = "eff_bad_smell", MoodLostAppetite = "eff_lost_appetite",
            MoodMurkyWater = "eff_murky_water", MoodTidyYard = "eff_tidy_yard", MoodDisgusted = "eff_disgusted";
        /// <summary>items.json'daki <c>farmAction</c> değerleri.</summary>
        public const string ActPileCollect = "pile_collect", ActCompostWet = "compost_add_wet", ActCompostDry = "compost_add_dry",
            ActCompostAsh = "compost_add_ash", ActCompostTurn = "compost_turn", ActCompostTake = "compost_take", ActWormFeed = "worm_feed",
            ActLeafFill = "leaf_fill", ActSlurryBrew = "slurry_brew", ActBarnClean = "barn_clean", ActBarnLitter = "barn_litter",
            ActDungStack = "dung_stack", ActRakeLeaves = "rake_leaves";
        /// <summary>Kıyı ve Güney seferinin getirdikleri.</summary>
        public const string SeaweedId = "item.seaweed", MarlId = "item.marl", DungCakeId = "item.dung_cake";

        public YardConfig YardNumbers { get; } = new YardConfig();

        private FilthService _filth;
        private ReliefWork _relief;
        private CompostService _compost;
        private YardHealth _yardHealth;
        private BarnService _barn;
        private CartFleet _fleet;
        private AnimalSystem _yardAnimals;
        private InteractionEffects _yardEffects;
        private readonly Dictionary<int, double> _accidentSeen = new Dictionary<int, double>();
        private readonly Random _yardRng = new Random(574);
        private double _refuseDebt;
        private YardSave _pendingYard;
        private bool _parkPending = true;

        public FilthService Filth => _filth ?? (_filth = CreateFilth());
        public CompostService Compost => _compost ?? (_compost = CreateCompost());
        public ReliefWork Relief => _relief ?? (_relief = CreateRelief());
        public YardHealth YardHealth => _yardHealth ?? (_yardHealth = new YardHealth(World, House, Filth));
        /// <summary>Ahır kirliliği; hayvan sistemi bağlanınca kurulur.</summary>
        public BarnService Barn => _barn;
        /// <summary>Araba filosu; hayvan sistemi bağlanınca kurulur.</summary>
        public CartFleet Fleet => _fleet;

        /// <summary>+18 paketi açık mı (küfürlü azar replikleri yalnızca orada).</summary>
        public Func<bool> AdultPack { get; set; }
        /// <summary>Haritada deniz kıyısı var mı (kıyı seferi yosun getirir).</summary>
        public Func<bool> HasCoast { get; set; }

        /// <summary>Azar: (gören, yapan, replik anahtarı). Oyun katmanı göreni yanına yürütür ve balonu çizer.</summary>
        public event Action<Character, Character, string> Scolded;
        /// <summary>Karakter pisliğe bastı (karakter, yığın).</summary>
        public event Action<Character, ItemInstance> SteppedInIt;
        /// <summary>Kedi pisliğini gömdü (yalnızca animasyon).</summary>
        public event Action<Animal> CatBuried;
        /// <summary>Fırtına tahılı vurdu (yatan, dökülen ekin sayısı): görünüm yatma animasyonunu başlatır.</summary>
        public event Action<int, int> CerealsHit;

        private bool IsNightNow => Clock.DayFraction < 0.22 || Clock.DayFraction > 0.86;

        private FilthService CreateFilth()
        {
            var f = new FilthService(World, Content, YardNumbers);
            f.Raining = () => RainyToday;
            f.Night = () => IsNightNow;
            f.IsRotten = it => Thatchwick.Core.Build.DecorSystem.IsRottenFill(it, Content);   // sepetteki çürük sinek ve koku yapar
            // Kırlangıç yuvası: bahar ve yazda altı karo içindeki sinekleri kırlangıçlar yer.
            f.FlyGuard = p =>
            {
                if (Clock.Season != Time.Season.Spring && Clock.Season != Time.Season.Summer) return false;
                foreach (var it in World.Items)
                    if (!it.IsDecor && it.Def.HasTag("swallow_nest") && Math.Abs(it.Position.X - p.X) <= YardNumbers.SwallowRadius && Math.Abs(it.Position.Y - p.Y) <= YardNumbers.SwallowRadius) return true;
                return false;
            };
            if (_heat != null) f.TemperatureAt = _heat.TemperatureAt;
            f.Decayed += (kind, pos) => { if (kind != PileKind.Dog && _plants != null && _plants.PlotOrNull(pos) != null) _plants.Enrich(pos, YardNumbers.DecayFertility); };
            return f;
        }

        private CompostService CreateCompost()
        {
            var c = new CompostService(World, House, Content, YardNumbers) { Now = () => Clock.TotalDays };
            if (_heat != null) c.TemperatureAt = _heat.TemperatureAt;
            c.Finished += (heap, grade, n) =>
                Say(LocText.Of(grade == CompostGrade.BlackSoil ? "yard.n.compost_black" : grade == CompostGrade.Mature ? "yard.n.compost_mature" : "yard.n.compost_raw", ("n", n)), NoticeLevel.Info);
            Filth.TierProviders.Add(it => { int t = c.SmellTier(it); return (t, t > 0 ? "compost" : null); });
            return c;
        }

        private ReliefWork CreateRelief()
        {
            var r = new ReliefWork(World, Filth) { People = () => Population.Members };
            r.Relieved += OnRelieved;
            return r;
        }

        // ---------- bağlama ----------

        private void AttachYard(InteractionEffects effects)
        {
            _yardEffects = effects;
            effects.Reserve = Compost;
            effects.RawCompostHasFilth = Compost.RawCompostHasFilth;
            effects.AppliedOn += OnYardApplied;
            effects.RawFilthHarvested += (c, crop) => YardHealth.AddTainted(1);
            effects.Fertilized += OnFertilized;
            var before = effects.ExtraBlocker;
            effects.ExtraBlocker = (c, it, i) => before?.Invoke(c, it, i) ?? YardBlocker(c, it, i);
            var chance = effects.FamilyYieldChance;
            effects.FamilyYieldChance = skill =>
                (chance != null ? chance(skill) : 0.0) + (_fleet == null ? 0.0 : skill == SkillType.Mining ? _fleet.HaulBonus(CartJob.Mine) : skill == SkillType.Woodcutting ? _fleet.HaulBonus(CartJob.Forest) : 0.0);
        }

        private void AttachYard(JobRunner runner)
        {
            runner.OpenReliefSpot = c => Relief.FindSpot(c);
            var before = runner.DurationFactor;
            runner.DurationFactor = (c, item, i) =>
                (before != null ? before(c, item, i) : 1.0) * (_barn != null && i != null && i.FarmAction == ActBarnClean ? _barn.CleanDurationFactor(item) : 1.0);
        }

        private void AttachYard(FarmWork farm) { farm.Yard = Relief; }

        private void AttachYard(PlantSystem plants)
        {
            plants.Now = () => Clock.TotalDays;
            plants.Fertigation = () => Compost.FertigationActive();
            plants.CerealsHit += (lodged, shattered) =>
            {
                if (lodged > 0) Say(LocText.Of("yard.n.lodged", ("n", lodged)), NoticeLevel.Warning);
                else if (shattered > 0) Say(LocText.Of("yard.n.shattered", ("n", shattered)), NoticeLevel.Warning);
                CerealsHit?.Invoke(lodged, shattered);
            };
            if (_barn != null) _barn.Plants = plants;
        }

        private void AttachYard(AnimalSystem animals)
        {
            if (animals == null || _yardAnimals == animals) return;
            _yardAnimals = animals;
            _barn = new BarnService(World, animals, House, Filth, Compost) { Plants = _plants, Grazing = () => animals.Released && Clock.Season != Time.Season.Winter };
            _barn.CatBuried += a => CatBuried?.Invoke(a);
            _barn.LitterShort += home => Say(LocText.Of("yard.n.litter_short", ("item", home.Def.Id)), NoticeLevel.Warning);
            animals.FilthPenalty = _barn.ComfortPenalty;
            animals.FilthSickRisk = _barn.SickRisk;
            Filth.TierProviders.Add(_barn.SmellTier);
            Compost.PoultryNear = heap =>
            {
                if (!animals.Released) return false;
                foreach (var a in animals.Animals)
                    if (!a.IsDead && a.Species.Housing == "coop" && Math.Abs(a.Position.X - heap.Position.X) <= 2 && Math.Abs(a.Position.Y - heap.Position.Y) <= 2) return true;
                return false;
            };
            _fleet = new CartFleet(World, House, animals, YardNumbers)
            {
                FindCharacter = id => Population.Find(id),
                CartingLevel = () => BestSkill(SkillType.Carting),
                Snow = () => Clock.Season == Time.Season.Winter,
                JobActive = (job, day) => job != CartJob.Market || (day + 1) % Economy.OxCart.TripEvery == 0,   // pazar seferi haftada bir
                AnimalLabel = a => string.IsNullOrEmpty(a.Name) ? a.Species.DisplayName : a.Species.DisplayName + " " + a.Name
            };
            if (_pendingYard != null) { _barn.Import(_pendingYard.barn); _fleet.Import(_pendingYard.fleet); }
        }

        /// <summary>Düven ve saban için boşta kalan çeki hayvanı (arabaya koşulanlar düşülür).</summary>
        public int FreeDraftAnimals() => _fleet != null ? _fleet.FreeDraftAnimals() : _yardAnimals?.DraftAnimals() ?? 0;

        /// <summary>Pazar seferinin engeli (OxCart.FleetBlocker); gidecekse null.</summary>
        public string MarketCartBlocker()
        {
            if (_fleet == null) return null;
            var b = _fleet.MarketBlocker(Clock.DayIndex);
            return b.IsEmpty ? null : Text(b);
        }

        /// <summary>Pazara giden ikinci ve sonraki arabaların eklediği yük.</summary>
        public int MarketExtraCapacity()
        {
            if (_fleet == null) return 0;
            int n = 0;
            foreach (var c in _fleet.Carts) if (c.Job == CartJob.Market && _fleet.Blocker(c, Clock.DayIndex).IsEmpty) n++;
            return Math.Max(0, n - 1) * 20;
        }

        /// <summary>
        /// Oyuncu arabaya görev, hayvan ve arabacı seçti. Olmuyorsa uyarı döner ("Öküz Karabaş bugün çok yoruldu,
        /// yapamaz") ve bildirim olarak da gösterilir; oluyorsa null.
        /// </summary>
        public string AssignCart(string cartKey, CartJob job, int animalId, int driverId)
        {
            if (_fleet == null) return null;
            var why = _fleet.Assign(_fleet.Find(cartKey), job, animalId, driverId, Clock.DayIndex);
            if (why.IsEmpty) return null;
            Say(why, NoticeLevel.Warning, driverId, transient: true);
            return Text(why);
        }

        // ---------- görevliler ----------

        private static InteractionDef ActionOf(ItemInstance item, string farmAction)
        {
            foreach (var i in item.Def.Interactions) if (i.FarmAction == farmAction) return i;
            return null;
        }

        /// <summary>Görevlinin avlu işleri: Çiftçi kompostu çevirir ve alır, Hayvan bakıcısı ahırı temizler, Ev işleri yığınları toplar.</summary>
        public IEnumerable<(ItemInstance item, InteractionDef interaction)> YardChores(DutyKind duty)
        {
            foreach (var item in new List<ItemInstance>(World.Items))
            {
                if (item.IsDecor) continue;
                InteractionDef act = null;
                if (duty == DutyKind.Farm && Compost.IsHeap(item))
                {
                    var h = Compost.HeapOf(item);
                    if (h.ReadyTotal > 0) act = ActionOf(item, ActCompostTake);
                    else if (Compost.NeedsTurn(item)) act = ActionOf(item, ActCompostTurn);
                }
                else if (duty == DutyKind.Animals && _barn != null && _barn.NeedsCleaning(item)) act = ActionOf(item, ActBarnClean);
                else if (Filth.IsPile(item))
                {
                    var pile = Filth.PileOf(item);
                    bool animal = pile != null && pile.Kind == PileKind.Animal;
                    if ((duty == DutyKind.Animals && animal) || (duty == DutyKind.House && !animal)) act = ActionOf(item, ActPileCollect);
                }
                if (act != null) yield return (item, act);
            }
        }

        // ---------- etkileşimler ----------

        private string YardBlocker(Character c, ItemInstance item, InteractionDef i)
        {
            if (i == null || item == null || string.IsNullOrEmpty(i.FarmAction)) return null;
            string key = null;
            switch (i.FarmAction)
            {
                case ActCompostWet:
                case ActCompostDry:
                    bool wet = i.FarmAction == ActCompostWet;
                    key = Compost.RoomIn(item) < 1 ? "yard.b.heap_full" : !Compost.HasInput(wet) ? (wet ? "yard.b.no_wet" : "yard.b.no_dry") : null;
                    break;
                case ActCompostAsh: key = Compost.RoomIn(item) < 1 ? "yard.b.heap_full" : House.Count(CompostService.AshId) <= 0 ? "yard.b.no_ash" : null; break;
                case ActCompostTurn: key = Compost.HeapOf(item) == null || Compost.HeapOf(item).IsEmpty ? "yard.b.heap_empty" : !Compost.CanTurn(item) ? "yard.b.turned" : null; break;
                case ActCompostTake: key = Compost.HeapOf(item) == null || Compost.HeapOf(item).ReadyTotal <= 0 ? "yard.b.not_ready" : null; break;
                case ActWormFeed: key = item.Fill >= Math.Max(1, item.Def.FillCapacity) ? "yard.b.heap_full" : !WormFood() ? "yard.b.no_wet" : null; break;
                case ActLeafFill: key = item.Fill >= Math.Max(1, item.Def.FillCapacity) ? "yard.b.heap_full" : House.Count(CompostService.LeavesId) <= 0 ? "yard.b.no_leaves" : null; break;
                case ActSlurryBrew:
                    key = Compost.SlurryState(item) != 0 ? "yard.b.slurry_busy" : Compost.ManureForSlurry < YardNumbers.SlurryManure ? "yard.b.no_manure" : null;
                    break;
                case ActDungStack: key = House.Count(CompostService.FreshManureId) <= 0 ? "yard.b.no_fresh" : Compost.Room(CompostService.ManureId) <= 0 ? "yard.b.heap_full" : null; break;
                case ActRakeLeaves: key = Clock.Season != Time.Season.Autumn ? "yard.b.leaves_season" : null; break;   // yaprak güzün dökülür
                case ActBarnClean: key = _barn == null ? "yard.b.clean" : _barn.DirtAt(item) < 1f ? "yard.b.clean" : null; break;
            }
            return key == null ? null : Text(LocText.Of(key));
        }

        private bool WormFood()
        {
            foreach (var kv in CompostService.WetInputs) if (kv.Key != CompostService.ManureId && House.Count(kv.Key) > 0) return true;
            return false;
        }

        private void OnYardApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (c == null || i == null) return;
            // Yıkanmak eli temizler; yemek ve su karın ağrısı çıkarabilir.
            if (i.Need == NeedType.Hygiene && i.Restore > 0) YardHealth.Wash(c.Id);
            if (i.Bath) YardHealth.Wash(c.Id);
            if (i.EmptyFill) YardHealth.MarkDirty(c.Id);
            if (i.Need == NeedType.Hunger && i.Restore > 0 && !c.IsVillager)
            {
                if (Filth.Nearest(c.Position, YardNumbers.AppetiteRadius, 1, fliesOnly: true).HasValue) c.AddMoodlet(MoodLostAppetite);
                Ache(c, YardHealth.OnAte(c.Id, c.Position));
            }
            if (item != null && item.Def.WaterSource && !c.IsVillager && YardHealth.WaterFoul(item))
            {
                c.AddMoodlet(MoodMurkyWater);
                Ache(c, YardHealth.OnDrewWater(item));
            }
            if (Consumes(i, "item.tea_mint") || Consumes(i, "item.tea_chamomile") || i.Heal > 0f)
                if (c.Mood.Remove(MoodStomachAche)) { c.BladderFactor = 1f; Say(LocText.Of("yard.n.ache_cured", ("name", c.Name)), NoticeLevel.Info, c.Id, transient: true); }
            if (i.Id == "relieve" && item != null && item.IsPlant) Relief.OnCoveredRelief(c);
            if (item == null || string.IsNullOrEmpty(i.FarmAction)) return;

            switch (i.FarmAction)
            {
                case ActPileCollect: CollectPile(c, item); break;
                case ActCompostWet: Compost.AddFromInventory(item, true); break;
                case ActCompostDry: Compost.AddFromInventory(item, false); break;
                case ActCompostAsh: Compost.AddAsh(item); break;
                case ActCompostTurn: Compost.Turn(item); break;
                case ActCompostTake:
                    var got = Compost.Take(item);
                    if (got[0] + got[1] + got[2] > 0)
                        Say(LocText.Of("yard.n.compost_taken", ("name", c.Name), ("raw", got[0]), ("mature", got[1]), ("black", got[2])), NoticeLevel.Info, c.Id, transient: true);
                    break;
                case ActWormFeed: Compost.FeedWorms(item); break;
                case ActLeafFill: Compost.FillLeaves(item); break;
                case ActSlurryBrew: Compost.BrewSlurry(item); break;
                case ActDungStack: Compost.StackFresh(); YardHealth.MarkDirty(c.Id); break;
                case ActBarnClean:
                    if (_barn != null)
                    {
                        int manure = _barn.Clean(item);
                        YardHealth.MarkDirty(c.Id);
                        Say(LocText.Of(manure > 0 ? "yard.n.barn_cleaned" : "yard.n.barn_cleaned_none", ("name", c.Name), ("n", manure)), NoticeLevel.Info, c.Id, transient: true);
                    }
                    break;
                case ActBarnLitter:
                    if (_barn != null)
                    {
                        bool on = !_barn.HasDeepLitter(item);
                        _barn.SetDeepLitter(item, on);
                        Say(LocText.Of(on ? "yard.n.litter_on" : "yard.n.litter_off"), NoticeLevel.Info, c.Id, transient: true);
                    }
                    break;
            }
        }

        private void CollectPile(Character c, ItemInstance item)
        {
            var pile = Filth.PileOf(item);
            if (pile == null) return;
            var kind = pile.Kind;
            bool dried = Filth.StageOf(pile) == PileStage.Dried;
            if (!Filth.Remove(item)) return;
            YardHealth.MarkDirty(c.Id);
            c.Needs.Set(NeedType.Hygiene, Math.Max(0f, c.Needs.Get(NeedType.Hygiene) - YardNumbers.StepHygieneLoss));
            if (kind == PileKind.Human)
            {
                // İnsan pisliği gübre yığınına girmez: kompost yığını varsa oraya (sıcak yığın zararsız kılar), yoksa gömülür.
                var heap = Compost.HeapWithRoom(1);
                if (heap != null) Compost.Add(heap, 1, 0, 0, 1);
                c.AddMoodlet(MoodDisgusted);
            }
            else if (kind == PileKind.Refuse)
            {
                var heap = Compost.HeapWithRoom(2);
                if (heap != null) Compost.Add(heap, 2, 0);
            }
            else if (kind == PileKind.Animal && dried && Content.Items.Contains(DungCakeId)) House.AddItem(DungCakeId, 1);   // kurumuş tezek yakacaktır
            else Compost.AddDung(FilthService.DungUnits(kind));
        }

        private void OnFertilized(Character c, ItemInstance crop, Fertilizer kind, FertilizeOutcome outcome)
        {
            if (c == null) return;
            if (kind == Fertilizer.FreshManure || kind == Fertilizer.HotManure || kind == Fertilizer.Manure || kind == Fertilizer.CompostRaw) YardHealth.MarkDirty(c.Id);
        }

        private void Ache(Character c, AcheCause cause)
        {
            if (cause == AcheCause.None || c == null || c.IsDead || c.Mood.Has(MoodStomachAche)) return;
            c.AddMoodlet(MoodStomachAche);
            c.BladderFactor = 2f;
            string key = cause == AcheCause.DirtyHands ? "yard.n.ache_hands" : cause == AcheCause.RawFilth ? "yard.n.ache_raw"
                : cause == AcheCause.MurkyWater ? "yard.n.ache_water" : "yard.n.ache_flies";
            Say(LocText.Of(key, ("name", c.Name)), NoticeLevel.Warning, c.Id);
        }

        // ---------- açıkta tuvalet ve azar ----------

        private void OnRelieved(Character actor, ItemInstance pile, bool covered, Character witness)
        {
            if (actor == null) return;
            if (!covered) actor.AddMoodlet(MoodCatalog.Ashamed);
            if (witness == null) return;
            actor.AddMoodlet(MoodCatalog.Ashamed);
            actor.AddMoodlet(MoodScolded);
            witness.AddMoodlet(MoodSawTooMuch);
            bool same = !actor.IsVillager && !witness.IsVillager;
            string key = ReliefWork.ScoldLineKey(witness, actor, same, AdultPack != null && AdultPack(), _yardRng.Next(1000));
            Scolded?.Invoke(witness, actor, key);
            Say(LocText.Of("yard.n.scold", ("witness", witness.Name), ("line", Text(LocText.Of(key, ("name", actor.Name))))), NoticeLevel.Info, actor.Id, transient: true);
            if (witness.IsVillager && !actor.IsVillager) Upbringing.Reputation.Add(-1, Text(LocText.Of("yard.rep.seen")));
        }

        // ---------- zaman ----------

        /// <summary>Her adımda: yığınlar yaşlanır, ahırın gün içi olayları, basma ve kaza denetimi.</summary>
        private void TickYard(double gameDays)
        {
            if (_parkPending) { _parkPending = false; ParkCarts(); }
            Filth.Tick(gameDays);
            _barn?.Tick(gameDays);
            bool anyPiles = Filth.Piles.Count > 0;
            foreach (var m in Population.Members)
            {
                if (m.IsDead || m.Away) continue;
                if (m.LastAccidentDay >= 0 && (!_accidentSeen.TryGetValue(m.Id, out var seen) || seen != m.LastAccidentDay))
                {
                    bool first = !_accidentSeen.ContainsKey(m.Id) && Clock.TotalDays - m.LastAccidentDay > 0.01;   // yüklenen eski kaza sayılmaz
                    _accidentSeen[m.Id] = m.LastAccidentDay;
                    if (!first) Filth.Drop(PileKind.Human, m.Position);
                }
                if (!anyPiles || m.Stage == AgeStage.Baby) continue;
                var stepped = Filth.StepCheck(m.Id, m.Position, IsNightNow);
                if (stepped == null) continue;
                m.AddMoodlet(MoodSteppedInIt);
                m.Needs.Set(NeedType.Hygiene, Math.Max(0f, m.Needs.Get(NeedType.Hygiene) - YardNumbers.StepHygieneLoss));
                SteppedInIt?.Invoke(m, stepped);
                if (!m.IsVillager) Say(LocText.Of("yard.n.stepped", ("name", m.Name)), NoticeLevel.Info, m.Id, transient: true);
            }
        }

        /// <summary>Ruh hali adımında: kötü koku ve karın ağrısının tuvalete etkisi.</summary>
        private void UpdateYardMood()
        {
            foreach (var m in Population.Members)
            {
                if (m.IsDead || m.IsVillager || m.Away) continue;
                m.BladderFactor = m.Mood.Has(MoodStomachAche) ? 2f : 1f;
                if (m.Asleep || m.Stage == AgeStage.Baby) continue;
                if (Filth.Nearest(m.Position, YardNumbers.SmellMoodRadius, 2).HasValue) m.AddMoodlet(MoodBadSmell);
            }
        }

        private void YardDay(long day)
        {
            var yesterday = Weather.Today(Clock.TotalDays - 0.5);
            Filth.OnNewDay(yesterday == WeatherType.Rain || yesterday == WeatherType.Storm, yesterday == WeatherType.Storm);
            Compost.OnNewDay();
            _barn?.OnNewDay();
            Filth.Refresh();
            FleetDay(day);

            int cats = 0;
            if (_yardAnimals != null) foreach (var a in _yardAnimals.Animals) if (!a.IsDead && a.IsMature && a.Species.Id == "animal.cat") cats++;
            var eaten = YardHealth.OnNewDay(cats, out int caught);
            if (caught > 0 && _yardAnimals != null)
                foreach (var a in _yardAnimals.Animals)
                    if (!a.IsDead && a.IsMature && a.Species.Id == "animal.cat") { _yardAnimals.AddDeed(a, Animals.Genetics.PetLegends.Rats, 1); if (--caught <= 0) break; }
            if (eaten.Count > 0) Say(LocText.Of("yard.n.mice", ("n", eaten.Count)), NoticeLevel.Warning);
            if (YardHealth.AnyFoulWater()) Say(LocText.Of("yard.n.water_foul"), NoticeLevel.Warning);

            if (day % 7 == 0)
            {
                if (YardHealth.ReekDays >= 3) Upbringing.Reputation.Add(-1, Text(LocText.Of("yard.rep.reek")));
                else if (YardHealth.CleanDays >= 7)
                    foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager) m.AddMoodlet(MoodTidyYard);
            }
        }

        /// <summary>Bozulan yiyecek: kompost yığını varsa oraya gider; yoksa üç bozuk mal avluya bir süprüntü yığını olur.</summary>
        private void OnSpoiledForYard(string itemId, int count)
        {
            if (count <= 0 || (Content.Items.TryGet(itemId, out var d) && d.HasTag("ice"))) return;
            var heap = Compost.HeapWithRoom(1);
            if (heap != null) { Compost.Add(heap, count, 0); return; }
            _refuseDebt += count;
            while (_refuseDebt >= 3)
            {
                _refuseDebt -= 3;
                Character who = null;
                foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager && !m.Away) { who = m; break; }
                var spot = who != null ? Relief.FindSpot(who) : null;
                if (!spot.HasValue || Filth.Drop(PileKind.Refuse, spot.Value) == null) { _refuseDebt = 0; break; }
            }
        }

        // ---------- araba filosu ----------

        /// <summary>
        /// Envanterdeki arabaları avluya koyar: sundurmanın, öküz arabasının ya da bir barınağın yakınında, ekili olmayan
        /// açık bir yere. Yeni yapılan araba da ertesi sabah böyle yerini bulur.
        /// </summary>
        public int ParkCarts()
        {
            if (_fleet == null) return 0;
            GridPos? anchor = null;
            foreach (var it in World.Items) if (!it.IsDecor && it.Def.HasTag(CartFleet.ShedTag)) { anchor = it.Position; break; }
            if (!anchor.HasValue) foreach (var it in World.Items) if (!it.IsDecor && it.Def.Id == CartFleet.OxCartId) { anchor = it.Position; break; }
            if (!anchor.HasValue) foreach (var it in World.Items) if (!it.IsDecor && it.Def.IsHousing) { anchor = it.Position; break; }
            if (!anchor.HasValue) foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager && !m.Away) { anchor = m.Position.OnLevel(0); break; }
            if (!anchor.HasValue) return 0;
            var at = anchor.Value;
            int n = _fleet.ParkInventory(Content, def =>
            {
                for (int r = 1; r <= 10; r++)
                    for (int dy = -r; dy <= r; dy++)
                        for (int dx = -r; dx <= r; dx++)
                        {
                            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                            var p = new GridPos(at.X + dx, at.Y + dy, 0);
                            if (!World.Map.InBounds(p) || World.Map.IsWater(p)) continue;
                            var room = World.Map.Rooms.RoomAt(p);
                            if (room != null && room.IsIndoor) continue;
                            if (_plants != null && _plants.PlotOrNull(p) != null) continue;   // tarlaya park edilmez
                            if (World.CanPlaceFloor(def, p, 0).Ok) return p;
                        }
                return null;
            });
            if (n > 0) Say(LocText.Of("yard.n.carts_parked", ("n", n)), NoticeLevel.Info);
            return n;
        }

        private bool StewardOnDuty()
        {
            foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager && !m.Away && m.Duty == DutyKind.Steward) return true;
            return false;
        }

        private bool LowPlots()
        {
            if (_plants == null) return false;
            foreach (var kv in _plants.SoilPlots) if (kv.Value.Worked && kv.Value.Fertility < _fleet.FertilizeBelow) return true;
            return false;
        }

        /// <summary>
        /// Kâhya (Kâhyalık 4, görevde): boşta duran arabalara o günün işini verir. Pazar günü pazara araba yoksa bir araba
        /// pazara; gübreleme kuralı açık ve yorgun karo varsa bir araba tarlaya. Gün bitince araba yine boşa döner;
        /// oyuncunun elle verdiği görevlere dokunmaz. Görev verilen arabaları döner.
        /// </summary>
        private List<Cart> StewardAssign(long day)
        {
            var given = new List<Cart>();
            if (!_fleet.StewardCarts || BestSkill(SkillType.Stewardship) < YardNumbers.StewardCartLevel || !StewardOnDuty()) return given;
            bool needMarket = (day + 1) % Economy.OxCart.TripEvery == 0, needField = _fleet.FertilizeRule && LowPlots();
            foreach (var c in _fleet.Carts) { if (c.Job == CartJob.Market) needMarket = false; if (c.Job == CartJob.Field) needField = false; }
            foreach (var cart in new List<Cart>(_fleet.Carts))
            {
                if (cart.Job != CartJob.Idle || cart.Broken || (!needMarket && !needField)) continue;
                var job = needField && (!needMarket || !CartFleet.NeedsAnimal(cart)) ? CartJob.Field : needMarket && CartFleet.NeedsAnimal(cart) ? CartJob.Market : CartJob.Idle;
                if (job == CartJob.Idle) continue;
                int animal = CartFleet.NeedsAnimal(cart) ? _fleet.PickAnimal(cart, day)?.Id ?? 0 : 0;
                int driver = 0;
                foreach (var m in Population.Members)
                    if (!m.IsDead && !m.IsVillager && !m.Away && _fleet.DriverBlocker(cart, m).IsEmpty) { driver = m.Id; break; }
                if (!_fleet.Assign(cart, job, animal, driver, day).IsEmpty) continue;
                given.Add(cart);
                if (job == CartJob.Market) needMarket = false; else needField = false;
            }
            return given;
        }

        /// <summary>Filonun günlük adımı (yeni gün başında kendiliğinden çalışır; testler doğrudan çağırır).</summary>
        public void RunFleetDay(long day) => FleetDay(day);

        private void FleetDay(long day)
        {
            if (_fleet == null) return;
            ParkCarts();
            var yesterday = Weather.Today(Clock.TotalDays - 0.5);
            if (yesterday == WeatherType.Rain || yesterday == WeatherType.Storm) _fleet.RainOnCarts();
            var bySteward = StewardAssign(day);
            var stuck = new List<(Cart cart, LocText why)>();
            var working = _fleet.OnNewDay(day, stuck);
            foreach (var cart in bySteward) { cart.Job = CartJob.Idle; cart.AnimalId = 0; cart.DriverId = 0; }
            foreach (var (cart, why) in stuck)
                Say(LocText.Of("yard.n.cart_stuck", ("item", cart.DefId), ("why", Text(why))), NoticeLevel.Warning);
            int seaweed = 0, marl = 0, mine = 0, forest = 0;
            foreach (var cart in working)
            {
                if (cart.Job == CartJob.Coast && (HasCoast == null || HasCoast()) && Content.Items.Contains(SeaweedId)) seaweed += YardNumbers.CoastSeaweedPerTrip;
                else if (cart.Job == CartJob.South && Content.Items.Contains(MarlId)) marl += YardNumbers.SouthMarlPerTrip;
                else if (cart.Job == CartJob.Mine) mine++;
                else if (cart.Job == CartJob.Forest) forest++;
                var driver = Population.Find(cart.DriverId);
                if (driver != null && !driver.IsDead) driver.GainSkillXp(SkillType.Carting, 4);
                CartSetOut?.Invoke(cart);
            }
            // Gün sonu kartı (belge 57e §2.4).
            if (seaweed > 0) { House.AddItem(SeaweedId, seaweed); Say(LocText.Of("yard.n.cart_seaweed", ("n", seaweed)), NoticeLevel.Info); Ledger.Note(Text(LocText.Of("yard.ledger.seaweed", ("n", seaweed)))); }
            if (marl > 0) { House.AddItem(MarlId, marl); Say(LocText.Of("yard.n.cart_marl", ("n", marl)), NoticeLevel.Info); Ledger.Note(Text(LocText.Of("yard.ledger.marl", ("n", marl)))); }
            if (mine > 0) Ledger.Note(Text(LocText.Of("yard.ledger.mine", ("n", mine), ("pct", (int)Math.Round(_fleet.HaulBonus(CartJob.Mine) * 100)))));
            if (forest > 0) Ledger.Note(Text(LocText.Of("yard.ledger.forest", ("n", forest), ("pct", (int)Math.Round(_fleet.HaulBonus(CartJob.Forest) * 100)))));
            if (_fleet.WorkingCount(CartJob.Market) > 0) Ledger.Note(Text(LocText.Of("yard.ledger.market", ("n", _fleet.WorkingCount(CartJob.Market)))));
            int tiles = _fleet.WorkingCapacity(CartJob.Field);
            if (tiles > 0 && _fleet.FertilizeRule)
            {
                int spread = SpreadByCart(tiles);
                if (spread > 0) { Say(LocText.Of("yard.n.cart_spread", ("n", spread)), NoticeLevel.Info); Ledger.Note(Text(LocText.Of("yard.ledger.spread", ("n", spread)))); }
            }
        }

        /// <summary>Araba bu sabah yola çıktı (görünüm ve ses).</summary>
        public event Action<Cart> CartSetOut;

        /// <summary>
        /// Tarla arabası kuralı: verimi eşiğin altındaki işlenmiş karolara, en yorgun olandan başlayarak gübre serer.
        /// Önce olgun kompost, sonra olgun gübre (taze gübre serilmez: ekini yakar). Serilen karo sayısı.
        /// </summary>
        public int SpreadByCart(int tiles)
        {
            if (_plants == null || tiles <= 0) return 0;
            var targets = new List<KeyValuePair<GridPos, float>>();
            foreach (var kv in _plants.SoilPlots)
                if (kv.Value.Worked && kv.Value.Fertility < _fleet.FertilizeBelow) targets.Add(new KeyValuePair<GridPos, float>(kv.Key, kv.Value.Fertility));
            targets.Sort((a, b) => a.Value != b.Value ? a.Value.CompareTo(b.Value) : a.Key.X != b.Key.X ? a.Key.X.CompareTo(b.Key.X) : a.Key.Y.CompareTo(b.Key.Y));
            int done = 0;
            foreach (var t in targets)
            {
                if (done >= tiles) break;
                var crop = World.GetItemAt(t.Key);
                if (crop != null && !(crop.IsPlant && crop.Def.IsCrop)) continue;
                if (_plants.CanFertilize(t.Key, Fertilizer.Compost) != FertilizeOutcome.Ok) continue;
                if (House.Count(CompostService.MatureId) > 0 && House.TryTakeItem(CompostService.MatureId))
                    _plants.ApplyFertilizer(t.Key, Fertilizer.Compost, 0.30f, crop);
                else if (TakeManure())
                    _plants.ApplyFertilizer(t.Key, Fertilizer.Manure, 0.45f, crop);   // olgun gübre yakmaz
                else continue;
                done++;
            }
            return done;
        }

        private bool TakeManure() => Compost.Withdraw(CompostService.ManureId, 1) > 0 || House.TryTakeItem(CompostService.ManureId);

        // ---------- kayıt ----------

        private YardSave ExportYard()
        {
            var save = new YardSave
            {
                piles = Filth.Export(), compost = Compost.Export(), health = YardHealth.Export(),
                barn = _barn != null ? _barn.Export() : _pendingYard?.barn, fleet = _fleet != null ? _fleet.Export() : _pendingYard?.fleet,
                spoiledForRefuse = _refuseDebt
            };
            if (_plants != null)
            {
                var lodged = new List<YardLodgedSave>();
                foreach (var kv in _plants.LodgedCrops) lodged.Add(new YardLodgedSave { itemId = kv.Key, loss = kv.Value });
                lodged.Sort((a, b) => a.itemId.CompareTo(b.itemId));
                save.lodged = lodged.ToArray();
                var shattered = new List<int>(_plants.ShatteredCrops);
                shattered.Sort();
                save.shattered = shattered.ToArray();
            }
            return save;
        }

        private void ImportYard(YardSave data)
        {
            _pendingYard = data;
            _accidentSeen.Clear();
            _refuseDebt = data?.spoiledForRefuse ?? 0;
            Filth.Import(data?.piles);
            Compost.Import(data?.compost);
            YardHealth.Import(data?.health);
            _barn?.Import(data?.barn);
            _fleet?.Import(data?.fleet);
            _parkPending = true;
            if (_plants != null && data != null)
            {
                var lodged = new List<KeyValuePair<int, float>>();
                if (data.lodged != null) foreach (var l in data.lodged) if (l != null) lodged.Add(new KeyValuePair<int, float>(l.itemId, l.loss));
                _plants.RestoreCerealDamage(lodged, data.shattered);
            }
        }
    }
}
