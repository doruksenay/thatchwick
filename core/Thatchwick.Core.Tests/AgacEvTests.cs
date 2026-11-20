using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ulu ağaçlar ve ağaç evler (Project belgesi 52): kuruluş, birlikte inşa, köşk, aşınma, hazine, ad kazıma,
    // Çınar Aşevi (ücretli yemek, hayır sofrası, bağış), Bereket Sofrası ve kayıt.
    public class AgacEvTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public Population Pop;
            public GameClock Clock;
            public TreehouseSystem Trees;
            public readonly List<string> Notices = new List<string>();
            public readonly List<(int n, string why)> Rep = new List<(int, string)>();
            public readonly List<(string item, string label, int who)> Keepsakes = new List<(string, string, int)>();
            public ItemDef D(string id) => Db.Items.Get(id);
            public InteractionDef I(ItemInstance tree, string id) => tree.Def.Interactions.First(i => i.Id == id);
        }

        private static Kit Make(int akce = 500)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(40, 40)), House = new Household(akce), Pop = new Population(), Clock = new GameClock() };
            k.Trees = new TreehouseSystem(k.World, db, k.House, k.Pop, k.Clock, seed: 3)
            {
                Notify = (t, _) => k.Notices.Add(t),
                AddReputation = (n, why) => k.Rep.Add((n, why)),
                GrantKeepsake = (id, label, who) => k.Keepsakes.Add((id, label, who)),
                HouseholdName = () => "Aldric hanesi"
            };
            return k;
        }

        private static Character Person(Kit k, int id, double age, bool villager = false)
        {
            var c = new Character(id, "K" + id, new GridPos(5, 5), !villager) { CanStarve = false, IsVillager = villager, MoodEnabled = true };
            c.SetAge(age);
            k.Pop.Add(c);
            return c;
        }

        private const double ChildAge = 15, TeenAge = 26, AdultAge = 45;

        private static ItemInstance Tree(Kit k, string id = GreatTrees.Oak, int x = 10, int y = 10)
        {
            var t = k.World.PlaceItem(k.D(id), new GridPos(x, y));
            k.Trees.Attach();
            return t;
        }

        private static void Do(Kit k, Character c, ItemInstance tree, string id)
        {
            var i = k.I(tree, id);
            Assert.Null(k.Trees.CanStart(c, tree, i));
            k.Trees.OnApplied(c, tree, i);
        }

        [Fact]
        public void Icerik_UluAgaclarIskeletlerVeTarifler()
        {
            var k = Make();
            foreach (var id in new[] { GreatTrees.Oak, GreatTrees.Linden, GreatTrees.Walnut, GreatTrees.Chestnut, GreatTrees.Plane })
            {
                var d = k.D(id);
                Assert.True(GreatTrees.IsGreatTree(d), id);
                Assert.False(d.IsPlant, id + " ulu ağaç büyüyen bitki değildir (kesilemez)");
                Assert.DoesNotContain(d.Interactions, i => i.RemovesItem);
                Assert.Contains(d.Interactions, i => i.TreeAction == TreeActions.Gather && i.Produce.Count > 0);
            }
            Assert.True(GreatTrees.IsInn(k.D(GreatTrees.Plane)));
            Assert.Equal(5, GreatTrees.KioskSize(k.D(GreatTrees.Oak)));
            Assert.Equal(4, GreatTrees.KioskSize(k.D(GreatTrees.Linden)));
            Assert.DoesNotContain(k.D(GreatTrees.Plane).Interactions, i => i.TreeAction == TreeActions.Found);
            var bench = k.D("item.workbench");
            var levels = new Dictionary<string, int> { ["nest"] = 2, ["lookout"] = 4, ["fort"] = 5, ["hut"] = 6, ["kiosk"] = 8 };
            foreach (var kv in levels)
            {
                var craft = bench.Interactions.First(i => i.Id == "craft:th_kit_" + kv.Key);
                Assert.Equal(kv.Value, craft.MinSkillLevel);
                Assert.Equal(SkillType.Carpentry, craft.Skill);
                Assert.Contains(k.D(GreatTrees.Oak).Interactions, i => i.Id == "found_" + kv.Key && i.AllConsumed().Any(c => c.ItemId == "item.th_kit_" + kv.Key));
            }
            Assert.Contains(k.D("item.cooking_pot").Interactions, i => i.AllConsumed().Any(c => c.ItemId == "item.walnut"));
        }

        [Fact]
        public void Yerlesim_ArsadaMeseKonur_TekrarKonmaz_BitkiKalkar()
        {
            var k = Make();
            var world = new WorldState(new GridMap(96, 72));
            var site = GreatTrees.Sites.First(s => s.RegionId == "farm");
            world.PlaceItem(k.D("item.tree_oak"), new GridPos(site.X, site.Y));
            var placed = GreatTrees.Ensure(world, k.Db);
            Assert.Single(placed);
            Assert.Equal(GreatTrees.Oak, placed[0].Def.Id);
            Assert.DoesNotContain(world.Items, it => it.Def.Id == "item.tree_oak");   // yerindeki genç meşe kalktı
            Assert.Empty(GreatTrees.Ensure(world, k.Db));                              // ikinci kez konmaz
            // Arsanın dışı sahipsizse çiftlik ağacı arsanın içinde aranır.
            var w2 = new WorldState(new GridMap(96, 72));
            w2.Map.RestrictOwnership(4, 4, 67, 67);
            var p2 = GreatTrees.Ensure(w2, k.Db).Single();
            Assert.True(w2.Map.IsOwned(p2.Position));
        }

        [Fact]
        public void Kurulus_YetiskinBaslatir_CocukYardimEder_BirlikteYaptik()
        {
            var k = Make();
            var tree = Tree(k);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Assert.Equal("Ağaç evi bir yetişkin kurar; çocuklar sonra yardıma gelir.", k.Trees.CanStart(kid, tree, k.I(tree, "found_nest")));
            Assert.True(k.Trees.Shows(tree, k.I(tree, "found_nest")));
            Assert.False(k.Trees.Shows(tree, k.I(tree, "play")));
            Do(k, dad, tree, "found_nest");
            var s = k.Trees.SiteOf(tree);
            Assert.Equal(TreehouseKind.Nest, s.Kind);
            Assert.Equal(1.0, s.Progress, 3);
            Assert.False(s.IsBuilt);
            Assert.False(k.Trees.Shows(tree, k.I(tree, "found_nest")));
            Assert.True(k.Trees.Shows(tree, k.I(tree, "build")));
            Do(k, kid, tree, "build");                 // çocuk yarım iş
            Assert.Equal(1.5, s.Progress, 3);
            Do(k, kid, tree, "build");
            Assert.True(s.IsBuilt);
            Assert.True(dad.Mood.Has(TreehouseSystem.BuiltTogetherMood));
            Assert.True(kid.Mood.Has(TreehouseSystem.BuiltTogetherMood));
            Assert.Contains(k.Notices, n => n.Contains("Dal Yuvası bitti"));
            // Oyun: çocuk oynar ("Kendi kalemiz"), yetişkin oynamaz.
            Assert.Equal("Burası çocukların yeri.", k.Trees.CanStart(dad, tree, k.I(tree, "play")));
            Do(k, kid, tree, "play");
            Assert.True(kid.Mood.Has(TreehouseSystem.OwnFortMood));
            Assert.True(TreehouseSystem.Hides(k.I(tree, "play")));
            Assert.False(TreehouseSystem.Hides(k.I(tree, "build")));
            // Türe özgü işler yalnızca o türde.
            Assert.False(k.Trees.Shows(tree, k.I(tree, "siege")));
            Assert.Equal("Bu bir gözcü kulesi değil.", k.Trees.CanStart(kid, tree, k.I(tree, "lookout")));
        }

        [Fact]
        public void Kale_KusatmaIcinArkadasGerekir_KoyCocuguUgrayinca_Oynanir()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Walnut);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_fort"); Do(k, dad, tree, "build"); Do(k, dad, tree, "build");
            Assert.True(k.Trees.SiteOf(tree).IsBuilt);
            Assert.Equal("Kuşatma için en az bir oyun arkadaşı gerekir.", k.Trees.CanStart(kid, tree, k.I(tree, "siege")));
            var villageKid = Person(k, 3, ChildAge, villager: true);
            k.Trees.SiteOf(tree).Visitors.Add(villageKid.Id);
            var affection = new List<(int, int, int)>();
            k.Trees.AddAffection = (a, b, d) => affection.Add((a, b, d));
            Do(k, kid, tree, "siege");
            Assert.Contains(affection, x => x.Item1 == kid.Id && x.Item2 == villageKid.Id);
            Assert.Contains(k.Notices, n => n.StartsWith("Kale kuşatması:"));
        }

        [Fact]
        public void Kosk_BuyukAgactaEkMalzeme_KatKabukMerdivenVeEsyaKoyma()
        {
            var k = Make();
            var tree = Tree(k);   // ulu meşe: 5x5 köşk
            var dad = Person(k, 1, AdultAge);
            Assert.StartsWith("Büyük ağaçta köşk için ayrıca", k.Trees.CanStart(dad, tree, k.I(tree, "found_kiosk")));
            k.House.AddItem("item.plank", 8); k.House.AddItem("item.nails", 4);
            Do(k, dad, tree, "found_kiosk");
            Assert.Equal(0, k.House.Count("item.plank"));
            for (int n = 0; n < 4; n++) Do(k, dad, tree, "build");
            var s = k.Trees.SiteOf(tree);
            Assert.True(s.IsBuilt);
            Assert.Equal(5, s.KioskN);
            var map = k.World.Map;
            int x0 = tree.Position.X - 1, y0 = tree.Position.Y - 1;
            for (int y = y0 - 1; y < y0 + 5; y++)
                for (int x = x0; x < x0 + 5; x++)
                    Assert.True(map.HasTile(new GridPos(x, y, 1)), $"köşk karosu {x},{y}");
            Assert.Equal(EdgeKind.Door, map.GetEdge(new Edge(tree.Position.X, y0, EdgeAxis.Horizontal, 1)));
            Assert.Equal(EdgeKind.Railing, map.GetEdge(new Edge(x0, y0 - 1, EdgeAxis.Horizontal, 1)));
            var room = map.Rooms.RoomAt(new GridPos(x0 + 2, y0 + 2, 1));
            Assert.NotNull(room);
            Assert.True(room.IsIndoor);
            // İp merdiveni: yerden balkona; merdiven eşyası yeniden kurulunca da kalır.
            var ladder = GreatTrees.LadderTile(tree);
            Assert.True(map.CanTraverse(ladder, new GridPos(ladder.X, ladder.Y, 1)));
            k.World.RebuildLinks();
            Assert.True(map.CanTraverse(ladder, new GridPos(ladder.X, ladder.Y, 1)));
            Assert.False(k.World.CanPlaceFloor(k.D("item.stool_wood"), ladder, 0).Ok);   // merdiven başına eşya konmaz
            // Kabuk sabit, içi hanenin: arsa dışında da eşya konur.
            map.RestrictOwnership(0, 0, 2, 2);
            var build = new BuildService(k.World, k.House, k.Db) { ExtraOwned = k.Trees.ExtraOwned, Shell = k.Trees.IsShell };
            Assert.True(k.Trees.IsShell(new GridPos(x0, y0, 1)));
            Assert.False(k.Trees.IsShell(new GridPos(x0, y0, 0)));
            k.House.AddItem("item.stool_wood", 1);
            var place = build.PlaceFromInventory("item.stool_wood", new GridPos(x0 + 2, y0 + 2, 1), 0, out var stool);
            Assert.True(place.Ok, place.Reason);
            Assert.False(build.CanPlaceFromInventory("item.stool_wood", new GridPos(x0 + 2, y0 + 2, 0), 0).Ok);   // yer kat arsa değil
            map.RestrictOwnership(0, 0, 39, 39);
            var wall = build.SetEdge(new Edge(x0, y0 + 1, EdgeAxis.Vertical, 1), EdgeKind.None);
            Assert.False(wall.Ok);
            Assert.Equal(BuildService.ShellLocked, wall.Reason);
        }

        [Fact]
        public void KoskAsevi_SartlarVeGunlukSatis()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Linden);   // 4x4
            var dad = Person(k, 1, AdultAge);
            Do(k, dad, tree, "found_kiosk");
            for (int n = 0; n < 4; n++) Do(k, dad, tree, "build");
            var s = k.Trees.SiteOf(tree);
            Assert.StartsWith("Aşevi için köşkte 2 masa", k.Trees.CanStart(dad, tree, k.I(tree, "inn_toggle")));
            int x0 = s.KioskX, y0 = s.KioskY;
            k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0, y0 + 3, 1));
            k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0 + 2, y0 + 3, 1));
            for (int n = 0; n < 4; n++) k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(x0 + n, y0 + 2, 1));
            k.World.PlaceItem(k.D("item.shop_counter"), new GridPos(x0, y0 + 1, 1));
            var (tables, seats, counters) = k.Trees.CountFurniture(s);
            Assert.True(tables >= 2 && seats >= 4 && counters >= 1, $"{tables} {seats} {counters}");
            Do(k, dad, tree, "inn_toggle");
            Assert.True(s.InnOpen);
            var cooked = k.Db.Items.All.First(d => d.HasTag("cooked") && d.Price > 0);
            k.House.AddItem(cooked.Id, 10);
            int before = k.House.Akce;
            k.Trees.OnNewDay();
            Assert.True(k.House.Akce > before);
            Assert.True(k.House.Count(cooked.Id) < 10);
        }

        [Fact]
        public void Asinma_HarapOlur_Onarilir_KoskDisindakiCoker()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Chestnut);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_nest"); Do(k, dad, tree, "build");
            var s = k.Trees.SiteOf(tree);
            s.Durability = 0.5;
            k.Trees.OnNewDay();
            Assert.True(s.IsRuined);
            Assert.Equal("Ağaç ev harap; önce onarılmalı.", k.Trees.CanStart(kid, tree, k.I(tree, "play")));
            Assert.True(k.Trees.Shows(tree, k.I(tree, "repair")));
            Do(k, dad, tree, "repair");
            Assert.False(s.IsRuined);
            Assert.Equal(100, s.Durability, 3);
            // Bir yıl harap kalan çöker; ağaç yeniden boş olur, kazınan adlar kalır.
            var teen = Person(k, 3, TeenAge);
            Do(k, teen, tree, "carve");
            s.Durability = 0; s.RuinedSince = 0;
            k.Clock.Restore(k.Clock.Config.DaysPerYear + 1, 1);
            k.Trees.OnNewDay();
            Assert.False(s.HasHouse);
            Assert.Contains("K3", s.CarvedNames);
            Assert.True(k.Trees.Shows(tree, k.I(tree, "found_nest")));
        }

        [Fact]
        public void Hazine_SaklayanBulamaz_YillarSonraTorunBulur_Hatira()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Walnut);
            var grandma = Person(k, 1, AdultAge);
            var mom = Person(k, 2, AdultAge);
            var kid = Person(k, 3, ChildAge);
            mom.BirthMotherId = grandma.Id;
            kid.BirthMotherId = mom.Id;
            Do(k, grandma, tree, "found_hut");
            for (int n = 0; n < 3; n++) Do(k, grandma, tree, "build");
            var toy = k.Db.Items.All.First(d => d.Category == "kids");
            k.House.AddItem(toy.Id, 1);
            var stasher = Person(k, 4, ChildAge);
            Do(k, stasher, tree, "stash");
            Assert.Equal(0, k.House.Count(toy.Id));
            var s = k.Trees.SiteOf(tree);
            s.Stash.OwnerId = grandma.Id; s.Stash.OwnerName = grandma.Name;   // bu hazineyi büyükanne çocukken saklamıştı
            Assert.Equal("Sandık sıkı kapalı; daha zaman var.", k.Trees.CanStart(kid, tree, k.I(tree, "dig")));
            k.Clock.Restore(k.Trees.StashWaitDays + 1, 1);
            Do(k, kid, tree, "dig");
            Assert.Equal(1, k.House.Count(toy.Id));
            Assert.Contains(k.Keepsakes, x => x.item == toy.Id && x.who == kid.Id && x.label.Contains(grandma.Name));
            Assert.Null(s.Stash);
        }

        [Fact]
        public void AdKazima_ErgendenItibaren_KisiBasinaBirKez()
        {
            var k = Make();
            var oak = Tree(k);
            var walnut = Tree(k, GreatTrees.Walnut, 20, 20);
            var kid = Person(k, 1, ChildAge);
            var teen = Person(k, 2, TeenAge);
            Assert.Equal("Adını kazımak için ergen olmalı.", k.Trees.CanStart(kid, oak, k.I(oak, "carve")));
            Do(k, teen, oak, "carve");
            Assert.Equal("Adını zaten bir ulu ağaca kazıdı.", k.Trees.CanStart(teen, walnut, k.I(walnut, "carve")));
            Assert.True(k.Trees.Shows(oak, k.I(oak, "found_nest")));   // ad kazımak ağaç ev kurmaz
        }

        [Fact]
        public void Asevi_UcretliYemek_HayirSofrasi_Bagis()
        {
            var k = Make(akce: 200);
            var plane = Tree(k, GreatTrees.Plane, 15, 15);
            var site = k.Trees.SiteOf(plane);
            Assert.NotNull(site);
            Assert.True(site.IsVillageInn);
            var c = Person(k, 1, AdultAge);
            k.Clock.Restore(0.5, 1);   // öğle
            Assert.True(k.Trees.Shows(plane, k.I(plane, "carve")));
            Assert.True(k.Trees.Shows(plane, k.I(plane, "inn_eat")));
            Assert.Equal("Hayır sofrası parası olmayanlar için.", k.Trees.CanStart(c, plane, k.I(plane, "inn_charity")));
            Do(k, c, plane, "inn_eat");
            Assert.Equal(200 - TreehouseSystem.MealPrice, k.House.Akce);
            Do(k, c, plane, "donate_100");
            Assert.Equal(100, k.Trees.Kitchen.Fund);
            Assert.Contains(k.Rep, r => r.n == 5 && r.why == "aşevine bağış");
            // Haftalık itibar tavanı: aynı hafta 10'u geçmez.
            k.House.AddAkce(500);
            Do(k, c, plane, "donate_100");
            Do(k, c, plane, "donate_100");
            Assert.Equal(CharityKitchen.WeeklyRepCap, k.Rep.Where(r => r.why == "aşevine bağış").Sum(r => r.n));
            // Parası bitti: hayır sofrası günde bir kez; kasa doluyken yahni.
            k.House.TrySpend(k.House.Akce);
            Do(k, c, plane, "inn_charity");
            Assert.True(c.Mood.Has(TreehouseSystem.CharityGoodMood));
            Assert.Equal("Bugün hayır sofrasına oturdu; yarın yine gelebilir.", k.Trees.CanStart(c, plane, k.I(plane, "inn_charity")));
            // Gece kapalı.
            k.Clock.Restore(1.95, 1);
            Assert.Equal("Aşevi şu an kapalı.", k.Trees.CanStart(c, plane, k.I(plane, "inn_charity")));
        }

        [Fact]
        public void BereketSofrasi_MevsimSonuAksami_SofraSahibiItibarAlir()
        {
            var k = Make(akce: 500);
            var plane = Tree(k, GreatTrees.Plane, 15, 15);
            var a = Person(k, 1, AdultAge);
            var b = Person(k, 2, ChildAge);
            k.Clock.Restore(0.5, 1);
            Do(k, a, plane, "donate_100");
            k.Trees.Kitchen.Donate(30, 0, 7, "Demirci hanesi");
            int last = k.Clock.Config.DaysPerSeason - 1;
            k.Clock.Restore(last + 0.5, 1);
            Assert.Null(k.Trees.Tick());                  // akşam olmadan kurulmaz
            k.Clock.Restore(last + 0.8, 1);
            var r = k.Trees.Tick();
            Assert.NotNull(r);
            Assert.Equal(CharityKitchen.PlayerKey, r.HostKey);
            Assert.Equal(3, r.Tier);                      // 130 akçe: yahni ve tatlı
            Assert.True(a.Mood.Has(TreehouseSystem.FeastMood));
            Assert.True(b.Mood.Has(TreehouseSystem.FeastMood));
            Assert.Contains(k.Rep, x => x.why == "Bereket Sofrası sahibi" && x.n == CharityKitchen.HostReputation);
            Assert.Null(k.Trees.Tick());                  // aynı gün ikinci kez kurulmaz
            Assert.Empty(k.Trees.Kitchen.SeasonDonations);
        }

        [Fact]
        public void Kacis_BunalimdakiCocukEnYakinAgacEveGider_YasakliGitmez()
        {
            var k = Make();
            var tree = Tree(k);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Assert.Null(k.Trees.RefugeFor(kid));
            Do(k, dad, tree, "found_nest"); Do(k, dad, tree, "build");
            Assert.Equal(GreatTrees.LadderTile(tree), k.Trees.RefugeFor(kid));
            Assert.Null(k.Trees.RefugeFor(dad));
            k.Trees.SetBanned(kid, true);
            Assert.Null(k.Trees.RefugeFor(kid));
            Assert.Equal("Ağaç eve çıkması yasak.", k.Trees.CanStart(kid, tree, k.I(tree, "play")));
        }

        [Fact]
        public void Kayit_GidipGelir_KoskBaglantisiYenidenKurulur()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Linden);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, TeenAge);
            Do(k, dad, tree, "found_kiosk");
            for (int n = 0; n < 4; n++) Do(k, dad, tree, "build");
            Do(k, kid, tree, "carve");
            var plane = Tree(k, GreatTrees.Plane, 25, 25);
            k.Clock.Restore(0.5, 1);
            Do(k, dad, plane, "donate_50");
            var save = k.Trees.Export();

            k.World.FixedLinks.Clear();
            k.World.RebuildLinks();
            var t2 = new TreehouseSystem(k.World, k.Db, k.House, k.Pop, k.Clock);
            t2.Import(save);
            var s = t2.SiteOf(tree);
            Assert.Equal(TreehouseKind.Kiosk, s.Kind);
            Assert.True(s.IsBuilt);
            Assert.Contains(kid.Id, s.CarvedIds);
            Assert.Equal(50, t2.Kitchen.Fund);
            Assert.True(t2.SiteOf(plane).IsVillageInn);
            var ladder = GreatTrees.LadderTile(tree);
            Assert.True(k.World.Map.CanTraverse(ladder, new GridPos(ladder.X, ladder.Y, 1)));
            Assert.True(t2.IsShell(new GridPos(s.KioskX, s.KioskY, 1)));
            // Eski kayıt: alan yok.
            var t3 = new TreehouseSystem(k.World, k.Db, k.House, k.Pop, k.Clock);
            t3.Import(null);
            Assert.True(t3.SiteOf(plane).IsVillageInn);   // aşevi her kayıtta hazır
            Assert.Null(t3.SiteOf(tree));
        }

        [Fact]
        public void Rakip_HaneMevsimBasindaOrtakAgacaKurar_ArsadakineKurmaz()
        {
            var k = Make();
            k.World.Map.RestrictOwnership(0, 0, 15, 15);
            var mine = Tree(k, GreatTrees.Oak, 8, 8);
            var common = Tree(k, GreatTrees.Walnut, 30, 30);
            k.Trees.Neighbors = () => new[] { (7, "Demirci hanesi") };
            for (int season = 0; season < 40 && k.Trees.SiteOf(common)?.HasHouse != true; season++)
            {
                k.Clock.Restore(season * k.Clock.Config.DaysPerSeason + 0.1, 1);
                k.Trees.OnNewDay();
            }
            var s = k.Trees.SiteOf(common);
            Assert.True(s.IsBuilt);
            Assert.Equal(7, s.Builder);
            Assert.True(s.Kind >= TreehouseKind.Nest && s.Kind <= TreehouseKind.Fort);
            Assert.Null(k.Trees.SiteOf(mine));
            Assert.Contains(k.Notices, n => n.StartsWith("Demirci hanesi"));
            // Ortak ağaç evi onaran itibar alır.
            var dad = Person(k, 1, AdultAge);
            s.Durability = 40;
            Do(k, dad, common, "repair");
            Assert.Contains(k.Rep, r => r.why == "ortak ağaç evi onardı");
        }

        [Fact]
        public void YasamSistemi_EngelZinciriKayitVeIpMerdiveniBaglanir()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            var world = new WorldState(new GridMap(40, 40));
            var house = new Household(100);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var tree = world.PlaceItem(db.Items.Get(GreatTrees.Walnut), new GridPos(10, 10));
            life.Trees.Attach();
            var fx = new Thatchwick.Core.Work.InteractionEffects(world, house, db);
            life.Attach(fx);
            var kid = new Character(1, "Kız", new GridPos(10, 7), true) { CanStarve = false };
            kid.SetAge(ChildAge);
            pop.Add(kid);
            var found = tree.Def.Interactions.First(i => i.Id == "found_nest");
            house.AddItem("item.th_kit_nest", 1);
            Assert.Equal("Ağaç evi bir yetişkin kurar; çocuklar sonra yardıma gelir.", fx.CanStart(kid, tree, found));   // ev bakımı engeli ile zincir
            var runner = new Thatchwick.Core.Jobs.JobRunner(kid, world);
            life.Attach(runner);
            Assert.NotNull(runner.Refuge);
            var data = life.Export();
            Assert.NotNull(data.treehouses);
            life.Restore(data);
            Assert.Null(life.Trees.SiteOf(tree));
        }

        [Fact]
        public void Yerlesim_GercekBaslangicDunyasindaBesUluAgac_ArsadaBir_KoskYeriBos()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var w = new WorldState(Thatchwick.Core.Layout.StarterWorld.BuildMap(lay));
            Thatchwick.Core.Layout.StarterWorld.Place(w, db, lay, 1);
            w.Map.RestrictOwnership(4, 4, 67, 67);
            var placed = GreatTrees.Ensure(w, db);
            Assert.Equal(GreatTrees.Sites.Length, placed.Count);
            Assert.Single(placed, t => w.Map.IsOwned(t.Position));                 // arsada bir tane (karar 0-1)
            foreach (var t in placed)
            {
                var region = Regions.At(t.Position);
                Assert.Equal(GreatTrees.Sites.First(x => x.ItemId == t.Def.Id).RegionId, region?.Id);
                Assert.True(w.Map.IsWalkable(GreatTrees.LadderTile(t)), t.Def.Id + " ip merdiveni karosu açık");
            }
            Assert.Empty(GreatTrees.Ensure(w, db));
        }

        // ---------------- ikinci parti: belgede olup ilk sürüme girmeyenler ----------------

        private static TreehouseSite BuiltKiosk(Kit k, ItemInstance tree, Character dad)
        {
            if (GreatTrees.KioskSize(tree.Def) == 5) { k.House.AddItem("item.plank", 8); k.House.AddItem("item.nails", 4); }
            Do(k, dad, tree, "found_kiosk");
            for (int n = 0; n < 4; n++) Do(k, dad, tree, "build");
            return k.Trees.SiteOf(tree);
        }

        [Fact]
        public void Kosk_TasimaSiniri_GicirdarSonraEnAgirEsyaDuser()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Chestnut);   // 4x4, kestane: %10 az
            var dad = Person(k, 1, AdultAge);
            var s = BuiltKiosk(k, tree, dad);
            Assert.Equal(36, k.Trees.KioskCapacity(s));
            int x0 = s.KioskX, y0 = s.KioskY;
            // 6 masa (2x1, yük 4) = 24: henüz sorun yok.
            for (int n = 0; n < 3; n++) { k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0, y0 + n, 1)); k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0 + 2, y0 + n, 1)); }
            k.Trees.CheckLoad(s);
            Assert.Equal(24, k.Trees.KioskLoad(s));
            Assert.DoesNotContain(k.Notices, n => n.Contains("gıcırdıyor"));
            // 2 masa ve bir tabure daha: 34 ≥ %90 → uyarı.
            k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0, y0 + 3, 1));
            k.World.PlaceItem(k.D("item.table_wood"), new GridPos(x0 + 2, y0 + 3, 1));
            var stool1 = k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(x0, y0 - 1, 1));
            k.Trees.OnFurnished(stool1);
            Assert.Contains(k.Notices, n => n.Contains("dallar gıcırdıyor"));
            // Balkona iki tabure daha: 36'yı aşar → en ağır eşya (masa) düşer, hasarlı olarak envantere.
            k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(x0 + 2, y0 - 1, 1));   // x0+1: ip merdiveninin ucu, eşya konmaz
            var stool2 = k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(x0 + 3, y0 - 1, 1));
            k.Trees.OnFurnished(stool2);
            Assert.True(k.Trees.KioskLoad(s) <= k.Trees.KioskCapacity(s));
            Assert.True(k.House.DamagedList().Any(kv => kv.Key == "item.table_wood"));
            Assert.Contains(k.Notices, n => n.Contains("dallardan aşağı düştü"));
        }

        [Fact]
        public void Kosk_OdaTuruVeManzara()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Linden);
            var dad = Person(k, 1, AdultAge);
            var s = BuiltKiosk(k, tree, dad);
            var home = new HomeLife(k.World, k.Db, k.House, k.Clock) { IsKioskRoom = k.Trees.IsKioskRoom };
            var room = k.World.Map.Rooms.RoomAt(new GridPos(s.KioskX + 1, s.KioskY + 1, 1));
            Assert.Equal(RoomKind.TreeKiosk, home.Classify(room).kind);
            Assert.Equal("Ağaç köşkü", HomeLife.RoomKindName(RoomKind.TreeKiosk));
            var seat = k.World.PlaceItem(k.D("item.stool_wood"), new GridPos(s.KioskX + 1, s.KioskY + 1, 1));
            Assert.Equal(seat.Def.Comfort > 0 ? TreehouseSystem.ViewComfort : 0, k.Trees.ComfortBonus(seat));
            Assert.True(k.Trees.InKiosk(new GridPos(s.KioskX, s.KioskY, 1)));
        }

        [Fact]
        public void Hazine_OyuncuSecer()
        {
            var k = Make();
            var tree = Tree(k, GreatTrees.Walnut);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_hut");
            for (int n = 0; n < 3; n++) Do(k, dad, tree, "build");
            var toys = k.Db.Items.All.Where(d => d.Category == "kids").OrderBy(d => d.Price).Take(2).ToList();
            foreach (var t in toys) k.House.AddItem(t.Id, 1);
            var cands = k.Trees.StashCandidates();
            Assert.Contains(toys[0].Id, cands);
            k.Trees.ChooseStash(kid.Id, toys[0].Id);   // ucuz olanı seçti (kendiliğinden pahalısı seçilirdi)
            Do(k, kid, tree, "stash");
            Assert.Equal(toys[0].Id, k.Trees.SiteOf(tree).Stash.ItemId);
            Assert.Equal(1, k.House.Count(toys[1].Id));
        }

        [Fact]
        public void Asevi_UcOgun_ParasizligeGoreHayirSofrasi_YiyecekBagisi()
        {
            var k = Make(akce: 6);
            var plane = Tree(k, GreatTrees.Plane, 15, 15);
            var c = Person(k, 1, AdultAge);
            k.Clock.Restore(0.5, 1);
            Assert.Equal("8 akçe gerekir.", k.Trees.CanStart(c, plane, k.I(plane, "inn_eat")));
            Assert.Equal("14 akçe gerekir.", k.Trees.CanStart(c, plane, k.I(plane, "inn_roast")));
            Assert.Equal("Hayır sofrası parası olmayanlar için.", k.Trees.CanStart(c, plane, k.I(plane, "inn_charity")));   // çorbaya (5) yetiyor
            Do(k, c, plane, "inn_soup");
            Assert.Equal(1, k.House.Akce);
            Do(k, c, plane, "inn_charity");                                                                                   // artık yetmiyor
            Assert.Equal("Bağışlanacak yiyecek yok (ekmek, tahıl, sebze, pişmiş yemek).", k.Trees.CanStart(c, plane, k.I(plane, "donate_food")));
            var food = k.Db.Items.All.First(d => d.HasTag("food") && !d.HasTag("raw") && d.Price > 0);
            k.House.AddItem(food.Id, 8);
            Do(k, c, plane, "donate_food");
            Assert.Equal(3, k.House.Count(food.Id));                                                                          // en çok 5
            Assert.Equal(5 * food.Price, k.Trees.Kitchen.Fund);
            Assert.True(c.Mood.Has(TreehouseSystem.GoodDeedMood));
        }

        [Fact]
        public void Koyluler_AseveOglen_AgacEveIkindi_SofrayaAksam()
        {
            var k = Make();
            var plane = Tree(k, GreatTrees.Plane, 15, 15);
            var walnut = Tree(k, GreatTrees.Walnut, 30, 30);
            var dad = Person(k, 1, AdultAge);
            Do(k, dad, walnut, "found_nest"); Do(k, dad, walnut, "build");
            var kid = Person(k, 2, ChildAge, villager: true);
            var adult = Person(k, 3, AdultAge, villager: true);
            k.Trees.Kitchen.Donate(50, 0);
            for (int d = 0; d < 30 && (!k.Trees.SiteOf(walnut).Visitors.Contains(kid.Id) || k.Trees.VillagerPlace(adult, 0.5, WeatherType.Clear) != "inn"); d++)
            {
                k.Clock.Restore(d + 0.1, 1);
                k.Trees.OnNewDay();
            }
            Assert.Contains(kid.Id, k.Trees.SiteOf(walnut).Visitors);
            Assert.Equal("tree:" + walnut.Id, k.Trees.VillagerPlace(kid, 0.6, WeatherType.Clear));
            Assert.Null(k.Trees.VillagerPlace(kid, 0.6, WeatherType.Rain));
            Assert.Equal(GreatTrees.LadderTile(walnut), k.Trees.PlaceTile("tree:" + walnut.Id));
            kid.Position = GreatTrees.LadderTile(walnut);
            Assert.True(k.Trees.VillagerInside(kid, 0.6, WeatherType.Clear));
            // Mevsimin son günü akşamı herkes sofraya.
            int last = k.Clock.Config.DaysPerSeason - 1;
            k.Clock.Restore(last + 0.72, 1);
            Assert.Equal("feast", k.Trees.VillagerPlace(adult, 0.72, WeatherType.Clear));
            Assert.True(k.Trees.FeastGatherNow);
            Assert.NotNull(k.Trees.PlaceTile("feast"));
        }

        [Fact]
        public void Ebeveyn_KusenCocuguAsagiCagirir()
        {
            var k = Make();
            var tree = Tree(k);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_nest"); Do(k, dad, tree, "build");
            var affection = new List<(int, int, int)>();
            k.Trees.AddAffection = (a, b, d) => affection.Add((a, b, d));
            Assert.False(k.Trees.Shows(tree, k.I(tree, "call_down")));
            Assert.True(kid.BeginBreakdown(Thatchwick.Core.Mood.BreakdownKind.Cry, 1.0));
            Assert.NotNull(k.Trees.RefugeFor(kid));
            Assert.True(k.Trees.Shows(tree, k.I(tree, "call_down")));
            Assert.Equal("Küsen çocuğu bir büyük çağırır.", k.Trees.CanStart(kid, tree, k.I(tree, "call_down")));
            Do(k, dad, tree, "call_down");
            Assert.False(kid.InBreakdown);
            Assert.True(kid.Mood.Has(TreehouseSystem.CalledDownMood));
            Assert.Contains(affection, x => x.Item1 == dad.Id && x.Item2 == kid.Id);
            Assert.True(dad.Skills.GetXp(SkillType.Parenting) > 0);
        }

        [Fact]
        public void Cocukluk_OyuncakAliskanligi_YukseklikSevdalisiIzi()
        {
            var k = Make();
            var toys = new Thatchwick.Core.Upbringing.ToyHabitSystem();
            k.Trees.ToyBoost = (c, f) => toys.PlayFavorite(c, k.Clock.TotalDays, f);
            var tree = Tree(k);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_nest"); Do(k, dad, tree, "build");
            toys.OnPlayed(kid, "item.spinning_top", 0);
            double before = toys.HabitOf(kid.Id, "item.spinning_top");
            Do(k, kid, tree, "play");
            Assert.Equal(before * (1 + TreehouseSystem.ToyBoostFactor), toys.HabitOf(kid.Id, "item.spinning_top"), 3);
            for (int n = 1; n < TreehouseSystem.HeightsPlays; n++) k.Trees.OnApplied(kid, tree, k.I(tree, "play"));
            Assert.True(k.Trees.EarnedHeightsMark(kid));
            Assert.True(toys.AddMark(kid.Id, TreehouseSystem.HeightsMark));
            Assert.Equal("Yükseklik sevdalısı", toys.MarkName(TreehouseSystem.HeightsMark));
            Assert.Equal(1.10, toys.XpFactor(kid.Id, SkillType.Woodcutting), 3);
            Assert.Equal(1.10, toys.XpFactor(kid.Id, SkillType.Riding), 3);
            // Oynama sayısı kayıtla taşınır.
            var t2 = new TreehouseSystem(k.World, k.Db, k.House, k.Pop, k.Clock);
            t2.Import(k.Trees.Export());
            Assert.True(t2.EarnedHeightsMark(kid));
        }

        [Fact]
        public void Dusme_NadirenGercekYara()
        {
            var k = Make();
            int injured = 0;
            k.Trees.Injure = c => { injured++; return true; };
            var tree = Tree(k);
            var dad = Person(k, 1, AdultAge);
            var kid = Person(k, 2, ChildAge);
            Do(k, dad, tree, "found_nest"); Do(k, dad, tree, "build");
            for (int n = 0; n < 4000; n++) k.Trees.OnApplied(kid, tree, k.I(tree, "play"));
            Assert.InRange(injured, 1, 30);   // 4000 × %2 × %10 ≈ 8
        }

        [Fact]
        public void Istek_KoyCocugununAilesiRicaEder_HaneCocuguDileginiSoyler()
        {
            var k = Make();
            k.World.Map.RestrictOwnership(0, 0, 15, 15);
            Tree(k, GreatTrees.Walnut, 30, 30);   // boş ortak ağaç
            var mom = Person(k, 1, AdultAge, villager: true);
            var vkid = Person(k, 2, ChildAge, villager: true);
            vkid.BirthMotherId = mom.Id;
            var ownKid = Person(k, 3, ChildAge);
            var asked = new List<(Character giver, Character kid)>();
            k.Trees.PostRequest = (g, c) => { asked.Add((g, c)); return true; };
            for (int season = 0; season < 30 && asked.Count == 0; season++)
            {
                k.Clock.Restore(season * k.Clock.Config.DaysPerSeason + 2.1, 1);
                k.Trees.OnNewDay();
            }
            Assert.Single(asked);
            Assert.Same(mom, asked[0].giver);
            Assert.Contains(k.Notices, n => n.StartsWith(ownKid.Name + ": \"Ulu ağaca bir ağaç ev"));
        }

        [Fact]
        public void Rakip_KulubeVeKoskDeKurar_KendiEviniOnarir()
        {
            var kinds = new HashSet<TreehouseKind>();
            for (int seed = 1; seed < 40 && !(kinds.Contains(TreehouseKind.Hut) && kinds.Contains(TreehouseKind.Kiosk)); seed++)
            {
                var k = Make();
                k.Trees = new TreehouseSystem(k.World, k.Db, k.House, k.Pop, k.Clock, seed) { Neighbors = () => new[] { (7, "Demirci hanesi") } };
                k.World.Map.RestrictOwnership(0, 0, 2, 2);
                var t = Tree(k, GreatTrees.Oak, 20, 20);
                for (int season = 0; season < 20 && k.Trees.SiteOf(t)?.HasHouse != true; season++)
                {
                    k.Clock.Restore(season * k.Clock.Config.DaysPerSeason + 0.1, 1);
                    k.Trees.OnNewDay();
                }
                var s = k.Trees.SiteOf(t);
                if (s?.HasHouse != true) continue;
                kinds.Add(s.Kind);
                if (s.Kind == TreehouseKind.Kiosk)
                {
                    Assert.Equal(5, s.KioskN);
                    Assert.False(k.Trees.ExtraOwned(new GridPos(s.KioskX, s.KioskY, 1)));   // rakibin köşkü oyuncunun eşya alanı değil
                    Assert.True(k.Trees.IsShell(new GridPos(s.KioskX, s.KioskY, 1)));
                }
                // Kendi evini onarır.
                s.Durability = 0; s.RuinedSince = k.Clock.TotalDays;
                for (int d = 1; d < 40 && s.IsRuined; d++) { k.Clock.Restore(k.Clock.TotalDays + 1, 1); k.Trees.OnNewDay(); }
                Assert.False(s.IsRuined);
            }
            Assert.Contains(TreehouseKind.Hut, kinds);
            Assert.Contains(TreehouseKind.Kiosk, kinds);
        }
    }
}
