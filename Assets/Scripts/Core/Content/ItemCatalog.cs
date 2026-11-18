using System.Collections.Generic;
using System.Text;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Content
{
    /// <summary>
    /// Satın alma menüsü (Sims tarzı) için kategori, oda ve stil seti adları ile eşya kartı özeti.
    /// Sıra menüdeki sıradır. Kimlikler items.json'daki category/rooms/styleSet değerleridir.
    /// </summary>
    public static class ItemCatalog
    {
        public static readonly (string id, string name)[] Categories =
        {
            ("seating", "Oturma"), ("sleep", "Uyku"), ("surfaces", "Masa"), ("heat_light", "Isı ve ışık"),
            ("kitchen", "Mutfak"), ("storage", "Depo"), ("work", "İş"), ("hygiene", "Hijyen"),
            ("decor", "Süs (masa)"), ("decor_wall", "Süs (yer, duvar)"), ("ceiling", "Tavan"), ("rug", "Halı"), ("garden", "Bahçe"), ("barn", "Ahır ve kümes"), ("farm", "Tarım düzenekleri"), ("kids", "Çocuk"), ("structure", "Merdiven ve yapı")
        };

        public static readonly (string id, string name)[] Rooms =
        {
            ("bedroom", "Yatak odası"), ("kitchen", "Mutfak"), ("living", "Oturma"), ("outdoor", "Dış"), ("kids", "Çocuk"), ("barn", "Ahır")
        };

        /// <summary>Stil setleri, kalite sırasıyla.</summary>
        public static readonly string[] StyleSets = { "Köylü", "Zanaatkâr", "Tüccar", "Asil" };

        public static string CategoryName(string id)
        {
            foreach (var c in Categories) if (c.id == id) return c.name;
            return id;
        }

        public static string NeedName(NeedType n)
        {
            switch (n)
            {
                case NeedType.Hunger: return "Açlık";
                case NeedType.Energy: return "Enerji";
                case NeedType.Warmth: return "Isınma";
                case NeedType.Fun: return "Keyif";
                default: return "Hijyen";
            }
        }

        /// <summary>Menüde gösterilen eşyalar: fiyatı olan, kaynak ya da bitki olmayan; kategoriye göre.</summary>
        public static List<ItemDef> InCategory(ContentDatabase content, string category, string room = null, string set = null) =>
            InCategory(content, category, room, set, null);

        /// <summary>Kategori ve süzgeçler (oda, takım/kademe, tarz). Tarz süzgeci D2 (Çatkı, Yörük).</summary>
        public static List<ItemDef> InCategory(ContentDatabase content, string category, string room, string set, string theme) =>
            InCategory(content, category, room, set, theme, null);

        /// <summary>
        /// "craftonly" eşya yalnızca yapılır; ama ShopUnlock'u olan (cam şişe) hane o kilidi açınca katalogda da satılır.
        /// </summary>
        public static bool IsCraftOnly(ItemDef d, Household house) =>
            d.HasTag("craftonly") && !(d.ShopUnlock != null && house != null && house.HasShopUnlock(d.ShopUnlock));

        /// <summary>Gezgin tüccar eşyası bugün alınabilir mi (hane verilmezse kısıt yok).</summary>
        public static bool MerchantAway(ItemDef d, Household house) =>
            d.HasTag(Decor.MerchantTag) && house != null && !house.MerchantInVillage;

        public static List<ItemDef> InCategory(ContentDatabase content, string category, string room, string set, string theme, Household house)
        {
            var list = new List<ItemDef>();
            foreach (var d in content.Items.All)
            {
                if (d.Price <= 0 || d.Placement == ItemPlacement.Resource || d.IsPlant || IsCraftOnly(d, house)) continue;   // düzenekler yalnızca yapılır
                if (MerchantAway(d, house)) continue;   // Kervan eşyası: yalnızca pazar günü
                if (d.Category != category) continue;
                if (room != null && !d.Rooms.Contains(room)) continue;
                if (set != null && d.StyleSet != set) continue;
                if (theme != null && d.StyleTheme != theme) continue;
                list.Add(d);
            }
            list.Sort((a, b) => a.UnlockLevel != b.UnlockLevel ? a.UnlockLevel.CompareTo(b.UnlockLevel) : a.Price.CompareTo(b.Price));
            return list;
        }

        /// <summary>Kart özeti (content verilirse doldurulan ürün adları): "Enerji +60 · Dinlenmiş · Isı 80 · Depo +40 · Işık · 3 yuva · Duvara asılır".</summary>
        /// <summary>Bu dekorun konabildiği yüzey tanımı mı (türü ve boyutu uyan en az bir yuvası var).</summary>
        public static bool SurfaceAccepts(ItemDef surface, ItemDef decor)
        {
            if (surface == null || decor == null || decor.Placement != ItemPlacement.Surface) return false;
            foreach (var slot in surface.SurfaceSlots)
                if (decor.FitsKind(surface.SlotKind(slot)) && decor.DecorSize <= slot.Size) return true;
            return false;
        }

        /// <summary>Dekorun konabildiği satın alınabilir eşyaların adları (en ucuzdan; en çok max tane).</summary>
        public static List<string> FittingSurfaces(ContentDatabase content, ItemDef decor, int max = 4)
        {
            var defs = new List<ItemDef>();
            if (content == null || decor == null) return new List<string>();
            foreach (var d in content.Items.All)
                if (d.Price > 0 && !d.HasTag("craftonly") && SurfaceAccepts(d, decor)) defs.Add(d);
            defs.Sort((a, b) => a.Price.CompareTo(b.Price));
            var names = new List<string>();
            foreach (var d in defs) { if (names.Count >= max) break; names.Add(d.DisplayName); }
            return names;
        }

        public static string Describe(ItemDef def, MoodCatalog moods = null) => Describe(def, moods, null);

        public static string Describe(ItemDef def, MoodCatalog moods, ContentDatabase content)
        {
            moods = moods ?? MoodCatalog.Default;
            var parts = new List<string>();
            int crafts = 0;
            foreach (var i in def.Interactions)
            {
                if (i.IsCraft) { crafts++; continue; }
                var s = new StringBuilder();
                if (i.RestoresNeed) s.Append($"{NeedName(i.Need)} +{i.Restore:0}");
                else s.Append(i.DisplayName);
                if (i.Moodlet != null && moods.TryGet(i.Moodlet, out var m)) s.Append($" ({m.DisplayName})");
                parts.Add(s.ToString());
            }
            if (crafts > 0) parts.Add($"{crafts} tarif");
            if (def.HeatOutput > 0f) parts.Add($"Isı {def.HeatOutput:0}");
            if (def.Storage > 0) parts.Add($"Depo +{def.Storage}");
            if (def.HasTag("light")) parts.Add("Işık");
            if (def.SurfaceSlots.Count > 0) parts.Add($"{def.SurfaceSlots.Count} yuva");
            if (def.IsHousing) parts.Add($"Barınak ({def.HousingCapacity})");
            if (def.RequiresIndoor) parts.Add("Kapalı odaya konur");
            if (def.IsTrough) parts.Add("Yemlik: odadaki hayvanlar yer");
            switch (def.AnimalRole)
            {
                case AnimalRoles.Water: parts.Add("Su (hayvan konforu)"); break;
                case AnimalRoles.Bedding: parts.Add("Altlık (hayvan konforu)"); break;
                case AnimalRoles.Collector: parts.Add("Her sabah ürünü depoya toplar"); break;
                case AnimalRoles.Feeder: parts.Add("Her sabah yemlikleri envanterden doldurur"); break;
                case AnimalRoles.Bell: parts.Add("Dokun: hayvanları içeri çağır / salıver"); break;
            }
            if (def.RequiresNearby != null) parts.Add($"{def.RequiresNearbyName ?? def.RequiresNearby} {def.NearbyRadius} karo yakınında çalışır");
            if (def.AnimalComfort > 0) parts.Add($"Hayvan konforu +{def.AnimalComfort}");
            if (def.WaterSource) parts.Add(def.EffectiveWaterFlow > 0 ? $"Su kaynağı: ark ve boruya günde {def.EffectiveWaterFlow} su verir" : "Su kaynağı (kanal bağlanır; boruya akmaz, kovayla çekilir)");
            if (def.IsFixture) parts.Add(def.FillCapacity > 0 ? $"Tesisat: boruya bağlıyken günde {def.WaterUse} su, kendi kendine yıkanır" : $"Tesisat: boruya bağlıyken günde {def.WaterUse} su{(def.IsTap ? ", kova buradan dolar" : ", kova taşınmaz")}");
            if (def.IsCistern) parts.Add($"Sarnıç: {def.CisternCapacity} su biriktirir (yağmurda +{def.CisternRain}, ağdaki artan su da dolar), günde en çok {(def.CisternFlow > 0 ? def.CisternFlow : ItemDef.DefaultWaterFlow)} verir");
            if (def.WaterHead) parts.Add("Su terazisi: ağa basınç verir, kurşun boru kolonuyla üst kata su çıkar");
            if (def.WaterRadius > 0) parts.Add(def.NeedsWaterSource ? $"Sular ({def.WaterRadius} karo, kaynağa bağlıyken)" : $"Sular ({def.WaterRadius} karo)");
            if (def.PowerSource) parts.Add($"Güç: kaynağa bağlı kanala değince {Work.FarmAutomation.PowerReach}+ karo (mühendislikle artar) yakındaki makineleri çevirir");
            if (def.AutoProcess != null) parts.Add("Güç alınca her gün kendiliğinden işler");
            if (def.WindPower) parts.Add("Rüzgârla güç verir (kanal gerekmez)");
            if (def.WaterGate) parts.Add("Dokun: suyu aç / kes");
            if (def.HarvestRadius > 0) parts.Add($"Güç alınca her sabah {def.HarvestRadius} karo çevresindeki olgun ekini biçer");
            if (def.SeedRadius > 0) parts.Add($"Güç alınca her sabah {def.SeedRadius} karo çevresindeki boş toprağa tohum eker");
            if (def.IceCollect > 0) parts.Add($"Donlu sabahlar {def.IceCollect} buz toplar");
            if (def.HasTag("craftonly")) parts.Add(def.ShopUnlock == "glassworks" ? "Yapılır; köye camcılık gelince çömlekçide de satılır" : "Yalnızca yapılır (tarif)");
            if (def.HasTag(Decor.SealedTag)) parts.Add("Ağzı bağlı: içindeki yiyecek 4 kat yavaş bozulur");
            if (def.HasTag(Decor.MerchantTag)) parts.Add("Gezgin tüccar getirir: yalnızca pazar günü satılır");
            if (def.IsKeepsake) parts.Add("Hatıra: kazanılır, satılmaz; nesiller geçtikçe güzelleşir");
            if (def.IsSeasonal)
            {
                var ss = new List<string>();
                foreach (var s0 in def.DecorSeasons) ss.Add(Life.FestivalService.SeasonName(s0));
                parts.Add($"Mevsim süsü ({string.Join(", ", ss)}); mevsimi geçince solar");
            }
            if (def.Id == "item.family_tree_embroidery") parts.Add("Nesil arttıkça dallanır; evdeki aileye \"Köklerimiz\", yası kısaltır");
            if (def.Id == "item.dowry_chest") parts.Add("Evlenip giden kızla gider; doluysa Soy Mührü ve yakınlık");
            if (def.Placement == ItemPlacement.Wall)
            {
                var bands = new List<string>();
                foreach (var b in def.EffectiveBands) bands.Add(Decor.BandName(b));
                parts.Add($"{(def.WindowMount ? "Pencereye" : "Duvara")} asılır ({string.Join("/", bands)} bant{(def.WallWidth > 1 ? $", {def.WallWidth} karo" : "")})");
            }
            if (def.Insulation > 0f) parts.Add($"Isı kaybını azaltır (%{(1f - def.Insulation) * 100f:0})");
            if (def.IsFillDecor)
            {
                var names = new List<string>();
                foreach (var f in def.FillWith) if (names.Count < 3) names.Add(content != null && content.Items.TryGet(f, out var fd) ? fd.DisplayName.ToLowerInvariant() : f);
                parts.Add($"Doldurulur: {def.FillCount} × {string.Join(", ", names)}{(def.FillWith.Count > 3 ? "..." : "")}");
            }
            if (def.IsCeiling) parts.Add("Tavana asılır");
            if (def.Placement == ItemPlacement.Surface)
            {
                var kinds = new List<string>();
                foreach (var k in def.EffectiveFits) kinds.Add(Decor.KindName(k));
                if (def.FitsKind(Decor.Cover) && def.EffectiveFits.Count == 1)
                {
                    // Örtü: hangi eşyanın örtü yuvası olduğu kartta yazsın (yalnız "örtü" yetmez).
                    var where = content != null ? FittingSurfaces(content, def) : new List<string>();
                    parts.Add(where.Count > 0 ? $"Örtü: {string.Join(", ", where)} üstüne serilir" : "Örtü: örtü yuvası olan masaya serilir");
                }
                else parts.Add($"{string.Join(", ", kinds)} · boyut {Decor.SizeLetter(def.DecorSize)} · yük {def.Weight}");
            }
            if (def.SurfaceSlots.Count > 0 && def.MaxLoad > 0) parts.Add($"Taşır {def.MaxLoad}");
            if (def.Tall) parts.Add("Uzun (önündeki duvarın alt ve orta bandını kapatır)");
            if (def.Beauty > 0) parts.Add($"Güzellik +{def.Beauty}");
            if (def.StyleTheme != null) parts.Add($"Tarz: {Decor.ThemeName(def.StyleTheme)}");
            if (def.IsMirror) parts.Add(def.TarnishDays > 0f ? "Ayna (görünüş değiştirme; kararır, cilalanır)" : "Ayna (görünüş değiştirme)");
            return string.Join(" · ", parts);
        }

        public static string Describe(AnimalSpeciesDef a, ContentDatabase content)
        {
            string house = a.Housing;
            foreach (var d in content.Items.All) if (d.HousingKind == a.Housing) { house = d.DisplayName; break; }
            string prod = a.Produces != null && content.Items.TryGet(a.Produces, out var p)
                ? $"{p.DisplayName} / {a.ProduceEveryDays:0.#} gün" : (a.IsPet ? "Evcil: keyif" : "");
            if (a.Rideable) prod = $"Binek (x{a.RideSpeed:0.#} hız, {a.MatureDays:0} günde büyür, eyer gerekir)";
            return $"{house} · yem {a.FeedPerDay:0.#}/gün · {prod}";
        }
    }
}
