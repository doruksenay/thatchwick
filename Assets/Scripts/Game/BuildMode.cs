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

        private enum Tool { Items, Wall, Door, Window, EraseEdge, Room, FloorWood, FloorStone, FloorDirt, FloorErase, Plot }
        private Tool _tool = Tool.Items;
        private GridPos? _roomCorner;
        private Tool? _pendingTool;
        private bool _showRooms;
        private WallView _walls;
        private int _tab;                    // 0 envanter, 1 mağaza
        private string _placeId;             // envanterden yerleştirilecek eşya
        private string _plantId;             // dikilecek fidan/tohum
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
            WorldView view, WorldInput input, Transform parent, WallView walls = null)
        {
            _walls = walls;
            _world = world; _house = house; _svc = svc; _content = content; _view = view; _input = input;
            _overlay = new GameObject("BuildOverlay").transform;
            _overlay.SetParent(parent, false);
            _world.Map.Changed += () => { if (Active) RefreshOverlay(); };
        }

        /// <summary>GUI koordinatlarında (ölçeklenmiş) panel dikdörtgeni.</summary>
        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 278f, 8f, 270f, Screen.height / guiScale - 16f);

        public void Toggle() { if (Active) Exit(); else Enter(); }

        private void Enter()
        {
            Active = true;
            _view.ShowPlot = true;
            _view.RefreshFloors();
            ClearSelection();
            _status = "Yapı modu: eşya seç ya da envanterden koy.";
        }

        private void Exit()
        {
            Active = false;
            _view.ShowPlot = false;
            _view.RefreshFloors();
            ClearSelection();
            _tab = 0;
        }

        private void ClearSelection()
        {
            _roomCorner = null;
            _placeId = null; _plantId = null; _selectedId = 0; _surfaceId = 0; _moving = false; _rot = 0;
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

        // Eşya küpleri tabandan yukarı doğru çizilir; oyuncu görünen üst yüze dokunur. Önce yükseltilmiş
        // (üst yüz) noktayı, sonra tabanı dener.
        private ItemInstance PickItem(Vector3 world)
        {
            var raised = _view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f));
            return _world.GetItemAt(raised) ?? _world.GetItemAt(_view.WorldToTile(world));
        }

        public void OnTap(Vector3 world)
        {
            var tile = _view.WorldToTile(world);
            if (_tool != Tool.Items) { OnToolTap(world, tile); return; }

            if (_plantId != null)
            {
                var r = _svc.Plant(_plantId, tile, out _);
                _status = r.Ok ? $"Dikildi. Kalan: {_house.Count(_plantId)}" : r.Reason;
                if (r.Ok && _house.Count(_plantId) <= 0) ClearSelectionKeepStatus();
                return;
            }

            // Dekor yerleştirme: önce yuva işaretleri, sonra yüzey seçimi.
            if (_placeId != null && IsSurfaceDef(_placeId))
            {
                if (_surfaceId != 0 && TryTapSlot(world)) return;
                var surface = PickItem(world);
                if (surface != null && surface.Def.SurfaceSlots.Count > 0)
                {
                    _surfaceId = surface.Id;
                    Focus(surface);
                    _status = $"{surface.Def.DisplayName}: bir yuvaya dokun.";
                }
                else _status = surface != null ? $"{surface.Def.DisplayName} dekor taşımaz; masa ya da raf seç." : "Dekor için masa ya da raf seç.";
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
            var item = PickItem(world);
            _selectedId = item != null ? item.Id : 0;
            if (_focused) { _input.RestoreView(); _focused = false; }
            if (item != null && item.Def.SurfaceSlots.Count > 0) Focus(item);
            _status = item != null ? item.Def.DisplayName : RoomInfo(tile);
            RefreshOverlay();
        }

        // Dokunulan noktaya en yakın karo kenarı (karonun dört kenarından).
        private Edge NearestEdge(Vector3 world)
        {
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y));
            int x = Mathf.FloorToInt(g.x), y = Mathf.FloorToInt(g.y);
            float fx = g.x - x, fy = g.y - y;
            float dW = fx, dE = 1f - fx, dN = fy, dS = 1f - fy;
            float m = Mathf.Min(Mathf.Min(dW, dE), Mathf.Min(dN, dS));
            if (m == dW) return new Edge(x, y, EdgeAxis.Vertical);
            if (m == dE) return new Edge(x + 1, y, EdgeAxis.Vertical);
            if (m == dN) return new Edge(x, y, EdgeAxis.Horizontal);
            return new Edge(x, y + 1, EdgeAxis.Horizontal);
        }

        private void OnToolTap(Vector3 world, GridPos tile)
        {
            PlacementCheck r;
            switch (_tool)
            {
                case Tool.Wall: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Wall); break;
                case Tool.Door: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Door); break;
                case Tool.Window: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Window); break;
                case Tool.EraseEdge: r = _svc.SetEdge(NearestEdge(world), EdgeKind.None); break;
                case Tool.FloorWood: r = _svc.PaintFloor(tile, FloorType.Wood); break;
                case Tool.FloorStone: r = _svc.PaintFloor(tile, FloorType.Stone); break;
                case Tool.FloorDirt: r = _svc.PaintFloor(tile, FloorType.Dirt); break;
                case Tool.FloorErase: r = _svc.PaintFloor(tile, FloorType.Grass); break;
                case Tool.Plot: r = _svc.BuyChunk(tile); break;
                case Tool.Room: BuildRoomTap(tile); return;
                default: return;
            }
            _status = r.Ok ? "Tamam." : r.Reason;
        }

        // Oda aracı: iki köşe karosuna dokun; çevresine duvar, içine tahta zemin.
        private void BuildRoomTap(GridPos tile)
        {
            if (!_roomCorner.HasValue)
            {
                _roomCorner = tile;
                _status = "Karşı köşeye dokun.";
                RefreshOverlay();
                return;
            }
            var a = _roomCorner.Value;
            _roomCorner = null;
            int x0 = Mathf.Min(a.X, tile.X), x1 = Mathf.Max(a.X, tile.X);
            int y0 = Mathf.Min(a.Y, tile.Y), y1 = Mathf.Max(a.Y, tile.Y);
            string fail = null; int built = 0;
            for (int x = x0; x <= x1 && fail == null; x++)
                for (int y = y0; y <= y1 && fail == null; y++)
                {
                    var r = _svc.PaintFloor(new GridPos(x, y), FloorType.Wood);
                    if (!r.Ok && r.Reason != "Orası zaten öyle.") fail = r.Reason;
                }
            for (int x = x0; x <= x1 && fail == null; x++)
            {
                fail = TryWall(new Edge(x, y0, EdgeAxis.Horizontal), ref built) ?? TryWall(new Edge(x, y1 + 1, EdgeAxis.Horizontal), ref built);
            }
            for (int y = y0; y <= y1 && fail == null; y++)
            {
                fail = TryWall(new Edge(x0, y, EdgeAxis.Vertical), ref built) ?? TryWall(new Edge(x1 + 1, y, EdgeAxis.Vertical), ref built);
            }
            _status = fail != null ? $"Oda yarım kaldı: {fail}" : $"Oda kuruldu ({built} duvar). Kapı için Kapı aracını duvara kullan.";
            RefreshOverlay();
        }

        private string TryWall(Edge e, ref int built)
        {
            if (_world.Map.GetEdge(e) != EdgeKind.None) return null;
            var r = _svc.SetEdge(e, EdgeKind.Wall);
            if (r.Ok) built++;
            return r.Ok ? null : r.Reason;
        }

        private string RoomInfo(GridPos tile)
        {
            var room = _world.Map.Rooms.RoomAt(tile);
            if (room == null) return "";
            if (!room.IsIndoor) return _world.Map.IsOwned(tile) ? "Dış mekân (senin arsan)." : "Dış mekân (sahipsiz arsa).";
            return $"Oda: {room.Area} karo, {room.DoorCount} kapı.";
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

            if (_showRooms)
            {
                foreach (var room in _world.Map.Rooms.IndoorRooms)
                {
                    var c = Color.HSVToRGB((room.Id * 0.17f) % 1f, 0.6f, 1f);
                    c.a = 0.3f;
                    foreach (var t in room.Tiles) Diamond(t, c, -8000, 1f);
                }
            }
            if (_roomCorner.HasValue) Diamond(_roomCorner.Value, new Color(0.3f, 0.7f, 1f, 0.7f), 6000, 1f);

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
            if (_pendingTool.HasValue && Event.current.type == EventType.Layout)
            {
                _tool = _pendingTool.Value;
                _pendingTool = null;
                ClearSelection();
                _status = "";
            }
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"YAPI MODU · Akçe: {_house.Akce}");
            if (GUILayout.Button("Kapat", GUILayout.Width(60f))) { Exit(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();
            if (_status.Length > 0) GUILayout.Label(_status);
            if (Application.isEditor) // yalnızca geliştirirken: girdinin nerede takıldığını gösterir
                GUILayout.Label($"Girdi: basma {_input.PressCount}, arayüzde {_input.BlockedCount}, dokunma {_input.TapCount}");

            DrawTools();
            if (_tool != Tool.Items) { GUILayout.EndArea(); return; }

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

        private void DrawTools()
        {
            GUILayout.BeginHorizontal();
            if (_walls != null)
            {
                GUILayout.Label("Duvar:", GUILayout.Width(44f));
                if (GUILayout.Toggle(_walls.Mode == WallMode.Up, "Yukarı", GUI.skin.button)) _walls.Mode = WallMode.Up;
                if (GUILayout.Toggle(_walls.Mode == WallMode.Cutaway, "Kesit", GUI.skin.button)) _walls.Mode = WallMode.Cutaway;
                if (GUILayout.Toggle(_walls.Mode == WallMode.Down, "Aşağı", GUI.skin.button)) _walls.Mode = WallMode.Down;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ToolButton("Eşya", Tool.Items); ToolButton("Oda", Tool.Room); ToolButton("Duvar", Tool.Wall);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ToolButton("Kapı", Tool.Door); ToolButton("Pencere", Tool.Window); ToolButton("Kaldır", Tool.EraseEdge);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ToolButton("Arsa", Tool.Plot);
            if (GUILayout.Toggle(_showRooms, "Odalar", GUI.skin.button) != _showRooms) { _showRooms = !_showRooms; RefreshOverlay(); }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ToolButton("Tahta", Tool.FloorWood); ToolButton("Taş", Tool.FloorStone); ToolButton("Toprak", Tool.FloorDirt); ToolButton("Çim", Tool.FloorErase);
            GUILayout.EndHorizontal();
            if (_tool != Tool.Items)
                GUILayout.Label(_tool == Tool.Room ? "İki köşe karosuna dokun."
                    : _tool == Tool.Plot ? $"Bitişik 4x4 parçaya dokun: {BuildService.ChunkPrice} akçe."
                    : "Haritaya dokun. Fiyatlar: duvar 3, kapı 8, pencere 6, zemin 1-3. Kaldırınca yarısı iade.");
        }

        private void ToolButton(string label, Tool tool)
        {
            // Arayüz yerleşimi bozulmasın diye araç değişimi bir sonraki Layout olayında uygulanır.
            if (GUILayout.Toggle(_tool == tool, label, GUI.skin.button) && _tool != tool) _pendingTool = tool;
        }

        private void DrawContext()
        {
            var sel = _selectedId != 0 ? _world.GetItem(_selectedId) : null;

            if (_plantId != null)
            {
                GUILayout.Label($"Dikilecek: {_content.Items.Get(_plantId).DisplayName} (x{_house.Count(_plantId)})");
                GUILayout.Label("Arsanda dışarıda, çim ya da toprak bir karoya dokun.");
                if (GUILayout.Button("Bitir")) ClearSelection();
                return;
            }

            if (_placeId != null)
            {
                var def = _content.Items.Get(_placeId);
                GUILayout.Label($"Koyulacak: {def.DisplayName}");
                if (def.Placement == ItemPlacement.Surface)
                {
                    // Dokunma yerine listeden de seçilebilir (mobil ve küçük hedefler için).
                    foreach (var it in _world.Items)
                    {
                        if (it.IsDecor || it.Def.SurfaceSlots.Count == 0) continue;
                        if (GUILayout.Button($"{it.Def.DisplayName} ({it.Position.X},{it.Position.Y})"))
                        {
                            _surfaceId = it.Id;
                            Focus(it);
                            _status = $"{it.Def.DisplayName}: bir yuvaya dokun.";
                            RefreshOverlay();
                        }
                    }
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
            if (sel.IsPlant)
            {
                GUILayout.Label(sel.IsGrown ? "Olgun: canlı modda dokunup kesebilirsin." : $"Büyüyor: %{sel.GrowthFraction * 100f:0}. Bitkiler taşınmaz ya da satılmaz.");
                return;
            }
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
                if (def.PlantsAs != null && GUILayout.Button("Dik", GUILayout.Width(44f)))
                {
                    ClearSelection();
                    _plantId = kv.Key;
                    _status = "Dışarıda bir karoya dokun.";
                }
                if (def.Placement != ItemPlacement.Resource && GUILayout.Button("Koy", GUILayout.Width(44f)))
                {
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
