using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class NeedSetTests
    {
        [Fact]
        public void YeniKarakter_TamDoygunBaslar()
        {
            var needs = new NeedSet();
            foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
                Assert.Equal(100f, needs.Get(n));
        }

        [Fact]
        public void Tick_OyunGunuUzerindenAzaltir()
        {
            var needs = new NeedSet();
            needs.SetDecayPerDay(NeedType.Hunger, 50f);
            needs.Tick(0.5);
            Assert.Equal(75.0, (double)needs.Get(NeedType.Hunger), 3);
        }

        [Fact]
        public void Deger_SifirinAltinaInmez_YuzuAsmaz()
        {
            var needs = new NeedSet();
            needs.Tick(10);
            Assert.Equal(0f, needs.Get(NeedType.Hunger));
            needs.Restore(NeedType.Hunger, 500f);
            Assert.Equal(100f, needs.Get(NeedType.Hunger));
        }

        [Fact]
        public void KritikEsik_VeAnyCritical()
        {
            var needs = new NeedSet();
            Assert.False(needs.AnyCritical());
            needs.Set(NeedType.Energy, NeedSet.CriticalThreshold);
            Assert.True(needs.IsCritical(NeedType.Energy));
            Assert.True(needs.AnyCritical());
            Assert.False(needs.IsCritical(NeedType.Hunger));
        }

        [Fact]
        public void GecersizGirdi_Reddedilir()
        {
            var needs = new NeedSet();
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.Tick(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.Restore(NeedType.Hunger, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => needs.SetDecayPerDay(NeedType.Hunger, -1));
        }

        [Fact]
        public void HizBiyolojiyiDegistirmez_OyunGunuAyniAzalirKritikAnDahil()
        {
            // T3: 1x'te 2 oyun günü ile 3x'te 2 oyun günü aynı azalmayı verir.
            var slow = new GameClock();
            var fast = new GameClock();
            fast.SetSpeed(3);
            var a = new NeedSet();
            var b = new NeedSet();

            a.Tick(slow.Advance(720));          // 2 gün, 1x
            b.Tick(fast.Advance(240));          // 240 sn * 3 / 360 = 2 gün
            Assert.Equal((double)a.Get(NeedType.Hunger), (double)b.Get(NeedType.Hunger), 3);
        }
    }

    public class CharacterTests
    {
        [Fact]
        public void OyuncuVeNpc_AyniTip_FarkBayrak()
        {
            var player = new Character(1, "Aldric", new GridPos(2, 2), isPlayerControlled: true);
            var npc = new Character(2, "Maud", new GridPos(3, 2));
            Assert.True(player.IsPlayerControlled);
            Assert.False(npc.IsPlayerControlled);
            Assert.Equal(player.GetType(), npc.GetType());
        }

        [Fact]
        public void Tick_IhtiyaclariAzaltir()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.Tick(0.25);
            Assert.True(c.Needs.Get(NeedType.Hunger) < 100f);
        }

        [Fact]
        public void BosIsim_Reddedilir()
        {
            Assert.Throws<ArgumentException>(() => new Character(1, " ", new GridPos(0, 0)));
        }
    }
}
