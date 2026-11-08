using System;

namespace Thatchwick.Core.Content
{
    // appearance.json'ın birebir şekli (Unity JsonUtility için genel alanlar).

    [Serializable]
    public sealed class AppearanceFileDto
    {
        public OptionDto[] hairStyles;
        public OptionDto[] facialHair;
        public OptionDto[] faceMarks;
        public OptionDto[] outfits;
        public OptionDto[] hats;
        public string[] skinTones;
        public string[] hairColors;
        public string[] eyeColors;
        public string[] clothColors;
    }

    [Serializable]
    public sealed class OptionDto
    {
        public string id;
        public string displayName;
        public string sex; // boş = ikisi de, "Female" ya da "Male"
    }
}
