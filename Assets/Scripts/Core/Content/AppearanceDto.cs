using System;

namespace Thatchwick.Core.Content
{
    // appearance.json'ın birebir şekli (Unity JsonUtility için genel alanlar). Eksik dizi eski dosya demektir.

    [Serializable]
    public sealed class AppearanceFileDto
    {
        public OptionDto[] hairStyles;
        public OptionDto[] bangs;
        public OptionDto[] mustaches;
        public OptionDto[] beards;
        public OptionDto[] facialHair;      // eski tek liste
        public OptionDto[] faceShapes;
        public OptionDto[] eyeShapes;
        public OptionDto[] brows;
        public OptionDto[] noses;
        public OptionDto[] mouths;
        public OptionDto[] ears;
        public OptionDto[] faceMarks;       // eski tek yuva
        public OptionDto[] skinDetails;
        public OptionDto[] scarPatterns;
        public OptionDto[] scarZones;
        public OptionDto[] tattooPatterns;
        public OptionDto[] tattooZones;
        public OptionDto[] jewelry;
        public OptionDto[] makeup;
        public OptionDto[] outfits;
        public OptionDto[] bottoms;
        public OptionDto[] shoes;
        public OptionDto[] outers;
        public OptionDto[] hats;
        public OptionDto[] patterns;
        public OptionDto[] laughs;
        public string[] skinTones;
        public string[] hairColors;
        public string[] eyeColors;
        public string[] clothColors;
        public string[] inkColors;
        public string[] makeupColors;
        public string[] recessiveHairColors;
        public string[] recessiveEyeColors;
    }

    [Serializable]
    public sealed class OptionDto
    {
        public string id;
        public string displayName;
        public string sex; // boş = ikisi de, "Female" ya da "Male": yalnızca rastgele üretim tercihi
        public string tag; // saç dokusu, takı bölgesi, ten detayı bölgeleri
    }
}
