using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Upbringing
{
    public sealed class GameResult
    {
        public string Game;
        public Character Winner;
        public bool HouseholdWon;
        public int Entrants;
    }

    /// <summary>
    /// Şenlik yarışları (atletizm, onay 18.11.2026; uygulama 19.11.2026). Her şenlikte üç yarış:
    /// çocuk koşusu (çocuk ve ergen), çuval yarışı (ergen ve yetişkin), güreş (genç yetişkin ve yetişkin).
    /// Puan = Atletizm seviyesi × 3 + şans (0-10) (+ güreşte yaş gücü). Hane kazanırsa akçe, itibar ve "Şampiyon";
    /// katılan herkes Atletizm XP'si alır.
    /// </summary>
    public sealed class FestivalGames
    {
        public const int PrizeAkce = 10, PrizeReputation = 4;
        public const double EntryXp = 8, WinXp = 15;

        private readonly Random _rng;

        public FestivalGames(int seed = 31) { _rng = new Random(seed); }

        public Household House { get; set; }
        public ReputationSystem Reputation { get; set; }
        public int Wins { get; private set; }

        private static bool CanRace(Character c, AgeStage min, AgeStage max) =>
            !c.IsDead && c.Stage >= min && c.Stage <= max;

        private double Score(Character c, bool wrestling)
        {
            double s = c.Skills.GetLevel(SkillType.Athletics) * 3 + _rng.NextDouble() * 10;
            if (wrestling && c.Stage == AgeStage.Adult) s += 2;   // olgun güç
            return s;
        }

        public List<GameResult> Run(IEnumerable<Character> everyone)
        {
            var all = new List<Character>(everyone);
            var results = new List<GameResult>
            {
                Race("Çocuk koşusu", all, AgeStage.Child, AgeStage.Teen, false),
                Race("Çuval yarışı", all, AgeStage.Teen, AgeStage.Adult, false),
                Race("Güreş", all, AgeStage.YoungAdult, AgeStage.Adult, true)
            };
            results.RemoveAll(r => r == null);
            return results;
        }

        private GameResult Race(string name, List<Character> all, AgeStage min, AgeStage max, bool wrestling)
        {
            var entrants = new List<Character>();
            foreach (var c in all) if (CanRace(c, min, max)) entrants.Add(c);
            bool household = false;
            foreach (var c in entrants) if (!c.IsVillager) household = true;
            if (entrants.Count < 2 || !household) return null;   // hanede yarışan yoksa yarış duyurulmaz
            Character best = null; double bestScore = double.MinValue;
            foreach (var c in entrants)
            {
                double s = Score(c, wrestling);
                if (s > bestScore) { bestScore = s; best = c; }
                if (!c.IsVillager) c.GainSkillXp(SkillType.Athletics, EntryXp);
            }
            var r = new GameResult { Game = name, Winner = best, HouseholdWon = !best.IsVillager, Entrants = entrants.Count };
            if (r.HouseholdWon)
            {
                Wins++;
                House?.AddAkce(PrizeAkce);
                Reputation?.Add(PrizeReputation, name.ToLowerInvariant());
                best.GainSkillXp(SkillType.Athletics, WinXp);
                best.AddMoodlet("champion", $"{name} şampiyonu", 4, 2.0);
            }
            return r;
        }
    }
}
