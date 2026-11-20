using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>
    /// Bir görünüm lokusu (Project belgesi 48 §3.1). Aleller baskından çekiniğe sıralıdır; ifade edilen alel, iki alelden
    /// sırası önde olanıdır. X'e bağlı lokusta (kedide kızıl) erkek tek alel taşır, ikinci alel <see cref="Genome.YAllele"/>.
    /// </summary>
    public sealed class LocusDef
    {
        public string Id;
        /// <summary>Baskından çekiniğe.</summary>
        public string[] Alleles;
        /// <summary>Satın alınan ya da rastgele üretilen hayvanda her alelin görülme ağırlığı (Alleles ile aynı sıra).</summary>
        public double[] Weights;
        public bool XLinked;
        /// <summary>Kısmi baskın (kremde tek kopya ayrı görünür): ifade alel sayısına bakar.</summary>
        public bool Dosage;

        public int Rank(string allele)
        {
            for (int i = 0; i < Alleles.Length; i++) if (string.Equals(Alleles[i], allele, StringComparison.Ordinal)) return i;
            return Alleles.Length;
        }

        /// <summary>Arayüz anahtarı: <c>gene.&lt;tür&gt;.&lt;lokus&gt;</c>.</summary>
        public string NameKey(string species) => "gene." + species + "." + Id;
    }

    /// <summary>Türe özgü istatistik (1-10). Soy kademesi tavanı belirler (§3.2).</summary>
    public static class GeneStat
    {
        // köpek
        public const string Speed = "speed", Nose = "nose", Courage = "courage", Stamina = "stamina", Temper = "temper", Size = "size";
        // kedi
        public const string Hunting = "hunting", Agility = "agility", Curiosity = "curiosity", Health = "health";
        // binek
        public const string Carry = "carry", Draft = "draft", Gait = "gait";

        public const int Min = 1, Max = 10;

        /// <summary>Soy kademesine göre istatistik tavanı: Köy 6, İyi soy 8, Soylu ve Efsane 10.</summary>
        public static int Cap(AnimalLineage lineage) => lineage == AnimalLineage.Village ? 6 : lineage == AnimalLineage.Good ? 8 : Max;

        public static string NameKey(string stat) => "gene.stat." + stat;
    }

    /// <summary>
    /// Bir tür içi tip (köpekte "tazı", atta "koşu atı"). Tipler ırk değil iştir (belge 48 §4-§7); yavrunun tipi ebeveynlerden gelir.
    /// </summary>
    public sealed class BreedTypeDef
    {
        public string Id;            // "dog.shepherd"
        public string Species;       // "animal.dog"
        /// <summary>İstatistik ortalaması (rastgele üretimde bu değer ±2).</summary>
        public Dictionary<string, int> Stats = new Dictionary<string, int>(StringComparer.Ordinal);
        /// <summary>Bu tipte ağırlığı değişen aleller: "lokus:alel" → ağırlık çarpanı.</summary>
        public Dictionary<string, double> LookBias = new Dictionary<string, double>(StringComparer.Ordinal);
        /// <summary>Sprite gövdesi (belge 48 §11: tip başına tek gövde, renk paletle).</summary>
        public string Body;
        /// <summary>Pazarda satın alınabilir (yoksa yalnız üreme, panayır ya da adlı kırma).</summary>
        public bool Market;
        /// <summary>Pazarda görülme ağırlığı.</summary>
        public double MarketWeight = 1;
        /// <summary>Adlı kırma (iki tipten doğar, pazarda yok).</summary>
        public bool Cross;
        /// <summary>Çocuk ve ergen binebilir (midilli).</summary>
        public bool ChildMount;
        /// <summary>Heybeye ek yuva (yük atı, katır).</summary>
        public int SaddleBagExtra;

        public string NameKey => "breed." + Id;
        public string DescKey => "breed." + Id + ".desc";
    }

    /// <summary>Hüner nadirliği (§3.3). Efsanevi hüner Efsane soy değildir; efsane denemesine küçük bonus verir.</summary>
    public enum GiftRarity { Common = 0, Rare = 1, Choice = 2, Legendary = 3 }

    /// <summary>Doğuştan ek yetenek.</summary>
    public sealed class GiftDef
    {
        public string Id;            // "gift.dog.night_eye"
        public string Species;
        /// <summary>Yalnız bu tiplerde çıkar (null = türün her tipinde).</summary>
        public string[] Types;
        public GiftRarity Rarity;

        public string NameKey => Id + ".name";
        public string DescKey => Id + ".desc";

        public bool AllowedFor(string typeId)
        {
            if (Types == null || Types.Length == 0) return true;
            foreach (var t in Types) if (string.Equals(t, typeId, StringComparison.Ordinal)) return true;
            return false;
        }
    }

    /// <summary>İki tipten adlı kırma (§4.2).</summary>
    public sealed class NamedCross
    {
        public string A, B, Result;
        public bool Matches(string x, string y) =>
            (string.Equals(A, x, StringComparison.Ordinal) && string.Equals(B, y, StringComparison.Ordinal))
            || (string.Equals(A, y, StringComparison.Ordinal) && string.Equals(B, x, StringComparison.Ordinal));
    }

    /// <summary>Türler arası melez tarifi (§3.5): yalnız tablodaki çiftler melez verir.</summary>
    public sealed class SpeciesCross
    {
        public string FemaleSpecies, MaleSpecies, Offspring;
        /// <summary>Gebe kalma şansı çarpanı (aynı türe göre).</summary>
        public double ChanceFactor = 0.6;
    }

    /// <summary>Akrabalık kusuru (§3.4): görünür, ölümcül değil.</summary>
    public static class GeneDefect
    {
        public const string Weak = "defect.weak";        // ömür −%15
        public const string Timid = "defect.timid";      // XP kazanımı −%20
        public const string BadTeeth = "defect.teeth";   // yem +%15
        public static readonly string[] All = { Weak, Timid, BadTeeth };
        public static string NameKey(string id) => id + ".name";
    }
}
