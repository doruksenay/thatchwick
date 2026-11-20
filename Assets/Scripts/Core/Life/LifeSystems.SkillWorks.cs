using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Beceri seviyesiyle açılan işler ve pasif etkiler (Project belgesi 45, 9. bölüm). İşler içerikte etkileşimin
    /// "skillWork" alanıyla çağrılır (eşya etkileşimi, beceri ve minLevel içerikte); pasifler gün başında ya da ilgili
    /// sistemin kancasında çalışır. Kalıcı durum (aşılı ağaç, stoklu gölet, kara toprak, imece) kayda "skillFlags" olarak girer.
    /// </summary>
    public sealed partial class LifeSystems
    {
        // ---- kalıcı bayraklar: "anahtar=değer" ----
        private readonly Dictionary<string, double> _skillFlags = new Dictionary<string, double>(StringComparer.Ordinal);

        public double SkillFlag(string key) => _skillFlags.TryGetValue(key, out var v) ? v : 0.0;
        public void SetSkillFlag(string key, double value) { if (value == 0) _skillFlags.Remove(key); else _skillFlags[key] = value; }
        public string[] ExportSkillFlags() => _skillFlags.Select(kv => kv.Key + "=" + kv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        public void ImportSkillFlags(string[] flags)
        {
            _skillFlags.Clear();
            if (flags == null) return;
            foreach (var f in flags)
            {
                int i = f?.IndexOf('=') ?? -1;
                if (i <= 0) continue;
                if (double.TryParse(f.Substring(i + 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                    _skillFlags[f.Substring(0, i)] = v;
            }
        }

        private int Today => (int)Math.Floor(Clock.TotalDays);

        // ---- sabitler ----
        public const double GiantCropChance = 0.06;
        public const int ImeceCooldownDays = 7, ImeceReputation = 100;
        public const double PruneFruitShare = 0.30;
        public const int GraftBonus = 2;
        public const int StockedPondDays = 10;
        public const double LegendFishChance = 0.04;
        public const double BadMushroomChance = 0.12;
        public const int ForestSecretEvery = 7;
        public const double BearingSpeed = 1.15, ClimbSpeed = 1.25;
        public const int GuestRoomEvery = 7, HouseTourEvery = 7, HouseTourBeauty = 3;
        public const float GreenManureFertility = 0.4f;
        public const string HundRailId = "item.hund_rail", WindlassId = "item.windlass", MineCandleId = "item.mine_candle", MasterLanternId = "item.master_lantern";

        /// <summary>Maden çatırtı çarpanı: usta madencinin feneri ×0,5, maden mumu ×0,8 (MineSystem.CreakFactor).</summary>
        public double MineCreakFactor() => (House.Count(MasterLanternId) > 0 ? 0.5 : 1.0) * (House.Count(MineCandleId) > 0 ? 0.8 : 1.0);
        public const int GardenContestPrize = 20, GardenContestReputation = 5;

        /// <summary>Toplu ek: tohumu bir karoya eker (BuildService.Plant; oyun katmanı bağlar). Ekildiyse true.</summary>
        public Func<string, GridPos, bool> PlantSeed { get; set; }

        /// <summary>Bir ekinin tohumu (plantsAs ile).</summary>
        public string SeedOf(ItemDef plant) => Content.Items.All.FirstOrDefault(d => d.PlantsAs == plant?.Id && d.Placement == ItemPlacement.Resource)?.Id;

        /// <summary>Hesap defteri (Okuryazarlık 9): gelir-gider ve kış hazırlığı tahmini.</summary>
        public string LedgerText()
        {
            var rep = Forecast.Report(Clock.TotalDays);
            int daysToWinter = Clock.Season == Season.Winter ? 0 : (3 - (int)Clock.Season) * Clock.Config.DaysPerSeason + (Clock.Config.DaysPerSeason - (int)(Clock.DayIndex % Clock.Config.DaysPerSeason));
            string food = rep.FoodDays >= daysToWinter + Clock.Config.DaysPerSeason ? "kışa yeter" : $"kışa {Math.Max(0, daysToWinter + Clock.Config.DaysPerSeason - rep.FoodDays):0} günlük yiyecek eksik";
            return $"Hesap defteri: kasa {House.Akce} akçe; yiyecek {Math.Max(0, rep.FoodDays):0} gün ({food}), odun {Math.Max(0, rep.FirewoodDays):0} gün; kışa {daysToWinter} gün.";
        }

        // Ürün kimlikleri (items.json).
        public const string BaitId = "item.bait", NetId = "item.fishing_net", LegendFishId = "item.legend_fish", TruffleId = "item.truffle",
            AlmanacId = "item.folk_almanac", StormBellId = "item.storm_bell", RepellentId = "item.insect_repellent",
            CoveredCartId = "item.covered_cart", WheelbarrowId = "item.wheelbarrow", HorseCartId = "item.horse_cart", SledId = "item.sled",
            RaceSaddleId = "item.race_saddle", SealingWaxId = "item.sealing_wax", BadMushroomMood = "bad_mushroom";

        // ================= iş engeli ve etkisi (InteractionEffects) =================

        /// <summary>"skillWork" işinin engeli (yoksa null).</summary>
        public string SkillWorkBlocker(Character c, ItemInstance item, InteractionDef i)
        {
            if (i?.SkillWork == null || item == null) return null;
            switch (i.SkillWork)
            {
                case "imece":
                    if (Today - SkillFlag("imece") < ImeceCooldownDays && SkillFlag("imece") > 0) return $"İmece haftada bir çağrılır ({ImeceCooldownDays - (Today - (int)SkillFlag("imece"))} gün sonra).";
                    if (Upbringing.Reputation.Points < ImeceReputation) return $"İmece için itibar {ImeceReputation} gerekli.";
                    if (c.Skills.GetLevel(SkillType.Farming) < 9 && c.Skills.GetLevel(SkillType.Stewardship) < 7) return "Çiftçilik 9 ya da Kâhyalık 7 gerekli.";
                    return null;
                case "prune":
                    if (!item.IsGrown) return "Ağaç henüz büyümedi.";
                    if (item.FruitReady) return "Meyvesi olgun: önce topla.";
                    if (SkillFlag("prune:" + item.Id) >= Today - 20 && SkillFlag("prune:" + item.Id) > 0) return "Bu yıl budandı.";
                    return null;
                case "graft":
                    if (!item.IsGrown) return "Ağaç henüz büyümedi.";
                    if (SkillFlag("graft:" + item.Id) > 0) return "Bu ağaç zaten aşılı.";
                    return GraftScion(item) == null ? "Aşı için envanterde başka bir meyve fidanı gerekli." : null;
                case "stock_pond":
                    if (SkillFlag("stock:" + item.Id) > Today) return "Gölet zaten stoklu.";
                    return null;
                case "black_earth":
                    if (SkillFlag("black_earth") > 0 && Today - SkillFlag("black_earth") < 40) return "Kara toprak yılda bir hazırlanır.";
                    return null;
                case "feast":
                    return FeastFoods(4).Count < 4 ? "Ziyafet için 4 çeşit yiyecek gerekli." : null;
                case "contract":
                    return SkillFlag("contract") == Today ? "Bugün sözleşme yazıldı." : House.Count(SealingWaxId) + House.Count("item.parchment") <= 0 ? "Parşömen ya da mühür mumu gerekli." : null;
                case "guest_room":
                    return Today - SkillFlag("guest_room") < GuestRoomEvery && SkillFlag("guest_room") > 0 ? "Misafir odası bu hafta hazırlandı." : null;
                case "branch":
                    return SkillFlag("branch:" + item.Id) == Today ? "Bu ağacın dalları bugün budandı." : !item.IsGrown ? "Ağaç henüz büyümedi." : null;
                case "resin":
                    return Today - SkillFlag("resin:" + item.Id) < 3 && SkillFlag("resin:" + item.Id) > 0 ? "Reçine yeniden birikiyor (3 günde bir)." : !item.IsGrown ? "Ağaç henüz büyümedi." : null;
                case "heritage":
                    return !item.IsGrown ? "Ağaç henüz büyümedi." : SkillFlag("heritage:" + item.Id) > 0 ? "Bu zaten yadigâr ağaç." : null;
                case "bulk_sow":
                    return PlantSeed == null ? "Toplu ek kullanılamıyor." : SeedOf(item.Def) == null || House.Count(SeedOf(item.Def)) <= 0 ? "Bu ekinin tohumu envanterde yok." : null;
                case "green_manure":
                    return !item.IsGrown ? "Fiğ henüz büyümedi." : null;
                case "watch_wild":
                    return SkillFlag("watch") == Today ? "Bugün gözlem yapıldı." : null;
                case "hunt_track":
                    return null;
                case "stroll":
                    return null;
            }
            return null;
        }

        /// <summary>"skillWork" işinin etkisi.</summary>
        public void OnSkillWorkApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (i == null || item == null) return;
            if (i.SkillWork != null) ApplySkillWork(c, item, i);
            // Pasifler: hasat, kesim, meyve, mantar, balık.
            if (i.Skill == SkillType.Farming && i.RemovesItem && item.Def.IsCrop && i.Id == "harvest") OnCropHarvest(c, item);
            if (i.Id == "chop" && i.Skill == SkillType.Woodcutting) OnTreeChopped(c, item);
            if (i.TakesFruit) OnFruitPicked(c, item);
            if (i.Skill == SkillType.Foraging && item.Def.Id == "item.forage_mushroom") OnMushroomGathered(c);
            if (i.Skill == SkillType.Fishing && item.Def.WaterSource) OnFished(c, item);
            // Değirmen +1 parti (Mühendislik 6): öğütme bir ölçü fazla çıkar.
            if (i.Id.StartsWith("grind") && i.Produce.Count > 0 && BestSkill(SkillType.Engineering) >= 6) House.AddItem(i.Produce[0].ItemId, 1);
            if (i.Skill == SkillType.Cooking && i.Id.StartsWith("smoke_") && c.Skills.GetLevel(SkillType.Cooking) >= 6 && _masteryRng.NextDouble() < 0.5 && i.Produce.Count > 0)
            { House.AddItem(i.Produce[0].ItemId, 1); Say($"{c.Name}'in tütsüsü bol çıktı (+1).", NoticeLevel.Info, c.Id, transient: true); }
        }

        private void ApplySkillWork(Character c, ItemInstance item, InteractionDef i)
        {
            switch (i.SkillWork)
            {
                case "inspect_field":
                {
                    var s = _plants?.Summary() ?? default;
                    float fert = _plants != null ? _plants.FertilityAt(item.Position) : 0.5f;
                    Say($"{c.Name} tarlayı inceledi: {s.Crops} ekin, {s.Thirsty} susuz, {s.Ripe} olgun; bu karonun verimi %{fert * 100:0}.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "smell_soil":
                {
                    double left = Math.Max(0, item.Def.GrowDays - item.GrowthDays);
                    int q = _plants != null ? _plants.QualityOf(item, c.Skills.GetLevel(SkillType.Farming)) : 0;
                    string tier = q >= 2 ? PlantSystem.QualityFinest : q == 1 ? PlantSystem.QualityFine : PlantSystem.QualityNormal;
                    Say($"{c.Name} toprağı kokladı: {item.Def.DisplayName} {left:0.#} günde olgunlaşır; ürün \"{tier}\" görünüyor.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "soil_check":
                {
                    float fert = _plants != null ? _plants.FertilityAt(item.Position) : 0.5f;
                    string last = _plants?.LastCropAt(item.Position);
                    string hint = _plants != null ? _plants.RotationHint(item.Position, item.Def) : null;
                    Say($"{c.Name} toprağa baktı: verim %{fert * 100:0}" + (last != null && Content.Items.TryGet(last, out var ld) ? $", önceki ekin {ld.DisplayName}" : "") + (hint != null ? $"; {hint}" : ""), NoticeLevel.Info, c.Id);
                    break;
                }
                case "imece":
                {
                    SetSkillFlag("imece", Today);
                    int watered = 0, grown = 0;
                    foreach (var it in World.Items)
                    {
                        if (!it.IsPlant || !it.Def.IsCrop) continue;
                        if (!it.IsGrown && !it.Watered) { PlantSystem.Water(it); watered++; }
                        if (!it.IsGrown) { it.GrowthDays = Math.Min(it.Def.GrowDays, it.GrowthDays + 1.0); grown++; }
                    }
                    Upbringing.Reputation.Add(2, "imece");
                    Say($"İmece: komşular tarlaya geldi; {watered} ekin sulandı, {grown} ekin bir gün ilerledi.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "prune":
                {
                    SetSkillFlag("prune:" + item.Id, Today);
                    if (item.Def.FruitRipenDays > 0) item.FruitDays = Math.Min(item.Def.FruitRipenDays, item.FruitDays + item.Def.FruitRipenDays * PruneFruitShare);
                    Say($"{c.Name} {item.Def.DisplayName.ToLowerInvariant()} budadı: meyvesi daha çabuk olgunlaşacak.", NoticeLevel.Info, c.Id, transient: true);
                    break;
                }
                case "graft":
                {
                    var scion = GraftScion(item);
                    if (scion == null || !House.TryTakeItem(scion.Value.sapling)) break;
                    SetSkillFlag("graft:" + item.Id, 1);
                    _graftFruit[item.Id] = scion.Value.fruit;
                    SetSkillFlag("graftfruit:" + item.Id + ":" + scion.Value.fruit, 1);
                    Say($"{c.Name} {item.Def.DisplayName.ToLowerInvariant()} aşıladı: artık {Name(scion.Value.fruit).ToLowerInvariant()} de verecek.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "black_earth":
                {
                    SetSkillFlag("black_earth", Today);
                    if (_plants != null)
                        for (int dx = -1; dx <= 1; dx++)
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                var p = new GridPos(item.Position.X + dx, item.Position.Y + dy, item.Position.Z);
                                _plants.SetSoil(p, 1.0f, _plants.LastCropAt(p));
                                SetSkillFlag($"rich:{p.X},{p.Y}", 1);
                            }
                    Say($"{Progress.Mastery.DisplayName(c)} çevreyi kara toprağa çevirdi: 3×3 alan kalıcı verimli.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "track":
                {
                    var rare = World.Items.Where(x => x.Def.Category == "forage" && (x.Def.Id == "item.forage_truffle" || x.Def.Id == "item.forage_hive" || x.Def.Id == "item.forage_chestnut"))
                        .OrderBy(x => Math.Abs(x.Position.X - c.Position.X) + Math.Abs(x.Position.Y - c.Position.Y)).Take(3).ToList();
                    Say(rare.Count == 0 ? $"{c.Name} iz sürdü ama nadir bir şey bulamadı." : $"{c.Name} iz sürdü: " + string.Join(", ", rare.Select(r => $"{r.Def.DisplayName} ({Direction(c.Position, r.Position)})")), NoticeLevel.Info, c.Id);
                    break;
                }
                case "forage_regrow":
                {
                    var seasons = item.Def.ForageSeasons;
                    Say($"{item.Def.DisplayName}: " + (seasons != null && seasons.Count > 0 ? $"{string.Join(", ", seasons.Select(SeasonName))} mevsiminde, yağmurdan sonra yeniden çıkar." : "her mevsim yeniden çıkar."), NoticeLevel.Info, c.Id);
                    break;
                }
                case "sky_look":
                {
                    var t = Weather.Today(Clock.TotalDays); var tm = Weather.Tomorrow(Clock.TotalDays);
                    string later = Clock.DayFraction < 0.5 && t != WeatherType.Clear ? "öğleden sonra da sürecek" : t == WeatherType.Clear && tm != WeatherType.Clear ? "akşama doğru bulutlanacak" : "gün böyle kalacak";
                    Say($"{c.Name} gökyüzüne baktı: {WeatherSystem.Name(t).ToLowerInvariant()}, {later}; yarın {WeatherSystem.Name(tm).ToLowerInvariant()}.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "taste":
                {
                    int cook = c.Skills.GetLevel(SkillType.Cooking);
                    string q = cook >= 9 ? "usta işi" : cook >= 7 ? "lezzetli" : "sıradan";
                    int herbs = House.Count("item.herb") + House.Count("item.dill") + House.Count("item.thyme");
                    Say($"{c.Name} tadına baktı: bugünkü yemek {q} olacak" + (herbs > 0 ? "; ot eklenirse bir basamak daha iyi." : "; ot yok."), NoticeLevel.Info, c.Id);
                    break;
                }
                case "feast":
                {
                    var foods = FeastFoods(4);
                    foreach (var f in foods) House.TryTakeItem(f);
                    foreach (var m in Household()) { m.Needs.Restore(NeedType.Hunger, 60f); m.Needs.Restore(NeedType.Social, 30f); m.AddMoodlet(MoodCatalog.Feast); }
                    Say($"{c.Name} ziyafet sofrası kurdu; hane bir arada doydu.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "tidy":
                {
                    var room = World.Map.Rooms.RoomAt(item.Position);
                    int n = 0;
                    if (room != null)
                    {
                        var tiles = new HashSet<GridPos>(room.Tiles);
                        foreach (var it in World.Items)
                            if (tiles.Contains(it.Position) && it.HasContents && it.Def.HasTag("clutter"))
                            {
                                foreach (var s in it.Contents) House.AddItem(s.ItemId);
                                it.Contents.Clear();
                                n++;
                            }
                    }
                    foreach (var m in Household()) if (room != null && World.Map.Rooms.RoomAt(m.Position) == room) m.Needs.Restore(NeedType.Fun, 5f);
                    Say(n > 0 ? $"{c.Name} odayı toparladı ({n} döküntü kaldırıldı)." : $"{c.Name} odayı toparladı; derli toplu.", NoticeLevel.Info, c.Id, transient: true);
                    break;
                }
                case "guest_room":
                    SetSkillFlag("guest_room", Today);
                    foreach (var m in Household()) m.AddMoodlet(MoodCatalog.Harmony);
                    Upbringing.Reputation.Add(1, "misafir odası");
                    Say($"{c.Name} misafir odasını hazırladı; gelen konuk rahat edecek (itibar +1).", NoticeLevel.Info, c.Id);
                    break;
                case "contract":
                    if (!House.TryTakeItem(SealingWaxId)) House.TryTakeItem("item.parchment");
                    SetSkillFlag("contract", Today);
                    Say($"{c.Name} sözleşme yazdı: bugün köy işlerinin ödülü %15 fazla.", NoticeLevel.Info, c.Id);
                    break;
                case "stock_pond":
                    SetSkillFlag("stock:" + item.Id, Today + StockedPondDays);
                    Say($"{c.Name} göleti yavru balıkla stokladı ({StockedPondDays} gün balık bol).", NoticeLevel.Info, c.Id);
                    break;
                case "chronicle":
                    Upbringing.Reputation.Add(2, "kronik");
                    foreach (var m in Household()) m.AddChronicle($"{c.Name} hanenin kroniğini yazdı (Yıl {Clock.YearIndex + 1}).");
                    break;
                case "weekly_plan":
                    Say(WeeklyPlanText(), NoticeLevel.Info, c.Id);
                    break;
                case "branch":
                    SetSkillFlag("branch:" + item.Id, Today);
                    break;
                case "resin":
                    SetSkillFlag("resin:" + item.Id, Today);
                    break;
                case "heritage":
                {
                    SetSkillFlag("heritage:" + item.Id, Clock.YearIndex + 1);
                    string note = $"{c.Name} {item.Def.DisplayName.ToLowerInvariant()} yadigâr ağaç ilan etti; torunlara kalacak.";
                    foreach (var m in Household()) m.AddChronicle(note);
                    Upbringing.Reputation.Add(1, "yadigâr ağaç");
                    Say(note, NoticeLevel.Info, c.Id);
                    break;
                }
                case "bulk_sow":
                {
                    string seed = SeedOf(item.Def);
                    int sown = 0;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if ((dx == 0 && dy == 0) || House.Count(seed) <= 0) continue;
                            if (PlantSeed(seed, new GridPos(item.Position.X + dx, item.Position.Y + dy, item.Position.Z))) sown++;
                        }
                    Say(sown > 0 ? $"{c.Name} çevreye {sown} {item.Def.DisplayName.ToLowerInvariant()} ekti." : "Çevrede ekilecek boş tarla yok (önce toprağı çapala).", NoticeLevel.Info, c.Id, transient: true);
                    break;
                }
                case "green_manure":
                    if (_plants != null)
                    {
                        float f = _plants.FertilityAt(item.Position);
                        _plants.SetSoil(item.Position, Math.Min(1f, f + GreenManureFertility), _plants.LastCropAt(item.Position));
                    }
                    Say($"{c.Name} fiği toprağa gömdü: bu karonun verimi arttı.", NoticeLevel.Info, c.Id, transient: true);
                    break;
                case "hunt_track":
                {
                    var seen = Animals.WildCatalog.All.Where(s => s.Huntable && (s.Seasons == null || s.Seasons.Length == 0 || s.Seasons.Contains(Clock.Season))).Take(4)
                        .Select(s => Animals.Hunting.TrackText(s.Id));
                    Say($"{c.Name} izleri okudu: " + string.Join(" · ", seen), NoticeLevel.Info, c.Id);
                    break;
                }
                case "watch_wild":
                {
                    SetSkillFlag("watch", Today);
                    var cands = Animals.WildCatalog.All.Where(s => (s.Seasons == null || s.Seasons.Length == 0 || s.Seasons.Contains(Clock.Season)) && (Journal.Get(s.Id) & Animals.JournalMark.Seen) == 0).ToList();
                    if (cands.Count == 0) { Say($"{c.Name} gözledi; bu mevsim deftere yeni bir tür eklenmedi.", NoticeLevel.Info, c.Id, transient: true); break; }
                    var sp = cands[_masteryRng.Next(cands.Count)];
                    Journal.Mark(sp.Id, Animals.JournalMark.Seen);
                    Say($"{c.Name} gözledi: {sp.Name} Doğa Defteri'ne girdi.", NoticeLevel.Info, c.Id);
                    break;
                }
                case "stroll":
                    c.AddMoodlet(MoodCatalog.Harmony);
                    break;
                case "ledger":
                    Say(LedgerText(), NoticeLevel.Info, c.Id);
                    break;
                case "storm_bell":
                    break;
            }
        }

        // ================= pasifler =================

        private void OnCropHarvest(Character c, ItemInstance crop)
        {
            if (c == null) return;
            int lv = c.Skills.GetLevel(SkillType.Farming);
            var harvest = crop.Def.Interactions.FirstOrDefault(x => x.Id == "harvest");
            string main = harvest?.Produce.FirstOrDefault()?.ItemId;
            if (lv >= 8 && main != null && _masteryRng.NextDouble() < GiantCropChance)
            {
                House.AddItem(main, harvest.Produce[0].Count);
                c.AddMoodlet(MoodCatalog.GoodHarvest);
                Say($"{c.Name} iri bir {Name(main).ToLowerInvariant()} hasadı çıkardı (iki kat).", NoticeLevel.Info, c.Id);
            }
            if (lv >= Progress.Mastery.MasterLevel)
            {
                var seed = Content.Items.All.FirstOrDefault(d => d.PlantsAs == crop.Def.Id);
                if (seed != null) House.AddItem(seed.Id, 1);   // soy tohumu: usta çiftçi her hasatta bir tohum fazla ayırır
            }
            // Kara toprak kalıcıdır: hasat verimi düşürmez.
            if (_plants != null && SkillFlag($"rich:{crop.Position.X},{crop.Position.Y}") > 0) _plants.SetSoil(crop.Position, 1.0f, crop.Def.Id, _plants.LastCropAt(crop.Position));
        }

        private void OnTreeChopped(Character c, ItemInstance tree)
        {
            if (c == null || tree.Def.FruitSeasons.Count > 0) return;
            if (c.Skills.GetLevel(SkillType.Woodcutting) < 7) return;
            // Koru: usta oduncu kestiği orman ağacının yerine fidan diker.
            if (World.CanPlaceFloor(tree.Def, tree.Position, 0).Ok)
            {
                World.PlaceItem(tree.Def, tree.Position);
                Say($"{c.Name} kestiği {tree.Def.DisplayName.ToLowerInvariant()} ağacının yerine fidan dikti.", NoticeLevel.Info, c.Id, transient: true);
            }
        }

        /// <summary>Yadigâr ağaç: Odunculuk 9+ olan keserken uyarır (engel).</summary>
        public string ChopBlocker(Character c, ItemInstance item, InteractionDef i)
        {
            if (i?.Id != "chop" || item == null || c == null) return null;
            bool heritage = item.Def.Id == OliveHeritage.TreeId && Olives.IsHeirloom(item.Id) || SkillFlag("graft:" + item.Id) > 0;
            if (heritage && c.Skills.GetLevel(SkillType.Woodcutting) >= 9) return "Bu bir yadigâr ağaç; usta oduncu kesmez.";
            return null;
        }

        private readonly Dictionary<int, string> _graftFruit = new Dictionary<int, string>();

        private void OnFruitPicked(Character c, ItemInstance tree)
        {
            string fruit = GraftFruitOf(tree.Id);
            if (fruit != null) { House.AddItem(fruit, GraftBonus); Say($"Aşılı ağaç {GraftBonus} {Name(fruit).ToLowerInvariant()} de verdi.", NoticeLevel.Info, c?.Id ?? 0, transient: true); }
            if (c != null && c.Skills.GetLevel(SkillType.Athletics) >= 4) c.GainSkillXp(SkillType.Athletics, 1);
        }

        private string GraftFruitOf(int itemId)
        {
            if (_graftFruit.TryGetValue(itemId, out var f)) return f;
            string prefix = "graftfruit:" + itemId + ":";
            foreach (var k in _skillFlags.Keys) if (k.StartsWith(prefix, StringComparison.Ordinal)) { f = k.Substring(prefix.Length); _graftFruit[itemId] = f; return f; }
            return null;
        }

        /// <summary>Aşı kalemi: envanterde bu ağaçtan başka bir meyve ağacının fidanı → (fidan, meyve).</summary>
        private (string sapling, string fruit)? GraftScion(ItemInstance tree)
        {
            foreach (var d in Content.Items.All)
            {
                if (d.PlantsAs == null || d.PlantsAs == tree.Def.Id || House.Count(d.Id) <= 0) continue;
                if (!Content.Items.TryGet(d.PlantsAs, out var other) || other.FruitSeasons.Count == 0) continue;
                var pick = other.Interactions.FirstOrDefault(x => x.TakesFruit && x.Produce.Count > 0);
                if (pick != null) return (d.Id, pick.Produce[0].ItemId);
            }
            return null;
        }

        private void OnMushroomGathered(Character c)
        {
            if (c == null || c.Skills.GetLevel(SkillType.Foraging) >= 5 || _masteryRng.NextDouble() >= BadMushroomChance) return;
            c.Mood.Add(new MoodletDef { Id = BadMushroomMood, DisplayName = "Mide bozuk", Description = "Zehirli bir mantarı fark etmeden tattı (Toplayıcılık 5 ayırt eder).", Value = -8, DurationDays = 1, EnergyDecay = 1.2f });
            Say($"{c.Name} zehirli bir mantarı yenilebilir sandı; midesi bozuldu.", NoticeLevel.Warning, c.Id);
        }

        private void OnFished(Character c, ItemInstance pond)
        {
            if (c == null) return;
            int lv = c.Skills.GetLevel(SkillType.Fishing);
            string fish = null;
            var fishInt = pond.Def.Interactions.FirstOrDefault(x => x.Id == "fish");
            if (fishInt != null)
                foreach (var p in fishInt.Produce)
                    if ((p.MinLevel <= lv) && (p.Seasons == null || p.Seasons.Count == 0 || p.Seasons.Contains(Clock.Season))) { fish = p.ItemId; break; }
            if (fish == null) return;
            int extra = 0;
            if (lv >= 2 && House.TryTakeItem(BaitId)) extra += _masteryRng.NextDouble() < 0.5 ? 1 : 0;     // yem
            if (lv >= 4 && House.Count(NetId) > 0 && _masteryRng.NextDouble() < 0.4) extra++;                // büyük ağ
            if (SkillFlag("stock:" + pond.Id) > Today) extra++;                                                  // stoklu gölet
            if (extra > 0) House.AddItem(fish, extra);
            if (lv >= Progress.Mastery.MasterLevel && SkillFlag("legend:" + pond.Id) < Clock.YearIndex + 1 && _masteryRng.NextDouble() < LegendFishChance && Content.Items.Contains(LegendFishId))
            {
                SetSkillFlag("legend:" + pond.Id, Clock.YearIndex + 1);
                House.AddItem(LegendFishId, 1);
                string note = $"{Progress.Mastery.DisplayName(c)} gölün efsane balığını tuttu!";
                Say(note, NoticeLevel.Info, c.Id);
                foreach (var m in Household()) m.AddChronicle(note);
                Upbringing.Reputation.Add(5, "efsane balık");
            }
        }

        /// <summary>İş hızına beceri pasifleri: yönlü devir (Odunculuk 4), ağaca tırman (Atletizm 4).</summary>
        private double SkillWorkSpeed(Character c, InteractionDef i)
        {
            double f = 1.0;
            if (i.Id == "chop" && c.Skills.GetLevel(SkillType.Woodcutting) >= 4) f *= BearingSpeed;
            if (i.TakesFruit && c.Skills.GetLevel(SkillType.Athletics) >= 4) f *= ClimbSpeed;
            // Ağır yük taşı (Atletizm 8): kesim, kazı, taş işi %10 hızlı.
            if (c.Skills.GetLevel(SkillType.Athletics) >= 8 && (i.Id == "chop" || i.Skill == SkillType.Mining || i.Skill == SkillType.Masonry)) f *= 1.10;
            // Hund rayı (Mühendislik 6): maden işi %20 hızlı.
            if (i.Skill == SkillType.Mining && House.Count(HundRailId) > 0) f *= 1.20;
            // Bocurgat ve kova dolabı (Mühendislik 7): su taşıyan iş %30 hızlı.
            if (i.FetchWater && House.Count(WindlassId) > 0) f *= 1.30;
            return f;
        }

        /// <summary>Gün başı beceri pasifleri (OnNewDay).</summary>
        private void SkillDaily(long day)
        {
            int weather = BestSkill(SkillType.WeatherLore);
            var tomorrow = Weather.Tomorrow(Clock.TotalDays);
            int dayInSeason = (int)(day % Clock.Config.DaysPerSeason);
            if (weather >= 3 && (tomorrow == WeatherType.Snow || (tomorrow != WeatherType.Clear && (Clock.Season == Season.Winter || (Clock.Season == Season.Spring && dayInSeason < 2)))))
                Say("Hava bilgisi: yarın gece don olabilir; ekinleri ve hayvanları koru.", NoticeLevel.Warning);
            if (weather >= 4 && tomorrow == WeatherType.Storm) Say("Hava bilgisi: yarın fırtına; hayvanları içeri al, ekinler hasar görebilir.", NoticeLevel.Warning);
            if (weather >= 5)
            {
                if (World.Items.Any(x => x.Def.Id == "item.crop_saffron" && x.IsGrown)) Say("Şafak: safran açtı; öğlene kadar topla, kalitesi düşmesin.", NoticeLevel.Info);
                if (dayInSeason == 0) Say(PlantingCalendar(), NoticeLevel.Info);
            }
            if (weather >= 7 && Clock.Season == Season.Autumn && dayInSeason == 0) Say(WinterOutlook(day), NoticeLevel.Info);
            // Ormanın sırrı: usta toplayıcı haftada bir nadir buluntu.
            if (BestSkill(SkillType.Foraging) >= Progress.Mastery.MasterLevel && day % ForestSecretEvery == 0)
            {
                string find = Clock.Season == Season.Autumn && Content.Items.Contains(TruffleId) ? TruffleId : Content.Items.Contains("item.honey") ? "item.honey" : null;
                if (find != null) { House.AddItem(find, 1); Say($"Usta toplayıcı ormandan nadir bir buluntuyla döndü: {Name(find)}.", NoticeLevel.Info); }
            }
            // Sabah koşusu (Atletizm 9).
            foreach (var m in Household())
                if (m.Skills.GetLevel(SkillType.Athletics) >= 9 && m.Stage >= AgeStage.Teen)
                {
                    m.Needs.Restore(NeedType.Energy, 10f);
                    m.Needs.Restore(NeedType.Fun, 10f);
                    m.GainSkillXp(SkillType.Athletics, 2);
                }
            // Ev gezisi (Ev düzenleme 10): güzel bir evi görmeye haftada bir köylüler gelir.
            if (BestSkill(SkillType.Decorating) >= Progress.Mastery.MasterLevel && day % HouseTourEvery == 0)
            {
                Upbringing.Reputation.Add(HouseTourBeauty, "ev gezisi");
                Say("Köylüler evi görmeye geldi; düzeninden söz ettiler (itibar +3).", NoticeLevel.Info);
            }
            // Irgat (Kâhyalık 3) ve yarıcılık (Kâhyalık 5); satış rotası (8).
            DailyHired(day);
            RunSalesRoute(day);
            // Fırtına çanı (Hava bilgisi 8): yarın fırtınaysa çan çalınır, köy hazırlanır.
            if (tomorrow == WeatherType.Storm && World.Items.Any(x => x.Def.Id == StormBellId))
            {
                Upbringing.Reputation.Add(1, "fırtına çanı");
                Say("Fırtına çanı çalındı: köy yarınki fırtınaya hazırlanıyor (itibar +1).", NoticeLevel.Info);
            }
            // Şenlik havası (Hava bilgisi 9): üç gün içindeki şenliğin havası.
            if (weather >= 9)
            {
                var fest = Festivals.NextFestival(out int fd);
                if (fest != null && fd >= 1 && fd <= 3) Say($"Hava bilgisi: {fest.DisplayName} günü hava {WeatherSystem.Name(Weather.WeatherOn(day + fd)).ToLowerInvariant()} olacak.", NoticeLevel.Info);
            }
            // Toprak bakımı 5: nadasa bırakılan toprak iki kat hızlı dinlenir.
            if (_plants != null) _plants.Config.FallowRecovery = BestSkill(SkillType.SoilCare) >= 5 ? 0.04f : 0.02f;
            // Bahçe yarışması (Bahçıvanlık 10): şenlik günü bahçe yarışmasında derece.
            if (BestSkill(SkillType.Horticulture) >= Progress.Mastery.MasterLevel && (Festivals.FestivalOn(day) != null))
            {
                int trees = World.Items.Count(x => x.IsPlant && x.Def.IsFruitTree && x.IsGrown);
                int flowers = World.Items.Count(x => x.Def.Id == "item.flower_bed");
                if (trees + flowers >= 3)
                {
                    House.AddAkce(GardenContestPrize);
                    Upbringing.Reputation.Add(GardenContestReputation, "bahçe yarışması");
                    Say($"Bahçe yarışması: hanenin bahçesi birinci oldu (+{GardenContestPrize} akçe).", NoticeLevel.Info);
                }
            }
            // Şenlik mumu (Mumculuk 8): şenlik günü yakılır, hane neşelenir.
            if ((Festivals.FestivalOn(day) != null) && House.TryTakeItem("item.festival_candle"))
                foreach (var m in Household()) m.AddMoodlet(MoodCatalog.Feast);
            // Köy okulu (Öğretmenlik 10): hanenin çocukları her gün okulda okuryazarlık öğrenir.
            if (BestSkill(SkillType.Teaching) >= Progress.Mastery.MasterLevel)
                foreach (var m in Household())
                    if (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) m.GainSkillXp(SkillType.Literacy, 3);
            // Böcek kovucu kullanılır: haftada bir tane.
            if (day % 7 == 0 && House.Count(RepellentId) > 0) House.TryTakeItem(RepellentId);
        }

        private string PlantingCalendar()
        {
            var crops = Content.Items.All.Where(d => d.IsCrop && PlantSystem.CanGrow(d, Clock.Season) && !d.SouthOnly).Select(d => d.DisplayName.Replace(" ekini", "")).Take(8).ToList();
            return crops.Count == 0 ? "Ekim takvimi: bu mevsim ekilecek ekin yok." : "Ekim takvimi: bu mevsim " + string.Join(", ", crops) + " ekilir.";
        }

        private string WinterOutlook(long day)
        {
            long start = day + Clock.Config.DaysPerSeason;
            int snow = 0, storm = 0;
            for (int d = 0; d < Clock.Config.DaysPerSeason; d++)
            {
                var w = Weather.WeatherOn(start + d);
                if (w == WeatherType.Snow) snow++;
                if (w == WeatherType.Storm) storm++;
            }
            string how = snow + storm >= Clock.Config.DaysPerSeason / 2 ? "sert" : snow + storm <= 2 ? "ılık" : "olağan";
            return $"Hava bilgisi: bu kış {how} geçecek ({snow} karlı, {storm} fırtınalı gün bekleniyor).";
        }

        // ---- yardımcılar ----

        private string Name(string id) => Content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        private static string SeasonName(Season s) => s == Season.Spring ? "ilkbahar" : s == Season.Summer ? "yaz" : s == Season.Autumn ? "sonbahar" : "kış";

        private static string Direction(GridPos from, GridPos to)
        {
            int dx = to.X - from.X, dy = to.Y - from.Y;
            string ns = dy < -2 ? "kuzey" : dy > 2 ? "güney" : "";
            string ew = dx > 2 ? "doğu" : dx < -2 ? "batı" : "";
            string dir = (ns + (ns.Length > 0 && ew.Length > 0 ? "" : "") + ew);
            return (dir.Length == 0 ? "hemen yakında" : dir + "da") + $", {Math.Abs(dx) + Math.Abs(dy)} karo";
        }

        private List<string> FeastFoods(int n)
        {
            var list = new List<string>();
            foreach (var kv in House.InventoryList())
                if (kv.Value > 0 && Content.Items.TryGet(kv.Key, out var d) && d.HasTag("food") && !d.HasTag("raw") && !list.Contains(kv.Key)) { list.Add(kv.Key); if (list.Count >= n) break; }
            return list;
        }

        /// <summary>Haftalık plan (Kâhyalık 4): görevler, olgunlaşacak ekin, pazar ve şenlik günleri, kış stoğu.</summary>
        public string WeeklyPlanText()
        {
            var parts = new List<string>();
            var duties = Household().Where(m => m.Duty != DutyKind.None).Select(m => $"{m.Name}: {Work.DutyPlanner.DutyName(m.Duty)}").ToList();
            parts.Add(duties.Count > 0 ? "Görevler " + string.Join(", ", duties) : "Görevli yok");
            var s = _plants?.Summary() ?? default;
            if (s.Crops > 0) parts.Add($"{s.Ripe} ekin olgun, {s.Thirsty} susuz");
            int toMarket = 0; while (toMarket < 7 && !Festivals.IsMarketDay(Clock.DayIndex + toMarket)) toMarket++;
            parts.Add(toMarket == 0 ? "bugün pazar" : $"pazar {toMarket} gün sonra");
            var next = Festivals.NextFestival(out int fd);
            if (next != null && fd <= 7) parts.Add($"{next.DisplayName} {fd} gün sonra");
            var rep = Forecast.Report(Clock.TotalDays);
            parts.Add($"yiyecek {Math.Max(0, rep.FoodDays):0} gün, odun {Math.Max(0, rep.FirewoodDays):0} gün");
            return "Haftalık plan: " + string.Join(" · ", parts) + ".";
        }

        // ---- ırgat ve yarıcılık (Kâhyalık 3, 5, 9) ----
        public const int HiredWage = 3;
        public bool HiredHand { get => SkillFlag("hired") > 0; set => SetSkillFlag("hired", value ? 1 : 0); }
        public bool Sharecrop { get => SkillFlag("sharecrop") > 0; set => SetSkillFlag("sharecrop", value ? 1 : 0); }

        private void DailyHired(long day)
        {
            int steward = BestSkill(SkillType.Stewardship);
            if (HiredHand && steward >= 3)
            {
                int hands = steward >= 9 ? 2 : 1;
                if (!House.TrySpend(HiredWage * hands)) { HiredHand = false; Say("Irgata verecek akçe kalmadı; ırgat gitti.", NoticeLevel.Warning); }
                else
                {
                    int done = 0;
                    foreach (var it in World.Items)
                        if (it.IsPlant && it.Def.IsCrop && !it.IsGrown && !it.Watered && done < 12 * hands) { PlantSystem.Water(it); done++; }
                    if (done > 0) Say($"Irgat{(hands > 1 ? "lar" : "")} {done} ekini suladı ({HiredWage * hands} akçe).", NoticeLevel.Info, transient: true);
                }
            }
            if (Sharecrop && steward >= 5 && day % 3 == 0)
            {
                var crop = Content.Items.All.Where(d => d.IsCrop && PlantSystem.CanGrow(d, Clock.Season) && !d.SouthOnly).OrderBy(d => d.Id).ToList();
                if (crop.Count > 0)
                {
                    var pick = crop[(int)(day / 3 % crop.Count)];
                    var main = pick.Interactions.FirstOrDefault(x => x.Id == "harvest")?.Produce.FirstOrDefault()?.ItemId;
                    if (main != null) { House.AddItem(main, 3); Say($"Yarıcı payını getirdi: 3 {Name(main).ToLowerInvariant()}.", NoticeLevel.Info, transient: true); }
                }
            }
        }

        // ---- ticaret ve arabacılık (belge 45) ----
        public const double MerchantMarkBonus = 0.05, MarketTripBonus = 0.05;
        public const int SouthTripEvery = 7, SouthTripLevel = 6, CaravanLevel = 8, FarLandLevel = 9;
        public static readonly string[] SouthGoods = { "item.olive_oil", "item.coffee_green", "item.pomegranate", "item.saffron", "item.bay_leaf" };

        private bool HasCart() => House.Count(WheelbarrowId) > 0 || House.Count(HorseCartId) > 0 || House.Count(CoveredCartId) > 0 || World.Items.Any(x => x.Def.Id == "item.ox_cart");
        private bool HasDraftCart() => House.Count(HorseCartId) > 0 || House.Count(CoveredCartId) > 0 || World.Items.Any(x => x.Def.Id == "item.ox_cart");

        /// <summary>Satış eki: Tüccar mührü (Ticaret 10) +%5; pazar seferi (Arabacılık 5, arabayla pazar günü) +%5.</summary>
        public double TradeCartBonus()
        {
            double b = BestSkill(SkillType.Trade) >= Progress.Mastery.MasterLevel ? MerchantMarkBonus : 0.0;
            if (BestSkill(SkillType.Carting) >= 5 && Festivals.IsMarketDay(Clock.DayIndex) && HasCart()) b += MarketTripBonus;
            return b;
        }

        /// <summary>Uzak diyar siparişi (Ticaret 9) ve kervan (Arabacılık 8): haftada bir özel sipariş.</summary>
        public IEnumerable<(string item, int count, int reward, string label, int reputation)> TradeOrders(long day)
        {
            if (BestSkill(SkillType.Trade) >= FarLandLevel)
            {
                var far = new[] { "item.silk", "item.tin_brooch", "item.dowry_linen", "item.aged_cheese", "item.glazed_tile" }.Where(Content.Items.Contains).ToList();
                if (far.Count > 0)
                {
                    var id = far[(int)(day / 5 % far.Count)];
                    yield return (id, 1, (int)Math.Round(Content.Items.Get(id).Price * 2.0), "Uzak diyar siparişi", 3);
                }
            }
            if (BestSkill(SkillType.Carting) >= CaravanLevel && HasDraftCart())
            {
                string raw = null; int most = 0;
                foreach (var kv in House.InventoryList())
                    if (kv.Value > most && Content.Items.TryGet(kv.Key, out var dd) && dd.HasTag("raw") && dd.Price > 0) { most = kv.Value; raw = kv.Key; }
                if (raw != null && most >= 6)
                {
                    int n = Math.Min(12, most);
                    yield return (raw, n, (int)Math.Round(Content.Items.Get(raw).Price * n * 0.9), "Kervana mal", 1);
                }
            }
        }

        /// <summary>Güney Yamaçları seferi (Arabacılık 6, çekili araba): haftada bir güney malı ucuza; ertesi sabah gelir. Sonuç metni.</summary>
        public string SouthTrip()
        {
            if (BestSkill(SkillType.Carting) < SouthTripLevel) return $"Güney seferi için Arabacılık {SouthTripLevel} gerekli.";
            if (!HasDraftCart()) return "Güney seferi için at ya da öküz arabası gerekli.";
            if (SkillFlag("south") > 0 && Today - SkillFlag("south") < SouthTripEvery) return $"Araba yolda; {SouthTripEvery - (Today - (int)SkillFlag("south"))} gün sonra yeniden.";
            var goods = SouthGoods.Where(Content.Items.Contains).ToList();
            int cost = goods.Sum(g => (int)Math.Ceiling(Content.Items.Get(g).Price * 2 * 0.8));
            if (!House.TrySpend(cost)) return $"Güney malı için {cost} akçe gerekli.";
            foreach (var g in goods) House.AddOrder(g, 2);
            SetSkillFlag("south", Today);
            foreach (var m in Household()) if (m.Skills.GetLevel(SkillType.Carting) >= SouthTripLevel) { m.GainSkillXp(SkillType.Carting, 20); break; }
            return $"Araba güneye çıktı ({cost} akçe): yarın sabah {string.Join(", ", goods.Select(Name))} gelir.";
        }

        // ---- kâhyalık: haftalık satış rotası (8) ----
        private Economy.Market _market;
        public const int SalesRouteLevel = 8, SalesRouteKeep = 20, SalesRouteMax = 10;
        public bool SalesRoute { get => SkillFlag("salesroute") > 0; set => SetSkillFlag("salesroute", value ? 1 : 0); }

        /// <summary>Pazar günü kâhya fazlayı satar: ham maddede 20'nin üstünü (her maldan en çok 10). Satılan akçe.</summary>
        public int RunSalesRoute(long day)
        {
            if (_market == null || !SalesRoute || BestSkill(SkillType.Stewardship) < SalesRouteLevel || !Festivals.IsMarketDay(day)) return 0;
            int earned = 0;
            foreach (var kv in House.InventoryList().ToList())
            {
                if (kv.Value <= SalesRouteKeep || !Content.Items.TryGet(kv.Key, out var dd) || !dd.HasTag("raw") || !_market.IsGood(kv.Key)) continue;
                earned += _market.Sell(kv.Key, Math.Min(SalesRouteMax, kv.Value - SalesRouteKeep));
            }
            if (earned > 0) Say($"Kâhyanın satış rotası: fazla mal pazarda satıldı (+{earned} akçe).", NoticeLevel.Info);
            return earned;
        }

        // ---- kâhyalık: görev öncelikleri (2), kiler düzeni (6) ----
        /// <summary>Görev önceliği (0 düşük, 1 orta, 2 yüksek); kâhya dağıtırken çarpan olarak kullanır.</summary>
        public int DutyPriority(DutyKind d) => SkillFlag("prio:" + d) == 0 ? 1 : (int)SkillFlag("prio:" + d) - 1;
        public void SetDutyPriority(DutyKind d, int p) => SetSkillFlag("prio:" + d, Math.Max(0, Math.Min(2, p)) + 1);
        public double DutyWeight(DutyKind d) => BestSkill(SkillType.Stewardship) >= 2 ? (DutyPriority(d) == 0 ? 0.5 : DutyPriority(d) == 2 ? 2.0 : 1.0) : 1.0;

        // ---- ev düzenleme: oda adları (2) ----
        private readonly Dictionary<string, string> _roomNames = new Dictionary<string, string>(StringComparer.Ordinal);
        private static string RoomKey(Room r)
        {
            if (r == null || r.Tiles.Count == 0) return null;
            var t = r.Tiles.OrderBy(p => p.Z).ThenBy(p => p.Y).ThenBy(p => p.X).First();
            return $"{t.X},{t.Y},{t.Z}";
        }
        public string RoomName(Room r) { var k = RoomKey(r); return k != null && _roomNames.TryGetValue(k, out var n) ? n : null; }
        public string SetRoomName(Room r, string name)
        {
            if (BestSkill(SkillType.Decorating) < 2) return "Odaya ad koymak için Ev düzenleme 2 gerekli.";
            var k = RoomKey(r);
            if (k == null) return "Oda yok.";
            name = (name ?? "").Trim();
            if (name.Length > 24) name = name.Substring(0, 24);
            if (name.Length == 0) _roomNames.Remove(k); else _roomNames[k] = name;
            return null;
        }
        public string[] ExportRoomNames() => _roomNames.Select(kv => kv.Key + "=" + kv.Value).ToArray();
        public void ImportRoomNames(string[] names)
        {
            _roomNames.Clear();
            if (names == null) return;
            foreach (var s in names) { int i = s?.IndexOf('=') ?? -1; if (i > 0) _roomNames[s.Substring(0, i)] = s.Substring(i + 1); }
        }

        // ---- diğer sistemlere verilen pasifler ----

        /// <summary>Sözleşme yazıldıysa bugünkü köy işlerinin ödülü x1,15 (Okuryazarlık 7).</summary>
        public double ContractRewardFactor() => SkillFlag("contract") == Today ? 1.15 : 1.0;

        /// <summary>Halk takvimi hanedeyse herkes 1 gün fazla görür (Hava bilgisi 10).</summary>
        public int AlmanacBonusDays() => House.Count(AlmanacId) > 0 ? 1 : 0;

        /// <summary>Depo eki: el arabası, at arabası, kızak (kışın).</summary>
        public int CartStorageBonus() =>
            (House.Count(WheelbarrowId) > 0 ? 15 : 0) + (House.Count(HorseCartId) > 0 ? 40 : 0)
            + (House.Count(SledId) > 0 && Clock.Season == Season.Winter ? 20 : 0) + (BestSkill(SkillType.Carting) >= 2 ? 10 : 0);

        /// <summary>Bozulma azaltma eki: böcek kovucu ve üstü örtülü araba.</summary>
        public float SkillSpoilReduction() => (House.Count(RepellentId) > 0 ? 0.05f : 0f) + (House.Count(CoveredCartId) > 0 ? 0.10f : 0f)
            + (BestSkill(SkillType.Stewardship) >= 6 ? 0.10f : 0f);   // kiler düzeni (Kâhyalık 6)
    }
}
