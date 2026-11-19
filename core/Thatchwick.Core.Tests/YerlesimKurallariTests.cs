using System.IO;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Yerleşim kuralları (belge 44 B): yön ve ekran eşlemesi, ayna kuralı, her kural için geçen ve kalan örnek.</summary>
    public class YerlesimKurallariTests
    {
        private static WorldState Room() => YerlesimDogrulayiciTests.Room();

        private static ItemDef Chair()
        {
            var d = new ItemDef { Id = "sandalye", DisplayName = "sandalye", Category = "seating", FrontUse = Placements.Sit, Site = ItemSite.Indoor };
            d.Art = new ItemArtDef { Mirror = true };
            d.Art.Faces[(int)Dir4.W] = "iso_item_chair_sw";
            return d;
        }

        private static ItemDef Table() => new ItemDef { Id = "masa", DisplayName = "masa", Width = 2, Category = "surfaces", Site = ItemSite.Indoor };

        private static ItemDef Chest(bool tall = false, int w = 1)
        {
            var d = new ItemDef { Id = tall ? "raf" : "sandik", DisplayName = tall ? "raf" : "sandık", Width = w, FrontUse = Placements.Open, Tall = tall, Site = ItemSite.Indoor };
            d.Art = new ItemArtDef { Mirror = true };
            if (w == 1) d.Art.Faces[(int)Dir4.N] = "iso_item_chest_se"; else d.Art.Faces[(int)Dir4.W] = "iso_item_shelf_2x1_se_nw";
            return d;
        }

        private static ItemInstance Put(WorldState w, ItemDef d, int x, int y, Dir4? facing = null) =>
            w.PlaceItem(d, new GridPos(x, y), facing.HasValue ? ItemFacing.RotationFor(d, facing.Value) : 0);

        private static System.Collections.Generic.List<Violation> Check(WorldState w, LayoutPlan plan = null)
        {
            var v = new PlacementValidator(w, plan);
            LayoutRules.AddAll(v, null);
            return v.Validate();
        }

        // ---------- yön ve ekran eşlemesi ----------

        [Fact]
        public void Yon_EkranEslemesi_TekYerde()
        {
            Assert.Equal("se", Facing.ScreenName(Dir4.N));
            Assert.Equal("ne", Facing.ScreenName(Dir4.E));
            Assert.Equal("nw", Facing.ScreenName(Dir4.S));
            Assert.Equal("sw", Facing.ScreenName(Dir4.W));
            Assert.Equal((0, -1), Facing.Vec(Dir4.N));
            Assert.Equal((1, 0), Facing.Vec(Dir4.E));
            Assert.True(Facing.FacesCamera(Dir4.N) && Facing.FacesCamera(Dir4.W));
            Assert.False(Facing.FacesCamera(Dir4.E) || Facing.FacesCamera(Dir4.S));
            // WallSides ile aynı kenar: kuzey (0) karonun y kenarı, doğu (1) x+1 kenarı.
            Assert.Equal(WallSides.EdgeOf(new GridPos(2, 2), 1), Facing.EdgeOf(new GridPos(2, 2), Dir4.E));
            Assert.True(Facing.TryFromScreen("sw", out var d) && d == Dir4.W);
        }

        [Fact]
        public void Ayna_YalnizcaSolSag_ArkasiDonukUretmez()
        {
            Assert.Equal(Dir4.W, Facing.Mirror(Dir4.N));
            Assert.Equal(Dir4.N, Facing.Mirror(Dir4.W));
            Assert.Equal(Dir4.S, Facing.Mirror(Dir4.E));
            var chair = Chair();
            Assert.Equal(new[] { 0, 1 }, ItemFacing.ValidRotations(chair));   // W (kendi görseli) ve N (aynası); E ve S yok
            Assert.True(ItemFacing.TryView(chair, 1, false, out var s, out var flip));
            Assert.Equal("iso_item_chair_sw", s);
            Assert.True(flip);
            Assert.False(ItemFacing.TryView(chair, 2, false, out _, out _));
        }

        [Fact]
        public void Ayna_SimetrikDegilse_YalnizcaKendiYonu()
        {
            var chair = Chair();
            chair.Art.Mirror = false;
            Assert.Equal(new[] { 0 }, ItemFacing.ValidRotations(chair));
        }

        [Fact]
        public void Dondur_YalnizcaGecerliYonlerArasinda()
        {
            var chair = Chair();
            Assert.Equal(1, ItemFacing.NextRotation(chair, 0));
            Assert.Equal(0, ItemFacing.NextRotation(chair, 1));
            Assert.Equal(0, ItemFacing.Snap(chair, 2));
        }

        [Fact]
        public void GorseliYokEsya_HerYoneDoner()
        {
            Assert.Equal(new[] { 0, 1, 2, 3 }, ItemFacing.ValidRotations(Table()));
        }

        [Fact]
        public void CokKaroluOnYuzluEsya_UzunKenarOnYuz()
        {
            var bench = new ItemDef { Id = "bank", Width = 2, FrontUse = Placements.Sit, Art = new ItemArtDef { Mirror = true } };
            bench.Art.Faces[(int)Dir4.W] = "bank_2x1_se_nw";
            Assert.Equal(Dir4.N, ItemFacing.FacingFor(bench, 0));    // ayak izi x boyunca, ön yüz -y
            Assert.Equal(Dir4.W, ItemFacing.FacingFor(bench, 3));
            Assert.Equal(new[] { 0, 3 }, ItemFacing.ValidRotations(bench));
            var fronts = ItemFacing.FrontCells(bench, new GridPos(4, 5), 0).ToList();
            Assert.Equal(new[] { new GridPos(4, 4), new GridPos(5, 4) }, fronts);
        }

        [Fact]
        public void Merdiven_YalnizcaVeriDekiDonusler()
        {
            var st = new ItemDef { Id = "merdiven", Height = 2, StairsDelta = 1, Art = new ItemArtDef { Mirror = true, AlongY = "stairs_2x1_se" } };
            st.Art.Rotations.Add(0); st.Art.Rotations.Add(1);
            Assert.Equal(new[] { 0, 1 }, ItemFacing.ValidRotations(st));
        }

        // ---------- oturma yönü ----------

        [Fact]
        public void Oturma_MasayaBakan_Gecer()
        {
            var w = Room();
            Put(w, Table(), 4, 4);
            Put(w, Chair(), 4, 5, Dir4.N);
            Put(w, Chair(), 6, 4, Dir4.W);
            Assert.DoesNotContain(Check(w), v => v.Code == Rules.Facing);
        }

        [Fact]
        public void Oturma_DuvaraBakan_Kalir()
        {
            var w = Room();
            Put(w, Chair(), 4, 3, Dir4.N);    // önü kuzey duvarı
            var v = Assert.Single(Check(w), x => x.Code == Rules.Facing);
            Assert.Contains("duvara", v.Reason);
        }

        [Fact]
        public void Oturma_BosaBakan_Kalir()
        {
            var w = Room();
            Put(w, Table(), 4, 4);
            Put(w, Chair(), 7, 6, Dir4.N);
            Assert.Contains(Check(w), v => v.Code == Rules.Facing && v.Reason.Contains("masaya"));
        }

        [Fact]
        public void GorselYonu_ArkasiDonukDonus_Kalir()
        {
            var w = Room();
            Put(w, Table(), 4, 4);
            var c = w.PlaceItem(Chair(), new GridPos(4, 3), 2);   // E: görsel yok
            Assert.Contains(Check(w), v => v.Code == Rules.NoView && v.ItemId == c.Id);
        }

        // ---------- etkileşim ----------

        [Fact]
        public void Etkilesim_OnuBos_Gecer()
        {
            var w = Room();
            Put(w, Chest(), 4, 6, Dir4.N);     // güney (arka) duvara sırt, önü 4,5
            Assert.DoesNotContain(Check(w), v => v.Code == Rules.Interaction);
        }

        [Fact]
        public void Etkilesim_OnuDolu_Kalir()
        {
            var w = Room();
            Put(w, Chest(), 4, 6, Dir4.N);
            w.PlaceItem(YerlesimDogrulayiciTests.Def("fici"), new GridPos(4, 5));
            Assert.Contains(Check(w), v => v.Code == Rules.Interaction && v.ItemDefId == "sandik");
        }

        [Fact]
        public void Etkilesim_OnuKapalidanUlasilamaz_Kalir()
        {
            var w = Room();
            Put(w, Chest(), 4, 6, Dir4.N);
            foreach (var (x, y) in new[] { (3, 5), (5, 5), (4, 4) }) w.PlaceItem(YerlesimDogrulayiciTests.Def("fici"), new GridPos(x, y));
            Assert.Contains(Check(w), v => v.Code == Rules.Interaction && v.Reason.Contains("ulaşılamıyor"));
        }

        [Fact]
        public void Etkilesim_OnuDuvar_Kalir()
        {
            var w = Room();
            Put(w, Chest(), 4, 3, Dir4.N);     // önü kuzey duvarının öbür yanı
            Assert.Contains(Check(w), v => v.Code == Rules.Interaction && v.ItemDefId == "sandik");
        }

        // ---------- oda türü ve iç/dış ----------

        private static LayoutPlan Plan(string type, string trade = null)
        {
            var plan = new LayoutPlan();
            var site = new Site { Id = "ev", Name = "ev", Building = new RectI(3, 3, 5, 4), Plot = new RectI(0, 0, 14, 14), MinLevel = 0, MaxLevel = 0 };
            site.Rooms.Add(new RoomZone { Type = type, Trade = trade, Level = 0, Rect = new RectI(3, 3, 5, 4) });
            plan.Sites.Add(site);
            return plan;
        }

        private static ItemDef Forge()
        {
            var d = new ItemDef { Id = "ocak", DisplayName = "demirci ocağı", Site = ItemSite.Any };
            d.Belongs.Add(Placements.Workshop); d.Trades.Add("smithy");
            return d;
        }

        [Fact]
        public void OdaTuru_YatakOdasindaDemirciOcagi_Kalir()
        {
            var w = Room();
            Put(w, Forge(), 4, 4);
            Assert.Contains(Check(w, Plan(Placements.Bedroom)), v => v.Code == Rules.WrongRoom);
        }

        [Fact]
        public void OdaTuru_DemirciDukkanindaOcak_Gecer()
        {
            var w = Room();
            Put(w, Forge(), 4, 4);
            Assert.DoesNotContain(Check(w, Plan(Placements.Shop, "smithy")), v => v.Code == Rules.WrongRoom);
        }

        [Fact]
        public void OdaTuru_TekOdaliEvdeBirdenCokTur()
        {
            var w = Room();
            var bed = new ItemDef { Id = "yatak", Width = 2 }; bed.Belongs.Add(Placements.Bedroom);
            Put(w, bed, 4, 4);
            Assert.DoesNotContain(Check(w, Plan("living|bedroom|kitchen")), v => v.Code == Rules.WrongRoom);
        }

        [Fact]
        public void IcDis_DisMekanEsyasiIceride_Kalir()
        {
            var w = Room();
            Put(w, new ItemDef { Id = "kuyu", DisplayName = "kuyu", Site = ItemSite.Outdoor }, 4, 4);
            Assert.Contains(Check(w), v => v.Code == Rules.Site);
        }

        [Fact]
        public void IcDis_IcMekanEsyasiDisarida_Kalir_HerYerEsyasiGecer()
        {
            var w = Room();
            Put(w, new ItemDef { Id = "koltuk", DisplayName = "koltuk", Site = ItemSite.Indoor }, 10, 10);
            Put(w, new ItemDef { Id = "fici", DisplayName = "fıçı", Site = ItemSite.Any }, 11, 11);
            var v = Check(w).Where(x => x.Code == Rules.Site).ToList();
            Assert.Single(v);
            Assert.Equal("koltuk", v[0].ItemDefId);
        }

        // ---------- ilişki ----------

        [Fact]
        public void Iliski_OcakEsyasiOcaktanUzak_Kalir_YanindaGecer()
        {
            var w = Room();
            var hearth = new ItemDef { Id = "ocak", DisplayName = "ocak", HeatOutput = 10f };
            var pot = new ItemDef { Id = "tencere", DisplayName = "tencere", Relation = Placements.NearHearth };
            Put(w, hearth, 6, 3);
            var near = Put(w, pot, 7, 3);
            var far = Put(w, pot, 3, 6);
            var v = Check(w).Where(x => x.Code == Rules.Relation).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == near.Id);
            Assert.Contains(v, x => x.ItemId == far.Id);
        }

        [Fact]
        public void Iliski_Komodin_YatakBasinda()
        {
            var w = Room();
            var bed = new ItemDef { Id = "yatak", Width = 2, Category = "sleep" };
            var stand = new ItemDef { Id = "komodin", DisplayName = "komodin", Relation = Placements.Bedside };
            Put(w, bed, 4, 3);
            var ok = Put(w, stand, 6, 3);
            var bad = Put(w, stand, 7, 6);
            var v = Check(w).Where(x => x.Code == Rules.Relation).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
            Assert.Contains(v, x => x.ItemId == bad.Id);
        }

        [Fact]
        public void Iliski_UzunEsyaSirtiDuvarda_Gecer_OrtadaKalir()
        {
            var w = Room();
            var shelf = Chest(tall: true, w: 2);
            var ok = Put(w, shelf, 6, 6, Dir4.N);     // sırtı güney duvarı (y=7 kenarı)
            var bad = Put(w, shelf, 4, 4, Dir4.N);
            var v = Check(w).Where(x => x.Code == Rules.Relation).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
            Assert.Contains(v, x => x.ItemId == bad.Id);
        }

        // ---------- yapı modu önizlemesi ----------

        [Fact]
        public void Onizleme_KapiOnuUyari_YerlestirmeEngellenmez()
        {
            var w = Room();
            var chest = Chest();
            var warn = LayoutRules.Preview(w, chest, new GridPos(5, 6), 0);
            Assert.Contains(warn, x => x.Code == Rules.DoorFront && x.Severity == Severity.Warning);
            Assert.True(w.CanPlaceFloor(chest, new GridPos(5, 6), 0).Ok);   // kesin engel değil
        }

        [Fact]
        public void Onizleme_BaskasininOnunuKapatma_Uyari()
        {
            var w = Room();
            Put(w, Chest(), 4, 6, Dir4.N);
            var warn = LayoutRules.Preview(w, YerlesimDogrulayiciTests.Def("fici"), new GridPos(4, 5), 0);
            Assert.Contains(warn, x => x.Code == Rules.Interaction);
        }

        // ---------- gerçek içerik ----------

        [Fact]
        public void GercekIcerik_GorselDosyalariVar_OnYuzluEsyaninGecerliYonuVar()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var root = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "..", "Art", "Iso"));
            var files = new System.Collections.Generic.HashSet<string>(
                Directory.GetFiles(root, "*.png", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)));
            foreach (var d in db.Items.All)
            {
                if (d.Art == null) continue;
                foreach (var name in new[] { d.Art.AlongX, d.Art.AlongY, d.Art.Lit, d.Art.Grown, d.Art.Young, d.Art.Fruit }.Concat(d.Art.Faces))
                    if (name != null) Assert.True(files.Contains(name), $"{d.Id}: görsel dosyası yok {name}");
                if (d.Placement == ItemPlacement.Floor) Assert.True(ItemFacing.ValidRotations(d).Count > 0, d.Id);
                if (d.HasFront && d.Art.HasFaces)
                    Assert.True(ItemFacing.ValidRotations(d).All(r => Facing.FacesCamera(ItemFacing.FacingFor(d, r))), $"{d.Id}: arkası dönük yön geçerli sayıldı");
            }
        }

        [Fact]
        public void GercekIcerik_OturmaEsyalarinaOnYuzVeIliski()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var id in new[] { "item.chair_wood", "item.chair_carved", "item.bench_long", "item.garden_bench" })
            {
                var d = db.Items.Get(id);
                Assert.True(d.IsSeat, id);
                Assert.Equal(Placements.FaceTable, d.Relation);
            }
            Assert.Equal(ItemSite.Outdoor, db.Items.Get("item.well").Site);
            Assert.Contains("smithy", db.Items.Get("item.forge").Trades);
            Assert.True(db.Items.Get("item.candlestick").Anywhere);
        }
    }
}
