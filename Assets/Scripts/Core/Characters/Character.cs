using System;
using Thatchwick.Core.Customization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Oyuncu karakteri ve NPC aynı varlık tipidir; fark yalnızca
    /// "oyuncu kontrolünde" bayrağıdır.
    /// </summary>
    public sealed class Character
    {
        public int Id { get; }
        public string Name { get; set; }
        public bool IsPlayerControlled { get; set; }
        public GridPos Position { get; set; }
        public NeedSet Needs { get; } = new NeedSet();
        public SkillSet Skills { get; } = new SkillSet();
        public Sex Sex { get; set; } = Sex.Female;
        public Appearance Appearance { get; set; } = new Appearance();

        // --- Hamilelik (T4 kenar durumları) ---
        /// <summary>Hamilelik süresi (oyun günü). Varsayılan 30 (yılın ~%75'i); denge turunda ayarlanır.</summary>
        public double PregnancyDays { get; set; } = 30.0;
        /// <summary>Doğumdan sonra yeniden hamile kalamama süresi (oyun günü).</summary>
        public double PostpartumDays { get; set; } = 10.0;
        public bool IsPregnant { get; private set; }
        public int? FatherId { get; private set; }
        private double _pregnancyLeft;
        private double _postpartumLeft;

        /// <summary>Doğumda (anne, babanın kimliği) ile bir kez tetiklenir. Ölürse doğum olmaz.</summary>
        public event Action<Character, int?> GaveBirth;

        /// <summary>Genç ya da Yetişkin, ölmemiş, hamile olmayan, doğum sonrası süresini doldurmuş kadın hamile kalabilir.</summary>
        public bool CanConceive =>
            !IsDead && Sex == Sex.Female && !IsPregnant && _postpartumLeft <= 0
            && (Stage == AgeStage.Youth || Stage == AgeStage.Adult);

        public bool TryStartPregnancy(int fatherId)
        {
            if (!CanConceive) return false;
            IsPregnant = true;
            FatherId = fatherId;
            _pregnancyLeft = PregnancyDays;
            return true;
        }

        /// <summary>Yaş (oyun günü). Doğumda 0.</summary>
        public double AgeDays { get; private set; }
        public AgeConfig AgeConfig { get; set; } = AgeConfig.Default;
        /// <summary>Doğal ölüm yaşı (oyun günü). Varsayılan üst sınır; yaratırken RollLifespan ile değişir.</summary>
        public double LifespanDays { get; set; } = AgeConfig.Default.MaxLifespanDay;
        public bool IsDead { get; private set; }
        public AgeStage Stage => AgeConfig.StageAt(AgeDays);

        /// <summary>Evre değişince (eski, yeni) ile tetiklenir.</summary>
        public event Action<Character, AgeStage, AgeStage> StageChanged;
        /// <summary>Doğal ölümde bir kez tetiklenir.</summary>
        public event Action<Character> Died;

        /// <summary>Çalışarak beceri XP'si kazanır; yaş evresine göre seviye tavanı uygulanır. Ölü karakter kazanmaz.</summary>
        public double GainSkillXp(SkillType skill, double xp)
        {
            if (IsDead) return 0;
            return Skills.AddXp(skill, xp, SkillRules.LevelCap(Stage));
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

            Needs.Tick(gameDays);

            var before = Stage;
            AgeDays += gameDays;
            var after = Stage;
            if (after != before) StageChanged?.Invoke(this, before, after);

            if (AgeDays >= LifespanDays)
            {
                IsDead = true;
                IsPregnant = false; // anne ölürse doğum olmaz
                FatherId = null;
                Died?.Invoke(this);
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
