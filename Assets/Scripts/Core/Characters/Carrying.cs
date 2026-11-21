using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;

namespace Thatchwick.Core.Characters
{
    /// <summary>
    /// Eşyanın taşıma ağırlığı (kg, Project belgesi 59 §6.2). Veride <see cref="ItemDef.Mass"/> yazılıysa o, yoksa kategori,
    /// etiket ve kimlikten tablo. Yaklaşık değerler; elle düzeltme veriye "mass" yazılarak yapılır.
    /// </summary>
    public static class ItemMass
    {
        public const float Default = 1f;

        public static float Of(ItemDef def)
        {
            if (def == null) return Default;
            if (def.Mass > 0f) return def.Mass;
            string id = def.Id ?? "";
            bool Tag(string t) => def.Tags != null && def.Tags.Contains(t);
            bool Has(string part) => id.IndexOf(part, StringComparison.Ordinal) >= 0;

            if (def.Placement != ItemPlacement.Resource)
            {
                // Yerleştirilmemiş mobilya (59 §6.2: 5-60 kg): ayak izine göre.
                switch (def.Placement)
                {
                    case ItemPlacement.Surface: return 1.5f;
                    case ItemPlacement.Wall: return 4f;
                    case ItemPlacement.Ceiling: return 4f;
                    case ItemPlacement.Rug: return 5f;
                }
                return Math.Min(60f, 8f * Math.Max(1, def.Width) * Math.Max(1, def.Height));
            }
            if (Tag("giant")) return 15f;
            if (Tag("treehouse_kit")) return 20f;
            if (id.StartsWith("item.seed_", StringComparison.Ordinal) || Tag("critter") || Has("bait")) return 0.1f;
            if (id.StartsWith("item.sapling_", StringComparison.Ordinal)) return 1.5f;
            if (Has("_log") || id == "item.log" || Has("timber")) return 15f;
            if (Tag("stone") || Has("ore") || Has("brick") || Has("slate") || Has("_stone") || Has("iron_bloom")) return 6f;
            if (Has("clay") || Has("lime") || Has("mortar") || Has("plaster")) return 5f;
            if (Tag("metal") || Has("ingot") || Has("_bar")) return 4f;
            if (Has("plank") || Has("board") || Has("beam")) return 4f;
            if (Tag("fertilizer") || Has("manure") || Has("compost")) return 4f;
            if (Tag("fuel") || Has("firewood") || Has("wood") || Has("charcoal")) return 3f;
            if (Tag("hide")) return 3f;
            if (Tag("sheaf")) return 3f;
            if (Tag("grain") || Tag("feed") || Tag("straw") || Has("flour")) return 2f;
            if (Tag("tool") || Tag("saddle")) return 2.5f;
            if (Tag("fish")) return 0.8f;
            if (Tag("drink")) return 1f;
            if (Tag("cloth") || Tag("fiber") || Tag("leather") || Has("wool") || def.Category == "clothing") return 1f;
            if (Tag("glass")) return 1f;
            if (Tag("herb") || Tag("spice") || Tag("dye") || Tag("dye_source") || Tag("writing") || Tag("trim")
                || Tag("potion") || Tag("remedy") || Tag("hair_care") || Tag("coffee") || Has("tea_")) return 0.2f;
            if (Tag("light")) return 0.3f;
            if (Tag("food") || Tag("cooked") || Tag("raw") || Tag("wild") || Tag("preserved") || Tag("condiment")) return 0.4f;
            return Default;
        }
    }

    /// <summary>
    /// Güç (1-10, belge 59 §7.1 ve 58d §1.2 kararları): hesaplanır, ayrı eğitilmez. Yaş, beden yapısı, Gürbüz özelliği,
    /// Atletizm, Hamal dalı, ömür boyu ağır işin küçük katkısı (en çok +1) ve anlık durum. Cinsiyet farkı yok.
    /// </summary>
    public static class Strength
    {
        public const int Min = 1, Max = 10;
        /// <summary>Ağır işte geçen bu kadar oyun günü +1 güç verir (tek sefer).</summary>
        public const double WorkBonusDays = 6.0;
        public const string SturdyTrait = "trait.sturdy", PorterBranch = "branch.ath_b";

        public static int Of(Character c)
        {
            if (c == null) return Min;
            int s;
            switch (c.Stage)
            {
                case AgeStage.Baby: return Min;
                case AgeStage.Child: s = 2; break;
                case AgeStage.Teen: s = 4; break;
                case AgeStage.Elder: s = 3; break;
                default: s = 5; break;
            }
            var build = c.Appearance != null ? c.Appearance.Build : BodyBuild.Average;
            if (build == BodyBuild.Slim) s -= 1;
            else if (build == BodyBuild.Sturdy) s += 1;
            if (c.HasTrait(SturdyTrait)) s += 1;
            s += c.Skills.GetLevel(SkillType.Athletics) / 3;
            if (c.HasBranch(PorterBranch)) s += 1;
            if (c.HeavyWorkDays >= WorkBonusDays) s += 1;
            // Anlık durum.
            if (c.Needs.Get(NeedType.Hunger) < 25f) s -= 1;
            if (c.Needs.Get(NeedType.Energy) < 20f) s -= 1;
            if (c.Injury != null) s -= 2;
            if (c.IsPregnant) s -= 1;
            if (c.Stage == AgeStage.Elder && c.WalkFactor < 1.0) s -= 1;   // diz sıkıntısı
            return s < Min ? Min : s > Max ? Max : s;
        }

        /// <summary>Rahat taşıma (kg): 10 + 4 × Güç (59 §7.2).</summary>
        public static float CapacityKg(int strength) => 10f + 4f * strength;

        /// <summary>Ağır iş: madencilik, odunculuk, taş ustalığı, demircilik.</summary>
        public static bool IsHeavySkill(SkillType? s) =>
            s == SkillType.Mining || s == SkillType.Woodcutting || s == SkillType.Masonry || s == SkillType.Smithing;
    }

    /// <summary>Yük basamağı (59 §7.3).</summary>
    public enum LoadLevel { Light = 0, Heavy = 1, Overloaded = 2, Full = 3 }

    /// <summary>Yük kuralları: rahat (≤%100), ağır (%100-125), aşırı (%125-150), dolu (%150: hiç alamaz).</summary>
    public static class LoadRules
    {
        public const float HeavyFrom = 1f, OverFrom = 1.25f, HardCap = 1.5f;

        public static LoadLevel LevelOf(float ratio) =>
            ratio >= HardCap ? LoadLevel.Full : ratio > OverFrom ? LoadLevel.Overloaded : ratio > HeavyFrom ? LoadLevel.Heavy : LoadLevel.Light;

        /// <summary>Yürüme çarpanı: ağır −%15, aşırı ve dolu −%35.</summary>
        public static double WalkFactor(float ratio)
        {
            var l = LevelOf(ratio);
            return l == LoadLevel.Light ? 1.0 : l == LoadLevel.Heavy ? 0.85 : 0.65;
        }

        /// <summary>Yorulma çarpanı: ağır ×1,15, aşırı ve dolu ×1,3.</summary>
        public static float EnergyFactor(float ratio)
        {
            var l = LevelOf(ratio);
            return l == LoadLevel.Light ? 1f : l == LoadLevel.Heavy ? 1.15f : 1.3f;
        }

        /// <summary>Aşırı yükte koşulmaz ve ata binilmez.</summary>
        public static bool CanRun(float ratio) => ratio <= OverFrom;
    }

    /// <summary>
    /// Kişisel çanta (59 §4, karar A): ev ve çiftlikte ortak depo; dışarıda (orman, göl, köy yolu, köy meydanı) toplanan ve
    /// alınan eşya çantaya girer, çiftliğe dönünce depoya boşalır. Sınır ağırlıktır: taşıma gücünün 1,5 katından sonra alamaz.
    /// </summary>
    public sealed class CarryBag
    {
        private readonly List<KeyValuePair<ItemKey, int>> _stacks = new List<KeyValuePair<ItemKey, int>>();

        public float TotalKg { get; private set; }
        public bool IsEmpty => _stacks.Count == 0;
        public IReadOnlyList<KeyValuePair<ItemKey, int>> Stacks => _stacks;
        public int Version { get; private set; }

        public void Add(ItemKey key, int count, float unitKg)
        {
            if (count <= 0) return;
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (!_stacks[i].Key.Equals(key)) continue;
                _stacks[i] = new KeyValuePair<ItemKey, int>(key, _stacks[i].Value + count);
                TotalKg += unitKg * count; Version++;
                return;
            }
            _stacks.Add(new KeyValuePair<ItemKey, int>(key, count));
            TotalKg += unitKg * count; Version++;
        }

        /// <summary>Çantayı boşaltır; içindekileri döner.</summary>
        public List<KeyValuePair<ItemKey, int>> Drain()
        {
            var all = new List<KeyValuePair<ItemKey, int>>(_stacks);
            _stacks.Clear(); TotalKg = 0f; Version++;
            return all;
        }

        /// <summary>Ağırlığı yeniden hesaplar (kayıttan yükleme; ağırlık tablosu değişmiş olabilir).</summary>
        public void Recompute(Func<string, float> unitKg)
        {
            float t = 0f;
            foreach (var kv in _stacks) t += (unitKg?.Invoke(kv.Key.Id) ?? ItemMass.Default) * kv.Value;
            TotalKg = t; Version++;
        }

        /// <summary>Kaç tane daha alınabilir (sert sınır <paramref name="hardCapKg"/>).</summary>
        public int Room(float unitKg, float hardCapKg)
        {
            if (unitKg <= 0f) return int.MaxValue;
            float room = hardCapKg - TotalKg;
            return room <= 0f ? 0 : (int)Math.Floor(room / unitKg + 1e-4);
        }
    }

    /// <summary>
    /// Eşyayı kimin aldığı: JobRunner kendi karakterinin işini yürütürken bunu o karaktere ayarlar; hane deposuna o anda
    /// giren eşya o karakterin çantasına yönlendirilebilir. Simülasyon tek iş parçacığında koşar.
    /// </summary>
    public static class CarryContext
    {
        [ThreadStatic] private static Character _current;
        public static Character Current => _current;

        public static Scope Begin(Character c) { var prev = _current; _current = c; return new Scope(prev); }

        public readonly struct Scope : IDisposable
        {
            private readonly Character _prev;
            public Scope(Character prev) { _prev = prev; }
            public void Dispose() { _current = _prev; }
        }
    }
}
