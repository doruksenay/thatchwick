using System.Text;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Denge 41: tarla görevlisi başlangıç bostanını hasat edip yeniden eker (gerçek içerikle 12 günlük benzetim).</summary>
    public class DengeTarlaSimTests
    {
        [Fact]
        public void TarlaGorevlisi_BostaniHasatEdipYenidenEker()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            var map = new GridMap(30, 20);
            var world = new WorldState(map);
            var house = new Household(100);
            var build = new BuildService(world, house, db);
            var plants = new PlantSystem(world, 3);
            build.Plants = plants;
            var fx = new InteractionEffects(world, house, db, seed: 1) { Plants = plants };
            var work = new FarmWork(world) { House = house, Content = db, Build = build, Plants = plants, Season = () => Season.Spring };
            var planner = new DutyPlanner(world, null, fx, null) { TileWork = work };
            StartingKit.TillBed(world, new GridPos(10, 5), ditch: db.Items.Get("item.irrigation_ditch"));
            world.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(3, 3));
            world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 8));
            house.AddItem("item.seed_turnip", 12);
            house.AddItem("item.seed_radish", 6);
            house.AddItem("item.turnip", 60);
            var c = new Character(1, "Maud", new GridPos(5, 5));
            c.SetAge(30);
            c.Duty = DutyKind.Farm;
            var r = new JobRunner(c, world) { Autonomous = true, Effects = fx, TileWork = work, DutyProvider = planner.Next };
            var log = new StringBuilder();
            int harvested = 0, sown = 0;
            work.Sown += (ch, t, s) => sown++;
            r.InteractionCompleted += (ch, i) => { if (i.Id == "harvest") harvested++; };
            const double dt = 0.01;
            double t = 0;
            int emptySamples = 0, samples = 0;
            for (int day = 0; day < 12; day++)
            {
                if (day > 0) plants.OnNewDay(rained: false);
                for (int k = 0; k < 100; k++) { c.Tick(dt); r.Tick(dt); plants.Tick(dt, Season.Spring); t += dt; }
                int crops = 0, empty = 0;
                for (int y = 5; y < 8; y++)
                    for (int x = 10; x < 16; x++)
                    {
                        var p = new GridPos(x, y);
                        if (map.GetFloor(p) != FloorType.Dirt) continue;
                        if (world.GetItemAt(p) == null) empty++; else crops++;
                    }
                if (day >= 2) { samples++; if (empty > 4) emptySamples++; }
                log.Append($"D{day} crops={crops} empty={empty} seedT={house.Count("item.seed_turnip")} seedR={house.Count("item.seed_radish")} sown={sown} harv={harvested} fail={r.LastFailure}\n");
            }
            Assert.True(harvested >= 12, log.ToString());
            Assert.True(emptySamples <= samples / 3, log.ToString());
        }
    }
}
