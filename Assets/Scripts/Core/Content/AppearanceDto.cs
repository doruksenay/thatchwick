using System;

namespace Thatchwick.Core.Content
{
    // appearance.json'ın birebir şekli (Unity JsonUtility için genel alanlar). Eksik dizi eski dosya demektir.

    [Serializable]
    public sealed class AppearanceFileDto
    {
        public OptionDto[] hairStyles;     // saç kütlesi (belge 43b); level = uzunluk
        public OptionDto[] bangs;          // saçın önü; level = gereken en az uzunluk
        public OptionDto[] hairExtras;     // örgü, topuz, at kuyruğu; level = gereken en az uzunluk
        public OptionDto[] hairOrnaments;  // saç süsü
        public HairPresetDto[] hairPresets;
        public LegacyHairDto[] legacyHair; // eski tam stil kimliği → modüller
        public OptionDto[] mustaches;
        public OptionDto[] beards;
        public OptionDto[] sideburns;
        public OptionDto[] facialHair;      // eski tek liste
        public OptionDto[] faceShapes;
        public OptionDto[] eyeShapes;
        public OptionDto[] brows;
        public OptionDto[] noses;
        public OptionDto[] mouths;
        public OptionDto[] ears;
        public OptionDto[] lashes;
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
        public string tag; // saç dokusu, takı bölgesi, ten detayı bölgeleri, göz/kaşta dünya deseni
        public int level;  // saç modülleri: kütlede uzunluk, diğerlerinde gereken en az uzunluk
        public string group; // ekrandaki alt grup
    }

    [Serializable]
    public sealed class HairPresetDto
    {
        public string id;
        public string displayName;
        public string group;
        public string mass;
        public string front;
        public string extra;
        public string sex;
    }

    [Serializable]
    public sealed class LegacyHairDto
    {
        public string id;
        public string mass;
        public string front;
        public string extra;
    }
}
