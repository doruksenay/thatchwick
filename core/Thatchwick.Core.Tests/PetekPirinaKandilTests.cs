using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Petekli bal, pirina (yakıt ve yem), zeytinyağı kandili, yadigâr zeytin ağacı (belge 32-7, 8.2).
    public class PetekPirinaKandilTests
    {
        private sealed class Ctx { public Content.ContentDatabase Db; public WorldState World; public Household House; public LifeSystems Life; public Character C; }

        private static Ctx Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var w = new WorldState(new GridMap(12, 12));
            var house = new Household(100);
            var pop = new Population();
            var c = new Character(1, "Ayşe", new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(60);
            pop.Add(c);
            var life = new LifeSystems(db, w, house, pop, new GameClock());
            return new Ctx { Db = db, World = w, House = house, Life = life, C = c };
        }

        [Fact]
        public void Petek_KovandakiIkiBaldanKesilir_YoksaKesilmez()
        {
            var x = Make();
            var hive = x.World.PlaceItem(x.Db.Items.Get("item.beehive"), new GridPos(3, 3));
            var cut = hive.Def.Interactions.First(i => i.Id == LifeSystems.CombInteraction);
            Thatchwick.Core.Animals.AnimalSystem.PutStock(hive, "item.honey", 1);
            x.Life.OnInteractionAppliedOn(x.C, hive, cut);
            Assert.Equal(0, x.House.Count(Honeycomb.CombId));
            Assert.Equal(1, hive.StockTotal);
            Thatchwick.Core.Animals.AnimalSystem.PutStock(hive, "item.honey", 2);
            x.Life.OnInteractionAppliedOn(x.C, hive, cut);
            Assert.Equal(1, x.House.Count(Honeycomb.CombId));
            Assert.Equal(1, hive.StockTotal);
            Assert.Contains(x.Db.Items.Get("item.cooking_pot").Interactions, i => i.Id == "eat_honeycomb");
        }

        [Fact]
        public void Pirina_OdunYoksaYanar_YarimSure_YemOlur()
        {
            var x = Make();
            var heat = new HeatSystem(x.World, new GameClock().Config);
            var def = x.Db.Items.All.First(d => d.HeatOutput > 0f && d.Rooms.Contains("living"));
            var a = x.World.PlaceItem(def, new GridPos(2, 5));
            var b = x.World.PlaceItem(def, new GridPos(8, 5));
            Assert.False(heat.TryStoke(a.Id, x.House));
            x.House.AddItem(HeatSystem.PomaceId, 1);
            Assert.True(heat.TryStoke(a.Id, x.House));
            x.House.AddItem("item.firewood", 1);
            Assert.True(heat.TryStoke(b.Id, x.House));
            Assert.Equal(a.FuelDays * 2, b.FuelDays, 3);
            Assert.Contains(x.Db.Items.Get("item.barn").Interactions, i => i.Id == "feed_pomace" && i.AddFeed > 0);
        }

        [Fact]
        public void Kandil_YagliykenIsikVerir_YagBitinceSoner()
        {
            var x = Make();
            var table = x.World.PlaceItem(x.Db.Items.Get("item.table_wood"), new GridPos(2, 2));
            var lamp = x.World.PlaceOnSurface(x.Db.Items.Get("item.olive_lamp"), table.Id, "a");
            Assert.False(lamp.GivesLight);
            var fill = lamp.Def.Interactions.First(i => i.Id == LifeSystems.LampFillInteraction);
            Assert.Equal("item.olive_oil", fill.ConsumeItem);
            x.Life.OnInteractionAppliedOn(x.C, lamp, fill);
            Assert.True(lamp.GivesLight);
            Assert.Equal(LifeSystems.LampOilDays, lamp.FuelDays, 3);
            x.Life.OnInteractionAppliedOn(x.C, lamp, fill);
            x.Life.OnInteractionAppliedOn(x.C, lamp, fill);
            Assert.Equal(LifeSystems.LampMaxDays, lamp.FuelDays, 3);
            Assert.True(x.Db.Items.Get("item.clay_lamp").HasTag("light"));
            Assert.True(x.World.PlaceOnSurface(x.Db.Items.Get("item.clay_lamp"), table.Id, "b").GivesLight);   // eski kandiller yağ istemez
        }

        [Fact]
        public void Yadigar_KirkGunSonra_HasatArtar_KayitGeriGelir()
        {
            var x = Make();
            var olives = new OliveHeritage();
            var tree = x.World.PlaceItem(x.Db.Items.Get(OliveHeritage.TreeId), new GridPos(4, 4));
            olives.OnNewDay(10, x.World, "Ayşe");
            Assert.Equal("Ayşe", olives.PlanterOf(tree.Id));
            Assert.Empty(olives.OnNewDay(49, x.World, "Mehmet"));
            Assert.Equal(0, olives.BonusOlives(tree.Id, 49));
            var news = olives.OnNewDay(50, x.World, "Mehmet");
            Assert.Single(news);
            Assert.Contains("Ayşe", news[0]);
            Assert.True(olives.IsHeirloom(tree.Id));
            Assert.Equal(1, olives.BonusOlives(tree.Id, 50));
            Assert.Equal(OliveHeritage.MaxBonus, olives.BonusOlives(tree.Id, 500));
            var back = new OliveHeritage(); back.Import(olives.Export());
            Assert.True(back.IsHeirloom(tree.Id));
            Assert.Equal("Ayşe", back.PlanterOf(tree.Id));
            x.World.RemoveItem(tree.Id);
            back.OnNewDay(60, x.World, "Mehmet");
            Assert.Equal(0, back.Count);
        }
    }
}
