using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Clothing
{
    /// <summary>Giysi yuvaları (belge 48 §4). Takı ayrı tutulur (<see cref="JewelryPlace"/>).</summary>
    public enum ClothingSlot { Body = 0, Work = 1, Legs = 2, Feet = 3, Head = 4, Outer = 5, Neck = 6, Hands = 7, Waist = 8 }

    /// <summary>Kademe (34 B9): Köylü, Zanaatkâr, Tüccar, Asil.</summary>
    public enum ClothingTier { Peasant = 0, Artisan = 1, Merchant = 2, Noble = 3 }

    /// <summary>Takının takıldığı yer (belge 48 §5.7). Broş pelerine, nazar boncuğu yakaya iliştirilir; ikisi 5 takı sınırına girmez.</summary>
    public enum JewelryPlace { Ear = 0, Nose = 1, Neck = 2, Wrist = 3, Hand = 4, Brooch = 5, Charm = 6 }

    /// <summary>Kesim seçeneği grubu (maske; belge 48 §6.6).</summary>
    public enum CutKind { Sleeve = 0, Collar = 1, Length = 2, Slit = 3 }

    // ---------------- JSON (clothing.json) ----------------
    [Serializable]
    public sealed class ClothingFileDto
    {
        public ClothingModelDto[] models;
        public JewelryDto[] jewelry;
    }

    [Serializable]
    public sealed class CutsDto
    {
        public string[] sleeve;
        public string[] collar;
        public string[] length;
        public string[] slit;
    }

    [Serializable]
    public sealed class ClothingModelDto
    {
        public string id;
        public string item;
        public string slot;
        public string tier;          // K Z T A
        public string style;         // Ç Y O
        public string layer;         // A B C
        public int family;
        public string material;
        public string skill;
        public int level;
        public string[] acquire;     // craft, elif, order, shop:<npc>, merchant, reward, find, dowry
        public string period;        // ok | late
        public string[] ages;        // AgeStage adları
        public bool adultCut;
        public bool childSize;
        public string cover;         // full | top | none (baş)
        public string[] functions;   // winter, rain, sun, bee, spark, mud, thorns, apron, fest, wedding, status, maternity, warm2, bib
        public CutsDto cuts;
        public string[] flags;
    }

    [Serializable]
    public sealed class JewelryDto
    {
        public string id;
        public string item;
        public string place;
        public string skill;
        public int level;
        public string[] acquire;
        /// <summary>Etki ailesi (earring, evil_eye, gem_necklace...); boşsa kimlik. Altın ve taşlı türler temel takının etkisini taşır.</summary>
        public string kind;
        /// <summary>Malzeme basamağı tabanı: 0 yok (ustalığa göre 1-2), 3 altın.</summary>
        public int step;
        /// <summary>Değerli taşlı (etkili basamak +1).</summary>
        public bool gem;
    }

    // ---------------- tanımlar ----------------
    public sealed class ClothingModelDef
    {
        public string Id;
        public string ItemId;
        public ClothingSlot Slot;
        public ClothingTier Tier;
        public string Style;
        public string Layer;
        public int Family;
        public string Material;
        public SkillType Skill;
        public int Level;
        public string[] Acquire = Array.Empty<string>();
        public bool LatePeriod;
        public HashSet<AgeStage> Ages = new HashSet<AgeStage>();
        /// <summary>Model kendisi açık kesim mi (Genç yetişkin ve üstü).</summary>
        public bool AdultCut;
        /// <summary>Çocuk mankeninde çizilebilir (A katmanı maske ya da çocuk boyu çizilmiş B parçası).</summary>
        public bool ChildSize;
        public string Cover = "none";
        public HashSet<string> Functions = new HashSet<string>(StringComparer.Ordinal);
        public HashSet<string> Flags = new HashSet<string>(StringComparer.Ordinal);
        public Dictionary<CutKind, string[]> Cuts = new Dictionary<CutKind, string[]>();

        public bool Has(string function) => Functions.Contains(function);
        public bool HasFlag(string flag) => Flags.Contains(flag);
        public bool IsBabyOnly => Ages.Count == 1 && Ages.Contains(AgeStage.Baby);
        public bool IsMaternity => Has("maternity");
        /// <summary>Kolsuz üst (yelek, kolsuz üstlük): altında gömlek yoksa açık kesim sayılır.</summary>
        public bool Sleeveless => HasFlag("sleeveless");
        public bool CanAcquire(string way)
        {
            foreach (var a in Acquire)
                if (string.Equals(a, way, StringComparison.Ordinal) || a.StartsWith(way + ":", StringComparison.Ordinal)) return true;
            return false;
        }
        public string NameKey => ItemId + ".name";
    }

    public sealed class JewelryDef
    {
        public string Id;
        public string ItemId;
        public JewelryPlace Place;
        public SkillType Skill;
        public int Level;
        public string[] Acquire = Array.Empty<string>();
        /// <summary>Etki ailesi: altın küpe de "earring" sayılır.</summary>
        public string Kind;
        /// <summary>Malzeme basamağı tabanı (altın 3); ustalık bunu düşürmez.</summary>
        public int BaseStep;
        /// <summary>Değerli taşlı.</summary>
        public bool Gem;
        /// <summary>5 takı sınırına girer mi (broş ve nazar boncuğu girmez).</summary>
        public bool CountsTowardLimit => Place != JewelryPlace.Brooch && Place != JewelryPlace.Charm;
        public bool IsSeal => Id == "seal_ring";
    }

    /// <summary>
    /// Giysi kataloğu (clothing.json). Her model bir eşyadır (category "clothing"): ad ve fiyat items.json'dan,
    /// yerelleştirme eşya adıyla. Kesim adları, renkler ve tonlar arayüz tablosunda (<c>cloth.*</c>).
    /// </summary>
    public sealed class ClothingCatalog
    {
        private readonly Dictionary<string, ClothingModelDef> _models = new Dictionary<string, ClothingModelDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, ClothingModelDef> _byItem = new Dictionary<string, ClothingModelDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, JewelryDef> _jewelry = new Dictionary<string, JewelryDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, JewelryDef> _jewelryByItem = new Dictionary<string, JewelryDef>(StringComparer.Ordinal);

        public IEnumerable<ClothingModelDef> Models => _models.Values;
        public IEnumerable<JewelryDef> Jewelry => _jewelry.Values;
        public int Count => _models.Count;
        public int JewelryCount => _jewelry.Count;

        public ClothingModelDef Get(string id) => id != null && _models.TryGetValue(id, out var m) ? m : null;
        public ClothingModelDef ByItem(string itemId) => itemId != null && _byItem.TryGetValue(itemId, out var m) ? m : null;
        public JewelryDef GetJewelry(string id) => id != null && _jewelry.TryGetValue(id, out var j) ? j : null;
        public JewelryDef JewelryByItem(string itemId) => itemId != null && _jewelryByItem.TryGetValue(itemId, out var j) ? j : null;
        public bool IsClothingItem(string itemId) => ByItem(itemId) != null || JewelryByItem(itemId) != null;

        public IEnumerable<ClothingModelDef> InSlot(ClothingSlot s)
        {
            foreach (var m in _models.Values) if (m.Slot == s) yield return m;
        }

        public static ClothingTier ParseTier(string t)
        {
            switch (t)
            {
                case "Z": return ClothingTier.Artisan;
                case "T": return ClothingTier.Merchant;
                case "A": return ClothingTier.Noble;
                default: return ClothingTier.Peasant;
            }
        }

        public static bool TryParseSlot(string s, out ClothingSlot slot)
        {
            switch (s)
            {
                case "body": slot = ClothingSlot.Body; return true;
                case "work": slot = ClothingSlot.Work; return true;
                case "legs": slot = ClothingSlot.Legs; return true;
                case "feet": slot = ClothingSlot.Feet; return true;
                case "head": slot = ClothingSlot.Head; return true;
                case "outer": slot = ClothingSlot.Outer; return true;
                case "neck": slot = ClothingSlot.Neck; return true;
                case "hands": slot = ClothingSlot.Hands; return true;
                case "waist": slot = ClothingSlot.Waist; return true;
                default: slot = ClothingSlot.Body; return false;
            }
        }

        private static bool TryParsePlace(string s, out JewelryPlace p)
        {
            switch (s)
            {
                case "ear": p = JewelryPlace.Ear; return true;
                case "nose": p = JewelryPlace.Nose; return true;
                case "neck": p = JewelryPlace.Neck; return true;
                case "wrist": p = JewelryPlace.Wrist; return true;
                case "hand": p = JewelryPlace.Hand; return true;
                case "brooch": p = JewelryPlace.Brooch; return true;
                case "charm": p = JewelryPlace.Charm; return true;
                default: p = JewelryPlace.Neck; return false;
            }
        }

        /// <summary>DTO'dan kurar; <paramref name="content"/> verilirse eşya kimlikleri denetlenir.</summary>
        public static ClothingCatalog Map(ClothingFileDto file, ContentDatabase content, List<string> errors)
        {
            var cat = new ClothingCatalog();
            if (file?.models != null)
                foreach (var d in file.models)
                {
                    if (d == null || string.IsNullOrWhiteSpace(d.id)) { errors.Add("clothing: kimliksiz model."); continue; }
                    if (cat._models.ContainsKey(d.id)) { errors.Add($"clothing: '{d.id}' iki kez."); continue; }
                    if (!TryParseSlot(d.slot, out var slot)) errors.Add($"clothing {d.id}: yuva '{d.slot}' tanınmadı.");
                    if (!Enum.TryParse(d.skill, out SkillType skill)) { errors.Add($"clothing {d.id}: beceri '{d.skill}' yok."); skill = SkillType.Tailoring; }
                    if (content != null && !content.Items.Contains(d.item)) errors.Add($"clothing {d.id}: eşya '{d.item}' yok.");
                    var m = new ClothingModelDef
                    {
                        Id = d.id, ItemId = d.item, Slot = slot, Tier = ParseTier(d.tier), Style = d.style ?? "O", Layer = d.layer ?? "A",
                        Family = d.family, Material = d.material ?? "wool", Skill = skill, Level = Math.Max(1, d.level),
                        Acquire = d.acquire ?? Array.Empty<string>(), LatePeriod = d.period == "late", AdultCut = d.adultCut,
                        ChildSize = d.childSize, Cover = string.IsNullOrEmpty(d.cover) ? "none" : d.cover
                    };
                    if (d.ages != null)
                        foreach (var a in d.ages)
                            if (Enum.TryParse(a, out AgeStage st)) m.Ages.Add(st); else errors.Add($"clothing {d.id}: yaş '{a}' tanınmadı.");
                    if (m.Ages.Count == 0) errors.Add($"clothing {d.id}: yaş aralığı boş.");
                    if (d.functions != null) foreach (var f in d.functions) m.Functions.Add(f);
                    if (d.flags != null) foreach (var f in d.flags) m.Flags.Add(f);
                    if (d.cuts != null)
                    {
                        if (d.cuts.sleeve != null && d.cuts.sleeve.Length > 0) m.Cuts[CutKind.Sleeve] = d.cuts.sleeve;
                        if (d.cuts.collar != null && d.cuts.collar.Length > 0) m.Cuts[CutKind.Collar] = d.cuts.collar;
                        if (d.cuts.length != null && d.cuts.length.Length > 0) m.Cuts[CutKind.Length] = d.cuts.length;
                        if (d.cuts.slit != null && d.cuts.slit.Length > 0) m.Cuts[CutKind.Slit] = d.cuts.slit;
                    }
                    // Dönem kuralı (belge 51 §2.5): diz üstü etek ve entari yok.
                    if ((d.family == 2 || slot == ClothingSlot.Legs && d.layer == "C") && m.Cuts.TryGetValue(CutKind.Length, out var lens))
                        foreach (var l in lens)
                            if (l == "knee_above") errors.Add($"clothing {d.id}: diz üstü etek/entari dönemde yok.");
                    // Çocuk ancak çocuk boyu varsa giyer.
                    if (m.Ages.Contains(AgeStage.Child) && !m.ChildSize) errors.Add($"clothing {d.id}: çocuk giyiyor ama çocuk boyu yok.");
                    cat._models[m.Id] = m;
                    if (m.ItemId != null) cat._byItem[m.ItemId] = m;
                }
            if (file?.jewelry != null)
                foreach (var d in file.jewelry)
                {
                    if (d == null || string.IsNullOrWhiteSpace(d.id)) continue;
                    if (!TryParsePlace(d.place, out var place)) errors.Add($"takı {d.id}: yer '{d.place}' tanınmadı.");
                    if (!Enum.TryParse(d.skill, out SkillType skill)) skill = SkillType.Smithing;
                    if (content != null && !content.Items.Contains(d.item)) errors.Add($"takı {d.id}: eşya '{d.item}' yok.");
                    var j = new JewelryDef { Id = d.id, ItemId = d.item, Place = place, Skill = skill, Level = Math.Max(1, d.level), Acquire = d.acquire ?? Array.Empty<string>(), Kind = string.IsNullOrEmpty(d.kind) ? d.id : d.kind, BaseStep = Math.Max(0, Math.Min(3, d.step)), Gem = d.gem };
                    cat._jewelry[j.Id] = j;
                    if (j.ItemId != null) cat._jewelryByItem[j.ItemId] = j;
                }
            return cat;
        }
    }

    /// <summary>
    /// Yaşa göre giyim (belge 51 §2): tek fonksiyon. Yaratma ekranı, dolap, ısmarlama, köylü görünümü, zar ve elmas
    /// paketleri buradan okur. Açık kesim <see cref="AgeStages.IsAdultContentAge"/> ile (Genç yetişkin ve üstü).
    /// </summary>
    public static class ClothingAge
    {
        /// <summary>Açık kesim seçenekleri: kayık ve derin yaka, yüksek yırtmaç.</summary>
        public static bool IsAdultCutOption(CutKind kind, string option) =>
            kind == CutKind.Collar && (option == "boat" || option == "deep") || kind == CutKind.Slit && option == "high";

        /// <summary>Model bu evrede giyilebilir mi (kesim seçenekleri hariç).</summary>
        public static bool CanWear(ClothingModelDef m, AgeStage stage)
        {
            if (m == null || !m.Ages.Contains(stage)) return false;
            if (stage == AgeStage.Child && !m.ChildSize) return false;
            if (m.AdultCut && !AgeStages.IsAdultContentAge(stage)) return false;
            return true;
        }

        /// <summary>Kesim seçeneği bu evrede seçilebilir mi.</summary>
        public static bool CanUseCut(CutKind kind, string option, AgeStage stage) =>
            !IsAdultCutOption(kind, option) || AgeStages.IsAdultContentAge(stage);

        /// <summary>Takım açık kesim sayılır mı: kolsuz üst (yelek, kolsuz üstlük) altında gömlek olmadan.</summary>
        public static bool IsBareArmed(ClothingModelDef body, ClothingModelDef work) => body == null && work != null && work.Sleeveless;

        /// <summary>Takı bu evrede takılabilir mi: bebeğe yalnız nazar boncuğu; çocuğa cam boncuk ve nazar boncuğu.</summary>
        public static bool CanWear(JewelryDef j, AgeStage stage)
        {
            if (j == null) return false;
            if (stage == AgeStage.Baby) return j.Place == JewelryPlace.Charm;
            if (stage == AgeStage.Child) return j.Place == JewelryPlace.Charm || j.Id == "glass_beads";
            return true;
        }
    }
}
