using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Upbringing
{
    public enum WeddingScale { Plain = 0, Fine = 1, Grand = 2 }

    /// <summary>Düğün hazırlığının bir kalemi: ekmek, içki, mum ya da çalgıcı.</summary>
    public sealed class WeddingSupply
    {
        public string Name;
        public string[] ItemIds;
        public int Need;
        public int Have;
        public bool Ok => Have >= Need;
    }

    public sealed class WeddingResult
    {
        public WeddingScale Scale;
        public int Reputation;
        public bool DowryFull;
        public List<string> Used = new List<string>();
        public string Text;
    }

    /// <summary>
    /// Çeyiz ve düğün (belge 34-D2, onay 19.11.2026). Düğün ekmek, içki, mum ve çalgıcı ister; hazır olan kalemler
    /// düğünde harcanır. Sayı ölçeği belirler: 0-1 sade, 2-3 güzel, 4 görkemli; dolu çeyiz sandığı ölçeği bir basamak artırır.
    /// Ölçek itibar ve hane ruh hali verir. Hanenin itibarı ve evdeki çeyiz, köye gelen taliplerin niteliğini artırır.
    /// </summary>
    public sealed class WeddingSystem
    {
        public const int ReputationPlain = 5, ReputationFine = 12, ReputationGrand = 25;
        public const int MusicLevel = 3;

        private readonly Household _house;
        private readonly Population _population;

        public WeddingSystem(Household house, Population population)
        {
            _house = house;
            _population = population ?? throw new ArgumentNullException(nameof(population));
        }

        /// <summary>Hanede o oyuncakta ustalaşmış biri çalgıcı sayılır (kaval, jonglör; belge 30).</summary>
        public Func<int, bool> IsPerformer { get; set; }
        /// <summary>Evdeki en dolu çeyiz sandığı (dolu, en çok); sandık yoksa (0, 0).</summary>
        public Func<(int filled, int max)> DowryFill { get; set; }
        public ReputationSystem Reputation { get; set; }
        public Action<string, int> Notify { get; set; }
        public Action<string> Chronicle { get; set; }

        public int Weddings { get; private set; }
        public int GrandWeddings { get; private set; }

        public static readonly string[] Breads = { "item.wedding_bread", "item.bread", "item.bread_sourdough", "item.bread_rye", "item.bread_maslin", "item.simit", "item.bazlama" };
        /// <summary>Düğün yemeği (Aşçılık 9): varsa harcanır, itibar +4 (belge 45).</summary>
        public const string WeddingDishId = "item.wedding_dish";
        public const int WeddingDishReputation = 4, FamilySongReputation = 5, FamilySongLevel = 10;
        public static readonly string[] Drinks = { "item.ale", "item.cider", "item.mead", "item.serbet", "item.boza" };
        public static readonly string[] Lights = { "item.candle", "item.tallow_candle", "item.beeswax_candle" };
        public const int BreadNeed = 4, DrinkNeed = 4, LightNeed = 3;

        /// <summary>Hazırlık durumu: üç malzeme kalemi ve çalgıcı.</summary>
        public List<WeddingSupply> Prep()
        {
            var list = new List<WeddingSupply>
            {
                new WeddingSupply { Name = "Ekmek", ItemIds = Breads, Need = BreadNeed, Have = Count(Breads) },
                new WeddingSupply { Name = "İçki", ItemIds = Drinks, Need = DrinkNeed, Have = Count(Drinks) },
                new WeddingSupply { Name = "Mum", ItemIds = Lights, Need = LightNeed, Have = Count(Lights) },
                new WeddingSupply { Name = "Çalgıcı", ItemIds = Array.Empty<string>(), Need = 1, Have = Musician() != null ? 1 : 0 }
            };
            return list;
        }

        /// <summary>Hanede çalgı çalabilen biri (Müzik 3+ ya da kaval/jonglörde usta).</summary>
        public Character Musician()
        {
            foreach (var m in _population.Members)
            {
                if (m.IsDead || m.IsVillager || m.Stage == AgeStage.Baby) continue;
                if (m.Skills.GetLevel(SkillType.Music) >= MusicLevel || (IsPerformer != null && IsPerformer(m.Id))) return m;
            }
            return null;
        }

        public static WeddingScale ScaleFor(int readyCount, bool dowryFull)
        {
            int step = readyCount >= 4 ? 2 : readyCount >= 2 ? 1 : 0;
            if (dowryFull) step++;
            return (WeddingScale)Math.Min(2, step);
        }

        public static string ScaleName(WeddingScale s) => s == WeddingScale.Grand ? "Görkemli" : s == WeddingScale.Fine ? "Güzel" : "Sade";

        /// <summary>Hazırlığa göre beklenen ölçek (harcamadan).</summary>
        public WeddingScale ExpectedScale(bool dowryFull)
        {
            int ready = 0;
            foreach (var s in Prep()) if (s.Ok) ready++;
            return ScaleFor(ready, dowryFull);
        }

        /// <summary>
        /// Düğün: hazır kalemleri harcar, ölçeği hesaplar, itibar ve ruh hali verir.
        /// a hane üyesi, b eşi (hane dışından gelen ya da giden). dowryFull: çeyiz sandığı tam doluydu.
        /// </summary>
        public WeddingResult Celebrate(Character a, Character b, bool dowryFull)
        {
            var r = new WeddingResult { DowryFull = dowryFull };
            int ready = 0;
            foreach (var s in Prep())
            {
                if (!s.Ok) continue;
                ready++;
                if (s.ItemIds.Length == 0) { r.Used.Add($"çalgıcı {Musician()?.Name}"); continue; }
                int left = s.Need;
                foreach (var id in s.ItemIds)
                    while (left > 0 && _house.TryTakeItem(id)) left--;
                r.Used.Add($"{s.Need} {s.Name.ToLowerInvariant()}");
            }
            r.Scale = ScaleFor(ready, dowryFull);
            r.Reputation = r.Scale == WeddingScale.Grand ? ReputationGrand : r.Scale == WeddingScale.Fine ? ReputationFine : ReputationPlain;
            if (_house.TryTakeItem(WeddingDishId)) { r.Reputation += WeddingDishReputation; r.Used.Add("düğün yemeği"); }
            var musician = Musician();
            if (musician != null && musician.Skills.GetLevel(SkillType.Music) >= FamilySongLevel) { r.Reputation += FamilySongReputation; r.Used.Add("aile ezgisi"); }
            Weddings++;
            if (r.Scale == WeddingScale.Grand) GrandWeddings++;

            string names = $"{a?.Name} ile {b?.Name}";
            Reputation?.Add(r.Reputation, $"{ScaleName(r.Scale).ToLowerInvariant()} düğün");
            foreach (var m in _population.Members)
            {
                if (m.IsDead || m.IsVillager) continue;
                if (r.Scale == WeddingScale.Grand) m.AddMoodlet("wedding_grand", "Görkemli düğün", 5, 3.0);
                else if (r.Scale == WeddingScale.Fine) m.AddMoodlet("wedding_fine", "Güzel düğün", 3, 2.0);
                else m.AddMoodlet("wedding_plain", "Düğün", 1, 1.0);
            }
            if (b != null && !b.IsDead) b.AddMoodlet("wedding_self", "Düğünüm", r.Scale == WeddingScale.Grand ? 6 : 3, 3.0);
            string used = r.Used.Count > 0 ? " (" + string.Join(", ", r.Used) + ")" : "";
            r.Text = $"{ScaleName(r.Scale)} düğün: {names}{used}{(dowryFull ? ", dolu çeyiz" : "")}. İtibar +{r.Reputation}.";
            Notify?.Invoke(r.Text, a?.Id ?? 0);
            if (r.Scale == WeddingScale.Grand) Chronicle?.Invoke($"Görkemli düğün: {names}");
            return r;
        }

        /// <summary>Damadın getirdikleri: alet ve yapı malzemesi, biraz akçe (gelinin çeyiz sandığının karşılığı).</summary>
        public static readonly (string id, int count)[] GroomGifts = { ("item.plank", 6), ("item.iron_bar", 2), ("item.leather", 2) };
        public const int GroomAkce = 20;

        /// <summary>Haneye damat geldi: alet malzemesi ve akçe getirir (belge 34-D2, damat tarafı). Bildirim metni ya da null.</summary>
        /// <summary>Damadın ailesinin hâli: 1 sıradan, 2 nitelikli talip. Getirdikleri bu katla gelir.</summary>
        public Func<int> GiftScale { get; set; }

        public string OnGroomJoined(Character groom, Func<string, bool> itemExists = null)
        {
            if (groom == null || groom.Sex != Sex.Male || _house == null) return null;
            int k = Math.Max(1, GiftScale != null ? GiftScale() : 1);
            var parts = new List<string>();
            foreach (var (id, count) in GroomGifts)
            {
                if (itemExists != null && !itemExists(id)) continue;
                _house.AddItem(id, count * k);
                parts.Add($"{count * k} {id.Replace("item.", "")}");
            }
            _house.AddAkce(GroomAkce * k);
            string msg = $"{groom.Name} damat olarak geldi; yanında {(k > 1 ? "bolca " : "")}alet malzemesi ve {GroomAkce * k} akçe getirdi.";
            Notify?.Invoke(msg, groom.Id);
            return msg;
        }

        /// <summary>
        /// Talip niteliği şansı (0-0,6): itibar (en çok 0,4) ve evdeki dolu çeyiz sandığı (+0,2).
        /// Köye gelen talip bu şansla ek yatkınlık ve becerili gelir.
        /// </summary>
        public double SuitorQuality()
        {
            double q = Reputation == null ? 0 : Math.Max(0, Math.Min(0.4, Reputation.Points / 600.0 * 0.4));
            if (DowryFill != null) { var (f, max) = DowryFill(); if (max > 0 && f >= max) q += 0.2; }
            return Math.Min(0.6, q);
        }

        private int Count(string[] ids)
        {
            int n = 0;
            if (_house == null) return 0;
            foreach (var id in ids) n += _house.Count(id);
            return n;
        }

        public WeddingSave Export() => new WeddingSave { weddings = Weddings, grand = GrandWeddings };

        public void Import(WeddingSave s)
        {
            Weddings = s?.weddings ?? 0;
            GrandWeddings = s?.grand ?? 0;
        }
    }

    [Serializable] public sealed class WeddingSave { public int weddings, grand; }
}
