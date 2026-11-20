using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Belge 47 §3: süreli görev (Ali'yi 4 saat madene yolla).
    public class SureliGorevTests
    {
        private static ContentDatabase Real()
        {
            var db = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(ContentFilesTests.LoadAll(), db));
            return db;
        }

        private sealed class Kit
        {
            public ContentDatabase Db; public Population Pop; public Household House; public TimedDutyService Duties; public double Now = 10.3;
        }

        private static Kit Make(int seed = 5)
        {
            var k = new Kit { Db = Real(), Pop = new Population(), House = new Household() };
            k.Duties = new TimedDutyService(k.Pop, k.House, k.Db, seed) { Now = () => k.Now };
            return k;
        }

        private static Character Person(Kit k, int id, double age, SkillType skill = SkillType.Mining, int level = 1)
        {
            var c = new Character(id, "Ali" + id, new GridPos(3, 3));
            c.SetAge(age);
            c.MoodEnabled = true;
            if (level > 1) c.Skills.RestoreXp(skill, SkillRules.XpForLevel(level));
            k.Pop.Add(c);
            return c;
        }

        private static void Run(Kit k, double hours, double step = 0.25)
        {
            for (double h = 0; h < hours; h += step) { k.Now += step / 24.0; k.Pop.Tick(step / 24.0); k.Duties.Tick(step / 24.0); }
        }

        [Fact]
        public void Oran_TabanBeceriAlet_TavanYuzdeKirkBes()
        {
            var k = Make();
            var ali = Person(k, 1, 40, SkillType.Mining, 4);
            k.Duties.ToolTier = id => 1;
            var place = TimedDutyService.Place("mine:gallery");
            Assert.Equal(0.38, k.Duties.Efficiency(ali, place), 6);
            var usta = Person(k, 2, 40, SkillType.Mining, 10);
            k.Duties.ToolTier = id => 3;
            k.Duties.HasLeader = () => true;
            Assert.Equal(0.45, k.Duties.Efficiency(usta, place), 6);
        }

        [Fact]
        public void DortSaatMaden_OntaCevherdeUcDortGetirir()
        {
            var k = Make();
            var ali = Person(k, 1, 40, SkillType.Mining, 4);
            k.Duties.ToolTier = id => 1;
            k.House.AddItem("item.bread", 4);
            var d = k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Mine, PlaceId = "mine:gallery", Hours = 4 }, out var why);
            Assert.Null(why);
            Assert.True(ali.Away);
            Run(k, 4.5);
            Assert.False(ali.Away);
            var r = k.Duties.LastResult;
            Assert.NotNull(r);
            int total = r.Items.Sum(i => i.Value);
            Assert.InRange(total, 3, 4);
            Assert.True(r.Xp > 0);
        }

        [Fact]
        public void Azik_EvdenAlinir_AcikincaYenir_ArtanGeriDoner()
        {
            var k = Make();
            var ali = Person(k, 1, 40);
            k.House.AddItem("item.bread", 6);
            k.House.AddItem("item.meat", 3);   // çiğ et alınmaz
            ali.Needs.Set(NeedType.Hunger, 40f);
            var d = k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 8 }, out _);
            Assert.True(d.Packed.ContainsKey("item.bread"));
            Assert.False(d.Packed.ContainsKey("item.meat"));
            int packed = d.Packed["item.bread"];
            Assert.Equal(6 - packed, k.House.Count("item.bread"));
            Run(k, 8.5);
            Assert.True(ali.Needs.Get(NeedType.Hunger) > 15f);
            Assert.Equal(TimedDutyEnd.Done, k.Duties.LastResult.End);
            Assert.True(k.House.Count("item.bread") >= 6 - packed);   // artan ekmek eve döndü
            Assert.Equal(3, k.House.Count("item.meat"));
        }

        [Fact]
        public void YemegiBiten_AcKalinca_ErkenDoner()
        {
            var k = Make();
            var ali = Person(k, 1, 40);
            ali.Needs.Set(NeedType.Hunger, 22f);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Fish, PlaceId = "fish:pond", Hours = 8 }, out var why);
            Assert.Null(why);
            Run(k, 8.5);
            Assert.Equal(TimedDutyEnd.Hungry, k.Duties.LastResult.End);
            Assert.Contains("Aç kaldı", k.Duties.LastResult.Summary);
        }

        [Fact]
        public void YasKurallari_CocukYalnizcaYakinaToplamaya()
        {
            var k = Make();
            var child = Person(k, 1, 15);
            var baby = Person(k, 2, 3);
            Assert.NotNull(k.Duties.CanStart(child, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Mine, PlaceId = "mine:gallery", Hours = 2 }));
            Assert.NotNull(k.Duties.CanStart(child, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:forest", Hours = 2 }));
            Assert.NotNull(k.Duties.CanStart(child, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 3 }));
            Assert.Null(k.Duties.CanStart(child, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 2 }));
            Assert.NotNull(k.Duties.CanStart(baby, new TimedDutyOrder { CharacterId = 2, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 1 }));
            var elder = Person(k, 3, 90);
            Assert.NotNull(k.Duties.CanStart(elder, new TimedDutyOrder { CharacterId = 3, Kind = TimedDutyKind.Mine, PlaceId = "mine:gallery", Hours = 2 }));
            Assert.Null(k.Duties.CanStart(elder, new TimedDutyOrder { CharacterId = 3, Kind = TimedDutyKind.Fish, PlaceId = "fish:pond", Hours = 4 }));
        }

        [Fact]
        public void CocukSikilinca_OyuncaklaOynar_IsDurur()
        {
            var k = Make(seed: 9);
            var child = Person(k, 1, 15);
            child.Needs.Set(NeedType.Fun, 30f);
            k.House.AddItem("item.spinning_top", 1);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 2 }, out _);
            Run(k, 2.5);
            var r = k.Duties.LastResult;
            Assert.Contains(r.Notes, n => n.Contains("sıkıldı"));
        }

        [Fact]
        public void Hamile_MadeneGonderilmez()
        {
            var k = Make();
            var mother = Person(k, 1, 35);
            mother.Sex = Sex.Female;
            Assert.True(mother.TryStartPregnancy(2));
            Assert.NotNull(k.Duties.CanStart(mother, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Mine, PlaceId = "mine:gallery", Hours = 2 }));
        }

        [Fact]
        public void GeriCagir_OAnaKadarkiyleDoner()
        {
            var k = Make();
            var ali = Person(k, 1, 40, SkillType.Woodcutting, 3);
            k.House.AddItem("item.bread", 3);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forest, PlaceId = "forest:forest", Hours = 8 }, out _);
            Run(k, 3);
            var r = k.Duties.Recall(1);
            Assert.Equal(TimedDutyEnd.Recalled, r.End);
            Assert.False(ali.Away);
            Assert.True(r.Hours < 4);
        }

        [Fact]
        public void Hedef_DolunncaErkenDoner()
        {
            var k = Make();
            Person(k, 1, 40, SkillType.Mining, 5);
            k.House.AddItem("item.bread", 4);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Clay, PlaceId = "clay:river", Hours = 8, TargetItem = "item.clay", TargetCount = 2 }, out _);
            Run(k, 8.5);
            Assert.Equal(TimedDutyEnd.Target, k.Duties.LastResult.End);
            Assert.True(k.House.Count("item.clay") >= 2);
        }

        [Fact]
        public void HerGunTekrarla_ErtesiGunAyniSaatteCikar()
        {
            var k = Make();
            Person(k, 1, 40);
            k.House.AddItem("item.bread", 20);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 2, Repeat = true }, out _);
            Run(k, 3);
            Assert.False(k.Duties.IsAway(1));
            Run(k, 22);   // ertesi gün aynı saat
            Assert.True(k.Duties.IsAway(1));
            Assert.Single(k.Duties.Repeats);
        }

        [Fact]
        public void Kayit_GorevVePlanGeriGelir()
        {
            var k = Make();
            Person(k, 1, 40);
            k.House.AddItem("item.bread", 5);
            k.Duties.Start(new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Forage, PlaceId = "forage:meadow", Hours = 4, Repeat = true }, out _);
            Run(k, 1);
            var save = k.Duties.Export();

            var k2 = Make();
            var c2 = Person(k2, 1, 40);
            k2.Duties.Import(save);
            Assert.True(c2.Away);
            Assert.Single(k2.Duties.Active);
            Assert.Single(k2.Duties.Repeats);
            Assert.True(k2.Duties.Active[0].WorkedHours > 0.5);
        }

        [Fact]
        public void KisinBalik_BuzDeligiIster()
        {
            var k = Make();
            var ali = Person(k, 1, 40, SkillType.Fishing, 3);
            k.Duties.SeasonNow = () => Season.Winter;
            Assert.Contains("Balıkçılık 7", k.Duties.CanStart(ali, new TimedDutyOrder { CharacterId = 1, Kind = TimedDutyKind.Fish, PlaceId = "fish:pond", Hours = 2 }));
        }

        [Fact]
        public void TamSayiyaCevirme_EnBuyukKalan()
        {
            var g = new Dictionary<string, double> { { "a", 1.6 }, { "b", 1.7 } };
            var w = TimedDutyService.Whole(g);
            Assert.Equal(3, w.Sum(x => x.Value));
            Assert.Equal(2, w.First(x => x.Key == "b").Value);
        }
    }
}
