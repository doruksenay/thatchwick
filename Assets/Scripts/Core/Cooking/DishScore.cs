using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Cooking
{
    /// <summary>Sofra puanının dökümü (belge 58 §6): altı bileşen ve ek baharat; tarif kartında görünür.</summary>
    public sealed class DishScoreBreakdown
    {
        public int Ingredients, Skill, Mastery, Equipment, Kitchen, Cook, Spice;
        public bool Burnt;
        public int Total => Ingredients + Skill + Mastery + Equipment + Kitchen + Cook + Spice;
        /// <summary>53'teki kalite basamağı (Sıradan / Seçme / Has); yanık yemek bir basamak düşer.</summary>
        public int Quality => Math.Max(ItemQuality.Common, DishScore.TierOf(Total) - (Burnt ? 1 : 0));
    }

    /// <summary>
    /// Pişen yemeğin kalitesi (belge 58 §6, karar 62-S5): mutfak kademesi tavan değildir; kalite altı ayrı durumun
    /// ortak puanından çıkar. Yalnızca pişen yemek, ekmek ve içecekte; işleme zincirleri 53'teki kuralda kalır.
    /// </summary>
    public static class DishScore
    {
        public const int MaxIngredients = 35, MaxSkill = 25, MaxMastery = 10, MaxEquipment = 10, MaxKitchen = 10, MaxCook = 10, MaxSpice = 5;
        /// <summary>Basamak eşikleri: bu puan ve üstü Seçme / Has.</summary>
        public const int FineAt = 50, SuperiorAt = 75;
        /// <summary>"Eli alıştı" eşikleri (belge 60b §5.3).</summary>
        public static readonly int[] MasteryAt = { 5, 15, 40 };

        public static int TierOf(int total) => total >= SuperiorAt ? ItemQuality.Superior : total >= FineAt ? ItemQuality.Fine : ItemQuality.Common;

        /// <summary>Malzeme: kaliteli girdilerin ortalaması (Sıradan 15, Has 35) ve grup üyelerinin payı (en çok ±3).</summary>
        public static int IngredientPoints(IReadOnlyList<ItemKey> taken, ContentDatabase content)
        {
            if (taken == null || taken.Count == 0) return 20;
            double sum = 0; int graded = 0, member = 0;
            foreach (var k in taken)
            {
                if (content.Items.TryGet(k.Id, out var d) && d.HasQuality) { sum += Math.Min(ItemQuality.NormalMax, k.Quality); graded++; }
                foreach (var g in content.Groups.All) if (g.Contains(k.Id)) { member += g.BonusOf(k.Id); break; }
            }
            double avg = graded > 0 ? sum / graded : 0.5;
            int pts = (int)Math.Round(15 + avg / ItemQuality.NormalMax * 20) + Math.Max(-3, Math.Min(3, member));
            return Math.Max(0, Math.Min(MaxIngredients, pts));
        }

        /// <summary>Beceri: tarifin kapısında 10; kapının üstündeki her seviye +3.</summary>
        public static int SkillPoints(int level, int minLevel) =>
            Math.Max(0, Math.Min(MaxSkill, 10 + 3 * (level - Math.Max(1, minLevel))));

        /// <summary>Tarif ustalığı: bu tarifi kaç kez pişirdi (5, 15, 40), aile tarifi, huy ve dal.</summary>
        public static int MasteryPoints(int timesCooked, bool familyRecipe, int traitBonus)
        {
            int pts = timesCooked >= MasteryAt[2] ? 8 : timesCooked >= MasteryAt[1] ? 6 : timesCooked >= MasteryAt[0] ? 4 : 2;
            if (familyRecipe) pts += 1;
            return Math.Max(0, Math.Min(MaxMastery, pts + traitBonus));
        }

        /// <summary>Aşçının hâli: ruh hali, yorgunluk, temizlik.</summary>
        public static int CookPoints(Characters.Character c)
        {
            int pts = 6;
            if (c.MoodEnabled)
                switch (c.Mood.Status)
                {
                    case Mood.MoodStatus.Happy: pts = 10; break;
                    case Mood.MoodStatus.Good: pts = 8; break;
                    case Mood.MoodStatus.Unhappy: pts = 3; break;
                    case Mood.MoodStatus.Miserable: pts = 1; break;
                }
            if (c.Needs.Get(Characters.NeedType.Energy) < 25f) pts -= 2;
            if (c.ComfortNeedsActive && c.Needs.Get(Characters.NeedType.Hygiene) < 25f) pts -= 2;
            return Math.Max(0, Math.Min(MaxCook, pts));
        }

        /// <summary>Ekipman: istasyonun kalitesi ve durumu (hasarlı, kararmış).</summary>
        public static int EquipmentPoints(World.ItemInstance station)
        {
            if (station == null) return 5;
            int pts = 6 + Math.Min(2, station.Quality) * 2;
            if (station.Condition == ItemCondition.Damaged) pts -= 4;
            else if (station.Condition == ItemCondition.Repaired) pts -= 1;
            if (station.IsTarnished) pts -= 1;
            return Math.Max(0, Math.Min(MaxEquipment, pts));
        }

        /// <summary>Ek baharat: uyumlu katkı işlenmişse +3, tazeyse +2; uyumsuz −3. Toplam ±5.</summary>
        public static int SpicePoints(IEnumerable<SpiceDef> spices, DishClass cls)
        {
            int pts = 0;
            if (spices != null)
                foreach (var s in spices)
                    pts += s.FitsClass(cls) ? (s.Processed ? 3 : 2) : -3;
            return Math.Max(-MaxSpice, Math.Min(MaxSpice, pts));
        }

        /// <summary>Beceriye göre ek baharat yuvası: Yemek 3'te 1, Yemek 6'da 2.</summary>
        public static int SpiceSlots(int cookingLevel) => cookingLevel >= 6 ? 2 : cookingLevel >= 3 ? 1 : 0;

        /// <summary>Dibi tutma şansı (62-S3): yalnızca düşük beceride, tarifin kapısına yakınken.</summary>
        public static double BurnChance(int level, int minLevel)
        {
            if (level > 3) return 0.0;
            int above = level - Math.Max(1, minLevel);
            return above <= 0 ? 0.12 : above == 1 ? 0.08 : 0.04;
        }
    }
}
