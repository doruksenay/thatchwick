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
            /// <summary>Atlas dışı görselin kendi dokusu (katman parçaları, ikonlar, atlas kurulmadan önce eklenenler).</summary>
            public Texture2D texture;
            /// <summary>Atlasa alınmış görselin sayfası (o zaman <see cref="texture"/> boştur, özgün PNG yapıya girmez).</summary>
            public Texture2D atlas;
            /// <summary>Atlas sayfasındaki piksel dikdörtgeni (sol-alt köşe, Unity doku koordinatı).</summary>
            public Rect rect;
            /// <summary>Kaynak PNG'nin içe aktarma özeti: değişmediyse atlas yeri korunur.</summary>
            public string sourceHash;
        }

        public List<Entry> entries = new List<Entry>();
    }
}
