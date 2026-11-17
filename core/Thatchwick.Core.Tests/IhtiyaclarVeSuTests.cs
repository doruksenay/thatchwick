using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Tuvalet ihtiyacı, hela dolumu ve gübre, banyo için kovayla su (bir sefer), ıslaklık, üşüme ve su karoları.
    public class IhtiyaclarVeSuTests
    {
        private static Character Adult(int id = 1, GridPos? at = null)
        {
            var c = new Character(id, "Wat", at ?? new GridPos(1, 1)) { MoodEnabled = true };
            c.SetAge(30);
            return c;
        }

        private static ContentDatabase Db()
        {
            var db = WorkshopTests.RealContent(out var errors);
            Assert.Empty(errors);
            return db;
        }

        private static InteractionDef Inter(ItemInstance item, string id)
        {
            foreach (var i in item.Def.Interactions) if (i.Id == id) return i;
            throw new KeyNotFoundException(id);
        }

        // İşi bitene kadar koşturur.
        private static void RunUntilIdle(Character c, JobRunner r, int maxSteps = 4000)
        {
            for (int i = 0; i < maxSteps && r.Queue.Current != null; i++) { c.Tick(0.002); r.Tick(0.002); }
        }

        // ---------- tuvalet ----------

        [Fact]
        public void Tuvalet_AzalirVeYetisemeyenKazaYapar()
        {
            var c = Adult();
            Assert.True(NeedKinds.IsUrgent(NeedType.Bladder));
            float before = c.Needs.Get(NeedType.Bladder);
            c.Tick(0.1);
            Assert.True(c.Needs.Get(NeedType.Bladder) < before);

            int accidents = 0;
            c.Accident += _ => accidents++;
            c.Needs.Set(NeedType.Bladder, 0.5f);
            c.Tick(0.01);
            Assert.Equal(1, accidents);
            Assert.True(c.Needs.Get(NeedType.Bladder) > NeedSet.Max - 1f);
            Assert.True(c.Needs.Get(NeedType.Hygiene) < 1f);
            Assert.True(c.Mood.Has(MoodCatalog.Accident));
        }

        [Fact]
        public void Tuvalet_SikisinceSikismaMoodletiGelir()
        {
            var c = Adult();
            c.Needs.SetDecayPerDay(NeedType.Bladder, 0f);
            c.Needs.Set(NeedType.Bladder, 10f);
            c.Tick(0.01);
            Assert.True(c.Mood.Has(MoodCatalog.Bursting));
        }

        [Fact]
        public void Hela_DolarBosalincaGubreVerir_DoluysaKullananBosaltir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(0);
            var fx = new InteractionEffects(world, house, db);
            var pot = world.PlaceItem(db.Items.Get("item.chamber_pot"), new GridPos(3, 3));
            var use = Inter(pot, "use");
            var empty = Inter(pot, "empty");
            var c = Adult();
            Assert.NotNull(fx.CanStart(c, pot, empty));          // boş lazımlık boşaltılmaz
            for (int i = 0; i < pot.Def.FillCapacity; i++)
            {
                Assert.Null(fx.CanStart(c, pot, use));
                fx.Apply(c, pot, use);
            }
            Assert.Equal(pot.Def.FillCapacity, pot.Fill);
            fx.Apply(c, pot, empty);
            Assert.Equal(0, pot.Fill);
            Assert.Equal(1, house.Count("item.manure"));

            // Dolu lazımlığı kullanan önce boşaltır: sıkışan beklemez.
            pot.Fill = pot.Def.FillCapacity;
            Assert.Null(fx.CanStart(c, pot, use));
            fx.Apply(c, pot, use);
            Assert.Equal(1, pot.Fill);
            Assert.Equal(2, house.Count("item.manure"));

            // Gübre tarlaya serilir.
            var field = db.Items.Get("item.outhouse");
            Assert.Equal("item.manure", field.FillProduct);
        }

        [Fact]
        public void Hela_GorevliDolmakUzereyiBosaltir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var house = new Household(0);
            var fx = new InteractionEffects(world, house, db);
            var oh = world.PlaceItem(db.Items.Get("item.outhouse"), new GridPos(5, 5));
            var duties = new DutyPlanner(world, null, fx, null);
            var c = Adult(at: new GridPos(1, 1));
            c.Duty = DutyKind.Farm;
            Assert.Null(duties.Next(c));
            oh.Fill = (int)(oh.Def.FillCapacity * DutyPlanner.EmptyAtFill) + 1;
            var job = duties.Next(c);
            Assert.NotNull(job);
            Assert.Equal(oh.Id, job.ItemId);
            Assert.Equal("empty_pit", job.InteractionId);
        }

        [Fact]
        public void Icmek_TuvaletiDoldurur()
        {
            var db = Db();
            int drinks = 0, meals = 0;
            foreach (var item in db.Items.All)
                foreach (var i in item.Interactions)
                {
                    if (i.BladderCost >= 15f) drinks++;
                    else if (i.BladderCost > 0f) meals++;
                }
            Assert.True(drinks >= 3, $"içecek {drinks}");
            Assert.True(meals >= 3, $"yemek {meals}");
        }

        // ---------- banyo: bir kova su ----------

        [Fact]
        public void Banyo_OnceKuyudanBirKovaSuGetirirSonraYikanir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(20, 20));
            var house = new Household(0);
            var fx = new InteractionEffects(world, house, db);
            world.PlaceItem(db.Items.Get("item.well"), new GridPos(15, 15));
            var tub = world.PlaceItem(db.Items.Get("item.wooden_tub"), new GridPos(3, 3));
            var wash = Inter(tub, "bath");
            Assert.True(wash.FetchWater);

            var c = Adult(at: new GridPos(2, 2));
            c.Needs.Set(NeedType.Hygiene, 10f);
            var r = new JobRunner(c, world) { Effects = fx, WaterNetworkAccess = () => WaterNetwork.WaterAccess(world) };
            r.Command(Job.UseItem(tub.Id, wash.Id));
            bool fetched = false, nearWell = false;
            var access = WaterNetwork.WaterAccess(world);
            for (int i = 0; i < 40000 && r.Queue.Current != null; i++)
            {
                c.Tick(0.0005); r.Tick(0.0005);        // yarım karo: kuyu başında durduğu an görünsün
                fetched |= r.IsFetchingWater;
                nearWell |= access.Contains(c.Position);
            }
            Assert.True(fetched);
            Assert.True(nearWell, "kuyuya gitmedi");
            Assert.True(c.Needs.Get(NeedType.Hygiene) > 50f);
            Assert.Null(r.LastFailure);
        }

        [Fact]
        public void Banyo_SuYoksaBaslamaz()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var fx = new InteractionEffects(world, new Household(0), db);
            var hut = world.PlaceItem(db.Items.Get("item.bath_hut"), new GridPos(3, 3));
            var c = Adult(at: new GridPos(1, 1));
            var r = new JobRunner(c, world) { Effects = fx, WaterNetworkAccess = () => WaterNetwork.WaterAccess(world) };
            r.Command(Job.UseItem(hut.Id, "wash"));
            RunUntilIdle(c, r);
            Assert.Contains("su yok", r.LastFailure);
        }

        [Fact]
        public void Banyo_GoletKiyisindanSuAlinir()
        {
            var map = new GridMap(10, 10);
            map.SetFloor(new GridPos(5, 5), FloorType.Water);
            var world = new WorldState(map);
            var goals = WaterNetwork.WaterAccess(world);
            Assert.Contains(new GridPos(4, 5), goals);
            Assert.DoesNotContain(new GridPos(5, 5), goals);       // suya girmeden kıyıdan
        }

        [Fact]
        public void Banyo_SogukSuUsutur_OcakYananOdadaSicakBanyo()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var fx = new InteractionEffects(world, new Household(0), db) { TemperatureAt = _ => 2f };
            var tub = world.PlaceItem(db.Items.Get("item.wooden_tub"), new GridPos(3, 3));
            var wash = Inter(tub, "bath");
            var c = Adult();
            float warm = c.Needs.Get(NeedType.Warmth);
            fx.Apply(c, tub, wash);
            Assert.True(c.Needs.Get(NeedType.Warmth) < warm);
            Assert.True(c.Mood.Has(MoodCatalog.ColdWater));
        }

        // ---------- ıslaklık ve üşüme ----------

        [Fact]
        public void Islaklik_SoguktaUsutur_SicaktaKurur()
        {
            var cold = Adult(1);
            cold.AmbientTemperature = 0f;
            cold.Soak(1f);
            Assert.True(cold.Mood.Has(MoodCatalog.Wet) || cold.Wetness > 0.9f);
            for (int i = 0; i < 30; i++) cold.Tick(0.01);
            Assert.True(cold.Mood.Has(MoodCatalog.Chilled), "üşümedi");
            Assert.True(cold.Mood.Has(MoodCatalog.Wet));

            var warm = Adult(2);
            warm.AmbientTemperature = 25f;
            warm.Soak(1f);
            for (int i = 0; i < 30; i++) warm.Tick(0.01);
            Assert.True(warm.Wetness < 0.01f);
            Assert.False(warm.Mood.Has(MoodCatalog.Chilled));
        }

        [Fact]
        public void Islaklik_IsinmayiHizliEritir()
        {
            var dry = Adult(1); var wet = Adult(2);
            dry.AmbientTemperature = wet.AmbientTemperature = -5f;
            wet.Soak(1f);
            dry.Tick(0.05); wet.Tick(0.05);
            Assert.True(wet.Needs.Get(NeedType.Warmth) < dry.Needs.Get(NeedType.Warmth));
        }

        [Fact]
        public void Havlu_Kurular_TedaviUsumeyiGiderir()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var fx = new InteractionEffects(world, new Household(0), db);
            var rack = world.PlaceItem(db.Items.Get("item.towel_rack"), new GridPos(3, 3));
            var c = Adult();
            c.Soak(1f);
            fx.Apply(c, rack, Inter(rack, "dry"));
            Assert.Equal(0f, c.Wetness);
            c.AddMoodlet(MoodCatalog.Chilled);
            Assert.True(c.Treat(10f));
            Assert.False(c.Mood.Has(MoodCatalog.Chilled));
        }

        // ---------- su karoları ----------

        [Fact]
        public void SuKarosu_YavasIslatirDonunceUstundenYurunur()
        {
            var map = new GridMap(10, 10);
            var p = new GridPos(4, 4);
            map.SetFloor(p, FloorType.Water);
            Assert.True(map.HasWater);
            Assert.True(map.IsOpenWater(p));
            Assert.True(System.Math.Abs(GridMap.WaterSpeed - map.WalkSpeedAt(p)) < 0.001);
            map.WaterFrozen = true;
            Assert.False(map.IsOpenWater(p));
            Assert.True(map.IsWater(p));
            map.SetFloor(p, FloorType.Grass);
            Assert.False(map.HasWater);

            // Yürürken suya giren ıslanır.
            var m2 = new GridMap(10, 3);
            for (int x = 2; x < 8; x++) for (int y = 0; y < 3; y++) m2.SetFloor(new GridPos(x, y), FloorType.Water);
            var world = new WorldState(m2);
            var c = Adult(at: new GridPos(0, 1));
            var r = new JobRunner(c, world);
            r.Command(Job.MoveTo(new GridPos(9, 1)));
            RunUntilIdle(c, r);
            Assert.Equal(new GridPos(9, 1), c.Position);
            Assert.True(c.Wetness > 0.3f);
        }

        [Fact]
        public void SuKarosu_EsyaKonmaz_KaydaGirer()
        {
            var db = Db();
            var map = new GridMap(10, 10);
            map.SetFloor(new GridPos(4, 4), FloorType.Water);
            var world = new WorldState(map);
            Assert.False(world.CanPlaceFloor(db.Items.Get("item.chest_wood"), new GridPos(4, 4), 0).Ok);
        }

        [Fact]
        public void SuKarosu_YikanBalikTutBuzKes()
        {
            var db = Db();
            var map = new GridMap(10, 10);
            var water = new GridPos(5, 5);
            map.SetFloor(water, FloorType.Water);
            var world = new WorldState(map);
            var house = new Household(0);
            var fw = new FarmWork(world) { House = house, Content = db, TemperatureAt = _ => 24f, Season = () => Time.Season.Summer };
            var c = Adult(at: new GridPos(1, 1));
            c.Needs.Set(NeedType.Hygiene, 10f);
            c.Needs.Set(NeedType.Fun, 20f);
            var r = new JobRunner(c, world) { TileWork = fw };

            r.Command(Job.OnTile(water, TileAction.Bathe));
            RunUntilIdle(c, r);
            Assert.Null(r.LastFailure);
            Assert.Equal(water, c.Position);                      // suya girdi
            Assert.True(c.Needs.Get(NeedType.Hygiene) > 60f);
            Assert.True(c.Wetness > 0.8f);
            Assert.True(c.Mood.Has(MoodCatalog.Swim));             // yaz serinliği

            double xp = c.Skills.GetXp(SkillType.Fishing);
            r.Command(Job.OnTile(water, TileAction.Fish));
            RunUntilIdle(c, r);
            Assert.Null(r.LastFailure);
            Assert.False(map.IsWater(c.Position));                 // kıyıdan
            Assert.True(c.Skills.GetXp(SkillType.Fishing) > xp);

            Assert.NotNull(fw.CanDo(c, water, TileAction.CutIce)); // donmamış
            map.WaterFrozen = true;
            Assert.NotNull(fw.CanDo(c, water, TileAction.Bathe));
            r.Command(Job.OnTile(water, TileAction.CutIce));
            RunUntilIdle(c, r);
            Assert.Equal(3, house.Count("item.ice"));
        }

        [Fact]
        public void SuKarosu_KisinYikanmakUsutur()
        {
            var db = Db();
            var map = new GridMap(6, 6);
            map.SetFloor(new GridPos(3, 3), FloorType.Water);
            var world = new WorldState(map);
            var fw = new FarmWork(world) { Content = db, TemperatureAt = _ => 3f };
            var c = Adult(at: new GridPos(2, 3));
            float warm = c.Needs.Get(NeedType.Warmth);
            var r = new JobRunner(c, world) { TileWork = fw };
            r.Command(Job.OnTile(new GridPos(3, 3), TileAction.Bathe));
            RunUntilIdle(c, r);
            Assert.True(c.Needs.Get(NeedType.Warmth) < warm - 10f);
            Assert.True(c.Mood.Has(MoodCatalog.ColdWater));
        }

        [Fact]
        public void SuKarosu_DegirmenVeSulamaSuyaBaglanir()
        {
            var db = Db();
            var map = new GridMap(10, 10);
            map.SetFloor(new GridPos(4, 4), FloorType.Water);
            var world = new WorldState(map);
            ItemDef node = null;
            foreach (var d in db.Items.All) if (d.NeedsWaterSource && !d.PowerSource && d.Width == 1 && d.Height == 1) { node = d; break; }
            Assert.NotNull(node);
            var it = world.PlaceItem(node, new GridPos(5, 4));
            Assert.True(WaterNetwork.TouchesWater(map, it));
            Assert.Contains(it.Id, WaterNetwork.Connected(world));
        }

        // ---------- otonomi ----------

        [Fact]
        public void Otonomi_SikisanUykudanKalkarTuvaleteGider()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var fx = new InteractionEffects(world, new Household(0), db);
            world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(2, 2));
            var pot = world.PlaceItem(db.Items.Get("item.chamber_pot"), new GridPos(6, 6));
            var c = Adult(at: new GridPos(1, 1));
            c.Needs.Set(NeedType.Energy, 5f);
            c.Needs.Set(NeedType.Bladder, 60f);
            var r = new JobRunner(c, world) { Effects = fx, Autonomous = true };
            bool slept = false, peed = false;
            for (int i = 0; i < 3000 && !peed; i++)
            {
                c.Tick(0.001); r.Tick(0.001);
                slept |= c.Asleep;
                peed = slept && pot.Fill > 0;
            }
            Assert.True(slept);
            Assert.True(peed, $"tuvalet {c.Needs.Get(NeedType.Bladder):0}");
            Assert.False(c.Mood.Has(MoodCatalog.Accident));
        }

        [Fact]
        public void Otonomi_YatmadanOnceTuvalet()
        {
            var db = Db();
            var world = new WorldState(new GridMap(12, 12));
            var fx = new InteractionEffects(world, new Household(0), db);
            world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(2, 2));
            var pot = world.PlaceItem(db.Items.Get("item.chamber_pot"), new GridPos(6, 6));
            var c = Adult(at: new GridPos(1, 1));
            c.Needs.Set(NeedType.Energy, 5f);
            c.Needs.Set(NeedType.Bladder, JobRunner.PreSleepBladder - 10f);
            var r = new JobRunner(c, world) { Effects = fx, Autonomous = true };
            r.Tick(0.0001);
            Assert.Equal(pot.Id, r.Queue.Current.ItemId);
        }

        [Fact]
        public void Otonomi_KritikSogukTuvaletiBekletir()
        {
            var c = Adult();
            c.Needs.Set(NeedType.Warmth, 10f);
            c.Needs.Set(NeedType.Bladder, 12f);
            var r = new JobRunner(c, new WorldState(new GridMap(6, 6)));
            var m = typeof(JobRunner).GetMethod("Rank", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int Rank(NeedType n) => (int)m.Invoke(r, new object[] { n });
            Assert.True(Rank(NeedType.Warmth) < Rank(NeedType.Bladder));
            Assert.True(Rank(NeedType.Bladder) < Rank(NeedType.Fun));
        }

        [Fact]
        public void Dogum_DisaridaDoganBebekEveKonur()
        {
            var db = Db();
            var map = new GridMap(14, 14);
            for (int x = 2; x < 7; x++)
            {
                map.SetEdge(GridMap.EdgeBetween(new GridPos(x, 1), new GridPos(x, 2)), EdgeKind.Wall);
                map.SetEdge(GridMap.EdgeBetween(new GridPos(x, 6), new GridPos(x, 7)), EdgeKind.Wall);
            }
            for (int y = 2; y < 7; y++)
            {
                map.SetEdge(GridMap.EdgeBetween(new GridPos(1, y), new GridPos(2, y)), EdgeKind.Wall);
                map.SetEdge(GridMap.EdgeBetween(new GridPos(6, y), new GridPos(7, y)), EdgeKind.Wall);
            }
            var world = new WorldState(map);
            world.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(3, 3));
            var room = map.Rooms.RoomAt(new GridPos(4, 4));
            Assert.True(room != null && room.IsIndoor);
            var spot = Population.ShelterNear(world, new GridPos(11, 11));
            Assert.Same(room, map.Rooms.RoomAt(spot));
            Assert.Equal(new GridPos(4, 4), Population.ShelterNear(world, new GridPos(4, 4)));
        }
    }
}
