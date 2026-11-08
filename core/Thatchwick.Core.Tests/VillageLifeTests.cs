using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 38 (köylüler, projeler), 39 (aletler), 40 (meslek dalları), 41 (hedefler), 43 (geri al, varyant, şablon); T21.
    public class VillageLifeTests
    {
        private sealed class Rig
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public Population Pop;
            public GameClock Clock;
            public LifeSystems Life;
            public FamilyService Family;
            public Character Head;
            public BuildService Build;
        }

        private static Rig NewRig(int akce = 0, bool ownAll = true)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            if (ownAll) for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var world = new WorldState(map);
            var house = new Household(akce);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var family = new FamilyService(pop, house, db, seed: 2);
            life.Attach(family);
            var head = new Character(pop.NextId(), "Aylin", new GridPos(8, 8), true) { Sex = Sex.Female, CanStarve = false };
            head.SetAge(40);
            pop.Add(head);
            var build = new BuildService(world, house, db);
            return new Rig { Db = db, World = world, House = house, Pop = pop, Clock = clock, Life = life, Family = family, Head = head, Build = build };
        }

        private static void Level(Character c, SkillType skill, int level) => c.GainSkillXp(skill, SkillRules.XpForLevel(level) + 1);

        // ---------- Meslek dalları (40) ----------

        [Fact]
        public void Dallar_HerBeceriIcinIkiVeDortSecenek()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                var mine = db.Branches.All.Where(b => b.Skill == s).ToList();
                Assert.Equal(2, mine.Count(b => b.Level == 5));
                Assert.Equal(4, mine.Count(b => b.Level == 10));
                foreach (var b10 in mine.Where(b => b.Level == 10)) Assert.Contains(mine, b5 => b5.Id == b10.Requires && b5.Level == 5);
            }
        }

        [Fact]
        public void Dal_SeviyeGerekirTekSecilirSonrakiKademeBaglidir()
        {
            var r = NewRig();
            var c = r.Head;
            Assert.Empty(LifeRules.AvailableBranches(r.Db, c));
            Level(c, SkillType.Farming, 5);
            Assert.Equal(2, LifeRules.AvailableBranches(r.Db, c).Count);

            Assert.True(LifeRules.ChooseBranch(r.Db, c, "branch.farm_gardener", out _));
            Assert.False(LifeRules.ChooseBranch(r.Db, c, "branch.farm_harvester", out var why));   // kademede tek dal
            Assert.Contains("kademe", why);
            Assert.True(c.Modifiers.GrowthSpeed > 1.1f);

            Assert.False(LifeRules.ChooseBranch(r.Db, c, "branch.farm_master", out why));           // seviye 10 yok
            Level(c, SkillType.Farming, 10);
            Assert.False(LifeRules.ChooseBranch(r.Db, c, "branch.farm_bounty", out why));           // yanlış dalın devamı
            Assert.True(LifeRules.ChooseBranch(r.Db, c, "branch.farm_master", out _));
            Assert.Equal(1.15f * 1.2f, c.Modifiers.GrowthSpeed, 3);
            Assert.Equal("Çiftçi", r.Life.TradeOf(c));
        }

        [Fact]
        public void BeceriSeviyesiAtlayinca_DalBildirimiGelir()
        {
            var r = NewRig();
            Level(r.Head, SkillType.Woodcutting, 5);
            Assert.Contains(r.Life.Log.Items, e => e.Text.Contains("meslek dalı seçebilir"));
        }

        [Fact]
        public void SoyMesleği_HanedanGecmisineYazilir()
        {
            var r = NewRig();
            Level(r.Head, SkillType.Woodcutting, 5);
            LifeRules.ChooseBranch(r.Db, r.Head, "branch.wood_sturdy", out _);
            var heir = new Character(r.Pop.NextId(), "Varis", new GridPos(8, 8)) { BirthMotherId = r.Head.Id, Generation = 2 };
            heir.SetAge(30);
            r.Pop.Add(heir);
            var dyn = new Dynasty();
            dyn.Found(r.Head, 0);
            var result = dyn.OnDeath(r.Head, r.Pop.Members, 12, r.Life.TradeOf(r.Head));
            Assert.Same(heir, result.Heir);
            Assert.Equal("Oduncu", dyn.History[0].Trade);
            Assert.Equal("Oduncu soyu", r.Life.FamilyTrade());
        }

        // ---------- Köylüler (38) ----------

        [Fact]
        public void Koyluler_OlusturulurSosyalEglenceAzalmazVeDemirciVar()
        {
            var r = NewRig();
            var spawned = r.Life.Villagers.SpawnMissing(name => new GridPos(3, 3));
            Assert.Equal(r.Db.Villagers.Count, spawned.Count);
            Assert.All(spawned, c => Assert.True(c.IsVillager));
            Assert.Empty(r.Life.Villagers.SpawnMissing());   // ikinci çağrı yenisini üretmez
            Assert.True(r.Life.Villagers.HasRole(VillagerService.RoleBlacksmith));

            var orhan = r.Life.Villagers.CharacterOf("villager.orhan");
            orhan.Needs.Set(NeedType.Social, 20); orhan.Needs.Set(NeedType.Fun, 20);
            for (int i = 0; i < 20; i++) orhan.Tick(0.1);
            Assert.Equal(20f, orhan.Needs.Get(NeedType.Social), 3);
            Assert.Equal(20f, orhan.Needs.Get(NeedType.Fun), 3);

            r.Head.Needs.Set(NeedType.Social, 20);
            for (int i = 0; i < 20; i++) r.Head.Tick(0.1);
            Assert.True(r.Head.Needs.Get(NeedType.Social) < 20f);
        }

        [Fact]
        public void Hediye_ZevkeGoreAyniGundeBirKez()
        {
            var r = NewRig();
            r.Life.Villagers.SpawnMissing();
            var v = r.Life.Villagers;
            var orhan = v.CharacterOf("villager.orhan");
            r.House.AddItem("item.honey", 2); r.House.AddItem("item.herb", 2); r.House.AddItem("item.turnip", 2);

            Assert.True(v.Give(r.Head, "villager.orhan", "item.herb", out var msg));    // sevmez
            Assert.Contains("hoşlanmadı", msg);
            Assert.Equal(0, r.Family.Affection(r.Head.Id, orhan.Id));                    // -15 ama sıfır altına inmez
            Assert.False(v.Give(r.Head, "villager.orhan", "item.honey", out msg));       // aynı gün
            Assert.Contains("bugün", msg);

            r.Clock.Restore(1.1, 1);
            Assert.True(v.Give(r.Head, "villager.orhan", "item.honey", out msg));       // bayılır
            Assert.Contains("bayıldı", msg);
            Assert.Equal(40, r.Family.Affection(r.Head.Id, orhan.Id));
            Assert.Equal(15, v.StateOf("villager.orhan").Romance);                       // baş kadın, Orhan erkek: romantik yakınlık da artar
        }

        [Fact]
        public void Hediye_EtiketleEslesirVeKaynakDisiReddedilir()
        {
            var r = NewRig();
            r.Life.Villagers.SpawnMissing();
            var def = r.Db.Villagers.Get("villager.orhan");
            Assert.Equal(GiftTaste.Like, VillagerService.TasteFor(def, r.Db.Items.Get("item.firewood")));   // tag:fuel
            Assert.Equal(GiftTaste.Love, VillagerService.TasteFor(def, r.Db.Items.Get("item.honey")));
            Assert.Equal(GiftTaste.Neutral, VillagerService.TasteFor(def, r.Db.Items.Get("item.egg")));
            r.House.AddItem("item.chair_wood");
            Assert.False(r.Life.Villagers.Give(r.Head, "villager.orhan", "item.chair_wood", out var msg));
            Assert.Contains("kaynak", msg);
            Assert.False(r.Life.Villagers.Give(r.Head, "villager.orhan", "item.turnip", out msg));
            Assert.Contains("Envanterde", msg);
        }

        [Fact]
        public void Evlilik_ArkadaslikVeRomantikYakinlikIkisiBirdenGerekir()
        {
            var r = NewRig();
            r.Life.Villagers.SpawnMissing();
            var v = r.Life.Villagers;
            var orhan = v.CharacterOf("villager.orhan");

            // Yalnız arkadaşlık (5 sohbet) yetmez.
            for (int i = 0; i < 5; i++) r.Family.Apply(r.Head, orhan, SocialAction.Chat);
            Assert.Equal(100, r.Family.Affection(r.Head.Id, orhan.Id));
            Assert.Contains("romantik", r.Family.CanDo(r.Head, orhan, SocialAction.Propose));

            // Yedi günde yedi sevdiği hediye romantik yakınlığı doldurur.
            r.House.AddItem("item.honey", 7);
            for (int d = 0; d < 7; d++)
            {
                r.Clock.Restore(d + 0.5, 1);
                Assert.True(v.Give(r.Head, "villager.orhan", "item.honey", out _));
            }
            Assert.Equal(100, v.StateOf("villager.orhan").Romance);
            Assert.Null(r.Family.CanDo(r.Head, orhan, SocialAction.Propose));

            r.Family.Apply(r.Head, orhan, SocialAction.Propose);
            Assert.Equal(orhan.Id, r.Head.SpouseId);
            Assert.False(orhan.IsVillager);
            Assert.Equal(1, r.Life.Marriages);
            Assert.True(r.Life.Goals.IsDone("goal.marry1"));
        }

        [Fact]
        public void KoyluOlaylari_EsigeUlasinca_BirKezOdulVerir()
        {
            var r = NewRig();
            r.Life.Villagers.SpawnMissing();
            var orhan = r.Life.Villagers.CharacterOf("villager.orhan");
            Assert.Empty(r.Life.Villagers.PollEvents());
            r.Family.SetAffection(r.Head.Id, orhan.Id, 35);
            int before = r.House.Akce;
            var events = r.Life.Villagers.PollEvents();
            Assert.Single(events);
            Assert.Equal("orhan_1", events[0].ev.Id);
            Assert.Equal(before + 10, r.House.Akce);
            Assert.Empty(r.Life.Villagers.PollEvents());      // bir kez

            r.Family.SetAffection(r.Head.Id, orhan.Id, 100);
            Assert.Equal(2, r.Life.Villagers.PollEvents().Count);
            Assert.Equal(6, r.House.Count("item.firewood"));
        }

        [Fact]
        public void KoyluProgrami_SaateHavayaVeSenlige_Gore()
        {
            var r = NewRig();
            var def = r.Db.Villagers.Get("villager.hatice");
            var v = r.Life.Villagers;
            Assert.Equal("home:2", v.LocationFor(def, 0.1, WeatherType.Clear, false));
            Assert.Equal("work:mill", v.LocationFor(def, 0.4, WeatherType.Clear, false));
            Assert.Equal("square", v.LocationFor(def, 0.8, WeatherType.Clear, false));
            Assert.Equal("home:2", v.LocationFor(def, 0.8, WeatherType.Rain, false));    // yağmurda meydana çıkmaz
            Assert.Equal("home:2", v.LocationFor(def, 0.4, WeatherType.Storm, false));   // fırtınada evde
            Assert.Equal("square", v.LocationFor(def, 0.5, WeatherType.Clear, true));    // şenlik günü meydan
        }

        [Fact]
        public void Demirci_OlunceAletYukseltilemez()
        {
            var r = NewRig(1000);
            r.Life.Villagers.SpawnMissing();
            r.House.AddItem("item.firewood", 30);
            r.Life.AccountLevel = () => 10;
            Assert.Null(r.Life.Tools.CanUpgrade("axe"));
            r.Life.Villagers.CharacterOf("villager.orhan").Tick(1000);   // yaşlılıktan ölür
            Assert.True(r.Life.Villagers.CharacterOf("villager.orhan").IsDead);
            Assert.Contains("demirci", r.Life.Tools.CanUpgrade("axe"));
        }

        // ---------- Aletler (39) ----------

        [Fact]
        public void Alet_YukseltmeMaliyetSeviyeVeHiz()
        {
            var r = NewRig(30);
            r.Life.Villagers.SpawnMissing();
            var t = r.Life.Tools;
            r.Life.AccountLevel = () => 1;
            Assert.Contains("Hesap seviyesi 3", t.CanUpgrade("axe"));
            r.Life.AccountLevel = () => 3;
            Assert.Contains("Akçe", t.CanUpgrade("axe"));
            r.House.AddAkce(100);
            Assert.Contains("Odun", t.CanUpgrade("axe"));
            r.House.AddItem("item.firewood", 5);

            var chop = r.Db.Items.Get("item.tree_oak").Interactions.First(i => i.Tool == "axe");
            double before = r.Life.WorkSpeedFor(r.Head, chop);
            Assert.True(t.Upgrade("axe", out _));
            Assert.Equal(1, t.Tier("axe"));
            Assert.Equal(130 - 60, r.House.Akce);
            Assert.Equal(1, r.House.Count("item.firewood"));
            Assert.Equal(before * 1.2, r.Life.WorkSpeedFor(r.Head, chop), 5);
            Assert.Equal("Demir balta", t.NameOf("axe"));

            Assert.Contains("Hesap seviyesi 6", t.CanUpgrade("axe"));
            Assert.Equal(1.0f, t.SpeedFor("hoe"));           // diğer aletler etkilenmez
        }

        [Fact]
        public void Alet_SonKademedeDurur()
        {
            var r = NewRig(5000);
            r.Life.Villagers.SpawnMissing();
            r.House.AddItem("item.firewood", 100);
            for (int i = 0; i < 3; i++) Assert.True(r.Life.Tools.Upgrade("saw", out _));
            Assert.False(r.Life.Tools.Upgrade("saw", out var why));
            Assert.Contains("son kademe", why);
            Assert.Equal(1.6f, r.Life.Tools.SpeedFor("saw"), 3);
        }

        // ---------- Köy projeleri (38) ----------

        [Fact]
        public void Proje_MaliyetiOderEtkisiniUygularBirKezYapilir()
        {
            var r = NewRig(200);
            var p = r.Life.Projects;
            Assert.Contains("yetmiyor", p.CanBuild("project.village_cellar"));
            r.House.AddItem("item.plank", 8); r.House.AddItem("item.log", 12);
            Assert.True(p.Build("project.village_cellar", out _));
            Assert.Equal(100 + 40, r.House.Akce);                                        // 40 akçe "İlk Köy Projesi" ödülü
            Assert.Equal(0, r.House.Count("item.plank"));
            Assert.Equal(0.1f, r.Life.SpoilReduction(), 3);
            Assert.Equal(6, p.VillageStatus);
            Assert.False(p.Build("project.village_cellar", out var why));
            Assert.Contains("tamamlandı", why);
            Assert.True(r.Life.Goals.IsDone("goal.project1"));
            Assert.Contains(r.Life.Log.Items, e => e.Text.Contains("Köy Mahzeni"));
        }

        [Fact]
        public void Proje_SogukDirenciKarakterlereIslenir()
        {
            var r = NewRig(200);
            r.House.AddItem("item.firewood", 20); r.House.AddItem("item.log", 8);
            Assert.Equal(0f, r.Head.Modifiers.ColdResist);
            Assert.True(r.Life.Projects.Build("project.common_hearth", out _));
            Assert.Equal(0.1f, r.Head.Modifiers.ColdResist, 3);
        }

        // ---------- Hedefler (41) ----------

        [Fact]
        public void Hedef_BirKezTamamlanirOdulVerilir()
        {
            var r = NewRig();
            Assert.Empty(r.Life.Goals.Update().Where(g => g.Metric == "akce"));
            r.House.AddAkce(1200);
            var newly = r.Life.Goals.Update();
            Assert.Contains(newly, g => g.Id == "goal.akce200");
            Assert.Contains(newly, g => g.Id == "goal.akce1000");
            Assert.Empty(r.Life.Goals.Update());                       // tekrar vermez
            Assert.True(r.Head.Skills.GetXp(SkillType.Farming) >= 60 + 250 || r.Head.Skills.GetXp(SkillType.Woodcutting) >= 310
                        || SkillTypes().Any(s => r.Head.Skills.GetXp(s) >= 310));
            var goal = r.Db.Goals.Get("goal.akce200");
            Assert.Equal(goal.Target, r.Life.Goals.Progress(goal));
            Assert.Contains(r.Life.Log.Items, e => e.Text.Contains("Hedef tamamlandı"));
        }

        private static IEnumerable<SkillType> SkillTypes() => (SkillType[])Enum.GetValues(typeof(SkillType));

        [Fact]
        public void Hedef_NesilVeYasayanOlcutleri()
        {
            var r = NewRig();
            Assert.Equal(1, r.Life.Goals.Metric("generations"));
            var kid = new Character(r.Pop.NextId(), "Cocuk", new GridPos(8, 8)) { Generation = 2, BirthMotherId = r.Head.Id };
            r.Pop.Add(kid);
            Assert.Equal(2, r.Life.Goals.Metric("generations"));
            Assert.Equal(2, r.Life.Goals.Metric("living"));
            r.Life.Villagers.SpawnMissing();
            Assert.Equal(2, r.Life.Goals.Metric("living"));            // köylüler hanede sayılmaz
            Assert.Equal(0, r.Life.Goals.Metric("bilinmeyen"));
        }

        // ---------- Geri al, varyant, şablon (43) ----------

        [Fact]
        public void GeriAl_SatinAlmaYerlestirmeTasimaVeDuvar()
        {
            var r = NewRig(200);
            var b = r.Build;
            Assert.False(b.Undo().Ok);

            Assert.True(b.Buy("item.chair_wood").Ok);                                   // 190
            Assert.True(b.PlaceFromInventory("item.chair_wood", new GridPos(2, 2), 0, out var chair).Ok);
            Assert.True(b.Move(chair.Id, new GridPos(5, 5), 0).Ok);
            Assert.True(b.SetEdge(new Edge(1, 1, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
            Assert.True(b.PaintFloor(new GridPos(1, 1), FloorType.Wood).Ok);
            Assert.Equal(5, b.UndoCount);

            Assert.True(b.Undo().Ok);                                                    // zemin
            Assert.Equal(FloorType.Grass, r.World.Map.GetFloor(new GridPos(1, 1)));
            Assert.True(b.Undo().Ok);                                                    // duvar
            Assert.Equal(EdgeKind.None, r.World.Map.GetEdge(new Edge(1, 1, EdgeAxis.Horizontal)));
            Assert.True(b.Undo().Ok);                                                    // taşıma
            Assert.Equal(new GridPos(2, 2), r.World.GetItem(chair.Id).Position);
            Assert.True(b.Undo().Ok);                                                    // yerleştirme
            Assert.Equal(1, r.House.Count("item.chair_wood"));
            Assert.Empty(r.World.Items);
            Assert.True(b.Undo().Ok);                                                    // satın alma
            Assert.Equal(200, r.House.Akce);
            Assert.Equal(0, r.House.Count("item.chair_wood"));
            Assert.False(b.CanUndo);
        }

        [Fact]
        public void GeriAl_SatisiVeEnvantereAlmayiGeriGetirir()
        {
            var r = NewRig(100);
            var b = r.Build;
            b.Buy("item.chest_wood"); b.PlaceFromInventory("item.chest_wood", new GridPos(3, 3), 0, out var chest);
            b.ClearUndo();
            int akce = r.House.Akce;

            Assert.True(b.SellPlaced(chest.Id).Ok);
            Assert.Equal(akce + 9, r.House.Akce);
            Assert.True(b.Undo().Ok);
            Assert.Equal(akce, r.House.Akce);
            Assert.Single(r.World.Items);

            var placed = r.World.Items[0];
            Assert.True(b.StoreToInventory(placed.Id).Ok);
            Assert.Empty(r.World.Items);
            Assert.True(b.Undo().Ok);
            Assert.Single(r.World.Items);
            Assert.Equal(0, r.House.Count("item.chest_wood"));
        }

        [Fact]
        public void Varyant_AyniYerdeDegisirFiyatFarkiOdenirGeriAlinir()
        {
            var r = NewRig(100);
            var b = r.Build;
            var chair = r.World.PlaceItem(r.Db.Items.Get("item.chair_wood"), new GridPos(4, 4), 1);
            var variants = b.VariantsOf(chair.Def);
            Assert.Single(variants);
            Assert.Equal("item.chair_carved", variants[0].Id);

            Assert.False(b.SwapVariant(chair.Id, "item.bed_straw", out _).Ok);
            Assert.True(b.SwapVariant(chair.Id, "item.chair_carved", out var carved).Ok);
            Assert.Equal(80, r.House.Akce);                                              // 30 - 10 = 20 fark
            Assert.Equal("item.chair_carved", carved.Def.Id);
            Assert.Equal(new GridPos(4, 4), carved.Position);
            Assert.Equal(1, carved.Rotation);

            Assert.True(b.Undo().Ok);
            Assert.Equal(100, r.House.Akce);
            Assert.Equal("item.chair_wood", r.World.Items[0].Def.Id);

            // Geri dönüş ucuzdur: satış oranı kadar iade.
            var fancy = r.World.PlaceItem(r.Db.Items.Get("item.chair_carved"), new GridPos(6, 6));
            Assert.True(b.SwapVariant(fancy.Id, "item.chair_wood", out _).Ok);
            Assert.Equal(110, r.House.Akce);
        }

        [Fact]
        public void Varyant_UstundeDekorVarkenDegismez()
        {
            var r = NewRig(100);
            var table = r.World.PlaceItem(r.Db.Items.Get("item.table_wood"), new GridPos(2, 2));
            Assert.Empty(r.Build.VariantsOf(table.Def));
            var shelf = r.World.PlaceItem(r.Db.Items.Get("item.shelf_wood"), new GridPos(6, 2));
            Assert.False(r.Build.SwapVariant(shelf.Id, "item.chair_carved", out _).Ok);
        }

        [Fact]
        public void Sablon_HepsiKurulurMaliyetTamTutarTekAdimdaGeriAlinir()
        {
            foreach (var id in ContentFilesTests.LoadDb(out _).Templates.All.Select(t => t.Id))
            {
                var r = NewRig(1000);
                var service = new RoomTemplateService(r.Build, r.World, r.House, r.Db);
                var origin = new GridPos(3, 3);
                var quote = service.Quote(id, origin);
                Assert.True(quote.Ok, id + ": " + quote.Problem);
                Assert.Null(service.CanBuild(id, origin));

                var result = service.Build(id, origin, out var placed);
                Assert.True(result.Ok, id + ": " + result.Reason);
                var t = r.Db.Templates.Get(id);
                Assert.Equal(t.Items.Count, placed.Count);
                Assert.Equal(1000 - quote.Akce, r.House.Akce);                           // dökümdeki tutar birebir ödendi
                Assert.Equal(1, r.Build.UndoCount);                                      // tek geri alma adımı

                var inside = new GridPos(origin.X + t.Width / 2, origin.Y + t.Height / 2);
                var room = r.World.Map.Rooms.RoomAt(inside);
                Assert.True(room != null && room.IsIndoor, id + ": oda kapalı değil");

                Assert.True(r.Build.Undo().Ok);
                Assert.Equal(1000, r.House.Akce);
                Assert.Empty(r.World.Items);
                Assert.Equal(0, r.House.InventoryList().Sum(kv => kv.Value));
                Assert.Equal(EdgeKind.None, r.World.Map.GetEdge(new Edge(origin.X, origin.Y, EdgeAxis.Horizontal)));
            }
        }

        [Fact]
        public void Sablon_YetersizAkceArsaDisiVeDoluYerdeHicbirSeyDegismez()
        {
            var r = NewRig(10);
            var service = new RoomTemplateService(r.Build, r.World, r.House, r.Db);
            var res = service.Build("template.cottage", new GridPos(3, 3), out _);
            Assert.False(res.Ok);
            Assert.Contains("Akçe", res.Reason);
            Assert.Equal(10, r.House.Akce);
            Assert.Empty(r.World.Items);
            Assert.Equal(0, r.Build.UndoCount);

            var r2 = NewRig(500);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) r2.World.Map.SetOwned(new GridPos(x, y), false);
            var s2 = new RoomTemplateService(r2.Build, r2.World, r2.House, r2.Db);
            Assert.Equal(BuildService.NotOwned, s2.CanBuild("template.cottage", new GridPos(3, 3)));

            var r3 = NewRig(500);
            var s3 = new RoomTemplateService(r3.Build, r3.World, r3.House, r3.Db);
            Assert.Contains("dışına", s3.CanBuild("template.cottage", new GridPos(14, 14)));
            r3.World.PlaceItem(r3.Db.Items.Get("item.chest_wood"), new GridPos(3, 3));      // yatağın yeri dolu
            Assert.NotNull(s3.CanBuild("template.cottage", new GridPos(3, 3)));
        }

        [Fact]
        public void Sablon_EnvanterdekiEsyayiOnceKullanir()
        {
            var r = NewRig(1000);
            var service = new RoomTemplateService(r.Build, r.World, r.House, r.Db);
            var full = service.Quote("template.cottage", new GridPos(3, 3));
            r.House.AddItem("item.bed_straw");
            var withBed = service.Quote("template.cottage", new GridPos(3, 3));
            Assert.Equal(30, full.Akce - withBed.Akce);
            Assert.Equal(1, withBed.FromInventory);
            Assert.True(service.Build("template.cottage", new GridPos(3, 3), out _).Ok);
            Assert.Equal(1000 - withBed.Akce, r.House.Akce);
        }

        [Fact]
        public void GercekIcerik_KoyKatmaniTamVeTutarli()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            Assert.True(db.Villagers.Count >= 6);
            Assert.Contains(db.Villagers.All, v => v.Role == VillagerService.RoleBlacksmith);
            Assert.All(db.Villagers.All, v => Assert.InRange(v.Events.Count, 2, 4));
            Assert.All(db.Villagers.All, v => Assert.NotEmpty(v.Loves));
            Assert.True(db.Projects.Count >= 5);
            Assert.True(db.Goals.Count >= 15);
            Assert.All(db.Goals.All, g => Assert.Contains(g.Metric, GoalDef.Metrics));
            Assert.Equal(4, db.Templates.Count);
            Assert.Equal(24, db.Branches.Count);
        }

        // ---------- Kayıt ----------

        [Fact]
        public void KoyKaydi_AletProjeHedefKoyluSayaclarGeriGelir()
        {
            var r = NewRig(1000);
            r.Life.Villagers.SpawnMissing();
            r.House.AddItem("item.firewood", 40); r.House.AddItem("item.plank", 6); r.House.AddItem("item.log", 6); r.House.AddItem("item.honey", 1);
            r.Life.AccountLevel = () => 10;
            Assert.True(r.Life.Tools.Upgrade("hoe", out _));
            Assert.True(r.Life.Projects.Build("project.well", out _));
            r.Life.Villagers.Give(r.Head, "villager.zeynep", "item.honey", out _);
            r.Family.SetAffection(r.Head.Id, r.Life.Villagers.CharacterOf("villager.zeynep").Id, 40);
            r.Life.Villagers.PollEvents();
            r.Life.Goals.Update();
            var json = JsonSerializer.Serialize(r.Life.Export(), new JsonSerializerOptions { IncludeFields = true });
            var data = JsonSerializer.Deserialize<LifeSaveData>(json, new JsonSerializerOptions { IncludeFields = true })!;

            var r2 = NewRig(1000);
            foreach (var m in r.Pop.Members.Where(m => m.IsVillager)) r2.Pop.Add(new Character(m.Id, m.Name, m.Position) { IsVillager = true });
            r2.Life.Restore(data);
            Assert.Equal(1, r2.Life.Tools.Tier("hoe"));
            Assert.True(r2.Life.Projects.IsDone("project.well"));
            Assert.Equal(0.05f, r2.Life.Village.CropGrowth, 3);
            Assert.Equal(string.Join(",", r.Life.Goals.Done.OrderBy(x => x)), string.Join(",", r2.Life.Goals.Done.OrderBy(x => x)));
            Assert.True(r2.Life.Goals.IsDone("goal.project1"));
            var z = r2.Life.Villagers.StateOf("villager.zeynep");
            Assert.NotNull(z);
            Assert.Contains("zeynep_1", z.EventsSeen);
            Assert.Equal(r.Life.Villagers.StateOf("villager.zeynep").Romance, z.Romance);
            Assert.Equal(r.Life.Villagers.StateOf("villager.zeynep").LastGiftDay, z.LastGiftDay);
            Assert.Empty(r2.Life.Villagers.SpawnMissing().Where(c => c.Name == "Zeynep"));    // yeniden oluşmaz
        }

        [Fact]
        public void EskiKayit_YeniAlanlarOlmadanYuklenir()
        {
            var r = NewRig();
            r.Life.Restore(new LifeSaveData());      // yeni alanlar null/sıfır
            Assert.Equal(0, r.Life.Tools.Tier("axe"));
            Assert.Equal(0, r.Life.Projects.CompletedCount);
            Assert.Equal(0, r.Life.Marriages);
            Assert.Empty(r.Life.Villagers.States);
        }
    }
}
