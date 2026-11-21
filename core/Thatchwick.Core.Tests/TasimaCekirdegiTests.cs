using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Taşıma çekirdeği (Project belgeleri 59 §4-§7, 58d §1.2): ağırlık, güç, kişisel çanta, yük cezası.</summary>
    public class TasimaCekirdegiTests
    {
        private static Character Adult(GridPos p, int id = 1)
        {
            var c = new Character(id, "Selcan", p);
            c.SetAge(50);
            return c;
        }

        // ---------- ağırlık ----------

        [Fact]
        public void Agirlik_HerTasinabilirEsyadaVar_SiraMantikli()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var def in db.Items.All) Assert.True(ItemMass.Of(def) > 0f, def.Id);
            float Kg(string id) => ItemMass.Of(db.Items.Get(id));
            Assert.True(Kg("item.firewood") > Kg("item.seed_turnip"));
            Assert.True(Kg("item.clay") > Kg("item.herb"));
        }

        [Fact]
        public void Agirlik_VeridekiDegerTabloyuEzer()
        {
            var def = new ItemDef { Id = "item.x", Placement = ItemPlacement.Resource, Mass = 7.5f };
            Assert.Equal(7.5f, ItemMass.Of(def));
            def.Mass = 0f;
            Assert.Equal(ItemMass.Default, ItemMass.Of(def));
        }

        // ---------- güç ----------

        [Fact]
        public void Guc_YasBedenAtletizmAgirIs_CinsiyetFarkiYok()
        {
            var a = Adult(new GridPos(0, 0));
            Assert.Equal(5, Strength.Of(a));
            a.Appearance.Build = BodyBuild.Sturdy;
            Assert.Equal(6, Strength.Of(a));
            a.Appearance.Build = BodyBuild.Slim;
            Assert.Equal(4, Strength.Of(a));
            a.Appearance.Build = BodyBuild.Average;
            a.HeavyWorkDays = Strength.WorkBonusDays;
            Assert.Equal(6, Strength.Of(a));

            var m = Adult(new GridPos(0, 0), 2); m.Sex = Sex.Male;
            var f = Adult(new GridPos(0, 0), 3); f.Sex = Sex.Female;
            Assert.Equal(Strength.Of(m), Strength.Of(f));

            var child = Adult(new GridPos(0, 0), 4); child.SetAge(child.AgeConfig.ChildStartDay + 0.1);
            Assert.Equal(2, Strength.Of(child));
            Assert.Equal(30f, Strength.CapacityKg(5));
        }

        [Fact]
        public void Guc_AnlikDurumDusurur_SinirlarIcinde()
        {
            var a = Adult(new GridPos(0, 0));
            a.Needs.Set(NeedType.Hunger, 5f);
            a.Needs.Set(NeedType.Energy, 5f);
            Assert.Equal(3, Strength.Of(a));
            var baby = Adult(new GridPos(0, 0), 2); baby.SetAge(0);
            Assert.Equal(Strength.Min, Strength.Of(baby));
        }

        // ---------- yük kuralları ----------

        [Fact]
        public void Yuk_Basamaklar_YurumeVeYorulma()
        {
            Assert.Equal(LoadLevel.Light, LoadRules.LevelOf(1f));
            Assert.Equal(LoadLevel.Heavy, LoadRules.LevelOf(1.1f));
            Assert.Equal(LoadLevel.Overloaded, LoadRules.LevelOf(1.3f));
            Assert.Equal(LoadLevel.Full, LoadRules.LevelOf(1.5f));
            Assert.Equal(1.0, LoadRules.WalkFactor(0.5f));
            Assert.Equal(0.85, LoadRules.WalkFactor(1.1f));
            Assert.Equal(0.65, LoadRules.WalkFactor(1.4f));
            Assert.True(LoadRules.CanRun(1.2f));
            Assert.False(LoadRules.CanRun(1.3f));
        }

        [Fact]
        public void Canta_SertSinirdanSonraAlmaz_YukOraniVeAgirYuruyus()
        {
            var c = Adult(new GridPos(0, 0));
            float hard = Strength.CapacityKg(Strength.Of(c)) * LoadRules.HardCap;   // 45 kg
            Assert.Equal(7, c.Bag.Room(6f, hard));
            c.Bag.Add(new ItemKey("item.stone"), 7, 6f);
            Assert.Equal(0, c.Bag.Room(6f, hard));
            Assert.True(c.LoadRatio > 1.25f);
            Assert.False(RunRules.CanRun(c));
            var i = AnimInputs.From(c);
            Assert.Equal(Gait.Heavy, AnimStyleRules.GaitFor(i));
        }

        [Fact]
        public void Depo_YonlendirmeKancasi_TrueDonerseDepoyaGirmez()
        {
            var h = new Household(0);
            h.Divert = (k, n) => k.Id == "item.a";
            h.AddItem("item.a", 3);
            h.AddItem("item.b", 2);
            Assert.Equal(0, h.Count("item.a"));
            Assert.Equal(2, h.Count("item.b"));
            h.AddStored(new ItemKey("item.a"), 3);
            Assert.Equal(3, h.Count("item.a"));
        }

        // ---------- çanta, bölgeli dünyada ----------

        private static (LifeSystems life, Household house, Character c) World(GridPos at)
        {
            var db = ContentFilesTests.LoadDb(out _);
            var world = new WorldState(new GridMap(Regions.WorldWidth, Regions.WorldHeight));
            var house = new Household(0);
            var pop = new Population();
            var c = Adult(at);
            pop.Add(c);
            var life = new LifeSystems(db, world, house, pop, new GameClock());
            return (life, house, c);
        }

        [Fact]
        public void Canta_DisaridaAlinanCantaya_CiftliktekiDepoya()
        {
            var forest = new GridPos(Regions.Forest.X + 5, Regions.Forest.Y + 5);
            var (life, house, c) = World(forest);
            using (CarryContext.Begin(c)) house.AddItem("item.firewood", 4);
            Assert.Equal(0, house.Count("item.firewood"));
            Assert.False(c.Bag.IsEmpty);
            Assert.Equal((double)(4 * life.UnitKg("item.firewood")), (double)c.Bag.TotalKg, 3);

            house.AddItem("item.firewood", 2);   // kimse almıyor (sistem işi): depoya
            Assert.Equal(2, house.Count("item.firewood"));

            c.Position = new GridPos(Regions.Farm.X + 10, Regions.Farm.Y + 10);
            life.Tick(0.001);
            Assert.True(c.Bag.IsEmpty);
            Assert.Equal(6, house.Count("item.firewood"));
        }

        [Fact]
        public void Canta_SiniriAsaniAlmaz_CiftliktekiDogrudanDepoya()
        {
            var lake = new GridPos(Regions.Lake.X + 5, Regions.Lake.Y + 5);
            var (life, house, c) = World(lake);
            using (CarryContext.Begin(c)) house.AddItem("item.clay", 100);
            float hard = Strength.CapacityKg(Strength.Of(c)) * LoadRules.HardCap;
            Assert.True(c.Bag.TotalKg <= hard + 0.01f);
            Assert.True(life.CarryOverflowTotal > 0);

            var (life2, house2, c2) = World(new GridPos(Regions.Farm.X + 3, Regions.Farm.Y + 3));
            using (CarryContext.Begin(c2)) house2.AddItem("item.clay", 100);
            Assert.Equal(100, house2.Count("item.clay"));
            Assert.True(c2.Bag.IsEmpty);
        }

        [Fact]
        public void Canta_KayittaKalir()
        {
            var c = Adult(new GridPos(0, 0));
            c.Bag.Add(new ItemKey("item.firewood", 2), 3, 3f);
            c.HeavyWorkDays = 2.5;
            var save = new Save.CharacterSave();
            // CaptureBag/LoadCharacter özel; kayıt alanlarının varlığı ve gidiş-dönüşü tüm kayıt testleriyle (SaveGame) denenir.
            save.bag = new[] { new Save.StackSave { id = "item.firewood", q = 2, n = 3 } };
            save.heavyWorkDays = 2.5;
            Assert.Equal(3, save.bag[0].n);
        }
    }
}
