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
        public const string RoleInnkeeper = "innkeeper";
        public const double InnOpens = 0.48, InnCloses = 0.995;

        private readonly ContentDatabase _content;
        private readonly Population _population;
        private readonly GameClock _clock;
        private readonly Household _house;
        private readonly Dictionary<string, VillagerState> _states = new Dictionary<string, VillagerState>(StringComparer.Ordinal);

        public FamilyService Family { get; set; }
        /// <summary>İlişki sistemi (67d): romantik çift başına burada, olaylar çifte bağlı.</summary>
        public Thatchwick.Core.Relations.RelationService Relations { get; set; }

        private static bool CanRomanceIgnoringStatus(Character a, Character b) =>
            a != null && b != null && FamilyService.IsAdultAge(a) && FamilyService.IsAdultAge(b) && a.Sex != b.Sex && !FamilyService.AreCloseRelatives(a, b);

        /// <summary>(köylü, olay, ödül metni)</summary>
        public event Action<Character, VillagerEventDef> EventTriggered;
        /// <summary>(köylü, eşya, zevk, ileti)</summary>
        public event Action<Character, ItemDef, GiftTaste, string> GiftGiven;

        private readonly Random _rng;

        /// <summary>Adlandırılmış yeri (home:1, work:smithy ...) karoya çeviren oyun katmanı işlevi; halef buna göre yerleşir.</summary>
        public Func<string, GridPos> PlaceResolver { get; set; }
        /// <summary>Hediye verenin gönül kazanma oranı (küpe: basamak başına %3); yakınlık ve romantik kazanç buna göre artar.</summary>
        public Func<Character, double> CourtBonus { get; set; }
        /// <summary>
        /// Ölen adlı köylünün dükkânını devralacak varis (komşu hane, Adım 55): varsa dışarıdan halef gelmez.
        /// Null ise ya da null dönerse eski düzen: yeni bir halef köye yerleşir.
        /// </summary>
        public Func<VillagerDef, Character, Character> Inheritor { get; set; }
        /// <summary>Dükkân varise geçti (varis, tanım).</summary>
        public event Action<Character, VillagerDef> RoleInherited;
        /// <summary>Ölen adlı köylünün yerine halefi geldi (karakter, tanım): oyun katmanı görünüm ve sahne nesnesi verir.</summary>
        public event Action<Character, VillagerDef> SuccessorArrived;
        /// <summary>Halef bu yaş aralığında (oyun günü) köye gelir: genç yetişkin bir usta.</summary>
        public const double SuccessorAgeMin = 30.5, SuccessorAgeMax = 40;   // Genç yetişkin

        public VillagerService(ContentDatabase content, Population population, Household house, GameClock clock, int seed = 55)
        {
            _rng = new Random(seed);
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>İçerik (köylü tanımları).</summary>
        public ContentDatabase Content => _content;

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
            if (resolve != null && PlaceResolver == null) PlaceResolver = resolve;
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

        /// <summary>
        /// Ölen (ya da toplulukta artık bulunmayan) adlı köylülerin yerine aynı işi sürdüren halef getirir: demirci ölünce
        /// alet yükseltme, değirmenci ölünce o köylünün hediye ve olayları kalıcı olarak kapanmasın. Halef yeni bir kişidir:
        /// dostluk ve romantik yakınlık sıfırdan başlar; yaşanmış olaylar (ödüller) yeniden verilmez. Gelenleri döner.
        /// </summary>
        public List<Character> ReplaceDeparted(bool inheritOnly = false)
        {
            var arrived = new List<Character>();
            foreach (var def in _content.Villagers.All)
            {
                if (!_states.TryGetValue(def.Id, out var state)) continue;      // hiç oluşmamış: SpawnMissing'in işi
                var old = _population.Find(state.CharacterId);
                if (old != null && !old.IsDead) continue;                        // yaşıyor (haneye katılmış olsa da işini sürdürür)

                // Önce hanesinden yetişkin bir varis (çocuğu, eşi ...) dükkânı devralır.
                var heir = Inheritor != null ? Inheritor(def, old) : null;
                if (heir != null && !heir.IsDead && heir.IsVillager)
                {
                    state.CharacterId = heir.Id;
                    state.Romance = 0;
                    state.LastGiftDay = -1;
                    RoleInherited?.Invoke(heir, def);
                    continue;
                }
                if (inheritOnly) continue;

                string name = _population.NameFor(def.Sex, def.Id);
                if (string.IsNullOrWhiteSpace(name) || (old != null && name == old.Name)) name = def.Name;
                var pos = PlaceResolver != null ? PlaceResolver(def.Home) : (old != null ? old.Position : new GridPos(0, 0));
                var c = new Character(_population.NextId(), name, pos)
                {
                    Sex = def.Sex,
                    IsVillager = true,
                    CanStarve = false
                };
                c.SetAge(SuccessorAgeMin + _rng.NextDouble() * (SuccessorAgeMax - SuccessorAgeMin));
                c.SetTraits(def.Traits);
                c.LifespanDays = c.AgeConfig.RollLifespan(_rng) + LifeRules.LifespanBonus(_content, c.TraitIds);
                _population.Add(c);
                LifeRules.Recompute(c, _content, _population.Village);

                state.CharacterId = c.Id;
                state.Romance = 0;
                state.LastGiftDay = -1;
                arrived.Add(c);
                SuccessorArrived?.Invoke(c, def);
            }
            return arrived;
        }

        /// <summary>Adlı köylünün rolü; adlı köylü değilse null.</summary>
        public string RoleOf(Character c)
        {
            if (c == null) return null;
            foreach (var s in _states.Values)
                if (s.CharacterId == c.Id && _content.Villagers.TryGet(s.DefId, out var d)) return d.Role;
            return null;
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
            if (_house.Count(itemId) <= 0) return Localization.CoreText.T("ui.common.not_held");
            if (s.LastGiftDay == _clock.DayIndex) return $"{v.Name} bugün zaten hediye aldı.";
            if (Family == null) return "Aile sistemi bağlı değil.";
            return null;
        }

        /// <summary>Son hediyenin kalitesi (53): sevilen eşyada yakınlık ×1 / 1,25 / 1,5 / 2.</summary>
        public int LastGiftQuality { get; private set; }

        public bool Give(Character giver, string villagerDefId, string itemId, out string message)
        {
            message = CanGive(giver, villagerDefId, itemId);
            if (message != null) return false;
            var s = StateOf(villagerDefId);
            var v = _population.Find(s.CharacterId);
            var def = _content.Villagers.Get(villagerDefId);
            var item = _content.Items.Get(itemId);
            var taste = TasteFor(def, item);

            // Hediye (53 §4.7): sevilen ya da beğenilen eşyada en iyisi verilir ve yakınlık kaliteyle artar; ötekinde en düşüğü.
            bool liked = taste == GiftTaste.Love || taste == GiftTaste.Like;
            _house.TryTake(itemId, liked ? TakePolicy.Highest : TakePolicy.Lowest, out var given);
            LastGiftQuality = item.HasQuality ? given.Quality : 0;
            s.LastGiftDay = _clock.DayIndex;
            int gain = AffectionFor(taste);
            if (liked) gain = (int)Math.Round(gain * ItemQuality.GiftFactor(LastGiftQuality));
            double court = liked && CourtBonus != null ? Math.Max(0.0, CourtBonus(giver)) : 0.0;
            if (court > 0) gain = (int)Math.Ceiling(gain * (1.0 + court));
            int aff = Family.Affection(giver.Id, v.Id) + gain;
            Family.SetAffection(giver.Id, v.Id, aff);
            // Romantik çift başına (67 H1): ilişki sistemi bağlıysa hediyeyi veren ile köylü arasında tutulur.
            int romance = (int)Math.Ceiling(RomanceFor(taste) * (1.0 + court));
            if (Relations != null)
            {
                if (taste == GiftTaste.Dislike) Relations.AddRomance(giver.Id, v.Id, -3);
                else if (CanRomance(giver, v)) Relations.AddRomance(giver.Id, v.Id, romance);
                Relations.Met(giver.Id, v.Id);
                Relations.Council?.OnGift(giver, v, liked);
                // Eşi ya da nişanlısı varken başkasına sevilen hediye kıskançlık doğurabilir (Stardew; 67c T9).
                if (liked && Relations.PartnerOf(giver) != null && Relations.PartnerOf(giver) != v && CanRomanceIgnoringStatus(giver, v)) Relations.ReportTrigger(Thatchwick.Core.Relations.SpouseTrigger.Flirting, giver, Relations.PartnerNear(giver));
            }
            else if (CanRomance(giver, v)) s.Romance = Math.Min(MaxRomance, s.Romance + romance);
            v.AddMoodlet("mood.gift", "Hediye aldı", taste == GiftTaste.Dislike ? -4 : 5, 0.6);
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
            if (Relations != null) return Relations.ProposalCheck(actor, target);
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

        /// <summary>
        /// Mesleğe göre yaban haritası gezintisi (rol, yer, başlangıç, bitiş; gün kesri). Otacı sabah ormanda ot toplar,
        /// marangoz kereste seçer, taşçı ocağa uğrar (dükkân açılmadan önce); balıkçı dükkânı kapattıktan sonra göle çıkar.
        /// Yer bu haritada yoksa (eski kayıt) PlaceAvailable false döner ve köylü normal programına uyar.
        /// </summary>
        public static readonly (string role, string place, double from, double until)[] Outings =
        {
            ("herbalist", "forest", 0.20, 0.33),
            ("carpenter", "forest", 0.20, 0.33),
            ("mason", "hill", 0.20, 0.29),
            ("fisher", "lake", 0.59, 0.70),
            ("shepherd", "slopes", 0.59, 0.70),
        };

        /// <summary>Adlandırılmış yerin bu dünyada olup olmadığı (oyun katmanı verir); null ise gezinti yok.</summary>
        public Func<string, bool> PlaceAvailable { get; set; }

        /// <summary>Günün saatine, havaya ve şenliğe göre köylünün bulunacağı adlandırılmış yer ("home:1", "work:smithy", "square").</summary>
        public string LocationFor(VillagerDef def, double dayFraction, WeatherType weather, bool festivalToday)
        {
            if (festivalToday && dayFraction >= 0.4 && dayFraction < SquareUntil + 0.05) return "square";
            if (weather == WeatherType.Storm) return def.Home;
            // Hancı akşam çalışır (meyhane 12:00-24:00, belge 42): öğlene kadar evde, sonra gece yarısına kadar meyhanede.
            if (def.Role == RoleInnkeeper)
                return dayFraction >= InnOpens && dayFraction < InnCloses && !string.IsNullOrEmpty(def.Work) ? def.Work : def.Home;
            if (dayFraction < HomeUntil) return def.Home;
            if (weather != WeatherType.Snow && weather != WeatherType.Rain && PlaceAvailable != null)
                foreach (var o in Outings)
                    if (o.role == def.Role && dayFraction >= o.from && dayFraction < o.until && PlaceAvailable(o.place)) return o.place;
            if (dayFraction < WorkUntil) return string.IsNullOrEmpty(def.Work) ? def.Home : def.Work;
            if (dayFraction < SquareUntil && weather != WeatherType.Snow && weather != WeatherType.Rain) return "square";
            return def.Home;
        }

        // ---- olaylar ----

        /// <summary>Olay yaşandı (köylü, olayı yaşayan hane üyesi, olay): 67 H3.</summary>
        public event Action<Character, Character, VillagerEventDef> EventTriggeredFor;

        /// <summary>Bu köylüyle yakınlığı en yüksek hane üyesi; yoksa null.</summary>
        public Character BestFriendOf(Character villager)
        {
            if (Family == null || villager == null) return null;
            Character best = null; int bestAff = int.MinValue;
            foreach (var m in _population.Members)
            {
                if (m.IsVillager || m.IsDead || m == villager) continue;
                int a = Family.Affection(m.Id, villager.Id);
                if (a > bestAff) { best = m; bestAff = a; }
            }
            return best;
        }

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
                var friend = BestFriendOf(c);
                foreach (var e in def.Events)
                {
                    if (s.EventsSeen.Contains(e.Id) || aff < e.MinAffection) continue;
                    s.EventsSeen.Add(e.Id);
                    if (e.RewardAkce > 0) _house.AddAkce(e.RewardAkce);
                    if (!string.IsNullOrEmpty(e.RewardItem)) _house.AddItem(e.RewardItem, Math.Max(1, e.RewardCount));
                    happened.Add((c, e));
                    EventTriggered?.Invoke(c, e);
                    // Olay, yakınlığı eşiği geçen hane üyesine bağlanır (67 H3): bildirim onun adıyla.
                    if (friend != null) EventTriggeredFor?.Invoke(c, friend, e);
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
