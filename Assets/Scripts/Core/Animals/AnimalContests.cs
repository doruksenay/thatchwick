using Thatchwick.Core.Localization;
using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Animals
{
    /// <summary>Simüle edilen hayvan yarışmaları (Project belgesi 29, bölüm 4): oynanış ya da yarış ekranı yok.</summary>
    public enum ContestKind
    {
        /// <summary>At yarışı (Yaz Panayırı ana yarışı; ilkbahar/sonbahar köy koşusu unvan vermez).</summary>
        HorseRace = 0,
        /// <summary>İnek güzellik yarışması (Hasat Şenliği).</summary>
        BeautyContest = 1,
        /// <summary>Öküz kütük çekme (Hasat Şenliği).</summary>
        LogPull = 2,
        /// <summary>Tazı koşusu (Hasat Şenliği; tazı ve kırma tazı, belge 48 §6.4).</summary>
        HoundRace = 3,
        /// <summary>Kedi güzellik yarışması (Yaz Panayırı, belge 48 §6.4).</summary>
        CatShow = 4
    }

    public sealed class ContestResult
    {
        public ContestKind Kind;
        /// <summary>1 = birinci.</summary>
        public int Place;
        public int Entrants;
        public bool Won => Place == 1;
        /// <summary>Kayıtta gösterilen kazanma şansı ("Bahisçiler: %18").</summary>
        public double WinChance;
        public int PrizeAkce;
        public double AnimalXp;
        /// <summary>Efsane unvanı bu yarışta kazanıldıysa unvan; yoksa null.</summary>
        public string LegendTitle;
        /// <summary>Kısa sonuç metni (sonuç kartı ve kronik).</summary>
        public string Summary;
        /// <summary>Kroniğe yazılan anahtarlı özet (SavedText); ekranda Summary gösterilir.</summary>
        public string SavedSummary;
    }

    /// <summary>
    /// Yarışma motoru: her katılımcının puanı + RNG (±%25; efsane atta ±%15). Rakipler yıla göre güçlenir. Kazanma şansı
    /// Monte Carlo ile önceden hesaplanır ve en çok %70'tir (favori rakip gerekirse güçlendirilir).
    /// </summary>
    public static class AnimalContests
    {
        public const int Rivals = 5;
        public const double RngSpread = 0.25, LegendRngSpread = 0.15, MaxWinChance = 0.70;
        public const int EstimateRuns = 400;
        /// <summary>Binici yarışta bu kadar gün yoktur (≈2 oyun saati); oyun katmanı bu sürede onu kasabaya gönderip geri getirir.</summary>
        public const double DurationDays = 2.0 / 24.0;
        public const int EntryFee = 10;
        public const int MinRaceLevel = 3;

        private static readonly int[] Prizes = { 120, 60, 30 };
        private static readonly double[] XpByPlace = { AnimalProgress.XpRace, AnimalProgress.XpRace / 2, AnimalProgress.XpRace / 4 };

        /// <summary>Katılım engeli; uygunsa null.</summary>
        public static string Blocker(ContestKind kind, Animal a, Character handler)
        {
            if (a == null || a.IsDead) return "Hayvan yok.";
            if (handler == null || handler.IsDead) return "Götürecek kimse yok.";
            var st = handler.Stage;
            if (st == AgeStage.Baby || st == AgeStage.Child || st == AgeStage.Teen) return "Yalnızca genç yetişkin ve üstü katılabilir.";
            switch (kind)
            {
                case ContestKind.HorseRace:
                    if (!a.Species.Rideable || a.Species.Id != Genetics.GeneticsCatalog.Horse) return "Yalnızca atlar yarışır.";
                    if (a.Saddle == null) return "At eyerli olmalı.";
                    if (a.Level < MinRaceLevel) return $"At {MinRaceLevel}. seviyede olmalı.";
                    return null;
                case ContestKind.BeautyContest:
                    return a.Species.Id == "animal.cow" && a.IsFemale && a.IsMature ? null : "Yalnızca erişkin inekler katılır.";
                case ContestKind.HoundRace:
                    return a.TypeId == "dog.sighthound" || a.TypeId == "dog.lurcher" ? (a.IsMature ? null : "Tazı daha yavru.") : "Yalnızca tazılar koşar.";
                case ContestKind.CatShow:
                    return a.Species.Id == Genetics.GeneticsCatalog.Cat && a.IsMature ? null : "Yalnızca erişkin kediler katılır.";
                case ContestKind.LogPull:
                    // Öküz şimdilik sığırın erkeğidir (ayrı tür kararı: Project belgesi 29; içerik sonra).
                    return a.Species.Draft && !a.IsFemale && a.IsMature ? null : "Yalnızca erişkin öküz katılır.";
            }
            return "Bilinmeyen yarışma.";
        }

        /// <summary>Katılımcının taban puanı.</summary>
        /// <summary>Beceri ekleri (belge 45): Hayvancılık 9 yarışa hazırla, 10 efsane denemesi; Binicilik 9; yarış eyeri. null ise 0.</summary>
        public static Func<ContestKind, Animal, Character, double> ExtraScore { get; set; }
        /// <summary>Yarışma koştuktan sonra (hazırlık bonusu harcanır).</summary>
        public static Action<Animal> AfterRun { get; set; }

        /// <summary>Beceri eki: yarışa hazırlanmış (+8), Hayvancılık 10 (+5), Binicilik 9 yarışta (+10).</summary>
        public static double SkillBonus(ContestKind kind, Animal a, Character handler, bool prepared, bool raceSaddle)
        {
            double b = prepared ? 8 : 0;
            if (handler == null) return b;
            if (handler.Skills.GetLevel(SkillType.AnimalHusbandry) >= 10) b += 5;
            if (kind == ContestKind.HorseRace)
            {
                if (handler.Skills.GetLevel(SkillType.Riding) >= 9) b += 10;
                if (raceSaddle) b += 6;
            }
            return b;
        }

        public static double Score(ContestKind kind, Animal a, Character handler) => BaseScore(kind, a, handler) + (ExtraScore != null ? ExtraScore(kind, a, handler) : 0);

        public static double BaseScore(ContestKind kind, Animal a, Character handler)
        {
            double lineage = Math.Min((int)a.Lineage, (int)AnimalLineage.Legendary);
            switch (kind)
            {
                case ContestKind.HorseRace:
                    int riding = handler != null ? handler.Skills.GetLevel(SkillType.Riding) : 1;
                    double race = 10 * (lineage + a.Level) + a.Stamina / 5.0 + 3 * riding + a.Friendship / 10.0;
                    // Genom (belge 48): Hız istatistiği 5'in üstünde puan başına +3, Yarış Yüreği +%10.
                    if (a.Genome != null) race += 3 * (a.Genome.Stat(Genetics.GeneStat.Speed) - 5);
                    if (AnimalSystem.HasGift(a, Genetics.Gift.RaceHeart)) race *= 1.1;
                    return race;
                case ContestKind.HoundRace:
                    return 10 * (lineage + a.Level) + 4 * (a.Genome?.Stat(Genetics.GeneStat.Speed) ?? 5) + a.Friendship / 10.0;
                case ContestKind.CatShow:
                    bool showy = a.TypeId == "cat.angora" || a.TypeId == "cat.van" || a.Genome?.RareCoat != null;
                    return 8 * lineage + 4 * a.Level + a.Friendship * 0.4 + 2 * (a.Genome?.Stat(Genetics.GeneStat.Temper) ?? 5) + (showy ? 8 : 0);
                case ContestKind.BeautyContest:
                    return 8 * lineage + 4 * a.Level + a.Comfort * 0.3 + a.Friendship * 0.4;
                default:
                    return 8 * (lineage + a.Level) + a.Fullness / 10.0 + a.Friendship / 10.0;
            }
        }

        /// <summary>Yıla göre rakip puanları (köy büyüdükçe rakipler güçlenir). year 0'dan.</summary>
        public static double[] RivalScores(ContestKind kind, int year)
        {
            var list = new double[Rivals];
            double baseScore = kind == ContestKind.HorseRace ? 55 : kind == ContestKind.BeautyContest ? 45 : 45;
            double growth = 1.0 + 0.06 * Math.Max(0, Math.Min(10, year));
            for (int i = 0; i < Rivals; i++) list[i] = baseScore * growth * (0.80 + 0.10 * i);   // en güçlüsü favori
            return list;
        }

        private static int Place(Random rng, double mine, double spreadMine, double[] rivals)
        {
            double me = mine * (1.0 + (rng.NextDouble() * 2 - 1) * spreadMine);
            int place = 1;
            foreach (var r in rivals) if (r * (1.0 + (rng.NextDouble() * 2 - 1) * RngSpread) > me) place++;
            return place;
        }

        /// <summary>Kazanma şansı tahmini (sabit tohumla, her çağrıda aynı) ve en çok %70'e çeken rakip düzeni.</summary>
        public static double EstimateWinChance(ContestKind kind, Animal a, Character handler, int year, out double[] rivals)
        {
            rivals = RivalScores(kind, year);
            double mine = Score(kind, a, handler);
            double spread = a.Lineage == AnimalLineage.Legendary ? LegendRngSpread : RngSpread;
            double chance = 0;
            for (int guard = 0; guard < 30; guard++)
            {
                var rng = new Random(9173 + year * 31 + (int)kind);
                int wins = 0;
                for (int i = 0; i < EstimateRuns; i++) if (Place(rng, mine, spread, rivals) == 1) wins++;
                chance = (double)wins / EstimateRuns;
                if (chance <= MaxWinChance) break;
                rivals[Rivals - 1] *= 1.05;   // favori rakip güçlenir
            }
            return Math.Min(MaxWinChance, chance);
        }

        /// <summary>
        /// Yarışmayı simüle eder. mainEvent: Yaz Panayırı / Hasat Şenliği ana yarışı (efsane unvanı yalnızca burada).
        /// Ödül, hayvan XP'si ve efsane unvanı uygulanır; akçe sonucu döner (hane kasasına oyun katmanı ekler).
        /// </summary>
        public static ContestResult Run(Random rng, ContestKind kind, Animal a, Character handler, int year, bool mainEvent)
        {
            if (Blocker(kind, a, handler) != null) return null;
            double chance = EstimateWinChance(kind, a, handler, year, out var rivals);
            double spread = a.Lineage == AnimalLineage.Legendary ? LegendRngSpread : RngSpread;
            int place = Place(rng, Score(kind, a, handler), spread, rivals);
            AfterRun?.Invoke(a);
            var r = new ContestResult { Kind = kind, Place = place, Entrants = Rivals + 1, WinChance = chance };
            if (place <= 3)
            {
                r.PrizeAkce = mainEvent ? Prizes[place - 1] : Prizes[place - 1] / 2;
                r.AnimalXp = XpByPlace[place - 1];
                AnimalProgress.AddXp(a, r.AnimalXp);
            }
            if (kind == ContestKind.HorseRace) handler.GainSkillXp(SkillType.Riding, place == 1 ? 30 : 10);
            if (place == 1 && mainEvent)
            {
                string title = TitleFor(kind, a);
                if (title != null && AnimalProgress.GrantLegend(a, title)) r.LegendTitle = title;
            }
            string what = kind == ContestKind.HorseRace ? "at yarışını" : kind == ContestKind.BeautyContest ? "güzellik yarışmasını"
                : kind == ContestKind.HoundRace ? "tazı koşusunu" : kind == ContestKind.CatShow ? "kedi güzellik yarışmasını" : "kütük çekmeyi";
            r.Summary = place == 1
                ? $"{a.Name}, {what} kazandı!" + (r.LegendTitle != null ? $" Artık bir efsane: {r.LegendTitle}." : "")
                : $"{a.Name}, {what} {place}. bitirdi.";
            string kindKey = "core.contest." + kind;
            r.SavedSummary = place != 1 ? SavedText.Encode("core.chronicle.contest_placed", ("name", a.Name), ("kind", SavedText.Key(kindKey)), ("place", place))
                : r.LegendTitle != null ? SavedText.Encode("core.chronicle.contest_won_legend", ("name", a.Name), ("kind", SavedText.Key(kindKey)), ("title", r.LegendTitle))
                : SavedText.Encode("core.chronicle.contest_won", ("name", a.Name), ("kind", SavedText.Key(kindKey)));
            return r;
        }

        private static string TitleFor(ContestKind kind, Animal a)
        {
            switch (kind)
            {
                case ContestKind.HorseRace: return "Panayır Rüzgârı";
                case ContestKind.BeautyContest: return "Hasat Güzeli";
                case ContestKind.LogPull: return "Demir Omuz";
                case ContestKind.HoundRace: return "Rüzgâr Tazısı";
                case ContestKind.CatShow: return "Panayır Güzeli";
                default: return AnimalProgress.TitleFor(a.Species.Id);
            }
        }

        /// <summary>Bahis (+18 paketi): kazanırsa yatırılanın (1 / şans) x 0,9 katı döner; şans en az %5 sayılır.</summary>
        public static int BetPayout(int stake, double winChance) => (int)Math.Floor(stake / Math.Max(0.05, winChance) * 0.9);
    }
}

namespace Thatchwick.Core.Animals
{
    /// <summary>
    /// Yarışmaya kayıt defteri (29-4.1): kayıtta giriş ücreti alınır, şenlik günü yarış simüle edilir; ödül kasaya,
    /// sonuç bildirime ve kroniğe gider. Yaz Panayırı: at yarışı (ana). Hasat Şenliği: güzellik ve kütük çekme (ana).
    /// İlkbahar şenliği: köy koşusu (unvan vermez).
    /// </summary>
    public sealed class ContestBook
    {
        public sealed class Entry { public ContestKind Kind; public int AnimalId, HandlerId; }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly Random _rng;
        public ContestBook(int seed = 151) { _rng = new Random(seed); }

        public IReadOnlyList<Entry> Entries => _entries;
        public event Action<ContestResult, Animal, Character> Finished;

        /// <summary>Bu türden yarışma hangi şenlikte (ana yarış mı).</summary>
        public static bool RunsAt(ContestKind kind, string festivalId, out bool mainEvent)
        {
            mainEvent = false;
            switch (kind)
            {
                case ContestKind.HorseRace:
                    if (festivalId == "festival.fair") { mainEvent = true; return true; }
                    return festivalId == "festival.seed";   // köy koşusu
                case ContestKind.CatShow:
                    mainEvent = festivalId == "festival.fair";
                    return mainEvent;
                default:
                    if (festivalId == "festival.harvest") { mainEvent = true; return true; }
                    return false;
            }
        }

        public bool IsRegistered(Animal a) { foreach (var e in _entries) if (e.AnimalId == a.Id) return true; return false; }

        /// <summary>Kayıt: engel yoksa giriş ücreti alınır. Hata metni ya da null.</summary>
        public string Register(ContestKind kind, Animal a, Character handler, Build.Household house)
        {
            var block = AnimalContests.Blocker(kind, a, handler);
            if (block != null) return block;
            if (IsRegistered(a)) return "Zaten kayıtlı.";
            if (house != null && !house.TrySpend(AnimalContests.EntryFee)) return "Giriş ücreti için akçe yetmiyor.";
            _entries.Add(new Entry { Kind = kind, AnimalId = a.Id, HandlerId = handler.Id });
            return null;
        }

        public void Cancel(Animal a) => _entries.RemoveAll(e => e.AnimalId == a.Id);

        /// <summary>Şenlik günü: o şenliğe ait kayıtları koşturur, ödülü kasaya ekler. Kayıt kalkar.</summary>
        public List<ContestResult> RunFestival(string festivalId, int year, Func<int, Animal> findAnimal, Func<int, Character> findCharacter, Build.Household house)
        {
            var results = new List<ContestResult>();
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var e = _entries[i];
                if (!RunsAt(e.Kind, festivalId, out bool main)) continue;
                _entries.RemoveAt(i);
                var a = findAnimal(e.AnimalId); var h = findCharacter(e.HandlerId);
                var r = a != null && h != null ? AnimalContests.Run(_rng, e.Kind, a, h, year, main) : null;
                if (r == null) continue;
                if (r.PrizeAkce > 0) house?.AddAkce(r.PrizeAkce);
                h.AddChronicle(r.SavedSummary ?? r.Summary);
                results.Add(r);
                Finished?.Invoke(r, a, h);
            }
            return results;
        }

        public string[] Export()
        {
            var l = new List<string>();
            foreach (var e in _entries) l.Add($"{(int)e.Kind}:{e.AnimalId}:{e.HandlerId}");
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _entries.Clear();
            if (data == null) return;
            foreach (var s in data)
            {
                var p = s?.Split(':');
                if (p == null || p.Length != 3) continue;
                if (int.TryParse(p[0], out int k) && int.TryParse(p[1], out int a) && int.TryParse(p[2], out int h) && Enum.IsDefined(typeof(ContestKind), k))
                    _entries.Add(new Entry { Kind = (ContestKind)k, AnimalId = a, HandlerId = h });
            }
        }
    }
}

namespace Thatchwick.Core.Animals
{
    /// <summary>Yarış kartındaki bir at: hanenin kayıtlı atı (AnimalId > 0) ya da köyün rakip atlarından biri.</summary>
    public sealed class RaceRunner
    {
        public string Name, Owner;
        public double Score, Spread, Chance;
        public int AnimalId;
        public bool Mine => AnimalId > 0;
    }

    /// <summary>Bir at yarışının kartı: koşacak atlar ve bahisçilerin biçtiği şanslar.</summary>
    public sealed class RaceCard
    {
        public string FestivalId;
        public long Day;
        public bool Main;
        public readonly List<RaceRunner> Runners = new List<RaceRunner>();
        internal string Key;
    }

    public sealed class RaceBet
    {
        public long Day;
        public string FestivalId, RunnerName;
        public int AnimalId, Stake;
        /// <summary>Bahsi yatıran hane üyesi (0: bilinmiyor; eski kayıt). Kaybederse eşi ona kırılır (67c §2).</summary>
        public int BettorId;
        /// <summary>Bahis yatırılırkenki şans; ödeme buna göre yapılır.</summary>
        public double Chance;
    }

    public sealed class BetOutcome
    {
        public RaceBet Bet;
        public string Winner;
        public bool Won;
        public int Payout;
    }

    /// <summary>
    /// At yarışı bahsi (+18 paketi; karar 20.11.2026): yarış arifesinde ve yarış sabahı karttan bir at seçilir, akçe yatırılır.
    /// Yarış şenlikte koşulunca kazanan at belli olur; tutan bahis <see cref="AnimalContests.BetPayout"/> kadar öder.
    /// Yarış başına tek bahis. Hanenin atı yarışıyorsa kartta o da vardır ve sonuç onun gerçek yarış sonucuyla aynıdır.
    /// </summary>
    public sealed class RaceBets
    {
        public static readonly int[] Stakes = { 5, 10, 25, 50 };
        public const int SimRuns = 2000;
        /// <summary>Köyün rakip atları, zayıftan favoriye (<see cref="AnimalContests.RivalScores"/> sırası).</summary>
        public static readonly string[] RivalHorses = { "Karayel", "Sarıkız", "Poyraz", "Kırçıl", "Yıldırım" };

        private readonly Random _rng;
        private RaceCard _card;

        public RaceBet Current { get; private set; }
        public event Action<BetOutcome> Settled;

        public RaceBets(int seed = 163) { _rng = new Random(seed); }

        /// <summary>Yarış kartı; bu şenlikte at yarışı yoksa null. Aynı gün ve aynı kayıtlar için aynı kart döner.</summary>
        public RaceCard Card(string festivalId, long day, int year, IReadOnlyList<(Animal animal, Character handler)> mine, IReadOnlyList<string> owners)
        {
            if (!ContestBook.RunsAt(ContestKind.HorseRace, festivalId, out bool main)) return null;
            string key = festivalId + ":" + day;
            if (mine != null) foreach (var m in mine) if (m.animal != null) key += ":" + m.animal.Id;
            if (_card != null && _card.Key == key) return _card;

            var card = new RaceCard { FestivalId = festivalId, Day = day, Main = main, Key = key };
            var rivals = AnimalContests.RivalScores(ContestKind.HorseRace, year);
            if (mine != null)
                foreach (var (a, h) in mine)
                {
                    if (a == null || h == null || AnimalContests.Blocker(ContestKind.HorseRace, a, h) != null) continue;
                    AnimalContests.EstimateWinChance(ContestKind.HorseRace, a, h, year, out var adjusted);   // favori rakip gerekirse güçlenir
                    for (int i = 0; i < rivals.Length; i++) rivals[i] = Math.Max(rivals[i], adjusted[i]);
                    card.Runners.Add(new RaceRunner
                    {
                        Name = a.Name, Owner = h.Name, AnimalId = a.Id, Score = AnimalContests.Score(ContestKind.HorseRace, a, h),
                        Spread = a.Lineage == AnimalLineage.Legendary ? AnimalContests.LegendRngSpread : AnimalContests.RngSpread
                    });
                }
            for (int i = 0; i < rivals.Length; i++)
                card.Runners.Add(new RaceRunner
                {
                    Name = RivalHorses[i % RivalHorses.Length], Owner = owners != null && owners.Count > 0 ? owners[i % owners.Count] : "",
                    Score = rivals[i], Spread = AnimalContests.RngSpread
                });

            var rng = new Random(9173 + (int)(day % 100000) * 31);   // sabit tohum: kart her açılışta aynı
            var wins = new int[card.Runners.Count];
            for (int run = 0; run < SimRuns; run++) wins[Fastest(rng, card, false)]++;
            for (int i = 0; i < wins.Length; i++) card.Runners[i].Chance = (double)wins[i] / SimRuns;
            _card = card;
            return card;
        }

        private static int Fastest(Random rng, RaceCard card, bool rivalsOnly)
        {
            int best = -1; double bestRun = double.MinValue;
            for (int i = 0; i < card.Runners.Count; i++)
            {
                var r = card.Runners[i];
                if (rivalsOnly && r.Mine) continue;
                double run = r.Score * (1.0 + (rng.NextDouble() * 2 - 1) * r.Spread);
                if (run > bestRun) { bestRun = run; best = i; }
            }
            return best;
        }

        /// <summary>Tutarsa ödenecek akçe.</summary>
        public static int PayoutFor(RaceRunner runner, int stake) => runner == null ? 0 : AnimalContests.BetPayout(stake, runner.Chance);

        /// <summary>Bahis yatırır; hata metni ya da boş (tamam).</summary>
        public LocText Place(RaceCard card, int runner, int stake, Build.Household house, int bettorId = 0)
        {
            if (!Thatchwick.Core.Content.ContentPacks.Adult) return LocText.Of("bet.err.pack");
            if (card == null || runner < 0 || runner >= card.Runners.Count) return LocText.Of("bet.err.no_race");
            if (Current != null) return LocText.Of("bet.err.already", ("horse", Current.RunnerName), ("stake", Current.Stake));
            if (Array.IndexOf(Stakes, stake) < 0) return LocText.Of("bet.err.stake");
            if (house == null || !house.TrySpend(stake)) return LocText.Of("bet.err.akce", ("stake", stake));
            var r = card.Runners[runner];
            Current = new RaceBet { Day = card.Day, FestivalId = card.FestivalId, RunnerName = r.Name, AnimalId = r.AnimalId, Stake = stake, Chance = r.Chance, BettorId = bettorId };
            return default;
        }

        /// <summary>
        /// Yarış koştu. winnerAnimalId: hanenin kazanan atı (kazanmadıysa 0); o zaman kazanan rakipler arasından çıkar.
        /// Bahis varsa sonuçlanır, tutan bahsin ödemesi kasaya girer. Kazanan atın adı döner.
        /// </summary>
        public string Resolve(RaceCard card, int winnerAnimalId, Build.Household house)
        {
            if (card == null || card.Runners.Count == 0) return null;
            RaceRunner winner = null;
            if (winnerAnimalId > 0) winner = card.Runners.Find(r => r.AnimalId == winnerAnimalId);
            if (winner == null)
            {
                int i = Fastest(_rng, card, rivalsOnly: true);
                if (i < 0) return null;
                winner = card.Runners[i];
            }
            _card = null;
            var bet = Current;
            if (bet == null || bet.Day != card.Day) return winner.Name;
            Current = null;
            bool won = bet.AnimalId == winner.AnimalId && bet.RunnerName == winner.Name;
            var outcome = new BetOutcome { Bet = bet, Winner = winner.Name, Won = won, Payout = won ? AnimalContests.BetPayout(bet.Stake, bet.Chance) : 0 };
            if (outcome.Payout > 0) house?.AddAkce(outcome.Payout);
            Settled?.Invoke(outcome);
            return winner.Name;
        }

        /// <summary>Yarışı koşulmadan günü geçen bahis (şenlik atlandıysa) iade edilir. İade edilen akçe döner.</summary>
        public int RefundStale(long today, Build.Household house)
        {
            if (Current == null || Current.Day >= today) return 0;
            int stake = Current.Stake;
            Current = null;
            house?.AddAkce(stake);
            return stake;
        }

        public string Export() => Current == null ? null
            : string.Join("|", Current.Day.ToString(System.Globalization.CultureInfo.InvariantCulture), Current.FestivalId, Current.AnimalId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Current.Stake.ToString(System.Globalization.CultureInfo.InvariantCulture), Current.Chance.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                Current.BettorId.ToString(System.Globalization.CultureInfo.InvariantCulture), Current.RunnerName);

        public void Import(string data)
        {
            Current = null; _card = null;
            // Eski kayıt 6 alan (bahisçi yok), yeni kayıt 7 alan: at adı her zaman sonda.
            var p = string.IsNullOrEmpty(data) ? null : data.Split('|');
            if (p == null || (p.Length != 6 && p.Length != 7)) return;
            int bettor = 0;
            if (p.Length == 7) int.TryParse(p[5], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out bettor);
            string runnerName = p[p.Length - 1];
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            if (long.TryParse(p[0], System.Globalization.NumberStyles.Integer, inv, out long day) && int.TryParse(p[2], System.Globalization.NumberStyles.Integer, inv, out int animal)
                && int.TryParse(p[3], System.Globalization.NumberStyles.Integer, inv, out int stake) && double.TryParse(p[4], System.Globalization.NumberStyles.Float, inv, out double chance) && stake > 0)
                Current = new RaceBet { Day = day, FestivalId = p[1], AnimalId = animal, Stake = stake, Chance = chance, RunnerName = runnerName, BettorId = bettor };
        }
    }
}
