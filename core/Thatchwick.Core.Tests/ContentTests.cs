using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Tests
{
    public class ContentTests
    {
        private static ItemDef Bed() => new ItemDef
        {
            Id = "item.bed_straw",
            DisplayName = "Saman yatak",
            Interactions = new List<InteractionDef>
            {
                new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60f, DurationDays = 0.3 }
            }
        };

        [Fact]
        public void Kayit_KimligeGoreBulunur_SiraKorunur()
        {
            var reg = new ContentRegistry<ItemDef>();
            reg.Register(Bed());
            reg.Register(new ItemDef { Id = "item.stool", DisplayName = "Tabure" });

            Assert.Equal(2, reg.Count);
            Assert.Equal("item.bed_straw", reg.All[0].Id);
            Assert.True(reg.Contains("item.stool"));
            Assert.False(reg.TryGet("yok", out _));
            Assert.Equal("Saman yatak", reg.Get("item.bed_straw").DisplayName);
        }

        [Fact]
        public void TekrarEdenKimlik_VeBosKimlik_Reddedilir()
        {
            var reg = new ContentRegistry<ItemDef>();
            reg.Register(Bed());
            Assert.Throws<InvalidOperationException>(() => reg.Register(Bed()));
            Assert.Throws<ArgumentException>(() => reg.Register(new ItemDef { Id = " " }));
            Assert.Throws<ArgumentNullException>(() => reg.Register(null!));
            Assert.Throws<KeyNotFoundException>(() => reg.Get("yok"));
        }

        [Fact]
        public void GecerliIcerik_HataVermez()
        {
            var db = new ContentDatabase();
            db.Items.Register(Bed());
            Assert.Empty(db.Validate());
        }

        [Fact]
        public void Dogrulama_HatalariRaporlar()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef
            {
                Id = "item.bad",
                DisplayName = "",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "a", Restore = 0f, DurationDays = 1 },
                    new InteractionDef { Id = "a", Restore = 50f, DurationDays = 0 }
                }
            });

            var errors = db.Validate();
            Assert.Contains(errors, e => e.Contains("görünen ad"));
            Assert.Contains(errors, e => e.Contains("tekrar"));
            Assert.Contains(errors, e => e.Contains("Restore"));
            Assert.Contains(errors, e => e.Contains("süre"));
        }

        [Fact]
        public void Etkilesim_IhtiyaciOyunGunuUzerindenDoyurur()
        {
            // Akıllı nesne: eşya tanımı etkileşimi söyler, karakter sistemi uygular.
            var def = Bed().Interactions[0];
            var needs = new NeedSet();
            needs.Set(NeedType.Energy, 10f);
            needs.Restore(def.Need, def.Restore);
            Assert.Equal(70.0, (double)needs.Get(NeedType.Energy), 3);
        }
    }
}
