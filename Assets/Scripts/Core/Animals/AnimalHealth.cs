using System;
using System.Collections.Generic;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    // Adım 42 kalanı (18.11.2026): hayvan hastalığı ve kış etkisi.
    // Dönem notu: ortaçağ köylüsü hasta hayvanı otla, merhemle ve iyi bakımla tedavi ederdi; salgın sürüye yayılırdı.
    public sealed partial class AnimalSystem
    {
        // ---------- kış ----------

        /// <summary>Barınak sıcaklığı bunun altındaysa hayvan üşür (°C).</summary>
        public const float ColdThreshold = 0f;
        /// <summary>Kışın yem ihtiyacı bu kadar artar (ısınmak için daha çok yer; otlak da büyümez).</summary>
        public const double WinterFeedMultiplier = 1.25;
        /// <summary>Üşüyen hayvanın ürün hızı çarpanı.</summary>
        public const double ColdOutputFactor = 0.6;
        /// <summary>Üşüyen hayvanın dostluğu artmaz; günde bu kadar azalır.</summary>
        public const float ColdFriendLoss = 1.5f;

        // ---------- hastalık ----------

        /// <summary>Günlük temel hastalanma şansı (iyi bakılan hayvan).</summary>
        public const double SickBaseChance = 0.002;
        /// <summary>Risk ekleri: üşüme, açlık, saman altlıksız barınak, dar barınak, aynı barınaktaki her hasta.</summary>
        public const double SickColdRisk = 0.03, SickHungerRisk = 0.02, SickNoBeddingRisk = 0.008, SickCrowdedRisk = 0.008, SickContagionRisk = 0.05;
        /// <summary>Tedavisiz bu kadar gün hasta kalırsa ölür.</summary>
        public const double SickDeathDays = 8.0;
        /// <summary>Kendiliğinden iyileşme: konforlu barınakta (60+) daha olası.</summary>
        public const double RecoverChanceGood = 0.25, RecoverChancePoor = 0.06;
        public const float RecoverComfort = 60f;

        /// <summary>Hastalık açık mı (varsayılan açık). Kapalıyken üşüme yine işler ama kimse hastalanmaz.</summary>
        public bool DiseaseEnabled { get; set; } = true;

        // Sağlık zarları ayrı rastgele kaynaktan: yavrulama ve gezinme sırası hastalıktan etkilenmez.
        private Random _healthRng;
        private Random HealthRng => _healthRng ?? (_healthRng = new Random(_healthSeed));
        private int _healthSeed = 4242;

        /// <summary>Sağlık zarlarının tohumu (oyun tohumundan türetilir; testlerde sabit).</summary>
        public int HealthSeed { set { _healthSeed = value; _healthRng = null; } }

        /// <summary>Bir hayvan hastalandı (bildirim için).</summary>
        public event Action<Animal> FellSick;
        /// <summary>Hasta hayvan iyileşti (kendiliğinden ya da merhemle).</summary>
        public event Action<Animal> Recovered;

        /// <summary>Kışın yem çarpanı (barınağı ne olursa olsun; ısınmak enerji ister).</summary>
        internal double WinterFeedFactor(Animal a) =>
            SeasonNow != null && SeasonNow() == Season.Winter ? WinterFeedMultiplier : 1.0;

        /// <summary>
        /// Hayvan üşüyor mu: barınak sıcaklığı eşiğin altında. Isı sistemi bağlı değilse kışın kapalı oda
        /// olmayan (hazır dış) barınak soğuk sayılır.
        /// </summary>
        public bool IsColdAt(ItemInstance home, Season season)
        {
            if (home == null) return season == Season.Winter;
            if (Temperature != null) return Temperature(home.Position) < ColdThreshold;
            return season == Season.Winter && RoomOf(home) == null;
        }

        /// <summary>Hastalık riskini artıran hayvancılık ustalığı indirimi: seviye başına %5 (10. seviyede %45 az).</summary>
        public static double HusbandryRiskFactor(int level) => Math.Max(0.55, 1.0 - 0.05 * Math.Max(0, level - 1));

        /// <summary>Bu hayvanın bugünkü hastalanma şansı (sağlıklıysa).</summary>
        /// <summary>Ek hastalık çarpanı (belge 45: Hekimlik 8 "salgın önleme" ×0,5); null ise 1.</summary>
        public Func<double> ExtraRiskFactor { get; set; }

        public double SickChance(Animal a, int sickNeighbours, int husbandryLevel)
        {
            double p = SickBaseChance;
            if (a.IsCold) p += SickColdRisk;
            if (a.Fullness < 20f) p += SickHungerRisk;
            var home = _world.GetItem(a.HomeItemId);
            if (home != null)
            {
                var r = ComfortOf(home);
                if (r.IsRoom && r.Bedding <= 0f) p += SickNoBeddingRisk;
                if (r.IsRoom && r.Space < 10f) p += SickCrowdedRisk;
            }
            p += SickContagionRisk * sickNeighbours;
            return Math.Min(0.6, p * HusbandryRiskFactor(husbandryLevel) * (ExtraRiskFactor != null ? ExtraRiskFactor() : 1.0));
        }

        /// <summary>Hastalığı başlatır (test ve olaylar için de).</summary>
        public void MakeSick(Animal a)
        {
            if (a == null || a.IsDead || a.IsSick) return;
            a.IsSick = true;
            a.SickDays = 0;
            FellSick?.Invoke(a);
        }

        /// <summary>Hayvanı iyileştirir.</summary>
        public void Cure(Animal a)
        {
            if (a == null || !a.IsSick) return;
            a.IsSick = false;
            a.SickDays = 0;
            Recovered?.Invoke(a);
        }

        // Gün başı (OnNewDay): üşüme, hastalığın ilerlemesi, kendiliğinden iyileşme, bulaşma ve hastalıktan ölüm.
        // Önce bugünkü hasta sayıları alınır (bulaşma dünkü hastalardan), sonra yeni hastalar belirlenir.
        private void UpdateHealth(Season season, DayReport report)
        {
            int husbandry = Math.Max(1, HusbandryLevel?.Invoke() ?? 1);
            var sickByHome = new Dictionary<int, int>();
            foreach (var a in _animals)
                if (!a.IsDead && a.IsSick) sickByHome[a.HomeItemId] = (sickByHome.TryGetValue(a.HomeItemId, out var n) ? n : 0) + 1;

            foreach (var a in new List<Animal>(_animals))
            {
                if (a.IsDead) continue;
                var home = _world.GetItem(a.HomeItemId);
                a.IsCold = IsColdAt(home, season);
                a.ColdDays = a.IsCold ? a.ColdDays + 1 : 0;
                if (a.IsCold) report.Cold++;

                if (a.IsSick)
                {
                    a.SickDays += 1.0;
                    if (a.SickDays >= SickDeathDays) { Kill(a, AnimalDeath.Sickness); report.DiedOfSickness++; continue; }
                    double recover = a.Comfort >= RecoverComfort && !a.IsCold ? RecoverChanceGood : RecoverChancePoor;
                    if (HealthRng.NextDouble() < recover) { Cure(a); report.Recovered++; }
                    continue;
                }

                if (!DiseaseEnabled) continue;
                int neighbours = sickByHome.TryGetValue(a.HomeItemId, out var s) ? s : 0;
                if (HealthRng.NextDouble() < SickChance(a, neighbours, husbandry)) { MakeSick(a); report.Sickened++; }
            }
        }
    }
}
