using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Relations
{
    /// <summary>Muhabbette yapılabilenler (67a §6.2, 67b §5).</summary>
    public enum GroupAct { Chat = 0, Joke = 1, Song = 2, Story = 3, Gossip = 4, Jab = 5, Listen = 6, Leave = 7 }

    /// <summary>Bir muhabbet halkası: 3-6 kişi, merkez, konu.</summary>
    public sealed class ChatCircle
    {
        public int Id;
        public GridPos Center;
        public readonly List<int> Members = new List<int>();
        public string Topic;
        public double Started;
        public double NextTurn;
        public double EndsAt;
        public int LastSpeaker;
        public readonly List<string> RecentLines = new List<string>();
    }

    /// <summary>
    /// Toplu sohbet (muhabbet; 67a §6, 67b §5, 67c §5): şenlik, düğün, taziye, meyhane, meydan akşamı ve sofrada 3-6 kişi
    /// halka kurar. Sıra kimseyi beklemez: oyuncu bir şey yapmıyorsa NPC'ler huylarına göre fıkra, türkü, hikâye, dedikodu,
    /// laf sokma yapar. Dinleyenler huya ve ilişkiye göre farklı tepki pozları oynar (aynı anda aynı poz iki kişiye verilmez).
    /// </summary>
    public sealed class GroupChatService
    {
        public const int MinMembers = 3, MaxMembers = 6;
        public const int Radius = 3;
        public const double TurnDays = 0.004, StepDays = 0.01;
        public const double MinLength = 0.06, MaxLength = 0.14;
        public const int RepeatMemory = 20;

        private readonly Population _population;
        private readonly Random _rng;
        private readonly List<ChatCircle> _circles = new List<ChatCircle>();
        private readonly Dictionary<int, int> _circleOf = new Dictionary<int, int>();
        private readonly Dictionary<int, Queue<string>> _recentByPerson = new Dictionary<int, Queue<string>>();
        private readonly Dictionary<int, GroupAct> _queued = new Dictionary<int, GroupAct>();
        private int _nextId = 1;

        public GroupChatService(Population population, int seed = 677)
        {
            _population = population;
            _rng = new Random(seed);
        }

        public RelationService Relations { get; set; }
        public KnowledgeService Knowledge { get; set; }
        /// <summary>Kişi muhabbete uygun mu (boşta ya da sohbette; oyun katmanı JobRunner'dan bakar).</summary>
        public Func<Character, bool> IsFree { get; set; } = _ => true;
        /// <summary>Muhabbet bağlamı: şenlik, meydan akşamı, meyhane, düğün, sofra sonrası (yer ve zaman).</summary>
        public Func<GridPos, bool> IsGathering { get; set; } = _ => false;
        public Action<Character, LocText> Bubble { get; set; }
        public Action<Character, string, float> Cue { get; set; }
        /// <summary>Kişiyi halkada tut (oyun günü).</summary>
        public Action<Character, double> Hold { get; set; }
        public Action<Character, GridPos> FaceTo { get; set; }
        /// <summary>Bu yerde halka için en az kaç kişi gerekir (sofra başında 2; başka yerde 3).</summary>
        public Func<GridPos, int> MinAt { get; set; }

        public IReadOnlyList<ChatCircle> Circles => _circles;

        public ChatCircle CircleOf(int characterId) =>
            _circleOf.TryGetValue(characterId, out var id) ? _circles.Find(c => c.Id == id) : null;

        public bool CircleHasSulker(int memberOfCircle, int joiner)
        {
            var c = CircleOf(memberOfCircle);
            if (c == null || Relations == null) return false;
            foreach (var m in c.Members) if (Relations.IsSulking(m, joiner)) return true;
            return false;
        }

        // ---- kurulma ----

        public void Tick(double now)
        {
            // Halkalar: üyelik, sıra.
            foreach (var c in new List<ChatCircle>(_circles))
            {
                Prune(c, now);
                if (c.Members.Count < 2 || now >= c.EndsAt) { Disband(c); continue; }
                if (now >= c.NextTurn) { Turn(c, now); c.NextTurn = now + TurnDays * (0.7 + _rng.NextDouble() * 0.8); }
            }
        }

        /// <summary>Yeni halkalar (ruh hali adımında): bağlamdaki boş kişilerden 3+ kişilik öbek.</summary>
        public void Form(double now)
        {
            var free = new List<Character>();
            foreach (var c in _population.Members)
                if (!c.IsDead && c.Stage >= AgeStage.Child && !_circleOf.ContainsKey(c.Id) && IsGathering(c.Position) && IsFree(c)) free.Add(c);
            var used = new HashSet<int>();
            foreach (var seed in free)
            {
                if (used.Contains(seed.Id)) continue;
                var group = new List<Character> { seed };
                foreach (var o in free)
                {
                    if (o == seed || used.Contains(o.Id) || group.Count >= MaxMembers || o.Position.Z != seed.Position.Z) continue;
                    if (Math.Abs(o.Position.X - seed.Position.X) + Math.Abs(o.Position.Y - seed.Position.Y) > Radius) continue;
                    bool sulk = false;
                    foreach (var g in group) if (Relations != null && Relations.IsSulking(g.Id, o.Id)) sulk = true;
                    if (!sulk) group.Add(o);
                }
                if (group.Count < (MinAt?.Invoke(seed.Position) ?? MinMembers)) continue;
                foreach (var g in group) used.Add(g.Id);
                Create(group, now);
            }
        }

        private ChatCircle Create(List<Character> group, double now)
        {
            int x = 0, y = 0;
            foreach (var g in group) { x += g.Position.X; y += g.Position.Y; }
            var c = new ChatCircle
            {
                Id = _nextId++, Center = new GridPos(x / group.Count, y / group.Count, group[0].Position.Z), Started = now,
                NextTurn = now + TurnDays, EndsAt = now + MinLength + _rng.NextDouble() * (MaxLength - MinLength)
            };
            c.Topic = PickTopic(group);
            _circles.Add(c);
            foreach (var g in group) Add(c, g);
            return c;
        }

        private void Add(ChatCircle c, Character g)
        {
            c.Members.Add(g.Id);
            _circleOf[g.Id] = c.Id;
            Hold?.Invoke(g, StepDays * 2);
            FaceTo?.Invoke(g, c.Center);
            if (Relations != null) foreach (var m in c.Members) if (m != g.Id) { var o = _population.Find(m); if (o != null) Relations.Introduce(g, o); }
        }

        /// <summary>Oyuncu (ya da biri) bir halkaya katılır: hedefin halkası.</summary>
        public bool Join(Character joiner, Character memberOfCircle)
        {
            var c = CircleOf(memberOfCircle.Id);
            if (c == null || c.Members.Count >= MaxMembers || _circleOf.ContainsKey(joiner.Id)) return false;
            Add(c, joiner);
            c.EndsAt = Math.Max(c.EndsAt, c.Started + MinLength) + 0.03;
            return true;
        }

        public void Leave(Character who)
        {
            var c = CircleOf(who.Id);
            if (c == null) return;
            c.Members.Remove(who.Id);
            _circleOf.Remove(who.Id);
            _queued.Remove(who.Id);
            Bubble?.Invoke(who, RelationLines.Pick("chat_leave", _rng));
        }

        private void Prune(ChatCircle c, double now)
        {
            for (int i = c.Members.Count - 1; i >= 0; i--)
            {
                var m = _population.Find(c.Members[i]);
                bool gone = m == null || m.IsDead || m.Position.Z != c.Center.Z
                    || Math.Abs(m.Position.X - c.Center.X) + Math.Abs(m.Position.Y - c.Center.Y) > Radius + 2 || !IsFree(m);
                if (!gone) { Hold?.Invoke(m, StepDays * 2); continue; }
                c.Members.RemoveAt(i);
                if (m != null) { _circleOf.Remove(m.Id); _queued.Remove(m.Id); }
            }
        }

        private void Disband(ChatCircle c)
        {
            foreach (var m in c.Members) { _circleOf.Remove(m); _queued.Remove(m); }
            _circles.Remove(c);
        }

        // ---- konu ----

        private static readonly string[] Topics = { "weather", "harvest", "animals", "food", "mine", "craft", "children", "festival", "trade", "hunt", "home", "season" };

        private string PickTopic(List<Character> group)
        {
            // Ortak beceri konusu varsa o (bugünkü "ortak konu"); yoksa rastgele.
            var counts = new Dictionary<string, int>();
            foreach (var g in group)
            {
                if (g.Skills.GetLevel(SkillType.Farming) >= 3) Inc(counts, "harvest");
                if (g.Skills.GetLevel(SkillType.AnimalHusbandry) >= 3) Inc(counts, "animals");
                if (g.Skills.GetLevel(SkillType.Cooking) >= 3) Inc(counts, "food");
                if (g.Skills.GetLevel(SkillType.Mining) >= 3) Inc(counts, "mine");
                if (g.Skills.GetLevel(SkillType.Carpentry) >= 3 || g.Skills.GetLevel(SkillType.Smithing) >= 3) Inc(counts, "craft");
                if (g.Skills.GetLevel(SkillType.Trade) >= 3) Inc(counts, "trade");
                if (g.Skills.GetLevel(SkillType.Hunting) >= 3) Inc(counts, "hunt");
                if (g.Stage == AgeStage.Elder || g.Skills.GetLevel(SkillType.Parenting) >= 3) Inc(counts, "children");
            }
            string best = null; int n = 1;
            foreach (var kv in counts) if (kv.Value > n) { best = kv.Key; n = kv.Value; }
            return best ?? Topics[_rng.Next(Topics.Length)];
        }

        private static void Inc(Dictionary<string, int> d, string k) { d.TryGetValue(k, out var v); d[k] = v + 1; }

        // ---- oyuncunun seçimi ----

        /// <summary>Oyuncu, halkadaki karakterine bir sonraki sözünü verir (sırası gelince yapar).</summary>
        public bool Queue(Character who, GroupAct act)
        {
            if (CircleOf(who.Id) == null) return false;
            if (act == GroupAct.Leave) { Leave(who); return true; }
            _queued[who.Id] = act;
            return true;
        }

        public string CanAct(Character who, GroupAct act)
        {
            var c = CircleOf(who.Id);
            if (c == null) return CoreText.T("rel.why.no_circle");
            switch (act)
            {
                case GroupAct.Song: return who.Skills.GetLevel(SkillType.Music) >= 3 ? null : CoreText.T("rel.why.music", ("n", 3));
                case GroupAct.Gossip: return Knowledge != null && GossipTarget(c, who) != null ? null : CoreText.T("rel.why.no_news");
                case GroupAct.Jab: return JabTarget(c, who, any: true) != null ? null : CoreText.T("rel.why.no_target");
                case GroupAct.Story: return who.Stage >= AgeStage.Teen ? null : CoreText.T("rel.why.child");
                default: return null;
            }
        }

        // ---- sıra ----

        private void Turn(ChatCircle c, double now)
        {
            var members = new List<Character>();
            foreach (var id in c.Members) { var m = _population.Find(id); if (m != null) members.Add(m); }
            if (members.Count < 2) return;
            Character speaker = null; GroupAct act;
            foreach (var m in members) if (_queued.ContainsKey(m.Id)) { speaker = m; break; }
            if (speaker != null) { act = _queued[speaker.Id]; _queued.Remove(speaker.Id); if (CanAct(speaker, act) != null) act = GroupAct.Chat; }
            else
            {
                speaker = PickSpeaker(members, c.LastSpeaker);
                // Oyuncunun seçili karakteri kendiliğinden yalnızca sohbet eder (oyuncu emri her zaman önce gelir).
                act = speaker.IsPlayerControlled ? GroupAct.Chat : PickAct(c, speaker);
            }
            c.LastSpeaker = speaker.Id;
            Perform(c, speaker, act, members);
        }

        private Character PickSpeaker(List<Character> members, int last)
        {
            double total = 0;
            foreach (var m in members) if (m.Id != last) total += RelationRules.Talkative(m);
            double r = _rng.NextDouble() * total;
            foreach (var m in members)
            {
                if (m.Id == last) continue;
                r -= RelationRules.Talkative(m);
                if (r <= 0) return m;
            }
            return members[0];
        }

        private GroupAct PickAct(ChatCircle c, Character s)
        {
            var w = new Dictionary<GroupAct, double> { [GroupAct.Chat] = 4, [GroupAct.Listen] = s.HasTrait("trait.introvert") ? 3 : 0.5 };
            if (s.HasTrait("trait.cheerful") || s.Skills.GetLevel(SkillType.Charm) >= 3) w[GroupAct.Joke] = 2;
            else w[GroupAct.Joke] = 0.6;
            if (s.Skills.GetLevel(SkillType.Music) >= 3) w[GroupAct.Song] = IsFestivalish(c) ? 2.5 : 0.8;
            if (s.Stage == AgeStage.Elder || s.Skills.GetLevel(SkillType.Literacy) >= 3) w[GroupAct.Story] = 1.5;
            if (Knowledge != null && GossipTarget(c, s) != null) w[GroupAct.Gossip] = 2.0 * RelationRules.Gossipy(s);
            if (JabTarget(c, s, any: false) != null) w[GroupAct.Jab] = 1.5 * RelationRules.Temper(s);
            if (s.MoodStatus == Mood.MoodStatus.Miserable) w[GroupAct.Listen] += 2;
            double total = 0; foreach (var v in w.Values) total += v;
            double r = _rng.NextDouble() * total;
            foreach (var kv in w) { r -= kv.Value; if (r <= 0) return kv.Key; }
            return GroupAct.Chat;
        }

        private bool IsFestivalish(ChatCircle c) => Relations != null && Relations.IsFestivalDay();

        private Character GossipTarget(ChatCircle c, Character teller)
        {
            if (Knowledge == null) return null;
            foreach (var id in c.Members)
            {
                if (id == teller.Id) continue;
                var m = _population.Find(id);
                if (m != null && Knowledge.PickToTell(teller, m, allowRumor: true) != null) return m;
            }
            return null;
        }

        private Character JabTarget(ChatCircle c, Character s, bool any)
        {
            if (Relations == null) return null;
            Character best = null; int worst = any ? 50 : -19;
            foreach (var id in c.Members)
            {
                if (id == s.Id) continue;
                int f = Relations.Friendship(s.Id, id);
                if (f < worst) { worst = f; best = _population.Find(id); }
            }
            return best;
        }

        private LocText Line(Character speaker, string group, params (string, object)[] args)
        {
            var q = Recent(speaker.Id);
            LocText t = RelationLines.Pick(group, _rng, args);
            for (int i = 0; i < 4 && t.Key != null && q.Contains(t.Key); i++) t = RelationLines.Pick(group, _rng, args);
            if (t.Key != null) { q.Enqueue(t.Key); while (q.Count > RepeatMemory) q.Dequeue(); }
            return t;
        }

        private Queue<string> Recent(int id)
        {
            if (!_recentByPerson.TryGetValue(id, out var q)) { q = new Queue<string>(); _recentByPerson[id] = q; }
            return q;
        }

        private void Perform(ChatCircle c, Character s, GroupAct act, List<Character> members)
        {
            int bonus = 3;
            string speakerPose = PoseSet(s, "talk");
            switch (act)
            {
                case GroupAct.Joke:
                    Bubble?.Invoke(s, Line(s, "joke"));
                    speakerPose = PoseSet(s, "story"); bonus = 5;
                    React(members, s, "laugh");
                    foreach (var m in members) if (m != s) m.Needs.Restore(NeedType.Fun, 8f);
                    break;
                case GroupAct.Song:
                    Bubble?.Invoke(s, Line(s, "song"));
                    speakerPose = "social"; bonus = 5;
                    React(members, s, "clap");
                    s.GainSkillXp(SkillType.Music, 3);
                    break;
                case GroupAct.Story:
                    Bubble?.Invoke(s, Line(s, "story"));
                    speakerPose = PoseSet(s, "story"); bonus = 4;
                    React(members, s, "listen");
                    if (Knowledge != null && s.Stage >= AgeStage.YoungAdult)
                        foreach (var m in members) if (m != s && m.Stage <= AgeStage.Teen) Knowledge.TellFamilyLore(s, m);
                    break;
                case GroupAct.Gossip:
                {
                    var t = GossipTarget(c, s);
                    if (t != null)
                    {
                        var f = Knowledge.PickToTell(s, t, allowRumor: true);
                        Bubble?.Invoke(s, Knowledge.LineFor(f));
                        foreach (var m in members) if (m != s) Knowledge.Learn(m, f, s);
                        speakerPose = "gossip";
                        React(members, s, "gasp");
                    }
                    break;
                }
                case GroupAct.Jab:
                {
                    var t = JabTarget(c, s, any: true);
                    if (t != null)
                    {
                        Bubble?.Invoke(s, Line(s, "jab", ("b", t.Name)));
                        speakerPose = "scoff";
                        Relations?.AddFriendship(s.Id, t.Id, -5);
                        Relations?.AddFeeling(t.Id, s.Id, FeelingKind.Hurt);
                        Cue?.Invoke(t, PoseSet(t, "cross"), 2.5f);
                        foreach (var m in members) if (m != s && m != t && _rng.NextDouble() < 0.4) Cue?.Invoke(m, PoseSet(m, "laugh"), 2f);
                        if (Relations != null && Relations.Friendship(s.Id, t.Id) <= -60 && _rng.NextDouble() < 0.25 && Relations.Command != null && !t.IsPlayerControlled)
                            Relations.Command(t, Jobs.Job.Social(s.Id, Jobs.SocialAction.Argue));
                        return;
                    }
                    break;
                }
                case GroupAct.Listen:
                    speakerPose = PoseSet(s, "listen");
                    bonus = 2;
                    break;
                default:
                    Bubble?.Invoke(s, Line(s, "topic." + c.Topic));
                    React(members, s, "nod");
                    break;
            }
            Cue?.Invoke(s, speakerPose, 2.5f);
            // Halkadaki her çifte küçük arkadaşlık (iki kişilik sohbetin yarısı kadar), sosyal ihtiyacı dolar.
            if (Relations == null) return;
            foreach (var m in members)
            {
                m.Needs.Restore(NeedType.Social, 6f * m.Modifiers.SocialGain);
                if (m == s) continue;
                int gain = (int)Math.Round(bonus * Relations.GainFactor(s, m));
                if (gain > 0 && !Relations.IsSulking(s.Id, m.Id)) Relations.AddFriendship(s.Id, m.Id, gain);
                Relations.Met(s.Id, m.Id);
                if (act != GroupAct.Listen && _rng.NextDouble() < 0.1) Relations.AddFeeling(m.Id, s.Id, FeelingKind.Close);
                Relations.LearnPref(m.Id, s.Id, 0.05);
            }
        }

        /// <summary>Dinleyenlere farklı tepki pozları: aynı anda aynı poz iki kişiye verilmez.</summary>
        private void React(List<Character> members, Character speaker, string mood)
        {
            var used = new HashSet<string>();
            foreach (var m in members)
            {
                if (m == speaker) continue;
                string pose = ReactionFor(m, speaker, mood, used);
                used.Add(pose);
                Cue?.Invoke(m, pose, 2.2f);
                if (_rng.NextDouble() < 0.25) Bubble?.Invoke(m, RelationLines.Pick("react." + mood, _rng));
            }
        }

        private string ReactionFor(Character m, Character speaker, string mood, HashSet<string> used)
        {
            int f = Relations?.Friendship(m.Id, speaker.Id) ?? 0;
            var options = new List<string>();
            if (m.Stage == AgeStage.Child)
            {
                options.AddRange(mood == "laugh" ? new[] { "laugh", "giggle", "clap" } : mood == "gasp" ? new[] { "gasp", "giggle" } : new[] { "listen_nod", "clap", "laugh" });
            }
            else if (m.Stage == AgeStage.Elder)
            {
                options.AddRange(mood == "laugh" || mood == "clap" ? new[] { "laugh", "listen_nod" } : new[] { "listen_nod", "laugh" });
            }
            else if (f <= -20) options.AddRange(new[] { "listen_arms", "scoff", "shrug" });
            else
            {
                switch (mood)
                {
                    case "laugh":
                        if (m.HasTrait("trait.cheerful") || m.HasTrait("trait.sociable")) options.Add("laugh_big");
                        if (m.HasTrait("trait.introvert")) options.Add("giggle");
                        options.AddRange(new[] { "laugh_smile", "giggle", "laugh_big", "clap" });
                        break;
                    case "gasp": options.AddRange(new[] { "gasp", "think", "giggle", "shrug" }); break;
                    case "clap": options.AddRange(new[] { "clap", "laugh_smile", "listen_nod" }); break;
                    case "listen": options.AddRange(new[] { "listen_nod", "think", "listen_arms" }); break;
                    default:
                        if (m.HasTrait("trait.gloomy")) options.Add("shrug");
                        options.AddRange(new[] { "listen_nod", "think", "laugh_smile", "shrug" });
                        break;
                }
            }
            foreach (var o in options) if (!used.Contains(o)) return o;
            return options[_rng.Next(options.Count)];
        }

        /// <summary>Konuşanın pozu yaşa göre (yaşlı ve çocuk gövdesinin kendi pozları var: 738, 749).</summary>
        private static string PoseSet(Character c, string kind)
        {
            bool elder = c.Stage == AgeStage.Elder, child = c.Stage == AgeStage.Child;
            switch (kind)
            {
                case "story": return elder || child ? "tell_story" : "tell_story";
                case "listen": return "listen_nod";
                case "laugh": return elder || child ? "laugh" : "laugh_smile";
                case "cross": return elder ? "sulk" : child ? "gasp" : "listen_arms";
                default: return "social";
            }
        }

        /// <summary>Halkadaki birine laf sokuldu (iki kişilik "Laf sok" halkada da duyulur).</summary>
        public void OnJab(Character actor, Character target)
        {
            var c = CircleOf(actor.Id);
            if (c == null) return;
            foreach (var id in c.Members)
            {
                if (id == actor.Id || id == target.Id) continue;
                var m = _population.Find(id);
                if (m != null && _rng.NextDouble() < 0.5) Cue?.Invoke(m, "gasp", 2f);
            }
        }
    }
}
