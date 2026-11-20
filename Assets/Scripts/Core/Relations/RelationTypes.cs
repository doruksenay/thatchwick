using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>Arkadaşlık aşamaları (Project belgesi 67b §1): Düşman, Gergin, Tanıdık, Arkadaş, Yakın arkadaş, Kanka.</summary>
    public enum FriendStage { Enemy = 0, Tense = 1, Acquaintance = 2, Friend = 3, CloseFriend = 4, BestFriend = 5 }

    /// <summary>Romantik aşamalar: Soğumuş, (yok), Hoşlanıyor, İlgili, Âşık, Hayatının aşkı.</summary>
    public enum RomanceStage { Cold = 0, None = 1, Attracted = 2, Interested = 3, InLove = 4, LoveOfLife = 5 }

    /// <summary>Resmî gönül durumu (çift başına). Evlilik <see cref="Character.SpouseId"/> ile tutulur.</summary>
    public enum CoupleStatus { None = 0, Sweetheart = 1, Engaged = 2, Ex = 3, ExSpouse = 4 }

    /// <summary>Aile ilişki türü (67a §9.4): Yakın, Mesafeli, Şakacı, Sert, Koruyucu, Gergin.</summary>
    public enum FamilyDynamic { None = 0, Close = 1, Distant = 2, Jokester = 3, Strict = 4, Protective = 5, Tense = 6 }

    /// <summary>Hisler (67a §4): bir karakterin başka biri hakkındaki tek yönlü hissi.</summary>
    public enum FeelingKind
    {
        Grateful = 0, Trusts = 1, Admires = 2, Close = 3, Misses = 4, Hurt = 5, Heartbroken = 6, Jealous = 7, Betrayed = 8,
        Grudge = 9, Ashamed = 10, SharedGrief = 11
    }

    /// <summary>Gönül tercihi etiketi (67b §4.1): huy, beceri, görünüş, bakım.</summary>
    public enum AttractionTag
    {
        Cheerful = 0, Diligent = 1, Placid = 2, Generous = 3, Gloomy = 4, Introvert = 5, Sociable = 6,
        GoodCook = 10, Craftsman = 11, Musician = 12, Strong = 13,
        Bearded = 20, Mustached = 21, CleanShaven = 22, LongHair = 23, Sturdy = 24, Slim = 25,
        Clean = 30, WellDressed = 31
    }

    /// <summary>Eşler arası tartışma tetikleri (67c §2, T1-T12).</summary>
    public enum SpouseTrigger
    {
        Infidelity = 1, SoldHeirloom = 2, SpentDowry = 3, NeglectedChild = 4, ForgotAnniversary = 5, UnfairChores = 6,
        InLawQuarrel = 7, SpilledSecret = 8, Flirting = 9, OutAllNight = 10, Gambling = 11, Mess = 12
    }

    /// <summary>İtibarın dört yönü (67b §7.3).</summary>
    public enum RepFacet { Word = 0, Generous = 1, Craft = 2, Peace = 3 }

    /// <summary>Tek yönlü bir his örneği.</summary>
    public sealed class Feeling
    {
        public FeelingKind Kind;
        public double FromDay;
        /// <summary>Bittiği gün; ömür boyu sürenler için <see cref="double.MaxValue"/>.</summary>
        public double UntilDay;
    }

    /// <summary>Hissin tanımı: süre, ruh hali etkisi (yanındayken), ilişki kazanç çarpanı.</summary>
    public sealed class FeelingDef
    {
        public FeelingKind Kind;
        public double Days;
        /// <summary>İki kişi 4 karo içindeyken his sahibine verilen iç ses değeri.</summary>
        public int NearMood;
        /// <summary>Arkadaşlık kazancına çarpan (1 = etkisiz).</summary>
        public double GainFactor = 1.0;
        public bool Positive;
        public string Key => "rel.feel." + Kind.ToString().ToLowerInvariant();
        public string Icon => "feel_" + IconName(Kind);

        private static string IconName(FeelingKind k)
        {
            switch (k)
            {
                case FeelingKind.Grateful: return "grateful";
                case FeelingKind.Trusts: return "trusts";
                case FeelingKind.Admires: return "admires";
                case FeelingKind.Close: return "close";
                case FeelingKind.Misses: return "misses";
                case FeelingKind.Hurt: return "hurt";
                case FeelingKind.Heartbroken: return "heartbroken";
                case FeelingKind.Jealous: return "jealous";
                case FeelingKind.Betrayed: return "betrayed";
                case FeelingKind.Grudge: return "grudge";
                case FeelingKind.Ashamed: return "ashamed";
                default: return "shared_grief";
            }
        }
    }

    /// <summary>Aşama, his ve tercih tabloları.</summary>
    public static class RelationRules
    {
        public const int Min = -100, Max = 100;
        /// <summary>Sevgili olmadan romantik bu değerin üstüne çıkmaz (Stardew'daki 8 kalp kilidi; 67b §3.3).</summary>
        public const int RomanceLockWithoutCouple = 59;
        public const int MaxFeelingsPerPair = 3;

        public static FriendStage StageOf(int friendship) =>
            friendship <= -60 ? FriendStage.Enemy : friendship <= -20 ? FriendStage.Tense : friendship < 20 ? FriendStage.Acquaintance
            : friendship < 50 ? FriendStage.Friend : friendship < 80 ? FriendStage.CloseFriend : FriendStage.BestFriend;

        public static RomanceStage RomanceStageOf(int romance) =>
            romance <= -30 ? RomanceStage.Cold : romance <= 0 ? RomanceStage.None : romance < 30 ? RomanceStage.Attracted
            : romance < 60 ? RomanceStage.Interested : romance < 90 ? RomanceStage.InLove : RomanceStage.LoveOfLife;

        /// <summary>Azalmanın durduğu taban: aşamanın alt sınırı (Arkadaş 20, Yakın arkadaş 50, Kanka 80).</summary>
        public static int DecayFloor(int friendship)
        {
            if (friendship >= 80) return 80;
            if (friendship >= 50) return 50;
            if (friendship >= 20) return 20;
            return 0;
        }

        public static LocText StageText(FriendStage s) => LocText.Of("rel.stage.f." + Snake(s.ToString()));
        public static LocText RomanceText(RomanceStage s) => LocText.Of("rel.stage.r." + Snake(s.ToString()));
        public static LocText StatusText(CoupleStatus s) => LocText.Of("rel.status." + Snake(s.ToString()));
        public static LocText DynamicText(FamilyDynamic d) => LocText.Of("rel.dyn." + Snake(d.ToString()));
        public static LocText TagText(AttractionTag t) => LocText.Of("rel.tag." + Snake(t.ToString()));
        public static LocText TriggerText(SpouseTrigger t) => LocText.Of("rel.trig." + Snake(t.ToString()));
        public static LocText FacetText(RepFacet f) => LocText.Of("rel.rep." + Snake(f.ToString()));

        public static string Snake(string s)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (char.IsUpper(ch) && i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(ch));
            }
            return sb.ToString();
        }

        public static readonly Dictionary<FeelingKind, FeelingDef> Feelings = new Dictionary<FeelingKind, FeelingDef>
        {
            [FeelingKind.Grateful] = new FeelingDef { Kind = FeelingKind.Grateful, Days = 40, NearMood = 3, GainFactor = 1.5, Positive = true },
            [FeelingKind.Trusts] = new FeelingDef { Kind = FeelingKind.Trusts, Days = 30, NearMood = 2, GainFactor = 1.2, Positive = true },
            [FeelingKind.Admires] = new FeelingDef { Kind = FeelingKind.Admires, Days = 30, NearMood = 2, GainFactor = 1.2, Positive = true },
            [FeelingKind.Close] = new FeelingDef { Kind = FeelingKind.Close, Days = 3, NearMood = 2, GainFactor = 1.25, Positive = true },
            [FeelingKind.Misses] = new FeelingDef { Kind = FeelingKind.Misses, Days = 5, NearMood = 0, GainFactor = 1.5, Positive = true },
            [FeelingKind.Hurt] = new FeelingDef { Kind = FeelingKind.Hurt, Days = 3, NearMood = -3, GainFactor = 0.5 },
            [FeelingKind.Heartbroken] = new FeelingDef { Kind = FeelingKind.Heartbroken, Days = 20, NearMood = -4, GainFactor = 0.5 },
            [FeelingKind.Jealous] = new FeelingDef { Kind = FeelingKind.Jealous, Days = 3, NearMood = -3, GainFactor = 0.7 },
            [FeelingKind.Betrayed] = new FeelingDef { Kind = FeelingKind.Betrayed, Days = 60, NearMood = -6, GainFactor = 0.3 },
            [FeelingKind.Grudge] = new FeelingDef { Kind = FeelingKind.Grudge, Days = 20, NearMood = -3, GainFactor = 0.5 },
            [FeelingKind.Ashamed] = new FeelingDef { Kind = FeelingKind.Ashamed, Days = 3, NearMood = -2, GainFactor = 0.8 },
            [FeelingKind.SharedGrief] = new FeelingDef { Kind = FeelingKind.SharedGrief, Days = 40, NearMood = 1, GainFactor = 1.2, Positive = true },
        };

        // ---- uyum (67 §6-5) ----

        private static readonly string[] Warm = { "trait.cheerful", "trait.sociable", "trait.generous", "trait.placid" };

        /// <summary>Uyumlu ve çatışan huy çiftleri (sırası önemsiz).</summary>
        private static readonly (string a, string b, int score)[] TraitPairs =
        {
            ("trait.cheerful", "trait.cheerful", 2), ("trait.cheerful", "trait.gloomy", -2), ("trait.sociable", "trait.introvert", -1),
            ("trait.introvert", "trait.introvert", 1), ("trait.diligent", "trait.diligent", 1), ("trait.placid", "trait.gloomy", 1),
            ("trait.generous", "trait.glutton", -1), ("trait.sociable", "trait.sociable", 1), ("trait.greenthumb", "trait.greenthumb", 1),
            ("trait.handy", "trait.handy", 1), ("trait.diligent", "trait.glutton", -1),
        };

        /// <summary>Uyum −2 (Kötü) … +2 (Çok iyi): huylar, zevkler, yaş farkı. Kazanç çarpanı <see cref="CompatFactor"/>.</summary>
        public static int Compatibility(Character a, Character b)
        {
            if (a == null || b == null) return 0;
            int score = 0;
            foreach (var p in TraitPairs)
                if ((a.HasTrait(p.a) && b.HasTrait(p.b)) || (a.HasTrait(p.b) && b.HasTrait(p.a))) score += p.score;
            if (!string.IsNullOrEmpty(a.LikedHobby) && a.LikedHobby == b.LikedHobby) score += 1;
            if (!string.IsNullOrEmpty(a.LikedFood) && a.LikedFood == b.LikedFood) score += 1;
            if (!string.IsNullOrEmpty(a.LikedFood) && a.LikedFood == b.DislikedFood) score -= 1;
            if (!string.IsNullOrEmpty(b.LikedFood) && b.LikedFood == a.DislikedFood) score -= 1;
            return Math.Max(-2, Math.Min(2, score));
        }

        public static double CompatFactor(int compat) => 1.0 + 0.2 * compat;   // ×0,6 … ×1,4

        public static LocText CompatText(int compat) => LocText.Of("rel.compat." + (compat + 2));

        public static bool IsWarm(Character c)
        {
            foreach (var t in Warm) if (c.HasTrait(t)) return true;
            return false;
        }

        /// <summary>Kavga ve laf sokmaya yatkın huy (karamsar, obur, içine kapanık değil).</summary>
        public static double Temper(Character c)
        {
            double t = 1.0;
            if (c.HasTrait("trait.gloomy")) t *= 1.6;
            if (c.HasTrait("trait.glutton")) t *= 1.2;
            if (c.HasTrait("trait.placid")) t *= 0.4;
            if (c.HasTrait("trait.cheerful")) t *= 0.6;
            return t;
        }

        /// <summary>Konuşkanlık (muhabbette söz alma ağırlığı).</summary>
        public static double Talkative(Character c)
        {
            double t = 1.0;
            if (c.HasTrait("trait.sociable")) t *= 1.8;
            if (c.HasTrait("trait.cheerful")) t *= 1.3;
            if (c.HasTrait("trait.introvert")) t *= 0.4;
            if (c.Stage == AgeStage.Elder) t *= 1.2;
            return t;
        }

        /// <summary>Dedikoduya yatkınlık (anlatma ve abartma).</summary>
        public static double Gossipy(Character c)
        {
            double t = 1.0;
            if (c.HasTrait("trait.sociable")) t *= 1.6;
            if (c.HasTrait("trait.introvert")) t *= 0.4;
            if (c.HasTrait("trait.placid")) t *= 0.7;
            return t;
        }

        // ---- gönül tercihleri (67b §4) ----

        public static readonly AttractionTag[] AllTags =
        {
            AttractionTag.Cheerful, AttractionTag.Diligent, AttractionTag.Placid, AttractionTag.Generous, AttractionTag.Gloomy,
            AttractionTag.Introvert, AttractionTag.Sociable, AttractionTag.GoodCook, AttractionTag.Craftsman, AttractionTag.Musician,
            AttractionTag.Strong, AttractionTag.Bearded, AttractionTag.Mustached, AttractionTag.CleanShaven, AttractionTag.LongHair,
            AttractionTag.Sturdy, AttractionTag.Slim, AttractionTag.Clean, AttractionTag.WellDressed
        };

        public static bool HasBeard(Character c) =>
            c != null && c.Appearance != null && (c.Appearance.BeardId != Customization.Appearance.None || c.Appearance.StubbleLevel > 0);

        public static bool HasMustache(Character c) =>
            c != null && c.Appearance != null && c.Appearance.MustacheId != Customization.Appearance.None;

        /// <summary>Karakter bu etiketi taşıyor mu (görünen ve oyunda olan şeylerden).</summary>
        public static bool Matches(Character c, AttractionTag tag)
        {
            if (c == null) return false;
            var a = c.Appearance;
            switch (tag)
            {
                case AttractionTag.Cheerful: return c.HasTrait("trait.cheerful");
                case AttractionTag.Diligent: return c.HasTrait("trait.diligent");
                case AttractionTag.Placid: return c.HasTrait("trait.placid");
                case AttractionTag.Generous: return c.HasTrait("trait.generous");
                case AttractionTag.Gloomy: return c.HasTrait("trait.gloomy");
                case AttractionTag.Introvert: return c.HasTrait("trait.introvert");
                case AttractionTag.Sociable: return c.HasTrait("trait.sociable");
                case AttractionTag.GoodCook: return c.Skills.GetLevel(SkillType.Cooking) >= 5;
                case AttractionTag.Craftsman: return c.Skills.GetLevel(SkillType.Carpentry) >= 7 || c.Skills.GetLevel(SkillType.Smithing) >= 7;
                case AttractionTag.Musician: return c.Skills.GetLevel(SkillType.Music) >= 4;
                case AttractionTag.Strong: return c.Skills.GetLevel(SkillType.Athletics) >= 5 || (a != null && a.Build == Customization.BodyBuild.Sturdy);
                case AttractionTag.Bearded: return a != null && (a.BeardId != Customization.Appearance.None || a.StubbleLevel >= 2);
                case AttractionTag.Mustached: return HasMustache(c);
                case AttractionTag.CleanShaven: return c.Sex == Sex.Male && !HasBeard(c) && !HasMustache(c);
                case AttractionTag.LongHair: return a != null && a.HairStyleId != null && a.HairStyleId.IndexOf("long", StringComparison.Ordinal) >= 0;
                case AttractionTag.Sturdy: return a != null && a.Build == Customization.BodyBuild.Sturdy;
                case AttractionTag.Slim: return a != null && a.Build == Customization.BodyBuild.Slim;
                case AttractionTag.Clean: return c.Needs.Get(Characters.NeedType.Hygiene) >= 60f;
                case AttractionTag.WellDressed: return c.HasTrait("trait.generous") || c.Skills.GetLevel(SkillType.Tailoring) >= 4;
                default: return false;
            }
        }
    }
}
