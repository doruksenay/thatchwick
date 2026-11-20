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
        /// <summary>Ek bilgi: saç stilinde doku (straight/wavy/curly), takıda bölge, desende bölge listesi; göz ve kaşta dünya deseni.</summary>
        public string Tag { get; set; }
        /// <summary>
        /// Saç modülleri için uzunluk düzeyi (belge 43b). Kütlede saçın uzunluğu (0 kel, 1 kısa, 2 ense, 3 omuz, 4 sırt, 5 bel);
        /// ön, eklenti ve süste en az gereken kütle uzunluğu.
        /// </summary>
        public int Level { get; set; }
        /// <summary>Ekrandaki ızgarada alt grup (ör. saçta kısa/orta/uzun/toplu).</summary>
        public string Group { get; set; }

        /// <summary>Rastgele üretimde bu cinsiyete uygun mu.</summary>
        public bool PreferredFor(Sex sex) => !OnlyFor.HasValue || OnlyFor.Value == sex;
        /// <summary>Geriye uyumluluk: artık her seçenek herkese açık.</summary>
        public bool AllowedFor(Sex sex) => true;
    }

    /// <summary>Hazır saç stili: modüllerden adı olan bir birleşim (belge 43b). Ekranda ilk ızgara bunlardır.</summary>
    public sealed class HairPreset
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Group { get; set; }
        public string Mass { get; set; }
        public string Front { get; set; } = Appearance.None;
        public string Extra { get; set; } = Appearance.None;
        public Sex? OnlyFor { get; set; }
    }

    /// <summary>Eski tam saç stili kimliğinin modüler karşılığı (kayıt ve eski dosya uyumu).</summary>
    public sealed class LegacyHair
    {
        public string Mass { get; set; }
        public string Front { get; set; } = Appearance.None;
        public string Extra { get; set; } = Appearance.None;
    }

    /// <summary>
    /// Karakter yaratmada seçilebilecek her şey (veri odaklı: appearance.json'dan yüklenir, yoksa Default).
    /// Doğrulama ve rastgele üretim buradadır. Proje dokümanı 14, bölüm 3.
    /// </summary>
    public sealed class AppearanceCatalog
    {
        // Saç ve sakal (modüler, belge 43b)
        /// <summary>Saç kütlesi (uzunluk ve doku). Level = uzunluk, Tag = doku.</summary>
        public List<OptionDef> HairStyles { get; } = new List<OptionDef>();
        /// <summary>Saçın önü. Level = gereken en az kütle uzunluğu.</summary>
        public List<OptionDef> Bangs { get; } = new List<OptionDef>();
        /// <summary>Saç eklentisi (örgü, topuz, at kuyruğu). Level = gereken en az kütle uzunluğu.</summary>
        public List<OptionDef> HairExtras { get; } = new List<OptionDef>();
        /// <summary>Saç süsü. Level = gereken en az kütle uzunluğu.</summary>
        public List<OptionDef> HairOrnaments { get; } = new List<OptionDef>();
        public List<OptionDef> Mustaches { get; } = new List<OptionDef>();
        public List<OptionDef> Beards { get; } = new List<OptionDef>();
        public List<OptionDef> Sideburns { get; } = new List<OptionDef>();
        /// <summary>Hazır saç stilleri (modül birleşimleri).</summary>
        public List<HairPreset> HairPresets { get; } = new List<HairPreset>();
        /// <summary>Eski tam saç stili kimliği → modüller.</summary>
        public Dictionary<string, LegacyHair> LegacyHairStyles { get; } = new Dictionary<string, LegacyHair>(StringComparer.Ordinal);
        /// <summary>Eski tek listeli sakal/bıyık (eski appearance.json uyumu). Yeni kod Mustaches ve Beards kullanır.</summary>
        public List<OptionDef> FacialHair { get; } = new List<OptionDef>();

        // Yüz (portre)
        public List<OptionDef> FaceShapes { get; } = new List<OptionDef>();
        public List<OptionDef> EyeShapes { get; } = new List<OptionDef>();
        public List<OptionDef> Brows { get; } = new List<OptionDef>();
        public List<OptionDef> Noses { get; } = new List<OptionDef>();
        public List<OptionDef> Mouths { get; } = new List<OptionDef>();
        public List<OptionDef> Ears { get; } = new List<OptionDef>();
        public List<OptionDef> Lashes { get; } = new List<OptionDef>();

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
            CheckOption(errors, Bangs, a.BangsId, "saç önü");
            CheckOption(errors, HairExtras, a.HairExtraId, "saç eklentisi");
            CheckOption(errors, HairOrnaments, a.HairOrnamentId, "saç süsü");
            CheckOption(errors, Mustaches, a.MustacheId, "bıyık");
            CheckOption(errors, Beards, a.BeardId, "sakal");
            CheckOption(errors, Sideburns, a.SideburnsId, "favori");
            CheckOption(errors, Lashes, a.LashesId, "kirpik");
            if (a.StubbleLevel < 0 || a.StubbleLevel > Appearance.MaxStubble) errors.Add($"Kirli sakal 0..{Appearance.MaxStubble} arasında olmalı.");
            errors.AddRange(ValidateHair(a.HairStyleId, a.BangsId, a.HairExtraId, a.HairOrnamentId));
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
            MigrateLegacy(a);
            a.HairStyleId = Valid(HairStyles, a.HairStyleId);
            a.BangsId = Valid(Bangs, a.BangsId);
            a.HairExtraId = Valid(HairExtras, a.HairExtraId);
            a.HairOrnamentId = Valid(HairOrnaments, a.HairOrnamentId);
            FitHair(a);
            a.MustacheId = Valid(Mustaches, a.MustacheId);
            a.BeardId = Valid(Beards, a.BeardId);
            a.SideburnsId = Valid(Sideburns, a.SideburnsId);
            a.LashesId = Valid(Lashes, a.LashesId);
            if (a.StubbleLevel < 0) a.StubbleLevel = 0;
            if (a.StubbleLevel > Appearance.MaxStubble) a.StubbleLevel = Appearance.MaxStubble;
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

        // --- Modüler saç (belge 43b) ---

        /// <summary>Kütlenin uzunluk düzeyi (bilinmeyen kimlik: 1).</summary>
        public int MassLength(string massId)
        {
            var m = Find(HairStyles, ResolveMass(massId));
            return m == null ? 1 : m.Level;
        }

        /// <summary>Eski tam stil kimliğini kütle kimliğine çevirir; değilse olduğu gibi döner.</summary>
        public string ResolveMass(string id) =>
            id != null && Find(HairStyles, id) == null && LegacyHairStyles.TryGetValue(id, out var l) ? l.Mass : id;

        /// <summary>Saç dokusu (genetik için): kütlenin Tag'i; eski kimlik çözülür.</summary>
        public string HairTextureOf(string massId) => Find(HairStyles, ResolveMass(massId))?.Tag;

        /// <summary>Bu modül (ön, eklenti ya da süs) bu kütleye uyar mı. "none" her zaman uyar.</summary>
        public bool Fits(List<OptionDef> modules, string moduleId, string massId)
        {
            if (string.IsNullOrEmpty(moduleId) || moduleId == Appearance.None) return true;
            var m = Find(modules, moduleId);
            if (m == null) return true;   // denetimi Validate yapar
            return m.Level <= MassLength(massId);
        }

        /// <summary>Kütleye uyan modüller (ekranda yalnız bunlar gösterilir).</summary>
        public IEnumerable<OptionDef> FittingModules(List<OptionDef> modules, string massId) =>
            modules.Where(o => Fits(modules, o.Id, massId));

        /// <summary>Uyumsuz saç birleşimleri için hata metinleri.</summary>
        public List<string> ValidateHair(string mass, string front, string extra, string ornament)
        {
            var errors = new List<string>();
            if (!Fits(Bangs, front, mass)) errors.Add($"Saç önü '{front}' bu saç boyuna uymuyor.");
            if (!Fits(HairExtras, extra, mass)) errors.Add($"Saç eklentisi '{extra}' bu saç boyuna uymuyor.");
            if (!Fits(HairOrnaments, ornament, mass)) errors.Add($"Saç süsü '{ornament}' bu saç boyuna uymuyor.");
            return errors;
        }

        /// <summary>Uymayan modülleri kaldırır (kütle kısaltılınca örgü düşer).</summary>
        public void FitHair(Appearance a)
        {
            if (!Fits(Bangs, a.BangsId, a.HairStyleId)) a.BangsId = Appearance.None;
            if (!Fits(HairExtras, a.HairExtraId, a.HairStyleId)) a.HairExtraId = Appearance.None;
            if (!Fits(HairOrnaments, a.HairOrnamentId, a.HairStyleId)) a.HairOrnamentId = Appearance.None;
        }

        /// <summary>
        /// Eski kayıt ve eski kod uyumu: tam saç stili kimliği kütle + ön + eklentiye, eski "stubble" sakalı
        /// kirli sakal dokusuna çevrilir. Yeni kimliklere dokunmaz.
        /// </summary>
        public void MigrateLegacy(Appearance a)
        {
            if (a.HairStyleId != null && Find(HairStyles, a.HairStyleId) == null && LegacyHairStyles.TryGetValue(a.HairStyleId, out var l))
            {
                a.HairStyleId = l.Mass;
                if ((a.BangsId == null || a.BangsId == Appearance.None) && l.Front != Appearance.None) a.BangsId = l.Front;
                if ((a.HairExtraId == null || a.HairExtraId == Appearance.None) && l.Extra != Appearance.None) a.HairExtraId = l.Extra;
            }
            if (a.BeardId == "stubble" && Find(Beards, "stubble") == null)
            {
                a.BeardId = Appearance.None;
                if (a.StubbleLevel == 0) a.StubbleLevel = 2;
            }
        }

        /// <summary>Hazır stili uygular (kütle, ön, eklenti); süse dokunmaz, uymuyorsa düşürür.</summary>
        public void ApplyPreset(Appearance a, HairPreset p)
        {
            if (p == null) return;
            a.HairStyleId = p.Mass;
            a.BangsId = p.Front ?? Appearance.None;
            a.HairExtraId = p.Extra ?? Appearance.None;
            FitHair(a);
        }

        /// <summary>Görünümün birebir eşleştiği hazır stil (yoksa null): ekranda seçili hücre.</summary>
        public HairPreset PresetOf(Appearance a) =>
            HairPresets.FirstOrDefault(p => p.Mass == a.HairStyleId && (p.Front ?? Appearance.None) == a.BangsId && (p.Extra ?? Appearance.None) == a.HairExtraId);

        /// <summary>Hazır stillerin geçerli olduğunu denetler (içerik dosyası testleri için).</summary>
        public List<string> ValidatePresets()
        {
            var errors = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in HairPresets)
            {
                if (!seen.Add(p.Id)) errors.Add($"Hazır stil tekrar ediyor: '{p.Id}'.");
                if (Find(HairStyles, p.Mass) == null) errors.Add($"Hazır stil '{p.Id}': bilinmeyen kütle '{p.Mass}'.");
                if (Bangs.Count > 0 && Find(Bangs, p.Front ?? Appearance.None) == null) errors.Add($"Hazır stil '{p.Id}': bilinmeyen ön '{p.Front}'.");
                if (HairExtras.Count > 0 && Find(HairExtras, p.Extra ?? Appearance.None) == null) errors.Add($"Hazır stil '{p.Id}': bilinmeyen eklenti '{p.Extra}'.");
                foreach (var e in ValidateHair(p.Mass, p.Front, p.Extra, Appearance.None)) errors.Add($"Hazır stil '{p.Id}': {e}");
            }
            foreach (var kv in LegacyHairStyles)
            {
                if (Find(HairStyles, kv.Value.Mass) == null) errors.Add($"Eski saç '{kv.Key}': bilinmeyen kütle '{kv.Value.Mass}'.");
                foreach (var e in ValidateHair(kv.Value.Mass, kv.Value.Front, kv.Value.Extra, Appearance.None)) errors.Add($"Eski saç '{kv.Key}': {e}");
            }
            return errors;
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

        /// <summary>Kel, tepe tıraşı, tepesi açık gibi kütleler: bebek ve çocuğa verilmez.</summary>
        public const string SpecialGroup = "ozel";

        /// <summary>Saç dokusuna uyan rastgele bir stil (kel dahil değil).</summary>
        public string PickHairForTexture(string texture, Sex sex, Random rng)
        {
            var match = HairStyles.Where(o => o.Id != "bald" && o.Group != SpecialGroup && o.PreferredFor(sex) && (string.IsNullOrEmpty(texture) || o.Tag == texture)).ToList();
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
                BangsId = Appearance.None,
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
            RandomizeHair(a, rng, sex);
            a.FacialHairColor = a.HairColor;
            if (sex == Sex.Male && rng.Next(2) == 0)
            {
                if (rng.Next(2) == 0) a.MustacheId = PickId(Mustaches, sex, rng, false);
                if (rng.Next(3) > 0) a.BeardId = PickId(Beards, sex, rng, false);
                if (Sideburns.Count > 1 && rng.Next(3) == 0) a.SideburnsId = PickId(Sideburns, sex, rng, false);
            }
            if (sex == Sex.Male && a.BeardId == Appearance.None && rng.Next(3) == 0) a.StubbleLevel = 1 + rng.Next(Appearance.MaxStubble);
            if (rng.Next(3) == 0 && SkinDetails.Count > 0)
            {
                var d = Pick(SkinDetails, rng);
                a.Marks.Add(new Mark { Kind = MarkKind.SkinDetail, Id = d.Id, Zone = FirstZone(d.Tag, "face") });
            }
            Normalize(a, sex);
            return a;
        }

        /// <summary>
        /// Rastgele saç: çoğunlukla hazır stillerden (tutarlı görünüm), bazen modüllerden. Her zaman uyumlu birleşim verir.
        /// </summary>
        public void RandomizeHair(Appearance a, Random rng, Sex sex)
        {
            var presets = HairPresets.Where(p => !p.OnlyFor.HasValue || p.OnlyFor.Value == sex).ToList();
            if (presets.Count > 0 && rng.Next(10) < 7)
            {
                ApplyPreset(a, presets[rng.Next(presets.Count)]);
            }
            else
            {
                if (HairStyles.Count > 0) a.HairStyleId = PickId(HairStyles, sex, rng);
                a.BangsId = Appearance.None;
                a.HairExtraId = Appearance.None;
                if (Bangs.Count > 0 && rng.Next(2) == 0) a.BangsId = PickFitting(Bangs, a.HairStyleId, sex, rng);
                if (HairExtras.Count > 0 && rng.Next(3) == 0) a.HairExtraId = PickFitting(HairExtras, a.HairStyleId, sex, rng);
            }
            a.HairOrnamentId = Appearance.None;
            if (HairOrnaments.Count > 1 && rng.Next(6) == 0)
            {
                a.HairOrnamentId = PickFitting(HairOrnaments, a.HairStyleId, sex, rng);
                a.HairOrnamentColor = PickColor(ClothColors, rng, a.HairOrnamentColor);
            }
            FitHair(a);
        }

        private string PickFitting(List<OptionDef> modules, string massId, Sex sex, Random rng)
        {
            var fit = modules.Where(o => o.Id != Appearance.None && o.PreferredFor(sex) && Fits(modules, o.Id, massId)).ToList();
            return fit.Count == 0 ? Appearance.None : fit[rng.Next(fit.Count)].Id;
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
            ClearAdultOnly(baby);
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
            ClearAdultOnly(baby);
            baby.HatId = Appearance.None;
            baby.OuterId = Appearance.None;
            baby.BangsId = Appearance.None;
            baby.Marks.Clear();
            Genetics.Express(genome, baby, this);
            string texture = Genetics.Expressed(genome, Genes.HairTexture, this);
            baby.HairStyleId = PickHairForTexture(texture, sex, rng);
            baby.HairExtraId = Appearance.None;
            Normalize(baby, sex);
            return baby;
        }

        /// <summary>Bebekte olmayanlar: sakal, bıyık, favori, kirli sakal, saç süsü ve eklentisi.</summary>
        private static void ClearAdultOnly(Appearance a)
        {
            a.MustacheId = Appearance.None;
            a.BeardId = Appearance.None;
            a.SideburnsId = Appearance.None;
            a.StubbleLevel = 0;
            a.HairExtraId = Appearance.None;
            a.HairOrnamentId = Appearance.None;
        }

        /// <summary>Yerleşik varsayılan katalog (appearance.json okunamazsa kullanılır).</summary>
        public static AppearanceCatalog Default()
        {
            var c = new AppearanceCatalog();
            void Add(List<OptionDef> l, string id, string name, Sex? only = null, string tag = null) =>
                l.Add(new OptionDef { Id = id, DisplayName = name, OnlyFor = only, Tag = tag });

            // Modüler saç (belge 43b). Level: kütlede uzunluk, modülde gereken en az uzunluk.
            void Mod(List<OptionDef> l, string id, string name, int level, string tag = null, string group = null, Sex? only = null) =>
                l.Add(new OptionDef { Id = id, DisplayName = name, Level = level, Tag = tag, Group = group, OnlyFor = only });
            Mod(c.HairStyles, "bald", "Kel", 0, null, SpecialGroup);
            Mod(c.HairStyles, "crop", "Kırpık", 1, "straight", "kisa");
            Mod(c.HairStyles, "short", "Kısa", 1, "straight", "kisa");
            Mod(c.HairStyles, "curls", "Kısa bukleler", 1, "curly", "kisa");
            Mod(c.HairStyles, "nape", "Ense boyu", 2, "wavy", "orta");
            Mod(c.HairStyles, "shoulder", "Omuz boyu", 3, "wavy", "orta");
            Mod(c.HairStyles, "long", "Uzun düz", 4, "straight", "uzun");
            Mod(c.HairStyles, "long_curly", "Uzun kıvırcık", 4, "curly", "uzun");

            Mod(c.Bangs, Appearance.None, "Alın açık", 0);
            Mod(c.Bangs, "straight", "Düz kâkül", 1);
            Mod(c.Bangs, "side", "Yana kâkül", 1);
            Mod(c.Bangs, "parted", "Ortadan ayrık", 1);

            Mod(c.HairExtras, Appearance.None, "Yok", 0);
            Mod(c.HairExtras, "ponytail", "At kuyruğu", 2);
            Mod(c.HairExtras, "braid", "Örgü", 3);
            Mod(c.HairExtras, "bun", "Topuz", 3);

            Mod(c.HairOrnaments, Appearance.None, "Yok", 0);
            Mod(c.HairOrnaments, "ribbon", "Kurdele", 2);
            Mod(c.HairOrnaments, "flower", "Çiçek", 1);

            Mod(c.Sideburns, Appearance.None, "Yok", 0);
            Mod(c.Sideburns, "short", "Kısa favori", 0, null, null, Sex.Male);

            Mod(c.Lashes, "normal", "Normal", 0);
            Mod(c.Lashes, "long", "Uzun", 0);

            c.HairPresets.Add(new HairPreset { Id = "p_short", DisplayName = "Kısa", Group = "kisa", Mass = "short" });
            c.HairPresets.Add(new HairPreset { Id = "p_bangs", DisplayName = "Kâküllü kısa", Group = "kisa", Mass = "short", Front = "straight" });
            c.HairPresets.Add(new HairPreset { Id = "p_ponytail", DisplayName = "At kuyruğu", Group = "toplu", Mass = "shoulder", Extra = "ponytail" });
            c.HairPresets.Add(new HairPreset { Id = "p_braid", DisplayName = "Örgü", Group = "toplu", Mass = "long", Front = "parted", Extra = "braid" });

            c.LegacyHairStyles["shaggy"] = new LegacyHair { Mass = "nape" };
            c.LegacyHairStyles["ponytail"] = new LegacyHair { Mass = "shoulder", Extra = "ponytail" };
            c.LegacyHairStyles["braid"] = new LegacyHair { Mass = "long", Extra = "braid" };
            c.LegacyHairStyles["bun"] = new LegacyHair { Mass = "shoulder", Extra = "bun" };

            Add(c.Mustaches, Appearance.None, "Yok");
            Add(c.Mustaches, "mustache", "Bıyık", Sex.Male);
            Add(c.Mustaches, "handlebar", "Burma bıyık", Sex.Male);

            Add(c.Beards, Appearance.None, "Yok");
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
