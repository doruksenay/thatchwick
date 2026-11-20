using System;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
namespace Thatchwick.Core.Tests
{
    /// <summary>Ev bakımı dengesi (belge 45 ek): örnek evlerle ölçülen sayılar test olarak sabitlenir.</summary>
    public class EvBakimiDengeTests
    {
        static void Room(WorldState w, int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++) { w.Map.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal), EdgeKind.Wall); w.Map.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = y0; y <= y1; y++) { w.Map.SetEdge(new Edge(x0, y, EdgeAxis.Vertical), EdgeKind.Wall); w.Map.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) w.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
        }
        static int Score(Action<ContentDatabase, WorldState, HomeLife> build)
        {
            var db = ContentFilesTests.LoadDb(out _);
            var w = new WorldState(new GridMap(40, 40));
            var house = new Household(0); var clock = new GameClock();
            var mood = new MoodService(w) { Content = db };
            var home = new HomeLife(w, db, house, clock) { BeautyOf = mood.BeautyOf };
            mood.RoomBeautyExtra = home.RoomBeautyExtra; mood.BeautyBonus = home.BeautyBonus;
            Room(w, 2, 2, 6, 5); Room(w, 8, 2, 11, 5); Room(w, 13, 2, 16, 5);
            build(db, w, home);
            return home.HomeScore();
        }
        static void P(ContentDatabase db, WorldState w, string id, int x, int y) => w.PlaceItem(db.Items.Get(id), new GridPos(x, y));
        static void Wl(ContentDatabase db, WorldState w, string id, int x, int y, int side = 0, int band = -1) => w.PlaceOnWall(db.Items.Get(id), new GridPos(x, y), side, band < 0 ? db.Items.Get(id).WallBands[0] : band);
        static void S(ContentDatabase db, WorldState w, ItemInstance on, string id) { var slot = on.Def.SurfaceSlots.FirstOrDefault(s => !s.IsCover && w.CanPlaceOnSurface(db.Items.Get(id), on.Id, s.Id).Ok); if (slot != null) w.PlaceOnSurface(db.Items.Get(id), on.Id, slot.Id); }

        [Fact]
        public void EvYarismasi_OrnekEvlerinPuani_KomsuEgrisiyleUyumlu()
        {
            int plain = Score((db, w, h) => { P(db, w, "item.bed_straw", 3, 3); P(db, w, "item.table_wood", 9, 3); P(db, w, "item.stool_wood", 9, 4); P(db, w, "item.hearth_stone", 14, 3); });
            int cared = Score((db, w, h) =>
            {
                P(db, w, "item.bed_frame", 3, 3); P(db, w, "item.rug_wool", 4, 4); P(db, w, "item.chest_wood", 6, 5); Wl(db, w, "item.painted_cloth", 3, 2); Wl(db, w, "item.wall_shelf", 5, 2);
                var t = w.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(9, 3)); S(db, w, t, "item.candle"); S(db, w, t, "item.jug"); S(db, w, t, "item.flower_pot");
                P(db, w, "item.kilim_large", 9, 4); Wl(db, w, "item.kilim_hanging", 8, 2); Wl(db, w, "item.harvest_wreath", 10, 2); P(db, w, "item.sedir", 11, 5);
                P(db, w, "item.hearth_stone", 14, 3); Wl(db, w, "item.plate_rack", 13, 2); P(db, w, "item.rush_mat_large", 14, 4); P(db, w, "item.hanging_lantern", 16, 5);
            });
            int rich = Score((db, w, h) =>
            {
                P(db, w, "item.canopy_bed_curtained", 3, 3); P(db, w, "item.rug_knotted", 4, 3); Wl(db, w, "item.tapestry_hunt", 3, 2); P(db, w, "item.carved_chest", 5, 5); P(db, w, "item.iron_chandelier", 6, 3);
                var t = w.PlaceItem(db.Items.Get("item.walnut_table"), new GridPos(9, 3)); S(db, w, t, "item.beeswax_candle"); S(db, w, t, "item.beeswax_candle"); S(db, w, t, "item.copper_pot");
                P(db, w, "item.rug_kufic", 8, 4); Wl(db, w, "item.tapestry_garden", 8, 2); Wl(db, w, "item.dynasty_banner", 10, 2, 0, Decor.BandHigh); P(db, w, "item.throne_chair", 9, 5); P(db, w, "item.jazari_peacock", 11, 5);
                P(db, w, "item.fireplace_large", 14, 3); Wl(db, w, "item.display_shelf", 13, 2); P(db, w, "item.caravan_rug", 14, 4); P(db, w, "item.water_clock", 16, 5); P(db, w, "item.pot_rose", 13, 5);
                P(db, w, "item.courtyard_pool", 20, 20); P(db, w, "item.sundial", 24, 20); P(db, w, "item.turf_seat", 26, 20); P(db, w, "item.dovecote", 20, 24);
            });
            // Sade ev hiç kazanamaz; özenli Zanaatkâr evi ilk yıl birinciliği zorlar; zengin Asil evi tavanı aşar (geç oyunda da kazanır).
            Assert.True(plain < HomeLife.NeighborBase, $"sade {plain}");
            Assert.InRange(cared, HomeLife.NeighborBase + HomeLife.NeighborSpread / 2, HomeLife.NeighborBase + HomeLife.NeighborSpread + 15);
            Assert.True(rich > HomeLife.NeighborCap, $"zengin {rich}");
        }

        [Fact]
        public void Mum_Is_Hizmetkar_Sureleri()
        {
            var db = ContentFilesTests.LoadDb(out _);
            double evening = HomeLife.EveningEnd - HomeLife.EveningStart;
            double Evenings(string id) => db.Items.Get(id).BurnDays / evening;
            Assert.InRange(Evenings("item.tallow_candle"), 5, 7);     // ucuz iç yağı ~6 akşam
            Assert.InRange(Evenings("item.candle"), 7, 9);            // mum ~8 akşam
            Assert.InRange(Evenings("item.beeswax_candle"), 11, 13);  // pahalı balmumu ~12 akşam
            // Mumun akşam başına bedeli yarım akçeyi geçmez.
            foreach (var id in new[] { "item.tallow_candle", "item.candle" }) Assert.True(db.Items.Get(id).Price / Evenings(id) <= 0.5, id);
            // Bacasız sürekli yanan ocakta is uyarısı 6-10 günde (mevsim 10 gün): mevsimde bir badana.
            Assert.InRange(HomeLife.SootWarnAt / HomeLife.SootPerDay, 6, 10);
            // Hizmetkârın ücreti bir ekmeğin yarısını geçmez (ortaçağda hizmet çoğunlukla sofra karşılığıydı).
            Assert.True(HomeLife.ServantWageAdult * 2 <= db.Items.Get("item.bread").Price);
        }
    }
}

