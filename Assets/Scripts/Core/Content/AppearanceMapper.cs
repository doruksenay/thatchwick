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
            Options(catalog.Bangs, file.bangs, "bangs", errors);
            Options(catalog.Mustaches, file.mustaches, "mustaches", errors);
            Options(catalog.Beards, file.beards, "beards", errors);
            Options(catalog.FacialHair, file.facialHair, "facialHair", errors);
            Options(catalog.FaceShapes, file.faceShapes, "faceShapes", errors);
            Options(catalog.EyeShapes, file.eyeShapes, "eyeShapes", errors);
            Options(catalog.Brows, file.brows, "brows", errors);
            Options(catalog.Noses, file.noses, "noses", errors);
            Options(catalog.Mouths, file.mouths, "mouths", errors);
            Options(catalog.Ears, file.ears, "ears", errors);
            Options(catalog.FaceMarks, file.faceMarks, "faceMarks", errors);
            Options(catalog.SkinDetails, file.skinDetails, "skinDetails", errors);
            Options(catalog.ScarPatterns, file.scarPatterns, "scarPatterns", errors);
            Options(catalog.ScarZones, file.scarZones, "scarZones", errors);
            Options(catalog.TattooPatterns, file.tattooPatterns, "tattooPatterns", errors);
            Options(catalog.TattooZones, file.tattooZones, "tattooZones", errors);
            Options(catalog.Jewelry, file.jewelry, "jewelry", errors);
            Options(catalog.Makeup, file.makeup, "makeup", errors);
            Options(catalog.Outfits, file.outfits, "outfits", errors);
            Options(catalog.Bottoms, file.bottoms, "bottoms", errors);
            Options(catalog.Shoes, file.shoes, "shoes", errors);
            Options(catalog.Outers, file.outers, "outers", errors);
            Options(catalog.Hats, file.hats, "hats", errors);
            Options(catalog.Patterns, file.patterns, "patterns", errors);
            Options(catalog.Laughs, file.laughs, "laughs", errors);
            Colors(catalog.SkinTones, file.skinTones, "skinTones", errors);
            Colors(catalog.HairColors, file.hairColors, "hairColors", errors);
            Colors(catalog.EyeColors, file.eyeColors, "eyeColors", errors);
            Colors(catalog.ClothColors, file.clothColors, "clothColors", errors);
            Colors(catalog.InkColors, file.inkColors, "inkColors", errors);
            Colors(catalog.MakeupColors, file.makeupColors, "makeupColors", errors);
            Colors(catalog.RecessiveHairColors, file.recessiveHairColors, "recessiveHairColors", errors);
            Colors(catalog.RecessiveEyeColors, file.recessiveEyeColors, "recessiveEyeColors", errors);

            // Eski dosya: tek listeli sakal/bıyıktan iki liste türet.
            if (catalog.Mustaches.Count == 0 && catalog.Beards.Count == 0 && catalog.FacialHair.Count > 0)
            {
                foreach (var o in catalog.FacialHair)
                {
                    if (o.Id == Appearance.None) { catalog.Mustaches.Add(o); catalog.Beards.Add(o); }
                    else if (o.Id == "mustache") catalog.Mustaches.Add(o);
                    else catalog.Beards.Add(o);
                }
            }

            Require(catalog.HairStyles.Count, "hairStyles", errors);
            Require(catalog.Outfits.Count, "outfits", errors);
            Require(catalog.SkinTones.Count, "skinTones", errors);
            Require(catalog.HairColors.Count, "hairColors", errors);
            Require(catalog.EyeColors.Count, "eyeColors", errors);
            Require(catalog.ClothColors.Count, "clothColors", errors);
            if (catalog.FacialHair.Count > 0) RequireNone(catalog.FacialHair, "facialHair", errors);
            if (catalog.FaceMarks.Count > 0) RequireNone(catalog.FaceMarks, "faceMarks", errors);
            RequireNone(catalog.Hats, "hats", errors);
            if (catalog.Bangs.Count > 0) RequireNone(catalog.Bangs, "bangs", errors);
            if (catalog.Mustaches.Count > 0) RequireNone(catalog.Mustaches, "mustaches", errors);
            if (catalog.Beards.Count > 0) RequireNone(catalog.Beards, "beards", errors);
            if (catalog.Outers.Count > 0) RequireNone(catalog.Outers, "outers", errors);
            return catalog;
        }

        public static AppearanceFileDto ToDto(AppearanceCatalog c) => new AppearanceFileDto
        {
            hairStyles = Dto(c.HairStyles),
            bangs = Dto(c.Bangs),
            mustaches = Dto(c.Mustaches),
            beards = Dto(c.Beards),
            facialHair = Dto(c.FacialHair),
            faceShapes = Dto(c.FaceShapes),
            eyeShapes = Dto(c.EyeShapes),
            brows = Dto(c.Brows),
            noses = Dto(c.Noses),
            mouths = Dto(c.Mouths),
            ears = Dto(c.Ears),
            faceMarks = Dto(c.FaceMarks),
            skinDetails = Dto(c.SkinDetails),
            scarPatterns = Dto(c.ScarPatterns),
            scarZones = Dto(c.ScarZones),
            tattooPatterns = Dto(c.TattooPatterns),
            tattooZones = Dto(c.TattooZones),
            jewelry = Dto(c.Jewelry),
            makeup = Dto(c.Makeup),
            outfits = Dto(c.Outfits),
            bottoms = Dto(c.Bottoms),
            shoes = Dto(c.Shoes),
            outers = Dto(c.Outers),
            hats = Dto(c.Hats),
            patterns = Dto(c.Patterns),
            laughs = Dto(c.Laughs),
            skinTones = Hex(c.SkinTones),
            hairColors = Hex(c.HairColors),
            eyeColors = Hex(c.EyeColors),
            clothColors = Hex(c.ClothColors),
            inkColors = Hex(c.InkColors),
            makeupColors = Hex(c.MakeupColors),
            recessiveHairColors = Hex(c.RecessiveHairColors),
            recessiveEyeColors = Hex(c.RecessiveEyeColors)
        };

        private static OptionDto[] Dto(List<OptionDef> list)
        {
            var arr = new OptionDto[list.Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new OptionDto { id = list[i].Id, displayName = list[i].DisplayName, sex = list[i].OnlyFor.HasValue ? list[i].OnlyFor.Value.ToString() : "", tag = list[i].Tag ?? "" };
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
                target.Add(new OptionDef { Id = o.id, DisplayName = o.displayName, OnlyFor = only, Tag = string.IsNullOrWhiteSpace(o.tag) ? null : o.tag });
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
