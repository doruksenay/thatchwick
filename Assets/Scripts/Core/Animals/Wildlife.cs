using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Animals
{
    /// <summary>Yaban hayvanı grubu (Project belgesi 31).</summary>
    public enum WildGroup { Harmless = 0, Dangerous = 1 }

    /// <summary>Yaban türü: veriyle tanımlı; dünya varlığı (sprite, gezinme) ayrıca gelecek.</summary>
    public sealed class WildSpecies
    {
        public string Id, Name;
        public WildGroup Group;
        public string Habitat;           // çayır, orman kenarı, derin orman, göl, bahçe
        public Season[] Seasons;         // görülebildiği mevsimler (null = her mevsim)
        public bool Night;               // gece görülür
        public bool Pettable;            // "sakin yaklaş" ile sevilebilir
        public bool Huntable;            // yay ya da tuzakla avlanır (ayı hayır)
        public bool Trappable;           // tuzağa düşer
        public double Shyness;           // 0-1: sevme zorluğu
        public string Meat, Hide;        // av ürünleri (yoksa null)
        public bool Adoptable;           // öksüz yavrusu sahiplenilebilir
    }

    /// <summary>Yaban türleri kataloğu (1.0 listesi, belge 31-7.1).</summary>
    public static class WildCatalog
    {
        private static readonly Season[] SprSumAut = { Season.Spring, Season.Summer, Season.Autumn };
        private static readonly Season[] AutWin = { Season.Autumn, Season.Winter };

        public static readonly WildSpecies[] All =
        {
            new WildSpecies { Id = "wild.rabbit", Name = "Tavşan", Habitat = "çayır", Pettable = true, Huntable = true, Trappable = true, Shyness = 0.6, Meat = "item.game_meat", Hide = "item.hide" },
            new WildSpecies { Id = "wild.squirrel", Name = "Sincap", Habitat = "orman kenarı", Seasons = SprSumAut, Pettable = true, Shyness = 0.5 },
            new WildSpecies { Id = "wild.hedgehog", Name = "Kirpi", Habitat = "bahçe", Seasons = SprSumAut, Night = true, Shyness = 0.2, Adoptable = true },
            new WildSpecies { Id = "wild.roe_deer", Name = "Karaca", Habitat = "orman kenarı", Pettable = true, Huntable = true, Shyness = 0.9, Meat = "item.game_meat", Hide = "item.hide", Adoptable = true },
            new WildSpecies { Id = "wild.frog", Name = "Kurbağa", Habitat = "göl", Seasons = SprSumAut, Shyness = 0.3 },
            new WildSpecies { Id = "wild.stork", Name = "Leylek", Habitat = "çayır", Seasons = new[] { Season.Spring, Season.Summer }, Shyness = 0.7 },
            new WildSpecies { Id = "wild.crow", Name = "Karga", Habitat = "çayır", Shyness = 0.6, Adoptable = true },
            new WildSpecies { Id = "wild.owl", Name = "Baykuş", Habitat = "orman kenarı", Night = true, Shyness = 0.8 },
            new WildSpecies { Id = "wild.sparrow", Name = "Serçe", Habitat = "bahçe", Pettable = true, Shyness = 0.4 },
            new WildSpecies { Id = "wild.butterfly", Name = "Kelebek", Habitat = "bahçe", Seasons = new[] { Season.Spring, Season.Summer }, Shyness = 0.2 },
            new WildSpecies { Id = "wild.firefly", Name = "Ateş böceği", Habitat = "çayır", Seasons = new[] { Season.Summer }, Night = true, Shyness = 0.1 },
            new WildSpecies { Id = "wild.fox", Name = "Tilki", Group = WildGroup.Dangerous, Habitat = "orman kenarı", Night = true, Huntable = true, Trappable = true, Shyness = 0.8, Hide = "item.hide", Adoptable = true },
            new WildSpecies { Id = "wild.wolf", Name = "Kurt", Group = WildGroup.Dangerous, Habitat = "derin orman", Seasons = AutWin, Night = true, Huntable = true, Shyness = 1.0, Hide = "item.hide" },
            new WildSpecies { Id = "wild.boar", Name = "Yaban domuzu", Group = WildGroup.Dangerous, Habitat = "orman kenarı", Night = true, Huntable = true, Trappable = true, Shyness = 0.9, Meat = "item.game_meat", Hide = "item.hide" },
            new WildSpecies { Id = "wild.rat", Name = "Fare", Group = WildGroup.Dangerous, Habitat = "ambar", Shyness = 0.5 },
            new WildSpecies { Id = "wild.bear", Name = "Boz ayı", Group = WildGroup.Dangerous, Habitat = "derin orman", Seasons = SprSumAut, Shyness = 1.0 },
        };

        public static WildSpecies Get(string id)
        {
            foreach (var s in All) if (s.Id == id) return s;
            return null;
        }

        public static bool InSeason(WildSpecies s, Season season)
        {
            if (s.Seasons == null) return true;
            foreach (var x in s.Seasons) if (x == season) return true;
            return false;
        }
    }

    /// <summary>Doğa Defteri kayıtları (bayrak).</summary>
    [Flags]
    public enum JournalMark { None = 0, Seen = 1, Petted = 2, Hunted = 4, YoungSeen = 8, Adopted = 16 }

    /// <summary>Doğa Defteri (31-6): her tür için görüldü/sevildi/avlandı/yavrusu görüldü/sahiplenildi. Hanedanla nesilden nesile geçer.</summary>
    public sealed class NatureJournal
    {
        private readonly Dictionary<string, JournalMark> _marks = new Dictionary<string, JournalMark>(StringComparer.Ordinal);

        public event Action<WildSpecies, JournalMark> NewEntry;

        public JournalMark Get(string id) => _marks.TryGetValue(id, out var m) ? m : JournalMark.None;

        /// <summary>İşaret koyar; yeni bir işaretse true (ilk kez görmek vb.).</summary>
        public bool Mark(string id, JournalMark mark)
        {
            var s = WildCatalog.Get(id);
            if (s == null || mark == JournalMark.None) return false;
            var cur = Get(id);
            if ((cur & mark) == mark) return false;
            _marks[id] = cur | mark;
            NewEntry?.Invoke(s, mark);
            return true;
        }

        /// <summary>Görülen tür sayısı ve toplam.</summary>
        public int SeenCount { get { int n = 0; foreach (var kv in _marks) if ((kv.Value & JournalMark.Seen) != 0) n++; return n; } }
        public static int Total => WildCatalog.All.Length;

        /// <summary>Tamamlanma ödülü: görülen tür başına Toplayıcılık ve Avcılık XP'si +%0,5 (en çok +%8).</summary>
        public double XpBonus => Math.Min(0.08, 0.005 * SeenCount);

        public string[] Export()
        {
            var list = new List<string>();
            foreach (var kv in _marks) list.Add(kv.Key + ":" + (int)kv.Value);
            list.Sort(string.CompareOrdinal);
            return list.ToArray();
        }

        public void Import(string[] data)
        {
            _marks.Clear();
            if (data == null) return;
            foreach (var e in data)
            {
                if (string.IsNullOrEmpty(e)) continue;
                int i = e.LastIndexOf(':');
                if (i <= 0 || !int.TryParse(e.Substring(i + 1), out int v)) continue;
                string id = e.Substring(0, i);
                if (WildCatalog.Get(id) != null) _marks[id] = (JournalMark)v;
            }
        }
    }

    /// <summary>Kurt baskınının sonucu.</summary>
    public sealed class WolfRaidResult
    {
        public int Pack, Defense;
        public List<Animal> Killed = new List<Animal>();
        public List<Animal> InjuredDogs = new List<Animal>();
        public bool Repelled => Killed.Count == 0;
    }

    /// <summary>
    /// Kurt saldırısı (31-4.1). Sonbahar sonu ve kışın, dışarı salınmış sürüye gece gelir; 1-2 gün önce uyarı (uluma, iz).
    /// Kurt sayısı sürüyle büyür; köpekler (seviyeye göre 1-3), Hayvan görevli çoban (1, ateş varsa 2) ve çit (kurtların
    /// yarısını oyalar) savunur. Açık kalan her kurt bir hayvan öldürür (önce yavru, sonra yaşlı). Köpek ve insan ölmez.
    /// </summary>
    public sealed class WolfSystem
    {
        public const int MaxPack = 6;
        public const int HerdPerWolf = 6;
        public const double DailyRaidChance = 0.25;
        public const double DogInjuryChance = 0.25;
        public const double DogRestDays = 3.0;

        private readonly AnimalSystem _animals;
        private readonly Random _rng;

        public WolfSystem(AnimalSystem animals, int seed = 113)
        {
            _animals = animals;
            _rng = new Random(seed);
        }

        /// <summary>Hanedeki "Hayvanlar" görevli genç yetişkin ve üstü sayısı (çoban).</summary>
        public Func<int> Shepherds { get; set; }
        /// <summary>Otlakta ateş/meşale var mı (çoban +1).</summary>
        public Func<bool> HasFire { get; set; }
        /// <summary>Otlak çitle çevrili mi (kurtların yarısı oyalanır).</summary>
        public Func<bool> Fenced { get; set; }
        /// <summary>Ek savunma (can yoldaşı "Sürü Dili" vb.).</summary>
        public Func<int> ExtraDefense { get; set; }

        /// <summary>Uyarı verildi ve baskına kalan gün (0 = bu gece). -1 = tehdit yok.</summary>
        public int RaidInDays { get; private set; } = -1;
        /// <summary>Bu mevsim avlanan kurt: kurt sayısını mevsim boyunca 1 azaltır (en çok 3).</summary>
        public int HuntedThisSeason { get; private set; }

        public event Action<string> Warning;
        public event Action<WolfRaidResult> Raided;

        /// <summary>Kurt mevsimi mi: sonbaharın ikinci yarısı ya da kış.</summary>
        public static bool IsWolfSeason(Season s, int dayOfSeason, int daysPerSeason) =>
            s == Season.Winter || s == Season.Autumn && dayOfSeason >= daysPerSeason / 2;

        /// <summary>Dışarıdaki (salınmış) çiftlik hayvanları: köpek, kedi, at ve arı sayılmaz.</summary>
        public List<Animal> Exposed()
        {
            var list = new List<Animal>();
            if (!_animals.Released) return list;   // kapısı kapalı ahır güvenli
            foreach (var a in _animals.Animals)
            {
                if (a.IsDead || a.Species.IsPet || a.Species.Rideable || a.Species.Id == "animal.bees") continue;
                list.Add(a);
            }
            return list;
        }

        public int PackSize(int exposed, Season season)
        {
            if (exposed <= 0) return 0;
            int n = Math.Min(MaxPack, 1 + exposed / HerdPerWolf + (season == Season.Winter ? 1 : 0)) - Math.Min(3, HuntedThisSeason);
            return Math.Max(1, n);
        }

        /// <summary>Köpeğin karşıladığı kurt: seviye 1-2 → 1, 3-4 → 2, 5 → 3. Yavru ve dinlenen köpek savunmaz.</summary>
        public static int DogDefense(Animal dog)
        {
            if (dog == null || dog.IsDead || !dog.IsMature || dog.ShyDays > 0) return 0;
            // Genetik (belge 48): Doğuştan önder 3. seviyede kurt başı; Gece gözü ve yüksek Cesaret +1.
            int leader = Genetics.GeneticsService.LeaderLevel(dog.Genome);
            int baseDef = dog.Level >= leader ? 3 : dog.Level >= 3 ? 2 : 1;
            return baseDef + Genetics.GeneticsService.DogDefenseBonus(dog.Genome);
        }

        public List<Animal> Dogs()
        {
            var list = new List<Animal>();
            foreach (var a in _animals.Animals) if (!a.IsDead && a.Species.Id == "animal.dog") list.Add(a);
            return list;
        }

        /// <summary>Çit ve kapı dışındaki toplam savunma.</summary>
        public int DefenseScore()
        {
            int d = 0;
            foreach (var dog in Dogs()) d += DogDefense(dog);
            int shepherds = Shepherds != null ? Shepherds() : 0;
            d += shepherds;
            if (shepherds > 0 && HasFire != null && HasFire()) d += 1;
            if (ExtraDefense != null) d += ExtraDefense();
            return d;
        }

        /// <summary>Panel göstergesi: (gelecek kurt, savunma). Risk = kurt &gt; savunma (çit hesaba katılarak).</summary>
        public (int wolves, int defense) Risk(Season season)
        {
            int pack = PackSize(Exposed().Count, season);
            int effective = Fenced != null && Fenced() ? (pack + 1) / 2 : pack;
            return (effective, DefenseScore());
        }

        /// <summary>Her sabah: kurt mevsiminde tehdit yoksa %25 şansla 1-2 gün sonrasına baskın kurulur ve uyarı gelir; varsa geri sayılır.</summary>
        public WolfRaidResult OnNewDay(Season season, int dayOfSeason, int daysPerSeason)
        {
            if (RaidInDays > 0) { RaidInDays--; if (RaidInDays > 0) return null; }
            if (RaidInDays == 0) { RaidInDays = -1; return Raid(season); }
            if (!IsWolfSeason(season, dayOfSeason, daysPerSeason)) return null;
            if (_rng.NextDouble() >= DailyRaidChance) return null;
            RaidInDays = 1 + _rng.Next(2);
            Warning?.Invoke(RaidInDays == 1
                ? "Orman kenarında kurt izi görüldü; gece uluma duyuldu. Yarın gece sürüyü kapat ya da köpek ve çoban hazırla."
                : "Uzaktan kurt uluması duyuldu. Bir iki gün içinde sürüye yaklaşabilirler.");
            return null;
        }

        /// <summary>Baskın (gece). Sürü içerideyse kimse ölmez.</summary>
        public WolfRaidResult Raid(Season season)
        {
            var exposed = Exposed();
            var r = new WolfRaidResult { Pack = PackSize(exposed.Count, season), Defense = DefenseScore() };
            if (r.Pack == 0) { Raided?.Invoke(r); return r; }
            int attackers = Fenced != null && Fenced() ? (r.Pack + 1) / 2 : r.Pack;
            int open = Math.Max(0, attackers - r.Defense);
            // Önce yavru, sonra yaşlı.
            exposed.Sort((a, b) =>
            {
                if (a.IsMature != b.IsMature) return a.IsMature ? 1 : -1;
                return b.AgeDays.CompareTo(a.AgeDays);
            });
            for (int i = 0; i < open && i < exposed.Count; i++)
            {
                _animals.KillByPredator(exposed[i]);
                r.Killed.Add(exposed[i]);
            }
            if (r.Defense > 0)
                foreach (var dog in Dogs())
                    if (DogDefense(dog) > 0 && _rng.NextDouble() < DogInjuryChance * (AnimalSystem.HasGift(dog, Genetics.Gift.LongBreathDog) ? 0.5 : 1.0))
                    {
                        dog.ShyDays = DogRestDays;
                        r.InjuredDogs.Add(dog);
                    }
            // Efsane yolu "Kurt Kıran" (belge 48 §6.4): kovulan her kurt savunan köpeklere sayılır.
            if (r.Repelled && r.Defense > 0)
                foreach (var dog in Dogs())
                    if (DogDefense(dog) > 0) _animals.AddDeed(dog, Genetics.PetLegends.Wolves, r.Pack);
            Raided?.Invoke(r);
            return r;
        }

        /// <summary>Bir kurt avlandı: bu mevsim baskın kurt sayısı 1 azalır (en çok 3).</summary>
        public void OnWolfHunted() => HuntedThisSeason = Math.Min(3, HuntedThisSeason + 1);

        public void OnSeasonEnd() => HuntedThisSeason = 0;

        public void Restore(int raidInDays, int hunted) { RaidInDays = raidInDays < -1 ? -1 : raidInDays; HuntedThisSeason = Math.Max(0, Math.Min(3, hunted)); }
    }

    /// <summary>Avcılık (31-5): yay (aktif) ve tuzak (pasif). Ayı avlanamaz; yavrulu dişiyi avlamak kötü moodlet.</summary>
    public enum HuntMethod { Bow = 0, Trap = 1 }

    public sealed class HuntResult
    {
        public bool Success;
        public string SpeciesId, Meat, Hide;
        public int MeatCount, HideCount;
        public string Message;
    }

    public static class Hunting
    {
        /// <summary>Başarı şansı: yay %35 + Avcılık seviye başına %6 − ürkeklik x %20 (tazı +%15); tuzak %25 + %4/sv.</summary>
        public static double Chance(WildSpecies s, HuntMethod m, int huntingLevel, bool houndHelps, double extra = 0)
        {
            if (s == null || !s.Huntable) return 0;
            if (m == HuntMethod.Trap && !s.Trappable) return 0;
            double c = m == HuntMethod.Bow
                ? 0.35 + 0.06 * (huntingLevel - 1) - 0.20 * s.Shyness + (houndHelps ? 0.15 + extra : 0)
                : 0.25 + 0.04 * (huntingLevel - 1);
            return Math.Max(0.05, Math.Min(0.95, c));
        }

        public static HuntResult Try(Random rng, string speciesId, HuntMethod m, Character hunter, bool houndHelps = false, bool motherWithYoung = false, double houndExtra = 0)
        {
            var s = WildCatalog.Get(speciesId);
            var r = new HuntResult { SpeciesId = speciesId };
            if (s == null || !s.Huntable) { r.Message = s != null && s.Id == "wild.bear" ? "Ayı avlanmaz; uzak dur." : "Bu avlanmaz."; return r; }
            int level = hunter != null ? hunter.Skills.GetLevel(SkillType.Hunting) : 1;
            double chance = Chance(s, m, level, houndHelps, houndExtra);
            if (chance <= 0) { r.Message = "Bu yöntemle olmaz."; return r; }
            hunter?.GainSkillXp(SkillType.Hunting, 4);
            if (rng.NextDouble() >= chance) { r.Message = m == HuntMethod.Bow ? $"{s.Name} kaçtı." : "Tuzak boş."; return r; }
            r.Success = true;
            r.Meat = s.Meat; r.MeatCount = s.Meat != null ? (s.Id == "wild.rabbit" ? 1 : 3) : 0;
            r.Hide = s.Hide; r.HideCount = s.Hide != null ? 1 : 0;
            hunter?.GainSkillXp(SkillType.Hunting, 8);
            r.Message = $"{s.Name} avlandı.";
            if (motherWithYoung && hunter != null && hunter.MoodEnabled)
            {
                hunter.Mood.Add(new Thatchwick.Core.Mood.MoodletDef { Id = "mood.hunted_mother", DisplayName = "Vicdan azabı", Description = "Yavrulu bir anneyi vurdu.", Value = -8, DurationDays = 2 });
                r.Message += " Yavrusu ortada kaldı…";
            }
            return r;
        }
    }
}

namespace Thatchwick.Core.Animals
{
    /// <summary>
    /// Öksüz yaban yavrusu sahiplenme (31-2, 1.0): ilkbahar-yazda nadiren yaralı/öksüz bir yavru bulunur; oyuncu
    /// sahiplenir ya da bırakır. Bahçede kalan yavrunun etkisi: kirpi ekin zararlısı −%10, karga ara sıra parlak hediye,
    /// tilki yavrusu öteki tilkileri uzak tutar, karaca büyüyünce doğaya döner ve yılda bir ziyarete gelir.
    /// </summary>
    public sealed class WildAdoptions
    {
        public const double FindChancePerDay = 0.03;
        public const double CrowGiftChance = 0.10;
        private static readonly string[] Candidates = { "wild.hedgehog", "wild.crow", "wild.fox", "wild.roe_deer" };
        private readonly Random _rng;
        private readonly List<string> _adopted = new List<string>();
        private readonly Dictionary<string, int> _adoptedDay = new Dictionary<string, int>();
        private readonly List<string> _returned = new List<string>();

        /// <summary>Karaca bu kadar gün (≈ 2 yıl) sonra doğaya döner (belge 31-2).</summary>
        public const int RoeReturnDays = 4;
        /// <summary>Doğaya dönen karaca her ilkbahar bu şansla uğrar.</summary>
        public const double RoeVisitChance = 0.15;

        public WildAdoptions(int seed = 131) { _rng = new Random(seed); }

        /// <summary>Büyüyüp doğaya dönmüş yavrular (yalnızca karaca).</summary>
        public IReadOnlyList<string> Returned => _returned;
        public bool HasReturned(string speciesId) => _returned.Contains(speciesId);

        /// <summary>Bulunan ve karar bekleyen yavru (tür kimliği); yoksa null.</summary>
        public string Pending { get; private set; }
        public IReadOnlyList<string> Adopted => _adopted;

        public bool Has(string speciesId) => _adopted.Contains(speciesId);
        /// <summary>Ekin zararlısı azalması (kirpi).</summary>
        public double PestReduction => Has("wild.hedgehog") ? 0.10 : 0.0;
        /// <summary>Tilki yavrusu kümesi korur.</summary>
        public bool FoxGuard => Has("wild.fox");

        /// <summary>Her sabah: ilkbahar/yazda %3 şansla öksüz yavru bulunur (bekleyen yoksa). Bulunanı döner.</summary>
        public WildSpecies OnNewDay(Season season)
        {
            if (Pending != null || season == Season.Winter || season == Season.Autumn) return null;
            if (_rng.NextDouble() >= FindChancePerDay) return null;
            var options = new List<string>();
            foreach (var id in Candidates) if (!Has(id)) options.Add(id);
            if (options.Count == 0) return null;
            Pending = options[_rng.Next(options.Count)];
            return WildCatalog.Get(Pending);
        }

        /// <summary>Bekleyen yavruyu sahiplen (true) ya da doğaya bırak (false).</summary>
        public bool Decide(bool adopt, NatureJournal journal, int day = 0)
        {
            if (Pending == null) return false;
            journal?.Mark(Pending, JournalMark.Seen | JournalMark.YoungSeen | (adopt ? JournalMark.Adopted : JournalMark.None));
            if (adopt) { _adopted.Add(Pending); _adoptedDay[Pending] = day; }
            Pending = null;
            return true;
        }

        /// <summary>Karganın getirdiği parlak hediye (günde %10); yoksa false.</summary>
        public bool CrowGiftToday() => Has("wild.crow") && _rng.NextDouble() < CrowGiftChance;

        /// <summary>Günlük: büyüyen karaca doğaya döner ("returned" döner); dönmüş karaca ilkbaharda uğrayabilir ("visit").</summary>
        public string OnGrowUp(int day, Season season)
        {
            const string roe = "wild.roe_deer";
            if (Has(roe) && _adoptedDay.TryGetValue(roe, out int since) && day - since >= RoeReturnDays)
            {
                _adopted.Remove(roe); _adoptedDay.Remove(roe);
                if (!_returned.Contains(roe)) _returned.Add(roe);
                return "returned";
            }
            if (_returned.Contains(roe) && season == Season.Spring && _rng.NextDouble() < RoeVisitChance) return "visit";
            return null;
        }

        public string[] Export()
        {
            var l = new List<string>();
            foreach (var a in _adopted) l.Add(_adoptedDay.TryGetValue(a, out int d) ? a + "@" + d : a);
            foreach (var r in _returned) l.Add("^" + r);
            if (Pending != null) l.Add("?" + Pending);
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _adopted.Clear(); _adoptedDay.Clear(); _returned.Clear(); Pending = null;
            if (data == null) return;
            foreach (var raw in data)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                if (raw[0] == '?') { if (WildCatalog.Get(raw.Substring(1)) != null) Pending = raw.Substring(1); continue; }
                if (raw[0] == '^') { var r = raw.Substring(1); if (WildCatalog.Get(r) != null && !_returned.Contains(r)) _returned.Add(r); continue; }
                string e = raw; int day = 0;
                int at = raw.IndexOf('@');
                if (at > 0) { e = raw.Substring(0, at); int.TryParse(raw.Substring(at + 1), out day); }
                if (WildCatalog.Get(e) != null && !_adopted.Contains(e)) { _adopted.Add(e); _adoptedDay[e] = day; }
            }
        }
    }
}
