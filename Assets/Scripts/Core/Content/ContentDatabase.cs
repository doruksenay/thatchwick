using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Tüm içerik kayıtlarının toplandığı yer ve doğrulama.</summary>
    public sealed class ContentDatabase
    {
        public ContentRegistry<ItemDef> Items { get; } = new ContentRegistry<ItemDef>();

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
                if (item.GrowDays < 0f) errors.Add($"{item.Id}: büyüme süresi negatif olamaz.");
                if (item.IsPlant && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: bitki yere konan eşya olmalı.");
                if (item.PlantsAs != null)
                {
                    if (item.Placement != ItemPlacement.Resource) errors.Add($"{item.Id}: dikilebilen fidan bir kaynak olmalı.");
                    if (!Items.TryGet(item.PlantsAs, out var plant)) errors.Add($"{item.Id}: dikilince oluşacak '{item.PlantsAs}' bulunamadı.");
                    else if (!plant.IsPlant) errors.Add($"{item.Id}: '{item.PlantsAs}' bir bitki değil (growDays yok).");
                }
                if (item.Placement == ItemPlacement.Resource && (item.Width != 1 || item.Height != 1 || item.SurfaceSlots.Count > 0 || item.Interactions.Count > 0))
                    errors.Add($"{item.Id}: kaynak 1x1 olmalı, yuvası ve etkileşimi olamaz.");
                if (item.Placement == ItemPlacement.Surface)
                {
                    if (item.Width != 1 || item.Height != 1) errors.Add($"{item.Id}: dekor 1x1 olmalı.");
                    if (item.SurfaceSlots.Count > 0) errors.Add($"{item.Id}: dekorun kendi yüzey yuvası olamaz.");
                }
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
                    if (it.HasEffects ? (it.Restore < 0f || it.Restore > 100f) : (it.Restore <= 0f || it.Restore > 100f))
                        errors.Add($"{where}: Restore 0-100 arasında olmalı (iş etkileşimi değilse sıfırdan büyük).");
                    if (it.RequiresGrown && !item.IsPlant) errors.Add($"{where}: requiresGrown yalnızca bitkide kullanılır.");
                    if (it.ConsumeItem != null && !Items.Contains(it.ConsumeItem)) errors.Add($"{where}: harcanan '{it.ConsumeItem}' bulunamadı.");
                    foreach (var p in it.Produce)
                    {
                        if (!Items.Contains(p.ItemId)) errors.Add($"{where}: üretilen '{p.ItemId}' bulunamadı.");
                        if (p.Count < 1) errors.Add($"{where}: üretim adedi en az 1 olmalı.");
                        if (p.Chance <= 0f || p.Chance > 1f) errors.Add($"{where}: üretim olasılığı 0-1 arasında olmalı.");
                    }
                    if (it.DurationDays <= 0.0) errors.Add($"{where}: süre sıfırdan büyük olmalı.");
                    if (it.Skill.HasValue && it.SkillXp <= 0f) errors.Add($"{where}: beceri verilmiş ama skillXp sıfırdan büyük değil.");
                    if (!it.Skill.HasValue && it.SkillXp != 0f) errors.Add($"{where}: skillXp var ama beceri yok.");
                }
            }

            return errors;
        }
    }
}
