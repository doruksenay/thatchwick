using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Family
{
    /// <summary>Beceri seviyesiyle açılan sosyal iş (Project belgesi 45, 9. bölüm).</summary>
    public sealed class SkillSocialDef
    {
        public SocialAction Action;
        public SkillType Skill;
        public int Level;
        public string Label;
        public double Days = 0.08;
        /// <summary>Hedef uygun mu; uygun değilse neden (null = uygun).</summary>
        public Func<FamilyService, Character, Character, string> Check;
    }

    /// <summary>
    /// Beceri sosyal işleri: Gönül kazanma, Müzik, Ebeveynlik, Öğretmenlik, Hekimlik, Hava bilgisi, Okuryazarlık.
    /// Kurallar burada, menü FamilyPanel'de (açılmış olanlar görünür). Dış sistemler kancalarla bağlanır (LifeSystems.Attach).
    /// </summary>
    public sealed partial class FamilyService
    {
        // ---- kancalar ----
        public Func<double> Today { get; set; } = () => 0;
        public Action<int, string> AddReputation { get; set; }
        public InjurySystem Injuries { get; set; }
        public Func<bool> IsFestivalDay { get; set; } = () => false;
        public Func<string> ForecastText { get; set; } = () => "";
        /// <summary>Görücü: tek başına yetişkin için köye talip çağırır (SuitorPool).</summary>
        public Action<Character> OnMatchmake { get; set; }
        /// <summary>Adlı köylünün rolü (demirci, meyhaneci...); köylü değilse null.</summary>
        public Func<Character, string> RoleOf { get; set; } = _ => null;

        // ---- sabitler ----
        public const double TopicAffectionFactor = 1.5;
        public const int TopicLevel = 3;
        public const double TopicXp = 2;
        public const int TavernPayBase = 2;
        public const int LessonPay = 3, PatientPay = 4;

        private static bool Adultish(Character c) => c.Stage >= AgeStage.Teen;
        private static bool Young(Character c) => c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen;

        private static string NotBaby(FamilyService f, Character a, Character t) => t.Stage == AgeStage.Baby ? "Bebekle olmaz." : null;
        private static string BabyOnly(FamilyService f, Character a, Character t) => t.Stage == AgeStage.Baby ? null : "Yalnızca bebeğe.";
        private static string YoungOnly(FamilyService f, Character a, Character t) => Young(t) ? null : "Çocuk ya da ergene.";
        private static string VillagerOnly(FamilyService f, Character a, Character t) => t.IsVillager ? null : "Köylüyle yapılır.";
        private static string FestivalOnly(FamilyService f, Character a, Character t) => f.IsFestivalDay() ? null : "Yalnızca şenlik günü.";

        public static readonly List<SkillSocialDef> SkillSocials = new List<SkillSocialDef>
        {
            // Gönül kazanma
            new SkillSocialDef { Action = SocialAction.Compliment, Skill = SkillType.Charm, Level = 2, Label = "İltifat et", Days = 0.04, Check = NotBaby },
            new SkillSocialDef { Action = SocialAction.Joke, Skill = SkillType.Charm, Level = 3, Label = "Fıkra anlat", Days = 0.05, Check = NotBaby },
            new SkillSocialDef { Action = SocialAction.Listen, Skill = SkillType.Charm, Level = 4, Label = "Dert dinle", Days = 0.1,
                Check = (f, a, t) => t.Stage == AgeStage.Baby ? "Bebekle olmaz." : WorstMood(t) == null ? $"{t.Name}'in derdi yok." : null },
            new SkillSocialDef { Action = SocialAction.Mediate, Skill = SkillType.Charm, Level = 6, Label = "Arabuluculuk", Days = 0.12, Check = VillagerOnly },
            new SkillSocialDef { Action = SocialAction.Matchmake, Skill = SkillType.Charm, Level = 7, Label = "Görücüye çık", Days = 0.15,
                Check = (f, a, t) => !t.IsVillager && IsAdultAge(t) && !t.SpouseId.HasValue ? null : "Hanenin bekâr genç yetişkini için." },
            new SkillSocialDef { Action = SocialAction.Persuade, Skill = SkillType.Charm, Level = 8, Label = "İkna et", Days = 0.08,
                Check = (f, a, t) => !t.IsVillager ? "Köylüyle yapılır." : a.Mood.Has(MoodCatalog.Persuasive) ? "Zaten ikna edici." : null },
            new SkillSocialDef { Action = SocialAction.FestivalSpeech, Skill = SkillType.Charm, Level = 9, Label = "Şenlikte söz al", Days = 0.08,
                Check = (f, a, t) => f.IsFestivalDay() ? (a.Mood.Has(MoodCatalog.FestivalSpeech) ? "Bugün söz aldı." : null) : "Yalnızca şenlik günü." },
            // Müzik
            new SkillSocialDef { Action = SocialAction.SingLullaby, Skill = SkillType.Music, Level = 2, Label = "Ninni çal", Days = 0.06, Check = BabyOnly },
            new SkillSocialDef { Action = SocialAction.LearnSong, Skill = SkillType.Music, Level = 4, Label = "Türkü öğren", Days = 0.1,
                Check = (f, a, t) => t.IsVillager || t.Skills.GetLevel(SkillType.Music) >= 3 ? null : "Türküyü bilen biri (köylü ya da Müzik 3+)." },
            new SkillSocialDef { Action = SocialAction.PlayTavern, Skill = SkillType.Music, Level = 5, Label = "Meyhanede çal", Days = 0.15,
                Check = (f, a, t) => t.IsVillager && f.RoleOf(t) == VillagerService.RoleInnkeeper ? (a.Mood.Has(MoodCatalog.PlayedTavern) ? "Bugün çaldı." : null) : "Meyhaneciye sorulur." },
            new SkillSocialDef { Action = SocialAction.OpenFestival, Skill = SkillType.Music, Level = 6, Label = "Şenliği aç", Days = 0.1, Check = FestivalOnly },
            new SkillSocialDef { Action = SocialAction.Compose, Skill = SkillType.Music, Level = 7, Label = "Ezgi bestele", Days = 0.2,
                Check = (f, a, t) => t.Stage == AgeStage.Baby ? "Bebekle olmaz." : a.Mood.Has(MoodCatalog.Composed) ? "Yakında bir ezgi besteledi." : null },
            new SkillSocialDef { Action = SocialAction.LeadDance, Skill = SkillType.Music, Level = 9, Label = "Halay çektir", Days = 0.12, Check = FestivalOnly },
            // Ebeveynlik
            new SkillSocialDef { Action = SocialAction.Lullaby, Skill = SkillType.Parenting, Level = 2, Label = "Ninni söyle", Days = 0.05, Check = BabyOnly },
            new SkillSocialDef { Action = SocialAction.PlayWithBaby, Skill = SkillType.Parenting, Level = 3, Label = "Oyna", Days = 0.06, Check = BabyOnly },
            new SkillSocialDef { Action = SocialAction.FirstSteps, Skill = SkillType.Parenting, Level = 4, Label = "İlk adım öğret", Days = 0.08,
                Check = (f, a, t) => t.Stage == AgeStage.Baby ? (t.Mood.Has(MoodCatalog.FirstSteps) ? "Bugün yürüdü." : null) : "Yalnızca bebeğe." },
            new SkillSocialDef { Action = SocialAction.ShowWork, Skill = SkillType.Parenting, Level = 6, Label = "İş göster", Days = 0.1, Check = YoungOnly },
            new SkillSocialDef { Action = SocialAction.Soothe, Skill = SkillType.Parenting, Level = 7, Label = "Sakinleştir", Days = 0.06,
                Check = (f, a, t) => !Young(t) ? "Çocuk ya da ergene." : t.InBreakdown || WorstMood(t) != null ? null : $"{t.Name} sakin." },
            new SkillSocialDef { Action = SocialAction.TalkTeen, Skill = SkillType.Parenting, Level = 9, Label = "Ergenle konuş", Days = 0.1,
                Check = (f, a, t) => t.Stage == AgeStage.Teen ? null : "Yalnızca ergenle." },
            // Öğretmenlik
            new SkillSocialDef { Action = SocialAction.TeachLetters, Skill = SkillType.Teaching, Level = 2, Label = "Harf öğret", Days = 0.1, Check = YoungOnly },
            new SkillSocialDef { Action = SocialAction.VillageLesson, Skill = SkillType.Teaching, Level = 6, Label = "Köy çocuğuna ders", Days = 0.12,
                Check = (f, a, t) => t.IsVillager && Young(t) ? null : "Köyün çocuklarına." },
            new SkillSocialDef { Action = SocialAction.Assess, Skill = SkillType.Teaching, Level = 7, Label = "Sına", Days = 0.05, Check = NotBaby },
            new SkillSocialDef { Action = SocialAction.ElderLesson, Skill = SkillType.Teaching, Level = 9, Label = "Yaşlıya ders", Days = 0.1,
                Check = (f, a, t) => t.Stage == AgeStage.Elder ? null : "Yalnızca yaşlıya." },
            // Hekimlik
            new SkillSocialDef { Action = SocialAction.Diagnose, Skill = SkillType.Medicine, Level = 2, Label = "Muayene et", Days = 0.04, Check = (f, a, t) => null },
            new SkillSocialDef { Action = SocialAction.Bandage, Skill = SkillType.Medicine, Level = 3, Label = "Sargı sar", Days = 0.06,
                Check = (f, a, t) => t.Injury == null ? $"{t.Name} yaralı değil." : t.Injury.Treated ? "Yarası zaten sarıldı." : null },
            new SkillSocialDef { Action = SocialAction.Midwife, Skill = SkillType.Medicine, Level = 5, Label = "Doğuma yardım et", Days = 0.1,
                Check = (f, a, t) => t.IsPregnant ? (t.Mood.Has(MoodCatalog.MidwifeCare) ? "Ebe zaten bakıyor." : null) : "Hamile birine." },
            new SkillSocialDef { Action = SocialAction.SetBone, Skill = SkillType.Medicine, Level = 6, Label = "Kırık sar", Days = 0.08,
                Check = (f, a, t) => t.Injury != null && (t.Injury.Kind == "horse_fall" || t.Injury.Kind == "cave_in" || t.Injury.Kind == "fall" || t.Injury.Kind == "child_fall") ? null : "Düşme ya da göçük yarası için." },
            new SkillSocialDef { Action = SocialAction.VisitPatient, Skill = SkillType.Medicine, Level = 7, Label = "Köylüye bak", Days = 0.12,
                Check = (f, a, t) => t.IsVillager ? (t.Mood.Has(MoodCatalog.DoctorVisit) ? "Bugün bakıldı." : null) : "Köylüye." },
            new SkillSocialDef { Action = SocialAction.ElderCare, Skill = SkillType.Medicine, Level = 9, Label = "Yaşlıya bak", Days = 0.08,
                Check = (f, a, t) => t.Stage == AgeStage.Elder ? null : "Yalnızca yaşlıya." },
            // Hava bilgisi, Okuryazarlık
            new SkillSocialDef { Action = SocialAction.TellWeather, Skill = SkillType.WeatherLore, Level = 6, Label = "Havayı söyle", Days = 0.04, Check = VillagerOnly },
            new SkillSocialDef { Action = SocialAction.WriteLetter, Skill = SkillType.Literacy, Level = 5, Label = "Mektup yaz", Days = 0.1, Check = VillagerOnly },
        };

        public static SkillSocialDef SkillSocial(SocialAction a) => SkillSocials.FirstOrDefault(d => d.Action == a);

        /// <summary>Bu karakterin açtığı beceri sosyal işleri (menü için).</summary>
        public static IEnumerable<SkillSocialDef> UnlockedSocials(Character actor) =>
            SkillSocials.Where(d => actor.Skills.GetLevel(d.Skill) >= d.Level);

        /// <summary>Ortak basamak Sv 3: iki tarafın da 3+ olduğu en yüksek ortak beceri; yoksa null.</summary>
        public static SkillType? SharedTopic(Character a, Character b)
        {
            SkillType? best = null; int bestSum = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                int la = a.Skills.GetLevel(s), lb = b.Skills.GetLevel(s);
                if (la < TopicLevel || lb < TopicLevel) continue;
                if (la + lb > bestSum) { best = s; bestSum = la + lb; }
            }
            return best;
        }

        /// <summary>En kötü (eksi) geçici moodlet; yas hariç (yas dinlemekle geçmez). Yoksa null.</summary>
        public static ActiveMoodlet WorstMood(Character c)
        {
            ActiveMoodlet worst = null;
            foreach (var m in c.Mood.Active)
            {
                if (m.Def.Value >= 0 || m.Def.IsCondition || m.Def.Id == MoodCatalog.Grief) continue;
                if (worst == null || m.Def.Value < worst.Def.Value) worst = m;
            }
            return worst;
        }

        private string CanDoSkill(Character actor, Character target, SocialAction action)
        {
            var d = SkillSocial(action);
            if (d == null) return "Bilinmeyen iş.";
            if (actor.Skills.GetLevel(d.Skill) < d.Level) return $"{SkillNames.Name(d.Skill)} {d.Level} gerekli.";
            if (actor.Stage < AgeStage.Teen) return "Çocuk bunu yapamaz.";
            if (_doneToday.Contains(DoneKey(actor, target, action))) return "Bugün yapıldı.";
            return d.Check?.Invoke(this, actor, target);
        }

        private double SkillDuration(SocialAction action) => SkillSocial(action)?.Days ?? 0.1;

        // Her beceri sosyal işi aynı kişiye günde bir kez (köylülerde moodlet yok; akçe ve itibar tekrarla kazanılmasın).
        private readonly HashSet<string> _doneToday = new HashSet<string>();
        private long _doneDay = long.MinValue;
        private string DoneKey(Character a, Character t, SocialAction action)
        {
            long day = (long)Math.Floor(Today());
            if (day != _doneDay) { _doneDay = day; _doneToday.Clear(); }
            return $"{a.Id}:{t.Id}:{(int)action}";
        }

        private void Affect(Character a, Character b, int delta) => SetAffection(a.Id, b.Id, Affection(a.Id, b.Id) + delta);

        private IEnumerable<Character> Household() => _population.Members.Where(m => !m.IsDead && !m.IsVillager);

        private string ApplySkill(Character actor, Character target, SocialAction action)
        {
            var d = SkillSocial(action);
            if (d == null) return null;
            int lv = actor.Skills.GetLevel(d.Skill);
            _doneToday.Add(DoneKey(actor, target, action));
            actor.GainSkillXp(d.Skill, 6 + d.Level);
            switch (action)
            {
                case SocialAction.Compliment:
                    Affect(actor, target, ChatAffection * 3 / 4);
                    target.AddMoodlet(MoodCatalog.Complimented);
                    return $"{actor.Name}, {target.Name}'e iltifat etti";
                case SocialAction.Joke:
                    foreach (var c in new[] { actor, target }) c.Needs.Restore(NeedType.Fun, 30f);
                    Affect(actor, target, 5);
                    target.AddMoodlet(MoodCatalog.HadFun);
                    return $"{actor.Name} bir fıkra anlattı; {target.Name} güldü";
                case SocialAction.Listen:
                {
                    var worst = WorstMood(target);
                    if (worst != null) target.Mood.Remove(worst.Def.Id);
                    target.AddMoodlet(MoodCatalog.Unburdened);
                    Affect(actor, target, 8);
                    return $"{actor.Name}, {target.Name}'i dinledi" + (worst != null ? $"; \"{worst.Def.DisplayName}\" geçti" : "");
                }
                case SocialAction.Mediate:
                    foreach (var m in Household()) SetAffection(m.Id, target.Id, Affection(m.Id, target.Id) + 4);
                    AddReputation?.Invoke(3, "arabuluculuk");
                    return $"{actor.Name}, {target.Name}'in derdine arabulucu oldu; hane ile arası düzeldi (itibar +3)";
                case SocialAction.Matchmake:
                    OnMatchmake?.Invoke(target);
                    target.AddMoodlet(MoodCatalog.Hopeful);
                    return $"{actor.Name}, {target.Name} için görücüye çıktı; köye bir talip haber salındı";
                case SocialAction.Persuade:
                    actor.AddMoodlet(MoodCatalog.Persuasive);
                    Affect(actor, target, 3);
                    return $"{actor.Name} pazarlıkta ikna edici: bugün satışlar daha iyi";
                case SocialAction.FestivalSpeech:
                    foreach (var m in Household()) m.AddMoodlet(MoodCatalog.FestivalSpeech);
                    AddReputation?.Invoke(3, "şenlik konuşması");
                    return $"{actor.Name} şenlikte söz aldı; hane gururlu (itibar +3)";
                case SocialAction.SingLullaby:
                case SocialAction.Lullaby:
                    target.Needs.Restore(NeedType.Energy, 25f);
                    target.Needs.Restore(NeedType.Fun, 15f);
                    target.AddMoodlet(MoodCatalog.Lulled);
                    if (action == SocialAction.SingLullaby) actor.GainSkillXp(SkillType.Parenting, 3);
                    return $"{actor.Name}, {target.Name} için ninni söyledi";
                case SocialAction.LearnSong:
                    actor.AddMoodlet(MoodCatalog.NewSong);
                    Affect(actor, target, 6);
                    actor.GainSkillXp(SkillType.Music, 10);
                    return $"{actor.Name}, {target.Name}'den yeni bir türkü öğrendi";
                case SocialAction.PlayTavern:
                {
                    int pay = TavernPayBase + lv;
                    _house.AddAkce(pay);
                    actor.AddMoodlet(MoodCatalog.PlayedTavern);
                    AddReputation?.Invoke(1, "meyhanede çaldı");
                    return $"{actor.Name} meyhanede çaldı; {pay} akçe bahşiş";
                }
                case SocialAction.OpenFestival:
                    foreach (var m in Household()) { m.Needs.Restore(NeedType.Fun, 20f); m.AddMoodlet(MoodCatalog.HadFun); }
                    AddReputation?.Invoke(2, "şenliği açtı");
                    return $"{actor.Name} şenliği çalgısıyla açtı (itibar +2)";
                case SocialAction.Compose:
                    actor.AddMoodlet(MoodCatalog.Composed);
                    target.AddMoodlet(MoodCatalog.Composed);
                    Affect(actor, target, 6);
                    return $"{actor.Name} yeni bir ezgi besteledi; ilk dinleyen {target.Name}";
                case SocialAction.LeadDance:
                    foreach (var m in _population.Members.Where(x => !x.IsDead && x.Stage != AgeStage.Baby))
                        if (Math.Abs(m.Position.X - actor.Position.X) + Math.Abs(m.Position.Y - actor.Position.Y) <= 12) { m.Needs.Restore(NeedType.Fun, 25f); m.AddMoodlet(MoodCatalog.HadFun); }
                    AddReputation?.Invoke(2, "halay");
                    return $"{actor.Name} halay çektirdi";
                case SocialAction.PlayWithBaby:
                    target.Needs.Restore(NeedType.Fun, 35f);
                    target.Needs.Restore(NeedType.Social, 25f);
                    Affect(actor, target, 6);
                    return $"{actor.Name}, {target.Name} ile oynadı";
                case SocialAction.FirstSteps:
                    target.AddMoodlet(MoodCatalog.FirstSteps);
                    actor.AddMoodlet(MoodCatalog.FirstSteps);
                    Affect(actor, target, 8);
                    return $"{target.Name} {actor.Name}'in elinden tutup ilk adımlarını attı";
                case SocialAction.ShowWork:
                {
                    var skill = BestSkillOf(actor);
                    double got = target.GainSkillXp(skill, 5 + lv);
                    Affect(actor, target, 4);
                    return $"{actor.Name}, {target.Name}'e {SkillNames.Name(skill).ToLowerInvariant()} işini gösterdi (+{got:0} XP)";
                }
                case SocialAction.Soothe:
                {
                    if (target.InBreakdown) target.EndBreakdown(true);
                    foreach (var m in target.Mood.Active.ToList())
                        if (m.Def.Value < 0 && !m.Def.IsCondition && m.Def.Id != MoodCatalog.Grief) target.Mood.Remove(m.Def.Id);
                    target.Needs.Restore(NeedType.Fun, 15f);
                    return $"{actor.Name}, {target.Name}'i sakinleştirdi";
                }
                case SocialAction.TalkTeen:
                    target.AddMoodlet(MoodCatalog.Understood);
                    Affect(actor, target, 10);
                    return $"{actor.Name} ve {target.Name} uzun uzun konuştu";
                case SocialAction.TeachLetters:
                {
                    double got = target.GainSkillXp(SkillType.Literacy, 6 + lv);
                    return $"{actor.Name}, {target.Name}'e harf öğretti (+{got:0} Okuryazarlık)";
                }
                case SocialAction.VillageLesson:
                {
                    target.GainSkillXp(SkillType.Literacy, 6 + lv);
                    _house.AddAkce(LessonPay);
                    AddReputation?.Invoke(1, "köy çocuğuna ders");
                    return $"{actor.Name}, köyün çocuğu {target.Name}'e ders verdi ({LessonPay} akçe)";
                }
                case SocialAction.Assess:
                    return $"{actor.Name} {target.Name}'i sınadı: {AssessText(target)}";
                case SocialAction.ElderLesson:
                    foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                        if (target.Skills.GetXp(s) > 0) target.Skills.MarkUsedThisSeason(s);
                    target.Needs.Restore(NeedType.Fun, 15f);
                    return $"{actor.Name}, {target.Name} ile bildiklerini tazeledi; bu mevsim paslanmaz";
                case SocialAction.Diagnose:
                    return $"{actor.Name} {target.Name}'i muayene etti: {DiagnoseText(target)}";
                case SocialAction.Bandage:
                    Injuries?.ApplySalve(target);
                    return $"{actor.Name}, {target.Name}'in yarasını sardı; daha çabuk iyileşecek";
                case SocialAction.Midwife:
                    target.AddMoodlet(MoodCatalog.MidwifeCare);
                    return $"{actor.Name} {target.Name}'in doğumuna ebelik edecek";
                case SocialAction.SetBone:
                {
                    var inj = target.Injury;
                    double left = inj.HealDay - Today();
                    if (left > 0) inj.HealDay = Today() + left * 0.5;
                    inj.Treated = true;
                    return $"{actor.Name}, {target.Name}'in kırığını sardı";
                }
                case SocialAction.VisitPatient:
                {
                    if (target.Injury != null) Injuries?.ApplySalve(target);
                    target.AddMoodlet(MoodCatalog.DoctorVisit);
                    _house.AddAkce(PatientPay);
                    AddReputation?.Invoke(2, "köylüye baktı");
                    return $"{actor.Name} {target.Name}'e baktı ({PatientPay} akçe, itibar +2)";
                }
                case SocialAction.ElderCare:
                    target.Needs.Restore(NeedType.Energy, 30f);
                    target.Needs.Restore(NeedType.Fun, 15f);
                    target.AddMoodlet(MoodCatalog.WellCared);
                    return $"{actor.Name}, {target.Name}'in hâlini hatırını sordu, ilacını verdi";
                case SocialAction.TellWeather:
                    Affect(actor, target, 5);
                    AddReputation?.Invoke(1, "havayı söyledi");
                    return $"{actor.Name}, {target.Name}'e havayı söyledi: {ForecastText()}";
                case SocialAction.WriteLetter:
                    Affect(actor, target, 8);
                    target.AddMoodlet(MoodCatalog.GotLetter);
                    return $"{actor.Name}, {target.Name}'e bir mektup yazdı";
                default:
                    return null;
            }
        }

        public static SkillType BestSkillOf(Character c)
        {
            var best = SkillType.Farming; double bx = -1;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                double x = c.Skills.GetXp(s);
                if (x > bx) { bx = x; best = s; }
            }
            return best;
        }

        /// <summary>Sına: en iyi üç beceri ve yatkınlıklar.</summary>
        public static string AssessText(Character t)
        {
            var top = ((SkillType[])Enum.GetValues(typeof(SkillType))).OrderByDescending(s => t.Skills.GetXp(s)).Take(3)
                .Where(s => t.Skills.GetXp(s) > 0).Select(s => $"{SkillNames.Name(s)} {t.Skills.GetLevel(s)}").ToList();
            var apt = t.Skills.Aptitudes.Select(kv => $"{SkillNames.Name(kv.Key)} ({SkillPacing.AptitudeName(kv.Value).ToLowerInvariant()})").ToList();
            return (top.Count > 0 ? "en iyi " + string.Join(", ", top) : "henüz bir işte ilerlememiş")
                + (apt.Count > 0 ? "; yatkın: " + string.Join(", ", apt) : "; belirgin yatkınlık yok");
        }

        /// <summary>Muayene: yara, iyileşmeye kalan gün ve kötü moodlet'lerin nedeni.</summary>
        public string DiagnoseText(Character t)
        {
            var parts = new List<string>();
            if (t.Injury != null) parts.Add($"{t.Injury.Reason}, {Math.Max(0, t.Injury.HealDay - Today()):0.#} günde iyileşir{(t.Injury.Treated ? " (sarılmış)" : "")}");
            foreach (var m in t.Mood.Active.Where(m => m.Def.Value < 0).OrderBy(m => m.Def.Value).Take(3))
                parts.Add($"{m.Def.DisplayName} ({m.Def.Description})");
            if (t.IsPregnant) parts.Add("hamile");
            return parts.Count == 0 ? "sağlıklı, bir derdi yok" : string.Join("; ", parts);
        }
    }
}
