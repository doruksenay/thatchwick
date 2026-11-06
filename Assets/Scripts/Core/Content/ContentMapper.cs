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
            if (file == null || file.items == null)
            {
                errors.Add("İçerik dosyası boş ya da okunamadı.");
                return errors;
            }

            foreach (var dto in file.items)
            {
                if (dto == null) continue;

                var item = new ItemDef { Id = dto.id, DisplayName = dto.displayName };
                if (dto.interactions != null)
                {
                    foreach (var i in dto.interactions)
                    {
                        if (i == null) continue;
                        if (!TryParseNeed(i.need, out var need))
                        {
                            errors.Add($"{dto.id}/{i.id}: bilinmeyen ihtiyaç '{i.need}'.");
                            continue;
                        }

                        item.Interactions.Add(new InteractionDef
                        {
                            Id = i.id,
                            DisplayName = i.displayName,
                            Need = need,
                            Restore = i.restore,
                            DurationDays = i.durationDays
                        });
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
