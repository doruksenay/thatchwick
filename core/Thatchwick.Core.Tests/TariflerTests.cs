using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Tarifler (17.11.2026): yemek etkileri ve 20 yemek, süt ürünleri ve içkiler, düzenek tarifleri,
    // zanaat malzemelerinin yapıya bağlanması.
    public class TariflerTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return db;
        }

        private static Character Cook(int id = 1, GridPos? at = null)
        {
            var c = new Character(id, "Aşçı", at ?? new GridPos(1, 1), true) { MoodEnabled = true };
            c.SetAge(40);
            return c;
        }

        private static void RunJob(JobRunner r, int steps = 80)
        {
            for (int k = 0; k < steps && !r.IsIdle; k++) r.Tick(0.01);
            Assert.True(r.IsIdle, r.LastFailure);
        }

        // ---------- aşama 1: yemek etkileri ----------

        [Fact]
        public void Etkiler_IcerikteTanimlanirSicakCorbaSoguguAzaltir()
        {
            Db();
            Assert.True(MoodCatalog.Default.TryGet("eff_warm_soup", out var soup));
            Assert.Equal(0.2, (double)soup.ColdResist, 3);
            Assert.True(MoodCatalog.Default.TryGet("eff_hearty", out var hearty) && hearty.SpeedBonus(SkillType.Mining) > 0.05f);

            float Lost(bool ate)
            {
                var c = Cook();
                c.AmbientTemperature = -5f;
                if (ate) Assert.True(c.AddMoodlet("eff_warm_soup"));
                c.Needs.Set(NeedType.Warmth, 100f);
                c.Tick(0.1);
                return 100f - c.Needs.Get(NeedType.Warmth);
            }
            Assert.True(Lost(true) < Lost(false) * 0.85f, $"{Lost(true)} vs {Lost(false)}");

            var full = Cook();
            full.AddMoodlet("eff_full_long");
            Assert.Equal(0.85, (double)full.Mood.HungerDecayFactor(), 3);
        }

        [Fact]
        public void Sofra_AyniYemekBezdirirCesitSevindirir()
        {
            var c = Cook();
            c.RecordMeal("item.turnip"); c.RecordMeal("item.turnip"); c.RecordMeal("item.turnip");
            Assert.True(c.Mood.Has(MoodCatalog.SameMeal));
            var d = Cook(2);
            d.RecordMeal("item.bread"); d.RecordMeal("item.stew_meat"); d.RecordMeal("item.apple");
            Assert.True(d.Mood.Has(MoodCatalog.VariedTable));
        }

        [Fact]
        public void Yemekler_HepsiDoyururBirCoguEtkiVerir()
        {
            var db = Db();
            var pot = db.Items.Get("item.cooking_pot");
            string[] dishes = { "soup_bean", "soup_fish", "stew_meat", "soup_chicken", "saute_mushroom", "kaygana", "breakfast", "pilaf", "sarma",
                "hosaf", "helva", "pekmez", "stew_game", "wrap_fish", "borek_leek", "kete_meat", "tart_apple", "simit", "rusk", "tarhana" };
            int withEffect = 0;
            foreach (var dish in dishes)
            {
                string id = "item." + dish;
                Assert.True(db.Items.Contains(id), id);
                // Bir yerde yapılır ...
                Assert.True(db.Items.All.Any(s => s.Interactions.Any(i => i.Produce.Any(p => p.ItemId == id) && (i.Skill == SkillType.Cooking || i.Skill == SkillType.Baking || i.Skill == SkillType.Brewing))), $"{id} yapılmıyor");   // pekmez İçecekçilik (belge 45)
                // ... ve kazanda ya da (pişirerek) ocakta yenince doyurur (63a).
                var eat = pot.Interactions.FirstOrDefault(i => i.ConsumeItem == id && i.Need == NeedType.Hunger)
                    ?? db.Items.Get("item.hearth_stone").Interactions.FirstOrDefault(i => i.ConsumeItem == id && i.Need == NeedType.Hunger);
                Assert.True(eat != null && eat.Restore > 0, $"{id} yenmiyor");
                if (eat.Moodlet != null || eat.GivesMood) withEffect++;
            }
            Assert.True(withEffect >= 16, $"etkili yemek {withEffect}");
        }

        [Fact]
        public void Zincir_SuttenYogurdaTarhanayaCorbaya()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household();
            var fx = new InteractionEffects(w, h, db);
            var c = Cook();
            var churn = w.PlaceItem(db.Items.Get("item.churn"), new GridPos(2, 4));
            var rack = w.PlaceItem(db.Items.Get("item.drying_rack"), new GridPos(5, 4));
            var pot = w.PlaceItem(db.Items.Get("item.hearth_stone"), new GridPos(8, 4));   // pişirme ocakta (63a)
            h.AddItem("item.milk", 2); h.AddItem("item.flour", 1); h.AddItem("item.onion", 1);
            var yogurt = churn.Def.Interactions.Single(i => i.Produce.Any(p => p.ItemId == "item.yogurt"));
            fx.Apply(c, churn, yogurt);
            Assert.Equal(2, h.Count("item.yogurt"));
            var tarhana = rack.Def.Interactions.Single(i => i.Produce.Any(p => p.ItemId == "item.tarhana"));
            Assert.Null(fx.CanStart(c, rack, tarhana));
            fx.Apply(c, rack, tarhana);
            Assert.Equal(3, h.Count("item.tarhana"));
            Assert.Equal(0f, db.Items.Get("item.tarhana").SpoilDays);                  // kışlık: bozulmaz

            c.Needs.Set(NeedType.Hunger, 20f);
            var runner = new JobRunner(c, w) { Autonomous = false, Effects = fx };
            runner.Command(Job.UseItem(pot.Id, "cook_tarhana"));
            RunJob(runner);
            Assert.True(c.Needs.Get(NeedType.Hunger) >= 74f);
            Assert.True(c.Mood.Has("eff_warm_soup"));
            Assert.True(c.Skills.GetXp(SkillType.Cooking) > 0);
            Assert.Equal("item.tarhana", c.RecentMeals.Last());
        }

        // ---------- aşama 2: süt ürünleri, fırın, içkiler ----------

        [Fact]
        public void SutFirinIcki_TarifleriVar()
        {
            var db = Db();
            bool Makes(string station, string product) => db.Items.Get(station).Interactions.Any(i => i.Produce.Any(p => p.ItemId == product));
            foreach (var p in new[] { "item.yogurt", "item.butter", "item.ayran" }) Assert.True(Makes("item.churn", p), p);
            foreach (var p in new[] { "item.borek_leek", "item.kete_meat", "item.tart_apple", "item.simit", "item.rusk", "item.bread" }) Assert.True(Makes("item.stone_oven", p), p);
            foreach (var p in new[] { "item.boza", "item.serbet", "item.vinegar" }) Assert.True(Makes("item.brew_barrel", p), p);
            Assert.Contains(db.Items.Get("item.salting_barrel").Interactions, i => i.AllConsumed().Any(c => c.ItemId == "item.vinegar"));
            var pot = db.Items.Get("item.cooking_pot");
            Assert.Contains(pot.Interactions, i => i.ConsumeItem == "item.boza" && i.Moodlet == "eff_boza");
            Assert.Contains(pot.Interactions, i => i.ConsumeItem == "item.soup_chicken" && i.Heal > 0f);
        }

        // ---------- aşama 3: düzenek tarifleri ----------

        [Fact]
        public void Duzenekler_YalnizcaYapilirKatalogdaYok()
        {
            var db = Db();
            foreach (var id in new[] { "item.windmill", "item.water_gate", "item.noria", "item.feed_mixer", "item.threshing_sledge", "item.ice_well" })
            {
                Assert.True(db.Items.Get(id).HasTag("craftonly"), id);
                Assert.True(db.Items.All.Any(s => s.Interactions.Any(i => i.IsCraft && i.Produce.Any(p => p.ItemId == id))), $"{id} tarifi yok");
                Assert.DoesNotContain(ItemCatalog.InCategory(db, db.Items.Get(id).Category), d => d.Id == id);
            }
        }

        [Fact]
        public void HasatTirmigiVeEkimMakinesi_EskiKayitIcinKalir_YapilamazAlinamaz()
        {
            // Dönem kararı (28. belge): mekanik biçer ve ekici yerine düven ve tohum serpen tarlacı.
            var db = Db();
            foreach (var id in new[] { "item.harvest_rake", "item.seeder" })
            {
                Assert.True(db.Items.Get(id).HasTag("legacy"), id);
                Assert.False(db.Items.All.Any(s => s.Interactions.Any(i => i.IsCraft && i.Produce.Any(p => p.ItemId == id))), $"{id} hâlâ yapılıyor");
                Assert.DoesNotContain(ItemCatalog.InCategory(db, db.Items.Get(id).Category), d => d.Id == id);
            }
        }

        [Fact]
        public void YelDegirmeni_SuOlmadanGucVerir_TirmikBicerEkiciEker()
        {
            var db = Db();
            var w = new WorldState(new GridMap(16, 16));
            var h = new Household(1000);
            var svc = new BuildService(w, h, db);
            var plants = new PlantSystem(w);
            var auto = new FarmAutomation(w, h) { Plants = plants, Build = svc, Content = db };
            w.PlaceItem(db.Items.Get("item.windmill"), new GridPos(2, 2));
            var mill = w.PlaceItem(db.Items.Get("item.millstone"), new GridPos(4, 2));
            var rake = w.PlaceItem(db.Items.Get("item.harvest_rake"), new GridPos(4, 4));
            Assert.True(auto.IsPowered(mill), "taş");
            Assert.True(auto.IsPowered(rake), "tırmık");

            // Tırmık: olgun ekini biçer.
            for (int x = 3; x <= 6; x++) w.Map.SetFloor(new GridPos(x, 6), FloorType.Dirt);
            h.AddItem("item.seed_turnip", 2);
            Assert.True(svc.Plant("item.seed_turnip", new GridPos(5, 6), out var crop).Ok);
            plants.Tick(30, Season.Spring);
            Assert.True(crop.IsGrown);
            auto.OnNewDay(Season.Spring);
            Assert.Null(w.GetItem(crop.Id));
            Assert.True(h.Count("item.turnip") >= 3);

            // Ekim makinesi: boş toprağa envanterdeki tohumu eker.
            var seeder = w.PlaceItem(db.Items.Get("item.seeder"), new GridPos(1, 4));
            Assert.True(auto.IsPowered(seeder), "ekici");
            for (int x = 0; x <= 2; x++) w.Map.SetFloor(new GridPos(x, 6), FloorType.Dirt);
            int seeds = h.Count("item.seed_turnip");
            Assert.True(seeds > 0, "tohum");
            auto.OnNewDay(Season.Spring);
            Assert.True(h.Count("item.seed_turnip") < seeds, "ekilmedi");
            Assert.Contains(w.Items, i => i.Def.Id == "item.crop_turnip" && i.Position.X <= 2 && i.Position.Y == 6);
        }

        [Fact]
        public void SuKapagi_KapaninCaKanalKurur_BuzKuyusuDondaToplar()
        {
            var db = Db();
            var w = new WorldState(new GridMap(16, 16));
            var h = new Household();
            var plants = new PlantSystem(w);
            w.PlaceItem(db.Items.Get("item.well"), new GridPos(2, 2));
            var gate = w.PlaceItem(db.Items.Get("item.water_gate"), new GridPos(3, 2));
            for (int x = 4; x <= 7; x++) w.PlaceItem(db.Items.Get("item.canal"), new GridPos(x, 2));
            var crop = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(9, 3));
            plants.OnNewDay(false);
            Assert.True(crop.Watered);
            gate.Toggled = true;
            plants.OnNewDay(false);
            Assert.False(crop.Watered);

            // Sulama dolabı kendisi bir kaynaktır ve çevresini sular.
            var noria = w.PlaceItem(db.Items.Get("item.noria"), new GridPos(10, 10));
            var near = w.PlaceItem(db.Items.Get("item.crop_turnip"), new GridPos(12, 12));
            plants.OnNewDay(false);
            Assert.True(near.Watered);
            Assert.Contains(noria.Id, WaterNetwork.Connected(w));

            float temp = 5f;
            var auto = new FarmAutomation(w, h) { TemperatureAt = p => temp };
            w.PlaceItem(db.Items.Get("item.ice_well"), new GridPos(12, 2));
            auto.OnNewDay();
            Assert.Equal(0, h.Count("item.ice"));
            temp = -4f;
            auto.OnNewDay();
            Assert.Equal(2, h.Count("item.ice"));
        }

        // ---------- aşama 4: zanaat malzemesi yapıya bağlanır ----------

        [Fact]
        public void Malzeme_VarsaHarcanirAkceYariyaIner_GeriAlinir()
        {
            var db = Db();
            var w = new WorldState(new GridMap(12, 12));
            var h = new Household(1000);
            var svc = new BuildService(w, h, db);
            var e1 = new Edge(3, 3, EdgeAxis.Horizontal);
            int full = svc.EdgeCost(EdgeKind.Wall, "wall_stone");
            int before = h.Akce;
            Assert.True(svc.SetEdge(e1, EdgeKind.Wall, "wall_stone").Ok);
            Assert.Equal(before - full, h.Akce);                                        // taş yok: tam akçe

            h.AddItem("item.stone", 1);
            var e2 = new Edge(5, 3, EdgeAxis.Horizontal);
            before = h.Akce;
            Assert.True(svc.SetEdge(e2, EdgeKind.Wall, "wall_stone").Ok);
            Assert.Equal(before - (full - full / 2), h.Akce);                           // taş var: yarı akçe
            Assert.Equal(0, h.Count("item.stone"));
            Assert.True(svc.Undo().Ok);
            Assert.Equal(1, h.Count("item.stone"));                                     // geri alınca taş döner
            Assert.Equal(before, h.Akce);

            // Tuğla zemin.
            h.AddItem("item.brick", 1);
            Assert.True(svc.PaintFloorPart(new GridPos(7, 7), "floor_brick").Ok);
            Assert.Equal(0, h.Count("item.brick"));
            Assert.NotNull(svc.MaterialHint(db.Parts.Get("roof_tile")));
            Assert.Equal("item.roof_tiles", db.Parts.Get("roof_tile").MaterialItem);
            Assert.Equal("item.dye_red", db.Parts.Get("cloth_madder").MaterialItem);
        }

        [Fact]
        public void Zanaat_TuglaKiremitTasBoyaSabunKece()
        {
            var db = Db();
            bool Makes(string station, string product, SkillType skill) =>
                db.Items.Get(station).Interactions.Any(i => i.Skill == skill && i.Produce.Any(p => p.ItemId == product));
            Assert.True(Makes("item.kiln", "item.brick", SkillType.Pottery));
            Assert.True(Makes("item.kiln", "item.roof_tiles", SkillType.Pottery));
            Assert.True(Makes("item.stonecutter", "item.cut_stone", SkillType.Masonry));
            Assert.True(Makes("item.dye_vat", "item.dye_red", SkillType.Weaving));
            Assert.True(Makes("item.chandler_table", "item.soap", SkillType.Chandlery));
            Assert.True(Makes("item.loom", "item.felt", SkillType.Weaving));
            Assert.Contains(db.Items.Get("item.wooden_tub").Interactions, i => i.ConsumeItem == "item.soap" && i.Need == NeedType.Hygiene);
        }
    }
}
