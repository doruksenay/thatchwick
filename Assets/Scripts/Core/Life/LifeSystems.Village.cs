using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Yaşayan köy, parti 1 (Project belgeleri 66d, 66e, 66f): köylülerin ihtiyaçları, kilerleri, Pazar ve dükkân stoğu.</summary>
    public sealed partial class LifeSystems
    {
        /// <summary>Köy hayatı: kiler, öğün, alım, Pazar, bunalım ve teselli, ikram, iş döngüsü.</summary>
        public LivingVillage Living { get; private set; }

        /// <summary>Dükkânların günlük stoğu (oyuncu ve köylü alımı aynı stoktan).</summary>
        public ShopStock ShopStock => Living.Stock;

        private void WireLivingVillage(int seed)
        {
            Living = new LivingVillage(Content, Population, Villagers, Homes, Clock, new ShopStock(Content), seed != 0 ? Seeds.Derive(seed, 66) : 66)
            {
                Weather = () => Weather.Today(Clock.TotalDays),
                FestivalToday = () => Festivals.TodaysFestival() != null,
                TreePlace = c => Trees.VillagerPlace(c, Clock.DayFraction, Weather.Today(Clock.TotalDays)),
                Charity = Trees.Kitchen,
                Requests = Requests,
                RecentDeaths = RecentGraves,
                Text = t => Text(t)
            };
            Living.Notice += (text, id) => Say(text, NoticeLevel.Info, id);
            Living.Bubble += (c, t) => Bubble?.Invoke(c, t);
            Breakdowns.VillagerDays = () => (Content.VillageLife.BreakdownMinDays, Content.VillageLife.BreakdownMaxDays);
            Breakdowns.Started += (c, kind) => { if (c.IsVillager) Living.OnBreakdownStarted(c); };
            Population.MemberDied += c =>
            {
                Living.OnDeath(c);
                if (c.IsVillager && Villagers.IsNamed(c)) _replaceDeparted = true;   // halef bir sonraki adımda (ölüm döngüsünün dışında)
            };
            Villagers.FamilyMemberArrived += (member, head) =>
            {
                if (member.SpouseId == head.Id) Say(LocText.Of("village.n.family_arrived", ("name", head.Name), ("spouse", member.Name)), NoticeLevel.Info, head.Id);
            };
            Population.BabyBorn += (baby, mother, father) => Living.OnBirth(baby, mother);
            Living.Import(null);   // yeni oyun: kilerler dolu başlar (kayıt yüklenirse Restore yeniden kurar)
        }

        /// <summary>Pazar (genel dükkân) alımları da günün stoğundan düşer; kasvetli tüccar pazarlığa kapalı.</summary>
        private void AttachVillageMarket(Market market)
        {
            market.Stock = Living.Stock;
            market.HaggleBlocked = () =>
            {
                var refusal = Living.HaggleRefusal(Living.OwnerOf(Shops.Get("general")));
                return refusal.HasValue ? Text(refusal.Value) : null;
            };
        }

        private bool _replaceDeparted;

        private void VillageTick(double gameDays)
        {
            if (_replaceDeparted)
            {
                _replaceDeparted = false;
                // Ölen adlı köylünün dükkânı boş kalmaz (belge 66g karar 1): önce hanesinden varis, yoksa dışarıdan halef,
                // tek başına ya da ailesiyle hemen taşınır.
                if (Villagers.ReplaceDeparted().Count > 0) { Houses.EnsureHouses(); Homes.AssignAll(); }
            }
            Living.Tick(gameDays);
        }

        private IEnumerable<(int id, string name, double day)> RecentGraves()
        {
            foreach (var g in Graves.Graves) yield return (g.CharacterId, g.Name, g.DeathDay);
        }

        /// <summary>Aile hizmetleri kurulunca (SetFamily) köy hayatı da yakınlığa bağlanır.</summary>
        internal void AttachVillageFamily(Thatchwick.Core.Family.FamilyService family)
        {
            if (Living != null) Living.Family = family;
        }
    }
}
