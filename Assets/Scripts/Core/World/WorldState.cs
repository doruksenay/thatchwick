using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>Yerleştirme denetiminin sonucu: Ok ya da nedeni (Türkçe, arayüzde gösterilir).</summary>
    public readonly struct PlacementCheck
    {
        public bool Ok { get; }
        public string Reason { get; }
        private PlacementCheck(bool ok, string reason) { Ok = ok; Reason = reason; }
        public static PlacementCheck Success => new PlacementCheck(true, null);
        public static PlacementCheck Fail(string reason) => new PlacementCheck(false, reason);
    }

    /// <summary>
    /// Harita ve üzerindeki eşyalar. Yer eşyası kapladığı karoları geçilemez yapar; dekor eşyaları
    /// başka bir eşyanın yüzey yuvasında durur. Yapı modu (taşı, döndür, kaldır) buradaki
    /// işlemlerle çalışır.
    /// </summary>
    public sealed class WorldState
    {
        private readonly List<ItemInstance> _items = new List<ItemInstance>();
        private int _nextId = 1;

        public GridMap Map { get; }
        public IReadOnlyList<ItemInstance> Items => _items;

        /// <summary>Herhangi bir eşya eklendi/taşındı/kaldırıldı (görünüm yenilemesi için).</summary>
        public event Action Changed;

        public WorldState(GridMap map)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
        }

        /// <summary>
        /// Yaşayan karakterlerin durduğu karolar (oyun katmanı verir; yoksa kimse yok sayılır). Yerleştirme denetimi
        /// bununla karakterin üstüne eşya konmasını, yapı servisi de merdiven kalkınca birinin katta kalmasını önler.
        /// </summary>
        public Func<IEnumerable<GridPos>> CharacterPositions { get; set; }

        public bool IsOccupiedByCharacter(GridPos tile)
        {
            if (CharacterPositions == null) return false;
            foreach (var p in CharacterPositions()) if (p == tile) return true;
            return false;
        }

        public bool AnyCharacterOnLevel(int level)
        {
            if (CharacterPositions == null) return false;
            foreach (var p in CharacterPositions()) if (p.Z == level) return true;
            return false;
        }

        // ---- denetimler ----

        /// <summary>Yer eşyasının (def) verilen konum ve dönüşle konup konamayacağı. ignoreItemId: taşınan eşyanın kendisi.</summary>
        /// <param name="restoring">Kayıttan yükleme: yalnızca yapısal denetimler (sınır, karo, çakışma). Oyun sırasında geçerli
        /// oluşmuş durumlar (merdivenin önüne sonradan gelen eşya gibi) yüklemede eşyayı düşürmesin.</param>
        public PlacementCheck CanPlaceFloor(ItemDef def, GridPos position, int rotation, int? ignoreItemId = null, bool landingMayBeCreated = false, bool restoring = false)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement == ItemPlacement.Resource) return PlacementCheck.Fail("Bu eşya yerleştirilmez.");
            if (def.Placement == ItemPlacement.Wall) return PlacementCheck.Fail("Bu eşya duvara asılır.");
            if (def.Placement != ItemPlacement.Floor) return PlacementCheck.Fail("Bu eşya yere konamaz, bir yüzeye konur.");

            if (!GridMap.IsValidLevel(position.Z)) return PlacementCheck.Fail("Geçersiz kat.");
            if (!def.AllowsLevel(position.Z)) return PlacementCheck.Fail(position.Z == 0 ? "Bu eşya zemin katta durmaz." : "Bu eşya yalnızca zemin katta olur.");

            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            ItemInstance ignored = ignoreItemId.HasValue ? GetItem(ignoreItemId.Value) : null;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var tile = new GridPos(position.X + dx, position.Y + dy, position.Z);
                    if (!Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışına taşıyor.");
                    if (!Map.HasTile(tile)) return PlacementCheck.Fail(position.Z > 0 ? "Önce üst kat döşemesi gerekir." : "Önce burayı kazmalısın.");
                    if (Map.IsBlocked(tile) && !(ignored != null && Occupies(ignored, tile)))
                        return PlacementCheck.Fail("Orası dolu ya da geçilemez.");
                    if (restoring) continue;
                    if (IsOccupiedByCharacter(tile)) return PlacementCheck.Fail("Orada biri duruyor.");
                    // Merdivenin önü ve inişi açık kalmalı (taşınan şey merdivenin kendisi değilse).
                    if (Map.HasLinkAt(tile) && !(ignored != null && ignored.Def.IsStairs))
                        return PlacementCheck.Fail("Merdivenin ucu kapanmasın.");
                }
            }

            if (def.IsStairs)
            {
                StairsEnds(def, position, rotation, out var entrance, out var landing);
                if (!GridMap.IsValidLevel(landing.Z)) return PlacementCheck.Fail(def.StairsDelta > 0 ? "Daha üst kat yok." : "Daha alt kat yok.");
                if (restoring) return PlacementCheck.Success;
                if (!Map.HasTile(entrance) || Map.IsBlocked(entrance) && !(ignored != null && Occupies(ignored, entrance)))
                    return PlacementCheck.Fail("Merdivenin önü açık olmalı.");
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                    {
                        var near = new GridPos(position.X + dx, position.Y + dy, position.Z);
                        if (!Map.EdgeFree(entrance, near)) return PlacementCheck.Fail("Merdiven ile önü arasında duvar var.");
                    }
                if (Map.HasTile(landing))
                {
                    if (Map.IsBlocked(landing)) return PlacementCheck.Fail("Merdivenin öbür ucu dolu.");
                }
                else if (!landingMayBeCreated)
                    return PlacementCheck.Fail(landing.Z > 0 ? "Merdivenin üst ucunda döşeme yok." : "Merdivenin alt ucu kazılmamış.");
            }
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Merdivenin iki ucu: girişi (kendi katında, merdivenin önü) ve inişi (öbür katta). Dönüş 0: +y yönünde tırmanır,
        /// 1: +x, 2: -y, 3: -x. Merdiven 1x2'dir.
        /// </summary>
        public static void StairsEnds(ItemDef def, GridPos position, int rotation, out GridPos entrance, out GridPos landing)
        {
            int z = position.Z, lz = position.Z + def.StairsDelta;
            switch (Footprint.Normalize(rotation))
            {
                case 0: entrance = new GridPos(position.X, position.Y - 1, z); landing = new GridPos(position.X, position.Y + 2, lz); break;
                case 1: entrance = new GridPos(position.X - 1, position.Y, z); landing = new GridPos(position.X + 2, position.Y, lz); break;
                case 2: entrance = new GridPos(position.X, position.Y + 2, z); landing = new GridPos(position.X, position.Y - 1, lz); break;
                default: entrance = new GridPos(position.X + 2, position.Y, z); landing = new GridPos(position.X - 1, position.Y, lz); break;
            }
        }

        /// <summary>Tüm merdiven eşyalarına göre kat bağlantılarını baştan kurar.</summary>
        public void RebuildLinks()
        {
            Map.ClearLinks();
            foreach (var item in _items)
            {
                if (!item.Def.IsStairs || item.IsDecor) continue;
                StairsEnds(item.Def, item.Position, item.Rotation, out var entrance, out var landing);
                if (Map.InBounds(entrance) && Map.InBounds(landing)) Map.AddLink(entrance, landing);
            }
        }

        public PlacementCheck CanPlaceOnSurface(ItemDef def, int surfaceItemId, string slotId, int? ignoreItemId = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement != ItemPlacement.Surface) return PlacementCheck.Fail("Bu eşya bir yüzeye konamaz.");

            var parent = GetItem(surfaceItemId);
            if (parent == null || parent.IsDecor) return PlacementCheck.Fail("Yüzey eşyası bulunamadı.");
            if (parent.Def.FindSlot(slotId) == null) return PlacementCheck.Fail("Bu yuva yok.");

            foreach (var other in _items)
            {
                if (other.ParentId == surfaceItemId && other.SlotId == slotId && other.Id != ignoreItemId)
                    return PlacementCheck.Fail("Bu yuva dolu.");
            }
            return PlacementCheck.Success;
        }

        /// <summary>Duvar eşyasının karonun verilen kenarına (WallSides) asılıp asılamayacağı.</summary>
        public PlacementCheck CanPlaceOnWall(ItemDef def, GridPos tile, int side, int? ignoreItemId = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement != ItemPlacement.Wall) return PlacementCheck.Fail("Bu eşya duvara asılmaz.");
            if (!Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            side = Footprint.Normalize(side);
            var edge = WallSides.EdgeOf(tile, side);
            if (!Map.EdgeInBounds(edge) || Map.GetEdge(edge) != EdgeKind.Wall) return PlacementCheck.Fail("Yalnızca düz duvara asılır (kapı ve pencereye değil).");
            foreach (var other in _items)
                if (other.Id != ignoreItemId && other.IsWallMounted && other.Position == tile && other.Rotation == side)
                    return PlacementCheck.Fail("Bu duvarda zaten bir eşya var.");
            return PlacementCheck.Success;
        }

        /// <summary>Duvar eşyasını karonun kenarına asar.</summary>
        public ItemInstance PlaceOnWall(ItemDef def, GridPos tile, int side)
        {
            var check = CanPlaceOnWall(def, tile, side);
            if (!check.Ok) throw new InvalidOperationException(check.Reason);
            var item = new ItemInstance(_nextId++, def, tile, Footprint.Normalize(side));
            _items.Add(item);
            Changed?.Invoke();
            return item;
        }

        /// <summary>Duvar eşyasını başka bir duvara taşır (üstündeki dekorlar da gelir).</summary>
        public PlacementCheck MoveWallItem(int itemId, GridPos tile, int side)
        {
            var item = GetItem(itemId);
            if (item == null || !item.IsWallMounted) return PlacementCheck.Fail("Duvar eşyası bulunamadı.");
            var check = CanPlaceOnWall(item.Def, tile, side, itemId);
            if (!check.Ok) return check;
            item.Position = tile;
            item.Rotation = Footprint.Normalize(side);
            foreach (var child in _items)
                if (child.ParentId == itemId) child.Position = tile;
            Changed?.Invoke();
            return PlacementCheck.Success;
        }

        /// <summary>Bir kenara (iki yüzüne de) asılı duvar eşyaları.</summary>
        public List<ItemInstance> WallItemsOn(Edge edge)
        {
            var list = new List<ItemInstance>();
            foreach (var item in _items)
                if (item.IsWallMounted && item.WallEdge.Equals(edge)) list.Add(item);
            return list;
        }

        private static bool Occupies(ItemInstance item, GridPos tile)
        {
            foreach (var t in item.OccupiedTiles()) if (t == tile) return true;
            return false;
        }

        // ---- yerleştirme ----

        /// <summary>Yer eşyası yerleştirir. Geçersizse InvalidOperationException.</summary>
        public ItemInstance PlaceItem(ItemDef def, GridPos position, int rotation = 0)
        {
            var check = CanPlaceFloor(def, position, rotation, null, landingMayBeCreated: false);
            if (!check.Ok) throw new InvalidOperationException($"{position}: {check.Reason}");

            var item = new ItemInstance(_nextId++, def, position, Footprint.Normalize(rotation));
            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);
            _items.Add(item);
            if (def.IsStairs) RebuildLinks();
            Changed?.Invoke();
            return item;
        }

        /// <summary>Dekor eşyasını bir yüzey yuvasına koyar.</summary>
        public ItemInstance PlaceOnSurface(ItemDef def, int surfaceItemId, string slotId)
        {
            var check = CanPlaceOnSurface(def, surfaceItemId, slotId);
            if (!check.Ok) throw new InvalidOperationException(check.Reason);

            var parent = GetItem(surfaceItemId);
            var item = new ItemInstance(_nextId++, def, parent.Position)
            {
                ParentId = surfaceItemId,
                SlotId = slotId
            };
            _items.Add(item);
            Changed?.Invoke();
            return item;
        }

        // ---- yapı modu işlemleri ----

        /// <summary>Yer eşyasını taşır ve/veya döndürür. Başarısızsa eşya olduğu yerde kalır.</summary>
        public PlacementCheck MoveItem(int itemId, GridPos position, int rotation)
        {
            var item = GetItem(itemId);
            if (item == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (item.IsDecor) return PlacementCheck.Fail("Dekor taşınamaz, yuvasından alınıp yeniden konur.");
            if (item.IsWallMounted) return MoveWallItem(itemId, position, rotation);

            var check = CanPlaceFloor(item.Def, position, rotation, itemId);
            if (!check.Ok) return check;

            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            item.Position = position;
            item.Rotation = Footprint.Normalize(rotation);
            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);

            // Üzerindeki dekorlar yeni konuma uyar.
            foreach (var child in _items)
                if (child.ParentId == itemId) child.Position = position;

            if (item.Def.IsStairs) RebuildLinks();
            Changed?.Invoke();
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Eşyayı (ve üzerindeki dekorları) haritadan kaldırır, tanımlarını döner (envantere
        /// iade için). Eşya yoksa boş liste.
        /// </summary>
        public List<ItemDef> RemoveItem(int itemId)
        {
            var returned = new List<ItemDef>();
            var item = GetItem(itemId);
            if (item == null) return returned;

            var children = new List<ItemInstance>();
            foreach (var other in _items)
                if (other.ParentId == itemId) children.Add(other);
            foreach (var c in children)
            {
                _items.Remove(c);
                returned.Add(c.Def);
            }

            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            _items.Remove(item);
            returned.Insert(0, item.Def);
            if (item.Def.IsStairs) RebuildLinks();
            Changed?.Invoke();
            return returned;
        }

        /// <summary>
        /// Kayıttan yükleme: eşyayı kimliğiyle olduğu gibi koyar. Yer eşyası karolarını kapatır; dekor,
        /// üstündeki eşya önce yüklenmiş olmalı. Changed tetiklenmez (yükleme sonunda görünüm baştan kurulur).
        /// </summary>
        internal ItemInstance RestoreItem(ItemDef def, int id, GridPos position, int rotation, int? parentId, string slotId,
            double fuelDays, double growthDays)
        {
            var item = new ItemInstance(id, def, position, Footprint.Normalize(rotation))
            {
                ParentId = parentId,
                SlotId = slotId,
                FuelDays = fuelDays,
                GrowthDays = growthDays
            };
            if (!item.IsDecor) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);
            _items.Add(item);
            if (id >= _nextId) _nextId = id + 1;
            if (def.IsStairs) RebuildLinks();
            return item;
        }

        /// <summary>Kimliğe göre eşya; yoksa null.</summary>
        public ItemInstance GetItem(int id)
        {
            foreach (var item in _items)
                if (item.Id == id) return item;
            return null;
        }

        /// <summary>Verilen karoyu kaplayan yer eşyası (dekor değil); yoksa null.</summary>
        public ItemInstance GetItemAt(GridPos tile)
        {
            foreach (var item in _items)
                if (!item.IsDecor && Occupies(item, tile)) return item;
            return null;
        }

        /// <summary>Bir yüzey eşyasının üzerindeki dekorlar.</summary>
        public List<ItemInstance> GetDecorOn(int surfaceItemId)
        {
            var list = new List<ItemInstance>();
            foreach (var item in _items)
                if (item.ParentId == surfaceItemId) list.Add(item);
            return list;
        }
    }
}
