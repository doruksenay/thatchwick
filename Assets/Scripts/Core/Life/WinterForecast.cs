using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    public sealed class WinterReport
    {
        public bool InWinter;
        /// <summary>Kışa kalan gün (kıştaysa 0); kış sonuna kalan gün WinterDaysLeft.</summary>
        public double DaysToWinter;
        public double WinterDaysLeft;
        /// <summary>Şu anki stokla kaç gün yeter.</summary>
        public double FoodDays;
        public double FirewoodDays;
        /// <summary>Kış sonuna kadar gereken toplam (stok farkıyla birlikte eksik hesaplanır).</summary>
        public double FoodNeededUntilSpring;
        public double FirewoodNeededUntilSpring;
        public double FoodPerDay;        // gıda birimi/gün (hane tüketimi)
        public double FirewoodPerDay;    // odun/gün (ısı kaynakları)
        public int FoodStock, FirewoodStock;
        public List<string> Warnings = new List<string>();
        public bool HasDanger;
    }

    /// <summary>
    /// "Kışa X gün; yiyecek X gün, odun X gün yeter" göstergesi (Adım 18c, T20). Tahmin, hane tüketimine
    /// (yaşayan hane üyelerinin açlık hızı / yiyeceğin doyurduğu miktar; her ısı kaynağının odun yakma hızı)
    /// dayanır; gerçek tüketimle %10 içinde tutması test edilir.
    /// </summary>
    public sealed class WinterForecast
    {
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly WorldState _world;
        private readonly Population _population;
        private readonly TimeConfig _time;
        private readonly HeatConfig _heat;

        /// <summary>Kış yaklaşırken uyarı eşiği (gün).</summary>
        public double WarnDaysBeforeWinter = 12;

        public WinterForecast(ContentDatabase content, Household house, WorldState world, Population population, TimeConfig time, HeatConfig heat = null)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _heat = heat ?? new HeatConfig();
        }

        /// <summary>Bir yiyeceğin doyurduğu açlık miktarı: onu harcayan en iyi Açlık etkileşimi; yoksa 0.</summary>
        public float FoodValue(string itemId)
        {
            float best = 0f;
            foreach (var def in _content.Items.All)
                foreach (var i in def.Interactions)
                    if (i.RestoresNeed && i.Need == NeedType.Hunger && i.ConsumeItem == itemId && i.Restore > best) best = i.Restore;
            return best;
        }

        /// <summary>1 kütükten çıkan odun adedi (odun yarma etkileşiminden); bulunamazsa 1.</summary>
        public int FirewoodPerLog()
        {
            foreach (var def in _content.Items.All)
                foreach (var i in def.Interactions)
                    if (i.ConsumeItem == "item.log")
                        foreach (var p in i.Produce) if (p.ItemId == _heat.FirewoodId && p.Chance >= 1f) return Math.Max(1, p.Count);
            return 1;
        }

        public double FoodStock()
        {
            double total = 0;
            foreach (var kv in _house.InventoryList())
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var def) && def.HasTag("food")) total += kv.Value * FoodValue(kv.Key);
            return total;
        }

        public int FirewoodStock()
        {
            int logs = _house.Count("item.log");
            return _house.Count(_heat.FirewoodId) + logs * FirewoodPerLog();
        }

        /// <summary>Hane tüketimi: günde kaç birim yiyecek (açlık puanı) harcanıyor.</summary>
        public double HungerPointsPerDay()
        {
            double sum = 0;
            foreach (var c in _population.Members)
            {
                if (c.IsDead || c.IsVillager) continue;
                sum += c.Needs.GetDecayPerDay(NeedType.Hunger) * c.Needs.GetDecayMultiplier(NeedType.Hunger);
            }
            return sum;
        }

        /// <summary>Isı kaynağı sayısı (yerleştirilmiş, dekor olmayan).</summary>
        public int HeatSourceCount()
        {
            int n = 0;
            foreach (var it in _world.Items) if (!it.IsDecor && it.Def.HeatOutput > 0f) n++;
            return n;
        }

        public WinterReport Report(double totalDays)
        {
            var r = new WinterReport();
            int dps = _time.DaysPerSeason;
            long day = (long)Math.Floor(totalDays);
            int seasonIndex = (int)((day / dps) % TimeConfig.SeasonsPerYear);
            double inSeason = totalDays - (day / dps) * dps;   // mevsimin kaçıncı günü (kesirli)
            bool winter = seasonIndex == (int)Season.Winter;

            r.InWinter = winter;
            r.WinterDaysLeft = winter ? dps - inSeason : dps;
            r.DaysToWinter = winter ? 0 : ((int)Season.Winter - seasonIndex) * dps - inSeason;

            // tüketim
            double hungerPoints = HungerPointsPerDay();
            // en çok doyuran besin ortalaması: stoktaki yiyeceğin ağırlıklı değeri
            r.FoodStock = 0;
            double unitsValue = 0; int units = 0;
            foreach (var kv in _house.InventoryList())
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var def) && def.HasTag("food")) { unitsValue += kv.Value * FoodValue(kv.Key); units += kv.Value; }
            r.FoodStock = units;
            double stockPoints = unitsValue;
            double perUnit = units > 0 ? unitsValue / units : 0;
            r.FoodPerDay = perUnit > 0 ? hungerPoints / perUnit : 0;
            r.FoodDays = hungerPoints > 0 ? stockPoints / hungerPoints : double.PositiveInfinity;

            int heat = HeatSourceCount();
            r.FirewoodStock = FirewoodStock();
            r.FirewoodPerDay = heat / Math.Max(0.01, _heat.BurnDaysPerLog);
            r.FirewoodDays = r.FirewoodPerDay > 0 ? r.FirewoodStock / r.FirewoodPerDay : double.PositiveInfinity;

            // Kış sonuna kadar gerekli (bugünden): kışa kalan + kış süresi (kışın içindeyse kalan kış)
            double horizon = winter ? r.WinterDaysLeft : r.DaysToWinter + dps;
            r.FoodNeededUntilSpring = hungerPoints * horizon / Math.Max(1.0, perUnit > 0 ? perUnit : FoodValue("item.turnip") > 0 ? FoodValue("item.turnip") : 50.0);
            r.FirewoodNeededUntilSpring = r.FirewoodPerDay * (winter ? r.WinterDaysLeft : dps);   // ısıtma yalnızca kışın

            // uyarılar
            if (horizon > 0)
            {
                if (r.FoodDays < horizon)
                {
                    r.Warnings.Add(winter
                        ? $"Yiyecek ilkbahara yetmeyebilir: {Fmt(r.FoodDays)} günlük var, {Fmt(horizon)} gün lazım."
                        : $"Kışı çıkarmak için daha çok yiyecek lazım: şimdilik {Fmt(r.FoodDays)} günlük var.");
                    if (winter && r.FoodDays < 3) r.HasDanger = true;
                }
                double needFire = winter ? r.WinterDaysLeft : dps;
                if (heat > 0 && r.FirewoodDays < needFire && (winter || r.DaysToWinter <= WarnDaysBeforeWinter))
                {
                    r.Warnings.Add(winter
                        ? $"Odun yetmeyebilir: {Fmt(r.FirewoodDays)} günlük var, {Fmt(needFire)} gün lazım."
                        : $"Kışa {Fmt(r.DaysToWinter)} gün var ve odun {Fmt(r.FirewoodDays)} gün yeter; {Fmt(needFire)} gün lazım.");
                    if (winter && r.FirewoodDays < 2) r.HasDanger = true;
                }
                if (heat == 0 && (winter || r.DaysToWinter <= WarnDaysBeforeWinter))
                {
                    r.Warnings.Add("Evde ısı kaynağı yok: kışta donarsınız. Bir ocak kurun.");
                    if (winter) r.HasDanger = true;
                }
            }
            return r;
        }

        private static string Fmt(double d) => double.IsInfinity(d) ? "sınırsız" : Math.Max(0, d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
    }
}
