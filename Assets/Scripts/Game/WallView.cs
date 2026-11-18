using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    public enum WallMode { Up, Cutaway, Down }

    /// <summary>
    /// Karo kenarlarındaki duvar/kapı/pencere/çit/korkulukları ve otomatik çatıyı yer tutucu sprite'larla çizer.
    /// Üç duvar modu (Sims gibi): Yukarı (hepsi tam), Kesit (içeriyi kapatan ön duvarlar alçak), Aşağı (hepsi alçak).
    /// Kesit kuralı: arkadaki karo döşeli, öndeki değilse duvar "ön duvardır" ve alçak çizilir.
    /// Renk: kenarın malzemesi/stili, duvarda kameraya bakan yüzün (SideA) kaplaması. Çatı yalnızca duvarlar Yukarı'dayken
    /// ve ShowRoof açıkken çizilir (içeri bakarken gizlenir).
    /// </summary>
    public sealed class WallView : MonoBehaviour
    {
        private GridMap _map;
        private WorldView _view;
        private ContentDatabase _content;
        private WallMode _mode = WallMode.Cutaway;
        private bool _showRoof = true;
        private readonly List<GameObject> _objects = new List<GameObject>();

        private static readonly Color DefaultWall = new Color(0.78f, 0.68f, 0.52f);
        private static readonly Color DefaultWindow = new Color(0.85f, 0.92f, 1f);

        public WallMode Mode
        {
            get => _mode;
            set { _mode = value; Rebuild(); ModeChanged?.Invoke(); }
        }

        /// <summary>Duvar modu değişti (duvar eşyaları alçak duvarda gizlenir; eşyalar yeniden çizilmeli).</summary>
        public event System.Action ModeChanged;

        /// <summary>
        /// Kenar şu an alçak mı çiziliyor (Aşağı'da bütün duvarlar, Kesit'te ön duvarlar). Alçak duvardaki asılı eşyalar
        /// havada kalmasın diye gizlenir, yerinde yalnızca küçük bir işaret kalır (Sims davranışı, 27 §4.4).
        /// </summary>
        public bool IsLowered(Edge e)
        {
            if (_map == null || !_map.EdgeInBounds(e)) return false;
            var kind = _map.GetEdge(e);
            bool fullHeight = EdgeKinds.IsRoomBoundary(kind) || kind == EdgeKind.HalfWall;
            return fullHeight && (_mode == WallMode.Down || (_mode == WallMode.Cutaway && IsFrontWall(e)));
        }

        /// <summary>Çatı görünsün mü (duvarlar Yukarı modundayken).</summary>
        public bool ShowRoof
        {
            get => _showRoof;
            set { _showRoof = value; Rebuild(); }
        }

        public void Init(GridMap map, WorldView view = null, ContentDatabase content = null)
        {
            _map = map;
            _view = view;
            _content = content;
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
                var kind = kv.Value;
                if (_view != null && !_view.IsLevelVisible(e.Z)) continue;
                bool fullHeight = EdgeKinds.IsRoomBoundary(kind) || kind == EdgeKind.HalfWall;
                bool low = fullHeight && (_mode == WallMode.Down || (_mode == WallMode.Cutaway && IsFrontWall(e)));
                var go = new GameObject($"Wall {e}");
                go.transform.SetParent(transform, false);
                go.transform.position = Iso.GridToWorld(e.X, e.Y, e.Z);
                var sr = go.AddComponent<SpriteRenderer>();
                var look = _map.GetLook(e);
                // Kameraya bakan yüz A yüzüdür (önde). Duvarın o yüzü kaplıysa kaplama rengi görünür.
                string finish = EdgeKinds.IsRoomBoundary(kind) ? look?.FinishA : null;
                bool plain = look?.Style == null && finish == null;
                // Gerçek görsel: önce parçanın kendi görseli (iso_wall_<parça>), varsayılan malzemede eski duvar seti; kaplama
                // gerçek görselin üstüne hafif renk olarak biner. Yoksa yer tutucu renkli parça.
                var part = _content != null && Thatchwick.Core.Build.BuildService.PartKindFor(kind) is BuildPartKind pk ? BuildParts.Resolve(_content, pk, look?.Style) : null;
                string variant = kind == EdgeKind.HalfWall ? (low ? "low" : "half") : low ? "low" : null;
                // Kaplamanın kendi dokusu varsa (lambri, duvar dokuması) o çizilir; yoksa malzeme görseli + kaplama rengi.
                var finishArt = finish != null ? IsoArt.WallPart(finish, e.Axis, variant) : null;
                var real = finishArt ?? IsoArt.WallPart(part?.Id, e.Axis, variant);
                if (real == null && look?.Style == null && (kind == EdgeKind.Wall || kind == EdgeKind.Door || kind == EdgeKind.Window)) real = IsoArt.Wall(e.Axis, kind);
                if (real != null)
                {
                    sr.sprite = real;
                    float alpha = low ? 0.3f : 1f;
                    var tint = finish != null && finishArt == null ? Color.Lerp(Color.white, EdgeColor(kind, look?.Style, finish), 0.6f) : Color.white;
                    sr.color = kind == EdgeKind.Window && plain ? new Color(0.75f, 0.85f, 1f, alpha) : new Color(tint.r, tint.g, tint.b, alpha);
                }
                else
                {
                    sr.sprite = SpriteMaterials.Wall(e.Axis, kind, low);
                    sr.color = EdgeColor(kind, look?.Style, finish);
                }
                sr.sharedMaterial = SpriteMaterials.Unlit;
                // Kenar orta noktasının derinliği; karakterin (karo merkezi) arkasında kalması için küçük sapma.
                var mid = Iso.GridToWorld(e.X + (e.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), e.Y + (e.Axis == EdgeAxis.Vertical ? 0.5f : 0f), e.Z);
                sr.sortingOrder = Iso.SortOrder(mid.y, 0, e.Z);
                _objects.Add(go);
            }

            if (_showRoof && _mode == WallMode.Up) DrawRoofs();
        }

        private Color EdgeColor(EdgeKind kind, string style, string finish)
        {
            Color fallback = kind == EdgeKind.Window ? DefaultWindow : DefaultWall;
            if (_content == null) return fallback;
            if (finish != null && _content.Parts.TryGet(finish, out var f)) return SpriteMaterials.PartColor(f, fallback);
            var pk = Thatchwick.Core.Build.BuildService.PartKindFor(kind);
            var part = pk.HasValue ? BuildParts.Resolve(_content, pk.Value, style) : null;
            return SpriteMaterials.PartColor(part, fallback);
        }

        // Çatı karoları: duvar tepesinden başlayıp sırta doğru yükselen elmaslar; eğim yönüne göre gölgelenir
        // (ışık sol üstten). Gerçek çatı görseli gelene kadar yer tutucu.
        private const float RoofBase = SpriteMaterials.WallHeightPx / 32f;
        private const float RoofRise = 0.45f;

        private void DrawRoofs()
        {
            foreach (var region in RoofBuilder.Compute(_map))
            {
                if (_view != null && !_view.IsLevelVisible(region.Level)) continue;
                var part = _content != null ? BuildParts.Resolve(_content, BuildPartKind.Roof, region.Style) : null;
                var baseColor = SpriteMaterials.PartColor(part, new Color(0.72f, 0.6f, 0.34f));
                foreach (var t in region.Tiles)
                {
                    if (_map.GetRoofStyle(t.Pos) == GridMap.NoRoof) continue;
                    var ground = Iso.GridToWorld(t.Pos.X + 0.5f, t.Pos.Y + 0.5f, t.Pos.Z);
                    var go = new GameObject($"Roof {t.Pos}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = ground + new Vector3(0f, RoofBase + (t.Height - 0.5f) * RoofRise, 0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    var roofArt = IsoArt.RoofPart(part?.Id, t.IsRidge);
                    sr.sprite = roofArt != null ? roofArt : SpriteMaterials.Diamond;
                    sr.sharedMaterial = SpriteMaterials.Unlit;
                    var tileColor = roofArt != null ? Color.white : baseColor;   // gerçek dokuda yalnızca eğim gölgesi
                    float shade = t.IsRidge ? 1.08f
                        : t.Down.Y < 0 ? 1.0f      // ekranda sağ-aşağı bakan yüz: aydınlık
                        : t.Down.X < 0 ? 0.9f      // sol-aşağı
                        : t.Down.Y > 0 ? 0.72f     // arkaya bakan yüz
                        : 0.8f;
                    sr.color = new Color(Mathf.Clamp01(tileColor.r * shade), Mathf.Clamp01(tileColor.g * shade), Mathf.Clamp01(tileColor.b * shade), 1f);
                    // Evin içindekileri örtsün ama önünden geçeni değil: kendi karosunun sırasına duvar boyu kadar pay.
                    sr.sortingOrder = Iso.SortOrder(ground.y, 60 + t.Height, t.Pos.Z);
                    _objects.Add(go);
                }
            }
        }

        private bool IsFrontWall(Edge e)
        {
            // Arkası kapalı bir oda, önü oda değilse ön duvardır. (Eskiden yalnızca zemine bakılıyordu: önü su, toprak
            // ya da taş olan dış duvar kesitte alçalmıyordu.) Oda bilgisi yoksa eski zemin kuralı.
            var rooms = _map.Rooms;
            if (rooms != null)
            {
                var back = _map.InBounds(e.SideB) ? rooms.RoomAt(e.SideB) : null;
                var front = _map.InBounds(e.SideA) ? rooms.RoomAt(e.SideA) : null;
                if (back != null && back.IsIndoor) return front == null || !front.IsIndoor;
            }
            bool behindFloor = _map.InBounds(e.SideB) && _map.GetFloor(e.SideB) != FloorType.Grass;
            bool frontFloor = _map.InBounds(e.SideA) && _map.GetFloor(e.SideA) != FloorType.Grass;
            return behindFloor && !frontFloor;
        }
    }
}
