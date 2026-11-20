using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Layout;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Çekirdek performans ölçümü (ikinci inceleme, 20.11.2026): gerçek başlangıç dünyası (StarterWorld), 30 karakter,
    /// 3x hız, 2 oyun günü. Test değildir; `tools/offline-tests/bench.sh` ile Release derlemesinde çalışır.
    /// Sayılar makineye göre değişir ve tekrarlar arasında oynar: aynı makinede önce/sonra karşılaştırması içindir.
    /// </summary>
    public static class Bench
    {
        static double Ms(Action a, int n) { a(); var sw = Stopwatch.StartNew(); for (int i = 0; i < n; i++) a(); return sw.Elapsed.TotalMilliseconds / n; }
        static long Alloc(Action a, int n) { a(); long b = GC.GetAllocatedBytesForCurrentThread(); for (int i = 0; i < n; i++) a(); return (GC.GetAllocatedBytesForCurrentThread() - b) / n; }

        public static void Run()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var swb = Stopwatch.StartNew();
            var w = new WorldState(StarterWorld.BuildMap(lay));
            double tMap = swb.Elapsed.TotalMilliseconds; swb.Restart();
            StarterWorld.Place(w, db, lay, 1);
            Console.WriteLine($"BENCH new world: BuildMap {tMap:0} ms, Place {swb.Elapsed.TotalMilliseconds:0} ms");
            var house = new Household();
            foreach (var d in db.Items.All.Where(d => d.SpoilDays > 0).Take(40)) house.AddItem(d.Id, 5);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, w, house, pop, clock);
            var plants = new PlantSystem(w);
            life.Attach(plants);
            var heat = new HeatSystem(w, clock.Config);
            var farm = Regions.Farm;
            var runners = new List<JobRunner>();
            var walk = new List<GridPos>();
            for (int y = farm.Y; y < farm.Y + farm.H; y++) for (int x = farm.X; x < farm.X + farm.W; x++) { var p = new GridPos(x, y); if (w.Map.IsWalkable(p)) walk.Add(p); }
            for (int i = 0; i < 30; i++)
            {
                var c = new Character(i + 1, "K" + i, walk[(i * 97) % walk.Count], i < 5);
                c.SetAge(30);
                pop.Add(c);
                runners.Add(new JobRunner(c, w) { Autonomous = true });
            }
            Console.WriteLine($"BENCH world {w.Map.Width}x{w.Map.Height} items={w.Items.Count} spoilableDefs={db.Items.All.Count(d => d.SpoilDays > 0)} defs={db.Items.All.Count()} chars={pop.Members.Count}");

            double dt60 = 1.0 / 60 / clock.Config.RealSecondsPerGameDay;   // 60 FPS, 1x
            Console.WriteLine($"BENCH life.Tick           {Ms(() => life.Tick(dt60), 300):0.000} ms  alloc {Alloc(() => life.Tick(dt60), 100)} B");
            Console.WriteLine($"BENCH  spoilage.Tick      {Ms(() => life.Spoilage.Tick(dt60), 300):0.000} ms  alloc {Alloc(() => life.Spoilage.Tick(dt60), 100)} B");
            Console.WriteLine($"BENCH  decor.Tick         {Ms(() => life.Decor.Tick(dt60), 300):0.000} ms");
            Console.WriteLine($"BENCH  requests.Tick      {Ms(() => life.Requests.Tick(), 300):0.000} ms  alloc {Alloc(() => life.Requests.Tick(), 100)} B");
            Console.WriteLine($"BENCH  home.Tick          {Ms(() => life.Home.Tick(dt60), 300):0.000} ms  alloc {Alloc(() => life.Home.Tick(dt60), 100)} B");
            Console.WriteLine($"BENCH plants.Tick         {Ms(() => plants.Tick(dt60, Season.Spring), 300):0.000} ms  alloc {Alloc(() => plants.Tick(dt60, Season.Spring), 100)} B");
            Console.WriteLine($"BENCH heat.Tick           {Ms(() => heat.Tick(dt60, 0.5), 300):0.000} ms  alloc {Alloc(() => heat.Tick(dt60, 0.5), 100)} B");
            Console.WriteLine($"BENCH pop.Tick            {Ms(() => pop.Tick(dt60), 300):0.000} ms");

            // Otonomi: ihtiyaçlar eşiğin altında.
            foreach (var c in pop.Members) { c.Needs.Set(NeedType.Fun, 20f); c.Needs.Set(NeedType.Hygiene, 20f); c.Needs.Set(NeedType.Social, 20f); }
            double tAuto = Ms(() => { foreach (var r in runners) r.Tick(dt60); }, 200);
            long aAuto = Alloc(() => { foreach (var r in runners) r.Tick(dt60); }, 50);
            Console.WriteLine($"BENCH 30 runners.Tick     {tAuto:0.000} ms  alloc {aAuto} B  (idle={runners.Count(r => r.IsIdle)})");

            var ids = w.Items.Select(i => i.Id).ToArray();
            Console.WriteLine($"BENCH GetItem x{ids.Length}      {Ms(() => { foreach (var id in ids) w.GetItem(id); }, 20):0.000} ms");
            Console.WriteLine($"BENCH GetItemAt x{walk.Count}   {Ms(() => { foreach (var p in walk) w.GetItemAt(p); }, 3):0.000} ms");

            var town = Regions.All.FirstOrDefault(r => r != farm);
            GridPos a = walk[0], b = walk[walk.Count - 1];
            var path = Pathfinder.FindPath(w.Map, a, b);
            Console.WriteLine($"BENCH path farm corner→corner len={(path?.Count ?? -1)}  {Ms(() => Pathfinder.FindPath(w.Map, a, b), 20):0.000} ms  alloc {Alloc(() => Pathfinder.FindPath(w.Map, a, b), 5)} B");
            GridPos far = default; bool found = false;
            for (int x = w.Map.Width - 1; x > 0 && !found; x--) for (int y = 0; y < w.Map.Height; y++) { var p = new GridPos(x, y); if (w.Map.IsWalkable(p) && Pathfinder.FindPath(w.Map, a, p) != null) { far = p; found = true; break; } }
            if (found) { var pf = Pathfinder.FindPath(w.Map, a, far); Console.WriteLine($"BENCH path farm→far east len={pf.Count}  {Ms(() => Pathfinder.FindPath(w.Map, a, far), 5):0.000} ms  alloc {Alloc(() => Pathfinder.FindPath(w.Map, a, far), 2)} B"); }
            // Ulaşılamayan hedef: kapalı bir cep ara (yürünür ama yol yok).
            GridPos? island = null;
            foreach (var p in walk) if (Pathfinder.FindPath(w.Map, a, p) == null) { island = p; break; }
            if (island.HasValue) Console.WriteLine($"BENCH path unreachable     {Ms(() => Pathfinder.FindPath(w.Map, a, island.Value), 5):0.000} ms  alloc {Alloc(() => Pathfinder.FindPath(w.Map, a, island.Value), 2)} B");
            else Console.WriteLine("BENCH path unreachable     (none found on farm)");

            // Duvar değişimi: bütün dünyanın oda haritası yeniden hesaplanır.
            var wallAt = new GridPos(farm.X + 40, farm.Y + 40);
            bool flip = false;
            double tRooms = Ms(() => { flip = !flip; w.Map.SetEdge(GridMap.EdgeBetween(wallAt, new GridPos(wallAt.X + 1, wallAt.Y)), flip ? EdgeKind.Wall : EdgeKind.None); var _ = w.Map.Rooms; }, 20);
            Console.WriteLine($"BENCH wall edit → rooms   {tRooms:0.00} ms  alloc {Alloc(() => { flip = !flip; w.Map.SetEdge(GridMap.EdgeBetween(wallAt, new GridPos(wallAt.X + 1, wallAt.Y)), flip ? EdgeKind.Wall : EdgeKind.None); var _ = w.Map.Rooms; }, 3)} B  rooms={w.Map.Rooms.Rooms.Count}");

            // Gün dönümü: en uzun adım.
            double worst = 0, sum = 0; int n = 0; double dayStart = clock.TotalDays;
            var parts = new double[5]; var worstPart = new double[5];
            var swd = new Stopwatch();
            double hz = double.Parse(Environment.GetEnvironmentVariable("BENCH_HZ") ?? "10", System.Globalization.CultureInfo.InvariantCulture);   // oyun saniyede 10 adım işler
            long gc0 = GC.CollectionCount(0); long al0 = GC.GetAllocatedBytesForCurrentThread();
            while (clock.TotalDays < dayStart + 2.05)
            {
                double gd = clock.Advance(1.0 / hz * 3);   // 3x hız
                double total = 0;
                void T(int k, Action a) { swd.Restart(); a(); double ms = swd.Elapsed.TotalMilliseconds; parts[k] += ms; worstPart[k] = Math.Max(worstPart[k], ms); total += ms; }
                T(0, () => heat.Tick(gd, clock.TotalDays)); T(1, () => plants.Tick(gd, clock.Season)); T(2, () => pop.Tick(gd)); T(3, () => life.Tick(gd));
                T(4, () => { foreach (var r in runners) r.Tick(gd); });
                worst = Math.Max(worst, total); sum += total; n++;
            }
            double realSec = n / hz;
            Console.WriteLine($"BENCH 2 days @3x {hz}Hz: steps={n} avg {sum / n:0.000} ms/step, worst {worst:0.0} ms, per real second {sum / realSec:0.0} ms, alloc/s {(GC.GetAllocatedBytesForCurrentThread() - al0) / realSec / 1024:0} KB, gen0 {GC.CollectionCount(0) - gc0}");
            string[] names = { "heat", "plants", "pop", "life", "runners" };
            for (int k = 0; k < 5; k++) Console.WriteLine($"BENCH   {names[k],-8} avg {parts[k] / n:0.000} ms  worst {worstPart[k]:0.0} ms");

            var swc = Stopwatch.StartNew();
            ContentFilesTests.LoadDb(out _);
            Console.WriteLine($"BENCH content load        {swc.Elapsed.TotalMilliseconds:0} ms");

            var chars = pop.Members.ToList();
            SaveData data = null;
            double tCap = Ms(() => data = SaveGame.Capture(clock, w, house, chars, null, heat, null, life: life, plants: plants), 10);
            var opt = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
            string json = System.Text.Json.JsonSerializer.Serialize(data, opt);
            double tJson = Ms(() => System.Text.Json.JsonSerializer.Serialize(data, opt), 5);
            string mapJson = System.Text.Json.JsonSerializer.Serialize(data.map, opt);
            foreach (var f in typeof(SaveData).GetFields().Select(f => (f.Name, len: System.Text.Json.JsonSerializer.Serialize(f.GetValue(data), opt).Length)).OrderByDescending(x => x.len).Take(6))
                Console.WriteLine($"BENCH   save.{f.Name} {f.len / 1024} KB");
            foreach (var f in typeof(MapSave).GetFields().Select(f => (f.Name, len: System.Text.Json.JsonSerializer.Serialize(f.GetValue(data.map), opt).Length)).OrderByDescending(x => x.len).Take(6))
                Console.WriteLine($"BENCH   map.{f.Name} {f.len / 1024} KB");
            var back = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json, opt);
            var swl = Stopwatch.StartNew();
            var loaded = SaveGame.Load(back, db);
            Console.WriteLine($"BENCH save load (SaveGame.Load) {swl.Elapsed.TotalMilliseconds:0} ms, items={loaded.World.Items.Count}");
            Console.WriteLine($"BENCH save capture        {tCap:0.0} ms; json {json.Length / 1024} KB (map {mapJson.Length / 1024} KB), serialize {tJson:0.0} ms (System.Text.Json)");
        }
    }
}
