using Thatchwick.Core.Characters;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>20.11.2026 inceleme düzeltmeleri: Güney Yamaçları kuralının bağlanması, +18 paketinin platform kapısı.</summary>
    public class DuzeltmelerTests
    {
        [Fact]
        public void GuneyYamaclari_YasamKatmaniBaglayincaTaninir()
        {
            var world = new WorldState(new GridMap(12, 12));
            var life = new LifeSystems(new ContentDatabase(), world, new Household(), new Population(), new GameClock());
            var plants = new PlantSystem(world);
            Assert.Null(plants.IsSouth);
            life.Attach(plants);
            Assert.NotNull(plants.IsSouth);   // bağlanmazsa zeytin hiçbir yerde, kahve yalnızca serada büyür
            var s = Regions.Slopes;
            Assert.True(plants.IsSouth(new GridPos(s.X + 1, s.Y + 1)));
            Assert.False(plants.IsSouth(new GridPos(Regions.Farm.X + 5, Regions.Farm.Y + 5)));
        }

        [Fact]
        public void YetiskinPaketi_PlatformKapaliysaAcilamaz()
        {
            bool allowed = ContentPacks.AdultAllowed, adult = ContentPacks.Adult;
            try
            {
                ContentPacks.AdultAllowed = false;
                ContentPacks.Adult = true;
                Assert.False(ContentPacks.Adult);                              // telefon derlemesi: ayar ne olursa olsun kapalı
                Assert.True(ContentPacks.IsHiddenAlcohol("item.ale"));
                ContentPacks.AdultAllowed = true;
                Assert.True(ContentPacks.Adult);
            }
            finally { ContentPacks.AdultAllowed = allowed; ContentPacks.Adult = adult; }
        }
    }
}
