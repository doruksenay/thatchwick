using System;
using System.Collections.Generic;
using System.Linq;

namespace Thatchwick.Core.Customization
{
    /// <summary>Bir genin iki aleli. Görünen değer GeneSpec kuralıyla seçilir.</summary>
    public sealed class AllelePair
    {
        public string A { get; set; }
        public string B { get; set; }
        public AllelePair() { }
        public AllelePair(string a, string b) { A = a; B = b; }
        public AllelePair Clone() => new AllelePair(A, B);
    }

    /// <summary>
    /// Gizli kalıtım verisi (karar 17.11.2026: genler oyuncuya gösterilmez). Her kalıtsal özellik iki alel taşır;
    /// çocuk her ebeveynden birer alel alır. Saç stili, dövme, iz, makyaj, kıyafet ve boya kalıtılmaz.
    /// </summary>
    public sealed class Genome
    {
        private readonly Dictionary<string, AllelePair> _genes = new Dictionary<string, AllelePair>(StringComparer.Ordinal);

        public IEnumerable<string> Keys => _genes.Keys;
        public int Count => _genes.Count;

        public AllelePair Get(string key) => _genes.TryGetValue(key, out var p) ? p : null;
        public void Set(string key, string a, string b) => _genes[key] = new AllelePair(a, b);
        public bool Has(string key) => _genes.ContainsKey(key);

        public Genome Clone()
        {
            var g = new Genome();
            foreach (var kv in _genes) g._genes[kv.Key] = kv.Value.Clone();
            return g;
        }

        /// <summary>Kayıt biçimi: "gen|A|B".</summary>
        public string[] Export() => _genes.OrderBy(k => k.Key, StringComparer.Ordinal).Select(kv => $"{kv.Key}|{kv.Value.A}|{kv.Value.B}").ToArray();

        public static Genome Import(string[] rows)
        {
            if (rows == null || rows.Length == 0) return null;
            var g = new Genome();
            foreach (var r in rows)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var parts = r.Split('|');
                if (parts.Length != 3 || parts[0].Length == 0) continue;
                g.Set(parts[0], parts[1], parts[2]);
            }
            return g.Count == 0 ? null : g;
        }
    }

    /// <summary>Gen kimlikleri.</summary>
    public static class Genes
    {
        public const string Skin = "skin";
        public const string HairColor = "hair_color";
        public const string HairTexture = "hair_texture";
        public const string EyeColor = "eye_color";
        public const string Heterochromia = "heterochromia";
        public const string FaceShape = "face_shape";
        public const string EyeShape = "eye_shape";
        public const string Nose = "nose";
        public const string Ears = "ears";
        public const string Brow = "brow";
        public const string Build = "build";
        public const string Freckles = "freckles";
        public const string EarlyGray = "early_gray";
        public const string Balding = "balding";
        public const string Voice = "voice";

        public const string Yes = "1";
        public const string No = "0";
    }

    public enum GeneMode
    {
        /// <summary>İki alelin karışımı (ten).</summary>
        Blend,
        /// <summary>Sıralı baskınlık: sırası yüksek olan görünür; eşitse A.</summary>
        Rank,
        /// <summary>Baskınlık yok: A görünür (alel sırası döllenmede rastgeledir).</summary>
        Categorical
    }

    public sealed class GeneSpec
    {
        public string Key { get; set; }
        public GeneMode Mode { get; set; }
        /// <summary>Mutasyon ve rastgele taşıyıcı için olası değerler.</summary>
        public List<string> Values { get; set; } = new List<string>();
        /// <summary>Rank modunda alelin baskınlık sırası (yüksek baskın).</summary>
        public Func<string, int> Rank { get; set; } = _ => 0;
    }

    /// <summary>
    /// Kalıtım kuralları (CK3 gibi baskın/çekinik; Sims 4'ün aksine büyükanne ve dededen gelen çekinik
    /// alel torunda ortaya çıkabilir). Doküman 14, bölüm 3H.
    /// </summary>
    public static class Genetics
    {
        /// <summary>Gen başına mutasyon olasılığı (komşu değer).</summary>
        public const double MutationChance = 0.02;
        /// <summary>Kurucu ve köylülerde görünmeyen (çekinik) taşıyıcı alel olasılığı.</summary>
        public const double CarrierChance = 0.35;

        public static List<GeneSpec> Specs(AppearanceCatalog cat)
        {
            var list = new List<GeneSpec>();
            var recHair = new HashSet<Rgb>(cat.RecessiveHairColors);
            var recEye = new HashSet<Rgb>(cat.RecessiveEyeColors);

            list.Add(new GeneSpec { Key = Genes.Skin, Mode = GeneMode.Blend, Values = cat.SkinTones.Select(c => c.ToHex()).ToList() });
            list.Add(new GeneSpec
            {
                Key = Genes.HairColor, Mode = GeneMode.Rank, Values = cat.HairColors.Select(c => c.ToHex()).ToList(),
                Rank = v => Rgb.TryParseHex(v, out var c) && recHair.Contains(Nearest(cat.HairColors, c)) ? 0 : 1
            });
            var textureRank = new Dictionary<string, int> { { "straight", 0 }, { "wavy", 1 }, { "curly", 2 } };
            list.Add(new GeneSpec { Key = Genes.HairTexture, Mode = GeneMode.Rank, Values = textureRank.Keys.ToList(), Rank = v => textureRank.TryGetValue(v ?? "", out var r) ? r : 0 });
            list.Add(new GeneSpec
            {
                Key = Genes.EyeColor, Mode = GeneMode.Rank, Values = cat.EyeColors.Select(c => c.ToHex()).ToList(),
                Rank = v => Rgb.TryParseHex(v, out var c) && recEye.Contains(Nearest(cat.EyeColors, c)) ? 0 : 1
            });
            // Çekinik nadir özellik: yalnızca iki alel de 1 ise görünür.
            list.Add(new GeneSpec { Key = Genes.Heterochromia, Mode = GeneMode.Rank, Values = new List<string> { Genes.No, Genes.Yes }, Rank = v => v == Genes.Yes ? 0 : 1 });
            list.Add(new GeneSpec { Key = Genes.FaceShape, Mode = GeneMode.Categorical, Values = cat.FaceShapes.Select(o => o.Id).ToList() });
            list.Add(new GeneSpec { Key = Genes.EyeShape, Mode = GeneMode.Categorical, Values = cat.EyeShapes.Select(o => o.Id).ToList() });
            list.Add(new GeneSpec { Key = Genes.Nose, Mode = GeneMode.Categorical, Values = cat.Noses.Select(o => o.Id).ToList() });
            list.Add(new GeneSpec { Key = Genes.Ears, Mode = GeneMode.Categorical, Values = cat.Ears.Select(o => o.Id).ToList() });
            list.Add(new GeneSpec { Key = Genes.Brow, Mode = GeneMode.Categorical, Values = cat.Brows.Select(o => o.Id).ToList() });
            list.Add(new GeneSpec { Key = Genes.Build, Mode = GeneMode.Categorical, Values = Enum.GetValues(typeof(BodyBuild)).Cast<int>().Select(i => i.ToString()).ToList() });
            // Baskın: tek alel yeter.
            list.Add(new GeneSpec { Key = Genes.Freckles, Mode = GeneMode.Rank, Values = new List<string> { Genes.No, Genes.Yes }, Rank = v => v == Genes.Yes ? 1 : 0 });
            list.Add(new GeneSpec { Key = Genes.EarlyGray, Mode = GeneMode.Rank, Values = new List<string> { Genes.No, Genes.Yes }, Rank = v => v == Genes.Yes ? 1 : 0 });
            list.Add(new GeneSpec { Key = Genes.Balding, Mode = GeneMode.Rank, Values = new List<string> { Genes.No, Genes.Yes }, Rank = v => v == Genes.Yes ? 0 : 1 });
            list.Add(new GeneSpec { Key = Genes.Voice, Mode = GeneMode.Categorical, Values = Enum.GetValues(typeof(VoiceTimbre)).Cast<int>().Select(i => i.ToString()).ToList() });
            return list;
        }

        /// <summary>Genin görünen değeri.</summary>
        public static string Expressed(GeneSpec spec, AllelePair p, AppearanceCatalog cat = null)
        {
            if (p == null) return null;
            switch (spec.Mode)
            {
                case GeneMode.Blend:
                    if (cat != null && Rgb.TryParseHex(p.A, out var a) && Rgb.TryParseHex(p.B, out var b))
                        return Nearest(cat.SkinTones, Rgb.Lerp(a, b, 0.5)).ToHex();
                    return p.A;
                case GeneMode.Rank:
                    return spec.Rank(p.B) > spec.Rank(p.A) ? p.B : p.A;
                default:
                    return p.A;
            }
        }

        /// <summary>Görünen değer (spec listesinden).</summary>
        public static string Expressed(Genome g, string key, AppearanceCatalog cat)
        {
            var spec = Specs(cat).FirstOrDefault(s => s.Key == key);
            return spec == null ? null : Expressed(spec, g?.Get(key), cat);
        }

        /// <summary>Özellik görünüyor mu (0/1 genler).</summary>
        public static bool Shows(Genome g, string key, AppearanceCatalog cat) => Expressed(g, key, cat) == Genes.Yes;

        /// <summary>
        /// Bir görünümden genom kurar (kurucu, köylü, eski kayıt). A aleli görünen değerdir; B aleli
        /// <paramref name="carrierChance"/> olasılıkla rastgele bir taşıyıcıdır ama görünümü değiştirmez.
        /// </summary>
        public static Genome FromAppearance(Appearance a, AppearanceCatalog cat, Random rng, double carrierChance = CarrierChance)
        {
            var g = new Genome();
            foreach (var spec in Specs(cat))
            {
                string shown = Current(a, spec.Key);
                if (shown == null) continue;
                string other = shown;
                if (spec.Mode != GeneMode.Blend && spec.Values.Count > 0 && rng.NextDouble() < carrierChance)
                {
                    var candidate = spec.Values[rng.Next(spec.Values.Count)];
                    // Taşıyıcı görünümü değiştirmemeli: kategoride A görünür; sıralıda aday daha baskınsa kullanılmaz.
                    if (spec.Mode == GeneMode.Categorical || spec.Rank(candidate) <= spec.Rank(shown)) other = candidate;
                    // Çekinik görünen değer (iki alel gerekir) için diğer alel de çekinik olmalı.
                    if (spec.Mode == GeneMode.Rank && spec.Rank(shown) < spec.Rank(other)) other = shown;
                }
                g.Set(spec.Key, shown, other);
            }
            return g;
        }

        // Görünümden bir genin bugünkü değeri (FromAppearance için).
        private static string Current(Appearance a, string key)
        {
            switch (key)
            {
                case Genes.Skin: return a.SkinColor.ToHex();
                case Genes.HairColor: return a.HairColor.ToHex();
                case Genes.HairTexture: return null;   // stilin dokusu kataloğa göre; kurucuda ayrıca atanır
                case Genes.EyeColor: return a.EyeColor.ToHex();
                case Genes.Heterochromia: return a.EyeColorRight.HasValue ? Genes.Yes : Genes.No;
                case Genes.FaceShape: return a.FaceShapeId;
                case Genes.EyeShape: return a.EyeShapeId;
                case Genes.Nose: return a.NoseId;
                case Genes.Ears: return a.EarsId;
                case Genes.Brow: return a.BrowId;
                case Genes.Build: return ((int)a.Build).ToString();
                case Genes.Freckles: return a.Marks.Any(m => m.Kind == MarkKind.SkinDetail && m.Id == "freckles") ? Genes.Yes : Genes.No;
                case Genes.EarlyGray: return Genes.No;
                case Genes.Balding: return Genes.No;
                case Genes.Voice: return ((int)a.Voice).ToString();
            }
            return null;
        }

        /// <summary>Kurucu genomu: FromAppearance + saç dokusu (stilden) + erken kırlaşma ve kellik taşıyıcısı.</summary>
        public static Genome ForFounder(Appearance a, AppearanceCatalog cat, Random rng)
        {
            var g = FromAppearance(a, cat, rng);
            string tex = cat.HairStyles.FirstOrDefault(o => o.Id == a.HairStyleId)?.Tag;
            if (string.IsNullOrEmpty(tex)) tex = "straight";
            g.Set(Genes.HairTexture, tex, rng.NextDouble() < CarrierChance ? new[] { "straight", "wavy", "curly" }[rng.Next(3)] : tex);
            // Görünen değeri bozmamak için daha baskın aday dokuyu geri al.
            var spec = Specs(cat).First(s => s.Key == Genes.HairTexture);
            if (spec.Rank(g.Get(Genes.HairTexture).B) > spec.Rank(tex)) g.Set(Genes.HairTexture, tex, tex);
            g.Set(Genes.EarlyGray, rng.NextDouble() < 0.15 ? Genes.Yes : Genes.No, Genes.No);
            g.Set(Genes.Balding, Genes.No, rng.NextDouble() < 0.3 ? Genes.Yes : Genes.No);
            return g;
        }

        /// <summary>Çocuğun genomu: her ebeveynden rastgele bir alel; gen başına %2 mutasyon.</summary>
        public static Genome Inherit(Genome mother, Genome father, Random rng, AppearanceCatalog cat)
        {
            var child = new Genome();
            foreach (var spec in Specs(cat))
            {
                var m = mother?.Get(spec.Key);
                var f = father?.Get(spec.Key);
                if (m == null && f == null) continue;
                string fromMother = m != null ? (rng.Next(2) == 0 ? m.A : m.B) : (f.A);
                string fromFather = f != null ? (rng.Next(2) == 0 ? f.A : f.B) : (m.A);
                // Alel sırası rastgele: kategorik genlerde görünen alel iki ebeveynden eşit olasılıkla gelir.
                bool swap = rng.Next(2) == 0;
                string a = swap ? fromFather : fromMother, b = swap ? fromMother : fromFather;
                if (rng.NextDouble() < MutationChance && spec.Values.Count > 1)
                {
                    if (rng.Next(2) == 0) a = Mutate(spec, a, rng); else b = Mutate(spec, b, rng);
                }
                child.Set(spec.Key, a, b);
            }
            return child;
        }

        private static string Mutate(GeneSpec spec, string value, Random rng)
        {
            int i = spec.Values.IndexOf(value);
            if (i < 0) return spec.Values[rng.Next(spec.Values.Count)];
            int j = i + (rng.Next(2) == 0 ? -1 : 1);
            if (j < 0) j = 1;
            if (j >= spec.Values.Count) j = spec.Values.Count - 2;
            return spec.Values[Math.Max(0, j)];
        }

        /// <summary>
        /// Genomu görünüme yansıtır (doğumda). Ten, saç ve göz rengi, yüz parçaları, beden, çil ve ses tınısı.
        /// Saç stili bu fonksiyonda seçilmez (çağıran dokuya uygun stil seçer).
        /// </summary>
        public static void Express(Genome g, Appearance a, AppearanceCatalog cat)
        {
            if (g == null) return;
            foreach (var spec in Specs(cat))
            {
                var p = g.Get(spec.Key);
                if (p == null) continue;
                string v = Expressed(spec, p, cat);
                switch (spec.Key)
                {
                    case Genes.Skin: if (Rgb.TryParseHex(v, out var skin)) a.SkinColor = skin; break;
                    case Genes.HairColor: if (Rgb.TryParseHex(v, out var hair)) { a.HairColor = hair; a.FacialHairColor = hair; } break;
                    case Genes.EyeColor: if (Rgb.TryParseHex(v, out var eye)) a.EyeColor = eye; break;
                    case Genes.FaceShape: a.FaceShapeId = v; break;
                    case Genes.EyeShape: a.EyeShapeId = v; break;
                    case Genes.Nose: a.NoseId = v; break;
                    case Genes.Ears: a.EarsId = v; break;
                    case Genes.Brow: a.BrowId = v; break;
                    case Genes.Build: if (int.TryParse(v, out var b) && Enum.IsDefined(typeof(BodyBuild), b)) a.Build = (BodyBuild)b; break;
                    case Genes.Voice: if (int.TryParse(v, out var t) && Enum.IsDefined(typeof(VoiceTimbre), t)) a.Voice = (VoiceTimbre)t; break;
                    case Genes.Freckles:
                        bool has = a.Marks.Any(m => m.Kind == MarkKind.SkinDetail && m.Id == "freckles");
                        if (v == Genes.Yes && !has) a.Marks.Add(new Mark { Kind = MarkKind.SkinDetail, Id = "freckles", Zone = "face" });
                        if (v != Genes.Yes && has) a.Marks.RemoveAll(m => m.Kind == MarkKind.SkinDetail && m.Id == "freckles");
                        break;
                }
            }
            // İki renkli göz: sağ göz, göz geninin öteki aleli (farklıysa) ya da paletteki komşu renk.
            if (Shows(g, Genes.Heterochromia, cat))
            {
                var eyes = g.Get(Genes.EyeColor);
                Rgb other;
                if (eyes != null && eyes.A != eyes.B && Rgb.TryParseHex(eyes.A == a.EyeColor.ToHex() ? eyes.B : eyes.A, out other)) a.EyeColorRight = other;
                else if (cat.EyeColors.Count > 1)
                {
                    int i = Math.Max(0, cat.EyeColors.IndexOf(Nearest(cat.EyeColors, a.EyeColor)));
                    a.EyeColorRight = cat.EyeColors[(i + 2) % cat.EyeColors.Count];
                }
            }
            else a.EyeColorRight = null;
        }

        public static Rgb Nearest(IList<Rgb> palette, Rgb target)
        {
            if (palette == null || palette.Count == 0) return target;
            Rgb best = palette[0];
            double bestDist = double.MaxValue;
            foreach (var c in palette)
            {
                double dr = c.R - target.R, dg = c.G - target.G, db = c.B - target.B;
                double d = dr * dr + dg * dg + db * db;
                if (d < bestDist) { bestDist = d; best = c; }
            }
            return best;
        }
    }
}
