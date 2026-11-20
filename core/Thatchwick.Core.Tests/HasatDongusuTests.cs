using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Tarım parti 2 (Project belgesi 59 §1-3): hasat döngüleri (devşirme, kes-yine-gelir, kalıcı yatak), sırık,
    // sararma ve yan ürün, görünür dev ürün.
    public class HasatDongusuTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db; public Household House; public WorldState World; public LifeSystems Life;
            public InteractionEffects Fx; public PlantSystem Plants; public GameClock Clock;
        }

        private static Kit Make(int plantSeed = 31)
        {
            var k = new Kit { Db = ContentFilesTests.LoadDb(out var errors), House = new Household(50), Clock = new GameClock() };
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            k.World = new WorldState(map);
            k.Life = new LifeSystems(k.Db, k.World, k.House, new Population(), k.Clock, 5);
            k.Plants = new PlantSystem(k.World, plantSeed);
            k.Plants.Config.CrowChance = 0;
            k.Plants.DefOf = id => k.Db.Items.TryGet(id, out var d) ? d : null;
            k.Fx = new InteractionEffects(k.World, k.House, k.Db, seed: 4) { Plants = k.Plants };
            k.Life.Attach(k.Fx);
            k.Life.Fields.Command = (c, job) =>
            {
                var item = k.World.GetItem(job.ItemId);
                k.Fx.Apply(c, item, item.Def.Interactions.First(i => i.Id == job.InteractionId));
                return true;
            };
            return k;
        }

        private static Character Farmer(int level = 1)
        {
            var c = new Character(1, "Çiftçi", new GridPos(1, 1), true) { CanStarve = false };
            if (level > 1) c.Skills.RestoreXp(SkillType.Farming, SkillRules.XpForLevel(level));
            return c;
        }

        private static ItemInstance Ripe(Kit k, string cropId, int x, int y)
        {
            var item = k.World.PlaceItem(k.Db.Items.Get(cropId), new GridPos(x, y));
            PlantSystem.SetGrown(item);
            return item;
        }

        private static InteractionDef Act(ItemInstance item, string id) => item.Def.Interactions.First(i => i.Id == id);

        [Fact]
        public void Icerik_DonguTipleriGercegeUygun()
        {
            var db = ContentFilesTests.LoadDb(out _);
            Assert.Equal(CropCycle.Repeat, db.Items.Get("item.crop_pea").Cycle);
            Assert.Equal(CropCycle.Repeat, db.Items.Get("item.crop_broad_bean").Cycle);
            Assert.Equal(CropCycle.Cut, db.Items.Get("item.crop_lettuce").Cycle);
            Assert.Equal(CropCycle.Cut, db.Items.Get("item.crop_spinach").Cycle);
            Assert.Equal(CropCycle.Perennial, db.Items.Get("item.crop_saffron").Cycle);
            Assert.Equal(CropCycle.Perennial, db.Items.Get("item.crop_madder").Cycle);
            Assert.Equal(CropCycle.Single, db.Items.Get("item.crop_turnip").Cycle);
            Assert.Equal(CropCycle.Single, db.Items.Get("item.crop_wheat").Cycle);
            Assert.True(db.Items.Get("item.crop_pea").NeedsStake);
            // Kalıcı yatak kışı toprakta geçirir; her tarla ekini sökülebilir.
            Assert.False(db.Items.Get("item.crop_saffron").DiesInWinter);
            foreach (var crop in db.Items.All.Where(i => i.IsCrop))
                Assert.True(crop.Interactions.Any(i => i.Id == HarvestOrders.UprootId && i.RequiresWithered && i.RemovesItem), crop.Id + " sökülemiyor");
        }

        [Fact]
        public void Devsirme_BitkiKalir_UcToplamadaSararir_SokunceTohumVerir()
        {
            var k = Make();
            var c = Farmer();
            var bean = Ripe(k, "item.crop_broad_bean", 4, 4);
            float before = k.Plants.FertilityAt(bean.Position);

            k.Fx.Apply(c, bean, Act(bean, "harvest"));
            Assert.NotNull(k.World.GetItem(bean.Id));                     // bitki yerinde
            Assert.False(bean.IsGrown);
            Assert.Equal(1, bean.Harvests);
            Assert.Equal(2, PlantSystem.HarvestsLeft(bean));
            Assert.True(k.House.Count("item.broad_bean") >= 2);
            Assert.Equal(0, k.House.Count("item.seed_broad_bean"));       // devşirme tohum vermez
            Assert.Equal(before, k.Plants.FertilityAt(bean.Position));    // toprak sökülünce yorulur, toplamada değil

            // Yeniden olgunlaşma: regrowDays kadar sulu gün.
            for (int i = 0; i < 3 && !bean.IsGrown; i++) { PlantSystem.Water(bean); k.Plants.Tick(1.0, Season.Spring); }
            Assert.True(bean.IsGrown);
            k.Fx.Apply(c, bean, Act(bean, "harvest"));
            PlantSystem.SetGrown(bean);
            k.Fx.Apply(c, bean, Act(bean, "harvest"));
            Assert.True(bean.Withered);                                   // toplama hakkı bitti
            Assert.False(bean.IsGrown);

            k.Fx.Apply(c, bean, Act(bean, HarvestOrders.UprootId));
            Assert.Null(k.World.GetItem(bean.Id));
            Assert.Equal(3, k.House.Count("item.seed_broad_bean"));       // kabukta kuruyan tane
            Assert.True(k.House.Count("item.feed") >= 1);                 // sap
            Assert.NotEqual(before, k.Plants.FertilityAt(new GridPos(4, 4)));   // baklagil sökülünce toprağı besler
        }

        [Fact]
        public void KesYineGelir_UcKesimdenSonraTohumaKacar()
        {
            var k = Make();
            var c = Farmer();
            var lettuce = Ripe(k, "item.crop_lettuce", 3, 3);
            for (int i = 0; i < 3; i++)
            {
                PlantSystem.SetGrown(lettuce);
                k.Fx.Apply(c, lettuce, Act(lettuce, "harvest"));
            }
            Assert.True(k.House.Count("item.lettuce") >= 6);
            Assert.True(lettuce.Withered && lettuce.WitheredMature);
            k.Fx.Apply(c, lettuce, Act(lettuce, HarvestOrders.UprootId));
            Assert.InRange(k.House.Count("item.seed_lettuce"), 2, 3);
            Assert.Equal(1, k.House.Count(PlantSystem.GreenItemId));
        }

        [Fact]
        public void KaliciYatak_SokulmezUyur_KisinOlmez()
        {
            var k = Make();
            var c = Farmer();
            var saffron = Ripe(k, "item.crop_saffron", 5, 5);
            k.Fx.Apply(c, saffron, Act(saffron, "harvest"));
            Assert.NotNull(k.World.GetItem(saffron.Id));
            Assert.False(saffron.IsGrown);
            Assert.Equal(-1, PlantSystem.HarvestsLeft(saffron));

            // Kış ve mevsim dışı: sararmaz, ölmez; kendi mevsiminde yeniden açar.
            k.Plants.Tick(3.0, Season.Winter);
            k.Plants.Tick(3.0, Season.Spring);
            Assert.NotNull(k.World.GetItem(saffron.Id));
            Assert.False(saffron.Withered);
            for (int i = 0; i < 8 && !saffron.IsGrown; i++) { PlantSystem.Water(saffron); k.Plants.Tick(1.0, Season.Autumn); }
            Assert.True(saffron.IsGrown);

            // Toplanmayan çiçek mevsimi geçince solar; yatak kalır.
            k.Plants.Tick(1.5, Season.Winter);
            Assert.False(saffron.IsGrown);
            Assert.False(saffron.Withered);
        }

        [Fact]
        public void Sirik_SiriksizUrunYarim_SirikSokunceGeriGelir()
        {
            var k = Make();
            var c = Farmer();
            var bare = Ripe(k, "item.crop_pea", 2, 2);
            Assert.True(PlantSystem.MissingStake(bare));
            k.Fx.Apply(c, bare, Act(bare, "harvest"));
            int unstaked = k.House.Count("item.pea");

            var staked = Ripe(k, "item.crop_pea", 6, 2);
            k.House.AddItem(PlantSystem.StakeItemId, 1);
            Assert.Null(k.Fx.CanStart(c, staked, Act(staked, "stake")));
            k.Fx.Apply(c, staked, Act(staked, "stake"));
            Assert.True(staked.Staked);
            Assert.Equal(0, k.House.Count(PlantSystem.StakeItemId));
            Assert.NotNull(k.Fx.CanStart(c, staked, Act(staked, "stake")));     // ikinci sırık dikilmez
            k.Fx.Apply(c, staked, Act(staked, "harvest"));
            int withStake = k.House.Count("item.pea") - unstaked;
            Assert.True(withStake >= 2 * unstaked, $"sırıklı {withStake}, sırıksız {unstaked}");

            k.Plants.Wither(staked, WitherCause.Season);
            k.Fx.Apply(c, staked, Act(staked, HarvestOrders.UprootId));
            Assert.Equal(1, k.House.Count(PlantSystem.StakeItemId));           // sırık depoya döner

            // Sırık istemeyen ekine dikilmez.
            var turnip = Ripe(k, "item.crop_turnip", 9, 9);
            Assert.DoesNotContain(turnip.Def.Interactions, i => i.Stakes);
        }

        [Fact]
        public void Sararma_MevsimiGecenEkinBirGundeSararir_YanUrunAileyeGore()
        {
            var k = Make();
            var c = Farmer();
            var withered = new List<WitherCause>();
            k.Plants.PlantWithered += (item, cause) => withered.Add(cause);

            var barley = Ripe(k, "item.crop_barley", 2, 2);            // ilkbahar ve yaz
            var sprout = k.World.PlaceItem(k.Db.Items.Get("item.crop_barley"), new GridPos(4, 2));   // filiz
            var wheat = Ripe(k, "item.crop_wheat", 6, 2);              // sonbahar ve ilkbahar: yazın sararır, kışın uyur
            k.Plants.Tick(0.5, Season.Autumn);
            Assert.False(barley.Withered);
            k.Plants.Tick(0.6, Season.Autumn);
            Assert.True(barley.Withered && barley.WitheredMature);
            Assert.True(sprout.Withered && !sprout.WitheredMature);
            Assert.False(wheat.Withered);
            Assert.Equal(2, withered.Count(w => w == WitherCause.Season));

            // Sararmış ekin hasat edilmez, sulanmaz; yalnız sökülür.
            Assert.False(barley.IsGrown);
            Assert.NotNull(k.Fx.CanStart(c, barley, Act(barley, "water")));
            Assert.Null(k.Fx.CanStart(c, barley, Act(barley, HarvestOrders.UprootId)));
            Assert.NotNull(k.Fx.CanStart(c, wheat, Act(wheat, HarvestOrders.UprootId)));   // canlı ekin sökülmez

            k.Fx.Apply(c, barley, Act(barley, HarvestOrders.UprootId));
            Assert.Equal(2, k.House.Count("item.straw"));              // tahıl: saman + demetin üçte biri
            Assert.True(k.House.Count("item.sheaf_barley") >= 1);
            Assert.Equal(0, k.House.Count("item.seed_barley"));

            k.Fx.Apply(c, sprout, Act(sprout, HarvestOrders.UprootId));
            Assert.Equal(1, k.House.Count(PlantSystem.GreenItemId));   // filiz: yalnız kompostluk yeşil

            // Kış: buğday uyur (donmaz, sararmaz).
            k.Plants.Tick(2.0, Season.Winter);
            Assert.NotNull(k.World.GetItem(wheat.Id));
            Assert.False(wheat.Withered);
        }

        [Fact]
        public void Kis_GelismisEkinSararir_FilizDonar_SokulmeyenCurur()
        {
            var k = Make();
            var lost = new List<PlantLoss>();
            k.Plants.PlantLost += (def, pos, why) => lost.Add(why);
            var ripe = Ripe(k, "item.crop_turnip", 2, 2);
            var sprout = k.World.PlaceItem(k.Db.Items.Get("item.crop_turnip"), new GridPos(4, 2));
            k.Plants.Tick(0.1, Season.Winter);
            Assert.True(ripe.Withered);
            Assert.Null(k.World.GetItem(sprout.Id));
            Assert.Contains(PlantLoss.Frost, lost);

            float before = k.Plants.FertilityAt(ripe.Position);
            k.Plants.Tick(k.Plants.Config.WitheredRotDays + 0.1, Season.Winter);
            Assert.Null(k.World.GetItem(ripe.Id));                      // çürüyüp toprağa karıştı
            Assert.Contains(PlantLoss.Rotted, lost);
            Assert.True(k.Plants.FertilityAt(new GridPos(2, 2)) > before);
        }

        [Fact]
        public void MevsimSonuUyarisi_RiskliEkinSayar()
        {
            var k = Make();
            Ripe(k, "item.crop_pea", 2, 2);        // yalnız ilkbahar
            Ripe(k, "item.crop_leek", 4, 2);       // yaz ve sonbahar
            Ripe(k, "item.crop_saffron", 6, 2);    // kalıcı yatak
            Assert.Equal(1, k.Plants.AtRiskNextSeason(Season.Summer));
            Assert.Equal(2, k.Plants.AtRiskNextSeason(Season.Winter));   // bezelye ve pırasa donar; safran kalır
        }

        [Fact]
        public void DevUrun_IkiyeIkiOlgunVeSuluKaro_TekEsyayaDonusur()
        {
            var k = Make();
            k.Plants.Config.GiantChance = 1.0;
            var block = new List<ItemInstance>();
            for (int y = 3; y <= 4; y++) for (int x = 3; x <= 4; x++) block.Add(Ripe(k, "item.crop_turnip", x, y));
            var lone = Ripe(k, "item.crop_turnip", 8, 8);
            var dry = new List<ItemInstance>();
            for (int y = 10; y <= 11; y++) for (int x = 10; x <= 11; x++) dry.Add(Ripe(k, "item.crop_cabbage", x, y));
            foreach (var b in block) PlantSystemTestAccess.ForceWater(b);

            ItemInstance giant = null;
            k.Plants.GiantGrown += (crop, g) => giant = g;
            k.Plants.OnNewDay(rained: false);
            Assert.NotNull(giant);
            Assert.Equal("item.giant_crop_turnip", giant.Def.Id);
            Assert.Equal(new GridPos(3, 3), giant.Position);
            Assert.All(block, b => Assert.Null(k.World.GetItem(b.Id)));
            Assert.NotNull(k.World.GetItem(lone.Id));                    // tek karo dev olmaz
            Assert.All(dry, d => Assert.NotNull(k.World.GetItem(d.Id)));   // susuz 2×2 dev olmaz

            var c = Farmer();
            k.Fx.Apply(c, giant, giant.Def.Interactions.First(i => i.Id == "harvest_giant"));
            Assert.Null(k.World.GetItem(giant.Id));
            Assert.Equal(1, k.House.Count("item.giant_turnip"));
            Assert.InRange(k.House.Count("item.turnip"), 10, 16);
        }

        [Fact]
        public void DevUrun_SansKucuk_UstaCiftcideIkiKat()
        {
            int Giants(int skill)
            {
                int n = 0;
                for (int seed = 1; seed <= 400; seed++)
                {
                    var k = Make(seed);
                    k.Plants.FarmingLevel = () => skill;
                    for (int y = 3; y <= 4; y++) for (int x = 3; x <= 4; x++) Ripe(k, "item.crop_beet", x, y);
                    k.Plants.GiantGrown += (crop, g) => n++;
                    k.Plants.OnNewDay(rained: true);
                }
                return n;
            }
            int novice = Giants(1), master = Giants(8);
            Assert.InRange(novice, 1, 25);            // ~%2
            Assert.True(master > novice, $"usta {master}, acemi {novice}");
        }

        [Fact]
        public void HasatEmri_SararmisTarlaTekDokunuslaSokulur()
        {
            var k = Make();
            var c = Farmer(HarvestOrders.FieldLevel);
            var a = Ripe(k, "item.crop_carrot", 3, 3);
            var b = Ripe(k, "item.crop_carrot", 4, 3);
            var ripe = Ripe(k, "item.crop_carrot", 5, 3);
            k.Plants.Wither(a, WitherCause.Season);
            k.Plants.Wither(b, WitherCause.Season);
            Assert.Equal(HarvestOrders.UprootId, HarvestOrders.WorkOf(a));
            Assert.Equal(HarvestOrders.HarvestId, HarvestOrders.WorkOf(ripe));

            Assert.Equal(2, k.Life.Fields.OrderFrom(c, a));              // bitişik sararmışlar; olgun olan karışmaz
            Assert.Null(k.World.GetItem(a.Id));
            Assert.Null(k.World.GetItem(b.Id));
            Assert.NotNull(k.World.GetItem(ripe.Id));
            Assert.True(k.House.Count("item.feed") >= 2);                // kök: odunsulaşır, yem olur
        }

        [Fact]
        public void Kayit_DonguDurumuKorunur()
        {
            var k = Make();
            var c = Farmer();
            var pea = Ripe(k, "item.crop_pea", 2, 2);
            k.House.AddItem(PlantSystem.StakeItemId, 1);
            k.Fx.Apply(c, pea, Act(pea, "stake"));
            k.Fx.Apply(c, pea, Act(pea, "harvest"));
            var old = Ripe(k, "item.crop_turnip", 5, 5);
            k.Plants.Wither(old, WitherCause.Season);

            var chars = new List<Character> { c };
            var data = SaveGame.Capture(k.Clock, k.World, k.House, chars, new Thatchwick.Core.Progress.AccountProgress(), null, null, plants: k.Plants);
            var json = new JsonSerializerOptions { IncludeFields = true };
            var round = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, json), json);
            var world2 = SaveGame.Load(round, k.Db).World;
            var pea2 = world2.GetItemAt(new GridPos(2, 2));
            var old2 = world2.GetItemAt(new GridPos(5, 5));
            Assert.True(pea2.Staked);
            Assert.Equal(1, pea2.Harvests);
            Assert.True(old2.Withered && old2.WitheredMature);
            Assert.False(old2.IsGrown);
        }
    }

    internal static class PlantSystemTestAccess
    {
        /// <summary>Ekini bugün sulanmış sayar (dev ürün koşulu "dün ya da bu sabah sulu").</summary>
        public static void ForceWater(ItemInstance item)
        {
            // Olgun ekin sulanmaz (etkileşim engeller); PlantSystem.Water yalnız sararmışı reddeder.
            PlantSystem.Water(item);
        }
    }
}
