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

        // ---- geri al (Adım 43) ----

        private sealed class UndoStep
        {
            public string Label;
            public Func<PlacementCheck> Run;
        }

        public const int UndoCapacity = 30;
        private readonly List<UndoStep> _undo = new List<UndoStep>();
        private List<UndoStep> _group;
        private string _groupLabel;
        private bool _undoing;

        public bool CanUndo => _undo.Count > 0;
        public string UndoLabel => _undo.Count > 0 ? _undo[_undo.Count - 1].Label : null;
        public int UndoCount => _undo.Count;
        /// <summary>Şimdiye dek kaydedilen toplam geri alma adımı (kapasite taşsa da artar).</summary>
        public int UndoPushes { get; private set; }
        public void ClearUndo() { _undo.Clear(); _group = null; }

        private void Record(string label, Func<PlacementCheck> undo)
        {
            if (_undoing) return;
            var step = new UndoStep { Label = label, Run = undo };
            if (_group != null) { _group.Add(step); return; }
            _undo.Add(step);
            UndoPushes++;
            if (_undo.Count > UndoCapacity) _undo.RemoveAt(0);
        }

        /// <summary>Birden çok işlemi tek geri alma adımında toplar (oda şablonu).</summary>
        public void BeginGroup(string label)
        {
            if (_group != null) return;
            _group = new List<UndoStep>();
            _groupLabel = label;
        }

        public void EndGroup()
        {
            if (_group == null) return;
            var steps = _group;
            _group = null;
            if (steps.Count == 0) return;
            Record(_groupLabel, () =>
            {
                for (int i = steps.Count - 1; i >= 0; i--)
                {
                    var r = steps[i].Run();
                    if (!r.Ok) return r;
                }
                return PlacementCheck.Success;
            });
        }

        /// <summary>Son yapı işlemini geri alır (akçe de geri döner).</summary>
        public PlacementCheck Undo()
        {
            if (_undo.Count == 0) return PlacementCheck.Fail("Geri alınacak bir şey yok.");
            var step = _undo[_undo.Count - 1];
            _undoing = true;
            PlacementCheck result;
            try { result = step.Run(); }
            finally { _undoing = false; }
            if (result.Ok) _undo.RemoveAt(_undo.Count - 1);
            return result;
        }

        private void GiveBack(List<ItemDef> defs, int from = 0)
        {
            for (int i = from; i < defs.Count; i++) _house.AddItem(defs[i].Id);
        }

        /// <summary>Depo kapasitesi; null ise sınırsız (testler, eski kurulumlar).</summary>
        public StorageService Storage { get; set; }

        /// <summary>Hesap seviyesi sağlayıcı (T10); null ise her eşya açık.</summary>
        public Func<int> AccountLevel { get; set; }

        /// <summary>İndirim oranı (0-0.5); hane reisinin Tüccar/Usta dalı gibi özelliklerinden gelir. Yoksa indirim yok.</summary>
        public Func<float> Discount { get; set; }

        /// <summary>İndirimli alış fiyatı (en az 1 akçe).</summary>
        public int PriceFor(ItemDef def)
        {
            float d = Discount == null ? 0f : Math.Max(0f, Math.Min(0.5f, Discount()));
            return d <= 0f ? def.Price : Math.Max(1, (int)Math.Round(def.Price * (1f - d)));
        }

        /// <summary>Anıt (mezar) ve yabani toplanabilir eşyalar oyuncu tarafından taşınmaz, satılmaz.</summary>
        public static bool IsLocked(ItemInstance item) =>
            item != null && (item.Def.Category == "memorial" || item.Def.IsForage);

        private const string Locked = "Bu taşınmaz ya da satılmaz.";

        /// <summary>Barınakta yaşayan hayvan sayısı (hayvan sistemi verir); null ise hep 0. Doluyken barınak kaldırılmaz.</summary>
        public Func<int, int> HousingResidents { get; set; }

        private const string HousingOccupied = "İçinde hayvan var: önce hayvanları sat.";
        private bool IsOccupiedHousing(ItemInstance item) => item.Def.IsHousing && HousingResidents != null && HousingResidents(item.Id) > 0;

        public bool IsUnlocked(ItemDef def) => AccountLevel == null || AccountLevel() >= def.UnlockLevel;

        /// <summary>Çiftçilik sistemi; verilirse pencereli sera odalarına da dikilebilir.</summary>
        public Work.PlantSystem Plants { get; set; }

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
            int paid = PriceFor(def);
            if (!_house.TrySpend(paid)) return PlacementCheck.Fail("Akçe yetmiyor.");
            _house.AddItem(itemId);
            Record("Satın alma", () =>
            {
                if (!_house.TryTakeItem(itemId)) return PlacementCheck.Fail("Eşya artık envanterde değil.");
                _house.AddAkce(paid);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Envanterdeki bir eşyayı satar.</summary>
        public PlacementCheck SellFromInventory(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (!_house.TryTakeItem(itemId)) return PlacementCheck.Fail("Envanterde yok.");
            int value = SellValue(def);
            _house.AddAkce(value);
            Record("Satış", () =>
            {
                if (!_house.TrySpend(value)) return PlacementCheck.Fail("Akçe yetmiyor, satış geri alınamaz.");
                _house.AddItem(itemId);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Yerleştirilmiş eşyayı (ve üstündeki dekorları) satar.</summary>
        private const string PlantLocked = "Bitkiler taşınmaz ya da satılmaz; olgunlaşınca kesilir.";

        public PlacementCheck SellPlaced(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(target)) return PlacementCheck.Fail(Locked);
            if (IsOccupiedHousing(target)) return PlacementCheck.Fail(HousingOccupied);
            var rootDef = target.Def; var rootPos = target.Position; int rootRot = target.Rotation;
            int total = 0;
            var removed = _world.RemoveItem(itemId);
            foreach (var def in removed) total += SellValue(def);
            if (total > 0) _house.AddAkce(total);
            Record("Eşya satışı", () =>
            {
                if (!_world.CanPlaceFloor(rootDef, rootPos, rootRot).Ok) return PlacementCheck.Fail("Eski yeri doldu, satış geri alınamaz.");
                if (!_house.TrySpend(total)) return PlacementCheck.Fail("Akçe yetmiyor, satış geri alınamaz.");
                _world.PlaceItem(rootDef, rootPos, rootRot);
                GiveBack(removed, 1);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        private PlacementCheck CheckOwned(ItemDef def, GridPos position, int rotation)
        {
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    if (!_world.Map.IsOwned(new GridPos(position.X + dx, position.Y + dy, position.Z)))
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
            var floor = _world.CanPlaceFloor(def, position, rotation, null, landingMayBeCreated: def.IsStairs);
            if (!floor.Ok || !def.IsStairs) return floor;

            // Merdivenin öbür ucunda karo yoksa (üst kat döşemesi / bodrum kazısı) onu da kurabilmeliyiz.
            WorldState.StairsEnds(def, position, rotation, out _, out var landing);
            if (_world.Map.HasTile(landing)) return floor;
            var create = CanBuildLevelTile(landing, stairEnd: true);
            if (!create.Ok) return create;
            return _house.Akce >= TilePrice(landing.Z) ? PlacementCheck.Success : PlacementCheck.Fail($"Merdivenin ucunu kurmak için {TilePrice(landing.Z)} akçe gerekir.");
        }

        public PlacementCheck PlaceFromInventory(string itemId, GridPos position, int rotation, out ItemInstance placed)
        {
            placed = null;
            var check = CanPlaceFromInventory(itemId, position, rotation);
            if (!check.Ok) return check;
            var def = _content.Items.Get(itemId);
            bool grouped = false;
            if (def.IsStairs)
            {
                WorldState.StairsEnds(def, position, rotation, out _, out var landing);
                if (!_world.Map.HasTile(landing))
                {
                    BeginGroup("Merdiven");
                    grouped = true;
                    var built = BuildLevelTile(landing, stairEnd: true);
                    if (!built.Ok) { EndGroup(); return built; }
                }
            }
            _house.TryTakeItem(itemId);
            var placedItem = _world.PlaceItem(def, position, rotation);
            placed = placedItem;
            int placedId = placedItem.Id;
            Record("Yerleştirme", () =>
            {
                if (_world.GetItem(placedId) == null) return PlacementCheck.Fail("Eşya artık orada değil.");
                GiveBack(_world.RemoveItem(placedId));
                return PlacementCheck.Success;
            });
            if (grouped) EndGroup();
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
            var wallItem = _world.PlaceOnWall(_content.Items.Get(itemId), tile, side);
            placed = wallItem;
            int wallId = wallItem.Id;
            Record("Duvara asma", () =>
            {
                if (_world.GetItem(wallId) == null) return PlacementCheck.Fail("Eşya artık orada değil.");
                GiveBack(_world.RemoveItem(wallId));
                return PlacementCheck.Success;
            });
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
            var decor = _world.PlaceOnSurface(def, surfaceItemId, slotId);
            placed = decor;
            int decorId = decor.Id;
            Record("Dekor", () =>
            {
                if (_world.GetItem(decorId) == null) return PlacementCheck.Fail("Dekor artık orada değil.");
                GiveBack(_world.RemoveItem(decorId));
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        public PlacementCheck Move(int itemId, GridPos position, int rotation)
        {
            var item = _world.GetItem(itemId);
            if (item != null && item.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(item)) return PlacementCheck.Fail(Locked);
            if (item != null && item.IsWallMounted)
            {
                if (!_world.Map.IsOwned(position)) return PlacementCheck.Fail(NotOwned);
            }
            else if (item != null)
            {
                var owned = CheckOwned(item.Def, position, rotation);
                if (!owned.Ok) return owned;
            }
            var oldPos = item != null ? item.Position : position;
            int oldRot = item != null ? item.Rotation : rotation;
            var moved = _world.MoveItem(itemId, position, rotation);
            if (moved.Ok && item != null)
                Record("Taşıma", () => _world.GetItem(itemId) == null ? PlacementCheck.Fail("Eşya artık yok.") : _world.MoveItem(itemId, oldPos, oldRot));
            return moved;
        }

        /// <summary>Eşyayı (ve dekorlarını) envantere geri alır.</summary>
        public PlacementCheck StoreToInventory(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(target)) return PlacementCheck.Fail(Locked);
            if (IsOccupiedHousing(target)) return PlacementCheck.Fail(HousingOccupied);
            var rootDef = target.Def; var rootPos = target.Position; int rootRot = target.Rotation;
            var removed = _world.RemoveItem(itemId);
            GiveBack(removed);
            Record("Envantere alma", () =>
            {
                if (!_world.CanPlaceFloor(rootDef, rootPos, rootRot).Ok) return PlacementCheck.Fail("Eski yeri doldu.");
                if (!_house.TryTakeItem(rootDef.Id)) return PlacementCheck.Fail("Eşya artık envanterde değil.");
                _world.PlaceItem(rootDef, rootPos, rootRot);
                return PlacementCheck.Success;
            });
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
            if (!map.HasTile(edge.SideA) && !map.HasTile(edge.SideB)) return PlacementCheck.Fail(edge.Z > 0 ? "Önce üst kat döşemesi gerekir." : "Önce burayı kazmalısın.");
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
            int net = refund - cost;
            Record("Duvar", () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                map.SetEdge(edge, old);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Karoya zemin kaplaması döşer (Grass = kaplamayı kaldır, yarısı iade).</summary>
        public PlacementCheck PaintFloor(GridPos tile, FloorType floor)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            if (!map.HasTile(tile)) return PlacementCheck.Fail("Burada zemin yok.");
            var old = map.GetFloor(tile);
            if (old == floor) return PlacementCheck.Fail("Orası zaten öyle.");
            int cost = Math.Max(0, FloorPrice(floor) - Refund(FloorPrice(old)));
            int refund = floor == FloorType.Grass ? Refund(FloorPrice(old)) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > 0) _house.AddAkce(refund);
            map.SetFloor(tile, floor);
            int net = refund - cost;
            Record("Zemin", () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                map.SetFloor(tile, old);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---- katlar: üst kat döşemesi ve bodrum kazısı ----

        public const int SlabPrice = 4, DigPrice = 5;

        /// <summary>Üst kat döşemesi ya da bodrum kazısının karo başına akçesi.</summary>
        public static int TilePrice(int level) => level > 0 ? SlabPrice : DigPrice;

        private static readonly GridPos[] FourDirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>Üst kat döşemesi, altındaki karo kapalı bir odanın içindeyse ya da altında duvar/kapı/pencere varsa taşınır.</summary>
        private bool IsSupported(GridPos upper)
        {
            var map = _world.Map;
            var below = upper.OnLevel(upper.Z - 1);
            if (!map.HasTile(below)) return false;
            var room = map.Rooms.RoomAt(below);
            if (room != null && room.IsIndoor) return true;
            foreach (var d in FourDirs)
                if (map.GetEdge(GridMap.EdgeBetween(below, below + d)) != EdgeKind.None) return true;
            return false;
        }

        /// <summary>Verilen karoda üst kat döşemesi (Z &gt; 0) ya da bodrum kazısı (Z &lt; 0) yapılabilir mi.</summary>
        public PlacementCheck CanBuildLevelTile(GridPos p, bool stairEnd = false)
        {
            var map = _world.Map;
            if (p.Z == 0) return PlacementCheck.Fail("Zemin kat zaten var.");
            if (!map.InBounds(p)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(p)) return PlacementCheck.Fail(NotOwned);
            if (map.HasTile(p)) return PlacementCheck.Fail("Orası zaten yapılmış.");
            if (p.Z > 0)
            {
                if (!IsSupported(p)) return PlacementCheck.Fail("Üst kat, duvarlı bir odanın üstüne kurulur.");
                return PlacementCheck.Success;
            }
            // Bodrum: mevcut bodrumun bitişiğinden kazılır; ilk karo merdivenin ucundan başlar.
            foreach (var d in FourDirs)
                if (map.HasTile(p + d)) return PlacementCheck.Success;
            return stairEnd ? PlacementCheck.Success : PlacementCheck.Fail("Bodrumu bir merdivenle başlat ya da mevcut bodrumun yanını kaz.");
        }

        /// <summary>Üst kat döşemesi yapar ya da bodrum karosu kazar (akçe harcar).</summary>
        public PlacementCheck BuildLevelTile(GridPos p, bool stairEnd = false)
        {
            // Merdiven ucu için ilk bodrum karosu komşusuz da kazılabilir.
            var check = CanBuildLevelTile(p, stairEnd);
            if (!check.Ok) return check;
            int price = TilePrice(p.Z);
            if (!_house.TrySpend(price)) return PlacementCheck.Fail("Akçe yetmiyor.");
            var map = _world.Map;
            map.SetTile(p, true);
            map.SetFloor(p, p.Z < 0 ? FloorType.Dirt : FloorType.Wood);
            int refund = Refund(price);
            Record(p.Z > 0 ? "Üst kat döşemesi" : "Bodrum kazısı", () =>
            {
                if (_world.GetItemAt(p) != null) return PlacementCheck.Fail("Üzerinde eşya var, geri alınamaz.");
                map.SetTile(p, false);
                _house.AddAkce(refund);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Üst kat döşemesini kaldırır ya da bodrum karosunu doldurur; yarısı iade. Üstünde eşya, çevresinde duvar ya da merdiven bağı olmamalı.</summary>
        public PlacementCheck RemoveLevelTile(GridPos p)
        {
            var map = _world.Map;
            if (p.Z == 0 || !map.HasTile(p)) return PlacementCheck.Fail("Orada kaldırılacak bir şey yok.");
            if (!map.IsOwned(p)) return PlacementCheck.Fail(NotOwned);
            if (_world.GetItemAt(p) != null) return PlacementCheck.Fail("Önce üstündeki eşyayı kaldır.");
            if (map.HasLinkAt(p)) return PlacementCheck.Fail("Merdiven buraya bağlı; önce merdiveni kaldır.");
            foreach (var d in FourDirs)
                if (map.GetEdge(GridMap.EdgeBetween(p, p + d)) != EdgeKind.None) return PlacementCheck.Fail("Önce çevresindeki duvarları kaldır.");
            // Ayakta durulan karo: başka bir üst kat karosu bunun üstüne dayanıyorsa kaldırılamaz (şimdilik yalnızca MaxLevel=1, bodrumun üstü zemin).
            var oldFloor = map.GetFloor(p);
            int price = TilePrice(p.Z);
            int refund = Refund(price);
            map.SetTile(p, false);
            _house.AddAkce(refund);
            Record(p.Z > 0 ? "Döşeme kaldırma" : "Bodrum doldurma", () =>
            {
                if (!_house.TrySpend(refund)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz.");
                map.SetTile(p, true);
                map.SetFloor(p, oldFloor);
                return PlacementCheck.Success;
            });
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
            if (room != null && room.IsIndoor && !(Plants != null && Plants.IsGreenhouse(room)))
                return PlacementCheck.Fail("Ev içine dikilmez, dışarıda bir yer seç (ya da odaya pencere aç: sera).");
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

        // ---- varyantlar (Adım 43) ----

        /// <summary>Yerleştirilmiş eşyanın aynı yerde seçilebilecek varyantları (aynı boyutlu, yer eşyası).</summary>
        public List<ItemDef> VariantsOf(ItemDef def)
        {
            var list = new List<ItemDef>();
            foreach (var id in def.Variants)
                if (_content.Items.TryGet(id, out var v) && v.Width == def.Width && v.Height == def.Height && v.Placement == def.Placement && IsUnlocked(v))
                    list.Add(v);
            return list;
        }

        /// <summary>
        /// Yerleştirilmiş eşyayı varyantıyla değiştirir (aynı yer ve yön). Fiyat farkı ödenir; ucuza geçişte satış oranı kadar iade edilir.
        /// Üstünde dekor varsa önce onlar alınmalı.
        /// </summary>
        public PlacementCheck SwapVariant(int itemId, string variantId, out ItemInstance replaced)
        {
            replaced = null;
            var item = _world.GetItem(itemId);
            if (item == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (item.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(item)) return PlacementCheck.Fail(Locked);
            if (!item.Def.Variants.Contains(variantId)) return PlacementCheck.Fail("Bu eşyanın böyle bir varyantı yok.");
            if (!_content.Items.TryGet(variantId, out var variant)) return PlacementCheck.Fail("Varyant bulunamadı.");
            if (!IsUnlocked(variant)) return PlacementCheck.Fail($"Hesap seviyesi {variant.UnlockLevel} gerekli.");
            foreach (var other in _world.Items)
                if (other.ParentId == itemId) return PlacementCheck.Fail("Önce üstündeki dekoru al.");

            var oldDef = item.Def; var pos = item.Position; int rot = item.Rotation;
            int diff = variant.Price - oldDef.Price;
            int delta = diff > 0 ? -diff : (int)Math.Floor(-diff * SellRatio);   // eksi = ödedi, artı = iade aldı
            if (diff > 0 && !_house.TrySpend(diff)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (diff < 0 && delta > 0) _house.AddAkce(delta);

            _world.RemoveItem(itemId);
            var created = _world.PlaceItem(variant, pos, rot);
            replaced = created;
            int newId = created.Id;
            Record("Varyant", () =>
            {
                var cur = _world.GetItem(newId);
                if (cur == null) return PlacementCheck.Fail("Eşya artık yok.");
                if (delta > 0) { if (!_house.TrySpend(delta)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (delta < 0) _house.AddAkce(-delta);
                _world.RemoveItem(newId);
                _world.PlaceItem(oldDef, pos, rot);
                return PlacementCheck.Success;
            });
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
