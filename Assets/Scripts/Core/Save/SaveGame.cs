using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Save
{
    /// <summary>Kayıttan kurulan oyun parçaları; ısı ve pazar bunlarla kurulup ardından durumları geri konur.</summary>
    public sealed class LoadedGame
    {
        public GameClock Clock;
        public WorldState World;
        public Household Household;
        public List<Character> Characters = new List<Character>();
        public AccountProgress Account;
        public float[] Temperatures;
        /// <summary>İç oda başına (x, y, z, sıcaklık) dörtlüleri (sürüm 3); eski kayıtta null.</summary>
        public float[] RoomTemperatures;
        public List<KeyValuePair<string, double>> MarketDemand = new List<KeyValuePair<string, double>>();
        public List<int> MarketTakenOffers = new List<int>();
        public List<KeyValuePair<string, int>> MarketRawSold = new List<KeyValuePair<string, int>>();
        public long MarketRawWeek = -1;
        public List<(int a, int b, int value)> Affection = new List<(int, int, int)>();
        /// <summary>Kayıtta hanedan yoksa null (eski kayıt): oyuncu karakteriyle yeniden kurulur.</summary>
        public Dynasty Dynasty;
        /// <summary>Yaşam katmanı kaydı; eski kayıtta null (varsayılan durumla başlar).</summary>
        public LifeSaveData Life;
        /// <summary>Hayvanlar (v2); AnimalSystem kurulunca SaveGame.RestoreAnimals ile geri konur.</summary>
        public List<AnimalSave> Animals = new List<AnimalSave>();
        /// <summary>Toprak verimi; PlantSystem kurulunca SaveGame.RestoreSoil ile geri konur.</summary>
        public List<SoilSave> Soil = new List<SoilSave>();
        /// <summary>Yenmiş otlak ve hayvan çanı; SaveGame.RestoreAnimals ile geri konur.</summary>
        public List<GrassSave> Pasture = new List<GrassSave>();
        public bool AnimalsHeld;
        public string[] AnimalPedigree;
        public AwayAnimalSave[] AnimalsAway;
        /// <summary>Kazı yerleri; MineSystem kurulunca SaveGame.RestoreMines ile geri konur.</summary>
        public List<Mining.MineSave> Mines = new List<Mining.MineSave>();
        /// <summary>Yüklenemeyen parçalar (ör. içerikten kaldırılmış eşya). Oyun yine açılır.</summary>
        public List<string> Warnings = new List<string>();
        /// <summary>Ana rastgelelik tohumu (eski kayıtta 0) ve kayıttaki yükleme sayacı.</summary>
        public int Seed;
        public int RngEpoch;
    }

    /// <summary>Oyun durumunu SaveData'ya çevirir ve geri kurar (T16: sürümlü, geriye uyumlu).</summary>
    public static class SaveGame
    {
        // ---- ev dekorasyonu (27) ----
        private static ItemCondition ParseCondition(int c) => c == 1 ? ItemCondition.Damaged : c == 2 ? ItemCondition.Repaired : ItemCondition.Sound;

        private static string[] ContentsOf(ItemInstance it)
        {
            if (!it.HasContents) return null;
            var list = new List<string>();
            foreach (var c in it.Contents)
            {
                if (c.Quality > 0 || c.Maker > 0)   // kaliteli (53): "kimlik|durum|yaş|kendi|kalite|eser"
                    list.Add(c.ItemId + "|" + (int)c.Condition + "|" + c.Age.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "|" + (c.Own ? 1 : 0)
                             + "|" + c.Quality + "|" + c.Maker);
                else if (c.Age > 0 || c.Own)   // doldurulan dekor: "kimlik|durum|yaş|kendi"
                    list.Add(c.ItemId + "|" + (int)c.Condition + "|" + c.Age.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "|" + (c.Own ? 1 : 0));
                else list.Add(c.Condition == ItemCondition.Sound ? c.ItemId : c.ItemId + "|" + (int)c.Condition);
            }
            return list.ToArray();
        }

        private static bool TryParseStored(string entry, out StoredItem stored)
        {
            stored = default;
            if (string.IsNullOrWhiteSpace(entry)) return false;
            int bar = entry.IndexOf('|');
            var parts = bar < 0 ? new[] { entry, "0" } : entry.Split('|');
            int.TryParse(parts[1], out int c);
            stored = new StoredItem(parts[0].Trim(), ParseCondition(c));
            if (parts.Length >= 4)
            {
                double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double age);
                stored.Age = Math.Max(0, age);
                stored.Own = parts[3].Trim() == "1";
            }
            if (parts.Length >= 6)
            {
                int.TryParse(parts[4], out int q);
                int.TryParse(parts[5], out int m);
                stored.Quality = ItemQuality.Clamp(q);
                stored.Maker = Math.Max(0, m);
            }
            var alias = ItemQuality.Resolve(stored.ItemId);   // eski ayrı kimlik (karar 3)
            if (alias.quality > 0) { stored.ItemId = alias.id; stored.Quality = Math.Max(stored.Quality, alias.quality); }
            return stored.ItemId.Length > 0;
        }

        public const int CurrentVersion = 3;
        /// <summary>Haritanın kayıt biçimi (eşyalar hariç; eşyaların kapattığı karolar engel sayılmaz).</summary>
        public static MapSave CaptureMap(WorldState world)
        {
            var map = world.Map;
            // Harita: eşyaların kapattığı karolar arazi engeli sayılmaz (yüklenince eşyalar yeniden kapatır).
            var itemTiles = new HashSet<GridPos>();
            foreach (var it in world.Items) foreach (var t in it.OccupiedTiles()) itemTiles.Add(t);
            int n = map.Width * map.Height;
            var m = new MapSave
            {
                width = map.Width, height = map.Height,
                terrainBlocked = new bool[n], floor = new int[n], owned = new bool[n], zone = new int[n]
            };
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    int i = y * map.Width + x;
                    m.terrainBlocked[i] = map.IsBlocked(p) && !itemTiles.Contains(p);
                    m.floor[i] = (int)map.GetFloor(p);
                    m.owned[i] = map.IsOwned(p);
                    m.zone[i] = (int)map.GetZone(p);
                }
            var edges = new List<EdgeSave>();
            foreach (var kv in map.Edges)
            {
                var look = map.GetLook(kv.Key);
                edges.Add(new EdgeSave
                {
                    x = kv.Key.X, y = kv.Key.Y, z = kv.Key.Z, axis = (int)kv.Key.Axis, kind = (int)kv.Value,
                    style = look?.Style, finishA = look?.FinishA, finishB = look?.FinishB
                });
            }
            edges.Sort((a, b) => a.z != b.z ? a.z.CompareTo(b.z) : a.y != b.y ? a.y.CompareTo(b.y) : a.x != b.x ? a.x.CompareTo(b.x) : a.axis.CompareTo(b.axis));
            m.edges = edges.ToArray();
            var levelTiles = new List<LevelTileSave>();
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
            {
                if (z == 0) continue;
                foreach (var t in map.TilesOnLevel(z)) levelTiles.Add(new LevelTileSave { x = t.X, y = t.Y, z = z, floor = (int)map.GetFloor(t), zone = (int)map.GetZone(t) });
            }
            m.levelTiles = levelTiles.ToArray();
            var styles = new List<TileStyleSave>();
            for (int z = GridMap.MinLevel; z <= GridMap.MaxLevel; z++)
                foreach (var t in map.TilesOnLevel(z))
                {
                    string fs = map.GetFloorStyle(t), rs = map.GetRoofStyle(t);
                    if (fs != null || rs != null) styles.Add(new TileStyleSave { x = t.X, y = t.Y, z = z, floor = fs, roof = rs });
                }
            m.styles = styles.ToArray();
            var roads = new List<RoadSave>();
            foreach (var r in map.RoadTiles())
                roads.Add(new RoadSave { x = r.pos.X, y = r.pos.Y, type = (int)r.type, cond = r.cond, natural = r.natural, trample = r.trample });
            m.roads = roads.ToArray();
            var conduits = new List<ConduitSave>();
            foreach (var (pos, type) in map.ConduitTiles()) conduits.Add(new ConduitSave { x = pos.X, y = pos.Y, type = (int)type });
            m.conduits = conduits.ToArray();
            var ponds = new List<PondSave>();
            foreach (var p in map.Ponds)
            {
                var ids = new List<string>(); var counts = new List<int>();
                foreach (var f in p.Fish) { ids.Add(f.Key); counts.Add(f.Value); }
                ponds.Add(new PondSave { x = p.Anchor.X, y = p.Anchor.Y, level = p.Level, spring = p.Spring, insects = p.Insects, filledDays = p.FilledDays, fishIds = ids.ToArray(), fishCounts = counts.ToArray(), silt = p.Silt });
            }
            m.ponds = ponds.ToArray();
            return m;
        }

        /// <summary>
        /// Eski, dar bir dünya kaydını yeni dünyanın genişliğine açar (aynı yükseklik): eski genişlik içindeki her şey
        /// kayıttan, dışındaki her şey (yeni bölgeler) 'fresh' haritadan gelir. Eşyalara dokunmaz.
        /// </summary>
        public static bool WidenMap(SaveData data, MapSave fresh)
        {
            var old = data?.map;
            old?.Unpack();
            fresh?.Unpack();
            if (old == null || fresh == null || fresh.height != old.height || fresh.width <= old.width) return false;
            int ow = old.width, fw = fresh.width;
            for (int y = 0; y < old.height; y++)
                for (int x = 0; x < ow; x++)
                {
                    int oi = y * ow + x, fi = y * fw + x;
                    if (old.terrainBlocked != null && oi < old.terrainBlocked.Length) fresh.terrainBlocked[fi] = old.terrainBlocked[oi];
                    if (old.floor != null && oi < old.floor.Length) fresh.floor[fi] = old.floor[oi];
                    if (old.owned != null && oi < old.owned.Length) fresh.owned[fi] = old.owned[oi];
                    if (old.zone != null && oi < old.zone.Length) fresh.zone[fi] = old.zone[oi];
                }
            fresh.edges = Merge(old.edges, fresh.edges, e => e.x > ow);
            fresh.levelTiles = Merge(old.levelTiles, fresh.levelTiles, t => t.x >= ow);
            fresh.styles = Merge(old.styles, fresh.styles, t => t.x >= ow);
            fresh.roads = Merge(old.roads, fresh.roads, t => t.x >= ow);
            fresh.conduits = Merge(old.conduits, fresh.conduits, t => t.x >= ow);
            fresh.ponds = Merge(old.ponds, fresh.ponds, t => t.x >= ow);
            data.map = fresh;
            return true;
        }

        private static T[] Merge<T>(T[] old, T[] fresh, Func<T, bool> keepFresh)
        {
            var list = new List<T>();
            if (old != null) list.AddRange(old);
            if (fresh != null) foreach (var t in fresh) if (keepFresh(t)) list.Add(t);
            return list.ToArray();
        }

        /// <summary>Kayıttan kabul edilen en büyük harita kenarı (oyun haritası 48x32).</summary>
        public const int MaxMapSide = 512;   // dünya 428x72 (bölgeler, belge 38)

        public static SaveData Capture(GameClock clock, WorldState world, Household house, IEnumerable<Character> characters,
            AccountProgress account, HeatSystem heat, Market market, FamilyService family = null, Dynasty dynasty = null,
            AnimalSystem animals = null, LifeSystems life = null, PlantSystem plants = null, int seed = 0, int rngEpoch = 0,
            Mining.MineSystem mines = null)
        {
            life?.FlushCoarse();   // biriken seyrek adım (dekor, kandil) kayda girsin
            var map = world.Map;
            var data = new SaveData
            {
                version = CurrentVersion,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                totalDays = clock.TotalDays,
                speed = clock.RequestedSpeed,
                seed = seed,
                rngEpoch = rngEpoch
            };

            data.map = CaptureMap(world);

            // Eşyalar: önce yer eşyaları, sonra dekorlar (yüklerken üstündeki eşya hazır olsun).
            var items = new List<ItemSave>();
            foreach (var pass in new[] { false, true })
                foreach (var it in world.Items)
                {
                    if (it.IsDecor != pass) continue;
                    KeyInt[] stock = null;
                    if (it.StockTotal > 0)
                    {
                        var sl = new List<KeyInt>();
                        foreach (var kv in it.Stock) if (kv.Value > 0) sl.Add(new KeyInt { key = kv.Key, value = kv.Value });
                        sl.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
                        stock = sl.ToArray();
                    }
                    items.Add(new ItemSave
                    {
                        id = it.Id, def = it.Def.Id, x = it.Position.X, y = it.Position.Y, z = it.Position.Z, rotation = it.Rotation,
                        parentId = it.ParentId ?? -1, slotId = it.SlotId, fuelDays = it.FuelDays, growthDays = it.GrowthDays,
                        feedStock = it.FeedStock, stock = stock, finish = it.FinishId, accent = it.AccentId, toggled = it.Toggled, fill = it.Fill,
                        watered = it.Watered, ageDays = it.AgeDays, wetDays = it.WetDays, fruitReady = it.FruitReady, fruitDays = it.FruitDays,
                        harvests = it.Harvests, staked = it.Staked, withered = it.Withered, witheredMature = it.WitheredMature, offSeasonDays = it.OffSeasonDays,
                        band = it.IsWallMounted ? it.WallBand : -1, layer = it.IsRug ? it.LayerOrder : 0, condition = (int)it.Condition,
                        tarnish = it.TarnishDays, contents = ContentsOf(it), quality = it.Quality, maker = it.Maker, embellished = it.Embellished
                    });
                }
            data.items = items.ToArray();

            if (plants != null)
            {
                var soil = new List<SoilSave>();
                foreach (var kv in plants.SoilPlots)
                    soil.Add(new SoilSave
                    {
                        x = kv.Key.X, y = kv.Key.Y, fertility = kv.Value.Fertility, lastCrop = kv.Value.LastCrop, prevCrop = kv.Value.PrevCrop,
                        worked = kv.Value.Worked, residual = kv.Value.Residual, residualNext = kv.Value.ResidualNext, cycle = (int)kv.Value.Cycle,
                        rawFilth = kv.Value.RawFilth, burnUntil = kv.Value.BurnUntil, marlUntil = kv.Value.MarlUntil, limedUntil = kv.Value.LimedUntil,
                        mulchUntil = kv.Value.MulchUntil, ashDay = kv.Value.AshDay
                    });
                soil.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
                data.soil = soil.ToArray();
            }

            var inv = new List<KeyInt>();
            foreach (var kv in house.InventoryList()) inv.Add(new KeyInt { key = kv.Key, value = kv.Value });
            var dmg = new List<KeyInt>();
            foreach (var kv in house.DamagedList()) dmg.Add(new KeyInt { key = kv.Key, value = kv.Value });
            var rep = new List<KeyInt>();
            foreach (var kv in house.RepairedList()) rep.Add(new KeyInt { key = kv.Key, value = kv.Value });
            var bought = new List<KeyInt>();
            foreach (var kv in house.BoughtList()) bought.Add(new KeyInt { key = kv.Key, value = kv.Value });
            var stacks = new List<StackSave>();
            foreach (var kv in house.AllStacks())
                stacks.Add(new StackSave { id = kv.Key.Id, q = kv.Key.Quality, c = (int)kv.Key.Condition, b = kv.Key.Bought, m = kv.Key.Maker, n = kv.Value });
            data.household = new HouseholdSave { akce = house.Akce, inventory = inv.ToArray(), damaged = dmg.ToArray(), repaired = rep.ToArray(), bought = bought.ToArray(),
                shopUnlocks = new List<string>(house.ShopUnlocks).ToArray(), stacks = stacks.ToArray() };
            var orders = new List<KeyInt>();
            foreach (var kv in house.Orders) orders.Add(new KeyInt { key = kv.Key, value = kv.Value });
            data.household.orders = orders.ToArray();

            var chars = new List<CharacterSave>();
            foreach (var c in characters) chars.Add(CaptureCharacter(c));
            data.characters = chars.ToArray();

            if (account != null)
            {
                var counters = new List<KeyInt>();
                foreach (var kv in account.Counters) counters.Add(new KeyInt { key = kv.Key, value = kv.Value });
                data.account = new AccountSave
                {
                    xp = account.Xp,
                    firsts = new List<string>(account.Firsts).ToArray(),
                    badges = new List<string>(account.EarnedBadges).ToArray(),
                    counters = counters.ToArray(),
                    workDay = account.WorkDay,
                    workToday = account.WorkXpTodayRaw,
                    diamonds = account.Diamonds,
                    diamondsSeeded = true
                };
            }

            data.roomTemperatures = heat?.ExportRoomTemperatures();   // oda başına; karo başına dizi artık yazılmaz
            if (market != null)
            {
                var demand = new List<KeyFloat>();
                foreach (var kv in market.ExportDemand()) demand.Add(new KeyFloat { key = kv.Key, value = kv.Value });
                data.marketDemand = demand.ToArray();
                data.marketTakenOffers = market.ExportTakenOffers().ToArray();
                var raw = new List<KeyFloat>();
                var rawSold = market.ExportRawSold(out long rawWeek);
                foreach (var kv in rawSold) raw.Add(new KeyFloat { key = kv.Key, value = kv.Value });
                data.marketRawSold = raw.ToArray();
                data.marketRawWeek = rawWeek;
            }
            if (family != null)
            {
                var aff = new List<AffectionSave>();
                foreach (var (a, b, v) in family.ExportAffection()) aff.Add(new AffectionSave { a = a, b = b, value = v });
                data.affection = aff.ToArray();
            }
            if (dynasty != null)
            {
                var hist = new List<HeadSave>();
                foreach (var h in dynasty.History)
                    hist.Add(new HeadSave { characterId = h.CharacterId, name = h.Name, generation = h.Generation, startDay = h.StartDay, endDay = h.EndDay, endCause = (int)h.EndCause, trade = h.Trade });
                data.dynasty = new DynastySave { name = dynasty.Name, settlement = dynasty.SettlementName, headId = dynasty.HeadId, ended = dynasty.Ended, history = hist.ToArray() };
            }
            if (animals != null)
            {
                var list = new List<AnimalSave>();
                foreach (var a in animals.Animals)
                {
                    if (a.IsDead) continue;
                    list.Add(new AnimalSave
                    {
                        id = a.Id, species = a.Species.Id, x = a.Position.X, y = a.Position.Y, home = a.HomeItemId,
                        fullness = a.Fullness, age = a.AgeDays, produce = a.ProduceProgress, hungryDays = a.HungryDays, friendship = a.Friendship,
                        petCooldown = a.PetCooldownDays, saddle = a.Saddle, comfort = a.Comfort,
                        sick = a.IsSick, sickDays = a.SickDays, coldDays = a.ColdDays,
                        sex = a.IsFemale ? 1 : 2, pregnant = a.PregnantDays, mother = a.MotherId, generation = a.Generation,
                        lineage = (int)a.Lineage, xp = a.Xp, legendDescent = a.LegendaryDescent, legendTitle = a.LegendaryTitle,
                        stamina = a.Stamina, shy = a.ShyDays,
                        father = a.FatherId, genome = a.Genome?.Export(), deeds = ExportDeeds(a),
                        sire = a.PregnancySireId, sireSpecies = a.PregnancySireSpecies, sireGenome = a.PregnancySireGenome?.Export()
                    });
                }
                data.animals = list.ToArray();
                data.animalPedigree = animals.ExportPedigree();
                data.animalsAway = animals.ExportAway();
                var grass = new List<GrassSave>();
                foreach (var kv in animals.ExportGrass()) grass.Add(new GrassSave { x = kv.Key.X, y = kv.Key.Y, grass = kv.Value });
                grass.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
                data.pasture = grass.ToArray();
                data.animalsHeld = !animals.Released;
            }
            data.life = life?.Export();
            data.mines = mines?.Export().ToArray();
            data.map?.Pack();   // karo dizileri sıkıştırılmış metne (dosya ~450 KB küçülür)
            return data;
        }

        internal static string[] ExportDeeds(Animal a)
        {
            if (a.Deeds.Count == 0) return null;
            var l = new List<string>();
            foreach (var kv in a.Deeds) l.Add(kv.Key + ":" + kv.Value);
            l.Sort(string.CompareOrdinal);
            return l.ToArray();
        }

        private static KeyFloat[] CaptureAptitudes(Character c)
        {
            var list = new List<KeyFloat>();
            foreach (var kv in c.Skills.Aptitudes) list.Add(new KeyFloat { key = kv.Key.ToString(), value = (int)kv.Value });
            list.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
            return list.ToArray();
        }

        private static KeyFloat[] CaptureToday(Character c)
        {
            var list = new List<KeyFloat>();
            foreach (var kv in c.Skills.TodayAll) if (kv.Value > 0) list.Add(new KeyFloat { key = kv.Key.ToString(), value = kv.Value });
            list.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
            return list.ToArray();
        }

        private static string[] CaptureUsed(Character c)
        {
            var list = new List<string>();
            foreach (var s in c.Skills.UsedSeasonAll) list.Add(s.ToString());
            list.Sort(string.CompareOrdinal);
            return list.ToArray();
        }

        private static CharacterSave CaptureCharacter(Character c)
        {
            var skills = new List<KeyFloat>();
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                double xp = c.Skills.GetXp(s);
                if (xp > 0) skills.Add(new KeyFloat { key = s.ToString(), value = xp });
            }
            var a = c.Appearance ?? new Appearance();
            var moodlets = new List<MoodletSave>();
            foreach (var m in c.Mood.Active)
                if (!MoodCatalog.Default.Contains(m.Def.Id))
                    moodlets.Add(new MoodletSave { id = m.Def.Id, text = m.Def.DisplayName, value = m.Def.Value, days = m.Def.IsCondition ? -1 : m.RemainingDays });
            return new CharacterSave
            {
                social = c.Needs.Get(NeedType.Social),
                traits = new List<string>(c.TraitIds).ToArray(), branches = new List<string>(c.BranchIds).ToArray(),
                extraMoodlets = moodlets.ToArray(),
                id = c.Id, name = c.Name, isPlayer = c.IsPlayerControlled, sex = (int)c.Sex, duty = (int)c.Duty,
                x = c.Position.X, y = c.Position.Y, z = c.Position.Z,
                ageDays = c.AgeDays, lifespanDays = c.LifespanDays,
                dead = c.IsDead, deathCause = (int)c.CauseOfDeath,
                pregnant = c.IsPregnant, fatherId = c.FatherId ?? -1,
                pregnancyLeft = c.PregnancyLeft, postpartumLeft = c.PostpartumLeft,
                freezeDays = c.FreezeDays, starveDays = c.StarveDays,
                hunger = c.Needs.Get(NeedType.Hunger), energy = c.Needs.Get(NeedType.Energy), warmth = c.Needs.Get(NeedType.Warmth),
                fun = c.Needs.Get(NeedType.Fun), hygiene = c.Needs.Get(NeedType.Hygiene), bladder = c.Needs.Get(NeedType.Bladder), wet = c.Wetness,
                moodlets = CaptureMoodlets(c),
                skills = skills.ToArray(),
                aptitudes = CaptureAptitudes(c), skillToday = CaptureToday(c), skillUsedSeason = CaptureUsed(c),
                elderOnset = c.Skills.ElderOnsetApplied,
                spouseId = c.SpouseId ?? 0,
                motherId = c.BirthMotherId ?? 0,
                birthFatherId = c.BirthFatherId ?? 0,
                generation = c.Generation,
                isVillager = c.IsVillager,
                appearance = CaptureAppearance(a, c.Genome),
                birthSeason = c.BirthSeason, birthDay = c.BirthDayOfSeason,
                likedFood = c.LikedFood, likedColor = c.LikedColor, likedHobby = c.LikedHobby, dislikedFood = c.DislikedFood,
                freeMirror = c.FreeMirrorAvailable,
                lastAccidentDay = c.LastAccidentDay,
                breakdown = (int)c.Breakdown,
                breakdownLeft = c.BreakdownLeft,
                miserableDays = c.MiserableDays,
                lastBreakdownAge = c.LastBreakdownAge,
                upbringingSum = c.UpbringingMoodSum,
                upbringingDays = c.UpbringingDays,
                chronicle = c.Chronicle.Count > 0 ? new List<string>(c.Chronicle).ToArray() : null,
                injury = c.Injury == null ? null : new InjurySave
                {
                    kind = c.Injury.Kind, zone = c.Injury.Zone, pattern = c.Injury.Pattern, reason = c.Injury.Reason,
                    startDay = c.Injury.StartDay, healDay = c.Injury.HealDay, deep = c.Injury.Deep, treated = c.Injury.Treated
                }
            };
        }

        private static AppearanceSave CaptureAppearance(Appearance a, Genome genome)
        {
            var marks = new List<MarkSave>();
            foreach (var m in a.Marks)
                marks.Add(new MarkSave { kind = (int)m.Kind, id = m.Id, zone = m.Zone, color = m.Color.ToHex(), fade = m.Fade, source = (int)m.Source, day = m.Day, reason = m.Reason, hidden = m.Hidden });
            return new AppearanceSave
            {
                build = (int)a.Build, heightPercent = 100,
                skin = a.SkinColor.ToHex(), eye = a.EyeColor.ToHex(),
                hairStyle = a.HairStyleId, hair = a.HairColor.ToHex(),
                facialHairStyle = a.FacialHairId, facialHair = a.FacialHairColor.ToHex(),
                faceMark = a.FaceMarkId, outfit = a.OutfitId, outfitColor = a.OutfitColor.ToHex(), trim = a.TrimColor.ToHex(),
                hat = a.HatId, hatColor = a.HatColor.ToHex(),
                frame = (int)a.Frame, faceShape = a.FaceShapeId, eyeShape = a.EyeShapeId,
                eyeRight = a.EyeColorRight.HasValue ? a.EyeColorRight.Value.ToHex() : "",
                brow = a.BrowId, browColor = a.BrowColor.ToHex(), browOwn = a.BrowFollowsHair ? 0 : 1,
                nose = a.NoseId, mouth = a.MouthId, ears = a.EarsId,
                bangs = a.BangsId, mustache = a.MustacheId, beard = a.BeardId,
                hairExtra = a.HairExtraId, hairOrnament = a.HairOrnamentId, hairOrnamentColor = a.HairOrnamentColor.ToHex(),
                sideburns = a.SideburnsId, stubble = a.StubbleLevel, lashes = a.LashesId,
                pattern = a.PatternId, bottom = a.BottomId, shoes = a.ShoesId, outer = a.OuterId, outerColor = a.OuterColor.ToHex(),
                voice = (int)a.Voice, voicePitch = a.VoicePitch, laugh = a.LaughId,
                marks = marks.ToArray(),
                genome = genome?.Export()
            };
        }

        private static KeyFloat[] CaptureMoodlets(Character c)
        {
            var list = new List<KeyFloat>();
            foreach (var kv in c.Mood.Export()) list.Add(new KeyFloat { key = kv.Key, value = kv.Value });
            return list.ToArray();
        }

        /// <summary>Kayıttaki kazı yerlerini kazı motoruna geri koyar.</summary>
        public static void RestoreMines(LoadedGame loaded, Mining.MineSystem mines) => mines.Restore(loaded.Mines);

        /// <summary>Kayıttaki sıcaklıkları ısı sistemine geri koyar (eski karo dizisi ya da oda başına değerler).</summary>
        public static void RestoreHeat(LoadedGame loaded, HeatSystem heat)
        {
            if (loaded == null || heat == null) return;
            heat.RestoreTemperatures(loaded.Temperatures);
            heat.RestoreRoomTemperatures(loaded.RoomTemperatures);
        }

        /// <summary>Kayıttaki toprak verimini çiftçilik sistemine geri koyar.</summary>
        public static void RestoreSoil(LoadedGame loaded, PlantSystem plants)
        {
            foreach (var s in loaded.Soil)
            {
                if (s == null) continue;
                var pos = new GridPos(s.x, s.y);
                plants.SetSoil(pos, s.fertility, s.lastCrop, s.prevCrop);
                var plot = plants.Plot(pos);
                plot.Worked = s.worked; plot.Residual = s.residual; plot.ResidualNext = s.residualNext;
                plot.Cycle = (Content.Fertilizer)s.cycle; plot.RawFilth = s.rawFilth;
                plot.BurnUntil = s.burnUntil; plot.MarlUntil = s.marlUntil; plot.LimedUntil = s.limedUntil; plot.MulchUntil = s.mulchUntil;
                plot.AshDay = s.ashDay == 0 && !s.worked ? -1000 : s.ashDay;
            }
        }

        /// <summary>Kayıttaki hayvanları kurulan hayvan sistemine geri koyar (barınağı olmayanlar atlanır).</summary>
        public static void RestoreAnimals(LoadedGame loaded, AnimalSystem system, ContentDatabase content)
        {
            foreach (var s in loaded.Animals)
            {
                if (s == null) continue;
                if (!content.Animals.TryGet(s.species, out var species)) { loaded.Warnings.Add($"Hayvan türü yok, atlandı: {s.species}"); continue; }
                var home = loaded.World.GetItem(s.home);
                if (home == null || home.Def.HousingKind != species.Housing) { loaded.Warnings.Add($"Hayvanın barınağı yok, atlandı: {s.species}"); continue; }
                system.Restore(s.id, species, new GridPos(s.x, s.y), s.home, s.fullness, s.age, s.produce, s.hungryDays, s.friendship, s.petCooldown,
                    s.saddle, s.comfort >= 0f ? s.comfort : 50f, s.sick, s.sickDays, s.coldDays,
                    s.sex == 1 ? true : s.sex == 2 ? false : (bool?)null, s.pregnant, s.mother, s.generation,
                    s.lineage, s.xp, s.legendDescent, s.legendTitle, s.stamina, s.shy,
                    s.genome, s.father, s.deeds, s.sire, s.sireSpecies, s.sireGenome);
            }
            system.ImportPedigree(loaded.AnimalPedigree);
            system.ImportAway(loaded.AnimalsAway, content, loaded.Warnings);
            var grass = new List<KeyValuePair<GridPos, float>>();
            foreach (var g in loaded.Pasture) if (g != null) grass.Add(new KeyValuePair<GridPos, float>(new GridPos(g.x, g.y), g.grass));
            system.RestoreGrass(grass);
            system.Released = !loaded.AnimalsHeld;
        }

        /// <summary>Kaydı geri kurar. Sürüm yeniyse ya da harita yoksa InvalidOperationException.</summary>
        private static string PartOrNull(ContentDatabase content, string id) =>
            !string.IsNullOrEmpty(id) && content.Parts.Contains(id) ? id : null;

        public static LoadedGame Load(SaveData data, ContentDatabase content, TimeConfig timeConfig = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            SaveMigrator.Upgrade(data);
            data.map?.Unpack();
            if (data.map == null || data.map.width <= 0 || data.map.height <= 0) throw new InvalidOperationException("Kayıtta harita yok.");
            // Bozuk ya da elle oynanmış dosya devasa bir dizi ayırtmasın.
            if (data.map.width > MaxMapSide || data.map.height > MaxMapSide)
                throw new InvalidOperationException($"Kayıttaki harita boyutu geçersiz: {data.map.width}x{data.map.height}.");

            var result = new LoadedGame();
            result.Clock = new GameClock(timeConfig);
            result.Clock.Restore(Math.Max(0, data.totalDays), data.speed);

            var m = data.map;
            var map = new GridMap(m.width, m.height);
            int n = m.width * m.height;
            for (int y = 0; y < m.height; y++)
                for (int x = 0; x < m.width; x++)
                {
                    int i = y * m.width + x;
                    var p = new GridPos(x, y);
                    if (m.terrainBlocked != null && i < m.terrainBlocked.Length && m.terrainBlocked[i]) map.SetBlocked(p, true);
                    if (m.floor != null && i < m.floor.Length && Enum.IsDefined(typeof(FloorType), m.floor[i])) map.SetFloor(p, (FloorType)m.floor[i]);
                    if (m.owned != null && i < m.owned.Length) map.SetOwned(p, m.owned[i]);
                    if (m.zone != null && i < m.zone.Length && Enum.IsDefined(typeof(ZoneType), m.zone[i])) map.SetZone(p, (ZoneType)m.zone[i]);
                }
            if (m.levelTiles != null)
                foreach (var lt in m.levelTiles)
                {
                    if (lt == null || lt.z == 0) continue;
                    var p = new GridPos(lt.x, lt.y, lt.z);
                    if (!map.InBounds(p)) continue;
                    map.SetTile(p, true);
                    if (Enum.IsDefined(typeof(FloorType), lt.floor)) map.SetFloor(p, (FloorType)lt.floor);
                    if (Enum.IsDefined(typeof(ZoneType), lt.zone)) map.SetZone(p, (ZoneType)lt.zone);
                }
            if (m.edges != null)
                foreach (var e in m.edges)
                {
                    if (e == null || !Enum.IsDefined(typeof(EdgeAxis), e.axis)) continue;
                    var edge = new Edge(e.x, e.y, (EdgeAxis)e.axis, e.z);
                    if (!map.EdgeInBounds(edge) || !Enum.IsDefined(typeof(EdgeKind), e.kind) || e.kind == (int)EdgeKind.None) continue;
                    map.SetEdge(edge, (EdgeKind)e.kind);
                    // Bilinmeyen parça kimliği (içerikten kaldırılmış) varsayılana düşer.
                    string style = PartOrNull(content, e.style), fa = PartOrNull(content, e.finishA), fb = PartOrNull(content, e.finishB);
                    if (style != null || fa != null || fb != null)
                        map.RestoreLook(edge, new EdgeLook { Style = style, FinishA = EdgeKinds.IsRoomBoundary((EdgeKind)e.kind) ? fa : null, FinishB = EdgeKinds.IsRoomBoundary((EdgeKind)e.kind) ? fb : null });
                }
            if (m.roads != null)
                foreach (var r in m.roads)
                {
                    if (r == null || r.x < 0 || r.y < 0 || r.x >= map.Width || r.y >= map.Height) continue;
                    var p = new GridPos(r.x, r.y);
                    if (r.type > 0 && Enum.IsDefined(typeof(RoadType), (byte)r.type)) map.SetRoad(p, (RoadType)r.type, r.cond, r.natural);
                    else map.SetTrample(p, r.trample);
                }
            if (m.conduits != null)
                foreach (var c in m.conduits)
                {
                    if (c == null || c.x < 0 || c.y < 0 || c.x >= map.Width || c.y >= map.Height) continue;
                    if (c.type > 0 && Enum.IsDefined(typeof(ConduitType), (byte)c.type)) map.SetConduit(new GridPos(c.x, c.y), (ConduitType)c.type);
                }
            if (m.ponds != null)
                foreach (var ps in m.ponds)
                {
                    if (ps == null || ps.x < 0 || ps.y < 0 || ps.x >= map.Width || ps.y >= map.Height) continue;
                    var fish = new List<KeyValuePair<string, int>>();
                    if (ps.fishIds != null && ps.fishCounts != null)
                        for (int i = 0; i < ps.fishIds.Length && i < ps.fishCounts.Length; i++)
                            if (ps.fishIds[i] != null && content.Items.Contains(ps.fishIds[i])) fish.Add(new KeyValuePair<string, int>(ps.fishIds[i], ps.fishCounts[i]));
                    map.RestorePond(new GridPos(ps.x, ps.y), ps.level, ps.spring, ps.insects, ps.filledDays, fish, ps.silt);
                }
            if (m.styles != null)
                foreach (var st in m.styles)
                {
                    if (st == null) continue;
                    var p = new GridPos(st.x, st.y, st.z);
                    if (!map.HasTile(p)) continue;
                    var floor = map.GetFloor(p);
                    if (st.floor != null && content.Parts.TryGet(st.floor, out var fp) && fp.Kind == BuildPartKind.Floor && fp.Base == floor)
                        map.SetFloor(p, floor, st.floor);
                    if (st.roof == GridMap.NoRoof || st.roof != null && content.Parts.TryGet(st.roof, out var rp) && rp.Kind == BuildPartKind.Roof)
                        map.SetRoofStyle(p, st.roof);
                }

            var world = new WorldState(map);
            result.World = world;
            result.Seed = data.seed;
            result.RngEpoch = Math.Max(0, data.rngEpoch);
            // Yerine konamayan eşya kaybolmaz: envantere döner (bitki, yabani ve anıt hariç; onlar envanterde durmaz).
            var returned = new List<ItemDef>();
            if (data.items != null)
                foreach (var s in data.items)
                {
                    if (s == null) continue;
                    if (!content.Items.TryGet(s.def, out var def)) { result.Warnings.Add($"Eşya tanımı yok, atlandı: {s.def}"); continue; }
                    int? parent = s.parentId >= 0 ? s.parentId : (int?)null;
                    if (parent.HasValue && world.GetItem(parent.Value) == null)
                    {
                        result.Warnings.Add($"Dekorun yüzeyi yok, {(KeepsInInventory(def) ? "envantere alındı" : "atlandı")}: {s.def}");
                        if (KeepsInInventory(def)) returned.Add(def);
                        continue;
                    }
                    var pos = new GridPos(s.x, s.y, s.z);
                    if (!parent.HasValue)
                    {
                        // Yükleme modu: yalnızca yapısal denetim. Oyun sırasında geçerli oluşmuş durumlar eşyayı düşürmez.
                        var fits = def.Placement == ItemPlacement.Wall ? world.CanPlaceOnWall(def, pos, s.rotation, s.band, null, restoring: true)
                            : def.IsRug ? world.CanPlaceRug(def, pos, s.rotation)
                            : def.IsCeiling ? world.CanPlaceCeiling(def, pos, null, restoring: true)
                            : world.CanPlaceFloor(def, pos, s.rotation, null, false, restoring: true);
                        if (!fits.Ok)
                        {
                            result.Warnings.Add($"Eşya yerine sığmadı, {(KeepsInInventory(def) ? "envantere alındı" : "atlandı")}: {s.def} {pos}");
                            if (KeepsInInventory(def)) returned.Add(def);
                            continue;
                        }
                    }
                    var restored = world.RestoreItem(def, s.id, pos, s.rotation, parent, s.slotId, Math.Max(0, s.fuelDays), Math.Max(0, s.growthDays));
                    restored.FeedStock = Math.Max(0, s.feedStock);
                    if (!string.IsNullOrEmpty(s.finish) && content.Parts.TryGet(s.finish, out var fin) && fin.Kind == BuildPartKind.ItemFinish && fin.Group == def.FinishGroup)
                        restored.FinishId = s.finish;
                    if (!string.IsNullOrEmpty(s.accent) && content.Parts.TryGet(s.accent, out var acc) && acc.Kind == BuildPartKind.ItemFinish && acc.Group == def.AccentGroup)
                        restored.AccentId = s.accent;
                        restored.Toggled = s.toggled;
                        restored.Fill = Math.Max(0, s.fill);
                    restored.SetPlantState(s.watered, Math.Max(0, s.ageDays), Math.Max(0, s.wetDays), s.fruitReady, Math.Max(0, s.fruitDays));
                    if (def.IsCrop) restored.SetCropState(Math.Max(0, s.harvests), s.staked, s.withered, s.witheredMature, Math.Max(0, s.offSeasonDays));
                    if (s.stock != null) foreach (var kv in s.stock) if (kv != null && kv.value > 0 && content.Items.Contains(ItemKey.FromStockKey(kv.key).Id)) restored.SetStock(kv.key, kv.value);
                    int band = s.band;
                    if (restored.IsWallMounted && band < 0) band = world.FirstFreeBand(def, pos, s.rotation, restored.Id);
                    world.RestoreDecorState(restored, band, s.layer, ParseCondition(s.condition), s.tarnish);
                    restored.Quality = def.HasQuality ? ItemQuality.Clamp(s.quality) : 0;
                    restored.Maker = Math.Max(0, s.maker);
                    restored.Embellished = s.embellished;
                    if (s.contents != null)
                        foreach (var entry in s.contents)
                            if (TryParseStored(entry, out var stored) && content.Items.Contains(stored.ItemId)) restored.Contents.Add(stored);
                }

            var hs = data.household ?? new HouseholdSave();
            var house = new Household(Math.Max(0, hs.akce));
            if (hs.stacks != null)
            {
                foreach (var st in hs.stacks)
                {
                    if (st == null || st.n <= 0 || string.IsNullOrEmpty(st.id)) continue;
                    var (id, aq) = ItemQuality.Resolve(st.id);   // eski ayrı kimlik kaliteye döner (karar 3)
                    if (!content.Items.TryGet(id, out var sd)) { result.Warnings.Add($"Envanterde bilinmeyen eşya, atlandı: {st.id}"); continue; }
                    int q = sd.HasQuality ? Math.Max(aq, ItemQuality.Clamp(st.q)) : 0;
                    house.Add(new ItemKey(id, q, ParseCondition(st.c), st.b, st.m), st.n);
                }
                foreach (var def in returned) house.AddItem(def.Id);
            }
            else
            {
                // Eski kayıt: kimlik × adet, yanında hasar ve pazar sayaçları (hepsi Sıradan; eski ayrı kimlik kaliteye döner).
                if (hs.inventory != null)
                    foreach (var kv in hs.inventory)
                    {
                        if (kv == null || kv.value <= 0) continue;
                        var (id, aq) = ItemQuality.Resolve(kv.key);
                        if (!content.Items.Contains(id)) { result.Warnings.Add($"Envanterde bilinmeyen eşya, atlandı: {kv.key}"); continue; }
                        house.AddItem(id, kv.value, aq);
                    }
                foreach (var def in returned) house.AddItem(def.Id);
                if (hs.damaged != null) foreach (var kv in hs.damaged) if (kv != null && content.Items.Contains(kv.key)) house.RestoreCondition(kv.key, kv.value, ItemCondition.Damaged);
                if (hs.repaired != null) foreach (var kv in hs.repaired) if (kv != null && content.Items.Contains(kv.key)) house.RestoreCondition(kv.key, kv.value, ItemCondition.Repaired);
                if (hs.bought != null) foreach (var kv in hs.bought) if (kv != null && content.Items.Contains(kv.key)) house.MarkBought(kv.key, kv.value);
            }
            if (hs.shopUnlocks != null) foreach (var k in hs.shopUnlocks) house.UnlockShop(k);
            if (hs.orders != null) foreach (var kv in hs.orders) if (kv != null && content.Items.Contains(kv.key)) house.AddOrder(kv.key, kv.value);
            result.Household = house;

            if (data.characters != null)
                foreach (var cs in data.characters)
                    if (cs != null)
                    {
                        var loaded = LoadCharacter(cs);
                        LifeRules.Recompute(loaded, content);
                        result.Characters.Add(loaded);
                    }

            ReleaseWidowed(result.Characters);

            result.Account = new AccountProgress();
            if (data.account != null)
            {
                var counters = new List<KeyValuePair<string, int>>();
                if (data.account.counters != null) foreach (var kv in data.account.counters) if (kv != null) counters.Add(new KeyValuePair<string, int>(kv.key, kv.value));
                result.Account.Restore(data.account.xp, data.account.firsts, data.account.badges, counters, data.account.workDay, data.account.workToday);
                result.Account.RestoreDiamonds(data.account.diamonds, data.account.diamondsSeeded);
            }

            result.Life = data.life;
            result.Temperatures = data.temperatures;
            result.RoomTemperatures = data.roomTemperatures;
            if (data.marketDemand != null) foreach (var kv in data.marketDemand) if (kv != null) result.MarketDemand.Add(new KeyValuePair<string, double>(kv.key, kv.value));
            if (data.marketTakenOffers != null) result.MarketTakenOffers.AddRange(data.marketTakenOffers);
            if (data.marketRawSold != null) foreach (var kv in data.marketRawSold) if (kv != null) result.MarketRawSold.Add(new KeyValuePair<string, int>(kv.key, (int)kv.value));
            result.MarketRawWeek = data.marketRawWeek;
            if (data.affection != null) foreach (var a in data.affection) if (a != null) result.Affection.Add((a.a, a.b, a.value));
            if (data.dynasty != null)
            {
                var d = new Dynasty
                {
                    Name = string.IsNullOrEmpty(data.dynasty.name) ? "Hanedan" : data.dynasty.name,
                    SettlementName = Settlement.Clean(data.dynasty.settlement)
                };
                var hist = new List<HeadRecord>();
                if (data.dynasty.history != null)
                    foreach (var h in data.dynasty.history)
                        if (h != null) hist.Add(new HeadRecord { CharacterId = h.characterId, Name = h.name, Generation = Math.Max(1, h.generation), StartDay = h.startDay, EndDay = h.endDay,
                            EndCause = Enum.IsDefined(typeof(DeathCause), h.endCause) ? (DeathCause)h.endCause : DeathCause.None, Trade = h.trade });
                d.Restore(data.dynasty.headId, hist, data.dynasty.ended);
                result.Dynasty = d;
            }
            if (data.animals != null) result.Animals.AddRange(data.animals);
            if (data.soil != null) result.Soil.AddRange(data.soil);
            if (data.pasture != null) result.Pasture.AddRange(data.pasture);
            result.AnimalsHeld = data.animalsHeld;
            result.AnimalPedigree = data.animalPedigree;
            result.AnimalsAway = data.animalsAway;
            if (data.mines != null) result.Mines.AddRange(data.mines);
            return result;
        }

        private static bool KeepsInInventory(ItemDef def) =>
            def.Placement != ItemPlacement.Resource && !def.IsPlant && !def.IsForage && def.Category != "memorial";

        // Eski kayıtları onarır: eşi ölmüş (ya da kayıtta olmayan) yaşayanın evlilik bağı çözülür, yeniden evlenebilir.
        private static void ReleaseWidowed(List<Character> characters)
        {
            var byId = new Dictionary<int, Character>();
            foreach (var c in characters) byId[c.Id] = c;
            foreach (var c in characters)
            {
                if (c.IsDead || !c.SpouseId.HasValue) continue;
                if (!byId.TryGetValue(c.SpouseId.Value, out var spouse) || spouse.IsDead) c.SpouseId = null;
            }
        }

        private static Character LoadCharacter(CharacterSave s)
        {
            var c = new Character(s.id, string.IsNullOrWhiteSpace(s.name) ? "İsimsiz" : s.name, new GridPos(s.x, s.y, s.z), s.isPlayer)
            {
                Sex = Enum.IsDefined(typeof(Sex), s.sex) ? (Sex)s.sex : Sex.Female,
                Duty = Enum.IsDefined(typeof(DutyKind), s.duty) ? (DutyKind)s.duty : DutyKind.None
            };
            c.RestoreBiology(Math.Max(0, s.ageDays), s.lifespanDays > 0 ? s.lifespanDays : c.LifespanDays, s.dead,
                Enum.IsDefined(typeof(DeathCause), s.deathCause) ? (DeathCause)s.deathCause : DeathCause.None,
                s.pregnant, s.fatherId >= 0 ? s.fatherId : (int?)null, s.pregnancyLeft, s.postpartumLeft, s.freezeDays, s.starveDays);
            c.Needs.Set(NeedType.Hunger, s.hunger);
            c.Needs.Set(NeedType.Energy, s.energy);
            c.Needs.Set(NeedType.Warmth, s.warmth);
            c.Needs.Set(NeedType.Fun, s.fun);
            c.Needs.Set(NeedType.Hygiene, s.hygiene);
            c.Needs.Set(NeedType.Bladder, s.bladder >= 0f ? s.bladder : NeedSet.Max);
            if (s.wet > 0f) c.Soak(s.wet);
            if (s.moodlets != null)
            {
                var ms = new List<KeyValuePair<string, double>>();
                foreach (var kv in s.moodlets) if (kv != null) ms.Add(new KeyValuePair<string, double>(kv.key, kv.value));
                c.Mood.Restore(ms, MoodCatalog.Default);
            }
            if (s.social >= 0f) c.Needs.Set(NeedType.Social, s.social);
            c.SetTraits(s.traits);
            c.SetBranches(s.branches);
            if (s.extraMoodlets != null)
                foreach (var m in s.extraMoodlets)
                    if (m != null && !string.IsNullOrEmpty(m.id) && !c.Mood.Has(m.id))
                        c.Mood.Add(new MoodletDef { Id = m.id, DisplayName = m.text, Description = m.text, Value = m.value, DurationDays = m.days < 0 ? 0 : Math.Max(0.001, m.days) });
            if (s.skills != null)
                foreach (var kv in s.skills)
                    if (kv != null && Enum.TryParse(kv.key, out SkillType skill)) c.Skills.RestoreXp(skill, kv.value);
            if (s.aptitudes != null)
                foreach (var kv in s.aptitudes)
                    if (kv != null && Enum.TryParse(kv.key, out SkillType skill) && kv.value >= 1 && kv.value <= 2)
                        c.Skills.SetAptitude(skill, (Aptitude)(int)kv.value);
            if (s.skillToday != null)
                foreach (var kv in s.skillToday)
                    if (kv != null && Enum.TryParse(kv.key, out SkillType skill)) c.Skills.RestoreToday(skill, kv.value);
            if (s.skillUsedSeason != null)
                foreach (var id in s.skillUsedSeason)
                    if (id != null && Enum.TryParse(id, out SkillType skill)) c.Skills.RestoreUsed(skill);
            c.Skills.ElderOnsetApplied = s.elderOnset;
            if (s.appearance != null)
            {
                c.Appearance = LoadAppearance(s.appearance);
                c.Genome = Genome.Import(s.appearance.genome);   // eski kayıtta null: ilk doğumda görünüşten kurulur
            }
            c.BirthSeason = s.birthSeason;
            c.BirthDayOfSeason = s.birthDay;
            c.LikedFood = s.likedFood ?? ""; c.LikedColor = s.likedColor ?? ""; c.LikedHobby = s.likedHobby ?? ""; c.DislikedFood = s.dislikedFood ?? "";
            c.FreeMirrorAvailable = s.freeMirror;
            c.LastAccidentDay = s.lastAccidentDay;
            c.RestoreBreakdown((BreakdownKind)s.breakdown, s.breakdownLeft);
            c.MiserableDays = s.miserableDays;
            c.LastBreakdownAge = s.lastBreakdownAge;
            c.UpbringingMoodSum = s.upbringingSum;
            c.UpbringingDays = s.upbringingDays;
            c.RestoreChronicle(s.chronicle);
            if (s.injury != null && !string.IsNullOrEmpty(s.injury.kind))
                c.Injury = new ActiveInjury
                {
                    Kind = s.injury.kind, Zone = s.injury.zone, Pattern = s.injury.pattern, Reason = s.injury.reason,
                    StartDay = s.injury.startDay, HealDay = s.injury.healDay, Deep = s.injury.deep, Treated = s.injury.treated
                };
            if (s.spouseId > 0) c.SpouseId = s.spouseId;
            if (s.motherId > 0) c.BirthMotherId = s.motherId;
            if (s.birthFatherId > 0) c.BirthFatherId = s.birthFatherId;
            c.Generation = Math.Max(1, s.generation);
            c.IsVillager = s.isVillager;
            return c;
        }

        private static Appearance LoadAppearance(AppearanceSave s)
        {
            var a = new Appearance();
            if (Enum.IsDefined(typeof(BodyBuild), s.build)) a.Build = (BodyBuild)s.build;
            if (s.heightPercent > 0) a.HeightPercent = s.heightPercent;
            Rgb c;
            if (Rgb.TryParseHex(s.skin, out c)) a.SkinColor = c;
            if (Rgb.TryParseHex(s.eye, out c)) a.EyeColor = c;
            if (!string.IsNullOrEmpty(s.hairStyle)) a.HairStyleId = s.hairStyle;
            if (Rgb.TryParseHex(s.hair, out c)) a.HairColor = c;
            if (!string.IsNullOrEmpty(s.facialHairStyle)) a.FacialHairId = s.facialHairStyle;
            if (Rgb.TryParseHex(s.facialHair, out c)) a.FacialHairColor = c;
            if (!string.IsNullOrEmpty(s.faceMark)) a.FaceMarkId = s.faceMark;
            if (!string.IsNullOrEmpty(s.outfit)) a.OutfitId = s.outfit;
            if (Rgb.TryParseHex(s.outfitColor, out c)) a.OutfitColor = c;
            if (Rgb.TryParseHex(s.trim, out c)) a.TrimColor = c;
            if (!string.IsNullOrEmpty(s.hat)) a.HatId = s.hat;
            if (Rgb.TryParseHex(s.hatColor, out c)) a.HatColor = c;
            a.HeightPercent = 100;   // boy kaldırıldı (17.11.2026)

            // Karakter yaratma 2.0 alanları; eski kayıtta yoktur.
            if (Enum.IsDefined(typeof(BodyFrame), s.frame)) a.Frame = (BodyFrame)s.frame;
            if (!string.IsNullOrEmpty(s.faceShape)) a.FaceShapeId = s.faceShape;
            if (!string.IsNullOrEmpty(s.eyeShape)) a.EyeShapeId = s.eyeShape;
            if (Rgb.TryParseHex(s.eyeRight, out c)) a.EyeColorRight = c;
            if (!string.IsNullOrEmpty(s.brow)) a.BrowId = s.brow;
            if (Rgb.TryParseHex(s.browColor, out c)) a.BrowColor = c;
            a.BrowFollowsHair = s.browOwn == 0;
            if (!string.IsNullOrEmpty(s.nose)) a.NoseId = s.nose;
            if (!string.IsNullOrEmpty(s.mouth)) a.MouthId = s.mouth;
            if (!string.IsNullOrEmpty(s.ears)) a.EarsId = s.ears;
            if (!string.IsNullOrEmpty(s.bangs)) a.BangsId = s.bangs;
            if (s.mustache != null || s.beard != null)
            {
                a.MustacheId = string.IsNullOrEmpty(s.mustache) ? Appearance.None : s.mustache;
                a.BeardId = string.IsNullOrEmpty(s.beard) ? Appearance.None : s.beard;
            }
            if (!string.IsNullOrEmpty(s.hairExtra)) a.HairExtraId = s.hairExtra;
            if (!string.IsNullOrEmpty(s.hairOrnament)) a.HairOrnamentId = s.hairOrnament;
            if (Rgb.TryParseHex(s.hairOrnamentColor, out c)) a.HairOrnamentColor = c;
            if (!string.IsNullOrEmpty(s.sideburns)) a.SideburnsId = s.sideburns;
            if (!string.IsNullOrEmpty(s.lashes)) a.LashesId = s.lashes;
            a.StubbleLevel = Math.Max(0, Math.Min(Appearance.MaxStubble, s.stubble));
            if (!string.IsNullOrEmpty(s.pattern)) a.PatternId = s.pattern;
            if (!string.IsNullOrEmpty(s.bottom)) a.BottomId = s.bottom;
            if (!string.IsNullOrEmpty(s.shoes)) a.ShoesId = s.shoes;
            if (!string.IsNullOrEmpty(s.outer)) a.OuterId = s.outer;
            if (Rgb.TryParseHex(s.outerColor, out c)) a.OuterColor = c;
            if (Enum.IsDefined(typeof(VoiceTimbre), s.voice)) a.Voice = (VoiceTimbre)s.voice;
            a.VoicePitch = Math.Max(Appearance.MinPitch, Math.Min(Appearance.MaxPitch, s.voicePitch));
            if (!string.IsNullOrEmpty(s.laugh)) a.LaughId = s.laugh;
            if (s.marks != null)
            {
                // Yeni kayıt: işaretler dizisi doğrudan; eski tek yuvalı faceMark'ın yerine geçer.
                a.Marks.Clear();
                foreach (var m in s.marks)
                {
                    if (m == null || string.IsNullOrEmpty(m.id) || !Enum.IsDefined(typeof(MarkKind), m.kind)) continue;
                    var mark = new Mark
                    {
                        Kind = (MarkKind)m.kind, Id = m.id, Zone = m.zone, Fade = Math.Max(0, Math.Min(2, m.fade)),
                        Source = Enum.IsDefined(typeof(MarkSource), m.source) ? (MarkSource)m.source : MarkSource.Chosen,
                        Day = m.day, Reason = m.reason, Hidden = m.hidden
                    };
                    if (Rgb.TryParseHex(m.color, out c)) mark.Color = c;
                    a.Marks.Add(mark);
                }
            }
            return a;
        }
    }

    /// <summary>
    /// Kayıt sürüm yükseltmeleri (T16). Her yeni sürümde buraya "eski → yeni" adımı eklenir ve
    /// eski bir kayıt örneği testlere konur.
    /// v1 → v2: keyif ve hijyen ihtiyaçları eklendi (eski kayıtta yok: tam doluyla başlar), moodlet'ler,
    /// hayvanlar ve barınak stokları (eksik alanlar boş gelir, ek iş gerekmez).
    /// </summary>
    public static class SaveMigrator
    {
        public static void Upgrade(SaveData data)
        {
            if (data.version <= 0) data.version = 1;   // sürümsüz (en eski) dosya: 1 sayılır
            if (data.version > SaveGame.CurrentVersion)
                throw new InvalidOperationException($"Kayıt daha yeni bir oyun sürümünden ({data.version}); güncelleme gerekli.");
            if (data.version == 1)
            {
                if (data.characters != null)
                    foreach (var c in data.characters)
                    {
                        if (c == null) continue;
                        c.fun = 100f;
                        c.hygiene = 100f;
                    }
                data.version = 2;
            }
            // 2 → 3: karo dizileri sıkıştırılmış metinde, sıcaklık oda başına. Yükleyici iki biçimi de okur; alan dönüşümü yok.
            if (data.version == 2) data.version = 3;
        }
    }
}
