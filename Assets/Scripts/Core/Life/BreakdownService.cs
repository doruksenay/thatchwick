using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Ruh hali uyarıları ve bunalım (karar 18.11.2026: bunalım var ve kapatılamaz; Project belgesi 23, bölüm 7.1).
    /// - Mutsuz'a düşünce bilgi, Perişan'a düşünce uyarı: "Wat bunalımın eşiğinde" + en kolay toparlanma yolu.
    /// - Bunalım yalnızca Perişan'da, en az yarım gün kaldıktan sonra; ortalama 2 günde bir; iki bunalım arası en az 2 gün.
    /// - Şiddetsiz, 2-6 saat: odasına kapanır, ağlar, meydana kaçar ya da bir öğün fazla yer (kış stoğu yetmiyorsa yemez).
    /// - Hayati güvenlik: açlık, uyku ya da soğuk kritikse başlamaz (sürerken kritikleşirse biter); bakılacak aç bebeğin
    ///   başka bakanı yoksa başlamaz; bebekte hiç, çocukta yalnızca ağlama.
    /// - Bitince "İçini döktü" (+20, 2 gün) art arda bunalımı önler (Character.EndBreakdown).
    /// Köylüler (belge 66f §B.9): ruh hali açık köylüde hafif biçimde işler. Uyarı yok; bunalım hep "odasına kapandı"
    /// (dükkânı erken kapatır, eve gider) ve süresi <see cref="VillagerDays"/> (life.json villageLife, öneri 1-2 gün).
    /// </summary>
    public sealed class BreakdownService
    {
        private readonly Population _population;
        private readonly Random _rng;
        private readonly Dictionary<int, int> _warned = new Dictionary<int, int>();          // kişi -> uyarılan basamak (0 yok, 1 Mutsuz, 2 Perişan)
        private readonly Dictionary<int, double> _lastWarnAge = new Dictionary<int, double>();
        /// <summary>Aynı kişiye aynı basamak uyarısı en erken bu kadar gün sonra yinelenir.</summary>
        public const double WarnRepeatDays = 1.0;
        /// <summary>Bebek bunun altında açsa ve başka bakan yoksa bakıcı bunalıma girmez.</summary>
        public const float BabyHungerCare = 45f;

        /// <summary>Köy meydanı varsa "Meydana kaçtı" seçilebilir.</summary>
        public Func<GridPos?> SquarePosition { get; set; }
        /// <summary>Kış stoğu yetmiyor mu (kış göstergesi kırmızı): öyleyse "Tıkındı" seçilmez.</summary>
        public Func<bool> FoodScarce { get; set; }

        /// <summary>Köylü bunalımının süresi (en az, en çok; oyun günü). Null ise köylü bunalıma girmez.</summary>
        public Func<(double min, double max)> VillagerDays { get; set; }

        /// <summary>Bildirim (düzey, metin, kişi).</summary>
        public event Action<NoticeLevel, string, int> Raised;
        /// <summary>Bunalım başladı (gün özeti için).</summary>
        public event Action<Character, BreakdownKind> Started;

        public BreakdownService(Population population, int seed = 83)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _rng = new Random(seed);
        }

        /// <summary>Uyarıları ve bunalım zarlarını işler; `gameDays` son çağrıdan bu yana geçen süre.</summary>
        public void Update(double gameDays)
        {
            var members = _population.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var c = members[i];
                if (c.IsDead || !c.MoodEnabled || c.Stage == AgeStage.Baby) continue;
                if (c.IsVillager)
                {
                    if (VillagerDays != null && c.Stage >= AgeStage.YoungAdult) TryStart(c, gameDays);
                    continue;
                }
                Warn(c);
                TryStart(c, gameDays);
            }
        }

        private void Warn(Character c)
        {
            var status = c.Mood.Status;
            int now = status == MoodStatus.Miserable ? 2 : status == MoodStatus.Unhappy ? 1 : 0;
            _warned.TryGetValue(c.Id, out int before);
            if (now > before)
            {
                bool recent = _lastWarnAge.TryGetValue(c.Id * 4 + now, out var last) && c.AgeDays - last < WarnRepeatDays;
                if (!recent)
                {
                    _lastWarnAge[c.Id * 4 + now] = c.AgeDays;
                    string hint = EasiestFix(c);
                    if (now == 2) Raised?.Invoke(NoticeLevel.Warning, $"{c.Name} bunalımın eşiğinde{hint}", c.Id);
                    else Raised?.Invoke(NoticeLevel.Info, $"{c.Name} mutsuz{hint}", c.Id);
                }
            }
            // Histerezis: Nötr'e çıkmadan yeniden uyarma.
            if (now > before || status >= MoodStatus.Neutral) _warned[c.Id] = now;
        }

        private void TryStart(Character c, double gameDays)
        {
            if (c.InBreakdown || gameDays <= 0) return;
            if (c.MiserableDays < MoodBehavior.BreakdownGraceDays) return;
            if (c.LastBreakdownAge >= 0 && c.AgeDays - c.LastBreakdownAge < MoodBehavior.BreakdownCooldownDays) return;
            if (!IsSafe(c)) return;
            double p = 1.0 - Math.Exp(-gameDays / MoodBehavior.BreakdownMeanDays);
            if (_rng.NextDouble() >= p) return;
            Start(c, PickKind(c));
        }

        /// <summary>Bunalımı hemen başlatır (testler ve olay zinciri için); güvenlik koşulları yine geçerli.</summary>
        public bool Start(Character c, BreakdownKind kind)
        {
            if (c == null || c.IsDead || c.InBreakdown || !IsSafe(c)) return false;
            if (c.IsVillager)
            {
                // Köylü: hafif bunalım, dükkânı kapatıp evine kapanır (belge 66f §B.9); bildirimi köy hayatı servisi verir.
                if (VillagerDays == null) return false;
                var (min, max) = VillagerDays();
                double vd = min + _rng.NextDouble() * Math.Max(0.0, max - min);
                if (!c.BeginBreakdown(BreakdownKind.Withdraw, vd)) return false;
                Started?.Invoke(c, BreakdownKind.Withdraw);
                return true;
            }
            if (c.Stage == AgeStage.Child) kind = BreakdownKind.Cry;
            if (kind == BreakdownKind.RunOff && SquarePosition?.Invoke() == null) kind = BreakdownKind.Cry;
            if (kind == BreakdownKind.Binge && FoodScarce != null && FoodScarce()) kind = BreakdownKind.Withdraw;
            double days = MoodBehavior.BreakdownMinDays + _rng.NextDouble() * (MoodBehavior.BreakdownMaxDays - MoodBehavior.BreakdownMinDays);
            if (!c.BeginBreakdown(kind, days)) return false;
            Raised?.Invoke(NoticeLevel.Warning, MoodBehavior.BreakdownNotice(c.Name, kind), c.Id);
            Started?.Invoke(c, kind);
            return true;
        }

        private BreakdownKind PickKind(Character c)
        {
            if (c.Stage == AgeStage.Child) return BreakdownKind.Cry;
            bool square = SquarePosition?.Invoke() != null;
            bool food = FoodScarce == null || !FoodScarce();
            int wWithdraw = 3, wCry = 3, wRun = square ? 2 : 0, wBinge = food ? 2 : 0;
            int roll = _rng.Next(wWithdraw + wCry + wRun + wBinge);
            if ((roll -= wWithdraw) < 0) return BreakdownKind.Withdraw;
            if ((roll -= wCry) < 0) return BreakdownKind.Cry;
            if ((roll -= wRun) < 0) return BreakdownKind.RunOff;
            return BreakdownKind.Binge;
        }

        /// <summary>
        /// Hayati güvenlik: açlık, uyku ya da soğuk kritikse bunalım olmaz; hanede aç bir bebek varsa ve ona bakabilecek
        /// başka biri (bunalımda olmayan, bebek ve çocuk olmayan) yoksa da olmaz.
        /// </summary>
        public bool IsSafe(Character c)
        {
            if (c.Stage == AgeStage.Baby || c.Needs.AnyCritical()) return false;
            if (c.IsVillager) return true;   // köylünün bakacağı hane bebeği yok
            foreach (var baby in _population.Members)
            {
                if (baby.IsDead || baby.IsVillager || baby.Stage != AgeStage.Baby) continue;
                if (baby.Needs.Get(NeedType.Hunger) >= BabyHungerCare) continue;
                bool otherCarer = false;
                foreach (var m in _population.Members)
                {
                    if (m == c || m.IsDead || m.IsVillager || m.InBreakdown) continue;
                    if (m.Stage == AgeStage.Baby || m.Stage == AgeStage.Child) continue;
                    otherCarer = true; break;
                }
                if (!otherCarer) return false;
            }
            return true;
        }

        /// <summary>En kolay toparlanma yolu: en ağır, giderilebilir olumsuz moodlet'in çaresi (": bir şeyler yesin").</summary>
        public static string EasiestFix(Character c)
        {
            ActiveMoodlet worst = null;
            string fix = null;
            foreach (var a in c.Mood.Active)
            {
                if (a.Def.Value >= 0) continue;
                string f = FixFor(a.Def.Id);
                if (f == null) continue;
                if (worst == null || a.Def.Value < worst.Def.Value) { worst = a; fix = f; }
            }
            if (worst != null) return $": {fix} ({worst.Def.DisplayName} {worst.Def.Value})";
            foreach (var a in c.Mood.Active)
                if (a.Def.Value < 0 && (worst == null || a.Def.Value < worst.Def.Value)) worst = a;
            return worst != null ? $": {worst.Def.DisplayName} ({worst.Def.Value}) zamanla geçer; sohbet ve iyi yemek iyi gelir" : "";
        }

        private static string FixFor(string moodletId)
        {
            switch (moodletId)
            {
                case MoodCatalog.Hungry: return "bir şeyler yesin";
                case MoodCatalog.Tired: return "uyusun";
                case MoodCatalog.Cold: return "ısınsın";
                case MoodCatalog.Bored: return "biraz eğlensin";
                case MoodCatalog.Dirty: return "yıkansın";
                case MoodCatalog.Lonely: return "biriyle sohbet etsin";
                case MoodCatalog.Bursting: return "tuvalete gitsin";
                case MoodCatalog.Wet: return "kurulansın";
                case MoodCatalog.SameMeal: return "farklı bir yemek yesin";
                default: return null;
            }
        }

        /// <summary>Kayıttan yüklemede uyarı durumunu sıfırlar (ilk karede eski basamak yeniden duyurulmasın).</summary>
        public void Prime()
        {
            foreach (var c in _population.Members)
            {
                if (c.IsDead || !c.MoodEnabled) continue;
                var s = c.Mood.Status;
                _warned[c.Id] = s == MoodStatus.Miserable ? 2 : s == MoodStatus.Unhappy ? 1 : 0;
            }
        }
    }
}
