using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Tests
{
    public class ContentMapperTests
    {
        private static ItemDto Bed() => new ItemDto
        {
            id = "item.bed_straw",
            displayName = "Saman yatak",
            interactions = new[]
            {
                new InteractionDto { id = "sleep", displayName = "Uyu", need = "Energy", restore = 60f, durationDays = 0.3 }
            }
        };

        [Fact]
        public void GecerliDosya_YuklenirHataYok()
        {
            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { Bed() } }, db);

            Assert.Empty(errors);
            var bed = db.Items.Get("item.bed_straw");
            Assert.Equal(NeedType.Energy, bed.Interactions.Single().Need);
            Assert.Equal(60f, bed.Interactions.Single().Restore);
        }

        [Fact]
        public void IhtiyacAdi_BuyukKucukHarfeDuyarsiz()
        {
            var dto = Bed();
            dto.interactions[0].need = "energy";
            var db = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(new ContentFileDto { items = new[] { dto } }, db));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Mutluluk")]
        [InlineData("99")]
        public void BilinmeyenIhtiyac_HataVerir_DigerKayitlarYuklenir(string need)
        {
            var bad = Bed();
            bad.id = "item.bad";
            bad.interactions[0].need = need;

            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { bad, new ItemDto { id = "item.ok", displayName = "Tamam" } } }, db);

            Assert.Contains(errors, e => e.Contains("bilinmeyen ihtiyaç"));
            Assert.True(db.Items.Contains("item.ok"));
        }

        [Fact]
        public void TekrarEdenKimlik_HataVerir()
        {
            var db = new ContentDatabase();
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { Bed(), Bed() } }, db);
            Assert.Contains(errors, e => e.Contains("iki kez"));
            Assert.Equal(1, db.Items.Count);
        }

        [Fact]
        public void BosVeyaOkunamayanDosya_HataVerir()
        {
            Assert.NotEmpty(ContentMapper.Apply(null, new ContentDatabase()));
            Assert.NotEmpty(ContentMapper.Apply(new ContentFileDto(), new ContentDatabase()));
        }

        [Fact]
        public void DogrulamaHatalari_YukleyiciyeYansir()
        {
            var bad = Bed();
            bad.interactions[0].restore = 0f;
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { bad } }, new ContentDatabase());
            Assert.Contains(errors, e => e.Contains("Restore"));
        }
    }
}
