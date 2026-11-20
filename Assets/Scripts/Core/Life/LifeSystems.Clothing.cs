using Thatchwick.Core.Localization;
using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Giyim bağlantıları (belge 48, 51): takım seçimi, ısı ve yağmur, kir ve eskime, dikilen giysinin örneğe dönmesi,
    /// "Hepsini yıka", giyim uyumu tepkileri, evre geçişinde küçülen giysiler ve ilk bayramlık, mühür yüzüğü.
    /// </summary>
    public sealed partial class LifeSystems
    {
        private ClothingService _clothing;
        /// <summary>Giyim sistemi; katalog ContentDatabase.Clothing'den (yoksa boş).</summary>
        public ClothingService Clothing => _clothing ?? (_clothing = CreateClothing());
        /// <summary>Köylü yorumu yapmak için en çok uzaklık (karo).</summary>
        public const int RemarkRange = 3;
        /// <summary>Yeni hane üyesine evden gelen sade giysiler verilir (oyunda açık; eski sınamalar için kapatılabilir).</summary>
        public bool StarterClothes { get; set; } = true;
        private readonly Random _remarkRng = new Random(733);

        private ClothingService CreateClothing()
        {
            var s = new ClothingService(Content.Clothing) { Moods = Content.Moods ?? Thatchwick.Core.Mood.MoodCatalog.Default };
            s.Remark += (villager, target, key) => ClothingRemark?.Invoke(villager, target, key);
            s.Equipped += c => { if (c != null) OddEvents?.OnOutfitChanged(c.Id, Clock.TotalDays); };
            return s;
        }

        private bool HasClothing => Content.Clothing != null && Content.Clothing.Count > 0;

        /// <summary>Takıların işlevleri (belge 48 §5.7): yüzük pazarlığa, küpe gönül kazanmaya, nazarlık kazaya karşı.</summary>
        private double JewelryTradeBonus()
        {
            if (!HasClothing) return 0;
            double best = 0;
            foreach (var c in Household()) if (!c.IsDead) best = Math.Max(best, Clothing.TradeBonus(c));
            return best;
        }
        private double JewelryCourtBonus(Character c) => HasClothing && c != null ? Clothing.CourtBonus(c) : 0.0;
        private double JewelryLuck(Character c) => HasClothing && c != null ? Clothing.AccidentFactor(c) : 1.0;

        /// <summary>Mühür yüzüğünün itibarı (belge 48 §5.7): mühürlü mektup ve sözleşme hanenin itibarını artırır.</summary>
        public const int SealReputation = 2;

        private void SealBonus(Character c, string reasonKey, string noticeKey)
        {
            if (!HasClothing || c == null || !Clothing.HasSeal(c)) return;
            Upbringing.Reputation.Add(SealReputation, Text(LocText.Of(reasonKey)));
            Say(LocText.Of(noticeKey, ("name", c.Name), ("rep", SealReputation)), NoticeLevel.Info, c.Id);
        }

        /// <summary>
        /// Envanterdeki takıyı (dükkândan ya da tüccardan alınan) karaktere takar. Takılamıyorsa eşya envanterde kalır.
        /// </summary>
        public EquipError WearJewelryFromInventory(Character c, string itemId)
        {
            if (!HasClothing || c == null || Content.Clothing.JewelryByItem(itemId) == null || House.Count(itemId) <= 0) return EquipError.Missing;
            var it = Clothing.FromItem(itemId, null, Clock.TotalDays);
            if (it == null) return EquipError.Missing;
            var err = Clothing.WearJewelry(c, it);
            if (err != EquipError.None) { Clothing.Remove(it.Id); return err; }
            House.TryTakeItem(itemId);
            return EquipError.None;
        }

        /// <summary>
        /// Çekmecedeki takıyı eşya olarak envantere koyar: satılabilir ve hediye edilebilir. Yapanın adı ve ustalık
        /// basamağı eşyada taşınmaz. Yadigâr ve mühür yüzüğü envantere konmaz.
        /// </summary>
        public bool JewelryToInventory(ClothingInstance it)
        {
            if (!HasClothing || it == null || !it.IsJewelry || it.OwnerId != 0 || it.Heirloom || it.Diamond) return false;
            var def = Clothing.JewelryOf(it);
            if (def == null || def.IsSeal || string.IsNullOrEmpty(def.ItemId) || !Content.Items.Contains(def.ItemId)) return false;
            if (!Clothing.Remove(it.Id)) return false;
            House.AddItem(def.ItemId, 1);
            return true;
        }

        /// <summary>Envanterde duran, takılabilir takı eşyaları (kimlik ve adet).</summary>
        public IEnumerable<KeyValuePair<string, int>> JewelryInInventory()
        {
            if (!HasClothing) yield break;
            foreach (var j in Content.Clothing.Jewelry)
            {
                int n = string.IsNullOrEmpty(j.ItemId) ? 0 : House.Count(j.ItemId);
                if (n > 0) yield return new KeyValuePair<string, int>(j.ItemId, n);
            }
        }

        /// <summary>Köylü bir hane üyesinin giyimine yorum yaptı (konuşan, hedef, arayüz anahtarı); oyun katmanı balon çizer.</summary>
        public event Action<Character, Character, string> ClothingRemark;

        /// <summary>Hane kademesi (giyim uyumu): itibardan.</summary>
        public ClothingTier HouseholdClothingTier => ClothingService.HouseholdTier(Upbringing.Reputation.Points);

        private bool IsOutdoors(Character c)
        {
            var room = World.Map.Rooms.RoomAt(c.Position);
            return room == null || !room.IsIndoor;
        }

        private void UpdateClothing()
        {
            if (Content.Clothing == null || Content.Clothing.Count == 0) return;
            bool festival = Festivals.TodaysFestival() != null;
            bool rain = RainyToday;
            double frac = Clock.DayFraction;
            bool workHours = frac >= 0.30 && frac <= 0.75;
            var tier = HouseholdClothingTier;
            foreach (var c in Household())
            {
                if (c.IsDead || c.Away) continue;
                if (!Clothing.HasWardrobe(c) && StarterClothes) GiveStarterClothes(c);
                bool outdoors = IsOutdoors(c);
                var ctx = new ClothingContext
                {
                    Festival = festival && workHours, Working = workHours && c.Duty != DutyKind.None,
                    Raining = rain && outdoors, Outdoors = outdoors, Temperature = c.AmbientTemperature
                };
                Clothing.Dress(c, ctx);
                c.ClothingColdResist = Clothing.ColdResist(c);
                c.RainGuard = Clothing.RainGuard(c);
                Clothing.UpdateMoods(c, festival);
                if (!outdoors) Clothing.ChangeAfterRemark(c, tier);
            }
            // Giyim uyumu: köylü yanından geçerken (günde bir tepki).
            foreach (var v in Population.Members)
            {
                if (!v.IsVillager || v.IsDead) continue;
                foreach (var c in Household())
                {
                    if (c.IsDead || c.Away) continue;
                    if (Math.Abs(c.Position.X - v.Position.X) + Math.Abs(c.Position.Y - v.Position.Y) > RemarkRange) continue;
                    Clothing.TryReact(c, v, Villagers.BestAffection(v), tier, festival, Clock.TotalDays, _remarkRng.Next(6));
                }
            }
        }

        /// <summary>Evden gelen sade giysiler: Günlük takım (gövde, alt, ayak) ve Uyku takımı (iç gömlek); bebeğe kundak.</summary>
        public void GiveStarterClothes(Character c)
        {
            if (c == null || Clothing.HasWardrobe(c)) return;
            void Put(string model, OutfitKind kind)
            {
                var it = Clothing.Create(model, null, 0, null, Clock.TotalDays);
                if (it == null) return;
                it.EverWorn = true;
                if (Clothing.Equip(c, it, kind) != EquipError.None) Clothing.Remove(it.Id);
            }
            switch (c.Stage)
            {
                case AgeStage.Baby:
                    Put("swaddle", OutfitKind.Daily); Put("baby_bonnet", OutfitKind.Daily);
                    break;
                case AgeStage.Child:
                    Put("child_shirt", OutfitKind.Daily); Put(c.Sex == Sex.Female ? "child_skirt" : "child_drawers", OutfitKind.Daily); Put("carik", OutfitKind.Daily);
                    break;
                default:
                    if (c.Sex == Sex.Female) { Put("plain_entari", OutfitKind.Daily); Put("linen_kerchief", OutfitKind.Daily); }
                    else { Put("tunic", OutfitKind.Daily); Put("linen_drawers", OutfitKind.Daily); }
                    Put("carik", OutfitKind.Daily);
                    Put("linen_undershirt", OutfitKind.Sleep);
                    break;
            }
            Clothing.WardrobeOf(c).Get(OutfitKind.Daily);
        }

        /// <summary>Tarifle dikilen giysi envantere değil sandığa, dikenin adı ve tarihiyle girer (Kendi emeğimiz).</summary>
        private void OnClothingCrafted(Character c, InteractionDef i)
        {
            if (c == null || i == null || !i.IsCraft || Content.Clothing == null) return;
            foreach (var p in i.Produce)
            {
                if (!Content.Clothing.IsClothingItem(p.ItemId)) continue;
                for (int n = 0; n < p.Count; n++)
                {
                    if (!House.TryTakeItem(p.ItemId)) break;
                    foreach (var mat in i.AllConsumed()) if (Content.Items.TryGet(mat.ItemId, out var md) && md.HasTag("cloth")) ClothUsed += mat.Count;
                    var skill = i.Skill ?? SkillType.Tailoring;
                    int lvl = c.Skills.GetLevel(skill);
                    int q = lvl >= 9 ? 3 : lvl >= 7 ? 2 : lvl >= 4 ? 1 : 0;
                    var it = Clothing.FromItem(p.ItemId, c, Clock.TotalDays, q, ownLabor: true);
                    if (it != null && it.IsJewelry)
                    {
                        it.JewelryStep = Math.Max(it.JewelryStep, lvl >= 9 ? 2 : 1);   // altın (3) ustalıkla düşmez
                        Clothing.WearJewelry(c, it);                                  // yapan takar; takamıyorsa çekmecede kalır
                    }
                }
            }
        }

        private void OnClothingWork(Character c, InteractionDef i)
        {
            if (c == null || i?.Skill == null) return;
            Clothing.AddDirt(c, i.Skill.Value);
        }

        /// <summary>"Hepsini yıka" (kirli sepeti): sabun varsa biri harcanır, kir tamamen gider; yoksa kül suyuyla.</summary>
        public int WashAllClothes(Character c)
        {
            bool soap = House.TryTakeItem("item.soap") || House.TryTakeItem("item.soap_olive") || House.TryTakeItem("item.bay_soap");
            int n = Clothing.WashAll(Household(), soap);
            Say(n > 0 ? $"{c?.Name ?? "Hane"} {n} giysi yıkadı{(soap ? " (sabunla)" : " (kül suyuyla)")}." : "Kirli sepeti boş.", NoticeLevel.Info, c?.Id ?? 0, transient: true);
            return n;
        }

        private void OnClothingStageChanged(Character c, AgeStage before, AgeStage after)
        {
            if (c == null || c.IsVillager || !Clothing.HasWardrobe(c)) return;
            var outgrown = Clothing.OnStageChanged(c);
            if (outgrown.Count > 0) Say($"{c.Name} {outgrown.Count} giysisinden büyüdü; sandıkta kardeşine saklanabilir.", NoticeLevel.Info, c.Id, transient: true);
            if (before == AgeStage.Child && after == AgeStage.Teen)
            {
                // İlk bayramlık hazırlanmışsa (sandıkta, Ergen'in giyebileceği bir bayram takımı) verilir.
                foreach (var it in Clothing.All)
                {
                    if (it.OwnerId != 0 && it.OwnerId != c.Id) continue;
                    var m = Clothing.ModelOf(it);
                    if (m == null || !m.Has("fest") || !ClothingAge.CanWear(m, AgeStage.Teen)) continue;
                    if (Clothing.GiveFirstFeast(c, it)) { Say($"{c.Name} ilk bayramlığını giydi: \"Büyüdüm!\"", NoticeLevel.Info, c.Id); break; }
                }
            }
            if (Clothing.Choose(c, default) == OutfitKind.Daily && !Clothing.WardrobeOf(c).Has(OutfitKind.Daily) && StarterClothes)
            {
                Clothing.WardrobeOf(c).Outfits.Clear();
                GiveStarterClothes(c);
            }
        }

        private void DailyClothing()
        {
            foreach (var c in Household()) if (!c.IsDead) Clothing.DailyWear(c);
            // Ulakla gelen ısmarlamalar
            foreach (var o in Tailor.DueCourier(Clock.TotalDays))
            {
                var who = Population.Find(o.CustomerId);
                var it = Tailor.Deliver(o, who, Clock.TotalDays, MakerName(o.Maker));
                if (it != null) Say($"Ulak, {who?.Name ?? "haneye"} ısmarlanan giysiyi getirdi.", NoticeLevel.Info, who?.Id ?? 0);
            }
        }

        // ================= gardırop ve terzi ekranının işlemleri (belge 48c "Kalanlar: arayüz") =================

        public const string ClothScrapId = "item.cloth_scrap", ThriftyBranch = "branch.tailor_thrifty";

        /// <summary>Giyme engelinin metin anahtarı.</summary>
        public static string EquipErrorKey(EquipError e)
        {
            switch (e)
            {
                case EquipError.Age: return "cloth.err.age";
                case EquipError.AdultCut: return "cloth.err.adult_cut";
                case EquipError.Torn: return "cloth.err.torn";
                case EquipError.JewelryLimit: return "cloth.err.jewelry_limit";
                case EquipError.Outgrown: return "cloth.err.outgrown";
                case EquipError.BareArmed: return "cloth.err.bare_armed";
                case EquipError.NotOwned: return "wardrobe.err.not_owned";
                default: return "wardrobe.err.other";
            }
        }

        /// <summary>Sandıktaki ya da karakterin kendi giysisini takıma koyar (sandıktaysa sahibi olur). Engel yoksa boş döner.</summary>
        public LocText WearClothing(Character c, ClothingInstance it, OutfitKind kind)
        {
            if (!HasClothing || c == null || it == null) return LocText.Of("wardrobe.err.other");
            var err = it.IsJewelry ? Clothing.WearJewelry(c, it) : Clothing.Equip(c, it, kind);
            return err == EquipError.None ? default : LocText.Of(EquipErrorKey(err));
        }

        /// <summary>Takımdan çıkarır. Hiçbir takımında kalmayan giysi sandığa döner (başkası giyebilir).</summary>
        public void TakeOffClothing(Character c, ClothingInstance it, OutfitKind kind)
        {
            if (!HasClothing || c == null || it == null) return;
            if (it.IsJewelry) { Clothing.TakeOffJewelry(c, it); return; }
            Clothing.Unequip(c, it.Id, kind);
            foreach (var o in Clothing.WardrobeOf(c).Outfits.Values) if (o.Contains(it.Id)) return;
            Clothing.Give(it, null);
        }

        /// <summary>Yama (belge 48 §12.5): Terzilik 1, bir kumaş parçası, dayanıklılık +40. Engel yoksa boş döner.</summary>
        public LocText PatchClothing(Character tailor, ClothingInstance it)
        {
            if (!HasClothing || tailor == null || it == null || it.IsJewelry) return LocText.Of("wardrobe.err.other");
            if (it.Torn) return LocText.Of("cloth.err.torn");
            if (it.Durability >= ClothingInstance.MaxDurability) return LocText.Of("wardrobe.err.no_wear");
            if (House.Count(ClothScrapId) <= 0) return LocText.Of("wardrobe.err.no_scrap");
            if (!Clothing.Patch(it)) return LocText.Of("wardrobe.err.other");
            House.TryTakeItem(ClothScrapId);
            tailor.GainSkillXp(SkillType.Tailoring, 4);
            return default;
        }

        /// <summary>Söküm (belge 48 §12.5): giysi kalkar, kumaşın yarısı (Tutumlu makas dalıyla %75'i) kumaş parçası olarak döner. Dönen parça sayısı.</summary>
        public int UnpickClothing(Character tailor, ClothingInstance it)
        {
            if (!HasClothing || it == null || it.IsJewelry || it.Heirloom) return 0;
            int back = Clothing.Unpick(it.Id, tailor != null && tailor.HasBranch(ThriftyBranch));
            if (back > 0 && Content.Items.Contains(ClothScrapId)) House.AddItem(ClothScrapId, back);
            return back;
        }

        /// <summary>Bu kişi için terziye ısmarlanabilen modeller (yaşına uyan, "order" ile edinilen).</summary>
        public IEnumerable<ClothingModelDef> OrderableModels(Character customer)
        {
            if (!HasClothing || customer == null) yield break;
            foreach (var m in Clothing.Catalog.Models)
                if (m.CanAcquire("order") && ClothingAge.CanWear(m, customer.Stage)) yield return m;
        }

        /// <summary>Ismarlama ücreti (ustanın stoğundan malzemeyle); ulak ayrıca.</summary>
        public int OrderQuote(ClothingModelDef m, bool courier)
        {
            if (m == null) return 0;
            Content.Items.TryGet(m.ItemId, out var item);
            double affinity = Villagers.CharacterOf("villager." + TailorOrders.MakerFor(m)) is Character maker ? Villagers.BestAffection(maker) : 0;
            return Tailor.Quote(m, item != null ? item.Price / 2 : 0, false, affinity, courier);
        }

        /// <summary>Ismarlama engeli; yoksa boş. Sipariş vermek için biri şehirde olmalı (usta dükkânında ısmarlanır).</summary>
        public LocText OrderBlocker(Character customer, ClothingModelDef m, bool courier)
        {
            if (!HasClothing || customer == null || m == null || !m.CanAcquire("order")) return LocText.Of("wardrobe.err.other");
            if (!ClothingAge.CanWear(m, customer.Stage)) return LocText.Of("cloth.err.age");
            if (!TailorOrders.TierAllowed(m.Tier, HouseholdClothingTier)) return LocText.Of("tailor.err.tier");
            if (!SomeoneInTown()) return LocText.Of("tailor.err.not_in_town");
            int quote = OrderQuote(m, courier);
            if (House.Akce < quote) return LocText.Of("tailor.err.akce", ("fee", quote));
            return default;
        }

        /// <summary>Ismarlar (boyasız, ustanın malzemesiyle). Engel varsa metni döner, yoksa boş.</summary>
        public LocText PlaceOrder(Character customer, ClothingModelDef m, bool courier)
        {
            var block = OrderBlocker(customer, m, courier);
            if (!block.IsEmpty) return block;
            var o = OrderClothing(customer, m.Id, DyePalette.Natural(m.Material), ownMaterials: false, courier: courier);
            return o != null ? default : LocText.Of("tailor.err.refused");
        }

        /// <summary>Hazır ısmarlamayı dükkândan alır: biri şehirde olmalı (ulakla ısmarlanan kendiliğinden gelir).</summary>
        public LocText PickUpOrder(TailorOrder o)
        {
            if (!HasClothing || o == null || o.Delivered) return LocText.Of("wardrobe.err.other");
            if (!o.Ready(Clock.TotalDays)) return LocText.Of("tailor.err.not_ready", ("day", (long)Math.Ceiling(o.DueDay) + 1));
            if (!SomeoneInTown()) return LocText.Of("tailor.err.not_in_town");
            var who = Population.Find(o.CustomerId);
            if (who == null || who.IsDead) who = FirstAdult();
            return Tailor.Deliver(o, who, Clock.TotalDays, MakerName(o.Maker)) != null ? default : LocText.Of("tailor.err.refused");
        }

        private Character FirstAdult()
        {
            foreach (var c in Household()) if (!c.IsDead && c.Stage >= AgeStage.YoungAdult) return c;
            foreach (var c in Household()) if (!c.IsDead) return c;
            return null;
        }

        /// <summary>Haneden biri (çocuk ve üstü) şehirde mi; bölgesiz haritada (eski kayıt, test) her zaman evet.</summary>
        public bool SomeoneInTown()
        {
            if (!Regions.IsRegional(World.Map)) return true;
            foreach (var c in Household())
                if (!c.IsDead && c.Stage >= AgeStage.Child && Regions.Town.Contains(c.Position)) return true;
            return false;
        }

        private TailorOrders _tailor;
        /// <summary>Terziye ısmarlama, hizmetler, biçki kalıpları, usta alametleri (belge 48 §10-11).</summary>
        public TailorOrders Tailor => _tailor ?? (_tailor = CreateTailor());

        private TailorOrders CreateTailor()
        {
            var t = new TailorOrders(Clothing);
            Upbringing.ExtraXp = (c, s) => t.HeirloomXpFactor(c, s);
            return t;
        }

        private string MakerName(string maker)
        {
            var c = Villagers.CharacterOf("villager." + maker);
            return c?.Name ?? (maker == "riza" ? "Rıza" : maker == "orhan" ? "Orhan" : "Elif");
        }

        /// <summary>Isınlama yoğun dönemi: şenlikten önceki 3 gün (belge 48 §10.2).</summary>
        public bool TailorBusy()
        {
            for (int d = 1; d <= TailorOrders.BusyDaysBeforeFestival; d++)
                if (Festivals.FestivalOn(Clock.DayIndex + d) != null) return true;
            return false;
        }

        /// <summary>Ismarlama verir: ücret akçeden düşer; malzemeyi oyuncu verirse işçilik.</summary>
        public TailorOrder OrderClothing(Character customer, string modelId, ClothColor color, bool ownMaterials, bool courier = false, string cutsKey = "")
        {
            var m = Clothing.Catalog.Get(modelId);
            if (m == null || customer == null) return null;
            Content.Items.TryGet(m.ItemId, out var item);
            double affinity = Villagers.CharacterOf("villager." + TailorOrders.MakerFor(m)) is Character maker ? Villagers.BestAffection(maker) : 0;
            int matValue = item != null ? item.Price / 2 : 0;
            int quote = Tailor.Quote(m, matValue, ownMaterials, affinity, courier);
            if (House.Akce < quote) return null;
            var o = Tailor.Place(customer, modelId, color, ownMaterials, matValue, affinity, HouseholdClothingTier, Clock.TotalDays, TailorBusy(), courier, cutsKey);
            if (o == null) return null;
            House.TrySpend(o.Fee);
            TailorSpent += o.Fee;
            return o;
        }

        /// <summary>Biçki kalıbını (eşya) okuyup öğrenir: kalıp harcanır, karakter o modeli diker; Okuryazarlık 3 ise deftere de yazar.</summary>
        public bool UsePattern(Character c, string modelId)
        {
            var m = Clothing.Catalog.Get(modelId);
            if (c == null || m == null || !TailorOrders.NeedsPattern(m) || Tailor.Knows(c, modelId)) return false;
            if (!House.TryTakeItem("item.sewing_pattern")) return false;
            Tailor.Learn(c, modelId);
            Tailor.Write(c, modelId);
            return true;
        }

        /// <summary>Denge raporu sayaçları (oturum içi): ısmarlamaya ödenen akçe, dikilen giysiye giden kumaş.</summary>
        public int TailorSpent { get; private set; }
        public int ClothUsed { get; private set; }

        /// <summary>Kalıbı gereken giysi tarifinin engeli (belge 48 §11.2).</summary>
        private string ClothingBlocker(Character c, ItemInstance it, InteractionDef i)
        {
            if (i == null || !i.IsCraft || Content.Clothing == null || Content.Clothing.Count == 0) return null;
            foreach (var p in i.Produce)
            {
                var m = Content.Clothing.ByItem(p.ItemId);
                if (m != null && !Tailor.Knows(c, m.Id)) return "Bu giysinin biçki kalıbı yok (bulunur, Elif'ten ya da tüccardan alınır).";
            }
            return null;
        }

        private void OnClothingMastery(Character c, SkillType skill)
        {
            if (c == null || c.IsVillager || Content.Clothing == null || Content.Clothing.Count == 0 || !TailorOrders.HasMastery(skill)) return;
            var it = Tailor.GrantMastery(c, skill, Clock.TotalDays);
            if (it != null) Say($"{c.Name} ustalığının alametini aldı: {Content.Items.Get(Clothing.ModelOf(it).ItemId)?.DisplayName}.", NoticeLevel.Info, c.Id);
        }
    }
}
