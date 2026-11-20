using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Thatchwick.Game
{
    /// <summary>Bir zemin karosunun çizim bilgisi (WorldView boyar, GroundLayer çizer).</summary>
    public sealed class TileCell
    {
        public Sprite sprite;
        public Color color = Color.white;
        public bool flipX;
        public bool enabled = true;

        public TileCell Reset()
        {
            sprite = null;
            color = Color.white;
            flipX = false;
            enabled = true;
            return this;
        }
    }

    /// <summary>
    /// Bir katın zemin karoları (performans incelemesi 20.11.2026). Eskiden her karo ayrı bir GameObject ve SpriteRenderer'dı
    /// (çiftlik bölgesi için 96×72×4 ≈ 27.600 nesne). Artık kat başına tek Tilemap: karolar toplu (SetTiles) yazılır,
    /// yalnızca değişen karo gönderilir ve TilemapRenderer parça (chunk) kipinde toplu çizer.
    /// <see cref="UseTilemap"/> kapatılırsa eski SpriteRenderer yolu çalışır (görsel karşılaştırma ve geri dönüş için).
    /// </summary>
    public sealed class GroundLayer
    {
        /// <summary>Zemin Tilemap ile mi çizilsin (varsayılan). Kapalıyken karo başına SpriteRenderer (eski yol).</summary>
        public static bool UseTilemap = true;

        private static readonly Dictionary<Sprite, Tile> TileCache = new Dictionary<Sprite, Tile>();

        private readonly Transform _root;
        private readonly Tilemap _tilemap;
        private readonly TilemapRenderer _renderer;
        private readonly int _sortingOrder;
        private RectInt _area;

        // Son gönderilen durum (yalnızca değişen karo yazılır).
        private Sprite[] _sprites;
        private Color[] _colors;
        private bool[] _flips;
        private bool[] _shown;
        private SpriteRenderer[] _legacy;
        private readonly List<TileChangeData> _pending = new List<TileChangeData>();
        private bool _visible = true;

        public GroundLayer(Transform parent, string name, Vector3 offset, int sortingOrder, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = offset;
            _root = go.transform;
            _sortingOrder = sortingOrder;
            if (!UseTilemap) return;

            // Izgara: 2:1 izometrik, karo 2x1 dünya birimi (Iso.GridToWorld ile aynı izdüşüm; hücre merkezi = TileToWorld).
            var grid = go.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(Iso.HalfWidth * 2f, Iso.HalfHeight * 2f, 1f);
            var tmGo = new GameObject("Tilemap");
            tmGo.transform.SetParent(go.transform, false);
            _tilemap = tmGo.AddComponent<Tilemap>();
            _tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            _renderer = tmGo.AddComponent<TilemapRenderer>();
            _renderer.mode = TilemapRenderer.Mode.Chunk;
            _renderer.sortingOrder = sortingOrder;
            if (material != null) _renderer.sharedMaterial = material;
        }

        /// <summary>Çizilen alanı kurar (eski karolar silinir).</summary>
        public void SetArea(RectInt area)
        {
            _area = area;
            int n = Mathf.Max(0, area.width * area.height);
            _sprites = new Sprite[n];
            _colors = new Color[n];
            _flips = new bool[n];
            _shown = new bool[n];
            _pending.Clear();
            if (_tilemap != null) _tilemap.ClearAllTiles();
            if (_legacy != null) foreach (var sr in _legacy) if (sr != null) Object.Destroy(sr.gameObject);
            _legacy = UseTilemap ? null : new SpriteRenderer[n];
        }

        /// <summary>Bütün katı gizler ya da gösterir (görünmeyen katın karoları tek tek gezilmez).</summary>
        public void SetVisible(bool visible)
        {
            if (_visible == visible) return;
            _visible = visible;
            _root.gameObject.SetActive(visible);
        }

        public bool Contains(int x, int y) => x >= _area.xMin && y >= _area.yMin && x < _area.xMax && y < _area.yMax;

        /// <summary>Karoyu yazar (değişmediyse hiçbir şey yapılmaz). Toplu gönderim için sonra <see cref="Flush"/>.</summary>
        public void Set(int x, int y, TileCell cell)
        {
            if (!Contains(x, y)) return;
            int i = (y - _area.yMin) * _area.width + (x - _area.xMin);
            bool show = cell.enabled && cell.sprite != null;
            if (_shown[i] == show && (!show || (_sprites[i] == cell.sprite && _flips[i] == cell.flipX && Same(_colors[i], cell.color)))) return;
            _shown[i] = show;
            _sprites[i] = show ? cell.sprite : null;
            _colors[i] = cell.color;
            _flips[i] = cell.flipX;

            if (_legacy != null) { SetLegacy(i, x, y, show, cell); return; }
            _pending.Add(new TileChangeData
            {
                position = new Vector3Int(x, y, 0),
                tile = show ? TileFor(cell.sprite) : null,
                color = cell.color,
                transform = cell.flipX ? Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)) : Matrix4x4.identity
            });
        }

        /// <summary>Bekleyen karoları tek çağrıda Tilemap'e gönderir.</summary>
        public void Flush()
        {
            if (_tilemap == null || _pending.Count == 0) return;
            _tilemap.SetTiles(_pending.ToArray(), true);
            _pending.Clear();
        }

        public void Destroy()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
        }

        private static bool Same(Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

        private static Tile TileFor(Sprite sprite)
        {
            if (TileCache.TryGetValue(sprite, out var tile) && tile != null) return tile;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.flags = TileFlags.None;   // renk ve dönüşüm karo başına yazılabilsin
            tile.colliderType = Tile.ColliderType.None;
            TileCache[sprite] = tile;
            return tile;
        }

        /// <summary>Play'den çıkınca sprite'lar yok edilir; önbellekteki karolar da bırakılır.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetCache()
        {
            foreach (var t in TileCache.Values) if (t != null) Object.Destroy(t);
            TileCache.Clear();
        }

        private void SetLegacy(int i, int x, int y, bool show, TileCell cell)
        {
            var sr = _legacy[i];
            if (sr == null)
            {
                if (!show) return;
                var go = new GameObject($"Tile {x},{y}");
                go.transform.SetParent(_root, false);
                go.transform.localPosition = Iso.GridToWorld(x + 0.5f, y + 0.5f);
                sr = go.AddComponent<SpriteRenderer>();
                sr.sharedMaterial = SpriteMaterials.Unlit;
                sr.sortingOrder = _sortingOrder;
                _legacy[i] = sr;
            }
            sr.enabled = show;
            if (!show) return;
            sr.sprite = cell.sprite;
            sr.color = cell.color;
            sr.flipX = cell.flipX;
        }
    }
}
