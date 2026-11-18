using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Customization
{
    /// <summary>
    /// Bir seçenek: kimlik, görünen ad, ek etiket (saç dokusu, takı bölgesi) ve rastgele üretimde tercih edilen cinsiyet.
    /// Karar (17.11.2026): hiçbir seçenek cinsiyete kilitli değildir; PreferFor yalnızca rastgele köylü üretimini etkiler.
    /// </summary>
    public sealed class OptionDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>Rastgele üretimde yalnızca bu cinsiyete verilir (null = ikisi de). Oyuncu seçimini kısıtlamaz.</summary>
        public Sex? OnlyFor { get; set; }
        /// <summary>Ek bilgi: saç stilinde doku (straight/wavy/curly), takıda bölge, desende bölge listesi.</summary>
        public string Tag { get; set; }

        /// <summary>Rastgele üretimde bu cinsiyete uygun mu.</summary>
        public bool PreferredFor(Sex sex) => !OnlyFor.HasValue || OnlyFor.Value == sex;
        /// <summary>Geriye uyumluluk: artık her seçenek herkese açık.</summary>
        public bool AllowedFor(Sex sex) => true;
    }

    /// <summary>
    /// Karakter yaratmada seçilebilecek her şey (veri odaklı: appearance.json'dan yüklenir, yoksa Default).
    /// Doğrulama ve rastgele üretim buradadır. Proje dokümanı 14, bölüm 3.
    /// </summary>
    public sealed class AppearanceCatalog
    {
        // Saç ve sakal
        public List<OptionDef> HairStyles { get; } = new List<OptionDef>();
        public List<OptionDef> Bangs { get; } = new List<OptionDef>();
        public List<OptionDef> Mustaches { get; } = new List<OptionDef>();
        public List<OptionDef> Beards { get; } = new List<OptionDef>();
        /// <summary>Eski tek listeli sakal/bıyık (eski appearance.json uyumu). Yeni kod Mustaches ve Beards kullanır.</summary>
        public List<OptionDef> FacialHair { get; } = new List<OptionDef>();

        // Yüz (portre)
        public List<OptionDef> FaceShapes { get; } = new List<OptionDef>();
        public List<OptionDef> EyeShapes { get; } = new List<OptionDef>();
        public List<OptionDef> Brows { get; } = new List<OptionDef>();
        public List<OptionDef> Noses { get; } = new List<OptionDef>();
        public List<OptionDef> Mouths { get; } = new List<OptionDef>();
        public List<OptionDef> Ears { get; } = new List<OptionDef>();

        // İşaretler
        /// <summary>Eski tek yuvalı yüz işareti (eski appearance.json uyumu).</summary>
        public List<OptionDef> FaceMarks { get; } = new List<OptionDef>();
        /// <summary>Ten detayları; Tag: virgülle bölgeler (face,arm,hand).</summary>
        public List<OptionDef> SkinDetails { get; } = new List<OptionDef>();
        public List<OptionDef> ScarPatterns { get; } = new List<OptionDef>();
        public List<OptionDef> ScarZones { get; } = new List<OptionDef>();
        public List<OptionDef> TattooPatterns { get; } = new List<OptionDef>();
        public List<OptionDef> TattooZones { get; } = new List<OptionDef>();
        /// <summary>Takı; Tag: bölge (ear, nose, neck).</summary>
        public List<OptionDef> Jewelry { get; } = new List<OptionDef>();
        public List<OptionDef> Makeup { get; } = new List<OptionDef>();

        // Kıyafet
        public List<OptionDef> Outfits { get; } = new List<OptionDef>();
        public List<OptionDef> Bottoms { get; } = new List<OptionDef>();
        public List<OptionDef> Shoes { get; } = new List<OptionDef>();
        public List<OptionDef> Outers { get; } = new List<OptionDef>();
        public List<OptionDef> Hats { get; } = new List<OptionDef>();
        public List<OptionDef> Patterns { get; } = new List<OptionDef>();

        // Ses
        public List<OptionDef> Laughs { get; } = new List<OptionDef>();

        /// <summary>Ten rengi yalnızca bu paletten seçilir (16 ton × 3 açıklık).</summary>
        public List<Rgb> SkinTones { get; } = new List<Rgb>();
        /// <summary>Saç, göz ve kıyafet için hızlı seçim paletleri; bu renklerde özgür renk de serbesttir.</summary>
        public List<Rgb> HairColors { get; } = new List<Rgb>();
        public List<Rgb> EyeColors { get; } = new List<Rgb>();
        public List<Rgb> ClothColors { get; } = new List<Rgb>();
        public List<Rgb> InkColors { get; } = new List<Rgb>();
        public List<Rgb> MakeupColors { get; } = new List<Rgb>();
        /// <summary>Kalıtımda çekinik sayılan saç renkleri (kızıl, sarı tonları).</summary>
        public List<Rgb> RecessiveHairColors { get; } = new List<Rgb>();
        /// <summary>Kalıtımda çekinik sayılan göz renkleri (mavi, yeşil, gri).</summary>
        public List<Rgb> RecessiveEyeColors { get; } = new List<Rgb>();

        /// <summary>Boy kaldırıldı (karar 17.11.2026); sabit 100.</summary>
        public const int MinHeight = 100;
        public const int MaxHeight = 100;

        /// <summary>Oyuncunun seçebileceği seçenekler: hepsi (cinsiyet kilidi yok).</summary>
        public IEnumerable<OptionDef> For(IEnumerable<OptionDef> list, Sex sex) => list;

        private static OptionDef Find(List<OptionDef> list, string id) => list.FirstOrDefault(o => o.Id == id);

        /// <summary>Hataları okunur metin olarak döner; boşsa görünüm geçerli. Cinsiyet artık bir seçeneği geçersiz yapmaz.</summary>
        public List<string> Validate(Appearance a, Sex sex)
        {
            var errors = new List<string>();
            if (a == null) { errors.Add("Görünüm boş."); return errors; }

            if (!Enum.IsDefined(typeof(BodyBuild), a.Build)) errors.Add("Geçersiz vücut tipi.");
            if (!Enum.IsDefined(typeof(BodyFrame), a.Frame)) errors.Add("Geçersiz beden çerçevesi.");
            if (SkinTones.Count > 0 && !SkinTones.Contains(a.SkinColor)) errors.Add("Ten rengi paletten seçilmeli.");
            if (a.VoicePitch < Appearance.MinPitch || a.VoicePitch > Appearance.MaxPitch) errors.Add($"Ses perdesi {Appearance.MinPitch}..{Appearance.MaxPitch} arasında olmalı.");

            CheckOption(errors, HairStyles, a.HairStyleId, "saç stili");
            CheckOption(errors, Bangs, a.BangsId, "kâkül");
            CheckOption(errors, Mustaches, a.MustacheId, "bıyık");
            CheckOption(errors, Beards, a.BeardId, "sakal");
            CheckOption(errors, FaceShapes, a.FaceShapeId, "yüz şekli");
            CheckOption(errors, EyeShapes, a.EyeShapeId, "göz şekli");
            CheckOption(errors, Brows, a.BrowId, "kaş");
            CheckOption(errors, Noses, a.NoseId, "burun");
            CheckOption(errors, Mouths, a.MouthId, "ağız");
            CheckOption(errors, Ears, a.EarsId, "kulak");
            CheckOption(errors, Outfits, a.OutfitId, "kıyafet");
            CheckOption(errors, Bottoms, a.BottomId, "alt giysi");
            CheckOption(errors, Shoes, a.ShoesId, "ayakkabı");
            CheckOption(errors, Outers, a.OuterId, "dış giysi");
            CheckOption(errors, Hats, a.HatId, "başlık");
            CheckOption(errors, Patterns, a.PatternId, "kumaş deseni");
            CheckOption(errors, Laughs, a.LaughId, "gülüş");
            errors.AddRange(ValidateMarks(a.Marks));
            return errors;
        }

        /// <summary>İşaret sınırları: ten detayı 3, seçilmiş iz 3, bölge başına bir dövme, takı 3, makyaj 4.</summary>
        public List<string> ValidateMarks(IReadOnlyList<Mark> marks)
        {
            var errors = new List<string>();
            if (MarkRules.Count(marks, MarkKind.SkinDetail) > MarkRules.MaxSkinDetails) errors.Add($"En çok {MarkRules.MaxSkinDetails} ten detayı.");
            if (marks.Count(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Chosen) > MarkRules.MaxChosenScars) errors.Add($"En çok {MarkRules.MaxChosenScars} yara izi seçilebilir.");
            if (MarkRules.Count(marks, MarkKind.Jewelry) > MarkRules.MaxJewelry) errors.Add($"En çok {MarkRules.MaxJewelry} takı.");
            if (MarkRules.Count(marks, MarkKind.Makeup) > MarkRules.MaxMakeup) errors.Add($"En çok {MarkRules.MaxMakeup} makyaj.");
            var tattooZones = marks.Where(m => m.Kind == MarkKind.Tattoo).Select(m => m.Zone).ToList();
            if (tattooZones.Count != tattooZones.Distinct().Count()) errors.Add("Bir bölgede en çok bir dövme olabilir.");
            foreach (var m in marks)
            {
                switch (m.Kind)
                {
                    case MarkKind.SkinDetail: if (SkinDetails.Count > 0 && Find(SkinDetails, m.Id) == null) errors.Add($"Bilinmeyen ten detayı: '{m.Id}'."); break;
                    case MarkKind.Scar:
                        if (ScarPatterns.Count > 0 && Find(ScarPatterns, m.Id) == null) errors.Add($"Bilinmeyen iz deseni: '{m.Id}'.");
                        if (ScarZones.Count > 0 && Find(ScarZones, m.Zone) == null) errors.Add($"Bilinmeyen iz bölgesi: '{m.Zone}'.");
                        break;
                    case MarkKind.Tattoo:
                        if (TattooPatterns.Count > 0 && Find(TattooPatterns, m.Id) == null) errors.Add($"Bilinmeyen dövme deseni: '{m.Id}'.");
                        if (TattooZones.Count > 0 && Find(TattooZones, m.Zone) == null) errors.Add($"Bilinmeyen dövme bölgesi: '{m.Zone}'.");
                        break;
                    case MarkKind.Jewelry: if (Jewelry.Count > 0 && Find(Jewelry, m.Id) == null) errors.Add($"Bilinmeyen takı: '{m.Id}'."); break;
                    case MarkKind.Makeup: if (Makeup.Count > 0 && Find(Makeup, m.Id) == null) errors.Add($"Bilinmeyen makyaj: '{m.Id}'."); break;
                }
                if (m.Fade < 0 || m.Fade > 2) errors.Add("İşaret tazeliği 0-2 olmalı.");
            }
            return errors;
        }

        private static void CheckOption(List<string> errors, List<OptionDef> list, string id, string what)
        {
            if (list.Count == 0) return;   // katalogda yoksa (eski dosya) denetlenmez
            if (Find(list, id) == null) errors.Add($"Bilinmeyen {what}: '{id}'.");
        }

        /// <summary>Katalogda olmayan seçimleri ilk seçeneğe çevirir, boyu sabitler. Cinsiyete göre değiştirmez.</summary>
        public void Normalize(Appearance a, Sex sex)
        {
            a.HairStyleId = Valid(HairStyles, a.HairStyleId);
            a.BangsId = Valid(Bangs, a.BangsId);
            a.MustacheId = Valid(Mustaches, a.MustacheId);
            a.BeardId = Valid(Beards, a.BeardId);
            a.FaceShapeId = Valid(FaceShapes, a.FaceShapeId);
            a.EyeShapeId = Valid(EyeShapes, a.EyeShapeId);
            a.BrowId = Valid(Brows, a.BrowId);
            a.NoseId = Valid(Noses, a.NoseId);
            a.MouthId = Valid(Mouths, a.MouthId);
            a.EarsId = Valid(Ears, a.EarsId);
            a.OutfitId = Valid(Outfits, a.OutfitId);
            a.BottomId = Valid(Bottoms, a.BottomId);
            a.ShoesId = Valid(Shoes, a.ShoesId);
            a.OuterId = Valid(Outers, a.OuterId);
            a.HatId = Valid(Hats, a.HatId);
            a.PatternId = Valid(Patterns, a.PatternId);
            a.LaughId = Valid(Laughs, a.LaughId);
            a.HeightPercent = 100;
            if (a.VoicePitch < Appearance.MinPitch) a.VoicePitch = Appearance.MinPitch;
            if (a.VoicePitch > Appearance.MaxPitch) a.VoicePitch = Appearance.MaxPitch;
            if (SkinTones.Count > 0 && !SkinTones.Contains(a.SkinColor)) a.SkinColor = Genetics.Nearest(SkinTones, a.SkinColor);
        }

        private static string Valid(List<OptionDef> list, string id)
        {
            if (list.Count == 0) return id;
            if (Find(list, id) != null) return id;
            var none = Find(list, Appearance.None);
            return none != null ? none.Id : list[0].Id;
        }

        private static T Pick<T>(IList<T> list, Random rng) => list[rng.Next(list.Count)];

        private string PickId(List<OptionDef> list, Sex sex, Random rng, bool allowNone = true)
        {
            var allowed = list.Where(o => o.PreferredFor(sex) && (allowNone || o.Id != Appearance.None)).ToList();
            if (allowed.Count == 0) allowed = list.ToList();
            return allowed.Count == 0 ? Appearance.None : Pick(allowed, rng).Id;
        }

        private Rgb PickColor(List<Rgb> list, Random rng, Rgb fallback) => list.Count > 0 ? Pick(list, rng) : fallback;

        /// <summary>Saç dokusuna uyan rastgele bir stil (kel dahil değil).</summary>
        public string PickHairForTexture(string texture, Sex sex, Random rng)
        {
            var match = HairStyles.Where(o => o.Id != "bald" && o.PreferredFor(sex) && (string.IsNullOrEmpty(texture) || o.Tag == texture)).ToList();
            if (match.Count == 0) match = HairStyles.Where(o => o.PreferredFor(sex)).ToList();
            return match.Count == 0 ? "short" : Pick(match, rng).Id;
        }

        /// <summary>Rastgele (ama her zaman geçerli) bir görünüm. Cinsiyet yalnızca tercihleri ağırlıklandırır.</summary>
        public Appearance Randomize(Random rng, Sex sex)
        {
            var a = new Appearance
            {
                Build = (BodyBuild)rng.Next(Enum.GetValues(typeof(BodyBuild)).Length),
                Frame = rng.Next(10) == 0 ? BodyFrame.Neutral : (sex == Sex.Female ? BodyFrame.Feminine : BodyFrame.Masculine),
                SkinColor = PickColor(SkinTones, rng, new Rgb(232, 195, 158)),
                EyeColor = PickColor(EyeColors, rng, new Rgb(90, 60, 40)),
                HairColor = PickColor(HairColors, rng, new Rgb(90, 60, 35)),
                OutfitColor = PickColor(ClothColors, rng, new Rgb(160, 110, 70)),
                TrimColor = PickColor(ClothColors, rng, new Rgb(70, 50, 35)),
                HatColor = PickColor(ClothColors, rng, new Rgb(120, 90, 60)),
                OuterColor = PickColor(ClothColors, rng, new Rgb(90, 70, 50)),
                HairStyleId = PickId(HairStyles, sex, rng),
                BangsId = rng.Next(3) == 0 ? PickId(Bangs, sex, rng) : Appearance.None,
                FaceShapeId = PickId(FaceShapes, sex, rng, false),
                EyeShapeId = PickId(EyeShapes, sex, rng, false),
                BrowId = PickId(Brows, sex, rng, false),
                NoseId = PickId(Noses, sex, rng, false),
                MouthId = PickId(Mouths, sex, rng, false),
                EarsId = PickId(Ears, sex, rng, false),
                OutfitId = PickId(Outfits, sex, rng, false),
                BottomId = PickId(Bottoms, sex, rng, false),
                ShoesId = PickId(Shoes, sex, rng, false),
                OuterId = rng.Next(4) == 0 ? PickId(Outers, sex, rng) : Appearance.None,
                HatId = rng.Next(4) == 0 ? PickId(Hats, sex, rng) : Appearance.None,
                PatternId = rng.Next(4) == 0 ? PickId(Patterns, sex, rng, false) : (Patterns.Count > 0 ? Patterns[0].Id : "plain"),
                Voice = (VoiceTimbre)rng.Next(4),
                VoicePitch = rng.Next(5) - 2,
                LaughId = PickId(Laughs, sex, rng, false)
            };
            a.FacialHairColor = a.HairColor;
            if (sex == Sex.Male && rng.Next(2) == 0)
            {
                if (rng.Next(2) == 0) a.MustacheId = PickId(Mustaches, sex, rng, false);
                if (rng.Next(3) > 0) a.BeardId = PickId(Beards, sex, rng, false);
            }
            if (rng.Next(3) == 0 && SkinDetails.Count > 0)
            {
                var d = Pick(SkinDetails, rng);
                a.Marks.Add(new Mark { Kind = MarkKind.SkinDetail, Id = d.Id, Zone = FirstZone(d.Tag, "face") });
            }
            Normalize(a, sex);
            return a;
        }

        private static string FirstZone(string tag, string fallback)
        {
            if (string.IsNullOrEmpty(tag)) return fallback;
            var z = tag.Split(',')[0].Trim();
            return z.Length == 0 ? fallback : z;
        }

        /// <summary>
        /// Çocuğun görünümü (genom yoksa eski yol): ten ebeveyn ortalaması (palete en yakın), saç ve göz rengi
        /// ebeveynlerden biri, beden ebeveynlerden biri, stiller rastgele. Genomlu doğum Population'da Genetics ile yapılır.
        /// </summary>
        public Appearance Inherit(Appearance mother, Appearance father, Random rng, Sex childSex)
        {
            var baby = Randomize(rng, childSex);
            baby.MustacheId = Appearance.None;
            baby.BeardId = Appearance.None;
            baby.HatId = Appearance.None;
            baby.OuterId = Appearance.None;
            baby.Marks.Clear();

            var parents = new[] { mother, father };
            baby.HairColor = Pick(parents, rng).HairColor;
            baby.FacialHairColor = baby.HairColor;
            baby.EyeColor = Pick(parents, rng).EyeColor;
            baby.Build = Pick(parents, rng).Build;

            var mix = Rgb.Lerp(mother.SkinColor, father.SkinColor, 0.5);
            baby.SkinColor = Genetics.Nearest(SkinTones, mix);
            Normalize(baby, childSex);
            return baby;
        }

        /// <summary>Genomdan bebek görünümü: rastgele temel + genlerin yansıması + dokuya uygun saç.</summary>
        public Appearance FromGenome(Genome genome, Random rng, Sex sex)
        {
            var baby = Randomize(rng, sex);
            baby.MustacheId = Appearance.None;
            baby.BeardId = Appearance.None;
            baby.HatId = Appearance.None;
            baby.OuterId = Appearance.None;
            baby.BangsId = Appearance.None;
            baby.Marks.Clear();
            Genetics.Express(genome, baby, this);
            string texture = Genetics.Expressed(genome, Genes.HairTexture, this);
            baby.HairStyleId = PickHairForTexture(texture, sex, rng);
            Normalize(baby, sex);
            return baby;
        }

        /// <summary>Yerleşik varsayılan katalog (appearance.json okunamazsa kullanılır).</summary>
        public static AppearanceCatalog Default()
        {
            var c = new AppearanceCatalog();
            void Add(List<OptionDef> l, string id, string name, Sex? only = null, string tag = null) =>
                l.Add(new OptionDef { Id = id, DisplayName = name, OnlyFor = only, Tag = tag });

            Add(c.HairStyles, "bald", "Kel");
            Add(c.HairStyles, "crop", "Kırpık", null, "straight");
            Add(c.HairStyles, "short", "Kısa", null, "straight");
            Add(c.HairStyles, "shaggy", "Dağınık", null, "wavy");
            Add(c.HairStyles, "long", "Uzun", null, "straight");
            Add(c.HairStyles, "ponytail", "At kuyruğu", null, "wavy");
            Add(c.HairStyles, "braid", "Örgü", null, "straight");
            Add(c.HairStyles, "bun", "Topuz", null, "curly");
            Add(c.HairStyles, "curls", "Bukleler", null, "curly");

            Add(c.Bangs, Appearance.None, "Yok");
            Add(c.Bangs, "straight", "Düz kâkül");
            Add(c.Bangs, "side", "Yana kâkül");

            Add(c.Mustaches, Appearance.None, "Yok");
            Add(c.Mustaches, "mustache", "Bıyık", Sex.Male);
            Add(c.Mustaches, "handlebar", "Burma bıyık", Sex.Male);

            Add(c.Beards, Appearance.None, "Yok");
            Add(c.Beards, "stubble", "Kirli sakal", Sex.Male);
            Add(c.Beards, "goatee", "Keçi sakalı", Sex.Male);
            Add(c.Beards, "beard", "Sakal", Sex.Male);
            Add(c.Beards, "full_beard", "Gür sakal", Sex.Male);

            foreach (var o in c.Beards) c.FacialHair.Add(o);
            c.FacialHair.Insert(1, c.Mustaches[1]);

            Add(c.FaceShapes, "oval", "Oval");
            Add(c.FaceShapes, "round", "Yuvarlak");
            Add(c.FaceShapes, "square", "Köşeli");
            Add(c.EyeShapes, "almond", "Badem");
            Add(c.EyeShapes, "round", "Yuvarlak");
            Add(c.Brows, "natural", "Doğal");
            Add(c.Brows, "thick", "Kalın");
            Add(c.Noses, "straight", "Düz");
            Add(c.Noses, "button", "Kalkık");
            Add(c.Mouths, "neutral", "Düz");
            Add(c.Mouths, "full", "Dolgun");
            Add(c.Ears, "normal", "Normal");
            Add(c.Ears, "small", "Küçük");

            Add(c.FaceMarks, Appearance.None, "Yok");
            Add(c.FaceMarks, "freckles", "Çil");
            Add(c.FaceMarks, "scar", "Yara izi");
            Add(c.FaceMarks, "mole", "Ben");
            Add(c.FaceMarks, "rosy", "Al yanaklar");

            Add(c.SkinDetails, "freckles", "Çil", null, "face");
            Add(c.SkinDetails, "mole", "Ben", null, "face");
            Add(c.SkinDetails, "rosy", "Al yanaklar", null, "face");
            Add(c.ScarPatterns, "split", "Yarık");
            Add(c.ScarPatterns, "scratch", "Çizik");
            Add(c.ScarPatterns, "burn", "Yanık");
            Add(c.ScarPatterns, "bite", "Isırık");
            Add(c.ScarZones, "brow", "Kaş-göz");
            Add(c.ScarZones, "cheek", "Yanak");
            Add(c.ScarZones, "hand", "El");
            Add(c.ScarZones, "larm", "Sol kol");
            Add(c.ScarZones, "rarm", "Sağ kol");
            Add(c.TattooPatterns, "knot", "Kelt düğümü");
            Add(c.TattooPatterns, "wheat", "Başak");
            Add(c.TattooZones, "larm", "Sol kol");
            Add(c.TattooZones, "rarm", "Sağ kol");
            Add(c.Jewelry, "earring", "Küpe", null, "ear");
            Add(c.Makeup, "kohl", "Sürme");

            Add(c.Outfits, "tunic", "Tunik");
            Add(c.Outfits, "long_tunic", "Uzun tunik");
            Add(c.Outfits, "jerkin", "Yelek");
            Add(c.Outfits, "apron", "Önlük");
            Add(c.Outfits, "robe", "Cüppe");
            Add(c.Outfits, "dress", "Elbise", Sex.Female);
            Add(c.Bottoms, "trousers", "Pantolon");
            Add(c.Bottoms, "skirt", "Etek", Sex.Female);
            Add(c.Shoes, "boots", "Çizme");
            Add(c.Shoes, "shoes", "Ayakkabı");
            Add(c.Outers, Appearance.None, "Yok");
            Add(c.Outers, "cloak", "Pelerin");
            Add(c.Patterns, "plain", "Düz");
            Add(c.Patterns, "striped", "Çizgili");

            Add(c.Hats, Appearance.None, "Yok");
            Add(c.Hats, "cap", "Takke");
            Add(c.Hats, "hood", "Kapüşon");
            Add(c.Hats, "straw", "Hasır şapka");
            Add(c.Hats, "kerchief", "Başörtüsü");

            Add(c.Laughs, "chuckle", "Kıkırdama");
            Add(c.Laughs, "hearty", "Kahkaha");

            foreach (var h in new[] { "#F3D9C0", "#E8C39E", "#D9A877", "#C68E5F", "#A9703F", "#8A5A35", "#6B4226", "#4B2E1C" })
                c.SkinTones.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#1C1A19", "#3B2A1D", "#5A3A22", "#7A4B25", "#A0662F", "#C28A3C", "#D9B45A", "#E8D28A", "#A5471F", "#8C8C8C", "#D4D4D4", "#5E3A55" })
                c.HairColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#4B3621", "#6B4A2B", "#2F5D8C", "#3E7C59", "#7A8F3A", "#6C6C6C", "#9A4F8E" })
                c.EyeColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#A06E46", "#8C6A4A", "#C9B28A", "#E6DCC3", "#6E7F3F", "#4F6B3A", "#3F5F8A", "#2E4A6B", "#8A3A32", "#B4533C", "#6A3E6E", "#C2A23B", "#464646", "#B9B9B9" })
                c.ClothColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#2F4E7A", "#2A2622", "#8E3324" }) c.InkColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#2A2622", "#B4533C", "#8A3A32", "#A0662F" }) c.MakeupColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#A5471F", "#D9B45A", "#E8D28A", "#C28A3C" }) c.RecessiveHairColors.Add(Rgb.FromHex(h));
            foreach (var h in new[] { "#2F5D8C", "#3E7C59", "#6C6C6C", "#7A8F3A" }) c.RecessiveEyeColors.Add(Rgb.FromHex(h));
            return c;
        }
    }
}
