using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>GameRoot'un eşya görünümlerine verdiği bağlam (ItemViews GameRoot'un iç alanlarını görmez).</summary>
    public interface IItemViewContext
    {
        WorldState World { get; }
        WorldView View { get; }
        WallView Walls { get; }
        LifeSystems Life { get; }
        ContentDatabase Content { get; }
        GameClock Clock { get; }
        Juice Juice { get; }
        /// <summary>Etkin bölge (bölgesiz dünyada null): yalnızca bu bölgenin eşyaları kurulur.</summary>
        Region Region { get; }
        /// <summary>Köy meydanı (şenlik bayrakları için).</summary>
        GridPos SquarePlace { get; }
    }

    /// <summary>
    /// Eşya görünümleri (GameRoot'tan ayrıldı, performans incelemesi 20.11.2026): eşya başına görünüm kovası, değişen eşyanın
    /// yerinde güncellenmesi (WorldState.TakeChanges), yalnızca etkin bölgenin kurulması, bitki ve ocak görünümleri.
    /// </summary>
    public sealed class ItemViews
    {
        private readonly IItemViewContext _ctx;
        private readonly Transform _parent;

        public ItemViews(IItemViewContext context, Transform parent)
        {
            _ctx = context;
            _parent = parent;
        }

        // Taşınan kod GameRoot alanlarıyla yazılmıştı; aynı adlar bağlamdan okunur.
        private WorldState _world => _ctx.World;
        private WorldView _view => _ctx.View;
        private WallView _walls => _ctx.Walls;
        private LifeSystems _life => _ctx.Life;
        private ContentDatabase _content => _ctx.Content;
        private GameClock _clock => _ctx.Clock;
        private Juice _juice => _ctx.Juice;
        private Region _region => _ctx.Region;
        private Transform transform => _parent;
        private static void Destroy(Object o) => Object.Destroy(o);

        // ---- GameRoot'un kullandığı yüz ----

        /// <summary>Eşya kimliği → ilk görünüm nesnesi (etiket, ışık, etkileşim için).</summary>
        public IReadOnlyDictionary<int, Transform> Views => _itemViews;
        public IReadOnlyDictionary<int, PlantView> Plants => _plantViews;
        public IReadOnlyDictionary<int, (ItemInstance item, SpriteRenderer view)> Heat => _heatViews;
        public bool BuntingShown => _buntingShown;

        private bool _itemsDirty;

        public sealed class PlantView { public ItemInstance Item; public Transform View; public SpriteRenderer Sr; public Transform Shadow; }
        private readonly Dictionary<int, PlantView> _plantViews = new Dictionary<int, PlantView>();
        private readonly Dictionary<int, (ItemInstance item, SpriteRenderer view)> _heatViews = new Dictionary<int, (ItemInstance, SpriteRenderer)>();
        private readonly Dictionary<int, Transform> _itemViews = new Dictionary<int, Transform>();
        private readonly HashSet<int> _seenItemIds = new HashSet<int>();
        private bool _seenItemsReady;
        private bool _buntingShown;

        /// <summary>WorldState.Changed: değişen eşyalar bir sonraki <see cref="Apply"/>'da güncellenir.</summary>
        public void MarkChanged() => _itemsDirty = true;

        /// <summary>Bölge değişti: yeni bölgenin eşyaları "yeni geldi" diye zıplamasın.</summary>
        public void ResetPop() => _seenItemsReady = false;

        /// <summary>
        /// LateUpdate: değişen eşyalar (WorldState.TakeChanges) yerinde güncellenir; kaynağı belirsiz değişiklik, kat, duvar kipi,
        /// bölge ya da mevsim değişimi bütün bölgeyi yeniden kurar. Halı sırası komşu halıları etkilediği için o da tam kurulum.
        /// Bir şey yapıldıysa true.
        /// </summary>
        public bool Apply()
        {
            if (!_itemsDirty) return false;
            _itemsDirty = false;
            _changedItems.Clear();
            bool full = _world.TakeChanges(_changedItems) | _itemsFullRebuild;
            _itemsFullRebuild = false;
            if (!full) foreach (var id in _changedItems) { var it = _world.GetItem(id); if (it != null && it.IsRug) { full = true; break; } }
            if (full) RebuildItemViews(); else UpdateItemViews(_changedItems);
            HideOtherRegions();
            return true;
        }

        /// <summary>Bütün eşyaları hemen yeniden kurar (görsel kipi değişimi, ilk kurulum).</summary>
        public void Rebuild()
        {
            _itemsFullRebuild = false;
            _world.TakeChanges(null);
            RebuildItemViews();
        }

        /// <summary>Bölgeye bağlı olmayan nesneler (şenlik bayrakları) başka bölgedeyse gizlenir.</summary>
        public void HideOtherRegions()
        {
            if (_region == null) return;
            foreach (var go in _itemObjects)
            {
                if (go == null) continue;
                var g = Iso.WorldToGrid(go.transform.position, 0);
                go.SetActive(_region.Contains(new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y))));
            }
        }

        public void MarkAll() => MarkAllItemsDirty();

        private static Color ItemColor(string id)
        {
            switch (id)
            {
                case "item.bed_straw": return new Color(0.75f, 0.6f, 0.35f);
                case "item.hearth_stone": return new Color(0.85f, 0.35f, 0.2f);
                case "item.cooking_pot": return new Color(0.55f, 0.55f, 0.6f);
                case "item.crop_turnip": return new Color(0.45f, 0.7f, 0.35f);
                case "item.market_stall": return new Color(0.75f, 0.35f, 0.3f);
                case "item.chopping_block": return new Color(0.48f, 0.38f, 0.28f);
                case "item.table_wood": return new Color(0.62f, 0.42f, 0.24f);
                case "item.chair_wood": return new Color(0.7f, 0.5f, 0.3f);
                case "item.chest_wood": return new Color(0.5f, 0.33f, 0.2f);
                case "item.shelf_wood": return new Color(0.66f, 0.5f, 0.34f);
                case "item.candle": return new Color(1f, 0.95f, 0.7f);
                case "item.flower_pot": return new Color(0.85f, 0.4f, 0.5f);
                case "item.clay_cup": return new Color(0.8f, 0.5f, 0.35f);
                default: return new Color(0.6f, 0.45f, 0.3f);
            }
        }

        private readonly List<GameObject> _itemObjects = new List<GameObject>();

        // Eşya görünümlerini dünyadan yeniden kurar (yerleştirme/taşıma/satış sonrası). Yer tutucu küpler:
        // çok karolu eşya karo başına bir küp, dekor küçük küp olarak yuvasında durur.
        /// <summary>Bütün (etkin bölgenin) eşya görünümleri bir sonraki LateUpdate'te baştan kurulur.</summary>
        private void MarkAllItemsDirty()
        {
            _itemsFullRebuild = true;
            _itemsDirty = true;
        }

        private void RebuildItemViews()
        {
            RebuildItemViewsCore();
            // Yeni gelen eşya (satın alınan, konan, hasatla düşen) kısa bir ezilip esnemeyle belirir.
            if (_juice != null && _seenItemsReady)
                foreach (var kv in _itemViews)
                    if (!_seenItemIds.Contains(kv.Key) && kv.Value != null)
                    {
                        _juice.Pop(kv.Value);
                        _juice.Emit(Juice.Kind.Dust, kv.Value.position, 4, Iso.SortOrder(kv.Value.position.y, 2, 0));
                    }
            _seenItemIds.Clear();
            foreach (var kv in _itemViews) _seenItemIds.Add(kv.Key);
            _seenItemsReady = true;
        }

        // Şenlik hazırlığı (27 §9): yaklaşan şenlikte meydanda iki direk arasında renkli bayrak dizisi. Görsel gelene kadar elmaslar.
        private static readonly Color[] BuntingColors =
        {
            new Color(0.78f, 0.22f, 0.18f), new Color(0.9f, 0.75f, 0.25f), new Color(0.22f, 0.4f, 0.7f), new Color(0.35f, 0.6f, 0.3f)
        };

        private void AddFestiveBunting()
        {
            _buntingShown = _life != null && _life.FestivePrep != null;
            if (!_buntingShown || !_view.IsLevelVisible(0)) return;
            var sq = _ctx.SquarePlace;
            var a = Iso.GridToWorld(sq.X - 2.5f, sq.Y + 0.5f, 0) + new Vector3(0f, 1.1f, 0f);
            var b = Iso.GridToWorld(sq.X + 3.5f, sq.Y + 0.5f, 0) + new Vector3(0f, 1.1f, 0f);
            const int flags = 11;
            for (int i = 0; i <= flags; i++)
            {
                float t = i / (float)flags;
                var p = Vector3.Lerp(a, b, t) - new Vector3(0f, 0.35f * Mathf.Sin(t * Mathf.PI), 0f);   // sarkan ip
                var go = new GameObject("festive_bunting");
                go.transform.SetParent(transform, false);
                go.transform.position = p;
                go.transform.localScale = new Vector3(0.16f, i == 0 || i == flags ? 0.5f : 0.22f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteMaterials.Diamond;
                sr.sharedMaterial = SpriteMaterials.Unlit;
                sr.color = i == 0 || i == flags ? new Color(0.45f, 0.32f, 0.2f) : BuntingColors[i % BuntingColors.Length];
                sr.sortingOrder = Iso.SortOrder(p.y - 1.1f, 3, 0);
                _itemObjects.Add(go);
            }
        }

        private void RebuildItemViewsCore()
        {
            foreach (var go in _itemObjects) if (go != null) Destroy(go);
            _itemObjects.Clear();
            foreach (var bucket in _itemBuckets.Values) foreach (var go in bucket) if (go != null) Destroy(go);
            _itemBuckets.Clear();
            AddFestiveBunting();
            _itemViews.Clear();
            _plantViews.Clear();
            _heatViews.Clear();

            foreach (var item in _world.Items)
                if (InActiveRegion(item)) BuildItemView(item);
        }

        /// <summary>Eşya etkin bölgede mi (bölgesiz dünyada hep). Diğer bölgelerin eşyası hiç kurulmaz.</summary>
        private bool InActiveRegion(ItemInstance item) => _region == null || _region.Contains(item.Position);

        // Eşya başına görünüm nesneleri: yalnızca değişen eşya yıkılıp yeniden kurulur (performans incelemesi 20.11.2026).
        private readonly Dictionary<int, List<GameObject>> _itemBuckets = new Dictionary<int, List<GameObject>>();
        private List<GameObject> _currentBucket;
        private readonly List<int> _changedItems = new List<int>();
        private bool _itemsFullRebuild = true;

        private void TrackItemObject(GameObject go)
        {
            if (_currentBucket != null) _currentBucket.Add(go);
            else _itemObjects.Add(go);
        }

        private void BuildItemView(ItemInstance item)
        {
            var bucket = new List<GameObject>();
            _currentBucket = bucket;
            try { BuildItemViewBody(item); }
            finally { _currentBucket = null; }
            if (bucket.Count > 0) _itemBuckets[item.Id] = bucket;
        }

        private void RemoveItemView(int id)
        {
            if (_itemBuckets.TryGetValue(id, out var bucket))
            {
                foreach (var go in bucket) if (go != null) Destroy(go);
                _itemBuckets.Remove(id);
            }
            _itemViews.Remove(id);
            _plantViews.Remove(id);
            _heatViews.Remove(id);
        }

        /// <summary>Yalnızca değişen eşyaları yeniden kurar; yeni gelen eşya kısa bir esnemeyle belirir.</summary>
        private void UpdateItemViews(List<int> ids)
        {
            foreach (var id in ids)
            {
                RemoveItemView(id);
                var item = _world.GetItem(id);
                if (item == null) { _seenItemIds.Remove(id); continue; }
                if (!InActiveRegion(item)) continue;
                BuildItemView(item);
                if (_juice != null && _seenItemsReady && !_seenItemIds.Contains(id) && _itemViews.TryGetValue(id, out var t) && t != null)
                {
                    _juice.Pop(t);
                    _juice.Emit(Juice.Kind.Dust, t.position, 4, Iso.SortOrder(t.position.y, 2, 0));
                }
                if (_itemViews.ContainsKey(id)) _seenItemIds.Add(id);
            }
        }

        // Bir eşyanın görünüm nesneleri (eskiden RebuildItemViewsCore döngüsünün gövdesi).
        private void BuildItemViewBody(ItemInstance item)
        {
                if (!_view.IsLevelVisible(item.Position.Z)) return;   // görünmeyen katın eşyası çizilmez
                int level = item.Position.Z;
                var color = ItemColor(item.Def.Id);
                if (item.IsWallMounted)
                {
                    if (WallItemFacesAway(item) && !WallItemHidden(item)) return;   // Yukarı'da ön duvarın iç yüzü: duvarın arkasında kalır
                    if (WallItemHidden(item))
                    {
                        // Alçak duvar (Kesit'te ön duvar, Aşağı'da hepsi): eşya havada kalmasın; duvarın tepesinde küçük işaret.
                        var mark = Iso.WallMountSpan(item.Position, item.Rotation, item.WallSpan, item.WallBand)
                            - new Vector3(0f, Iso.BandHeight(item.WallBand) - LoweredWallTop, 0f);
                        var msr = AddItemSprite(item, mark, color, 0.12f * Mathf.Max(1, item.WallSpan), Iso.WallMountOrder(item.Position, item.Rotation) + 1, SpriteMaterials.Diamond);
                        msr.color = new Color(color.r, color.g, color.b, 0.75f);
                        return;
                    }
                    // Bantlı duvar (27): alt, orta, üst yükseklik; geniş eşya kenarlarının ortasında, genişliğince büyük.
                    var wallArt = IsoArt.Item(item, out _);
                    float wscale = (wallArt != null ? 1f : 0.3f) * (item.WallSpan > 1 && wallArt == null ? item.WallSpan : 1f);
                    var wsr = AddItemSprite(item, Iso.WallMountSpan(item.Position, item.Rotation, item.WallSpan, item.WallBand), color, wscale,
                        Iso.WallMountOrder(item.Position, item.Rotation) + item.WallBand, wallArt);
                    wsr.color = ConditionTint(item, wallArt != null ? Color.white : color);
                    return;
                }
                if (item.IsCeiling)
                {
                    // Duvarlar Aşağı'dayken tavan eşyası da gizlenir (alçak duvarların üstünde havada kalmasın).
                    if (_walls != null && _walls.Mode == WallMode.Down) return;
                    // Tavan ve kiriş (27): karonun ortasında, tavan yüksekliğinde; kat ya da çatı görünürken zaten çizilmez.
                    var cpos = _view.TileToWorld(item.Position) + new Vector3(0f, Iso.CeilingHeight, 0f);
                    var ceilArt = IsoArt.Item(item, out _);
                    var csr = AddItemSprite(item, cpos, color, ceilArt != null ? 1f : 0.3f, Iso.SortOrder(_view.TileToWorld(item.Position).y, 3, level), ceilArt);
                    csr.color = ConditionTint(item, ceilArt != null ? Color.white : color);
                    return;
                }
                if (item.IsDecor)
                {
                    // Kedinin yere düşürdüğü süs (belge 55, Kedi Alex): yerinde değil, yanındaki karoda yan yatmış çizilir.
                    var fallen = _life?.Scenes.KnockedOf(item.Id);
                    if (fallen != null)
                    {
                        var fpos = _view.TileToWorld(fallen.Floor);
                        var fallenArt = IsoArt.Item(item, out _);
                        var fsr = AddItemSprite(item, fpos + new Vector3(0f, 0.12f, 0f), color, fallenArt != null ? 1f : 0.3f, Iso.SortOrder(fpos.y, 0, level) + 1, fallenArt);
                        fsr.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        fsr.color = ConditionTint(item, fallenArt != null ? Color.white : color);
                        return;
                    }
                    var parent = _world.GetItem(item.ParentId.Value);
                    if (parent == null || !parent.TryGetSlotPosition(item.SlotId, out float gx, out float gy)) return;
                    if (parent.IsWallMounted)
                    {
                        if (WallItemHidden(parent) || WallItemFacesAway(parent)) return;   // gizli ya da duvar arkasındaki rafın üstündekiler
                        var onShelf = IsoArt.Item(item, out _);
                        var dsr = AddItemSprite(item, Iso.GridToWorld(gx, gy, level) + new Vector3(0f, Iso.BandHeight(parent.WallBand) + 0.1f, 0f), color,
                            onShelf != null ? 1f : 0.25f, Iso.WallMountOrder(parent.Position, parent.Rotation) + parent.WallBand + 1, onShelf);
                        dsr.color = ConditionTint(item, onShelf != null ? Color.white : color);
                        return;
                    }
                    if (parent.IsCeiling)
                    {
                        if (_walls != null && _walls.Mode == WallMode.Down) return;
                        // Kiriş kancasında asılı: kancanın biraz altında.
                        var onHook = IsoArt.Item(item, out _);
                        AddItemSprite(item, _view.TileToWorld(parent.Position) + new Vector3(0f, Iso.CeilingHeight - 0.35f, 0f), color,
                            onHook != null ? 1f : 0.25f, Iso.SortOrder(_view.TileToWorld(parent.Position).y, 4, level), onHook);
                        return;
                    }
                    // Gerçek masanın üst yüzü yer tutucu küpten biraz yüksek.
                    float topPx = IsoArt.Item(parent, out _) != null ? 27f : 24f;
                    var slotDef = parent.Def.FindSlot(item.SlotId);
                    if (slotDef != null && slotDef.IsCover)
                    {
                        // Masa ya da sedir örtüsü (27 D2): eşyanın bütün üstüne serili düz elmaslar, diğer dekorun altında.
                        var coverColor = ConditionTint(item, color);
                        // Tek karoluk yüzeyin (kütük sehpa) üstü karodan dar: örtü taşmasın.
                        float coverScale = parent.Width * parent.Height <= 1 ? 0.62f : 0.95f;
                        foreach (var t in parent.OccupiedTiles())
                        {
                            var cp = _view.TileToWorld(t) + new Vector3(0f, topPx / 32f - 0.02f, 0f);
                            var csr2 = AddItemSprite(item, cp, coverColor, coverScale, Iso.SortOrder(_view.TileToWorld(t).y, 0, level) + 1, SpriteMaterials.Diamond);
                            csr2.color = coverColor;
                        }
                        return;
                    }
                    var pos = Iso.GridToWorld(gx, gy, level) + new Vector3(0f, topPx / 32f, 0f);
                    var decorArt = IsoArt.Item(item, out _);
                    var dsr2 = AddItemSprite(item, pos, color, decorArt != null ? 1f : 0.35f, Iso.SortOrder(Iso.GridToWorld(gx, gy, level).y, 0, level) + 2, decorArt);
                    if (item.Def.IsFillDecor) dsr2.color = FillTint(item, decorArt != null ? Color.white : color);
                    return;
                }
                if (item.IsRug)
                {
                    // Halı: yere serili düz elmaslar (zeminin hemen üstünde, eşyaların altında). Halılar üst üste serilir (27):
                    // karodaki sırası çizim sırasını belirler. Döküntü yığını küçük, koyu bir leke.
                    bool debris = item.Def.Id == Thatchwick.Core.Content.Decor.Debris;
                    bool rugFlip = false;
                    var rugArt = debris ? null : IsoArt.Item(item, out rugFlip);
                    if (rugArt != null)
                    {
                        // Gerçek halı görseli (390): tek düz görsel, ayak izinin merkezinde; sırası en alttaki karoya göre.
                        var rc = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
                        int rugRank = 0;
                        foreach (var tile in item.OccupiedTiles()) rugRank = Mathf.Max(rugRank, _world.RugsAt(tile).IndexOf(item));
                        var rart = AddItemSprite(item, rc, Color.white, 1f, -9990 + Mathf.Clamp(rugRank, 0, 9) + level * Iso.LevelSortStep, rugArt);
                        rart.color = ConditionTint(item, FinishTint(item, Color.white, 0.45f));
                        rart.flipX = rugFlip;
                        AddPatternOverlay(item, rc, -9990 + Mathf.Clamp(rugRank, 0, 9) + level * Iso.LevelSortStep);   // işlenmiş desen (belge 45)
                        return;
                    }
                    var rugColor = debris ? new Color(0.35f, 0.27f, 0.2f) : ConditionTint(item, FinishTint(item, ItemColor(item.Def.Id), 1f));
                    foreach (var tile in item.OccupiedTiles())
                    {
                        int rank = _world.RugsAt(tile).IndexOf(item);
                        var rsr = AddItemSprite(item, _view.TileToWorld(tile), rugColor, debris ? 0.55f : 0.92f,
                            -9990 + Mathf.Clamp(rank, 0, 9) + level * Iso.LevelSortStep, SpriteMaterials.Diamond);
                        rsr.color = rugColor;
                    }
                    return;
                }
                if (GreatTrees.IsGreatTree(item.Def)) { AddGreatTree(item, level); return; }   // belge 52
                bool flip = false;
                var art = item.IsPlant ? null : IsoArt.Item(item, out flip);
                if (art != null)
                {
                    // Tek görsel, ayak izinin merkezinde (pivot = taban elmasının merkezi).
                    var center = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
                    var sr = AddItemSprite(item, center, Color.white, 1f, Iso.SortOrder(center.y, 0, level), art);
                    AddItemShadow(item, center, level);
                    sr.color = ConditionTint(item, FinishTint(item, Color.white, 0.45f));   // kaplama: gerçek görsel gelene kadar hafif renk
                    sr.flipX = flip;
                    AddAccentOverlay(item, sr, center, level);                         // ikinci renk: korkuluk, minder, kulp (belge 45)
                    if (item.Def.HeatOutput > 0f) _heatViews[item.Id] = (item, sr);
                    return;
                }
                if (item.IsPlant)
                {
                    var pos = _view.TileToWorld(item.Position);
                    var plantArt = IsoArt.Plant(item, out bool staged);
                    var psr = AddItemSprite(item, pos, plantArt != null ? Color.white : color, ViewScale(item, staged), Iso.SortOrder(pos.y, 1, level), plantArt ?? SpriteMaterials.Tree);
                    var sway = SpriteMaterials.Sway;   // rüzgârda sallanma (vertex shader; yoksa düz)
                    if (sway != null) psr.sharedMaterial = sway;
                    // Ağaç gölgesi: ışık sol üstten, gölge taç boyunca sağ alta uzanır; büyüdükçe büyür.
                    var plantShadow = AddShadowObject(pos + new Vector3(0.18f, -0.06f, 0f) * PlantScale(item), 0.95f * PlantScale(item), level);
                    // Büyüme ölçeği bitkinin kendisine uygulanır (eskiden yanlışlıkla gölge nesnesine uygulanıyordu; tam yeniden kurulum
                    // sık olduğu için fark edilmiyordu).
                    _plantViews[item.Id] = new PlantView { Item = item, View = psr.transform, Sr = psr, Shadow = plantShadow };
                    return;
                }
                var tinted = FinishTint(item, color, 1f);
                AddItemShadow(item, Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level), level);
                foreach (var tile in item.OccupiedTiles())
                {
                    var pos = _view.TileToWorld(tile);
                    AddItemSprite(item, pos, tinted, 1f, Iso.SortOrder(pos.y, 0, level));
                }
        }

        // ---- Ulu ağaçlar ve ağaç evler (belge 52; GameRootTrees'ten taşındı) ----
        // Görsel ölçüleri (art/queue 492-493): 2x2 ağaç tuvali 192x256, pivot (96, 40); 3x3 çınar 256x320, pivot (128, 56).
        private const float GreatTreePivotX2 = 96f, GreatTreePivotY2 = 40f, GreatTreePivotX3 = 128f, GreatTreePivotY3 = 56f;

        private static string SeasonKey(Season s) => s switch
        {
            Season.Spring => "spring", Season.Summer => "summer", Season.Autumn => "autumn", _ => "winter"
        };

        private static string Species(ItemDef def) => def.Id.StartsWith("item.great_") ? def.Id.Substring("item.great_".Length) : "oak";

        private void AddGreatTree(ItemInstance item, int level)
        {
            bool big = item.Width >= 3;
            float px = big ? GreatTreePivotX3 : GreatTreePivotX2, py = big ? GreatTreePivotY3 : GreatTreePivotY2;
            var center = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
            int order = Iso.SortOrder(center.y, 1, level);
            string species = Species(item.Def);
            var art = IsoArt.Get($"iso_greattree_{species}_{SeasonKey(_clock.Season)}_se", px, py)
                      ?? IsoArt.Get($"iso_greattree_{species}_summer_se", px, py);
            SpriteRenderer tree;
            if (art != null) tree = AddItemSprite(item, center, Color.white, 1f, order, art);
            else
            {
                // Yer tutucu: büyük, koyu yeşil ağaç (eski ağaç kaplamasıyla), mevsime göre renk.
                var tint = _clock.Season == Season.Autumn ? new Color(0.55f, 0.42f, 0.18f) : _clock.Season == Season.Winter ? new Color(0.45f, 0.42f, 0.38f) : new Color(0.24f, 0.42f, 0.2f);
                tree = AddItemSprite(item, center, tint, big ? 2.6f : 2.1f, order, SpriteMaterials.Tree);
                tree.color = tint;
            }
            var sway = SpriteMaterials.Sway;
            if (sway != null && art != null) tree.sharedMaterial = sway;
            AddShadowObject(center + new Vector3(0.35f, -0.1f, 0f), big ? 2.6f : 2.0f, level);

            var site = _life?.Trees?.SiteOf(item);
            if (site == null || !site.HasHouse) return;
            string key = TreehouseArtKey(site, GreatTrees.IsInn(item.Def));
            var over = IsoArt.Get(key, px, py);
            if (over != null) AddItemSprite(item, center, Color.white, 1f, order + 1, over);
            else
            {
                // Yer tutucu: tacın içinde kahverengi bir elmas (yapımda soluk, harapta gri).
                var c = !site.IsBuilt ? new Color(0.62f, 0.5f, 0.34f, 0.7f) : site.IsRuined ? new Color(0.45f, 0.43f, 0.4f) : new Color(0.5f, 0.33f, 0.18f);
                var sr = AddItemSprite(item, center + new Vector3(0f, big ? 2.2f : 1.7f, 0f), c, 0.5f + 0.08f * (int)site.Kind, order + 1, SpriteMaterials.Diamond);
                sr.color = c;
            }
            if (_clock.Season == Season.Winter && site.IsBuilt)
            {
                var snow = IsoArt.Get(big ? "iso_treehouse_snow_big_se" : "iso_treehouse_snow_se", px, py);
                if (snow != null) AddItemSprite(item, center, Color.white, 1f, order + 2, snow);
            }
        }

        /// <summary>Ağaç ev katmanının görsel adı: yapım evresi (1-3), harap, tür; kasaba çınarında aşevi.</summary>
        private static string TreehouseArtKey(TreehouseSite site, bool inn)
        {
            if (inn) return "iso_treehouse_inn_se";
            if (!site.IsBuilt)
            {
                int stage = site.Needed <= 0 ? 1 : 1 + Mathf.Clamp((int)(3 * site.Progress / site.Needed), 0, 2);
                return $"iso_treehouse_site_{stage}_se";
            }
            string kind = TreehouseSystem.KindKey(site.Kind);
            return site.IsRuined ? $"iso_treehouse_{kind}_ruin_se" : $"iso_treehouse_{kind}_se";
        }

        // Hasarlı eşya koyu ve soluk, onarılmış hafif soluk, kararmış ayna gri (27-ev-dekorasyonu).
        /// <summary>Kesit modunda alçak duvarın tepesi (dünya birimi; SpriteMaterials.LowWallHeightPx / 32).</summary>
        private const float LoweredWallTop = 0.34f;

        // Duvar eşyası alçak duvarda mı (gizlenir): geniş eşyanın herhangi bir kenarı alçaksa hepsi.
        private bool WallItemHidden(ItemInstance item)
        {
            if (_walls == null || !item.IsWallMounted) return false;
            foreach (var seg in item.WallSegments()) if (_walls.IsLowered(seg.edge)) return true;
            return false;
        }

        // Duvar eşyası kameraya bakmayan yüzde mi (ön duvarın oda içi yüzü): Yukarı modunda duvarın arkasında kalır, çizilmez.
        // Kameraya bakan yüz kenarın A yüzüdür (WallView); eşya karosu B yüzündeyse arkaya bakar.
        private static bool WallItemFacesAway(ItemInstance item)
        {
            if (!item.IsWallMounted) return false;
            foreach (var seg in item.WallSegments()) if (seg.tile == seg.edge.SideB) return true;
            return false;
        }

        // Doldurulan dekor (27 D2): boşken soluk, bayatta kahverengimsi, çürükte yeşilimsi (görsel gelene kadar).
        private Color FillTint(ItemInstance item, Color baseColor)
        {
            var c = ConditionTint(item, baseColor);
            if (!item.HasContents) return Color.Lerp(c, new Color(0.55f, 0.5f, 0.45f), 0.45f);
            int f = Thatchwick.Core.Build.DecorSystem.WorstFreshness(item, _content);
            if (f == Thatchwick.Core.Content.Decor.Rotten) return Color.Lerp(c, new Color(0.35f, 0.45f, 0.2f), 0.6f);
            if (f == Thatchwick.Core.Content.Decor.Stale) return Color.Lerp(c, new Color(0.55f, 0.42f, 0.28f), 0.3f);
            return c;
        }

        private Color ConditionTint(ItemInstance item, Color baseColor)
        {
            var c = baseColor;
            if (item.Condition == Thatchwick.Core.Content.ItemCondition.Damaged) c = Color.Lerp(c, new Color(0.35f, 0.3f, 0.28f), 0.55f);
            else if (item.Condition == Thatchwick.Core.Content.ItemCondition.Repaired) c = Color.Lerp(c, new Color(0.7f, 0.65f, 0.6f), 0.2f);
            if (item.IsTarnished) c = Color.Lerp(c, new Color(0.4f, 0.42f, 0.38f), 0.5f);
            if (_clock != null && Thatchwick.Core.Build.DecorSystem.IsWilted(item, _clock.Season))
                c = Color.Lerp(c, new Color(0.5f, 0.38f, 0.22f), 0.55f);    // solmuş mevsim süsü (D3b): kurumuş kahverengi
            var home = _life?.Home;
            if (home != null)
            {
                if (home.PotWilted(item)) c = Color.Lerp(c, new Color(0.55f, 0.45f, 0.25f), 0.5f);          // solmuş saksı
                if (item.BurntOut) c = Color.Lerp(c, new Color(0.45f, 0.42f, 0.4f), 0.35f);                // bitmiş mum
                if (home.IsStrewnStale(item)) c = Color.Lerp(c, new Color(0.55f, 0.5f, 0.38f), 0.3f);       // bayat ot serpmesi
                else if (home.IsStrewnFresh(item)) c = Color.Lerp(c, new Color(0.75f, 0.9f, 0.6f), 0.2f);  // taze ot
                if (item.Def.IsVivarium)   // cam bahçe dünyada zeminine göre renk alır (içi yakın görünümde, belge 47)
                {
                    var viv = home.VivOf(item);
                    if (viv?.Habitat != null)
                    {
                        var hc = viv.Habitat == HomeLife.Salt ? new Color(0.45f, 0.65f, 0.75f) : viv.Habitat == HomeLife.Fresh ? new Color(0.5f, 0.7f, 0.55f)
                               : viv.Habitat == HomeLife.Dry ? new Color(0.9f, 0.8f, 0.6f) : new Color(0.45f, 0.65f, 0.4f);
                        c = Color.Lerp(c, hc, viv.Entries.Count > 0 ? 0.3f : 0.15f);
                    }
                }
            }
            return c;
        }

        // İkinci renk (belge 45 §10): eşyanın "_accent" maske görseli varsa ikinci kaplamanın rengiyle üstüne çizilir.
        // Maske yoksa (görsel işi bekliyor) ikinci renk yalnızca kartta görünür.
        private void AddAccentOverlay(ItemInstance item, SpriteRenderer body, Vector3 center, int level)
        {
            if (string.IsNullOrEmpty(item.Def.AccentGroup)) return;
            var mask = IsoArt.ItemAccent(item);
            if (mask == null) return;
            BuildPartDef part = null;
            if (item.AccentId != null) _content.Parts.TryGet(item.AccentId, out part);
            else foreach (var p in BuildParts.AccentsFor(_content, item.Def)) if (p.IsDefault) { part = p; break; }
            if (part == null) return;
            var asr = AddItemSprite(item, center, Color.white, 1f, body.sortingOrder + 1, mask);
            asr.color = SpriteMaterials.PartColor(part, Color.white);
            asr.flipX = body.flipX;
        }

        // İşlenmiş desen (belge 45 §8): halının üstüne 8x8 desen, yere serili elmas olarak (yarı saydam).
        private readonly System.Collections.Generic.Dictionary<string, Sprite> _patternSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
        private void AddPatternOverlay(ItemInstance item, Vector3 center, int order)
        {
            var pat = _life?.Home?.PatternOn(item);
            if (pat == null) return;
            if (!_patternSprites.TryGetValue(pat.Cells, out var sprite) || sprite == null)
            {
                var tex = new Texture2D(8, 8) { filterMode = FilterMode.Point };
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                    {
                        int idx = Mathf.Clamp(pat.Cells[(7 - y) * 8 + x] - '0', 0, HomeLife.Palette.Length - 1);
                        var col = HomePanel.Hex(HomeLife.Palette[idx].Hex);
                        col.a = 0.8f;
                        tex.SetPixel(x, y, col);
                    }
                tex.Apply();
                sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
                _patternSprites[pat.Cells] = sprite;
            }
            // Kare deseni izometrik elmasa çevir: 45° döndür, dikeyde yarıya bastır (üst nesne ölçeği).
            var holder = new GameObject(item.Def.Id + "_desen");
            holder.transform.SetParent(transform, false);
            holder.transform.position = center + new Vector3(0f, 0f, -0.001f);   // halının hemen önünde
            float span = Mathf.Min(item.Width, item.Height) * 0.62f;
            holder.transform.localScale = new Vector3(1f, 0.5f, 1f);
            var child = new GameObject("desen");
            child.transform.SetParent(holder.transform, false);
            child.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            child.transform.localScale = new Vector3(span * 1.41f, span * 1.41f, 1f);
            var sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.sortingOrder = order;
            TrackItemObject(holder);
        }

        // Mobilya kaplamasının rengi (amount: 1 = tam kaplama rengi, küçük = beyaza karışık); kaplama yoksa baseColor.
        private Color FinishTint(ItemInstance item, Color baseColor, float amount)
        {
            if (item.FinishId == null || !_content.Parts.TryGet(item.FinishId, out var part)) return baseColor;
            var c = SpriteMaterials.PartColor(part, baseColor);
            return amount >= 1f ? c : Color.Lerp(Color.white, c, amount);
        }

        internal static float PlantScale(ItemInstance item) => 0.3f + 0.7f * item.GrowthFraction;

        /// <summary>
        /// Görünüm ölçeği: evre görseli varsa (genç ağaç, ekin evresi) küçültme yalnızca evre içinde hafif;
        /// olgun görsel tek başınaysa büyüme oranıyla küçülür.
        /// </summary>
        internal static float ViewScale(ItemInstance item, bool staged)
        {
            if (!staged) return PlantScale(item);
            if (item.Def.IsCrop || item.IsGrown) return 1f;
            return 0.75f + 0.25f * item.GrowthFraction;   // genç ağaç görseli fidandan ağaca büyür
        }

        // ---------------- gölgeler (görsel analiz, 18.11.2026) ----------------

        /// <summary>Gölgenin saydamlığı. Gölgeler zeminin ve halıların üstünde, eşyaların altında çizilir.</summary>
        private const float ShadowAlpha = Shadows.Alpha;
        private static int ShadowOrder(int level) => Shadows.Order(level);
        private static SpriteRenderer NewShadow(Transform parent, Vector3 local, float size) => Shadows.Attach(parent, local, size);

        /// <summary>Eşyanın ayak izi kadar gölge (halılar ayrı çizilir, gölgesizdir).</summary>
        private void AddItemShadow(ItemInstance item, Vector3 footprintCenter, int level)
        {
            float size = (item.Width + item.Height) * 0.5f * 0.92f;
            AddShadowObject(footprintCenter + new Vector3(0.08f, -0.04f, 0f), size, level);
        }

        private Transform AddShadowObject(Vector3 pos, float size, int level)
        {
            var go = new GameObject("Shadow");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Shadow;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = new Color(0f, 0f, 0f, ShadowAlpha);
            sr.sortingOrder = ShadowOrder(level);
            TrackItemObject(go);
            return go.transform;
        }

        private SpriteRenderer AddItemSprite(ItemInstance item, Vector3 pos, Color color, float scale, int order, Sprite sprite = null)
        {
            var go = new GameObject(item.Def.Id);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : SpriteMaterials.Cube(color);
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.sortingOrder = order;
            TrackItemObject(go);
            if (!_itemViews.ContainsKey(item.Id)) _itemViews[item.Id] = go.transform;
            return sr;
        }
    }
}
