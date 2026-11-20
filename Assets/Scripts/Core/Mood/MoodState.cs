using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Mood
{
    /// <summary>Etkin bir moodlet ve kalan süresi (koşul moodlet'inde kalan süre anlamsızdır).</summary>
    public sealed class ActiveMoodlet
    {
        public MoodletDef Def { get; }
        public double RemainingDays { get; internal set; }
        internal ActiveMoodlet(MoodletDef def, double remaining) { Def = def; RemainingDays = remaining; }
    }

    /// <summary>
    /// Bir karakterin ruh hali: etkin moodlet'ler, toplam değer, basamak (Perişan ... Mutlu) ve
    /// becerilere etkisi. Aynı moodlet ikinci kez gelirse yığılmaz, süresi yenilenir.
    /// </summary>
    public sealed class MoodState
    {
        private readonly List<ActiveMoodlet> _active = new List<ActiveMoodlet>();

        public IReadOnlyList<ActiveMoodlet> Active => _active;

        /// <summary>Yeni bir moodlet eklendi (yenileme değil).</summary>
        public event Action<MoodletDef> Added;

        public int Total
        {
            get
            {
                int t = 0;
                foreach (var a in _active) t += a.Def.Value;
                return t;
            }
        }

        public MoodStatus Status => MoodRules.StatusFor(Total);

        public bool Has(string id) => Find(id) != null;

        private ActiveMoodlet Find(string id)
        {
            foreach (var a in _active) if (a.Def.Id == id) return a;
            return null;
        }

        /// <summary>Moodlet ekler ya da süresini yeniler. Yeni eklendiyse true.</summary>
        public bool Add(MoodletDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            var existing = Find(def.Id);
            if (existing != null)
            {
                existing.RemainingDays = Math.Max(existing.RemainingDays, def.DurationDays);
                return false;
            }
            _active.Add(new ActiveMoodlet(def, def.DurationDays));
            Added?.Invoke(def);
            return true;
        }

        /// <summary>Etkin moodlet'in kalan süresini çarpanla uzatır (kaliteli yemeğin etkisi, 53 §4.5). Etkin değilse false.</summary>
        public bool Extend(string id, double factor)
        {
            var a = Find(id);
            if (a == null || factor <= 1.0) return false;
            a.RemainingDays *= factor;
            return true;
        }

        public bool Remove(string id)
        {
            var a = Find(id);
            return a != null && _active.Remove(a);
        }

        /// <summary>Koşul moodlet'ini açar ya da kapatır.</summary>
        public void SetCondition(MoodletDef def, bool on)
        {
            if (on) { if (!Has(def.Id)) Add(def); }
            else Remove(def.Id);
        }

        /// <summary>Süreli moodlet'lerin süresini azaltır, bitenleri kaldırır.</summary>
        public void Tick(double gameDays)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var a = _active[i];
                if (a.Def.IsCondition) continue;
                a.RemainingDays -= gameDays;
                if (a.RemainingDays <= 0) _active.RemoveAt(i);
            }
        }

        /// <summary>Bir beceride kazanılan XP'nin çarpanı: basamak çarpanı × (1 + moodlet etkileri), en az 0,1.</summary>
        public double XpMultiplier(SkillType skill)
        {
            double bonus = 0;
            foreach (var a in _active) bonus += a.Def.XpBonus(skill);
            return Math.Max(0.1, MoodRules.XpMultiplier(Status) * (1.0 + bonus));
        }

        /// <summary>Bir beceri işinin hız çarpanı (süre buna bölünür), en az 0,3.</summary>
        public double SpeedMultiplier(SkillType skill)
        {
            double bonus = 0;
            foreach (var a in _active) bonus += a.Def.SpeedBonus(skill);
            return Math.Max(0.3, MoodRules.SpeedMultiplier(Status) * (1.0 + bonus));
        }

        /// <summary>Etkilerden gelen soğuk dayanıklılığı (toplam, en çok 0,6).</summary>
        public float ColdResist()
        {
            float r = 0f;
            foreach (var a in _active) r += a.Def.ColdResist;
            return Math.Min(0.6f, Math.Max(-0.5f, r));       // üşütmek dayanıklılığı eksiye düşürür
        }

        /// <summary>Etkilerden gelen acıkma ve yorulma çarpanları (çarpılır, en az 0,5).</summary>
        public float HungerDecayFactor()
        {
            float f = 1f;
            foreach (var a in _active) f *= a.Def.HungerDecay;
            return Math.Max(0.5f, f);
        }

        public float EnergyDecayFactor()
        {
            float f = 1f;
            foreach (var a in _active) f *= a.Def.EnergyDecay;
            return Math.Max(0.5f, f);
        }

        /// <summary>Kayıt için (kimlik, kalan gün).</summary>
        public List<KeyValuePair<string, double>> Export()
        {
            var list = new List<KeyValuePair<string, double>>();
            foreach (var a in _active) list.Add(new KeyValuePair<string, double>(a.Def.Id, a.RemainingDays));
            return list;
        }

        /// <summary>Kayıttan yükleme: bilinmeyen kimlikler atlanır, olay tetiklenmez.</summary>
        public void Restore(IEnumerable<KeyValuePair<string, double>> moodlets, MoodCatalog catalog)
        {
            _active.Clear();
            if (moodlets == null) return;
            foreach (var kv in moodlets)
                if (catalog.TryGet(kv.Key, out var def) && Find(def.Id) == null)
                    _active.Add(new ActiveMoodlet(def, def.IsCondition ? 0 : Math.Max(0, kv.Value)));
        }
    }
}
