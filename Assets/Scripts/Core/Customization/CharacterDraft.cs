using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Customization
{
    /// <summary>
    /// Karakter yaratma ekranının taslağı (Unity'siz, test edilebilir): isim, cinsiyet, başlangıç
    /// yaşı, görünüm ve beceri eğilimi. Ekran bunu düzenler, "Tamam" deyince Character üretilir.
    /// </summary>
    public sealed class CharacterDraft
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 16;
        public const double YouthStartAge = 30.0;
        public const double AdultStartAge = 40.0;
        /// <summary>Beceri eğilimi: seçilen beceri bu kadar XP ile başlar (seviye 2).</summary>
        public const double FocusXp = 100.0;

        private static readonly Regex NamePattern = new Regex(@"^[\p{L}][\p{L} '\-]*$", RegexOptions.Compiled);

        public string Name { get; set; } = "";
        public Sex Sex { get; private set; } = Sex.Male;
        /// <summary>Başlangıç evresi: yalnızca Genç ya da Yetişkin seçilebilir.</summary>
        public AgeStage StartStage { get; set; } = AgeStage.Adult;
        public Appearance Appearance { get; set; } = new Appearance();
        public SkillType? SkillFocus { get; set; }

        // --- Karakter yaratma 2.0 (doküman 14) ---
        public const int MaxDynastyLength = 20;
        /// <summary>Hanedan adı (yeni oyunda bir kez). Boşsa "&lt;isim&gt; hanedanı".</summary>
        public string DynastyName { get; set; } = "";
        /// <summary>Doğum günü: mevsim (0-3) ve mevsim günü.</summary>
        public int BirthSeason { get; set; }
        public int BirthDayOfSeason { get; set; }
        public string LikedFood { get; set; } = "";
        public string LikedColor { get; set; } = "";
        public string LikedHobby { get; set; } = "";
        public string DislikedFood { get; set; } = "";
        /// <summary>Kaza ayarı: oyuncu yeni oyunda kendisi seçer (karar 17.11.2026); seçilmeden başlanmaz.</summary>
        public AccidentSetting Accidents { get; set; } = AccidentSetting.NotSet;

        public string EffectiveDynastyName => string.IsNullOrWhiteSpace(DynastyName) ? $"{(Name ?? "").Trim()} hanedanı" : DynastyName.Trim();

        /// <summary>Seçilen özellikler (en çok Character.MaxTraits, çakışmayan). Boşsa oyun başlarken rastgele verilir.</summary>
        public List<string> TraitIds { get; } = new List<string>();

        /// <summary>Özelliği seçer ya da seçimden çıkarır; sınır/çakışma varsa false (neden `why`).</summary>
        public bool ToggleTrait(ContentDatabase db, string traitId, out string why)
        {
            why = null;
            if (TraitIds.Remove(traitId)) return true;
            if (TraitIds.Count >= Character.MaxTraits) { why = $"En çok {Character.MaxTraits} özellik seçilebilir."; return false; }
            if (!LifeRules.CanAdd(db, TraitIds, traitId)) { why = "Bu özellik seçtiklerinle çakışıyor."; return false; }
            TraitIds.Add(traitId);
            return true;
        }

        /// <summary>Seçili özellikler içerikte var, sınırı aşmıyor ve çakışmıyor mu.</summary>
        public List<string> ValidateTraits(ContentDatabase db)
        {
            var errors = new List<string>();
            if (TraitIds.Count > Character.MaxTraits) errors.Add($"En çok {Character.MaxTraits} özellik seçilebilir.");
            var seen = new List<string>();
            foreach (var id in TraitIds)
            {
                if (!db.Traits.Contains(id)) { errors.Add($"Bilinmeyen özellik: {id}."); continue; }
                if (!LifeRules.CanAdd(db, seen, id)) errors.Add("Seçilen özellikler çakışıyor.");
                seen.Add(id);
            }
            return errors;
        }

        /// <summary>Cinsiyeti değiştirir; o cinsiyete uymayan seçenekleri (ör. sakal) uygun olana çevirir.</summary>
        public void SetSex(Sex sex, AppearanceCatalog catalog)
        {
            Sex = sex;
            catalog.Normalize(Appearance, sex);
        }

        public static CharacterDraft CreateRandom(AppearanceCatalog catalog, Random rng, string name = "")
        {
            var d = new CharacterDraft { Name = name };
            d.Sex = rng.Next(2) == 0 ? Sex.Female : Sex.Male;
            d.StartStage = rng.Next(2) == 0 ? AgeStage.Youth : AgeStage.Adult;
            d.Appearance = catalog.Randomize(rng, d.Sex);
            d.BirthSeason = rng.Next(4);
            d.BirthDayOfSeason = rng.Next(10);
            d.LikedColor = Likes.Colors[rng.Next(Likes.Colors.Length)];
            d.LikedHobby = Likes.Hobbies[rng.Next(Likes.Hobbies.Length)];
            return d;
        }

        public List<string> Validate(AppearanceCatalog catalog)
        {
            var errors = new List<string>();
            string name = Name == null ? "" : Name.Trim();
            if (name.Length < MinNameLength || name.Length > MaxNameLength)
                errors.Add($"İsim {MinNameLength}-{MaxNameLength} karakter olmalı.");
            else if (!NamePattern.IsMatch(name))
                errors.Add("İsim yalnızca harf, boşluk, kesme işareti ve tire içerebilir.");

            if (StartStage != AgeStage.Youth && StartStage != AgeStage.Adult)
                errors.Add("Başlangıç evresi Genç ya da Yetişkin olmalı.");

            string dyn = DynastyName == null ? "" : DynastyName.Trim();
            if (dyn.Length > 0 && (dyn.Length < MinNameLength || dyn.Length > MaxDynastyLength || !NamePattern.IsMatch(dyn)))
                errors.Add($"Hanedan adı {MinNameLength}-{MaxDynastyLength} harf olmalı (boş bırakılabilir).");
            if (BirthSeason < 0 || BirthSeason > 3) errors.Add("Doğum mevsimi geçersiz.");
            if (BirthDayOfSeason < 0) errors.Add("Doğum günü geçersiz.");

            errors.AddRange(catalog.Validate(Appearance, Sex));
            return errors;
        }

        /// <summary>Yeni oyunun kurucusu için ek denetim: kaza ayarı seçilmiş olmalı (karar 17.11.2026).</summary>
        public List<string> ValidateNewGame(AppearanceCatalog catalog)
        {
            var errors = Validate(catalog);
            if (Accidents == AccidentSetting.NotSet) errors.Add("Kaza ayarını seç: Kapalı, Seyrek ya da Normal.");
            return errors;
        }

        public double StartAge => StartStage == AgeStage.Youth ? YouthStartAge : AdultStartAge;

        /// <summary>Taslaktan karakter üretir. Geçersizse ArgumentException fırlatır (önce Validate).</summary>
        public Character CreateCharacter(int id, GridPos position, bool playerControlled, AppearanceCatalog catalog, Random rng)
        {
            var errors = Validate(catalog);
            if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

            var c = new Character(id, Name.Trim(), position, playerControlled)
            {
                Sex = Sex,
                Appearance = Appearance.Clone(),
                BirthSeason = BirthSeason,
                BirthDayOfSeason = BirthDayOfSeason,
                LikedFood = LikedFood ?? "", LikedColor = LikedColor ?? "", LikedHobby = LikedHobby ?? "", DislikedFood = DislikedFood ?? ""
            };
            c.Appearance.HeightPercent = 100;
            // Gizli genom: görünen hâl + rastgele çekinik taşıyıcılar (oyuncu görmez).
            c.Genome = Genetics.ForFounder(c.Appearance, catalog, rng);
            c.SetAge(StartAge);
            c.LifespanDays = c.AgeConfig.RollLifespan(rng);
            if (SkillFocus.HasValue) c.GainSkillXp(SkillFocus.Value, FocusXp);
            c.SetTraits(TraitIds);
            return c;
        }
    }
}
