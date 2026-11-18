using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // Beceri temposu (karar 18.11.2026, Project belgesi 28): yatkınlık, günlük doygunluk, yaşlılık gerilemesi, 116 dal.
    public class BeceriTemposuTests
    {
        private static Character Person(int id = 1, double age = 45)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { CanStarve = false };
            c.SetAge(age);
            return c;
        }

        /// <summary>Her NextDouble'da sırayla verilen değerleri döner (kalıtım şansını sınamak için).</summary>
        private sealed class FixedRandom : Random
        {
            private readonly double[] _values; private int _i;
            public FixedRandom(params double[] values) { _values = values; }
            public override double NextDouble() => _values[Math.Min(_i++, _values.Length - 1)];
            public override int Next(int maxValue) => 0;
        }

        // ---------- yatkınlık ----------

        [Fact]
        public void Yatkinlik_XpyiKatlar()
        {
            var plain = Person(1); var minor = Person(2); var major = Person(3);
            minor.Skills.SetAptitude(SkillType.Smithing, Aptitude.Minor);
            major.Skills.SetAptitude(SkillType.Smithing, Aptitude.Major);
            plain.GainSkillXp(SkillType.Smithing, 20);
            minor.GainSkillXp(SkillType.Smithing, 20);
            major.GainSkillXp(SkillType.Smithing, 20);
            Assert.Equal(20.0, plain.Skills.GetXp(SkillType.Smithing), 6);
            Assert.Equal(30.0, minor.Skills.GetXp(SkillType.Smithing), 6);
            Assert.Equal(40.0, major.Skills.GetXp(SkillType.Smithing), 6);
            // Başka beceride etkisi yok.
            minor.GainSkillXp(SkillType.Farming, 20);
            Assert.Equal(20.0, minor.Skills.GetXp(SkillType.Farming), 6);
        }

        [Fact]
        public void Yatkinlik_EnCokUcTaneVeYalnizcaYukselir()
        {
            var c = Person();
            Assert.True(c.Skills.SetAptitude(SkillType.Farming, Aptitude.Minor));
            Assert.True(c.Skills.SetAptitude(SkillType.Fishing, Aptitude.Minor));
            Assert.True(c.Skills.SetAptitude(SkillType.Cooking, Aptitude.Major));
            Assert.False(c.Skills.SetAptitude(SkillType.Mining, Aptitude.Minor));   // dördüncü girmez
            Assert.Equal(3, c.Skills.AptitudeCount);
            Assert.False(c.Skills.SetAptitude(SkillType.Cooking, Aptitude.Minor)); // düşürülmez
            Assert.True(c.Skills.SetAptitude(SkillType.Farming, Aptitude.Major));  // yükselir
            Assert.Equal(Aptitude.Major, c.Skills.GetAptitude(SkillType.Farming));
        }

        [Fact]
        public void Yatkinlik_IsiYaparkenKucukKeyifVerir()
        {
            var c = Person(); c.MoodEnabled = true;
            c.Skills.SetAptitude(SkillType.Weaving, Aptitude.Minor);
            c.GainSkillXp(SkillType.Farming, 5);
            Assert.False(c.Mood.Has(Character.AptitudeMoodId));
            c.GainSkillXp(SkillType.Weaving, 5);
            Assert.True(c.Mood.Has(Character.AptitudeMoodId));
        }

        [Fact]
        public void Yatkinlik_EbeveyndenSansla_Gecer()
        {
            var mother = Person(1); var father = Person(2);
            mother.Skills.SetAptitude(SkillType.Weaving, Aptitude.Major);
            father.Skills.SetAptitude(SkillType.Smithing, Aptitude.Minor);
            // Anne: geçer (0,1 < 0,35) ve çok yatkın kalır (0,2 < 0,5); baba: geçmez (0,9).
            var child = Person(3, 0);
            LifeRules.InheritAptitudes(new FixedRandom(0.1, 0.2, 0.9), child, mother, father);
            Assert.Equal(Aptitude.Major, child.Skills.GetAptitude(SkillType.Weaving));
            Assert.Equal(Aptitude.None, child.Skills.GetAptitude(SkillType.Smithing));
            // Çok yatkın, ikinci zarda yatkına iner.
            var child2 = Person(4, 0);
            LifeRules.InheritAptitudes(new FixedRandom(0.1, 0.8, 0.1), child2, mother, father);
            Assert.Equal(Aptitude.Minor, child2.Skills.GetAptitude(SkillType.Weaving));
            Assert.Equal(Aptitude.Minor, child2.Skills.GetAptitude(SkillType.Smithing));
        }

        [Fact]
        public void Yatkinlik_CocuklukIsindenDogar()
        {
            var c = Person(1, 10);
            c.Skills.RestoreXp(SkillType.Fishing, SkillRules.XpForLevel(3));
            c.Skills.RestoreXp(SkillType.Farming, 60);
            var note = LifeRules.ApplyChildhoodAptitude(c);
            Assert.NotNull(note);
            Assert.Equal(Aptitude.Minor, c.Skills.GetAptitude(SkillType.Fishing));
            Assert.Contains(note, c.Chronicle);
            // Zaten yatkınsa çok yatkına çıkar; çok yatkınsa değişmez.
            Assert.NotNull(LifeRules.ApplyChildhoodAptitude(c));
            Assert.Equal(Aptitude.Major, c.Skills.GetAptitude(SkillType.Fishing));
            Assert.Null(LifeRules.ApplyChildhoodAptitude(c));
        }

        [Fact]
        public void Yatkinlik_CocuklukIsiAzsaDogmaz()
        {
            var c = Person(1, 10);
            c.Skills.RestoreXp(SkillType.Fishing, SkillRules.XpForLevel(2));
            Assert.Null(LifeRules.ApplyChildhoodAptitude(c));
            Assert.Equal(0, c.Skills.AptitudeCount);
        }

        // ---------- günlük doygunluk ----------

        [Fact]
        public void Doygunluk_VarsayilanKapali()
        {
            var c = Person();
            c.GainSkillXp(SkillType.Farming, 500);
            Assert.Equal(500.0, c.Skills.GetXp(SkillType.Farming), 6);
        }

        [Fact]
        public void Doygunluk_EsiktenSonraYuzde25()
        {
            var c = Person(); c.Skills.PacingEnabled = true;
            double threshold = SkillPacing.DailyThreshold(1, Aptitude.None);   // 1. seviye aralığı 50 → 20
            Assert.Equal(20.0, threshold, 6);
            c.GainSkillXp(SkillType.Farming, 10);
            Assert.False(c.Skills.IsSaturated(SkillType.Farming));
            c.GainSkillXp(SkillType.Farming, 30);   // 10 tam + 20 x 0,25
            Assert.Equal(25.0, c.Skills.GetXp(SkillType.Farming), 6);
            Assert.True(c.Skills.IsSaturated(SkillType.Farming));
            // Başka beceri etkilenmez.
            c.GainSkillXp(SkillType.Fishing, 15);
            Assert.Equal(15.0, c.Skills.GetXp(SkillType.Fishing), 6);
            // Gece sıfırlanır.
            c.Skills.NewDay();
            c.GainSkillXp(SkillType.Farming, 10);
            Assert.Equal(35.0, c.Skills.GetXp(SkillType.Farming), 6);
        }

        [Fact]
        public void Doygunluk_YatkinlikEsigiYukseltir()
        {
            Assert.Equal(SkillPacing.DailyThreshold(4, Aptitude.None) * 1.5, SkillPacing.DailyThreshold(4, Aptitude.Minor), 6);
            Assert.Equal(SkillPacing.DailyThreshold(4, Aptitude.Minor), SkillPacing.DailyThreshold(4, Aptitude.Major), 6);
            // Bir günde tam hızla en çok ~yarım seviye.
            for (int l = 1; l < SkillRules.MaxLevel; l++)
                Assert.True(SkillPacing.DailyThreshold(l, Aptitude.None) < SkillRules.XpForLevel(l + 1) - SkillRules.XpForLevel(l));
        }

        [Fact]
        public void Doygunluk_HaneUyesindeAcikKoyludeKapali()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            var pop = new Population();
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), new Household(), pop, new GameClock());
            var member = Person(pop.NextId());
            var villager = Person(pop.NextId()); villager.IsVillager = true;
            pop.Add(member); pop.Add(villager);
            Assert.True(member.Skills.PacingEnabled);
            Assert.False(villager.Skills.PacingEnabled);
            life.SkillPacing = false;
            Assert.False(member.Skills.PacingEnabled);
        }

        // ---------- yaşlılık gerilemesi ----------

        [Fact]
        public void Gerileme_YaslilikGirisindeBirKezYuzde10_SeviyeDusmez()
        {
            var c = Person(); c.Skills.PacingEnabled = true;
            c.Skills.RestoreXp(SkillType.Carpentry, SkillRules.XpForLevel(5) + 100);
            Assert.Equal(10.0, c.Skills.ApplyElderOnset(), 6);
            Assert.Equal(SkillRules.XpForLevel(5) + 90, c.Skills.GetXp(SkillType.Carpentry), 6);
            Assert.Equal(5, c.Skills.GetLevel(SkillType.Carpentry));
            Assert.Equal(0.0, c.Skills.ApplyElderOnset(), 6);   // bir kereye mahsus
        }

        [Fact]
        public void Gerileme_YaslidaKullanilmayanBeceriMevsimSonundaYuzde25()
        {
            var c = Person(); c.Skills.PacingEnabled = true;
            c.Skills.RestoreXp(SkillType.Carpentry, SkillRules.XpForLevel(5) + 100);
            c.Skills.RestoreXp(SkillType.Farming, SkillRules.XpForLevel(5) + 100);
            c.GainSkillXp(SkillType.Farming, 1);   // mevsimde bir kez çalıştı
            var decayed = c.Skills.OnSeasonEnd(elder: true);
            Assert.Equal(new[] { SkillType.Carpentry }, decayed.ToArray());
            Assert.Equal(SkillRules.XpForLevel(5) + 75, c.Skills.GetXp(SkillType.Carpentry), 6);
            Assert.Equal(SkillRules.XpForLevel(5) + 101, c.Skills.GetXp(SkillType.Farming), 6);
            // Kullanım kaydı sıfırlandı: ertesi mevsim çalışmazsa çiftçilik de geriler, ama seviye hiç düşmez.
            for (int i = 0; i < 30; i++) c.Skills.OnSeasonEnd(elder: true);
            Assert.Equal(5, c.Skills.GetLevel(SkillType.Farming));
            Assert.Equal(5, c.Skills.GetLevel(SkillType.Carpentry));
        }

        [Fact]
        public void Gerileme_YetiskindeVeTempoKapaliykenYok()
        {
            var adult = Person(); adult.Skills.PacingEnabled = true;
            adult.Skills.RestoreXp(SkillType.Carpentry, SkillRules.XpForLevel(5) + 100);
            Assert.Empty(adult.Skills.OnSeasonEnd(elder: false));
            var off = Person(2);
            off.Skills.RestoreXp(SkillType.Carpentry, SkillRules.XpForLevel(5) + 100);
            Assert.Empty(off.Skills.OnSeasonEnd(elder: true));
            Assert.Equal(0.0, off.Skills.ApplyElderOnset(), 6);
            Assert.Equal(SkillRules.XpForLevel(5) + 100, off.Skills.GetXp(SkillType.Carpentry), 6);
        }

        [Fact]
        public void Gerileme_YaslanincaLifeSystemsUygular()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            var pop = new Population();
            var life = new LifeSystems(db, new WorldState(new GridMap(8, 8)), new Household(), pop, new GameClock());
            var c = Person(pop.NextId(), AgeConfig.Default.ElderStartDay - 0.01);
            pop.Add(c);
            c.Skills.RestoreXp(SkillType.Trade, SkillRules.XpForLevel(3) + 50);
            c.Tick(0.05);
            Assert.Equal(AgeStage.Elder, c.Stage);
            Assert.True(c.Skills.ElderOnsetApplied);
            Assert.Equal(SkillRules.XpForLevel(3) + 45, c.Skills.GetXp(SkillType.Trade), 6);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_YatkinlikSayacVeKullanimGeriGelir()
        {
            var s = SaveTests.BuildState();
            var c = s.Chars[0];
            c.Skills.PacingEnabled = true;
            c.Skills.SetAptitude(SkillType.Weaving, Aptitude.Major);
            c.Skills.SetAptitude(SkillType.Fishing, Aptitude.Minor);
            c.GainSkillXp(SkillType.Fishing, 7);
            c.Skills.ElderOnsetApplied = true;
            var data = SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market);
            var json = new JsonSerializerOptions { IncludeFields = true };
            var loaded = SaveGame.Load(JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(data, json), json), s.Db);
            var l = loaded.Characters.First(x => x.Id == c.Id);
            Assert.Equal(Aptitude.Major, l.Skills.GetAptitude(SkillType.Weaving));
            Assert.Equal(Aptitude.Minor, l.Skills.GetAptitude(SkillType.Fishing));
            Assert.Equal(c.Skills.TodayXp(SkillType.Fishing), l.Skills.TodayXp(SkillType.Fishing), 6);
            Assert.True(l.Skills.UsedThisSeason(SkillType.Fishing));
            Assert.True(l.Skills.ElderOnsetApplied);
        }

        // ---------- dal düzeni ----------

        [Fact]
        public void Dallar_116DuzenineUyar()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.Empty(errors);
            int total = 0, main = 0, none = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                int want = SkillNames.BranchCount(s);
                total += want;
                if (want == 6) main++;
                if (want == 0) none++;
                var list = db.Branches.All.Where(b => b.Skill == s).ToList();
                Assert.Equal(want, list.Count);
                if (want == 2) Assert.All(list, b => Assert.Equal(5, b.Level));
            }
            Assert.Equal(118, total);   // 116 + Atletizm (5. seviye 2 dal)
            Assert.Equal(14, main);
            Assert.Equal(4, none);
            Assert.Equal(118, db.Branches.Count);
        }
    }
}
