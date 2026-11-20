using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 57c ve 57d: beslenme tipleri, nadas tavanı, artık bereket, gübre türleri, tahıl yatması.
    public class ToprakBeslenmeTests
    {
        private sealed class Ctx
        {
            public ContentDatabase Db;
            public WorldState World;
            public Household House;
            public BuildService Build;
            public PlantSystem Plants;
            public InteractionEffects Effects;
            public double Day;
        }

        private static ItemDef Crop(string id, FeedType feed, string family = null, bool nitrogen = false, bool raw = false, params string[] tags)
        {
            var d = new ItemDef
            {
                Id = "crop_" + id, DisplayName = id, GrowDays = 6f, NeedsTilledSoil = true, DiesInWinter = true,
                Feed = feed, CropFamily = family, FixesNitrogen = nitrogen, EatenRaw = raw,
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef
                    {
                        Id = "harvest", DisplayName = "Hasat et", DurationDays = 0.05, Skill = SkillType.Farming, SkillXp = 5,
                        RequiresGrown = true, RemovesItem = true,
                        Produce = new List<ProduceDef> { new ProduceDef { ItemId = id, Count = 10 } }
                    },
                    Spread("manure", "manure", 0.45f, Fertilizer.FreshManure),
                    Spread("compost", "compost", 0.35f, Fertilizer.Compost),
                    Spread("ash", "ash", 0.10f, Fertilizer.Ash),
                    Spread("marl", "marl", 0.01f, Fertilizer.Marl),
                    Spread("mulch", "leaf_mould", 0.01f, Fertilizer.Mulch)
                }
            };
            d.Tags.AddRange(tags);
            return d;
        }

        private static InteractionDef Spread(string id, string item, float amount, Fertilizer kind) => new InteractionDef
        {
            Id = id, DisplayName = id, DurationDays = 0.02, Fertilize = amount, FertilizerKind = kind,
            Consume = new List<ConsumeDef> { new ConsumeDef { ItemId = item, Count = 1 } }
        };

        private static Ctx Setup()
        {
            var db = new ContentDatabase();
            foreach (var id in new[] { "cabbage", "leek", "carrot", "herb", "pea", "wheat", "rye", "flax", "lettuce", "manure", "compost", "ash", "marl", "leaf_mould" })
                db.Items.Register(new ItemDef { Id = id, DisplayName = id, Placement = ItemPlacement.Resource, Price = 1 });
            db.Items.Register(Crop("cabbage", FeedType.Heavy, "yaprak"));
            db.Items.Register(Crop("leek", FeedType.Medium, "sogan"));
            db.Items.Register(Crop("carrot", FeedType.Light, "kok"));
            db.Items.Register(Crop("herb", FeedType.Lean, "ot"));
            db.Items.Register(Crop("pea", FeedType.Builder, "baklagil", nitrogen: true));
            db.Items.Register(Crop("wheat", FeedType.Heavy, "tahil"));
            db.Items.Register(Crop("rye", FeedType.Lean, "tahil", false, false, "tall"));
            db.Items.Register(Crop("flax", FeedType.Medium, "lif", false, false, "exhausting"));
            db.Items.Register(Crop("lettuce", FeedType.Medium, "yaprak", false, true));
            var c = new Ctx { Db = db, World = new WorldState(new GridMap(24, 12)), House = new Household(0) };
            c.Plants = new PlantSystem(c.World, seed: 7) { Now = () => c.Day };
            c.Build = new BuildService(c.World, c.House, db) { Plants = c.Plants };
            c.Effects = new InteractionEffects(c.World, c.House, db, 7) { Plants = c.Plants };
            return c;
        }

        private static ItemInstance Place(Ctx c, string crop, int x, int y, bool grown = false)
        {
            c.World.Map.SetFloor(new GridPos(x, y), FloorType.Dirt);
            var it = c.World.PlaceItem(c.Db.Items.Get("crop_" + crop), new GridPos(x, y));
            if (grown) PlantSystem.SetGrown(it);
            return it;
        }

        private static string Do(Ctx c, ItemInstance item, string interaction)
        {
            var ch = new Character(1, "A", new GridPos(item.Position.X + 1, item.Position.Y), true);
            ch.SetAge(30);
            var r = new JobRunner(ch, c.World) { Effects = c.Effects };
            string blocked = c.Effects.CanStart(ch, item, item.Def.Interactions.Find(i => i.Id == interaction));
            r.Command(Job.UseItem(item.Id, interaction));
            for (int i = 0; i < 80 && !r.IsIdle; i++) r.Tick(0.02);
            return blocked;
        }

        private static void Near(double expected, double actual, double tol = 0.0015) =>
            Assert.True(System.Math.Abs(expected - actual) <= tol, $"beklenen {expected:0.####}, gelen {actual:0.####}");

        private static double GrowOneDay(Ctx c, ItemInstance it)
        {
            foreach (var w in c.World.Items) if (w.IsPlant) w.Watered = true;
            double before = it.GrowthDays;
            c.Plants.Tick(1.0, Season.Spring);
            return it.GrowthDays - before;
        }

        [Fact]
        public void Obur_ekin_fakir_toprakta_belirgin_yavas_iyi_toprakta_hizli()
        {
            var c = Setup();
            var poor = Place(c, "cabbage", 1, 1); c.Plants.SetSoil(poor.Position, 0.40f, null);
            var rich = Place(c, "cabbage", 3, 1); c.Plants.SetSoil(rich.Position, 0.90f, null);
            var mid = Place(c, "leek", 5, 1); c.Plants.SetSoil(mid.Position, 0.40f, null);
            double gPoor = GrowOneDay(c, poor), gRich = rich.GrowthDays, gMid = mid.GrowthDays;
            Assert.InRange(gPoor, 0.80, 0.86);        // gübresiz obur: %15-20 yavaş
            Assert.InRange(gRich, 1.15, 1.25);        // iyi gübreli obur: yaklaşık %20 hızlı
            Assert.InRange(gMid, 0.94, 0.96);         // orta ekin: %5 yavaş
        }

        [Fact]
        public void Kanaatkar_ve_yagsiz_toprak_seven_fakir_toprakta_ceza_almaz()
        {
            var c = Setup();
            var carrot = Place(c, "carrot", 1, 1); c.Plants.SetSoil(carrot.Position, 0.40f, null);
            var herb = Place(c, "herb", 3, 1); c.Plants.SetSoil(herb.Position, 0.30f, null);
            var pea = Place(c, "pea", 5, 1); c.Plants.SetSoil(pea.Position, 0.30f, null);
            Assert.InRange(GrowOneDay(c, carrot), 0.97, 1.0);
            Near(1.0, herb.GrowthDays);
            Near(1.0, pea.GrowthDays);
        }

        [Fact]
        public void Hasat_topragi_tipe_gore_yorar()
        {
            var c = Setup();
            float Draw(string crop, int x)
            {
                var it = Place(c, crop, x, 1, grown: true);
                c.Plants.SetSoil(it.Position, 0.60f, null);
                c.Plants.OnHarvest(it);
                return 0.60f - c.Plants.FertilityAt(it.Position);
            }
            Near(0.18f, Draw("cabbage", 1));
            Near(0.12f, Draw("leek", 3));
            Near(0.07f, Draw("carrot", 5));
            Near(0.07f, Draw("herb", 7));
            Assert.True(Draw("pea", 9) < 0f);   // baklagil toprağa verir
        }

        [Fact]
        public void Islenmis_toprak_nadasla_ancak_tavana_doner_bakir_toprak_eski_kuralda()
        {
            var c = Setup();
            var worked = new GridPos(1, 1);
            var it = Place(c, "cabbage", 1, 1, grown: true);
            c.Plants.SetSoil(worked, 0.20f, null);
            c.Plants.OnHarvest(it);
            c.World.RemoveItem(it.Id);
            var virgin = new GridPos(3, 1);
            c.Plants.SetSoil(virgin, 0.20f, null);
            for (int d = 0; d < 40; d++) { c.Day = d; c.Plants.OnNewDay(false); }
            Near(0.40f, c.Plants.FertilityAt(worked));
            Near(0.50f, c.Plants.FertilityAt(virgin));
        }

        [Fact]
        public void Marn_nadas_tavanini_yukseltir_erken_tekrar_kireclendirir()
        {
            var c = Setup();
            var p = new GridPos(1, 1);
            c.Plants.Plot(p).Worked = true;
            c.Plants.SetSoil(p, 0.10f, null);
            c.Plants.Plot(p).Worked = true;
            Assert.Equal(FertilizeOutcome.Ok, c.Plants.ApplyFertilizer(p, Fertilizer.Marl, 0f));
            Near(0.10f, c.Plants.FertilityAt(p));   // anlık verim vermez
            for (int d = 0; d < 40; d++) { c.Day = d; c.Plants.OnNewDay(false); }
            Near(0.50f, c.Plants.FertilityAt(p));
            Assert.Equal(FertilizeOutcome.Limed, c.Plants.ApplyFertilizer(p, Fertilizer.Marl, 0f));
            float before = c.Plants.FertilityAt(p);
            c.Plants.ApplyFertilizer(p, Fertilizer.Compost, 0.30f);
            Near(before + 0.30f * 0.6f * 0.5f, c.Plants.FertilityAt(p));   // kireçlenmiş toprakta gübre yarı etkili
        }

        [Fact]
        public void Gubrenin_bir_kismi_sonraki_ekime_kalir_ve_karo_dongude_bir_kez_gubrelenir()
        {
            var c = Setup();
            var it = Place(c, "cabbage", 1, 1);
            c.Plants.SetSoil(it.Position, 0.40f, null);
            c.House.AddItem("compost", 2);
            Assert.Null(Do(c, it, "compost"));
            Near(0.40f + 0.35f * 0.6f, c.Plants.FertilityAt(it.Position), 0.006);
            Assert.NotNull(Do(c, it, "compost"));                                   // toprak doymuş
            Assert.Equal(1, c.House.Count("compost"));
            PlantSystem.SetGrown(it);
            float beforeHarvest = c.Plants.FertilityAt(it.Position);
            c.Plants.OnHarvest(it);
            Near(beforeHarvest - 0.18f + 0.35f * 0.4f, c.Plants.FertilityAt(it.Position), 0.006);   // artık bereket hasatta geçer
            Assert.Equal(FertilizeOutcome.Ok, c.Plants.CanFertilize(it.Position, Fertilizer.Compost)); // yeni döngü
        }

        [Fact]
        public void Taze_gubre_ekili_karoyu_yakar_bos_topraga_zararsizdir()
        {
            var c = Setup();
            var empty = new GridPos(1, 1);
            Assert.Equal(FertilizeOutcome.Ok, c.Plants.ApplyFertilizer(empty, Fertilizer.FreshManure, 0.45f));
            var it = Place(c, "leek", 3, 1);
            Assert.Equal(FertilizeOutcome.Burned, c.Plants.ApplyFertilizer(it.Position, Fertilizer.FreshManure, 0.45f, it));
            Near(0.0, GrowOneDay(c, it));          // bir gün büyümez
            c.Day = 1.5;
            Assert.True(GrowOneDay(c, it) > 0.9);
        }

        [Fact]
        public void Sert_gubre_obur_ekini_yakmaz()
        {
            var c = Setup();
            var cab = Place(c, "cabbage", 1, 1);
            var leek = Place(c, "leek", 3, 1);
            Assert.Equal(FertilizeOutcome.Ok, c.Plants.ApplyFertilizer(cab.Position, Fertilizer.HotManure, 0.60f, cab));
            Assert.Equal(FertilizeOutcome.Burned, c.Plants.ApplyFertilizer(leek.Position, Fertilizer.HotManure, 0.60f, leek));
        }

        [Fact]
        public void Kok_taze_gubrede_catallanir_ama_oburun_ardindan_ekilince_zarar_gormez()
        {
            var c = Setup();
            // aynı döngüde taze gübre: kalite düşer
            var bad = Place(c, "carrot", 1, 1);
            c.Plants.SetSoil(bad.Position, 0.9f, null);
            bad.Watered = true; c.Plants.Tick(6.0, Season.Spring);
            int clean = c.Plants.QualityOf(bad, 5);
            c.Plants.ApplyFertilizer(bad.Position, Fertilizer.FreshManure, 0.10f, bad);
            Assert.True(c.Plants.IsSpoiledByManure(bad));
            Assert.Equal(System.Math.Max(0, clean - 1), c.Plants.QualityOf(bad, 5));

            // nöbet: gübrelenmiş lahana hasat edilir, aynı karoya havuç gelir
            var cab = Place(c, "cabbage", 5, 1, grown: true);
            c.Plants.ApplyFertilizer(cab.Position, Fertilizer.FreshManure, 0.45f);
            c.Plants.OnHarvest(cab);
            c.World.RemoveItem(cab.Id);
            var good = Place(c, "carrot", 5, 1);
            Assert.False(c.Plants.IsSpoiledByManure(good));
        }

        [Fact]
        public void Yagsiz_toprak_seven_fazla_verimde_cok_ama_kalitesiz_urun_verir()
        {
            var c = Setup();
            var lean = Place(c, "herb", 1, 1); c.Plants.SetSoil(lean.Position, 0.45f, null);
            var fat = Place(c, "herb", 3, 1); c.Plants.SetSoil(fat.Position, 0.90f, null);
            foreach (var it in new[] { lean, fat }) { it.Watered = true; }
            c.Plants.Tick(6.0, Season.Spring);
            Assert.True(c.Plants.IsLeafy(fat));
            Assert.False(c.Plants.IsLeafy(lean));
            Assert.True(c.Plants.YieldFactor(fat) > c.Plants.YieldFactor(lean));
            Assert.True(c.Plants.QualityOf(fat, 5) < c.Plants.QualityOf(lean, 5));
        }

        [Fact]
        public void Toprak_yapan_fazla_gubrede_az_urun_verir_gubresiz_obur_da()
        {
            var c = Setup();
            var pea = Place(c, "pea", 1, 1, grown: true); c.Plants.SetSoil(pea.Position, 0.9f, null);
            var cab = Place(c, "cabbage", 3, 1, grown: true); c.Plants.SetSoil(cab.Position, 0.40f, null);
            Near(0.80f, c.Plants.YieldFactor(pea));
            Near(0.85f, c.Plants.YieldFactor(cab));
        }

        [Fact]
        public void Kul_yilda_bir_serilir_ve_ayni_gun_taze_gubrenin_yarisini_ucurur()
        {
            var c = Setup();
            var p = new GridPos(1, 1);
            c.Plants.SetSoil(p, 0.30f, null);
            c.Plants.ApplyFertilizer(p, Fertilizer.FreshManure, 0.40f);
            float afterManure = c.Plants.FertilityAt(p);
            c.Plants.Plot(p).Cycle = Fertilizer.None;   // küle izin (aynı döngü doygunluğu ayrı testte)
            c.Plants.ApplyFertilizer(p, Fertilizer.Ash, 0.10f);
            Near(afterManure - 0.40f * 0.6f * 0.5f + 0.10f * 0.6f, c.Plants.FertilityAt(p));
            Assert.Equal(FertilizeOutcome.AshUsed, c.Plants.CanFertilize(p, Fertilizer.Ash));
            c.Day = 41;
            Assert.Equal(FertilizeOutcome.Ok, c.Plants.CanFertilize(p, Fertilizer.Ash));
        }

        [Fact]
        public void Sevdigi_gubre_kaliteyi_artirir_sevmedigi_urunu_azaltir()
        {
            var c = Setup();
            var def = c.Db.Items.Get("crop_cabbage");
            def.LikesFertilizer = Fertilizer.Ash;
            var a = Place(c, "cabbage", 1, 1); var b = Place(c, "cabbage", 3, 1);
            foreach (var it in new[] { a, b }) { c.Plants.SetSoil(it.Position, 0.55f, null); it.Watered = true; }
            c.Plants.Tick(6.0, Season.Spring);
            c.Plants.ApplyFertilizer(a.Position, Fertilizer.Ash, 0.0f, a);
            Assert.True(c.Plants.QualityOf(a, 3) > c.Plants.QualityOf(b, 3));
            def.LikesFertilizer = Fertilizer.None; def.DislikesFertilizer = Fertilizer.Ash;
            Near(0.8f, c.Plants.YieldFactor(a));
        }

        [Fact]
        public void Malc_sulanmamis_gunu_hafifletir_ve_nadas_kaybini_onler()
        {
            var c = Setup();
            var bare = Place(c, "leek", 1, 1); var mulched = Place(c, "leek", 3, 1);
            c.Plants.ApplyFertilizer(mulched.Position, Fertilizer.Mulch, 0f, mulched);
            c.Plants.Tick(1.0, Season.Spring);   // ikisi de sulanmadı
            Assert.True(mulched.GrowthDays > bare.GrowthDays * 1.8);
            Near(c.Plants.FertilityAt(bare.Position), c.Plants.FertilityAt(mulched.Position));   // verim eklemez

            var p = new GridPos(7, 1); var q = new GridPos(9, 1);
            foreach (var t in new[] { p, q }) { c.Plants.SetSoil(t, 0.80f, null); c.Plants.Plot(t).Worked = true; }
            c.Plants.ApplyFertilizer(q, Fertilizer.Mulch, 0f);
            for (int d = 0; d < 10; d++) { c.Day = d; c.Plants.OnNewDay(false); }
            Near(0.75f, c.Plants.FertilityAt(p));   // tavanın üstü yıkanır
            Near(0.80f, c.Plants.FertilityAt(q));   // malçlı toprak yitirmez
        }

        [Fact]
        public void Keten_ust_uste_ekilince_topragi_uc_kat_yorar()
        {
            var c = Setup();
            float Twice(string crop, int x)
            {
                var a = Place(c, crop, x, 1, grown: true);
                c.Plants.SetSoil(a.Position, 0.9f, null);
                c.Plants.OnHarvest(a); c.World.RemoveItem(a.Id);
                float mid = c.Plants.FertilityAt(new GridPos(x, 1));
                var b = Place(c, crop, x, 1, grown: true);
                c.Plants.OnHarvest(b);
                return mid - c.Plants.FertilityAt(new GridPos(x, 1));
            }
            float flax = Twice("flax", 1), leek = Twice("leek", 3);
            Near(0.12f + 0.06f * 1.5f, leek);
            Near(0.12f + 0.06f * 3f * 1.5f, flax);
        }

        [Fact]
        public void Olgun_tahil_firtinada_doker_asiri_gubreli_tahil_yatar()
        {
            var c = Setup();
            c.Plants.Config.LodgeChance = 1.0; c.Plants.Config.CrowChance = 0;
            var lean = Place(c, "wheat", 1, 1, grown: true); c.Plants.SetSoil(lean.Position, 0.60f, null);
            var rich = Place(c, "wheat", 3, 1, grown: true); c.Plants.SetSoil(rich.Position, 0.90f, null);
            var young = Place(c, "wheat", 5, 1); c.Plants.SetSoil(young.Position, 0.90f, null);   // filiz: yatmaz
            var cab = Place(c, "cabbage", 7, 1, grown: true); c.Plants.SetSoil(cab.Position, 0.95f, null);
            int lodged = 0, shattered = 0;
            c.Plants.CerealsHit += (l, s) => { lodged = l; shattered = s; };
            c.Plants.OnNewDay(rained: true, stormed: true);
            Assert.Equal(1, lodged); Assert.Equal(1, shattered);
            Assert.Equal(CerealDamage.Shattered, c.Plants.DamageOf(lean));
            Assert.Equal(CerealDamage.Lodged, c.Plants.DamageOf(rich));
            Assert.Equal(CerealDamage.None, c.Plants.DamageOf(young));
            Assert.Equal(CerealDamage.None, c.Plants.DamageOf(cab));
            Near(0.70f, c.Plants.YieldFactor(lean));
            Near(0.50f, c.Plants.YieldFactor(rich));
            // tarlada kaldıkça kayıp artar, bir yerde durur
            c.Plants.OnNewDay(false); c.Plants.OnNewDay(false);
            Near(0.40f, c.Plants.YieldFactor(rich));
            for (int i = 0; i < 20; i++) c.Plants.OnNewDay(false);
            Near(0.20f, c.Plants.YieldFactor(rich));
            // hasat kaydı temizler
            c.Plants.OnHarvest(rich);
            Assert.Equal(CerealDamage.None, c.Plants.DamageOf(rich));
        }

        [Fact]
        public void Uzun_boylu_tahil_daha_dusuk_verimde_yatar_yatmaz_cesit_yatmaz()
        {
            var c = Setup();
            c.Plants.Config.LodgeChance = 1.0; c.Plants.Config.CrowChance = 0;
            var rye = Place(c, "rye", 1, 1, grown: true); c.Plants.SetSoil(rye.Position, 0.82f, null);
            var wheat = Place(c, "wheat", 3, 1, grown: true); c.Plants.SetSoil(wheat.Position, 0.82f, null);
            Assert.True(c.Plants.LodgeRisk(rye)); Assert.False(c.Plants.LodgeRisk(wheat));
            c.Db.Items.Get("crop_wheat").Tags.Add("no_lodge");
            c.Plants.SetSoil(wheat.Position, 0.95f, null);
            c.Plants.OnNewDay(true, true);
            Assert.Equal(CerealDamage.Lodged, c.Plants.DamageOf(rye));
            Assert.Equal(CerealDamage.Shattered, c.Plants.DamageOf(wheat));
        }

        [Fact]
        public void Yatan_tahilin_hasadi_az_urun_verir_ve_notu_yazar()
        {
            var c = Setup();
            c.Plants.Config.LodgeChance = 1.0; c.Plants.Config.CrowChance = 0;
            var ok = Place(c, "wheat", 1, 1, grown: true); c.Plants.SetSoil(ok.Position, 0.5f, null);
            var down = Place(c, "wheat", 5, 1, grown: true); c.Plants.SetSoil(down.Position, 0.5f, null);
            c.Plants.RestoreCerealDamage(new[] { new KeyValuePair<int, float>(down.Id, 0.5f) }, null);
            string note = null;
            c.Effects.Applied += (ch, i, text) => note = text;
            Do(c, ok, "harvest"); int full = c.House.Count("wheat");
            Do(c, down, "harvest"); int half = c.House.Count("wheat") - full;
            Assert.True(half <= full / 2 + 1 && half < full, $"{half} / {full}");
            Assert.Contains("yard.note.lodged", note);   // testte çözücü kurulu değil: anahtar döner
        }

        [Fact]
        public void Pislikli_ham_kompost_cig_yenen_ekinde_risk_tasir()
        {
            var c = Setup();
            var lettuce = Place(c, "lettuce", 1, 1); var leek = Place(c, "leek", 3, 1);
            c.Plants.ApplyFertilizer(lettuce.Position, Fertilizer.CompostRaw, 0.15f, lettuce, filth: true);
            c.Plants.ApplyFertilizer(leek.Position, Fertilizer.CompostRaw, 0.15f, leek, filth: true);
            Assert.True(c.Plants.HasRawFilth(lettuce));
            Assert.False(c.Plants.HasRawFilth(leek));   // pişirilerek yenir
            PlantSystem.SetGrown(lettuce);
            int risk = 0;
            c.Effects.RawFilthHarvested += (ch, it) => risk++;
            Do(c, lettuce, "harvest");
            Assert.Equal(1, risk);
        }

        [Fact]
        public void Bereketli_su_sulanan_karoyu_her_gun_biraz_besler()
        {
            var c = Setup();
            var it = Place(c, "leek", 1, 1);
            c.Plants.SetSoil(it.Position, 0.50f, null);
            c.Db.Items.Register(new ItemDef { Id = "ditch", DisplayName = "Hendek", WaterRadius = 1, Interactions = new List<InteractionDef>() });
            c.World.PlaceItem(c.Db.Items.Get("ditch"), new GridPos(2, 1));
            c.Plants.Config.CrowChance = 0;
            c.Plants.OnNewDay(false);
            Near(0.50f, c.Plants.FertilityAt(it.Position));
            c.Plants.Fertigation = () => true;
            c.Plants.OnNewDay(false); c.Plants.OnNewDay(false);
            Near(0.52f, c.Plants.FertilityAt(it.Position));
        }

        [Fact]
        public void Icerik_beslenme_tipini_ve_gubre_turunu_okur()
        {
            Assert.Equal(FeedType.Heavy, ContentMapper.ParseFeed("heavy", false));
            Assert.Equal(FeedType.Builder, ContentMapper.ParseFeed(null, true));
            Assert.Equal(FeedType.Medium, ContentMapper.ParseFeed("", false));
            Assert.Equal(Fertilizer.Marl, ContentMapper.ParseFertilizer("marl"));
            Assert.Equal(Fertilizer.CompostRaw, ContentMapper.ParseFertilizer("compostRaw"));
            Assert.Equal(Fertilizer.Generic, ContentMapper.ParseFertilizer(null));
        }

        /// <summary>240 günlük (altı yıl) sanal köy: altı karoluk tarla, nöbetleşe ekim. Dönen (ilk yıl, son yıl, toplam, en düşük verim).</summary>
        private static (int first, int last, int total, float minFertility) Village(bool fertilize)
        {
            var c = Setup();
            string[] rota = { "wheat", "carrot", "pea", "cabbage", "leek", "lettuce" };
            int first = 0, last = 0, total = 0, cycle = 0;
            float min = 1f;
            int Stock() { int n = 0; foreach (var id in rota) n += c.House.Count(id); return n; }
            while (c.Day < 240)
            {
                var crops = new List<ItemInstance>();
                for (int x = 0; x < 6; x++)
                {
                    var crop = Place(c, rota[(x + cycle) % rota.Length], 1 + x * 2, 1);
                    crops.Add(crop);
                    if (fertilize && c.Plants.FertilityAt(crop.Position) < 0.6f) { c.House.AddItem("compost", 1); Do(c, crop, "compost"); }
                }
                for (int d = 0; d < 30 && crops.Exists(k => !k.IsGrown); d++)
                {
                    foreach (var w in c.World.Items) if (w.IsPlant) w.Watered = true;
                    c.Plants.Tick(1.0, Season.Spring);
                    c.Day += 1; c.Plants.OnNewDay(false);
                }
                int before = Stock();
                foreach (var crop in crops) Do(c, crop, "harvest");
                int got = Stock() - before;
                total += got;
                if (c.Day <= 40) first += got;
                if (c.Day > 200) last += got;
                for (int x = 0; x < 6; x++) min = System.Math.Min(min, c.Plants.FertilityAt(new GridPos(1 + x * 2, 1)));
                for (int d = 0; d < 2; d++) { c.Day += 1; c.Plants.OnNewDay(false); }   // iki gün nadas
                cycle++;
            }
            return (first, last, total, min);
        }

        [Fact]
        public void SanalKoy_240Gun_GubrelemeyenBatmaz_GubreleyenBelirginKazanir()
        {
            var lazy = Village(false);
            var keen = Village(true);
            // 20.11.2026 ölçümü: gübresiz (ilk yıl 88, son yıl 87, toplam 455, en düşük verim 0,32); gübreli (92, 93, 564, 0,46).
            Assert.True(lazy.last >= lazy.first * 0.8, $"gübrelemeyen çöktü: ilk yıl {lazy.first}, son yıl {lazy.last}");
            Assert.True(lazy.minFertility >= 0.2f, $"toprak tükendi: {lazy.minFertility}");
            Assert.True(keen.total >= lazy.total * 1.15, $"gübrelemenin ödülü az: {lazy.total} -> {keen.total}");
            Assert.True(keen.total <= lazy.total * 1.6, $"gübreleme oyunu bozuyor: {lazy.total} -> {keen.total}");
        }
    }
}
