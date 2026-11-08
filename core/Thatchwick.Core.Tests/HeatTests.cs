using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class HeatTests
    {
        private static readonly TimeConfig Time = TimeConfig.Default;

        private static void Box(GridMap m, int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++)
            {
                m.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal), EdgeKind.Wall);
            }
            for (int y = y0; y <= y1; y++)
            {
                m.SetEdge(new Edge(x0, y, EdgeAxis.Vertical), EdgeKind.Wall);
                m.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical), EdgeKind.Wall);
            }
        }

        private static ItemDef Hearth() => new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f };
        private static ItemDef Wood() => new ItemDef { Id = "item.firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2 };

        // Kış ortasında (gün 35) başlar.
        private const double Winter = 35.0;

        private static (WorldState w, HeatSystem h) Setup(int size = 3)
        {
            var map = new GridMap(14, 14);
            Box(map, 2, 2, 1 + size, 1 + size);
            var w = new WorldState(map);
            return (w, new HeatSystem(w, Time, null, Winter));
        }

        private static void Run(HeatSystem h, double days, double start)
        {
            for (double t = 0; t < days; t += 0.05) h.Tick(0.05, start + t);
        }

        [Fact]
        public void DisSicaklik_MevsimEgrisi()
        {
            Assert.Equal(-8.0, (double)Climate.OutdoorTemperature(Time, 35), 2);
            Assert.Equal(20.0, (double)Climate.OutdoorTemperature(Time, 15), 2);
            Assert.Equal(10.0, (double)Climate.OutdoorTemperature(Time, 5), 2);
            Assert.Equal(8.0, (double)Climate.OutdoorTemperature(Time, 25), 2);
            Assert.Equal(1.0, (double)Climate.OutdoorTemperature(Time, 0), 2);   // kıştan ilkbahara geçiş
            Assert.Equal((double)Climate.OutdoorTemperature(Time, 3), (double)Climate.OutdoorTemperature(Time, 43), 3);
        }

        [Fact]
        public void IsitilmayanOda_DisSicakligaYaklasir()
        {
            var (w, h) = Setup();
            var inside = new GridPos(3, 3);
            Run(h, 0.3, Winter);
            float t1 = h.TemperatureAt(inside);
            Run(h, 15, Winter);
            Assert.InRange(h.TemperatureAt(inside), -8.5f, -7.5f);
            Assert.Equal(-8.0, (double)h.TemperatureAt(new GridPos(10, 10)), 1);
            Assert.True(t1 <= h.OutdoorTemperature + 8f);
        }

        [Fact]
        public void YananOcak_OdayiBelirgin_Isitir()
        {
            var (w, h) = Setup();
            var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
            var house = new Household();
            house.AddItem("item.firewood", 3);
            Assert.True(h.TryStoke(hearth.Id, house));
            Assert.True(h.TryStoke(hearth.Id, house));
            Assert.True(h.TryStoke(hearth.Id, house));
            Run(h, 1.5, Winter);
            Assert.True(h.TemperatureAt(new GridPos(3, 3)) > 0f, $"oda {h.TemperatureAt(new GridPos(3, 3))}");
            Assert.True(h.TemperatureAt(new GridPos(10, 10)) < -7.5f);
        }

        [Fact]
        public void Yakit_Tukenir_OdaTekrarSogur()
        {
            var (w, h) = Setup();
            var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
            var house = new Household();
            house.AddItem("item.firewood", 1);
            h.TryStoke(hearth.Id, house);
            Run(h, 0.6, Winter);
            Assert.False(hearth.IsLit);
            float warm = h.TemperatureAt(new GridPos(3, 3));
            Run(h, 8, Winter + 0.6);
            Assert.True(h.TemperatureAt(new GridPos(3, 3)) < warm - 3f);
        }

        [Fact]
        public void YakitTuketimi_KutukBasinaYarimGun()
        {
            var (w, h) = Setup();
            var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
            var house = new Household();
            house.AddItem("item.firewood", 2);
            h.TryStoke(hearth.Id, house);
            Assert.Equal(0.5, hearth.FuelDays, 3);
            Run(h, 0.25, Winter);
            Assert.InRange(hearth.FuelDays, 0.24, 0.26);
        }

        [Fact]
        public void Yakit_DolunculaVeOdunYoksaAtilmaz()
        {
            var (w, h) = Setup();
            var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
            var house = new Household();
            Assert.False(h.TryStoke(hearth.Id, house));          // odun yok
            house.AddItem("item.firewood", 10);
            Assert.True(h.TryStoke(hearth.Id, house));
            Assert.True(h.TryStoke(hearth.Id, house));
            Assert.True(h.TryStoke(hearth.Id, house));           // 1.5 gün = tavan
            Assert.False(h.TryStoke(hearth.Id, house));
            Assert.Equal(7, house.Count("item.firewood"));
        }

        [Fact]
        public void BuyukOda_AyniOcakla_DahaSoguk()
        {
            var (w1, h1) = Setup(3);
            var (w2, h2) = Setup(6);
            var hh = new Household(); hh.AddItem("item.firewood", 40);
            var a = w1.PlaceItem(Hearth(), new GridPos(3, 3));
            var b = w2.PlaceItem(Hearth(), new GridPos(3, 3));
            for (int i = 0; i < 8; i++) { h1.TryStoke(a.Id, hh); h2.TryStoke(b.Id, hh); Run(h1, 0.5, Winter + i * 0.5); Run(h2, 0.5, Winter + i * 0.5); }
            Assert.True(h1.TemperatureAt(new GridPos(3, 3)) > h2.TemperatureAt(new GridPos(3, 3)) + 3f);
        }

        [Fact]
        public void Pencere_KapidanVeDuvardanDahaCokIsiKacirir()
        {
            float Final(EdgeKind kind)
            {
                var (w, h) = Setup();
                w.Map.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), kind);
                var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
                var hh = new Household(); hh.AddItem("item.firewood", 40);
                for (int i = 0; i < 10; i++) { h.TryStoke(hearth.Id, hh); Run(h, 0.5, Winter + i * 0.5); }
                return h.TemperatureAt(new GridPos(3, 3));
            }
            float wall = Final(EdgeKind.Wall), door = Final(EdgeKind.Door), window = Final(EdgeKind.Window);
            Assert.True(wall > door);
            Assert.True(door > window);
        }

        [Fact]
        public void DuvarKalkinca_OdaDisMekanaDoner_ZeminSogur()
        {
            var (w, h) = Setup();
            var hearth = w.PlaceItem(Hearth(), new GridPos(3, 3));
            var hh = new Household(); hh.AddItem("item.firewood", 40);
            for (int i = 0; i < 4; i++) { h.TryStoke(hearth.Id, hh); Run(h, 0.5, Winter + i * 0.5); }
            Assert.True(h.TemperatureAt(new GridPos(3, 3)) > 0f);
            w.Map.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.None);   // boşluk: oda dışa açıldı
            Assert.Equal((double)h.OutdoorTemperature, (double)h.TemperatureAt(new GridPos(3, 3)), 2);
        }

        // ---- karakter ve soğuk ----

        [Fact]
        public void Soguk_IsinmayiDusurur_KonforluOrtamdaToparlar()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            c.AmbientTemperature = -8f;
            c.Tick(0.5);
            Assert.True(c.Needs.Get(NeedType.Warmth) < 70f);
            c.AmbientTemperature = 18f;
            c.Tick(1.0);
            Assert.Equal(100.0, (double)c.Needs.Get(NeedType.Warmth), 1);
        }

        [Fact]
        public void KonforluOrtam_IsinmayiDusurmez()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            c.Tick(5);
            Assert.Equal(100.0, (double)c.Needs.Get(NeedType.Warmth), 1);
        }

        [Fact]
        public void UzunSureDonma_Oldurur()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            c.AmbientTemperature = -8f;
            int died = 0;
            c.Died += _ => died++;
            for (int i = 0; i < 100 && !c.IsDead; i++) c.Tick(0.1);
            Assert.True(c.IsDead);
            Assert.Equal(DeathCause.Cold, c.CauseOfDeath);
            Assert.Equal(1, died);
        }

        [Fact]
        public void KisaSureDonma_Oldurmez_IsinincaSayacAzalir()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            c.AmbientTemperature = -8f;
            c.Tick(1.5);                       // ısınma sıfırlandı, ~0.8 gün donma
            Assert.False(c.IsDead);
            Assert.True(c.FreezeDays > 0);
            c.AmbientTemperature = 18f;
            c.Tick(1.0);
            Assert.Equal(0.0, c.FreezeDays, 3);
        }

        [Fact]
        public void YasliDogalOlum_SebepYas()
        {
            var c = new Character(1, "A", new GridPos(0, 0));
            c.SetAge(143.9);
            c.Tick(0.2);
            Assert.True(c.IsDead);
            Assert.Equal(DeathCause.OldAge, c.CauseOfDeath);
        }

        // ---- içerik ----

        [Fact]
        public void Icerik_IsiVeKaynakDogrulanir()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "bad1", DisplayName = "x", HeatOutput = -1f });
            db.Items.Register(new ItemDef { Id = "bad2", DisplayName = "x", HeatOutput = 5f, Placement = ItemPlacement.Surface });
            db.Items.Register(new ItemDef { Id = "bad3", DisplayName = "x", Placement = ItemPlacement.Resource, Width = 2 });
            db.Items.Register(new ItemDef { Id = "ok", DisplayName = "x", HeatOutput = 5f });
            var errors = db.Validate();
            Assert.Equal(3, errors.Count);
        }

        [Fact]
        public void Kaynak_YerlestirilmezVeMagazadaSatilir()
        {
            var db = new ContentDatabase();
            db.Items.Register(Wood());
            var w = new WorldState(new GridMap(6, 6));
            var house = new Household(10);
            var svc = new BuildService(w, house, db);
            Assert.True(svc.Buy("item.firewood").Ok);
            Assert.False(svc.PlaceFromInventory("item.firewood", new GridPos(1, 1), 0, out _).Ok);
            Assert.Equal(1, house.Count("item.firewood"));
        }
    }
}
