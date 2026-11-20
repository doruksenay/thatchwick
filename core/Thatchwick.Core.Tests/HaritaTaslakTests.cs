using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Content;
using Thatchwick.Core.Layout;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Harita taslakları ve bina kimlik kartları (belge 64, 64a).</summary>
    public class HaritaTaslakTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static LayoutCatalog Layouts(ContentDatabase db)
        {
            var errs = new List<string>();
            var cat = LayoutCatalog.Map(JsonSerializer.Deserialize<LayoutFileDto>(File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "layouts.json")), Json), db, errs);
            Assert.Empty(errs);
            return cat;
        }

        private static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            var errs = ContentMapper.Apply(ContentFilesTests.LoadAll(), db);
            return db;
        }

        [Fact]
        public void PaletDosyasiKodlaAyni()
        {
            var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "maps", "palette.json")));
            var entries = doc.RootElement.GetProperty("entries").EnumerateArray().ToList();
            Assert.Equal(MapPalette.Entries.Length, entries.Count);
            foreach (var e in MapPalette.Entries)
            {
                string hex = $"#{e.r:X2}{e.g:X2}{e.b:X2}";
                Assert.Contains(entries, x => x.GetProperty("terrain").GetString() == e.name && x.GetProperty("color").GetString() == hex);
            }
        }

        [Fact]
        public void TaslaklarOkunurVeBolgeBoyundadir()
        {
            MapFiles.Reset();
            int found = 0;
            foreach (var r in Regions.All)
            {
                var m = MapFiles.Get(r.Id);
                if (m == null) continue;
                found++;
                Assert.Equal(r.W, m.W); Assert.Equal(r.H, m.H);
                Assert.DoesNotContain(MapTerrain.Unknown, m.Terrain);
                foreach (var p in m.Markers.ponds ?? new MapPondDto[0])
                    Assert.True(m.At(p.x, p.y) == MapTerrain.Water || m.At(p.x, p.y) == MapTerrain.DeepWater, $"{r.Id}: balık noktası suda değil ({p.x},{p.y})");
            }
            Assert.Empty(MapFiles.Errors);
            Assert.True(found >= 1);
        }

        [Fact]
        public void KoyYoluTaslaktanKurulurGecitlerBagli()
        {
            var map = StarterWorld.BuildMap(Layouts(Content()));
            var comp = OutdoorRules.RoadComponents(map, out int main);
            foreach (var q in new[] { new GridPos(100, 40), new GridPos(100, 41), new GridPos(139, 28), new GridPos(139, 29), new GridPos(107, 16), new GridPos(412, 23) })
            {
                Assert.True(comp.TryGetValue(q, out var c), $"{q} yol değil");
                Assert.Equal(main, c);
            }
            // Köprü: dere ile yolun kesiştiği yer ahşap.
            int bridge = 0;
            for (int y = Regions.Road.Y; y < Regions.Road.Y + Regions.Road.H; y++)
                for (int x = Regions.Road.X; x < Regions.Road.X + Regions.Road.W; x++)
                    if (map.GetFloor(new GridPos(x, y)) == FloorType.Wood) bridge++;
            Assert.True(bridge >= 4);
        }

        [Fact]
        public void KenarBandiDoluVeBitkiOrtusuBandaGirmez()
        {
            var db = Content(); var cat = Layouts(db);
            var world = new WorldState(StarterWorld.BuildMap(cat));
            StarterWorld.Place(world, db, cat, 1);
            var data = MapFiles.Get(Regions.Road.Id);
            int band = 0, filled = 0;
            for (int y = 0; y < data.H; y++)
                for (int x = 0; x < data.W; x++)
                {
                    if (data.At(x, y) != MapTerrain.Band) continue;
                    band++;
                    if (world.GetItemAt(new GridPos(Regions.Road.X + x, Regions.Road.Y + y)) != null) filled++;
                }
            Assert.True(filled >= band / 6, $"bant {band}, dolu {filled}");
            // Toplama düğümlerinin hepsi köy yolunda kaldı (belge 64a Aşama D).
            foreach (var id in new[] { "item.forage_berry_bush", "item.forage_strawberry", "item.forage_dry_branch", "item.forage_elderflower", "item.forage_mushroom", "item.forage_wild_garlic", "item.forage_nettle" })
                Assert.Contains(world.Items, it => it.Def.Id == id && Regions.Road.Contains(it.Position));
        }

        [Fact]
        public void HerDukkanKitliKomsularAyirtEdilir()
        {
            var cat = Layouts(Content());
            var kits = BuildingKits.Assign(cat);
            foreach (var b in Regions.TownBuildings) Assert.True(kits.ContainsKey(b.Id), $"{b.Id} kitsiz");
            foreach (var a in Regions.TownBuildings)
                foreach (var b in Regions.TownBuildings)
                {
                    if (string.CompareOrdinal(a.Id, b.Id) >= 0) continue;
                    if (OutdoorBuilder.Dist(a.Rect, new GridPos(b.X0, b.Y0)) > 6 && OutdoorBuilder.Dist(b.Rect, new GridPos(a.X0, a.Y0)) > 6) continue;
                    Assert.False(BuildingKits.Same(kits[a.Id], kits[b.Id]), $"{a.Id} ve {b.Id} dışarıdan aynı");
                }
        }

        [Fact]
        public void DukkandaTekAhsapTonuVeCepheStili()
        {
            var db = Content(); var cat = Layouts(db);
            var map = StarterWorld.BuildMap(cat);
            var world = new WorldState(map);
            StarterWorld.Place(world, db, cat, 1);
            var kits = BuildingKits.Assign(cat);
            foreach (var b in Regions.TownBuildings)
            {
                var woods = world.Items.Where(i => i.Def.FinishGroup == "wood" && b.Rect.Contains(new GridPos(i.Position.X, i.Position.Y))).Select(i => i.FinishId).Distinct().ToList();
                Assert.True(woods.Count <= 1, $"{b.Id}: {string.Join(",", woods)}");
                var k = kits[b.Id];
                if (k.Wall == null) continue;
                for (int x = b.X0; x <= b.X1; x++)
                    foreach (var e in new[] { new Edge(x, b.Y0, EdgeAxis.Horizontal), new Edge(x, b.Y1 + 1, EdgeAxis.Horizontal) })
                        if (map.GetEdge(e) == EdgeKind.Wall) Assert.Equal(k.Wall, map.GetLook(e)?.Style);
            }
        }
    }
}
