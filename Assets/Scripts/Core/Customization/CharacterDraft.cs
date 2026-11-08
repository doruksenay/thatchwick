using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Thatchwick.Core.Characters;
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

            errors.AddRange(catalog.Validate(Appearance, Sex));
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
                Appearance = Appearance.Clone()
            };
            c.SetAge(StartAge);
            c.LifespanDays = c.AgeConfig.RollLifespan(rng);
            if (SkillFocus.HasValue) c.GainSkillXp(SkillFocus.Value, FocusXp);
            return c;
        }
    }
}
