using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 47 (20.11.2026): gün 12 dakika, işler gerçek zamanda sabit, hızlı gece, menüde duraklatma.
    public class ZamanOlcegiTests
    {
        private static ItemDef Item(string id, NeedType need, float restore, double days, SkillType? skill = null) => new ItemDef
        {
            Id = id,
            DisplayName = id,
            Interactions = new List<InteractionDef>
            {
                new InteractionDef { Id = "use", DisplayName = "Kullan", Need = need, Restore = restore, DurationDays = days, Skill = skill }
            }
        };

        [Fact]
        public void VarsayilanGun_OnIkiDakika_IsOlcegiYarim()
        {
            var cfg = TimeConfig.Default;
            Assert.Equal(720.0, cfg.RealSecondsPerGameDay, 6);
            Assert.Equal(0.5, cfg.ActionTimeScale, 6);
            Assert.Equal(1.0, new TimeConfig(360.0).ActionTimeScale, 6);
        }

        [Fact]
        public void HizliGece_DortKatAkar_KritikAndaDurur()
        {
            var clock = new GameClock();
            clock.FastNight = true;
            clock.Advance(180);   // çeyrek gün gerçek süre → ×4 = 1 gün
            Assert.Equal(1.0, clock.TotalDays, 6);

            clock.EnterCriticalMoment();
            Assert.False(clock.IsFastNightActive);
            clock.Advance(720);
            Assert.Equal(2.0, clock.TotalDays, 6);
            clock.ExitCriticalMoment();
        }

        [Fact]
        public void Duraklatma_ZamaniDurdurur()
        {
            var clock = new GameClock();
            clock.Paused = true;
            Assert.Equal(0.0, clock.Advance(720), 9);
            Assert.Equal(0.0, clock.TotalDays, 9);
            clock.Paused = false;
            clock.Advance(720);
            Assert.Equal(1.0, clock.TotalDays, 6);
        }

        [Fact]
        public void IsSuresi_OlcekleKisalir_UykuKisalmaz()
        {
            var world = new WorldState(new GridMap(10, 10));
            var c = new Character(1, "Ali", new GridPos(1, 1));
            c.SetAge(35);
            var runner = new JobRunner(c, world) { ActionTimeScale = 0.5 };
            var eat = new InteractionDef { Id = "eat", Need = NeedType.Hunger, Restore = 40, DurationDays = 0.1 };
            var chop = new InteractionDef { Id = "chop", Skill = SkillType.Woodcutting, DurationDays = 0.2 };
            var sleep = new InteractionDef { Id = "sleep", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 };
            Assert.Equal(0.05, runner.EffectiveDuration(eat), 6);
            Assert.Equal(0.1, runner.EffectiveDuration(chop), 6);
            Assert.Equal(0.3, runner.EffectiveDuration(sleep), 6);
        }

        [Fact]
        public void Yurume_GercekZamandaAyniHizda_OyunGunundeYariSurer()
        {
            var world = new WorldState(new GridMap(20, 10));
            var c = new Character(1, "Ali", new GridPos(0, 5), true);
            var runner = new JobRunner(c, world) { ActionTimeScale = 0.5 };
            runner.Command(Job.MoveTo(new GridPos(10, 5)));
            runner.Tick(5.0 / JobRunner.TilesPerGameDay + 1e-9);   // eskiden 5 karo, şimdi 10 karo
            Assert.True(runner.IsIdle);
            Assert.Equal(new GridPos(10, 5), c.Position);
        }

        [Fact]
        public void YasamSistemi_KosucuyaSaattenOlcekVerir()
        {
            var db = new ContentDatabase();
            var world = new WorldState(new GridMap(10, 10));
            var life = new LifeSystems(db, world, new Household(), new Population(), new GameClock());
            var c = new Character(1, "Ali", new GridPos(1, 1));
            var runner = new JobRunner(c, world);
            life.Attach(runner);
            Assert.Equal(0.5, runner.ActionTimeScale, 6);
        }

        [Fact]
        public void YemekKisalir_IhtiyacDoygunluguAyniKalir()
        {
            var world = new WorldState(new GridMap(10, 10));
            var c = new Character(1, "Ali", new GridPos(1, 1), true);
            var runner = new JobRunner(c, world) { ActionTimeScale = 0.5 };
            var table = world.PlaceItem(Item("table", NeedType.Hunger, 40, 0.1), new GridPos(2, 1));
            c.Needs.Set(NeedType.Hunger, 20f);
            runner.Command(Job.UseItem(table.Id, "use"));
            runner.Tick(0.06);   // eski sürede (0,1) bitmezdi; ölçekle 0,05
            Assert.True(runner.IsIdle);
            Assert.True(c.Needs.Get(NeedType.Hunger) >= 59f);
        }
    }
}
