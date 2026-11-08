using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 16 (bakım, evlilik, çocuk) ve Adım 17 (ölüm, miras, baş karakter geçişi; T8).
    public class FamilyTests
    {
        private sealed class Ctx
        {
            public WorldState World; public Population Pop; public Household House; public FamilyService Family;
            public Dictionary<int, JobRunner> Runners = new Dictionary<int, JobRunner>();
            public Character Add(int id, string name, Sex sex, double age, GridPos pos, bool player = false, bool villager = false)
            {
                var c = new Character(id, name, pos, player) { Sex = sex, IsVillager = villager, CanStarve = !villager };
                c.SetAge(age);
                Pop.Add(c);
                Runners[id] = new JobRunner(c, World) { Social = Family, Autonomous = !player && !villager };
                return c;
            }
            public void Tick(double days, double step = 0.01)
            {
                for (double t = 0; t < days; t += step)
                {
                    Pop.Tick(step);
                    foreach (var c in Pop.Members.ToList())
                    {
                        if (!Runners.ContainsKey(c.Id)) Runners[c.Id] = new JobRunner(c, World) { Social = Family, Autonomous = !c.IsVillager };
                        if (!c.IsDead && c.Stage != AgeStage.Baby) Runners[c.Id].Tick(step);   // oyundaki gibi: bebek iş yapmaz
                    }
                }
            }
            public void Run(int id, Job job, double maxDays = 1)
            {
                Runners[id].Command(job);
                for (double t = 0; t < maxDays && !Runners[id].IsIdle; t += 0.01) Tick(0.01);
            }
        }

        private static Ctx Setup(int food = 10)
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "food" } });
            var house = new Household();
            if (food > 0) house.AddItem("turnip", food);
            var pop = new Population(1, seed: 3);
            return new Ctx { World = new WorldState(new GridMap(20, 12)), Pop = pop, House = house, Family = new FamilyService(pop, house, db, seed: 2) };
        }

        // ---- bakım ----

        [Fact]
        public void Yetiskin_AcBebegiKendiligindenBesler_YiyecekHarcar()
        {
            var c = Setup(food: 5);
            c.Add(1, "Ana", Sex.Female, 40, new GridPos(2, 2));
            var baby = c.Add(2, "Bebek", Sex.Male, 1, new GridPos(8, 2));
            baby.Needs.Set(NeedType.Hunger, 20f);
            c.Tick(0.2);
            Assert.True(baby.Needs.Get(NeedType.Hunger) > 50f, $"açlık {baby.Needs.Get(NeedType.Hunger)}");
            Assert.True(c.House.Count("turnip") < 5);
        }

        [Fact]
        public void Bebek_YiyecekYoksaBeslenemez_SonundaOlur()
        {
            var c = Setup(food: 0);
            c.Add(1, "Ana", Sex.Female, 40, new GridPos(2, 2));
            var baby = c.Add(2, "Bebek", Sex.Male, 1, new GridPos(8, 2));
            c.Tick(5, 0.05);
            Assert.True(baby.IsDead);
            Assert.Equal(DeathCause.Starvation, baby.CauseOfDeath);
        }

        [Fact]
        public void Bebek_SosyalIsYapamaz_CocukBebekBesleyemez()
        {
            // Bebeğin iş kuyruğu oyunda hiç çalıştırılmaz (GameRoot); aile kuralları da bebeği yapan olarak kabul etmez.
            var c = Setup();
            var baby = c.Add(1, "Bebek", Sex.Male, 1, new GridPos(2, 2));
            var child = c.Add(2, "Çocuk", Sex.Female, 15, new GridPos(4, 2));
            Assert.Equal("Bebek bunu yapamaz.", c.Family.CanDo(baby, child, SocialAction.Chat));
            Assert.Equal("Çocuk bebeği besleyemez.", c.Family.CanDo(child, baby, SocialAction.Feed));
        }

        // ---- evlilik ----

        [Fact]
        public void Sohbet_YakinlikArtirir_YuzdeEvlenilir_KoyluHaneyeKatilir()
        {
            var c = Setup();
            var aldric = c.Add(1, "Aldric", Sex.Male, 30, new GridPos(2, 2), player: true);
            var edith = c.Add(2, "Edith", Sex.Female, 28, new GridPos(10, 5), villager: true);
            Character joined = null;
            c.Family.JoinedHousehold += x => joined = x;

            Assert.Contains("Yakınlık", c.Family.CanDo(aldric, edith, SocialAction.Propose));
            for (int i = 0; i < 5; i++) c.Run(1, Job.Social(2, SocialAction.Chat));
            Assert.Equal(100, c.Family.Affection(1, 2));
            c.Run(1, Job.Social(2, SocialAction.Propose));
            Assert.Equal(2, aldric.SpouseId);
            Assert.Equal(1, edith.SpouseId);
            Assert.False(edith.IsVillager);
            Assert.Same(edith, joined);
        }

        [Fact]
        public void Evlilik_YakinAkrabaAyniCinsYasVeEvliKurallari()
        {
            var c = Setup();
            var mom = c.Add(1, "Ana", Sex.Female, 50, new GridPos(1, 1));
            var son = c.Add(2, "Oğul", Sex.Male, 30, new GridPos(2, 1));
            son.BirthMotherId = 1;
            var sis = c.Add(3, "Kız", Sex.Female, 28, new GridPos(3, 1));
            sis.BirthMotherId = 1;
            var kid = c.Add(4, "Çocuk", Sex.Female, 15, new GridPos(4, 1));
            var man = c.Add(5, "Wat", Sex.Male, 30, new GridPos(5, 1));
            foreach (var (a, b) in new[] { (1, 2), (2, 3), (2, 4), (2, 5) }) c.Family.SetAffection(a, b, 100);
            Assert.Contains("akraba", c.Family.CanDo(son, mom, SocialAction.Propose));
            Assert.Contains("akraba", c.Family.CanDo(son, sis, SocialAction.Propose));
            Assert.Contains("genç ya da yetişkin", c.Family.CanDo(son, kid, SocialAction.Propose));
            Assert.Contains("kadın ile erkek", c.Family.CanDo(son, man, SocialAction.Propose));
        }

        [Fact]
        public void Esler_CocukIster_HamilelikDogumVeNesil()
        {
            var c = Setup();
            var aldric = c.Add(1, "Aldric", Sex.Male, 30, new GridPos(2, 2), player: true);
            var edith = c.Add(2, "Edith", Sex.Female, 28, new GridPos(3, 2));
            aldric.SpouseId = 2; edith.SpouseId = 1;
            edith.PregnancyDays = 1;
            for (int i = 0; i < 10 && !edith.IsPregnant; i++) c.Run(1, Job.Social(2, SocialAction.TryForBaby));
            Assert.True(edith.IsPregnant);
            Character baby = null;
            c.Pop.BabyBorn += (b, m, f) => baby = b;
            c.Tick(1.2);
            Assert.NotNull(baby);
            Assert.Equal(2, baby.BirthMotherId);
            Assert.Equal(1, baby.BirthFatherId);
            Assert.Equal(2, baby.Generation);
            Assert.True(baby.CanStarve);
        }

        [Fact]
        public void CocukIstemek_YalnizcaEslerArasinda()
        {
            var c = Setup();
            var a = c.Add(1, "A", Sex.Male, 30, new GridPos(2, 2));
            var b = c.Add(2, "B", Sex.Female, 30, new GridPos(3, 2));
            Assert.Contains("eşler", c.Family.CanDo(a, b, SocialAction.TryForBaby));
        }

        // ---- hanedan ve miras (T8) ----

        [Fact]
        public void T8_BasOlunce_EnBuyukGencCocukVarisOlur_BeceriMirasi()
        {
            var c = Setup();
            var head = c.Add(1, "Aldric", Sex.Male, 70, new GridPos(1, 1), player: true);
            head.GainSkillXp(SkillType.Woodcutting, 2000);
            var wife = c.Add(2, "Edith", Sex.Female, 60, new GridPos(2, 1));
            head.SpouseId = 2; wife.SpouseId = 1;
            var older = c.Add(3, "Wat", Sex.Male, 30, new GridPos(3, 1)); older.BirthFatherId = 1; older.Generation = 2;
            var younger = c.Add(4, "Joan", Sex.Female, 26, new GridPos(4, 1)); younger.BirthFatherId = 1; younger.Generation = 2;
            var dynasty = new Dynasty();
            dynasty.Found(head, 0);

            head.LifespanDays = 70.5;
            head.Tick(1);
            Assert.True(head.IsDead);
            var r = dynasty.OnDeath(head, c.Pop.Members, 10);
            Assert.Same(older, r.Heir);
            Assert.True(older.IsPlayerControlled);
            Assert.False(head.IsPlayerControlled);
            Assert.Equal(3, dynasty.HeadId);
            Assert.Equal(2, dynasty.CurrentGeneration);
            Assert.Equal(200.0, older.Skills.GetXp(SkillType.Woodcutting), 3);
            Assert.Equal(DeathCause.OldAge, dynasty.History[0].EndCause);
        }

        [Fact]
        public void T8_VarisSirasi_CocukYoksaEs_OYoksaEnYasliUye()
        {
            var c = Setup();
            var head = c.Add(1, "Aldric", Sex.Male, 50, new GridPos(1, 1), player: true);
            var wife = c.Add(2, "Edith", Sex.Female, 30, new GridPos(2, 1));
            var maud = c.Add(3, "Maud", Sex.Female, 45, new GridPos(3, 1));
            var babyKid = c.Add(4, "Bebek", Sex.Male, 2, new GridPos(4, 1)); babyKid.BirthFatherId = 1;
            head.SpouseId = 2; wife.SpouseId = 1;
            Assert.Same(wife, Dynasty.ChooseHeir(head, c.Pop.Members));
            head.SpouseId = null;
            Assert.Same(maud, Dynasty.ChooseHeir(head, c.Pop.Members));
        }

        [Fact]
        public void T8_KimseKalmazsaHanedanBiter_BaskasininOlumuDevretmez()
        {
            var c = Setup();
            var head = c.Add(1, "Aldric", Sex.Male, 50, new GridPos(1, 1), player: true);
            var villager = c.Add(2, "Köylü", Sex.Female, 30, new GridPos(2, 1), villager: true);
            var dynasty = new Dynasty();
            dynasty.Found(head, 0);
            Assert.Null(dynasty.OnDeath(villager, c.Pop.Members, 1));
            var r = dynasty.OnDeath(head, c.Pop.Members, 5);
            Assert.True(r.DynastyEnded);
            Assert.True(dynasty.Ended);
        }
    }
}
