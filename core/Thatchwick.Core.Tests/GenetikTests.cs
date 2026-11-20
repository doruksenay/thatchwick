using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Animals.Genetics;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Hayvan genetiği, katır, at tipleri, köy merası ve yayla, böcek ve balık defteri, perhiz, çekirge (Project belgesi 48, 20.11.2026).
    public class GenetikTests
    {
        private sealed class Ctx
        {
            public ContentDatabase Db; public WorldState World; public Household House; public BuildService Build;
            public RoomTemplateService Templates; public AnimalSystem Animals;
        }

        private static Ctx Make(int size = 24)
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(size, size);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) map.SetOwned(new GridPos(x, y), true);
            var c = new Ctx { Db = db, World = new WorldState(map), House = new Household(20000) };
            c.Build = new BuildService(c.World, c.House, db);
            c.Templates = new RoomTemplateService(c.Build, c.World, c.House, db);
            c.Animals = new AnimalSystem(c.World, db) { DiseaseEnabled = false };
            c.Build.HousingResidents = c.Animals.Residents;
            return c;
        }

        private static Genome G(string species, string type, Dictionary<string, (string, string)> loci, bool female = true, int seed = 1)
        {
            var g = GeneticsService.Random(species, type, AnimalLineage.Noble, female, new Random(seed));
            foreach (var kv in loci) g.Loci[kv.Key] = kv.Value;
            return g;
        }

        private static BreedContext Ctx0 => new BreedContext { Comfort = 0, MotherLineage = AnimalLineage.Village, FatherLineage = AnimalLineage.Village };

        // ---------- katalog ----------

        [Fact]
        public void Katalog_AltiKopekAltiKediAltiAtTipi_HepsininAdiVar()
        {
            Assert.True(GeneticsCatalog.TypesOf(GeneticsCatalog.Dog).Count(t => !t.Cross) >= 6);
            Assert.Equal(6, GeneticsCatalog.TypesOf(GeneticsCatalog.Cat).Count());
            Assert.Equal(6, GeneticsCatalog.TypesOf(GeneticsCatalog.Horse).Count(t => !t.Cross));
            var ui = LoadUi();
            foreach (var t in GeneticsCatalog.Types) Assert.True(ui.ContainsKey("breed." + t.Id), "breed." + t.Id);
            foreach (var g in GeneticsCatalog.Gifts) { Assert.True(ui.ContainsKey(g.NameKey), g.NameKey); Assert.True(ui.ContainsKey(g.DescKey), g.DescKey); }
            foreach (var sp in new[] { GeneticsCatalog.Dog, GeneticsCatalog.Cat, GeneticsCatalog.Horse })
                foreach (var s in GeneticsCatalog.Stats(sp)) Assert.True(ui.ContainsKey(GeneStat.NameKey(s)), s);
            foreach (var set in CreatureBook.Sets) Assert.True(ui.ContainsKey(set.NameKey), set.NameKey);
            foreach (var d in GeneDefect.All) Assert.True(ui.ContainsKey(GeneDefect.NameKey(d)), d);
        }

        [Fact]
        public void Gorunus_UretilenHerAnahtarArayuzTablosundaVar()
        {
            var ui = LoadUi();
            var rng = new Random(3);
            foreach (var sp in new[] { GeneticsCatalog.Dog, GeneticsCatalog.Cat, GeneticsCatalog.Horse, GeneticsCatalog.Donkey })
                for (int i = 0; i < 400; i++)
                {
                    var g = GeneticsService.Random(sp, null, AnimalLineage.Noble, i % 2 == 0, rng);
                    if (i % 50 == 0) g.RareCoat = GeneticsCatalog.RareCoats(sp)[0];
                    var p = Phenotype.Of(g, rng.NextDouble(), i);
                    Assert.True(ui.ContainsKey(p.ColorKey), p.ColorKey);
                    if (p.PatternKey != null) Assert.True(ui.ContainsKey(p.PatternKey), p.PatternKey);
                }
        }

        private static Dictionary<string, string> LoadUi()
        {
            string dir = AppContext.BaseDirectory;
            string path = null;
            for (var d = new System.IO.DirectoryInfo(dir); d != null; d = d.Parent)
            {
                var p = System.IO.Path.Combine(d.FullName, "Assets", "Resources", "Localization", "ui.tr.json");
                if (System.IO.File.Exists(p)) { path = p; break; }
            }
            if (path == null) path = "/home/claude/thatchwick/Assets/Resources/Localization/ui.tr.json";
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(path));
        }

        // ---------- Mendel ----------

        [Fact]
        public void Mendel_IkiTasiyicidanDortteBirCekinik()
        {
            var mom = G(GeneticsCatalog.Dog, "dog.shepherd", new Dictionary<string, (string, string)> { ["D"] = ("D", "d") });
            var dad = G(GeneticsCatalog.Dog, "dog.shepherd", new Dictionary<string, (string, string)> { ["D"] = ("d", "D") }, female: false);
            var rng = new Random(11);
            int dd = 0, n = 4000;
            for (int i = 0; i < n; i++)
            {
                var b = GeneticsService.Breed(mom, dad, GeneticsCatalog.Dog, i % 2 == 0, AnimalLineage.Noble, Ctx0, rng).Genome;
                if (b.Count("D", "d") == 2) dd++;
            }
            Assert.InRange(dd / (double)n, 0.22, 0.28);
            Assert.Contains("D:d", mom.Revealed);   // seyreltik yavru doğunca taşıyıcılık açılır
            Assert.Contains("D:d", dad.Revealed);
        }

        [Fact]
        public void Kedi_KizilXeBagli_BagaYalnizDisi_ErkekUcRenkliNadirVeKisir()
        {
            var mom = G(GeneticsCatalog.Cat, "cat.barn", new Dictionary<string, (string, string)> { ["O"] = ("O", "o"), ["W"] = ("w", "w"), ["S"] = ("s", "s") });
            var dad = G(GeneticsCatalog.Cat, "cat.barn", new Dictionary<string, (string, string)> { ["O"] = ("o", Genome.YAllele), ["W"] = ("w", "w") }, female: false);
            var rng = new Random(5);
            int femaleTortie = 0, femaleN = 0, maleRed = 0, maleN = 0, rare = 0;
            for (int i = 0; i < 20000; i++)
            {
                bool female = i % 2 == 0;
                var r = GeneticsService.Breed(mom, dad, GeneticsCatalog.Cat, female, AnimalLineage.Noble, Ctx0, rng);
                var look = Phenotype.Of(r.Genome);
                if (female) { femaleN++; if (look.ColorKey == "look.cat.color.tortie" || look.ColorKey == "look.cat.color.calico") femaleTortie++; }
                else if (r.RareEvent == "event.male_calico") { rare++; Assert.True(r.Genome.Sterile); Assert.Contains(look.ColorKey, new[] { "look.cat.color.tortie", "look.cat.color.calico" }); }
                else { maleN++; Assert.NotEqual("look.cat.color.tortie", look.ColorKey); if (look.ColorKey == "look.cat.color.red") maleRed++; }
            }
            Assert.InRange(femaleTortie / (double)femaleN, 0.45, 0.55);   // annenin O'su yarı yarıya
            Assert.InRange(maleRed / (double)maleN, 0.45, 0.55);
            Assert.InRange(rare, 1, 40);                                   // ~%0,1
        }

        [Fact]
        public void Gorunus_AlexTekirBeyazCorap_BranMaskeliKum()
        {
            var rng = new Random(2);
            var alex = Phenotype.Of(GeneticsService.Alex(false, rng));
            Assert.Equal("look.cat.color.black_tabby", alex.ColorKey);
            Assert.Equal("look.cat.pattern.mackerel", alex.PatternKey);
            Assert.Equal(1, alex.White);
            var bran = Phenotype.Of(GeneticsService.Bran(false, rng));
            Assert.Equal("look.dog.color.sable", bran.ColorKey);
            Assert.True(bran.Mask);
            Assert.True(bran.LongCoat);
        }

        [Fact]
        public void Gorunus_KirAtYaslandikcaAgarir()
        {
            var g = G(GeneticsCatalog.Horse, "horse.rouncey", new Dictionary<string, (string, string)> { ["G"] = ("G", "g"), ["E"] = ("e", "e"), ["Cr"] = ("n", "n") });
            Assert.Equal("look.horse.color.chestnut", Phenotype.Of(g, 0.05).ColorKey);
            Assert.Equal("look.horse.color.grey", Phenotype.Of(g, 0.5).ColorKey);
        }

        // ---------- istatistik, mutasyon, hüner, tip ----------

        [Fact]
        public void Istatistik_IyiEbeveynYuzde55_KonforlaArtar_TavanAsilmaz()
        {
            var mom = G(GeneticsCatalog.Horse, "horse.courser", new Dictionary<string, (string, string)>());
            var dad = G(GeneticsCatalog.Horse, "horse.courser", new Dictionary<string, (string, string)>(), female: false);
            mom.Stats[GeneStat.Speed] = 9; dad.Stats[GeneStat.Speed] = 4;
            int Hi(BreedContext ctx, int seed)
            {
                var rng = new Random(seed); int hi = 0;
                for (int i = 0; i < 4000; i++) if (GeneticsService.Breed(mom, dad, GeneticsCatalog.Horse, true, AnimalLineage.Noble, ctx, rng).Genome.Stat(GeneStat.Speed) >= 9) hi++;
                return hi;
            }
            Assert.InRange(Hi(Ctx0, 1) / 4000.0, 0.52, 0.61);
            var comfy = Ctx0; comfy.Comfort = 100; comfy.HusbandryBonus = 0.10;
            Assert.InRange(Hi(comfy, 2) / 4000.0, 0.71, 0.80);
            var low = GeneticsService.Breed(mom, dad, GeneticsCatalog.Horse, true, AnimalLineage.Village, Ctx0, new Random(3)).Genome;
            Assert.True(low.Stats.Values.All(v => v <= GeneStat.Cap(AnimalLineage.Village)));
        }

        [Fact]
        public void Huner_NadirlikDagilimi_EfsaneviYuzdeIki()
        {
            var rng = new Random(9);
            var counts = new int[4];
            for (int i = 0; i < 50000; i++) counts[(int)GeneticsService.RollRarity(rng)]++;
            Assert.InRange(counts[0] / 50000.0, 0.58, 0.62);
            Assert.InRange(counts[3] / 50000.0, 0.015, 0.025);
        }

        [Fact]
        public void Tip_AyniTipAyniKalir_TaziZagardanKirmaTaziCikabilir()
        {
            var rng = new Random(4);
            var a = G(GeneticsCatalog.Dog, "dog.sighthound", new Dictionary<string, (string, string)>());
            var b = G(GeneticsCatalog.Dog, "dog.scenthound", new Dictionary<string, (string, string)>(), female: false);
            Assert.Equal("dog.sighthound", GeneticsService.ChildType(a, a, GeneticsCatalog.Dog, false, rng));
            int lurcher = 0;
            for (int i = 0; i < 2000; i++) if (GeneticsService.ChildType(a, b, GeneticsCatalog.Dog, false, rng) == "dog.lurcher") lurcher++;
            Assert.InRange(lurcher / 2000.0, 0.30, 0.40);
        }

        [Fact]
        public void Genom_KayitGidipGelir()
        {
            var g = GeneticsService.Random(GeneticsCatalog.Cat, "cat.van", AnimalLineage.Good, false, new Random(1));
            g.Gifts.Add(Gift.NineLives); g.Defects.Add(GeneDefect.Timid); g.RareCoat = "rare.smoke"; g.Revealed.Add("D:d"); g.Inbreeding = 0.25;
            var back = Genome.Import(GeneticsCatalog.Cat, g.Export());
            Assert.Equal(g.Export(), back.Export());
            Assert.Equal("cat.van", back.TypeId);
            Assert.Equal(Genome.YAllele, back.Loci["O"].b);
        }

        // ---------- akrabalık ----------

        [Fact]
        public void Akrabalik_KardesEslesmesiCeyrek()
        {
            var table = new Dictionary<int, (int m, int f)> { [1] = (0, 0), [2] = (0, 0), [3] = (1, 2), [4] = (1, 2), [5] = (3, 0) };
            bool Look(int id, out int m, out int f, out double i) { i = 0; if (table.TryGetValue(id, out var p)) { m = p.m; f = p.f; return true; } m = f = 0; return false; }
            Assert.Equal(0.25, GeneticsService.Inbreeding(3, 4, Look), 6);     // kardeş
            Assert.Equal(0.125, GeneticsService.Inbreeding(5, 4, Look), 6);    // yeğen-amca
            Assert.Equal(0.0, GeneticsService.Inbreeding(1, 2, Look), 6);
        }

        // ---------- üreme: köpek, katır ----------

        [Fact]
        public void Kopek_KulubeyleYavrular_YavrununGenomuVeSoyuVar()
        {
            var x = Make();
            var dog = x.Db.Animals.Get("animal.dog");
            Assert.True(dog.Breeds);
            x.Animals.HusbandryLevel = () => 10;
            var k1 = x.World.PlaceItem(x.Db.Items.Get("item.kennel"), new GridPos(3, 3));
            var k2 = x.World.PlaceItem(x.Db.Items.Get("item.kennel"), new GridPos(6, 3));
            for (int i = 0; i < 4; i++) x.World.PlaceItem(x.Db.Items.Get("item.kennel"), new GridPos(3 + 3 * i, 8));
            var mom = x.Animals.Restore(1, dog, new GridPos(3, 4), k1.Id, 100f, 10, 0, 0, friendship: 80f, comfort: 60f, female: true);
            var dad = x.Animals.Restore(2, dog, new GridPos(6, 4), k2.Id, 100f, 10, 0, 0, friendship: 80f, comfort: 60f, female: false);
            Assert.NotNull(mom.Genome);
            var born = new List<Animal>();
            x.Animals.Born += (m, b) => born.Add(b);
            for (int d = 0; d < 300 && born.Count == 0; d++) x.Animals.OnNewDay(Season.Spring);
            Assert.NotEmpty(born);
            var pup = born[0];
            Assert.NotNull(pup.Genome);
            Assert.Equal(1, pup.MotherId);
            Assert.Equal(2, pup.FatherId);
            Assert.Equal((1, 2), x.Animals.ParentsOf(pup.Id));
            Assert.False(x.Animals.SetLook(pup, new Dictionary<string, string> { ["E"] = "e" }));   // yavrunun görünüşü genden
        }

        [Fact]
        public void Katir_KisrakVeEsekAygirindan_KisirVeGucluDogar()
        {
            var x = Make();
            var r = x.Templates.Build("template.stable_room", new GridPos(2, 2), out var placed);
            Assert.True(r.Ok, r.Reason);
            // Şablonda tek bölme var: odaya iki bölme daha koy (kısrak, eşek aygırı, tay).
            var room = x.World.Map.Rooms.RoomAt(new GridPos(3, 3));
            Assert.NotNull(room);
            int extra = 0;
            foreach (var t in room.Tiles)
            {
                if (extra >= 2) break;
                x.House.AddItem("item.horse_stall", 1);
                if (x.Build.PlaceFromInventory("item.horse_stall", t, 0, out _).Ok) extra++;
                else x.House.TryTakeItem("item.horse_stall");
            }
            var stalls = x.World.Items.Where(i => i.Def.Id == "item.horse_stall").ToList();
            Assert.True(stalls.Count >= 3, "bölme sayısı " + stalls.Count);
            x.Animals.HusbandryLevel = () => 10;
            var horse = x.Db.Animals.Get("animal.horse");
            var donkey = x.Db.Animals.Get("animal.donkey");
            var mare = x.Animals.Restore(1, horse, stalls[0].Position, stalls[0].Id, 100f, 30, 0, 0, friendship: 80f, comfort: 60f, female: true, lineage: (int)AnimalLineage.Noble);
            var jack = x.Animals.Restore(2, donkey, stalls[1].Position, stalls[1].Id, 100f, 30, 0, 0, friendship: 80f, comfort: 60f, female: false, lineage: (int)AnimalLineage.Noble);
            mare.Genome.Stats[GeneStat.Carry] = 6; jack.Genome.Stats[GeneStat.Carry] = 8;
            Assert.Null(x.Animals.BreedBlocker(mare));
            Animal foal = null;
            x.Animals.Born += (m, b) => foal = b;
            for (int d = 0; d < 400 && foal == null; d++) x.Animals.OnNewDay(Season.Spring);
            Assert.NotNull(foal);
            Assert.Equal("animal.mule", foal.Species.Id);
            Assert.True(foal.IsSterile);
            Assert.False(foal.Species.Breeds);
            Assert.True(foal.Genome.Stat(GeneStat.Carry) >= 8, "taşıma " + foal.Genome.Stat(GeneStat.Carry));   // ortalama 7 + melez gücü
            Assert.Contains("Kısır", x.Animals.BreedBlocker(foal));
            Assert.True(GeneticsService.SaddleBagSlots(foal.Genome) > HorseAbilities.SaddleBagSlots + 6, "heybe " + GeneticsService.SaddleBagSlots(foal.Genome));
        }

        [Fact]
        public void Melez_YalnizTablodakiCiftler()
        {
            Assert.Equal(GeneticsCatalog.Mule, GeneticsCatalog.CrossFor(GeneticsCatalog.Horse, GeneticsCatalog.Donkey).Offspring);
            Assert.Equal(GeneticsCatalog.Hinny, GeneticsCatalog.CrossFor(GeneticsCatalog.Donkey, GeneticsCatalog.Horse).Offspring);
            Assert.Null(GeneticsCatalog.CrossFor("animal.cow", GeneticsCatalog.Horse));
            Assert.Null(GeneticsCatalog.CrossFor(GeneticsCatalog.Dog, GeneticsCatalog.Cat));
        }

        // ---------- binek ----------

        [Fact]
        public void Binek_TipHiziEtkiler_EskiKayitNotr_CocukYalnizMidilliye()
        {
            var x = Make();
            var horse = x.Db.Animals.Get("animal.horse");
            var legacy = x.Animals.Restore(10, horse, new GridPos(4, 4), 0, 100f, 40, 0, 0, 60f, 0, "item.saddle");
            Assert.Equal("horse.rouncey", legacy.TypeId);
            Assert.Equal(1.0, GeneticsService.RideFactor(legacy.Genome), 6);
            var courser = GeneticsService.Random(GeneticsCatalog.Horse, "horse.courser", AnimalLineage.Noble, true, new Random(1));
            courser.Stats[GeneStat.Speed] = 8;
            Assert.True(GeneticsService.RideFactor(courser) > 1.05);

            var child = new Character(5, "Çocuk", new GridPos(4, 4), true);
            for (int d = 1; child.Stage != AgeStage.Child && d < 400; d++) child.SetAge(d);
            Assert.Equal(AgeStage.Child, child.Stage);
            x.Animals.FindCharacter = id => id == child.Id ? child : null;
            legacy.Friendship = 80f;
            Assert.Contains("midilli", x.Animals.MountBlocker(child.Id, legacy));
            var pony = x.Animals.Restore(11, horse, new GridPos(4, 5), 0, 100f, 40, 0, 0, 80f, 0, "item.saddle",
                genome: GeneticsService.Random(GeneticsCatalog.Horse, "horse.pony", AnimalLineage.Village, true, new Random(2)).Export());
            Assert.True(pony.ChildMount);
            Assert.Null(x.Animals.MountBlocker(child.Id, pony));
        }

        // ---------- köy merası ve yayla ----------

        [Fact]
        public void Mera_KotaVeYayla_PeynirBirikir_KisinDoner()
        {
            var x = Make();
            var barn = x.World.PlaceItem(x.Db.Items.Get("item.barn"), new GridPos(5, 5));
            var cow = x.Db.Animals.Get("animal.cow");
            var sheep = x.Db.Animals.Get("animal.sheep");
            int rep = 0;
            x.Animals.ReputationNow = () => rep;
            Assert.Equal(AnimalSystem.StintBase, x.Animals.StintQuota);
            rep = 250;
            Assert.Equal(AnimalSystem.StintBase + 2 * AnimalSystem.StintPer100Reputation, x.Animals.StintQuota);

            var milker = x.Animals.Restore(1, cow, new GridPos(5, 6), barn.Id, 100f, 12, 0, 0, friendship: 50f, female: true);
            var ewe = x.Animals.Restore(2, sheep, new GridPos(6, 6), barn.Id, 100f, 8, 0, 0, friendship: 50f, female: true);
            Assert.Contains("Kış", x.Animals.SendAwayBlocker(milker, AwayPlace.Yayla, Season.Winter));
            Assert.Contains("ilkbahar", x.Animals.SendAwayBlocker(milker, AwayPlace.Yayla, Season.Autumn));
            Assert.True(x.Animals.SendAway(milker, AwayPlace.Yayla, Season.Summer, 0), x.Animals.SendAwayBlocker(milker, AwayPlace.Yayla, Season.Summer));
            Assert.True(x.Animals.SendAway(ewe, AwayPlace.Common, Season.Summer, 0), x.Animals.SendAwayBlocker(ewe, AwayPlace.Common, Season.Summer));
            Assert.DoesNotContain(milker, x.Animals.Animals);
            Assert.Equal(1, x.Animals.AwayCount(AwayPlace.Yayla));

            for (int d = 0; d < 6; d++) x.Animals.AwayDay(Season.Summer, x.House, d);
            var w = x.Animals.Away.FirstOrDefault(a => a.Animal == milker);
            if (w != null) Assert.True(w.Stock.TryGetValue("item.cheese", out var cheese) && cheese > 0, "stok: " + string.Join(",", w.Stock.Select(kv => kv.Key + "=" + kv.Value)));

            // Kayıt gidip gelir.
            var saved = x.Animals.ExportAway();
            var y = Make();
            y.World.PlaceItem(y.Db.Items.Get("item.barn"), new GridPos(5, 5));
            y.Animals.ImportAway(saved, y.Db);
            Assert.Equal(x.Animals.Away.Count, y.Animals.Away.Count);

            int back = -1;
            x.Animals.AwayReturned += (b, s) => back = b;
            x.Animals.AwayDay(Season.Winter, x.House, 10);
            Assert.Empty(x.Animals.Away);
            Assert.True(back >= 1, "dönen " + back);
        }

        // ---------- gölet, defter, perhiz, çekirge ----------

        [Fact]
        public void Golet_CamurUremeyiYavaslatir_TemizlikGubreVerir()
        {
            var map = new GridMap(10, 10);
            for (int i = 0; i < 3; i++) map.SetFloor(new GridPos(2 + i, 2), FloorType.Water);
            map.RestorePond(new GridPos(2, 2), 1f, spring: true, insects: true, filledDays: 30, new[] { new KeyValuePair<string, int>("item.fish_carp", 2) }, silt: 90f);
            var pond = map.PondAt(new GridPos(2, 2));
            Assert.True(pond.BreedFactor < 0.5);
            int manure = map.CleanPond(new GridPos(2, 2));
            Assert.Equal(4, manure);
            Assert.Equal(0f, pond.Silt);
            map.OnNewDayPonds(0f, 10f, false, new Random(1));
            Assert.True(pond.Silt > 0f);
        }

        [Fact]
        public void Defter_SetTamamlaninca_EfsaneviDahil()
        {
            var book = new CreatureBook();
            var set = CreatureBook.Sets.First(s => s.Id == "fish_river");
            CreatureSet done = null;
            book.SetCompleted += s => done = s;
            foreach (var m in set.Members) book.Record(m);
            Assert.Null(done);
            Assert.False(book.Record(set.Members[0]));   // ikinci kez yeni değil
            book.Record(set.Legendary);
            Assert.Same(set, done);
            var back = new CreatureBook();
            back.Import(book.Export());
            Assert.True(back.IsDone(set));
            Assert.True(back.TryClaimLegendFish("item.fish_sturgeon_old", 2));
            Assert.False(back.TryClaimLegendFish("item.fish_sturgeon_old", 2));
        }

        [Fact]
        public void Defter_HerKayitIcerikteVar()
        {
            var db = ContentFilesTests.LoadDb(out _);
            foreach (var s in CreatureBook.Sets)
            {
                Assert.True(db.Items.Contains(s.Legendary), s.Legendary);
                foreach (var m in s.Members) Assert.True(db.Items.Contains(m), m);
            }
            foreach (var e in CreatureBook.Extras) Assert.True(db.Items.Contains(e), e);
            // Her efsanevi böcek bir toplama yerinden yakalanabilir.
            foreach (var s in CreatureBook.Sets.Where(z => z.Kind == CreatureKind.Insect))
                Assert.Contains(db.Items.All, i => i.Catches.Any(c => c.Item == s.Legendary));
        }

        [Fact]
        public void Perhiz_BalikPahali_EtUcuz_YalnizPerhizde()
        {
            var db = ContentFilesTests.LoadDb(out _);
            var fish = db.Items.Get("item.fish_carp");
            Assert.True(FastingCalendar.IsFasting(Season.Spring, FastingCalendar.SpringStart));
            Assert.False(FastingCalendar.IsFasting(Season.Summer, FastingCalendar.SpringStart));
            Assert.Equal(FastingCalendar.FishFactor, FastingCalendar.PriceFactor(fish, true));
            Assert.Equal(1.0, FastingCalendar.PriceFactor(fish, false));
        }

        [Fact]
        public void Cekirge_OnceUyari_SonraEkinYer_KumesYariyaIndirir()
        {
            var l = new LocustSwarm(1);
            Assert.Equal(1, l.OnNewDay(Season.Summer, false, 10, 0, out _, force: true));
            Assert.Equal(2, l.OnNewDay(Season.Summer, false, 10, 0, out int eaten));
            Assert.Equal(LocustSwarm.CropsMax, eaten);
            l.OnNewDay(Season.Summer, false, 10, 5, out _, force: true);
            l.OnNewDay(Season.Summer, false, 10, 5, out int halved);
            Assert.Equal(LocustSwarm.CropsMax / 2, halved);
            Assert.Equal(0, l.OnNewDay(Season.Winter, false, 10, 0, out _));
        }

        // ---------- efsane yolları ----------

        [Fact]
        public void Efsane_KatirSeferle_KediFareyle()
        {
            var x = Make();
            var mule = x.Animals.Restore(1, x.Db.Animals.Get("animal.mule"), new GridPos(4, 4), 0, 100f, 40, 0, 0, 60f, 0, null);
            Assert.Equal("mule.common", mule.TypeId);
            AnimalProgress.AddXp(mule, 10000);
            for (int i = 0; i < PetLegends.TripsNeeded - 1; i++) Assert.False(x.Animals.AddDeed(mule, PetLegends.Trips));
            Assert.True(x.Animals.AddDeed(mule, PetLegends.Trips));
            Assert.Equal("Dağ Aşan", mule.LegendaryTitle);

            var cat = x.Animals.Restore(2, x.Db.Animals.Get("animal.cat"), new GridPos(4, 5), 0, 100f, 20, 0, 0, 60f, 0, null, lineage: (int)AnimalLineage.Village);
            Assert.False(x.Animals.AddDeed(cat, PetLegends.Rats, 100));   // Köy kedisi: Soylu + 5. seviye kuralı
            Assert.Null(cat.LegendaryTitle);
        }

        [Fact]
        public void Yarisma_TaziKosusuVeKediYarismasi_KatirAtYarisinaGiremez()
        {
            var x = Make();
            var handler = new Character(1, "Usta", new GridPos(4, 4), true);
            handler.SetAge(40);
            var dog = x.Animals.Restore(1, x.Db.Animals.Get("animal.dog"), new GridPos(4, 4), 0, 100f, 20, 0, 0, 60f, 0, null,
                genome: GeneticsService.Random(GeneticsCatalog.Dog, "dog.sighthound", AnimalLineage.Noble, true, new Random(1)).Export());
            Assert.Null(AnimalContests.Blocker(ContestKind.HoundRace, dog, handler));
            Assert.True(ContestBook.RunsAt(ContestKind.HoundRace, "festival.harvest", out bool main) && main);
            Assert.True(ContestBook.RunsAt(ContestKind.CatShow, "festival.fair", out main) && main);
            var mule = x.Animals.Restore(2, x.Db.Animals.Get("animal.mule"), new GridPos(4, 5), 0, 100f, 40, 0, 0, 60f, 0, "item.saddle");
            Assert.NotNull(AnimalContests.Blocker(ContestKind.HorseRace, mule, handler));
        }
    }
}
