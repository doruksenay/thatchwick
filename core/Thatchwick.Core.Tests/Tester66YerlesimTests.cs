using System.Linq;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Tester raporu 66 §1.4: merdiven ön duvarda kesit yüzünden duvarın dışına taşmış görünüyordu; kenar bandı öbeği kapı önüne
    // düşebiliyordu ve kapsamlı denetim kapının 2. dış karosunu görmüyordu.
    public class Tester66YerlesimTests
    {
        private static (WorldState w, StarterWorld.Result res) Town(int seed)
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var w = new WorldState(StarterWorld.BuildMap(lay));
            return (w, StarterWorld.Place(w, db, lay, seed));
        }

        [Fact]
        public void Merdiven_ArkaDuvarda_StairsFrontIseOnDuvarda()
        {
            // Küçük köylü evinde (4x3) arka duvarda merdivene yer yok; ön duvar yedektir. Kural dükkân ve handa denetlenir.
            foreach (var b in Regions.TownBuildings.Where(x => x.Kind != "house"))
            {
                TownBuilder.PlanStairs(b, true, true, false, out var up, out _);
                if (!up.HasValue) continue;
                var s = up.Value;
                bool front = (s.Rot == 0 && s.Pos.X == b.X0) || (s.Rot == 1 && s.Pos.Y == b.Y0);
                bool back = (s.Rot == 0 && s.Pos.X == b.X1) || (s.Rot == 1 && s.Pos.Y == b.Y1);
                if (b.StairsFront) Assert.True(front || !back, $"{b.Id}: StairsFront ama merdiven arka duvarda");
                else Assert.False(front, $"{b.Id}: merdiven ön duvarda ({s.Pos}, dönüş {s.Rot})");
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        public void Koy_IhlalsizVeEksiksizKurulur(int seed)
        {
            var (w, res) = Town(seed);
            Assert.True(res.Reports.All(r => r.Missing.Count == 0),
                string.Join("\n", res.Reports.Where(r => r.Missing.Count > 0).Select(r => r.Room + ": " + string.Join(",", r.Missing))));
            var errors = PlacementValidator.Run(w, res.Plan).Where(v => v.Severity == Severity.Error).ToList();
            Assert.True(errors.Count == 0, string.Join("\n", errors.Select(e => $"{e.At} {e.ItemDefId} {e.Reason}")));
        }
    }
}
