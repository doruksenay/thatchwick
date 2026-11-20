using Thatchwick.Core.Progress;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Elmas: yalnızca oyun içinden kazanılır (seviye, rozet, hedef); Ayna'da harcanır (karar 20.11.2026).</summary>
    public class ElmasTests
    {
        [Fact]
        public void SeviyeAtlayinca_ElmasGelir()
        {
            var a = new AccountProgress();
            int diff = 0;
            a.DiamondsChanged += (now, d) => diff += d;
            a.AddWorkXp(100, 1);   // Sv 2
            Assert.Equal(2, a.Level);
            Assert.Equal(AccountProgress.DiamondsPerLevel, a.Diamonds);
            Assert.Equal(AccountProgress.DiamondsPerLevel, diff);
        }

        [Fact]
        public void Rozet_Ve_Hedef_ElmasVerir()
        {
            var a = new AccountProgress();
            a.Count("act:chop", 10);   // Oduncu rozeti (+100 XP: Sv 2)
            Assert.True(a.HasBadge("badge.woodcutter"));
            int afterBadge = a.Diamonds;
            Assert.Equal(AccountProgress.DiamondsPerBadge + AccountProgress.DiamondsPerLevel, afterBadge);
            a.OnGoalCompleted();
            Assert.Equal(afterBadge + AccountProgress.DiamondsPerGoal, a.Diamonds);
        }

        [Fact]
        public void Harcama_YetmezseDegismez()
        {
            var a = new AccountProgress();
            a.AddDiamonds(15);
            Assert.False(a.TrySpendDiamonds(20));
            Assert.Equal(15, a.Diamonds);
            Assert.True(a.TrySpendDiamonds(15));
            Assert.Equal(0, a.Diamonds);
            Assert.False(a.TrySpendDiamonds(-1));
        }

        [Fact]
        public void EskiKayit_SeviyeVeRozettenBirKezHesaplanir()
        {
            var a = new AccountProgress();
            a.Restore(AccountProgress.XpForLevel(4), null, new[] { "badge.farmer" }, null, -1, 0);
            a.RestoreDiamonds(0, seeded: false);
            Assert.Equal(3 * AccountProgress.DiamondsPerLevel + AccountProgress.DiamondsPerBadge, a.Diamonds);
            a.RestoreDiamonds(7, seeded: true);   // yeni kayıt: yazılan bakiye aynen
            Assert.Equal(7, a.Diamonds);
        }
    }
}
