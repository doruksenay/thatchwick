using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Bir karakterin iş kuyruğunu oyun günü bütçesiyle yürütür: yürüme ve eşya kullanma.
    /// Oyun saatinde işler, bu yüzden hız çarpanı otomatik uygulanır ve parçalı ilerleme
    /// tek adımla aynı sonucu verir. NPC'ler (otonom) ihtiyaçları düşünce kendi iş seçer.
    /// </summary>
    public sealed class JobRunner
    {
        /// <summary>Yürüme hızı: oyun günü başına karo (3 karo/sn gerçek zaman, 1x hızda).</summary>
        public const double TilesPerGameDay = 1080.0;
        /// <summary>Otonomi puanında her etkileşime eklenen sabit süre (karar, yanaşma): küçük ama sık dolduranı cezalandırır.</summary>
        public const double AutonomyOverheadDays = 0.03;
        /// <summary>İnsanın bir adımda çime/yola bıraktığı iz (inek 3, koyun 1).</summary>
        public const float HumanTrample = 0.2f;

        /// <summary>Otonom karakter, ihtiyacı bu değerin altına düşünce kendiliğinden iş arar.</summary>
        public const float AutonomyThreshold = 30f;

        private const int MaxIterations = 64;

        /// <summary>Otonom karakter ulaşamadığı eşyayı bu kadar oyun günü boyunca yeniden denemez.</summary>
        public const double UnreachableCooldownDays = 0.25;

        private static readonly GridPos[] AdjacentOffsets =
        {
            new GridPos(0, 1), new GridPos(1, 0), new GridPos(0, -1), new GridPos(-1, 0)
        };

        private readonly Character _character;
        private readonly WorldState _world;

        private List<GridPos> _path;
        private int _pathIndex;
        private double _stepFraction;
        private bool _using;
        private double _useProgress;
        private double _elapsedDays;
        private readonly Dictionary<int, double> _unreachableUntil = new Dictionary<int, double>();
        // Ulaşılamayan kişi (bebek, sohbet arkadaşı) için de aynı bekleme: otonomi her karede yeniden denemesin.
        private readonly Dictionary<int, double> _unreachablePersonUntil = new Dictionary<int, double>();
        private static readonly NeedType[] AllNeeds = (NeedType[])Enum.GetValues(typeof(NeedType));
        /// <summary>Otonomi eşya seçerken başka kattaki eşyaya eklenen uzaklık (karo): önce kendi katındaki seçilsin.</summary>
        public const int OtherLevelDistance = 12;
        private readonly List<NeedType> _needOrder = new List<NeedType>();

        public JobQueue Queue { get; } = new JobQueue();

        /// <summary>Açıksa, kuyruk boşken ve ihtiyaç kritikken karakter kendi iş seçer (NPC).</summary>
        public bool Autonomous { get; set; }
        /// <summary>
        /// Otonomi kapalıyken de boşta görev işi alır (hizmetkâr: köylü, ihtiyaç otonomisi yok ama sabah Ev görevini yapar; belge 45).
        /// </summary>
        public bool DutyWhenIdle { get; set; }

        /// <summary>
        /// İş etkileşimlerinin (kaynak harcama, ürün, eşyayı kaldırma) uygulayıcısı. Yoksa bu tür
        /// etkileşimler başlamaz. Hane envanterini JobRunner bilmez; bu arayüz üzerinden çalışır.
        /// </summary>
        public IInteractionEffects Effects { get; set; }

        /// <summary>Karakterler arası işler (bakım, sohbet, evlilik). Yoksa bu işler başlamaz.</summary>
        public ISocialEffects Social { get; set; }

        /// <summary>
        /// Isı kaynağına envanterden bir kütük atar (HeatSystem.TryStoke); attıysa true. Null ise otonomi sönük ocağı yakamaz.
        /// (Denge 41: soğukta üşüyen, odun varsa ocağı kendisi yakar; eskiden yalnızca oyuncu düğmeyle yakabiliyordu.)
        /// </summary>
        public Func<ItemInstance, bool> Stoke { get; set; }
        /// <summary>Hanede odun var mı (sönük ocak otonomide ancak o zaman seçilir).</summary>
        public Func<bool> HasFirewood { get; set; }
        /// <summary>Isınırken ocakta bu kadar günlük yakıt yoksa odun eklenir (en çok StokeMaxLogs kütük).</summary>
        public const double StokeTargetDays = 1.0;
        public const int StokeMaxLogs = 2;

        /// <summary>
        /// Ek hız çarpanı (alet kademesi, hava durumu...). Yalnızca iş etkileşimlerine uygulanır; yoksa 1.
        /// Özellik, dal ve ruh hali çarpanları karakterden okunur.
        /// </summary>
        public Func<Character, InteractionDef, double> SpeedProvider { get; set; }

        /// <summary>Eşyanın ya da aletin işe kattığı çarpan (yadigâr): hız, XP ve ihtiyaç doldurma. Null ise etkisiz.</summary>
        public Func<ItemInstance, InteractionDef, ItemBonus> ItemBonus { get; set; }

        private ItemBonus BonusOf(ItemInstance item, InteractionDef interaction) =>
            ItemBonus != null ? ItemBonus(item, interaction) : Jobs.ItemBonus.None;

        /// <summary>Hayvan işleri (sev, tımar, eyer, bin). Yoksa bu işler başlamaz.</summary>
        public IAnimalEffects Animals { get; set; }

        /// <summary>Karo işleri (çapala). Yoksa bu işler başlamaz.</summary>
        public ITileWork TileWork { get; set; }

        /// <summary>Bir etkileşim başarıyla bitince (kaza sistemi, istatistik). Etkiler uygulandıktan sonra çağrılır.</summary>
        public event Action<Character, InteractionDef> InteractionCompleted;
        /// <summary>Eşya etkileşimi bitti; eşya da verilir (oyuncak alışkanlığı için). Eşya kaldırılmış olabilir.</summary>
        public event Action<Character, ItemInstance, InteractionDef> ItemUsed;
        /// <summary>
        /// Bir iş bitti (eşya, karo ya da hayvan işi): beceri ve iş kimliği. İstekler (belge 46) mevsim işlerini ve "N kez yap"
        /// hedeflerini bununla sayar.
        /// </summary>
        public event Action<Character, SkillType?, string> WorkDone;

        /// <summary>Karo işinin becerisi (istek sayımı için).</summary>
        public static SkillType? SkillOf(TileAction a)
        {
            switch (a)
            {
                case TileAction.Till: case TileAction.Sow: return SkillType.Farming;
                case TileAction.Repair: return SkillType.Masonry;
                case TileAction.Fish: case TileAction.Stock: return SkillType.Fishing;
                case TileAction.CutIce: case TileAction.Dig: case TileAction.Support: case TileAction.Bail: case TileAction.Light: return SkillType.Mining;
                default: return null;
            }
        }

        /// <summary>Yürüme hızı çarpanı (ata binince ~2); yoksa 1.</summary>
        public Func<double> WalkSpeed { get; set; }

        /// <summary>
        /// İş süresi ölçeği (belge 47, karar 5): gün 6 dakikadan uzayınca işler gerçek zamanda aynı sürsün diye oyun saatindeki
        /// süreler bu çarpanla kısalır (12 dakikalık günde 0,5). Yürüme hızı da gerçek zamanda sabit kalır (karo/gün ÷ ölçek).
        /// Uyku ve uzun dinlenme ölçeklenmez (gece oyun saatinde kalır). LifeSystems.Attach saat ayarından kurar; yoksa 1.
        /// </summary>
        public double ActionTimeScale { get; set; } = 1.0;

        /// <summary>Uyku ve uzun dinlenme (enerji, 0,1 günden uzun): oyun saatinde kalır, ölçeklenmez.</summary>
        public static bool IsRest(InteractionDef i) => i != null && i.Need == NeedType.Energy && i.RestoresNeed && i.DurationDays >= 0.1;

        private double Scale => ActionTimeScale > 0 ? ActionTimeScale : 1.0;

        /// <summary>Ölçekli yürüme hızı (karo / oyun günü).</summary>
        public double TilesPerDayScaled => TilesPerGameDay / Scale;

        /// <summary>
        /// Görev işi: ihtiyaçlar iyiyken karakterin görevine (tarla, hayvanlar) göre yapılacak iş; yoksa null.
        /// Yalnızca otonom karakterde, kuyruk boşken sorulur.
        /// </summary>
        public Func<Character, Job> DutyProvider { get; set; }
        /// <summary>
        /// Boşta inisiyatif (Sims gibi): giderecek ihtiyacı ve görevi olmayan karakter yakınındaki ufak ev işini kendisi
        /// yapar (belge 55: kedinin yere düşürdüğü süsü yerine koyar). Null dönerse iş yok.
        /// </summary>
        public Func<Character, Job> IdleChore { get; set; }

        /// <summary>Görev işlerinin hız çarpanı (kâhyalık); null ise 1.</summary>
        public Func<double> DutySpeed { get; set; }

        /// <summary>Görev işi bulunamayınca (ya da başarısız olunca) bu kadar oyun günü yeniden sorulmaz.</summary>
        public const double DutyIdleDays = 0.03;
        private double _dutyIdleUntil;

        /// <summary>Şu an yürütülen işin nedeni (otonom ihtiyaç); oyuncu komutunda ya da boştayken null.</summary>
        public NeedType? CurrentReason => Queue.Current?.Reason;

        /// <summary>Bunalımdaki çocuğun ya da ergenin küseceği yer (en yakın ağaç evin ip merdiveni, belge 52); null ise yok.</summary>
        public Func<Character, GridPos?> Refuge { get; set; }
        public const string RefugeNote = "ağaç eve küstü";

        /// <summary>Son başarısız işin nedeni (arayüzde gösterilir); yoksa null.</summary>
        public string LastFailure { get; private set; }

        public JobRunner(Character character, WorldState world)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            Autonomous = !character.IsPlayerControlled;
        }

        /// <summary>Yürürken gidilen sonraki karo; yürümüyorsa null (görünüm ara değerlemesi için).</summary>
        public GridPos? NextTile =>
            _path != null && !_using && _pathIndex < _path.Count ? _path[_pathIndex] : (GridPos?)null;

        /// <summary>Mevcut karodan NextTile'a 0-1 arası ilerleme.</summary>
        public float StepProgress => (float)_stepFraction;

        public bool IsUsingItem => _using;

        // ---- İzlenen eylem zinciri (Project belgesi 60) ----
        private List<ActionStepDef> _steps;
        private double[] _stepEnds;          // adımların bittiği yer, 0-1 arası birikimli pay
        private int _stepIndex = -1;

        /// <summary>Sürmekte olan işin şu anki zincir adımı; iş zincirsizse ya da karakter henüz yürüyorsa null.</summary>
        public ActionStepDef CurrentStep => _using && _steps != null && _stepIndex >= 0 && _stepIndex < _steps.Count ? _steps[_stepIndex] : null;

        /// <summary>Bu iş için oynayacak adımlar (koşulu tutmayanlar ayıklanmış); zincir yoksa boş.</summary>
        public IReadOnlyList<ActionStepDef> ActiveSteps => _steps ?? (IReadOnlyList<ActionStepDef>)Array.Empty<ActionStepDef>();

        /// <summary>Şu anki adımda karakter soyunuk mu (peştamal; yıkanma).</summary>
        public bool IsUndressed => CurrentStep != null && CurrentStep.Undress;

        /// <summary>Zincirde yeni adıma geçildi (görünüm: poz, ses, elde eşya). İş bitince ya da kesilince çağrılmaz.</summary>
        public event Action<Character, ActionStepDef> StepChanged;

        /// <summary>
        /// İşe başlarken zinciri kurar: koşulu tutmayan adımlar ayıklanır, kalanların payları 0-1 aralığına dizilir.
        /// Zincir toplam süreyi değiştirmez; yalnızca böler.
        /// </summary>
        private void BeginChain(ItemInstance item, InteractionDef interaction)
        {
            _steps = null; _stepEnds = null; _stepIndex = -1;
            var chain = interaction.ChainDef;
            if (chain == null) return;
            var steps = new List<ActionStepDef>(chain.Steps.Count);
            double total = 0;
            foreach (var st in chain.Steps)
            {
                if (st.When != null && !StepApplies(st.When, item, interaction)) continue;
                steps.Add(st);
                total += st.Share;
            }
            if (steps.Count == 0 || total <= 0) return;
            var ends = new double[steps.Count];
            double acc = 0;
            for (int k = 0; k < steps.Count; k++) { acc += steps[k].Share; ends[k] = acc / total; }
            ends[steps.Count - 1] = 1.0;
            _steps = steps; _stepEnds = ends;
            UpdateStep(0.0);
        }

        private bool StepApplies(string when, ItemInstance item, InteractionDef interaction)
        {
            switch (when)
            {
                case ChainConditions.SkillLow:
                    return interaction.Skill.HasValue && _character.Skills.GetLevel(interaction.Skill.Value) <= ChainConditions.LowSkillMax;
                case ChainConditions.SkillHigh:
                    return interaction.Skill.HasValue && _character.Skills.GetLevel(interaction.Skill.Value) >= ChainConditions.HighSkillMin;
                case ChainConditions.MoodGood:
                    return _character.MoodEnabled && (_character.Mood.Status == MoodStatus.Good || _character.Mood.Status == MoodStatus.Happy);
                case ChainConditions.MoodBad:
                    return _character.MoodEnabled && (_character.Mood.Status == MoodStatus.Unhappy || _character.Mood.Status == MoodStatus.Miserable);
                case ChainConditions.CarriedWater:
                    return _carriedWater;
                case ChainConditions.Fire:
                    return item != null && item.Def.HeatOutput > 0f;
                default:
                    return false;
            }
        }

        /// <summary>İlerleme oranına (0-1) göre adımı günceller; adım değiştiyse haber verir.</summary>
        private void UpdateStep(double fraction)
        {
            if (_steps == null) return;
            int index = _steps.Count - 1;
            for (int k = 0; k < _stepEnds.Length; k++)
                if (fraction < _stepEnds[k]) { index = k; break; }
            if (index == _stepIndex) return;
            _stepIndex = index;
            StepChanged?.Invoke(_character, _steps[index]);
        }

        private void ClearChain() { _steps = null; _stepEnds = null; _stepIndex = -1; _carriedWater = false; }
        private bool _carriedWater;

        public bool IsIdle => Queue.Current == null;

        /// <summary>Mevcut işi ve kuyruğu bırakıp yeni bir işi tek iş olarak başlatır (oyuncu komutu).</summary>
        public bool Command(Job job)
        {
            if (job != null && _character.InBreakdown && job.Kind != JobKind.Breakdown)
            {
                // Bunalımda emir sıraya girer: bunalım (ve bunalımdaki öğün) sürer, bekleyen eski emirler yerini yenisine bırakır.
                for (int i = Queue.Count - 1; i >= 1; i--)
                    if (Queue.Jobs[i].Kind != JobKind.Breakdown) Queue.RemoveAt(i);
                LastRefusal = $"{MoodBehavior.BreakdownName(_character.Breakdown)}: emir bunalım geçince yapılacak.";
                return Queue.TryEnqueue(job);
            }
            // Aynı yere art arda dokunmak yolu baştan kurmasın (hızlı dokunuşta takılma).
            var cur = Queue.Current;
            if (job != null && job.Kind == JobKind.MoveTo && cur != null && cur.Kind == JobKind.MoveTo && Queue.Count == 1 && cur.Target.Equals(job.Target))
                return true;
            Cancel();
            return Queue.TryEnqueue(job);
        }

        /// <summary>Kuyruktaki işi iptal eder (kuyruk şeridi). Şu anki işse yürütme durumu sıfırlanır.</summary>
        public bool CancelAt(int index)
        {
            bool wasCurrent = index == 0;
            if (!Queue.RemoveAt(index)) return false;
            if (wasCurrent) ResetJobState();
            return true;
        }

        /// <summary>Kuyruktaki işi öne alır; şu anki iş bir sonrakine kayar (ilerlemesi sıfırlanır).</summary>
        public bool Prioritize(int index)
        {
            if (!Queue.MoveToFront(index)) return false;
            ResetJobState();
            return true;
        }

        public void Cancel()
        {
            Queue.Clear();
            ResetJobState();
        }

        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));

            _elapsedDays += gameDays;
            SyncBreakdown();
            if (Autonomous) InterruptForToilet();
            double budget = gameDays;
            for (int guard = 0; guard < MaxIterations && budget > 1e-12; guard++)
            {
                var job = Queue.Current;
                if (job == null && (_carryTile.HasValue || _settling))
                {
                    // İş iptal edildi ya da bitti ama karakter iki karonun arasında: adımı tamamlar (geri sıçramaz).
                    budget -= FinishCarriedStep(budget);
                    continue;
                }
                if (job == null)
                {
                    if (!Autonomous) { if (!DutyWhenIdle || !TryStartDuty()) return; continue; }
                    if (!TryStartAutonomy()) return;
                    continue;
                }

                if (!ReferenceEquals(job, _checkedJob))
                {
                    _checkedJob = job;
                    if (RefuseIfDying(job)) continue;
                }

                budget -= Advance(job, budget);
                if (_using) _carryTile = null;   // yürümeden başlayan iş: yarım adım geçersiz
            }
        }

        // ---- Ruh halinin davranışa etkisi (18.11.2026, Project belgesi 23) ----

        private static readonly NeedType[] LethalNeeds = { NeedType.Hunger, NeedType.Energy, NeedType.Warmth };
        private Job _checkedJob;

        /// <summary>Son emir reddinin ya da bunalımda sıraya alınan emrin metni (arayüz); yoksa null.</summary>
        public string LastRefusal { get; private set; }

        /// <summary>Karakter, ölümcül bir ihtiyacı yüzünden bir işi başlatmadı (önce ihtiyacını gideriyor). Metin balon/bildirim içindir.</summary>
        public event Action<Character, string> Refused;

        /// <summary>Köy meydanı (bunalımda "Meydana kaçtı"); yoksa null ve karakter yerinde kalır.</summary>
        public Func<GridPos?> SquarePosition { get; set; }

        /// <summary>
        /// Uç durumda emir reddi (karar 18.11.2026): açlık, enerji ya da ısınma 5'in altındaysa karakter o ihtiyacı gidermeyen
        /// işi başlatmaz; önce ihtiyacını giderir, emir silinmez (sırada kalır). Yürüme, bebeği besleme ve ihtiyacı gideren
        /// işler hiç reddedilmez. Giderecek bir şey yoksa (yiyecek yok) iş engellenmez: hasat edip yiyecek bulabilsin.
        /// </summary>
        private bool RefuseIfDying(Job job)
        {
            if (job.Kind == JobKind.MoveTo || job.Kind == JobKind.Breakdown) return false;
            if (job.Kind == JobKind.Social && job.Action == SocialAction.Feed) return false;
            NeedType? dying = null;
            foreach (var n in LethalNeeds)
            {
                float v = _character.Needs.Get(n);
                if (v < MoodBehavior.RefuseBelow && (dying == null || v < _character.Needs.Get(dying.Value))) dying = n;
            }
            if (dying == null) return false;
            if (job.Kind == JobKind.UseItem)
            {
                var it = _world.GetItem(job.ItemId);
                var inter = it == null ? null : FindInteraction(it.Def, job.InteractionId);
                if (inter != null)
                    foreach (var n in LethalNeeds)
                        if (_character.Needs.Get(n) < MoodBehavior.RefuseBelow && inter.RestoreFor(n) > 0f) return false;
            }
            FindBestFor(dying.Value, out var fixItem, out var fixInteraction);
            if (fixItem == null) return false;
            string text = MoodBehavior.RefusalText(dying.Value);
            LastRefusal = text;
            Queue.InsertFront(Job.UseItem(fixItem.Id, fixInteraction.Id).WithReason(dying.Value).WithNote(text));
            ResetJobState();
            Refused?.Invoke(_character, text);
            return true;
        }

        /// <summary>
        /// Bunalım işini kuyrukla eşler: bunalım başlayınca en öne bunalım işi girer (Tıkındı: önce bir öğün),
        /// bitince kalkar. Oyuncu bunalım işini iptal etse ya da başka işi öne alsa da bunalım sürdükçe geri gelir;
        /// yalnızca ihtiyaç işleri (bunalımdaki öğün, tuvalet) önüne geçebilir.
        /// </summary>
        private void SyncBreakdown()
        {
            int index = -1;
            for (int i = 0; i < Queue.Count; i++) if (Queue.Jobs[i].Kind == JobKind.Breakdown) { index = i; break; }

            if (!_character.InBreakdown)
            {
                if (index < 0) return;
                Queue.RemoveAt(index);
                if (index == 0) ResetJobState();
                return;
            }

            if (index == 0) return;
            var cur = Queue.Current;
            if (index > 0)
            {
                if (cur != null && cur.Reason.HasValue) return;     // önce öğün ya da tuvalet
                Queue.MoveToFront(index);
                ResetJobState();
                return;
            }

            // Bunalım yeni başladı.
            var kind = _character.Breakdown;
            GridPos? target = null;
            int itemId = 0;
            Job meal = null;
            // Çocuk ve ergen ağlarken ya da kaçarken en yakın ağaç eve küser (belge 52).
            bool minor = _character.Stage == AgeStage.Child || _character.Stage == AgeStage.Teen;
            GridPos? refuge = minor && kind != BreakdownKind.Binge && Refuge != null ? Refuge(_character) : null;
            if (refuge.HasValue)
            {
                Queue.InsertFront(Job.BreakdownAt(refuge, 0).WithNote(RefugeNote));
                return;
            }
            switch (kind)
            {
                case BreakdownKind.Withdraw:
                    FindBestFor(NeedType.Energy, out var bed, out _);
                    if (bed != null) itemId = bed.Id;
                    break;
                case BreakdownKind.RunOff:
                    target = SquarePosition?.Invoke();
                    break;
                case BreakdownKind.Binge:
                    FindBestFor(NeedType.Hunger, out var food, out var eat);
                    if (food != null) meal = Job.UseItem(food.Id, eat.Id).WithReason(NeedType.Hunger).WithNote("derdini yemekle bastırıyor");
                    break;
            }
            Queue.InsertFront(Job.BreakdownAt(target, itemId).WithNote(MoodBehavior.BreakdownName(kind)));
            if (meal != null) Queue.InsertFront(meal);
            ResetJobState();
        }

        // Bunalım: (varsa) yatağın yanına ya da meydana yürür, sonra bunalım bitene kadar orada kalır.
        private double AdvanceBreakdown(Job job, double budget)
        {
            if (!_character.InBreakdown) { Finish(); return 0; }
            if (!_using)
            {
                if (_path == null)
                {
                    var item = job.ItemId != 0 ? _world.GetItem(job.ItemId) : null;
                    if (item != null) _path = PathToAdjacent(item);
                    else if (job.HasTarget) _path = Pathfinder.FindPath(_world.Map, _character.Position, job.Target);
                    if (_path == null) _path = new List<GridPos> { _character.Position };   // hedef yok ya da ulaşılamıyor: yerinde kalır (yarım adım tamamlanır)
                    _pathIndex = 1;
                    _stepFraction = 0;
                }
                double used = Walk(budget);
                if (_path == null) { _using = true; return used; }
                if (_pathIndex < _path.Count) return used;
                _using = true;
                return used;
            }
            return budget;   // bunalım sürüyor: iş yok
        }

        // Otonomi puanı ruh hali basamağının hız çarpanını içermez (eskiden kazara Mutlu karakteri pişirmeye itiyordu);
        // pişirme hevesi aşağıda açık kural olarak verilir.
        private double AutonomyDuration(InteractionDef interaction, ItemInstance item)
        {
            double d = EffectiveDuration(interaction, item);
            if (IsWork(interaction) && interaction.Skill.HasValue && _character.MoodEnabled)
                d *= MoodRules.SpeedMultiplier(_character.Mood.Status);
            return d;
        }

        private bool RepeatsMeal(string foodId)
        {
            var meals = _character.RecentMeals;
            int n = meals.Count;
            return n >= 2 && meals[n - 1] == foodId && meals[n - 2] == foodId;
        }

        /// <summary>Şu an başlanabilecek, başka bir yiyecekle doyuran bir etkileşim var mı (sofra çeşidi cezası için).</summary>
        private bool HasOtherFood(string foodId)
        {
            foreach (var item in _world.NeedItems)
                foreach (var interaction in item.Def.Interactions)
                {
                    if (interaction.RestoreFor(NeedType.Hunger) <= 0f || interaction.ConsumeItem == null || interaction.ConsumeItem == foodId) continue;
                    if (interaction.RemovesItem || interaction.RequiresGrown || interaction.Produce.Count > 0 || interaction.IsCraft) continue;
                    if (interaction.MinSkillLevel > 0 && interaction.Skill.HasValue && _character.Skills.GetLevel(interaction.Skill.Value) < interaction.MinSkillLevel) continue;
                    if (interaction.RequiresBranch != null && !_character.HasBranch(interaction.RequiresBranch)) continue;
                    if (interaction.HasEffects && (Effects == null || Effects.CanStart(_character, item, interaction) != null)) continue;
                    return true;
                }
            return false;
        }

        // Bir iş için en çok `budget` kadar ilerler; harcanan oyun gününü döner.
        private double Advance(Job job, double budget)
        {
            switch (job.Kind)
            {
                case JobKind.MoveTo: return AdvanceMove(job, budget);
                case JobKind.UseItem: return AdvanceUse(job, budget);
                case JobKind.Social: return AdvanceSocial(job, budget);
                case JobKind.Animal: return AdvanceAnimal(job, budget);
                case JobKind.Tile: return AdvanceTile(job, budget);
                case JobKind.Breakdown: return AdvanceBreakdown(job, budget);
                default:
                    Fail($"Bilinmeyen iş türü: {job.Kind}");
                    return 0;
            }
        }

        private double AdvanceMove(Job job, double budget)
        {
            if (_path == null)
            {
                _path = Pathfinder.FindPath(_world.Map, _character.Position, job.Target);
                if (_path == null) { Fail("Oraya ulaşılamıyor."); return 0; }
                _pathIndex = 1;
                _stepFraction = 0;
            }

            double used = Walk(budget);
            if (_path == null) return used;      // yol kapandı: bir sonraki turda yeniden hesaplanır
            if (_pathIndex >= _path.Count) Finish();
            return used;
        }

        private double AdvanceSocial(Job job, double budget)
        {
            if (Social == null) { Fail("Bu iş şu an yapılamaz."); return 0; }
            var target = Social.Find(job.TargetCharacterId);
            if (target == null || target.IsDead) { Fail("Kişi bulunamadı."); return 0; }

            if (!_using)
            {
                string blocked = Social.CanDo(_character, target, job.Action);
                if (blocked != null) { Fail(blocked); return 0; }

                // Hedefin yanına (ya da durduğu karoya) git; hedef yer değiştirdiyse yolu yenile.
                if (_path != null && _path.Count > 0 && !IsNear(_path[_path.Count - 1], target.Position)) _path = null;
                if (_path == null)
                {
                    var goals = new HashSet<GridPos> { target.Position };
                    foreach (var o in AdjacentOffsets) goals.Add(target.Position + o);
                    _path = Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
                    if (_path == null)
                    {
                        _unreachablePersonUntil[target.Id] = _elapsedDays + UnreachableCooldownDays;
                        Fail($"{target.Name} yanına gidilemiyor.");
                        return 0;
                    }
                    _pathIndex = 1;
                    _stepFraction = 0;
                }
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;
                if (_pathIndex < _path.Count) return used;
                if (!IsNear(_character.Position, target.Position)) { _path = null; return used; }
                _using = true;
            }

            double remaining = Social.Duration(job.Action) * Scale - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                Finish();
                Social.Apply(_character, target, job.Action);
                return used + remaining;
            }
            _useProgress += available;
            return used + available;
        }

        private double AdvanceAnimal(Job job, double budget)
        {
            if (Animals == null) { Fail("Bu iş şu an yapılamaz."); return 0; }
            var pos = Animals.PositionOf(job.AnimalId);
            if (pos == null) { Fail("Hayvan bulunamadı."); return 0; }
            var target = pos.Value;

            if (!_using)
            {
                string blocked = Animals.CanDo(_character, job.AnimalId, job.AnimalAction);
                if (blocked != null) { Fail(blocked); return 0; }
                // Hayvan dolaşır: hedef yer değiştirdiyse yol yenilenir (sohbetteki gibi).
                if (_path != null && _path.Count > 0 && !IsNear(_path[_path.Count - 1], target)) _path = null;
                if (_path == null)
                {
                    var goals = new HashSet<GridPos> { target };
                    foreach (var o in AdjacentOffsets) goals.Add(target + o);
                    _path = Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
                    if (_path == null) { Fail("Hayvanın yanına gidilemiyor."); return 0; }
                    _pathIndex = 1;
                    _stepFraction = 0;
                }
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;
                if (_pathIndex < _path.Count) return used;
                if (!IsNear(_character.Position, target)) { _path = null; return used; }
                _using = true;
            }

            double remaining = Animals.Duration(job.AnimalAction) * Scale - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                string gone = Animals.CanDo(_character, job.AnimalId, job.AnimalAction);
                if (gone != null) { Fail(gone); return used + remaining; }
                Finish();
                Animals.Apply(_character, job.AnimalId, job.AnimalAction);
                WorkDone?.Invoke(_character, SkillType.AnimalHusbandry, "animal:" + job.AnimalAction.ToString().ToLowerInvariant());
                return used + remaining;
            }
            _useProgress += available;
            return used + available;
        }

        private double AdvanceTile(Job job, double budget)
        {
            if (TileWork == null) { Fail("Bu iş şu an yapılamaz."); return 0; }
            if (!_using)
            {
                string blocked = TileWork.CanDo(_character, job.Target, job.TileAction, job.InteractionId);
                if (blocked != null) { Fail(blocked); return 0; }
                if (_path == null)
                {
                    var goals = new HashSet<GridPos>();
                    var map = _world.Map;
                    if (map.IsWalkable(job.Target)) goals.Add(job.Target);
                    if (job.TileAction != TileAction.Bathe)          // yıkanmak için suyun içine girilir; öbür işler kıyıdan da olur
                        foreach (var o in AdjacentOffsets) if (map.IsWalkable(job.Target + o)) goals.Add(job.Target + o);
                    if (job.TileAction == TileAction.Fish || job.TileAction == TileAction.Stock)   // kıyıdan (kıyı yoksa suya girilir)
                    {
                        var dry = new HashSet<GridPos>();
                        foreach (var g in goals) if (!map.IsWater(g)) dry.Add(g);
                        if (dry.Count > 0) goals = dry;
                    }
                    _path = goals.Count == 0 ? null : Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
                    if (_path == null) { Fail("Oraya ulaşılamıyor."); return 0; }
                    _pathIndex = 1;
                    _stepFraction = 0;
                }
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;
                if (_pathIndex < _path.Count) return used;
                _using = true;
            }

            double remaining = (TileWork is ITileDuration td ? td.Duration(_character, job.Target, job.TileAction) : TileWork.Duration(_character, job.TileAction)) * Scale - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                string gone = TileWork.CanDo(_character, job.Target, job.TileAction, job.InteractionId);
                if (gone != null) { Fail(gone); return used + remaining; }
                Finish();
                TileWork.Apply(_character, job.Target, job.TileAction, job.InteractionId);
                WorkDone?.Invoke(_character, SkillOf(job.TileAction), "tile:" + job.TileAction.ToString().ToLowerInvariant());
                return used + remaining;
            }
            _useProgress += available;
            return used + available;
        }

        private static bool IsNear(GridPos a, GridPos b) => a.Z == b.Z && Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) <= 1;

        private double AdvanceUse(Job job, double budget)
        {
            var item = _world.GetItem(job.ItemId);
            InteractionDef interaction = item == null ? null : FindInteraction(item.Def, job.InteractionId);
            if (interaction == null) { Fail("Eşya ya da etkileşim bulunamadı."); return 0; }

            if (!_using)
            {
                string blocked = StartBlocker(item, interaction);
                if (blocked != null) { Fail(blocked); return 0; }
            }

            // Banyo ve leğen: önce en yakın sudan (kuyu, gölet, dere) bir kova su getirilir; her banyoya bir sefer.
            // Tesisata su geliyorsa (borulu küvet, lüleli leğen) kova taşınmaz.
            if (!_using && interaction.FetchWater && !_hasWater && PipedWater != null && PipedWater(item)) _hasWater = true;
            if (!_using && interaction.FetchWater && !_hasWater && WaterNetworkAccess != null)
            {
                if (_path == null)
                {
                    var goals = WaterNetworkAccess();
                    _path = goals.Count == 0 ? null : Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
                    if (_path == null) { Fail("Yakında su yok: kuyu, gölet ya da dere gerekir."); return 0; }
                    _pathIndex = 1;
                    _stepFraction = 0;
                    _fetching = true;
                }
                double walked = Walk(budget);
                if (_path == null) return walked;
                if (_pathIndex < _path.Count) return walked;
                _hasWater = true;            // kova dolu: şimdi eşyaya
                _carriedWater = true;
                _fetching = false;
                _path = null;
                return walked;
            }

            if (_path == null && !_using)
            {
                _path = PathToAdjacent(item);
                if (_path == null)
                {
                    // Otonomi aynı ulaşılamayan eşyayı sürekli seçip takılmasın.
                    _unreachableUntil[item.Id] = _elapsedDays + UnreachableCooldownDays;
                    Fail("Eşyaya ulaşılamıyor.");
                    return 0;
                }
                _pathIndex = 1;
                _stepFraction = 0;
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;  // yol kapandı: yeniden hesaplanır
                if (_pathIndex < _path.Count) return used;
                _using = true;
                // Isınmaya gelen sönük ya da azalan ocağa odun atar; odun bu arada bittiyse iş boşa gider.
                if (interaction.Need == NeedType.Warmth && item.Def.HeatOutput > 0f)
                {
                    TopUpFire(item);
                    if (!item.IsLit) { Fail("Ocak sönük, odun kalmadı."); return used; }
                }
                BeginChain(item, interaction);
            }
            _character.Asleep = interaction.Need == NeedType.Energy && interaction.RestoresNeed;

            double remaining = EffectiveDuration(interaction, item) - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                // Rezervasyon yok: malzeme bu arada başkasınca harcanmış olabilir. Koşul bozulduysa iş boşa gider;
                // karşılıksız doyma, XP ya da moodlet verilmez.
                if (interaction.HasEffects)
                {
                    string gone = Effects == null ? "Bu iş şu an yapılamaz." : Effects.CanStart(_character, item, interaction);
                    if (gone != null) { Fail(gone); return used + remaining; }
                }
                // Müzik: çalgı çalmak seviye başına %5 daha çok keyif verir.
                float restore = interaction.Skill == SkillType.Music
                    ? (float)(interaction.Restore * SkillNames.Factor(_character.Skills.GetLevel(SkillType.Music), 0.05)) : interaction.Restore;
                var bonus = BonusOf(item, interaction);
                if (bonus.Restore != 1.0) restore = (float)(restore * bonus.Restore);
                if (interaction.RestoresNeed) _character.Needs.Restore(interaction.Need, restore);
                foreach (var extra in interaction.ExtraRestore) _character.Needs.Restore(extra.Need, extra.Amount);
                // İçmek ve yemek tuvaleti doldurur (içecek çok, yemek az).
                if (interaction.BladderCost > 0f && _character.ComfortNeedsActive)
                    _character.Needs.Set(NeedType.Bladder, _character.Needs.Get(NeedType.Bladder) - interaction.BladderCost);
                // Öğün: açlık doyuran ve yiyecek harcayan iş sofra çeşidine sayılır.
                if (interaction.Need == NeedType.Hunger && interaction.RestoresNeed && interaction.ConsumeItem != null)
                    _character.RecordMeal(interaction.ConsumeItem, HasOtherFood(interaction.ConsumeItem));
                if (interaction.GivesMood)
                    _character.AddMoodlet(interaction.MoodId, interaction.MoodText ?? interaction.DisplayName, interaction.MoodValue, interaction.MoodDays);
                if (interaction.Skill.HasValue && interaction.SkillXp > 0f)
                    _character.GainSkillXp(interaction.Skill.Value, interaction.SkillXp * bonus.Xp);
                if (interaction.Moodlet != null) _character.AddMoodlet(interaction.Moodlet);
                Finish();
                // Etkiler en sonda: eşyayı kaldırabilir (kesilen ağaç) ve dünyayı değiştirir.
                if (interaction.HasEffects) Effects?.Apply(_character, item, interaction);
                if (interaction.HasEffects && Effects is Work.InteractionEffects fx) ApplyEatenQuality(fx, interaction, restore, fx.LastConsumedQuality);
                InteractionCompleted?.Invoke(_character, interaction);
                ItemUsed?.Invoke(_character, item, interaction);
                if (IsWork(interaction)) WorkDone?.Invoke(_character, interaction.Skill, interaction.Id);
                return used + remaining;
            }

            _useProgress += available;
            if (_steps != null)
            {
                double whole = EffectiveDuration(interaction, item);
                UpdateStep(whole > 0 ? _useProgress / whole : 1.0);
            }
            return used + available;
        }

        // Yol boyunca yürür (kesirli ilerleme korunur); harcanan oyun gününü döner.
        private double Walk(double budget)
        {
            ContinueCarriedStep();
            double used = 0;
            // Ata binen daha hızlı gider (merdivende de; basitlik için).
            // Atletizm: yaya yürüyüşte seviye başına %2 (binekteyken binek hızı belirler).
            double ride = WalkSpeed?.Invoke() ?? 1.0;
            double tilesPerDay = TilesPerDayScaled * Math.Max(0.1, ride) * (ride <= 1.0 ? _character.AthleticSpeedFactor * _character.WalkFactor : 1.0);
            while (_pathIndex < _path.Count && budget - used > 0)
            {
                // Yol yenileme: sonraki adım (yeni duvar, eşya) kapandıysa yol iptal edilir.
                if (_stepFraction <= 0 && !_world.Map.CanTraverse(_character.Position, _path[_pathIndex]))
                {
                    _path = null;
                    _stepFraction = 0;
                    return used;
                }
                // Merdiven adımı daha uzundur (yatay mesafe + 1 karo).
                int span = GridMap.StepSpan(_character.Position, _path[_pathIndex]);
                // Yol hızlandırır (çim 1, patika 1,6, kaldırım 2): adımın iki ucunun ortalaması.
                double stepTiles = tilesPerDay * _world.Map.StepSpeed(_character.Position, _path[_pathIndex]);
                double needed = (1.0 - _stepFraction) * span / stepTiles;
                double available = budget - used;
                if (available >= needed)
                {
                    used += needed;
                    _character.Position = _path[_pathIndex];
                    _pathIndex++;
                    _stepFraction = 0;
                    _world.Map.Trample(_character.Position, HumanTrample);
                    if (ride <= 1.0 && _character.Stage != AgeStage.Baby) _character.GainSkillXp(SkillType.Athletics, Character.AthleticXpPerTile * span);
                    if (_world.Map.IsOpenWater(_character.Position)) _character.Soak(0.6f);   // suya giren ıslanır
                }
                else
                {
                    _stepFraction += available * stepTiles / span;
                    used += available;
                }
            }
            return used;
        }

        // Eşyanın kapladığı karolara dik komşu, açık ve arada duvar olmayan karolardan en ucuz
        // ulaşılanına tek A* ile gider. Zaten böyle bir karodaysa tek elemanlı yol.
        private List<GridPos> PathToAdjacent(ItemInstance item)
        {
            // Dekor, üzerinde durduğu eşyanın yanından kullanılır.
            var owner = item.IsDecor ? _world.GetItem(item.ParentId.Value) : item;
            if (owner == null) return null;

            var goals = new HashSet<GridPos>();
            if (owner.IsWallMounted)
            {
                // Duvar eşyası önündeki karodan kullanılır (geniş eşyada herhangi bir kenarın önü).
                foreach (var seg in owner.WallSegments()) if (_world.Map.IsWalkable(seg.tile)) goals.Add(seg.tile);
                return goals.Count == 0 ? null : Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
            }
            if (owner.IsCeiling)
            {
                // Tavan eşyası altındaki karodan (ya da yanından) kullanılır.
                if (_world.Map.IsWalkable(owner.Position)) goals.Add(owner.Position);
                foreach (var offset in AdjacentOffsets)
                {
                    var tile = owner.Position + offset;
                    if (_world.Map.IsWalkable(tile) && !_world.Map.EdgeBlocks(GridMap.EdgeBetween(tile, owner.Position))) goals.Add(tile);
                }
                return goals.Count == 0 ? null : Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
            }
            foreach (var occupied in owner.OccupiedTiles())
            {
                foreach (var offset in AdjacentOffsets)
                {
                    var tile = occupied + offset;
                    if (!_world.Map.IsWalkable(tile)) continue;
                    if (_world.Map.EdgeBlocks(GridMap.EdgeBetween(tile, occupied))) continue; // duvarın öbür yanından kullanılmaz
                    goals.Add(tile);
                }
            }
            return Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
        }

        /// <summary>İş etkileşimi mi (beceri, ürün ya da eşya kaldırma)? İhtiyaç etkileşimleri hız çarpanından etkilenmez.</summary>
        public static bool IsWork(InteractionDef i) => (i.Skill.HasValue && i.Skill != SkillType.Music) || i.Produce.Count > 0 || i.RemovesItem;

        /// <summary>İş hızı: özellik/dal × ruh hali × (alet, hava...). İhtiyaç etkileşiminde 1.</summary>
        public double SpeedFactor(InteractionDef interaction, ItemInstance item = null)
        {
            if (!IsWork(interaction)) return 1.0;
            double f = _character.Modifiers.WorkSpeed * (interaction.Skill.HasValue ? _character.WorkSpeed(interaction.Skill.Value) : 1.0);
            if (SpeedProvider != null) f *= SpeedProvider(_character, interaction);
            f *= BonusOf(item, interaction).Speed;
            if (DutySpeed != null && Queue.Current != null && Queue.Current.IsDuty) f *= DutySpeed();
            return f < 0.1 ? 0.1 : f;
        }

        /// <summary>
        /// Kaliteli yiyecek ve içecek (53 §4.5): açlık doyumu ×1,1 / 1,25 / 1,4 / 1,5; pişmiş yemekte sofra ruh hali; yemek
        /// etkisinin (moodlet) süresi uzar.
        /// </summary>
        private void ApplyEatenQuality(Work.InteractionEffects fx, InteractionDef interaction, float restore, int quality)
        {
            if (quality <= 0 || interaction.ConsumeItem == null) return;
            double satiety = ItemQuality.SatietyFactor(quality);
            if (interaction.RestoresNeed && satiety > 1.0) _character.Needs.Restore(interaction.Need, (float)(restore * (satiety - 1.0)));
            if (interaction.Need == NeedType.Hunger && fx.Content.Items.TryGet(interaction.ConsumeItem, out var food) && food.HasTag("cooked"))
                _character.AddMoodlet(ItemQuality.MealMood(quality));
            if (interaction.Moodlet != null && _character.MoodEnabled) _character.Mood.Extend(interaction.Moodlet, ItemQuality.EffectFactor(quality));
        }

        /// <summary>İş süresi çarpanı (ustalık eseri denemesi iki kat sürer, 53 §4.6); null ise 1.</summary>
        public Func<Character, ItemInstance, InteractionDef, double> DurationFactor { get; set; }

        /// <summary>Beceri seviyesi işi hızlandırır: seviye başına %5, en çok yarı süre. Hız çarpanı bölünür.</summary>
        public double EffectiveDuration(InteractionDef interaction, ItemInstance item = null)
        {
            double duration = interaction.DurationDays;
            if (interaction.Skill.HasValue)
            {
                int level = _character.Skills.GetLevel(interaction.Skill.Value);
                duration *= Math.Max(0.5, 1.0 - 0.05 * (level - 1));
            }
            if (!IsRest(interaction)) duration *= Scale;
            if (DurationFactor != null) duration *= Math.Max(0.1, DurationFactor(_character, item, interaction));
            return duration / SpeedFactor(interaction, item);
        }

        /// <summary>Etkileşime şu an başlanamıyorsa nedeni (etkileşim menüsünde gri seçenek); başlanabiliyorsa null.</summary>
        public string WhyCannotStart(ItemInstance item, InteractionDef interaction) => StartBlocker(item, interaction);

        /// <summary>
        /// İstasyonun yanında olması gereken eşya yoksa nedeni (camcı tezgâhı → "Cam ocağı yakında olmalı (4 karo)."); varsa null.
        /// Uzaklık iki ayak izi arasındaki en kısa karo farkıdır (Chebyshev), aynı kat.
        /// </summary>
        public static string NearbyBlocker(WorldState world, ItemInstance station)
        {
            string need = station.Def.RequiresNearby;
            if (need == null) return null;
            int r = station.Def.NearbyRadius;
            foreach (var other in world.Items)
            {
                if (other.Def.Id != need || other.Position.Z != station.Position.Z) continue;
                int dx = Math.Max(0, Math.Max(other.Position.X - (station.Position.X + station.Width - 1), station.Position.X - (other.Position.X + other.Width - 1)));
                int dy = Math.Max(0, Math.Max(other.Position.Y - (station.Position.Y + station.Height - 1), station.Position.Y - (other.Position.Y + other.Height - 1)));
                if (Math.Max(dx, dy) <= r) return null;
            }
            return $"{station.Def.RequiresNearbyName ?? need} yakında olmalı ({r} karo).";
        }

        // Etkileşime başlanamıyorsa nedeni (arayüzde gösterilir); başlanabiliyorsa null.
        private string StartBlocker(ItemInstance item, InteractionDef interaction)
        {
            if (IsUnlitHeatSource(item, interaction) && !CanStoke) return "Ocak sönük, önce odun at.";
            if (interaction.Hidden) return "+18 paketi kapalı.";
            if (interaction.AdultPack && !AgeStages.IsAdultContentAge(_character.Stage)) return "Genç yetişkin ve üstü.";
            if (interaction.AdultOnly && (_character.Stage == AgeStage.Baby || _character.Stage == AgeStage.Child || _character.Stage == AgeStage.Teen)) return "Çocuklar kahve içmez.";
            if (interaction.RequiresGrown && !item.IsGrown) return $"{item.Def.DisplayName} henüz büyümedi.";
            if (interaction.MinSkillLevel > 0 && interaction.Skill.HasValue
                && _character.Skills.GetLevel(interaction.Skill.Value) < interaction.MinSkillLevel)
                return $"{SkillName(interaction.Skill.Value)} {interaction.MinSkillLevel}. seviye gerekli.";
            if (interaction.RequiresBranch != null && !_character.HasBranch(interaction.RequiresBranch))
                return $"{interaction.RequiresBranchName ?? "Meslek"} dalı gerekir.";
            if (interaction.RequiresLit && !item.IsLit) return "Ateş sönük, önce odun at.";
            if (interaction.Consume.Count > 0 && item.Def.RequiresNearby != null)
            {
                string near = NearbyBlocker(_world, item);
                if (near != null) return near;
            }
            if (interaction.HasEffects)
            {
                if (Effects == null) return "Bu iş şu an yapılamaz.";
                return Effects.CanStart(_character, item, interaction);
            }
            return null;
        }

        public static string SkillName(SkillType skill) => SkillNames.Name(skill);

        private static bool IsUnlitHeatSource(ItemInstance item, InteractionDef interaction) =>
            interaction.Need == NeedType.Warmth && item.Def.HeatOutput > 0f && !item.IsLit;

        private bool CanStoke => Stoke != null && HasFirewood != null && HasFirewood();

        // Isınmaya gelen ocağı besler: yakıt StokeTargetDays altındaysa en çok StokeMaxLogs kütük atar.
        private void TopUpFire(ItemInstance item)
        {
            if (Stoke == null || item.Def.HeatOutput <= 0f) return;
            for (int i = 0; i < StokeMaxLogs && item.FuelDays < StokeTargetDays; i++)
                if (!Stoke(item)) break;
        }

        /// <summary>
        /// İhtiyaç için kullanılabilir mi (otonomi): hanenin arsasındaki her eşya; arsa dışında yalnızca aynı bölgedeki
        /// açık havadaki eşya (meydan bankı, kuyu). Başkasının evindeki yatak, fırın, küvet ve öbür bölgedeki eşya seçilmez.
        /// (Denge 41: bölgeli haritada aç karakter şehirdeki hanın fırınına 150 karo yürüyüp açlıktan ölüyordu.)
        /// </summary>
        public static bool MayUseForNeeds(WorldState world, Character character, ItemInstance item)
        {
            var map = world.Map;
            var at = item.IsDecor && item.ParentId.HasValue ? (world.GetItem(item.ParentId.Value)?.Position ?? item.Position) : item.Position;
            if (map.IsOwned(at)) return true;
            var room = map.Rooms.RoomAt(at);
            if (room != null && room.IsIndoor) return false;
            if (!Regions.IsRegional(map)) return true;
            return Regions.At(at) == Regions.At(character.Position);
        }

        private static InteractionDef FindInteraction(ItemDef def, string interactionId)
        {
            foreach (var i in def.Interactions)
                if (i.Id == interactionId) return i;
            return null;
        }

        // Otonomi: eşik altındaki ihtiyaçlar sırayla denenir. Önce hayatî olanlar (Açlık, Enerji, Isınma; en düşükten),
        // sonra Sosyal/Eğlence. Bir ihtiyaç için uygun iş yoksa bir sonrakine geçilir (eskiden takılıp kalırdı).
        private bool TryStartAutonomy()
        {
            // Bakım: kendisi kritik değilse önce aç bebeği besler.
            if (Social != null && !_character.Needs.AnyCritical())
            {
                var dependent = Social.DependentNeedingCare(_character);
                if (dependent != null && !PersonOnCooldown(dependent.Id) && Social.CanDo(_character, dependent, SocialAction.Feed) == null)
                {
                    ResetJobState();
                    return Queue.TryEnqueue(Job.Social(dependent.Id, SocialAction.Feed).WithReason(NeedType.Hunger));
                }
            }

            _needOrder.Clear();
            foreach (var need in AllNeeds)
                if (_character.Needs.Get(need) < AutonomyThreshold) _needOrder.Add(need);
            if (_needOrder.Count == 0) return TryStartDuty() || TryStartChore();
            _needOrder.Sort((a, b) =>
            {
                int ka = Rank(a), kb = Rank(b);
                if (ka != kb) return ka.CompareTo(kb);
                return _character.Needs.Get(a).CompareTo(_character.Needs.Get(b));
            });
            // Görevli, yalnızca rahatlık ihtiyaçları (keyif, hijyen, sosyal) eşiğin altındayken ve hiçbiri ComfortDeferFloor'a
            // inmemişse önce görevine bakar (Denge 41: rahatlık ihtiyaçları hemen her an eşiğin altında olduğu için
            // tarla görevlisi biçtiği ekini günlerce yeniden ekmiyordu). İş yoksa ihtiyaç işine geçer.
            if (Rank(_needOrder[0]) == 2 && ComfortCanWait() && TryStartDuty()) return true;

            // Başarısız arama her adımda yinelenmesin: bütün dünyayı tarar ve rahatlık ihtiyaçları hemen her an eşiğin altındadır.
            if (_elapsedDays < _autonomyIdleUntil && !_character.Needs.AnyCritical()) return TryStartChore();
            foreach (var need in _needOrder)
            {
                if (need == NeedType.Social && TryStartChat()) return true;
                // (sıra: kritik hayatta kalma > tuvalet ve öbür hayatta kalma > keyif ihtiyaçları)
                if (need == NeedType.Energy && _character.ComfortNeedsActive && _character.Needs.Get(NeedType.Bladder) < PreSleepBladder
                    && TryStartNeedItem(NeedType.Bladder)) return true;          // yatmadan önce tuvalet
                if (TryStartNeedItem(need)) return true;
            }
            _autonomyIdleUntil = _elapsedDays + AutonomyRetry;
            return TryStartChore();
        }

        /// <summary>İhtiyaç işi bulunamadıysa ve kritik ihtiyaç yoksa yakındaki ufak ev işi (IdleChore).</summary>
        private bool TryStartChore()
        {
            if (IdleChore == null || _character.Needs.AnyCritical()) return false;
            var job = IdleChore(_character);
            if (job == null) return false;
            ResetJobState();
            return Queue.TryEnqueue(job);
        }

        /// <summary>
        /// Otonomi giderecek bir şey bulamazsa bu kadar oyun günü yeniden aramaz (≈3 oyun dakikası, 1x hızda ~1,4 sn).
        /// Kritik ihtiyaç (açlık, enerji, ısınma) bekletilmez.
        /// </summary>
        public const double AutonomyRetryDays = 0.002;

        /// <summary>
        /// Bekleme süresi (oyun günü). Oyun katmanı bunu gerçek süreye göre ayarlar (hız 3x ve hızlı gecede oyun günü
        /// cinsinden sabit süre gerçek zamanda kısalır ve arama sıklaşırdı).
        /// </summary>
        public double AutonomyRetry { get; set; } = AutonomyRetryDays;
        private double _autonomyIdleUntil;

        /// <summary>Tuvalet bunun altına inince yürüdüğü işi ya da uykuyu bırakıp tuvalete koşar; iş sonra sürer.</summary>
        public const float BladderInterrupt = 15f;
        /// <summary>Yatmadan önce tuvalet bunun altındaysa önce tuvalete gider.</summary>
        public const float PreSleepBladder = 55f;
        private double _toiletCheckUntil;

        /// <summary>Görevli rahatlık ihtiyaçlarını bu değere inene dek görev uğruna erteler.</summary>
        public const float ComfortDeferFloor = 15f;

        private bool ComfortCanWait()
        {
            foreach (var need in _needOrder)
                if (_character.Needs.Get(need) < ComfortDeferFloor) return false;
            return true;
        }

        // Otonomi sırası: kritik açlık/uyku/soğuk önce; sonra tuvalet ve öbür hayatta kalma; en son keyif ihtiyaçları.
        private int Rank(NeedType need)
        {
            if (NeedKinds.IsSurvival(need) && _character.Needs.IsCritical(need)) return 0;
            return NeedKinds.IsUrgent(need) ? 1 : 2;
        }

        private void InterruptForToilet()
        {
            if (!_character.ComfortNeedsActive || _elapsedDays < _toiletCheckUntil) return;
            if (_character.Needs.AnyCritical()) return;                    // donan, açlıktan bayılan önce onu giderir
            if (_character.Needs.Get(NeedType.Bladder) >= BladderInterrupt) return;
            var cur = Queue.Current;
            if (cur == null) return;                                         // boştaysa otonomi zaten seçer
            if (_using && !_character.Asleep) return;                        // yemeği, banyoyu yarıda bırakmaz; uykudan uyanır
            if (cur.Kind == JobKind.UseItem && cur.Reason == NeedType.Bladder) return;
            if (cur.Kind == JobKind.UseItem)
            {
                var it = _world.GetItem(cur.ItemId);
                var i = it == null ? null : FindInteraction(it.Def, cur.InteractionId);
                if (i != null && i.RestoreFor(NeedType.Bladder) > 0f) return;
            }
            _toiletCheckUntil = _elapsedDays + 0.05;                        // tuvalet yoksa her an aramasın
            FindBestFor(NeedType.Bladder, out var item, out var inter);
            if (item == null) return;
            Queue.InsertFront(Job.UseItem(item.Id, inter.Id).WithReason(NeedType.Bladder));
            ResetJobState();
        }

        // Görev: ihtiyaçları eşiğin üstündeyken görevine göre (tarla, hayvanlar) bir iş alır. Bulamazsa bir süre sormaz.
        private bool TryStartDuty()
        {
            if (DutyProvider == null || _character.Duty == DutyKind.None || _elapsedDays < _dutyIdleUntil) return false;
            var job = DutyProvider(_character);
            if (job == null) { _dutyIdleUntil = _elapsedDays + DutyIdleDays; return false; }
            ResetJobState();
            return Queue.TryEnqueue(job.AsDuty());
        }

        private bool TryStartChat()
        {
            if (Social == null) return false;
            var friend = Social.FriendFor(_character);
            if (friend == null || PersonOnCooldown(friend.Id) || Social.CanDo(_character, friend, SocialAction.Chat) != null) return false;
            // Büyük biri çocukla "sohbet" etmek yerine ona masal anlatır.
            var act = Social.CanDo(_character, friend, SocialAction.TellStory) == null ? SocialAction.TellStory : SocialAction.Chat;
            ResetJobState();
            return Queue.TryEnqueue(Job.Social(friend.Id, act).WithReason(NeedType.Social));
        }

        private bool PersonOnCooldown(int characterId) =>
            _unreachablePersonUntil.TryGetValue(characterId, out var until) && until > _elapsedDays;

        // İhtiyacı en iyi doyuracak eşyayı seçer: birim zamanda en çok dolduran (yürüme ve iş süresi dahil).
        private bool TryStartNeedItem(NeedType need)
        {
            FindBestFor(need, out var bestItem, out var bestInteraction);
            if (bestItem == null) return false;

            ResetJobState();
            var job = Job.UseItem(bestItem.Id, bestInteraction.Id).WithReason(need);
            if (_lastPickEager) job.WithNote(MoodBehavior.EagerCookNote);
            return Queue.TryEnqueue(job);
        }

        private bool _lastPickEager;

        private void FindBestFor(NeedType need, out ItemInstance bestItem, out InteractionDef bestInteraction)
        {
            bestItem = null;
            bestInteraction = null;
            _lastPickEager = false;
            bool eagerMood = need == NeedType.Hunger && _character.MoodEnabled && MoodBehavior.IsEagerCook(_character.Mood.Status);
            double bestScore = double.MinValue;
            int bestDistance = int.MaxValue;
            float missing = Math.Max(1f, 100f - _character.Needs.Get(need));
            // Yalnızca bu ihtiyaç için aday eşyalar taranır (WorldState.NeedItemsFor); tanım düzeyindeki elemeler
            // (iş olan, beceri isteyen etkileşimler) ItemDef.AutonomyInteractions'ta önceden yapılmıştır. Eskiden her aramada
            // dünyadaki bütün eşyaların bütün etkileşimleri geziliyordu (çekirdek sürenin üçte ikisi).
            var candidates = _world.NeedItemsFor(need);
            for (int ci = 0; ci < candidates.Count; ci++)
            {
                var item = candidates[ci];
                // Eşyaya bağlı elemeler etkileşimlerden önce bir kez: kullanabilir mi (arsa, bölge), ulaşılır mı.
                if (_unreachableUntil.Count > 0 && _unreachableUntil.TryGetValue(item.Id, out var until) && until > _elapsedDays) continue;
                if (!MayUseForNeeds(_world, _character, item)) continue;
                foreach (var interaction in item.Def.AutonomyInteractions(need))
                {
                    float restore = interaction.RestoreFor(need);
                    // Otonomi iş yapmaz (ağaç kesmez, hasat etmez, üretmez, yem koymaz); yalnızca ihtiyaç giderir.
                    // Malzeme ya da hayvan gerektiren ihtiyaç etkileşimi (şalgam yemek, köpekle oynamak) koşul sağlanıyorsa seçilir.
                    if (interaction.HasEffects && (Effects == null || Effects.CanStart(_character, item, interaction) != null)) continue;
                    if (interaction.RequiresLit && !item.IsLit) continue;
                    if (IsUnlitHeatSource(item, interaction) && !CanStoke) continue;   // sönük ocağa ancak odun varsa gidilir (yakar)

                    int distance = Math.Max(
                        Math.Abs(item.Position.X - _character.Position.X),
                        Math.Abs(item.Position.Y - _character.Position.Y))
                        + Math.Abs(item.Position.Z - _character.Position.Z) * OtherLevelDistance;

                    // Eksik olandan fazlası boşa gider; yürüme ve iş süresine ek sabit bir hazırlık payı, sık sık küçük
                    // dolduran eşyayı (tabure) uzun dolduranın (yatak) önüne geçirmez.
                    double score = Math.Min(restore, missing) / (AutonomyDuration(interaction, item) + distance / TilesPerDayScaled + AutonomyOverheadDays * Scale);
                    // Sofra çeşidi: iki öğündür yenen yiyecek geri planda kalır (başka yiyecek varsa otonomi çeşitlenir).
                    if (need == NeedType.Hunger && interaction.ConsumeItem != null && RepeatsMeal(interaction.ConsumeItem)) score *= MoodBehavior.RepeatMealPenalty;
                    // Pişirme hevesi (açık kural): İyi ya da Mutlu karakter sıcak yemeği yeğler.
                    bool eager = eagerMood && interaction.Skill == SkillType.Cooking;
                    if (eager) score *= MoodBehavior.EagerCookBonus;
                    bool better = bestInteraction == null
                        || score > bestScore + 1e-9
                        || (Math.Abs(score - bestScore) <= 1e-9 && distance < bestDistance);
                    if (better)
                    {
                        bestItem = item;
                        bestInteraction = interaction;
                        bestScore = score;
                        bestDistance = distance;
                        _lastPickEager = eager;
                    }
                }
            }
        }

        private void Finish()
        {
            Queue.RemoveCurrent();
            ResetJobState();
            LastFailure = null;
        }

        private void Fail(string reason)
        {
            // Başarısız görev işi hemen yeniden seçilip döngüye girmesin.
            if (Queue.Current != null && Queue.Current.IsDuty) _dutyIdleUntil = _elapsedDays + DutyIdleDays;
            LastFailure = reason;
            Queue.RemoveCurrent();
            ResetJobState();
        }

        /// <summary>Su alınabilecek karolar (WaterNetwork.WaterAccess). Null ise su taşınmaz (eşyada su hazır sayılır).</summary>
        public Func<HashSet<GridPos>> WaterNetworkAccess { get; set; }
        /// <summary>Tesisata su geliyor mu: geliyorsa banyo için kova getirilmez (su işleri).</summary>
        public Func<ItemInstance, bool> PipedWater { get; set; }

        private bool _hasWater, _fetching;

        /// <summary>Şu an banyo için su taşıyor mu (eylem balonu).</summary>
        public bool IsFetchingWater => _fetching;

        // ---- Yarım kalan adım (18.11.2026: hızlı dokunuşta geri sıçrama ve takılma) ----
        // Karakter iki karonun arasındayken iş değişirse (yeni emir, iptal, öne alma, otonomi) konumu mantıkta hâlâ
        // çıktığı karodur. Eskiden adım ilerlemesi sıfırlanınca görünüm o karoya geri sıçrıyor, hızlı dokunuşta karakter
        // hiç ilerleyemiyordu. Şimdi yarım adım saklanır: yeni yol aynı yöne gidiyorsa adım kaldığı yerden sürer, başka yöne
        // gidiyorsa karakter adımın ortasından geri döner; iş yoksa adımı tamamlar.
        private GridPos? _carryTile;
        private double _carryFraction;

        /// <summary>Şu an iki karo arasında yarım kalmış bir adım var mı (test ve görünüm için).</summary>
        public bool HasCarriedStep => _carryTile.HasValue;

        private void CaptureCarry()
        {
            if (_path != null && !_using && _pathIndex > 0 && _pathIndex < _path.Count && _stepFraction > 1e-9)
            {
                _carryTile = _path[_pathIndex];
                _carryFraction = _stepFraction;
            }
        }

        private void ContinueCarriedStep()
        {
            if (!_carryTile.HasValue || _path == null || _pathIndex != 1 || _stepFraction > 0 || _path.Count == 0
                || !_path[0].Equals(_character.Position))
                return;
            var b = _carryTile.Value;
            double f = _carryFraction;
            _carryTile = null;
            if (_path.Count > 1 && _path[1].Equals(b))
            {
                _stepFraction = f;                     // aynı yöne: kaldığı yerden
                return;
            }
            // Başka yöne: adımı bitirip oradan gitmek daha kısaysa öyle (hızlı dokunuşta ileri-geri salınmasın) ...
            var goal = _path[_path.Count - 1];
            var fromB = Pathfinder.FindPath(_world.Map, b, goal);
            if (fromB != null && (1.0 - f) + (fromB.Count - 1) <= f + (_path.Count - 1))
            {
                fromB.Insert(0, _character.Position);
                _path.Clear();
                _path.AddRange(fromB);
                _stepFraction = f;
                return;
            }
            // ... değilse karakter adımın ortasından çıktığı karoya döner (görünüm aynı noktadan başlar).
            _character.Position = b;
            _path.Insert(0, b);
            _stepFraction = 1.0 - f;
        }

        private bool _settling;

        // İş yokken yarım adımı tamamlar; yol görünüm için kurulu kalır (NextTile, StepProgress).
        private double FinishCarriedStep(double budget)
        {
            if (!_settling)
            {
                var b = _carryTile.Value;
                _path = new List<GridPos> { _character.Position, b };
                _pathIndex = 1;
                _stepFraction = _carryFraction;
                _using = false;
                _carryTile = null;
                _settling = true;
            }
            double used = Walk(budget);
            if (_path == null || _pathIndex >= _path.Count)
            {
                _settling = false;
                _path = null; _pathIndex = 0; _stepFraction = 0;
                return Math.Max(used, 1e-12);
            }
            return budget;   // adım bu karede bitmedi
        }

        private void ResetJobState()
        {
            if (!_carryTile.HasValue) CaptureCarry();
            _settling = false;
            _character.Asleep = false;
            _hasWater = false;
            _fetching = false;
            _path = null;
            _pathIndex = 0;
            _stepFraction = 0;
            _using = false;
            _useProgress = 0;
            ClearChain();
        }
    }
}
