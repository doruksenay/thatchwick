using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Adım 18 otomatik oyun testi: Unity olmadan bir haneyi gerçek sistemlerle (saat, ısı, ihtiyaç,
    /// otonomi, bakım, evlilik, doğum, ölüm, miras) iki nesil boyunca oynatır. Basit bir "oyuncu"
    /// politikası ocağı besler ve çocuk ister. T1 (kışı ısınarak geçirme) ve T8 (nesil geçişi).
    /// </summary>
    public class GenerationSimulationTests
    {
        internal sealed class Sim
        {
            public GameClock Clock = new GameClock();
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public Population Pop;
            public HeatSystem Heat;
            public FamilyService Family;
            public InteractionEffects Effects;
            public Dynasty Dynasty = new Dynasty();
            public Dictionary<int, JobRunner> Runners = new Dictionary<int, JobRunner>();
            public ItemInstance Hearth;
            public List<string> Log = new List<string>();
            public bool HeatOn = true;

            public JobRunner RunnerOf(Character c)
            {
                if (!Runners.TryGetValue(c.Id, out var r))
                {
                    r = new JobRunner(c, World) { Effects = Effects, Social = Family, Autonomous = !c.IsPlayerControlled };
                    Runners[c.Id] = r;
                }
                return r;
            }

            public void Step(double dt)
            {
                double before = Clock.TotalDays;
                Clock.Restore(before + dt, 1);
                Heat.Tick(dt, Clock.TotalDays);
                foreach (var c in Pop.Members) c.AmbientTemperature = Heat.TemperatureAt(c.Position);
                Pop.Tick(dt);
                foreach (var c in Pop.Members.ToList())
                {
                    var r = RunnerOf(c);
                    r.Autonomous = true;    // otomatik testte oyuncu karakteri de ihtiyaçlarını kendisi giderir
                    if (!c.IsDead && c.Stage != AgeStage.Baby) r.Tick(dt);   // oyundaki gibi: bebek iş yapmaz, büyükler besler
                }
            }
        }

        internal static Sim Build()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "item.firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "fuel" } });
            db.Items.Register(new ItemDef { Id = "item.turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "food" } });
            db.Items.Register(new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f,
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "warm", DisplayName = "Isın", Need = NeedType.Warmth, Restore = 40, DurationDays = 0.1 } } });
            db.Items.Register(new ItemDef { Id = "pot", DisplayName = "Kazan",
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "eat", DisplayName = "Ye", Need = NeedType.Hunger, Restore = 50, DurationDays = 0.15, ConsumeItem = "item.turnip" } } });
            db.Items.Register(new ItemDef { Id = "bed", DisplayName = "Yatak", Width = 2,
                Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 } } });
            Assert.Empty(db.Validate());

            var map = new GridMap(24, 16);
            // 6x5 ev (4..9, 4..8), güneyde kapı
            for (int x = 4; x <= 9; x++) { map.SetEdge(new Edge(x, 4, EdgeAxis.Horizontal), EdgeKind.Wall); map.SetEdge(new Edge(x, 9, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = 4; y <= 8; y++) { map.SetEdge(new Edge(4, y, EdgeAxis.Vertical), EdgeKind.Wall); map.SetEdge(new Edge(10, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            map.SetEdge(new Edge(6, 4, EdgeAxis.Horizontal), EdgeKind.Door);
            var world = new WorldState(map);
            var house = new Household(100);
            house.AddItem("item.turnip", 2000);
            house.AddItem("item.firewood", 2000);
            var pop = new Population(1, seed: 8);
            var sim = new Sim { Db = db, World = world, House = house, Pop = pop };
            sim.Heat = new HeatSystem(world, sim.Clock.Config, null, 0);
            sim.Family = new FamilyService(pop, house, db, seed: 4);
            sim.Effects = new InteractionEffects(world, house, db, seed: 6);
            sim.Hearth = world.PlaceItem(db.Items.Get("hearth"), new GridPos(9, 8));
            world.PlaceItem(db.Items.Get("pot"), new GridPos(5, 8));
            world.PlaceItem(db.Items.Get("bed"), new GridPos(5, 5));
            world.PlaceItem(db.Items.Get("bed"), new GridPos(8, 5));
            sim.Family.Happened += (a, t, act, msg) => sim.Log.Add($"{sim.Clock.TotalDays:0.0}: {msg}");
            pop.BabyBorn += (b, m, f) => sim.Log.Add($"{sim.Clock.TotalDays:0.0}: doğum {b.Name} (nesil {b.Generation})");
            return sim;
        }

        internal static (Character head, Character wife) Founders(Sim sim, double headLifespan)
        {
            var head = new Character(1, "Aldric", new GridPos(6, 6), true) { Sex = Sex.Male, CanStarve = true, LifespanDays = headLifespan };
            head.SetAge(30);
            var wife = new Character(2, "Edith", new GridPos(7, 6), false) { Sex = Sex.Female, CanStarve = true, LifespanDays = 140 };
            wife.SetAge(28);
            head.SpouseId = 2; wife.SpouseId = 1;
            sim.Pop.Add(head); sim.Pop.Add(wife);
            sim.Dynasty.Found(head, 0);
            return (head, wife);
        }

        // Basit oyuncu: ocak hep yanık; eşler hamile değilse ve hanede 3'ten az çocuk varsa çocuk ister.
        internal static void Play(Sim sim, double days, Action<Sim> onHeadDeath = null)
        {
            const double dt = 0.02;
            double nextPolicy = 0;
            foreach (var c in sim.Pop.Members) c.Died += d =>
            {
                var r = sim.Dynasty.OnDeath(d, sim.Pop.Members, sim.Clock.TotalDays);
                if (r != null) sim.Log.Add($"{sim.Clock.TotalDays:0.0}: {r.Message}");
            };
            sim.Pop.BabyBorn += (b, m, f) => b.Died += d =>
            {
                var r = sim.Dynasty.OnDeath(d, sim.Pop.Members, sim.Clock.TotalDays);
                if (r != null) sim.Log.Add($"{sim.Clock.TotalDays:0.0}: {r.Message}");
            };

            for (double t = 0; t < days; t += dt)
            {
                if (sim.Clock.TotalDays >= nextPolicy)
                {
                    nextPolicy = sim.Clock.TotalDays + 0.25;
                    if (sim.HeatOn) while (sim.Heat.TryStoke(sim.Hearth.Id, sim.House)) { }
                    var head = sim.Dynasty.Head(sim.Pop);
                    if (head != null && !head.IsDead && head.SpouseId.HasValue)
                    {
                        var spouse = sim.Pop.Find(head.SpouseId.Value);
                        int kids = sim.Pop.Members.Count(c => !c.IsDead && (c.BirthMotherId == spouse?.Id || c.BirthFatherId == head.Id));
                        if (spouse != null && !spouse.IsDead && kids < 3 && sim.Family.CanDo(head, spouse, SocialAction.TryForBaby) == null && sim.RunnerOf(head).IsIdle)
                            sim.RunnerOf(head).Command(Job.Social(spouse.Id, SocialAction.TryForBaby));
                    }
                }
                sim.Step(dt);
            }
        }

        [Fact]
        public void T8_IkiNesil_DogumBakimOlumVeMiras()
        {
            var sim = Build();
            var (head, wife) = Founders(sim, headLifespan: 100);
            Play(sim, 110);

            var log = string.Join("\n", sim.Log);
            var kids = sim.Pop.Members.Where(c => c.Generation == 2).ToList();
            Assert.True(kids.Count >= 1, "çocuk doğmadı:\n" + log);
            Assert.True(kids.All(k => !k.IsDead), "çocuk öldü:\n" + log + "\n" + string.Join(",", kids.Select(k => k.Name + ":" + k.CauseOfDeath)));
            Assert.True(head.IsDead, "baş karakter ölmedi");
            Assert.Equal(DeathCause.OldAge, head.CauseOfDeath);
            var newHead = sim.Dynasty.Head(sim.Pop);
            Assert.NotNull(newHead);
            Assert.Equal(2, newHead.Generation);
            Assert.True(newHead.IsPlayerControlled);
            Assert.Equal(2, sim.Dynasty.History.Count);
            Assert.False(wife.IsDead, "eş öldü: " + wife.CauseOfDeath + "\n" + log);
        }

        [Fact]
        public void T1_IsitilanEvdeKisGecilir_IsitilmayandaDonulur()
        {
            // Kış ortasından başla (gün 33), ocaklı evde 7 gün: kimse donmamalı.
            var warm = Build();
            warm.Clock.Restore(33, 1);
            var (h1, w1) = Founders(warm, 140);
            Play(warm, 7);
            Assert.False(h1.IsDead || w1.IsDead, string.Join("\n", warm.Log));

            // Aynı kış, ocak hiç yakılmıyor ve ısınma etkileşimi sönük ocakta yok: donma beklenir.
            var cold = Build();
            cold.Clock.Restore(33, 1);
            cold.HeatOn = false;
            var (h2, w2) = Founders(cold, 140);
            Play(cold, 7);
            Assert.True(h2.IsDead && h2.CauseOfDeath == DeathCause.Cold, $"ısıtılmayan evde donma bekleniyordu: {h2.CauseOfDeath}");
        }
    }
}
