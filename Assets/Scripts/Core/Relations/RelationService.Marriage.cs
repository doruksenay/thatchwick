using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>Eşte bekleyen bir mesele (67c §2): tartışma ya da kavgaya kapı açar.</summary>
    public sealed class SpouseIssue
    {
        public SpouseTrigger Trigger;
        public int Offender;
        public double Day;
        public int Count;
    }

    /// <summary>
    /// Evlilik (67 §6-6, 67b §6, 67c §2): Sıcaklık, mutlu/soğuk yuva, yıldönümü, eşler arası tetikler (T1-T12), ihanet ayrılığı
    /// (boşanma yok; yalnızca ihanetten sonra baba evine dönüş), iş yükü adaleti.
    /// </summary>
    public sealed partial class RelationService
    {
        public const int IssueDays = 7, SeparationColdDays = 7, ColdWarmth = 10, ChoreStreakDays = 5;

        private readonly Dictionary<long, SpouseIssue> _issues = new Dictionary<long, SpouseIssue>();
        private readonly Dictionary<int, int> _choresToday = new Dictionary<int, int>();
        private readonly Dictionary<long, int> _choreStreak = new Dictionary<long, int>();

        /// <summary>Bir yılın gün sayısı (yıldönümü).</summary>
        public Func<int> DaysPerYear { get; set; } = () => 40;
        /// <summary>Kişi gece evinin dışında mı (T10).</summary>
        public Func<Character, bool> AwayAtNight { get; set; }
        /// <summary>Gece içkili mi (+18 paketi): sarhoş eve gelme tetiği (T10'un +18 hâli, karar 67d).</summary>
        public Func<Character, bool> Tipsy { get; set; } = _ => false;

        public const double ConfideChance = 0.15, ConfideGapDays = 3;
        private readonly Dictionary<int, double> _lastConfide = new Dictionary<int, double>();

        /// <summary>
        /// Eşler sohbet edince bazen biri öbürüne bir sırrını açar (T8'in kaynağı): bilgi yalnız ikisinde kalır. Eşi o sırrı
        /// yakın dostuna anlatırsa sırrı açan kırılır. Dönen: açıldıysa sır.
        /// </summary>
        public Fact MaybeConfide(Character a, Character b, double? roll = null)
        {
            if (a == null || b == null || a.SpouseId != b.Id || Knowledge == null) return null;
            if (_lastConfide.TryGetValue(a.Id, out var last) && Today() - last < ConfideGapDays) return null;
            if ((roll ?? _rng.NextDouble()) >= ConfideChance) return null;
            if (Knowledge.SecretsOf(a.Id) >= 3) return null;
            _lastConfide[a.Id] = Today();
            var f = Knowledge.RecordSecret(a, b, _rng.Next(KnowledgeService.Secrets.Length));
            if (f != null)
            {
                Cue?.Invoke(a, "gossip", 3f);
                AddWarmth(a.Id, b.Id, 3);
                AddFeeling(b.Id, a.Id, FeelingKind.Trusts);
                if (IsHousehold(a) || IsHousehold(b)) Notice?.Invoke(LocText.Of("rel.n.confided", ("a", a.Name), ("b", b.Name)), a.Id);
            }
            return f;
        }
        /// <summary>İhanetten ayrılan, doğduğu haneye döner (köylü olur).</summary>
        public Action<Character> SendToBirthHouse { get; set; }

        public IEnumerable<(long pair, SpouseIssue issue)> Issues { get { foreach (var kv in _issues) yield return (kv.Key, kv.Value); } }

        public int Warmth(int a, int b) => Find(a, b)?.Warmth ?? -1;

        public void AddWarmth(int a, int b, int delta)
        {
            var p = Pair(a, b);
            if (p.Warmth < 0) p.Warmth = WarmthStart;
            p.Warmth = Math.Max(0, Math.Min(100, p.Warmth + delta));
        }

        /// <summary>Çocuk isteme şansına çarpan: sıcaklık yüksekse biraz artar, soğuksa düşer.</summary>
        public double ConceiveFactor(Character a, Character b)
        {
            int w = Warmth(a.Id, b.Id);
            if (w < 0) return 1.0;
            return w >= 70 ? 1.15 : w <= 20 ? 0.7 : 1.0;
        }

        public SpouseIssue PendingIssue(int a, int b)
        {
            if (!_issues.TryGetValue(Key(a, b), out var i)) return null;
            if (Today() - i.Day > IssueDays) { _issues.Remove(Key(a, b)); return null; }
            return i;
        }

        public static int Weight(SpouseTrigger t)
        {
            switch (t)
            {
                case SpouseTrigger.Infidelity: return 40;
                case SpouseTrigger.SoldHeirloom: case SpouseTrigger.SpentDowry: case SpouseTrigger.Gambling: return 20;
                case SpouseTrigger.DrunkHome:
                case SpouseTrigger.NeglectedChild: case SpouseTrigger.InLawQuarrel: case SpouseTrigger.SpilledSecret: case SpouseTrigger.Flirting: return 12;
                default: return 6;   // yıldönümü, iş yükü, gece dışarıda, pislik
            }
        }

        /// <summary>
        /// Bir tetik yaşandı (67c §2): eşin sıcaklığı düşer, hissi oluşur, mesele bekler ve eş (oyuncunun seçili karakteri
        /// değilse) tartışma ya da kavgaya gelir. Aldatma ve flört nişanlıda da geçerlidir. scale: olayın ağırlık katı
        /// (eşinin soyundan yadigâr 1,5; büyük bahis 1,4; birkaç dokuma 0,7).
        /// </summary>
        public void ReportTrigger(SpouseTrigger trigger, Character offender, bool known = true, double scale = 1.0)
        {
            if (offender == null || offender.IsDead) return;
            var spouse = offender.SpouseId.HasValue ? _population.Find(offender.SpouseId.Value) : null;
            if (spouse == null && (trigger == SpouseTrigger.Infidelity || trigger == SpouseTrigger.Flirting)) spouse = PartnerOf(offender);
            if (spouse == null || spouse.IsDead || !known) return;
            int w = Math.Max(1, (int)Math.Round(Weight(trigger) * Math.Max(0.1, scale)));
            var p = Pair(offender.Id, spouse.Id);
            bool married = offender.SpouseId == spouse.Id;
            if (married) AddWarmth(offender.Id, spouse.Id, -w);
            AddFriendship(offender.Id, spouse.Id, -w / 3);
            var feeling = trigger == SpouseTrigger.Infidelity ? FeelingKind.Betrayed : trigger == SpouseTrigger.Flirting ? FeelingKind.Jealous : FeelingKind.Hurt;
            AddFeeling(spouse.Id, offender.Id, feeling);
            if (trigger == SpouseTrigger.Infidelity) p.Betrayals++;
            long k = Key(offender.Id, spouse.Id);
            if (_issues.TryGetValue(k, out var issue) && Today() - issue.Day <= IssueDays) { issue.Count++; issue.Day = Today(); if (Weight(trigger) > Weight(issue.Trigger)) issue.Trigger = trigger; }
            else _issues[k] = issue = new SpouseIssue { Trigger = trigger, Offender = offender.Id, Day = Today(), Count = 1 };
            Notice?.Invoke(LocText.Of("rel.n.trigger", ("spouse", spouse.Name), ("who", offender.Name), ("what", RelationRules.TriggerText(trigger))), spouse.Id);
            // Nişanlıda aldatma: NPC nişanı bozabilir.
            if (!married && trigger == SpouseTrigger.Infidelity && p.Status == CoupleStatus.Engaged && !IsHousehold(spouse) && _rng.NextDouble() < 0.5)
            {
                BreakEngagement(spouse, offender);
                return;
            }
            // Tepki: ağırsa ya da tekrarsa kavga, değilse tartışma (seçili oyuncu karakterine iş verilmez).
            if (spouse.IsPlayerControlled || Command == null) return;
            bool fight = married && (trigger == SpouseTrigger.Infidelity || (w >= 20 && issue.Count >= 2)) && CanFight(spouse, offender) == null;
            Command(spouse, Job.Social(offender.Id, fight ? SocialAction.Fight : SocialAction.Argue));
        }

        /// <summary>Tartışma ya da kavga meseleyi kapatır (kısmen: tekrar sayısı azalır).</summary>
        public void ResolveIssue(int a, int b, bool partially)
        {
            long k = Key(a, b);
            if (!_issues.TryGetValue(k, out var i)) return;
            if (!partially || --i.Count <= 0) _issues.Remove(k);
        }

        /// <summary>Görevli iş yapıldı (T6 iş yükü adaleti için).</summary>
        public void NoteChore(Character c)
        {
            if (c == null) return;
            _choresToday.TryGetValue(c.Id, out var n);
            _choresToday[c.Id] = n + 1;
        }

        // ---- yıldönümü ----

        private int YearOf(PairState p, double now) => p.MarriedDay < 0 ? -1 : (int)Math.Floor((now - p.MarriedDay) / Math.Max(1, DaysPerYear()));

        /// <summary>Takvim için: hanedeki evli çiftlerin yaklaşan yıldönümleri (çift, kaç gün sonra, kaçıncı yıl).</summary>
        public List<(Character a, Character b, int days, int year)> UpcomingAnniversaries(int withinDays)
        {
            var list = new List<(Character, Character, int, int)>();
            double now = Today(), perYear = Math.Max(1, DaysPerYear());
            foreach (var p in _pairs.Values)
            {
                if (p.MarriedDay < 0) continue;
                var a = _population.Find(p.A); var b = _population.Find(p.B);
                if (a == null || b == null || a.IsDead || b.IsDead || a.SpouseId != b.Id || (!IsHousehold(a) && !IsHousehold(b))) continue;
                int year = (int)Math.Floor((now - p.MarriedDay) / perYear) + 1;
                double day = p.MarriedDay + year * perYear;
                if (p.AnniversaryYear >= year - 1 && AnniversaryDue(p)) { list.Add((a, b, 0, year - 1)); continue; }
                int days = (int)Math.Ceiling(Math.Floor(day) - Math.Floor(now));
                if (days >= 0 && days <= withinDays) list.Add((a, b, days, year));
            }
            list.Sort((x, y) => x.Item3.CompareTo(y.Item3));
            return list;
        }

        public bool AnniversaryDue(PairState p)
        {
            if (p == null || p.MarriedDay < 0) return false;
            double now = Today();
            int year = YearOf(p, now);
            if (year < 1 || p.AnniversaryYear >= year) return false;
            double day = p.MarriedDay + year * DaysPerYear();
            return now >= day && now < day + 1.5;
        }

        private LocText Celebrate(Character a, Character b)
        {
            var p = Pair(a.Id, b.Id);
            p.AnniversaryYear = YearOf(p, Today());
            AddWarmth(a.Id, b.Id, 25);
            foreach (var c in new[] { a, b }) { c.AddMoodlet(RelationMoods.Anniversary, T("rel.mood.anniversary"), 8, 1.5); Pose(c, "love", 3f); }
            Say(b, "anniversary");
            return LocText.Of("rel.n.anniversary", ("a", a.Name), ("b", b.Name), ("year", p.AnniversaryYear));
        }

        // ---- günlük ----

        private void MarriageNewDay(PairState p, Character a, Character b, double now)
        {
            if (a.SpouseId != b.Id)
            {
                // Evlilik başka yoldan (köy evliliği, eski kayıt) kurulduysa başlat.
                return;
            }
            if (p.MarriedDay < 0) { p.MarriedDay = Math.Floor(now); p.AnniversaryYear = 0; }
            if (p.Warmth < 0) p.Warmth = WarmthStart;
            p.Warmth = Math.Max(0, (int)Math.Round(p.Warmth - WarmthDecayPerDay));
            if (p.Warmth >= 70) foreach (var c in new[] { a, b }) c.AddMoodlet(RelationMoods.HappyHome, T("rel.mood.happy_home"), 3, 1.1);
            else if (p.Warmth <= 20) foreach (var c in new[] { a, b }) c.AddMoodlet(RelationMoods.ColdHome, T("rel.mood.cold_home"), -3, 1.1);
            p.ColdDays = p.Warmth < ColdWarmth ? p.ColdDays + 1 : 0;

            // Yıldönümü: sabah hatırlatma; kutlanmadan geçerse unutulmuş sayılır (T5).
            int year = YearOf(p, now);
            if (year >= 1 && p.AnniversaryYear < year)
            {
                double day = p.MarriedDay + year * DaysPerYear();
                if (now >= day && now < day + 1) Notice?.Invoke(LocText.Of("rel.n.anniversary_today", ("a", a.Name), ("b", b.Name), ("year", year)), a.Id);
                else if (now >= day + 1.5)
                {
                    p.AnniversaryYear = year;
                    foreach (var c in new[] { a, b }) c.AddMoodlet(RelationMoods.ForgotAnniv, T("rel.mood.forgot_anniversary"), -3, 1.0);
                    var forgot = RelationRules.Temper(a) >= RelationRules.Temper(b) ? b : a;   // sinirli olan öbürüne kızar
                    ReportTrigger(SpouseTrigger.ForgotAnniversary, forgot);
                }
            }

            // İş yükü adaleti (T6): biri 5 gün üst üste bütün görevli işi yaptıysa.
            _choresToday.TryGetValue(a.Id, out var ca);
            _choresToday.TryGetValue(b.Id, out var cb);
            long k = Key(a.Id, b.Id);
            _choreStreak.TryGetValue(k, out var streak);
            int sign = ca >= 3 && cb == 0 ? 1 : cb >= 3 && ca == 0 ? -1 : 0;
            streak = sign == 0 ? 0 : (Math.Sign(streak) == sign ? streak + sign : sign);
            _choreStreak[k] = streak;
            if (Math.Abs(streak) >= ChoreStreakDays) { _choreStreak[k] = 0; ReportTrigger(SpouseTrigger.UnfairChores, streak > 0 ? b : a); }

            // Gece dışarıda (T10): biri evdeyken öbürü gece evin dışında (iş, şenlik ve düğün sayılmaz; hook ayırır).
            // +18 paketinde içkili eve gelen ayrı tetik: sarhoş eve geldi (karar 67d).
            if (a.Stage >= AgeStage.YoungAdult && b.Stage >= AgeStage.YoungAdult)
            {
                bool aw = AwayAtNight != null && AwayAtNight(a), bw = AwayAtNight != null && AwayAtNight(b);
                bool at = Tipsy(a), bt = Tipsy(b);
                if (at != bt && _rng.NextDouble() < 0.6) ReportTrigger(SpouseTrigger.DrunkHome, at ? a : b);
                else if (aw != bw && _rng.NextDouble() < 0.5) ReportTrigger(SpouseTrigger.OutAllNight, aw ? a : b);
            }

            // İhanet ayrılığı: "İhanete uğramış" + Sıcaklık 10'un altında 7 gün, ya da ikinci ihanet.
            foreach (var (betrayed, cheater) in new[] { (a, b), (b, a) })
            {
                if (!HasFeeling(betrayed.Id, cheater.Id, FeelingKind.Betrayed)) continue;
                if (p.ColdDays < SeparationColdDays && p.Betrayals < 2) continue;
                if (_decisions.Exists(d => d.Kind == DecisionKind.Separation && (d.A == betrayed.Id || d.B == betrayed.Id))) continue;
                if (IsHousehold(betrayed) && !betrayed.IsVillager)
                {
                    AddDecision(DecisionKind.Separation, betrayed.Id, cheater.Id, 0, now + 3);
                    Notice?.Invoke(LocText.Of("rel.n.separation_choice", ("a", betrayed.Name), ("b", cheater.Name)), betrayed.Id);
                }
                else ResolveSeparation(betrayed, cheater, AutoLeave(betrayed));
                break;
            }
        }

        public bool AutoLeave(Character betrayed) =>
            betrayed != null && !(betrayed.HasTrait("trait.placid") || betrayed.HasTrait("trait.generous")) && _rng.NextDouble() < 0.6;

        /// <summary>
        /// İhanet sonrası karar (67b §6): affet ya da ayrıl. Ayrılıkta soy hanesine sonradan gelen (gelin ya da damat) doğduğu
        /// haneye döner; çocuklar soy hanesinde kalır; ikisi "Eski eş" olur; aldatana itibar kaybı ve aileler küs.
        /// </summary>
        public void ResolveSeparation(Character betrayed, Character cheater, bool leave)
        {
            if (betrayed == null || cheater == null || betrayed.SpouseId != cheater.Id) return;
            var p = Pair(betrayed.Id, cheater.Id);
            if (!leave)
            {
                RemoveFeeling(betrayed.Id, cheater.Id, FeelingKind.Betrayed);
                AddFeeling(betrayed.Id, cheater.Id, FeelingKind.Hurt, 10);
                p.Warmth = 40; p.ColdDays = 0;
                betrayed.AddMoodlet(RelationMoods.Forgave, T("rel.mood.forgave"), 3, 2.0);
                cheater.AddMoodlet(RelationMoods.Regret, T("rel.mood.regret"), -3, 2.0);
                Notice?.Invoke(LocText.Of("rel.n.forgave", ("a", betrayed.Name), ("b", cheater.Name)), betrayed.Id);
                return;
            }
            betrayed.SpouseId = null;
            cheater.SpouseId = null;
            p.Status = CoupleStatus.ExSpouse; p.StatusDay = Today(); p.Warmth = -1; p.Romance = Math.Min(p.Romance, -30);
            _issues.Remove(Key(betrayed.Id, cheater.Id));
            // Soy hanesine sonradan gelen gider (oyuncu hanesi), köylü çiftte ihanete uğrayan.
            Character leaver = betrayed;
            if (IsHousehold(betrayed) && IsHousehold(cheater)) leaver = MarriedIn(betrayed) ? betrayed : MarriedIn(cheater) ? cheater : betrayed;
            ForgetDowry(leaver.Id);   // çeyizi onunla gider (defterden düşer)
            SendToBirthHouse?.Invoke(leaver);
            Reputation?.Invoke(RepFacet.Word, -15, LocText.Of("rel.rep.infidelity"), cheater);
            Knowledge?.Record(FactKind.Separation, cheater, betrayed, RepFacet.Word, -6, null);
            Knowledge?.RecordHouseLink(FactKind.HouseFeud, betrayed, cheater);
            Notice?.Invoke(LocText.Of("rel.n.separated", ("a", betrayed.Name), ("b", cheater.Name), ("leaver", leaver.Name)), betrayed.Id);
        }

        /// <summary>Haneye evlenerek gelmiş mi: anne-babası hanede (ya da yaşayan herhangi bir hane üyesinin ebeveyni) değil.</summary>
        private bool MarriedIn(Character c)
        {
            foreach (var pid in new[] { c.BirthMotherId, c.BirthFatherId })
            {
                if (!pid.HasValue) continue;
                var par = _population.Find(pid.Value);
                if (par != null && IsHousehold(par)) return false;
            }
            return c.Generation <= 1 ? c.BirthMotherId.HasValue || c.BirthFatherId.HasValue : true;
        }

        internal void EndDayChores() => _choresToday.Clear();

        public bool AnniversaryDueFor(Character a, Character b) => AnniversaryDue(Find(a.Id, b.Id));
    }
}
