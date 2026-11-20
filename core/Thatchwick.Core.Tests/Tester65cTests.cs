using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Belge 65c (tester tur 5): hızlı gece yalnızca gece, uykuda enerji azalmaz, gece yatma eşiği.</summary>
    public class Tester65cTests
    {
        [Fact]
        public void HizliGece_GunduzSekerlemesindeUygulanmaz()
        {
            var clock = GameClock.ForNewGame();          // 06:00
            clock.FastNight = true;                      // herkes uyuyor
            clock.Advance(720 * 9.0 / 24.0);             // 15:00
            Assert.False(clock.IsFastNightHour);
            Assert.False(clock.IsFastNightActive);
            Assert.Equal(1.0, clock.EffectiveMultiplier, 9);

            clock.Advance(720 * 6.0 / 24.0);             // 21:00
            Assert.True(clock.IsFastNightHour);
            Assert.True(clock.IsFastNightActive);
            Assert.Equal(GameClock.FastNightMultiplier, clock.EffectiveMultiplier, 9);
        }

        [Fact]
        public void Uykuda_EnerjiAzalmaz()
        {
            var c = new Character(1, "Wat", new GridPos(1, 1));
            c.SetAge(30);
            c.Needs.Set(NeedType.Energy, 40f);
            c.Asleep = true;
            c.Tick(0.25);
            Assert.Equal(40.0, (double)c.Needs.Get(NeedType.Energy), 3);

            c.Asleep = false;
            c.Tick(0.25);
            Assert.True(c.Needs.Get(NeedType.Energy) < 40f);
        }

        [Fact]
        public void EnerjiEsigi_GeceYuksek_GunduzDusuk()
        {
            var c = new Character(1, "Wat", new GridPos(1, 1));
            double hour = 14;
            var r = new JobRunner(c, new WorldState(new GridMap(4, 4))) { HourOfDay = () => hour };
            Assert.Equal(JobRunner.AutonomyThreshold, r.AutonomyThresholdFor(NeedType.Energy));
            hour = 22;
            Assert.Equal(JobRunner.NightSleepThreshold, r.AutonomyThresholdFor(NeedType.Energy));
            hour = 3;
            Assert.Equal(JobRunner.NightSleepThreshold, r.AutonomyThresholdFor(NeedType.Energy));
            Assert.Equal(JobRunner.AutonomyThreshold, r.AutonomyThresholdFor(NeedType.Hunger));

            var noClock = new JobRunner(c, new WorldState(new GridMap(4, 4)));
            Assert.Equal(JobRunner.AutonomyThreshold, noClock.AutonomyThresholdFor(NeedType.Energy));
        }

        private static Thatchwick.Core.Content.ItemDef Seat(string id, float restore, double days) => new Thatchwick.Core.Content.ItemDef
        {
            Id = id, DisplayName = id,
            Interactions = new System.Collections.Generic.List<Thatchwick.Core.Content.InteractionDef>
            {
                new Thatchwick.Core.Content.InteractionDef { Id = "rest", DisplayName = "Dinlen", Need = NeedType.Energy, Restore = restore, DurationDays = days }
            }
        };

        [Fact]
        public void Gece_YorgunKarakter_SandalyeYerineYatagaGider()
        {
            var world = new WorldState(new GridMap(12, 12));
            var chair = world.PlaceItem(Seat("chair", 8f, 0.02), new GridPos(2, 1));
            var bed = world.PlaceItem(Seat("bed", 65f, 0.3), new GridPos(9, 9));
            var c = new Character(1, "Wat", new GridPos(1, 1));
            c.SetAge(30);
            c.Needs.Set(NeedType.Energy, 55f);
            double hour = 22;
            var r = new JobRunner(c, world) { Autonomous = true, HourOfDay = () => hour };
            r.Tick(0.001);
            Assert.NotNull(r.Queue.Current);
            Assert.Equal(bed.Id, r.Queue.Current.ItemId);
        }
    }
}
