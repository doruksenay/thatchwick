using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Relations
{
    public enum CourtEventKind { Asking = 0, Engagement = 1, Henna = 2, Wedding = 3 }

    /// <summary>Gönül yolu sahnesi (67b §3): roller, zaman çizelgesi, sonuç.</summary>
    public sealed class CourtEvent
    {
        public int Id;
        public CourtEventKind Kind;
        /// <summary>Damat (erkek tarafı) ve gelin.</summary>
        public int Groom, Bride;
        public double StartDay;
        public int Step;
        public bool Started, Done;
        /// <summary>İstemede ikram edilen kahve: −1 seçilmedi, 0 şekerli, 1 sade, 2 tuzlu.</summary>
        public int Coffee = -1;
        public readonly List<int> Guests = new List<int>();
        /// <summary>Oyuncunun görmediği (ikisi de köylü) sahne: adımlar sessiz, yalnızca sonuç.</summary>
        public bool Offscreen;
    }

    /// <summary>Oyuncunun vermesi gereken karar (panelde): kahve, ihanet sonrası, heyet uzlaşması.</summary>
    public enum DecisionKind { Coffee = 0, Separation = 1, Mediation = 2 }

    public sealed class PendingDecision
    {
        public int Id;
        public DecisionKind Kind;
        public int A, B;
        public int EventId;
        public double Deadline;
        public int Amount;
    }

    public sealed partial class RelationService
    {
        public const double AskingHour = 0.75, EngagementHour = 0.72, HennaHour = 0.78, WeddingHour = 0.45;
        public const double StepDays = 0.012;

        private readonly List<CourtEvent> _events = new List<CourtEvent>();
        private readonly List<PendingDecision> _decisions = new List<PendingDecision>();
        private int _nextEventId = 1, _nextDecisionId = 1;

        public IReadOnlyList<CourtEvent> Events => _events;
        public IReadOnlyList<PendingDecision> Decisions => _decisions;

        // ---- kancalar ----
        /// <summary>Kişiyi bir evin önüne yürüt (karakter, ev sahibi kişi).</summary>
        public Action<Character, Character> Gather { get; set; }
        /// <summary>Kişinin hanesindeki herkes (oyuncu hanesi ya da komşu hane).</summary>
        public Func<Character, IEnumerable<Character>> HouseMembers { get; set; }
        /// <summary>Oyuncu hanesi akçe öder (başarılıysa true).</summary>
        public Func<int, bool> PayAkce { get; set; }
        public Action<int> AddAkce { get; set; }
        /// <summary>Oyuncu hanesinden biriyle evlilik (FamilyService.Apply Propose).</summary>
        public Action<Character, Character> MarryHousehold { get; set; }
        /// <summary>İki köylüyü evlendirir (NeighborHouseService.Marry).</summary>
        public Action<Character, Character> MarryVillagers { get; set; }
        /// <summary>Hane itibarı 0-1000 (istemede ev sahibinin kararı).</summary>
        public Func<Character, int> HouseReputation { get; set; } = _ => 0;
        /// <summary>Kişi düğün yerinde mi (davetli geldi mi): kişi, ev sahibi.</summary>
        public Func<Character, Character, bool> NearHome { get; set; }
        public Func<bool> CoupleEventsVisible { get; set; } = () => true;

        public static bool IsHousehold(Character c) => c != null && !c.IsVillager;

        // ---- isteme ----

        public string CanAsk(Character actor, Character target)
        {
            var p = Find(actor.Id, target.Id);
            if (p == null || p.Status != CoupleStatus.Sweetheart) return T("rel.why.not_sweethearts");
            if (Today() - p.StatusDay < AskSweetheartDays) return T("rel.why.too_soon", ("n", AskSweetheartDays));
            if (p.Romance < AskRomance) return T("rel.why.romance_low", ("n", AskRomance));
            foreach (var e in _events) if (!e.Done && IsOf(e, actor.Id, target.Id)) return T("rel.why.event_pending");
            return null;
        }

        private static bool IsOf(CourtEvent e, int a, int b) => (e.Groom == a && e.Bride == b) || (e.Groom == b && e.Bride == a);

        private static (Character groom, Character bride) Roles(Character a, Character b) => a.Sex == Sex.Male ? (a, b) : (b, a);

        /// <summary>İsteme günü: ertesi akşam (67b §3.4). Damat tarafı gelin evine gider.</summary>
        public LocText ScheduleAsking(Character actor, Character target)
        {
            var (groom, bride) = Roles(actor, target);
            var e = NewEvent(CourtEventKind.Asking, groom, bride, Math.Floor(Today()) + 1 + AskingHour);
            // Gelin oyuncu hanesindeyse kahveyi oyuncu seçer.
            if (IsHousehold(bride)) AddDecision(DecisionKind.Coffee, bride.Id, groom.Id, e.Id, e.StartDay);
            Notice?.Invoke(LocText.Of("rel.n.asking_scheduled", ("groom", groom.Name), ("bride", bride.Name)), actor.Id);
            return LocText.Of("rel.n.asking_scheduled", ("groom", groom.Name), ("bride", bride.Name));
        }

        private CourtEvent NewEvent(CourtEventKind kind, Character groom, Character bride, double start)
        {
            var e = new CourtEvent { Id = _nextEventId++, Kind = kind, Groom = groom.Id, Bride = bride.Id, StartDay = start, Offscreen = !IsHousehold(groom) && !IsHousehold(bride) };
            _events.Add(e);
            return e;
        }

        private PendingDecision AddDecision(DecisionKind kind, int a, int b, int eventId, double deadline, int amount = 0)
        {
            var d = new PendingDecision { Id = _nextDecisionId++, Kind = kind, A = a, B = b, EventId = eventId, Deadline = deadline, Amount = amount };
            _decisions.Add(d);
            return d;
        }

        /// <summary>Oyuncu bir karar verdi (seçenek: kahvede 0/1/2; ayrılıkta 0 affet, 1 baba evine dön; uzlaşmada 0 kabul, 1 ret).</summary>
        public bool Decide(int decisionId, int option)
        {
            var d = _decisions.Find(x => x.Id == decisionId);
            if (d == null) return false;
            _decisions.Remove(d);
            switch (d.Kind)
            {
                case DecisionKind.Coffee:
                    var e = _events.Find(x => x.Id == d.EventId);
                    if (e != null) e.Coffee = Math.Max(0, Math.Min(2, option));
                    return true;
                case DecisionKind.Separation:
                    ResolveSeparation(_population.Find(d.A), _population.Find(d.B), option == 1);
                    return true;
                case DecisionKind.Mediation:
                    Council?.ResolveMediation(d, option == 0);
                    return true;
            }
            return false;
        }

        internal PendingDecision RequestDecision(DecisionKind kind, int a, int b, double deadline, int amount) => AddDecision(kind, a, b, 0, deadline, amount);

        private Character Elder(Character of, Character exclude)
        {
            Character best = null;
            if (HouseMembers != null)
                foreach (var m in HouseMembers(of))
                    if (m != null && !m.IsDead && m != exclude && m.Stage >= AgeStage.YoungAdult && (best == null || m.AgeDays > best.AgeDays)) best = m;
            // Anne-baba önce gelir.
            foreach (var pid in new[] { of.BirthFatherId, of.BirthMotherId })
                if (pid.HasValue)
                {
                    var par = _population.Find(pid.Value);
                    if (par != null && !par.IsDead && par != exclude) return par;
                }
            return best;
        }

        /// <summary>Sahnelerin zamanı (her adımda): sırası gelen olayı başlatır ve adımlarını oynatır.</summary>
        public void Tick()
        {
            double now = Today();
            foreach (var e in new List<CourtEvent>(_events))
            {
                if (e.Done || now < e.StartDay) continue;
                var groom = _population.Find(e.Groom); var bride = _population.Find(e.Bride);
                if (groom == null || bride == null || groom.IsDead || bride.IsDead) { e.Done = true; continue; }
                if (!e.Started) { StartEvent(e, groom, bride); e.Started = true; }
                while (!e.Done && e.Started && e.Step <= (int)((now - e.StartDay) / StepDays)) { RunStep(e, groom, bride, e.Step); e.Step++; }
            }
            _events.RemoveAll(x => x.Done && Today() - x.StartDay > 1);
            for (int i = _decisions.Count - 1; i >= 0; i--)
            {
                var d = _decisions[i];
                if (now < d.Deadline) continue;
                _decisions.RemoveAt(i);
                // Süre doldu: varsayılan seçim.
                if (d.Kind == DecisionKind.Separation) ResolveSeparation(_population.Find(d.A), _population.Find(d.B), AutoLeave(_population.Find(d.A)));
                else if (d.Kind == DecisionKind.Mediation) Council?.ResolveMediation(d, true);
            }
            Chats?.Tick(now);
        }

        private void StartEvent(CourtEvent e, Character groom, Character bride)
        {
            if (e.Offscreen) return;
            var host = e.Kind == CourtEventKind.Wedding ? groom : bride;
            var people = new List<Character> { groom, bride };
            var ge = Elder(groom, groom); if (ge != null) people.Add(ge);
            var be = Elder(bride, bride); if (be != null) people.Add(be);
            if (e.Kind == CourtEventKind.Henna)
            {
                people.Remove(groom); if (ge != null) people.Remove(ge);
                foreach (var m in _population.Members)
                    if (!m.IsDead && m.Sex == Sex.Female && m != bride && m.Stage >= AgeStage.Teen && Friendship(m.Id, bride.Id) >= 20 && !people.Contains(m)) people.Add(m);
            }
            if (e.Kind == CourtEventKind.Wedding || e.Kind == CourtEventKind.Engagement)
            {
                foreach (var m in _population.Members)
                    if (!m.IsDead && m.Stage >= AgeStage.Child && !people.Contains(m) && (Friendship(m.Id, groom.Id) >= 20 || Friendship(m.Id, bride.Id) >= 20
                        || FamilyService.AreCloseRelatives(m, groom) || FamilyService.AreCloseRelatives(m, bride)) && !m.IsPlayerControlled)
                        people.Add(m);
                if (e.Kind == CourtEventKind.Wedding) Notice?.Invoke(LocText.Of("rel.n.wedding_today", ("groom", groom.Name), ("bride", bride.Name)), groom.Id);
            }
            foreach (var c in people) if (c != null && c != host) Gather?.Invoke(c, host);
            foreach (var c in people) if (c != null) { e.Guests.Add(c.Id); Hold?.Invoke(c, StepDays * 8); }
        }

        private void Say(Character c, string group) { if (c != null) Bubble?.Invoke(c, Line(group)); }
        private void Pose(Character c, string pose, float secs = 3f) { if (c != null) Cue?.Invoke(c, pose, secs); }

        private void RunStep(CourtEvent e, Character groom, Character bride, int step)
        {
            switch (e.Kind)
            {
                case CourtEventKind.Asking: AskingStep(e, groom, bride, step); break;
                case CourtEventKind.Engagement: EngagementStep(e, groom, bride, step); break;
                case CourtEventKind.Henna: HennaStep(e, groom, bride, step); break;
                default: WeddingStep(e, groom, bride, step); break;
            }
        }

        // ---- isteme adımları ----

        private void AskingStep(CourtEvent e, Character groom, Character bride, int step)
        {
            var ge = Elder(groom, groom); var be = Elder(bride, bride) ?? bride;
            if (e.Offscreen && step < 5) return;
            switch (step)
            {
                case 1: Say(ge ?? groom, "asking_greet"); Say(be, "asking_welcome"); Pose(ge ?? groom, "social"); break;
                case 2:
                    if (e.Coffee < 0) e.Coffee = _rng.NextDouble() < 0.7 ? 2 : 0;   // NPC gelin çoğu zaman tuzlu yapar
                    _decisions.RemoveAll(d => d.Kind == DecisionKind.Coffee && d.EventId == e.Id);
                    Pose(bride, "serve_coffee"); Say(bride, "coffee_serve");
                    break;
                case 3:
                    Pose(groom, "drink_grimace");
                    if (e.Coffee == 2)
                    {
                        bool brave = groom.HasTrait("trait.placid") || groom.HasTrait("trait.hardy") || _rng.NextDouble() < 0.5 + Romance(groom.Id, bride.Id) / 300.0;
                        if (brave) { groom.AddMoodlet(RelationMoods.SaltyOk, T("rel.mood.salty_ok"), 3, 0.5); Say(groom, "salty_ok"); AddFriendship(be.Id, groom.Id, 6); }
                        else { groom.AddMoodlet(RelationMoods.SaltyBad, T("rel.mood.salty_bad"), -1, 0.3); Say(groom, "salty_bad"); }
                    }
                    else Say(groom, "coffee_sweet");
                    break;
                case 4: Say(ge ?? groom, "ask_hand"); Pose(ge ?? groom, "tell_story"); break;
                case 5: DecideAsking(e, groom, bride, be); break;
                case 6: e.Done = true; break;
            }
        }

        /// <summary>Ev sahibi büyüğün kararı (67b §3.4): romantik, büyüklerle ilişki, aile dostluğu/küslüğü, itibar, tercih, kahve.</summary>
        public double AskingScore(Character groom, Character bride, Character hostElder, int coffee)
        {
            var ge = Elder(groom, groom);
            double s = 0.25;
            s += Romance(groom.Id, bride.Id) / 200.0;
            if (hostElder != null && hostElder != bride)
            {
                s += Friendship(hostElder.Id, groom.Id) / 250.0;
                if (ge != null) s += Friendship(hostElder.Id, ge.Id) / 300.0;
            }
            int hg = HouseOf(groom), hb = HouseOf(bride);
            if (Knowledge != null && hostElder != null)
            {
                if (Knowledge.KnowsHouseLink(hostElder.Id, hg, hb, FactKind.HouseFeud)) s -= 0.35;
                if (Knowledge.KnowsHouseLink(hostElder.Id, hg, hb, FactKind.HouseBond)) s += 0.15;
            }
            s += Math.Max(-0.2, Math.Min(0.25, HouseReputation(groom) / 1200.0));
            if (hostElder != null) s += (AttractionFactor(hostElder, groom) - 1.0);
            if (coffee == 2 && groom.Mood.Has(RelationMoods.SaltyOk)) s += 0.08;
            return s;
        }

        private void DecideAsking(CourtEvent e, Character groom, Character bride, Character be)
        {
            double s = AskingScore(groom, bride, be, e.Coffee);
            var p = Pair(groom.Id, bride.Id);
            if (s >= 0.6 || (e.Offscreen && s >= 0.35))
            {
                Say(be, "ask_yes");
                foreach (var c in new[] { groom, bride }) c.AddMoodlet(RelationMoods.WordGiven, T("rel.mood.word_given"), 8, 2.0);
                var en = NewEvent(CourtEventKind.Engagement, groom, bride, Math.Floor(Today()) + 2 + _rng.Next(4) + EngagementHour);
                Notice?.Invoke(LocText.Of("rel.n.asking_yes", ("groom", groom.Name), ("bride", bride.Name), ("days", (int)Math.Ceiling(en.StartDay - Today()))), groom.Id);
                Knowledge?.Record(FactKind.WordGiven, groom, bride, RepFacet.Word, 1, null);
                p.StatusDay = p.StatusDay < 0 ? Today() : p.StatusDay;
            }
            else if (s >= 0.4)
            {
                Say(be, "ask_think");
                var hint = HouseReputation(groom) < 100 ? "rel.n.asking_hint_rep" : Romance(groom.Id, bride.Id) < 80 ? "rel.n.asking_hint_love" : "rel.n.asking_hint_elder";
                Notice?.Invoke(LocText.Of("rel.n.asking_think", ("groom", groom.Name), ("bride", bride.Name), ("hint", LocText.Of(hint))), groom.Id);
                p.StatusDay = Today() - AskSweetheartDays + 3;   // 3 gün sonra yeniden istenebilir
            }
            else
            {
                Say(be, "ask_no");
                AddFeeling(groom.Id, be?.Id ?? bride.Id, FeelingKind.Hurt);
                p.StatusDay = Today() + 10 - AskSweetheartDays;   // bir mevsim (10 gün) beklenir
                Notice?.Invoke(LocText.Of("rel.n.asking_no", ("groom", groom.Name), ("bride", bride.Name)), groom.Id);
            }
        }

        // ---- nişan ----

        private void EngagementStep(CourtEvent e, Character groom, Character bride, int step)
        {
            if (e.Offscreen && step < 4) return;
            var be = Elder(bride, bride);
            switch (step)
            {
                case 1:
                    Say(be ?? bride, "engage_welcome");
                    foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && g != groom && g != bride && _rng.NextDouble() < 0.4) Pose(g, "clap", 2f); }
                    break;
                case 2:
                {
                    int quality = 0;
                    if (IsHousehold(groom) && TakeItem != null)
                    {
                        string ring = TakeItem(IsRing);
                        if (ring == null)
                        {
                            Notice?.Invoke(LocText.Of("rel.n.no_ring", ("groom", groom.Name)), groom.Id);
                            e.StartDay += 1; e.Step = -1; e.Started = false;   // ertelenir
                            return;
                        }
                        quality = ring.Contains("gem") ? 2 : ring.Contains("gold") ? 1 : 0;
                    }
                    Pose(groom, "put_ring"); Pose(bride, "put_ring");
                    foreach (var c in new[] { groom, bride }) c.AddMoodlet(RelationMoods.Engaged, T("rel.mood.engaged"), 10 + quality * 3, 3.0);
                    break;
                }
                case 3: Pose(be ?? groom, "cut_ribbon"); Say(be ?? groom, "ribbon"); break;
                case 4:
                {
                    var p = Pair(groom.Id, bride.Id);
                    p.Status = CoupleStatus.Engaged; p.StatusDay = Today();
                    p.WeddingDay = Math.Floor(Today()) + 3 + _rng.Next(5) + WeddingHour;
                    NewEvent(CourtEventKind.Henna, groom, bride, Math.Floor(p.WeddingDay) - 1 + HennaHour);
                    NewEvent(CourtEventKind.Wedding, groom, bride, p.WeddingDay);
                    Knowledge?.Record(FactKind.Engagement, groom, bride, RepFacet.Word, 1, GuestList(e));
                    Notice?.Invoke(LocText.Of("rel.n.engaged", ("groom", groom.Name), ("bride", bride.Name), ("days", (int)Math.Ceiling(p.WeddingDay - Today()))), groom.Id);
                    e.Done = true;
                    break;
                }
            }
        }

        private List<Character> GuestList(CourtEvent e)
        {
            var list = new List<Character>();
            foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && !g.IsDead) list.Add(g); }
            return list;
        }

        // ---- kına gecesi ----

        private void HennaStep(CourtEvent e, Character groom, Character bride, int step)
        {
            if (Status(groom.Id, bride.Id) != CoupleStatus.Engaged) { e.Done = true; return; }
            if (e.Offscreen) { if (step >= 3) e.Done = true; return; }
            var mother = bride.BirthMotherId.HasValue ? _population.Find(bride.BirthMotherId.Value) : null;
            switch (step)
            {
                case 1: Pose(bride, "sit", 4f); Say(mother ?? Elder(bride, bride), "henna_song"); break;
                case 2:
                    Pose(mother ?? Elder(bride, bride), "henna");
                    bride.AddMoodlet(RelationMoods.Henna, T("rel.mood.henna"), 6, 1.0);
                    if (mother != null && !mother.IsDead) mother.AddMoodlet(RelationMoods.GivingAway, T("rel.mood.giving_away"), 2, 1.0);
                    foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && g != bride && _rng.NextDouble() < 0.5) Pose(g, "clap", 2f); }
                    break;
                case 3: Say(bride, "henna_tears"); e.Done = true; break;
            }
        }

        // ---- düğün ----

        private void WeddingStep(CourtEvent e, Character groom, Character bride, int step)
        {
            var p = Pair(groom.Id, bride.Id);
            if (p.Status != CoupleStatus.Engaged) { e.Done = true; return; }
            if (e.Offscreen && step < 6) return;
            switch (step)
            {
                case 1:
                    // Gelin alma alayı: davul zurna.
                    foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && (FamilyService.AreCloseRelatives(g, groom) || g == groom)) Pose(g, "procession", 4f); }
                    Say(groom, "procession");
                    break;
                case 2:
                    // Kapı bahşişi (damat tarafı oyuncuysa öder).
                    if (IsHousehold(groom))
                    {
                        int tip = 2 + _rng.Next(4);
                        bool paid = PayAkce != null && PayAkce(tip);
                        Notice?.Invoke(LocText.Of(paid ? "rel.n.door_tip" : "rel.n.door_tip_none", ("n", tip)), groom.Id);
                    }
                    Say(Elder(bride, bride) ?? bride, "door_tip");
                    break;
                case 3:
                {
                    // Eşikte testi kırma.
                    Pose(bride, "throw_jar");
                    bool broke = _rng.NextDouble() < 0.85;
                    if (broke) foreach (var c in new[] { groom, bride }) c.AddMoodlet(RelationMoods.Blessing, T("rel.mood.blessing"), 4, 2.0);
                    Say(Elder(groom, groom) ?? groom, broke ? "jar_broke" : "jar_whole");
                    break;
                }
                case 4:
                    foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && g.Stage >= AgeStage.Teen && g.Stage != AgeStage.Elder) Pose(g, "halay", 5f); }
                    break;
                case 5:
                {
                    // Takı: misafir sayısı ve itibara göre akçe.
                    int guests = e.Guests.Count;
                    int gift = guests * (2 + Math.Max(0, HouseReputation(groom)) / 200);
                    if ((IsHousehold(groom) || IsHousehold(bride)) && gift > 0)
                    {
                        AddAkce?.Invoke(gift);
                        Notice?.Invoke(LocText.Of("rel.n.wedding_gifts", ("n", gift), ("guests", guests)), groom.Id);
                    }
                    foreach (var id in e.Guests) { var g = _population.Find(id); if (g != null && g != groom && g != bride && _rng.NextDouble() < 0.4) Say(g, "toast"); }
                    break;
                }
                case 6:
                    Marry(e, groom, bride, p);
                    e.Done = true;
                    break;
            }
        }

        private void Marry(CourtEvent e, Character groom, Character bride, PairState p)
        {
            p.Status = CoupleStatus.None;
            p.WeddingDay = -1;
            if (IsHousehold(groom) || IsHousehold(bride))
            {
                p.Status = CoupleStatus.Engaged;   // FamilyService.CanDo(Propose) nişanlı ister
                p.WeddingDay = Today();
                MarryHousehold?.Invoke(IsHousehold(groom) ? groom : bride, IsHousehold(groom) ? bride : groom);
                p.Status = CoupleStatus.None; p.WeddingDay = -1;
            }
            else MarryVillagers?.Invoke(groom, bride);
            if (groom.SpouseId != bride.Id) return;
            p.MarriedDay = Math.Floor(Today());
            p.Warmth = WarmthStart;
            p.AnniversaryYear = 0;
            foreach (var c in new[] { groom, bride }) c.AddMoodlet(RelationMoods.Wedding, T("rel.mood.wedding"), 12, 3.0);
            var guests = GuestList(e);
            Knowledge?.Record(FactKind.Wedding, groom, bride, RepFacet.Generous, 2 + guests.Count / 3, guests);
            // Davetliler: gelen hane üyesi arkadaşlık ve itibar; gelmeyen yakın dosta kırgınlık.
            foreach (var m in _population.Members)
            {
                if (m.IsDead || m == groom || m == bride || m.Stage < AgeStage.Child) continue;
                bool close = Friendship(m.Id, groom.Id) >= 50 || Friendship(m.Id, bride.Id) >= 50;
                bool came = guests.Contains(m) && (NearHome == null || NearHome(m, groom) || NearHome(m, bride));
                if (came && IsHousehold(m) && !IsHousehold(groom) && !IsHousehold(bride))
                {
                    m.AddMoodlet(RelationMoods.Guest, T("rel.mood.wedding_guest"), 4, 1.0);
                    AddFriendship(m.Id, groom.Id, 5); AddFriendship(m.Id, bride.Id, 5);
                    Reputation?.Invoke(RepFacet.Generous, 1, LocText.Of("rel.rep.wedding_guest"), m);
                }
                else if (!came && close && IsHousehold(m) && !e.Offscreen)
                {
                    AddFeeling(groom.Id, m.Id, FeelingKind.Hurt); AddFeeling(bride.Id, m.Id, FeelingKind.Hurt);
                }
            }
        }

        /// <summary>NPC çifti için doğrudan nişan ve düğün takvimi (köy evliliği, 67c §2 kalanı).</summary>
        public void EngageVillagers(Character a, Character b)
        {
            var (groom, bride) = Roles(a, b);
            var p = Pair(groom.Id, bride.Id);
            p.Status = CoupleStatus.Engaged; p.StatusDay = Today();
            if (p.Romance < 60) p.Romance = 60;
            p.WeddingDay = Math.Floor(Today()) + 3 + _rng.Next(4) + WeddingHour;
            NewEvent(CourtEventKind.Henna, groom, bride, Math.Floor(p.WeddingDay) - 1 + HennaHour);
            NewEvent(CourtEventKind.Wedding, groom, bride, p.WeddingDay);
            Knowledge?.Record(FactKind.Engagement, groom, bride, RepFacet.Word, 0, null);
            Notice?.Invoke(LocText.Of("rel.n.village_engaged", ("groom", groom.Name), ("bride", bride.Name), ("days", (int)Math.Ceiling(p.WeddingDay - Today()))), 0);
        }

        // ---- sevgili, ayrılık, nişan bozma ----

        private LocText DoHandkerchief(Character actor, Character target)
        {
            Pose(actor, "offer_handkerchief");
            Say(actor, "handkerchief_offer");
            double accept = 0.55 + (Romance(actor.Id, target.Id) - SweetheartRomance) / 60.0 + (AttractionFactor(target, actor) - 1.0) * 2.0;
            if (target.MoodStatus == Mood.MoodStatus.Miserable) accept -= 0.3;
            if (_rng.NextDouble() >= accept)
            {
                Say(target, "handkerchief_no");
                AddFeeling(actor.Id, target.Id, FeelingKind.Heartbroken, 5);
                return LocText.Of("rel.n.handkerchief_no", ("a", actor.Name), ("b", target.Name));
            }
            TakeItem?.Invoke(IsHandkerchief);
            var p = Pair(actor.Id, target.Id);
            p.Status = CoupleStatus.Sweetheart; p.StatusDay = Today();
            foreach (var c in new[] { actor, target }) c.AddMoodlet(RelationMoods.Sweethearts, T("rel.mood.sweethearts"), 8, 2.0);
            Say(target, "handkerchief_yes");
            Pose(target, "kiss_cheek", 2f);
            Knowledge?.Record(FactKind.Sweethearts, actor, target, RepFacet.Word, 0, Witnesses(actor, target));
            return LocText.Of("rel.n.sweethearts", ("a", actor.Name), ("b", target.Name));
        }

        private LocText BreakUp(Character actor, Character target)
        {
            var p = Pair(actor.Id, target.Id);
            p.Status = CoupleStatus.Ex; p.StatusDay = Today();
            p.Romance = 0;
            AddFeeling(target.Id, actor.Id, FeelingKind.Heartbroken);
            AddFriendship(actor.Id, target.Id, -15);
            Pose(actor, "offer_handkerchief"); Say(target, "breakup");
            Knowledge?.Record(FactKind.BreakUp, actor, target, RepFacet.Word, 0, Witnesses(actor, target));
            return LocText.Of("rel.n.breakup", ("a", actor.Name), ("b", target.Name));
        }

        private LocText BreakEngagement(Character actor, Character target)
        {
            var p = Pair(actor.Id, target.Id);
            p.Status = CoupleStatus.Ex; p.StatusDay = Today(); p.WeddingDay = -1; p.Romance = 0;
            _events.RemoveAll(e => !e.Started && IsOf(e, actor.Id, target.Id));
            AddFeeling(target.Id, actor.Id, FeelingKind.Heartbroken);
            if (HasFeeling(target.Id, actor.Id, FeelingKind.Betrayed)) AddFeeling(target.Id, actor.Id, FeelingKind.Betrayed);
            AddFriendship(actor.Id, target.Id, -30);
            Say(target, "breakup");
            // İki aile küs olur (aile küslüğü, bilgiyle aktarılır).
            Knowledge?.Record(FactKind.BrokeEngagement, actor, target, RepFacet.Word, -5, Witnesses(actor, target));
            Knowledge?.RecordHouseLink(FactKind.HouseFeud, actor, target);
            return LocText.Of("rel.n.engagement_broken", ("a", actor.Name), ("b", target.Name));
        }

        /// <summary>FamilyService.CanDo(Propose): evlilik yalnızca nişanlı çiftin düğün gününde (sahne çağırır).</summary>
        public string ProposalCheck(Character actor, Character target)
        {
            var p = Find(actor.Id, target.Id);
            if (p == null || p.Status != CoupleStatus.Engaged) return T("rel.why.courtship_path");
            if (p.WeddingDay < 0 || Today() < Math.Floor(p.WeddingDay)) return T("rel.why.wedding_day", ("days", Math.Max(1, (int)Math.Ceiling(p.WeddingDay - Today()))));
            return null;
        }

        // ---- günlük ----

        private void CourtshipNewDay(long day, double now)
        {
            // Bekleyen düğün günü geçtiyse ama olay yoksa (eski kayıt) yeniden kur.
            foreach (var p in _pairs.Values)
            {
                if (p.Status != CoupleStatus.Engaged || p.WeddingDay < 0) continue;
                bool has = false;
                foreach (var e in _events) if (!e.Done && e.Kind == CourtEventKind.Wedding && IsOf(e, p.A, p.B)) has = true;
                if (has) continue;
                var a = _population.Find(p.A); var b = _population.Find(p.B);
                if (a == null || b == null || a.IsDead || b.IsDead) continue;
                var (groom, bride) = Roles(a, b);
                if (p.WeddingDay < now) p.WeddingDay = Math.Floor(now) + WeddingHour + 1;
                NewEvent(CourtEventKind.Wedding, groom, bride, p.WeddingDay);
            }
        }
    }
}
