using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// İçerik DTO'larındaki oyuncuya görünen metinleri, ContentMapper'dan önce seçili dile çevirir.
    ///
    /// Geçiş dönemi: içerik JSON'larının metinleri şu an Türkçe (<see cref="JsonLanguage"/>). Seçili dil Türkçeyse
    /// JSON olduğu gibi kalır; başka dilde önce o dilin tablosuna, sonra İngilizce tabloya bakılır, ikisinde de
    /// yoksa JSON metni kalır. Bütün özellik dalları birleşince JSON metinleri İngilizceye çevrilecek ve Türkçe
    /// tabloya taşınacak (tools/loc/flip_source.py); o zaman <see cref="JsonLanguage"/> "en" olur.
    /// </summary>
    public static class ContentLocalizer
    {
        /// <summary>İçerik JSON'larındaki displayName/description metinlerinin dili.</summary>
        public const string JsonLanguage = Languages.Turkish;

        public static void Apply(ContentFileDto file, Localizer loc)
        {
            if (file == null || !ShouldApply(loc)) return;
            Walk(file, (key, text) => Pick(loc, key, text));
        }

        public static void Apply(AppearanceFileDto file, Localizer loc)
        {
            if (file == null || !ShouldApply(loc)) return;
            Walk(file, (key, text) => Pick(loc, key, text));
        }

        /// <summary>İçerikteki çevrilecek her metin: (anahtar, JSON'daki metin). Boş metinler atlanır.</summary>
        public static List<KeyValuePair<string, string>> Entries(ContentFileDto file, AppearanceFileDto appearance = null)
        {
            var list = new List<KeyValuePair<string, string>>();
            Func<string, string, string> collect = (key, text) =>
            {
                if (!string.IsNullOrWhiteSpace(text)) list.Add(new KeyValuePair<string, string>(key, text));
                return text;
            };
            if (file != null) Walk(file, collect);
            if (appearance != null) Walk(appearance, collect);
            return list;
        }

        /// <summary>Her metin alanını <paramref name="f"/>(anahtar, metin) ile değiştirir. Anahtar şeması: <see cref="ContentKeys"/>.</summary>
        private static void Walk(ContentFileDto file, Func<string, string, string> f)
        {
            foreach (var it in file.items ?? new ItemDto[0])
            {
                if (it == null) continue;
                it.displayName = f(ContentKeys.Name(it.id), it.displayName);
                foreach (var act in it.interactions ?? new InteractionDto[0])
                {
                    if (act == null) continue;
                    act.displayName = f(ContentKeys.Interaction(it.id, act.id), act.displayName);
                    act.moodText = f(ContentKeys.InteractionMood(it.id, act.id), act.moodText);
                }
            }
            foreach (var r in file.recipes ?? new RecipeDto[0])
                if (r != null) r.displayName = f(ContentKeys.Recipe(r.id), r.displayName);
            foreach (var a in file.animals ?? new AnimalDto[0])
                if (a != null) a.displayName = f(ContentKeys.Name(a.id), a.displayName);
            foreach (var p in file.buildParts ?? new BuildPartDto[0])
                if (p != null) p.displayName = f(ContentKeys.BuildPart(p.id), p.displayName);
            foreach (var e in file.effects ?? new EffectDto[0])
            {
                if (e == null) continue;
                e.displayName = f(ContentKeys.EffectName(e.id), e.displayName);
                e.description = f(ContentKeys.EffectDesc(e.id), e.description);
            }

            foreach (var t in file.traits ?? new TraitDto[0])
            {
                if (t == null) continue;
                t.displayName = f(ContentKeys.Name(t.id), t.displayName);
                t.description = f(ContentKeys.Desc(t.id), t.description);
            }
            foreach (var b in file.branches ?? new BranchDto[0])
            {
                if (b == null) continue;
                b.displayName = f(ContentKeys.Name(b.id), b.displayName);
                b.description = f(ContentKeys.Desc(b.id), b.description);
            }
            foreach (var p in file.projects ?? new ProjectDto[0])
            {
                if (p == null) continue;
                p.displayName = f(ContentKeys.Name(p.id), p.displayName);
                p.description = f(ContentKeys.Desc(p.id), p.description);
            }
            foreach (var fe in file.festivals ?? new FestivalDto[0])
            {
                if (fe == null) continue;
                fe.displayName = f(ContentKeys.Name(fe.id), fe.displayName);
                fe.description = f(ContentKeys.Desc(fe.id), fe.description);
            }
            foreach (var g in file.goals ?? new GoalDto[0])
            {
                if (g == null) continue;
                g.displayName = f(ContentKeys.Name(g.id), g.displayName);
                g.description = f(ContentKeys.Desc(g.id), g.description);
            }
            foreach (var t in file.tools ?? new ToolDto[0])
            {
                if (t == null) continue;
                t.displayName = f(ContentKeys.Tool(t.id), t.displayName);
                var tiers = t.tiers ?? new ToolTierDto[0];
                for (int i = 0; i < tiers.Length; i++)
                    if (tiers[i] != null) tiers[i].name = f(ContentKeys.ToolTier(t.id, i), tiers[i].name);
            }
            foreach (var t in file.templates ?? new RoomTemplateDto[0])
                if (t != null) t.displayName = f(ContentKeys.Name(t.id), t.displayName);
            foreach (var v in file.villagers ?? new VillagerDto[0])
            {
                if (v == null) continue;
                // Köylü adı (v.name) çevrilmez: dönem ve kültür kararına bağlı (Project dokümanı 20).
                v.roleName = f(ContentKeys.VillagerRole(v.id), v.roleName);
                foreach (var ev in v.events ?? new VillagerEventDto[0])
                    if (ev != null) ev.text = f(ContentKeys.VillagerEvent(v.id, ev.id), ev.text);
            }
        }

        private static void Walk(AppearanceFileDto file, Func<string, string, string> f)
        {
            // Renk listeleri (skinTones, hairColors ...) yalnızca hex kodu; çevrilecek metin yok.
            WalkGroup("hairStyles", file.hairStyles, f);
            WalkGroup("bangs", file.bangs, f);
            WalkGroup("mustaches", file.mustaches, f);
            WalkGroup("beards", file.beards, f);
            WalkGroup("facialHair", file.facialHair, f);
            WalkGroup("faceShapes", file.faceShapes, f);
            WalkGroup("eyeShapes", file.eyeShapes, f);
            WalkGroup("brows", file.brows, f);
            WalkGroup("noses", file.noses, f);
            WalkGroup("mouths", file.mouths, f);
            WalkGroup("ears", file.ears, f);
            WalkGroup("faceMarks", file.faceMarks, f);
            WalkGroup("skinDetails", file.skinDetails, f);
            WalkGroup("scarPatterns", file.scarPatterns, f);
            WalkGroup("scarZones", file.scarZones, f);
            WalkGroup("tattooPatterns", file.tattooPatterns, f);
            WalkGroup("tattooZones", file.tattooZones, f);
            WalkGroup("jewelry", file.jewelry, f);
            WalkGroup("makeup", file.makeup, f);
            WalkGroup("outfits", file.outfits, f);
            WalkGroup("bottoms", file.bottoms, f);
            WalkGroup("shoes", file.shoes, f);
            WalkGroup("outers", file.outers, f);
            WalkGroup("hats", file.hats, f);
            WalkGroup("patterns", file.patterns, f);
            WalkGroup("laughs", file.laughs, f);
        }

        private static void WalkGroup(string group, OptionDto[] options, Func<string, string, string> f)
        {
            foreach (var o in options ?? new OptionDto[0])
                if (o != null) o.displayName = f(ContentKeys.Look(group, o.id), o.displayName);
        }

        /// <summary>
        /// ContentMapper'ın koddan ürettiği etkileşim adları (kavanozla yakala, doldur, mevsimi geçen süsü kaldır, yükselt)
        /// için çevirici: (anahtar, Türkçe metin) → seçili dildeki metin. Anahtarlar ui tablolarında (content.act.*).
        /// </summary>
        public static Func<string, string, string> Generated(Localizer loc) => (key, tr) => ShouldApply(loc) ? Pick(loc, key, tr) : tr;

        private static bool ShouldApply(Localizer loc) =>
            loc != null && loc.Language != Languages.Normalize(JsonLanguage);

        /// <summary>Boş JSON metni boş kalır (ör. adı koddan üretilen tarifler).</summary>
        private static string Pick(Localizer loc, string key, string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText)) return jsonText;
            return loc.TryGet(key, out var text) && !string.IsNullOrEmpty(text) ? text : jsonText;
        }
    }
}
