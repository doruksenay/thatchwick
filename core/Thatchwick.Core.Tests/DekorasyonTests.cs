using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Ev dekorasyonu D1 (Project belgesi 27, kararlar 18.11.2026): yuva türü ve boyutu, yük ve raf çökmesi, hasar ve
    // onarım, üç duvar bandı, geniş duvar eşyası, uzun mobilya, tavan katmanı, sınırsız halı, güzellik puanı, ayna şartı.
    public class DekorasyonTests
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
            public List<Character> Members = new List<Character>();
            public ItemDef D(string id) => Db.Items.Get(id);
        }

        private static Kit Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var k = new Kit { Db = db, World = new WorldState(new GridMap(20, 20)), House = new Household(5000) };
            k.Build = new BuildService(k.World, k.House, db);
            k.Effects = new InteractionEffects(k.World, k.House, db);
            k.Decor = new DecorSystem(k.World, db) { House = k.House, Members = () => k.Members };
            k.Decor.Notify = (t, _) => k.Notices.Add(t);
            // 2..7 x 2..5 iç oda (6x4 = 24 karo), duvarlarla kapalı.
            for (int x = 2; x <= 7; x++) { Wall(k, x, 2, EdgeAxis.Horizontal); Wall(k, x, 6, EdgeAxis.Horizontal); }
            for (int y = 2; y <= 5; y++) { Wall(k, 2, y, EdgeAxis.Vertical); Wall(k, 8, y, EdgeAxis.Vertical); }
            for (int x = 2; x <= 7; x++) for (int y = 2; y <= 5; y++) k.World.Map.SetFloor(new GridPos(x, y), FloorType.Wood);
            return k;
        }

        private static void Wall(Kit k, int x, int y, EdgeAxis axis) => k.World.Map.SetEdge(new Edge(x, y, axis), EdgeKind.Wall);

        // Odanın alt duvarı (y=2 yatay kenarlar): karo (x,2)'nin 0. kenarı.
        private static readonly GridPos A = new GridPos(3, 2);

        private static InteractionDef Inter(ItemDef def, string id) => def.Interactions.First(i => i.Id == id);

        // ---------- yuva türü ve boyutu ----------

        [Fact]
        public void Yuva_RafaYalnizcaRafaUyumluEsyaGirer_BoyutSiniri()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), A, 0);
            Assert.Equal(Decor.BandMid, shelf.WallBand);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.candle"), shelf.Id, "a").Ok);
            var ladle = k.World.CanPlaceOnSurface(k.D("item.ladle"), shelf.Id, "a");          // kepçe yalnızca kancaya
            Assert.False(ladle.Ok);
            Assert.Contains("yalnızca raf", ladle.Reason);
            var jug = k.World.CanPlaceOnSurface(k.D("item.jug"), shelf.Id, "a");              // testi orta, yuva küçük
            Assert.False(jug.Ok);
            Assert.Contains("büyük (O > K)", jug.Reason);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.morris_board"), shelf.Id, "a").Ok);   // oyun tahtası yalnızca masada

            var rail = k.World.PlaceOnWall(k.D("item.peg_rail"), new GridPos(4, 2), 0);
            Assert.Equal(Decor.BandLow, rail.WallBand);                                         // ilk bandı alt
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.ladle"), rail.Id, "a").Ok);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.horn_lantern"), rail.Id, "b").Ok);  // fener kancaya da asılır
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.candle"), rail.Id, "a").Ok);       // mum kancaya asılmaz

            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.morris_board"), table.Id, "a").Ok);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.storage_pot_small"), table.Id, "a").Ok);  // büyük küp orta yuvaya girmez
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.storage_pot_small"), table.Id, "b").Ok);   // ortadaki büyük yuva
        }

        [Fact]
        public void Yuk_AsimEngellenmez_YukHesaplanir()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), A, 0);      // taşır 4
            k.World.PlaceOnSurface(k.D("item.herb_pot_basil"), shelf.Id, "a");    // 2
            Assert.False(k.World.WouldOverload(k.D("item.herb_pot_thyme"), shelf.Id));   // 2 + 2 = 4
            k.World.PlaceOnSurface(k.D("item.herb_pot_thyme"), shelf.Id, "b");
            Assert.Equal(4, k.World.LoadOf(shelf.Id));
            Assert.False(k.World.IsOverloaded(shelf));
            Assert.Contains("Taşır 4", ItemCatalog.Describe(k.D("item.wall_shelf")));
            Assert.Contains("orta/alt bant", ItemCatalog.Describe(k.D("item.wall_shelf")));
            Assert.Contains("boyut O · yük 3", ItemCatalog.Describe(k.D("item.jug")));
        }

        // ---------- raf çökmesi ----------

        [Theory]
        [InlineData(18, 18, 0.0)]
        [InlineData(19, 18, 0.111)]
        [InlineData(10, 9, 0.222)]
        [InlineData(36, 18, 1.0)]
        public void CokmeSansi_AsimOranininIkiKati_EnAzYuzdeOn(int load, int max, double expected)
        {
            Assert.Equal(expected, DecorSystem.CollapseChance(load, max), 3);
        }

        private static (Kit k, ItemInstance shelf) OverloadedLongShelf()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.long_shelf_iron"), A, 0);   // 2 karo, taşır 18
            k.World.PlaceOnSurface(k.D("item.copper_pot"), shelf.Id, "a");          // metal 5
            k.World.PlaceOnSurface(k.D("item.storage_pot_small"), shelf.Id, "b");   // kil 8
            k.World.PlaceOnSurface(k.D("item.jug"), shelf.Id, "c");                 // kil 3
            Assert.True(k.World.WouldOverload(k.D("item.oil_jug"), shelf.Id));
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.oil_jug"), shelf.Id, "d").Ok);   // aşım engellenmez
            k.World.PlaceOnSurface(k.D("item.oil_jug"), shelf.Id, "d");             // kil 3 → 19/18
            Assert.True(k.World.IsOverloaded(shelf));
            return (k, shelf);
        }

        [Fact]
        public void Cokme_ZarTutmazsaRafDurur_TutarsaCoker()
        {
            var (k, shelf) = OverloadedLongShelf();
            k.Decor.Roll = () => 0.5;            // şans %11: tutmaz
            k.Decor.Tick(1.0);
            Assert.NotNull(k.World.GetItem(shelf.Id));
            Assert.Empty(k.Notices);

            var wat = new Character(1, "Wat", new GridPos(5, 4)) { MoodEnabled = true };
            var outside = new Character(2, "Hob", new GridPos(12, 12)) { MoodEnabled = true };
            k.Members.Add(wat); k.Members.Add(outside);
            k.Decor.Roll = () => 0.05;
            k.Decor.Tick(DecorSystem.HourDays + 1e-6);

            Assert.Null(k.World.GetItem(shelf.Id));                       // duvar rafı duvardan koptu
            Assert.Empty(k.World.GetDecorOn(shelf.Id));
            var pile = k.World.GetRugAt(A);
            Assert.NotNull(pile);
            Assert.Equal(Decor.Debris, pile.Def.Id);
            Assert.Equal(3, pile.Contents.Count(c => c.ItemId == Decor.BrokenPottery));                   // üç kil kap kırıldı
            Assert.Contains(pile.Contents, c => c.ItemId == "item.copper_pot" && c.Condition == ItemCondition.Damaged);
            Assert.Contains(pile.Contents, c => c.ItemId == "item.long_shelf_iron" && c.Condition == ItemCondition.Damaged);
            Assert.True(k.World.Map.IsWalkable(A));                        // döküntü yolu kapatmaz
            Assert.Single(k.Notices);
            Assert.Contains("çöktü", k.Notices[0]);
            Assert.Contains("3 eşya kırıldı", k.Notices[0]);
            Assert.True(wat.Mood.Has(DecorSystem.ScareMoodId));
            Assert.False(outside.Mood.Has(DecorSystem.ScareMoodId));
        }

        [Fact]
        public void Dokuntu_Topla_HasarliEnvantere_SatisDegeriDuser_Onarim()
        {
            var (k, shelf) = OverloadedLongShelf();
            k.Decor.Roll = () => 0.0;
            k.Decor.CheckOverloads();
            var pile = k.World.GetRugAt(A);
            var collect = Inter(pile.Def, "collect");
            var wat = new Character(1, "Wat", new GridPos(3, 3));
            Assert.Null(k.Effects.CanStart(wat, pile, collect));
            k.Effects.Apply(wat, pile, collect);
            Assert.Null(k.World.GetItem(pile.Id));                         // yığın kalktı
            Assert.Equal(3, k.House.Count(Decor.BrokenPottery));
            Assert.Equal(1, k.House.DamagedCount("item.copper_pot"));
            Assert.Equal(1, k.House.DamagedCount("item.long_shelf_iron"));

            // Satış değeri: hasarlı yarı, onarılmış dörtte üç (bakır tencere 40 akçe, satış oranı 0,5).
            var pot = k.D("item.copper_pot");
            Assert.Equal(20, k.Build.SellValue(pot, ItemCondition.Sound));
            Assert.Equal(10, k.Build.SellValue(pot, ItemCondition.Damaged));
            Assert.Equal(15, k.Build.SellValue(pot, ItemCondition.Repaired));
            int akce = k.House.Akce;
            Assert.True(k.Build.SellFromInventory("item.copper_pot").Ok);  // yalnız hasarlısı var
            Assert.Equal(akce + 10, k.House.Akce);

            // Marangoz tezgâhında onarım: tahta 1 + çivi 2.
            var bench = k.D("item.workbench");
            var repair = Inter(bench, "repair_wood");
            var benchItem = k.World.PlaceItem(bench, new GridPos(10, 10));
            Assert.Contains("Tahta", k.Effects.CanStart(wat, benchItem, repair));
            k.House.AddItem("item.plank", 1);
            k.House.AddItem("item.nails", 2);
            Assert.Null(k.Effects.CanStart(wat, benchItem, repair));
            k.Effects.Apply(wat, benchItem, repair);
            Assert.Equal(0, k.House.DamagedCount("item.long_shelf_iron"));
            Assert.Equal(1, k.House.RepairedCount("item.long_shelf_iron"));
            Assert.Equal("Onarılacak hasarlı ahşap eşya yok.", k.Effects.CanStart(wat, benchItem, repair));

            // Onarılmış raf yeniden asılır, durumu taşınır; değeri kalıcı %75.
            Assert.True(k.Build.PlaceWallFromInventory("item.long_shelf_iron", new GridPos(5, 2), 0, out var hung).Ok);
            Assert.Equal(ItemCondition.Repaired, hung.Condition);
            Assert.Equal((int)System.Math.Floor(60 * 0.5 * 0.75), k.Build.SellValue(hung));
        }

        [Fact]
        public void Cokme_YerYuzeyiHasarliKalir_YuvalariKapanir_YerindeOnarilir()
        {
            var k = Make();
            var stump = k.World.PlaceItem(k.D("item.stump_table"), new GridPos(4, 4));   // taşır 6, tek orta yuva
            var slot = stump.Def.SurfaceSlots[0].Id;
            k.World.PlaceOnSurface(k.D("item.copper_pot"), stump.Id, slot);                // 5: sığar
            Assert.False(k.World.IsOverloaded(stump));
            k.World.RemoveItem(k.World.GetDecorOn(stump.Id)[0].Id);
            k.World.PlaceOnSurface(k.D("item.serving_tray"), stump.Id, slot);              // 3: sığar
            Assert.Null(k.Decor.Collapse(null));
            var report = k.Decor.Collapse(stump);
            Assert.Equal(1, report.Damaged);
            Assert.Equal(ItemCondition.Damaged, stump.Condition);
            Assert.NotNull(k.World.GetItem(stump.Id));                                     // yer eşyası yerinde kalır
            var blocked = k.World.CanPlaceOnSurface(k.D("item.candle"), stump.Id, slot);
            Assert.False(blocked.Ok);
            Assert.Contains("hasarlı", blocked.Reason);

            var bench = k.World.PlaceItem(k.D("item.workbench"), new GridPos(10, 10));
            k.House.AddItem("item.plank", 1); k.House.AddItem("item.nails", 2);
            var wat = new Character(1, "Wat", new GridPos(3, 3));
            var repair = Inter(bench.Def, "repair_wood");
            Assert.Null(k.Effects.CanStart(wat, bench, repair));                          // envanterde yok, yerdeki onarılır
            k.Effects.Apply(wat, bench, repair);
            Assert.Equal(ItemCondition.Repaired, stump.Condition);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.candle"), stump.Id, slot).Ok);
        }

        [Fact]
        public void Envanter_DurumSirasi_OnceSaglamAlinir_KayitGeriYukler()
        {
            var h = new Household();
            h.AddItem("x", 2);
            h.AddItem("x", 1, ItemCondition.Damaged);
            Assert.Equal(3, h.Count("x"));
            Assert.Equal(2, h.SoundCount("x"));
            Assert.True(h.TryTakeItem("x", out var c1)); Assert.Equal(ItemCondition.Sound, c1);
            Assert.True(h.TryTakeItem("x", out var c2)); Assert.Equal(ItemCondition.Sound, c2);
            Assert.True(h.TryTakeItem("x", out var c3)); Assert.Equal(ItemCondition.Damaged, c3);
            Assert.Equal(0, h.DamagedCount("x"));
            h.AddItem("y", 1, ItemCondition.Damaged);
            Assert.False(h.TakeCondition("y", ItemCondition.Sound));
            Assert.True(h.RepairOne("y"));
            Assert.Equal(1, h.RepairedCount("y"));
        }

        // ---------- duvar bantları, geniş eşya, uzun mobilya ----------

        [Fact]
        public void Duvar_UcBant_AyniKenardaRafMesaleVeKanca()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), A, 0);
            var torch = k.World.PlaceOnWall(k.D("item.torch_wall"), A, 0);
            var rail = k.World.PlaceOnWall(k.D("item.peg_rail"), A, 0);
            Assert.Equal(Decor.BandMid, shelf.WallBand);
            Assert.Equal(Decor.BandHigh, torch.WallBand);
            Assert.Equal(Decor.BandLow, rail.WallBand);
            Assert.Equal(3, k.World.WallItemsOn(WallSides.EdgeOf(A, 0)).Count);
            var full = k.World.CanPlaceOnWall(k.D("item.wall_shelf_oak"), A, 0);
            Assert.False(full.Ok);                                             // orta ve alt dolu
            Assert.False(k.World.CanPlaceOnWall(k.D("item.torch_wall"), A, 0, Decor.BandLow, null).Ok);   // meşale alta asılmaz
            Assert.True(k.World.MoveWallItem(rail.Id, new GridPos(4, 2), 0).Ok);
            Assert.True(k.World.CanPlaceOnWall(k.D("item.wall_shelf_oak"), A, 0).Ok);   // alt boşaldı
        }

        [Fact]
        public void UzunMobilya_OnundekiDuvarinAltVeOrtaBandiniKapatir()
        {
            var k = Make();
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), A, 0);
            var tall = k.World.CanPlaceFloor(k.D("item.shelf_wood"), A, 0);
            Assert.False(tall.Ok);
            Assert.Contains("önünü kapatır", tall.Reason);
            k.World.RemoveItem(shelf.Id);
            k.World.PlaceOnWall(k.D("item.torch_wall"), A, 0);                 // üst bant kapanmaz
            var cupboard = k.World.PlaceItem(k.D("item.shelf_wood"), A, 0);
            Assert.True(cupboard.Def.Tall);
            var mid = k.World.CanPlaceOnWall(k.D("item.wall_shelf"), A, 0);
            Assert.False(mid.Ok);
            Assert.Contains("uzun", mid.Reason);
            Assert.True(k.World.CanPlaceOnWall(k.D("item.flower_wreath"), new GridPos(4, 2), 0, Decor.BandHigh, null).Ok);
        }

        [Fact]
        public void GenisDuvarEsyasi_IkiKenarIster_KapiAraGiremez()
        {
            var k = Make();
            var cloth = k.D("item.painted_cloth");
            Assert.Equal(2, cloth.WallWidth);
            Assert.True(k.World.CanPlaceOnWall(cloth, A, 0).Ok);
            k.World.Map.SetEdge(new Edge(4, 2, EdgeAxis.Horizontal), EdgeKind.Door);
            var r = k.World.CanPlaceOnWall(cloth, A, 0);
            Assert.False(r.Ok);
            Assert.Contains("2 karo düz duvar", r.Reason);
            Assert.False(k.World.CanPlaceOnWall(cloth, new GridPos(7, 2), 0).Ok);          // köşeden taşar
            var hung = k.World.PlaceOnWall(cloth, new GridPos(5, 2), 0);
            Assert.Single(k.World.WallItemsOn(new Edge(6, 2, EdgeAxis.Horizontal)));      // ikinci kenar da onun
            Assert.False(k.World.CanPlaceOnWall(k.D("item.flower_wreath"), new GridPos(6, 2), 0, Decor.BandMid, null).Ok);
            Assert.True(k.World.CanPlaceOnWall(k.D("item.flower_wreath"), new GridPos(6, 2), 0, Decor.BandHigh, null).Ok);
            Assert.Equal(2, hung.WallSegments().Count());
        }

        // ---------- tavan ----------

        [Fact]
        public void Tavan_KapaliOdada_KaroBasinaBir_YoluKapatmaz_Kanca()
        {
            var k = Make();
            var hook = k.D("item.beam_hook");
            Assert.False(k.World.CanPlaceCeiling(hook, new GridPos(12, 12)).Ok);          // dışarıda tavan yok
            var inside = new GridPos(4, 3);
            var hung = k.World.PlaceItem(hook, inside);                                    // PlaceItem tavana yönlendirir
            Assert.True(hung.IsCeiling);
            Assert.True(k.World.Map.IsWalkable(inside));
            Assert.Null(k.World.GetItemAt(inside));
            Assert.Same(hung, k.World.CeilingAt(inside));
            Assert.False(k.World.CanPlaceCeiling(k.D("item.hanging_lantern"), inside).Ok);
            Assert.True(k.World.CanPlaceOnSurface(k.D("item.hanging_basket"), hung.Id, "a").Ok);
            Assert.False(k.World.CanPlaceOnSurface(k.D("item.candle"), hung.Id, "a").Ok);
            k.House.AddItem("item.hanging_lantern");
            Assert.True(k.Build.PlaceFromInventory("item.hanging_lantern", new GridPos(5, 3), 0, out var lantern).Ok);
            Assert.True(lantern.IsCeiling);
            Assert.True(k.Build.Move(lantern.Id, new GridPos(6, 3), 0).Ok);
            Assert.True(k.Build.StoreToInventory(lantern.Id).Ok);
            Assert.Equal(1, k.House.Count("item.hanging_lantern"));
            Assert.Contains("Tavana asılır", ItemCatalog.Describe(hook));
            Assert.True(ItemCatalog.InCategory(k.Db, "ceiling").Count >= 5);
        }

        // ---------- halılar ----------

        [Fact]
        public void Hali_SinirsizKatman_SiraDegisir_GuzellikteYalnizcaUstteki()
        {
            var k = Make();
            var p = new GridPos(4, 3);
            var a = k.World.PlaceItem(k.D("item.rush_mat_large"), p);       // 2x2
            var b = k.World.PlaceItem(k.D("item.felt_rug_small"), p);
            var c = k.World.PlaceItem(k.D("item.goat_hide"), p);
            Assert.Same(c, k.World.GetRugAt(p));
            Assert.Equal(new[] { a.Id, b.Id, c.Id }, k.World.RugsAt(p).Select(r => r.Id).ToArray());
            Assert.True(k.World.RugToBack(c.Id));
            Assert.Same(b, k.World.GetRugAt(p));
            Assert.True(k.World.RugStep(a.Id, +1));                            // a, b'nin üstüne çıkar
            Assert.Same(a, k.World.GetRugAt(p));
            Assert.True(k.World.RugToFront(c.Id));
            Assert.Same(c, k.World.GetRugAt(p));
            Assert.True(k.World.Map.IsWalkable(p));

            // Güzellik: alttaki halılar yalnızca açıkta kalan karolarıyla sayılır.
            var room = k.World.Map.Rooms.RoomAt(p);
            var mood = new MoodService(k.World);
            int stacked = mood.BeautyOf(room).Items;
            k.World.RemoveItem(c.Id); k.World.RemoveItem(b.Id); k.World.RemoveItem(a.Id);
            k.World.PlaceItem(k.D("item.rush_mat_large"), p);
            k.World.PlaceItem(k.D("item.felt_rug_small"), new GridPos(6, 3));
            k.World.PlaceItem(k.D("item.goat_hide"), new GridPos(7, 3));
            Assert.True(mood.BeautyOf(room).Items > stacked);
        }

        // ---------- güzellik ----------

        [Fact]
        public void Guzellik_KatmanCesitliligi_Basamaklar_Dokuntu_Moodlet()
        {
            var k = Make();
            var room = k.World.Map.Rooms.RoomAt(new GridPos(3, 3));
            var mood = new MoodService(k.World);
            Assert.Equal(0, mood.BeautyOf(room).Score);
            var table = k.World.PlaceItem(k.D("item.table_painted"), new GridPos(4, 4));
            k.World.PlaceOnSurface(k.D("item.beeswax_candle"), table.Id, table.Def.SurfaceSlots[0].Id);
            k.World.PlaceOnSurface(k.D("item.chess_set"), table.Id, table.Def.SurfaceSlots[1].Id);
            k.World.PlaceOnWall(k.D("item.applique_hanging"), A, 0);
            k.World.PlaceItem(k.D("item.rug_woven_large"), new GridPos(3, 3));
            k.World.PlaceItem(k.D("item.iron_chandelier"), new GridPos(5, 3));
            var b = mood.BeautyOf(room);
            Assert.Equal(8, b.Variety);                                    // halı, duvar, yüzey, tavan, ışık
            Assert.True(b.FineItems >= 2);
            Assert.True(b.Score >= MoodService.BeautyNice, b.Breakdown());

            var wat = new Character(1, "Wat", new GridPos(3, 3)) { MoodEnabled = true };
            mood.Update(new[] { wat });
            Assert.True(wat.Mood.Has(MoodService.BeautyId));

            // Döküntü güzelliği düşürür.
            int before = mood.BeautyOf(room).Raw;
            k.Decor.Drop(new GridPos(6, 4), new List<StoredItem> { new StoredItem(Decor.BrokenPottery, ItemCondition.Sound) });
            Assert.Equal(before - 3, mood.BeautyOf(room).Raw);
            Assert.Equal(2, MoodService.BeautyTier(40, 1));                 // göz alıcı için üst kademeden 2 eşya gerekir
            Assert.Equal(3, MoodService.BeautyTier(40, 2));
            Assert.Equal(1, MoodService.BeautyTier(MoodService.BeautyNice, 0));
            Assert.Equal(0, MoodService.BeautyTier(MoodService.BeautyNice - 1, 5));
        }

        [Fact]
        public void Guzellik_KonfordenAyri_SusunKonforuYok()
        {
            var db = ContentFilesTests.LoadDb(out _);
            Assert.Equal(0, db.Items.Get("item.candle").Comfort);
            Assert.Equal(3, db.Items.Get("item.candle").Beauty);
            Assert.True(db.Items.Get("item.rug_wool").Comfort > 0);         // halı hem konfor hem güzellik
            Assert.True(db.Items.Get("item.rug_wool").Beauty > 0);
        }

        // ---------- ayna ----------

        [Fact]
        public void Ayna_EvdeAynaYoksaDegisiklikYok_MetalAynaKararir_Cilalanir()
        {
            var k = Make();
            var cat = AppearanceCatalog.Default();
            var c = new Character(1, "Edith", new GridPos(3, 3)) { Sex = Sex.Female, FreeMirrorAvailable = true };
            c.SetAge(40);
            c.Appearance.SkinColor = cat.SkinTones[0];
            var after = c.Appearance.Clone();
            after.HairColor = new Rgb(200, 30, 30);
            Assert.False(DecorSystem.HasMirror(k.World));
            var q = MirrorRules.Quote(c, after, null, cat, DecorSystem.HasMirror(k.World));
            Assert.Contains(MirrorRules.NoMirror, q.Errors);
            Assert.False(MirrorRules.Apply(c, after, null, cat, _ => true, DecorSystem.HasMirror(k.World), out var err));
            Assert.Contains("ayna", err);
            Assert.True(c.FreeMirrorAvailable);                              // hak saklanır

            // Hatırlatma bir kez.
            k.Members.Add(c);
            k.Decor.RemindFreeMirror();
            k.Decor.RemindFreeMirror();
            Assert.Single(k.Notices);

            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 4));
            var mirror = k.World.PlaceOnSurface(k.D("item.copper_hand_mirror"), table.Id, "a");
            Assert.True(DecorSystem.HasMirror(k.World));
            Assert.True(MirrorRules.Apply(c, after, null, cat, _ => true, DecorSystem.HasMirror(k.World), out _));
            Assert.False(c.FreeMirrorAvailable);

            var polish = Inter(mirror.Def, "polish");
            Assert.Equal("Ayna zaten parlak.", k.Effects.CanStart(c, mirror, polish));
            k.Decor.Tick(10.0);
            Assert.True(mirror.IsTarnished);
            Assert.Equal(1.0, MoodService.BeautyOfItem(mirror));             // 2 → yarısı
            Assert.Null(k.Effects.CanStart(c, mirror, polish));
            k.Effects.Apply(c, mirror, polish);
            Assert.False(mirror.IsTarnished);
            Assert.Contains("kararır", ItemCatalog.Describe(mirror.Def));
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_BantKatmanDurumKararmaDokuntuVeEnvanterGeriGelir()
        {
            var k = Make();
            var torch = k.World.PlaceOnWall(k.D("item.torch_wall"), A, 0);
            var shelf = k.World.PlaceOnWall(k.D("item.wall_shelf"), A, 0);
            shelf.Condition = ItemCondition.Repaired;
            var rail = k.World.PlaceOnWall(k.D("item.peg_rail"), A, 0);
            k.World.PlaceOnSurface(k.D("item.ladle"), rail.Id, "a");
            var lower = k.World.PlaceItem(k.D("item.felt_rug"), new GridPos(4, 4));
            var upper = k.World.PlaceItem(k.D("item.rush_mat"), new GridPos(4, 4));
            k.World.RugToBack(upper.Id);
            var hook = k.World.PlaceItem(k.D("item.beam_hook"), new GridPos(6, 4));
            var table = k.World.PlaceItem(k.D("item.table_wood"), new GridPos(4, 3));
            var mirror = k.World.PlaceOnSurface(k.D("item.copper_hand_mirror"), table.Id, "a");
            mirror.TarnishDays = 7.5;
            var pile = k.Decor.Drop(new GridPos(6, 3), new List<StoredItem>
            {
                new StoredItem("item.jug", ItemCondition.Damaged), new StoredItem(Decor.BrokenPottery, ItemCondition.Sound)
            });
            k.House.AddItem("item.copper_pot", 2);
            k.House.AddItem("item.copper_pot", 1, ItemCondition.Damaged);
            k.House.AddItem("item.wall_shelf", 1, ItemCondition.Repaired);

            var data = SaveGame.Capture(new GameClock(), k.World, k.House, new List<Character>(), new Progress.AccountProgress(), null, null);
            var json = JsonSerializer.Serialize(data, Json);
            var loaded = SaveGame.Load(JsonSerializer.Deserialize<SaveData>(json, Json), k.Db);
            var w = loaded.World;

            Assert.Equal(Decor.BandHigh, w.GetItem(torch.Id).WallBand);
            Assert.Equal(Decor.BandMid, w.GetItem(shelf.Id).WallBand);
            Assert.Equal(Decor.BandLow, w.GetItem(rail.Id).WallBand);
            Assert.Equal(ItemCondition.Repaired, w.GetItem(shelf.Id).Condition);
            Assert.Single(w.GetDecorOn(rail.Id));
            Assert.Equal(lower.Id, w.GetRugAt(new GridPos(4, 4)).Id);        // en alta indirilen hasır alta kaldı
            Assert.True(w.GetItem(hook.Id).IsCeiling);
            Assert.Equal(7.5, w.GetItem(mirror.Id).TarnishDays, 3);
            var loadedPile = w.GetItem(pile.Id);
            Assert.Equal(2, loadedPile.Contents.Count);
            Assert.Contains(loadedPile.Contents, c => c.ItemId == "item.jug" && c.Condition == ItemCondition.Damaged);
            Assert.Equal(3, loaded.Household.Count("item.copper_pot"));
            Assert.Equal(1, loaded.Household.DamagedCount("item.copper_pot"));
            Assert.Equal(1, loaded.Household.RepairedCount("item.wall_shelf"));
        }

        [Fact]
        public void Kayit_EskiKayittakiDuvarEsyasiVarsayilanBandaGelir()
        {
            var k = Make();
            var torch = k.World.PlaceOnWall(k.D("item.torch_wall"), A, 0);
            var data = SaveGame.Capture(new GameClock(), k.World, k.House, new List<Character>(), new Progress.AccountProgress(), null, null);
            foreach (var s in data.items) s.band = -1;                        // eski kayıtta bant yok
            var loaded = SaveGame.Load(data, k.Db);
            Assert.Equal(Decor.BandHigh, loaded.World.GetItem(torch.Id).WallBand);
        }
    }
}
