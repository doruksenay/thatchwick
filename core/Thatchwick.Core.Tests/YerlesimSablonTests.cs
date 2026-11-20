using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    /// <summary>Oda şablonları, dükkân katları ve iç mekân (belge 44 C).</summary>
    public class YerlesimSablonTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        internal static LayoutCatalog LoadLayouts(ContentDatabase db, out List<string> errors)
        {
            errors = new List<string>();
            var path = Path.Combine(ContentFilesTests.ContentDir(), "layouts.json");
            var file = JsonSerializer.Deserialize<LayoutFileDto>(File.ReadAllText(path), Json);
            return LayoutCatalog.Map(file, db, errors);
        }

        private static (WorldState world, StarterWorld.Result res, ContentDatabase db) Town(int seed = 1)
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = LoadLayouts(db, out _);
            var map = StarterWorld.BuildMap(lay);
            var w = new WorldState(map);
            var res = StarterWorld.Place(w, db, lay, seed);
            return (w, res, db);
        }

        [Fact]
        public void Sablonlar_HatasizYuklenir_KoordinatYok()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = LoadLayouts(db, out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.True(lay.Rooms.Count >= 20);
            foreach (var trade in new[] { "tavern", "general", "scribe", "herb", "smithy", "carpenter", "mill", "tailor", "potter", "fisher", "saddler", "mason" })
                Assert.NotNull(lay.Building(trade));
            // Şablon verisinde konum alanı yok: yalnızca kural (çapa, ilişki).
            var raw = File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "layouts.json"));
            Assert.DoesNotContain("\"x\":", raw);
            Assert.DoesNotContain("\"y\":", raw);
        }

        // ---------- oda döşeyici (sentetik oda) ----------

        private static ContentDatabase SmallDb()
        {
            var db = new ContentDatabase();
            var table = new ItemDef { Id = "masa", DisplayName = "masa", Width = 2, Category = "surfaces", Site = ItemSite.Indoor };
            table.SurfaceSlots.Add(new SurfaceSlotDef { Id = "a", U = 0.5f, V = 0.5f });
            table.Belongs.Add("living");
            var chair = new ItemDef { Id = "sandalye", DisplayName = "sandalye", Category = "seating", FrontUse = Placements.Sit, Site = ItemSite.Indoor, Art = new ItemArtDef { Mirror = true } };
            chair.Art.Faces[(int)Dir4.W] = "chair_sw"; chair.Belongs.Add("living");
            var chest = new ItemDef { Id = "sandik", DisplayName = "sandık", FrontUse = Placements.Open, Site = ItemSite.Indoor, Art = new ItemArtDef { Mirror = true } };
            chest.Art.Faces[(int)Dir4.N] = "chest_se"; chest.Belongs.Add("living");
            var cup = new ItemDef { Id = "fincan", DisplayName = "fincan", Placement = ItemPlacement.Surface };
            var big = new ItemDef { Id = "dev", DisplayName = "dev", Width = 6, Height = 6 };
            foreach (var d in new[] { table, chair, chest, cup, big }) db.Items.Register(d);
            return db;
        }

        private static RoomTemplate Template(params (string item, string anchor, string with, int count, bool optional)[] steps)
        {
            var t = new RoomTemplate { Id = "t", Type = "living" };
            foreach (var s in steps)
            {
                var st = new LayoutStep { Anchor = s.anchor ?? Anchors.Any, With = s.with, Count = s.count <= 0 ? 1 : s.count, Optional = s.optional };
                st.Items.Add(s.item);
                t.Steps.Add(st);
            }
            return t;
        }

        /// <summary>İç 1-7 x 1-6 (7x6) oda, güney duvarında x=4'te kapı.</summary>
        private static (WorldState w, RoomSpec spec) Room()
        {
            var m = new GridMap(10, 10);
            for (int x = 1; x <= 7; x++)
            {
                m.SetEdge(new Edge(x, 1, EdgeAxis.Horizontal), EdgeKind.Wall);
                m.SetEdge(new Edge(x, 7, EdgeAxis.Horizontal), x == 4 ? EdgeKind.Door : EdgeKind.Wall);
            }
            for (int y = 1; y <= 6; y++) { m.SetEdge(new Edge(1, y, EdgeAxis.Vertical), EdgeKind.Wall); m.SetEdge(new Edge(8, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            var spec = new RoomSpec { Name = "oda", Level = 0, DoorDir = Dir4.S, Zone = new RoomZone { Type = "living", Level = 0, Rect = new RectI(1, 1, 7, 6) } };
            for (int y = 1; y <= 6; y++) for (int x = 1; x <= 7; x++) spec.Tiles.Add(new GridPos(x, y));
            spec.Entrances.Add(new GridPos(4, 6));
            spec.Keep.Add(new GridPos(4, 6)); spec.Keep.Add(new GridPos(4, 5));
            return (new WorldState(m), spec);
        }

        private static RoomTemplate Dining() => Template(("sandik", Anchors.BackWall, null, 1, false), ("masa", Anchors.Center, null, 1, false),
            ("sandalye", null, Anchors.SeatsAround, 2, false), ("fincan", null, Anchors.OnSurface, 1, true));

        [Fact]
        public void Doseyici_SandalyelerMasayaBakar_DogrulayicidanGecer()
        {
            var db = SmallDb();
            var (w, spec) = Room();
            var rep = new RoomFurnisher(w, db).Furnish(spec, Dining(), 7);
            Assert.True(rep.Ok, string.Join("; ", rep.Missing.Concat(rep.Violations.Select(v => v.ToString()))));
            var table = w.Items.Single(i => i.Def.Id == "masa");
            var tableTiles = table.OccupiedTiles().ToList();
            foreach (var c in w.Items.Where(i => i.Def.Id == "sandalye"))
            {
                var front = ItemFacing.InteractionCells(c).Single();
                Assert.Contains(front, tableTiles);                                    // masaya bakıyor
                Assert.True(Facing.FacesCamera(ItemFacing.Of(c).Value));               // arkası dönük görsel yok: kameraya bakan yan
            }
            var chest = w.Items.Single(i => i.Def.Id == "sandik");
            var f = ItemFacing.Of(chest).Value;
            Assert.All(ItemFacing.BackEdges(chest.Def, chest.Position, chest.Rotation, f), b => Assert.Equal(EdgeKind.Wall, w.Map.GetEdge(b.back)));
            Assert.Empty(w.Items.Where(i => PlacementValidator.Occupies(i)).SelectMany(i => i.OccupiedTiles()).Where(spec.Keep.Contains));
            Assert.Single(w.Items, i => i.Def.Id == "fincan");
            var v = new RoomFurnisher(w, db).Validate(spec);
            Assert.DoesNotContain(v, x => x.Severity == Severity.Error);
        }

        [Fact]
        public void Doseyici_AyniTohumAyniOda_FarkliTohumCesitlilik()
        {
            var db = SmallDb();
            string Layout(int seed)
            {
                var (w, spec) = Room();
                new RoomFurnisher(w, db).Furnish(spec, Dining(), seed);
                return string.Join(";", w.Items.OrderBy(i => i.Def.Id).ThenBy(i => i.Position.X).ThenBy(i => i.Position.Y).Select(i => $"{i.Def.Id}{i.Position}{i.Rotation}"));
            }
            Assert.Equal(Layout(5), Layout(5));
            var distinct = Enumerable.Range(1, 8).Select(Layout).Distinct().Count();
            Assert.True(distinct > 1, "farklı tohumlar hep aynı odayı verdi");
        }

        [Fact]
        public void Doseyici_ZorunluEsyaSigmazsa_EksikRaporlanir_IsteğeBagliAtlanir()
        {
            var db = SmallDb();
            var (w, spec) = Room();
            var rep = new RoomFurnisher(w, db).Furnish(spec, Template(("dev", Anchors.Any, null, 1, false)), 1);
            Assert.Contains("dev", rep.Missing);
            Assert.False(rep.Ok);
            var (w2, spec2) = Room();
            var rep2 = new RoomFurnisher(w2, db).Furnish(spec2, Template(("dev", Anchors.Any, null, 1, true), ("masa", Anchors.Center, null, 1, false)), 1);
            Assert.True(rep2.Ok);
        }

        [Fact]
        public void Doseyici_KapiOnuVeGirisBosKalir_DolasimKopmaz()
        {
            var db = SmallDb();
            var (w, spec) = Room();
            var t = Template(("sandik", Anchors.Wall, null, 6, true), ("masa", Anchors.Any, null, 3, true));
            new RoomFurnisher(w, db).Furnish(spec, t, 3);
            var reach = Reach.From(w.Map, spec.Entrances[0], spec.Tiles.Contains);
            foreach (var it in w.Items.Where(i => i.Def.HasFront))
                foreach (var c in ItemFacing.InteractionCells(it)) Assert.Contains(c, reach);
            Assert.False(w.Map.IsBlocked(new GridPos(4, 5)));
        }

        // ---------- şehir ----------

        [Fact]
        public void Sehir_HerOdaSablondan_DogrulayicidanGecer()
        {
            var (w, res, _) = Town();
            Assert.True(res.Reports.Count >= 40);
            foreach (var r in res.Reports)
                Assert.True(r.Ok, $"{r.Room}: eksik {string.Join(",", r.Missing)} ihlal {string.Join(" | ", r.Violations)}");
        }

        [Fact]
        public void Sehir_FarkliTohumlarda_DeSablonlarGecer()
        {
            foreach (var seed in new[] { 2, 77, -991 })
            {
                var (_, res, _) = Town(seed);
                Assert.All(res.Reports, r => Assert.True(r.Ok, $"tohum {seed}, {r.Room}: eksik {string.Join(",", r.Missing)}"));
            }
        }

        private static readonly Dictionary<string, string[]> Required = new Dictionary<string, string[]>
        {
            ["tavern"] = new[] { "hearth_stone", "copper_pots", "prep_counter", "brew_barrel", "tankard_rack", "singer_stand" },
            ["general"] = new[] { "shop_counter", "pantry_shelf", "storage_jar", "seed_crate", "barrel", "wicker_basket" },
            ["scribe"] = new[] { "writing_lectern", "scroll_rack", "shelf_wood", "ink_quill", "wax_seal", "chest_wood", "letter_box" },
            ["herb"] = new[] { "potion_shelf", "mortar_pestle", "drying_rack", "ceiling_herbs", "bed_straw", "glass_bottle" },
            ["smithy"] = new[] { "forge", "anvil", "bellows", "quench_tub", "whetstone", "tool_rack_wall", "iron_pile" },
            ["carpenter"] = new[] { "workbench", "tool_rack_wall", "lumber_pile", "unfinished_furniture", "table_painted" },
            ["mill"] = new[] { "millstone", "quern", "stone_oven", "kneading_trough", "flour_sack", "bread_rack" },
            ["tailor"] = new[] { "cutting_table", "cloth_bolt_shelf", "spinning_wheel", "loom", "yarn_basket", "garment_stand", "clothes_press" },
            ["potter"] = new[] { "pottery_wheel", "clay_trough", "pot_drying_rack" },
            ["fisher"] = new[] { "fish_basket", "salting_barrel", "net_curtain", "oar_rack", "dried_fish_string" },
            ["saddler"] = new[] { "saddler_bench", "saddle_rack", "hide_rack", "tool_rack_wall" },
            ["mason"] = new[] { "stonecutter", "stone_blocks", "chisel_rack" },
        };

        [Fact]
        public void Sehir_DukkanZemini_EkTablosundakiZorunluEsyalar()
        {
            var (w, _, _) = Town();
            foreach (var kv in Required)
            {
                var b = Regions.TownBuildingById(kv.Key);
                var ids = new HashSet<string>();
                foreach (var it in w.Items)
                {
                    var at = it.IsDecor ? w.GetItem(it.ParentId.Value).Position : it.Position;
                    if (at.Z == 0 && b.Rect.Contains(at)) ids.Add(it.Def.Id);
                }
                foreach (var id in kv.Value) Assert.True(ids.Contains("item." + id), $"{kv.Key}: {id} yok");
            }
        }

        [Fact]
        public void Sehir_KatPlani_EkTablosu()
        {
            var (w, _, _) = Town();
            bool Upper(string id) { var b = Regions.TownBuildingById(id); return w.Map.HasTile(new GridPos(b.X0 + 2, b.Y1, 1)) || w.Map.HasTile(new GridPos(b.X1 - 1, b.Y1, 1)); }
            bool Cellar(string id) { var b = Regions.TownBuildingById(id); return w.Map.HasTile(new GridPos(b.X0 + 1, b.Y0 + 1, -1)); }
            foreach (var id in new[] { "tavern", "general", "scribe", "herb", "carpenter", "mill", "tailor", "potter", "fisher", "saddler" }) Assert.True(Upper(id), id + " üst kat");
            foreach (var id in new[] { "tavern", "general", "scribe", "herb", "mill", "fisher" }) Assert.True(Cellar(id), id + " bodrum");
            foreach (var id in new[] { "smithy", "mason" }) { Assert.False(Upper(id), id); Assert.False(Cellar(id), id); }
            foreach (var id in new[] { "carpenter", "tailor", "potter", "saddler" }) Assert.False(Cellar(id), id);
            // Han: üst katta en az üç kiralık oda (yataklı).
            var tavern = Regions.TownBuildingById("tavern");
            int beds = w.Items.Count(i => i.Position.Z == 1 && tavern.Rect.Contains(i.Position) && i.Def.Id == "item.bed_straw");
            Assert.True(beds >= 4, $"handa yatak {beds}");
        }

        [Fact]
        public void Sehir_DukkanSahibi_UstKattakiEvineYolBulur()
        {
            var (w, _, _) = Town();
            var places = Regions.Places(w.Map);
            var square = places["square"];
            foreach (var key in new[] { "home:mill", "home:general", "home:herb", "home:potter", "home:tavern", "home:scribe", "home:tailor", "home:fisher" })
            {
                var home = places[key];
                Assert.Equal(1, home.Z);
                Assert.False(w.Map.IsBlocked(home), key + " dolu karo");
                Assert.NotNull(Pathfinder.FindPath(w.Map, square, home));
            }
        }

        [Fact]
        public void EskiHarita_UstKatYoksa_SahiplerEskiEvineDuser()
        {
            var map = StarterWorld.BuildMap(null);
            var places = Regions.Places(map);
            var old = Regions.Places();
            Assert.Equal(old["home:2"], places["home:mill"]);
            Assert.Equal(old["home:5"], places["home:general"]);
            Assert.Equal(old["home:tavern"], places["home:tavern"]);
        }

        [Fact]
        public void CiftlikEvi_BaslangicNoktalariVeMerdivenUclariBos()
        {
            var (w, res, _) = Town();
            foreach (var p in new[] { new GridPos(31, 31), new GridPos(33, 31), new GridPos(32, 32), new GridPos(29, 26), new GridPos(35, 26), new GridPos(35, 29, 1), new GridPos(29, 29, -1) })
                Assert.False(w.Map.IsBlocked(p), p.ToString());
            Assert.Contains(w.Items, i => i.Def.Id == "item.hearth_stone" && i.Position.Z == 0 && i.Position.X >= StarterWorld.HouseX0 && i.Position.X <= StarterWorld.HouseX1 && i.Position.Y >= 26 && i.Position.Y <= 32);
            Assert.Equal(3, w.Items.Count(i => i.Def.Id == "item.bed_straw" && i.Position.Z == 1 && i.Position.X >= 28 && i.Position.X <= 36 && i.Position.Y >= 26 && i.Position.Y <= 32));
        }

        [Fact]
        public void EskiKayitGocu_YeniBolgeler_YeniOyunlaAyni_EskiAlanaDokunmaz()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var lay = LoadLayouts(db, out _);
            const int minX = Regions.LegacyWorldWidth;
            var full = new WorldState(StarterWorld.BuildMap(lay));
            StarterWorld.Place(full, db, lay, 7);
            var want = new List<string>();
            foreach (var it in full.Items)
            {
                var root = it.IsDecor ? full.GetItem(it.ParentId.Value) : it;
                bool after = root.Position.X >= minX;
                foreach (var t in root.OccupiedTiles()) if (t.X < minX) after = false;
                if (after) want.Add($"{it.Def.Id}@{root.Position}/{it.Rotation}/{it.SlotId}");
            }

            // eski kayıt: eski alanda oyuncunun eşyası var, yeni bölgeler boş
            var old = new WorldState(StarterWorld.BuildMap(lay));
            db.Items.TryGet("item.chest_wood", out var chest);
            var mine = old.PlaceItem(chest, new GridPos(40, 20));
            int n = StarterWorld.PlaceFrom(old, db, lay, 7, minX);

            Assert.True(n > 0);
            Assert.NotNull(old.GetItem(mine.Id));
            var got = new List<string>();
            foreach (var it in old.Items)
            {
                if (it.Id == mine.Id) continue;
                var root = it.IsDecor ? old.GetItem(it.ParentId.Value) : it;
                Assert.True(root.Position.X >= minX, $"eski alana eşya kondu: {it.Def.Id} {root.Position}");
                got.Add($"{it.Def.Id}@{root.Position}/{it.Rotation}/{it.SlotId}");
            }
            want.Sort(); got.Sort();
            Assert.Equal(want, got);
        }
    }
}
