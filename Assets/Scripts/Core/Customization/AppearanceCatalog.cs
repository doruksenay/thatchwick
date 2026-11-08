using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Customization
{
    /// <summary>Bir seçenek: kimlik, görünen ad ve hangi cinsiyete özel olduğu (null = ikisi de).</summary>
    public sealed class OptionDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Sex? OnlyFor { get; set; }

        public bool AllowedFor(Sex sex) => !OnlyFor.HasValue || OnlyFor.Value == sex;
    }

    /// <summary>
    /// Karakter yaratmada seçilebilecek her şey (veri odaklı: appearance.json'dan yüklenir,
    /// yoksa Default kullanılır). Doğrulama ve rastgele üretim buradadır.
    /// </summary>
    public sealed class AppearanceCatalog
    {
        public List<OptionDef> HairStyles { get; } = new List<OptionDef>();
        public List<OptionDef> FacialHair { get; } = new List<OptionDef>();
        public List<OptionDef> FaceMarks { get; } = new List<OptionDef>();
        public List<OptionDef> Outfits { get; } = new List<OptionDef>();
        public List<OptionDef> Hats { get; } = new List<OptionDef>();

        /// <summary>Ten rengi yalnızca bu paletten seçilir (özgür renk yok).</summary>
        public List<Rgb> SkinTones { get; } = new List<Rgb>();
        /// <summary>Saç, göz ve kıyafet için hızlı seçim paletleri; bu renklerde özgür renk de serbesttir.</summary>
        public List<Rgb> HairColors { get; } = new List<Rgb>();
        public List<Rgb> EyeColors { get; } = new List<Rgb>();
        public List<Rgb> ClothColors { get; } = new List<Rgb>();

        public const int MinHeight = 90;
        public const int MaxHeight = 110;

        public IEnumerable<OptionDef> For(IEnumerable<OptionDef> list, Sex sex) => list.Where(o => o.AllowedFor(sex));

        private static OptionDef Find(List<OptionDef> list, string id) => list.FirstOrDefault(o => o.Id == id);

        /// <summary>Hataları okunur metin olarak döner; boşsa görünüm bu cinsiyet için geçerli.</summary>
        public List<string> Validate(Appearance a, Sex sex)
        {
            var errors = new List<string>();
            if (a == null) { errors.Add("Görünüm boş."); return errors; }

            if (!Enum.IsDefined(typeof(BodyBuild), a.Build)) errors.Add("Geçersiz vücut tipi.");
            if (a.HeightPercent < MinHeight || a.HeightPercent > MaxHeight) errors.Add($"Boy {MinHeight}-{MaxHeight} arasında olmalı.");
            if (!SkinTones.Contains(a.SkinColor)) errors.Add("Ten rengi paletten seçilmeli.");

            CheckOption(errors, HairStyles, a.HairStyleId, sex, "saç stili");
            CheckOption(errors, FacialHair, a.FacialHairId, sex, "sakal/bıyık");
            CheckOption(errors, FaceMarks, a.FaceMarkId, sex, "yüz işareti");
            CheckOption(errors, Outfits, a.OutfitId, sex, "kıyafet");
            CheckOption(errors, Hats, a.HatId, sex, "başlık");
            return errors;
        }

        private static void CheckOption(List<string> errors, List<OptionDef> list, string id, Sex sex, string what)
        {
            var def = Find(list, id);
            if (def == null) errors.Add($"Bilinmeyen {what}: '{id}'.");
            else if (!def.AllowedFor(sex)) errors.Add($"'{id}' ({what}) bu cinsiyet için uygun değil.");
        }

        /// <summary>Cinsiyete uymayan seçenekleri ilk uygun seçeneğe çevirir (cinsiyet değişince).</summary>
        public void Normalize(Appearance a, Sex sex)
        {
            a.HairStyleId = Valid(HairStyles, a.HairStyleId, sex);
            a.FacialHairId = Valid(FacialHair, a.FacialHairId, sex);
            a.FaceMarkId = Valid(FaceMarks, a.FaceMarkId, sex);
            a.OutfitId = Valid(Outfits, a.OutfitId, sex);
            a.HatId = Valid(Hats, a.HatId, sex);
            if (a.HeightPercent < MinHeight) a.HeightPercent = MinHeight;
            if (a.HeightPercent > MaxHeight) a.HeightPercent = MaxHeight;
            if (SkinTones.Count > 0 && !SkinTones.Contains(a.SkinColor)) a.SkinColor = SkinTones[0];
        }

        private string Valid(List<OptionDef> list, string id, Sex sex)
        {
            var def = Find(list, id);
            if (def != null && def.AllowedFor(sex)) return id;
            var first = list.FirstOrDefault(o => o.AllowedFor(sex));
            return first != null ? first.Id : id;
        }

        private static T Pick<T>(IList<T> list, Random rng) => list[rng.Next(list.Count)];

        private string PickId(List<OptionDef> list, Sex sex, Random rng)
        {
            var allowed = list.Where(o => o.AllowedFor(sex)).ToList();
            return allowed.Count == 0 ? Appearance.None : Pick(allowed, rng).Id;
        }

        /// <summary>Rastgele (ama her zaman geçerli) bir görünüm.</summary>
        public Appearance Randomize(Random rng, Sex sex)
        {
            var a = new Appearance
            {
                Build = (BodyBuild)rng.Next(3),
                HeightPercent = MinHeight + rng.Next(MaxHeight - MinHeight + 1),
                SkinColor = SkinTones.Count > 0 ? Pick(SkinTones, rng) : new Rgb(232, 195, 158),
                EyeColor = EyeColors.Count > 0 ? Pick(EyeColors, rng) : new Rgb(90, 60, 40),
                HairColor = HairColors.Count > 0 ? Pick(HairColors, rng) : new Rgb(90, 60, 35),
                OutfitColor = ClothColors.Count > 0 ? Pick(ClothColors, rng) : new Rgb(160, 110, 70),
                TrimColor = ClothColors.Count > 0 ? Pick(ClothColors, rng) : new Rgb(70, 50, 35),
                HatColor = ClothColors.Count > 0 ? Pick(ClothColors, rng) : new Rgb(120, 90, 60),
                HairStyleId = PickId(HairStyles, sex, rng),
                OutfitId = PickId(Outfits, sex, rng),
                FaceMarkId = rng.Next(3) == 0 ? PickId(FaceMarks, sex, rng) : Appearance.None,
                HatId = rng.Next(4) == 0 ? PickId(Hats, sex, rng) : Appearance.None
            };
            a.FacialHairColor = a.HairColor;
            a.FacialHairId = sex == Sex.Male && rng.Next(2) == 0 ? PickId(FacialHair, sex, rng) : Appearance.None;
            Normalize(a, sex);
            return a;
        }

        /// <summary>
        /// Çocuğun görünümü: ten rengi anne-baba ortalaması (palette en yakın), saç ve göz rengi
        /// ebeveynlerden birinden (rastgele), vücut tipi ebeveynlerden biri, stiller rastgele.
        /// </summary>
        public Appearance Inherit(Appearance mother, Appearance father, Random rng, Sex childSex)
        {
            var baby = Randomize(rng, childSex);
            baby.FacialHairId = Appearance.None;
            baby.HatId = Appearance.None;
            baby.HeightPercent = 100;

            var parents = new[] { mother, father };
            baby.HairColor = Pick(parents, rng).HairColor;
            baby.FacialHairColor = baby.HairColor;
            baby.EyeColor = Pick(parents, rng).EyeColor;
            baby.Build = Pick(parents, rng).Build;

            var mix = Rgb.Lerp(mother.SkinColor, father.SkinColor, 0.5);
            baby.SkinColor = Nearest(SkinTones, mix, baby.SkinColor);
            Normalize(baby, childSex);
            return baby;
        }

        private static Rgb Nearest(List<Rgb> palette, Rgb target, Rgb fallback)
        {
            if (palette.Count == 0) return fallback;
            Rgb best = palette[0];
            double bestDist = double.MaxValue;
            foreach (var c in palette)
            {
                double dr = c.R - target.R, dg = c.G - target.G, db = c.B - target.B;
                double d = dr * dr + dg * dg + db * db;
                if (d < bestDist) { bestDist = d; best = c; }
            }
            return best;
        }

        /// <summary>Yerleşik varsayılan katalog (appearance.json okunamazsa kullanılır).</summary>
        public static AppearanceCatalog Default()
        {
            var c = new AppearanceCatalog();
            void Add(List<OptionDef> l, string id, string name, Sex? only = null) =>
                l.Add(new OptionDef { Id = id, DisplayName = name, OnlyFor = only });

            Add(c.HairStyles, "bald", "Kel");
            Add(c.HairStyles, "crop", "Kırpık");
            Add(c.HairStyles, "short", "Kısa");
            Add(c.HairStyles, "shaggy", "Dağınık");
            Add(c.HairStyles, "long", "Uzun");
            Add(c.HairStyles, "ponytail", "At kuyruğu");
            Add(c.HairStyles, "braid", "Örgü");
            Add(c.HairStyles, "bun", "Topuz");

            Add(c.FacialHair, Appearance.None, "Yok");
            Add(c.FacialHair, "stubble", "Kirli sakal", Sex.Male);
            Add(c.FacialHair, "mustache", "Bıyık", Sex.Male);
            Add(c.FacialHair, "goatee", "Keçi sakalı", Sex.Male);
            Add(c.FacialHair, "beard", "Sakal", Sex.Male);
            Add(c.FacialHair, "full_beard", "Gür sakal", Sex.Male);

            Add(c.FaceMarks, Appearance.None, "Yok");
            Add(c.FaceMarks, "freckles", "Çil");
            Add(c.FaceMarks, "scar", "Yara izi");
            Add(c.FaceMarks, "mole", "Ben");
            Add(c.FaceMarks, "rosy", "Al yanaklar");

            Add(c.Outfits, "tunic", "Tunik");
            Add(c.Outfits, "long_tunic", "Uzun tunik");
            Add(c.Outfits, "jerkin", "Yelek");
            Add(c.Outfits, "apron", "Önlük");
            Add(c.Outfits, "robe", "Cüppe");
            Add(c.Outfits, "dress", "Elbise", Sex.Female);

            Add(c.Hats, Appearance.None, "Yok");
            Add(c.Hats, "cap", "Takke");
            Add(c.Hats, "hood", "Kapüşon");
            Add(c.Hats, "straw", "Hasır şapka");
            Add(c.Hats, "kerchief", "Başörtüsü");

            foreach (var h in new[] { "#F3D9C0", "#E8C39E", "#D9A877", "#C68E5F", "#A9703F", "#8A5A35", "#6B4226", "#4B2E1C" })
                c.SkinTones.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#1C1A19", "#3B2A1D", "#5A3A22", "#7A4B25", "#A0662F", "#C28A3C", "#D9B45A", "#E8D28A", "#A5471F", "#8C8C8C", "#D4D4D4", "#5E3A55" })
                c.HairColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#4B3621", "#6B4A2B", "#2F5D8C", "#3E7C59", "#7A8F3A", "#6C6C6C", "#9A4F8E" })
                c.EyeColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#A06E46", "#8C6A4A", "#C9B28A", "#E6DCC3", "#6E7F3F", "#4F6B3A", "#3F5F8A", "#2E4A6B", "#8A3A32", "#B4533C", "#6A3E6E", "#C2A23B", "#464646", "#B9B9B9" })
                c.ClothColors.Add(Rgb.FromHex(h));
            return c;
        }
    }
}
