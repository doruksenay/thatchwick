using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Clothing
{
    /// <summary>Giyme engeli.</summary>
    public enum EquipError { None = 0, Missing, NotOwned, Age, AdultCut, Torn, WrongSlot, JewelryLimit, Outgrown, BareArmed }

    /// <summary>Giyim uyumu tepkisi (belge 48 §12.3).</summary>
    public enum OutfitReaction { None = 0, Praise = 1, Overreach = 2, Sloppy = 3 }

    /// <summary>Takım seçimi için anlık durum (LifeSystems doldurur).</summary>
    public struct ClothingContext
    {
        public bool Festival;      // şenlik, düğün, cenaze, görücü
        public bool Working;       // atanmış iş saatinde ya da beceri eyleminde
        public bool Raining;
        public bool Outdoors;
        public float Temperature;
    }

    /// <summary>
    /// Giyim sistemi (belge 48 §6, §9-12; belge 51 §2): giysi örnekleri, dolaplar ve takımlar, kendiliğinden giyinme,
    /// işlev (ısı, yağmur, iş tehlikeleri), takılar, eskime, yama, söküm, kir ve çamaşır, giyim uyumu ve moodlet'ler.
    /// Çamaşır ve yama yalnızca oyuncu komutuyla (karar 4): hizmetçi ve Ev görevi yapmaz.
    /// </summary>
    public sealed class ClothingService
    {
        // ---- sabitler (yer tutucu; belge 48 §12) ----
        public const float ColdTemperature = 8f;
        public const float WarmthPerPoint = 0.03f;
        public const float MaxColdResist = 0.5f;
        public const float RainPerPoint = 0.08f;
        public const float MaxRainGuard = 0.9f;
        public const float PatchAmount = 40f;
        public const double ChildWearFactor = 1.5;
        public const int MaxJewelry = 5;
        public const int MaxRings = 2;
        public const int MaxJewelryJoy = 4;
        public const double TrustedAffinity = 60;
        public const double PraiseLongFactor = 1.5;
        public const double ScornedFactor = 0.5;
        public const double ShyShameFactor = 1.5;
        public const string ShyTrait = "trait.introvert";

        public const string NewClothes = "eff_new_clothes", SelfSewn = "eff_self_sewn", OurLabor = "eff_our_labor", FestiveDress = "eff_festive_dress",
            Patched = "eff_patched", Ancestors = "eff_ancestors", Confident = "eff_confident", Ashamed = "eff_ashamed_dress",
            DirtyClothes = "eff_dirty_clothes", HandMeDownMood = "eff_hand_me_down", MasteryMood = "eff_mastery_mark", GrownUp = "eff_grown_up", Comfy = "eff_comfy", ComfyPlus = "eff_comfy_plus";
        public const string JewelryMood = "mood.jewelry";

        public ClothingCatalog Catalog { get; }
        public MoodCatalog Moods { get; set; } = MoodCatalog.Default;

        private readonly Dictionary<int, ClothingInstance> _items = new Dictionary<int, ClothingInstance>();
        private readonly Dictionary<int, Wardrobe> _wardrobes = new Dictionary<int, Wardrobe>();
        private readonly Dictionary<int, long> _reactionDay = new Dictionary<int, long>();
        private int _nextId = 1;

        /// <summary>Köylü yorumu (konuşma balonu): konuşan, hedef, arayüz anahtarı.</summary>
        public event Action<Character, Character, string> Remark;

        public ClothingService(ClothingCatalog catalog) { Catalog = catalog ?? new ClothingCatalog(); }

        public IEnumerable<ClothingInstance> All => _items.Values;
        public int Count => _items.Count;
        public ClothingInstance Get(int id) => _items.TryGetValue(id, out var i) ? i : null;
        public ClothingModelDef ModelOf(ClothingInstance i) => i == null || i.IsJewelry ? null : Catalog.Get(i.ModelId);
        public JewelryDef JewelryOf(ClothingInstance i) => i == null || !i.IsJewelry ? null : Catalog.GetJewelry(i.ModelId);

        public Wardrobe WardrobeOf(Character c) => WardrobeOf(c?.Id ?? 0);
        public Wardrobe WardrobeOf(int id)
        {
            if (!_wardrobes.TryGetValue(id, out var w)) _wardrobes[id] = w = new Wardrobe(id);
            return w;
        }
        public bool HasWardrobe(Character c) => c != null && _wardrobes.TryGetValue(c.Id, out var w) && w.Outfits.Count > 0;

        // ---------------- üretim ----------------

        /// <summary>Yeni giysi. Renk verilmezse malzemenin boyasız rengi.</summary>
        public ClothingInstance Create(string modelId, ClothColor? color = null, int quality = 0, Character maker = null, double day = 0, bool ownLabor = false)
        {
            var m = Catalog.Get(modelId);
            if (m == null) return null;
            var it = new ClothingInstance
            {
                Id = _nextId++, ModelId = modelId, Material = m.Material, Color = color ?? DyePalette.Natural(m.Material),
                Quality = Math.Max(0, Math.Min(ClothingInstance.MaxQuality, quality)), MakerId = maker?.Id ?? 0, MakerName = maker?.Name ?? "",
                MadeDay = day, OwnLabor = ownLabor
            };
            foreach (var kv in m.Cuts) it.Cuts[kv.Key] = DefaultCut(kv.Key, kv.Value);
            _items[it.Id] = it;
            return it;
        }

        private static string DefaultCut(CutKind k, string[] options)
        {
            string want = k == CutKind.Sleeve ? "long" : k == CutKind.Collar ? "round" : k == CutKind.Slit ? "none" : null;
            if (want != null) foreach (var o in options) if (o == want) return o;
            foreach (var o in options) if (!ClothingAge.IsAdultCutOption(k, o)) return o;
            return options[0];
        }

        public ClothingInstance CreateJewelry(string jewelryId, int step = 1, bool gem = false, Character maker = null, double day = 0)
        {
            var j = Catalog.GetJewelry(jewelryId);
            if (j == null) return null;
            var it = new ClothingInstance
            {
                Id = _nextId++, ModelId = jewelryId, IsJewelry = true, JewelryStep = Math.Max(0, Math.Min(3, step)), Gem = gem,
                MakerId = maker?.Id ?? 0, MakerName = maker?.Name ?? "", MadeDay = day, Material = "metal"
            };
            _items[it.Id] = it;
            return it;
        }

        /// <summary>Bir eşyayı (envanterden) giysi örneğine çevirir; giysi değilse null.</summary>
        public ClothingInstance FromItem(string itemId, Character maker = null, double day = 0, int quality = 0, bool ownLabor = false)
        {
            var m = Catalog.ByItem(itemId);
            if (m != null) return Create(m.Id, null, quality, maker, day, ownLabor);
            var j = Catalog.JewelryByItem(itemId);
            return j != null ? CreateJewelry(j.Id, Math.Max(1, j.BaseStep), j.Gem, maker, day) : null;
        }

        public void Give(ClothingInstance it, Character c) { if (it != null) it.OwnerId = c?.Id ?? 0; }

        public IEnumerable<ClothingInstance> OwnedBy(Character c)
        {
            foreach (var i in _items.Values) if (i.OwnerId == (c?.Id ?? -1)) yield return i;
        }

        /// <summary>Örneği tamamen kaldırır (söküm, satış); takımlardan da çıkar.</summary>
        public bool Remove(int id)
        {
            if (!_items.Remove(id)) return false;
            foreach (var w in _wardrobes.Values)
            {
                foreach (var o in w.Outfits.Values) o.Remove(id);
                if (w.RainCloak == id) w.RainCloak = 0;
            }
            return true;
        }

        // ---------------- giyme ----------------

        /// <summary>Giyilebilir mi (yaş, açık kesim, yırtık, küçüldü, takı sınırı).</summary>
        public EquipError CanEquip(Character c, ClothingInstance it, OutfitKind kind)
        {
            if (c == null || it == null) return EquipError.Missing;
            if (it.OwnerId != 0 && it.OwnerId != c.Id) return EquipError.NotOwned;
            var stage = c.Stage;
            if (it.IsJewelry)
            {
                var j = JewelryOf(it);
                if (!ClothingAge.CanWear(j, stage)) return EquipError.Age;
                var o = WardrobeOf(c).Get(kind);
                int count = 0, rings = 0;
                foreach (var id in o.Jewelry)
                {
                    if (id == it.Id) return EquipError.None;
                    var jj = JewelryOf(Get(id));
                    if (jj == null) continue;
                    if (jj.CountsTowardLimit) count++;
                    if (jj.Place == JewelryPlace.Hand) rings++;
                    else if (jj.Place == j.Place && j.Place != JewelryPlace.Hand) return EquipError.None;   // aynı yerdeki değiştirilir
                }
                if (j.CountsTowardLimit && count >= MaxJewelry) return EquipError.JewelryLimit;
                if (j.Place == JewelryPlace.Hand && rings >= MaxRings) return EquipError.JewelryLimit;
                return EquipError.None;
            }
            var m = ModelOf(it);
            if (m == null) return EquipError.Missing;
            if (it.Torn) return EquipError.Torn;
            if (it.Outgrown && !m.Ages.Contains(stage)) return EquipError.Outgrown;
            if (!ClothingAge.CanWear(m, stage)) return EquipError.Age;
            if (it.HasAdultCut() && !AgeStages.IsAdultContentAge(stage)) return EquipError.AdultCut;
            if (m.Sleeveless && !AgeStages.IsAdultContentAge(stage))
            {
                var o = WardrobeOf(c).Get(kind);
                if (!o.Slots.ContainsKey(ClothingSlot.Body)) return EquipError.BareArmed;
            }
            return EquipError.None;
        }

        /// <summary>Bir giysi giyildi (takı hariç): "aynı kıyafet" easter egg'i son değişimi buradan bilir (belge 55 §2b).</summary>
        public event Action<Character> Equipped;

        public EquipError Equip(Character c, ClothingInstance it, OutfitKind kind)
        {
            var err = CanEquip(c, it, kind);
            if (err != EquipError.None) return err;
            it.OwnerId = c.Id;
            var o = WardrobeOf(c).Get(kind);
            if (it.IsJewelry)
            {
                if (o.Jewelry.Contains(it.Id)) return EquipError.None;
                var j = JewelryOf(it);
                if (j.Place != JewelryPlace.Hand)
                    o.Jewelry.RemoveAll(id => JewelryOf(Get(id))?.Place == j.Place);
                o.Jewelry.Add(it.Id);
                return EquipError.None;
            }
            o.Slots[ModelOf(it).Slot] = it.Id;
            Equipped?.Invoke(c);
            return EquipError.None;
        }

        /// <summary>Takımdan çıkarır. Gövde çıkınca ergen ve çocukta kolsuz üst de çıkar (gömleksiz kolsuz üst yok).</summary>
        public void Unequip(Character c, int id, OutfitKind kind)
        {
            var o = WardrobeOf(c).Get(kind);
            o.Remove(id);
            if (c != null && !AgeStages.IsAdultContentAge(c.Stage) && !o.Slots.ContainsKey(ClothingSlot.Body)
                && o.Slots.TryGetValue(ClothingSlot.Work, out var w) && ModelOf(Get(w))?.Sleeveless == true)
                o.Slots.Remove(ClothingSlot.Work);
        }

        /// <summary>Duruma göre takım (belge 48 §6.9). Takımı boşsa Günlük'e düşer.</summary>
        public OutfitKind Choose(Character c, ClothingContext ctx)
        {
            var w = WardrobeOf(c);
            OutfitKind want;
            if (c.Asleep) want = OutfitKind.Sleep;
            else if (ctx.Festival) want = OutfitKind.Festive;
            else if (ctx.Working) want = OutfitKind.Work;
            else if (ctx.Outdoors && ctx.Temperature < ColdTemperature) want = OutfitKind.Winter;
            else want = OutfitKind.Daily;
            if (want == OutfitKind.Daily && c.IsPregnant && w.AutoMaternity && w.Has(OutfitKind.Maternity)) want = OutfitKind.Maternity;
            if (c.Stage == AgeStage.Baby && want != OutfitKind.Sleep && want != OutfitKind.Winter) want = OutfitKind.Daily;
            if (!w.Has(want)) want = c.IsPregnant && w.AutoMaternity && w.Has(OutfitKind.Maternity) ? OutfitKind.Maternity : OutfitKind.Daily;
            return want;
        }

        /// <summary>Şu an giyilenler (yağmurda dış yuva yağmur pelerinidir).</summary>
        public List<ClothingInstance> Worn(Character c, bool raining = false)
        {
            var list = new List<ClothingInstance>();
            if (c == null || !_wardrobes.TryGetValue(c.Id, out var w)) return list;
            if (!w.Outfits.TryGetValue(w.Current, out var o)) o = w.Get(OutfitKind.Daily);
            foreach (var kv in o.Slots)
            {
                if (raining && kv.Key == ClothingSlot.Outer && w.RainCloak != 0) continue;
                var it = Get(kv.Value);
                if (it != null && !it.Torn) list.Add(it);
            }
            if (raining && w.RainCloak != 0 && Get(w.RainCloak) is ClothingInstance rc && !rc.Torn) list.Add(rc);
            foreach (var j in o.Jewelry) if (Get(j) is ClothingInstance ji) list.Add(ji);
            return list;
        }

        /// <summary>Takımı günceller; değiştiyse true. İlk giyişte "Yeni giysi".</summary>
        public bool Dress(Character c, ClothingContext ctx)
        {
            if (c == null || !HasWardrobe(c)) return false;
            var w = WardrobeOf(c);
            var k = Choose(c, ctx);
            bool changed = k != w.Current;
            w.Current = k;
            foreach (var it in Worn(c, ctx.Raining))
                if (!it.EverWorn)
                {
                    it.EverWorn = true;
                    if (!it.Diamond && c.Stage != AgeStage.Baby) c.AddMoodlet(NewClothes, Moods);
                }
            return changed;
        }

        // ---------------- işlev (belge 48 §12.1-12.2) ----------------

        private static float MatHeat(string m)
        {
            switch (m)
            {
                case "linen": case "silk": case "hemp": return 1f;
                case "wool": case "fine_wool": case "knit": return 3f;
                case "felt": return 4f;
                case "leather": return 2f;
                case "fur": return 5f;
                default: return 0f;
            }
        }

        private static float MatRain(string m)
        {
            switch (m)
            {
                case "wool": case "fine_wool": case "knit": case "fur": return 1f;
                case "felt": return 3f;
                case "leather": return 2f;
                default: return 0f;
            }
        }

        public static float SlotFactor(ClothingSlot s)
        {
            switch (s)
            {
                case ClothingSlot.Body: return 1f;
                case ClothingSlot.Outer: return 1.5f;
                case ClothingSlot.Legs: return 0.7f;
                case ClothingSlot.Head: case ClothingSlot.Feet: case ClothingSlot.Work: return 0.5f;
                case ClothingSlot.Hands: case ClothingSlot.Neck: return 0.3f;
                default: return 0f;
            }
        }

        private bool HasBrooch(Character c, bool raining)
        {
            foreach (var it in Worn(c, raining)) if (!it.Diamond && JewelryOf(it)?.Place == JewelryPlace.Brooch) return true;
            return false;
        }

        /// <summary>Isı puanı: malzeme × yuva × kalite (+%25/basamak); elmas giysi saymaz; broş pelerine +%10.</summary>
        public float WarmthPoints(Character c, bool raining = false)
        {
            float w = 0f;
            bool brooch = HasBrooch(c, raining);
            foreach (var it in Worn(c, raining))
            {
                var m = ModelOf(it);
                if (m == null || it.Diamond) continue;
                float p = MatHeat(it.Material ?? m.Material) * SlotFactor(m.Slot) * (1f + 0.25f * it.Quality);
                if (m.Has("warm2")) p += 2f;
                if (brooch && m.Slot == ClothingSlot.Outer) p *= 1.1f;
                w += p;
            }
            return w;
        }

        /// <summary>Soğuğa dayanıklılık (0-0,5); dolabı olmayan karakterde 0 (eski davranış).</summary>
        public float ColdResist(Character c) => Math.Min(MaxColdResist, WarmthPoints(c) * WarmthPerPoint);

        /// <summary>Yağmurda ıslanmayı azaltma (0-1). Yağmur işlevli dış giysi (kepenek, yağmurluk) ıslaklık biriktirmez.</summary>
        public float RainGuard(Character c)
        {
            float r = 0f;
            bool brooch = HasBrooch(c, true);
            foreach (var it in Worn(c, true))
            {
                var m = ModelOf(it);
                if (m == null || it.Diamond) continue;
                if (m.Slot == ClothingSlot.Outer && m.Has("rain")) return 1f;
                float p = MatRain(it.Material ?? m.Material) * SlotFactor(m.Slot) * (1f + 0.25f * it.Quality);
                if (m.Has("rain")) p += 3f;
                if (brooch && m.Slot == ClothingSlot.Outer) p *= 1.1f;
                r += p;
            }
            return Math.Min(MaxRainGuard, r * RainPerPoint);
        }

        /// <summary>İş tehlikesi çarpanı (belge 48 §12.2): bee (arı sokması), spark (yanık), thorns (küçük yara), mud (çamurda yavaşlama).</summary>
        public double HazardFactor(Character c, string hazard)
        {
            if (c == null) return 1.0;
            foreach (var it in Worn(c))
            {
                var m = ModelOf(it);
                if (m == null || it.Diamond || !m.Has(hazard)) continue;
                return hazard == "bee" ? 0.25 : 0.5;
            }
            return 1.0;
        }

        // ---------------- takılar (belge 48 §5.7) ----------------

        private IEnumerable<(JewelryDef def, ClothingInstance it)> WornJewelry(Character c)
        {
            foreach (var it in Worn(c))
            {
                if (!it.IsJewelry || it.Diamond) continue;
                var j = JewelryOf(it);
                if (j != null) yield return (j, it);
            }
        }

        /// <summary>Takılardan keyif (en çok +4).</summary>
        public int JewelryJoy(Character c)
        {
            int joy = 0;
            foreach (var (j, it) in WornJewelry(c))
            {
                switch (j.Kind)
                {
                    case "cord_necklace": joy += it.Gem ? 2 : 1; break;
                    case "glass_beads": joy += c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen ? 3 : 2; break;
                    case "nose_ring": joy += 1; break;
                    case "gem_necklace": joy += 3; break;                          // taşlı gerdanlık
                    default: if (it.EffectiveStep >= 3) joy += 1; break;           // altın takmanın keyfi
                }
            }
            return Math.Min(MaxJewelryJoy, joy);
        }

        /// <summary>Gönül kazanmada yakınlık oranı (küpe: basamak başına %3).</summary>
        public double CourtBonus(Character c)
        {
            double b = 0;
            foreach (var (j, it) in WornJewelry(c)) if (j.Kind == "earring") b += 0.03 * (it.EffectiveStep + 1);
            return b;
        }

        /// <summary>Pazarlıkta Ticaret oranı (yüzük: basamak başına %2, en çok iki yüzük).</summary>
        public double TradeBonus(Character c)
        {
            double b = 0;
            foreach (var (j, it) in WornJewelry(c)) if (j.Place == JewelryPlace.Hand) b += 0.02 * (it.EffectiveStep + 1);
            return b;
        }

        /// <summary>Kaza ve iz olaylarında şans çarpanı (nazar boncuğu −%10, taşlı nazarlık −%20).</summary>
        public double AccidentFactor(Character c)
        {
            double f = 1.0;
            foreach (var (j, it) in WornJewelry(c)) if (j.Kind == "evil_eye") f = Math.Min(f, it.Gem ? 0.8 : 0.9);
            return f;
        }

        public const string JewelryJoyMood = "eff_jewelry_joy", JewelryFineMood = "eff_jewelry_fine";

        /// <summary>Kimsenin takmadığı takılar (hanenin çekmecesi): tezgâhtan yeni çıkan ya da çıkarılan.</summary>
        public IEnumerable<ClothingInstance> UnwornJewelry()
        {
            foreach (var it in _items.Values) if (it.IsJewelry && !it.Diamond && it.OwnerId == 0) yield return it;
        }

        /// <summary>Karakterin sahip olduğu takılar.</summary>
        public IEnumerable<ClothingInstance> JewelryOwnedBy(Character c)
        {
            if (c == null) yield break;
            foreach (var it in _items.Values) if (it.IsJewelry && !it.Diamond && it.OwnerId == c.Id) yield return it;
        }

        /// <summary>
        /// Takıyı karaktere takar: bütün takımlara girer (takı günlük, iş ve bayramda aynı kalır). Günlük takıma
        /// giremiyorsa (yaş, 5 takı ya da iki yüzük sınırı) hiçbirine girmez ve nedeni döner.
        /// </summary>
        public EquipError WearJewelry(Character c, ClothingInstance it)
        {
            if (it == null || !it.IsJewelry) return EquipError.Missing;
            var err = CanEquip(c, it, OutfitKind.Daily);
            if (err != EquipError.None) return err;
            foreach (OutfitKind k in Enum.GetValues(typeof(OutfitKind))) Equip(c, it, k);
            return EquipError.None;
        }

        /// <summary>Takıyı çıkarır ve hanenin çekmecesine koyar (başkası takabilir).</summary>
        public void TakeOffJewelry(Character c, ClothingInstance it)
        {
            if (c == null || it == null || !it.IsJewelry || it.OwnerId != c.Id) return;
            foreach (var o in WardrobeOf(c).Outfits.Values) o.Remove(it.Id);
            it.OwnerId = 0;
        }

        /// <summary>Mühür yüzüğü takılı mı (mektup ve sözleşmede itibar +2).</summary>
        public bool HasSeal(Character c)
        {
            foreach (var (j, _) in WornJewelry(c)) if (j.IsSeal) return true;
            return false;
        }

        /// <summary>Hane reisi değişti: mühür yüzüğü yeni reise geçer (belge 48 §5.7).</summary>
        public int PassSeal(Character oldHead, Character newHead)
        {
            if (newHead == null) return 0;
            int n = 0;
            foreach (var it in _items.Values)
            {
                if (!it.IsJewelry || JewelryOf(it)?.IsSeal != true) continue;
                if (oldHead != null && it.OwnerId != oldHead.Id && it.OwnerId != 0) continue;
                foreach (var w in _wardrobes.Values) foreach (var o in w.Outfits.Values) o.Remove(it.Id);
                it.OwnerId = newHead.Id;
                it.Heirloom = true;
                Equip(newHead, it, OutfitKind.Daily);
                Equip(newHead, it, OutfitKind.Festive);
                n++;
            }
            return n;
        }

        // ---------------- eskime, kir, yama, söküm, çamaşır (belge 48 §12.5-12.6) ----------------

        /// <summary>Gün sonu: giyilen her giysi −1 (İş takımı −2; çocukta ×1,5); şapsız boya 10 giyilişte bir ton solar.</summary>
        public void DailyWear(Character c)
        {
            if (c == null || !HasWardrobe(c)) return;
            var w = WardrobeOf(c);
            double f = c.Stage == AgeStage.Child ? ChildWearFactor : 1.0;
            var seen = new HashSet<int>();
            void Wear(Outfit o, float amount)
            {
                foreach (var id in o.Slots.Values)
                {
                    if (!seen.Add(id)) continue;
                    var it = Get(id);
                    if (it == null || it.Torn) continue;
                    it.Durability = (float)Math.Max(0.0, it.Durability - amount * f);
                    it.WearCount++;
                    if (!it.Fixed && it.WearCount % ClothingInstance.FadeEveryWears == 0) it.Color = DyePalette.Fade(it.Color);
                }
            }
            if (c.Duty != DutyKind.None && w.Has(OutfitKind.Work)) Wear(w.Get(OutfitKind.Work), 2f);
            Wear(w.Get(w.Current == OutfitKind.Work ? OutfitKind.Daily : w.Current), 1f);
        }

        /// <summary>İşin kirlettiği miktar (beceriye göre); 0 = kirletmez.</summary>
        public static float DirtFor(SkillType s)
        {
            switch (s)
            {
                case SkillType.Mining: return 10f;
                case SkillType.Smithing: return 8f;
                case SkillType.Farming: case SkillType.AnimalHusbandry: case SkillType.SoilCare: case SkillType.Masonry: case SkillType.Pottery: return 6f;
                case SkillType.Woodcutting: case SkillType.Leatherwork: case SkillType.Hunting: return 4f;
                case SkillType.Cooking: case SkillType.Baking: return 2f;
                default: return 0f;
            }
        }

        /// <summary>İş kirletir: giyilen her giysiye (çocukta ×1,5). Bez önlük yemek ve fırında kiri tutar.</summary>
        public void AddDirt(Character c, SkillType skill, float scale = 1f)
        {
            float d = DirtFor(skill) * scale;
            if (d <= 0f || c == null || !HasWardrobe(c)) return;
            if ((skill == SkillType.Cooking || skill == SkillType.Baking) && HazardFactor(c, "apron") < 1.0) return;
            if (c.Stage == AgeStage.Child) d *= (float)ChildWearFactor;
            foreach (var it in Worn(c))
                if (!it.IsJewelry) it.Dirt = Math.Min(100f, it.Dirt + d);
        }

        /// <summary>Kirli sepeti: kirli, şu an giyilmeyen giysiler.</summary>
        public List<ClothingInstance> Basket(IEnumerable<Character> members)
        {
            var worn = new HashSet<int>();
            if (members != null) foreach (var m in members) foreach (var it in Worn(m)) worn.Add(it.Id);
            var list = new List<ClothingInstance>();
            foreach (var it in _items.Values) if (it.IsDirty && !worn.Contains(it.Id)) list.Add(it);
            return list;
        }

        /// <summary>Yıka: sabunla kir tamamen gider, kül suyuyla %30'u kalır.</summary>
        public void Wash(ClothingInstance it, bool soap)
        {
            if (it == null || it.IsJewelry) return;
            it.Dirt = soap ? 0f : it.Dirt * 0.3f;
        }

        /// <summary>"Hepsini yıka": sepetteki her şey. Yıkanan sayısı.</summary>
        public int WashAll(IEnumerable<Character> members, bool soap)
        {
            int n = 0;
            foreach (var it in Basket(members)) { Wash(it, soap); n++; }
            return n;
        }

        /// <summary>Yama (Terzilik 1, oyuncu komutuyla): +40 dayanıklılık, yama damgası. Yırtık giysi yamanmaz, sökülür.</summary>
        public bool Patch(ClothingInstance it)
        {
            if (it == null || it.IsJewelry || it.Torn || it.Durability >= ClothingInstance.MaxDurability) return false;
            it.Durability = Math.Min(ClothingInstance.MaxDurability, it.Durability + PatchAmount);
            it.Patches++;
            return true;
        }

        /// <summary>Kumaş birimi (modelin tarifindeki ana kumaş sayısı yaklaşık: yuvaya göre).</summary>
        public static int ClothUnits(ClothingModelDef m)
        {
            if (m == null) return 0;
            switch (m.Slot)
            {
                case ClothingSlot.Outer: return 3;
                case ClothingSlot.Body: case ClothingSlot.Work: case ClothingSlot.Legs: return 2;
                default: return 1;
            }
        }

        /// <summary>Söküm: kumaşın %50'si (Tutumlu makas %75) kumaş parçası olarak döner; giysi kalkar.</summary>
        public int Unpick(int id, bool thrifty)
        {
            var it = Get(id);
            if (it == null || it.IsJewelry) return 0;
            int units = ClothUnits(ModelOf(it)) * 2;   // her kumaş iki parça
            int back = (int)Math.Floor(units * (thrifty ? 0.75 : 0.5) + 1e-9);
            Remove(id);
            return Math.Max(1, back);
        }

        // ---------------- yaş evresi (belge 51 §2.6) ----------------

        /// <summary>Evre değişti: artık giyilemeyen giysiler "küçüldü" olur, takımdan çıkıp sandığa döner (kardeşe saklanır).</summary>
        public List<ClothingInstance> OnStageChanged(Character c)
        {
            var outgrown = new List<ClothingInstance>();
            if (c == null) return outgrown;
            foreach (var it in _items.Values)
            {
                if (it.OwnerId != c.Id || it.IsJewelry) continue;
                var m = ModelOf(it);
                if (m == null || ClothingAge.CanWear(m, c.Stage) && !(it.HasAdultCut() && !AgeStages.IsAdultContentAge(c.Stage))) continue;
                it.Outgrown = true;
                outgrown.Add(it);
            }
            foreach (var it in outgrown)
            {
                foreach (var o in WardrobeOf(c).Outfits.Values) o.Remove(it.Id);
                it.OwnerId = 0;
            }
            return outgrown;
        }

        /// <summary>"Kardeşe sakla": küçülen giysi bir kardeşe ayrılır (yaşı tutuyorsa giyilebilir).</summary>
        public bool KeepForSibling(ClothingInstance it, Character sibling)
        {
            if (it == null || sibling == null) return false;
            it.OwnerId = sibling.Id;
            it.Outgrown = false;
            it.HandMeDown = true;
            return true;
        }

        /// <summary>İlk bayramlık (belge 51 §2.6): Ergen'e geçen çocuğa hazırlanmış bayram takımı; "Büyüdüm" (+6, 2 gün).</summary>
        public bool GiveFirstFeast(Character c, ClothingInstance suit)
        {
            if (c == null || suit == null || c.Stage != AgeStage.Teen) return false;
            if (Equip(c, suit, OutfitKind.Festive) != EquipError.None) return false;
            suit.HeirloomNote = suit.MakerName.Length > 0 ? suit.MakerName : suit.HeirloomNote;
            c.AddMoodlet(GrownUp, Moods);
            return true;
        }

        // ---------------- giyim uyumu (belge 48 §12.3) ----------------

        /// <summary>Hane itibarından giysi kademesi: Bilinmez Köylü, Saygın Zanaatkâr, Tanınmış Tüccar, Ünlü Asil.</summary>
        public static ClothingTier HouseholdTier(int reputation) =>
            reputation >= 600 ? ClothingTier.Noble : reputation >= 300 ? ClothingTier.Merchant : reputation >= 100 ? ClothingTier.Artisan : ClothingTier.Peasant;

        public static ClothingTier TierOf(JewelryDef j, ClothingInstance it)
        {
            int s = it.EffectiveStep;
            return s >= 3 ? ClothingTier.Noble : s == 2 ? ClothingTier.Merchant : s == 1 ? ClothingTier.Artisan : ClothingTier.Peasant;
        }

        /// <summary>Görünen en yüksek kademe (elmas eşya sayılmaz); giysi yoksa null.</summary>
        public ClothingTier? TopTier(Character c)
        {
            ClothingTier? top = null;
            foreach (var it in Worn(c))
            {
                if (it.Diamond) continue;
                ClothingTier t;
                if (it.IsJewelry) { var j = JewelryOf(it); if (j == null) continue; t = TierOf(j, it); }
                else { var m = ModelOf(it); if (m == null) continue; t = m.Tier; }
                if (top == null || t > top) top = t;
            }
            return top;
        }

        /// <summary>Tepki değerlendirmesi (köylünün sözü).</summary>
        public OutfitReaction Evaluate(Character c, ClothingTier household, bool occasion, double today)
        {
            if (c == null || c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child || !HasWardrobe(c)) return OutfitReaction.None;
            var w = WardrobeOf(c);
            var worn = Worn(c);
            bool sloppy = false, praiseable = false;
            foreach (var it in worn)
            {
                if (it.Diamond || it.IsJewelry) continue;
                if (it.IsDirty || it.NeedsPatch) sloppy = true;
                if (!it.LooksWorn && !it.IsDirty && (it.Quality >= 2 || today - it.MadeDay <= 2.0)) praiseable = true;
            }
            if (occasion)
            {
                if (sloppy || w.Current == OutfitKind.Work) return OutfitReaction.Sloppy;
                if (w.Current == OutfitKind.Festive) return OutfitReaction.Praise;
            }
            var top = TopTier(c);
            if (top == null) return OutfitReaction.None;
            int diff = (int)top.Value - (int)household;
            if (diff >= 2) return OutfitReaction.Overreach;
            if (diff >= 0 && praiseable && !sloppy) return OutfitReaction.Praise;
            return OutfitReaction.None;
        }

        private static readonly string[] PraiseKeys = { "cloth.remark.praise.1", "cloth.remark.praise.2", "cloth.remark.praise.3" };
        private static readonly string[] OverKeys = { "cloth.remark.overreach.1", "cloth.remark.overreach.2" };
        private static readonly string[] SloppyKeys = { "cloth.remark.sloppy.1", "cloth.remark.sloppy.2" };

        /// <summary>
        /// Köylü yanından geçerken (günde en çok bir tepki). Övgü "Özgüvenli" (yakınlık 60+ ise ×1,5 süre), yergi "Mahcup"
        /// (küs köylüde yarı süre, içe dönük karakterde ×1,5). Uygulanan tepki döner.
        /// </summary>
        public OutfitReaction TryReact(Character target, Character villager, double affinity, ClothingTier household, bool occasion, double today, int pick = 0)
        {
            if (target == null || villager == null || target.IsDead) return OutfitReaction.None;
            long day = (long)Math.Floor(today);
            if (_reactionDay.TryGetValue(target.Id, out var d) && d == day) return OutfitReaction.None;
            var r = Evaluate(target, household, occasion, today);
            if (r == OutfitReaction.None) return r;
            _reactionDay[target.Id] = day;
            double factor = 1.0;
            string mood;
            string[] keys;
            if (r == OutfitReaction.Praise)
            {
                mood = Confident; keys = PraiseKeys;
                if (affinity >= TrustedAffinity) factor *= PraiseLongFactor;
            }
            else
            {
                mood = Ashamed; keys = r == OutfitReaction.Overreach ? OverKeys : SloppyKeys;
                if (affinity < 0) factor *= ScornedFactor;
                if (target.HasTrait(ShyTrait)) factor *= ShyShameFactor;
                target.Mood.Remove(Confident);
            }
            if (r != OutfitReaction.Praise) target.Mood.Remove(Confident); else target.Mood.Remove(Ashamed);
            AddScaled(target, mood, factor);
            Remark?.Invoke(villager, target, keys[Math.Abs(pick) % keys.Length]);
            return r;
        }

        private void AddScaled(Character c, string id, double factor)
        {
            if (!c.MoodEnabled || !Moods.TryGet(id, out var def)) return;
            c.Mood.Remove(id);
            c.Mood.Add(Clone(def, def.DurationDays * factor));
        }

        /// <summary>Mahcup karakter evdeyken üstünü değiştirir (serbest irade): haddini aşan parçalar çıkar, moodlet biter.</summary>
        public bool ChangeAfterRemark(Character c, ClothingTier household)
        {
            if (c == null || !c.Mood.Has(Ashamed) || !HasWardrobe(c)) return false;
            var w = WardrobeOf(c);
            var o = w.Get(w.Current);
            var drop = new List<int>();
            foreach (var id in o.All())
            {
                var it = Get(id);
                if (it == null) continue;
                ClothingTier t = it.IsJewelry ? TierOf(JewelryOf(it), it) : ModelOf(it)?.Tier ?? ClothingTier.Peasant;
                if ((int)t - (int)household >= 2 || it.IsDirty) drop.Add(id);
            }
            foreach (var id in drop) Unequip(c, id, w.Current);
            c.Mood.Remove(Ashamed);
            return true;
        }

        // ---------------- moodlet'ler (belge 48 §12.4, belge 51 §2.6-2.7) ----------------

        private static MoodletDef Clone(MoodletDef d, double days)
        {
            var m = new MoodletDef
            {
                Id = d.Id, DisplayName = d.DisplayName, Description = d.Description, Value = d.Value, DurationDays = days,
                AllXp = d.AllXp, AllSpeed = d.AllSpeed, ColdResist = d.ColdResist, HungerDecay = d.HungerDecay, EnergyDecay = d.EnergyDecay
            };
            foreach (var kv in d.SkillXp) m.SkillXp[kv.Key] = kv.Value;
            foreach (var kv in d.SkillSpeed) m.SkillSpeed[kv.Key] = kv.Value;
            return m;
        }

        private void Condition(Character c, string id, bool on)
        {
            if (!c.MoodEnabled) return;
            if (!on) { c.Mood.Remove(id); return; }
            if (c.Mood.Has(id) || !Moods.TryGet(id, out var def)) return;
            c.Mood.Add(Clone(def, 0));
        }

        /// <summary>Giyildikçe süren moodlet'ler (Kendi diktiğim, Kendi emeğimiz, Atalarımın izinde, Kirli giysi, Rahatım) ve takı keyfi.</summary>
        public void UpdateMoods(Character c, bool festivalToday)
        {
            if (c == null || c.IsDead || !c.MoodEnabled) return;
            bool self = false, labor = false, heir = false, dirty = false, maternity = false, band = false, handed = false;
            foreach (var it in Worn(c))
            {
                if (it.Diamond) continue;
                if (it.MakerId == c.Id && it.MakerId != 0) self = true;
                if (it.OwnLabor) labor = true;
                if (it.Heirloom) heir = true;
                if (it.IsDirty) dirty = true;
                if (it.HandMeDown && it.MakerId != c.Id) handed = true;
                var m = ModelOf(it);
                if (m != null && m.IsMaternity) { if (m.Slot == ClothingSlot.Body) maternity = true; else band = true; }
            }
            int jewelJoy = JewelryJoy(c);
            Condition(c, JewelryJoyMood, jewelJoy > 0 && jewelJoy < 3);
            Condition(c, JewelryFineMood, jewelJoy >= 3);
            Condition(c, SelfSewn, self);
            Condition(c, OurLabor, labor && !self);
            Condition(c, Ancestors, heir);
            Condition(c, DirtyClothes, dirty);
            Condition(c, HandMeDownMood, handed);
            bool comfy = c.IsPregnant && maternity;
            Condition(c, ComfyPlus, comfy && band);
            Condition(c, Comfy, comfy && !band);
            if (festivalToday && HasWardrobe(c) && WardrobeOf(c).Current == OutfitKind.Festive) c.AddMoodlet(FestiveDress, Moods);
            int joy = JewelryJoy(c);
            var cur = c.Mood.Has(JewelryMood);
            if (joy <= 0) { if (cur) c.Mood.Remove(JewelryMood); }
            else
            {
                c.Mood.Remove(JewelryMood);
                c.Mood.Add(new MoodletDef { Id = JewelryMood, DisplayName = "Takılar", Description = "Taktığı takılar hoşuna gidiyor.", Value = joy, DurationDays = 0 });
            }
        }

        // ---------------- kayıt ----------------

        public ClothingSave Export()
        {
            var s = new ClothingSave { nextId = _nextId };
            var items = new List<ClothingItemSave>();
            foreach (var i in _items.Values)
                items.Add(new ClothingItemSave
                {
                    id = i.Id, model = i.ModelId, jewelry = i.IsJewelry, color = i.Color.Key, trim = i.Trim.IsEmpty ? "" : i.Trim.Key,
                    pattern = i.Pattern ?? "", patternColor = i.PatternColor.IsEmpty ? "" : i.PatternColor.Key, cuts = i.CutsKey(),
                    embPlace = i.EmbroideryPlace ?? "", embMotif = i.EmbroideryMotif ?? "", embThread = i.EmbroideryThread ?? "", buttons = i.Buttons ?? "",
                    quality = i.Quality, durability = i.Durability, dirt = i.Dirt, patches = i.Patches, fixedDye = i.Fixed, wears = i.WearCount,
                    makerId = i.MakerId, maker = i.MakerName ?? "", madeDay = i.MadeDay, ownLabor = i.OwnLabor, heirloom = i.Heirloom,
                    heirloomNote = i.HeirloomNote ?? "", diamond = i.Diamond, outgrown = i.Outgrown, owner = i.OwnerId, everWorn = i.EverWorn,
                    material = i.Material ?? "", step = i.JewelryStep, gem = i.Gem, mark = i.Mark ?? "", makerSkill = i.MakerSkill ?? "", handMeDown = i.HandMeDown
                });
            s.items = items.ToArray();
            var ws = new List<WardrobeSave>();
            foreach (var w in _wardrobes.Values)
            {
                var outfits = new List<OutfitSave>();
                foreach (var kv in w.Outfits)
                {
                    var slots = new List<int>();
                    foreach (var sv in kv.Value.Slots) { slots.Add((int)sv.Key); slots.Add(sv.Value); }
                    outfits.Add(new OutfitSave { kind = (int)kv.Key, slots = slots.ToArray(), jewelry = kv.Value.Jewelry.ToArray() });
                }
                ws.Add(new WardrobeSave { character = w.CharacterId, rainCloak = w.RainCloak, autoMaternity = w.AutoMaternity, current = (int)w.Current, outfits = outfits.ToArray() });
            }
            s.wardrobes = ws.ToArray();
            return s;
        }

        public void Import(ClothingSave s)
        {
            _items.Clear(); _wardrobes.Clear(); _reactionDay.Clear(); _nextId = 1;
            if (s == null) return;
            if (s.items != null)
                foreach (var d in s.items)
                {
                    if (d == null || string.IsNullOrEmpty(d.model)) continue;
                    var i = new ClothingInstance
                    {
                        Id = d.id, ModelId = d.model, IsJewelry = d.jewelry, Pattern = d.pattern ?? "", EmbroideryPlace = d.embPlace ?? "",
                        EmbroideryMotif = d.embMotif ?? "", EmbroideryThread = d.embThread ?? "", Buttons = d.buttons ?? "", Quality = d.quality,
                        Durability = d.durability, Dirt = d.dirt, Patches = d.patches, Fixed = d.fixedDye, WearCount = d.wears, MakerId = d.makerId,
                        MakerName = d.maker ?? "", MadeDay = d.madeDay, OwnLabor = d.ownLabor, Heirloom = d.heirloom, HeirloomNote = d.heirloomNote ?? "",
                        Diamond = d.diamond, Outgrown = d.outgrown, OwnerId = d.owner, EverWorn = d.everWorn, Material = d.material ?? "",
                        JewelryStep = d.step, Gem = d.gem, Mark = d.mark ?? "", MakerSkill = d.makerSkill ?? "", HandMeDown = d.handMeDown
                    };
                    if (ClothColor.TryParse(d.color, out var c)) i.Color = c;
                    if (ClothColor.TryParse(d.trim, out var t)) i.Trim = t;
                    if (ClothColor.TryParse(d.patternColor, out var pc)) i.PatternColor = pc;
                    i.ParseCuts(d.cuts);
                    _items[i.Id] = i;
                    _nextId = Math.Max(_nextId, i.Id + 1);
                }
            _nextId = Math.Max(_nextId, s.nextId);
            if (s.wardrobes != null)
                foreach (var d in s.wardrobes)
                {
                    if (d == null) continue;
                    var w = WardrobeOf(d.character);
                    w.RainCloak = _items.ContainsKey(d.rainCloak) ? d.rainCloak : 0;
                    w.AutoMaternity = d.autoMaternity;
                    w.Current = Enum.IsDefined(typeof(OutfitKind), d.current) ? (OutfitKind)d.current : OutfitKind.Daily;
                    if (d.outfits == null) continue;
                    foreach (var o in d.outfits)
                    {
                        if (o == null || !Enum.IsDefined(typeof(OutfitKind), o.kind)) continue;
                        var outfit = w.Get((OutfitKind)o.kind);
                        if (o.slots != null)
                            for (int k = 0; k + 1 < o.slots.Length; k += 2)
                                if (Enum.IsDefined(typeof(ClothingSlot), o.slots[k]) && _items.ContainsKey(o.slots[k + 1]))
                                    outfit.Slots[(ClothingSlot)o.slots[k]] = o.slots[k + 1];
                        if (o.jewelry != null) foreach (var j in o.jewelry) if (_items.ContainsKey(j)) outfit.Jewelry.Add(j);
                    }
                }
        }
    }

    // ---- kayıt biçimi (JsonUtility uyumlu; alanlar eklemeli) ----
    [Serializable]
    public sealed class ClothingSave
    {
        public int nextId;
        public ClothingItemSave[] items;
        public WardrobeSave[] wardrobes;
    }

    [Serializable]
    public sealed class ClothingItemSave
    {
        public int id;
        public string model;
        public bool jewelry;
        public string color, trim, pattern, patternColor, cuts, embPlace, embMotif, embThread, buttons;
        public int quality;
        public float durability, dirt;
        public int patches;
        public bool fixedDye;
        public int wears, makerId;
        public string maker;
        public double madeDay;
        public bool ownLabor, heirloom;
        public string heirloomNote;
        public bool diamond, outgrown;
        public int owner;
        public bool everWorn;
        public string material;
        public int step;
        public bool gem;
        public string mark;
        public string makerSkill;
        public bool handMeDown;
    }

    [Serializable]
    public sealed class WardrobeSave
    {
        public int character, rainCloak, current;
        public bool autoMaternity;
        public OutfitSave[] outfits;
    }

    [Serializable]
    public sealed class OutfitSave
    {
        public int kind;
        public int[] slots;   // yuva, örnek, yuva, örnek ...
        public int[] jewelry;
    }
}
