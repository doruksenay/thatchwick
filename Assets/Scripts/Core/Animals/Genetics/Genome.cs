using System;
using System.Collections.Generic;
using System.Globalization;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>
    /// Bir hayvanın genomu (Project belgesi 48 §3): tip, görünüm alelleri, istatistik genleri, hünerler, kusurlar.
    /// Seviye ve eğitim genoma yazılmaz; yavruya yalnız doğuştan değerler geçer (§3.2).
    /// </summary>
    public sealed class Genome
    {
        /// <summary>X'e bağlı lokusta erkeğin ikinci aleli.</summary>
        public const string YAllele = "Y";

        public string Species { get; internal set; }
        public string TypeId { get; internal set; }
        /// <summary>lokus → (anneden, babadan).</summary>
        public Dictionary<string, (string a, string b)> Loci { get; } = new Dictionary<string, (string a, string b)>(StringComparer.Ordinal);
        public Dictionary<string, int> Stats { get; } = new Dictionary<string, int>(StringComparer.Ordinal);
        public List<string> Gifts { get; } = new List<string>();
        public List<string> Defects { get; } = new List<string>();
        /// <summary>Mutasyonla gelen nadir renk (yoksa null).</summary>
        public string RareCoat { get; internal set; }
        /// <summary>Soy Defteri'nde açığa çıkmış taşıyıcılık: "lokus:alel" (yavrusu doğunca açılır).</summary>
        public HashSet<string> Revealed { get; } = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Kısır (katır, bardo, erkek üç renkli kedi).</summary>
        public bool Sterile { get; internal set; }
        /// <summary>Akrabalık katsayısı (Wright, 0-1).</summary>
        public double Inbreeding { get; internal set; }

        public BreedTypeDef Type => GeneticsCatalog.Type(TypeId);

        public int Stat(string id) => Stats.TryGetValue(id, out var v) ? v : 5;
        public bool HasGift(string id) => Gifts.Contains(id);
        public bool HasDefect(string id) => Defects.Contains(id);

        public bool Has(string locus, string allele)
        {
            if (!Loci.TryGetValue(locus, out var p)) return false;
            return string.Equals(p.a, allele, StringComparison.Ordinal) || string.Equals(p.b, allele, StringComparison.Ordinal);
        }

        public int Count(string locus, string allele)
        {
            if (!Loci.TryGetValue(locus, out var p)) return 0;
            int n = 0;
            if (string.Equals(p.a, allele, StringComparison.Ordinal)) n++;
            if (string.Equals(p.b, allele, StringComparison.Ordinal)) n++;
            return n;
        }

        /// <summary>İfade edilen (baskın) alel; lokus yoksa null.</summary>
        public string Expressed(string locus)
        {
            var def = GeneticsCatalog.Locus(Species, locus);
            if (def == null || !Loci.TryGetValue(locus, out var p)) return null;
            if (p.b == YAllele) return p.a;
            return def.Rank(p.a) <= def.Rank(p.b) ? p.a : p.b;
        }

        /// <summary>
        /// Kayıt biçimi (dile bağlı değil): "t|tip", "l|lokus|a|b", "s|stat|değer", "g|hüner", "d|kusur", "r|nadir renk",
        /// "k|lokus:alel", "x|kısır", "i|katsayı".
        /// </summary>
        public string[] Export()
        {
            var list = new List<string> { "t|" + TypeId };
            var keys = new List<string>(Loci.Keys); keys.Sort(StringComparer.Ordinal);
            foreach (var k in keys) list.Add("l|" + k + "|" + Loci[k].a + "|" + Loci[k].b);
            keys = new List<string>(Stats.Keys); keys.Sort(StringComparer.Ordinal);
            foreach (var k in keys) list.Add("s|" + k + "|" + Stats[k].ToString(CultureInfo.InvariantCulture));
            foreach (var g in Gifts) list.Add("g|" + g);
            foreach (var d in Defects) list.Add("d|" + d);
            if (RareCoat != null) list.Add("r|" + RareCoat);
            var rev = new List<string>(Revealed); rev.Sort(StringComparer.Ordinal);
            foreach (var r in rev) list.Add("k|" + r);
            if (Sterile) list.Add("x|1");
            if (Inbreeding > 0) list.Add("i|" + Inbreeding.ToString("0.####", CultureInfo.InvariantCulture));
            return list.ToArray();
        }

        /// <summary>Kayıttan geri kurar; tanınmayan satırlar atlanır. Boşsa null.</summary>
        public static Genome Import(string species, string[] data)
        {
            if (data == null || data.Length == 0) return null;
            var g = new Genome { Species = species };
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var p = line.Split('|');
                switch (p[0])
                {
                    case "t": if (p.Length > 1 && GeneticsCatalog.Type(p[1]) != null) g.TypeId = p[1]; break;
                    case "l": if (p.Length > 3) g.Loci[p[1]] = (p[2], p[3]); break;
                    case "s":
                        if (p.Length > 2 && int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                            g.Stats[p[1]] = Math.Max(GeneStat.Min, Math.Min(GeneStat.Max, v));
                        break;
                    case "g": if (p.Length > 1 && GeneticsCatalog.GiftById(p[1]) != null && !g.Gifts.Contains(p[1])) g.Gifts.Add(p[1]); break;
                    case "d": if (p.Length > 1 && Array.IndexOf(GeneDefect.All, p[1]) >= 0 && !g.Defects.Contains(p[1])) g.Defects.Add(p[1]); break;
                    case "r": if (p.Length > 1) g.RareCoat = p[1]; break;
                    case "k": if (p.Length > 1) g.Revealed.Add(p[1]); break;
                    case "x": g.Sterile = true; break;
                    case "i":
                        if (p.Length > 1 && double.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double f)) g.Inbreeding = Math.Max(0, Math.Min(1, f));
                        break;
                }
            }
            if (g.TypeId == null) g.TypeId = GeneticsCatalog.DefaultType(species)?.Id;
            return g;
        }
    }
}
