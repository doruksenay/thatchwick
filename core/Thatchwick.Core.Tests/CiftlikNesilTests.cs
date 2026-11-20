using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // 28. belge (18.11.2026): hayvan üremesi (cinsiyet, gebelik, yavru, soy), ürün kalitesi, domuz,
    // harman ve saman, düven, aile bazlı ekim nöbeti ve tohum serpen tarlacı.
    public class CiftlikNesilTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public AnimalSystem Animals;
            public InteractionEffects Effects;
        }

        private static Ctx Make(int size = 24)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            var c = new Ctx { Db = db, World = new WorldState(map), House = new Household(20000) };
            c.Build = new BuildService(c.World, c.House, db);
            c.Animals = new AnimalSystem(c.World, db) { DiseaseEnabled = false };
            c.Build.HousingResidents = c.Animals.Residents;
            c.Effects = new InteractionEffects(c.World, c.House, db) { Animals = c.Animals };
            return c;
        }

        private static Character Adult(int id = 1, int x = 1, int y = 1)
        {
            var c = new Character(id, "Agnes", new GridPos(x, y));
            c.SetAge(30);
            return c;
        }

        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);

        // ---------- üreme ----------

        [Fact]
        public void Satin_IlkIkiHayvanCiftOlur_GerisiDisi_EskiKayitTekKimlikDisi()
        {
            var x = Make();
            x.World.PlaceItem(x.Db.Items.Get("item.barn"), new GridPos(5, 5));
            Assert.True(x.Animals.Buy("animal.cow", x.House, out var a1).Ok);
            Assert.True(x.Animals.Buy("animal.cow", x.House, out var a2).Ok);
            Assert.True(x.Animals.Buy("animal.cow", x.House, out var a3).Ok);
            Assert.True(a1.IsFemale);
            Assert.False(a2.IsFemale);                       // sürüde dişi var, erkek yok: öküz
            Assert.True(a3.IsFemale);
            Assert.Equal("boğa", a2.SexText);
            Assert.True(x.Animals.Buy("animal.cow", x.House, out var a4, female: false).Ok);   // oyuncu seçebilir
            Assert.False(a4.IsFemale);

            var hen = x.Db.Animals.Get("animal.chicken");
            var coop = x.World.PlaceItem(x.Db.Items.Get("item.coop"), new GridPos(12, 12));
            Assert.True(x.Animals.Restore(11, hen, new GridPos(11, 11), coop.Id, 100f, 10, 0, 0).IsFemale);
            Assert.False(x.Animals.Restore(12, hen, new GridPos(11, 12), coop.Id, 100f, 10, 0, 0).IsFemale);
        }

        [Fact]
        public void Horoz_YumurtlamazKoc_YunVerir()
        {
            var x = Make();
            var coop = x.World.PlaceItem(x.Db.Items.Get("item.coop"), new GridPos(5, 5));
            AnimalSystem.AddFeed(coop, 100);
            var rooster = x.Animals.Restore(2, x.Db.Animals.Get("animal.chicken"), new GridPos(4, 4), coop.Id, 100f, 10, 0, 0, female: false);
            x.Animals.Tick(0.5);
            Assert.Equal(0.0, rooster.ProduceProgress, 6);
            Assert.False(x.Db.Animals.Get("animal.sheep").FemaleProduces);
            Assert.True(x.Db.Animals.Get("animal.cow").FemaleProduces);
        }

        [Fact]
        public void Ureme_ErkekGerekir_GebelikSurer_SonraDogar_KusakArtar()
        {
            var x = Make();
            var barn = x.World.PlaceItem(x.Db.Items.Get("item.barn"), new GridPos(5, 5));
            var cow = x.Db.Animals.Get("animal.cow");
            x.Animals.HusbandryLevel = () => 10;
            var mom = x.Animals.Restore(1, cow, new GridPos(4, 4), barn.Id, 100f, 20, 0, 0, friendship: 80f, comfort: 60f, female: true, generation: 2);
            var other = x.Animals.Restore(3, cow, new GridPos(4, 5), barn.Id, 100f, 20, 0, 0, friendship: 80f, comfort: 60f, female: true);
            Assert.Contains("boğa", x.Animals.BreedBlocker(mom));
            for (int d = 0; d < 60; d++) x.Animals.OnNewDay(Season.Spring);
            Assert.False(mom.IsPregnant || other.IsPregnant);        // erkeksiz gebelik yok
            Assert.Equal(2, x.Animals.Animals.Count);

            var bull = x.Animals.Restore(2, cow, new GridPos(5, 4), barn.Id, 100f, 20, 0, 0, friendship: 80f, comfort: 60f, female: false, generation: 0);
            Assert.Null(x.Animals.BreedBlocker(mom));
            Animal conceived = null;
            x.Animals.Conceived += (f, father) => { conceived = f; Assert.Equal(bull.Id, father); };
            var born = new List<Animal>();
            x.Animals.Born += (m, b) => born.Add(b);
            for (int d = 0; d < 200 && conceived == null; d++) x.Animals.OnNewDay(Season.Spring);
            Assert.NotNull(conceived);
            Assert.True(conceived.IsPregnant);
            Assert.Equal(2.0, conceived.PregnantDays, 6);            // inek: 2 gün (yaklaşık bir yıl)
            x.Animals.OnNewDay(Season.Spring);
            Assert.Empty(born);
            x.Animals.OnNewDay(Season.Spring);
            var calf = Assert.Single(born);
            Assert.False(conceived.IsPregnant);
            Assert.Equal(conceived.Id, calf.MotherId);
            Assert.Equal(conceived.Generation + 1, calf.Generation);
            Assert.False(calf.IsMature);
            Assert.Equal((double)(conceived.Friendship * AnimalSystem.InheritFriendship), (double)calf.Friendship, 3);
        }

        [Fact]
        public void Dogum_YerYoksaBekler_YerAcilincaDogar()
        {
            var x = Make();
            var sty = x.World.PlaceItem(x.Db.Items.Get("item.pigsty"), new GridPos(5, 5));   // 3 yer
            var pig = x.Db.Animals.Get("animal.pig");
            var sow = x.Animals.Restore(1, pig, new GridPos(4, 4), sty.Id, 100f, 10, 0, 0, friendship: 80f, comfort: 60f, female: true, pregnantDays: 1);
            x.Animals.Restore(2, pig, new GridPos(4, 5), sty.Id, 100f, 10, 0, 0, female: false);
            x.Animals.Restore(4, pig, new GridPos(5, 4), sty.Id, 100f, 10, 0, 0, female: true);
            int delayed = 0;
            x.Animals.BirthDelayed += _ => delayed++;
            x.Animals.OnNewDay(Season.Spring);
            Assert.Equal(1, delayed);
            Assert.True(sow.IsPregnant);
            Assert.Equal(3, x.Animals.Animals.Count);
            x.World.PlaceItem(x.Db.Items.Get("item.pigsty"), new GridPos(12, 12));
            // Yeni ağıl başka yerde: anneyle aynı barınak değil, doğum yine bekler; aynı ağıldan biri satılınca doğar.
            x.Animals.Sell(4, x.House);
            x.Animals.OnNewDay(Season.Spring);
            Assert.False(sow.IsPregnant);
            Assert.Equal(3, x.Animals.Animals.Count);                // yer kadar: 1 yavru
        }

        [Fact]
        public void Domuz_Icerik_AgilVeBolme_GubreVerir()
        {
            var x = Make();
            var pig = x.Db.Animals.Get("animal.pig");
            Assert.Equal("sty", pig.Housing);
            Assert.Equal("item.manure", pig.Produces);
            Assert.True(pig.LitterMax >= 3);
            Assert.Equal("sty", x.Db.Items.Get("item.pigsty").HousingKind);
            Assert.True(x.Db.Items.Get("item.pig_pen").RequiresIndoor);
            x.World.PlaceItem(x.Db.Items.Get("item.pigsty"), new GridPos(5, 5));
            Assert.True(x.Animals.Buy("animal.pig", x.House, out var p).Ok);
            Assert.NotNull(x.Db.Animals.Get("animal.pig").Meat);
        }

        [Fact]
        public void Kalite_DostlukVeKonfordan_SoyHayvaniKolay_UrunuArtirir()
        {
            Assert.Equal(0, AnimalSystem.QualityTierOf(20f, 40f, false));
            Assert.Equal(1, AnimalSystem.QualityTierOf(60f, 50f, false));
            Assert.Equal(2, AnimalSystem.QualityTierOf(90f, 80f, false));
            Assert.Equal(2, AnimalSystem.QualityTierOf(75f, 70f, true));     // soy hayvanı +10
            Assert.Equal("Has", AnimalSystem.QualityName(2));   // belge 53: beş renkli düzen

            int best = 0;
            int Eggs(float friendship, float comfort)
            {
                var x = Make();
                var coop = x.World.PlaceItem(x.Db.Items.Get("item.coop"), new GridPos(5, 5));
                AnimalSystem.AddFeed(coop, 1000);
                x.Animals.Restore(1, x.Db.Animals.Get("animal.chicken"), new GridPos(4, 4), coop.Id, 100f, 10, 0, 0, friendship: friendship, comfort: comfort, female: true);
                int n = 0;
                for (int d = 0; d < 40; d++)
                {
                    x.Animals.Tick(1.0);
                    foreach (var kv in coop.Stock)   // barınak yığını kaliteyi "kimlik#kalite" anahtarıyla tutar (belge 53)
                    {
                        var key = ItemKey.FromStockKey(kv.Key);
                        if (key.Id != "item.egg") continue;
                        n += kv.Value;
                        if (friendship >= 100f && kv.Value > 0) best = System.Math.Max(best, key.Quality);
                    }
                    AnimalSystem.TakeStock(coop);
                }
                return n;
            }
            Assert.True(Eggs(100f, 100f) > Eggs(60f, 50f));
            Assert.True(best >= ItemQuality.Fine);   // bakımlı tavuğun yumurtası kaliteli
        }

        [Fact]
        public void Kayit_CinsiyetGebelikAnneKusak_GeriGelir()
        {
            var x = Make();
            var barn = x.World.PlaceItem(x.Db.Items.Get("item.barn"), new GridPos(5, 5));
            var cow = x.Db.Animals.Get("animal.cow");
            x.Animals.Restore(2, cow, new GridPos(4, 4), barn.Id, 100f, 20, 0, 0, female: true, pregnantDays: 1, motherId: 9, generation: 4);
            x.Animals.Restore(3, cow, new GridPos(4, 5), barn.Id, 100f, 20, 0, 0, female: false);
            var data = SaveGame.Capture(new GameClock(), x.World, x.House, new Character[0], null, null, null, animals: x.Animals);
            data = JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json);
            var loaded = SaveGame.Load(data, x.Db);
            var sys = new AnimalSystem(loaded.World, x.Db);
            SaveGame.RestoreAnimals(loaded, sys, x.Db);
            var a = sys.Get(2);
            Assert.True(a.IsFemale);
            Assert.True(a.IsPregnant);
            Assert.Equal(9, a.MotherId);
            Assert.Equal(4, a.Generation);
            Assert.True(a.IsHeritage);
            Assert.False(sys.Get(3).IsFemale);
        }

        // ---------- harman, düven, nöbet, ekim ----------

        [Fact]
        public void Harman_ElleIkiDemetIkiTaneBirSaman_DuvenCekiHayvaniylaSabahDover()
        {
            var x = Make();
            var floor = x.World.PlaceItem(x.Db.Items.Get("item.threshing_floor"), new GridPos(3, 3));
            var c = Adult();
            x.House.AddItem("item.sheaf_wheat", 4);
            var thresh = Inter(floor.Def, "thresh_wheat");
            Assert.Null(x.Effects.CanStart(c, floor, thresh));
            x.Effects.Apply(c, floor, thresh);
            Assert.Equal(2, x.House.Count("item.wheat"));
            Assert.Equal(1, x.House.Count("item.straw"));
            Assert.Equal(2, x.House.Count("item.sheaf_wheat"));

            x.World.PlaceItem(x.Db.Items.Get("item.threshing_sledge"), new GridPos(10, 10));
            var auto = new FarmAutomation(x.World, x.House) { Content = x.Db, DraftAnimals = () => 0 };
            x.House.AddItem("item.sheaf_barley", 10);
            auto.OnNewDay(Season.Summer);
            Assert.Equal(10, x.House.Count("item.sheaf_barley"));       // çeki hayvanı yok: düven durur

            var barn = x.World.PlaceItem(x.Db.Items.Get("item.barn"), new GridPos(14, 3));
            x.Animals.Restore(1, x.Db.Animals.Get("animal.cow"), new GridPos(13, 3), barn.Id, 100f, 20, 0, 0, female: false);
            Assert.Equal(1, x.Animals.DraftAnimals());
            auto.DraftAnimals = x.Animals.DraftAnimals;
            var made = auto.OnNewDay(Season.Summer);
            int sledge = x.Db.Items.Get("item.threshing_sledge").ThreshPerDay;
            Assert.Equal(12 - sledge, x.House.Count("item.sheaf_barley") + x.House.Count("item.sheaf_wheat"));
            Assert.Contains(made, kv => kv.Key == "item.straw" && kv.Value == sledge);     // düven her demetten bir saman
        }

        [Fact]
        public void Nobet_AileyeBakar_UcTarlaEkVerir()
        {
            var x = Make();
            var plants = new PlantSystem(x.World) { FamilyOf = id => x.Db.Items.TryGet(id, out var d) ? d.CropFamily : null };
            var cfg = plants.Config;
            var pos = new GridPos(5, 5);
            x.World.Map.SetFloor(pos, FloorType.Dirt);
            void Harvest(string crop)
            {
                var it = x.World.PlaceItem(x.Db.Items.Get(crop), pos);
                plants.OnHarvest(it);
                x.World.RemoveItem(it.Id);
            }
            Harvest("item.crop_wheat");
            float f0 = plants.FertilityAt(pos);
            Harvest("item.crop_barley");                            // ikisi de tahıl: yorar
            Assert.Equal((double)(f0 - cfg.HarvestDraw - cfg.MonocropPenalty), (double)plants.FertilityAt(pos), 4);
            Assert.Contains("Aynı aile", plants.RotationHint(pos, x.Db.Items.Get("item.crop_oats")));
            Assert.Contains("Nöbetleşe", plants.RotationHint(pos, x.Db.Items.Get("item.crop_turnip")));
            float f1 = plants.FertilityAt(pos);
            Harvest("item.crop_pea");                               // baklagil: azot + nöbet (önceki iki ekin aynı aile: üç tarla yok)
            Assert.Equal((double)(f1 - cfg.HarvestDraw + cfg.NitrogenBonus + cfg.RotationBonus), (double)plants.FertilityAt(pos), 4);
            float f2 = plants.FertilityAt(pos);
            Harvest("item.crop_turnip");                            // tahıl → baklagil → kök: üç tarla
            Assert.Equal((double)(f2 - cfg.DrawOf(FeedType.Light) + cfg.RotationBonus + cfg.ThreeFieldBonus), (double)plants.FertilityAt(pos), 4);   // şalgam az yiyen (57c)
            Assert.Equal("item.crop_pea", plants.SoilPlots.First().Value.PrevCrop);
        }

        [Fact]
        public void TohumSerpen_GorevliBosTopragaNobeteUyanTohumuEker()
        {
            var x = Make();
            var plants = new PlantSystem(x.World);
            x.Build.Plants = plants;
            var work = new FarmWork(x.World) { House = x.House, Content = x.Db, Build = x.Build, Plants = plants, Season = () => Season.Spring };
            var tile = new GridPos(6, 6);
            x.World.Map.SetFloor(tile, FloorType.Dirt);
            plants.SetSoil(tile, 0.5f, "item.crop_wheat");
            x.House.AddItem("item.seed_barley", 3);                 // tahıl: nöbete uymaz
            x.House.AddItem("item.seed_pea", 3);
            Assert.Equal("item.seed_pea", work.PickSeed(tile));

            var planner = new DutyPlanner(x.World, x.Animals, x.Effects, null) { TileWork = work };
            var c = Adult(1, 6, 7);
            c.Duty = DutyKind.Farm;
            var job = planner.Next(c);
            Assert.NotNull(job);
            Assert.Equal(TileAction.Sow, job.TileAction);
            work.Apply(c, tile, TileAction.Sow);
            Assert.Equal("item.crop_pea", x.World.GetItemAt(tile).Def.Id);
            Assert.Equal(2, x.House.Count("item.seed_pea"));

            // Kışın tohum serpilmez.
            var winter = new FarmWork(x.World) { House = x.House, Content = x.Db, Build = x.Build, Season = () => Season.Winter };
            var t2 = new GridPos(8, 8);
            x.World.Map.SetFloor(t2, FloorType.Dirt);
            Assert.NotNull(winter.CanDo(c, t2, TileAction.Sow));
        }
    }
}
