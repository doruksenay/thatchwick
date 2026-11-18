using System;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Build;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Family;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Adım 56 (karar 17.11.2026): eski "Genç" evresi Ergen (24-30) ve Genç yetişkin (30-42) olarak ikiye bölündü.
    public class ErgenGencYetiskinTests
    {
        private static Character At(int id, double age, Sex sex = Sex.Female, bool villager = false)
        {
            var c = new Character(id, "K" + id, new GridPos(id, 0)) { Sex = sex, IsVillager = villager };
            c.SetAge(age);
            return c;
        }

        [Fact]
        public void Evreler_SiraliVeAdlari()
        {
            var order = new[] { AgeStage.Baby, AgeStage.Child, AgeStage.Teen, AgeStage.YoungAdult, AgeStage.Adult, AgeStage.Elder };
            for (int i = 1; i < order.Length; i++)
                Assert.True(order[i - 1] < order[i]);
            Assert.Equal("Ergen", AgeStages.Name(AgeStage.Teen));
            Assert.Equal("Genç yetişkin", AgeStages.Name(AgeStage.YoungAdult));
            var cfg = AgeConfig.Default;
            Assert.Equal(24.0, cfg.StartOf(AgeStage.Teen));
            Assert.Equal(30.0, cfg.StartOf(AgeStage.YoungAdult));
            Assert.Equal(42.0, cfg.StartOf(AgeStage.Adult));
        }

        [Fact]
        public void BeceriTavani_Ergen7_GencYetiskin9_Yetiskin10()
        {
            Assert.Equal(3, SkillRules.LevelCap(AgeStage.Child));
            Assert.Equal(7, SkillRules.LevelCap(AgeStage.Teen));
            Assert.Equal(9, SkillRules.LevelCap(AgeStage.YoungAdult));
            Assert.Equal(10, SkillRules.LevelCap(AgeStage.Adult));
            Assert.Equal(10, SkillRules.LevelCap(AgeStage.Elder));

            var teen = At(1, 26);
            teen.GainSkillXp(SkillType.Farming, SkillRules.XpForLevel(10));
            Assert.Equal(7, teen.Skills.GetLevel(SkillType.Farming));
            teen.Tick(6);   // Genç yetişkin oldu: tavan 9'a çıkar
            Assert.Equal(AgeStage.YoungAdult, teen.Stage);
            teen.GainSkillXp(SkillType.Farming, SkillRules.XpForLevel(10));
            Assert.Equal(9, teen.Skills.GetLevel(SkillType.Farming));
        }

        [Fact]
        public void Hamilelik_ErgendeOlmaz_GencYetiskindeOlur()
        {
            var teen = At(1, 27);
            Assert.False(teen.CanConceive);
            Assert.False(teen.TryStartPregnancy(9));
            var young = At(2, 31);
            Assert.True(young.CanConceive);
        }

        [Fact]
        public void Evlilik_ErgenEvlenemez_GencYetiskinEvlenebilir()
        {
            Assert.False(FamilyService.IsAdultAge(At(1, 29)));
            Assert.True(FamilyService.IsAdultAge(At(2, 30)));
            Assert.True(FamilyService.IsAdultAge(At(3, 50)));
            Assert.False(FamilyService.IsAdultAge(At(4, 80)));   // yaşlı: önceki kuralla aynı

            var house = new Household();
            var pop = new Population();
            var man = At(10, 35, Sex.Male); pop.Add(man);
            var teen = At(11, 27); pop.Add(teen);
            var family = new FamilyService(pop, house, ContentFilesTests.LoadDb(out _));
            Assert.Contains("genç yetişkin ya da yetişkin", family.CanDo(man, teen, SocialAction.Propose));
        }

        [Fact]
        public void TalipHavuzu_YalnizcaGencYetiskinVeYetiskin()
        {
            var head = At(1, 35, Sex.Male);
            Assert.False(SuitorPool.IsEligible(head, At(2, 27, Sex.Female, villager: true)));
            Assert.True(SuitorPool.IsEligible(head, At(3, 31, Sex.Female, villager: true)));
            Assert.True(SuitorPool.ArrivalAgeMin >= AgeConfig.Default.YoungAdultStartDay);
            Assert.True(VillagerService.SuccessorAgeMin >= AgeConfig.Default.YoungAdultStartDay);
        }

        [Fact]
        public void Ayna_YalnizcaCocuktanErgeneGecerkenAcilir()
        {
            var c = At(1, 29.99);
            c.Tick(0.05);   // Ergen -> Genç yetişkin: Ayna yok
            Assert.Equal(AgeStage.YoungAdult, c.Stage);
            Assert.False(c.FreeMirrorAvailable);
        }

        [Fact]
        public void KarakterYaratma_BaslangicGencYetiskinYaDaYetiskin()
        {
            Assert.True(AgeStages.IsStartStage(AgeStage.YoungAdult));
            Assert.True(AgeStages.IsStartStage(AgeStage.Adult));
            Assert.False(AgeStages.IsStartStage(AgeStage.Teen));
            Assert.Equal(AgeStage.YoungAdult, AgeConfig.Default.StageAt(CharacterDraft.YoungAdultStartAge));
            Assert.Equal(AgeStage.Adult, AgeConfig.Default.StageAt(CharacterDraft.AdultStartAge));
            var rng = new Random(3);
            for (int i = 0; i < 20; i++)
            {
                var d = new CharacterDraft { StartStage = i % 2 == 0 ? AgeStage.YoungAdult : AgeStage.Adult };
                Assert.Equal(d.StartStage, AgeConfig.Default.StageAt(d.StartAge));
            }
        }

        [Fact]
        public void YetiskinIcerigi_GencYetiskinVeUstu()
        {
            Assert.False(AgeStages.IsAdultContentAge(AgeStage.Teen));
            Assert.True(AgeStages.IsAdultContentAge(AgeStage.YoungAdult));
            Assert.True(AgeStages.IsAdultContentAge(AgeStage.Elder));
        }
    }
}
