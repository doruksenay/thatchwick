using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Farmyard;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 57-57e: yerdeki pislik ve sinek, açıkta tuvalet ve azar, kompost süreci, ahır kirliliği, sağlık, fare, araba filosu.
    public class AvluTests
    {
        private static readonly ContentDatabase Db = ContentFilesTests.LoadDb(out _);

        private static WorldState World(int w = 30, int h = 20)
        {
            var world = new WorldState(new GridMap(w, h));
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) world.Map.SetFloor(new GridPos(x, y), FloorType.Dirt);
            return world;
        }

        private static Character Person(int id, int x, int y, AgeStage stage = AgeStage.Adult, bool villager = false)
        {
            var c = new Character(id, "K" + id, new GridPos(x, y), !villager) { CanStarve = false, IsVillager = villager, MoodEnabled = true };
            c.SetAge(stage == AgeStage.Child ? c.AgeConfig.ChildStartDay + 1 : c.AgeConfig.AdultStartDay + 1);
            Assert.Equal(stage, c.Stage);
            return c;
        }

        private static ItemInstance Put(WorldState world, string id, int x, int y) => world.PlaceItem(Db.Items.Get(id), new GridPos(x, y));

        // ---------- yığınlar ----------

        [Fact]
        public void Pislik_SineklenirKokarKururVeTopragaKarisir()
        {
            var world = World();
            var filth = new FilthService(world, Db) { TemperatureAt = _ => 15f };
            var decayed = new List<PileKind>();
            filth.Decayed += (k, _) => decayed.Add(k);
            var pile = filth.Drop(PileKind.Human, new GridPos(5, 5));
            Assert.NotNull(pile);
            Assert.Equal(PileStage.Fresh, filth.StageOf(pile));
            filth.Tick(0.2); filth.Refresh();
            Assert.Equal(PileStage.Flies, filth.StageOf(pile));
            Assert.Single(filth.Sources);
            Assert.True(filth.HasFlies(filth.Sources[0]));
            filth.Tick(1.0); filth.Refresh();
            Assert.Equal(PileStage.Reeking, filth.StageOf(pile));
            Assert.Equal(2, filth.Sources[0].Tier);
            filth.Tick(1.0); filth.Refresh();
            Assert.Equal(PileStage.Dried, filth.StageOf(pile));
            Assert.Empty(filth.Sources);
            filth.Tick(2.0);
            Assert.Empty(filth.Piles);
            Assert.Null(world.GetItem(pile.Id));
            Assert.Equal(new[] { PileKind.Human }, decayed);
        }

        [Fact]
        public void Pislik_SogukYavaslatirDonDurdurur_YagmurdaSinekYok()
        {
            var world = World();
            float temp = 4f; bool rain = false;
            var filth = new FilthService(world, Db) { TemperatureAt = _ => temp, Raining = () => rain };
            var pile = filth.Drop(PileKind.Human, new GridPos(5, 5));
            filth.Tick(0.2);
            Assert.Equal(PileStage.Fresh, filth.StageOf(pile));      // 0,2 gün × 0,3 = 0,06 < 0,125
            temp = -3f; filth.Tick(5);
            Assert.Equal(PileStage.Fresh, filth.StageOf(pile));      // donda ilerlemez
            temp = 15f; filth.Tick(0.2); filth.Refresh();
            Assert.True(filth.HasFlies(filth.Sources[0]));
            rain = true;
            Assert.False(filth.HasFlies(filth.Sources[0]));          // açıkta, yağmurda sinek uçmaz
            Assert.Equal(0, filth.FlyCount(filth.Sources[0]));
            temp = 5f; rain = false;
            Assert.False(filth.HasFlies(filth.Sources[0]));          // soğukta sinek yok
        }

        [Fact]
        public void Pislik_YagmurIlerletir_FirtinaSiler_TavanAsilmaz()
        {
            var world = World();
            var cfg = new YardConfig { MaxPiles = 3 };
            var filth = new FilthService(world, Db, cfg) { TemperatureAt = _ => 15f };
            var a = filth.Drop(PileKind.Human, new GridPos(2, 2));
            var b = filth.Drop(PileKind.Animal, new GridPos(4, 2));
            var c = filth.Drop(PileKind.Refuse, new GridPos(6, 2));
            Assert.Null(filth.Drop(PileKind.Dog, new GridPos(8, 2)));           // tavan
            filth.OnNewDay(rained: true, stormed: false);
            Assert.Equal(PileStage.Flies, filth.StageOf(a));
            filth.OnNewDay(rained: true, stormed: true);
            Assert.Null(world.GetItem(a.Id));
            Assert.Null(world.GetItem(b.Id));
            Assert.NotNull(world.GetItem(c.Id));                                 // süprüntü kendiliğinden gitmez
            // aynı karoya ikinci yığın konmaz, yanına düşer
            var d = filth.Drop(PileKind.Human, new GridPos(6, 2));
            Assert.NotNull(d);
            Assert.NotEqual(new GridPos(6, 2), d.Position);
        }

        [Fact]
        public void Pislik_BasincaYakalanir_KurumusaBasilmaz_AyniKarodaTekrarYok()
        {
            var world = World();
            var cfg = new YardConfig { StepChance = 1.0, StepChanceDark = 1.0 };
            var filth = new FilthService(world, Db, cfg) { TemperatureAt = _ => 15f };
            var pile = filth.Drop(PileKind.Dog, new GridPos(5, 5));
            Assert.Null(filth.StepCheck(1, new GridPos(4, 5), false));
            Assert.Same(pile, filth.StepCheck(1, new GridPos(5, 5), false));
            Assert.Null(filth.StepCheck(1, new GridPos(5, 5), false));           // yerinde duran yeniden basmaz
            var tezek = filth.Drop(PileKind.Animal, new GridPos(8, 5));
            Assert.Null(filth.StepCheck(1, new GridPos(8, 5), false));           // tezeğe basmak moodlet vermez
            var human = filth.Drop(PileKind.Human, new GridPos(10, 5));
            filth.Tick(2.5);
            Assert.Equal(PileStage.Dried, filth.StageOf(human));
            Assert.Null(filth.StepCheck(1, new GridPos(10, 5), false));
        }

        [Fact]
        public void Pislik_OdaGuzelliginiDusurur_KayittanDoner()
        {
            var world = World();
            var filth = new FilthService(world, Db) { TemperatureAt = _ => 15f };
            var pile = filth.Drop(PileKind.Human, new GridPos(5, 5));
            filth.Tick(1.2);
            var save = filth.Export();
            var again = new FilthService(world, Db) { TemperatureAt = _ => 15f };
            again.Import(save);
            Assert.Equal(PileStage.Reeking, again.StageOf(pile));
            Assert.Equal(1, again.ReekingCount);
            Assert.NotNull(again.Nearest(new GridPos(6, 6), 2, 2));
            Assert.Null(again.Nearest(new GridPos(12, 12), 2, 2));
        }

        // ---------- açıkta tuvalet ----------

        [Fact]
        public void Tuvalet_SiperDibiniSecer_YoksaAcigaYapar()
        {
            var world = World();
            var filth = new FilthService(world, Db);
            var relief = new ReliefWork(world, filth);
            var c = Person(1, 10, 10);
            var open = relief.FindSpot(c);
            Assert.True(open.HasValue);
            Assert.False(relief.HasCoverNear(open.Value));
            var tree = Put(world, "item.tree_oak", 15, 10);
            PlantSystem.SetGrown(tree);
            var spot = relief.FindSpot(c);
            Assert.True(relief.HasCoverNear(spot.Value));                        // beş karo ötedeki ağaç, yanı başındaki açıklığın önünde
        }

        [Fact]
        public void Tuvalet_AciktaGorenAzarlar_SiperdeUzaktakiGormez()
        {
            var world = World();
            var filth = new FilthService(world, Db);
            var actor = Person(1, 10, 10);
            var witness = Person(2, 14, 10);
            var relief = new ReliefWork(world, filth) { People = () => new[] { actor, witness } };
            actor.Needs.Set(NeedType.Bladder, 10f);
            (bool covered, Character seen)? got = null;
            relief.Relieved += (a, pile, covered, w) => { got = (covered, w); Assert.NotNull(pile); };
            relief.Apply(actor, actor.Position, TileAction.Relieve);
            Assert.Equal(80f, actor.Needs.Get(NeedType.Bladder));
            Assert.False(got.Value.covered);
            Assert.Same(witness, got.Value.seen);                                // dört karo: açıkta görür
            Assert.Single(filth.Piles);

            var tree = Put(world, "item.tree_oak", 11, 11);
            PlantSystem.SetGrown(tree);
            relief.Apply(actor, actor.Position, TileAction.Relieve);
            Assert.True(got.Value.covered);
            Assert.Null(got.Value.seen);                                         // siper arkasında dört karo ötedeki görmez
            witness.Asleep = true;
            Assert.Null(relief.FindWitness(actor, 6));
        }

        [Fact]
        public void Azar_KufurluReplikYalnizOnSekizPaketinde()
        {
            var adult = Person(2, 0, 0);
            var child = Person(3, 0, 0, AgeStage.Child);
            var actor = Person(1, 0, 0);
            for (int roll = 0; roll < 12; roll++)
            {
                Assert.DoesNotContain(".adult.", ReliefWork.ScoldLineKey(adult, actor, true, false, roll));
                Assert.Contains(".adult.", ReliefWork.ScoldLineKey(adult, actor, true, true, roll));
                Assert.Equal("yard.scold.child", ReliefWork.ScoldLineKey(child, actor, true, true, roll));   // çocuk küfretmez
            }
        }

        // ---------- kompost ----------

        private static (WorldState world, Household house, CompostService compost, ItemInstance heap) Compost(double[] day)
        {
            var world = World();
            var house = new Household(0);
            var compost = new CompostService(world, house, Db) { Now = () => day[0], TemperatureAt = _ => 18f };
            return (world, house, compost, Put(world, "item.compost_heap", 5, 5));
        }

        [Fact]
        public void Kompost_DengeliVeCevrilmisYiginKaraToprakVerir()
        {
            var day = new[] { 0.0 };
            var (_, house, compost, heap) = Compost(day);
            compost.Add(heap, 8, 8);
            CompostGrade? grade = null; int count = 0;
            compost.Finished += (_, g, n) => { grade = g; count = n; };
            for (int d = 0; d < 12 && grade == null; d++)
            {
                if (compost.CanTurn(heap) && compost.NeedsTurn(heap)) compost.Turn(heap);
                day[0] += 1; compost.OnNewDay();
            }
            Assert.Equal(CompostGrade.BlackSoil, grade);
            Assert.Equal(8, count);
            Assert.True(day[0] <= 6);                                            // 7 gün / 1,5 çevirme hızı ≈ 5 gün
            var got = compost.Take(heap);
            Assert.Equal(8, got[2]);
            Assert.Equal(8, house.Count("item.black_soil"));
        }

        [Fact]
        public void Kompost_BakimsizYasYiginKokarVeHamKalir_PislikliyseRiskTasir()
        {
            var day = new[] { 0.0 };
            var (_, house, compost, heap) = Compost(day);
            compost.Add(heap, 14, 2, 0, filth: 2);
            CompostGrade? grade = null;
            compost.Finished += (_, g, n) => grade = g;
            for (int d = 0; d < 40 && grade == null; d++)
            {
                day[0] += 1; compost.OnNewDay();
                if (d == 4) Assert.Equal(2, compost.SmellTier(heap));            // fazla yaş ve çevrilmemiş: kokar
            }
            Assert.Equal(CompostGrade.Raw, grade);
            Assert.True(day[0] > 10);                                            // dengeli yığından çok yavaş
            compost.Take(heap);
            Assert.True(house.Count("item.compost_raw") > 0);
            Assert.True(compost.RawCompostHasFilth());
        }

        [Fact]
        public void Kompost_AzDoluYiginIsinmaz_FazlaKulKaliteyiDusurur()
        {
            var day = new[] { 0.0 };
            var (_, _, compost, heap) = Compost(day);
            compost.Add(heap, 3, 3);                                             // 6 birim: ısınmaz
            CompostGrade? grade = null;
            compost.Finished += (_, g, n) => grade = g;
            for (int d = 0; d < 40 && grade == null; d++) { compost.Turn(heap); day[0] += 1; compost.OnNewDay(); }
            Assert.Equal(CompostGrade.Raw, grade);

            grade = null;
            compost.Add(heap, 7, 7, ash: 4);                                     // kül payı %22
            for (int d = 0; d < 40 && grade == null; d++) { compost.Turn(heap); day[0] += 1; compost.OnNewDay(); }
            Assert.Equal(CompostGrade.Mature, grade);                            // kara toprak olacakken bir kademe düşer
        }

        [Fact]
        public void GubreYigini_AyriStoktur_PislikBirimiGubreyeDoner()
        {
            var day = new[] { 0.0 };
            var (world, house, compost, _) = Compost(day);
            Assert.Equal(1, compost.AddDung(5));                                 // yığın yokken envantere, taze
            Assert.Equal(1, house.Count("item.manure_fresh"));
            Assert.Equal(1, compost.PendingDungUnits);
            var dung = Put(world, "item.dung_heap", 10, 5);
            Assert.Equal(2, compost.AddDung(7));
            Assert.Equal(2, compost.FreshIn(dung));                              // yığında taze bekler
            Assert.Equal(0, compost.Count("item.manure"));
            Assert.Equal(1, compost.StackFresh());                               // envanterdeki taze de yığına
            Assert.Equal(0, house.Count("item.manure_fresh"));
            day[0] = 5; compost.OnNewDay();
            Assert.Equal(0, dung.Fill);
            day[0] = 6; compost.OnNewDay();                                      // altı günde olgunlaşır
            Assert.Equal(3, dung.Fill);
            Assert.Equal(0, compost.FreshIn(dung));
            Assert.Equal(37, compost.Room("item.manure"));
            Assert.Equal(2, compost.Withdraw("item.manure", 2));
            Assert.Equal(1, dung.Fill);
            Assert.Equal(0, compost.Count("item.compost"));
            Assert.Equal(1, compost.SmellTier(dung));
        }

        [Fact]
        public void SolucanKasasi_YaprakAgili_SerbetFicisi()
        {
            var day = new[] { 0.0 };
            var (world, house, compost, _) = Compost(day);
            var worms = Put(world, "item.worm_bin", 8, 8);
            house.AddItem("item.herb", 6);
            Assert.Equal(5, compost.FeedWorms(worms));
            for (int d = 0; d < 3; d++) { day[0] += 1; compost.OnNewDay(); }
            Assert.Equal(1, house.Count("item.compost"));
            Assert.Equal(1, house.Count("item.viv_worm"));

            var leaves = Put(world, "item.leaf_bin", 10, 8);
            house.AddItem("item.leaves", 12);
            Assert.Equal(12, compost.FillLeaves(leaves));
            for (int d = 0; d < 19; d++) { day[0] += 1; compost.OnNewDay(); }
            Assert.Equal(0, house.Count("item.leaf_mould"));
            day[0] += 2; compost.OnNewDay();
            Assert.Equal(6, house.Count("item.leaf_mould"));

            var barrel = Put(world, "item.slurry_barrel", 12, 8);
            Assert.False(compost.BrewSlurry(barrel));                            // gübre yok
            house.AddItem("item.manure", 2);
            Assert.True(compost.BrewSlurry(barrel));
            Assert.Equal(1, compost.SlurryState(barrel));
            day[0] += 2.5;
            Assert.Equal(2, compost.SlurryState(barrel));
            Assert.False(compost.FertigationActive());                           // sulama yoluna bağlı değil
            day[0] += 6;
            Assert.Equal(0, compost.SlurryState(barrel));
        }

        // ---------- ahır ----------

        private sealed class Farm
        {
            public WorldState World; public Household House; public AnimalSystem Animals; public FilthService Filth;
            public CompostService Compost; public BarnService Barn; public ItemInstance Home; public double Day;
        }

        private static Farm Barn(string home = "item.barn", string species = "animal.cow", int count = 2)
        {
            var f = new Farm { World = World(), House = new Household(0) };
            f.Animals = new AnimalSystem(f.World, Db, 7);
            f.Filth = new FilthService(f.World, Db) { TemperatureAt = _ => 15f };
            f.Compost = new CompostService(f.World, f.House, Db) { Now = () => f.Day };
            f.Barn = new BarnService(f.World, f.Animals, f.House, f.Filth, f.Compost) { Grazing = () => false };
            f.Home = Put(f.World, home, 5, 5);
            for (int i = 0; i < count; i++) Animal(f, species, 100 + i, 9 + i, 9);
            return f;
        }

        private static Animal Animal(Farm f, string species, int id, int x, int y) =>
            f.Animals.Restore(id, Db.Animals.Get(species), new GridPos(x, y), f.Home.Id, 100f, 30, 0, 0);

        [Fact]
        public void Ahir_KirlenirKonforuVeSagligiBozar_TemizlenenceGubreCikar()
        {
            var f = Barn();
            f.Barn.OnNewDay(); f.Barn.OnNewDay();
            Assert.Equal(48f, f.Barn.DirtAt(f.Home));
            Assert.Equal(0f, f.Barn.ComfortPenalty(f.Home));
            f.Barn.OnNewDay();
            Assert.True(f.Barn.IsDirty(f.Home));
            Assert.Equal(5f, f.Barn.ComfortPenalty(f.Home));
            Assert.Equal(0.004, f.Barn.SickRisk(f.Home));
            Assert.Equal((1, "barn"), f.Barn.SmellTier(f.Home));
            f.Barn.OnNewDay();
            Assert.True(f.Barn.IsFilthy(f.Home));
            Assert.Equal(15f, f.Barn.ComfortPenalty(f.Home));
            Assert.True(f.Barn.NeedsCleaning(f.Home));
            Assert.Equal(2, f.Barn.Clean(f.Home));                               // 9,6 birim ≈ 10: iki gübre, iki birim bekler
            Assert.Equal(2, f.House.Count("item.manure_fresh"));              // ahırdan çıkan taze gübredir
            Assert.Equal(0f, f.Barn.DirtAt(f.Home));
            Assert.Equal((0, "barn"), f.Barn.SmellTier(f.Home));
        }

        [Fact]
        public void Ahir_DerinAltlikSamanYerAzKirlenir_OlukYarilar_KapakHizlandirir()
        {
            var f = Barn();
            f.Barn.SetDeepLitter(f.Home, true);
            f.House.AddItem("item.straw", 4);
            f.Barn.OnNewDay();
            Assert.Equal(6f, f.Barn.DirtAt(f.Home));                             // 24 × 0,25
            Assert.Equal(2, f.House.Count("item.straw"));                        // büyükbaş başına bir saman
            f.Barn.OnNewDay();
            bool shortOf = false;
            f.Barn.LitterShort += _ => shortOf = true;
            f.Barn.OnNewDay();                                                   // saman bitti
            Assert.True(shortOf);
            Assert.Equal(36f, f.Barn.DirtAt(f.Home));
            Assert.Equal(1, f.Barn.Clean(f.Home));                               // 7,2 birim

            var g = Barn();
            Put(g.World, "item.barn_drain", 9, 5);
            g.Barn.OnNewDay();
            Assert.Equal(12f, g.Barn.DirtAt(g.Home));
            Assert.Equal(1.0, g.Barn.CleanDurationFactor(g.Home));
            Put(g.World, "item.manure_hatch", 5, 8);
            Assert.Equal(1.0, g.Barn.CleanDurationFactor(g.Home));               // gübre yığını yokken kapak işlemez
            Put(g.World, "item.dung_heap", 12, 12);
            Assert.Equal(0.5, g.Barn.CleanDurationFactor(g.Home));
        }

        [Fact]
        public void Ahir_KumesAzKirletir_KediKopekSaymaz_AgilNadasiBesler()
        {
            var f = Barn("item.coop", "animal.chicken", 4);
            f.Barn.OnNewDay();
            Assert.Equal(12f, f.Barn.DirtAt(f.Home));
            Assert.Equal(0f, f.Barn.DirtOf(Db.Animals.Get("animal.dog")));
            Assert.Equal(0f, f.Barn.DirtOf(Db.Animals.Get("animal.bees")));

            var g = Barn("item.barn", "animal.sheep", 4);
            var plants = new PlantSystem(g.World, seed: 3) { Now = () => g.Day };
            g.Barn.Plants = plants;
            g.Barn.Grazing = () => true;
            Put(g.World, "item.fold_hurdle", 20, 10);
            var plot = plants.Plot(new GridPos(21, 10));
            plot.Fertility = 0.30f; plot.Worked = true;
            g.Barn.OnNewDay();
            Assert.True(plants.FertilityAt(new GridPos(21, 10)) > 0.335f);       // dört davar: tam etki 0,04
            for (int i = 0; i < 30; i++) g.Barn.OnNewDay();
            Assert.True(plants.FertilityAt(new GridPos(21, 10)) <= 0.7001f);     // tavan
            Assert.Null(plants.PlotOrNull(new GridPos(25, 15)));                 // sürülmemiş toprağa işlemez
        }

        [Fact]
        public void Ahir_KopekPisligiBirakir_KediGomer_MeradaTezekVeKigi()
        {
            var cfg = new YardConfig { DogDropChance = 1.0, PastureDropChance = 1.0 };
            var f = new Farm { World = World(), House = new Household(0) };
            f.Animals = new AnimalSystem(f.World, Db, 7);
            f.Filth = new FilthService(f.World, Db, cfg) { TemperatureAt = _ => 15f };
            f.Compost = new CompostService(f.World, f.House, Db, cfg);
            f.Barn = new BarnService(f.World, f.Animals, f.House, f.Filth, f.Compost) { Grazing = () => true };
            f.Home = Put(f.World, "item.barn", 5, 5);
            Animal(f, "animal.cow", 1, 12, 12);
            Animal(f, "animal.sheep", 2, 14, 12);
            Animal(f, "animal.dog", 3, 16, 12);
            Animal(f, "animal.cat", 4, 18, 12);
            int buried = 0;
            f.Barn.CatBuried += _ => buried++;
            f.Barn.OnNewDay();
            f.Barn.Tick(0.9);
            Assert.Equal(1, buried);
            var kinds = f.Filth.Piles.Select(p => f.World.GetItem(p.ItemId).Def.Id).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "item.animal_dung", "item.dog_pile", "item.sheep_dung" }, kinds);
        }

        // ---------- sağlık ve fare ----------

        [Fact]
        public void Saglik_KirliElKarinAgritir_YikaninceGecer_KuyuBulanir()
        {
            var world = World();
            var house = new Household(0);
            var cfg = new YardConfig { DirtyHandsAche = 1.0, MurkyWaterAche = 1.0 };
            var filth = new FilthService(world, Db, cfg) { TemperatureAt = _ => 15f };
            var health = new YardHealth(world, house, filth);
            Assert.Equal(AcheCause.None, health.OnAte(1, new GridPos(3, 3)));
            health.MarkDirty(1);
            Assert.Equal(AcheCause.DirtyHands, health.OnAte(1, new GridPos(3, 3)));
            health.Wash(1);
            Assert.Equal(AcheCause.None, health.OnAte(1, new GridPos(3, 3)));

            var well = Put(world, "item.well", 10, 10);
            Assert.False(health.WaterFoul(well));
            filth.Drop(PileKind.Human, new GridPos(11, 11));
            filth.Drop(PileKind.Human, new GridPos(12, 10));
            filth.Tick(1.2); filth.Refresh();                                    // iki kokmuş yığın: 4 puan
            Assert.True(health.WaterFoul(well));
            Assert.Equal(AcheCause.MurkyWater, health.OnDrewWater(well));
        }

        [Fact]
        public void Fare_BayatSupruntuCagirir_KilerdenYer_KediTutar()
        {
            var world = World();
            var house = new Household(0);
            var filth = new FilthService(world, Db) { TemperatureAt = _ => 15f };
            var health = new YardHealth(world, house, filth);
            house.AddItem("item.wheat", 10);
            filth.Drop(PileKind.Refuse, new GridPos(5, 5));
            filth.Tick(3.5); filth.Refresh();
            var eaten = health.OnNewDay(0, out int caught);
            Assert.Equal(1, health.Mice);
            Assert.Equal(new[] { "item.wheat" }, eaten.ToArray());
            health.OnNewDay(0, out _);
            Assert.Equal(2, health.Mice);
            Assert.Equal(7, house.Count("item.wheat"));
            eaten = health.OnNewDay(5, out caught);
            Assert.Equal(3, caught);                                             // üçüncü fare de geldi, kediler hepsini tuttu
            Assert.Equal(0, health.Mice);
            Assert.Empty(eaten);
        }

        // ---------- araba filosu ----------

        private static (Farm f, CartFleet fleet, Character driver, Character driver2) Fleet()
        {
            var f = Barn("item.barn", "animal.ox", 1);
            Animal(f, "animal.horse", 200, 12, 9);
            var d1 = Person(1, 3, 3); var d2 = Person(2, 4, 3);
            var fleet = new CartFleet(f.World, f.House, f.Animals) { FindCharacter = id => id == 1 ? d1 : id == 2 ? d2 : null };
            f.House.AddItem("item.horse_cart", 2);
            return (f, fleet, d1, d2);
        }

        [Fact]
        public void Araba_HerArabaAyriGorev_HayvanTekArabayaKosulur()
        {
            var (f, fleet, d1, d2) = Fleet();
            Assert.Equal(2, fleet.Carts.Count);
            var a = fleet.Carts[0]; var b = fleet.Carts[1];
            Assert.Equal("yard.cart.b.wrong_animal", fleet.Assign(a, CartJob.Field, 100, 1, 0).Key);   // öküz hafif arabayı çekmez
            Assert.True(fleet.Assign(a, CartJob.Field, 200, 1, 0).IsEmpty);
            Assert.Equal("yard.cart.b.busy", fleet.Assign(b, CartJob.Mine, 200, 2, 0).Key);            // at başka arabada
            Assert.Equal(CartJob.Idle, b.Job);
            Animal(f, "animal.mule", 201, 13, 9);
            Assert.Equal("yard.cart.b.driver_busy", fleet.Assign(b, CartJob.Mine, 201, 1, 0).Key);     // arabacı tek arabada
            Assert.True(fleet.Assign(b, CartJob.Mine, 201, 2, 0).IsEmpty);
            var working = fleet.OnNewDay(0);
            Assert.Equal(2, working.Count);                                                           // iki araba iki görevi aynı gün yapar
            Assert.Equal(1, fleet.WorkingCount(CartJob.Mine));
            Assert.Equal(8, fleet.WorkingCapacity(CartJob.Field));
            Assert.Equal(0.15, fleet.HaulBonus(CartJob.Mine));
            Assert.Equal(1, fleet.FreeDraftAnimals());                                                // yalnızca öküz boşta
        }

        [Fact]
        public void Araba_YorgunHayvanSecilinceUyariVerir_BirGunDinlenir()
        {
            var (f, fleet, d1, _) = Fleet();
            var cart = fleet.Carts[0];
            Assert.True(fleet.Assign(cart, CartJob.Market, 200, 1, 0).IsEmpty);
            var horse = f.Animals.Get(200);
            for (long day = 0; day < 3; day++) Assert.Single(fleet.OnNewDay(day));
            Assert.True(fleet.IsTired(200, 3));
            var why = fleet.AnimalBlocker(cart, horse, 3);
            Assert.Equal("yard.cart.b.tired", why.Key);                                               // "bugün çok yoruldu, yapamaz"
            var stuck = new List<(Cart, Localization.LocText)>();
            Assert.Empty(fleet.OnNewDay(3, stuck));
            Assert.Single(stuck);
            Assert.False(fleet.MarketBlocker(3).IsEmpty);
            Assert.False(fleet.IsTired(200, 4));                                                      // bir gün dinlendi
            Assert.Single(fleet.OnNewDay(4));
        }

        [Fact]
        public void Araba_HayvansizGitmez_ElArabasiHayvanIstemez_TekerlekKirilinceDurur()
        {
            var f = Barn("item.barn", "animal.sheep", 1);
            var d1 = Person(1, 3, 3);
            var fleet = new CartFleet(f.World, f.House, f.Animals) { FindCharacter = id => id == 1 ? d1 : null };
            f.House.AddItem("item.horse_cart", 1);
            f.House.AddItem("item.wheelbarrow", 1);
            var cart = fleet.Find("item.horse_cart#0"); var barrow = fleet.Find("item.wheelbarrow#0");
            Assert.Equal("yard.cart.b.no_animal", fleet.Assign(cart, CartJob.Field, 0, 1, 0).Key);
            Assert.Equal("yard.cart.b.barrow_far", fleet.Assign(barrow, CartJob.Mine, 0, 1, 0).Key);
            Assert.True(fleet.Assign(barrow, CartJob.Field, 0, 1, 0).IsEmpty);
            Assert.Single(fleet.OnNewDay(0));
            Assert.Equal(4, fleet.WorkingCapacity(CartJob.Field));
            barrow.Wear = 1.0;
            Assert.Equal("yard.cart.b.wheel", fleet.Blocker(barrow, 1).Key);
            Assert.False(fleet.Repair(barrow));
            f.House.AddItem("item.cart_wheel", 1);
            Assert.True(fleet.Repair(barrow));
            Assert.True(fleet.Blocker(barrow, 1).IsEmpty);
        }

        [Fact]
        public void Araba_EskiKayittaOkuzArabasiPazaraGider_KayitGeriGelir()
        {
            var (f, fleet, d1, _) = Fleet();
            var ox = Put(f.World, "item.ox_cart", 20, 10);
            var placed = fleet.Find("item:" + ox.Id);
            Assert.Equal(CartJob.Market, placed.Job);                                                 // eski davranış sürer
            Assert.True(fleet.MarketBlocker(0).IsEmpty);                                              // hayvan kendiliğinden seçilir
            fleet.Assign(fleet.Carts.First(c => c.DefId == "item.horse_cart"), CartJob.Forest, 200, 1, 0);
            fleet.FertilizeRule = true;
            fleet.OnNewDay(0);
            var save = fleet.Export();
            var again = new CartFleet(f.World, f.House, f.Animals) { FindCharacter = id => id == 1 ? d1 : null };
            again.Import(save);
            Assert.True(again.FertilizeRule);
            Assert.Equal(CartJob.Market, again.Find("item:" + ox.Id).Job);
            var horseCart = again.Carts.First(c => c.Job == CartJob.Forest);
            Assert.Equal(200, horseCart.AnimalId);
            Assert.Equal(1, again.WorkStreak(200, 1));
            again.Assign(again.Find("item:" + ox.Id), CartJob.Idle, 0, 0, 1);
            var third = new CartFleet(f.World, f.House, f.Animals);
            third.Import(again.Export());
            Assert.Equal(CartJob.Idle, third.Find("item:" + ox.Id).Job);                              // oyuncunun kararı korunur
        }

        [Fact]
        public void Araba_EnvanterdenAvluyaGocer_SundurmaYagmurdanKorur_SeferEvreleri()
        {
            var (f, fleet, d1, _) = Fleet();
            Assert.True(fleet.Assign(fleet.Carts[0], CartJob.Mine, 200, 1, 0).IsEmpty);
            fleet.Carts[0].Wear = 0.4;
            int next = 0;
            var spots = new[] { new GridPos(20, 15), new GridPos(24, 15) };
            Assert.Equal(2, fleet.ParkInventory(Db, def => next < spots.Length ? spots[next++] : (GridPos?)null));
            Assert.Equal(0, f.House.Count("item.horse_cart"));
            Assert.Equal(2, f.World.Items.Count(i => i.Def.Id == "item.horse_cart"));
            var mine = fleet.Carts.Single(c => c.Job == CartJob.Mine);
            Assert.True(mine.ItemId != 0);                                                            // görev, hayvan ve yıpranma taşındı
            Assert.Equal(200, mine.AnimalId);
            Assert.Equal(0.4, mine.Wear);
            Assert.Equal(0, fleet.ParkInventory(Db, def => new GridPos(1, 1)));                       // ikinci kez göçmez

            Assert.False(fleet.Sheltered(mine));
            Assert.Equal(2, fleet.RainOnCarts());
            Assert.Equal(0.41, mine.Wear, 6);
            var item = f.World.GetItem(mine.ItemId);
            Put(f.World, "item.cart_shed", item.Position.X, item.Position.Y + 2);
            Assert.True(fleet.Sheltered(mine));
            Assert.Equal(1, fleet.RainOnCarts());                                                     // yalnızca açıktaki ıslanır

            Assert.Equal((0, 0f), fleet.TripPhase(mine, 0.30));                                       // bugün çalışmadı
            fleet.OnNewDay(0);
            Assert.Equal(0, fleet.TripPhase(mine, 0.10).phase);
            Assert.Equal(1, fleet.TripPhase(mine, 0.30).phase);
            Assert.Equal(2, fleet.TripPhase(mine, 0.50).phase);
            Assert.Equal(3, fleet.TripPhase(mine, 0.73).phase);
            Assert.Equal(0, fleet.TripPhase(mine, 0.90).phase);
            // kayıt: yerleşik araba anahtarıyla geri gelir
            var again = new CartFleet(f.World, f.House, f.Animals) { FindCharacter = id => d1 };
            again.Import(fleet.Export());
            Assert.Equal(CartJob.Mine, again.Find(mine.Key).Job);
        }

        // ---------- bütün ----------

        private sealed class Game
        {
            public WorldState World; public Household House; public Population Pop; public GameClock Clock; public LifeSystems Life;
            public InteractionEffects Effects; public PlantSystem Plants; public AnimalSystem Animals;
        }

        private static Game Whole()
        {
            var g = new Game { World = World(), House = new Household(0), Pop = new Population(), Clock = new GameClock() };
            g.Life = new LifeSystems(Db, g.World, g.House, g.Pop, g.Clock);
            g.Plants = new PlantSystem(g.World, seed: 5);
            g.Animals = new AnimalSystem(g.World, Db, 7);
            g.Effects = new InteractionEffects(g.World, g.House, Db, 7) { Plants = g.Plants, Animals = g.Animals };
            g.Life.Attach(g.Plants);
            g.Life.Attach(g.Animals, null);
            g.Life.Attach(g.Effects);
            return g;
        }

        private static string Use(Game g, Character c, ItemInstance item, string interaction)
        {
            var def = item.Def.Interactions.Find(i => i.Id == interaction);
            string blocked = g.Effects.CanStart(c, item, def);
            if (blocked != null) return blocked;
            var r = new JobRunner(c, g.World) { Effects = g.Effects };
            r.Command(Job.UseItem(item.Id, interaction));
            for (int i = 0; i < 200 && !r.IsIdle; i++) r.Tick(0.01);
            return null;
        }

        [Fact]
        public void Butun_YiginToplanirGubreOlur_ElKirlenir_GorevliListesineGirer()
        {
            var g = Whole();
            var c = Person(1, 6, 5);
            g.Pop.Add(c);
            var dung = Put(g.World, "item.dung_heap", 12, 5);
            var a = g.Life.Filth.Drop(PileKind.Animal, new GridPos(5, 5));
            var b = g.Life.Filth.Drop(PileKind.Animal, new GridPos(5, 7));
            var h = g.Life.Filth.Drop(PileKind.Human, new GridPos(8, 8));
            Assert.Equal(2, g.Life.YardChores(DutyKind.Animals).Count());
            Assert.Single(g.Life.YardChores(DutyKind.House));
            Assert.Null(Use(g, c, a, "collect"));
            Assert.Null(Use(g, c, b, "collect"));
            Assert.Equal(1, g.Life.Compost.FreshIn(dung));                       // 2 + 2 birim = 1 taze gübre, yığında olgunlaşacak
            Assert.True(g.Life.YardHealth.HasDirtyHands(c.Id));
            Assert.Empty(g.Life.YardChores(DutyKind.Animals));
            Assert.Null(Use(g, c, h, "collect"));
            Assert.True(c.Mood.Has(LifeSystems.MoodDisgusted));
            Assert.Empty(g.Life.Filth.Piles);
        }

        [Fact]
        public void Butun_KompostEtkilesimleriVeGubreYiginindanSerme()
        {
            var g = Whole();
            var c = Person(1, 6, 5);
            g.Pop.Add(c);
            var heap = Put(g.World, "item.compost_heap", 5, 5);
            Assert.NotNull(Use(g, c, heap, "add_wet"));                          // girdi yok
            g.House.AddItem("item.herb", 5); g.House.AddItem("item.straw", 5);
            Assert.Null(Use(g, c, heap, "add_wet"));
            Assert.Null(Use(g, c, heap, "add_dry"));
            Assert.Equal(10, g.Life.Compost.HeapOf(heap).Total);
            Assert.Equal(0, g.House.Count("item.herb"));
            Assert.NotNull(Use(g, c, heap, "take"));                             // henüz olmadı
            Assert.Null(Use(g, c, heap, "turn"));
            Assert.NotNull(Use(g, c, heap, "turn"));                             // yeni çevrildi

            // gübre yığınındaki gübre, envanterde yokken de ekine serilir
            var dung = Put(g.World, "item.dung_heap", 12, 5);
            g.Life.Compost.Deposit("item.manure", 3);
            g.World.Map.SetFloor(new GridPos(15, 5), FloorType.Dirt);
            var crop = Put(g.World, "item.crop_cabbage", 15, 5);
            for (int i = 0; i < 60; i++) c.GainSkillXp(SkillType.SoilCare, 5000);
            Assert.Null(Use(g, c, crop, "manure"));
            Assert.Equal(2, dung.Fill);
            Assert.True(g.Life.YardHealth.HasDirtyHands(c.Id));
        }

        [Fact]
        public void Butun_AvluKaydiGeriGelir()
        {
            var g = Whole();
            var pile = g.Life.Filth.Drop(PileKind.Human, new GridPos(5, 5));
            g.Life.Filth.Tick(1.5);
            var heap = Put(g.World, "item.compost_heap", 9, 9);
            g.Life.Compost.Add(heap, 6, 6);
            g.Life.YardHealth.MarkDirty(7);
            var barn = Put(g.World, "item.barn", 15, 10);
            g.Animals.Restore(1, Db.Animals.Get("animal.cow"), new GridPos(18, 12), barn.Id, 100f, 30, 0, 0);
            g.Life.Barn.OnNewDay();
            float dirt = g.Life.Barn.DirtAt(barn);
            Assert.True(dirt > 0f);
            var save = g.Life.Export();

            var again = Whole();
            // aynı dünya eşyaları (kimlikler aynı sırayla verilir)
            var pile2 = again.Life.Filth.Drop(PileKind.Human, new GridPos(5, 5));
            var heap2 = Put(again.World, "item.compost_heap", 9, 9);
            var barn2 = Put(again.World, "item.barn", 15, 10);
            Assert.Equal(pile.Id, pile2.Id);
            again.Life.Restore(save);
            Assert.Equal(PileStage.Reeking, again.Life.Filth.StageOf(pile2));
            Assert.Equal(12, again.Life.Compost.HeapOf(heap2).Total);
            Assert.True(again.Life.YardHealth.HasDirtyHands(7));
            Assert.Equal(dirt, again.Life.Barn.DirtAt(barn2));
        }

        [Fact]
        public void Butun_OlgunGubreYakmaz_TazeYakar_YaprakGuzunToplanir()
        {
            var g = Whole();
            var c = Person(1, 6, 5);
            g.Pop.Add(c);
            for (int i = 0; i < 60; i++) c.GainSkillXp(SkillType.SoilCare, 5000);
            var a = Put(g.World, "item.crop_carrot", 15, 5);
            var b = Put(g.World, "item.crop_carrot", 17, 5);
            g.House.AddItem("item.manure", 1); g.House.AddItem("item.manure_fresh", 1);
            Assert.Null(Use(g, c, a, "manure"));
            Assert.Null(Use(g, c, b, "manure_fresh"));
            Assert.True(g.Plants.Plot(a.Position).BurnUntil <= 0);               // olgun gübre az yiyeni de yakmaz
            Assert.True(g.Plants.Plot(b.Position).BurnUntil > 0);                // taze gübre yakar
            Assert.False(g.Plants.IsSpoiledByManure(a));
            Assert.True(g.Plants.IsSpoiledByManure(b));

            var tree = Put(g.World, "item.tree_oak", 20, 10);
            PlantSystem.SetGrown(tree);
            Assert.Equal(Season.Spring, g.Clock.Season);
            Assert.NotNull(Use(g, c, tree, "rake_leaves"));                      // yaprak güzün dökülür
        }

        [Fact]
        public void Butun_KahyaBostakiArabayaIsVerir_ElleVerilenGoreveDokunmaz()
        {
            var g = Whole();
            var steward = Person(1, 6, 5);
            g.Pop.Add(steward);
            steward.Duty = DutyKind.Steward;
            var barn = Put(g.World, "item.barn", 10, 10);
            g.Animals.Restore(1, Db.Animals.Get("animal.horse"), new GridPos(14, 12), barn.Id, 100f, 30, 0, 0);
            g.House.AddItem("item.wheelbarrow", 1);
            g.House.AddItem("item.compost", 3);
            var plot = g.Plants.Plot(new GridPos(22, 4));
            plot.Fertility = 0.2f; plot.Worked = true;
            g.Life.Fleet.FertilizeRule = true;
            g.Life.Fleet.StewardCarts = true;

            g.Life.RunFleetDay(0);
            Assert.Equal(0.2f, g.Plants.FertilityAt(new GridPos(22, 4)));        // Kâhyalık 4 yok: kâhya araba dağıtmaz
            for (int i = 0; i < 60; i++) steward.GainSkillXp(SkillType.Stewardship, 5000);
            g.Life.RunFleetDay(1);
            Assert.True(g.Plants.FertilityAt(new GridPos(22, 4)) > 0.35f);       // el arabası tarlaya çıktı, kompost serildi
            Assert.Equal(2, g.House.Count("item.compost"));
            Assert.All(g.Life.Fleet.Carts, cart => Assert.Equal(CartJob.Idle, cart.Job));   // gün bitince yine boşta
            Assert.Equal(1, g.World.Items.Count(i => i.Def.Id == "item.wheelbarrow"));      // araba avluya çekildi
        }

        [Fact]
        public void Icerik_HerEkinBeslenmeTipiBildirir_YeniEsyalarVar()
        {
            var file = ContentFilesTests.LoadAll();
            var missing = file.items.Where(i => !string.IsNullOrEmpty(i.cropFamily) && string.IsNullOrEmpty(i.feed)).Select(i => i.id).ToArray();
            Assert.True(missing.Length == 0, "beslenme tipi eksik: " + string.Join(", ", missing));
            foreach (var id in new[] { FilthService.HumanPileId, FilthService.DogPileId, FilthService.AnimalPileId, FilthService.SheepPileId, FilthService.RefusePileId,
                         CompostService.RawId, CompostService.BlackId, CompostService.FreshManureId, "item.bone_mill", "item.cart_shed", CompostService.LeavesId, CompostService.LeafMouldId, LifeSystems.SeaweedId, LifeSystems.MarlId, LifeSystems.DungCakeId })
                Assert.True(Db.Items.Contains(id), id);
            foreach (var id in new[] { LifeSystems.MoodStomachAche, LifeSystems.MoodScolded, LifeSystems.MoodSawTooMuch, LifeSystems.MoodSteppedInIt, LifeSystems.MoodBadSmell,
                         LifeSystems.MoodLostAppetite, LifeSystems.MoodMurkyWater, LifeSystems.MoodTidyYard, LifeSystems.MoodDisgusted })
                Assert.True(Person(1, 0, 0).AddMoodlet(id), id);
            Assert.Equal(FeedType.Heavy, Db.Items.Get("item.crop_cabbage").Feed);
            Assert.Equal(FeedType.Builder, Db.Items.Get("item.crop_pea").Feed);
            Assert.True(Db.Items.Get("item.crop_rye").HasTag("tall"));
            // Arabalar yerleştirilen eşya (belge 57e); olgun gübre ayrı tür, çok yiyen sever.
            foreach (var id in CartFleet.CartIds) Assert.NotEqual(ItemPlacement.Resource, Db.Items.Get(id).Placement);
            Assert.True((Db.Items.Get("item.crop_cabbage").LikesFertilizer & Fertilizer.Manure) != 0);
            Assert.Equal(Fertilizer.Manure, Db.Items.Get("item.crop_cabbage").Interactions.Find(i => i.Id == "manure").FertilizerKind);
            Assert.Equal(Fertilizer.FreshManure, Db.Items.Get("item.crop_cabbage").Interactions.Find(i => i.Id == "manure_fresh").FertilizerKind);
        }
    }
}
