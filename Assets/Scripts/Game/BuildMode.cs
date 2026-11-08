using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Animals;
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

        private enum Tool { Items, Wall, Door, Window, EraseEdge, Room, FloorWood, FloorStone, FloorDirt, FloorErase, Plot, ZoneStorage, ZoneErase }
        private Tool _tool = Tool.Items;
        private GridPos? _roomCorner;
        private Tool? _pendingTool;
        private bool _showRooms;
        private WallView _walls;
        private int _tab;                    // 0 envanter, 1 satın al
        private int _shopCat;                // kategori sırası; Categories.Length = hayvanlar
        private int _roomFilter;             // 0 hepsi, sonra ItemCatalog.Rooms
        private int _setFilter;              // 0 hepsi, sonra ItemCatalog.StyleSets
        private GUIStyle _wrap;
        // Arayüz yapısını değiştiren tıklamalar (kategori, satın alma) bir sonraki Layout olayında uygulanır;
        // aynı olayda denetim sayısı değişip IMGUI hatası vermesin.
        private System.Action _deferred;
        private readonly AnimalSystem _animals;
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

        /// <summary>Hesap XP'si için yapı modu etkinlikleri (plant ...).</summary>
        public event System.Action<string> Activity;

        public BuildMode(WorldState world, Household house, BuildService svc, ContentDatabase content,
            WorldView view, WorldInput input, Transform parent, WallView walls = null, AnimalSystem animals = null)
        {
            _walls = walls;
            _animals = animals;
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
            var wall = PickWallItem(_world, world);
            if (wall != null) return wall;
            var raised = _view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f));
            return _world.GetItemAt(raised) ?? _world.GetItemAt(_view.WorldToTile(world));
        }

        /// <summary>Dokunulan noktaya yakın (çizildiği yerde) duvar eşyası; yoksa null.</summary>
        public static ItemInstance PickWallItem(WorldState world, Vector3 point)
        {
            ItemInstance best = null;
            float bestD = 0.45f;
            foreach (var it in world.Items)
            {
                if (!it.IsWallMounted) continue;
                float d = Vector2.Distance(Iso.WallMount(it.Position, it.Rotation), point);
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }

        /// <summary>
        /// Duvar eşyası için dokunulan noktaya en yakın düz duvar: (önündeki karo, kenar). Dokunuş duvarın
        /// yüzüne (asılma yüksekliğine) yapılır; o yükseklik düşülüp karonun duvarlı kenarlarından en yakını seçilir.
        /// </summary>
        private bool NearestWallSide(Vector3 world, out GridPos tile, out int side)
        {
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y - Iso.WallMountHeight));
            tile = new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y));
            float fx = g.x - tile.X, fy = g.y - tile.Y;
            float[] dist = { fy, 1f - fx, 1f - fy, fx };   // kenar 0 (-y), 1 (+x), 2 (+y), 3 (-x)
            side = -1;
            float best = float.MaxValue;
            for (int k = 0; k < 4; k++)
            {
                if (_world.Map.GetEdge(WallSides.EdgeOf(tile, k)) != EdgeKind.Wall) continue;
                if (dist[k] < best) { best = dist[k]; side = k; }
            }
            return side >= 0;
        }

        public void OnTap(Vector3 world)
        {
            var tile = _view.WorldToTile(world);
            if (_tool != Tool.Items) { OnToolTap(world, tile); return; }

            if (_plantId != null)
            {
                var r = _svc.Plant(_plantId, tile, out _);
                _status = r.Ok ? $"Dikildi. Kalan: {_house.Count(_plantId)}" : r.Reason;
                if (r.Ok) Activity?.Invoke("plant");
                if (r.Ok && _house.Count(_plantId) <= 0) ClearSelectionKeepStatus();
                return;
            }

            // Duvar eşyası: dokunulan duvarın iç yüzüne asılır.
            if (_placeId != null && IsWallDef(_placeId))
            {
                if (!NearestWallSide(world, out var wt, out int side)) { _status = "Bir duvarın yüzüne dokun (kapı ve pencere olmaz)."; return; }
                var wr = _svc.PlaceWallFromInventory(_placeId, wt, side, out _);
                _status = wr.Ok ? $"Asıldı. Kalan: {_house.Count(_placeId)}" : wr.Reason;
                if (wr.Ok && _house.Count(_placeId) <= 0) ClearSelectionKeepStatus();
                RefreshOverlay();
                return;
            }

            // Duvar eşyasını taşıma: yeni duvara dokununca hemen taşınır.
            var movingSel = _moving && _selectedId != 0 ? _world.GetItem(_selectedId) : null;
            if (movingSel != null && movingSel.IsWallMounted)
            {
                if (!NearestWallSide(world, out var mt, out int mside)) { _status = "Bir duvarın yüzüne dokun."; return; }
                var mr = _svc.Move(movingSel.Id, mt, mside);
                _status = mr.Ok ? "Taşındı." : mr.Reason;
                if (mr.Ok) _moving = false;
                RefreshOverlay();
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
                case Tool.ZoneStorage: r = _svc.SetZone(tile, ZoneType.Storage); break;
                case Tool.ZoneErase: r = _svc.SetZone(tile, ZoneType.None); break;
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
        private bool IsWallDef(string id) => _content.Items.Get(id).Placement == ItemPlacement.Wall;

        private Vector3 SlotWorld(ItemInstance surface, string slotId)
        {
            surface.TryGetSlotPosition(slotId, out float gx, out float gy);
            if (surface.IsWallMounted) return Iso.GridToWorld(gx, gy) + new Vector3(0f, Iso.WallMountHeight + 0.1f, 0f); // duvar rafı
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
            if (sel != null && sel.IsWallMounted)
            {
                Diamond(sel.Position, new Color(1f, 0.9f, 0.3f, 0.4f), -9000, 1f);
                DiamondAt(Iso.WallMount(sel.Position, sel.Rotation), new Color(1f, 0.9f, 0.3f, 0.9f), 6001, 0.35f);
            }

            // Hayalet
            ItemDef ghostDef = null; int? ignore = null;
            if (_placeId != null && !IsSurfaceDef(_placeId) && !IsWallDef(_placeId)) ghostDef = _content.Items.Get(_placeId);
            else if (_moving && sel != null && !sel.IsWallMounted) { ghostDef = sel.Def; ignore = sel.Id; }
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
            if (_wrap == null) _wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 11 };
            if (_deferred != null && Event.current.type == EventType.Layout)
            {
                var d = _deferred;
                _deferred = null;
                d();
            }
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
            if (GUILayout.Toggle(_tab == 1, "Satın al", GUI.skin.button)) _tab = 1;
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
            ToolButton("Depo", Tool.ZoneStorage); ToolButton("Depo sil", Tool.ZoneErase);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ToolButton("Tahta", Tool.FloorWood); ToolButton("Taş", Tool.FloorStone); ToolButton("Toprak", Tool.FloorDirt); ToolButton("Çim", Tool.FloorErase);
            GUILayout.EndHorizontal();
            if (_tool != Tool.Items)
                GUILayout.Label(_tool == Tool.Room ? "İki köşe karosuna dokun."
                    : _tool == Tool.Plot ? $"Bitişik 4x4 parçaya dokun: {BuildService.ChunkPrice} akçe."
                    : _tool == Tool.ZoneStorage || _tool == Tool.ZoneErase ? $"Depo bölgesi: karo başına +{StorageService.UnitsPerZoneTile} kaynak kapasitesi (ücretsiz). Sandık +40."
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
                else if (def.Placement == ItemPlacement.Wall)
                {
                    GUILayout.Label("Bir duvarın iç yüzüne dokun: eşya oraya asılır. (Duvarı göremiyorsan Duvar: Yukarı.)");
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
            if (_moving && sel.IsWallMounted)
            {
                GUILayout.Label("Yeni bir duvara dokun.");
                if (GUILayout.Button("Vazgeç")) { _moving = false; RefreshOverlay(); }
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

            GUILayout.Label(ItemCatalog.Describe(sel.Def), _wrap);
            if (sel.Def.IsHousing && _animals != null)
                GUILayout.Label($"Hayvan {_animals.Residents(sel.Id)}/{sel.Def.HousingCapacity} · yem {sel.FeedStock:0.#} · ürün {sel.StockTotal}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Taşı")) { _moving = true; _ghostPos = sel.Position; _rot = sel.Rotation; RefreshOverlay(); }
            if (GUILayout.Button("Envantere al")) { var r = _svc.StoreToInventory(sel.Id); _status = r.Ok ? "Envantere alındı." : r.Reason; if (r.Ok) ClearSelectionKeepStatus(); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button($"Sat (+{_svc.SellValue(sel.Def)} akçe, dekorlar dahil)")) { var r = _svc.SellPlaced(sel.Id); _status = r.Ok ? "Satıldı." : r.Reason; if (r.Ok) ClearSelectionKeepStatus(); }
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
                if (def.Placement != ItemPlacement.Resource && GUILayout.Button("Koy", GUILayout.Width(44f))) StartPlacing(kv.Key);
                if (def.Placement != ItemPlacement.Resource && GUILayout.Button($"Sat {_svc.SellValue(def)}", GUILayout.Width(62f))) { _svc.SellFromInventory(kv.Key); _status = "Satıldı."; }
                GUILayout.EndHorizontal();
            }
        }

        private void StartPlacing(string itemId)
        {
            ClearSelection();
            _placeId = itemId;
            _ghostPos = new GridPos(24, 12);
            var def = _content.Items.Get(itemId);
            _status = def.Placement == ItemPlacement.Surface ? "Bir masa ya da rafa dokun."
                : def.Placement == ItemPlacement.Wall ? "Bir duvarın iç yüzüne dokun." : "Haritaya dokun.";
            RefreshOverlay();
        }

        private static readonly string[] RoomFilterNames = BuildRoomFilterNames();
        private static readonly string[] SetFilterNames = BuildSetFilterNames();

        private static string[] BuildRoomFilterNames()
        {
            var n = new string[ItemCatalog.Rooms.Length + 1];
            n[0] = "Her oda";
            for (int i = 0; i < ItemCatalog.Rooms.Length; i++) n[i + 1] = ItemCatalog.Rooms[i].name;
            return n;
        }

        private static string[] BuildSetFilterNames()
        {
            var n = new string[ItemCatalog.StyleSets.Length + 1];
            n[0] = "Her takım";
            for (int i = 0; i < ItemCatalog.StyleSets.Length; i++) n[i + 1] = ItemCatalog.StyleSets[i];
            return n;
        }

        // Sims tarzı satın alma: kategori sekmeleri, oda ve takım filtresi, eşya kartları.
        private void DrawShop()
        {
            int n = ItemCatalog.Categories.Length;
            var names = new string[n + 1];
            for (int i = 0; i < n; i++) names[i] = ItemCatalog.Categories[i].name;
            names[n] = "Hayvan";
            int cat = GUILayout.SelectionGrid(_shopCat, names, 3);
            if (cat != _shopCat) _deferred = () => { _shopCat = cat; _scroll = Vector2.zero; };
            if (_shopCat == n) { DrawAnimals(); return; }

            int roomF = GUILayout.SelectionGrid(_roomFilter, RoomFilterNames, 3);
            if (roomF != _roomFilter) _deferred = () => _roomFilter = roomF;
            int setF = GUILayout.SelectionGrid(_setFilter, SetFilterNames, 3);
            if (setF != _setFilter) _deferred = () => _setFilter = setF;
            string room = _roomFilter > 0 ? ItemCatalog.Rooms[_roomFilter - 1].id : null;
            string set = _setFilter > 0 ? ItemCatalog.StyleSets[_setFilter - 1] : null;

            var list = ItemCatalog.InCategory(_content, ItemCatalog.Categories[_shopCat].id, room, set);
            if (list.Count == 0) GUILayout.Label("Bu filtreyle eşya yok.");
            foreach (var def in list) DrawCard(def);
        }

        private void DrawCard(ItemDef def)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            string set = string.IsNullOrEmpty(def.StyleSet) ? "" : $" · {def.StyleSet}";
            GUILayout.Label($"{def.DisplayName} ({def.Width}x{def.Height}){set}");
            string info = ItemCatalog.Describe(def);
            if (info.Length > 0) GUILayout.Label(info, _wrap);
            bool unlocked = _svc.IsUnlocked(def);
            int have = _house.Count(def.Id);
            GUILayout.BeginHorizontal();
            GUI.enabled = unlocked;
            if (GUILayout.Button(unlocked ? $"Al {def.Price}" : $"Sv {def.UnlockLevel} gerekli"))
                _deferred = () =>
                {
                    var r = _svc.Buy(def.Id);
                    _status = r.Ok ? $"{def.DisplayName} alındı (envanterde)." : r.Reason;
                };
            if (unlocked && GUILayout.Button("Al ve koy"))
                _deferred = () =>
                {
                    var r = _svc.Buy(def.Id);
                    if (r.Ok) StartPlacing(def.Id); else _status = r.Reason;
                };
            GUI.enabled = true;
            if (have > 0) GUILayout.Label($"Envanterde {have}", GUILayout.Width(80f));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawAnimals()
        {
            if (_animals == null) { GUILayout.Label("Hayvanlar bu sahnede yok."); return; }
            GUILayout.Label("Önce barınak koy (Bahçe: kümes, ahır, kulübe, sepet, kovan), sonra hayvanı al. Yemi pazardan al, barınağa dokunup \"Yem koy\".", _wrap);
            int level = _svc.AccountLevel != null ? _svc.AccountLevel() : int.MaxValue;
            foreach (var a in _content.Animals.All)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"{a.DisplayName} · {a.Price} akçe");
                GUILayout.Label(ItemCatalog.Describe(a, _content), _wrap);
                bool unlocked = level >= a.UnlockLevel;
                GUI.enabled = unlocked;
                if (GUILayout.Button(unlocked ? $"Al {a.Price}" : $"Sv {a.UnlockLevel} gerekli"))
                {
                    var species = a;
                    _deferred = () =>
                    {
                        var r = _animals.Buy(species.Id, _house, out var bought, level);
                        _status = r.Ok ? $"{bought.Name} geldi." : r.Reason;
                        if (r.Ok) Activity?.Invoke("animal");
                    };
                }
                GUI.enabled = true;
                GUILayout.EndVertical();
            }
            if (_animals.Animals.Count == 0) return;
            GUILayout.Label("— Hayvanların —");
            foreach (var a in new List<Animal>(_animals.Animals))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{a.Name} · tokluk {a.Fullness:0}{(a.IsMature ? "" : " · yavru")}");
                if (GUILayout.Button($"Sat {(int)(a.Species.Price * AnimalSystem.SellRatio)}", GUILayout.Width(62f)))
                {
                    var animal = a;
                    _deferred = () =>
                    {
                        var r = _animals.Sell(animal.Id, _house);
                        _status = r.Ok ? $"{animal.Name} satıldı." : r.Reason;
                    };
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
