using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Hızlı dokunuşta takılma ve geri sıçrama (kullanıcı bulgusu 18.11.2026): yarım kalan adım korunur.
    public class YurumeAkiciligiTests
    {
        /// <summary>1x hızda bir kare (60 fps): 1 oyun günü 360 saniye.</summary>
        private const double Frame = 1.0 / (360.0 * 60.0);

        private static (Character c, JobRunner r) Walker()
        {
            var world = new WorldState(new GridMap(30, 30));
            var c = new Character(1, "Wat", new GridPos(5, 5), true);
            c.SetAge(30);
            return (c, new JobRunner(c, world));
        }

        // Görünümün çizdiği nokta (GameRoot.UpdateView ile aynı ara değer), ızgara biriminde.
        private static (double x, double y) Visual(Character c, JobRunner r)
        {
            double x = c.Position.X, y = c.Position.Y;
            if (r.NextTile is GridPos n) { x += (n.X - x) * r.StepProgress; y += (n.Y - y) * r.StepProgress; }
            return (x, y);
        }

        private static double Dist((double x, double y) a, (double x, double y) b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));

        [Fact]
        public void AyniYereArtArdaDokunmak_YuruyusuBozmaz()
        {
            var (c, r) = Walker();
            var target = new GridPos(15, 5);
            int frames = 0;
            for (; frames < 2000 && !c.Position.Equals(target); frames++)
            {
                if (frames % 2 == 0) r.Command(Job.MoveTo(target));       // her iki karede bir dokunuş
                r.Tick(Frame);
            }
            Assert.Equal(target, c.Position);
            Assert.InRange(frames, 190, 215);                              // 10 karo ≈ 200 kare, tek dokunuşla aynı
        }

        [Fact]
        public void FarkliYerlereHizliDokunmak_KarakterTakilmazIlerler()
        {
            var (c, r) = Walker();
            for (int f = 0; f < 400; f++)
            {
                if (f % 3 == 0) r.Command(Job.MoveTo(new GridPos(20, f % 2 == 0 ? 5 : 6)));   // hep biraz farklı hedef
                r.Tick(Frame);
            }
            Assert.True(c.Position.X >= 20, $"x={c.Position.X}");          // eskiden her dokunuşta adım sıfırlanıp yerinde sayıyordu
        }

        [Fact]
        public void AdimOrtasindaYonDegisince_GeriSicramaz()
        {
            var (c, r) = Walker();
            r.Command(Job.MoveTo(new GridPos(15, 5)));
            for (int f = 0; f < 10; f++) r.Tick(Frame);                    // adımın yarısı
            var before = Visual(c, r);
            Assert.InRange(r.StepProgress, 0.3f, 0.7f);

            r.Command(Job.MoveTo(new GridPos(0, 5)));                       // geri dön
            r.Tick(Frame);
            var after = Visual(c, r);
            Assert.True(Dist(before, after) < 0.1, $"{before} -> {after}");  // en çok bir karelik yol, sıçrama yok

            for (int f = 0; f < 400 && !c.Position.Equals(new GridPos(0, 5)); f++) r.Tick(Frame);
            Assert.Equal(new GridPos(0, 5), c.Position);
        }

        [Fact]
        public void AdimOrtasindaIptal_AdimTamamlanir()
        {
            var (c, r) = Walker();
            r.Command(Job.MoveTo(new GridPos(15, 5)));
            for (int f = 0; f < 10; f++) r.Tick(Frame);
            var before = Visual(c, r);
            r.Cancel();
            r.Tick(Frame);
            Assert.True(Dist(before, Visual(c, r)) < 0.1);
            Assert.True(r.NextTile.HasValue);                               // hâlâ adımı bitiriyor
            for (int f = 0; f < 30; f++) r.Tick(Frame);
            Assert.Equal(new GridPos(6, 5), c.Position);                    // geri dönmedi, adımı bitirdi
            Assert.Null(r.NextTile);
            Assert.False(r.HasCarriedStep);
        }
    }
}
