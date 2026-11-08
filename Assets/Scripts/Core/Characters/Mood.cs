using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Characters
{
    public enum MoodLevel
    {
        Miserable = 0,  // Perişan
        Low = 1,        // Keyifsiz
        Okay = 2,       // Fena değil
        Good = 3,       // İyi
        Great = 4       // Harika
    }

    /// <summary>Geçici ruh hali etkisi (Sims'teki moodlet): "Rahat bir ev" +8, "Yas" -25 gibi.</summary>
    public sealed class Moodlet
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Value { get; set; }
        /// <summary>Kalan süre (oyun günü). Eksi = süresiz (kalıcı özellik etkisi gibi).</summary>
        public double RemainingDays { get; set; }
    }

    /// <summary>
    /// Ruh hali (Adım 32): temel 50 + özellik tabanı + ihtiyaç eksikleri + geçici etkiler (moodlet).
    /// Varsayılan (tam doygun, etkisiz) karakter tam 50 alır ve çarpanlar 1'dir; ruh hali yalnızca
    /// ihtiyaç ve olaylar gerçekten değiştirdiğinde iş hızını etkiler.
    /// </summary>
    public sealed class Mood
    {
        public const int Base = 50;
        private readonly List<Moodlet> _moodlets = new List<Moodlet>();

        public IReadOnlyList<Moodlet> Moodlets => _moodlets;

        /// <summary>Aynı kimlikli etki varsa yenilenir (değer ve süre üzerine yazılır).</summary>
        public void Add(string id, string text, int value, double days)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Moodlet kimliği boş.", nameof(id));
            foreach (var m in _moodlets)
            {
                if (m.Id != id) continue;
                m.Text = text; m.Value = value; m.RemainingDays = days;
                return;
            }
            _moodlets.Add(new Moodlet { Id = id, Text = text, Value = value, RemainingDays = days });
        }

        public bool Has(string id) { foreach (var m in _moodlets) if (m.Id == id) return true; return false; }

        public bool Remove(string id)
        {
            for (int i = 0; i < _moodlets.Count; i++)
                if (_moodlets[i].Id == id) { _moodlets.RemoveAt(i); return true; }
            return false;
        }

        public void Clear() => _moodlets.Clear();

        /// <summary>Süresi dolanları kaldırır.</summary>
        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            for (int i = _moodlets.Count - 1; i >= 0; i--)
            {
                var m = _moodlets[i];
                if (m.RemainingDays < 0) continue;
                m.RemainingDays -= gameDays;
                if (m.RemainingDays <= 0) _moodlets.RemoveAt(i);
            }
        }

        public int MoodletSum()
        {
            int sum = 0;
            foreach (var m in _moodlets) sum += m.Value;
            return sum;
        }

        /// <summary>
        /// İhtiyaç eksiklerinin ruh haline etkisi (hep sıfır ya da eksi): Açlık/Enerji/Isınma 50'nin altında
        /// her puan için 0,3 (en çok -15), Sosyal/Eğlence 40'ın altında her puan için 0,2 (en çok -8).
        /// </summary>
        public static double NeedsPenalty(NeedSet needs)
        {
            double sum = 0;
            foreach (NeedType n in Enum.GetValues(typeof(NeedType)))
            {
                float v = needs.Get(n);
                if (NeedKinds.IsSurvival(n)) { if (v < 50f) sum -= Math.Min(15.0, (50f - v) * 0.3); }
                else if (v < 40f) sum -= Math.Min(8.0, (40f - v) * 0.2);
            }
            return sum;
        }

        public int Score(NeedSet needs, int traitBase = 0)
        {
            double s = Base + traitBase + NeedsPenalty(needs) + MoodletSum();
            return (int)Math.Max(0, Math.Min(100, Math.Round(s)));
        }

        public static MoodLevel LevelFor(int score)
        {
            if (score < 25) return MoodLevel.Miserable;
            if (score < 40) return MoodLevel.Low;
            if (score <= 60) return MoodLevel.Okay;
            if (score <= 75) return MoodLevel.Good;
            return MoodLevel.Great;
        }

        /// <summary>İş hızı çarpanı: perişan 0,8 ... harika 1,1; "fena değil" tam 1.</summary>
        public static float WorkSpeedFor(MoodLevel level)
        {
            switch (level)
            {
                case MoodLevel.Miserable: return 0.8f;
                case MoodLevel.Low: return 0.9f;
                case MoodLevel.Good: return 1.05f;
                case MoodLevel.Great: return 1.1f;
                default: return 1f;
            }
        }

        public static string LevelName(MoodLevel level)
        {
            switch (level)
            {
                case MoodLevel.Miserable: return "Perişan";
                case MoodLevel.Low: return "Keyifsiz";
                case MoodLevel.Okay: return "Fena değil";
                case MoodLevel.Good: return "İyi";
                default: return "Harika";
            }
        }
    }
}
