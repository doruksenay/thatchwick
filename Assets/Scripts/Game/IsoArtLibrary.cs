using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Assets/Art/Iso altındaki görsellerin listesi (ad → doku). Editör betiği
    /// (Thatchwick.Editor.IsoArtLibraryBuilder) bunu Resources/IsoArt/IsoArtLibrary.asset olarak kurar ve
    /// görseller değişince günceller; oyun çalışırken Resources'tan yüklenir. Sprite'lar pivotlarıyla
    /// birlikte IsoArt içinde kodla üretilir (pivot kuralları görsel yönergeden).
    /// </summary>
    public sealed class IsoArtLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string name;
            public Texture2D texture;
        }

        public List<Entry> entries = new List<Entry>();
    }
}
