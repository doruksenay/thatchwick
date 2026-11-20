using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Xunit;

namespace Thatchwick.Core.Tests
{
    public class MenuVeRehberTests
    {
        [Fact]
        public void Yuvalar_BirinciYuvaEskiDosyaAdi()
        {
            Assert.Equal("thatchwick_save.json", SaveSlots.FileName(1));
            Assert.Equal("thatchwick_save_2.json", SaveSlots.FileName(2));
            Assert.Equal("thatchwick_save_3.json", SaveSlots.FileName(9));   // sınır dışı yuva en yakın yuvaya iner
            Assert.Equal("thatchwick_save.json", SaveSlots.FileName(0));
        }

        [Fact]
        public void YuvaOzeti_YilGunVeNesil()
        {
            Assert.Null(SaveSlots.Describe(null));
            var data = new SaveData
            {
                totalDays = 85.4, savedAtUtc = "2026-11-20T10:00:00Z",
                dynasty = new DynastySave { name = "Demirci", settlement = "Sazköy", history = new HeadSave[2] }
            };
            var info = SaveSlots.Describe(data, 40);
            Assert.Equal("Demirci", info.Dynasty);
            Assert.Equal("Sazköy", info.Settlement);
            Assert.Equal(3, info.Year);
            Assert.Equal(6, info.DayOfYear);
            Assert.Equal(2, info.Generation);
            Assert.False(info.Ended);
        }

        [Fact]
        public void Rehber_AdimlarHesaptanOkunur_BitinceKapanir()
        {
            var a = new AccountProgress();
            Assert.True(FirstDayGuide.IsActive(a));
            Assert.Equal(0, FirstDayGuide.DoneCount(a));
            a.Record("chop", 1);                 // iş: ilk kez + sayaç
            a.Count("act:water");                // yalnızca sayaç
            Assert.Equal(2, FirstDayGuide.DoneCount(a));
            Assert.False(FirstDayGuide.CloseIfComplete(a));
            a.Record("eat_egg", 1); a.Record("furnish", 1, isWork: false); a.Record("fire", 1, isWork: false); a.Record("chat", 1, isWork: false);
            Assert.Equal(FirstDayGuide.Steps.Count, FirstDayGuide.DoneCount(a));
            Assert.True(FirstDayGuide.CloseIfComplete(a));
            Assert.False(FirstDayGuide.IsActive(a));
            Assert.False(FirstDayGuide.CloseIfComplete(a));   // bir kez
        }

        [Fact]
        public void Rehber_OyuncuKapatabilir()
        {
            var a = new AccountProgress();
            FirstDayGuide.Dismiss(a);
            Assert.False(FirstDayGuide.IsActive(a));
        }
    }
}
