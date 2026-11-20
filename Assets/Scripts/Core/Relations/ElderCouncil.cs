using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>Heyetin oylayacağı bir onarım ya da köy projesi.</summary>
    public sealed class CouncilOption
    {
        public string Id;
        public LocText Name;
    }

    /// <summary>
    /// İhtiyar heyeti (67c §4, karar 67d): itibarı en yüksek hanelerin 3-5 büyüğü. Mevsim başında toplanır; 1.0'da üç işi var:
    /// meydan ve köy projesi onarım sırasını oylamak, küs haneleri uzlaştırmak (özür + küçük tazminat), imece çağırmak.
    /// Oyuncunun hanesi Tanınmış'ta aday, Ünlü'de kesin üye. Üye değilse dilekçe verir (kâtip yazar). Toplantıdan önce
    /// üyelerle sohbet ve hediyeyle lobi yapılabilir.
    /// </summary>
    public sealed class ElderCouncil
    {
        public const int Seats = 5, CandidateRep = 300, MemberRep = 600, PetitionCost = 2;
        public const double MeetingHour = 0.5, ImeceReward = 2;

        private readonly Population _population;
        private readonly Random _rng;
        private readonly List<int> _members = new List<int>();
        /// <summary>NPC üyenin bu toplantıda oyuncunun önerisine eğilimi (lobi).</summary>
        private readonly Dictionary<int, double> _lean = new Dictionary<int, double>();
        private readonly Dictionary<int, long> _lobbiedDay = new Dictionary<int, long>();

        public ElderCouncil(Population population, int seed = 679)
        {
            _population = population;
            _rng = new Random(seed);
        }

        public RelationService Relations { get; set; }
        public Func<double> Today { get; set; } = () => 0;
        /// <summary>Komşu hanelerin başları ve namları (hane kimliği, baş).</summary>
        public Func<IEnumerable<(int house, Character head)>> Heads { get; set; } = () => Array.Empty<(int, Character)>();
        public Func<int> PlayerReputation { get; set; } = () => 0;
        public Func<Character> PlayerHead { get; set; } = () => null;
        /// <summary>Oylanacak onarımlar (64c meydan, 41 köy projeleri) ve kazanan önceliğe alınır.</summary>
        public Func<IEnumerable<CouncilOption>> Options { get; set; } = () => Array.Empty<CouncilOption>();
        public Action<string> Prioritize { get; set; }
        public Func<bool> ScribeAlive { get; set; } = () => true;
        public Func<int, bool> PayAkce { get; set; }
        public Action<LocText, int> Notice { get; set; }
        /// <summary>İmece günü: katılan (meydanda bulunan) hane üyesi.</summary>
        public Func<Character, bool> AtSquare { get; set; } = _ => false;
        public Action<RepFacet, int, LocText, Character> Reputation { get; set; }
        /// <summary>İmece emeği köy projesine katkı.</summary>
        public Action<int> ImeceWork { get; set; }

        public IReadOnlyList<int> Members => _members;
        /// <summary>Oyuncunun önerdiği onarım (toplantıda oya sunulur).</summary>
        public string Proposal { get; set; }
        public bool Petitioned { get; private set; }
        public string LastDecision { get; private set; }
        public double ImeceDay { get; private set; } = -1;
        public double LastMeeting { get; private set; } = -1;

        public bool IsMember(Character c) => c != null && _members.Contains(c.Id);
        public bool PlayerIsMember => IsMember(PlayerHead());

        /// <summary>Üyeleri yeniden seçer: itibarı (bilgi namı) en yüksek komşu hanelerin büyükleri ve oyuncu (eşikteyse).</summary>
        public void Elect()
        {
            _members.Clear();
            var heads = new List<(int house, Character head, int score)>();
            foreach (var (house, head) in Heads())
                if (head != null && !head.IsDead && head.Stage >= AgeStage.Adult)
                    heads.Add((house, head, (Relations?.Knowledge?.HouseScore(house) ?? 0) + (int)(head.AgeDays / 4)));
            heads.Sort((a, b) => b.score.CompareTo(a.score));
            var player = PlayerHead();
            int rep = PlayerReputation();
            bool playerIn = player != null && (rep >= MemberRep || (rep >= CandidateRep && heads.Count >= Seats && rep / 10 > heads[Seats - 1].score));
            if (playerIn) _members.Add(player.Id);
            foreach (var h in heads) { if (_members.Count >= Seats) break; _members.Add(h.head.Id); }
            if (player != null && playerIn) player.AddMoodlet(RelationMoods.Council, CoreText.T("rel.mood.council"), 3, 2.0);
        }

        public string CanPetition()
        {
            if (PlayerIsMember) return CoreText.T("rel.why.member");
            if (!ScribeAlive()) return CoreText.T("rel.why.no_scribe");
            if (Petitioned) return CoreText.T("rel.why.petitioned");
            return null;
        }

        public bool Petition(string optionId)
        {
            if (CanPetition() != null || PayAkce == null || !PayAkce(PetitionCost)) return false;
            Proposal = optionId;
            Petitioned = true;
            return true;
        }

        public string CanLobby(Character actor, Character target)
        {
            if (!IsMember(target) || target.Id == PlayerHead()?.Id) return CoreText.T("rel.why.not_member");
            if (string.IsNullOrEmpty(Proposal)) return CoreText.T("rel.why.no_proposal");
            long day = (long)Math.Floor(Today());
            if (_lobbiedDay.TryGetValue(target.Id, out var d) && d == day) return CoreText.T("rel.why.lobbied");
            return null;
        }

        /// <summary>Lobi: Gönül kazanma ve ilişkiyle üyenin oyuncu önerisine eğilimi artar.</summary>
        public LocText Lobby(Character actor, Character target)
        {
            _lobbiedDay[target.Id] = (long)Math.Floor(Today());
            double gain = 0.15 + actor.Skills.GetLevel(SkillType.Charm) * 0.03 + Math.Max(0, Relations?.Friendship(actor.Id, target.Id) ?? 0) / 400.0;
            _lean.TryGetValue(target.Id, out var l);
            _lean[target.Id] = Math.Min(1.0, l + gain);
            Relations?.AddFriendship(actor.Id, target.Id, 3);
            return LocText.Of("rel.n.lobby", ("a", actor.Name), ("b", target.Name));
        }

        /// <summary>Hediye verildi: üye heyet üyesiyse lobi eğilimi de artar.</summary>
        public void OnGift(Character giver, Character villager, bool liked)
        {
            if (!IsMember(villager) || string.IsNullOrEmpty(Proposal) || giver == null || giver.IsVillager) return;
            _lean.TryGetValue(villager.Id, out var l);
            _lean[villager.Id] = Math.Min(1.0, l + (liked ? 0.25 : 0.05));
        }

        /// <summary>Gün başı: mevsimin ilk günü öğlen toplantı; imece günü sonunda ödül.</summary>
        public void OnNewDay(long day, int dayOfSeason)
        {
            double now = Today();
            if (ImeceDay >= 0 && now >= ImeceDay + 1) EndImece();
            if (dayOfSeason != 0) return;
            Meet(now);
        }

        public void Meet(double now)
        {
            Elect();
            LastMeeting = now;
            var heads = new List<Character>();
            foreach (var id in _members) { var c = _population.Find(id); if (c != null) heads.Add(c); }
            if (heads.Count == 0) return;
            // 1) Onarım sırası.
            var options = new List<CouncilOption>(Options());
            string chosen = null;
            if (options.Count > 0)
            {
                var votes = new Dictionary<string, double>();
                var player = PlayerHead();
                foreach (var m in heads)
                {
                    string vote;
                    if (player != null && m.Id == player.Id) vote = Proposal;
                    else
                    {
                        _lean.TryGetValue(m.Id, out var lean);
                        double toPlayer = (Relations?.Friendship(m.Id, player?.Id ?? 0) ?? 0) / 200.0 + lean;
                        vote = !string.IsNullOrEmpty(Proposal) && _rng.NextDouble() < 0.25 + toPlayer ? Proposal : options[_rng.Next(options.Count)].Id;
                    }
                    if (string.IsNullOrEmpty(vote)) continue;
                    votes.TryGetValue(vote, out var v);
                    votes[vote] = v + 1;
                }
                double best = 0;
                foreach (var kv in votes) if (kv.Value > best) { best = kv.Value; chosen = kv.Key; }
                if (chosen != null) Prioritize?.Invoke(chosen);
                var name = options.Find(o => o.Id == chosen)?.Name ?? LocText.Plain(chosen ?? "");
                Notice?.Invoke(LocText.Of(chosen == Proposal && !string.IsNullOrEmpty(Proposal) ? "rel.n.council_won" : "rel.n.council_chose", ("what", name)), player?.Id ?? 0);
                LastDecision = chosen;
            }
            // 2) Uzlaştırma: en eski küs çift (haneler arası).
            Mediate(now);
            // 3) İmece: ertesi gün.
            ImeceDay = Math.Floor(now) + 1;
            Notice?.Invoke(LocText.Of("rel.n.imece_called"), PlayerHead()?.Id ?? 0);
            _lean.Clear();
            Proposal = null;
            Petitioned = false;
        }

        private void Mediate(double now)
        {
            if (Relations == null) return;
            PairState oldest = null;
            foreach (var p in Relations.Pairs)
                if (p.Sulking && (oldest == null || p.SulkDay < oldest.SulkDay)) oldest = p;
            if (oldest == null) return;
            var a = _population.Find(oldest.A); var b = _population.Find(oldest.B);
            if (a == null || b == null || a.IsDead || b.IsDead) return;
            // Küsen kim, kusurlu öbürü: tazminatı kusurlu öder.
            var offender = oldest.SulkBy == a.Id ? b : a;
            int amount = 3 + _rng.Next(4);
            if (!offender.IsVillager)
            {
                Relations.RequestDecision(DecisionKind.Mediation, offender.Id, (offender == a ? b : a).Id, now + 2, amount);
                Notice?.Invoke(LocText.Of("rel.n.mediation_offer", ("a", a.Name), ("b", b.Name), ("n", amount)), offender.Id);
            }
            else Reconcile(a, b);
        }

        public void ResolveMediation(PendingDecision d, bool accept)
        {
            var a = _population.Find(d.A); var b = _population.Find(d.B);
            if (a == null || b == null) return;
            if (accept && PayAkce != null && PayAkce(d.Amount)) { Reconcile(a, b); Reputation?.Invoke(RepFacet.Peace, 3, LocText.Of("rel.rep.mediation"), a); }
            else { Reputation?.Invoke(RepFacet.Peace, -2, LocText.Of("rel.rep.mediation_refused"), a); Notice?.Invoke(LocText.Of("rel.n.mediation_refused", ("a", a.Name)), a.Id); }
        }

        private void Reconcile(Character a, Character b)
        {
            Relations.EndSulk(a.Id, b.Id);
            Relations.AddFriendship(a.Id, b.Id, 10);
            foreach (var c in new[] { a, b }) c.AddMoodlet(RelationMoods.MadeUp, CoreText.T("rel.mood.made_up"), 4, 1.0);
            Relations.Knowledge?.RecordHouseLink(FactKind.HouseBond, a, b);
            Notice?.Invoke(LocText.Of("rel.n.reconciled", ("a", a.Name), ("b", b.Name)), a.Id);
        }

        // ---- imece ----

        public bool ImeceToday => ImeceDay >= 0 && Today() >= ImeceDay && Today() < ImeceDay + 1;
        private readonly HashSet<int> _imeceCame = new HashSet<int>();

        /// <summary>İmece günü (ruh hali adımında): meydanda bulunan hane üyeleri katılmış sayılır.</summary>
        public void TickImece(IEnumerable<Character> household)
        {
            if (!ImeceToday) return;
            foreach (var c in household)
                if (!c.IsDead && c.Stage >= AgeStage.Teen && AtSquare(c) && _imeceCame.Add(c.Id))
                {
                    Reputation?.Invoke(RepFacet.Generous, (int)ImeceReward, LocText.Of("rel.rep.imece"), c);
                    ImeceWork?.Invoke(1);
                }
        }

        private void EndImece()
        {
            ImeceDay = -1;
            _imeceCame.Clear();
        }

        // ---- kayıt ----

        public CouncilSave Export() => new CouncilSave
        {
            members = _members.ToArray(), proposal = Proposal, petitioned = Petitioned, lastDecision = LastDecision, imeceDay = ImeceDay,
            lastMeeting = LastMeeting, imeceCame = new List<int>(_imeceCame).ToArray()
        };

        public void Import(CouncilSave s)
        {
            _members.Clear(); _imeceCame.Clear(); _lean.Clear();
            if (s == null) return;
            if (s.members != null) _members.AddRange(s.members);
            Proposal = s.proposal; Petitioned = s.petitioned; LastDecision = s.lastDecision; ImeceDay = s.imeceDay; LastMeeting = s.lastMeeting;
            if (s.imeceCame != null) foreach (var x in s.imeceCame) _imeceCame.Add(x);
        }
    }

    [Serializable] public sealed class CouncilSave
    {
        public int[] members;
        public string proposal;
        public bool petitioned;
        public string lastDecision;
        public double imeceDay = -1, lastMeeting = -1;
        public int[] imeceCame;
    }
}
