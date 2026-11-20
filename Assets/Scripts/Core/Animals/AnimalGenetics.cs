using System;
using System.Collections.Generic;
using System.Globalization;
using Thatchwick.Core.Animals.Genetics;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Animals
{
    // Genetik ve soy defteri (Project belgesi 48 §3, §6): genom atama, soy kaydı, akrabalık, evcil hayvan tavanı.
    public sealed partial class AnimalSystem
    {
        /// <summary>Köpek ve kedide tür başına en çok hayvan (barınak 1'er alır; bu tavan aşırı çoğalmayı keser, §6.1).</summary>
        public const int PetCap = 8;

        // Genetik zarları ayrı: eski testlerin hayvan zarı dizisi değişmesin.
        private readonly Random _geneRng = new Random(991);

        private readonly struct PedigreeEntry
        {
            public readonly int Mother, Father;
            public readonly double Inbreeding;
            public readonly string Species;
            public PedigreeEntry(int m, int f, double i, string species) { Mother = m; Father = f; Inbreeding = i; Species = species; }
        }

        // Soy kaydı: ölen ve satılan hayvanlar da kalır (akrabalık 5 kuşak geriye bakar).
        private readonly Dictionary<int, PedigreeEntry> _pedigree = new Dictionary<int, PedigreeEntry>();

        /// <summary>Doğuşta nadir olay (erkek üç renkli kedi, mucize tay): yavru ve anahtar.</summary>
        public event Action<Animal, string> GeneEvent;

        /// <summary>Pazarda satılan tip: tipin pazar ağırlığına göre (çoban köpeği, ambar kedisi, beygir en sık).</summary>
        public string MarketType(string species, Random rng = null)
        {
            rng = rng ?? _geneRng;
            double total = 0;
            foreach (var t in GeneticsCatalog.TypesOf(species)) if (t.Market) total += t.MarketWeight;
            if (total <= 0) return GeneticsCatalog.DefaultType(species)?.Id;
            double r = rng.NextDouble() * total;
            foreach (var t in GeneticsCatalog.TypesOf(species))
            {
                if (!t.Market) continue;
                r -= t.MarketWeight;
                if (r <= 0) return t.Id;
            }
            return GeneticsCatalog.DefaultType(species)?.Id;
        }

        /// <summary>Genomlu türde rastgele genom atar (satın alma, eski kayıt). Tip verilmezse pazar tipi.</summary>
        internal void AssignGenome(Animal a, string typeId = null)
        {
            if (a == null || !a.Species.HasGenetics) return;
            var type = GeneticsCatalog.Type(typeId);
            if (type == null || type.Species != a.Species.Id) typeId = MarketType(a.Species.Id);
            a.Genome = GeneticsService.Random(a.Species.Id, typeId, a.Lineage, a.IsFemale, _geneRng);
        }

        /// <summary>Oyuncunun seçtiği görünüm (belge 31-0.5 ve 48 §6.2: satın alınan ya da bulunan hayvanda seçim kalır).</summary>
        public bool SetLook(Animal a, IDictionary<string, string> expressed, string typeId = null)
        {
            if (a == null || !a.Species.HasGenetics || a.Generation > 0) return false;   // doğan yavrunun görünümü genden gelir
            a.Genome = GeneticsService.FromLook(a.Species.Id, typeId ?? a.TypeId, expressed, a.Lineage, a.IsFemale, _geneRng);
            return true;
        }

        /// <summary>Başlangıç hayvanlarına hazır genom: Alex (kedi) ve Bran (köpek).</summary>
        public void ApplyPreset(Animal a, string preset)
        {
            if (a == null) return;
            if (preset == "alex" && a.Species.Id == GeneticsCatalog.Cat) { a.Genome = GeneticsService.Alex(a.IsFemale, _geneRng); a.Name = "Alex"; }
            else if (preset == "bran" && a.Species.Id == GeneticsCatalog.Dog) { a.Genome = GeneticsService.Bran(a.IsFemale, _geneRng); a.Name = "Bran"; }
        }

        internal void RecordPedigree(Animal a)
        {
            if (a == null) return;
            _pedigree[a.Id] = new PedigreeEntry(a.MotherId, a.FatherId, a.Genome?.Inbreeding ?? 0, a.Species.Id);
        }

        private bool LookupPedigree(int id, out int mother, out int father, out double inbreeding)
        {
            if (_pedigree.TryGetValue(id, out var e)) { mother = e.Mother; father = e.Father; inbreeding = e.Inbreeding; return true; }
            mother = father = 0; inbreeding = 0;
            return false;
        }

        /// <summary>İki hayvanın yavrusunun akrabalık katsayısı (eşleştirme uyarısı, §3.4).</summary>
        public double InbreedingOf(int motherId, int fatherId) => GeneticsService.Inbreeding(motherId, fatherId, LookupPedigree);

        /// <summary>Bu hayvanın kayıtlı annesi ve babası (ölmüş ya da satılmış olabilir).</summary>
        public (int mother, int father) ParentsOf(int id) => _pedigree.TryGetValue(id, out var e) ? (e.Mother, e.Father) : (0, 0);

        /// <summary>Türün yaşayan hayvan sayısı (evcil tavanı için).</summary>
        public int CountAlive(string species)
        {
            int n = 0;
            foreach (var a in _animals) if (!a.IsDead && a.Species.Id == species) n++;
            return n;
        }

        public bool AtPetCap(AnimalSpeciesDef species) => species.IsPet && CountAlive(species.Id) >= PetCap;

        /// <summary>Kayıt: "id:anne:baba:katsayı:tür".</summary>
        public string[] ExportPedigree()
        {
            var ids = new List<int>(_pedigree.Keys);
            ids.Sort();
            var list = new List<string>();
            foreach (var id in ids)
            {
                var e = _pedigree[id];
                list.Add(id + ":" + e.Mother + ":" + e.Father + ":" + e.Inbreeding.ToString("0.####", CultureInfo.InvariantCulture) + ":" + e.Species);
            }
            return list.ToArray();
        }

        public void ImportPedigree(string[] data)
        {
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var p = line.Split(':');
                if (p.Length < 3 || !int.TryParse(p[0], out int id) || !int.TryParse(p[1], out int m) || !int.TryParse(p[2], out int f)) continue;
                double i = p.Length > 3 && double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ? x : 0;
                string sp = p.Length > 4 ? p[4] : null;
                if (!_pedigree.ContainsKey(id)) _pedigree[id] = new PedigreeEntry(m, f, i, sp);
            }
        }

        /// <summary>Hüner sorgusu (oyun etkileri için).</summary>
        public static bool HasGift(Animal a, string gift) => a != null && !a.IsDead && a.Genome != null && a.Genome.HasGift(gift);

        /// <summary>Hanede bu hünere sahip yaşayan hayvan var mı.</summary>
        public bool AnyGift(string gift)
        {
            foreach (var a in _animals) if (HasGift(a, gift) && a.IsMature) return true;
            return false;
        }

        /// <summary>Efsane yolu sayacı (belge 48 §6.4): sayar ve eşiğe gelince unvanı verir. Unvan verildiyse true.</summary>
        public bool AddDeed(Animal a, string deed, int amount = 1)
        {
            if (a == null || a.IsDead || amount <= 0) return false;
            a.Deeds[deed] = a.Deed(deed) + amount;
            return PetLegends.TryGrant(a);
        }
    }
}
