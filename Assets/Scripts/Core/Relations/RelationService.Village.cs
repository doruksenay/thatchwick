using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>
    /// Köyün kendi sosyal ağı (67 §P4, karar 67a-10): köylüler yan yana gelince kendi aralarında konuşur, arkadaşlık ve gönül
    /// kurar, haber taşır; bekâr köylüler sevgili olur, nişanlanır ve evlenir (oyuncu düğüne davet edilir). Düşman köylüler
    /// arada bir tartışır ya da kavga eder.
    /// </summary>
    public sealed partial class RelationService
    {
        public const double VillageStepDays = 0.05;
        public const double VillageChatChance = 0.15, VillageQuarrelChance = 0.03;
        private double _villageAcc;

        /// <summary>Gün içinde arada bir (yaklaşık 70 oyun dakikasında bir): yakın duran köylü çiftleri konuşur.</summary>
        public void VillageTick(double gameDays)
        {
            _villageAcc += gameDays;
            if (_villageAcc < VillageStepDays) return;
            _villageAcc = 0;
            var people = new List<Character>();
            foreach (var c in _population.Members)
                if (!c.IsDead && c.IsVillager && c.Stage >= AgeStage.Child) people.Add(c);
            for (int i = 0; i < people.Count; i++)
                for (int j = i + 1; j < people.Count; j++)
                {
                    var a = people[i]; var b = people[j];
                    if (a.Position.Z != b.Position.Z || Math.Abs(a.Position.X - b.Position.X) + Math.Abs(a.Position.Y - b.Position.Y) > 3) continue;
                    if (Chats != null && (Chats.CircleOf(a.Id) != null || Chats.CircleOf(b.Id) != null)) continue;
                    if (!Pair(a.Id, b.Id).Introduced) Introduce(a, b);
                    if (IsSulking(a.Id, b.Id)) continue;
                    int f = Friendship(a.Id, b.Id);
                    if (f <= -20 && _rng.NextDouble() < VillageQuarrelChance * RelationRules.Temper(a))
                    {
                        if (CanFight(a, b) == null && f <= -60 && _rng.NextDouble() < 0.3 && Command != null) Command(a, Job.Social(b.Id, SocialAction.Fight));
                        else if (Command != null) Command(a, Job.Social(b.Id, SocialAction.Argue));
                        continue;
                    }
                    if (_rng.NextDouble() >= VillageChatChance) continue;
                    double g = GainFactor(a, b);
                    var p = Pair(a.Id, b.Id);
                    if (p.ChatDay != (long)Math.Floor(Today())) { p.ChatDay = (long)Math.Floor(Today()); p.ChatsToday = 0; }
                    if (++p.ChatsToday > 2) continue;
                    AddFriendship(a.Id, b.Id, (int)Math.Max(1, Math.Round(8 * g)));
                    Met(a.Id, b.Id);
                    if (CanRomance(a, b) && PartnerOf(a) == null && PartnerOf(b) == null && !a.SpouseId.HasValue && !b.SpouseId.HasValue
                        && RelationRules.Compatibility(a, b) >= 0)
                        AddRomance(a.Id, b.Id, 2 + _rng.Next(3));
                    // Haber taşırlar.
                    if (Knowledge != null && _rng.NextDouble() < 0.5 * RelationRules.Gossipy(a)) Knowledge.TellBest(a, b, allowRumor: true, quiet: true);
                }
        }

        /// <summary>Köylü çiftlerin gönül yolu (gün başında): sevgili olma, isteme (sahnesiz), nişan.</summary>
        private void VillageNewDay(long day, double now)
        {
            foreach (var p in new List<PairState>(_pairs.Values))
            {
                var a = _population.Find(p.A); var b = _population.Find(p.B);
                if (a == null || b == null || !a.IsVillager || !b.IsVillager || a.IsDead || b.IsDead) continue;
                if (a.SpouseId.HasValue || b.SpouseId.HasValue || !CanRomance(a, b)) continue;
                if (p.Status == CoupleStatus.None && p.Romance >= SweetheartRomance && Friendship(a.Id, b.Id) >= SweetheartFriend
                    && PartnerOf(a) == null && PartnerOf(b) == null && !CourtedByHousehold(a) && !CourtedByHousehold(b))
                {
                    p.Status = CoupleStatus.Sweetheart; p.StatusDay = now;
                    Knowledge?.Record(FactKind.Sweethearts, a, b, RepFacet.Word, 0, null);
                }
                else if (p.Status == CoupleStatus.Sweetheart && now - p.StatusDay >= AskSweetheartDays && p.Romance >= AskRomance)
                {
                    bool busy = false;
                    foreach (var e in _events) if (!e.Done && IsOf(e, a.Id, b.Id)) busy = true;
                    if (!busy) EngageVillagers(a, b);
                }
            }
        }

        /// <summary>Hanenin bir üyesi bu köylüye kur yapıyor mu (romantik 15+): köylü başkasıyla evlendirilmez.</summary>
        public bool CourtedByHousehold(Character villager)
        {
            if (villager == null) return false;
            foreach (var p in _pairs.Values)
            {
                if (p.A != villager.Id && p.B != villager.Id) continue;
                var o = _population.Find(p.A == villager.Id ? p.B : p.A);
                if (o != null && !o.IsDead && !o.IsVillager && p.Romance >= 15) return true;
            }
            return false;
        }
    }
}
