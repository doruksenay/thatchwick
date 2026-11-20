using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Clothing
{
    /// <summary>Bir ısmarlama (belge 48 §10): kim, ne, hangi renk ve kesim, kimin malzemesi, ücret, teslim günü.</summary>
    public sealed class TailorOrder
    {
        public int Id;
        /// <summary>Usta: "elif" (terzi), "riza" (saraç), "orhan" (demirci).</summary>
        public string Maker = "elif";
        public int CustomerId;
        public string ModelId;
        public ClothColor Color;
        public ClothColor Trim;
        public string CutsKey = "";
        public bool OwnMaterials;
        public int Fee;
        public double PlacedDay;
        public double DueDay;
        public bool ByCourier;
        public bool Delivered;
        public bool Ready(double today) => !Delivered && today >= DueDay;
    }

    /// <summary>Usta hizmetleri (belge 48 §10.4).</summary>
    public enum TailorService { Dye = 0, Patch = 1, Alter = 2, Measure = 3 }

    /// <summary>
    /// Terziye ısmarlama ve hizmetler (belge 48 §10), biçki kalıpları (§11.2), usta alametleri ve yadigâr (§11.3-11.4).
    /// Ücret ve süreler yer tutucudur; ısmarlama üretimden 2-3 kat pahalıdır ve en çok kalite 2 verir (kalite 3, aile
    /// nakışı, imzalı ustalık eseri yalnızca hanede).
    /// </summary>
    public sealed class TailorOrders
    {
        public const int QualityCap = 2;
        public const double StockMaterialFactor = 1.5;
        public const int CourierFee = 3;
        public const int DyeFee = 5, PatchFee = 3, AlterFee = 10, MeasureFee = 6;
        public const double DiscountAt30 = 0.10;
        public const int PatternsAt = 60, ApprenticeAt = 100;
        public const int BusyDaysBeforeFestival = 3;
        /// <summary>Kalıbı deftere yazmak için Okuryazarlık.</summary>
        public const int WriteLiteracy = 3;
        public const double HeirloomXpBonus = 0.05;

        private readonly ClothingService _clothing;
        private readonly List<TailorOrder> _orders = new List<TailorOrder>();
        private int _nextId = 1;
        // Kalıplar: model → bilen karakter (0 = deftere yazılı, hane bilir).
        private readonly Dictionary<string, HashSet<int>> _patterns = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        private readonly HashSet<string> _written = new HashSet<string>(StringComparer.Ordinal);

        public TailorOrders(ClothingService clothing) { _clothing = clothing ?? throw new ArgumentNullException(nameof(clothing)); }

        public IReadOnlyList<TailorOrder> Orders => _orders;

        // ---------------- ücret ve süre (§10.2) ----------------

        public static int Labor(ClothingTier t) => t == ClothingTier.Noble ? 200 : t == ClothingTier.Merchant ? 70 : t == ClothingTier.Artisan ? 25 : 8;
        public static int Days(ClothingTier t) => t == ClothingTier.Noble ? 5 : t == ClothingTier.Merchant ? 3 : t == ClothingTier.Artisan ? 2 : 1;

        /// <summary>Hane bu kademeyi ısmarlayabilir mi: Tüccar itibar orta (Saygın), Asil yüksek (Tanınmış) ister.</summary>
        public static bool TierAllowed(ClothingTier model, ClothingTier household) =>
            model <= ClothingTier.Artisan || model == ClothingTier.Merchant && household >= ClothingTier.Artisan || model == ClothingTier.Noble && household >= ClothingTier.Merchant;

        /// <summary>Elif hangi ustanın işi: deri ayakkabı ve çizme Rıza'da, gümüş ve taç Orhan'da (§10.6).</summary>
        public static string MakerFor(ClothingModelDef m)
        {
            if (m == null) return null;
            if (m.Skill == SkillType.Leatherwork) return "riza";
            if (m.Skill == SkillType.Smithing) return "orhan";
            return "elif";
        }

        /// <summary>Ücret: işçilik + (Elif'in stoğundan malzeme ×1,5); yakınlık 30+ %10 indirim; ulak 3 akçe.</summary>
        public int Quote(ClothingModelDef m, int materialValue, bool ownMaterials, double affinity, bool courier)
        {
            if (m == null) return 0;
            double fee = Labor(m.Tier) + (ownMaterials ? 0 : Math.Ceiling(materialValue * StockMaterialFactor));
            if (affinity >= 30) fee *= 1 - DiscountAt30;
            return (int)Math.Round(fee, MidpointRounding.AwayFromZero) + (courier ? CourierFee : 0);
        }

        /// <summary>Teslim günü: kademe süresi; şenlikten önceki 3 gün ve köy düğünü öncesi yoğun dönem +1; sıradaki her açık iş +0,5.</summary>
        public double DueDay(ClothingModelDef m, string maker, double today, bool busy)
        {
            int open = 0;
            foreach (var o in _orders) if (!o.Delivered && o.Maker == maker && !o.Ready(today)) open++;
            return Math.Floor(today) + Days(m.Tier) + (busy ? 1 : 0) + 0.5 * open;
        }

        /// <summary>Sipariş verir; engel varsa null. Nadir malzeme (sırma, kırmız) Asil kademede yalnız oyuncu malzemesiyle.</summary>
        public TailorOrder Place(Character customer, string modelId, ClothColor color, bool ownMaterials, int materialValue, double affinity,
            ClothingTier household, double today, bool busy, bool courier = false, string cutsKey = "", ClothColor trim = default)
        {
            var m = _clothing.Catalog.Get(modelId);
            if (customer == null || m == null || !m.CanAcquire("order")) return null;
            if (!TierAllowed(m.Tier, household)) return null;
            if (m.Tier == ClothingTier.Noble && !ownMaterials && (m.HasFlag("silver") || color.Id == DyePalette.Kermes)) return null;
            if (!ClothingAge.CanWear(m, customer.Stage)) return null;
            string maker = MakerFor(m);
            var o = new TailorOrder
            {
                Id = _nextId++, Maker = maker, CustomerId = customer.Id, ModelId = modelId, Color = color, Trim = trim, CutsKey = cutsKey ?? "",
                OwnMaterials = ownMaterials, Fee = Quote(m, materialValue, ownMaterials, affinity, courier), PlacedDay = today,
                DueDay = DueDay(m, maker, today, busy), ByCourier = courier
            };
            _orders.Add(o);
            return o;
        }

        /// <summary>Hazır siparişi teslim eder: kalite 2, yapan usta. Ulakla gelen kendiliğinden, öbürü dükkândan alınır.</summary>
        public ClothingInstance Deliver(TailorOrder o, Character customer, double today, string makerName)
        {
            if (o == null || o.Delivered || !o.Ready(today)) return null;
            var it = _clothing.Create(o.ModelId, o.Color, QualityCap, null, today);
            if (it == null) return null;
            it.MakerName = makerName ?? "";
            it.Trim = o.Trim;
            if (!string.IsNullOrEmpty(o.CutsKey))
            {
                var tmp = new ClothingInstance();
                tmp.ParseCuts(o.CutsKey);
                var m = _clothing.ModelOf(it);
                foreach (var kv in tmp.Cuts)
                    if (m.Cuts.TryGetValue(kv.Key, out var opts) && Array.IndexOf(opts, kv.Value) >= 0 && ClothingAge.CanUseCut(kv.Key, kv.Value, customer?.Stage ?? AgeStage.Adult))
                        it.Cuts[kv.Key] = kv.Value;
            }
            _clothing.Give(it, customer);
            o.Delivered = true;
            return it;
        }

        /// <summary>Ulakla gelecek ve hazır olan siparişler (gün başında).</summary>
        public List<TailorOrder> DueCourier(double today)
        {
            var l = new List<TailorOrder>();
            foreach (var o in _orders) if (o.ByCourier && o.Ready(today)) l.Add(o);
            return l;
        }

        // ---------------- hizmetler (§10.4) ----------------

        public static int ServiceFee(TailorService s, double affinity)
        {
            int f = s == TailorService.Dye ? DyeFee : s == TailorService.Patch ? PatchFee : s == TailorService.Alter ? AlterFee : MeasureFee;
            return affinity >= 30 ? (int)Math.Round(f * (1 - DiscountAt30), MidpointRounding.AwayFromZero) : f;
        }

        /// <summary>Boyatma: hazır giysi boyanır (boya yalnızca koyulaştırır). Boya tutmazsa false.</summary>
        public bool Dye(ClothingInstance it, string dyeColorId, ClothTone tone, bool alum = true)
        {
            if (it == null || it.IsJewelry) return false;
            var r = DyePalette.Mix(it.Color, dyeColorId, tone, out bool took);
            if (!took) return false;
            it.Color = r;
            it.Fixed = alum;
            it.WearCount = 0;
            return true;
        }

        /// <summary>Elif'te yama: dayanıklılığı doldurur (yırtık hariç).</summary>
        public bool Patch(ClothingInstance it)
        {
            if (it == null || it.IsJewelry || it.Torn) return false;
            it.Durability = ClothingInstance.MaxDurability;
            it.Patches++;
            return true;
        }

        /// <summary>Tadilat: kesim değişikliği (modelin izin verdiği ve yaşa uygun seçenek).</summary>
        public bool Alter(ClothingInstance it, CutKind kind, string option, AgeStage wearer)
        {
            var m = _clothing.ModelOf(it);
            if (m == null || !m.Cuts.TryGetValue(kind, out var opts) || Array.IndexOf(opts, option) < 0) return false;
            if (!ClothingAge.CanUseCut(kind, option, wearer)) return false;
            it.Cuts[kind] = option;
            return true;
        }

        /// <summary>Ölçü: küçülen çocuk giysisini bir boy büyütür (bir kez).</summary>
        public bool Measure(ClothingInstance it)
        {
            if (it == null || it.IsJewelry || !it.Outgrown || it.Mark == "measured") return false;
            it.Outgrown = false;
            it.Mark = "measured";
            return true;
        }

        // ---------------- kalıplar (§11.2) ----------------

        /// <summary>
        /// Kalıp gerekir mi: Köylü baştan bilinir, Zanaatkâr beceriyle açılır; Tüccar ve Asil kalıplarının bir kısmı
        /// ("pattern" işaretli: tülbent, saç filesi, şaperon, ipek gömlek, dar ceket, armalı pelerin, düğün kaftanı, mest)
        /// bulunur ya da alınır.
        /// </summary>
        public static bool NeedsPattern(ClothingModelDef m) => m != null && m.HasFlag("pattern") && m.CanAcquire("craft");

        /// <summary>Karakter (ya da hane defteri) kalıbı biliyor mu.</summary>
        public bool Knows(Character c, string modelId)
        {
            var m = _clothing.Catalog.Get(modelId);
            if (!NeedsPattern(m)) return true;
            if (_written.Contains(modelId)) return true;
            return c != null && _patterns.TryGetValue(modelId, out var who) && who.Contains(c.Id);
        }

        public void Learn(Character c, string modelId)
        {
            if (c == null || _clothing.Catalog.Get(modelId) == null) return;
            if (!_patterns.TryGetValue(modelId, out var who)) _patterns[modelId] = who = new HashSet<int>();
            who.Add(c.Id);
        }

        /// <summary>Kalıbı deftere yazar (Okuryazarlık 3): kaybolmaz, çocuklara kalır.</summary>
        public bool Write(Character c, string modelId)
        {
            if (c == null || c.Skills.GetLevel(SkillType.Literacy) < WriteLiteracy || !Knows(c, modelId)) return false;
            return _written.Add(modelId);
        }

        public bool IsWritten(string modelId) => _written.Contains(modelId);

        // ---------------- usta alametleri ve yadigâr (§11.3-11.4) ----------------

        private static readonly Dictionary<SkillType, (string model, string color, string motif)> Marks = new Dictionary<SkillType, (string, string, string)>
        {
            { SkillType.Smithing, ("leather_apron", "black_wool.mid", "") },
            { SkillType.Hunting, ("hunter_hat", "forest_dark", "") },
            { SkillType.AnimalHusbandry, ("kepenek", "white_wool.mid", "koç boynuzu") },
            { SkillType.Riding, ("hooded_cloak", "true_indigo.dark", "yıldız") },
            { SkillType.Farming, ("straw_hat", "weld_yellow.light", "başak") },
            { SkillType.Fishing, ("oiled_fisher_hood", "slate_blue", "") },
            { SkillType.Medicine, ("long_veil", "bleached_linen.mid", "") },
            { SkillType.Music, ("felt_kulah", "madder_red.dark", "sarmaşık") },
            { SkillType.Cooking, ("cloth_apron", "bleached_linen.mid", "lale") },
            { SkillType.Weaving, ("wool_shawl", "kermes.mid", "hayat ağacı") },
            { SkillType.Leatherwork, ("leather_vest", "brown_wool.dark", "") },
            { SkillType.Tailoring, ("kaftan", "kermes.dark", "lale") },
        };

        public static bool HasMastery(SkillType s) => Marks.ContainsKey(s);

        /// <summary>Usta alameti: 10. seviyeye ulaşan karaktere özel renk ve işlemeli giysi (yadigâr adayı).</summary>
        public ClothingInstance GrantMastery(Character c, SkillType skill, double today)
        {
            if (c == null || !Marks.TryGetValue(skill, out var mk)) return null;
            ClothColor.TryParse(mk.color, out var color);
            var it = _clothing.Create(mk.model, color.IsEmpty ? (ClothColor?)null : color, 3, c, today, true);
            if (it == null) return null;
            it.Mark = "mastery." + skill;
            it.MakerSkill = skill.ToString();
            if (mk.motif.Length > 0) { it.EmbroideryMotif = mk.motif; it.EmbroideryPlace = "yaka"; it.EmbroideryThread = "ipek"; }
            _clothing.Give(it, c);
            return it;
        }

        /// <summary>Yadigâr giysiyi diken ustanın becerisinde +%5 XP (giyiliyken).</summary>
        public double HeirloomXpFactor(Character c, SkillType skill)
        {
            foreach (var it in _clothing.Worn(c))
                if (it.Heirloom && it.MakerId != c.Id && string.Equals(it.MakerSkill, skill.ToString(), StringComparison.Ordinal)) return 1.0 + HeirloomXpBonus;
            return 1.0;
        }

        /// <summary>Mevsim Defteri %100 ödülü (§11.1): mevsim giysisi.</summary>
        public ClothingInstance SeasonGarment(Character c, int season, double today)
        {
            string model = season == 0 ? "flower_crown" : season == 1 ? "straw_hat" : season == 2 ? "wheat_crown" : "knit_scarf";
            var it = _clothing.Create(model, null, 2, null, today);
            if (it == null) return null;
            if (season == 1) it.Trim = new ClothColor(DyePalette.Madder, ClothTone.Mid);
            if (season == 3) { it.Trim = new ClothColor(DyePalette.BrownWool, ClothTone.Dark); it.Material = "knit"; }
            _clothing.Give(it, c);
            return it;
        }

        // ---------------- kayıt ----------------

        public TailorSave Export()
        {
            var s = new TailorSave { nextId = _nextId, written = new List<string>(_written).ToArray() };
            var orders = new List<TailorOrderSave>();
            foreach (var o in _orders)
                orders.Add(new TailorOrderSave
                {
                    id = o.Id, maker = o.Maker, customer = o.CustomerId, model = o.ModelId, color = o.Color.Key, trim = o.Trim.IsEmpty ? "" : o.Trim.Key,
                    cuts = o.CutsKey, own = o.OwnMaterials, fee = o.Fee, placed = o.PlacedDay, due = o.DueDay, courier = o.ByCourier, delivered = o.Delivered
                });
            s.orders = orders.ToArray();
            var pats = new List<string>();
            foreach (var kv in _patterns) foreach (var id in kv.Value) pats.Add(kv.Key + ":" + id);
            s.patterns = pats.ToArray();
            return s;
        }

        public void Import(TailorSave s)
        {
            _orders.Clear(); _patterns.Clear(); _written.Clear(); _nextId = 1;
            if (s == null) return;
            _nextId = Math.Max(1, s.nextId);
            if (s.orders != null)
                foreach (var d in s.orders)
                {
                    if (d == null) continue;
                    var o = new TailorOrder
                    {
                        Id = d.id, Maker = d.maker ?? "elif", CustomerId = d.customer, ModelId = d.model, CutsKey = d.cuts ?? "", OwnMaterials = d.own,
                        Fee = d.fee, PlacedDay = d.placed, DueDay = d.due, ByCourier = d.courier, Delivered = d.delivered
                    };
                    if (ClothColor.TryParse(d.color, out var c)) o.Color = c;
                    if (ClothColor.TryParse(d.trim, out var t)) o.Trim = t;
                    _orders.Add(o);
                    _nextId = Math.Max(_nextId, o.Id + 1);
                }
            if (s.written != null) foreach (var w in s.written) _written.Add(w);
            if (s.patterns != null)
                foreach (var p in s.patterns)
                {
                    int colon = (p ?? "").LastIndexOf(':');
                    if (colon <= 0 || !int.TryParse(p.Substring(colon + 1), out int cid)) continue;
                    string model = p.Substring(0, colon);
                    if (!_patterns.TryGetValue(model, out var who)) _patterns[model] = who = new HashSet<int>();
                    who.Add(cid);
                }
        }
    }

    [Serializable]
    public sealed class TailorSave
    {
        public int nextId;
        public TailorOrderSave[] orders;
        public string[] patterns;   // model:karakter
        public string[] written;
    }

    [Serializable]
    public sealed class TailorOrderSave
    {
        public int id, customer, fee;
        public string maker, model, color, trim, cuts;
        public bool own, courier, delivered;
        public double placed, due;
    }
}
