using System.Collections.Generic;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>
    /// Genomun görünüşü (belge 48 §3.1, §4.4, §5.2, §7.2): renk, desen, beyaz, tüy ve sprite katmanları.
    /// Metin anahtardır (<c>look.dog.color.red</c>); Game katmanı çevirir. Renk paletle gövdeye uygulanır (§11).
    /// </summary>
    public sealed class Phenotype
    {
        /// <summary>Ana renk anahtarı (<c>look.&lt;tür&gt;.color.&lt;ad&gt;</c>).</summary>
        public string ColorKey;
        /// <summary>Desen anahtarı (tekir, kaplan çizgili, iki renkli...); yoksa null.</summary>
        public string PatternKey;
        /// <summary>Beyaz leke 0 yok, 1 az (çorap, göğüs), 2 çok (alaca, üç renkli), 3 tamamen beyaz.</summary>
        public int White;
        public bool LongCoat;
        /// <summary>Köpekte siyah maske.</summary>
        public bool Mask;
        /// <summary>Ana renk (yer tutucu çizim ve palet için).</summary>
        public string TintHex;
        /// <summary>Gövde + katmanlar: "dog_shepherd", "coat_red", "pat_brindle", "white_1", "mask", "long".</summary>
        public List<string> Layers = new List<string>();
        /// <summary>Kedide mavi gözlü beyaz: "Derin uyku" (çağırınca gelmez; ceza değil karakter, §5.2).</summary>
        public bool DeepSleeper;
        /// <summary>Gri (kır) at: yaşla ağarır.</summary>
        public double GreyFraction;

        public static Phenotype Of(Genome g, double ageFraction = 0.5, int seed = 0)
        {
            var p = new Phenotype();
            if (g == null) return p;
            var type = g.Type;
            if (type != null) p.Layers.Add(type.Body);
            switch (g.Species)
            {
                case GeneticsCatalog.Dog: Dog(g, p); break;
                case GeneticsCatalog.Cat: Cat(g, p, seed); break;
                case GeneticsCatalog.Donkey: Donkey(g, p); break;
                default: if (GeneticsCatalog.IsEquine(g.Species)) Horse(g, p, ageFraction); break;
            }
            if (g.RareCoat != null)
            {
                p.ColorKey = "look." + g.RareCoat;
                p.Layers.Add(g.RareCoat.Replace("rare.", "coat_rare_"));
                p.TintHex = RareTint(g.RareCoat);
            }
            return p;
        }

        private static void Dog(Genome g, Phenotype p)
        {
            bool dilute = g.Count("D", "d") == 2, liver = g.Count("B", "b") == 2;
            string eum = liver ? (dilute ? "isabella" : "liver") : (dilute ? "blue" : "black");
            if (g.Count("E", "e") == 2)
            {
                p.ColorKey = "look.dog.color." + (dilute ? "cream" : "red");
                p.TintHex = dilute ? "#E6D2A8" : "#B5652E";
            }
            else
            {
                string k = g.Expressed("K");
                if (k == "KB") { p.ColorKey = "look.dog.color." + eum; p.TintHex = EumTint(eum); }
                else
                {
                    if (k == "kbr") p.PatternKey = "look.dog.pattern.brindle";
                    if (g.Expressed("A") == "ay") { p.ColorKey = "look.dog.color.sable"; p.TintHex = "#C29A5B"; }
                    else { p.ColorKey = "look.dog.color." + eum + "_tan"; p.TintHex = EumTint(eum); if (p.PatternKey == null) p.PatternKey = "look.dog.pattern.tan_points"; }
                }
                p.Mask = g.Expressed("E") == "Em";
            }
            int sp = g.Count("S", "sp");
            p.White = sp == 2 ? 2 : sp == 1 ? 1 : 0;
            p.LongCoat = g.Count("L", "l") == 2;
            p.Layers.Add("coat_" + p.ColorKey.Substring(p.ColorKey.LastIndexOf('.') + 1));
            if (p.PatternKey != null) p.Layers.Add("pat_" + p.PatternKey.Substring(p.PatternKey.LastIndexOf('.') + 1));
            if (p.Mask) p.Layers.Add("mask");
            if (p.White > 0) p.Layers.Add("white_" + p.White);
            if (p.LongCoat) p.Layers.Add("long");
        }

        private static string EumTint(string eum) =>
            eum == "black" ? "#2B2522" : eum == "liver" ? "#6B3E26" : eum == "blue" ? "#6E7378" : "#A98C78";

        private static void Cat(Genome g, Phenotype p, int seed)
        {
            p.LongCoat = g.Count("L", "l") == 2;
            int dose = g.Count("S", "S");
            if (g.Has("W", "W"))
            {
                p.ColorKey = "look.cat.color.white";
                p.TintHex = "#F2EFE8";
                p.White = 3;
                // Baskın beyazda göz rengi: kimlikten (mavi, tek mavi, turuncu); mavi gözlü "Derin uyku".
                int eye = ((seed % 3) + 3) % 3;
                p.PatternKey = eye == 0 ? "look.cat.eyes.blue" : eye == 1 ? "look.cat.eyes.odd" : "look.cat.eyes.amber";
                p.DeepSleeper = eye == 0;
                p.Layers.Add("coat_white");
                if (p.LongCoat) p.Layers.Add("long");
                return;
            }
            bool dilute = g.Count("D", "d") == 2;
            bool choc = g.Count("B", "b") == 2;
            string nonRed = choc ? (dilute ? "lilac" : "chocolate") : (dilute ? "blue" : "black");
            string red = dilute ? "cream" : "red";
            (string a, string b) o = g.Loci.TryGetValue("O", out var op) ? op : ("o", "o");
            bool male = o.b == Genome.YAllele;
            int orange = (o.a == "O" ? 1 : 0) + (!male && o.b == "O" ? 1 : 0);
            bool tortie = !male && orange == 1;   // erkek üç renkli (XXY) Y taşımaz, iki X'i vardır: bu yoldan bağa görünür
            bool tabby = g.Has("A", "A");
            string pattern = g.Expressed("Tb") == "Mc" ? "mackerel" : g.Expressed("Tb") == "Sp" ? "spotted" : "classic";
            if (tortie)
            {
                p.ColorKey = dose >= 1 ? "look.cat.color.calico" : "look.cat.color.tortie";
                p.TintHex = dilute ? "#B9A08C" : "#5A3B28";
                if (tabby) p.PatternKey = "look.cat.pattern." + pattern;
            }
            else if (orange > 0)
            {
                p.ColorKey = "look.cat.color." + red;
                p.TintHex = dilute ? "#E8C79A" : "#D07A32";
                p.PatternKey = "look.cat.pattern." + pattern;   // kızıl kedi hep çizgili görünür
            }
            else
            {
                p.ColorKey = "look.cat.color." + nonRed + (tabby ? "_tabby" : "");
                p.TintHex = nonRed == "black" ? (tabby ? "#7A6A55" : "#2A2624") : nonRed == "blue" ? "#7E848C" : nonRed == "chocolate" ? "#5E3B2A" : "#B5A2A0";
                if (tabby) p.PatternKey = "look.cat.pattern." + pattern;
            }
            p.White = dose;
            p.Layers.Add("coat_" + p.ColorKey.Substring(p.ColorKey.LastIndexOf('.') + 1));
            if (p.PatternKey != null) p.Layers.Add("pat_" + pattern);
            if (p.White > 0) p.Layers.Add("white_" + p.White);
            if (p.LongCoat) p.Layers.Add("long");
        }

        private static void Horse(Genome g, Phenotype p, double ageFraction)
        {
            bool red = g.Count("E", "e") == 2;
            bool bay = !red && g.Has("A", "A");
            int cream = g.Count("Cr", "Cr");
            string baseColor = red ? "chestnut" : bay ? "bay" : "black";
            string color = baseColor;
            if (cream == 1) color = red ? "palomino" : bay ? "buckskin" : "smoky_black";
            else if (cream == 2) color = "cream";
            p.ColorKey = "look.horse.color." + color;
            p.TintHex = color == "chestnut" ? "#8E4A24" : color == "bay" ? "#6B3B22" : color == "black" ? "#231E1C"
                : color == "palomino" ? "#D8B15E" : color == "buckskin" ? "#C49A5E" : color == "smoky_black" ? "#3D332C" : "#EDE3CC";
            if (g.Has("Rn", "Rn")) p.PatternKey = "look.horse.pattern.roan";
            if (g.Has("G", "G"))
            {
                // Kır at renkli doğar, yaşla ağarır (§7.2): ömrünün ~%40'ında tamamen beyazlar.
                p.GreyFraction = System.Math.Max(0, System.Math.Min(1, ageFraction / 0.4));
                if (p.GreyFraction >= 0.5) { p.ColorKey = "look.horse.color.grey"; p.TintHex = p.GreyFraction >= 1 ? "#E6E4DF" : "#A9A6A0"; }
            }
            int w = g.Count("W", "Wm");
            p.White = w;
            p.Layers.Add("coat_" + color);
            if (p.PatternKey != null) p.Layers.Add("pat_roan");
            if (p.GreyFraction > 0) p.Layers.Add("grey_" + (int)(p.GreyFraction * 4));
            if (w > 0) p.Layers.Add("white_" + w);
        }

        private static void Donkey(Genome g, Phenotype p)
        {
            string c = g.Expressed("C") == "Gy" ? "grey" : g.Expressed("C") == "Br" ? "brown" : "black";
            p.ColorKey = "look.donkey.color." + c;
            p.TintHex = c == "grey" ? "#8C8378" : c == "brown" ? "#6E4E36" : "#2D2723";
            if (g.Has("Sp", "Sp")) { p.PatternKey = "look.donkey.pattern.spotted"; p.White = 2; }
            p.Layers.Add("coat_" + c);
            if (p.White > 0) p.Layers.Add("white_2");
        }

        private static string RareTint(string rare) =>
            rare == "rare.golden" ? "#E3B652" : rare == "rare.smoke" ? "#55575C" : rare == "rare.silver" ? "#B7BCC2"
            : rare == "rare.white_donkey" ? "#EFEDE7" : "#E9E1D2";
    }
}
