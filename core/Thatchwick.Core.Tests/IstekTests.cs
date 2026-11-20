using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;
using Thatchwick.Core.Life.Requests;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 46: istek sistemi (rica, sipariş, mevsim defteri, çağrı, hikâye, vasiyet, adak, hane hafızası, rakip haneler).
    public class IstekTests
    {
        private sealed class Kit
        {
            public ContentDatabase Db; public Population Pop; public Household House; public LifeSystems Life; public FamilyService Family;
            public Character Ali; public double Now;
            public RequestService Req => Life.Requests;
        }

        private static Kit Make(int seed = 3)
        {
            var k = new Kit { Db = ContentFilesTests.LoadDb(out _), Pop = new Population(), House = new Household(50) };
            var w = new WorldState(new GridMap(8, 8));
            k.Ali = new Character(1, "Ali", new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            k.Ali.SetAge(40);
            k.Pop.Add(k.Ali);
            k.Life = new LifeSystems(k.Db, w, k.House, k.Pop, new GameClock(), seed);
            k.Family = new FamilyService(k.Pop, k.House, k.Db);
            k.Life.Attach(k.Family);
            k.Life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            k.Life.Houses.EnsureHouses();
            k.Req.Now = () => k.Now;
            return k;
        }

        private static Character Villager(Kit k, string defId) => k.Life.Villagers.CharacterOf(defId);

        private static void Level(Character c, SkillType s, int level) => c.Skills.RestoreXp(s, SkillRules.XpForLevel(level));

        [Fact]
        public void Katalog_ButunEsyalarIcerikte_VeAsgariSablonSayisi()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var missing = RequestCatalog.AllItemIds().Where(id => !db.Items.Contains(id)).Distinct().ToList();
            Assert.Empty(missing);
            Assert.True(RequestCatalog.TemplateCount() >= 120, RequestCatalog.TemplateCount().ToString());   // belge 46 §15.4 asgari
            Assert.Equal(28, RequestCatalog.Seasonal.Length);
            foreach (Season s in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
                Assert.Equal(7, RequestCatalog.Seasonal.Count(t => t.Season == s));
            Assert.Equal(13, RequestCatalog.NamedStories.Count);
            foreach (var kv in RequestCatalog.NamedStories) Assert.True(db.Villagers.Contains(kv.Key), kv.Key);
        }

        /// <summary>Pano + tellal (karar 20.11.2026): pano bütün ilanları tutar, tellal sabah yalnızca yenileri bağırır.</summary>
        [Fact]
        public void Tellal_SabahYeniIlanlariDuyurur_DuyanOkumusSayilir()
        {
            var k = Make();
            var notes = new List<string>();
            k.Req.Notify = (t, id) => notes.Add(t.Key);
            k.Now = 5.0;
            var r1 = k.Req.GenerateFavors(roll: 0.0);
            Assert.NotNull(r1);
            Assert.False(r1.Read);
            Assert.DoesNotContain("core.req.n.posted_unread", notes);        // sabah turundan önce: tellal duyuracak
            k.Now = 5.2;
            Assert.False(k.Req.CrierDue);                                    // saat daha 04:48
            k.Now = 5.0 + RequestService.CrierHour / 24.0 + 0.01;
            Assert.True(k.Req.CrierDue);

            var far = k.Req.Cry(heard: false);                               // haneden kimse kasabada değil
            Assert.Single(far.Fresh);
            Assert.Equal(1, far.Unread);
            Assert.False(r1.Read);
            Assert.False(k.Req.CrierDue);                                    // günde bir tur

            k.Now = 5.9;
            var r2 = k.Req.GenerateFavors(roll: 0.0);                        // turdan sonra asılan: kâğıt bildirimi gelir
            Assert.NotNull(r2);
            Assert.Contains("core.req.n.posted_unread", notes);
            k.Now = 6.4;
            var near = k.Req.Cry(heard: true);                               // bugün biri meydanda
            Assert.Equal(new[] { r2.Id }, near.Fresh.Select(r => r.Id).ToArray());   // dünkü ilan yeniden bağırılmaz
            Assert.Equal(1, near.NewlyRead);
            Assert.True(r2.Read);
            Assert.False(r1.Read);                                           // kaçırılan ilan panoda bekler
            Assert.Equal(1, k.Req.ReadBoard());

            var copy = Make();
            copy.Req.Import(k.Req.Export());
            copy.Now = 6.5;
            Assert.False(copy.Req.CrierDue);                                 // kayıttan dönünce aynı gün yeniden bağırmaz
        }

        [Fact]
        public void Tellal_HaneUyesiKasabadaysaDuyar()
        {
            foreach (bool inTown in new[] { true, false })
            {
                var k = new Kit { Db = ContentFilesTests.LoadDb(out _), Pop = new Population(), House = new Household(50) };
                var w = new WorldState(new GridMap(Regions.WorldWidth, Regions.WorldHeight));
                var spot = inTown ? new GridPos(Regions.Town.X + 20, Regions.Town.Y + 20) : new GridPos(4, 4);
                k.Ali = new Character(1, "Ali", spot, true) { CanStarve = false, MoodEnabled = true };
                k.Ali.SetAge(40);
                k.Pop.Add(k.Ali);
                k.Life = new LifeSystems(k.Db, w, k.House, k.Pop, new GameClock(), 3);
                k.Family = new FamilyService(k.Pop, k.House, k.Db);
                k.Life.Attach(k.Family);
                k.Life.Villagers.SpawnMissing(_ => new GridPos(Regions.Town.X + 3, Regions.Town.Y + 3));
                k.Life.Houses.EnsureHouses();
                k.Req.Now = () => k.Now;
                k.Now = 2.0;
                var r = k.Req.GenerateFavors(roll: 0.0);
                Assert.NotNull(r);
                RequestService.CrierCall call = null;
                k.Life.CrierCalled += c => call = c;
                k.Now = 2.5;
                k.Life.Tick(0.0001);
                Assert.NotNull(call);
                Assert.Equal(inTown, call.Heard);
                Assert.Equal(inTown, r.Read);
            }
        }

        [Fact]
        public void Rica_PanodanOkunur_KabulEdilir_UlaklaTeslim()
        {
            var k = Make();
            var r = k.Req.GenerateFavors(roll: 0.0);
            Assert.NotNull(r);
            Assert.Equal(RequestKind.Favor, r.Kind);
            Assert.False(r.Read);                                            // okuryazar yok: panoya gidilmeli
            Assert.False(k.Req.Accept(r.Id, out var why));
            Assert.Equal("core.req.err.unread", why.Key);
            Assert.Equal(1, k.Life.ReadBoard(k.Ali));
            var o = r.Objectives[0];
            if (o.Kind == ObjectiveKind.Deliver) k.House.AddItem(o.ItemId, o.Count);
            else k.Req.OnVisit(k.Ali, o.Region);
            Assert.True(k.Req.Accept(r.Id, out why), why.ToString());
            int akce = k.House.Akce;
            var giver = k.Pop.Find(r.GiverId);
            k.Req.Near = (a, b) => false;                                    // uzakta
            if (o.Kind == ObjectiveKind.Deliver)
            {
                Assert.False(k.Req.Deliver(r.Id, k.Ali, courier: false, out why));
                Assert.Equal("core.req.err.go_to", why.Key);
                Assert.True(k.Req.Deliver(r.Id, k.Ali, courier: true, out why), why.ToString());
                Assert.Equal(akce + r.Reward.Akce - RequestService.CourierFee, k.House.Akce);
            }
            Assert.Equal(RequestState.Done, r.State);
            Assert.Equal(RequestService.FavorAffinity, k.Family.Affection(k.Ali.Id, giver.Id));
            Assert.Equal(HouseMemory.Favor, k.Req.Memory.Of(giver));
        }

        [Fact]
        public void Okuryazar_Hane_PanoyuUzaktanOkur()
        {
            var k = Make();
            k.Ali.GainSkillXp(SkillType.Literacy, 5);
            Assert.True(k.Req.Literate);
            var r = k.Req.GenerateFavors(roll: 0.0);
            Assert.True(r.Read);
        }

        [Fact]
        public void Rica_KoyluSevmedigiSeyiIstemez_EnFazlaUcAskida()
        {
            var k = Make();
            for (int i = 0; i < 10; i++) k.Req.GenerateFavors(roll: 0.0);
            Assert.Equal(RequestService.MaxFavorsPosted, k.Req.CountOpen(RequestKind.Favor, RequestState.Posted));
            foreach (var r in k.Req.All)
            {
                var def = k.Life.Villagers.DefOf(k.Pop.Find(r.GiverId));
                var item = r.Objectives[0].ItemId;
                if (def != null && item != null) Assert.NotEqual(GiftTaste.Dislike, VillagerService.TasteFor(def, k.Db.Items.Get(item)));
            }
        }

        [Fact]
        public void Siparis_BeceriKapisi_Zorlayici_VeUstaSiparisi()
        {
            var k = Make();
            Level(k.Ali, SkillType.Smithing, 10);
            var orders = k.Req.GenerateOrders();
            Assert.NotEmpty(orders);
            Assert.True(orders.Count(o => o.Stretch) <= 1);
            foreach (var o in orders)
            {
                Assert.True(o.Skill.HasValue);
                if (!o.Stretch) Assert.True(o.Level <= k.Req.Best(o.Skill.Value));
                else Assert.Equal(k.Req.Best(o.Skill.Value) + 1, o.Level);
                Assert.Equal(-2, o.Penalty);
            }
            Assert.Contains(orders, o => o.Master && o.Skill == SkillType.Smithing);
        }

        [Fact]
        public void Siparis_KabulEdilipYetismezse_ItibarEksi_HafizaEksi()
        {
            var k = Make();
            Level(k.Ali, SkillType.Smithing, 5);
            var o = k.Req.GenerateOrders().First(x => !x.Stretch);
            k.Req.ReadBoard();
            Assert.True(k.Req.Accept(o.Id, out var why), why.ToString());
            int rep = k.Life.Upbringing.Reputation.Points;
            k.Now += RequestService.OrderDays + 0.1;
            k.Req.Tick();
            Assert.Equal(RequestState.Failed, o.State);
            Assert.Equal(rep - 2, k.Life.Upbringing.Reputation.Points);
            Assert.Equal(HouseMemory.FailedOrder, k.Req.Memory.Of(k.Pop.Find(o.GiverId)));
        }

        [Fact]
        public void Siparis_BirGunAlinmazsa_RakipHaneyeGider_SiralamadaGorunur()
        {
            var k = Make();
            Level(k.Ali, SkillType.Carpentry, 5);
            var orders = k.Req.GenerateOrders();
            Assert.NotEmpty(orders);
            k.Now += 1.01;
            k.Req.OnNewDay();
            Assert.All(orders, o => Assert.Equal(RequestState.Rival, o.State));
            var rank = k.Req.Ranking("Ali hanesi");
            Assert.Contains(rank, x => !x.IsPlayer && x.Points > 0);
            Assert.Equal(rank.OrderByDescending(x => x.Points).First().Points, rank[0].Points);
        }

        [Fact]
        public void MevsimDefteri_Pencere_KoyIlerler_BesIsteTakimParcasi()
        {
            var k = Make();
            k.Now = 0.3;                                         // ilkbahar 1. gün
            k.Req.OnNewDay();
            var book = k.Req.Open(RequestKind.Seasonal).ToList();
            Assert.Equal(7, book.Count);
            var plough = book.First(r => r.TemplateId == "mev.sp.plough");
            var shearing = book.First(r => r.TemplateId == "mev.sp.shearing");
            k.Req.OnWork(k.Ali, SkillType.Farming, "tile:till");
            Assert.Equal(1, plough.Objectives[0].Progress);
            Assert.False(k.Req.InWindow(shearing));               // 8-10. günler
            k.House.AddItem("item.wool", 5);
            Assert.False(k.Req.Deliver(shearing.Id, k.Ali, false, out _));
            Assert.True(k.Req.VillageProgress(plough) > 0);
            // Beş iş bitsin.
            int done = 0;
            foreach (var r in book)
            {
                if (done >= 5) break;
                k.Now = r.DayFrom - 1 + 0.4;
                var o = r.Objectives[0];
                if (o.Kind == ObjectiveKind.Deliver) { k.House.AddItem(o.ItemId, o.Count); k.Req.Deliver(r.Id, k.Ali, false, out _); }
                else for (int i = 0; i < o.Count; i++) k.Req.OnWork(k.Ali, o.Skill, "x");
                if (r.State == RequestState.Done) done++;
            }
            Assert.True(done >= 5);
            k.Now = 10.2;                                        // yaz
            k.Req.OnNewDay();
            Assert.Equal(1, k.House.Count("item.defter_spring_ribbon"));
            Assert.True(k.Ali.Mood.Has(RequestService.BountifulMood));
            Assert.Equal(7, k.Req.Open(RequestKind.Seasonal).Count());
            Assert.All(book, r => Assert.False(r.IsOpen));
        }

        [Fact]
        public void Cagri_AkceYolu_HemenBiter_YarimItibar_BeceriYoluSureliGorev()
        {
            var k = Make();
            var c = k.Req.GenerateCall("cagri.storm_roof");
            Assert.NotNull(c);
            int paid = c.Paths.FindIndex(p => p.Paid);
            Assert.True(k.Req.PathBlocker(c, paid, k.Ali).IsEmpty);
            int rep = k.Life.Upbringing.Reputation.Points;
            Assert.True(k.Req.ChoosePath(c.Id, paid, k.Ali, out var msg), msg.ToString());
            Assert.Equal(RequestState.Done, c.State);
            Assert.Equal(rep + RequestService.CallRep / 2, k.Life.Upbringing.Reputation.Points);

            var c2 = k.Req.GenerateCall("cagri.storm_roof");
            int carp = c2.Paths.FindIndex(p => p.Skill == SkillType.Carpentry);
            Assert.Equal("core.req.err.skill_needed", k.Req.PathBlocker(c2, carp, k.Ali).Key);
            Level(k.Ali, SkillType.Carpentry, 3);
            k.House.AddItem("item.plank", 4);
            k.Now = 0.3;
            Assert.True(k.Req.ChoosePath(c2.Id, carp, k.Ali, out msg), msg.ToString());
            Assert.True(k.Ali.Away);
            Assert.Equal(0, k.House.Count("item.plank"));
            for (int i = 0; i < 40 && c2.IsOpen; i++) { k.Now += 0.25 / 24; k.Life.Duties.Tick(0.25 / 24); }
            Assert.Equal(RequestState.Done, c2.State);
            Assert.False(k.Ali.Away);
        }

        [Fact]
        public void Hikaye_YakinlikEsiginde_Acilir_AdimAdimIlerler()
        {
            var k = Make();
            var orhan = Villager(k, "villager.orhan");
            k.Req.OnNewDay();
            Assert.DoesNotContain(k.Req.All, r => r.Kind == RequestKind.Story && r.GiverId == orhan.Id);
            k.Family.SetAffection(k.Ali.Id, orhan.Id, 30);
            k.Req.OnNewDay();
            var s = k.Req.All.First(r => r.Kind == RequestKind.Story && r.GiverId == orhan.Id);
            Assert.Equal("villager.orhan", s.StoryKey);
            k.House.AddItem("item.charcoal", 4);
            Assert.True(k.Req.Deliver(s.Id, k.Ali, false, out var msg), msg.ToString());
            Assert.Equal(RequestState.Done, s.State);
            Assert.Equal(1, k.Req.StoryStepOf("villager.orhan"));
            Assert.Equal(50, k.Family.Affection(k.Ali.Id, orhan.Id));      // 30 + adım ödülü 20
            k.Req.OnNewDay();
            Assert.Equal(0, k.Req.All.Count(r => r.Kind == RequestKind.Story && r.GiverId == orhan.Id && r.IsOpen));   // ikinci adım 60'ta
            k.Family.SetAffection(k.Ali.Id, orhan.Id, 60);
            k.Req.OnNewDay();
            var s2 = k.Req.All.Single(r => r.Kind == RequestKind.Story && r.GiverId == orhan.Id && r.IsOpen);
            Assert.Equal(ObjectiveKind.Skill, s2.Objectives[0].Kind);
            Level(k.Ali, SkillType.Smithing, 3);
            k.Now += 0.1;
            k.Req.Tick();
            Assert.Equal(RequestState.Done, s2.State);
        }

        [Fact]
        public void Vasiyet_YakinKoyluOlunce_Kalir_SuresizdirVeHatiraVerir()
        {
            var k = Make();
            var hatice = Villager(k, "villager.hatice");
            k.Family.SetAffection(k.Ali.Id, hatice.Id, 70);
            var b = k.Req.OnDied(hatice);
            Assert.NotNull(b);
            Assert.Equal(RequestKind.Bequest, b.Kind);
            Assert.False(b.HasDeadline);
            Assert.Equal("vas.miller", b.TemplateId);
            k.House.AddItem("item.bread", 6);
            Assert.True(k.Req.Deliver(b.Id, k.Ali, false, out var msg), msg.ToString());
            Assert.Equal(RequestState.Done, b.State);
            Assert.Equal(1, k.House.Count("item.elder_tin_cup"));
            // Uzak köylü vasiyet bırakmaz.
            var kerem = Villager(k, "villager.kerem");
            Assert.Null(k.Req.OnDied(kerem));
        }

        [Fact]
        public void Adak_Asamalar_KusaklarBoyu_SoyMuhruVerir()
        {
            var k = Make();
            var child = new Character(2, "Can", new GridPos(0, 0), true) { CanStarve = false };
            child.SetAge(child.AgeConfig.ChildStartDay + 1);
            k.Pop.Add(child);
            Assert.False(k.Req.CanVow("adak.fountain", child).IsEmpty);
            var v = k.Req.Vow("adak.fountain", k.Ali, out var msg);
            Assert.NotNull(v);
            Assert.Contains(k.Ali.Chronicle, n => n.StartsWith("@@core.chronicle.vow_made"));   // kroniğe anahtarlı yazılır (SavedText)
            k.House.AddItem("item.stone", 10);
            k.House.AddItem("item.limestone", 6);
            Assert.True(k.Req.Deliver(v.Id, k.Ali, false, out msg), msg.ToString());
            Assert.Equal(1, v.Stage);
            Level(k.Ali, SkillType.Masonry, 6);
            k.Now += 0.1;
            k.Req.Tick();
            Assert.Equal(2, v.Stage);
            int seals = k.Life.Legacy.Seals;
            k.House.AddAkce(40);
            Assert.True(k.Req.Deliver(v.Id, k.Ali, false, out msg), msg.ToString());
            Assert.Equal(RequestState.Done, v.State);
            Assert.Equal(seals + 2, k.Life.Legacy.Seals);
            Assert.DoesNotContain(k.Req.AvailableVows(), t => t.Id == "adak.fountain");
        }

        [Fact]
        public void Ata_EsyaTeslimi_SureliGoreveCevrilir_DonunceTeslimEdilir()
        {
            var k = Make();
            var ayse = Villager(k, "villager.ayse");
            var t = RequestCatalog.Favors.First(f => f.Id == "rica.potter.clay");
            var r = k.Req.PostFavor(ayse, t);
            k.Req.ReadBoard();
            k.Now = 0.3;
            k.House.AddItem("item.bread", 3);
            Assert.True(k.Req.Assign(r.Id, k.Ali, out var msg), msg.ToString());
            Assert.True(k.Ali.Away, msg.ToString());
            var duty = k.Life.Duties.Find(k.Ali.Id);
            Assert.Equal("item.clay", duty.Order.TargetItem);
            Assert.Equal(r.Id, duty.Order.RequestId);
            k.House.AddItem("item.clay", 6);   // eve başka yoldan kil gelmiş olsun: dönüşte teslim edilir
            k.Life.Duties.Recall(k.Ali.Id);
            Assert.Equal(RequestState.Done, r.State);
        }

        [Fact]
        public void HaneHafizasi_YeniKusagaYuzdeYirmiBes()
        {
            var k = Make();
            var murat = Villager(k, "villager.murat");
            k.Req.Memory.Add(murat, 40, "test");
            Assert.Equal(10, k.Req.Memory.InitialAffection(k.Req.Memory.KeyOf(murat)));
            var baby = new Character(900, "Bebek", new GridPos(0, 0), true);
            k.Pop.Add(baby);
            k.Req.OnBabyBorn(baby, k.Ali);   // oyuncunun yeni kuşağı
            Assert.Equal(10, k.Family.Affection(baby.Id, murat.Id));
        }

        [Fact]
        public void Kayit_GidipGelir()
        {
            var k = Make();
            Level(k.Ali, SkillType.Smithing, 5);
            k.Req.GenerateFavors(roll: 0.0);
            k.Req.GenerateOrders();
            k.Req.GenerateCall();
            k.Req.Vow("adak.bridge", k.Ali, out _);
            k.Req.Memory.Add(Villager(k, "villager.orhan"), 12, "x");
            var save = k.Req.Export();
            var k2 = Make();
            k2.Req.Import(save);
            Assert.Equal(k.Req.All.Count, k2.Req.All.Count);
            for (int i = 0; i < k.Req.All.Count; i++)
            {
                var a = k.Req.All[i]; var b = k2.Req.All[i];
                Assert.Equal(a.Id, b.Id); Assert.Equal(a.Kind, b.Kind); Assert.Equal(a.State, b.State); Assert.Equal(LocCodec.Encode(a.Title), LocCodec.Encode(b.Title));
                Assert.Equal(a.Objectives.Count, b.Objectives.Count); Assert.Equal(a.Stages.Count, b.Stages.Count); Assert.Equal(a.Paths.Count, b.Paths.Count);
                Assert.Equal(a.HasDeadline, b.HasDeadline); Assert.Equal(a.Reward.Akce, b.Reward.Akce);
            }
            Assert.Equal(12, k2.Req.Memory.Of(Villager(k2, "villager.orhan")));
            Assert.NotNull(k2.Life.Export().requests);
        }
    }
}
