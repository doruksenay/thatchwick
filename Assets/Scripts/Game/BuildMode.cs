using System.Collections.Generic;
using System.Linq;
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
        /// <summary>Miras: seçili eşyayı yadigâr yapmak (Soy Mührü). Null ise düğme yok.</summary>
        public Thatchwick.Core.Family.Legacy Legacy { get; set; }
        /// <summary>Oda konforu dökümü (boyut, zemin, pencere); null ise gösterilmez.</summary>
        public Thatchwick.Core.Life.MoodService Comfort { get; set; }
        /// <summary>Yadigârı yapan kişi (baş karakter) ve bugünün günü.</summary>
        public System.Func<Thatchwick.Core.Characters.Character> Maker { get; set; }
        public System.Func<double> Today { get; set; }

        private const float FocusZoom = 3.6f;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly BuildService _svc;
        private readonly ContentDatabase _content;
        private readonly WorldView _view;
        private readonly WorldInput _input;
        private readonly Transform _overlay;

        private enum Tool
        {
            Items, Room, Wall, HalfWall, Door, Window, EraseEdge, Fence, Gate, Railing,
            Finish, FinishRoom, Floor, FloorRoom, FloorErase, Roof,
            PushWall, MoveRoom, CopyRoom, Eyedropper, Demolish,
            Plot, ZoneStorage, ZoneErase, LevelBuild, LevelRemove, ZonePasture, RoadPath, RoadGravel, RoadPaved, RoadErase, Pond, Conduit
        }
        private Tool _tool = Tool.Items;
        private GridPos? _roomCorner;
        // Araç başına seçili parça (malzeme, kapı/pencere stili, zemin, çatı, kaplama); null = varsayılan / çıplak.
        private readonly Dictionary<BuildPartKind, string> _style = new Dictionary<BuildPartKind, string>();
        private GridPos? _lineStart;          // çizgi aracının ilk köşesi (x, y = köşe noktası)
        private GridPos? _roomPick;           // taşınacak/kopyalanacak odanın bir karosu
        private Edge? _pushEdge;              // itilecek duvar
        private string _pendingFinish;        // damlalıkla alınan eşyanın kaplaması (yerleşince uygulanır)
        private Vector2 _partScroll;
        private int _houseValue = -1;         // ev değeri önbelleği (harita ya da eşya değişince yeniden)
        private const string NoRoofChoice = GridMap.NoRoof;
        private Tool? _pendingTool;
        private ConduitType _conduitType = ConduitType.Ditch;     // su yolu aracında seçili tür (None = kaldır)
        private ConduitType? _pendingConduit;
        private GridPos? _conduitStart;                           // su yolu hattının ilk karosu
        private int? _pendingLevel;
        private bool _showRooms;
        private WallView _walls;
        private int _mode = 1;               // 0 İnşa, 1 Eşya (satın al), 2 Envanter (29-menu-yapisi)
        private int _buildGroup;             // İnşa sekmesinde seçili araç grubu
        private string _search = "";         // eşya arama kutusu
        private string _activeSearch = "";   // Layout'ta kesinleşen arama (aynı olayda denetim sayısı değişmesin)
        private bool _showFilters;
        private HashSet<string> _seen;       // görülmüş açık eşyalar ("Yeni" rozeti; şimdilik kayda yazılmaz)
        private readonly HashSet<string> _pendingSeen = new HashSet<string>();
        private readonly List<int> _catTabs = new List<int>();   // görünen kategori sekmeleri (boşlar gizli)
        private string[] _catNames;
        private int _shopGroup;              // Eşya ana grubu (ShopGroups)
        private readonly List<int> _groupTabs = new List<int>();
        private string[] _groupNames;
        private int _shopCat;                // kategori sırası; Categories.Length = hayvanlar
        private int _roomFilter;             // 0 hepsi, sonra ItemCatalog.Rooms
        private int _setFilter;              // 0 hepsi, sonra ItemCatalog.StyleSets
        private int _themeFilter;            // 0 hepsi, sonra Decor.Themes (Çatkı, Yörük)
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
        private int _band = -1;              // duvar eşyası bandı (-1: kendiliğinden ilk boş bant; 27-ev-dekorasyonu)
        private string _confirmSlot;         // aşırı yük onayı bekleyen yuva (raf çökebilir)
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
            _world.Map.Changed += () => { _houseValue = -1; if (Active) RefreshOverlay(); };
            _world.Changed += () => _houseValue = -1;
        }

        /// <summary>GUI koordinatlarında (ölçeklenmiş) panel dikdörtgeni.</summary>
        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 278f, 8f, 270f, Screen.height / guiScale - 104f);   // alttaki ihtiyaç çubuğuna binmesin

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
            MarkSeen();
            _mode = 1;
            _tool = Tool.Items;
        }

        private void ClearSelection()
        {
            _roomCorner = null; _lineStart = null; _roomPick = null; _pushEdge = null; _conduitStart = null;
            _placeId = null; _plantId = null; _selectedId = 0; _surfaceId = 0; _moving = false; _rot = 0;
            if (_focused) { _input.RestoreView(); _focused = false; }
            RefreshOverlay();
        }

        private void Focus(ItemInstance item)
        {
            var c = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, item.Position.Z);
            _input.FocusOn(new Vector2(c.x, c.y + 0.3f), FocusZoom);
            _focused = true;
        }

        // ---------- girdi ----------

        // Eşya küpleri tabandan yukarı doğru çizilir; oyuncu görünen üst yüze dokunur. Önce yükseltilmiş
        // (üst yüz) noktayı, sonra tabanı dener.
        private ItemInstance PickItem(Vector3 world)
        {
            var wall = PickWallItem(_world, world, _view);
            if (wall != null) return wall;
            var raised = _view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f));
            var tile = _view.WorldToTile(world);
            return _world.GetItemAt(raised) ?? _world.GetItemAt(tile) ?? CeilingNear(world) ?? _world.GetRugAt(tile);
        }

        // Tavan eşyası: tavan yüksekliğinde dokunulan karo (kat görünürken ve çatı açıkken).
        private ItemInstance CeilingNear(Vector3 world)
        {
            var t = _view.WorldToTile(world - new Vector3(0f, Iso.CeilingHeight, 0f));
            return _world.CeilingAt(t);
        }

        /// <summary>Dokunulan noktaya yakın (çizildiği yerde) duvar eşyası; yoksa null.</summary>
        public static ItemInstance PickWallItem(WorldState world, Vector3 point, WorldView view = null)
        {
            ItemInstance best = null;
            float bestD = 0.45f;
            foreach (var it in world.Items)
            {
                if (!it.IsWallMounted) continue;
                if (view != null && !view.IsLevelVisible(it.Position.Z)) continue;
                float d = Vector2.Distance(Iso.WallMountSpan(it.Position, it.Rotation, it.WallSpan, it.WallBand), point);
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }

        /// <summary>
        /// Duvar eşyası için dokunulan noktaya en yakın düz duvar: (önündeki karo, kenar). Dokunuş duvarın
        /// yüzüne (asılma yüksekliğine) yapılır; o yükseklik düşülüp karonun duvarlı kenarlarından en yakını seçilir.
        /// </summary>
        private bool NearestWallSide(Vector3 world, out GridPos tile, out int side, bool window = false)
        {
            var want = window ? EdgeKind.Window : EdgeKind.Wall;   // pencere içi tahtası ve perde yalnızca pencereye (D2)
            int z = _view.ViewLevel;
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y - Iso.WallMountHeight), z);
            tile = new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y), z);
            float fx = g.x - tile.X, fy = g.y - tile.Y;
            float[] dist = { fy, 1f - fx, 1f - fy, fx };   // kenar 0 (-y), 1 (+x), 2 (+y), 3 (-x)
            side = -1;
            float best = float.MaxValue;
            for (int k = 0; k < 4; k++)
            {
                if (_world.Map.GetEdge(WallSides.EdgeOf(tile, k)) != want) continue;
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
                bool win = _content.Items.TryGet(_placeId, out var wdef) && wdef.WindowMount;
                if (!NearestWallSide(world, out var wt, out int side, win)) { _status = win ? "Bir pencereye dokun." : "Bir duvarın yüzüne dokun (kapı ve pencere olmaz)."; return; }
                var wr = _svc.PlaceWallFromInventory(_placeId, wt, side, out var hungItem, _band);
                _status = wr.Ok ? $"Asıldı ({Decor.BandName(hungItem.WallBand)} bant). Kalan: {_house.Count(_placeId)}" : wr.Reason;
                if (wr.Ok && _house.Count(_placeId) <= 0) ClearSelectionKeepStatus();
                RefreshOverlay();
                return;
            }

            // Duvar eşyasını taşıma: yeni duvara dokununca hemen taşınır.
            var movingSel = _moving && _selectedId != 0 ? _world.GetItem(_selectedId) : null;
            if (movingSel != null && movingSel.IsWallMounted)
            {
                if (!NearestWallSide(world, out var mt, out int mside, movingSel.Def.WindowMount)) { _status = movingSel.Def.WindowMount ? "Bir pencereye dokun." : "Bir duvarın yüzüne dokun."; return; }
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
                    _status = NoFitStatus(surface) ?? $"{surface.Def.DisplayName}: bir yuvaya dokun.";
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
            int z = _view.ViewLevel;
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y), z);
            int x = Mathf.FloorToInt(g.x), y = Mathf.FloorToInt(g.y);
            float fx = g.x - x, fy = g.y - y;
            float dW = fx, dE = 1f - fx, dN = fy, dS = 1f - fy;
            float m = Mathf.Min(Mathf.Min(dW, dE), Mathf.Min(dN, dS));
            if (m == dW) return new Edge(x, y, EdgeAxis.Vertical, z);
            if (m == dE) return new Edge(x + 1, y, EdgeAxis.Vertical, z);
            if (m == dN) return new Edge(x, y, EdgeAxis.Horizontal, z);
            return new Edge(x, y + 1, EdgeAxis.Horizontal, z);
        }

        // Dokunulan noktaya en yakın karo köşesi (çizgi araçları köşeden köşeye çizer).
        private GridPos NearestVertex(Vector3 world)
        {
            int z = _view.ViewLevel;
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y), z);
            return new GridPos(Mathf.RoundToInt(g.x), Mathf.RoundToInt(g.y), z);
        }

        private string Style(BuildPartKind kind) => _style.TryGetValue(kind, out var s) ? s : null;

        private void OnToolTap(Vector3 world, GridPos tile)
        {
            PlacementCheck r;
            switch (_tool)
            {
                case Tool.Wall: LineTap(world, EdgeKind.Wall); return;
                case Tool.HalfWall: LineTap(world, EdgeKind.HalfWall); return;
                case Tool.Fence: LineTap(world, EdgeKind.Fence); return;
                case Tool.Railing: LineTap(world, EdgeKind.Railing); return;
                case Tool.Door: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Door, Style(BuildPartKind.Door)); break;
                case Tool.Window: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Window, Style(BuildPartKind.Window)); break;
                case Tool.Gate: r = _svc.SetEdge(NearestEdge(world), EdgeKind.Gate, Style(BuildPartKind.Gate)); break;
                case Tool.EraseEdge: r = _svc.SetEdge(NearestEdge(world), EdgeKind.None); break;
                case Tool.Finish:
                {
                    // Dokunulan karoya bakan duvar yüzü boyanır.
                    var e = NearestEdge(world);
                    r = _svc.PaintWallSide(e, e.SideB == tile, Style(BuildPartKind.WallFinish));
                    break;
                }
                case Tool.FinishRoom:
                    r = _svc.PaintRoomWalls(_world.Map.Rooms.RoomAt(tile), Style(BuildPartKind.WallFinish), out int faces);
                    if (r.Ok) { _status = $"{faces} duvar yüzü kaplandı."; return; }
                    break;
                case Tool.Floor:
                    r = Style(BuildPartKind.Floor) != null ? _svc.PaintFloorPart(tile, Style(BuildPartKind.Floor))
                        : _svc.PaintFloor(tile, tile.Z < 0 ? FloorType.Dirt : FloorType.Wood);
                    break;
                case Tool.FloorRoom:
                {
                    var part = Style(BuildPartKind.Floor) ?? BuildParts.DefaultFloor(_content, tile.Z < 0 ? FloorType.Dirt : FloorType.Wood)?.Id;
                    r = _svc.PaintRoomFloor(_world.Map.Rooms.RoomAt(tile), part, out int tiles);
                    if (r.Ok) { _status = $"{tiles} karo döşendi."; return; }
                    break;
                }
                case Tool.FloorErase: r = _svc.PaintFloor(tile, FloorType.Grass); break;
                case Tool.Pond: r = _svc.PaintFloor(tile, FloorType.Water); break;
                case Tool.Roof:
                    r = _svc.SetRoofStyle(tile, Style(BuildPartKind.Roof) ?? BuildParts.Default(_content, BuildPartKind.Roof)?.Id);
                    break;
                case Tool.PushWall:
                {
                    var e = NearestEdge(world);
                    if (!EdgeKinds.IsRoomBoundary(_world.Map.GetEdge(e))) { _status = "Bir odanın duvarına dokun."; return; }
                    _pushEdge = e;
                    _status = "Oklarla duvarı it ya da çek.";
                    RefreshOverlay();
                    return;
                }
                case Tool.MoveRoom:
                case Tool.CopyRoom: RoomPickTap(tile); return;
                case Tool.Eyedropper: EyedropperTap(world, tile); return;
                case Tool.Demolish: DemolishTap(tile); return;
                case Tool.Plot: r = _svc.BuyChunk(tile); break;
                case Tool.LevelBuild: r = _svc.BuildLevelTile(tile); break;
                case Tool.LevelRemove: r = _svc.RemoveLevelTile(tile); break;
                case Tool.ZoneStorage: r = _svc.SetZone(tile, ZoneType.Storage); break;
                case Tool.ZoneErase: r = _svc.SetZone(tile, ZoneType.None); break;
                case Tool.ZonePasture: r = _svc.SetZone(tile, ZoneType.Pasture); break;
                case Tool.RoadPath: r = _svc.PaintRoad(tile, RoadType.Path); break;
                case Tool.RoadGravel: r = _svc.PaintRoad(tile, RoadType.Gravel); break;
                case Tool.RoadPaved: r = _svc.PaintRoad(tile, RoadType.Paved); break;
                case Tool.RoadErase: r = _svc.PaintRoad(tile, RoadType.None); break;
                case Tool.Conduit: ConduitTap(tile); return;
                case Tool.Room: BuildRoomTap(tile); return;
                default: return;
            }
            _status = r.Ok ? "Tamam." : r.Reason;
        }

        // Su yolu: hattın iki ucuna dokun; arası L biçiminde (önce yatay, sonra dikey) döşenir. Aynı karoya iki kez
        // dokunmak tek karo döşer.
        private void ConduitTap(GridPos tile)
        {
            if (tile.Z != 0) { _status = "Su yolu zemin katta döşenir: Kat'tan zemin katı seç (üst kata kurşun boru kolonuyla çıkar)."; return; }
            if (!_conduitStart.HasValue)
            {
                _conduitStart = tile;
                _status = $"{Conduits.Name(_conduitType)}: hattın öbür ucuna dokun (aynı karo: tek karo).";
                return;
            }
            var from = _conduitStart.Value;
            _conduitStart = null;
            int n = _svc.PaintConduitRun(from, tile, _conduitType, out var problem);
            string what = _conduitType == ConduitType.None ? "su yolu kaldırıldı" : $"{Conduits.Name(_conduitType)} döşendi";
            _status = n > 0 ? $"{n} karo {what}." + (problem != null ? $" Atlanan: {problem}" : "") : problem ?? "Değişen bir şey yok.";
        }

        // Çizgi araçları: iki köşeye dokun; arası düz hat olarak kurulur (aynı köşeye iki kez dokunmak vazgeçer).
        private void LineTap(Vector3 world, EdgeKind kind)
        {
            var v = NearestVertex(world);
            if (!_lineStart.HasValue)
            {
                _lineStart = v;
                _status = "Hattın öbür ucundaki köşeye dokun.";
                RefreshOverlay();
                return;
            }
            var a = _lineStart.Value;
            _lineStart = null;
            if (a.X == v.X && a.Y == v.Y) { _status = "Vazgeçildi."; RefreshOverlay(); return; }
            string style = kind == EdgeKind.Fence ? Style(BuildPartKind.Fence)
                : kind == EdgeKind.Railing ? Style(BuildPartKind.Railing)
                : Style(BuildPartKind.WallMaterial);
            var r = _svc.SetEdgeLine(a.X, a.Y, v.X, v.Y, v.Z, kind, style, out int built);
            _status = r.Ok ? $"{built} parça kuruldu." : r.Reason;
            RefreshOverlay();
        }

        // Oda taşı/kopyala: önce odaya, sonra odanın sol-üst köşesinin gideceği karoya dokun.
        private void RoomPickTap(GridPos tile)
        {
            if (!_roomPick.HasValue)
            {
                var room = _world.Map.Rooms.RoomAt(tile);
                if (room == null || !room.IsIndoor) { _status = "Bir odanın içine dokun."; return; }
                _roomPick = tile;
                _status = "Odanın sol-üst köşesinin gideceği karoya dokun.";
                RefreshOverlay();
                return;
            }
            var src = _world.Map.Rooms.RoomAt(_roomPick.Value);
            _roomPick = null;
            if (src == null || !src.IsIndoor) { _status = "Oda artık yok."; RefreshOverlay(); return; }
            var anchor = RoomAnchor(src);
            int dx = tile.X - anchor.X, dy = tile.Y - anchor.Y;
            if (_tool == Tool.MoveRoom)
            {
                var r = _svc.MoveRoom(src, dx, dy);
                _status = r.Ok ? "Oda taşındı." : r.Reason;
            }
            else
            {
                var r = _svc.CopyRoom(src, dx, dy, out int skipped);
                _status = r.Ok ? (skipped > 0 ? $"Oda kopyalandı ({skipped} eşya kopyalanmadı: bitki, merdiven, barınak)." : "Oda kopyalandı.") : r.Reason;
            }
            RefreshOverlay();
        }

        private static GridPos RoomAnchor(Room room)
        {
            int x = int.MaxValue, y = int.MaxValue;
            foreach (var t in room.Tiles) { if (t.X < x) x = t.X; if (t.Y < y) y = t.Y; }
            return new GridPos(x, y, room.Level);
        }

        // Balyoz: dikdörtgenin iki köşe karosu.
        private void DemolishTap(GridPos tile)
        {
            if (!_roomCorner.HasValue)
            {
                _roomCorner = tile;
                _status = "Yıkılacak alanın karşı köşesine dokun.";
                RefreshOverlay();
                return;
            }
            var a = _roomCorner.Value;
            _roomCorner = null;
            var r = _svc.Demolish(a.X, a.Y, tile.X, tile.Y, tile.Z, out int removed, out int skipped);
            _status = r.Ok ? $"Yıkıldı: {removed} parça{(skipped > 0 ? $", {skipped} kaldırılamadı" : "")}. Geri al ile döner." : r.Reason;
            RefreshOverlay();
        }

        // Damlalık: dokunulan duvarın, eşyanın ya da zeminin aracını ve stilini seçer (Sims gibi).
        private void EyedropperTap(Vector3 world, GridPos tile)
        {
            var map = _world.Map;
            var item = PickItem(world);
            if (item != null && !item.IsPlant && !BuildService.IsLocked(item) && item.Def.Price > 0)
            {
                var def = item.Def;
                string finish = item.FinishId;
                if (_house.Count(def.Id) <= 0)
                {
                    var buy = _svc.Buy(def.Id);
                    if (!buy.Ok) { _status = buy.Reason; return; }
                }
                _tool = Tool.Items;
                StartPlacing(def.Id);
                _pendingFinish = finish;
                _status = $"{def.DisplayName}: aynısını koy{(finish != null ? " (aynı kaplamayla)" : "")}.";
                return;
            }
            var e = NearestEdge(world);
            var kind = map.EdgeInBounds(e) ? map.GetEdge(e) : EdgeKind.None;
            if (kind != EdgeKind.None)
            {
                var look = map.GetLook(e);
                var pk = BuildService.PartKindFor(kind);
                if (pk.HasValue) _style[pk.Value] = look?.Style;
                string finish = look?.FinishB != null && e.SideB == tile ? look.FinishB : look?.FinishA;
                if (finish != null) _style[BuildPartKind.WallFinish] = finish;
                _tool = kind == EdgeKind.Door ? Tool.Door : kind == EdgeKind.Window ? Tool.Window : kind == EdgeKind.HalfWall ? Tool.HalfWall
                    : kind == EdgeKind.Fence ? Tool.Fence : kind == EdgeKind.Gate ? Tool.Gate : kind == EdgeKind.Railing ? Tool.Railing : Tool.Wall;
                ClearSelection();
                _status = "Stil alındı.";
                return;
            }
            var floor = map.GetFloor(tile);
            if (floor != FloorType.Grass)
            {
                _style[BuildPartKind.Floor] = map.GetFloorStyle(tile) ?? BuildParts.DefaultFloor(_content, floor)?.Id;
                _tool = Tool.Floor;
                ClearSelection();
                _status = "Zemin alındı.";
                return;
            }
            _status = "Alınacak bir şey yok: duvara, eşyaya ya da döşeli zemine dokun.";
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
            int z = tile.Z;
            int x0 = Mathf.Min(a.X, tile.X), x1 = Mathf.Max(a.X, tile.X);
            int y0 = Mathf.Min(a.Y, tile.Y), y1 = Mathf.Max(a.Y, tile.Y);
            string fail = null; int built = 0;
            // Seçili zemin stili (yoksa katın varsayılanı) ve seçili duvar malzemesi kullanılır.
            var floorPart = Style(BuildPartKind.Floor) != null && _content.Parts.TryGet(Style(BuildPartKind.Floor), out var fp) ? fp : null;
            var wanted = floorPart != null ? floorPart.Base : z < 0 ? FloorType.Dirt : FloorType.Wood;
            string wantedStyle = floorPart?.Id;
            _svc.BeginGroup("Oda");     // bütün oda tek geri alma adımı
            try
            {
                for (int x = x0; x <= x1 && fail == null; x++)
                    for (int y = y0; y <= y1 && fail == null; y++)
                    {
                        var p = new GridPos(x, y, z);
                        // Üst katta ve bodrumda önce döşeme/kazı gerekir (zemin katta tile her zaman vardır).
                        if (z != 0 && !_world.Map.HasTile(p))
                        {
                            var lr = _svc.BuildLevelTile(p);
                            if (!lr.Ok) { fail = lr.Reason; break; }
                        }
                        if (_world.Map.GetFloor(p) == wanted && (wantedStyle == null || _world.Map.GetFloorStyle(p) == wantedStyle
                            || floorPart.IsDefault && _world.Map.GetFloorStyle(p) == null)) continue;     // zaten döşeli
                        var r = _svc.PaintFloor(p, wanted, wantedStyle);
                        if (!r.Ok) fail = r.Reason;
                    }
                for (int x = x0; x <= x1 && fail == null; x++)
                {
                    fail = TryWall(new Edge(x, y0, EdgeAxis.Horizontal, z), ref built) ?? TryWall(new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), ref built);
                }
                for (int y = y0; y <= y1 && fail == null; y++)
                {
                    fail = TryWall(new Edge(x0, y, EdgeAxis.Vertical, z), ref built) ?? TryWall(new Edge(x1 + 1, y, EdgeAxis.Vertical, z), ref built);
                }
            }
            finally
            {
                _svc.EndGroup();
            }
            _status = fail != null ? $"Oda yarım kaldı: {fail}" : $"Oda kuruldu ({built} duvar). Kapı için Kapı aracını duvara kullan.";
            RefreshOverlay();
        }

        private string TryWall(Edge e, ref int built)
        {
            if (_world.Map.GetEdge(e) != EdgeKind.None) return null;
            var r = _svc.SetEdge(e, EdgeKind.Wall, Style(BuildPartKind.WallMaterial));
            if (r.Ok) built++;
            return r.Ok ? null : r.Reason;
        }

        private string RoomInfo(GridPos tile)
        {
            var room = _world.Map.Rooms.RoomAt(tile);
            if (room == null) return "";
            if (!room.IsIndoor) return _world.Map.IsOwned(tile) ? "Dış mekân (senin arsan)." : "Dış mekân (sahipsiz arsa).";
            var rc = Comfort?.ComfortOf(room);
            string comfort = rc != null ? $" Konfor {rc.Total}: {rc.Breakdown()}." : "";
            return $"Oda: {room.Area} karo, {room.DoorCount} kapı, {room.WindowCount} pencere.{comfort}";
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
            // Aşırı yük engellenmez ama raf çökebilir (karar 18.11.2026): bir kez onay istenir.
            var def = _content.Items.Get(_placeId);
            var surfaceNow = _world.GetItem(_surfaceId);
            if (surfaceNow != null && _world.WouldOverload(def, _surfaceId) && _world.CanPlaceOnSurface(def, _surfaceId, slotId).Ok && _confirmSlot != slotId)
            {
                _confirmSlot = slotId;
                int load = _world.LoadOf(_surfaceId) + def.Weight;
                _status = $"{surfaceNow.Def.DisplayName} bu yükü taşımaz ({load}/{surfaceNow.Def.MaxLoad}). Çökebilir. Yine de koymak için yuvaya tekrar dokun.";
                RefreshOverlay();
                return;
            }
            _confirmSlot = null;
            var r = _svc.PlaceDecorFromInventory(_placeId, _surfaceId, slotId, out _);
            _status = r.Ok ? (surfaceNow != null && _world.IsOverloaded(surfaceNow) ? "Yerleştirildi. Dikkat: raf aşırı yüklü, çökebilir." : "Yerleştirildi.") : r.Reason;
            if (r.Ok && _house.Count(_placeId) <= 0) { _placeId = null; _surfaceId = 0; ClearSelectionKeepStatus(); }
            RefreshOverlay();
        }

        private void ClearSelectionKeepStatus() { var s = _status; ClearSelection(); _status = s; }

        private bool IsSurfaceDef(string id) => _content.Items.Get(id).Placement == ItemPlacement.Surface;
        private bool IsWallDef(string id) => _content.Items.Get(id).Placement == ItemPlacement.Wall;

        private Vector3 SlotWorld(ItemInstance surface, string slotId)
        {
            surface.TryGetSlotPosition(slotId, out float gx, out float gy);
            if (surface.IsWallMounted) return Iso.GridToWorld(gx, gy, surface.Position.Z) + new Vector3(0f, Iso.BandHeight(surface.WallBand) + 0.1f, 0f); // duvar rafı
            if (surface.IsCeiling) return _view.TileToWorld(surface.Position) + new Vector3(0f, Iso.CeilingHeight - 0.35f, 0f); // kiriş kancası
            return Iso.GridToWorld(gx, gy, surface.Position.Z) + new Vector3(0f, 24f / 32f, 0f); // masa üstü yüksekliği
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
                    if (room.Level != _view.ViewLevel) continue;
                    var c = Color.HSVToRGB((room.Id * 0.17f) % 1f, 0.6f, 1f);
                    c.a = 0.3f;
                    foreach (var t in room.Tiles) Diamond(t, c, -8000, 1f);
                }
            }
            if (_roomCorner.HasValue) Diamond(_roomCorner.Value, new Color(0.3f, 0.7f, 1f, 0.7f), 6000, 1f);
            if (_lineStart.HasValue)
                DiamondAt(Iso.GridToWorld(_lineStart.Value.X, _lineStart.Value.Y, _lineStart.Value.Z), new Color(0.3f, 0.7f, 1f, 0.9f), 6001, 0.35f);
            if (_roomPick.HasValue)
            {
                var picked = _world.Map.Rooms.RoomAt(_roomPick.Value);
                if (picked != null) foreach (var t in picked.Tiles) Diamond(t, new Color(0.4f, 0.8f, 1f, 0.35f), 6000, 1f);
            }
            if (_pushEdge.HasValue)
            {
                var pe = _pushEdge.Value;
                var mid = Iso.GridToWorld(pe.X + (pe.Axis == EdgeAxis.Horizontal ? 0.5f : 0f), pe.Y + (pe.Axis == EdgeAxis.Vertical ? 0.5f : 0f), pe.Z);
                DiamondAt(mid, new Color(1f, 0.6f, 0.2f, 0.9f), 6001, 0.4f);
            }

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
                var check = !ignore.HasValue ? _svc.CanPlaceFromInventory(ghostDef.Id, _ghostPos, _rot)
                    : ghostDef.IsRug ? _world.CanPlaceRug(ghostDef, _ghostPos, _rot, ignore)
                    : _world.CanPlaceFloor(ghostDef, _ghostPos, _rot, ignore);
                var col = check.Ok ? new Color(0.3f, 1f, 0.4f, 0.6f) : new Color(1f, 0.3f, 0.3f, 0.6f);
                var (w, h) = Footprint.Dimensions(ghostDef.Width, ghostDef.Height, _rot);
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                        Diamond(new GridPos(_ghostPos.X + dx, _ghostPos.Y + dy, _ghostPos.Z), col, 6000, 1f);
                if (ghostDef.IsStairs)
                {
                    // Merdivenin öbür ucu (diğer katta) küçük bir işaretle gösterilir.
                    WorldState.StairsEnds(ghostDef, _ghostPos, _rot, out _, out var landing);
                    DiamondAt(_view.TileToWorld(landing), new Color(0.4f, 0.8f, 1f, 0.8f), 6001, 0.5f);
                }
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
            sr.sortingOrder = order + _view.ViewLevel * Iso.LevelSortStep;   // işaretler görünen katın bandında çizilir
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
            if (_pendingLevel.HasValue && Event.current.type == EventType.Layout)
            {
                int lv = _pendingLevel.Value;
                _pendingLevel = null;
                _view.ViewLevel = lv;
                _ghostPos = new GridPos(_ghostPos.X, _ghostPos.Y, lv);
                _roomCorner = null;
                if (_focused) { _input.RestoreView(); _focused = false; }
                _selectedId = 0; _surfaceId = 0; _moving = false;
                RefreshOverlay();
                _status = lv > 0 ? "Üst kat: boş düzleme Kat aracıyla döşeme yap (altı duvarlı oda olmalı)."
                    : lv < 0 ? "Bodrum: merdivenle başla, sonra Kat aracıyla yanını kaz."
                    : "Zemin kat.";
            }
            if (_pendingTool.HasValue && Event.current.type == EventType.Layout)
            {
                _tool = _pendingTool.Value;
                _pendingTool = null;
                ClearSelection();
                _status = "";
            }
            if (_pendingConduit.HasValue && Event.current.type == EventType.Layout)
            {
                _conduitType = _pendingConduit.Value;
                _pendingConduit = null;
                _conduitStart = null;
                _status = "";
            }
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"YAPI MODU · Akçe: {_house.Akce}");
            if (GUILayout.Button("Kapat", GUILayout.Width(60f))) { Exit(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();
            if (_status.Length > 0) GUILayout.Label(_status, _wrap);
            // (Girdi sayıları hata ayıklama satırı menu sadeleştirmesiyle kaldırıldı.)

            if (Event.current.type == EventType.Layout)
            {
                // Araç seçiliyse İnşa sekmesi; Eşya aracına dönülünce (ör. damlalık) Eşya sekmesi.
                if (_tool != Tool.Items) _mode = 0;
                else if (_mode == 0) _mode = 1;
                _activeSearch = _search ?? "";
            }
            DrawHeader();
            DrawModeTabs();
            if (_mode == 0) { DrawBuildGroups(); DrawToolContext(); GUILayout.EndArea(); return; }

            DrawContext();

            // Dikey kaydırma çubuğu her zaman ayrılır, içerik sabit genişlikte: sağdan taşma/kesilme olmasın.
            _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
            GUILayout.BeginVertical(GUILayout.Width(236f));
            if (_mode == 2) DrawInventory(); else DrawShop();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // Üst şerit (her sekmede): kat, duvar görünümü, hızlı araçlar.
        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            LevelButton("Kazı", -2); LevelButton("Bodrum", -1); LevelButton("Zemin", 0); LevelButton("Üst kat", 1);
            GUILayout.EndHorizontal();
            if (_walls != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(_walls.Mode == WallMode.Up, UiSkin.C("Yukarı", "top_walls_up"), GUI.skin.button, GUILayout.Width(HeaderW))) _walls.Mode = WallMode.Up;
                if (GUILayout.Toggle(_walls.Mode == WallMode.Cutaway, UiSkin.C("Kesit", "top_walls_cut"), GUI.skin.button, GUILayout.Width(HeaderW))) _walls.Mode = WallMode.Cutaway;
                if (GUILayout.Toggle(_walls.Mode == WallMode.Down, UiSkin.C("Aşağı", "top_walls_down"), GUI.skin.button, GUILayout.Width(HeaderW))) _walls.Mode = WallMode.Down;
                if (GUILayout.Toggle(_walls.ShowRoof, UiSkin.C("Çatı", "top_roof"), GUI.skin.button, GUILayout.Width(HeaderW)) != _walls.ShowRoof) _walls.ShowRoof = !_walls.ShowRoof;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            _headerRow = true; ToolButton("Damlalık", Tool.Eyedropper); ToolButton("Balyoz", Tool.Demolish); _headerRow = false;
            if (GUILayout.Toggle(_showRooms, UiSkin.C("Odalar", "top_rooms"), GUI.skin.button, GUILayout.Width(HeaderW)) != _showRooms) { _showRooms = !_showRooms; RefreshOverlay(); }
            // Düğme her karede çizilir (koşullu denetim IMGUI yerleşimini bozar); boş yığında pasif.
            GUI.enabled = _svc.CanUndo;
            if (GUILayout.Button(new GUIContent("Geri al", UiSkin.Icon("top_undo"), _svc.CanUndo ? _svc.UndoLabel : null), GUILayout.Width(HeaderW))) _deferred = () => { var u = _svc.Undo(); _status = u.Ok ? "Geri alındı." : u.Reason; RefreshOverlay(); };
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        // Üç ana sekme (Sims/AC gibi): İnşa · Eşya · Envanter.
        private void DrawModeTabs()
        {
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            ModeTab("İNŞA", "tab_build", 0); ModeTab("EŞYA", "tab_items", 1); ModeTab("ENVANTER", "tab_inventory", 2);
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        private void ModeTab(string label, string icon, int mode)
        {
            if (!GUILayout.Toggle(_mode == mode, UiSkin.C(label, icon), GUI.skin.button, GUILayout.Height(30f)) || _mode == mode) return;
            _deferred = () =>
            {
                MarkSeen();
                _mode = mode;
                if (mode == 0) SelectGroup(_buildGroup); else _tool = Tool.Items;
                ClearSelection();
                _status = "";
                _scroll = Vector2.zero;
            };
        }

        // İnşa araçları iki seviye: grup, sonra araç (en çok 2 seviye kuralı).
        private static readonly (string name, (string label, Tool tool)[] tools)[] BuildGroups =
        {
            ("Duvar", new[] { ("Oda", Tool.Room), ("Duvar", Tool.Wall), ("Yarım duvar", Tool.HalfWall), ("Kapı", Tool.Door),
                ("Pencere", Tool.Window), ("Kaldır", Tool.EraseEdge), ("Duvar it", Tool.PushWall), ("Oda taşı", Tool.MoveRoom), ("Oda kopyala", Tool.CopyRoom) }),
            ("Zemin", new[] { ("Zemin", Tool.Floor), ("Oda zemini", Tool.FloorRoom), ("Duvar kapl.", Tool.Finish), ("Oda kapl.", Tool.FinishRoom),
                ("Çatı", Tool.Roof), ("Çim", Tool.FloorErase) }),
            ("Bahçe", new[] { ("Çit", Tool.Fence), ("Çit kapısı", Tool.Gate), ("Korkuluk", Tool.Railing), ("Gölet kaz", Tool.Pond),
                ("Patika", Tool.RoadPath), ("Çakıl yol", Tool.RoadGravel), ("Kaldırım", Tool.RoadPaved), ("Yol sil", Tool.RoadErase) }),
            ("Su", new (string, Tool)[0]),
            ("Arsa", new[] { ("Döşeme", Tool.LevelBuild), ("Döşeme sök", Tool.LevelRemove), ("Arsa al", Tool.Plot),
                ("Depo", Tool.ZoneStorage), ("Otlak", Tool.ZonePasture), ("Bölge sil", Tool.ZoneErase) }),
        };
        private static readonly string[] BuildGroupNames = BuildGroups.Select(g => g.name).ToArray();
        private static readonly string[] BuildGroupIcons = { "bgroup_wall", "bgroup_floor", "bgroup_garden", "bgroup_water", "bgroup_plot" };
        private static readonly string[] ShopGroupIcons = { "sgroup_furniture", "sgroup_household", "sgroup_decor", "sgroup_outdoor" };

        private void SelectGroup(int g)
        {
            _buildGroup = g;
            var tools = BuildGroups[g].tools;
            if (tools.Length > 0) _tool = tools[0].tool;
            else { _tool = Tool.Conduit; _conduitType = ConduitType.Ditch; _conduitStart = null; }
        }

        private void DrawBuildGroups()
        {
            var gc = new GUIContent[BuildGroupNames.Length];
            for (int i = 0; i < gc.Length; i++) gc[i] = UiSkin.C(BuildGroupNames[i], BuildGroupIcons[i]);
            int g = GUILayout.Toolbar(_buildGroup, gc, GUILayout.Height(26f), GUILayout.Width(246f));
            if (g != _buildGroup) { int gg = g; _deferred = () => { SelectGroup(gg); ClearSelection(); _status = ""; }; }
            GUILayout.Space(4f);
            var tools = BuildGroups[_buildGroup].tools;
            if (tools.Length == 0)
            {
                GUILayout.BeginHorizontal();
                ConduitButton("Ark", ConduitType.Ditch); ConduitButton("Taş ark", ConduitType.StoneChannel); ConduitButton("Oluk", ConduitType.Flume);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                ConduitButton("Kütük boru", ConduitType.WoodPipe); ConduitButton("Künk", ConduitType.ClayPipe); ConduitButton("Kurşun", ConduitType.LeadPipe); ConduitButton("Sil", ConduitType.None);
                GUILayout.EndHorizontal();
            }
            else
            {
                for (int i = 0; i < tools.Length; i += 3)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = i; j < i + 3 && j < tools.Length; j++) ToolButton(ToolLabel(tools[j].label, tools[j].tool), tools[j].tool);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Label(ToolHelp(), _wrap);
        }

        private string ToolLabel(string label, Tool tool) =>
            tool == Tool.LevelBuild ? (_view.ViewLevel < 0 ? "Kaz" : "Döşeme")
            : tool == Tool.LevelRemove ? (_view.ViewLevel < 0 ? "Doldur" : "Döşeme sök")
            : label;

        private void ToolRow(params (string label, Tool tool)[] tools)
        {
            GUILayout.BeginHorizontal();
            foreach (var t in tools) ToolButton(t.label, t.tool);
            GUILayout.EndHorizontal();
        }

        private string ToolHelp()
        {
            switch (_tool)
            {
                case Tool.Room: return "İki köşe karosuna dokun: seçili malzemeyle duvar, seçili zeminle döşeme.";
                case Tool.Wall:
                case Tool.HalfWall:
                case Tool.Fence:
                case Tool.Railing: return "Bir köşeye, sonra hattın öbür ucuna dokun (sürükler gibi). Yarım duvar, çit ve korkuluk odayı bölmez ama geçilmez.";
                case Tool.Door:
                case Tool.Window: return "Bir duvara dokun: duvar seçili stille kapı/pencereye döner.";
                case Tool.Gate: return "Çit hattına ya da boş bir kenara dokun.";
                case Tool.EraseEdge: return "Kaldırılacak duvar, kapı, pencere ya da çite dokun (yarısı iade).";
                case Tool.Finish: return "Duvarın boyanacak yüzünün önündeki karoya, duvara yakın dokun.";
                case Tool.FinishRoom: return "Bir odanın içine dokun: bütün iç yüzler kaplanır.";
                case Tool.Floor: return "Karoya dokun: seçili zemin döşenir.";
                case Tool.FloorRoom: return "Bir odanın içine dokun: bütün zemini döşenir.";
                case Tool.FloorErase: return "Karonun döşemesini kaldırır (çim; yarısı iade).";
                case Tool.Roof: return "Çatı, üstü açık iç odalara kendiliğinden kurulur. Bir evin içine dokun: çatı stili ya da çatısız. Görmek için Duvar: Yukarı ve Çatı açık.";
                case Tool.PushWall: return "Bir odanın duvarına dokun, sonra oklarla it/çek: oda büyür ya da küçülür.";
                case Tool.MoveRoom: return "Odaya, sonra odanın sol-üst köşesinin gideceği karoya dokun. Eşyalarıyla taşınır; ücretsiz.";
                case Tool.CopyRoom: return "Odaya, sonra kopyanın sol-üst köşesine dokun. Duvar, zemin, kaplama ve eşyalar yeniden alınır.";
                case Tool.Eyedropper: return "Bir duvara, eşyaya ya da zemine dokun: aynısını koymak için aracı ve stili seçer.";
                case Tool.Demolish: return "Yıkılacak alanın iki köşesine dokun: eşyalar satılır, duvar ve zemin kalkar. Geri al ile döner.";
                case Tool.Plot: return $"Bitişik 4x4 parçaya dokun: {BuildService.ChunkPrice} akçe.";
                case Tool.LevelBuild: return _view.ViewLevel > 0 ? $"Üst kat döşemesi: karo başına {BuildService.SlabPrice} akçe; altında duvarlı oda ya da duvar gerekir (bir karo çıkma: balkon). Merdiven üstüne döşeme olmaz."
                    : _view.ViewLevel <= -2 ? "Bodrumun altı (kazı) yapı modunda akçeyle kazılmaz: bodruma aşağı inen merdiven koy (Madencilik 2), sonra canlı modda kayaya dokunup kaz."
                    : _view.ViewLevel < 0 ? $"Bodrum kazısı: karo başına {BuildService.DigPrice} akçe; mevcut bodrumun yanını kaz (ilk karo merdivenle açılır)."
                    : "Zemin kat zaten var; Kat: Üst kat ya da Bodrum seç.";
                case Tool.LevelRemove: return "Döşemeyi sök / bodrum karosunu doldur: üstünde eşya, çevresinde duvar ya da merdiven olmamalı. Yarısı iade.";
                case Tool.RoadPath:
                case Tool.RoadGravel:
                case Tool.RoadPaved:
                case Tool.RoadErase:
                    return $"Yol (arsanda, açık havada): patika x{GridMap.PathSpeed:0.#} hız {BuildService.RoadPrice(RoadType.Path)} akçe, çakıl x{GridMap.GravelSpeed:0.#} {BuildService.RoadPrice(RoadType.Gravel)} akçe, kaldırım x{GridMap.PavedSpeed:0.#} {BuildService.RoadPrice(RoadType.Paved)} akçe. Üstünden geçildikçe aşınır (inek, at çok); aynı yolu yeniden döşemek yarı fiyatına onarır. Hayvanlar barınaktan otlağa yoldan gider.";
                case Tool.Conduit:
                    return _conduitType == ConduitType.None
                        ? "Su yolu sil: hattın iki ucuna dokun. Boru envantere döner, ark kazısının yarısı iade."
                        : $"{Conduits.Describe(_conduitType)}. Karo başına {Conduits.Price(_conduitType)} akçe"
                          + (Conduits.Material(_conduitType) != null ? $" (envanterde {_content.Items.Get(Conduits.Material(_conduitType)).DisplayName} varsa o harcanır: {_house.Count(Conduits.Material(_conduitType))})" : "")
                          + ". Gölete, dereye, sarnıca ya da sulama dolabına değen hat su taşır; kuyu boruya su vermez. Kapak arkın üstüne konur. Tesisat (küvet, leğen, hela, çeşme) borunun üstüne ya da yanına konur; üst kat için kurşun boru ve su terazisi gerekir.";
                case Tool.Pond: return $"Gölet kaz (açık havada, karo başına {BuildService.FloorPrice(FloorType.Water)} akçe): çukur boş kalır, yalnızca yağmurla dolar; yazın yağmursuz günlerde buharlaşır. Dolunca içine girilip yıkanılır, kova doldurulur, değirmen ve sulama ona bağlanır; kışın donar. Birkaç gün dolu kalınca böcekler gelir. Balık için aynı türden en az iki balık at: böcek varken ürerler. Çim aracı geri doldurur.";
                case Tool.ZonePasture: return "Otlak: hayvanlar gündüz çimde otlar (yem yerine ot, kışın ot büyümez). Çitle çevir; barınağın kapısı otlağa açılsın. Çan ile içeri çağrılır.";
                case Tool.ZoneStorage:
                case Tool.ZoneErase: return $"Depo bölgesi: karo başına +{StorageService.UnitsPerZoneTile} kaynak kapasitesi (ücretsiz). Sandık +40.";
                default: return "";
            }
        }

        // Araca göre seçilebilen parça türü (yoksa null).
        private static BuildPartKind? KindFor(Tool tool)
        {
            switch (tool)
            {
                case Tool.Room:
                case Tool.Wall:
                case Tool.HalfWall: return BuildPartKind.WallMaterial;
                case Tool.Door: return BuildPartKind.Door;
                case Tool.Window: return BuildPartKind.Window;
                case Tool.Fence: return BuildPartKind.Fence;
                case Tool.Gate: return BuildPartKind.Gate;
                case Tool.Railing: return BuildPartKind.Railing;
                case Tool.Finish:
                case Tool.FinishRoom: return BuildPartKind.WallFinish;
                case Tool.Floor:
                case Tool.FloorRoom: return BuildPartKind.Floor;
                case Tool.Roof: return BuildPartKind.Roof;
                default: return null;
            }
        }

        // Yapı araçlarının bağlam paneli: stil/malzeme seçici, duvar itme okları.
        private void DrawToolContext()
        {
            if (_tool == Tool.PushWall && _pushEdge.HasValue)
            {
                var e = _pushEdge.Value;
                bool horiz = e.Axis == EdgeAxis.Horizontal;
                GUILayout.BeginHorizontal();
                // Ekranda +x sağ-yukarı (↗), +y sol-yukarı (↖).
                if (GUILayout.Button(horiz ? "↖ it" : "↗ it")) Push(+1);
                if (GUILayout.Button(horiz ? "↘ it" : "↙ it")) Push(-1);
                if (GUILayout.Button("Bitir")) { _pushEdge = null; RefreshOverlay(); }
                GUILayout.EndHorizontal();
            }
            if ((_tool == Tool.MoveRoom || _tool == Tool.CopyRoom || _tool == Tool.Demolish) && (_roomPick.HasValue || _roomCorner.HasValue))
                if (GUILayout.Button("Vazgeç")) { _roomPick = null; _roomCorner = null; RefreshOverlay(); }
            if (_lineStart.HasValue && GUILayout.Button("Çizgiden vazgeç")) { _lineStart = null; RefreshOverlay(); }

            var kind = KindFor(_tool);
            if (!kind.HasValue) return;
            int level = _view.ViewLevel;
            var parts = BuildParts.OfKind(_content, kind.Value, kind.Value == BuildPartKind.Window ? level : int.MinValue);
            if (kind.Value == BuildPartKind.Floor) parts.Sort((a, b) => a.Base != b.Base ? a.Base.CompareTo(b.Base) : a.Price.CompareTo(b.Price));
            string current = Style(kind.Value);
            if (current == null || current != NoRoofChoice && !_content.Parts.Contains(current)) current = null;
            _partScroll = GUILayout.BeginScrollView(_partScroll);
            if (kind.Value == BuildPartKind.WallFinish && PartToggle(current == null, "Çıplak (kaplamayı kaldır)", null)) _style[kind.Value] = null;
            int accountLevel = _svc.AccountLevel != null ? _svc.AccountLevel() : int.MaxValue;
            int nextPart = int.MaxValue;   // yalnızca sıradaki kilit kademesi görünür (29-menu-yapisi)
            foreach (var p in parts) if (p.UnlockLevel > accountLevel && p.UnlockLevel < nextPart) nextPart = p.UnlockLevel;
            foreach (var p in parts)
            {
                if (p.UnlockLevel > nextPart) continue;
                bool selected = current == p.Id || current == null && p.IsDefault && kind.Value != BuildPartKind.WallFinish;
                bool locked = accountLevel < p.UnlockLevel;
                string price = kind.Value == BuildPartKind.Roof ? (p.Price > 0 ? $"{p.Price}/karo" : "ücretsiz")
                    : kind.Value == BuildPartKind.Floor ? $"{p.Price}/karo · {FloorBaseName(p.Base)}"
                    : _tool == Tool.HalfWall ? $"{System.Math.Max(1, (p.Price + 1) / 2)}"
                    : $"{p.Price}";
                string label = locked ? $"{p.DisplayName} · Sv {p.UnlockLevel} gerekli" : $"{p.DisplayName} · {price}{(p.Comfort > 0 ? $" · konfor +{p.Comfort}" : "")}";
                GUI.enabled = !locked;
                if (PartToggle(selected, label, p)) _style[kind.Value] = p.Id;
                GUI.enabled = true;
            }
            if (kind.Value == BuildPartKind.Roof && PartToggle(current == NoRoofChoice, "Çatısız (avlu, çardak)", null)) _style[kind.Value] = NoRoofChoice;
            GUILayout.EndScrollView();
        }

        private static string FloorBaseName(FloorType f) => f == FloorType.Wood ? "ahşap" : f == FloorType.Stone ? "taş" : "toprak";

        // Renk örnekli seçim satırı; tıklanınca true.
        private bool PartToggle(bool selected, string label, BuildPartDef part)
        {
            GUILayout.BeginHorizontal();
            var old = GUI.color;
            GUI.color = SpriteMaterials.PartColor(part, Color.white);
            GUILayout.Box("", GUILayout.Width(16f), GUILayout.Height(16f));
            GUI.color = old;
            bool clicked = GUILayout.Toggle(selected, label, GUI.skin.button) && !selected;
            GUILayout.EndHorizontal();
            return clicked;
        }

        private void Push(int delta)
        {
            if (!_pushEdge.HasValue) return;
            var e = _pushEdge.Value;
            var r = _svc.PushWall(e, delta);
            _status = r.Ok ? "Duvar itildi." : r.Reason;
            if (r.Ok) _pushEdge = e.Axis == EdgeAxis.Horizontal ? new Edge(e.X, e.Y + delta, e.Axis, e.Z) : new Edge(e.X + delta, e.Y, e.Axis, e.Z);
            RefreshOverlay();
        }

        private const float HeaderW = 60f;   // üst şeritte dörtlü satır: ikonla birlikte taşmasın
        private bool _headerRow;

        private void LevelButton(string label, int level)
        {
            // Kat değişimi de bir sonraki Layout olayında uygulanır (çizim ve işaretler yeniden kurulur).
            string icon = level == -1 ? "top_level_basement" : level == 0 ? "top_level_ground" : level == 1 ? "top_level_upper" : null;
            if (GUILayout.Toggle(_view.ViewLevel == level, UiSkin.C(label, icon), GUI.skin.button, GUILayout.Width(HeaderW)) && _view.ViewLevel != level) _pendingLevel = level;
        }

        private void ConduitButton(string label, ConduitType type)
        {
            bool on = _tool == Tool.Conduit && _conduitType == type;
            if (GUILayout.Toggle(on, UiSkin.C(label, ConduitIcon(type)), GUI.skin.button) && !on)
            {
                if (_tool != Tool.Conduit) _pendingTool = Tool.Conduit;
                _pendingConduit = type;
            }
        }

        private static string[] ConduitIcon(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.Ditch: return new[] { "tool_conduit_ditch" };
                case ConduitType.StoneChannel: return new[] { "tool_conduit_stone" };
                case ConduitType.Flume: return new[] { "tool_conduit_flume" };
                case ConduitType.WoodPipe: return new[] { "tool_conduit_wood", "tool_conduit_pipe_wood" };
                case ConduitType.ClayPipe: return new[] { "tool_conduit_clay", "tool_conduit_pipe_clay" };
                case ConduitType.LeadPipe: return new[] { "tool_conduit_lead", "tool_conduit_pipe_lead" };
                default: return new[] { "tool_conduit_erase" };
            }
        }

        // Araç → ikon anahtarı (ui_icon_<anahtar>, 392-394 ve 399a/b).
        private static string ToolIcon(Tool t)
        {
            switch (t)
            {
                case Tool.Room: return "tool_room"; case Tool.Wall: return "tool_wall"; case Tool.HalfWall: return "tool_halfwall";
                case Tool.Door: return "tool_door"; case Tool.Window: return "tool_window"; case Tool.EraseEdge: return "tool_erase_edge";
                case Tool.Fence: return "tool_fence"; case Tool.Gate: return "tool_gate"; case Tool.Railing: return "tool_railing";
                case Tool.Finish: return "tool_finish"; case Tool.FinishRoom: return "tool_finish_room"; case Tool.Floor: return "tool_floor";
                case Tool.FloorRoom: return "tool_floor_room"; case Tool.FloorErase: return "tool_grass"; case Tool.Roof: return "tool_roof";
                case Tool.PushWall: return "tool_push_wall"; case Tool.MoveRoom: return "tool_move_room"; case Tool.CopyRoom: return "tool_copy_room";
                case Tool.Eyedropper: return "top_eyedropper"; case Tool.Demolish: return "top_sledgehammer"; case Tool.Plot: return "tool_plot";
                case Tool.ZoneStorage: return "tool_zone_storage"; case Tool.ZoneErase: return "tool_zone_erase"; case Tool.ZonePasture: return "tool_zone_pasture";
                case Tool.LevelBuild: return "tool_level_build"; case Tool.LevelRemove: return "tool_level_remove";
                case Tool.RoadPath: return "tool_road_path"; case Tool.RoadGravel: return "tool_road_gravel"; case Tool.RoadPaved: return "tool_road_paved";
                case Tool.RoadErase: return "tool_road_erase"; case Tool.Pond: return "tool_pond";
                default: return null;
            }
        }

        private void ToolButton(string label, Tool tool)
        {
            // Arayüz yerleşimi bozulmasın diye araç değişimi bir sonraki Layout olayında uygulanır.
            if (GUILayout.Toggle(_tool == tool, UiSkin.C(label, ToolIcon(tool)), GUI.skin.button, _headerRow ? GUILayout.Width(HeaderW) : GUILayout.MinWidth(0f)) && _tool != tool) _pendingTool = tool;
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
                    // Yalnızca bu eşyanın uyduğu yüzeyler listelenir (örtü yalnızca örtü yuvası olan masaya; 27 D2).
                    int fitting = 0;
                    foreach (var it in _world.Items)
                    {
                        if (it.IsDecor || it.Def.SurfaceSlots.Count == 0 || !ItemCatalog.SurfaceAccepts(it.Def, def)) continue;
                        fitting++;
                        if (GUILayout.Button($"{it.Def.DisplayName} ({it.Position.X},{it.Position.Y})"))
                        {
                            _surfaceId = it.Id;
                            Focus(it);
                            _status = $"{it.Def.DisplayName}: bir yuvaya dokun.";
                            RefreshOverlay();
                        }
                    }
                    if (fitting == 0)
                    {
                        var where = ItemCatalog.FittingSurfaces(_content, def);
                        GUILayout.Label(where.Count > 0
                            ? $"Evde {def.DisplayName} konacak yer yok. Uyduğu eşyalar: {string.Join(", ", where)}."
                            : $"Evde {def.DisplayName} konacak yer yok.", _wrap);
                    }
                    if (_surfaceId != 0)
                    {
                        var surface = _world.GetItem(_surfaceId);
                        string noFit = surface != null ? NoFitStatus(surface) : null;
                        if (noFit != null) GUILayout.Label(noFit, _wrap);
                        if (surface != null)
                        {
                            // Yuvalar: tür, boyut ve uyup uymadığı (rafa yalnızca rafa uyumlu eşya; 27-ev-dekorasyonu).
                            if (surface.Def.MaxLoad > 0) GUILayout.Label($"Yük {_world.LoadOf(surface.Id)}/{surface.Def.MaxLoad} (bu eşya {def.Weight})");
                            foreach (var slot in surface.Def.SurfaceSlots)
                            {
                                var fit = _world.CanPlaceOnSurface(def, surface.Id, slot.Id);
                                string kind = Decor.KindName(surface.Def.SlotKind(slot));
                                string label = $"Yuva {slot.Id} ({kind}, {Decor.SizeLetter(slot.Size)})" + (fit.Ok ? (_confirmSlot == slot.Id ? " · yine de koy" : "") : " ✗");
                                if (GUILayout.Button(new GUIContent(label, fit.Ok ? null : fit.Reason)))
                                {
                                    if (fit.Ok) PlaceDecor(slot.Id); else _status = fit.Reason;
                                }
                            }
                        }
                    }
                }
                else if (def.Placement == ItemPlacement.Wall)
                {
                    GUILayout.Label("Bir duvarın iç yüzüne dokun: eşya oraya asılır. (Duvarı göremiyorsan Duvar: Yukarı.)");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Bant:");
                    if (GUILayout.Toggle(_band < 0, "Kendiliğinden", GUI.skin.button)) _band = -1;
                    foreach (var b in def.EffectiveBands)
                        if (GUILayout.Toggle(_band == b, Decor.BandName(b), GUI.skin.button)) _band = b;
                    GUILayout.EndHorizontal();
                    if (def.WallWidth > 1) GUILayout.Label($"{def.WallWidth} karo düz duvar ister; dokunduğun karodan sağa/ileri uzanır.");
                }
                else if (def.IsCeiling)
                {
                    GUILayout.Label("Kapalı bir odada bir karoya dokun: eşya tavana asılır (yolu kapatmaz).");
                    if (GUILayout.Button("As")) PlaceFloor();
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
            var finishes = BuildParts.FinishesFor(_content, sel.Def);
            if (finishes.Count > 0)
            {
                GUILayout.Label("Renk / malzeme:");
                int lv = _svc.AccountLevel != null ? _svc.AccountLevel() : int.MaxValue;
                float cur = _svc.FinishFactor(sel);
                foreach (var f in finishes)
                {
                    bool selected = sel.FinishId == f.Id || sel.FinishId == null && f.IsDefault;
                    int diff = (int)System.Math.Round(sel.Def.Price * (f.PriceFactor - cur));
                    string label = lv < f.UnlockLevel ? $"{f.DisplayName} · Sv {f.UnlockLevel}" : diff > 0 ? $"{f.DisplayName} (+{diff})" : f.DisplayName;
                    GUI.enabled = lv >= f.UnlockLevel;
                    if (PartToggle(selected, label, f))
                    {
                        var id = sel.Id; var fid = f.Id;
                        _deferred = () => { var r = _svc.SetItemFinish(id, fid); _status = r.Ok ? "Kaplama değişti." : r.Reason; };
                    }
                    GUI.enabled = true;
                }
            }
            DrawDecorControls(sel);
            if (Legacy != null) DrawHeirloom(sel);
            if (sel.Def.IsHousing && _animals != null)
                GUILayout.Label($"Hayvan {_animals.Residents(sel.Id)}/{sel.Def.HousingCapacity} · yem {sel.FeedStock:0.#} · ürün {sel.StockTotal}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Taşı")) { _moving = true; _ghostPos = sel.Position; _rot = sel.Rotation; RefreshOverlay(); }
            if (GUILayout.Button("Envantere al")) { var r = _svc.StoreToInventory(sel.Id); _status = r.Ok ? "Envantere alındı." : r.Reason; if (r.Ok) ClearSelectionKeepStatus(); }
            GUILayout.EndHorizontal();
            if (sel.IsPlant && sel.Def.IsCrop)
            {
                // Ekin satılmaz ama sökülebilir (yanlış yere ekilen ya da ortada kalan ekin tarlayı kilitlemesin).
                if (GUILayout.Button("Ekini sök")) { var r = _svc.SellPlaced(sel.Id); _status = r.Ok ? "Ekin söküldü." : r.Reason; if (r.Ok) ClearSelectionKeepStatus(); }
            }
            else if (GUILayout.Button(Legacy != null && Legacy.HeirloomOf(sel.Id) != null
                ? $"Sat (+{_svc.SellValue(sel)} akçe; yadigârın geçmişi silinir)"
                : $"Sat (+{_svc.SellValue(sel)} akçe, dekorlar dahil)")) { var r = _svc.SellPlaced(sel.Id); _status = r.Ok ? "Satıldı." : r.Reason; if (r.Ok) ClearSelectionKeepStatus(); }
        }

        // Dekor denetimleri (27): duvar bandı, halı sırası, yük, durum, kararma, döküntü.
        // Seçilen yüzeyde bu dekorun türüne ve boyutuna uyan yuva yoksa nedeni ve uyduğu eşyalar; uyuyorsa null.
        private string NoFitStatus(ItemInstance surface)
        {
            if (_placeId == null || !_content.Items.TryGet(_placeId, out var def) || ItemCatalog.SurfaceAccepts(surface.Def, def)) return null;
            var where = ItemCatalog.FittingSurfaces(_content, def);
            string kinds = string.Join(", ", def.EffectiveFits.Select(Decor.KindName));
            return $"{def.DisplayName} {surface.Def.DisplayName} üstüne konmaz (yalnızca {kinds} yuvası)."
                + (where.Count > 0 ? $" Uyduğu eşyalar: {string.Join(", ", where)}." : "");
        }

        private void DrawDecorControls(ItemInstance sel)
        {
            if (sel.Condition != ItemCondition.Sound)
                GUILayout.Label(sel.Condition == ItemCondition.Damaged
                    ? "Hasarlı: yuvaları kapalı, değeri yarı. Marangoz tezgâhında, demircide ya da dokuma tezgâhında onarılır."
                    : "Onarılmış: işler, ama değeri kalıcı olarak %75.", _wrap);
            if (sel.Def.MaxLoad > 0 && sel.Def.SurfaceSlots.Count > 0)
            {
                int load = _world.LoadOf(sel.Id);
                GUILayout.Label(load > sel.Def.MaxLoad
                    ? $"Aşırı yük {load}/{sel.Def.MaxLoad}: her saat çökme şansı %{Thatchwick.Core.Build.DecorSystem.CollapseChance(load, sel.Def.MaxLoad) * 100:0}. Bir eşyayı al."
                    : $"Yük {load}/{sel.Def.MaxLoad}");
            }
            if (sel.IsTarnished) GUILayout.Label("Ayna kararmış: canlı modda \"Cilala\".");
            if (sel.Def.IsFillDecor)
            {
                // Doldurulan dekor (D2): içi gerçek ürünle, canlı modda "Doldur"; envantere alınınca içindekiler de döner.
                GUILayout.Label($"İçi: {Thatchwick.Core.Build.DecorSystem.FillLabel(sel, _content)}. Canlı modda \"Doldur\" envanterden gerçek ürün alır.", _wrap);
            }
            else if (sel.HasContents) GUILayout.Label($"Döküntü: {sel.Contents.Count} eşya. Canlı modda \"Topla\" ya da envantere al.");
            if (sel.IsWallMounted && sel.Def.EffectiveBands.Count > 1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Bant:");
                foreach (var b in sel.Def.EffectiveBands)
                    if (GUILayout.Toggle(sel.WallBand == b, Decor.BandName(b), GUI.skin.button) && sel.WallBand != b)
                    {
                        int id = sel.Id, band = b;
                        _deferred = () => { var r = _svc.SetWallBand(id, band); _status = r.Ok ? $"{Decor.BandName(band)} banda taşındı." : r.Reason; };
                    }
                GUILayout.EndHorizontal();
            }
            if (sel.IsRug && _world.RugsAt(sel.Position).Count > 1 || sel.IsRug && sel.OccupiedTiles().Any(t => _world.RugsAt(t).Count > 1))
            {
                GUILayout.BeginHorizontal();
                foreach (var (label, step) in new[] { ("En üste", 2), ("Bir üste", 1), ("Bir alta", -1), ("En alta", -2) })
                    if (GUILayout.Button(label))
                    {
                        int id = sel.Id, st = step;
                        _deferred = () => { var r = _svc.MoveRugLayer(id, st); _status = r.Ok ? "Halı sırası değişti." : r.Reason; };
                    }
                GUILayout.EndHorizontal();
            }
        }

        // Yadigâr: Soy Mührü harcanarak eşya hanedanın yadigârı olur; her nesil hız, XP, ihtiyaç doldurma ve konfor bonusu büyür.
        private void DrawHeirloom(ItemInstance sel)
        {
            var h = Legacy.HeirloomOf(sel.Id);
            if (h != null)
            {
                GUILayout.Label($"✦ {h.DisplayName}");
                GUILayout.Label($"   {h.Level}. basamak: iş ve dinlenme +%{Thatchwick.Core.Family.Legacy.BonusOf(h) * 100:0}, oda konforu +{Thatchwick.Core.Family.Legacy.ComfortOf(h)}");
                return;
            }
            if (!Thatchwick.Core.Family.Legacy.IsEligible(sel.Def)) return;
            string why = Legacy.CanMakeHeirloom(sel);
            int cost = Thatchwick.Core.Family.Legacy.CostFor(sel.Def);
            GUI.enabled = why == null;
            if (GUILayout.Button(new GUIContent($"Yadigâr yap ({cost} mühür, sende {Legacy.Seals})", why ?? "Bu eşyada iş ve dinlenme +%5, konfor +2; her nesil artar")))
            {
                Legacy.MakeHeirloom(sel, Maker != null ? Maker() : null, Today != null ? Today() : 0, out var msg);
                _status = msg;
            }
            GUI.enabled = true;
        }

        private void PlaceFloor()
        {
            var r = _svc.PlaceFromInventory(_placeId, _ghostPos, _rot, out var placed);
            _status = r.Ok ? "Yerleştirildi." : r.Reason;
            if (r.Ok && _pendingFinish != null && placed != null)
            {
                var fr = _svc.SetItemFinish(placed.Id, _pendingFinish);
                if (!fr.Ok) _status = $"Yerleştirildi; kaplama: {fr.Reason}";
            }
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
                int damaged = _house.DamagedCount(kv.Key), repaired = _house.RepairedCount(kv.Key);
                string worn = damaged + repaired == 0 ? "" : $" ({(damaged > 0 ? $"{damaged} hasarlı" : "")}{(damaged > 0 && repaired > 0 ? ", " : "")}{(repaired > 0 ? $"{repaired} onarılmış" : "")})";
                GUILayout.Label($"{def.DisplayName} x{kv.Value}{worn}");
                if (def.PlantsAs != null && GUILayout.Button("Dik", GUILayout.Width(44f)))
                {
                    ClearSelection();
                    _plantId = kv.Key;
                    _status = "Dışarıda bir karoya dokun.";
                }
                if (def.Placement != ItemPlacement.Resource && GUILayout.Button("Koy", GUILayout.Width(44f))) StartPlacing(kv.Key);
                if (def.Placement != ItemPlacement.Resource && _house.SoundCount(kv.Key) > 0 && GUILayout.Button($"Sat {_svc.SellValue(def)}", GUILayout.Width(62f))) { _svc.SellFromInventory(kv.Key, ItemCondition.Sound); _status = "Satıldı."; }
                if (damaged > 0 && GUILayout.Button($"Hasarlıyı sat {_svc.SellValue(def, ItemCondition.Damaged)}", GUILayout.Width(120f))) { _svc.SellFromInventory(kv.Key, ItemCondition.Damaged); _status = "Hasarlı eşya satıldı."; }
                GUILayout.EndHorizontal();
            }
        }

        private void StartPlacing(string itemId)
        {
            ClearSelection();
            _pendingFinish = null;
            _placeId = itemId;
            _ghostPos = new GridPos(24, 12, _view.ViewLevel);
            var def = _content.Items.Get(itemId);
            _band = -1;
            _confirmSlot = null;
            _status = def.Placement == ItemPlacement.Surface ? "Bir masa, raf ya da kancaya dokun."
                : def.Placement == ItemPlacement.Wall ? "Bir duvarın iç yüzüne dokun."
                : def.IsCeiling ? "Kapalı bir odada bir karoya dokun (tavana asılır)." : "Haritaya dokun.";
            RefreshOverlay();
        }

        private static readonly string[] RoomFilterNames = BuildRoomFilterNames();
        private static readonly string[] SetFilterNames = BuildSetFilterNames();
        private static readonly string[] ThemeFilterNames = BuildThemeFilterNames();

        private static string[] BuildThemeFilterNames()
        {
            var n = new string[Decor.Themes.Length + 1];
            n[0] = "Her tarz";
            for (int i = 0; i < Decor.Themes.Length; i++) n[i + 1] = Decor.Themes[i].name;
            return n;
        }

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
        // Eşya sekmesi: arama, boş kategoriler gizli, karma kilit modeli, "Yeni" rozeti (29-menu-yapisi).
        private void DrawShop()
        {
            int level = _svc.AccountLevel != null ? _svc.AccountLevel() : int.MaxValue;
            if (_seen == null)
            {
                // İlk açılışta açık olanlar görülmüş sayılır; sonradan açılanlar "Yeni" görünür.
                _seen = new HashSet<string>();
                foreach (var c in ItemCatalog.Categories)
                    foreach (var d in ItemCatalog.InCategory(_content, c.id)) if (_svc.IsUnlocked(d)) _seen.Add(d.Id);
            }
            if (_catNames == null || Event.current.type == EventType.Layout) RebuildShopTabs(level);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Ara:", GUILayout.Width(32f));
            _search = GUILayout.TextField(_search ?? "", GUILayout.Width(196f));
            GUILayout.EndHorizontal();
            string q = _activeSearch.Trim();
            if (q.Length > 0) { DrawSearch(q, level); return; }

            if (_catTabs.Count == 0) { GUILayout.Label("Henüz alınabilecek eşya yok."); return; }
            int n = ItemCatalog.Categories.Length;
            int gsel = Mathf.Max(0, _groupTabs.IndexOf(_shopGroup));
            var grc = new GUIContent[_groupNames.Length];
            for (int i = 0; i < grc.Length; i++) { int gi = _groupTabs[i]; grc[i] = UiSkin.C(_groupNames[i], gi < ShopGroupIcons.Length ? ShopGroupIcons[gi] : null); }
            int gpick = GUILayout.Toolbar(gsel, grc, GUILayout.Height(26f), GUILayout.Width(232f));
            if (gpick != gsel) { int gg = _groupTabs[gpick]; _deferred = () => { MarkSeen(); _shopGroup = gg; _shopCat = -1; _scroll = Vector2.zero; }; }
            int sel = Mathf.Max(0, _catTabs.IndexOf(_shopCat));
            var cc = new GUIContent[_catNames.Length];
            for (int i = 0; i < cc.Length; i++) { int ci = _catTabs[i]; cc[i] = UiSkin.C(_catNames[i], ci < ItemCatalog.Categories.Length ? "cat_" + ItemCatalog.Categories[ci].id : "cat_animals"); }
            int pick = GUILayout.SelectionGrid(sel, cc, 2, GUILayout.Width(232f));
            if (pick != sel) { int c = _catTabs[pick]; _deferred = () => { MarkSeen(); _shopCat = c; _scroll = Vector2.zero; }; }
            int cat = _catTabs[sel];
            if (cat == n) { DrawAnimals(); return; }

            bool filtering = _roomFilter > 0 || _setFilter > 0 || _themeFilter > 0;
            string fl = (filtering ? "Filtre (açık)" : "Filtre") + (_showFilters ? " ▴" : " ▾");
            if (GUILayout.Toggle(_showFilters, fl, GUI.skin.button) != _showFilters) { bool v = !_showFilters; _deferred = () => _showFilters = v; }
            if (_showFilters)
            {
                int roomF = GUILayout.SelectionGrid(_roomFilter, RoomFilterNames, 3);
                if (roomF != _roomFilter) _deferred = () => _roomFilter = roomF;
                int setF = GUILayout.SelectionGrid(_setFilter, SetFilterNames, 3);
                if (setF != _setFilter) _deferred = () => _setFilter = setF;
                int themeF = GUILayout.SelectionGrid(_themeFilter, ThemeFilterNames, 3);
                if (themeF != _themeFilter) _deferred = () => _themeFilter = themeF;
            }
            string room = _roomFilter > 0 ? ItemCatalog.Rooms[_roomFilter - 1].id : null;
            string set = _setFilter > 0 ? ItemCatalog.StyleSets[_setFilter - 1] : null;
            string theme = _themeFilter > 0 ? Decor.Themes[_themeFilter - 1].id : null;

            var list = Visible(ItemCatalog.InCategory(_content, ItemCatalog.Categories[cat].id, room, set, theme), level);
            if (list.Count == 0) GUILayout.Label("Bu filtreyle eşya yok.");
            foreach (var def in list) DrawCard(def);
        }

        // Görünür eşyalar: açıklar + yalnızca bir sonraki kilit kademesi (kilitli, şartıyla). Daha uzaktakiler gizli.
        private List<ItemDef> Visible(List<ItemDef> list, int level)
        {
            int next = int.MaxValue;
            foreach (var d in list)
                if (!_svc.IsUnlocked(d) && d.UnlockLevel > level && d.UnlockLevel < next) next = d.UnlockLevel;
            return list.Where(d => _svc.IsUnlocked(d) || d.UnlockLevel <= next).ToList();
        }

        // Kategori sekmeleri yalnızca Layout'ta kurulur; boş kategori gizli, yeni eşyası olan "•" alır.
        // Eşya kategorileri iki seviye (Sims gibi): önce ana grup, sonra alt kategori. "animals" = Hayvan sekmesi.
        private static readonly (string name, string[] cats)[] ShopGroups =
        {
            ("Mobilya", new[] { "seating", "sleep", "surfaces", "storage", "kids" }),
            ("Ev işi", new[] { "kitchen", "work", "hygiene", "heat_light" }),
            ("Süs", new[] { "decor", "decor_wall", "rug", "ceiling" }),
            ("Dışarı", new[] { "garden", "farm", "barn", "animals", "structure" }),
        };

        private int CatIndex(string id)
        {
            if (id == "animals") return _animals != null ? ItemCatalog.Categories.Length : -1;
            for (int i = 0; i < ItemCatalog.Categories.Length; i++) if (ItemCatalog.Categories[i].id == id) return i;
            return -1;
        }

        // Sekmeler yalnızca Layout'ta kurulur; boş kategori ve boş grup gizli, yeni eşyası olan "•" alır.
        private void RebuildShopTabs(int level)
        {
            _groupTabs.Clear(); _catTabs.Clear();
            var groupNames = new List<string>(); var catNames = new List<string>();
            for (int g = 0; g < ShopGroups.Length; g++)
            {
                var cats = new List<(int idx, string name)>();
                bool groupFresh = false;
                foreach (var id in ShopGroups[g].cats)
                {
                    int i = CatIndex(id);
                    if (i < 0) continue;
                    if (i == ItemCatalog.Categories.Length) { cats.Add((i, "Hayvan")); continue; }
                    var vis = Visible(ItemCatalog.InCategory(_content, ItemCatalog.Categories[i].id), level);
                    if (vis.Count == 0) continue;
                    bool fresh = vis.Any(d => _svc.IsUnlocked(d) && !_seen.Contains(d.Id));
                    groupFresh |= fresh;
                    cats.Add((i, ItemCatalog.Categories[i].name + (fresh ? " •" : "")));
                }
                if (cats.Count == 0) continue;
                _groupTabs.Add(g);
                groupNames.Add(ShopGroups[g].name + (groupFresh ? " •" : ""));
                if (g == _shopGroup) foreach (var c in cats) { _catTabs.Add(c.idx); catNames.Add(c.name); }
            }
            if (!_groupTabs.Contains(_shopGroup) && _groupTabs.Count > 0) { _shopGroup = _groupTabs[0]; RebuildShopTabs(level); return; }
            _groupNames = groupNames.ToArray();
            _catNames = catNames.ToArray();
        }

        private void DrawSearch(string q, int level)
        {
            var ids = new HashSet<string>();
            int shown = 0;
            foreach (var c in ItemCatalog.Categories)
                foreach (var d in Visible(ItemCatalog.InCategory(_content, c.id), level))
                    if (ids.Add(d.Id) && d.DisplayName.IndexOf(q, System.StringComparison.CurrentCultureIgnoreCase) >= 0) { DrawCard(d); shown++; }
            if (shown == 0) GUILayout.Label("Bulunamadı.");
        }

        private void MarkSeen()
        {
            if (_seen != null) _seen.UnionWith(_pendingSeen);
            _pendingSeen.Clear();
        }

        private void DrawCard(ItemDef def)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            string set = string.IsNullOrEmpty(def.StyleSet) ? "" : $" · {def.StyleSet}";
            GUILayout.Label($"{def.DisplayName} ({def.Width}x{def.Height}){set}");
            string info = ItemCatalog.Describe(def, null, _content);
            if (info.Length > 0) GUILayout.Label(info, _wrap);
            bool unlocked = _svc.IsUnlocked(def);
            int have = _house.Count(def.Id);
            if (unlocked && _seen != null && !_seen.Contains(def.Id))
            {
                _pendingSeen.Add(def.Id);
                GUILayout.Label("YENİ");
            }
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
            int nextAnimal = int.MaxValue;
            foreach (var a in _content.Animals.All) if (a.UnlockLevel > level && a.UnlockLevel < nextAnimal) nextAnimal = a.UnlockLevel;
            foreach (var a in _content.Animals.All)
            {
                if (a.UnlockLevel > nextAnimal) continue;   // uzak kilitler gizli, yalnızca sıradaki kademe görünür
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
                GUILayout.Label($"{a.Name} · tokluk {a.Fullness:0}{(a.IsMature ? "" : " · yavru")} · {a.HealthText}");
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
