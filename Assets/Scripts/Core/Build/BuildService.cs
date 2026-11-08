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

        /// <summary>Depo kapasitesi; null ise sınırsız (testler, eski kurulumlar).</summary>
        public StorageService Storage { get; set; }

        /// <summary>Hesap seviyesi sağlayıcı (T10); null ise her eşya açık.</summary>
        public Func<int> AccountLevel { get; set; }

        public bool IsUnlocked(ItemDef def) => AccountLevel == null || AccountLevel() >= def.UnlockLevel;

        /// <summary>Barınakta yaşayan hayvan sayısı (hayvan sistemi verir); null ise hep 0. Doluyken barınak kaldırılmaz.</summary>
        public Func<int, int> HousingResidents { get; set; }

        private const string HousingOccupied = "İçinde hayvan var: önce hayvanları sat.";
        private bool IsOccupiedHousing(ItemInstance item) => item.Def.IsHousing && HousingResidents != null && HousingResidents(item.Id) > 0;

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
            if (!IsUnlocked(def)) return PlacementCheck.Fail($"Hesap seviyesi {def.UnlockLevel} gerekli.");
            if (Storage != null && !Storage.HasRoomFor(itemId)) return PlacementCheck.Fail("Depo dolu: depo bölgesi çiz ya da sandık koy.");
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
        private const string PlantLocked = "Bitkiler taşınmaz ya da satılmaz; olgunlaşınca kesilir.";

        public PlacementCheck SellPlaced(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsOccupiedHousing(target)) return PlacementCheck.Fail(HousingOccupied);
            int total = 0;
            foreach (var def in _world.RemoveItem(itemId)) total += SellValue(def);
            if (total > 0) _house.AddAkce(total);
            return PlacementCheck.Success;
        }

        private PlacementCheck CheckOwned(ItemDef def, GridPos position, int rotation)
        {
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    if (!_world.Map.IsOwned(new GridPos(position.X + dx, position.Y + dy)))
                        return PlacementCheck.Fail(NotOwned);
            return PlacementCheck.Success;
        }

        public const string NotOwned = "Burası senin arsan değil.";

        public PlacementCheck CanPlaceFromInventory(string itemId, GridPos position, int rotation)
        {
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            if (def.Placement == ItemPlacement.Wall) return PlacementCheck.Fail("Bu eşya duvara asılır.");
            var owned = CheckOwned(def, position, rotation);
            if (!owned.Ok) return owned;
            return _world.CanPlaceFloor(def, position, rotation);
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

        /// <summary>Duvar eşyasının karonun bir kenarına asılıp asılamayacağı (envanter, arsa, duvar).</summary>
        public PlacementCheck CanPlaceWallFromInventory(string itemId, GridPos tile, int side)
        {
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            if (!_world.Map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            return _world.CanPlaceOnWall(def, tile, side);
        }

        public PlacementCheck PlaceWallFromInventory(string itemId, GridPos tile, int side, out ItemInstance placed)
        {
            placed = null;
            var check = CanPlaceWallFromInventory(itemId, tile, side);
            if (!check.Ok) return check;
            _house.TryTakeItem(itemId);
            placed = _world.PlaceOnWall(_content.Items.Get(itemId), tile, side);
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

        public PlacementCheck Move(int itemId, GridPos position, int rotation)
        {
            var item = _world.GetItem(itemId);
            if (item != null && item.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (item != null && item.IsWallMounted)
            {
                if (!_world.Map.IsOwned(position)) return PlacementCheck.Fail(NotOwned);
            }
            else if (item != null)
            {
                var owned = CheckOwned(item.Def, position, rotation);
                if (!owned.Ok) return owned;
            }
            return _world.MoveItem(itemId, position, rotation);
        }

        /// <summary>Eşyayı (ve dekorlarını) envantere geri alır.</summary>
        public PlacementCheck StoreToInventory(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsOccupiedHousing(target)) return PlacementCheck.Fail(HousingOccupied);
            foreach (var def in _world.RemoveItem(itemId)) _house.AddItem(def.Id);
            return PlacementCheck.Success;
        }

        // ---- duvar, kapı, pencere, zemin (envantersiz, doğrudan akçe) ----

        public const int WallPrice = 3, DoorPrice = 8, WindowPrice = 6;

        public static int EdgePrice(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall: return WallPrice;
                case EdgeKind.Door: return DoorPrice;
                case EdgeKind.Window: return WindowPrice;
                default: return 0;
            }
        }

        public static int FloorPrice(FloorType floor)
        {
            switch (floor)
            {
                case FloorType.Dirt: return 1;
                case FloorType.Wood: return 2;
                case FloorType.Stone: return 3;
                default: return 0;
            }
        }

        private int Refund(int price) => (int)Math.Floor(price * SellRatio);

        /// <summary>Kenara duvar/kapı/pencere koyar ya da (None) kaldırır. Kapı ve pencere yalnızca duvarın yerine konur.</summary>
        public PlacementCheck SetEdge(Edge edge, EdgeKind kind)
        {
            var map = _world.Map;
            if (!map.EdgeInBounds(edge)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(edge.SideA) && !map.IsOwned(edge.SideB)) return PlacementCheck.Fail(NotOwned);
            var old = map.GetEdge(edge);
            if (old == kind) return PlacementCheck.Fail("Orada zaten var.");
            if ((kind == EdgeKind.Door || kind == EdgeKind.Window) && old != EdgeKind.Wall && old != EdgeKind.Door && old != EdgeKind.Window)
                return PlacementCheck.Fail("Önce duvar kur.");

            int cost = Math.Max(0, EdgePrice(kind) - Refund(EdgePrice(old)));
            int refund = kind == EdgeKind.None ? Refund(EdgePrice(old)) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > 0) _house.AddAkce(refund);
            // Düz duvar kalkınca (ya da kapı/pencere olunca) asılı eşyalar envantere döner.
            if (old == EdgeKind.Wall)
                foreach (var hung in _world.WallItemsOn(edge))
                    foreach (var def in _world.RemoveItem(hung.Id)) _house.AddItem(def.Id);
            map.SetEdge(edge, kind);
            return PlacementCheck.Success;
        }

        /// <summary>Karoya zemin kaplaması döşer (Grass = kaplamayı kaldır, yarısı iade).</summary>
        public PlacementCheck PaintFloor(GridPos tile, FloorType floor)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            var old = map.GetFloor(tile);
            if (old == floor) return PlacementCheck.Fail("Orası zaten öyle.");
            int cost = Math.Max(0, FloorPrice(floor) - Refund(FloorPrice(old)));
            int refund = floor == FloorType.Grass ? Refund(FloorPrice(old)) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > 0) _house.AddAkce(refund);
            map.SetFloor(tile, floor);
            return PlacementCheck.Success;
        }

        // ---- dikim ----

        /// <summary>
        /// Envanterdeki fidanı/tohumu dikiyor: kendi arsanda, dış mekânda (oda içinde değil), çim ya da
        /// toprak zeminli, boş karoya. Bitki büyümeye 0'dan başlar.
        /// </summary>
        public PlacementCheck Plant(string seedId, GridPos tile, out ItemInstance planted)
        {
            planted = null;
            if (!_content.Items.TryGet(seedId, out var seed) || seed.PlantsAs == null) return PlacementCheck.Fail("Bu dikilemez.");
            if (_house.Count(seedId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            var room = map.Rooms.RoomAt(tile);
            if (room != null && room.IsIndoor) return PlacementCheck.Fail("Ev içine dikilmez, dışarıda bir yer seç.");
            var floor = map.GetFloor(tile);
            var plantDef = _content.Items.Get(seed.PlantsAs);
            if (plantDef.NeedsTilledSoil && floor != FloorType.Dirt) return PlacementCheck.Fail("Ekin yalnızca toprağa ekilir (yapı modu > Toprak).");
            if (floor != FloorType.Grass && floor != FloorType.Dirt) return PlacementCheck.Fail("Yalnızca çim ya da toprağa dikilir.");
            var check = _world.CanPlaceFloor(plantDef, tile, 0);
            if (!check.Ok) return check;

            _house.TryTakeItem(seedId);
            planted = _world.PlaceItem(plantDef, tile);
            return PlacementCheck.Success;
        }

        // ---- bölgeler ----

        /// <summary>Kendi arsandaki karoya bölge çizer ya da siler (ücretsiz).</summary>
        public PlacementCheck SetZone(GridPos tile, ZoneType zone)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            if (map.GetZone(tile) == zone) return PlacementCheck.Fail("Orası zaten öyle.");
            map.SetZone(tile, zone);
            return PlacementCheck.Success;
        }

        // ---- arsa ----

        public const int ChunkSize = 4;
        public const int ChunkPrice = 20;

        /// <summary>Verilen karonun ait olduğu 4x4 arsa parçasının sol-üst karosu.</summary>
        public static GridPos ChunkOrigin(GridPos tile) =>
            new GridPos(Math.Max(0, tile.X) / ChunkSize * ChunkSize, Math.Max(0, tile.Y) / ChunkSize * ChunkSize);

        /// <summary>Arsa genişletme: komşu (dik) 4x4 parçayı satın alır. Yalnızca sahip olunan alana bitişik parça alınabilir.</summary>
        public PlacementCheck BuyChunk(GridPos tile)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            var o = ChunkOrigin(tile);
            if (map.IsOwned(o)) return PlacementCheck.Fail("Burası zaten senin.");

            bool adjacent = false;
            var dirs = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (int dy = 0; dy < ChunkSize && !adjacent; dy++)
                for (int dx = 0; dx < ChunkSize && !adjacent; dx++)
                {
                    var t = new GridPos(o.X + dx, o.Y + dy);
                    if (!map.InBounds(t)) continue;
                    foreach (var d in dirs)
                    {
                        var n = t + d;
                        bool inside = n.X >= o.X && n.X < o.X + ChunkSize && n.Y >= o.Y && n.Y < o.Y + ChunkSize;
                        if (!inside && map.IsOwned(n)) { adjacent = true; break; }
                    }
                }
            if (!adjacent) return PlacementCheck.Fail("Önce komşu parçayı al.");
            if (!_house.TrySpend(ChunkPrice)) return PlacementCheck.Fail("Akçe yetmiyor.");

            for (int dy = 0; dy < ChunkSize; dy++)
                for (int dx = 0; dx < ChunkSize; dx++)
                {
                    var t = new GridPos(o.X + dx, o.Y + dy);
                    if (map.InBounds(t)) map.SetOwned(t, true);
                }
            return PlacementCheck.Success;
        }

        /// <summary>Mağazada satılan eşyalar (fiyatı olanlar), kategoriye ve ada göre.</summary>
        public List<ItemDef> ShopList()
        {
            var list = new List<ItemDef>();
            // Kaynaklar (odun, şalgam...) mağazada değil pazarda alınıp satılır.
            foreach (var d in _content.Items.All) if (d.Price > 0 && d.Placement != ItemPlacement.Resource) list.Add(d);
            list.Sort((a, b) =>
            {
                int c = string.CompareOrdinal(a.Category, b.Category);
                if (c != 0) return c;
                c = a.UnlockLevel.CompareTo(b.UnlockLevel);
                return c != 0 ? c : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });
            return list;
        }
    }
}
