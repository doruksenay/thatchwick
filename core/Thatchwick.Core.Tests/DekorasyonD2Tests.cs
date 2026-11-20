using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ev dekorasyonu D2 (Project belgesi 27): tarz ekseni ve süzgeci, "Özenli ev", doldurulan dekor (gerçek ürün,
    // envanterden; bozulur, yenir, boşaltılır; "Kendi emeğimiz"), masa ve sedir örtüsü, pencere içi ve perde, ocak yuvası,
    // perde ve kumaş örtünün ısı etkisi.
    public class DekorasyonD2Tests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private sealed class Kit
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public InteractionEffects Effects;
            public DecorSystem Decor;
            public List<string> Notices = new List<string>();
            public ItemDef D(string id) => Db.Items.Get(id);
        }

        private static Kit Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(20, 20)), House = new Household(5000) };
            k.Build = new BuildService(k.World, k.House, db);
            k.Effects = new InteractionEffects(k.World, k.House, db);
            k.Decor = new DecorSystem(k.World, db) { House = k.House };
            k.Decor.Notify = (t, _) => k.Notices.Add(t);
            // 2..7 x 2..5 iç oda; alt duvarın (3,2) kenarı pencere.
            for (int x = 2; x <= 7; x++) { Wall(k, x, 2, EdgeAxis.Horizontal); Wall(k, x, 6, EdgeAxis.Horizontal); }
            for (int y = 2; y <= 5; y++) { Wall(k, 2, y, EdgeAxis.Vertical); Wall(k, 8, y, EdgeAxis.Vertical); }
            k.World.Map.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Window);
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 5; y++) k.World.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
            return k;
        }

        private static void Wall(Kit k, int x, int y, EdgeAxis axis) => k.World.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall);
        private static readonly GridPos Win = new GridPos(3, 2);     // pencerenin önündeki karo (kenar 0)
        private static readonly GridPos Plain = new GridPos(5, 2);   // düz duvarın önü
        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);
        private static Character Wat() => new Character(1, "Wat", new GridPos(3, 3));

        private ItemInstance Basket(Kit k)
        {
            // Her çağrıda yeni bir masa (oda 6x4; 2x1 masalar 3..6 x 3..5 arasına dizilir).
            var spots = new[] { new GridPos(4, 3), new GridPos(4, 5), new GridPos(6, 3), new GridPos(6, 5), new GridPos(3, 4), new GridPos(5, 4) };
            GridPos at = spots[0];
            foreach (var p in spots) if (k.World.GetItemAt(p) == null && k.World.GetItemAt(new GridPos(p.X + 1, p.Y)) == null) { at = p; break; }
            var table = k.World.PlaceItem(k.D("item.table_wood"), at);
            return k.World.PlaceOnSurface(k.D("item.fruit_basket"), table.Id, "b");
        }

        // ---------- tarz ----------

        [Fact]
        public void Tarz_KatalogSuzgeci_KartaYazar()
        {
            var k = Make();
            var yoruk = ItemCatalog.InCategory(k.Db, "seating", null, null, "Yörük");
            Assert.Contains(yoruk, d => d.Id == "item.sedir");
            Assert.All(yoruk, d => Assert.Equal("Yörük", d.StyleTheme));
            Assert.DoesNotContain(ItemCatalog.InCategory(k.Db, "seating", null, null, "Çatkı"), d => d.Id == "item.sedir");
            Assert.Contains("Tarz: Yörük Odası", ItemCatalog.Describe(k.D("item.sedir")));
            // Her iki 1.0 tarzında da oturma, masa, depo, halı ve süs var.
            foreach (var theme in new[] { "Çatkı", "Yörük" })
                foreach (var cat in new[] { "seating", "surfaces", "storage", "rug", "decor", "decor_wall" })
                    Assert.True(ItemCatalog.InCategory(k.Db, cat, null, null, theme).Count > 0, $"{theme}/{cat} boş");
        }

        [Fact]
        public void Tarz_DortAyniTarzUyumlu_TakimlaBirlikteOzenliEv()
        {
            var k = Make();
            var room = k.World.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.NotNull(room);
            // 4 Yörük eşyası, farklı takımlardan: tarz uyumu var, takım uyumu yok.
            k.World.PlaceItem(k.D("item.floor_cushion"), new GridPos(3, 3));      // Köylü
            k.World.PlaceItem(k.D("item.sedir"), new GridPos(4, 3));              // Zanaatkâr
            k.World.PlaceItem(k.D("item.cicim"), new GridPos(4, 4));              // Zanaatkâr halı
            k.World.PlaceItem(k.D("item.sini_table"), new GridPos(6, 3));         // Köylü
            Assert.Equal("Yörük", RoomMoodSystem.HarmonyTheme(room, k.World));
            Assert.Null(RoomMoodSystem.HarmonySet(room, k.World));

            var c = new Character(1, "Ayşe", new GridPos(3, 4)) { MoodEnabled = true };
            c.SetAge(30);
            new RoomMoodSystem(k.World).Tick(1, new[] { c });
            Assert.True(c.Mood.Has(MoodCatalog.Harmony));
            Assert.False(c.Mood.Has(MoodCatalog.HarmonyFine));

            // İki Zanaatkâr Yörük daha: hem takım hem tarz 4+ → "Özenli ev".
            k.World.PlaceItem(k.D("item.kilim_small"), new GridPos(2, 5));
            k.World.PlaceItem(k.D("item.yukluk"), new GridPos(6, 5));
            Assert.Equal("Zanaatkâr", RoomMoodSystem.HarmonySet(room, k.World));
            var c2 = new Character(2, "Orhan", new GridPos(3, 4)) { MoodEnabled = true };
            c2.SetAge(30);
            new RoomMoodSystem(k.World).Tick(1, new[] { c2 });
            Assert.True(c2.Mood.Has(MoodCatalog.HarmonyFine));
            Assert.False(c2.Mood.Has(MoodCatalog.Harmony));
        }

        // ---------- doldurulan dekor ----------

        [Fact]
        public void Doldur_BosGelir_EnvanterdenGercekUrunIster()
        {
            var k = Make();
            var basket = Basket(k);
            var fill = Inter(basket.Def, "fill");
            var wat = Wat();
            Assert.False(basket.HasContents);
            Assert.Equal("Envanterde elma ya da benzeri yok (0/6).", k.Effects.CanStart(wat, basket, fill));
            Assert.Equal("Boş (0/6)", DecorSystem.FillLabel(basket, k.Db));

            k.House.AddItem("item.apple", 4);
            k.House.AddItem("item.pear", 5);
            Assert.Null(k.Effects.CanStart(wat, basket, fill));
            k.Effects.Apply(wat, basket, fill);
            Assert.Equal(6, basket.Contents.Count);
            Assert.Equal(0, k.House.Count("item.apple"));                       // tercih sırası: önce elma
            Assert.Equal(3, k.House.Count("item.pear"));
            Assert.Equal("Meyve sepeti zaten dolu.", k.Effects.CanStart(wat, basket, fill));
            Assert.Contains("Kendi emeğimiz", DecorSystem.FillLabel(basket, k.Db));
        }

        [Fact]
        public void Doldur_OnceKendiUrunu_PazarMaliEkAlmaz()
        {
            var k = Make();
            var basket = Basket(k);
            k.House.AddItem("item.apple", 6);
            k.House.MarkBought("item.apple", 4);                                // 4 pazar malı, 2 kendi ürünü
            Assert.Equal(2, k.House.OwnCount("item.apple"));
            DecorSystem.Fill(basket, k.House, k.Db);
            Assert.Equal(6, basket.Contents.Count);
            Assert.Equal(2, basket.Contents.Count(c => c.Own));
            Assert.Equal(0, k.House.BoughtCount("item.apple"));
            Assert.DoesNotContain("Kendi emeğimiz", DecorSystem.FillLabel(basket, k.Db));

            // Güzellik: tam dolu ama karışık → taban 5; hepsi kendi ürünü olsaydı +2.
            Assert.Equal(5.0, DecorSystem.FillBeauty(basket, k.Db), 3);
            var own = Basket(k);
            k.House.AddItem("item.pear", 6);
            DecorSystem.Fill(own, k.House, k.Db);
            Assert.Equal(5.0 + Decor.OwnWorkBonus, DecorSystem.FillBeauty(own, k.Db), 3);

            // Boşaltınca pazar malı yine "alınmış" sayılır.
            var (back, gone) = DecorSystem.EmptyInto(basket, k.House, k.Db);
            Assert.Equal((6, 0), (back, gone));
            Assert.Equal(6, k.House.Count("item.apple"));
            Assert.Equal(4, k.House.BoughtCount("item.apple"));
        }

        [Fact]
        public void Icerik_OdaSicakligindaBozulur_BayatCuruk_GuzellikDuser_Bildirim()
        {
            var k = Make();
            var basket = Basket(k);
            k.House.AddItem("item.cherry", 6);                                  // kiraz 5 günde bozulur
            DecorSystem.Fill(basket, k.House, k.Db);
            double full = DecorSystem.FillBeauty(basket, k.Db);

            k.Decor.TemperatureAt = _ => 18f;                                   // normal hız
            k.Decor.Tick(3.0);                                                  // %60 → bayat
            Assert.Equal(Decor.Stale, DecorSystem.WorstFreshness(basket, k.Db));
            var filth = new Thatchwick.Core.Farmyard.FilthService(k.World, k.Db) { IsRotten = it => DecorSystem.IsRottenFill(it, k.Db) };
            filth.Refresh();
            Assert.DoesNotContain(filth.Sources, s => s.ItemId == basket.Id);    // bayat kokmaz
            Assert.Contains("bayat", DecorSystem.FillLabel(basket, k.Db));
            Assert.True(DecorSystem.FillBeauty(basket, k.Db) < full);

            k.Decor.TemperatureAt = _ => 30f;                                   // sıcak oda: 1,4 kat
            k.Decor.Tick(1.5);                                                  // 3 + 2,1 = 5,1 ≥ 5 → çürük
            Assert.Equal(Decor.Rotten, DecorSystem.WorstFreshness(basket, k.Db));
            Assert.Equal(Decor.RottenBeauty, DecorSystem.FillBeauty(basket, k.Db), 3);
            Assert.Contains(k.Notices, n => n.Contains("çürüdü"));
            filth.Refresh();                                                    // çürük sepet koku ve sinek kaynağıdır
            Assert.Contains(filth.Sources, s => s.ItemId == basket.Id && s.Kind == Thatchwick.Core.Farmyard.FilthService.DecorKind && s.Tier == 1);

            // Çürük yenmez; boşaltınca atılır.
            var wat = Wat();
            Assert.Equal("İçindekiler çürümüş; önce boşalt.", k.Effects.CanStart(wat, basket, Inter(basket.Def, "eat_one")));
            k.Effects.Apply(wat, basket, Inter(basket.Def, "empty"));
            Assert.False(basket.HasContents);
            Assert.Equal(0, k.House.Count("item.cherry"));
        }

        [Fact]
        public void Icerik_Yenir_OnceEnBayati_SepetBosalinca()
        {
            var k = Make();
            var board = k.World.PlaceOnSurface(k.D("item.cheese_board"), k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 3)).Id, "a");
            var eat = Inter(board.Def, "eat_one");
            Assert.True(eat.RestoresNeed);
            var wat = Wat();
            Assert.Equal("Peynir tahtası boş.", k.Effects.CanStart(wat, board, eat));
            k.House.AddItem("item.kasar", 1);
            k.Effects.Apply(wat, board, Inter(board.Def, "fill"));
            Assert.Single(board.Contents);
            Assert.Null(k.Effects.CanStart(wat, board, eat));
            k.Effects.Apply(wat, board, eat);
            Assert.False(board.HasContents);
            Assert.Equal(0, k.House.Count("item.kasar"));                       // yendi, envantere dönmedi
        }

        [Fact]
        public void Icerik_EnvanterdekiEnEskiPartininYasiylaGirer()
        {
            var k = Make();
            var basket = Basket(k);
            k.House.AddItem("item.apple", 6);
            k.Effects.FoodAge = id => id == "item.apple" ? 10.0 : 0.0;
            k.Effects.Apply(Wat(), basket, Inter(basket.Def, "fill"));
            Assert.All(basket.Contents, c => Assert.Equal(10.0, c.Age, 3));
        }

        [Fact]
        public void Kaldirma_Satis_Cokme_IcindekilerinAkibeti()
        {
            var k = Make();
            // Envantere kaldırma: içindekiler de döner.
            var basket = Basket(k);
            k.House.AddItem("item.apple", 6);
            DecorSystem.Fill(basket, k.House, k.Db);
            Assert.True(k.Build.StoreToInventory(basket.Id).Ok);
            Assert.Equal(6, k.House.Count("item.apple"));
            Assert.Equal(1, k.House.Count("item.fruit_basket"));

            // Satış: sepet satılır, içindekiler envantere döner.
            var b2 = Basket(k);
            DecorSystem.Fill(b2, k.House, k.Db);
            Assert.True(k.Build.SellPlaced(b2.Id).Ok);
            Assert.Equal(6, k.House.Count("item.apple"));

            // Çöken raftan düşen sepetin yiyeceği yere dökülür.
            var shelf = k.World.PlaceItem(k.D("item.pantry_shelf"), new GridPos(6, 4));
            var b3 = k.World.PlaceOnSurface(k.D("item.vegetable_crate"), shelf.Id, "a");
            k.House.AddItem("item.turnip", 6);
            DecorSystem.Fill(b3, k.House, k.Db);
            var report = k.Decor.Collapse(shelf);
            Assert.Equal(6, report.Spilled);
            Assert.Contains("6 yiyecek yere döküldü", report.Text);
            Assert.Equal(0, k.House.Count("item.turnip"));
        }

        [Fact]
        public void Kayit_IcerikYasKendiUrunuVeAlinmisAdetGeriGelir()
        {
            var k = Make();
            var basket = Basket(k);
            k.House.AddItem("item.apple", 3);
            k.House.AddItem("item.pear", 5);
            k.House.MarkBought("item.pear", 5);
            DecorSystem.Fill(basket, k.House, k.Db);
            k.Decor.Tick(2.0);
            k.House.AddItem("item.pear", 2);
            k.House.MarkBought("item.pear", 1);

            var data = SaveGame.Capture(new GameClock(), k.World, k.House, new List<Character>(), new Progress.AccountProgress(), null, null);
            var loaded = SaveGame.Load(JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, Json), Json), k.Db);
            var lb = loaded.World.GetItem(basket.Id);
            Assert.Equal(6, lb.Contents.Count);
            Assert.Equal(3, lb.Contents.Count(c => c.Own));
            Assert.All(lb.Contents, c => Assert.Equal(2.0, c.Age, 3));
            Assert.Equal(4, loaded.Household.Count("item.pear"));
            Assert.Equal(3, loaded.Household.BoughtCount("item.pear"));
        }

        // ---------- örtü ----------

        [Fact]
        public void Ortu_MasayaVeSedireSerilir_YuvalariKapatmaz_TekOrtu()
        {
            var k = Make();
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 3));
            var cloth = k.D("item.tablecloth_linen");
            Assert.True(k.World.CanPlaceOnSurface(cloth, table.Id, "cover").Ok);
            Assert.False(k.World.CanPlaceOnSurface(cloth, table.Id, "a").Ok);              // örtü yalnızca örtü yuvasına
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.candle"), table.Id, "cover").Ok);
            k.World.PlaceOnSurface(cloth, table.Id, "cover");
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.candle"), table.Id, "a").Ok);   // diğer yuvalar açık
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.table_carpet"), table.Id, "cover").Ok);
            Assert.True(table.Def.FindSlot("cover").IsCover);

            var sedir = k.World.PlaceItem(k.D("item.sedir"), new GridPos(4, 5));
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.sedir_cover_kilim"), sedir.Id, "cover").Ok);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.big_cushion"), sedir.Id, "a").Ok);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.candle"), sedir.Id, "a").Ok);   // minder yuvası
        }

        [Fact]
        public void Ortu_KutukSehpayaDaSerilir_KartUyduguEsyalariYazar()
        {
            var k = Make();
            var stump = k.World.PlaceItem(k.D("item.stump_table"), new GridPos(4, 3));
            var cloth = k.D("item.tablecloth_linen");
            Assert.True(k.World.CanPlaceOnSurface(cloth, stump.Id, "cover").Ok);
            Assert.True(ItemCatalog.SurfaceAccepts(stump.Def, cloth));
            Assert.False(ItemCatalog.SurfaceAccepts(k.D("item.hearth_stone"), cloth));
            var where = ItemCatalog.FittingSurfaces(k.Db, cloth);
            Assert.Equal("Kütük sehpa", where[0]);                                   // en ucuzu önce
            Assert.Contains("Tahta masa", where);
            Assert.Contains("Örtü: Kütük sehpa, ", ItemCatalog.Describe(cloth, null, k.Db));
        }

        // ---------- pencere ve ocak ----------

        [Fact]
        public void Pencere_IcTahtasiVePerdeYalnizcaPencereye_SaksiPencereIcine()
        {
            var k = Make();
            var sill = k.D("item.window_sill_board");
            var bad = k.World.CanPlaceOnWall(sill, Plain, 0);
            Assert.False(bad.Ok);
            Assert.Contains("yalnızca pencereye", bad.Reason);
            Assert.False(k.World.CanPlaceOnWall(k.D("item.wall_shelf"), Win, 0).Ok);        // raf pencereye asılmaz
            var board = k.World.PlaceOnWall(sill, Win, 0);
            Assert.Equal(Decor.BandLow, board.WallBand);
            var curtain = k.World.PlaceOnWall(k.D("item.linen_curtain"), Win, 0);           // aynı pencerede orta bant
            Assert.Equal(Decor.BandMid, curtain.WallBand);

            Assert.True(k.World.CanPlaceOnSurface(k.D("item.herb_pot_basil"), board.Id, "a").Ok);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.elderflower_jug"), board.Id, "b").Ok);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.jug"), board.Id, "a").Ok);      // testi pencere içine sığmaz
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.ladle"), board.Id, "a").Ok);    // kanca eşyası
        }

        [Fact]
        public void Pencere_DuvaraDonusunceEsyalariEnvantereDoner()
        {
            var k = Make();
            var board = k.World.PlaceOnWall(k.D("item.window_sill_board"), Win, 0);
            k.World.PlaceOnSurface(k.D("item.herb_pot_basil"), board.Id, "a");
            Assert.True(k.Build.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Wall).Ok);
            Assert.Null(k.World.GetItem(board.Id));
            Assert.Equal(1, k.House.Count("item.window_sill_board"));
            Assert.Equal(1, k.House.Count("item.herb_pot_basil"));
        }

        [Fact]
        public void Ocak_TasOcaginRafiOcakEsyasiAlir()
        {
            var k = Make();
            var hearth = k.World.PlaceItem(k.D("item.hearth_stone"), new GridPos(5, 4));
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.copper_ewer"), hearth.Id, "b").Ok);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.big_cushion"), hearth.Id, "b").Ok);
        }

        // ---------- ısı ----------

        [Fact]
        public void Isi_PerdeVeKumasOrtuKenarKaybiniAzaltir()
        {
            var k = Make();
            var win = new Edge(3, 2, EdgeAxis.Horizontal);
            var wall = new Edge(5, 2, EdgeAxis.Horizontal);
            Assert.Equal(1f, DecorSystem.EdgeInsulation(k.World, win));
            var curtain = k.World.PlaceOnWall(k.D("item.wool_curtain"), Win, 0);
            Assert.Equal(0.6, (double)DecorSystem.EdgeInsulation(k.World, win), 3);
            k.World.PlaceOnWall(k.D("item.painted_cloth"), Plain, 0);                      // 2 karo genişliğinde
            Assert.Equal(0.9, (double)DecorSystem.EdgeInsulation(k.World, wall), 3);
            Assert.Equal(0.9, (double)DecorSystem.EdgeInsulation(k.World, new Edge(6, 2, EdgeAxis.Horizontal)), 3);
            curtain.Condition = ItemCondition.Damaged;                                       // yırtık perde işe yaramaz
            Assert.Equal(1f, DecorSystem.EdgeInsulation(k.World, win));
            Assert.Contains("Isı kaybını azaltır (%40)", ItemCatalog.Describe(k.D("item.wool_curtain")));
        }
    }
}
