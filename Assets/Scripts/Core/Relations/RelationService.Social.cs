using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>
    /// İlişki sosyal işleri (67a §5, 67b §3.2-3.5): tartış, laf sok, kavga, küs, özür, barış, bastonla tehdit; flört ve gönül
    /// yolu; muhabbete katıl, dedikodu, söylenti, yalanla, heyet lobisi. FamilyService bu işleri buraya yönlendirir.
    /// </summary>
    public sealed partial class RelationService
    {
        public const int WitnessRange = 6;
        public const int FightLoss = 30, ArgueLoss = 8, JabLoss = 5;
        public const double ScuffleScale = 0.5;

        /// <summary>Ev envanterinden şarta uyan bir eşyayı alır (alınan kimlik ya da null).</summary>
        public Func<Predicate<string>, string> TakeItem { get; set; }
        public Func<Predicate<string>, bool> HasItem { get; set; }
        /// <summary>Kişiye iş verir (otonom tepki: eşin tartışması, NPC kavgası).</summary>
        public Action<Character, Job> Command { get; set; }
        public GroupChatService Chats { get; set; }
        public ElderCouncil Council { get; set; }

        /// <summary>(yapan, hedef, iş, sonuç metni).</summary>
        public event Action<Character, Character, SocialAction, LocText> Happened;
        /// <summary>Kavga bitti (kazanan, kaybeden, ergen itişmesi mi).</summary>
        public event Action<Character, Character, bool> FightEnded;

        public static bool Handles(SocialAction a) => (int)a >= 90 && (int)a < 130;

        public static bool IsFlirt(SocialAction a) =>
            a == SocialAction.Wink || a == SocialAction.RomanticCompliment || a == SocialAction.HoldHands || a == SocialAction.Hug
            || a == SocialAction.KissCheek || a == SocialAction.Kiss || a == SocialAction.GiveFlowers;

        public static bool IsHandkerchief(string id) => id != null && id.StartsWith("item.handkerchief", StringComparison.Ordinal);
        public static bool IsRing(string id) => id != null && id.StartsWith("item.jw_ring", StringComparison.Ordinal);
        public static bool IsFlower(string id) =>
            id == "item.flower_clump" || id == "item.flower_wreath" || id == "item.cl_flower_crown" || id == "item.elderflower" || id == "item.spring_garland";

        private static bool Adult(Character c) => c.Stage >= AgeStage.YoungAdult;
        private static bool Teen(Character c) => c.Stage == AgeStage.Teen;

        private static string T(string key, params (string, object)[] args) => CoreText.T(key, args);

        public string CanDo(Character actor, Character target, SocialAction action)
        {
            if (actor == null || target == null || actor.IsDead || target.IsDead) return T("rel.why.nobody");
            if (actor.Stage == AgeStage.Baby || target.Stage == AgeStage.Baby) return T("rel.why.baby");
            var p = Find(actor.Id, target.Id);
            int f = Friendship(actor.Id, target.Id);
            switch (action)
            {
                case SocialAction.Argue:
                    if (actor.Stage == AgeStage.Child || target.Stage == AgeStage.Child) return T("rel.why.child");
                    if (f >= 50 && !HasGrievance(actor, target)) return T("rel.why.too_close");
                    if (IsSpouse(actor, target) && !HasGrievance(actor, target)) return T("rel.why.spouse_reason");
                    return null;
                case SocialAction.Jab:
                    if (actor.Stage == AgeStage.Child) return T("rel.why.child");
                    if (f >= 50) return T("rel.why.too_close");
                    return null;
                case SocialAction.Fight:
                    return CanFight(actor, target);
                case SocialAction.CaneThreat:
                    if (actor.Stage != AgeStage.Elder) return T("rel.why.elder_only");
                    if (f > -20 && !HasGrievance(actor, target) && target.Stage != AgeStage.Child && target.Stage != AgeStage.Teen) return T("rel.why.no_quarrel");
                    return null;
                case SocialAction.Sulk:
                    if (p != null && p.Sulking) return T("rel.why.already_sulking");
                    if (f > -20 && !HasGrievance(actor, target)) return T("rel.why.no_quarrel");
                    return null;
                case SocialAction.Apologize:
                    if (actor.Stage == AgeStage.Child && target.Stage == AgeStage.Child) return null;
                    if ((p == null || !p.Sulking) && !HasFeeling(target.Id, actor.Id, FeelingKind.Hurt) && !HasFeeling(target.Id, actor.Id, FeelingKind.Grudge)
                        && !HasFeeling(target.Id, actor.Id, FeelingKind.Betrayed) && f >= 0) return T("rel.why.nothing_to_mend");
                    return null;
                case SocialAction.MakeUp:
                    if (p == null || !p.Sulking) return T("rel.why.not_sulking");
                    return null;
                case SocialAction.Wink:
                case SocialAction.RomanticCompliment:
                case SocialAction.GiveFlowers:
                case SocialAction.HoldHands:
                case SocialAction.Hug:
                case SocialAction.KissCheek:
                case SocialAction.Kiss:
                {
                    if (!CanRomance(actor, target)) return T("rel.why.no_romance");
                    if (p != null && p.Sulking) return T("rel.why.sulking");
                    if (f < -19) return T("rel.why.tense");
                    int r = Romance(actor.Id, target.Id);
                    int need = action == SocialAction.HoldHands || action == SocialAction.Hug ? 15 : action == SocialAction.KissCheek ? 20 : action == SocialAction.Kiss ? 30 : 0;
                    if (IsCouple(actor.Id, target.Id)) need = 0;
                    if (r < need) return T("rel.why.romance_low", ("n", need));
                    if (action == SocialAction.GiveFlowers && (HasItem == null || !HasItem(IsFlower))) return T("rel.why.no_flowers");
                    return null;
                }
                case SocialAction.GiveHandkerchief:
                    if (!CanRomance(actor, target)) return T("rel.why.no_romance");
                    if (actor.SpouseId.HasValue || target.SpouseId.HasValue) return T("rel.why.married");
                    if (IsCouple(actor.Id, target.Id)) return T("rel.why.already_couple");
                    if (PartnerOf(actor) != null || PartnerOf(target) != null) return T("rel.why.has_partner");
                    if (f < SweetheartFriend) return T("rel.why.friend_low", ("n", SweetheartFriend));
                    if (Romance(actor.Id, target.Id) < SweetheartRomance) return T("rel.why.romance_low", ("n", SweetheartRomance));
                    if (HasItem == null || !HasItem(IsHandkerchief)) return T("rel.why.no_handkerchief");
                    return null;
                case SocialAction.ReturnHandkerchief:
                    return p != null && p.Status == CoupleStatus.Sweetheart ? null : T("rel.why.not_sweethearts");
                case SocialAction.AskHand:
                    return CanAsk(actor, target);
                case SocialAction.BreakEngagement:
                    return p != null && p.Status == CoupleStatus.Engaged ? null : T("rel.why.not_engaged");
                case SocialAction.CelebrateAnniversary:
                    if (!IsSpouse(actor, target)) return T("rel.why.not_spouse");
                    return AnniversaryDue(p) ? null : T("rel.why.not_anniversary");
                case SocialAction.JoinChat:
                    if (Chats == null || Chats.CircleOf(target.Id) == null) return T("rel.why.no_circle");
                    if (Chats.CircleOf(actor.Id) != null) return T("rel.why.in_circle");
                    if (Chats.CircleHasSulker(target.Id, actor.Id)) return T("rel.why.sulker_in_circle");
                    return null;
                case SocialAction.Gossip:
                    if (actor.Stage == AgeStage.Child) return T("rel.why.child");
                    if (Knowledge == null || Knowledge.PickToTell(actor, target, allowRumor: false) == null) return T("rel.why.no_news");
                    return null;
                case SocialAction.SpreadRumor:
                    if (Knowledge == null || !Knowledge.HasPreparedRumor(actor.Id)) return T("rel.why.no_rumor");
                    if (!target.IsVillager) return T("rel.why.villager_only");
                    return null;
                case SocialAction.DenyRumor:
                    if (Knowledge == null || Knowledge.RumorAbout(HouseOf(actor), target.Id) == null) return T("rel.why.no_rumor_known");
                    return null;
                case SocialAction.Lobby:
                    return Council == null ? T("rel.why.no_council") : Council.CanLobby(actor, target);
            }
            return T("rel.why.nobody");
        }

        public double Duration(SocialAction action)
        {
            switch (action)
            {
                case SocialAction.Fight: return 0.05;
                case SocialAction.Argue: return 0.05;
                case SocialAction.Jab: case SocialAction.Wink: case SocialAction.KissCheek: return 0.02;
                case SocialAction.CaneThreat: case SocialAction.Sulk: return 0.02;
                case SocialAction.Kiss: case SocialAction.Hug: case SocialAction.HoldHands: return 0.03;
                case SocialAction.GiveHandkerchief: case SocialAction.AskHand: return 0.05;
                case SocialAction.JoinChat: return 0.01;
                default: return 0.04;
            }
        }

        /// <summary>Kişinin eşi ya da sevgilisi yakında mı (görür mü).</summary>
        public bool PartnerNear(Character c)
        {
            var p = PartnerOf(c);
            return p != null && p.Position.Z == c.Position.Z && Math.Abs(p.Position.X - c.Position.X) + Math.Abs(p.Position.Y - c.Position.Y) <= WitnessRange;
        }

        public static bool IsSpouse(Character a, Character b) => a != null && b != null && a.SpouseId == b.Id;

        /// <summary>Haklı bir şikâyet: kıskançlık, ihanet, kin, kırgınlık ya da eşte bekleyen bir tetik.</summary>
        public bool HasGrievance(Character actor, Character target) =>
            HasFeeling(actor.Id, target.Id, FeelingKind.Jealous) || HasFeeling(actor.Id, target.Id, FeelingKind.Betrayed)
            || HasFeeling(actor.Id, target.Id, FeelingKind.Grudge) || HasFeeling(actor.Id, target.Id, FeelingKind.Hurt)
            || PendingIssue(actor.Id, target.Id) != null;

        private string CanFight(Character actor, Character target)
        {
            if (actor.Stage == AgeStage.Elder) return T("rel.why.elder_cane");
            if (target.Stage == AgeStage.Elder || target.Stage == AgeStage.Child || actor.Stage == AgeStage.Child) return T("rel.why.no_fight_age");
            if (Teen(actor) != Teen(target)) return T("rel.why.no_fight_age");
            if (actor.IsPregnant || target.IsPregnant) return T("rel.why.pregnant");
            if (actor.Injury != null || target.Injury != null) return T("rel.why.injured");
            if (IsSpouse(actor, target))
            {
                var issue = PendingIssue(actor.Id, target.Id);
                if (issue == null) return T("rel.why.spouse_fight");
                return null;
            }
            if (Friendship(actor.Id, target.Id) > -20 && !HasGrievance(actor, target)) return T("rel.why.no_quarrel");
            return null;
        }

        /// <summary>İş başladı (JobRunner): kavga ve tartışmada öbür taraf durur ve döner.</summary>
        public void OnSocialStarted(Character actor, Character target, SocialAction action)
        {
            if (action == SocialAction.Fight || action == SocialAction.Argue || action == SocialAction.AskHand || action == SocialAction.GiveHandkerchief
                || action == SocialAction.Kiss || action == SocialAction.Hug || action == SocialAction.HoldHands)
                Hold?.Invoke(target, Duration(action) + 0.005);
        }

        private List<Character> Witnesses(Character a, Character b)
        {
            var list = new List<Character>();
            foreach (var c in _population.Members)
            {
                if (c == a || c == b || c.IsDead || c.Stage == AgeStage.Baby || c.Position.Z != a.Position.Z) continue;
                if (Math.Abs(c.Position.X - a.Position.X) + Math.Abs(c.Position.Y - a.Position.Y) <= WitnessRange) list.Add(c);
            }
            return list;
        }

        public LocText Apply(Character actor, Character target, SocialAction action)
        {
            if (CanDo(actor, target, action) != null) return default;
            Met(actor.Id, target.Id);
            LocText msg;
            switch (action)
            {
                case SocialAction.Argue: msg = DoArgue(actor, target); break;
                case SocialAction.Jab: msg = DoJab(actor, target); break;
                case SocialAction.Fight: msg = DoFight(actor, target); break;
                case SocialAction.CaneThreat: msg = DoCane(actor, target); break;
                case SocialAction.Sulk:
                {
                    var p = Pair(actor.Id, target.Id);
                    p.Sulking = true; p.SulkBy = actor.Id; p.SulkDay = Today();
                    Cue?.Invoke(actor, "sulk", 3f);
                    Bubble?.Invoke(actor, Line("sulk"));
                    Knowledge?.Record(FactKind.Sulk, actor, target, RepFacet.Peace, -1, Witnesses(actor, target));
                    msg = LocText.Of("rel.n.sulk", ("a", actor.Name), ("b", target.Name));
                    break;
                }
                case SocialAction.Apologize: msg = DoApologize(actor, target); break;
                case SocialAction.MakeUp: msg = DoMakeUp(actor, target); break;
                case SocialAction.GiveHandkerchief: msg = DoHandkerchief(actor, target); break;
                case SocialAction.ReturnHandkerchief: msg = BreakUp(actor, target); break;
                case SocialAction.AskHand: msg = ScheduleAsking(actor, target); break;
                case SocialAction.BreakEngagement: msg = BreakEngagement(actor, target); break;
                case SocialAction.CelebrateAnniversary: msg = Celebrate(actor, target); break;
                case SocialAction.JoinChat: msg = Chats.Join(actor, target) ? LocText.Of("rel.n.joined_chat", ("a", actor.Name)) : default; break;
                case SocialAction.Gossip: msg = Knowledge.TellBest(actor, target, allowRumor: false); break;
                case SocialAction.SpreadRumor: msg = Knowledge.SpreadPrepared(actor, target); break;
                case SocialAction.DenyRumor: msg = Knowledge.Deny(actor, target); break;
                case SocialAction.Lobby: msg = Council.Lobby(actor, target); break;
                default: msg = DoFlirt(actor, target, action); break;
            }
            Happened?.Invoke(actor, target, action, msg);
            return msg;
        }

        // ---- olumsuz ----

        private LocText DoArgue(Character actor, Character target)
        {
            int loss = (int)Math.Round(ArgueLoss * (RelationRules.Temper(actor) + RelationRules.Temper(target)) / 2.0);
            AddFriendship(actor.Id, target.Id, -loss);
            foreach (var c in new[] { actor, target }) c.AddMoodlet(RelationMoods.Argued, T("rel.mood.argued"), -3, 0.3);
            Cue?.Invoke(actor, "argue", 3f); Cue?.Invoke(target, "argue", 3f);
            Bubble?.Invoke(actor, Line("argue"));
            Bubble?.Invoke(target, Line("argue_back"));
            if (IsSpouse(actor, target)) { AddWarmth(actor.Id, target.Id, -8); ResolveIssue(actor.Id, target.Id, partially: true); }
            var w = Witnesses(actor, target);
            Knowledge?.Record(FactKind.Argument, actor, target, RepFacet.Peace, -1, w);
            Nudge(actor.Id, target.Id, -0.15);
            // Tırmanma: iki taraf da sinirliyse ve zaten Gergin'se kavga çıkabilir (NPC inisiyatifi).
            if (Friendship(actor.Id, target.Id) <= -20 && !IsSpouse(actor, target) && _rng.NextDouble() < 0.15 * RelationRules.Temper(target)
                && CanFight(target, actor) == null && !target.IsPlayerControlled)
                Command?.Invoke(target, Job.Social(actor.Id, SocialAction.Fight));
            return LocText.Of("rel.n.argue", ("a", actor.Name), ("b", target.Name));
        }

        private LocText DoJab(Character actor, Character target)
        {
            int loss = (int)Math.Round(JabLoss * RelationRules.Temper(target));
            AddFriendship(actor.Id, target.Id, -loss);
            AddFeeling(target.Id, actor.Id, FeelingKind.Hurt);
            actor.Needs.Restore(NeedType.Fun, 6f);
            Cue?.Invoke(actor, "scoff", 2f);
            Bubble?.Invoke(actor, Line("jab", ("b", target.Name)));
            if (Chats != null) Chats.OnJab(actor, target);
            if (IsSpouse(actor, target)) AddWarmth(actor.Id, target.Id, -5);
            // Eşin ailesine laf sokmak (T7).
            var spouse = actor.SpouseId.HasValue ? _population.Find(actor.SpouseId.Value) : null;
            if (spouse != null && FamilyService.AreCloseRelatives(spouse, target)) ReportTrigger(SpouseTrigger.InLawQuarrel, actor, true);
            return LocText.Of("rel.n.jab", ("a", actor.Name), ("b", target.Name));
        }

        private double FightScore(Character c)
        {
            double s = c.Skills.GetLevel(SkillType.Athletics) * 0.6;
            var a = c.Appearance;
            if (a != null) s += a.Build == Customization.BodyBuild.Sturdy ? 2.0 : a.Build == Customization.BodyBuild.Plump ? 1.0 : a.Build == Customization.BodyBuild.Slim ? -0.5 : 0.5;
            if (c.HasTrait("trait.sturdy")) s += 1.5;
            s += (c.MoodStatus == Mood.MoodStatus.Happy || c.MoodStatus == Mood.MoodStatus.Good) ? 0.5 : c.MoodStatus == Mood.MoodStatus.Miserable ? -1.0 : 0;
            if (c.Stage == AgeStage.Adult) s += 0.5;
            return s + _rng.NextDouble() * 4.0;
        }

        private LocText DoFight(Character actor, Character target)
        {
            bool scuffle = Teen(actor);
            double scale = scuffle ? ScuffleScale : 1.0;
            var winner = FightScore(actor) >= FightScore(target) ? actor : target;
            var loser = winner == actor ? target : actor;
            AddFriendship(actor.Id, target.Id, -(int)(FightLoss * scale));
            // Kaybeden: Morarmış (1 gün) + his; yara sistemine bağlanmaz (karar 67c).
            loser.AddMoodlet(RelationMoods.Bruised, T("rel.mood.bruised"), scuffle ? -2 : -4, scuffle ? 0.5 : 1.0);
            AddFeeling(loser.Id, winner.Id, RelationRules.Temper(loser) > 1.0 ? FeelingKind.Grudge : FeelingKind.Ashamed);
            var w = Witnesses(actor, target);
            if (!scuffle) winner.AddMoodlet(RelationMoods.ProudWin, T("rel.mood.proud_win"), w.Count > 0 ? 1 : 3, 0.5);
            Cue?.Invoke(winner, "fight_win", 3f);
            Cue?.Invoke(loser, "fight_lose", 3.5f);
            foreach (var c in w)
            {
                if (c.Stage == AgeStage.Child) { c.AddMoodlet(RelationMoods.ScaredFight, T("rel.mood.scared_fight"), -4, 0.5); Cue?.Invoke(c, "gasp", 2f); continue; }
                if (_rng.NextDouble() < 0.6) Bubble?.Invoke(c, Line(scuffle ? "witness_scuffle" : "witness_fight"));
                Cue?.Invoke(c, "gasp", 2f);
            }
            // Ara bulucu tanık: Gönül kazanma 5+ olan biri araya girer, ayırana itibar.
            Character peacemaker = null;
            foreach (var c in w) if (!c.IsVillager && c.Stage >= AgeStage.YoungAdult && c.Skills.GetLevel(SkillType.Charm) >= 5) { peacemaker = c; break; }
            if (peacemaker != null)
            {
                AddFriendship(actor.Id, target.Id, (int)(FightLoss * scale / 3));
                Bubble?.Invoke(peacemaker, Line("separate"));
                Reputation?.Invoke(RepFacet.Peace, 2, LocText.Of("rel.rep.separated"), peacemaker);
            }
            if (IsSpouse(actor, target)) { AddWarmth(actor.Id, target.Id, -20); ResolveIssue(actor.Id, target.Id, partially: false); }
            if (!scuffle) Knowledge?.Record(FactKind.Fight, actor, target, RepFacet.Peace, -4, w);
            Nudge(actor.Id, target.Id, -0.3);
            FightEnded?.Invoke(winner, loser, scuffle);
            return LocText.Of(scuffle ? "rel.n.scuffle" : "rel.n.fight", ("a", actor.Name), ("b", target.Name), ("winner", winner.Name));
        }

        private LocText DoCane(Character actor, Character target)
        {
            AddFriendship(actor.Id, target.Id, -3);
            target.AddMoodlet(RelationMoods.Shamed, T("rel.mood.shamed"), -2, 0.3);
            Cue?.Invoke(actor, "cane_threat", 3f);
            Bubble?.Invoke(actor, Line("cane"));
            foreach (var c in Witnesses(actor, target)) if (_rng.NextDouble() < 0.5) Cue?.Invoke(c, "laugh_big", 2f);
            return LocText.Of("rel.n.cane", ("a", actor.Name), ("b", target.Name));
        }

        private LocText DoApologize(Character actor, Character target)
        {
            Cue?.Invoke(actor, "apologize", 3f);
            Bubble?.Invoke(actor, Line("apology"));
            double chance = 0.55 + actor.Skills.GetLevel(SkillType.Charm) * 0.04 + (target.HasTrait("trait.placid") || target.HasTrait("trait.generous") ? 0.2 : 0)
                - (HasFeeling(target.Id, actor.Id, FeelingKind.Grudge) ? 0.2 : 0) - (HasFeeling(target.Id, actor.Id, FeelingKind.Betrayed) ? 0.3 : 0);
            if (_rng.NextDouble() < chance)
            {
                RemoveFeeling(target.Id, actor.Id, FeelingKind.Hurt);
                AddFriendship(actor.Id, target.Id, 8);
                EndSulk(actor.Id, target.Id);
                foreach (var c in new[] { actor, target }) c.AddMoodlet(RelationMoods.MadeUp, T("rel.mood.made_up"), 4, 0.5);
                if (IsSpouse(actor, target)) { AddWarmth(actor.Id, target.Id, 10); ResolveIssue(actor.Id, target.Id, partially: true); }
                Bubble?.Invoke(target, Line("accept_apology"));
                Nudge(actor.Id, target.Id, 0.15);
                return LocText.Of("rel.n.apology_ok", ("a", actor.Name), ("b", target.Name));
            }
            Bubble?.Invoke(target, Line("refuse_apology"));
            return LocText.Of("rel.n.apology_no", ("a", actor.Name), ("b", target.Name));
        }

        private LocText DoMakeUp(Character actor, Character target)
        {
            bool festival = IsFestivalDay();
            double chance = festival ? 1.0 : 0.4 + actor.Skills.GetLevel(SkillType.Charm) * 0.04 + (Friendship(actor.Id, target.Id) + 100) / 400.0;
            Cue?.Invoke(actor, "apologize", 2.5f);
            if (_rng.NextDouble() >= chance) { Bubble?.Invoke(target, Line("refuse_apology")); return LocText.Of("rel.n.makeup_no", ("a", actor.Name), ("b", target.Name)); }
            EndSulk(actor.Id, target.Id);
            AddFriendship(actor.Id, target.Id, 5);
            string mood = festival ? RelationMoods.Festival : RelationMoods.MadeUp;
            foreach (var c in new[] { actor, target }) c.AddMoodlet(mood, T(festival ? "rel.mood.festival_makeup" : "rel.mood.made_up"), festival ? 6 : 4, 1.0);
            Bubble?.Invoke(target, Line(festival ? "bayram" : "accept_apology"));
            Knowledge?.Record(FactKind.MadeUp, actor, target, RepFacet.Peace, 1, Witnesses(actor, target));
            return LocText.Of(festival ? "rel.n.makeup_festival" : "rel.n.makeup", ("a", actor.Name), ("b", target.Name));
        }

        // ---- flört ----

        private LocText DoFlirt(Character actor, Character target, SocialAction action)
        {
            int r = Romance(actor.Id, target.Id);
            int need = action == SocialAction.Kiss ? 30 : action == SocialAction.KissCheek ? 20 : action == SocialAction.Hug || action == SocialAction.HoldHands ? 15 : 0;
            bool couple = IsCouple(actor.Id, target.Id);
            double accept = couple ? 0.95
                : 0.55 + (r - need) / 50.0 + (AttractionFactor(target, actor) - 1.0) * 2.0
                  + (target.MoodStatus == Mood.MoodStatus.Miserable ? -0.4 : target.MoodStatus == Mood.MoodStatus.Happy ? 0.1 : 0);
            var witnesses = Witnesses(actor, target);
            if (witnesses.Count >= 3 && !couple && action != SocialAction.Wink) accept -= 0.15;   // kalabalıkta utanır
            if (_rng.NextDouble() >= accept)
            {
                actor.AddMoodlet(RelationMoods.Rejected, T("rel.mood.rejected"), -3, 0.3);
                Bubble?.Invoke(target, Line("shy_no"));
                Cue?.Invoke(target, "giggle", 2f);
                return LocText.Of("rel.n.flirt_no", ("a", actor.Name), ("b", target.Name));
            }
            int gain;
            string pose;
            switch (action)
            {
                case SocialAction.Wink: gain = 3; pose = "social"; break;
                case SocialAction.RomanticCompliment: gain = 5; pose = "social"; break;
                case SocialAction.GiveFlowers: gain = 8; pose = "offer_handkerchief"; TakeItem?.Invoke(IsFlower); break;
                case SocialAction.HoldHands: gain = 6; pose = "hold_hands"; break;
                case SocialAction.Hug: gain = 6; pose = "love"; break;
                case SocialAction.KissCheek: gain = 7; pose = "kiss_cheek"; break;
                default: gain = 10; pose = "kiss"; break;
            }
            AddRomance(actor.Id, target.Id, gain);
            AddFriendship(actor.Id, target.Id, 2);
            foreach (var c in new[] { actor, target })
            {
                c.AddMoodlet(action == SocialAction.Kiss || action == SocialAction.KissCheek ? RelationMoods.Kissed : RelationMoods.Flirted,
                    T(action == SocialAction.Kiss || action == SocialAction.KissCheek ? "rel.mood.kissed" : "rel.mood.flirted"), 4, 0.5);
                Cue?.Invoke(c, pose, 2.5f);
            }
            if (action == SocialAction.Kiss || action == SocialAction.KissCheek) { Oops(actor, target, action); Oops(target, actor, action); }
            if (IsSpouse(actor, target) || IsEngaged(actor.Id, target.Id)) AddWarmth(actor.Id, target.Id, action == SocialAction.Kiss ? 4 : 2);
            Pair(actor.Id, target.Id).LastFlirt = Today();
            Bubble?.Invoke(target, Line("flirt_yes"));
            if (RelationRules.RomanceStageOf(Romance(actor.Id, target.Id)) >= Thatchwick.Core.Relations.RomanceStage.Attracted) LearnPref(actor.Id, target.Id, 0.2);
            // Başkasının sevgilisi, nişanlısı ya da eşi: kıskançlık ve ihanet (T1, T9).
            CheckCheating(actor, target, action, witnesses);
            CheckCheating(target, actor, action, witnesses);
            if (witnesses.Count > 0 && !couple && (action == SocialAction.Kiss || action == SocialAction.HoldHands))
                Knowledge?.Record(FactKind.Flirt, actor, target, RepFacet.Peace, 0, witnesses);
            return LocText.Of("rel.n.flirt." + RelationRules.Snake(action.ToString()), ("a", actor.Name), ("b", target.Name));
        }

        /// <summary>"Oops" (karar 67c): sakallı ya da bıyıklı biriyle öpüşünce tahriş (67b §4.2 tablosu).</summary>
        public void Oops(Character who, Character bearded, SocialAction action)
        {
            if (who == null || bearded == null || bearded.Appearance == null) return;
            var a = bearded.Appearance;
            bool beard = a.BeardId != Customization.Appearance.None;
            bool stubble = !beard && a.StubbleLevel > 0;
            bool mustache = RelationRules.HasMustache(bearded);
            if (!beard && !stubble && !mustache) return;
            var prefs = PrefsOf(who);
            bool dislikes = prefs != null && (prefs.Dislike == AttractionTag.Bearded || prefs.Dislike == AttractionTag.Mustached
                || prefs.Like1 == AttractionTag.CleanShaven || prefs.Like2 == AttractionTag.CleanShaven);
            bool likes = prefs != null && (prefs.Like1 == AttractionTag.Bearded || prefs.Like2 == AttractionTag.Bearded);
            bool longBeard = beard && (a.BeardId == "full_beard" || a.BeardId == "beard");
            double chance; int value; double hours;
            if (stubble) { chance = dislikes ? 1.0 : 0.5; value = dislikes ? -4 : -2; hours = dislikes ? 4 : 2; }
            else if (beard && !longBeard) { chance = dislikes ? 1.0 : 0.25; value = dislikes ? -3 : -1; hours = 3; }
            else if (beard) { chance = dislikes ? 1.0 : 0.10; value = dislikes ? -2 : -1; hours = 3; }
            else { chance = dislikes ? 0.5 : 0.10; value = dislikes ? -2 : -1; hours = 2; }
            if (action == SocialAction.KissCheek) chance *= 0.7;
            if (likes && !stubble)
            {
                who.AddMoodlet("rel.loves_beard", T("rel.mood.loves_beard"), 2, 0.2);
                return;
            }
            if (_rng.NextDouble() >= chance) return;
            who.AddMoodlet(RelationMoods.Oops, T("rel.mood.oops"), value, hours / 24.0);
            Bubble?.Invoke(who, Line("oops"));
            if (dislikes) AddRomance(who.Id, bearded.Id, -1);
        }

        private void CheckCheating(Character actor, Character other, SocialAction action, List<Character> witnesses)
        {
            var partner = PartnerOf(actor);
            if (partner == null || partner == other) return;
            bool heavy = action == SocialAction.Kiss || action == SocialAction.KissCheek || action == SocialAction.HoldHands || action == SocialAction.Hug;
            bool partnerSaw = witnesses.Contains(partner);
            var trigger = heavy ? SpouseTrigger.Infidelity : SpouseTrigger.Flirting;
            if (heavy)
            {
                // Görülmeyen aldatma sır olarak kaydolur; tanıklar ve dedikodu eşe ulaştırabilir (67c §3.5).
                var fact = Knowledge?.Record(FactKind.Infidelity, actor, other, RepFacet.Word, -8, witnesses, secret: !partnerSaw && witnesses.Count == 0);
                if (fact != null) fact.Partner = partner.Id;
            }
            if (partnerSaw) ReportTrigger(trigger, actor, true);
        }

        // ---- yardımcı ----

        /// <summary>Muhabbet ve ilişki repliği: "rel.line.&lt;grup&gt;.&lt;n&gt;" (sayılar <see cref="RelationLines"/>).</summary>
        public LocText Line(string group, params (string name, object value)[] args) => RelationLines.Pick(group, _rng, args);
    }
}
