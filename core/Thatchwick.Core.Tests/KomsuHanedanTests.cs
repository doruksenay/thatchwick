using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 55 kalanı (18.11.2026): komşu NPC hanedanları. Köydeki ailelerin kendi başları, soy ağaçları ve genetiği.
    public class KomsuHanedanTests
    {
        private sealed class Ctx
        {
            public ContentDatabase Db; public Population Pop; public LifeSystems Life; public GameClock Clock;
            public NeighborHouseService Houses => Life.Houses;
            public Character Named(string defId) => Life.Villagers.CharacterOf(defId);
        }

        private static Ctx Village()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(20, 20);
            var clock = new GameClock();
            var pop = new Population();
            var life = new LifeSystems(db, new WorldState(map), new Household(), pop, clock);
            life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            life.Houses.EnsureHouses();
            return new Ctx { Db = db, Pop = pop, Life = life, Clock = clock };
        }

        private static Character Villager(Ctx x, string name, Sex sex, double age, int? mother = null, int? father = null)
        {
            var c = new Character(x.Pop.NextId(), name, new GridPos(2, 2))
            {
                Sex = sex, IsVillager = true, CanStarve = false, BirthMotherId = mother, BirthFatherId = father
            };
            c.SetAge(age);
            x.Pop.Add(c);
            return c;
        }

        private static void Kill(Character c)
        {
            c.LifespanDays = c.AgeDays + 0.001;
            c.Tick(0.01);
            Assert.True(c.IsDead);
        }

        [Fact]
        public void AdliKoyluler_KendiDukkanHanelerindeBaslar()
        {
            var x = Village();
            var orhan = x.Named("villager.orhan");
            var h = x.Houses.HouseOf(orhan);
            Assert.NotNull(h);
            Assert.Equal("Demirci hanesi", h.Name);
            Assert.Equal("villager.orhan", h.DefId);
            Assert.Equal(orhan.Id, h.HeadId);
            // Her adlı köylü kendi hanesinde başlar; sayı içerikle büyüdüğü için sabit değil (6 → 13, şehir haritası).
            Assert.Equal(x.Life.Villagers.States.Count(), x.Houses.Houses.Count(hh => hh.DefId != null));
        }

        [Fact]
        public void Evlilik_GelinDamadinHanesine_DukkanSahibiHanesindeKalir()
        {
            var x = Village();
            var wat = Villager(x, "Wat", Sex.Male, 33);
            var joan = Villager(x, "Joan", Sex.Female, 31);
            x.Houses.EnsureHouses();
            var watHouse = x.Houses.HouseOf(wat);
            x.Houses.Marry(wat, joan);
            Assert.Same(watHouse, x.Houses.HouseOf(joan));
            Assert.Equal(wat.Id, joan.SpouseId);
            Assert.True(x.Houses.Houses.Single(h => h.Name == "Joan hanesi").Extinct);   // boşalan hane kapanır

            // Değirmenci Hatice dükkân hanesinin başı: evlenen erkek onun hanesine girer.
            var hatice = x.Named("villager.hatice");
            var odo = Villager(x, "Odo", Sex.Male, 40);
            x.Houses.EnsureHouses();
            x.Houses.Marry(odo, hatice);
            Assert.Equal("Değirmenci hanesi", x.Houses.HouseOf(odo).Name);
            Assert.Equal(hatice.Id, x.Houses.HouseOf(odo).HeadId);
        }

        [Fact]
        public void Dogum_BebekBabasininHanesine_GenomKalitimiIsler()
        {
            var x = Village();
            var murat = x.Named("villager.murat");
            var ayse = x.Named("villager.ayse");
            x.Houses.Marry(murat, ayse);
            Assert.True(ayse.TryStartPregnancy(murat.Id));
            Character baby = null;
            x.Pop.BabyBorn += (b, m, f) => baby = b;
            for (int i = 0; i < 12 && baby == null; i++) ayse.Tick(1.0);
            Assert.NotNull(baby);
            Assert.True(baby.IsVillager);
            Assert.Same(x.Houses.HouseOf(murat), x.Houses.HouseOf(baby));
            Assert.Equal(murat.Id, baby.BirthFatherId);
        }

        [Fact]
        public void Varis_BasOlunceYetiskinCocukBasaGecer_DukkaniDevralir()
        {
            var x = Village();
            var orhan = x.Named("villager.orhan");
            var son = Villager(x, "Hasan", Sex.Male, 34, father: orhan.Id);
            var girl = Villager(x, "Elif", Sex.Female, 14, father: orhan.Id);
            x.Houses.EnsureHouses();
            var h = x.Houses.HouseOf(orhan);
            Assert.Same(h, x.Houses.HouseOf(son));
            Assert.Same(h, x.Houses.HouseOf(girl));

            Character inherited = null;
            x.Life.Villagers.RoleInherited += (heir, def) => inherited = heir;
            Kill(orhan);
            x.Houses.OnNewDay();
            Assert.Equal(son.Id, h.HeadId);
            Assert.Equal(2, h.HeadCount);
            x.Life.Villagers.ReplaceDeparted(inheritOnly: true);
            Assert.Same(son, x.Named("villager.orhan"));      // demircilik soyda kaldı
            Assert.Same(son, inherited);
            Assert.True(x.Life.Villagers.HasRole(VillagerService.RoleBlacksmith));
        }

        [Fact]
        public void Varis_KucukseDukkanaDisaridanUstaGelir_HaneSurer()
        {
            var x = Village();
            var orhan = x.Named("villager.orhan");
            var boy = Villager(x, "Ali", Sex.Male, 15, father: orhan.Id);
            x.Houses.EnsureHouses();
            var h = x.Houses.HouseOf(orhan);
            Kill(orhan);
            x.Houses.OnNewDay();
            Assert.Equal(boy.Id, h.HeadId);                    // çocuk baş, ama dükkânı süremez
            Assert.Empty(x.Life.Villagers.ReplaceDeparted(inheritOnly: true));
            var arrived = x.Life.Villagers.ReplaceDeparted();  // mevsim başı: dışarıdan usta
            Assert.Single(arrived);
            x.Houses.EnsureHouses();
            var smithHouse = x.Houses.HouseOfDef("villager.orhan");
            Assert.NotSame(h, smithHouse);
            Assert.Equal(arrived[0].Id, smithHouse.HeadId);
            Assert.Null(h.DefId);
            Assert.False(h.Extinct);
            Assert.Contains("(", smithHouse.Name);             // aynı adlı ikinci hane: kurucu adıyla ayrılır
        }

        [Fact]
        public void SoyuTukenir_YasayanUyeKalmazsa()
        {
            var x = Village();
            var loner = Villager(x, "Godric", Sex.Male, 50);
            x.Houses.EnsureHouses();
            var h = x.Houses.HouseOf(loner);
            NeighborHouse ended = null;
            x.Houses.HouseEnded += hh => ended = hh;
            Kill(loner);
            x.Houses.OnNewDay();
            Assert.True(h.Extinct);
            Assert.Same(h, ended);
        }

        [Fact]
        public void KoyEvliligi_BekarlarEvlenir_AkrabaVeKurYapilanEvlenmez()
        {
            var x = Village();
            var murat = x.Named("villager.murat");
            x.Life.Villagers.StateOf(murat).Romance = NeighborHouseService.CourtedRomance;   // oyuncu kur yapıyor
            var mother = Villager(x, "Anna", Sex.Female, 70);
            var bro = Villager(x, "Piers", Sex.Male, 35, mother: mother.Id);
            var sis = Villager(x, "Mabel", Sex.Female, 33, mother: mother.Id);
            x.Houses.EnsureHouses();
            int weddings = 0;
            x.Houses.VillagersMarried += (a, b) =>
            {
                weddings++;
                Assert.False(FamilyService.AreCloseRelatives(a, b));
                Assert.NotEqual(a.Sex, b.Sex);
                Assert.True(AgeStages.CanMarry(a.Stage) && AgeStages.CanMarry(b.Stage));
            };
            for (int d = 0; d < 300; d++) x.Houses.TryVillageMarriage();
            Assert.True(weddings > 0);
            Assert.False(murat.SpouseId.HasValue);
            Assert.False(bro.SpouseId == sis.Id);
        }

        [Fact]
        public void SoyAgaci_KusaklariDizer()
        {
            var x = Village();
            var orhan = x.Named("villager.orhan");
            var wife = Villager(x, "Gül", Sex.Female, 50);
            x.Houses.EnsureHouses();
            x.Houses.Marry(orhan, wife);
            var son = Villager(x, "Hasan", Sex.Male, 34, mother: wife.Id, father: orhan.Id);
            var grandson = Villager(x, "Kemal", Sex.Male, 3, father: son.Id);
            x.Houses.EnsureHouses();
            var h = x.Houses.HouseOf(orhan);
            var tree = x.Houses.TreeOf(h);
            Assert.Equal(1, tree.Single(n => n.Person == orhan).Depth);
            Assert.Equal(1, tree.Single(n => n.Person == wife).Depth);      // eş, eşinin kuşağında
            Assert.Equal(2, tree.Single(n => n.Person == son).Depth);
            Assert.Equal(3, tree.Single(n => n.Person == grandson).Depth);
            Assert.True(tree.Single(n => n.Person == orhan).IsHead);
            Assert.Equal(3, x.Houses.Generations(h));
        }

        [Fact]
        public void KomsuHaneyeEvlendirme_UyeEsininHanesineGirer()
        {
            var x = Village();
            var murat = x.Named("villager.murat");
            var daughter = new Character(x.Pop.NextId(), "Edith", new GridPos(1, 1)) { Sex = Sex.Female };
            daughter.SetAge(32);
            x.Pop.Add(daughter);
            var family = new FamilyService(x.Pop, new Household(), x.Db);
            family.SetAffection(daughter.Id, murat.Id, Legacy.MarryOutAffection);
            Assert.True(x.Life.Legacy.MarryOut(daughter, murat, null, family, 1.0, out var msg), msg);
            Assert.Same(x.Houses.HouseOf(murat), x.Houses.HouseOf(daughter));
        }

        [Fact]
        public void Kayit_HanelerBaslarVeUyelikGeriGelir()
        {
            var x = Village();
            var orhan = x.Named("villager.orhan");
            var son = Villager(x, "Hasan", Sex.Male, 34, father: orhan.Id);
            var wat = Villager(x, "Wat", Sex.Male, 33);
            var joan = Villager(x, "Joan", Sex.Female, 31);
            x.Houses.EnsureHouses();
            x.Houses.Marry(wat, joan);
            Kill(orhan);
            x.Houses.OnNewDay();

            var saved = x.Life.Export();
            Assert.NotNull(saved.houses);
            var y = new LifeSystems(x.Db, new WorldState(new GridMap(20, 20)), new Household(), x.Pop, new GameClock());
            y.Villagers.Restore(saved.villagers);
            y.Houses.Restore(saved.houses);
            var h = y.Houses.HouseOf(son);
            Assert.Equal("Demirci hanesi", h.Name);
            Assert.Equal(son.Id, h.HeadId);
            Assert.Equal(2, h.HeadCount);
            Assert.Same(y.Houses.HouseOf(wat), y.Houses.HouseOf(joan));
            Assert.Equal(x.Houses.Houses.Count, y.Houses.Houses.Count);
        }

        [Fact]
        public void EskiKayit_HanelerKoylulerdenKurulur()
        {
            var x = Village();
            var wat = Villager(x, "Wat", Sex.Male, 33);
            var joan = Villager(x, "Joan", Sex.Female, 31);
            wat.SpouseId = joan.Id; joan.SpouseId = wat.Id;
            var kid = Villager(x, "Tom", Sex.Male, 5, mother: joan.Id, father: wat.Id);
            var y = new LifeSystems(x.Db, new WorldState(new GridMap(20, 20)), new Household(), x.Pop, new GameClock());
            y.Villagers.Restore(x.Life.Villagers.Export().ToArray());
            y.Houses.Restore(null);                              // eski kayıtta alan yok
            Assert.Equal("Wat hanesi", y.Houses.HouseOf(wat).Name);
            Assert.Same(y.Houses.HouseOf(wat), y.Houses.HouseOf(joan));
            Assert.Same(y.Houses.HouseOf(wat), y.Houses.HouseOf(kid));
            Assert.Equal("Demirci hanesi", y.Houses.HouseOf(x.Named("villager.orhan")).Name);
        }

        [Fact]
        public void NufusTavani_KoyDoluncaYeniGebelikOlmaz()
        {
            var x = Village();
            for (int i = 0; i < Legacy.NeighborVillageCap; i++) Villager(x, "K" + i, i % 2 == 0 ? Sex.Male : Sex.Female, 35);
            var w = Villager(x, "Mary", Sex.Female, 33);
            var m = Villager(x, "John", Sex.Male, 35);
            w.SpouseId = m.Id; m.SpouseId = w.Id;
            for (int d = 0; d < 200; d++) Assert.Empty(x.Life.Legacy.NeighborFamiliesDaily());
        }
    }
}
