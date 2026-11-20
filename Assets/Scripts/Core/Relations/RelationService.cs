using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>İki kişi arasındaki ilişkinin arkadaşlık dışındaki her şeyi (arkadaşlık FamilyService'te kalır).</summary>
    public sealed class PairState
    {
        public int A, B;
        public int Romance;
        public CoupleStatus Status;
        /// <summary>Durumun başladığı gün (sevgili, nişan).</summary>
        public double StatusDay = -1;
        public double LastMet = -1;
        public long ChatDay = -1;
        public int ChatsToday;
        public FamilyDynamic Dynamic;
        /// <summary>Türün kayma birikimi: −1 … +1 (eşiği geçince tür değişir, önce uyarı).</summary>
        public double Drift;
        public bool DriftWarned;
        /// <summary>Eşlerde Sıcaklık 0-100 (67 §6-6); eş değilse −1.</summary>
        public int Warmth = -1;
        public bool Sulking;
        public int SulkBy;
        public double SulkDay = -1;
        /// <summary>Nişanlıda planlanan düğün günü; yoksa −1.</summary>
        public double WeddingDay = -1;
        /// <summary>Evlilik günü (yıldönümü için); yoksa −1.</summary>
        public double MarriedDay = -1;
        public int Betrayals;
        /// <summary>Sıcaklık 10'un altında geçen gün (ihanet ayrılığı için).</summary>
        public int ColdDays;
        /// <summary>İlk tanışma yapıldı (aile dostluğu ya da küslüğü başlangıç değerini verdi).</summary>
        public bool Introduced;
        /// <summary>Kıskançlık ve flört tanıklığı için: son flört günü.</summary>
        public double LastFlirt = -1;
        /// <summary>Yıldönümü bu yıl kutlandı mı (yıl numarası).</summary>
        public int AnniversaryYear = -1;
    }

    /// <summary>Gönül tercihleri: 2 hoşlanılan, 1 hoşlanılmayan (67b §4.1).</summary>
    public sealed class AttractionPrefs
    {
        public AttractionTag Like1, Like2, Dislike;
    }

    /// <summary>
    /// İlişki sistemi (Project belgeleri 67, 67a-d): iki eksen (arkadaşlık FamilyService'te, romantik burada), aşamalar, küslük,
    /// azalma, hisler, uyum, gönül tercihleri, aile ilişki türleri, sıcaklık. Kavga ve sosyal işler, gönül yolu, evlilik
    /// ve köy ağı ayrı partial dosyalarda.
    /// </summary>
    public sealed partial class RelationService
    {
        public const int DecayStartDays = 7;
        public const double DecayPerDay = 0.5;
        public const int SweetheartFriend = 20, SweetheartRomance = 30, AskRomance = 60, AskSweetheartDays = 7;
        public const int WarmthStart = 80;
        public const double WarmthDecayPerDay = 2.0;

        private readonly Population _population;
        private readonly Random _rng;
        private readonly Dictionary<long, PairState> _pairs = new Dictionary<long, PairState>();
        private readonly Dictionary<long, List<Feeling>> _feelings = new Dictionary<long, List<Feeling>>();
        private readonly Dictionary<int, AttractionPrefs> _prefs = new Dictionary<int, AttractionPrefs>();
        /// <summary>(bilen, kişi) → bilinen tercih sayısı (0-3): tercihler sohbette öğrenilir.</summary>
        private readonly Dictionary<long, int> _prefsKnown = new Dictionary<long, int>();
        private readonly Dictionary<long, double> _decayAcc = new Dictionary<long, double>();

        public RelationService(Population population, int seed = 67)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _rng = new Random(seed);
        }

        // ---- kancalar ----
        public FamilyService Family { get; set; }
        public Func<double> Today { get; set; } = () => 0;
        /// <summary>Bildirim (metin, kişi).</summary>
        public Action<LocText, int> Notice { get; set; }
        /// <summary>Kişinin başında balon.</summary>
        public Action<Character, LocText> Bubble { get; set; }
        /// <summary>Kısa poz (karakter, poz adı, gerçek saniye).</summary>
        public Action<Character, string, float> Cue { get; set; }
        /// <summary>İtibar (yön, miktar, neden, kişi).</summary>
        public Action<RepFacet, int, LocText, Character> Reputation { get; set; }
        public KnowledgeService Knowledge { get; set; }
        /// <summary>Kişinin hanesi: oyuncu hanesi 0, komşu hane kimliği, bilinmiyorsa −1.</summary>
        public Func<Character, int> HouseOf { get; set; } = c => c == null ? -1 : c.IsVillager ? -1 : 0;
        public Func<bool> IsFestivalDay { get; set; } = () => false;
        /// <summary>Kişiyi yerinde tut (oyun günü): kavga ve sahne sırasında öbür taraf durur.</summary>
        public Action<Character, double> Hold { get; set; }

        public Population Population => _population;
        public IEnumerable<PairState> Pairs => _pairs.Values;

        public static long Key(int a, int b) => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        private static long Dir(int from, int to) => ((long)from << 32) | (uint)to;

        public PairState Pair(int a, int b)
        {
            long k = Key(a, b);
            if (!_pairs.TryGetValue(k, out var p))
            {
                p = new PairState { A = Math.Min(a, b), B = Math.Max(a, b) };
                _pairs[k] = p;
            }
            return p;
        }

        public PairState Find(int a, int b) => _pairs.TryGetValue(Key(a, b), out var p) ? p : null;

        // ---- arkadaşlık (FamilyService) ----

        public int Friendship(int a, int b) => Family?.Affection(a, b) ?? 0;

        public void AddFriendship(int a, int b, int delta)
        {
            if (Family == null || delta == 0) return;
            Family.SetAffection(a, b, Friendship(a, b) + delta);
        }

        public FriendStage Stage(int a, int b) => RelationRules.StageOf(Friendship(a, b));

        // ---- romantik ----

        public int Romance(int a, int b) => Find(a, b)?.Romance ?? 0;

        public bool IsCouple(int a, int b)
        {
            var ca = _population.Find(a);
            if (ca != null && ca.SpouseId == b) return true;
            var p = Find(a, b);
            return p != null && (p.Status == CoupleStatus.Sweetheart || p.Status == CoupleStatus.Engaged);
        }

        public bool IsEngaged(int a, int b) => Find(a, b)?.Status == CoupleStatus.Engaged;

        /// <summary>Romantik ilişkiye uygun mu: genç yetişkin ve üstü, karşı cins, akraba değil.</summary>
        public static bool CanRomance(Character a, Character b) =>
            a != null && b != null && a != b && !a.IsDead && !b.IsDead && FamilyService.IsAdultAge(a) && FamilyService.IsAdultAge(b)
            && a.Sex != b.Sex && !FamilyService.AreCloseRelatives(a, b);

        public void AddRomance(int a, int b, int delta)
        {
            if (delta == 0) return;
            var ca = _population.Find(a); var cb = _population.Find(b);
            if (delta > 0 && !CanRomance(ca, cb)) return;
            var p = Pair(a, b);
            int cap = IsCouple(a, b) ? RelationRules.Max : RelationRules.RomanceLockWithoutCouple;
            if (delta > 0 && ca != null && cb != null) delta = (int)Math.Round(delta * (AttractionFactor(ca, cb) + AttractionFactor(cb, ca)) / 2.0);
            p.Romance = Math.Max(RelationRules.Min, Math.Min(Math.Max(cap, p.Romance), p.Romance + delta));
        }

        public void SetRomance(int a, int b, int value) => Pair(a, b).Romance = Math.Max(RelationRules.Min, Math.Min(RelationRules.Max, value));

        public RomanceStage RomanceStage(int a, int b) => RelationRules.RomanceStageOf(Romance(a, b));

        public CoupleStatus Status(int a, int b) => Find(a, b)?.Status ?? CoupleStatus.None;

        /// <summary>Kişinin sevgilisi ya da nişanlısı (eşi değil); yoksa null.</summary>
        public Character PartnerOf(Character c)
        {
            if (c == null) return null;
            if (c.SpouseId.HasValue) return _population.Find(c.SpouseId.Value);
            foreach (var p in _pairs.Values)
                if ((p.A == c.Id || p.B == c.Id) && (p.Status == CoupleStatus.Sweetheart || p.Status == CoupleStatus.Engaged))
                {
                    var o = _population.Find(p.A == c.Id ? p.B : p.A);
                    if (o != null && !o.IsDead) return o;
                }
            return null;
        }

        // ---- küslük ----

        public bool IsSulking(int a, int b) => Find(a, b)?.Sulking ?? false;

        public void EndSulk(int a, int b)
        {
            var p = Find(a, b);
            if (p == null || !p.Sulking) return;
            p.Sulking = false;
            if (Friendship(a, b) < -19) Family?.SetAffection(a, b, -19);   // Tanıdık sınırına çıkar
        }

        // ---- günlük sohbet ve kazanç çarpanı ----

        /// <summary>
        /// Sohbet kazancının çarpanı (67 §6-7, 67a §9.1): günün ilk sohbeti tam, ikincisi yarı, sonrası 0; uyum, hisler ve yüksek
        /// değerde yavaşlama (Kanka ≈ 2 mevsim düzenli ilgi). Küslükte 0.
        /// </summary>
        public double ChatFactor(Character a, Character b, long day)
        {
            var p = Pair(a.Id, b.Id);
            if (p.Sulking) return 0;
            if (p.ChatDay != day) { p.ChatDay = day; p.ChatsToday = 0; }
            p.ChatsToday++;
            double f = p.ChatsToday == 1 ? 1.0 : p.ChatsToday == 2 ? 0.5 : 0.0;
            return f * GainFactor(a, b);
        }

        /// <summary>Arkadaşlık kazancına uyum, his ve yüksek değer yavaşlaması (sohbet dışı kazançlarda da kullanılır).</summary>
        public double GainFactor(Character a, Character b)
        {
            double f = RelationRules.CompatFactor(RelationRules.Compatibility(a, b));
            f *= FeelingGain(a.Id, b.Id) * FeelingGain(b.Id, a.Id);
            // Yüksek aşamada yavaşlar (karar 67a-11: düzenli ilgiyle Kanka ≈ 2 mevsim).
            int v = Friendship(a.Id, b.Id);
            f *= v < 20 ? 1.0 : v < 50 ? 0.5 : 0.3;
            if (Missing(a.Id, b.Id)) f *= 1.5;
            return f;
        }

        private bool Missing(int a, int b) => HasFeeling(a, b, FeelingKind.Misses) || HasFeeling(b, a, FeelingKind.Misses);

        /// <summary>İki kişi bugün görüştü (sohbet, iş, etkinlik): azalma sayacı sıfırlanır, özlem biter.</summary>
        public void Met(int a, int b)
        {
            var p = Pair(a, b);
            p.LastMet = Today();
            RemoveFeeling(a, b, FeelingKind.Misses);
            RemoveFeeling(b, a, FeelingKind.Misses);
        }

        // ---- hisler ----

        public IReadOnlyList<Feeling> Feelings(int from, int to) =>
            _feelings.TryGetValue(Dir(from, to), out var list) ? (IReadOnlyList<Feeling>)list : Array.Empty<Feeling>();

        public bool HasFeeling(int from, int to, FeelingKind kind)
        {
            if (!_feelings.TryGetValue(Dir(from, to), out var list)) return false;
            foreach (var f in list) if (f.Kind == kind) return true;
            return false;
        }

        public void AddFeeling(int from, int to, FeelingKind kind, double? days = null)
        {
            if (from == to) return;
            long k = Dir(from, to);
            if (!_feelings.TryGetValue(k, out var list)) { list = new List<Feeling>(); _feelings[k] = list; }
            double now = Today();
            double len = days ?? RelationRules.Feelings[kind].Days;
            foreach (var f in list)
                if (f.Kind == kind) { f.UntilDay = Math.Max(f.UntilDay, now + len); return; }
            // Kalbi kırık ve ihanet "güven"i, kırgınlık "yakın hissetme"yi siler.
            if (kind == FeelingKind.Betrayed || kind == FeelingKind.Heartbroken) list.RemoveAll(f => f.Kind == FeelingKind.Trusts || f.Kind == FeelingKind.Close);
            if (kind == FeelingKind.Hurt || kind == FeelingKind.Grudge) list.RemoveAll(f => f.Kind == FeelingKind.Close);
            list.Add(new Feeling { Kind = kind, FromDay = now, UntilDay = now + len });
            if (list.Count > RelationRules.MaxFeelingsPerPair)
            {
                // En zayıf (en erken bitecek) his gider.
                Feeling weakest = list[0];
                foreach (var f in list) if (f.UntilDay < weakest.UntilDay) weakest = f;
                list.Remove(weakest);
            }
        }

        public void RemoveFeeling(int from, int to, FeelingKind kind)
        {
            if (_feelings.TryGetValue(Dir(from, to), out var list)) list.RemoveAll(f => f.Kind == kind);
        }

        private double FeelingGain(int from, int to)
        {
            if (!_feelings.TryGetValue(Dir(from, to), out var list) || list.Count == 0) return 1.0;
            double f = 1.0;
            foreach (var x in list) if (x.Kind != FeelingKind.Misses) f *= RelationRules.Feelings[x.Kind].GainFactor;
            return Math.Max(0.2, Math.Min(2.0, f));
        }

        public IEnumerable<(int from, int to, Feeling feeling)> AllFeelings()
        {
            foreach (var kv in _feelings)
                foreach (var f in kv.Value) yield return ((int)(kv.Key >> 32), (int)(kv.Key & 0xffffffff), f);
        }

        // ---- gönül tercihleri ----

        public AttractionPrefs PrefsOf(Character c)
        {
            if (c == null) return null;
            if (_prefs.TryGetValue(c.Id, out var p)) return p;
            var r = new Random(unchecked(c.Id * 7919 + 67));
            var tags = RelationRules.AllTags;
            var l1 = tags[r.Next(tags.Length)];
            AttractionTag l2;
            do l2 = tags[r.Next(tags.Length)]; while (l2 == l1);
            AttractionTag d;
            do d = tags[r.Next(tags.Length)]; while (d == l1 || d == l2 || Opposite(d, l1) || Opposite(d, l2));
            p = new AttractionPrefs { Like1 = l1, Like2 = l2, Dislike = d };
            _prefs[c.Id] = p;
            return p;
        }

        private static bool Opposite(AttractionTag x, AttractionTag y) =>
            (x == AttractionTag.Bearded && y == AttractionTag.CleanShaven) || (y == AttractionTag.Bearded && x == AttractionTag.CleanShaven);

        public void SetPrefs(Character c, AttractionPrefs prefs) { if (c != null && prefs != null) _prefs[c.Id] = prefs; }

        /// <summary>Gönül tercihine göre romantik kazanç çarpanı ×0,8 … ×1,25 (asla engel değil).</summary>
        public double AttractionFactor(Character who, Character toward)
        {
            var p = PrefsOf(who);
            if (p == null || toward == null) return 1.0;
            double f = 1.0;
            if (RelationRules.Matches(toward, p.Like1)) f += 0.125;
            if (RelationRules.Matches(toward, p.Like2)) f += 0.125;
            if (RelationRules.Matches(toward, p.Dislike)) f -= 0.2;
            return Math.Max(0.8, Math.Min(1.25, f));
        }

        /// <summary>Bilenin, kişinin tercihlerinden kaçını bildiği (0-3; sırası: hoşlandığı 1, 2, hoşlanmadığı).</summary>
        public int KnownPrefs(int knower, int about) => knower == about ? 3 : _prefsKnown.TryGetValue(Dir(knower, about), out var n) ? n : 0;

        /// <summary>Sohbette tercihlerden biri öğrenilir (şansla); öğrenilen sırayı döner, yoksa −1.</summary>
        public int LearnPref(int knower, int about, double chance = 0.25)
        {
            int n = KnownPrefs(knower, about);
            if (n >= 3 || _rng.NextDouble() >= chance) return -1;
            _prefsKnown[Dir(knower, about)] = n + 1;
            return n;
        }

        public void SetKnownPrefs(int knower, int about, int n) => _prefsKnown[Dir(knower, about)] = Math.Max(0, Math.Min(3, n));

        // ---- aile ilişki türü ----

        public FamilyDynamic DynamicOf(int a, int b) => Find(a, b)?.Dynamic ?? FamilyDynamic.None;

        public static bool IsKin(Character a, Character b) =>
            a != null && b != null && (a.SpouseId == b.Id || FamilyService.AreCloseRelatives(a, b));

        /// <summary>
        /// Oyuncu yalnızca başlangıç hanesi için seçer (karar 67a §8): oyunun ilk günü, ikisi de hanede. Sonradan doğanlara atanamaz.
        /// </summary>
        public string CanChooseDynamic(Character a, Character b)
        {
            if (a == null || b == null || a.IsVillager || b.IsVillager) return CoreText.T("rel.why.dyn_household");
            if (!IsKin(a, b)) return CoreText.T("rel.why.dyn_kin");
            if (Today() >= 1.0) return CoreText.T("rel.why.dyn_start");
            return null;
        }

        public bool ChooseDynamic(Character a, Character b, FamilyDynamic d)
        {
            if (CanChooseDynamic(a, b) != null) return false;
            var p = Pair(a.Id, b.Id);
            p.Dynamic = d;
            p.Drift = 0;
            return true;
        }

        /// <summary>Doğumda bağ (67 §P3-1): anne ve baba ile bebek Arkadaş+ (60), kardeşler 30; tür kendiliğinden "Yakın".</summary>
        public void OnBorn(Character baby)
        {
            if (baby == null) return;
            foreach (var m in _population.Members)
            {
                if (m == baby || m.IsDead) continue;
                bool parent = baby.BirthMotherId == m.Id || baby.BirthFatherId == m.Id;
                bool sibling = !parent && FamilyService.AreCloseRelatives(baby, m);
                if (!parent && !sibling) continue;
                Family?.SetAffection(baby.Id, m.Id, Math.Max(Friendship(baby.Id, m.Id), parent ? 60 : 30));
                var p = Pair(baby.Id, m.Id);
                if (p.Dynamic == FamilyDynamic.None) p.Dynamic = FamilyDynamic.Close;
                p.Introduced = true;
            }
        }

        /// <summary>Kaymayı biriktirir (+ yakınlaştırır, − uzaklaştırır); eşik geçilince tür değişir (önce uyarı).</summary>
        public void Nudge(int a, int b, double amount)
        {
            var ca = _population.Find(a); var cb = _population.Find(b);
            if (!IsKin(ca, cb)) return;
            var p = Pair(a, b);
            if (p.Dynamic == FamilyDynamic.None) p.Dynamic = FamilyDynamic.Close;
            p.Drift = Math.Max(-1.0, Math.Min(1.0, p.Drift + amount));
        }

        private void UpdateDynamic(PairState p, Character a, Character b)
        {
            if (p.Dynamic == FamilyDynamic.None) return;
            // Uzun ilgisizlik uzaklaştırır.
            if (p.LastMet >= 0 && Today() - p.LastMet > 5) p.Drift = Math.Max(-1.0, p.Drift - 0.05);
            FamilyDynamic next = p.Dynamic;
            if (p.Drift <= -0.6)
                next = p.Dynamic == FamilyDynamic.Strict ? FamilyDynamic.Tense
                    : p.Dynamic == FamilyDynamic.Close || p.Dynamic == FamilyDynamic.Jokester || p.Dynamic == FamilyDynamic.Protective ? FamilyDynamic.Distant
                    : p.Dynamic;
            else if (p.Drift >= 0.6)
                next = p.Dynamic == FamilyDynamic.Distant || p.Dynamic == FamilyDynamic.Tense ? FamilyDynamic.Close
                    : p.Dynamic == FamilyDynamic.Jokester ? FamilyDynamic.Close : p.Dynamic;
            if (next == p.Dynamic) { if (Math.Abs(p.Drift) < 0.3) p.DriftWarned = false; return; }
            if (!p.DriftWarned)
            {
                p.DriftWarned = true;
                Notice?.Invoke(LocText.Of(p.Drift < 0 ? "rel.n.drift_apart" : "rel.n.drift_closer", ("a", a.Name), ("b", b.Name)), a.Id);
                p.Drift *= 0.75;   // bir mevsimlik süre: uyarıdan sonra bir kez daha birikmeli
                return;
            }
            p.Dynamic = next;
            p.Drift = 0;
            p.DriftWarned = false;
            Notice?.Invoke(LocText.Of("rel.n.dynamic_changed", ("a", a.Name), ("b", b.Name), ("kind", RelationRules.DynamicText(next))), a.Id);
        }

        /// <summary>Aile ilişki türüne göre sosyal kazanç: Yakın ve Şakacı daha çok, Mesafeli ve Gergin daha az.</summary>
        public double DynamicFactor(int a, int b)
        {
            switch (DynamicOf(a, b))
            {
                case FamilyDynamic.Close: return 1.25;
                case FamilyDynamic.Jokester: return 1.15;
                case FamilyDynamic.Protective: return 1.1;
                case FamilyDynamic.Distant: return 0.7;
                case FamilyDynamic.Tense: return 0.6;
                case FamilyDynamic.Strict: return 0.9;
                default: return 1.0;
            }
        }

        // ---- ilk tanışma (aile dostluğu / küslüğü, 67a §7, 67c §3.7) ----

        /// <summary>
        /// İki kişi ilk kez karşılaşıyor: aile dostluğu ya da küslüğünü **biliyorlarsa** başlangıç değeri verir. Kimse
        /// anlatmadıysa küslük de dostluk da söner (karar 67d).
        /// </summary>
        public void Introduce(Character a, Character b)
        {
            if (a == null || b == null) return;
            var p = Pair(a.Id, b.Id);
            if (p.Introduced) return;
            p.Introduced = true;
            if (Knowledge == null || Friendship(a.Id, b.Id) != 0) return;
            int ha = HouseOf(a), hb = HouseOf(b);
            if (ha < 0 || hb < 0 || ha == hb) return;
            bool aKnowsFeud = Knowledge.KnowsHouseLink(a.Id, ha, hb, FactKind.HouseFeud), bKnowsFeud = Knowledge.KnowsHouseLink(b.Id, ha, hb, FactKind.HouseFeud);
            bool aKnowsBond = Knowledge.KnowsHouseLink(a.Id, ha, hb, FactKind.HouseBond), bKnowsBond = Knowledge.KnowsHouseLink(b.Id, ha, hb, FactKind.HouseBond);
            if (aKnowsFeud || bKnowsFeud) Family?.SetAffection(a.Id, b.Id, (aKnowsFeud && bKnowsFeud) ? -20 : -10);
            else if (aKnowsBond || bKnowsBond) Family?.SetAffection(a.Id, b.Id, (aKnowsBond && bKnowsBond) ? 20 : 10);
        }

        // ---- günlük ----

        /// <summary>Gün döndü: azalma, his süreleri, özlem, sıcaklık, tür kayması, küslük, gönül yolu ve eş takvimi.</summary>
        public void OnNewDay(long day)
        {
            double now = Today();
            // Hisler.
            foreach (var kv in _feelings) kv.Value.RemoveAll(f => f.UntilDay <= now);
            // Azalma (aile içi ve eşler azalmaz; arkadaşlar aşamanın tabanında durur).
            if (Family != null)
            {
                var pairs = new List<(int a, int b, int v)>(Family.ExportAffection());
                foreach (var (a, b, v) in pairs)
                {
                    var ca = _population.Find(a); var cb = _population.Find(b);
                    if (ca == null || cb == null || ca.IsDead || cb.IsDead) continue;
                    if (IsKin(ca, cb)) continue;
                    var p = Find(a, b);
                    double last = p?.LastMet ?? -1;
                    if (last < 0) { Pair(a, b).LastMet = now; continue; }
                    if (now - last < DecayStartDays) continue;
                    if (v > 0)
                    {
                        int floor = RelationRules.DecayFloor(v);
                        if (v <= floor) { if (v >= 20 && !HasFeeling(a, b, FeelingKind.Misses)) { AddFeeling(a, b, FeelingKind.Misses); AddFeeling(b, a, FeelingKind.Misses); } continue; }
                        Accumulate(a, b, -DecayPerDay, floor);
                    }
                    else if (v < 0)
                    {
                        // Düşmanlık da zamanla yumuşar (kin tutan daha yavaş).
                        double rate = HasFeeling(a, b, FeelingKind.Grudge) || HasFeeling(b, a, FeelingKind.Grudge) ? DecayPerDay * 0.3 : DecayPerDay;
                        Accumulate(a, b, rate, 0);
                    }
                }
            }
            // Evli çiftlerin kaydı (evlilik başka yoldan kurulduysa).
            foreach (var m in _population.Members)
                if (!m.IsDead && m.SpouseId.HasValue && m.Id < m.SpouseId.Value) Pair(m.Id, m.SpouseId.Value);
            // Çiftler: sıcaklık, kayma, küslük, gönül yolu, eşler.
            foreach (var p in new List<PairState>(_pairs.Values))
            {
                var a = _population.Find(p.A); var b = _population.Find(p.B);
                if (a == null || b == null) continue;
                if (a.IsDead || b.IsDead)
                {
                    if (p.Status == CoupleStatus.Sweetheart || p.Status == CoupleStatus.Engaged) { p.Status = CoupleStatus.None; p.WeddingDay = -1; }
                    continue;
                }
                UpdateDynamic(p, a, b);
                // Küslük kendiliğinden bitmez; ama arkadaşlık Tanıdık'a çıktıysa (hediye, iyilik) düşer.
                if (p.Sulking && Friendship(p.A, p.B) > -20) p.Sulking = false;
                MarriageNewDay(p, a, b, now);
            }
            CourtshipNewDay(day, now);
            VillageNewDay(day, now);
            EndDayChores();
        }

        private void Accumulate(int a, int b, double delta, int floor)
        {
            long k = Key(a, b);
            _decayAcc.TryGetValue(k, out var acc);
            acc += delta;
            int whole = (int)acc;
            if (whole == 0) { _decayAcc[k] = acc; return; }
            _decayAcc[k] = acc - whole;
            int v = Friendship(a, b);
            int next = v + whole;
            if (delta < 0) next = Math.Max(floor, next); else next = Math.Min(floor, next);
            Family?.SetAffection(a, b, next);
        }

        // ---- yas (67 H5) ----

        /// <summary>
        /// Ölümün yasını ilişkiye göre ölçekler: akrabada ilişki eksiyse yas hafifler; akraba olmayan Kanka ve eşinin
        /// sevgilisi "Dostunu yitirdi" yası tutar; cenazede yanında duran "Yası ortak" hissi alır.
        /// </summary>
        public int OnDeath(Character dead)
        {
            if (dead == null) return 0;
            int n = 0;
            foreach (var m in _population.Members)
            {
                if (m == dead || m.IsDead || m.Stage == AgeStage.Baby) continue;
                int f = Friendship(dead.Id, m.Id);
                bool kin = Mood.MoodEvents.IsClose(dead, m);
                if (kin)
                {
                    if (f < -19) { m.Mood.Remove(Mood.MoodCatalog.Grief); m.AddMoodlet(RelationMoods.GriefLight, CoreText.T("rel.mood.grief_light"), -8, 3.0); n++; }
                }
                else if (f >= 80 || IsCouple(dead.Id, m.Id))
                {
                    m.AddMoodlet(RelationMoods.GriefFriend, CoreText.T("rel.mood.grief_friend"), -20, 6.0);
                    n++;
                }
            }
            // Yası paylaşanlar: ölenin yakınlarından ikisi arasında "Yası ortak".
            var mourners = new List<Character>();
            foreach (var m in _population.Members)
                if (!m.IsDead && m != dead && (Mood.MoodEvents.IsClose(dead, m) || Friendship(dead.Id, m.Id) >= 50)) mourners.Add(m);
            foreach (var x in mourners)
                foreach (var y in mourners)
                    if (x != y && Friendship(x.Id, y.Id) >= 0) AddFeeling(x.Id, y.Id, FeelingKind.SharedGrief);
            foreach (var p in _pairs.Values)
                if ((p.A == dead.Id || p.B == dead.Id) && (p.Status == CoupleStatus.Sweetheart || p.Status == CoupleStatus.Engaged))
                { p.Status = CoupleStatus.None; p.WeddingDay = -1; }
            return n;
        }

        // ---- yakında olma (his iç sesleri) ----

        /// <summary>Hisler iki kişi 4 karo içindeyken his sahibine küçük iç ses verir (ruh hali güncellemesinde çağrılır).</summary>
        public void UpdateNearMoods()
        {
            foreach (var kv in _feelings)
            {
                if (kv.Value.Count == 0) continue;
                var from = _population.Find((int)(kv.Key >> 32)); var to = _population.Find((int)(kv.Key & 0xffffffff));
                if (from == null || to == null || from.IsDead || to.IsDead) continue;
                if (Math.Abs(from.Position.X - to.Position.X) + Math.Abs(from.Position.Y - to.Position.Y) > 4 || from.Position.Z != to.Position.Z) continue;
                int value = 0;
                foreach (var f in kv.Value) value += RelationRules.Feelings[f.Kind].NearMood;
                if (value == 0) continue;
                from.AddMoodlet(value > 0 ? RelationMoods.NearGood : RelationMoods.NearBad,
                    CoreText.T(value > 0 ? "rel.mood.near_good" : "rel.mood.near_bad", ("name", to.Name)), Math.Max(-8, Math.Min(6, value)), 0.1);
            }
        }
    }

    /// <summary>İlişki sisteminin iç ses kimlikleri.</summary>
    public static class RelationMoods
    {
        public const string Bruised = "rel.bruised", Oops = "rel.oops", ProudWin = "rel.proud_win", ScaredFight = "rel.scared_fight",
            Argued = "rel.argued", MadeUp = "rel.made_up", Festival = "rel.festival_makeup", Sweethearts = "rel.sweethearts",
            Rejected = "rel.rejected", Engaged = "rel.engaged", SaltyOk = "rel.salty_ok", SaltyBad = "rel.salty_bad",
            WordGiven = "rel.word_given", Henna = "rel.henna", GivingAway = "rel.giving_away", Wedding = "rel.wedding",
            Blessing = "rel.blessing", HappyHome = "rel.happy_home", ColdHome = "rel.cold_home", Anniversary = "rel.anniversary",
            ForgotAnniv = "rel.forgot_anniversary", Forgave = "rel.forgave", Regret = "rel.regret", GriefFriend = "rel.grief_friend",
            GriefLight = "rel.grief_light", NearGood = "rel.near_good", NearBad = "rel.near_bad", Flirted = "rel.flirted",
            Kissed = "rel.kissed", Guest = "rel.wedding_guest", NotInvitedHurt = "rel.no_show", Council = "rel.council",
            Gossiped = "rel.gossiped", ChatCircle = "rel.chat_circle", Shamed = "rel.shamed", Rumored = "rel.rumored", Cleared = "rel.cleared";
    }
}
