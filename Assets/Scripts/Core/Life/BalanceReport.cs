using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Denge simülasyonu raporu (belge 41): uzun hızlı oynatmada günde bir (ya da daha sık) örnek alır; sonunda CSV ve
    /// okunur özet verir. Oyun katmanı (Editor menüsü ya da MCP) dünyayı ilerletir, bu sınıf yalnızca kaydeder ve yorumlar.
    /// Özet: ilk ölüm (gün, neden), açlık/soğuk sıkıntısı yaşanan gün sayısı, yiyecek ve odunun en düşük değeri,
    /// akçe değişimi, beceri seviyesi kazancı, ortalama ruh hali.
    /// </summary>
    public sealed class BalanceReport
    {
        /// <summary>İhtiyaç bunun altındaysa o örnekte "sıkıntı" sayılır.</summary>
        public const float DistressBelow = 15f;

        public sealed class MemberSample
        {
            public int Id;
            public string Name;
            public bool Dead;
            public DeathCause Cause;
            public float Hunger, Energy, Warmth, Hygiene, Fun;
            public int Mood;
            public double StarveDays, FreezeDays;
            public int SkillLevels;
            public string Activity;
        }

        public sealed class Sample
        {
            public double Day;
            public string Season;
            public float Outdoor;
            public int Akce;
            public double Food;
            public int Firewood;
            public List<MemberSample> Members = new List<MemberSample>();
        }

        private readonly List<Sample> _samples = new List<Sample>();
        public IReadOnlyList<Sample> Samples => _samples;
        /// <summary>Senaryo adı (rapor başlığı).</summary>
        public string Scenario { get; set; } = "denge";

        public static int SkillLevelTotal(Character c)
        {
            int n = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType))) n += c.Skills.GetLevel(s);
            return n;
        }

        /// <summary>Bir örnek alır. Köylüler sayılmaz; ölmüş hane üyesi (listede kaldıysa) ölü olarak yazılır.</summary>
        public Sample Add(double day, string season, float outdoor, Household house, double food, int firewood,
            IEnumerable<Character> members, Func<Character, string> activity = null)
        {
            var s = new Sample { Day = day, Season = season, Outdoor = outdoor, Akce = house?.Akce ?? 0, Food = food, Firewood = firewood };
            foreach (var c in members)
            {
                if (c == null || c.IsVillager) continue;
                s.Members.Add(new MemberSample
                {
                    Id = c.Id, Name = c.Name, Dead = c.IsDead, Cause = c.CauseOfDeath,
                    Hunger = c.Needs.Get(NeedType.Hunger), Energy = c.Needs.Get(NeedType.Energy), Warmth = c.Needs.Get(NeedType.Warmth),
                    Hygiene = c.Needs.Get(NeedType.Hygiene), Fun = c.Needs.Get(NeedType.Fun),
                    Mood = c.Mood.Total, StarveDays = c.StarveDays, FreezeDays = c.FreezeDays,
                    SkillLevels = SkillLevelTotal(c), Activity = c.IsDead ? "" : activity?.Invoke(c) ?? ""
                });
            }
            _samples.Add(s);
            return s;
        }

        private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        private static string Csv(string v) => v == null ? "" : v.IndexOfAny(new[] { ',', '"', '\n' }) >= 0 ? "\"" + v.Replace("\"", "\"\"") + "\"" : v;

        /// <summary>Uzun biçim: her satır bir (örnek, üye).</summary>
        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.Append("day,season,outdoor,akce,food,firewood,member,dead,cause,hunger,energy,warmth,hygiene,fun,mood,starve,freeze,skills,activity\n");
            foreach (var s in _samples)
            {
                if (s.Members.Count == 0)
                {
                    sb.Append($"{F(s.Day)},{s.Season},{F(s.Outdoor)},{s.Akce},{F(s.Food)},{s.Firewood},,,,,,,,,,,,,\n");
                    continue;
                }
                foreach (var m in s.Members)
                    sb.Append($"{F(s.Day)},{s.Season},{F(s.Outdoor)},{s.Akce},{F(s.Food)},{s.Firewood},{Csv(m.Name)},{(m.Dead ? 1 : 0)},{(m.Dead ? m.Cause.ToString() : "")},"
                        + $"{F(m.Hunger)},{F(m.Energy)},{F(m.Warmth)},{F(m.Hygiene)},{F(m.Fun)},{m.Mood},{F(m.StarveDays)},{F(m.FreezeDays)},{m.SkillLevels},{Csv(m.Activity)}\n");
            }
            return sb.ToString();
        }

        /// <summary>İlk ölüm (gün, ad, neden); yoksa null.</summary>
        public (double day, string name, DeathCause cause)? FirstDeath()
        {
            foreach (var s in _samples)
                foreach (var m in s.Members)
                    if (m.Dead) return (s.Day, m.Name, m.Cause);
            return null;
        }

        /// <summary>Örneklerin kaçında birinin açlığı / ısınması DistressBelow altındaydı (yaşayanlar).</summary>
        public (int hungry, int cold, int total) DistressSamples()
        {
            int h = 0, c = 0;
            foreach (var s in _samples)
            {
                bool hs = false, cs = false;
                foreach (var m in s.Members)
                {
                    if (m.Dead) continue;
                    if (m.Hunger < DistressBelow) hs = true;
                    if (m.Warmth < DistressBelow) cs = true;
                }
                if (hs) h++;
                if (cs) c++;
            }
            return (h, c, _samples.Count);
        }

        public string Summary()
        {
            var sb = new StringBuilder();
            sb.Append($"== {Scenario} ==\n");
            if (_samples.Count == 0) { sb.Append("örnek yok\n"); return sb.ToString(); }
            var first = _samples[0];
            var last = _samples[_samples.Count - 1];
            sb.Append($"Gün {F(first.Day)} → {F(last.Day)} ({_samples.Count} örnek)\n");
            var death = FirstDeath();
            sb.Append(death.HasValue ? $"İlk ölüm: gün {F(death.Value.day)}, {death.Value.name} ({death.Value.cause})\n" : "Ölüm yok\n");
            var (hungry, cold, total) = DistressSamples();
            sb.Append($"Sıkıntı (<{DistressBelow}): açlık {hungry}/{total} örnek, soğuk {cold}/{total} örnek\n");
            double minFood = double.MaxValue, minFoodDay = 0; int minWood = int.MaxValue; double minWoodDay = 0;
            foreach (var s in _samples)
            {
                if (s.Food < minFood) { minFood = s.Food; minFoodDay = s.Day; }
                if (s.Firewood < minWood) { minWood = s.Firewood; minWoodDay = s.Day; }
            }
            sb.Append($"Yiyecek: {F(first.Food)} → {F(last.Food)} (en düşük {F(minFood)}, gün {F(minFoodDay)})\n");
            sb.Append($"Odun: {first.Firewood} → {last.Firewood} (en düşük {minWood}, gün {F(minWoodDay)})\n");
            sb.Append($"Akçe: {first.Akce} → {last.Akce} ({(last.Akce - first.Akce >= 0 ? "+" : "")}{last.Akce - first.Akce})\n");
            foreach (var m0 in first.Members)
            {
                MemberSample m1 = null;
                foreach (var m in last.Members) if (m.Id == m0.Id) m1 = m;
                double moodSum = 0; int n = 0;
                foreach (var s in _samples) foreach (var m in s.Members) if (m.Id == m0.Id && !m.Dead) { moodSum += m.Mood; n++; }
                string state = m1 == null ? "listeden çıktı" : m1.Dead ? $"öldü ({m1.Cause})" : "yaşıyor";
                int gain = m1 == null ? 0 : m1.SkillLevels - m0.SkillLevels;
                sb.Append($"  {m0.Name}: {state}; beceri seviyesi +{gain}; ort. ruh hali {(n > 0 ? F(moodSum / n) : "-")}\n");
            }
            return sb.ToString();
        }
    }
}
