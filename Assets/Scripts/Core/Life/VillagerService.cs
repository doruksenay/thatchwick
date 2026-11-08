using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    public enum GiftTaste { Love = 0, Like = 1, Neutral = 2, Dislike = 3 }

    /// <summary>Bir köylünün oyun içi durumu (tanım içerikte, durum burada).</summary>
    public sealed class VillagerState
    {
        public string DefId;
        public int CharacterId;
        /// <summary>Romantik yakınlık 0-100; arkadaşlık (FamilyService.Yakınlık) ayrı tutulur.</summary>
        public int Romance;
        public long LastGiftDay = -1;
        public readonly List<string> EventsSeen = new List<string>();
    }

    /// <summary>
    /// Adlı köylüler (Adım 38): demirci, değirmenci, otacı... Her birinin programı, zevkleri ve birkaç olayı vardır.
    /// Hediye (günde bir) arkadaşlığı artırır, uygun biriyse romantik yakınlığı da; evlilik için ikisi de gerekir
    /// (FamilyService.ExtraProposalCheck). Köylülerin Sosyal/Eğlence ihtiyacı azalmaz.
    /// </summary>
    public sealed class VillagerService
    {
        public const int MaxRomance = 100;
        public const string RoleBlacksmith = "blacksmith";

        private readonly ContentDatabase _content;
        private readonly Population _population;
        private readonly GameClock _clock;
        private readonly Household _house;
        private readonly Dictionary<string, VillagerState> _states = new Dictionary<string, VillagerState>(StringComparer.Ordinal);

        public FamilyService Family { get; set; }

        /// <summary>(köylü, olay, ödül metni)</summary>
        public event Action<Character, VillagerEventDef> EventTriggered;
        /// <summary>(köylü, eşya, zevk, ileti)</summary>
        public event Action<Character, ItemDef, GiftTaste, string> GiftGiven;

        public VillagerService(ContentDatabase content, Population population, Household house, GameClock clock)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IEnumerable<VillagerState> States => _states.Values;
        public VillagerState StateOf(string defId) => defId != null && _states.TryGetValue(defId, out var s) ? s : null;

        public VillagerState StateOf(Character c)
        {
            if (c == null) return null;
            foreach (var s in _states.Values) if (s.CharacterId == c.Id) return s;
            return null;
        }

        public bool IsNamed(Character c) => StateOf(c) != null;
        public Character CharacterOf(string defId)
        {
            var s = StateOf(defId);
            return s == null ? null : _population.Find(s.CharacterId);
        }

        public VillagerDef DefOf(Character c)
        {
            var s = StateOf(c);
            return s != null && _content.Villagers.TryGet(s.DefId, out var d) ? d : null;
        }

        /// <summary>İçerikte tanımlı ama henüz oluşturulmamış köylüleri karakter olarak ekler; yeni karakterleri döner.</summary>
        public List<Character> SpawnMissing(Func<string, GridPos> resolve = null)
        {
            var created = new List<Character>();
            foreach (var def in _content.Villagers.All)
            {
                if (_states.ContainsKey(def.Id)) continue;
                var pos = resolve != null ? resolve(def.Home) : new GridPos(0, 0);
                var c = new Character(_population.NextId(), def.Name, pos)
                {
                    Sex = def.Sex,
                    IsVillager = true,
                    CanStarve = false
                };
                c.SetAge(def.Age);
                c.SetTraits(def.Traits);
                _population.Add(c);
                LifeRules.Recompute(c, _content, _population.Village);
                _states[def.Id] = new VillagerState { DefId = def.Id, CharacterId = c.Id };
                created.Add(c);
            }
            return created;
        }

        public bool HasRole(string role)
        {
            foreach (var s in _states.Values)
            {
                if (!_content.Villagers.TryGet(s.DefId, out var d) || d.Role != role) continue;
                var c = _population.Find(s.CharacterId);
                if (c != null && !c.IsDead) return true;
            }
            return false;
        }

        // ---- zevk ve hediye ----

        private static bool Matches(string entry, ItemDef item)
        {
            if (entry == null) return false;
            if (entry.StartsWith("tag:")) return item.HasTag(entry.Substring(4));
            return entry == item.Id;
        }

        public static GiftTaste TasteFor(VillagerDef def, ItemDef item)
        {
            foreach (var e in def.Loves) if (Matches(e, item)) return GiftTaste.Love;
            foreach (var e in def.Dislikes) if (Matches(e, item)) return GiftTaste.Dislike;
            foreach (var e in def.Likes) if (Matches(e, item)) return GiftTaste.Like;
            return GiftTaste.Neutral;
        }

        public static int AffectionFor(GiftTaste taste)
        {
            switch (taste)
            {
                case GiftTaste.Love: return 40;
                case GiftTaste.Like: return 20;
                case GiftTaste.Neutral: return 8;
                default: return -15;
            }
        }

        public static int RomanceFor(GiftTaste taste)
        {
            switch (taste)
            {
                case GiftTaste.Love: return 15;
                case GiftTaste.Like: return 8;
                case GiftTaste.Neutral: return 2;
                default: return 0;
            }
        }

        /// <summary>Romantik ilişkiye uygun mu: ikisi de genç/yetişkin, bekâr, karşı cins, akraba değil.</summary>
        public static bool CanRomance(Character giver, Character villager)
        {
            if (giver == null || villager == null) return false;
            return FamilyService.IsAdultAge(giver) && FamilyService.IsAdultAge(villager)
                && !giver.SpouseId.HasValue && !villager.SpouseId.HasValue
                && giver.Sex != villager.Sex && !FamilyService.AreCloseRelatives(giver, villager);
        }

        public string CanGive(Character giver, string villagerDefId, string itemId)
        {
            var s = StateOf(villagerDefId);
            if (s == null) return "Böyle bir köylü yok.";
            var v = _population.Find(s.CharacterId);
            if (v == null || v.IsDead) return "Köylü artık yok.";
            if (giver == null || giver.IsDead) return "Hediye verecek kimse yok.";
            if (giver.Stage == AgeStage.Baby) return "Bebek hediye veremez.";
            if (!v.IsVillager) return $"{v.Name} artık hanenin bir üyesi.";
            if (!_content.Items.TryGet(itemId, out var item)) return "Böyle bir eşya yok.";
            if (item.Placement != ItemPlacement.Resource) return "Yalnızca kaynak eşyalar hediye edilir.";
            if (_house.Count(itemId) <= 0) return "Envanterde yok.";
            if (s.LastGiftDay == _clock.DayIndex) return $"{v.Name} bugün zaten hediye aldı.";
            if (Family == null) return "Aile sistemi bağlı değil.";
            return null;
        }

        public bool Give(Character giver, string villagerDefId, string itemId, out string message)
        {
            message = CanGive(giver, villagerDefId, itemId);
            if (message != null) return false;
            var s = StateOf(villagerDefId);
            var v = _population.Find(s.CharacterId);
            var def = _content.Villagers.Get(villagerDefId);
            var item = _content.Items.Get(itemId);
            var taste = TasteFor(def, item);

            _house.TryTakeItem(itemId);
            s.LastGiftDay = _clock.DayIndex;
            int aff = Family.Affection(giver.Id, v.Id) + AffectionFor(taste);
            Family.SetAffection(giver.Id, v.Id, aff);
            if (CanRomance(giver, v)) s.Romance = Math.Min(MaxRomance, s.Romance + RomanceFor(taste));
            v.Mood.Add("mood.gift", "Hediye aldı", taste == GiftTaste.Dislike ? -4 : 5, 0.6);
            v.Needs.Restore(NeedType.Fun, taste == GiftTaste.Love ? 20f : taste == GiftTaste.Like ? 10f : 0f);

            switch (taste)
            {
                case GiftTaste.Love: message = $"{v.Name}, {item.DisplayName} hediyesine bayıldı!"; break;
                case GiftTaste.Like: message = $"{v.Name}, {item.DisplayName} hediyesini beğendi."; break;
                case GiftTaste.Dislike: message = $"{v.Name}, {item.DisplayName} hediyesinden hoşlanmadı."; break;
                default: message = $"{v.Name}, {item.DisplayName} hediyesine teşekkür etti."; break;
            }
            GiftGiven?.Invoke(v, item, taste, message);
            return true;
        }

        // ---- evlilik kancası ----

        /// <summary>FamilyService.ExtraProposalCheck: adlı köylüyle evlenmek için romantik yakınlık da 100 olmalı.</summary>
        public string ProposalCheck(Character actor, Character target)
        {
            foreach (var c in new[] { actor, target })
            {
                var s = StateOf(c);
                if (s != null && c.IsVillager && s.Romance < MaxRomance)
                    return $"{c.Name} ile romantik yakınlık yetmiyor ({s.Romance}/{MaxRomance}); hediye verin.";
            }
            return null;
        }

        // ---- program ----

        public const double HomeUntil = 0.20, WorkUntil = 0.70, SquareUntil = 0.88;

        /// <summary>Günün saatine, havaya ve şenliğe göre köylünün bulunacağı adlandırılmış yer ("home:1", "work:smithy", "square").</summary>
        public string LocationFor(VillagerDef def, double dayFraction, WeatherType weather, bool festivalToday)
        {
            if (festivalToday && dayFraction >= 0.4 && dayFraction < SquareUntil + 0.05) return "square";
            if (weather == WeatherType.Storm) return def.Home;
            if (dayFraction < HomeUntil) return def.Home;
            if (dayFraction < WorkUntil) return string.IsNullOrEmpty(def.Work) ? def.Home : def.Work;
            if (dayFraction < SquareUntil && weather != WeatherType.Snow && weather != WeatherType.Rain) return "square";
            return def.Home;
        }

        // ---- olaylar ----

        /// <summary>Hane üyelerinin bu köylüyle en yüksek yakınlığı.</summary>
        public int BestAffection(Character villager)
        {
            if (Family == null || villager == null) return 0;
            int best = 0;
            foreach (var m in _population.Members)
            {
                if (m.IsVillager || m.IsDead || m == villager) continue;
                best = Math.Max(best, Family.Affection(m.Id, villager.Id));
            }
            return best;
        }

        /// <summary>Yakınlık eşiğine ulaşan, henüz yaşanmamış olayları yaşatır (ödül haneye); yaşananları döner.</summary>
        public List<(Character villager, VillagerEventDef ev)> PollEvents()
        {
            var happened = new List<(Character, VillagerEventDef)>();
            foreach (var s in _states.Values)
            {
                var c = _population.Find(s.CharacterId);
                if (c == null || c.IsDead || !_content.Villagers.TryGet(s.DefId, out var def)) continue;
                int aff = BestAffection(c);
                foreach (var e in def.Events)
                {
                    if (s.EventsSeen.Contains(e.Id) || aff < e.MinAffection) continue;
                    s.EventsSeen.Add(e.Id);
                    if (e.RewardAkce > 0) _house.AddAkce(e.RewardAkce);
                    if (!string.IsNullOrEmpty(e.RewardItem)) _house.AddItem(e.RewardItem, Math.Max(1, e.RewardCount));
                    happened.Add((c, e));
                    EventTriggered?.Invoke(c, e);
                }
            }
            return happened;
        }

        // ---- kayıt ----

        public List<VillagerSave> Export()
        {
            var list = new List<VillagerSave>();
            foreach (var s in _states.Values)
                list.Add(new VillagerSave
                {
                    id = s.DefId, characterId = s.CharacterId, romance = s.Romance, lastGiftDay = s.LastGiftDay,
                    events = s.EventsSeen.ToArray()
                });
            list.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
            return list;
        }

        public void Restore(IEnumerable<VillagerSave> saved)
        {
            _states.Clear();
            if (saved == null) return;
            foreach (var v in saved)
            {
                if (v == null || !_content.Villagers.Contains(v.id)) continue;
                var s = new VillagerState { DefId = v.id, CharacterId = v.characterId, Romance = Math.Max(0, Math.Min(MaxRomance, v.romance)), LastGiftDay = v.lastGiftDay };
                if (v.events != null) s.EventsSeen.AddRange(v.events);
                _states[v.id] = s;
            }
        }
    }
}
