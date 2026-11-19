using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Kahve: içerik zinciri, ılık sera, alışkanlık, Mahmur, baş ağrısı, fal (belge 32).
    public class KahveTests
    {
        private static Character Adult(int id)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(40);
            return c;
        }

        private static void Ensure()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
        }

        [Fact]
        public void Icerik_ZincirTam_CezveVeEtkiler()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var bush = db.Items.Get("item.bush_coffee");
            Assert.True(bush.WarmOnly);
            Assert.True(bush.IsFruitTree);
            Assert.Equal("item.bush_coffee", db.Items.Get("item.sapling_coffee").PlantsAs);
            Assert.True(db.Items.Get("item.coffee_ground").HasTag(CoffeeHabits.Tag));
            Assert.Contains(db.Items.Get("item.mortar_pestle").Interactions, i => i.Id == "craft:grind_coffee");
            Assert.Contains(db.Items.Get("item.drying_rack").Interactions, i => i.Id == "craft:dry_coffee");
            Assert.Contains(db.Items.Get("item.forge").Interactions, i => i.Id == "craft:cezve");
            var cezve = db.Items.Get("item.cezve");
            Assert.True(cezve.Interactions.First(i => i.Id == "cezve_brew").AdultOnly);
            Assert.Contains(cezve.Interactions, i => i.Id == LifeSystems.FortuneInteraction);
            Assert.True(db.Items.Get("item.cooking_pot").Interactions.First(i => i.Id == "drink_coffee").AdultOnly);
        }

        [Fact]
        public void Aliskanlik_DortGunSonraBaslar_MahmurVeBasAgrisi()
        {
            Ensure();
            var h = new CoffeeHabits();
            var c = Adult(1);
            for (int d = 0; d < 3; d++) h.OnCoffee(c, d + 0.3);
            Assert.False(h.HasHabit(c));
            h.OnCoffee(c, 3.3);
            Assert.True(h.HasHabit(c));
            Assert.Equal(4, h.Streak(c));

            // Gün 4: sabah kahve yok → Mahmur; içince kalkar.
            h.Tick(4.31, new[] { c });
            Assert.True(c.Mood.Has(CoffeeHabits.MahmurId));
            h.OnCoffee(c, 4.4);
            Assert.False(c.Mood.Has(CoffeeHabits.MahmurId));
            h.Tick(4.75, new[] { c });
            Assert.False(c.Mood.Has(CoffeeHabits.HeadacheId));   // bugün içti

            // Gün 5: hiç içmedi → akşamüstü baş ağrısı.
            h.Tick(5.31, new[] { c });
            h.Tick(5.72, new[] { c });
            Assert.True(c.Mood.Has(CoffeeHabits.HeadacheId));
        }

        [Fact]
        public void Aliskanlik_UcGunIcmezseGecer_SeriKopar_KayitGeriGelir()
        {
            Ensure();
            var h = new CoffeeHabits();
            var c = Adult(1);
            for (int d = 0; d < 4; d++) h.OnCoffee(c, d + 0.3);
            var back = new CoffeeHabits(); back.Import(h.Export());
            Assert.True(back.HasHabit(c));
            Assert.Equal(4, back.Streak(c));
            back.OnNewDay(6, id => c);
            Assert.True(back.HasHabit(c));
            back.OnNewDay(7, id => c);
            Assert.False(back.HasHabit(c));
            // Ara verince seri baştan başlar.
            var s = new CoffeeHabits();
            s.OnCoffee(c, 0.3); s.OnCoffee(c, 1.3); s.OnCoffee(c, 3.3);
            Assert.Equal(1, s.Streak(c));
        }

        [Fact]
        public void Cocuk_KahveSayilmaz_AksamKahvesiUykuKacirir()
        {
            Ensure();
            var h = new CoffeeHabits();
            var kid = new Character(2, "Çocuk", new GridPos(0, 0), true);
            kid.SetAge(8);
            Assert.False(CoffeeHabits.CanDrink(kid));
            for (int d = 0; d < 5; d++) h.OnCoffee(kid, d + 0.3);
            Assert.False(h.HasHabit(kid));
            var c = Adult(3);
            h.OnCoffee(c, 2.8);
            Assert.True(c.Mood.Has(CoffeeHabits.LateId));
            Assert.False(string.IsNullOrEmpty(h.Fortune()));
        }
    }
}
