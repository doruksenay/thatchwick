using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Family
{
    /// <summary>Hanedanın baş karakter geçmişi (soy ağacı ve sıralamalar için).</summary>
    public sealed class HeadRecord
    {
        public int CharacterId;
        public string Name;
        public int Generation;
        public double StartDay;
        public double EndDay = -1;
        public DeathCause EndCause;
        /// <summary>Başın mesleği (seçtiği dallardan; yoksa boş): soy ağacında "Oduncu" gibi gösterilir.</summary>
        public string Trade;
    }

    /// <summary>Baş karakter ölünce kimin geçeceği ve mirasın sonucu.</summary>
    public sealed class SuccessionResult
    {
        public Character Heir;
        public bool DynastyEnded;
        public string Message;
    }

    /// <summary>
    /// Hanedan ve miras (Adım 17, T8): baş karakter (oyuncu kontrollü) ölünce sıradaki baş karakter seçilir:
    /// 1) yaşayan çocuklarından genç/yetişkin olan en büyük, 2) çocuk evresindeki en büyük çocuğu,
    /// 3) eşi, 4) hanenin bebek olmayan en yaşlı üyesi. Kimse yoksa hanedan sona erer.
    /// Miras: hane (ev, eşya, akçe, envanter) zaten ortaktır; ayrıca ölenin her becerisindeki XP'nin
    /// %10'u varise "aile bilgisi" olarak geçer (varisin yaş tavanı uygulanır).
    /// </summary>
    public sealed class Dynasty
    {
        public const double SkillInheritance = 0.10;

        public string Name { get; set; } = "Hanedan";
        public int HeadId { get; private set; }
        public List<HeadRecord> History { get; } = new List<HeadRecord>();
        public bool Ended { get; private set; }

        public Character Head(Population population) => population.Find(HeadId);
        public int CurrentGeneration => History.Count > 0 ? History[History.Count - 1].Generation : 1;

        public void Found(Character founder, double day)
        {
            if (founder == null) throw new ArgumentNullException(nameof(founder));
            HeadId = founder.Id;
            founder.IsPlayerControlled = true;
            History.Clear();
            History.Add(new HeadRecord { CharacterId = founder.Id, Name = founder.Name, Generation = founder.Generation, StartDay = day });
            Ended = false;
        }

        /// <summary>Kayıttan yükleme.</summary>
        public void Restore(int headId, IEnumerable<HeadRecord> history, bool ended)
        {
            HeadId = headId;
            History.Clear();
            if (history != null) History.AddRange(history);
            Ended = ended;
        }

        public static Character ChooseHeir(Character dead, IEnumerable<Character> members)
        {
            var alive = new List<Character>();
            foreach (var c in members)
                if (c != null && !c.IsDead && !c.IsVillager && c != dead) alive.Add(c);

            Character Oldest(Func<Character, bool> filter)
            {
                Character best = null;
                foreach (var c in alive)
                    if (filter(c) && (best == null || c.AgeDays > best.AgeDays)) best = c;
                return best;
            }

            bool IsChild(Character c) => c.BirthMotherId == dead.Id || c.BirthFatherId == dead.Id;
            return Oldest(c => IsChild(c) && FamilyService.IsAdultAge(c))
                ?? Oldest(c => IsChild(c) && c.Stage == AgeStage.Child)
                ?? Oldest(c => dead.SpouseId == c.Id)
                ?? Oldest(c => c.Stage != AgeStage.Baby);
        }

        /// <summary>Ölen baş karaktersa varisi belirler, kontrolü devreder, mirası uygular.</summary>
        public SuccessionResult OnDeath(Character dead, IEnumerable<Character> members, double day, string trade = null)
        {
            if (dead == null || dead.Id != HeadId || Ended) return null;
            dead.IsPlayerControlled = false;
            if (History.Count > 0)
            {
                var last = History[History.Count - 1];
                last.EndDay = day;
                last.EndCause = dead.CauseOfDeath;
                last.Trade = trade;
            }

            var heir = ChooseHeir(dead, members);
            if (heir == null)
            {
                Ended = true;
                return new SuccessionResult { DynastyEnded = true, Message = $"{dead.Name} öldü. Hanede kimse kalmadı: hanedan sona erdi." };
            }

            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                double xp = dead.Skills.GetXp(s) * SkillInheritance;
                if (xp > 0) heir.GainSkillXp(s, xp);
            }
            heir.IsPlayerControlled = true;
            HeadId = heir.Id;
            History.Add(new HeadRecord { CharacterId = heir.Id, Name = heir.Name, Generation = heir.Generation, StartDay = day });
            return new SuccessionResult
            {
                Heir = heir,
                Message = $"{dead.Name} öldü. Hanedanın yeni başı {heir.Name} ({heir.Generation}. nesil). Aile bilgisi ona geçti."
            };
        }
    }
}
