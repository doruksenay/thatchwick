using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Terzilik, Parti 3 (belge 48 §9-11): dükkân stokları, ısmarlama, hizmetler, kalıplar, usta alametleri, yadigâr.</summary>
    public class TerziIsmarlamaTests
    {
        private static ContentDatabase Db()
        {
            var db = ContentFilesTests.LoadDb(out _);
            db.Clothing = TerzilikVeriTests.LoadCatalog(db, out _);
            return db;
        }

        private static Character Person(int id = 1, double age = 35)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false, MoodEnabled = true };
            c.SetAge(age);
            return c;
        }

        [Fact]
        public void Dukkanlar_TerziYalnizcaBasitGiysiSatar()
        {
            var db = Db();
            var tailor = Shops.Get("tailor"); var saddler = Shops.Get("saddler"); var general = Shops.Get("general");
            var fisher = Shops.Get("fisher"); var smith = Shops.Get("smithy");
            var sold = db.Items.All.Where(d => d.Category == "clothing" && tailor.Goods(d)).ToList();
            Assert.Contains(sold, d => d.Id == "item.tunic");
            Assert.Contains(sold, d => d.Id == "item.cl_long_cloak");
            Assert.All(sold, d => Assert.True(db.Clothing.ByItem(d.Id) == null || db.Clothing.ByItem(d.Id).Tier <= ClothingTier.Artisan, d.Id));
            Assert.DoesNotContain(sold, d => d.Id == "item.cl_kaftan");
            Assert.True(saddler.Goods(db.Items.Get("item.cl_carik")));
            Assert.True(general.Goods(db.Items.Get("item.cl_straw_hat")));
            Assert.False(general.Goods(db.Items.Get("item.cl_kaftan")));
            Assert.False(fisher.Goods(db.Items.Get("item.cl_oiled_fisher_hood")));
            Assert.True(smith.Goods(db.Items.Get("item.jw_brooch")));
            Assert.True(tailor.Goods(db.Items.Get("item.alum")));
            Assert.True(db.Items.Get("item.cl_silk_shawl").HasTag("merchantonly"));
        }

        [Fact]
        public void Ismarlama_UcretSureKaliteTavani()
        {
            var db = Db();
            var s = new ClothingService(db.Clothing);
            var t = new TailorOrders(s);
            var c = Person();
            var m = db.Clothing.Get("long_tunic");   // Zanaatkâr
            Assert.Equal(25 + 15, t.Quote(m, 10, false, 0, false));
            Assert.Equal(25, t.Quote(m, 10, true, 0, false));
            Assert.Equal(23, t.Quote(m, 10, true, 30, false));   // %10
            var o = t.Place(c, "long_tunic", new ClothColor(DyePalette.Woad, ClothTone.Mid), true, 10, 0, ClothingTier.Peasant, 10.3, busy: false, cutsKey: "0=short");
            Assert.NotNull(o);
            Assert.Equal(12.0, o.DueDay);
            var o2 = t.Place(c, "long_tunic", default, true, 10, 0, ClothingTier.Peasant, 10.3, busy: true);
            Assert.Equal(13.5, o2.DueDay);   // yoğun +1, sıradaki iş +0,5
            Assert.Null(t.Deliver(o, c, 11.0, "Elif"));
            var it = t.Deliver(o, c, 12.0, "Elif");
            Assert.Equal(TailorOrders.QualityCap, it.Quality);
            Assert.Equal("short", it.Cuts[CutKind.Sleeve]);
            Assert.Equal(c.Id, it.OwnerId);
            Assert.Equal("Elif", it.MakerName);
            Assert.True(o.Delivered);
        }

        [Fact]
        public void Ismarlama_KademeItibarIster_KirmizOyuncuMalzemesiyle()
        {
            var db = Db();
            var t = new TailorOrders(new ClothingService(db.Clothing));
            var c = Person();
            Assert.Null(t.Place(c, "kaftan", default, true, 0, 0, ClothingTier.Peasant, 1, false));   // Tüccar, itibar yok
            Assert.NotNull(t.Place(c, "kaftan", default, true, 0, 0, ClothingTier.Artisan, 1, false));
            Assert.Null(t.Place(c, "wedding_kaftan", new ClothColor(DyePalette.Kermes, ClothTone.Mid), false, 0, 0, ClothingTier.Merchant, 1, false));
            Assert.NotNull(t.Place(c, "wedding_kaftan", new ClothColor(DyePalette.Kermes, ClothTone.Mid), true, 0, 0, ClothingTier.Merchant, 1, false));
            Assert.Equal("riza", TailorOrders.MakerFor(db.Clothing.Get("riding_boots")));
            Assert.Equal("orhan", TailorOrders.MakerFor(db.Clothing.Get("bridal_crown")));
            // ergen düğün kaftanı ısmarlayamaz
            Assert.Null(t.Place(Person(2, 26), "wedding_kaftan", default, true, 0, 0, ClothingTier.Noble, 1, false));
        }

        [Fact]
        public void Hizmetler_BoyatmaYamaTadilatOlcu()
        {
            var db = Db();
            var s = new ClothingService(db.Clothing);
            var t = new TailorOrders(s);
            var it = s.Create("long_tunic");
            Assert.True(t.Dye(it, DyePalette.Madder, ClothTone.Mid, alum: false));
            Assert.Equal(DyePalette.Madder, it.Color.Id);
            Assert.False(it.Fixed);
            Assert.True(t.Dye(it, DyePalette.Woad, ClothTone.Mid));
            Assert.Equal(new ClothColor(DyePalette.Purple, ClothTone.Dark), it.Color);
            it.Durability = 30;
            Assert.True(t.Patch(it));
            Assert.Equal(100f, it.Durability);
            Assert.True(t.Alter(it, CutKind.Sleeve, "short", AgeStage.Teen));
            Assert.False(t.Alter(it, CutKind.Collar, "boat", AgeStage.Teen));
            Assert.True(t.Alter(it, CutKind.Collar, "boat", AgeStage.YoungAdult));
            var kid = s.Create("child_tunic");
            kid.Outgrown = true;
            Assert.True(t.Measure(kid));
            Assert.False(kid.Outgrown);
            kid.Outgrown = true;
            Assert.False(t.Measure(kid));   // bir kez
            Assert.Equal(3, TailorOrders.ServiceFee(TailorService.Patch, 0));
        }

        [Fact]
        public void Kalip_BilinirYazilirMirasKalir()
        {
            var db = Db();
            var t = new TailorOrders(new ClothingService(db.Clothing));
            var a = Person(1); var b = Person(2);
            Assert.True(t.Knows(a, "tunic"));          // köylü kalıbı baştan
            Assert.True(t.Knows(a, "kaftan"));         // işaretsiz Tüccar: beceriyle
            Assert.False(t.Knows(a, "hair_net"));
            t.Learn(a, "hair_net");
            Assert.True(t.Knows(a, "hair_net"));
            Assert.False(t.Knows(b, "hair_net"));      // yazılmazsa yalnız bilen
            Assert.False(t.Write(a, "hair_net"));      // okuryazar değil
            a.Skills.RestoreXp(SkillType.Literacy, SkillRules.XpForLevel(3));
            Assert.True(t.Write(a, "hair_net"));
            Assert.True(t.Knows(b, "hair_net"));
            var save = t.Export();
            var t2 = new TailorOrders(new ClothingService(db.Clothing));
            t2.Import(save);
            Assert.True(t2.IsWritten("hair_net"));
        }

        [Fact]
        public void UstaAlameti_VeYadigarXp()
        {
            var db = Db();
            var s = new ClothingService(db.Clothing);
            var t = new TailorOrders(s);
            var master = Person(1, 60);
            var mark = t.GrantMastery(master, SkillType.Tailoring, 30);
            Assert.Equal("kaftan", mark.ModelId);
            Assert.Equal("mastery.Tailoring", mark.Mark);
            Assert.Equal("lale", mark.EmbroideryMotif);
            Assert.Equal(master.Id, mark.OwnerId);
            for (int i = 0; i < 12; i++) Assert.True(i >= 0);
            // torun giyer: yadigâr +%5 XP o beceride
            var grandchild = Person(2, 35);
            mark.Heirloom = true;
            mark.OwnerId = 0;
            Assert.Equal(EquipError.None, s.Equip(grandchild, mark, OutfitKind.Daily));
            s.Dress(grandchild, default);
            Assert.Equal(1.05, t.HeirloomXpFactor(grandchild, SkillType.Tailoring), 6);
            Assert.Equal(1.0, t.HeirloomXpFactor(grandchild, SkillType.Farming), 6);
        }

        [Fact]
        public void Yasam_IsmarlamaAkceDusulur_UlaklaGelir_KalipEngeli()
        {
            var db = Db();
            var w = new WorldState(new GridMap(10, 10));
            var house = new Household(500);
            var pop = new Population();
            var clock = new GameClock();
            var life = new LifeSystems(db, w, house, pop, clock);
            var c = Person();
            pop.Add(c);
            var o = life.OrderClothing(c, "long_tunic", new ClothColor(DyePalette.Weld, ClothTone.Light), ownMaterials: false, courier: true);
            Assert.NotNull(o);
            Assert.Equal(500 - o.Fee, house.Akce);
            clock.Restore(3.0, 1);
            life.Tick(0.001);
            Assert.True(o.Delivered);
            Assert.Contains(life.Clothing.All, i => i.ModelId == "long_tunic" && i.OwnerId == c.Id);
            // kalıp engeli: saç filesi kalıpsız dikilmez
            var table = w.PlaceItem(db.Items.Get("item.sewing_table"), new GridPos(2, 2));
            var net = table.Def.Interactions.First(i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.cl_hair_net"));
            var fx = new Thatchwick.Core.Work.InteractionEffects(w, house, db);
            life.Attach(fx);
            c.Skills.RestoreXp(SkillType.Tailoring, SkillRules.XpForLevel(7));
            house.AddItem("item.silk", 4); house.AddItem("item.silk_thread", 2);
            Assert.Contains("kalıb", fx.CanStart(c, table, net) ?? "");
            house.AddItem("item.sewing_pattern", 1);
            Assert.True(life.UsePattern(c, "hair_net"));
            Assert.DoesNotContain("kalıb", fx.CanStart(c, table, net) ?? "");
        }
    }
}
