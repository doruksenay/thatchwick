using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Content
{
    /// <summary>JSON şeklindeki (DTO) içeriği ContentDatabase'e kaydeder ve hataları toplar.</summary>
    public static class ContentMapper
    {
        /// <summary>Hata listesini döner; boşsa içerik eksiksiz yüklendi. Hatalı kayıtlar atlanır, diğerleri yüklenir.</summary>
        public static List<string> Apply(ContentFileDto file, ContentDatabase db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            var errors = new List<string>();
            bool hasLife = file != null && (file.traits != null || file.branches != null || file.projects != null || file.villagers != null
                || file.festivals != null || file.goals != null || file.tools != null || file.templates != null);
            if (file == null || (file.items == null && !hasLife))
            {
                errors.Add("İçerik dosyası boş ya da okunamadı.");
                return errors;
            }

            foreach (var dto in file.items ?? new ItemDto[0])
            {
                if (dto == null) continue;

                var item = new ItemDef
                {
                    Id = dto.id,
                    DisplayName = dto.displayName,
                    Width = dto.width <= 0 ? 1 : dto.width,
                    Height = dto.height <= 0 ? 1 : dto.height,
                    Price = dto.price,
                    HeatOutput = dto.heat,
                    GrowDays = dto.growDays,
                    NeedsTilledSoil = dto.tilled,
                    Storage = dto.storage,
                    UnlockLevel = dto.level <= 0 ? 1 : dto.level,
                    Tags = dto.tags != null ? new List<string>(dto.tags) : new List<string>(),
                    DiesInWinter = dto.diesInWinter,
                    Comfort = dto.comfort,
                    SpoilDays = dto.spoilDays,
                    Preserves = dto.preserves,
                    Variants = dto.variants != null ? new List<string>(dto.variants) : new List<string>(),
                    ForageMax = dto.forageMax,
                    FruitRipenDays = dto.fruitDays,
                    FixesNitrogen = dto.nitrogen,
                    CropFamily = string.IsNullOrWhiteSpace(dto.cropFamily) ? null : dto.cropFamily,
                    ThreshesTo = string.IsNullOrWhiteSpace(dto.threshesTo) ? null : dto.threshesTo,
                    ThreshPerDay = Math.Max(0, dto.threshPerDay),
                    WaterRadius = dto.waterRadius,
                    ProtectRadius = dto.protectRadius,
                    PollinateRadius = dto.pollinate,
                    PlantsAs = string.IsNullOrWhiteSpace(dto.plantsAs) ? null : dto.plantsAs,
                    Category = string.IsNullOrWhiteSpace(dto.category) ? "furniture" : dto.category,
                    StyleSet = string.IsNullOrWhiteSpace(dto.styleSet) ? null : dto.styleSet,
                    Rooms = dto.rooms != null ? new List<string>(dto.rooms) : new List<string>(),
                    HousingKind = string.IsNullOrWhiteSpace(dto.housing) ? null : dto.housing,
                    HousingCapacity = dto.capacity,
                    StairsDelta = dto.stairs,
                    FinishGroup = string.IsNullOrWhiteSpace(dto.finishGroup) ? null : dto.finishGroup,
                    RequiresIndoor = dto.requiresIndoor,
                    IsIceBox = dto.iceBox,
                    IsFishTrap = dto.fishTrap,
                    WindPower = dto.windPower,
                    WaterGate = dto.waterGate,
                    FillCapacity = Math.Max(0, dto.fillCapacity),
                    FillProduct = string.IsNullOrWhiteSpace(dto.fillProduct) ? null : dto.fillProduct,
                    FillPerProduct = dto.fillPerProduct <= 0 ? 4 : dto.fillPerProduct,
                    HarvestRadius = Math.Max(0, dto.harvestRadius),
                    SeedRadius = Math.Max(0, dto.seedRadius),
                    IceCollect = Math.Max(0, dto.iceCollect),
                    FoodCapacity = Math.Max(0, dto.foodCapacity),
                    IsTrough = dto.trough,
                    AnimalComfort = dto.animalComfort,
                    AnimalRole = string.IsNullOrWhiteSpace(dto.animalRole) ? null : dto.animalRole,
                    WaterSource = dto.waterSource,
                    NeedsWaterSource = dto.needsSource,
                    PowerSource = dto.powerSource,
                    WaterFlow = dto.waterFlow < 0 ? -1 : dto.waterFlow,
                    WaterUse = Math.Max(0, dto.waterUse),
                    WaterHead = dto.waterHead,
                    CisternCapacity = Math.Max(0, dto.cistern),
                    CisternRain = Math.Max(0, dto.cisternRain),
                    CisternFlow = Math.Max(0, dto.cisternFlow),
                    IsTap = dto.tap,
                    AutoProcess = dto.autoProcess == null || string.IsNullOrWhiteSpace(dto.autoProcess.item) ? null : new AutoProcessDef
                    {
                        Item = dto.autoProcess.item, Count = Math.Max(1, dto.autoProcess.count),
                        Produces = dto.autoProcess.produces, ProducesCount = Math.Max(1, dto.autoProcess.producesCount)
                    },
                    AllowedLevels = dto.levels != null ? new List<int>(dto.levels) : new List<int>()
                };
                if (dto.stairs != 0 && (dto.width * dto.height != 2 && (dto.width > 0 || dto.height > 0)))
                    errors.Add($"{dto.id}: merdiven 1x2 olmalı.");
                foreach (var lv in item.AllowedLevels)
                    if (!Thatchwick.Core.World.GridMap.IsValidLevel(lv)) errors.Add($"{dto.id}: geçersiz kat {lv}.");

                if (!string.IsNullOrWhiteSpace(dto.placement))
                {
                    if (!Enum.TryParse(dto.placement, true, out ItemPlacement placement) || !Enum.IsDefined(typeof(ItemPlacement), placement))
                        errors.Add($"{dto.id}: bilinmeyen yerleşim '{dto.placement}'.");
                    else
                        item.Placement = placement;
                }

                // Ev dekorasyonu (27): boyut, yük, uyduğu yuvalar, duvar bantları, güzellik, malzeme, ayna.
                if (!string.IsNullOrWhiteSpace(dto.decorSize))
                {
                    if (TryParseDecorSize(dto.decorSize, out var ds)) item.DecorSize = ds;
                    else errors.Add($"{dto.id}: bilinmeyen dekor boyutu '{dto.decorSize}'.");
                }
                item.Weight = dto.weight <= 0 ? 1 : dto.weight;
                item.MaxLoad = Math.Max(0, dto.maxLoad);
                item.WallWidth = dto.wallWidth <= 0 ? 1 : dto.wallWidth;
                item.Tall = dto.tall;
                item.Beauty = dto.beauty;
                item.Material = string.IsNullOrWhiteSpace(dto.material) ? null : dto.material.Trim();
                item.IsMirror = dto.mirror;
                item.TarnishDays = Math.Max(0f, dto.tarnishDays);
                item.StyleTheme = string.IsNullOrWhiteSpace(dto.styleTheme) ? null : dto.styleTheme.Trim();
                if (dto.fillWith != null) foreach (var f in dto.fillWith) if (!string.IsNullOrWhiteSpace(f)) item.FillWith.Add(f.Trim());
                item.FillCount = Math.Max(0, dto.fillCount);
                item.WindowMount = dto.window;
                item.Insulation = Math.Max(0f, dto.insulation);
                item.RequiresNearby = string.IsNullOrEmpty(dto.requiresNear) ? null : dto.requiresNear;
                item.NearbyRadius = dto.nearRadius > 0 ? dto.nearRadius : 4;
                item.ShopUnlock = string.IsNullOrEmpty(dto.shopUnlock) ? null : dto.shopUnlock;
                if (dto.fitsOn != null) foreach (var f in dto.fitsOn) if (!string.IsNullOrWhiteSpace(f)) item.FitsOn.Add(f.Trim());
                if (dto.wallBands != null)
                    foreach (var b in dto.wallBands)
                    {
                        if (TryParseBand(b, out int band)) { if (!item.WallBands.Contains(band)) item.WallBands.Add(band); }
                        else errors.Add($"{dto.id}: bilinmeyen duvar bandı '{b}'.");
                    }

                if (dto.growSeasons != null)
                    foreach (var gs in dto.growSeasons)
                    {
                        if (LifeContentMapper.TryParseSeason(gs, out var gsSeason)) item.GrowSeasons.Add(gsSeason);
                        else errors.Add($"{dto.id}: bilinmeyen büyüme mevsimi '{gs}'.");
                    }
                if (dto.seasonal != null)
                    foreach (var ss in dto.seasonal)
                    {
                        if (LifeContentMapper.TryParseSeason(ss, out var sSeason)) { if (!item.DecorSeasons.Contains(sSeason)) item.DecorSeasons.Add(sSeason); }
                        else errors.Add($"{dto.id}: bilinmeyen süs mevsimi '{ss}'.");
                    }
                if (dto.forageSeasons != null)
                    foreach (var fs in dto.forageSeasons)
                    {
                        if (LifeContentMapper.TryParseSeason(fs, out var fsSeason)) item.ForageSeasons.Add(fsSeason);
                        else errors.Add($"{dto.id}: bilinmeyen toplama mevsimi '{fs}'.");
                    }

                if (dto.fruitSeasons != null)
                    foreach (var fr in dto.fruitSeasons)
                    {
                        if (LifeContentMapper.TryParseSeason(fr, out var frSeason)) item.FruitSeasons.Add(frSeason);
                        else errors.Add($"{dto.id}: bilinmeyen meyve mevsimi '{fr}'.");
                    }
                if (item.FruitSeasons.Count > 0 && (item.FruitRipenDays <= 0f || item.GrowDays <= 0f))
                    errors.Add($"{dto.id}: meyve ağacı için growDays ve fruitDays gerekir.");

                if (dto.surfaceSlots != null)
                {
                    foreach (var s in dto.surfaceSlots)
                    {
                        if (s == null) continue;
                        var slot = new SurfaceSlotDef { Id = s.id, U = s.u, V = s.v, Kind = string.IsNullOrWhiteSpace(s.kind) ? null : s.kind.Trim() };
                        if (!string.IsNullOrWhiteSpace(s.size))
                        {
                            if (TryParseDecorSize(s.size, out var ss)) slot.Size = ss;
                            else errors.Add($"{dto.id}/{s.id}: bilinmeyen yuva boyutu '{s.size}'.");
                        }
                        item.SurfaceSlots.Add(slot);
                    }
                }

                if (dto.interactions != null)
                {
                    foreach (var i in dto.interactions)
                    {
                        if (i == null) continue;
                        bool hasEffects = !string.IsNullOrWhiteSpace(i.consume) || (i.produce != null && i.produce.Length > 0) || i.removeItem
                            || (i.materials != null && i.materials.Length > 0) || i.addFeed > 0 || i.collect || i.requiresAnimal
                            || i.addIce > 0f || i.requiresFrost || i.piped || i.befriendAnimals > 0f || i.teach || i.heal > 0f || i.wets > 0f || i.bath || i.addFill > 0 || i.emptyFill || i.dries || i.water || i.fertilize > 0f || i.takesFruit || i.requiresFruit || i.petAnimals || i.slaughter
                            || !string.IsNullOrWhiteSpace(i.repair) || i.polish || i.collectContents || i.fillDecor || i.eatContents;
                        NeedType need = default;
                        // Saf iş etkileşimi: ihtiyaç doyurmaz, etkisi, becerisi ya da moodlet'i vardır (baltayı bilemek).
                        bool needless = string.IsNullOrWhiteSpace(i.need) && (hasEffects || !string.IsNullOrWhiteSpace(i.skill) || !string.IsNullOrWhiteSpace(i.moodlet));
                        if (!needless && !TryParseNeed(i.need, out need))
                        {
                            errors.Add($"{dto.id}/{i.id}: bilinmeyen ihtiyaç '{i.need}'.");
                            continue;
                        }

                        SkillType? skill = null;
                        if (!string.IsNullOrWhiteSpace(i.skill))
                        {
                            if (!Enum.TryParse(i.skill, true, out SkillType parsed) || !Enum.IsDefined(typeof(SkillType), parsed))
                            {
                                errors.Add($"{dto.id}/{i.id}: bilinmeyen beceri '{i.skill}'.");
                                continue;
                            }
                            skill = parsed;
                        }

                        var interaction = new InteractionDef
                        {
                            Id = i.id,
                            DisplayName = i.displayName,
                            Need = need,
                            Restore = i.restore,
                            DurationDays = i.durationDays,
                            Skill = skill,
                            SkillXp = i.skillXp,
                            RequiresGrown = i.requiresGrown,
                            ConsumeItem = string.IsNullOrWhiteSpace(i.consume) ? null : i.consume,
                            RemovesItem = i.removeItem,
                            Moodlet = string.IsNullOrWhiteSpace(i.moodlet) ? null : i.moodlet,
                            MinSkillLevel = i.minLevel,
                            AddFeed = i.addFeed,
                            AddIce = Math.Max(0f, i.addIce),
                            BefriendAnimals = Math.Max(0f, i.befriendAnimals),
                            Teach = i.teach,
                            BladderCost = Math.Max(0f, i.bladder),
                            FetchWater = i.fetchWater,
                            Wets = Math.Max(0f, Math.Min(1f, i.wets)),
                            Bath = i.bath,
                            AddFill = Math.Max(0, i.addFill),
                            EmptyFill = i.emptyFill,
                            Dries = i.dries,
                            Heal = Math.Max(0f, i.heal),
                            RequiresFrost = i.requiresFrost,
                            NeedsPipedWater = i.piped,
                            Collect = i.collect,
                            RequiresAnimal = i.requiresAnimal,
                            Tool = string.IsNullOrWhiteSpace(i.tool) ? null : i.tool,
                            MoodId = string.IsNullOrWhiteSpace(i.moodId) ? null : i.moodId,
                            MoodText = i.moodText,
                            MoodValue = i.moodValue,
                            MoodDays = i.moodDays,
                            RequiresLit = i.requiresLit,
                            Water = i.water,
                            Fertilize = i.fertilize,
                            RequiresFruit = i.requiresFruit,
                            TakesFruit = i.takesFruit,
                            PetsAnimals = i.petAnimals,
                            Slaughter = i.slaughter,
                            RepairMaterial = string.IsNullOrWhiteSpace(i.repair) ? null : i.repair,
                            Polish = i.polish,
                            CollectContents = i.collectContents,
                            FillDecor = i.fillDecor,
                            EatContents = i.eatContents
                        };
                        AddMaterials(interaction, i.materials);
                        bool badExtra = false;
                        if (i.extraRestore != null)
                            foreach (var er in i.extraRestore)
                            {
                                if (er == null) continue;
                                if (!TryParseNeed(er.need, out var extraNeed)) { errors.Add($"{dto.id}/{i.id}: bilinmeyen ek ihtiyaç '{er.need}'."); badExtra = true; break; }
                                interaction.ExtraRestore.Add(new NeedRestoreDef { Need = extraNeed, Amount = er.amount });
                            }
                        if (badExtra) continue;
                        if (i.produce != null)
                        {
                            foreach (var p in i.produce)
                            {
                                if (p == null) continue;
                                interaction.Produce.Add(new ProduceDef
                                {
                                    ItemId = p.item,
                                    Count = p.count <= 0 ? 1 : p.count,
                                    Chance = p.chance <= 0f ? 1f : p.chance,
                                    MinLevel = p.minLevel,
                                    MaxLevel = p.maxLevel,
                                    Seasons = ParseSeasons(p.seasons, $"{dto.id}.{i.id}", errors)
                                });
                            }
                        }
                        item.Interactions.Add(interaction);
                    }
                }

                try
                {
                    db.Items.Register(item);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    errors.Add(ex.Message);
                }
            }

            ApplyRecipes(file.recipes, db, errors);
            ApplyAnimals(file.animals, db, errors);
            ApplyBuildParts(file.buildParts, db, errors);
            ApplyEffects(file.effects, db, errors);

            LifeContentMapper.Apply(file, db, errors);
            errors.AddRange(db.Validate());
            return errors;
        }

        private static void ApplyBuildParts(BuildPartDto[] parts, ContentDatabase db, List<string> errors)
        {
            if (parts == null) return;
            foreach (var dto in parts)
            {
                if (dto == null) continue;
                if (!Enum.TryParse(dto.kind, true, out BuildPartKind kind) || !Enum.IsDefined(typeof(BuildPartKind), kind))
                {
                    errors.Add($"{dto.id}: bilinmeyen yapı parçası türü '{dto.kind}'.");
                    continue;
                }
                var def = new BuildPartDef
                {
                    Id = dto.id,
                    DisplayName = dto.displayName,
                    Kind = kind,
                    Price = dto.price,
                    UnlockLevel = dto.level <= 0 ? 1 : dto.level,
                    Color = string.IsNullOrWhiteSpace(dto.color) ? "#C8B490" : dto.color,
                    HeatLoss = dto.heatLoss <= 0f ? 1f : dto.heatLoss,
                    Comfort = dto.comfort,
                    Group = string.IsNullOrWhiteSpace(dto.group) ? null : dto.group,
                    PriceFactor = dto.priceFactor <= 0f ? 1f : dto.priceFactor,
                    Levels = dto.levels != null ? new List<int>(dto.levels) : new List<int>(),
                    IsDefault = dto.isDefault,
                    MaterialItem = string.IsNullOrWhiteSpace(dto.material) ? null : dto.material,
                    MaterialCount = dto.materialCount <= 0 ? 1 : dto.materialCount
                };
                if (kind == BuildPartKind.Floor)
                {
                    if (string.IsNullOrWhiteSpace(dto.floorBase) || !Enum.TryParse(dto.floorBase, true, out World.FloorType fb)
                        || !Enum.IsDefined(typeof(World.FloorType), fb) || fb == World.FloorType.Grass)
                    {
                        errors.Add($"{dto.id}: zemin parçasının floorBase'i Dirt, Wood ya da Stone olmalı.");
                        continue;
                    }
                    def.Base = fb;
                }
                try { db.Parts.Register(def); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
            }
        }

        private static void AddMaterials(InteractionDef interaction, ConsumeDto[] materials)
        {
            if (materials == null) return;
            foreach (var m in materials)
                if (m != null) interaction.Consume.Add(new ConsumeDef { ItemId = m.item, Count = m.count <= 0 ? 1 : m.count });
        }

        /// <summary>Her tarif, istasyon eşyasına "craft:&lt;id&gt;" kimlikli bir iş etkileşimi olarak eklenir.</summary>
        private static void ApplyRecipes(RecipeDto[] recipes, ContentDatabase db, List<string> errors)
        {
            if (recipes == null) return;
            foreach (var r in recipes)
            {
                if (r == null) continue;
                if (!db.Items.TryGet(r.station, out var station)) { errors.Add($"tarif {r.id}: istasyon '{r.station}' bulunamadı."); continue; }
                if (string.IsNullOrWhiteSpace(r.produces)) { errors.Add($"tarif {r.id}: ürün yok."); continue; }
                SkillType skill = SkillType.Carpentry;
                if (!string.IsNullOrWhiteSpace(r.skill) && (!Enum.TryParse(r.skill, true, out skill) || !Enum.IsDefined(typeof(SkillType), skill)))
                {
                    errors.Add($"tarif {r.id}: bilinmeyen beceri '{r.skill}'.");
                    continue;
                }
                string name = r.displayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = "Yap: " + (db.Items.TryGet(r.produces, out var product) ? product.DisplayName : r.produces);
                var interaction = new InteractionDef
                {
                    Id = "craft:" + r.id,
                    DisplayName = name,
                    DurationDays = r.durationDays,
                    Skill = skill,
                    SkillXp = r.skillXp,
                    MinSkillLevel = r.minLevel,
                    IsCraft = true,
                    Moodlet = string.IsNullOrWhiteSpace(r.moodlet) ? Mood.MoodCatalog.Proud : r.moodlet,
                    RequiresBranch = string.IsNullOrWhiteSpace(r.requiresBranch) ? null : r.requiresBranch.Trim()
                };
                interaction.Produce.Add(new ProduceDef { ItemId = r.produces, Count = r.count <= 0 ? 1 : r.count, Chance = 1f });
                AddMaterials(interaction, r.materials);
                if (interaction.Consume.Count == 0) errors.Add($"tarif {r.id}: malzeme yok.");
                station.Interactions.Add(interaction);
            }
        }

        // Yemek/içki etkileri ruh hali kataloğuna kaydedilir; etkileşimler "moodlet" alanıyla verir.
        private static void ApplyEffects(EffectDto[] effects, ContentDatabase db, List<string> errors)
        {
            if (effects == null) return;
            foreach (var f in effects)
            {
                if (f == null || string.IsNullOrWhiteSpace(f.id)) { errors.Add("Etkinin kimliği boş."); continue; }
                if (f.days <= 0f) { errors.Add($"{f.id}: etki süresi pozitif olmalı."); continue; }
                var m = new Mood.MoodletDef
                {
                    Id = f.id, DisplayName = f.displayName ?? f.id, Description = f.description ?? "", Value = f.value, DurationDays = f.days,
                    AllSpeed = f.allSpeed, AllXp = f.allXp, ColdResist = Math.Max(0f, f.coldResist),
                    HungerDecay = f.hungerDecay > 0f ? f.hungerDecay : 1f, EnergyDecay = f.energyDecay > 0f ? f.energyDecay : 1f
                };
                foreach (var (rates, target) in new[] { (f.skillSpeed, m.SkillSpeed), (f.skillXp, m.SkillXp) })
                    if (rates != null)
                        foreach (var r in rates)
                        {
                            if (r != null && Enum.TryParse(r.skill, true, out SkillType s) && Enum.IsDefined(typeof(SkillType), s)) target[s] = r.rate;
                            else errors.Add($"{f.id}: bilinmeyen beceri '{r?.skill}'.");
                        }
                (db.Moods ?? Mood.MoodCatalog.Default).Register(m);
            }
        }

        private static void ApplyAnimals(AnimalDto[] animals, ContentDatabase db, List<string> errors)
        {
            if (animals == null) return;
            foreach (var a in animals)
            {
                if (a == null) continue;
                var def = new AnimalSpeciesDef
                {
                    Id = a.id,
                    DisplayName = a.displayName,
                    Price = a.price,
                    Housing = a.housing,
                    FeedPerDay = a.feedPerDay,
                    Produces = string.IsNullOrWhiteSpace(a.produces) ? null : a.produces,
                    ProduceEveryDays = a.produceEveryDays <= 0 ? 1f : a.produceEveryDays,
                    SeasonOutput = a.seasonOutput != null && a.seasonOutput.Length == 4 ? a.seasonOutput : null,
                    LifespanDays = a.lifespanDays <= 0 ? 120f : a.lifespanDays,
                    MatureDays = a.matureDays,
                    IsPet = a.pet,
                    Meat = string.IsNullOrWhiteSpace(a.meat) ? null : a.meat,
                    MeatCount = a.meatCount <= 0 ? 1 : a.meatCount,
                    Hide = string.IsNullOrWhiteSpace(a.hide) ? null : a.hide,
                    HideCount = a.hideCount <= 0 ? 1 : a.hideCount,
                    Rideable = a.rideable,
                    RideSpeed = a.rideSpeed <= 0f ? 2f : a.rideSpeed,
                    Trample = a.trample >= 0f ? a.trample : a.housing == "stable" ? 3f : a.housing == "barn" ? 2f : a.housing == "kennel" ? 0.5f : 0f,
                    Space = a.space > 0f ? a.space : a.housing == "coop" ? 1.5f : a.housing == "stable" ? 6f : a.housing == "barn" ? 4f : 2f,
                    UnlockLevel = a.level <= 0 ? 1 : a.level,
                    GestationDays = a.gestationDays > 0f ? a.gestationDays : 1f,
                    LitterMin = Math.Max(1, a.litterMin),
                    LitterMax = Math.Max(Math.Max(1, a.litterMin), a.litterMax),
                    FemaleProduces = a.femaleProduces,
                    Draft = a.draft,
                    FemaleName = string.IsNullOrWhiteSpace(a.femaleName) ? null : a.femaleName,
                    MaleName = string.IsNullOrWhiteSpace(a.maleName) ? null : a.maleName,
                    MaleOnly = a.maleOnly
                };
                try { db.Animals.Register(def); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) { errors.Add(ex.Message); }
            }
        }

        private static bool TryParseNeed(string text, out NeedType need)
        {
            need = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!Enum.TryParse(text, true, out need)) return false;
            return Enum.IsDefined(typeof(NeedType), need);
        }

        private static List<Time.Season> ParseSeasons(string[] names, string where, List<string> errors)
        {
            if (names == null || names.Length == 0) return null;
            var list = new List<Time.Season>();
            foreach (var n in names)
            {
                if (Enum.TryParse(n, true, out Time.Season s) && Enum.IsDefined(typeof(Time.Season), s)) list.Add(s);
                else errors.Add($"{where}: bilinmeyen mevsim '{n}'.");
            }
            return list;
        }

        /// <summary>S/M/L, K/O/B ya da small/medium/large.</summary>
        public static bool TryParseDecorSize(string text, out DecorSize size)
        {
            size = DecorSize.Small;
            switch ((text ?? "").Trim().ToLowerInvariant())
            {
                case "s": case "k": case "small": case "küçük": size = DecorSize.Small; return true;
                case "m": case "o": case "medium": case "orta": size = DecorSize.Medium; return true;
                case "l": case "b": case "large": case "büyük": size = DecorSize.Large; return true;
                default: return false;
            }
        }

        /// <summary>alt/orta/üst ya da low/mid/high ya da 0/1/2.</summary>
        public static bool TryParseBand(string text, out int band)
        {
            band = Decor.BandMid;
            switch ((text ?? "").Trim().ToLowerInvariant())
            {
                case "alt": case "low": case "0": band = Decor.BandLow; return true;
                case "orta": case "mid": case "1": band = Decor.BandMid; return true;
                case "üst": case "ust": case "high": case "2": band = Decor.BandHigh; return true;
                default: return false;
            }
        }
    }
}
