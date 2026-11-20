using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Relations;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>İlişki sistemi (Project belgeleri 67, 67a-d).</summary>
    public class IliskiSistemiTests
    {
        private sealed class Rig
        {
            public Population Pop;
            public GameClock Clock;
            public LifeSystems Life;
            public FamilyService Family;
            public Household House;
            public RelationService Rel => Life.Relations;
        }

        private static Rig NewRig()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var world = new WorldState(new GridMap(20, 20));
            var house = new Household(50);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, world, house, pop, clock);
            var family = new FamilyService(pop, house, db, seed: 3);
            life.Attach(family);
            return new Rig { Pop = pop, Clock = clock, Life = life, Family = family, House = house };
        }

        private static Character Add(Rig r, string name, Sex sex, double age, bool villager = false, int x = 5, int y = 5)
        {
            var c = new Character(r.Pop.NextId(), name, new GridPos(x, y)) { Sex = sex, IsVillager = villager, CanStarve = false, MoodEnabled = true };
            c.SetAge(age);
            r.Pop.Add(c);
            return c;
        }

        [Fact]
        public void Asamalar_ModernAdlarlaVeEksiyle()
        {
            Assert.Equal(FriendStage.Enemy, RelationRules.StageOf(-80));
            Assert.Equal(FriendStage.Tense, RelationRules.StageOf(-30));
            Assert.Equal(FriendStage.Acquaintance, RelationRules.StageOf(0));
            Assert.Equal(FriendStage.Friend, RelationRules.StageOf(20));
            Assert.Equal(FriendStage.CloseFriend, RelationRules.StageOf(55));
            Assert.Equal(FriendStage.BestFriend, RelationRules.StageOf(80));
            Assert.Equal(RomanceStage.LoveOfLife, RelationRules.RomanceStageOf(95));
            Assert.Equal("rel.stage.f.best_friend", RelationRules.StageText(FriendStage.BestFriend).Key);
        }

        [Fact]
        public void Sohbet_GunlukAzalanGetiri_KankaYavasGelir()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Veli", Sex.Male, 40, x: 6);
            for (int i = 0; i < 4; i++) r.Family.Apply(a, b, SocialAction.Chat);
            Assert.Equal(25, r.Family.Affection(a.Id, b.Id));   // 20 + 20×0,5 (ikinci sohbet)×0,5 (Arkadaş aşaması) + 0 + 0
            // Her 2 günde bir sohbetle Kanka (80) yaklaşık 2 mevsim (20 gün) civarında.
            int days = 0;
            for (int d = 1; d < 60 && r.Family.Affection(a.Id, b.Id) < 80; d += 2) { r.Clock.Restore(d + 0.3, 1); r.Family.Apply(a, b, SocialAction.Chat); days = d; }
            Assert.InRange(days, 10, 40);
        }

        [Fact]
        public void Azalma_AsamaTabanindaDurur_AileAzalmaz()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Veli", Sex.Male, 40);
            var mom = Add(r, "Anne", Sex.Female, 60); var son = Add(r, "Oğul", Sex.Male, 35);
            son.BirthMotherId = mom.Id;
            r.Family.SetAffection(a.Id, b.Id, 60);
            r.Family.SetAffection(mom.Id, son.Id, 60);
            r.Rel.Met(a.Id, b.Id); r.Rel.Met(mom.Id, son.Id);
            for (int d = 1; d < 80; d++) { r.Clock.Restore(d, 1); r.Rel.OnNewDay(d); }
            Assert.Equal(50, r.Family.Affection(a.Id, b.Id));   // Yakın arkadaş tabanında durur
            Assert.Equal(60, r.Family.Affection(mom.Id, son.Id));
            Assert.True(r.Rel.HasFeeling(a.Id, b.Id, FeelingKind.Misses));
        }

        [Fact]
        public void Kus_KonusmazlarVeBarisinca_Duzelir()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Veli", Sex.Male, 40, x: 6);
            r.Family.SetAffection(a.Id, b.Id, -40);
            Assert.Null(r.Family.CanDo(a, b, SocialAction.Sulk));
            r.Family.Apply(a, b, SocialAction.Sulk);
            Assert.True(r.Rel.IsSulking(a.Id, b.Id));
            Assert.Contains("rel.why.sulking", r.Family.CanDo(a, b, SocialAction.Chat));
            r.Life.Relations.IsFestivalDay = () => true;   // şenlikte barışma kesin (bayramlaşma)
            r.Family.Apply(b, a, SocialAction.MakeUp);
            Assert.False(r.Rel.IsSulking(a.Id, b.Id));
            Assert.True(r.Family.Affection(a.Id, b.Id) > -20);
        }

        [Fact]
        public void Kavga_MorarmisVeHis_YaraSistemineBaglanmaz()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Veli", Sex.Male, 40, x: 6);
            var kid = Add(r, "Çocuk", Sex.Female, 15, x: 7);
            r.Family.SetAffection(a.Id, b.Id, -40);
            Assert.Null(r.Family.CanDo(a, b, SocialAction.Fight));
            Character winner = null, loser = null;
            r.Rel.FightEnded += (w, l, scuffle) => { winner = w; loser = l; };
            r.Family.Apply(a, b, SocialAction.Fight);
            Assert.NotNull(loser);
            Assert.True(loser.Mood.Has(RelationMoods.Bruised));
            Assert.Null(loser.Injury);
            Assert.True(r.Rel.HasFeeling(loser.Id, winner.Id, FeelingKind.Grudge) || r.Rel.HasFeeling(loser.Id, winner.Id, FeelingKind.Ashamed));
            Assert.True(kid.Mood.Has(RelationMoods.ScaredFight));
            Assert.Equal(-70, r.Family.Affection(a.Id, b.Id));
        }

        [Fact]
        public void Kavga_Yasli_Cocuk_Hamile_Esler()
        {
            var r = NewRig();
            var elder = Add(r, "Dede", Sex.Male, 120); var man = Add(r, "Ali", Sex.Male, 40);
            var wife = Add(r, "Ayşe", Sex.Female, 40); man.SpouseId = wife.Id; wife.SpouseId = man.Id;
            r.Family.SetAffection(elder.Id, man.Id, -50);
            Assert.Contains("rel.why.elder_cane", r.Family.CanDo(elder, man, SocialAction.Fight));
            Assert.Null(r.Family.CanDo(elder, man, SocialAction.CaneThreat));
            r.Family.SetAffection(man.Id, wife.Id, -50);
            Assert.Contains("rel.why.spouse_fight", r.Family.CanDo(man, wife, SocialAction.Fight));   // eşler yalnız aldatma gibi durumlarda
            r.Rel.ReportTrigger(SpouseTrigger.Infidelity, man);
            Assert.Null(r.Family.CanDo(wife, man, SocialAction.Fight));
            Assert.True(r.Rel.HasFeeling(wife.Id, man.Id, FeelingKind.Betrayed));
        }

        [Fact]
        public void Oops_SakalliOpucuk()
        {
            var r = NewRig();
            var man = Add(r, "Ali", Sex.Male, 40); var woman = Add(r, "Ayşe", Sex.Female, 38, x: 6);
            man.Appearance.StubbleLevel = 2;
            r.Rel.SetPrefs(woman, new AttractionPrefs { Like1 = AttractionTag.Musician, Like2 = AttractionTag.GoodCook, Dislike = AttractionTag.Bearded });
            r.Rel.Oops(woman, man, SocialAction.Kiss);
            Assert.True(woman.Mood.Has(RelationMoods.Oops));   // hoşlanmayan: her zaman
        }

        [Fact]
        public void BilgiYayildikca_ItibarEtkiEder_SoylentiCurur()
        {
            var r = NewRig();
            var head = Add(r, "Aylin", Sex.Female, 40);
            var villagers = Enumerable.Range(0, 6).Select(i => Add(r, "K" + i, Sex.Male, 40, villager: true, x: 15, y: 15)).ToList();
            var k = r.Life.Knowledge;
            int before = r.Life.Upbringing.Reputation.Points;
            var f = k.Record(FactKind.Fight, head, villagers[0], RepFacet.Peace, -6, new[] { villagers[0] });
            int afterOne = r.Life.Upbringing.Reputation.Points;
            Assert.True(afterOne > before - 6);                      // bir kişi biliyor: tam etki yok
            k.Learn(villagers[1], f, villagers[0]);
            k.Learn(villagers[2], f, villagers[0]);
            Assert.Equal(before - 6, r.Life.Upbringing.Reputation.Points);   // köyün yarısı biliyor: tam etki
            Assert.True(r.Life.Upbringing.Reputation.Facet((int)RepFacet.Peace) < 0);

            // Rakip hane hakkında söylenti: doğruyu bilen (hanenin kendisi) çürütür, yalancı haberi doğar.
            r.Life.Houses.EnsureHouses();
            var house = r.Life.Houses.HouseOf(villagers[3]);
            Assert.True(k.PrepareRumor(head, house.Id, RepFacet.Craft));
            k.SpreadPrepared(head, villagers[4]);
            var rumor = k.Facts.First(x => x.Kind == FactKind.Rumor);
            k.Learn(villagers[3], rumor, villagers[4]);
            Assert.True(rumor.Debunked);
            Assert.Contains(k.Facts, x => x.Kind == FactKind.Liar && x.Subject == head.Id);
        }

        [Fact]
        public void AileKusluguAnlatilarakGecer_AnlatilmazsaSoner()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var kid = Add(r, "Can", Sex.Male, 15);
            kid.BirthFatherId = a.Id;
            var v = Add(r, "Mustafa", Sex.Male, 40, villager: true); var vKid = Add(r, "Halil", Sex.Male, 15, villager: true);
            vKid.BirthFatherId = v.Id;
            r.Life.Houses.EnsureHouses();
            r.Life.Knowledge.RecordHouseLink(FactKind.HouseFeud, a, v);
            // Çocuklara kimse anlatmadı: sıfırdan başlarlar.
            r.Rel.Introduce(kid, vKid);
            Assert.Equal(0, r.Family.Affection(kid.Id, vKid.Id));
            var kid2 = Add(r, "Ece", Sex.Female, 15); kid2.BirthFatherId = a.Id;
            r.Life.Knowledge.TellFamilyLore(a, kid2);   // masalda anlatıldı
            r.Rel.Introduce(kid2, vKid);
            Assert.True(r.Family.Affection(kid2.Id, vKid.Id) < 0);
        }

        [Fact]
        public void Muhabbet_HalkaKurulurNpcKonusur()
        {
            var r = NewRig();
            var people = Enumerable.Range(0, 4).Select(i => Add(r, "K" + i, i % 2 == 0 ? Sex.Male : Sex.Female, 40, villager: true, x: 5 + i % 2, y: 5 + i / 2)).ToList();
            r.Life.Chats.IsGathering = _ => true;
            int lines = 0;
            r.Life.Chats.Bubble = (c, t) => lines++;
            r.Life.Chats.Form(0);
            var circle = r.Life.Chats.CircleOf(people[0].Id);
            Assert.NotNull(circle);
            Assert.True(circle.Members.Count >= 3);
            for (double t = 0; t < 0.05; t += 0.002) r.Life.Chats.Tick(t);
            Assert.True(lines > 3);
            Assert.True(r.Family.Affection(people[0].Id, people[1].Id) > 0);
        }

        [Fact]
        public void Heyet_ToplanirOyVerirImeceCagirir()
        {
            var r = NewRig();
            r.Life.Villagers.SpawnMissing(_ => new GridPos(3, 3));
            r.Life.Houses.EnsureHouses();
            r.Clock.Restore(10.5, 1);
            r.Life.Council.Meet(10.5);
            Assert.True(r.Life.Council.Members.Count > 0);
            Assert.True(r.Life.Council.ImeceDay >= 11);
        }

        [Fact]
        public void Ihanet_AyrilikKarari_BabaEvineDoner()
        {
            var r = NewRig();
            var husband = Add(r, "Ali", Sex.Male, 40);
            var bride = Add(r, "Elif", Sex.Female, 38);
            var her = Add(r, "Hatice", Sex.Female, 60, villager: true); bride.BirthMotherId = her.Id;
            r.Life.Houses.EnsureHouses();
            husband.SpouseId = bride.Id; bride.SpouseId = husband.Id;
            r.Rel.AddWarmth(husband.Id, bride.Id, -100);
            r.Rel.ReportTrigger(SpouseTrigger.Infidelity, bride);   // gelin aldattı; Ali'ye ihanet
            r.Rel.ReportTrigger(SpouseTrigger.Infidelity, bride);
            r.Rel.OnNewDay(1);
            var d = r.Rel.Decisions.Single(x => x.Kind == DecisionKind.Separation);
            Assert.True(r.Rel.Decide(d.Id, 1));
            Assert.Null(husband.SpouseId);
            Assert.True(bride.IsVillager);                          // soy hanesine sonradan gelen gider
            Assert.Equal(CoupleStatus.ExSpouse, r.Rel.Status(husband.Id, bride.Id));
        }

        [Fact]
        public void Yildonumu_UnutulursaTetik()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Ayşe", Sex.Female, 40);
            a.SpouseId = b.Id; b.SpouseId = a.Id;
            r.Rel.OnNewDay(0);
            int year = r.Rel.DaysPerYear();
            for (int d = 1; d <= year + 3; d++) { r.Clock.Restore(d, 1); r.Rel.OnNewDay(d); }
            Assert.True(a.Mood.Has(RelationMoods.ForgotAnniv) || b.Mood.Has(RelationMoods.ForgotAnniv));
            Assert.True(r.Rel.Issues.Any());
        }

        [Fact]
        public void AileTuru_YalnizBaslangictaSecilir()
        {
            var r = NewRig();
            var mom = Add(r, "Anne", Sex.Female, 50); var son = Add(r, "Oğul", Sex.Male, 20); son.BirthMotherId = mom.Id;
            Assert.True(r.Rel.ChooseDynamic(mom, son, FamilyDynamic.Strict));
            r.Clock.Restore(2, 1);
            Assert.NotNull(r.Rel.CanChooseDynamic(mom, son));
        }

        [Fact]
        public void Kayit_GidipGelir()
        {
            var r = NewRig();
            var a = Add(r, "Ali", Sex.Male, 40); var b = Add(r, "Ayşe", Sex.Female, 38);
            r.Rel.SetRomance(a.Id, b.Id, 45);
            r.Rel.AddFeeling(a.Id, b.Id, FeelingKind.Admires);
            r.Life.Knowledge.Record(FactKind.Kindness, a, b, RepFacet.Generous, 2, null);
            var save = r.Rel.Export();
            var r2 = NewRig();
            Add(r2, "Ali", Sex.Male, 40); Add(r2, "Ayşe", Sex.Female, 38);
            r2.Rel.Import(save);
            Assert.Equal(45, r2.Rel.Romance(a.Id, b.Id));
            Assert.True(r2.Rel.HasFeeling(a.Id, b.Id, FeelingKind.Admires));
            Assert.Contains(r2.Life.Knowledge.Facts, f => f.Kind == FactKind.Kindness);
        }

        [Fact]
        public void Replikler_SayilarDosyaylaTutar()
        {
            int total = 0;
            foreach (var g in RelationLines.Groups) total += RelationLines.Count(g);
            Assert.True(total >= 300);
        }

        [Fact]
        public void Sahne_DamatGelinKarsilikli_YerlerAyriVeYurunur()
        {
            var center = new GridPos(10, 10);
            var roles = new[] { SceneRole.Groom, SceneRole.Bride, SceneRole.GroomSide, SceneRole.BrideSide, SceneRole.Guest, SceneRole.Guest, SceneRole.Guest };
            var blocked = new GridPos(11, 10);   // gelinin yeri dolu: en yakın boş karo
            var seats = SceneSeats.Layout(center, roles, p => p != blocked);
            Assert.All(seats, s => Assert.True(s.seat.HasValue));
            Assert.Equal(seats.Count, seats.Select(s => s.seat.Value).Distinct().Count());
            Assert.DoesNotContain(seats, s => s.seat.Value == blocked);
            Assert.Equal(new GridPos(9, 10), seats[0].seat.Value);
            Assert.True(seats[0].face.X > seats[0].seat.Value.X);   // damat geline (doğuya) döner
            Assert.True(seats[1].face.X < seats[1].seat.Value.X);   // gelin damada döner
            Assert.True(seats[2].seat.Value.X < center.X && seats[3].seat.Value.X > center.X);   // taraflar kendi yanında
            Assert.All(seats.Skip(4), s => Assert.Equal(center, s.face));   // davetliler ortaya bakar
            // Kına: gelin ortada, annesi yanı başında.
            var henna = SceneSeats.Layout(center, new[] { SceneRole.Center, SceneRole.BrideSide }, null);
            Assert.Equal(center, henna[0].seat.Value);
            Assert.Equal(1, System.Math.Abs(henna[1].seat.Value.X - center.X) + System.Math.Abs(henna[1].seat.Value.Y - center.Y));
        }

        private static (Character husband, Character bride) Couple(Rig r)
        {
            var h = Add(r, "Kerem", Sex.Male, 40); var w = Add(r, "Aslı", Sex.Female, 38, x: 6);
            h.SpouseId = w.Id; w.SpouseId = h.Id;
            return (h, w);
        }

        [Fact]
        public void Ceyiz_SatilirsaGelinKocasinaKirilir_EvdeHarcanmasiSayilmaz()
        {
            var r = NewRig();
            var (h, w) = Couple(r);
            r.Rel.RecordDowry(w.Id, "item.linen", 2);
            // Evde dikişte 2 keten harcandı, hanede kendi 3 keteni var: 1 satış gelinin payına dokunmaz.
            r.Rel.OnGoodsLeft("item.linen", 1, ownedAfter: 2, seller: h);
            Assert.Null(r.Rel.PendingIssue(h.Id, w.Id));
            Assert.Equal(2, r.Rel.DowryCount(w.Id, "item.linen"));
            // Hepsi satıldı: gelinin payı gitti.
            r.Rel.OnGoodsLeft("item.linen", 2, ownedAfter: 0, seller: h);
            var issue = r.Rel.PendingIssue(h.Id, w.Id);
            Assert.NotNull(issue);
            Assert.Equal(SpouseTrigger.SpentDowry, issue.Trigger);
            Assert.Equal(h.Id, issue.Offender);
            Assert.Equal(0, r.Rel.DowryCount(w.Id, "item.linen"));
        }

        [Fact]
        public void Ceyiz_GelinKendiSatarsaKimseKirilmaz_TakiAgir()
        {
            var r = NewRig();
            var (h, w) = Couple(r);
            r.Rel.RecordDowry(w.Id, "item.dowry_linen", 1);
            r.Rel.OnGoodsLeft("item.dowry_linen", 1, 0, seller: w);
            Assert.Null(r.Rel.PendingIssue(h.Id, w.Id));
            r.Rel.RecordDowry(w.Id, RelationService.WeddingJewel, 1);
            r.Rel.OnGoodsLeft(RelationService.WeddingJewel, 1, 0, seller: h);
            Assert.Equal(80 - 24, r.Rel.Warmth(h.Id, w.Id));   // 20 × 1,2
            Assert.True(RelationService.DowryScale("item.linen", 1) < RelationService.DowryScale(RelationService.WeddingJewel, 1));
        }

        [Fact]
        public void Yadigar_EsinSoyundanAgir_KendiSoyundanHafif_KumarBahisleBuyur()
        {
            var r = NewRig();
            var (h, w) = Couple(r);
            var wifesMother = Add(r, "Nine", Sex.Female, 70); w.BirthMotherId = wifesMother.Id;
            var hisFather = Add(r, "Dede", Sex.Male, 70); h.BirthFatherId = hisFather.Id;
            Assert.Equal(1.5, LifeSystems.HeirloomScale(new Heirloom { MakerId = wifesMother.Id }, h, w));
            Assert.Equal(0.5, LifeSystems.HeirloomScale(new Heirloom { MakerId = hisFather.Id }, h, w));
            Assert.Equal(1.0, LifeSystems.HeirloomScale(new Heirloom { MakerId = 999 }, h, w));
            r.Rel.ReportTrigger(SpouseTrigger.SoldHeirloom, h, scale: 1.5);
            Assert.Equal(80 - 30, r.Rel.Warmth(h.Id, w.Id));
            Assert.True(LifeSystems.GambleScale(5) < LifeSystems.GambleScale(50));
        }

        [Fact]
        public void Bahis_YatiraniKaydeder_EskiKayitOkunur()
        {
            var bets = new RaceBets();
            bets.Import("12|fest.spring|0|25|0.2|7|Karayel");
            Assert.Equal(7, bets.Current.BettorId);
            Assert.Equal("Karayel", bets.Current.RunnerName);
            var again = new RaceBets(); again.Import(bets.Export());
            Assert.Equal(7, again.Current.BettorId);
            var old = new RaceBets(); old.Import("12|fest.spring|0|25|0.2|Poyraz");
            Assert.Equal(0, old.Current.BettorId);
            Assert.Equal("Poyraz", old.Current.RunnerName);
        }

        [Fact]
        public void Ceyiz_KayittaKalir()
        {
            var r = NewRig();
            var (h, w) = Couple(r);
            r.Rel.RecordDowry(w.Id, "item.felt", 3);
            var save = r.Rel.Export();
            r.Rel.Import(save);
            Assert.Equal(3, r.Rel.DowryCount(w.Id, "item.felt"));
        }
    }
}
