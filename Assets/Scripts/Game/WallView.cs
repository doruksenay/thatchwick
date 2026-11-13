using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    public enum WallMode { Up, Cutaway, Down }

    /// <summary>
    /// Karo kenarlarındaki duvar/kapı/pencereleri yer tutucu sprite'larla çizer. Üç mod (Sims gibi):
    /// Yukarı (hepsi tam), Kesit (içeriyi kapatan ön duvarlar alçak), Aşağı (hepsi alçak).
    /// Kesit kuralı: arkadaki karo döşeli, öndeki değilse duvar "ön duvardır" ve alçak çizilir.
    /// </summary>
    public sealed class WallView : MonoBehaviour
    {
        private GridMap _map;
        private WorldView _view;
        private WallMode _mode = WallMode.Cutaway;
        private readonly List<GameObject> _objects = new List<GameObject>();

        public WallMode Mode
        {
            get => _mode;
            set { _mode = value; Rebuild(); }
        }

        public void Init(GridMap map, WorldView view = null)
        {
            _map = map;
            _view = view;
            map.Changed += MarkDirty;
            if (view != null) view.LevelChanged += Rebuild;
            Rebuild();
        }

        // Harita aynı karede defalarca değişebilir (oda aracı onlarca duvar kurar): duvarlar karede bir kez yeniden kurulur.
        private bool _dirty;
        private void MarkDirty() { _dirty = true; }

        private void LateUpdate()
        {
            if (_dirty) Rebuild();
        }

        private void OnDestroy()
        {
            if (_map != null) _map.Changed -= MarkDirty;
            if (_view != null) _view.LevelChanged -= Rebuild;
        }

        public void Rebuild()
        {
            _dirty = false;
            foreach (var go in _objects) if (go != null) Destroy(go);
            _objects.Clear();
            if (_map == null) return;

            foreach (var kv in _map.Edges)
            {
                var e = kv.Key;
                if (_view != null && !_view.IsLevelVisible(e.Z)) continue;
                bool low = _mode == WallMode.Down || (_mode == WallMode.Cutaway && IsFrontWall(e));
                var go = new GameObject($"Wall {e}");
                go.transform.SetParent(transform, false);
                var start = Iso.GridToWorld(e.X, e.Y, e.Z);
                go.transform.position = start;
                var sr = go.AddComponent<SpriteRenderer>();
                // Gerçek duvar görseli varsa o; alçak (kesit) duvar için görsel gelene kadar yarı saydam tam duvar.
                var real = IsoArt.Wall(e.Axis, kv.Value);
                if (real != null)
                {
                    sr.sprite = real;
                    float alpha = low ? 0.3f : 1f;
                    sr.color = kv.Value == EdgeKind.Window ? new Color(0.75f, 0.85f, 1f, alpha) : new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    sr.sprite = SpriteMaterials.Wall(e.Axis, kv.Value, low);
                    sr.color = kv.Value == EdgeKind.Window ? new Color(0.85f, 0.92f, 1f) : new Color(0.78f, 0.68f, 0.52f);
                }
                sr.sharedMaterial = SpriteMaterials.Unlit;
                // Kenar orta noktasının derinliği; karakterin (karo merkezi) arkasında kalması için küçük sapma.
                var mid = Iso.GridToWorld(e.X + (e.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), e.Y + (e.Axis == EdgeAxis.Vertical ? 0.5f : 0f), e.Z);
                sr.sortingOrder = Iso.SortOrder(mid.y, 0, e.Z);
                _objects.Add(go);
            }
        }

        private bool IsFrontWall(Edge e)
        {
            bool behindFloor = _map.InBounds(e.SideB) && _map.GetFloor(e.SideB) != FloorType.Grass;
            bool frontFloor = _map.InBounds(e.SideA) && _map.GetFloor(e.SideA) != FloorType.Grass;
            return behindFloor && !frontFloor;
        }
    }
}
