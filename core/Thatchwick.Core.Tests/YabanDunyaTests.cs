using System;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Öküz türü, yaban ziyaretçileri, tuzak, atlama engelleri, ağıl/ateş algısı, kurucu yatkınlığı.
    public class YabanDunyaTests
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

        private static Character Adult(int id, SkillType? skill = null, int level = 1)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(40);
            if (skill.HasValue && level > 1) c.Skills.RestoreXp(skill.Value, SkillRules.XpForLevel(level));
            return c;
        }

        [Fact]
        public void Okuz_AyriTur_HepErkek_Uremez_KutukCeker()
        {
            var x = Make();
            var ox = x.Db.Animals.Get("animal.ox");
            Assert.True(ox.Draft);
            Assert.True(ox.MaleOnly);
            Assert.False(ox.Breeds);
            var a = x.Animals.Restore(5, ox, new GridPos(2, 2), 0, 100f, 30, 0, 0, 50f, female: true);
            Assert.False(a.IsFemale);
            Assert.Equal("öküz", a.SexText);
            Assert.Null(AnimalContests.Blocker(ContestKind.LogPull, a, Adult(1)));
            Assert.Equal("boğa", x.Db.Animals.Get("animal.cow").MaleName);
        }

        [Fact]
        public void Ziyaretci_GelirDeftereYazilirCikar()
        {
            var map = new GridMap(12, 12);
            var j = new NatureJournal();
            var r = new WildRoamers(map, 3) { Journal = j };
            string first = null;
            r.FirstSeen += s => first = s.Id;
            var v = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
            Assert.NotNull(v);
            Assert.Equal("wild.rabbit", first);
            Assert.True((j.Get("wild.rabbit") & JournalMark.Seen) != 0);
            Assert.Same(v, r.At(v.Position));
            for (int i = 0; i < 400 && r.Visitors.Count > 0; i++) r.Tick(0.01, 1 + i * 0.01, Season.Summer, 0.5);
            Assert.Empty(r.Visitors);
            Assert.DoesNotContain(WildRoamers.Candidates(Season.Winter, false), s => s.Id == "wild.wolf" || s.Id == "wild.bear");
            Assert.Contains(WildRoamers.Candidates(Season.Summer, true), s => s.Id == "wild.hedgehog");
        }

        [Fact]
        public void Ziyaretci_SevilirVeAvlanir()
        {
            var x = Make();
            var r = new WildRoamers(x.World.Map, 4) { Journal = new NatureJournal() };
            var c = Adult(1, SkillType.AnimalHusbandry, 10);
            bool petted = false;
            for (int i = 0; i < 30 && !petted; i++)
            {
                var v = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
                petted = r.TryPet(v.Id, c).Contains("kaçmadı");
            }
            Assert.True(petted);
            Assert.True((r.Journal.Get("wild.rabbit") & JournalMark.Petted) != 0);
            var fox = r.Arrive(WildCatalog.Get("wild.fox"), 0);
            Assert.Contains("sevilmez", r.TryPet(fox.Id, c));

            var hunter = Adult(2, SkillType.Hunting, 10);
            bool got = false;
            for (int i = 0; i < 30 && !got; i++)
            {
                var rab = r.Arrive(WildCatalog.Get("wild.rabbit"), 0);
                got = r.Shoot(rab.Id, hunter, x.House, true).Success;
            }
            Assert.True(got);
            Assert.True(x.House.Count("item.game_meat") >= 1);
            Assert.True((r.Journal.Get("wild.rabbit") & JournalMark.Hunted) != 0);
        }

        [Fact]
        public void Tuzak_GunlukYakalar()
        {
            var x = Make();
            var r = new WildRoamers(x.World.Map, 9) { Journal = new NatureJournal() };
            var hunter = Adult(1, SkillType.Hunting, 8);
            int caught = 0;
            for (int d = 0; d < 20; d++) caught += r.CheckSnares(Season.Autumn, 5, hunter, x.House).Count;
            Assert.True(caught > 0);
            Assert.True(caught <= 20 * WildRoamers.MaxSnares);
            Assert.True(x.House.Count("item.game_meat") > 0);
        }

        [Fact]
        public void Atlama_CitVeAlcakEsya()
        {
            var x = Make();
            var from = new GridPos(5, 5);
            x.World.Map.SetEdge(GridMap.EdgeBetween(from, new GridPos(6, 5)), EdgeKind.Fence);
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(5, 6));
            var spots = JumpObstacles.Find(x.World, from);
            Assert.Contains(spots, s => s.Landing == new GridPos(6, 5) && s.Tiles == 2 && s.Label == "çit");
            Assert.Contains(spots, s => s.Landing == new GridPos(5, 7) && s.Tiles == 2);
            // İki engel üst üste: 3 karo.
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(4, 5));
            x.World.PlaceItem(x.Db.Items.Get("item.wattle_hurdle"), new GridPos(3, 5));
            Assert.Contains(JumpObstacles.Find(x.World, from), s => s.Landing == new GridPos(2, 5) && s.Tiles == 3);
        }

        [Fact]
        public void Agil_VeAtes_KurtSavunmasinaGirer()
        {
            var x = Make();
            Assert.False(WildWorld.HasPen(x.World.Map));
            for (int i = 0; i < WildWorld.PenFenceEdges; i++)
                x.World.Map.SetEdge(GridMap.EdgeBetween(new GridPos(i, 8), new GridPos(i, 9)), EdgeKind.Fence);
            Assert.True(WildWorld.HasPen(x.World.Map));
            Assert.False(WildWorld.HasOutdoorFire(x.World));
            x.World.PlaceItem(x.Db.Items.Get("item.bonfire"), new GridPos(12, 12));
            Assert.True(WildWorld.HasOutdoorFire(x.World));
            var wolves = new WolfSystem(x.Animals, 1) { Fenced = () => WildWorld.HasPen(x.World.Map), HasFire = () => WildWorld.HasOutdoorFire(x.World), Shepherds = () => 1 };
            Assert.Equal(2, wolves.DefenseScore());   // çoban 1 + ateş 1
        }

        [Fact]
        public void Kurucu_YatkinlikSecer()
        {
            var d = new CharacterDraft();
            Assert.True(d.ToggleAptitude(SkillType.Carpentry));
            Assert.True(d.ToggleAptitude(SkillType.Riding));
            Assert.False(d.ToggleAptitude(SkillType.Cooking));
            Assert.True(d.ToggleAptitude(SkillType.Riding));   // kaldırır
            Assert.Single(d.Aptitudes);
        }
    }
}
