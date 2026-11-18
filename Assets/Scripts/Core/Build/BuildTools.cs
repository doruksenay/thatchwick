using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Sims düzeyinde yapı araçları: malzemeli duvar, kapı, pencere, yarım duvar, çit, çit kapısı, korkuluk;
    /// duvar yüzü kaplaması; zemin stilleri; otomatik çatı stili; mobilya kaplaması; çizgi duvar, duvar itme,
    /// oda taşıma ve kopyalama, balyoz, ev değeri; merdiven boşluğu. Parça kataloğu items.json "buildParts"tır;
    /// katalogsuz içerikte (testler) türün sabit fiyatı ve stilsiz parça kullanılır.
    /// </summary>
    public sealed partial class BuildService
    {
        // ---------- parça kataloğu ----------

        /// <summary>Kenar türünün stil aldığı parça türü (None için null).</summary>
        public static BuildPartKind? PartKindFor(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall:
                case EdgeKind.HalfWall: return BuildPartKind.WallMaterial;
                case EdgeKind.Door: return BuildPartKind.Door;
                case EdgeKind.Window: return BuildPartKind.Window;
                case EdgeKind.Fence: return BuildPartKind.Fence;
                case EdgeKind.Gate: return BuildPartKind.Gate;
                case EdgeKind.Railing: return BuildPartKind.Railing;
                default: return null;
            }
        }

        public bool IsUnlocked(BuildPartDef part) => part == null || AccountLevel == null || AccountLevel() >= part.UnlockLevel;

        /// <summary>Kenarın parçası (stil yoksa ya da tanınmıyorsa türün varsayılanı); katalogda o tür yoksa null.</summary>
        public BuildPartDef EdgePart(EdgeKind kind, string style)
        {
            var pk = PartKindFor(kind);
            return pk.HasValue ? BuildParts.Resolve(_content, pk.Value, style) : null;
        }

        /// <summary>Kenarın akçesi (yarım duvar malzemenin yarısı, en az 1).</summary>
        public int EdgeCost(EdgeKind kind, string style)
        {
            if (kind == EdgeKind.None) return 0;
            var part = EdgePart(kind, style);
            int price = part != null ? part.Price : EdgePrice(kind);
            return kind == EdgeKind.HalfWall && part != null ? Math.Max(1, (price + 1) / 2) : price;
        }

        private int PartPrice(string id) => !string.IsNullOrEmpty(id) && _content.Parts.TryGet(id, out var p) ? p.Price : 0;

        // Varsayılan parça kayıtta null tutulur (eski kayıtlarla aynı, dosya küçük).
        private static string StoredStyle(BuildPartDef part) => part == null || part.IsDefault ? null : part.Id;

        // ---------- kenarlar ----------

        /// <summary>
        /// Kenara duvar, yarım duvar, kapı, pencere, çit, çit kapısı ya da korkuluk koyar, malzemesini/stilini değiştirir ya da
        /// (None) kaldırır. Kapı ve pencere yalnızca duvarın yerine konur; duvar bir eşyanın içinden geçemez.
        /// </summary>
        // ---- malzeme (tuğla, kiremit, yontma taş, boya) ----

        /// <summary>
        /// Parçanın malzemesi envanterde yeterliyse akçe yarıya iner ve malzeme harcanır (MaterialUse.Take); yoksa tam akçe.
        /// Akçe yoksa hiçbir şey harcanmaz.
        /// </summary>
        private struct MaterialUse
        {
            public string Item; public int Count; public int Cost;
            public void Take(Household h) { for (int i = 0; i < Count; i++) h.TryTakeItem(Item); }
            public void GiveBack(Household h) { if (Item != null && Count > 0) h.AddItem(Item, Count); }
        }

        private MaterialUse PlanMaterial(BuildPartDef part, int units, int cost)
        {
            var use = new MaterialUse { Cost = cost };
            if (part?.MaterialItem == null || units <= 0 || cost <= 0) return use;
            int need = part.MaterialCount * units;
            if (_house.Count(part.MaterialItem) < need) return use;
            use.Item = part.MaterialItem;
            use.Count = need;
            use.Cost = cost - cost / 2;
            return use;
        }

        /// <summary>Malzemeli parça için kısa ipucu: "tuğla varsa yarı fiyat".</summary>
        public string MaterialHint(BuildPartDef part) =>
            part?.MaterialItem == null ? null
            : $"{(_content.Items.TryGet(part.MaterialItem, out var d) ? d.DisplayName : part.MaterialItem)} varsa ({part.MaterialCount}/birim) yarı fiyat";

        public PlacementCheck SetEdge(Edge edge, EdgeKind kind, string style = null)
        {
            var map = _world.Map;
            if (!map.EdgeInBounds(edge)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(edge.SideA) && !map.IsOwned(edge.SideB)) return PlacementCheck.Fail(NotOwned);
            if (!map.HasTile(edge.SideA) && !map.HasTile(edge.SideB)) return PlacementCheck.Fail(edge.Z > 0 ? "Önce üst kat döşemesi gerekir." : "Önce burayı kazmalısın.");
            var old = map.GetEdge(edge);
            var oldLook = map.GetLook(edge)?.Clone();

            string newStyle = null;
            if (kind != EdgeKind.None)
            {
                var part = EdgePart(kind, style);
                if (part != null)
                {
                    if (!part.AllowsLevel(edge.Z))
                        return PlacementCheck.Fail(part.Levels.Count == 1 && part.Levels[0] < 0 ? $"{part.DisplayName} yalnızca bodrumda kullanılır." : $"{part.DisplayName} bu katta kullanılmaz.");
                    if (!IsUnlocked(part)) return PlacementCheck.Fail($"Hesap seviyesi {part.UnlockLevel} gerekli.");
                    newStyle = StoredStyle(part);
                }
            }
            if (old == kind && (oldLook?.Style) == newStyle) return PlacementCheck.Fail("Orada zaten var.");
            if (EdgeKinds.IsOpening(kind) && !EdgeKinds.IsRoomBoundary(old)) return PlacementCheck.Fail("Önce duvar kur.");
            if (kind == EdgeKind.Gate && old != EdgeKind.None && old != EdgeKind.Fence && old != EdgeKind.Gate)
                return PlacementCheck.Fail("Çit kapısı çit hattına ya da boş kenara konur.");
            if (EdgeKinds.BlocksMovement(kind) && SplitsItem(edge)) return PlacementCheck.Fail("Duvar bir eşyanın içinden geçemez.");

            int newPrice = EdgeCost(kind, newStyle), oldPrice = EdgeCost(old, oldLook?.Style);
            var mat = PlanMaterial(kind == EdgeKind.None ? null : EdgePart(kind, newStyle), 1, Math.Max(0, newPrice - Refund(oldPrice)));
            int cost = mat.Cost;
            int refund = kind == EdgeKind.None ? Refund(oldPrice) : 0;
            // Kaplamalar yalnızca duvar türü kenarda kalır; düşen kaplamanın yarısı iade.
            if (!EdgeKinds.IsRoomBoundary(kind) && oldLook != null)
                refund += Refund(PartPrice(oldLook.FinishA)) + Refund(PartPrice(oldLook.FinishB));
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            mat.Take(_house);
            if (refund > 0) _house.AddAkce(refund);
            // Düz duvar kalkınca (ya da kapı, pencere, yarım duvar olunca) asılı eşyalar envantere döner.
            if (old == EdgeKind.Wall && kind != EdgeKind.Wall)
                foreach (var hung in _world.WallItemsOn(edge))
                    foreach (var def in _world.RemoveItem(hung.Id)) _house.AddItem(def.Id);
            map.SetEdge(edge, kind, newStyle);
            int net = refund - cost;
            Record(kind == EdgeKind.None ? "Duvar kaldırma" : "Duvar", () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                mat.GiveBack(_house);
                map.SetEdgeFull(edge, old, oldLook);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // Kenarın iki yanını aynı eşya kaplıyorsa duvar onu böler.
        private bool SplitsItem(Edge edge)
        {
            var a = _world.GetItemAt(edge.SideA);
            return a != null && a == _world.GetItemAt(edge.SideB);
        }

        /// <summary>
        /// Duvar çizgisi (Sims'te sürükleyerek duvar): iki köşe noktası arasındaki düz hatta kenar koyar; köşeler aynı
        /// hizada değilse uzun eksene oturur. Tek geri alma adımıdır; zaten aynı olan kenarlar atlanır.
        /// </summary>
        public PlacementCheck SetEdgeLine(int x0, int y0, int x1, int y1, int z, EdgeKind kind, string style, out int built)
        {
            built = 0;
            if (kind == EdgeKind.None || EdgeKinds.IsOpening(kind) || kind == EdgeKind.Gate)
                return PlacementCheck.Fail("Çizgi aracı duvar, yarım duvar, çit ve korkuluk içindir.");
            var edges = LineEdges(x0, y0, x1, y1, z);
            if (edges.Count == 0) return PlacementCheck.Fail("İki farklı köşeye dokun.");
            var part = EdgePart(kind, style);
            string stored = StoredStyle(part);
            int before = UndoPushes;
            BeginGroup(kind == EdgeKind.Fence ? "Çit" : "Duvar çizgisi");
            PlacementCheck fail = PlacementCheck.Success;
            try
            {
                foreach (var e in edges)
                {
                    if (_world.Map.EdgeInBounds(e) && _world.Map.GetEdge(e) == kind && _world.Map.GetEdgeStyle(e) == stored) continue;
                    var r = SetEdge(e, kind, style);
                    if (!r.Ok) { fail = r; break; }
                    built++;
                }
            }
            finally { EndGroup(); }
            if (!fail.Ok && built > 0 && UndoPushes > before)
            {
                Undo();      // yarım hat kalmasın
                built = 0;
            }
            return fail.Ok ? PlacementCheck.Success : fail;
        }

        /// <summary>İki köşe noktası arasındaki düz hattın kenarları (uzun eksene oturtulur).</summary>
        public static List<Edge> LineEdges(int x0, int y0, int x1, int y1, int z)
        {
            var list = new List<Edge>();
            if (Math.Abs(x1 - x0) >= Math.Abs(y1 - y0))
            {
                for (int x = Math.Min(x0, x1); x < Math.Max(x0, x1); x++) list.Add(new Edge(x, y0, EdgeAxis.Horizontal, z));
            }
            else
            {
                for (int y = Math.Min(y0, y1); y < Math.Max(y0, y1); y++) list.Add(new Edge(x0, y, EdgeAxis.Vertical, z));
            }
            return list;
        }

        // ---------- duvar yüzü kaplaması ----------

        /// <summary>Duvarın bir yüzünü kaplar (badana, sıva, lambri...) ya da (null) çıplak bırakır. sideB: SideB karosuna bakan yüz.</summary>
        public PlacementCheck PaintWallSide(Edge edge, bool sideB, string finishId)
        {
            var map = _world.Map;
            if (!map.EdgeInBounds(edge)) return PlacementCheck.Fail("Harita dışı.");
            if (!EdgeKinds.IsRoomBoundary(map.GetEdge(edge))) return PlacementCheck.Fail("Kaplama yalnızca duvara yapılır.");
            var face = sideB ? edge.SideB : edge.SideA;
            if (!map.IsOwned(face)) return PlacementCheck.Fail(NotOwned);
            BuildPartDef part = null;
            if (finishId != null)
            {
                if (!_content.Parts.TryGet(finishId, out part) || part.Kind != BuildPartKind.WallFinish) return PlacementCheck.Fail("Böyle bir kaplama yok.");
                if (!IsUnlocked(part)) return PlacementCheck.Fail($"Hesap seviyesi {part.UnlockLevel} gerekli.");
            }
            var old = map.GetFinish(edge, sideB);
            if (old == finishId) return PlacementCheck.Fail("Bu yüz zaten öyle.");
            int cost = Math.Max(0, PartPrice(finishId) - Refund(PartPrice(old)));
            int refund = finishId == null ? Refund(PartPrice(old)) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (refund > 0) _house.AddAkce(refund);
            map.SetFinish(edge, sideB, finishId);
            int net = refund - cost;
            Record("Duvar kaplaması", () =>
            {
                if (!EdgeKinds.IsRoomBoundary(map.GetEdge(edge))) return PlacementCheck.Fail("Duvar artık yok.");
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                map.SetFinish(edge, sideB, old);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Odanın bütün iç duvar yüzlerini kaplar (Sims'te Shift ile oda boyama). Tek geri alma adımı.</summary>
        public PlacementCheck PaintRoomWalls(Room room, string finishId, out int painted)
        {
            painted = 0;
            if (room == null || !room.IsIndoor) return PlacementCheck.Fail("Bir odanın içine dokun.");
            var map = _world.Map;
            var faces = new List<(Edge e, bool b)>();
            foreach (var t in room.Tiles)
                foreach (var d in FourDirs)
                {
                    var e = GridMap.EdgeBetween(t, t + d);
                    if (!map.EdgeInBounds(e) || !EdgeKinds.IsRoomBoundary(map.GetEdge(e))) continue;
                    bool b = e.SideB == t;
                    if (map.GetFinish(e, b) != finishId) faces.Add((e, b));
                }
            if (faces.Count == 0) return PlacementCheck.Fail("Bu odanın duvarları zaten öyle.");
            return RunGroup("Oda kaplaması", faces, f => PaintWallSide(f.e, f.b, finishId), out painted);
        }

        // ---------- zemin ----------

        private int FloorCost(FloorType floor, string style)
        {
            if (floor == FloorType.Grass) return 0;
            var part = FloorPart(floor, style);
            return part != null ? part.Price : FloorPrice(floor);
        }

        /// <summary>Zemin türü ve stiline göre zemin parçası (stil tanınmazsa türün varsayılanı); katalog yoksa null.</summary>
        public BuildPartDef FloorPart(FloorType floor, string style)
        {
            if (floor == FloorType.Grass) return null;
            if (!string.IsNullOrEmpty(style) && _content.Parts.TryGet(style, out var p) && p.Kind == BuildPartKind.Floor && p.Base == floor) return p;
            return BuildParts.DefaultFloor(_content, floor);
        }

        /// <summary>Karoya zemin döşer: tür ve (isteğe bağlı) stil. Grass = kaplamayı kaldır, yarısı iade.</summary>
        public PlacementCheck PaintFloor(GridPos tile, FloorType floor, string style = null)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            if (!map.HasTile(tile)) return PlacementCheck.Fail("Burada zemin yok.");
            if (floor == FloorType.Water)
            {
                if (tile.Z != 0) return PlacementCheck.Fail("Gölet yalnızca zemin katta kazılır.");
                var room = map.Rooms.RoomAt(tile);
                if (room != null && room.IsIndoor) return PlacementCheck.Fail("Gölet açık havada kazılır.");
                if (_world.GetItemAt(tile) != null || map.IsBlocked(tile)) return PlacementCheck.Fail("Üstünde bir şey var.");
                if (map.GetRoad(tile) != RoadType.None) return PlacementCheck.Fail("Önce yolu kaldır.");
                if (map.GetConduit(tile) != ConduitType.None) return PlacementCheck.Fail("Önce su yolunu kaldır (gölete değen ark zaten beslenir).");
            }
            string newStyle = null;
            if (floor != FloorType.Grass)
            {
                var part = FloorPart(floor, style);
                if (part != null && !IsUnlocked(part)) return PlacementCheck.Fail($"Hesap seviyesi {part.UnlockLevel} gerekli.");
                newStyle = StoredStyle(part);
            }
            var old = map.GetFloor(tile);
            var oldStyle = map.GetFloorStyle(tile);
            if (old == floor && oldStyle == newStyle) return PlacementCheck.Fail("Orası zaten öyle.");
            int newPrice = FloorCost(floor, newStyle), oldPrice = FloorCost(old, oldStyle);
            var mat = PlanMaterial(floor == FloorType.Grass ? null : FloorPart(floor, style), 1, Math.Max(0, newPrice - Refund(oldPrice)));
            int cost = mat.Cost;
            int refund = floor == FloorType.Grass ? Refund(oldPrice) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            mat.Take(_house);
            if (refund > 0) _house.AddAkce(refund);
            map.SetFloor(tile, floor, newStyle);
            int net = refund - cost;
            Record("Zemin", () =>
            {
                if (!map.HasTile(tile)) return PlacementCheck.Fail("Zemin artık yok.");
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                mat.GiveBack(_house);
                map.SetFloor(tile, old, oldStyle);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Zemin parçasıyla döşer (tür parçadan gelir).</summary>
        public PlacementCheck PaintFloorPart(GridPos tile, string partId)
        {
            if (partId == null || !_content.Parts.TryGet(partId, out var p) || p.Kind != BuildPartKind.Floor) return PlacementCheck.Fail("Böyle bir zemin yok.");
            return PaintFloor(tile, p.Base, p.Id);
        }

        /// <summary>Odanın bütün zeminini döşer. Tek geri alma adımı.</summary>
        public PlacementCheck PaintRoomFloor(Room room, string partId, out int painted)
        {
            painted = 0;
            if (room == null) return PlacementCheck.Fail("Bir odaya dokun.");
            if (partId == null || !_content.Parts.TryGet(partId, out var p) || p.Kind != BuildPartKind.Floor) return PlacementCheck.Fail("Böyle bir zemin yok.");
            var map = _world.Map;
            string stored = StoredStyle(p);
            var tiles = new List<GridPos>();
            foreach (var t in room.Tiles) if (map.GetFloor(t) != p.Base || map.GetFloorStyle(t) != stored) tiles.Add(t);
            if (tiles.Count == 0) return PlacementCheck.Fail("Bu oda zaten öyle döşeli.");
            return RunGroup("Oda zemini", tiles, t => PaintFloor(t, p.Base, p.Id), out painted);
        }

        // Bir dizi işlemi tek geri alma adımında yapar; biri başarısız olursa yapılanlar geri alınır.
        private PlacementCheck RunGroup<T>(string label, List<T> steps, Func<T, PlacementCheck> run, out int done)
        {
            done = 0;
            int before = UndoPushes;
            PlacementCheck fail = PlacementCheck.Success;
            BeginGroup(label);
            try
            {
                foreach (var s in steps)
                {
                    var r = run(s);
                    if (!r.Ok) { fail = r; break; }
                    done++;
                }
            }
            finally { EndGroup(); }
            if (!fail.Ok && done > 0 && UndoPushes > before)
            {
                Undo();
                done = 0;
            }
            return fail;
        }

        // ---------- otomatik çatı ----------

        /// <summary>Karonun üstündeki otomatik çatı (görünmeyen bölgeler dahil); yoksa null.</summary>
        public RoofRegion RoofAt(GridPos tile)
        {
            foreach (var r in RoofBuilder.Compute(_world.Map, includeHidden: true))
                if (r.Level == tile.Z && r.Contains(tile)) return r;
            return null;
        }

        /// <summary>Çatının (bölgenin hepsi) stilini değiştirir ya da GridMap.NoRoof ile çatısız bırakır. Karo başına fiyat farkı.</summary>
        public PlacementCheck SetRoofStyle(GridPos tile, string style)
        {
            var region = RoofAt(tile);
            if (region == null) return PlacementCheck.Fail("Burada çatı yok: çatı, üstü açık iç odalara kendiliğinden kurulur.");
            BuildPartDef part = null;
            if (style != GridMap.NoRoof)
            {
                if (style == null || !_content.Parts.TryGet(style, out part) || part.Kind != BuildPartKind.Roof) return PlacementCheck.Fail("Böyle bir çatı yok.");
                if (!IsUnlocked(part)) return PlacementCheck.Fail($"Hesap seviyesi {part.UnlockLevel} gerekli.");
            }
            string stored = style == GridMap.NoRoof ? GridMap.NoRoof : StoredStyle(part);
            var map = _world.Map;
            var olds = new List<(GridPos p, string s)>();
            int cost = 0, refund = 0;
            foreach (var t in region.Tiles)
            {
                string old = map.GetRoofStyle(t.Pos);
                if (old == stored) continue;
                olds.Add((t.Pos, old));
                int oldPrice = RoofTilePrice(old), newPrice = RoofTilePrice(stored);
                cost += Math.Max(0, newPrice - Refund(oldPrice));
                if (stored == GridMap.NoRoof) refund += Refund(oldPrice);
            }
            if (olds.Count == 0) return PlacementCheck.Fail("Çatı zaten öyle.");
            var mat = PlanMaterial(part, olds.Count, cost);      // kiremit karo başına
            cost = mat.Cost;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            mat.Take(_house);
            if (refund > 0) _house.AddAkce(refund);
            foreach (var o in olds) map.SetRoofStyle(o.p, stored);
            int net = refund - cost;
            Record("Çatı", () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                mat.GiveBack(_house);
                foreach (var o in olds) if (map.HasTile(o.p)) map.SetRoofStyle(o.p, o.s);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        private int RoofTilePrice(string style)
        {
            if (style == GridMap.NoRoof) return 0;
            var part = BuildParts.Resolve(_content, BuildPartKind.Roof, style);
            return part != null ? part.Price : 0;
        }

        // ---------- mobilya kaplaması (renk/malzeme örneği) ----------

        /// <summary>Kaplamanın fiyat çarpanı (yoksa ya da varsayılansa 1).</summary>
        public float FinishFactor(ItemInstance item)
        {
            if (item?.FinishId == null || !_content.Parts.TryGet(item.FinishId, out var p)) return 1f;
            return p.PriceFactor;
        }

        // Kaplama farkının satış oranı (satışta ve envantere alırken iade edilir).
        private int FinishSellBonus(ItemInstance item)
        {
            float extra = FinishFactor(item) - 1f;
            return extra <= 0f ? 0 : (int)Math.Floor(item.Def.Price * extra * SellRatio);
        }

        /// <summary>Yerleştirilmiş mobilyanın kaplamasını değiştirir (null = varsayılan). Pahalı kaplama farkı ödenir, ucuzunda yarısı iade.</summary>
        public PlacementCheck SetItemFinish(int itemId, string finishId)
        {
            var item = _world.GetItem(itemId);
            if (item == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (IsLocked(item) || item.IsPlant) return PlacementCheck.Fail("Bu eşyanın kaplaması değişmez.");
            if (string.IsNullOrEmpty(item.Def.FinishGroup)) return PlacementCheck.Fail("Bu eşyanın renk ya da malzeme seçeneği yok.");
            BuildPartDef part = null;
            if (finishId != null)
            {
                if (!_content.Parts.TryGet(finishId, out part) || part.Kind != BuildPartKind.ItemFinish || part.Group != item.Def.FinishGroup)
                    return PlacementCheck.Fail("Bu eşyaya uymayan kaplama.");
                if (!IsUnlocked(part)) return PlacementCheck.Fail($"Hesap seviyesi {part.UnlockLevel} gerekli.");
                if (part.IsDefault) finishId = null;
            }
            var old = item.FinishId;
            if (old == finishId) return PlacementCheck.Fail("Zaten öyle.");
            float oldF = FinishFactor(item), newF = part != null ? part.PriceFactor : 1f;
            int diff = (int)Math.Round(item.Def.Price * (newF - oldF));
            var mat = PlanMaterial(part, 1, diff > 0 ? diff : 0);     // boyalı kumaş: boya
            int cost = mat.Cost;
            int refund = diff < 0 ? (int)Math.Floor(-diff * SellRatio) : 0;
            if (!_house.TrySpend(cost)) return PlacementCheck.Fail("Akçe yetmiyor.");
            mat.Take(_house);
            if (refund > 0) _house.AddAkce(refund);
            item.FinishId = finishId;
            _world.NotifyChanged();
            int net = refund - cost;
            Record("Renk/malzeme", () =>
            {
                var cur = _world.GetItem(itemId);
                if (cur == null) return PlacementCheck.Fail("Eşya artık yok.");
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                mat.GiveBack(_house);
                cur.FinishId = old;
                _world.NotifyChanged();
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---------- merdiven boşluğu ----------

        /// <summary>Yukarı çıkan merdivenin üstündeki (üst kattaki) karolar: merdiven boşluğu.</summary>
        private List<GridPos> StairwellTiles(ItemDef def, GridPos position, int rotation)
        {
            var list = new List<GridPos>();
            if (def == null || def.StairsDelta <= 0) return list;
            int up = position.Z + 1;
            if (!GridMap.IsValidLevel(up)) return list;
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                {
                    var p = new GridPos(position.X + dx, position.Y + dy, up);
                    if (_world.Map.HasTile(p)) list.Add(p);
                }
            return list;
        }

        // Merdiven boşluğu açılamıyorsa nedeni (üstte eşya, biri ya da başka bir merdivenin ucu var).
        private string StairwellBlocker(ItemDef def, GridPos position, int rotation)
        {
            foreach (var p in StairwellTiles(def, position, rotation))
            {
                if (_world.GetItemAt(p) != null || _world.GetRugAt(p) != null) return "Merdivenin üstündeki döşemede eşya var: merdiven boşluğu açılamaz.";
                if (_world.IsOccupiedByCharacter(p)) return "Merdivenin üstünde biri duruyor.";
                if (_world.Map.HasLinkAt(p)) return "Merdivenin üstünde başka bir merdivenin ucu var.";
            }
            return null;
        }

        // Merdivenin üstündeki döşemeyi kaldırır (boşluk); yarısı iade, geri alınabilir.
        private void CutStairwell(ItemDef def, GridPos position, int rotation)
        {
            var map = _world.Map;
            foreach (var p in StairwellTiles(def, position, rotation))
            {
                var oldFloor = map.GetFloor(p);
                var oldStyle = map.GetFloorStyle(p);
                int refund = Refund(TilePrice(p.Z));
                map.SetTile(p, false);
                _house.AddAkce(refund);
                Record("Merdiven boşluğu", () =>
                {
                    if (!_house.TrySpend(refund)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz.");
                    map.SetTile(p, true);
                    map.SetFloor(p, oldFloor, oldStyle);
                    return PlacementCheck.Success;
                });
            }
        }

        // Karo, alt kattaki yukarı çıkan bir merdivenin üstünde mi.
        private bool IsAboveStairs(GridPos upper)
        {
            var below = upper.OnLevel(upper.Z - 1);
            foreach (var it in _world.Items)
            {
                if (!it.Def.IsStairs || it.Def.StairsDelta <= 0 || it.IsDecor || it.Position.Z != below.Z) continue;
                foreach (var t in it.OccupiedTiles()) if (t == below) return true;
            }
            return false;
        }

        // ---------- duvar itme (Sims: push/pull wall) ----------

        /// <summary>
        /// Duvar hattını (dokunulan kenarın bulunduğu kesintisiz duvar/kapı/pencere hattı) dik yönde delta karo iter.
        /// Uçlardaki dik duvarlar uzar ya da kısalır (oda büyür ya da küçülür); hat üstündeki kapı, pencere, kaplama ve
        /// asılı eşyalar hatla gelir. Yeni dik duvarlar hat malzemesiyle ücretlidir, kısalan kısmın yarısı iade.
        /// </summary>
        public PlacementCheck PushWall(Edge edge, int delta)
        {
            var map = _world.Map;
            if (delta == 0 || Math.Abs(delta) > 8) return PlacementCheck.Fail("1-8 karo it.");
            if (!map.EdgeInBounds(edge) || !EdgeKinds.IsRoomBoundary(map.GetEdge(edge))) return PlacementCheck.Fail("Bir duvara dokun.");
            bool horiz = edge.Axis == EdgeAxis.Horizontal;
            int z = edge.Z;
            // Hat: aynı eksende bitişik oda sınırı kenarları.
            int a = horiz ? edge.X : edge.Y, b = a;
            Func<int, Edge> at = i => horiz ? new Edge(i, edge.Y, EdgeAxis.Horizontal, z) : new Edge(edge.X, i, EdgeAxis.Vertical, z);
            while (map.EdgeInBounds(at(a - 1)) && EdgeKinds.IsRoomBoundary(map.GetEdge(at(a - 1)))) a--;
            while (map.EdgeInBounds(at(b + 1)) && EdgeKinds.IsRoomBoundary(map.GetEdge(at(b + 1)))) b++;
            int line = horiz ? edge.Y : edge.X;
            Func<int, int, Edge> run = (i, l) => horiz ? new Edge(i, l, EdgeAxis.Horizontal, z) : new Edge(l, i, EdgeAxis.Vertical, z);
            // Dik kenar: hat köşesi v'de, hat ekseninde k konumundan k+1'e uzanan.
            Func<int, int, Edge> perp = (v, k) => horiz ? new Edge(v, k, EdgeAxis.Vertical, z) : new Edge(k, v, EdgeAxis.Horizontal, z);

            var runEdges = new List<Edge>();
            for (int i = a; i <= b; i++) runEdges.Add(run(i, line));
            var runSet = new HashSet<Edge>(runEdges);
            string material = map.GetEdgeStyle(edge);
            if (map.GetEdge(edge) != EdgeKind.Wall)
                foreach (var r in runEdges) if (map.GetEdge(r) == EdgeKind.Wall) { material = map.GetEdgeStyle(r); break; }

            // Plan: kaldırılacak ve eklenecek kenarlar.
            var plan = new Dictionary<Edge, (EdgeKind kind, EdgeLook look)>();
            foreach (var r in runEdges) plan[r] = (EdgeKind.None, null);
            foreach (var r in runEdges)
            {
                var target = run(horiz ? r.X : r.Y, line + delta);
                if (!map.EdgeInBounds(target)) return PlacementCheck.Fail("Duvar harita dışına çıkar.");
                if (map.GetEdge(target) != EdgeKind.None && !runSet.Contains(target)) return PlacementCheck.Fail("Önünde başka bir duvar var.");
                plan[target] = (map.GetEdge(r), map.GetLook(r)?.Clone());
            }
            int addCost = 0, removeRefund = 0;
            foreach (int v in new[] { a, b + 1 })
            {
                bool before = EdgeKinds.IsRoomBoundary(map.GetEdge(perp(v, line - 1)));   // dik duvar hattın küçük tarafında
                bool after = EdgeKinds.IsRoomBoundary(map.GetEdge(perp(v, line)));        // büyük tarafında
                int steps = Math.Abs(delta);
                for (int s = 0; s < steps; s++)
                {
                    int k = delta > 0 ? line + s : line - 1 - s;
                    var pe = perp(v, k);
                    if (!map.EdgeInBounds(pe)) return PlacementCheck.Fail("Duvar harita dışına çıkar.");
                    bool shrink = delta > 0 ? after && !before : before && !after;
                    var cur = map.GetEdge(pe);
                    if (shrink)
                    {
                        if (cur != EdgeKind.None) { plan[pe] = (EdgeKind.None, null); removeRefund += Refund(EdgeCost(cur, map.GetEdgeStyle(pe))); }
                    }
                    else if (cur == EdgeKind.None && !plan.ContainsKey(pe))
                    {
                        plan[pe] = (EdgeKind.Wall, material != null ? new EdgeLook { Style = material } : null);
                        addCost += EdgeCost(EdgeKind.Wall, material);
                    }
                }
            }
            foreach (var kv in plan)
            {
                var e = kv.Key;
                if (kv.Value.kind == EdgeKind.None) continue;
                if (!map.IsOwned(e.SideA) && !map.IsOwned(e.SideB)) return PlacementCheck.Fail(NotOwned);
                if (!map.HasTile(e.SideA) && !map.HasTile(e.SideB)) return PlacementCheck.Fail("Duvarın yanında zemin yok.");
                if (map.GetEdge(e) == EdgeKind.None && SplitsItem(e)) return PlacementCheck.Fail("Duvar bir eşyanın içinden geçemez.");
            }
            int net = removeRefund - addCost;
            if (net < 0 && _house.Akce < -net) return PlacementCheck.Fail("Akçe yetmiyor.");

            // Uygula (önce eski hali kaydet).
            var snapshot = new Dictionary<Edge, (EdgeKind, EdgeLook)>();
            foreach (var e in plan.Keys) snapshot[e] = (map.GetEdge(e), map.GetLook(e)?.Clone());
            var hung = new List<ItemInstance>();
            foreach (var r in runEdges) hung.AddRange(_world.WallItemsOn(r));
            var hungOld = new List<(int id, GridPos p, int rot)>();
            foreach (var it in hung) hungOld.Add((it.Id, it.Position, it.Rotation));
            foreach (var kv in plan) if (kv.Value.kind == EdgeKind.None) map.SetEdgeFull(kv.Key, EdgeKind.None, null);
            foreach (var kv in plan) if (kv.Value.kind != EdgeKind.None) map.SetEdgeFull(kv.Key, kv.Value.kind, kv.Value.look);
            var off = horiz ? new GridPos(0, delta) : new GridPos(delta, 0);
            var stored = new List<string>();
            foreach (var it in hung)
            {
                var np = new GridPos(it.Position.X + off.X, it.Position.Y + off.Y, it.Position.Z);
                if (!_world.MoveWallItem(it.Id, np, it.Rotation).Ok)
                    foreach (var def in _world.RemoveItem(it.Id)) { _house.AddItem(def.Id); stored.Add(def.Id); }
            }
            if (net > 0) _house.AddAkce(net); else if (net < 0) _house.TrySpend(-net);
            Record("Duvar itme", () =>
            {
                if (net > 0) { if (!_house.TrySpend(net)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (net < 0) _house.AddAkce(-net);
                foreach (var kv in snapshot) if (kv.Value.Item1 == EdgeKind.None) map.SetEdgeFull(kv.Key, EdgeKind.None, null);
                foreach (var kv in snapshot) if (kv.Value.Item1 != EdgeKind.None) map.SetEdgeFull(kv.Key, kv.Value.Item1, kv.Value.Item2);
                foreach (var h in hungOld) if (_world.GetItem(h.id) != null) _world.MoveWallItem(h.id, h.p, h.rot);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---------- balyoz ----------

        /// <summary>
        /// Dikdörtgendeki her şeyi kaldırır (Sims balyozu): eşyalar satılır, en az bir yanı alanda olan duvar/çit/kapılar
        /// yıkılır, zemin kat zemini çime döner. Taşınmaz ve kaldırılamayan eşyalar atlanır. Tek geri alma adımı.
        /// </summary>
        public PlacementCheck Demolish(int x0, int y0, int x1, int y1, int z, out int removed, out int skipped)
        {
            removed = 0; skipped = 0;
            var map = _world.Map;
            int ax = Math.Min(x0, x1), bx = Math.Max(x0, x1), ay = Math.Min(y0, y1), by = Math.Max(y0, y1);
            bool InRect(GridPos p) => p.Z == z && p.X >= ax && p.X <= bx && p.Y >= ay && p.Y <= by;
            var items = new List<int>();
            foreach (var it in _world.Items)
            {
                if (it.IsDecor) continue;
                bool hit = it.IsWallMounted ? InRect(it.Position) : false;
                foreach (var t in it.OccupiedTiles()) if (InRect(t)) { hit = true; break; }
                if (hit) items.Add(it.Id);
            }
            var edges = new List<Edge>();
            foreach (var kv in map.Edges)
                if (kv.Key.Z == z && (InRect(kv.Key.SideA) || InRect(kv.Key.SideB))) edges.Add(kv.Key);
            var floors = new List<GridPos>();
            if (z == 0)
                for (int y = ay; y <= by; y++)
                    for (int x = ax; x <= bx; x++)
                    {
                        var p = new GridPos(x, y, z);
                        if (map.InBounds(p) && map.IsOwned(p) && map.GetFloor(p) != FloorType.Grass) floors.Add(p);
                    }
            if (items.Count + edges.Count + floors.Count == 0) return PlacementCheck.Fail("Burada yıkılacak bir şey yok.");

            BeginGroup("Balyoz");
            try
            {
                // Önce duvar eşyaları ve eşyalar (duvar kalkınca envantere dönmesin, satılsın), sonra kenarlar, sonra zemin.
                items.Sort((p, q) => (_world.GetItem(q)?.IsWallMounted == true).CompareTo(_world.GetItem(p)?.IsWallMounted == true));
                foreach (var id in items)
                {
                    var it = _world.GetItem(id);
                    if (it == null) continue;
                    var r = SellPlaced(id);
                    if (r.Ok) removed++; else skipped++;
                }
                foreach (var e in edges)
                {
                    if (map.GetEdge(e) == EdgeKind.None) continue;
                    var r = SetEdge(e, EdgeKind.None);
                    if (r.Ok) removed++; else skipped++;
                }
                foreach (var p in floors)
                {
                    var r = PaintFloor(p, FloorType.Grass);
                    if (r.Ok) removed++; else skipped++;
                }
            }
            finally { EndGroup(); }
            return removed > 0 ? PlacementCheck.Success : PlacementCheck.Fail("Hiçbir şey kaldırılamadı.");
        }

        // ---------- ev değeri ----------

        /// <summary>
        /// Ev değeri (Sims gibi): arsandaki zemin, kat döşemesi ve kazı, duvar ve parçalar, kaplamalar, çatı ve eşyalar
        /// (kaplamasıyla). Kaynak ve bitkiler sayılmaz.
        /// </summary>
        public int HouseValue()
        {
            var map = _world.Map;
            int total = 0;
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                foreach (var t in map.TilesOnLevel(z))
                {
                    if (!map.IsOwned(t)) continue;
                    total += FloorCost(map.GetFloor(t), map.GetFloorStyle(t));
                    if (z != 0) total += TilePrice(z);
                }
            foreach (var kv in map.Edges)
            {
                var e = kv.Key;
                if (!map.IsOwned(e.SideA) && !map.IsOwned(e.SideB)) continue;
                var look = map.GetLook(e);
                total += EdgeCost(kv.Value, look?.Style) + PartPrice(look?.FinishA) + PartPrice(look?.FinishB);
            }
            foreach (var r in RoofBuilder.Compute(map))
                foreach (var t in r.Tiles)
                    if (map.IsOwned(t.Pos)) total += RoofTilePrice(map.GetRoofStyle(t.Pos));
            foreach (var it in _world.Items)
            {
                if (it.IsPlant || it.Def.Placement == ItemPlacement.Resource || IsLocked(it)) continue;
                if (!map.IsOwned(it.Position)) continue;
                total += (int)Math.Round(it.Def.Price * FinishFactor(it));
            }
            return total;
        }

        // ---------- oda taşıma ve kopyalama ----------

        // Odanın kenarları: karolarından komşuya geçen her dolu kenar (iç bölmeler dahil).
        private List<Edge> RoomEdges(Room room, HashSet<GridPos> tiles)
        {
            var map = _world.Map;
            var set = new HashSet<Edge>();
            foreach (var t in tiles)
                foreach (var d in FourDirs)
                {
                    var e = GridMap.EdgeBetween(t, t + d);
                    if (map.EdgeInBounds(e) && map.GetEdge(e) != EdgeKind.None) set.Add(e);
                }
            var list = new List<Edge>(set);
            list.Sort((p, q) => p.Y != q.Y ? p.Y.CompareTo(q.Y) : p.X != q.X ? p.X.CompareTo(q.X) : p.Axis.CompareTo(q.Axis));
            return list;
        }

        // Odanın eşyaları: tüm karoları odada olan yer eşyaları ve halılar, odanın duvarlarına içeriden asılı eşyalar.
        private List<ItemInstance> RoomItems(HashSet<GridPos> tiles, HashSet<Edge> edges)
        {
            var list = new List<ItemInstance>();
            foreach (var it in _world.Items)
            {
                if (it.IsDecor) continue;
                if (it.IsWallMounted) { if (tiles.Contains(it.Position) && edges.Contains(it.WallEdge)) list.Add(it); continue; }
                bool inside = true;
                foreach (var t in it.OccupiedTiles()) if (!tiles.Contains(t)) { inside = false; break; }
                if (inside) list.Add(it);
            }
            return list;
        }

        private static Edge Shift(Edge e, int dx, int dy) => new Edge(e.X + dx, e.Y + dy, e.Axis, e.Z);

        /// <summary>
        /// Odayı (duvarları, kapı-pencereleri, kaplamaları, zemini, eşyalarıyla) dx, dy karo taşır. Komşu odayla paylaşılan
        /// duvar yerinde kalır (komşu açıkta kalmasın). Ücretsizdir; hedef boş olmalı. Tek geri alma adımı.
        /// </summary>
        public PlacementCheck MoveRoom(Room room, int dx, int dy)
        {
            if (room == null || !room.IsIndoor) return PlacementCheck.Fail("Taşınacak odanın içine dokun.");
            if (dx == 0 && dy == 0) return PlacementCheck.Fail("Odayı bir yöne kaydır.");
            var map = _world.Map;
            var tiles = new HashSet<GridPos>(room.Tiles);
            foreach (var t in tiles)
            {
                var q = new GridPos(t.X + dx, t.Y + dy, t.Z);
                if (!map.InBounds(q) || !map.HasTile(q)) return PlacementCheck.Fail("Oda hedefte sığmıyor (harita ya da kat dışı).");
                if (!map.IsOwned(q)) return PlacementCheck.Fail(NotOwned);
                if (!tiles.Contains(q) && (map.IsBlocked(q) || _world.GetRugAt(q) != null || _world.IsOccupiedByCharacter(q)))
                    return PlacementCheck.Fail("Hedefte eşya ya da biri var.");
            }
            var edges = RoomEdges(room, tiles);
            var edgeSet = new HashSet<Edge>(edges);
            // Paylaşılan duvar: öbür yanı başka bir iç oda olan kenar; yerinde kalır (kopyası taşınır).
            var rooms = map.Rooms;
            bool Shared(Edge e)
            {
                var other = tiles.Contains(e.SideA) ? e.SideB : e.SideA;
                if (tiles.Contains(other)) return false;
                var r = rooms.RoomAt(other);
                return r != null && r.IsIndoor && r != room;
            }
            var targets = new Dictionary<Edge, (EdgeKind, EdgeLook)>();
            foreach (var e in edges)
            {
                var t = Shift(e, dx, dy);
                if (!map.EdgeInBounds(t)) return PlacementCheck.Fail("Oda harita dışına taşar.");
                var cur = map.GetEdge(t);
                if (cur != EdgeKind.None && !(edgeSet.Contains(t) && !Shared(t)) && cur != map.GetEdge(e))
                    return PlacementCheck.Fail("Hedefte başka bir duvar var.");
                targets[t] = (map.GetEdge(e), map.GetLook(e)?.Clone());
            }
            var items = RoomItems(tiles, edgeSet);

            // Anlık görüntü: dokunulan kenarlar ve zeminler.
            var touched = new HashSet<Edge>(edges);
            foreach (var t in targets.Keys) touched.Add(t);
            var edgeSnap = new Dictionary<Edge, (EdgeKind, EdgeLook)>();
            foreach (var e in touched) edgeSnap[e] = (map.GetEdge(e), map.GetLook(e)?.Clone());
            var floorTiles = new HashSet<GridPos>(tiles);
            foreach (var t in tiles) floorTiles.Add(new GridPos(t.X + dx, t.Y + dy, t.Z));
            var floorSnap = new Dictionary<GridPos, (FloorType, string)>();
            foreach (var p in floorTiles) floorSnap[p] = (map.GetFloor(p), map.GetFloorStyle(p));

            void RestoreMap()
            {
                foreach (var kv in edgeSnap) if (kv.Value.Item1 == EdgeKind.None) map.SetEdgeFull(kv.Key, EdgeKind.None, null);
                foreach (var kv in edgeSnap) if (kv.Value.Item1 != EdgeKind.None) map.SetEdgeFull(kv.Key, kv.Value.Item1, kv.Value.Item2);
                foreach (var kv in floorSnap) map.SetFloor(kv.Key, kv.Value.Item1, kv.Value.Item2);
            }

            // Uygula: eski kenarlar (paylaşılanlar hariç) kalkar, yenileri kurulur, zemin taşınır, eşyalar kayar.
            var shared = new HashSet<Edge>();
            foreach (var e in edges) if (Shared(e)) shared.Add(e);
            var floorsFrom = new Dictionary<GridPos, (FloorType, string)>();
            foreach (var t in tiles) floorsFrom[t] = floorSnap[t];
            void ApplyMap()
            {
                foreach (var e in edges) if (!shared.Contains(e)) map.SetEdgeFull(e, EdgeKind.None, null);
                foreach (var kv in targets) map.SetEdgeFull(kv.Key, kv.Value.Item1, kv.Value.Item2);
                foreach (var t in tiles) map.SetFloor(t, t.Z == 0 ? FloorType.Grass : t.Z < 0 ? FloorType.Dirt : FloorType.Wood);
                foreach (var kv in floorsFrom) map.SetFloor(new GridPos(kv.Key.X + dx, kv.Key.Y + dy, kv.Key.Z), kv.Value.Item1, kv.Value.Item2);
            }
            ApplyMap();
            var ids = new List<int>();
            foreach (var it in items) ids.Add(it.Id);
            var moved = _world.ShiftItems(ids, dx, dy);
            if (!moved.Ok)
            {
                RestoreMap();
                return moved;
            }
            Record("Oda taşıma", () =>
            {
                // Önce duvarlar eski yerine (duvar eşyaları oraya asılacak), sonra eşyalar.
                RestoreMap();
                var back = _world.ShiftItems(ids, -dx, -dy);
                if (back.Ok) return PlacementCheck.Success;
                ApplyMap();
                return PlacementCheck.Fail($"Geri alınamadı: {back.Reason}");
            });
            return PlacementCheck.Success;
        }

        /// <summary>
        /// Odanın kopyasını dx, dy karo öteye kurar: duvarlar, kapı-pencereler, kaplamalar, zemin ve eşyalar (yeni alınır).
        /// Bitki, merdiven ve barınak kopyalanmaz. Hepsi akçeyle ödenir; bir adım olmazsa yapılanlar geri alınır.
        /// </summary>
        public PlacementCheck CopyRoom(Room room, int dx, int dy, out int skippedItems)
        {
            skippedItems = 0;
            if (room == null || !room.IsIndoor) return PlacementCheck.Fail("Kopyalanacak odanın içine dokun.");
            if (dx == 0 && dy == 0) return PlacementCheck.Fail("Kopyayı bir yöne kaydır.");
            var map = _world.Map;
            var tiles = new HashSet<GridPos>(room.Tiles);
            foreach (var t in tiles)
            {
                var q = new GridPos(t.X + dx, t.Y + dy, t.Z);
                if (tiles.Contains(q)) return PlacementCheck.Fail("Kopya odanın üstüne binmesin: daha uzağa koy.");
                if (!map.InBounds(q) || !map.HasTile(q)) return PlacementCheck.Fail("Kopya hedefte sığmıyor (harita ya da kat dışı).");
                if (map.IsBlocked(q)) return PlacementCheck.Fail("Hedefte eşya ya da engel var.");
            }
            var edges = RoomEdges(room, tiles);
            var edgeSet = new HashSet<Edge>(edges);
            var items = RoomItems(tiles, edgeSet);
            int before = UndoPushes;
            PlacementCheck fail = PlacementCheck.Success;
            BeginGroup("Oda kopyası");
            try
            {
                foreach (var t in tiles)
                {
                    var q = new GridPos(t.X + dx, t.Y + dy, t.Z);
                    var f = map.GetFloor(t);
                    var fs = map.GetFloorStyle(t);
                    if (map.GetFloor(q) == f && map.GetFloorStyle(q) == fs) continue;
                    var r = PaintFloor(q, f, fs);
                    if (!r.Ok) { fail = r; break; }
                }
                // Önce duvarlar (kapı ve pencere duvara açılır), sonra açıklıklar ve diğerleri, en son kaplamalar.
                foreach (var pass in new[] { 0, 1 })
                {
                    if (!fail.Ok) break;
                    foreach (var e in edges)
                        {
                            var kind = map.GetEdge(e);
                            var target = Shift(e, dx, dy);
                            bool opening = EdgeKinds.IsOpening(kind);
                            if (pass == 0 && opening && !EdgeKinds.IsRoomBoundary(map.GetEdge(target)))
                            {
                                var w = SetEdge(target, EdgeKind.Wall, null);
                                if (!w.Ok) { fail = w; break; }
                                continue;
                            }
                            if (pass == 0 && opening) continue;
                            if (pass == 1 && !opening) continue;
                            if (map.GetEdge(target) == kind && map.GetEdgeStyle(target) == map.GetEdgeStyle(e)) continue;
                            var r = SetEdge(target, kind, map.GetEdgeStyle(e));
                            if (!r.Ok) { fail = r; break; }
                        }
                }
                if (fail.Ok)
                    foreach (var e in edges)
                    {
                        if (!EdgeKinds.IsRoomBoundary(map.GetEdge(e))) continue;
                        var target = Shift(e, dx, dy);
                        foreach (var side in new[] { false, true })
                        {
                            var fin = map.GetFinish(e, side);
                            if (fin == null || map.GetFinish(target, side) == fin) continue;
                            var r = PaintWallSide(target, side, fin);
                            if (!r.Ok) { fail = r; break; }
                        }
                        if (!fail.Ok) break;
                    }
                if (fail.Ok)
                {
                    var newIds = new Dictionary<int, int>();
                    foreach (var it in items)
                    {
                        if (it.IsPlant || it.Def.IsStairs || it.Def.IsHousing || IsLocked(it)) { skippedItems++; continue; }
                        var buy = Buy(it.Def.Id);
                        if (!buy.Ok) { fail = buy; break; }
                        var np = new GridPos(it.Position.X + dx, it.Position.Y + dy, it.Position.Z);
                        ItemInstance made;
                        var r = it.IsWallMounted ? PlaceWallFromInventory(it.Def.Id, np, it.Rotation, out made) : PlaceFromInventory(it.Def.Id, np, it.Rotation, out made);
                        if (!r.Ok) { fail = r; break; }
                        newIds[it.Id] = made.Id;
                        if (it.FinishId != null) { var fr = SetItemFinish(made.Id, it.FinishId); if (!fr.Ok) { fail = fr; break; } }
                        foreach (var decor in _world.GetDecorOn(it.Id))
                        {
                            var db = Buy(decor.Def.Id);
                            if (!db.Ok) { fail = db; break; }
                            var dr = PlaceDecorFromInventory(decor.Def.Id, made.Id, decor.SlotId, out _);
                            if (!dr.Ok) { fail = dr; break; }
                        }
                        if (!fail.Ok) break;
                    }
                }
            }
            finally { EndGroup(); }
            if (!fail.Ok)
            {
                if (UndoPushes > before) Undo();
                return PlacementCheck.Fail($"Kopya kurulamadı: {fail.Reason}");
            }
            return PlacementCheck.Success;
        }
    }
}
