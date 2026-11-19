using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Yerleşim debug katmanı (yalnızca editör ve geliştirme derlemesi, F3 ile aç/kapa). Etkin bölgenin görünen katında:
    /// karo ızgarası, çarpışma ayak izleri, eşya yön okları, kapı önü hücreleri, etkileşim noktaları, yürünebilir ve
    /// engelli karolar, ulaşılamayan karolar ve kural ihlalleri (kırmızı). Doğrulayıcı Core'dadır (PlacementValidator);
    /// bu sınıf yalnızca çizer. Dünya değişince yeniden kurulur.
    /// </summary>
    public sealed class PlacementDebugOverlay
    {
        public static bool Allowed => Application.isEditor || Debug.isDebugBuild;

        private readonly Transform _root;
        private readonly List<GameObject> _objects = new List<GameObject>();
        private Sprite _outline;
        private List<Violation> _violations = new List<Violation>();
        public bool Visible { get; private set; }
        public bool Dirty { get; set; } = true;

        private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.18f);
        private static readonly Color FootColor = new Color(0.3f, 0.55f, 1f, 0.45f);
        private static readonly Color BlockedColor = new Color(0.25f, 0.25f, 0.25f, 0.35f);
        private static readonly Color UnreachColor = new Color(0.85f, 0.2f, 0.9f, 0.4f);
        private static readonly Color DoorColor = new Color(1f, 0.85f, 0.2f, 0.55f);
        private static readonly Color InteractColor = new Color(0.3f, 1f, 0.45f, 0.75f);
        private static readonly Color ArrowColor = new Color(1f, 1f, 1f, 0.95f);
        private static readonly Color BadColor = new Color(1f, 0.15f, 0.12f, 0.8f);

        public PlacementDebugOverlay(Transform parent)
        {
            var go = new GameObject("PlacementDebug");
            go.transform.SetParent(parent, false);
            _root = go.transform;
        }

        public void Toggle() { Visible = !Visible; Dirty = true; if (!Visible) Clear(); }

        private void Clear()
        {
            foreach (var o in _objects) if (o != null) Object.Destroy(o);
            _objects.Clear();
        }

        /// <summary>Katmanı kurar. area: etkin bölge (null = bütün harita), level: görünen kat.</summary>
        public void Rebuild(WorldState world, LayoutPlan plan, RectInt? area, int level, GridPos? reachFrom)
        {
            Dirty = false;
            Clear();
            if (!Visible || world == null) return;
            var map = world.Map;
            var validator = new PlacementValidator(world, plan, p => p.Z == level && (!area.HasValue || area.Value.Contains(new Vector2Int(p.X, p.Y))));
            LayoutRules.AddAll(validator, null);
            _violations = validator.Validate();
            int x0 = area?.xMin ?? 0, y0 = area?.yMin ?? 0, x1 = area?.xMax ?? map.Width, y1 = area?.yMax ?? map.Height;

            HashSet<GridPos> reach = null;
            if (reachFrom.HasValue && reachFrom.Value.Z == level)
                reach = Reach.From(map, reachFrom.Value, p => p.X >= x0 && p.Y >= y0 && p.X < x1 && p.Y < y1);

            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    var p = new GridPos(x, y, level);
                    if (!map.HasTile(p)) continue;
                    Mark(p, GridColor, 0.98f, 9500, outline: true);
                    var occ = world.GetItemAt(p);
                    if (occ != null && occ.Def.Placement == Thatchwick.Core.Content.ItemPlacement.Floor) Mark(p, FootColor, 0.92f, 9501);
                    else if (map.IsBlocked(p) || map.IsWater(p)) Mark(p, BlockedColor, 0.92f, 9501);
                    else if (reach != null && !reach.Contains(p)) Mark(p, UnreachColor, 0.6f, 9502);
                }

            foreach (var d in PlacementValidator.Doors(map))
            {
                if (d.Inside.Z != level) continue;
                Mark(d.Inside, DoorColor, 0.55f, 9503);
                var o = d.Outside;
                int depth = d.Exterior ? PlacementValidator.DoorOutside : PlacementValidator.DoorInside;
                for (int i = 0; i < depth; i++) { Mark(o, DoorColor, 0.55f, 9503); o = Facing.Step(o, d.OutDir); }
            }

            foreach (var it in world.Items)
            {
                if (it.Position.Z != level || !PlacementValidator.Occupies(it)) continue;
                if (area.HasValue && !area.Value.Contains(new Vector2Int(it.Position.X, it.Position.Y))) continue;
                var f = ItemFacing.Of(it);
                if (f.HasValue)
                {
                    // Ok: ayak izinin ön kenarında küçük elmas, ön yüze doğru iki nokta.
                    var (dx, dy) = Facing.Vec(f.Value);
                    float cx = it.Position.X + it.Width * 0.5f, cy = it.Position.Y + it.Height * 0.5f;
                    MarkAt(Iso.GridToWorld(cx + dx * 0.3f, cy + dy * 0.3f, level), ArrowColor, 0.22f, 9506);
                    MarkAt(Iso.GridToWorld(cx + dx * 0.48f, cy + dy * 0.48f, level), ArrowColor, 0.14f, 9506);
                }
                foreach (var t in ItemFacing.InteractionCells(it)) Mark(t, InteractColor, 0.35f, 9505);
            }

            foreach (var v in _violations)
                if (v.At.Z == level) Mark(v.At, BadColor, 0.8f, 9507);
        }

        private void Mark(GridPos p, Color c, float scale, int order, bool outline = false) =>
            MarkAt(Iso.GridToWorld(p.X + 0.5f, p.Y + 0.5f, p.Z), c, scale, order, outline);

        private void MarkAt(Vector3 pos, Color c, float scale, int order, bool outline = false)
        {
            var go = new GameObject("dbg");
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = outline ? Outline() : SpriteMaterials.Diamond;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = c;
            sr.sortingOrder = 30000 + order;
            _objects.Add(go);
        }

        // 64x32 elmasın yalnızca kenar çizgisi (karo ızgarası).
        private Sprite Outline()
        {
            if (_outline != null) return _outline;
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) tex.SetPixel(x, y, clear);
            for (int x = 0; x < w / 2; x++)
            {
                int y = x / 2;
                tex.SetPixel(x, h / 2 - 1 - y, Color.white); tex.SetPixel(w - 1 - x, h / 2 - 1 - y, Color.white);
                tex.SetPixel(x, h / 2 + y, Color.white); tex.SetPixel(w - 1 - x, h / 2 + y, Color.white);
            }
            tex.Apply();
            _outline = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
            return _outline;
        }

        public void OnGUI(float scale)
        {
            if (!Visible) return;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var counts = new Dictionary<string, int>();
            foreach (var v in _violations) { counts.TryGetValue(v.Code, out int n); counts[v.Code] = n + 1; }
            var lines = new List<string> { $"Yerleşim debug (F3) — ihlal: {_violations.Count}" };
            foreach (var kv in counts) lines.Add($"  {Rules.Name(kv.Key)}: {kv.Value}");
            lines.Add("mavi ayak izi · sarı kapı önü · yeşil etkileşim · beyaz ok yön · gri engel · mor ulaşılamaz · kırmızı ihlal");
            GUI.Box(new Rect(8f, 120f, 520f, 20f + lines.Count * 18f), string.Join("\n", lines));
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
