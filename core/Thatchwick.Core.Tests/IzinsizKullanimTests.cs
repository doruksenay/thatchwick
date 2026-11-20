using System.Collections.Generic;
using System.IO;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Başkasının evindeki eşyayı kullanmak (kullanıcı 20.11.2026): yatağında dinlenilir; ev sahibi gelince söylenir ya da kaldırır.
    public class IzinsizKullanimTests
    {
        private sealed class Kit
        {
            public WorldState World;
            public TrespassService T;
            public Character User, Owner;
            public Dictionary<(int, int), int> Aff = new Dictionary<(int, int), int>();
            public TownBuilding House, Shop;
        }

        private static Kit Make()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var map = new GridMap(Regions.WorldWidth, Regions.WorldHeight);
            var k = new Kit { World = new WorldState(map), House = Regions.TownBuildingById("home:1"), Shop = Regions.TownBuildingById("general") };
            k.User = new Character(1, "Wat", new GridPos(k.House.X0 + 1, k.House.Y0 + 1)) { MoodEnabled = true };
            k.User.SetAge(30);
            k.Owner = new Character(2, "Edith", new GridPos(5, 5)) { IsVillager = true };
            k.Owner.SetAge(40);
            k.T = new TrespassService(map)
            {
                HomeOf = c => c.Id == 2 ? "home:1" : null,
                Affection = (a, b) => k.Aff.TryGetValue((a, b), out var v) ? v : 0,
                AddAffection = (a, b, d) => k.Aff[(a, b)] = (k.Aff.TryGetValue((a, b), out var v) ? v : 0) + d,
            };
            return k;
        }

        private static ItemInstance Bed(Kit k) =>
            k.World.PlaceItem(ContentFilesTests.LoadDb(out _).Items.Get("item.bed_straw"), new GridPos(k.House.X0 + 1, k.House.Y0 + 1));

        [Fact]
        public void EvSahibiYokken_DinlenilirTepkiYok()
        {
            var k = Make();
            var bed = Bed(k);
            var sleep = bed.Def.Interactions.Find(i => i.Id == "sleep");
            Assert.Null(k.T.Check(k.User, bed, sleep, new[] { k.Owner }));
        }

        [Fact]
        public void YabanciGelince_Kaldirir_RuhHaliVeYakinlikDuser()
        {
            var k = Make();
            var bed = Bed(k);
            var sleep = bed.Def.Interactions.Find(i => i.Id == "sleep");
            k.Owner.Position = new GridPos(k.House.X0, k.House.Y0);
            var r = k.T.Check(k.User, bed, sleep, new[] { k.Owner });
            Assert.NotNull(r);
            Assert.True(r.KickedOut);
            Assert.True(r.Bed);
            Assert.Equal("core.trespass.kick_bed", r.Line.Key);
            Assert.True(k.User.Mood.Has(TrespassService.KickMoodId));
            Assert.Equal(TrespassService.KickAffection, k.Aff[(1, 2)]);
            Assert.Null(k.T.Check(k.User, bed, sleep, new[] { k.Owner }));   // aynı kullanımda bir kez
            k.T.Done(k.User, bed.Id);
            Assert.NotNull(k.T.Check(k.User, bed, sleep, new[] { k.Owner }));
        }

        [Fact]
        public void Dost_Soylenir_IzinVerir()
        {
            var k = Make();
            var bed = Bed(k);
            var sleep = bed.Def.Interactions.Find(i => i.Id == "sleep");
            k.Aff[(1, 2)] = 70;
            k.Owner.Position = new GridPos(k.House.X0 + 2, k.House.Y0 + 1);
            var r = k.T.Check(k.User, bed, sleep, new[] { k.Owner });
            Assert.NotNull(r);
            Assert.False(r.KickedOut);
            Assert.Equal("core.trespass.scold_bed", r.Line.Key);
            Assert.True(k.User.Mood.Has(TrespassService.ScoldMoodId));
            Assert.Equal(68, k.Aff[(1, 2)]);
        }

        [Fact]
        public void DukkaninOrtakEsyasi_Serbest_YatagiOzel()
        {
            var k = Make();
            var db = ContentFilesTests.LoadDb(out _);
            var table = k.World.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(k.Shop.X0 + 2, k.Shop.Y0 + 2));
            Assert.False(TrespassService.IsPrivate(k.Shop, table, null));
            var bed = k.World.PlaceItem(db.Items.Get("item.bed_straw"), new GridPos(k.Shop.X0 + 4, k.Shop.Y0 + 3));
            Assert.True(TrespassService.IsPrivate(k.Shop, bed, bed.Def.Interactions.Find(i => i.Id == "sleep")));
            Assert.Null(k.T.BuildingAt(new GridPos(5, 5)));
            Assert.Equal("home:1", k.T.BuildingAt(new GridPos(k.House.X0, k.House.Y0)).Owner);
        }

        [Fact]
        public void Sozler_IkiDildeVar()
        {
            string dir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            foreach (var f in new[] { "ui.tr.json", "ui.en.json" })
            {
                var t = LocTable.Parse(File.ReadAllText(Path.Combine(dir, f)), new List<string>());
                var loc = new Localizer(f.StartsWith("ui.tr") ? "tr" : "en", t);
                foreach (var key in new[] { "core.trespass.scold_bed", "core.trespass.scold", "core.trespass.kick_bed", "core.trespass.kick" })
                    Assert.True(loc.Has(key), f + " " + key);
            }
        }
    }
}
