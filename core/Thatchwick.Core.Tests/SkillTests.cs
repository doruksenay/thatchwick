using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    public class SkillTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(49.9, 1)]
        [InlineData(50, 2)]
        [InlineData(200, 3)]
        [InlineData(4050, 10)]
        [InlineData(99999, 10)]
        public void XpdenSeviye_Dogru(double xp, int level)
        {
            Assert.Equal(level, SkillRules.LevelForXp(xp));
        }

        [Fact]
        public void YeniKarakter_TumBecerilerSeviye1()
        {
            var s = new SkillSet();
            foreach (SkillType t in System.Enum.GetValues(typeof(SkillType)))
                Assert.Equal(1, s.GetLevel(t));
        }

        [Fact]
        public void AddXp_SeviyeAtlarsa_OlayTetiklenir()
        {
            var s = new SkillSet();
            var events = new List<(SkillType, int, int)>();
            s.LevelChanged += (t, a, b) => events.Add((t, a, b));
            s.AddXp(SkillType.Cooking, 60, 10);
            Assert.Single(events);
            Assert.Equal((SkillType.Cooking, 1, 2), events[0]);
        }

        [Fact]
        public void Tavan_XpUstSeviyeyeGecirmez()
        {
            var s = new SkillSet();
            s.AddXp(SkillType.Farming, 100000, 3);
            Assert.Equal(3, s.GetLevel(SkillType.Farming));
            Assert.Equal(0.0, s.AddXp(SkillType.Farming, 50, 3));
        }

        [Fact]
        public void Tavan_Yukselince_YenidenOgrenebilir()
        {
            var s = new SkillSet();
            s.AddXp(SkillType.Farming, 100000, 3);
            s.AddXp(SkillType.Farming, 1000, 7);
            Assert.True(s.GetLevel(SkillType.Farming) > 3);
            Assert.True(s.GetLevel(SkillType.Farming) <= 7);
        }

        [Fact]
        public void Bebek_Ogrenemez()
        {
            var c = new Character(1, "Bebe", new GridPos(0, 0));
            Assert.Equal(0.0, c.GainSkillXp(SkillType.Cooking, 100));
            Assert.Equal(1, c.Skills.GetLevel(SkillType.Cooking));
        }

        [Fact]
        public void Cocuk_Tavani3()
        {
            var c = new Character(1, "Cocuk", new GridPos(0, 0));
            c.SetAge(13);
            c.GainSkillXp(SkillType.Woodcutting, 100000);
            Assert.Equal(3, c.Skills.GetLevel(SkillType.Woodcutting));
        }

        [Fact]
        public void Yetiskin_Seviye10aUlasir()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.SetAge(40);
            c.GainSkillXp(SkillType.Carpentry, 100000);
            Assert.Equal(10, c.Skills.GetLevel(SkillType.Carpentry));
        }

        [Fact]
        public void OluKarakter_Kazanmaz()
        {
            var c = new Character(1, "Aldric", new GridPos(0, 0));
            c.SetAge(40);
            c.LifespanDays = 41;
            c.Tick(5);
            Assert.True(c.IsDead);
            Assert.Equal(0.0, c.GainSkillXp(SkillType.Cooking, 10));
        }

        [Fact]
        public void EsyaKullanimi_Bitince_BeceriXpVerir()
        {
            var item = new ItemDef
            {
                Id = "pot",
                DisplayName = "pot",
                Interactions = new List<InteractionDef>
                {
                    new InteractionDef { Id = "cook", DisplayName = "Pişir", Need = NeedType.Hunger, Restore = 60, DurationDays = 0.2,
                        Skill = SkillType.Cooking, SkillXp = 25 }
                }
            };
            var world = new WorldState(new GridMap(10, 10));
            var placed = world.PlaceItem(item, new GridPos(5, 5));
            var c = new Character(1, "Aldric", new GridPos(0, 5), isPlayerControlled: true);
            c.SetAge(40);
            var runner = new JobRunner(c, world);
            runner.Command(Job.UseItem(placed.Id, "cook"));
            runner.Tick(2.0);
            Assert.True(runner.IsIdle);
            Assert.Equal(25.0, c.Skills.GetXp(SkillType.Cooking), 6);
        }

        [Fact]
        public void Mapper_BeceriOkunur_GecersizBeceriHatadir()
        {
            var ok = new ItemDto { id = "pot", displayName = "Pot", interactions = new[] {
                new InteractionDto { id = "cook", displayName = "Pişir", need = "Hunger", restore = 60, durationDays = 0.2, skill = "cooking", skillXp = 25 } } };
            var db = new ContentDatabase();
            Assert.Empty(ContentMapper.Apply(new ContentFileDto { items = new[] { ok } }, db));
            Assert.Equal(SkillType.Cooking, db.Items.Get("pot").Interactions.Single().Skill);

            var bad = new ItemDto { id = "pot2", displayName = "Pot", interactions = new[] {
                new InteractionDto { id = "cook", displayName = "Pişir", need = "Hunger", restore = 60, durationDays = 0.2, skill = "magic", skillXp = 25 } } };
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { bad } }, new ContentDatabase());
            Assert.Contains(errors, e => e.Contains("magic"));
        }

        [Fact]
        public void Dogrulama_SkillXpOlmadanBeceriReddedilir()
        {
            var dto = new ItemDto { id = "pot", displayName = "Pot", interactions = new[] {
                new InteractionDto { id = "cook", displayName = "Pişir", need = "Hunger", restore = 60, durationDays = 0.2, skill = "Cooking", skillXp = 0 } } };
            var errors = ContentMapper.Apply(new ContentFileDto { items = new[] { dto } }, new ContentDatabase());
            Assert.NotEmpty(errors);
        }
    }
}
