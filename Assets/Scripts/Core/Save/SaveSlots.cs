using System;

namespace Thatchwick.Core.Save
{
    /// <summary>
    /// Kayıt yuvaları (karar 20.11.2026: üç yuva). Birinci yuva eski tek kaydın dosya adını kullanır; böylece
    /// yuvalardan önceki kayıt kendiliğinden 1. yuvada görünür.
    /// </summary>
    public static class SaveSlots
    {
        public const int Count = 3;

        public static int Clamp(int slot) => Math.Max(1, Math.Min(Count, slot));

        public static string FileName(int slot) => Clamp(slot) == 1 ? "thatchwick_save.json" : $"thatchwick_save_{Clamp(slot)}.json";

        /// <summary>Yuva özeti (başlık ekranındaki satır).</summary>
        public sealed class Info
        {
            public string Dynasty, Settlement, SavedAtUtc;
            public int Year, DayOfYear, Generation;
            public bool Ended;
        }

        /// <summary>Kayıttan özet çıkarır; kayıt yoksa (null) null.</summary>
        public static Info Describe(SaveData data, int daysPerYear = 40)
        {
            if (data == null) return null;
            int day = (int)Math.Floor(Math.Max(0, data.totalDays));
            int perYear = Math.Max(1, daysPerYear);
            return new Info
            {
                Dynasty = data.dynasty?.name,
                Settlement = data.dynasty?.settlement,
                SavedAtUtc = data.savedAtUtc,
                Year = day / perYear + 1,
                DayOfYear = day % perYear + 1,
                Generation = Math.Max(1, data.dynasty?.history?.Length ?? 1),
                Ended = data.dynasty != null && data.dynasty.ended
            };
        }
    }
}
