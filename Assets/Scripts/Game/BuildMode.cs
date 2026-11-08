using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Yapı modu arayüzü: envanter/mağaza, yerleştirme hayaleti, taşı/döndür/sat, masa gibi yüzeylere
    /// dekor koyarken kameranın eşyaya otomatik yaklaşması. Oyun yapı modunda durmaz.
    /// Kurallar BuildService'te; burada yalnızca girdi ve görsel var.
    /// </summary>
    public sealed class BuildMode
    {
        private const float FocusZoom = 3.6f;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly BuildService _svc;
        private readonly ContentDatabase _content;
        private readonly WorldView _view;
        private readonly WorldInput _input;
        private readonly Transform _overlay;

        private int _tab;                    // 0 envanter, 1 mağaza
        private string _placeId;             // envanterden yerleştirilecek eşya
        private int _selectedId;             // seçili yerleştirilmiş eşya (0 yok)
        private int _surfaceId;              // dekor koymak için odaklanılan yüzey eşyası (0 yok)
        private bool _moving;
        private GridPos _ghostPos;
        private int _rot;
        private string _status = "";
        private Vector2 _scroll;
        private bool _focused;

        public bool Active { get; private set; }

        public BuildMode(WorldState world, Household house, BuildService svc, ContentDatabase content,
            WorldView view, WorldInput input, Transform parent)
        {
            _world = world; _house = house; _svc = svc; _content = content; _view = view; _input = input;
            _overlay = new GameObject("BuildOverlay").transform;
            _overlay.SetParent(parent, false);
        }

        /// <summary>GUI koordinatlarında (ölçeklenmiş) panel dikdörtgeni.</summary>
        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 278f, 8f, 270f, Screen.height / guiScale - 16f);

        public void Toggle() { if (Active) Exit(); else Enter(); }

        private void Enter()
        {
            Active = true;
            ClearSelection();
            _status = "Yapı modu: eşya seç ya da envanterden koy.";
        }

        private void Exit()
        {
            Active = false;
            ClearSelection();
            _tab = 0;
        }

        private void ClearSelection()
        {
            _placeId = null; _selectedId = 0; _surfaceId = 0; _moving = false; _rot = 0;
            if (_focused) { _input.RestoreView(); _focused = false; }
            RefreshOverlay();
        }

        private void Focus(ItemInstance item)
        {
            var c = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f);
            _input.FocusOn(new Vector2(c.x, c.y + 0.3f), FocusZoom);
            _focused = true;
        }

        // ---------- girdi ----------

        public void OnTap(Vector3 world)
        {
            var tile = _view.WorldToTile(world);

            // Dekor yerleştirme: önce yuva işaretleri, sonra yüzey seçimi.
            if (_placeId != null && IsSurfaceDef(_placeId))
            {
                if (_surfaceId != 0 && TryTapSlot(world)) return;
                var surface = _world.GetItemAt(tile);
                if (surface != null && surface.Def.SurfaceSlots.Count > 0)
                {
                    _surfaceId = surface.Id;
                    Focus(surface);
                    _status = $"{surface.Def.DisplayName}: bir yuvaya dokun.";
                }
                else _status = "Dekor için masa ya da raf seç.";
                RefreshOverlay();
                return;
            }

            if (_placeId != null || _moving)
            {
                _ghostPos = tile;
                RefreshOverlay();
                return;
            }

            // Seçim
            var item = _world.GetItemAt(tile);
            _selectedId = item != null ? item.Id : 0;
            if (_focused) { _input.RestoreView(); _focused = false; }
            if (item != null && item.Def.SurfaceSlots.Count > 0) Focus(item);
            _status = item != null ? item.Def.DisplayName : "";
            RefreshOverlay();
        }

        private bool TryTapSlot(Vector3 world)
        {
            var surface = _world.GetItem(_surfaceId);
            if (surface == null) return false;
            string best = null; float bestD = 0.7f;
            foreach (var slot in surface.Def.SurfaceSlots)
            {
                var p = SlotWorld(surface, slot.Id);
                float d = Vector2.Distance(p, world);
                if (d < bestD) { bestD = d; best = slot.Id; }
            }
            if (best == null) return false;
            PlaceDecor(best);
            return true;
        }

        private void PlaceDecor(string slotId)
        {
            var r = _svc.PlaceDecorFromInventory(_placeId, _surfaceId, slotId, out _);
            _status = r.Ok ? "Yerleştirildi." : r.Reason;
            if (r.Ok && _house.Count(_placeId) <= 0) { _placeId = null; _surfaceId = 0; ClearSelectionKeepStatus(); }
            RefreshOverlay();
        }

        private void ClearSelectionKeepStatus() { var s = _status; ClearSelection(); _status = s; }

        private bool IsSurfaceDef(string id) => _content.Items.Get(id).Placement == ItemPlacement.Surface;

        private Vector3 SlotWorld(ItemInstance surface, string slotId)
        {
            surface.TryGetSlotPosition(slotId, out float gx, out float gy);
            return Iso.GridToWorld(gx, gy) + new Vector3(0f, 24f / 32f, 0f); // masa üstü yüksekliği
        }

        // ---------- görsel ----------

        private readonly List<GameObject> _overlayObjects = new List<GameObject>();

        public void RefreshOverlay()
        {
            foreach (var go in _overlayObjects) if (go != null) Object.Destroy(go);
            _overlayObjects.Clear();
            if (!Active) return;

            // Seçili eşya vurgusu
            var sel = _selectedId != 0 ? _world.GetItem(_selectedId) : null;
            if (sel != null && !_moving)
                foreach (var t in sel.OccupiedTiles()) Diamond(t, new Color(1f, 0.9f, 0.3f, 0.55f), -9000, 1f);

            // Hayalet
            ItemDef ghostDef = null; int? ignore = null;
            if (_placeId != null && !IsSurfaceDef(_placeId)) ghostDef = _content.Items.Get(_placeId);
            else if (_moving && sel != null) { ghostDef = sel.Def; ignore = sel.Id; }
            if (ghostDef != null)
            {
                var check = _world.CanPlaceFloor(ghostDef, _ghostPos, _rot, ignore);
                var col = check.Ok ? new Color(0.3f, 1f, 0.4f, 0.6f) : new Color(1f, 0.3f, 0.3f, 0.6f);
                var (w, h) = Footprint.Dimensions(ghostDef.Width, ghostDef.Height, _rot);
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                        Diamond(new GridPos(_ghostPos.X + dx, _ghostPos.Y + dy), col, 6000, 1f);
            }

            // Yuva işaretleri
            if (_surfaceId != 0)
            {
                var surface = _world.GetItem(_surfaceId);
                if (surface != null)
                {
                    foreach (var slot in surface.Def.SurfaceSlots)
                    {
                        bool full = !_world.CanPlaceOnSurface(_content.Items.Get(_placeId), _surfaceId, slot.Id).Ok;
                        var p = SlotWorld(surface, slot.Id);
                        DiamondAt(p, full ? new Color(1f, 0.4f, 0.3f, 0.9f) : new Color(0.4f, 1f, 0.5f, 0.9f), 6001, 0.32f);
                    }
                }
            }
        }

        private void Diamond(GridPos tile, Color color, int order, float scale) =>
            DiamondAt(_view.TileToWorld(tile), color, order, scale);

        private void DiamondAt(Vector3 pos, Color color, int order, float scale)
        {
            var go = new GameObject("BuildMark");
            go.transform.SetParent(_overlay, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Diamond;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = color;
            sr.sortingOrder = order;
            _overlayObjects.Add(go);
        }

        // ---------- panel ----------

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"YAPI MODU · Akçe: {_house.Akce}");
            if (GUILayout.Button("Kapat", GUILayout.Width(60f))) { Exit(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();
            if (_status.Length > 0) GUILayout.Label(_status);

            DrawContext();

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_tab == 0, "Envanter", GUI.skin.button)) _tab = 0;
            if (GUILayout.Toggle(_tab == 1, "Mağaza", GUI.skin.button)) _tab = 1;
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll);
            if (_tab == 0) DrawInventory(); else DrawShop();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawContext()
        {
            var sel = _selectedId != 0 ? _world.GetItem(_selectedId) : null;

            if (_placeId != null)
            {
                var def = _content.Items.Get(_placeId);
                GUILayout.Label($"Koyulacak: {def.DisplayName}");
                if (def.Placement == ItemPlacement.Surface)
                {
                    if (_surfaceId != 0)
                    {
                        var surface = _world.GetItem(_surfaceId);
                        GUILayout.BeginHorizontal();
                        if (surface != null)
                            foreach (var slot in surface.Def.SurfaceSlots)
                                if (GUILayout.Button($"Yuva {slot.Id}")) PlaceDecor(slot.Id);
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("Haritaya dokunarak konumu seç.");
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Döndür")) { _rot = (_rot + 1) % 4; RefreshOverlay(); }
                    if (GUILayout.Button("Koy")) PlaceFloor();
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("İptal")) ClearSelection();
                return;
            }

            if (sel == null) return;

            GUILayout.Label($"Seçili: {sel.Def.DisplayName}");
            if (_moving)
            {
                GUILayout.Label("Yeni konuma dokun, sonra Koy.");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Döndür")) { _rot = (_rot + 1) % 4; RefreshOverlay(); }
                if (GUILayout.Button("Koy"))
                {
                    var r = _svc.Move(sel.Id, _ghostPos, _rot);
                    _status = r.Ok ? "Taşındı." : r.Reason;
                    if (r.Ok) { _moving = false; ClearSelectionKeepStatus(); _selectedId = sel.Id; }
                    RefreshOverlay();
                }
                if (GUILayout.Button("Vazgeç")) { _moving = false; RefreshOverlay(); }
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Taşı")) { _moving = true; _ghostPos = sel.Position; _rot = sel.Rotation; RefreshOverlay(); }
            if (GUILayout.Button("Envantere al")) { _svc.StoreToInventory(sel.Id); _status = "Envantere alındı."; ClearSelectionKeepStatus(); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button($"Sat (+{_svc.SellValue(sel.Def)} akçe, dekorlar dahil)")) { _svc.SellPlaced(sel.Id); _status = "Satıldı."; ClearSelectionKeepStatus(); }
        }

        private void PlaceFloor()
        {
            var r = _svc.PlaceFromInventory(_placeId, _ghostPos, _rot, out _);
            _status = r.Ok ? "Yerleştirildi." : r.Reason;
            if (r.Ok && _house.Count(_placeId) <= 0) ClearSelectionKeepStatus();
            RefreshOverlay();
        }

        private void DrawInventory()
        {
            var list = _house.InventoryList();
            if (list.Count == 0) GUILayout.Label("Envanter boş. Mağazadan eşya al.");
            foreach (var kv in list)
            {
                if (!_content.Items.TryGet(kv.Key, out var def)) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{def.DisplayName} x{kv.Value}");
                if (GUILayout.Button("Koy", GUILayout.Width(44f)))
                {
                    var keep = _status;
                    ClearSelection();
                    _placeId = kv.Key;
                    _ghostPos = new GridPos(24, 12);
                    _status = def.Placement == ItemPlacement.Surface ? "Bir masa ya da rafa dokun." : "Haritaya dokun.";
                    RefreshOverlay();
                }
                if (GUILayout.Button($"Sat {_svc.SellValue(def)}", GUILayout.Width(62f))) { _svc.SellFromInventory(kv.Key); _status = "Satıldı."; }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawShop()
        {
            string cat = null;
            foreach (var def in _svc.ShopList())
            {
                if (def.Category != cat) { cat = def.Category; GUILayout.Label(cat == "decor" ? "— Dekor —" : "— Mobilya —"); }
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{def.DisplayName} ({def.Width}x{def.Height})");
                if (GUILayout.Button($"Al {def.Price}", GUILayout.Width(62f)))
                {
                    var r = _svc.Buy(def.Id);
                    _status = r.Ok ? $"{def.DisplayName} alındı." : r.Reason;
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
