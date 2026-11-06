using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class JobTests
    {
        private static ItemDef Item(string id, NeedType need, float restore, double days) => new ItemDef
        {
            Id = id,
            DisplayName = id,
            Interactions = new List<InteractionDef>
            {
                new InteractionDef { Id = "use", DisplayName = "Kullan", Need = need, Restore = restore, DurationDays = days }
            }
        };

        private static (WorldState world, Character c, JobRunner runner) Setup(bool player = true, int w = 20, int h = 12)
        {
            var world = new WorldState(new GridMap(w, h));
            var c = new Character(1, "Aldric", new GridPos(0, 5), isPlayerControlled: player);
            return (world, c, new JobRunner(c, world));
        }

        [Fact]
        public void EsyaKonunca_KaroGecilemezOlur_DoluKaroyaKonamaz()
        {
            var world = new WorldState(new GridMap(5, 5));
            world.PlaceItem(Item("bed", NeedType.Energy, 60, 0.3), new GridPos(2, 2));
            Assert.True(world.Map.IsBlocked(new GridPos(2, 2)));
            Assert.Throws<InvalidOperationException>(() => world.PlaceItem(Item("x", NeedType.Energy, 5, 0.1), new GridPos(2, 2)));
            Assert.Throws<InvalidOperationException>(() => world.PlaceItem(Item("x", NeedType.Energy, 5, 0.1), new GridPos(9, 9)));
        }

        [Fact]
        public void Yurume_MesafeyeGoreOyunGunuHarcar()
        {
            var (_, c, runner) = Setup();
            runner.Command(Job.MoveTo(new GridPos(10, 5)));

            runner.Tick(9.0 / JobRunner.TilesPerGameDay);
            Assert.False(runner.IsIdle);
            Assert.Equal(new GridPos(9, 5), c.Position);

            runner.Tick(1.0 / JobRunner.TilesPerGameDay + 1e-9);
            Assert.True(runner.IsIdle);
            Assert.Equal(new GridPos(10, 5), c.Position);
        }

        [Fact]
        public void ParcaliIlerleme_TekAdimlaAyniSonucuVerir()
        {
            var (_, c1, r1) = Setup();
            var (_, c2, r2) = Setup();
            r1.Command(Job.MoveTo(new GridPos(7, 9)));
            r2.Command(Job.MoveTo(new GridPos(7, 9)));

            r1.Tick(0.004);
            for (int i = 0; i < 4000; i++) r2.Tick(0.000001);

            Assert.Equal(c1.Position, c2.Position);
            Assert.Equal(r1.IsIdle, r2.IsIdle);
        }

        [Fact]
        public void UlasilamayanHedef_IsiDusurur_NedenKaydedilir()
        {
            var (world, c, runner) = Setup();
            for (int y = 0; y < 12; y++) world.Map.SetBlocked(new GridPos(10, y), true);

            runner.Command(Job.MoveTo(new GridPos(15, 5)));
            runner.Tick(0.01);

            Assert.True(runner.IsIdle);
            Assert.NotNull(runner.LastFailure);
            Assert.Equal(new GridPos(0, 5), c.Position);
        }

        [Fact]
        public void EsyaKullanma_YanaGiderSureBekler_IhtiyaciDoyurur()
        {
            var (world, c, runner) = Setup();
            var bed = world.PlaceItem(Item("bed", NeedType.Energy, 60, 0.3), new GridPos(6, 5));
            c.Needs.Set(NeedType.Energy, 10f);

            runner.Command(Job.UseItem(bed.Id, "use"));

            runner.Tick(5.0 / JobRunner.TilesPerGameDay + 0.1); // yürüme bitti, kullanım sürüyor
            Assert.Equal(new GridPos(5, 5), c.Position);
            Assert.True(runner.IsUsingItem);
            Assert.Equal(10.0, (double)c.Needs.Get(NeedType.Energy), 3);

            runner.Tick(0.3);
            Assert.True(runner.IsIdle);
            Assert.Equal(70.0, (double)c.Needs.Get(NeedType.Energy), 3);
        }

        [Fact]
        public void Kuyruk_SiraylaCalisir_SinirdaReddeder()
        {
            var (world, c, runner) = Setup();
            runner.Queue.MaxLength = 2;
            Assert.True(runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(3, 5))));
            Assert.True(runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(3, 8))));
            Assert.False(runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(0, 0))));

            runner.Tick(1.0);
            Assert.True(runner.IsIdle);
            Assert.Equal(new GridPos(3, 8), c.Position);
        }

        [Fact]
        public void Komut_KuyrugunYerineGecer()
        {
            var (_, c, runner) = Setup();
            runner.Queue.TryEnqueue(Job.MoveTo(new GridPos(19, 11)));
            runner.Command(Job.MoveTo(new GridPos(2, 5)));
            Assert.Equal(1, runner.Queue.Count);

            runner.Tick(1.0);
            Assert.Equal(new GridPos(2, 5), c.Position);
        }

        [Fact]
        public void Npc_KritikIhtiyaciOlunca_KendiEsyaSecer()
        {
            var (world, c, runner) = Setup(player: false);
            world.PlaceItem(Item("bed", NeedType.Energy, 60, 0.3), new GridPos(6, 5));
            c.Needs.Set(NeedType.Energy, 10f);

            runner.Tick(1.0);

            Assert.True(runner.IsIdle);
            Assert.Equal(70.0, (double)c.Needs.Get(NeedType.Energy), 3);
        }

        [Fact]
        public void Oyuncu_KendiliginenIsYapmaz()
        {
            var (world, c, runner) = Setup(player: true);
            world.PlaceItem(Item("bed", NeedType.Energy, 60, 0.3), new GridPos(6, 5));
            c.Needs.Set(NeedType.Energy, 10f);

            runner.Tick(1.0);

            Assert.Equal(10.0, (double)c.Needs.Get(NeedType.Energy), 3);
            Assert.Equal(new GridPos(0, 5), c.Position);
        }

        [Fact]
        public void Npc_EnCokDoyurani_Secer()
        {
            var (world, c, runner) = Setup(player: false);
            world.PlaceItem(Item("stool", NeedType.Energy, 5, 0.05), new GridPos(2, 5));  // yakın ama az doyurur
            world.PlaceItem(Item("bed", NeedType.Energy, 60, 0.3), new GridPos(15, 5));   // uzak ama çok doyurur
            c.Needs.Set(NeedType.Energy, 10f);

            runner.Tick(1.0);

            Assert.Equal(70.0, (double)c.Needs.Get(NeedType.Energy), 3);
        }

        [Fact]
        public void Npc_UygunEsyaYoksa_BosBekler()
        {
            var (world, c, runner) = Setup(player: false);
            world.PlaceItem(Item("pot", NeedType.Hunger, 50, 0.15), new GridPos(6, 5));
            c.Needs.Set(NeedType.Energy, 10f);

            runner.Tick(1.0);

            Assert.True(runner.IsIdle);
            Assert.Equal(new GridPos(0, 5), c.Position);
        }
    }
}
