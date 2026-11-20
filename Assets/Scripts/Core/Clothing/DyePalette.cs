using System;
using System.Collections.Generic;
using Thatchwick.Core.Customization;

namespace Thatchwick.Core.Clothing
{
    /// <summary>Renk tonu. Adlı karışım tonlarının tek tonu vardır (<see cref="Single"/>).</summary>
    public enum ClothTone { Single = 0, Light = 1, Mid = 2, Dark = 3 }

    /// <summary>Renk grubu (belge 48 §6.1): doğal, tek banyo, iki banyo, lüks, ağartma; karışımdan çıkan adlı tonlar (§6.1a).</summary>
    public enum DyeGroup { Natural = 0, SingleBath = 1, TwoBath = 2, Luxury = 3, Bleach = 4, Mixed = 5 }

    /// <summary>Koyun (yün) rengi (belge 48 §6.1a): genetik yok; yavru %50 annenin, %50 babanın rengini alır.</summary>
    public enum FleeceColor { White = 0, Grey = 1, Brown = 2, Black = 3 }

    /// <summary>Giysinin rengi: palet kimliği ve ton. Anahtar "madder_red.mid" ya da adlı tonda "oxblood".</summary>
    public readonly struct ClothColor : IEquatable<ClothColor>
    {
        public string Id { get; }
        public ClothTone Tone { get; }
        public ClothColor(string id, ClothTone tone) { Id = id; Tone = tone; }
        public string Key => Tone == ClothTone.Single ? Id : Id + "." + ToneKey(Tone);
        public bool IsEmpty => string.IsNullOrEmpty(Id);
        public static string ToneKey(ClothTone t) => t == ClothTone.Light ? "light" : t == ClothTone.Dark ? "dark" : "mid";

        public static bool TryParse(string key, out ClothColor c)
        {
            c = default;
            if (string.IsNullOrEmpty(key)) return false;
            int dot = key.LastIndexOf('.');
            if (dot < 0) { c = new ClothColor(key, ClothTone.Single); return DyePalette.Has(c); }
            string t = key.Substring(dot + 1);
            var tone = string.Equals(t, "light", StringComparison.Ordinal) ? ClothTone.Light
                : string.Equals(t, "dark", StringComparison.Ordinal) ? ClothTone.Dark
                : string.Equals(t, "mid", StringComparison.Ordinal) ? ClothTone.Mid : ClothTone.Single;
            if (tone == ClothTone.Single) return false;
            c = new ClothColor(key.Substring(0, dot), tone);
            return DyePalette.Has(c);
        }

        public bool Equals(ClothColor o) => string.Equals(Id, o.Id, StringComparison.Ordinal) && Tone == o.Tone;
        public override bool Equals(object obj) => obj is ClothColor o && Equals(o);
        public override int GetHashCode() => (Id ?? "").GetHashCode() * 7 + (int)Tone;
        public override string ToString() => Key;
    }

    /// <summary>Palet girdisi: 21 temel renk üç tonlu, karışım tonları tek tonlu.</summary>
    public sealed class DyeColorDef
    {
        public string Id;
        public DyeGroup Group;
        /// <summary>Açık, orta, koyu (#RRGGBB); adlı tonda üçü de aynı.</summary>
        public string Light, Mid, Dark;
        /// <summary>Bu rengi veren boya eşyası (doğal ve karışım tonlarında null).</summary>
        public string DyeItem;
        public bool Toned => Group != DyeGroup.Mixed;
        public string NameKey => "cloth.color." + Id;
        public string Hex(ClothTone t) => t == ClothTone.Light ? Light : t == ClothTone.Dark ? Dark : Mid;
    }

    /// <summary>
    /// Kumaş paleti ve boya karışımı (belge 48 §6.1-6.1a). Kural: <b>boya yalnızca koyulaştırır</b>. Boya yünün kendi
    /// renginin üstüne biner: ak yün boyanın gerçek rengini alır; gri, kahve ve kara yün tablodaki adlı tonlara gider
    /// (kara yün + kökboya = koyu bordo). Boyanmış giysinin rengi yeni tabandır (kırmızı giysi + çivit = koyu mor).
    /// Sonuç tabandan açık çıkamaz; çıkacaksa bir koyu tona ya da koyu sütuna geçilir, o da yoksa boya tutmaz.
    /// Ağartma yalnızca boyasız ak yün ve keten üstünde çalışır. Paletteki her renk appearance.json clothColors'ta da var
    /// (karakter yaratmada her renk serbest; rampalar tools/art/ramps.py build ile).
    /// </summary>
    public static class DyePalette
    {
        public const string WhiteWool = "white_wool", RawLinen = "raw_linen", GreyWool = "grey_wool", BrownWool = "brown_wool",
            BlackWool = "black_wool", BleachedLinen = "bleached_linen";
        public const string Madder = "madder_red", Weld = "weld_yellow", Woad = "woad_blue", Walnut = "walnut_brown", Gall = "gall_grey",
            Black = "dye_black", Ochre = "ochre_red", Lichen = "lichen_purple", Green = "green", Purple = "purple", Orange = "orange",
            Olive = "olive", Kermes = "kermes", Saffron = "saffron", TrueIndigo = "true_indigo";

        private static DyeColorDef T(string id, DyeGroup g, string l, string m, string d, string dye = null) =>
            new DyeColorDef { Id = id, Group = g, Light = l, Mid = m, Dark = d, DyeItem = dye };
        private static DyeColorDef N(string id, string hex) => new DyeColorDef { Id = id, Group = DyeGroup.Mixed, Light = hex, Mid = hex, Dark = hex };

        public static readonly DyeColorDef[] All =
        {
            // doğal (boyasız)
            T(WhiteWool, DyeGroup.Natural, "#EEE8D8", "#E2D9C2", "#CFC4A8"),
            T(RawLinen, DyeGroup.Natural, "#EFE7D2", "#E6DCC3", "#D2C4A2"),
            T(GreyWool, DyeGroup.Natural, "#C4C1BA", "#9E9A92", "#7A766F"),
            T(BrownWool, DyeGroup.Natural, "#8E6E50", "#6E5038", "#523A28"),
            T(BlackWool, DyeGroup.Natural, "#3E3A36", "#2E2B28", "#211F1D"),
            // tek banyo
            T(Woad, DyeGroup.SingleBath, "#7F9BB8", "#4F6F96", "#34507A", "item.dye_blue"),
            T(Madder, DyeGroup.SingleBath, "#C2776A", "#A8483A", "#7E2E26", "item.dye_red"),
            T(Weld, DyeGroup.SingleBath, "#E2CE7A", "#C9AE45", "#A48C2C", "item.dye_yellow"),
            T(Walnut, DyeGroup.SingleBath, "#A27E5A", "#7D5A3C", "#5C4029", "item.dye_brown"),
            T(Gall, DyeGroup.SingleBath, "#A6A39A", "#7F7C74", "#5E5B55", "item.dye_grey"),
            T(Black, DyeGroup.SingleBath, "#4A4643", "#343130", "#252322", "item.dye_black"),
            T(Ochre, DyeGroup.SingleBath, "#C48A62", "#A8673F", "#84492B", "item.dye_ochre"),
            T(Lichen, DyeGroup.SingleBath, "#A88AA8", "#86618A", "#634468", "item.dye_lichen"),
            // iki banyo
            T(Green, DyeGroup.TwoBath, "#8FA772", "#5F7F45", "#3F5C2E", "item.dye_green"),
            T(Purple, DyeGroup.TwoBath, "#8F6A96", "#6A3E6E", "#4A2A52", "item.dye_purple"),
            T(Orange, DyeGroup.TwoBath, "#D9975C", "#C2723A", "#985428", "item.dye_orange"),
            T(Olive, DyeGroup.TwoBath, "#A39E5E", "#7F7A3E", "#5E5A2C", "item.dye_olive"),
            // lüks
            T(Kermes, DyeGroup.Luxury, "#D0455A", "#B5203A", "#8A1428", "item.dye_crimson"),
            T(Saffron, DyeGroup.Luxury, "#EDC24A", "#E0A82A", "#BF8A1A", "item.dye_saffron"),
            T(TrueIndigo, DyeGroup.Luxury, "#3E4E80", "#2A3664", "#1A2248", "item.dye_navy"),
            // ağartma
            T(BleachedLinen, DyeGroup.Bleach, "#FAF6EC", "#F3EEE2", "#E8E1D2"),
            // adlı karışım tonları (§6.1a)
            N("rosewood", "#A0665E"), N("chestnut", "#6E3A2A"), N("oxblood", "#461A1A"),
            N("cherry", "#8A2A3A"), N("red_chestnut", "#7A2E22"), N("bordeaux", "#4E1220"),
            N("slate_blue", "#56677A"), N("dark_khaki", "#4E4E3A"), N("midnight", "#1E2538"),
            N("khaki", "#8C8660"), N("mustard", "#8A6E2A"), N("sage", "#7E8C6E"), N("forest_dark", "#1E2A1A"),
            N("smoke_purple", "#7A6A7E"), N("aubergine", "#4E3040"), N("plum_dark", "#2E1E2A"),
            N("brick", "#9E5A3E"), N("rust", "#7A4426"), N("soot_brown", "#2E2420"),
            N("earth", "#6A5642"), N("deep_brown", "#3E2C20"),
        };

        private static readonly Dictionary<string, DyeColorDef> ById = Index();
        private static Dictionary<string, DyeColorDef> Index()
        {
            var d = new Dictionary<string, DyeColorDef>(StringComparer.Ordinal);
            foreach (var c in All) d[c.Id] = c;
            return d;
        }

        public static DyeColorDef Get(string id) => id != null && ById.TryGetValue(id, out var d) ? d : null;
        public static bool Has(ClothColor c)
        {
            var d = Get(c.Id);
            return d != null && (d.Toned ? c.Tone != ClothTone.Single : c.Tone == ClothTone.Single);
        }

        /// <summary>Paletteki bütün renkler (21 x 3 ton + adlı tonlar).</summary>
        public static IEnumerable<ClothColor> Colors()
        {
            foreach (var d in All)
            {
                if (!d.Toned) { yield return new ClothColor(d.Id, ClothTone.Single); continue; }
                yield return new ClothColor(d.Id, ClothTone.Light);
                yield return new ClothColor(d.Id, ClothTone.Mid);
                yield return new ClothColor(d.Id, ClothTone.Dark);
            }
        }

        public static string Hex(ClothColor c) => Get(c.Id)?.Hex(c.Tone) ?? "#808080";
        public static Rgb ToRgb(ClothColor c) => Rgb.TryParseHex(Hex(c), out var rgb) ? rgb : new Rgb(128, 128, 128);

        /// <summary>Boya eşyasının verdiği renk; boya değilse null.</summary>
        public static DyeColorDef ForDyeItem(string itemId)
        {
            if (itemId == null) return null;
            foreach (var d in All) if (string.Equals(d.DyeItem, itemId, StringComparison.Ordinal)) return d;
            return null;
        }

        public static ClothColor Fleece(FleeceColor f)
        {
            switch (f)
            {
                case FleeceColor.Grey: return new ClothColor(GreyWool, ClothTone.Mid);
                case FleeceColor.Brown: return new ClothColor(BrownWool, ClothTone.Mid);
                case FleeceColor.Black: return new ClothColor(BlackWool, ClothTone.Mid);
                default: return new ClothColor(WhiteWool, ClothTone.Mid);
            }
        }

        /// <summary>Boya birimi: açık 1, orta 2, koyu 3 (koyu ton ikinci banyodur).</summary>
        public static int DyeUnits(ClothTone t) => t == ClothTone.Light ? 1 : t == ClothTone.Dark ? 3 : 2;

        // ---------------- karışım ----------------
        private enum Column { White = 0, Grey = 1, Brown = 2, Black = 3 }

        // boya → [ak, gri, kahve, kara] (belge 48 §6.1a tablosu); null = ak sütununda boyanın kendi rengi.
        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { Madder, new[] { null, "rosewood", "chestnut", "oxblood" } },
            { Kermes, new[] { null, "cherry", "red_chestnut", "bordeaux" } },
            { Woad, new[] { null, "slate_blue", "dark_khaki", "midnight" } },
            { Weld, new[] { null, "khaki", "mustard", BlackWool + ".mid" } },
            { Green, new[] { null, "sage", Olive + ".dark", "forest_dark" } },
            { Purple, new[] { null, "smoke_purple", "aubergine", "plum_dark" } },
            { Orange, new[] { null, "brick", "rust", "soot_brown" } },
            { Walnut, new[] { null, "earth", "deep_brown", BlackWool + ".mid" } },
            { Black, new[] { null, Black + ".dark", Black + ".dark", BlackWool + ".mid" } },
        };

        // boyanmış giysiye ikinci renk: iki banyo karışımı (koyu tonda)
        private static readonly (string a, string b, string result)[] Overdye =
        {
            (Madder, Woad, Purple), (Kermes, Woad, Purple), (Madder, TrueIndigo, Purple), (Kermes, TrueIndigo, Purple),
            (Woad, Weld, Green), (TrueIndigo, Weld, Green), (Woad, Saffron, Green),
            (Madder, Weld, Orange), (Madder, Saffron, Orange), (Kermes, Weld, Orange),
            (Weld, Gall, Olive), (Saffron, Gall, Olive),
        };

        private static Column? ColumnOf(ClothColor c)
        {
            switch (c.Id)
            {
                case WhiteWool: case RawLinen: case BleachedLinen: return Column.White;
                case GreyWool: case Gall: return Column.Grey;
                case BrownWool: case Walnut: return Column.Brown;
                case BlackWool: case Black: return Column.Black;
                default: return null;
            }
        }

        private static ClothColor ParseRef(string r, ClothTone tone)
        {
            if (r.IndexOf('.') >= 0 && ClothColor.TryParse(r, out var c)) return c;
            var d = Get(r);
            return d != null && d.Toned ? new ClothColor(r, tone) : new ClothColor(r, ClothTone.Single);
        }

        /// <summary>Göreli parlaklık (0-1, sRGB doğrusal).</summary>
        public static double Luminance(ClothColor c)
        {
            var rgb = ToRgb(c);
            return 0.2126 * Lin(rgb.R) + 0.7152 * Lin(rgb.G) + 0.0722 * Lin(rgb.B);
        }
        private static double Lin(byte v) { double x = v / 255.0; return x <= 0.04045 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4); }

        /// <summary>Taban renkte seçilebilen tonlar: ak tabanda üçü; gri ve kahvede orta ve koyu; karada tek ton.</summary>
        public static ClothTone[] TonesOn(ClothColor baseColor)
        {
            var col = ColumnOf(baseColor);
            if (col == Column.White) return new[] { ClothTone.Light, ClothTone.Mid, ClothTone.Dark };
            if (col == Column.Black) return new[] { ClothTone.Mid };
            return new[] { ClothTone.Mid, ClothTone.Dark };
        }

        /// <summary>
        /// Boyama sonucu: taban rengin üstüne boya (ton isteği). Sonuç tabandan açık değildir; boya tutmazsa taban döner
        /// (<paramref name="took"/> false).
        /// </summary>
        public static ClothColor Mix(ClothColor baseColor, string dyeId, ClothTone tone, out bool took)
        {
            took = false;
            var dye = Get(dyeId);
            if (dye == null || !dye.Toned || dye.Group == DyeGroup.Natural || dye.Group == DyeGroup.Bleach) return baseColor;
            if (tone == ClothTone.Single) tone = ClothTone.Mid;
            double baseLum = Luminance(baseColor);
            var candidates = new List<ClothColor>();
            var col = ColumnOf(baseColor);
            if (col.HasValue)
            {
                int start = (int)col.Value;
                // gri ve kahvede koyu ton bir koyu sütundur
                if (col.Value != Column.White && col.Value != Column.Black && tone == ClothTone.Dark) start++;
                if (col.Value == Column.White)
                {
                    for (int t = (int)tone; t <= (int)ClothTone.Dark; t++) candidates.Add(new ClothColor(dye.Id, (ClothTone)t));
                    start = 1;
                }
                if (Table.TryGetValue(dye.Id, out var row))
                    for (int i = Math.Max(1, start); i <= 3; i++) candidates.Add(ParseRef(row[i], ClothTone.Mid));
                else candidates.Add(Snap(Multiply(baseColor, new ClothColor(dye.Id, tone)), baseLum));
            }
            else
            {
                // boyanmış giysi: iki banyo karışımı ya da çarpımın en yakın koyu rengi
                string over = null;
                foreach (var o in Overdye)
                    if ((o.a == baseColor.Id && o.b == dye.Id) || (o.b == baseColor.Id && o.a == dye.Id)) { over = o.result; break; }
                if (over != null) candidates.Add(new ClothColor(over, ClothTone.Dark));
                else if (baseColor.Id == dye.Id && baseColor.Tone != ClothTone.Single && baseColor.Tone < ClothTone.Dark)
                    candidates.Add(new ClothColor(dye.Id, ClothTone.Dark));   // aynı boya: koyulaşır
                candidates.Add(Snap(Multiply(baseColor, new ClothColor(dye.Id, tone)), baseLum));
            }
            foreach (var c in candidates)
                if (!c.IsEmpty && Has(c) && Luminance(c) <= baseLum + 1e-9) { took = !c.Equals(baseColor); return c; }
            return baseColor;
        }

        public static ClothColor Mix(ClothColor baseColor, string dyeId, ClothTone tone) => Mix(baseColor, dyeId, tone, out _);

        private static Rgb Multiply(ClothColor a, ClothColor b)
        {
            var x = ToRgb(a); var y = ToRgb(b);
            return new Rgb((byte)(x.R * y.R / 255), (byte)(x.G * y.G / 255), (byte)(x.B * y.B / 255));
        }

        /// <summary>Hedefe en yakın, tabandan açık olmayan palet rengi.</summary>
        private static ClothColor Snap(Rgb target, double maxLum)
        {
            ClothColor best = default; double bd = double.MaxValue;
            foreach (var c in Colors())
            {
                if (Luminance(c) > maxLum + 1e-9) continue;
                var r = ToRgb(c);
                double d = Sq(r.R - target.R) + Sq(r.G - target.G) + Sq(r.B - target.B);
                if (d < bd) { bd = d; best = c; }
            }
            return best;
        }
        private static double Sq(int v) => (double)v * v;

        /// <summary>Ağartılabilir mi: yalnızca boyasız ak yün ve keten (bir giysi boyayla açılamaz).</summary>
        public static bool CanBleach(ClothColor c, string material) =>
            (c.Id == WhiteWool || c.Id == RawLinen || c.Id == BleachedLinen) &&
            (material == "linen" || material == "wool" || material == "fine_wool" || material == "knit" || material == "hemp");

        /// <summary>Ağartma: keten ak kerene, yün ak yünün açık tonuna döner.</summary>
        public static ClothColor Bleach(ClothColor c, string material)
        {
            if (!CanBleach(c, material)) return c;
            return material == "linen" || material == "hemp" ? new ClothColor(BleachedLinen, ClothTone.Mid) : new ClothColor(WhiteWool, ClothTone.Light);
        }

        /// <summary>Solma (şapsız boya, ya da yıpranmış giysi): bir ton açılır; açık ton ve adlı ton solmaz.</summary>
        public static ClothColor Fade(ClothColor c) =>
            c.Tone == ClothTone.Dark ? new ClothColor(c.Id, ClothTone.Mid) : c.Tone == ClothTone.Mid ? new ClothColor(c.Id, ClothTone.Light) : c;

        /// <summary>Malzemenin boyasız varsayılan rengi.</summary>
        public static ClothColor Natural(string material)
        {
            switch (material)
            {
                case "linen": case "hemp": case "flower": return new ClothColor(RawLinen, ClothTone.Mid);
                case "leather": case "fur": case "wood": case "straw": return new ClothColor(BrownWool, ClothTone.Mid);
                case "silver": return new ClothColor(GreyWool, ClothTone.Light);
                default: return new ClothColor(WhiteWool, ClothTone.Mid);
            }
        }
    }
}
