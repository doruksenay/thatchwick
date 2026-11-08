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
using Thatchwick.Core.Mood;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Time;
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
        public List<KeyValuePair<string, double>> MarketDemand = new List<KeyValuePair<string, double>>();
        public List<int> MarketTakenOffers = new List<int>();
        public List<(int a, int b, int value)> Affection = new List<(int, int, int)>();
        /// <summary>Kayıtta hanedan yoksa null (eski kayıt): oyuncu karakteriyle yeniden kurulur.</summary>
        public Dynasty Dynasty;
        /// <summary>Hayvanlar (v2); AnimalSystem kurulunca SaveGame.RestoreAnimals ile geri konur.</summary>
        public List<AnimalSave> Animals = new List<AnimalSave>();
        /// <summary>Yüklenemeyen parçalar (ör. içerikten kaldırılmış eşya). Oyun yine açılır.</summary>
        public List<string> Warnings = new List<string>();
    }

    /// <summary>Oyun durumunu SaveData'ya çevirir ve geri kurar (T16: sürümlü, geriye uyumlu).</summary>
    public static class SaveGame
    {
        public const int CurrentVersion = 2;

        public static SaveData Capture(GameClock clock, WorldState world, Household house, IEnumerable<Character> characters,
            AccountProgress account, HeatSystem heat, Market market, FamilyService family = null, Dynasty dynasty = null,
            AnimalSystem animals = null)
        {
            var map = world.Map;
            var data = new SaveData
            {
                version = CurrentVersion,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                totalDays = clock.TotalDays,
                speed = clock.RequestedSpeed
            };

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
            foreach (var kv in map.Edges) edges.Add(new EdgeSave { x = kv.Key.X, y = kv.Key.Y, axis = (int)kv.Key.Axis, kind = (int)kv.Value });
            edges.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x != b.x ? a.x.CompareTo(b.x) : a.axis.CompareTo(b.axis));
            m.edges = edges.ToArray();
            data.map = m;

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
                        id = it.Id, def = it.Def.Id, x = it.Position.X, y = it.Position.Y, rotation = it.Rotation,
                        parentId = it.ParentId ?? -1, slotId = it.SlotId, fuelDays = it.FuelDays, growthDays = it.GrowthDays,
                        feedStock = it.FeedStock, stock = stock
                    });
                }
            data.items = items.ToArray();

            var inv = new List<KeyInt>();
            foreach (var kv in house.InventoryList()) inv.Add(new KeyInt { key = kv.Key, value = kv.Value });
            data.household = new HouseholdSave { akce = house.Akce, inventory = inv.ToArray() };

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
                    workToday = account.WorkXpTodayRaw
                };
            }

            data.temperatures = heat?.ExportTemperatures();
            if (market != null)
            {
                var demand = new List<KeyFloat>();
                foreach (var kv in market.ExportDemand()) demand.Add(new KeyFloat { key = kv.Key, value = kv.Value });
                data.marketDemand = demand.ToArray();
                data.marketTakenOffers = market.ExportTakenOffers().ToArray();
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
                    hist.Add(new HeadSave { characterId = h.CharacterId, name = h.Name, generation = h.Generation, startDay = h.StartDay, endDay = h.EndDay, endCause = (int)h.EndCause });
                data.dynasty = new DynastySave { name = dynasty.Name, headId = dynasty.HeadId, ended = dynasty.Ended, history = hist.ToArray() };
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
                        fullness = a.Fullness, age = a.AgeDays, produce = a.ProduceProgress, hungryDays = a.HungryDays
                    });
                }
                data.animals = list.ToArray();
            }
            return data;
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
            return new CharacterSave
            {
                id = c.Id, name = c.Name, isPlayer = c.IsPlayerControlled, sex = (int)c.Sex,
                x = c.Position.X, y = c.Position.Y,
                ageDays = c.AgeDays, lifespanDays = c.LifespanDays,
                dead = c.IsDead, deathCause = (int)c.CauseOfDeath,
                pregnant = c.IsPregnant, fatherId = c.FatherId ?? -1,
                pregnancyLeft = c.PregnancyLeft, postpartumLeft = c.PostpartumLeft,
                freezeDays = c.FreezeDays, starveDays = c.StarveDays,
                hunger = c.Needs.Get(NeedType.Hunger), energy = c.Needs.Get(NeedType.Energy), warmth = c.Needs.Get(NeedType.Warmth),
                fun = c.Needs.Get(NeedType.Fun), hygiene = c.Needs.Get(NeedType.Hygiene),
                moodlets = CaptureMoodlets(c),
                skills = skills.ToArray(),
                spouseId = c.SpouseId ?? 0,
                motherId = c.BirthMotherId ?? 0,
                birthFatherId = c.BirthFatherId ?? 0,
                generation = c.Generation,
                isVillager = c.IsVillager,
                appearance = new AppearanceSave
                {
                    build = (int)a.Build, heightPercent = a.HeightPercent,
                    skin = a.SkinColor.ToHex(), eye = a.EyeColor.ToHex(),
                    hairStyle = a.HairStyleId, hair = a.HairColor.ToHex(),
                    facialHairStyle = a.FacialHairId, facialHair = a.FacialHairColor.ToHex(),
                    faceMark = a.FaceMarkId, outfit = a.OutfitId, outfitColor = a.OutfitColor.ToHex(), trim = a.TrimColor.ToHex(),
                    hat = a.HatId, hatColor = a.HatColor.ToHex()
                }
            };
        }

        private static KeyFloat[] CaptureMoodlets(Character c)
        {
            var list = new List<KeyFloat>();
            foreach (var kv in c.Mood.Export()) list.Add(new KeyFloat { key = kv.Key, value = kv.Value });
            return list.ToArray();
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
                system.Restore(s.id, species, new GridPos(s.x, s.y), s.home, s.fullness, s.age, s.produce, s.hungryDays);
            }
        }

        /// <summary>Kaydı geri kurar. Sürüm yeniyse ya da harita yoksa InvalidOperationException.</summary>
        public static LoadedGame Load(SaveData data, ContentDatabase content, TimeConfig timeConfig = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            SaveMigrator.Upgrade(data);
            if (data.map == null || data.map.width <= 0 || data.map.height <= 0) throw new InvalidOperationException("Kayıtta harita yok.");

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
            if (m.edges != null)
                foreach (var e in m.edges)
                {
                    var edge = new Edge(e.x, e.y, (EdgeAxis)e.axis);
                    if (map.EdgeInBounds(edge) && Enum.IsDefined(typeof(EdgeKind), e.kind)) map.SetEdge(edge, (EdgeKind)e.kind);
                }

            var world = new WorldState(map);
            result.World = world;
            if (data.items != null)
                foreach (var s in data.items)
                {
                    if (s == null) continue;
                    if (!content.Items.TryGet(s.def, out var def)) { result.Warnings.Add($"Eşya tanımı yok, atlandı: {s.def}"); continue; }
                    int? parent = s.parentId >= 0 ? s.parentId : (int?)null;
                    if (parent.HasValue && world.GetItem(parent.Value) == null) { result.Warnings.Add($"Dekorun yüzeyi yok, atlandı: {s.def}"); continue; }
                    var pos = new GridPos(s.x, s.y);
                    if (!parent.HasValue)
                    {
                        var fits = def.Placement == ItemPlacement.Wall ? world.CanPlaceOnWall(def, pos, s.rotation) : world.CanPlaceFloor(def, pos, s.rotation);
                        if (!fits.Ok) { result.Warnings.Add($"Eşya yerine sığmadı, atlandı: {s.def} {pos}"); continue; }
                    }
                    var restored = world.RestoreItem(def, s.id, pos, s.rotation, parent, s.slotId, Math.Max(0, s.fuelDays), Math.Max(0, s.growthDays));
                    restored.FeedStock = Math.Max(0, s.feedStock);
                    if (s.stock != null) foreach (var kv in s.stock) if (kv != null && kv.value > 0 && content.Items.Contains(kv.key)) restored.SetStock(kv.key, kv.value);
                }

            var hs = data.household ?? new HouseholdSave();
            var house = new Household(Math.Max(0, hs.akce));
            if (hs.inventory != null)
                foreach (var kv in hs.inventory)
                {
                    if (kv == null || kv.value <= 0) continue;
                    if (!content.Items.Contains(kv.key)) { result.Warnings.Add($"Envanterde bilinmeyen eşya, atlandı: {kv.key}"); continue; }
                    house.AddItem(kv.key, kv.value);
                }
            result.Household = house;

            if (data.characters != null)
                foreach (var cs in data.characters)
                    if (cs != null) result.Characters.Add(LoadCharacter(cs));

            result.Account = new AccountProgress();
            if (data.account != null)
            {
                var counters = new List<KeyValuePair<string, int>>();
                if (data.account.counters != null) foreach (var kv in data.account.counters) if (kv != null) counters.Add(new KeyValuePair<string, int>(kv.key, kv.value));
                result.Account.Restore(data.account.xp, data.account.firsts, data.account.badges, counters, data.account.workDay, data.account.workToday);
            }

            result.Temperatures = data.temperatures;
            if (data.marketDemand != null) foreach (var kv in data.marketDemand) if (kv != null) result.MarketDemand.Add(new KeyValuePair<string, double>(kv.key, kv.value));
            if (data.marketTakenOffers != null) result.MarketTakenOffers.AddRange(data.marketTakenOffers);
            if (data.affection != null) foreach (var a in data.affection) if (a != null) result.Affection.Add((a.a, a.b, a.value));
            if (data.dynasty != null)
            {
                var d = new Dynasty { Name = string.IsNullOrEmpty(data.dynasty.name) ? "Hanedan" : data.dynasty.name };
                var hist = new List<HeadRecord>();
                if (data.dynasty.history != null)
                    foreach (var h in data.dynasty.history)
                        if (h != null) hist.Add(new HeadRecord { CharacterId = h.characterId, Name = h.name, Generation = Math.Max(1, h.generation), StartDay = h.startDay, EndDay = h.endDay,
                            EndCause = Enum.IsDefined(typeof(DeathCause), h.endCause) ? (DeathCause)h.endCause : DeathCause.None });
                d.Restore(data.dynasty.headId, hist, data.dynasty.ended);
                result.Dynasty = d;
            }
            if (data.animals != null) result.Animals.AddRange(data.animals);
            return result;
        }

        private static Character LoadCharacter(CharacterSave s)
        {
            var c = new Character(s.id, string.IsNullOrWhiteSpace(s.name) ? "İsimsiz" : s.name, new GridPos(s.x, s.y), s.isPlayer)
            {
                Sex = Enum.IsDefined(typeof(Sex), s.sex) ? (Sex)s.sex : Sex.Female
            };
            c.RestoreBiology(Math.Max(0, s.ageDays), s.lifespanDays > 0 ? s.lifespanDays : c.LifespanDays, s.dead,
                Enum.IsDefined(typeof(DeathCause), s.deathCause) ? (DeathCause)s.deathCause : DeathCause.None,
                s.pregnant, s.fatherId >= 0 ? s.fatherId : (int?)null, s.pregnancyLeft, s.postpartumLeft, s.freezeDays, s.starveDays);
            c.Needs.Set(NeedType.Hunger, s.hunger);
            c.Needs.Set(NeedType.Energy, s.energy);
            c.Needs.Set(NeedType.Warmth, s.warmth);
            c.Needs.Set(NeedType.Fun, s.fun);
            c.Needs.Set(NeedType.Hygiene, s.hygiene);
            if (s.moodlets != null)
            {
                var ms = new List<KeyValuePair<string, double>>();
                foreach (var kv in s.moodlets) if (kv != null) ms.Add(new KeyValuePair<string, double>(kv.key, kv.value));
                c.Mood.Restore(ms, MoodCatalog.Default);
            }
            if (s.skills != null)
                foreach (var kv in s.skills)
                    if (kv != null && Enum.TryParse(kv.key, out SkillType skill)) c.Skills.RestoreXp(skill, kv.value);
            if (s.appearance != null) c.Appearance = LoadAppearance(s.appearance);
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
        }
    }
}
