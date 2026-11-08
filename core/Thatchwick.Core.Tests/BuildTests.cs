using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class BuildTests
    {
        private static ItemDef Bed() => new ItemDef { Id = "bed", DisplayName = "Yatak", Width = 2, Height = 1,
            Interactions = new List<InteractionDef> { new InteractionDef { Id = "sleep", DisplayName = "Uyu", Need = NeedType.Energy, Restore = 60, DurationDays = 0.3 } } };

        private static ItemDef Table() => new ItemDef { Id = "table", DisplayName = "Masa", Width = 2, Height = 1,
            SurfaceSlots = new List<SurfaceSlotDef>
            {
                new SurfaceSlotDef { Id = "a", U = 0.25f, V = 0.5f },
                new SurfaceSlotDef { Id = "b", U = 0.75f, V = 0.5f }
            } };

        private static ItemDef Candle() => new ItemDef { Id = "candle", DisplayName = "Mum", Placement = ItemPlacement.Surface };

        private static WorldState World() => new WorldState(new GridMap(12, 12));

        [Fact]
        public void CokKaroluEsya_ButunKaroalriKaplar()
        {
            var w = World();
            var bed = w.PlaceItem(Bed(), new GridPos(3, 3));
            Assert.True(w.Map.IsBlocked(new GridPos(3, 3)));
            Assert.True(w.Map.IsBlocked(new GridPos(4, 3)));
            Assert.False(w.Map.IsBlocked(new GridPos(5, 3)));
            Assert.Equal(2, bed.OccupiedTiles().Count());
        }

        [Fact]
        public void Dondurme_BoyutlariDegistirir()
        {
            var w = World();
            var bed = w.PlaceItem(Bed(), new GridPos(3, 3), rotation: 1);
            Assert.Equal(1, bed.Width);
            Assert.Equal(2, bed.Height);
            Assert.True(w.Map.IsBlocked(new GridPos(3, 4)));
            Assert.False(w.Map.IsBlocked(new GridPos(4, 3)));
        }

        [Fact]
        public void HaritaDisinaVeDoluyeKonamaz()
        {
            var w = World();
            Assert.False(w.CanPlaceFloor(Bed(), new GridPos(11, 3), 0).Ok); // ikinci karo dışarıda
            w.PlaceItem(Bed(), new GridPos(3, 3));
            Assert.False(w.CanPlaceFloor(Bed(), new GridPos(4, 3), 0).Ok);  // çakışma
            Assert.Throws<InvalidOperationException>(() => w.PlaceItem(Bed(), new GridPos(4, 3)));
        }

        [Fact]
        public void Tasima_KendiKarolarinaCakismazVeEskiKarolariBosaltir()
        {
            var w = World();
            var bed = w.PlaceItem(Bed(), new GridPos(3, 3));
            // Bir karo kayma: kendi eski karosuyla çakışması sorun değil.
            Assert.True(w.MoveItem(bed.Id, new GridPos(4, 3), 0).Ok);
            Assert.False(w.Map.IsBlocked(new GridPos(3, 3)));
            Assert.True(w.Map.IsBlocked(new GridPos(4, 3)));
            Assert.True(w.Map.IsBlocked(new GridPos(5, 3)));
        }

        [Fact]
        public void BasarisizTasima_EsyayiOlduguYerdeBirakir()
        {
            var w = World();
            var bed = w.PlaceItem(Bed(), new GridPos(3, 3));
            w.PlaceItem(Bed(), new GridPos(3, 6));
            var r = w.MoveItem(bed.Id, new GridPos(3, 6), 0);
            Assert.False(r.Ok);
            Assert.Equal(new GridPos(3, 3), bed.Position);
            Assert.True(w.Map.IsBlocked(new GridPos(3, 3)));
        }

        [Fact]
        public void YuzeyDekoru_YuvayaKonurDoluYuvayaKonmaz()
        {
            var w = World();
            var table = w.PlaceItem(Table(), new GridPos(5, 5));
            var c1 = w.PlaceOnSurface(Candle(), table.Id, "a");
            Assert.True(c1.IsDecor);
            Assert.False(w.CanPlaceOnSurface(Candle(), table.Id, "a").Ok);
            Assert.True(w.CanPlaceOnSurface(Candle(), table.Id, "b").Ok);
            Assert.False(w.CanPlaceOnSurface(Candle(), table.Id, "zzz").Ok);
            Assert.False(w.CanPlaceOnSurface(Bed(), table.Id, "b").Ok);       // yer eşyası yüzeye konamaz
            Assert.False(w.CanPlaceFloor(Candle(), new GridPos(1, 1), 0).Ok); // dekor yere konamaz
        }

        [Fact]
        public void Dekor_KaroKaplamaz()
        {
            var w = World();
            var table = w.PlaceItem(Table(), new GridPos(5, 5));
            w.PlaceOnSurface(Candle(), table.Id, "a");
            Assert.Equal(2, w.Map.Width > 0 ? new[] { 5, 6 }.Count(x => w.Map.IsBlocked(new GridPos(x, 5))) : 0);
            Assert.Null(w.GetItemAt(new GridPos(7, 5)));
            Assert.Same(table, w.GetItemAt(new GridPos(6, 5)));
        }

        [Fact]
        public void Masa_Tasininca_DekorlarTakipEder_KaldirilincaBirlikteGider()
        {
            var w = World();
            var table = w.PlaceItem(Table(), new GridPos(5, 5));
            var candle = w.PlaceOnSurface(Candle(), table.Id, "a");
            Assert.True(w.MoveItem(table.Id, new GridPos(2, 2), 0).Ok);
            Assert.Equal(new GridPos(2, 2), candle.Position);

            var returned = w.RemoveItem(table.Id);
            Assert.Equal(new[] { "table", "candle" }, returned.Select(d => d.Id).ToArray());
            Assert.Empty(w.Items);
            Assert.False(w.Map.IsBlocked(new GridPos(2, 2)));
        }

        [Fact]
        public void YuvaKonumu_DonmeyleBirlikteDoner()
        {
            var w = World();
            var table = w.PlaceItem(Table(), new GridPos(5, 5));
            Assert.True(table.TryGetSlotPosition("a", out var x0, out var y0));
            Assert.Equal(5.5, x0, 3);
            Assert.Equal(5.5, y0, 3);

            w.MoveItem(table.Id, new GridPos(5, 5), 1); // 90 derece
            Assert.True(table.TryGetSlotPosition("a", out var x1, out var y1));
            // Masa artık 1 geniş 2 yüksek; "a" yuvası (u=.25) dikeyde alt tarafa düşer.
            Assert.InRange(x1, 5.0, 6.0);
            Assert.InRange(y1, 5.0, 7.0);
            Assert.Equal(5.5, x1, 3);
            Assert.Equal(5.5, y1, 3);
            Assert.True(table.TryGetSlotPosition("b", out var x2, out var y2));
            Assert.Equal(5.5, x2, 3);
            Assert.Equal(6.5, y2, 3);
        }

        [Fact]
        public void CokKaroluYatak_HerKaroNunKomsusundanKullanilir()
        {
            var w = new WorldState(new GridMap(12, 6));
            var bed = w.PlaceItem(Bed(), new GridPos(5, 2));
            var c = new Character(1, "Aldric", new GridPos(0, 2), true);
            var runner = new JobRunner(c, w);
            runner.Command(Job.UseItem(bed.Id, "sleep"));
            runner.Tick(2.0);
            Assert.True(runner.IsIdle);
            Assert.Null(runner.LastFailure);
            Assert.True(c.Needs.Get(NeedType.Energy) >= 99f);
            // Karakter yatağın dolu karolarından birinin komşusunda durmuş olmalı (soldan: x=4).
            Assert.Equal(new GridPos(4, 2), c.Position);
        }

        [Fact]
        public void Mapper_BoyutYerlesimVeYuvalariOkur_GecersizleriReddeder()
        {
            var dto = new ItemDto
            {
                id = "item.table_wood", displayName = "Masa", width = 2, height = 1, price = 40, category = "furniture",
                surfaceSlots = new[] { new SurfaceSlotDto { id = "a", u = 0.25f, v = 0.5f } }
            };
            var db = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(new ContentFileDto { items = new[] { dto } }, db));
            var def = db.Items.Get("item.table_wood");
            Assert.Equal(2, def.Width);
            Assert.Single(def.SurfaceSlots);
            Assert.Equal(40, def.Price);

            var decor = new ItemDto { id = "item.candle", displayName = "Mum", placement = "Surface" };
            var db2 = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(new ContentFileDto { items = new[] { decor } }, db2));
            Assert.Equal(ItemPlacement.Surface, db2.Items.Get("item.candle").Placement);

            var bad = new ItemDto { id = "item.bad", displayName = "X", width = 9, placement = "Sky",
                surfaceSlots = new[] { new SurfaceSlotDto { id = "a", u = 2f, v = 0f } } };
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { bad } }, new ContentDatabase());
            Assert.Contains(errors, e => e.Contains("Sky"));
            Assert.Contains(errors, e => e.Contains("boyut"));
            Assert.Contains(errors, e => e.Contains("0-1"));
        }

        [Fact]
        public void Degisiklik_OlayiTetiklenir()
        {
            var w = World();
            int n = 0;
            w.Changed += () => n++;
            var table = w.PlaceItem(Table(), new GridPos(5, 5));
            w.PlaceOnSurface(Candle(), table.Id, "a");
            w.MoveItem(table.Id, new GridPos(1, 1), 0);
            w.RemoveItem(table.Id);
            Assert.Equal(4, n);
        }
    }
}
