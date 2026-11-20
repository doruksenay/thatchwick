using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Progress
{
    /// <summary>
    /// Masaüstü görüntü ayarlarının saf mantığı (çözünürlük listesi ve gezinme). Unity'ye dokunmaz; Game katmanı
    /// <c>Screen.resolutions</c> ile besler ve seçimi uygular.
    /// </summary>
    public static class DisplayOptions
    {
        /// <summary>Arayüz 540 birim yüksekliğe göre ölçeklenir; bunun altı okunmaz.</summary>
        public const int MinHeight = 540;
        public const int MinWidth = 960;

        /// <summary>
        /// Seçilebilir çözünürlükler: yinelenenler (aynı boyut, farklı tazeleme hızı) tekilleşir, çok küçükler atılır,
        /// küçükten büyüğe sıralanır. Hiçbiri kalmazsa geçerli boyut tek seçenek olur.
        /// </summary>
        public static List<(int width, int height)> Choices(IEnumerable<(int width, int height)> available, (int width, int height) current)
        {
            var seen = new HashSet<long>();
            var list = new List<(int width, int height)>();
            if (available != null)
                foreach (var r in available)
                {
                    if (r.width < MinWidth || r.height < MinHeight) continue;
                    if (seen.Add(((long)r.width << 32) | (uint)r.height)) list.Add(r);
                }
            if (list.Count == 0 && current.width > 0 && current.height > 0) list.Add(current);
            list.Sort((a, b) => a.width != b.width ? a.width.CompareTo(b.width) : a.height.CompareTo(b.height));
            return list;
        }

        /// <summary>Listede geçerli boyuta en yakın girdinin sırası (boş listede -1).</summary>
        public static int IndexOf(IReadOnlyList<(int width, int height)> choices, (int width, int height) current)
        {
            int best = -1;
            long bestDiff = long.MaxValue;
            for (int i = 0; i < choices.Count; i++)
            {
                long diff = Math.Abs((long)choices[i].width - current.width) * 4 + Math.Abs((long)choices[i].height - current.height);
                if (diff < bestDiff) { bestDiff = diff; best = i; }
            }
            return best;
        }

        /// <summary>Bir sonraki (+1) ya da önceki (-1) seçenek; uçlarda durur.</summary>
        public static (int width, int height) Step(IReadOnlyList<(int width, int height)> choices, (int width, int height) current, int direction)
        {
            if (choices == null || choices.Count == 0) return current;
            int i = IndexOf(choices, current) + Math.Sign(direction);
            if (i < 0) i = 0;
            if (i >= choices.Count) i = choices.Count - 1;
            return choices[i];
        }
    }
}
