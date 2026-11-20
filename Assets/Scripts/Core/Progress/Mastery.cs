using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Progress
{
    /// <summary>
    /// Ustalık (Project belgesi 45, 8. bölüm, ortak basamaklar): Sv 7 boşta beceri gösterisi, Sv 10 usta unvanı,
    /// ustalık eseri ve pazar günü usta siparişi. Kurallar burada; bağlantılar LifeSystems ve GameRoot'ta.
    /// </summary>
    public static class Mastery
    {
        public const int ShowcaseLevel = 7;
        public const int MasterLevel = 10;
        /// <summary>Usta bir tarifi bitirince ustalık eseri çıkma olasılığı (bir fazla ürün, moodlet, kronik).</summary>
        public const double MasterworkChance = 0.25;
        public const int MasterworkReputation = 1;
        public const int OrderReputation = 5;
        public const double OrderPriceFactor = 1.6;
        public const int OrderCount = 2;

        /// <summary>Unvanı ayrı adla anılan beceriler; ötekilerde meslek adı (SkillNames.TradeName).</summary>
        private static readonly Dictionary<SkillType, string> Titles = new Dictionary<SkillType, string>
        {
            { SkillType.Horticulture, "Bağban" }, { SkillType.Woodcutting, "Baltacı" }, { SkillType.Riding, "Süvari" },
            { SkillType.Hunting, "Avcıbaşı" }, { SkillType.Medicine, "Köy hekimi" }, { SkillType.Athletics, "Pehlivan" },
            { SkillType.AnimalHusbandry, "Sürü sahibi" }, { SkillType.Stewardship, "Kâhya" }, { SkillType.Charm, "Söz ehli" }
        };

        /// <summary>Karakterin 10. seviyedeki becerisi (birden çoksa en çok XP'li); yoksa null.</summary>
        public static SkillType? MasterSkill(Character c)
        {
            if (c == null) return null;
            SkillType? best = null; double bx = -1;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                if (c.Skills.GetLevel(s) < MasterLevel) continue;
                double x = c.Skills.GetXp(s);
                if (x > bx) { bx = x; best = s; }
            }
            return best;
        }

        public static string TitleFor(SkillType s) => Titles.TryGetValue(s, out var t) ? t : SkillNames.TradeName(s);

        /// <summary>Usta unvanı ("Demirci"); usta değilse null.</summary>
        public static string Title(Character c) => MasterSkill(c) is SkillType s ? TitleFor(s) : null;

        /// <summary>Unvanlı ad: "Demirci Maud"; usta değilse yalnızca ad.</summary>
        public static string DisplayName(Character c) => c == null ? "" : Title(c) is string t ? $"{t} {c.Name}" : c.Name;

        // ---- Sv 7: boşta beceri gösterisi ----

        private static readonly Dictionary<SkillType, string> ShowTexts = new Dictionary<SkillType, string>
        {
            { SkillType.Carpentry, "bir tahta parçasına oyma yapıyor" }, { SkillType.Music, "kopuzunu akort edip bir ezgi çalıyor" },
            { SkillType.Weaving, "parmaklarında ip eğiriyor" }, { SkillType.Smithing, "bir çivi parçasını ustaca büküyor" },
            { SkillType.Cooking, "yarın için baharat ayıklıyor" }, { SkillType.Charm, "çevresindekilere bir hikâye anlatıyor" },
            { SkillType.Athletics, "taş kaldırıp güç gösteriyor" }, { SkillType.Herbalism, "kurumuş otları kokusundan tanıyor" },
            { SkillType.Leatherwork, "bir deri parçasına desen işliyor" }, { SkillType.Pottery, "avucunda kilden küçük bir kuş yapıyor" },
            { SkillType.WeatherLore, "bulutlara bakıp yarını söylüyor" }, { SkillType.Literacy, "bir duvara harfler kazıyor" },
            { SkillType.Riding, "atının yelesini örüyor" }, { SkillType.Hunting, "bir kuşun ötüşünü taklit ediyor" },
        };

        /// <summary>Gösteriye çıkacak beceri: 7+ seviyedeki en yüksek beceri; yoksa null.</summary>
        public static SkillType? ShowcaseSkill(Character c)
        {
            if (c == null || c.Stage < AgeStage.YoungAdult) return null;
            SkillType? best = null; double bx = -1;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                if (c.Skills.GetLevel(s) < ShowcaseLevel) continue;
                double x = c.Skills.GetXp(s);
                if (x > bx) { bx = x; best = s; }
            }
            return best;
        }

        public static string ShowText(Character c, SkillType s) =>
            $"{DisplayName(c)} {(ShowTexts.TryGetValue(s, out var t) ? t : SkillNames.Name(s).ToLowerInvariant() + " ustalığını gösteriyor")}.";

        /// <summary>
        /// Gösteriyi uygular: izleyenlerin keyfi dolar ve "Ustayı seyretti" moodlet'i alır; çocuk ve ergen izleyen o becerinin
        /// XP'sini kazanır; gösteren biraz Gönül kazanma XP'si alır. İzleyen sayısını döner.
        /// </summary>
        public static int ApplyShowcase(Character performer, SkillType s, IEnumerable<Character> nearby)
        {
            int n = 0;
            foreach (var w in nearby)
            {
                if (w == null || w.IsDead || w.Id == performer.Id || w.Stage == AgeStage.Baby) continue;
                w.Needs.Restore(NeedType.Fun, 10f);
                w.AddMoodlet(MoodCatalog.SkillShowcase);
                if (w.Stage == AgeStage.Child || w.Stage == AgeStage.Teen) w.GainSkillXp(s, 3);
                n++;
            }
            if (n > 0) performer.GainSkillXp(SkillType.Charm, 2);
            return n;
        }

        // ---- Sv 10: ustalık eseri ----

        /// <summary>Usta, becerisinin bir tarifini bitirdi mi (ustalık eseri denemesi için).</summary>
        public static bool IsMasterCraft(Character c, InteractionDef i) =>
            c != null && i != null && i.IsCraft && i.Skill.HasValue && i.Produce.Count > 0 && c.Skills.GetLevel(i.Skill.Value) >= MasterLevel;

        // ---- Sv 10: pazar günü usta siparişi ----

        /// <summary>
        /// Hanede usta olan her beceri için bir sipariş: o becerinin bir tarifinin ürünü (pazar fiyatı olan), 2 tane,
        /// pazar fiyatının 1,6 katı ve itibar +5. Tohum gün ve beceriye bağlı (yüklemede aynı).
        /// </summary>
        public static List<(SkillType skill, string item, int count, int reward)> Orders(ContentDatabase db, IEnumerable<SkillType> masterSkills, long day)
        {
            var list = new List<(SkillType, string, int, int)>();
            if (db == null) return list;
            foreach (var s in masterSkills.Distinct())
            {
                var products = new List<ItemDef>();
                foreach (var d in db.Items.All)
                    foreach (var i in d.Interactions)
                        if (i.IsCraft && i.Skill == s && !i.Hidden && i.Produce.Count > 0 && db.Items.TryGet(i.Produce[0].ItemId, out var p)
                            && p.Price > 0 && p.Placement == ItemPlacement.Resource && !products.Contains(p)) products.Add(p);
                if (products.Count == 0) continue;
                products.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
                var rng = new Random(unchecked((int)day * 7919 + (int)s * 104729));
                var pick = products[rng.Next(products.Count)];
                list.Add((s, pick.Id, OrderCount, Math.Max(1, (int)Math.Ceiling(pick.Price * OrderCount * OrderPriceFactor))));
            }
            return list;
        }
    }
}
