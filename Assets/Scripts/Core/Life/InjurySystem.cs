using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Life
{
    /// <summary>Kaza sıklığı: oyuncu yeni oyunda seçer (karar 17.11.2026). NotSet eski kayıt demektir: Seyrek sayılır.</summary>
    public enum AccidentSetting
    {
        NotSet = 0,
        Off = 1,      // Kapalı
        Rare = 2,     // Seyrek
        Normal = 3    // Normal
    }

    /// <summary>Bir kaza türü: hangi işlerde olur, nereye denk gelir, iz kalırsa deseni.</summary>
    public sealed class InjuryDef
    {
        public string Id { get; set; }
        public HashSet<SkillType> Skills { get; set; } = new HashSet<SkillType>();
        /// <summary>Bölge ve ağırlığı.</summary>
        public List<KeyValuePair<string, int>> Zones { get; set; } = new List<KeyValuePair<string, int>>();
        public string Pattern { get; set; }
        public double BaseChance { get; set; }
        public double HealDays { get; set; }
        public string MoodletId { get; set; }
        /// <summary>Neden metni; {0} bölgenin belirtme hâli ("elini").</summary>
        public string ReasonFormat { get; set; }
    }

    /// <summary>
    /// Kazalar ve kazanılmış izler (proje dokümanı 14, bölüm 3F).
    /// - İş bitince küçük olasılıkla kaza: taban × ayar × beceri × ruh hali × alet × hava × tecrübe.
    /// - Kaza: "Yaralı" moodlet'i (ilgili beceride yavaşlık), iyileşme günleri.
    /// - İyileşince iz kalabilir: %30 × 0,5^(kazadan kalmış iz sayısı); hekim ya da merhem üçte bire indirir, derin yara kesin.
    /// - Sınırlar: karakter başına mevsimde bir, hanede günde bir kaza; bebek ve köylüde yok; ölümcül kaza yok.
    /// </summary>
    public sealed class InjurySystem
    {
        public const double ScarChance = 0.30;
        public const double ScarChancePerScar = 0.5;
        public const double AccidentPerScar = 0.8;
        public const double TreatedFactor = 1.0 / 3.0;
        public const double DeepChance = 0.10;
        public const double AthleticHealPerLevel = 0.04, AthleticHealMax = 0.30;

        /// <summary>İyileşme süresi çarpanı: Atletizm 1'de 1,0; her seviye %4 kısaltır, en çok %30.</summary>
        public static double AthleticHealFactor(Character c) =>
            1.0 - Math.Min(AthleticHealMax, AthleticHealPerLevel * Math.Max(0, c.Skills.GetLevel(SkillType.Athletics) - 1));
        public const int HealerLevel = 3;
        public const double HealerHealFactor = 0.7;

        private readonly Population _population;
        private readonly Random _rng;
        private readonly Dictionary<string, InjuryDef> _defs;
        private double _lastHouseholdAccidentDay = -1;

        public AccidentSetting Setting { get; set; } = AccidentSetting.NotSet;
        /// <summary>Bir karakterin iki kazası arasındaki en az gün (bir mevsim).</summary>
        public double MinDaysBetween { get; set; } = 10;
        /// <summary>Bugünün oyun günü.</summary>
        public Func<double> Today { get; set; } = () => 0;
        /// <summary>Karakter dışarıda mı (oda yok ya da dış mekân).</summary>
        public Func<Character, bool> IsOutdoors { get; set; } = _ => true;
        /// <summary>Bugünün hava çarpanı (fırtına 2, kar 1,5); yalnızca dışarıda uygulanır.</summary>
        public Func<double> WeatherFactor { get; set; } = () => 1.0;
        /// <summary>Kişiye özel şans çarpanı (nazar boncuğu 0,9; taşlı nazarlık 0,8): kaza ve iz kalma olasılığını çarpar.</summary>
        public Func<Character, double> LuckFactor { get; set; }
        /// <summary>Aletin hız çarpanı (iyi alet daha az kaza).</summary>
        public Func<string, double> ToolSpeed { get; set; } = _ => 1.0;
        /// <summary>Bildirim (metin, karakter).</summary>
        public Action<string, int> Notify { get; set; }

        public event Action<Character, ActiveInjury> Injured;
        public event Action<Character, Mark> Scarred;

        public InjurySystem(Population population, int seed = 0)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _rng = seed != 0 ? new Random(seed) : new Random(7);
            _defs = Catalog().ToDictionary(d => d.Id);
        }

        public IEnumerable<InjuryDef> Defs => _defs.Values;

        public static List<InjuryDef> Catalog()
        {
            InjuryDef D(string id, string pattern, double chance, double heal, string mood, string reason, IEnumerable<SkillType> skills, params (string, int)[] zones) =>
                new InjuryDef
                {
                    Id = id, Pattern = pattern, BaseChance = chance, HealDays = heal, MoodletId = mood, ReasonFormat = reason,
                    Skills = new HashSet<SkillType>(skills ?? Enumerable.Empty<SkillType>()),
                    Zones = zones.Select(z => new KeyValuePair<string, int>(z.Item1, z.Item2)).ToList()
                };
            return new List<InjuryDef>
            {
                D("axe", "split", 0.004, 3, MoodCatalog.InjuredAxe, "baltayla {0} kesti",
                    new[] { SkillType.Woodcutting, SkillType.Carpentry }, ("hand", 50), ("larm", 20), ("rarm", 20), ("cheek", 10)),
                D("burn", "burn", 0.004, 3, MoodCatalog.InjuredBurn, "ocakta {0} yaktı",
                    new[] { SkillType.Cooking, SkillType.Smithing, SkillType.Baking, SkillType.Chandlery, SkillType.Pottery, SkillType.Brewing }, ("hand", 60), ("larm", 20), ("rarm", 20)),
                D("animal", "bite", 0.004, 3, MoodCatalog.InjuredAnimal, "bir hayvan {0} ısırdı",
                    new[] { SkillType.AnimalHusbandry, SkillType.Riding, SkillType.Hunting }, ("larm", 35), ("rarm", 35), ("hand", 20), ("cheek", 10)),
                D("sickle", "scratch", 0.003, 2, MoodCatalog.InjuredSickle, "orakla {0} kesti",
                    new[] { SkillType.Farming, SkillType.Horticulture, SkillType.Foraging }, ("hand", 100)),
                // Düşme: dışarıda, yalnızca kar ya da fırtına çarpanıyla (WeatherFactor > 1) anlamlıdır.
                D("fall", "split", 0.003, 2, MoodCatalog.InjuredFall, "kaygan yerde düşüp {0} yardı",
                    null, ("brow", 40), ("forehead", 35), ("lip", 25)),
                D("child_fall", "scratch", 0.006, 1, MoodCatalog.InjuredFall, "oynarken düşüp {0} sıyırdı",
                    null, ("forehead", 60), ("brow", 40)),
                // Attan düşme (29): iş zarıyla gelmez; başarısız atlamada AnimalSystem.RiderFell ile verilir. Ölümcül değildir.
                D("horse_fall", "split", 0, 2, MoodCatalog.InjuredFall, "attan düşüp {0} yardı",
                    null, ("brow", 30), ("forehead", 25), ("larm", 25), ("rarm", 20)),
                // Arı sokması (32-8.2): iş zarıyla gelmez; bal toplarken Hayvancılık düşükse LifeSystems verir. Küçük, ölümcül değil.
                D("bee_sting", "scratch", 0, 1, MoodCatalog.InjuredAnimal, "arı {0} soktu",
                    null, ("hand", 50), ("cheek", 30), ("neck", 20)),
                // Göçük: iş zarıyla gelmez (şans 0); kazı motoru çöken karodaki karaktere doğrudan verir.
                D("cave_in", "split", 0, 4, MoodCatalog.InjuredCaveIn, "göçükte {0} yardı",
                    null, ("forehead", 30), ("larm", 25), ("rarm", 25), ("brow", 20))
            };
        }

        public static string ZoneObject(string zone)
        {
            switch (zone)
            {
                case "hand": return "elini";
                case "larm": return "sol kolunu";
                case "rarm": return "sağ kolunu";
                case "cheek": return "yanağını";
                case "brow": return "kaşını";
                case "forehead": return "alnını";
                case "lip": return "dudağını";
                case "neck": return "boynunu";
                default: return "bir yerini";
            }
        }

        public static double SettingFactor(AccidentSetting s)
        {
            switch (s)
            {
                case AccidentSetting.Off: return 0;
                case AccidentSetting.Normal: return 1.0;
                default: return 0.4;   // Seyrek ve belirsiz (eski kayıt)
            }
        }

        /// <summary>Beceri çarpanı: 1. seviyede 2, 10. seviyede 0,2 (usta nadiren yaralanır).</summary>
        public static double SkillFactor(int level) => 2.0 * Math.Pow(0.1, (Math.Max(1, Math.Min(10, level)) - 1) / 9.0);

        public static double MoodFactor(MoodStatus s)
        {
            switch (s)
            {
                case MoodStatus.Miserable: return 1.5;
                case MoodStatus.Unhappy: return 1.2;
                case MoodStatus.Good: return 0.9;
                case MoodStatus.Happy: return 0.8;
                default: return 1.0;
            }
        }

        /// <summary>Bu kişinin bu işte kaza geçirebilir olması (sınırlar).</summary>
        public bool CanHaveAccident(Character c)
        {
            if (c == null || c.IsDead || c.IsVillager || c.Stage == AgeStage.Baby) return false;
            if (Setting == AccidentSetting.Off) return false;
            if (c.Injury != null) return false;
            double today = Today();
            if (c.LastAccidentDay >= 0 && today - c.LastAccidentDay < MinDaysBetween) return false;
            if (_lastHouseholdAccidentDay >= 0 && Math.Floor(today) == Math.Floor(_lastHouseholdAccidentDay)) return false;
            return true;
        }

        /// <summary>Bu iş için uygun kaza türü (yoksa null).</summary>
        public InjuryDef DefFor(Character c, InteractionDef interaction, bool outdoors)
        {
            if (interaction == null) return null;
            if (interaction.Skill.HasValue)
            {
                var d = _defs.Values.FirstOrDefault(x => x.Skills.Contains(interaction.Skill.Value));
                if (d != null) return d;
            }
            if (!outdoors) return null;
            if (c.Stage == AgeStage.Child && !interaction.Skill.HasValue) return _defs["child_fall"];
            if (WeatherFactor() > 1.0) return _defs["fall"];
            return null;
        }

        /// <summary>Olasılık (test ve denge için açık).</summary>
        public double Chance(Character c, InteractionDef interaction, InjuryDef def, bool outdoors)
        {
            if (def == null) return 0;
            double p = def.BaseChance * SettingFactor(Setting);
            int level = interaction.Skill.HasValue ? c.Skills.GetLevel(interaction.Skill.Value) : 1;
            if (def.Id != "child_fall") p *= SkillFactor(level);
            p *= MoodFactor(c.MoodStatus);
            if (!string.IsNullOrEmpty(interaction.Tool))
            {
                double speed = Math.Max(0.5, Math.Min(2.0, ToolSpeed(interaction.Tool)));
                p /= speed;
            }
            if (outdoors) p *= Math.Max(1.0, WeatherFactor());
            p *= Math.Pow(AccidentPerScar, c.EventScarCount);
            if (LuckFactor != null) p *= Math.Max(0.0, LuckFactor(c));
            return Math.Max(0, Math.Min(1, p));
        }

        /// <summary>İş bitince çağrılır (JobRunner.InteractionCompleted). Kaza olduysa yarayı döner.</summary>
        public ActiveInjury OnWorkDone(Character c, InteractionDef interaction)
        {
            if (!CanHaveAccident(c)) return null;
            bool outdoors = IsOutdoors(c);
            var def = DefFor(c, interaction, outdoors);
            if (def == null) return null;
            if (_rng.NextDouble() >= Chance(c, interaction, def, outdoors)) return null;
            return Injure(c, def);
        }

        /// <summary>Kazayı uygular (testlerde doğrudan çağrılabilir).</summary>
        /// <summary>Katalogdaki kimlikle yara verir (attan düşme, göçük); ölü ya da bilinmeyen kimlikte null.</summary>
        public ActiveInjury InjureById(Character c, string id)
        {
            if (c == null || c.IsDead || id == null || !_defs.TryGetValue(id, out var def)) return null;
            return Injure(c, def);
        }

        public ActiveInjury Injure(Character c, InjuryDef def)
        {
            double today = Today();
            string zone = PickZone(def);
            var inj = new ActiveInjury
            {
                Kind = def.Id, Zone = zone, Pattern = def.Pattern, StartDay = today,
                HealDay = today + def.HealDays, Deep = _rng.NextDouble() < DeepChance,
                Reason = string.Format(def.ReasonFormat, ZoneObject(zone))
            };
            // Hanede 3. seviye hekim varsa yarayı sarar: iyileşme kısalır, iz şansı üçte bire iner.
            if (HasHealer(c))
            {
                inj.Treated = true;
                inj.HealDay = today + def.HealDays * HealerHealFactor;
            }
            // Atletizm: sağlam beden çabuk toparlanır (seviye başına %4, en çok %30 kısa).
            inj.HealDay = today + (inj.HealDay - today) * AthleticHealFactor(c);
            c.Injury = inj;
            c.LastAccidentDay = today;
            _lastHouseholdAccidentDay = today;
            if (c.MoodEnabled && MoodCatalog.Default.TryGet(def.MoodletId, out var md))
            {
                var mood = new MoodletDef { Id = md.Id, DisplayName = md.DisplayName, Description = $"{Cap(inj.Reason)}. İyileşince iz kalabilir.", Value = md.Value, DurationDays = Math.Max(0.1, inj.HealDay - today), AllSpeed = md.AllSpeed };
                foreach (var kv in md.SkillSpeed) mood.SkillSpeed[kv.Key] = kv.Value;
                c.Mood.Add(mood);
            }
            Notify?.Invoke($"{c.Name} {inj.Reason}.", c.Id);
            Injured?.Invoke(c, inj);
            return inj;
        }

        /// <summary>Şifalı merhem: iyileşmeyi yarıya, iz şansını üçte bire indirir. Yara yoksa false.</summary>
        public bool ApplySalve(Character c)
        {
            if (c?.Injury == null || c.Injury.Treated) return false;
            c.Injury.Treated = true;
            double left = c.Injury.HealDay - Today();
            if (left > 0) c.Injury.HealDay = Today() + left * 0.5;
            return true;
        }

        /// <summary>Gün başında: iyileşen yaralar ve iz tazeliği.</summary>
        public void DailyUpdate()
        {
            double today = Today();
            foreach (var c in _population.Members.ToList())
            {
                if (c.IsDead) continue;
                if (c.Injury != null && today >= c.Injury.HealDay) Heal(c);
                MarkRules.UpdateFade(c.Appearance.Marks, today);
            }
        }

        /// <summary>Yara iyileşir; iz kalırsa işareti döner.</summary>
        public Mark Heal(Character c)
        {
            var inj = c.Injury;
            if (inj == null) return null;
            c.Injury = null;
            if (_rng.NextDouble() >= ScarChanceFor(c, inj)) return null;
            var mark = new Mark { Kind = MarkKind.Scar, Id = inj.Pattern, Zone = inj.Zone, Source = MarkSource.Event, Day = inj.StartDay, Reason = inj.Reason, Fade = 0 };
            c.Appearance.Marks.Add(mark);
            Notify?.Invoke($"{c.Name}: yarası iyileşti, {ZoneText(inj.Zone)} iz kaldı.", c.Id);
            Scarred?.Invoke(c, mark);
            return mark;
        }

        /// <summary>İz kalma olasılığı: %30 × 0,5^(kazadan kalmış iz), tedavi üçte bir; derin yara kesin.</summary>
        public static double ScarChanceFor(Character c, ActiveInjury inj)
        {
            if (inj.Deep) return 1.0;
            double p = ScarChance * Math.Pow(ScarChancePerScar, c.EventScarCount);
            if (inj.Treated) p *= TreatedFactor;
            return p;
        }

        private static string ZoneText(string zone)
        {
            switch (zone)
            {
                case "hand": return "elinde";
                case "larm": return "sol kolunda";
                case "rarm": return "sağ kolunda";
                case "cheek": return "yanağında";
                case "brow": return "kaşında";
                case "forehead": return "alnında";
                case "lip": return "dudağında";
                case "neck": return "boynunda";
                default: return "bedeninde";
            }
        }

        private static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        private bool HasHealer(Character injured)
        {
            foreach (var m in _population.Members)
                if (!m.IsDead && !m.IsVillager && m.Skills.GetLevel(SkillType.Medicine) >= HealerLevel) return true;
            return false;
        }

        private string PickZone(InjuryDef def)
        {
            int total = def.Zones.Sum(z => z.Value);
            int r = _rng.Next(Math.Max(1, total));
            foreach (var z in def.Zones)
            {
                if (r < z.Value) return z.Key;
                r -= z.Value;
            }
            return def.Zones.Count > 0 ? def.Zones[0].Key : "hand";
        }

        // ---- kayıt ----
        public double LastHouseholdAccidentDay { get => _lastHouseholdAccidentDay; set => _lastHouseholdAccidentDay = value; }
    }
}
