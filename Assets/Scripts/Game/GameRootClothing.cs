using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using UnityEngine;

namespace Thatchwick.Game
{
    // Giyim penceresi (belge 48c "Kalanlar: arayüz"; karar 20.11.2026): takımlar, sandık, takılar ve terzi ısmarlaması.
    // UI Toolkit (Ui.ListWindow); kurallar Core'da (LifeSystems.Clothing), burada yalnızca liste kurulur ve düğmeler bağlanır.
    public sealed partial class GameRoot
    {
        private const int WardrobeOutfits = 0, WardrobeChest = 1, WardrobeJewelry = 2, WardrobeTailor = 3;

        private Ui.ListWindow _wardrobeWindow;
        private Character _wardrobeFor;
        private int _wardrobeTab;
        private OutfitKind _wardrobeOutfit = OutfitKind.Daily;
        private string _wardrobeStatus = "";

        private bool WardrobeVisible => _wardrobeWindow != null && _wardrobeWindow.Visible;

        /// <summary>Ortada duran bir UI Toolkit penceresi açık mı (zaman durur, dünya ve IMGUI dokunma almaz).</summary>
        private bool UiWindowOpen => BetVisible || WardrobeVisible || (_cartPanel != null && _cartPanel.Active) || (_relationsPanel != null && _relationsPanel.Active);

        /// <summary>Esc: açık pencereyi kapatır; kapattıysa true.</summary>
        private bool CloseUiWindows()
        {
            if (BetVisible) { _betWindow.Visible = false; return true; }
            if (WardrobeVisible) { _wardrobeWindow.Visible = false; return true; }
            if (_cartPanel != null && _cartPanel.Active) { _cartPanel.Close(); return true; }
            if (_relationsPanel != null && _relationsPanel.Active) { _relationsPanel.Close(); return true; }
            return false;
        }

        private bool ClothingReady => _life != null && _content != null && _content.Clothing != null && _content.Clothing.Count > 0;

        /// <summary>Aile panelindeki "Gardırop" düğmesi: pencereyi o kişi için açar.</summary>
        private void OpenWardrobe(Character c)
        {
            if (c == null || !ClothingReady) return;
            var host = EnsureUi();
            if (host == null) { ShowMessage(T("ui.ach.unavailable")); return; }
            if (_wardrobeWindow == null) _wardrobeWindow = new Ui.ListWindow(host, WardrobeModel, () => T("ui.wardrobe.close"), 600f, 7);
            _wardrobeFor = c;
            _wardrobeStatus = "";
            _familyPanel?.Close();   // IMGUI paneli pencerenin üstüne çizilmesin
            _wardrobeWindow.Visible = true;
            _wardrobeWindow.Refresh();
        }

        private static string OutfitName(OutfitKind k) => T("cloth.outfit." + k.ToString().ToLowerInvariant());
        private static string SlotName(ClothingSlot s) => T("cloth.slot." + s.ToString().ToLowerInvariant());

        private static string TierName(ClothingTier t) =>
            T(t == ClothingTier.Noble ? "ui.styleset.noble" : t == ClothingTier.Merchant ? "ui.styleset.merchant"
                : t == ClothingTier.Artisan ? "ui.styleset.artisan" : "ui.styleset.villager");

        private string ClothName(ClothingInstance it)
        {
            var svc = _life.Clothing;
            string itemId = it.IsJewelry ? svc.JewelryOf(it)?.ItemId : svc.ModelOf(it)?.ItemId;
            return itemId != null ? ItemName(itemId) : it.ModelId;
        }

        private string ClothDetail(ClothingInstance it)
        {
            var parts = new List<string>
            {
                T("ui.wardrobe.detail", ("quality", it.Quality), ("max", ClothingInstance.MaxQuality), ("wear", Mathf.RoundToInt(it.Durability)))
            };
            if (it.Torn) parts.Add(T("ui.wardrobe.flag.torn"));
            else if (it.NeedsPatch) parts.Add(T("ui.wardrobe.flag.needs_patch"));
            if (it.IsDirty) parts.Add(T("ui.wardrobe.flag.dirty"));
            if (it.Patches > 0) parts.Add(T("ui.wardrobe.flag.patches", ("n", it.Patches)));
            if (it.Heirloom) parts.Add(T("ui.wardrobe.flag.heirloom"));
            if (!string.IsNullOrEmpty(it.MakerName)) parts.Add(T("ui.wardrobe.made_by", ("maker", it.MakerName)));
            return string.Join(" · ", parts);
        }

        private string JewelryName(ClothingInstance it)
        {
            string name = ClothName(it);
            if (it.Heirloom && !string.IsNullOrEmpty(it.HeirloomNote)) return T("ui.family.jewelry.heirloom", ("item", name), ("from", it.HeirloomNote));
            return string.IsNullOrEmpty(it.MakerName) ? name : T("ui.family.jewelry.made_by", ("item", name), ("maker", it.MakerName));
        }

        /// <summary>Giyme ve takma sonucunu durum satırına yazar (boşsa temizler).</summary>
        private void WardrobeResult(Thatchwick.Core.Localization.LocText error, string done = "")
        {
            _wardrobeStatus = error.IsEmpty ? done : GameLocalization.R(error);
        }

        private string JewelryErrorText(EquipError err, Character c) =>
            err == EquipError.None ? ""
            : T(err == EquipError.Age ? "ui.family.jewelry.err_age" : err == EquipError.JewelryLimit ? "ui.family.jewelry.err_limit" : "ui.family.jewelry.err_other",
                ("name", c.Name));

        private Ui.ListWindow.Model WardrobeModel()
        {
            var c = _wardrobeFor;
            if (c == null || c.IsDead || c.IsVillager || !ClothingReady) return null;

            var model = new Ui.ListWindow.Model { Title = T("ui.wardrobe.title", ("name", c.Name)), Status = _wardrobeStatus };
            var svc = _life.Clothing;

            int chestCount = 0;
            foreach (var _ in svc.Chest()) chestCount++;
            var pages = model.AddTabRow();
            pages.Add(new Ui.ListWindow.Act(T("ui.wardrobe.tab.outfits"), () => SetWardrobeTab(WardrobeOutfits), _wardrobeTab == WardrobeOutfits));
            pages.Add(new Ui.ListWindow.Act(T("ui.wardrobe.tab.chest", ("n", chestCount)), () => SetWardrobeTab(WardrobeChest), _wardrobeTab == WardrobeChest));
            pages.Add(new Ui.ListWindow.Act(T("ui.wardrobe.tab.jewelry"), () => SetWardrobeTab(WardrobeJewelry), _wardrobeTab == WardrobeJewelry));
            pages.Add(new Ui.ListWindow.Act(T("ui.wardrobe.tab.tailor"), () => SetWardrobeTab(WardrobeTailor), _wardrobeTab == WardrobeTailor));

            var people = model.AddTabRow();
            foreach (var m in _population.Members)
            {
                if (m.IsVillager || m.IsDead) continue;
                var member = m;
                people.Add(new Ui.ListWindow.Act(m.Name, () => { _wardrobeFor = member; _wardrobeStatus = ""; }, m == c));
            }

            switch (_wardrobeTab)
            {
                case WardrobeChest: FillChest(model, c, svc); break;
                case WardrobeJewelry: FillJewelry(model, c, svc); break;
                case WardrobeTailor: FillTailor(model, c); break;
                default: FillOutfits(model, c, svc); break;
            }
            return model;
        }

        private void SetWardrobeTab(int tab)
        {
            _wardrobeTab = tab;
            _wardrobeStatus = "";
        }

        // ---- takımlar ----

        private void FillOutfits(Ui.ListWindow.Model model, Character c, ClothingService svc)
        {
            var wardrobe = svc.WardrobeOf(c);
            var kinds = model.AddTabRow();
            foreach (OutfitKind k in Enum.GetValues(typeof(OutfitKind)))
            {
                var kind = k;
                string label = k == wardrobe.Current ? T("ui.wardrobe.current_mark", ("outfit", OutfitName(k))) : OutfitName(k);
                kinds.Add(new Ui.ListWindow.Act(label, () => { _wardrobeOutfit = kind; _wardrobeStatus = ""; }, k == _wardrobeOutfit));
            }
            model.Info = T("ui.wardrobe.outfit_info", ("current", OutfitName(wardrobe.Current)));

            var outfit = wardrobe.Get(_wardrobeOutfit);
            var tailor = BestMember(Thatchwick.Core.Characters.SkillType.Tailoring) ?? c;
            model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.h.in_outfit", ("outfit", OutfitName(_wardrobeOutfit)))) { Header = true });
            if (outfit.IsEmpty) model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.empty_outfit")));
            foreach (ClothingSlot slot in Enum.GetValues(typeof(ClothingSlot)))
            {
                if (!outfit.Slots.TryGetValue(slot, out int id)) continue;
                var it = svc.Get(id);
                if (it == null) continue;
                model.Rows.Add(PieceRow(c, it, tailor, T("ui.wardrobe.slot_line", ("slot", SlotName(slot)), ("item", ClothName(it))), worn: true));
            }
            foreach (int id in new List<int>(outfit.Jewelry))
            {
                var it = svc.Get(id);
                if (it == null) continue;
                model.Rows.Add(PieceRow(c, it, tailor, T("ui.wardrobe.slot_line", ("slot", T("cloth.slot.jewelry")), ("item", JewelryName(it))), worn: true));
            }

            model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.h.owned")) { Header = true });
            int others = 0;
            foreach (var it in new List<ClothingInstance>(svc.OwnedBy(c)))
            {
                if (it.IsJewelry || it.Diamond || outfit.Contains(it.Id)) continue;
                others++;
                var m = svc.ModelOf(it);
                string slot = m != null ? SlotName(m.Slot) : "";
                model.Rows.Add(PieceRow(c, it, tailor, T("ui.wardrobe.slot_line", ("slot", slot), ("item", ClothName(it))), worn: false));
            }
            if (others == 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.none_owned")));
        }

        /// <summary>Giysi satırı: giyiliyse Çıkar, değilse Giy; yıpranmışsa Yama.</summary>
        private Ui.ListWindow.Row PieceRow(Character c, ClothingInstance it, Character tailor, string title, bool worn)
        {
            var row = new Ui.ListWindow.Row(title, it.IsJewelry ? null : ClothDetail(it)) { Marked = worn };
            var piece = it;
            if (worn)
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.wardrobe.take_off"), () => { _life.TakeOffClothing(c, piece, _wardrobeOutfit); _wardrobeStatus = ""; }));
            else
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.wardrobe.wear"), () => WardrobeResult(_life.WearClothing(c, piece, _wardrobeOutfit))));
            if (!it.IsJewelry && !it.Torn && it.Durability < ClothingInstance.MaxDurability)
                row.Actions.Add(new Ui.ListWindow.Act(T("cloth.patch"), () => WardrobeResult(_life.PatchClothing(tailor, piece), T("ui.wardrobe.patched", ("name", tailor.Name)))));
            return row;
        }

        // ---- sandık ----

        private void FillChest(Ui.ListWindow.Model model, Character c, ClothingService svc)
        {
            var dirty = svc.Basket(LivingHouseholdMembers());
            model.Info = T("ui.wardrobe.chest_info", ("outfit", OutfitName(_wardrobeOutfit)), ("scrap", _household.Count(Thatchwick.Core.Life.LifeSystems.ClothScrapId)),
                ("dirty", dirty.Count));
            var tailor = BestMember(Thatchwick.Core.Characters.SkillType.Tailoring) ?? c;
            var chest = new List<ClothingInstance>(svc.Chest());
            if (chest.Count == 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.chest_empty")));
            foreach (var it in chest)
            {
                var piece = it;
                var m = svc.ModelOf(it);
                string slot = m != null ? SlotName(m.Slot) : "";
                var row = PieceRow(c, it, tailor, T("ui.wardrobe.slot_line", ("slot", slot), ("item", ClothName(it))), worn: false);
                if (!it.Heirloom)
                    row.Actions.Add(new Ui.ListWindow.Act(T("cloth.unpick"), () =>
                    {
                        int back = _life.UnpickClothing(tailor, piece);
                        _wardrobeStatus = back > 0 ? T("ui.wardrobe.unpicked", ("n", back)) : T("wardrobe.err.other");
                    }));
                model.Rows.Add(row);
            }
            if (dirty.Count > 0)
                model.Footer.Add(new Ui.ListWindow.Act(T("cloth.wash_all") + " (" + dirty.Count + ")", () =>
                {
                    int n = _life.WashAllClothes(c);
                    _wardrobeStatus = T("ui.wardrobe.washed", ("n", n));
                }));
        }

        // GameRoot.HouseholdMembers ölüleri de sayar (main'den geldi); çamaşır sepeti yalnızca yaşayanlara bakar.
        private IEnumerable<Character> LivingHouseholdMembers()
        {
            foreach (var m in _population.Members) if (!m.IsVillager && !m.IsDead) yield return m;
        }

        // ---- takılar ----

        private void FillJewelry(Ui.ListWindow.Model model, Character c, ClothingService svc)
        {
            model.Info = T("ui.family.jewelry.effects", ("name", c.Name),
                ("trade", Math.Round(svc.TradeBonus(c) * 100)), ("court", Math.Round(svc.CourtBonus(c) * 100)),
                ("luck", Math.Round((1.0 - svc.AccidentFactor(c)) * 100)), ("joy", svc.JewelryJoy(c)));

            model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.h.worn_jewelry")) { Header = true });
            var worn = new List<ClothingInstance>(svc.JewelryOwnedBy(c));
            if (worn.Count == 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.no_jewelry")));
            foreach (var it in worn)
            {
                var piece = it;
                var row = new Ui.ListWindow.Row(JewelryName(it)) { Marked = true };
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.family.jewelry.take_off"), () => { svc.TakeOffJewelry(c, piece); _wardrobeStatus = ""; }));
                model.Rows.Add(row);
            }

            var drawer = new List<ClothingInstance>(svc.UnwornJewelry());
            if (drawer.Count > 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.family.jewelry.drawer")) { Header = true });
            foreach (var it in drawer)
            {
                var piece = it;
                var row = new Ui.ListWindow.Row(JewelryName(it));
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.family.jewelry.wear"), () => _wardrobeStatus = JewelryErrorText(svc.WearJewelry(c, piece), c)));
                if (!it.Heirloom)
                    row.Actions.Add(new Ui.ListWindow.Act(T("ui.family.jewelry.to_stock"), () => { _life.JewelryToInventory(piece); _wardrobeStatus = ""; }));
                model.Rows.Add(row);
            }

            var stock = new List<KeyValuePair<string, int>>(_life.JewelryInInventory());
            if (stock.Count > 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.wardrobe.h.stock_jewelry")) { Header = true });
            foreach (var kv in stock)
            {
                string itemId = kv.Key;
                var row = new Ui.ListWindow.Row(T("ui.family.jewelry.in_stock", ("item", ItemName(kv.Key)), ("n", kv.Value)));
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.family.jewelry.wear"), () => _wardrobeStatus = JewelryErrorText(_life.WearJewelryFromInventory(c, itemId), c)));
                model.Rows.Add(row);
            }
        }

        // ---- terzi ----

        private string MakerDisplayName(string maker) => _life.MakerName(maker);

        private void FillTailor(Ui.ListWindow.Model model, Character c)
        {
            var svc = _life.Clothing;
            model.Info = T("ui.tailor.info", ("fee", TailorOrders.CourierFee), ("tier", TierName(_life.HouseholdClothingTier)));

            model.Rows.Add(new Ui.ListWindow.Row(T("ui.tailor.h.orders")) { Header = true });
            int open = 0;
            foreach (var o in new List<TailorOrder>(_life.Tailor.Orders))
            {
                if (o.Delivered) continue;
                open++;
                var order = o;
                var m = svc.Catalog.Get(o.ModelId);
                string item = m != null ? ItemName(m.ItemId) : o.ModelId;
                string who = _population.Find(o.CustomerId)?.Name ?? "";
                bool ready = o.Ready(_clock.TotalDays);
                string when = ready ? T("ui.tailor.order_ready") : T("ui.tailor.order_due", ("date", _life.Festivals.DateText((long)Math.Ceiling(o.DueDay))));
                if (o.ByCourier) when += " · " + T("ui.tailor.order_by_courier");
                var row = new Ui.ListWindow.Row(T("ui.tailor.order_line", ("item", item), ("customer", who), ("maker", MakerDisplayName(o.Maker))), when) { Marked = ready };
                if (!o.ByCourier)
                    row.Actions.Add(new Ui.ListWindow.Act(T("ui.tailor.pick_up"),
                        () => WardrobeResult(_life.PickUpOrder(order), T("ui.tailor.picked", ("item", item))), disabled: !ready));
                model.Rows.Add(row);
            }
            if (open == 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.tailor.no_orders")));

            model.Rows.Add(new Ui.ListWindow.Row(T("ui.tailor.h.models", ("name", c.Name))) { Header = true });
            var models = new List<ClothingModelDef>(_life.OrderableModels(c));
            models.Sort((a, b) => a.Tier != b.Tier ? a.Tier.CompareTo(b.Tier) : string.CompareOrdinal(a.Id, b.Id));
            if (models.Count == 0) model.Rows.Add(new Ui.ListWindow.Row(T("ui.tailor.none")));
            foreach (var def in models)
            {
                var m = def;
                string item = ItemName(m.ItemId);
                var row = new Ui.ListWindow.Row(T("ui.tailor.model_line", ("item", item), ("tier", TierName(m.Tier))),
                    T("ui.tailor.model_detail", ("slot", SlotName(m.Slot)), ("maker", MakerDisplayName(TailorOrders.MakerFor(m))),
                        ("fee", _life.OrderQuote(m, false)), ("days", TailorOrders.Days(m.Tier))));
                bool allowed = TailorOrders.TierAllowed(m.Tier, _life.HouseholdClothingTier);
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.tailor.order"), () => PlaceTailorOrder(c, m, item, false), disabled: !allowed));
                row.Actions.Add(new Ui.ListWindow.Act(T("ui.tailor.order_courier"), () => PlaceTailorOrder(c, m, item, true), disabled: !allowed));
                model.Rows.Add(row);
            }
        }

        private void PlaceTailorOrder(Character c, ClothingModelDef m, string item, bool courier)
        {
            var err = _life.PlaceOrder(c, m, courier);
            if (!err.IsEmpty) { _wardrobeStatus = GameLocalization.R(err); return; }
            TailorOrder placed = null;
            foreach (var o in _life.Tailor.Orders) if (!o.Delivered && o.ModelId == m.Id && o.CustomerId == c.Id) placed = o;
            _wardrobeStatus = T("ui.tailor.ordered", ("item", item),
                ("date", placed != null ? _life.Festivals.DateText((long)Math.Ceiling(placed.DueDay)) : ""));
        }
    }
}
