using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Dış mekân kuralları, bahçeler, bitki örtüsü ve yol ağı (belge 44 D).</summary>
    public class YerlesimDisMekanTests
    {
        // ---------- sentetik dünya: bina (iç 5-9 x 5-8), güneyde kapı x=7, kapıdan güneye patika, doğu-batı ana yol y=14 ----------

        private static (WorldState w, LayoutPlan plan) World()
        {
            var m = new GridMap(24, 20);
            for (int x = 5; x <= 9; x++)
            {
                m.SetEdge(new Edge(x, 5, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, 9, EdgeAxis.Horizontal), x == 7 ? EdgeKind.Door : EdgeKind.Wall);
            }
            for (int y = 5; y <= 8; y++) { m.SetEdge(new Edge(5, y, EdgeAxis.Vertical), EdgeKind.Wall); m.SetEdge(new Edge(10, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            for (int y = 9; y <= 14; y++) m.SetRoad(new GridPos(7, y), RoadType.Path);
            for (int x = 0; x < 24; x++) m.SetRoad(new GridPos(x, 14), RoadType.Path);
            var plan = new LayoutPlan();
            plan.Sites.Add(new Site { Id = "dukkan", Name = "dükkân", Kind = "shop", Trade = "smithy", Building = new RectI(5, 5, 5, 4), Plot = new RectI(1, 1, 13, 12), DoorDir = Dir4.S, DoorInside = new GridPos(7, 8) });
            return (new WorldState(m), plan);
        }

        private static List<Violation> Check(WorldState w, LayoutPlan plan)
        {
            var v = new PlacementValidator(w, plan);
            LayoutRules.AddAll(v, null);
            return v.Validate();
        }

        private static ItemDef D(string id, int w = 1, int h = 1, ItemSite site = ItemSite.Outdoor) => new ItemDef { Id = id, DisplayName = id, Width = w, Height = h, Site = site };

        [Fact]
        public void YolAgi_KapiYolaBagli_Gecer_KopukYolKalir()
        {
            var (w, plan) = World();
            Assert.DoesNotContain(Check(w, plan), v => v.Code == Rules.Path || v.Code == Rules.RoadNet);
            w.Map.SetRoad(new GridPos(7, 12), RoadType.None);   // patika kopar: kapının yolu ana ağa bağlı değil
            Assert.Contains(Check(w, plan), v => v.Code == Rules.RoadNet);
            w.Map.SetRoad(new GridPos(7, 9), RoadType.None);
            Assert.Contains(Check(w, plan), v => v.Code == Rules.Path && v.Reason.Contains("patika yok"));
        }

        [Fact]
        public void YolUstundeEsya_Kalir()
        {
            var (w, plan) = World();
            w.PlaceItem(D("fici", site: ItemSite.Any), new GridPos(15, 14));
            Assert.Contains(Check(w, plan), v => v.Code == Rules.Path && v.ItemDefId == "fici");
        }

        [Fact]
        public void Kuyu_YolKesisimininYaninda_Gecer_KapiOnundeKalir()
        {
            var (w, plan) = World();
            var well = D("item.well");
            w.PlaceItem(well, new GridPos(9, 13));          // kesişim (7,14): 3 karo içinde
            Assert.DoesNotContain(Check(w, plan), v => v.Code == Rules.WellSite);
            var (w2, plan2) = World();
            w2.PlaceItem(well, new GridPos(15, 5));
            Assert.Contains(Check(w2, plan2), v => v.Code == Rules.WellSite);
            var (w3, plan3) = World();
            w3.PlaceItem(well, new GridPos(7, 10));         // kapının önü (2. karo) ve patika
            var v3 = Check(w3, plan3);
            Assert.Contains(v3, v => v.Code == Rules.DoorFront);
        }

        private static ItemDef Bench()
        {
            var d = new ItemDef { Id = "bank", DisplayName = "bank", Width = 2, Category = "seating", FrontUse = Placements.Sit, Site = ItemSite.Outdoor, Art = new ItemArtDef { Mirror = true } };
            d.Art.Faces[(int)Dir4.W] = "bank_2x1_se_nw";
            return d;
        }

        [Fact]
        public void Bank_YolaBakar_SirtiDuvarda_Gecer()
        {
            var (w, plan) = World();
            // Binanın kuzeyinde, duvara sırt (güney kenarı bina duvarı), kuzeye (N) bakan bank; önünde yol.
            for (int x = 5; x <= 9; x++) w.Map.SetRoad(new GridPos(x, 2), RoadType.Path);
            w.PlaceItem(Bench(), new GridPos(5, 4), ItemFacing.RotationFor(Bench(), Dir4.N));
            Assert.DoesNotContain(Check(w, plan), v => v.Code == Rules.BenchFacing);
        }

        [Fact]
        public void Bank_YolaBakmaz_YaDaSirtiBos_Kalir()
        {
            var (w, plan) = World();
            w.PlaceItem(Bench(), new GridPos(5, 4), ItemFacing.RotationFor(Bench(), Dir4.N));   // önünde yol yok
            Assert.Contains(Check(w, plan), v => v.Code == Rules.BenchFacing && v.Reason.Contains("bakmıyor"));
            var (w2, plan2) = World();
            w2.PlaceItem(Bench(), new GridPos(15, 16), ItemFacing.RotationFor(Bench(), Dir4.N));  // yola bakar ama sırtı açık
            Assert.Contains(Check(w2, plan2), v => v.Code == Rules.BenchFacing && v.Reason.Contains("sırtı"));
        }

        [Fact]
        public void YagmurFicisi_BinaKosesinde_Gecer_OrtadaKalir()
        {
            var (w, plan) = World();
            var rb = D("item.rain_barrel");
            var ok = w.PlaceItem(rb, new GridPos(10, 5));    // doğu duvarının kuzey ucu
            var bad = w.PlaceItem(rb, new GridPos(15, 3));
            var v = Check(w, plan).Where(x => x.Code == Rules.Barrel).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
            Assert.Contains(v, x => x.ItemId == bad.Id);
        }

        [Fact]
        public void OdunYigini_YanDuvarda_Gecer_KapiDuvarindaKalir()
        {
            var (w, plan) = World();
            var wood = D("item.woodshed");
            var ok = w.PlaceItem(wood, new GridPos(4, 6));                // batı (yan) duvar
            var bad = w.PlaceItem(wood, new GridPos(8, 9));               // kapı (güney) duvarı
            var v = Check(w, plan).Where(x => x.Code == Rules.Barrel).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
            Assert.Contains(v, x => x.ItemId == bad.Id);
        }

        private static ItemDef Tree() => new ItemDef { Id = "item.tree_oak", DisplayName = "meşe", GrowDays = 10, Site = ItemSite.Outdoor };

        [Fact]
        public void Agac_BinayaBirKaroBosluk_VeKameraCephesiniOrtmez()
        {
            var (w, plan) = World();
            var near = w.PlaceItem(Tree(), new GridPos(10, 6));            // doğu duvarına bitişik
            var facade = w.PlaceItem(Tree(), new GridPos(3, 6));           // batı (kameraya bakan) cephenin 2 karo önü
            var ok = w.PlaceItem(Tree(), new GridPos(13, 7));
            var v = Check(w, plan);
            Assert.Contains(v, x => x.Code == Rules.TreeClear && x.ItemId == near.Id);
            Assert.Contains(v, x => x.Code == Rules.TreeFront && x.ItemId == facade.Id);
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
        }

        [Fact]
        public void MeslekDisEsyasi_KendiDukkanininYaninda()
        {
            var (w, plan) = World();
            var kiln = D("ocak"); kiln.Trades.Add("smithy");
            var other = D("hizar"); other.Trades.Add("carpenter");
            var ok = w.PlaceItem(kiln, new GridPos(12, 6));
            var bad = w.PlaceItem(other, new GridPos(12, 7));
            var v = Check(w, plan).Where(x => x.Code == Rules.TradeYard).ToList();
            Assert.DoesNotContain(v, x => x.ItemId == ok.Id);
            Assert.Contains(v, x => x.ItemId == bad.Id);
        }

        [Fact]
        public void Tabela_KapininYanindaYolaDegen_Gecer_UzaktaKalir()
        {
            var (w, plan) = World();
            var sign = D("item.shop_sign");
            var ok = w.PlaceItem(sign, new GridPos(8, 11));     // kapının dış karosu (7,9): 3 karo
            var v = Check(w, plan).Where(x => x.Code == Rules.Sign).ToList();
            Assert.Contains(v, x => x.ItemId == ok.Id);           // 3 karo: uzak
            w.RemoveItem(ok.Id);
            var good = w.PlaceItem(sign, new GridPos(8, 10));
            Assert.DoesNotContain(Check(w, plan), x => x.Code == Rules.Sign && x.ItemId == good.Id);
        }

        [Fact]
        public void Kesisim_UcYonluYolBulunur()
        {
            var (w, _) = World();
            var j = OutdoorRules.Junctions(w.Map);
            Assert.Contains(j, p => System.Math.Abs(p.X - 7) <= 1 && p.Y == 14);
        }

        // ---------- gerçek şehir ----------

        private static (WorldState w, StarterWorld.Result res) Town(int seed)
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var w = new WorldState(StarterWorld.BuildMap(lay));
            var res = StarterWorld.Place(w, db, lay, seed);
            return (w, res);
        }

        [Fact]
        public void Sehir_BastanSona_IhlalYok_HerKapiYolAginda()
        {
            foreach (var seed in new[] { 1, 2 })
            {
                var (w, res) = Town(seed);
                var v = new PlacementValidator(w, res.Plan);
                LayoutRules.AddAll(v, null);
                var errors = v.Validate().Where(x => x.Severity == Severity.Error).ToList();
                Assert.True(errors.Count == 0, $"tohum {seed}: " + string.Join("\n", errors.Take(10)));
                Assert.All(res.Reports, r => Assert.True(r.Missing.Count == 0, $"tohum {seed}, {r.Room}: eksik {string.Join(",", r.Missing)}"));
            }
        }

        [Fact]
        public void YeniOyun_HicbirBaslangicEsyasiDusmez()
        {
            // Elle koordinatlı kalan yaban haritası eşyaları dahil: her eşya konur ya da günlüğe düşer; günlük boş olmalı.
            var db = ContentFilesTests.LoadDb(out _);
            var lay = YerlesimSablonTests.LoadLayouts(db, out _);
            var w = new WorldState(StarterWorld.BuildMap(lay));
            var log = new List<string>();
            StarterWorld.Place(w, db, lay, 1, log.Add);
            Assert.True(log.Count == 0, string.Join("\n", log));
        }

        [Fact]
        public void PazarTezgahi_MeydandaDegil_KeremDukkanininOnunde()
        {
            var (w, res) = Town(1);
            var stalls = w.Items.Where(i => i.Def.Id == "item.market_stall").ToList();
            Assert.Single(stalls);
            Assert.False(Regions.PlazaRect.Contains(stalls[0].Position));
            var general = Regions.TownBuildingById("general");
            Assert.True(OutdoorBuilder.Dist(general.Rect, stalls[0].Position) <= 4, $"{stalls[0].Position}");
        }

        [Fact]
        public void HayvanPazari_TekKatliKulube_AgilYaninda_KapisiYolAginda()
        {
            var (w, res) = Town(1);
            var hut = Regions.TownBuildingById("animals_hut");
            Assert.NotNull(hut);
            Assert.False(w.Map.HasTile(new GridPos(hut.X0, hut.Y0, 1)));    // üst kat yok
            Assert.False(w.Map.HasTile(new GridPos(hut.X0, hut.Y0, -1)));   // bodrum yok
            Assert.True(OutdoorBuilder.Dist(Regions.PenRect, new GridPos(hut.X1, hut.Y0)) <= 4);
            var site = res.Plan.Get("animals_hut");
            Assert.Equal("animals", site.Trade);
            var inside = w.Items.Where(i => hut.Rect.Contains(i.Position) && i.Position.Z == 0).Select(i => i.Def.Id).ToList();
            Assert.Contains("item.hay_bale", inside);
            Assert.Contains("item.saddle_rack", inside);
            Assert.True(OutdoorRules.Walkway(w.Map, new GridPos(hut.X1 + 1, hut.DoorInside.Y)));
        }

        [Fact]
        public void AynaListesi_YaziVeYonTasiyanEsyaAynalanmaz()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var id in new[] { "item.sundial", "item.rug_kufic" })
            {
                Assert.True(db.Items.TryGet(id, out var d), id);
                Assert.False(d.Art.Mirror, id);
                foreach (var r in ItemFacing.ValidRotations(d))
                {
                    Assert.True(ItemFacing.TryView(d, r, false, out _, out bool flip), $"{id} {r}");
                    Assert.False(flip, $"{id} dönüş {r} aynalanıyor");
                }
            }
        }

        [Fact]
        public void Sehir_MeydanKuyusuOrtada_TabelalarVar_AgaclarBinadanUzak()
        {
            var (w, res) = Town(1);
            var c = new GridPos(Regions.Town.X + Regions.PlazaCX, Regions.Town.Y + Regions.PlazaCY);
            Assert.Equal("item.well", w.GetItemAt(c)?.Def.Id);
            int signs = w.Items.Count(i => i.Def.Id == "item.shop_sign");
            Assert.True(signs >= 10, $"tabela {signs}");
            foreach (var t in w.Items.Where(i => OutdoorRules.IsTree(i.Def) && Regions.At(i.Position) == Regions.Town))
                foreach (var b in Regions.TownBuildings) Assert.True(OutdoorBuilder.Dist(b.Rect, t.Position) >= 2, $"{t.Def.Id} {t.Position} {b.Id}");
            int trees = w.Items.Count(i => OutdoorRules.IsTree(i.Def) && Regions.At(i.Position) == Regions.Town);
            Assert.InRange(trees, 25, 400);
        }

        [Fact]
        public void BitkiOrtusu_AyniTohumAyniDagilim()
        {
            string Sig(int seed)
            {
                var (w, _) = Town(seed);
                return string.Join(";", w.Items.Where(i => i.Def.IsPlant && Regions.At(i.Position) == Regions.Town).Select(i => $"{i.Def.Id}{i.Position}").OrderBy(s => s));
            }
            Assert.Equal(Sig(3), Sig(3));
            Assert.NotEqual(Sig(3), Sig(4));
        }

        [Fact]
        public void Sehir_YolBinaninIcindenGecmez()
        {
            var (w, _) = Town(1);
            foreach (var b in Regions.TownBuildings)
                for (int y = b.Y0; y <= b.Y1; y++) for (int x = b.X0; x <= b.X1; x++)
                    Assert.Equal(RoadType.None, w.Map.GetRoad(new GridPos(x, y)));
        }
    }
}
