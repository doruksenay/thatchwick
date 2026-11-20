using System;
using System.Collections.Generic;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Characters
{
    /// <summary>Aile üyesine verilen kalıcı görev (otomasyonun işçi tarafı).</summary>
    public enum DutyKind
    {
        None = 0,
        /// <summary>Tarla: susuz ekini sula, olgun ekini ve meyveyi topla.</summary>
        Farm = 1,
        /// <summary>Hayvanlar: yemliği doldur, ürünü topla, sev; kemente düşen avı topla.</summary>
        Animals = 2,
        /// <summary>Maden: işaretli karoları kaz, çatırdayana destek kur, suyu boşalt (arsa kazısı; ocakta sabah vardiyası).</summary>
        Mine = 3,
        /// <summary>Kâhya: her sabah öbür aile üyelerine bekleyen işe göre görev dağıtır (kendisi tarlaya inmez).</summary>
        Steward = 4,
        /// <summary>Ev: mum değiştirir, sepeti tazeler, döküntüyü toplar, aynayı cilalar, saksıyı sular, ot serper, badana yapar (belge 45).</summary>
        House = 5,
        /// <summary>Mutfak: oyuncunun öğün çizelgesindeki tarifi pişirir, sofrayı kurar ve toplar, bulaşığı yıkar, fazlayı saklar (belge 58 §9, 58a §4).</summary>
        Kitchen = 6
    }

    /// <summary>
    /// Oyuncu karakteri ve NPC aynı varlık tipidir; fark yalnızca
    /// "oyuncu kontrolünde" bayrağıdır.
    /// </summary>
    public sealed class Character
    {
        public int Id { get; }
        public string Name { get; set; }
        public bool IsPlayerControlled { get; set; }
        /// <summary>Hanede verilen görev: ihtiyaçları iyiyken iş saatinde kendiliğinden yapar (tarla, hayvanlar).</summary>
        public DutyKind Duty { get; set; }
        public GridPos Position { get; set; }
        public NeedSet Needs { get; } = new NeedSet();
        public SkillSet Skills { get; } = new SkillSet();
        public Sex Sex { get; set; } = Sex.Female;
        public Appearance Appearance { get; set; } = new Appearance();
        /// <summary>Gizli kalıtım verisi (oyuncuya gösterilmez). Null ise doğumda görünümden kurulur.</summary>
        public Genome Genome { get; set; }

        // --- Karakter yaratma 2.0 (doküman 14) ---
        /// <summary>Doğum günü: mevsim (0-3) ve mevsim günü (0'dan). -1 = belirsiz (eski kayıt).</summary>
        public int BirthSeason { get; set; } = -1;
        public int BirthDayOfSeason { get; set; } = -1;
        /// <summary>Sevdikleri ve sevmedikleri (hediye tepkisi, küçük moodlet). Boş = belirsiz.</summary>
        public string LikedFood { get; set; } = "";
        public string LikedColor { get; set; } = "";
        public string LikedHobby { get; set; } = "";
        public string DislikedFood { get; set; } = "";
        /// <summary>Çocuk Ergen evresine geçince bir kez açılan ücretsiz Ayna hakkı (karar 17.11.2026).</summary>
        public bool FreeMirrorAvailable { get; set; }
        /// <summary>İyileşmekte olan yara (kaza); yoksa null.</summary>
        public ActiveInjury Injury { get; set; }
        /// <summary>Son kaza günü (mevsimde en çok bir kaza); -1 = hiç.</summary>
        public double LastAccidentDay { get; set; } = -1;
        /// <summary>Kazadan kalmış iz sayısı (görünmeyenler dahil).</summary>
        public int EventScarCount
        {
            get
            {
                int n = 0;
                foreach (var m in Appearance.Marks) if (m.Kind == MarkKind.Scar && m.Source == MarkSource.Event) n++;
                return n;
            }
        }

        // --- Ruh hali (keyif, hijyen, moodlet) ---
        public MoodState Mood { get; } = new MoodState();
        /// <summary>
        /// Ruh hali sistemi açık mı: keyif ve hijyen azalır, ihtiyaç moodlet'leri açılır, ruh hali beceri
        /// XP'sini ve iş hızını etkiler. Oyunda hane üyeleri için açıktır (GameRoot); köylüler ve yalnızca
        /// biyolojiyi ya da işi sınayan testler etkilenmesin diye varsayılan kapalı (CanStarve gibi).
        /// </summary>
        public bool MoodEnabled { get; set; }
        /// <summary>Keyif ve hijyen azalıyor mu: ruh hali açık ve bebek değil.</summary>
        public bool ComfortNeedsActive => MoodEnabled && Stage != AgeStage.Baby;

        /// <summary>Katalogdaki moodlet'i ekler (ruh hali kapalıysa ya da ölüyse eklemez).</summary>
        public bool AddMoodlet(string id) => AddMoodlet(id, MoodCatalog.Default);

        public bool AddMoodlet(string id, MoodCatalog catalog)
        {
            if (!MoodEnabled || IsDead || string.IsNullOrEmpty(id)) return false;
            if (!catalog.TryGet(id, out var def)) return false;
            Mood.Add(def);
            return true;
        }

        /// <summary>Beceri işinin hız çarpanı (ruh hali kapalıysa 1); yaşlılık sıkıntıları (belge 51 §3) ayrıca çarpar.</summary>
        public double WorkSpeed(SkillType skill) => (MoodEnabled ? Mood.SpeedMultiplier(skill) : 1.0) * (ConditionSpeed?.Invoke(skill) ?? 1.0);

        /// <summary>Yaşlılık sıkıntılarının iş hızı çarpanı (null = 1).</summary>
        public Func<SkillType, double> ConditionSpeed { get; set; }
        /// <summary>Yaşlılık sıkıntılarının XP çarpanı (eksi; null = 1).</summary>
        public Func<SkillType, double> ConditionXp { get; set; }
        /// <summary>Yürüme çarpanı (dizi tutmayan yaşlı; 1 = etkisiz).</summary>
        public double WalkFactor { get; set; } = 1.0;
        /// <summary>Isınma ihtiyacı çarpanı ("Kanım çekildi" ×1,3; 1 = etkisiz).</summary>
        public float WarmthNeedFactor { get; set; } = 1f;

        private void UpdateNeedMoodlets(MoodCatalog catalog)
        {
            SetNeedMood(catalog, MoodCatalog.Hungry, Needs.Get(NeedType.Hunger) < MoodCatalog.HungerTiredMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Tired, Needs.Get(NeedType.Energy) < MoodCatalog.HungerTiredMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Cold, Needs.Get(NeedType.Warmth) < MoodCatalog.ColdMoodThreshold);
            bool comfort = ComfortNeedsActive;
            SetNeedMood(catalog, MoodCatalog.Bored, comfort && Needs.Get(NeedType.Fun) < MoodCatalog.NeedMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Dirty, comfort && Needs.Get(NeedType.Hygiene) < MoodCatalog.NeedMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Lonely, comfort && Needs.Get(NeedType.Social) < MoodCatalog.NeedMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Bursting, comfort && Needs.Get(NeedType.Bladder) < MoodCatalog.NeedMoodThreshold);
            SetNeedMood(catalog, MoodCatalog.Wet, Wetness > 0.05f);
            UpdateTraitMood();
        }

        private int _traitMoodApplied;
        private void UpdateTraitMood()
        {
            int want = Modifiers.MoodBase;
            if (want == _traitMoodApplied && (want == 0 || Mood.Has("mood.trait_base"))) return;
            Mood.Remove("mood.trait_base");
            _traitMoodApplied = want;
            if (want != 0)
                Mood.Add(new MoodletDef { Id = "mood.trait_base", DisplayName = "Mizaç", Description = "Özelliklerin verdiği genel ruh hali.", Value = want, DurationDays = 0 });
        }

        private void SetNeedMood(MoodCatalog catalog, string id, bool on)
        {
            if (catalog.TryGet(id, out var def)) Mood.SetCondition(def, on);
        }


        // --- Özellikler ve meslek dalları (Adım 18a, 40) ---
        private readonly List<string> _traitIds = new List<string>();
        private readonly List<string> _branchIds = new List<string>();
        /// <summary>Özellik kimlikleri (en çok 2). Etkileri LifeRules.Recompute ile Modifiers'a işlenir.</summary>
        public IReadOnlyList<string> TraitIds => _traitIds;
        /// <summary>Seçilmiş meslek dalı kimlikleri.</summary>
        public IReadOnlyList<string> BranchIds => _branchIds;
        /// <summary>Özellik, dal ve köy projelerinden gelen çarpanlar; varsayılan "etki yok".</summary>
        public LifeModifiers Modifiers { get; set; } = new LifeModifiers();
        public const int MaxTraits = 2;

        public void SetTraits(IEnumerable<string> ids)
        {
            _traitIds.Clear();
            if (ids == null) return;
            foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id) || _traitIds.Contains(id)) continue;
                if (_traitIds.Count >= MaxTraits) break;
                _traitIds.Add(id);
            }
        }

        public void SetBranches(IEnumerable<string> ids)
        {
            _branchIds.Clear();
            if (ids == null) return;
            foreach (var id in ids) if (!string.IsNullOrWhiteSpace(id) && !_branchIds.Contains(id)) _branchIds.Add(id);
        }

        public bool HasBranch(string id) => id != null && _branchIds.Contains(id);
        public bool HasTrait(string id) => id != null && _traitIds.Contains(id);

        public bool AddBranch(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || _branchIds.Contains(id)) return false;
            _branchIds.Add(id);
            return true;
        }

        /// <summary>
        /// Anlık (katalog dışı) moodlet: olay metni ve değeri çağıran verir (şenlik, yas, oda konforu, köy projesi).
        /// days &lt; 0 süresiz koşuldur (Remove ile kalkar). Ruh hali kapalıysa ya da ölüyse eklemez.
        /// Aynı kimlik varsa değer ve süre yenilenir.
        /// </summary>
        public bool AddMoodlet(string id, string text, int value, double days)
        {
            if (!MoodEnabled || IsDead || string.IsNullOrEmpty(id)) return false;
            Mood.Remove(id);
            Mood.Add(new MoodletDef { Id = id, DisplayName = text, Description = text, Value = value, DurationDays = days < 0 ? 0 : Math.Max(0.001, days) });
            return true;
        }

        /// <summary>Ruh hali basamağı (Perişan ... Mutlu); ruh hali kapalıysa Nötr.</summary>
        public MoodStatus MoodStatus => MoodEnabled ? Mood.Status : MoodStatus.Neutral;

        // --- Aile (Adım 16-17) ---
        public int? BirthMotherId { get; set; }
        public int? BirthFatherId { get; set; }
        public int? SpouseId { get; set; }
        /// <summary>Hanedandaki nesil (kurucu 1, çocukları 2 ...).</summary>
        public int Generation { get; set; } = 1;
        /// <summary>Hane üyesi değil, köylü (pazar çevresinde yaşar; evlenince haneye katılır).</summary>
        public bool IsVillager { get; set; }

        // --- Hamilelik (T4 kenar durumları) ---
        /// <summary>Hamilelik süresi (oyun günü). Varsayılan 30 (yılın ~%75'i); denge turunda ayarlanır.</summary>
        public double PregnancyDays { get; set; } = 5.0;   // 17.11.2026: 1 oyun günü ≈ yarım yıl oranı (önce 30, ömrün dörtte biriydi)
        /// <summary>Doğumdan sonra yeniden hamile kalamama süresi (oyun günü).</summary>
        public double PostpartumDays { get; set; } = 10.0;
        public bool IsPregnant { get; private set; }
        public int? FatherId { get; private set; }
        private double _pregnancyLeft;
        private double _postpartumLeft;

        /// <summary>Doğumda (anne, babanın kimliği) ile bir kez tetiklenir. Ölürse doğum olmaz.</summary>
        public event Action<Character, int?> GaveBirth;

        /// <summary>Genç yetişkin ya da Yetişkin (Ergen değil), ölmemiş, hamile olmayan, doğum sonrası süresini doldurmuş kadın hamile kalabilir.</summary>
        public bool CanConceive =>
            !IsDead && Sex == Sex.Female && !IsPregnant && _postpartumLeft <= 0
            && AgeStages.CanConceive(Stage);

        public bool TryStartPregnancy(int fatherId)
        {
            if (!CanConceive) return false;
            IsPregnant = true;
            FatherId = fatherId;
            _pregnancyLeft = PregnancyDays;
            return true;
        }

        /// <summary>Kayıt için iç durum (yalnızca Save sistemi kullanır).</summary>
        internal double PregnancyLeft => _pregnancyLeft;
        internal double PostpartumLeft => _postpartumLeft;

        /// <summary>Ebe doğumda yanındaydı (belge 66g karar 2): doğum sonrası toparlanma kısalır.</summary>
        public void ShortenPostpartum(double share)
        {
            if (_postpartumLeft > 0 && share > 0) _postpartumLeft *= 1.0 - Math.Min(1.0, share);
        }

        /// <summary>Kayıttan yükleme: biyolojik iç durumu olduğu gibi geri koyar (olay tetiklemez).</summary>
        internal void RestoreBiology(double ageDays, double lifespan, bool dead, DeathCause cause, bool pregnant, int? fatherId,
            double pregnancyLeft, double postpartumLeft, double freezeDays, double starveDays)
        {
            AgeDays = ageDays;
            LifespanDays = lifespan;
            IsDead = dead;
            CauseOfDeath = cause;
            IsPregnant = pregnant;
            FatherId = fatherId;
            _pregnancyLeft = pregnancyLeft;
            _postpartumLeft = postpartumLeft;
            FreezeDays = freezeDays;
            StarveDays = starveDays;
        }

        /// <summary>Yaş (oyun günü). Doğumda 0.</summary>
        public double AgeDays { get; private set; }
        public AgeConfig AgeConfig { get; set; } = AgeConfig.Default;
        /// <summary>Doğal ölüm yaşı (oyun günü). Varsayılan üst sınır; yaratırken RollLifespan ile değişir.</summary>
        public double LifespanDays { get; set; } = AgeConfig.Default.MaxLifespanDay;
        public bool IsDead { get; private set; }

        // --- Soğuk (T1: kışta donma riski) ---
        /// <summary>Karakterin bulunduğu yerin sıcaklığı (°C). Varsayılan konforlu; HeatSystem her karede günceller.</summary>
        public float AmbientTemperature { get; set; } = 20f;
        public const float ComfortTemperature = 10f;
        /// <summary>Konfor sıcaklığının altında her °C için günlük Isınma kaybı.</summary>
        public const float WarmthLossPerDegreeDay = 4f;
        public const float WarmthRecoveryPerDay = 60f;
        /// <summary>Isınma 0'da bu kadar gün kalırsa donarak ölür.</summary>
        public const double FreezeDeathDays = 2.0;
        public double FreezeDays { get; private set; }

        /// <summary>Açlık 0'da bu kadar gün kalırsa açlıktan ölür.</summary>
        public const double StarveDeathDays = 3.0;
        public double StarveDays { get; private set; }
        /// <summary>
        /// Açlıktan ölüm açık mı. Oyunda açıktır (GameRoot); yalnızca biyolojiyi (yaş, hamilelik) sınayan
        /// testler yemek döngüsü kurmadan uzun süre ilerletebilsin diye varsayılan kapalı.
        /// </summary>
        public bool CanStarve { get; set; }
        /// <summary>Soğuktan donarak ölebilir mi. Köylüler ölmez (belge 66f §A.1): soğuk kasvet, hastalık ve rica doğurur.</summary>
        public bool CanFreeze { get; set; } = true;
        public DeathCause CauseOfDeath { get; private set; }
        public AgeStage Stage => AgeConfig.StageAt(AgeDays);

        /// <summary>Evre değişince (eski, yeni) ile tetiklenir.</summary>
        public event Action<Character, AgeStage, AgeStage> StageChanged;
        /// <summary>Doğal ölümde bir kez tetiklenir.</summary>
        public event Action<Character> Died;

        /// <summary>Ölüm sebebini kaydeder, gebeliği keser ve Died olayını tetikler.</summary>
        private void Die(DeathCause cause)
        {
            IsDead = true;
            CauseOfDeath = cause;
            IsPregnant = false; // anne ölürse doğum olmaz
            FatherId = null;
            Died?.Invoke(this);
        }

        // ---- Atletizm (18.11.2026) ----
        /// <summary>Atletizm: seviye başına %2 yürüme hızı (10. seviyede %18).</summary>
        public const double AthleticSpeedPerLevel = 0.02;
        /// <summary>Atletizm: seviye başına %2 daha yavaş yorulma (10. seviyede %18).</summary>
        public const float AthleticEnergyPerLevel = 0.02f;
        /// <summary>Atletizm: seviye başına %1 soğuğa dayanma (10. seviyede %9).</summary>
        public const float AthleticColdPerLevel = 0.01f;
        /// <summary>Yürünen her karo için Atletizm XP'si.</summary>
        public const double AthleticXpPerTile = 0.05;

        private int AthleticSteps => Math.Max(0, Skills.GetLevel(SkillType.Athletics) - 1);
        public double AthleticSpeedFactor => 1.0 + AthleticSpeedPerLevel * AthleticSteps;
        public float AthleticEnergyFactor => 1f - AthleticEnergyPerLevel * AthleticSteps;
        public float AthleticColdResist => AthleticColdPerLevel * AthleticSteps;

        /// <summary>Yetiştirilme sistemlerinden gelen ek XP çarpanı (çocukluk izi, can yoldaşı, çıraklık, aile tarifi).</summary>
        public Func<SkillType, double> ExtraXpFactor { get; set; }

        /// <summary>Çalışarak beceri XP'si kazanır; yaş evresine göre seviye tavanı uygulanır. Ölü karakter kazanmaz.</summary>
        public double GainSkillXp(SkillType skill, double xp)
        {
            if (IsDead) return 0;
            // Beceri başına ölçek (süre tablosu, belge 45 bölüm 4); doygunluk gibi yalnızca tempo açıkken (hane üyeleri).
            if (Skills.PacingEnabled) xp *= SkillPacing.XpScale(skill);
            // Belge 45 karar 4: XP bonusları toplanır (ruh hali ve moodlet'ler, Okuryazarlık, yetiştirme: çocukluk izi,
            // can yoldaşı, çıraklık, aile tarifi; dal ve özellik hızı), toplam en çok +%100. Eksiler (yas, bunalım)
            // çarpan olarak kalır. Yatkınlık ayrı çarpandır.
            double bonus = 0.0, penalty = 1.0;
            void Pool(double f)
            {
                if (f > 1.0) bonus += f - 1.0;
                else if (f < 1.0) penalty *= Math.Max(0.0, f);
            }
            if (MoodEnabled) Pool(Mood.XpMultiplier(skill));
            // Okuryazarlık: bütün becerilerde seviye başına %2 fazla XP (kendisi hariç).
            if (skill != SkillType.Literacy) Pool(SkillNames.Factor(Skills.GetLevel(SkillType.Literacy), 0.02));
            if (ExtraXpFactor != null) Pool(ExtraXpFactor(skill));
            if (ConditionXp != null) Pool(ConditionXp(skill));
            Pool(Modifiers.SkillRate(skill));
            xp *= penalty * (1.0 + Math.Min(bonus, SkillPacing.MaxXpBonus));
            // Yatkınlık (Project belgesi 28): XP x1,5 / x2 ve o işi yaparken küçük keyif.
            var aptitude = Skills.GetAptitude(skill);
            if (aptitude != Aptitude.None)
            {
                xp *= SkillPacing.XpMultiplier(aptitude);
                if (MoodEnabled && xp > 0 && !Mood.Has(AptitudeMoodId))
                    Mood.Add(new MoodletDef
                    {
                        Id = AptitudeMoodId, DisplayName = "Sevdiği iş",
                        Description = "Yatkın olduğu işi yapıyor.", Value = AptitudeMoodValue, DurationDays = AptitudeMoodDays
                    });
            }
            // Günlük doygunluk: eşikten sonra XP %25 (yalnızca tempo açıkken).
            xp = Skills.ApplyDailySaturation(skill, xp);
            return Skills.AddXp(skill, xp, SkillRules.LevelCap(Stage));
        }

        public const string AptitudeMoodId = "mood.aptitude_work";
        public const int AptitudeMoodValue = 3;
        public const double AptitudeMoodDays = 0.25;

        /// <summary>Tuvalete yetişemedi (bildirim için).</summary>
        public event Action<Character> Accident;
        /// <summary>Tuvalet ihtiyacının azalma çarpanı (karın ağrısında 2; belge 57b Z2). Kayda girmez, ruh halinden türetilir.</summary>
        public float BladderFactor { get; set; } = 1f;

        // ---- ıslaklık ----

        /// <summary>Islaklık 0-1: suya giren, dışarıda yıkanan, yağmurda kalan ıslanır; ısı ve havaya göre kurur.</summary>
        public float Wetness { get; private set; }

        /// <summary>Yatakta uyuyor (JobRunner): tuvalet uykuda yavaş dolar.</summary>
        public bool Asleep { get; set; }

        /// <summary>
        /// Süreli görevde (belge 47 §3): haritadan çıktı, madende ya da gölde çalışıyor. Sahnede çizilmez, iş koşucusu işlemez;
        /// ihtiyaçlarını Work.TimedDutyService yürütür (azıktan yer).
        /// </summary>
        public bool Away { get; set; }
        public const float SleepBladderFactor = 0.35f;
        // Islak ve soğukta geçen süre (üşütme için).
        private double _wetColdDays;

        /// <summary>Islanır (0-1, en ıslak olan kalır).</summary>
        public void Soak(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Wetness = Math.Min(1f, Math.Max(Wetness, amount));
        }

        /// <summary>Giysilerin soğuğa dayanıklılığı (belge 48 §12.1; ClothingService günceller).</summary>
        public float ClothingColdResist { get; set; }
        /// <summary>Giysilerin yağmurda ıslanmayı azaltması (0-1).</summary>
        public float RainGuard { get; set; }

        /// <summary>Yağmurda ıslanır: giysi koruması kadar az (kepenek, yağmurluk ıslaklık biriktirmez).</summary>
        public void SoakRain(float amount)
        {
            float f = 1f - Math.Max(0f, Math.Min(1f, RainGuard));
            if (f > 0f) Soak(amount * f);
        }

        /// <summary>Kurulanır (havlu): ıslaklık hemen gider.</summary>
        public void Dry() { Wetness = 0f; _wetColdDays = 0; }

        /// <summary>Islakken soğukta (bu sıcaklığın altında) bu kadar gün kalan üşütür.</summary>
        public const float ChillTemperature = 8f;
        public const double ChillDays = 0.15;

        /// <summary>Kuruma (gün başına): 2 + (sıcaklık - 5) x 0,3; ocak başında birkaç saatte, kışın dışarıda yarım günde.</summary>
        public static double DryPerDay(float temp) => 2.0 + Math.Max(0f, temp - 5f) * 0.3;

        private void TickWetness(double gameDays)
        {
            if (Wetness <= 0f) { _wetColdDays = 0; return; }
            Wetness = (float)Math.Max(0.0, Wetness - DryPerDay(AmbientTemperature) * gameDays);
            if (AmbientTemperature < ChillTemperature && Wetness > 0.3f)
            {
                _wetColdDays += gameDays;
                if (_wetColdDays >= ChillDays && MoodEnabled && !Mood.Has(MoodCatalog.Chilled)) AddMoodlet(MoodCatalog.Chilled);
            }
            else _wetColdDays = Math.Max(0.0, _wetColdDays - gameDays);
        }

        // --- Ruh halinin davranışa etkisi (18.11.2026, Project belgesi 23) ---

        /// <summary>Süren bunalım; yoksa None. Bunalımda karakter iş yapmaz, oyuncunun emri sıraya girer.</summary>
        public BreakdownKind Breakdown { get; private set; }
        /// <summary>Bunalımın kalan süresi (oyun günü).</summary>
        public double BreakdownLeft { get; private set; }
        public bool InBreakdown => Breakdown != BreakdownKind.None;
        /// <summary>Kesintisiz Perişan kalınan süre (bunalım bundan sonra gelebilir; önce uyarı).</summary>
        public double MiserableDays { get; set; }
        /// <summary>Son bunalımın bittiği gün (yaş günü cinsinden; bekleme süresi için). Yoksa -1.</summary>
        public double LastBreakdownAge { get; set; } = -1;

        /// <summary>Bunalım bitti (süre doldu ya da hayati bir ihtiyaç kritikleşti). İkinci değer: süre dolmadan kesildi mi.</summary>
        public event Action<Character, BreakdownKind, bool> BreakdownEnded;

        public bool BeginBreakdown(BreakdownKind kind, double days)
        {
            if (kind == BreakdownKind.None || IsDead || InBreakdown || days <= 0) return false;
            Breakdown = kind;
            BreakdownLeft = days;
            return true;
        }

        /// <summary>Bunalımı bitirir: "İçini döktü" moodlet'i (+20, 2 gün) sarmalı önler.</summary>
        public void EndBreakdown(bool interrupted = false)
        {
            if (!InBreakdown) return;
            var kind = Breakdown;
            Breakdown = BreakdownKind.None;
            BreakdownLeft = 0;
            MiserableDays = 0;
            LastBreakdownAge = AgeDays;
            if (MoodEnabled) AddMoodlet(MoodCatalog.Catharsis);
            BreakdownEnded?.Invoke(this, kind, interrupted);
        }

        /// <summary>
        /// Teselli (belge 66f §B.10): kalan bunalım süresinin <paramref name="share"/> kadarı gider; 1 ya da çok kısa kalırsa
        /// bunalım biter (kesilmiş sayılmaz).
        /// </summary>
        public void ShortenBreakdown(double share)
        {
            if (!InBreakdown || share <= 0) return;
            BreakdownLeft *= 1.0 - Math.Min(1.0, share);
            if (BreakdownLeft <= 0.001) EndBreakdown();
        }

        /// <summary>Kayıttan yükleme.</summary>
        public void RestoreBreakdown(BreakdownKind kind, double left)
        {
            Breakdown = left > 0 ? kind : BreakdownKind.None;
            BreakdownLeft = Breakdown == BreakdownKind.None ? 0 : left;
        }

        private void TickBreakdown(double gameDays)
        {
            if (MoodEnabled && Mood.Status == MoodStatus.Miserable) MiserableDays += gameDays;
            else if (!InBreakdown) MiserableDays = 0;
            if (!InBreakdown) return;
            // Hayati güvenlik: açlık, uyku ya da soğuk kritikleşince bunalım biter; karakter ihtiyacını giderir.
            if (Needs.AnyCritical()) { EndBreakdown(interrupted: true); return; }
            BreakdownLeft -= gameDays;
            if (BreakdownLeft <= 0) EndBreakdown();
        }

        /// <summary>Çocuk ve Ergen evresinde ruh hali toplamının zaman ağırlıklı toplamı ve ölçülen gün (yetişme).</summary>
        public double UpbringingMoodSum { get; set; }
        public double UpbringingDays { get; set; }
        public double UpbringingAverage => UpbringingDays > 0 ? UpbringingMoodSum / UpbringingDays : 0.0;

        private void TickUpbringing(double gameDays)
        {
            if (!MoodEnabled) return;
            var s = Stage;
            if (s != AgeStage.Child && s != AgeStage.Teen) return;
            UpbringingMoodSum += Mood.Total * gameDays;
            UpbringingDays += gameDays;
        }

        // Kişinin hayat notları (hanedan geçmişinde gösterilir): "Neşeli bir soydan geldi ama zor bir çocukluk geçirdi".
        private readonly List<string> _chronicle = new List<string>();
        public IReadOnlyList<string> Chronicle => _chronicle;
        public void AddChronicle(string note) { if (!string.IsNullOrWhiteSpace(note)) _chronicle.Add(note); }
        public void RestoreChronicle(IEnumerable<string> notes)
        {
            _chronicle.Clear();
            if (notes != null) foreach (var n in notes) AddChronicle(n);
        }

        // Son üç öğün (çeşit ödülü); kayda girmez, oturum içinde tutulur.
        private readonly List<string> _meals = new List<string>(4);
        public IReadOnlyList<string> RecentMeals => _meals;

        /// <summary>
        /// Bir öğün yedi: üst üste üç kez aynı yemek "Hep aynı yemek" (-3), son üç öğün üçü de farklıysa "Zengin sofra" (+4).
        /// Elde başka yiyecek yoksa (hadAlternative false) "Hep aynı yemek" gelmez: oyuncu, seçeneği olmadığı için cezalanmaz.
        /// </summary>
        public void RecordMeal(string foodId, bool hadAlternative = true)
        {
            if (string.IsNullOrEmpty(foodId)) return;
            _meals.Add(foodId);
            if (_meals.Count > 3) _meals.RemoveAt(0);
            if (!MoodEnabled || _meals.Count < 3) return;
            if (_meals[0] == foodId && _meals[1] == foodId) { if (hadAlternative) AddMoodlet(MoodCatalog.SameMeal); }
            else if (_meals[0] != _meals[1] && _meals[1] != _meals[2] && _meals[0] != _meals[2]) AddMoodlet(MoodCatalog.VariedTable);
        }

        /// <summary>Hekimlik: soğuk ve açlık yarasını (ölüme sayan günleri) bu kadar gün azaltır. Bir şey iyileştiyse true.</summary>
        public bool Treat(double days)
        {
            if (IsDead || days <= 0) return false;
            bool chilled = MoodEnabled && Mood.Remove(MoodCatalog.Chilled);   // hekim üşütmeyi de geçirir
            if (FreezeDays <= 0 && StarveDays <= 0) return chilled;
            FreezeDays = Math.Max(0.0, FreezeDays - days);
            StarveDays = Math.Max(0.0, StarveDays - days);
            return true;
        }

        /// <summary>Karakteri belirli bir yaşta başlatır (başlangıç karakteri, test).</summary>
        public void SetAge(double ageDays)
        {
            if (ageDays < 0) throw new ArgumentOutOfRangeException(nameof(ageDays));
            AgeDays = ageDays;
        }

        public Character(int id, string name, GridPos position, bool isPlayerControlled = false)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("İsim boş olamaz.", nameof(name));
            Id = id;
            Name = name;
            Position = position;
            IsPlayerControlled = isPlayerControlled;
        }

        public void Tick(double gameDays)
        {
            if (IsDead) return;

            // Soğuk: konfor sıcaklığının altında Isınma düşer, üstünde toparlanır; 0'da kalırsa donar.
            float cold = ComfortTemperature - AmbientTemperature;
            if (cold > 0f)
            {
                // Sıcak çorba gibi yemek etkileri de soğuğa dayanıklılık verir (toplam en çok %90).
                float resist = Math.Min(0.9f, Modifiers.ColdResist + (MoodEnabled ? Mood.ColdResist() : 0f) + AthleticColdResist + ClothingColdResist);
                // Islak giysi soğukta ısıyı iki kat hızlı kaçırır.
                Needs.SetDecayPerDay(NeedType.Warmth, cold * WarmthLossPerDegreeDay * (1f - resist) * (1f + Wetness) * WarmthNeedFactor);
            }
            else
            {
                Needs.SetDecayPerDay(NeedType.Warmth, 0f);
                Needs.Restore(NeedType.Warmth, (float)(WarmthRecoveryPerDay * gameDays));
            }

            // Özellik ve dallardan gelen ihtiyaç azalma çarpanları.
            Needs.SetDecayMultiplier(NeedType.Hunger, Modifiers.HungerDecay * (MoodEnabled ? Mood.HungerDecayFactor() : 1f));
            // Uyurken enerji azalmaz (belge 65c B2): uyku enerjiyi bitişte tek seferde verir; eskiden uyku boyunca azalma
            // sürdüğü için 8 saatlik uyku net ~40 kazandırıyordu ve karakter gündüz yeniden yatıyordu.
            Needs.SetDecayMultiplier(NeedType.Energy, Asleep ? 0f
                : Modifiers.EnergyDecay * (MoodEnabled ? Mood.EnergyDecayFactor() : 1f) * AthleticEnergyFactor);
            Needs.SetDecayMultiplier(NeedType.Social, Modifiers.SocialDecay);
            Needs.SetDecayMultiplier(NeedType.Fun, Modifiers.FunDecay);
            Needs.SetDecayMultiplier(NeedType.Bladder, (Asleep ? SleepBladderFactor : 1f) * BladderFactor);

            // Keyif ve hijyen yalnızca ruh hali açıkken (ve bebek değilken) azalır.
            float fun = Needs.Get(NeedType.Fun), hygiene = Needs.Get(NeedType.Hygiene), social = Needs.Get(NeedType.Social), bladder = Needs.Get(NeedType.Bladder);
            Needs.Tick(gameDays);
            if (!ComfortNeedsActive)
            {
                Needs.Set(NeedType.Fun, fun);
                Needs.Set(NeedType.Hygiene, hygiene);
                Needs.Set(NeedType.Social, social);
                Needs.Set(NeedType.Bladder, bladder);
            }
            else if (Needs.Get(NeedType.Bladder) <= NeedSet.Min)
            {
                // Kaza: tuvalete yetişemedi. Ölmez; hijyen sıfırlanır, utanır.
                Needs.Set(NeedType.Bladder, NeedSet.Max);
                Needs.Set(NeedType.Hygiene, NeedSet.Min);
                if (MoodEnabled) AddMoodlet(MoodCatalog.Accident);
                Accident?.Invoke(this);
            }
            TickWetness(gameDays);
            if (MoodEnabled)
            {
                Mood.Tick(gameDays);
                UpdateNeedMoodlets(MoodCatalog.Default);
            }
            TickBreakdown(gameDays);
            TickUpbringing(gameDays);

            if (Needs.Get(NeedType.Warmth) <= NeedSet.Min) FreezeDays += gameDays;
            else FreezeDays = Math.Max(0.0, FreezeDays - 2.0 * gameDays);
            if (FreezeDays >= FreezeDeathDays && CanFreeze)
            {
                Die(DeathCause.Cold);
                return;
            }

            // Bebeği hanenin büyükleri besler (bakım işi); beslenmeyen bebek de açlıktan ölebilir.
            if (CanStarve && Needs.Get(NeedType.Hunger) <= NeedSet.Min) StarveDays += gameDays;
            else StarveDays = Math.Max(0.0, StarveDays - 2.0 * gameDays);
            if (StarveDays >= StarveDeathDays)
            {
                Die(DeathCause.Starvation);
                return;
            }

            var before = Stage;
            AgeDays += gameDays;
            var after = Stage;
            if (after != before)
            {
                // Çocuk Ergen'e geçerken bir kez ücretsiz Ayna (karar 17.11.2026).
                if (before == AgeStage.Child && after == AgeStage.Teen) FreeMirrorAvailable = true;
                StageChanged?.Invoke(this, before, after);
            }

            if (AgeDays >= LifespanDays)
            {
                Die(DeathCause.OldAge);
                return;
            }

            if (_postpartumLeft > 0) _postpartumLeft = Math.Max(0, _postpartumLeft - gameDays);

            if (IsPregnant)
            {
                _pregnancyLeft -= gameDays;
                if (_pregnancyLeft <= 0)
                {
                    int? father = FatherId;
                    IsPregnant = false;
                    FatherId = null;
                    _postpartumLeft = PostpartumDays;
                    GaveBirth?.Invoke(this, father);
                }
            }
        }
    }
}
