using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>Bilgi türleri (67c §3.2).</summary>
    public enum FactKind
    {
        Fight = 0, Argument = 1, Sulk = 2, MadeUp = 3, Flirt = 4, Sweethearts = 5, WordGiven = 6, Engagement = 7, BrokeEngagement = 8,
        BreakUp = 9, Wedding = 10, Infidelity = 11, Separation = 12, Donation = 13, ContestWin = 14, LostChild = 15, Filth = 16,
        AnimalTalk = 17, Rumor = 18, Liar = 19, HouseFeud = 20, HouseBond = 21, Masterwork = 22, Kindness = 23, Secret = 24
    }

    /// <summary>Bir bilgi: konu, yön, değer, doğruluk; kimin bildiği ayrı tutulur.</summary>
    public sealed class Fact
    {
        public int Id;
        public FactKind Kind;
        /// <summary>Konu kişi (yapan) ve öbür kişi; hane bağlarında hanelerin kimliği (oyuncu 0).</summary>
        public int Subject, Other;
        public int SubjectHouse = -1, OtherHouse = -1;
        public RepFacet Facet;
        /// <summary>İtibar etkisi (konu haneye; köyün yarısı bilince tam).</summary>
        public int Value;
        public bool Truth = true;
        public bool Secret;
        /// <summary>Söylentiyi çıkaran (yalan ortaya çıkınca).</summary>
        public int Source;
        /// <summary>Aldatmada aldatılan eş (dedikodu eşe ulaşınca tetik).</summary>
        public int Partner;
        public double Day;
        /// <summary>Şimdiye kadar uygulanan itibar.</summary>
        public int Applied;
        /// <summary>Köy hafızası: unutulmaz, kronikte kalır.</summary>
        public bool Memory;
        public bool Debunked;
    }

    /// <summary>
    /// Bilgi sistemi (67c §3, karar 67d): karakterler bir şeyi görerek, duyarak ya da okuyarak bilir. Dedikodu bilenden
    /// bilmeyene geçer, itibar köyün bildiklerinin toplamıdır (köyün yarısı bilince tam etki), söylenti doğruyu bilenle
    /// karşılaşınca çürür, küçük haberler unutulur, aile küslüğü ve dostluğu anlatılarak geçer.
    /// </summary>
    public sealed class KnowledgeService
    {
        public const double ForgetDays = 10;   // bir mevsim
        public const double FullEffectShare = 0.5;
        public const int RivalRumorsPerYear = 3;
        public const int RumorGapDays = 10;

        private readonly Population _population;
        private readonly Random _rng;
        private readonly List<Fact> _facts = new List<Fact>();
        private readonly Dictionary<int, HashSet<int>> _knows = new Dictionary<int, HashSet<int>>();
        private readonly Dictionary<int, (int house, FactKind kind)> _prepared = new Dictionary<int, (int, FactKind)>();
        private readonly Dictionary<int, double> _lastRivalRumor = new Dictionary<int, double>();
        private readonly Dictionary<int, int> _houseScore = new Dictionary<int, int>();
        private int _nextId = 1;

        public KnowledgeService(Population population, int seed = 673)
        {
            _population = population;
            _rng = new Random(seed);
        }

        public RelationService Relations { get; set; }
        public Func<double> Today { get; set; } = () => 0;
        /// <summary>Kişinin hanesi: oyuncu 0, komşu hane kimliği, yoksa −1.</summary>
        public Func<Character, int> HouseOf { get; set; } = c => c == null ? -1 : c.IsVillager ? -1 : 0;
        /// <summary>Oyuncu hanesine itibar (yön, miktar, neden, kişi).</summary>
        public Action<RepFacet, int, LocText, Character> Reputation { get; set; }
        public Action<LocText, int> Notice { get; set; }
        public Action<Character, LocText> Bubble { get; set; }
        /// <summary>Yaşlı dalgınlığı: bilgi unutturur (51).</summary>
        public Func<Character, bool> AbsentMinded { get; set; } = _ => false;
        /// <summary>Komşu hanenin adı (rakip söylentisi için) ve rakip haneler.</summary>
        public Func<int, string> HouseName { get; set; } = h => "";
        public Func<IEnumerable<int>> RivalHouses { get; set; } = () => Array.Empty<int>();
        public Func<int> DaysPerYear { get; set; } = () => 40;
        public Func<int, IEnumerable<Character>> HouseMembersById { get; set; }

        public IReadOnlyList<Fact> Facts => _facts;
        public Fact Get(int id) => _facts.Find(f => f.Id == id);

        private HashSet<int> Set(int c)
        {
            if (!_knows.TryGetValue(c, out var s)) { s = new HashSet<int>(); _knows[c] = s; }
            return s;
        }

        public bool Knows(int character, Fact f) => f != null && _knows.TryGetValue(character, out var s) && s.Contains(f.Id);

        public int KnowerCount(Fact f, bool villagersOnly = true)
        {
            int n = 0;
            foreach (var kv in _knows)
            {
                if (!kv.Value.Contains(f.Id)) continue;
                var c = _population.Find(kv.Key);
                if (c == null || c.IsDead || (villagersOnly && !c.IsVillager)) continue;
                n++;
            }
            return n;
        }

        private int VillagerCount()
        {
            int n = 0;
            foreach (var c in _population.Members) if (!c.IsDead && c.IsVillager && c.Stage >= AgeStage.Child) n++;
            return Math.Max(1, n);
        }

        // ---- kayıt ----

        /// <summary>Bir olay olur: konu, öbür kişi, tanıklar (gördü, kesin bilir), sır mı.</summary>
        public Fact Record(FactKind kind, Character subject, Character other, RepFacet facet, int value, IEnumerable<Character> witnesses, bool secret = false)
        {
            if (subject == null) return null;
            var f = new Fact
            {
                Id = _nextId++, Kind = kind, Subject = subject.Id, Other = other?.Id ?? 0, SubjectHouse = HouseOf(subject),
                OtherHouse = other != null ? HouseOf(other) : -1, Facet = facet, Value = value, Secret = secret, Day = Today(),
                Memory = Math.Abs(value) >= 6 || kind == FactKind.Wedding && value >= 6
            };
            _facts.Add(f);
            Set(subject.Id).Add(f.Id);
            if (other != null) Set(other.Id).Add(f.Id);
            if (witnesses != null) foreach (var w in witnesses) if (w != null && !w.IsDead) Learn(w, f, null);
            ApplyReputation(f);
            return f;
        }

        /// <summary>İki hane arasında küslük ya da dostluk bağı (bilgi): iki tarafın hanesi de bilir.</summary>
        public Fact RecordHouseLink(FactKind kind, Character a, Character b)
        {
            int ha = HouseOf(a), hb = HouseOf(b);
            if (ha < 0 || hb < 0 || ha == hb) return null;
            foreach (var f in _facts)
                if (f.Kind == kind && !f.Debunked && ((f.SubjectHouse == ha && f.OtherHouse == hb) || (f.SubjectHouse == hb && f.OtherHouse == ha))) { Spread(f, a, b); return f; }
            // Karşıtı varsa (dostluk ↔ küslük) söner.
            var opposite = kind == FactKind.HouseFeud ? FactKind.HouseBond : FactKind.HouseFeud;
            _facts.RemoveAll(f => f.Kind == opposite && ((f.SubjectHouse == ha && f.OtherHouse == hb) || (f.SubjectHouse == hb && f.OtherHouse == ha)));
            var fact = new Fact { Id = _nextId++, Kind = kind, Subject = a.Id, Other = b.Id, SubjectHouse = ha, OtherHouse = hb, Day = Today(), Memory = true };
            _facts.Add(fact);
            Spread(fact, a, b);
            return fact;
        }

        private void Spread(Fact f, Character a, Character b)
        {
            foreach (var c in new[] { a, b })
                foreach (var m in _population.Members)
                    if (!m.IsDead && m.Stage >= AgeStage.Teen && HouseOf(m) == HouseOf(c)) Set(m.Id).Add(f.Id);
        }

        /// <summary>İki hane arasında küslük/dostluk bilgisini biliyor mu (bilen kişi).</summary>
        public bool KnowsHouseLink(int character, int ha, int hb, FactKind kind)
        {
            if (ha < 0 || hb < 0 || !_knows.TryGetValue(character, out var s)) return false;
            foreach (var f in _facts)
                if (f.Kind == kind && !f.Debunked && s.Contains(f.Id) && ((f.SubjectHouse == ha && f.OtherHouse == hb) || (f.SubjectHouse == hb && f.OtherHouse == ha))) return true;
            return false;
        }

        // ---- öğrenme ----

        /// <summary>Bilgiyi öğrenir; konuya bakışı değişir; aldatmayı duyan eş tetiklenir; söylenti doğruyu bilene çürür.</summary>
        public bool Learn(Character listener, Fact f, Character teller)
        {
            if (listener == null || f == null || f.Debunked) return false;
            var set = Set(listener.Id);
            if (set.Contains(f.Id)) return false;
            // Düşmanından duyduğuna inanmaz.
            if (teller != null && Relations != null && Relations.Friendship(listener.Id, teller.Id) <= -60 && !f.Truth) return false;
            if (!f.Truth && teller != null && KnowsTruth(listener, f))
            {
                Debunk(f, listener, teller);
                return false;
            }
            set.Add(f.Id);
            if (Relations != null && listener.Id != f.Subject)
            {
                int change = f.Kind == FactKind.Infidelity ? -8 : f.Kind == FactKind.Fight ? -2 : f.Kind == FactKind.Liar ? -5
                    : f.Kind == FactKind.Donation || f.Kind == FactKind.Kindness ? 3 : f.Kind == FactKind.Rumor ? -3 : 0;
                if (change != 0 && Relations.Friendship(listener.Id, f.Subject) < 80) Relations.AddFriendship(listener.Id, f.Subject, change);
                if (f.Kind == FactKind.Infidelity && f.Partner == listener.Id)
                {
                    var cheater = _population.Find(f.Subject);
                    if (cheater != null) Relations.ReportTrigger(SpouseTrigger.Infidelity, cheater, true);
                }
            }
            ApplyReputation(f);
            return true;
        }

        /// <summary>Söylentinin doğrusunu bilir mi: konu kişi, hanesi ya da Kanka'sı.</summary>
        private bool KnowsTruth(Character c, Fact f)
        {
            if (c.Id == f.Subject) return true;
            if (f.SubjectHouse >= 0 && HouseOf(c) == f.SubjectHouse) return true;
            return Relations != null && Relations.Friendship(c.Id, f.Subject) >= 80;
        }

        private void Debunk(Fact rumor, Character by, Character teller)
        {
            rumor.Debunked = true;
            // Uygulanan itibar geri alınır; yalancı haberi doğar.
            if (rumor.Applied != 0 && rumor.SubjectHouse == 0) Reputation?.Invoke(rumor.Facet, -rumor.Applied, LocText.Of("rel.rep.rumor_cleared"), null);
            AddHouseScore(rumor.SubjectHouse, -rumor.Applied);
            rumor.Applied = 0;
            var source = _population.Find(rumor.Source);
            if (source != null)
            {
                var liar = Record(FactKind.Liar, source, by, RepFacet.Word, -4, new[] { by });
                if (teller != null) Learn(teller, liar, by);
            }
            Bubble?.Invoke(by, LocText.Of("rel.line.debunk.0"));
            if (rumor.SubjectHouse == 0) Notice?.Invoke(LocText.Of("rel.n.rumor_debunked", ("who", by.Name)), by.Id);
        }

        private void AddHouseScore(int house, int delta)
        {
            if (house <= 0 || delta == 0) return;
            _houseScore.TryGetValue(house, out var v);
            _houseScore[house] = v + delta;
        }

        /// <summary>Komşu hanenin bilgiyle oluşan namı (rakip sıralaması için).</summary>
        public int HouseScore(int house) => _houseScore.TryGetValue(house, out var v) ? v : 0;

        /// <summary>Köyün bildiği oran kadar itibar uygular (karar 67d: yayıldıkça).</summary>
        private void ApplyReputation(Fact f)
        {
            if (f.Value == 0 || f.Debunked) return;
            double share = Math.Min(1.0, KnowerCount(f) / (FullEffectShare * VillagerCount()));
            int target = (int)Math.Round(f.Value * share);
            int delta = target - f.Applied;
            if (delta == 0) return;
            f.Applied = target;
            if (f.SubjectHouse == 0)
                Reputation?.Invoke(f.Facet, delta, LocText.Of("rel.fact." + RelationRules.Snake(f.Kind.ToString())), _population.Find(f.Subject));
            else AddHouseScore(f.SubjectHouse, delta);
        }

        // ---- anlatma ----

        private static int Juice(Fact f)
        {
            switch (f.Kind)
            {
                case FactKind.Infidelity: case FactKind.Separation: case FactKind.BrokeEngagement: return 10;
                case FactKind.Fight: case FactKind.Rumor: case FactKind.Liar: return 8;
                case FactKind.Engagement: case FactKind.Sweethearts: case FactKind.BreakUp: return 6;
                case FactKind.Wedding: case FactKind.LostChild: case FactKind.Filth: case FactKind.AnimalTalk: return 5;
                case FactKind.HouseFeud: case FactKind.HouseBond: return 3;
                default: return 2;
            }
        }

        /// <summary>Anlatılacak en "sulu" bilgi: dinleyen bilmiyor, anlatan biliyor; sırları yalnız yakın dosta.</summary>
        public Fact PickToTell(Character teller, Character listener, bool allowRumor)
        {
            if (teller == null || listener == null || !_knows.TryGetValue(teller.Id, out var mine)) return null;
            Fact best = null; double bestScore = 0;
            int f2 = Relations?.Friendship(teller.Id, listener.Id) ?? 0;
            foreach (var f in _facts)
            {
                if (f.Debunked || !mine.Contains(f.Id) || Knows(listener.Id, f)) continue;
                if (!allowRumor && !f.Truth) continue;
                if (f.Subject == listener.Id || f.Partner == 0 && f.Other == listener.Id) continue;
                if (f.Secret && f2 < 50) continue;
                // Kanka'sının kötü haberini kolay anlatmaz.
                int toSubject = Relations?.Friendship(teller.Id, f.Subject) ?? 0;
                if (f.Value < 0 && toSubject >= 80 && f.Subject != teller.Id) continue;
                if (f.Subject == teller.Id && f.Value < 0) continue;
                double score = Juice(f) * (1.0 + Math.Max(0, -toSubject) / 100.0) / (1.0 + (Today() - f.Day) / 3.0);
                if (score > bestScore) { best = f; bestScore = score; }
            }
            return best;
        }

        /// <summary>Anlatır: dinleyen öğrenir; anlatan dedikoducuysa abartır. Dönen metin bildirim içindir.</summary>
        public LocText TellBest(Character teller, Character listener, bool allowRumor, bool quiet = false)
        {
            var f = PickToTell(teller, listener, allowRumor);
            if (f == null) return default;
            if (RelationRules.Gossipy(teller) > 1.2 && f.Value < 0 && !f.Memory && _rng.NextDouble() < 0.3) f.Value -= 1;   // abartma
            bool learned = Learn(listener, f, teller);
            if (!quiet)
            {
                Bubble?.Invoke(teller, LineFor(f));
                Bubble?.Invoke(listener, LocText.Of("rel.line.gasp." + _rng.Next(RelationLines.Count("gasp"))));
            }
            // Eşin sırrını yaymak (T8).
            if (learned && f.Secret && Relations != null)
            {
                var subj = _population.Find(f.Subject);
                if (subj != null && teller.SpouseId == subj.Id) Relations.ReportTrigger(SpouseTrigger.SpilledSecret, teller, true);
            }
            return learned ? LocText.Of("rel.n.gossip", ("a", teller.Name), ("b", listener.Name), ("what", Describe(f))) : default;
        }

        public LocText LineFor(Fact f)
        {
            var subject = _population.Find(f.Subject); var other = _population.Find(f.Other);
            string group = "gossip." + RelationRules.Snake(f.Kind.ToString());
            int n = RelationLines.Count(group);
            if (n == 0) { group = "gossip.generic"; n = RelationLines.Count(group); }
            return LocText.Of("rel.line." + group + "." + _rng.Next(Math.Max(1, n)), ("a", subject?.Name ?? ""), ("b", other?.Name ?? ""),
                ("house", f.SubjectHouse > 0 ? HouseName(f.SubjectHouse) : ""));
        }

        public LocText Describe(Fact f)
        {
            var subject = _population.Find(f.Subject); var other = _population.Find(f.Other);
            return LocText.Of("rel.fact." + RelationRules.Snake(f.Kind.ToString()) + ".d", ("a", subject?.Name ?? ""), ("b", other?.Name ?? ""),
                ("house", f.SubjectHouse > 0 ? HouseName(f.SubjectHouse) : ""));
        }

        // ---- söylenti (karar 67d) ----

        public bool HasPreparedRumor(int character) => _prepared.ContainsKey(character);

        /// <summary>Oyuncu bir rakip hane hakkında söylenti hazırlar (panelde): hane ve tür (yön).</summary>
        public bool PrepareRumor(Character teller, int house, RepFacet facet)
        {
            if (teller == null || house <= 0) return false;
            _prepared[teller.Id] = (house, FactKind.Rumor);
            _preparedFacet[teller.Id] = facet;
            return true;
        }

        private readonly Dictionary<int, RepFacet> _preparedFacet = new Dictionary<int, RepFacet>();

        public LocText SpreadPrepared(Character teller, Character listener)
        {
            if (!_prepared.TryGetValue(teller.Id, out var prep)) return default;
            _prepared.Remove(teller.Id);
            _preparedFacet.TryGetValue(teller.Id, out var facet);
            Character head = null;
            if (HouseMembersById != null) foreach (var m in HouseMembersById(prep.house)) { if (!m.IsDead && (head == null || m.AgeDays > head.AgeDays)) head = m; }
            if (head == null) return default;
            int charm = teller.Skills.GetLevel(SkillType.Charm);
            var f = new Fact
            {
                Id = _nextId++, Kind = FactKind.Rumor, Subject = head.Id, SubjectHouse = prep.house, Facet = facet, Value = -(3 + charm / 3),
                Truth = false, Source = teller.Id, Day = Today()
            };
            _facts.Add(f);
            Set(teller.Id).Add(f.Id);
            if (!Learn(listener, f, teller)) return LocText.Of("rel.n.rumor_failed", ("b", listener.Name));
            Bubble?.Invoke(teller, LineFor(f));
            return LocText.Of("rel.n.rumor_spread", ("a", teller.Name), ("b", listener.Name), ("house", HouseName(prep.house)));
        }

        /// <summary>Oyuncu hanesi hakkında, dinleyenin bildiği çürütülmemiş söylenti.</summary>
        public Fact RumorAbout(int house, int listener)
        {
            foreach (var f in _facts)
                if (f.Kind == FactKind.Rumor && !f.Truth && !f.Debunked && f.SubjectHouse == house && Knows(listener, f)) return f;
            return null;
        }

        /// <summary>Yalanla (67c §3.6): Gönül kazanma ve ilişkiyle şans; tutarsa söylenti çürür.</summary>
        public LocText Deny(Character actor, Character listener)
        {
            var f = RumorAbout(HouseOf(actor), listener.Id);
            if (f == null) return default;
            double chance = 0.4 + actor.Skills.GetLevel(SkillType.Charm) * 0.05 + (Relations?.Friendship(actor.Id, listener.Id) ?? 0) / 200.0;
            if (_rng.NextDouble() >= chance) return LocText.Of("rel.n.deny_failed", ("b", listener.Name));
            Debunk(f, listener, null);
            return LocText.Of("rel.n.deny_ok", ("a", actor.Name), ("b", listener.Name));
        }

        // ---- günlük: unutma, rakip söylentisi ----

        public void OnNewDay(long day)
        {
            double now = Today();
            foreach (var kv in _knows)
            {
                var c = _population.Find(kv.Key);
                if (c == null || c.IsDead) continue;
                bool dalgin = AbsentMinded(c);
                kv.Value.RemoveWhere(id =>
                {
                    var f = Get(id);
                    if (f == null) return true;
                    if (f.Subject == c.Id || f.Other == c.Id) return false;
                    if (f.Memory) return dalgin && _rng.NextDouble() < 0.02;
                    double age = now - f.Day;
                    return age > ForgetDays && _rng.NextDouble() < (dalgin ? 0.5 : 0.2);
                });
            }
            // Kimsenin bilmediği eski bilgi kaldırılır (köy hafızası kalır).
            _facts.RemoveAll(f => !f.Memory && now - f.Day > ForgetDays * 2 && KnowerCount(f, false) == 0);
            foreach (var f in _facts) ApplyReputation(f);
            RivalRumors(now);
        }

        /// <summary>Rakip haneler oyuncu hakkında rastgele zamanlarda söylenti çıkarır: yılda ~3, aynı hane 10 gün ara.</summary>
        private void RivalRumors(double now)
        {
            int year = Math.Max(1, DaysPerYear());
            double chance = (double)RivalRumorsPerYear / year;
            if (_rng.NextDouble() >= chance) return;
            var houses = new List<int>(RivalHouses());
            if (houses.Count == 0) return;
            int house = houses[_rng.Next(houses.Count)];
            if (_lastRivalRumor.TryGetValue(house, out var last) && now - last < RumorGapDays) return;
            if (KnowsAnyHouseBond(house)) return;   // aile dostu hane söylenti çıkarmaz
            Character teller = null, head = null;
            if (HouseMembersById != null) foreach (var m in HouseMembersById(house)) if (!m.IsDead && m.Stage >= AgeStage.YoungAdult) { teller = m; break; }
            foreach (var m in _population.Members) if (!m.IsDead && !m.IsVillager && (head == null || m.AgeDays > head.AgeDays)) head = m;
            if (teller == null || head == null) return;
            int rel = Relations?.Friendship(teller.Id, head.Id) ?? 0;
            if (rel >= 20 && _rng.NextDouble() < 0.7) return;   // arkadaş hane seyrek
            _lastRivalRumor[house] = now;
            var facet = (RepFacet)_rng.Next(4);
            var f = new Fact { Id = _nextId++, Kind = FactKind.Rumor, Subject = head.Id, SubjectHouse = 0, Facet = facet, Value = -3, Truth = false, Source = teller.Id, Day = now };
            _facts.Add(f);
            Set(teller.Id).Add(f.Id);
            Notice?.Invoke(LocText.Of("rel.n.rival_rumor", ("house", HouseName(house))), head.Id);
        }

        private bool KnowsAnyHouseBond(int house)
        {
            foreach (var f in _facts) if (f.Kind == FactKind.HouseBond && !f.Debunked && ((f.SubjectHouse == house && f.OtherHouse == 0) || (f.OtherHouse == house && f.SubjectHouse == 0))) return true;
            return false;
        }

        /// <summary>Masal/sofra: büyük, çocuğa ailenin küslük ve dostluk bilgilerini anlatır (aktarım).</summary>
        public int TellFamilyLore(Character elder, Character child)
        {
            if (elder == null || child == null || !_knows.TryGetValue(elder.Id, out var mine)) return 0;
            int n = 0;
            foreach (var f in _facts)
                if ((f.Kind == FactKind.HouseFeud || f.Kind == FactKind.HouseBond || f.Memory) && mine.Contains(f.Id) && !Knows(child.Id, f))
                { Set(child.Id).Add(f.Id); n++; }
            return n;
        }

        /// <summary>Bilinen haber listesi (panel): oyuncu hanesi hakkında, kaç köylü biliyor.</summary>
        public IEnumerable<(Fact fact, int knowers)> AboutHouse(int house)
        {
            foreach (var f in _facts)
                if (!f.Debunked && (f.SubjectHouse == house || (f.Kind == FactKind.HouseFeud || f.Kind == FactKind.HouseBond) && f.OtherHouse == house))
                    yield return (f, KnowerCount(f));
        }

        public IEnumerable<Fact> KnownBy(int character)
        {
            if (!_knows.TryGetValue(character, out var s)) yield break;
            foreach (var f in _facts) if (s.Contains(f.Id) && !f.Debunked) yield return f;
        }

        // ---- kayıt ----

        public KnowledgeSave Export()
        {
            var facts = new List<FactSave>();
            foreach (var f in _facts)
                facts.Add(new FactSave
                {
                    id = f.Id, kind = (int)f.Kind, subject = f.Subject, other = f.Other, subjectHouse = f.SubjectHouse, otherHouse = f.OtherHouse,
                    facet = (int)f.Facet, value = f.Value, truth = f.Truth, secret = f.Secret, source = f.Source, partner = f.Partner, day = f.Day,
                    applied = f.Applied, memory = f.Memory, debunked = f.Debunked
                });
            var knows = new List<KnowsSave>();
            foreach (var kv in _knows) if (kv.Value.Count > 0) knows.Add(new KnowsSave { character = kv.Key, facts = new List<int>(kv.Value).ToArray() });
            var scores = new List<int>();
            foreach (var kv in _houseScore) { scores.Add(kv.Key); scores.Add(kv.Value); }
            var rivals = new List<double>();
            foreach (var kv in _lastRivalRumor) { rivals.Add(kv.Key); rivals.Add(kv.Value); }
            return new KnowledgeSave { nextId = _nextId, facts = facts.ToArray(), knows = knows.ToArray(), houseScores = scores.ToArray(), rivalRumors = rivals.ToArray() };
        }

        public void Import(KnowledgeSave s)
        {
            _facts.Clear(); _knows.Clear(); _houseScore.Clear(); _lastRivalRumor.Clear(); _prepared.Clear();
            if (s == null) return;
            _nextId = Math.Max(1, s.nextId);
            if (s.facts != null)
                foreach (var x in s.facts)
                    if (x != null)
                        _facts.Add(new Fact
                        {
                            Id = x.id, Kind = (FactKind)x.kind, Subject = x.subject, Other = x.other, SubjectHouse = x.subjectHouse, OtherHouse = x.otherHouse,
                            Facet = (RepFacet)x.facet, Value = x.value, Truth = x.truth, Secret = x.secret, Source = x.source, Partner = x.partner, Day = x.day,
                            Applied = x.applied, Memory = x.memory, Debunked = x.debunked
                        });
            if (s.knows != null) foreach (var k in s.knows) if (k?.facts != null) Set(k.character).UnionWith(k.facts);
            if (s.houseScores != null) for (int i = 0; i + 1 < s.houseScores.Length; i += 2) _houseScore[s.houseScores[i]] = s.houseScores[i + 1];
            if (s.rivalRumors != null) for (int i = 0; i + 1 < s.rivalRumors.Length; i += 2) _lastRivalRumor[(int)s.rivalRumors[i]] = s.rivalRumors[i + 1];
        }
    }

    [Serializable] public sealed class FactSave
    {
        public int id, kind, subject, other, subjectHouse, otherHouse, facet, value, source, partner, applied;
        public bool truth, secret, memory, debunked;
        public double day;
    }

    [Serializable] public sealed class KnowsSave { public int character; public int[] facts; }

    [Serializable] public sealed class KnowledgeSave
    {
        public int nextId;
        public FactSave[] facts;
        public KnowsSave[] knows;
        public int[] houseScores;
        public double[] rivalRumors;
    }
}
