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

        // ---- denetimler ----

        /// <summary>Yer eşyasının (def) verilen konum ve dönüşle konup konamayacağı. ignoreItemId: taşınan eşyanın kendisi.</summary>
        public PlacementCheck CanPlaceFloor(ItemDef def, GridPos position, int rotation, int? ignoreItemId = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement != ItemPlacement.Floor) return PlacementCheck.Fail("Bu eşya yere konamaz, bir yüzeye konur.");

            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            ItemInstance ignored = ignoreItemId.HasValue ? GetItem(ignoreItemId.Value) : null;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var tile = new GridPos(position.X + dx, position.Y + dy);
                    if (!Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışına taşıyor.");
                    if (Map.IsBlocked(tile) && !(ignored != null && Occupies(ignored, tile)))
                        return PlacementCheck.Fail("Orası dolu ya da geçilemez.");
                }
            }
            return PlacementCheck.Success;
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

        private static bool Occupies(ItemInstance item, GridPos tile)
        {
            foreach (var t in item.OccupiedTiles()) if (t == tile) return true;
            return false;
        }

        // ---- yerleştirme ----

        /// <summary>Yer eşyası yerleştirir. Geçersizse InvalidOperationException.</summary>
        public ItemInstance PlaceItem(ItemDef def, GridPos position, int rotation = 0)
        {
            var check = CanPlaceFloor(def, position, rotation);
            if (!check.Ok) throw new InvalidOperationException($"{position}: {check.Reason}");

            var item = new ItemInstance(_nextId++, def, position, Footprint.Normalize(rotation));
            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);
            _items.Add(item);
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

            var check = CanPlaceFloor(item.Def, position, rotation, itemId);
            if (!check.Ok) return check;

            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            item.Position = position;
            item.Rotation = Footprint.Normalize(rotation);
            foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);

            // Üzerindeki dekorlar yeni konuma uyar.
            foreach (var child in _items)
                if (child.ParentId == itemId) child.Position = position;

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
            Changed?.Invoke();
            return returned;
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
