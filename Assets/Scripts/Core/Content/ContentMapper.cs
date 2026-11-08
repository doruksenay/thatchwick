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
                    StyleSet = string.IsNullOrWhiteSpace(dto.styleSet) ? null : dto.styleSet,
                    SpoilDays = dto.spoilDays,
                    Preserves = dto.preserves,
                    ReadyDays = dto.readyDays,
                    Variants = dto.variants != null ? new List<string>(dto.variants) : new List<string>(),
                    ForageMax = dto.forageMax,
                    PlantsAs = string.IsNullOrWhiteSpace(dto.plantsAs) ? null : dto.plantsAs,
                    Category = string.IsNullOrWhiteSpace(dto.category) ? "furniture" : dto.category
                };

                if (!string.IsNullOrWhiteSpace(dto.placement))
                {
                    if (!Enum.TryParse(dto.placement, true, out ItemPlacement placement) || !Enum.IsDefined(typeof(ItemPlacement), placement))
                        errors.Add($"{dto.id}: bilinmeyen yerleşim '{dto.placement}'.");
                    else
                        item.Placement = placement;
                }

                if (dto.growSeasons != null)
                    foreach (var gs in dto.growSeasons)
                    {
                        if (LifeContentMapper.TryParseSeason(gs, out var gsSeason)) item.GrowSeasons.Add(gsSeason);
                        else errors.Add($"{dto.id}: bilinmeyen büyüme mevsimi '{gs}'.");
                    }
                if (dto.forageSeasons != null)
                    foreach (var fs in dto.forageSeasons)
                    {
                        if (LifeContentMapper.TryParseSeason(fs, out var fsSeason)) item.ForageSeasons.Add(fsSeason);
                        else errors.Add($"{dto.id}: bilinmeyen toplama mevsimi '{fs}'.");
                    }

                if (dto.surfaceSlots != null)
                {
                    foreach (var s in dto.surfaceSlots)
                    {
                        if (s == null) continue;
                        item.SurfaceSlots.Add(new SurfaceSlotDef { Id = s.id, U = s.u, V = s.v });
                    }
                }

                if (dto.interactions != null)
                {
                    foreach (var i in dto.interactions)
                    {
                        if (i == null) continue;
                        bool hasEffects = !string.IsNullOrWhiteSpace(i.consume) || (i.consumeMore != null && i.consumeMore.Length > 0)
                            || (i.produce != null && i.produce.Length > 0) || i.removeItem || i.resetsReady;
                        NeedType need = default;
                        bool needless = hasEffects && string.IsNullOrWhiteSpace(i.need);   // saf iş etkileşimi
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
                            Tool = string.IsNullOrWhiteSpace(i.tool) ? null : i.tool,
                            MoodId = string.IsNullOrWhiteSpace(i.moodId) ? null : i.moodId,
                            MoodText = i.moodText,
                            MoodValue = i.moodValue,
                            MoodDays = i.moodDays,
                            RequiresLit = i.requiresLit,
                            RequiresReady = i.requiresReady,
                            ResetsReady = i.resetsReady
                        };
                        if (i.consumeMore != null)
                            foreach (var cm in i.consumeMore)
                                if (cm != null) interaction.ConsumeMore.Add(new ConsumeDef { ItemId = cm.item, Count = cm.count <= 0 ? 1 : cm.count });
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
                                    MaxLevel = p.maxLevel
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

            LifeContentMapper.Apply(file, db, errors);
            errors.AddRange(db.Validate());
            return errors;
        }

        private static bool TryParseNeed(string text, out NeedType need)
        {
            need = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!Enum.TryParse(text, true, out need)) return false;
            return Enum.IsDefined(typeof(NeedType), need);
        }
    }
}
