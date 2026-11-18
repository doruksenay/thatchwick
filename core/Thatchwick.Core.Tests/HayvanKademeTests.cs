using System;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Hayvan kademeleri, at yetenekleri, yarışmalar ve efsane (karar 18.11.2026, Project belgesi 29).
    public class HayvanKademeTests
    {
        private sealed class Ctx { public ContentDatabase Db; public WorldState World; public Household House; public AnimalSystem Animals; }

        private static Ctx Make()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var map = new GridMap(16, 16);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) map.SetOwned(new GridPos(x, y), true);
            var w = new WorldState(map);
            return new Ctx { Db = db, World = w, House = new Household(5000), Animals = new AnimalSystem(w, db) { DiseaseEnabled = false } };
        }

        private static Animal Horse(Ctx x, int id = 50, int lineage = 0, double xp = 0, float friendship = 60f, float stamina = -1f, double shy = 0) =>
            x.Animals.Restore(id, x.Db.Animals.Get("animal.horse"), new GridPos(4, 4), 0, 100f, 40, 0, 0, friendship, 0, "item.saddle",
                lineage: lineage, xp: xp, stamina: stamina, shyDays: shy);

        private static Character Rider(int id = 1, int ridingLevel = 1)
        {
            var c = new Character(id, "Binici" + id, new GridPos(4, 4), true) { CanStarve = false };
            c.SetAge(40);
            if (ridingLevel > 1) c.Skills.RestoreXp(SkillType.Riding, SkillRules.XpForLevel(ridingLevel));
            return c;
        }

        private static void Mount(Ctx x, Character rider, Animal horse)
        {
            x.Animals.FindCharacter = id => id == rider.Id ? rider : null;
            Assert.True(x.Animals.Mount(rider.Id, horse));
        }

        // ---------- soy ve seviye ----------

        [Fact]
        public void Seviye_SoyTavaniniAsmaz()
        {
            var x = Make();
            var village = Horse(x, 50, (int)AnimalLineage.Village);
            var noble = Horse(x, 51, (int)AnimalLineage.Noble);
            AnimalProgress.AddXp(village, 10000);
            AnimalProgress.AddXp(noble, 10000);
            Assert.Equal(3, village.Level);
            Assert.Equal(5, noble.Level);
            Assert.Equal(4, AnimalProgress.MaxLevelFor(AnimalLineage.Good));
            Assert.Equal(1, AnimalProgress.LevelForXp(19, AnimalLineage.Noble));
            Assert.Equal(2, AnimalProgress.LevelForXp(20, AnimalLineage.Noble));
        }

        [Fact]
        public void Seviye_SevmeTimarVeBinmeXpVerir()
        {
            var x = Make();
            var h = Horse(x);
            Assert.True(x.Animals.PetOne(h));
            Assert.Equal(AnimalProgress.XpPet, h.Xp, 6);
            var h2 = Horse(x, 51);
            Assert.True(x.Animals.Brush(h2));
            Assert.Equal(AnimalProgress.XpBrush, h2.Xp, 6);
            var r = Rider(1, 5);
            Mount(x, r, h);
            x.Animals.RiderPosition = id => r.Position;
            double before = h.Xp;
            r.Position = new GridPos(7, 4);
            x.Animals.Tick(0.01);
            Assert.True(h.Xp > before);
        }

        [Fact]
        public void Kalite_SoyVeSeviyeEkler()
        {
            Assert.Equal(0f, AnimalProgress.QualityBonus(AnimalLineage.Village, 1));
            Assert.Equal(10f + 8f, AnimalProgress.QualityBonus(AnimalLineage.Noble, 5));
            // Aynı dostluk/konforda Soylu ve 5. seviye hayvan bir basamak üste çıkar.
            Assert.Equal(0, AnimalSystem.QualityTierOf(50f, 50f, false));
            Assert.Equal(1, AnimalSystem.QualityTierOf(50f, 50f, false, AnimalProgress.QualityBonus(AnimalLineage.Noble, 5)));
        }

        [Fact]
        public void Kalitim_OrtalamaArtiSans_EfsaneSoyuEtiketi()
        {
            var x = Make();
            var mom = Horse(x, 50, (int)AnimalLineage.Good);
            var dad = Horse(x, 51, (int)AnimalLineage.Noble);
            // Ortalama (1+2)/2 = 1; zar 0,9 → değişmez.
            Assert.Equal(AnimalLineage.Good, AnimalProgress.InheritLineage(new FixedRandom(0.9), mom, dad, 0, out var d1));
            Assert.False(d1);
            // Zar 0,1 < %15 → bir üst.
            Assert.Equal(AnimalLineage.Noble, AnimalProgress.InheritLineage(new FixedRandom(0.1), mom, dad, 0, out _));
            // Zar 0,2: %15 ile %25 arası → bir alt.
            Assert.Equal(AnimalLineage.Village, AnimalProgress.InheritLineage(new FixedRandom(0.2), mom, dad, 0, out _));
            // Efsane ebeveyn: Soylu sayılır, üst şansı +%20 ve etiket; Soylu'yu aşmaz.
            var legend = Horse(x, 52, (int)AnimalLineage.Legendary);
            var l = AnimalProgress.InheritLineage(new FixedRandom(0.3), legend, legend, 0, out var d2);
            Assert.True(d2);
            Assert.Equal(AnimalLineage.Noble, l);
        }

        [Fact]
        public void Efsane_YalnizcaSoyluVe5Seviye()
        {
            var x = Make();
            var good = Horse(x, 50, (int)AnimalLineage.Good, 10000);
            Assert.False(AnimalProgress.GrantLegend(good, "Panayır Rüzgârı"));
            var noble = Horse(x, 51, (int)AnimalLineage.Noble, 100);
            Assert.False(AnimalProgress.GrantLegend(noble, "Panayır Rüzgârı"));
            AnimalProgress.AddXp(noble, 10000);
            Assert.True(AnimalProgress.GrantLegend(noble, "Panayır Rüzgârı"));
            Assert.Equal(AnimalLineage.Legendary, noble.Lineage);
            Assert.Equal("Panayır Rüzgârı", noble.LegendaryTitle);
            Assert.False(AnimalProgress.GrantLegend(noble, "x"));   // bir kez
            Assert.Equal("Hasat Güzeli", AnimalProgress.TitleFor("animal.cow"));
        }

        // ---------- at yetenekleri ----------

        [Fact]
        public void Yetenek_AtVeBiniciSeviyesiIster()
        {
            var x = Make();
            var h = Horse(x, 50, (int)AnimalLineage.Noble, AnimalProgress.XpForLevel(3));
            var novice = Rider(1, 1);
            var expert = Rider(2, 4);
            Assert.Equal("At 2 · Binicilik 2", HorseAbilities.Blocker(HorseAbility.Gallop, Horse(x, 51), expert));
            Assert.Equal("At 3 · Binicilik 4", HorseAbilities.Blocker(HorseAbility.Jump, h, novice));
            Assert.Null(HorseAbilities.Blocker(HorseAbility.Jump, h, expert));
            Assert.NotNull(HorseAbilities.Blocker(HorseAbility.HighJump, Horse(x, 52, (int)AnimalLineage.Good, 10000), Rider(3, 10)));
        }

        [Fact]
        public void Dortnala_HizlandirirDayaniklilikBitinceDurur()
        {
            var x = Make();
            var h = Horse(x, 50, (int)AnimalLineage.Good, AnimalProgress.XpForLevel(2));
            var r = Rider(1, 2);
            Mount(x, r, h);
            x.Animals.RiderPosition = id => r.Position;
            double walk = x.Animals.RideSpeed(r.Id);
            Assert.True(x.Animals.SetGallop(r.Id, true));
            Assert.Equal(walk * HorseAbilities.GallopSpeed, x.Animals.RideSpeed(r.Id), 6);
            for (int i = 0; i < 40 && h.Galloping; i++) x.Animals.Tick(0.01);
            Assert.False(h.Galloping);
            Assert.Equal(0f, h.Stamina);
            Assert.False(x.Animals.SetGallop(r.Id, true));
            Assert.True(x.Animals.GiveTreat(h));
            Assert.Equal(HorseAbilities.TreatStamina, h.Stamina);
            Assert.True(x.Animals.SetGallop(r.Id, true));
            // Yetersiz binici dörtnala açamaz.
            var y = Make();
            var h2 = Horse(y, 60, (int)AnimalLineage.Good, AnimalProgress.XpForLevel(2));
            var weak = Rider(2, 1);
            Mount(y, weak, h2);
            Assert.False(y.Animals.SetGallop(weak.Id, true));
        }

        [Fact]
        public void Atlama_SansFormuluVeSinirlar()
        {
            var x = Make();
            var h = Horse(x, 50, (int)AnimalLineage.Noble, AnimalProgress.XpForLevel(3), friendship: 0f);
            var r = Rider(1, 4);
            Assert.Equal(0.85, HorseAbilities.JumpChance(h, r, 2, false), 6);
            Assert.Equal(0.75, HorseAbilities.JumpChance(h, r, 2, true), 6);
            var tired = Horse(x, 52, (int)AnimalLineage.Noble, AnimalProgress.XpForLevel(3), 0f, stamina: 10f);
            Assert.Equal(0.60, HorseAbilities.JumpChance(tired, r, 2, true), 6);
            Assert.Equal(0.0, HorseAbilities.JumpChance(h, r, 3, false), 6);   // 3 karo: at 5, Binicilik 8
            var top = Horse(x, 51, (int)AnimalLineage.Noble, 10000, 100f);
            Assert.Equal(0.99, HorseAbilities.JumpChance(top, Rider(2, 10), 2, false), 6);
        }

        [Fact]
        public void Atlama_BasarisizlikUrkutur_DusenBiniciYaralanirAmaOlmez()
        {
            var x = Make();
            var r = Rider(1, 4);
            r.MoodEnabled = true;
            x.Animals.FindCharacter = id => id == r.Id ? r : null;
            var pop = new Population(); pop.Add(r);
            var injuries = new InjurySystem(pop, seed: 3);
            Character fallen = null;
            x.Animals.RiderFell += (a, c) => { fallen = c; injuries.InjureById(c, "horse_fall"); };
            // Yağmur ve yorgun at: şans %60. Her denemede taze at; düşme, ürkme ve geçiş hepsi görülür.
            bool sawFall = false, sawRefused = false, sawCleared = false;
            for (int i = 0; i < 300 && !(sawFall && sawRefused && sawCleared); i++)
            {
                var h = Horse(x, 100 + i, (int)AnimalLineage.Noble, AnimalProgress.XpForLevel(3), 60f, stamina: 5f);
                Assert.True(x.Animals.Mount(r.Id, h));
                var outcome = x.Animals.TryJump(r.Id, 2, badWeather: true);
                if (outcome == JumpOutcome.Fell) { sawFall = true; Assert.Equal(0, h.RiderId); Assert.True(h.ShyDays > 0); }
                if (outcome == JumpOutcome.Refused) { sawRefused = true; Assert.Equal(r.Id, h.RiderId); Assert.True(h.ShyDays > 0); Assert.True(h.Friendship < 60f); }
                if (outcome == JumpOutcome.Cleared) sawCleared = true;
                if (h.RiderId != 0) x.Animals.Dismount(r.Id);
            }
            Assert.True(sawFall && sawRefused && sawCleared);
            Assert.Same(r, fallen);
            Assert.NotNull(r.Injury);
            Assert.Equal("horse_fall", r.Injury.Kind);
            Assert.False(r.IsDead);
            // Ürkek atla atlanamaz.
            var shy = Horse(x, 999, (int)AnimalLineage.Noble, AnimalProgress.XpForLevel(3), 60f, shy: 1);
            Assert.True(x.Animals.Mount(r.Id, shy));
            Assert.Equal(JumpOutcome.NotAllowed, x.Animals.TryJump(r.Id, 2));
        }

        // ---------- yarışmalar ----------

        [Fact]
        public void Yaris_KatilimSartlari()
        {
            var x = Make();
            var r = Rider(1, 3);
            var low = Horse(x, 50, 0, AnimalProgress.XpForLevel(2));
            Assert.Contains("3. seviye", AnimalContests.Blocker(ContestKind.HorseRace, low, r));
            var ok = Horse(x, 51, 0, AnimalProgress.XpForLevel(3));
            Assert.Null(AnimalContests.Blocker(ContestKind.HorseRace, ok, r));
            var teen = Rider(2); teen.SetAge(AgeConfig.Default.ChildStartDay + 0.1);
            Assert.NotNull(AnimalContests.Blocker(ContestKind.HorseRace, ok, teen));
        }

        [Fact]
        public void Yaris_SansEnCok70VeDeterministik()
        {
            var x = Make();
            var r = Rider(1, 10);
            var star = Horse(x, 50, (int)AnimalLineage.Noble, 10000, 100f);
            double c1 = AnimalContests.EstimateWinChance(ContestKind.HorseRace, star, r, 0, out _);
            double c2 = AnimalContests.EstimateWinChance(ContestKind.HorseRace, star, r, 0, out _);
            Assert.Equal(c1, c2, 9);
            Assert.True(c1 <= AnimalContests.MaxWinChance + 1e-9);
            Assert.True(c1 > 0.3, "güçlü at " + c1);
            var weak = Horse(x, 51, 0, AnimalProgress.XpForLevel(3), 10f);
            Assert.True(AnimalContests.EstimateWinChance(ContestKind.HorseRace, weak, Rider(2, 1), 0, out _) < c1);
        }

        [Fact]
        public void Yaris_KazananSoyluAtEfsaneOlur_KoyKosusuUnvanVermez()
        {
            var x = Make();
            var r = Rider(1, 10);
            Animal winner = null; ContestResult res = null;
            for (int seed = 0; seed < 50 && winner == null; seed++)
            {
                var y = Make();
                var h = Horse(y, 50, (int)AnimalLineage.Noble, 10000, 100f);
                var side = AnimalContests.Run(new Random(seed), ContestKind.HorseRace, h, r, 0, mainEvent: false);
                Assert.Null(side.LegendTitle);
                var main = AnimalContests.Run(new Random(seed), ContestKind.HorseRace, h, r, 0, mainEvent: true);
                if (main.Won) { winner = h; res = main; }
            }
            Assert.NotNull(winner);
            Assert.Equal("Panayır Rüzgârı", res.LegendTitle);
            Assert.Equal(AnimalLineage.Legendary, winner.Lineage);
            Assert.Equal(120, res.PrizeAkce);
            Assert.Contains("efsane", res.Summary);
            Assert.True(AnimalContests.BetPayout(10, 0.25) == 36);
        }

        [Fact]
        public void Yarisma_IneklerVeOkuzler()
        {
            var x = Make();
            var cowDef = x.Db.Animals.Get("animal.cow");
            var cow = x.Animals.Restore(70, cowDef, new GridPos(2, 2), 0, 100f, 40, 0, 0, 80f, female: true);
            var ox = x.Animals.Restore(72, cowDef, new GridPos(2, 3), 0, 100f, 40, 0, 0, 80f, female: false);
            var r = Rider(1);
            Assert.Null(AnimalContests.Blocker(ContestKind.BeautyContest, cow, r));
            Assert.NotNull(AnimalContests.Blocker(ContestKind.BeautyContest, ox, r));
            Assert.Null(AnimalContests.Blocker(ContestKind.LogPull, ox, r));
            Assert.NotNull(AnimalContests.Blocker(ContestKind.LogPull, cow, r));
            var res = AnimalContests.Run(new Random(1), ContestKind.BeautyContest, cow, r, 0, true);
            Assert.InRange(res.Place, 1, 6);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_SoySeviyeUnvanVeDayaniklilikGeriGelir()
        {
            var x = Make();
            var h = Horse(x, 50, (int)AnimalLineage.Noble, 10000, 100f);
            AnimalProgress.GrantLegend(h, "Panayır Rüzgârı");
            var save = new AnimalSave { id = 1, species = "animal.horse", lineage = (int)h.Lineage, xp = h.Xp, legendTitle = h.LegendaryTitle, legendDescent = true, stamina = 33f, shy = 0.5, saddle = "item.saddle", friendship = 100f, age = 40, fullness = 100f };
            var json = new JsonSerializerOptions { IncludeFields = true };
            var back = JsonSerializer.Deserialize<AnimalSave>(JsonSerializer.Serialize(save, json), json);
            var y = Make();
            var r = y.Animals.Restore(back.id, y.Db.Animals.Get(back.species), new GridPos(1, 1), 0, back.fullness, back.age, 0, 0, back.friendship, 0, back.saddle,
                lineage: back.lineage, xp: back.xp, legendaryDescent: back.legendDescent, legendTitle: back.legendTitle, stamina: back.stamina, shyDays: back.shy);
            Assert.Equal(AnimalLineage.Legendary, r.Lineage);
            Assert.Equal(5, r.Level);
            Assert.Equal("Panayır Rüzgârı", r.LegendaryTitle);
            Assert.True(r.LegendaryDescent);
            Assert.Equal(33f, r.Stamina);
            Assert.Equal(0.5, r.ShyDays, 6);
            // Eski kayıt: varsayılanlar.
            var old = y.Animals.Restore(2, y.Db.Animals.Get("animal.horse"), new GridPos(1, 2), 0, 100f, 40, 0, 0);
            Assert.Equal(AnimalLineage.Village, old.Lineage);
            Assert.Equal(HorseAbilities.MaxStamina, old.Stamina);
        }

        private sealed class FixedRandom : Random
        {
            private readonly double[] _v; private int _i;
            public FixedRandom(params double[] v) { _v = v; }
            public override double NextDouble() => _v[Math.Min(_i++, _v.Length - 1)];
        }
    }
}
