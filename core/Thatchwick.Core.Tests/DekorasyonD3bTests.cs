using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ev dekorasyonu D3b (Project belgesi 27 §7.6-§7.12, §8.2, §8.4, §9): kazanılan hatıralar, soy ağacı işlemesi,
    // çeyiz sandığı, Asil kademe, mevsim süsleri, hayvan bölmesi süsleri.
    public class DekorasyonD3bTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public Population Pop;
            public GameClock Clock;
            public LifeSystems Life;
            public BuildService Build;
            public ItemDef D(string id) => Db.Items.Get(id);
        }

        private static Kit Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(20, 20)), House = new Household(5000), Pop = new Population(), Clock = new GameClock() };
            k.Life = new LifeSystems(db, k.World, k.House, k.Pop, k.Clock);
            k.Build = new BuildService(k.World, k.House, db);
            // 2..7 x 2..5 iç oda.
            for (int x = 2; x <= 7; x++) { Wall(k, x, 2, EdgeAxis.Horizontal); Wall(k, x, 6, EdgeAxis.Horizontal); }
            for (int y = 2; y <= 5; y++) { Wall(k, 2, y, EdgeAxis.Vertical); Wall(k, 8, y, EdgeAxis.Vertical); }
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 5; y++) k.World.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
            return k;
        }

        private static void Wall(Kit k, int x, int y, EdgeAxis axis) => k.World.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall);

        private static Character Member(Kit k, string name, Sex sex, double age = 30, bool villager = false)
        {
            var c = new Character(k.Pop.NextId(), name, new GridPos(3, 3)) { Sex = sex, MoodEnabled = true, IsVillager = villager };
            c.SetAge(age);
            k.Pop.Add(c);
            return c;
        }

        [Fact]
        public void Icerik_HatiralarSatilmazAlinmaz_MevsimVeHayvanSusleri()
        {
            var k = Make();
            foreach (var id in new[] { KeepsakeService.FestivalCup, KeepsakeService.HarvestRibbon, KeepsakeService.FirstFish, KeepsakeService.FirstShoes, KeepsakeService.ElderCup })
            {
                var d = k.D(id);
                Assert.True(d.IsKeepsake, id);
                Assert.Equal(0, d.Price);
                Assert.DoesNotContain(d, ItemCatalog.InCategory(k.Db, d.Category, null, null, (string)null));
                Assert.Contains("Hatıra", ItemCatalog.Describe(d, null, k.Db));
            }
            var tree = k.D(KeepsakeService.FamilyTree);
            Assert.Equal(3, tree.WallWidth);
            Assert.True(tree.HasTag("craftonly"));
            var chest = k.D(KeepsakeService.DowryChest);
            Assert.True(chest.IsFillDecor);
            Assert.Equal(ItemPlacement.Floor, chest.Placement);
            Assert.True(chest.AcceptsFill("item.linen"));
            Assert.Equal(new[] { Season.Spring }, k.D("item.spring_branch").DecorSeasons.ToArray());
            Assert.True(k.D("item.door_wreath").InSeason(Season.Winter));
            Assert.False(k.D("item.door_wreath").InSeason(Season.Spring));
            Assert.Contains("solar", ItemCatalog.Describe(k.D("item.winter_greenery"), null, k.Db));
            Assert.Equal(3, k.D("item.coop_curtain").AnimalComfort);
            Assert.Equal(ItemPlacement.Wall, k.D("item.horseshoe_display").Placement);
            Assert.Equal((4, 3), (k.D("item.rug_kufic").Width, k.D("item.rug_kufic").Height));
            Assert.Equal("Asil", k.D("item.tapestry_hunt").StyleSet);
            Assert.Equal(3, k.D("item.tapestry_hunt").WallWidth);
            // Tarifler: soy ağacı ve armalı halı dokuma tezgâhında, çeyiz sandığı marangozda.
            Assert.Contains(k.D("item.loom").Interactions, i => i.Id == "craft:family_tree_embroidery" && i.MinSkillLevel == 5);
            Assert.Contains(k.D("item.loom").Interactions, i => i.Id == "craft:rug_crest");
            Assert.Contains(k.D("item.workbench").Interactions, i => i.Id == "craft:dowry_chest");
            Assert.Contains(k.D("item.forge").Interactions, i => i.Id == "craft:horseshoe_display");
        }

        [Fact]
        public void Senlik_KupaVerir_KatilanYoksaVermez()
        {
            var k = Make();
            Assert.Null(k.Life.Keepsakes.OnFestival("Bahar Şenliği", 0));
            var r = k.Life.Keepsakes.OnFestival("Bahar Şenliği", 3);
            Assert.NotNull(r);
            Assert.Equal("Bahar Şenliği kupası (1. yıl)", r.Label);
            Assert.Equal(1, k.House.Count(KeepsakeService.FestivalCup));
            Assert.Contains(k.Life.Log.Items, e => e.Text.Contains("Hatıra kazanıldı"));
        }

        [Fact]
        public void IlkBalik_KisiBasinaBirKez_HasatKurdelesiYildaBir()
        {
            var k = Make();
            var wat = Member(k, "Wat", Sex.Male);
            var joan = Member(k, "Joan", Sex.Female);
            var villager = Member(k, "Orhan", Sex.Male, villager: true);
            Assert.NotNull(k.Life.Keepsakes.OnFishCaught(wat, "item.fish_carp"));
            Assert.Null(k.Life.Keepsakes.OnFishCaught(wat, "item.fish_trout"));
            Assert.NotNull(k.Life.Keepsakes.OnFishCaught(joan, "item.fish_trout"));
            Assert.Null(k.Life.Keepsakes.OnFishCaught(villager, "item.fish_trout"));   // köylünün balığı hanenin hatırası değil
            Assert.Equal(2, k.House.Count(KeepsakeService.FirstFish));
            Assert.Equal("Wat: ilk balık (sazan)", k.Life.Keepsakes.Records[0].Label.Replace("Sazan", "sazan"));

            var crop = k.World.PlaceItem(k.D("item.crop_wheat"), new GridPos(12, 12));
            Assert.Null(k.Life.Keepsakes.OnCropHarvested(wat, crop, 1));                 // seçme yetmez
            Assert.NotNull(k.Life.Keepsakes.OnCropHarvested(wat, crop, 2));
            Assert.Null(k.Life.Keepsakes.OnCropHarvested(joan, crop, 2));                // aynı yıl ikincisi yok
            Assert.Equal(1, k.House.Count(KeepsakeService.HarvestRibbon));
        }

        [Fact]
        public void Dogum_IlkAyakkabi_YasliOlunce_KalayliKupa()
        {
            var k = Make();
            var baby = Member(k, "Ada", Sex.Female, 0.5);
            Assert.NotNull(k.Life.Keepsakes.OnBirth(baby));
            var elder = Member(k, "Dede", Sex.Male, 30);
            Assert.Null(k.Life.Keepsakes.OnDeath(elder));                                // yaşlı değil
            elder.SetAge(elder.AgeConfig.ElderStartDay + 1);
            Assert.Equal(AgeStage.Elder, elder.Stage);
            Assert.Contains("Dede: kalaylı kupa", k.Life.Keepsakes.OnDeath(elder).Label);
            Assert.Equal(1, k.House.Count(KeepsakeService.FirstShoes));
            Assert.Equal(1, k.House.Count(KeepsakeService.ElderCup));
        }

        [Fact]
        public void Hatira_SatilmazRaflaSatilincaEnvantereDoner_YaziyiTasir()
        {
            var k = Make();
            k.Life.Keepsakes.OnFestival("Hasat Şenliği", 4);
            Assert.False(k.Build.SellFromInventory(KeepsakeService.FestivalCup).Ok);
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), new GridPos(5, 2), 0);
            Assert.True(k.House.TryTakeItem(KeepsakeService.FestivalCup));
            var cup = k.World.PlaceOnSurface(k.D(KeepsakeService.FestivalCup), shelf.Id, "a");
            Assert.NotNull(cup);
            Assert.Equal("Hasat Şenliği kupası (1. yıl)", k.Life.Keepsakes.LabelOf(cup));
            Assert.False(k.Build.SellPlaced(cup.Id).Ok);
            int akce = k.House.Akce;
            Assert.True(k.Build.SellPlaced(shelf.Id).Ok);                                  // raf satılır, kupa envantere döner
            Assert.Equal(1, k.House.Count(KeepsakeService.FestivalCup));
            Assert.True(k.House.Akce > akce);
            k.Life.Keepsakes.Reconcile();
            Assert.Single(k.Life.Keepsakes.Records);
            Assert.Equal(0, k.Life.Keepsakes.Records[0].InstanceId);                      // kayıt envantere döndü, yazısı duruyor

            // Envanterden kaybolan hatıranın kaydı defterden düşer.
            k.House.TryTakeItem(KeepsakeService.FestivalCup);
            k.Life.Keepsakes.Reconcile();
            Assert.Empty(k.Life.Keepsakes.Records);
        }

        [Fact]
        public void Guzellik_HatiraNesilleArtar_SoyAgaciDallanir()
        {
            var k = Make();
            int gen = 1;
            k.Life.Keepsakes.Generation = () => gen;
            k.Life.Keepsakes.OnFestival("Kış Şenliği", 2);
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), new GridPos(4, 2), 0);
            k.House.TryTakeItem(KeepsakeService.FestivalCup);
            var cup = k.World.PlaceOnSurface(k.D(KeepsakeService.FestivalCup), shelf.Id, "a");
            var tree = k.World.PlaceOnWall(k.D(KeepsakeService.FamilyTree), new GridPos(5, 2), 0);
            Assert.NotNull(tree);
            Assert.Equal(0, k.Life.Keepsakes.BeautyBonus(cup));
            Assert.Equal(0, k.Life.Keepsakes.BeautyBonus(tree));
            var room = k.World.Map.Rooms.RoomAt(new GridPos(3, 3));
            int before = k.Life.Mood.BeautyOf(room).Items;
            gen = 3;
            Assert.Equal(2, k.Life.Keepsakes.BeautyBonus(cup));
            Assert.Equal(2, k.Life.Keepsakes.BeautyBonus(tree));
            gen = 12;
            Assert.Equal(KeepsakeService.MaxAgeBonus, k.Life.Keepsakes.BeautyBonus(cup));
            Assert.Equal(KeepsakeService.MaxTreeBonus, k.Life.Keepsakes.BeautyBonus(tree));
            Assert.True(k.Life.Mood.BeautyOf(room).Items > before);
            Assert.StartsWith("Soy ağacı: 12 nesil", k.Life.Keepsakes.LabelOf(tree));
        }

        [Fact]
        public void SoyAgaci_Koklerimiz_YasiKisaltir()
        {
            var k = Make();
            var wat = Member(k, "Wat", Sex.Male);
            var joan = Member(k, "Joan", Sex.Female);
            wat.AddMoodlet(MoodCatalog.Grief);
            joan.AddMoodlet(MoodCatalog.Grief);
            double Grief(Character c) => c.Mood.Active.First(m => m.Def.Id == MoodCatalog.Grief).RemainingDays;
            double start = Grief(wat);
            k.Life.Keepsakes.Daily(new[] { wat });
            Assert.False(wat.Mood.Has(KeepsakeService.RootsMoodId));
            Assert.Equal(start, Grief(wat), 6);

            k.World.PlaceOnWall(k.D(KeepsakeService.FamilyTree), new GridPos(5, 2), 0);
            Assert.True(k.Life.Keepsakes.HasFamilyTree());
            k.Life.Keepsakes.Daily(new[] { wat });
            Assert.True(wat.Mood.Has(KeepsakeService.RootsMoodId));
            Assert.Equal(start - KeepsakeService.RootsGriefSpeed, Grief(wat), 6);
            Assert.Equal(start, Grief(joan), 6);
        }

        [Fact]
        public void CeyizSandigi_DoluysaGelinleGider_MuhurVeYakinlik()
        {
            var k = Make();
            var family = new FamilyService(k.Pop, k.House, k.Db);
            k.Life.Attach(family);
            var father = Member(k, "Wat", Sex.Male, 55);
            var daughter = Member(k, "Edith", Sex.Female, 30);
            var groom = Member(k, "Murat", Sex.Male, 31, villager: true);
            family.SetAffection(daughter.Id, groom.Id, Legacy.MarryOutAffection);

            var chest = k.World.PlaceItem(k.D(KeepsakeService.DowryChest), new GridPos(4, 4));
            Assert.NotNull(chest);
            k.House.AddItem("item.linen", 4);
            k.House.AddItem("item.cloth", 2);
            Assert.Null(DecorSystem.FillBlocker(chest, k.House, k.Db));
            Assert.Equal(6, DecorSystem.Fill(chest, k.House, k.Db));
            Assert.True(MoodService.BeautyOfItem(chest, k.Db) > k.D(KeepsakeService.DowryChest).Beauty * 0.3);

            int seals = k.Life.Legacy.Seals;
            Assert.True(k.Life.Legacy.MarryOut(daughter, groom, null, family, 1.0, out var msg), msg);
            Assert.Null(k.World.GetItem(chest.Id));                                           // sandık gelinle gitti
            Assert.Equal(seals + Legacy.MarriedOutSeals + KeepsakeService.DowryFullSeals, k.Life.Legacy.Seals);
            Assert.Equal(KeepsakeService.DowryAffection, family.Affection(father.Id, groom.Id));
            Assert.True(daughter.Mood.Has(KeepsakeService.DowryMoodId));
            Assert.Equal(0, k.House.Count("item.linen"));                                     // dokumalar sandıkla gitti
        }

        [Fact]
        public void CeyizSandigi_YarimsaBonusYok_GelinGelinceSandikGelir()
        {
            var k = Make();
            var groom = Member(k, "Murat", Sex.Male, 31, villager: true);
            var daughter = Member(k, "Edith", Sex.Female, 30);
            var chest = k.World.PlaceItem(k.D(KeepsakeService.DowryChest), new GridPos(4, 4));
            k.House.AddItem("item.felt", 2);
            DecorSystem.Fill(chest, k.House, k.Db);
            int seals = k.Life.Legacy.Seals;
            string msg = k.Life.Keepsakes.OnMarriedOut(daughter, groom, null, k.Life.Legacy);
            Assert.Contains("yarım çeyiz", msg);
            Assert.Equal(seals, k.Life.Legacy.Seals);
            Assert.Null(k.Life.Keepsakes.OnMarriedOut(Member(k, "Hal", Sex.Male), groom));  // oğul sandık götürmez

            var bride = Member(k, "Leyla", Sex.Female, 25, villager: true);
            Assert.Contains("çeyiz sandığıyla geldi", k.Life.Keepsakes.OnJoined(bride));
            Assert.Equal(1, k.House.Count(KeepsakeService.DowryChest));
            Assert.Equal(2, k.House.Count("item.linen"));
            Assert.Equal(1, k.House.Count("item.embroidery_panel"));
        }

        [Fact]
        public void MevsimSusu_MevsimiGecinceSolar_GuzellikEksi()
        {
            var k = Make();
            var branch = k.D("item.winter_greenery");
            var it = k.World.PlaceOnWall(branch, new GridPos(5, 2), 0, Decor.BandHigh);
            Assert.NotNull(it);
            Assert.False(DecorSystem.IsWilted(it, Season.Winter));
            Assert.Equal(0, DecorSystem.SeasonalBeauty(it, Season.Winter));
            Assert.True(DecorSystem.IsWilted(it, Season.Spring));
            Assert.Equal(DecorSystem.WiltedBeauty, MoodService.BeautyOfItem(it, k.Db) + DecorSystem.SeasonalBeauty(it, Season.Spring), 6);
            var notes = new List<string>();
            var decor = new DecorSystem(k.World, k.Db) { Notify = (t, _) => notes.Add(t) };
            Assert.Equal(0, decor.OnSeasonChanged(Season.Winter));
            Assert.Equal(1, decor.OnSeasonChanged(Season.Spring));
            Assert.Contains("kış dalı", notes.Single());
        }

        [Fact]
        public void HayvanBolmesi_DuvarSusleriKonforaSayilir()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(24, 24);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) map.SetOwned(new GridPos(x, y), true);
            var world = new WorldState(map);
            var house = new Household(20000);
            var build = new BuildService(world, house, db);
            var templates = new RoomTemplateService(build, world, house, db);
            var animals = new AnimalSystem(world, db);
            build.HousingResidents = animals.Residents;
            Assert.True(templates.Build("template.coop_room", new GridPos(2, 2), out var placed).Ok);
            var home = placed.First(i => i.Def.IsHousing);
            float before = animals.ComfortOf(home).Decor;
            var curtain = db.Items.Get("item.coop_curtain");
            ItemInstance hung = null;
            foreach (var t in world.Map.Rooms.RoomAt(home.Position).Tiles)
            {
                for (int side = 0; side < 4 && hung == null; side++)
                    if (world.CanPlaceOnWall(curtain, t, side).Ok) hung = world.PlaceOnWall(curtain, t, side);
                if (hung != null) break;
            }
            Assert.NotNull(hung);
            Assert.Equal(System.Math.Min(15f, before + 3f), animals.ComfortOf(home).Decor);
        }

        [Fact]
        public void Kayit_HatiraDefteriVeIlklerGeriGelir()
        {
            var k = Make();
            var wat = Member(k, "Wat", Sex.Male);
            k.Life.Keepsakes.OnFishCaught(wat, "item.fish_carp");
            k.Life.Keepsakes.OnFestival("Yaz Şenliği", 2);
            var data = k.Life.Export();
            Assert.Equal(2, data.keepsakes.records.Length);

            var k2 = Make();
            k2.Life.Restore(data);
            Assert.Equal(2, k2.Life.Keepsakes.Records.Count);
            Assert.Equal("Yaz Şenliği kupası (1. yıl)", k2.Life.Keepsakes.Records[1].Label);
            Assert.True(k2.Life.Keepsakes.HasFirst($"fish:{wat.Id}"));
            Assert.Null(k2.Life.Keepsakes.OnFishCaught(wat, "item.fish_trout"));
        }
    }
}
