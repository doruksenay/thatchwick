using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Yapı modu işlemleri: satın al, sat, envanterden yerleştir, envantere al, taşı.
    /// Kurallar burada, arayüz yalnızca çağırır ve nedeni gösterir.
    /// </summary>
    public sealed class BuildService
    {
        public const double DefaultSellRatio = 0.5;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;

        public double SellRatio { get; }

        public BuildService(WorldState world, Household house, ContentDatabase content, double sellRatio = DefaultSellRatio)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            if (sellRatio < 0 || sellRatio > 1) throw new ArgumentOutOfRangeException(nameof(sellRatio));
            SellRatio = sellRatio;
        }

        /// <summary>Bir eşyanın satış değeri (fiyatın oranı, aşağı yuvarlanır).</summary>
        public int SellValue(ItemDef def) => (int)Math.Floor(def.Price * SellRatio);

        public PlacementCheck Buy(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (def.Price <= 0) return PlacementCheck.Fail("Bu eşya satılmıyor.");
            if (!_house.TrySpend(def.Price)) return PlacementCheck.Fail("Akçe yetmiyor.");
            _house.AddItem(itemId);
            return PlacementCheck.Success;
        }

        /// <summary>Envanterdeki bir eşyayı satar.</summary>
        public PlacementCheck SellFromInventory(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (!_house.TryTakeItem(itemId)) return PlacementCheck.Fail("Envanterde yok.");
            _house.AddAkce(SellValue(def));
            return PlacementCheck.Success;
        }

        /// <summary>Yerleştirilmiş eşyayı (ve üstündeki dekorları) satar.</summary>
        public PlacementCheck SellPlaced(int itemId)
        {
            if (_world.GetItem(itemId) == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            int total = 0;
            foreach (var def in _world.RemoveItem(itemId)) total += SellValue(def);
            if (total > 0) _house.AddAkce(total);
            return PlacementCheck.Success;
        }

        public PlacementCheck CanPlaceFromInventory(string itemId, GridPos position, int rotation)
        {
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            return _world.CanPlaceFloor(_content.Items.Get(itemId), position, rotation);
        }

        public PlacementCheck PlaceFromInventory(string itemId, GridPos position, int rotation, out ItemInstance placed)
        {
            placed = null;
            var check = CanPlaceFromInventory(itemId, position, rotation);
            if (!check.Ok) return check;
            _house.TryTakeItem(itemId);
            placed = _world.PlaceItem(_content.Items.Get(itemId), position, rotation);
            return PlacementCheck.Success;
        }

        public PlacementCheck PlaceDecorFromInventory(string itemId, int surfaceItemId, string slotId, out ItemInstance placed)
        {
            placed = null;
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            var check = _world.CanPlaceOnSurface(def, surfaceItemId, slotId);
            if (!check.Ok) return check;
            _house.TryTakeItem(itemId);
            placed = _world.PlaceOnSurface(def, surfaceItemId, slotId);
            return PlacementCheck.Success;
        }

        public PlacementCheck Move(int itemId, GridPos position, int rotation) => _world.MoveItem(itemId, position, rotation);

        /// <summary>Eşyayı (ve dekorlarını) envantere geri alır.</summary>
        public PlacementCheck StoreToInventory(int itemId)
        {
            if (_world.GetItem(itemId) == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            foreach (var def in _world.RemoveItem(itemId)) _house.AddItem(def.Id);
            return PlacementCheck.Success;
        }

        /// <summary>Mağazada satılan eşyalar (fiyatı olanlar), kategoriye ve ada göre.</summary>
        public List<ItemDef> ShopList()
        {
            var list = new List<ItemDef>();
            foreach (var d in _content.Items.All) if (d.Price > 0) list.Add(d);
            list.Sort((a, b) =>
            {
                int c = string.CompareOrdinal(a.Category, b.Category);
                return c != 0 ? c : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });
            return list;
        }
    }
}
