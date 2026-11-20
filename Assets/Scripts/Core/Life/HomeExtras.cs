using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Sergi rafında tamamlanan koleksiyon.</summary>
    public sealed class CollectionDef
    {
        public string Id;
        public string Name;
        public List<string> Items = new List<string>();
    }

    /// <summary>Ev yarışmasının sonucu (Yaz Panayırı).</summary>
    public sealed class HomeContestResult
    {
        public int Rank;
        public int Entrants;
        public int Score;
        public int BestNeighbor;
        public string Text;
    }

    public sealed partial class HomeLife
    {
        // ---- koleksiyonlar (belge 45 §9) ----
        public const string DisplayTag = "display";
        public const int CollectionBeautyBonus = 4;
        private readonly HashSet<string> _collectionsDone = new HashSet<string>(StringComparer.Ordinal);
        private List<CollectionDef> _collections;

        /// <summary>Koleksiyon tamamlandı (itibar ve hanedan geçmişi için).</summary>
        public event Action<CollectionDef> CollectionCompleted;

        public IReadOnlyList<CollectionDef> Collections
        {
            get
            {
                if (_collections != null) return _collections;
                _collections = new List<CollectionDef>();
                var relics = new CollectionDef { Id = "col.relics", Name = "Ocak buluntuları" };
                foreach (var d in _content.Items.All) if (d.HasTag("relic")) relics.Items.Add(d.Id);
                Add(relics);
                Add(new CollectionDef { Id = "col.shore", Name = "Göl kıyısı", Items = { "item.mussel_shell", "item.snail_shell", "item.river_pebble", "item.heron_feather" } });
                Add(new CollectionDef { Id = "col.herbs", Name = "Aktarın çekmecesi", Items = { "item.herb", "item.tea_mint", "item.tea_chamomile", "item.tea_sage" } });
                return _collections;

                void Add(CollectionDef c)
                {
                    if (c.Items.Count < 2) return;
                    foreach (var id in c.Items) if (!_content.Items.Contains(id)) return;
                    _collections.Add(c);
                }
            }
        }

        public bool IsCollectionDone(string id) => _collectionsDone.Contains(id);
        public int CollectionsDone => _collectionsDone.Count;

        /// <summary>Sergi raflarında duran eşyalar (her biri bir kez).</summary>
        public HashSet<string> Displayed()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var it in _world.Items)
                if (it.Def.HasTag(DisplayTag)) foreach (var c in it.Contents) set.Add(c.ItemId);
            return set;
        }

        public (int have, int total) Progress(CollectionDef c)
        {
            var shown = Displayed();
            int have = 0;
            foreach (var id in c.Items) if (shown.Contains(id)) have++;
            return (have, c.Items.Count);
        }

        private void CheckCollections()
        {
            var shown = Displayed();
            foreach (var c in Collections)
            {
                if (_collectionsDone.Contains(c.Id)) continue;
                bool all = true;
                foreach (var id in c.Items) if (!shown.Contains(id)) { all = false; break; }
                if (!all) continue;
                _collectionsDone.Add(c.Id);
                Say($"Koleksiyon tamamlandı: {c.Name}. Sergi rafı göz alıyor; köyde konuşulur.");
                CollectionCompleted?.Invoke(c);
            }
        }

        /// <summary>Sergi rafı tam bir koleksiyonu taşıyorsa güzellik eki.</summary>
        private double CollectionBeauty(ItemInstance item)
        {
            if (!item.Def.HasTag(DisplayTag) || !item.HasContents) return 0;
            var here = new HashSet<string>(StringComparer.Ordinal);
            foreach (var c in item.Contents) here.Add(c.ItemId);
            foreach (var c in Collections)
            {
                bool all = true;
                foreach (var id in c.Items) if (!here.Contains(id)) { all = false; break; }
                if (all) return CollectionBeautyBonus;
            }
            return 0;
        }

        // ---- güvercinlik ----
        public const string DovecoteTag = "dovecote";

        /// <summary>Güvercinlik her sabah gübre verir (ilkbahar ve yaz iki); güvercin gübresi bahçede çok değerliydi.</summary>
        private void Dovecotes()
        {
            int n = 0;
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.HasTag(DovecoteTag)) n++;
            if (n == 0 || !_content.Items.Contains("item.manure")) return;
            int per = _clock.Season == Time.Season.Spring || _clock.Season == Time.Season.Summer ? 2 : 1;
            _house.AddItem("item.manure", n * per);
        }

        // ---- hizmetkâr (belge 45 §2) ----
        public const int ServantWageTeen = 1, ServantWageAdult = 2;   // denge (belge 45 ek): ortaçağ hizmetkârı çoğunlukla sofra ve küçük ücretle çalışırdı
        public const double ServantStartLoyalty = 50, ServantLeaveBelow = 20;
        public const int ServantBaseChores = 3;

        /// <summary>Köylünün işi var mı (dükkân sahibi, usta): hizmete girmez. Null ise kimse meşgul değil.</summary>
        public Func<Character, bool> IsBusy { get; set; }
        /// <summary>Hizmetkâr ile hane üyesi arasındaki yakınlığı değiştirir (FamilyService); null ise yok.</summary>
        public Action<int, int, int> AddAffection { get; set; }
        /// <summary>Kişi bulucu (hizmetkâr kimliğinden); null ise Members'tan aranır.</summary>
        public Func<int, Character> FindPerson { get; set; }

        /// <summary>
        /// Hizmetkâr dünyada yürür (oyun katmanı bağlar): sabah hizmet saatlerinde Ev görevine geçer, işleri gerçekten
        /// yapar, öğlen evine döner. Kapalıysa (testler, eski davranış) sabah işleri anında yapılır.
        /// </summary>
        public bool ServantWalks { get; set; }
        /// <summary>Hizmet saatleri (gün kesri): 07:00-12:00.</summary>
        public const double ServiceStart = 0.29, ServiceEnd = 0.5;
        public bool InService => ServantId != 0 && ServantWalks && _clock.DayFraction >= ServiceStart && _clock.DayFraction < ServiceEnd;
        private readonly Dictionary<string, int> _servantDone = new Dictionary<string, int>(StringComparer.Ordinal);
        private bool _wasInService;

        /// <summary>Yürüyen hizmetkârın görevini saate göre açar ve kapatır; hizmet bitince raporu yazar.</summary>
        private void ServantTick()
        {
            if (!ServantWalks || ServantId == 0) return;
            var s = Servant;
            if (s == null || s.IsDead || !s.IsVillager) return;
            bool on = InService;
            var want = on ? DutyKind.House : DutyKind.None;
            if (s.Duty != want) s.Duty = want;
            if (_wasInService && !on)
            {
                LastServantReport = _servantDone.Count == 0 ? $"{s.Name} geldi; yapılacak iş yoktu." : $"{s.Name} sabah işleri: {Summary(_servantDone)}.";
                _servantDone.Clear();
            }
            _wasInService = on;
        }

        private static string Summary(Dictionary<string, int> done)
        {
            var parts = new List<string>();
            foreach (var kv in done) parts.Add($"{kv.Value} {kv.Key}");
            return string.Join(", ", parts);
        }

        /// <summary>Hizmetkârın yaptığı iş (yürüyen hizmetkâr; OnApplied çağırır).</summary>
        private void NoteServantWork(Character c, InteractionDef i)
        {
            if (c == null || c.Id != ServantId || ServantId == 0) return;
            string key = ChoreName(i);
            _servantDone[key] = (_servantDone.TryGetValue(key, out var n) ? n : 0) + 1;
        }

        public int ServantId { get; private set; }
        public int ServantWage { get; private set; }
        public double ServantLoyalty { get; private set; }
        public int ServantDays { get; private set; }
        public string LastServantReport { get; private set; }

        public Character Servant
        {
            get
            {
                if (ServantId == 0) return null;
                if (FindPerson != null) return FindPerson(ServantId);
                if (Members != null) foreach (var c in Members()) if (c.Id == ServantId) return c;
                return null;
            }
        }

        public static bool CanServe(Character c) =>
            c != null && !c.IsDead && c.IsVillager && (c.Stage == AgeStage.Teen || c.Stage == AgeStage.YoungAdult);

        /// <summary>Hizmete girebilecek köylüler: ergen ve genç yetişkin, işi olmayan.</summary>
        public List<Character> ServantCandidates()
        {
            var list = new List<Character>();
            if (Members == null) return list;
            foreach (var c in Members())
                if (CanServe(c) && c.Id != ServantId && (IsBusy == null || !IsBusy(c))) list.Add(c);
            return list;
        }

        public static int WageFor(Character c) => c != null && c.Stage == AgeStage.Teen ? ServantWageTeen : ServantWageAdult;

        public string Hire(Character c)
        {
            if (!CanServe(c)) return "Yalnızca köyün ergen ve genç yetişkinleri hizmete girer.";
            if (IsBusy != null && IsBusy(c)) return $"{c.Name}nin kendi işi var.";
            if (ServantId != 0) Dismiss(quiet: true);
            ServantId = c.Id;
            ServantWage = WageFor(c);
            ServantLoyalty = ServantStartLoyalty;
            ServantDays = 0;
            Say($"{c.Name} hizmete girdi: sabahları gelip ev işlerini görür, günde {ServantWage} akçe.", c.Id);
            return null;
        }

        public void Dismiss(bool quiet = false)
        {
            if (ServantId == 0) return;
            var s = Servant;
            if (!quiet && s != null) Say($"{s.Name} hizmetten ayrıldı.", s.Id);
            if (s != null && s.Duty == DutyKind.House) s.Duty = DutyKind.None;
            ServantId = 0; ServantWage = 0; ServantLoyalty = 0; ServantDays = 0;
        }

        private void ServantDay()
        {
            if (ServantId == 0) return;
            var s = Servant;
            if (s == null || s.IsDead) { Dismiss(quiet: true); return; }
            if (!s.IsVillager) { Say($"{s.Name} artık aileden; hizmetkârlık bitti.", s.Id); Dismiss(quiet: true); return; }
            if (s.Stage != AgeStage.Teen && s.Stage != AgeStage.YoungAdult) { Say($"{s.Name} kendi hanesini kurmaya gitti; hizmet bitti.", s.Id); Dismiss(quiet: true); return; }
            ServantWage = WageFor(s);
            if (_house.TrySpend(ServantWage)) ServantLoyalty = Math.Min(100, ServantLoyalty + 2);
            else
            {
                ServantLoyalty -= 15;
                Say($"{s.Name}nin ücreti ödenemedi ({ServantWage} akçe).", s.Id);
                if (ServantLoyalty < ServantLeaveBelow) { Say($"{s.Name} ücretsiz çalışmaya razı olmadı; hizmetten ayrıldı.", s.Id); Dismiss(quiet: true); return; }
            }
            ServantDays++;
            if (ServantWalks)
            {
                // Yürüyen hizmetkâr işleri sabah kendisi yapar (ServantTick); burada yalnızca ücret ve yakınlık.
                if (AddAffection != null && Members != null)
                    foreach (var m in Members())
                        if (!m.IsDead && !m.IsVillager && m.Id != s.Id) AddAffection(s.Id, m.Id, 1);
                return;
            }

            // Sabah işleri: sadakat arttıkça daha çok iş. Yükseltme ve zanaat hizmetkârın işi değil.
            int budget = ServantBaseChores + (int)(ServantLoyalty / 25);
            var done = new Dictionary<string, int>(StringComparer.Ordinal);
            if (Effects != null)
                foreach (var (item, i) in Chores())
                {
                    if (budget <= 0) break;
                    if (i.HomeAction == HomeActions.Upgrade || i.IsCraft) continue;
                    if (_world.GetItem(item.Id) == null) continue;
                    if (Effects.CanStart(s, item, i) != null) continue;
                    Effects.Apply(s, item, i);
                    budget--;
                    string key = ChoreName(i);
                    done[key] = (done.TryGetValue(key, out var n) ? n : 0) + 1;
                }
            if (done.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kv in done) parts.Add($"{kv.Value} {kv.Key}");
                LastServantReport = $"{s.Name} sabah işleri: {string.Join(", ", parts)}.";
            }
            else LastServantReport = $"{s.Name} geldi; yapılacak iş yoktu.";

            // Aynı evde günler geçtikçe aile ile yakınlık (talip olabilir).
            if (AddAffection != null && Members != null)
                foreach (var m in Members())
                    if (!m.IsDead && !m.IsVillager && m.Id != s.Id) AddAffection(s.Id, m.Id, 1);
        }

        private static string ChoreName(InteractionDef i)
        {
            if (i.CollectContents) return "toplama";
            if (i.FillDecor) return "doldurma";
            if (i.Polish) return "cila";
            switch (i.HomeAction)
            {
                case HomeActions.Candle: return "mum";
                case HomeActions.Strew: return "ot serpme";
                case HomeActions.Whitewash: return "badana";
                case HomeActions.WaterPot: return "sulama";
                case HomeActions.HarvestPot: return "ot hasadı";
                case HomeActions.Stow: return "solan süs";
                case HomeActions.Mist: return "cam bahçe nemi";
            }
            return "iş";
        }

        // ---- ev yarışması (Yaz Panayırı) ve talip ziyareti ----
        public const string ContestFestival = "festival.fair";
        /// <summary>
        /// Komşu evlerin puanı: taban + yıl başına artış + rastgele (0..yayılım), en çok tavan. Denge ölçümü (EvBakimiDengeTests):
        /// sade ev 0, özenli Zanaatkâr evi ~40, zengin Asil evi ~120. Özenli ev ilk yıl birinciliği zorlar, sonra köy de güzelleşir;
        /// geç oyunda yalnızca zengin ev kazanır.
        /// </summary>
        public const int NeighborBase = 20, NeighborPerYear = 8, NeighborSpread = 20, NeighborCap = 110;
        public const string HomeRibbon = "item.home_ribbon";
        private readonly List<long> _contestYears = new List<long>();
        public int ContestWins { get; private set; }
        public HomeContestResult LastContest { get; private set; }
        /// <summary>Köydeki öbür hane sayısı (yarışmacı); null ise 4.</summary>
        public Func<int> NeighborCount { get; set; }
        /// <summary>Hatıra verme (kurdele); null ise verilmez.</summary>
        public Func<string, string, KeepsakeRecord> GrantKeepsake { get; set; }
        public event Action<HomeContestResult> ContestJudged;
        /// <summary>Puan hesabında oda güzelliği (MoodService); LifeSystems bağlar.</summary>
        public Func<Room, RoomBeauty> BeautyOf { get; set; }

        /// <summary>Hanenin ev puanı: en güzel üç iç oda + bahçe süsü (en çok 20) + koleksiyon ×3 + desenli eşya (en çok 5) + arma 2.</summary>
        public int HomeScore()
        {
            var scores = new List<int>();
            if (BeautyOf != null)
                foreach (var room in _world.Map.Rooms.Rooms)
                    if (room.IsIndoor && room.Tiles.Count > 0 && _world.Map.IsOwned(room.Tiles[0])) scores.Add(Math.Max(0, BeautyOf(room).Score));
            scores.Sort((a, b) => b.CompareTo(a));
            int score = 0;
            for (int i = 0; i < scores.Count && i < 3; i++) score += scores[i];
            score += GardenBeauty();
            score += 3 * _collectionsDone.Count;
            score += Math.Min(5, PatternedItemCount());
            if (_arma != null) score += 2;
            return score;
        }

        /// <summary>Arsadaki dış süslerin güzelliği (çiçek tarhı, çimen sıra, havuz), en çok 20.</summary>
        public int GardenBeauty()
        {
            double sum = 0;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || it.Def.Beauty <= 0 || !_world.Map.IsOwned(it.Position)) continue;
                var room = RoomOf(it);
                if (room != null && room.IsIndoor) continue;
                sum += it.Def.Beauty;
            }
            return Math.Min(20, (int)Math.Round(sum));
        }

        /// <summary>Panayırda köy büyükleri evleri gezer. Yılda bir kez; komşu puanları yılla artar (köy de güzelleşir).</summary>
        public HomeContestResult JudgeContest(int? seedOverride = null)
        {
            long year = _clock.YearIndex;
            if (_contestYears.Contains(year)) return null;
            _contestYears.Add(year);
            int neighbors = Math.Max(2, NeighborCount?.Invoke() ?? 4);
            var rng = new Random(seedOverride ?? (int)(year * 7919 + 13));
            int best = 0;
            var all = new List<int>();
            for (int i = 0; i < neighbors; i++)
            {
                int s = Math.Min(NeighborCap, NeighborBase + (int)year * NeighborPerYear + rng.Next(0, NeighborSpread + 1));
                all.Add(s);
                best = Math.Max(best, s);
            }
            int mine = HomeScore();
            int rank = 1;
            foreach (var s in all) if (s > mine) rank++;
            var r = new HomeContestResult { Rank = rank, Entrants = neighbors + 1, Score = mine, BestNeighbor = best };
            if (rank == 1)
            {
                ContestWins++;
                r.Text = $"Panayırda köy büyükleri evleri gezdi: en güzel ev bizimki ({mine} puan)! Kurdele duvara asılacak.";
                GrantKeepsake?.Invoke(HomeRibbon, $"En güzel ev kurdelesi ({year + 1}. yıl)");
            }
            else if (rank == 2) r.Text = $"Panayırda ev yarışması: ikinci olduk ({mine} puan; birinci {best}).";
            else r.Text = $"Panayırda ev yarışması: {r.Entrants} ev içinde {rank}. olduk ({mine} puan; birinci {best}). Odalara biraz daha özen.";
            LastContest = r;
            Say(r.Text);
            ContestJudged?.Invoke(r);
            return r;
        }

        /// <summary>
        /// Talip ya da konuk evi gezer, en güzel odayı yorumlar. Güzel ev (basamak 2+) hanenin bekâr yetişkinlerine
        /// yakınlık verir. Yorum metni döner (yoksa null).
        /// </summary>
        public string GuestVisit(Character guest)
        {
            if (guest == null || BeautyOf == null) return null;
            int bestTier = 0; Room bestRoom = null;
            foreach (var room in _world.Map.Rooms.Rooms)
            {
                if (!room.IsIndoor || room.Tiles.Count == 0 || !_world.Map.IsOwned(room.Tiles[0])) continue;
                int tier = BeautyOf(room).Tier;
                if (bestRoom == null || tier > bestTier) { bestTier = tier; bestRoom = room; }
            }
            if (bestRoom == null) return null;
            string where = RoomKindName(Classify(bestRoom).kind).ToLowerInvariant();
            string text;
            switch (bestTier)
            {
                case 3: text = $"{guest.Name} evi gezdi: \"Böyle {where} köyde yok, göz alıcı!\""; break;
                case 2: text = $"{guest.Name} evi gezdi: \"{Capital(where)} ne güzel döşenmiş.\""; break;
                case 1: text = $"{guest.Name} evi gezdi: \"Hoş bir ev.\""; break;
                default: text = $"{guest.Name} evi gezdi, bir şey demedi."; break;
            }
            if (bestTier >= 2 && AddAffection != null && Members != null)
                foreach (var m in Members())
                    if (!m.IsDead && !m.IsVillager && SuitorPool.IsSingleAdult(m)) AddAffection(guest.Id, m.Id, bestTier == 3 ? 8 : 4);
            Say(text, guest.Id);
            return text;
        }

        private static string Capital(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
    }
}
