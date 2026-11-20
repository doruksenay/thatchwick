using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>Çiftleşmenin bağlamı: barınak konforu, Hayvancılık, akrabalık.</summary>
    public struct BreedContext
    {
        /// <summary>Barınak konforu 0-100: iyi ebeveynin istatistiğini alma şansını en çok +%10 artırır (Mewgenics "uyarım").</summary>
        public float Comfort;
        /// <summary>Hayvancılık dalı vb. ek şans (0-0,10).</summary>
        public double HusbandryBonus;
        /// <summary>Yavrunun akrabalık katsayısı (Wright).</summary>
        public double Inbreeding;
        public AnimalLineage MotherLineage, FatherLineage;
        public bool LegendaryDescent;
    }

    /// <summary>Doğumun sonucu (birden çok alan: genom, cinsiyet düzeltmesi, kayıt notu).</summary>
    public sealed class BirthGenes
    {
        public Genome Genome;
        /// <summary>Erkek üç renkli (XXY) gibi doğuştan nadir görünüm: kronik ve bildirim için anahtar.</summary>
        public string RareEvent;
        public bool Mutated;
    }

    /// <summary>
    /// Kalıtım kuralları (Project belgesi 48 §3): görünüm Mendel-lite (her lokusta her ebeveynden bir alel, kızıl X'e bağlı),
    /// istatistik ARK tarzı (iki ebeveynden biri, iyisi %55 + konfor + Hayvancılık, en çok %75), 3 x %2 mutasyon,
    /// hüner tek ebeveynden %35 ya da yeni zarla (Yaygın %60, Nadir %28, Seçkin %10, Efsanevi %2), akrabalık kusuru.
    /// </summary>
    public static class GeneticsService
    {
        public const double BetterStatBase = 0.55, BetterStatComfortMax = 0.10, BetterStatHusbandryMax = 0.10, BetterStatMax = 0.75;
        public const int MutationRolls = 3;
        public const double MutationChance = 0.02, MutationRareCoatShare = 0.25;
        public const double GiftInheritChance = 0.35, NewGiftChance = 0.45;
        public const int MaxGifts = 2;
        public const double RarityCommon = 0.60, RarityRare = 0.28, RarityChoice = 0.10;   // kalan %2 Efsanevi
        public const double NobleParentBump = 0.10, LegendaryDescentBump = 0.05;
        public const double DefectThreshold = 0.125, DefectFactor = 0.4;
        public const double MaleCalicoChance = 0.001;
        /// <summary>Katır ve bardoda melez gücü: dayanıklılık ve taşımaya +%15 (en az +1).</summary>
        public const double HybridVigor = 0.15;

        // ---------------- rastgele genom (satın alma, eski kayıt, yaban) ----------------

        /// <summary>Tipine göre rastgele genom. female: X'e bağlı lokus için gerekir.</summary>
        public static Genome Random(string species, string typeId, AnimalLineage lineage, bool female, Random rng)
        {
            var type = GeneticsCatalog.Type(typeId) ?? GeneticsCatalog.DefaultType(species);
            var g = new Genome { Species = species, TypeId = type?.Id };
            foreach (var l in GeneticsCatalog.Loci(species))
            {
                string a = Draw(l, type, rng);
                string b = l.XLinked && !female ? Genome.YAllele : Draw(l, type, rng);
                g.Loci[l.Id] = (a, b);
            }
            int cap = GeneStat.Cap(lineage);
            foreach (var s in GeneticsCatalog.Stats(species))
            {
                int baseV = type != null && type.Stats.TryGetValue(s, out var v) ? v : 5;
                g.Stats[s] = Clamp(baseV + rng.Next(-2, 3), cap);
            }
            if (GeneticsCatalog.IsSterileHybrid(species)) g.Sterile = true;
            return g;
        }

        private static string Draw(LocusDef l, BreedTypeDef type, Random rng)
        {
            double total = 0;
            var w = new double[l.Alleles.Length];
            for (int i = 0; i < w.Length; i++)
            {
                double x = l.Weights != null && i < l.Weights.Length ? l.Weights[i] : 1.0;
                if (type != null && type.LookBias.TryGetValue(l.Id + ":" + l.Alleles[i], out var m)) x *= m;
                w[i] = x; total += x;
            }
            double r = rng.NextDouble() * total;
            for (int i = 0; i < w.Length; i++) { r -= w[i]; if (r <= 0) return l.Alleles[i]; }
            return l.Alleles[l.Alleles.Length - 1];
        }

        /// <summary>
        /// Oyuncunun seçtiği görünümden genom (satın alınan ya da bulunan hayvan, belge 48 §6.2): seçilen alel ifade edilir,
        /// ikinci alel ondan çekinik ya da aynı olanlardan rastgele (taşıyıcılık gizli kalır).
        /// </summary>
        public static Genome FromLook(string species, string typeId, IDictionary<string, string> expressed, AnimalLineage lineage, bool female, Random rng)
        {
            var g = Random(species, typeId, lineage, female, rng);
            if (expressed == null) return g;
            foreach (var kv in expressed)
            {
                var l = GeneticsCatalog.Locus(species, kv.Key);
                if (l == null) continue;
                int rank = l.Rank(kv.Value);
                if (rank >= l.Alleles.Length) continue;
                if (l.XLinked && !female) { g.Loci[l.Id] = (kv.Value, Genome.YAllele); continue; }
                if (l.Dosage)
                {
                    // Dozlu lokusta "ifade" kopya sayısıdır: alel 1 kopya, ikinci alelin kendisi 0 kopya; çift kopya için iki kez yaz.
                    g.Loci[l.Id] = (kv.Value, l.Alleles[Math.Min(l.Alleles.Length - 1, rank + 1)]);
                    continue;
                }
                string second = l.Alleles[rank + rng.Next(l.Alleles.Length - rank)];
                g.Loci[l.Id] = rng.NextDouble() < 0.5 ? (kv.Value, second) : (second, kv.Value);
            }
            return g;
        }

        // ---------------- üreme ----------------

        /// <summary>
        /// Yavru genomu. offspringSpecies anneninkinden farklıysa (katır) melezdir: renk at ebeveyninden, istatistik ortalama
        /// ve melez gücü. female: yavrunun cinsiyeti (X'e bağlı lokus).
        /// </summary>
        public static BirthGenes Breed(Genome mother, Genome father, string offspringSpecies, bool female, AnimalLineage lineage, BreedContext ctx, Random rng)
        {
            var result = new BirthGenes();
            if (mother == null) return result;
            if (father == null) father = mother;
            bool hybrid = !string.Equals(offspringSpecies, mother.Species, StringComparison.Ordinal)
                       || !string.Equals(mother.Species, father.Species, StringComparison.Ordinal);
            var g = new Genome { Species = offspringSpecies, Inbreeding = Math.Max(0, Math.Min(1, ctx.Inbreeding)) };
            g.TypeId = ChildType(mother, father, offspringSpecies, hybrid, rng);

            // Görünüm
            if (hybrid)
            {
                // Melezin rengi at ebeveyninden (iki çekiliş); eşek lokusları yok.
                var horse = mother.Species == GeneticsCatalog.Horse ? mother : father.Species == GeneticsCatalog.Horse ? father : mother;
                foreach (var l in GeneticsCatalog.Loci(offspringSpecies))
                    g.Loci[l.Id] = horse.Loci.TryGetValue(l.Id, out var hp) ? (Pick(hp, rng), Pick(hp, rng)) : (l.Alleles[0], l.Alleles[l.Alleles.Length - 1]);
            }
            else
            {
                foreach (var l in GeneticsCatalog.Loci(offspringSpecies))
                {
                    (string a, string b) m = mother.Loci.TryGetValue(l.Id, out var mp) ? mp : (l.Alleles[0], l.Alleles[0]);
                    (string a, string b) f = father.Loci.TryGetValue(l.Id, out var fp) ? fp : (l.Alleles[0], l.Alleles[0]);
                    if (l.XLinked)
                    {
                        string fromMother = Pick(m, rng);
                        string fatherX = f.b == Genome.YAllele ? f.a : Pick(f, rng);
                        if (female) g.Loci[l.Id] = (fromMother, fatherX);
                        else if (m.a != m.b && m.a != Genome.YAllele && rng.NextDouble() < MaleCalicoChance)
                        {
                            // Erkek üç renkli (XXY): iki X, kısır; çok nadir doğal sürpriz (§5.2).
                            g.Loci[l.Id] = (m.a, m.b);
                            g.Sterile = true;
                            result.RareEvent = "event.male_calico";
                        }
                        else g.Loci[l.Id] = (fromMother, Genome.YAllele);
                    }
                    else g.Loci[l.Id] = (Pick(m, rng), Pick(f, rng));
                }
            }

            // İstatistik
            int cap = GeneStat.Cap(lineage);
            double better = Math.Min(BetterStatMax, BetterStatBase
                + BetterStatComfortMax * Math.Max(0f, Math.Min(100f, ctx.Comfort)) / 100.0
                + Math.Max(0, Math.Min(BetterStatHusbandryMax, ctx.HusbandryBonus)));
            foreach (var s in GeneticsCatalog.Stats(offspringSpecies))
            {
                int mv = mother.Stat(s), fv = father.Stat(s);
                int v;
                if (hybrid)
                {
                    double avg = (mv + fv) / 2.0;
                    if (s == GeneStat.Stamina || s == GeneStat.Carry) avg = Math.Max(avg + 1, avg * (1 + HybridVigor));
                    v = (int)Math.Round(avg, MidpointRounding.AwayFromZero);
                }
                else
                {
                    int hi = Math.Max(mv, fv), lo = Math.Min(mv, fv);
                    v = rng.NextDouble() < better ? hi : lo;
                }
                g.Stats[s] = Clamp(v, cap);
            }

            // Mutasyon (§3.2): 3 zar x %2, her biri bir istatistiğe +1; dörtte biri nadir renk getirir.
            var stats = GeneticsCatalog.Stats(offspringSpecies);
            for (int i = 0; i < MutationRolls && stats.Count > 0; i++)
            {
                if (rng.NextDouble() >= MutationChance) continue;
                string s = stats[rng.Next(stats.Count)];
                g.Stats[s] = Clamp(g.Stat(s) + 1, cap);
                result.Mutated = true;
                var rare = GeneticsCatalog.RareCoats(offspringSpecies);
                if (rare.Length > 0 && rng.NextDouble() < MutationRareCoatShare) g.RareCoat = rare[rng.Next(rare.Length)];
            }
            if (g.RareCoat == null)
            {
                // Nadir renk kalıtımı: iki ebeveyn de taşıyorsa %50, biri taşıyorsa %25 (basit baskın kural, §9.5 ile aynı).
                int carriers = (mother.RareCoat != null ? 1 : 0) + (father.RareCoat != null ? 1 : 0);
                if (carriers > 0 && rng.NextDouble() < 0.25 * carriers)
                {
                    string rare = mother.RareCoat ?? father.RareCoat;
                    if (Array.IndexOf(GeneticsCatalog.RareCoats(offspringSpecies), rare) >= 0) g.RareCoat = rare;
                }
            }

            // Hüner (§3.3)
            var parent = rng.NextDouble() < 0.5 ? mother : father;
            foreach (var gift in parent.Gifts)
            {
                var def = GeneticsCatalog.GiftById(gift);
                if (g.Gifts.Count >= MaxGifts || def == null) break;
                if (def.AllowedFor(g.TypeId) && PoolMatches(def, offspringSpecies) && rng.NextDouble() < GiftInheritChance) g.Gifts.Add(gift);
            }
            if (g.Gifts.Count < MaxGifts && rng.NextDouble() < NewGiftChance)
            {
                double bump = (ctx.MotherLineage >= AnimalLineage.Noble || ctx.FatherLineage >= AnimalLineage.Noble ? NobleParentBump : 0)
                            + (ctx.LegendaryDescent ? LegendaryDescentBump : 0);
                var rarity = RollRarity(rng, bump);
                var gift = RollGift(offspringSpecies, g.TypeId, rarity, g.Gifts, rng);
                if (gift != null) g.Gifts.Add(gift);
            }

            // Kısırlık ve akrabalık kusuru (§3.4)
            if (GeneticsCatalog.IsSterileHybrid(offspringSpecies)) g.Sterile = true;
            if (g.Inbreeding > DefectThreshold && rng.NextDouble() < g.Inbreeding * DefectFactor)
                g.Defects.Add(GeneDefect.All[rng.Next(GeneDefect.All.Length)]);

            // Soy Defteri: yavrunun çekinik homozigot lokusu ebeveynlerin taşıyıcılığını açığa çıkarır.
            if (!hybrid) RevealCarriers(g, mother, father);
            result.Genome = g;
            return result;
        }

        private static bool PoolMatches(GiftDef def, string species) =>
            def.Species == species || (def.Species == GeneticsCatalog.Equine && GeneticsCatalog.IsEquine(species));

        private static string Pick((string a, string b) p, Random rng)
        {
            if (p.b == Genome.YAllele) return p.a;
            return rng.NextDouble() < 0.5 ? p.a : p.b;
        }

        private static int Clamp(int v, int cap) => Math.Max(GeneStat.Min, Math.Min(cap, v));

        /// <summary>Yavrunun tipi: aynı tip aynı kalır; köpekte adlı kırma %35; atta farklı tiplerden "kırma at" %35; yoksa yarı yarıya.</summary>
        public static string ChildType(Genome mother, Genome father, string offspringSpecies, bool hybrid, Random rng)
        {
            if (hybrid || mother == null) return GeneticsCatalog.DefaultType(offspringSpecies)?.Id;
            string a = mother.TypeId, b = father?.TypeId ?? a;
            if (string.Equals(a, b, StringComparison.Ordinal)) return a;
            foreach (var c in GeneticsCatalog.NamedCrosses)
                if (c.Matches(a, b)) return rng.NextDouble() < GeneticsCatalog.NamedCrossChance ? c.Result : (rng.NextDouble() < 0.5 ? a : b);
            if (offspringSpecies == GeneticsCatalog.Horse && rng.NextDouble() < GeneticsCatalog.HorseCrossbredChance) return "horse.crossbred";
            return rng.NextDouble() < 0.5 ? a : b;
        }

        public static GiftRarity RollRarity(Random rng, double bump = 0)
        {
            double r = rng.NextDouble();
            var rarity = r < RarityCommon ? GiftRarity.Common : r < RarityCommon + RarityRare ? GiftRarity.Rare
                : r < RarityCommon + RarityRare + RarityChoice ? GiftRarity.Choice : GiftRarity.Legendary;
            if (bump > 0 && rarity < GiftRarity.Legendary && rng.NextDouble() < bump) rarity++;
            return rarity;
        }

        private static string RollGift(string species, string typeId, GiftRarity rarity, List<string> have, Random rng)
        {
            for (var r = rarity; r >= GiftRarity.Common; r--)
            {
                var pool = new List<string>();
                foreach (var g in GeneticsCatalog.GiftsFor(species, typeId)) if (g.Rarity == r && !have.Contains(g.Id)) pool.Add(g.Id);
                if (pool.Count > 0) return pool[rng.Next(pool.Count)];
            }
            return null;
        }

        /// <summary>Yavruda iki kopya çekinik alel görülünce iki ebeveynin de o aleli taşıdığı Soy Defteri'nde açılır.</summary>
        public static void RevealCarriers(Genome child, Genome mother, Genome father)
        {
            foreach (var l in GeneticsCatalog.Loci(child.Species))
            {
                if (!child.Loci.TryGetValue(l.Id, out var p) || p.b == Genome.YAllele) continue;
                if (!string.Equals(p.a, p.b, StringComparison.Ordinal) || l.Rank(p.a) == 0) continue;
                string key = l.Id + ":" + p.a;
                child.Revealed.Add(key);
                if (mother != null && mother.Has(l.Id, p.a)) mother.Revealed.Add(key);
                if (father != null && father.Has(l.Id, p.a)) father.Revealed.Add(key);
            }
        }

        /// <summary>
        /// Soy Defteri'nde görülen aleller: ifade edilen her zaman; Hayvancılık 6+ hepsi; 3+ ilk yarısı; ayrıca yavru doğumuyla açılanlar.
        /// Bilinmeyen alel "?" döner.
        /// </summary>
        public static (string a, string b) KnownPair(Genome g, string locus, int husbandryLevel)
        {
            if (g == null || !g.Loci.TryGetValue(locus, out var p)) return ("?", "?");
            if (husbandryLevel >= 6) return p;
            var loci = GeneticsCatalog.Loci(g.Species);
            int index = 0;
            for (int i = 0; i < loci.Count; i++) if (loci[i].Id == locus) index = i;
            if (husbandryLevel >= 3 && index < (loci.Count + 1) / 2) return p;
            string expressed = g.Expressed(locus);
            string Known(string x) => x == expressed || x == Genome.YAllele || g.Revealed.Contains(locus + ":" + x) ? x : "?";
            return (Known(p.a), Known(p.b));
        }

        // ---------------- akrabalık (Wright) ----------------

        /// <summary>Soy kaydı: kimlik → (anne, baba, akrabalık katsayısı).</summary>
        public delegate bool PedigreeLookup(int id, out int mother, out int father, out double inbreeding);

        /// <summary>
        /// Anne ve babanın ortak atalarından yavrunun akrabalık katsayısı (5 kuşak): F = Σ (1/2)^(n1+n2+1) (1+F_A).
        /// Kardeş eşleşmesi 0,25; yeğen-hala 0,125. Yol çakışması ihmal edilir (oyun için yeterli).
        /// </summary>
        public static double Inbreeding(int motherId, int fatherId, PedigreeLookup lookup, int maxDepth = 5)
        {
            if (motherId <= 0 || fatherId <= 0 || lookup == null) return 0;
            var ma = Ancestors(motherId, lookup, maxDepth);
            var fa = Ancestors(fatherId, lookup, maxDepth);
            double f = 0;
            foreach (var kv in ma)
            {
                if (!fa.TryGetValue(kv.Key, out var fPaths)) continue;
                double fa0 = lookup(kv.Key, out _, out _, out var fA) ? fA : 0;
                foreach (int n1 in kv.Value)
                    foreach (int n2 in fPaths)
                        f += Math.Pow(0.5, n1 + n2 + 1) * (1 + fa0);
            }
            return Math.Min(1, f);
        }

        private static Dictionary<int, List<int>> Ancestors(int id, PedigreeLookup lookup, int maxDepth)
        {
            var map = new Dictionary<int, List<int>>();
            void Walk(int x, int depth)
            {
                if (x <= 0 || depth > maxDepth) return;
                if (!map.TryGetValue(x, out var list)) { list = new List<int>(); map[x] = list; }
                list.Add(depth);
                if (!lookup(x, out int m, out int f, out _)) return;
                Walk(m, depth + 1);
                Walk(f, depth + 1);
            }
            Walk(id, 0);
            return map;
        }

        // ---------------- oyun etkileri ----------------

        /// <summary>Binek hızı çarpanı: Hız 5 = 1,0; her puan %3 (Hız 8: x1,09). Rüzgâr Doğan x1,08.</summary>
        public static double RideFactor(Genome g)
        {
            if (g == null) return 1.0;
            double f = 0.85 + 0.03 * g.Stat(GeneStat.Speed);
            if (g.HasGift(Gift.WindBorn)) f *= 1.08;
            return f;
        }

        /// <summary>Heybe yuvası: taban 6 + tip (yük atı +4, katır +8) + Taşıma 5'in üstündeki her puan +1 + Demir Sırt +4.</summary>
        public static int SaddleBagSlots(Genome g)
        {
            int slots = HorseAbilities.SaddleBagSlots;
            if (g == null) return slots;
            slots += g.Type?.SaddleBagExtra ?? 0;
            slots += Math.Max(0, g.Stat(GeneStat.Carry) - 5);
            if (g.HasGift(Gift.IronBack)) slots += 4;
            return slots;
        }

        /// <summary>Ömür çarpanı: Dokuz Can x1,4, zayıf bünye x0,85; katır x1,4 türün kendi ömründe.</summary>
        public static double LifespanFactor(Genome g)
        {
            if (g == null) return 1.0;
            double f = 1.0;
            if (g.HasGift(Gift.NineLives)) f *= 1.4;
            if (g.HasDefect(GeneDefect.Weak)) f *= 0.85;
            return f;
        }

        public static double XpFactor(Genome g) => g != null && g.HasDefect(GeneDefect.Timid) ? 0.8 : 1.0;
        public static double FeedFactor(Genome g) => g != null && g.HasDefect(GeneDefect.BadTeeth) ? 1.15 : 1.0;

        /// <summary>Çocuk ve ergen binebilir (midilli, eşek).</summary>
        public static bool ChildMount(Genome g) => g?.Type?.ChildMount == true;

        /// <summary>Kurt savunmasına köpeğin katkısı: tip (çoban ve kırmaları, mastıf +), Cesaret ve hüner.</summary>
        public static int DogDefenseBonus(Genome g)
        {
            if (g == null) return 0;
            int b = 0;
            if (g.HasGift(Gift.NightEye)) b++;
            if (g.Stat(GeneStat.Courage) >= 9) b++;
            return b;
        }

        /// <summary>Kurt başı sayılmak için gereken köpek seviyesi (Doğuştan önder: 3, normal 5).</summary>
        public static int LeaderLevel(Genome g) => g != null && g.HasGift(Gift.BornLeader) ? 3 : 5;

        /// <summary>Can Yoldaşı bağ çarpanı (Çocuk dostu, Sessiz adım).</summary>
        public static double BondFactor(Genome g) => g != null && (g.HasGift(Gift.ChildFriend) || g.HasGift(Gift.SilentStep)) ? 1.5 : 1.0;

        // ---------------- hazır hayvanlar ----------------

        /// <summary>Başlangıç kedisi Alex: ambar kedisi, tekir (uskumru), beyaz çoraplı, kısa tüy (belge 31-0.5).</summary>
        public static Genome Alex(bool female, Random rng) => FromLook(GeneticsCatalog.Cat, "cat.barn",
            new Dictionary<string, string> { ["O"] = "o", ["A"] = "A", ["Tb"] = "Mc", ["B"] = "B", ["D"] = "D", ["S"] = "S", ["W"] = "w", ["L"] = "L" },
            AnimalLineage.Village, female, rng);

        /// <summary>Başlangıç köpeği Bran: iri çoban köpeği, kum gövde, siyah maske, uzun kalın tüy.</summary>
        public static Genome Bran(bool female, Random rng)
        {
            var g = FromLook(GeneticsCatalog.Dog, "dog.shepherd",
                new Dictionary<string, string> { ["E"] = "Em", ["K"] = "ky", ["A"] = "ay", ["B"] = "B", ["D"] = "D", ["S"] = "S" },
                AnimalLineage.Village, female, rng);
            g.Loci["L"] = ("l", "l");
            g.Loci["K"] = ("ky", "ky");
            return g;
        }
    }
}
