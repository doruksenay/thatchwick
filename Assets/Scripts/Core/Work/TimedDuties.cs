using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Work
{
    /// <summary>Süreli görevin türü (belge 47 §3.2).</summary>
    public enum TimedDutyKind
    {
        None = 0,
        Mine = 1,
        Fish = 2,
        Forest = 3,
        Forage = 4,
        Traps = 5,
        Clay = 6,
        /// <summary>Köy işi (istek çözüm yolu: çatı onarımı, nöbet, arama). Ürünsüz; bitince isteğe bildirilir.</summary>
        Errand = 7
    }

    /// <summary>Görev nasıl bitti.</summary>
    public enum TimedDutyEnd
    {
        Running = 0,
        Done = 1,
        Target = 2,
        Hungry = 3,
        Tired = 4,
        Cold = 5,
        Recalled = 6,
        Danger = 7,
        Died = 8
    }

    /// <summary>Gidilebilecek yer: tür, gereken beceri ve en düşük seviye.</summary>
    public sealed class TimedDutyPlace
    {
        public string Id;
        public TimedDutyKind Kind;
        public SkillType Skill;
        public int MinLevel = 1;
        /// <summary>Gereken alet (ToolService kimliği); null ise yok.</summary>
        public string Tool;
        /// <summary>Envanterde bulunması gereken eşya (tuzak); null ise yok.</summary>
        public string NeedsItem;
        /// <summary>Elle oynanınca 1. seviye ve temel aletle saatte ortalama toplam ürün.</summary>
        public double BaseRate;
        /// <summary>Maden içi: dış sıcaklıktan bağımsız sabit ~10 °C.</summary>
        public bool Underground;
        /// <summary>Çocuk da gidebilir (yakın bölge).</summary>
        public bool ChildSafe;
    }

    /// <summary>Oyuncunun verdiği görev emri.</summary>
    public sealed class TimedDutyOrder
    {
        public int CharacterId;
        public TimedDutyKind Kind;
        public string PlaceId;
        /// <summary>Çalışma süresi (oyun saati); 0 ya da altı ise akşama (19.00) kadar.</summary>
        public double Hours;
        /// <summary>Hedef ürün ve adedi: hedef dolunca karakter erken döner (istek "Ata", belge 47 §3.7).</summary>
        public string TargetItem;
        public int TargetCount;
        /// <summary>Her gün aynı saatte tekrarla (sürekli göreve dönüşür).</summary>
        public bool Repeat;
        /// <summary>Bağlı istek (RequestService); 0 ise yok.</summary>
        public int RequestId;
        /// <summary>Köy işi (Errand) için görünen ad (anahtarlı; istek başlığı).</summary>
        public LocText Label;
    }

    /// <summary>Süren görev.</summary>
    public sealed class TimedDuty
    {
        public int Id;
        public TimedDutyOrder Order;
        public double StartDay;
        public double PlannedHours;
        public double WorkedHours;
        public double BreakHours;
        public double PlayedHours;
        public bool Playing;
        public World.GridPos StartPos;
        public readonly Dictionary<string, double> Gains = new Dictionary<string, double>();
        /// <summary>Azık: anahtar <see cref="Content.ItemKey.Token"/> ("item.bread", "item.bread#2!"), değer adet. Kalite ve kaynak korunur.</summary>
        public readonly Dictionary<string, int> Packed = new Dictionary<string, int>();
        public readonly List<LocText> Events = new List<LocText>();
        public double Xp;
        /// <summary>Maden görevinde kazılmayı bekleyen karo kesri (gerçek karolar tüketilir; belge 58b karar 2).</summary>
        public double MineTiles;
        public TimedDutyEnd End;
        public double ElapsedHours => WorkedHours + BreakHours + PlayedHours;
    }

    /// <summary>Dönüş kartı.</summary>
    public sealed class TimedDutyResult
    {
        public int CharacterId;
        public string CharacterName;
        public TimedDutyKind Kind;
        public string PlaceId;
        public TimedDutyEnd End;
        public readonly List<KeyValuePair<string, int>> Items = new List<KeyValuePair<string, int>>();
        public double Xp;
        public readonly List<LocText> Notes = new List<LocText>();
        public int RequestId;
        public double Hours;
        public LocText Summary;
    }

    /// <summary>
    /// Süreli görev (belge 47 §3): bir aile üyesi belli bir süre için madene, balığa, ormana, toplamaya, tuzak işine ya da kil
    /// toplamaya gönderilir. Sonuç formülle hesaplanır: elle saatlik verim × oran (taban %30, beceri seviyesi başına +%1,5, alet
    /// kademesi başına +%2, ocakbaşı/kâhya +%3, en çok %45; karar 0-7). Görevdeki karakter azığından yer; aç, yorgun ya da üşümüş
    /// olursa erken döner. Çocuk sıkılınca işi bırakıp oyuncağıyla oynar (§7.1). UnityEngine yok; test edilebilir.
    /// </summary>
    public sealed class TimedDutyService
    {
        public const double BaseEfficiency = 0.30;
        public const double PerSkillLevel = 0.015;
        public const double PerToolTier = 0.02;
        public const double LeaderBonus = 0.03;
        public const double MaxEfficiency = 0.45;
        /// <summary>Görevde kazanılan XP elle kazanılacağın bu kadarı.</summary>
        public const double XpShare = 0.5;
        /// <summary>Elle bir saatlik işin XP'si (beceri işi).</summary>
        public const double ManualXpPerHour = 30.0;
        public const double RareFactor = 0.5;
        public const double EatBelow = 30.0, HungryReturn = 15.0, TiredReturn = 15.0, ColdReturn = 25.0;
        public const double MealBreakHours = 0.25;
        public const double MineTemperature = 10.0;
        public const double EveningHour = 19.0;
        public const double StormYield = 0.7;
        public const double ChildFunFactor = 2.0, TeenFunFactor = 1.3;
        public const double BoredBelow = 40.0, BoredChancePerHour = 0.35;
        public const double CaveInPerHour = 0.002;   // elle oynamanın yarısı (riskli karoya girmez)
        public const double WolfPerHour = 0.02, BearPerHour = 0.004;

        private readonly Population _population;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly List<TimedDuty> _active = new List<TimedDuty>();
        private readonly List<TimedDutyOrder> _repeats = new List<TimedDutyOrder>();
        private readonly Dictionary<int, double> _repeatStartFraction = new Dictionary<int, double>();
        private readonly Dictionary<int, long> _repeatLastDay = new Dictionary<int, long>();
        private int _nextId = 1;

        public Func<double> Now { get; set; } = () => 0;
        public Func<Season> SeasonNow { get; set; } = () => Season.Spring;
        public Func<WeatherType> WeatherNow { get; set; } = () => WeatherType.Clear;
        public Func<double> OutdoorTemperature { get; set; } = () => 12.0;
        /// <summary>Alet kademesi (ToolService.Tier); null ise 0.</summary>
        public Func<string, int> ToolTier { get; set; }
        /// <summary>Yer açık mı (maden yeri kazılmış mı vb.); null ise hepsi açık.</summary>
        public Func<string, bool> PlaceOpen { get; set; }
        /// <summary>Ocakbaşı ya da kâhya var mı (+%3).</summary>
        public Func<bool> HasLeader { get; set; }
        /// <summary>Hanedeki köpek sayısı (kurt riskini kaldırır).</summary>
        public Func<int> Dogs { get; set; }
        /// <summary>Oyuncak alışkanlığı (çocuk sıkılınca oynar, §7.1).</summary>
        public Thatchwick.Core.Upbringing.ToyHabitSystem Toys { get; set; }
        /// <summary>Yaralanma (göçük): InjurySystem.InjureById.</summary>
        public Action<Character, string> Injure { get; set; }
        /// <summary>Bildirim (anahtarlı metin, karakter).</summary>
        public Action<LocText, int> Notify { get; set; }
        /// <summary>Anahtarlı metni çözer (moodlet gibi düz metin bekleyen eski API'ler için; oyun katmanı verir).</summary>
        public Func<LocText, string> Resolve { get; set; } = t => t.Literal ?? t.Key ?? "";
        /// <summary>
        /// Maden görevini gerçek haritaya bağlar (belge 58d M0.1): (yer, kişi, karo sayısı) → kazılan karolardan çıkanlar.
        /// Önce işaretli karolar, işaret yoksa girişe en yakın düz taş. Bağlı değilse eski oran tablosu kullanılır.
        /// </summary>
        public Func<TimedDutyPlace, Character, int, List<KeyValuePair<string, int>>> MineDig { get; set; }

        private static LocText L(string key, params (string name, object value)[] args) => LocText.Of(key, args);

        /// <summary>Görev bitti, ürünler eve kondu.</summary>
        public event Action<TimedDutyResult> Finished;

        public IReadOnlyList<TimedDuty> Active => _active;
        public IReadOnlyList<TimedDutyOrder> Repeats => _repeats;
        public TimedDutyResult LastResult { get; private set; }

        public TimedDutyService(Population population, Household house, ContentDatabase content, int seed = 311)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        // ---- yerler ----

        public static readonly TimedDutyPlace[] Places =
        {
            new TimedDutyPlace { Id = "mine:gallery", Kind = TimedDutyKind.Mine, Skill = SkillType.Mining, Tool = "pickaxe", BaseRate = 2.2, Underground = true },
            new TimedDutyPlace { Id = "mine:plot", Kind = TimedDutyKind.Mine, Skill = SkillType.Mining, MinLevel = 2, Tool = "pickaxe", BaseRate = 2.0, Underground = true },
            new TimedDutyPlace { Id = "fish:pond", Kind = TimedDutyKind.Fish, Skill = SkillType.Fishing, Tool = "rod", BaseRate = 1.0, ChildSafe = false },
            new TimedDutyPlace { Id = "fish:stream", Kind = TimedDutyKind.Fish, Skill = SkillType.Fishing, Tool = "rod", BaseRate = 1.0 },
            new TimedDutyPlace { Id = "fish:river", Kind = TimedDutyKind.Fish, Skill = SkillType.Fishing, Tool = "rod", BaseRate = 1.1 },
            new TimedDutyPlace { Id = "fish:lake", Kind = TimedDutyKind.Fish, Skill = SkillType.Fishing, MinLevel = 2, Tool = "rod", BaseRate = 1.0 },
            new TimedDutyPlace { Id = "fish:marsh", Kind = TimedDutyKind.Fish, Skill = SkillType.Fishing, MinLevel = 3, Tool = "rod", BaseRate = 0.9 },
            new TimedDutyPlace { Id = "forest:forest", Kind = TimedDutyKind.Forest, Skill = SkillType.Woodcutting, Tool = "axe", BaseRate = 1.6 },
            new TimedDutyPlace { Id = "forest:hill", Kind = TimedDutyKind.Forest, Skill = SkillType.Woodcutting, MinLevel = 2, Tool = "axe", BaseRate = 1.5 },
            new TimedDutyPlace { Id = "forest:river", Kind = TimedDutyKind.Forest, Skill = SkillType.Woodcutting, Tool = "axe", BaseRate = 1.4 },
            new TimedDutyPlace { Id = "forage:meadow", Kind = TimedDutyKind.Forage, Skill = SkillType.Foraging, BaseRate = 3.0, ChildSafe = true },
            new TimedDutyPlace { Id = "forage:forest", Kind = TimedDutyKind.Forage, Skill = SkillType.Foraging, BaseRate = 3.0 },
            new TimedDutyPlace { Id = "forage:river", Kind = TimedDutyKind.Forage, Skill = SkillType.Foraging, BaseRate = 2.6 },
            new TimedDutyPlace { Id = "traps:forest", Kind = TimedDutyKind.Traps, Skill = SkillType.Hunting, NeedsItem = "item.snare", BaseRate = 0.5 },
            new TimedDutyPlace { Id = "traps:marsh", Kind = TimedDutyKind.Traps, Skill = SkillType.Fishing, MinLevel = 5, NeedsItem = "item.fish_trap", BaseRate = 0.8 },
            new TimedDutyPlace { Id = "clay:river", Kind = TimedDutyKind.Clay, Skill = SkillType.Mining, BaseRate = 2.5 },
            new TimedDutyPlace { Id = "clay:shore", Kind = TimedDutyKind.Clay, Skill = SkillType.Mining, BaseRate = 2.5 },
            new TimedDutyPlace { Id = "errand:village", Kind = TimedDutyKind.Errand, Skill = SkillType.Athletics, BaseRate = 0 },
        };

        public static TimedDutyPlace Place(string id)
        {
            foreach (var p in Places) if (p.Id == id) return p;
            return null;
        }

        /// <summary>Görev türünün adı (anahtarlı).</summary>
        public static LocText KindText(TimedDutyKind k) => LocText.Of("duty.kind." + k.ToString().ToLowerInvariant());

        /// <summary>Yerin adı (anahtarlı): "duty.place.mine.gallery".</summary>
        public static LocText PlaceText(string placeId) => LocText.Of("duty.place." + (placeId ?? "").Replace(':', '.'));

        // ---- kim, nereye, ne kadar (belge 47 §3.6) ----

        /// <summary>Bu yaşta bu türe gidebilir mi; en çok kaç saat.</summary>
        public static double MaxHours(Character c, TimedDutyKind kind)
        {
            if (c == null) return 0;
            switch (c.Stage)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return kind == TimedDutyKind.Forage ? 2 : 0;
                case AgeStage.Teen:
                    return kind == TimedDutyKind.Forage || kind == TimedDutyKind.Fish || kind == TimedDutyKind.Forest || kind == TimedDutyKind.Clay || kind == TimedDutyKind.Errand ? 4 : 0;
                case AgeStage.Elder:
                    return kind == TimedDutyKind.Fish || kind == TimedDutyKind.Forage || kind == TimedDutyKind.Errand ? 4 : 0;
                default: return 12;
            }
        }

        public bool IsAway(int characterId) => Find(characterId) != null;
        public TimedDuty Find(int characterId)
        {
            foreach (var d in _active) if (d.Order.CharacterId == characterId) return d;
            return null;
        }

        /// <summary>Görevin başlayamama nedeni; başlayabiliyorsa boş.</summary>
        public LocText CanStart(Character c, TimedDutyOrder order)
        {
            if (c == null || c.IsDead) return L("duty.err.nobody");
            if (c.IsVillager) return L("duty.err.household_only");
            if (IsAway(c.Id)) return L("duty.err.away", ("name", c.Name));
            var place = Place(order?.PlaceId);
            if (place == null) return L("duty.err.no_place");
            if (place.Kind != order.Kind) return L("duty.err.wrong_place");
            double max = MaxHours(c, order.Kind);
            if (max <= 0)
                return c.Stage == AgeStage.Baby ? L("duty.err.baby") : c.Stage == AgeStage.Child ? L("duty.err.child") : L("duty.err.cannot", ("name", c.Name));
            if (c.Stage == AgeStage.Child && !place.ChildSafe) return L("duty.err.child_place");
            if (c.IsPregnant && (order.Kind == TimedDutyKind.Mine || order.Kind == TimedDutyKind.Traps)) return L("duty.err.pregnant");
            double hours = HoursFor(order);
            if (hours > max + 1e-9) return L("duty.err.max_hours", ("name", c.Name), ("hours", (int)Math.Round(max)));
            if (hours < 0.5) return L("duty.err.evening");
            if (place.Kind != TimedDutyKind.Errand && c.Skills.GetLevel(place.Skill) < place.MinLevel)
                return L("duty.err.skill", ("skill", LocText.Of("skill." + place.Skill)), ("level", place.MinLevel));
            if (place.NeedsItem != null && _house.Count(place.NeedsItem) <= 0) return L("duty.err.item", ("item", place.NeedsItem));
            if (PlaceOpen != null && !PlaceOpen(place.Id)) return L("duty.err.closed", ("place", PlaceText(place.Id)));
            if (order.Kind == TimedDutyKind.Fish && SeasonNow() == Season.Winter && c.Skills.GetLevel(SkillType.Fishing) < 7)
                return L("duty.err.ice");
            if (c.Needs.Get(NeedType.Energy) < TiredReturn + 5) return L("duty.err.tired", ("name", c.Name));
            return default;
        }

        /// <summary>Görev kaç oyun saati sürer ("akşama kadar" ise şimdiden 19.00'a kadar).</summary>
        public double HoursFor(TimedDutyOrder order)
        {
            if (order.Hours > 0) return order.Hours;
            double hour = (Now() - Math.Floor(Now())) * 24.0;
            return Math.Max(0, EveningHour - hour);
        }

        // ---- verim (belge 47 §3.3) ----

        /// <summary>Görevin verim oranı: taban %30 + beceri × %1,5 + alet kademesi × %2 + ocakbaşı %3; en çok %45.</summary>
        public double Efficiency(Character c, TimedDutyPlace place)
        {
            if (c == null || place == null) return 0;
            int level = place.Kind == TimedDutyKind.Errand ? 1 : c.Skills.GetLevel(place.Skill);
            int tier = place.Tool != null && ToolTier != null ? ToolTier(place.Tool) : 0;
            double e = BaseEfficiency + PerSkillLevel * level + PerToolTier * tier + (HasLeader != null && HasLeader() ? LeaderBonus : 0);
            return Math.Min(MaxEfficiency, e);
        }

        /// <summary>Elle oynanınca saatte ortalama toplam ürün: temel oran, beceri seviyesi başına %5 hız (en çok iki kat; JobRunner ile aynı kural).</summary>
        public static double ManualRate(Character c, TimedDutyPlace place)
        {
            if (place == null || place.BaseRate <= 0) return 0;
            int level = c?.Skills.GetLevel(place.Skill) ?? 1;
            double speed = 1.0 / Math.Max(0.5, 1.0 - 0.05 * (level - 1));
            if (c != null && c.Stage == AgeStage.Teen && place.Kind == TimedDutyKind.Forest) speed *= 0.5;   // dal ve küçük ağaç
            return place.BaseRate * speed;
        }

        /// <summary>Yerin ürün tablosu: (eşya, pay, nadir mi). Paylar toplamı 1'e yakın; nadirler ayrı zarla.</summary>
        public List<(string item, double share, bool rare)> YieldTable(Character c, TimedDutyPlace place)
        {
            var list = new List<(string, double, bool)>();
            int lvl = c?.Skills.GetLevel(place.Skill) ?? 1;
            var season = SeasonNow();
            void Add(string id, double share, bool rare = false) { if (_content.Items.Contains(id)) list.Add((id, share, rare)); }
            switch (place.Id)
            {
                case "mine:gallery":
                    Add("item.stone", 0.45); Add("item.limestone", 0.15); Add("item.flint", 0.05); Add("item.clay", 0.07);
                    if (lvl >= 2) { Add("item.coal", 0.1); Add("item.iron_ore", 0.12); Add("item.ochre", 0.03); }
                    if (lvl >= 3) { Add("item.gypsum", 0.03); Add("item.slate_stone", 0.03); }
                    Add("item.relic_fossil", 0.004, rare: true);
                    break;
                case "mine:plot":
                    Add("item.clay", 0.35); Add("item.stone", 0.3); Add("item.limestone", 0.2);
                    if (lvl >= 2) { Add("item.coal", 0.1); Add("item.iron_ore", 0.05); }
                    Add("item.relic_buried_jar", 0.004, rare: true);
                    break;
                case "fish:pond": Add("item.fish_carp", 0.6); Add("item.fish_crayfish", 0.4); break;
                case "fish:stream": Add("item.fish_crayfish", 0.5); if (lvl >= 2) Add("item.fish_trout", 0.5); break;
                case "fish:river":
                    Add("item.fish_carp", 0.4); if (lvl >= 2) Add("item.fish_trout", 0.35);
                    if (lvl >= 4) Add("item.fish_pike", 0.15); if (lvl >= 6) Add("item.fish_eel", 0.1);
                    break;
                case "fish:lake": Add("item.fish_carp", 0.6); if (lvl >= 4) Add("item.fish_pike", 0.25); if (lvl >= 6) Add("item.fish_eel", 0.15); break;
                case "fish:marsh": Add("item.fish_crayfish", 0.6); if (lvl >= 6) Add("item.fish_eel", 0.4); break;
                case "forest:forest": Add("item.log", 0.7); Add("item.firewood", 0.3); if (season == Season.Autumn) Add("item.oak_gall", 0.05); break;
                case "forest:hill": Add("item.log", 0.75); Add("item.firewood", 0.2); if (lvl >= 6) Add("item.resin", 0.08); break;
                case "forest:river": Add("item.log", 0.6); Add("item.firewood", 0.4); break;
                case "forage:meadow": AddForage(list, new[] { "chamomile", "strawberry", "rosehip", "elderflower", "linden", "sage", "nettle", "berry" }, season, lvl); break;
                case "forage:forest": AddForage(list, new[] { "mushroom", "hazel", "chestnut", "berry", "herbs", "pinecone", "dry_branch", "hive" }, season, lvl); break;
                case "forage:river": AddForage(list, new[] { "cress", "mint", "wild_garlic", "nettle" }, season, lvl); break;
                case "traps:forest": Add("item.game_meat", 0.7); Add("item.hide", 0.3); break;
                case "traps:marsh": Add("item.fish_crayfish", 1.0); break;
                case "clay:river": Add("item.clay", 1.0); break;
                case "clay:shore": Add("item.sand", 1.0); break;
            }
            return list;
        }

        // Toplama düğümlerinden mevsimin ürünleri (içerikten okunur, belge 47: ayrı tablo tutulmaz).
        private void AddForage(List<(string, double, bool)> list, string[] keys, Season season, int level)
        {
            var found = new List<string>();
            foreach (var def in _content.Items.All)
            {
                if (def.Category != "forage") continue;
                bool match = false;
                foreach (var k in keys) if (def.Id.Contains(k)) { match = true; break; }
                if (!match) continue;
                if (def.ForageSeasons != null && def.ForageSeasons.Count > 0 && !def.ForageSeasons.Contains(season)) continue;
                foreach (var i in def.Interactions)
                {
                    if (i.Skill.HasValue && i.MinSkillLevel > level) continue;
                    foreach (var p in i.Produce) if (p.ItemId != null && !found.Contains(p.ItemId)) found.Add(p.ItemId);
                    break;
                }
            }
            if (found.Count == 0) return;
            foreach (var id in found) list.Add((id, 1.0 / found.Count, false));
        }

        // ---- azık (belge 47 §3.4) ----

        /// <summary>Yiyeceğin doyurduğu (azık etiketi en çok, pişmiş orta, çiğ az); yiyecek değilse 0. Çiğ et ve çiğ balık alınmaz.</summary>
        public float MealValue(string itemId)
        {
            if (itemId == null || !_content.Items.TryGet(itemId, out var d) || !d.HasTag("food")) return 0f;
            bool raw = d.HasTag("raw");
            if (raw && (d.HasTag("fish") || itemId.Contains("meat"))) return 0f;
            if (d.HasTag("packed")) return 60f;
            if (d.HasTag("cooked") || d.HasTag("preserved") || d.HasTag("salted")) return 45f;
            return 30f;
        }

        /// <summary>Kaç öğün gerekir: süre boyunca beklenen açlık düşüşünü karşılayacak kadar + 1 yedek.</summary>
        public int MealsNeeded(Character c, double hours, float mealValue = 45f)
        {
            double decay = c.Needs.GetDecayPerDay(NeedType.Hunger) * c.Needs.GetDecayMultiplier(NeedType.Hunger) * hours / 24.0;
            double room = Math.Max(0, c.Needs.Get(NeedType.Hunger) - (EatBelow + 5));
            double need = Math.Max(0, decay - room);
            return (int)Math.Ceiling(need / Math.Max(1f, mealValue)) + 1;
        }

        /// <summary>Hane envanterinden azık önerisi: azık etiketliler, sonra pişmiş, sonra çiğ (et ve balık hariç).</summary>
        public List<KeyValuePair<string, int>> SuggestFood(Character c, double hours)
        {
            var candidates = new List<(string id, float value, int stock)>();
            foreach (var kv in _house.InventoryList())
            {
                float v = MealValue(kv.Key);
                if (v > 0 && kv.Value > 0) candidates.Add((kv.Key, v, kv.Value));
            }
            candidates.Sort((a, b) => b.value.CompareTo(a.value));
            var pick = new List<KeyValuePair<string, int>>();
            int meals = MealsNeeded(c, hours);
            double needValue = meals * 45.0;
            foreach (var (id, value, stock) in candidates)
            {
                if (needValue <= 0) break;
                int take = Math.Min(stock, (int)Math.Ceiling(needValue / value));
                if (take <= 0) continue;
                pick.Add(new KeyValuePair<string, int>(id, take));
                needValue -= take * value;
            }
            return pick;
        }

        // ---- başlat, işle, bitir ----

        /// <summary>Görevi başlatır: azığı envanterden alır, karakteri haritadan çıkarır. Başlayamazsa null ve nedeni.</summary>
        public TimedDuty Start(TimedDutyOrder order, out LocText reason, List<KeyValuePair<string, int>> food = null)
        {
            var c = _population.Find(order.CharacterId);
            reason = CanStart(c, order);
            if (!reason.IsEmpty) return null;
            double hours = Math.Min(HoursFor(order), MaxHours(c, order.Kind));
            var duty = new TimedDuty { Id = _nextId++, Order = order, StartDay = Now(), PlannedHours = hours, StartPos = c.Position };
            // Azık en düşük kaliteden alınır (belge 53 §4.7); anahtar kaliteyi ve kaynağı taşır ("item.bread#2"), artan eve öyle döner.
            foreach (var kv in food ?? SuggestFood(c, hours))
                for (int i = 0; i < kv.Value; i++)
                {
                    if (!_house.TryTake(kv.Key, TakePolicy.Lowest, out var taken)) break;
                    string token = taken.Token();
                    duty.Packed[token] = (duty.Packed.TryGetValue(token, out int n) ? n : 0) + 1;
                }
            c.Away = true;
            c.Asleep = false;
            _active.Add(duty);
            if (order.Repeat) SetRepeat(order);
            return duty;
        }

        public bool Start(TimedDutyOrder order) => Start(order, out _) != null;

        /// <summary>Görevi hemen bitirir (geri çağır): o ana kadarki ürünle döner.</summary>
        public TimedDutyResult Recall(int characterId)
        {
            var d = Find(characterId);
            return d == null ? null : Finish(d, TimedDutyEnd.Recalled);
        }

        /// <summary>"Her gün tekrarla" planını kaldırır.</summary>
        public void StopRepeat(int characterId)
        {
            _repeats.RemoveAll(o => o.CharacterId == characterId);
            _repeatStartFraction.Remove(characterId);
            _repeatLastDay.Remove(characterId);
        }

        private void SetRepeat(TimedDutyOrder order)
        {
            StopRepeat(order.CharacterId);
            _repeats.Add(order);
            _repeatStartFraction[order.CharacterId] = Now() - Math.Floor(Now());
            _repeatLastDay[order.CharacterId] = (long)Math.Floor(Now());
        }

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) { CheckRepeats(); return; }
            double hours = gameDays * 24.0;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var d = _active[i];
                var c = _population.Find(d.Order.CharacterId);
                if (c == null || c.IsDead) { Finish(d, TimedDutyEnd.Died); continue; }
                var place = Place(d.Order.PlaceId);
                c.AmbientTemperature = (float)(place != null && place.Underground ? MineTemperature : OutdoorTemperature());
                if (c.Needs.Get(NeedType.Bladder) < 25f) c.Needs.Set(NeedType.Bladder, 100f);   // doğada
                var end = Step(d, c, place, hours);
                if (end != TimedDutyEnd.Running) Finish(d, end);
            }
            CheckRepeats();
        }

        private TimedDutyEnd Step(TimedDuty d, Character c, TimedDutyPlace place, double hours)
        {
            // Yeme molası: açlık eşiğin altındaysa heybeden yer.
            if (c.Needs.Get(NeedType.Hunger) < EatBelow)
            {
                if (EatPacked(d, c)) { d.BreakHours += MealBreakHours; hours = Math.Max(0, hours - MealBreakHours); }
                else if (c.Needs.Get(NeedType.Hunger) < HungryReturn) return TimedDutyEnd.Hungry;
            }
            if (c.Needs.Get(NeedType.Energy) < TiredReturn) return TimedDutyEnd.Tired;
            if (c.Needs.Get(NeedType.Warmth) < ColdReturn) return TimedDutyEnd.Cold;

            // Çocuk ve ergen görevde daha çabuk sıkılır (§7.1).
            double funFactor = c.Stage == AgeStage.Child ? ChildFunFactor : c.Stage == AgeStage.Teen ? TeenFunFactor : 1.0;
            if (funFactor > 1.0)
            {
                double extra = c.Needs.GetDecayPerDay(NeedType.Fun) * (funFactor - 1.0) * hours / 24.0;
                c.Needs.Set(NeedType.Fun, (float)Math.Max(0, c.Needs.Get(NeedType.Fun) - extra));
            }
            if (d.Playing)
            {
                d.PlayedHours += hours;
                PlayToy(d, c, hours);
                if (c.Needs.Get(NeedType.Fun) >= 80f) d.Playing = false;
                return d.ElapsedHours >= d.PlannedHours ? TimedDutyEnd.Done : TimedDutyEnd.Running;
            }
            if (funFactor > 1.0 && c.Needs.Get(NeedType.Fun) < BoredBelow)
            {
                double trait = c.HasTrait("trait.diligent") ? 0.5 : c.HasTrait("trait.cheerful") ? 1.2 : 1.0;
                if (c.Stage == AgeStage.Teen) trait *= 0.6;
                if (_rng.NextDouble() < 1 - Math.Pow(1 - BoredChancePerHour * trait, hours))
                {
                    d.Playing = true;
                    string toy = ToyFor(c);
                    d.Events.Add(toy != null ? L("duty.ev.bored_toy", ("toy", toy)) : L("duty.ev.bored"));
                    return TimedDutyEnd.Running;
                }
            }

            d.WorkedHours += hours;
            if (place != null && place.Kind != TimedDutyKind.Errand) Produce(d, c, place, hours);
            if (place != null && place.Kind != TimedDutyKind.Errand)
            {
                double xp = ManualXpPerHour * XpShare * hours;
                c.GainSkillXp(place.Skill, xp);
                d.Xp += xp;
            }

            // Riskler (§3.5).
            if (place != null && place.Kind == TimedDutyKind.Mine && Roll(CaveInPerHour * (c.Skills.GetLevel(SkillType.Mining) >= 9 ? 0.5 : 1.0), hours))
            {
                Injure?.Invoke(c, "cave_in");
                d.Events.Add(L("duty.ev.cave_in"));
                return TimedDutyEnd.Danger;
            }
            if (place != null && (place.Id == "forest:forest" || place.Id == "traps:forest" || place.Id == "forage:forest"))
            {
                bool wolves = SeasonNow() == Season.Winter && c.Skills.GetLevel(SkillType.Hunting) < 7 && (Dogs == null || Dogs() <= 0);
                if (wolves && Roll(WolfPerHour, hours)) { d.Events.Add(L("duty.ev.wolf")); return TimedDutyEnd.Danger; }
                if (Roll(BearPerHour, hours)) { d.Events.Add(L("duty.ev.bear")); return TimedDutyEnd.Danger; }
            }
            if (d.Order.TargetItem != null && d.Order.TargetCount > 0 && GainOf(d, d.Order.TargetItem) >= d.Order.TargetCount) return TimedDutyEnd.Target;
            return d.ElapsedHours >= d.PlannedHours - 1e-9 ? TimedDutyEnd.Done : TimedDutyEnd.Running;
        }

        private bool Roll(double perHour, double hours) => perHour > 0 && _rng.NextDouble() < 1 - Math.Pow(1 - perHour, hours);

        private void Produce(TimedDuty d, Character c, TimedDutyPlace place, double hours)
        {
            double rate = ManualRate(c, place) * Efficiency(c, place) * hours;
            if (place.Kind == TimedDutyKind.Mine && MineDig != null)
            {
                // Verim tavanı kazılan karo sayısına uygulanır; ürün kazılan karodan gelir (damar tükenir).
                d.MineTiles += rate;
                int n = (int)Math.Floor(d.MineTiles + 1e-9);
                if (n <= 0) return;
                d.MineTiles -= n;
                var got = MineDig(place, c, n);
                if (got != null) foreach (var kv in got) Add(d, kv.Key, kv.Value);
                return;
            }
            var w = WeatherNow();
            if (!place.Underground && w == WeatherType.Storm) rate *= StormYield;
            foreach (var (item, share, rare) in YieldTable(c, place))
            {
                if (rare)
                {
                    if (_rng.NextDouble() < share * RareFactor * hours) Add(d, item, 1);
                    continue;
                }
                Add(d, item, rate * share);
            }
        }

        private static void Add(TimedDuty d, string item, double amount) => d.Gains[item] = (d.Gains.TryGetValue(item, out var v) ? v : 0) + amount;
        private static int GainOf(TimedDuty d, string item) => d.Gains.TryGetValue(item, out var v) ? (int)Math.Floor(v + 1e-9) : 0;

        /// <summary>Şu ana kadarki kazanç (görev şeridinde gösterilir).</summary>
        public static List<KeyValuePair<string, int>> SoFar(TimedDuty d) => Whole(d.Gains);

        private bool EatPacked(TimedDuty d, Character c)
        {
            // Kaliteli azık daha çok doyurur (53 §4.5); en çok doyuranı yer.
            string best = null; float bestValue = 0;
            foreach (var kv in d.Packed)
            {
                if (kv.Value <= 0) continue;
                var key = ItemKey.FromToken(kv.Key);
                float v = (float)(MealValue(key.Id) * ItemQuality.SatietyFactor(key.Quality));
                if (v > bestValue) { best = kv.Key; bestValue = v; }
            }
            if (best == null) return false;
            d.Packed[best]--;
            c.Needs.Restore(NeedType.Hunger, bestValue);
            return true;
        }

        private string ToyFor(Character c)
        {
            if (Toys != null)
                foreach (var p in Toys.PersonalOf(c.Id)) if (!p.IsBroken) return p.ItemId;
            foreach (var kv in _house.InventoryList())
                if (_content.Items.TryGet(kv.Key, out var d) && d.Category == "kids" && kv.Value > 0)
                    foreach (var i in d.Interactions) if (i.Need == NeedType.Fun) return kv.Key;
            return null;
        }


        private void PlayToy(TimedDuty d, Character c, double hours)
        {
            c.Needs.Restore(NeedType.Fun, (float)(30.0 * hours));   // saatte 30 keyif
            var toy = ToyFor(c);
            if (toy != null && Toys != null && Toys.IsToy(toy)) Toys.OnPlayed(c, toy, Now());   // alışkanlığa işler
        }

        private TimedDutyResult Finish(TimedDuty d, TimedDutyEnd end)
        {
            _active.Remove(d);
            d.End = end;
            var c = _population.Find(d.Order.CharacterId);
            var place = Place(d.Order.PlaceId);
            var r = new TimedDutyResult
            {
                CharacterId = d.Order.CharacterId, CharacterName = c?.Name ?? "?", Kind = d.Order.Kind, PlaceId = d.Order.PlaceId,
                End = end, Xp = d.Xp, RequestId = d.Order.RequestId, Hours = d.ElapsedHours
            };
            // Kalite (47 §3.3, 53 §4.3): elle toplamanın dağılımı, bir basamak düşük.
            int level = c != null && place != null && place.Kind != TimedDutyKind.Errand ? c.Skills.GetLevel(place.Skill) : 1;
            foreach (var kv in Whole(d.Gains))
            {
                bool graded = _content.Items.TryGet(kv.Key, out var gd) && gd.HasQuality;
                for (int u = 0; u < kv.Value; u++)
                    _house.AddItem(kv.Key, 1, graded ? Math.Max(0, ItemQuality.Gathered(level, _rng) - 1) : 0);
                r.Items.Add(kv);
            }
            foreach (var kv in d.Packed) if (kv.Value > 0) _house.Add(ItemKey.FromToken(kv.Key), kv.Value);   // artan azık kalitesiyle eve döner
            r.Notes.AddRange(d.Events);
            if (c != null)
            {
                c.Away = false;
                c.Position = d.StartPos;
                if (end != TimedDutyEnd.Died && place != null && place.Kind == TimedDutyKind.Mine && c.MoodEnabled)
                    c.AddMoodlet("mood.dusty", Resolve(L("duty.mood.dusty")), -3, 0.5);
                if (end == TimedDutyEnd.Tired && c.MoodEnabled) c.AddMoodlet("mood.duty_tired", Resolve(L("duty.mood.tired")), -3, 0.5);
            }
            r.Summary = Summarize(r);
            LastResult = r;
            Notify?.Invoke(r.Summary, r.CharacterId);
            Finished?.Invoke(r);
            return r;
        }

        /// <summary>
        /// Kesirli kazancı tam sayıya çevirir: toplamın tam kısmı en büyük kalan yöntemiyle paylaştırılır (3,9 cevher 3 değil, kalanlarla 3-4).
        /// </summary>
        public static List<KeyValuePair<string, int>> Whole(Dictionary<string, double> gains)
        {
            var result = new List<KeyValuePair<string, int>>();
            double sum = 0;
            foreach (var kv in gains) sum += kv.Value;
            int total = (int)Math.Floor(sum + 1e-9);
            var counts = new Dictionary<string, int>();
            var rest = new List<(string id, double frac)>();
            int given = 0;
            foreach (var kv in gains)
            {
                int n = (int)Math.Floor(kv.Value + 1e-9);
                counts[kv.Key] = n; given += n;
                rest.Add((kv.Key, kv.Value - n));
            }
            rest.Sort((a, b) => b.frac.CompareTo(a.frac));
            for (int i = 0; i < rest.Count && given < total; i++) { counts[rest[i].id]++; given++; }
            foreach (var kv in gains) if (counts[kv.Key] > 0) result.Add(new KeyValuePair<string, int>(kv.Key, counts[kv.Key]));
            return result;
        }

        public LocText Summarize(TimedDutyResult r)
        {
            LocText what = L("duty.sum.nothing");
            for (int i = 0; i < r.Items.Count; i++)
            {
                var part = L("duty.sum.item", ("n", r.Items[i].Value), ("item", r.Items[i].Key));
                what = i == 0 ? part : L("core.req.list", ("a", what), ("b", part));
            }
            var text = L("duty.sum", ("name", r.CharacterName), ("kind", KindText(r.Kind)), ("place", PlaceText(r.PlaceId)),
                ("why", L("duty.end." + r.End.ToString().ToLowerInvariant())), ("what", what));
            foreach (var note in r.Notes) text = L("duty.sum.note", ("a", text), ("b", note));
            return text;
        }

        // "Her gün tekrarla": ertesi gün aynı saatte kendiliğinden yeniden çıkar.
        private void CheckRepeats()
        {
            if (_repeats.Count == 0) return;
            double now = Now();
            long day = (long)Math.Floor(now);
            double frac = now - day;
            foreach (var order in _repeats.ToArray())
            {
                int id = order.CharacterId;
                if (_repeatLastDay.TryGetValue(id, out long last) && last >= day) continue;
                if (!_repeatStartFraction.TryGetValue(id, out double start) || frac < start) continue;
                _repeatLastDay[id] = day;
                var c = _population.Find(id);
                if (c == null || c.IsDead || c.IsVillager) { StopRepeat(id); continue; }
                if (IsAway(id)) continue;
                var copy = new TimedDutyOrder { CharacterId = id, Kind = order.Kind, PlaceId = order.PlaceId, Hours = order.Hours, TargetItem = order.TargetItem, TargetCount = order.TargetCount, Repeat = false };
                if (Start(copy, out var why) == null) Notify?.Invoke(L("duty.n.repeat_failed", ("name", c.Name), ("why", why)), id);
            }
        }

        // ---- kayıt ----

        public TimedDutyServiceSave Export()
        {
            var s = new TimedDutyServiceSave { nextId = _nextId };
            var list = new List<TimedDutySave>();
            foreach (var d in _active)
            {
                var ds = new TimedDutySave
                {
                    id = d.Id, characterId = d.Order.CharacterId, kind = (int)d.Order.Kind, place = d.Order.PlaceId, hours = d.Order.Hours,
                    targetItem = d.Order.TargetItem, targetCount = d.Order.TargetCount, requestId = d.Order.RequestId, label = LocCodec.Encode(d.Order.Label),
                    startDay = d.StartDay, plannedHours = d.PlannedHours, workedHours = d.WorkedHours, breakHours = d.BreakHours, playedHours = d.PlayedHours,
                    playing = d.Playing, x = d.StartPos.X, y = d.StartPos.Y, z = d.StartPos.Z, xp = d.Xp, mineTiles = d.MineTiles, events = d.Events.ConvertAll(LocCodec.Encode).ToArray()
                };
                var gk = new List<string>(); var gv = new List<double>();
                foreach (var kv in d.Gains) { gk.Add(kv.Key); gv.Add(kv.Value); }
                ds.gainItems = gk.ToArray(); ds.gainAmounts = gv.ToArray();
                var pk = new List<string>(); var pv = new List<int>();
                foreach (var kv in d.Packed) { pk.Add(kv.Key); pv.Add(kv.Value); }
                ds.packedItems = pk.ToArray(); ds.packedCounts = pv.ToArray();
                list.Add(ds);
            }
            s.active = list.ToArray();
            var reps = new List<TimedDutySave>();
            foreach (var o in _repeats)
                reps.Add(new TimedDutySave
                {
                    characterId = o.CharacterId, kind = (int)o.Kind, place = o.PlaceId, hours = o.Hours, targetItem = o.TargetItem, targetCount = o.TargetCount,
                    startDay = _repeatStartFraction.TryGetValue(o.CharacterId, out var f) ? f : 0.3,
                    workedHours = _repeatLastDay.TryGetValue(o.CharacterId, out var l) ? l : -1
                });
            s.repeats = reps.ToArray();
            return s;
        }

        public void Import(TimedDutyServiceSave s)
        {
            _active.Clear(); _repeats.Clear(); _repeatStartFraction.Clear(); _repeatLastDay.Clear();
            if (s == null) return;
            _nextId = Math.Max(1, s.nextId);
            if (s.active != null)
                foreach (var ds in s.active)
                {
                    if (ds == null) continue;
                    var order = new TimedDutyOrder { CharacterId = ds.characterId, Kind = (TimedDutyKind)ds.kind, PlaceId = ds.place, Hours = ds.hours, TargetItem = ds.targetItem, TargetCount = ds.targetCount, RequestId = ds.requestId, Label = LocCodec.Decode(ds.label) };
                    var d = new TimedDuty
                    {
                        Id = ds.id, Order = order, StartDay = ds.startDay, PlannedHours = ds.plannedHours, WorkedHours = ds.workedHours, BreakHours = ds.breakHours,
                        PlayedHours = ds.playedHours, Playing = ds.playing, StartPos = new World.GridPos(ds.x, ds.y, ds.z), Xp = ds.xp, MineTiles = ds.mineTiles
                    };
                    if (ds.events != null) foreach (var e in ds.events) d.Events.Add(LocCodec.Decode(e));
                    if (ds.gainItems != null) for (int i = 0; i < ds.gainItems.Length && i < (ds.gainAmounts?.Length ?? 0); i++) d.Gains[ds.gainItems[i]] = ds.gainAmounts[i];
                    if (ds.packedItems != null) for (int i = 0; i < ds.packedItems.Length && i < (ds.packedCounts?.Length ?? 0); i++) d.Packed[ds.packedItems[i]] = ds.packedCounts[i];
                    _active.Add(d);
                    var c = _population.Find(order.CharacterId);
                    if (c != null) c.Away = true;
                }
            if (s.repeats != null)
                foreach (var r in s.repeats)
                {
                    if (r == null) continue;
                    _repeats.Add(new TimedDutyOrder { CharacterId = r.characterId, Kind = (TimedDutyKind)r.kind, PlaceId = r.place, Hours = r.hours, TargetItem = r.targetItem, TargetCount = r.targetCount, Repeat = true });
                    _repeatStartFraction[r.characterId] = r.startDay;
                    _repeatLastDay[r.characterId] = (long)r.workedHours;
                }
        }
    }

    [Serializable]
    public sealed class TimedDutySave
    {
        public int id, characterId, kind, targetCount, requestId, x, y, z;
        public string place, targetItem, label;
        public double hours, startDay, plannedHours, workedHours, breakHours, playedHours, xp, mineTiles;
        public bool playing;
        public string[] events, gainItems, packedItems;
        public double[] gainAmounts;
        public int[] packedCounts;
    }

    [Serializable]
    public sealed class TimedDutyServiceSave
    {
        public int nextId = 1;
        public TimedDutySave[] active;
        public TimedDutySave[] repeats;
    }
}
