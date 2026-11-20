using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Upbringing
{
    public sealed class Apprenticeship
    {
        public int MasterId;
        public int ApprenticeId;
        public SkillType Skill;
        public double StartDay;
    }

    /// <summary>
    /// Çıraklık (belge 34, D1 ve D4): seviye 5 ve üstü usta bir çırak alır; çırak o beceride daha hızlı öğrenir.
    /// Usta yaşlıysa ya da Öğretmenlik 5+ ise çarpan büyür. Çırak 5. seviyeye ulaşınca usta olur, çıraklık biter.
    /// </summary>
    public sealed class ApprenticeshipSystem
    {
        public const int MasterLevel = 5;
        public const double BaseFactor = 1.5, ElderOrTeacherBonus = 0.5;
        public const int TeacherLevel = 5;
        public const double MasterDailyTeachingXp = 3;
        public const double VillagerDailyXp = 8;

        private readonly Population _population;
        private readonly List<Apprenticeship> _list = new List<Apprenticeship>();

        public ApprenticeshipSystem(Population population)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
        }

        public event Action<Character, Character, SkillType> Graduated;
        public IReadOnlyList<Apprenticeship> All => _list;

        public Apprenticeship OfApprentice(int id)
        {
            foreach (var a in _list) if (a.ApprenticeId == id) return a;
            return null;
        }

        public Apprenticeship OfMaster(int id)
        {
            foreach (var a in _list) if (a.MasterId == id) return a;
            return null;
        }

        /// <summary>Çırak alamama nedeni (null: alabilir).</summary>
        public string Blocker(Character master, Character apprentice, SkillType skill)
        {
            if (master == null || apprentice == null || master.IsDead || apprentice.IsDead) return "Kişi yok.";
            if (master.Id == apprentice.Id) return "Kendine çırak olamaz.";
            if (master.Stage < AgeStage.YoungAdult) return "Usta yetişkin olmalı.";
            if (master.Skills.GetLevel(skill) < MasterLevel) return $"Usta olmak için {SkillNames.Name(skill)} {MasterLevel}. seviye gerekir.";
            if (apprentice.Stage < AgeStage.Child) return "Bebek çırak olamaz.";
            if (apprentice.Skills.GetLevel(skill) >= MasterLevel) return "Zaten usta.";
            if (OfMaster(master.Id) != null) return $"{master.Name}'in zaten bir çırağı var.";
            if (OfApprentice(apprentice.Id) != null) return $"{apprentice.Name} zaten çırak.";
            return null;
        }

        public bool Take(Character master, Character apprentice, SkillType skill, double today)
        {
            if (Blocker(master, apprentice, skill) != null) return false;
            _list.Add(new Apprenticeship { MasterId = master.Id, ApprenticeId = apprentice.Id, Skill = skill, StartDay = today });
            return true;
        }

        /// <summary>Kişinin (usta ya da çırak) çıraklığını bitirir.</summary>
        public bool End(int characterId) => _list.RemoveAll(a => a.ApprenticeId == characterId || a.MasterId == characterId) > 0;

        /// <summary>Çırağın o beceride XP çarpanı (çırak değilse 1).</summary>
        public double XpFactor(int characterId, SkillType skill)
        {
            var a = OfApprentice(characterId);
            if (a == null || a.Skill != skill) return 1.0;
            var m = _population.Find(a.MasterId);
            if (m == null || m.IsDead) return 1.0;
            return BaseFactor + (m.Stage == AgeStage.Elder || m.Skills.GetLevel(SkillType.Teaching) >= TeacherLevel ? ElderOrTeacherBonus : 0);
        }

        /// <summary>Günlük: usta öğretmenlik XP'si kazanır; ölen usta ya da çırakta çıraklık biter; usta olan çırak mezun olur.</summary>
        public void DailyUpdate()
        {
            for (int i = _list.Count - 1; i >= 0; i--)
            {
                var a = _list[i];
                var m = _population.Find(a.MasterId);
                var ap = _population.Find(a.ApprenticeId);
                if (m == null || ap == null || m.IsDead || ap.IsDead) { _list.RemoveAt(i); continue; }
                m.GainSkillXp(SkillType.Teaching, MasterDailyTeachingXp);
                // Köylü çırak hanede iş yapmaz; ustanın yanında geçen gün doğrudan XP verir.
                if (ap.IsVillager) ap.GainSkillXp(a.Skill, VillagerDailyXp * XpFactor(ap.Id, a.Skill));
                if (ap.Skills.GetLevel(a.Skill) >= MasterLevel)
                {
                    _list.RemoveAt(i);
                    Graduated?.Invoke(m, ap, a.Skill);
                }
            }
        }

        public ApprenticeshipSave[] Export()
        {
            var l = new List<ApprenticeshipSave>();
            foreach (var a in _list) l.Add(new ApprenticeshipSave { masterId = a.MasterId, apprenticeId = a.ApprenticeId, skill = (int)a.Skill, startDay = a.StartDay });
            return l.ToArray();
        }

        public void Import(ApprenticeshipSave[] rows)
        {
            _list.Clear();
            if (rows == null) return;
            foreach (var r in rows)
                if (r != null) _list.Add(new Apprenticeship { MasterId = r.masterId, ApprenticeId = r.apprenticeId, Skill = (SkillType)r.skill, StartDay = r.startDay });
        }
    }

    /// <summary>Aile sırrı/tarifi tanımı: hangi beceride, kaçıncı seviyede açılır, etkisi.</summary>
    public sealed class FamilyRecipeDef
    {
        public string Id;
        public SkillType Skill;
        public string NamePattern;   // "{0}nin peyniri": {0} ilk bulanın adı
        public int UnlockLevel = 8;
        /// <summary>O beceride ek XP çarpanı ve ek ürün şansı.</summary>
        public double XpBonus = 0.10, ExtraYield = 0.10;
    }

    public sealed class FamilyRecipe
    {
        public string DefId;
        public string Name;
        public int FounderId;
        public double FoundedDay;
        public bool Written;
        public bool Lost;
        /// <summary>Tarifi bilen kişiler (sözlü aktarım).</summary>
        public List<int> Knowers = new List<int>();
        /// <summary>Kaç kuşaktır ailede (bilenlerin en yüksek ve en düşük kuşak farkı + 1).</summary>
        public int Generations = 1;
    }

    /// <summary>
    /// Aile tarifleri (belge 34, D1): ustalıkta açılır; sözlü aktarılır (usta ölmeden öğretilmezse kaybolur)
    /// ya da Okuryazarlık 3+ olan biri boş defterle yazıya geçirir (yazılı tarif kaybolmaz, okuma bilen herkes öğrenir).
    /// </summary>
    public sealed class FamilyRecipeBook
    {
        public const int WriteLiteracy = 3, ReadLiteracy = 2, TeachingWriteLevel = 8;
        public const string BlankBookItem = "item.blank_book";

        private readonly Population _population;
        private readonly Dictionary<string, FamilyRecipeDef> _defs = new Dictionary<string, FamilyRecipeDef>();
        private readonly List<FamilyRecipe> _recipes = new List<FamilyRecipe>();

        public FamilyRecipeBook(Population population, IEnumerable<FamilyRecipeDef> defs = null)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            foreach (var d in defs ?? DefaultDefs()) _defs[d.Id] = d;
        }

        public event Action<Character, FamilyRecipe> Founded;
        public event Action<FamilyRecipe> LostForever;

        public IReadOnlyList<FamilyRecipe> Recipes => _recipes;
        public FamilyRecipeDef Def(string id) => id != null && _defs.TryGetValue(id, out var d) ? d : null;

        public FamilyRecipe Get(string defId)
        {
            foreach (var r in _recipes) if (r.DefId == defId) return r;
            return null;
        }

        public bool Knows(int characterId, FamilyRecipe r) => r != null && !r.Lost && r.Knowers.Contains(characterId);

        /// <summary>Beceri seviyesi değişti: açılış seviyesine ulaşan hane üyesi henüz yoksa tarifi bulur.</summary>
        public FamilyRecipe OnLevel(Character c, SkillType skill, int level, double today)
        {
            if (c == null || c.IsDead || c.IsVillager) return null;
            foreach (var d in _defs.Values)
            {
                if (d.Skill != skill || level < d.UnlockLevel) continue;
                var existing = Get(d.Id);
                if (existing != null && !existing.Lost) continue;
                if (existing != null) _recipes.Remove(existing);
                var r = new FamilyRecipe { DefId = d.Id, Name = string.Format(d.NamePattern, c.Name), FounderId = c.Id, FoundedDay = today };
                r.Knowers.Add(c.Id);
                _recipes.Add(r);
                Founded?.Invoke(c, r);
                return r;
            }
            return null;
        }

        /// <summary>Sözlü öğretme: bilen, bilmeyene (çocuk dahil) öğretir. Yazılıysa okuma bilen kendisi de öğrenebilir.</summary>
        public string TeachBlocker(Character knower, Character learner, FamilyRecipe r)
        {
            if (r == null || r.Lost) return "Tarif yok.";
            if (learner == null || learner.IsDead || learner.Stage < AgeStage.Child) return "Öğrenecek kimse yok.";
            if (Knows(learner.Id, r)) return "Zaten biliyor.";
            if (knower == null)
                return r.Written && learner.Skills.GetLevel(SkillType.Literacy) >= ReadLiteracy ? null : "Okumak için Okuryazarlık 2 gerekir.";
            if (!Knows(knower.Id, r) || knower.IsDead) return $"{knower.Name} tarifi bilmiyor.";
            return null;
        }

        public bool Teach(Character knower, Character learner, FamilyRecipe r)
        {
            if (TeachBlocker(knower, learner, r) != null) return false;
            r.Knowers.Add(learner.Id);
            int lo = int.MaxValue, hi = 0;
            var founder = _population.Find(r.FounderId);
            if (founder != null) { lo = Math.Min(lo, founder.Generation); hi = Math.Max(hi, founder.Generation); }
            foreach (var id in r.Knowers)
            {
                var k = _population.Find(id);
                if (k == null) continue;
                lo = Math.Min(lo, k.Generation); hi = Math.Max(hi, k.Generation);
            }
            if (lo != int.MaxValue) r.Generations = Math.Max(r.Generations, hi - lo + 1);
            learner.GainSkillXp(DefSkill(r), 15);
            return true;
        }

        /// <summary>Yazıya geçirme engeli (null: yazılabilir). hasBlankBook: hanede boş defter var mı.</summary>
        public string WriteBlocker(Character writer, FamilyRecipe r, bool hasBlankBook)
        {
            if (r == null || r.Lost) return "Tarif yok.";
            if (r.Written) return "Zaten yazılı.";
            if (writer == null || !Knows(writer.Id, r)) return "Yazan tarifi bilmeli.";
            if (writer.Skills.GetLevel(SkillType.Literacy) < WriteLiteracy) return "Yazmak için Okuryazarlık 3 gerekir.";
            if (!hasBlankBook) return "Boş defter gerekir.";
            return null;
        }

        public bool Write(Character writer, FamilyRecipe r, bool hasBlankBook)
        {
            if (WriteBlocker(writer, r, hasBlankBook) != null) return false;
            r.Written = true;
            writer.GainSkillXp(SkillType.Literacy, 10);
            // Öğretmenlik 8 (belge 45): öğretici yazım; hanede okuyabilen herkes tarifi hemen öğrenir.
            if (writer.Skills.GetLevel(SkillType.Teaching) >= TeachingWriteLevel)
                foreach (var m in _population.Members)
                    if (m != writer && !m.IsDead && !m.IsVillager && TeachBlocker(null, m, r) == null) Teach(null, m, r);
            return true;
        }

        /// <summary>Ölüm: bilen kalmadıysa ve yazılı değilse tarif kaybolur.</summary>
        public List<FamilyRecipe> OnDied(Character c)
        {
            var lost = new List<FamilyRecipe>();
            if (c == null) return lost;
            foreach (var r in _recipes)
            {
                if (r.Lost || !r.Knowers.Remove(c.Id)) continue;
                if (r.Written || AliveKnowers(r) > 0) continue;
                r.Lost = true;
                lost.Add(r);
                LostForever?.Invoke(r);
            }
            return lost;
        }

        public int AliveKnowers(FamilyRecipe r)
        {
            int n = 0;
            foreach (var id in r.Knowers) { var k = _population.Find(id); if (k != null && !k.IsDead) n++; }
            return n;
        }

        /// <summary>Yaşayan biri sözlü tarifin tek bileniyse uyarı metni (yaşlıysa): "aktarılmazsa kaybolacak".</summary>
        public List<string> AtRiskWarnings()
        {
            var list = new List<string>();
            foreach (var r in _recipes)
            {
                if (r.Lost || r.Written || AliveKnowers(r) != 1) continue;
                foreach (var id in r.Knowers)
                {
                    var k = _population.Find(id);
                    if (k != null && !k.IsDead && k.Stage == AgeStage.Elder)
                        list.Add($"{r.Name}: yalnızca {k.Name} biliyor. Öğretilmez ya da yazılmazsa kaybolacak.");
                }
            }
            return list;
        }

        private SkillType DefSkill(FamilyRecipe r) => Def(r.DefId)?.Skill ?? SkillType.Cooking;

        /// <summary>Tarifi bilenin o beceride XP çarpanı.</summary>
        public double XpFactor(int characterId, SkillType skill)
        {
            double f = 1.0;
            foreach (var r in _recipes)
            {
                var d = Def(r.DefId);
                if (d != null && d.Skill == skill && Knows(characterId, r)) f += d.XpBonus;
            }
            return f;
        }

        /// <summary>Hanede yaşayan biri tarifi biliyorsa o beceride ek ürün şansı.</summary>
        public double ExtraYield(SkillType skill)
        {
            double y = 0;
            foreach (var r in _recipes)
            {
                var d = Def(r.DefId);
                if (d == null || d.Skill != skill || r.Lost || AliveKnowers(r) == 0) continue;
                y += d.ExtraYield;
            }
            return y;
        }

        public static List<FamilyRecipeDef> DefaultDefs() => new List<FamilyRecipeDef>
        {
            new FamilyRecipeDef { Id = "recipe.family_cheese", Skill = SkillType.Cooking, NamePattern = "{0}nin peyniri" },
            new FamilyRecipeDef { Id = "recipe.family_ale", Skill = SkillType.Brewing, NamePattern = "{0}nin birası" },
            new FamilyRecipeDef { Id = "recipe.family_sourdough", Skill = SkillType.Baking, NamePattern = "Aile ekşi mayası ({0})" },
            new FamilyRecipeDef { Id = "recipe.family_salve", Skill = SkillType.Herbalism, NamePattern = "{0}nin merhemi" },
            new FamilyRecipeDef { Id = "recipe.family_pattern", Skill = SkillType.Weaving, NamePattern = "{0}nin deseni" },
            new FamilyRecipeDef { Id = "recipe.family_seed", Skill = SkillType.Horticulture, NamePattern = "{0}nin tohumu" },
            new FamilyRecipeDef { Id = "recipe.family_forge", Skill = SkillType.Smithing, NamePattern = "{0}nin su vermesi" }
        };

        public FamilyRecipeSave[] Export()
        {
            var l = new List<FamilyRecipeSave>();
            foreach (var r in _recipes)
                l.Add(new FamilyRecipeSave { defId = r.DefId, name = r.Name, founderId = r.FounderId, foundedDay = r.FoundedDay, written = r.Written, lost = r.Lost, knowers = r.Knowers.ToArray(), generations = r.Generations });
            return l.ToArray();
        }

        public void Import(FamilyRecipeSave[] rows)
        {
            _recipes.Clear();
            if (rows == null) return;
            foreach (var s in rows)
                if (s != null)
                    _recipes.Add(new FamilyRecipe
                    {
                        DefId = s.defId, Name = s.name, FounderId = s.founderId, FoundedDay = s.foundedDay, Written = s.written, Lost = s.lost,
                        Knowers = s.knowers != null ? new List<int>(s.knowers) : new List<int>(), Generations = Math.Max(1, s.generations)
                    });
        }
    }

    [Serializable] public sealed class ApprenticeshipSave { public int masterId, apprenticeId, skill; public double startDay; }
    [Serializable] public sealed class FamilyRecipeSave { public string defId, name; public int founderId, generations; public double foundedDay; public bool written, lost; public int[] knowers; }
}
