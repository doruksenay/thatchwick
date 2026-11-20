using Thatchwick.Core.Mood;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Karakter kartının gösterim kuralları (Project belgesi 68): güç işareti, ihtiyaç bandı, ikon adayları, ayrıntı metni.</summary>
    public class IcSeslerKartTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 1)]
        [InlineData(-9, 1)]
        [InlineData(10, 2)]
        [InlineData(-15, 2)]
        [InlineData(20, 3)]
        [InlineData(-30, 3)]
        public void GucIsaretiDegerinBuyuklugundenGelir(int value, int pips)
        {
            Assert.Equal(pips, InnerVoiceView.Strength(value));
        }

        [Fact]
        public void IhtiyacBandiSayiYerineRenkVerir()
        {
            Assert.Equal(NeedBand.Critical, InnerVoiceView.Band(10f));
            Assert.Equal(NeedBand.Low, InnerVoiceView.Band(20f));
            Assert.Equal(NeedBand.Fair, InnerVoiceView.Band(45f));
            Assert.Equal(NeedBand.Good, InnerVoiceView.Band(88f));
        }

        [Fact]
        public void IkonAdaylariEsAileVeYedekSirasiyla()
        {
            // İhtiyaç iç sesi ihtiyacın ikonunu taşır.
            Assert.Equal("need_hunger", InnerVoiceView.IconKeys(MoodCatalog.Hungry, -15)[0]);
            // Yara ailesi tek ikon.
            Assert.Contains("moodlet_injured", InnerVoiceView.IconKeys(MoodCatalog.InjuredAxe, -10));
            // Ön ekli kimlik ön eksiz hâliyle de aranır.
            Assert.Contains("moodlet_festival", InnerVoiceView.IconKeys("mood.festival", 8));
            Assert.Contains("moodlet_mint_fresh", InnerVoiceView.IconKeys("eff_mint_fresh", 4));
            // Her listenin sonunda işarete göre genel yedek.
            var joy = InnerVoiceView.IconKeys("hic_yok", 5);
            Assert.Equal(InnerVoiceView.FallbackJoy, joy[joy.Count - 1]);
            var gloom = InnerVoiceView.IconKeys("hic_yok", -5);
            Assert.Equal(InnerVoiceView.FallbackGloom, gloom[gloom.Count - 1]);
        }

        [Fact]
        public void RuhHaliAyrintisiSonrakiBasamagaUzakligiSoyler()
        {
            var t = InnerVoiceView.MoodDetail(19);   // İyi; Mutlu 30'da
            Assert.Equal("ui.mood.card.total_next", t.Key);
            Assert.Contains(t.Args, a => a.name == "need" && (int)a.value == 11);
            Assert.Equal("ui.mood.card.total_top", InnerVoiceView.MoodDetail(40).Key);
            Assert.Equal("ui.mood.card.total_next", InnerVoiceView.MoodDetail(-50).Key);
        }

        [Fact]
        public void KoşulluIcSesBitmezVeYanipSonmez()
        {
            var c = new Characters.Character(1, "Test", new World.GridPos(0, 0)) { MoodEnabled = true };
            c.AddMoodlet(MoodCatalog.Hungry);
            c.AddMoodlet(MoodCatalog.GoodMeal);
            ActiveMoodlet hungry = null, meal = null;
            foreach (var m in c.Mood.Active)
            {
                if (m.Def.Id == MoodCatalog.Hungry) hungry = m;
                if (m.Def.Id == MoodCatalog.GoodMeal) meal = m;
            }
            Assert.NotNull(hungry);
            Assert.False(InnerVoiceView.Expiring(hungry));
            Assert.Equal("ui.mood.card.while_condition", InnerVoiceView.RemainingText(hungry).Key);
            Assert.NotNull(meal);
            Assert.False(InnerVoiceView.Expiring(meal));   // yeni eklendi, süresi dolu
            Assert.Equal("ui.mood.card.hours_left", InnerVoiceView.RemainingText(meal).Key);
        }
    }
}
