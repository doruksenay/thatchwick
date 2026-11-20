using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Cooking
{
    /// <summary>
    /// Evin canlı hâli (belge 60b §3-4, 60c; kararlar F2, F3, K1, K2, F9):
    /// koku tepkileri, birlikte iş ve baş üstü konuşmalar, boşta hareketler ve "Me time", köpeğin dilenmesi, kedinin balık
    /// çalması, çocuğun taklidi. Baş üstü yazılar <see cref="Bubble"/> olayıyla Game katmanına gider.
    /// </summary>
    public sealed class SocialLife
    {
        public const int SmellRange = 5, WitnessRange = 6, WatchRange = 4;
        /// <summary>Birlikte işte söz-cevap aralığı (oyun günü; 12 dakikalık günde ~8 saniye) ve hız çarpanı.</summary>
        public const double TalkEvery = 0.011, HelpSpeed = 1.3;
        public const int MaxTalksPerJob = 5;
        /// <summary>Boş kalma eşikleri (gün): can sıkıntısı ve bunalma; Çalışkan'da yarısı.</summary>
        public const double BoredAfter = 0.5, IdleAfter = 1.0, MeTimeWork = 0.2, MeTimeRest = 0.02, IdleActEvery = 0.035;
        public const double CatCooldown = 0.5;
        public const int ImitationsForAptitude = 5;
        public const string Diligent = "trait.diligent", Placid = "trait.placid", Generous = "trait.generous", Sociable = "trait.sociable";

        private readonly ContentDatabase _content;
        private readonly MealService _meals;
        private readonly Random _rng;
        private readonly Dictionary<int, int> _helping = new Dictionary<int, int>();          // yardımcı → çalışan
        private readonly Dictionary<int, (double next, int said)> _talk = new Dictionary<int, (double, int)>();
        private readonly HashSet<int> _ending = new HashSet<int>();
        private readonly Dictionary<int, SkillType> _helpSkill = new Dictionary<int, SkillType>();   // yardım edilen işin becerisi (iş bitince de bilinsin)
        private readonly Dictionary<string, long> _usedDay = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<int, double> _idle = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _workStreak = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _nextAct = new Dictionary<int, double>();
        private readonly Dictionary<int, SkillType> _watched = new Dictionary<int, SkillType>();
        private readonly Dictionary<int, Dictionary<SkillType, int>> _imitated = new Dictionary<int, Dictionary<SkillType, int>>();
        private double _catUntil;
        private int _begHabit;
        private bool _begFedToday;

        public SocialLife(ContentDatabase content, MealService meals, int seed = 60)
        {
            _content = content; _meals = meals; _rng = new Random(seed);
        }

        public Func<double> Now { get; set; } = () => 0;
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";
        public Func<IEnumerable<Character>> People { get; set; }
        public Func<int, Character> Find { get; set; }
        public Func<Character, bool> IsIdle { get; set; }
        /// <summary>Karakterin o an yaptığı iş etkileşimi (yoksa null).</summary>
        public Func<Character, InteractionDef> WorkOf { get; set; }
        public Func<int, int, int> Affection { get; set; }
        public Action<int, int, int> AddAffection { get; set; }
        public Func<IEnumerable<Animal>> Pets { get; set; }
        public Func<int, GridPos?> TablePosition { get; set; }
        public Func<bool> AdultPack { get; set; }
        public Func<bool> BadWeather { get; set; }

        /// <summary>Baş üstü yazı (kişi, metin anahtarı).</summary>
        public event Action<Character, LocText> Bubble;
        /// <summary>Boşta hareket (kişi, tür): window, stretch, yawn, poke_fire, wipe_table, pet, flower, show_skill.</summary>
        public event Action<Character, string> IdleAct;
        public event Action<Animal, int> CatStole;
        public event Action<Animal, int> DogBegs;
        public event Action<Character, SkillType, int> Imitated;
        public event Action<LocText, Character> Notice;

        private static int Dist(GridPos a, GridPos b) => a.Z != b.Z ? 999 : Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        private IEnumerable<Character> Alive() => People == null ? Enumerable.Empty<Character>() : People().Where(c => c != null && !c.IsDead && c.Stage != AgeStage.Baby);

        // ---- konuşma satırı seçimi ----

        /// <summary>Etiketlere uyan, bugün kullanılmamış bir satır; yoksa null. İstenen bütün etiketler satırda olmalı.</summary>
        public TalkLineDef Pick(params string[] tags)
        {
            long day = (long)Math.Floor(Now());
            var pool = new List<TalkLineDef>();
            foreach (var t in _content.Talk.All)
            {
                if (t.AdultPack && (AdultPack == null || !AdultPack())) continue;
                bool ok = true;
                foreach (var tag in tags) if (tag != null && !t.HasTag(tag)) { ok = false; break; }
                if (!ok || (_usedDay.TryGetValue(t.Id, out long used) && used == day)) continue;
                if (t.Rare && _rng.NextDouble() > 0.03) continue;
                pool.Add(t);
            }
            if (pool.Count == 0) return null;
            var pick = pool[_rng.Next(pool.Count)];
            _usedDay[pick.Id] = day;
            return pick;
        }

        private void Say(Character c, TalkLineDef line, int index, params (string, object)[] args)
        {
            if (c == null || line == null || index >= line.Lines) return;
            Bubble?.Invoke(c, LocText.Of("talk." + line.Id + "." + (char)('a' + index), args));
        }

        // ---- koku (60c §2) ----

        /// <summary>Yemek pişti: yakındakiler kokuya tepki verir; aynı kişi aynı yemeğe bir kez.</summary>
        public void OnCooked(Character cook, ItemInstance station, DishDef dish, DishScoreBreakdown score, IReadOnlyList<SpiceDef> spices)
        {
            if (cook == null || station == null || dish == null || score == null) return;
            bool bad = score.Burnt || (spices != null && spices.Any(s => !s.FitsClass(dish.Class)));
            bool good = !bad && score.Quality >= ItemQuality.Fine;
            if (!bad && !good) return;
            int range = SmellRange + (spices?.Sum(s => s.Smell) ?? 0);
            bool praised = false, blamed = false;
            foreach (var c in Alive())
            {
                if (c == cook || c.Asleep || Dist(c.Position, station.Position) > range) continue;
                if (good)
                {
                    c.AddMoodlet("cook.smell_good", Text(LocText.Of("cook.mood.smell_good")), 2, 0.15);
                    if (c.Needs.Get(NeedType.Hunger) < 55f) c.AddMoodlet("cook.mouth_waters", Text(LocText.Of("cook.mood.mouth_waters")), 1, 0.15);
                    Say(c, Pick("smell_good", RoleTag(c, cook)) ?? Pick("smell_good", "anyone"), 0);
                    praised = true;
                }
                else
                {
                    c.AddMoodlet("cook.smell_burnt", Text(LocText.Of("cook.mood.smell_burnt")), -2, 0.15);
                    bool polite = c.HasTrait(Generous) || c.HasTrait(Sociable);
                    Say(c, Pick("smell_bad", polite ? "polite" : RoleTag(c, cook)) ?? Pick("smell_bad", "anyone"), 0);
                    if (!polite) blamed = true;
                }
            }
            if (praised)
            {
                cook.AddMoodlet("cook.praised", Text(LocText.Of("cook.mood.praised")), 2, 0.3);
                Say(cook, Pick("smell_reply_good"), 0);
            }
            else if (blamed)
            {
                cook.AddMoodlet("cook.embarrassed", Text(LocText.Of("cook.mood.embarrassed")), -2, 0.3);
                Say(cook, Pick("smell_reply_bad"), 0);
            }
        }

        private static string RoleTag(Character c, Character cook)
        {
            if (c.IsVillager) return "villager";
            if (c.Stage == AgeStage.Child) return "child";
            if (c.SpouseId == cook.Id) return "spouse";
            if (c.Stage == AgeStage.Elder) return "elder";
            return "anyone";
        }

        // ---- birlikte iş (60b §3) ----

        public bool IsHelping(Character helper, Character worker) => helper != null && worker != null && _helping.TryGetValue(helper.Id, out int w) && w == worker.Id;
        public bool HasHelper(Character worker) => worker != null && _helping.ContainsValue(worker.Id);

        public string CanHelp(Character helper, Character worker)
        {
            if (helper.Stage == AgeStage.Baby || worker.Stage == AgeStage.Baby) return Text(LocText.Of("cook.block.help_baby"));
            var work = WorkOf?.Invoke(worker);
            if (work == null || !Jobs.JobRunner.IsWork(work)) return Text(LocText.Of("cook.block.help_no_work", ("name", worker.Name)));
            if (_helping.TryGetValue(helper.Id, out int cur) && cur == worker.Id) return null;
            if (HasHelper(worker)) return Text(LocText.Of("cook.block.help_taken", ("name", worker.Name)));
            return null;
        }

        /// <summary>JobRunner.WorkingWith: yardım sürüyor mu. İlk çağrıda yardımı başlatır.</summary>
        public bool WorkingWith(Character helper, Character worker)
        {
            var work = WorkOf?.Invoke(worker);
            // İş bitti: yardım kaydı kapanışa geçer; ödülü EndHelp verir.
            if (work == null || !Jobs.JobRunner.IsWork(work)) { if (_helping.Remove(helper.Id)) _ending.Add(helper.Id); return false; }
            if (!_helping.ContainsKey(helper.Id))
            {
                if (HasHelper(worker)) return false;
                _helping[helper.Id] = worker.Id;
                _talk[helper.Id] = (Now() + TalkEvery * 0.5, 0);
            }
            if (work.Skill.HasValue) _helpSkill[helper.Id] = work.Skill.Value;
            return true;
        }

        /// <summary>Çalışanın hız çarpanı: yardımcısı varsa süre kısalır.</summary>
        public double SpeedFor(Character worker) => HasHelper(worker) ? HelpSpeed : 1.0;

        /// <summary>Yardım bitti (iş bitti ya da bırakıldı): yakınlık, sosyal ihtiyaç ve usta-çırak XP'si.</summary>
        public void EndHelp(Character helper, Character worker)
        {
            if (helper == null) return;
            bool was = _helping.Remove(helper.Id);
            if (!_ending.Remove(helper.Id) && !was) return;
            _talk.TryGetValue(helper.Id, out var t);
            _talk.Remove(helper.Id);
            if (worker == null || t.said == 0) { _helpSkill.Remove(helper.Id); return; }
            bool sour = Affection != null && Affection(helper.Id, worker.Id) < 15 && helper.Id != worker.Id;
            AddAffection?.Invoke(helper.Id, worker.Id, sour ? 1 : 2 + Math.Min(3, t.said));
            foreach (var c in new[] { helper, worker }) c.Needs.Restore(NeedType.Social, 10f);
            helper.AddMoodlet("cook.worked_together", Text(LocText.Of("cook.mood.worked_together")), 2, 0.3);
            worker.AddMoodlet("cook.worked_together", Text(LocText.Of("cook.mood.worked_together")), 2, 0.3);
            SkillType? skill = _helpSkill.TryGetValue(helper.Id, out var hs) ? hs : (SkillType?)null;
            _helpSkill.Remove(helper.Id);
            if (!skill.HasValue) return;
            int gap = worker.Skills.GetLevel(skill.Value) - helper.Skills.GetLevel(skill.Value);
            if (gap >= 3) helper.GainSkillXp(skill.Value, 6 + 2 * t.said);        // usta öğretir
            else if (gap <= -3) worker.GainSkillXp(skill.Value, 6 + 2 * t.said);
            else helper.GainSkillXp(skill.Value, 3);
        }

        private string FamilyTag(InteractionDef work)
        {
            if (work == null || !work.Skill.HasValue) return "any";
            switch (work.Skill.Value)
            {
                case SkillType.Cooking: case SkillType.Baking: case SkillType.Brewing: return "kitchen";
                case SkillType.Farming: case SkillType.Horticulture: case SkillType.Foraging: case SkillType.SoilCare: return "farm";
                case SkillType.AnimalHusbandry: return "animal";
                default: return "craft";
            }
        }

        private string RelationTag(Character a, Character b)
        {
            if (a.SpouseId == b.Id) return "spouse";
            bool parent = b.BirthMotherId == a.Id || b.BirthFatherId == a.Id || a.BirthMotherId == b.Id || a.BirthFatherId == b.Id;
            if (parent && (a.Stage <= AgeStage.Teen || b.Stage <= AgeStage.Teen)) return "parentchild";
            if (a.BirthMotherId.HasValue && a.BirthMotherId == b.BirthMotherId) return "sibling";
            return null;
        }

        private void TalkTick(double now)
        {
            if (_helping.Count == 0) return;
            foreach (var kv in _helping.ToList())
            {
                var helper = Find?.Invoke(kv.Key); var worker = Find?.Invoke(kv.Value);
                if (helper == null || worker == null) { _helping.Remove(kv.Key); continue; }
                if (!_talk.TryGetValue(kv.Key, out var t) || now < t.next || t.said >= MaxTalksPerJob) continue;
                var work = WorkOf?.Invoke(worker);
                TalkLineDef line = null;
                bool sour = Affection != null && Affection(helper.Id, worker.Id) < 15;
                int gap = work?.Skill != null ? Math.Abs(worker.Skills.GetLevel(work.Skill.Value) - helper.Skills.GetLevel(work.Skill.Value)) : 0;
                double roll = _rng.NextDouble();
                if (sour) line = Pick("work", "sour");
                else if (gap >= 3 && roll < 0.35) line = Pick("work", "teach");
                else if (roll < 0.55) { string rel = RelationTag(helper, worker); if (rel != null) line = Pick("work", rel); }
                else if (roll < 0.70) line = Pick("work", helper.MoodStatus >= Mood.MoodStatus.Good ? "cheerful" : helper.Needs.Get(NeedType.Energy) < 30f ? "tired" : "weather");
                if (line == null) line = Pick("work", FamilyTag(work)) ?? Pick("work", "any");
                if (line == null) { _talk[kv.Key] = (now + TalkEvery, t.said); continue; }
                // Öğretme satırını usta söyler; ötekilerde sırayı zar belirler.
                bool workerFirst = line.HasTag("teach") ? work?.Skill != null && worker.Skills.GetLevel(work.Skill.Value) >= helper.Skills.GetLevel(work.Skill.Value) : _rng.Next(2) == 0;
                var first = workerFirst ? worker : helper; var second = workerFirst ? helper : worker;
                Say(first, line, 0); Say(second, line, 1); Say(first, line, 2);
                _talk[kv.Key] = (now + TalkEvery * (0.75 + _rng.NextDouble() * 0.5), t.said + 1);
            }
        }

        // ---- boşta hareketler ve Me time (60b §4, 60c §3) ----

        private void IdleTick(double now, double dt)
        {
            foreach (var c in Alive())
            {
                if (c.IsVillager) continue;
                bool idle = IsIdle != null && IsIdle(c) && !c.Asleep && !c.InBreakdown;
                var work = idle ? null : WorkOf?.Invoke(c);
                if (!idle)
                {
                    _idle.Remove(c.Id);
                    if (work != null && Jobs.JobRunner.IsWork(work)) { _workStreak.TryGetValue(c.Id, out double w); _workStreak[c.Id] = w + dt; }
                    if (c.Mood.Has("cook.bored")) c.Mood.Remove("cook.bored");
                    if (c.Mood.Has("cook.idle")) c.Mood.Remove("cook.idle");
                    continue;
                }
                _idle.TryGetValue(c.Id, out double t);
                t += dt;
                _idle[c.Id] = t;
                if (_workStreak.TryGetValue(c.Id, out double streak) && streak >= MeTimeWork && t >= MeTimeRest)
                {
                    _workStreak.Remove(c.Id);
                    c.AddMoodlet("cook.me_time", Text(LocText.Of("cook.mood.me_time")), 2, 0.25);
                }
                else if (t >= MeTimeRest * 4) _workStreak.Remove(c.Id);
                if (c.Stage == AgeStage.Child) { TryImitate(c, now); continue; }
                double factor = c.HasTrait(Diligent) ? 0.5 : 1.0;
                if (c.Stage == AgeStage.Elder) { if (t >= BoredAfter) c.AddMoodlet("cook.resting", Text(LocText.Of("cook.mood.resting")), 1, 0.2); }
                else if (c.HasTrait(Placid)) { if (t >= BoredAfter) c.AddMoodlet("cook.at_ease", Text(LocText.Of("cook.mood.at_ease")), 1, 0.2); }
                else if (t >= IdleAfter * factor) { c.Mood.Remove("cook.bored"); c.AddMoodlet("cook.idle", Text(LocText.Of("cook.mood.idle")), -4, 0.2); }
                else if (t >= BoredAfter * factor) c.AddMoodlet("cook.bored", Text(LocText.Of("cook.mood.bored")), -2, 0.2);
                if (!_nextAct.TryGetValue(c.Id, out double next)) { _nextAct[c.Id] = now + IdleActEvery; continue; }
                if (now < next) continue;
                _nextAct[c.Id] = now + IdleActEvery * (0.7 + _rng.NextDouble() * 0.8);
                string act = IdleActs[_rng.Next(IdleActs.Length)];
                if (act == "show_skill" && !HasHighSkill(c)) act = "stretch";
                IdleAct?.Invoke(c, act);
                if (_rng.NextDouble() < 0.3) Say(c, Pick("idle", act) ?? Pick("idle", "anyone"), 0);
            }
        }

        private static bool HasHighSkill(Character c)
        {
            foreach (SkillType s in Enum.GetValues(typeof(SkillType))) if (c.Skills.GetLevel(s) >= 7) return true;
            return false;
        }

        public static readonly string[] IdleActs = { "window", "stretch", "yawn", "poke_fire", "wipe_table", "pet", "flower", "show_skill" };

        public double IdleDays(Character c) => c != null && _idle.TryGetValue(c.Id, out double t) ? t : 0;

        // ---- çocuk taklidi (60b §5.6) ----

        /// <summary>Bir yetişkin beceri işi yapıyor: yakınındaki çocuk izler.</summary>
        public void OnAdultWorking(Character adult, SkillType skill)
        {
            if (adult == null || adult.Stage < AgeStage.YoungAdult) return;
            foreach (var c in Alive())
                if (c.Stage == AgeStage.Child && !c.IsVillager && Dist(c.Position, adult.Position) <= WatchRange) _watched[c.Id] = skill;
        }

        private void TryImitate(Character child, double now)
        {
            if (!_watched.TryGetValue(child.Id, out var skill)) return;
            _watched.Remove(child.Id);
            if (!_imitated.TryGetValue(child.Id, out var d)) _imitated[child.Id] = d = new Dictionary<SkillType, int>();
            d.TryGetValue(skill, out int n);
            d[skill] = ++n;
            child.Needs.Restore(NeedType.Fun, 8f);
            Say(child, Pick("imitate"), 0);
            Imitated?.Invoke(child, skill, n);
        }

        public int Imitations(Character child, SkillType skill) =>
            child != null && _imitated.TryGetValue(child.Id, out var d) && d.TryGetValue(skill, out int n) ? n : 0;

        // ---- köpek ve kedi (60c §2.4-2.5) ----

        private Animal Pet(string speciesId) => Pets?.Invoke().FirstOrDefault(a => !a.IsDead && a.Species.Id == speciesId && a.IsMature);

        /// <summary>Sofra kuruldu: köpek masanın yanına gelir ve bakar (alıştıysa her öğün).</summary>
        public void OnServed(Serving s)
        {
            var dog = Pet("animal.dog");
            if (dog != null && (_begHabit >= 3 || _rng.NextDouble() < 0.5)) { _begFedToday = false; DogBegs?.Invoke(dog, s.TableId); }
        }

        public bool HasDog => Pet("animal.dog") != null;

        /// <summary>"Bir lokma ver": dostluk artar, köpek alışır.</summary>
        public void FeedScrap(Character c)
        {
            var dog = Pet("animal.dog");
            if (dog == null) return;
            dog.Friendship = Math.Min(100f, dog.Friendship + 4f);
            _begHabit++; _begFedToday = true;
            Say(c, Pick("dog_scrap"), 0, ("pet", dog.Name));
        }

        private void CatTick(double now)
        {
            if (now < _catUntil || !_meals.AnyServing) return;
            var cat = Pet("animal.cat");
            if (cat == null || cat.Fullness > 0.6f) return;
            foreach (var s in _meals.Servings.Values.ToList())
            {
                if (!_content.TryDish(s.DishId, out var dish) || dish.Class != DishClass.Fish) continue;
                var pos = TablePosition?.Invoke(s.TableId);
                if (!pos.HasValue) continue;
                // Balığa bakan (iki karo içinde uyanık biri) varken kedi yaklaşmaz.
                if (Alive().Any(c => !c.Asleep && Dist(c.Position, pos.Value) <= 2)) continue;
                var cook = Find?.Invoke(s.CookId);
                if (_meals.StealPortion(s.TableId) == null) continue;
                _catUntil = now + CatCooldown;
                cat.Fullness = 1f;
                foreach (var c in Alive())
                {
                    if (c.Asleep || Dist(c.Position, pos.Value) > WitnessRange) continue;
                    if (c == cook) continue;
                    if (c.Stage == AgeStage.Child) { c.AddMoodlet("cook.cat_laugh", Text(LocText.Of("cook.mood.cat_laugh")), 3, 0.3); Say(c, Pick("cat_steal", "child"), 0); }
                    else { c.AddMoodlet("cook.cat_stunned", Text(LocText.Of("cook.mood.cat_stunned")), 1, 0.1); Say(c, Pick("cat_steal", "anyone"), 0); }
                }
                if (cook != null && !cook.IsDead)
                {
                    cook.AddMoodlet("cook.cat_ate", Text(LocText.Of("cook.mood.cat_ate")), -2, 0.3);
                    Say(cook, Pick("cat_steal", "owner"), 0, ("pet", cat.Name));
                }
                Notice?.Invoke(LocText.Of("cook.n.cat_stole", ("pet", cat.Name), ("item", LocText.Of(ContentKeys.Name(s.DishId)))), cook);
                CatStole?.Invoke(cat, s.TableId);
                return;
            }
        }

        public void Tick(double gameDays)
        {
            double now = Now();
            TalkTick(now);
            IdleTick(now, gameDays);
            CatTick(now);
        }

        public void OnNewDay() { if (!_begFedToday && _begHabit > 0) _begHabit--; _begFedToday = false; }

        // kayıt: b:<alışkanlık> | i:<çocuk>:<beceri>=<n>,…
        public string[] Export()
        {
            var l = new List<string> { "b:" + _begHabit.ToString(CultureInfo.InvariantCulture) };
            foreach (var kv in _imitated)
                if (kv.Value.Count > 0) l.Add("i:" + kv.Key + ":" + string.Join(",", kv.Value.Select(x => x.Key + "=" + x.Value)));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _helping.Clear(); _talk.Clear(); _idle.Clear(); _workStreak.Clear(); _watched.Clear(); _imitated.Clear(); _begHabit = 0;
            if (data == null) return;
            foreach (var line in data)
            {
                var p = (line ?? "").Split(':');
                if (p[0] == "b" && p.Length == 2) int.TryParse(p[1], out _begHabit);
                else if (p[0] == "i" && p.Length == 3 && int.TryParse(p[1], out int id))
                {
                    var d = _imitated[id] = new Dictionary<SkillType, int>();
                    foreach (var e in p[2].Split(','))
                    {
                        var kv = e.Split('=');
                        if (kv.Length == 2 && Enum.TryParse(kv[0], out SkillType s) && int.TryParse(kv[1], out int n)) d[s] = n;
                    }
                }
            }
        }
    }
}
