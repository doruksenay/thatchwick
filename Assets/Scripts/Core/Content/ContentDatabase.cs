using System;
using System.Collections.Generic;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Content
{
    /// <summary>Tüm içerik kayıtlarının toplandığı yer ve doğrulama.</summary>
    public sealed class ContentDatabase
    {
        public ContentRegistry<ItemDef> Items { get; } = new ContentRegistry<ItemDef>();
        public ContentRegistry<TraitDef> Traits { get; } = new ContentRegistry<TraitDef>();
        public ContentRegistry<BranchDef> Branches { get; } = new ContentRegistry<BranchDef>();
        public ContentRegistry<ProjectDef> Projects { get; } = new ContentRegistry<ProjectDef>();
        public ContentRegistry<VillagerDef> Villagers { get; } = new ContentRegistry<VillagerDef>();
        public ContentRegistry<FestivalDef> Festivals { get; } = new ContentRegistry<FestivalDef>();
        public ContentRegistry<GoalDef> Goals { get; } = new ContentRegistry<GoalDef>();
        public ContentRegistry<ToolDef> Tools { get; } = new ContentRegistry<ToolDef>();
        public ContentRegistry<RoomTemplateDef> Templates { get; } = new ContentRegistry<RoomTemplateDef>();
        public ContentRegistry<AnimalSpeciesDef> Animals { get; } = new ContentRegistry<AnimalSpeciesDef>();
        /// <summary>Etkileşimlerin başvurduğu moodlet kataloğu (doğrulama için).</summary>
        public Mood.MoodCatalog Moods { get; set; } = Mood.MoodCatalog.Default;

        /// <summary>
        /// İçerik hatalarını okunur metin olarak döner (boşsa içerik geçerli).
        /// Veri yüklenirken ve CI'da çalıştırılır; hatalı içerik oyunu sessizce bozmasın.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            foreach (var item in Items.All)
            {
                if (string.IsNullOrWhiteSpace(item.DisplayName))
                    errors.Add($"{item.Id}: görünen ad boş.");

                if (item.Width < 1 || item.Width > ItemDef.MaxSize || item.Height < 1 || item.Height > ItemDef.MaxSize)
                    errors.Add($"{item.Id}: boyut 1-{ItemDef.MaxSize} arasında olmalı.");
                if (item.Price < 0) errors.Add($"{item.Id}: fiyat negatif olamaz.");
                if (item.HeatOutput < 0f) errors.Add($"{item.Id}: ısı gücü negatif olamaz.");
                if (item.HeatOutput > 0f && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: ısı kaynağı yere konan eşya olmalı.");
                if (item.UnlockLevel < 1 || item.UnlockLevel > 20) errors.Add($"{item.Id}: açılma seviyesi 1-20 arasında olmalı.");
                if (item.Storage < 0) errors.Add($"{item.Id}: depo kapasitesi negatif olamaz.");
                if (item.Storage > 0 && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: depo eşyası yere konan eşya olmalı.");
                if (item.GrowDays < 0f) errors.Add($"{item.Id}: büyüme süresi negatif olamaz.");
                if ((item.NeedsTilledSoil || item.DiesInWinter) && !item.IsPlant) errors.Add($"{item.Id}: tilled/diesInWinter yalnızca bitkide kullanılır.");
                if (item.IsPlant && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: bitki yere konan eşya olmalı.");
                if (item.PlantsAs != null)
                {
                    if (item.Placement != ItemPlacement.Resource) errors.Add($"{item.Id}: dikilebilen fidan bir kaynak olmalı.");
                    if (!Items.TryGet(item.PlantsAs, out var plant)) errors.Add($"{item.Id}: dikilince oluşacak '{item.PlantsAs}' bulunamadı.");
                    else if (!plant.IsPlant) errors.Add($"{item.Id}: '{item.PlantsAs}' bir bitki değil (growDays yok).");
                }
                if (item.Placement == ItemPlacement.Resource && (item.Width != 1 || item.Height != 1 || item.SurfaceSlots.Count > 0 || item.Interactions.Count > 0))
                    errors.Add($"{item.Id}: kaynak 1x1 olmalı, yuvası ve etkileşimi olamaz.");
                if (item.Placement == ItemPlacement.Wall)
                {
                    if (item.Width != 1 || item.Height != 1) errors.Add($"{item.Id}: duvar eşyası 1x1 olmalı.");
                    if (item.IsHousing) errors.Add($"{item.Id}: duvar eşyası barınak olamaz.");
                }
                if (item.IsHousing)
                {
                    if (item.HousingCapacity < 1) errors.Add($"{item.Id}: barınak kapasitesi en az 1 olmalı.");
                    if (item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: barınak yere konan eşya olmalı.");
                }
                else if (item.HousingCapacity != 0) errors.Add($"{item.Id}: capacity yalnızca barınakta kullanılır.");
                if (item.Placement == ItemPlacement.Surface)
                {
                    if (item.Width != 1 || item.Height != 1) errors.Add($"{item.Id}: dekor 1x1 olmalı.");
                    if (item.SurfaceSlots.Count > 0) errors.Add($"{item.Id}: dekorun kendi yüzey yuvası olamaz.");
                }
                if (item.Comfort < 0 || item.Comfort > 50) errors.Add($"{item.Id}: konfor 0-50 arasında olmalı.");
                if (item.SpoilDays < 0f) errors.Add($"{item.Id}: bozulma süresi negatif olamaz.");
                if (item.SpoilDays > 0f && !item.HasTag("food")) errors.Add($"{item.Id}: bozulma yalnızca yiyecek etiketli eşyada olur.");
                if (item.Preserves < 0f || item.Preserves > 1f) errors.Add($"{item.Id}: kiler çarpanı 0-1 arasında olmalı.");
                if (item.ForageMax < 0) errors.Add($"{item.Id}: toplama sınırı negatif olamaz.");
                if (item.IsForage && (item.Placement != ItemPlacement.Floor || !item.IsPlant && item.Interactions.Count == 0)) errors.Add($"{item.Id}: toplanabilir eşya yere konan ve etkileşimli olmalı.");
                if (item.GrowSeasons.Count > 0 && !item.IsPlant) errors.Add($"{item.Id}: growSeasons yalnızca bitkide kullanılır.");
                var slotIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var slot in item.SurfaceSlots)
                {
                    if (string.IsNullOrWhiteSpace(slot.Id)) { errors.Add($"{item.Id}: kimliksiz yuva."); continue; }
                    if (!slotIds.Add(slot.Id)) errors.Add($"{item.Id}/{slot.Id}: yuva kimliği tekrar ediyor.");
                    if (slot.U < 0f || slot.U > 1f || slot.V < 0f || slot.V > 1f) errors.Add($"{item.Id}/{slot.Id}: U ve V 0-1 arasında olmalı.");
                }

                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var it in item.Interactions)
                {
                    string where = $"{item.Id}/{it.Id}";
                    if (string.IsNullOrWhiteSpace(it.Id))
                    {
                        errors.Add($"{item.Id}: kimliksiz etkileşim.");
                        continue;
                    }
                    if (!seen.Add(it.Id)) errors.Add($"{where}: etkileşim kimliği tekrar ediyor.");
                    bool pureWork = it.HasEffects || it.Skill.HasValue || it.Moodlet != null;
                    if (pureWork ? (it.Restore < 0f || it.Restore > 100f) : (it.Restore <= 0f || it.Restore > 100f))
                        errors.Add($"{where}: Restore 0-100 arasında olmalı (iş etkileşimi değilse sıfırdan büyük).");
                    if (it.RequiresGrown && !item.IsPlant) errors.Add($"{where}: requiresGrown yalnızca bitkide kullanılır.");
                    if (it.ConsumeItem != null && !Items.Contains(it.ConsumeItem)) errors.Add($"{where}: harcanan '{it.ConsumeItem}' bulunamadı.");
                    foreach (var c in it.Consume)
                    {
                        if (!Items.TryGet(c.ItemId, out var mat)) errors.Add($"{where}: malzeme '{c.ItemId}' bulunamadı.");
                        else if (mat.Placement != ItemPlacement.Resource) errors.Add($"{where}: malzeme '{c.ItemId}' bir kaynak olmalı.");
                        if (c.Count < 1) errors.Add($"{where}: malzeme adedi en az 1 olmalı.");
                    }
                    if (it.Moodlet != null && (Moods == null || !Moods.Contains(it.Moodlet))) errors.Add($"{where}: moodlet '{it.Moodlet}' bulunamadı.");
                    if (it.MinSkillLevel < 0 || it.MinSkillLevel > Characters.SkillRules.MaxLevel) errors.Add($"{where}: minLevel 0-{Characters.SkillRules.MaxLevel} arasında olmalı.");
                    if (it.MinSkillLevel > 0 && !it.Skill.HasValue) errors.Add($"{where}: minLevel için beceri gerekli.");
                    if ((it.AddFeed > 0 || it.Collect || it.RequiresAnimal) && !item.IsHousing) errors.Add($"{where}: yem/toplama/hayvan yalnızca barınakta kullanılır.");
                    if (it.AddFeed > 0 && it.AllConsumed().Count == 0) errors.Add($"{where}: yem koymak için malzeme (consume) gerekli.");
                    if (it.AddFeed < 0) errors.Add($"{where}: addFeed negatif olamaz.");
                    foreach (var er in it.ExtraRestore)
                        if (er.Amount <= 0f || er.Amount > 100f) errors.Add($"{where}: ek doygunluk 0-100 arasında olmalı.");
                    if (it.RequiresLit && item.HeatOutput <= 0f) errors.Add($"{where}: requiresLit yalnızca ısı kaynağında kullanılır.");
                    if (!string.IsNullOrEmpty(it.MoodId) && (it.MoodValue == 0 || it.MoodDays <= 0.0)) errors.Add($"{where}: ruh hali etkisi için moodValue ve moodDays gerekli.");
                    if (!string.IsNullOrEmpty(it.Tool) && !Tools.Contains(it.Tool)) errors.Add($"{where}: bilinmeyen alet '{it.Tool}'.");
                    foreach (var p in it.Produce)
                    {
                        if (!Items.Contains(p.ItemId)) errors.Add($"{where}: üretilen '{p.ItemId}' bulunamadı.");
                        if (p.Count < 1) errors.Add($"{where}: üretim adedi en az 1 olmalı.");
                        if (p.Chance <= 0f || p.Chance > 1f) errors.Add($"{where}: üretim olasılığı 0-1 arasında olmalı.");
                        if (p.MinLevel < 0 || p.MaxLevel < 0 || (p.MaxLevel > 0 && p.MinLevel > p.MaxLevel)) errors.Add($"{where}: üretim seviye aralığı geçersiz.");
                        if ((p.MinLevel > 0 || p.MaxLevel > 0) && !it.Skill.HasValue) errors.Add($"{where}: seviye sınırlı üretim için etkileşimde beceri gerekli.");
                    }
                    if (it.DurationDays <= 0.0) errors.Add($"{where}: süre sıfırdan büyük olmalı.");
                    if (it.Skill.HasValue && it.SkillXp <= 0f) errors.Add($"{where}: beceri verilmiş ama skillXp sıfırdan büyük değil.");
                    if (!it.Skill.HasValue && it.SkillXp != 0f) errors.Add($"{where}: skillXp var ama beceri yok.");
                }
            }

            foreach (var a in Animals.All)
            {
                if (string.IsNullOrWhiteSpace(a.DisplayName)) errors.Add($"{a.Id}: görünen ad boş.");
                if (a.Price < 0) errors.Add($"{a.Id}: fiyat negatif olamaz.");
                if (a.FeedPerDay < 0f) errors.Add($"{a.Id}: yem negatif olamaz.");
                if (a.Produces != null && !Items.Contains(a.Produces)) errors.Add($"{a.Id}: ürün '{a.Produces}' bulunamadı.");
                if (a.Meat != null && !Items.Contains(a.Meat)) errors.Add($"{a.Id}: et '{a.Meat}' bulunamadı.");
                if (a.MatureDays < 0f || a.MatureDays >= a.LifespanDays) errors.Add($"{a.Id}: erişkinlik yaşı 0 ile ömür arasında olmalı.");
                bool housed = false;
                foreach (var item in Items.All) if (item.HousingKind == a.Housing) { housed = true; break; }
                if (!housed) errors.Add($"{a.Id}: '{a.Housing}' türünde barınak eşyası yok.");
            }

            ValidateLife(errors);
            return errors;
        }

        private void ValidateLife(List<string> errors)
        {
            foreach (var t in Traits.All)
            {
                if (string.IsNullOrWhiteSpace(t.DisplayName)) errors.Add($"{t.Id}: görünen ad boş.");
                foreach (var c in t.Conflicts) if (!Traits.Contains(c)) errors.Add($"{t.Id}: çakışan özellik '{c}' bulunamadı.");
            }
            foreach (var b in Branches.All)
            {
                if (b.Level != 5 && b.Level != 10) errors.Add($"{b.Id}: dal seviyesi 5 ya da 10 olmalı.");
                if (b.Requires != null)
                {
                    if (!Branches.TryGet(b.Requires, out var first)) errors.Add($"{b.Id}: gereken dal '{b.Requires}' bulunamadı.");
                    else if (first.Skill != b.Skill || first.Level >= b.Level) errors.Add($"{b.Id}: gereken dal aynı beceride ve daha düşük seviyede olmalı.");
                }
                else if (b.Level == 10) errors.Add($"{b.Id}: 10. seviye dal bir 5. seviye dala bağlı olmalı (requires).");
            }
            foreach (var p in Projects.All)
            {
                if (p.Akce < 0) errors.Add($"{p.Id}: akçe maliyeti negatif olamaz.");
                if (p.Akce == 0 && p.Costs.Count == 0) errors.Add($"{p.Id}: proje maliyetsiz olamaz.");
                foreach (var c in p.Costs) if (!Items.Contains(c.ItemId)) errors.Add($"{p.Id}: maliyet kalemi '{c.ItemId}' bulunamadı.");
            }
            foreach (var v in Villagers.All)
            {
                if (string.IsNullOrWhiteSpace(v.Name)) errors.Add($"{v.Id}: köylü adı boş.");
                if (v.Age < 24 || v.Age > 100) errors.Add($"{v.Id}: yaş 24-100 oyun günü arasında olmalı.");
                foreach (var tr in v.Traits) if (!Traits.Contains(tr)) errors.Add($"{v.Id}: özellik '{tr}' bulunamadı.");
                foreach (var list in new[] { v.Loves, v.Likes, v.Dislikes })
                    foreach (var g in list)
                        if (!g.StartsWith("tag:") && !Items.Contains(g)) errors.Add($"{v.Id}: hediye zevki '{g}' bulunamadı.");
                foreach (var e in v.Events)
                {
                    if (e.MinAffection < 1 || e.MinAffection > 100) errors.Add($"{v.Id}/{e.Id}: yakınlık eşiği 1-100 olmalı.");
                    if (e.RewardItem != null && !Items.Contains(e.RewardItem)) errors.Add($"{v.Id}/{e.Id}: ödül eşyası '{e.RewardItem}' bulunamadı.");
                }
            }
            foreach (var i in Items.All)
                foreach (var vid in i.Variants)
                {
                    if (!Items.TryGet(vid, out var v)) { errors.Add($"{i.Id}: varyant '{vid}' bulunamadı."); continue; }
                    if (v.Width != i.Width || v.Height != i.Height || v.Placement != i.Placement) errors.Add($"{i.Id}: varyant '{vid}' aynı boyut ve yerleşimde olmalı.");
                    if (!v.Variants.Contains(i.Id)) errors.Add($"{i.Id}: varyant '{vid}' karşılıklı değil.");
                }
            foreach (var f in Festivals.All)
                if (f.Day < 0 || f.Day > 11) errors.Add($"{f.Id}: şenlik günü 0-11 arasında olmalı.");
            foreach (var g in Goals.All)
            {
                if (g.Target <= 0) errors.Add($"{g.Id}: hedef değeri sıfırdan büyük olmalı.");
                if (string.IsNullOrWhiteSpace(g.Metric)) errors.Add($"{g.Id}: ölçüt boş.");
                else if (Array.IndexOf(GoalDef.Metrics, g.Metric) < 0) errors.Add($"{g.Id}: bilinmeyen ölçüt '{g.Metric}'.");
            }
            foreach (var t in Tools.All)
            {
                if (t.Tiers.Count == 0) errors.Add($"{t.Id}: alet kademesi yok.");
                foreach (var tier in t.Tiers) if (tier.Akce < 0 || tier.Firewood < 0) errors.Add($"{t.Id}: kademe maliyeti negatif olamaz.");
            }
            foreach (var t in Templates.All)
            {
                if (t.Width < 2 || t.Height < 2 || t.Width > 12 || t.Height > 12) errors.Add($"{t.Id}: şablon boyutu 2-12 arasında olmalı.");
                int sideLen = (t.DoorSide == TemplateSide.North || t.DoorSide == TemplateSide.South) ? t.Width : t.Height;
                if (t.DoorOffset < 0 || t.DoorOffset >= sideLen) errors.Add($"{t.Id}: kapı konumu duvar dışında.");
                foreach (var w in t.Windows) if (w < 0 || w >= sideLen) errors.Add($"{t.Id}: pencere konumu duvar dışında.");
                if (!string.IsNullOrWhiteSpace(t.Floor) && !(Enum.TryParse<World.FloorType>(t.Floor, true, out var fl) && Enum.IsDefined(typeof(World.FloorType), fl)))
                    errors.Add($"{t.Id}: bilinmeyen zemin '{t.Floor}'.");
                foreach (var i in t.Items)
                {
                    if (!Items.Contains(i.ItemId)) errors.Add($"{t.Id}: şablon eşyası '{i.ItemId}' bulunamadı.");
                    if (i.X < 0 || i.Y < 0 || i.X >= t.Width || i.Y >= t.Height) errors.Add($"{t.Id}: '{i.ItemId}' şablon dışında.");
                }
            }
        }
    }
}
