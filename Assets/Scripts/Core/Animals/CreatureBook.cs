using System;
using System.Collections.Generic;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Animals
{
    public enum CreatureKind { Insect = 0, Fish = 1 }

    /// <summary>Böcek ya da balık kaydı (eşya kimliğiyle; yakalanınca envantere o eşya gelir).</summary>
    public sealed class CreatureDef
    {
        public string Item;
        public CreatureKind Kind;
        public string Set;
        public bool Legendary;
    }

    /// <summary>Koleksiyon seti: 5 canlı + setin efsanevisi (Fields of Mistria düzeni, Project belgesi 48 §9.2).</summary>
    public sealed class CreatureSet
    {
        public string Id;
        public CreatureKind Kind;
        /// <summary>Setin mevsimi (sudakiler her mevsim: null).</summary>
        public Season? Season;
        public string[] Members;
        public string Legendary;
        public string NameKey => "book." + Id;
    }

    /// <summary>
    /// Böcek ve balık defteri (belge 48 §9): mevsim setleri, her setin efsanevisi. Böcekler cam kavanozla toplama yerlerinden
    /// (47-cam-bahçe), balıklar oltayla gelir. Set tamamlanınca itibar ve beceri XP'si; hepsi hanedanla kalır.
    /// </summary>
    public sealed class CreatureBook
    {
        public static readonly CreatureSet[] Sets =
        {
            new CreatureSet { Id = "insects_spring", Kind = CreatureKind.Insect, Season = Time.Season.Spring,
                Members = new[] { "item.viv_ladybird", "item.viv_aphid", "item.viv_snail", "item.viv_caterpillar", "item.bug_swallowtail" }, Legendary = "item.bug_peacock_moth" },
            new CreatureSet { Id = "insects_summer", Kind = CreatureKind.Insect, Season = Time.Season.Summer,
                Members = new[] { "item.viv_firefly", "item.viv_cricket", "item.bug_dragonfly", "item.bug_mantis", "item.bug_cicada" }, Legendary = "item.bug_stag_beetle" },
            new CreatureSet { Id = "insects_autumn", Kind = CreatureKind.Insect, Season = Time.Season.Autumn,
                Members = new[] { "item.locust", "item.bug_night_moth", "item.bug_dung_beetle", "item.viv_spider", "item.kermes" }, Legendary = "item.bug_rhino_beetle" },
            new CreatureSet { Id = "insects_winter", Kind = CreatureKind.Insect, Season = Time.Season.Winter,
                Members = new[] { "item.viv_woodlouse", "item.viv_ground_beetle", "item.bug_ant", "item.bug_woodworm", "item.viv_worm" }, Legendary = "item.bug_snow_fly" },
            new CreatureSet { Id = "insects_water", Kind = CreatureKind.Insect,
                Members = new[] { "item.viv_tadpole", "item.bug_diving_beetle", "item.bug_water_scorpion", "item.leech", "item.bug_pond_skater" }, Legendary = "item.bug_silver_water_beetle" },
            new CreatureSet { Id = "fish_still", Kind = CreatureKind.Fish,
                Members = new[] { "item.fish_carp", "item.fish_tench", "item.fish_bream", "item.fish_rudd", "item.fish_catfish" }, Legendary = "item.fish_catfish_giant" },
            new CreatureSet { Id = "fish_river", Kind = CreatureKind.Fish,
                Members = new[] { "item.fish_trout", "item.fish_perch", "item.fish_salmon", "item.fish_bullhead", "item.fish_crayfish" }, Legendary = "item.fish_sturgeon_old" },
            new CreatureSet { Id = "fish_reeds", Kind = CreatureKind.Fish,
                Members = new[] { "item.fish_eel", "item.fish_pike", "item.mussel", "item.viv_stickleback", "item.viv_minnow" }, Legendary = "item.fish_pike_old" },
        };

        /// <summary>Set dışı kayıtlar (deftere yazılır ama set saymaz): altın sazan (gölet mutasyonu), kelebek.</summary>
        public static readonly string[] Extras = { "item.fish_carp_gold", "item.viv_butterfly" };

        /// <summary>Set tamamlama ödülü: itibar ve yakalayana beceri XP'si.</summary>
        public const int SetReputation = 5;
        public const double SetSkillXp = 40;

        private readonly HashSet<string> _seen = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _done = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Yıl başına efsane balık (kimlik:yıl): her gölün efsanesi yılda bir (45 Balıkçılık 10).</summary>
        private readonly HashSet<string> _legendYear = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Set tamamlandı (set).</summary>
        public event Action<CreatureSet> SetCompleted;
        /// <summary>İlk kez kaydedildi (eşya, efsanevi mi).</summary>
        public event Action<string, bool> FirstRecorded;

        public static bool IsTracked(string item) => Find(item) != null || Array.IndexOf(Extras, item) >= 0;

        public static CreatureSet Find(string item)
        {
            foreach (var s in Sets)
            {
                if (s.Legendary == item) return s;
                foreach (var m in s.Members) if (m == item) return s;
            }
            return null;
        }

        public static bool IsLegendary(string item)
        {
            foreach (var s in Sets) if (s.Legendary == item) return true;
            return item == "item.fish_carp_gold";
        }

        public bool Has(string item) => _seen.Contains(item);
        public bool IsDone(CreatureSet set) => _done.Contains(set.Id);
        public int SeenCount => _seen.Count;
        public static int TotalCount { get { int n = Extras.Length; foreach (var s in Sets) n += s.Members.Length + 1; return n; } }

        /// <summary>Kayıt: yeni ise true. Set tamamlanınca olay.</summary>
        public bool Record(string item)
        {
            if (item == null || !IsTracked(item) || !_seen.Add(item)) return false;
            FirstRecorded?.Invoke(item, IsLegendary(item));
            var set = Find(item);
            if (set != null && !_done.Contains(set.Id) && Complete(set))
            {
                _done.Add(set.Id);
                SetCompleted?.Invoke(set);
            }
            return true;
        }

        public bool Complete(CreatureSet set)
        {
            foreach (var m in set.Members) if (!_seen.Contains(m)) return false;
            return _seen.Contains(set.Legendary);
        }

        /// <summary>Setteki ilerleme (yakalanan, toplam).</summary>
        public (int have, int total) Progress(CreatureSet set)
        {
            int n = 0;
            foreach (var m in set.Members) if (_seen.Contains(m)) n++;
            if (_seen.Contains(set.Legendary)) n++;
            return (n, set.Members.Length + 1);
        }

        /// <summary>Bu suyun efsane balığı bu yıl tutulmadıysa true (tutulduğunda işaretler).</summary>
        public bool TryClaimLegendFish(string item, int year)
        {
            return _legendYear.Add(item + ":" + year);
        }

        public bool LegendFishTaken(string item, int year) => _legendYear.Contains(item + ":" + year);

        public string[] Export()
        {
            var l = new List<string>();
            foreach (var s in _seen) l.Add("s|" + s);
            foreach (var d in _done) l.Add("d|" + d);
            foreach (var y in _legendYear) l.Add("y|" + y);
            l.Sort(StringComparer.Ordinal);
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _seen.Clear(); _done.Clear(); _legendYear.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 3) continue;
                string v = line.Substring(2);
                switch (line[0])
                {
                    case 's': _seen.Add(v); break;
                    case 'd': _done.Add(v); break;
                    case 'y': _legendYear.Add(v); break;
                }
            }
        }
    }

    /// <summary>
    /// Perhiz günleri (belge 48 §9.4): ilkbaharda ve kışta üçer gün; et talebi düşer, balık fiyatı x1,5, NPC istekleri balık ister.
    /// Kurgusal yerleşimin takvimi: dönemde oruç kuralları balığa talebi artırmış, gölet yetiştiriciliğini yaymıştı.
    /// </summary>
    public static class FastingCalendar
    {
        public const double FishFactor = 1.5, MeatFactor = 0.8;
        /// <summary>Mevsim içindeki günler (0'dan): ilkbahar 3-5, kış 6-8.</summary>
        public const int SpringStart = 3, WinterStart = 6, Length = 3;

        public static bool IsFasting(Season season, int dayOfSeason) =>
            (season == Season.Spring && dayOfSeason >= SpringStart && dayOfSeason < SpringStart + Length)
            || (season == Season.Winter && dayOfSeason >= WinterStart && dayOfSeason < WinterStart + Length);

        /// <summary>Yarın perhiz başlıyor mu (duyuru için).</summary>
        public static bool StartsTomorrow(Season season, int dayOfSeason) =>
            (season == Season.Spring && dayOfSeason == SpringStart - 1) || (season == Season.Winter && dayOfSeason == WinterStart - 1);

        /// <summary>Pazar fiyat çarpanı: balık x1,5, et x0,8 (yalnız perhizde).</summary>
        public static double PriceFactor(Content.ItemDef def, bool fasting)
        {
            if (!fasting || def == null) return 1.0;
            if (def.HasTag("fish")) return FishFactor;
            if (def.HasTag("meat")) return MeatFactor;
            return 1.0;
        }
    }

    /// <summary>
    /// Çekirge sürüsü (belge 48 §9.3): yazın kuru günlerde nadiren gelir; bir gün önce ufukta bulut görülür. Tarlada ekin yer;
    /// salınmış kümes hayvanları (en az 3 tavuk, ördek ya da kaz) hasarı yarıya indirir. Toplanan çekirge yem olur.
    /// </summary>
    public sealed class LocustSwarm
    {
        public const double Chance = 0.03;
        public const int DryDaysNeeded = 2, CropsMax = 4, PoultryNeeded = 3, LocustYield = 4;
        private readonly Random _rng;
        public LocustSwarm(int seed = 821) { _rng = new Random(seed); }

        public int DryDays { get; set; }
        /// <summary>Yarın sürü gelecek (uyarı verildi).</summary>
        public bool Incoming { get; set; }

        /// <summary>Gün başı. Dönen: 0 olay yok, 1 uyarı, 2 sürü geldi (yenen ekin sayısı eaten'da).</summary>
        public int OnNewDay(Season season, bool rainy, int crops, int releasedPoultry, out int eaten, bool force = false)
        {
            eaten = 0;
            DryDays = rainy ? 0 : DryDays + 1;
            if (Incoming)
            {
                Incoming = false;
                if (crops <= 0) return 0;
                int n = Math.Min(CropsMax, crops);
                if (releasedPoultry >= PoultryNeeded) n = (n + 1) / 2;
                eaten = n;
                return 2;
            }
            if (season != Season.Summer) return 0;
            if (!force && (DryDays < DryDaysNeeded || _rng.NextDouble() >= Chance)) return 0;
            Incoming = true;
            return 1;
        }
    }
}
