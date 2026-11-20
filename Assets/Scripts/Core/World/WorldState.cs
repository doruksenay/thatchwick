using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Yerleştirme denetiminin sonucu: Ok ya da nedeni. Yeni nedenler anahtarlıdır (<see cref="Text"/>); eski nedenler düz
    /// Türkçe metindir ve dosya dosya anahtara taşınacak (belge 63 madde 4).
    /// </summary>
    public readonly struct PlacementCheck
    {
        private readonly string _reason;
        public bool Ok { get; }
        /// <summary>Anahtarlı neden; eski (düz metinli) nedenlerde boş.</summary>
        public Thatchwick.Core.Localization.LocText Text { get; }
        /// <summary>Arayüzde gösterilen neden. Anahtarlıysa oyun katmanının çevirdiği metin; çevirici yoksa (testler) anahtar.</summary>
        public string Reason => Text.IsEmpty ? _reason : (Resolve != null ? Resolve(Text) : Text.Key);
        /// <summary>Anahtarlı nedeni seçili dile çevirir (oyun katmanı açılışta kurar).</summary>
        public static System.Func<Thatchwick.Core.Localization.LocText, string> Resolve { get; set; }
        private PlacementCheck(bool ok, string reason, Thatchwick.Core.Localization.LocText text) { Ok = ok; _reason = reason; Text = text; }
        public static PlacementCheck Success => new PlacementCheck(true, null, default);
        public static PlacementCheck Fail(string reason) => new PlacementCheck(false, reason, default);
        public static PlacementCheck Fail(Thatchwick.Core.Localization.LocText text) => new PlacementCheck(false, null, text);
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

        /// <summary>Eşyanın görünüşü değişti (kaplama): görünüm yenilensin.</summary>
        public void NotifyChanged() { _touchedAll = true; OnChanged(); }

        /// <summary>Yalnızca bu eşyanın görünümü değişti (mum, desen, cam bahçe): görünüm yalnızca onu yeniden kurar.</summary>
        public void NotifyChanged(int itemId) { Touch(itemId); OnChanged(); }

        // ---- değişiklik izleme (görünüm farkı; performans incelemesi 20.11.2026) ----
        // Changed yayınlanınca görünüm TakeChanges ile hangi eşyaların değiştiğini alır; yalnızca onları yeniden kurar.
        // Kaynağı belirsiz değişiklik (NotifyChanged(), RaiseChanged, halı sırası, toplu kaydırma) "hepsi" sayılır.
        private readonly HashSet<int> _touched = new HashSet<int>();
        private bool _touchedAll = true;

        private void Touch(int itemId) => _touched.Add(itemId);

        // Eşya ve (yüzeyse) üstündeki dekorlar, (dekorsa) altındaki yüzey.
        private void TouchWithFamily(ItemInstance item)
        {
            if (item == null) return;
            _touched.Add(item.Id);
            if (item.ParentId.HasValue) _touched.Add(item.ParentId.Value);
            foreach (var other in _items) if (other.ParentId == item.Id) _touched.Add(other.Id);
        }

        /// <summary>
        /// Son çağrıdan beri değişen eşya kimliklerini <paramref name="into"/>'ya yazar. Değişikliğin kaynağı belirsizse
        /// (bütün görünüm yeniden kurulmalı) true döner.
        /// </summary>
        public bool TakeChanges(ICollection<int> into)
        {
            bool all = _touchedAll;
            if (!all && into != null) foreach (var id in _touched) into.Add(id);
            _touched.Clear();
            _touchedAll = false;
            return all;
        }

        // ---- dizinler (performans incelemesi 20.11.2026) ----
        // Kimliğe göre sözlük her ekleme/çıkarmada güncel tutulur. Karo dizini (GetItemAt) tembel kurulur;
        // eşya eklenince, çıkınca, taşınınca ya da Changed yayınlanınca geçersiz olur.
        private readonly Dictionary<int, ItemInstance> _byId = new Dictionary<int, ItemInstance>();
        private readonly Dictionary<GridPos, ItemInstance> _tileIndex = new Dictionary<GridPos, ItemInstance>();
        private bool _tileIndexValid;

        // Otonomi adayları: herhangi bir ihtiyacı gideren eşyalar, dünya listesiyle aynı sırada (eşit puanda ilk gelen
        // kazandığı için sıra korunur). Otonomi 1.500 eşyanın hepsi yerine yalnızca bunları tarar.
        private readonly List<ItemInstance> _needItems = new List<ItemInstance>();

        /// <summary>Herhangi bir ihtiyacı gideren etkileşimi olan eşyalar (<see cref="Items"/> ile aynı sırada).</summary>
        public IReadOnlyList<ItemInstance> NeedItems => _needItems;

        // İhtiyaç başına adaylar (otonominin o ihtiyaç için seçebileceği etkileşimi olan eşyalar), aynı sırada.
        private static readonly int NeedTypeCount = System.Enum.GetValues(typeof(Characters.NeedType)).Length;
        private readonly List<ItemInstance>[] _needBuckets = NewBuckets();
        private static List<ItemInstance>[] NewBuckets()
        {
            var b = new List<ItemInstance>[NeedTypeCount];
            for (int i = 0; i < b.Length; i++) b[i] = new List<ItemInstance>();
            return b;
        }

        /// <summary>Otonominin verilen ihtiyaç için seçebileceği eşyalar (<see cref="Items"/> ile aynı sırada).</summary>
        public IReadOnlyList<ItemInstance> NeedItemsFor(Characters.NeedType need) => _needBuckets[(int)need];

        /// <summary>Eşya her eklendiğinde ya da çıkarıldığında artar (eşya listesinden türetilen önbellekler için).</summary>
        public int ItemsVersion { get; private set; }

        private void AddToIndex(ItemInstance item)
        {
            ItemsVersion++;
            if (item.Def.RestoresAnyNeed)
            {
                _needItems.Add(item);
                for (int n = 0; n < NeedTypeCount; n++)
                    if (item.Def.AutonomyInteractions((Characters.NeedType)n).Length > 0) _needBuckets[n].Add(item);
            }
            _touched.Add(item.Id);
            if (item.ParentId.HasValue) _touched.Add(item.ParentId.Value);
            _items.Add(item);
            _byId[item.Id] = item;
            _tileIndexValid = false;
        }

        private void RemoveFromIndex(ItemInstance item)
        {
            if (!_items.Remove(item)) return;
            ItemsVersion++;
            if (item.Def.RestoresAnyNeed)
            {
                _needItems.Remove(item);
                for (int n = 0; n < NeedTypeCount; n++)
                    if (item.Def.AutonomyInteractions((Characters.NeedType)n).Length > 0) _needBuckets[n].Remove(item);
            }
            _touched.Add(item.Id);
            if (item.ParentId.HasValue) _touched.Add(item.ParentId.Value);
            if (_byId.TryGetValue(item.Id, out var known) && ReferenceEquals(known, item)) _byId.Remove(item.Id);
            _tileIndexValid = false;
        }

        private void OnChanged()
        {
            _tileIndexValid = false;
            Changed?.Invoke();
        }

        private void EnsureTileIndex()
        {
            if (_tileIndexValid) return;
            _tileIndex.Clear();
            // Liste sırası korunur: aynı karoda ilk gelen eşya döner (eski doğrusal aramayla aynı sonuç).
            foreach (var item in _items)
            {
                if (item.IsDecor || item.IsRug) continue;
                foreach (var t in item.OccupiedTiles())
                    if (!_tileIndex.ContainsKey(t)) _tileIndex[t] = item;
            }
            _tileIndexValid = true;
        }

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
        /// <summary>Kapalı oda isteyen eşyanın (folluk, ahır bölmesi) açıkta konamama nedeni.</summary>
        public const string IndoorOnly = "Bu eşya kapalı bir odaya konur (kümes, ahır, at ahırı).";

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
                    if (Map.IsWater(tile)) return PlacementCheck.Fail("Suyun üstüne bir şey konmaz.");
                    if (restoring) continue;
                    if (IsOccupiedByCharacter(tile)) return PlacementCheck.Fail("Orada biri duruyor.");
                    // Merdivenin önü ve inişi açık kalmalı (taşınan şey merdivenin kendisi değilse).
                    if (Map.HasLinkAt(tile) && !(ignored != null && ignored.Def.IsStairs))
                        return PlacementCheck.Fail("Merdivenin ucu kapanmasın.");
                }
            }

            if (def.Tall && !restoring)
            {
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                    {
                        var tile = new GridPos(position.X + dx, position.Y + dy, position.Z);
                        foreach (var other in _items)
                            if (other.IsWallMounted && other.Id != ignoreItemId && other.WallBand < Decor.BandHigh && HangsInFrontOf(other, tile))
                                return PlacementCheck.Fail($"Uzun eşya duvardaki {other.Def.DisplayName} önünü kapatır.");
                    }
            }

            if (def.RequiresIndoor && !restoring)
            {
                var room = Map.Rooms.RoomAt(position);
                if (room == null || !room.IsIndoor) return PlacementCheck.Fail(IndoorOnly);
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

        /// <summary>
        /// Eşyaya bağlı olmayan sabit kat bağlantıları (ağaç köşkünün ip merdiveni, belge 52). RebuildLinks bunları da
        /// yeniden kurar. Kayda yazılmaz; sahibi sistem yüklemeden sonra yeniden ekler.
        /// </summary>
        public readonly List<KeyValuePair<GridPos, GridPos>> FixedLinks = new List<KeyValuePair<GridPos, GridPos>>();

        /// <summary>Sabit bağlantı ekler ve hemen kurar (aynısı varsa eklemez).</summary>
        public void AddFixedLink(GridPos a, GridPos b)
        {
            foreach (var l in FixedLinks) if (l.Key.Equals(a) && l.Value.Equals(b)) return;
            FixedLinks.Add(new KeyValuePair<GridPos, GridPos>(a, b));
            if (Map.InBounds(a) && Map.InBounds(b)) Map.AddLink(a, b);
        }

        /// <summary>Tüm merdiven eşyalarına göre kat bağlantılarını baştan kurar.</summary>
        public void RebuildLinks()
        {
            Map.ClearLinks();
            foreach (var l in FixedLinks) if (Map.InBounds(l.Key) && Map.InBounds(l.Value)) Map.AddLink(l.Key, l.Value);
            foreach (var item in _items)
            {
                if (!item.Def.IsStairs || item.IsDecor) continue;
                StairsEnds(item.Def, item.Position, item.Rotation, out var entrance, out var landing);
                if (Map.InBounds(entrance) && Map.InBounds(landing)) Map.AddLink(entrance, landing);
            }
        }

        /// <summary>
        /// Halı serilebilir mi: karolar var ve arazi engeli yok (mobilyanın altına serilir). Halılar sınırsız üst üste
        /// serilir (karar 18.11.2026); yeni halı en üste gelir. Halı yolu kapatmaz.
        /// </summary>
        public PlacementCheck CanPlaceRug(ItemDef def, GridPos position, int rotation, int? ignoreItemId = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (!def.IsRug) return PlacementCheck.Fail("Bu eşya halı değil.");
            if (!GridMap.IsValidLevel(position.Z)) return PlacementCheck.Fail("Geçersiz kat.");
            if (!def.AllowsLevel(position.Z)) return PlacementCheck.Fail("Bu halı bu katta serilmez.");
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                {
                    var tile = new GridPos(position.X + dx, position.Y + dy, position.Z);
                    if (!Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışına taşıyor.");
                    if (!Map.HasTile(tile)) return PlacementCheck.Fail(position.Z > 0 ? "Önce üst kat döşemesi gerekir." : "Önce burayı kazmalısın.");
                    if (Map.IsBlocked(tile) && GetItemAt(tile) == null) return PlacementCheck.Fail("Orası geçilemez.");
                }
            return PlacementCheck.Success;
        }

        /// <summary>Karodaki en üstteki halı; yoksa null.</summary>
        public ItemInstance GetRugAt(GridPos tile)
        {
            ItemInstance top = null;
            foreach (var item in _items)
                if (item.IsRug && Occupies(item, tile) && (top == null || item.LayerOrder > top.LayerOrder)) top = item;
            return top;
        }

        /// <summary>Karodaki halılar, alttan üste.</summary>
        public List<ItemInstance> RugsAt(GridPos tile)
        {
            var list = new List<ItemInstance>();
            foreach (var item in _items)
                if (item.IsRug && Occupies(item, tile)) list.Add(item);
            list.Sort((a, b) => a.LayerOrder.CompareTo(b.LayerOrder));
            return list;
        }

        private int _nextLayer = 1;
        private int NextLayer() => _nextLayer++;

        /// <summary>Halıyı en üste alır.</summary>
        public bool RugToFront(int itemId)
        {
            var rug = GetItem(itemId);
            if (rug == null || !rug.IsRug) return false;
            rug.LayerOrder = NextLayer();
            _touchedAll = true; OnChanged();
            return true;
        }

        /// <summary>Halıyı en alta indirir.</summary>
        public bool RugToBack(int itemId)
        {
            var rug = GetItem(itemId);
            if (rug == null || !rug.IsRug) return false;
            int min = int.MaxValue;
            foreach (var it in _items) if (it.IsRug && it.LayerOrder < min) min = it.LayerOrder;
            rug.LayerOrder = min == int.MaxValue ? 0 : min - 1;
            _touchedAll = true; OnChanged();
            return true;
        }

        /// <summary>Halıyı bir kat yukarı (dir +1) ya da aşağı (-1) alır: üstünden ya da altından geçen ilk halıyla yer değiştirir.</summary>
        public bool RugStep(int itemId, int dir)
        {
            var rug = GetItem(itemId);
            if (rug == null || !rug.IsRug || dir == 0) return false;
            var tiles = new HashSet<GridPos>(rug.OccupiedTiles());
            ItemInstance swap = null;
            foreach (var it in _items)
            {
                if (!it.IsRug || it.Id == itemId) continue;
                bool overlaps = false;
                foreach (var t in it.OccupiedTiles()) if (tiles.Contains(t)) { overlaps = true; break; }
                if (!overlaps) continue;
                if (dir > 0 && it.LayerOrder > rug.LayerOrder && (swap == null || it.LayerOrder < swap.LayerOrder)) swap = it;
                if (dir < 0 && it.LayerOrder < rug.LayerOrder && (swap == null || it.LayerOrder > swap.LayerOrder)) swap = it;
            }
            if (swap == null) return false;
            int tmp = rug.LayerOrder; rug.LayerOrder = swap.LayerOrder; swap.LayerOrder = tmp;
            _touchedAll = true; OnChanged();
            return true;
        }

        // ---- tavan ----

        /// <summary>Tavan eşyası asılabilir mi: kapalı iç oda, döşeli karo, karoda başka tavan eşyası yok.</summary>
        public PlacementCheck CanPlaceCeiling(ItemDef def, GridPos tile, int? ignoreItemId = null, bool restoring = false)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (!def.IsCeiling) return PlacementCheck.Fail("Bu eşya tavana asılmaz.");
            if (!GridMap.IsValidLevel(tile.Z) || !Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!Map.HasTile(tile)) return PlacementCheck.Fail("Burada döşeme yok.");
            if (!restoring)
            {
                var room = Map.Rooms.RoomAt(tile);
                if (room == null || !room.IsIndoor) return PlacementCheck.Fail("Tavan eşyası kapalı bir odaya asılır.");
            }
            var other = CeilingAt(tile);
            if (other != null && other.Id != ignoreItemId) return PlacementCheck.Fail("Bu karonun tavanında zaten bir eşya var.");
            return PlacementCheck.Success;
        }

        /// <summary>Karonun tavanına asılı eşya; yoksa null.</summary>
        public ItemInstance CeilingAt(GridPos tile)
        {
            foreach (var item in _items)
                if (item.IsCeiling && item.Position == tile) return item;
            return null;
        }

        public ItemInstance PlaceOnCeiling(ItemDef def, GridPos tile)
        {
            var check = CanPlaceCeiling(def, tile);
            if (!check.Ok) throw new InvalidOperationException(check.Reason);
            var item = new ItemInstance(_nextId++, def, tile);
            AddToIndex(item);
            OnChanged();
            return item;
        }

        public PlacementCheck CanPlaceOnSurface(ItemDef def, int surfaceItemId, string slotId, int? ignoreItemId = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement != ItemPlacement.Surface) return PlacementCheck.Fail("Bu eşya bir yüzeye konamaz.");

            var parent = GetItem(surfaceItemId);
            if (parent == null || parent.IsDecor) return PlacementCheck.Fail("Yüzey eşyası bulunamadı.");
            var slot = parent.Def.FindSlot(slotId);
            if (slot == null) return PlacementCheck.Fail("Bu yuva yok.");
            if (parent.Condition == ItemCondition.Damaged) return PlacementCheck.Fail($"{parent.Def.DisplayName} hasarlı: önce onarılmalı.");

            string kind = parent.Def.SlotKind(slot);
            if (!def.FitsKind(kind)) return PlacementCheck.Fail($"Bu yuva yalnızca {Decor.KindName(kind)} eşyası alır; {def.DisplayName} oraya uymaz.");
            if (def.DecorSize > slot.Size)
                return PlacementCheck.Fail($"{def.DisplayName} bu yuvaya büyük ({Decor.SizeLetter(def.DecorSize)} > {Decor.SizeLetter(slot.Size)}).");

            foreach (var other in _items)
            {
                if (other.ParentId == surfaceItemId && other.SlotId == slotId && other.Id != ignoreItemId)
                    return PlacementCheck.Fail("Bu yuva dolu.");
            }
            return PlacementCheck.Success;
        }

        /// <summary>Yüzeyin üstündeki dekorların toplam yükü.</summary>
        public int LoadOf(int surfaceItemId, int? ignoreItemId = null)
        {
            int load = 0;
            foreach (var other in _items)
                if (other.ParentId == surfaceItemId && other.Id != ignoreItemId) load += Math.Max(0, other.Def.Weight);
            return load;
        }

        /// <summary>Bu dekor konunca yüzeyin taşıması aşılır mı (aşım engellenmez, raf çökebilir: karar 18.11.2026).</summary>
        public bool WouldOverload(ItemDef def, int surfaceItemId, int? ignoreItemId = null)
        {
            var parent = GetItem(surfaceItemId);
            if (parent == null || parent.Def.MaxLoad <= 0 || def == null) return false;
            return LoadOf(surfaceItemId, ignoreItemId) + Math.Max(0, def.Weight) > parent.Def.MaxLoad;
        }

        /// <summary>Yüzey aşırı yüklü mü (çökme şansı var).</summary>
        public bool IsOverloaded(ItemInstance surface) =>
            surface != null && surface.Def.MaxLoad > 0 && LoadOf(surface.Id) > surface.Def.MaxLoad;

        /// <summary>Duvar eşyasının karonun verilen kenarına (WallSides) asılıp asılamayacağı. band &lt; 0: eşyanın ilk boş bandı.</summary>
        public PlacementCheck CanPlaceOnWall(ItemDef def, GridPos tile, int side, int? ignoreItemId = null) =>
            CanPlaceOnWall(def, tile, side, -1, ignoreItemId);

        /// <summary>
        /// Duvar eşyası verilen banda asılabilir mi: geniş eşyanın bütün kenarları düz duvar olmalı (kapı ve pencere değil);
        /// aynı yüzde aynı bandı başka eşya tutmamalı; alt ve orta bandın önünde uzun mobilya olmamalı.
        /// </summary>
        public PlacementCheck CanPlaceOnWall(ItemDef def, GridPos tile, int side, int band, int? ignoreItemId, bool restoring = false)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Placement != ItemPlacement.Wall) return PlacementCheck.Fail("Bu eşya duvara asılmaz.");
            if (!Map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            side = Footprint.Normalize(side);
            if (band < 0)
            {
                PlacementCheck first = PlacementCheck.Fail("Bu duvarda yer yok.");
                foreach (var b in def.EffectiveBands)
                {
                    var c = CanPlaceOnWall(def, tile, side, b, ignoreItemId, restoring);
                    if (c.Ok) return c;
                    if (b == def.DefaultBand) first = c;
                }
                return first;
            }
            if (!def.AllowsBand(band)) return PlacementCheck.Fail($"{def.DisplayName} {Decor.BandName(band)} banda asılmaz.");
            int span = Math.Max(1, def.WallWidth);
            foreach (var (t, edge) in WallSides.Segments(tile, side, span))
            {
                if (!Map.InBounds(t)) return PlacementCheck.Fail("Harita dışı.");
                if (def.WindowMount)
                {
                    if (!Map.EdgeInBounds(edge) || Map.GetEdge(edge) != EdgeKind.Window)
                        return PlacementCheck.Fail($"{def.DisplayName} yalnızca pencereye asılır.");
                }
                else if (!Map.EdgeInBounds(edge) || Map.GetEdge(edge) != EdgeKind.Wall)
                    return PlacementCheck.Fail(span > 1 ? $"{def.DisplayName} {span} karo düz duvar ister (kapı ve pencere araya girmesin)." : "Yalnızca düz duvara asılır (kapı ve pencereye değil).");
                if (def.IsNiche && Array.IndexOf(Decor.ThickWalls, Map.GetEdgeStyle(edge)) < 0)
                    return PlacementCheck.Fail($"{def.DisplayName} yalnızca kalın duvara oyulur (kerpiç, taş ya da kesme taş).");
                foreach (var other in _items)
                {
                    if (other.Id == ignoreItemId || !other.IsWallMounted || other.Rotation != side || other.WallBand != band) continue;
                    foreach (var seg in other.WallSegments())
                        if (seg.tile == t) return PlacementCheck.Fail($"Duvarın {Decor.BandName(band)} bandında zaten {other.Def.DisplayName} var.");
                }
                if (!restoring && band < Decor.BandHigh)
                {
                    var front = GetItemAt(t);
                    if (front != null && front.Def.Tall && front.Id != ignoreItemId)
                        return PlacementCheck.Fail($"Önünde uzun bir eşya ({front.Def.DisplayName}) var: yalnızca üst banda asılır.");
                }
            }
            return PlacementCheck.Success;
        }

        /// <summary>Bu karonun önündeki duvarda asılı mı (geniş eşyanın herhangi bir kenarı).</summary>
        private static bool HangsInFrontOf(ItemInstance wallItem, GridPos tile)
        {
            foreach (var seg in wallItem.WallSegments()) if (seg.tile == tile) return true;
            return false;
        }

        /// <summary>Duvar eşyasını karonun kenarına asar (band &lt; 0: ilk uygun bant).</summary>
        public ItemInstance PlaceOnWall(ItemDef def, GridPos tile, int side, int band = -1)
        {
            int chosen = band >= 0 ? band : FirstFreeBand(def, tile, side, null);
            var check = CanPlaceOnWall(def, tile, side, chosen, null);
            if (!check.Ok) throw new InvalidOperationException(check.Reason);
            var item = new ItemInstance(_nextId++, def, tile, Footprint.Normalize(side)) { WallBand = chosen };
            AddToIndex(item);
            OnChanged();
            return item;
        }

        /// <summary>Eşyanın asılabileceği ilk bant (varsayılan bant önce); hiçbiri uymuyorsa varsayılan bant.</summary>
        public int FirstFreeBand(ItemDef def, GridPos tile, int side, int? ignoreItemId)
        {
            if (CanPlaceOnWall(def, tile, side, def.DefaultBand, ignoreItemId).Ok) return def.DefaultBand;
            foreach (var b in def.EffectiveBands)
                if (CanPlaceOnWall(def, tile, side, b, ignoreItemId).Ok) return b;
            return def.DefaultBand;
        }

        /// <summary>Duvar eşyasını başka bir duvara ya da banda taşır (üstündeki dekorlar da gelir). band &lt; 0: bandı korur, olmazsa ilk uygun bant.</summary>
        public PlacementCheck MoveWallItem(int itemId, GridPos tile, int side, int band = -1)
        {
            var item = GetItem(itemId);
            if (item == null || !item.IsWallMounted) return PlacementCheck.Fail("Duvar eşyası bulunamadı.");
            int b = band;
            if (b < 0) b = CanPlaceOnWall(item.Def, tile, side, item.WallBand, itemId).Ok ? item.WallBand : FirstFreeBand(item.Def, tile, side, itemId);
            var check = CanPlaceOnWall(item.Def, tile, side, b, itemId);
            if (!check.Ok) return check;
            item.Position = tile;
            _tileIndexValid = false;
            TouchWithFamily(item);
            item.Rotation = Footprint.Normalize(side);
            item.WallBand = b;
            foreach (var child in _items)
                if (child.ParentId == itemId) child.Position = tile;
            OnChanged();
            return PlacementCheck.Success;
        }

        /// <summary>Bir kenara (iki yüzüne de) asılı duvar eşyaları (geniş eşyanın herhangi bir kenarı).</summary>
        public List<ItemInstance> WallItemsOn(Edge edge)
        {
            var list = new List<ItemInstance>();
            foreach (var item in _items)
                if (item.IsWallMounted && item.HangsOn(edge)) list.Add(item);
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
            if (def != null && def.IsCeiling) return PlaceOnCeiling(def, position);
            var check = def != null && def.IsRug ? CanPlaceRug(def, position, rotation) : CanPlaceFloor(def, position, rotation, null, landingMayBeCreated: false);
            if (!check.Ok) throw new InvalidOperationException($"{position}: {check.Reason}");

            var item = new ItemInstance(_nextId++, def, position, Footprint.Normalize(rotation));
            if (def.IsRug) item.LayerOrder = NextLayer();
            if (!def.IsRug) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);
            AddToIndex(item);
            if (def.IsStairs) RebuildLinks();
            OnChanged();
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
            AddToIndex(item);
            OnChanged();
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
            if (item.IsCeiling)
            {
                var cc = CanPlaceCeiling(item.Def, position, itemId);
                if (!cc.Ok) return cc;
                item.Position = position;
                _tileIndexValid = false;
                TouchWithFamily(item);
                foreach (var child in _items) if (child.ParentId == itemId) child.Position = position;
                OnChanged();
                return PlacementCheck.Success;
            }

            var check = item.IsRug ? CanPlaceRug(item.Def, position, rotation, itemId) : CanPlaceFloor(item.Def, position, rotation, itemId);
            if (!check.Ok) return check;

            if (!item.IsRug) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            item.Position = position;
            _tileIndexValid = false;
            TouchWithFamily(item);
            item.Rotation = Footprint.Normalize(rotation);
            if (!item.IsRug) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);

            // Üzerindeki dekorlar yeni konuma uyar.
            foreach (var child in _items)
                if (child.ParentId == itemId) child.Position = position;

            if (item.Def.IsStairs) RebuildLinks();
            OnChanged();
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Eşyaları birlikte (dx, dy) kaydırır (oda taşıma): yer eşyaları, halılar ve duvar eşyaları; dekorlar üstündeki
        /// eşyayla gelir. Kimlik ve durum (yakıt, stok, kaplama) korunur. Hedeflerden biri uymazsa hiçbiri kaymaz.
        /// Duvar eşyalarının hedef duvarı önceden kurulmuş olmalı.
        /// </summary>
        public PlacementCheck ShiftItems(ICollection<int> ids, int dx, int dy)
        {
            var moving = new List<ItemInstance>();
            var set = new HashSet<int>();
            foreach (var id in ids)
            {
                var it = GetItem(id);
                if (it == null || it.IsDecor || !set.Add(id)) continue;
                moving.Add(it);
            }
            if (moving.Count == 0 || dx == 0 && dy == 0) return PlacementCheck.Success;

            foreach (var it in moving)
                if (Blocks(it)) foreach (var t in it.OccupiedTiles()) Map.SetBlocked(t, false);

            string fail = null;
            foreach (var it in moving)
            {
                var np = new GridPos(it.Position.X + dx, it.Position.Y + dy, it.Position.Z);
                if (it.IsWallMounted)
                {
                    foreach (var (st, e) in WallSides.Segments(np, it.Rotation, it.WallSpan))
                    {
                        if (!Map.EdgeInBounds(e) || Map.GetEdge(e) != EdgeKind.Wall) { fail = $"{it.Def.DisplayName} için hedefte duvar yok."; break; }
                        foreach (var other in _items)
                        {
                            if (set.Contains(other.Id) || !other.IsWallMounted || other.Rotation != it.Rotation || other.WallBand != it.WallBand) continue;
                            foreach (var seg in other.WallSegments()) if (seg.tile == st) { fail = "Hedef duvarda başka bir eşya var."; break; }
                            if (fail != null) break;
                        }
                        if (fail != null) break;
                    }
                    if (fail != null) break;
                    continue;
                }
                if (it.IsCeiling)
                {
                    if (!Map.InBounds(np) || !Map.HasTile(np)) { fail = "Hedef harita ya da kat dışında."; break; }
                    foreach (var other in _items)
                        if (!set.Contains(other.Id) && other.IsCeiling && other.Position == np) { fail = "Hedefin tavanında başka bir eşya var."; break; }
                    if (fail != null) break;
                    continue;
                }
                var (w, h) = Footprint.Dimensions(it.Def.Width, it.Def.Height, it.Rotation);
                for (int yy = 0; yy < h && fail == null; yy++)
                    for (int xx = 0; xx < w && fail == null; xx++)
                    {
                        var t = new GridPos(np.X + xx, np.Y + yy, np.Z);
                        if (!Map.InBounds(t) || !Map.HasTile(t)) { fail = "Hedef harita ya da kat dışında."; break; }
                        if (it.IsRug)
                        {
                            if (Map.IsBlocked(t) && GetItemAt(t) == null) fail = "Hedefte geçilmez bir yer var.";
                            continue;
                        }
                        if (Map.IsBlocked(t)) fail = $"{it.Def.DisplayName} için hedef dolu.";
                        else if (IsOccupiedByCharacter(t)) fail = "Hedefte biri duruyor.";
                    }
                if (fail == null && it.Def.IsStairs)
                {
                    StairsEnds(it.Def, np, it.Rotation, out var entrance, out var landing);
                    if (!Map.HasTile(entrance) || !Map.HasTile(landing)) fail = "Merdivenin uçları hedefte açık değil.";
                }
                if (fail != null) break;
            }

            if (fail != null)
            {
                foreach (var it in moving)
                    if (Blocks(it)) foreach (var t in it.OccupiedTiles()) Map.SetBlocked(t, true);
                return PlacementCheck.Fail(fail);
            }

            bool stairs = false;
            foreach (var it in moving)
            {
                it.Position = new GridPos(it.Position.X + dx, it.Position.Y + dy, it.Position.Z);
                _tileIndexValid = false;
                TouchWithFamily(it);
                if (Blocks(it)) foreach (var t in it.OccupiedTiles()) Map.SetBlocked(t, true);
                foreach (var child in _items) if (child.ParentId == it.Id) child.Position = it.Position;
                stairs |= it.Def.IsStairs;
            }
            if (stairs) RebuildLinks();
            OnChanged();
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Eşyayı (ve üzerindeki dekorları) haritadan kaldırır, tanımlarını döner (envantere
        /// iade için). Eşya yoksa boş liste.
        /// </summary>
        public List<ItemDef> RemoveItem(int itemId)
        {
            var returned = new List<ItemDef>();
            foreach (var it in RemoveItemInstances(itemId)) returned.Add(it.Def);
            return returned;
        }

        /// <summary>RemoveItem gibi, ama örnekleri döner (durum ve içindekiler envantere taşınabilsin). İlk eleman eşyanın kendisi.</summary>
        public List<ItemInstance> RemoveItemInstances(int itemId)
        {
            var returned = new List<ItemInstance>();
            var item = GetItem(itemId);
            if (item == null) return returned;

            var children = new List<ItemInstance>();
            foreach (var other in _items)
                if (other.ParentId == itemId) children.Add(other);
            foreach (var c in children)
            {
                RemoveFromIndex(c);
                returned.Add(c);
            }

            if (Blocks(item)) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            RemoveFromIndex(item);
            returned.Insert(0, item);
            if (item.Def.IsStairs) RebuildLinks();
            OnChanged();
            return returned;
        }

        /// <summary>Dekor eşyasını yuvasından kaldırır (yalnızca kendisi).</summary>
        internal void RemoveSingle(ItemInstance item)
        {
            if (item == null) return;
            if (Blocks(item)) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, false);
            RemoveFromIndex(item);
        }

        /// <summary>Karo kapatan yer eşyası mı (halı, dekor, duvar ve tavan eşyası kapatmaz).</summary>
        private static bool Blocks(ItemInstance item) => !item.IsRug && !item.IsDecor && !item.IsWallMounted && !item.IsCeiling;

        /// <summary>Dekor sistemi için olay yayını.</summary>
        internal void RaiseChanged() { _touchedAll = true; OnChanged(); }

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
            if (Blocks(item)) foreach (var t in item.OccupiedTiles()) Map.SetBlocked(t, true);
            AddToIndex(item);
            if (id >= _nextId) _nextId = id + 1;
            if (def.IsStairs) RebuildLinks();
            return item;
        }

        /// <summary>Kayıttan yükleme: duvar bandı, halı sırası, durum ve kararma.</summary>
        internal void RestoreDecorState(ItemInstance item, int band, int layer, ItemCondition condition, double tarnish)
        {
            if (item == null) return;
            if (item.IsWallMounted && band >= 0 && band < Decor.BandCount) item.WallBand = band;
            if (item.IsRug)
            {
                item.LayerOrder = layer;
                if (layer >= _nextLayer) _nextLayer = layer + 1;
            }
            item.Condition = condition;
            item.TarnishDays = Math.Max(0, tarnish);
        }

        /// <summary>Kimliğe göre eşya; yoksa null.</summary>
        public ItemInstance GetItem(int id) => _byId.TryGetValue(id, out var item) ? item : null;

        /// <summary>Verilen karoyu kaplayan yer eşyası (dekor ve halı değil); yoksa null.</summary>
        public ItemInstance GetItemAt(GridPos tile)
        {
            EnsureTileIndex();
            return _tileIndex.TryGetValue(tile, out var item) ? item : null;
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
