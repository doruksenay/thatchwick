using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Cooking
{
    /// <summary>Pazar yemek tezgâhının gün sonu dökümü (belge 60b §5.7).</summary>
    public sealed class StallResult
    {
        public int Sold, Unsold, Revenue, Reputation;
        public string Favorite;
    }

    /// <summary>
    /// Baş başa etkinlikler, yemeğe davet ve pazar tezgâhı (belge 58 §10, 60b §5.7; kararlar 62-S8, S9, S10, F10):
    /// <list type="bullet">
    /// <item>Piknik: sepet ve en az iki yiyecekle; yakınlık, keyif, "Piknik" moodlet'i. Gönüllü ya da eşlerse "Baş başa".</item>
    /// <item>Akşam yürüyüşü, birlikte balık tutma, ateş başı.</item>
    /// <item>"Gönüllü": köylüyle romantik yakınlık 50 ve üstü.</item>
    /// <item>Yemeğe davet: tek misafir köylü, sofradaki yemekten bir porsiyon; yemek zevki ve kalite yakınlığı belirler.</item>
    /// <item>Pazar tezgâhı: pişmiş yemek porsiyonla satılır; kalite ve çeşit fiyatı belirler, itibar kazandırır.</item>
    /// </list>
    /// </summary>
    public sealed class Outings
    {
        public const string BasketId = "item.picnic_basket";
        public const int SweetheartRomance = 50;
        public const int PicnicFoods = 2;
        public const double DayFrom = 0.30, DayUntil = 0.80, EveningFrom = 0.70;
        public const int StallBaseDemand = 6;

        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly MealService _meals;
        private readonly Random _rng;
        private readonly Dictionary<int, long> _dinnerDay = new Dictionary<int, long>();
        private readonly List<(string dish, int quality, int count)> _stall = new List<(string, int, int)>();
        private int _stallSeller;
        private long _stallDay = -1;
        private string _villageFavorite;
        private long _favoriteUntil = -1;

        public Outings(Household house, ContentDatabase content, MealService meals, int seed = 62)
        {
            _house = house; _content = content; _meals = meals; _rng = new Random(seed);
        }

        public Func<double> Now { get; set; } = () => 0;
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";
        public Func<int, int, int> Affection { get; set; }
        public Action<int, int, int> AddAffection { get; set; }
        public Func<bool> BadWeather { get; set; }
        public Func<bool> IsWinter { get; set; }
        /// <summary>Köylünün romantik yakınlığı (köylü değilse −1) ve artırma.</summary>
        public Func<Character, int> RomanceOf { get; set; }
        public Action<Character, int> AddRomance { get; set; }
        /// <summary>Yerin güzelliği 0-3: ağaç altı, su kenarı, çiçek, ulu ağaç.</summary>
        public Func<GridPos, int> SpotBeauty { get; set; }
        public Func<GridPos, bool> NearWater { get; set; }
        public Func<GridPos, bool> NearFire { get; set; }
        /// <summary>Köylünün yemeğe bakışı: 2 bayılır, 1 sever, 0 nötr, −1 sevmez.</summary>
        public Func<Character, string, int> VillagerTaste { get; set; }
        public Func<bool> MarketDay { get; set; }
        public Action<int, string> AddReputation { get; set; }
        public Func<int> Reputation { get; set; }
        public event Action<LocText, Character> Notice;
        public event Action<Character, Character, SocialAction> Happened;

        public static bool Handles(SocialAction a) => a == SocialAction.Picnic || a == SocialAction.EveningWalk || a == SocialAction.FishTogether
            || a == SocialAction.Fireside || a == SocialAction.InviteDinner;

        private double Frac => Now() - Math.Floor(Now());

        /// <summary>Gönüllü: romantik yakınlığı 50 ve üstü olan köylü (evli değil).</summary>
        public bool IsSweetheart(Character a, Character b)
        {
            if (a == null || b == null || a.SpouseId.HasValue || b.SpouseId.HasValue) return false;
            var villager = b.IsVillager ? b : a.IsVillager ? a : null;
            return villager != null && RomanceOf != null && RomanceOf(villager) >= SweetheartRomance;
        }

        public bool IsRomantic(Character a, Character b) => a != null && b != null && (a.SpouseId == b.Id || IsSweetheart(a, b));

        /// <summary>Piknik sepetine konacak yiyecekler: pişmiş ve hazır yiyecekten en iyi iki tür.</summary>
        public List<string> PicnicFood()
        {
            var foods = new List<(string id, int score)>();
            foreach (var kv in _house.InventoryList())
            {
                if (!_content.Items.TryGet(kv.Key, out var d) || !d.HasTag("food") || d.HasTag("raw") || !_content.Foods.TryGetValue(kv.Key, out var f) || f.Need != NeedType.Hunger) continue;
                foods.Add((kv.Key, (d.HasTag("cooked") ? 100 : 0) + (int)f.Restore + _house.BestQuality(kv.Key) * 20));
            }
            return foods.OrderByDescending(f => f.score).Take(PicnicFoods).Select(f => f.id).ToList();
        }

        public string CanDo(Character actor, Character target, SocialAction action)
        {
            if (target.Stage == AgeStage.Baby) return Text(LocText.Of("cook.out.block.baby"));
            if (target.Asleep) return Text(LocText.Of("cook.out.block.asleep", ("name", target.Name)));
            switch (action)
            {
                case SocialAction.Picnic:
                    if (_house.Count(BasketId) < 1) return Text(LocText.Of("cook.out.block.no_basket"));
                    if (PicnicFood().Count < PicnicFoods) return Text(LocText.Of("cook.out.block.no_food"));
                    if (Frac < DayFrom || Frac > DayUntil) return Text(LocText.Of("cook.out.block.daytime"));
                    if (BadWeather != null && BadWeather()) return Text(LocText.Of("cook.out.block.weather"));
                    return Decline(actor, target, 20);
                case SocialAction.EveningWalk:
                    if (Frac < EveningFrom) return Text(LocText.Of("cook.out.block.evening"));
                    if (BadWeather != null && BadWeather()) return Text(LocText.Of("cook.out.block.weather"));
                    return Decline(actor, target, 10);
                case SocialAction.FishTogether:
                    if (actor.Stage == AgeStage.Child && target.Stage == AgeStage.Child) return Text(LocText.Of("cook.out.block.adult_needed"));
                    if (NearWater != null && !NearWater(actor.Position)) return Text(LocText.Of("cook.out.block.no_water"));
                    return Decline(actor, target, 10);
                case SocialAction.Fireside:
                    if (NearFire != null && !NearFire(actor.Position)) return Text(LocText.Of("cook.out.block.no_fire"));
                    return null;
                case SocialAction.InviteDinner:
                    if (!target.IsVillager || actor.IsVillager) return Text(LocText.Of("cook.out.block.villager_only"));
                    if (!_meals.AnyServing) return Text(LocText.Of("cook.out.block.no_table"));
                    if (_dinnerDay.TryGetValue(target.Id, out long d) && d == (long)Math.Floor(Now())) return Text(LocText.Of("cook.out.block.already_invited", ("name", target.Name)));
                    return Decline(actor, target, 25);
            }
            return null;
        }

        /// <summary>Karşı taraf nazikçe reddeder mi: yakınlık yetmiyorsa ya da perişansa.</summary>
        private string Decline(Character actor, Character target, int need)
        {
            bool family = !actor.IsVillager && !target.IsVillager;
            if (!family && Affection != null && Affection(actor.Id, target.Id) < need) return Text(LocText.Of("cook.out.block.not_close", ("name", target.Name), ("n", need)));
            if (target.MoodStatus == Mood.MoodStatus.Miserable) return Text(LocText.Of("cook.out.block.mood", ("name", target.Name)));
            return null;
        }

        public double Duration(SocialAction action)
        {
            switch (action)
            {
                case SocialAction.Picnic: return 0.12;
                case SocialAction.EveningWalk: return 0.06;
                case SocialAction.FishTogether: return 0.10;
                case SocialAction.Fireside: return 0.06;
                default: return 0.04;
            }
        }

        public void Apply(Character actor, Character target, SocialAction action)
        {
            if (CanDo(actor, target, action) != null) return;
            bool romantic = IsRomantic(actor, target);
            var both = new[] { actor, target };
            int affection = 0, romance = 0;
            switch (action)
            {
                case SocialAction.Picnic:
                {
                    var foods = PicnicFood();
                    int quality = 0; float restore = 0f;
                    foreach (var f in foods)
                    {
                        // İki kişi birer porsiyon yer; ikinci porsiyon yoksa paylaşırlar.
                        for (int k = 0; k < 2; k++) if (_house.TryTake(f, TakePolicy.Highest, out var took)) quality = Math.Max(quality, took.Quality);
                        restore += _content.Foods[f].Restore * 0.6f;
                    }
                    int beauty = SpotBeauty != null ? Math.Max(0, Math.Min(3, SpotBeauty(actor.Position))) : 0;
                    affection = 6 + quality * 2 + beauty;
                    foreach (var c in both)
                    {
                        c.Needs.Restore(NeedType.Hunger, restore);
                        c.Needs.Restore(NeedType.Fun, 25f + beauty * 3f);
                        c.Needs.Restore(NeedType.Social, 20f);
                        c.AddMoodlet("cook.picnic", Text(LocText.Of("cook.mood.picnic")), 4 + Math.Min(2, beauty), 1.0);
                    }
                    romance = 8;
                    break;
                }
                case SocialAction.EveningWalk:
                    affection = 3; romance = 4;
                    foreach (var c in both) { c.Needs.Restore(NeedType.Fun, 10f); c.Needs.Restore(NeedType.Social, 12f); c.AddMoodlet("cook.walk", Text(LocText.Of("cook.mood.walk")), 2, 0.4); }
                    break;
                case SocialAction.FishTogether:
                {
                    affection = 4; romance = 3;
                    foreach (var c in both) { c.Needs.Restore(NeedType.Fun, 14f); c.Needs.Restore(NeedType.Social, 12f); c.GainSkillXp(SkillType.Fishing, 8f); }
                    if (_content.Groups.TryGet("group.fish", out var fish) && fish.Members.Count > 0)
                    {
                        var common = fish.Members.Take(Math.Max(1, fish.Members.Count / 2)).ToList();
                        _house.AddItem(common[_rng.Next(common.Count)].ItemId, 1);
                    }
                    break;
                }
                case SocialAction.Fireside:
                {
                    bool winter = IsWinter != null && IsWinter();
                    affection = winter ? 4 : 3; romance = 3;
                    foreach (var c in both)
                    {
                        c.Needs.Restore(NeedType.Warmth, 20f); c.Needs.Restore(NeedType.Fun, 10f); c.Needs.Restore(NeedType.Social, 12f);
                        c.AddMoodlet("cook.fireside", Text(LocText.Of("cook.mood.fireside")), winter ? 3 : 2, 0.4);
                    }
                    break;
                }
                case SocialAction.InviteDinner:
                {
                    var serving = _meals.Servings.Values.OrderByDescending(s => s.Quality).First();
                    string dish = serving.DishId;
                    int q = _meals.TakePortion(serving.TableId);
                    int taste = VillagerTaste != null ? VillagerTaste(target, dish) : 0;
                    affection = Math.Max(1, 4 + q * 2 + taste * 3 - (serving.Burnt ? 4 : 0));
                    _dinnerDay[target.Id] = (long)Math.Floor(Now());
                    actor.AddMoodlet("cook.hosted", Text(LocText.Of("cook.mood.hosted")), 3, 0.5);
                    actor.Needs.Restore(NeedType.Social, 20f);
                    if (q >= ItemQuality.Superior && !serving.Burnt) AddReputation?.Invoke(1, Text(LocText.Of("cook.rep.dinner")));
                    Notice?.Invoke(LocText.Of(serving.Burnt ? "cook.n.dinner_burnt" : taste > 0 ? "cook.n.dinner_loved" : "cook.n.dinner", ("guest", target.Name),
                        ("item", LocText.Of(ContentKeys.Name(dish)))), actor);
                    break;
                }
            }
            AddAffection?.Invoke(actor.Id, target.Id, affection);
            if (romantic && action != SocialAction.InviteDinner)
            {
                foreach (var c in both) c.AddMoodlet("cook.together", Text(LocText.Of("cook.mood.together")), action == SocialAction.Picnic ? 5 : 3, 1.0);
                var villager = target.IsVillager ? target : actor.IsVillager ? actor : null;
                if (villager != null) AddRomance?.Invoke(villager, romance);
            }
            else if (action == SocialAction.Picnic || action == SocialAction.EveningWalk)
            {
                // Piknik ve yürüyüş, hediyeden sonra ikinci romantik yakınlık kaynağı (62-S9): uygun köylüyle yakınlık artar.
                var villager = target.IsVillager ? target : null;
                if (villager != null && RomanceOf != null && RomanceOf(villager) >= 0) AddRomance?.Invoke(villager, romance / 2);
            }
            Happened?.Invoke(actor, target, action);
        }

        // ---- pazar tezgâhı ----

        public bool StallOpen => _stallDay == (long)Math.Floor(Now()) && _stall.Count > 0;
        public int StallSeller => _stallSeller;
        public string VillageFavorite => _favoriteUntil >= (long)Math.Floor(Now()) ? _villageFavorite : null;

        public string StallBlocker(Character seller)
        {
            if (MarketDay != null && !MarketDay()) return Text(LocText.Of("cook.stall.block.not_market"));
            if (seller == null || seller.Stage < AgeStage.Teen) return Text(LocText.Of("cook.stall.block.seller"));
            if (StallOpen) return Text(LocText.Of("cook.stall.block.open"));
            return null;
        }

        /// <summary>Tezgâha konabilecek pişmiş yemekler (envanterdeki porsiyonlar).</summary>
        public List<(string dish, int count)> Sellable() =>
            _house.InventoryList().Where(kv => _content.TryDish(kv.Key, out var d) && d.IsMeal && kv.Value > 0).Select(kv => (kv.Key, kv.Value)).ToList();

        /// <summary>Tezgâhı açar: seçilen yemekler envanterden tezgâha geçer; gün sonunda <see cref="CloseStall"/>.</summary>
        public bool OpenStall(Character seller, IEnumerable<(string dish, int count)> goods)
        {
            if (StallBlocker(seller) != null || goods == null) return false;
            _stall.Clear();
            foreach (var (dish, count) in goods)
                for (int k = 0; k < count; k++)
                {
                    if (!_content.TryDish(dish, out _) || !_house.TryTake(dish, TakePolicy.Highest, out var took)) break;
                    int at = _stall.FindIndex(e => e.dish == dish && e.quality == took.Quality);
                    if (at >= 0) _stall[at] = (dish, took.Quality, _stall[at].count + 1); else _stall.Add((dish, took.Quality, 1));
                }
            if (_stall.Count == 0) return false;
            _stallSeller = seller.Id; _stallDay = (long)Math.Floor(Now());
            return true;
        }

        public static int PortionPrice(ItemDef dish, int quality, int kinds, bool favorite)
        {
            double price = Math.Max(2, dish.Price) * (quality >= ItemQuality.Superior ? 1.7 : quality >= ItemQuality.Fine ? 1.3 : 1.0);
            price *= Math.Min(1.5, 1.0 + 0.1 * (kinds - 1));
            if (favorite) price *= 1.25;
            return (int)Math.Round(price * 1.25);   // porsiyonla satış ham satıştan kârlıdır (işleme kazandırır ilkesi)
        }

        /// <summary>Gün sonu: talep kadar porsiyon satılır, kalan envantere döner; en çok satan yemek "köyün sevdiği" olur.</summary>
        public StallResult CloseStall(Character seller)
        {
            var r = new StallResult();
            if (_stall.Count == 0) return r;
            int kinds = _stall.Select(e => e.dish).Distinct().Count();
            int demand = StallBaseDemand + kinds * 2 + Math.Max(0, (Reputation?.Invoke() ?? 0) / 10) + (seller != null ? seller.Skills.GetLevel(SkillType.Charm) : 0);
            var sold = new Dictionary<string, int>(StringComparer.Ordinal);
            int good = 0;
            foreach (var e in _stall.OrderByDescending(e => e.quality))
            {
                if (!_content.Items.TryGet(e.dish, out var def)) continue;
                bool fav = VillageFavorite == e.dish;
                int n = Math.Min(e.count, Math.Max(0, demand + (fav ? 3 : 0)));
                demand -= Math.Min(n, demand);
                r.Sold += n; r.Unsold += e.count - n;
                r.Revenue += n * PortionPrice(def, e.quality, kinds, fav);
                if (e.quality >= ItemQuality.Fine) good += n;
                sold.TryGetValue(e.dish, out int s); sold[e.dish] = s + n;
                if (e.count > n) _house.AddItem(e.dish, e.count - n, e.quality);
            }
            _stall.Clear();
            _house.AddAkce(r.Revenue);
            r.Reputation = good / 5;
            if (r.Reputation > 0) AddReputation?.Invoke(r.Reputation, Text(LocText.Of("cook.rep.stall")));
            if (sold.Count > 0 && r.Sold > 0)
            {
                r.Favorite = sold.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.Ordinal).First().Key;
                _villageFavorite = r.Favorite; _favoriteUntil = (long)Math.Floor(Now()) + 7;
            }
            seller?.GainSkillXp(SkillType.Charm, 4f + r.Sold);
            Notice?.Invoke(LocText.Of("cook.n.stall_closed", ("sold", r.Sold), ("akce", r.Revenue), ("left", r.Unsold)), seller);
            return r;
        }

        /// <summary>Gün döndü: açık kalan tezgâh kapanır.</summary>
        public void OnNewDay(Func<int, Character> find)
        {
            if (_stall.Count > 0) CloseStall(find?.Invoke(_stallSeller));
        }

        // kayıt: d:<köylü>=<gün> | f:<yemek>=<gün> | s:<satıcı>:<gün>:<yemek>*<kalite>*<adet>;…
        public string[] Export()
        {
            var l = new List<string>();
            foreach (var kv in _dinnerDay) l.Add("d:" + kv.Key + "=" + kv.Value.ToString(CultureInfo.InvariantCulture));
            if (_villageFavorite != null) l.Add("f:" + _villageFavorite + "=" + _favoriteUntil.ToString(CultureInfo.InvariantCulture));
            if (_stall.Count > 0) l.Add("s:" + _stallSeller + ":" + _stallDay.ToString(CultureInfo.InvariantCulture) + ":" + string.Join(";", _stall.Select(e => e.dish + "*" + e.quality + "*" + e.count)));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _dinnerDay.Clear(); _stall.Clear(); _villageFavorite = null; _favoriteUntil = -1; _stallDay = -1;
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 3) continue;
                string body = line.Substring(2);
                if (line[0] == 'd') { var p = body.Split('='); if (p.Length == 2 && int.TryParse(p[0], out int id) && long.TryParse(p[1], out long day)) _dinnerDay[id] = day; }
                else if (line[0] == 'f') { var p = body.Split('='); if (p.Length == 2 && long.TryParse(p[1], out long until)) { _villageFavorite = p[0]; _favoriteUntil = until; } }
                else if (line[0] == 's')
                {
                    var p = body.Split(':');
                    if (p.Length != 3 || !int.TryParse(p[0], out _stallSeller) || !long.TryParse(p[1], out _stallDay)) continue;
                    foreach (var e in p[2].Split(';'))
                    {
                        var q = e.Split('*');
                        if (q.Length == 3 && int.TryParse(q[1], out int quality) && int.TryParse(q[2], out int count)) _stall.Add((q[0], quality, count));
                    }
                }
            }
        }
    }
}
