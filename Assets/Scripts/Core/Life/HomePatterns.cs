using System;
using System.Collections.Generic;
using System.Text;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Kök boya paletinin bir rengi (desen ve arma). Tincture: arma kuralı için "metal" ya da "colour"; null = armada yok.</summary>
    public sealed class DyeColor
    {
        public int Index;
        public string Name;
        public string Hex;
        public string[] Dyes;      // harcanan boyalar (boşsa bedava: doğal yün)
        public string Tincture;
        public string[] AltDyes;   // boya yerine geçebilen (çivit kalıbı)
    }

    /// <summary>Oyuncunun çizdiği 8x8 desen (kilim, işleme pano, sancak, örtü).</summary>
    public sealed class PatternDef
    {
        public string Id;
        public string Name;
        /// <summary>64 karakter: her biri palet sırası ('0'-'6').</summary>
        public string Cells;
    }

    /// <summary>Hanenin arması: alan, bölünüş, ikinci renk, şekil ve şekil rengi.</summary>
    public sealed class ArmaDef
    {
        public int Field;
        public string Division = "duz";
        public int Second = 2;
        public string Charge = "yok";
        public int ChargeColor = 2;

        public string Encode() => $"{Field}|{Division}|{Second}|{Charge}|{ChargeColor}";
        public static ArmaDef Decode(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var p = s.Split('|');
            if (p.Length < 5) return null;
            int.TryParse(p[0], out int f); int.TryParse(p[2], out int sc); int.TryParse(p[4], out int cc);
            return new ArmaDef { Field = f, Division = p[1], Second = sc, Charge = p[3], ChargeColor = cc };
        }
    }

    public sealed partial class HomeLife
    {
        public const int PatternSize = 8;
        public const int MaxPatterns = 12;
        public const string ArmaPatternId = "arma";
        public const string PatternableTag = "patternable";
        public const string HeraldicTag = "heraldic";
        public const int PatternBeautyBonus = 2, ArmaBeautyBonus = 3;
        public static readonly string[] PatternStations = { "item.loom", "item.embroidery_frame" };

        /// <summary>
        /// Kök boya paleti (dönem: kökboya, cehri, çivit, mazı karası; yeşil cehri üstüne çivitle). Doğal yün bedava.
        /// Arma renkleri (tincture): maden = altın (sarı) ve gümüş (doğal yün); renk = kırmızı, mavi, kara, yeşil. Aşı boyası armada yok.
        /// </summary>
        public static readonly DyeColor[] Palette =
        {
            new DyeColor { Index = 0, Name = "Doğal yün", Hex = "#E9DFC8", Dyes = new string[0], Tincture = "metal" },
            new DyeColor { Index = 1, Name = "Kökboya kırmızısı", Hex = "#A83A2C", Dyes = new[] { "item.dye_red" }, Tincture = "colour" },
            new DyeColor { Index = 2, Name = "Cehri sarısı", Hex = "#D9A934", Dyes = new[] { "item.dye_yellow" }, Tincture = "metal" },
            new DyeColor { Index = 3, Name = "Çivit mavisi", Hex = "#2F4E86", Dyes = new[] { "item.dye_blue" }, Tincture = "colour", AltDyes = new[] { "item.indigo" } },
            new DyeColor { Index = 4, Name = "Mazı karası", Hex = "#2A2522", Dyes = new[] { "item.oak_gall" }, Tincture = "colour" },
            new DyeColor { Index = 5, Name = "Yeşil (cehri + çivit)", Hex = "#4C7A3A", Dyes = new[] { "item.dye_yellow", "item.dye_blue" }, Tincture = "colour" },
            new DyeColor { Index = 6, Name = "Aşı boyası", Hex = "#B5703A", Dyes = new[] { "item.ochre" }, Tincture = null }
        };

        public static readonly (string id, string name)[] Divisions =
        {
            ("duz", "Düz"), ("dikine", "Dikine yarım"), ("enine", "Enine yarım"), ("dortlu", "Dörtlü"), ("bant", "Çapraz bant"), ("serit", "Kırık şerit")
        };

        public static readonly (string id, string name)[] Charges =
        {
            ("yok", "Şekilsiz"), ("yildiz", "Yıldız"), ("hac", "Haç"), ("basak", "Başak"), ("agac", "Ağaç"), ("balik", "Balık"),
            ("anahtar", "Anahtar"), ("kule", "Kule"), ("cark", "Çark"), ("nal", "Nal"), ("hilal", "Hilal"), ("ari", "Arı")
        };

        // 4x4 şekil maskeleri (satır satır, '#' dolu); 8x8 armanın ortasına (2..5) konur.
        private static readonly Dictionary<string, string> ChargeMasks = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "yildiz", ".#..####.##.#..#" }, { "hac", ".#..####.#...#.." }, { "basak", "#.#..#.##.#..#.." }, { "agac", ".##.####.##..##." },
            { "balik", "....#.#######.##" }, { "anahtar", "##..##...#...##." }, { "kule", "#.#.####.##..##." }, { "cark", ".##.#..##..#.##." },
            { "nal", "#..##..##..#.##." }, { "hilal", ".##.#...#....##." }, { "ari", "#..#.##.####.##." }
        };

        private readonly List<PatternDef> _patterns = new List<PatternDef>();
        private ArmaDef _arma;
        private int _patternSeq;

        public IReadOnlyList<PatternDef> Patterns => _patterns;
        public ArmaDef Arma => _arma;

        public PatternDef FindPattern(string id)
        {
            if (id == ArmaPatternId) return _arma == null ? null : new PatternDef { Id = ArmaPatternId, Name = "Hanedan arması", Cells = ArmaCells(_arma) };
            foreach (var p in _patterns) if (p.Id == id) return p;
            return null;
        }

        /// <summary>Desen çizmek için evde dokuma tezgâhı ya da nakış kasnağı gerekir.</summary>
        public string DesignBlocker()
        {
            foreach (var it in _world.Items) if (Array.IndexOf(PatternStations, it.Def.Id) >= 0) return null;
            return "Desen için evde dokuma tezgâhı ya da nakış kasnağı olmalı.";
        }

        public static bool ValidCells(string cells)
        {
            if (cells == null || cells.Length != PatternSize * PatternSize) return false;
            foreach (var ch in cells) if (ch < '0' || ch >= (char)('0' + Palette.Length)) return false;
            return true;
        }

        /// <summary>Deseni kitaplığa kaydeder (aynı kimlik varsa günceller). Kimlik ya da hata (Problem) döner.</summary>
        public string SavePattern(string name, string cells, string id = null)
        {
            if (DesignBlocker() is string why) { LastPatternProblem = why; return null; }
            if (!ValidCells(cells)) { LastPatternProblem = "Desen 8x8 ve paletteki renklerden olmalı."; return null; }
            if (string.IsNullOrWhiteSpace(name)) name = "Desen";
            var existing = id != null ? FindPattern(id) : null;
            if (existing != null && existing.Id != ArmaPatternId) { existing.Name = name.Trim(); existing.Cells = cells; LastPatternProblem = null; _world.NotifyChanged(); return existing.Id; }
            if (_patterns.Count >= MaxPatterns) { LastPatternProblem = $"Kitaplık dolu (en çok {MaxPatterns} desen)."; return null; }
            var p = new PatternDef { Id = "pat" + (++_patternSeq), Name = name.Trim(), Cells = cells };
            _patterns.Add(p);
            LastPatternProblem = null;
            return p.Id;
        }

        public string LastPatternProblem { get; private set; }

        public bool DeletePattern(string id)
        {
            for (int i = 0; i < _patterns.Count; i++)
                if (_patterns[i].Id == id)
                {
                    _patterns.RemoveAt(i);
                    foreach (var s in _items.Values) if (s.Pattern == id) s.Pattern = null;
                    _world.NotifyChanged();
                    return true;
                }
            return false;
        }

        /// <summary>Deseni bir eşyaya uygulamanın boya bedeli: kullanılan her boyalı renk için bir boya (yeşil iki boya).</summary>
        public static Dictionary<string, int> PatternCost(string cells)
        {
            var cost = new Dictionary<string, int>(StringComparer.Ordinal);
            if (!ValidCells(cells)) return cost;
            var used = new HashSet<int>();
            foreach (var ch in cells) used.Add(ch - '0');
            foreach (var idx in used)
                foreach (var dye in Palette[idx].Dyes)
                    cost[dye] = (cost.TryGetValue(dye, out var n) ? n : 0) + 1;
            return cost;
        }

        private int DyeCount(string dye)
        {
            int n = _house.Count(dye);
            foreach (var c in Palette)
                if (c.AltDyes != null && Array.IndexOf(c.Dyes, dye) >= 0) foreach (var alt in c.AltDyes) n += _house.Count(alt);
            return n;
        }

        private bool TakeDye(string dye)
        {
            if (_house.TryTakeItem(dye)) return true;
            foreach (var c in Palette)
                if (c.AltDyes != null && Array.IndexOf(c.Dyes, dye) >= 0)
                    foreach (var alt in c.AltDyes) if (_house.TryTakeItem(alt)) return true;
            return false;
        }

        public string ApplyPatternBlocker(ItemInstance item, string patternId)
        {
            if (item == null) return "Eşya yok.";
            if (!item.Def.HasTag(PatternableTag)) return $"{item.Def.DisplayName} desen almaz (kilim, örtü, pano, sancak alır).";
            var p = FindPattern(patternId);
            if (p == null) return patternId == ArmaPatternId ? "Önce armanı çiz." : "Desen yok.";
            if (patternId == ArmaPatternId && !item.Def.HasTag(HeraldicTag) && !item.Def.HasTag(PatternableTag)) return "Arma bu eşyaya işlenmez.";
            if (DesignBlocker() is string why) return why;
            foreach (var kv in PatternCost(p.Cells))
                if (DyeCount(kv.Key) < kv.Value) return $"{Name(kv.Key)} yetmiyor ({DyeCount(kv.Key)}/{kv.Value}).";
            return null;
        }

        public bool ApplyPattern(ItemInstance item, string patternId)
        {
            if (ApplyPatternBlocker(item, patternId) != null) return false;
            var p = FindPattern(patternId);
            foreach (var kv in PatternCost(p.Cells)) for (int i = 0; i < kv.Value; i++) TakeDye(kv.Key);
            Ensure(item.Id).Pattern = patternId;
            _world.NotifyChanged();
            Say($"{item.Def.DisplayName}: \"{p.Name}\" deseni işlendi.");
            return true;
        }

        public void ClearPattern(ItemInstance item)
        {
            var s = item != null ? StateOf(item.Id) : null;
            if (s == null || s.Pattern == null) return;
            s.Pattern = null;
            _world.NotifyChanged();
        }

        /// <summary>Eşyaya işlenmiş desen (8x8 hücre) ya da null: oyun katmanı çizer.</summary>
        public PatternDef PatternOn(ItemInstance item)
        {
            var s = item != null ? StateOf(item.Id) : null;
            return s?.Pattern != null ? FindPattern(s.Pattern) : null;
        }

        private double PatternBeauty(ItemInstance item)
        {
            var s = StateOf(item.Id);
            if (s?.Pattern == null || FindPattern(s.Pattern) == null) return 0;
            return s.Pattern == ArmaPatternId && item.Def.HasTag(HeraldicTag) ? ArmaBeautyBonus : PatternBeautyBonus;
        }

        public int PatternedItemCount()
        {
            int n = 0;
            foreach (var it in _world.Items) if (PatternOn(it) != null) n++;
            return n;
        }

        // ---- arma ----

        /// <summary>
        /// Arma kuralı (ortaçağ hanedan arması): renk üstüne renk, maden üstüne maden konmaz. Bölünmüş alanda şekil serbest;
        /// bant ve şerit alanın üstünde sayılır. Aşı boyası armada kullanılmaz. Uyuyorsa null.
        /// </summary>
        public static string ArmaRule(ArmaDef a)
        {
            if (a == null) return "Arma yok.";
            if (!Valid(a.Field) || !Valid(a.Second) || !Valid(a.ChargeColor)) return "Geçersiz renk.";
            foreach (var idx in new[] { a.Field, a.Second, a.ChargeColor })
                if (Palette[idx].Tincture == null) return $"{Palette[idx].Name} armada kullanılmaz (maden: sarı, doğal yün; renk: kırmızı, mavi, kara, yeşil).";
            bool divided = a.Division == "dikine" || a.Division == "enine" || a.Division == "dortlu";
            bool ordinary = a.Division == "bant" || a.Division == "serit";
            if (!divided && a.Division != "duz" && !ordinary) return "Bilinmeyen bölünüş.";
            if (divided && a.Field == a.Second) return "Bölünmüş armada iki yarı ayrı renk olmalı.";
            if (ordinary && Clash(a.Field, a.Second)) return RuleText;
            if (!divided && a.Charge != "yok" && Clash(a.Field, a.ChargeColor)) return RuleText;
            if (a.Charge != "yok" && !ChargeMasks.ContainsKey(a.Charge)) return "Bilinmeyen şekil.";
            return null;
        }

        private const string RuleText = "Renk üstüne renk, maden üstüne maden konmaz (sarı ya da doğal yün, renklerin üstüne; renk, madenin üstüne).";
        private static bool Valid(int i) => i >= 0 && i < Palette.Length;
        private static bool Clash(int a, int b) => Palette[a].Tincture == Palette[b].Tincture;

        public string SetArma(ArmaDef a)
        {
            var why = ArmaRule(a);
            if (why != null) return why;
            _arma = a;
            _world.NotifyChanged();
            return null;
        }

        /// <summary>Armanın 8x8 hücreleri.</summary>
        public static string ArmaCells(ArmaDef a)
        {
            var sb = new StringBuilder(64);
            for (int y = 0; y < PatternSize; y++)
                for (int x = 0; x < PatternSize; x++)
                {
                    int c = a.Field;
                    switch (a.Division)
                    {
                        case "dikine": if (x >= 4) c = a.Second; break;
                        case "enine": if (y >= 4) c = a.Second; break;
                        case "dortlu": if ((x >= 4) != (y >= 4)) c = a.Second; break;
                        case "bant": if (Math.Abs(x - y) <= 1) c = a.Second; break;
                        case "serit": if (y == 7 - Math.Abs(2 * x - 7) / 2 || y == 6 - Math.Abs(2 * x - 7) / 2) c = a.Second; break;
                    }
                    if (a.Charge != "yok" && ChargeMasks.TryGetValue(a.Charge, out var mask) && x >= 2 && x < 6 && y >= 2 && y < 6
                        && mask.Length == 16 && mask[(y - 2) * 4 + (x - 2)] == '#') c = a.ChargeColor;
                    sb.Append((char)('0' + c));
                }
            return sb.ToString();
        }

        public static string DivisionName(string id) { foreach (var d in Divisions) if (d.id == id) return d.name; return id; }
        public static string ChargeName(string id) { foreach (var d in Charges) if (d.id == id) return d.name; return id; }
    }
}
