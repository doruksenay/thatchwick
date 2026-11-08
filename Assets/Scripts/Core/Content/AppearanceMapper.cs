using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;

namespace Thatchwick.Core.Content
{
    /// <summary>appearance.json (DTO) -> AppearanceCatalog. Hatalı kayıtlar atlanır, hatalar toplanır.</summary>
    public static class AppearanceMapper
    {
        public static AppearanceCatalog Build(AppearanceFileDto file, List<string> errors)
        {
            var catalog = new AppearanceCatalog();
            if (file == null)
            {
                errors.Add("Görünüm dosyası boş ya da okunamadı.");
                return catalog;
            }

            Options(catalog.HairStyles, file.hairStyles, "hairStyles", errors);
            Options(catalog.FacialHair, file.facialHair, "facialHair", errors);
            Options(catalog.FaceMarks, file.faceMarks, "faceMarks", errors);
            Options(catalog.Outfits, file.outfits, "outfits", errors);
            Options(catalog.Hats, file.hats, "hats", errors);
            Colors(catalog.SkinTones, file.skinTones, "skinTones", errors);
            Colors(catalog.HairColors, file.hairColors, "hairColors", errors);
            Colors(catalog.EyeColors, file.eyeColors, "eyeColors", errors);
            Colors(catalog.ClothColors, file.clothColors, "clothColors", errors);

            Require(catalog.HairStyles.Count, "hairStyles", errors);
            Require(catalog.Outfits.Count, "outfits", errors);
            Require(catalog.SkinTones.Count, "skinTones", errors);
            Require(catalog.HairColors.Count, "hairColors", errors);
            Require(catalog.EyeColors.Count, "eyeColors", errors);
            Require(catalog.ClothColors.Count, "clothColors", errors);
            RequireNone(catalog.FacialHair, "facialHair", errors);
            RequireNone(catalog.FaceMarks, "faceMarks", errors);
            RequireNone(catalog.Hats, "hats", errors);
            return catalog;
        }

        public static AppearanceFileDto ToDto(AppearanceCatalog c) => new AppearanceFileDto
        {
            hairStyles = Dto(c.HairStyles),
            facialHair = Dto(c.FacialHair),
            faceMarks = Dto(c.FaceMarks),
            outfits = Dto(c.Outfits),
            hats = Dto(c.Hats),
            skinTones = Hex(c.SkinTones),
            hairColors = Hex(c.HairColors),
            eyeColors = Hex(c.EyeColors),
            clothColors = Hex(c.ClothColors)
        };

        private static OptionDto[] Dto(List<OptionDef> list)
        {
            var arr = new OptionDto[list.Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new OptionDto { id = list[i].Id, displayName = list[i].DisplayName, sex = list[i].OnlyFor.HasValue ? list[i].OnlyFor.Value.ToString() : "" };
            return arr;
        }

        private static string[] Hex(List<Rgb> list)
        {
            var arr = new string[list.Count];
            for (int i = 0; i < arr.Length; i++) arr[i] = list[i].ToHex();
            return arr;
        }

        private static void Options(List<OptionDef> target, OptionDto[] source, string where, List<string> errors)
        {
            if (source == null) return;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var o in source)
            {
                if (o == null) continue;
                if (string.IsNullOrWhiteSpace(o.id)) { errors.Add($"{where}: kimliksiz seçenek."); continue; }
                if (!seen.Add(o.id)) { errors.Add($"{where}/{o.id}: kimlik tekrar ediyor."); continue; }
                if (string.IsNullOrWhiteSpace(o.displayName)) { errors.Add($"{where}/{o.id}: görünen ad boş."); continue; }

                Sex? only = null;
                if (!string.IsNullOrWhiteSpace(o.sex))
                {
                    if (!Enum.TryParse(o.sex, true, out Sex parsed) || !Enum.IsDefined(typeof(Sex), parsed))
                    {
                        errors.Add($"{where}/{o.id}: bilinmeyen cinsiyet '{o.sex}'.");
                        continue;
                    }
                    only = parsed;
                }
                target.Add(new OptionDef { Id = o.id, DisplayName = o.displayName, OnlyFor = only });
            }
        }

        private static void Colors(List<Rgb> target, string[] source, string where, List<string> errors)
        {
            if (source == null) return;
            foreach (var h in source)
            {
                if (Rgb.TryParseHex(h, out var c)) { if (!target.Contains(c)) target.Add(c); }
                else errors.Add($"{where}: geçersiz renk '{h}'.");
            }
        }

        private static void Require(int count, string where, List<string> errors)
        {
            if (count == 0) errors.Add($"{where}: en az bir seçenek gerekli.");
        }

        private static void RequireNone(List<OptionDef> list, string where, List<string> errors)
        {
            if (!list.Exists(o => o.Id == Appearance.None)) errors.Add($"{where}: 'none' (yok) seçeneği gerekli.");
        }
    }
}
