using System;

namespace Thatchwick.Core.Customization
{
    public enum BodyBuild
    {
        Slim = 0,    // İnce
        Average = 1, // Orta
        Sturdy = 2   // Gürbüz
    }

    /// <summary>
    /// Bir karakterin görünümü: her özellik için tür (kimlik) ve renk. Kimlikler
    /// AppearanceCatalog'daki seçeneklere karşılık gelir ("none" = yok). Katmanlı sprite ve
    /// palet değişimi bu verilerden çizilir.
    /// </summary>
    public sealed class Appearance
    {
        public const string None = "none";

        public BodyBuild Build { get; set; } = BodyBuild.Average;
        /// <summary>Boy yüzdesi (90-110). Sprite ölçeğini etkiler.</summary>
        public int HeightPercent { get; set; } = 100;

        public Rgb SkinColor { get; set; } = new Rgb(232, 195, 158);
        public Rgb EyeColor { get; set; } = new Rgb(90, 60, 40);

        public string HairStyleId { get; set; } = "short";
        public Rgb HairColor { get; set; } = new Rgb(90, 60, 35);

        public string FacialHairId { get; set; } = None;
        public Rgb FacialHairColor { get; set; } = new Rgb(90, 60, 35);

        public string FaceMarkId { get; set; } = None;

        public string OutfitId { get; set; } = "tunic";
        public Rgb OutfitColor { get; set; } = new Rgb(160, 110, 70);
        /// <summary>Kemer, kenar ve pantolon rengi.</summary>
        public Rgb TrimColor { get; set; } = new Rgb(70, 50, 35);

        public string HatId { get; set; } = None;
        public Rgb HatColor { get; set; } = new Rgb(120, 90, 60);

        public Appearance Clone() => (Appearance)MemberwiseClone();

        /// <summary>Önbellek anahtarı: aynı görünüm aynı metni verir.</summary>
        public string Key() =>
            $"{(int)Build}|{HeightPercent}|{SkinColor}|{EyeColor}|{HairStyleId}|{HairColor}|{FacialHairId}|{FacialHairColor}|{FaceMarkId}|{OutfitId}|{OutfitColor}|{TrimColor}|{HatId}|{HatColor}";
    }
}
