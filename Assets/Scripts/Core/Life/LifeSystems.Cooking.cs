using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Cooking;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Save;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yemek geliştirmesinin bağlantıları (belge 58-62): tarif defteri, sofra, mezbaha, Mutfak görevi, rutin, koku ve
    /// konuşmalar, baş başa etkinlikler. Alt sistemler <c>Core/Cooking</c> altındadır; burada yalnızca birbirine ve
    /// oyunun geri kalanına bağlanır.
    /// </summary>
    public sealed partial class LifeSystems
    {
        public RecipeBook Recipes { get; private set; }
        public MealService Meals { get; private set; }
        public Slaughterhouse Butchery { get; private set; }
        public KitchenDuty Kitchen { get; private set; }
        public Routines Routines { get; private set; }
        public SocialLife Social { get; private set; }
        public Outings Outings { get; private set; }

        /// <summary>Baş üstü yazı (koku, birlikte iş, boşta söz): Game katmanı balon olarak gösterir.</summary>
        public event Action<Character, LocText> Bubble;
        /// <summary>Boşta hareket (kişi, tür): Game katmanı kısa bir poz oynatır.</summary>
        public event Action<Character, string> IdleAct;
        /// <summary>Evcil hayvan sahnesi: (hayvan, masa eşyası kimliği, "steal" | "beg").</summary>
        public event Action<Animals.Animal, int, string> PetScene;

        private readonly Dictionary<int, JobRunner> _runners = new Dictionary<int, JobRunner>();
        private readonly Dictionary<string, IReadOnlyList<string>> _groupMembers = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        private Work.InteractionEffects _cookEffects;
        private Animals.AnimalSystem _cookAnimals;
        private Random _cookRng;
        private double _cookTick;
        private long _autumnReminder = -1;
        /// <summary>Sofra, mezbaha ve konuşma sistemlerinin güncelleme adımı (oyun günü).</summary>
        public const double CookingStepDays = 0.002;
        public const double RequestRecipeChance = 0.2;

        private void InitCooking(int seed)
        {
            _cookRng = new Random(seed != 0 ? Seeds.Derive(seed, 58) : 58);
            Func<double> now = () => Clock.TotalDays;
            Func<LocText, string> text = t => Text(t);
            House.GroupMembers = GroupMembersOf;
            Recipes = new RecipeBook(Content);
            Meals = new MealService(World, House, Content, Recipes, seed != 0 ? Seeds.Derive(seed, 59) : 59)
            {
                Now = now, Text = text, Find = Population.Find, KitchenPoints = KitchenPointsFor, Members = Household,
                HasFamilyRecipe = (c, skill) => Legacy != null && FamilyRecipeOf(c, skill),
                // Bulaşık tek sistemde (belge 63a §4): sofranın tabakları mutfak aletlerinin bulaşığına girer.
                // Kalite (63a §5): ekipman puanı sofra puanının ekipman bileşenidir; kabın tavanı basamağı keser.
                EquipmentPoints = KitchenTools.EquipmentPointsFor, QualityCap = KitchenTools.QualityCapFor,
                PlateUsed = KitchenTools.OnTablePlate, PlatesCleared = KitchenTools.AddDirtyTableware,
                DirtyPile = () => KitchenTools.Enforce ? KitchenTools.DirtyPieces : 0, WashChores = KitchenTools.WashChores
            };
            KitchenTools.PlatesOnTables = () => Meals.PlatesOnTables;
            Butchery = new Slaughterhouse(null, House, Content, seed != 0 ? Seeds.Derive(seed, 60) : 60)
            {
                Now = now, Text = text,
                InRoom = hook => Home != null && Home.Classify(Home.RoomOf(hook)).kind == RoomKind.Slaughterhouse,
                Cooled = () => World.Items.Any(it => it.Def.HasTag(Slaughterhouse.CoolerTag)),
                HasSlaughterhouse = () => World.Items.Any(it => it.Def.HasTag(Slaughterhouse.HookTag) && Home != null && Home.Classify(Home.RoomOf(it)).kind == RoomKind.Slaughterhouse)
            };
            Kitchen = new KitchenDuty(World, House, Content, Meals, Recipes) { Now = now, CanStart = (c, it, i) => _cookEffects?.CanStart(c, it, i), ExtraChores = DepotChores };
            Routines = new Routines(World) { Now = now, CanStart = (c, it, i) => _cookEffects?.CanStart(c, it, i), HasServing = id => Meals.ServingAt(id) != null };
            Social = new SocialLife(Content, Meals, seed != 0 ? Seeds.Derive(seed, 61) : 61)
            {
                Now = now, Text = text, People = () => Population.Members, Find = Population.Find,
                IsIdle = c => _runners.TryGetValue(c.Id, out var r) && r.IsIdle,
                WorkOf = c => _runners.TryGetValue(c.Id, out var r) ? r.CurrentInteraction : null,
                Pets = () => _cookAnimals != null ? _cookAnimals.Animals : (IEnumerable<Animals.Animal>)Array.Empty<Animals.Animal>(),
                TablePosition = id => World.GetItem(id)?.Position,
                BadWeather = BadWeatherNow
            };
            Outings = new Outings(House, Content, Meals, seed != 0 ? Seeds.Derive(seed, 62) : 62)
            {
                Now = now, Text = text, BadWeather = BadWeatherNow, IsWinter = () => Clock.Season == Time.Season.Winter,
                RomanceOf = v => Villagers?.StateOf(v)?.Romance ?? -1,
                AddRomance = (v, n) => { var st = Villagers?.StateOf(v); if (st != null && st.Romance > 0) st.Romance = Math.Min(VillagerService.MaxRomance, st.Romance + n); },
                SpotBeauty = SpotBeautyAt, NearWater = p => NearTile(p, 6, World.Map.IsWater),
                NearFire = p => World.Items.Any(it => it.Def.HeatOutput > 0f && it.IsLit && it.Position.Z == p.Z && Math.Abs(it.Position.X - p.X) + Math.Abs(it.Position.Y - p.Y) <= 4),
                VillagerTaste = (v, dish) =>
                {
                    var def = Villagers?.DefOf(v);
                    if (def == null || !Content.Items.TryGet(dish, out var item)) return 0;
                    switch (VillagerService.TasteFor(def, item)) { case GiftTaste.Love: return 2; case GiftTaste.Like: return 1; case GiftTaste.Dislike: return -1; default: return 0; }
                },
                MarketDay = () => Festivals != null && Festivals.IsMarketDayToday,
                AddReputation = (n, why) => Upbringing?.Reputation.Add(n, why),
                Reputation = () => Upbringing?.Reputation.Points ?? 0
            };

            Recipes.Learned += (c, dish, source) => Say(LocText.Of("cook.n.learned." + source, ("name", c.Name), ("item", LocText.Of(ContentKeys.Name(dish.Id)))), NoticeLevel.Info, c.Id);
            Recipes.HandSettled += (c, dish, tier) => Say(LocText.Of("cook.n.hand_settled", ("name", c.Name), ("item", LocText.Of(ContentKeys.Name(dish.Id))), ("n", DishScore.MasteryAt[tier - 1])), NoticeLevel.Info, c.Id);
            Meals.Notice += (t, c) => Say(t, NoticeLevel.Warning, c?.Id ?? 0);
            Meals.Cooked += OnDishCooked;
            Meals.Served += OnServed;
            Meals.Ate += (c, s, dish) => { if (Social.HasDog && c.Stage == AgeStage.Child && dish != null && Recipes.TastesOf(c, Population.Find)?.Disliked == dish.Id) SneakToDog(c); };
            Butchery.Notice += t => Say(t, NoticeLevel.Warning);
            Butchery.Slaughtered += OnSlaughtered;
            Kitchen.Notice += (t, c) => Say(t, NoticeLevel.Warning, c?.Id ?? 0);
            Social.Bubble += (c, t) => Bubble?.Invoke(c, t);
            Social.IdleAct += (c, act) => IdleAct?.Invoke(c, act);
            Social.Notice += (t, c) => Say(t, NoticeLevel.Info, c?.Id ?? 0);
            Social.CatStole += (cat, table) => { Oddities.Mark(OddityIds.CatThief); PetScene?.Invoke(cat, table, "steal"); };
            Social.DogBegs += (dog, table) => PetScene?.Invoke(dog, table, "beg");
            Social.Imitated += OnImitated;
            Outings.Notice += (t, c) => Say(t, NoticeLevel.Info, c?.Id ?? 0);
            // İstek ödülü olarak tarif (58 §4, 46): tamamlanan isteklerin beşte birinde hanenin en iyi aşçısı bir tarif öğrenir.
            Requests.Completed += r =>
            {
                if (_cookRng.NextDouble() >= RequestRecipeChance) return;
                var cook = Household().Where(m => !m.IsDead && m.Stage >= AgeStage.Teen).OrderByDescending(m => m.Skills.GetLevel(SkillType.Cooking)).FirstOrDefault();
                if (cook != null) Recipes.GrantUnknown(cook, LearnKind.Request, _cookRng);
            };
        }

        private IReadOnlyList<string> GroupMembersOf(string id)
        {
            if (_groupMembers.TryGetValue(id, out var cached)) return cached;
            IReadOnlyList<string> list = Content.Groups.TryGet(id, out var g) ? g.Members.Select(m => m.ItemId).ToList() : new List<string>();
            _groupMembers[id] = list;
            return list;
        }

        private bool FamilyRecipeOf(Character c, SkillType skill)
        {
            var fam = _cookEffects?.FamilyRecipe?.Invoke(c, skill);
            return fam.HasValue && fam.Value.generations > 0;
        }

        private bool BadWeatherNow()
        {
            var w = Weather.Today(Clock.TotalDays);
            return w != WeatherType.Clear;
        }

        private static bool NearTile(GridPos p, int radius, Func<GridPos, bool> test)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    if (Math.Abs(dx) + Math.Abs(dy) <= radius && test(new GridPos(p.X + dx, p.Y + dy, p.Z))) return true;
            return false;
        }

        /// <summary>Piknik yerinin güzelliği (0-3): su kenarı, ağaç altı, ulu ağaç.</summary>
        private int SpotBeautyAt(GridPos p)
        {
            int score = NearTile(p, 4, World.Map.IsWater) ? 1 : 0;
            bool tree = false, great = false;
            foreach (var it in World.Items)
            {
                if (it.Position.Z != p.Z) continue;
                int d = Math.Abs(it.Position.X - p.X) + Math.Abs(it.Position.Y - p.Y);
                if (d <= 3 && it.IsPlant && it.IsGrown && !it.Def.IsCrop) tree = true;
                if (d <= 5 && it.Def.HasTag("great_tree")) great = true;
            }
            return score + (tree ? 1 : 0) + (great ? 1 : 0);
        }

        /// <summary>Mutfak odasının puanı (58 §6, bileşen 5): oda türü ve düzeni, ışık, baharat rafı, hazırlık tezgâhı, kiler yakınlığı.</summary>
        private int KitchenPointsFor(ItemInstance station)
        {
            if (station == null || Home == null) return 4;
            var room = Home.RoomOf(station);
            if (room == null || !room.IsIndoor) return 2;   // açıkta pişirme
            var (kind, tidy) = Home.Classify(room);
            int pts = 2 + (kind == RoomKind.Kitchen ? 1 : 0) + (tidy ? 2 : 0);
            bool light = false, rack = false, prep = false, pantry = false;
            foreach (var it in World.Items)
            {
                if (it.Position.Z != station.Position.Z) continue;
                bool same = Home.RoomOf(it) == room;
                if (same && it.GivesLight) light = true;
                if (same && it.Def.HasTag("spicerack")) rack = true;
                if (same && it.Def.HasTag("preptable")) prep = true;
                if (it.Def.HasTag(CookingContentMapper.FoodPointTag) && Math.Abs(it.Position.X - station.Position.X) + Math.Abs(it.Position.Y - station.Position.Y) <= 8) pantry = true;
            }
            return Math.Min(DishScore.MaxKitchen, pts + (light ? 1 : 0) + (rack ? 2 : 0) + (prep ? 1 : 0) + (pantry ? 1 : 0));
        }

        // ---- bağlantılar ----

        private void AttachCooking(FamilyService family)
        {
            family.Outings = Outings; family.SocialLife = Social;
            Social.Affection = family.Affection; Outings.Affection = family.Affection;
            Action<int, int, int> add = (a, b, d) => family.SetAffection(a, b, family.Affection(a, b) + d);
            Social.AddAffection = add; Outings.AddAffection = add;
        }

        private void AttachCooking(Animals.AnimalSystem animals)
        {
            _cookAnimals = animals;
            Butchery.Animals = animals;
            Butchery.IsCompanion = a => Upbringing?.Companions != null && Population.Members.Any(m => Upbringing.Companions.Get(m.Id, a.Id) != null);
        }

        private void AttachCooking(Work.InteractionEffects effects)
        {
            _cookEffects = effects;
            effects.DishQuality = Meals.QualityFor;
            effects.AppliedOn += OnCookingApplied;
        }

        private void AttachCooking(JobRunner runner)
        {
            _runners[runner.Owner.Id] = runner;
            runner.JobChain = Content.JobChain;
            runner.StepPlace = StepPlaceFor;
            runner.WorkingWith = Social.WorkingWith;
            runner.RoutineProvider = Routines.Next;
            runner.StepChanged += (c, step) =>
            {
                var i = runner.CurrentInteraction;
                if (i != null && i.Skill.HasValue && JobRunner.IsWork(i)) Social.OnAdultWorking(c, i.Skill.Value);
            };
        }

        /// <summary>Mutfak görevini ve sofra işlerini görev planlayıcısına bağlar (GameRoot ve testler çağırır; Ev işleri atandıktan sonra).</summary>
        public void Attach(Work.DutyPlanner planner)
        {
            if (planner == null) return;
            planner.KitchenChores = Kitchen.Chores;
            var prev = planner.HouseChores;
            planner.HouseChores = () => (prev != null ? prev() : Enumerable.Empty<(ItemInstance, InteractionDef)>()).Concat(Meals.Chores());
        }

        public JobRunner RunnerOf(Character c) => c != null && _runners.TryGetValue(c.Id, out var r) ? r : null;

        /// <summary>Kesim sürüyor mu: Game mezbaha odasını kapalı çizer (60a §2.2 görünmezlik).</summary>
        public bool SlaughterInProgress(ItemInstance hook)
        {
            if (hook == null) return false;
            foreach (var r in _runners.Values)
                if (r.CurrentItem == hook && r.CurrentInteraction?.MealAction == Slaughterhouse.SlaughterAction) return true;
            return false;
        }

        private string CookingBlocker(Character c, ItemInstance item, InteractionDef i)
        {
            if (i.MealAction == Slaughterhouse.SlaughterAction || i.MealAction == Slaughterhouse.CollectAction) return Butchery.HookBlocker(c, item, i);
            if (i.MealAction == MealService.FeedScrap)
                return !Social.HasDog ? Text(LocText.Of("cook.block.no_dog")) : Meals.ServingAt(item.Id) == null ? Text(LocText.Of("cook.block.no_serving")) : null;
            if (i.MealAction == "writeRecipes") return Recipes.HasUnwritten(c) ? null : Text(LocText.Of("cook.block.nothing_to_write"));
            if (i.MealAction == "readRecipes") return Recipes.NextReadable(c) != null ? null : Text(LocText.Of("cook.block.nothing_to_read"));
            if (i.MealAction == "studyTreatise")
            {
                foreach (var d in i.Teaches) if (Content.Dishes.TryGet(d, out var dish) && !Recipes.Knows(c, dish)) return null;
                return Text(LocText.Of("cook.block.treatise_known"));
            }
            return Meals.Blocker(c, item, i, _cookEffects != null && _cookEffects.IsFinishing);
        }

        private void OnCookingApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (c == null || i == null) return;
            switch (i.MealAction)
            {
                case Slaughterhouse.SlaughterAction:
                case Slaughterhouse.CollectAction:
                    Butchery.OnApplied(c, item, i);
                    return;
                case MealService.FeedScrap: Social.FeedScrap(c); return;
                case "writeRecipes":
                {
                    int n = Recipes.WriteDown(c);
                    if (n > 0) Say(LocText.Of("cook.n.written", ("name", c.Name), ("n", n)), NoticeLevel.Info, c.Id);
                    return;
                }
                case "readRecipes": Recipes.ReadFromBook(c); return;
                case "studyTreatise":
                    foreach (var d in i.Teaches) Recipes.Learn(c, d, "treatise");
                    return;
            }
            Meals.OnApplied(c, item, i);
            // Kilerden ya da kazandan tek başına yenen pişmiş yemekte de damak tadı işler.
            if (i.MealAction == null && i.Dish == null && i.ConsumeItem != null && i.Produce.Count == 0 && Content.TryDish(i.ConsumeItem, out var eaten))
            {
                Meals.ApplyTaste(c, i.ConsumeItem);
                Recipes.OnAte(c, eaten.Id);
            }
        }

        private void OnDishCooked(Character c, ItemInstance station, DishDef dish, DishScoreBreakdown score, IReadOnlyList<SpiceDef> spices)
        {
            Kitchen.OnCooked(c, dish);
            if (score == null) return;
            Social.OnCooked(c, station, dish, score, spices);
            if (score.Burnt)
            {
                c.AddMoodlet("cook.burnt", Text(LocText.Of("cook.mood.burnt")), -2, 0.3);
                Say(LocText.Of("cook.n.burnt", ("name", c.Name), ("item", LocText.Of(ContentKeys.Name(dish.Id)))), NoticeLevel.Info, c.Id, transient: true);
            }
        }

        /// <summary>"Sofra hazır": açlığı eşiğin altındaki, işi kesilebilir hane üyeleri sofraya gelir; oyuncu emri kesilmez.</summary>
        private void OnServed(Serving s, ItemInstance table, Character cook)
        {
            if (cook != null) Bubble?.Invoke(cook, LocText.Of("cook.say.table_ready"));
            Social.OnServed(s);
            int called = 0;
            foreach (var m in Household())
            {
                if (m.IsDead || m.Stage == AgeStage.Baby || m.Asleep || m.InBreakdown) continue;
                if (m.Needs.Get(NeedType.Hunger) >= MealService.CallBelowHunger) continue;
                if (!_runners.TryGetValue(m.Id, out var r)) continue;
                var cur = r.Queue.Current;
                bool interruptible = cur == null || cur.Reason.HasValue || cur.IsDuty;
                if (!interruptible || called >= s.Portions) continue;
                if (r.Queue.TryEnqueue(Job.UseItem(table.Id, MealService.EatInteractionId).WithReason(NeedType.Hunger))) called++;
            }
        }

        /// <summary>Çocuk sevmediği yemeği gizlice köpeğe verir (60c §2.4); gören ebeveyn tepki verir.</summary>
        private void SneakToDog(Character child)
        {
            if (_cookRng.NextDouble() >= 0.35) return;
            Social.FeedScrap(child);
            foreach (var m in Household())
                if ((m.Id == child.BirthMotherId || m.Id == child.BirthFatherId) && !m.IsDead && !m.Asleep
                    && m.Position.Z == child.Position.Z && Math.Abs(m.Position.X - child.Position.X) + Math.Abs(m.Position.Y - child.Position.Y) <= SocialLife.WitnessRange)
                { Bubble?.Invoke(m, LocText.Of("cook.say.saw_that")); break; }
        }

        private void OnSlaughtered(Character c, Animals.Animal a, List<(string item, int count)> got)
        {
            string list = string.Join(", ", got.Select(g => g.count + " " + Text(LocText.Of(ContentKeys.Name(g.item)))));
            Say(LocText.Of("cook.sl.n.done", ("animal", a.Name ?? a.Species.DisplayName), ("list", list)), NoticeLevel.Info, c.Id);
            // Adı konmuş, sevilen bir hayvan kesildiyse çocuklar üzülür (60a §2.2).
            if (a.Friendship >= 40f)
                foreach (var m in Household())
                    if (!m.IsDead && (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen))
                        m.AddMoodlet("cook.sad_animal", Text(LocText.Of("cook.sl.mood.sad", ("animal", a.Name ?? a.Species.DisplayName))), -3, 1.0);
        }

        private void OnImitated(Character child, SkillType skill, int count)
        {
            if (count != SocialLife.ImitationsForAptitude) return;
            // Tekrarlanan taklit, o beceride yatkınlık doğma şansını artırır (28 kuralı): beşinci taklitte beşte bir şans.
            if (child.Skills.GetAptitude(skill) != Aptitude.None || _cookRng.NextDouble() >= 0.2) return;
            if (child.Skills.SetAptitude(skill, Aptitude.Minor))
                Say(LocText.Of("cook.n.imitation", ("name", child.Name), ("skill", SkillNames.Name(skill))), NoticeLevel.Info, child.Id);
        }

        private IEnumerable<(ItemInstance item, InteractionDef interaction)> DepotChores()
        {
            if (Butchery.DepotCount == 0) yield break;
            foreach (var it in World.Items)
            {
                var i = it.Def.Interactions.FirstOrDefault(x => x.MealAction == Slaughterhouse.CollectAction);
                if (i != null) { yield return (it, i); yield break; }
            }
        }

        /// <summary>
        /// Zincir adımının yeri (62 §2.4): işin eşyasına en yakın uygun eşyanın yanındaki yürünür karo. Yer yoksa null:
        /// adım eşyanın başında oynar.
        /// </summary>
        private GridPos? StepPlaceFor(Character c, ItemInstance station, string role)
        {
            if (station == null || role == null) return null;
            ItemInstance best = null; int bestD = int.MaxValue;
            foreach (var it in World.Items)
            {
                if (it == station || it.Position.Z != station.Position.Z || !FitsPlace(it, role)) continue;
                int d = Math.Abs(it.Position.X - station.Position.X) + Math.Abs(it.Position.Y - station.Position.Y);
                if (d < bestD && d <= 14) { best = it; bestD = d; }
            }
            if (best == null && role == "prep") return StepPlaceFor(c, station, "table");
            if (best == null) return null;
            GridPos? stand = null; int standD = int.MaxValue;
            foreach (var tile in best.OccupiedTiles())
                foreach (var o in StepOffsets)
                {
                    var p = new GridPos(tile.X + o.x, tile.Y + o.y, tile.Z);
                    if (!World.Map.InBounds(p) || !World.Map.IsWalkable(p)) continue;
                    int d = Math.Abs(p.X - c.Position.X) + Math.Abs(p.Y - c.Position.Y);
                    if (d < standD) { stand = p; standD = d; }
                }
            return stand;
        }

        private static readonly (int x, int y)[] StepOffsets = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        private bool FitsPlace(ItemInstance it, string role)
        {
            var d = it.Def;
            switch (role)
            {
                case "storage": return d.HasTag(CookingContentMapper.FoodPointTag) || d.Preserves > 0f;
                case "prep": return d.HasTag("preptable");
                case "table": return MealService.IsTable(d);
                case "water": return d.IsTap || d.WaterSource;
                case "fire": return d.HeatOutput > 0f && it.IsLit;
                default: return false;
            }
        }

        // ---- zaman ----

        private void CookingTick(double gameDays)
        {
            _cookTick += gameDays;
            if (_cookTick < CookingStepDays) return;
            double step = _cookTick;
            _cookTick = 0;
            Meals.Tick();
            Butchery.Tick();
            Social.Tick(step);
        }

        private void CookingNewDay(long day)
        {
            Butchery.OnNewDay(day);
            Social.OnNewDay();
            Outings.OnNewDay(Population.Find);
            if (_family != null && Villagers != null)
                Recipes.PollVillagerRecipes(Household(), (m, defId) =>
                {
                    var v = Villagers.CharacterOf(defId);
                    return v == null || v.IsDead ? -1 : _family.Affection(m.Id, v.Id);
                });
            // Sonbaharın ilk günü: kiler planı hatırlatması (60b §5.4).
            if (Clock.Season == Time.Season.Autumn && Clock.DayOfSeason == 0 && _autumnReminder != day)
            {
                _autumnReminder = day;
                int eaters = Household().Count(m => !m.IsDead);
                Kitchen.Pantry(eaters, out float days);
                Say(LocText.Of("cook.n.pantry_autumn", ("days", (int)Math.Floor(days))), days < 10f ? NoticeLevel.Warning : NoticeLevel.Info);
            }
        }

        private CookingSave ExportCooking() => new CookingSave
        {
            recipes = Recipes.Export(), meals = Meals.Export(), butchery = Butchery.Export(), kitchen = Kitchen.Export(),
            routines = Routines.Export(), social = Social.Export(), outings = Outings.Export()
        };

        private void ImportCooking(CookingSave data)
        {
            Recipes.Import(data?.recipes); Meals.Import(data?.meals); Butchery.Import(data?.butchery); Kitchen.Import(data?.kitchen);
            Routines.Import(data?.routines); Social.Import(data?.social); Outings.Import(data?.outings);
        }
    }
}
