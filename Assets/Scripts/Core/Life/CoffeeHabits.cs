using System;
using System.Collections.Generic;
using System.Globalization;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Kahve alışkanlığı (belge 32-6):
    /// <list type="bullet">
    /// <item>Üst üste <see cref="HabitAfterDays"/> gün kahve içen karakterde alışkanlık başlar.</item>
    /// <item>Alışkanlığı olan karakter sabah kahve içmezse "Mahmur" olur (3 saat); kahve içince hemen kalkar.</item>
    /// <item>O gün hiç içmezse akşamüstü "Baş ağrısı" (bir gün).</item>
    /// <item><see cref="QuitAfterDays"/> gün hiç içmezse alışkanlık geçer.</item>
    /// <item>Akşam içilen kahve uykuyu kaçırır.</item>
    /// </list>
    /// Kahve falı (cezve) kısa, eğlenceli bir yorum döner; gerçek etkisi yoktur.
    /// </summary>
    public sealed class CoffeeHabits
    {
        public const int HabitAfterDays = 4;
        public const int QuitAfterDays = 3;
        /// <summary>Uyanma sonrası Mahmur'un düştüğü an (günün kesri; uyanış ~0,25).</summary>
        public const double MorningCheck = 0.30;
        /// <summary>Baş ağrısının başladığı akşamüstü.</summary>
        public const double HeadacheAt = 0.70;
        /// <summary>Bu saatten sonra içilen kahve uykuyu kaçırır.</summary>
        public const double LateAfter = 0.75;

        public const string Tag = "coffee";
        public const string MahmurId = "eff_mahmur";
        public const string HeadacheId = "eff_headache";
        public const string LateId = "eff_coffee_late";

        private sealed class State
        {
            public int LastDay = -1;      // en son kahve içilen gün
            public int Streak;            // üst üste günler
            public bool Habit;
            public int MorningDay = -1;   // Mahmur denetiminin yapıldığı son gün
            public int HeadacheDay = -1;
        }

        private readonly Dictionary<int, State> _states = new Dictionary<int, State>();
        private readonly Random _rng;

        public CoffeeHabits(int seed = 509) { _rng = new Random(seed); }

        public event Action<Character, string> Notice;

        private State Get(int id) { if (!_states.TryGetValue(id, out var s)) _states[id] = s = new State(); return s; }

        public bool HasHabit(Character c) => c != null && _states.TryGetValue(c.Id, out var s) && s.Habit;
        public int Streak(Character c) => c != null && _states.TryGetValue(c.Id, out var s) ? s.Streak : 0;
        public bool DrankToday(Character c, int day) => c != null && _states.TryGetValue(c.Id, out var s) && s.LastDay == day;

        public static bool CanDrink(Character c) =>
            c != null && !c.IsDead && c.Stage != AgeStage.Baby && c.Stage != AgeStage.Child && c.Stage != AgeStage.Teen;

        /// <summary>Kahve içildi: seri ilerler, Mahmur kalkar, akşamsa uyku kaçar.</summary>
        public void OnCoffee(Character c, double now)
        {
            if (!CanDrink(c)) return;
            int day = (int)Math.Floor(now);
            var s = Get(c.Id);
            if (s.LastDay != day)
            {
                s.Streak = s.LastDay == day - 1 ? s.Streak + 1 : 1;
                s.LastDay = day;
                if (!s.Habit && s.Streak >= HabitAfterDays)
                {
                    s.Habit = true;
                    Notice?.Invoke(c, $"{c.Name} kahveye alıştı; sabah kahvesi olmadan kendine gelemiyor.");
                }
            }
            c.Mood.Remove(MahmurId);
            if (now - Math.Floor(now) >= LateAfter) c.AddMoodlet(LateId);
        }

        /// <summary>Sürekli: sabah Mahmur, akşamüstü baş ağrısı.</summary>
        public void Tick(double now, IEnumerable<Character> members)
        {
            int day = (int)Math.Floor(now);
            double frac = now - day;
            foreach (var c in members)
            {
                if (c.IsDead || c.IsVillager || !_states.TryGetValue(c.Id, out var s) || !s.Habit) continue;
                if (frac >= MorningCheck && s.MorningDay < day)
                {
                    s.MorningDay = day;
                    if (s.LastDay < day && c.AddMoodlet(MahmurId)) Notice?.Invoke(c, $"{c.Name} mahmur; bir fincan kahve iyi gelir.");
                }
                if (frac >= HeadacheAt && s.HeadacheDay < day && s.LastDay < day)
                {
                    s.HeadacheDay = day;
                    if (c.AddMoodlet(HeadacheId)) Notice?.Invoke(c, $"{c.Name} bütün gün kahve içmedi; başı ağrıyor.");
                }
            }
        }

        /// <summary>Günlük: üç gün hiç içmeyenin alışkanlığı geçer.</summary>
        public void OnNewDay(int day, Func<int, Character> find)
        {
            foreach (var kv in _states)
            {
                var s = kv.Value;
                if (!s.Habit || day - s.LastDay <= QuitAfterDays) continue;
                s.Habit = false; s.Streak = 0;
                var c = find?.Invoke(kv.Key);
                if (c != null && !c.IsDead) Notice?.Invoke(c, $"{c.Name} kahveyi bıraktı; artık sabahları kendiliğinden uyanıyor.");
            }
        }

        private static readonly string[] Fortunes =
        {
            "Fincanda bir yol var: yakında uzaktan bir haber gelecek.",
            "Telvede bir kuş görünüyor: sevindirici bir söz duyacaksın.",
            "Dibinde bir yüzük şekli: biri sana gönül vermiş.",
            "Kenarda bir dağ: önündeki iş zor ama aşılır.",
            "Bir balık beliriyor: kesene bereket girecek.",
            "Telve dağılmış: kafan karışık, bir gün dinlen.",
            "Bir kapı görünüyor: beklemediğin bir misafir gelecek.",
            "Ortada bir ağaç: aile büyüyecek, ocak tütecek.",
            "Bir at koşuyor: yolculuk ya da şenlik yakın.",
            "Fincan tertemiz: dileğin olacak.",
        };

        /// <summary>Kahve falı: kısa, eğlenceli bir yorum (gerçek etkisi yok).</summary>
        public string Fortune() => Fortunes[_rng.Next(Fortunes.Length)];

        // id:son gün:seri:alışkanlık(0/1)
        public string[] Export()
        {
            var l = new List<string>();
            foreach (var kv in _states)
                l.Add(string.Join(":", kv.Key.ToString(CultureInfo.InvariantCulture), kv.Value.LastDay.ToString(CultureInfo.InvariantCulture),
                    kv.Value.Streak.ToString(CultureInfo.InvariantCulture), kv.Value.Habit ? "1" : "0"));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _states.Clear();
            if (data == null) return;
            foreach (var e in data)
            {
                var p = (e ?? "").Split(':');
                if (p.Length < 4 || !int.TryParse(p[0], out int id)) continue;
                var s = Get(id);
                int.TryParse(p[1], out s.LastDay);
                int.TryParse(p[2], out s.Streak);
                s.Habit = p[3] == "1";
            }
        }
    }
}
