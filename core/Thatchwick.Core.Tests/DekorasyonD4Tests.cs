using System.Linq;
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
    // Ev dekorasyonu D3 tamamlama ve D4 (Project belgesi 27e): kavanoz, şişeleme, cam ocağı yakınlığı, köye camcılık,
    // kalay, şenlik hazırlığı, Kıyı Evi / Han Salonu / Kervan Odası tarzları ve gezgin tüccar.
    public class DekorasyonD4Tests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static InteractionDef Craft(ContentDatabase db, string station, string recipe) =>
            db.Items.Get(station).Interactions.First(i => i.Id == "craft:" + recipe);

        [Fact]
        public void Kavanoz_AgziBagli_IcindekiDortKatYavasBozulur()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household();
            var table = world.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(2, 2));
            var jar = world.PlaceOnSurface(db.Items.Get("item.glass_jar"), table.Id, "a");
            var basket = world.PlaceOnSurface(db.Items.Get("item.fruit_basket"), table.Id, "b");
            house.AddItem("item.plum", 7);
            Assert.Equal(1, DecorSystem.Fill(jar, house, db));
            Assert.True(DecorSystem.Fill(basket, house, db) > 0);
            var decor = new DecorSystem(world, db);
            decor.Tick(2.0);
            Assert.Equal(0.5, jar.Contents[0].Age, 6);
            Assert.Equal(2.0, basket.Contents[0].Age, 6);
            Assert.Contains("4 kat yavaş", ItemCatalog.Describe(db.Items.Get("item.glass_jar")));
        }

        [Fact]
        public void Sisele_IksirYapilirkenCamSiseHarcanir_BirFazlaCikar()
        {
            var db = Db();
            var world = new WorldState(new GridMap(10, 10));
            var house = new Household();
            var fx = new InteractionEffects(world, house, db);
            var alembic = world.PlaceItem(db.Items.Get("item.alembic"), new GridPos(2, 2));
            var c = new Character(1, "Ayşe", new GridPos(1, 1));
            c.SetAge(30);
            c.GainSkillXp(SkillType.Herbalism, SkillRules.XpForLevel(3));
            var calm = Craft(db, "item.alembic", "potion_calm");

            house.AddItem("item.elderflower", 4); house.AddItem("item.honey", 2);
            fx.Apply(c, alembic, calm);
            int plain = house.Count("item.potion_calm");
            Assert.True(plain >= 1);

            house.AddItem("item.glass_bottle", 1);
            fx.Apply(c, alembic, calm);
            Assert.Equal(0, house.Count("item.glass_bottle"));
            Assert.True(house.Count("item.potion_calm") - plain >= 2);
        }

        [Fact]
        public void CamciTezgahi_CamOcagiYakindaOlmali()
        {
            var db = Db();
            var world = new WorldState(new GridMap(20, 20));
            var bench = world.PlaceItem(db.Items.Get("item.glass_bench"), new GridPos(2, 2));
            Assert.Equal("Cam ocağı yakında olmalı (4 karo).", JobRunner.NearbyBlocker(world, bench));
            var far = world.PlaceItem(db.Items.Get("item.glass_furnace"), new GridPos(12, 12));
            Assert.NotNull(JobRunner.NearbyBlocker(world, bench));
            world.RemoveItem(far.Id);
            world.PlaceItem(db.Items.Get("item.glass_furnace"), new GridPos(7, 2));      // tezgâh 2-3, ocak 7-8: 4 karo
            Assert.Null(JobRunner.NearbyBlocker(world, bench));

            var c = new Character(1, "Ayşe", new GridPos(1, 1));
            c.SetAge(30);
            c.GainSkillXp(SkillType.Pottery, SkillRules.XpForLevel(9));
            var bottle = Craft(db, "item.glass_bench", "glass_bottle");
            Assert.NotEqual("Cam ocağı yakında olmalı (4 karo).", new JobRunner(c, world).WhyCannotStart(bench, bottle));
            Assert.Contains("Cam ocağı 4 karo yakınında", ItemCatalog.Describe(db.Items.Get("item.glass_bench")));
        }

        [Fact]
        public void KoyeCamcilikGeldi_CamOcagiKurulunca_KatalogdaCamSise()
        {
            var db = Db();
            var world = new WorldState(new GridMap(20, 20));
            var house = new Household();
            var life = new LifeSystems(db, world, house, new Population(), new GameClock());
            var bottle = db.Items.Get("item.glass_bottle");
            Assert.DoesNotContain(bottle, ItemCatalog.InCategory(db, bottle.Category, null, null, null, house));
            Assert.False(life.CheckGlassworks());
            world.PlaceItem(db.Items.Get(LifeSystems.GlassFurnaceId), new GridPos(5, 5));
            Assert.True(life.CheckGlassworks());
            Assert.False(life.CheckGlassworks());
            Assert.True(house.HasShopUnlock(LifeSystems.GlassworksUnlock));
            Assert.Contains(bottle, ItemCatalog.InCategory(db, bottle.Category, null, null, null, house));
            Assert.DoesNotContain(db.Items.Get("item.hourglass"), ItemCatalog.InCategory(db, "decor", null, null, null, house));   // kum saati yine yalnızca yapılır
            Assert.Contains(life.Log.Items, e => e.Text.Contains("Köye camcılık geldi"));
        }

        [Fact]
        public void Kalay_PazardanAlinir_AynaKuresiIkiTaneVerir()
        {
            var db = Db();
            var tin = db.Items.Get("item.tin");
            Assert.Equal(ItemPlacement.Resource, tin.Placement);
            Assert.False(tin.HasTag("craftonly"));
            var sphere = Craft(db, "item.glass_bench", "mirror_sphere_tin");
            Assert.Contains(sphere.Consume, c => c.ItemId == "item.tin");
            Assert.Equal(2, sphere.Produce[0].Count);
            Assert.Equal(LifeSystems.GlassBranchId, sphere.RequiresBranch);
        }

        [Fact]
        public void SenlikHazirligi_YaklasincaDuyurulur_SusluEveSevinc()
        {
            var db = Db();
            var world = new WorldState(new GridMap(20, 20));
            var house = new Household();
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var wat = new Character(pop.NextId(), "Wat", new GridPos(3, 3)) { MoodEnabled = true };
            wat.SetAge(30);
            pop.Add(wat);
            // İç oda
            for (int x = 2; x <= 5; x++) { world.Map.SetEdge(new Edge(x, 2, EdgeAxis.Horizontal), EdgeKind.Wall); world.Map.SetEdge(new Edge(x, 6, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = 2; y <= 5; y++) { world.Map.SetEdge(new Edge(2, y, EdgeAxis.Vertical), EdgeKind.Wall); world.Map.SetEdge(new Edge(6, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            for (int x = 2; x <= 5; x++) for (int y = 2; y <= 5; y++) world.Map.SetFloor(new GridPos(x, y), FloorType.Wood);

            var next = life.Festivals.NextFestival(out int days);
            Assert.NotNull(next);
            // Şenlikten 2 gün öncesine git.
            clock.GetType().GetProperty("TotalDays").SetValue(clock, (double)(clock.DayIndex + days - 2) + 0.3);
            life.UpdateFestivePrep();
            Assert.Equal(next.Id, life.FestivePrep.Id);
            Assert.Contains(life.Log.Items, e => e.Text.Contains("2 gün sonra"));
            Assert.False(wat.Mood.Has(LifeSystems.FestiveHomeMoodId));

            var season = clock.Season;
            var deco = db.Items.All.First(d => d.IsSeasonal && d.InSeason(season) && d.Placement == ItemPlacement.Wall);
            ItemInstance hung = null;
            for (int side = 0; side < 4 && hung == null; side++)
                if (world.CanPlaceOnWall(deco, new GridPos(3, 2), side).Ok) hung = world.PlaceOnWall(deco, new GridPos(3, 2), side);
            Assert.NotNull(hung);
            Assert.True(life.HomeIsFestive());
            life.UpdateFestivePrep();
            Assert.True(wat.Mood.Has(LifeSystems.FestiveHomeMoodId));

            // Şenlikten uzaktayken hazırlık yok.
            clock.GetType().GetProperty("TotalDays").SetValue(clock, (double)(clock.DayIndex + 3 + LifeSystems.FestivePrepDays));
            life.UpdateFestivePrep();
            if (life.Festivals.NextFestival(out int d2) != null && d2 > LifeSystems.FestivePrepDays) Assert.Null(life.FestivePrep);
        }

        [Fact]
        public void D4Tarzlar_UcYeniTarz_HerbirindeSekizEsya()
        {
            var db = Db();
            foreach (var theme in new[] { "Kıyı", "Han", "Kervan" })
            {
                Assert.NotNull(Decor.ThemeName(theme));
                int n = db.Items.All.Count(d => d.StyleTheme == theme);
                Assert.True(n >= 8, $"{theme}: {n}");
            }
            Assert.Equal("Kıyı Evi", Decor.ThemeName("Kıyı"));
            var fishString = db.Items.Get("item.dried_fish_string");
            Assert.True(fishString.IsFillDecor);
            Assert.True(fishString.AcceptsFill("item.fish_trout"));
            Assert.Contains(db.Items.Get("item.forage_sand").Interactions[0].Produce, p => p.ItemId == "item.mussel_shell");
            Assert.Equal("item.shell_bowl", Craft(db, "item.pottery_wheel", "shell_bowl").Produce[0].ItemId);
            Assert.Contains(db.Items.Get("item.singer_stand").Interactions, i => i.Id == "sing" && i.Need == NeedType.Fun);
            // Han Salonu alkol göstermez: maşrapa boş bir süs.
            Assert.Empty(db.Items.Get("item.tankard").Interactions);
        }

        [Fact]
        public void Kervan_YalnizcaPazarGunuSatilir()
        {
            var db = Db();
            var house = new Household();
            var lamp = db.Items.Get("item.brass_lantern");
            Assert.True(lamp.HasTag(Decor.MerchantTag));
            Assert.Contains("pazar günü", ItemCatalog.Describe(lamp));
            house.MerchantInVillage = false;
            Assert.DoesNotContain(lamp, ItemCatalog.InCategory(db, lamp.Category, null, null, null, house));
            house.MerchantInVillage = true;
            Assert.Contains(lamp, ItemCatalog.InCategory(db, lamp.Category, null, null, null, house));

            var world = new WorldState(new GridMap(10, 10));
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, new Population(), clock);
            // Pazar gününe git.
            long day = clock.DayIndex;
            while (!life.Festivals.IsMarketDay(day)) day++;
            clock.GetType().GetProperty("TotalDays").SetValue(clock, day + 0.3);
            house.MerchantInVillage = false;
            life.UpdateMerchant(true);
            Assert.True(house.MerchantInVillage);
            Assert.Contains(life.Log.Items, e => e.Text.Contains("Gezgin tüccar köyde"));
            clock.GetType().GetProperty("TotalDays").SetValue(clock, day + 1.3);
            life.UpdateMerchant(true);
            Assert.False(house.MerchantInVillage);
        }

        [Fact]
        public void Kayit_KoyKilitleriGeriGelir()
        {
            var db = Db();
            var house = new Household();
            house.UnlockShop(LifeSystems.GlassworksUnlock);
            var world = new WorldState(new GridMap(10, 10));
            var data = Thatchwick.Core.Save.SaveGame.Capture(new GameClock(), world, house, new Character[0], null, null, null);
            Assert.Contains(LifeSystems.GlassworksUnlock, data.household.shopUnlocks);
        }
    }
}
