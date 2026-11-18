using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Sims düzeyinde yapı modu (17.11.2026): malzemeler, kaplamalar, zemin ve çatı stilleri, halı, mobilya kaplaması,
    // çizgi duvar, duvar itme, oda taşıma/kopyalama, balyoz, ev değeri, balkon, merdiven boşluğu, bodrum penceresi.
    public class YapiModuTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static (BuildService svc, WorldState w, Household h, ContentDatabase db) Setup(int akce = 5000, int size = 24)
        {
            var db = Db();
            var w = new WorldState(new GridMap(size, size));
            var h = new Household(akce);
            return (new BuildService(w, h, db), w, h, db);
        }

        // (x0,y0)-(x1,y1) karolarını çevreleyen duvarlar (tek geri alma adımı her kenar için ayrı değil: çizgi aracı).
        private static void Box(BuildService svc, int x0, int y0, int x1, int y1, int z = 0, string material = null)
        {
            Assert.True(svc.SetEdgeLine(x0, y0, x1 + 1, y0, z, EdgeKind.Wall, material, out _).Ok);
            Assert.True(svc.SetEdgeLine(x0, y1 + 1, x1 + 1, y1 + 1, z, EdgeKind.Wall, material, out _).Ok);
            Assert.True(svc.SetEdgeLine(x0, y0, x0, y1 + 1, z, EdgeKind.Wall, material, out _).Ok);
            Assert.True(svc.SetEdgeLine(x1 + 1, y0, x1 + 1, y1 + 1, z, EdgeKind.Wall, material, out _).Ok);
        }

        // ---------- katalog ----------

        [Fact]
        public void Katalog_HerTurunVarsayilaniVar()
        {
            var db = Db();
            foreach (var kind in new[] { BuildPartKind.WallMaterial, BuildPartKind.Door, BuildPartKind.Window, BuildPartKind.Fence, BuildPartKind.Gate, BuildPartKind.Railing, BuildPartKind.Roof })
            {
                var d = BuildParts.Default(db, kind);
                Assert.NotNull(d);
                Assert.True(d.IsDefault, $"{kind} varsayılanı işaretli değil");
                Assert.True(BuildParts.OfKind(db, kind).Count >= 2, $"{kind} için seçenek az");
            }
            Assert.Equal(BuildService.WallPrice, BuildParts.Default(db, BuildPartKind.WallMaterial).Price);
            Assert.Equal(BuildService.DoorPrice, BuildParts.Default(db, BuildPartKind.Door).Price);
            Assert.Equal(BuildService.WindowPrice, BuildParts.Default(db, BuildPartKind.Window).Price);
            foreach (var f in new[] { FloorType.Dirt, FloorType.Wood, FloorType.Stone })
                Assert.Equal(BuildService.FloorPrice(f), BuildParts.DefaultFloor(db, f).Price);
            Assert.True(BuildParts.OfKind(db, BuildPartKind.WallFinish).Count >= 5);
            foreach (var item in db.Items.All.Where(i => i.FinishGroup != null))
                Assert.True(BuildParts.FinishesFor(db, item).Count(p => p.IsDefault) == 1, item.Id);
            Assert.True(db.Items.All.Count(i => i.IsRug) >= 5);
            Assert.True(db.Items.All.Count(i => i.FinishGroup != null) >= 40);
            Assert.Equal(-1, db.Items.Get("item.cellar_hatch").StairsDelta);
        }

        // ---------- duvar, kapı, pencere, çit ----------

        [Fact]
        public void Duvar_MalzemeFiyatiVeDegisimi()
        {
            var (svc, w, h, db) = Setup();
            var e = new Edge(5, 5, EdgeAxis.Horizontal);
            int start = h.Akce;
            Assert.True(svc.SetEdge(e, EdgeKind.Wall).Ok);
            Assert.Equal(start - 3, h.Akce);
            Assert.Null(w.Map.GetEdgeStyle(e));                           // varsayılan kayda null yazılır
            Assert.True(svc.SetEdge(e, EdgeKind.Wall, "wall_stone").Ok);   // 8 - yarım iade 1
            Assert.Equal(start - 3 - 7, h.Akce);
            Assert.Equal("wall_stone", w.Map.GetEdgeStyle(e));
            Assert.False(svc.SetEdge(e, EdgeKind.Wall, "wall_stone").Ok);  // zaten öyle
            Assert.True(svc.Undo().Ok);
            Assert.Null(w.Map.GetEdgeStyle(e));
            Assert.Equal(start - 3, h.Akce);

            var half = new Edge(6, 5, EdgeAxis.Horizontal);
            int before = h.Akce;
            Assert.True(svc.SetEdge(half, EdgeKind.HalfWall, "wall_stone").Ok);
            Assert.Equal(before - 4, h.Akce);                                 // yarım duvar malzemenin yarısı
        }

        [Fact]
        public void Pencere_BodrumPenceresiYalnizcaBodrumda()
        {
            var (svc, w, h, db) = Setup();
            var e = new Edge(5, 5, EdgeAxis.Horizontal);
            Assert.True(svc.SetEdge(e, EdgeKind.Wall).Ok);
            var r = svc.SetEdge(e, EdgeKind.Window, "window_cellar");
            Assert.False(r.Ok);
            Assert.Contains("bodrumda", r.Reason);
            Assert.True(svc.SetEdge(e, EdgeKind.Window, "window_lattice").Ok);
            Assert.Equal(EdgeKind.Window, w.Map.GetEdge(e));
            Assert.Equal("window_lattice", w.Map.GetEdgeStyle(e));

            var low = new Edge(5, 5, EdgeAxis.Horizontal, -1);
            w.Map.SetTile(new GridPos(5, 5, -1), true);
            w.Map.SetTile(new GridPos(5, 4, -1), true);
            Assert.True(svc.SetEdge(low, EdgeKind.Wall).Ok);
            Assert.False(svc.SetEdge(low, EdgeKind.Window).Ok);                 // normal pencere bodrumda olmaz
            Assert.True(svc.SetEdge(low, EdgeKind.Window, "window_cellar").Ok);
        }

        [Fact]
        public void Kapi_PencereStiliVeKilitSeviyesi()
        {
            var (svc, w, h, db) = Setup();
            svc.AccountLevel = () => 1;
            var e = new Edge(3, 3, EdgeAxis.Vertical);
            Assert.True(svc.SetEdge(e, EdgeKind.Wall).Ok);
            var locked = svc.SetEdge(e, EdgeKind.Door, "door_arched");
            Assert.False(locked.Ok);
            Assert.Contains("seviye", locked.Reason);
            svc.AccountLevel = () => 20;
            Assert.True(svc.SetEdge(e, EdgeKind.Door, "door_arched").Ok);
            Assert.True(svc.SetEdge(new Edge(4, 4, EdgeAxis.Vertical), EdgeKind.Door).Ok == false);   // duvarsız kapı olmaz
        }

        [Fact]
        public void Cit_OdaYapmazAmaGecisiKeser_KapisiAcar()
        {
            var (svc, w, h, db) = Setup();
            // 3x3 alanı çitle çevir (2..4, 2..4).
            Assert.True(svc.SetEdgeLine(2, 2, 5, 2, 0, EdgeKind.Fence, null, out _).Ok);
            Assert.True(svc.SetEdgeLine(2, 5, 5, 5, 0, EdgeKind.Fence, null, out _).Ok);
            Assert.True(svc.SetEdgeLine(2, 2, 2, 5, 0, EdgeKind.Fence, "fence_picket", out _).Ok);
            Assert.True(svc.SetEdgeLine(5, 2, 5, 5, 0, EdgeKind.Fence, null, out _).Ok);
            var inside = new GridPos(3, 3);
            Assert.False(w.Map.Rooms.RoomAt(inside).IsIndoor);                         // çit oda yapmaz
            Assert.Null(Pathfinder.FindPath(w.Map, new GridPos(10, 10), inside));        // ama geçilmez
            Assert.True(svc.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Gate).Ok);
            Assert.NotNull(Pathfinder.FindPath(w.Map, new GridPos(10, 10), inside));     // çit kapısından girilir
            Assert.False(svc.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Window).Ok);   // çite pencere olmaz
        }

        [Fact]
        public void YarimDuvar_OdayiBolmez_KorkulukGecisiKeser()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 7, 5);
            Assert.True(svc.SetEdgeLine(5, 2, 5, 6, 0, EdgeKind.HalfWall, null, out _).Ok);   // odanın ortasında yarım duvar
            var a = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(a.IsIndoor);
            Assert.Same(a, w.Map.Rooms.RoomAt(new GridPos(6, 3)));                           // tek oda
            Assert.Null(Pathfinder.FindPath(w.Map, new GridPos(3, 3), new GridPos(6, 3)));    // yarım duvar geçilmez
            Assert.True(svc.SetEdge(new Edge(5, 4, EdgeAxis.Vertical), EdgeKind.None).Ok);
            Assert.NotNull(Pathfinder.FindPath(w.Map, new GridPos(3, 3), new GridPos(6, 3)));
        }

        [Fact]
        public void Duvar_EsyaninIcindenGecmez()
        {
            var (svc, w, h, db) = Setup();
            h.AddItem("item.bed_straw");
            Assert.True(svc.PlaceFromInventory("item.bed_straw", new GridPos(4, 4), 0, out _).Ok);   // 2x1: (4,4)-(5,4)
            var r = svc.SetEdge(new Edge(5, 4, EdgeAxis.Vertical), EdgeKind.Wall);
            Assert.False(r.Ok);
            Assert.Contains("içinden", r.Reason);
        }

        // ---------- kaplama ve zemin ----------

        [Fact]
        public void Kaplama_OdaninIcYuzleri_KonforaKatkiVerir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            var mood = new MoodService(w) { Content = db };
            Assert.Equal(0, mood.ComfortOf(room).Structure);
            int before = h.Akce;
            Assert.True(svc.PaintRoomWalls(room, "finish_tapestry", out int painted).Ok);
            Assert.Equal(12, painted);                                     // 3x3 odanın 12 iç yüzü
            Assert.Equal(before - 12 * 8, h.Akce);
            var north = new Edge(3, 2, EdgeAxis.Horizontal);
            Assert.Equal("finish_tapestry", w.Map.GetFinish(north, true));  // iç yüz (SideB odada)
            Assert.Null(w.Map.GetFinish(north, false));                    // dış yüz çıplak
            Assert.Equal(3, mood.ComfortOf(room).Structure);
            Assert.Equal(1, svc.UndoCount - 4);                              // dört duvar çizgisi + tek kaplama adımı
            Assert.True(svc.Undo().Ok);
            Assert.Null(w.Map.GetFinish(north, true));
            Assert.Equal(before, h.Akce);
            Assert.False(svc.PaintWallSide(new Edge(10, 10, EdgeAxis.Vertical), false, "finish_whitewash").Ok);   // duvar yok
        }

        [Fact]
        public void Zemin_StilFiyatGeriAlVeOdaZemini()
        {
            var (svc, w, h, db) = Setup();
            var t = new GridPos(3, 3);
            int before = h.Akce;
            Assert.True(svc.PaintFloorPart(t, "floor_oak").Ok);
            Assert.Equal(FloorType.Wood, w.Map.GetFloor(t));
            Assert.Equal("floor_oak", w.Map.GetFloorStyle(t));
            Assert.Equal(before - 4, h.Akce);
            Assert.True(svc.PaintFloor(t, FloorType.Wood).Ok);                 // varsayılan çam tahtaya dön
            Assert.Null(w.Map.GetFloorStyle(t));
            Assert.True(svc.Undo().Ok);
            Assert.Equal("floor_oak", w.Map.GetFloorStyle(t));

            Box(svc, 6, 6, 8, 7);
            var room = w.Map.Rooms.RoomAt(new GridPos(7, 7));
            Assert.True(svc.PaintRoomFloor(room, "floor_brick", out int n).Ok);
            Assert.Equal(6, n);
            Assert.All(room.Tiles, p => Assert.Equal(FloorType.Stone, w.Map.GetFloor(p)));
            Assert.True(svc.Undo().Ok);
            Assert.All(room.Tiles, p => Assert.Equal(FloorType.Grass, w.Map.GetFloor(p)));
        }

        // ---------- otomatik çatı ----------

        [Fact]
        public void Cati_OdaninUstundeBesikCati_UstKatOlunceYukariCikar()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 7, 4);                                       // 6x3 oda
            var roofs = RoofBuilder.Compute(w.Map);
            Assert.Single(roofs);
            var r = roofs[0];
            Assert.Equal(0, r.Level);
            Assert.True(r.RidgeAlongX);
            Assert.Equal(18, r.Tiles.Count);
            Assert.Equal(2, r.MaxHeight);
            Assert.Equal(1, r.Tiles.First(t => t.Pos == new GridPos(4, 2)).Height);    // saçak
            Assert.Equal(2, r.Tiles.First(t => t.Pos == new GridPos(4, 3)).Height);    // sırt
            Assert.True(r.Tiles.First(t => t.Pos == new GridPos(4, 3)).IsRidge);

            // Üst kat: odanın üstüne döşeme ve duvar; zemin katın çatısı kalkar, üst katın çatısı olur.
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 4; y++) Assert.True(svc.BuildLevelTile(new GridPos(x, y, 1)).Ok);
            Box(svc, 2, 2, 7, 4, 1);
            roofs = RoofBuilder.Compute(w.Map);
            Assert.Single(roofs);
            Assert.Equal(1, roofs[0].Level);

            // Stil: arduvaz karo başına 2; çatısız bırakınca kaybolur, yeniden açılabilir.
            int before = h.Akce;
            Assert.True(svc.SetRoofStyle(new GridPos(3, 3, 1), "roof_slate").Ok);
            Assert.Equal(before - 18 * 2, h.Akce);
            Assert.Equal("roof_slate", RoofBuilder.Compute(w.Map)[0].Style);
            Assert.True(svc.SetRoofStyle(new GridPos(3, 3, 1), GridMap.NoRoof).Ok);
            Assert.Empty(RoofBuilder.Compute(w.Map));
            Assert.True(svc.SetRoofStyle(new GridPos(3, 3, 1), "roof_thatch").Ok);
            Assert.Single(RoofBuilder.Compute(w.Map));
            Assert.False(svc.SetRoofStyle(new GridPos(15, 15), "roof_slate").Ok);   // çatı yok
        }

        // ---------- halı ve mobilya kaplaması ----------

        [Fact]
        public void Hali_YoluKapatmaz_UstuneMobilyaKonur_UstUsteSerilir()
        {
            var (svc, w, h, db) = Setup();
            h.AddItem("item.rug_wool", 2);
            h.AddItem("item.table_wood");
            Assert.True(svc.PlaceFromInventory("item.rug_wool", new GridPos(4, 4), 0, out var rug).Ok);
            Assert.True(w.Map.IsWalkable(new GridPos(4, 4)));
            Assert.Null(w.GetItemAt(new GridPos(4, 4)));
            Assert.Same(rug, w.GetRugAt(new GridPos(5, 5)));
            Assert.True(svc.PlaceFromInventory("item.table_wood", new GridPos(4, 4), 0, out _).Ok);   // halının üstüne masa
            // Karar 18.11.2026: halılar sınırsız üst üste serilir; yeni halı üstte.
            Assert.True(svc.PlaceFromInventory("item.rug_wool", new GridPos(5, 5), 0, out var top).Ok);
            Assert.Same(top, w.GetRugAt(new GridPos(5, 5)));
            Assert.Same(rug, w.GetRugAt(new GridPos(4, 4)));
            Assert.True(svc.StoreToInventory(top.Id).Ok);
            Assert.True(svc.Move(rug.Id, new GridPos(8, 8), 0).Ok);                                  // masanın altından da taşınır
            Assert.True(svc.SellPlaced(rug.Id).Ok);
            Assert.Null(w.GetRugAt(new GridPos(8, 8)));
            Assert.True(svc.Undo().Ok);
            Assert.NotNull(w.GetRugAt(new GridPos(8, 8)));
        }

        [Fact]
        public void MobilyaKaplamasi_FiyatFarki_SatisVeEnvanter()
        {
            var (svc, w, h, db) = Setup();
            h.AddItem("item.chair_carved");                                  // 30 akçe, wood grubu
            Assert.True(svc.PlaceFromInventory("item.chair_carved", new GridPos(4, 4), 0, out var chair).Ok);
            int before = h.Akce;
            Assert.True(svc.SetItemFinish(chair.Id, "wood_walnut").Ok);      // ×1,3 → 9 akçe
            Assert.Equal(before - 9, h.Akce);
            Assert.Equal("wood_walnut", chair.FinishId);
            Assert.False(svc.SetItemFinish(chair.Id, "cloth_madder").Ok);    // başka grup
            Assert.True(svc.SetItemFinish(chair.Id, "wood_natural").Ok);     // varsayılana dönüş: farkın yarısı iade
            Assert.Null(chair.FinishId);
            Assert.Equal(before - 9 + 4, h.Akce);
            Assert.True(svc.Undo().Ok);
            Assert.Equal("wood_walnut", chair.FinishId);

            int beforeSell = h.Akce;
            Assert.True(svc.SellPlaced(chair.Id).Ok);
            Assert.Equal(beforeSell + 15 + 4, h.Akce);                         // satış 15 + kaplama farkı 9'un yarısı
            Assert.True(svc.Undo().Ok);
            var back = w.Items.First(i => i.Def.Id == "item.chair_carved");
            Assert.Equal("wood_walnut", back.FinishId);                        // geri alınca kaplama da döner
            Assert.False(svc.SetItemFinish(w.PlaceItem(db.Items.Get("item.hearth_stone"), new GridPos(10, 10)).Id, "wood_dark").Ok);   // kaplamasız eşya
        }

        // ---------- araçlar ----------

        [Fact]
        public void CizgiDuvar_TekAdim_CaprazdaUzunEkseneOturur()
        {
            var (svc, w, h, db) = Setup();
            Assert.True(svc.SetEdgeLine(2, 2, 7, 3, 0, EdgeKind.Wall, "wall_timber", out int built).Ok);
            Assert.Equal(5, built);
            Assert.Equal(1, svc.UndoCount);
            for (int x = 2; x < 7; x++) Assert.Equal("wall_timber", w.Map.GetEdgeStyle(new Edge(x, 2, EdgeAxis.Horizontal)));
            Assert.True(svc.Undo().Ok);
            Assert.Empty(w.Map.Edges);
            Assert.False(svc.SetEdgeLine(2, 2, 7, 2, 0, EdgeKind.Door, null, out _).Ok);
        }

        [Fact]
        public void DuvarItme_OdaBuyurKuculur_KapiVeKaplamaHatlaGelir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4);                                                   // 3x3
            var south = new Edge(3, 5, EdgeAxis.Horizontal);
            Assert.True(svc.SetEdge(south, EdgeKind.Door, "door_ledged").Ok);
            Assert.True(svc.PaintWallSide(new Edge(2, 5, EdgeAxis.Horizontal), false, "finish_whitewash").Ok);
            Assert.True(svc.PushWall(south, 2).Ok);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(room.IsIndoor);
            Assert.Equal(15, room.Area);                                            // 3x5
            Assert.Equal(EdgeKind.Door, w.Map.GetEdge(new Edge(3, 7, EdgeAxis.Horizontal)));
            Assert.Equal("door_ledged", w.Map.GetEdgeStyle(new Edge(3, 7, EdgeAxis.Horizontal)));
            Assert.Equal("finish_whitewash", w.Map.GetFinish(new Edge(2, 7, EdgeAxis.Horizontal), false));
            Assert.Equal(EdgeKind.None, w.Map.GetEdge(south));
            Assert.True(svc.PushWall(new Edge(2, 7, EdgeAxis.Horizontal), -3).Ok);     // geri çek: 3x2
            Assert.Equal(6, w.Map.Rooms.RoomAt(new GridPos(3, 3)).Area);
            Assert.True(svc.Undo().Ok);
            Assert.True(svc.Undo().Ok);
            Assert.Equal(9, w.Map.Rooms.RoomAt(new GridPos(3, 3)).Area);
            Assert.Equal(EdgeKind.Door, w.Map.GetEdge(south));

            // Önünde başka duvar varsa itilmez.
            Assert.True(svc.SetEdge(new Edge(3, 7, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
            Assert.False(svc.PushWall(south, 2).Ok);
        }

        [Fact]
        public void Balyoz_AlandakiHerSeyiKaldirir_TekAdimdaGeriGelir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(svc.PaintRoomFloor(room, "floor_oak", out _).Ok);
            h.AddItem("item.chair_wood");
            h.AddItem("item.torch_wall");
            Assert.True(svc.PlaceFromInventory("item.chair_wood", new GridPos(3, 3), 0, out _).Ok);
            Assert.True(svc.PlaceWallFromInventory("item.torch_wall", new GridPos(3, 2), 0, out _).Ok);
            int value = svc.HouseValue();
            Assert.True(value > 0);
            int undoBefore = svc.UndoCount;
            Assert.True(svc.Demolish(2, 2, 4, 4, 0, out int removed, out int skipped).Ok);
            Assert.Equal(0, skipped);
            Assert.True(removed >= 12 + 9 + 2);
            Assert.Empty(w.Map.Edges);
            Assert.Empty(w.Items);
            Assert.Equal(FloorType.Grass, w.Map.GetFloor(new GridPos(3, 3)));
            Assert.True(svc.HouseValue() < value);
            Assert.Equal(undoBefore + 1, svc.UndoCount);
            Assert.True(svc.Undo().Ok);
            Assert.Equal(12, w.Map.Edges.Count());
            Assert.Equal(2, w.Items.Count);
            Assert.Equal("floor_oak", w.Map.GetFloorStyle(new GridPos(3, 3)));
        }

        [Fact]
        public void OdaTasima_HerSeyIleKayar_PaylasilanDuvarKalir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4, material: "wall_stone");
            Box(svc, 5, 2, 7, 4);                                                    // komşu oda, x=5 duvarı ortak
            Assert.True(svc.SetEdge(new Edge(3, 5, EdgeAxis.Horizontal), EdgeKind.Door).Ok);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(svc.PaintRoomFloor(room, "floor_cobble", out _).Ok);
            Assert.True(svc.PaintRoomWalls(room, "finish_whitewash", out _).Ok);
            h.AddItem("item.chair_wood"); h.AddItem("item.rug_rush"); h.AddItem("item.torch_wall");
            Assert.True(svc.PlaceFromInventory("item.chair_wood", new GridPos(3, 3), 1, out var chair).Ok);
            Assert.True(svc.PlaceFromInventory("item.rug_rush", new GridPos(2, 4), 0, out var rug).Ok);
            Assert.True(svc.PlaceWallFromInventory("item.torch_wall", new GridPos(2, 3), 3, out var torch).Ok);
            Assert.True(svc.SetItemFinish(chair.Id, "wood_red").Ok);

            room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            var r = svc.MoveRoom(room, 0, 8);
            Assert.True(r.Ok, r.Reason);
            var moved = w.Map.Rooms.RoomAt(new GridPos(3, 11));
            Assert.True(moved.IsIndoor);
            Assert.Equal(9, moved.Area);
            Assert.Equal(new GridPos(3, 11), w.GetItem(chair.Id).Position);
            Assert.Equal("wood_red", w.GetItem(chair.Id).FinishId);
            Assert.Equal(new GridPos(2, 12), w.GetItem(rug.Id).Position);
            Assert.Equal(new GridPos(2, 11), w.GetItem(torch.Id).Position);
            Assert.Equal("floor_cobble", w.Map.GetFloorStyle(new GridPos(3, 11)));
            Assert.Equal("wall_stone", w.Map.GetEdgeStyle(new Edge(3, 10, EdgeAxis.Horizontal)));
            Assert.Equal(EdgeKind.Door, w.Map.GetEdge(new Edge(3, 13, EdgeAxis.Horizontal)));
            Assert.Equal(FloorType.Grass, w.Map.GetFloor(new GridPos(3, 3)));
            Assert.Equal(EdgeKind.Wall, w.Map.GetEdge(new Edge(5, 3, EdgeAxis.Vertical)));          // ortak duvar yerinde
            Assert.True(w.Map.Rooms.RoomAt(new GridPos(6, 3)).IsIndoor);                         // komşu oda kapalı kaldı
            Assert.True(svc.Undo().Ok);
            Assert.Equal(new GridPos(3, 3), w.GetItem(chair.Id).Position);
            Assert.Equal(9, w.Map.Rooms.RoomAt(new GridPos(3, 3)).Area);
            Assert.Equal(EdgeKind.None, w.Map.GetEdge(new Edge(3, 10, EdgeAxis.Horizontal)));
            Assert.False(svc.MoveRoom(w.Map.Rooms.RoomAt(new GridPos(3, 3)), 3, 0).Ok);            // komşunun üstüne taşınmaz
        }

        [Fact]
        public void OdaKopyalama_YeniEsyalarlaKurulur_ParasiOdenir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4, material: "wall_timber");
            Assert.True(svc.SetEdge(new Edge(3, 5, EdgeAxis.Horizontal), EdgeKind.Window, "window_lattice").Ok);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(svc.PaintRoomWalls(room, "finish_panel", out _).Ok);
            Assert.True(svc.PaintRoomFloor(room, "floor_oak", out _).Ok);
            h.AddItem("item.table_wood"); h.AddItem("item.candle");
            Assert.True(svc.PlaceFromInventory("item.table_wood", new GridPos(3, 3), 0, out var table).Ok);
            Assert.True(svc.PlaceDecorFromInventory("item.candle", table.Id, table.Def.SurfaceSlots[0].Id, out _).Ok);
            int before = h.Akce, items = w.Items.Count;
            room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            var r = svc.CopyRoom(room, 10, 0, out int skipped);
            Assert.True(r.Ok, r.Reason);
            Assert.Equal(0, skipped);
            Assert.Equal(items * 2, w.Items.Count);
            var copy = w.Map.Rooms.RoomAt(new GridPos(13, 3));
            Assert.True(copy.IsIndoor);
            Assert.Equal("floor_oak", w.Map.GetFloorStyle(new GridPos(13, 3)));
            Assert.Equal("wall_timber", w.Map.GetEdgeStyle(new Edge(13, 2, EdgeAxis.Horizontal)));
            Assert.Equal("window_lattice", w.Map.GetEdgeStyle(new Edge(13, 5, EdgeAxis.Horizontal)));
            Assert.Equal("finish_panel", w.Map.GetFinish(new Edge(13, 2, EdgeAxis.Horizontal), true));
            Assert.True(h.Akce < before - 20 - 4);                                   // masa, mum, duvar, zemin, kaplama ödendi
            Assert.True(svc.Undo().Ok);                                               // tek adımda geri
            Assert.Equal(items, w.Items.Count);
            Assert.Equal(before, h.Akce);
            Assert.False(svc.CopyRoom(w.Map.Rooms.RoomAt(new GridPos(3, 3)), 1, 0, out _).Ok);   // kendi üstüne olmaz
        }

        // ---------- katlar ----------

        [Fact]
        public void Balkon_BirKaroCikmaOlur_IkiKaroOlmaz()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 4, 4);
            Assert.True(svc.BuildLevelTile(new GridPos(3, 4, 1)).Ok);
            Assert.True(svc.BuildLevelTile(new GridPos(3, 5, 1)).Ok);                // duvarın hemen dışı: duvar taşır
            Assert.True(svc.BuildLevelTile(new GridPos(3, 6, 1)).Ok);                // bir karo daha: balkon çıkması
            Assert.False(svc.BuildLevelTile(new GridPos(3, 7, 1)).Ok);               // daha fazlası olmaz
            Assert.False(w.Map.Rooms.RoomAt(new GridPos(3, 6, 1)).IsIndoor);
            Assert.True(svc.SetEdge(new Edge(3, 7, EdgeAxis.Horizontal, 1), EdgeKind.Railing, "railing_turned").Ok);
            Assert.False(svc.SetEdge(new Edge(3, 7, EdgeAxis.Horizontal, 1), EdgeKind.Railing, "railing_turned").Ok);
            Assert.Null(Pathfinder.FindPath(w.Map, new GridPos(3, 6, 1), new GridPos(3, 7, 1)));   // korkuluk geçilmez (karo da yok)
        }

        [Fact]
        public void MerdivenBoslugu_UstundekiDoseme_KesilirVeOdaIcerdeKalir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 8, 8);
            for (int x = 2; x <= 8; x++) for (int y = 2; y <= 8; y++) Assert.True(svc.BuildLevelTile(new GridPos(x, y, 1)).Ok);
            Box(svc, 2, 2, 8, 8, 1);
            Assert.True(w.Map.Rooms.RoomAt(new GridPos(5, 5, 1)).IsIndoor);
            h.AddItem("item.stairs_up");
            int before = h.Akce;
            var r = svc.PlaceFromInventory("item.stairs_up", new GridPos(4, 4), 0, out var stairs);   // ayak izi (4,4)-(4,5), iniş (4,6,1)
            Assert.True(r.Ok, r.Reason);
            Assert.False(w.Map.HasTile(new GridPos(4, 4, 1)));
            Assert.False(w.Map.HasTile(new GridPos(4, 5, 1)));
            Assert.True(w.Map.HasTile(new GridPos(4, 6, 1)));
            Assert.True(h.Akce > before);                                             // kesilen döşemenin yarısı iade
            Assert.True(w.Map.Rooms.RoomAt(new GridPos(6, 6, 1)).IsIndoor);          // boşluk odayı dışarı açmaz
            Assert.False(svc.BuildLevelTile(new GridPos(4, 4, 1)).Ok);               // boşluğa döşeme yapılmaz
            Assert.NotNull(Pathfinder.FindPath(w.Map, new GridPos(4, 3), new GridPos(6, 6, 1)));
            Assert.True(svc.Undo().Ok);                                               // merdiven ve boşluk tek adımda
            Assert.True(w.Map.HasTile(new GridPos(4, 4, 1)));
            Assert.Null(w.GetItem(stairs.Id));
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_StillerKaplamalarHaliVeCatiGeriGelir()
        {
            var (svc, w, h, db) = Setup();
            Box(svc, 2, 2, 5, 4, material: "wall_stone");
            Assert.True(svc.SetEdge(new Edge(3, 5, EdgeAxis.Horizontal), EdgeKind.Door, "door_iron").Ok);
            var room = w.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.True(svc.PaintRoomWalls(room, "finish_ochre", out _).Ok);
            Assert.True(svc.PaintRoomFloor(room, "floor_herringbone", out _).Ok);
            Assert.True(svc.SetRoofStyle(new GridPos(3, 3), "roof_tile").Ok);
            Assert.True(svc.SetEdgeLine(8, 8, 12, 8, 0, EdgeKind.Fence, "fence_hedge", out _).Ok);
            h.AddItem("item.rug_wool"); h.AddItem("item.chair_wood");
            Assert.True(svc.PlaceFromInventory("item.rug_wool", new GridPos(3, 3), 0, out _).Ok);
            Assert.True(svc.PlaceFromInventory("item.chair_wood", new GridPos(3, 3), 0, out var chair).Ok);
            Assert.True(svc.SetItemFinish(chair.Id, "wood_blue").Ok);

            var clock = new GameClock();
            var data = SaveGame.Capture(clock, w, h, new List<Character>(), new Progress.AccountProgress(), null, null);
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var back = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var loaded = SaveGame.Load(back, db);
            var m = loaded.World.Map;
            Assert.Equal("wall_stone", m.GetEdgeStyle(new Edge(3, 2, EdgeAxis.Horizontal)));
            Assert.Equal("door_iron", m.GetEdgeStyle(new Edge(3, 5, EdgeAxis.Horizontal)));
            Assert.Equal("finish_ochre", m.GetFinish(new Edge(3, 2, EdgeAxis.Horizontal), true));
            Assert.Equal("floor_herringbone", m.GetFloorStyle(new GridPos(4, 3)));
            Assert.Equal("roof_tile", RoofBuilder.Compute(m)[0].Style);
            Assert.Equal("fence_hedge", m.GetEdgeStyle(new Edge(9, 8, EdgeAxis.Horizontal)));
            Assert.Equal(EdgeKind.Fence, m.GetEdge(new Edge(9, 8, EdgeAxis.Horizontal)));
            Assert.NotNull(loaded.World.GetRugAt(new GridPos(4, 4)));
            Assert.Equal("wood_blue", loaded.World.Items.First(i => i.Def.Id == "item.chair_wood").FinishId);
            Assert.True(m.IsWalkable(new GridPos(4, 4)));                              // halı yüklemede de yolu kapatmaz
        }
    }
}
