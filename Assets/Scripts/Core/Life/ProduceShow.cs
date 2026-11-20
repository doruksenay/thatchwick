using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Life
{
    public enum ProduceShowKind
    {
        /// <summary>Tadım: hane tek bir ürün sunar, ürün tadılır (harcanır).</summary>
        Tasting = 0,
        /// <summary>Sergi: hane en çok <see cref="ProduceShowService.ExhibitSlots"/> ürün dizer; çeşitlilik puan getirir, ürünler eve döner.</summary>
        Exhibit = 1
    }

    /// <summary>Şenlik ürün yarışması tanımı (Project belgesi 57).</summary>
    public sealed class ShowDef
    {
        public string Id;
        public string FestivalId;
        public ProduceShowKind Kind;
        public Func<ItemDef, bool> Accepts;
        /// <summary>Ad: ui anahtarı "show.&lt;id&gt;".</summary>
        public LocText Name => LocText.Of("show." + Id);
    }

    public sealed class ShowResult
    {
        public ShowDef Show;
        /// <summary>1 = birinci; 0 = katılmadı.</summary>
        public int Rank;
        public int Entrants;
        public double Score;
        public readonly List<ItemKey> Entries = new List<ItemKey>();
        /// <summary>Sıralama: (hane adı, puan); oyuncu hanesi adı boş.</summary>
        public readonly List<KeyValuePair<string, double>> Board = new List<KeyValuePair<string, double>>();
        public int Akce, Reputation;
        public int Year;
    }

    /// <summary>
    /// Şenlik ürün yarışmaları (Project belgesi 57; kalite belgesi 53 §4.5): her şenlikte bir tadım, Hasat Şenliği'nde büyük
    /// sergi. Puan kaliteden ve ürünün değerinden gelir; ustalık eseri ve yadigâr çok güçlüdür. Rakip köy haneleri yıla göre
    /// güçlenir; her iki tarafa ±%15 şans. Hane kendiliğinden en iyi ürününü sunar; oyuncu başka bir ürün seçebilir ya da
    /// katılmayabilir. Yarışmanın kendisi simülasyondur (ekran yok); sonuç bildirim, kurdele hatırası, akçe ve itibar.
    /// </summary>
    public sealed class ProduceShowService
    {
        public const int ExhibitSlots = 9;
        public const int RivalCount = 4;
        public const string TastingRibbon = "item.show_ribbon", ExhibitRosette = "item.show_rosette";
        /// <summary>Ödül (birinci, ikinci, üçüncü): akçe ve itibar.</summary>
        public static readonly int[] TastingAkce = { 15, 6, 3 }, TastingRep = { 4, 2, 0 };
        public static readonly int[] ExhibitAkce = { 40, 15, 6 }, ExhibitRep = { 8, 4, 2 };
        /// <summary>Sergide her farklı ürün sınıfı (tarla, hayvan, saklama, yemek, içecek) için ek puan.</summary>
        public const double VarietyBonus = 8;

        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly Random _rng;
        private readonly Dictionary<string, string> _chosen = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _withdrawn = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _wins = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Yıl (0'dan); rakipler buna göre güçlenir.</summary>
        public Func<int> Year { get; set; } = () => 0;
        /// <summary>Rakip hane adları (köy haneleri); yoksa yer tutucu adlar.</summary>
        public Func<IEnumerable<string>> RivalNames { get; set; }

        public ProduceShowService(ContentDatabase content, Household house, int seed = 57)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _rng = new Random(seed);
        }

        // ---- tanımlar ----

        private static bool Has(ItemDef d, params string[] parts)
        {
            foreach (var p in parts) if (d.Id.IndexOf(p, StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static bool Edible(ItemDef d) => d.HasQuality && !d.IsRetired && (d.HasTag("food") || d.HasTag("drink"));
        private static bool Alcohol(ItemDef d) => ContentPacks.IsHiddenAlcohol(d.Id);

        public static readonly ShowDef[] Shows =
        {
            // Tohum Şenliği: kıştan kalan saklama (turşu, kuru meyve, reçel, pekmez değil).
            new ShowDef { Id = "preserves", FestivalId = "festival.seed", Kind = ProduceShowKind.Tasting,
                Accepts = d => Edible(d) && (d.HasTag("preserved") || d.HasTag("salted")) && !Has(d, "cheese", "pekmez") },
            // Yaz Panayırı: peynir ve bal.
            new ShowDef { Id = "dairy", FestivalId = "festival.fair", Kind = ProduceShowKind.Tasting,
                Accepts = d => Edible(d) && Has(d, "cheese", "butter", "yogurt", "kasar", "honey") },
            // Bağbozumu: bağın ürünü (şarap +18 paketiyle).
            new ShowDef { Id = "grape", FestivalId = "festival.vintage", Kind = ProduceShowKind.Tasting,
                Accepts = d => Edible(d) && !Alcohol(d) && (d.Id == "item.wine_new" || d.Id == "item.pekmez" || d.Id == "item.must" || d.Id == "item.vinegar") },
            // Hasat Şenliği: büyük sergi.
            new ShowDef { Id = "exhibit", FestivalId = "festival.harvest", Kind = ProduceShowKind.Exhibit,
                Accepts = d => Edible(d) && !Alcohol(d) },
            // Kış Gecesi: pişmiş yemek (aşçının işi).
            new ShowDef { Id = "feast", FestivalId = "festival.winter_night", Kind = ProduceShowKind.Tasting,
                Accepts = d => Edible(d) && d.HasTag("cooked") },
        };

        public static ShowDef ForFestival(string festivalId)
        {
            foreach (var s in Shows) if (s.FestivalId == festivalId) return s;
            return null;
        }

        public static ShowDef Get(string id)
        {
            foreach (var s in Shows) if (s.Id == id) return s;
            return null;
        }

        /// <summary>Sergi sınıfı: içecek, yemek, saklama, hayvan ürünü, tarla ürünü.</summary>
        public static string Category(ItemDef d)
        {
            if (d.HasTag("drink")) return "drink";
            if (d.HasTag("cooked")) return "cooked";
            if (d.HasTag("preserved") || d.HasTag("salted")) return "preserved";
            if (d.HasTag("fish") || Has(d, "cheese", "milk", "egg", "butter", "honey", "yogurt", "kasar", "ayran", "meat", "poultry")) return "animal";
            return "produce";
        }

        // ---- puan ----

        private static readonly double[] QualityPoints = { 10, 18, 28, 45, 60 };

        /// <summary>Ürün puanı: kalite (10 / 18 / 28 / 45 / 60) + değer (fiyatın üçte biri, en çok 10).</summary>
        public static double ItemScore(ItemDef d, int quality) =>
            QualityPoints[ItemQuality.Clamp(quality)] + Math.Min(10.0, (d?.Price ?? 0) / 3.0);

        public double EntryScore(ShowDef show, IReadOnlyList<ItemKey> entries)
        {
            double sum = 0;
            var cats = new HashSet<string>(StringComparer.Ordinal);
            foreach (var k in entries)
            {
                if (!_content.Items.TryGet(k.Id, out var d)) continue;
                sum += ItemScore(d, k.Quality);
                cats.Add(Category(d));
            }
            if (show.Kind == ProduceShowKind.Exhibit) sum += VarietyBonus * cats.Count;
            return sum;
        }

        /// <summary>
        /// Rakiplerin beklenen puanı: yıl başına güçlenir, 10. yılda durur. Tadımda tek ürün (18'den 30'a: Sıradan ürün ancak ilk
        /// yıllarda şanslıysa kazanır, Has orta oyunda yarı yarıya, ustalık eseri hep önde); sergide dokuz ürün ve çeşitlilik (140'tan 240'a).
        /// </summary>
        public const int RivalGrowthYears = 10;
        public static double RivalBase(ProduceShowKind kind, int year)
        {
            int y = Math.Max(0, Math.Min(RivalGrowthYears, year));
            return kind == ProduceShowKind.Exhibit ? 140 + 10 * y : 18 + 1.2 * y;
        }

        private static readonly double[] RivalSpread = { 0.75, 0.9, 1.0, 1.15 };

        // ---- aday ve seçim ----

        /// <summary>Hanenin bu yarışmaya sunabileceği ürünler: eşya başına en iyi yığın, puana göre sıralı.</summary>
        public List<ItemKey> Candidates(ShowDef show)
        {
            var list = new List<ItemKey>();
            if (show == null) return list;
            foreach (var kv in _house.InventoryList())
            {
                if (!_content.Items.TryGet(kv.Key, out var d) || !show.Accepts(d)) continue;
                ItemKey best = default; bool any = false;
                foreach (var st in _house.Stacks(kv.Key))
                {
                    if (st.Key.Condition == ItemCondition.Damaged) continue;
                    if (!any || st.Key.Quality > best.Quality) { best = st.Key; any = true; }
                }
                if (any) list.Add(best);
            }
            list.Sort((a, b) =>
            {
                double sa = ItemScore(_content.Items.Get(a.Id), a.Quality), sb = ItemScore(_content.Items.Get(b.Id), b.Quality);
                return sa != sb ? sb.CompareTo(sa) : string.CompareOrdinal(a.Id, b.Id);
            });
            return list;
        }

        public bool Participating(string showId) => !_withdrawn.Contains(showId);
        public void SetParticipating(string showId, bool on) { if (on) _withdrawn.Remove(showId); else _withdrawn.Add(showId); }

        /// <summary>Oyuncunun seçtiği tadım ürünü (yoksa en iyisi kendiliğinden).</summary>
        public void Choose(string showId, ItemKey key) { if (key.Id == null) _chosen.Remove(showId); else _chosen[showId] = key.Token(); }

        public int Wins(string showId) => _wins.TryGetValue(showId, out int n) ? n : 0;

        /// <summary>Şenlikte sunulacaklar. Tadım: seçilen (hâlâ varsa) ya da en iyi aday. Sergi: önce her sınıftan en iyisi, sonra en yüksek puanlılar.</summary>
        public List<ItemKey> PlannedEntries(ShowDef show)
        {
            var result = new List<ItemKey>();
            if (show == null || !Participating(show.Id)) return result;
            var candidates = Candidates(show);
            if (candidates.Count == 0) return result;
            if (show.Kind == ProduceShowKind.Tasting)
            {
                if (_chosen.TryGetValue(show.Id, out var token))
                {
                    var key = ItemKey.FromToken(token);
                    foreach (var st in _house.Stacks(key.Id))
                        if (st.Key.Equals(key)) { result.Add(key); return result; }
                    foreach (var c in candidates) if (c.Id == key.Id) { result.Add(c); return result; }   // aynı ürünün en iyisi
                }
                result.Add(candidates[0]);
                return result;
            }
            var used = new HashSet<string>(StringComparer.Ordinal);
            var cats = new HashSet<string>(StringComparer.Ordinal);
            foreach (var c in candidates)
            {
                string cat = Category(_content.Items.Get(c.Id));
                if (cats.Add(cat)) { result.Add(c); used.Add(c.Id); }
            }
            foreach (var c in candidates)
            {
                if (result.Count >= ExhibitSlots) break;
                if (used.Add(c.Id)) result.Add(c);
            }
            if (result.Count > ExhibitSlots) result.RemoveRange(ExhibitSlots, result.Count - ExhibitSlots);
            return result;
        }

        // ---- yarışma ----

        /// <summary>Şenliğin yarışmasını koşar; şenliğin yarışması yoksa null. Tadımda sunulan ürün tadılır (harcanır); sergi eve döner.</summary>
        public ShowResult Run(string festivalId, int? seedOverride = null)
        {
            var show = ForFestival(festivalId);
            if (show == null) return null;
            var rng = seedOverride.HasValue ? new Random(seedOverride.Value) : _rng;
            int year = Year?.Invoke() ?? 0;
            var result = new ShowResult { Show = show, Year = year };
            var entries = PlannedEntries(show);
            if (entries.Count == 0) return result;   // katılmadı ya da sunacak ürün yok
            result.Entries.AddRange(entries);
            double mine = EntryScore(show, entries) * (0.85 + 0.3 * rng.NextDouble());
            result.Score = mine;
            var names = new List<string>();
            if (RivalNames != null) foreach (var n in RivalNames()) if (!string.IsNullOrEmpty(n) && names.Count < RivalCount) names.Add(n);
            while (names.Count < RivalCount) names.Add("");
            double baseScore = RivalBase(show.Kind, year);
            var board = new List<KeyValuePair<string, double>> { new KeyValuePair<string, double>(null, mine) };
            for (int i = 0; i < RivalCount; i++)
                board.Add(new KeyValuePair<string, double>(names[i], baseScore * RivalSpread[i] * (0.85 + 0.3 * rng.NextDouble())));
            board.Sort((a, b) => b.Value.CompareTo(a.Value));
            result.Board.AddRange(board);
            result.Entrants = board.Count;
            for (int i = 0; i < board.Count; i++) if (board[i].Key == null) { result.Rank = i + 1; break; }

            if (show.Kind == ProduceShowKind.Tasting) _house.TryTakeKey(entries[0]);   // jüri tadar
            int idx = result.Rank - 1;
            var akce = show.Kind == ProduceShowKind.Exhibit ? ExhibitAkce : TastingAkce;
            var rep = show.Kind == ProduceShowKind.Exhibit ? ExhibitRep : TastingRep;
            if (idx >= 0 && idx < akce.Length)
            {
                result.Akce = akce[idx];
                result.Reputation = rep[idx];
                if (result.Akce > 0) _house.AddAkce(result.Akce);
            }
            if (result.Rank == 1) _wins[show.Id] = Wins(show.Id) + 1;
            return result;
        }

        // ---- kayıt ----

        public ProduceShowSave Export()
        {
            var s = new ProduceShowSave { withdrawn = new List<string>(_withdrawn).ToArray() };
            var ck = new List<string>(); var cv = new List<string>();
            foreach (var kv in _chosen) { ck.Add(kv.Key); cv.Add(kv.Value); }
            s.chosenShows = ck.ToArray(); s.chosenItems = cv.ToArray();
            var wk = new List<string>(); var wv = new List<int>();
            foreach (var kv in _wins) { wk.Add(kv.Key); wv.Add(kv.Value); }
            s.winShows = wk.ToArray(); s.winCounts = wv.ToArray();
            return s;
        }

        public void Import(ProduceShowSave s)
        {
            _chosen.Clear(); _withdrawn.Clear(); _wins.Clear();
            if (s == null) return;
            if (s.withdrawn != null) foreach (var w in s.withdrawn) if (w != null) _withdrawn.Add(w);
            if (s.chosenShows != null)
                for (int i = 0; i < s.chosenShows.Length && i < (s.chosenItems?.Length ?? 0); i++)
                    if (s.chosenShows[i] != null && s.chosenItems[i] != null) _chosen[s.chosenShows[i]] = s.chosenItems[i];
            if (s.winShows != null)
                for (int i = 0; i < s.winShows.Length && i < (s.winCounts?.Length ?? 0); i++)
                    if (s.winShows[i] != null) _wins[s.winShows[i]] = Math.Max(0, s.winCounts[i]);
        }
    }

    [Serializable]
    public sealed class ProduceShowSave
    {
        public string[] withdrawn, chosenShows, chosenItems, winShows;
        public int[] winCounts;
    }
}
