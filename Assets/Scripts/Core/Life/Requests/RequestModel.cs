using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Work;

namespace Thatchwick.Core.Life.Requests
{
    /// <summary>İsteğin türü (belge 46 §4). "Görev" adı aile işine ayrıldı; NPC'den gelenler "İstek".</summary>
    public enum RequestKind
    {
        /// <summary>Rica: tek köylü, 1-2 gün, ceza yok.</summary>
        Favor = 1,
        /// <summary>Sipariş: dükkân ustası, beceri kapısı, 3-5 gün; kabul edilip bitirilmezse itibar −2.</summary>
        Order = 2,
        /// <summary>Mevsim işi: köyün Mevsim Defteri.</summary>
        Seasonal = 3,
        /// <summary>Çağrı: acil olay, çözüm yolları becerilerle açılır.</summary>
        Call = 4,
        /// <summary>Hikâye: köylünün yakınlık eşiklerinde açılan adımları.</summary>
        Story = 5,
        /// <summary>Adak: kuşaklar boyu süren çok aşamalı hedef.</summary>
        Vow = 6,
        /// <summary>Vasiyet: ölen köylünün son isteği; süresiz.</summary>
        Bequest = 7
    }

    public enum RequestState
    {
        /// <summary>Panoda asılı (ya da köylünün başının üstünde), kabul bekliyor.</summary>
        Posted = 0,
        Accepted = 1,
        Done = 2,
        /// <summary>Kabul edilip süresi doldu (yalnızca siparişte ceza).</summary>
        Failed = 3,
        /// <summary>Kabul edilmeden süresi doldu.</summary>
        Expired = 4,
        /// <summary>Rakip hane üstlendi (belge 46 §8.5).</summary>
        Rival = 5,
        /// <summary>Veren öldü ya da oyuncu bıraktı (cezasız).</summary>
        Dropped = 6
    }

    public enum ObjectiveKind
    {
        /// <summary>Eşya teslim et (ItemId, Count).</summary>
        Deliver = 1,
        /// <summary>Bir işi N kez yap: becerisi (Skill) ya da iş kimliği (ActId) tutan iş.</summary>
        Act = 2,
        /// <summary>Bir bölgeye git (Region).</summary>
        Visit = 3,
        /// <summary>Köylüyle sohbet et (veren ya da VillagerId).</summary>
        Talk = 4,
        /// <summary>Hanede biri bu beceride en az Level olsun.</summary>
        Skill = 5,
        /// <summary>Akçe öde (Count).</summary>
        Pay = 6,
        /// <summary>Bu türde süreli görevde N saat çalış (DutyKind).</summary>
        Duty = 7
    }

    public sealed class Objective
    {
        public ObjectiveKind Kind;
        public string ItemId;
        public int Count = 1;
        public int Progress;
        public SkillType? Skill;
        public int Level;
        public string ActId;
        public string Region;
        public int VillagerId;
        public TimedDutyKind DutyKind;
        /// <summary>Görünen metin ("6 taş getir"); boşsa üretilir.</summary>
        public string Text;

        public bool Done => Progress >= Count;
        public int Left => Math.Max(0, Count - Progress);

        public Objective Clone() => (Objective)MemberwiseClone();
    }

    public sealed class Reward
    {
        public int Akce;
        public int Affinity;
        public int Reputation;
        public double Xp;
        public SkillType? XpSkill;
        public readonly List<KeyValuePair<string, int>> Items = new List<KeyValuePair<string, int>>();
        public int Seals;
        /// <summary>Hatıra eşyası (Keepsakes.Grant) ve etiketi.</summary>
        public string KeepsakeItem, KeepsakeLabel;
        /// <summary>Hane hafızasına (veren hane) eklenecek.</summary>
        public int Memory;
        public string MoodText;
        public int MoodValue;

        public Reward Clone()
        {
            var copy = new Reward
            {
                Akce = Akce, Affinity = Affinity, Reputation = Reputation, Xp = Xp, XpSkill = XpSkill, Seals = Seals, KeepsakeItem = KeepsakeItem,
                KeepsakeLabel = KeepsakeLabel, Memory = Memory, MoodText = MoodText, MoodValue = MoodValue
            };
            copy.Items.AddRange(Items);
            return copy;
        }
    }

    /// <summary>Çağrının bir çözüm yolu (belge 46 §5.2): beceri, eşya, akçe ve süre.</summary>
    public sealed class CallPath
    {
        public string Label;
        public SkillType? Skill;
        public int Level;
        public string ItemId;
        public int ItemCount;
        public int Akce;
        /// <summary>Çözen aile üyesi bu kadar saat köy işinde (süreli görev, Errand).</summary>
        public double Hours;
        public string Outcome;
        /// <summary>Akçe yolu: iş ücretliye gider, itibar yarım.</summary>
        public bool Paid => Akce > 0 && !Skill.HasValue;
    }

    /// <summary>Adağın bir aşaması.</summary>
    public sealed class VowStage
    {
        public string Title;
        public readonly List<Objective> Objectives = new List<Objective>();
        public Reward Reward = new Reward();
    }

    /// <summary>Panodaki ya da defterdeki bir istek.</summary>
    public sealed class Request
    {
        public int Id;
        public RequestKind Kind;
        public string TemplateId;
        public string Title;
        public string Text;
        /// <summary>Veren köylünün karakter kimliği (0: köy).</summary>
        public int GiverId;
        public string GiverName;
        /// <summary>Veren adlı köylüyse tanımı (dükkânı devralan varis siparişi üstlenir).</summary>
        public string GiverDefId;
        public int HouseId;
        public double PostedDay;
        public double AcceptedDay = -1;
        /// <summary>Son gün (mutlak oyun günü); süresizse double.MaxValue.</summary>
        public double Deadline = double.MaxValue;
        /// <summary>Kabul edildikten sonra kaç gün süre verilir.</summary>
        public double Days;
        public RequestState State;
        /// <summary>Panodan okundu (okuryazar hane uzaktan okur; değilse panoya gidilir).</summary>
        public bool Read;
        /// <summary>"Ata" ile verilen aile üyesi (0: yok).</summary>
        public int AssignedTo;
        public readonly List<Objective> Objectives = new List<Objective>();
        public Reward Reward = new Reward();
        /// <summary>Kabul edilip bitirilmezse itibar (siparişte −2).</summary>
        public int Penalty;
        /// <summary>Beceri kapısı ("zorlayıcı" işareti için).</summary>
        public SkillType? Skill;
        public int Level;
        public bool Stretch;
        /// <summary>Usta siparişi (Sv 10).</summary>
        public bool Master;
        // Çağrı
        public readonly List<CallPath> Paths = new List<CallPath>();
        public int ChosenPath = -1;
        public int SolverId;
        /// <summary>Çağrıyla ilgilenilmezse olayın kendi sonucu.</summary>
        public string IgnoreText;
        // Mevsim işi
        public int Season = -1;
        public int DayFrom, DayTo;
        public double VillageProgress;
        // Adak
        public readonly List<VowStage> Stages = new List<VowStage>();
        public int Stage;
        public int VowedBy;
        public string VowedByName;
        public double VowedDay;
        // Hikâye
        public int StoryStep;
        public string StoryKey;

        public bool IsOpen => State == RequestState.Posted || State == RequestState.Accepted;
        public bool IsAccepted => State == RequestState.Accepted;
        public bool HasDeadline => Deadline < double.MaxValue / 2;

        /// <summary>Şu anki hedefler (adakta aşamanın hedefleri).</summary>
        public List<Objective> CurrentObjectives => Kind == RequestKind.Vow && Stage < Stages.Count ? Stages[Stage].Objectives : Objectives;

        public bool AllDone
        {
            get
            {
                foreach (var o in CurrentObjectives) if (!o.Done) return false;
                return CurrentObjectives.Count > 0;
            }
        }

        public static string KindName(RequestKind k)
        {
            switch (k)
            {
                case RequestKind.Favor: return "Rica";
                case RequestKind.Order: return "Sipariş";
                case RequestKind.Seasonal: return "Mevsim işi";
                case RequestKind.Call: return "Çağrı";
                case RequestKind.Story: return "Hikâye";
                case RequestKind.Vow: return "Adak";
                case RequestKind.Bequest: return "Vasiyet";
                default: return "İstek";
            }
        }
    }
}
