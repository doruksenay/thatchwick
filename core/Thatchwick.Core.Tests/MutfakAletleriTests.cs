using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Mutfak aletleri, yakıt, bulaşık ve kap kademesi (Project belgeleri 63, 63a).</summary>
    public class MutfakAletleriTests
    {
        private static ContentDatabase Db() => ContentFilesTests.LoadDb(out _);

        private static Character Person(int id, int cooking, GridPos pos)
        {
            var c = new Character(id, "Aşçı" + id, pos) { CanStarve = false, MoodEnabled = true };
            c.SetAge(60);
            c.Skills.RestoreXp(SkillType.Cooking, SkillRules.XpForLevel(cooking));
            return c;
        }

        private sealed class Kitchen
        {
            public ContentDatabase Db; public WorldState W; public Household H; public InteractionEffects Fx; public KitchenSystem K;
            public ItemInstance Hearth, Pot, Table, Knife;

            public JobRunner Runner(Character c)
            {
                var r = new JobRunner(c, W) { Autonomous = false, Effects = Fx, Errands = K.ErrandFor, PrepareUse = K.Prepare };
                r.DurationFactor = K.DurationFactor;
                r.InteractionCompleted += K.OnCompleted;
                return r;
            }

            public InteractionDef At(ItemInstance it, string id) => it.Def.Interactions.First(i => i.Id == id);
        }

        // Arsa sahipli 16x16 dünya: taş ocak (2,2), demir kazan (8,2) yerde, masa (8,8) üstünde bıçak.
        private static Kitchen Make(ContentDatabase db, bool pot = true, bool knife = true)
        {
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var k = new Kitchen { Db = db, W = new WorldState(map), H = new Household() };
            k.Fx = new InteractionEffects(k.W, k.H, db, seed: 5);
            k.K = new KitchenSystem(k.W, k.H, db) { Now = () => 1.0, Text = Thatchwick.Core.Localization.LocCodec.Encode };   // anahtar ve argümanlar görünsün
            k.Fx.ExtraBlocker = k.K.Blocker;
            k.Fx.KitchenQuality = k.K.QualityFor;
            k.Fx.AppliedOn += k.K.OnApplied;
            k.Hearth = k.W.PlaceItem(db.Items.Get("item.hearth_stone"), new GridPos(2, 2));
            if (pot) k.Pot = k.W.PlaceItem(db.Items.Get("item.cooking_pot"), new GridPos(8, 2));
            k.Table = k.W.PlaceItem(db.Items.Get("item.table_wood"), new GridPos(8, 8));
            if (knife) k.Knife = k.W.PlaceOnSurface(db.Items.Get("item.kitchen_knife"), k.Table.Id, k.Table.Def.SurfaceSlots[0].Id);
            k.H.AddItem("item.firewood", 4);
            return k;
        }

        private static void Veg(Household h, int q = 0) { foreach (var id in new[] { "item.turnip", "item.cabbage", "item.leek" }) h.AddItem(id, 1, q); }

        // ---- matematik (63a §5.2-5.3) ----

        [Fact]
        public void KapKademesi_TavanVePuan_BelgedekiSaglama()
        {
            Assert.Equal(70, KitchenQuality.Cap(1));
            Assert.Equal(85, KitchenQuality.Cap(2));
            Assert.Equal(100, KitchenQuality.Cap(3));
            Assert.Equal(100, KitchenQuality.Cap(0));   // kap istemeyen yöntem
            Assert.Equal(ItemQuality.Fine, KitchenQuality.MaxQuality(70));
            Assert.Equal(ItemQuality.Superior, KitchenQuality.MaxQuality(85));
            Assert.Equal(ItemQuality.Max, KitchenQuality.MaxQuality(100));
            // Kil tencere, yardımcısız: 2. Demir kazan, tam alet: 4 + 2 = 6. Has bakır, tam alet, sabunlu: 6 + 2 + 2 + 1 → 10 (tavan).
            Assert.Equal(2, KitchenQuality.EquipmentScore(1, 0, 0, 2, false, false, false));
            Assert.Equal(6, KitchenQuality.EquipmentScore(2, 0, 2, 2, false, false, false));
            Assert.Equal(10, KitchenQuality.EquipmentScore(3, ItemQuality.Superior, 3, 3, false, false, true));
            // Kirli kap 2, kör bıçak 1 düşer; alt sınır 0.
            Assert.Equal(3, KitchenQuality.EquipmentScore(2, 0, 2, 2, true, true, false));
            Assert.Equal(0, KitchenQuality.EquipmentScore(1, 0, 0, 2, true, true, false));
            // Kademe ile puan doğrusal: her kademe +2.
            for (int t = 1; t < 3; t++)
                Assert.Equal(2, KitchenQuality.EquipmentScore(t + 1, 0, 1, 2, false, false, false) - KitchenQuality.EquipmentScore(t, 0, 1, 2, false, false, false));
            Assert.True(KitchenQuality.LevelShift(0) < 0 && KitchenQuality.LevelShift(5) == 0 && KitchenQuality.LevelShift(10) > 0);
        }

        // ---- içerik ----

        [Fact]
        public void Icerik_PisirmeOcakta_KazandaYalnizcaYemek_YontemlerVeGerekenler()
        {
            var db = Db();
            var pot = db.Items.Get("item.cooking_pot");
            Assert.Equal("Demir kazan", pot.DisplayName);
            Assert.Equal("boil", pot.VesselFor);
            Assert.Equal(2, pot.EquipTier);
            Assert.DoesNotContain(pot.Interactions, i => i.Cook != null || (i.Skill == SkillType.Cooking && i.Id != "feast" && i.Id != "taste"));
            Assert.Contains(pot.Interactions, i => i.Id == "eat_stew_plain");
            Assert.Equal(3, db.Items.Get("item.copper_pots").EquipTier);
            foreach (var hid in new[] { "item.hearth_stone", "item.fireplace_large" })
            {
                var h = db.Items.Get(hid);
                Assert.True(h.CookFire);
                var cooking = h.Interactions.Where(i => i.Skill == SkillType.Cooking && i.Id != "feast" && i.Id != "taste").ToList();
                Assert.True(cooking.Count > 30);
                Assert.All(cooking, i => Assert.NotNull(i.Cook));
                Assert.Equal("cook_pot", cooking.First(i => i.Id == "cook_stew").ChainDef.Id);
            }
            var stew = db.Items.Get("item.hearth_stone").Interactions.First(i => i.Id == "cook_stew");
            Assert.Equal(CookMethods.Stew, stew.Cook);
            Assert.Contains("tool:knife", stew.NeedTools);
            Assert.Equal(CookMethods.Grill, db.Items.Get("item.hearth_stone").Interactions.First(i => i.Id == "roast_meat").Cook);
            Assert.Equal(CookMethods.Fry, db.Items.Get("item.hearth_stone").Interactions.First(i => i.Id == "cook_fish_carp").Cook);
            // Kazan tarifleri iki ocakta da.
            Assert.Contains(db.Items.Get("item.hearth_stone").Interactions, i => i.Id == "craft:pastirma_pepper");
            Assert.Contains(db.Items.Get("item.fireplace_large").Interactions, i => i.Id == "craft:pastirma_pepper__fire");
            // Fırın: odunlu işte kürek, hamur işinde oklava, tartta tepsi.
            var oven = db.Items.Get("item.stone_oven");
            Assert.Contains("tool:peel", oven.Interactions.First(i => i.Id == "bake_bread").NeedTools);
            Assert.Contains("tool:pin", oven.Interactions.First(i => i.Id == "make_borek_leek").NeedTools);
            Assert.Contains("vessel:bake", oven.Interactions.First(i => i.Id == "make_tart_apple").NeedTools);
            // Bulaşık yerleri ve zinciri.
            Assert.Equal(WashPlaces.Tub, db.Items.Get("item.dish_tub").WashPlace);
            Assert.Equal("wash_dishes", db.Items.Get("item.dish_tub").Interactions.First(i => i.WashDishes).ChainDef.Id);
            Assert.Equal("wash_dishes_stand", db.Items.Get("item.kitchen_spout").Interactions.First(i => i.WashDishes).ChainDef.Id);
            // Her yeni alet bir tezgâhta yapılır.
            foreach (var id in new[] { "item.pot_clay", "item.griddle_iron", "item.guvec_plain", "item.spit_iron", "item.kitchen_knife", "item.wooden_spoon",
                                       "item.rolling_board", "item.baker_peel", "item.dish_basket", "item.dish_tub", "item.washing_stone", "item.cooking_pot" })
                Assert.True(db.Items.All.Any(s => s.Interactions.Any(i => i.IsCraft && i.Produce.Any(p => p.ItemId == id))), id + " yapılmıyor");
        }

        // ---- gerekenler (63a-K1) ----

        [Fact]
        public void Gerekenler_KapYoksaBicakYoksaOdunYoksaYapilmaz_YardimciYoksaYapilir()
        {
            var db = Db();
            var k = Make(db, pot: false);
            var c = Person(1, 3, new GridPos(4, 4));
            Veg(k.H);
            var stew = k.At(k.Hearth, "cook_stew");
            Assert.Contains("kitchen.vessel.stew", k.Fx.CanStart(c, k.Hearth, stew));
            k.H.AddItem("item.guvec_plain", 1);
            Assert.Null(k.Fx.CanStart(c, k.Hearth, stew));   // kaşık yok: yardımcı, iş yine yapılır
            Assert.True(k.K.DurationFactor(c, k.Hearth, stew) > 1.0);

            var soup = k.At(k.Hearth, "make_soup_bean");
            k.H.AddItem("item.broad_bean", 2); k.H.AddItem("item.onion", 1);
            Assert.Contains("kitchen.vessel.boil", k.Fx.CanStart(c, k.Hearth, soup));
            k.H.AddItem("item.pot_clay", 1);
            Assert.Null(k.Fx.CanStart(c, k.Hearth, soup));

            k.W.RemoveItem(k.Knife.Id);
            Assert.Contains("kitchen.tool.knife", k.Fx.CanStart(c, k.Hearth, soup));
            k.H.AddItem("item.kitchen_knife", 1);
            Assert.Null(k.Fx.CanStart(c, k.Hearth, soup));

            while (k.H.TryTakeItem("item.firewood")) { }
            Assert.Contains("kitchen.need.firewood", k.Fx.CanStart(c, k.Hearth, soup));   // ocak sönük, odun yok

            // Köylü muaf.
            var villager = Person(2, 3, new GridPos(4, 5)); villager.IsVillager = true;
            Assert.Null(k.Fx.CanStart(villager, k.Hearth, soup));
        }

        // ---- alıp getirme, yakıt, yerine koyma (63 §2.7) ----

        [Fact]
        public void Pisirme_KaziAlirOcagaGetirir_AtesiYakar_BitinceYerineKoyar()
        {
            var db = Db();
            var k = Make(db);
            var c = Person(1, 3, new GridPos(12, 12));
            k.H.AddItem("item.pea", 3);
            var r = k.Runner(c);
            Assert.False(k.Hearth.IsLit);
            Assert.True(r.Command(Job.UseItem(k.Hearth.Id, "cook_pea_soup")));

            bool sawTaken = false, carriedPot = false, sawPutBack = false, kindled = false;
            r.StepChanged += (_, st) => { if (st.Id == "put_back") sawPutBack = true; if (st.Id == "kindle") kindled = true; };
            for (int i = 0; i < 4000 && !r.IsIdle; i++)
            {
                r.Tick(0.0005);
                if (k.Pot.TakenBy == c.Id) sawTaken = true;
                if (r.HeldItemId == "item.cooking_pot") carriedPot = true;
            }
            Assert.True(r.IsIdle, r.LastFailure);
            Assert.True(sawTaken, "kazan alınmadı");
            Assert.True(carriedPot, "kazan elde taşınmadı");
            Assert.True(kindled, "ateş tutuşturulmadı");
            Assert.True(sawPutBack, "yerine konmadı");
            Assert.Equal(0, k.Pot.TakenBy);
            Assert.Equal(2, k.H.Count("item.soup_pea"));
            Assert.True(k.H.Count("item.firewood") < 4, "yakıt harcanmadı");
            Assert.True(k.K.DirtyCount("item.cooking_pot") == 1 && k.K.DirtyCount("item.kitchen_knife") == 0);   // bezelyeye bıçak gerekmez
        }

        [Fact]
        public void IptalEdilirse_AlinanKapSerbestKalir()
        {
            var db = Db();
            var k = Make(db);
            var c = Person(1, 3, new GridPos(9, 3));
            k.H.AddItem("item.pea", 3);
            var r = k.Runner(c);
            r.Command(Job.UseItem(k.Hearth.Id, "cook_pea_soup"));
            for (int i = 0; i < 400 && k.Pot.TakenBy != c.Id; i++) r.Tick(0.0005);
            Assert.Equal(c.Id, k.Pot.TakenBy);
            r.Cancel();
            Assert.Equal(0, k.Pot.TakenBy);
            var other = Person(2, 3, new GridPos(4, 4));
            Assert.Null(k.Fx.CanStart(other, k.Hearth, k.At(k.Hearth, "cook_pea_soup")));   // ayrılma da kalktı
        }

        // ---- kalite tavanı (63a §5) ----

        [Fact]
        public void KilTencere_UstaVeHasMalzemeyleBileSecmeyiGecmez()
        {
            var db = Db();
            var k = Make(db, pot: false);
            k.H.AddItem("item.pot_clay", 1);
            var c = Person(1, 10, new GridPos(4, 4));
            var soup = k.At(k.Hearth, "make_soup_bean");
            int best = 0;
            for (int n = 0; n < 12; n++)
            {
                k.H.AddItem("item.broad_bean", 2, ItemQuality.Superior); k.H.AddItem("item.onion", 1, ItemQuality.Superior);
                k.Fx.Apply(c, k.Hearth, soup);
                k.K.OnApplied(c, k.Hearth, soup);
            }
            best = k.H.BestQuality("item.soup_bean");
            Assert.True(k.H.Count("item.soup_bean") > 0);
            Assert.Equal(ItemQuality.Fine, best);

            // Bakır kazanla aynı aşçı Has yapabilir.
            var k2 = Make(db, pot: false);
            k2.H.AddItem("item.copper_pots", 1);
            int best2 = 0;
            for (int n = 0; n < 12; n++)
            {
                k2.H.AddItem("item.broad_bean", 2, ItemQuality.Superior); k2.H.AddItem("item.onion", 1, ItemQuality.Superior);
                k2.Fx.Apply(c, k2.Hearth, soup);
            }
            best2 = k2.H.BestQuality("item.soup_bean");
            Assert.Equal(ItemQuality.Superior, best2);
        }

        // ---- bulaşık (63a §4) ----

        [Fact]
        public void Bulasik_SepetleToplanirLegendeYikanir_SabunPirilPiril()
        {
            var db = Db();
            var k = Make(db);
            var c = Person(1, 3, new GridPos(4, 4));
            var soup = k.At(k.Hearth, "cook_pea_soup");
            k.H.AddItem("item.pea", 3);
            k.Fx.Apply(c, k.Hearth, soup);
            Assert.Equal(1, k.K.DirtyPieces);
            // Kirli kap ekipman puanını düşürür. Çorba bir yemek: puan sofra puanının ekipman bileşenine girer, seviye kaymaz.
            Assert.True(k.K.EquipmentPointsFor(c, k.Hearth, soup) <= KitchenQuality.EquipmentScore(2, 0, 0, 1, true, false, false));
            Assert.Equal(0, k.K.QualityFor(c, k.Hearth, soup).Value.levelShift);

            var tub = k.W.PlaceItem(db.Items.Get("item.dish_tub"), new GridPos(12, 2));
            var wash = k.At(tub, "wash_dishes");
            Assert.Equal("kitchen.block.no_basket", k.Fx.CanStart(c, tub, wash));
            k.H.AddItem(KitchenSystem.BasketId, 1);
            k.H.AddItem("item.soap", 1);
            Assert.Null(k.Fx.CanStart(c, tub, wash));
            var r = k.Runner(c);
            r.Command(Job.UseItem(tub.Id, "wash_dishes"));
            for (int i = 0; i < 4000 && !r.IsIdle; i++) r.Tick(0.0005);
            Assert.True(r.IsIdle, r.LastFailure);
            Assert.Equal(0, k.K.DirtyPieces);
            Assert.True(k.K.Sparkling);
            Assert.Equal(0, k.H.Count("item.soap"));
            Assert.Equal("kitchen.block.no_dishes", k.Fx.CanStart(c, tub, wash));
        }

        [Fact]
        public void YikamaTasi_SuKenarindaOlmali()
        {
            var db = Db();
            var k = Make(db);
            var stone = k.W.PlaceItem(db.Items.Get("item.washing_stone"), new GridPos(13, 13));
            k.H.AddItem(KitchenSystem.BasketId, 1);
            k.H.AddItem("item.pea", 3);
            var c = Person(1, 3, new GridPos(4, 4));
            k.Fx.Apply(c, k.Hearth, k.At(k.Hearth, "cook_pea_soup"));
            Assert.Equal("kitchen.block.stone_dry", k.Fx.CanStart(c, stone, k.At(stone, "wash_dishes")));
        }

        [Fact]
        public void KirliTabak_TemizTabakKalmayincaKasvet_GorevliYikar()
        {
            var db = Db();
            var k = Make(db);
            var c = Person(1, 3, new GridPos(4, 4));
            k.H.AddItem("item.wooden_bowl", 1);
            k.H.AddItem(KitchenSystem.BasketId, 1);
            var eat = db.Items.Get("item.cooking_pot").Interactions.First(i => i.Id == "eat_stew_plain");
            k.K.OnCompleted(c, eat);
            Assert.Equal(1, k.K.DirtyTableware);
            k.K.OnCompleted(c, eat);
            Assert.True(c.Mood.Has(KitchenSystem.DirtyPlateMood));
            Assert.Null(k.K.WashJob(c));   // az ve taze: görevli beklemez
            k.K.Now = () => 1.0 + KitchenSystem.DutyWashAfterDays;   // yarım gün bekledi
            var tub = k.W.PlaceItem(db.Items.Get("item.dish_tub"), new GridPos(12, 2));
            var job = k.K.WashJob(c);
            Assert.NotNull(job);
            Assert.Equal(tub.Id, job.ItemId);
        }

        // ---- kayıt ----

        [Fact]
        public void Kayit_KirliVeBicakKorunur_EskiKayitHediyeTakimAlir()
        {
            var db = Db();
            var k = Make(db);
            var c = Person(1, 3, new GridPos(4, 4));
            k.H.AddItem("item.pea", 3);
            k.Fx.Apply(c, k.Hearth, k.At(k.Hearth, "cook_pea_soup"));
            var save = k.K.Export();
            var k2 = new KitchenSystem(k.W, k.H, db);
            k2.Restore(save);
            Assert.Equal(1, k2.DirtyCount("item.cooking_pot"));

            var fresh = Make(db, pot: false, knife: false);
            fresh.K.Restore(null);   // bu kurallardan önceki kayıt
            foreach (var id in new[] { "item.cooking_pot", "item.griddle_iron", "item.guvec_plain", "item.spit_iron", "item.kitchen_knife", KitchenSystem.BasketId })
                Assert.True(fresh.H.Count(id) > 0, id);
        }

        [Fact]
        public void BaslangicTakimi_YeniOyunCorbaYapabilir()
        {
            var db = Db();
            foreach (var (id, _) in KitchenSystem.StarterKit) Assert.True(db.Items.Contains(id), id);
            var k = Make(db, pot: false, knife: false);
            StartingKit.Apply(k.H);
            var c = Person(1, 1, new GridPos(4, 4));
            Assert.Null(k.Fx.CanStart(c, k.Hearth, k.At(k.Hearth, "cook")));   // şalgam pişir ve ye: kil tencere + bıçak
        }
    }
}
